using Acrya.ECSHandlers;
using Acrya.Renderer;



namespace Acrya.ECSComponents
{
    /// <summary>
    /// Component that causes the entity to be rendered in its position. <br/><br/>
    /// <b><u>REQURES:</u></b><br/>
    /// <see cref="EC_Entity"/>
    /// </summary>
    internal struct EC_Render : IEntityComponent
    {
        // <<Requirements>> //
        // EC_Entity //


        // <<Public Variables> //
        public double angle;
        public readonly IntPtr Image => imageName.Length == 0 ? nint.Zero : Renderer.Renderer.images[imageName];
        public string ImageName { readonly get => imageName; set => imageName = value; }
        private string imageName = "";
        public float width;
        public float height;

        public bool Active { readonly get => active; set => active = value; }
        private bool active = true;

        public static bool UsesGPU => false;

        // <<Private Variables>> //



        public EC_Render() { imageName = ""; }
        public EC_Render(string imageName, int width, int height)
        {
            this.imageName = imageName;
            this.width = width;
            this.height = height;
        }

        public readonly void Action(float dt, int uid)
        {
            ECSHandler.GetEntityComponent(uid, out EC_Entity Me);

            int gridX = Me.intX / Renderer.Renderer.drawGridTileSize;
            int gridY = Me.intY / Renderer.Renderer.drawGridTileSize;

            int gridImageSize = (int)MathF.Ceiling((width + 1) / (float)(2 * Renderer.Renderer.drawGridTileSize));
            for (int x = gridX - gridImageSize; x <= gridX + gridImageSize; x++)
            {
                for (int y = gridY - gridImageSize; y <= gridY + gridImageSize; y++)
                {
                    Renderer.Renderer.RequestDrawGrid(x, y);
                }
            }

            Renderer.Renderer.RequestEntityDraw(gridX, gridY, uid);
        }

        public readonly void Cleanup(int uid)
        {
            ECSHandler.GetEntityComponent(uid, out EC_Entity Me);

            int gridX = Me.intX / Renderer.Renderer.drawGridTileSize;
            int gridY = Me.intY / Renderer.Renderer.drawGridTileSize;

            lock (ECSHandler.updatedGrids)
            {
                Renderer.Renderer.RequestDrawGrid(gridX, gridY);
            }
        }
    }
}

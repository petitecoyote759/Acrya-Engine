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
        public readonly IntPtr Image => imageName.Length == 0 ? IntPtr.Zero : RendererTools.images[imageName];
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

            int gridX = Me.intX / RendererTools.drawGridTileSize;
            int gridY = Me.intY / RendererTools.drawGridTileSize;

            int gridImageSize = (int)MathF.Ceiling((width + 1) / (float)(2 * RendererTools.drawGridTileSize));
            for (int x = gridX - gridImageSize; x <= gridX + gridImageSize; x++)
            {
                for (int y = gridY - gridImageSize; y <= gridY + gridImageSize; y++)
                {
                    RendererTools.RequestDrawGrid(x, y);
                }
            }

            RendererTools.RequestEntityDraw(gridX, gridY, uid);
        }

        public readonly void Cleanup(int uid)
        {
            ECSHandler.GetEntityComponent(uid, out EC_Entity Me);

            int gridX = Me.intX / RendererTools.drawGridTileSize;
            int gridY = Me.intY / RendererTools.drawGridTileSize;

            lock (ECSHandler.updatedGrids)
            {
                RendererTools.RequestDrawGrid(gridX, gridY);
            }
        }
    }
}

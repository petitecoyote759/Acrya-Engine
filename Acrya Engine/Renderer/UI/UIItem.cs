using SDL2;
using static SDL2.SDL;



namespace Acrya.Renderer.UI
{
    public abstract class UIItem
    {
        public SDL_Rect targetRect = new SDL_Rect();
        public SDL_Rect srcRect = new SDL_Rect();
        public bool useSrcRect = false;
        internal IntPtr image = IntPtr.Zero;
        public bool showing = true;

        public UIItem()
        {
            Renderer.UIItems.Add(this);
        }
    }
}

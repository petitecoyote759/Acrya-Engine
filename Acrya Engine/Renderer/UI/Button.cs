using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace Acrya.Renderer.UI
{
    public class Button : UIItem
    {
        public TextField? buttonText = null;

        public bool HoveredOver { get; internal set; }

        public IntPtr? highlightImage;

        // <<Constructors>> //

        // <<<Overloaded Constructors>>> //
        public Button(SDL_Rect position, string imageName, TextField? text = null, string? highlightImageName = null) : 
            this(position, Renderer.images[imageName], text, highlightImageName is null ? null : Renderer.images[highlightImageName]) { }
        public Button(SDL_Rect position, string imageName, TextField? text = null, IntPtr? highlightImage = null) : 
            this(position, Renderer.images[imageName], text, highlightImage) { }
        public Button(SDL_Rect position, IntPtr image, TextField? text = null, string? highlightImageName = null) : 
            this(position, image, text, highlightImageName is null ? null : Renderer.images[highlightImageName]) { }

        // All constructors funnel to this one.
        public Button(SDL_Rect position, IntPtr image, TextField? text = null, IntPtr? highlightImage = null)
        {
            this.image = image;
            this.buttonText = text;
            this.useSrcRect = false;
            this.highlightImage = highlightImage;
            this.targetRect = position;
        }
    }
}

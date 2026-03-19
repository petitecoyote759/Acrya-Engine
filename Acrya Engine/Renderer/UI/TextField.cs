using SDL2;
using static SDL2.SDL;

namespace Acrya.Renderer.UI
{
    public class TextField : UIItem
	{
        public string Text { get => text; set { UpdateText(value); } }
        private string text = "";

        public string Font { get => font; set { font = value; UpdateText(text); } }
        private string font = "Aller_Bd";

        public (byte, byte, byte, byte)? backgroundColour = null;

        public int posX = 0;
        public int posY = 0;
        public int widthPerChar = 20;
        public int charHeight = 30;
        

        public TextField(int x, int y, int widthPerChar, int charHeight, string text, (byte, byte, byte, byte)? backgroundColour = null)
        {
            this.posX = x; this.posY = y;
            this.widthPerChar = widthPerChar;
            this.charHeight = charHeight;
            this.backgroundColour = backgroundColour;

            UpdateText(text);
        }



        private void UpdateText(string value)
        {
            string oldText = text;
            this.text = value;

            image = Renderer.GetTextImage(text, preserve: true);

            targetRect.x = posX; targetRect.y = posY;
            targetRect.w = widthPerChar * text.Length; targetRect.h = charHeight;
        }
    }
}

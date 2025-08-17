using System.Drawing;

namespace ConsoleGame.Renderer
{
    public struct Chexel
    {
        public char Char;
        public Color ForegroundColor;
        public Color BackgroundColor;

        public Chexel(char ch, Color fgColor, Color bgColor)
        {
            Char = ch;
            ForegroundColor = fgColor;
            BackgroundColor = bgColor;
        }
    }
}
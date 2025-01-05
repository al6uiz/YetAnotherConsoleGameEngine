namespace ConsoleGame.Renderer
{
    public struct Chexel
    {
        public char Char;
        public ConsoleColor ForegroundColor;
        public ConsoleColor BackgroundColor;

        public Chexel(char ch, ConsoleColor fgColor, ConsoleColor bgColor)
        {
            Char = ch;
            ForegroundColor = fgColor;
            BackgroundColor = bgColor;
        }
    }
}
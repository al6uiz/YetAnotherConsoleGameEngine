namespace ConsoleGame.Renderer
{
    public class Framebuffer
    {
        public int Width;
        public int Height;

        public int ViewportX;
        public int ViewportY;

        private Chexel[,] chexels;

        public Framebuffer(int width, int height, int viewportX = 0, int viewportY = 0)
        {
            Width = width;
            Height = height;
            ViewportX = viewportX;
            ViewportY = viewportY;
            chexels = new Chexel[width, height];

            // Initialize chexels with default values (spaces with default console colors)
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    chexels[x, y] = new Chexel(' ', ConsoleColor.Black, ConsoleColor.White);
        }

        public Chexel GetChexel(int x, int y)
        {
            return chexels[x, y];
        }

        public void SetChexel(int x, int y, Chexel chexel)
        {
            chexels[x, y] = chexel;
        }

        // Method to clear the framebuffer
        public void Clear()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    chexels[x, y] = new Chexel(' ', ConsoleColor.Black, ConsoleColor.White);
        }
    }
}
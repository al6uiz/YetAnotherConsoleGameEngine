using Spectre.Console;
using Spectre.Console.Rendering;

using Color = System.Drawing.Color;

namespace ConsoleGame.Renderer
{
    public class TerminalRenderer
    {
        private List<Framebuffer> frameBuffers;
        public int consoleWidth;
        public int consoleHeight;
        private char[] lineBuffer; // Pre-allocated buffer for rendering lines

        public TerminalRenderer(int width, int height)
        {
            frameBuffers = new List<Framebuffer>();
            consoleWidth = width;
            consoleHeight = height;

            Console.CursorVisible = false;

            // Allocate the line buffer once
            lineBuffer = new char[consoleWidth];
        }

        public void AddFrameBuffer(Framebuffer fb)
        {
            frameBuffers.Add(fb);
        }

        public void RemoveFrameBuffer(Framebuffer fb)
        {
            frameBuffers.Remove(fb);
        }

        private Chexel GetChexelForPoint(int screenX, int screenY)
        {
            for (int i = frameBuffers.Count - 1; i >= 0; i--)
            {
                Framebuffer fb = frameBuffers[i];
                int fbX = screenX - fb.ViewportX;
                int fbY = screenY - fb.ViewportY;

                if (fbX >= 0 && fbX < fb.Width && fbY >= 0 && fbY < fb.Height)
                {
                    Chexel chexel = fb.chexels[fbX, fbY];
                    if (chexel.Char != ' ')
                    {
                        return chexel;
                    }
                }
            }

            return new Chexel(' ', Color.Black, Color.Black);
        }

        public void Render()
        {
            Console.SetCursorPosition(0, 0);

            for (int y = 0; y < consoleHeight; y++)
            {
                AnsiConsole.Write(GetLines(y));
                AnsiConsole.WriteLine();
            }
        }

        private IRenderable GetLines(int y)
        {
            return new SegmentLine(frameBuffers[1], y);
        }
    }
}
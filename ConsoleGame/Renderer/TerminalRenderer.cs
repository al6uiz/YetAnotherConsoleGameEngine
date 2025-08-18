using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleGame.Renderer
{
    public class TerminalRenderer
    {
        private List<Framebuffer> frameBuffers;
        private SegmentLineRenderer segmentRenderer;
        public int consoleWidth;
        public int consoleHeight;

        public TerminalRenderer(int width, int height)
        {
            frameBuffers = new List<Framebuffer>();
            consoleWidth = width;
            consoleHeight = height;

            Console.CursorVisible = false;
        }

        public void AddFrameBuffer(Framebuffer fb)
        {
            frameBuffers.Add(fb);
        }

        public void RemoveFrameBuffer(Framebuffer fb)
        {
            frameBuffers.Remove(fb);
        }

        public void Render()
        {
            segmentRenderer ??= new SegmentLineRenderer(frameBuffers[1]);

            Console.SetCursorPosition(0, 0);

            for (int y = 0; y < consoleHeight; y++)
            {
                segmentRenderer.CurrentLine = y;
                AnsiConsole.Write(segmentRenderer);
                AnsiConsole.WriteLine();
            }
        } 
    }
}
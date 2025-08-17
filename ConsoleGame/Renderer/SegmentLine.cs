using Spectre.Console;
using Spectre.Console.Rendering;

using Color = System.Drawing.Color;

namespace ConsoleGame.Renderer
{
    public class SegmentLine(Framebuffer buffer, int y) : IRenderable
    {
        public Measurement Measure(RenderOptions options, int maxWidth)
        {
            return new Measurement(maxWidth, maxWidth);
        }

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            for (int i = 0; i < buffer.Width; i++)
            {
                yield return new Segment(
                    buffer.chexels[i, y].Char == '▀' ? "▀" : " ",
                    new Style(Convert(buffer.chexels[i, y].ForegroundColor), Convert(buffer.chexels[i, y].BackgroundColor)));
            }
        }

        private Spectre.Console.Color? Convert(Color color)
        {
            return new Spectre.Console.Color(color.R, color.G, color.B);
        }
    }
}
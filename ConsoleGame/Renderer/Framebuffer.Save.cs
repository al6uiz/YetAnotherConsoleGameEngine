using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace ConsoleGame.Renderer
{
    public partial class Framebuffer
    {
        // Export the framebuffer to a PNG image; each Chexel becomes a cell with background fill and a drawn glyph in the foreground color.
        public void ToPng(string filePath, int cellWidth = 8, int cellHeight = 16, string fontFamilyName = "Consolas", float fontSize = 12f, bool antialias = true)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("filePath must be non-empty.", nameof(filePath));
            if (cellWidth <= 0) throw new ArgumentOutOfRangeException(nameof(cellWidth), "cellWidth must be > 0.");
            if (cellHeight <= 0) throw new ArgumentOutOfRangeException(nameof(cellHeight), "cellHeight must be > 0.");

            int bmpWidth = Width * cellWidth;
            int bmpHeight = Height * cellHeight;

            using (Bitmap bmp = new Bitmap(bmpWidth, bmpHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Black);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.TextRenderingHint = antialias ? TextRenderingHint.AntiAliasGridFit : TextRenderingHint.SingleBitPerPixelGridFit;
                    using (Font font = new Font(fontFamilyName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                    {
                        StringFormat sf = StringFormat.GenericDefault;
                        sf.Alignment = StringAlignment.Near;
                        sf.LineAlignment = StringAlignment.Near;

                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                Chexel chx = chexels[x, y];
                                Rectangle cellRect = new Rectangle(x * cellWidth, y * cellHeight, cellWidth, cellHeight);

                                using (Brush bg = new SolidBrush(chx.BackgroundColor))
                                {
                                    g.FillRectangle(bg, cellRect);
                                }

                                if (chx.Char != ' ')
                                {
                                    using (Brush fg = new SolidBrush(chx.ForegroundColor))
                                    {
                                        g.DrawString(chx.Char.ToString(), font, fg, cellRect.Location, sf);
                                    }
                                }
                            }
                        }
                    }
                }

                bmp.Save(filePath, ImageFormat.Png);
            }
        }
    }
}

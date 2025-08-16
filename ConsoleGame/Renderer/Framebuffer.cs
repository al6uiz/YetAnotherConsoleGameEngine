using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

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

                                using (Brush bg = new SolidBrush(ConsoleColorConverter.ToColor(chx.BackgroundColor)))
                                {
                                    g.FillRectangle(bg, cellRect);
                                }

                                if (chx.Char != ' ')
                                {
                                    using (Brush fg = new SolidBrush(ConsoleColorConverter.ToColor(chx.ForegroundColor)))
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

    // ConsoleColor -> System.Drawing.Color map (classic 16-color palette).
    internal static class ConsoleColorConverter
    {
        public static Color ToColor(ConsoleColor consoleColor)
        {
            switch (consoleColor)
            {
                case ConsoleColor.Black: return Color.FromArgb(0, 0, 0);
                case ConsoleColor.DarkBlue: return Color.FromArgb(0, 0, 128);
                case ConsoleColor.DarkGreen: return Color.FromArgb(0, 128, 0);
                case ConsoleColor.DarkCyan: return Color.FromArgb(0, 128, 128);
                case ConsoleColor.DarkRed: return Color.FromArgb(128, 0, 0);
                case ConsoleColor.DarkMagenta: return Color.FromArgb(128, 0, 128);
                case ConsoleColor.DarkYellow: return Color.FromArgb(128, 128, 0);
                case ConsoleColor.Gray: return Color.FromArgb(192, 192, 192);
                case ConsoleColor.DarkGray: return Color.FromArgb(128, 128, 128);
                case ConsoleColor.Blue: return Color.FromArgb(0, 0, 255);
                case ConsoleColor.Green: return Color.FromArgb(0, 255, 0);
                case ConsoleColor.Cyan: return Color.FromArgb(0, 255, 255);
                case ConsoleColor.Red: return Color.FromArgb(255, 0, 0);
                case ConsoleColor.Magenta: return Color.FromArgb(255, 0, 255);
                case ConsoleColor.Yellow: return Color.FromArgb(255, 255, 0);
                case ConsoleColor.White: return Color.FromArgb(255, 255, 255);
                default: return Color.White;
            }
        }
    }
}

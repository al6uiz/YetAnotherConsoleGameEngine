using System;
using System.Collections.Generic;

namespace ConsoleGame.Renderer
{
    public class TerminalRenderer
    {
        private List<Framebuffer> frameBuffers;
        private int consoleWidth;
        private int consoleHeight;
        private ConsoleColor currentFg;
        private ConsoleColor currentBg;
        private char[] lineBuffer; // Pre-allocated buffer for rendering lines

        public TerminalRenderer()
        {
            frameBuffers = new List<Framebuffer>();
            consoleWidth = Console.WindowWidth;
            consoleHeight = Console.WindowHeight - 1;

            Console.CursorVisible = false;

            currentFg = Console.ForegroundColor;
            currentBg = Console.BackgroundColor;

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
            // Iterate through framebuffers in reverse order (last added has priority)
            for (int i = frameBuffers.Count - 1; i >= 0; i--)
            {
                Framebuffer fb = frameBuffers[i];
                int fbX = screenX - fb.ViewportX;
                int fbY = screenY - fb.ViewportY;

                if (fbX >= 0 && fbX < fb.Width && fbY >= 0 && fbY < fb.Height)
                {
                    Chexel chexel = fb.GetChexel(fbX, fbY);

                    // If the chexel is not transparent or empty, return it
                    if (chexel.Char != ' ')
                    {
                        return chexel;
                    }
                }
            }

            // Default to a space with default colors if no framebuffer covers this point
            return new Chexel(' ', currentFg, currentBg);
        }

        public void Render()
        {
            Console.SetCursorPosition(0, 0);

            for (int y = 0; y < consoleHeight; y++)
            {
                ConsoleColor? lineFg = null;
                ConsoleColor? lineBg = null;

                for (int x = 0; x < consoleWidth; x++)
                {
                    Chexel chexel = GetChexelForPoint(x, y);

                    if (lineFg == null || chexel.ForegroundColor != lineFg || chexel.BackgroundColor != lineBg)
                    {
                        // Colors changed, write the current line segment
                        if (lineFg != null)
                        {
                            Console.Write(lineBuffer, 0, x);
                        }

                        // Update colors if necessary
                        if (currentFg != chexel.ForegroundColor)
                        {
                            Console.ForegroundColor = chexel.ForegroundColor;
                            currentFg = chexel.ForegroundColor;
                        }
                        if (currentBg != chexel.BackgroundColor)
                        {
                            Console.BackgroundColor = chexel.BackgroundColor;
                            currentBg = chexel.BackgroundColor;
                        }

                        // Start new line segment
                        lineFg = chexel.ForegroundColor;
                        lineBg = chexel.BackgroundColor;
                    }

                    // Store the character in the buffer
                    lineBuffer[x] = chexel.Char;
                }

                // Write the entire line
                Console.Write(lineBuffer, 0, consoleWidth);
            }

            // Reset console colors to defaults (optional)
            Console.ResetColor();
        }
    }
}

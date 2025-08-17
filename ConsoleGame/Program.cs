using ConsoleGame.Entities;
using ConsoleGame.Renderer;

namespace ConsoleRayTracing
{
    public static class Program
    {
        public static Terminal terminal;

        private static void Main(string[] args)
        {
            Console.CursorVisible = false;

            int cellsW = Console.WindowWidth;
            int cellsH = Console.WindowHeight - 1;

            terminal = new Terminal(cellsW, cellsH);

            int superSample = 3;
            if (args != null && args.Length > 0)
            {
                int parsed;
                if (int.TryParse(args[0], out parsed) && parsed > 0)
                {
                    superSample = parsed;
                }
            }

            int pxW = cellsW * superSample;
            int pxH = cellsH * superSample;

            Framebuffer rayFb = new Framebuffer(cellsW, cellsH, 0, 0);
            terminal.AddFrameBuffer(rayFb);

            BaseEntity rt = new BaseEntity(0, 0, new Chexel());
            rt.AddComponent(new RaytraceEntity(rt, rayFb, pxW, pxH, superSample));
            terminal.AddEntity(rt);

            terminal.Start();

            Console.ResetColor();
            Console.CursorVisible = true;
        }
    }
}
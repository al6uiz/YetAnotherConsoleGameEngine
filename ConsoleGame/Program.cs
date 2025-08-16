using ConsoleGame.Renderer;
using ConsoleGame.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ConsoleGame.Components;

namespace ConsoleRayTracing
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Console.CursorVisible = false;

            Terminal terminal = new Terminal();

            int cellsW = Console.WindowWidth - 1;
            int cellsH = Console.WindowHeight - 1;

            int superSample = 1;
            if (args != null && args.Length > 0)
            {
                int parsed;
                if (int.TryParse(args[0], out parsed) && parsed > 0)
                {
                    superSample = parsed;
                }
            }

            int pxW = cellsW * superSample;
            int pxH = cellsH * 2 * superSample;

            Framebuffer rayFb = new Framebuffer(cellsW, cellsH, 0, 0);
            terminal.AddFrameBuffer(rayFb);

            //Scene scene = Scenes.BuildTestScene();
            //Scene scene = Scenes.BuildDemoScene();
            //Scene scene = Scenes.BuildCornellBox();
            //Scene scene = Scenes.BuildMirrorSpheresOnChecker();
            //Scene scene = Scenes.BuildCylindersDisksAndTriangles();
            //Scene scene = Scenes.BuildBoxesShowcase();
            //Scene scene = Scenes.BuildVolumeGridTestScene();
            //Scene scene = MeshScenes.BuildAllMeshesScene();
            Scene scene = MeshScenes.BuildBunnyScene();
            //Scene scene = MeshScenes.BuildTeapotScene();
            //Scene scene = MeshScenes.BuildCowScene();
            //Scene scene = MeshScenes.BuildDragonScene();
            //Func<int, int, Material> materialLookup = (id, meta) =>
            //{
            //    switch (id)
            //    {
            //        case 1: // stone
            //            return new Material(new Vec3(0.55, 0.55, 0.55), 0.0, 0.0, Vec3.Zero);
            //        case 2: // dirt
            //            return new Material(new Vec3(0.40, 0.25, 0.15), 0.0, 0.0, Vec3.Zero);
            //        case 3: // grass
            //            return new Material(new Vec3(0.15, 0.75, 0.20), 0.1, 0.0, Vec3.Zero);
            //        case 4: // water
            //            return new Material(new Vec3(0.15, 0.35, 0.95), 0.0, 0.02, Vec3.Zero);
            //        case 5: // sand / ore placeholder
            //            return new Material(new Vec3(0.9, 0.85, 0.55), 0.0, 0.0, Vec3.Zero);
            //        default: // fallback
            //            return new Material(new Vec3(0.7, 0.7, 0.7), 0.0, 0.0, Vec3.Zero);
            //    }
            //};

            //Scene scene = VolumeScenes.BuildMinecraftLike("test.bin", new Vec3(-100, -100, -100), new Vec3(2, 2, 2), materialLookup, 8, 8, 8, 32);

            float fovDeg = 60.0f;
            BaseEntity rt = new BaseEntity(0, 0, new Chexel());
            rt.AddComponent(new RaytraceEntity(rt, rayFb, scene, fovDeg, pxW, pxH, superSample));
            terminal.AddEntity(rt);

            terminal.Start();

            Console.ResetColor();
            Console.CursorVisible = true;
        }

       
    }
}

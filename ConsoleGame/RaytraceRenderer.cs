// File: RaytraceRenderer.cs
using ConsoleGame.Renderer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRayTracing
{
    public sealed class RaytraceRenderer
    {
        private enum DebugView { None = 0, TraversalHeat = 1, DepthFalseColor = 2, LeafId = 3 }

        private readonly Scene scene;
        private readonly float fovDeg;
        private readonly int hiW;
        private readonly int hiH;
        private readonly int ss;

        private readonly int fbW;
        private readonly int fbH;

        private readonly Chexel[][,] buffers = new Chexel[2][,];
        private int frontIndex = 0;
        private int backIndex = 1;

        private readonly AutoResetEvent evtFrameReady = new AutoResetEvent(false);
        private readonly AutoResetEvent evtFlipDone = new AutoResetEvent(false);
        private volatile bool runRenderLoop = true;
        private Thread renderThread;
        private long frameCounter = 0;

        private readonly object camLock = new object();
        private Vec3 camPos = new Vec3(0.0, 1.0, 0.0);
        private float yaw = 0.0f;
        private float pitch = 0.0f;

        private volatile DebugView debugView = DebugView.None;
        private volatile float debugScale = 0.08f;

        private const int DiffuseBounces = 1;
        private const int IndirectSamples = 1;
        private const int MaxMirrorBounces = 2;
        private const float MirrorThreshold = 0.9f;
        private const float Eps = 1e-4f;

        public RaytraceRenderer(Framebuffer framebuffer, Scene scene, float fovDeg, int pxW, int pxH, int superSample)
        {
            this.scene = scene;
            this.fovDeg = fovDeg;
            this.hiW = pxW;
            this.hiH = pxH;
            this.ss = Math.Max(1, superSample);

            this.fbW = framebuffer.Width;
            this.fbH = framebuffer.Height;

            buffers[0] = new Chexel[fbW, fbH];
            buffers[1] = new Chexel[fbW, fbH];

            scene.RebuildBVH();

            renderThread = new Thread(RenderLoop);
            renderThread.IsBackground = true;
            renderThread.Name = "Raytrace-RenderLoop";
            renderThread.Start();
        }

        public void SetCamera(Vec3 pos, float yaw, float pitch)
        {
            lock (camLock)
            {
                this.camPos = pos;
                this.yaw = yaw;
                this.pitch = pitch;
            }
        }

        public void SetDebugView(int mode)
        {
            if (mode < 0) mode = 0;
            if (mode > 3) mode = 3;
            debugView = (DebugView)mode;
        }

        public void SetDebugScale(float scale)
        {
            if (scale <= 0.0f) scale = 0.0001f;
            debugScale = scale;
        }

        public void TryFlipAndBlit(Framebuffer fb)
        {
            if (evtFrameReady.WaitOne(0))
            {
                int newFront = backIndex;
                int newBack = frontIndex;
                frontIndex = newFront;
                backIndex = newBack;
                evtFlipDone.Set();
            }

            Chexel[,] front = buffers[frontIndex];
            for (int cy = 0; cy < fbH; cy++)
            {
                for (int cx = 0; cx < fbW; cx++)
                {
                    fb.SetChexel(cx, cy, front[cx, cy]);
                }
            }
        }

        private void RenderLoop()
        {
            try
            {
                while (runRenderLoop)
                {
                    float aspect = (float)hiW / (float)hiH;

                    Vec3 camPosSnapshot;
                    float yawSnapshot;
                    float pitchSnapshot;
                    lock (camLock)
                    {
                        camPosSnapshot = camPos;
                        yawSnapshot = yaw;
                        pitchSnapshot = pitch;
                    }

                    DebugView dvSnapshot = debugView;
                    float dbgScaleSnapshot = debugScale;

                    Camera cam = BuildCamera(camPosSnapshot, yawSnapshot, pitchSnapshot, fovDeg, aspect);

                    int procCount = Math.Max(1, Environment.ProcessorCount);
                    long frame = Interlocked.Increment(ref frameCounter);

                    Chexel[,] target = buffers[backIndex];

                    Parallel.For(0, procCount, worker =>
                    {
                        int yStart = (worker * fbH) / procCount;
                        int yEnd = ((worker + 1) * fbH) / procCount;
                        Rng rng = new Rng(unchecked(((ulong)Environment.TickCount << 32) ^ ((ulong)worker * 0x9E3779B97F4A7C15UL) ^ (ulong)frame));

                        for (int cy = yStart; cy < yEnd; cy++)
                        {
                            int yTopPx0 = cy * 2 * ss;
                            int yBotPx0 = (cy * 2 + 1) * ss;

                            for (int cx = 0; cx < fbW; cx++)
                            {
                                int xPx0 = cx * ss;

                                Vec3 topSum = Vec3.Zero;
                                Vec3 botSum = Vec3.Zero;

                                for (int syi = 0; syi < ss; syi++)
                                {
                                    int yTopPx = yTopPx0 + syi;
                                    int yBotPx = yBotPx0 + syi;

                                    for (int sxi = 0; sxi < ss; sxi++)
                                    {
                                        int xPx = xPx0 + sxi;

                                        Ray rTop = cam.MakeRay(xPx, yTopPx, hiW, hiH);
                                        Vec3 cTop = Trace(scene, rTop, 0, ref rng, dvSnapshot, dbgScaleSnapshot);
                                        topSum = topSum + cTop;

                                        Ray rBot = cam.MakeRay(xPx, yBotPx, hiW, hiH);
                                        Vec3 cBot = Trace(scene, rBot, 0, ref rng, dvSnapshot, dbgScaleSnapshot);
                                        botSum = botSum + cBot;
                                    }
                                }

                                float inv = 1.0f / (float)(ss * ss);
                                Vec3 topAvg = new Vec3(topSum.X * inv, topSum.Y * inv, topSum.Z * inv);
                                Vec3 botAvg = new Vec3(botSum.X * inv, botSum.Y * inv, botSum.Z * inv);

                                Vec3 topTM = ToneMapReinhard(topAvg);
                                Vec3 botTM = ToneMapReinhard(botAvg);
                                Vec3 topSRGB = Gamma(topTM, 1.0f / 2.2f).Saturate();
                                Vec3 botSRGB = Gamma(botTM, 1.0f / 2.2f).Saturate();

                                ConsoleColor fg = ConsolePalette.NearestColor(topSRGB);
                                ConsoleColor bg = ConsolePalette.NearestColor(botSRGB);
                                target[cx, cy] = new Chexel('▀', fg, bg);
                            }
                        }
                    });

                    evtFrameReady.Set();
                    evtFlipDone.WaitOne();
                }
            }
            catch
            {
            }
        }

        private static Camera BuildCamera(Vec3 pos, float yaw, float pitch, float fovDeg, float aspect)
        {
            Vec3 fwd = ForwardFromYawPitch(yaw, pitch);
            return new Camera(pos, pos + fwd, new Vec3(0.0, 1.0, 0.0), fovDeg, aspect);
        }

        private static Vec3 ForwardFromYawPitch(float yaw, float pitch)
        {
            float cp = MathF.Cos(pitch);
            return new Vec3(MathF.Sin(yaw) * cp, MathF.Sin(pitch), -MathF.Cos(yaw) * cp);
        }

        private static Vec3 Trace(Scene scene, Ray r, int depth, ref Rng rng, DebugView dv, float dbgScale)
        {
            HitRecord rec = default;
            if (!scene.Hit(r, 0.001f, float.MaxValue, ref rec))
            {
                if (dv != DebugView.None)
                {
                    return new Vec3(0.0f, 0.0f, 0.0f);
                }
                float tbg = 0.5f * (r.Dir.Y + 1.0f);
                return Lerp(scene.BackgroundBottom, scene.BackgroundTop, tbg);
            }

            if (dv != DebugView.None)
            {
                return DebugColor(rec, dv, dbgScale);
            }

            Vec3 color = rec.Mat.Emission;

            if ((float)rec.Mat.Reflectivity >= MirrorThreshold)
            {
                if (depth >= MaxMirrorBounces)
                {
                    return color;
                }
                Vec3 reflDir = Reflect(r.Dir, rec.N).Normalized();
                Ray reflRay = new Ray(rec.P + rec.N * Eps, reflDir);
                Vec3 reflCol = Trace(scene, reflRay, depth + 1, ref rng, dv, dbgScale);
                color += new Vec3(reflCol.X * (float)rec.Mat.Albedo.X, reflCol.Y * (float)rec.Mat.Albedo.Y, reflCol.Z * (float)rec.Mat.Albedo.Z);
                return color;
            }

            for (int i = 0; i < scene.Lights.Count; i++)
            {
                PointLight light = scene.Lights[i];
                Vec3 toL = light.Position - rec.P;
                float dist2 = toL.Dot(toL);
                float dist = MathF.Sqrt(dist2);
                Vec3 ldir = toL / dist;

                float nDotL = MathF.Max(0.0f, rec.N.Dot(ldir));
                if (nDotL <= 0.0)
                {
                    continue;
                }

                Ray shadow = new Ray(rec.P + rec.N * Eps, ldir);
                if (scene.Occluded(shadow, dist - Eps))
                {
                    continue;
                }

                float atten = light.Intensity / dist2;
                Vec3 lambert = rec.Mat.Albedo * (nDotL * atten) * light.Color;
                color += lambert;
            }

            if (depth < DiffuseBounces)
            {
                Vec3 indirect = Vec3.Zero;
                for (int s = 0; s < IndirectSamples; s++)
                {
                    Vec3 bounceDir = CosineSampleHemisphere(rec.N, ref rng);
                    Ray bounce = new Ray(rec.P + rec.N * Eps, bounceDir);
                    Vec3 Li = Trace(scene, bounce, depth + 1, ref rng, dv, dbgScale);
                    indirect += new Vec3(Li.X * (float)rec.Mat.Albedo.X, Li.Y * (float)rec.Mat.Albedo.Y, Li.Z * (float)rec.Mat.Albedo.Z);
                }
                float invSpp = 1.0f / (float)IndirectSamples;
                color += new Vec3(indirect.X * invSpp, indirect.Y * invSpp, indirect.Z * invSpp);
            }

            return color;
        }

        private static Vec3 DebugColor(HitRecord rec, DebugView dv, float scale)
        {
            if (rec.DebugWasBVH == 0)
            {
                return new Vec3(0.0f, 0.0f, 0.0f);
            }
            if (dv == DebugView.TraversalHeat)
            {
                float t = 1.0f - MathF.Exp(-MathF.Max(0.0f, rec.DebugAabbTests) * MathF.Max(0.00001f, scale));
                float r = t;
                float g = t * 0.55f;
                float b = 1.0f - t * 0.85f;
                return new Vec3(r, g, b);
            }
            if (dv == DebugView.DepthFalseColor)
            {
                int d = rec.DebugHitDepth;
                float h = (d % 64) / 64.0f;
                return HsvToRgb(h, 0.9f, 1.0f);
            }
            if (dv == DebugView.LeafId)
            {
                return HashToRgb(rec.DebugLeafId);
            }
            return new Vec3(0.0f, 0.0f, 0.0f);
        }

        private static Vec3 CosineSampleHemisphere(Vec3 n, ref Rng rng)
        {
            float u1 = rng.NextUnit();
            float u2 = rng.NextUnit();
            float r = MathF.Sqrt(u1);
            float theta = 2.0f * MathF.PI * u2;
            float x = r * MathF.Cos(theta);
            float y = r * MathF.Sin(theta);
            float z = MathF.Sqrt(MathF.Max(0.0f, 1.0f - u1));

            Vec3 w = n;
            Vec3 a = MathF.Abs(w.X) > 0.1 ? new Vec3(0.0, 1.0, 0.0) : new Vec3(1.0, 0.0, 0.0);
            Vec3 v = w.Cross(a).Normalized();
            Vec3 u = v.Cross(w);

            Vec3 dir = (u * x + v * y + w * z).Normalized();
            return dir;
        }

        private static Vec3 ToneMapReinhard(Vec3 c)
        {
            return new Vec3(c.X / (1.0 + c.X), c.Y / (1.0 + c.Y), c.Z / (1.0 + c.Z));
        }

        private static Vec3 Reflect(Vec3 v, Vec3 n)
        {
            return v - n * (2.0f * v.Dot(n));
        }

        private static Vec3 Lerp(Vec3 a, Vec3 b, float t)
        {
            return a * (1.0f - t) + b * t;
        }

        private static Vec3 Gamma(Vec3 c, float invGamma)
        {
            return new Vec3(MathF.Pow(c.X, invGamma), MathF.Pow(c.Y, invGamma), MathF.Pow(c.Z, invGamma));
        }

        private static Vec3 HsvToRgb(float h, float s, float v)
        {
            float c = v * s;
            float hh = (h - MathF.Floor(h)) * 6.0f;
            float x = c * (1.0f - MathF.Abs((hh % 2.0f) - 1.0f));
            float r = 0.0f, g = 0.0f, b = 0.0f;
            if (hh < 1.0f) { r = c; g = x; b = 0.0f; }
            else if (hh < 2.0f) { r = x; g = c; b = 0.0f; }
            else if (hh < 3.0f) { r = 0.0f; g = c; b = x; }
            else if (hh < 4.0f) { r = 0.0f; g = x; b = c; }
            else if (hh < 5.0f) { r = x; g = 0.0f; b = c; }
            else { r = c; g = 0.0f; b = x; }
            float m = v - c;
            return new Vec3(r + m, g + m, b + m);
        }

        private static Vec3 HashToRgb(int id)
        {
            uint x = (uint)id;
            x ^= x >> 17;
            x *= 0xED5AD4BBu;
            x ^= x >> 11;
            x *= 0xAC4C1B51u;
            x ^= x >> 15;
            x *= 0x31848BABu;
            x ^= x >> 14;
            float r = (x & 0xFF) / 255.0f;
            float g = ((x >> 8) & 0xFF) / 255.0f;
            float b = ((x >> 16) & 0xFF) / 255.0f;
            return new Vec3(r, g, b);
        }
    }
}

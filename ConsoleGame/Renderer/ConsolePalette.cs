namespace ConsoleRayTracing
{
    // File: MeshSwatches.cs
    // Curated sRGB swatches that intentionally "snap" to the 16-color console palette,
    // with gentle brightness trims to avoid clipping after lighting + tone mapping.
    // Use these Vec3 values for Material.Albedo in your mesh scenes.
    //
    // How to use (examples):
    //   var gold = MeshSwatches.Gold;                     // Vec3
    //   var mat  = MeshSwatches.Matte(gold, 0.30, 0.06);  // Material
    //   s.Objects.Add(Mesh.FromObj(@"assets\teapot.obj", mat, 0.60f, new Vec3(...)));
    //
    // Rationale: We bias albedos toward the ANSI-16 / Windows Console defaults so the
    // ConsolePalette.NearestColor() OKLab quantizer consistently returns the intended
    // console color while still looking good under shading.
    //
    // Notes:
    // - All constants are in sRGB [0,1] and chosen from or derived from the 16-color palette.
    // - The *Soft variants are slightly dimmed to reduce highlight blowout on bright primaries.
    // - Matte()/Mirror() helpers are provided for convenience; tweak spec/reflection per asset.

    using System;

    namespace ConsoleRayTracing
    {
        public static class MeshSwatches
        {
            // Exact 16-color console sRGB anchors (match ConsolePalette order):
            // Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta, DarkYellow, Gray,
            // DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White
            // (Kept here locally so scenes can pick deterministic swatches without peeking into ConsolePalette internals.)
            private static readonly Vec3[] Palette16 = new Vec3[]
            {
            new Vec3(0.00f,0.00f,0.00f),  // Black
            new Vec3(0.00f,0.00f,0.50f),  // DarkBlue
            new Vec3(0.00f,0.50f,0.00f),  // DarkGreen
            new Vec3(0.00f,0.50f,0.50f),  // DarkCyan
            new Vec3(0.50f,0.00f,0.00f),  // DarkRed
            new Vec3(0.50f,0.00f,0.50f),  // DarkMagenta
            new Vec3(0.50f,0.50f,0.00f),  // DarkYellow
            new Vec3(0.75f,0.75f,0.75f),  // Gray
            new Vec3(0.50f,0.50f,0.50f),  // DarkGray
            new Vec3(0.00f,0.00f,1.00f),  // Blue
            new Vec3(0.00f,1.00f,0.00f),  // Green
            new Vec3(0.00f,1.00f,1.00f),  // Cyan
            new Vec3(1.00f,0.00f,0.00f),  // Red
            new Vec3(1.00f,0.00f,1.00f),  // Magenta
            new Vec3(1.00f,1.00f,0.00f),  // Yellow
            new Vec3(1.00f,1.00f,1.00f)   // White
            };

            private static Vec3 FromConsole(ConsoleColor c)
            {
                int idx = (int)c;
                if (idx < 0 || idx >= Palette16.Length) return Palette16[0];
                return Palette16[idx];
            }

            private static Vec3 Scale(ConsoleColor c, float k)
            {
                if (k < 0.0f) k = 0.0f;
                if (k > 1.0f) k = 1.0f;
                Vec3 v = FromConsole(c);
                return new Vec3(v.X * k, v.Y * k, v.Z * k);
            }

            // ---------- Neutrals (great for bones, clay, porcelain, etc.) ----------
            public static readonly Vec3 Black = FromConsole(ConsoleColor.Black);

            public static readonly Vec3 Charcoal = FromConsole(ConsoleColor.DarkGray);
            public static readonly Vec3 Stone = FromConsole(ConsoleColor.Gray);
            public static readonly Vec3 WhiteSoft = Scale(ConsoleColor.White, 0.85f);   // avoids harsh clipping
            public static readonly Vec3 White = FromConsole(ConsoleColor.White);

            // ---------- Metals / warm materials ----------
            public static readonly Vec3 Gold = Scale(ConsoleColor.Yellow, 0.90f);

            public static readonly Vec3 Brass = Scale(ConsoleColor.DarkYellow, 1.00f);
            public static readonly Vec3 Copper = new Vec3(0.80f, 0.45f, 0.25f);      // will quantize near DarkYellow/Red

            // ---------- Gem-ish primaries (bright but softened a bit) ----------
            public static readonly Vec3 Ruby = Scale(ConsoleColor.Red, 0.92f);

            public static readonly Vec3 Emerald = Scale(ConsoleColor.Green, 0.85f);
            public static readonly Vec3 Sapphire = Scale(ConsoleColor.Blue, 0.85f);
            public static readonly Vec3 Amethyst = Scale(ConsoleColor.Magenta, 0.88f);
            public static readonly Vec3 CyanSoft = Scale(ConsoleColor.Cyan, 0.85f);
            public static readonly Vec3 Jade = Scale(ConsoleColor.DarkCyan, 1.00f);

            // ---------- Helpful dark accents ----------
            public static readonly Vec3 OxideRed = FromConsole(ConsoleColor.DarkRed);

            public static readonly Vec3 PineGreen = FromConsole(ConsoleColor.DarkGreen);
            public static readonly Vec3 Navy = FromConsole(ConsoleColor.DarkBlue);
            public static readonly Vec3 Plum = FromConsole(ConsoleColor.DarkMagenta);

            // ---------- Console-aligned primaries (exact anchors) ----------
            public static readonly Vec3 Red = FromConsole(ConsoleColor.Red);

            public static readonly Vec3 Green = FromConsole(ConsoleColor.Green);
            public static readonly Vec3 Blue = FromConsole(ConsoleColor.Blue);
            public static readonly Vec3 Magenta = FromConsole(ConsoleColor.Magenta);
            public static readonly Vec3 Yellow = FromConsole(ConsoleColor.Yellow);
            public static readonly Vec3 Cyan = FromConsole(ConsoleColor.Cyan);

            // ---------- Material helpers ----------
            public static Material Matte(Vec3 albedo, double specular = 0.10, double reflectivity = 0.00)
            {
                return new Material(albedo, specular, reflectivity, Vec3.Zero);
            }

            public static Material Mirror(Vec3 tint, double reflectivity = 0.85)
            {
                return new Material(tint, 0.0, reflectivity, Vec3.Zero);
            }

            public static Material Emissive(Vec3 emission)
            {
                return new Material(new Vec3(0.0f, 0.0f, 0.0f), 0.0, 0.0, emission);
            }
        }
    }

    public static class ConsolePalette
    {
        private static readonly ConsoleColor[] Colors = new ConsoleColor[]
        {
            ConsoleColor.Black, ConsoleColor.DarkBlue, ConsoleColor.DarkGreen, ConsoleColor.DarkCyan, ConsoleColor.DarkRed, ConsoleColor.DarkMagenta, ConsoleColor.DarkYellow, ConsoleColor.Gray, ConsoleColor.DarkGray, ConsoleColor.Blue, ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Red, ConsoleColor.Magenta, ConsoleColor.Yellow, ConsoleColor.White
        };

        private static readonly Vec3[] SRGB = new Vec3[]
        {
            new Vec3(0.0f,0.0f,0.0f),   // Black
            new Vec3(0.0f,0.0f,0.5f),   // DarkBlue
            new Vec3(0.0f,0.5f,0.0f),   // DarkGreen
            new Vec3(0.0f,0.5f,0.5f),   // DarkCyan
            new Vec3(0.5f,0.0f,0.0f),   // DarkRed
            new Vec3(0.5f,0.0f,0.5f),   // DarkMagenta
            new Vec3(0.5f,0.5f,0.0f),   // DarkYellow
            new Vec3(0.75f,0.75f,0.75f),// Gray
            new Vec3(0.5f,0.5f,0.5f),   // DarkGray
            new Vec3(0.0f,0.0f,1.0f),   // Blue
            new Vec3(0.0f,1.0f,0.0f),   // Green
            new Vec3(0.0f,1.0f,1.0f),   // Cyan
            new Vec3(1.0f,0.0f,0.0f),   // Red
            new Vec3(1.0f,0.0f,1.0f),   // Magenta
            new Vec3(1.0f,1.0f,0.0f),   // Yellow
            new Vec3(1.0f,1.0f,1.0f)    // White
        };

        private static readonly Vec3[] OKLab;     // (L, a, b)
        private static readonly Vec3[] OKLCh;     // (L, C, hRadians) precomputed for palette
        private static readonly int[] GrayIndices = new int[] { 0, 8, 7, 15 }; // Black, DarkGray, Gray, White

        // Tunables (empirically good for 16-color mapping)
        private const float ChromaNeutralThreshold = 0.020f;        // lower than before so fewer colors collapse to gray

        private const float LWeight = 0.5f;                         // lightness weight
        private const float CWeight = 1.8f;                         // chroma weight
        private const float HWeightBase = 1.0f;                     // base hue weight
        private const float HueChromaBoost = 0.6f;                  // hue weight scales with avg chroma: HWeightBase*(HueChromaBoost+avgC)

        static ConsolePalette()
        {
            int n = SRGB.Length;
            OKLab = new Vec3[n];
            OKLCh = new Vec3[n];
            for (int i = 0; i < n; i++)
            {
                Vec3 lab = SRGBtoOKLab(SRGB[i]);
                OKLab[i] = lab;
                OKLCh[i] = LabToLCh(lab);
            }
        }

        // Input c is expected in sRGB [0,1] (already tone-mapped + gamma, e.g., Gamma(...).Saturate()).
        public static ConsoleColor NearestColor(Vec3 c)
        {
            Vec3 clamped = new Vec3(Clamp01(c.X), Clamp01(c.Y), Clamp01(c.Z));
            Vec3 lab = SRGBtoOKLab(clamped);
            Vec3 lch = LabToLCh(lab);
            float L = lch.X;
            float C = lch.Y;
            float h = lch.Z;

            if (C < ChromaNeutralThreshold)
            {
                int bestIdx = GrayIndices[0];
                float bestD = float.MaxValue;
                for (int gi = 0; gi < GrayIndices.Length; gi++)
                {
                    int idx = GrayIndices[gi];
                    float dL = L - OKLCh[idx].X;
                    float dist = (dL * dL) * 1.2f; // emphasize correct gray level a bit more
                    if (dist < bestD)
                    {
                        bestD = dist;
                        bestIdx = idx;
                    }
                }
                return Colors[bestIdx];
            }
            else
            {
                int bestIdx = 0;
                float bestD = float.MaxValue;
                for (int i = 0; i < OKLCh.Length; i++)
                {
                    // De-prioritize gray candidates when input is clearly chromatic
                    bool isGrayCandidate = IsGrayIndex(i);
                    float grayPenalty = isGrayCandidate ? 0.08f : 0.0f;

                    float Li = OKLCh[i].X;
                    float Ci = OKLCh[i].Y;
                    float hi = OKLCh[i].Z;

                    float dL = L - Li;
                    float dC = C - Ci;
                    float dh = WrapAngle(h - hi);
                    float avgC = 0.5f * (C + Ci);
                    float wH = HWeightBase * (HueChromaBoost + avgC);

                    float dist = LWeight * dL * dL + CWeight * dC * dC + wH * dh * dh + grayPenalty;
                    if (dist < bestD)
                    {
                        bestD = dist;
                        bestIdx = i;
                    }
                }
                return Colors[bestIdx];
            }
        }

        // -------- Perceptual color helpers (OKLab / OKLCh) --------

        private static Vec3 SRGBtoOKLab(Vec3 srgb)
        {
            float r = SRGBToLinear(srgb.X);
            float g = SRGBToLinear(srgb.Y);
            float b = SRGBToLinear(srgb.Z);

            float l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
            float m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
            float s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

            float l_ = Cbrt(l);
            float m_ = Cbrt(m);
            float s_ = Cbrt(s);

            float L = 0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_;
            float A = 1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_;
            float B = 0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_;

            return new Vec3(L, A, B);
        }

        private static Vec3 LabToLCh(Vec3 lab)
        {
            float L = lab.X;
            float a = lab.Y;
            float b = lab.Z;
            float C = MathF.Sqrt(a * a + b * b);
            float h = MathF.Atan2(b, a); // [-π, π]
            return new Vec3(L, C, h);
        }

        private static float WrapAngle(float a)
        {
            while (a > MathF.PI) a -= 2.0f * MathF.PI;
            while (a < -MathF.PI) a += 2.0f * MathF.PI;
            return a;
        }

        private static bool IsGrayIndex(int idx)
        {
            for (int i = 0; i < GrayIndices.Length; i++)
            {
                if (GrayIndices[i] == idx) return true;
            }
            return false;
        }

        private static float SRGBToLinear(float c)
        {
            if (c <= 0.04045f) return c / 12.92f;
            return MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
        }

        private static float Cbrt(float x)
        {
            if (x <= 0.0f) return 0.0f;
            return MathF.Pow(x, 1.0f / 3.0f);
        }

        private static float Clamp01(float v)
        {
            if (v < 0.0f) return 0.0f;
            if (v > 1.0f) return 1.0f;
            return v;
        }
    }
}
// File: ObjMesh.cs
using System.Globalization;
using System.Text;

namespace ConsoleRayTracing
{
    public sealed class Mesh : Hittable
    {
        public readonly Vec3 BoundsMin;
        public readonly Vec3 BoundsMax;

        private readonly BVH bvh; // local BVH over triangles

        private Mesh(List<Triangle> triangles, Vec3 min, Vec3 max)
        {
            BoundsMin = min;
            BoundsMax = max;
            bvh = new BVH(triangles);
        }

        public override bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            return bvh.Hit(r, tMin, tMax, ref rec);
        }

        public static Mesh FromObj(string path, Material defaultMaterial, float scale = 1.0f, Vec3? translate = null)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path");
            if (!File.Exists(path)) throw new FileNotFoundException("OBJ not found", path);

            Vec3 t = translate ?? new Vec3(0.0f, 0.0f, 0.0f);
            CultureInfo ci = CultureInfo.InvariantCulture;

            List<Vec3> positions = new List<Vec3>(1 << 16);
            List<Vec3> normals = new List<Vec3>(1 << 15); // optional, we don't require them
            List<(float u, float v)> uvs = new List<(float, float)>(1 << 15); // optional
            List<Triangle> tris = new List<Triangle>(1 << 17);

            bool haveAny = false;
            float minX = 1e30f, minY = 1e30f, minZ = 1e30f;
            float maxX = -1e30f, maxY = -1e30f, maxZ = -1e30f;

            Span<Range> ranges = stackalloc Range[16];
            Span<int> vIdx = stackalloc int[16];

            using (var sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length == 0 || line[0] == '#') continue;
                    var lineSpan = line.AsSpan();
                    var tokCount = lineSpan.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries);
                    if (tokCount == 0) continue;

                    if (Ascii.Equals(lineSpan[ranges[0]], "v") && tokCount >= 4)
                    {
                        float x = float.Parse(lineSpan[ranges[1]], ci) * scale + t.X;
                        float y = float.Parse(lineSpan[ranges[2]], ci) * scale + t.Y;
                        float z = float.Parse(lineSpan[ranges[3]], ci) * scale + t.Z;
                        positions.Add(new Vec3(x, y, z));
                        if (!haveAny) haveAny = true;
                        if (x < minX) minX = x; if (y < minY) minY = y; if (z < minZ) minZ = z;
                        if (x > maxX) maxX = x; if (y > maxY) maxY = y; if (z > maxZ) maxZ = z;
                    }
                    else if (Ascii.Equals(lineSpan[ranges[0]], "vn") && tokCount >= 4)
                    {
                        float x = float.Parse(lineSpan[ranges[1]], ci);
                        float y = float.Parse(lineSpan[ranges[2]], ci);
                        float z = float.Parse(lineSpan[ranges[3]], ci);
                        normals.Add(new Vec3(x, y, z).Normalized());
                    }
                    else if (Ascii.Equals(lineSpan[ranges[0]], "vt") && tokCount >= 3)
                    {
                        float u = float.Parse(lineSpan[ranges[1]], ci);
                        float v = float.Parse(lineSpan[ranges[2]], ci);
                        uvs.Add((u, v));
                    }
                    else if (Ascii.Equals(lineSpan[ranges[0]], "f") && tokCount >= 4)
                    {
                        // Parse polygon -> fan triangulate: (0, i-1, i)
                        int faceVerts = tokCount - 1;

                        for (int i = 0; i < faceVerts; i++)
                        {
                            // token forms: v, v/vt, v//vn, v/vt/vn; OBJ indexing is 1-based, negatives allowed

                            var tokenSpan = lineSpan[ranges[i + 1]];
                            var partCount = tokenSpan.Split(ranges, '/');
                            int vi = ParseIndex(tokenSpan[ranges[0]], positions.Count);
                            vIdx[i] = vi;
                        }

                        for (int i = 2; i < faceVerts; i++)
                        {
                            Vec3 a = positions[vIdx[0]];
                            Vec3 b = positions[vIdx[i - 1]];
                            Vec3 c = positions[vIdx[i]];
                            tris.Add(new Triangle(a, b, c, defaultMaterial));
                        }
                    }
                    // ignore: mtllib/usemtl/o/g/s — keep loader minimal and robust
                }
            }

            if (!haveAny || tris.Count == 0) throw new InvalidDataException("OBJ had no triangles.");

            Vec3 mn = new Vec3(minX, minY, minZ);
            Vec3 mx = new Vec3(maxX, maxY, maxZ);
            return new Mesh(tris, mn, mx);
        }

        private static int ParseIndex(ReadOnlySpan<char> token, int count)
        {
            if (token.Length == 0) return 0;
            int idx = int.Parse(token, CultureInfo.InvariantCulture);
            if (idx > 0) return idx - 1; // 1-based -> 0-based
            return count + idx;          // negative indices: -1 = last
        }
    }
}

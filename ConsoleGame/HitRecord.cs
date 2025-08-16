namespace ConsoleRayTracing
{
    public struct HitRecord
    {
        public float T;
        public Vec3 P;
        public Vec3 N;
        public Material Mat;
        public float U;
        public float V;
        public int DebugNodeVisits;
        public int DebugAabbTests;
        public int DebugMisses;
        public int DebugLeafTests;
        public int DebugLeafHits;
        public int DebugHitDepth;
        public int DebugStackPeak;
        public int DebugLeafId;
        public int DebugWasBVH;
    }

}

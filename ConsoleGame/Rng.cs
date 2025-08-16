namespace ConsoleRayTracing
{
    public struct Rng
    {
        private ulong state;

        public Rng(ulong seed)
        {
            state = seed + 0x9E3779B97F4A7C15UL;
            state = Scramble(state);
        }

        public float NextUnit()
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong z = Scramble(state);
            return (float)((z >> 11) * (1.0 / 9007199254740992.0));
        }

        private static ulong Scramble(ulong x)
        {
            x ^= x >> 30;
            x *= 0xBF58476D1CE4E5B9UL;
            x ^= x >> 27;
            x *= 0x94D049BB133111EBUL;
            x ^= x >> 31;
            return x;
        }
    }
}

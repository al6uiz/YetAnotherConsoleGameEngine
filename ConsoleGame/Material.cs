namespace ConsoleRayTracing
{
    public sealed class Material
    {
        public Vec3 Albedo;
        public double Specular;
        public double Reflectivity;
        public Vec3 Emission;

        public Material(Vec3 albedo, double specular, double reflectivity, Vec3 emission)
        {
            Albedo = albedo;
            Specular = specular;
            Reflectivity = reflectivity;
            Emission = emission;
        }
    }
}

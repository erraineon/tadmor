namespace Tadmor.Services.Sonagen
{
    public abstract class SonaWeightedObject
    {
        public int Id { get; set; }
        public string? Value { get; set; }
        public float Weight { get; set; } = 1f;
    }
}
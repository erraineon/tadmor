namespace Tadmor.Services.NationalDay
{
    public class Holiday
    {
        public Holiday(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name { get; }
        public string Url { get; }
    }
}
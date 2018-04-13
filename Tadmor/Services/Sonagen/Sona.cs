using System.Linq;

namespace Tadmor.Services.Sonagen
{
    public class Sona
    {
        public string Species { get; set; }
        public string Gender { get; set; }
        public ILookup<string, (string value, AttributeType type)> AttributesByGroup { get; set; }
        public string Description { get; set; }
    }
}
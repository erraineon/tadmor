using System.Collections.Generic;

namespace Tadmor.Services.Sonagen
{
    public class AttributeGroup : SonaWeightedObject
    {
        public int Max { get; set; } = 1;
        public List<SonaAttribute> Attributes { get; set; } = null!;
    }
}
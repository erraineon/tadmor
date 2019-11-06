using System.Collections.Generic;

namespace Tadmor.Services.Sonagen
{
    public class SonagenOptions
    {
        public List<AttributeGroup> AttributeGroups { get; set; } = null!;
        public List<SonaGender> Genders { get; set; } = null!;
        public List<SonaSpecies> Species { get; set; } = null!;
    }
}
using System.Collections.Generic;

namespace Tadmor.Services.Sonagen
{
    public class SonagenOptions
    {
        public List<AttributeGroup> AttributeGroups { get; set; }
        public List<SonaGender> Genders { get; set; }
        public List<SonaSpecies> Species { get; set; }
    }
}
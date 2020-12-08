using System.Collections.Generic;

namespace Tadmor.Services.Compliments
{
    [Options]
    public class ComplimentsOptions
    {
        public List<string> Compliments { get; set; } = new List<string>();
    }
}
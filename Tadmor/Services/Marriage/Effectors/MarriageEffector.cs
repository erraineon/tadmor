using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    public abstract class MarriageEffector
    {
        protected MarriageEffector(StringLogger logger)
        {
            Logger = logger;
        }

        protected readonly StringLogger Logger;

        protected string GetBabyNames(IEnumerable<Baby> babies) => babies.Select(b => b.Name).Humanize();
    }
}
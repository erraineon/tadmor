using System;
using Humanizer;

namespace Tadmor.Services.Marriage.Babies
{
    public abstract class Baby
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return $"{Name} - {GetType().Name.Humanize()}: {GetDescription()}";
        }

        public abstract string GetDescription();
    }
}
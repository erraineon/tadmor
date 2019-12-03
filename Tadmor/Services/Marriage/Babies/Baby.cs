using System;
using System.Threading.Tasks;
using Humanizer;

namespace Tadmor.Services.Marriage.Babies
{
    public abstract class Baby
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }

        public override string ToString()
        {
            return $"{Name} - {GetType().Name.Humanize()}: {GetDescription()}";
        }

        public abstract string GetDescription();
        public abstract Task Release(MarriedCouple marriage);
    }
}
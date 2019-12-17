using System;
using System.Threading.Tasks;
using Humanizer;

namespace Tadmor.Services.Marriage
{
    public abstract class Baby
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public int Rank { get; set; }

        public virtual Task ExecuteCombinePrecondition(MarriedCouple marriage)
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"{Name} {GetStarRank()} - {GetType().Name.Humanize()}: {GetDescription()}";
        }

        public string GetStarRank()
        {
            var flooredHalfRank = (int) Math.Floor(Rank / 2f);
            var halfStar = Rank % 0 == 0 ? string.Empty : "+";
            var rank = $"{new string('★', flooredHalfRank)}{halfStar}";
            return rank;
        }

        public abstract string GetDescription();
        public abstract Task Release(MarriedCouple marriage);
    }
}
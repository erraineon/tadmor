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
        public int Rank { get; set; }
        public virtual bool CanCombine { get; } = true;

        public override string ToString()
        {
            return $"{Name} {GetStarRank()} - {GetType().Name.Humanize()}: {GetDescription()}";
        }

        public string GetStarRank()
        {
            var halfRank = Rank / 2f;
            var flooredHalfRank = (int) Math.Floor(halfRank);
            var halfStar = halfRank - flooredHalfRank > 0 ? "+" : string.Empty;
            var rank = $"{new string('★', flooredHalfRank)}{halfStar}";
            return rank;
        }

        public abstract string GetDescription();
        public abstract Task Release(MarriedCouple marriage);
    }
}
using System.Linq;
using Tadmor.Logging;

namespace Tadmor.Services.Marriage
{
    public class PatientBabyEffector : MarriageEffector, IKissGainEffector, IAttemptedKissOnCooldownAffector
    {
        public PatientBabyEffector(StringLogger logger) : base(logger)
        {
        }

        public double GetNewValue(double current, double seed, MarriedCouple couple)
        {
            var gainMultiplier = couple.Babies.OfType<PatientBaby>().Sum(b => b.Rank);
            return current + seed * gainMultiplier;
        }

        public void Execute(MarriedCouple couple)
        {
            var patientBabies = couple.Babies.OfType<PatientBaby>().ToList();
            if (patientBabies.Any())
            {
                var divider = patientBabies.Count + 1;
                couple.Kisses /= divider;
                foreach (var patientBaby in patientBabies) couple.Babies.Remove(patientBaby);
                Logger.Log($"{GetBabyNames(patientBabies)} divided your total kisses by {divider} and disappeared");
            }
        }
    }
}
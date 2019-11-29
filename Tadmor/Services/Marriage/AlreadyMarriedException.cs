using System;

namespace Tadmor.Services.Marriage
{
    internal class AlreadyMarriedException : Exception
    {
        public MarriedCouple ExistingMarriage { get; }

        public AlreadyMarriedException(MarriedCouple existingMarriage)
        {
            ExistingMarriage = existingMarriage;
        }
    }
}
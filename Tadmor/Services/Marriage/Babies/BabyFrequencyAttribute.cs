using System;

namespace Tadmor.Services.Marriage
{
    internal class BabyFrequencyAttribute : Attribute
    {
        public BabyFrequencyAttribute(double weight)
        {
            Weight = weight;
        }

        public double Weight { get; }
    }
}
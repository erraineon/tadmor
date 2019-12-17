using System;

namespace Tadmor.Services.Marriage
{
    internal class BabyFrequencyAttribute : Attribute
    {
        public BabyFrequencyAttribute(float weight)
        {
            Weight = weight;
        }

        public float Weight { get; }
    }
}
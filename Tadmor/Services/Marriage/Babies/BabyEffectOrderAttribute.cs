using System;

namespace Tadmor.Services.Marriage.Babies
{
    public class BabyEffectOrderAttribute : Attribute
    {
        public int Order { get; }

        public BabyEffectOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
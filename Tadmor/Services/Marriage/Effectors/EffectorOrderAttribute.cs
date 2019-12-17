using System;

namespace Tadmor.Services.Marriage
{
    public class EffectorOrderAttribute : Attribute
    {
        public int Order { get; }

        public EffectorOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
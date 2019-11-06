using System;

namespace Tadmor.Services
{
    public class ScopedServiceAttribute : Attribute { }
    public class SingletonServiceAttribute : Attribute { }
    public class TransientServiceAttribute : Attribute { }
}
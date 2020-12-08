using System;

namespace Tadmor.Commands.Interfaces
{
    public interface IModuleRegistration
    {
        Type ModuleType { get; }
    }
}
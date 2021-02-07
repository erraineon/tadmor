using System;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface IModuleRegistration
    {
        Type ModuleType { get; }
    }
}
using System;
using Tadmor.Core.Commands.Interfaces;

namespace Tadmor.Core.Commands.Models
{
    public sealed record ModuleRegistration(Type ModuleType) : IModuleRegistration;
}
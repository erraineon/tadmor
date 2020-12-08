using System;
using Tadmor.Commands.Interfaces;

namespace Tadmor.Commands.Models
{
    public sealed record ModuleRegistration(Type ModuleType) : IModuleRegistration;
}
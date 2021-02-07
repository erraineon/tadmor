using System;

namespace Tadmor.Core.Commands.Models
{
    public class ModuleException : Exception
    {
        public ModuleException(string? message) : base(message)
        {
        }
    }
}
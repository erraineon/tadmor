using System;

namespace Tadmor.Commands.Models
{
    public class ModuleException : Exception
    {
        public ModuleException(string? message) : base(message)
        {
        }
    }
}
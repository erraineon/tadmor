using System;

namespace Tadmor.Commands.Models
{
    public class FrontEndException : Exception
    {
        public FrontEndException(string? message) : base(message)
        {
        }
    }
}
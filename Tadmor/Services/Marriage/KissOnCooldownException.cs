using System;

namespace Tadmor.Services.Marriage
{
    public class KissOnCooldownException : Exception
    {
        public KissOnCooldownException(string message) : base(message)
        {
        }
    }
}
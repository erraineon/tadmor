using System;
using Microsoft.Extensions.Options;

namespace Tadmor.Utils
{
    public interface IWritableOptionsSnapshot<out T> : IOptionsSnapshot<T>, IDisposable where T : class, new()
    {
    }
}
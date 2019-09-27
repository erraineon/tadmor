using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Tadmor.Utils
{
    public interface IWritableOptionsSnapshot<out T> : IOptionsSnapshot<T> where T : class, new()
    {
        Task UpdateAsync(Action<T> applyChanges);
    }
}
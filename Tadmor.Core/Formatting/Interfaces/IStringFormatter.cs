using System.Threading.Tasks;

namespace Tadmor.Core.Formatting.Interfaces
{
    public interface IStringFormatter<in T>
    {
        Task<string> ToStringAsync(T value);
    }
}
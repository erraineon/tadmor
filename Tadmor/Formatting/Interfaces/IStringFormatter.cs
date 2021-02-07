using System.Threading.Tasks;

namespace Tadmor.Formatting.Interfaces
{
    public interface IStringFormatter<in T>
    {
        Task<string> ToStringAsync(T value);
    }
}
using System.Threading.Tasks;

namespace Tadmor.Abstractions.Interfaces
{
    public interface IStringFormatter<in T>
    {
        Task<string> ToStringAsync(T value);
    }
}
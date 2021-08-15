using System.Threading.Tasks;

namespace Tadmor.TextGeneration.Interfaces
{
    public interface ITadmorMindClient
    {
        Task<string> GenerateAsync();
    }
}
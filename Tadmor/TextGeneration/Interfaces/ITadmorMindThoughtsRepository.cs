using System.Threading.Tasks;

namespace Tadmor.TextGeneration.Interfaces
{
    public interface ITadmorMindThoughtsRepository
    {
        Task<string> ReceiveAsync();
        void Add(string thought);
        int Count { get; }
    }
}
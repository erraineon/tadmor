using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Tadmor.TextGeneration.Interfaces;

namespace Tadmor.TextGeneration.Services
{
    public class TadmorMindThoughtsRepository : ITadmorMindThoughtsRepository
    {
        private readonly BufferBlock<string> _entriesBuffer = new();

        public async Task<string> ReceiveAsync()
        {
            return await _entriesBuffer.ReceiveAsync();
        }
        public void Add(string thought)
        {
            _entriesBuffer.Post(thought);
        }

        public int Count => _entriesBuffer.Count;
    }
}
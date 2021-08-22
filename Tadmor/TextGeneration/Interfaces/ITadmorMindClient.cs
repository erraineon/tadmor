using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tadmor.TextGeneration.Interfaces
{
    public interface ITadmorMindClient
    {
        Task<List<string>> GenerateEntriesAsync();
    }
}
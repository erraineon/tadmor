using System.Threading.Tasks;
using Discord;

namespace Tadmor.Utils
{
    public interface IReplyMessage : IMessage
    {
        Task<IMessage?> GetQuotedMessageAsync();
    }
}
using System.Threading.Tasks;
using Discord;

namespace Tadmor.Services.Abstractions
{
    public interface IMessageListener
    {
        Task OnMessageReceivedAsync(IDiscordClient client, IMessage message);
    }
}
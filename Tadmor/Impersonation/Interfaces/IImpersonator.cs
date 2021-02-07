using System;
using System.Threading.Tasks;
using Discord;

namespace Tadmor.Impersonation.Interfaces
{
    public interface IImpersonator
    {
        Task WhileImpersonating(IDiscordClient chatClient, IUser target, Func<Task> asScope);
    }
}
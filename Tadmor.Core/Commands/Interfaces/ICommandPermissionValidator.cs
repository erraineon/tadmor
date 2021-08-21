using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface ICommandPermissionValidator
    {
        Task<bool?> CanRunAsync(ExecuteCommandRequest executeCommandRequest, CancellationToken cancellationToken);
        Task<bool> CanRunAsync(ICommandContext commandContext, CommandInfo commandInfo);
    }
}
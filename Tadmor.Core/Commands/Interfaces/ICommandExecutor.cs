using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface ICommandExecutor
    {
        Task<IResult> ExecuteAsync(ExecuteCommandRequest request, CancellationToken cancellationToken);
    }
}
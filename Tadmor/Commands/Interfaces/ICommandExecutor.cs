using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Commands.Models;

namespace Tadmor.Commands.Interfaces
{
    public interface ICommandExecutor
    {
        Task<IResult> ExecuteAsync(ExecuteCommandRequest request, CancellationToken cancellationToken);
    }
}
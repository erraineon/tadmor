using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;

namespace Tadmor.Commands.Services
{
    public class CommandResultPublisher : ICommandResultPublisher
    {
        public async Task PublishAsync(PublishCommandResultRequest request, CancellationToken cancellationToken)
        {
            switch (request.CommandResult)
            {
                case RuntimeResult runtimeResult:
                    await request.CommandContext.Channel.SendMessageAsync(runtimeResult.Reason);
                    break;
                case ExecuteResult {Exception: FrontEndException e}:
                    await request.CommandContext.Channel.SendMessageAsync(e.Message);
                    break;
            }
        }
    }
}
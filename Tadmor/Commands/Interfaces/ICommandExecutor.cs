using System.Threading.Tasks;
using Discord.Commands;

namespace Tadmor.Commands.Interfaces
{
    public interface ICommandExecutor
    {
        Task<IResult> BeginExecutionAsync(ICommandContext commandContext, string input);
    }
}
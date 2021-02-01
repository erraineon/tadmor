using System.Threading;
using System.Threading.Tasks;
using Tadmor.Commands.Models;

namespace Tadmor.Commands.Interfaces
{
    public interface ICommandPrefixValidator
    {
        Task<CommandPrefixValidationResult> ValidatePrefix(MessageValidatedNotification notification, CancellationToken cancellationToken);
    }
}
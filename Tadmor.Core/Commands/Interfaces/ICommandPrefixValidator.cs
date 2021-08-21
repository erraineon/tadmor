using System.Threading;
using System.Threading.Tasks;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface ICommandPrefixValidator
    {
        Task<CommandPrefixValidationResult> ValidatePrefix(
            MessageValidatedNotification notification,
            CancellationToken cancellationToken);
    }
}
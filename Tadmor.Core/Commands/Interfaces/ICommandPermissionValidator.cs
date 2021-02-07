using System.Threading;
using System.Threading.Tasks;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface ICommandPermissionValidator
    {
        Task<bool> CanRunAsync(ExecuteCommandRequest executeCommandRequest, CancellationToken cancellationToken);
    }
}
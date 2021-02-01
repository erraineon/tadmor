using System.Threading;
using System.Threading.Tasks;
using Tadmor.Commands.Models;

namespace Tadmor.Commands.Interfaces
{
    public interface ICommandPermissionValidator
    {
        Task<bool> CanRunAsync(ExecuteCommandRequest executeCommandRequest, CancellationToken cancellationToken);
    }
}
using System.Threading;
using System.Threading.Tasks;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface ICommandResultPublisher
    {
        Task PublishAsync(PublishCommandResultRequest request, CancellationToken cancellationToken);
    }
}
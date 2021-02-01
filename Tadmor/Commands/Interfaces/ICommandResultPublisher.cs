using System.Threading;
using System.Threading.Tasks;
using Tadmor.Commands.Models;

namespace Tadmor.Commands.Interfaces
{
    public interface ICommandResultPublisher
    {
        Task PublishAsync(PublishCommandResultRequest request, CancellationToken cancellationToken);
    }
}
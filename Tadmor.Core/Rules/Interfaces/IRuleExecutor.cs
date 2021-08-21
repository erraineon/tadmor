using System.Threading;
using System.Threading.Tasks;

namespace Tadmor.Core.Rules.Interfaces
{
    public interface IRuleExecutor
    {
        Task ExecuteRuleAsync(IRuleTriggerContext ruleTriggerContext, CancellationToken cancellationToken);
    }
}
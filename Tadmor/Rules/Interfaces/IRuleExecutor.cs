using System.Threading;
using System.Threading.Tasks;

namespace Tadmor.Rules.Interfaces
{
    public interface IRuleExecutor
    {
        Task ExecuteRuleAsync(IRuleTriggerContext ruleTriggerContext, CancellationToken cancellationToken);
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Interfaces
{
    public interface ITimeRuleProvider
    {
        Task<ILookup<ulong, TimeRule>> GetRulesByChannelId(ulong guildId, DateTime dueDate);
    }
}
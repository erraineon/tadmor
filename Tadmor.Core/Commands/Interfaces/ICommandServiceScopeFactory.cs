﻿using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface ICommandServiceScopeFactory
    {
        Task<IServiceScope> CreateScopeAsync(ICommandContext commandContext);
    }
}
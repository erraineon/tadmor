using System;
using Discord.Commands;
using Tadmor.Core.Commands.Interfaces;

namespace Tadmor.Core.Commands.Services
{
    public class CommandContextResolver : ICommandContextResolver
    {
        private ICommandContext? _currentCommandContext;

        public ICommandContext CurrentCommandContext
        {
            get => _currentCommandContext ??
                throw new Exception($"the command context was not set on the {nameof(ICommandContextResolver)}");
            set => _currentCommandContext = value;
        }
    }
}
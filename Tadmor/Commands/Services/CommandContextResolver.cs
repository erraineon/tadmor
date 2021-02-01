using System;
using Discord.Commands;
using Tadmor.Commands.Interfaces;

namespace Tadmor.Commands.Services
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
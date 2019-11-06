using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Tadmor.Utils;

namespace Tadmor.Services.Options
{
    [SingletonService]
    public class ChatOptionsService
    {
        private readonly IServiceProvider _services;

        public ChatOptionsService(IServiceProvider services)
        {
            _services = services;
        }

        // TODO: add non writable options method to prevent constant rewriting
        public IWritableOptionsSnapshot<ChatOptions> GetOptions()
        {
            return _services.GetService<IWritableOptionsSnapshot<ChatOptions>>();
        }

        public GuildOptions GetGuildOptions(ulong guildId, ChatOptions chatOptions)
        {
            var guildOptions = chatOptions.GuildOptions.SingleOrDefault(options => options.Id == guildId);
            if (guildOptions == null)
            {
                guildOptions = new GuildOptions {Id = guildId};
                chatOptions.GuildOptions.Add(guildOptions);
            }

            return guildOptions;
        }

        public string GetCommandsPrefix(IGuild guild)
        {
            var options = GetOptions();
            var guildOptions = GetGuildOptions(guild.Id, options.Value);
            var commandPrefix = guildOptions.CommandPrefix is var p && !string.IsNullOrEmpty(p) ? p : ".";
            return commandPrefix;
        }
    }
}
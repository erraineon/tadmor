using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Tadmor.Services.Commands;
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

        public IWritableOptionsSnapshot<ChatOptions> GetOptions()
        {
            return _services.GetService<IWritableOptionsSnapshot<ChatOptions>>();
        }

        public GuildOptions GetGuildOptions(ulong guildId, ChatOptions chatOptions)
        {
            var guildOptions = (chatOptions.GuildOptions ??= new List<GuildOptions>())
                .SingleOrDefault(options => options.Id == guildId);
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

        public IEnumerable<CommandUsagePermission> GetPermissions(string commandName)
        {
            var commandOptions = GetOptions();
            return GetPermissions(commandName, commandOptions.Value);
        }

        private static IEnumerable<CommandUsagePermission> GetPermissions(string commandName, ChatOptions commandOptions)
        {
            var permissions = (commandOptions.CommandUsagePermissions ??= new List<CommandUsagePermission>())
                .Where(options => string.Equals(options.CommandName, commandName, StringComparison.OrdinalIgnoreCase));
            return permissions;
        }

        public void AddOrUpdatePermissions(string commandName, IEntity<ulong> entity, PermissionType permissionType)
        {
            using var options = GetOptions();
            var scopeType = entity switch
            {
                IUser _ => CommandUsagePermissionScopeType.User,
                IChannel _ => CommandUsagePermissionScopeType.Channel,
                IGuild _ => CommandUsagePermissionScopeType.Guild,
                _ => throw new NotSupportedException($"only users, channels and guilds are supported")
            };
            var permission = GetPermissions(commandName, options.Value)
                .FirstOrDefault(p => p.ScopeId == entity.Id && p.ScopeType == scopeType);
            if (permission == null)
            {
                permission = new CommandUsagePermission
                {
                    CommandName = commandName,
                    ScopeId = entity.Id,
                    ScopeType = scopeType
                };
                options.Value.CommandUsagePermissions.Add(permission);
            }

            permission.PermissionType = permissionType;
        }
    }
}
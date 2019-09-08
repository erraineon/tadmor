using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Tadmor.Services.Discord;

namespace Tadmor.Modules
{
    [Summary("guild options")]
    [RequireOwner(Group = "admin")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
    public class GuildOptionsModule : ModuleBase<ICommandContext>
    {
        private readonly DiscordOptions _discordOptions;

        public GuildOptionsModule(IOptionsSnapshot<DiscordOptions> discordOptions)
        {
            _discordOptions = discordOptions.Value;
        }

        [Summary("change the prefix for commands on this guild")]
        [Command("prefix")]
        public async Task ChangePrefix(string newPrefix)
        {
            var guildId = Context.Guild.Id;
            var guildOptions = GetOrAddOptions(_discordOptions, guildId);
            guildOptions.CommandPrefix = newPrefix;
            await Program.UpdateOptions(_discordOptions);
            await ReplyAsync("ok");
        }

        [Summary("toggles good boy mode where nsfw commands are limited to nsfw channels")]
        [Command("goodboymode")]
        public async Task GoodBoyMode()
        {
            var guildId = Context.Guild.Id;
            var guildOptions = GetOrAddOptions(_discordOptions, guildId);
            guildOptions.GoodBoyMode = !guildOptions.GoodBoyMode;
            await Program.UpdateOptions(_discordOptions);
            await ReplyAsync($"good boy mode is {(guildOptions.GoodBoyMode ? "on" : "off")}");
        }

        [RequireOwner(Group = "admin")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [Group("on")]
        public class EventsModule : ModuleBase<ICommandContext>
        {
            private readonly ChatEventService _events;
            private readonly DiscordOptions _discordOptions;

            public EventsModule(IOptionsSnapshot<DiscordOptions> discordOptions, ChatEventService events)
            {
                _events = events;
                _discordOptions = discordOptions.Value;
            }

            [Summary("add a welcome response command for this guild")]
            [Command("join")]
            public async Task OnJoin([Remainder] string reaction)
            {
                await AddGuildEvent(default, reaction, default, GuildEventTriggerType.GuildJoin);
                await ReplyAsync("ok");
            }

            [Summary("add a word filter and a response command to a message")]
            [Command("filter")]
            public async Task OnInputDelete(string input, [Remainder] string reaction)
            {
                await AddRegexMatchGuildEvent(input, reaction, true);
                await ReplyAsync("ok");
            }

            [Summary("lists events")]
            [Command("ls")]
            public async Task ViewEvents()
            {
                var eventStrings = _events.GetEventInfos(Context.Guild);
                var eventsInfo = eventStrings.Any()
                    ? string.Join(Environment.NewLine, eventStrings)
                    : throw new Exception("no events on this guild");
                await ReplyAsync(eventsInfo);
            }

            [Summary("removes the event with the specified id")]
            [Command("rm")]
            public async Task RemoveEvent(string eventId)
            {
                var guildId = Context.Guild.Id;
                var guildOptions = GetOrAddOptions(_discordOptions, guildId);
                if (guildOptions.Events.SingleOrDefault(e => e.Id == eventId) is GuildEvent guildEvent)
                {
                    guildOptions.Events.Remove(guildEvent);
                    await Program.UpdateOptions(_discordOptions);
                    await ReplyAsync("ok");
                }
                else throw new Exception("event not found");
            }

            [Summary("add an event in response to a message")]
            [Command, Priority(-1)]
            public async Task OnInput(string input, [Remainder] string reaction)
            {
                await AddRegexMatchGuildEvent(input, reaction, false);
                await ReplyAsync("ok");
            }

            private async Task AddRegexMatchGuildEvent(string input, string reaction, bool deleteTrigger)
            {
                await AddGuildEvent(input, reaction, deleteTrigger, GuildEventTriggerType.RegexMatch);
            }

            private async Task AddGuildEvent(string input, string reaction, bool deleteTrigger, GuildEventTriggerType triggerType)
            {
                var guildId = Context.Guild.Id;
                var guildOptions = GetOrAddOptions(_discordOptions, guildId);
                guildOptions.Events.Add(new GuildEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    ChannelId = Context.Channel.Id,
                    Scope = GuildEventScope.Guild,
                    Reaction = reaction,
                    Trigger = input,
                    TriggerType = triggerType,
                    DeleteTrigger = deleteTrigger
                });
                await Program.UpdateOptions(_discordOptions);
            }
        }


        private static GuildOptions GetOrAddOptions(DiscordOptions discordOptions, ulong guildId)
        {
            var guildOptions = discordOptions.GuildOptions.SingleOrDefault(options => options.Id == guildId);
            if (guildOptions == null)
            {
                guildOptions = new GuildOptions {Id = guildId};
                discordOptions.GuildOptions.Add(guildOptions);
            }

            return guildOptions;
        }
    }
}
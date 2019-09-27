using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Discord;
using Tadmor.Utils;

namespace Tadmor.Modules
{
    [Summary("guild options")]
    [RequireOwner(Group = "admin")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
    public class GuildOptionsModule : ModuleBase<ICommandContext>
    {
        private readonly IWritableOptionsSnapshot<DiscordOptions> _discordOptions;

        public GuildOptionsModule(IWritableOptionsSnapshot<DiscordOptions> discordOptions)
        {
            _discordOptions = discordOptions;
        }

        [Summary("change the prefix for commands on this guild")]
        [Command("prefix")]
        public async Task ChangePrefix(string newPrefix)
        {
            await _discordOptions.UpdateAsync(options =>
            {
                var guildOptions = options.GetOrAddGuildOptions(Context.Guild.Id);
                guildOptions.CommandPrefix = newPrefix;
            });
            await ReplyAsync("ok");
        }

        [Summary("toggles good boy mode where nsfw commands are limited to nsfw channels")]
        [Command("goodboymode")]
        public async Task GoodBoyMode()
        {
            var goodBoyMode = false;
            await _discordOptions.UpdateAsync(options =>
            {
                var guildOptions = options.GetOrAddGuildOptions(Context.Guild.Id);
                guildOptions.GoodBoyMode = goodBoyMode = !guildOptions.GoodBoyMode;
            });
            await ReplyAsync($"good boy mode is {(goodBoyMode ? "on" : "off")}");
        }

        [RequireOwner(Group = "admin")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [Group("on")]
        public class EventsModule : ModuleBase<ICommandContext>
        {
            private readonly ChatEventService _events;

            public EventsModule(ChatEventService events)
            {
                _events = events;
            }

            [Summary("add a welcome response command for this guild")]
            [Command("join")]
            public async Task OnJoin([Remainder] string reaction)
            {
                await _events.AddJoinEvent(Context.Guild.Id, Context.Channel.Id, reaction);
                await ReplyAsync("ok");
            }

            [Summary("add a word filter and a response command to a message")]
            [Command("filter")]
            public async Task OnInputDelete(string input, [Remainder] string reaction)
            {
                await _events.AddInputEvent(Context.Guild.Id, Context.Channel.Id, reaction, input, true);
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
                if (await _events.TryRemoveEvent(Context.Guild.Id, eventId)) await ReplyAsync("ok");
                else throw new Exception("event not found");
            }

            [Summary("add an event in response to a message")]
            [Command]
            [Priority(-1)]
            public async Task OnInput(string input, [Remainder] string reaction)
            {
                await _events.AddInputEvent(Context.Guild.Id, Context.Channel.Id, reaction, input, false);
                await ReplyAsync("ok");
            }
        }
    }
}
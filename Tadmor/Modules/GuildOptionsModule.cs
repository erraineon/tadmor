﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Discord;
using Tadmor.Services.Options;

namespace Tadmor.Modules
{
    [Summary("guild options")]
    [RequireOwner(Group = "admin")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
    public class GuildOptionsModule : ModuleBase<ICommandContext>
    {
        private readonly ChatOptionsService _chatOptions;

        public GuildOptionsModule(ChatOptionsService chatOptions)
        {
            _chatOptions = chatOptions;
        }

        [Summary("change the prefix for commands on this guild")]
        [Command("prefix")]
        public async Task ChangePrefix(string newPrefix)
        {
            using var writableOptions = _chatOptions.GetOptions();
            var options = _chatOptions.GetGuildOptions(Context.Guild.Id, writableOptions.Value);
            options.CommandPrefix = newPrefix;
            await ReplyAsync("ok");
        }

        [Summary("toggles good boy mode where nsfw commands are limited to nsfw channels")]
        [Command("goodboymode")]
        public async Task GoodBoyMode()
        {
            using var writableOptions = _chatOptions.GetOptions();
            var options = _chatOptions.GetGuildOptions(Context.Guild.Id, writableOptions.Value);
            options.GoodBoyMode = !options.GoodBoyMode;
            await ReplyAsync($"good boy mode is {(options.GoodBoyMode ? "on" : "off")}");
        }

        [RequireOwner(Group = "admin")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [Group("on")]
        public class ChatEventsModule : ModuleBase<ICommandContext>
        {
            private readonly ChatEventService _events;

            public ChatEventsModule(ChatEventService events)
            {
                _events = events;
            }

            [Summary("add a welcome response command for this guild")]
            [Command("join")]
            public async Task OnJoin([Remainder] string reaction)
            {
                _events.AddJoinEvent(Context, reaction);
                await ReplyAsync("ok");
            }

            [Summary("add a word filter and a response command to a message")]
            [Command("filter")]
            public async Task OnInputDelete(string input, [Remainder] string reaction)
            {
                _events.AddInputEvent(Context, default, reaction, input, true);
                await ReplyAsync("ok");
            }

            [Summary("add an event that will run after the specified amount of messages")]
            [Command("every")]
            public async Task OnInputDelete(int messageCount, [Remainder] string reaction)
            {
                _events.AddEveryEvent(Context, reaction, messageCount);
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
                if (_events.TryRemoveEvent(Context.Guild.Id, eventId)) await ReplyAsync("ok");
                else throw new Exception("event not found");
            }

            [Summary("add an event in response to a message")]
            [Command]
            [Priority(-1)]
            public async Task OnInput(string input, [Remainder] string reaction)
            {
                _events.AddInputEvent(Context, default, reaction, input, false);
                await ReplyAsync("ok");
            }

            [Summary("add an event in response to a specific user's message")]
            [Command]
            [Priority(-1)]
            public async Task OnInput(IGuildUser sender, string input, [Remainder] string reaction)
            {
                _events.AddInputEvent(Context, sender, reaction, input, false);
                await ReplyAsync("ok");
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramEventProvider : ITelegramEventProvider
    {
        private readonly ITelegramGuildFactory _telegramGuildFactory;
        private readonly ITelegramGuildUserFactory _telegramGuildUserFactory;
        private readonly ITelegramChatClient _telegramChatClient;
        private readonly ITelegramUserMessageFactory _telegramUserMessageFactory;
        private readonly IUserMessageCache _userMessageCache;
        public event Func<IChatClient, IMessage, Task> MessageReceived = (_, _) => Task.CompletedTask;
        public event Func<IChatClient, IGuildUser, IGuildUser, Task> GuildMemberUpdated= (_, _, _) => Task.CompletedTask;
        public event Func<IChatClient, LogMessage, Task> Log = (_, _) => Task.CompletedTask;

        public TelegramEventProvider(
            ITelegramGuildFactory telegramGuildFactory,
            ITelegramUserMessageFactory telegramUserMessageFactory, 
            ITelegramGuildUserFactory telegramGuildUserFactory,
            ITelegramChatClient telegramChatClient,
            IUserMessageCache userMessageCache)
        {
            _telegramGuildFactory = telegramGuildFactory;
            _telegramUserMessageFactory = telegramUserMessageFactory;
            _telegramGuildUserFactory = telegramGuildUserFactory;
            _telegramChatClient = telegramChatClient;
            _userMessageCache = userMessageCache;
        }

        public async Task HandleInboundMessageAsync(Message apiMessage)
        {
            if (apiMessage.Chat.Type != ChatType.Private)
            {
                var telegramGuild = _telegramGuildFactory.Create(apiMessage.Chat);
                var telegramGuildUser = apiMessage.From.Id == (int)_telegramChatClient.CurrentUser.Id
                    ? _telegramGuildUserFactory.Create(telegramGuild, apiMessage.From, true)
                    // TODO: this is an extra http call per message, just to get up-to-date user permissions
                    // instead consider caching them and using the synchronous overload above
                    : await _telegramGuildUserFactory.CreateOrNullAsync(telegramGuild, apiMessage.From.Id);
                if (telegramGuildUser != null)
                {
                    var message = _telegramUserMessageFactory.Create(apiMessage, telegramGuild, telegramGuildUser);
                    _userMessageCache.AddUserMessage(message, telegramGuild.Id);
                    await MessageReceived(_telegramChatClient, message);
                }
            }
        }

        public async Task HandleInboundUpdateAsync(Update update)
        {
            // TODO: handle avatar updates
        }
    }
}
using System;
using System.Threading.Tasks;
using Discord;
using Tadmor.ChatClients.Interfaces;
using Tadmor.ChatClients.Telegram.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Tadmor.ChatClients.Telegram.Services
{
    public class TelegramEventProvider : ITelegramEventProvider
    {
        private readonly ITelegramGuildFactory _telegramGuildFactory;
        private readonly ITelegramUserMessageFactory _telegramUserMessageFactory;
        private readonly ITelegramGuildUserFactory _telegramGuildUserFactory;
        private readonly ITelegramClient _telegramClient;
        private readonly IUserMessageCache _userMessageCache;
        public event Func<IChatClient, IMessage, Task> MessageReceived = (_, _) => Task.CompletedTask;
        public event Func<IChatClient, IGuildUser, IGuildUser, Task> GuildMemberUpdated= (_, _, _) => Task.CompletedTask;
        public event Func<IChatClient, LogMessage, Task> Log = (_, _) => Task.CompletedTask;

        public TelegramEventProvider(
            ITelegramGuildFactory telegramGuildFactory,
            ITelegramUserMessageFactory telegramUserMessageFactory, 
            ITelegramGuildUserFactory telegramGuildUserFactory,
            ITelegramClient telegramClient,
            IUserMessageCache userMessageCache)
        {
            _telegramGuildFactory = telegramGuildFactory;
            _telegramUserMessageFactory = telegramUserMessageFactory;
            _telegramGuildUserFactory = telegramGuildUserFactory;
            _telegramClient = telegramClient;
            _userMessageCache = userMessageCache;
        }

        public async Task HandleInboundMessageAsync(Message apiMessage)
        {
            if (apiMessage.Chat.Type != ChatType.Private)
            {
                var telegramGuild = _telegramGuildFactory.Create(apiMessage.Chat);
                var telegramGuildUser = await _telegramGuildUserFactory.CreateOrNullAsync(telegramGuild, apiMessage.From.Id);
                if (telegramGuildUser != null)
                {
                    var message = _telegramUserMessageFactory.Create(apiMessage, telegramGuild, telegramGuildUser);
                    _userMessageCache.AddUserMessage(message, telegramGuild.Id);
                    await MessageReceived(_telegramClient, message);
                }
            }
        }
    }
}
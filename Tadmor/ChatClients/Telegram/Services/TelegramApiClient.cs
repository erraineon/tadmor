using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tadmor.ChatClients.Telegram.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Requests;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;
using File = Telegram.Bot.Types.File;

namespace Tadmor.ChatClients.Telegram.Services
{
    public class TelegramApiClient : ITelegramApiClient
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<TelegramApiClient> _logger;

        public TelegramApiClient(ILogger<TelegramApiClient> logger, ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        private async void FireMessageReceivedSafe(Message message)
        {
            try
            {
                await MessageReceivedAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"error when receiving telegram message: {message.Text}");
            }
        }

        private void OnMessageReceived(object? sender, MessageEventArgs e)
        {
            FireMessageReceivedSafe(e.Message);
        }

        public event Func<Message, Task> MessageReceivedAsync = _ => Task.CompletedTask;
        public async Task<TResponse> MakeRequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken())
        {
            var response = await _telegramBotClient.MakeRequestAsync(request, cancellationToken);
            if (response is Message responseMessage)
            {
                FireMessageReceivedSafe(responseMessage);
            }
            return response;
        }

        public Task<bool> TestApiAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.TestApiAsync(cancellationToken);
        }

        public void StartReceiving(UpdateType[]? allowedUpdates = null, CancellationToken cancellationToken = new CancellationToken())
        {
            _telegramBotClient.OnMessage += OnMessageReceived;
            _telegramBotClient.StartReceiving(allowedUpdates, cancellationToken);
        }

        public void StopReceiving()
        {
            _telegramBotClient.StopReceiving();
            _telegramBotClient.OnMessage -= OnMessageReceived;
        }

        public Task<Update[]> GetUpdatesAsync(
            int offset = 0,
            int limit = 0,
            int timeout = 0,
            IEnumerable<UpdateType> allowedUpdates = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetUpdatesAsync(offset, limit, timeout, allowedUpdates, cancellationToken);
        }

        public Task SetWebhookAsync(
            string url,
            InputFileStream certificate = null,
            int maxConnections = 0,
            IEnumerable<UpdateType> allowedUpdates = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetWebhookAsync(url, certificate, maxConnections, allowedUpdates, cancellationToken);
        }

        public Task DeleteWebhookAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.DeleteWebhookAsync(cancellationToken);
        }

        public Task<WebhookInfo> GetWebhookInfoAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetWebhookInfoAsync(cancellationToken);
        }

        public Task<User> GetMeAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetMeAsync(cancellationToken);
        }

        public Task<Message> SendTextMessageAsync(
            ChatId chatId,
            string text,
            ParseMode parseMode = ParseMode.Default,
            bool disableWebPagePreview = false,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendTextMessageAsync(chatId, text, parseMode, disableWebPagePreview, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> ForwardMessageAsync(
            ChatId chatId,
            ChatId fromChatId,
            int messageId,
            bool disableNotification = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.ForwardMessageAsync(chatId, fromChatId, messageId, disableNotification, cancellationToken);
        }

        public Task<Message> SendPhotoAsync(
            ChatId chatId,
            InputOnlineFile photo,
            string caption = null,
            ParseMode parseMode = ParseMode.Default,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendPhotoAsync(chatId, photo, caption, parseMode, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> SendAudioAsync(
            ChatId chatId,
            InputOnlineFile audio,
            string caption = null,
            ParseMode parseMode = ParseMode.Default,
            int duration = 0,
            string performer = null,
            string title = null,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            InputMedia thumb = null)
        {
            return _telegramBotClient.SendAudioAsync(chatId, audio, caption, parseMode, duration, performer, title, disableNotification, replyToMessageId, replyMarkup, cancellationToken, thumb);
        }

        public Task<Message> SendDocumentAsync(
            ChatId chatId,
            InputOnlineFile document,
            string caption = null,
            ParseMode parseMode = ParseMode.Default,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            InputMedia thumb = null)
        {
            return _telegramBotClient.SendDocumentAsync(chatId, document, caption, parseMode, disableNotification, replyToMessageId, replyMarkup, cancellationToken, thumb);
        }

        public Task<Message> SendStickerAsync(
            ChatId chatId,
            InputOnlineFile sticker,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendStickerAsync(chatId, sticker, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> SendVideoAsync(
            ChatId chatId,
            InputOnlineFile video,
            int duration = 0,
            int width = 0,
            int height = 0,
            string caption = null,
            ParseMode parseMode = ParseMode.Default,
            bool supportsStreaming = false,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            InputMedia thumb = null)
        {
            return _telegramBotClient.SendVideoAsync(chatId, video, duration, width, height, caption, parseMode, supportsStreaming, disableNotification, replyToMessageId, replyMarkup, cancellationToken, thumb);
        }

        public Task<Message> SendAnimationAsync(
            ChatId chatId,
            InputOnlineFile animation,
            int duration = 0,
            int width = 0,
            int height = 0,
            InputMedia thumb = null,
            string caption = null,
            ParseMode parseMode = ParseMode.Default,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendAnimationAsync(chatId, animation, duration, width, height, thumb, caption, parseMode, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> SendVoiceAsync(
            ChatId chatId,
            InputOnlineFile voice,
            string caption = null,
            ParseMode parseMode = ParseMode.Default,
            int duration = 0,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendVoiceAsync(chatId, voice, caption, parseMode, duration, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> SendVideoNoteAsync(
            ChatId chatId,
            InputTelegramFile videoNote,
            int duration = 0,
            int length = 0,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            InputMedia thumb = null)
        {
            return _telegramBotClient.SendVideoNoteAsync(chatId, videoNote, duration, length, disableNotification, replyToMessageId, replyMarkup, cancellationToken, thumb);
        }

        public Task<Message[]> SendMediaGroupAsync(
            ChatId chatId,
            IEnumerable<InputMediaBase> media,
            bool disableNotification = false,
            int replyToMessageId = 0,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendMediaGroupAsync(chatId, media, disableNotification, replyToMessageId, cancellationToken);
        }

        public Task<Message[]> SendMediaGroupAsync(
            IEnumerable<IAlbumInputMedia> inputMedia,
            ChatId chatId,
            bool disableNotification = false,
            int replyToMessageId = 0,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendMediaGroupAsync(inputMedia, chatId, disableNotification, replyToMessageId, cancellationToken);
        }

        public Task<Message> SendLocationAsync(
            ChatId chatId,
            float latitude,
            float longitude,
            int livePeriod = 0,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendLocationAsync(chatId, latitude, longitude, livePeriod, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> SendVenueAsync(
            ChatId chatId,
            float latitude,
            float longitude,
            string title,
            string address,
            string foursquareId = null,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            string foursquareType = null)
        {
            return _telegramBotClient.SendVenueAsync(chatId, latitude, longitude, title, address, foursquareId, disableNotification, replyToMessageId, replyMarkup, cancellationToken, foursquareType);
        }

        public Task<Message> SendContactAsync(
            ChatId chatId,
            string phoneNumber,
            string firstName,
            string lastName = null,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            string vCard = null)
        {
            return _telegramBotClient.SendContactAsync(chatId, phoneNumber, firstName, lastName, disableNotification, replyToMessageId, replyMarkup, cancellationToken, vCard);
        }

        public Task<Message> SendPollAsync(
            ChatId chatId,
            string question,
            IEnumerable<string> options,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            bool? isAnonymous = null,
            PollType? type = null,
            bool? allowsMultipleAnswers = null,
            int? correctOptionId = null,
            bool? isClosed = null,
            string explanation = null,
            ParseMode explanationParseMode = ParseMode.Default,
            int? openPeriod = null,
            DateTime? closeDate = null)
        {
            return _telegramBotClient.SendPollAsync(chatId, question, options, disableNotification, replyToMessageId, replyMarkup, cancellationToken, isAnonymous, type, allowsMultipleAnswers, correctOptionId, isClosed, explanation, explanationParseMode, openPeriod, closeDate);
        }

        public Task<Message> SendDiceAsync(
            ChatId chatId,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            Emoji? emoji = null)
        {
            return _telegramBotClient.SendDiceAsync(chatId, disableNotification, replyToMessageId, replyMarkup, cancellationToken, emoji);
        }

        public Task SendChatActionAsync(
            ChatId chatId,
            ChatAction chatAction,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendChatActionAsync(chatId, chatAction, cancellationToken);
        }

        public Task<UserProfilePhotos> GetUserProfilePhotosAsync(
            int userId,
            int offset = 0,
            int limit = 0,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetUserProfilePhotosAsync(userId, offset, limit, cancellationToken);
        }

        public Task<File> GetFileAsync(string fileId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetFileAsync(fileId, cancellationToken);
        }

        public Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.DownloadFileAsync(filePath, cancellationToken);
        }

        public Task DownloadFileAsync(
            string filePath,
            Stream destination,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.DownloadFileAsync(filePath, destination, cancellationToken);
        }

        public Task<File> GetInfoAndDownloadFileAsync(
            string fileId,
            Stream destination,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetInfoAndDownloadFileAsync(fileId, destination, cancellationToken);
        }

        public Task KickChatMemberAsync(
            ChatId chatId,
            int userId,
            DateTime untilDate = new DateTime(),
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.KickChatMemberAsync(chatId, userId, untilDate, cancellationToken);
        }

        public Task LeaveChatAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.LeaveChatAsync(chatId, cancellationToken);
        }

        public Task UnbanChatMemberAsync(ChatId chatId, int userId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.UnbanChatMemberAsync(chatId, userId, cancellationToken);
        }

        public Task<Chat> GetChatAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetChatAsync(chatId, cancellationToken);
        }

        public Task<ChatMember[]> GetChatAdministratorsAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetChatAdministratorsAsync(chatId, cancellationToken);
        }

        public Task<int> GetChatMembersCountAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetChatMembersCountAsync(chatId, cancellationToken);
        }

        public Task<ChatMember> GetChatMemberAsync(ChatId chatId, int userId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetChatMemberAsync(chatId, userId, cancellationToken);
        }

        public Task AnswerCallbackQueryAsync(
            string callbackQueryId,
            string text = null,
            bool showAlert = false,
            string url = null,
            int cacheTime = 0,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.AnswerCallbackQueryAsync(callbackQueryId, text, showAlert, url, cacheTime, cancellationToken);
        }

        public Task RestrictChatMemberAsync(
            ChatId chatId,
            int userId,
            ChatPermissions permissions,
            DateTime untilDate = new DateTime(),
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate, cancellationToken);
        }

        public Task PromoteChatMemberAsync(
            ChatId chatId,
            int userId,
            bool? canChangeInfo = null,
            bool? canPostMessages = null,
            bool? canEditMessages = null,
            bool? canDeleteMessages = null,
            bool? canInviteUsers = null,
            bool? canRestrictMembers = null,
            bool? canPinMessages = null,
            bool? canPromoteMembers = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.PromoteChatMemberAsync(chatId, userId, canChangeInfo, canPostMessages, canEditMessages, canDeleteMessages, canInviteUsers, canRestrictMembers, canPinMessages, canPromoteMembers, cancellationToken);
        }

        public Task SetChatAdministratorCustomTitleAsync(
            ChatId chatId,
            int userId,
            string customTitle,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetChatAdministratorCustomTitleAsync(chatId, userId, customTitle, cancellationToken);
        }

        public Task SetChatPermissionsAsync(
            ChatId chatId,
            ChatPermissions permissions,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetChatPermissionsAsync(chatId, permissions, cancellationToken);
        }

        public Task<BotCommand[]> GetMyCommandsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetMyCommandsAsync(cancellationToken);
        }

        public Task SetMyCommandsAsync(IEnumerable<BotCommand> commands, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetMyCommandsAsync(commands, cancellationToken);
        }

        public Task<Message> EditMessageTextAsync(
            ChatId chatId,
            int messageId,
            string text,
            ParseMode parseMode = ParseMode.Default,
            bool disableWebPagePreview = false,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.EditMessageTextAsync(chatId, messageId, text, parseMode, disableWebPagePreview, replyMarkup, cancellationToken);
        }

        public Task EditMessageTextAsync(
            string inlineMessageId,
            string text,
            ParseMode parseMode = ParseMode.Default,
            bool disableWebPagePreview = false,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.EditMessageTextAsync(inlineMessageId, text, parseMode, disableWebPagePreview, replyMarkup, cancellationToken);
        }

        public Task<Message> StopMessageLiveLocationAsync(
            ChatId chatId,
            int messageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.StopMessageLiveLocationAsync(chatId, messageId, replyMarkup, cancellationToken);
        }

        public Task StopMessageLiveLocationAsync(
            string inlineMessageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.StopMessageLiveLocationAsync(inlineMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageCaptionAsync(
            ChatId chatId,
            int messageId,
            string caption,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            ParseMode parseMode = ParseMode.Default)
        {
            return _telegramBotClient.EditMessageCaptionAsync(chatId, messageId, caption, replyMarkup, cancellationToken, parseMode);
        }

        public Task EditMessageCaptionAsync(
            string inlineMessageId,
            string caption,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            ParseMode parseMode = ParseMode.Default)
        {
            return _telegramBotClient.EditMessageCaptionAsync(inlineMessageId, caption, replyMarkup, cancellationToken, parseMode);
        }

        public Task<Message> EditMessageMediaAsync(
            ChatId chatId,
            int messageId,
            InputMediaBase media,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.EditMessageMediaAsync(chatId, messageId, media, replyMarkup, cancellationToken);
        }

        public Task EditMessageMediaAsync(
            string inlineMessageId,
            InputMediaBase media,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.EditMessageMediaAsync(inlineMessageId, media, replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageReplyMarkupAsync(
            ChatId chatId,
            int messageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.EditMessageReplyMarkupAsync(chatId, messageId, replyMarkup, cancellationToken);
        }

        public Task EditMessageReplyMarkupAsync(
            string inlineMessageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.EditMessageReplyMarkupAsync(inlineMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageLiveLocationAsync(
            ChatId chatId,
            int messageId,
            float latitude,
            float longitude,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.EditMessageLiveLocationAsync(chatId, messageId, latitude, longitude, replyMarkup, cancellationToken);
        }

        public Task EditMessageLiveLocationAsync(
            string inlineMessageId,
            float latitude,
            float longitude,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.EditMessageLiveLocationAsync(inlineMessageId, latitude, longitude, replyMarkup, cancellationToken);
        }

        public Task<Poll> StopPollAsync(
            ChatId chatId,
            int messageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.StopPollAsync(chatId, messageId, replyMarkup, cancellationToken);
        }

        public Task DeleteMessageAsync(ChatId chatId, int messageId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.DeleteMessageAsync(chatId, messageId, cancellationToken);
        }

        public Task AnswerInlineQueryAsync(
            string inlineQueryId,
            IEnumerable<InlineQueryResultBase> results,
            int? cacheTime = null,
            bool isPersonal = false,
            string nextOffset = null,
            string switchPmText = null,
            string switchPmParameter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.AnswerInlineQueryAsync(inlineQueryId, results, cacheTime, isPersonal, nextOffset, switchPmText, switchPmParameter, cancellationToken);
        }

        public Task<Message> SendInvoiceAsync(
            int chatId,
            string title,
            string description,
            string payload,
            string providerToken,
            string startParameter,
            string currency,
            IEnumerable<LabeledPrice> prices,
            string providerData = null,
            string photoUrl = null,
            int photoSize = 0,
            int photoWidth = 0,
            int photoHeight = 0,
            bool needName = false,
            bool needPhoneNumber = false,
            bool needEmail = false,
            bool needShippingAddress = false,
            bool isFlexible = false,
            bool disableNotification = false,
            int replyToMessageId = 0,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken(),
            bool sendPhoneNumberToProvider = false,
            bool sendEmailToProvider = false)
        {
            return _telegramBotClient.SendInvoiceAsync(chatId, title, description, payload, providerToken, startParameter, currency, prices, providerData, photoUrl, photoSize, photoWidth, photoHeight, needName, needPhoneNumber, needEmail, needShippingAddress, isFlexible, disableNotification, replyToMessageId, replyMarkup, cancellationToken, sendPhoneNumberToProvider, sendEmailToProvider);
        }

        public Task AnswerShippingQueryAsync(
            string shippingQueryId,
            IEnumerable<ShippingOption> shippingOptions,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.AnswerShippingQueryAsync(shippingQueryId, shippingOptions, cancellationToken);
        }

        public Task AnswerShippingQueryAsync(
            string shippingQueryId,
            string errorMessage,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.AnswerShippingQueryAsync(shippingQueryId, errorMessage, cancellationToken);
        }

        public Task AnswerPreCheckoutQueryAsync(
            string preCheckoutQueryId,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.AnswerPreCheckoutQueryAsync(preCheckoutQueryId, cancellationToken);
        }

        public Task AnswerPreCheckoutQueryAsync(
            string preCheckoutQueryId,
            string errorMessage,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.AnswerPreCheckoutQueryAsync(preCheckoutQueryId, errorMessage, cancellationToken);
        }

        public Task<Message> SendGameAsync(
            long chatId,
            string gameShortName,
            bool disableNotification = false,
            int replyToMessageId = 0,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SendGameAsync(chatId, gameShortName, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> SetGameScoreAsync(
            int userId,
            int score,
            long chatId,
            int messageId,
            bool force = false,
            bool disableEditMessage = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetGameScoreAsync(userId, score, chatId, messageId, force, disableEditMessage, cancellationToken);
        }

        public Task SetGameScoreAsync(
            int userId,
            int score,
            string inlineMessageId,
            bool force = false,
            bool disableEditMessage = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetGameScoreAsync(userId, score, inlineMessageId, force, disableEditMessage, cancellationToken);
        }

        public Task<GameHighScore[]> GetGameHighScoresAsync(
            int userId,
            long chatId,
            int messageId,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetGameHighScoresAsync(userId, chatId, messageId, cancellationToken);
        }

        public Task<GameHighScore[]> GetGameHighScoresAsync(
            int userId,
            string inlineMessageId,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetGameHighScoresAsync(userId, inlineMessageId, cancellationToken);
        }

        public Task<StickerSet> GetStickerSetAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.GetStickerSetAsync(name, cancellationToken);
        }

        public Task<File> UploadStickerFileAsync(
            int userId,
            InputFileStream pngSticker,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.UploadStickerFileAsync(userId, pngSticker, cancellationToken);
        }

        public Task CreateNewStickerSetAsync(
            int userId,
            string name,
            string title,
            InputOnlineFile pngSticker,
            string emojis,
            bool isMasks = false,
            MaskPosition maskPosition = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.CreateNewStickerSetAsync(userId, name, title, pngSticker, emojis, isMasks, maskPosition, cancellationToken);
        }

        public Task AddStickerToSetAsync(
            int userId,
            string name,
            InputOnlineFile pngSticker,
            string emojis,
            MaskPosition maskPosition = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.AddStickerToSetAsync(userId, name, pngSticker, emojis, maskPosition, cancellationToken);
        }

        public Task CreateNewAnimatedStickerSetAsync(
            int userId,
            string name,
            string title,
            InputFileStream tgsSticker,
            string emojis,
            bool isMasks = false,
            MaskPosition maskPosition = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.CreateNewAnimatedStickerSetAsync(userId, name, title, tgsSticker, emojis, isMasks, maskPosition, cancellationToken);
        }

        public Task AddAnimatedStickerToSetAsync(
            int userId,
            string name,
            InputFileStream tgsSticker,
            string emojis,
            MaskPosition maskPosition = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.AddAnimatedStickerToSetAsync(userId, name, tgsSticker, emojis, maskPosition, cancellationToken);
        }

        public Task SetStickerPositionInSetAsync(
            string sticker,
            int position,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetStickerPositionInSetAsync(sticker, position, cancellationToken);
        }

        public Task DeleteStickerFromSetAsync(string sticker, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.DeleteStickerFromSetAsync(sticker, cancellationToken);
        }

        public Task SetStickerSetThumbAsync(
            string name,
            int userId,
            InputOnlineFile thumb = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetStickerSetThumbAsync(name, userId, thumb, cancellationToken);
        }

        public Task<string> ExportChatInviteLinkAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.ExportChatInviteLinkAsync(chatId, cancellationToken);
        }

        public Task SetChatPhotoAsync(
            ChatId chatId,
            InputFileStream photo,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetChatPhotoAsync(chatId, photo, cancellationToken);
        }

        public Task DeleteChatPhotoAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.DeleteChatPhotoAsync(chatId, cancellationToken);
        }

        public Task SetChatTitleAsync(ChatId chatId, string title, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetChatTitleAsync(chatId, title, cancellationToken);
        }

        public Task SetChatDescriptionAsync(
            ChatId chatId,
            string description = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetChatDescriptionAsync(chatId, description, cancellationToken);
        }

        public Task PinChatMessageAsync(
            ChatId chatId,
            int messageId,
            bool disableNotification = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.PinChatMessageAsync(chatId, messageId, disableNotification, cancellationToken);
        }

        public Task UnpinChatMessageAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.UnpinChatMessageAsync(chatId, cancellationToken);
        }

        public Task SetChatStickerSetAsync(
            ChatId chatId,
            string stickerSetName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.SetChatStickerSetAsync(chatId, stickerSetName, cancellationToken);
        }

        public Task DeleteChatStickerSetAsync(ChatId chatId, CancellationToken cancellationToken = new CancellationToken())
        {
            return _telegramBotClient.DeleteChatStickerSetAsync(chatId, cancellationToken);
        }

        public int BotId => _telegramBotClient.BotId;

        public TimeSpan Timeout
        {
            get => _telegramBotClient.Timeout;
            set => _telegramBotClient.Timeout = value;
        }

        public bool IsReceiving => _telegramBotClient.IsReceiving;

        public int MessageOffset
        {
            get => _telegramBotClient.MessageOffset;
            set => _telegramBotClient.MessageOffset = value;
        }

        public event EventHandler<ApiRequestEventArgs> MakingApiRequest
        {
            add => _telegramBotClient.MakingApiRequest += value;
            remove => _telegramBotClient.MakingApiRequest -= value;
        }

        public event EventHandler<ApiResponseEventArgs> ApiResponseReceived
        {
            add => _telegramBotClient.ApiResponseReceived += value;
            remove => _telegramBotClient.ApiResponseReceived -= value;
        }

        public event EventHandler<UpdateEventArgs> OnUpdate
        {
            add => _telegramBotClient.OnUpdate += value;
            remove => _telegramBotClient.OnUpdate -= value;
        }

        public event EventHandler<MessageEventArgs> OnMessage
        {
            add => _telegramBotClient.OnMessage += value;
            remove => _telegramBotClient.OnMessage -= value;
        }

        public event EventHandler<MessageEventArgs> OnMessageEdited
        {
            add => _telegramBotClient.OnMessageEdited += value;
            remove => _telegramBotClient.OnMessageEdited -= value;
        }

        public event EventHandler<InlineQueryEventArgs> OnInlineQuery
        {
            add => _telegramBotClient.OnInlineQuery += value;
            remove => _telegramBotClient.OnInlineQuery -= value;
        }

        public event EventHandler<ChosenInlineResultEventArgs> OnInlineResultChosen
        {
            add => _telegramBotClient.OnInlineResultChosen += value;
            remove => _telegramBotClient.OnInlineResultChosen -= value;
        }

        public event EventHandler<CallbackQueryEventArgs> OnCallbackQuery
        {
            add => _telegramBotClient.OnCallbackQuery += value;
            remove => _telegramBotClient.OnCallbackQuery -= value;
        }

        public event EventHandler<ReceiveErrorEventArgs> OnReceiveError
        {
            add => _telegramBotClient.OnReceiveError += value;
            remove => _telegramBotClient.OnReceiveError -= value;
        }

        public event EventHandler<ReceiveGeneralErrorEventArgs> OnReceiveGeneralError
        {
            add => _telegramBotClient.OnReceiveGeneralError += value;
            remove => _telegramBotClient.OnReceiveGeneralError -= value;
        }
    }
}

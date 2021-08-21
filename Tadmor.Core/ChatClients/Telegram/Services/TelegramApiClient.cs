using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;
using File = Telegram.Bot.Types.File;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramApiClient : ITelegramApiClient
    {
        private readonly ILogger<TelegramApiClient> _logger;
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramApiClient(ILogger<TelegramApiClient> logger, ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        public event Func<Message, Task> MessageReceivedAsync = _ => Task.CompletedTask;

        public async Task<TResponse> MakeRequestAsync<TResponse>(IRequest<TResponse> request,
            CancellationToken cancellationToken = new())
        {
            var response = await _telegramBotClient.MakeRequestAsync(request, cancellationToken);
            if (response is Message responseMessage) FireMessageReceivedSafe(responseMessage);
            return response;
        }

        public Task<bool> TestApiAsync(CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.TestApiAsync(cancellationToken);
        }

        public void StartReceiving(UpdateType[]? allowedUpdates = null, CancellationToken cancellationToken = new())
        {
            _telegramBotClient.OnMessage += OnMessageReceived;
            _telegramBotClient.StartReceiving(allowedUpdates, cancellationToken);
        }

        public void StopReceiving()
        {
            try
            {
                _telegramBotClient.StopReceiving();
            }
            catch
            {
                // TODO: this stack is being deprecated, replace with whatever's the new thing.
                // It throws when it's being closed.
            }
            _telegramBotClient.OnMessage -= OnMessageReceived;
        }

        public Task<Update[]> GetUpdatesAsync(
            int offset = 0,
            int limit = 0,
            int timeout = 0,
            IEnumerable<UpdateType> allowedUpdates = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetUpdatesAsync(offset, limit, timeout, allowedUpdates, cancellationToken);
        }

        public Task SetWebhookAsync(string url, InputFileStream certificate = null, string ipAddress = null,
            int maxConnections = 0,
            IEnumerable<UpdateType> allowedUpdates = null, bool dropPendingUpdates = false,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetWebhookAsync(url, certificate, ipAddress, maxConnections, allowedUpdates,
                dropPendingUpdates, cancellationToken);
        }

        public Task DeleteWebhookAsync(bool dropPendingUpdates = false, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.DeleteWebhookAsync(dropPendingUpdates, cancellationToken);
        }

        public Task<WebhookInfo> GetWebhookInfoAsync(CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetWebhookInfoAsync(cancellationToken);
        }

        public Task<User> GetMeAsync(CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetMeAsync(cancellationToken);
        }

        public Task LogOutAsync(CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.LogOutAsync(cancellationToken);
        }

        public Task CloseAsync(CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.CloseAsync(cancellationToken);
        }

        public async Task<Message> SendTextMessageAsync(ChatId chatId, string text,
            ParseMode parseMode = ParseMode.Default, IEnumerable<MessageEntity> entities = null,
            bool disableWebPagePreview = false, bool disableNotification = false, int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            var apiMessage = await _telegramBotClient.SendTextMessageAsync(chatId, text, parseMode, entities,
                disableWebPagePreview, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup,
                cancellationToken);
            FireMessageReceivedSafe(apiMessage);
            return apiMessage;
        }

        public Task<Message> ForwardMessageAsync(
            ChatId chatId,
            ChatId fromChatId,
            int messageId,
            bool disableNotification = false,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.ForwardMessageAsync(chatId, fromChatId, messageId, disableNotification,
                cancellationToken);
        }

        public Task<MessageId> CopyMessageAsync(ChatId chatId, ChatId fromChatId, int messageId, string caption = null,
            ParseMode parseMode = ParseMode.Default, IEnumerable<MessageEntity> captionEntities = null,
            int replyToMessageId = 0,
            bool disableNotification = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.CopyMessageAsync(chatId, fromChatId, messageId, caption, parseMode,
                captionEntities, replyToMessageId, disableNotification, replyMarkup, cancellationToken);
        }

        public async Task<Message> SendPhotoAsync(ChatId chatId, InputOnlineFile photo, string caption = null,
            ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity> captionEntities = null, bool disableNotification = false,
            int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            var apiMessage = await _telegramBotClient.SendPhotoAsync(chatId, photo, caption, parseMode, captionEntities,
                disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup, cancellationToken);
            FireMessageReceivedSafe(apiMessage);
            FireMessageReceivedSafe(apiMessage);
            return apiMessage;
        }

        public Task<Message> SendAudioAsync(ChatId chatId, InputOnlineFile audio, string caption = null,
            ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity> captionEntities = null, int duration = 0, string performer = null,
            string title = null,
            InputMedia thumb = null, bool disableNotification = false, int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendAudioAsync(chatId, audio, caption, parseMode, captionEntities, duration,
                performer, title, thumb, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup,
                cancellationToken);
        }

        public Task<Message> SendDocumentAsync(ChatId chatId, InputOnlineFile document, InputMedia thumb = null,
            string caption = null,
            ParseMode parseMode = ParseMode.Default, IEnumerable<MessageEntity> captionEntities = null,
            bool disableContentTypeDetection = false,
            bool disableNotification = false, int replyToMessageId = 0, bool allowSendingWithoutReply = false,
            IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendDocumentAsync(chatId, document, thumb, caption, parseMode, captionEntities,
                disableContentTypeDetection, disableNotification, replyToMessageId, allowSendingWithoutReply,
                replyMarkup, cancellationToken);
        }

        public async Task<Message> SendStickerAsync(ChatId chatId, InputOnlineFile sticker,
            bool disableNotification = false,
            int replyToMessageId = 0, bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            var apiMessage = await _telegramBotClient.SendStickerAsync(chatId, sticker, disableNotification,
                replyToMessageId, allowSendingWithoutReply, replyMarkup, cancellationToken);
            FireMessageReceivedSafe(apiMessage);
            return apiMessage;
        }

        public Task<Message> SendVideoAsync(ChatId chatId, InputOnlineFile video, int duration = 0, int width = 0,
            int height = 0,
            InputMedia thumb = null, string caption = null, ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity> captionEntities = null,
            bool supportsStreaming = false, bool disableNotification = false, int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendVideoAsync(chatId, video, duration, width, height, thumb, caption, parseMode,
                captionEntities, supportsStreaming, disableNotification, replyToMessageId, allowSendingWithoutReply,
                replyMarkup, cancellationToken);
        }

        public async Task<Message> SendAnimationAsync(ChatId chatId, InputOnlineFile animation, int duration = 0,
            int width = 0, int height = 0,
            InputMedia thumb = null, string caption = null, ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity> captionEntities = null,
            bool disableNotification = false, int replyToMessageId = 0, bool allowSendingWithoutReply = false,
            IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = new())
        {
            var apiMessage = await _telegramBotClient.SendAnimationAsync(chatId, animation, duration, width, height,
                thumb, caption, parseMode, captionEntities, disableNotification, replyToMessageId,
                allowSendingWithoutReply, replyMarkup, cancellationToken);
            FireMessageReceivedSafe(apiMessage);
            return apiMessage;
        }

        public Task<Message> SendVoiceAsync(ChatId chatId, InputOnlineFile voice, string caption = null,
            ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity> captionEntities = null, int duration = 0, bool disableNotification = false,
            int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendVoiceAsync(chatId, voice, caption, parseMode, captionEntities, duration,
                disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup, cancellationToken);
        }

        public Task<Message> SendVideoNoteAsync(ChatId chatId, InputTelegramFile videoNote, int duration = 0,
            int length = 0,
            InputMedia thumb = null, bool disableNotification = false, int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendVideoNoteAsync(chatId, videoNote, duration, length, thumb,
                disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup, cancellationToken);
        }

        public Task<Message[]> SendMediaGroupAsync(ChatId chatId, IEnumerable<IAlbumInputMedia> media,
            bool disableNotification = false, int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendMediaGroupAsync(chatId, media, disableNotification, replyToMessageId,
                allowSendingWithoutReply, cancellationToken);
        }

        public Task<Message> SendLocationAsync(ChatId chatId, float latitude, float longitude, int livePeriod = 0,
            int heading = 0,
            int proximityAlertRadius = 0, bool disableNotification = false, int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendLocationAsync(chatId, latitude, longitude, livePeriod, heading,
                proximityAlertRadius, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup,
                cancellationToken);
        }

        public Task<Message> SendVenueAsync(ChatId chatId, float latitude, float longitude, string title,
            string address,
            string foursquareId = null, string foursquareType = null, string googlePlaceId = null,
            string googlePlaceType = null, bool disableNotification = false, int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendVenueAsync(chatId, latitude, longitude, title, address, foursquareId,
                foursquareType, googlePlaceId, googlePlaceType, disableNotification, replyToMessageId,
                allowSendingWithoutReply, replyMarkup, cancellationToken);
        }

        public Task<Message> SendContactAsync(ChatId chatId, string phoneNumber, string firstName,
            string lastName = null, string vCard = null,
            bool disableNotification = false, int replyToMessageId = 0, bool allowSendingWithoutReply = false,
            IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendContactAsync(chatId, phoneNumber, firstName, lastName, vCard,
                disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup, cancellationToken);
        }

        public Task<Message> SendPollAsync(ChatId chatId, string question, IEnumerable<string> options,
            bool? isAnonymous = null, PollType? type = null,
            bool? allowsMultipleAnswers = null, int? correctOptionId = null, string explanation = null,
            ParseMode explanationParseMode = ParseMode.Default, IEnumerable<MessageEntity> explanationEntities = null,
            int? openPeriod = null,
            DateTime? closeDate = null, bool? isClosed = null, bool disableNotification = false,
            int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendPollAsync(chatId, question, options, isAnonymous, type, allowsMultipleAnswers,
                correctOptionId, explanation, explanationParseMode, explanationEntities, openPeriod, closeDate,
                isClosed, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup,
                cancellationToken);
        }

        public Task<Message> SendDiceAsync(ChatId chatId, Emoji? emoji = null, bool disableNotification = false,
            int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendDiceAsync(chatId, emoji, disableNotification, replyToMessageId,
                allowSendingWithoutReply, replyMarkup, cancellationToken);
        }

        public Task SendChatActionAsync(
            ChatId chatId,
            ChatAction chatAction,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendChatActionAsync(chatId, chatAction, cancellationToken);
        }

        public Task<UserProfilePhotos> GetUserProfilePhotosAsync(long userId, int offset = 0, int limit = 0,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetUserProfilePhotosAsync(userId, offset, limit, cancellationToken);
        }

        public Task<File> GetFileAsync(string fileId, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetFileAsync(fileId, cancellationToken);
        }

        public Task DownloadFileAsync(
            string filePath,
            Stream destination,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.DownloadFileAsync(filePath, destination, cancellationToken);
        }

        public Task<File> GetInfoAndDownloadFileAsync(
            string fileId,
            Stream destination,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetInfoAndDownloadFileAsync(fileId, destination, cancellationToken);
        }

        public Task KickChatMemberAsync(ChatId chatId, long userId, DateTime untilDate = new(),
            bool? revokeMessages = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.KickChatMemberAsync(chatId, userId, untilDate, revokeMessages, cancellationToken);
        }

        public Task LeaveChatAsync(ChatId chatId, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.LeaveChatAsync(chatId, cancellationToken);
        }

        public Task UnbanChatMemberAsync(ChatId chatId, long userId, bool onlyIfBanned = false,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.UnbanChatMemberAsync(chatId, userId, onlyIfBanned, cancellationToken);
        }

        public Task<Chat> GetChatAsync(ChatId chatId, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetChatAsync(chatId, cancellationToken);
        }

        public Task<ChatMember[]> GetChatAdministratorsAsync(ChatId chatId, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetChatAdministratorsAsync(chatId, cancellationToken);
        }

        public Task<int> GetChatMembersCountAsync(ChatId chatId, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetChatMembersCountAsync(chatId, cancellationToken);
        }

        public Task<ChatMember> GetChatMemberAsync(ChatId chatId, long userId,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetChatMemberAsync(chatId, userId, cancellationToken);
        }

        public Task AnswerCallbackQueryAsync(
            string callbackQueryId,
            string text = null,
            bool showAlert = false,
            string url = null,
            int cacheTime = 0,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.AnswerCallbackQueryAsync(callbackQueryId, text, showAlert, url, cacheTime,
                cancellationToken);
        }

        public Task RestrictChatMemberAsync(ChatId chatId, long userId, ChatPermissions permissions,
            DateTime untilDate = new(), CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate,
                cancellationToken);
        }

        public Task PromoteChatMemberAsync(ChatId chatId, long userId, bool? isAnonymous = null,
            bool? canManageChat = null,
            bool? canChangeInfo = null, bool? canPostMessages = null, bool? canEditMessages = null,
            bool? canDeleteMessages = null, bool? canManageVoiceChats = null, bool? canInviteUsers = null,
            bool? canRestrictMembers = null, bool? canPinMessages = null, bool? canPromoteMembers = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.PromoteChatMemberAsync(chatId, userId, isAnonymous, canManageChat, canChangeInfo,
                canPostMessages, canEditMessages, canDeleteMessages, canManageVoiceChats, canInviteUsers,
                canRestrictMembers, canPinMessages, canPromoteMembers, cancellationToken);
        }

        public Task SetChatAdministratorCustomTitleAsync(ChatId chatId, long userId, string customTitle,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetChatAdministratorCustomTitleAsync(chatId, userId, customTitle,
                cancellationToken);
        }

        public Task SetChatPermissionsAsync(
            ChatId chatId,
            ChatPermissions permissions,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetChatPermissionsAsync(chatId, permissions, cancellationToken);
        }

        public Task<BotCommand[]> GetMyCommandsAsync(CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetMyCommandsAsync(cancellationToken);
        }

        public Task SetMyCommandsAsync(IEnumerable<BotCommand> commands, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetMyCommandsAsync(commands, cancellationToken);
        }

        public Task<Message> EditMessageTextAsync(ChatId chatId, int messageId, string text,
            ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity> entities = null, bool disableWebPagePreview = false,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageTextAsync(chatId, messageId, text, parseMode, entities,
                disableWebPagePreview, replyMarkup, cancellationToken);
        }

        public Task EditMessageTextAsync(string inlineMessageId, string text, ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity> entities = null, bool disableWebPagePreview = false,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageTextAsync(inlineMessageId, text, parseMode, entities,
                disableWebPagePreview, replyMarkup, cancellationToken);
        }

        public Task<Message> StopMessageLiveLocationAsync(
            ChatId chatId,
            int messageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.StopMessageLiveLocationAsync(chatId, messageId, replyMarkup, cancellationToken);
        }

        public Task StopMessageLiveLocationAsync(
            string inlineMessageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.StopMessageLiveLocationAsync(inlineMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageCaptionAsync(ChatId chatId, int messageId, string caption,
            ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity> captionEntities = null, InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageCaptionAsync(chatId, messageId, caption, parseMode, captionEntities,
                replyMarkup, cancellationToken);
        }

        public Task EditMessageCaptionAsync(string inlineMessageId, string caption,
            ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity> captionEntities = null, InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageCaptionAsync(inlineMessageId, caption, parseMode, captionEntities,
                replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageMediaAsync(
            ChatId chatId,
            int messageId,
            InputMediaBase media,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageMediaAsync(chatId, messageId, media, replyMarkup, cancellationToken);
        }

        public Task EditMessageMediaAsync(
            string inlineMessageId,
            InputMediaBase media,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageMediaAsync(inlineMessageId, media, replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageReplyMarkupAsync(
            ChatId chatId,
            int messageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageReplyMarkupAsync(chatId, messageId, replyMarkup, cancellationToken);
        }

        public Task EditMessageReplyMarkupAsync(
            string inlineMessageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageReplyMarkupAsync(inlineMessageId, replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageLiveLocationAsync(ChatId chatId, int messageId, float latitude, float longitude,
            float horizontalAccuracy = 0, int heading = 0, int proximityAlertRadius = 0,
            InlineKeyboardMarkup replyMarkup = null, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageLiveLocationAsync(chatId, messageId, latitude, longitude,
                horizontalAccuracy, heading, proximityAlertRadius, replyMarkup, cancellationToken);
        }

        public Task EditMessageLiveLocationAsync(string inlineMessageId, float latitude, float longitude,
            float horizontalAccuracy = 0,
            int heading = 0, int proximityAlertRadius = 0, InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditMessageLiveLocationAsync(inlineMessageId, latitude, longitude,
                horizontalAccuracy, heading, proximityAlertRadius, replyMarkup, cancellationToken);
        }

        public Task<Poll> StopPollAsync(
            ChatId chatId,
            int messageId,
            InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.StopPollAsync(chatId, messageId, replyMarkup, cancellationToken);
        }

        public Task DeleteMessageAsync(ChatId chatId, int messageId, CancellationToken cancellationToken = new())
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
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.AnswerInlineQueryAsync(inlineQueryId, results, cacheTime, isPersonal, nextOffset,
                switchPmText, switchPmParameter, cancellationToken);
        }

        public Task<Message> SendInvoiceAsync(long chatId, string title, string description, string payload,
            string providerToken,
            string currency, IEnumerable<LabeledPrice> prices, int maxTipAmount = 0, int[] suggestedTipAmounts = null,
            string startParameter = null, string providerData = null, string photoUrl = null, int photoSize = 0,
            int photoWidth = 0, int photoHeight = 0, bool needName = false, bool needPhoneNumber = false,
            bool needEmail = false, bool needShippingAddress = false, bool sendPhoneNumberToProvider = false,
            bool sendEmailToProvider = false, bool isFlexible = false, bool disableNotification = false,
            int replyToMessageId = 0, bool allowSendingWithoutReply = false, InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendInvoiceAsync(chatId, title, description, payload, providerToken, currency,
                prices, maxTipAmount, suggestedTipAmounts, startParameter, providerData, photoUrl, photoSize,
                photoWidth, photoHeight, needName, needPhoneNumber, needEmail, needShippingAddress,
                sendPhoneNumberToProvider, sendEmailToProvider, isFlexible, disableNotification, replyToMessageId,
                allowSendingWithoutReply, replyMarkup, cancellationToken);
        }

        public Task AnswerShippingQueryAsync(
            string shippingQueryId,
            IEnumerable<ShippingOption> shippingOptions,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.AnswerShippingQueryAsync(shippingQueryId, shippingOptions, cancellationToken);
        }

        public Task AnswerShippingQueryAsync(
            string shippingQueryId,
            string errorMessage,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.AnswerShippingQueryAsync(shippingQueryId, errorMessage, cancellationToken);
        }

        public Task AnswerPreCheckoutQueryAsync(
            string preCheckoutQueryId,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.AnswerPreCheckoutQueryAsync(preCheckoutQueryId, cancellationToken);
        }

        public Task AnswerPreCheckoutQueryAsync(
            string preCheckoutQueryId,
            string errorMessage,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.AnswerPreCheckoutQueryAsync(preCheckoutQueryId, errorMessage, cancellationToken);
        }

        public Task<Message> SendGameAsync(long chatId, string gameShortName, bool disableNotification = false,
            int replyToMessageId = 0,
            bool allowSendingWithoutReply = false, InlineKeyboardMarkup replyMarkup = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SendGameAsync(chatId, gameShortName, disableNotification, replyToMessageId,
                allowSendingWithoutReply, replyMarkup, cancellationToken);
        }

        public Task<Message> SetGameScoreAsync(long userId, int score, long chatId, int messageId, bool force = false,
            bool disableEditMessage = false, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetGameScoreAsync(userId, score, chatId, messageId, force, disableEditMessage,
                cancellationToken);
        }

        public Task SetGameScoreAsync(long userId, int score, string inlineMessageId, bool force = false,
            bool disableEditMessage = false, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetGameScoreAsync(userId, score, inlineMessageId, force, disableEditMessage,
                cancellationToken);
        }

        public Task<GameHighScore[]> GetGameHighScoresAsync(long userId, long chatId, int messageId,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetGameHighScoresAsync(userId, chatId, messageId, cancellationToken);
        }

        public Task<GameHighScore[]> GetGameHighScoresAsync(long userId, string inlineMessageId,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetGameHighScoresAsync(userId, inlineMessageId, cancellationToken);
        }

        public Task<StickerSet> GetStickerSetAsync(string name, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.GetStickerSetAsync(name, cancellationToken);
        }

        public Task<File> UploadStickerFileAsync(long userId, InputFileStream pngSticker,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.UploadStickerFileAsync(userId, pngSticker, cancellationToken);
        }

        public Task CreateNewStickerSetAsync(long userId, string name, string title, InputOnlineFile pngSticker,
            string emojis,
            bool isMasks = false, MaskPosition maskPosition = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.CreateNewStickerSetAsync(userId, name, title, pngSticker, emojis, isMasks,
                maskPosition, cancellationToken);
        }

        public Task AddStickerToSetAsync(long userId, string name, InputOnlineFile pngSticker, string emojis,
            MaskPosition maskPosition = null, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.AddStickerToSetAsync(userId, name, pngSticker, emojis, maskPosition,
                cancellationToken);
        }

        public Task CreateNewAnimatedStickerSetAsync(long userId, string name, string title, InputFileStream tgsSticker,
            string emojis,
            bool isMasks = false, MaskPosition maskPosition = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.CreateNewAnimatedStickerSetAsync(userId, name, title, tgsSticker, emojis, isMasks,
                maskPosition, cancellationToken);
        }

        public Task AddAnimatedStickerToSetAsync(long userId, string name, InputFileStream tgsSticker, string emojis,
            MaskPosition maskPosition = null, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.AddAnimatedStickerToSetAsync(userId, name, tgsSticker, emojis, maskPosition,
                cancellationToken);
        }

        public Task SetStickerPositionInSetAsync(
            string sticker,
            int position,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetStickerPositionInSetAsync(sticker, position, cancellationToken);
        }

        public Task DeleteStickerFromSetAsync(string sticker, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.DeleteStickerFromSetAsync(sticker, cancellationToken);
        }

        public Task SetStickerSetThumbAsync(string name, long userId, InputOnlineFile thumb = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetStickerSetThumbAsync(name, userId, thumb, cancellationToken);
        }

        public Task<string> ExportChatInviteLinkAsync(ChatId chatId, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.ExportChatInviteLinkAsync(chatId, cancellationToken);
        }

        public Task SetChatPhotoAsync(
            ChatId chatId,
            InputFileStream photo,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetChatPhotoAsync(chatId, photo, cancellationToken);
        }

        public Task DeleteChatPhotoAsync(ChatId chatId, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.DeleteChatPhotoAsync(chatId, cancellationToken);
        }

        public Task SetChatTitleAsync(ChatId chatId, string title, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetChatTitleAsync(chatId, title, cancellationToken);
        }

        public Task SetChatDescriptionAsync(
            ChatId chatId,
            string description = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetChatDescriptionAsync(chatId, description, cancellationToken);
        }

        public Task PinChatMessageAsync(
            ChatId chatId,
            int messageId,
            bool disableNotification = false,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.PinChatMessageAsync(chatId, messageId, disableNotification, cancellationToken);
        }

        public Task UnpinChatMessageAsync(ChatId chatId, int messageId = 0,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.UnpinChatMessageAsync(chatId, messageId, cancellationToken);
        }

        public Task UnpinAllChatMessages(ChatId chatId, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.UnpinAllChatMessages(chatId, cancellationToken);
        }

        public Task SetChatStickerSetAsync(
            ChatId chatId,
            string stickerSetName,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.SetChatStickerSetAsync(chatId, stickerSetName, cancellationToken);
        }

        public Task DeleteChatStickerSetAsync(ChatId chatId, CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.DeleteChatStickerSetAsync(chatId, cancellationToken);
        }

        public Task<ChatInviteLink> CreateChatInviteLinkAsync(ChatId chatId, DateTime? expireDate = null,
            int? memberLimit = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.CreateChatInviteLinkAsync(chatId, expireDate, memberLimit, cancellationToken);
        }

        public Task<ChatInviteLink> EditChatInviteLinkAsync(ChatId chatId, string inviteLink,
            DateTime? expireDate = null, int? memberLimit = null,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.EditChatInviteLinkAsync(chatId, inviteLink, expireDate, memberLimit,
                cancellationToken);
        }

        public Task<ChatInviteLink> RevokeChatInviteLinkAsync(ChatId chatId, string inviteLink,
            CancellationToken cancellationToken = new())
        {
            return _telegramBotClient.RevokeChatInviteLinkAsync(chatId, inviteLink, cancellationToken);
        }

        public long? BotId => _telegramBotClient.BotId;

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

        public event AsyncEventHandler<ApiRequestEventArgs>? OnMakingApiRequest
        {
            add => _telegramBotClient.OnMakingApiRequest += value;
            remove => _telegramBotClient.OnMakingApiRequest -= value;
        }

        public event AsyncEventHandler<ApiResponseEventArgs>? OnApiResponseReceived
        {
            add => _telegramBotClient.OnApiResponseReceived += value;
            remove => _telegramBotClient.OnApiResponseReceived -= value;
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
    }
}
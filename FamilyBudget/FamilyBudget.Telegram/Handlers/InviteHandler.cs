using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Telegram.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers
{
    public class InviteHandler
    {
        private readonly IFamilyInviteService _inviteService;
        private readonly IUserService _userService;
        private readonly UserStateService _stateService;
        private readonly TempInviteStorage _inviteStorage;
        private readonly RegistrationHandler _registrationHandler;

        public InviteHandler(
            IFamilyInviteService inviteService,
            IUserService userService,
            UserStateService stateService,
            TempInviteStorage inviteStorage,
            RegistrationHandler registrationHandler)
        {
            _inviteService = inviteService;
            _userService = userService;
            _stateService = stateService;
            _inviteStorage = inviteStorage;
            _registrationHandler = registrationHandler;
        }

        // Existing Message-based API (kept for compatibility)
        //public async Task CreateInvite(
        //    ITelegramBotClient bot,
        //    Message message)
        //{
        //    var user = await _userService.GetByTelegramIdAsync(message.From!.Id);

        //    if (user == null)
        //    {
        //        await bot.SendMessage(
        //            message.Chat.Id,
        //            "❌ Вы не зарегистрированы. Используйте /start для регистрации.");
        //        return;
        //    }

        //    var code = await _inviteService.CreateInvite(user.FamilyId);

        //    var link = $"https://t.me/YOUR_BOT_NAME?start=join_{code}";

        //    await bot.SendMessage(
        //        message.Chat.Id,
        //        $"📩 Приглашение в семью:\n\n{link}\n\n⏱ Действует 24 часа");
        //}

        // New overload: handle CallbackQuery (button press) where query.From is the user
        public async Task CreateInvite(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            var chatId = query.Message?.Chat.Id ?? query.From.Id;

            var user = await _userService.GetByTelegramIdAsync(query.From.Id);

            if (user == null || !user.FamilyId.HasValue)
            {
                await bot.SendMessage(
                    chatId,
                    "❌ Вы не зарегистрированы. Используйте /start для регистрации.");
                return;
            }

            var code = await _inviteService.CreateInvite(user.FamilyId.Value);

            var botInfo = await bot.GetMe();
            var link = $"https://t.me/{botInfo.Username}?start=join_{code}";

            await bot.SendMessage(
                chatId,
                $"📩 Приглашение в семью:\n\n{link}\n\n⏱ Действует 24 часа");
        }

        public async Task JoinByInvite(
            ITelegramBotClient bot,
            Message message)
        {
            var code = message.Text!
                .Replace("/start join_", "");

            if (!await _inviteService.IsInviteValid(code))
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    "❌ Ссылка недействительна или истекла");

                return;
            }

            await StartJoinFlowAsync(bot, message, code);
        }

        public async Task EnterInviteCodeAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            _stateService.Set(query.From.Id, UserState.WaitingForInviteCode);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Введите пригласительный код:");
        }

        public async Task HandleInviteCodeMessageAsync(
            ITelegramBotClient bot,
            Message message)
        {
            var telegramId = message.From!.Id;

            var code = message.Text!.Trim();

            if (!await _inviteService.IsInviteValid(code))
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    "❌ Код недействителен или истек. Попробуйте снова.");
                return;
            }

            await StartJoinFlowAsync(bot, message, code);
        }

        private async Task StartJoinFlowAsync(ITelegramBotClient bot, Message message, string code)
        {
            var telegramId = message.From!.Id;
            var existingUser = await _userService.GetByTelegramIdAsync(telegramId);
            var suggestedName = existingUser?.Name
                ?? message.From.Username
                ?? message.From.FirstName
                ?? "Участник";

            _inviteStorage.SaveInviteCode(telegramId, code);
            await _registrationHandler.StartInviteFlowAsync(
                bot,
                telegramId,
                message.Chat.Id,
                suggestedName);
        }
    }
}

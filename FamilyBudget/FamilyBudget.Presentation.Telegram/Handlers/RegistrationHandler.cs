using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Presentation.Telegram.Keyboards;
using FamilyBudget.Presentation.Telegram.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using System.Net;

namespace FamilyBudget.Presentation.Telegram.Handlers
{
    public class RegistrationHandler
    {
        private readonly IRegistrationService _registrationService;
        private readonly IUserService _userService;
        private readonly IFamilyInviteService _familyInviteService;
        private readonly UserStateService _state;
        private readonly TempInviteStorage _inviteStorage;
        private readonly TempRegistrationStorage _registrationStorage;

        public RegistrationHandler(
            IRegistrationService registrationService,
            IUserService userService,
            IFamilyInviteService familyInviteService,
            UserStateService state,
            TempInviteStorage inviteStorage,
            TempRegistrationStorage registrationStorage)
        {
            _registrationService = registrationService;
            _userService = userService;
            _familyInviteService = familyInviteService;
            _state = state;
            _inviteStorage = inviteStorage;
            _registrationStorage = registrationStorage;
        }

        public async Task StartAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            _state.Set(query.From.Id, UserState.WaitingForFamilyName);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Введите название семьи:");
        }

        public async Task StartInviteFlowAsync(
            ITelegramBotClient bot,
            long telegramId,
            long chatId,
            string suggestedName)
        {
            _inviteStorage.SaveSuggestedName(telegramId, suggestedName);
            _state.Set(telegramId, UserState.WaitingForInviteUserName);

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Оставить имя", "keep_name"),
                    InlineKeyboardButton.WithCallbackData("Изменить имя", "change_name")
                }
            });

            await bot.SendMessage(
                chatId,
                $"Ваше имя: <b>{WebUtility.HtmlEncode(suggestedName)}</b>\n\nХотите оставить это имя или изменить?",
                replyMarkup: keyboard,
                parseMode: ParseMode.Html);
        }

        public async Task HandleMessageAsync(
            ITelegramBotClient bot,
            Message message)
        {
            var telegramId = message.From!.Id;
            var state = _state.Get(telegramId);

            if (state == UserState.WaitingForFamilyName)
            {
                _registrationStorage.SaveFamilyName(telegramId, message.Text!);
                _state.Set(telegramId, UserState.WaitingForUserName);

                await bot.SendMessage(
                    message.Chat.Id,
                    "Введите ваше имя:");
                return;
            }

            if (state == UserState.WaitingForUserName)
            {
                var familyName = _registrationStorage.GetFamilyName(telegramId);
                if (string.IsNullOrWhiteSpace(familyName))
                {
                    _state.Clear(telegramId);
                    await bot.SendMessage(message.Chat.Id, "Сценарий регистрации устарел. Начните снова с /start.");
                    return;
                }
                var userName = message.Text!;

                await _registrationService
                    .RegisterNewFamilyAsync(
                        telegramId,
                        familyName,
                        userName,
                        message.From.Username);

                _registrationStorage.Clear(telegramId);
                _state.Set(telegramId, UserState.WaitingForInitialBalance);

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Да", "add_initial_balance"),
                        InlineKeyboardButton.WithCallbackData("Позже", "skip_initial_balance")
                    }
                });

                await bot.SendMessage(
                    message.Chat.Id,
                    "🎉 Семья успешно создана!\n\nХотите добавить начальный баланс?",
                    replyMarkup: keyboard);
                return;
            }

            if (state == UserState.WaitingForInitialBalance)
            {
                if (!decimal.TryParse(message.Text, out var amount) || amount < 0)
                {
                    await bot.SendMessage(
                        message.Chat.Id,
                        "Введите корректный неотрицательный баланс.");
                    return;
                }

                var user = await _userService.GetByTelegramIdAsync(telegramId);
                if (user is null || !user.FamilyId.HasValue)
                {
                    await bot.SendMessage(message.Chat.Id, "Ошибка при добавлении баланса.");
                    return;
                }

                await _userService.SetBalanceAsync(telegramId, amount);

                _state.Clear(telegramId);

                await bot.SendMessage(
                    message.Chat.Id,
                    $"✅ Баланс счёта установлен: <b>{amount:F2}</b>",
                    replyMarkup: KeyboardFactory.MainMenu(),
                    parseMode: ParseMode.Html);
                return;
            }

            if (state == UserState.WaitingForInviteUserName)
            {
                var newName = message.Text!;
                var inviteCode = _inviteStorage.GetInviteCode(telegramId);

                if (string.IsNullOrWhiteSpace(inviteCode))
                {
                    await bot.SendMessage(message.Chat.Id, "❌ Ошибка: семья не найдена");
                    return;
                }

                var joinResult = await _familyInviteService.JoinFamilyAsync(
                    inviteCode,
                    telegramId,
                    newName,
                    message.From.Username);
                if (!joinResult.IsSuccess)
                {
                    await bot.SendMessage(message.Chat.Id, $"❌ {joinResult.Error}");
                    return;
                }

                _inviteStorage.Clear(telegramId);
                _state.Clear(telegramId);

                await bot.SendMessage(
                    message.Chat.Id,
                    "✅ Вы присоединились к семье!",
                    replyMarkup: KeyboardFactory.MainMenu());
            }
        }

        public async Task HandleKeepNameAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            var telegramId = query.From.Id;
            var suggestedName = _inviteStorage.GetSuggestedName(telegramId);
            var inviteCode = _inviteStorage.GetInviteCode(telegramId);

            if (string.IsNullOrEmpty(suggestedName) || string.IsNullOrWhiteSpace(inviteCode))
            {
                await bot.SendMessage(query.Message!.Chat.Id, "❌ Ошибка: данные не найдены");
                return;
            }

            var joinResult = await _familyInviteService.JoinFamilyAsync(
                inviteCode,
                telegramId,
                suggestedName,
                query.From.Username);
            if (!joinResult.IsSuccess)
            {
                await bot.SendMessage(query.Message!.Chat.Id, $"❌ {joinResult.Error}");
                return;
            }

            _inviteStorage.Clear(telegramId);
            _state.Clear(telegramId);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "✅ Вы присоединились к семье!",
                replyMarkup: KeyboardFactory.MainMenu());
        }

        public async Task HandleChangeNameAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            _state.Set(query.From.Id, UserState.WaitingForInviteUserName);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Введите новое имя:");
        }

        public async Task HandleAddInitialBalanceAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            _state.Set(query.From.Id, UserState.WaitingForInitialBalance);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Введите текущий баланс на вашем счёте:");
        }

        public async Task HandleSkipInitialBalanceAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            _state.Clear(query.From.Id);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Вы можете указать баланс позже в главном меню.",
                replyMarkup: KeyboardFactory.MainMenu());
        }
    }
}

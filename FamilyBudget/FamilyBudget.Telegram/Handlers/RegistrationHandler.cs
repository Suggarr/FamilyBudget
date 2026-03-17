using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Telegram.Keyboards;
using FamilyBudget.Telegram.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace FamilyBudget.Telegram.Handlers
{
    public class RegistrationHandler
    {
        private readonly IRegistrationService _registrationService;
        private readonly IUserService _userService;
        private readonly UserStateService _state;
        private readonly TempInviteStorage _inviteStorage;

        private readonly Dictionary<long, string> _tempFamilyNames = new();

        public RegistrationHandler(
            IRegistrationService registrationService,
            IUserService userService,
            UserStateService state,
            TempInviteStorage inviteStorage)
        {
            _registrationService = registrationService;
            _userService = userService;
            _state = state;
            _inviteStorage = inviteStorage;
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
                $"Ваше имя: <b>{suggestedName}</b>\n\nХотите оставить это имя или изменить?",
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
                _tempFamilyNames[telegramId] = message.Text!;
                _state.Set(telegramId, UserState.WaitingForUserName);

                await bot.SendMessage(
                    message.Chat.Id,
                    "Введите ваше имя:");
                return;
            }

            if (state == UserState.WaitingForUserName)
            {
                var familyName = _tempFamilyNames[telegramId];
                var userName = message.Text!;

                await _registrationService
                    .RegisterNewFamilyAsync(
                        telegramId,
                        familyName,
                        userName);

                _tempFamilyNames.Remove(telegramId);
                _state.Clear(telegramId);

                await bot.SendMessage(
                    message.Chat.Id,
                    "Семья успешно создана!",
                    replyMarkup: KeyboardFactory.MainMenu());
                return;
            }

            if (state == UserState.WaitingForInviteUserName)
            {
                var newName = message.Text!;
                var familyId = _inviteStorage.GetFamilyId(telegramId);

                if (familyId == null)
                {
                    await bot.SendMessage(message.Chat.Id, "❌ Ошибка: семья не найдена");
                    return;
                }

                // Используем новый метод который правильно обновляет User
                await _userService.SetFamilyForUserAsync(telegramId, newName, familyId.Value);

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
            var familyId = _inviteStorage.GetFamilyId(telegramId);

            if (string.IsNullOrEmpty(suggestedName) || familyId == null)
            {
                await bot.SendMessage(query.Message!.Chat.Id, "❌ Ошибка: данные не найдены");
                return;
            }

            // Используем новый метод который правильно обновляет User
            await _userService.SetFamilyForUserAsync(telegramId, suggestedName, familyId.Value);

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
    }
}

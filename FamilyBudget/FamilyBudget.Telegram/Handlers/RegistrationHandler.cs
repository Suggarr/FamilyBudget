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

namespace FamilyBudget.Telegram.Handlers
{
    public class RegistrationHandler
    {
        private readonly IRegistrationService _registrationService;
        private readonly UserStateService _state;

        private readonly Dictionary<long, string> _tempFamilyNames = new();

        public RegistrationHandler(
            IRegistrationService registrationService,
            UserStateService state)
        {
            _registrationService = registrationService;
            _state = state;
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
            }
        }
    }
}

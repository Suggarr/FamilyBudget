using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Telegram.Keyboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Telegram.Handlers
{
    public class StartHandler
    {
        private readonly IRegistrationService _registrationService;

        public StartHandler(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        public async Task HandleAsync(
            ITelegramBotClient bot,
            Message message)
        {
            var telegramId = message.From!.Id;

            var isRegistered = await _registrationService
                .IsRegisteredAsync(telegramId);

            if (!isRegistered)
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    "Вы не зарегистрированы.",
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData(
                            "Создать семью",
                            "create_family")));

                return;
            }

            await bot.SendMessage(
                message.Chat.Id,
                "Главное меню",
                replyMarkup: KeyboardFactory.MainMenu());
        }
    }
}

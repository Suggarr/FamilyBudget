using FamilyBudget.Application.Interfaces;
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
        private readonly IUserService _userService;

        public StartHandler(
            IRegistrationService registrationService,
            IUserService userService)
        {
            _registrationService = registrationService;
            _userService = userService;
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
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Создать семью", "create_family"),
                        InlineKeyboardButton.WithCallbackData("Войти по коду", "enter_invite_code")
                    }
                });

                await bot.SendMessage(
                    message.Chat.Id,
                    "Вы не зарегистрированы.",
                    replyMarkup: keyboard);

                return;
            }

            // Проверяем есть ли у пользователя семья
            var user = await _userService.GetByTelegramIdAsync(telegramId);
            
            if (user != null && user.FamilyId == null)
            {
                // Пользователь есть, но нет семьи (вышел из семьи)
                var keyboardWithInvite = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Создать семью", "create_family"),
                        InlineKeyboardButton.WithCallbackData("Войти по коду", "enter_invite_code")
                    }
                });

                await bot.SendMessage(
                    message.Chat.Id,
                    "Вы покинули семью. Создайте новую или введите пригласительный код.",
                    replyMarkup: keyboardWithInvite);
                return;
            }

            await bot.SendMessage(
                message.Chat.Id,
                "Главное меню",
                replyMarkup: KeyboardFactory.MainMenu());
        }
    }
}

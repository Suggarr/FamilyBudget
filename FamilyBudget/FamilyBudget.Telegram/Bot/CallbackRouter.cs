using FamilyBudget.Telegram.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Bot
{
    public class CallbackRouter
    {
        private readonly ExpenseHandler _expenseHandler;
        private readonly RegistrationHandler _registrationHandler;

        public CallbackRouter(
            ExpenseHandler expenseHandler, RegistrationHandler registrationHandler)
        {
            _expenseHandler = expenseHandler;
            _registrationHandler = registrationHandler;
        }

        public async Task RouteAsync(ITelegramBotClient bot, CallbackQuery query)
        {
            await bot.AnswerCallbackQuery(query.Id);

            switch (query.Data)
            {
                case "expense":
                    await _expenseHandler.StartAsync(bot, query);
                    break;

                case "create_family":
                    await _registrationHandler.StartAsync(bot, query);
                    break;
            }
        }
    }
}

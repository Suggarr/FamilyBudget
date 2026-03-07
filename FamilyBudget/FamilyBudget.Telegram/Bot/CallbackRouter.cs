using FamilyBudget.Core.Enums;
using FamilyBudget.Telegram.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FamilyBudget.Telegram.Bot
{
    public class CallbackRouter
    {
        private readonly ExpenseHandler _expenseHandler;
        private readonly RegistrationHandler _registrationHandler;
        private readonly IncomeHandler _incomeHandler;
        private readonly ReportHandler _reportHandler;

        public CallbackRouter(
            ExpenseHandler expenseHandler, RegistrationHandler registrationHandler, IncomeHandler incomeHandler, ReportHandler reportHandler)
        {
            _expenseHandler = expenseHandler;
            _registrationHandler = registrationHandler;
            _incomeHandler = incomeHandler;
            _reportHandler = reportHandler;
        }

        public async Task RouteAsync(ITelegramBotClient bot, CallbackQuery query)
        {
            await bot.AnswerCallbackQuery(query.Id);

            if (query.Data!.StartsWith("report:"))
            {
                await _reportHandler.HandleMonth(bot, query);
            }

            switch (query.Data)
            {
                case "expense":
                    await _expenseHandler.StartAsync(bot, query);
                    break;

                case "income":
                    await _incomeHandler.StartAsync(bot, query);
                    break;

                case "cat_food":
                    await _expenseHandler.FinishAsync(bot, query, ExpenseCategory.Food);
                    break;

                case "cat_transport":
                    await _expenseHandler.FinishAsync(bot, query, ExpenseCategory.Transport);
                    break;

                case "cat_home":
                    await _expenseHandler.FinishAsync(bot, query, ExpenseCategory.Home);
                    break;

                case "cat_fun":
                    await _expenseHandler.FinishAsync(bot, query, ExpenseCategory.Entertainment);
                    break;

                case "cat_health":
                    await _expenseHandler.FinishAsync(bot, query, ExpenseCategory.Health);
                    break;

                case "cat_other":
                    await _expenseHandler.FinishAsync(bot, query, ExpenseCategory.Other);
                    break;

                case "report":
                    await _reportHandler.ShowMonths(bot, query.Message!);
                    break;

                case "create_family":
                    await _registrationHandler.StartAsync(bot, query);
                    break;
            }
        }
    }
}

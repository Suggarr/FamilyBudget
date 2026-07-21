using FamilyBudget.Core.Enums;
using FamilyBudget.Telegram.Handlers;
using FamilyBudget.Telegram.Keyboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FamilyBudget.Telegram.Bot
{
    public class CallbackRouter
    {
        private readonly ExpenseHandler _expenseHandler;
        private readonly RegistrationHandler _registrationHandler;
        private readonly IncomeHandler _incomeHandler;
        private readonly ReportHandler _reportHandler;
        private readonly InviteHandler _inviteHandler;
        private readonly FamilyHandler _familyHandler;
        private readonly InitialBalanceHandler _initialBalanceHandler;
        private readonly SavingsHandler _savingsHandler;
        private readonly ReceiptHandler _receiptHandler;

        public CallbackRouter(ExpenseHandler expenseHandler, RegistrationHandler registrationHandler, IncomeHandler incomeHandler, ReportHandler reportHandler, 
            InviteHandler inviteHandler, FamilyHandler familyHandler, InitialBalanceHandler initialBalanceHandler, SavingsHandler savingsHandler, ReceiptHandler receiptHandler)
        {
            _expenseHandler = expenseHandler;
            _registrationHandler = registrationHandler;
            _incomeHandler = incomeHandler;
            _reportHandler = reportHandler;
            _inviteHandler = inviteHandler;
            _familyHandler = familyHandler;
            _initialBalanceHandler = initialBalanceHandler;
            _savingsHandler = savingsHandler;
            _receiptHandler = receiptHandler;
        }

        public async Task RouteAsync(ITelegramBotClient bot, CallbackQuery query)
        {
            try
            {
                await bot.AnswerCallbackQuery(query.Id);
            }
            catch (ApiRequestException ex) when (ex.ErrorCode == 400 && ex.Message.Contains("query is too old", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (query.Data!.StartsWith("report:"))
            {
                await _reportHandler.HandleMonth(bot, query);
            }

            if (query.Data.StartsWith("receipt_confirm:"))
            {
                await _receiptHandler.ConfirmAsync(bot, query);
                return;
            }

            if (query.Data.StartsWith("receipt_reject:"))
            {
                await _receiptHandler.RejectAsync(bot, query);
                return;
            }

            switch (query.Data)
            {
                case "expense":
                    await _expenseHandler.ShowInputOptionsAsync(bot, query);
                    break;

                case "expense_manual":
                    await _expenseHandler.StartAsync(bot, query);
                    break;

                case "receipt_start":
                    await _receiptHandler.StartAsync(bot, query);
                    break;

                case "income":
                    await _incomeHandler.StartAsync(bot, query);
                    break;

                case "account":
                    await _initialBalanceHandler.ShowAsync(bot, query);
                    break;

                case "set_account_balance":
                    await _initialBalanceHandler.StartAsync(bot, query);
                    break;

                case "add_initial_balance":
                    await _registrationHandler.HandleAddInitialBalanceAsync(bot, query);
                    break;

                case "skip_initial_balance":
                    await _registrationHandler.HandleSkipInitialBalanceAsync(bot, query);
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

                case "enter_invite_code":
                    await _inviteHandler.EnterInviteCodeAsync(bot, query);
                    break;

                case "keep_name":
                    await _registrationHandler.HandleKeepNameAsync(bot, query);
                    break;

                case "change_name":
                    await _registrationHandler.HandleChangeNameAsync(bot, query);
                    break;

                case "📩 Пригласить участника":
                    await _inviteHandler.CreateInvite(bot, query);
                    break;
                case "family_invite":
                    await _inviteHandler.CreateInvite(bot, query);
                    break;

                case "family_members":
                    await _familyHandler.ShowMembers(bot, query);
                    break;

                case "savings":
                    await _savingsHandler.ShowAsync(bot, query);
                    break;

                case "savings_contribute":
                    await _savingsHandler.StartContributionAsync(bot, query);
                    break;

                case "savings_withdraw":
                    await _savingsHandler.StartWithdrawalAsync(bot, query);
                    break;

                case "family_leave":
                    await _familyHandler.LeaveFamily(bot, query);
                    break;

                case "family":
                    await bot.SendMessage(
                        query.Message!.Chat.Id,
                        "👨‍👩‍👧 Семья",
                        replyMarkup: KeyboardFactory.FamilyMenu());
                    break;

                case "main_menu":
                    await bot.SendMessage(
                        query.Message!.Chat.Id,
                        "Главное меню",
                        replyMarkup: KeyboardFactory.MainMenu());
                    break;
            }
        }
    }
}

using FamilyBudget.Core.Enums;
using FamilyBudget.Telegram.Handlers;
using FamilyBudget.Telegram.Keyboards;
using FamilyBudget.Telegram.Receipts;
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
        private readonly FamilyHistoryHandler _familyHistoryHandler;
        private readonly ReceiptHistoryHandler _receiptHistoryHandler;
        private readonly ReportSubscriptionHandler _reportSubscriptionHandler;

        public CallbackRouter(ExpenseHandler expenseHandler, RegistrationHandler registrationHandler, IncomeHandler incomeHandler, ReportHandler reportHandler, 
            InviteHandler inviteHandler, FamilyHandler familyHandler, InitialBalanceHandler initialBalanceHandler, SavingsHandler savingsHandler,
            ReceiptHandler receiptHandler, FamilyHistoryHandler familyHistoryHandler, ReceiptHistoryHandler receiptHistoryHandler,
            ReportSubscriptionHandler reportSubscriptionHandler)
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
            _familyHistoryHandler = familyHistoryHandler;
            _receiptHistoryHandler = receiptHistoryHandler;
            _reportSubscriptionHandler = reportSubscriptionHandler;
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

            var data = query.Data;
            if (string.IsNullOrWhiteSpace(data))
                return;

            if (data.StartsWith("receipt_confirm:"))
            {
                await _receiptHandler.ConfirmAsync(bot, query);
                return;
            }

            if (data.StartsWith("report_subscription_day:", StringComparison.Ordinal))
            {
                var dayText = data["report_subscription_day:".Length..];
                if (int.TryParse(dayText, out var dayValue) &&
                    Enum.IsDefined((DayOfWeek)dayValue))
                {
                    await _reportSubscriptionHandler.SelectDayAsync(
                        bot,
                        query,
                        (DayOfWeek)dayValue);
                }

                return;
            }

            if (data.StartsWith("receipt_reject:"))
            {
                await _receiptHandler.RejectAsync(bot, query);
                return;
            }

            if (data.StartsWith("history_page:", StringComparison.Ordinal))
            {
                var pageText = data["history_page:".Length..];
                if (int.TryParse(pageText, out var page) && page > 0)
                    await _familyHistoryHandler.ShowAsync(bot, query, page);

                return;
            }

            if (data.StartsWith("receipts_page:", StringComparison.Ordinal))
            {
                var pageText = data["receipts_page:".Length..];
                if (int.TryParse(pageText, out var page) && page > 0)
                    await _receiptHistoryHandler.ShowListAsync(bot, query, page);

                return;
            }

            if (data.StartsWith("rv:", StringComparison.Ordinal))
            {
                var parts = data.Split(':');
                if (parts.Length == 5 &&
                    Guid.TryParse(parts[1], out var receiptId) &&
                    int.TryParse(parts[2], out var itemPage) && itemPage > 0 &&
                    TryParseReceiptOrigin(parts[3], out var origin) &&
                    int.TryParse(parts[4], out var originPage) && originPage > 0)
                {
                    await _receiptHistoryHandler.ShowDetailsAsync(
                        bot,
                        query,
                        receiptId,
                        itemPage,
                        origin,
                        originPage);
                }

                return;
            }

            if (data.StartsWith("receipt_view:", StringComparison.Ordinal))
            {
                var parts = data.Split(':');
                if (parts.Length == 4 &&
                    Guid.TryParse(parts[1], out var receiptId) &&
                    int.TryParse(parts[2], out var itemPage) && itemPage > 0 &&
                    int.TryParse(parts[3], out var listPage) && listPage > 0)
                {
                    await _receiptHistoryHandler.ShowDetailsAsync(
                        bot,
                        query,
                        receiptId,
                        itemPage,
                        ReceiptNavigationOrigin.ReceiptList,
                        listPage);
                }

                return;
            }

            switch (data)
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

                case "history":
                    await _familyHistoryHandler.ShowAsync(bot, query);
                    break;

                case "history_noop":
                    break;

                case "receipts":
                    await _receiptHistoryHandler.ShowListAsync(bot, query);
                    break;

                case "receipts_noop":
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
                    await _reportHandler.ShowPeriodsAsync(bot, query);
                    break;

                case "report_all":
                    await _reportHandler.ShowAllTimeAsync(bot, query);
                    break;

                case "report_7_days":
                    await _reportHandler.ShowRecentDaysAsync(bot, query, 7);
                    break;

                case "report_30_days":
                    await _reportHandler.ShowRecentDaysAsync(bot, query, 30);
                    break;

                case "report_custom":
                    await _reportHandler.StartCustomPeriodAsync(bot, query);
                    break;

                case "report_cancel":
                    await _reportHandler.CancelAsync(bot, query);
                    break;

                case "report_subscription":
                    await _reportSubscriptionHandler.ShowAsync(bot, query);
                    break;

                case "report_subscription_setup":
                    await _reportSubscriptionHandler.StartSetupAsync(bot, query);
                    break;

                case "report_subscription_disable":
                    await _reportSubscriptionHandler.DisableAsync(bot, query);
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

        private static bool TryParseReceiptOrigin(
            string value,
            out ReceiptNavigationOrigin origin)
        {
            origin = value == "h"
                ? ReceiptNavigationOrigin.FamilyHistory
                : ReceiptNavigationOrigin.ReceiptList;

            return value is "h" or "r";
        }
    }
}

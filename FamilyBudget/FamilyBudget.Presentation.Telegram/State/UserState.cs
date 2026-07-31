using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Presentation.Telegram.State
{
    public enum UserState
    {
        None,
        WaitingForExpenseAmount,
        WaitingForExpenseCategory,
        WaitingForFamilyName,
        WaitingForUserName,
        WaitingForExpenseDescription,
        WaitingForIncomeAmount,
        WaitingForIncomeDescription,
        WaitingForInviteCode,
        WaitingForInviteUserName,
        WaitingForInitialBalance,
        WaitingForSavingsContribution,
        WaitingForSavingsWithdrawal,
        WaitingForReceiptPhoto,
        WaitingForReportPeriod,
        WaitingForReportSubscriptionTime
    }
}

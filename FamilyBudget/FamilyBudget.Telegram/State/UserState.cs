using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Telegram.State
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
        WaitingForIncomeDescription
    }
}

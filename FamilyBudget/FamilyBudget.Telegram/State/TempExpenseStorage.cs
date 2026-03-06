using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Telegram.State
{
    public class TempExpenseStorage
    {
        private readonly Dictionary<long, decimal> _amounts = new();

        public void SaveAmount(long id, decimal amount) =>
            _amounts[id] = amount;

        public decimal GetAmount(long id) =>
            _amounts[id];

        public void Clear(long id) =>
            _amounts.Remove(id);
    }
}

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Presentation.Telegram.State
{
    public class TempIncomeStorage
    {
        private readonly ConcurrentDictionary<long, decimal> _amounts = new();

        public void SetAmount(long userId, decimal amount)
        {
            _amounts[userId] = amount;
        }

        public decimal GetAmount(long userId)
        {
            return _amounts[userId];
        }

        public void Clear(long userId)
        {
            _amounts.TryRemove(userId, out _);
        }
    }
}

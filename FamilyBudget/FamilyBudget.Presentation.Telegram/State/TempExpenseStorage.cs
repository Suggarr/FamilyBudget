using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Presentation.Telegram.State
{
    public class TempExpenseStorage
    {
        private readonly ConcurrentDictionary<long, decimal> _amounts = new();
        private readonly ConcurrentDictionary<long, string> _descriptions = new();

        public void SaveAmount(long id, decimal amount) =>
            _amounts[id] = amount;

        public decimal GetAmount(long id) =>
            _amounts[id];

        public void SaveDescription(long id, string description) =>
            _descriptions[id] = description;

        public string GetDescription(long id) =>
            _descriptions[id];

        public void Clear(long id)
        {
            _amounts.TryRemove(id, out _);
            _descriptions.TryRemove(id, out _);
        }
    }
}

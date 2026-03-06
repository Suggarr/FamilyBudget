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
        private readonly Dictionary<long, string> _descriptions = new();

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
            _amounts.Remove(id);
            _descriptions.Remove(id);
        }
    }
}

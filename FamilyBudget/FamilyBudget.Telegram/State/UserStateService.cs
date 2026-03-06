using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Telegram.State
{
    public class UserStateService
    {
        private readonly Dictionary<long, UserState> _states = new();

        public void Set(long id, UserState state) =>
            _states[id] = state;

        public UserState Get(long id) =>
            _states.TryGetValue(id, out var s) ? s : UserState.None;

        public void Clear(long id) =>
            _states.Remove(id);
    }
}

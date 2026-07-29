using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Telegram.State
{
    public class UserStateService
    {
        private readonly ConcurrentDictionary<long, UserState> _states = new();

        public void Set(long id, UserState state) =>
            _states[id] = state;

        public UserState Get(long id) =>
            _states.TryGetValue(id, out var s) ? s : UserState.None;

        public void Clear(long id) =>
            _states.TryRemove(id, out _);
    }
}

using System.Collections.Concurrent;

namespace ASM_1.Services
{
    public class TableTrackerService : ITableTrackerService
    {
        private readonly ConcurrentDictionary<int, HashSet<string>> _activeGuests = new();

        public void AddGuest(int TableId, string sessionId)
        {
            var guests = _activeGuests.GetOrAdd(TableId, _ => new HashSet<string>());
            lock (guests)
            {
                guests.Add(sessionId);
            }
        }

        public void RemoveGuest(int TableId, string sessionId)
        {
            if (_activeGuests.TryGetValue(TableId, out var guests))
            {
                lock (guests)
                {
                    guests.Remove(sessionId);
                    if (guests.Count == 0)
                        _activeGuests.TryRemove(TableId, out _);
                }
            }
        }

        public int GetGuestCount(int TableId)
        {
            if (_activeGuests.TryGetValue(TableId, out var guests))
                return guests.Count;
            return 0;
        }
    }
}

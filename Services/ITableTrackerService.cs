namespace ASM_1.Services
{
    public interface ITableTrackerService
    {
        void AddGuest(int TableId, string sessionId);
        void RemoveGuest(int TableId, string sessionId);
        int GetGuestCount(int TableId);
    }
}

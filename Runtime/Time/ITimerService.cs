namespace Air.UnityGameCore.Runtime.Time
{
    public interface ITimerService
    {
        void RegisterTimer(Timer timer);
        void DeregisterTimer(Timer timer);
        void UpdateTimers();
        void Clear();
    }
}

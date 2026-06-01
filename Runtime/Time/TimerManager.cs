using System.Collections.Generic;

namespace Air.UnityGameCore.Runtime.Time
{
    public static class TimerManager
    {
        static readonly List<Timer> timers = new();
        static readonly List<Timer> sweep = new();

        public static void RegisterTimer(Timer timer) => timers.Add(timer);
        public static void DeregisterTimer(Timer timer) => timers.Remove(timer);

        public static void UpdateTimers()
        {
            if (timers.Count == 0) return;

            sweep.Clear();
            sweep.AddRange(timers);
            foreach (var timer in sweep) {
                timer.Tick();
            }
        }
        
        public static void Clear()
        {
            sweep.Clear();
            sweep.AddRange(timers);
            foreach (var timer in sweep) {
                timer.Dispose();
            }
            
            timers.Clear();
            sweep.Clear();
        }
    }
}
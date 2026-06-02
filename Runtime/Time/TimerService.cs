using System.Collections.Generic;

namespace Air.UnityGameCore.Runtime.Time
{
  public sealed class TimerService : ITimerService
  {
    readonly List<Timer> _timers = new();
    readonly List<Timer> _sweep = new();

    public void RegisterTimer(Timer timer) => _timers.Add(timer);

    public void DeregisterTimer(Timer timer) => _timers.Remove(timer);

    public void UpdateTimers()
    {
      if (_timers.Count == 0)
        return;

      _sweep.Clear();
      _sweep.AddRange(_timers);
      foreach (var timer in _sweep)
        timer.Tick();
    }

    public void Clear()
    {
      _sweep.Clear();
      _sweep.AddRange(_timers);
      foreach (var timer in _sweep)
        timer.Dispose();

      _timers.Clear();
      _sweep.Clear();
    }
  }
}

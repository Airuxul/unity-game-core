using System;
using UnityEngine;

namespace Air.UnityGameCore.Runtime.Time
{
  public abstract class Timer : IDisposable
  {
    readonly ITimerService _timers;

    public float CurrentTime { get; protected set; }
    public bool IsRunning { get; private set; }

    protected float initialTime;

    public float Progress =>
      initialTime <= 0f ? 0f : Mathf.Clamp(CurrentTime / initialTime, 0, 1);

    public Action OnTimerStart = delegate { };
    public Action OnTimerStop = delegate { };

    protected Timer(float value, ITimerService timers)
    {
      initialTime = value;
      _timers = timers ?? throw new ArgumentNullException(nameof(timers));
    }

    public void Start()
    {
      CurrentTime = initialTime;
      if (!IsRunning)
      {
        IsRunning = true;
        _timers.RegisterTimer(this);
        OnTimerStart.Invoke();
      }
    }

    public void Stop()
    {
      if (IsRunning)
      {
        IsRunning = false;
        _timers.DeregisterTimer(this);
        OnTimerStop.Invoke();
      }
    }

    public abstract void Tick();
    public abstract bool IsFinished { get; }

    public void Resume() => IsRunning = true;
    public void Pause() => IsRunning = false;

    public virtual void Reset() => CurrentTime = initialTime;

    public virtual void Reset(float newTime)
    {
      initialTime = newTime;
      Reset();
    }

    bool _disposed;

    ~Timer() => Dispose(false);

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (_disposed)
        return;

      if (disposing)
        _timers.DeregisterTimer(this);

      _disposed = true;
    }
  }
}

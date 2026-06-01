namespace Air.UnityGameCore.Runtime.Time {
    /// <summary>
    /// Timer that counts up from zero to infinity.  Great for measuring durations.
    /// </summary>
    public class StopwatchTimer : Timer {
        public StopwatchTimer() : base(0) { }

        public override void Tick() {
            if (IsRunning) {
                CurrentTime += UnityEngine.Time.deltaTime;
            }
        }

        public override bool IsFinished => false;
    }
}
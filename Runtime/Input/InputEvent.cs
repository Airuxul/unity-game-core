namespace Air.UnityGameCore.Runtime.Input
{
    public readonly struct InputEvent
    {
        public string ActionId { get; }
        public InputPhase Phase { get; }

        public InputEvent(string actionId, InputPhase phase)
        {
            ActionId = actionId;
            Phase = phase;
        }
    }
}

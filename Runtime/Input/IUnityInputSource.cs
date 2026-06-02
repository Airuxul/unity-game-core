using System.Collections.Generic;

namespace Air.UnityGameCore.Runtime.Input
{
    /// <summary>Collects raw input as logical <see cref="InputEvent"/> samples (Unity-specific).</summary>
    public interface IUnityInputSource
    {
        void Collect(List<InputEvent> into);
    }
}

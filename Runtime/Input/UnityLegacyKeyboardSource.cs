using System.Collections.Generic;
using UnityEngine;

namespace Air.UnityGameCore.Runtime.Input
{
  /// <summary>Maps Unity legacy <see cref="Input"/> keys to logical input action ids.</summary>
  public sealed class UnityLegacyKeyboardSource : IUnityInputSource
  {
    readonly List<KeyMapping> _mappings = new();
    readonly List<ChordMapping> _chords = new();

    public void MapKey(KeyCode key, string actionId)
    {
      if (string.IsNullOrEmpty(actionId))
        return;
      _mappings.Add(new KeyMapping(key, actionId));
    }

    public void MapChord(KeyCode modifier, KeyCode key, string actionId)
    {
      if (string.IsNullOrEmpty(actionId))
        return;
      _chords.Add(new ChordMapping(modifier, key, actionId));
    }

    public void Collect(List<InputEvent> into)
    {
      if (into == null)
        return;

      foreach (var mapping in _mappings)
      {
        if (UnityEngine.Input.GetKeyDown(mapping.Key))
          into.Add(new InputEvent(mapping.ActionId, InputPhase.Started));
        if (UnityEngine.Input.GetKey(mapping.Key))
          into.Add(new InputEvent(mapping.ActionId, InputPhase.Performed));
        if (UnityEngine.Input.GetKeyUp(mapping.Key))
          into.Add(new InputEvent(mapping.ActionId, InputPhase.Canceled));
      }

      foreach (var chord in _chords)
      {
        if (UnityEngine.Input.GetKey(chord.Modifier) && UnityEngine.Input.GetKeyDown(chord.Key))
          into.Add(new InputEvent(chord.ActionId, InputPhase.Started));
      }
    }

    readonly struct KeyMapping
    {
      public readonly KeyCode Key;
      public readonly string ActionId;

      public KeyMapping(KeyCode key, string actionId)
      {
        Key = key;
        ActionId = actionId;
      }
    }

    readonly struct ChordMapping
    {
      public readonly KeyCode Modifier;
      public readonly KeyCode Key;
      public readonly string ActionId;

      public ChordMapping(KeyCode modifier, KeyCode key, string actionId)
      {
        Modifier = modifier;
        Key = key;
        ActionId = actionId;
      }
    }
  }
}

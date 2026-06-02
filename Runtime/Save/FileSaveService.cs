using System;
using System.IO;
using Air.GameCore.Serialization;
using UnityEngine;

namespace Air.UnityGameCore.Runtime.Save
{
  public sealed class FileSaveService : ISaveService
  {
    readonly IJsonSerializer _json;
    readonly string _rootDirectory;

    public FileSaveService(IJsonSerializer json, string rootDirectory = null)
    {
      _json = json ?? throw new ArgumentNullException(nameof(json));
      _rootDirectory = string.IsNullOrWhiteSpace(rootDirectory)
        ? Path.Combine(Application.persistentDataPath, "saves")
        : rootDirectory;
    }

    public void Save<T>(string slot, T data)
    {
      if (string.IsNullOrWhiteSpace(slot))
        throw new ArgumentException("Slot is required.", nameof(slot));

      Directory.CreateDirectory(_rootDirectory);
      var path = GetPath(slot);
      File.WriteAllText(path, _json.Serialize(data));
    }

    public bool TryLoad<T>(string slot, out T data)
    {
      data = default;
      if (string.IsNullOrWhiteSpace(slot))
        return false;

      var path = GetPath(slot);
      if (!File.Exists(path))
        return false;

      data = _json.Deserialize<T>(File.ReadAllText(path));
      return true;
    }

    public bool Exists(string slot) =>
      !string.IsNullOrWhiteSpace(slot) && File.Exists(GetPath(slot));

    public bool Delete(string slot)
    {
      if (string.IsNullOrWhiteSpace(slot))
        return false;

      var path = GetPath(slot);
      if (!File.Exists(path))
        return false;
      File.Delete(path);
      return true;
    }

    string GetPath(string slot) =>
      Path.Combine(_rootDirectory, SanitizeSlot(slot) + ".json");

    static string SanitizeSlot(string slot)
    {
      foreach (var c in Path.GetInvalidFileNameChars())
        slot = slot.Replace(c, '_');
      return slot;
    }
  }
}

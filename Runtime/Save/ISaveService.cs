namespace Air.UnityGameCore.Runtime.Save
{
  public interface ISaveService
  {
    void Save<T>(string slot, T data);

    bool TryLoad<T>(string slot, out T data);

    bool Exists(string slot);

    bool Delete(string slot);
  }
}

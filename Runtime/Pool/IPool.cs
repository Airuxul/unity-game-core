namespace Air.UnityGameCore.Runtime.Pool
{
    public interface IPool
    {
        int CountAll { get; }
        int CountInactive { get; }
        int CountActive { get; }
        void Clear();
    }

    public interface IPool<T> : IPool
    {
        T Get();
        void Release(T element);
        void Prewarm(int count);
    }

    public interface IComponentPool : IPool
    {
        void Release(UnityEngine.Component component);
    }
}

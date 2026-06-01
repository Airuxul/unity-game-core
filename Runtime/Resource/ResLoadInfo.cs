namespace Air.UnityGameCore.Runtime.Resource
{
    public enum EResLoadStatus
    {
        Unload = 0,
        Loading = 1,
        Loaded = 2,
    }
    
    public enum ELoadType
    {
        Asset,      // 加载资源本身
        Instance,   // 加载并实例化
    }

    public interface IResLoadInfo
    {
        public string Path { get; set; }
        public int LoadCount { get; set; }
        public EResLoadStatus LoadStatus { get; set; }
        public string BundlePath { get; set; }
    }
    
    public class ResLoadInfo<T> : IResLoadInfo where T: UnityEngine.Object
    {
        public T Asset;
        public string Path { get; set; }
        public int LoadCount { get; set; }
        public EResLoadStatus LoadStatus { get; set; }
        public string BundlePath { get; set; }
        
        public bool IsLoaded()
        {
            return this is { LoadStatus: EResLoadStatus.Loaded };
        }

        public bool IsLoading()
        {
            return this is { LoadStatus: EResLoadStatus.Loading };
        }

        public bool NeedUnload()
        {
            return !IsLoading() && LoadCount <= 0;
        }
    }
}
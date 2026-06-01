using System;
using System.Collections.Generic;

namespace Air.UnityGameCore.Editor.AssetDependency
{
    /// <summary>
    /// 资源依赖缓存数据
    /// 可序列化用于持久化存储
    /// </summary>
    [Serializable]
    public class AssetDependencyCacheData
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath;
        
        /// <summary>
        /// 资源 GUID
        /// </summary>
        public string AssetGuid;
        
        /// <summary>
        /// 文件最后修改时间（Ticks）
        /// </summary>
        public long LastModifiedTicks;
        
        /// <summary>
        /// 直接依赖列表
        /// </summary>
        public List<string> DirectDependencies = new List<string>();
        
        /// <summary>
        /// 所有依赖列表（递归）
        /// </summary>
        public List<string> AllDependencies = new List<string>();
        
        /// <summary>
        /// 反向依赖列表（哪些资源依赖此资源）
        /// </summary>
        public List<string> ReverseDependencies = new List<string>();
        
        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize;
        
        /// <summary>
        /// 资源类型
        /// </summary>
        public string AssetType;
        
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(AssetPath);
        
        /// <summary>
        /// 获取最后修改时间
        /// </summary>
        public DateTime LastModified => new DateTime(LastModifiedTicks);
    }
    
    /// <summary>
    /// 完整的缓存数据库
    /// </summary>
    [Serializable]
    public class AssetDependencyCacheDatabase
    {
        /// <summary>
        /// 缓存版本号（用于缓存格式升级）
        /// </summary>
        public int Version = 1;
        
        /// <summary>
        /// 缓存创建时间
        /// </summary>
        public long CreatedTimeTicks;
        
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public long LastUpdatedTicks;
        
        /// <summary>
        /// 资源缓存字典 (AssetPath -> CacheData)
        /// </summary>
        public Dictionary<string, AssetDependencyCacheData> AssetCache = new Dictionary<string, AssetDependencyCacheData>();
        
        /// <summary>
        /// GUID 到路径的映射
        /// </summary>
        public Dictionary<string, string> GuidToPathMap = new Dictionary<string, string>();
        
        /// <summary>
        /// 缓存统计信息
        /// </summary>
        public CacheStatistics Statistics = new CacheStatistics();
    }
    
    /// <summary>
    /// 缓存统计信息
    /// </summary>
    [Serializable]
    public class CacheStatistics
    {
        /// <summary>
        /// 总资源数
        /// </summary>
        public int TotalAssets;
        
        /// <summary>
        /// 缓存命中次数
        /// </summary>
        public int CacheHits;
        
        /// <summary>
        /// 缓存未命中次数
        /// </summary>
        public int CacheMisses;
        
        /// <summary>
        /// 增量更新次数
        /// </summary>
        public int IncrementalUpdates;
        
        /// <summary>
        /// 全量重建次数
        /// </summary>
        public int FullRebuilds;
        
        /// <summary>
        /// 命中率
        /// </summary>
        public float HitRate
        {
            get
            {
                int total = CacheHits + CacheMisses;
                return total > 0 ? (float)CacheHits / total : 0f;
            }
        }
    }
}



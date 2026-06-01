using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Air.UnityGameCore.Editor.AssetDependency
{
    /// <summary>
    /// 资源依赖缓存管理器
    /// 提供高性能的依赖查询和增量更新
    /// </summary>
    public class AssetDependencyCache
    {
        private static AssetDependencyCache _instance;
        private AssetDependencyCacheDatabase _database;
        private bool _isDirty;
        private bool _isBuilding;
        
        private const string CacheFilePath = "Library/AssetDependencyCache.json";
        private const int CacheVersion = 1;
        
        /// <summary>
        /// 单例实例
        /// </summary>
        public static AssetDependencyCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AssetDependencyCache();
                    _instance.Initialize();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 缓存统计信息
        /// </summary>
        public CacheStatistics Statistics => _database?.Statistics;
        
        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// 初始化缓存
        /// </summary>
        private void Initialize()
        {
            LoadCache();
            
            if (_database == null || !ValidateCache())
            {
                Debug.Log("[AssetDependencyCache] 缓存无效或不存在，将在首次使用时构建");
                _database = new AssetDependencyCacheDatabase
                {
                    CreatedTimeTicks = DateTime.Now.Ticks,
                    LastUpdatedTicks = DateTime.Now.Ticks
                };
            }
            
            IsInitialized = true;
        }
        
        /// <summary>
        /// 验证缓存有效性
        /// </summary>
        private bool ValidateCache()
        {
            if (_database == null)
                return false;
            
            if (_database.Version != CacheVersion)
            {
                Debug.Log($"[AssetDependencyCache] 缓存版本不匹配 (当前: {_database.Version}, 需要: {CacheVersion})");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 加载缓存
        /// </summary>
        private void LoadCache()
        {
            try
            {
                if (File.Exists(CacheFilePath))
                {
                    string json = File.ReadAllText(CacheFilePath);
                    _database = JsonUtility.FromJson<AssetDependencyCacheDatabase>(json);
                    Debug.Log($"[AssetDependencyCache] 已加载缓存，包含 {_database.AssetCache.Count} 个资源");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AssetDependencyCache] 加载缓存失败: {e.Message}");
                _database = null;
            }
        }
        
        /// <summary>
        /// 保存缓存
        /// </summary>
        public void SaveCache()
        {
            if (_database == null || !_isDirty)
                return;
            
            try
            {
                _database.LastUpdatedTicks = DateTime.Now.Ticks;
                string json = JsonUtility.ToJson(_database, false);
                File.WriteAllText(CacheFilePath, json);
                _isDirty = false;
                Debug.Log($"[AssetDependencyCache] 已保存缓存，包含 {_database.AssetCache.Count} 个资源");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetDependencyCache] 保存缓存失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 构建完整缓存
        /// </summary>
        public void BuildFullCache(Action<float, string> progressCallback = null)
        {
            if (_isBuilding)
            {
                Debug.LogWarning("[AssetDependencyCache] 缓存正在构建中");
                return;
            }
            
            _isBuilding = true;
            
            try
            {
                _database = new AssetDependencyCacheDatabase
                {
                    Version = CacheVersion,
                    CreatedTimeTicks = DateTime.Now.Ticks,
                    LastUpdatedTicks = DateTime.Now.Ticks
                };
                
                // 获取所有资源
                string[] allAssets = AssetDatabase.GetAllAssetPaths()
                    .Where(path => path.StartsWith("Assets/") || path.StartsWith("Packages/"))
                    .Where(path => !AssetDependencyAnalyzer.IsBuiltInAsset(path))
                    .Where(path => File.Exists(path))
                    .ToArray();
                
                Debug.Log($"[AssetDependencyCache] 开始构建缓存，共 {allAssets.Length} 个资源");
                
                // 第一阶段：构建直接依赖
                for (int i = 0; i < allAssets.Length; i++)
                {
                    string assetPath = allAssets[i];
                    float progress = (float)i / allAssets.Length;
                    
                    progressCallback?.Invoke(progress * 0.5f, $"分析依赖 ({i + 1}/{allAssets.Length}): {Path.GetFileName(assetPath)}");
                    
                    BuildCacheForAsset(assetPath);
                }
                
                // 第二阶段：构建反向依赖
                progressCallback?.Invoke(0.5f, "构建反向依赖索引...");
                BuildReverseDependencies();
                
                _database.Statistics.TotalAssets = _database.AssetCache.Count;
                _database.Statistics.FullRebuilds++;
                
                _isDirty = true;
                SaveCache();
                
                Debug.Log($"[AssetDependencyCache] 缓存构建完成，共 {_database.AssetCache.Count} 个资源");
            }
            finally
            {
                _isBuilding = false;
            }
        }
        
        /// <summary>
        /// 为单个资源构建缓存
        /// </summary>
        private void BuildCacheForAsset(string assetPath)
        {
            try
            {
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                FileInfo fileInfo = new FileInfo(assetPath);
                
                AssetDependencyCacheData cacheData = new AssetDependencyCacheData
                {
                    AssetPath = assetPath,
                    AssetGuid = guid,
                    LastModifiedTicks = fileInfo.LastWriteTime.Ticks,
                    FileSize = fileInfo.Length,
                    AssetType = Path.GetExtension(assetPath)
                };
                
                // 获取直接依赖
                string[] directDeps = AssetDatabase.GetDependencies(assetPath, false);
                cacheData.DirectDependencies = directDeps
                    .Where(dep => dep != assetPath)
                    .Where(dep => !AssetDependencyAnalyzer.IsBuiltInAsset(dep))
                    .ToList();
                
                // 获取所有依赖（递归）
                string[] allDeps = AssetDatabase.GetDependencies(assetPath, true);
                cacheData.AllDependencies = allDeps
                    .Where(dep => dep != assetPath)
                    .Where(dep => !AssetDependencyAnalyzer.IsBuiltInAsset(dep))
                    .ToList();
                
                _database.AssetCache[assetPath] = cacheData;
                _database.GuidToPathMap[guid] = assetPath;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AssetDependencyCache] 构建资源缓存失败 {assetPath}: {e.Message}");
            }
        }
        
        /// <summary>
        /// 构建反向依赖索引
        /// </summary>
        private void BuildReverseDependencies()
        {
            // 清空现有的反向依赖
            foreach (var cache in _database.AssetCache.Values)
            {
                cache.ReverseDependencies.Clear();
            }
            
            // 重新构建反向依赖
            foreach (var kvp in _database.AssetCache)
            {
                string assetPath = kvp.Key;
                var cacheData = kvp.Value;
                
                // 为每个依赖项添加反向引用
                foreach (string dependency in cacheData.DirectDependencies)
                {
                    if (_database.AssetCache.TryGetValue(dependency, out var depCache))
                    {
                        if (!depCache.ReverseDependencies.Contains(assetPath))
                        {
                            depCache.ReverseDependencies.Add(assetPath);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 增量更新资源缓存
        /// </summary>
        public void UpdateAsset(string assetPath)
        {
            if (!IsEnabled || _isBuilding)
                return;
            
            // 确保缓存已初始化
            if (_database.AssetCache.Count == 0)
            {
                return;
            }
            
            // 移除旧的反向依赖引用
            if (_database.AssetCache.TryGetValue(assetPath, out var oldCache))
            {
                foreach (string dep in oldCache.DirectDependencies)
                {
                    if (_database.AssetCache.TryGetValue(dep, out var depCache))
                    {
                        depCache.ReverseDependencies.Remove(assetPath);
                    }
                }
            }
            
            // 重新构建资源缓存
            BuildCacheForAsset(assetPath);
            
            // 更新反向依赖
            if (_database.AssetCache.TryGetValue(assetPath, out var newCache))
            {
                foreach (string dep in newCache.DirectDependencies)
                {
                    if (_database.AssetCache.TryGetValue(dep, out var depCache))
                    {
                        if (!depCache.ReverseDependencies.Contains(assetPath))
                        {
                            depCache.ReverseDependencies.Add(assetPath);
                        }
                    }
                }
            }
            
            _database.Statistics.IncrementalUpdates++;
            _isDirty = true;
        }
        
        /// <summary>
        /// 删除资源缓存
        /// </summary>
        public void RemoveAsset(string assetPath)
        {
            if (!IsEnabled || _isBuilding)
                return;
            
            if (_database.AssetCache.TryGetValue(assetPath, out var cache))
            {
                // 从反向依赖中移除
                foreach (string dep in cache.DirectDependencies)
                {
                    if (_database.AssetCache.TryGetValue(dep, out var depCache))
                    {
                        depCache.ReverseDependencies.Remove(assetPath);
                    }
                }
                
                // 移除缓存
                _database.AssetCache.Remove(assetPath);
                _database.GuidToPathMap.Remove(cache.AssetGuid);
                
                _isDirty = true;
            }
        }
        
        /// <summary>
        /// 获取资源依赖（使用缓存）
        /// </summary>
        public List<string> GetDependencies(string assetPath, bool recursive)
        {
            if (!IsEnabled || _database == null)
            {
                _database.Statistics.CacheMisses++;
                return null;
            }
            
            if (_database.AssetCache.TryGetValue(assetPath, out var cache))
            {
                _database.Statistics.CacheHits++;
                return recursive ? new List<string>(cache.AllDependencies) : new List<string>(cache.DirectDependencies);
            }
            
            _database.Statistics.CacheMisses++;
            return null;
        }
        
        /// <summary>
        /// 获取反向依赖（使用缓存）
        /// </summary>
        public List<string> GetReverseDependencies(string assetPath)
        {
            if (!IsEnabled || _database == null)
            {
                _database.Statistics.CacheMisses++;
                return null;
            }
            
            if (_database.AssetCache.TryGetValue(assetPath, out var cache))
            {
                _database.Statistics.CacheHits++;
                return new List<string>(cache.ReverseDependencies);
            }
            
            _database.Statistics.CacheMisses++;
            return null;
        }
        
        /// <summary>
        /// 检查资源是否需要更新
        /// </summary>
        public bool NeedsUpdate(string assetPath)
        {
            if (!File.Exists(assetPath))
                return false;
            
            if (!_database.AssetCache.TryGetValue(assetPath, out var cache))
                return true;
            
            FileInfo fileInfo = new FileInfo(assetPath);
            return fileInfo.LastWriteTime.Ticks != cache.LastModifiedTicks;
        }
        
        /// <summary>
        /// 清空缓存
        /// </summary>
        public void ClearCache()
        {
            _database = new AssetDependencyCacheDatabase
            {
                Version = CacheVersion,
                CreatedTimeTicks = DateTime.Now.Ticks,
                LastUpdatedTicks = DateTime.Now.Ticks
            };
            _isDirty = true;
            
            if (File.Exists(CacheFilePath))
            {
                File.Delete(CacheFilePath);
            }
            
            Debug.Log("[AssetDependencyCache] 缓存已清空");
        }
    }
}



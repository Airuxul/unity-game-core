using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Air.UnityGameCore.Editor.AssetDependency
{
    /// <summary>
    /// 资源依赖分析器
    /// 提供资源依赖和反依赖查询功能
    /// </summary>
    public static class AssetDependencyAnalyzer
    {
        /// <summary>
        /// 获取指定资源的所有依赖项
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="recursive">是否递归查找</param>
        /// <returns>依赖资源路径列表</returns>
        public static List<string> GetDependencies(string assetPath, bool recursive = true)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                return new List<string>();
            }

            // 尝试从缓存获取
            if (AssetDependencyCache.Instance.IsEnabled)
            {
                var cachedResult = AssetDependencyCache.Instance.GetDependencies(assetPath, recursive);
                if (cachedResult != null)
                {
                    return cachedResult.OrderBy(dep => dep).ToList();
                }
            }

            // 缓存未命中，使用 Unity API 查询
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, recursive);
            
            // 过滤掉自身和内置资源
            return dependencies
                .Where(dep => dep != assetPath)
                .Where(dep => !IsBuiltInAsset(dep))
                .OrderBy(dep => dep)
                .ToList();
        }

        /// <summary>
        /// 获取指定资源的直接依赖项（非递归）
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>直接依赖资源路径列表</returns>
        public static List<string> GetDirectDependencies(string assetPath)
        {
            return GetDependencies(assetPath, false);
        }

        /// <summary>
        /// 获取依赖指定资源的所有资源（反向依赖）
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>依赖该资源的资源路径列表</returns>
        public static List<string> GetReverseDependencies(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                return new List<string>();
            }

            // 尝试从缓存获取
            if (AssetDependencyCache.Instance.IsEnabled)
            {
                var cachedResult = AssetDependencyCache.Instance.GetReverseDependencies(assetPath);
                if (cachedResult != null)
                {
                    return cachedResult.OrderBy(dep => dep).ToList();
                }
            }

            // 缓存未命中，使用传统方式查询
            List<string> reverseDeps = new List<string>();
            
            // 获取所有资源路径
            string[] allAssets = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/") || path.StartsWith("Packages/"))
                .Where(path => !IsBuiltInAsset(path))
                .ToArray();

            // 检查每个资源是否依赖目标资源
            foreach (string asset in allAssets)
            {
                if (asset == assetPath)
                    continue;

                string[] dependencies = AssetDatabase.GetDependencies(asset, false);
                if (dependencies.Contains(assetPath))
                {
                    reverseDeps.Add(asset);
                }
            }

            return reverseDeps.OrderBy(dep => dep).ToList();
        }

        /// <summary>
        /// 获取资源的依赖树结构
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="maxDepth">最大深度，-1表示无限制</param>
        /// <returns>依赖树节点</returns>
        public static DependencyNode BuildDependencyTree(string assetPath, int maxDepth = -1)
        {
            HashSet<string> visited = new HashSet<string>();
            return BuildDependencyTreeRecursive(assetPath, visited, 0, maxDepth);
        }

        private static DependencyNode BuildDependencyTreeRecursive(
            string assetPath, 
            HashSet<string> visited, 
            int currentDepth, 
            int maxDepth)
        {
            DependencyNode node = new DependencyNode
            {
                AssetPath = assetPath,
                AssetName = Path.GetFileName(assetPath),
                Depth = currentDepth
            };

            if (visited.Contains(assetPath) || (maxDepth >= 0 && currentDepth >= maxDepth))
            {
                node.IsCircular = visited.Contains(assetPath);
                return node;
            }

            visited.Add(assetPath);

            List<string> directDeps = GetDirectDependencies(assetPath);
            foreach (string dep in directDeps)
            {
                DependencyNode childNode = BuildDependencyTreeRecursive(dep, visited, currentDepth + 1, maxDepth);
                node.Children.Add(childNode);
            }

            visited.Remove(assetPath);
            return node;
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>资源信息</returns>
        public static AssetInfo GetAssetInfo(string assetPath)
        {
            AssetInfo info = new AssetInfo
            {
                AssetPath = assetPath,
                AssetName = Path.GetFileName(assetPath),
                AssetType = Path.GetExtension(assetPath)
            };

            if (File.Exists(assetPath))
            {
                FileInfo fileInfo = new FileInfo(assetPath);
                info.FileSize = fileInfo.Length;
                info.LastModified = fileInfo.LastWriteTime;

                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset != null)
                {
                    info.AssetTypeName = asset.GetType().Name;
                }
            }

            return info;
        }

        /// <summary>
        /// 判断是否为内置资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>是否为内置资源</returns>
        public static bool IsBuiltInAsset(string assetPath)
        {
            return assetPath.StartsWith("Resources/") || 
                   assetPath.StartsWith("Library/") ||
                   assetPath.Contains("unity default resources") ||
                   assetPath.Contains("unity_builtin_extra");
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// 依赖树节点
    /// </summary>
    public class DependencyNode
    {
        public string AssetPath { get; set; }
        public string AssetName { get; set; }
        public int Depth { get; set; }
        public bool IsCircular { get; set; }
        public List<DependencyNode> Children { get; set; } = new List<DependencyNode>();
    }

    /// <summary>
    /// 资源信息
    /// </summary>
    public class AssetInfo
    {
        public string AssetPath { get; set; }
        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string AssetTypeName { get; set; }
        public long FileSize { get; set; }
        public System.DateTime LastModified { get; set; }
    }
}


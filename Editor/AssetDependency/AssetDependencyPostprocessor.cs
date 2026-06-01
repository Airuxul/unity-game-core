using UnityEditor;
using UnityEngine;

namespace Air.UnityGameCore.Editor.AssetDependency
{
    /// <summary>
    /// 资源依赖监听器
    /// 监听资源的导入、删除、移动，自动更新缓存
    /// </summary>
    public class AssetDependencyPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// 资源导入完成后调用（批量处理）
        /// </summary>
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!AssetDependencyCache.Instance.IsEnabled)
                return;
            
            bool hasChanges = false;
            
            // 处理导入的资源
            if (importedAssets.Length > 0)
            {
                foreach (string assetPath in importedAssets)
                {
                    if (IsValidAsset(assetPath))
                    {
                        AssetDependencyCache.Instance.UpdateAsset(assetPath);
                        hasChanges = true;
                    }
                }
            }
            
            // 处理删除的资源
            if (deletedAssets.Length > 0)
            {
                foreach (string assetPath in deletedAssets)
                {
                    AssetDependencyCache.Instance.RemoveAsset(assetPath);
                    hasChanges = true;
                }
            }
            
            // 处理移动的资源
            if (movedAssets.Length > 0)
            {
                for (int i = 0; i < movedAssets.Length; i++)
                {
                    string oldPath = movedFromAssetPaths[i];
                    string newPath = movedAssets[i];
                    
                    // 先删除旧路径
                    AssetDependencyCache.Instance.RemoveAsset(oldPath);
                    
                    // 再添加新路径
                    if (IsValidAsset(newPath))
                    {
                        AssetDependencyCache.Instance.UpdateAsset(newPath);
                    }
                    
                    hasChanges = true;
                }
            }
            
            // 保存缓存
            if (hasChanges)
            {
                AssetDependencyCache.Instance.SaveCache();
            }
        }
        
        /// <summary>
        /// 判断是否为有效的资源（需要缓存的）
        /// </summary>
        private static bool IsValidAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
            
            // 只处理 Assets 和 Packages 下的资源
            if (!assetPath.StartsWith("Assets/") && !assetPath.StartsWith("Packages/"))
                return false;
            
            // 排除内置资源
            if (AssetDependencyAnalyzer.IsBuiltInAsset(assetPath))
                return false;
            
            // 排除 .meta 文件
            if (assetPath.EndsWith(".meta"))
                return false;
            
            return true;
        }
    }
    
    /// <summary>
    /// 编辑器初始化回调
    /// 在编辑器启动时自动初始化缓存
    /// </summary>
    [InitializeOnLoad]
    public static class AssetDependencyCacheInitializer
    {
        static AssetDependencyCacheInitializer()
        {
            // 编辑器退出时保存缓存
            EditorApplication.quitting += () =>
            {
                AssetDependencyCache.Instance.SaveCache();
            };
        }
    }
}



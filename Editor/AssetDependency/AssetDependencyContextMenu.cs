using UnityEditor;
using UnityEngine;

namespace Air.UnityGameCore.Editor.AssetDependency
{
    /// <summary>
    /// 资源依赖查询的右键菜单扩展
    /// </summary>
    public static class AssetDependencyContextMenu
    {
        /// <summary>
        /// 在Project窗口中添加"查看依赖"菜单项
        /// </summary>
        [MenuItem("Assets/资源依赖/查看依赖关系", false, 1000)]
        private static void ShowDependenciesFromContextMenu()
        {
            if (Selection.activeObject != null)
            {
                AssetDependencyWindow.ShowWindow(Selection.activeObject);
            }
        }

        /// <summary>
        /// 验证菜单项是否可用
        /// </summary>
        [MenuItem("Assets/资源依赖/查看依赖关系", true)]
        private static bool ValidateShowDependenciesFromContextMenu()
        {
            return Selection.activeObject != null;
        }

        /// <summary>
        /// 快速分析依赖并显示在控制台
        /// </summary>
        [MenuItem("Assets/资源依赖/打印依赖到控制台", false, 1001)]
        private static void PrintDependenciesToConsole()
        {
            if (Selection.activeObject == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var dependencies = AssetDependencyAnalyzer.GetDependencies(assetPath, true);

            Debug.Log($"=== {Selection.activeObject.name} 的依赖项 ({dependencies.Count}) ===");
            foreach (string dep in dependencies)
            {
                Debug.Log($"  - {dep}");
            }
        }

        /// <summary>
        /// 验证打印依赖菜单项是否可用
        /// </summary>
        [MenuItem("Assets/资源依赖/打印依赖到控制台", true)]
        private static bool ValidatePrintDependenciesToConsole()
        {
            return Selection.activeObject != null;
        }

        /// <summary>
        /// 快速分析反向依赖并显示在控制台
        /// </summary>
        [MenuItem("Assets/资源依赖/打印反向依赖到控制台", false, 1002)]
        private static void PrintReverseDependenciesToConsole()
        {
            if (Selection.activeObject == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            
            EditorUtility.DisplayProgressBar("分析中", "正在查找反向依赖...", 0.5f);
            
            try
            {
                var reverseDeps = AssetDependencyAnalyzer.GetReverseDependencies(assetPath);

                Debug.Log($"=== 依赖 {Selection.activeObject.name} 的资源 ({reverseDeps.Count}) ===");
                if (reverseDeps.Count == 0)
                {
                    Debug.Log("  没有资源依赖该资源");
                }
                else
                {
                    foreach (string dep in reverseDeps)
                    {
                        Debug.Log($"  - {dep}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// 验证打印反向依赖菜单项是否可用
        /// </summary>
        [MenuItem("Assets/资源依赖/打印反向依赖到控制台", true)]
        private static bool ValidatePrintReverseDependenciesToConsole()
        {
            return Selection.activeObject != null;
        }
    }
}


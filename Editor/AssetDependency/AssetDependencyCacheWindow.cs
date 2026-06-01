using UnityEditor;
using UnityEngine;

namespace Air.UnityGameCore.Editor.AssetDependency
{
    /// <summary>
    /// 资源依赖缓存管理窗口
    /// </summary>
    public class AssetDependencyCacheWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _isBuilding;
        private float _buildProgress;
        private string _buildStatus;

        [MenuItem("Tools/资源依赖/资源依赖缓存管理")]
        public static void ShowWindow()
        {
            AssetDependencyCacheWindow window = GetWindow<AssetDependencyCacheWindow>();
            window.titleContent = new GUIContent("依赖缓存管理");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawCacheStatus();
            EditorGUILayout.Space(10);

            DrawStatistics();
            EditorGUILayout.Space(10);

            DrawActions();
            EditorGUILayout.Space(10);

            DrawSettings();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制标题
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField("资源依赖缓存管理", titleStyle);

            EditorGUILayout.LabelField("通过缓存加速依赖查询，支持增量更新", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制缓存状态
        /// </summary>
        private void DrawCacheStatus()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("缓存状态", EditorStyles.boldLabel);

            var cache = AssetDependencyCache.Instance;
            var stats = cache.Statistics;

            if (stats != null && stats.TotalAssets > 0)
            {
                EditorGUILayout.LabelField($"状态: ", "✓ 已初始化", EditorStyles.label);
                EditorGUILayout.LabelField($"启用: ", cache.IsEnabled ? "✓ 是" : "✗ 否");
                EditorGUILayout.LabelField($"缓存资源数: ", $"{stats.TotalAssets:N0} 个");

                // 计算缓存文件大小
                string cacheFilePath = "Library/AssetDependencyCache.json";
                if (System.IO.File.Exists(cacheFilePath))
                {
                    var fileInfo = new System.IO.FileInfo(cacheFilePath);
                    string fileSize = AssetDependencyAnalyzer.FormatFileSize(fileInfo.Length);
                    EditorGUILayout.LabelField($"缓存文件大小: ", fileSize);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("缓存未构建或为空", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制统计信息
        /// </summary>
        private void DrawStatistics()
        {
            var stats = AssetDependencyCache.Instance.Statistics;
            if (stats == null || stats.TotalAssets == 0)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("性能统计", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"缓存命中: ", $"{stats.CacheHits:N0} 次");
            EditorGUILayout.LabelField($"缓存未命中: ", $"{stats.CacheMisses:N0} 次");
            EditorGUILayout.LabelField($"命中率: ", $"{stats.HitRate:P1}");

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField($"增量更新: ", $"{stats.IncrementalUpdates:N0} 次");
            EditorGUILayout.LabelField($"全量重建: ", $"{stats.FullRebuilds:N0} 次");

            // 性能提示
            if (stats.HitRate > 0.8f)
            {
                EditorGUILayout.HelpBox("✓ 缓存命中率良好，查询性能优秀", MessageType.Info);
            }
            else if (stats.HitRate > 0.5f)
            {
                EditorGUILayout.HelpBox("缓存命中率一般，考虑重建缓存", MessageType.Warning);
            }
            else if (stats.CacheHits + stats.CacheMisses > 10)
            {
                EditorGUILayout.HelpBox("缓存命中率低，建议重建缓存", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制操作按钮
        /// </summary>
        private void DrawActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("缓存操作", EditorStyles.boldLabel);

            // 构建缓存按钮
            GUI.enabled = !_isBuilding;
            if (GUILayout.Button("构建完整缓存", GUILayout.Height(30)))
            {
                BuildFullCache();
            }
            GUI.enabled = true;

            // 显示构建进度
            if (_isBuilding)
            {
                EditorGUILayout.Space(5);
                EditorGUI.ProgressBar(
                    EditorGUILayout.GetControlRect(false, 20),
                    _buildProgress,
                    _buildStatus
                );
                Repaint();
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            // 保存缓存按钮
            if (GUILayout.Button("保存缓存", GUILayout.Height(25)))
            {
                AssetDependencyCache.Instance.SaveCache();
                ShowNotification(new GUIContent("缓存已保存"));
            }

            // 清空缓存按钮
            if (GUILayout.Button("清空缓存", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清空所有缓存吗？", "确定", "取消"))
                {
                    AssetDependencyCache.Instance.ClearCache();
                    ShowNotification(new GUIContent("缓存已清空"));
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制设置
        /// </summary>
        private void DrawSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("设置", EditorStyles.boldLabel);

            // 启用缓存开关
            bool isEnabled = AssetDependencyCache.Instance.IsEnabled;
            bool newEnabled = EditorGUILayout.Toggle("启用缓存", isEnabled);

            if (newEnabled != isEnabled)
            {
                AssetDependencyCache.Instance.IsEnabled = newEnabled;
                ShowNotification(new GUIContent(newEnabled ? "缓存已启用" : "缓存已禁用"));
            }

            EditorGUILayout.HelpBox(
                "启用缓存后，依赖查询将优先使用缓存数据。\n" +
                "文件修改会自动触发增量更新。",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 构建完整缓存
        /// </summary>
        private void BuildFullCache()
        {
            _isBuilding = true;
            _buildProgress = 0f;
            _buildStatus = "准备构建...";

            try
            {
                AssetDependencyCache.Instance.BuildFullCache((progress, status) =>
                {
                    _buildProgress = progress;
                    _buildStatus = status;
                    Repaint();
                });

                ShowNotification(new GUIContent("缓存构建完成"));
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"构建缓存失败: {e.Message}", "确定");
                Debug.LogError($"构建缓存失败: {e}");
            }
            finally
            {
                _isBuilding = false;
                _buildProgress = 0f;
                _buildStatus = "";
            }
        }

        /// <summary>
        /// 定期刷新窗口（显示实时统计）
        /// </summary>
        private void Update()
        {
            // 每秒刷新一次
            if (EditorApplication.timeSinceStartup % 1 < 0.1)
            {
                Repaint();
            }
        }
    }
}



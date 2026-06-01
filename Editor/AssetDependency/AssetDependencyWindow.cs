using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Air.UnityGameCore.Editor.AssetDependency
{
    /// <summary>
    /// 资源依赖查询窗口
    /// 使用 UIToolkit 实现的编辑器工具
    /// </summary>
    public class AssetDependencyWindow : EditorWindow
    {
        private Object _targetAsset;
        private string _targetAssetPath;
        
        private ObjectField _assetField;
        private Label _assetInfoLabel;
        private Button _analyzeDependenciesBtn;
        private Button _analyzeReverseDependenciesBtn;
        private Button _clearBtn;
        private Toggle _recursiveToggle;
        
        private ScrollView _resultScrollView;
        private Label _resultCountLabel;
        
        private List<string> _currentResults = new();

        [MenuItem("Tools/资源依赖/资源依赖查询工具")]
        public static void ShowWindow()
        {
            AssetDependencyWindow window = GetWindow<AssetDependencyWindow>();
            window.titleContent = new GUIContent("资源依赖查询");
            window.minSize = new Vector2(600, 400);
        }
        
        /// <summary>
        /// 打开窗口并设置目标资源
        /// </summary>
        /// <param name="asset">要分析的资源</param>
        public static void ShowWindow(Object asset)
        {
            AssetDependencyWindow window = GetWindow<AssetDependencyWindow>();
            window.titleContent = new GUIContent("资源依赖查询");
            window.minSize = new Vector2(600, 400);
            window.Show();
            window.SetTargetAsset(asset);
        }
        
        /// <summary>
        /// 设置目标资源
        /// </summary>
        /// <param name="asset">要分析的资源</param>
        public void SetTargetAsset(Object asset)
        {
            if (_assetField != null)
            {
                _assetField.value = asset;
            }
        }

        public void CreateGUI()
        {
            // 加载UXML
            string uxmlPath = GetAssetPath("AssetDependencyWindow.uxml");
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            
            if (visualTree == null)
            {
                Debug.LogError($"无法加载UXML文件: {uxmlPath}");
                return;
            }
            
            visualTree.CloneTree(rootVisualElement);
            
            // 加载USS
            string ussPath = GetAssetPath("AssetDependencyWindow.uss");
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning($"无法加载USS文件: {ussPath}");
            }
            
            // 绑定UI元素
            BindUIElements();
            
            // 注册事件
            RegisterEvents();
        }
        
        /// <summary>
        /// 获取资源路径
        /// </summary>
        private string GetAssetPath(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets($"{Path.GetFileNameWithoutExtension(fileName)} t:Script");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.Contains("AssetDependency"))
                {
                    string directory = Path.GetDirectoryName(assetPath);
                    string targetPath = Path.Combine(directory, fileName).Replace("\\", "/");
                    if (File.Exists(targetPath))
                    {
                        return targetPath;
                    }
                }
            }
            
            // 备用路径
            return $"CustomPackages/packages/com.air.unity-game-core/Editor/AssetDependency/{fileName}";
        }
        
        /// <summary>
        /// 绑定UI元素
        /// </summary>
        private void BindUIElements()
        {
            _assetField = rootVisualElement.Q<ObjectField>("asset-field");
            _assetInfoLabel = rootVisualElement.Q<Label>("asset-info-label");
            _analyzeDependenciesBtn = rootVisualElement.Q<Button>("analyze-dependencies-btn");
            _analyzeReverseDependenciesBtn = rootVisualElement.Q<Button>("analyze-reverse-dependencies-btn");
            _clearBtn = rootVisualElement.Q<Button>("clear-btn");
            _recursiveToggle = rootVisualElement.Q<Toggle>("recursive-toggle");
            _resultScrollView = rootVisualElement.Q<ScrollView>("result-scroll-view");
            _resultCountLabel = rootVisualElement.Q<Label>("result-count-label");
            
            // 设置ObjectField属性
            if (_assetField != null)
            {
                _assetField.objectType = typeof(Object);
                _assetField.allowSceneObjects = false;
            }
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            if (_assetField != null)
            {
                _assetField.RegisterValueChangedCallback(OnAssetChanged);
            }
            
            if (_analyzeDependenciesBtn != null)
            {
                _analyzeDependenciesBtn.clicked += AnalyzeDependencies;
            }
            
            if (_analyzeReverseDependenciesBtn != null)
            {
                _analyzeReverseDependenciesBtn.clicked += AnalyzeReverseDependencies;
            }
            
            if (_clearBtn != null)
            {
                _clearBtn.clicked += ClearResults;
            }
        }


        /// <summary>
        /// 资源变更回调
        /// </summary>
        /// <param name="evt">变更事件</param>
        private void OnAssetChanged(ChangeEvent<Object> evt)
        {
            _targetAsset = evt.newValue;
            
            if (_targetAsset != null)
            {
                _targetAssetPath = AssetDatabase.GetAssetPath(_targetAsset);
                UpdateAssetInfo();
            }
            else
            {
                _targetAssetPath = null;
                _assetInfoLabel.text = "请选择一个资源";
            }
            
            ClearResults();
        }

        /// <summary>
        /// 更新资源信息显示
        /// </summary>
        private void UpdateAssetInfo()
        {
            if (string.IsNullOrEmpty(_targetAssetPath))
                return;

            AssetInfo info = AssetDependencyAnalyzer.GetAssetInfo(_targetAssetPath);
            string sizeStr = AssetDependencyAnalyzer.FormatFileSize(info.FileSize);
            
            _assetInfoLabel.text = $"路径: {info.AssetPath}\n" +
                                   $"类型: {info.AssetTypeName} ({info.AssetType})\n" +
                                   $"大小: {sizeStr}";
        }

        /// <summary>
        /// 分析依赖
        /// </summary>
        private void AnalyzeDependencies()
        {
            if (string.IsNullOrEmpty(_targetAssetPath))
            {
                EditorUtility.DisplayDialog("提示", "请先选择一个资源", "确定");
                return;
            }

            bool recursive = _recursiveToggle != null && _recursiveToggle.value;
            _currentResults = AssetDependencyAnalyzer.GetDependencies(_targetAssetPath, recursive);
            
            DisplayResults($"{_targetAsset.name} 的依赖项");
        }

        /// <summary>
        /// 分析反向依赖
        /// </summary>
        private void AnalyzeReverseDependencies()
        {
            if (string.IsNullOrEmpty(_targetAssetPath))
            {
                EditorUtility.DisplayDialog("提示", "请先选择一个资源", "确定");
                return;
            }

            EditorUtility.DisplayProgressBar("分析中", "正在查找反向依赖...", 0.5f);
            
            try
            {
                _currentResults = AssetDependencyAnalyzer.GetReverseDependencies(_targetAssetPath);
                DisplayResults($"依赖 {_targetAsset.name} 的资源");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// 显示结果
        /// </summary>
        /// <param name="title">结果标题</param>
        private void DisplayResults(string title)
        {
            if (_resultScrollView == null || _resultCountLabel == null)
                return;
                
            _resultScrollView.Clear();
            _resultCountLabel.text = $"{_currentResults.Count} 个结果";

            if (_currentResults.Count == 0)
            {
                Label emptyLabel = new Label("未找到任何依赖");
                emptyLabel.AddToClassList("empty-label");
                _resultScrollView.Add(emptyLabel);
                return;
            }

            // 按类型分组
            var groupedResults = _currentResults
                .GroupBy(path => Path.GetExtension(path))
                .OrderBy(g => g.Key);

            foreach (var group in groupedResults)
            {
                // 分组标题
                Label groupLabel = new Label($"{group.Key} ({group.Count()})");
                groupLabel.AddToClassList("group-label");
                _resultScrollView.Add(groupLabel);

                // 分组项
                foreach (string assetPath in group)
                {
                    VisualElement itemContainer = CreateResultItem(assetPath);
                    _resultScrollView.Add(itemContainer);
                }
            }
        }

        /// <summary>
        /// 创建结果项
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>结果项视觉元素</returns>
        private VisualElement CreateResultItem(string assetPath)
        {
            VisualElement itemContainer = new VisualElement();
            itemContainer.AddToClassList("result-item");

            // 资源图标和名称
            VisualElement leftContainer = new VisualElement();
            leftContainer.AddToClassList("result-item-left");

            Texture2D icon = AssetDatabase.GetCachedIcon(assetPath) as Texture2D;
            
            Image iconImage = new Image();
            iconImage.image = icon;
            iconImage.AddToClassList("result-item-icon");
            leftContainer.Add(iconImage);

            Label nameLabel = new Label(Path.GetFileName(assetPath));
            nameLabel.AddToClassList("result-item-name");
            leftContainer.Add(nameLabel);

            itemContainer.Add(leftContainer);

            // 操作按钮
            VisualElement buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("result-item-buttons");

            Button selectBtn = new Button(() => SelectAsset(assetPath))
            {
                text = "选择"
            };
            selectBtn.AddToClassList("result-item-button");
            buttonContainer.Add(selectBtn);

            Button pingBtn = new Button(() => PingAsset(assetPath))
            {
                text = "定位"
            };
            pingBtn.AddToClassList("result-item-button");
            buttonContainer.Add(pingBtn);

            itemContainer.Add(buttonContainer);

            return itemContainer;
        }

        /// <summary>
        /// 选择资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        private void SelectAsset(string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
            }
        }

        /// <summary>
        /// 在Project窗口中定位资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        private void PingAsset(string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
            }
        }

        /// <summary>
        /// 清除结果
        /// </summary>
        private void ClearResults()
        {
            _currentResults.Clear();
            
            if (_resultScrollView != null)
            {
                _resultScrollView.Clear();
            }
            
            if (_resultCountLabel != null)
            {
                _resultCountLabel.text = "0 个结果";
            }
        }
    }
}


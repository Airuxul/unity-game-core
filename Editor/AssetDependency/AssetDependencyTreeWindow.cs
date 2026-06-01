using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Air.UnityGameCore.Editor.AssetDependency
{
    /// <summary>
    /// 资源依赖树形视图窗口
    /// 使用 TreeView 展示层级依赖关系
    /// </summary>
    public class AssetDependencyTreeWindow : EditorWindow
    {
        private Object _targetAsset;
        private TreeViewState _treeViewState;
        private DependencyTreeView _treeView;
        private SearchField _searchField;
        private Vector2 _scrollPosition;
        
        private int _maxDepth = 5;
        private bool _showFileSize = true;

        [MenuItem("Tools/资源依赖/资源依赖树形视图")]
        public static void ShowWindow()
        {
            AssetDependencyTreeWindow window = GetWindow<AssetDependencyTreeWindow>();
            window.titleContent = new GUIContent("资源依赖树");
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable()
        {
            if (_treeViewState == null)
                _treeViewState = new TreeViewState();

            _treeView = new DependencyTreeView(_treeViewState);
            _searchField = new SearchField();
            _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawTreeView();
        }

        /// <summary>
        /// 绘制工具栏
        /// </summary>
        private void DrawToolbar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("资源依赖树形视图", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(5);

            // 资源选择
            EditorGUI.BeginChangeCheck();
            _targetAsset = EditorGUILayout.ObjectField("目标资源", _targetAsset, typeof(Object), false);
            if (EditorGUI.EndChangeCheck() && _targetAsset != null)
            {
                AnalyzeDependencyTree();
            }

            EditorGUILayout.Space(5);

            // 选项
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("最大深度", GUILayout.Width(70));
            _maxDepth = EditorGUILayout.IntSlider(_maxDepth, 1, 10);
            
            EditorGUILayout.Space(10);
            
            _showFileSize = EditorGUILayout.ToggleLeft("显示文件大小", _showFileSize, GUILayout.Width(100));
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 按钮
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("重新分析", GUILayout.Height(25)))
            {
                if (_targetAsset != null)
                {
                    AnalyzeDependencyTree();
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "请先选择一个资源", "确定");
                }
            }
            
            if (GUILayout.Button("展开全部", GUILayout.Height(25)))
            {
                _treeView?.ExpandAll();
            }
            
            if (GUILayout.Button("折叠全部", GUILayout.Height(25)))
            {
                _treeView?.CollapseAll();
            }
            
            if (GUILayout.Button("导出文本", GUILayout.Height(25)))
            {
                ExportToText();
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 搜索栏
            if (_treeView != null)
            {
                _treeView.searchString = _searchField.OnGUI(_treeView.searchString);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制树形视图
        /// </summary>
        private void DrawTreeView()
        {
            if (_treeView != null)
            {
                Rect rect = GUILayoutUtility.GetRect(0, position.height - 180, GUILayout.ExpandWidth(true));
                _treeView.OnGUI(rect);
            }
            else
            {
                EditorGUILayout.HelpBox("请选择一个资源开始分析", MessageType.Info);
            }
        }

        /// <summary>
        /// 分析依赖树
        /// </summary>
        private void AnalyzeDependencyTree()
        {
            string assetPath = AssetDatabase.GetAssetPath(_targetAsset);
            
            EditorUtility.DisplayProgressBar("分析中", "正在构建依赖树...", 0.5f);
            
            try
            {
                DependencyNode rootNode = AssetDependencyAnalyzer.BuildDependencyTree(assetPath, _maxDepth);
                _treeView.BuildTree(rootNode, _showFileSize);
                _treeView.ExpandAll();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// 导出为文本
        /// </summary>
        private void ExportToText()
        {
            if (_targetAsset == null)
            {
                EditorUtility.DisplayDialog("提示", "请先选择一个资源", "确定");
                return;
            }

            string path = EditorUtility.SaveFilePanel("导出依赖树", "", $"{_targetAsset.name}_dependencies.txt", "txt");
            if (string.IsNullOrEmpty(path))
                return;

            string assetPath = AssetDatabase.GetAssetPath(_targetAsset);
            DependencyNode rootNode = AssetDependencyAnalyzer.BuildDependencyTree(assetPath, _maxDepth);

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine($"资源依赖树: {_targetAsset.name}");
                writer.WriteLine($"路径: {assetPath}");
                writer.WriteLine($"生成时间: {System.DateTime.Now}");
                writer.WriteLine($"最大深度: {_maxDepth}");
                writer.WriteLine();
                writer.WriteLine("===================================");
                writer.WriteLine();
                
                WriteNodeToText(writer, rootNode, "", true);
            }

            EditorUtility.RevealInFinder(path);
            Debug.Log($"依赖树已导出到: {path}");
        }

        /// <summary>
        /// 将节点写入文本
        /// </summary>
        private void WriteNodeToText(StreamWriter writer, DependencyNode node, string indent, bool isLast)
        {
            string prefix = isLast ? "└─ " : "├─ ";
            writer.WriteLine($"{indent}{prefix}{node.AssetName}");
            
            if (node.IsCircular)
            {
                writer.WriteLine($"{indent}   (循环依赖)");
                return;
            }

            string childIndent = indent + (isLast ? "   " : "│  ");
            
            for (int i = 0; i < node.Children.Count; i++)
            {
                bool isLastChild = i == node.Children.Count - 1;
                WriteNodeToText(writer, node.Children[i], childIndent, isLastChild);
            }
        }

        /// <summary>
        /// 依赖树视图
        /// </summary>
        private class DependencyTreeView : TreeView
        {
            private DependencyNode _rootNode;
            private bool _showFileSize;

            public DependencyTreeView(TreeViewState state) : base(state)
            {
                Reload();
            }

            public void BuildTree(DependencyNode rootNode, bool showFileSize)
            {
                _rootNode = rootNode;
                _showFileSize = showFileSize;
                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                TreeViewItem root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

                if (_rootNode != null)
                {
                    TreeViewItem rootItem = BuildTreeRecursive(_rootNode, 1);
                    root.AddChild(rootItem);
                }
                else
                {
                    root.AddChild(new TreeViewItem { id = 1, depth = 0, displayName = "无数据" });
                }

                SetupDepthsFromParentsAndChildren(root);
                return root;
            }

            private TreeViewItem BuildTreeRecursive(DependencyNode node, int id)
            {
                string displayName = node.AssetName;
                
                if (_showFileSize && !node.IsCircular)
                {
                    AssetInfo info = AssetDependencyAnalyzer.GetAssetInfo(node.AssetPath);
                    string sizeStr = AssetDependencyAnalyzer.FormatFileSize(info.FileSize);
                    displayName += $" ({sizeStr})";
                }
                
                if (node.IsCircular)
                {
                    displayName += " [循环依赖]";
                }

                TreeViewItem item = new DependencyTreeViewItem
                {
                    id = id,
                    depth = node.Depth,
                    displayName = displayName,
                    AssetPath = node.AssetPath,
                    IsCircular = node.IsCircular
                };

                int childId = id * 1000 + 1;
                foreach (DependencyNode child in node.Children)
                {
                    TreeViewItem childItem = BuildTreeRecursive(child, childId);
                    item.AddChild(childItem);
                    childId++;
                }

                return item;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                DependencyTreeViewItem item = args.item as DependencyTreeViewItem;
                
                if (item != null && item.IsCircular)
                {
                    // 循环依赖显示为黄色
                    Color oldColor = GUI.color;
                    GUI.color = Color.yellow;
                    base.RowGUI(args);
                    GUI.color = oldColor;
                }
                else
                {
                    base.RowGUI(args);
                }
            }

            protected override void DoubleClickedItem(int id)
            {
                TreeViewItem item = FindItem(id, rootItem);
                if (item is DependencyTreeViewItem depItem)
                {
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(depItem.AssetPath);
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
            }

            protected override void ContextClickedItem(int id)
            {
                TreeViewItem item = FindItem(id, rootItem);
                if (item is DependencyTreeViewItem depItem)
                {
                    GenericMenu menu = new GenericMenu();
                    
                    menu.AddItem(new GUIContent("选择资源"), false, () =>
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath<Object>(depItem.AssetPath);
                        if (asset != null)
                        {
                            Selection.activeObject = asset;
                        }
                    });
                    
                    menu.AddItem(new GUIContent("定位资源"), false, () =>
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath<Object>(depItem.AssetPath);
                        if (asset != null)
                        {
                            EditorGUIUtility.PingObject(asset);
                        }
                    });
                    
                    menu.AddItem(new GUIContent("复制路径"), false, () =>
                    {
                        EditorGUIUtility.systemCopyBuffer = depItem.AssetPath;
                    });
                    
                    menu.AddSeparator("");
                    
                    menu.AddItem(new GUIContent("在新窗口中分析"), false, () =>
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath<Object>(depItem.AssetPath);
                        if (asset != null)
                        {
                            AssetDependencyTreeWindow newWindow = CreateInstance<AssetDependencyTreeWindow>();
                            newWindow.titleContent = new GUIContent("资源依赖树");
                            newWindow._targetAsset = asset;
                            newWindow.Show();
                            newWindow.AnalyzeDependencyTree();
                        }
                    });
                    
                    menu.ShowAsContext();
                }
            }
        }

        /// <summary>
        /// 自定义树视图项
        /// </summary>
        private class DependencyTreeViewItem : TreeViewItem
        {
            public string AssetPath { get; set; }
            public bool IsCircular { get; set; }
        }
    }
}



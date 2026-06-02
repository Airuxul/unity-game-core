using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Air.UnityGameCore.Runtime.Resource
{
    public abstract class ResManager : IResManager
    {
        protected readonly Dictionary<string, IResLoadInfo> _loadInfoDict = new();
        protected readonly Dictionary<string, Delegate> _loadCallback = new();
        protected readonly Dictionary<string, Delegate> _loadInstCallback = new();

        public void LoadResAsync<T>(string path, Action<T> callback) where T: Object
        {
            LoadAsyncInternal(path, callback, ELoadType.Asset);
        }

        public void LoadInstanceAsync<T>(string path, Action<T> callback) where T : Object
        {
            LoadAsyncInternal(path, callback, ELoadType.Instance);
        }

        private void LoadAsyncInternal<T>(string path, Action<T> callback, ELoadType loadType) where T : Object
        {
            var loadInfo = GetOrCreateLoadInfo<T>(path);
            var callbackDict = GetCallbackDict(loadType);
            
            // 如果已加载完成，直接调用回调
            if (loadInfo.IsLoaded())
            {
                loadInfo.LoadCount++;
                InvokeCallback(loadInfo.Asset, callback, loadType);
                return;
            }

            // 如果正在加载中，添加回调到队列
            if (loadInfo.IsLoading())
            {
                AddCallback(path, callback, callbackDict);
                return;
            }

            // 开始新的加载
            loadInfo.LoadStatus = EResLoadStatus.Loading;
            callbackDict[path] = callback;
            
            // 调用子类实现的具体加载逻辑
            LoadAssetAsync(path, loadInfo, loadType);
        }

        /// <summary>
        /// 抽象方法：子类实现具体的资源加载逻辑
        /// </summary>
        protected abstract void LoadAssetAsync<T>(string path, ResLoadInfo<T> loadInfo, ELoadType loadType) where T : Object;

        /// <summary>
        /// 资源加载完成后的通用处理
        /// </summary>
        protected void OnAssetLoadCompleted<T>(string path, ResLoadInfo<T> loadInfo, T asset, ELoadType loadType) where T: Object
        {
            var callbackDict = GetCallbackDict(loadType);
            
            if (asset != null)
            {
                loadInfo.Asset = asset;
                loadInfo.LoadStatus = EResLoadStatus.Loaded;
                loadInfo.LoadCount++;
                
                // 从字典中获取并调用所有回调
                if (callbackDict.TryGetValue(path, out var callbackDelegate))
                {
                    if (callbackDelegate is Action<T> typedCallback)
                    {
                        InvokeCallback(asset, typedCallback, loadType);
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to load resource at path: {loadInfo.Path}");
                loadInfo.LoadStatus = EResLoadStatus.Unload;
            }
            
            // 清空回调队列
            callbackDict.Remove(path);
        }

        private void InvokeCallback<T>(T asset, Action<T> callback, ELoadType loadType) where T : Object
        {
            if (callback == null) return;

            if (loadType == ELoadType.Instance)
            {
                var instance = Object.Instantiate(asset);
                callback.Invoke(instance);
            }
            else
            {
                callback.Invoke(asset);
            }
        }

        protected Dictionary<string, Delegate> GetCallbackDict(ELoadType loadType)
        {
            return loadType == ELoadType.Instance ? _loadInstCallback : _loadCallback;
        }

        protected void AddCallback<T>(string path, Action<T> callback, Dictionary<string, Delegate> callbackDict) where T : Object
        {
            if (callbackDict.TryGetValue(path, out var existingCallback))
            {
                callbackDict[path] = Delegate.Combine(existingCallback, callback);
            }
            else
            {
                callbackDict[path] = callback;
            }
        }

        public ResLoadInfo<T> GetOrCreateLoadInfo<T>(string path) where T: Object
        {
            if (!_loadInfoDict.TryGetValue(path, out var info))
            {
                var newInfo = new ResLoadInfo<T>
                {
                    Path = path,
                    LoadCount = 0,
                    LoadStatus = EResLoadStatus.Unload,
                };
                _loadInfoDict[path] = newInfo;
                return newInfo;
            }

            // 确保类型匹配
            if (info is ResLoadInfo<T> typedInfo)
            {
                return typedInfo;
            }
            
            Debug.LogError($"Type mismatch for resource at path: {path}. Expected {typeof(T)}, but found {info.GetType()}");
            return null;
        }

        public virtual void UnloadRes(string path)
        {
            if (!_loadInfoDict.TryGetValue(path, out var info))
            {
                return;
            }

            // 减少引用计数
            if (info is ResLoadInfo<Object> loadInfo)
            {
                loadInfo.LoadCount--;
                
                // 引用计数为 0 时才真正卸载
                if (!loadInfo.NeedUnload()) return;
                
                // 调用子类的卸载逻辑
                OnUnloadAsset(loadInfo);
                
                loadInfo.LoadStatus = EResLoadStatus.Unload;
                _loadInfoDict.Remove(path);
                _loadCallback.Remove(path);
                _loadInstCallback.Remove(path);
            }
        }

        /// <summary>
        /// 抽象方法：子类实现具体的资源卸载逻辑
        /// </summary>
        protected abstract void OnUnloadAsset(IResLoadInfo loadInfo);
    }
}
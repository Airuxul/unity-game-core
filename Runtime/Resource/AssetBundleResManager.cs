using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Air.UnityGameCore.Runtime.Resource
{
    /// <summary>
    /// AssetBundle 资源管理器。
    /// </summary>
    public sealed class AssetBundleResManager : ResManager
    {
        readonly AssetBundleLoader _bundleLoader;
        readonly Func<string, string> _bundlePathResolver;

        public AssetBundleResManager(
            string bundleRootPath,
            Func<string, string> bundlePathResolver = null,
            Func<string, string[]> dependenciesResolver = null)
        {
            _bundlePathResolver = bundlePathResolver ?? DefaultBundlePathResolver;
            _bundleLoader = new AssetBundleLoader(bundleRootPath, dependenciesResolver);
        }

        static string DefaultBundlePathResolver(string assetPath) => assetPath.ToLower();

        protected override T LoadAsset<T>(string path, ResLoadInfo<T> loadInfo, ELoadType loadType)
        {
            var bundlePath = _bundlePathResolver(path);
            _bundleLoader.LoadBundle(bundlePath);
            var request = _bundleLoader.LoadBundle(bundlePath);
            var asset = request.LoadAsset<T>(path);
            OnAssetLoadCompleted(path, loadInfo, asset, loadType);
            return asset;
        }

        protected override void LoadAssetAsync<T>(string path, ResLoadInfo<T> loadInfo, ELoadType loadType)
        {
            var bundlePath = _bundlePathResolver(path);

            _bundleLoader.LoadBundleAsync(bundlePath, bundle =>
            {
                if (bundle == null)
                {
                    Debug.LogError($"Failed to load AssetBundle: {bundlePath} for asset: {path}");
                    OnAssetLoadCompleted(path, loadInfo, null, loadType);
                    return;
                }

                loadInfo.BundlePath = bundlePath;

                var request = bundle.LoadAssetAsync<T>(path);
                request.completed += _ =>
                {
                    var asset = request.asset as T;
                    OnAssetLoadCompleted(path, loadInfo, asset, loadType);
                };
            });
        }

        protected override void OnUnloadAsset(IResLoadInfo loadInfo)
        {
            if (loadInfo is not ResLoadInfo<Object> typedLoadInfo) return;
            typedLoadInfo.Asset = null;
            if (!string.IsNullOrEmpty(typedLoadInfo.BundlePath))
                _bundleLoader.UnloadBundle(typedLoadInfo.BundlePath);
        }

        public void UnloadUnusedBundles() => _bundleLoader.UnloadUnusedBundles();

        public void Clear()
        {
            _loadInfoDict.Clear();
            _loadCallback.Clear();
            _loadInstCallback.Clear();
            _bundleLoader.Clear();
        }
    }
}

using UnityEngine;
using Object = UnityEngine.Object;

namespace Air.UnityGameCore.Runtime.Resource
{
    /// <summary>
    /// Unity Resources 资源管理器
    /// </summary>
    public class UnityResManager : ResManager
    {
        protected override void LoadAssetAsync<T>(string path, ResLoadInfo<T> loadInfo, ELoadType loadType)
        {
            // 从 Resources 文件夹异步加载资源
            var request = Resources.LoadAsync<T>(path);
            request.completed += _ =>
            {
                var asset = request.asset as T;
                OnAssetLoadCompleted(path, loadInfo, asset, loadType);
            };
        }

        protected override void OnUnloadAsset(IResLoadInfo loadInfo)
        {
            // 卸载 Resources 资源
            if (loadInfo is ResLoadInfo<Object> typedLoadInfo && typedLoadInfo.Asset != null)
            {
                Resources.UnloadAsset(typedLoadInfo.Asset);
                typedLoadInfo.Asset = null;
            }
        }
    }
}
using System;

namespace Air.UnityGameCore.Runtime.Resource
{
    public interface IResManager
    {
        public T LoadRes<T>(string path) where T : UnityEngine.Object;
        
        public T LoadInstance<T>(string path) where T : UnityEngine.Object;
        
        public void LoadResAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object;
        
        public void LoadInstanceAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object;
        
        public ResLoadInfo<T> GetOrCreateLoadInfo<T>(string path) where T : UnityEngine.Object;
        
        public void UnloadRes(string path);
    }
}
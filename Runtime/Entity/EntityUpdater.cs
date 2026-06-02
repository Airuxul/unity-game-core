using UnityEngine;

namespace Air.UnityGameCore.Runtime.Entity
{
    public sealed class EntityUpdater : MonoBehaviour
    {
        UnityEntityManager _manager;

        public void Install(UnityEntityManager manager) => _manager = manager;

        void Update()
        {
            if (_manager != null)
                _manager.Tick(UnityEngine.Time.deltaTime);
        }
    }
}

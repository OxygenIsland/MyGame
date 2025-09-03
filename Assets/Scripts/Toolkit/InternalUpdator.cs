
using System;
using StarWorld.FrameWork;

namespace StarWorld.Common.Utility
{
    public class InternalUpdator : MonoSingleton<InternalUpdator>
    {
        public event Action OnUpdate;

        public event Action OnFixedUpdate;

        public event Action OnLateUpdate;
        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }
    }
}
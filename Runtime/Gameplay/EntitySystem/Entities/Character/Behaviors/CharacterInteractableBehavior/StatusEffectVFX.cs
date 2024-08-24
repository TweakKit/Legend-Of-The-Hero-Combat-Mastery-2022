using System;
using UnityEngine;
using UnityEngine.Events;
using Runtime.Gameplay.Manager;
using Sirenix.OdinInspector;

namespace Runtime.Gameplay.EntitySystem
{
    public sealed class StatusEffectVFX : MonoBehaviour
    {
        #region Members

        [Tooltip("Ticked if this status effect vfx is self-destroyed, meaning when its status effect removed from the character, " +
                 "the vfx won't be destroyed right along, but by its sprite animation player instead.")]
        [SerializeField]
        private bool _isSelfDestroyed;
        [ShowIf(nameof(_isSelfDestroyed), true)]
        [SerializeField]
        private UnityEvent _destroyedEventCaller;

        private bool _disposed;

        #endregion Members

        #region API Methods

        private void OnEnable()
        {
            _disposed = false;
        }

        #endregion API Methods

        #region Class Methods

        public void Dispose(bool parentIsDisposing = false)
        {
            if (parentIsDisposing)
                DestroySelf();

            if (_disposed)
                return;

            _disposed = true;

            if (_isSelfDestroyed)
            {
                if (_destroyedEventCaller != null)
                    _destroyedEventCaller.Invoke();
                else
                    DestroySelf();
            }
            else DestroySelf();
        }

        private void DestroySelf()
        {
            PoolManager.Instance.Remove(gameObject);
        }
            

        #endregion Class Methods
    }
}
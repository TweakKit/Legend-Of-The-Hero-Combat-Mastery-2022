using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Core.Singleton;
using Runtime.Extensions;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class DamageFloatingTextController : MonoSingleton<DamageFloatingTextController>
    {
        #region Members

        [SerializeField]
        private int _maxDamageTextNumber;
        private List<DamageFloatingText> _damageFloatingTexts;

        #endregion Members

        #region Class Methods

        protected override void Awake()
        {
            base.Awake();
            _damageFloatingTexts = new();
        }

        #endregion Class Methods

        #region Class Methods

        public async UniTask Spawn(string assetName, float value, Vector2 spawnPosition, CancellationToken token)
        {
            while(_damageFloatingTexts.Count >= _maxDamageTextNumber)
            {
                // Despawn oldest text;
                var firstDamageFloatingText = _damageFloatingTexts[0];
                _damageFloatingTexts.Remove(firstDamageFloatingText);
                PoolManager.Instance.Remove(firstDamageFloatingText.gameObject);
            }

            var damageTextObject = await PoolManager.Instance.Get(assetName, cancellationToken: token);
            var damageText = damageTextObject.GetOrAddComponent<DamageFloatingText>();
            damageText.Init(value, spawnPosition);
            _damageFloatingTexts.Add(damageText);
        }

        public void Despawn(DamageFloatingText floatingText)
        {
            _damageFloatingTexts.Remove(floatingText);
            PoolManager.Instance.Remove(floatingText.gameObject);
        }

        #endregion Class Methods
    }
}

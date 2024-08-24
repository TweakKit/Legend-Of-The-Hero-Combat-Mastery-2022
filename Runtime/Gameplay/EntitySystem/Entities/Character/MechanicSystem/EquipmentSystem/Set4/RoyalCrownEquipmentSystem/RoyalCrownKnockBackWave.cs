using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class RoyalCrownKnockBackWave : SpriteAnimatorDamageBox
    {
        #region Members

        [SerializeField]
        private CircleCollider2D _waveCollider;
        [SerializeField]
        private float _waveScaleSpeed;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region API Methods

        private void OnDisable()
            => _cancellationTokenSource?.Cancel();

        #endregion API Methods

        #region Class Methods

        public override void Init(CharacterModel creatorModel, DamageSource damageSource, bool destroyWithCreator, float damageBonus = 0, DamageFactor[] damageFactors = null,
                                 StatusEffectModel[] modifierModels = null,
                                 Action finishedAction = null, List<uint> avoidEntityUids = null,
                                 Action<IProjectile> projectileInteractAction = null,
                                 Action<IInteractable> createdDamageAction = null, Vector2 direction = default)
        {
            base.Init(creatorModel, damageSource, destroyWithCreator, damageBonus, damageFactors, modifierModels,
                      finishedAction, avoidEntityUids, projectileInteractAction = null, createdDamageAction, direction);
            _cancellationTokenSource = new CancellationTokenSource();
            StartWavingAsync(_cancellationTokenSource.Token).Forget();
        }

        protected override void OnCreatorDeath(DamageSource damageSource)
        {
            _cancellationTokenSource?.Cancel();
            base.OnCreatorDeath(damageSource);
        }

        protected override void OnStopAnim()
        {
            _cancellationTokenSource?.Cancel();
            base.OnStopAnim();
        }

        private async UniTask StartWavingAsync(CancellationToken cancellationToken)
        {
            _waveCollider.radius = 0.0f;
            var currentWaveColliderRadius = 0.0f;
            while (currentWaveColliderRadius < 1.0f)
            {
                currentWaveColliderRadius += Time.deltaTime * _waveScaleSpeed;
                _waveCollider.radius = currentWaveColliderRadius;
                await UniTask.Yield(cancellationToken: cancellationToken);
            }
            _waveCollider.radius = 1.0f;
        }

        #endregion Class Methods
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Runtime.Animation;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class DecreaseProjectileSpeedEffectArea : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private EntityTargetDetector _entityTargetDetector;
        [SerializeField]
        private SingleFullSpriteAnimationPlayersController _spriteEffectController;
        private List<IProjectile> _projectileInside;
        private float _decreaseSpeedPercent;
        private EntityModel _creatorModel;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region API Methods

        private void Awake()
            => _entityTargetDetector.Init(colliderEnteredAction: OnColliderEntered, colliderExitedAction: OnColliderExited);

        private void OnDisable()
        {
            if(_projectileInside != null)
            {
                foreach (var projectile in _projectileInside)
                    projectile?.UpdateAdjustSpeedFactor(_decreaseSpeedPercent);
                _projectileInside.Clear();
            }
            _cancellationTokenSource?.Cancel();
        }    

        #endregion API Methods

        #region Class Methods

        public void Init(float decreaseSpeedPercent, float lifeTime, EntityModel creatorModel)
        {
            _creatorModel = creatorModel;
            _projectileInside = new();
            _decreaseSpeedPercent = decreaseSpeedPercent;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            RunCooldownAsync(lifeTime, _cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid RunCooldownAsync(float lifeTime, CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(lifeTime), cancellationToken: token);
            _spriteEffectController.StopAnimation(() => PoolManager.Instance.Remove(gameObject));
        }

        private void OnColliderEntered(Collider2D collider)
        {
            var projectile = collider.GetComponent<IProjectile>();
            if (projectile != null && !_projectileInside.Contains(projectile) && _creatorModel.EntityType.CanCauseDamage(projectile.CreatorModel.EntityType))
            {
                projectile.UpdateAdjustSpeedFactor(-_decreaseSpeedPercent);
                _projectileInside.Add(projectile);
            }
        }

        private void OnColliderExited(Collider2D collider)
        {
            var projectile = collider.GetComponent<IProjectile>();
            if (projectile != null && _projectileInside.Contains(projectile) && _creatorModel.EntityType.CanCauseDamage(projectile.CreatorModel.EntityType))
            {
                projectile.UpdateAdjustSpeedFactor(_decreaseSpeedPercent);
                _projectileInside.Remove(projectile);
            }
        }

        #endregion Class Methods
    }
}
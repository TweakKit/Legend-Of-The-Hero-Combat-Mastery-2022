using System.Threading;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class GumBootsDamageZone : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private float _realDamageRadius;
        private float _existTime;
        private float _damageInterval;
        private DamageSource _damageSource;
        private float _damageBonus;
        private DamageFactor[] _damageFactors;
        private StatusEffectModel[] _damageModifierModels;
        private CharacterModel _creatorModel;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region API Methods

        private void OnDestroy()
            => _cancellationTokenSource?.Cancel();

        #endregion API Methods
        #region Class Methods

        public void Init(CharacterModel creatorModel, float existTime, float damageInterval,
                         DamageSource damageSource, float damageBonus,
                         DamageFactor[] damageFactors, StatusEffectModel[] damageModifierModels)
        {
            _creatorModel = creatorModel;
            _existTime = existTime;
            _damageInterval = damageInterval;
            _damageSource = damageSource;
            _damageBonus = damageBonus;
            _damageFactors = damageFactors;
            _damageModifierModels = damageModifierModels;
            _cancellationTokenSource = new CancellationTokenSource();
            StartOperationAsync(_cancellationTokenSource.Token).Forget();
        }

        private async UniTask StartOperationAsync(CancellationToken cancellationToken)
        {
            var currentExistTime = 0.0f;
            var currentDamageTime = 0.0f;
            while (currentExistTime < _existTime)
            {
                currentExistTime += Time.deltaTime;
                currentDamageTime += Time.deltaTime;
                if (currentDamageTime >= _damageInterval)
                {
                    currentDamageTime = 0.0f;
                    DamageTargets();
                }
                await UniTask.Yield(cancellationToken: cancellationToken);
            }
            PoolManager.Instance.Remove(gameObject);
        }

        public void DamageTargets()
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, _realDamageRadius);
            foreach (var collider in colliders)
            {
                var entity = collider.GetComponent<IEntity>();
                if (entity != null)
                {
                    var interactable = entity.GetBehavior<IInteractable>(true);
                    if (interactable != null && !interactable.Model.IsDead)
                    {
                        if (interactable.Model == _creatorModel)
                            continue;

                        var damageDirection = (interactable.Model.Position - (Vector2)transform.position).normalized;
                        var damageInfo = _creatorModel.GetDamageInfo(_damageSource, _damageBonus, _damageFactors, _damageModifierModels, interactable.Model);
                        interactable.GetHit(damageInfo, new DamageMetaData(damageDirection, transform.position));
                    }
                }
            }
        }

        #endregion Class Methods
    }
}
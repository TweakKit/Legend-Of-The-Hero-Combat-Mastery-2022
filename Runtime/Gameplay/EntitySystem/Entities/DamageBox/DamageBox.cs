using System;
using System.Collections.Generic;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class DamageBox : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private EntityTargetDetector _targetDetector;
        [SerializeField]
        private bool _includeEntityProxy;

        private DamageSource _damageSource;
        private float _damageBonus;
        private DamageFactor[] _damageFactors;
        private StatusEffectModel[] _damageModifierModels;
        private List<uint> _avoidEntityUids;
        private CharacterModel _creatorModel;
        private Action<IProjectile> _projectileInteractAction;
        private Action<IInteractable> _createdDamageAction;
        private Vector2 _direction;
        private List<IInteractable> _listDamaged;

        #endregion Members

        #region Properties

        public List<IInteractable> ListDamaged => _listDamaged;

        #endregion Properties

        #region API Methods

        private void OnEnable()
        {
            _targetDetector.Disable();
        }

        #endregion API Methods

        #region Class Methods

        public virtual void Init(CharacterModel creatorModel, DamageSource damageSource, bool destroyWithCreator, float damageBonus = 0, DamageFactor[] damageFactors = null,
                                 StatusEffectModel[] modifierModels = null, List<uint> avoidEntityUids = null,
                                 Action<IProjectile> projectileInteractAction = null,
                                 Action<IInteractable> createdDamageAction = null, Vector2 direction = default)
        {
            _listDamaged = new();
            _damageSource = damageSource;
            _damageBonus = damageBonus;
            _damageFactors = damageFactors;
            _damageModifierModels = modifierModels;
            _creatorModel = creatorModel;
            _avoidEntityUids = avoidEntityUids;
            _projectileInteractAction = projectileInteractAction;
            _createdDamageAction = createdDamageAction;
            _direction = direction;

            if (destroyWithCreator)
                _creatorModel.DeathEvent += OnCreatorDeath;

            _targetDetector.Init(null, null, OnColliderEnterredAction);
            _targetDetector.Enable();
        }

        protected virtual void OnColliderEnterredAction(Collider2D collider)
        {
            if (_projectileInteractAction != null)
            {
                var projectile = collider.GetComponent<IProjectile>();
                if (projectile != null)
                {
                    _projectileInteractAction.Invoke(projectile);
                    return;
                }
            }

            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>(_includeEntityProxy);
                if (interactable != null && !interactable.Model.IsDead && (_avoidEntityUids == null || !_avoidEntityUids.Contains(interactable.Model.EntityUId)))
                {
                    if (_creatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                    {
                        var hitPoint = collider.ClosestPoint(transform.position);
                        _listDamaged.Add(interactable);
                        HitTarget(interactable, hitPoint);
                    }
                }
            }
        }

        protected virtual void HitTarget(IInteractable interactable, Vector2 hitPoint)
        {
            var hitDirection = _direction == Vector2.zero ? hitPoint - (Vector2)transform.position : _direction;
            var damageInfo = _creatorModel.GetDamageInfo(_damageSource, _damageBonus, _damageFactors, _damageModifierModels, interactable.Model);
            interactable.GetHit(damageInfo, new DamageMetaData(hitDirection, _creatorModel.Position));
            _createdDamageAction?.Invoke(interactable);
        }

        protected virtual void OnCreatorDeath(DamageSource damageSource)
            => PoolManager.Instance.Remove(gameObject);

        #endregion Class Methods
    }
}
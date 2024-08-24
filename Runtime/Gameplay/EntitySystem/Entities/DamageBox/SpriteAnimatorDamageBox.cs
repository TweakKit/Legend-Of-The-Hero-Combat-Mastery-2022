using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Runtime.Animation;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpriteAnimatorDamageBox : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private bool _enableTriggerAtStart;
        [SerializeField]
        private int[] _triggeredColliderFrames;
        [SerializeField]
        private int[] _turnOffColliderFrames;
        [SerializeField]
        private SpriteAnimator _spriteAnimator;
        [SerializeField]
        private EntityTargetDetector _targetDetector;
        [SerializeField]
        private bool _notUseSelfDestroy;
        [SerializeField]
        private bool _notUseDestroyByCreator;
        [SerializeField]
        private bool _includeEntityProxy;

        private DamageSource _damageSource;
        private float _damageBonus;
        private DamageFactor[] _damageFactors;
        private StatusEffectModel[] _damageModifierModels;
        private List<uint> _avoidEntityUids;
        private CharacterModel _creatorModel;
        private Action _finishedAction;
        private int _currentFrame;
        private Action<IProjectile> _projectileInteractAction;
        private Action<IInteractable> _createdDamageAction;
        private Vector2 _direction;

        #endregion Members

        #region API Methods

        protected virtual void Update()
        {
            if (_currentFrame != _spriteAnimator.CurrentFrame)
            {
                _currentFrame = _spriteAnimator.CurrentFrame;
                if (_triggeredColliderFrames.Contains(_currentFrame))
                    _targetDetector.Enable();
                else if (_turnOffColliderFrames.Contains(_currentFrame))
                    _targetDetector.Disable();
            }
        }

        #endregion API Methods

        #region Class Methods

        public virtual void Init(CharacterModel creatorModel, DamageSource damageSource, bool destroyWithCreator, float damageBonus = 0, DamageFactor[] damageFactors = null,
                                 StatusEffectModel[] modifierModels = null,
                                 Action finishedAction = null, List<uint> avoidEntityUids = null,
                                 Action<IProjectile> projectileInteractAction = null,
                                 Action<IInteractable> createdDamageAction = null, Vector2 direction = default)
        {
            _spriteAnimator.Play(true);
            _damageSource = damageSource;
            _damageBonus = damageBonus;
            _damageFactors = damageFactors;
            _damageModifierModels = modifierModels;
            _finishedAction = finishedAction;
            _creatorModel = creatorModel;
            _spriteAnimator.AnimationStoppedAction = null;
            _spriteAnimator.AnimationStoppedAction = OnStopAnim;
            _currentFrame = -1;
            _avoidEntityUids = avoidEntityUids;
            _projectileInteractAction = projectileInteractAction;
            _createdDamageAction = createdDamageAction;
            _direction = direction;

            if (destroyWithCreator)
                _creatorModel.DeathEvent += OnCreatorDeath;

            if (_enableTriggerAtStart)
                _targetDetector.Enable();
            else
                _targetDetector.Disable();

            _targetDetector.Init(null, null, OnColliderEnterredAction);
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
        {
            if(!_notUseDestroyByCreator)
                PoolManager.Instance.Remove(gameObject);
        }

        protected virtual void OnStopAnim()
        {
            _finishedAction?.Invoke();

            if(!_notUseSelfDestroy)
                PoolManager.Instance.Remove(gameObject);
        }

        #endregion Class Methods
    }
}
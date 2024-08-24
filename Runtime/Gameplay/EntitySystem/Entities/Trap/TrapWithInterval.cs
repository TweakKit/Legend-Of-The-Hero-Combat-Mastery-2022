using System;
using System.Linq;
using Runtime.Animation;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [RequireComponent(typeof(SimpleTilemapAnimator))]
    [RequireComponent(typeof(Collider2D))]
    public class TrapWithInterval : Entity
    {
        #region Members

        [SerializeField]
        private Collider2D _collider;
        private SimpleTilemapAnimator _animator;
        private TrapWithIntervalModel _ownerModel;
        private float _currentCountTime;
        private int _currentFrame;
        private bool _isTriggering;
        private bool _isInited;

        #endregion Members

        #region API Methods

        private void Awake()
        {
            _animator = GetComponent<SimpleTilemapAnimator>();
        }

        private void OnEnable()
        {
            _isInited = false;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (_ownerModel == null)
                return;
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>(true);
                if (interactable != null && !interactable.Model.IsDead)
                    interactable.GetHit(_ownerModel.GetDamageInfo(interactable.Model), default);
            }
        }

        private void Update()
        {
            if (!_isInited)
                return;

            _currentCountTime += Time.deltaTime;
            if(_currentCountTime >= _ownerModel.interval)
            {
                _currentCountTime = 0;
                TriggerAnimation();
            }

            if(_isTriggering)
            {
                if (_currentFrame != _animator.CurrentFrameIndex)
                {
                    _currentFrame = _animator.CurrentFrameIndex;
                    if (_animator.TileMapAnimation.triggeredColliderFrames.Contains(_currentFrame))
                    {
                        _collider.enabled = true;
                    }
                    else if (_animator.TileMapAnimation.turnOffColliderFrames.Contains(_currentFrame))
                    {
                        _collider.enabled = false;
                    }
                }
            }

        }

        #endregion API Methods

        #region Class Methods

        public override void Build(EntityModel model, Vector3 position)
        {
            base.Build(model, position);
            _ownerModel = model as TrapWithIntervalModel;
            _currentCountTime = 0;
            _isTriggering = false;
            _isInited = true;
            _collider.enabled = false;
            _animator.StopAction = null;
            _animator.StopAction = OnStopAnim;
        }

        public override T GetBehavior<T>(bool includeProxy = false) where T : class => this as T;

        private void TriggerAnimation()
        {
            _currentFrame = -1;
            _isTriggering = true;
            _animator.Play();
        }

        private void OnStopAnim()
        {
            _currentFrame = -1;
            _isTriggering = false;
            _collider.enabled = false;
        }

        #endregion Class Methods
    }
}
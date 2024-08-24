using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyForwardRotateIncreaseProjectileStrategyData : ProjectileStrategyData
    {
        #region Members

        public float projectileMoveDistance;
        public float projectileMoveSpeed;
        public float projectileRotateSpeed;
        public float projectileRotateDegree;
        public float projectileScaleSpeed;
        public string attachedChildProjectilePrefabName;
        public uint numberOfAttachedChildProjectiles;
        public float attachedChildProjectileCenterOffsetDistance;
        public float attachedChildProjectileDamageBonus;
        public DamageFactor[] attachedChildProjectileDamageFactors;

        #endregion Members

        #region Class Methods

        public FlyForwardRotateIncreaseProjectileStrategyData(DamageSource damageSource, float projectileMoveDistance, float projectileMoveSpeed, float projectileRotateSpeed,
                                                              float projectileRotateDegree, float projectileScaleSpeed, uint numberOfAttachedChildProjectiles, string attachedChildProjectilePrefabName,
                                                              float attachedChildProjectileCenterOffsetDistance, DamageFactor[] attachedChildProjectileDamageFactors, float attachedChildProjectileDamageBonus)
            : base(damageSource, ProjectileStrategyType.ForwardRotateIncrease)
        {
            this.projectileMoveDistance = projectileMoveDistance;
            this.projectileMoveSpeed = projectileMoveSpeed;
            this.projectileRotateSpeed = projectileRotateSpeed;
            this.projectileRotateDegree = projectileRotateDegree;
            this.projectileScaleSpeed = projectileScaleSpeed;
            this.attachedChildProjectilePrefabName = attachedChildProjectilePrefabName;
            this.numberOfAttachedChildProjectiles = numberOfAttachedChildProjectiles;
            this.attachedChildProjectileCenterOffsetDistance = attachedChildProjectileCenterOffsetDistance;
            this.attachedChildProjectileDamageBonus = attachedChildProjectileDamageBonus;
            this.attachedChildProjectileDamageFactors = attachedChildProjectileDamageFactors;
        }

        #endregion Class Methods
    }

    public class FlyForwardRotateIncreaseProjectileStrategy : ProjectileStrategy<FlyForwardRotateIncreaseProjectileStrategyData>
    {
        #region Properties

        private Vector2 _currentDirection;
        private Vector2 _originalPosition;
        private bool _isGeneratedChildren;
        private bool _isClockwise;
        private Quaternion _nextRotation;
        private Quaternion _startRotation;
        private float _timeCount;
        private float _timeRotate;
        private List<Projectile> _attachedChildProjectiles;

        #endregion Properties

        #region Class Methods

        public override void Init(ProjectileStrategyData projectileStrategyData, Projectile controllerProjectile, Vector2 direction, Vector2 originalPosition, EntityModel targetModel = null)
        {
            base.Init(projectileStrategyData, controllerProjectile, direction, originalPosition, targetModel);
            _originalPosition = originalPosition;
            _currentDirection = direction;
            _timeRotate = strategyData.projectileRotateDegree / strategyData.projectileRotateSpeed;
            _attachedChildProjectiles = new List<Projectile>();
        }

        public override void Start()
        {
            _isGeneratedChildren = false;
            _isClockwise = true;
            _nextRotation = Quaternion.AngleAxis(strategyData.projectileRotateDegree, Vector3.forward);
            _startRotation = Quaternion.AngleAxis(0, Vector3.forward);
            _timeCount = 0;
            CreateAttachedChildProjectilesAsync().Forget();
        }

        public override void Update()
        {
            if (!_isGeneratedChildren)
                return;

            controllerProjectile.UpdatePositionBySpeed(strategyData.projectileMoveSpeed, _currentDirection);
            controllerProjectile.UpdateRotation(Quaternion.Lerp(_startRotation, _nextRotation, Mathf.Clamp01(_timeRotate - _timeCount / _timeRotate)));

            _timeCount += Time.deltaTime;
            if (_timeCount >= _timeRotate)
            {
                _timeCount = 0;
                _isClockwise = !_isClockwise;
                if (_isClockwise)
                {
                    _nextRotation = Quaternion.AngleAxis(strategyData.projectileRotateDegree, Vector3.forward);
                    _startRotation = Quaternion.AngleAxis(0, Vector3.forward);
                }
                else
                {
                    _nextRotation = Quaternion.AngleAxis(0, Vector3.forward);
                    _startRotation = Quaternion.AngleAxis(strategyData.projectileRotateDegree, Vector3.forward);
                }
            }

            float highestRadius = 0;
            for (int i = 0; i < _attachedChildProjectiles.Count; i++)
            {
                var direction = (_attachedChildProjectiles[i].CenterPosition - controllerProjectile.CenterPosition);
                var distance = Vector2.Distance(_attachedChildProjectiles[i].CenterPosition, controllerProjectile.CenterPosition);
                if (distance > highestRadius)
                    highestRadius = distance;
                var scaleDirection = direction.normalized;
                _attachedChildProjectiles[i].UpdatePositionBySpeed(strategyData.projectileScaleSpeed, scaleDirection);
            }

            if (highestRadius != 0)
            {
                var collider2D = controllerProjectile.GetComponent<CircleCollider2D>();
                if (collider2D)
                    collider2D.radius = highestRadius;
            }

            if (Vector2.SqrMagnitude(_originalPosition - controllerProjectile.CenterPosition) > strategyData.projectileMoveDistance * strategyData.projectileMoveDistance)
                DestroySelf();
        }

        private async UniTask CreateAttachedChildProjectilesAsync()
        {
            var angle = Constants.CIRCLE_DEGREES / strategyData.numberOfAttachedChildProjectiles;
            for (int i = 0; i < strategyData.numberOfAttachedChildProjectiles; i++)
            {
                var attachedChildProjectileGameObject = await EntitiesManager.Instance.CreateProjectileAsync(strategyData.attachedChildProjectilePrefabName, controllerProjectile.CreatorModel, Vector2.zero);
                var attachedChildProjectileStrategyData = new IdleThroughProjectileStrategyData(parentController: controllerProjectile.transform,
                                                                                                damageSource: strategyData.damageSource,
                                                                                                damageFactors: strategyData.attachedChildProjectileDamageFactors,
                                                                                                damageBonus: strategyData.attachedChildProjectileDamageBonus);
                var attachedChildProjectile = attachedChildProjectileGameObject.GetComponent<Projectile>();
                var attachedChildProjectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(attachedChildProjectileStrategyData.projectileStrategyType);
                var attachedChildProjectileSpawnDirection = Quaternion.AngleAxis(angle * i, Vector3.forward) * Vector2.up;
                var attachedChildProjectileSpawnPosition = controllerProjectile.CenterPosition + (Vector2)attachedChildProjectileSpawnDirection * strategyData.attachedChildProjectileCenterOffsetDistance;
                attachedChildProjectileStrategy.Init(attachedChildProjectileStrategyData, controllerProjectile, attachedChildProjectileSpawnDirection, attachedChildProjectileSpawnPosition);
                attachedChildProjectile.InitStrategies(new[] { attachedChildProjectileStrategy });
                attachedChildProjectileGameObject.transform.SetParent(controllerProjectile.transform);
                attachedChildProjectileGameObject.transform.localRotation = Quaternion.AngleAxis(angle * i, Vector3.forward);
                attachedChildProjectileGameObject.transform.localPosition = attachedChildProjectileSpawnDirection * strategyData.attachedChildProjectileCenterOffsetDistance;
                _attachedChildProjectiles.Add(attachedChildProjectile);
            }
            _isGeneratedChildren = true;
        }

        private void DestroySelf()
        {
            foreach (var attachedChildProjectile in _attachedChildProjectiles)
                attachedChildProjectile.InitStrategies(null);
            _attachedChildProjectiles = null;
            Complete(false, true);
        }

        #endregion Class Methods
    }
}
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Extensions;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class MagicBallAttackStrategy : AttackStrategy<MagicBallWeaponModel>
    {
        #region Members

        private const float POSITION_OFFSET_Y = 0.4f;
        private const string PIVOT_NAME = "weapon_pivot_point";
        private const string NORMAL_PROJECTILE_NAME = "weapon_110005_normal_projectile";
        private const string SPECIAL_IDLE_PROJECTILE_NAME = "weapon_110005_special_idle_projectile";
        private const string SPECIAL_ACTIVE_PROJECTILE_NAME = "weapon_110005_special_active_projectile";
        private const string SPECIAL_MUZZLE_VFX_NAME = "weapon_110005_special_muzzle_effect";
        private const string NORMAL_MUZZLE_VFX_NAME = "weapon_110005_normal_muzzle_effect";
        private const string PROJECTILE_SPAWN_POINT_TRANSFORM_NAME = "spawn_vfx_point";
        private static readonly float s_angleOffset = 90f;
        private static readonly float s_angleOffsetWithHolderDirection = 1f;
        private int _bonusDamageForProjectileAfterFiredSomeCounter;
        private bool _isSpawningSmallProjectile;
        private bool _isDeployingSpecialAttack;
        private List<Projectile> _spawnedProjectiles;
        private Transform _weaponCenterPointTransform;
        private Transform _projectileSpawnPointTransform;
        private CancellationTokenSource _updateCancellationTokenSource;

        #endregion Members

        #region Class Methods

        public override void Init(WeaponModel weaponModel, CharacterModel creatorModel, Transform creatorTransform)
        {
            base.Init(weaponModel, creatorModel, creatorTransform);
            _spawnedProjectiles = new List<Projectile>();
            _isDeployingSpecialAttack = false;
            _isSpawningSmallProjectile = false;
            _weaponCenterPointTransform = new GameObject(PIVOT_NAME).transform;
            _weaponCenterPointTransform.position = creatorModel.Position + new Vector2(0, POSITION_OFFSET_Y);
            _updateCancellationTokenSource = new CancellationTokenSource();
            _projectileSpawnPointTransform = creatorTransform.FindChildTransform(PROJECTILE_SPAWN_POINT_TRANSFORM_NAME);
            if (_projectileSpawnPointTransform == null)
            {
#if DEBUGGING
                Debug.LogError("No projectile spawn point transform is found, the character's position will be the projectile spawn position instead!");
#endif
            }
            UpdateAsync(_updateCancellationTokenSource.Token).Forget();
        }

        public override void Dispose()
        {
            base.Dispose();
            Object.Destroy(_weaponCenterPointTransform.gameObject);
        }

        public override void Cancel()
        {
            base.Cancel();
            foreach (var scissorProjectile in _spawnedProjectiles)
            {
                if (scissorProjectile != null)
                    PoolManager.Instance.Remove(scissorProjectile.gameObject);
            }
            _spawnedProjectiles = null;
            _updateCancellationTokenSource?.Cancel();
        }

        public override bool CheckCanAttack()
            => base.CheckCanAttack() && !_isDeployingSpecialAttack;

        public override bool CheckCanSpecialAttack()
            => ownerWeaponModel.IsSpecialAttackReady && !_isDeployingSpecialAttack;

        protected override async UniTask TriggerAttack(CancellationToken cancellationToken)
        {
            _isSpawningSmallProjectile = true;
            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.NormalAttack,
                operatedPointTriggeredCallbackAction: () => {
                    Vector2 projectilePosition = _projectileSpawnPointTransform != null ? _projectileSpawnPointTransform.position : creatorModel.Position;
                    SpawnMuzzleAsync(NORMAL_MUZZLE_VFX_NAME, cancellationToken, projectilePosition).Forget();
                    SpawnNormalProjectile(creatorModel, ownerWeaponModel.DamageFactors, projectilePosition, cancellationToken).Forget();
                    SpawnIdleProjectilesAsync(cancellationToken).Forget();
                },
                endActionCallbackAction: () => {
                    _isSpawningSmallProjectile = false;
                }
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            await UniTask.WaitUntil(() => !_isSpawningSmallProjectile, cancellationToken: cancellationToken);
        }

        protected override async UniTask TriggerSpecialAttack(CancellationToken cancellationToken)
        {
            _isDeployingSpecialAttack = true;
            while (_spawnedProjectiles.Count > 0)
            {
                await UniTask.Yield(cancellationToken);
                var selectedIndex = -1;
                var numberOfSpawnedProjectiles = _spawnedProjectiles.Count;
                for (int i = 0; i < numberOfSpawnedProjectiles; i++)
                {
                    var direction = _spawnedProjectiles[i].CenterPosition - creatorModel.Position;
                    var angle = Vector2.SignedAngle(direction, creatorModel.FaceDirection);
                    if (Mathf.Abs(angle) <= ownerWeaponModel.ProjectileFireRotateSpeed * Time.deltaTime + s_angleOffsetWithHolderDirection)
                    {
                        var projectileDamageFactors = ownerWeaponModel.ProjectileFlyDamageFactors;
                        if (ownerWeaponModel.CanAddBonusDamageForProjectileAfterFiredSome)
                        {
                            _bonusDamageForProjectileAfterFiredSomeCounter++;
                            if (_bonusDamageForProjectileAfterFiredSomeCounter > ownerWeaponModel.BonusDamageAfterFireProjectilesCount +
                                                                                 ownerWeaponModel.ConsecutiveBonusDamageAfterFireProjectilesCount)
                            {
                                _bonusDamageForProjectileAfterFiredSomeCounter = 0;
                            }
                            else if (_bonusDamageForProjectileAfterFiredSomeCounter > ownerWeaponModel.BonusDamageAfterFireProjectilesCount)
                            {
                                projectileDamageFactors = projectileDamageFactors.Multiply(1 + ownerWeaponModel.BonusDamagePercentAfterFireProjectiles);
                            }
                        }

                        var flyForwardProjectileStrategyData = new FlyForwardProjectileStrategyData(DamageSource.FromSpecialAttack,
                                                                                                    moveDistance: ownerWeaponModel.ProjectileFlyRange,
                                                                                                    moveSpeed: ownerWeaponModel.ProjectileFlySpeed,
                                                                                                    damageFactors: projectileDamageFactors,
                                                                                                    damageBonus: ownerWeaponModel.ProjectileFlyDamageBonus,
                                                                                                    damageModifierModels: ownerWeaponModel.ProjectileFlyDamageModifierModels);

                        var projectileGameObject = await EntitiesManager.Instance.CreateProjectileAsync(SPECIAL_ACTIVE_PROJECTILE_NAME, creatorModel, _spawnedProjectiles[i].CenterPosition, cancellationToken);
                        SpawnMuzzleAsync(SPECIAL_MUZZLE_VFX_NAME, cancellationToken, projectileGameObject.transform.position).Forget();
                        var projectile =  projectileGameObject.GetComponent<Projectile>();
                        var projectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(flyForwardProjectileStrategyData.projectileStrategyType);

                        Vector2 projectileDirection = creatorModel.FaceDirection;
                        if (creatorModel.currentTargetedTarget != null)
                            projectileDirection = creatorModel.currentTargetedTarget.Position - projectile.CenterPosition;

                        projectileStrategy.Init(flyForwardProjectileStrategyData, projectile, projectileDirection, projectile.CenterPosition);
                        projectile.InitStrategies(new[] { projectileStrategy });
                        selectedIndex = i;
                        break;
                    }
                }

                if (selectedIndex != -1)
                {
                    _spawnedProjectiles[selectedIndex].InitStrategies(null);
                    _spawnedProjectiles.RemoveAt(selectedIndex);
                }
            }

            ownerWeaponModel.IsSpecialAttackReady = false;
            _isDeployingSpecialAttack = false;
        }

        private async UniTask SpawnMuzzleAsync(string muzzleName, CancellationToken cancellationToken, Vector2 position)
        {
            var muzzle = await PoolManager.Instance.Get(muzzleName, cancellationToken: cancellationToken);
            muzzle.transform.position = position;
        }

        private async UniTask SpawnNormalProjectile(CharacterModel holder, DamageFactor[] damageFactors, Vector2 spawnPoint, CancellationToken cancellationToken)
        {
            var flyForwardProjectileStrategyData = new FlyForwardProjectileStrategyData(DamageSource.FromNormalAttack,
                                                                                        moveDistance: ownerWeaponModel.NormalFlyRange,
                                                                                        moveSpeed: ownerWeaponModel.NormalFlySpeed,
                                                                                        damageFactors: damageFactors, damageModifierModels: ownerWeaponModel.DamageModifierModels);
            var projectileGameObject = await EntitiesManager.Instance.CreateProjectileAsync(NORMAL_PROJECTILE_NAME, creatorModel, spawnPoint, cancellationToken);
            var projectile = projectileGameObject.GetComponent<Projectile>();
            var projectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(flyForwardProjectileStrategyData.projectileStrategyType);

            Vector2 direction = holder.FaceDirection;
            if (holder.currentTargetedTarget != null)
                direction = holder.currentTargetedTarget.Position - spawnPoint;

            projectileStrategy.Init(flyForwardProjectileStrategyData, projectile, direction, spawnPoint);
            projectile.InitStrategies(new[] { projectileStrategy });
        }

        private async UniTaskVoid SpawnIdleProjectilesAsync(CancellationToken cancellationToken)
        {
            if (_spawnedProjectiles.Count < ownerWeaponModel.NumberOfProjectiles)
            {
                var projectileGameObject = await EntitiesManager.Instance.CreateProjectileAsync(SPECIAL_IDLE_PROJECTILE_NAME, creatorModel, Vector2.zero, cancellationToken);
                var projectile = projectileGameObject.GetComponent<Projectile>();
                IdleThroughProjectileStrategyData idleThroughProjectileStrategyData;
                if (ownerWeaponModel.AllowProjectileIdleToDamage)
                {
                    idleThroughProjectileStrategyData = new IdleThroughProjectileStrategyData(DamageSource.FromOther, _weaponCenterPointTransform,
                                                                                               damageBonus: ownerWeaponModel.ProjectileIdleDamageBonus,
                                                                                              damageFactors: ownerWeaponModel.ProjectileIdleDamageFactors,
                                                                                              damageModifierModels: ownerWeaponModel.ProjectileIdleDamageModifierModels);
                }
                else idleThroughProjectileStrategyData = new IdleThroughProjectileStrategyData(DamageSource.FromOther, _weaponCenterPointTransform);

                var projectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(idleThroughProjectileStrategyData.projectileStrategyType);
                var angle = _spawnedProjectiles.Count > 0 ? Constants.CIRCLE_DEGREES / _spawnedProjectiles.Count : Constants.CIRCLE_DEGREES;
                var projectileSpawnDirection = Quaternion.AngleAxis(angle * (_spawnedProjectiles.Count - 1), Vector3.forward) * Vector2.up;
                var projectileSpawnPosition = creatorModel.Position + (Vector2)projectileSpawnDirection * ownerWeaponModel.ProjectileCenterOffsetRadius;
                projectileStrategy.Init(idleThroughProjectileStrategyData, projectile, projectileSpawnDirection, projectileSpawnPosition);
                projectile.InitStrategies(new[] { projectileStrategy });
                _spawnedProjectiles.Add(projectile);
                ownerWeaponModel.IsSpecialAttackReady = true;
                RearrangeSpawnedProjectiles();
            }
        }

        private void RearrangeSpawnedProjectiles()
        {
            var angle = Constants.CIRCLE_DEGREES / _spawnedProjectiles.Count;
            for (int i = 0; i < _spawnedProjectiles.Count; i++)
            {
                var spawnedProjectile = _spawnedProjectiles[i];
                var direction = Quaternion.AngleAxis(angle * i, Vector3.forward) * Vector2.up;
                spawnedProjectile.transform.SetParent(_weaponCenterPointTransform);
                spawnedProjectile.transform.localRotation = Quaternion.AngleAxis(angle * i + s_angleOffset, Vector3.forward);
                spawnedProjectile.transform.localPosition = direction * ownerWeaponModel.ProjectileCenterOffsetRadius;
            }
        }

        private async UniTaskVoid UpdateAsync(CancellationToken cancellationToken)
        {
            while (!creatorModel.IsDead)
            {
                _weaponCenterPointTransform.position = creatorModel.Position + new Vector2(0, POSITION_OFFSET_Y);
                if (_spawnedProjectiles.Count > 0)
                {
                    if (_isDeployingSpecialAttack)
                        _weaponCenterPointTransform.Rotate(Vector3.forward, ownerWeaponModel.ProjectileFireRotateSpeed * Time.fixedDeltaTime);
                    else
                        _weaponCenterPointTransform.Rotate(Vector3.forward, ownerWeaponModel.ProjectileIdleRotateSpeed * Time.fixedDeltaTime);
                }
                await UniTask.WaitForFixedUpdate(cancellationToken);
            }
        }

        #endregion Class Methods
    }
}
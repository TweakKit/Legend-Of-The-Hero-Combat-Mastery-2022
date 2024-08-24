using System.Threading;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Runtime.Extensions;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class BidetGunAttackStrategy : AttackStrategy<BidetGunWeaponModel>
    {
        #region Members

        private const string NORMAL_PROJECTILE_NAME = "weapon_110000_normal_projectile";
        private const string PROJECTILE_SPAWN_POINT_TRANSFORM_NAME = "spawn_vfx_point";
        private bool _isShooting;
        private Transform _projectileSpawnPointTransform;

        #endregion Members

        #region Class Methods

        public override void Init(WeaponModel weaponModel, CharacterModel creatorModel, Transform creatorTransform)
        {
            base.Init(weaponModel, creatorModel, creatorTransform);
            _projectileSpawnPointTransform = creatorTransform.FindChildTransform(PROJECTILE_SPAWN_POINT_TRANSFORM_NAME);
#if DEBUGGING
            if (_projectileSpawnPointTransform == null)
                Debug.LogError("No projectile spawn point transform is found, the character's position will be the projectile spawn position instead!");
#endif
        }

        public override bool CheckCanAttack()
            => base.CheckCanAttack() && !_isShooting;

        public override bool CheckCanSpecialAttack()
            => ownerWeaponModel.IsSpecialAttackReady && !_isShooting;

        protected override async UniTask TriggerAttack(CancellationToken cancellationToken)
        {
            _isShooting = true;
            ownerWeaponModel.CurrentCoundownCounterCount = ownerWeaponModel.CurrentCoundownCounterCount - 1 > 0 ? ownerWeaponModel.CurrentCoundownCounterCount - 1 : 0;
            ownerWeaponModel.IsSpecialAttackReady = ownerWeaponModel.CurrentCoundownCounterCount == 0;
            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.NormalAttack,
                operatedPointTriggeredCallbackAction: () => {
                    var numberOfProjectiles = ownerWeaponModel.NumberOfProjectiles;
                    var projectileCenterAngleOffset = ownerWeaponModel.ProjectileCenterAngleOffset;
                    var isSpecialAttack = false;
                    FireProjectiles(numberOfProjectiles, projectileCenterAngleOffset, isSpecialAttack, cancellationToken);
                },
                endActionCallbackAction: () => {
                    _isShooting = false;
                }
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            await UniTask.WaitUntil(() => !_isShooting, cancellationToken: cancellationToken);
        }

        protected override async UniTask TriggerSpecialAttack(CancellationToken cancellationToken)
        {
            _isShooting = true;
            ownerWeaponModel.CurrentCoundownCounterCount = ownerWeaponModel.CountdownCounterCount;
            ownerWeaponModel.IsSpecialAttackReady = false;
            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.SpecialAttack,
                operatedPointTriggeredCallbackAction: () => {
                    var numberOfProjectiles = ownerWeaponModel.NumberOfProjectiles + ownerWeaponModel.BonusNumberOfProjectiles;
                    var projectileCenterAngleOffset = ownerWeaponModel.ProjectileCenterAngleOffset;
                    var isSpecialAttack = true;
                    FireProjectiles(numberOfProjectiles, projectileCenterAngleOffset, isSpecialAttack, cancellationToken);
                },
                endActionCallbackAction: () => {
                    _isShooting = false;
                }
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            await UniTask.WaitUntil(() => !_isShooting, cancellationToken: cancellationToken);
        }

        private void FireProjectiles(int numberOfProjectiles, float projectileCenterAngleOffset, bool isSpecialAttack, CancellationToken cancellationToken)
        {
            var bigAngle = numberOfProjectiles * projectileCenterAngleOffset;
            var firstDegree = - bigAngle / 2;
            Vector2 projectilePosition = _projectileSpawnPointTransform != null ? _projectileSpawnPointTransform.position : creatorModel.Position;
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                var direction = (Quaternion.AngleAxis(firstDegree + projectileCenterAngleOffset * i, Vector3.forward) * creatorModel.FaceDirection).normalized;
                SpawnProjectileAsync(isSpecialAttack, direction, projectilePosition, cancellationToken).Forget();
            }
        }

        private async UniTaskVoid SpawnProjectileAsync(bool isSpecialAttack, Vector2 direction, Vector2 projectilePosition, CancellationToken cancellationToken)
        {
            FlyForwardProjectileStrategyData flyForwardProjectileStrategyData = null;
            if (isSpecialAttack)
            {
                flyForwardProjectileStrategyData = new FlyForwardProjectileStrategyData(DamageSource.FromSpecialAttack,
                                                                                        ownerWeaponModel.AttackRange,
                                                                                        ownerWeaponModel.ProjectileSpeed,
                                                                                        ownerWeaponModel.SpecialDamageBonus,
                                                                                        damageFactors: ownerWeaponModel.SpecialDamageFactors,
                                                                                        damageModifierModels: ownerWeaponModel.SpecialDamageModifierModels);
            }
            else
            {
                flyForwardProjectileStrategyData = new FlyForwardProjectileStrategyData(DamageSource.FromNormalAttack,
                                                                                        ownerWeaponModel.AttackRange,
                                                                                        ownerWeaponModel.ProjectileSpeed,
                                                                                        damageBonus: ownerWeaponModel.DamageBonus,
                                                                                        damageFactors: ownerWeaponModel.DamageFactors,
                                                                                        damageModifierModels: ownerWeaponModel.DamageModifierModels);
            }

            var projectileGameObject = await EntitiesManager.Instance.CreateProjectileAsync(NORMAL_PROJECTILE_NAME, creatorModel, projectilePosition, cancellationToken);
            var projectile = projectileGameObject.GetComponent<Projectile>();
            var projectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(ProjectileStrategyType.Forward);
            projectileStrategy.Init(flyForwardProjectileStrategyData, projectile, direction, projectilePosition);
            projectile.InitStrategies(new[] { projectileStrategy });
        }

        #endregion Class Methods
    }
}
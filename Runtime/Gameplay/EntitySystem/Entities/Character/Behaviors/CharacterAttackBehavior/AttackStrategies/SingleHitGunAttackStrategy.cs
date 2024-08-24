using System.Threading;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Extensions;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class SingleHitGunAttackStrategy : AttackStrategy<SingleHitGunWeaponModel>
    {
        #region Members

        private const string NORMAL_PROJECTILE_NAME = "weapon_110003_special_projectile";
        private const string SPECIAL_PROJECTILE_NAME = "weapon_110003_normal_projectile";
        private const string SPECIAL_GRAPHIC = "graphic_special";
        private const string NORMAL_GRAPHIC = "graphic_normal";
        private const string PASSIVE_PROJECTILE_NAME = "weapon_110003_rotten_eggs";
        private const string PROJECTILE_SPAWN_POINT_TRANSFORM_NAME = "spawn_vfx_point";
        private bool _isShooting;
        private GameObject _specialGraphicGameObject;
        private GameObject _normalGraphicGameObject;
        private Transform _projectileSpawnPointTransform;
        private CancellationTokenSource _triggeredSpecialCancellationTokenSource;
        private CancellationTokenSource _specialAttackCooldownCancellationTokenSource;

        #endregion Members

        #region Class Methods

        public override void Init(WeaponModel weaponModel, CharacterModel creatorModel, Transform creatorTransform)
        {
            base.Init(weaponModel, creatorModel, creatorTransform);
            _isShooting = false;
            _specialGraphicGameObject = creatorTransform.FindChildGameObject(SPECIAL_GRAPHIC);
            _normalGraphicGameObject = creatorTransform.FindChildGameObject(NORMAL_GRAPHIC);
            _projectileSpawnPointTransform = creatorTransform.FindChildTransform(PROJECTILE_SPAWN_POINT_TRANSFORM_NAME);
            if (_projectileSpawnPointTransform == null)
            {
#if DEBUGGING
                Debug.LogError("No projectile spawn point transform is found, the character's position will be the projectile spawn position instead!");
#endif
            }
            UpdateGraphic();
        }

        public override bool CheckCanAttack()
            => base.CheckCanAttack() && !_isShooting;

        public override bool CheckCanSpecialAttack()
        {
            return ownerWeaponModel.IsSpecialAttackReady;
        }

        public override void Cancel()
        {
            base.Cancel();
            _specialAttackCooldownCancellationTokenSource?.Cancel();
            _triggeredSpecialCancellationTokenSource?.Cancel();
            ownerWeaponModel.IsSpecialAttackReady = true;
            ownerWeaponModel.HasTriggeredSpecialAttack = false;
        }

        protected override async UniTask TriggerAttack(CancellationToken cancellationToken)
        {
            _isShooting = true;

            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.NormalAttack,
                operatedPointTriggeredCallbackAction: () => {
                    if (!ownerWeaponModel.HasTriggeredSpecialAttack)
                    {
                        Vector2 projectilePosition = _projectileSpawnPointTransform != null ? _projectileSpawnPointTransform.position : creatorModel.Position;

                        var attackRange = ownerWeaponModel.AttackRange;

                        SpawnProjectileAsync(DamageSource.FromNormalAttack, attackRange, creatorModel.FaceDirection, projectilePosition, false, cancellationToken).Forget();
                    }
                    else
                    {
                        var attackRange = ownerWeaponModel.AttackRange;
                        if (ownerWeaponModel.CanExtendAttackRangeWhenInSpecialAttackState)
                            attackRange = ownerWeaponModel.AttackRange * (1 + ownerWeaponModel.SpecialAttackBonusAttackRangePercent);

                        Vector2 projectilePosition = _projectileSpawnPointTransform != null ? _projectileSpawnPointTransform.position : creatorModel.Position;
                        SpawnProjectileAsync(DamageSource.FromSpecialAttack, attackRange, creatorModel.FaceDirection, projectilePosition, true, cancellationToken).Forget();
                    }
                },
                endActionCallbackAction: () => {
                    _isShooting = false;
                }
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            await UniTask.WaitUntil(() => !_isShooting, cancellationToken: cancellationToken);
        }

        protected override UniTask TriggerSpecialAttack(CancellationToken cancellationToken)
        {
            _triggeredSpecialCancellationTokenSource = new CancellationTokenSource();
            TriggerSpecialAttackAsync(cancellationToken).Forget();
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid TriggerSpecialAttackAsync(CancellationToken cancellationToken)
        {
            ownerWeaponModel.IsSpecialAttackReady = false;
            ownerWeaponModel.HasTriggeredSpecialAttack = true;
            ownerWeaponModel.CurrentSpecialAttackDuration = ownerWeaponModel.SpecialAttackDuration;

            UpdateGraphic();

            if (ownerWeaponModel.CanCatchTargetsInSpecialAttackRangeToAffect)
            {
                var attackRange = ownerWeaponModel.AttackRange;
                if (ownerWeaponModel.CanExtendAttackRangeWhenInSpecialAttackState)
                    attackRange = ownerWeaponModel.AttackRange * (1 + ownerWeaponModel.SpecialAttackBonusAttackRangePercent);
                CatchTargetsInSpecialAttackRangeToAffect(attackRange, cancellationToken).Forget();
            }

            creatorModel.DebuffStat(StatType.MoveSpeed, ownerWeaponModel.SpecialAttackDecreaseMovementSpeedPercent, StatModifyType.BaseMultiply);
            creatorModel.BuffStat(StatType.AttackSpeed, ownerWeaponModel.SpecialAttackIncreaseAttackSpeedPercent, StatModifyType.BaseMultiply);

            while (ownerWeaponModel.CurrentSpecialAttackDuration > 0 && ownerWeaponModel.HasTriggeredSpecialAttack)
            {
                ownerWeaponModel.CurrentSpecialAttackDuration -= Time.deltaTime;
                creatorModel.ActionTriggeredEvent.Invoke(ActionInputType.Attack);
                await UniTask.Yield(_triggeredSpecialCancellationTokenSource.Token);
            }

            ConvertBackNormalState();
        }


        private void StopUsingSpecial()
        {
            _triggeredSpecialCancellationTokenSource?.Cancel();
            ConvertBackNormalState();
        }

        private void ConvertBackNormalState()
        {
            ownerWeaponModel.HasTriggeredSpecialAttack = false;
            UpdateGraphic();
            creatorModel.BuffStat(StatType.MoveSpeed, ownerWeaponModel.SpecialAttackDecreaseMovementSpeedPercent, StatModifyType.BaseMultiply);
            creatorModel.DebuffStat(StatType.AttackSpeed, ownerWeaponModel.SpecialAttackIncreaseAttackSpeedPercent, StatModifyType.BaseMultiply);
            RunSpecialAttackCooldownAsync().Forget();
        }

        private async UniTaskVoid RunSpecialAttackCooldownAsync()
        {
            _specialAttackCooldownCancellationTokenSource = new CancellationTokenSource();
            ownerWeaponModel.CurrentSpecialAttackCooldownTime = ownerWeaponModel.SpecialAttackCooldownTime;
            while (ownerWeaponModel.CurrentSpecialAttackCooldownTime > 0)
            {
                ownerWeaponModel.CurrentSpecialAttackCooldownTime -= Time.deltaTime;
                await UniTask.Yield(_specialAttackCooldownCancellationTokenSource.Token);
            }
            FinishSpecialAttackCooldown();
        }

        private void FinishSpecialAttackCooldown()
            => ownerWeaponModel.IsSpecialAttackReady = true;

        private void UpdateGraphic()
        {
            _normalGraphicGameObject.SetActive(!ownerWeaponModel.HasTriggeredSpecialAttack);
            _specialGraphicGameObject.SetActive(ownerWeaponModel.HasTriggeredSpecialAttack);
        }

        private async UniTask CatchTargetsInSpecialAttackRangeToAffect(float affectRange, CancellationToken cancellationToken)
        {
            var colliders = Physics2D.OverlapCircleAll(creatorModel.Position, affectRange);
            foreach (var collider in colliders)
            {
                var entity = collider.GetComponent<IEntity>();
                if (entity != null)
                {
                    var interactable = entity.GetBehavior<IInteractable>();
                    if (interactable != null && !interactable.Model.IsDead && interactable.Model != creatorModel)
                    {
                        if (interactable.Model.EntityType.IsCharacter())
                        {
                            var affectedStatusEffectInfo = new AffectedStatusEffectInfo(ownerWeaponModel.SpecialAttackAffectedModifierModels, null, creatorModel);
                            var affectedStatusEffectDirection = (interactable.Model.Position - creatorModel.Position).normalized;
                            var passiveBullet = await PoolManager.Instance.Get(PASSIVE_PROJECTILE_NAME, cancellationToken);
                            passiveBullet.transform.position = creatorModel.Position;
                            FollowTargetObject followTarget = passiveBullet.GetComponent<FollowTargetObject>();
                            followTarget.Init(interactable, 15f, creatorModel.Position, affectRange, affectedStatusEffectInfo, new StatusEffectMetaData(affectedStatusEffectDirection, creatorModel.Position));
                        }
                    }
                }
            }
        }

        private async UniTaskVoid SpawnProjectileAsync(DamageSource damageSource, float projectileFlyRange, Vector2 direction, Vector2 spawnPoint, bool isSpecial, CancellationToken token)
        {
            FlyForwardProjectileStrategyData flyForwardProjectileStrategyData = new FlyForwardProjectileStrategyData(damageSource,
                                                                                                                     projectileFlyRange,
                                                                                                                     ownerWeaponModel.ProjectileSpeed,
                                                                                                                     damageBonus: ownerWeaponModel.DamageBonus,
                                                                                                                     damageFactors: ownerWeaponModel.DamageFactors,
                                                                                                                     damageModifierModels: ownerWeaponModel.DamageModifierModels);
            var projectileGameObject = await EntitiesManager.Instance.CreateProjectileAsync(isSpecial ? SPECIAL_PROJECTILE_NAME : NORMAL_PROJECTILE_NAME, creatorModel, spawnPoint, token);
            var projectile = projectileGameObject.GetComponent<Projectile>();
            var projectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(ProjectileStrategyType.Forward);
            projectileStrategy.Init(flyForwardProjectileStrategyData, projectile, direction, spawnPoint);
            projectile.InitStrategies(new[] { projectileStrategy });
        }

        #endregion Class Methods
    }
}
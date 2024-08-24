using System.Linq;
using System.Threading;
using UnityEngine;
using Runtime.Extensions;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class MagicMaceAttackStrategy : AttackStrategy<MagicMaceWeaponModel>
    {
        #region Members

        private const string AOE_RANGE_EFFECT_NAME = "aoe_range_display_vfx";
        private const string LIGHTNING_NORMAL_VFX = "weapon_110002_lighting_normal";
        private const string LIGHTNING_SPECIAL_VFX = "weapon_110002_lighting_special";
        private const string LIGHTNING_ORIGIN_POINT_TRANSFORM_NAME = "spawn_vfx_point";
        private bool _isFiringLightning;
        private bool _isSpecialCoolingDown;
        private Transform _lightningOriginPointTransform;
        private GameObject _areaOfEffect;
        private CancellationTokenSource _specialAttackCooldownCancellationTokenSource;
        private CancellationTokenSource _updateCancellationTokenSource;

        #endregion Members

        #region Class Methods

        public override void Init(WeaponModel weaponModel, CharacterModel creatorModel, Transform creatorTransform)
        {
            base.Init(weaponModel, creatorModel, creatorTransform);
            _isFiringLightning = false;
            _lightningOriginPointTransform = creatorTransform.FindChildTransform(LIGHTNING_ORIGIN_POINT_TRANSFORM_NAME);
            if (_lightningOriginPointTransform == null)
            {
#if DEBUGGING
                Debug.LogError("No lightning origin point transform is found, the character's position will be the lightning origin point instead!");
#endif
            }

            ownerWeaponModel.IsSpecialAttackReady = true;
            _updateCancellationTokenSource = new();
            InitAsync(_updateCancellationTokenSource.Token).Forget();
        }

        public override void Cancel()
        {
            base.Cancel();
            _updateCancellationTokenSource?.Cancel();
            _specialAttackCooldownCancellationTokenSource?.Cancel();
            ownerWeaponModel.IsSpecialAttackReady = true;
            _isSpecialCoolingDown = false;

            if (_areaOfEffect != null)
                PoolManager.Instance.Remove(_areaOfEffect);
        }

        public override bool CheckCanAttack()
           => base.CheckCanAttack() && !_isFiringLightning;

        public override bool CheckCanSpecialAttack()
            => ownerWeaponModel.IsSpecialAttackReady && !_isFiringLightning;

        public async override UniTask OperateSpecialAttack()
        {
            _isSpecialCoolingDown = true;
            await base.OperateSpecialAttack();
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
            => _isSpecialCoolingDown = false;

        protected override async UniTask TriggerAttack(CancellationToken cancellationToken)
        {
            _isFiringLightning = true;
            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.NormalAttack,
                operatedPointTriggeredCallbackAction: () => {
                    Vector2 lightningOriginPosition = _lightningOriginPointTransform != null ? _lightningOriginPointTransform.position : creatorModel.Position;
                    SpawnLightning(lightningOriginPosition, cancellationToken);
                },
                endActionCallbackAction: () => {
                    _isFiringLightning = false;
                }
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            await UniTask.WaitUntil(() => !_isFiringLightning, cancellationToken: cancellationToken);
        }

        protected override async UniTask TriggerSpecialAttack(CancellationToken cancellationToken)
        {
            _isFiringLightning = true;
            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.SpecialAttack,
                operatedPointTriggeredCallbackAction: () => {
                    SpawnSpecialLightning(cancellationToken);
                },
                endActionCallbackAction: () => {
                    _isFiringLightning = false;
                }
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            await UniTask.WaitUntil(() => !_isFiringLightning, cancellationToken: cancellationToken);
        }

        private void SpawnLightning(Vector2 spawnVfxPosition, CancellationToken cancellationToken)
        {
            var hitEnemiesCount = 0;
            var baseAttackDamage = creatorModel.GetTotalStatValue(StatType.AttackDamage);
            var colliders = Physics2D.OverlapCircleAll(creatorModel.Position, ownerWeaponModel.AttackRange);
            if (ownerWeaponModel.CanCauseScaleDamageForLimitedEnemies)
            {
                foreach (var collider in colliders)
                {
                    var entity = collider.GetComponent<IEntity>();
                    if (entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>();
                        if (interactable != null && !interactable.Model.IsDead && interactable.Model.EntityType.IsCharacter())
                        {
                            if (creatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                                hitEnemiesCount++;
                        }
                    }
                }
            }

            if (ownerWeaponModel.CanCauseScaleDamageForLimitedEnemies && hitEnemiesCount == ownerWeaponModel.LimitedEnemyTriggerScaleDamageCount)
                creatorModel.BuffStat(StatType.AttackDamage, baseAttackDamage * (ownerWeaponModel.ScaleDamageWhenAttackLimitedEnemy - 1), StatModifyType.BaseBonus);

            foreach (var collider in colliders)
            {
                var entity = collider.GetComponent<IEntity>();
                if (entity != null)
                {
                    var interactable = entity.GetBehavior<IInteractable>();
                    if (interactable != null && !interactable.Model.IsDead)
                    {
                        if (creatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                        {
                            var damageFactors = ownerWeaponModel.DamageFactors;
                            if (interactable.Model.EntityType.IsCharacter())
                            {
                                if (ownerWeaponModel.CanCauseMoreDamageForEffectedEnemies)
                                {
                                    var damageTargetModel = interactable.Model as CharacterModel;
                                    foreach (var statusEffectModel in ownerWeaponModel.ConvertStatusEffectModels)
                                    {
                                        if (damageTargetModel.GetStatusEffectStackCount(statusEffectModel.StatusEffectType) > 0)
                                            damageFactors = damageFactors.Multiply(1 + ownerWeaponModel.BonusDamagePercentForEffectedEnemies);
                                    }
                                }
                            }
                            var damageInfo = creatorModel.GetDamageInfo(DamageSource.FromNormalAttack, ownerWeaponModel.DamageBonus, damageFactors, ownerWeaponModel.DamageModifierModels, interactable.Model);
                            var damageDirection = (interactable.Model.Position - creatorModel.Position).normalized;
                            SpawnNormalVFX(interactable.Model, spawnVfxPosition, cancellationToken).Forget();
                            interactable.GetHit(damageInfo, new DamageMetaData(damageDirection, creatorModel.Position));
                        }
                    }
                }
            }

            if (ownerWeaponModel.CanCauseScaleDamageForLimitedEnemies && hitEnemiesCount == ownerWeaponModel.LimitedEnemyTriggerScaleDamageCount)
                creatorModel.DebuffStat(StatType.AttackDamage, baseAttackDamage * (ownerWeaponModel.ScaleDamageWhenAttackLimitedEnemy - 1), StatModifyType.BaseBonus);
        }

        private void SpawnSpecialLightning(CancellationToken cancellationToken)
        {
            var colliders = Physics2D.OverlapCircleAll(creatorModel.Position, ownerWeaponModel.AttackRange);
            foreach (var collider in colliders)
            {
                var entity = collider.GetComponent<IEntity>();
                if (entity != null)
                {
                    var interactable = entity.GetBehavior<IInteractable>();
                    if (interactable != null && !interactable.Model.IsDead && interactable.Model != creatorModel)
                    {
                        TriggerAffectStatusEffect(interactable, cancellationToken);
                    }
                }
            }
        }

        private void TriggerAffectStatusEffect(IInteractable interactable, CancellationToken cancellationToken)
        {
            if (interactable.Model.EntityType.IsCharacter())
            {
                var cutOffStatusEffectTypes = ownerWeaponModel.DamageModifierModels.Select(x => x.StatusEffectType).ToArray();
                var affectedStatusEffectModels = ownerWeaponModel.ConvertStatusEffectModels;
                var affectedStatusEffectInfo = new AffectedStatusEffectInfo(affectedStatusEffectModels, cutOffStatusEffectTypes, creatorModel);
                var affectedStatusEffectDirection = (interactable.Model.Position - creatorModel.Position).normalized;

                SpawnSpecialVFX(interactable.Model, cancellationToken).Forget();
                interactable.GetAffected(affectedStatusEffectInfo, new StatusEffectMetaData(affectedStatusEffectDirection, creatorModel.Position));
            }
        }

        private async UniTaskVoid SpawnSpecialVFX(EntityModel targetModel, CancellationToken cancellationToken)
        {
            var vfx = await PoolManager.Instance.Get(LIGHTNING_SPECIAL_VFX);
            vfx.transform.position = targetModel.Position;
        }

        private async UniTaskVoid SpawnNormalVFX(EntityModel targetModel, Vector2 startPosition, CancellationToken cancellationToken)
        {
            var vfx = await PoolManager.Instance.Get(LIGHTNING_NORMAL_VFX, cancellationToken);
            vfx.transform.position = startPosition;

            var scalableVFX = vfx.GetComponent<ScalableVFX>();
            if (scalableVFX)
                scalableVFX.Scale((targetModel.Position - startPosition).magnitude);
            var direction = (targetModel.Position - startPosition).normalized;
            scalableVFX.transform.rotation = direction.ToQuaternion();
        }

        private async UniTaskVoid InitAsync(CancellationToken cancellationToken)
        {
            _areaOfEffect = await PoolManager.Instance.Get(AOE_RANGE_EFFECT_NAME, cancellationToken);
            _areaOfEffect.transform.localScale = Vector3.one * ownerWeaponModel.AttackRange;

            while (!creatorModel.IsDead)
            {
                _areaOfEffect.transform.position = creatorModel.Position;
                if (creatorModel.currentTargetedTarget != null && creatorModel.currentTargetedTarget.EntityType.IsCharacter()
                    && !creatorModel.currentTargetedTarget.IsDead && !_isSpecialCoolingDown && Vector2.Distance(creatorModel.Position, creatorModel.currentTargetedTarget.Position) <= ownerWeaponModel.AttackRange)
                    ownerWeaponModel.IsSpecialAttackReady = true;
                else
                    ownerWeaponModel.IsSpecialAttackReady = false;

                await UniTask.WaitForFixedUpdate(cancellationToken);
            }
        }
        #endregion Class Methods
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Extensions;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class ShovelAttackStrategy : AttackStrategy<ShovelWeaponModel>
    {
        #region Members

        private const string IMPACT_NAME = "weapon_110006_impact";
        private bool _isSlashing;
        private const string SHOVEL_ORIGIN_POINT_TRANSFORM_NAME = "spawn_vfx_point";
        private const string SPECIAL_DAMAGE_BOX = "special_damage_box";
        private Transform _shovelOriginPointTransform;
        private EntityTargetDetector _specialDamageBox;
        private List<IInteractable> _specialTargets;
        private CancellationTokenSource _specialAttackCooldownCancellationTokenSource;
        private CancellationTokenSource _buffSpeedCancellationTokenSource;
        private CharacterWeapon _characterWeapon;

        private int _currentCountHitForTriggeredBuff;
        private int _currentCountHitPermanent;
        private int _currentRecoredHitIndex;

        #endregion Members

        #region Class Methods

        public override void Init(WeaponModel weaponModel, CharacterModel creatorModel, Transform creatorTransform)
        {
            base.Init(weaponModel, creatorModel, creatorTransform);
            //creatorModel.IgnoreTarget = true;
            _isSlashing = false;
            _currentCountHitPermanent = 0;
            _currentRecoredHitIndex = -1;
            ownerWeaponModel.IsSpecialAttackReady = true;
            _shovelOriginPointTransform = creatorTransform.FindChildTransform(SHOVEL_ORIGIN_POINT_TRANSFORM_NAME);
            var damageBoxGameObject = creatorTransform.FindChildGameObject(SPECIAL_DAMAGE_BOX);
            if (damageBoxGameObject != null)
            {
                _specialDamageBox = damageBoxGameObject.GetComponent<EntityTargetDetector>();
                damageBoxGameObject.SetActive(false);
            }

            _characterWeapon = creatorTransform.GetComponentInChildren<CharacterWeapon>();

#if DEBUGGING
            if (_shovelOriginPointTransform == null)
                Debug.LogError("No projectile spawn point transform is found, the character's position will be the projectile spawn position instead!");
#endif
        }

        public override bool CheckCanAttack()
            => base.CheckCanAttack() && !_isSlashing;

        public override bool CheckCanSpecialAttack()
            => ownerWeaponModel.IsSpecialAttackReady && !_isSlashing;

        protected async override UniTask TriggerAttack(CancellationToken cancellationToken)
        {
            _isSlashing = true;
            _currentCountHitPermanent++;
            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.NormalAttack,
                operatedPointTriggeredCallbackAction: () => {
                    Vector2 slashOriginPosition = _shovelOriginPointTransform != null ? _shovelOriginPointTransform.position : creatorModel.Position;
                    SpawnEffect(creatorModel, ownerWeaponModel.DamageBonus, ownerWeaponModel.DamageFactors, slashOriginPosition, cancellationToken).Forget();
                },
                endActionCallbackAction: () => {
                    _isSlashing = false;
                }
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            await UniTask.WaitUntil(() => !_isSlashing, cancellationToken: cancellationToken);
        }

        public async override UniTask OperateSpecialAttack()
        {
            ownerWeaponModel.IsSpecialAttackReady = false;
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
            => ownerWeaponModel.IsSpecialAttackReady = true;

        protected async override UniTask TriggerSpecialAttack(CancellationToken cancellationToken)
        {
            _isSlashing = true;
            _characterWeapon.PausedRotate = true;
            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.SpecialAttack
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            _specialTargets = new();
            _specialDamageBox.gameObject.SetActive(true);
            _specialDamageBox.Init(colliderEnteredAction: OnSpecialColliderEntered, colliderExitedAction: OnSpecialColliderExited);
            float currentTime = 0;
            float currentIntervalTime = 0;
            while (currentTime < ownerWeaponModel.SpecialDuration)
            {
                currentIntervalTime += Time.deltaTime;
                if (currentIntervalTime >= ownerWeaponModel.IntervalBetweenSpecialDamage)
                {
                    currentIntervalTime = 0;
                    TriggerDamageForTargets();
                }
                currentTime += Time.deltaTime;
                await UniTask.Yield(cancellationToken);
            }

            characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.Idle
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            _specialTargets.Clear();
            _specialDamageBox.gameObject.SetActive(false);
            _isSlashing = false;
            _characterWeapon.PausedRotate = false;
        }

        private void TriggerDamageForTargets()
        {
            _currentCountHitPermanent++;

            var haveTarget = false;
            foreach (var interactable in _specialTargets)
            {
                if (interactable != null && !interactable.Model.IsDead)
                {
                    var hitDirection = interactable.Model.Position - creatorModel.Position;

                    var damageInfo = creatorModel.GetDamageInfo(DamageSource.FromSpecialAttack, ownerWeaponModel.SpecialDamageBonus, ownerWeaponModel.SpecialDamageFactors, null, interactable.Model);
                    if (ownerWeaponModel.CanTriggerStatus && _currentCountHitPermanent % ownerWeaponModel.NumberDamageToTriggerStatus == 0)
                    {
                        damageInfo = creatorModel.GetDamageInfo(DamageSource.FromSpecialAttack, ownerWeaponModel.SpecialDamageBonus, ownerWeaponModel.SpecialDamageFactors, ownerWeaponModel.TriggeredDamageModifierModels, interactable.Model);
                    }
                    
                    interactable.GetHit(damageInfo, new DamageMetaData(hitDirection, creatorModel.Position));
                    CreateImpactEffect(interactable.Model.Position);
                    haveTarget = true;
                }
            }

            if (haveTarget)
            {
                CheckCanBuff();
            }
        }

        private void CheckCanBuff()
        {
            if(ownerWeaponModel.CanIncreaseSpeed)
            {
                var previousCount = _currentCountHitForTriggeredBuff;
                if (_currentCountHitForTriggeredBuff < ownerWeaponModel.MaxStackIncreaseSpeed)
                {
                    _currentCountHitForTriggeredBuff++;
                }

                if(previousCount > 0)
                {
                    creatorModel.DebuffStat(StatType.MoveSpeed, ownerWeaponModel.IncreaseSpeed * previousCount, StatModifyType.BaseBonus);
                }

                creatorModel.BuffStat(StatType.MoveSpeed, ownerWeaponModel.IncreaseSpeed * _currentCountHitForTriggeredBuff, StatModifyType.BaseBonus);
                _buffSpeedCancellationTokenSource?.Cancel();
                _buffSpeedCancellationTokenSource = new();
                StartCooldownBuffAsync(_buffSpeedCancellationTokenSource.Token).Forget();
            }
        }

        private async UniTaskVoid StartCooldownBuffAsync(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(ownerWeaponModel.DurationIncreaseSpeed), cancellationToken: token);
            creatorModel.DebuffStat(StatType.MoveSpeed, ownerWeaponModel.IncreaseSpeed * _currentCountHitForTriggeredBuff, StatModifyType.BaseBonus);
            _currentCountHitForTriggeredBuff = 0;
        }

        protected virtual void CreateImpactEffect(Vector2 impactEffectPosition)
        {
            var impactEffectName = VFXNames.MELEE_ATTACK_IMPACT_EFFECT_PREFAB;
            SpawnImpactEffectAsync(impactEffectName, impactEffectPosition).Forget();
        }

        protected virtual async UniTask SpawnImpactEffectAsync(string impactEffectName, Vector2 impactEffectPosition)
        {
            var impactEffect = await PoolManager.Instance.Get(impactEffectName);
            impactEffect.transform.position = impactEffectPosition;
        }

        private void OnSpecialColliderExited(Collider2D collider)
        {
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>();
                if (interactable != null && !interactable.Model.IsDead)
                {
                    if (creatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                    {
                        _specialTargets?.Remove(interactable);
                    }
                }
            }
        }

        private void OnSpecialColliderEntered(Collider2D collider)
        {
            var projectile = collider.GetComponent<IProjectile>();
            if (projectile != null)
            {
                OnProjectileInteract(projectile);
                return;
            }

            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>();
                if (interactable != null && !interactable.Model.IsDead)
                {
                    if (creatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                    {
                        _specialTargets?.Add(interactable);
                    }
                }
            }
        }

        private async UniTask SpawnEffect(CharacterModel holder, float damageBonus, DamageFactor[] damageFactors, Vector2 spawnPoint, CancellationToken cancellationToken)
        {
            var shovelGameObject = await PoolManager.Instance.Get(IMPACT_NAME, cancellationToken: cancellationToken);
            shovelGameObject.transform.position = spawnPoint;
            shovelGameObject.transform.localScale = holder.FaceRight ? new Vector2(-1, 1) : new Vector2(1, 1);
            shovelGameObject.transform.rotation = holder.FaceRight ? holder.FaceDirection.ToQuaternion(0) : (-holder.FaceDirection).ToQuaternion(0);
            var damageBox = shovelGameObject.GetComponent<WeaponDamageBox>();

            if (ownerWeaponModel.CanTriggerStatus && _currentCountHitPermanent % ownerWeaponModel.NumberDamageToTriggerStatus == 0)
                damageBox.Init(holder, DamageSource.FromNormalAttack, destroyWithCreator: true, damageFactors: damageFactors, damageBonus: damageBonus, 
                    modifierModels: ownerWeaponModel.TriggeredDamageModifierModels, 
                    projectileInteractAction: OnProjectileInteract, createdDamageAction: interactable => OnCreatedDamage(_currentCountHitPermanent, interactable),
                    direction: holder.FaceDirection);
            else
                damageBox.Init(holder, DamageSource.FromNormalAttack, destroyWithCreator: true, damageFactors: damageFactors, damageBonus: damageBonus, 
                    projectileInteractAction: OnProjectileInteract, createdDamageAction: interactable => OnCreatedDamage(_currentCountHitPermanent, interactable), 
                    direction: holder.FaceDirection); 
        }

        private void OnCreatedDamage(int hitIndex, IInteractable interactable)
        {
            if (hitIndex != _currentRecoredHitIndex)
            {
                _currentRecoredHitIndex = hitIndex;
                CheckCanBuff();
            }
        }

        private void OnProjectileInteract(IProjectile projectile)
        {
            if(projectile != null)
            {
                projectile.InitStrategies(null);
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            _characterWeapon.PausedRotate = false;
            _isSlashing = false;
            _specialDamageBox.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            base.Dispose();
            _buffSpeedCancellationTokenSource?.Cancel();
            _specialAttackCooldownCancellationTokenSource?.Cancel();
            ownerWeaponModel.IsSpecialAttackReady = true;
        }

        #endregion Class Methods
    }
}
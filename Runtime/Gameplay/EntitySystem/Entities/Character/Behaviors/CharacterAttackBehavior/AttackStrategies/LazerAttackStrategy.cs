using System.Threading;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Extensions;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class LazerAttackStrategy : AttackStrategy<LazerWeaponModel>
    {
        #region Members

        private const string SMALL_LAZER_EFFECT_NAME = "weapon_110001_lazer_beam_normal";
        private const string BIG_LAZER_EFFECT_NAME = "weapon_110001_lazer_beam_special";
        private const string SMALL_LAZER_IMPACT_EFFECT_NAME = "weapon_110001_lazer_beam_normal_impact_vfx";
        private const string LAZER_ORIGIN_POINT_TRANSFORM_NAME = "spawn_vfx_point";
        private bool _isShootingLazer;
        private Transform _lazerOriginPointTransform;
        private CancellationTokenSource _specialAttackCooldownCancellationTokenSource;

        #endregion Members

        #region Class Methods

        public override void Init(WeaponModel weaponModel, CharacterModel creatorModel, Transform creatorTransform)
        {
            base.Init(weaponModel, creatorModel, creatorTransform);
            _isShootingLazer = false;
            ownerWeaponModel.IsSpecialAttackReady = true;
            _lazerOriginPointTransform = creatorTransform.FindChildTransform(LAZER_ORIGIN_POINT_TRANSFORM_NAME);
#if DEBUGGING
            if (_lazerOriginPointTransform == null)
                Debug.LogError("No lazer origin point transform is found, the character's position will be the lazer origin point instead!");
#endif
        }

        public override void Cancel()
        {
            _specialAttackCooldownCancellationTokenSource?.Cancel();
            ownerWeaponModel.IsSpecialAttackReady = true;
            base.Cancel();
        }

        public override bool CheckCanAttack()
            => base.CheckCanAttack() && !_isShootingLazer;

        public override bool CheckCanSpecialAttack()
            => ownerWeaponModel.IsSpecialAttackReady && !_isShootingLazer;

        public override async UniTask OperateSpecialAttack()
        {
            ownerWeaponModel.IsSpecialAttackReady = false;
            await base.OperateSpecialAttack();
            RunSpecialAttackCooldownAsync().Forget();
        }

        protected override async UniTask TriggerAttack(CancellationToken cancellationToken)
        {
            _isShootingLazer = true;
            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.NormalAttack,
                operatedPointTriggeredCallbackAction: () => {
                    Vector2 smallLazerOriginPosition = _lazerOriginPointTransform != null ? _lazerOriginPointTransform.position : creatorModel.Position;
                    SpawnSmallLazerAsync(smallLazerOriginPosition, cancellationToken).Forget();
                },
                endActionCallbackAction: () => {
                    _isShootingLazer = false;
                }
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            await UniTask.WaitUntil(() => !_isShootingLazer, cancellationToken: cancellationToken);
        }

        protected override async UniTask TriggerSpecialAttack(CancellationToken cancellationToken)
        {
            _isShootingLazer = true;
            var characterPlayedWeaponAction = new CharacterPlayedWeaponAction
            (
                animationType: CharacterWeaponAnimationType.SpecialAttack,
                operatedPointTriggeredCallbackAction: () => {
                    Vector2 bigLazerOriginPosition = _lazerOriginPointTransform != null ? _lazerOriginPointTransform.position : creatorModel.Position;
                    SpawnBigLazerAsync(bigLazerOriginPosition, cancellationToken).Forget();
                },
                endActionCallbackAction: () => {
                    _isShootingLazer = false;
                }
            );
            characterWeaponActionPlayer.Play(characterPlayedWeaponAction);
            await UniTask.WaitUntil(() => !_isShootingLazer, cancellationToken: cancellationToken);
        }

        private async UniTaskVoid SpawnSmallLazerAsync(Vector2 smallLazerOriginPosition, CancellationToken cancellationToken)
        {
            var correctLazerLength = GetCorrectLazerLength(smallLazerOriginPosition, creatorModel.FaceDirection, ownerWeaponModel.AttackRange);
            var smallLazerEffect = await PoolManager.Instance.Get(SMALL_LAZER_EFFECT_NAME, cancellationToken);
            smallLazerEffect.transform.position = smallLazerOriginPosition;
            var lazerBeam = smallLazerEffect.GetComponent<ScalableVFX>();
            if (lazerBeam)
                lazerBeam.Scale(correctLazerLength, ownerWeaponModel.SmallLazerAffectWidth);
            smallLazerEffect.transform.rotation = creatorModel.FaceDirection.ToQuaternion();

            var hitBoxCenterPoint = smallLazerOriginPosition + creatorModel.FaceDirection * correctLazerLength / 2;
            var hitBoxSize = new Vector3(ownerWeaponModel.SmallLazerAffectWidth, correctLazerLength);
            var hitBoxAngle = Vector2.SignedAngle(Vector2.up, creatorModel.FaceDirection);
            var colliders = Physics2D.OverlapBoxAll(hitBoxCenterPoint, hitBoxSize, hitBoxAngle);

            var damageFactors = ownerWeaponModel.DamageFactors;
            if (ownerWeaponModel.CanAddMoreDamageAfterHitSomeEnemies)
            {
                int hitEnemiesCount = 0;
                foreach (var collider in colliders)
                {
                    var entity = collider.GetComponent<IEntity>();
                    if (entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>();
                        if (interactable != null && !interactable.Model.IsDead && interactable.Model != creatorModel && interactable.Model.EntityType.IsEnemy())
                            hitEnemiesCount++;
                    }
                }

                if (hitEnemiesCount >= ownerWeaponModel.AddBonusDamageForEnemiesCount)
                    damageFactors = damageFactors.Multiply(1 + ownerWeaponModel.HitEnemiesAddedDamagePercent);
            }

            foreach (var collider in colliders)
            {
                var entity = collider.GetComponent<IEntity>();
                if (entity != null)
                {
                    var interactable = entity.GetBehavior<IInteractable>();
                    if (interactable != null && !interactable.Model.IsDead && interactable.Model != creatorModel)
                    {
                        if (creatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                        {
                            var damageDirection = (interactable.Model.Position - smallLazerOriginPosition).normalized;
                            var damageAttractedPoint = GetAttractedPointOnLazer(smallLazerOriginPosition, creatorModel.FaceDirection, interactable.Model.Position);

                            var hitPoint = collider.ClosestPoint(interactable.Model.Position);
                            SpawnImpactAsync(hitPoint, cancellationToken).Forget();
                            var damageInfo = creatorModel.GetDamageInfo(DamageSource.FromNormalAttack, ownerWeaponModel.DamageBonus, damageFactors, ownerWeaponModel.DamageModifierModels, interactable.Model);
                            interactable.GetHit(damageInfo, new DamageMetaData(damageDirection, damageAttractedPoint));

                        }
                    }
                }
            }
        }

        private async UniTaskVoid SpawnBigLazerAsync(Vector2 bigLazerOriginPosition, CancellationToken cancellationToken)
        {
            var bigLazerEffect = await PoolManager.Instance.Get(BIG_LAZER_EFFECT_NAME, cancellationToken);
            bigLazerEffect.transform.position = bigLazerOriginPosition;

            var lazerBeam = bigLazerEffect.GetComponent<ScalableVFX>();
            if (lazerBeam)
                lazerBeam.Scale(ownerWeaponModel.BigLazerRange, ownerWeaponModel.BigLazerAffectWidth);
            bigLazerEffect.transform.rotation = creatorModel.FaceDirection.ToQuaternion();

            var hitBoxCenterPoint = bigLazerOriginPosition + creatorModel.FaceDirection * ownerWeaponModel.BigLazerRange / 2;
            var hitBoxSize = new Vector3(ownerWeaponModel.BigLazerAffectWidth, ownerWeaponModel.BigLazerRange);
            var hitBoxAngle = Vector2.SignedAngle(Vector2.up, creatorModel.FaceDirection);
            var colliders = Physics2D.OverlapBoxAll(hitBoxCenterPoint, hitBoxSize, hitBoxAngle);
            var damageFactors = ownerWeaponModel.BigLazerDamageFactors;

            if (ownerWeaponModel.CanAddMoreDamageAfterHitSomeEnemies)
            {
                int hitEnemiesCount = 0;
                foreach (var collider in colliders)
                {
                    var entity = collider.GetComponent<IEntity>();
                    if (entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>();
                        if (interactable != null && !interactable.Model.IsDead && interactable.Model != creatorModel && interactable.Model.EntityType.IsEnemy())
                            hitEnemiesCount++;
                    }
                }

                if (hitEnemiesCount >= ownerWeaponModel.AddBonusDamageForEnemiesCount)
                    damageFactors = damageFactors.Multiply(1 + ownerWeaponModel.HitEnemiesAddedDamagePercent);
            }

            foreach (var collider in colliders)
            {
                var entity = collider.GetComponent<IEntity>();
                if (entity != null)
                {
                    var interactable = entity.GetBehavior<IInteractable>();
                    if (interactable != null && !interactable.Model.IsDead && interactable.Model != creatorModel)
                    {
                        if (creatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                        {
                            var damageDirection = (interactable.Model.Position - hitBoxCenterPoint).normalized;
                            var damageAttractedPoint = GetAttractedPointOnLazer(bigLazerOriginPosition, creatorModel.FaceDirection, interactable.Model.Position);
                            foreach (var biglazerStatusModel in ownerWeaponModel.BigLazerModifierModels)
                                biglazerStatusModel.SetFinishedEvent(() => ApplyLockStatusEffect(interactable));
                            var damageInfo = creatorModel.GetDamageInfo(DamageSource.FromSpecialAttack, ownerWeaponModel.DamageBonus, damageFactors, ownerWeaponModel.BigLazerModifierModels, interactable.Model);
                            interactable.GetHit(damageInfo, new DamageMetaData(damageDirection, damageAttractedPoint));
                        }
                    }
                }
            }
        }

        private void ApplyLockStatusEffect(IInteractable interactable)
        {
            if (interactable != null && !interactable.Model.IsDead && ownerWeaponModel.CanLockEneniesInPlace)
            {
                var damageInfo = new DamageInfo(DamageSource.FromSpecialAttack, 0, ownerWeaponModel.LockEnemiesModifierModels, creatorModel, interactable.Model);
                interactable.GetHit(damageInfo, default);
            }
        }

        private Vector2 GetAttractedPointOnLazer(Vector2 lazerOrigin, Vector2 lazerDirection, Vector2 targetPosition)
        {
            var lazerToTargetDirection = targetPosition - lazerOrigin;
            float dotValue = Vector2.Dot(lazerToTargetDirection, lazerDirection);
            return lazerOrigin + lazerDirection * dotValue;
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

        private float GetCorrectLazerLength(Vector2 lazerOrigin, Vector2 lazerDirection, float lazerLength)
        {
            var correctLazerLength = lazerLength;
            var raycastHits = Physics2D.RaycastAll(lazerOrigin, lazerDirection, lazerLength, Layers.OBJECT_OBSTACLE_LAYER_MASK);
            foreach (var raycastHit in raycastHits)
            {
                if (!raycastHit.collider.CompareTag(TagNames.OBSTACLE_NOT_BLOCK))
                {
                    var distanceToHitPoint = Vector2.Distance(raycastHit.point, lazerOrigin);
                    if (correctLazerLength > distanceToHitPoint)
                        correctLazerLength = distanceToHitPoint;
                }
            }
            return correctLazerLength;
        }

        private async UniTaskVoid SpawnImpactAsync(Vector2 spawnPoint, CancellationToken token)
        {
            var impact = await PoolManager.Instance.Get(SMALL_LAZER_IMPACT_EFFECT_NAME, cancellationToken: token);
            impact.transform.position = spawnPoint;
        }

        #endregion Class Methods
    }
}
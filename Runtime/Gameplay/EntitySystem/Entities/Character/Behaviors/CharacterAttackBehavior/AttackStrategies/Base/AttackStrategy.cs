using System;
using System.Threading;
using UnityEngine;
using Runtime.Definition;
using Runtime.Audio;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class AttackStrategy<T> : IAttackStrategy where T : WeaponModel
    {
        #region Members

        protected T ownerWeaponModel;
        protected CharacterModel creatorModel;
        protected float attackSpeed;
        protected bool isAttackReady;
        protected float attackCooldownTime;
        protected float currentAttackCooldownTime;
        protected ICharacterWeaponActionPlayer characterWeaponActionPlayer;
        protected CancellationTokenSource attackCooldownCancellationTokenSource;
        protected CancellationTokenSource executeAttackCancellationTokenSource;
        protected CancellationTokenSource executeSpecialAttackCancellationTokenSource;

        #endregion Members

        #region Class Methods

        public virtual void Init(WeaponModel weaponModel, CharacterModel creatorModel, Transform creatorTransform)
        {
            var tryGetAttackSpeed = 0.0f;
            if (creatorModel.TryGetStat(StatType.AttackSpeed, out tryGetAttackSpeed))
            {
                attackSpeed = tryGetAttackSpeed;
                creatorModel.StatChangedEvent += OnStatChanged;
            }
#if DEBUGGING
            else
            {
                Debug.LogError($"Require {StatType.AttackSpeed} for this behavior to work!");
                return;
            }
#endif
            characterWeaponActionPlayer = creatorTransform.GetComponentInChildren<ICharacterWeaponActionPlayer>(true);
            if (characterWeaponActionPlayer != null)
            {
                characterWeaponActionPlayer.Init();
            }
#if DEBUGGING
            else
            {
                Debug.LogError($"Require a character weapon animation for this behavior!");
                return;
            }
#else
            else return;
#endif
            ownerWeaponModel = weaponModel as T;
            this.creatorModel = creatorModel;
            isAttackReady = true;
            attackCooldownTime = 1 / (weaponModel.AttackSpeedPercent * attackSpeed);
            currentAttackCooldownTime = 0.0f;
        }

        public virtual bool CheckCanAttack() => isAttackReady;
        public virtual bool CheckCanSpecialAttack() => true;

        public virtual async UniTask OperateAttack()
        {
            PlayNormalAttackSound();
            isAttackReady = false;
            attackCooldownCancellationTokenSource?.Cancel();
            executeAttackCancellationTokenSource?.Cancel();
            executeAttackCancellationTokenSource = new CancellationTokenSource();
            await TriggerAttack(executeAttackCancellationTokenSource.Token);
            RunAttackCooldownAsync().Forget();
        }

        public virtual async UniTask OperateSpecialAttack()
        {
            PlaySpecialAttackSound();
            executeSpecialAttackCancellationTokenSource?.Cancel();
            executeSpecialAttackCancellationTokenSource = new CancellationTokenSource();
            await TriggerSpecialAttack(executeSpecialAttackCancellationTokenSource.Token);
        }

        public virtual void Dispose()
            => Cancel();

        public virtual void Cancel()
        {
            executeAttackCancellationTokenSource?.Cancel();
            executeSpecialAttackCancellationTokenSource?.Cancel();
            attackCooldownCancellationTokenSource?.Cancel();
            isAttackReady = true;
            characterWeaponActionPlayer.Cancel();
        }

        protected abstract UniTask TriggerAttack(CancellationToken cancellationToken);
        protected abstract UniTask TriggerSpecialAttack(CancellationToken cancellationToken);

        protected virtual void OnStatChanged(StatType statType, float updatedValue)
        {
            if (statType == StatType.AttackSpeed)
            {
                var newAttackCooldownTime = 1 / (updatedValue * ownerWeaponModel.AttackSpeedPercent);
                attackCooldownTime = newAttackCooldownTime;
                if (newAttackCooldownTime <= currentAttackCooldownTime)
                {
                    attackCooldownCancellationTokenSource?.Cancel();
                    FinishAttackCooldown();
                }
            }
        }

        protected virtual async UniTaskVoid RunAttackCooldownAsync()
        {
            attackCooldownCancellationTokenSource = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(attackCooldownTime), cancellationToken: attackCooldownCancellationTokenSource.Token);
            FinishAttackCooldown();
        }

        protected virtual void FinishAttackCooldown()
        {
            isAttackReady = true;
            currentAttackCooldownTime = 0.0f;
        }

        protected virtual void PlayNormalAttackSound()
        {
            switch (ownerWeaponModel.WeaponType)
            {
                case WeaponType.BidetGun:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_BIDET_GUN_ATTACK);
                    break;

                case WeaponType.Lazer:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_LAZER_ATTACK);
                    break;

                case WeaponType.MagicMace:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_MAGIC_MACE_ATTACK);
                    break;

                case WeaponType.SingleHitGun:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_CHICKEN_ATTACK);
                    break;

                case WeaponType.MagicBall:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_MAGIC_BALL_ATTACK);
                    break;

                case WeaponType.Shovel:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_SHOVEL_ATTACK);
                    break;
            }
        }

        protected virtual void PlaySpecialAttackSound()
        {
            switch (ownerWeaponModel.WeaponType)
            {
                case WeaponType.BidetGun:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_BIDET_GUN_SPECIAL_ATTACK);
                    break;

                case WeaponType.Lazer:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_LAZER_SPECIAL_ATTACK);
                    break;

                case WeaponType.MagicMace:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_MAGIC_MACE_SPECIAL_ATTACK);
                    break;

                case WeaponType.SingleHitGun:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_CHICKEN_SPECIAL_ATTACK);
                    break;

                case WeaponType.MagicBall:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_MAGIC_BALL_SPECIAL_ATTACK);
                    break;

                case WeaponType.Shovel:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.HERO_SHOVEL_SPECIAL_ATTACK);
                    break;
            }
        }

        #endregion Class Methods
    }
}
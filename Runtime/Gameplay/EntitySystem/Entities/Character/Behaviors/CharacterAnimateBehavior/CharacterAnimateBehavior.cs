using System;
using System.Threading;
using UnityEngine;
using Runtime.Extensions;
using Runtime.Definition;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class CharacterAnimateBehavior : CharacterBehavior, IDisable
    {
        #region Members

        private const string GRAPHICS = "graphics";
        private static readonly Color s_appearanceHitEffectColor = new Color(1.0f, 1.0f, 1.0f, 0.85f);
        private static readonly Color s_appearanceNormalColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        private static readonly float s_showHitEffectColorDuration = 1/16f;
        private static readonly float s_showHitEffectColorDurationWhenDie = 1/7f;
        private GameObject _graphics;
        private bool _isShowingGetHurt;
        private ICharacterAnimationPlayer _characterAnimationPlayer;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

#if UNITY_EDITOR
        public override void Validate(Transform ownerTransform)
        {
            _graphics = ownerTransform.FindChildGameObject(GRAPHICS);
            if (_graphics == null)
            {
                Debug.LogError("Graphics name is not mapped!");
                return;
            }
        }
#endif

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            _characterAnimationPlayer = transform.GetComponentInChildren<ICharacterAnimationPlayer>(true);
#if DEBUGGING
            if (_characterAnimationPlayer == null)
            {
                Debug.LogError($"Require a character animation for this behavior!");
                return false;
            }
#endif

            _characterAnimationPlayer.Init(ownerModel);
            _graphics = transform.FindChildGameObject(GRAPHICS);
            _graphics.transform.localScale = new Vector2(1, 1);
            _isShowingGetHurt = false;
            ownerModel.MovementChangedEvent += OnMovementChanged;
            ownerModel.HealthChangedEvent += OnHealthChanged;
            ownerModel.ShieldChangedEvent += OnShieldChanged;
            ownerModel.DirectionChangedEvent += OnDirectionChanged;
            ownerModel.DeathEvent += OnDeath;
            ownerModel.ReactionChangedEvent += OnReactionChange;
            ownerModel.ReactionChangedEvent += OnReactionChange;
            ownerModel.HardCCImpactedEvent += OnHardCCImpacted;
            ownerModel.HardCCStoppedEvent += OnHardCCStopped;
            SetAnimation(CharacterAnimationState.Idle);
            _characterAnimationPlayer.TintColor(s_appearanceNormalColor);

            return true;
        }

        private void OnMovementChanged()
        {
            if (ownerModel.IsMoving)
                SetAnimation(CharacterAnimationState.Run);
            else
                SetAnimation(CharacterAnimationState.Idle);
        }

        private void OnReactionChange(CharacterReactionType characterReactionType)
        {
            if (characterReactionType == CharacterReactionType.JustFinishedUseSkill)
            {
                if (ownerModel.IsMoving)
                    SetAnimation(CharacterAnimationState.Run);
                else
                    SetAnimation(CharacterAnimationState.Idle);
            }
        }

        private void OnShieldChanged(float deltaShield, DamageProperty damageProperty)
        {
            if (deltaShield < 0)
            {
                if (!_isShowingGetHurt)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    var showHitEffectColorTimes = ownerModel.EntityType.ShowHitEffectColorTimes();
                    ShowHitEffectAsync(showHitEffectColorTimes, s_showHitEffectColorDuration, _cancellationTokenSource.Token).Forget();
                }
            }
        }

        private void OnHealthChanged(float deltaHp, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (deltaHp < 0)
            {
                if (!_isShowingGetHurt)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    var showHitEffectColorTimes = ownerModel.EntityType.ShowHitEffectColorTimes();
                    ShowHitEffectAsync(showHitEffectColorTimes, s_showHitEffectColorDuration, _cancellationTokenSource.Token).Forget();
                }
            }
        }

        private void OnDirectionChanged()
        {
            if (ownerModel.FaceRight)
                _graphics.transform.localScale = new Vector2(-1, 1);
            else
                _graphics.transform.localScale = new Vector2(1, 1);
        }

        private void OnDeath(DamageSource damageSource)
        {
            SetAnimation(CharacterAnimationState.Die);
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            ShowHitEffectAsync(Constants.GetDeathBlinkVFXTimes(ownerModel.EntityType), s_showHitEffectColorDurationWhenDie, _cancellationTokenSource.Token).Forget();
        }

        private void OnHardCCImpacted(StatusEffectType statusEffectType)
        {
            if (statusEffectType == StatusEffectType.Freeze)
                _characterAnimationPlayer.Pause();
        }

        private void OnHardCCStopped(StatusEffectType statusEffectType)
        {
            if (statusEffectType == StatusEffectType.Freeze && !ownerModel.IsInAnimationLockedStatus)
                _characterAnimationPlayer.Continue();
        }

        private void SetAnimation(CharacterAnimationState state)
            => _characterAnimationPlayer.Play(state);

        private async UniTask ShowHitEffectAsync(int showHitEffectColorTimes, float showHitEffectColorDuration, CancellationToken cancellationToken)
        {
            _isShowingGetHurt = true;
            int currentShowHitEffectColorTimes = 0;
            while (currentShowHitEffectColorTimes < showHitEffectColorTimes)
            {
                currentShowHitEffectColorTimes++;
                _characterAnimationPlayer.TintColor(s_appearanceHitEffectColor);
                await UniTask.Delay(TimeSpan.FromSeconds(showHitEffectColorDuration), cancellationToken: cancellationToken);
                _characterAnimationPlayer.TintColor(s_appearanceNormalColor);
                await UniTask.Delay(TimeSpan.FromSeconds(showHitEffectColorDuration), cancellationToken: cancellationToken);
            }
            _characterAnimationPlayer.TintColor(s_appearanceNormalColor);
            _isShowingGetHurt = false;
        }

        public void Disable()
            => _cancellationTokenSource?.Cancel();

        #endregion Class Methods
    }
}
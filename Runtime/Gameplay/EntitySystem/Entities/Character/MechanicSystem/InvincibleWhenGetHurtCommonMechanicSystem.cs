using System;

using System.Threading;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvincibleWhenGetHurtCommonMechanicSystem : IMechanicSystem, IUpdateHealthModifier
    {
        #region Members

        public const float CHARACTER_DISABLE_COLLIDE_WHEN_GET_HURT = 0.5f;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isInvincible;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => -100;

        #endregion Properties

        #region Class Methods

        public void Init(HeroModel heroModel)
        {
            _isInvincible = false;
            heroModel.AddUpdateHealthModifier(this);
            heroModel.HealthChangedEvent += OnHealthChanged;
        }

        public void Reset(HeroModel heroModel)
        {
            _isInvincible = false;
            heroModel.AddUpdateHealthModifier(this);
            heroModel.HealthChangedEvent += OnHealthChanged;
        }

        public void Disable()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void OnHealthChanged(float deltaHp, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (deltaHp < 0)
            {
                if(!_isInvincible)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    StartInvincibleAsync(_cancellationTokenSource.Token).Forget();
                }
            }
        }

        private async UniTaskVoid StartInvincibleAsync(CancellationToken token)
        {
            _isInvincible = true;
            await UniTask.Delay(TimeSpan.FromSeconds(CHARACTER_DISABLE_COLLIDE_WHEN_GET_HURT), cancellationToken: token);
            _isInvincible = false;
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty) => (value, damageProperty);

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel creatorModel)
        {
            if (_isInvincible)
                return 0;
            return value;
        }

        #endregion Class Methods
    }
}
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class AstralBootsEquipmentSystem : EquipmentSystem<AstralBootsEquipmentSystemModel>, IDamageCreatedModifier
    {
        #region Members

        private bool _isBuffed;
        private float _remainBuffedTime;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddDamageCreatedModifier(this);
            _remainBuffedTime = 0;
            _isBuffed = false;

            _cancellationTokenSource = new CancellationTokenSource();
            StartCountTimeAsync().Forget();
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddDamageCreatedModifier(this);
            _remainBuffedTime = 0;
            _isBuffed = false;
        }

        public override void Disable()
        {
            base.Disable();
            _cancellationTokenSource?.Cancel();
        }

        public async UniTaskVoid StartCountTimeAsync()
        {
            while (true)
            {
                await UniTask.Yield(cancellationToken: _cancellationTokenSource.Token);
                if (_remainBuffedTime <= 0 && _isBuffed)
                {
                    _isBuffed = false;
                    creatorModel.DebuffStat(StatType.MoveSpeed, ownerModel.SpeedIncrease, StatModifyType.BaseBonus);
                    if (ownerModel.CanBuffDamage)
                        creatorModel.DebuffStat(StatType.AttackDamage, ownerModel.DamagePercentIncrease, StatModifyType.BaseMultiply);
                }
                else
                {
                    _remainBuffedTime -= Time.deltaTime;
                }
            }
        }

        public float CreateDamage(float damage, EntityModel receiver)
        {
            if (damage > 0 && receiver.EntityType.IsBoss() && ownerModel.CanBuffSpeed)
            {
                if (!_isBuffed)
                {
                    _isBuffed = true;
                    creatorModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustBuffSpeed);
                    creatorModel.BuffStat(StatType.MoveSpeed, ownerModel.SpeedIncrease, StatModifyType.BaseBonus);
                    if (ownerModel.CanBuffDamage)
                        creatorModel.BuffStat(StatType.AttackDamage, ownerModel.DamagePercentIncrease, StatModifyType.BaseMultiply);
                }
                _remainBuffedTime = ownerModel.TimeSpeedIncrease;
            }

            return damage;
        }

        #endregion Class Methods
    }
}
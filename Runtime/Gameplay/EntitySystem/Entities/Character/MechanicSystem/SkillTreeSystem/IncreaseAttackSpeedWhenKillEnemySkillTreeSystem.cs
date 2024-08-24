using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Random = UnityEngine.Random;

namespace Runtime.Gameplay.EntitySystem
{
    public class IncreaseAttackSpeedWhenKillEnemySkillTreeSystem : SkillTreeSystem<IncreaseAttackSpeedWhenKillEnemySkillTreeSystemModel>, IDamageCreatedModifier
    {
        #region Members

        private bool _isBuffed;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddDamageCreatedModifier(this);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddDamageCreatedModifier(this);
        }

        public float CreateDamage(float damage, EntityModel receiver)
        {
            if (receiver.IsDead && receiver.EntityType.IsEnemy())
            {
                var rate = Random.Range(0, 1f);
                if(rate < ownerModel.RateBuff)
                {
                    if (!_isBuffed)
                    {
                        creatorModel.BuffStat(StatType.AttackSpeed, ownerModel.IncreaseAttackSpeedPercent, StatModifyType.BaseMultiply);
                    }

                    _isBuffed = true;
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    StartCountTimeAsync().Forget();
                }
            }

            return damage;
        }

        private async UniTaskVoid StartCountTimeAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.TimeBuff), cancellationToken: _cancellationTokenSource.Token);
            _isBuffed = false;
            creatorModel.DebuffStat(StatType.AttackSpeed, ownerModel.IncreaseAttackSpeedPercent, StatModifyType.BaseMultiply);
        }

        public override void Disable()
        {
            base.Disable();
            _cancellationTokenSource?.Cancel();
        }

        #endregion Class Methods
    }

}

using UnityEngine;
using Runtime.Definition;
using Runtime.Extensions;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior updates the data input for the character.<br/>
    /// Since one character's skill defines the data input, such as movement input, skill trigger input,...
    /// the auto input strategy will be based on the skill auto input type.<br/>
    /// If the character doesn't have any skill, then a default auto input strategy will be applied instead.<br/>
    /// This behavior is used for characters those are not controlled by the player, but AI.
    /// </summary>
    public sealed class CharacterAutoInputBehavior : CharacterBehavior, IUpdatable, IDisable
    {
        #region Members

        private IAutoInputStrategy _autoInputStrategy;

        #endregion Members

        #region Class Methods

        public void Update()
        {
            if (ownerModel.IsControllable)
                _autoInputStrategy.Update();
        }

        public void Disable()
            => _autoInputStrategy.Disable();

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            InitAutoInputStrategy();
            return true;
        }

        private void InitAutoInputStrategy()
        {
            if (ownerModel.SkillModels.Count > 0)
            {
                var firstSkillModel = ownerModel.SkillModels[0];
                ownerModel.SkillUsageChangedEvent += OnSkillUsageChanged;
                UpdateAutoInputStrategyBySkill(firstSkillModel);
            }
            else _autoInputStrategy = new AutoInputDefaultStrategy(ownerModel);
        }

        private void OnSkillUsageChanged(int skillIndex)
        {
            var changedSkillModel = ownerModel.SkillModels[skillIndex];
            UpdateAutoInputStrategyBySkill(changedSkillModel);
        }

        private void UpdateAutoInputStrategyBySkill(SkillModel skillModel)
        {
            switch (skillModel.AutoInputStrategyType)
            {
                case AutoInputStrategyType.TowardsHero:
                    _autoInputStrategy = new AutoInputTowardsHeroStrategy(ownerModel, skillModel.CastRange);
                    break;

                case AutoInputStrategyType.TowardsAlly:
                    _autoInputStrategy = new AutoInputTowardsAllyStrategy(ownerModel, skillModel.CastRange);
                    break;

                case AutoInputStrategyType.KeepDistanceHero:
                    _autoInputStrategy = new AutoInputKeepDistanceHeroStrategy(ownerModel, skillModel.CastRange);
                    break;

                case AutoInputStrategyType.Idle:
                    _autoInputStrategy = new AutoInputIdleStrategy(ownerModel);
                    break;

                default:
                    _autoInputStrategy = new AutoInputDoNothingStrategy();
                    break;
            }
        }

        #endregion Class Methods
    }
}
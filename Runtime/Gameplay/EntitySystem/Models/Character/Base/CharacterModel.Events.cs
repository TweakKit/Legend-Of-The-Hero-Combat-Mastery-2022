using System;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public enum CharacterActionType
    {
        Idle = -1,
        FirstWeaponAttack = 0,
        SecondWeaponAttack = 1,
        ThirdWeaponAttack = 2,
        PrecastFirstSkill = 3,
        PrecastSecondSkill = 4,
        PrecastThirdSkill = 5,
        UseFirstSkill = 6,
        UseSecondSkill = 7,
        UseThirdSkill = 8,
        BackSwingFirstSkill = 9,
        BackSwingSecondSkill = 10,
        BackSwingThirdSkill = 11
    }

    public enum CharacterReactionType
    {
        JustNormalAttack,
        JustSpecialAttack,
        JustGetHit,
        JustFinishedUseSkill,

        JustBuffSpeed,
        JustBuffBigSpeed,
        JustDodge,
    }

    public abstract partial class CharacterModel : EntityModel
    {
        #region Properties

        public Action<CharacterReactionType> ReactionChangedEvent { get; set; }
        public Action<bool> TargetedEvent { get; set; }
        public Action MovementChangedEvent { get; set; }
        public Action MovePositionUpdatedEvent { get; set; }
        public Action DirectionChangedEvent { get; set; }
        public Action<bool> CollideToggledEvent { get; set; }
        public Action<ActionInputType> ActionTriggeredEvent { get; set; }
        public Action<float, DamageProperty, DamageSource> HealthChangedEvent { get; set; }
        public Action<float, DamageProperty> ShieldChangedEvent { get; set; }
        public Action<StatType, float> StatChangedEvent { get; set; }
        public Action<DamageSource> DeathEvent { get; set; }
        public Action<StatusEffectType> HardCCImpactedEvent { get; set; }
        public Action<StatusEffectType> HardCCStoppedEvent { get; set; }
        public Action<int> SkillUsageChangedEvent { get; set; }

        #endregion Properties

        #region Class Methods

        protected virtual partial void InitEvents()
        {
            ReactionChangedEvent = _ => { };
            TargetedEvent = _ => { };
            MovementChangedEvent = () => { };
            MovePositionUpdatedEvent = () => { };
            CollideToggledEvent = _ => { };
            DirectionChangedEvent = () => { };
            ActionTriggeredEvent = _ => { };
            HealthChangedEvent = (_, _, _) => { };
            ShieldChangedEvent = (_, _) => { };
            StatChangedEvent = (_, _) => { };
            DeathEvent = _ => { };
            HardCCImpactedEvent = _ => { };
            HardCCStoppedEvent = _ => { };
            SkillUsageChangedEvent = _ => { };
        }

        public virtual void TriggerSkill()
            => ActionTriggeredEvent.Invoke(ActionInputType.PrimarySkill);

        #endregion Class Methods
    }
}
namespace Runtime.Gameplay.EntitySystem
{
    public class TrapCauseLessDamageForHeroSkillTreeSystem : SkillTreeSystem<TrapCauseLessDamageForHeroSkillTreeSystemModel>, IUpdateHealthModifier
    {
        #region Properties

        public int UpdateHealthPriority => -1;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddUpdateHealthModifier(this);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddUpdateHealthModifier(this);
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty) => (value, damageProperty);

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel)
        {
            if (damageSource == DamageSource.FromTrap)
                value = value * (1 - ownerModel.DecreaseDamagePercent);
            return value;
        }

        #endregion Class Methods
    }
}
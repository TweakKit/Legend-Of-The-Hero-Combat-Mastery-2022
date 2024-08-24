namespace Runtime.Gameplay.EntitySystem
{
    public class DropHpIncreaseValueSkillTreeSystem : SkillTreeSystem<DropHpIncreaseValueSkillTreeSystemModel>, IUpdateHealthModifier
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

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty)
        {
            if (damageSource == DamageSource.FromDroppable)
            {
                value = value * (1 + ownerModel.IncreaseBuffPercent);
            }

            return (value, damageProperty);
        }

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel creatorModel) => value;

        #endregion Class Methods
    }

}
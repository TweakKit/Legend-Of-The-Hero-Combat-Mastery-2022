namespace Runtime.Gameplay.EntitySystem
{
    public class InvincibleWithFirstBossAttackSkillTreeSystem : SkillTreeSystem<InvincibleWithFirstBossAttacksSkillTreeSystemModel>, IUpdateHealthModifier
    {
        #region Members

        private int _currentAttacks;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => -1;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _currentAttacks = 0;
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
            if (value > 0 && damageCreatorModel.EntityType.IsBoss())
            {
                if(damageSource != DamageSource.FromCollide)
                    _currentAttacks += 1;
                else
                    return value;

                if (_currentAttacks <= ownerModel.NumberOfAttacks)
                    value = 0;
            }

            return value;
        }

        #endregion Class Methods
    }

}
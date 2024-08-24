namespace Runtime.Gameplay.EntitySystem
{
    public class AstralLocketEquipmentSystem : EquipmentSystem<AstralLocketEquipmentSystemModel>, IDamageModifier
    {
        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddDamageModifier(this);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddDamageModifier(this);
        }

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource) => damageInfo;

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier)
        {
            if (ownerModel.CanApplyCritChanceForAllDamage)
            {
                prepareDamageModifier.critChance += ownerModel.AppliedCritChance;
                if (targetModel.EntityType.IsCharacter())
                {
                    var characterModel = targetModel as CharacterModel;
                    if (ownerModel.CanTriggerCritChanceFactor && !characterModel.EntityType.IsBoss() && characterModel.CurrentHp / characterModel.MaxHp <= ownerModel.EnemyHealthPercentTriggered)
                        prepareDamageModifier.critChance *= ownerModel.TriggeredCritChanceFactor;
                }
            }
            return prepareDamageModifier;
        }

        #endregion Class Methods
    }
}
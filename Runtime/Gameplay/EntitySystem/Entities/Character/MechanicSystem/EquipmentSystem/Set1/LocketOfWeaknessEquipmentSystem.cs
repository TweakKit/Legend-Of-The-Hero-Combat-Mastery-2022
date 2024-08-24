using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class LocketOfWeaknessEquipmentSystem : EquipmentSystem<LocketOfWeaknessEquipmentSystemModel>, IDamageModifier
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

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource)
        {
            if (ownerModel.ApplyForControlMoveStatus)
            {
                if (damageInfo.targetModel is CharacterModel)
                {
                    if (((CharacterModel)damageInfo.targetModel).CheckContainStatusEffectInStack(Constants.GetControlMoveStatusEffect()))
                        damageInfo.damage *= (1 + ownerModel.DamageIncreasePercent);
                }
            }
            else
            {
                if (damageInfo.targetModel is CharacterModel)
                {
                    if(ownerModel.EffectStatusTypes != null)
                    {
                        if (((CharacterModel)damageInfo.targetModel).CheckContainStatusEffectInStack(ownerModel.EffectStatusTypes))
                            damageInfo.damage *= (1 + ownerModel.DamageIncreasePercent);
                    }
                }
            }
            return damageInfo;
        }

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier)
            => prepareDamageModifier;

        #endregion Class Methods
    }
}
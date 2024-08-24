namespace Runtime.Gameplay.EntitySystem
{
    public class PrepareDamageModifier
    {
        public float damageBonus;
        public float critChance;
        public DamageFactor[] damageFactors;
        public StatusEffectModel[] damageModifierModels;

        public PrepareDamageModifier(float damageBonus, DamageFactor[] damageFactors, float critChance, StatusEffectModel[] damageModifierModels)
        {
            this.damageBonus = damageBonus;
            this.damageFactors = damageFactors;
            this.critChance = critChance;
            this.damageModifierModels = damageModifierModels;
        }
    }
}
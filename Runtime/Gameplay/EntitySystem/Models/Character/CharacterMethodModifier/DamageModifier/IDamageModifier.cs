namespace Runtime.Gameplay.EntitySystem
{
    public interface IDamageModifier
    {
        #region Interface Methods

        PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier);
        DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource);

        #endregion Interface Methods
    }
}
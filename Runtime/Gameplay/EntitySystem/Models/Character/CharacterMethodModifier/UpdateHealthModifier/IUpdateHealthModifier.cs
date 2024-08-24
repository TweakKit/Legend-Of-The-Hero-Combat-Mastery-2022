namespace Runtime.Gameplay.EntitySystem
{
    public interface IUpdateHealthModifier
    {
        #region Members

        /// <summary>
        /// Used for sorting modifier, some modifier should at the end or should at the start. lowest will start first, highest will start at end.
        /// </summary>
        int UpdateHealthPriority { get; }

        #endregion Members

        #region Interface Methods

        (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty);

        float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel creatorModel);

        #endregion Interface Methods
    }
}
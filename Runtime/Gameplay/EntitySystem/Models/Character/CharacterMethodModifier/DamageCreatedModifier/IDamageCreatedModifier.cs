namespace Runtime.Gameplay.EntitySystem
{
    public interface IDamageCreatedModifier
    {
        #region Interface Methods

        public float CreateDamage(float damage, EntityModel receiver);

        #endregion Interface Methods
    }
}
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IInteractable
    {
        #region Properties

        public EntityModel Model { get; }

        #endregion Propreties

        #region Class Methods

        void GetHit(DamageInfo damageData, DamageMetaData damageMetaData);
        void GetAffected(AffectedStatusEffectInfo affectedStatusEffectInfo, StatusEffectMetaData statusEffectMetaData);
        void GetTrapped(TrapType trapType, DamageInfo damageData, DamageMetaData damageMetaData);
        void StopTrapped(TrapType trapType);

        #endregion Class Methods
    }
}
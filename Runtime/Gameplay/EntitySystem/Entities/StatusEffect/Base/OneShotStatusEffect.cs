namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// One shot status effect affects the target until they die, meaning there is no duration.
    /// </summary>
    public abstract class OneShotStatusEffect<T> : StatusEffect<T>, IStatusEffect where T : StatusEffectModel
    {
        #region Members

        protected CharacterModel affectedModel;

        #endregion Members

        #region Class Methods

        public override void Init(StatusEffectModel statusEffectModel, EntityModel senderModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
        {
            base.Init(statusEffectModel, senderModel, receiverModel, statusEffectMetaData);
            affectedModel = receiverModel;
            HasFinished = true;
            StartAffect(affectedModel);
        }

        public override void Update() { }
        protected virtual void StartAffect(CharacterModel receiverModel) { }

        #endregion Class methods
    }
}
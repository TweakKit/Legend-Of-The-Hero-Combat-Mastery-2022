namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// Per frame duration status effect also runs tasks in every frame.
    /// </summary>
    public abstract class PerFrameDurationStatusEffect<T> : DurationStatusEffect<T> where T : StatusEffectModel
    {
        #region Class Methods

        public override void Update()
        {
            AffectPerFrame(affectedModel);
            base.Update();
        }

        protected virtual void AffectPerFrame(CharacterModel receiverModel) { }

        #endregion Class methods
    }
}
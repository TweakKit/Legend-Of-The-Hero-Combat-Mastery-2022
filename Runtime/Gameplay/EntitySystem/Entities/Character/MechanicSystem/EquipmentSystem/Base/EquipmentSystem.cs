namespace Runtime.Gameplay.EntitySystem
{
    public abstract class EquipmentSystem<TModel> : IEquipmentMechanicSystem where TModel : EquipmentSystemModel
    {
        #region Members

        protected HeroModel creatorModel;
        protected TModel ownerModel;

        #endregion Members

        #region Class Methods

        public void Init(EquipmentSystemModel equipmentSystemModel, HeroModel heroModel)
        {
            this.ownerModel = equipmentSystemModel as TModel;
            this.creatorModel = heroModel;
            Initialize();
        }

        public virtual void Reset(HeroModel heroModel)
            => this.creatorModel = heroModel;

        protected virtual void Initialize() { }
        public virtual void Disable() { }

        #endregion Class Methods
    }
}
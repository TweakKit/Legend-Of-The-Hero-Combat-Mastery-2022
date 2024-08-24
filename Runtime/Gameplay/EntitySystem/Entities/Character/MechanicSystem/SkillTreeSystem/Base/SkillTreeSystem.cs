namespace Runtime.Gameplay.EntitySystem
{
    public class SkillTreeSystem<T> : ISkillTreeMechanicSystem where T : SkillTreeSystemModel
    {
        #region Members

        protected HeroModel creatorModel;
        protected T ownerModel;

        #endregion Members

        #region Class Methods

        public void Init(SkillTreeSystemModel skillTreeSystemModel, HeroModel heroModel)
        {
            this.ownerModel = skillTreeSystemModel as T;
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
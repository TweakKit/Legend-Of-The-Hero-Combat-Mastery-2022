using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public abstract class Quest<T> : IQuest where T : QuestModel
    {
        #region Members

        protected T ownerModel;

        #endregion Members

        #region Properties

        public bool HasCompleted { get; set; }
        public QuestType QuestType { get; set; }
        public uint QuestId { get; set; }
        public string Info { get; set; }

        #endregion Properties

        #region Class Methods
        public virtual void Init(QuestModel questModel)
        {
            ownerModel = questModel as T;
            QuestType = ownerModel.QuestType;
            QuestId = ownerModel.Id;
            Info = ownerModel.GetLocalizedInfo();
            HasCompleted = false;
        }

        public virtual void Update(float deltaTime) { }
        public virtual void Dispose() { }

        public virtual void Complete()
            => HasCompleted = true;

        #endregion Class Methods
    }
}
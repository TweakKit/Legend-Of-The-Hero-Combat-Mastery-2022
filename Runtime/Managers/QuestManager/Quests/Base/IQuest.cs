using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public interface IQuest
    {
        #region Properties

        bool HasCompleted { get; set; }
        QuestType QuestType { get; set; }
        uint QuestId { get; set; }
        string Info { get; set; }

        #endregion Properties

        #region Interface Methods

        void Init(QuestModel questModel);
        void Update(float deltaTime);
        void Complete();
        void Dispose();

        #endregion Interface Methods
    }
}
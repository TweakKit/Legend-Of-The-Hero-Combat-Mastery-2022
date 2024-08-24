using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class ZombieModel
    {
        #region Class Methods

        public override void GettingTaunt()
            => ActionTriggeredEvent.Invoke(ActionInputType.PrimarySkill);

        #endregion Class Methods
    }
}
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class HeroModel
    {
        #region Class Methods

        public override void GettingTaunt()
            => ActionTriggeredEvent.Invoke(ActionInputType.Attack);

        #endregion Class Methods
    }
}
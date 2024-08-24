using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IEntityDeathData
    {
        #region Properties

        DeathDataIdentity DeathDataIdentity { get; }

        #endregion Properties
    }
}
using Pathfinding;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IAutoInputStrategy
    {
        #region Interface Methods

        void Update();
        void Disable();

        #endregion Interface Methods
    }
}
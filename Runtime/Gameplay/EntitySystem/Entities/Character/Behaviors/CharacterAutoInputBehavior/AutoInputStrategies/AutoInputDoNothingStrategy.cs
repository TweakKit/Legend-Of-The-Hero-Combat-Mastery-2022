using Pathfinding;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This auto input strategy doesn't update any input data.
    /// </summary>
    public sealed class AutoInputDoNothingStrategy : IAutoInputStrategy
    {
        #region Class Methods

        public AutoInputDoNothingStrategy() { }
        public void Update() { }
        public void Disable() { }

        #endregion Class Methods
    }
}
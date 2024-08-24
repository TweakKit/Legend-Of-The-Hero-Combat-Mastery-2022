namespace Runtime.Gameplay.EntitySystem
{
    public interface IUpdatable
    {
        #region Interface Methods

        void Update();

        #endregion Interface Methods
    }

    public interface IGizmozable
    {
        #region Interface Methods

        void OnGizmos();

        #endregion Interface Methods
    }

    public interface IPhysicsUpdatable
    {
        #region Interface Methods

        void Update();

        #endregion Interface Methods
    }
}
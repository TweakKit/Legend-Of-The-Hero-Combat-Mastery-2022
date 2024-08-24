namespace Runtime.FeatureSystem
{
    public interface IUnlockable
    {
        #region Properties

        bool IsUnlocked { get; }
        public string RequiredUnlockDescription { get; }

        #endregion Properties
    }
}
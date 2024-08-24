using ScreenNavigatorActivity = UnityScreenNavigator.Runtime.Core.Activities.Activity;

namespace Runtime.UI
{
    /// <summary>
    /// For activities that need to update data when popped up, pass in the T argument as the data and override
    /// the Init() method to update its stuff.
    /// </summary>
    public abstract class Activity<T> : ScreenNavigatorActivity
    {
        #region Class Methods

        /// <summary>
        /// Init the data for the activity.
        /// </summary>
        /// <param name="activityData"></param>
        public abstract void Init(T activityData);

        #endregion Class Methods
    }

    /// <summary>
    /// For activities that don't need to update data, just inherit only and do nothing else.
    /// </summary>
    public abstract class Activity : ScreenNavigatorActivity { }
}
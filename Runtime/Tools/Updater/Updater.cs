using System;

namespace Runtime.Gameplay.Tools.Updater
{
    /// <summary>
    /// A very simple updater that runs in every frame to execute an action.
    /// Used in cases where a class needs to update different actions depending on different conditions, or simply update something in every frame.
    /// </summary>
    public class Updater
    {
        #region Members

        /// <summary>
        /// The action that is executed every frame.
        /// This action should not be null, so that the null checking can be skipped.
        /// </summary>
        private Action _executeAction;

        /// <summary>
        /// The action that is saved for pausing/returning the execute action.
        /// </summary>
        private Action _lastExecuteAction;

        #endregion Members

        #region Class Methods

        /// <summary>
        /// Initialize an empty Updater with the execute action set to a not null action.
        /// </summary>
        public Updater()
        {
            _executeAction = () => { };
        }

        /// <summary>
        /// Initialize a non-empty Updater.
        /// </summary>
        /// <param name="executeAction">The action that will be executed</param>
        public Updater(Action executeAction)
        {
            if (executeAction == null)
                executeAction = () => { };

            _executeAction = executeAction;
        }

        /// <summary>
        /// Execute an action.
        /// </summary>
        /// <param name="executeAction">The action that will be executed</param>
        /// <returns>Return a reference of this for chainability.</returns>
        public Updater Execute(Action executeAction)
        {
            if (executeAction == null)
                executeAction = () => { };

            _executeAction = executeAction;
            return this;
        }

        /// <summary>
        /// Pause the current execute action.
        /// </summary>
        /// <returns>Return a reference of this for chainability.</returns>
        public Updater Pause()
        {
            _lastExecuteAction = _executeAction;
            _executeAction = () => { };
            return this;
        }

        /// <summary>
        /// Continue runing the current execute action.
        /// </summary>
        /// <returns>Return a reference of this for chainability.</returns>
        public Updater Continue()
        {
            _executeAction = _lastExecuteAction;
            _lastExecuteAction = null;
            return this;
        }

        /// <summary>
        /// Stop executing the execute action.
        /// </summary>
        /// <returns>Return a reference of this for chainability.</returns>
        public Updater Stop()
        {
            _executeAction = () => { };
            return this;
        }

        /// <summary>
        /// Run in every single frame to update the execute action.
        /// </summary>
        public void Update()
        {
            _executeAction();
        }

        #endregion Class Methods
    }
}
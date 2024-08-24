namespace Runtime.Server.CallbackData
{
    public abstract class CallbackData : ICallbackData
    {
        #region Members

        protected int resultCode;

        #endregion Members

        #region Properties

        public int ResultCode => resultCode;

        #endregion Properties

        #region Class Methods

        public CallbackData(int resultCode) => this.resultCode = resultCode;

        #endregion Class Methods
    }
}
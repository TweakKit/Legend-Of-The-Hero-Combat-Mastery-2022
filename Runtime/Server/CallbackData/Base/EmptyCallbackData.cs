namespace Runtime.Server.CallbackData
{
    public class EmptyRequestData : IRequestData { }

    public class EmptyCallbackData : CallbackData
    {
        #region Class Methods

        public EmptyCallbackData(int resultCode) : base(resultCode)
        {
        }

        #endregion Class Methods
    }

}
namespace Runtime.Authentication
{
    public enum AuthenticationResultType
    {
        Successful,
        Failed,
        NotLoggedIn,
        LoggedIn,
        Binded,
        Canceled,
    }

    public struct AuthenticationResult
    {
        #region Members

        public AuthenticationResultType resultType;
        public string userId;

        #endregion Members

        #region Struct Methods

        public AuthenticationResult(AuthenticationResultType resultType, string userId)
        {
            this.resultType = resultType;
            this.userId = userId;
        }

        #endregion Struct Methods
    }
}
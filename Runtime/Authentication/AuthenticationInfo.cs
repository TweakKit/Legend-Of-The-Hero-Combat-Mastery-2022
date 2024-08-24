namespace Runtime.Authentication
{
    public enum AuthMethod
    {
        LoginFacebook,
        LoginGoogle,
        LoginApple,
        StartConnectToServer,
        Manual,
        BindGoogle,
        BindFacebook,
    }

    public struct AuthenticationInfo
    {
        #region Members

        public AuthMethod authMethod;
        public string userId;

        #endregion Members
    }
}
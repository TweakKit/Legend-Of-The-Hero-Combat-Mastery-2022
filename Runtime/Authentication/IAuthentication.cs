using System;

namespace Runtime.Authentication
{
    public interface IAuthentication
    {
        #region Interface Methods

        void Login(Action<AuthenticationResult> resultCallback);
        void Binding(Action<AuthenticationResult> resultCallback);
        void Logout(Action<AuthenticationResult> resultCallback);
        void ClearCacheLoggedIn();

        #endregion Interface Methods
    }
}
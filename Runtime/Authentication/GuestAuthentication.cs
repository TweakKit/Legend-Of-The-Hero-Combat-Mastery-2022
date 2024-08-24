using System;
using UnityEngine;
using Runtime.Server;
using Runtime.Definition;

namespace Runtime.Authentication
{
    public class GuestAuthentication : IAuthentication
    {
        #region Class Methods

        public void Login(Action<AuthenticationResult> resultCallback)
        {
            var localGuestId = PlayerPrefs.GetString(DataKeys.FIREBASE_GUEST_ID);
            if (string.IsNullOrEmpty(localGuestId))
            {
                var auth = AuthenticationManager.Instance.Auth;
                NetworkServer.Instance.IsProcessing = true;
                auth.SignInAnonymouslyAsync().ContinueWith(task => {
                    NetworkServer.Instance.IsProcessing = false;
                    if (task.IsCanceled)
                    {
#if DEBUGGING
                        Debug.LogError("SignInAnonymouslyAsync was canceled.");
#endif
                        return;
                    }
                    if (task.IsFaulted)
                    {
#if DEBUGGING
                        Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
#endif
                        return;
                    }

                    var newUser = task.Result;
#if DEBUGGING
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);
#endif

                    var authenticationInfo = new AuthenticationInfo()
                    {
                        authMethod = AuthMethod.StartConnectToServer,
                        userId = newUser.UserId
                    };

                    NetworkServer.Login(authenticationInfo, resultCallback);
                });
            }
            else
            {
                var authenticationInfo = new AuthenticationInfo()
                {
                    authMethod = AuthMethod.StartConnectToServer,
                    userId = localGuestId
                };
                NetworkServer.Login(authenticationInfo, resultCallback);
            }
        }

        public void Logout(Action<AuthenticationResult> resultCallback) { }
        public void Binding(Action<AuthenticationResult> resultCallback) { }
        public void ClearCacheLoggedIn() { }

        #endregion Class Methods
    }
}
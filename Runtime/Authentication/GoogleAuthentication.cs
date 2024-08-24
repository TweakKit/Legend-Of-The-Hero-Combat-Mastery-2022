using System;
using UnityEngine;
using Runtime.Server;
using Cysharp.Threading.Tasks;
using Google;

namespace Runtime.Authentication
{
    public class GoogleAuthentication : IAuthentication
    {
        #region Members

        private static readonly string s_googleWebClientId = "";

        #endregion Members

        #region Class Methods

        public void Login(Action<AuthenticationResult> resultCallback)
            => LoginAsync(resultCallback).Forget();

        public void Binding(Action<AuthenticationResult> resultCallback)
            => BindingAsync(resultCallback).Forget();

        public void Logout(Action<AuthenticationResult> resultCallback) { }

        public void ClearCacheLoggedIn()
        {
            try
            {
                if (GoogleSignIn.Configuration == null)
                {
                    GoogleSignIn.Configuration = new GoogleSignInConfiguration {
                        WebClientId = s_googleWebClientId,
                        RequestIdToken = true,
                        UseGameSignIn = false
                    };
                }
                GoogleSignIn.DefaultInstance.SignOut();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
        }

        private async UniTask LoginAsync(Action<AuthenticationResult> resultCallback)
        {
            try
            {
                GoogleSignInUser googleSignInUser = await SignInGoogle(s_googleWebClientId);
                if (googleSignInUser == null)
                {
                    resultCallback?.Invoke(new AuthenticationResult(AuthenticationResultType.Failed, string.Empty));
                    return;
                }

                var (resultType, user) = await AuthenticationManager.Instance.BindWithFirebase(AuthMethod.LoginGoogle, googleSignInUser.IdToken);
                if (resultType != AuthenticationResultType.Successful)
                {
                    resultCallback?.Invoke(new AuthenticationResult(resultType, string.Empty));
                    return;
                }

                var authenticationInfo = new AuthenticationInfo()
                {
                    authMethod = AuthMethod.LoginGoogle,
                    userId = user.UserId
                };

                NetworkServer.Login(authenticationInfo, resultCallback);
                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
        }

        private async UniTask BindingAsync(Action<AuthenticationResult> resultCallback)
        {
            try
            {
                GoogleSignInUser googleSignInUser = await SignInGoogle(s_googleWebClientId);
                if (googleSignInUser == null)
                {
                    resultCallback?.Invoke(new AuthenticationResult(AuthenticationResultType.Failed, string.Empty));
                    return;
                }

                var (resultType, user) = await AuthenticationManager.Instance.BindWithFirebase(AuthMethod.BindGoogle, googleSignInUser.IdToken);
                if (resultType != AuthenticationResultType.Successful)
                {
                    resultCallback?.Invoke(new AuthenticationResult(resultType, string.Empty));
                    return;
                }

                resultCallback?.Invoke(new AuthenticationResult(AuthenticationResultType.Successful, user.UserId));
                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
        }

        private async UniTask<GoogleSignInUser> SignInGoogle(string googleWebClientId)
        {
            if (GoogleSignIn.Configuration == null)
            {
                GoogleSignIn.Configuration = new GoogleSignInConfiguration {
                    WebClientId = googleWebClientId,
                    RequestIdToken = true,
                    UseGameSignIn = false
                };
            }

            return await GoogleSignIn.DefaultInstance.SignIn();
        }

        #endregion Class Methods
    }
}
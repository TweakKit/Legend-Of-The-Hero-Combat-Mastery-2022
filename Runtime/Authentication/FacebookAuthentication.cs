using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Server;
using Runtime.Manager;
using Runtime.Definition;
using Facebook.Unity;
using Cysharp.Threading.Tasks;

namespace Runtime.Authentication
{
    public class FacebookAuthentication : IAuthentication
    {
        #region Class Methods

        public void Login(Action<AuthenticationResult> resultCallback)
        {
            if (!FB.IsInitialized)
                FB.Init(() => InitCallbackLogIn(resultCallback), OnHideUnity);
            else
                InitCallbackLogIn(resultCallback);
        }

        public void Binding(Action<AuthenticationResult> resultCallback)
        {
            if (!FB.IsInitialized)
                FB.Init(() => InitCallbackBinding(resultCallback), OnHideUnity);
            else
                InitCallbackBinding(resultCallback);
        }

        public void Logout(Action<AuthenticationResult> resultCallback) { }

        public void ClearCacheLoggedIn()
        {
            try
            {
                if (FB.IsInitialized && FB.IsLoggedIn)
                    FB.LogOut();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
        }

        private void InitCallbackLogIn(Action<AuthenticationResult> resultCallback)
        {
            FB.ActivateApp();
            if (FB.IsLoggedIn)
            {
                var accessToken = AccessToken.CurrentAccessToken.TokenString;
                LogInFacebookAsync(resultCallback, accessToken).Forget();
            }
            else
            {
                var permissions = new List<string>() { "public_profile", "email" };
                FB.LogInWithReadPermissions(permissions, result => OnLogInWithReadPermissions(result, resultCallback));
            }
        }

        private void InitCallbackBinding(Action<AuthenticationResult> resultCallback)
        {
            FB.ActivateApp();
            if (FB.IsLoggedIn)
            {
                var accessToken = AccessToken.CurrentAccessToken.TokenString;
                BindingFacebookAsync(resultCallback, accessToken).Forget();
            }
            else
            {
                var permissions = new List<string>() { "public_profile", "email" };
                FB.LogInWithReadPermissions(permissions, result => OnBindingWithReadPermissions(result, resultCallback));
            }
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
                GameManager.Instance.StopGameFlow(GameFlowTimeControllerType.LogInFacebook);
            else
                GameManager.Instance.ContinueGameFlow(GameFlowTimeControllerType.LogInFacebook);
        }

        private void OnLogInWithReadPermissions(IResult result, Action<AuthenticationResult> resultCallback)
        {
            if (!string.IsNullOrEmpty(result.Error))
            {
                resultCallback?.Invoke(new AuthenticationResult(AuthenticationResultType.Failed, string.Empty));
            }
            else
            {
                var accessToken = AccessToken.CurrentAccessToken.TokenString;
                LogInFacebookAsync(resultCallback, accessToken).Forget();
            }
        }

        private void OnBindingWithReadPermissions(IResult result, Action<AuthenticationResult> resultCallback)
        {
            if (!string.IsNullOrEmpty(result.Error))
            {
                resultCallback?.Invoke(new AuthenticationResult(AuthenticationResultType.Failed, string.Empty));
            }
            else
            {
                var accessToken = AccessToken.CurrentAccessToken.TokenString;
                BindingFacebookAsync(resultCallback, accessToken).Forget();
            }
        }

        private async UniTask LogInFacebookAsync(Action<AuthenticationResult> resultCallback, string accessToken)
        {
            try
            {
                var (resultType, user) = await AuthenticationManager.Instance.BindWithFirebase(AuthMethod.LoginFacebook, accessToken);
                if (resultType != AuthenticationResultType.Successful)
                {
                    resultCallback?.Invoke(new AuthenticationResult(resultType, string.Empty));
                    return;
                }

                var authenticationInfo = new AuthenticationInfo()
                {
                    authMethod = AuthMethod.LoginFacebook,
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

        private async UniTask BindingFacebookAsync(Action<AuthenticationResult> resultCallback, string accessToken)
        {
            try
            {
                var (resultType, user) = await AuthenticationManager.Instance.BindWithFirebase(AuthMethod.BindFacebook, accessToken);
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

        #endregion Class Methods
    }
}
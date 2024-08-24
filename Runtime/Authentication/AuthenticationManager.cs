using System;
using UnityEngine;
using Runtime.Core.Singleton;
using Runtime.Definition;
using Runtime.Manager.Data;
using Runtime.Message;
using Runtime.Server;
using Runtime.Server.Models;
using Runtime.Tracking;
using AppsFlyerSDK;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Crashlytics;
using Core.Foundation.PubSub;

namespace Runtime.Authentication
{
    public class AuthenticationManager : PersistentMonoSingleton<AuthenticationManager>
    {
        #region Members

        private Registry<FirebaseInitializedMessage> _registryFirebaseInitialized;

        #endregion Members

        #region Properties

        public bool LoggedIn { get; set; }
        public FirebaseAuth Auth => FirebaseAuth.DefaultInstance;
        public FirebaseUser CurrentUser { get; private set; }

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            LoggedIn = false;
            _registryFirebaseInitialized = Messenger.Subscriber().Subscribe<FirebaseInitializedMessage>(OnFirebaseFinitilized);
            AuthStateChanged(this, null);
        }

        private void OnDestroy()
            => _registryFirebaseInitialized.Dispose();

        #endregion API Methods

        #region Class Methods

        public void UpdateUserId(string userId)
        {
            PlayerPrefs.SetString(DataKeys.FIREBASE_GUEST_ID, userId);
            PlayerPrefs.Save();
            IronSource.Agent.setUserId(userId);
            AppsFlyer.setCustomerUserId(userId);
            FirebaseAnalytics.SetUserId(userId);
            Crashlytics.SetUserId(userId);
            FirebaseManager.Instance.SetUserProperty(FirebaseUserProperty.PLAYER_ID, userId);
        }

        public void ExecuteMethod(AuthMethod authType, Action<AuthenticationResult> resultCallback, string accountName = null, string accountPassword = null)
        {
            if (NetworkServer.Instance.IsProcessing)
                return;

            switch (authType)
            {
                case AuthMethod.LoginFacebook:
                    new FacebookAuthentication().Login(resultCallback);
                    break;

                case AuthMethod.LoginGoogle:
                    new GoogleAuthentication().Login(resultCallback);
                    break;

                case AuthMethod.StartConnectToServer:
                    new GuestAuthentication().Login(resultCallback);
                    break;

                case AuthMethod.BindFacebook:
                    new FacebookAuthentication().Binding(resultCallback);
                    break;

                case AuthMethod.BindGoogle:
                    new GoogleAuthentication().Binding(resultCallback);
                    break;
            }
        }

        public void ClearCacheLoggedIn()
        {
            new FacebookAuthentication().ClearCacheLoggedIn();
            new GuestAuthentication().ClearCacheLoggedIn();
            new GoogleAuthentication().ClearCacheLoggedIn();
        }

        public bool IsGoogleBinded() => IsAccountBinded(BindingAccountType.Google);
        public bool IsFacebookBinded() => IsAccountBinded(BindingAccountType.Facebook);

        public bool IsAccountBinded(BindingAccountType bindingAccountType)
            => DataManager.Server.BindingAccountTypes.Contains(bindingAccountType);

        public async UniTask<(AuthenticationResultType, FirebaseUser)> BindWithFirebase(AuthMethod authMethod, string token)
        {
            switch (authMethod)
            {
                case AuthMethod.LoginGoogle:
                    var loginGoogleCredential = GoogleAuthProvider.GetCredential(token, null);
#if DEBUGGING
                    Debug.Log($"[FIREBASE AUTH] Sign in with Firebase googleIdToken: {token}");
#endif
                    var googleLoggedInTask = Auth.SignInWithCredentialAsync(loginGoogleCredential);
                    await UniTask.WaitUntil(() => googleLoggedInTask.IsCompleted);
                    if (googleLoggedInTask.IsCanceled)
                        return (AuthenticationResultType.Canceled, null);
                    else if (googleLoggedInTask.IsFaulted)
                        return (AuthenticationResultType.Failed, null);
                    else
                        return (AuthenticationResultType.Successful, googleLoggedInTask.Result);

                case AuthMethod.BindGoogle:
                    var bindGoogleCredential = GoogleAuthProvider.GetCredential(token, null);
#if DEBUGGING
                    Debug.Log($"[FIREBASE AUTH] Link with Firebase googleIdToken: {token}");
#endif
                    var googleBindingTask = CurrentUser.LinkWithCredentialAsync(bindGoogleCredential);
                    await UniTask.WaitUntil(() => googleBindingTask.IsCompleted);
                    if (googleBindingTask.IsCanceled)
                        return (AuthenticationResultType.Canceled, null);
                    else if (googleBindingTask.IsFaulted)
                        return (AuthenticationResultType.Binded, null);
                    else
                        return (AuthenticationResultType.Successful, googleBindingTask.Result);

                case AuthMethod.LoginFacebook:
                    var loginFacebookCredential = FacebookAuthProvider.GetCredential(token);
#if DEBUGGING
                    Debug.Log($"[FIREBASE AUTH] Sign in with Firebase facebookIdToken: {token}");
#endif
                    var facebookLoggedInTask = Auth.SignInWithCredentialAsync(loginFacebookCredential);
                    await UniTask.WaitUntil(() => facebookLoggedInTask.IsCompleted);
                    if (facebookLoggedInTask.IsCanceled)
                        return (AuthenticationResultType.Canceled, null);
                    else if (facebookLoggedInTask.IsFaulted)
                        return (AuthenticationResultType.Failed, null);
                    else
                        return (AuthenticationResultType.Successful, facebookLoggedInTask.Result);

                case AuthMethod.BindFacebook:
                    var bindFacebookCredential = FacebookAuthProvider.GetCredential(token);
#if DEBUGGING
                    Debug.Log($"[FIREBASE AUTH] Link with Firebase facebookIdToken: {token}");
#endif
                    var facebookBindingTask = CurrentUser.LinkWithCredentialAsync(bindFacebookCredential);
                    await UniTask.WaitUntil(() => facebookBindingTask.IsCompleted);
                    if (facebookBindingTask.IsCanceled)
                        return (AuthenticationResultType.Canceled, null);
                    else if (facebookBindingTask.IsFaulted)
                        return (AuthenticationResultType.Binded, null);
                    else
                        return (AuthenticationResultType.Successful, facebookBindingTask.Result);
            }

            return (AuthenticationResultType.Failed, null);
        }

        private void OnFirebaseFinitilized(FirebaseInitializedMessage message)
            => Auth.StateChanged += AuthStateChanged;

        private void AuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (Auth.CurrentUser != this.CurrentUser)
            {
                var signedIn = this.CurrentUser != Auth.CurrentUser && Auth.CurrentUser != null;
                if (!signedIn && this.CurrentUser != null)
                {
#if DEBUGGING
                    Debug.Log($"[FIREBASE AUTH] Signed out ID = {this.CurrentUser.UserId}");
#endif
                }

#if DEBUGGING
                Debug.LogWarning("CURRENT USER " + Auth.CurrentUser.UserId);
#endif

                this.CurrentUser = Auth.CurrentUser;
                if (signedIn)
                {
#if DEBUGGING
                    Debug.Log($"[FIREBASE AUTH] Signed in ID = {this.CurrentUser.UserId} " +
                              $"(userId for AppsFlyer, Crashlytics, Firebase)");
#endif
                }
            }
        }

        #endregion Class Methods
    }
}
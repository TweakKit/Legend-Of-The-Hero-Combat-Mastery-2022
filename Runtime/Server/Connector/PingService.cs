using System;
using System.Threading;
using UnityEngine.InputSystem;
using Runtime.Message;
using Runtime.Core.Singleton;
using Runtime.Manager;
using Runtime.Authentication;
using Runtime.Definition;
using Runtime.Manager.Data;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;

namespace Runtime.Server
{
    public class PingService : PersistentMonoSingleton<PingService>
    {
        #region Members

        private static readonly float s_pingTime = 5.0f;
        private Registry<GameStateChangedMessage> _gameStateChangedRegistry;
        private Registry<TryReconnectMessage> _tryReconnectRegistry;
        private CancellationTokenSource _pingCancellationTokenSource;
        private AuthenticationInfo? _loginInfo;
        private bool _isReconnecting;
        private bool _hasNotifiedConnectionError;
        private bool _isInitialized;

        #endregion Members

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _isInitialized = false;
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

#if UNITY_EDITOR
            if (Keyboard.current.gKey.IsActuated())
                ConnectorService.Instance.Disconnect();
#endif
        }

        private void OnDestroy()
        {
            _gameStateChangedRegistry.Dispose();
            _tryReconnectRegistry.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        public UniTask Init()
        {
            _isInitialized = true;
            _gameStateChangedRegistry = Messenger.Subscriber().Subscribe<GameStateChangedMessage>(OnGameStateChanged);
            _tryReconnectRegistry = Messenger.Subscriber().Subscribe<TryReconnectMessage>(OnTryReconnect);
            return UniTask.CompletedTask;
        }

        private void LoginByReconnectedResultCallback(AuthenticationResult result)
        {
            if (result.resultType == AuthenticationResultType.Successful)
                Messenger.Publisher().Publish(new GameStateChangedMessage(GameStateEventType.Reconnected));
        }

        private void OnGameStateChanged(GameStateChangedMessage gameStateChangedMessage)
        {
            if (gameStateChangedMessage.GameStateType == GameStateEventType.Connected)
                Connected();
            else if (gameStateChangedMessage.GameStateType == GameStateEventType.Disconnected)
                Disconnected();
            else if (gameStateChangedMessage.GameStateType == GameStateEventType.Reconnected)
                Reconnected();
            else if (gameStateChangedMessage.GameStateType == GameStateEventType.HandShakeSucceed)
                HandShakeSucceed();
        }

        private void OnTryReconnect(TryReconnectMessage message)
        {
            DataManager.Transitioned.SetReloadAfterDisconnect(false);
            OnTryConnect();
        }

        private void Connected()
        {
            _pingCancellationTokenSource = new CancellationTokenSource();
            StartPingAsync(_pingCancellationTokenSource.Token).Forget();
        }

        private void Disconnected()
        {
            _isReconnecting = false;
            _pingCancellationTokenSource?.Cancel();
            NotifyConnectionError();
        }

        private void Reconnected()
        {
            _isReconnecting = false;
            _hasNotifiedConnectionError = false;
        }

        private void HandShakeSucceed()
        {
            if (_isReconnecting)
            {
                if (_loginInfo != null)
                {
                    var loginInfo = (AuthenticationInfo)_loginInfo;
                    switch (loginInfo.authMethod)
                    {
                        case AuthMethod.LoginFacebook:
                            break;
                        case AuthMethod.LoginGoogle:
                            break;
                        case AuthMethod.StartConnectToServer:
                            NetworkServer.Login(loginInfo, LoginByReconnectedResultCallback);
                            break;
                        case AuthMethod.Manual:
                            break;
                        default:
                            break;
                    }
                }
                else Messenger.Publisher().Publish(new GameStateChangedMessage(GameStateEventType.Reconnected));
            }
        }

        private void NotifyConnectionError()
        {
            if (!_hasNotifiedConnectionError)
            {
                _hasNotifiedConnectionError = true;
                var isInGameplay = GameManager.Instance.IsInGameplay;
                if (isInGameplay)
                {
                    var connectToServerLostMessage = new ConnectToServerLostMessage(ServerLostConnectionType.LostConnectionInGameplay, OnTryConnect);
                    Messenger.Publisher().Publish(connectToServerLostMessage);
                }
                else
                {
                    var connectToServerLostMessage = new ConnectToServerLostMessage(ServerLostConnectionType.LostConnectionOutOfGameplay, OnTryConnect);
                    Messenger.Publisher().Publish(connectToServerLostMessage);
                }
            }
        }

        private void OnTryConnect()
        {
            _isReconnecting = true;
            ConnectorService.Instance.Connect(true);
        }

        private async UniTaskVoid StartPingAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(s_pingTime), cancellationToken: cancellationToken, ignoreTimeScale: true);
                if (ConnectorService.Instance.IsConnected())
                    ConnectorService.Instance.Ping();
                else
                    break;
            }
        }

        // TODO Change when update other login method.
        public void SaveLoginInfo(AuthenticationInfo authenticationInfo) => _loginInfo = authenticationInfo;

        #endregion Class Methods
    }
}
using UnityEngine;
using Runtime.Message;
using Core.Foundation.PubSub;
using Com.LuisPedroFonseca.ProCamera2D;

namespace Runtime.Gameplay
{
    public class CameraManager : MonoBehaviour
    {
        #region Members

        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;

        #endregion Members

        #region API Methods

        private void Awake()
            => _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);

        private void OnDestroy()
            => _heroSpawnedRegistry.Dispose();

        #endregion API Methods

        #region Class Methods

        private void OnHeroSpawned(HeroSpawnedMessage heroSpawnedMessage)
        {
            ProCamera2D.Instance.Init();
            ProCamera2D.Instance.AddCameraTarget(heroSpawnedMessage.HeroTransform);
            ProCamera2D.Instance.CenterOnTargets();
        }

        #endregion Class Methods
    }
}
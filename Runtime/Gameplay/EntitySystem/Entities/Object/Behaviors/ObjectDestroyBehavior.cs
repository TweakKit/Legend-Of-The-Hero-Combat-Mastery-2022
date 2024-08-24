using UnityEngine;
using Runtime.Message;
using Runtime.Gameplay.Manager;
using Runtime.Extensions;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public sealed class ObjectDestroyBehavior : ObjectBehavior
    {
        #region Members

        private const string OWNER_OBSTACLE_TRANSFORM_NAME = "obstacle";
        private static readonly float s_entitySpawnFromObjectOffset = 1.5f;
        private static readonly int s_ownerObstacleMaxBoundNodes = 5;
        private Transform _ownerObstacleTransform;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            ownerModel.DestroyEvent += OnDestroy;
            _ownerObstacleTransform = transform.FindChildTransform(OWNER_OBSTACLE_TRANSFORM_NAME);
            if (_ownerObstacleTransform == null)
            {
#if DEBUGGING
                Debug.LogError("No obstacle transform is found inside this object game object!");
#endif
            }
            else _ownerObstacleTransform.gameObject.SetActive(true);
            return true;
        }

        private void OnDestroy()
        {
            UpdateMap();
            SpawnVFX(ownerModel.Position).Forget();

            if (ownerModel.SpawnedEntitiesInfo != null && ownerModel.SpawnedEntitiesInfo.Length > 0)
            {
                var spawnedCenterPosition = ownerModel.Position;
                var spawnedCenterOffsetDistance = s_entitySpawnFromObjectOffset;
                var spawnedEntityInfo = ownerModel.SpawnedEntitiesInfo;
                EntitiesManager.Instance.CreateEntitiesAsync(ownerModel.SpawnedWaveIndex, spawnedCenterPosition, spawnedCenterOffsetDistance, false,
                                                             ownerTransform.GetCancellationTokenOnDestroy(), spawnedEntityInfo).Forget();
            }

            if (ownerModel.EntitySpawnRewardsData != null && ownerModel.EntitySpawnRewardsData.Length > 0)
                Messenger.Publisher().Publish(new RewardsDroppedMessage(ownerModel.Position, ownerModel.EntitySpawnRewardsData));

            Messenger.Publisher().Publish(new ObjectDestroyedMessage(ownerModel));
            PoolManager.Instance.Remove(ownerTransform.gameObject);
        }

        private async UniTaskVoid SpawnVFX(Vector2 spawnPosition)
        {
            var vfx = await PoolManager.Instance.Get(ownerModel.DestroyVFX);
            vfx.transform.position = spawnPosition;
        }

        private void UpdateMap()
        {
            _ownerObstacleTransform.gameObject.SetActive(false);
            MapManager.Instance.RescanMapArea(_ownerObstacleTransform.position, s_ownerObstacleMaxBoundNodes);
        }

        #endregion Class Methods
    }
}
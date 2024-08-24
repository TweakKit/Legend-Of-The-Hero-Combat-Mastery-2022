using System;
using System.Threading;
using UnityEngine;
using Runtime.Extensions;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class CloakOfTimeEquipmentSystem : EquipmentSystem<CloakOfTimeEquipmentSystemModel>
    {
        #region Members

        private const string AREA_DECREASE_PROJECTILE_SPEED = "130001_area_decrease_projectile_speed";
        private static float s_delayActive = 0.3f;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _cancellationTokenSource = new CancellationTokenSource();
            if (ownerModel.CanTriggerSpawnArea)
                StartCountTimeAsync().Forget();
        }

        private async UniTaskVoid StartCountTimeAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(s_delayActive), cancellationToken: _cancellationTokenSource.Token);
            while (true)
            {
                await SpawnAreaAsync(creatorModel.Position);
                await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.AreaCooldown * (1 - creatorModel.GetTotalStatValue(Definition.StatType.CooldownReduction))), cancellationToken: _cancellationTokenSource.Token);
            }
        }

        public override void Disable()
        {
            base.Disable();
            _cancellationTokenSource?.Cancel();
        }

        private async UniTask SpawnAreaAsync(Vector2 spawnPoint)
        {
            var areaGameObject = await PoolManager.Instance.Get(AREA_DECREASE_PROJECTILE_SPEED, cancellationToken: _cancellationTokenSource.Token);
            var area = areaGameObject.GetOrAddComponent<DecreaseProjectileSpeedEffectArea>();
            area.transform.position = spawnPoint;
            area.Init(ownerModel.ProjectileSpeedDecreasePercent, ownerModel.AreaLifeTime, creatorModel);
        }

        #endregion Class Methods
    }
}
using System;
using System.Threading;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Runtime.Extensions;
using Runtime.Definition;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class GumBootsEquipmentSystem : EquipmentSystem<GumBootsEquipmentSystemModel>
    {
        #region Members

        private const string DAMAGE_ZONE_PREFAB_NAME = "equipment_160004_damage_zone";
        private static float s_delayActive = 0.3f;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            if (ownerModel.CanCreateDamageZone)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                if (ownerModel.CanCreateDamageZone)
                    StartCountTimeAsync().Forget();
            }
        }

        public override void Disable()
        {
            base.Disable();
            if (ownerModel.CanCreateDamageZone)
                _cancellationTokenSource?.Cancel();
        }

        private async UniTaskVoid StartCountTimeAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(s_delayActive), cancellationToken: _cancellationTokenSource.Token);
            while (true)
            {
                await CreatDamageZoneAsync(creatorModel.Position, _cancellationTokenSource.Token);
                await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.DelayToCreateDamageZone), cancellationToken: _cancellationTokenSource.Token);
            }
        }

        private async UniTask CreatDamageZoneAsync(Vector2 spawnPoint, CancellationToken cancellationToken)
        {
            var damageZoneEffect = await PoolManager.Instance.Get(DAMAGE_ZONE_PREFAB_NAME, cancellationToken: cancellationToken);
            damageZoneEffect.transform.position = spawnPoint;
            var intervalDamageCircle = damageZoneEffect.GetOrAddComponent<GumBootsDamageZone>();
            intervalDamageCircle.Init(creatorModel, ownerModel.DamageZoneExistTime, ownerModel.DamageZoneTriggeredInterval, DamageSource.FromOther, 0.0f,
                                      new[] { new DamageFactor(StatType.AttackDamage, ownerModel.CreatedDamagePercent) },
                                      ownerModel.DamageZoneModifierIdentities);
        }

        #endregion Class Methods
    }
}
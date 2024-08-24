using System.Threading;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class RoyalCrownEquipmentSystem : EquipmentSystem<RoyalCrownEquipmentSystemModel>
    {
        #region Members

        private const string AFFECT_RANGE_EFFECT_PREFAB_NAME = "equipment_120004_affect_range_effect";
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            if (ownerModel.CanAffect)
            {
                creatorModel.HealthChangedEvent += OnHealthChanged;
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            if (ownerModel.CanAffect)
            {
                creatorModel.HealthChangedEvent += OnHealthChanged;
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }

        public override void Disable()
        {
            base.Disable();
            _cancellationTokenSource?.Cancel();
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (value < 0)
                CreateDamageBoxAsync(creatorModel.Position).Forget();
        }

        private async UniTask CreateDamageBoxAsync(Vector2 spawnPoint)
        {
            var knockBackWaveGameObject = await PoolManager.Instance.Get(AFFECT_RANGE_EFFECT_PREFAB_NAME, _cancellationTokenSource.Token);
            knockBackWaveGameObject.transform.position = spawnPoint;
            knockBackWaveGameObject.transform.localScale = new Vector2(ownerModel.AffectRange, ownerModel.AffectRange);
            var knockBackWave = knockBackWaveGameObject.GetComponent<RoyalCrownKnockBackWave>();
            knockBackWave.Init(creatorModel, DamageSource.FromOther, false, 0.0f, null, ownerModel.SendToEnemiesModifierModels);
        }

        #endregion Class Methods
    }
}
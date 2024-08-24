using System.Collections.Generic;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// Damage area is scalable, cause damage over time like trap, but spawned by characters.
    /// </summary>
    public class DamageAreaData
    {
        #region Members

        public CharacterModel creatorModel;
        public DamageSource damageSource;
        public float lifeTime;
        public float damageInterval;
        public float width;
        public float height;
        public DamageFactor[] damageAreaDamageFactors;
        public StatusEffectModel[] damageAreaDamageModifierModels;
        public DamageFactor[] firstInitDamageFactors;
        public StatusEffectModel[] firstInitDamageModifierModels;

        #endregion Members

        #region Class Methods

        public DamageAreaData(CharacterModel creatorModel, float lifeTime, float damageInterval, DamageSource damageSource,
                              float width, float height, DamageFactor[] damageAreaDamageFactors, StatusEffectModel[] damageAreaDamageModifierModels,
                              DamageFactor[] firstInitDamageFactors, StatusEffectModel[] firstInitDamageModifierModels)
        {
            this.creatorModel = creatorModel;
            this.damageSource = damageSource;
            this.damageAreaDamageFactors = damageAreaDamageFactors;
            this.lifeTime = lifeTime;
            this.width = width;
            this.height = height;
            this.damageInterval = damageInterval;
            this.damageAreaDamageModifierModels = damageAreaDamageModifierModels;
            this.firstInitDamageFactors = firstInitDamageFactors;
            this.firstInitDamageModifierModels = firstInitDamageModifierModels;
        }

        public DamageInfo GetDamageAreaDamageInfo(EntityModel targetModel)
            => creatorModel.GetDamageInfo(damageSource, 0.0f, damageAreaDamageFactors, damageAreaDamageModifierModels, targetModel);

        public DamageInfo GetFirstInitDamageInfo(EntityModel targetModel)
            => creatorModel.GetDamageInfo(damageSource, 0.0f, firstInitDamageFactors, firstInitDamageModifierModels, targetModel);

        #endregion Class Methods
    }

    public class DamageArea : Disposable
    {
        #region Members

        protected float currentDamageTime;
        protected float currentLifetime;
        protected List<IInteractable> damagedTargets;
        protected DamageAreaData data;

        #endregion Members

        #region API Methods

        private void Update()
        {
            if (currentDamageTime > data.damageInterval)
            {
                currentDamageTime = 0.0f;
                DamageTargets();
            }
            else currentDamageTime += Time.deltaTime;

            if (currentLifetime <= data.lifeTime)
                currentLifetime += Time.deltaTime;
            else
                DestroySelf();
        }

        private void OnDisable() => Dispose();

        #endregion API Methods

        #region Class Methods

        public virtual async UniTask BuildAsync(CharacterModel creatorModel, Vector3 position, DamageAreaData data)
        {
            transform.position = position;
            transform.localScale = new Vector2(data.width / 2, data.height / 2);
            this.data = data;
            currentLifetime = 0;
            damagedTargets = new();

            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }

        private void DamageTargets()
        {
            foreach (var damagedTarget in damagedTargets)
            {
                if (data.creatorModel.EntityType.CanCauseDamage(damagedTarget.Model.EntityType))
                {
                    var damageInfo = data.GetDamageAreaDamageInfo(damagedTarget.Model);
                    var damageDirection = (damagedTarget.Model.Position - (Vector2)transform.position).normalized;
                    damagedTarget.GetHit(damageInfo, new DamageMetaData(damageDirection, (Vector2)transform.position));
                }
            }
        }

        private void DestroySelf() => PoolManager.Instance.Remove(gameObject);

        public override void Dispose()
        {
        }

        #endregion Class Methods
    }

}
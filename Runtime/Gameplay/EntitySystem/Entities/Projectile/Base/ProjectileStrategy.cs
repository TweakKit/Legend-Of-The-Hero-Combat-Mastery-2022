using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class ProjectileStrategy<T> : IProjectileStrategy where T : ProjectileStrategyData
    {
        #region Members

        protected T strategyData;
        protected Projectile controllerProjectile;

        #endregion Members

        #region Properties

        #endregion Properties

        #region Class Methods

        public virtual void Init(ProjectileStrategyData projectileStrategyData, Projectile controllerProjectile, Vector2 direction, Vector2 originalPosition, EntityModel targetModel = null)
        {
            strategyData = projectileStrategyData as T;
            this.controllerProjectile = controllerProjectile;
        }

        public virtual void Collide(Collider2D collider) { }
        public virtual void Start() { }
        public virtual void Update() { }

        protected virtual void CreateImpactEffect(Vector2 impactEffectPosition)
        {
            var impactEffectName = string.IsNullOrEmpty(controllerProjectile.ImpactPrefabName) ? VFXNames.RANGED_ATTACK_IMPACT_EFFECT_PREFAB : controllerProjectile.ImpactPrefabName;
            SpawnImpactEffectAsync(impactEffectName, impactEffectPosition).Forget();
        }

        protected virtual async UniTask SpawnImpactEffectAsync(string impactEffectName, Vector2 impactEffectPosition)
        {
            controllerProjectile.GeneratedImpact = true;
            var impactEffect = await PoolManager.Instance.Get(impactEffectName);
            impactEffect.transform.position = impactEffectPosition;
        }

        public virtual void Complete(bool forceComplete, bool displayImpact)
        {
            if(!forceComplete)
                controllerProjectile.CompleteStrategy(displayImpact);
        }

        #endregion Class Methods
    }
}
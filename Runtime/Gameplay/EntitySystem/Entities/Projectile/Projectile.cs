using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IProjectile
    {
        #region Properties

        CharacterModel CreatorModel { get; }

        #endregion Properties

        #region Interface Methods

        UniTask BuildAsync(CharacterModel creatorModel, Vector3 position);
        void InitStrategies(IProjectileStrategy[] projectileStrategies);
        void CompleteStrategy(bool displayImpact);
        void UpdateAdjustSpeedFactor(float adjustSpeedFactor);

        void EnableCollide();
        void DisableCollide();

        #endregion Interface Methods
    }

    /// <summary>
    /// This entity simulates the behavior of a projectile.<br/>
    /// It was originally designed as an entity with many behaviors attached, but because it doesn't have
    /// many behaviors and for the sack of simplicity and performance, its behaviors are written in a single class.
    /// Also this entity doesn't have its own model, as we aim at removing unnecessary model's instances for something like 'Projectile' to save performance.
    /// That's why it doesn't implement the IEntity interface.
    /// </summary>
    public class Projectile : Disposable, IProjectile
    {
        #region Members

        [SerializeField]
        protected string impactPrefabName;
        protected Queue<IProjectileStrategy> strategiesQueue;
        protected IProjectileStrategy currentStrategy;
        protected float adjustSpeedFactor;

        #endregion Members

        #region Properties

        public CharacterModel CreatorModel
        {
            get;
            private set;
        }

        public string ImpactPrefabName => impactPrefabName;
        public bool GeneratedImpact { get; set; }

        public Vector2 CreatorPosition => CreatorModel.Position;
        public Vector2 CenterPosition => transform.position;
        public Vector2 Direction => transform.up;
        public uint SpawnedWaveIndex => CreatorModel.SpawnedWaveIndex;

        #endregion Properties

        #region API Methods

        protected virtual void Update()
            => currentStrategy?.Update();

        #endregion API Methods

        #region Class Methods

        public virtual async UniTask BuildAsync(CharacterModel creatorModel, Vector3 position)
        {
            GeneratedImpact = false;
            currentStrategy = null;
            strategiesQueue?.Clear();
            transform.position = position;
            CreatorModel = creatorModel;
            HasDisposed = false;
            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }

        public override void Dispose() { }

        public void UpdateAdjustSpeedFactor(float adjustSpeedFactor)
            => this.adjustSpeedFactor += adjustSpeedFactor;

        public virtual void InitStrategies(IProjectileStrategy[] projectileStrategies)
        {
            if (strategiesQueue != null)
            {
                while (strategiesQueue.Count > 0)
                {
                    var strategy = strategiesQueue.Dequeue();
                    strategy.Complete(true, true);
                }
            }

            if (projectileStrategies != null)
            {
                projectileStrategies.Reverse();
                strategiesQueue = new Queue<IProjectileStrategy>(projectileStrategies);
            }
            else strategiesQueue = new Queue<IProjectileStrategy>();
            CompleteStrategy(true);
        }

        public void CompleteStrategy(bool displayImpact)
        {
            if (strategiesQueue.TryDequeue(out var nextStrategy))
            {
                currentStrategy = nextStrategy;
                currentStrategy.Start();
            }
            else DestroySelf(displayImpact);
        }

        public void UpdatePosition(Vector3 position)
            => transform.position = position;

        public void UpdatePositionBySpeed(float speed, Vector3 direction)
        {
            var speedFactor = Mathf.Max(0, (1 + adjustSpeedFactor));
            transform.position = transform.position + speed * speedFactor * direction.normalized * Time.deltaTime;
        }

        public void UpdateRotation(Quaternion rotation)
            => transform.rotation = rotation;

        public void CauseDamage(IInteractable targetInteractable, DamageInfo damageInfo, DamageMetaData damageMetaData)
            => targetInteractable.GetHit(damageInfo, damageMetaData);

        protected void DestroySelf(bool displayImpact)
        {
            if (displayImpact)
                GenerateImpact(transform.position).Forget();
            gameObject.transform.parent = null;
            PoolManager.Instance.Remove(gameObject);
        }

        protected async UniTaskVoid GenerateImpact(Vector3 position)
        {
            if (string.IsNullOrEmpty(impactPrefabName) || GeneratedImpact)
                return;

            var impact = await PoolManager.Instance.Get(impactPrefabName);
            impact.transform.position = position;
        }

        public virtual void EnableCollide()
        {
        }

        public virtual void DisableCollide()
        {
        }

        #endregion Class Methods
    }
}
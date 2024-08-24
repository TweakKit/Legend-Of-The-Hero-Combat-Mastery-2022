using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Gameplay.CollisionDetection;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [RequireComponent(typeof(Collider2D))]
    public class CauseActionDroppable : Disposable, ICollisionBody
    {
        #region Members

        [SerializeField]
        private string _disposeEffectName;
        [SerializeField]
        private float _flySpeed;
        [SerializeField]
        private GameObject _trail;
        private ICollisionShape _collisionShape;
        private CollisionSearchTargetType[] _collisionSearchTargetTypes;
        private Collider2D _collider;
        private Action<IInteractable> _collectAction;

        private CancellationTokenSource _lifeTimeCancellationTokenSource;
        private bool _isCollected;

        #endregion Members

        #region Properties

        public int RefId { get; set; }
        public ICollisionShape CollisionShape => _collisionShape;
        public Vector2 CollisionSystemPosition => _collider.bounds.center;
        public Collider2D Collider => _collider;
        public CollisionBodyType CollisionBodyType => CollisionBodyType.TargetDetect;

        public CollisionSearchTargetType[] CollisionSearchTargetTypes => _collisionSearchTargetTypes;

        #endregion Properties

        #region API Methods

        private void OnEnable()
        {
            _collisionSearchTargetTypes = new[] { CollisionSearchTargetType.Hero };
            _collider = gameObject.GetComponent<Collider2D>();
            _collisionShape = this.CreateCollisionShape(_collider);
            HasDisposed = false;
            CollisionSystem.Instance.AddBody(this);
        }

        private void OnDisable()
            => Dispose();

        #endregion API Methods

        #region Class Methods

        public void Init(float lifeTime, Action<IInteractable> collectAction)
        {
            _trail.SetActive(false);
            _isCollected = false;
            _collectAction = collectAction;
            if(lifeTime > 0)
            {
                _lifeTimeCancellationTokenSource = new CancellationTokenSource();
                StartCountLifeTime(lifeTime).Forget();
            }
        }

        public void OnCollision(CollisionResult result, ICollisionBody other)
        {
            if (_isCollected)
                return;

            if (other.Collider != null)
            {
                if (result.collisionType == CollisionType.Enter)
                {
                    var entity = other.Collider.GetComponent<IEntity>();
                    if (entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>();
                        if (interactable != null)
                        {
                            _isCollected = true;
                            _lifeTimeCancellationTokenSource?.Cancel();
                            StartFlyToTarget(interactable).Forget();
                        }
                    }
                }
            }
        }

        private async UniTaskVoid StartCountLifeTime(float lifeTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(lifeTime), cancellationToken: _lifeTimeCancellationTokenSource.Token);
            PoolManager.Instance.Remove(gameObject);
        }

        private async UniTaskVoid StartFlyToTarget(IInteractable interactable)
        {
            _trail.SetActive(true);
            while (interactable != null && interactable.Model != null && Vector2.Distance(interactable.Model.Position, transform.position) > _flySpeed * Time.deltaTime)
            {
                transform.position = Vector2.MoveTowards(transform.position, interactable.Model.Position, _flySpeed * Time.deltaTime);
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }

            _collectAction?.Invoke(interactable);
            SpawnEffectAsync(gameObject.transform.position).Forget();
            PoolManager.Instance.Remove(gameObject);
        }

        private async UniTaskVoid SpawnEffectAsync(Vector2 spawnPoint)
        {
            if (!string.IsNullOrEmpty(_disposeEffectName))
            {
                var effect = await PoolManager.Instance.Get(_disposeEffectName, cancellationToken: this.GetCancellationTokenOnDestroy());
                effect.transform.position = spawnPoint;
            }
        }

        public override void Dispose()
        {
            if (!HasDisposed)
            {
                _lifeTimeCancellationTokenSource?.Cancel();
                HasDisposed = true;
                CollisionSystem.Instance.RemoveBody(this);
            }
        }

        #endregion Class Methods
    }
}
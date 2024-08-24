using System.Threading;
using UnityEngine;
using Runtime.Animation;
using Runtime.Gameplay.Tools.Easing;
using Runtime.Gameplay.Manager;
using Runtime.Extensions;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This entity represents a ray pillar (in the appearance of a little grape), it stands in the scene and is used by other entities.
    /// This ray pillar is used in the case that other entities want to fire ray damage at the position of this ray pillar.<br/>
    /// It's supposed to be designed as an entity with many behaviors attached, but because it doesn't have
    /// many behaviors and for the sack of simplicity and performance, its behaviors are written in a single class.<br/>
    /// That's why it doesn't drive from the BehavioralEntity class, instead drives from the Entity class.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RayPillar : Parasite
    {
        #region Members

        private const string OWNER_OBSTACLE_TRANSFORM_NAME = "obstacle";
        private static readonly int s_ownerObstacleMaxBoundNodes = 3;
        [SerializeField]
        private SingleFullSpriteAnimationPlayer _spriteAnimationPlayer;
        [SerializeField]
        private float _rollingRange;
        [SerializeField]
        private float _rollingDuration;
        [SerializeField]
        private Transform _firingPointTransform;
        private Vector3 _rollingDirection;
        private CancellationTokenSource _cancellationTokenSource;
        private Transform _ownerObstacleTransform;

        #endregion Members

        #region Properties

        public Vector2 FiringPoint => _firingPointTransform != null ? _firingPointTransform.position : transform.position;

        #endregion Properties

        #region Class Methods

        public override void Build(EntityModel model, Vector3 position)
        {
            base.Build(model, position);
            var ownerModel = model as ParasiteModel;
            _rollingDirection = (position - ownerModel.HostPosition).normalized;
            _cancellationTokenSource = new CancellationTokenSource();
            _ownerObstacleTransform = transform.FindChildTransform(OWNER_OBSTACLE_TRANSFORM_NAME);
            if (_ownerObstacleTransform == null)
            {
#if DEBUGGING
                Debug.LogError("No obstacle transform is found inside this ray pillar game object!");
#endif
            }
            else _ownerObstacleTransform.gameObject.SetActive(true);
            StartRollingAsync(_cancellationTokenSource.Token).Forget();
        }

        public override void Dispose()
        {
            if (!HasDisposed)
            {
                HasDisposed = true;
                _ownerObstacleTransform.gameObject.SetActive(false);
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                UpdateMap();
            }
        }

        private async UniTask StartRollingAsync(CancellationToken cancellationToken)
        {
            var originPosition = transform.position;
            var currentRollingDuration = 0.0f;
            while (currentRollingDuration <= _rollingDuration)
            {
                currentRollingDuration += Time.deltaTime;
                var easeValue = Easing.Linear(0.0f, 1.0f, Mathf.Clamp01(currentRollingDuration / _rollingDuration));
                float interpolationValue = Mathf.Lerp(0, _rollingRange,  easeValue);
                Vector3 rollingPosition = originPosition + _rollingDirection * interpolationValue;
                transform.position = rollingPosition;
                if (!MapManager.Instance.IsWalkable(rollingPosition))
                {
                    _spriteAnimationPlayer.Stop(null);
                    break;
                }
                await UniTask.Yield(cancellationToken: cancellationToken);
            }
            UpdateMap();
            _spriteAnimationPlayer.Stop(null);
        }

        private void UpdateMap()
            => MapManager.Instance.RescanMapArea(_ownerObstacleTransform.position, s_ownerObstacleMaxBoundNodes);

        #endregion Class Methods
    }
}
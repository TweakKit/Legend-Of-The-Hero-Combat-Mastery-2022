using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class FloatingText : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private float _lifeTime = 0.5f;
        [SerializeField]
        private float _minX = -0.5f;
        [SerializeField]
        private float _minY = 0.2f;
        [SerializeField]
        private float _maxX = 0.5f;
        [SerializeField]
        private float _maxY = 1.5f;

        private TweenerCore<Vector3, Vector3, VectorOptions> _tweenMove;

        #endregion Members

        #region API Methods

        private void OnEnable()
        {
            var movePosition = new Vector2(Random.Range(_minX, _maxX), Random.Range(_minY,_maxY));
            _tweenMove = transform.DOLocalMove(movePosition, _lifeTime).SetEase(Ease.Linear).SetRelative(true).OnComplete(OnComplete);
        }

        private void OnDisable()
        {
            _tweenMove?.Kill();
        }

        #endregion API Methods

        #region Class Methods

        protected virtual void OnComplete()
        {
            PoolManager.Instance.Remove(gameObject);
        }


        #endregion Class Methods
    }
}

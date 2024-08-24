using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class DropItem : MonoBehaviour
    {
        #region Members

        private const string RECEIVE_REWARD_VFX = "receive_reward_vfx";
        private const string RECEIVE_REWARD_TEXT = "receive_reward_text";

        [SerializeField]
        private GameObject _shadow;
        [SerializeField]
        private Transform _graphic;
        [SerializeField]
        private GameObject _trail;
        [SerializeField]
        private SpriteRenderer _graphicSprite;
        [SerializeField]
        private float _flySpeed;
        [SerializeField]
        private float _stayTime;
        [SerializeField]
        private float _flyDuration;
        [SerializeField]
        private float _flyHeightMax;
        [SerializeField]
        private float _flyHeightMin;
        [SerializeField]
        private float _offsetDropY;
        [SerializeField]
        private float _offsetDropX;
        [SerializeField]
        private float _minOffsetX;
        [SerializeField]
        private float _minOffsetY;

        private Transform _targetTransform;

        #endregion Members

        #region Class Methods

        public void Init(long value, Vector2 spawnPosition, Transform target, Sprite sprite)
        {
            _graphicSprite.sprite = sprite;
            gameObject.SetActive(true);
            _trail.SetActive(false);
            _shadow.SetActive(false);
            transform.position = spawnPosition;
            _targetTransform = target;

            var randomPositionX = UnityEngine.Random.Range(-_offsetDropX, _offsetDropX);
            if (randomPositionX > -_minOffsetX && randomPositionX < 0)
                randomPositionX = -_minOffsetX;
            else if (randomPositionX < _minOffsetX && randomPositionX >= 0)
                randomPositionX = _minOffsetX;

            var randomPositionY = UnityEngine.Random.Range(-_offsetDropY, _offsetDropY);
            if (randomPositionY > -_minOffsetY && randomPositionY < 0)
                randomPositionY = -_minOffsetY;
            else if (randomPositionY < _minOffsetY && randomPositionY >= 0)
                randomPositionY = _minOffsetY;

            var targetDropPosition = (Vector2)transform.position + new Vector2(randomPositionX, randomPositionY);
            transform.DOMove(targetDropPosition, _flyDuration);
            var flyHeight = UnityEngine.Random.Range(_flyHeightMin, _flyHeightMax);
            _graphic.DOLocalMoveY(flyHeight, _flyDuration / 3).SetEase(Ease.OutQuad).OnComplete(() => {
                _graphic.DOLocalMoveY(0, _flyDuration * 2 / 3).SetEase(Ease.OutBounce).OnComplete(() => {
                    StartFlyToTarget(value).Forget();
                });
            });
        }

        private async UniTaskVoid StartFlyToTarget(long value)
        {
            _trail.SetActive(true);
            _shadow.SetActive(true);

            await UniTask.Delay(TimeSpan.FromSeconds(_stayTime), cancellationToken: this.GetCancellationTokenOnDestroy());
            while(_targetTransform && Vector2.Distance(_targetTransform.position, transform.position) > _flySpeed * Time.deltaTime)
            {
                transform.position = Vector2.MoveTowards(transform.position, _targetTransform.position, _flySpeed * Time.deltaTime);
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }

            if (_targetTransform)
            {
                var fx = await PoolManager.Instance.Get(RECEIVE_REWARD_VFX, cancellationToken: this.GetCancellationTokenOnDestroy());
                fx.transform.position = gameObject.transform.position;
                if(value > 0)
                {
                    var receiveRewardsText = await PoolManager.Instance.Get(RECEIVE_REWARD_TEXT, cancellationToken: this.GetCancellationTokenOnDestroy());
                    receiveRewardsText.GetComponent<ReceiveRewardFloatingText>().Init(_targetTransform.position, _graphicSprite.sprite, value);
                }
            }
            PoolManager.Instance.Remove(gameObject);
        }

        #endregion Class Methods
    }
}
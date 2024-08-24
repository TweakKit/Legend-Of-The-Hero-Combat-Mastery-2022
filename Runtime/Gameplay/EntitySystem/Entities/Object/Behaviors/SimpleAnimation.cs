using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Runtime.Definition;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class SimpleAnimation : MonoBehaviour
    {
        #region Members

        private static readonly Color s_appearanceHitEffectColor = new Color(1.0f, 1.0f, 1.0f, 0.85f);
        private static readonly Color s_appearanceNormalColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        [SerializeField]
        private AnimationCurve _animationScaleCurveX;
        [SerializeField]
        private AnimationCurve _animationScaleCurveY;
        [SerializeField]
        private SpriteRenderer[] _spriteRenderers;
        private bool _animating;

        #endregion Members

        #region API Methods

        private void OnEnable()
        {
            _animating = false;
            transform.localScale = new Vector2(1, 1);
        }

        #endregion API Methods

        #region Class Methods

        public void RunAnim(float animationTime)
        {
            if (!_animating)
            {
                _animating = true;
                PresentAnimAsync(animationTime).Forget();
            }
        }

        private async UniTask PresentAnimAsync(float animationTime)
        {
            transform.DOScaleX(1, animationTime).SetEase(_animationScaleCurveX).SetRelative(true).OnComplete(() => _animating = false);
            transform.DOScaleY(1, animationTime).SetEase(_animationScaleCurveY).SetRelative(true).OnComplete(() => _animating = false);
            TintColor(s_appearanceHitEffectColor);
            await UniTask.Delay(TimeSpan.FromSeconds(animationTime/2), cancellationToken: this.GetCancellationTokenOnDestroy());
            TintColor(s_appearanceNormalColor);
        }

        private void TintColor(Color color)
        {
            foreach (var spriteRenderer in _spriteRenderers)
                spriteRenderer.material.SetColor(Constants.HIT_MATERIAL_COLOR_PROPERTY, color);
        }

        #endregion Class Methods
    }
}

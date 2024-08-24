using UnityEngine;

namespace Runtime.Tutorial
{
    [ExecuteInEditMode]
    public class TutorialBlockIndicatorHighlightProp : TutorialBlockIndicatorProp
    {
        #region Members

        public float animatedScaleSpeed;
        public float animatedScaleValue;
        private float _interpolationTime;
        private float _originalScaleValue;

        #endregion Members

        #region Class Methods

        public override void Init()
        {
            base.Init();
            _originalScaleValue = 1.0f;
            _interpolationTime = 0.0f;
        }

        public override void RunUpdate()
        {
            base.RunUpdate();
            if (_interpolationTime > 1.0f)
                _interpolationTime = 0.0f;
            _interpolationTime += Time.unscaledDeltaTime * animatedScaleSpeed;
            var parapolaInterpolationValue = 4.0f * (-_interpolationTime * _interpolationTime + _interpolationTime);
            var scaleValue = Mathf.Lerp(_originalScaleValue, animatedScaleValue, parapolaInterpolationValue);
            transform.localScale = Vector3.one * scaleValue;
        }

        #endregion Class Methods
    }
}
using UnityEngine;

namespace Runtime.FeatureSystem
{
    public class UnlockFeatureObject : UnlockFeature
    {
        #region Members

        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private GameObject _lockGameObject;
        [SerializeField]
        private GameObject[] _disableObjectsWhenLock;

        #endregion Members

        public override void UpdateStatus(bool isUnlocked)
        {
            base.UpdateStatus(isUnlocked);

            if(_lockGameObject)
                _lockGameObject.SetActive(!isUnlocked);

            if (_spriteRenderer)
            {
                if (isUnlocked)
                    _spriteRenderer.material.SetFloat("_GrayScale_Fade_1", 0);
                else
                    _spriteRenderer.material.SetFloat("_GrayScale_Fade_1", 1);
            }
            foreach (var disableObjectWhenLock in _disableObjectsWhenLock)
                disableObjectWhenLock.SetActive(isUnlocked);
        }
    }
}
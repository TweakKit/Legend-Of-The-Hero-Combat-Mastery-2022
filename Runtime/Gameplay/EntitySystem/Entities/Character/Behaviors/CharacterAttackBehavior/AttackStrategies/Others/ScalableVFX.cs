using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class ScalableVFX : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private float _lineLength = 2;
        [SerializeField]
        private float _lineWidth = 1;
        [SerializeField]
        private Transform _lineContainer;
        [SerializeField]
        private GameObject _linePartObject;
        [SerializeField]
        private GameObject _endObject;
        [SerializeField]
        private GameObject _startObject;
        [SerializeField]
        private GameObject[] _scaleParts;
        [SerializeField]
        private GameObject[] _scalePartsByX;
        [SerializeField]
        private GameObject[] _scalePartsByY;

        #endregion Members

        #region Class Methods

        public void Scale(float length, float width = 1)
        {
            foreach (Transform linePart in _lineContainer)
                Destroy(linePart.gameObject);

            var numberOfPart = Mathf.FloorToInt(length / _lineLength);
            if(numberOfPart > 0)
            {
                for (int i = 0; i < numberOfPart; i++)
                {
                    var linePart = Instantiate(_linePartObject, _lineContainer);
                    linePart.transform.localPosition = new Vector2(i * _lineLength, 0);
                    linePart.transform.localScale = new Vector2(1, width / _lineWidth);
                    linePart.SetActive(true);
                }
            }

            var remainLength = length - numberOfPart * _lineLength;
            if (remainLength > 0)
            {
                var finalLinePath = Instantiate(_linePartObject, _lineContainer);
                finalLinePath.transform.localPosition = new Vector2(numberOfPart * _lineLength, 0);
                finalLinePath.transform.localScale = new Vector2(remainLength / _lineLength, width / _lineWidth);
                finalLinePath.SetActive(true);
            }

            if(_startObject)
                _startObject.transform.localScale = new Vector2(width / _lineWidth, width / _lineWidth);

            if (_endObject)
            {
                _endObject.transform.localPosition = new Vector2(length, 0);
                _endObject.transform.localScale = new Vector2(width / _lineWidth, width / _lineWidth);
            }

            foreach (var scalePart in _scaleParts)
                scalePart.transform.localScale = new Vector2(width / _lineWidth, length / _lineLength);

            foreach (var scalePart in _scalePartsByX)
                scalePart.transform.localScale = new Vector2(1, length / _lineLength);

            foreach (var scalePart in _scalePartsByY)
                scalePart.transform.localScale = new Vector2(width / _lineWidth, 1);
        }

        #endregion Class Methods
    }
}
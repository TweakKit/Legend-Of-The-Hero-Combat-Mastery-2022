using System;

namespace Runtime.UI
{
    [Serializable]
    public struct MinMax
    {
        #region Members

        public float min;
        public float max;

        #endregion Members

        #region Struct Methods

        public MinMax(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        #endregion Struct Methods
    }
}
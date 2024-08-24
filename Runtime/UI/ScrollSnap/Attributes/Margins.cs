using System;

namespace Runtime.UI
{
    [Serializable]
    public class Margins
    {
        #region Members

        public float Left, Right, Top, Bottom;

        #endregion Members

        #region Class Methods

        public Margins(float m)
        {
            Left = Right = Top = Bottom = m;
        }

        public Margins(float x, float y)
        {
            Left = Right = x;
            Top = Bottom = y;
        }

        public Margins(float l, float r, float t, float b)
        {
            Left = l;
            Right = r;
            Top = t;
            Bottom = b;
        }

        #endregion Members
    }
}
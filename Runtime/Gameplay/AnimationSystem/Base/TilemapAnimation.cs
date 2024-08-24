using System;
using UnityEngine;

namespace Runtime.Animation
{
    [Serializable]
    public struct TilesetAnimation
    {
        public TrapWithIntervalTile tile;
        public int numberOfFrame;
    }

    [CreateAssetMenu(fileName = "TilemapAnimation", menuName = "Animation/Tilemap")]
    public class TilemapAnimation : ScriptableObject
    {
        #region Members

        public int fps;
        public int[] triggeredColliderFrames;
        public int[] turnOffColliderFrames;
        public TilesetAnimation[] animations;

        #endregion Members
    }

}
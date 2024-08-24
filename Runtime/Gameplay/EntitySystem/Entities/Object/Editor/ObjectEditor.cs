#if UNITY_EDITOR

using UnityEditor;

namespace Runtime.Gameplay.EntitySystem
{
    [CustomEditor(typeof(Object))]
    public sealed class ObjectEditor : BehavioralEntityEditor<ObjectBehaviorType>
    {
        #region Properties

        protected override BehavioralEntity<ObjectBehaviorType> BehavioralEntity => target as Object;

        #endregion Properties
    }
}

#endif
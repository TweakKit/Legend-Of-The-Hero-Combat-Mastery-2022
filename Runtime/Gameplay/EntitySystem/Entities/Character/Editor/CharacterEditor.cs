#if UNITY_EDITOR

using UnityEditor;

namespace Runtime.Gameplay.EntitySystem
{
    [CustomEditor(typeof(Character))]
    public class CharacterEditor : BehavioralEntityEditor<CharacterBehaviorType>
    {
        #region Properties

        protected override BehavioralEntity<CharacterBehaviorType> BehavioralEntity => target as Character;

        #endregion Properties
    }
}

#endif
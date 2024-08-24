#if UNITY_EDITOR

using UnityEditor;

namespace Runtime.Gameplay.EntitySystem
{
    [CustomEditor(typeof(PhysicsCharacter))]
    public sealed class PhysicsCharacterEditor : CharacterEditor { }
}

#endif
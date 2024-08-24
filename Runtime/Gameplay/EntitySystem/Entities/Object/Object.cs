using System;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public enum ObjectBehaviorType
    {
        Interactable = 0,
        Animate = 1,
        Destroy = 4,
    }

    [DisallowMultipleComponent]
    public sealed class Object : BehavioralEntity<ObjectBehaviorType>
    {
        #region Class Methods

        protected override EntityBehavior GetBehaviorByType(Enum behaviorType)
        {
            switch (behaviorType)
            {
                case ObjectBehaviorType.Interactable:
                    return new ObjectInteractableBehavior();

                case ObjectBehaviorType.Destroy:
                    return new ObjectDestroyBehavior();
            }

            return null;
        }

        #endregion Class Methods
    }
}
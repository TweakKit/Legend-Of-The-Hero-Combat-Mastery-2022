using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [DisallowMultipleComponent]
    public sealed class PhysicsCharacter : Character
    {
        #region Properties

        private int PhysicsUpdatableBehaviorsCount { get; set; }
        private IPhysicsUpdatable[] PhysicsUpdatableBehaviors { get; set; }

        #endregion Properties

        #region API Methods

        private void FixedUpdate()
        {
            for (int i = 0; i < PhysicsUpdatableBehaviorsCount; i++)
                PhysicsUpdatableBehaviors[i].Update();
        }

        #endregion API Methods

        #region Class Methods

        protected override void SetUpBehaviors(EntityModel model)
        {
            base.SetUpBehaviors(model);
            var filteredPhysicsUpdatableBehaviors = new List<IPhysicsUpdatable>();
            foreach (var behavior in Behaviors)
            {
                if (behavior is IPhysicsUpdatable)
                    filteredPhysicsUpdatableBehaviors.Add(behavior as IPhysicsUpdatable);
            }
            PhysicsUpdatableBehaviorsCount = filteredPhysicsUpdatableBehaviors.Count;
            PhysicsUpdatableBehaviors = filteredPhysicsUpdatableBehaviors.ToArray();
        }

        #endregion Class Methods
    }
}
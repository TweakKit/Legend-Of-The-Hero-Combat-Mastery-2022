using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class BehavioralEntity<BT> : Entity where BT : Enum
    {
        #region Members

        [SerializeField]
        protected List<BT> behaviorTypes;

        #endregion Members

        #region Properties

        public List<BT> BehaviorTypes => behaviorTypes;
        protected EntityBehavior[] Behaviors { get; set; }

        #endregion Properties

        #region API Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            foreach (var behaviorType in behaviorTypes.Distinct().ToList())
            {
                EntityBehavior behavior = GetBehaviorByType(behaviorType);
                if (behavior != null)
                    behavior.Validate(transform);
            }
        }
#endif

        private void OnDisable() => Dispose();

        #endregion API Methods

        #region Class Methods

        public override void Build(EntityModel model, Vector3 position)
        {
            base.Build(model, position);
            SetUpBehaviors(model);
        }

        public override void Dispose()
        {
            if (!HasDisposed)
            {
                HasDisposed = true;
                if (Behaviors != null)
                {
                    for (int i = 0; i < Behaviors.Length; i++)
                        if (Behaviors[i] is IDisable disable)
                            disable.Disable();
                }
            }
        }

        public override T GetBehavior<T>(bool includeProxy) where T : class
        {
            for (int i = 0; i < Behaviors.Length; i++)
                if (Behaviors[i] is T)
                    return Behaviors[i] as T;
            return null;
        }

        protected virtual void SetUpBehaviors(EntityModel model)
        {
            behaviorTypes = behaviorTypes.Distinct().ToList();
            var behaviors = new List<EntityBehavior>();
            foreach (var behaviorType in behaviorTypes)
            {
                EntityBehavior behavior = GetBehaviorByType(behaviorType);
                if (behavior != null)
                {
                    var isInitializable = behavior.InitModel(model, transform);
                    if (isInitializable)
                        behaviors.Add(behavior);
                }
            }
            Behaviors = behaviors.ToArray();
        }

        protected abstract EntityBehavior GetBehaviorByType(Enum behaviorType);

        #endregion Class Methods
    }
}
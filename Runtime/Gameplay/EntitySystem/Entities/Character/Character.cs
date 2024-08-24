using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public enum CharacterBehaviorType
    {
        AutoInput = 0,
        ControlInput = 1,
        MoveByPhysics = 2,
        MoveByTransform = 3,
        Animate = 4,
        Attack = 5,
        Interactable = 6,
        DetectTarget = 8,
        GetCollideDamage = 9,
        DisplayHUD = 10,
        DisplayIndicator = 11,
        Die = 12,
        ExecuteSkill = 13,
        ExecuteSkillSequenceWithAdditionalSkill = 14,
        ExecuteSkillSequence = 16,
        DisplayDamage = 17,
        Collide = 18,
        Speak = 19,
        DisplayMoveEffect = 20,
        NewControlInput = 21
    }

    [DisallowMultipleComponent]
    public class Character : BehavioralEntity<CharacterBehaviorType>
    {
        #region Properties

        private int UpdatableBehaviorsCount { get; set; }
        private IUpdatable[] UpdatableBehaviors { get; set; }

#if UNITY_EDITOR
        private int GizmozBehaviorsCount { get; set; }
        private IGizmozable[] GizmozBehaviors { get; set; }
#endif

        #endregion Properties

        #region API Methods

        private void Update()
        {
            for (int i = 0; i < UpdatableBehaviorsCount; i++)
                UpdatableBehaviors[i].Update();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for (int i = 0; i < GizmozBehaviorsCount; i++)
                GizmozBehaviors[i].OnGizmos();
        }
#endif

        #endregion API Methods

        #region Class Methods

        protected override EntityBehavior GetBehaviorByType(Enum behaviorType)
        {
            switch (behaviorType)
            {
                case CharacterBehaviorType.AutoInput:
                    return new CharacterAutoInputBehavior();

                case CharacterBehaviorType.ControlInput:
                    return new CharacterControlInputBehavior();

                case CharacterBehaviorType.MoveByTransform:
                    return new CharacterMoveBehavior();

                case CharacterBehaviorType.MoveByPhysics:
                    return new CharacterMovePhysicsBehavior();

                case CharacterBehaviorType.Animate:
                    return new CharacterAnimateBehavior();

                case CharacterBehaviorType.Attack:
                    return new CharacterAttackBehavior();

                case CharacterBehaviorType.Interactable:
                    return new CharacterInteractableBehavior();

                case CharacterBehaviorType.DetectTarget:
                    return new CharacterDetectTargetBehavior();

                case CharacterBehaviorType.GetCollideDamage:
                    return new CharacterGetCollideDamageBehavior();

                case CharacterBehaviorType.DisplayHUD:
                    return new CharacterDisplayHUDBehavior();

                case CharacterBehaviorType.DisplayIndicator:
                    return new CharacterDisplayIndicatorBehavior();

                case CharacterBehaviorType.Die:
                    return new CharacterDieBehavior();

                case CharacterBehaviorType.ExecuteSkill:
                    return new CharacterExecuteSkillBehavior();

                case CharacterBehaviorType.ExecuteSkillSequence:
                    return new CharacterExecuteSkillSequenceBehavior();

                case CharacterBehaviorType.ExecuteSkillSequenceWithAdditionalSkill:
                    return new CharacterExecuteSkillSequenceWithAdditionalSkillBehavior();

                case CharacterBehaviorType.DisplayDamage:
                    return new CharacterDisplayDamageBehavior();

                case CharacterBehaviorType.Collide:
                    return new CharacterCollideBehavior();

                case CharacterBehaviorType.Speak:
                    return new CharacterSpeakBehavior();

                case CharacterBehaviorType.DisplayMoveEffect:
                    return new CharacterDisplayMoveEffectBehavior();

                case CharacterBehaviorType.NewControlInput:
                    return new CharacterNewControlInputBehavior();
            }
            return null;
        }

        protected override void SetUpBehaviors(EntityModel model)
        {
            base.SetUpBehaviors(model);
            var filteredUpdatableBehaviors = new List<IUpdatable>();
#if UNITY_EDITOR
            var filteredGizmozableBehaviors = new List<IGizmozable>();
#endif
            foreach (var behavior in Behaviors)
            {
                if (behavior is IUpdatable)
                    filteredUpdatableBehaviors.Add(behavior as IUpdatable);
#if UNITY_EDITOR
                if (behavior is IGizmozable)
                    filteredGizmozableBehaviors.Add(behavior as IGizmozable);
#endif
            }
            UpdatableBehaviorsCount = filteredUpdatableBehaviors.Count;
            UpdatableBehaviors = filteredUpdatableBehaviors.ToArray();
#if UNITY_EDITOR
            GizmozBehaviorsCount = filteredGizmozableBehaviors.Count;
            GizmozBehaviors = filteredGizmozableBehaviors.ToArray();
#endif
        }

        #endregion Class Methods
    }
}
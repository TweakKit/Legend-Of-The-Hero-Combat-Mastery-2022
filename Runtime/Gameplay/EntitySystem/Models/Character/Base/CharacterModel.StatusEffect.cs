using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    [Flags]
    public enum CharacterStatus
    {
        Stunned = 1 << 0,
        KnockedBack = 1 << 1,
        Pulled = 1 << 2,
        Terrored = 1 << 3,
        Taunted = 1 << 4,
        TrappedPoison = 1 << 5,
        Freezed = 1 << 6,

        MovementLocked = Stunned | KnockedBack | Pulled | Freezed,
        SkillLocked = Stunned | KnockedBack | Pulled | Terrored | Freezed,
        AttackLocked = Stunned | KnockedBack | Pulled | Terrored | Freezed,
        AnimationLocked = Freezed,
        HardCC = Stunned | KnockedBack | Pulled | Terrored | Taunted | Freezed,
        HardCCPriority2 = Stunned | Freezed,
        HardCCPriority3 = KnockedBack | Pulled,
    }

    public abstract partial class CharacterModel : EntityModel
    {
        #region Members

        protected List<StatusEffectType> statusEffectsStackData = new();
        protected CharacterStatus characterStatus;

        #endregion Members

        #region Properties

        public bool IsInMovementLockedStatus => (characterStatus & CharacterStatus.MovementLocked) > 0;
        public bool IsInSkillLockedStatus => (characterStatus & CharacterStatus.SkillLocked) > 0;
        public bool IsInAttackLockedStatus => (characterStatus & CharacterStatus.AttackLocked) > 0;
        public bool IsInHardCCStatus => (characterStatus & CharacterStatus.HardCC) > 0;
        public bool IsInHardCCPriority2Status => (characterStatus & CharacterStatus.HardCCPriority2) > 0;
        public bool IsInHardCCPriority3Status => (characterStatus & CharacterStatus.HardCCPriority3) > 0;
        public bool IsInTrappedPoisonStatus => (characterStatus & CharacterStatus.TrappedPoison) > 0;
        public bool IsInAnimationLockedStatus => (characterStatus & CharacterStatus.AnimationLocked) > 0;

        #endregion Properties

        #region Class Methods

        public virtual void StackStatusEffect(StatusEffectType statusEffectType)
        {
            statusEffectsStackData.Add(statusEffectType);
            int sameCount = statusEffectsStackData.Count(x => x == statusEffectType);
        }

        public virtual void CleanStatusEffectStack(StatusEffectType statusEffectType)
        {
            for (int i = statusEffectsStackData.Count - 1; i >= 0; i--)
                if (statusEffectsStackData[i] == statusEffectType)
                    statusEffectsStackData.RemoveAt(i);
        }

        public virtual int GetStatusEffectStackCount(StatusEffectType statusEffectType)
            => statusEffectsStackData.Count(x => x == statusEffectType);

        public virtual bool CheckContainStatusEffectInStack(HashSet<StatusEffectType> statusEffectTypes)
            => statusEffectsStackData.Any(x => statusEffectTypes.Contains(x));
        public virtual bool CheckContainStatusEffectInStack(StatusEffectType[] statusEffectTypes)
           => statusEffectsStackData.Any(x => statusEffectTypes.Contains(x));

        public virtual void StartGettingStun()
        {
            characterStatus |= CharacterStatus.Stunned;
            SetMoveDirection(Vector2.zero, false);
            HardCCImpactedEvent.Invoke(StatusEffectType.Stun);
        }

        public virtual void StopGettingStun()
        {
            characterStatus &= ~CharacterStatus.Stunned;
            HardCCStoppedEvent.Invoke(StatusEffectType.Stun);
        }

        public virtual void StartGettingFreeze()
        {
            characterStatus |= CharacterStatus.Freezed;
            SetMoveDirection(Vector2.zero, false);
            HardCCImpactedEvent.Invoke(StatusEffectType.Freeze);
        }

        public virtual void StopGettingFreeze()
        {
            characterStatus &= ~CharacterStatus.Freezed;
            HardCCStoppedEvent.Invoke(StatusEffectType.Freeze);
        }

        public virtual void StartGettingKnockback()
        {
            characterStatus |= CharacterStatus.KnockedBack;
            SetMoveDirection(Vector2.zero, false);
            HardCCImpactedEvent.Invoke(StatusEffectType.KnockBack);
        }

        public virtual void GettingKnockback(Vector2 knockbackPosition)
            => MovePosition = knockbackPosition;

        public virtual void StopGettingKnockback()
        {
            characterStatus &= ~CharacterStatus.KnockedBack;
            HardCCStoppedEvent.Invoke(StatusEffectType.KnockBack);
        }

        public virtual void StartGettingPull()
        {
            characterStatus |= CharacterStatus.Pulled;
            SetMoveDirection(Vector2.zero, false);
            HardCCImpactedEvent.Invoke(StatusEffectType.Pull);
        }

        public virtual void GettingPull(Vector2 pullPosition)
            => MovePosition = pullPosition;

        public virtual void StopGettingPull()
        {
            characterStatus &= ~CharacterStatus.Pulled;
            HardCCStoppedEvent.Invoke(StatusEffectType.Pull);
        }

        public virtual void StartGettingTerror()
        {
            characterStatus |= CharacterStatus.Terrored;
            HardCCImpactedEvent.Invoke(StatusEffectType.Terror);
        }

        public virtual void StopGettingTerror()
        {
            characterStatus &= ~CharacterStatus.Terrored;
            if (IsInHardCCPriority2Status || IsInHardCCPriority3Status)
                return;

            SetMoveDirection(Vector2.zero);
            HardCCStoppedEvent.Invoke(StatusEffectType.Terror);
        }

        public virtual void StartGettingTaunt()
        {
            characterStatus |= CharacterStatus.Taunted;
            HardCCImpactedEvent.Invoke(StatusEffectType.Taunt);
        }

        public abstract void GettingTaunt();

        public virtual void StopGettingTaunt()
        {
            characterStatus &= ~CharacterStatus.Taunted;
            if (IsInHardCCPriority2Status || IsInHardCCPriority3Status)
                return;

            SetMoveDirection(Vector2.zero);
            HardCCStoppedEvent.Invoke(StatusEffectType.Taunt);
        }

        public virtual void StartGettingTrapped(TrapType trapType)
        {
            switch (trapType)
            {
                case TrapType.Poison:
                    characterStatus |= CharacterStatus.TrappedPoison;
                    break;
            }
        }

        public virtual void StopGettingTrapped(TrapType trapType)
        {
            switch (trapType)
            {
                case TrapType.Poison:
                    characterStatus &= ~CharacterStatus.TrappedPoison;
                    break;
            }
        }

        #endregion Class Methods
    }
}
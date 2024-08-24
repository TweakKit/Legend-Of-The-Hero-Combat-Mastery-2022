using UnityEngine;
using Runtime.ConfigModel;
using Runtime.Definition;
using System;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class StatusEffectModel
    {
        #region Members

        protected float chance;
        protected Action finishedEvent;

        #endregion Members

        #region Propties

        public Action FinishedEvent => finishedEvent;

        public abstract StatusEffectType StatusEffectType { get; }
        public abstract bool IsStackable { get; }
        public virtual int MaxStack { get; private set; }
        public abstract bool IsOneShot { get; }
        public bool IsAffectable => UnityEngine.Random.Range(0.0f, 1.0f) <= chance;

        #endregion Propties

        #region Class Methods

        public StatusEffectModel(StatusEffectData statusEffectData)
            => chance = statusEffectData.statusEffectDataConfigItem.chance;

        public StatusEffectModel(float chance)
            => this.chance = chance;

        public abstract StatusEffectModel Clone();
        public virtual void Stack(StatusEffectModel stackedStatusEffectModel, bool isMaxStack) { }
        public virtual void AddDuration(float addedDuration) { }
        public virtual void SetMaxStack(int maxStack) => MaxStack = maxStack;
        public void SetFinishedEvent(Action action) => finishedEvent = action;

        #endregion Class Methods
    }

    public class StatusEffectData
    {
        #region Members

        public ModifierDataConfigItem statusEffectDataConfigItem;

        #endregion Members

        #region Class Methods

        public StatusEffectData(ModifierDataConfigItem statusEffectDataConfigItem)
            => this.statusEffectDataConfigItem = statusEffectDataConfigItem;

        #endregion Class Methods
    }
}
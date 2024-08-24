using System;
using Runtime.Definition;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public abstract class StatusEffect<T> : IStatusEffect where T : StatusEffectModel
    {
        #region Members

        protected T ownerModel;

        #endregion Members

        #region Properties

        public StatusEffectModel StatusEffectModel => ownerModel;
        public bool HasFinished { get; protected set; }
        public StatusEffectType StatusEffectType => ownerModel.StatusEffectType;

        #endregion Properties

        #region Class methods

        public virtual void Init(StatusEffectModel statusEffectModel, EntityModel creatorModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
            => ownerModel = statusEffectModel as T;

        public abstract void Update();
        public virtual void Stop() {}

        #endregion Class methods
    }
}
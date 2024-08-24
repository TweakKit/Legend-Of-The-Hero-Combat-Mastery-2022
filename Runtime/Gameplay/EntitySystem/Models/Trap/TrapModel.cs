using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class TrapModel : EntityModel
    {
        #region Members

        public float enterredDamage;
        public TrapType trapType;
        public StatusEffectModel[] damageModifierModels;
        public List<IInteractable> trappedTargets;

        #endregion Members

        #region Properties

        public override EntityType EntityType => EntityType.Trap;
        public Action TriggerStartedEvent { get; set; }
        public Action TriggerStoppedEvent { get; set; }

        #endregion Properties

        #region Class Methods

        public TrapModel(uint spawnedWaveIndex, uint trapUId, uint trapId, TrapLevelConfigItem trapConfigItem, StatusEffectModel[] modifierModels)
            : base(spawnedWaveIndex, trapUId, trapId.ToString(), -1)
        {
            enterredDamage = trapConfigItem.enterredDamage;
            trapType = trapConfigItem.trapType;
            damageModifierModels = modifierModels;
            damageModifierModels = modifierModels;
            trappedTargets = new List<IInteractable>();
            TriggerStartedEvent = () => { };
            TriggerStoppedEvent = () => { };
        }

        public DamageInfo GetDamageInfo(EntityModel targetModel)
        {
#if DEBUGGING
            Debug.Log($"damage_log|| owner: {EntityId}/{EntityType} | entered damage: {enterredDamage}");
#endif
            return new DamageInfo(DamageSource.FromTrap, enterredDamage, damageModifierModels, this, targetModel);
        }

        #endregion Class Methods
    }
}
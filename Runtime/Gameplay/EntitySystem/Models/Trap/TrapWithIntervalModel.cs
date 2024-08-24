using Runtime.ConfigModel;
using Runtime.Definition;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class TrapWithIntervalModel : EntityModel
    {
        #region Members

        public float damage;
        public float interval;

        #endregion  Members

        #region Properties

        public override EntityType EntityType => EntityType.Trap;

        #endregion Properties

        #region Class Methods

        public TrapWithIntervalModel(uint spawnedWaveIndex, uint trapUId, string trapId, TrapWithIntervalLevelConfigItem trapConfigItem) : base(spawnedWaveIndex, trapUId, trapId, -1)
        {
            damage = trapConfigItem.damage;
            interval = trapConfigItem.interval;
        }

        public DamageInfo GetDamageInfo(EntityModel targetModel)
        {
#if DEBUGGING
            Debug.Log($"damage_log|| owner: {EntityId}/{EntityType} | damage: {damage}");
#endif
            return new DamageInfo(DamageSource.FromTrap, damage, null, this, targetModel);
        }

        #endregion Class Methods
    }
}
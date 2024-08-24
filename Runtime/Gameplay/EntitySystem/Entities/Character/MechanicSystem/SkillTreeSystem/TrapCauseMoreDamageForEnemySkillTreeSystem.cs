using Runtime.Message;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class TrapCauseMoreDamageForEnemySkillTreeSystem : SkillTreeSystem<TrapCauseMoreDamageForEnemySkillTreeSystemModel>, IUpdateHealthModifier
    {
        #region Members

        private Registry<EntitySpawnedMessage> _entitySpawnedRegistry;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => -1;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _entitySpawnedRegistry = Messenger.Subscriber().Subscribe<EntitySpawnedMessage>(OnEntitySpawned);
        }

        public override void Disable()
        {
            base.Disable();
            _entitySpawnedRegistry.Dispose();
        }

        private void OnEntitySpawned(EntitySpawnedMessage message)
        {
            if (message.EntityModel.EntityType.IsEnemy())
                (message.EntityModel as CharacterModel).AddUpdateHealthModifier(this);
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty) => (value, damageProperty);

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel)
        {
            if(damageSource == DamageSource.FromTrap)
                value = value * (1 + ownerModel.IncreaseDamagePercent);

            return value;
        }

        #endregion Class Methods
    }

}
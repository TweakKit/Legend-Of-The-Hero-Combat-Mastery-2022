using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class ExplodeDiedEnemySkillTreeSystem : SkillTreeSystem<ExplodeDiedEnemySkillTreeSystemModel>
    {
        #region Members

        private const string EXPLODE_PREFAB = "enemy_died_explode_damage";
        private Registry<CharacterDiedHandleCompletedMessage> _characterDiedHandleCompletedRegistry;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _characterDiedHandleCompletedRegistry = Messenger.Subscriber().Subscribe<CharacterDiedHandleCompletedMessage>(OnCharacterDiedHandleCompleted);
        }

        public override void Disable()
        {
            base.Disable();
            _characterDiedHandleCompletedRegistry.Dispose();
        }

        private void OnCharacterDiedHandleCompleted(CharacterDiedHandleCompletedMessage characterDiedHandleCompletedMessage)
        {
            if (characterDiedHandleCompletedMessage.IsEnemyDied)
            {
                var rate = Random.Range(0, 1f);
                if(rate < ownerModel.RateExplode)
                {
                    var damage = characterDiedHandleCompletedMessage.CharacterModel.MaxHp * ownerModel.HealthPercentToDamage;
                    CreateDamageBoxAsync(damage, creatorModel, characterDiedHandleCompletedMessage.CharacterModel.Position).Forget();
                }
            }
        }

        private async UniTask CreateDamageBoxAsync(float damage, CharacterModel creator, Vector2 spawnPoint)
        {
            var damageBoxGameObject = await PoolManager.Instance.Get(EXPLODE_PREFAB);
            var damageBox = damageBoxGameObject.GetComponent<SpriteAnimatorDamageBox>();
            damageBox.Init(creator, DamageSource.FromOther, false, damage, null, null);
            damageBox.transform.position = spawnPoint;
        }

        #endregion Class Methods
    }
}

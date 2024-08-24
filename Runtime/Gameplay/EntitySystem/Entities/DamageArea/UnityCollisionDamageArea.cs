using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [RequireComponent(typeof(Collider2D))]
    public class UnityCollisionDamageArea : DamageArea
    {
        #region Members

        [SerializeField] private Collider2D _collider;
        protected float timeToCauseFirstTimeDamage = 0.1f;

        #endregion Members

        #region API Methods

        private void OnEnable()
        {
            _collider.enabled = false;
        }

        public async override UniTask BuildAsync(CharacterModel creatorModel, Vector3 position, DamageAreaData data)
        {
            await base.BuildAsync(creatorModel, position, data);
            _collider.enabled = true;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>(true);
                if (interactable != null && !interactable.Model.IsDead)
                {
                    if (interactable.Model.EntityType != data.creatorModel.EntityType && !damagedTargets.Contains(interactable))
                    {
                        damagedTargets.Add(interactable);
                        if (currentLifetime <= timeToCauseFirstTimeDamage)
                        {
                            var damageInfo = data.GetFirstInitDamageInfo(interactable.Model);
                            interactable.GetHit(damageInfo, default);
                        }
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>(true);
                if (interactable != null)
                {
                    if (damagedTargets.Contains(interactable))
                        damagedTargets.Remove(interactable);
                }
            }
        }

        #endregion API Methods
    }
}
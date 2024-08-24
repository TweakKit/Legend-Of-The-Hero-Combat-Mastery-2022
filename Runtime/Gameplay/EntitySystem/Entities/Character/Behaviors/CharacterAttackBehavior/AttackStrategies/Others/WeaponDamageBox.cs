using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class WeaponDamageBox : SpriteAnimatorDamageBox
    {
        #region Class Methods

        protected override void HitTarget(IInteractable interactable, Vector2 hitPoint)
        {
            base.HitTarget(interactable, hitPoint);
            CreateImpactEffect(hitPoint);
        }

        private void CreateImpactEffect(Vector2 impactEffectPosition)
        {
            var impactEffectName = VFXNames.MELEE_ATTACK_IMPACT_EFFECT_PREFAB;
            SpawnImpactEffectAsync(impactEffectName, impactEffectPosition).Forget();
        }

        private async UniTask SpawnImpactEffectAsync(string impactEffectName, Vector2 impactEffectPosition)
        {
            var impactEffect = await PoolManager.Instance.Get(impactEffectName);
            impactEffect.transform.position = impactEffectPosition;
        }

        #endregion Class Methods
    }
}
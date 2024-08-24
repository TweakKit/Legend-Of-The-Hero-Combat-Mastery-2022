using System.Threading;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Runtime.Extensions;
using Runtime.Definition;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior displays visuals when get hits.
    /// </summary>
    public class CharacterDisplayDamageBehavior : CharacterBehavior, IDisable
    {
        #region Members

        private static string s_vfxHolderName = "vfx_holder";
        private static string s_vfxTopPositionName = "top_position";
        private static string s_vfxMiddlePositionName = "middle_position";
        private const string DAMAGE_TEXT = "damage_text";
        private const string DAMAGE_CRIT_TEXT = "damage_crit_text";
        private const string DAMAGE_POISON_TEXT = "damage_poison_text";
        private const string HERO_DAMAGE_TEXT = "hero_damage_text";

        private const string HEAL_TEXT = "heal_text";
        private const string INSTANT_HEAL_VFX_NAME = "instant_heal_vfx";
        private const string CRIT_INSTANT_HEAL_VFX_NAME = "crit_instant_heal_vfx";

        private CancellationTokenSource _cancellationTokenSource;
        private Transform _topPosition;
        private Transform _middlePosition;

        #endregion Members

        #region Class Methods

#if UNITY_EDITOR
        public override void Validate(Transform ownerTransform)
        {
            var vfxHolder = ownerTransform.FindChildTransform(s_vfxHolderName);
            if (vfxHolder == null)
            {
                Debug.LogError("VFX holder's name is not mapped!");
                return;
            }
        }
#endif

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            ownerModel.HealthChangedEvent += OnHealthChanged;
            var vfxHolder = transform.FindChildTransform(s_vfxHolderName);
            _topPosition = vfxHolder.Find(s_vfxTopPositionName);
            _middlePosition = vfxHolder.Find(s_vfxMiddlePositionName);
            _cancellationTokenSource = new CancellationTokenSource();
            return true;
        }

        private void OnHealthChanged(float deltaHp, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (deltaHp != 0)
            {
                if (deltaHp > 0)
                    DisplayHeal(deltaHp, damageProperty).Forget();
                else
                    DisplayDamageText(deltaHp, damageProperty).Forget();
            }
        }

        private async UniTaskVoid DisplayHeal(float deltaHP, DamageProperty damageProperty)
        {
            DamageFloatingTextController.Instance.Spawn(HEAL_TEXT, deltaHP, _topPosition.position, _cancellationTokenSource.Token).Forget();
            GameObject healFX = await PoolManager.Instance.Get(damageProperty.IsCrit() ? CRIT_INSTANT_HEAL_VFX_NAME : INSTANT_HEAL_VFX_NAME, _cancellationTokenSource.Token);
            healFX.transform.SetParent(_middlePosition);
            healFX.transform.localPosition = Vector2.zero;
        }

        private async UniTaskVoid DisplayDamageText(float deltaHP, DamageProperty damageProperty)
        {
            if (damageProperty == DamageProperty.InstantKill)
            {
                var instantKillEffect = await PoolManager.Instance.Get(SpeakText.INSTANT_KILL, cancellationToken: _cancellationTokenSource.Token);
                instantKillEffect.transform.position = _topPosition.position;
            }
            else
            {
                var damageText = DAMAGE_TEXT;
                if (ownerModel.EntityType.IsHero())
                {
                    damageText = HERO_DAMAGE_TEXT;
                }
                else
                {
                    if (damageProperty.IsCrit())
                        damageText = DAMAGE_CRIT_TEXT;
                    else if (damageProperty == DamageProperty.Poison)
                        damageText = DAMAGE_POISON_TEXT;
                }

                await DamageFloatingTextController.Instance.Spawn(damageText, deltaHP, _topPosition.position, _cancellationTokenSource.Token);
            }
        }

        public void Disable() => _cancellationTokenSource.Cancel();

        #endregion Class Methods
    }
}
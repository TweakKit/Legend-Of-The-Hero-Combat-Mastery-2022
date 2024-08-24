using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Extensions;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior tells the world whether the character is interactable with the world or not.<br/>
    /// The interactable actions includes:<br/>
    ///     + Get Hit: Indicates that the character can get hit.<br/>
    ///     + Get Affected: Indicates that the character can get affected by status effects.<br/>
    ///     + Get Trapped: Indicates that the character can get trapped.<br/>
    ///     + Stop Trapped: Indicates the character can stop getting trapped.<br/>
    /// </summary>
    public sealed class CharacterInteractableBehavior : CharacterBehavior, IInteractable, IUpdatable, IDisable
    {
        #region Members

        private static string s_vfxHolderName = "vfx_holder";
        private static string s_vfxTopPositionName = "top_position";
        private static string s_vfxBottomPositionName = "bottom_position";
        private static string s_vfxMiddlePositionName = "middle_position";
        private Transform _topPosition;
        private Transform _bottomPosition;
        private Transform _middlePosition;
        private List<IStatusEffect> _statusEffects;
        private Dictionary<StatusEffectType, StatusEffectVFX> _statusEffectVFXsDictionary;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Properties

        public EntityModel Model => ownerModel;

        #endregion Properties

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
            var vfxHolder = transform.FindChildTransform(s_vfxHolderName);
            _topPosition = vfxHolder.Find(s_vfxTopPositionName);
            _bottomPosition = vfxHolder.Find(s_vfxBottomPositionName);
            _middlePosition = vfxHolder.Find(s_vfxMiddlePositionName);
            _statusEffects = new List<IStatusEffect>();
            _statusEffectVFXsDictionary = new Dictionary<StatusEffectType, StatusEffectVFX>();
            ownerModel.DeathEvent += OnDeath;
            _cancellationTokenSource = new CancellationTokenSource();
            return true;
        }

        public void GetHit(DamageInfo damageInfo, DamageMetaData damageMetaData)
        {
            if (ownerModel.IsDamagable)
            {
                HandleReceiveDamage(damageInfo);
                HandleReceiveStatusEffects(damageInfo.creatorModel, damageInfo.damageStatusEffectModels, new StatusEffectMetaData(damageMetaData));
            }
        }

        public void GetAffected(AffectedStatusEffectInfo affectedStatusEffectInfo, StatusEffectMetaData statusEffectMetaData)
        {
            HandleReceiveStatusEffects(affectedStatusEffectInfo.creatorModel, affectedStatusEffectInfo.affectedStatusEffectModels, statusEffectMetaData);
            HandleCutOffStatusEffects(affectedStatusEffectInfo.cutOffStatusEffectTypes);
        }

        public void GetTrapped(TrapType trapType, DamageInfo damageInfo, DamageMetaData damageMetaData)
        {
            ownerModel.StartGettingTrapped(trapType);
            GetHit(damageInfo, damageMetaData);
        }

        public void StopTrapped(TrapType trapType)
            => ownerModel.StopGettingTrapped(trapType);

        public void Update()
        {
            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                _statusEffects[i].Update();
                if (_statusEffects[i].HasFinished)
                {
                    var numberEffectsSameType = _statusEffects.Count(x => x.StatusEffectType == _statusEffects[i].StatusEffectType);
                    if (numberEffectsSameType == 1 && _statusEffectVFXsDictionary.ContainsKey(_statusEffects[i].StatusEffectType))
                    {
                        var statusEffectVFX = _statusEffectVFXsDictionary[_statusEffects[i].StatusEffectType];
                        statusEffectVFX.Dispose();
                        _statusEffectVFXsDictionary.Remove(_statusEffects[i].StatusEffectType);
                    }
                    ownerModel.CleanStatusEffectStack(_statusEffects[i].StatusEffectType);
                    _statusEffects.RemoveAt(i);
                }
            }
        }

        private void OnDeath(DamageSource damageSource)
        {
            var statusEffectsVFX = ownerTransform.GetComponentsInChildren<StatusEffectVFX>();
            foreach (var statusEffectVFX in statusEffectsVFX)
                statusEffectVFX.Dispose(true);
        }

        public void Disable()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void HandleReceiveDamage(DamageInfo damageInfo)
        {
            var dodgeChance = ownerModel.GetTotalStatValue(StatType.DodgeChance);
            if (Random.Range(0, 1f) >= dodgeChance)
            {
                var armor = ownerModel.GetTotalStatValue(StatType.Armor);
                var damageReduction = ownerModel.GetTotalStatValue(StatType.DamageReduction);
                var damageTaken = (damageInfo.damage - armor * (1 - damageInfo.armorPenetration)) * (1 - damageReduction);
                damageTaken = damageTaken > 0 ? damageTaken : 0;

#if DEBUGGING
                var log = $"get_damage_log || target: {ownerModel.EntityId}/{ownerModel.EntityType} | damageReduction: {damageReduction} " +
                      $"| armor: {armor} | armorPenetration: {damageInfo.armorPenetration} | damageTaken: {damageTaken}";
                Debug.Log($"{log}");
#endif

                ownerModel.DebuffHp(damageTaken, damageInfo.damageSource, damageInfo.damageProperty, damageInfo.creatorModel);
                ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustGetHit);

                if (damageInfo.creatorModel != null && damageInfo.creatorModel is CharacterModel)
                {
                    var characterModel = damageInfo.creatorModel as CharacterModel;
                    characterModel.CreateDamage(damageTaken, ownerModel, damageInfo);
                }
            }
            else
            {
                ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustDodge);
#if DEBUGGING

                var log = $"get_damage_log || target: {ownerModel.EntityId}/{ownerModel.EntityType} | dodged";
                Debug.Log($"{log}");
#endif
            }
        }

        private void HandleReceiveStatusEffects(EntityModel statusEffectsSenderModel, StatusEffectModel[] statusEffectModels, StatusEffectMetaData statusEffectMetaData)
        {
            if (statusEffectModels == null || ownerModel.IsDead)
                return;

            foreach (var statusEffectModel in statusEffectModels)
            {
                var newStatusEffectModel = statusEffectModel.Clone();
                if (newStatusEffectModel.IsAffectable)
                {
                    var newStatusEffectType = newStatusEffectModel.StatusEffectType;
                    var newStatusEffectPriority = GetStatusPriorityType(newStatusEffectType);

                    // Status priority 0 will be applied no matter what.
                    if (newStatusEffectPriority != StatusPriorityType.Priority0)
                    {
                        var highestPriority = StatusPriorityType.Priority0;
                        if (_statusEffects.Count > 0)
                            highestPriority = _statusEffects.Max(x => GetStatusPriorityType(x.StatusEffectType));

                        // Status which has lower priority will not be applied.
                        if (newStatusEffectPriority < highestPriority)
                            continue;

                        // Status priority 3 will not remove other status.
                        if (newStatusEffectPriority != StatusPriorityType.Priority3)
                        {
                            // Remove status same priority (1 or 2) and different type.
                            for (int i = _statusEffects.Count - 1; i >= 0; i--)
                            {
                                if (_statusEffects[i].StatusEffectType != newStatusEffectType && GetStatusPriorityType(_statusEffects[i].StatusEffectType) == newStatusEffectPriority)
                                    _statusEffects[i].Stop();
                            }
                        }
                    }

                    var numberOfSameStatusEffects = _statusEffects.Count(x => x.StatusEffectType == newStatusEffectType);
                    if (newStatusEffectModel.IsStackable)
                    {
                        // Stack status effect: The new status effect will always replace the old one.
                        if (numberOfSameStatusEffects > 0)
                        {
                            var oldStatusEffect = _statusEffects.First(x => x.StatusEffectType == newStatusEffectType);
                            newStatusEffectModel.Stack(oldStatusEffect.StatusEffectModel,
                                                        ownerModel.GetStatusEffectStackCount(newStatusEffectModel.StatusEffectType) >= newStatusEffectModel.MaxStack && newStatusEffectModel.MaxStack > 0);
                            oldStatusEffect.Stop();
                            _statusEffects.Remove(oldStatusEffect);
                        }
                        ownerModel.StackStatusEffect(newStatusEffectType);
                    }
                    else
                    {
                        // Stop all old same type status effects.
                        if (numberOfSameStatusEffects > 0)
                        {
                            for (int i = _statusEffects.Count - 1; i >= 0; i--)
                            {
                                if (_statusEffects[i].StatusEffectType == newStatusEffectType)
                                {
                                    _statusEffects[i].Stop();
                                    _statusEffects.Remove(_statusEffects[i]);
                                }
                            }
                        }
                        else ownerModel.StackStatusEffect(newStatusEffectType);
                    }

                    // Create a new status effect.
                    var newStatusEffect = StatusEffectFactory.GetStatusEffect(newStatusEffectModel.StatusEffectType);
                    newStatusEffect.Init(newStatusEffectModel, statusEffectsSenderModel, ownerModel, statusEffectMetaData);
                    _statusEffects.Add(newStatusEffect);

                    if (numberOfSameStatusEffects == 0 && !_statusEffectVFXsDictionary.ContainsKey(newStatusEffect.StatusEffectType))
                    {
                        string statusEffectVFXPrefabName = VFXNames.GetStatusEffectPrefabName(newStatusEffect.StatusEffectType);
                        if (!string.IsNullOrEmpty(statusEffectVFXPrefabName))
                            CreateStatusEffectVFX(newStatusEffectModel, statusEffectVFXPrefabName).Forget();
                    }
                }
            }
        }

        private void HandleCutOffStatusEffects(StatusEffectType[] cutOffStatusEffectTypes)
        {
            if (cutOffStatusEffectTypes != null)
            {
                foreach (var cutOffStatusEffectType in cutOffStatusEffectTypes)
                {
                    for (int i = _statusEffects.Count - 1; i >= 0; i--)
                    {
                        if (_statusEffects[i].StatusEffectType == cutOffStatusEffectType)
                            _statusEffects[i].Stop();
                    }
                }
            }
        }

        private async UniTaskVoid CreateStatusEffectVFX(StatusEffectModel statusEffectModel, string statusEffectPrefabName)
        {
            var statusEffectVFXGameObject = await PoolManager.Instance.Get(statusEffectPrefabName, cancellationToken: _cancellationTokenSource.Token);
            var statusEffectVFX = statusEffectVFXGameObject.GetOrAddComponent<StatusEffectVFX>();
            statusEffectVFXGameObject.transform.SetParent(ownerTransform);
            statusEffectVFXGameObject.transform.localPosition = GetStatusEffectPosition(statusEffectModel.StatusEffectType);
            if (!statusEffectModel.IsOneShot)
                _statusEffectVFXsDictionary.Add(statusEffectModel.StatusEffectType, statusEffectVFX);
        }

        private StatusPriorityType GetStatusPriorityType(StatusEffectType statusEffectType)
        {
            switch (statusEffectType)
            {
                case StatusEffectType.Pull:
                case StatusEffectType.KnockBack:
                    return StatusPriorityType.Priority3;

                case StatusEffectType.Stun:
                    return StatusPriorityType.Priority2;

                case StatusEffectType.Terror:
                case StatusEffectType.Taunt:
                    return StatusPriorityType.Priority1;

                case StatusEffectType.Bleed:
                case StatusEffectType.Regen:
                case StatusEffectType.Dread:
                case StatusEffectType.Haste:
                case StatusEffectType.Quick:
                case StatusEffectType.Tough:
                case StatusEffectType.Hardened:
                case StatusEffectType.Chill:
                case StatusEffectType.Freeze:
                case StatusEffectType.TrappedPoison:
                case StatusEffectType.DamageReductionDebuff:
                default:
                    return StatusPriorityType.Priority0;
            }
        }

        private Vector3 GetStatusEffectPosition(StatusEffectType statusEffectType)
        {
            switch (statusEffectType)
            {
                case StatusEffectType.Stun:
                case StatusEffectType.Taunt:
                case StatusEffectType.Terror:
                case StatusEffectType.TrappedPoison:
                case StatusEffectType.DamageReductionDebuff:
                    return _topPosition.localPosition;
                case StatusEffectType.Chill:
                case StatusEffectType.Bleed:
                    return _middlePosition.localPosition;
                case StatusEffectType.Freeze:
                    return _bottomPosition.localPosition;
                default:
                    return _bottomPosition.localPosition;
            }
        }

        #endregion Class Methods
    }
}
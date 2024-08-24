using System.Collections.Generic;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class SkillModel
    {
        #region Properties

        public abstract SkillType SkillType { get; }
        public abstract bool CanBeCanceled { get; }
        public abstract AutoInputStrategyType AutoInputStrategyType { get; }
        public SkillTargetType TargetType { get; protected set; }
        public float CastRange { get; protected set; }
        public float Cooldown { get; set; }
        public float CurrentCooldown { get; set; }
        public bool IsUsing { get; set; }
        public bool IsReady => CurrentCooldown <= 0;

        #endregion Properties

        #region Class Methods

        public SkillModel(SkillData skillData)
        {
            TargetType = skillData.configItem.TargetType;
            CastRange = skillData.configItem.castRange != -1 ? skillData.configItem.castRange : float.MaxValue;
            Cooldown = skillData.configItem.cooldown;
            CurrentCooldown = 0;
            IsUsing = false;
        }

        #endregion Class Methods
    }

    public class SkillData
    {
        #region Members

        public SkillDataConfigItem configItem;
        public Dictionary<string, StatusEffectModel[]> modifierModelsDictionary;

        #endregion Members

        #region Class Methods

        public SkillData(SkillDataConfigItem configItem, Dictionary<string, StatusEffectModel[]> modifierModelsDictionary)
        {
            this.configItem = configItem;
            this.modifierModelsDictionary = modifierModelsDictionary;
        }

        public StatusEffectModel[] GetModifierModels(string key)
        {
            if (modifierModelsDictionary != null)
            {
                if (modifierModelsDictionary.ContainsKey(key))
                    return modifierModelsDictionary[key];
                else
                    return null;
            }
            else return null;
        }

        #endregion Class Methods
    }
}
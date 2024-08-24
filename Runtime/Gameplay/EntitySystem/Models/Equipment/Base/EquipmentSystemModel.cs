using System.Collections.Generic;
using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class EquipmentSystemModel
    {
        #region Properties

        public abstract EquipmentSystemType EquipmentSystemType { get; }
        public Dictionary<string, StatusEffectModel[]> ModifierModelsDictionary { get; set; }

        #endregion Properties

        #region Class Methods

        public abstract void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem);

        public StatusEffectModel[] GetModifierModels(string key)
        {
            if (ModifierModelsDictionary != null)
            {
                if (ModifierModelsDictionary.ContainsKey(key))
                    return ModifierModelsDictionary[key];
                else
                    return null;
            }
            else return null;
        }


        #endregion Class Methods
    }
}
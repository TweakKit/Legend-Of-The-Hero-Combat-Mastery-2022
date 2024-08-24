using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Runtime.ConfigModel;
using Runtime.Core.Singleton;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Manager.Data;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class MechanicSystemManager : MonoSingleton<MechanicSystemManager>, IDisposable
    {
        #region Members

        private List<IMechanicSystem> _mechanicSystems;
        private bool _isInitialized;

        #endregion Members

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _isInitialized = false;
        }

        #endregion API Methods

        #region Class Methods

        public async UniTask InitAsync(HeroModel heroModel, EquipmentEquippedData[] equipmentsEquippedData)
        {
            if (_isInitialized)
            {
                if (_mechanicSystems != null)
                {
                    foreach (var equipmentSystem in _mechanicSystems)
                        equipmentSystem.Reset(heroModel);
                }
                return;
            }

            _isInitialized = true;
            _mechanicSystems = new List<IMechanicSystem>();

            // Mechanics from common
            var invincibleWhenGetHurt = new InvincibleWhenGetHurtCommonMechanicSystem();
            invincibleWhenGetHurt.Init(heroModel);
            _mechanicSystems.Add(invincibleWhenGetHurt);

            // Mechanics from equipments
            var requiredUnlockedSubStat = GameplayDataManager.Instance.EquipmentUnlockedSubStatConfig.items.Select(x => x.requiredLevel).ToArray();
            foreach (var equipment in equipmentsEquippedData)
            {
                var maxRequiredSatisfiedLevel = -1;
                var indexOfmaxRequiredSatisfiedLevel = -1;

                for (int i = 0; i < requiredUnlockedSubStat.Length; i++)
                {
                    var requiredLevel = requiredUnlockedSubStat[i];
                    if (requiredLevel <= equipment.Level && requiredLevel >= maxRequiredSatisfiedLevel)
                    {
                        maxRequiredSatisfiedLevel = requiredLevel;
                        indexOfmaxRequiredSatisfiedLevel = i;
                    }
                }

                if (indexOfmaxRequiredSatisfiedLevel != -1)
                {
                    var rarityUnlocked = (RarityType)Mathf.Min(indexOfmaxRequiredSatisfiedLevel + 1, (int)equipment.RarityType);
                    var equipmentSystemType = (EquipmentSystemType)equipment.EquipmentId;
                    var equipmentSystem = EquipmentSystemFactory.GetEquipmentSystem(equipmentSystemType);
                    if (equipmentSystem != null)
                    {
                        var equipmentSystemModel = await GameplayDataManager.Instance.GetEquipmentSystemModel(equipmentSystemType, rarityUnlocked);
                        equipmentSystem.Init(equipmentSystemModel, heroModel);
                        _mechanicSystems.Add(equipmentSystem);
                    }
                }
            }

            // Mechanics from skill trees
            SkillTreeSystemData[] unlockedSkillTreeSystems = DataDispatcher.Instance.UnlockedSkillTreeSystems;

            if(unlockedSkillTreeSystems != null)
            {
                foreach (var unlockedSkillTreeSystem in unlockedSkillTreeSystems)
                {
                    var skillTreeSystem = SkillTreeSystemFactory.GetSkillTreeSystem(unlockedSkillTreeSystem.skillTreeSystemType);
                    if (skillTreeSystem != null)
                    {
                        var skillTreeSystemModel = await GameplayDataManager.Instance.GetSkillTreeSystemModel(unlockedSkillTreeSystem.skillTreeSystemType, unlockedSkillTreeSystem.skillTreeSystemDataId);
                        skillTreeSystem.Init(skillTreeSystemModel, heroModel);
                        _mechanicSystems.Add(skillTreeSystem);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_mechanicSystems != null)
            {
                foreach (var equipmentSystem in _mechanicSystems)
                    equipmentSystem.Disable();
            }
        }

        #endregion Class Methods
    }
}
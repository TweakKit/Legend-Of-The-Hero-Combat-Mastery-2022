using System.Collections.Generic;
using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public static class EquipmentSystemModelFactory
    {
        #region Class Methods

        public static EquipmentSystemModel GetEquipmentModel(EquipmentSystemType equipmentSystemType, EquipmentMechanicDataConfigItem equipmentData, Dictionary<string, StatusEffectModel[]> modifierModelsDictionary)
        {
            EquipmentSystemModel equipmentSystemModel = null;
            switch (equipmentSystemType)
            {
                // SET 1
                case EquipmentSystemType.HatOfEndurance:
                    equipmentSystemModel = new HatOfEnduranceEquipmentSystemModel();
                    break;
                case EquipmentSystemType.CloakOfTime:
                    equipmentSystemModel = new CloakOfTimeEquipmentSystemModel();
                    break;
                case EquipmentSystemType.LocketOfWeakness:
                    equipmentSystemModel = new LocketOfWeaknessEquipmentSystemModel();
                    break;
                case EquipmentSystemType.GauntletsOfTheOverlord:
                    equipmentSystemModel = new GauntletsOfTheOverlordEquipmentSystemModel();
                    break;
                case EquipmentSystemType.BootsOfSwiftness:
                    equipmentSystemModel = new BootsOfSwiftnessEquipmentSystemModel();
                    break;

                // SET 2
                case EquipmentSystemType.FlowerWreath:
                    equipmentSystemModel = new FlowerWreathEquipmentSystemModel();
                    break;
                case EquipmentSystemType.MagicCloak:
                    equipmentSystemModel = new MagicCloakEquipmentSystemModel();
                    break;
                case EquipmentSystemType.PuppyGloves:
                    equipmentSystemModel = new PuppyGlovesEquipmentSystemModel();
                    break;
                case EquipmentSystemType.MonsterBoots:
                    equipmentSystemModel = new MonsterBootsEquipmentSystemModel();
                    break;

                // Set 3
                case EquipmentSystemType.Antler:
                    equipmentSystemModel = new AntlerEquipmentSystemModel();
                    break;
                case EquipmentSystemType.LeatherJacket:
                    equipmentSystemModel = new LeatherJacketEquipmentSystemModel();
                    break;
                case EquipmentSystemType.TwinNecklace:
                    equipmentSystemModel = new TwinNecklaceEquipmentSystemModel();
                    break;
                case EquipmentSystemType.InvisibleGloves:
                    equipmentSystemModel = new InvisibleGlovesEquipmentSystemModel();
                    break;
                case EquipmentSystemType.FluffyBoots:
                    equipmentSystemModel = new FluffyBootsEquipmentSystemModel();
                    break;

                // Set 4
                case EquipmentSystemType.OracleEye:
                    equipmentSystemModel = new OracleEyeEquipmentSystemModel();
                    break;
                case EquipmentSystemType.BoxingGloves:
                    equipmentSystemModel = new BoxingGlovesEquipmentSystemModel();
                    break;
                case EquipmentSystemType.RoyalCrown:
                    equipmentSystemModel = new RoyalCrownEquipmentSystemModel();
                    break;
                case EquipmentSystemType.SpikeVest:
                    equipmentSystemModel = new SpikeVestEquipmentSystemModel();
                    break;
                case EquipmentSystemType.GumBoots:
                    equipmentSystemModel = new GumBootsEquipmentSystemModel();
                    break;

                // SET 5
                case EquipmentSystemType.AstralHelm:
                    equipmentSystemModel = new AstralHelmEquipmentSystemModel();
                    break;
                case EquipmentSystemType.AstralCloak:
                    equipmentSystemModel = new AstralCloakEquipmentSystemModel();
                    break;
                case EquipmentSystemType.AstralLocket:
                    equipmentSystemModel = new AstralLocketEquipmentSystemModel();
                    break;
                case EquipmentSystemType.AstralGaunlet:
                    equipmentSystemModel = new AstralGauntletEquipmentSystemModel();
                    break;
                case EquipmentSystemType.AstralBoots:
                    equipmentSystemModel = new AstralBootsEquipmentSystemModel();
                    break;
            }

            if (equipmentSystemModel != null)
            {
                equipmentSystemModel.ModifierModelsDictionary = modifierModelsDictionary;
                equipmentSystemModel.Init(equipmentData);
                return equipmentSystemModel;
            }
            return null;
        }

        #endregion Class Methods
    }
}
using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class EquipmentSystemFactory
    {
        public static IEquipmentMechanicSystem GetEquipmentSystem(EquipmentSystemType equipmentSystemType)
        {
            switch (equipmentSystemType)
            {
                case EquipmentSystemType.None:
                    break;

                // SET 1
                case EquipmentSystemType.HatOfEndurance:
                    return new HatOfEnduranceEquipmentSystem();
                case EquipmentSystemType.CloakOfTime:
                    return new CloakOfTimeEquipmentSystem();
                case EquipmentSystemType.LocketOfWeakness:
                    return new LocketOfWeaknessEquipmentSystem();
                case EquipmentSystemType.GauntletsOfTheOverlord:
                    return new GauntletsOfTheOverlordEquipmentSystem();
                case EquipmentSystemType.BootsOfSwiftness:
                    return new BootsOfSwiftnessEquipmentSystem();

                // SET 2
                case EquipmentSystemType.FlowerWreath:
                    return new FlowerWreathEquipmentSystem();
                case EquipmentSystemType.MagicCloak:
                    return new MagicCloakEquipmentSystem();
                case EquipmentSystemType.PuppyGloves:
                    return new PuppyGlovesEquipmentSystem();
                case EquipmentSystemType.MonsterBoots:
                    return new MonsterBootsEquipmentSystem();

                // SET 3
                case EquipmentSystemType.Antler:
                    return new AntlerEquipmentSystem();
                case EquipmentSystemType.FluffyBoots:
                    return new FluffyBootsEquipmentSystem();
                case EquipmentSystemType.InvisibleGloves:
                    return new InvisibleGlovesEquipmentSystem();
                case EquipmentSystemType.LeatherJacket:
                    return new LeatherJacketEquipmentSystem();
                case EquipmentSystemType.TwinNecklace:
                    return new TwinNecklaceEquipmentSystem();

                // SET 4
                case EquipmentSystemType.OracleEye:
                    return new OracleEyeEquipmentSystem();
                case EquipmentSystemType.BoxingGloves:
                    return new BoxingGlovesEquipmentSystem();
                case EquipmentSystemType.RoyalCrown:
                    return new RoyalCrownEquipmentSystem();
                case EquipmentSystemType.SpikeVest:
                    return new SpikeVestEquipmentSystem();
                case EquipmentSystemType.GumBoots:
                    return new GumBootsEquipmentSystem();

                // SET 5
                case EquipmentSystemType.AstralHelm:
                    return new AstralHelmEquipmentSystem();
                case EquipmentSystemType.AstralCloak:
                    return new AstralCloakEquipmentSystem();
                case EquipmentSystemType.AstralLocket:
                    return new AstralLocketEquipmentSystem();
                case EquipmentSystemType.AstralGaunlet:
                    return new AstralGaunletEquipmentSystem();
                case EquipmentSystemType.AstralBoots:
                    return new AstralBootsEquipmentSystem();
                default:
                    break;
            }
            return null;
        }
    }
}
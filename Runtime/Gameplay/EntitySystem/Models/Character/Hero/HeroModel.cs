using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class HeroModel : CharacterModel, IEntityWeaponData
    {
        #region Properties

        public WeaponModel WeaponModel { get; private set; }
        public override EntityType EntityType => EntityType.Hero;

        #endregion Properties

        #region Class Methods

        public HeroModel(uint spawnedWaveIndex, uint heroUId, uint heroId, HeroLevelModel heroLevelModel, WeaponModel weaponModel)
            : base(spawnedWaveIndex, heroUId, heroId, heroLevelModel, null)
            => WeaponModel = weaponModel;

        #endregion Class Methods
    }

    public class HeroLevelModel : CharacterLevelModel
    {
        #region Class Methods

        public HeroLevelModel(uint level, int detectedPriority, CharacterStatsInfo characterStatsInfo)
            : base(level, detectedPriority, characterStatsInfo) { }

        #endregion Class Methods
    }
}
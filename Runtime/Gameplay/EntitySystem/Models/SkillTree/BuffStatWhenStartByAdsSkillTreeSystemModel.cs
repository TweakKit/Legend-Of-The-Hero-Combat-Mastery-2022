using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class BuffStatWhenStartByAdsSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.BuffStatWhenStartByAds;
        public BuffStatItem[] BuffStats { get; private set; }

        #endregion Properties

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as BuffStatWhenStartByAdsSkillTreeDataConfigItem;
            BuffStats = dataConfig.buffStats;
        }
    }
}
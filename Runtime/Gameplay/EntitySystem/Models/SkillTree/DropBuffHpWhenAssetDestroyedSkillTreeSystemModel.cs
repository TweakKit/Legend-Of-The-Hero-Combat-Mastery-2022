using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class DropBuffHpWhenAssetDestroyedSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.DropBuffHpWhenAssetDestroyed;
        public float RateDrop { get; private set; }
        public float BuffHealthValue { get; private set; }
        public float LifeTime { get; private set; }

        #endregion Properties

        #region Members

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as DropBuffHpWhenAssetDestroyedSkillTreeDataConfigItem;
            RateDrop = dataConfig.rateDrop;
            BuffHealthValue = dataConfig.buffHealthValue;
            LifeTime = dataConfig.lifeTime;
        }

        #endregion Members
    }

}
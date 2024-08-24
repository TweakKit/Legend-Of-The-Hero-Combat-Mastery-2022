using Runtime.ConfigModel;
using Runtime.Definition;
using Runtime.Localization;
using Runtime.Manager.Data;
using Runtime.Message;
using Runtime.Manager;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class BuffStatWhenStartByAdsSkillTreeSystem : SkillTreeSystem<BuffStatWhenStartByAdsSkillTreeSystemModel>
    {
        #region Members

        private static readonly string s_bonusTextColorHexan = "009c5b";
        private BuffStatItem _buffStat;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;
        private bool _displayedToast;
        private bool _startedByWatchedAds;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _buffStat = ownerModel.BuffStats[Random.Range(0, ownerModel.BuffStats.Length)];
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            _startedByWatchedAds = DataDispatcher.Instance.StartedByWatchedAds;
            DataDispatcher.Instance.StartedByWatchedAds = false;
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            if (_startedByWatchedAds)
            {
                message.HeroModel.BuffStat(_buffStat.statType, _buffStat.value, _buffStat.statModifyType);

                if (!_displayedToast)
                {
                    _displayedToast = true;
                    var description = "";
                    var statTypeText = LocalizationManager.GetLocalize(LocalizeTable.STATS, LocalizeKeys.GetEntityStatKey(_buffStat.statType));
                    if (_buffStat.statModifyType.IsPercentValue() || _buffStat.statType.IsPercentValue())
                        description = $"{statTypeText} <color=#{s_bonusTextColorHexan}>+{(int)(_buffStat.value * 100)}%</color>";
                    else
                        description = $"{statTypeText} <color=#{s_bonusTextColorHexan}>+{_buffStat.value}</color>";
                    ToastController.Instance.Show(description);
                }
            }
        }

        public override void Disable()
        {
            base.Disable();
            _heroSpawnedRegistry.Dispose();
        }

        #endregion Class Methods
    }
}
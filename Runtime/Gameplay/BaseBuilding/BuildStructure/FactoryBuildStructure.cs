using Cysharp.Threading.Tasks;
using Runtime.ConfigModel;
using Runtime.Definition;
using Runtime.Manager.Data;
using Runtime.Message;
using Runtime.Server.Models;
using Runtime.UI;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.BaseBuilder
{
    public class FactoryBuildStructure : BuildStructure
    {
        #region Members

        private const string PRODUCE_ANIM = "produce";
        [SerializeField]
        private StructureType _structureType;
        [SerializeField]
        private TimeRemainElement _timeRemainElement;
        [SerializeField]
        private GameObject _notification;

        private Registry<DataUpdatedMessage> _dataUpdatedRegistry;
        private FactoryProductionConfigItem _config;
        private FactoryData _data;
        private bool _initialized;

        #endregion Members

        #region API Methods

        private void OnEnable()
        {
            if (_initialized)
                UpdateUI();

            _dataUpdatedRegistry = Messenger.Subscriber().Subscribe<DataUpdatedMessage>(OnDataUpdated);
        }

        private void OnDisable()
        {
            _timeRemainElement.Dispose();
            _dataUpdatedRegistry.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        public override async UniTask Initialize(BuildStructureModel buildStructureModel)
        {
            await base.Initialize(buildStructureModel);
            var factoryProductionConfig = DataManager.Config.GetFactoryProductionInfo();
            _config = factoryProductionConfig.GetDataConfig((int)_structureType);
            _initialized = true;
            UpdateUI();
        }

        private void UpdateUI()
        {
            _data = DataManager.Server.GetFactoryData((int)_structureType);
            var factoryProduceItems = DataManager.Server.GetFactoryProduceItems(StructureType.JuiceFactory, _config);
            var offset = (factoryProduceItems.CurrentProduceItem == null ? 0 : 1) + factoryProduceItems.ProducedItems.Count;

            _timeRemainElement.Dispose();
            _notification.SetActive(factoryProduceItems.ProducedItems.Count > 0);

            if (factoryProduceItems.CurrentProduceItem != null)
            {
                var config = _config.GetOutputConfigItem(factoryProduceItems.CurrentProduceItem.ProductId);
                _timeRemainElement.ExtraSetup(null);
                _timeRemainElement.SetTimeRemain(factoryProduceItems.CurrentProduceItem.ProduceStartTime + config.produceTime, onFinishTimeRemain: OnFinishProduceTime);
                PlayAnim(PRODUCE_ANIM);
            }
            else PlayAnim(IDLE_ANIM);
        }

        private void OnFinishProduceTime()
        {
            UpdateUI();
        }

        private void OnDataUpdated(DataUpdatedMessage message)
        {
            if (message.DataUpdatedType == _structureType.GetDataUpdatedTypeByStructureType())
                UpdateUI();
        }

        #endregion Class Methods
    }
}

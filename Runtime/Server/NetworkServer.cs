using System;
using Runtime.Message;
using Runtime.Authentication;
using Runtime.Core.Singleton;
using Runtime.Server.CallbackData;
using Core.Foundation.PubSub;
using Runtime.Definition;
using Runtime.Server.Handlers;

namespace Runtime.Server
{
    public class NetworkServer : PersistentMonoSingleton<NetworkServer>
    {
        #region Members

        private Registry<GameStateChangedMessage> _gameStateChangedRegistry;

        #endregion Members

        #region Properties

        #region Listeners
        private static HandShakeHandler HandShakeHandler { get; set; }
        private static NotifyNewOrderSlotUnlockHandler NotifyNewOrderSlotUnlockHandler { get; set; }
        private static NotifyFromServerHandler NotifyFromServerHandler { get; set; }
        private static NotifyServerMaintenanceHandler NotifyServerMaintenanceHandler { get; set; }

        #endregion Listeners

        public bool IsProcessing { get; set; }
        private static AuthenticationHandler AuthenticationHandler { get; set; }
        private static PlayerHandler PlayerHandler { get; set; }
        private static CompleteTutorialStepHandler CompleteTutorialStepHandler { get; set; }
        private static CampaignStageStartHandler CampaignStageStartHandler { get; set; }
        private static CampaignStageEndHandler CampaignStageEndHandler { get; set; }
        private static CampaignClaimWorldStarProgressHandler CampaignClaimWorldStarProgressHandler { get; set; }
        private static CampaignReviveHandler CampaignReviveHandler { get; set; }
        private static CampaignStageClaimAdsRewardHandler CampaignStageClaimAdsRewardHandler { get; set; }
        private static SurvivalStageStartHandler SurvivalStageStartHandler { get; set; }
        private static SurvivalSaveProgressHandler SurvivalSaveProgressHandler { get; set; }
        private static SurvivalStageEndHandler SurvivalStageEndHandler { get; set; }
        private static GooglePaymentPurchaseHandler GooglePaymentPurchaseHandler { get; set; }
        private static GooglePaymentPurchaseCustomBundleHandler GooglePaymentPurchaseCustomBundleHandler { get; set; }
        private static ClaimFirstPurchaseRewardsHandler ClaimFirstPurchaseRewardsHandler { get; set; }
        private static ClaimMailRewardsHandler ClaimMailRewardsHandler { get; set; }
        private static TriggerHeroAwakenHandler TriggerHeroAwakenHandler { get; set; }
        private static EquipmentSaveHandler EquipmentSaveHandler { get; set; }
        private static LevelUpEquipmentHandler LevelUpEquipmentHandler { get; set; }
        private static LevelDownEquipmentHandler LevelDownEquipmentHandler { get; set; }
        private static DowngradeEquipmentHandler DowngradeEquipmentHandler { get; set; }
        private static MergeEquipmentHandler MergeEquipmentHandler { get; set; }
        private static QuickMergeEquipmentHandler QuickMergeEquipmentHandler { get; set; }
        private static BuildNewStructureHandler BuildNewStructureHandler { get; set; }
        private static BuildCompleteStructureHandler BuildCompleteStructureHandler { get; set; }
        private static EditStructureHandler EditStructureHandler { get; set; }
        private static SpeedUpConstructionTimeHandler SpeedUpConstructionTimeHandler { get; set; }
        private static CancelConstructionHandler CancelConstructionHandler { get; set; }
        private static UpgradeStructureHandler UpgradeStructureHandler { get; set; }
        private static GachaHandler GachaHandler { get; set; }
        private static FirstTimeGachaHandler FirstTimeGachaHandler { get; set; }
        private static SurveySubmitHandler SurveySubmitHandler { get; set; }
        private static UnlockSkillTreeHandler UnlockSkillTreeHandler { get; set; }
        private static GetMailDataHandler GetMailDataHandler { get; set; }

        private static PurchaseGachaShopItemHandler PurchaseGachaShopItemHandler { get; set; }
        private static PurchaseResourceShopItemHandler PurchaseResourceShopItemHandler { get; set; }
        private static PurchaseSurvivalShopItemHandler PurchaseSurvivalShopItemHandler { get; set; }
        private static ClaimCampaignPassHandler ClaimCampaignPassHandler { get; set; }

        private static UnlockFactorySlotHandler UnlockFactorySlotHandler { get; set; }
        private static BeginFactoryProduceHandler BeginFactoryProduceHandler { get; set; }
        private static ClaimFactoryProductHandler ClaimFactoryProductHandler { get; set; }
        private static SkipTimeFactoryProduceHandler SkipTimeFactoryProduceHandler { get; set; }

        private static SendOrderHandler SendOrderHandler { get; set; }
        private static ClaimOrderDailyMilestoneRewardsHandler ClaimOrderDailyMilestoneRewardsHandler { get; set; }
        private static RefreshOrderHandler RefreshOrderHandler { get; set; }
        private static SkipTimeRefreshOrderHandler SkipTimeRefreshOrderHandler { get; set; }
        private static BindingAccountHandler BindingAccountHandler { get; set; }
        private static SpendConsumableSelectResourceHandler SpendConsumableSelectResourceHandler { get; set; }
        private static ClaimDailyQuestHandler ClaimDailyQuestHandler { get; set; }
        private static ClaimDailyQuestLandMarkHandler ClaimDailyQuestLandMarkHandler { get; set; }
        private static ClaimMainQuestHandler ClaimMainQuestHandler { get; set; }
        private static ClaimDailyFortuneHandler ClaimDailyFortuneHandler { get; set; }


#if ENABLE_CHEAT
        private static CheatCampaignStageHandler CheatCampaignStageHandler { get; set; }
        private static CheatAddResourceHandler CheatAddResourceHandler { get; set; }
        private static CheatEquipmentHandler CheatEquipmentHandler { get; set; }
        private static CheatGachaHandler CheatGachaHandler { get; set; }
        private static CheatResetPlayerDataHandler CheatResetPlayerDataHandler { get; set; }
        private static CheatFakeMailHandler CheatFakeMailHandler { get; set; }
        private static CheatPurchasePackHandler CheatPurchasePackHandler { get; set; }
        private static CheatPurchasePackCustomBundleHandler CheatPurchasePackCustomBundleHandler { get; set; }
#endif

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _gameStateChangedRegistry = Messenger.Subscriber().Subscribe<GameStateChangedMessage>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            _gameStateChangedRegistry.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        public static void Login(AuthenticationInfo authenticationInfo, Action<AuthenticationResult> resultCallback)
            => AuthenticationHandler.Login(authenticationInfo, resultCallback);

        public static void ClaimWorldStarProgress(ClaimWorldStarProgressRequestData requestData, Action<ClaimWorldStarProgressCallbackData> callback)
            => CampaignClaimWorldStarProgressHandler.Request(requestData, callback);

        public static void GetPlayerData(GetPlayerDataRequestData requestData, Action<GetPlayerDataCallbackData> callback)
            => PlayerHandler.Request(requestData, callback);

        public static void CompleteStepTutorial(CompleteTutorialStepRequestData requestData, Action<CompleteTutorialStepCallbackData> callback)
            => CompleteTutorialStepHandler.Request(requestData, callback);

        public static void SendCampaignStageStartRequest(CampaignStageStartRequestData requestData, Action<CampaignStageStartCallbackData> callback)
        {
            if (!Instance.IsProcessing)
                CampaignStageStartHandler.Request(requestData, callback);
        }

        public static void SendCampaignStageEndRequest(CampaignStageEndRequestData requestData, Action<CampaignStageEndCallbackData> callback)
            => CampaignStageEndHandler.Request(requestData, callback);

        public static void SendCampaignRevive(CampaignReviveRequestData requestData, Action<CampaignReviveCallbackData> callback)
            => CampaignReviveHandler.Request(requestData, callback);

        public static void SendCampaignStageClaimAdsReward(CampaignStageClaimAdsRewardRequestData requestData, Action<CampaignStageClaimAdsRewardCallbackData> callback)
            => CampaignStageClaimAdsRewardHandler.Request(requestData, callback);

        public static void SendSurvivalStageStartRequest(SurvivalStageStartRequestData requestData, Action<SurvivalStageStartCallbackData> callback)
            => SurvivalStageStartHandler.Request(requestData, callback);

        public static void SaveSurvivalProgressRequest(SurvivalSaveProgressRequestData requestData, Action<SurvivalSaveProgressCallbackData> callback)
            => SurvivalSaveProgressHandler.Request(requestData, callback);

        public static void SendSurvivalStageEndRequest(SurvivalStageEndRequestData requestData, Action<SurvivalStageEndCallbackData> callback)
            => SurvivalStageEndHandler.Request(requestData, callback);

        public static void SendGooglePaymentPurchase(GooglePaymentPurchaseRequestData requestData, Action<GooglePaymentPurchaseCallbackData> callback)
            => GooglePaymentPurchaseHandler.Request(requestData, callback);

        public static void SendGooglePaymentPurchaseCustomBundle(GooglePaymentPurchaseCustomBundleRequestData requestData, Action<GooglePaymentPurchaseCallbackData> callback)
            => GooglePaymentPurchaseCustomBundleHandler.Request(requestData, callback);

        public static void SendClaimFirstPurchaseRewards(ClaimFirstPurchaseRewardsRequestData requestData, Action<ClaimFirstPurchaseRewardsCallbackData> callback)
            => ClaimFirstPurchaseRewardsHandler.Request(requestData, callback);

        public static void SendClaimMailRewards(ClaimMailRewardsRequestData requestData, Action<ClaimMailRewardsCallbackData> callback)
            => ClaimMailRewardsHandler.Request(requestData, callback);

        public static void SaveEquipment(EquipmentSaveRequestData requestData, Action<EquipmentSaveCallbackData> callback)
            => EquipmentSaveHandler.Request(requestData, callback);
        public static void MergeEquipment(MergeEquipmentRequestData requestData, Action<MergeEquipmentCallbackData> callback)
            => MergeEquipmentHandler.Request(requestData, callback);
        public static void QuickMergeEquipment(Action<QuickMergeEquipmentCallbackData> callback)
            => QuickMergeEquipmentHandler.Request(new EmptyRequestData(), callback);

        public static void LevelUpEquipment(LevelUpEquipmentRequestData requestData, Action<LevelUpEquipmentCallbackData> callback)
            => LevelUpEquipmentHandler.Request(requestData, callback);

        public static void LevelDownEquipment(LevelDownEquipmentRequestData requestData, Action<LevelDownEquipmentCallbackData> callback)
            => LevelDownEquipmentHandler.Request(requestData, callback);

        public static void DowngradeEquipment(DowngradeEquipmentRequestData requestData, Action<DowngradeEquipmentCallbackData> callback)
            => DowngradeEquipmentHandler.Request(requestData, callback);

        public static void BuildNewStructure(BuildNewStructureRequestData requestData, Action<BuildNewStructureCallbackData> callback)
            => BuildNewStructureHandler.Request(requestData, callback);

        public static void BuildCompleteStructure(BuildCompleteStructureRequestData requestData, Action<BuildCompleteStructureCallbackData> callback)
            => BuildCompleteStructureHandler.Request(requestData, callback);

        public static void EditStructure(EditStructureRequestData requestData, Action<EditStructureCallbackData> callback)
            => EditStructureHandler.Request(requestData, callback);

        public static void SpeedUpConstructionTime(SpeedUpConstructionTimeRequestData requestData, Action<SpeedUpConstructionTimeCallbackData> callback)
           => SpeedUpConstructionTimeHandler.Request(requestData, callback);

        public static void CancelConstruction(CancelConstructionRequestData requestData, Action<CancelConstructionCallbackData> callback)
            => CancelConstructionHandler.Request(requestData, callback);

        public static void UpgradeStructure(UpgradeStructureRequestData requestData, Action<UpgradeStructureCallbackData> callback)
            => UpgradeStructureHandler.Request(requestData, callback);

        public static void TriggerHeroAwaken(TriggerHeroAwakenRequestData requestData, Action<TriggerHeroAwakenCallbackData> callback)
            => TriggerHeroAwakenHandler.Request(requestData, callback);

        public static void PurchaseGachaShopItem(PurchaseGachaShopItemRequestData requestData, Action<PurchaseGachaShopItemCallbackData> callback)
            => PurchaseGachaShopItemHandler.Request(requestData, callback);

        public static void Gacha(GachaRequestData requestData, Action<GachaCallbackData> callback)
            => GachaHandler.Request(requestData, callback);

        public static void GachaForFirstTime(FirstGachaRequestData requestData, Action<GachaCallbackData> callback)
            => FirstTimeGachaHandler.Request(requestData, callback);

        public static void SubmitSurvey(SurveySubmitRequestData requestData, Action<ResourceRewardCallbackData> callback)
            => SurveySubmitHandler.Request(requestData, callback);

        public static void UnlockSkillTree(UnlockSkillTreeRequestData requestData, Action<ResourceRewardCallbackData> callback)
           => UnlockSkillTreeHandler.Request(requestData, callback);

        public static void GetMailData(GetMailDataRequestData requestData, Action<GetMailDataCallbackData> callback)
           => GetMailDataHandler.Request(requestData, callback);

        public static void UnlockFactorySlot(UnlockFactorySlotRequestData requestData, Action<UnlockFactorySlotCallbackData> callback)
            => UnlockFactorySlotHandler.Request(requestData, callback);

        public static void BeginFactoryProduce(BeginFactoryProduceRequestData requestData, Action<BeginFactoryProduceCallbackData> callback)
            => BeginFactoryProduceHandler.Request(requestData, callback);

        public static void ClaimFactoryProduct(ClaimFactoryProductRequestData requestData, Action<ClaimFactoryProductCallbackData> callback)
            => ClaimFactoryProductHandler.Request(requestData, callback);

        public static void SkipTimeFactoryProduce(SkipTimeFactoryProduceRequestData requestData, Action<SkipTimeFactoryProduceCallbackData> callback)
            => SkipTimeFactoryProduceHandler.Request(requestData, callback);

        public static void SendOrder(SendOrderRequestData requestData, Action<SendOrderCallbackData> callback)
            => SendOrderHandler.Request(requestData, callback);

        public static void ClaimOrderDailyMilestoneRewards(ClaimOrderDailyMilestoneRewardsRequestData requestData, Action<ClaimOrderDailyMilestoneRewardsCallbackData> callback)
            => ClaimOrderDailyMilestoneRewardsHandler.Request(requestData, callback);

        public static void RefreshOrder(RefreshOrderRequestData requestData, Action<RefreshOrderCallbackData> callback)
            => RefreshOrderHandler.Request(requestData, callback);

        public static void SkipTimeRefreshOrder(SKipTimeRefreshOrderRequestData requestData, Action<SkipTimeRefreshOrderCallbackData> callback)
            => SkipTimeRefreshOrderHandler.Request(requestData, callback);

        public static void BindingAccount(BindingAccountRequestData requestData, Action<BindingAccountCallbackData> callback)
            => BindingAccountHandler.Request(requestData, callback);

        public static void PurchaseResourceShopItem(PurchaseResourceShopItemRequestData requestData, Action<PurchaseResourceShopItemCallbackData> callback)
            => PurchaseResourceShopItemHandler.Request(requestData, callback);

        public static void PurchaseSurvivalShopItem(PurchaseSurvivalShopItemRequestData requestData, Action<PurchaseSurvivalShopItemCallbackData> callback)
            => PurchaseSurvivalShopItemHandler.Request(requestData, callback);

        public static void ClaimCampaignPass(ClaimCampaignPassRequestData requestData, Action<ClaimCampaignPassCallbackData> callback)
            => ClaimCampaignPassHandler.Request(requestData, callback);

        public static void SpendConsumableSelectResource(SpendConsumableSelectResourceRequestData requestData, Action<RewardsCallbackData> callback)
            => SpendConsumableSelectResourceHandler.Request(requestData, callback);

        public static void ClaimDailyQuest(ClaimDailyQuestRequestData requestData, Action<RewardsCallbackData> callback)
            => ClaimDailyQuestHandler.Request(requestData, callback);

        public static void ClaimDailyQuestLandMark(ClaimDailyQuestLandMarkRequestData requestData, Action<RewardsCallbackData> callback)
            => ClaimDailyQuestLandMarkHandler.Request(requestData, callback);

        public static void ClaimMainQuest(ClaimMainQuestRequestData requestData, Action<RewardsCallbackData> callback)
            => ClaimMainQuestHandler.Request(requestData, callback);

        public static void ClaimDailyFortune(ClaimDailyFortuneRequestData requestData, Action<ClaimDailyFortuneCallbackData> callback)
            => ClaimDailyFortuneHandler.Request(requestData, callback);

#if ENABLE_CHEAT
        public static void CheatAddResource(CheatResourceRequestData requestData, Action<CheatResourceCallbackData> callback)
            => CheatAddResourceHandler.Request(requestData, callback);

        public static void CheatEquipment(CheatEquipmentRequestData requestData, Action<CheatEquipmentCallbackData> callback)
            => CheatEquipmentHandler.Request(requestData, callback);

        public static void CheatGacha(CheatGachaRequestData requestData, Action<CheatGachaCallbackData> callback)
            => CheatGachaHandler.Request(requestData, callback);

        public static void CheatCampaign(CheatCampaignStageRequestData requestData, Action<CheatCampaignStageCallbackData> callback)
            => CheatCampaignStageHandler.Request(requestData, callback);

        public static void CheatResetPlayerData(CheatResetPlayerDataRequestData requestData, Action<EmptyCallbackData> callback)
            => CheatResetPlayerDataHandler.Request(requestData, callback);

        public static void CheatFakeMail(EmptyRequestData requestData, Action<EmptyCallbackData> callback)
            => CheatFakeMailHandler.Request(requestData, callback);

        public static void CheatPurchasePack(CheatPurchasePackRequestData requestData, Action<GooglePaymentPurchaseCallbackData> callback)
            => CheatPurchasePackHandler.Request(requestData, callback);

        public static void CheatPurchaseCustomBundlePack(CheatPurchasePackCustomBundleRequestData requestData, Action<GooglePaymentPurchaseCallbackData> callback)
            => CheatPurchasePackCustomBundleHandler.Request(requestData, callback);
#endif

        private void OnGameStateChanged(GameStateChangedMessage gameStateChangedMessage)
        {
            if (gameStateChangedMessage.GameStateType == GameStateEventType.InitNetwork)
            {
                ConnectorService connectorService = ConnectorService.Instance;
                Init(connectorService.Connector);
            }
        }

        private void Init(Connector connector)
        {
            IsProcessing = false;
            HandShakeHandler = new HandShakeHandler(connector);
            NotifyNewOrderSlotUnlockHandler = new NotifyNewOrderSlotUnlockHandler(connector);
            NotifyFromServerHandler = new NotifyFromServerHandler(connector);
            NotifyServerMaintenanceHandler = new NotifyServerMaintenanceHandler(connector);

            AuthenticationHandler = new AuthenticationHandler(connector);
            PlayerHandler = new PlayerHandler(connector);
            CompleteTutorialStepHandler = new CompleteTutorialStepHandler(connector);
            CampaignStageStartHandler = new CampaignStageStartHandler(connector);
            CampaignStageEndHandler = new CampaignStageEndHandler(connector);
            CampaignClaimWorldStarProgressHandler = new CampaignClaimWorldStarProgressHandler(connector);
            CampaignReviveHandler = new CampaignReviveHandler(connector);
            CampaignStageClaimAdsRewardHandler = new CampaignStageClaimAdsRewardHandler(connector);
            ClaimFirstPurchaseRewardsHandler = new ClaimFirstPurchaseRewardsHandler(connector);
            SurvivalStageStartHandler = new SurvivalStageStartHandler(connector);
            SurvivalSaveProgressHandler = new SurvivalSaveProgressHandler(connector);
            SurvivalStageEndHandler = new SurvivalStageEndHandler(connector);
            ClaimMailRewardsHandler = new ClaimMailRewardsHandler(connector);
            TriggerHeroAwakenHandler = new TriggerHeroAwakenHandler(connector);
            EquipmentSaveHandler = new EquipmentSaveHandler(connector);
            LevelUpEquipmentHandler = new LevelUpEquipmentHandler(connector);
            LevelDownEquipmentHandler = new LevelDownEquipmentHandler(connector);
            DowngradeEquipmentHandler = new DowngradeEquipmentHandler(connector);
            MergeEquipmentHandler = new MergeEquipmentHandler(connector);
            QuickMergeEquipmentHandler = new QuickMergeEquipmentHandler(connector);
            BuildNewStructureHandler = new BuildNewStructureHandler(connector);
            BuildCompleteStructureHandler = new BuildCompleteStructureHandler(connector);
            EditStructureHandler = new EditStructureHandler(connector);
            SpeedUpConstructionTimeHandler = new SpeedUpConstructionTimeHandler(connector);
            CancelConstructionHandler = new CancelConstructionHandler(connector);
            UpgradeStructureHandler = new UpgradeStructureHandler(connector);
            GachaHandler = new GachaHandler(connector);
            FirstTimeGachaHandler = new FirstTimeGachaHandler(connector);
            SurveySubmitHandler = new SurveySubmitHandler(connector);
            UnlockSkillTreeHandler = new UnlockSkillTreeHandler(connector);
            GetMailDataHandler = new GetMailDataHandler(connector);

            GooglePaymentPurchaseHandler = new GooglePaymentPurchaseHandler(connector);
            GooglePaymentPurchaseCustomBundleHandler = new GooglePaymentPurchaseCustomBundleHandler(connector);
            PurchaseGachaShopItemHandler = new PurchaseGachaShopItemHandler(connector);
            PurchaseResourceShopItemHandler = new PurchaseResourceShopItemHandler(connector);
            PurchaseSurvivalShopItemHandler = new PurchaseSurvivalShopItemHandler(connector);
            ClaimCampaignPassHandler = new ClaimCampaignPassHandler(connector);

            UnlockFactorySlotHandler = new UnlockFactorySlotHandler(connector);
            BeginFactoryProduceHandler = new BeginFactoryProduceHandler(connector);
            ClaimFactoryProductHandler = new ClaimFactoryProductHandler(connector);
            SkipTimeFactoryProduceHandler = new SkipTimeFactoryProduceHandler(connector);

            SendOrderHandler = new SendOrderHandler(connector);
            ClaimOrderDailyMilestoneRewardsHandler = new ClaimOrderDailyMilestoneRewardsHandler(connector);
            RefreshOrderHandler = new RefreshOrderHandler(connector);
            SkipTimeRefreshOrderHandler = new SkipTimeRefreshOrderHandler(connector);
            BindingAccountHandler = new BindingAccountHandler(connector);
            SpendConsumableSelectResourceHandler = new SpendConsumableSelectResourceHandler(connector);
            ClaimDailyQuestLandMarkHandler = new ClaimDailyQuestLandMarkHandler(connector);
            ClaimDailyQuestHandler = new ClaimDailyQuestHandler(connector);
            ClaimMainQuestHandler = new ClaimMainQuestHandler(connector);
            ClaimDailyFortuneHandler = new ClaimDailyFortuneHandler(connector);

#if ENABLE_CHEAT
            CheatAddResourceHandler = new CheatAddResourceHandler(connector);
            CheatEquipmentHandler = new CheatEquipmentHandler(connector);
            CheatGachaHandler = new CheatGachaHandler(connector);
            CheatCampaignStageHandler = new CheatCampaignStageHandler(connector);
            CheatResetPlayerDataHandler = new CheatResetPlayerDataHandler(connector);
            CheatFakeMailHandler = new CheatFakeMailHandler(connector);
            CheatPurchasePackHandler = new CheatPurchasePackHandler(connector);
            CheatPurchasePackCustomBundleHandler = new CheatPurchasePackCustomBundleHandler(connector);
#endif
        }

        #endregion API Methods
    }
}
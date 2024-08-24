using System;
using System.Threading;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using Runtime.Server;
using Runtime.Server.CallbackData;
using Runtime.UI;
using Runtime.Manager.Data;
using Runtime.Server.Models;
using Runtime.Definition;
using Runtime.Utilities;
using Runtime.Manager;
using Runtime.Tracking;
using Runtime.Extensions;
using Runtime.Core.Mics;
using Runtime.FeatureSystem;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.BaseBuilder
{
    public class BuildStructure : MonoBehaviour, IBuildStructure, IUnlockable, IClickable
    {
        #region Members

        protected const string GRAPHICS = "graphics";
        protected const string LOCKED_ANIM = "locked";
        protected const string IDLE_ANIM = "idle";

        [SerializeField]
        protected BuildStructureHUD buildStructureHUD;
        [SerializeField]
        protected Animator[] animators;

        protected BuildStructureModel buildStructureModel;
        protected Transform graphic;
        protected DateTime buildStartDateTime;
        protected TimeSpan totalBuildTimeTimespan;
        protected CancellationTokenSource cancellationTokenSource;
        protected bool hasFinishedBuilding;
        protected UnlockFeature unlockFeature;

        #endregion Members

        #region Properties

        public BuildStructureModel BuildStructureModel => buildStructureModel;
        public Transform RootTransform => transform;
        public bool IsUnlocked => unlockFeature != null ? unlockFeature.IsUnlocked : true;
        public string RequiredUnlockDescription => unlockFeature != null ? unlockFeature.UnlockDescription : "";

        #endregion Properties

        #region API Methods

        private void OnDestroy()
            => cancellationTokenSource?.Cancel();

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (buildStructureHUD == null)
                buildStructureHUD = gameObject.GetComponentInChildren<BuildStructureHUD>();
            else
                Debug.LogWarning("Add HUD to display the structure's info!");
        }
#endif

        #endregion API Methods

        #region Class Methods

        public virtual UniTask Initialize(BuildStructureModel buildStructureModel)
        {
            graphic = RootTransform.Find(GRAPHICS);
            unlockFeature = gameObject.GetComponentInChildren<UnlockFeature>();
            this.buildStructureModel = buildStructureModel;
            this.buildStructureModel.OnFlipped += Flip;
            if (unlockFeature != null)
            {
                var buildStructureType = (StructureType)buildStructureModel.id;
                var structureUnlockedData = FeatureUnlockChecker.GetStructureUnlockedData(buildStructureType);
                unlockFeature.Init(structureUnlockedData.isUnlocked, structureUnlockedData.unlockedRequimentDescription, OnUpdateLockStatusAnimation);
            }
            else OnUpdateLockStatusAnimation(true);
            Flip(buildStructureModel.IsFlipped);
            buildStructureHUD.Init(buildStructureModel, ConfirmBuildCompleteAction);
            return UniTask.CompletedTask;
        }

        public void Click()
            => OpenFeature();

        public void OpenFeature()
        {
#if TRACKING
            FirebaseManager.Instance.TrackButtonClick(ButtonSourceType.Building.ToTrackingFormat(),
                                                      ((StructureType)buildStructureModel.id).ToTrackingFormat(),
                                                      buildStructureModel.id);
#endif

            transform.DOScale(Vector3.one * 0.9f, 0).OnComplete(() => {
                transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
            });

            if (IsUnlocked)
            {
                var buildStructureType = (StructureType)buildStructureModel.id;
                switch (buildStructureType)
                {
                    case StructureType.Headquarter:
                        var headquarterScreenWindowOptions = new WindowOptions(ScreenIds.HEADQUARTER_SCREEN, true);
                        ScreenNavigator.Instance.LoadScreen(headquarterScreenWindowOptions, new EmptyScreenData()).Forget();
                        break;

                    case StructureType.Gacha:
                        var gachaScreenWindowOptions = new WindowOptions(ScreenIds.GACHA_SCREEN, true);
                        ScreenNavigator.Instance.LoadScreen(gachaScreenWindowOptions, new EmptyScreenData()).Forget();
                        break;

                    case StructureType.Storage:
                        var storageScreenWindowOptions = new WindowOptions(ScreenIds.STORAGE_SCREEN, true);
                        ScreenNavigator.Instance.LoadScreen(storageScreenWindowOptions, new EmptyScreenData()).Forget();
                        break;

                    case StructureType.Order:
                        var orderScreenWindowOptions = new WindowOptions(ScreenIds.ORDER_SCREEN, true);
                        ScreenNavigator.Instance.LoadScreen(orderScreenWindowOptions, new EmptyScreenData()).Forget();
                        break;

                    case StructureType.JuiceFactory:
                        var juiceFactoryModalWindowOptions = new WindowOptions(ModalIds.JUICE_FACTORY, true);
                        ScreenNavigator.Instance.LoadModal(juiceFactoryModalWindowOptions, new EmptyModalData()).Forget();
                        break;
                }
            }
            else ToastController.Instance.Show(RequiredUnlockDescription);
        }

        public virtual void CheckBuildStatus(ConstructionData constructionData)
        {
            if (constructionData != null)
            {
                if (constructionData.ConstructionType == ConstructionType.Building || constructionData.ConstructionType == ConstructionType.Upgrading)
                {
                    totalBuildTimeTimespan = TimeSpan.FromSeconds(buildStructureModel.id.GetBuildTime(buildStructureModel.currentLevel + 1));
                    buildStartDateTime = GetBuildStartDateTime(constructionData.StartTimeTotalSecondsFromOriginTime);
                    if (!HasFinishedBuildTimeButNotCompleted())
                    {
                        buildStructureHUD.PrepareBuild(totalBuildTimeTimespan);
                        cancellationTokenSource?.Cancel();
                        cancellationTokenSource = new CancellationTokenSource();
                        StartBuildProcessAsync(cancellationTokenSource.Token).Forget();
                    }
                    else
                    {
                        hasFinishedBuilding = true;
                        buildStructureHUD.CompleteBuild();
                    }
                }
                else buildStructureHUD.Hide();
            }
        }

        public virtual void UpdateSkipBuildTime(ConstructionData constructionData)
            => buildStartDateTime = GetBuildStartDateTime(constructionData.StartTimeTotalSecondsFromOriginTime);

        public void CancelConstruction()
        {
            cancellationTokenSource?.Cancel();
            buildStructureHUD.Hide();
        }

        public virtual void Select()
        {
            if (buildStructureModel.currentState == StructureState.Building || buildStructureModel.currentState == StructureState.Upgrading)
                buildStructureHUD.SelectToSkipBuildTime();
            else if (buildStructureModel.currentState == StructureState.Built)
                buildStructureHUD.SelectToUpgrade();
        }

        public virtual void Unselect()
            => buildStructureHUD.Unselect();

        public void HandleEnterEditMode(bool isEnteredEditMode)
        {
            if (isEnteredEditMode)
                buildStructureHUD.Hide();
            else
                buildStructureHUD.Show(hasFinishedBuilding);
        }

        protected void PlayAnim(string anim)
        {
            foreach (var animator in animators)
                animator.Play(anim);
        }

        private void OnUpdateLockStatusAnimation(bool isUnlocked)
        {
            if (!isUnlocked)
                PlayAnim(LOCKED_ANIM);
            else
                PlayAnim(IDLE_ANIM);
        }

        private void Flip(bool flipped)
        {
            if (flipped == graphic.localScale.x > 0)
                graphic.localScale = new Vector2(-graphic.localScale.x, graphic.localScale.y);
        }

        private async UniTask StartBuildProcessAsync(CancellationToken token)
        {
            hasFinishedBuilding = false;
            while (!hasFinishedBuilding)
            {
                await UniTask.Yield(token);
                TimeSpan builtTimeSpan = GameTime.ServerUtcNow - buildStartDateTime;
                TimeSpan leftBuildTimespan = totalBuildTimeTimespan - builtTimeSpan;
                buildStructureHUD.UpdateBuild(leftBuildTimespan, totalBuildTimeTimespan);
                hasFinishedBuilding = leftBuildTimespan.TotalSeconds <= 0;
            }
        }

        private DateTime GetBuildStartDateTime(long startTimeTotalSecondsFromOriginTime)
        {
            var returnedBuildStartDateTime = GameTime.ConverToDateTime(startTimeTotalSecondsFromOriginTime);
            return returnedBuildStartDateTime;
        }

        private bool HasFinishedBuildTimeButNotCompleted()
        {
            TimeSpan builtTimeSpan = GameTime.ServerUtcNow - buildStartDateTime;
            TimeSpan leftBuildTimespan = totalBuildTimeTimespan - builtTimeSpan;
            return leftBuildTimespan.TotalSeconds <= 0;
        }

        private void ConfirmBuildCompleteAction()
        {
            var requestData = new BuildCompleteStructureRequestData(buildStructureModel.uid);
            NetworkServer.BuildCompleteStructure(requestData, (callbackData) => {
                if (callbackData.ResultCode == LogicCode.SUCCESS)
                {
                    ToastController.Instance.Show("Build Completed!");
                    DataManager.Server.UpdateStructure(callbackData.Structure);
                    BuildingManager.Instance.UpdateStructure(callbackData.Structure, false);
                    buildStructureHUD.Hide();
                }
                else if (callbackData.ResultCode == LogicCode.BUILD_TIME_NOT_ENOUGH)
                    ToastController.Instance.Show("Build time is not enough!");
                else
                    ToastController.Instance.Show("Error happened!");
            });
        }

        #endregion Class Methods
    }
}
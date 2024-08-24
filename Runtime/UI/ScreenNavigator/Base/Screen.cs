using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.Manager;
using Runtime.Audio;
using UnityScreenNavigator.Runtime.Core.Sheets;
using Cysharp.Threading.Tasks;
using ScreenNavigatorScreen = UnityScreenNavigator.Runtime.Core.Screens.Screen;

namespace Runtime.UI
{
    public abstract class SheetContainerScreen<T> : Screen<T> where T : ScreenData
    {
        #region Members

        [SerializeField]
        protected SheetContainer container;
        protected int[] sheetIds;
        protected bool isLoading;

        #endregion Members

        #region Properties

        protected abstract Dictionary<int, string> SheetDictionary { get; }

        #endregion Properties

        public override async UniTask Initialize(T screenData)
        {
            isLoading = false;
            sheetIds = new int[SheetDictionary.Count];
            ResetSheetIds();
            await UniTask.CompletedTask;
        }

        public override UniTask Cleanup()
        {
            ResetSheetIds();
            return base.Cleanup();
        }

        protected void ResetSheetIds()
        {
            for (int i = 0; i < sheetIds.Length; i++)
                sheetIds[i] = -1;
        }

        public async UniTask LoadSheetByIndex(int index, bool isOn, params object[] args)
        {
            if (isOn && !isLoading)
            {
                if (container.ActiveSheetId != sheetIds[index])
                {
                    isLoading = true;
                    if (sheetIds[index] == -1)
                    {
                        var sheetName = SheetDictionary[index];
                        await container.Register(new SheetOptions(sheetName, (sheetId, sheet) => sheetIds[index] = sheetId), args);
                    }

                    await container.Show(sheetIds[index], false);
                    isLoading = false;
                }
            }
        }
    }

    public abstract class Screen<T> : ScreenNavigatorScreen where T : ScreenData
    {
        #region Members

        protected T ownerScreenData;

        #endregion Members

        #region Class Methods

        public override async UniTask InitializeInternal(object arg)
        {
            var screenData = arg as T;
            if (screenData != null)
                await Initialize(screenData);
        }

        public virtual async UniTask Initialize(T screenData)
        {
            ownerScreenData = screenData;
            await UniTask.CompletedTask;
        }

        public virtual void Close(bool playAnimation)
        {
            if (IsActive())
                ScreenNavigator.Instance.PopScreen(playAnimation).Forget();
        }

        public virtual void ShowToast(string toastMessage)
            => ToastController.Instance.Show(toastMessage);

        public virtual void SetGameStatusType(GameStatusType gameStatusType)
            => GameManager.Instance.SetGameStatusType(gameStatusType);

        #endregion Class Methods
    }

    public class ScreenData
    {
        #region Members

        public Action onClosedCallbackAction;

        #endregion Members

        #region Class Methods

        public ScreenData(Action onClosedCallbackAction)
            => this.onClosedCallbackAction = onClosedCallbackAction;

        #endregion Class Methods
    }

    public class EmptyScreenData : ScreenData
    {
        #region Class Methods

        public EmptyScreenData() : base(null) { }

        #endregion Class Methods
    }
}
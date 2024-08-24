using System;
using System.Collections.Generic;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Sheets;
using Runtime.Definition;
using Runtime.Manager;
using Cysharp.Threading.Tasks;
using ScreenNavigatorSheet = UnityScreenNavigator.Runtime.Core.Sheets.Sheet;

namespace Runtime.UI
{
    /// <summary>
    /// For modals that need to update data when popped up, pass in the T argument as the data and override
    /// </summary>
    public abstract class SheetContainerSheet<T> : Sheet<T>
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

        public override UniTask Initialize(T modalData)
        {
            isLoading = false;
            sheetIds = new int[SheetDictionary.Count];
            ResetSheetIds();
            return UniTask.CompletedTask;
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

    public abstract class SheetContainerSheet : Sheet
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

        #region API Methods

        public override UniTask Initialize(Memory<object> args)
        {
            sheetIds = new int[SheetDictionary.Count];
            ResetSheetIds();
            return base.Initialize(args);
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

        #endregion API Methods
    }

    /// <summary>
    /// For sheets that need to update data when popped up, pass in the T argument as the data and override
    /// the Init() method to update its stuff.
    /// </summary>
    public abstract class Sheet<T> : ScreenNavigatorSheet
    {
        #region Class Methods

        /// <summary>
        /// Init the data for the sheet.
        /// </summary>
        /// <param name="sheetData"></param>
        public override async UniTask Initialize(Memory<object> args)
        {
            if (args.Span.Length > 0)
            {
                var data = (T)args.Span[0];
                await Initialize(data);
                return;
            }
            await Initialize(default);
        }

        public abstract UniTask Initialize(T modalData);

        public virtual void ShowToast(string toastMessage)
            => ToastController.Instance.Show(toastMessage);

        #endregion Class Methods
    }

    /// <summary>
    /// For sheets that don't need to update data, just inherit only and do nothing else.
    /// </summary>
    public abstract class Sheet : ScreenNavigatorSheet
    {
        #region Class Methods

        public virtual void ShowToast(string toastMessage)
            => ToastController.Instance.Show(toastMessage);

        public virtual void SetGameStatusType(GameStatusType gameStatusType)
            => GameManager.Instance.SetGameStatusType(gameStatusType);

        #endregion Class Methods
    }
}
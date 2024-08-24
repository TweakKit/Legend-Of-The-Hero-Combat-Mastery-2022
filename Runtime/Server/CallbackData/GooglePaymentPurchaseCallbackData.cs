using System.Collections.Generic;
using UnityEngine;
using Runtime.ConfigModel;
using Runtime.Server.Models;

namespace Runtime.Server.CallbackData
{
    public readonly struct GooglePaymentPurchaseRequestData : IRequestData
    {
        #region Members

        public readonly string AndroidPackageName;
        public readonly string ProductId;
        public readonly string PurchaseToken;

        #endregion Members

        #region Struct Methods

        public GooglePaymentPurchaseRequestData(string productId, string purchaseToken)
        {
            AndroidPackageName = Application.identifier;
            ProductId = productId;
            PurchaseToken = purchaseToken;
        }

        #endregion Struct Methods
    }

    public readonly struct GooglePaymentPurchaseCustomBundleRequestData : IRequestData
    {
        #region Members

        public readonly string AndroidPackageName;
        public readonly string ProductId;
        public readonly string PurchaseToken;
        public readonly ResourceData[] SelectedRewards;

        #endregion Members

        #region Struct Methods

        public GooglePaymentPurchaseCustomBundleRequestData(string productId, string purchaseToken, ResourceData[] selectedRewards)
        {
            AndroidPackageName = Application.identifier;
            ProductId = productId;
            PurchaseToken = purchaseToken;
            SelectedRewards = selectedRewards;
        }

        #endregion Struct Methods
    }

    public class GooglePaymentPurchaseCallbackData : RewardsCallbackData
    {
        #region Members

        public readonly PackData ReturnedPackData;
        public readonly bool IsTest;
        public readonly Dictionary<string, int> DailyResetPack;

        #endregion Members

        #region Struct Methods

        public GooglePaymentPurchaseCallbackData(int resultCode, ResourceData[] costResourcesData, List<EquipmentData> equipmentsData,
                                                 ResourceData[] receivedResourcesData, PackData returnedPackData, bool isTest, Dictionary<string, int> dailyResetPack)
            : base(resultCode, costResourcesData, equipmentsData, receivedResourcesData)
        {
            ReturnedPackData = returnedPackData;
            IsTest = isTest;
            DailyResetPack = dailyResetPack;
        }

        #endregion Struct Methods
    }
}
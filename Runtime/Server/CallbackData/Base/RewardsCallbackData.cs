using System.Collections.Generic;
using Runtime.ConfigModel;
using Runtime.Server.Models;

namespace Runtime.Server.CallbackData
{
    public class RewardsCallbackData : CallbackData
    {
        #region Members

        public readonly ResourceData[] CostResourcesData;
        public readonly List<EquipmentData> EquipmentsData;
        public readonly ResourceData[] ReceivedResourcesData;

        #endregion Members

        #region Class Methods

        public RewardsCallbackData(int resultCode, ResourceData[] costResoucesData, List<EquipmentData> equipmentsData,
                                       ResourceData[] receivedResourcesData) : base(resultCode)
        {
            CostResourcesData = costResoucesData;
            EquipmentsData = equipmentsData;
            ReceivedResourcesData = receivedResourcesData;
        }

        #endregion Class Methods
    }
}
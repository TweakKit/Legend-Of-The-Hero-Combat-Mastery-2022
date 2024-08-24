using System.Collections.Generic;
using Runtime.ConfigModel;
using Runtime.Server.Models;

namespace Runtime.Server.CallbackData
{
    public class ResourceRewardCallbackData : CallbackData
    {
        #region Members

        public readonly ResourceData[] ReceivedResourcesData;

        #endregion Members

        #region Class Methods

        public ResourceRewardCallbackData(int resultCode, ResourceData[] receivedResources) : base(resultCode)
        {
            ReceivedResourcesData = receivedResources;
        }

        #endregion Class Methods
    }
}

using System;
using UnityEngine;
using Runtime.Definition;
using BigMath = System.Numerics.BigInteger;

namespace Runtime.Server.Helper
{
    public static class Helper
    {
        #region Class Methods

        public static long GetBitMaskValueFromPlayerDataTypes(PlayerDataType[] playerDataTypes)
        {
            BigMath value = 0;
            foreach (var playerDataType in playerDataTypes)
            {
                var bonusValue = BigMath.Pow(10, ((int)playerDataType));
                value += bonusValue;
            }

            var finalValue = Convert.ToInt32(value.ToString(), 2);
            return finalValue;
        }

        public static PlatformType GetPlatformType()
        {
            var platformType = PlatformType.Windows;

            if (Application.platform == RuntimePlatform.Android)
                platformType = PlatformType.Android;
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                platformType = PlatformType.IOS;

            return platformType;
        }

        #endregion Class Methods
    }
}
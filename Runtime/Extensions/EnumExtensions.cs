using System;

namespace Runtime.Extensions
{
    public static class EnumExtensions
    {
        #region Class Methods

        public static T ParseEnum<T>(this string value)
            => (T)Enum.Parse(typeof(T), value, true);

        public static bool TryParseEnum<T>(this string value, out object result)
            => Enum.TryParse(typeof(T), value, true, out result);

        #endregion Class Methods
    }
}
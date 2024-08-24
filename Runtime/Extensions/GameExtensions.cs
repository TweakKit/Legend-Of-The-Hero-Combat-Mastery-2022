using System;
using System.Text;
using UnityEngine;
using Runtime.Definition;
using System.Collections.Generic;

namespace Runtime.Extensions
{
    public static class GameExtensions
    {
        #region Class Methods

        /// <summary>
        /// This function converts a vector2 direction to an index based on the slice count.
        /// This goes in a counter-clockwise direction.
        /// </summary>
        public static int ToIsometricDirectionIndex(this Vector2 direction, uint sliceCount)
        {
            Vector2 normalDirection = direction.normalized;
            float step = Constants.CIRCLE_DEGREES / sliceCount;
            float halfstep = step / 2.0f;
            float angle = Vector2.SignedAngle(Vector2.up, normalDirection);
            angle += halfstep;
            if (angle < 0.0f)
                angle += Constants.CIRCLE_DEGREES;
            float stepCount = angle / step;
            return Mathf.FloorToInt(stepCount);
        }

        /// <summary>
        /// This function converts an isometric direction index to a direction based on the slice count.
        /// </summary>
        public static Vector2 ToDirection(this int isometricDirectionIndex, uint sliceCount)
        {
            float step = Constants.CIRCLE_DEGREES / sliceCount;
            float radians = (isometricDirectionIndex * step + Constants.COORDINATE_AXES_OFFSET_DEGREES) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        public static Vector2 RotateByDeflectionAngle(this Vector2 direction, float deflectionAngle)
        {
            float angleInDegrees = Vector2.SignedAngle(Vector2.up, direction);
            float newAngleInDegrees = angleInDegrees + deflectionAngle;
            float newAngleInRadians = (newAngleInDegrees + Constants.COORDINATE_AXES_OFFSET_DEGREES) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(newAngleInRadians), Mathf.Sin(newAngleInRadians));
        }

        public static Vector3 ChangeDirection(this Vector3 origin, Vector3 direction)
        {
            float rotationInRadians = Vector3.Angle(direction, Vector3.forward) * Mathf.Deg2Rad;

            if (Vector3.Dot(Vector3.right, direction) >= 0)
            {
                float offsetX = origin.z * Mathf.Sin(rotationInRadians) + origin.x * Mathf.Cos((rotationInRadians));
                float offsetZ = origin.z * Mathf.Cos(rotationInRadians) - origin.x * Mathf.Sin((rotationInRadians));
                float offsetY = origin.y;
                return new Vector3(offsetX, offsetY, offsetZ);
            }
            else
            {
                float offsetX = -origin.z * Mathf.Sin(rotationInRadians) + origin.x * Mathf.Cos((rotationInRadians));
                float offsetZ = origin.z * Mathf.Cos(rotationInRadians) + origin.x * Mathf.Sin((rotationInRadians));
                float offsetY = origin.y;
                return new Vector3(offsetX, offsetY, offsetZ);
            }
        }

        public static Quaternion ToQuaternion(this Vector2 direction, float offsetDegree = Constants.COORDINATE_AXES_OFFSET_DEGREES)
            => Quaternion.Euler(Vector3.forward * (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - offsetDegree));

        public static string ToColorString(this string textInfo, Color color)
        {
            string colorString = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{textInfo}</color>";
            return colorString;
        }

        public static string ToTrackingFormat(this Enum enumType)
        {
            var inputString = enumType.ToString();
            return ToTrackingFormat(inputString);
        }

        public static string ToTrackingFormat(this string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return inputString;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(char.ToLower(inputString[0]));

            for (int i = 1; i < inputString.Length; i++)
            {
                char character = inputString[i];
                if (char.IsUpper(character) || (char.IsDigit(character) && i > 1 && !char.IsDigit(inputString[i - 1])))
                    stringBuilder.Append('_');
                stringBuilder.Append(char.ToLower(character));
            }

            return stringBuilder.ToString();
        }

        public static string ToIronSourcePlacementFormat(this Enum enumType)
        {
            var inputString = enumType.ToString();
            return ToIronSourcePlacementFormat(inputString);
        }

        public static string ToIronSourcePlacementFormat(this string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return inputString;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(char.ToUpper(inputString[0]));

            for (int i = 1; i < inputString.Length; i++)
            {
                char character = inputString[i];
                if (char.IsUpper(character) || (char.IsDigit(character) && i > 1 && !char.IsDigit(inputString[i - 1])))
                {
                    stringBuilder.Append('_');
                    stringBuilder.Append(character);
                }
                else stringBuilder.Append(char.ToLower(character));
            }

            return stringBuilder.ToString();
        }

        public static string ToLowercaseFormat(this Enum enumType)
        {
            var inputString = enumType.ToString();
            return ToLowercaseFormat(inputString);
        }

        public static string ToLowercaseFormat(this string inputString)
        {
            var outputString = inputString.ToLower();
            return outputString;
        }

        public static string ToUppercaseFormat(this Enum enumType)
        {
            var inputString = enumType.ToString();
            return ToUppercaseFormat(inputString);
        }

        public static string ToUppercaseFormat(this string inputString)
        {
            var outputString = inputString.ToUpper();
            return outputString;
        }

        public static string ToExactValueFormat(this long resourceNumber)
        {
            var str = resourceNumber.ToString();
            string returnedStr = string.Empty;
            var count = 0;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                count++;
                returnedStr = str[i] + returnedStr;
                if (count == 3 && i != 0)
                {
                    count = 0;
                    returnedStr = "." + returnedStr;
                }
            }

            return returnedStr;
        }

        public static string ToDisplayString(this long resourceNumber)
        {
            if (resourceNumber >= 1000000000)
            {
                return $"{(double)resourceNumber / 1000000000:F2}B";
            }
            else if (resourceNumber >= 1000000)
            {
                return $"{(double)resourceNumber / 1000000:F2}M";
            }
            else
            {
                int numDigits = (int)Math.Floor(Math.Log10(resourceNumber)) + 1;
                if (numDigits <= 3)
                    return $"{resourceNumber:F0}";
                else
                    return $"{resourceNumber / 1000.0:F3}";
            }
        }

        public static void Replace<TKey, TValue>(this Dictionary<TKey, TValue> original, TKey key, TValue value)
        {
            if (original.ContainsKey(key))
            {
                original.Remove(key);
            }
            original.Add(key, value);
        }

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> original,
            TKey key, TValue defaultValue)
        {
            if (original.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }


        #endregion Class Methods
    }
}
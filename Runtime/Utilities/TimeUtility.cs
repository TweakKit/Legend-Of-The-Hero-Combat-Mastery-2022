using System;

namespace Runtime.Utilities
{
    public enum TimeFormatType
    {
        AutoDetect,
        Hhmmss,
        Ddhhmmss,
        Short,
        Mmss,
    }

    public static class TimeUtility
    {
        #region Class Methods

        public static (string, bool) FormatTimeDuration(long duration, TimeFormatType formatType, int hoursWarning)
        {
            if (formatType == TimeFormatType.AutoDetect)
            {
                if (duration <= 0)
                    return ("0d:00:00:00", true);

                TimeSpan t = TimeSpan.FromSeconds(duration);

                if (t.Days > 0)
                {
                    var days = t.Days;
                    t = t.Subtract(TimeSpan.FromDays(days));
                    return (string.Format("{0}d {1:00}:{2:00}:{3:00}", days, (int)t.TotalHours, t.Minutes, t.Seconds), false);
                }

                return (string.Format("{0:00}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds), t.TotalHours <= hoursWarning);
            }
            else if (formatType == TimeFormatType.Ddhhmmss)
            {
                if (duration <= 0)
                    return ("0d:00:00:00", true);

                TimeSpan t = TimeSpan.FromSeconds(duration);
                return (string.Format("{0}d {1:00}:{2:00}:{3:00}", t.Days, t.Hours, t.Minutes, t.Seconds), t.TotalHours <= hoursWarning);
            }
            else if (formatType == TimeFormatType.Hhmmss)
            {
                if (duration <= 0)
                    return ("00:00:00", true);

                TimeSpan t = TimeSpan.FromSeconds(duration);
                return (string.Format("{0:00}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds), t.TotalHours <= hoursWarning);
            }
            else if (formatType == TimeFormatType.Mmss)
            {
                if (duration <= 0)
                    return ("00:00", true);

                TimeSpan t = TimeSpan.FromSeconds(duration);
                return (string.Format("{0:00}:{1:00}", t.Minutes, t.Seconds), t.TotalHours <= hoursWarning);
            }
            else if (formatType == TimeFormatType.Short)
            {
                if (duration <= 0)
                    return ("0m 0s", true);

                var seconds = duration % 60;
                var minutes = duration / 60;
                if (minutes > 0)
                {
                    if (seconds > 0)
                        return ($"{minutes}m {seconds}s", false);
                    else
                        return ($"{minutes}m", false);
                }
                else
                {
                    return ($"{seconds}s", false);
                }
            }

            return (duration.ToString(), false);
        }

        public static string FormatTimeDuration(long duration, TimeFormatType formatType)
        {
            if (formatType == TimeFormatType.AutoDetect)
            {
                if (duration <= 0)
                    return "0d:00:00:00";

                TimeSpan t = TimeSpan.FromSeconds(duration);
                if (t.Days > 0)
                {
                    var days = t.Days;
                    t = t.Subtract(TimeSpan.FromDays(days));
                    return string.Format("{0}d {1:00}:{2:00}:{3:00}", days, (int)t.TotalHours, t.Minutes, t.Seconds);
                }

                return string.Format("{0:00}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds);
            }
            else if (formatType == TimeFormatType.Ddhhmmss)
            {
                if (duration <= 0)
                    return "0d:00:00:00";

                TimeSpan t = TimeSpan.FromSeconds(duration);
                return string.Format("{0}d {1:00}:{2:00}:{3:00}", t.Days, t.Hours, t.Minutes, t.Seconds);
            }
            else if (formatType == TimeFormatType.Hhmmss)
            {
                if (duration <= 0)
                    return "00:00:00";

                TimeSpan t = TimeSpan.FromSeconds(duration);
                return string.Format("{0:00}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds);
            }
            else if (formatType == TimeFormatType.Mmss)
            {
                if (duration <= 0)
                    return "00:00";

                TimeSpan t = TimeSpan.FromSeconds(duration);
                return string.Format("{0:00}:{1:00}", t.Minutes, t.Seconds);
            }
            else if (formatType == TimeFormatType.Short)
            {
                if (duration <= 0)
                    return "0m 0s";

                var seconds = duration % 60;
                var minutes = duration / 60;
                if (minutes > 0)
                {
                    if (seconds > 0)
                        return $"{minutes}m {seconds}s";
                    else
                        return $"{minutes}m";
                }
                else
                {
                    return $"{seconds}s";
                }
            }

            return duration.ToString();
        }

        #endregion Class Methods
    }
}
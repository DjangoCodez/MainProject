using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.Util
{
    public static class SettingsUtility
    {
        #region User Settings

        public static String GetStringUserSetting(Dictionary<int, object> settings, UserSettingType type, string defaultValue = "")
        {
            try
            {
                return settings.ContainsKey((int)type) ? settings[(int)type].ToString() : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int GetIntUserSetting(Dictionary<int, object> settings, UserSettingType type, int defaultValue = 0)
        {
            try
            {
                return settings.ContainsKey((int)type) ? Int32.Parse(settings[(int)type].ToString()) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool GetBoolUserSetting(Dictionary<int, object> settings, UserSettingType type, bool defaultValue = false)
        {
            try
            {
                return settings.ContainsKey((int)type) ? Boolean.Parse(settings[(int)type].ToString()) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static DateTime GetDateUserSetting(Dictionary<int, object> settings, UserSettingType type, DateTime defaultValue)
        {
            try
            {
                return settings.ContainsKey((int)type) ? DateTime.Parse(settings[(int)type].ToString()) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region Company Settings

        public static String GetStringCompanySetting(Dictionary<int, object> settings, CompanySettingType type, string defaultValue = "")
        {
            try
            {
                return settings.ContainsKey((int)type) ? settings[(int)type].ToString() : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int GetIntCompanySetting(Dictionary<int, object> settings, CompanySettingType type, int defaultValue = 0, bool allowZero = true)
        {
            try
            {
                int value = defaultValue;
                if (settings.ContainsKey((int)type))
                {
                    foreach (var key in settings)
                    {
                        if (key.Key == (int)type)
                        {
                            value = Convert.ToInt32(key.Value);
                            break;
                        }
                    }
                }

                if (!allowZero && value == 0)
                    value = defaultValue;

                return value;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal GetDecimalCompanySetting(Dictionary<int, object> settings, CompanySettingType type, decimal defaultValue = 0)
        {
            try
            {
                return settings.ContainsKey((int)type) ? Decimal.Parse(settings[(int)type].ToString()) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool GetBoolCompanySetting(Dictionary<int, object> settings, CompanySettingType type, bool defaultValue = false)
        {
            try
            {
                return settings.ContainsKey((int)type) ? Boolean.Parse(settings[(int)type].ToString()) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static DateTime GetDateCompanySetting(Dictionary<int, object> settings, CompanySettingType type, DateTime defaultValue)
        {
            try
            {
                return settings.ContainsKey((int)type) ? DateTime.Parse(settings[(int)type].ToString()) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion
    }
}

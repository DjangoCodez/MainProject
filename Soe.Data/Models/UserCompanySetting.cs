using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Data
{
    public partial class UserCompanySetting
    {

    }

    public static partial class EntityExtensions
    {
        #region UserCompanySetting

        public static object GetSettingValue(this UserCompanySetting setting)
        {
            object value = null;
            if (setting != null)
            {
                switch (setting.DataTypeId)
                {
                    case (int)SettingDataType.String:
                        value = setting.StrData;
                        break;
                    case (int)SettingDataType.Integer:
                        value = setting.IntData;
                        break;
                    case (int)SettingDataType.Boolean:
                        value = setting.BoolData;
                        break;
                    case (int)SettingDataType.Date:
                    case (int)SettingDataType.Time:
                        value = setting.DateData;
                        break;
                    case (int)SettingDataType.Decimal:
                        value = setting.DecimalData;
                        break;
                }
            }

            return value;
        }

        #endregion
    }
}

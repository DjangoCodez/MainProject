using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    #region FieldSetting

    public class FieldSetting
    {
        #region Variables

        public int? ActorCompanyId { get; set; }
        public int? RoleId { get; set; }
        public int FormId { get; set; }
        public String FormName { get; set; }
        public int FieldId { get; set; }
        public String FieldName { get; set; }
        private List<FieldSettingDetail> fieldSettingDetails;

        #endregion

        #region Methods

        /// <summary>
        /// Add a FieldSettingDetail
        /// </summary>
        /// <param name="key">The SysSettingId</param>
        /// <param name="value">The FieldSettingDetail</param>
        public void AddFieldSettingDetail(FieldSettingDetail fieldSettingDetail)
        {
            if (fieldSettingDetails == null)
            {
                fieldSettingDetails = new List<FieldSettingDetail>();
                fieldSettingDetails.Add(fieldSettingDetail);
            }
            else
            {
                //Ensure that a FieldSettingDetail of the same type not already been added
                if (!SettingExists(fieldSettingDetail.SysSettingId))
                    fieldSettingDetails.Add(fieldSettingDetail);
            }
        }

        /// <summary>
        /// Get s Dictionary of FieldSettingDetails
        /// </summary>
        /// <returns>The FieldSettingDetails</returns>
        public List<FieldSettingDetail> GetFieldSettingDetails()
        {
            return fieldSettingDetails;
        }

        /// <summary>
        /// Checks if a setting with the given SysSettingId already has been added to the FieldSettingDetail List
        /// </summary>
        /// <param name="SysSettingId"></param>
        /// <returns></returns>
        private bool SettingExists(int sysSettingId)
        {
            return fieldSettingDetails?.Any(i => i.SysSettingId == sysSettingId) ?? false;
        }

        #endregion
    }

    #endregion

    #region FieldSettingDetail

    public class FieldSettingDetail
    {
        #region Variables

        public int SysSettingId { get; set; }
        public bool HasValue { get; set; }
        private String settingValue;

        #endregion

        #region Methods

        public String Value
        {
            set
            {
                //Only set if not been set before (in higher priority)
                if (!HasValue)
                {
                    settingValue = value;
                    HasValue = true;
                }
            }
            get
            {
                return settingValue;
            }
        }

        #endregion
    }

    #endregion

    #region SettingObject

    public class SettingObject
    {
        #region Variables

        public int? ActorCompanyId { get; set; } //has value only if is CompanyFieldSetting
        public int? RoleId { get; set; } //has value only if is RoleFieldSetting
        public int FormId { get; set; }
        public string FormName { get; set; }
        public int? FieldId { get; set; } //has value only if is RoleFieldSetting or CompanyFieldSetting
        public string FieldName { get; set; } //has value only if is RoleFieldSetting or CompanyFieldSetting
        public int SysSettingId { get; set; }
        public string Value { get; set; }
        public SoeFieldSettingType Type { get; set; }

        #endregion
    }

    #endregion

    #region UserCompanySettingObject

    public class UserCompanySettingObject
    {
        public int DataTypeId { get; set; }
        public String StringSetting { get; set; }
        public int? IntSetting { get; set; }
        public bool? BoolSetting { get; set; }
        public DateTime? DateSetting { get; set; }
        public Decimal? DecimalSetting { get; set; }
    }

    #endregion
}

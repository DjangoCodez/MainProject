using SoftOne.Soe.Common.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    public class ApiSettingDTO
    {
        public int ApiSettingId { get; set; }
        public TermGroup_ApiSettingType Type { get; set; }
        [TsIgnore]
        public string Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extensions
        public string Name { get; set; }
        public string Description { get; set; }
        public SettingDataType DataType { get; set; }
        public string StringValue { get; set; }
        public int? IntegerValue { get; set; }
        public bool? BooleanValue { get; set; }
        public bool IsModified { get; set; }

        public bool GetBool(DateTime? date)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            switch(this.Type)
            {
                case TermGroup_ApiSettingType.KeepEmploymentAccount:
                case TermGroup_ApiSettingType.KeepEmploymentPriceType:
                case TermGroup_ApiSettingType.DoNotSetChiefToStandard:
                case TermGroup_ApiSettingType.DoNotCloseEmployeeAccountAndAttestRole:
                case TermGroup_ApiSettingType.DoSetDefaultCompanyWhenUpdatingRoles:
                    if (this.IsActive(date))
                        return StringUtility.GetBool(this.Value);
                    break;
            }

            return false;
        }
        public int? GetNullableInt(DateTime? date)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            switch (this.Type)
            {
                case TermGroup_ApiSettingType.AccountDimIdForEmployeeAccount:
                    if (this.IsActive(date))
                        return StringUtility.GetNullableInt(this.Value);
                    break;
            }

            return null;
        }
        public void SetNameAndDescription(SysTermDTO term)
        {
            if (term == null)
                return;

            this.Name = term.Name;
            this.Description = term.Description;
        }
        public void SetValue()
        {
            switch (this.DataType)
            {
                case SettingDataType.String:
                    this.Value = this.StringValue;
                    break;
                case SettingDataType.Integer:
                    this.Value = this.IntegerValue?.ToNullable()?.ToString();
                    break;
                case SettingDataType.Boolean:
                    this.Value = this.BooleanValue == true ? "1" : "0";
                    break;
                default:
                    this.Value = null;
                    break;
            }
        }
        public void SetDataType()
        {
            switch (this.Type)
            {
                case TermGroup_ApiSettingType.KeepEmploymentAccount:
                case TermGroup_ApiSettingType.KeepEmploymentPriceType:
                case TermGroup_ApiSettingType.DoNotSetChiefToStandard:
                case TermGroup_ApiSettingType.DoNotCloseEmployeeAccountAndAttestRole:
                case TermGroup_ApiSettingType.DoSetDefaultCompanyWhenUpdatingRoles:
                    this.DataType = SettingDataType.Boolean;
                    this.BooleanValue = StringUtility.GetBool(this.Value, valueIfEmpty: false);
                    this.Value = this.BooleanValue == true ? "1" : "0";
                    break;
                case TermGroup_ApiSettingType.AccountDimIdForEmployeeAccount:
                    this.DataType = SettingDataType.Integer;
                    this.IntegerValue = StringUtility.GetNullableInt(this.Value);
                    break;
            }
        }
        private bool IsActive(DateTime? date)
        {
            if (date.HasValue)
            {
                if (this.StartDate.HasValue && this.StartDate.Value > date.Value)
                    return false;
                if (this.StopDate.HasValue && this.StopDate.Value < date.Value)
                    return false;
            }

            return true;
        }
        public static ApiSettingDTO Create(TermGroup_ApiSettingType type, SysTermDTO term)
        {
            ApiSettingDTO dto = new ApiSettingDTO()
            {
                Type = type,
            };
            dto.SetDataType();
            dto.SetNameAndDescription(term);
            return dto;
        }
    }

    public static class ApiSettingExtensions
    {
        public static List<ApiSettingDTO> Filter(this List<ApiSettingDTO> l, TermGroup_ApiSettingType type)
        {
            return l?.Where(i => i.Type == type && i.State == SoeEntityState.Active).OrderByDescending(i => i.ApiSettingId).ToList() ?? new List<ApiSettingDTO>();
        }
        public static void CreateMissing(this List<ApiSettingDTO> l, List<SysTermDTO> terms)
        {
            if (l == null)
                l = new List<ApiSettingDTO>();

            foreach (TermGroup_ApiSettingType type in Enum.GetValues(typeof(TermGroup_ApiSettingType)))
            {
                if (type == TermGroup_ApiSettingType.Uknown)
                    continue;

                ApiSettingDTO setting = l.FirstOrDefault(e => e.Type == type);
                if (setting == null)
                {
                    setting = ApiSettingDTO.Create(type, terms.FirstOrDefault(term => term.SysTermId == (int)type));
                    l.Add(setting);
                }
            }
        }
        public static bool GetBool(this List<ApiSettingDTO> l, TermGroup_ApiSettingType type, DateTime? date = null)
        {
            return l.Filter(type).Any(i => i.GetBool(date));
        }
        public static int? GetNullableInt(this List<ApiSettingDTO> l, TermGroup_ApiSettingType type, DateTime? date = null)
        {
            foreach (var e in l.Filter(type))
            {
                int? value = e.GetNullableInt(date);
                if (value.HasValue)
                    return value.Value;
            }
            return null;
        }
    }
}

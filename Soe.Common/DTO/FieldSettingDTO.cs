using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public interface ISoeSetting
    {
        string Label { get; set; }
        bool? Visible { get; set; }
        bool? SkipTabStop { get; set; }
        bool? ReadOnly { get; set; }
        bool? BoldLabel { get; set; }
    }

    [TSInclude]
    public class FieldSettingGridDTO
    {
        public string FormName { get; set; }
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string CompanySettingsSummary { get; set; }
        public string RoleSettingsSummary { get; set; }
    }

    [TSInclude]
    public class FieldSettingDTO
    {
        public int FormId { get; set; }
        public string FormName { get; set; }
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public SoeFieldSettingType Type { get; set; }
        public string CompanySettingsSummary { get; set; }
        public string RoleSettingsSummary { get; set; }
        public CompanyFieldSettingDTO CompanySetting { get; set; }
        public List<RoleFieldSettingDTO> RoleSettings { get; set; }
    }

    [TSInclude]
    public class CompanyFieldSettingDTO : ISoeSetting
    {
        public int ActorCompanyId { get; set; }
        public string Label { get; set; }
        public bool? Visible { get; set; }
        public bool? SkipTabStop { get; set; }
        public bool? ReadOnly { get; set; }
        public bool? BoldLabel { get; set; }
    }

    [TSInclude]
    public class RoleFieldSettingDTO : ISoeSetting
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Label { get; set; }
        public bool? Visible { get; set; }
        public bool? SkipTabStop { get; set; }
        public bool? ReadOnly { get; set; }
        public bool? BoldLabel { get; set; }

        //Names for supported settings now:
        public string VisibleName { get; set; }
    }
}


using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class EmploymentTypeDTO
    {
        public int? EmploymentTypeId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public bool Standard { get { return !this.EmploymentTypeId.HasValue; } }
        public bool ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment { get; set; }
        public bool SettingOnly { get; set; }
        public string ExternalCode { get; set; }
        public SoeEntityState State { get; set; }

        // Extentions
        public bool Active { get; set; }
        public string TypeName { get; set; }
        public string CodeAndName
        {
            get
            {
                return $"{this.Code} {this.Name}";
            }
        }
        public bool DisableActive
        {
            get { return this.Standard; }
        }
        public bool HideEdit
        {
            get { return this.Standard; }
        }
        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }

        public EmploymentTypeDTO(int type, string name, bool active = true)
        {
            this.EmploymentTypeId = null;
            this.Type = type;
            this.Name = name;
            this.Description = null;
            this.Code = type.ToString();
            this.State = active ? SoeEntityState.Active : SoeEntityState.Inactive;
            this.Active = active;
        }

        public EmploymentTypeDTO()
        {
        }

        public int GetEmploymentType()
        {
            return this.Standard ? this.Type : (this.EmploymentTypeId ?? 0);
        }

        public static bool IsStandard(int id)
        {
            return id < 10000;
        }

        public static bool IsStandard(string value, out int id, out int max)
        {
            max = (int)Enum.GetValues(typeof(TermGroup_EmploymentType)).Cast<TermGroup_EmploymentType>().Max();
            return Int32.TryParse(value, out id) && id <= max;
        }
    }

    [TSInclude]
    public class EmploymentTypeGridDTO
    {
        public int? EmploymentTypeId { get; set; }
        public int GridId { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public bool AllowEdit { get; set; }
        public bool ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment { get; set; }
        public bool SettingOnly { get; set; }
        public string ExternalCode { get; set; }
        public SoeEntityState State { get; set; }
        public bool Standard { get; set; }
        public string StandardText { get; set; }
    }

    [TSInclude]
    public class EmploymentTypeSmallDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public bool Active { get; set; }
    }
}

using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class AttestRole : ICreatedModified, IState
    {
        public string ModuleName { get; set; }
        public List<string> ExternalCodes { get; set; }
        public string ExternalCodesString { get; set; }

        public List<CompanyCategoryRecord> PrimaryCategoryRecords { get; set; }
        public List<CompanyCategoryRecord> SecondaryCategoryRecords { get; set; }

        public bool IsSigningRole
        {
            get { return this.Module == (int)SoeModule.Manage; }
        }
    }

    public static partial class EntityExtensions
    {
        #region AttestRole

        public static AttestRoleDTO ToDTO(this AttestRole e)
        {
            if (e == null)
                return null;

            AttestRoleDTO dto = new AttestRoleDTO()
            {
                AttestRoleId = e.AttestRoleId,
                ActorCompanyId = e.ActorCompanyId,
                Module = (SoeModule)e.Module,
                Name = e.Name,
                Description = e.Description,
                ExternalCodesString = e.ExternalCodesString,
                DefaultMaxAmount = e.DefaultMaxAmount,
                ShowUncategorized = e.ShowUncategorized,
                ShowAllCategories = e.ShowAllCategories,
                ShowAllSecondaryCategories = e.ShowAllSecondaryCategories,
                ShowTemplateSchedule = e.ShowTemplateSchedule,
                AlsoAttestAdditionsFromTime = e.AlsoAttestAdditionsFromTime,
                HumanResourcesPrivacy = e.HumanResourcesPrivacy,
                ReminderAttestStateId = e.ReminderAttestStateId,
                ReminderNoOfDays = e.ReminderNoOfDays,
                ReminderPeriodType = e.ReminderPeriodType,
                IsExecutive = e.IsExecutive,
                AttestByEmployeeAccount = e.AttestByEmployeeAccount,
                StaffingByEmployeeAccount = e.StaffingByEmployeeAccount,
                AllowToAddOtherEmployeeAccounts = e.AllowToAddOtherEmployeeAccounts,
                TransitionIds = e.AttestTransition.Select(x => x.AttestTransitionId).ToList(),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Sort = e.Sort,
                Active = e.State == (int)SoeEntityState.Active,
            };

            if (!e.ExternalCodes.IsNullOrEmpty())
            {
                dto.ExternalCodes = e.ExternalCodes;
                dto.ExternalCodesString = e.ExternalCodesString;
            }

            dto.PrimaryCategoryRecords = e.PrimaryCategoryRecords?.ToDTOs(false).ToList() ?? new List<CompanyCategoryRecordDTO>();
            dto.SecondaryCategoryRecords = e.SecondaryCategoryRecords?.ToDTOs(false).ToList() ?? new List<CompanyCategoryRecordDTO>();
            dto.AttestRoleMapping = e.ParentAttestRoleMapping?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<AttestRoleMappingDTO>();

            return dto;
        }

        public static AttestRoleGridDTO ToGridDTO(this AttestRole e, List<GenericType> yesNoTerms)
        {
            if (e == null)
                return null;

            return new AttestRoleGridDTO()
            {
                AttestRoleId = e.AttestRoleId,
                Name = e.Name,
                Description = e.Description,
                ExternalCodesString = e.ExternalCodesString,
                DefaultMaxAmount = e.DefaultMaxAmount,
                ShowUncategorizedText = yesNoTerms?.FirstOrDefault(x => x.Id == (int)(e.ShowUncategorized ? TermGroup_YesNo.Yes : TermGroup_YesNo.No))?.Name ?? string.Empty,
                ShowAllCategoriesText = yesNoTerms?.FirstOrDefault(x => x.Id == (int)(e.ShowAllCategories ? TermGroup_YesNo.Yes : TermGroup_YesNo.No))?.Name ?? string.Empty,
                Sort = e.Sort,
                IsActive = e.State == (int)SoeEntityState.Active,
            };
        }

        public static IEnumerable<AttestRoleDTO> ToDTOs(this IEnumerable<AttestRole> l)
        {
            var dtos = new List<AttestRoleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<AttestRoleGridDTO> ToGridDTOs(this IEnumerable<AttestRole> l, List<GenericType> yesNoTerms)
        {
            var dtos = new List<AttestRoleGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(yesNoTerms));
                }
            }
            return dtos;
        }

        #endregion
    }
}

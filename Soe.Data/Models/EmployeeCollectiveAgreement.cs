using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeCollectiveAgreement : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeeCollectiveAgreement

        public static EmployeeCollectiveAgreementDTO ToDTO(this EmployeeCollectiveAgreement e)
        {
            if (e == null)
                return null;

            EmployeeCollectiveAgreementDTO dto = new EmployeeCollectiveAgreementDTO()
            {
                EmployeeCollectiveAgreementId = e.EmployeeCollectiveAgreementId,
                ActorCompanyId = e.ActorCompanyId,
                Code = e.Code,
                ExternalCode = e.ExternalCode,
                Name = e.Name,
                Description = e.Description,
                EmployeeGroupId = e.EmployeeGroupId,
                PayrollGroupId = e.PayrollGroupId,
                VacationGroupId = e.VacationGroupId,
                AnnualLeaveGroupId = e.AnnualLeaveGroupId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.EmployeeGroup != null)
                dto.EmployeeGroupName = e.EmployeeGroup.Name;
            if (e.PayrollGroup != null)
                dto.PayrollGroupName = e.PayrollGroup.Name;
            if (e.VacationGroup != null)
                dto.VacationGroupName = e.VacationGroup.Name;
            if (e.AnnualLeaveGroup != null)
                dto.AnnualLeaveGroupName = e.AnnualLeaveGroup.Name;

            return dto;
        }

        public static List<EmployeeCollectiveAgreementDTO> ToDTOs(this IEnumerable<EmployeeCollectiveAgreement> l)
        {
            var dtos = new List<EmployeeCollectiveAgreementDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;
        }

        public static EmployeeCollectiveAgreementGridDTO ToGridDTO(this EmployeeCollectiveAgreement e)
        {
            if (e == null)
                return null;

            EmployeeCollectiveAgreementGridDTO dto = new EmployeeCollectiveAgreementGridDTO()
            {
                EmployeeCollectiveAgreementId = e.EmployeeCollectiveAgreementId,
                Code = e.Code,
                ExternalCode = e.ExternalCode,
                Name = e.Name,
                Description = e.Description,
                State = (SoeEntityState)e.State
            };

            if (e.EmployeeGroup != null)
                dto.EmployeeGroupName = e.EmployeeGroup.Name;

            if (e.PayrollGroup != null)
                dto.PayrollGroupName = e.PayrollGroup.Name;

            if (e.VacationGroup != null)
                dto.VacationGroupName = e.VacationGroup.Name;

            if (e.AnnualLeaveGroup != null)
                dto.AnnualLeaveGroupName = e.AnnualLeaveGroup.Name;

            if (!e.EmployeeTemplate.IsNullOrEmpty())
                dto.EmployeeTemplateNames = string.Join(", ", e.EmployeeTemplate.Where(t => t.State == (int)SoeEntityState.Active || t.State == (int)SoeEntityState.Inactive).Select(t => t.Name ?? string.Empty));

            return dto;
        }

        public static List<EmployeeCollectiveAgreementGridDTO> ToGridDTOs(this IEnumerable<EmployeeCollectiveAgreement> l)
        {
            var dtos = new List<EmployeeCollectiveAgreementGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }

            return dtos;
        }


        public static EmployeeCollectiveAgreement CopyFrom(this EmployeeCollectiveAgreement employeeCollectiveAgreement, int employeeGroupId, int? payrollGroupId, int? VacationGroupId)
        {
            return new EmployeeCollectiveAgreement()
            {
                Name = employeeCollectiveAgreement.Name,
                Description = employeeCollectiveAgreement.Description,
                EmployeeGroupId = employeeGroupId,
                PayrollGroupId = payrollGroupId,
                VacationGroupId = VacationGroupId,
                Code = employeeCollectiveAgreement.Code,
                ExternalCode = employeeCollectiveAgreement.ExternalCode
            };
        }
        #endregion
    }
}

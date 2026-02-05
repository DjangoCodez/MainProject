using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;


namespace SoftOne.Soe.Data
{
    public partial class EmployeeCalculateVacationResult : ICreatedModified, IState
    {

    }

    public partial class EmployeeCalculateVacationResultHead : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeeCalculateVacationResultHead

        public static IEnumerable<EmployeeCalculateVacationResultHeadDTO> ToDTOs(this IEnumerable<EmployeeCalculateVacationResultHead> l)
        {
            var dtos = new List<EmployeeCalculateVacationResultHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeCalculateVacationResultHeadDTO ToDTO(this EmployeeCalculateVacationResultHead e)
        {
            if (e == null)
                return null;

            EmployeeCalculateVacationResultHeadDTO dto = new EmployeeCalculateVacationResultHeadDTO()
            {
                EmployeeCalculateVacationResultHeadId = e.EmployeeCalculateVacationResultHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Date = e.Date,
                Created = e.Created,
                CreatedBy = e.CreatedBy ?? string.Empty,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy ?? string.Empty,
                State = (SoeEntityState)e.State,
            };

            if (e.EmployeeCalculateVacationResult != null)
            {
                dto.EmployeeContainer = new List<CalculateVacationResultContainer>();

                foreach (var item in e.EmployeeCalculateVacationResult.GroupBy(x => x.EmployeeId))
                {
                    dto.EmployeeContainer.Add(new CalculateVacationResultContainer()
                    {
                        EmployeeId = item.Key,
                        EmployeeCalculateVacationResultHeadId = item.First().EmployeeCalculateVacationResultHeadId,
                        EmployeeNr = item.First().Employee?.EmployeeNr,
                        EmployeeName = item.First().Employee?.Name,
                        Results = item.ToDTOs().ToList()
                    });
                }
            }

            return dto;
        }

        #endregion

        #region EmployeeCalculateVacationResult

        public static IEnumerable<EmployeeCalculateVacationResultDTO> ToDTOs(this IEnumerable<EmployeeCalculateVacationResult> l)
        {
            var dtos = new List<EmployeeCalculateVacationResultDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeCalculateVacationResultDTO ToDTO(this EmployeeCalculateVacationResult e)
        {
            if (e == null)
                return null;

            return new EmployeeCalculateVacationResultDTO()
            {
                EmployeeCalculateVacationResultId = e.EmployeeCalculateVacationResultId,
                EmployeeCalculateVacationResultHeadId = e.EmployeeCalculateVacationResultHeadId,
                EmployeeId = e.EmployeeId,
                Success = e.Success,
                Type = e.Type,
                Name = e.Name,
                Value = e.Value,
                FormulaPlain = e.FormulaPlain,
                FormulaExtracted = e.FormulaExtracted,
                FormulaNames = e.FormulaNames,
                FormulaOrigin = e.FormulaOrigin,
                Error = e.Error,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        #endregion
    }
}

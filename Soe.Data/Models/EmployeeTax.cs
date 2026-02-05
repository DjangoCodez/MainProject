using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeTaxSE : ICreatedModified, IState
    {
        public string EmployeeName
        {
            get { return this.Employee?.ContactPerson?.Name ?? string.Empty; }
        }
        public string EmployeeNumber
        {
            get { return this.Employee?.EmployeeNr ?? string.Empty; }
        }
        public string EmployeeSocialSec
        {
            get { return this.Employee?.ContactPerson?.SocialSec ?? string.Empty; }
        }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeCalculatedCost

        public static IEnumerable<EmployeeCalculatedCostDTO> ToDTOs(this IEnumerable<EmployeeCalculatedCost> l)
        {
            var dtos = new List<EmployeeCalculatedCostDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeCalculatedCostDTO ToDTO(this EmployeeCalculatedCost c)
        {
            return new EmployeeCalculatedCostDTO
            {
                EmployeeCalculatedCostId = c.EmployeeCalculatedCostId,
                CalculatedCostPerHour = c.CalculatedCostPerHour,
                EmployeeId = c.EmployeeId,
                fromDate = c.FromDate,
                ProjectId = c.ProjectId
            };
        }

        #endregion
    }
}

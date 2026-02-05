using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeChild : ICreatedModified, IState
    {
        public string Name
        {
            get
            {
                string fullName = this.FirstName + " " + this.LastName;

                if (!string.IsNullOrEmpty(fullName.Trim()))
                    return fullName;
                else if (this.BirthDate.HasValue)
                    return this.BirthDate.Value.ToShortDateString();
                else
                    return "";
            }
        }

        public int UsedDays { get; set; }
        public int UsedDaysPayroll { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeChild

        public static EmployeeChildDTO ToDTO(this EmployeeChild e)
        {
            if (e == null)
                return null;

            return new EmployeeChildDTO()
            {
                EmployeeChildId = e.EmployeeChildId,
                EmployeeId = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                BirthDate = e.BirthDate,
                SingleCustody = e.SingelCustody,
                OpeningBalanceUsedDays = e.OpeningBalanceUsedDays,
                UsedDays = e.UsedDays,
                UsedDaysPayroll = e.UsedDaysPayroll,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<EmployeeChildDTO> ToDTOs(this IEnumerable<EmployeeChild> l)
        {
            var dtos = new List<EmployeeChildDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}

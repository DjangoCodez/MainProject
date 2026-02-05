using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeHibernatingAbsenceHead : ICreatedModified, IState
    {
        public DateTime DateFrom 
        { 
            get
            {
                return this.Employment.GetDateFromOrMin();
            }
        }
        public DateTime DateTo 
        { 
            get
            {
                return this.Employment.GetDateToOrMax();
            }
        }
    }

    public partial class TimeHibernatingAbsenceRow : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region TimeHibernatingAbsenceHead

        public static TimeHibernatingAbsenceHeadDTO ToDTO(this TimeHibernatingAbsenceHead e, bool fillEmptyDays = false)
        {
            if (e == null)
                return null;

            var dto = new TimeHibernatingAbsenceHeadDTO()
            {
                TimeHibernatingAbsenceHeadId = e.TimeHibernatingAbsenceHeadId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeName = e.Employee?.Name ?? "",
                EmployeeNr = e.Employee?.EmployeeNr ?? "",
                EmployeeId = e.EmployeeId,
                EmploymentId = e.EmploymentId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.TimeHibernatingAbsenceRow != null)
                dto.Rows = e.TimeHibernatingAbsenceRow.Where(w => w.State == (int)SoeEntityState.Active).ToDTOs(false);

            if (e.Employment != null)
            {
                dto.Employment = e.Employment.ToDTO();

                if (fillEmptyDays && e.Employment.DateFrom.HasValue && e.Employment.DateTo.HasValue)
                {
                    DateTime currentDate = e.Employment.DateFrom.Value;
                    while (currentDate <= e.Employment.DateTo.Value)
                    {
                        if (!dto.Rows.Any(w => w.Date == currentDate))
                        {
                            TimeHibernatingAbsenceRowDTO row = new TimeHibernatingAbsenceRowDTO()
                            {
                                TimeHibernatingAbsenceHeadId = e.TimeHibernatingAbsenceHeadId,
                                ActorCompanyId = e.ActorCompanyId,
                                EmployeeId = e.EmployeeId,
                                Date = currentDate,
                                AbsenceTimeMinutes = 0,
                                ScheduleTimeMinutes = 0,
                                State = (int)SoeEntityState.Active,
                            };

                            dto.Rows.Add(row);
                        }

                        currentDate = currentDate.AddDays(1);
                    }

                    dto.Rows = dto.Rows.OrderBy(x => x.Date).ToList();
                }
            }

            return dto;
        }

        public static List<TimeHibernatingAbsenceHeadDTO> ToDTOs(this IEnumerable<TimeHibernatingAbsenceHead> l, bool fillEmptyDays = false)
        {
            var dtos = new List<TimeHibernatingAbsenceHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(fillEmptyDays));
                }
            }
            return dtos;
        }

        #endregion

        #region TimeHibernatingAbsenceRow

        public static TimeHibernatingAbsenceRowDTO ToDTO(this TimeHibernatingAbsenceRow e, bool includeHead)
        {
            if (e == null)
                return null;

            var dto  = new TimeHibernatingAbsenceRowDTO()
            {
                TimeHibernatingAbsenceRowId = e.TimeHibernatingAbsenceRowId,
                TimeHibernatingAbsenceHeadId = e.TimeHibernatingAbsenceHeadId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,                
                EmploymeeChildId = e.EmployeeChildId,
                Date = e.Date,
                AbsenceTimeMinutes = e.AbsenceTimeMinutes,
                ScheduleTimeMinutes = e.ScheduleTimeMinutes,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (includeHead && e.TimeHibernatingAbsenceHead != null)
                dto.TimeHibernatingAbsenceHead = e.TimeHibernatingAbsenceHead.ToDTO();

            return dto;
        }

        public static List<TimeHibernatingAbsenceRowDTO> ToDTOs(this IEnumerable<TimeHibernatingAbsenceRow> l, bool includeHead)
        {
            var dtos = new List<TimeHibernatingAbsenceRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeHead));
                }
            }
            return dtos;
        }

        #endregion
    }
}

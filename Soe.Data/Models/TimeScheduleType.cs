using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleType : ICreatedModified, IState
    {
        public ReportScheduleType ReportScheduleType
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Code))
                {
                    if (this.Code.Contains("ÖT"))
                        return ReportScheduleType.OverTime;
                    if (this.Code.Contains("MT"))
                        return ReportScheduleType.AddedTime;
                }
                return ReportScheduleType.Unknown;
            }
        }
    }

    public partial class TimeScheduleTypeFactor : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleType

        public static TimeScheduleTypeDTO ToDTO(this TimeScheduleType e, bool includeFactors)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeFactors && !e.TimeScheduleTypeFactor.IsLoaded)
                {
                    e.TimeScheduleTypeFactor.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleType.cs e.TimeScheduleTypeFactor");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeScheduleTypeDTO dto = new TimeScheduleTypeDTO()
            {
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                ActorCompanyId = e.ActorCompanyId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                IsAll = e.IsAll,
                IsNotScheduleTime = e.IsNotScheduleTime,
                UseScheduleTimeFactor = e.UseScheduleTimeFactor,
                IsBilagaJ = e.IsBilagaJ,
                IgnoreIfExtraShift = e.IgnoreIfExtraShift,
                ShowInTerminal = e.ShowInTerminal,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                TimeDeviationCauseName = e.TimeDeviationCause?.Name ?? string.Empty,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (includeFactors)
            {
                dto.Factors = new List<TimeScheduleTypeFactorDTO>();
                if (!e.TimeScheduleTypeFactor.IsNullOrEmpty())
                    dto.Factors = e.TimeScheduleTypeFactor.Where(f => f.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }

            return dto;
        }

        public static IEnumerable<TimeScheduleTypeDTO> ToDTOs(this IEnumerable<TimeScheduleType> l, bool includeFactors, bool onlyShowInTerminal = false)
        {
            var dtos = new List<TimeScheduleTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    if (onlyShowInTerminal && !e.ShowInTerminal)
                        continue;

                    dtos.Add(e.ToDTO(includeFactors));
                }
            }
            return dtos;
        }
        public static TimeScheduleTypeGridDTO ToGridDTO(this TimeScheduleType e)
        {
            if (e == null)
                return null;

            TimeScheduleTypeGridDTO dto = new TimeScheduleTypeGridDTO()
            {
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                IsAll = e.IsAll,
                IsBilagaJ = e.IsBilagaJ,
                ShowInTerminal = e.ShowInTerminal,
                TimeDeviationCauseName = e.TimeDeviationCause?.Name ?? string.Empty,
                State = (SoeEntityState)e.State,

            };

            return dto;
        }

        public static IEnumerable<TimeScheduleTypeGridDTO> ToGridDTOs(this IEnumerable<TimeScheduleType> l)
        {
            var dtos = new List<TimeScheduleTypeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static TimeScheduleTypeSmallDTO ToSmallDTO(this TimeScheduleType e, bool includeFactors)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeFactors && !e.TimeScheduleTypeFactor.IsLoaded)
                { 
                    e.TimeScheduleTypeFactor.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleType.cs e.TimeScheduleTypeFactor");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeScheduleTypeSmallDTO dto = new TimeScheduleTypeSmallDTO()
            {
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                Code = e.Code,
                Name = e.Name
            };

            if (includeFactors)
                dto.Factors = e.TimeScheduleTypeFactor?.Where(f => f.State == (int)SoeEntityState.Active).ToSmallDTOs().ToList() ?? new List<TimeScheduleTypeFactorSmallDTO>();

            return dto;
        }

        public static IEnumerable<TimeScheduleTypeSmallDTO> ToSmallDTOs(this IEnumerable<TimeScheduleType> l, bool includeFactors)
        {
            var dtos = new List<TimeScheduleTypeSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO(includeFactors));
                }
            }
            return dtos;
        }

        public static TimeScheduleTypeFactorDTO ToDTO(this TimeScheduleTypeFactor e)
        {
            if (e == null)
                return null;

            return new TimeScheduleTypeFactorDTO()
            {
                TimeScheduleTypeFactorId = e.TimeScheduleTypeFactorId,
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                Factor = e.Factor,
                FromTime = e.FromTime,
                ToTime = e.ToTime,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<TimeScheduleTypeFactorDTO> ToDTOs(this IEnumerable<TimeScheduleTypeFactor> l)
        {
            var dtos = new List<TimeScheduleTypeFactorDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeScheduleTypeFactorSmallDTO ToSmallDTO(this TimeScheduleTypeFactor e)
        {
            if (e == null)
                return null;

            return new TimeScheduleTypeFactorSmallDTO()
            {
                Factor = e.Factor,
                FromTime = e.FromTime,
                ToTime = e.ToTime,
            };
        }

        public static IEnumerable<TimeScheduleTypeFactorSmallDTO> ToSmallDTOs(this IEnumerable<TimeScheduleTypeFactor> l)
        {
            var dtos = new List<TimeScheduleTypeFactorSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static List<int> GetTimeScheduleTypeIdsIsNotScheduleTime(this List<TimeScheduleType> l)
        {
            return l?.Where(e => e.IsNotScheduleTime).Select(e => e.TimeScheduleTypeId).ToList() ?? new List<int>();
        }

        public static List<int> GetTimeScheduleTypeIdsFactorZero(this List<TimeScheduleType> l, DateTime startTime, DateTime stopTime)
        {
            return l?.Where(e => e.UseScheduleTimeFactor && e.ContainsFactor(startTime, stopTime, 0)).Select(e => e.TimeScheduleTypeId).ToList() ?? new List<int>();
        }

        public static bool ContainsFactor(this TimeScheduleType e, DateTime startTime, DateTime stopTime, decimal factor)
        {
            return e?.TimeScheduleTypeFactor != null && e.TimeScheduleTypeFactor.Any(f => f.State == (int)SoeEntityState.Active && f.Factor == factor && f.IsFactorValid(startTime, stopTime));
        }

        #endregion

        #region TimeScheduleTypeFactor

        public static bool IsFactorValid(this TimeScheduleTypeFactor e, DateTime startTime, DateTime stopTime)
        {
            return CalendarUtility.IsCurrentOverlappedByNew(e.FromTime, e.ToTime, startTime, stopTime);
        }

        #endregion
    }
}

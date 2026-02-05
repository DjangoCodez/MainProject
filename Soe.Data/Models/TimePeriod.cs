using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimePeriod : ICreatedModified
    {
        public bool ShowAsDefault { get; set; }

        public (DateTime? startDate, DateTime? stopDate) GetDates()
        {
            if (this.ExtraPeriod)
                return (null, null);
            else
                return (this.StartDate, this.StopDate);
        }
        public (DateTime startDate, DateTime stopDate) GetRetroactiveDates()
        {
            if (this.ExtraPeriod)
            {
                DateTime paymentDate = this.PaymentDate ?? this.StopDate;
                return (paymentDate.AddYears(-1), paymentDate);
            }
            else
                return (this.StartDate, this.StopDate);
        }

        public int Days()
        {
            return (int)(this.StopDate - this.StartDate).TotalDays + 1;
        }
    }

    public partial class TimePeriodAccountValue : ICreatedModified
    {

    }

    public partial class TimePeriodHead : ICreatedModified
    {
        public string TimePeriodTypeName { get; set; }
        public string AccountName { get; set; }
        public string ChildName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimePeriod

        public static TimePeriodDTO ToDTO(this TimePeriod e)
        {
            if (e == null)
                return null;

            TimePeriodDTO dto = new TimePeriodDTO()
            {
                TimePeriodId = e.TimePeriodId,
                TimePeriodHeadId = e.TimePeriodHead?.TimePeriodHeadId ?? 0,
                RowNr = e.RowNr,
                Name = e.Name,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                PayrollStartDate = e.PayrollStartDate,
                PayrollStopDate = e.PayrollStopDate,
                PaymentDate = e.PaymentDate,
                ExtraPeriod = e.ExtraPeriod,
                ShowAsDefault = e.ShowAsDefault,
                Comment = e.Comment,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            if (e.TimePeriodHead != null)
                dto.TimePeriodHead = e.TimePeriodHead.ToDTO(false);

            return dto;
        }

        public static IEnumerable<TimePeriodDTO> ToDTOs(this IEnumerable<TimePeriod> l)
        {
            var dtos = new List<TimePeriodDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimePeriod GetTimePeriod(this List<TimePeriod> l, DateTime paymentDate)
        {
            return l.FirstOrDefault(x => x.PaymentDate.HasValue && x.PaymentDate.Value == paymentDate.Date);
        }

        public static bool IsExtraPeriod(this TimePeriod e)
        {
            return e?.ExtraPeriod ?? false;
        }

        public static bool HasPayrollDates(this TimePeriod e)
        {
            return e.PayrollStartDate.HasValue && e.PayrollStopDate.HasValue;
        }

        public static bool IsValid(this TimePeriod e)
        {
            return e != null;
        }

        #endregion

        #region TimePeriodHead

        public static TimePeriodHeadDTO ToDTO(this TimePeriodHead e, bool includeTimePeriods)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeTimePeriods && !e.TimePeriod.IsLoaded)
                {
                    e.TimePeriod.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("TimePeriod.cs e.TimePeriod");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimePeriodHeadDTO dto = new TimePeriodHeadDTO()
            {
                TimePeriodHeadId = e.TimePeriodHeadId,
                ActorCompanyId = e.ActorCompanyId,
                TimePeriodType = (TermGroup_TimePeriodType)e.TimePeriodType,
                Name = e.Name,
                Description = e.Description,
                AccountId = e.AccountId,
                ChildId = e.ChildId,
                TimePeriodTypeName = e.TimePeriodTypeName,
                PayrollProductDistributionRuleHeadId = e.PayrollProductDistributionRuleHeadId, 
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (includeTimePeriods)
                dto.TimePeriods = e.TimePeriod.Where(w=> w.State == (int)SoeEntityState.Active).ToDTOs().OrderByDescending(p => p.StartDate).ToList();

            return dto;
        }

        public static IEnumerable<TimePeriodHeadDTO> ToDTOs(this IEnumerable<TimePeriodHead> l, bool includeTimePeriods)
        {
            var dtos = new List<TimePeriodHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeTimePeriods));
                }
            }
            return dtos;
        }

        public static TimePeriodHeadGridDTO ToGridDTO(this TimePeriodHead e)
        {
            if (e == null)
                return null;

            return new TimePeriodHeadGridDTO()
            {
                TimePeriodHeadId = e.TimePeriodHeadId,
                TimePeriodType = (TermGroup_TimePeriodType)e.TimePeriodType,
                Name = e.Name,
                Description = e.Description,
                TimePeriodTypeName = e.TimePeriodTypeName,
                AccountName = e.AccountName,
                ChildName = e.ChildName,
                PayrollProductDistributionRuleHeadId = e.PayrollProductDistributionRuleHeadId,
            };
        }

        public static IEnumerable<TimePeriodHeadGridDTO> ToGridDTOs(this IEnumerable<TimePeriodHead> l)
        {
            var dtos = new List<TimePeriodHeadGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region TimePeriodAccountValue

        public static IEnumerable<TimePeriodAccountValueDTO> ToDTOs(this IEnumerable<TimePeriodAccountValue> l)
        {
            var dtos = new List<TimePeriodAccountValueDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimePeriodAccountValueDTO ToDTO(this TimePeriodAccountValue e)
        {
            if (e == null)
                return null;

            return new TimePeriodAccountValueDTO()
            {
                TimePeriodAccountValueId = e.TimePeriodAccountValueId,
                ActorCompanyId = e.ActorCompanyId,
                TimePeriodId = e.TimePeriodId,
                AccountId = e.AccountId,
                Type = (SoeTimePeriodAccountValueType)e.Type,
                Status = (SoeTimePeriodAccountValueStatus)e.Status,
                Value = e.Value,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };
        }

        public static IEnumerable<TimePeriodAccountValueGridDTO> ToGridDTOs(this IEnumerable<TimePeriodAccountValue> l)
        {
            var dtos = new List<TimePeriodAccountValueGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static TimePeriodAccountValueGridDTO ToGridDTO(this TimePeriodAccountValue e)
        {
            if (e == null)
                return null;

            return new TimePeriodAccountValueGridDTO()
            {
                TimePeriodAccountValueId = e.TimePeriodAccountValueId,
                ActorCompanyId = e.ActorCompanyId,
                TimePeriodId = e.TimePeriodId,
                AccountId = e.AccountId,
                Type = (SoeTimePeriodAccountValueType)e.Type,
                Status = (SoeTimePeriodAccountValueStatus)e.Status,
                Value = e.Value,
                IsModified = false,
            };
        }

        #endregion
    }
}

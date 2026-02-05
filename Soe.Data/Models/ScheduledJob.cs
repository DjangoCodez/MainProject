using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ScheduledJobLog
    {
        public string LogLevelName { get; set; }
        public string StatusName { get; set; }
    }

    public partial class ScheduledJobHead : ICreatedModified, IState
    {
        public string RecurrenceIntervalText { get; set; }
        public string TimeIntervalText { get; set; }
    }

    public partial class ScheduledJobRow : ICreatedModified, IState
    {
        public string RecurrenceIntervalText { get; set; }
        public string TimeIntervalText { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region ScheduledJob

        #region ScheduledJobHead

        public static ScheduledJobHeadDTO ToDTO(this ScheduledJobHead e)
        {
            if (e == null)
                return null;

            ScheduledJobHeadDTO dto = new ScheduledJobHeadDTO()
            {
                ScheduledJobHeadId = e.ScheduledJobHeadId,
                ParentId = e.ParentId,
                Name = e.Name,
                Description = e.Description,
                Sort = e.Sort,
                SharedOnLicense = e.SharedOnLicense,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (!e.ScheduledJobRow.IsNullOrEmpty())
                dto.Rows = e.ScheduledJobRow.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs();

            if (!e.ScheduledJobLog.IsNullOrEmpty())
                dto.Logs = e.ScheduledJobLog.ToDTOs();

            if (!e.ScheduledJobSetting.IsNullOrEmpty())
                dto.Settings = e.ScheduledJobSetting.Where(r => r.State == (int)SoeEntityState.Active).OrderBy(s => s.Type).ToDTOs();

            return dto;
        }

        public static List<ScheduledJobHeadDTO> ToDTOs(this IEnumerable<ScheduledJobHead> l)
        {
            List<ScheduledJobHeadDTO> dtos = new List<ScheduledJobHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ScheduledJobHeadGridDTO ToGridDTO(this ScheduledJobHead e)
        {
            if (e == null)
                return null;

            return new ScheduledJobHeadGridDTO()
            {
                ScheduledJobHeadId = e.ScheduledJobHeadId,
                Name = e.Name,
                Description = e.Description,
                Sort = e.Sort,
                SharedOnLicense = e.SharedOnLicense,
                State = (SoeEntityState)e.State,
            };
        }

        public static List<ScheduledJobHeadGridDTO> ToGridDTOs(this IEnumerable<ScheduledJobHead> l)
        {
            List<ScheduledJobHeadGridDTO> dtos = new List<ScheduledJobHeadGridDTO>();
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

        #region ScheduledJobRow

        public static ScheduledJobRowDTO ToDTO(this ScheduledJobRow e)
        {
            if (e == null)
                return null;

            return new ScheduledJobRowDTO()
            {
                ScheduledJobRowId = e.ScheduledJobRowId,
                ScheduledJobHeadId = e.ScheduledJobHeadId,
                RecurrenceInterval = e.RecurrenceInterval,
                SysTimeIntervalId = e.SysTimeIntervalId,
                NextExecutionTime = e.NextExecutionTime,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                RecurrenceIntervalText = e.RecurrenceIntervalText,
                TimeIntervalText = e.TimeIntervalText,
            };
        }

        public static List<ScheduledJobRowDTO> ToDTOs(this IEnumerable<ScheduledJobRow> l)
        {
            List<ScheduledJobRowDTO> dtos = new List<ScheduledJobRowDTO>();
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

        #region ScheduledJobLog

        public static ScheduledJobLogDTO ToDTO(this ScheduledJobLog e)
        {
            if (e == null)
                return null;

            return new ScheduledJobLogDTO()
            {
                ScheduledJobLogId = e.ScheduledJobLogId,
                ScheduledJobHeadId = e.ScheduledJobHeadId,
                ScheduledJobRowId = e.ScheduledJobRowId,
                BatchNr = e.BatchNr,
                Status = (TermGroup_ScheduledJobLogStatus)e.Status,
                LogLevel = (TermGroup_ScheduledJobLogLevel)e.LogLevel,
                Time = e.Time,
                Message = e.Message,
                LogLevelName = e.LogLevelName,
                StatusName = e.StatusName,
            };
        }

        public static List<ScheduledJobLogDTO> ToDTOs(this IEnumerable<ScheduledJobLog> l)
        {
            List<ScheduledJobLogDTO> dtos = new List<ScheduledJobLogDTO>();
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

        #region ScheduledJobSetting

        public static ScheduledJobSettingDTO ToDTO(this ScheduledJobSetting e)
        {
            if (e == null)
                return null;

            return new ScheduledJobSettingDTO()
            {
                ScheduledJobSettingId = e.ScheduledJobSettingId,
                ScheduledJobHeadId = e.ScheduledJobHeadId,
                Type = (TermGroup_ScheduledJobSettingType)e.Type,
                DataType = (SettingDataType)e.DataType,
                Name = e.Name,
                StrData = e.StrData,
                IntData = e.IntData,
                DecimalData = e.DecimalData,
                BoolData = e.BoolData,
                DateData = e.DateData,
                TimeData = e.TimeData,
                State = (SoeEntityState)e.State,
                Options = e.Options
            };
        }

        public static List<ScheduledJobSettingDTO> ToDTOs(this IEnumerable<ScheduledJobSetting> l)
        {
            List<ScheduledJobSettingDTO> dtos = new List<ScheduledJobSettingDTO>();
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

        #endregion
    }
}

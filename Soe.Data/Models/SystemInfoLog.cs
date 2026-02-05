using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class SystemInfoLog : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region SystemInfoLog

        public static SystemInfoLogDTO ToDTO(this SystemInfoLog e)
        {
            if (e == null)
                return null;

            return new SystemInfoLogDTO()
            {
                SystemInfoLogId = e.SystemInfoLogId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                LogLevel = (SystemInfoLogLevel)e.LogLevel,
                Type = (SystemInfoType)e.Type,
                Entity = (SoeEntityType)e.Entity,
                RecordId = e.RecordId,
                Date = e.Date,
                Text = e.Text,
                DeleteManually = e.DeleteManually,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<SystemInfoLogDTO> ToDTOs(this IEnumerable<SystemInfoLog> l)
        {
            var dtos = new List<SystemInfoLogDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool HasSystemInfoLogs(this List<SystemInfoLog> l, int recordId, DateTime fromDate, DateTime toDate, int? employeeId = null, string text = null)
        {
            return l?.FilterSystemInfoLogs(recordId, fromDate, toDate, employeeId, text).Any() ?? false;
        }

        public static List<SystemInfoLog> FilterSystemInfoLogs(this List<SystemInfoLog> l, int recordId, DateTime fromDate, DateTime toDate, int? employeeId = null, string text = null)
        {
            return l?.Where(e => e.RecordId == recordId && e.Date >= fromDate && e.Date <= toDate && (!employeeId.HasValue || e.EmployeeId == employeeId.Value) && (string.IsNullOrEmpty(text) || text.Equals(e.Text))).ToList() ?? new List<SystemInfoLog>();
        }

        #endregion
    }
}

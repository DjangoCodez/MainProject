using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeRequest : ICreatedModified, IState
    {
        public string EmployeeName
        {
            get { return this.Employee?.Name ?? string.Empty; }
        }
        public string EmployeeNrAndName
        {
            get { return this.Employee?.EmployeeNrAndName ?? string.Empty; }
        }
        public string EmployeeChildFirstName
        {
            get { return this.EmployeeChild?.FirstName ?? string.Empty; }
        }
        public string TimeDeviationCauseName
        {
            get { return this.TimeDeviationCause?.Name ?? string.Empty; }
        }

        public string StatusName { get; set; }
        public string ResultStatusName { get; set; }
        public bool RequestIntersectsWithCurrent { get; set; }
        public string IntersectMessage { get; set; }
        public List<string> CategoryNames { get; set; }
        public string CategoryNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(CategoryNames, addWhiteSpace: true);
            }
        }

        public bool IsWholeDay
        {
            get
            {
                return Start.Date == Stop.Date && Start == CalendarUtility.GetBeginningOfDay(Start) && Stop == CalendarUtility.GetEndOfDay(Stop);
            }
        }

        #region Accounts

        public List<string> AccountNames { get; set; }
        public string AccountNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(AccountNames, addWhiteSpace: true);
            }
        }

        #endregion
    }

    public static partial class EntityExtensions
    {
        #region EmployeeRequest

        public static EmployeeRequestDTO ToDTO(this EmployeeRequest e, bool includeExtendedSettings, bool includeEmployeeChild)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeExtendedSettings && !e.ExtendedAbsenceSettingReference.IsLoaded)
                    {
                        e.ExtendedAbsenceSettingReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeRequest.cs e.SkillReference");
                    }

                    if (includeEmployeeChild && !e.EmployeeChildReference.IsLoaded)
                    { 
                        e.EmployeeChildReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeRequest.cs e.EmployeeChildReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            EmployeeRequestDTO dto = new EmployeeRequestDTO()
            {
                EmployeeRequestId = e.EmployeeRequestId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                Type = (TermGroup_EmployeeRequestType)e.Type,
                Status = (TermGroup_EmployeeRequestStatus)e.Status,
                ResultStatus = (TermGroup_EmployeeRequestResultStatus)e.ResultStatus,
                Start = e.Start,
                Stop = e.Stop,
                StartString = (e.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest) ? e.Start.ToShortDateString() : e.Start.ToShortDateShortTimeString(),
                StopString = (e.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest) ? e.Stop.ToShortDateString() : e.Stop.ToShortDateShortTimeString(),
                Comment = e.Comment,
                Created = e.Created,
                CreatedString = e.Created.HasValue ? e.Created.Value.ToShortDateString() : String.Empty,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                RequestIntersectsWithCurrent = e.RequestIntersectsWithCurrent,
                IntersectMessage = e.IntersectMessage,
                EmployeeChildId = e.EmployeeChildId,
                EmployeeName = e.EmployeeNrAndName,
                TimeDeviationCauseName = e.TimeDeviationCauseName,
                StatusName = e.StatusName,
                ResultStatusName = e.ResultStatusName,
                EmployeeChildName = e.EmployeeChildFirstName,
                CategoryNamesString = e.CategoryNamesString,
                AccountNamesString = e.AccountNamesString,
                ReActivate = e.ReActivate,
            };

            if (includeExtendedSettings && e.ExtendedAbsenceSetting != null)
                dto.ExtendedSettings = e.ExtendedAbsenceSetting.ToDTO();

            return dto;
        }

        public static EmployeeRequestDTO ToDTONew(this EmployeeRequest e, bool includeExtendedSettings, bool includeEmployeeChild)
        {
            if (e == null)
                return null;

            EmployeeRequestDTO dto = new EmployeeRequestDTO()
            {
                EmployeeRequestId = e.EmployeeRequestId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                Type = (TermGroup_EmployeeRequestType)e.Type,
                Status = (TermGroup_EmployeeRequestStatus)e.Status,
                ResultStatus = (TermGroup_EmployeeRequestResultStatus)e.ResultStatus,
                Start = e.Start,
                Stop = e.Stop,
                StartString = (e.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest) ? e.Start.ToShortDateString() : e.Start.ToShortDateShortTimeString(),
                StopString = (e.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest) ? e.Stop.ToShortDateString() : e.Stop.ToShortDateShortTimeString(),
                Comment = e.Comment,
                Created = e.Created,
                CreatedString = e.Created.HasValue ? e.Created.Value.ToShortDateString() : String.Empty,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                RequestIntersectsWithCurrent = e.RequestIntersectsWithCurrent,
                IntersectMessage = e.IntersectMessage,
                EmployeeChildId = e.EmployeeChildId,
                EmployeeName = e.EmployeeNrAndName,
                TimeDeviationCauseName = e.TimeDeviationCauseName,
                StatusName = e.StatusName,
                ResultStatusName = e.ResultStatusName,
                EmployeeChildName = e.EmployeeChildFirstName,
                //CategoryNamesString = e.CategoryNamesString,
                //AccountNamesString = e.AccountNamesString,
                ReActivate = e.ReActivate,
            };

            if (includeExtendedSettings && e.ExtendedAbsenceSetting != null)
                dto.ExtendedSettings = e.ExtendedAbsenceSetting.ToDTO();

            return dto;
        }

        public static IEnumerable<EmployeeRequestDTO> ToDTOs(this IEnumerable<EmployeeRequest> l)
        {
            var dtos = new List<EmployeeRequestDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(false, false));
                }
            }
            return dtos;
        }

        public static EmployeeRequestGridDTO ToGridDTO(this EmployeeRequest e, bool includeExtendedSettings, bool includeEmployeeChild)
        {
            if (e == null)
                return null;

            EmployeeRequestGridDTO dto = new EmployeeRequestGridDTO()
            {
                EmployeeRequestId = e.EmployeeRequestId,
                //ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                Type = (TermGroup_EmployeeRequestType)e.Type,
                Status = (TermGroup_EmployeeRequestStatus)e.Status,
                ResultStatus = (TermGroup_EmployeeRequestResultStatus)e.ResultStatus,
                Start = e.Start,
                Stop = e.Stop,
                //StartString = (e.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest) ? e.Start.ToShortDateString() : e.Start.ToShortDateShortTimeString(),
                //StopString = (e.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest) ? e.Stop.ToShortDateString() : e.Stop.ToShortDateShortTimeString(),
                Comment = e.Comment,
                Created = e.Created,
                //CreatedString = e.Created.HasValue ? e.Created.Value.ToShortDateString() : String.Empty,
                //CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                //ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                //RequestIntersectsWithCurrent = e.RequestIntersectsWithCurrent,
                //IntersectMessage = e.IntersectMessage,
                //EmployeeChildId = e.EmployeeChildId,
                EmployeeName = e.EmployeeNrAndName,
                //TimeDeviationCauseName = e.TimeDeviationCauseName,
                //StatusName = e.StatusName,
                //ResultStatusName = e.ResultStatusName,
                //EmployeeChildName = e.EmployeeChildFirstName,
                CategoryNames = e.CategoryNames,
                AccountNames = e.AccountNames,
                //CategoryNamesString = e.CategoryNamesString,
                //AccountNamesString = e.AccountNamesString,
                //ReActivate = e.ReActivate,
            };

            //if (includeExtendedSettings && e.ExtendedAbsenceSetting != null)
            //    dto.ExtendedSettings = e.ExtendedAbsenceSetting.ToDTO();

            return dto;
        }

        public static IEnumerable<EmployeeRequestGridDTO> ToGridDTOs(this IEnumerable<EmployeeRequest> l)
        {
            var dtos = new List<EmployeeRequestGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(false, false));
                }
            }
            return dtos;
        }

        public static EmployeeRequest FromDTO(this EmployeeRequestDTO dto)
        {
            if (dto == null)
                return null;

            EmployeeRequest e = new EmployeeRequest()
            {
                EmployeeRequestId = dto.EmployeeRequestId,
                ActorCompanyId = dto.ActorCompanyId,
                EmployeeId = dto.EmployeeId,
                TimeDeviationCauseId = dto.TimeDeviationCauseId,
                Type = (int)dto.Type,
                Status = (int)dto.Status,
                ResultStatus = (int)dto.ResultStatus,
                Start = dto.Start,
                Stop = dto.Stop,
                Comment = dto.Comment,
                Created = dto.Created,
                CreatedBy = dto.CreatedBy,
                Modified = dto.Modified,
                ModifiedBy = dto.ModifiedBy,
                State = (int)dto.State,
                EmployeeChildId = dto.EmployeeChildId,
                ReActivate = dto.ReActivate,
            };

            if (dto.ExtendedSettings != null)
                e.ExtendedAbsenceSetting = dto.ExtendedSettings.FromDTO();
            return e;
        }

        public static EmployeeRequestDTO CopyAsNew(this EmployeeRequest e, DateTime start, DateTime stop, TermGroup_EmployeeRequestStatus status = TermGroup_EmployeeRequestStatus.RequestPending, TermGroup_EmployeeRequestResultStatus resultStatus = TermGroup_EmployeeRequestResultStatus.None)
        {
            var dto = e.ToDTO(true, false);
            dto.EmployeeRequestId = 0;
            if (dto.ExtendedSettings != null)
                dto.ExtendedSettings.ExtendedAbsenceSettingId = 0;

            dto.Start = start;
            dto.Stop = stop;
            dto.Status = status;
            dto.ResultStatus = resultStatus;

            return dto;
        }

        public static decimal? GetRatio(this EmployeeRequest e)
        {
            if (e?.ExtendedAbsenceSetting == null || !e.ExtendedAbsenceSetting.PercentalAbsence)
                return null;
            return e.ExtendedAbsenceSetting.PercentalValue;
        }

        #endregion
    }
}

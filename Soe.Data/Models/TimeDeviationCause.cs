using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeDeviationCause : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public string CodeAndName
        {
            get 
            {
                if (this.ExtCode.IsNullOrEmpty())
                    return this.Name;
                return $"{this.ExtCode} {this.Name}";
            }
        }
        public bool IsAbsence
        {
            get { return (this.Type == (int)TermGroup_TimeDeviationCauseType.Absence) || (this.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence); }
        }
        public bool IsPresence
        {
            get { return (this.Type == (int)TermGroup_TimeDeviationCauseType.Presence) || (this.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence); }
        }
        public bool HasAttachZeroDaysNbrOfDaySetting
        {
            get
            {
                return this.AttachZeroDaysNbrOfDaysAfter != 0 || this.AttachZeroDaysNbrOfDaysBefore != 0;
            }
        }

        public List<string> ExternalCodes
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ExtCode))
                {
                    if (this.ExtCode.Contains("#"))
                        return this.ExtCode.Split('#').ToList();
                    else
                        return new List<string>() { this.ExtCode };
                }
                return new List<string>();
            }
        }
        public string ExternalCodesString { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeDeviationCause

        public static TimeDeviationCauseDTO ToDTO(this TimeDeviationCause e)
        {
            if (e == null)
                return null;

            var dto = new TimeDeviationCauseDTO
            {
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                ActorCompanyId = e.ActorCompanyId,
                TimeCodeId = e.TimeCodeId,
                TimeCode = e.TimeCode?.ToDTO(),
                TimeCodeName = e.TimeCode?.Name,
                Type = (TermGroup_TimeDeviationCauseType)e.Type,
                TypeName = e.TypeName,
                Name = e.Name,
                Description = e.Description,
                ExtCode = e.ExtCode,
                ImageSource = e.ImageSource,
                EmployeeRequestPolicyNbrOfDaysBefore = e.EmployeeRequestPolicyNbrOfDaysBefore,
                EmployeeRequestPolicyNbrOfDaysBeforeCanOverride = e.EmployeeRequestPolicyNbrOfDaysBeforeCanOverride,
                AttachZeroDaysNbrOfDaysAfter = e.AttachZeroDaysNbrOfDaysAfter,
                AttachZeroDaysNbrOfDaysBefore = e.AttachZeroDaysNbrOfDaysBefore,
                ChangeCauseOutsideOfPlannedAbsence = e.ChangeCauseOutsideOfPlannedAbsence,
                ChangeCauseInsideOfPlannedAbsence = e.ChangeCauseInsideOfPlannedAbsence,
                ChangeDeviationCauseAccordingToPlannedAbsence = e.ChangeDeviationCauseAccordingToPlannedAbsence,
                AdjustTimeOutsideOfPlannedAbsence = e.AdjustTimeOutsideOfPlannedAbsence,
                AdjustTimeInsideOfPlannedAbsence = e.AdjustTimeInsideOfPlannedAbsence,
                AllowGapToPlannedAbsence = e.AllowGapToPlannedAbsence,
                ShowZeroDaysInAbsencePlanning = e.ShowZeroDaysInAbsencePlanning,
                CalculateAsOtherTimeInSales = e.CalculateAsOtherTimeInSales,
                IsAbsence = e.IsAbsence,
                IsPresence = e.IsPresence,
                IsVacation = e.IsVacation,
                Payed = e.Payed,
                NotChargeable = e.NotChargeable,
                OnlyWholeDay = e.OnlyWholeDay,
                SpecifyChild = e.SpecifyChild,
                ExcludeFromPresenceWorkRules = e.ExcludeFromPresenceWorkRules,
                ExcludeFromScheduleWorkRules = e.ExcludeFromScheduleWorkRules,
                ValidForHibernating = e.ValidForHibernating,
                ValidForStandby = e.ValidForStandby,
                CandidateForOvertime = e.CandidateForOvertime,
                MandatoryNote = e.MandatoryNote,
                MandatoryTime = e.MandatoryTime,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.EmployeeGroupTimeDeviationCause != null)
            {
                dto.EmployeeGroupIds = new List<int>();
                foreach (EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCause in e.EmployeeGroupTimeDeviationCause.Where(d => d.State == (int)SoeEntityState.Active))
                {
                    dto.EmployeeGroupIds.Add(employeeGroupTimeDeviationCause.EmployeeGroupId);
                }
            }

            return dto;
        }

        public static TimeDeviationCauseGridDTO ToGridDTO(this TimeDeviationCause e)
        {
            if (e == null)
                return null;

            return new TimeDeviationCauseGridDTO
            {
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                Type = (TermGroup_TimeDeviationCauseType)e.Type,
                Name = e.Name,
                Description = e.Description,
                ImageSource = e.ImageSource,
                SpecifyChild = e.SpecifyChild,
                ValidForStandby = e.ValidForStandby,
                MandatoryNote = e.MandatoryNote,
                TypeName = e.TypeName,
                TimeCodeName = e.TimeCode?.Name ?? string.Empty,
                ValidForHibernating = e.ValidForHibernating,
                CandidateForOvertime = e.CandidateForOvertime,
            };
        }

        public static IEnumerable<TimeDeviationCauseDTO> ToDTOs(this IEnumerable<TimeDeviationCause> l)
        {
            var dtos = new List<TimeDeviationCauseDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<TimeDeviationCauseGridDTO> ToGridDTOs(this IEnumerable<TimeDeviationCause> l)
        {
            var dtos = new List<TimeDeviationCauseGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static Dictionary<int, string> ToDict(this List<TimeDeviationCause> timeDeviationCauses, bool addEmptyRow = false, bool removeAbsence = false, bool removePresence = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");
            if (timeDeviationCauses.IsNullOrEmpty())
                return dict;

            foreach (TimeDeviationCause timeDeviationCause in timeDeviationCauses)
            {
                //if (!removeAbsence || timeDeviationCause.IsPresence)
                //    dict.Add(timeDeviationCause.TimeDeviationCauseId, timeDeviationCause.Name);

                if ((timeDeviationCause.IsAbsence && !removeAbsence) || (timeDeviationCause.IsPresence && !removePresence))
                    dict.Add(timeDeviationCause.TimeDeviationCauseId, timeDeviationCause.Name);
            }

            return dict.Sort();
        }

        public static List<TimeDeviationCause> Filter(this List<TimeDeviationCause> l, TermGroup_TimeDeviationCauseType type)
        {
            return l.Where(i => i.Type == (int)type && !i.OnlyWholeDay).ToList();
        }

        public static List<TimeDeviationCause> SortTimeDeviationCausesByType(this List<TimeDeviationCause> l)
        {
            return l?.OrderByDescending(i => i.Type).ThenBy(i => i.Name).ToList() ?? new List<TimeDeviationCause>();
        }

        public static List<TimeDeviationCause> SortTimeDeviationCausesByName(this List<TimeDeviationCause> l)
        {
            return l?.OrderBy(t => t.Name).ToList() ?? new List<TimeDeviationCause>();
        }

        public static List<TimeDeviationCause> SortTimeDeviationCausesByStandard(this List<TimeDeviationCause> l, string standardText = "standard")
        {
            standardText = standardText.ToLower();
            TimeDeviationCause standardItem = l?.FirstOrDefault(x => x.Name.ToLower() == standardText);
            if (standardItem != null)
            {
                l.Remove(standardItem);
                l.Insert(0, standardItem);
            }
            return l;
        }

        public static List<string> GetNames(this List<TimeDeviationCause> l, List<int> timeDeviationCauseIds, int? excludeTimeDeviationCauseId = null)
        {
            if (l.IsNullOrEmpty() || timeDeviationCauseIds.IsNullOrEmpty())
                return new List<string>();

            List<string> names = new List<string>();

            foreach (var timeDeviationCauseId in timeDeviationCauseIds)
            {
                if (excludeTimeDeviationCauseId.HasValue && excludeTimeDeviationCauseId.Value == timeDeviationCauseId)
                    continue;

                string name = l.FirstOrDefault(i => i.TimeDeviationCauseId == timeDeviationCauseId)?.Name;
                if (!string.IsNullOrEmpty(name) && !names.Contains(name))
                    names.Add(name);
            }

            return names;
        }

        public static Dictionary<string, TimeSpan> ConvertToNameDict(this List<TimeDeviationCause> l, Dictionary<int, TimeSpan> idDict)
        {
            if (l.IsNullOrEmpty() || idDict.IsNullOrEmpty())
                return new Dictionary<string, TimeSpan>();

            Dictionary<string, TimeSpan> strDict = new Dictionary<string, TimeSpan>();

            foreach (var pair in idDict)
            {
                TimeDeviationCause e = l.FirstOrDefault(tdc => tdc.TimeDeviationCauseId == pair.Key);
                if (e != null)
                    strDict.AddTime(e.Name, pair.Value);
            }

            return strDict;
        }

        public static List<int> GetAbsenceDeviationCauseIds(this List<TimeDeviationCause> l)
        {
            return l?.Where(t => t.Type == (int)TermGroup_TimeDeviationCauseType.Absence).Select(t => t.TimeDeviationCauseId).ToList() ?? new List<int>();
        }

        public static List<int> GetOvertimeDeviationCauseIds(this List<TimeDeviationCause> l)
        {
            return l?.Where(t => t.CandidateForOvertime).Select(t => t.TimeDeviationCauseId).ToList() ?? new List<int>();
        }

        public static bool IsWholeDayAbsence(this List<TimeDeviationCause> l, int timeDeviationCauseId)
        {
            var e = l?.FirstOrDefault(i => i.TimeDeviationCauseId == timeDeviationCauseId);
            return e != null && e.Type == (int)TermGroup_TimeDeviationCauseType.Absence && e.OnlyWholeDay;
        }

        public static bool IsStandby(this TimeDeviationCause e, EmployeeGroup employeeGroup)
        {
            return e != null && (e.ValidForStandby || e.TimeDeviationCauseId == employeeGroup?.TimeDeviationCauseId);
        }

        #endregion
    }
}

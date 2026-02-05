using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class ScheduleTimer : ImportExportManager
    {
        private List<ShiftType> shiftTypes = new List<ShiftType>();
        private List<AccountInternal> accounts = new List<AccountInternal>();
        private List<TimeDeviationCause> causes = new List<TimeDeviationCause>();

        public ScheduleTimer(ParameterObject parameterObject) : base(parameterObject) { }

        public List<dynamic> GetTimeScheduleTemplateBlockIODTOs(string content, bool isTemplate)
        {
            List<dynamic> dtos = new List<dynamic>();

            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(base.ActorCompanyId, onlyInternal: true);
            AccountDim accountDim = accountDims.FirstOrDefault(a => a.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);
            if (accountDim == null)
                return null;
            this.accounts = AccountManager.GetAccountInternalsByDim(accountDim.AccountDimId, base.ActorCompanyId);
            this.shiftTypes = TimeScheduleManager.GetShiftTypes(base.ActorCompanyId, loadAccounts: true);

            List<string> lines = new List<string>();
            string line;

            using (StringReader reader = new StringReader(content))
            {
                int row = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (row != 0)
                    {
                        lines.Add(line);
                    }
                    row++;
                }
            }

            foreach (var scheduleLine in lines)
            {
                var split = scheduleLine.Split(';').ToList();

                if (split.Count < 10)
                    continue;

                var UnitNr = split[0];
                var EmpNr = split[1];
                var type = split[2];
                var date1 = split[3];
                var date2 = split[4];
                var Start = split[5];
                var stop = split[6];
                var amount = split[7];
                // var breakTime = split[8];
                var breakBlock = split[9];

                if (type != "P")
                    continue;

                DateTime dateT = CalendarUtility.GetDateTime(date1, "yyyy-MM-dd");
                var startTime = dateT.Add(CalendarUtility.TextToTimeSpan(Start));
                var stopTime = dateT.Add(CalendarUtility.TextToTimeSpan(stop));

                DateTime breakStart = CalendarUtility.DATETIME_DEFAULT;
                DateTime breakStop = CalendarUtility.DATETIME_DEFAULT;
                TimeScheduleBlockIODTO timeScheduleBlockIODTO = new TimeScheduleBlockIODTO();
                Guid link = Guid.NewGuid();
                timeScheduleBlockIODTO.TimeScheduleTemplatePeriodId = 0;
                timeScheduleBlockIODTO.TimeScheduleEmployeePeriodId = 0;
                timeScheduleBlockIODTO.EmployeeNr = EmpNr;
                timeScheduleBlockIODTO.TimeScheduleTypeId = 0;
                timeScheduleBlockIODTO.DayNumber = 0;
                timeScheduleBlockIODTO.Description = string.Empty;

                timeScheduleBlockIODTO.StartTime = startTime;
                timeScheduleBlockIODTO.StopTime = stopTime;
                timeScheduleBlockIODTO.LengthMinutes = Convert.ToInt32((stopTime - startTime).TotalMinutes);
                timeScheduleBlockIODTO.Date = dateT;

                DateTime break1StartTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime break2StartTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime break3StartTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime break4StartTime = CalendarUtility.DATETIME_DEFAULT;

                timeScheduleBlockIODTO.Break1Id = 0;
                timeScheduleBlockIODTO.Break1StartTime = break1StartTime;
                timeScheduleBlockIODTO.Break1Minutes = 0;
                timeScheduleBlockIODTO.Break1Link = string.Empty;
                timeScheduleBlockIODTO.Break2Id = 0;
                timeScheduleBlockIODTO.Break2StartTime = break1StartTime;
                timeScheduleBlockIODTO.Break2Minutes = 0;
                timeScheduleBlockIODTO.Break2Link = string.Empty;
                timeScheduleBlockIODTO.Break3Id = 0;
                timeScheduleBlockIODTO.Break3StartTime = break1StartTime;
                timeScheduleBlockIODTO.Break3Minutes = 0;
                timeScheduleBlockIODTO.Break3Link = string.Empty;
                timeScheduleBlockIODTO.Break4Id = 0;
                timeScheduleBlockIODTO.Break4StartTime = break1StartTime;
                timeScheduleBlockIODTO.Break4Minutes = 0;
                timeScheduleBlockIODTO.Break4Link = string.Empty;

                ShiftType shiftType = GetShiftType(UnitNr) ?? GetShiftType("Standard");

                if (shiftType != null)
                    timeScheduleBlockIODTO.ShiftTypeId = shiftType.ShiftTypeId;

                timeScheduleBlockIODTO.ShiftTypeName = string.Empty;
                timeScheduleBlockIODTO.ShiftTypeDescription = string.Empty;
                timeScheduleBlockIODTO.ShiftTypeTimeScheduleTypeId = 0;
                timeScheduleBlockIODTO.Link = link.ToString();
                timeScheduleBlockIODTO.IsTemplate = false;

                AccountInternal account = GetAccount(UnitNr);

                if (account != null)
                    timeScheduleBlockIODTO.HierachyAccountId = account.AccountId;
                timeScheduleBlockIODTO.AccountNr = string.Empty;
                timeScheduleBlockIODTO.AccountDim2Nr = account != null ? account.Account.AccountNr : string.Empty;
                timeScheduleBlockIODTO.AccountDim3Nr = string.Empty;
                timeScheduleBlockIODTO.AccountDim4Nr = string.Empty;
                timeScheduleBlockIODTO.AccountDim5Nr = string.Empty;
                timeScheduleBlockIODTO.AccountDim6Nr = string.Empty;

                if (!string.IsNullOrEmpty(breakBlock))
                {
                    var splitBreak = breakBlock.Split('|').ToList();

                    if (splitBreak.Count == 2)
                    {
                        if (splitBreak[0] != splitBreak[1])
                        {
                            var breakStartTime = CalendarUtility.DATETIME_DEFAULT.Add(CalendarUtility.TextToTimeSpan(splitBreak[0]));
                            var breakStopTime = CalendarUtility.DATETIME_DEFAULT.Add(CalendarUtility.TextToTimeSpan(splitBreak[1]));

                            if (breakStartTime != breakStopTime)
                            {
                                TimeScheduleBlockIODTO breakDTO = new TimeScheduleBlockIODTO();

                                breakDTO.TimeScheduleTemplatePeriodId = 0;
                                breakDTO.TimeScheduleEmployeePeriodId = 0;
                                breakDTO.EmployeeNr = EmpNr;
                                breakDTO.TimeScheduleTypeId = 0;
                                breakDTO.DayNumber = 0;
                                breakDTO.Description = string.Empty;
                                breakDTO.Link = link.ToString();

                                breakDTO.StartTime = breakStartTime;
                                breakDTO.StopTime = breakStopTime;
                                breakDTO.LengthMinutes = Convert.ToInt32((breakStopTime - breakStartTime).TotalMinutes);
                                breakDTO.Date = dateT;
                                breakDTO.IsBreak = true;
                                breakDTO.IsTemplate = false;

                                breakDTO.Break1Id = 0;
                                breakDTO.Break1StartTime = breakStartTime;
                                breakDTO.Break1Minutes = Convert.ToInt32((breakStopTime - breakStartTime).TotalMinutes);
                                breakDTO.Break1Link = string.Empty;
                                breakDTO.Break2Id = 0;
                                breakDTO.Break2StartTime = break1StartTime;
                                breakDTO.Break2Minutes = 0;
                                breakDTO.Break2Link = string.Empty;
                                breakDTO.Break3Id = 0;
                                breakDTO.Break3StartTime = break1StartTime;
                                breakDTO.Break3Minutes = 0;
                                breakDTO.Break3Link = string.Empty;
                                breakDTO.Break4Id = 0;
                                breakDTO.Break4StartTime = break1StartTime;
                                breakDTO.Break4Minutes = 0;
                                breakDTO.Break4Link = string.Empty;

                                breakDTO.ShiftTypeName = string.Empty;
                                breakDTO.ShiftTypeDescription = string.Empty;
                                breakDTO.ShiftTypeTimeScheduleTypeId = 0;
                                breakDTO.Link = link.ToString();

                                breakDTO.AccountNr = string.Empty;
                                breakDTO.AccountDim2Nr = account != null ? account.Account.AccountNr : string.Empty;
                                breakDTO.AccountDim3Nr = string.Empty;
                                breakDTO.AccountDim4Nr = string.Empty;
                                breakDTO.AccountDim5Nr = string.Empty;
                                breakDTO.AccountDim6Nr = string.Empty;

                                dtos.Add(breakDTO);
                            }
                        }
                    }
                }
                dtos.Add(timeScheduleBlockIODTO);
            }

            return dtos;
        }


        public List<dynamic> GetTemplateTimeScheduleTemplateBlockIODTOs(string content)
        {
            List<string> lines = new List<string>();
            string line;

            using (StringReader reader = new StringReader(content))
            {
                int row = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (row != 0)
                    {
                        lines.Add(line);
                    }
                    row++;
                }
            }

            List<dynamic> dtos = new List<dynamic>();
            foreach (var scheduleLine in lines)
            {
                var split = scheduleLine.Split(';').ToList();

                if (split.Count < 9)
                    continue;

                var unitNr = split[0];
                // var templName = split[1];
                // var weeks = split[2];
                var empNr = split[3];
                // var dayIndex = split[4];
                var dayDate = split[5];
                var start = split[6];
                var stop = split[7];
                // var amount = split[8];
                var breakBlock = split[9];


                DateTime dateT = CalendarUtility.GetDateTime(dayDate, "yyyy-MM-dd");
                var startTime = dateT.Add(CalendarUtility.TextToTimeSpan(start));
                var stopTime = dateT.Add(CalendarUtility.TextToTimeSpan(stop));

                // DateTime breakStart = CalendarUtility.DATETIME_DEFAULT;
                // DateTime breakStop = CalendarUtility.DATETIME_DEFAULT;

                TimeScheduleBlockIODTO timeScheduleBlockIODTO = new TimeScheduleBlockIODTO();

                AccountInternal account = GetAccount(unitNr);
                Guid link = Guid.NewGuid();

                timeScheduleBlockIODTO.TimeScheduleTemplatePeriodId = 0;
                timeScheduleBlockIODTO.TimeScheduleEmployeePeriodId = 0;
                timeScheduleBlockIODTO.EmployeeNr = empNr;
                timeScheduleBlockIODTO.TimeScheduleTypeId = 0;
                timeScheduleBlockIODTO.DayNumber = 0;
                timeScheduleBlockIODTO.Description = string.Empty;

                timeScheduleBlockIODTO.StartTime = startTime;
                timeScheduleBlockIODTO.StopTime = stopTime;
                timeScheduleBlockIODTO.LengthMinutes = Convert.ToInt32((stopTime - startTime).TotalMinutes);
                timeScheduleBlockIODTO.Date = dateT;
                timeScheduleBlockIODTO.AccountNr = string.Empty;
                timeScheduleBlockIODTO.AccountDim2Nr = account != null ? account.Account.AccountNr : string.Empty;
                timeScheduleBlockIODTO.AccountDim3Nr = string.Empty;
                timeScheduleBlockIODTO.AccountDim4Nr = string.Empty;
                timeScheduleBlockIODTO.AccountDim5Nr = string.Empty;
                timeScheduleBlockIODTO.AccountDim6Nr = string.Empty;

                DateTime break1StartTime = CalendarUtility.DATETIME_DEFAULT;
                // DateTime break2StartTime = CalendarUtility.DATETIME_DEFAULT;
                // DateTime break3StartTime = CalendarUtility.DATETIME_DEFAULT;
                // DateTime break4StartTime = CalendarUtility.DATETIME_DEFAULT;

                timeScheduleBlockIODTO.Break1Id = 0;
                timeScheduleBlockIODTO.Break1StartTime = break1StartTime;
                timeScheduleBlockIODTO.Break1Minutes = 0;
                timeScheduleBlockIODTO.Break1Link = string.Empty;
                timeScheduleBlockIODTO.Break2Id = 0;
                timeScheduleBlockIODTO.Break2StartTime = break1StartTime;
                timeScheduleBlockIODTO.Break2Minutes = 0;
                timeScheduleBlockIODTO.Break2Link = string.Empty;
                timeScheduleBlockIODTO.Break3Id = 0;
                timeScheduleBlockIODTO.Break3StartTime = break1StartTime;
                timeScheduleBlockIODTO.Break3Minutes = 0;
                timeScheduleBlockIODTO.Break3Link = string.Empty;
                timeScheduleBlockIODTO.Break4Id = 0;
                timeScheduleBlockIODTO.Break4StartTime = break1StartTime;
                timeScheduleBlockIODTO.Break4Minutes = 0;
                timeScheduleBlockIODTO.Break4Link = string.Empty;

                ShiftType shiftType = GetShiftType(unitNr) ?? GetShiftType("Standard");

                if (shiftType != null)
                    timeScheduleBlockIODTO.ShiftTypeId = shiftType.ShiftTypeId;

                timeScheduleBlockIODTO.ShiftTypeName = string.Empty;
                timeScheduleBlockIODTO.ShiftTypeDescription = string.Empty;
                timeScheduleBlockIODTO.ShiftTypeTimeScheduleTypeId = 0;
                timeScheduleBlockIODTO.Link = link.ToString();
                timeScheduleBlockIODTO.IsTemplate = true;


                if (!string.IsNullOrEmpty(breakBlock))
                {
                    var splitBreak = breakBlock.Split('|').ToList();

                    if (splitBreak.Count == 2 && splitBreak[0] != splitBreak[1])
                    {
                        var breakStartTime = CalendarUtility.DATETIME_DEFAULT.Add(CalendarUtility.TextToTimeSpan(splitBreak[0]));
                        var breakStopTime = CalendarUtility.DATETIME_DEFAULT.Add(CalendarUtility.TextToTimeSpan(splitBreak[1]));

                        if (breakStartTime != breakStopTime)
                        {
                            TimeScheduleBlockIODTO breakDTO = new TimeScheduleBlockIODTO();

                            breakDTO.TimeScheduleTemplatePeriodId = 0;
                            breakDTO.TimeScheduleEmployeePeriodId = 0;
                            breakDTO.EmployeeNr = empNr;
                            breakDTO.TimeScheduleTypeId = 0;
                            breakDTO.DayNumber = 0;
                            breakDTO.Description = string.Empty;
                            breakDTO.Link = link.ToString();
                            breakDTO.IsTemplate = true;

                            breakDTO.StartTime = breakStartTime;
                            breakDTO.StopTime = breakStopTime;
                            breakDTO.LengthMinutes = Convert.ToInt32((breakStopTime - breakStartTime).TotalMinutes);
                            breakDTO.Date = dateT;
                            breakDTO.IsBreak = true;

                            breakDTO.Break1Id = 0;
                            breakDTO.Break1StartTime = breakStartTime;
                            breakDTO.Break1Minutes = Convert.ToInt32((breakStopTime - breakStartTime).TotalMinutes);
                            breakDTO.Break1Link = string.Empty;

                            breakDTO.AccountNr = string.Empty;
                            breakDTO.AccountDim2Nr = account != null ? account.Account.AccountNr : string.Empty;
                            breakDTO.AccountDim3Nr = string.Empty;
                            breakDTO.AccountDim4Nr = string.Empty;
                            breakDTO.AccountDim5Nr = string.Empty;
                            breakDTO.AccountDim6Nr = string.Empty;

                            dtos.Add(breakDTO);
                        }
                    }
                }

                dtos.Add(timeScheduleBlockIODTO);
            }

            return dtos;
        }


        private List<TimeScheduleBlockIODTO> AddZeroDaysToBeginning(List<TimeScheduleBlockIODTO> dtos, bool isTemplate)
        {
            if (dtos == null)
                return null;

            List<TimeScheduleBlockIODTO> newList = new List<TimeScheduleBlockIODTO>();
            var groupOnEmployee = dtos.GroupBy(g => g.EmployeeNr);
            DateTime startDate = dtos.OrderBy(o => o.Date).FirstOrDefault().Date;

            foreach (var employee in groupOnEmployee)
            {
                var employeeStartDate = employee.OrderBy(o => o.Date).FirstOrDefault().Date;
                newList.AddRange(employee);

                if (startDate != employeeStartDate)
                {
                    DateTime currentDate = startDate;
                    var first = employee.First();

                    while (currentDate < employeeStartDate)
                    {
                        TimeScheduleBlockIODTO timeScheduleBlockIODTO = new TimeScheduleBlockIODTO();
                        Guid link = Guid.NewGuid();

                        timeScheduleBlockIODTO.TimeScheduleTemplatePeriodId = 0;
                        timeScheduleBlockIODTO.TimeScheduleEmployeePeriodId = 0;
                        timeScheduleBlockIODTO.EmployeeNr = first.EmployeeNr;
                        timeScheduleBlockIODTO.TimeScheduleTypeId = 0;
                        timeScheduleBlockIODTO.DayNumber = 0;
                        timeScheduleBlockIODTO.Description = string.Empty;

                        timeScheduleBlockIODTO.StartTime = CalendarUtility.DATETIME_DEFAULT;
                        timeScheduleBlockIODTO.StopTime = CalendarUtility.DATETIME_DEFAULT;
                        timeScheduleBlockIODTO.LengthMinutes = 0;
                        timeScheduleBlockIODTO.Date = currentDate;

                        DateTime break1StartTime = CalendarUtility.DATETIME_DEFAULT;
                        DateTime break2StartTime = CalendarUtility.DATETIME_DEFAULT;
                        DateTime break3StartTime = CalendarUtility.DATETIME_DEFAULT;
                        DateTime break4StartTime = CalendarUtility.DATETIME_DEFAULT;

                        timeScheduleBlockIODTO.Break1Id = 0;
                        timeScheduleBlockIODTO.Break1StartTime = break1StartTime;
                        timeScheduleBlockIODTO.Break1Minutes = 0;
                        timeScheduleBlockIODTO.Break1Link = string.Empty;
                        timeScheduleBlockIODTO.Break2Id = 0;
                        timeScheduleBlockIODTO.Break2StartTime = break1StartTime;
                        timeScheduleBlockIODTO.Break2Minutes = 0;
                        timeScheduleBlockIODTO.Break2Link = string.Empty;
                        timeScheduleBlockIODTO.Break3Id = 0;
                        timeScheduleBlockIODTO.Break3StartTime = break1StartTime;
                        timeScheduleBlockIODTO.Break3Minutes = 0;
                        timeScheduleBlockIODTO.Break3Link = string.Empty;
                        timeScheduleBlockIODTO.Break4Id = 0;
                        timeScheduleBlockIODTO.Break4StartTime = break1StartTime;
                        timeScheduleBlockIODTO.Break4Minutes = 0;
                        timeScheduleBlockIODTO.Break4Link = string.Empty;

                        timeScheduleBlockIODTO.ShiftTypeId = 0;
                        timeScheduleBlockIODTO.ShiftTypeName = string.Empty;
                        timeScheduleBlockIODTO.ShiftTypeDescription = string.Empty;
                        timeScheduleBlockIODTO.ShiftTypeTimeScheduleTypeId = 0;
                        timeScheduleBlockIODTO.Link = link.ToString();

                        timeScheduleBlockIODTO.AccountNr = string.Empty;
                        timeScheduleBlockIODTO.AccountDim2Nr = string.Empty;
                        timeScheduleBlockIODTO.AccountDim3Nr = string.Empty;
                        timeScheduleBlockIODTO.AccountDim4Nr = string.Empty;
                        timeScheduleBlockIODTO.AccountDim5Nr = string.Empty;
                        timeScheduleBlockIODTO.AccountDim6Nr = string.Empty;

                        if (!newList.Any(w => w.EmployeeNr == first.EmployeeNr && w.Date == currentDate))
                            newList.Add(timeScheduleBlockIODTO);
                        currentDate = currentDate.AddDays(1);
                    }
                }
            }

            foreach (var block in newList)
                block.IsTemplate = isTemplate;

            return newList;
        }

        private AccountInternal GetAccount(string costAccExtCode)
        {
            if (this.accounts == null || string.IsNullOrEmpty(costAccExtCode))
                return null;

            AccountInternal account = this.accounts.FirstOrDefault(a => !string.IsNullOrEmpty(a.Account.ExternalCode) && a.Account.ExternalCode.ToLower().Equals(costAccExtCode.ToLower()));
            if (account == null)
                account = this.accounts.FirstOrDefault(a => !string.IsNullOrEmpty(a.Account.AccountNr) && a.Account.AccountNr.ToLower().Equals(costAccExtCode.ToLower()));

            return account;
        }

        private TimeDeviationCause GetTimeDeviationCause(string leaveSEVKey)
        {
            if (this.causes == null || string.IsNullOrEmpty(leaveSEVKey))
                return null;

            TimeDeviationCause cause = this.causes.FirstOrDefault(a => !string.IsNullOrEmpty(a.ExtCode) && a.ExtCode.ToLower().Equals(leaveSEVKey.ToLower()));
            if (cause == null)
                cause = this.causes.FirstOrDefault(a => !string.IsNullOrEmpty(a.Name) && a.Name.ToLower().Equals(leaveSEVKey.ToLower()));

            return cause;
        }

        private ShiftType GetShiftType(string costAccExtCode)
        {
            if (this.shiftTypes == null || string.IsNullOrEmpty(costAccExtCode))
                return null;

            foreach (ShiftType type in this.shiftTypes)
            {
                if (string.IsNullOrEmpty(type.ExternalCode))
                    continue;

                if (type.ExternalCode.Trim().ToLower().Equals(costAccExtCode.Trim().ToLower()))
                    return type;

                int integerValue = 0;

                int.TryParse(costAccExtCode, out integerValue);

                if (integerValue != 0 && type.ExternalCode.Trim().ToLower().Equals(integerValue.ToString()))
                    return type;

            }

            foreach (ShiftType type in this.shiftTypes)
            {
                if (string.IsNullOrEmpty(type.NeedsCode))
                    continue;

                if (type.NeedsCode.Trim().ToLower().Equals(costAccExtCode.Trim().ToLower()))
                    return type;

                int integerValue = 0;

                int.TryParse(costAccExtCode, out integerValue);

                if (integerValue != 0 && type.NeedsCode.Trim().ToLower().Equals(integerValue.ToString()))
                    return type;
            }

            AccountInternal account = GetAccount(costAccExtCode);
            if (account == null)
                account = this.accounts.FirstOrDefault(a => !string.IsNullOrEmpty(a.Account.AccountNr) && a.Account.AccountNr.Trim().ToLower().Equals(costAccExtCode.Trim().ToLower()));

            if (account != null && this.shiftTypes != null)
            {
                foreach (ShiftType type in this.shiftTypes)
                {
                    foreach (AccountInternal accountInternal in type.AccountInternal)
                    {
                        if (account.AccountId == accountInternal.AccountId)
                            return type;
                    }
                }
            }

            foreach (ShiftType type in this.shiftTypes)
            {
                if (string.IsNullOrEmpty(type.Name))
                    continue;

                if (type.Name.Trim().ToLower().Equals(costAccExtCode.Trim().ToLower()))
                    return type;
            }

            return null;
        }

    }
}

using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class ScheduleTidTid : ImportExportManager
    {
        private List<ShiftType> shiftTypes;
        private List<AccountInternal> accounts;
        private List<TimeDeviationCause> causes;


        public ScheduleTidTid(ParameterObject parameterObject) : base(parameterObject) { }

        public List<dynamic> GetTimeScheduleTemplateBlockIODTOs(string content, bool isTemplate)
        {
            //<?xml version="1.0" encoding="ISO-8859-1"?>
            //<xml IPoolTILTidExchangeFormatVersion="Version 27 - 2014-06-13 14:00:00" TILTidVersionDate="2015-01-14 12:00" CompanyNo="0" TILTidCompanyNo="2167" TILTidCompanyName="ICA Maxi Barkaraby" TILTidUser="Matilda Lindblom" TILTidMachine="SE14921C19" UseSpeedCodeInsteadOfCostAccountId="0">
            //	<EmployeeSchedule Scope="All">
            //		<Date Value="2017-11-13">
            //			<Employee>
            //				<EmplNo>10</EmplNo>
            //				<FirstIn>360</FirstIn>
            //				<LastOut>900</LastOut>
            //				<TotBreak>75</TotBreak>
            //				<SchFrag>
            //					<In>360</In>
            //					<Out>900</Out>
            //					<CostAccExtCode>31</CostAccExtCode>
            //				</SchFrag>
            //			</Employee>
            //			<Employee>
            //				<EmplNo>11</EmplNo>
            //				<FirstIn>360</FirstIn>
            //				<LastOut>900</LastOut>
            //				<TotBreak>75</TotBreak>
            //				<SchFrag>
            //					<In>360</In>
            //					<Out>900</Out>
            //					<CostAccExtCode>31</CostAccExtCode>
            //				</SchFrag>
            //			</Employee>
            //			<Employee>
            //				<EmplNo>14</EmplNo>
            //				<FirstIn>360</FirstIn>
            //				<LastOut>960</LastOut>
            //				<TotBreak>90</TotBreak>
            //				<SchFrag>
            //					<In>360</In>
            //					<Out>510</Out>
            //					<CostAccExtCode>93</CostAccExtCode>
            //				</SchFrag>
            //				<SchFrag>
            //					<In>540</In>
            //					<Out>660</Out>
            //					<CostAccExtCode>93</CostAccExtCode>
            //				</SchFrag>
            //				<SchFrag>
            //					<In>690</In>
            //					<Out>780</Out>
            //					<CostAccExtCode>93</CostAccExtCode>
            //				</SchFrag>
            //				<SchFrag>
            //					<In>780</In>
            //					<Out>840</Out>
            //					<CostAccExtCode>94</CostAccExtCode>
            //				</SchFrag>
            //				<SchFrag>
            //					<In>870</In>
            //					<Out>960</Out>
            //					<CostAccExtCode>94</CostAccExtCode>
            //				</SchFrag>
            //			</Employee>
            //		</Date>
            //	</EmployeeSchedule>
            //</xml>

            List<TimeScheduleBlockIODTO> timeScheduleBlockIODTOs = new List<TimeScheduleBlockIODTO>();
            List<Employee> employees = EmployeeManager.GetAllEmployees(base.ActorCompanyId);
            this.causes = TimeDeviationCauseManager.GetTimeDeviationCauses(base.ActorCompanyId);
            this.shiftTypes = TimeScheduleManager.GetShiftTypes(base.ActorCompanyId, loadAccounts: true);

            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(base.ActorCompanyId, onlyInternal: true);
            AccountDim accountDim = accountDims.FirstOrDefault(a => a.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);
            if (accountDim == null)
                return null;

            this.accounts = AccountManager.GetAccountInternalsByDim(accountDim.AccountDimId, base.ActorCompanyId);
            List<dynamic> dtos = new List<dynamic>();
            XElement xml = null;

            try
            {
                xml = XElement.Parse(content);
            }
            catch
            {
                return null;
            }

            XElement scheduleRoot = xml.Elements("EmployeeSchedule").FirstOrDefault();
            foreach (var item in XmlUtil.GetChildElements(scheduleRoot, "Date"))
            {
                var datestring = XmlUtil.GetAttributeStringValue(item, "Value");
                var date = CalendarUtility.GetDateTime(datestring);

                foreach (var employee in XmlUtil.GetChildElements(item, "Employee"))
                {
                    string employeeNr = XmlUtil.GetChildElementValue(employee, "EmplNo");

                    List<TimeScheduleBlockIODTO> blocksOnDateOnEmployee = new List<TimeScheduleBlockIODTO>();

                    var frags = XmlUtil.GetChildElements(employee, "SchFrag");

                    if (frags.Count < 2)
                    {
                        var frag = frags.First();
                        var leaveSEVKeys = XmlUtil.GetChildElements(frag, "LeaveSEVKey");
                        string leaveSEVKey = string.Empty;
                        if (!leaveSEVKeys.IsNullOrEmpty())
                            leaveSEVKey = leaveSEVKeys.First().Value;

                        TimeScheduleBlockIODTO timeScheduleBlockIODTO = new TimeScheduleBlockIODTO();
                        Guid link = Guid.NewGuid();

                        DateTime startTime = CalendarUtility.MinutesToDateTime(date, XmlUtil.GetElementIntValue(employee, "FirstIn"));
                        DateTime stopTime = CalendarUtility.MinutesToDateTime(date, XmlUtil.GetElementIntValue(employee, "LastOut"));

                        timeScheduleBlockIODTO.TimeScheduleTemplatePeriodId = 0;
                        timeScheduleBlockIODTO.TimeScheduleEmployeePeriodId = 0;
                        timeScheduleBlockIODTO.EmployeeNr = employeeNr;
                        timeScheduleBlockIODTO.TimeScheduleTypeId = 0;
                        timeScheduleBlockIODTO.DayNumber = 0;
                        timeScheduleBlockIODTO.Description = string.Empty;

                        timeScheduleBlockIODTO.StartTime = startTime;
                        timeScheduleBlockIODTO.StopTime = stopTime;
                        timeScheduleBlockIODTO.LengthMinutes = Convert.ToInt32((stopTime - startTime).TotalMinutes);
                        timeScheduleBlockIODTO.Date = date;

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

                        ShiftType shiftType = !frags.Any() ? GetShiftType(XmlUtil.GetChildElementValue(employee, "CostAccExtCode")) : GetShiftType(XmlUtil.GetChildElementValue(frags.First(), "CostAccExtCode"));

                        if (shiftType != null)
                            timeScheduleBlockIODTO.ShiftTypeId = shiftType.ShiftTypeId;

                        timeScheduleBlockIODTO.ShiftTypeName = string.Empty;
                        timeScheduleBlockIODTO.ShiftTypeDescription = string.Empty;
                        timeScheduleBlockIODTO.ShiftTypeTimeScheduleTypeId = 0;
                        timeScheduleBlockIODTO.Link = link.ToString();

                        AccountInternal account = !frags.Any() ? GetAccount(XmlUtil.GetChildElementValue(employee, "CostAccExtCode")) : GetAccount(XmlUtil.GetChildElementValue(frags.First(), "CostAccExtCode"));

                        timeScheduleBlockIODTO.AccountNr = string.Empty;
                        timeScheduleBlockIODTO.AccountDim2Nr = account != null ? account.Account.AccountNr : string.Empty;
                        timeScheduleBlockIODTO.AccountDim3Nr = string.Empty;
                        timeScheduleBlockIODTO.AccountDim4Nr = string.Empty;
                        timeScheduleBlockIODTO.AccountDim5Nr = string.Empty;
                        timeScheduleBlockIODTO.AccountDim6Nr = string.Empty;
                        if (!isTemplate && !string.IsNullOrEmpty(leaveSEVKey))
                        {
                            var cause = GetTimeDeviationCause(leaveSEVKey);
                            timeScheduleBlockIODTO.TimeDeviationCauseId = cause != null ? cause.TimeDeviationCauseId : 0;
                        }
                        blocksOnDateOnEmployee.Add(timeScheduleBlockIODTO);


                        int breakMinutes = XmlUtil.GetElementIntValue(employee, "TotBreak");
                        if (breakMinutes > 0)
                        {
                            DateTime breakStart = CalendarUtility.AdjustAccordingToInterval((CalendarUtility.GetMiddleTime(startTime, stopTime).AddMinutes(-(double)decimal.Divide(breakMinutes, 2))), 0, 15);
                            DateTime breakStop = breakStart.AddMinutes(breakMinutes);

                            TimeScheduleBlockIODTO breakDTO = new TimeScheduleBlockIODTO();

                            breakDTO.TimeScheduleTemplatePeriodId = 0;
                            breakDTO.TimeScheduleEmployeePeriodId = 0;
                            breakDTO.EmployeeNr = employeeNr;
                            breakDTO.TimeScheduleTypeId = 0;
                            breakDTO.DayNumber = 0;
                            breakDTO.Description = string.Empty;

                            breakDTO.StartTime = breakStart;
                            breakDTO.StopTime = breakStop;
                            breakDTO.LengthMinutes = Convert.ToInt32((stopTime - startTime).TotalMinutes);
                            breakDTO.Date = date;

                            breakDTO.Break1Id = 0;
                            breakDTO.Break1StartTime = breakStart;
                            breakDTO.Break1Minutes = breakMinutes;
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

                            breakDTO.ShiftTypeId = shiftType != null ? shiftType.ShiftTypeId : 0;
                            breakDTO.ShiftTypeName = string.Empty;
                            breakDTO.ShiftTypeDescription = string.Empty;
                            breakDTO.ShiftTypeTimeScheduleTypeId = 0;
                            breakDTO.Link = link.ToString();

                            breakDTO.AccountNr = string.Empty;
                            breakDTO.AccountDim2Nr = string.Empty;
                            breakDTO.AccountDim3Nr = string.Empty;
                            breakDTO.AccountDim4Nr = string.Empty;
                            breakDTO.AccountDim5Nr = string.Empty;
                            breakDTO.AccountDim6Nr = string.Empty;
                            blocksOnDateOnEmployee.Add(breakDTO);
                        }
                    }
                    else
                    {
                        int numberOfFrags = frags.Count;
                        Guid link = Guid.NewGuid();
                        int breakMinutes = XmlUtil.GetElementIntValue(employee, "TotBreak");

                        for (int i = 0; i < numberOfFrags; i++)
                        {
                            var scheduleblock = frags[i];
                            var nextBlock = i + 1 < numberOfFrags ? frags[i + 1] : null;
                            int fragGap = 0;

                            var frag = scheduleblock;
                            var leaveSEVKeys = XmlUtil.GetChildElements(frag, "LeaveSEVKey");
                            string leaveSEVKey = string.Empty;
                            if (!leaveSEVKeys.IsNullOrEmpty())
                                leaveSEVKey = leaveSEVKeys.First().Value;

                            TimeScheduleBlockIODTO timeScheduleBlockIODTO = new TimeScheduleBlockIODTO();

                            DateTime startTime = CalendarUtility.MinutesToDateTime(date, XmlUtil.GetElementIntValue(scheduleblock, "In"));
                            DateTime stopTime = CalendarUtility.MinutesToDateTime(date, XmlUtil.GetElementIntValue(scheduleblock, "Out"));
                            if (nextBlock != null)
                            {
                                DateTime newStopTime = CalendarUtility.MinutesToDateTime(date, XmlUtil.GetElementIntValue(nextBlock, "In"));
                                fragGap = Convert.ToInt32((newStopTime - stopTime).TotalMinutes);
                                stopTime = newStopTime;
                            }

                            timeScheduleBlockIODTO.TimeScheduleTemplatePeriodId = 0;
                            timeScheduleBlockIODTO.TimeScheduleEmployeePeriodId = 0;
                            timeScheduleBlockIODTO.EmployeeNr = employeeNr;
                            timeScheduleBlockIODTO.TimeScheduleTypeId = 0;
                            timeScheduleBlockIODTO.DayNumber = 0;
                            timeScheduleBlockIODTO.Description = string.Empty;

                            timeScheduleBlockIODTO.StartTime = startTime;
                            timeScheduleBlockIODTO.StopTime = stopTime;
                            timeScheduleBlockIODTO.LengthMinutes = Convert.ToInt32((stopTime - startTime).TotalMinutes);
                            timeScheduleBlockIODTO.Date = date;

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


                            ShiftType shiftType = GetShiftType(XmlUtil.GetChildElementValue(scheduleblock, "CostAccExtCode"));

                            timeScheduleBlockIODTO.ShiftTypeId = shiftType != null ? shiftType.ShiftTypeId : 0;
                            timeScheduleBlockIODTO.ShiftTypeName = string.Empty;
                            timeScheduleBlockIODTO.ShiftTypeDescription = string.Empty;
                            timeScheduleBlockIODTO.ShiftTypeTimeScheduleTypeId = 0;
                            timeScheduleBlockIODTO.Link = link.ToString();

                            AccountInternal account = GetAccount(XmlUtil.GetChildElementValue(scheduleblock, "CostAccExtCode"));

                            timeScheduleBlockIODTO.AccountNr = string.Empty;
                            timeScheduleBlockIODTO.AccountDim2Nr = account != null ? account.Account.AccountNr : string.Empty;
                            timeScheduleBlockIODTO.AccountDim3Nr = string.Empty;
                            timeScheduleBlockIODTO.AccountDim4Nr = string.Empty;
                            timeScheduleBlockIODTO.AccountDim5Nr = string.Empty;
                            timeScheduleBlockIODTO.AccountDim6Nr = string.Empty;
                            if (!isTemplate && !string.IsNullOrEmpty(leaveSEVKey))
                            {
                                var cause = GetTimeDeviationCause(leaveSEVKey);
                                timeScheduleBlockIODTO.TimeDeviationCauseId = cause != null ? cause.TimeDeviationCauseId : 0;
                            }

                            blocksOnDateOnEmployee.Add(timeScheduleBlockIODTO);


                            if (fragGap > 0)
                            {
                                breakMinutes = 0;
                                TimeScheduleBlockIODTO breakDTO = new TimeScheduleBlockIODTO();

                                DateTime breakStart = timeScheduleBlockIODTO.StopTime.AddMinutes(-fragGap);
                                DateTime breakStop = timeScheduleBlockIODTO.StopTime;

                                breakDTO.TimeScheduleTemplatePeriodId = 0;
                                breakDTO.TimeScheduleEmployeePeriodId = 0;
                                breakDTO.EmployeeNr = employeeNr;
                                breakDTO.TimeScheduleTypeId = 0;
                                breakDTO.DayNumber = 0;
                                breakDTO.Description = string.Empty;

                                breakDTO.StartTime = breakStart;
                                breakDTO.StopTime = breakStop;
                                breakDTO.LengthMinutes = Convert.ToInt32((breakStop - breakStart).TotalMinutes);
                                breakDTO.Date = date;

                                breakDTO.Break1Id = 0;
                                breakDTO.Break1StartTime = breakStart;
                                breakDTO.Break1Minutes = fragGap;
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

                                breakDTO.ShiftTypeId = null;
                                breakDTO.ShiftTypeName = string.Empty;
                                breakDTO.ShiftTypeDescription = string.Empty;
                                breakDTO.ShiftTypeTimeScheduleTypeId = 0;
                                breakDTO.Link = link.ToString();


                                breakDTO.AccountNr = string.Empty;
                                breakDTO.AccountDim2Nr = null;
                                breakDTO.AccountDim3Nr = string.Empty;
                                breakDTO.AccountDim4Nr = string.Empty;
                                breakDTO.AccountDim5Nr = string.Empty;
                                breakDTO.AccountDim6Nr = string.Empty;

                                blocksOnDateOnEmployee.Add(breakDTO);
                            }
                        }

                        if (breakMinutes > 0)
                        {
                            DateTime startTime = CalendarUtility.MinutesToDateTime(date, XmlUtil.GetElementIntValue(employee, "FirstIn"));
                            DateTime stopTime = CalendarUtility.MinutesToDateTime(date, XmlUtil.GetElementIntValue(employee, "LastOut"));
                            DateTime breakStart = CalendarUtility.AdjustAccordingToInterval((CalendarUtility.GetMiddleTime(startTime, stopTime).AddMinutes(-(double)decimal.Divide(breakMinutes, 2))), 0, 15);
                            DateTime breakStop = breakStart.AddMinutes(breakMinutes);
                            DateTime break1StartTime = CalendarUtility.DATETIME_DEFAULT;
                            TimeScheduleBlockIODTO breakDTO = new TimeScheduleBlockIODTO();

                            breakDTO.TimeScheduleTemplatePeriodId = 0;
                            breakDTO.TimeScheduleEmployeePeriodId = 0;
                            breakDTO.EmployeeNr = employeeNr;
                            breakDTO.TimeScheduleTypeId = 0;
                            breakDTO.DayNumber = 0;
                            breakDTO.Description = string.Empty;

                            breakDTO.StartTime = breakStart;
                            breakDTO.StopTime = breakStop;
                            breakDTO.LengthMinutes = Convert.ToInt32((stopTime - startTime).TotalMinutes);
                            breakDTO.Date = date;

                            breakDTO.Break1Id = 0;
                            breakDTO.Break1StartTime = breakStart;
                            breakDTO.Break1Minutes = breakMinutes;
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

                            breakDTO.ShiftTypeId = 0;
                            breakDTO.ShiftTypeName = string.Empty;
                            breakDTO.ShiftTypeDescription = string.Empty;
                            breakDTO.ShiftTypeTimeScheduleTypeId = 0;
                            breakDTO.Link = link.ToString();

                            breakDTO.AccountNr = string.Empty;
                            breakDTO.AccountDim2Nr = string.Empty;
                            breakDTO.AccountDim3Nr = string.Empty;
                            breakDTO.AccountDim4Nr = string.Empty;
                            breakDTO.AccountDim5Nr = string.Empty;
                            breakDTO.AccountDim6Nr = string.Empty;
                            blocksOnDateOnEmployee.Add(breakDTO);
                        }
                    }

                    timeScheduleBlockIODTOs.AddRange(MergeTimeScheduleBlockIODTOs(blocksOnDateOnEmployee));
                }
            }

            foreach (var block in timeScheduleBlockIODTOs)
                block.IsTemplate = isTemplate;

            dtos.AddRange(AddZeroDaysToBeginning(timeScheduleBlockIODTOs, isTemplate));

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

            return null;
        }

    }
}

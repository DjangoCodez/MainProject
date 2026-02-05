using SoftOne.Soe.Business.Billing.Template.Managers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Template;
using SoftOne.Soe.Business.Core.Template.Managers;
using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Core.Template.Models.Billing;
using SoftOne.Soe.Business.Core.Template.Models.Economy;
using SoftOne.Soe.Business.Core.Template.Models.Time;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;

namespace SoftOne.Soe.Business.Template.Managers
{
    public class TimeTemplateManager : ManagerBase
    {
        private readonly CompanyTemplateManager companyTemplateManager;
        private readonly EconomyTemplateManager economyTemplateManager;
        private readonly AttestTemplateManager attestTemplateManager;
        public TimeTemplateManager(ParameterObject parameterObject) : base(parameterObject)
        {
            companyTemplateManager = new CompanyTemplateManager(base.parameterObject);
            economyTemplateManager = new EconomyTemplateManager(base.parameterObject);
            attestTemplateManager = new AttestTemplateManager(base.parameterObject);
        }

        public TemplateCompanyTimeDataItem GetTemplateCompanyTimeDataItem(CopyFromTemplateCompanyInputDTO inputDTO)
        {
            TemplateCompanyTimeDataItem item = new TemplateCompanyTimeDataItem();

            if (inputDTO.DoCopy(TemplateCompanyCopy.DaytypesHalfDaysAndHolidays))
            {
                item.DayTypeCopyItems = GetDayTypeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.TimeHalfDayCopyItems = GetTimeHalfDayCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.HolidayCopyItems = GetHolidayCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            }

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimePeriods))
                item.TimePeriodHeadCopyItems = GetTimePeriodHeadCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.Positions))
                item.PositionCopyItems = GetPositionCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas))
            {
                item.VacationGroupCopyItems = GetVacationGroupCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.PayrollPriceTypeCopyItems = GetPayrollPriceTypeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.PayrollPriceFormulaCopyItems = GetPayrollPriceFormulaCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.PayrollProductCopyItems = GetPayrollProductCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.PayrollGroupCopyItems = GetPayrollGroupCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
            }

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeScheduleTypesAndShiftTypes))
            {
                item.TimeScheduleTypeCopyItems = GetTimeScheduleTypeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.ShiftTypeCopyItems = GetShiftTypeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
            }

            if (inputDTO.DoCopy(TemplateCompanyCopy.Skills))
                item.SkillCopyItems = GetSkillCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.ScheduleCykles))
                item.ScheduleCycleCopyItems = GetScheduleCycleCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.FollowUpTypes))
                item.FollowUpTypeCopyItems = GetFollowUpTypeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollProductsAndTimeCodes))
            {
                item.TimeScheduleTypeCopyItems = !item.TimeScheduleTypeCopyItems.Any() ? GetTimeScheduleTypeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId) : item.TimeScheduleTypeCopyItems;
                item.PayrollProductCopyItems = !item.PayrollProductCopyItems.Any() ? GetPayrollProductCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId) : item.PayrollProductCopyItems;
                item.InvoiceProductCopyItems = GetInvoiceProductCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.TimeCodeCopyItems = GetTimeCodeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.TimeBreakTemplateCopyItems = GetTimeBreakTemplateCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.TimeCodeBreakGroupCopyItems = GetTimeCodeBreakGroupCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.TimeCodeRankingGroupCopyItems = GetTimeCodeRankingGroupCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
            }

            if (inputDTO.DoCopy(TemplateCompanyCopy.DeviationCauses))
                item.TimeDeviationCauseCopyItems = GetTimeDeviationCauseCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.EmployeeGroups))
                item.EmployeeGroupCopyItems = GetEmployeeGroupCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.EmploymentTypes))
                item.EmploymentTypeCopyItems = GetEmploymentTypeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeAccumulators))
                item.TimeAccumulatorCopyItems = GetTimeAccumulatorCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeRules))
                item.TimeRuleCopyItems = GetTimeRuleCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeAbsenseRules))
                item.TimeAbsenceRuleCopyItems = GetTimeAbsenceRuleCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeAttestRules))
                item.TimeAttestRuleCopyItems = GetTimeAttestRuleCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            item.EmployeeCollectiveAgreementCopyItems = GetEmployeeCollectiveAgreementCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
            item.EmployeeTemplateCopyItems = GetEmployeeTemplateCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            return item;
        }

        public List<TemplateResult> CopyTemplateCompanyTimeDataItem(CopyFromTemplateCompanyInputDTO inputDTO, TemplateCompanyDataItem templateCompanyDataItem)
        {
            List<TemplateResult> templateResults = new List<TemplateResult>();

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestTime))
                templateResults.Add(attestTemplateManager.CopyAttestRolesFromTemplateCompany(templateCompanyDataItem, SoeModule.Time));

            if (inputDTO.DoCopy(TemplateCompanyCopy.BaseAccountsTime))
                templateResults.Add(economyTemplateManager.CopyBaseAccountsFromTemplateCompany(templateCompanyDataItem, SoeModule.Time));

            if (inputDTO.DoCopy(TemplateCompanyCopy.DaytypesHalfDaysAndHolidays))
                templateResults.Add(CopyDaytypesHalfDaysAndHolidaysFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimePeriods))
                templateResults.Add(CopyTimePeriodsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.Positions))
                templateResults.Add(CopyPositionsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas) || inputDTO.DoCopy(TemplateCompanyCopy.VacationGroups))
            {
                templateResults.Add(CopyPayrollPriceTypesFromTemplateCompany(templateCompanyDataItem, !inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas)));
                templateResults.Add(CopyPayrollPriceFormulasFromTemplateCompany(templateCompanyDataItem, !inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas)));
                templateResults.Add(CopyVacationGroupsFromTemplateCompany(templateCompanyDataItem));
                templateResults.Add(CopyPayrollGroupsFromTemplateCompany(templateCompanyDataItem));
            }

            if (inputDTO.DoCopy(TemplateCompanyCopy.VacationGroups) && !inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas))
                templateResults.Add(CopyVacationGroupsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.Skills))
                templateResults.Add(CopySkillsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.ScheduleCykles))
                templateResults.Add(CopyScheduleCyclesFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.FollowUpTypes))
                templateResults.Add(CopyFollowUpTypesFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollProductsAndTimeCodes))
            {
                templateResults.Add(CopyShiftTypeAndTimeScheduleTypeFromTemplateCompany(templateCompanyDataItem));
                templateResults.Add(CopyPayrollProductsFromTemplateCompany(templateCompanyDataItem));
                templateResults.Add(CopyInvoiceProductsFromTemplateCompany(templateCompanyDataItem));
                templateResults.Add(CopyTimeCodesFromTemplateCompany(templateCompanyDataItem));
                templateResults.Add(CopyTimeCodeRankingFromTemplateCompany(templateCompanyDataItem));
            }

            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas))
                templateResults.Add(CopyPayrollGroupPayrollProductFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.DeviationCauses))
                templateResults.Add(CopyTimeDeviationCausesFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeScheduleTypesAndShiftTypes) && !inputDTO.DoCopy(TemplateCompanyCopy.PayrollProductsAndTimeCodes))
                templateResults.Add(CopyShiftTypeAndTimeScheduleTypeFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollProductsAndTimeCodes))
                templateResults.Add(CopyTimeBreakTemplatesFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.EmployeeGroups))
                templateResults.Add(CopyEmployeeGroupsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.EmploymentTypes))
                templateResults.Add(CopyEmploymentTypesFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeAccumulators))
                templateResults.Add(CopyTimeAccumulatorsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeRules))
                templateResults.Add(CopyTimeRulesFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeAbsenseRules))
                templateResults.Add(CopyTimeAbsenceRulesFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeAttestRules))
                templateResults.Add(CopyTimeAttestRulesFromTemplateCompany(templateCompanyDataItem));

            return templateResults;
        }


        #region DayTypes
        public TemplateResult CopyDaytypesHalfDaysAndHolidaysFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();


            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                {
                    templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                    return templateResult;
                }

                //Existing for new Company
                var existingDayTypes = CalendarManager.GetDayTypesByCompany(entities, item.DestinationActorCompanyId);
                var existingTimeHalfDays = CalendarManager.GetTimeHalfdays(entities, item.DestinationActorCompanyId, false).ToList();
                var existingHolidays = CalendarManager.GetHolidaysByCompany(entities, item.DestinationActorCompanyId, loadDayType: true).ToList();

                #endregion

                #region DayTypes

                foreach (var dayTypeCopyItem in item.TemplateCompanyTimeDataItem.DayTypeCopyItems)
                {
                    #region DayType

                    DayType dayType = item.Update ? existingDayTypes.FirstOrDefault(d => d.Name == dayTypeCopyItem.Name) : null;
                    if (dayType == null)
                    {
                        #region Add

                        dayType = new DayType()
                        {
                            SysDayTypeId = dayTypeCopyItem.SysDayTypeId,
                            Name = dayTypeCopyItem.Name,
                            Description = dayTypeCopyItem.Description,
                            StandardWeekdayFrom = dayTypeCopyItem.StandardWeekdayFrom,
                            StandardWeekdayTo = dayTypeCopyItem.StandardWeekdayTo,
                            Type = dayTypeCopyItem.Type,
                            ActorCompanyId = item.DestinationActorCompanyId
                        };
                        SetCreatedProperties(dayType);
                        entities.DayType.AddObject(dayType);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        dayType.SysDayTypeId = dayTypeCopyItem.SysDayTypeId;
                        dayType.Name = dayTypeCopyItem.Name;
                        dayType.Description = dayTypeCopyItem.Description;
                        dayType.StandardWeekdayFrom = dayTypeCopyItem.StandardWeekdayFrom;
                        dayType.StandardWeekdayTo = dayTypeCopyItem.StandardWeekdayTo;
                        dayType.Type = dayTypeCopyItem.Type;
                        dayType.State = dayTypeCopyItem.State;
                        SetModifiedProperties(dayType);

                        #endregion
                    }
                    item.TemplateCompanyTimeDataItem.AddDayTypeMapping(dayTypeCopyItem.DayTypeId, dayType);

                    #endregion
                }

                var saveDayTypeResult = SaveChanges(entities);
                if (!saveDayTypeResult.Success)
                    companyTemplateManager.LogCopyError("DayTypes", item, saved: true);

                templateResult.ActionResults.Add(saveDayTypeResult);

                //Get existing DayTypes again after save
                existingDayTypes = CalendarManager.GetDayTypesByCompany(entities, item.DestinationActorCompanyId);

                #endregion

                #region HalfDays

                foreach (var templateHalfDay in item.TemplateCompanyTimeDataItem.TimeHalfDayCopyItems)
                {
                    #region HalfDay

                    TimeHalfday timeHalfDay = item.Update ? existingTimeHalfDays.FirstOrDefault(h => h.Name == templateHalfDay.Name) : null;
                    DayType existingDaytype = item.Update ? existingDayTypes.FirstOrDefault(d => d.Name == templateHalfDay.Name) : null;
                    if (timeHalfDay == null && existingDaytype != null)
                    {
                        #region Add

                        timeHalfDay = new TimeHalfday()
                        {
                            Name = templateHalfDay.Name,
                            Description = templateHalfDay.Description,
                            Type = templateHalfDay.Type,
                            Value = templateHalfDay.Value,
                            State = templateHalfDay.State,
                            DayTypeId = existingDaytype.DayTypeId
                        };
                        SetCreatedProperties(timeHalfDay);
                        entities.TimeHalfday.AddObject(timeHalfDay);

                        #endregion
                    }
                    else if (existingDaytype != null)
                    {
                        #region Update

                        timeHalfDay.Name = templateHalfDay.Name;
                        timeHalfDay.Description = templateHalfDay.Description;
                        timeHalfDay.Type = templateHalfDay.Type;
                        timeHalfDay.Value = templateHalfDay.Value;
                        timeHalfDay.DayTypeId = existingDaytype.DayTypeId;
                        SetModifiedProperties(timeHalfDay);
                        #endregion
                    }

                    #endregion
                }

                var saveHalfDayResult = SaveChanges(entities);
                if (!saveHalfDayResult.Success)
                    companyTemplateManager.LogCopyError("DayTypes", item, saved: true);

                templateResult.ActionResults.Add(saveHalfDayResult);

                #endregion

                #region Holidays

                foreach (var templateHolidaysById in item.TemplateCompanyTimeDataItem.HolidayCopyItems.GroupBy(g => g.HolidayId))
                {
                    #region Holiday

                    var templateHoliday = templateHolidaysById.First();

                    Holiday holiday = null;
                    DayType dayType = item.TemplateCompanyTimeDataItem.GetDayType(templateHoliday.DayTypeId);
                    if (item.Update && existingHolidays.Any())
                        holiday = existingHolidays.FirstOrDefault(h => h.Name == templateHoliday.Name).FromDTO();

                    if (holiday == null && dayType != null)
                    {
                        #region Add

                        holiday = new Holiday()
                        {
                            Name = templateHoliday.Name,
                            Description = templateHoliday.Description,
                            SysHolidayTypeId = templateHoliday.SysHolidayTypeId,
                            SysHolidayId = templateHoliday.SysHolidayId,
                            DayTypeId = dayType.DayTypeId,
                            Date = templateHoliday.Date,
                            IsRedDay = templateHoliday.IsRedDay,
                        };

                        if (holiday.SysHolidayTypeId.HasValue)
                            holiday.Date = CalendarUtility.DATETIME_DEFAULT;

                        SetCreatedProperties(holiday);
                        newCompany.Holiday.Add(holiday);

                        #endregion
                    }
                    else if (dayType != null)
                    {
                        #region Update

                        holiday.Name = templateHoliday.Name;
                        holiday.Description = templateHoliday.Description;
                        holiday.SysHolidayTypeId = templateHoliday.SysHolidayTypeId;
                        holiday.SysHolidayId = templateHoliday.SysHolidayId;
                        holiday.Date = templateHoliday.Date;
                        holiday.DayTypeId = dayType.DayTypeId;


                        if (holiday.SysHolidayTypeId.HasValue)
                            holiday.Date = CalendarUtility.DATETIME_DEFAULT;

                        SetModifiedProperties(holiday);

                        #endregion
                    }

                    #endregion
                }

                var saveHolidayResult = SaveChanges(entities);
                if (!saveHolidayResult.Success)
                    saveHolidayResult = companyTemplateManager.LogCopyError("Holiday", item, saved: true);

                templateResult.ActionResults.Add(saveHolidayResult);

                #endregion
            }

            return templateResult;
        }


        public List<DayTypeCopyItem> GetDayTypeCopyItems(int actorCompanyId)
        {
            List<DayTypeCopyItem> dayTypeCopyItems = new List<DayTypeCopyItem>();
            var dayTypes = CalendarManager.GetDayTypesByCompany(actorCompanyId);

            foreach (var dayType in dayTypes)
            {
                DayTypeCopyItem item = new DayTypeCopyItem()
                {
                    DayTypeId = dayType.DayTypeId,
                    Name = dayType.Name,
                    Description = dayType.Description,
                    StandardWeekdayFrom = dayType.StandardWeekdayFrom,
                    StandardWeekdayTo = dayType.StandardWeekdayTo,
                    Type = dayType.Type,
                    State = dayType.State,
                    SysDayTypeId = dayType.SysDayTypeId
                };

                dayTypeCopyItems.Add(item);
            }

            return dayTypeCopyItems;
        }

        public List<DayTypeCopyItem> GetDayTypeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetDayTypeCopyItems(actorCompanyId);

            return timeTemplateConnector.GetDayTypeCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<TimeHalfDayCopyItem> GetTimeHalfDayCopyItems(int actorCompanyId)
        {
            List<TimeHalfDayCopyItem> timeHalfDayCopyItems = new List<TimeHalfDayCopyItem>();
            var timeHalfDays = CalendarManager.GetTimeHalfdays(actorCompanyId, false).ToList();

            foreach (var timeHalfDay in timeHalfDays)
            {
                TimeHalfDayCopyItem item = new TimeHalfDayCopyItem()
                {
                    TimeHalfDayId = timeHalfDay.TimeHalfdayId,
                    Name = timeHalfDay.Name,
                    Description = timeHalfDay.Description,
                    Type = timeHalfDay.Type,
                    Value = timeHalfDay.Value,
                    State = timeHalfDay.State
                };

                timeHalfDayCopyItems.Add(item);
            }

            return timeHalfDayCopyItems;
        }

        public List<TimeHalfDayCopyItem> GetTimeHalfDayCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeHalfDayCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeHalfDayCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<HolidayCopyItem> GetHolidayCopyItems(int actorCompanyId)
        {
            List<HolidayCopyItem> holidayCopyItems = new List<HolidayCopyItem>();
            var holidays = CalendarManager.GetHolidaysByCompany(actorCompanyId, loadDayType: true);

            foreach (var holiday in holidays.Where(w => w.State == (int)SoeEntityState.Active && (w.SysHolidayTypeId.HasValue || w.Date > DateTime.Today.AddYears(-1))))
            {
                HolidayCopyItem item = new HolidayCopyItem()
                {
                    HolidayId = holiday.HolidayId,
                    Date = holiday.Date,
                    Name = holiday.Name,
                    Description = holiday.Description,
                    SysHolidayId = holiday.SysHolidayId,
                    SysHolidayTypeId = holiday.SysHolidayTypeId,
                    IsRedDay = holiday.IsRedDay,
                    DayTypeId = holiday.DayTypeId
                };

                holidayCopyItems.Add(item);
            }

            return holidayCopyItems;
        }

        public List<HolidayCopyItem> GetHolidayCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetHolidayCopyItems(actorCompanyId);

            return timeTemplateConnector.GetHolidayCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region TimePeriods

        public TemplateResult CopyTimePeriodsFromTemplateCompany(TemplateCompanyDataItem item)
        {

            TemplateResult templateResult = new TemplateResult();
            #region Prereq

            if (!item.TemplateCompanyTimeDataItem.TimePeriodHeadCopyItems.Any())
                return new TemplateResult(new ActionResult());

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<TimePeriodHead> existingTimePeriodHeads = TimePeriodManager.GetTimePeriodHeads(item.DestinationActorCompanyId, TermGroup_TimePeriodType.Unknown, false, false);

                #endregion

                #region TimePeriodHead

                foreach (var timePeriodHeadCopyItem in item.TemplateCompanyTimeDataItem.TimePeriodHeadCopyItems)
                {
                    #region TimePeriodHead

                    TimePeriodHead timePeriodHead = existingTimePeriodHeads.FirstOrDefault(pt => pt.Name == timePeriodHeadCopyItem.Name);
                    if (timePeriodHead == null)
                    {
                        timePeriodHead = new TimePeriodHead();
                        SetCreatedProperties(timePeriodHead, user);
                        entities.TimePeriodHead.AddObject(timePeriodHead);
                    }
                    else
                    {
                        SetModifiedProperties(timePeriodHead, user);
                    }

                    timePeriodHead.TimePeriodType = (int)timePeriodHeadCopyItem.TimePeriodType;
                    timePeriodHead.Name = timePeriodHeadCopyItem.Name;
                    timePeriodHead.Description = timePeriodHeadCopyItem.Description;

                    //Set FK
                    timePeriodHead.ActorCompanyId = item.DestinationActorCompanyId;

                    if (timePeriodHead.TimePeriod == null)
                        timePeriodHead.TimePeriod = new EntityCollection<TimePeriod>();

                    item.TemplateCompanyTimeDataItem.AddTimePeriodHeadMapping(timePeriodHeadCopyItem.TimePeriodHeadId, timePeriodHead);

                    foreach (var timePeriodCopyItem in timePeriodHeadCopyItem.TimePeriods)
                    {
                        #region TimePeriod

                        TimePeriod timePeriod = timePeriodHead.TimePeriod.FirstOrDefault(pt => pt.Name == timePeriodHeadCopyItem.Name);
                        if (timePeriod == null)
                        {
                            timePeriod = new TimePeriod();
                            SetCreatedProperties(timePeriod, user);
                            timePeriodHead.TimePeriod.Add(timePeriod);
                        }
                        else
                        {
                            SetModifiedProperties(timePeriod, user);
                        }

                        timePeriod.RowNr = timePeriodCopyItem.RowNr;
                        timePeriod.Name = timePeriodCopyItem.Name;
                        timePeriod.StartDate = timePeriodCopyItem.StartDate;
                        timePeriod.StopDate = timePeriodCopyItem.StopDate;
                        timePeriod.PayrollStartDate = timePeriodCopyItem.PayrollStartDate;
                        timePeriod.PayrollStopDate = timePeriodCopyItem.PayrollStopDate;
                        timePeriod.PaymentDate = timePeriodCopyItem.PaymentDate;

                        #endregion
                    }

                    #endregion
                }

                #endregion

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("PayrollPrice", item, saved: true);

                templateResult.ActionResults.Add(result);

                return templateResult;
            }
        }

        public List<TimePeriodHeadCopyItem> GetTimePeriodHeadCopyItems(int actorCompanyId)
        {
            List<TimePeriodHeadCopyItem> timePeriodHeadCopyItems = new List<TimePeriodHeadCopyItem>();
            var timePeriodHeads = TimePeriodManager.GetTimePeriodHeads(actorCompanyId, TermGroup_TimePeriodType.Unknown, false, false);

            foreach (var timePeriodHead in timePeriodHeads)
            {
                TimePeriodHeadCopyItem item = new TimePeriodHeadCopyItem()
                {
                    TimePeriodHeadId = timePeriodHead.TimePeriodHeadId,
                    Name = timePeriodHead.Name,
                    Description = timePeriodHead.Description,
                    TimePeriodType = (TermGroup_TimePeriodType)timePeriodHead.TimePeriodType,
                    TimePeriods = GetTimePeriodCopyItems(timePeriodHead.TimePeriod)
                };

                timePeriodHeadCopyItems.Add(item);
            }

            return timePeriodHeadCopyItems;
        }

        public List<TimePeriodHeadCopyItem> GetTimePeriodHeadCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimePeriodHeadCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimePeriodHeadCopyItems(sysCompDbId, actorCompanyId);
        }

        private List<TimePeriodCopyItem> GetTimePeriodCopyItems(EntityCollection<TimePeriod> timePeriods)
        {
            List<TimePeriodCopyItem> timePeriodCopyItems = new List<TimePeriodCopyItem>();

            foreach (var timePeriod in timePeriods)
            {
                if (timePeriod.State != (int)SoeEntityState.Active)
                    continue;

                TimePeriodCopyItem item = new TimePeriodCopyItem()
                {
                    Name = timePeriod.Name,
                    StartDate = timePeriod.StartDate,
                    StopDate = timePeriod.StopDate,
                    PayrollStartDate = timePeriod.PayrollStartDate,
                    PayrollStopDate = timePeriod.PayrollStopDate,
                    PaymentDate = timePeriod.PaymentDate,
                    RowNr = timePeriod.RowNr
                };

                timePeriodCopyItems.Add(item);
            }

            return timePeriodCopyItems;
        }

        #endregion

        #region Position
        public TemplateResult CopyPositionsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                var existingPositions = EmployeeManager.GetPositions(entities, item.DestinationActorCompanyId, true);

                #endregion

                foreach (PositionCopyItem templatePosition in item.TemplateCompanyTimeDataItem.PositionCopyItems)
                {
                    #region Position

                    var pos = existingPositions.FirstOrDefault(p => p.Name == templatePosition.Name && p.Code == templatePosition.Code);
                    if (pos == null)
                    {
                        pos = new Position();
                        SetCreatedProperties(pos, user);
                        entities.Position.AddObject(pos);
                    }
                    else
                    {
                        SetModifiedProperties(pos, user);
                    }

                    pos.Code = templatePosition.Code;
                    pos.Name = templatePosition.Name;
                    pos.Description = templatePosition.Description;
                    pos.ActorCompanyId = item.DestinationActorCompanyId;
                    pos.SysPositionId = templatePosition.SysPositionId;

                    #endregion

                    #region PositionSkill

                    // Only supports adding, no updating
                    if (pos.PositionSkill == null)
                        pos.PositionSkill = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<PositionSkill>();
                    else
                    {
                        while (pos.PositionSkill.Any())
                        {
                            entities.DeleteObject(pos.PositionSkill.First());
                        }

                        pos.PositionSkill.Clear();
                    }

                    var saveResult = SaveChanges(entities);
                    if (!saveResult.Success)
                        saveResult = companyTemplateManager.LogCopyError("Positions", item, saved: true);

                    templateResult.ActionResults.Add(saveResult);

                    if (saveResult.Success)
                    {
                        foreach (var tempPosSkill in templatePosition.PositionSkillCopyItems)
                        {
                            var skill = TimeScheduleManager.GetSkill(entities, item.DestinationActorCompanyId, tempPosSkill.SkillName);
                            var skillType = skill?.SkillType;

                            if (skill == null)
                            {
                                if (tempPosSkill?.SkillType != null)
                                {
                                    skillType = new SkillType()
                                    {
                                        ActorCompanyId = item.DestinationActorCompanyId,
                                        Description = tempPosSkill.SkillType.Description,
                                        Name = tempPosSkill.SkillType.Name,
                                    };
                                    SetCreatedProperties(skillType, user);
                                }

                                skill = new Skill()
                                {
                                    ActorCompanyId = item.DestinationActorCompanyId,
                                    Description = tempPosSkill.SkillName,
                                    Name = tempPosSkill.SkillName,
                                    SkillType = skillType,
                                };
                                SetCreatedProperties(skill, user);
                            }

                            var positionSkill = new PositionSkill()
                            {
                                Skill = skill,
                                SkillLevel = tempPosSkill.SkillLevel,
                                Position = pos,
                            };
                            SetCreatedProperties(positionSkill);
                        }
                    }

                    #endregion
                }

                var saveResult2 = SaveChanges(entities);
                if (!saveResult2.Success)
                    saveResult2 = companyTemplateManager.LogCopyError("Positions", item, saved: true);

                templateResult.ActionResults.Add(saveResult2);
                return templateResult;
            }
        }


        public List<PositionCopyItem> GetPositionCopyItems(int actorCompanyId)
        {
            List<PositionCopyItem> positionCopyItems = new List<PositionCopyItem>();
            var positions = EmployeeManager.GetPositions(actorCompanyId, true);

            foreach (var position in positions)
            {
                PositionCopyItem item = new PositionCopyItem()
                {
                    Code = position.Code,
                    Name = position.Name,
                    Description = position.Description,
                    SysPositionId = position.SysPositionId,

                };

                foreach (var skill in position.PositionSkill)
                {
                    item.PositionSkillCopyItems.Add(new PositionSkillCopyItem()
                    {
                        SkillLevel = skill.SkillLevel,
                        SkillName = skill.Skill.Name,
                        SkillType = new SkillTypeCopyItem()
                        {
                            Description = skill.Skill.SkillType.Description,
                            Name = skill.Skill.SkillType.Name
                        }
                    });
                }

                positionCopyItems.Add(item);
            }

            return positionCopyItems;
        }

        public List<PositionCopyItem> GetPositionCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetPositionCopyItems(actorCompanyId);

            return timeTemplateConnector.GetPositionCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region PayrollPriceType and PayrollPriceFormula

        public TemplateResult CopyPayrollGroupsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                #endregion   

                List<PayrollGroup> existingPayrollGroups = PayrollManager.GetPayrollGroups(entities, item.DestinationActorCompanyId, loadPriceTypes: true, loadTimePeriods: true, loadSettings: true, loadAccountStd: true, loadPayrollGroupVacationGroup: true, includePayrollGroupPayrollProduct: true);

                foreach (var templatePayrollGroup in item.TemplateCompanyTimeDataItem.PayrollGroupCopyItems)
                {
                    #region PayrollGroup

                    PayrollGroup payrollGroup = existingPayrollGroups.FirstOrDefault(pt => pt.Name == templatePayrollGroup.Name);
                    if (payrollGroup == null)
                    {
                        payrollGroup = new PayrollGroup()
                        {
                            //Set FK
                            ActorCompanyId = item.DestinationActorCompanyId,
                        };
                        SetCreatedProperties(payrollGroup, user);
                        entities.PayrollGroup.AddObject(payrollGroup);
                    }
                    else
                    {
                        item.TemplateCompanyTimeDataItem.AddPayrollGroupMapping(templatePayrollGroup.PayrollGroupId, payrollGroup);
                        //accourding to rickard - to dangerous
                        continue;
                    }

                    item.TemplateCompanyTimeDataItem.AddPayrollGroupMapping(templatePayrollGroup.PayrollGroupId, payrollGroup);

                    payrollGroup.Name = templatePayrollGroup.Name;

                    #endregion

                    #region TimePeriodHead

                    payrollGroup.TimePeriodHeadId = item.TemplateCompanyTimeDataItem.GetTimePeriodHead(templatePayrollGroup.TimePeriodHeadId ?? 0)?.TimePeriodHeadId;

                    #endregion

                    #region OneTimeTaxFormula

                    payrollGroup.OneTimeTaxFormulaId = item.TemplateCompanyTimeDataItem.GetPriceFormula(templatePayrollGroup.OneTimeTaxFormulaId ?? 0)?.PayrollPriceFormulaId;

                    #endregion

                    #region PayrollGroupSetting


                    foreach (var templateSetting in templatePayrollGroup.PayrollGroupSettings)
                    {
                        var intData = templateSetting.IntData;

                        if (intData.HasValue && templateSetting.Type == (int)PayrollGroupSettingType.PayrollFormula
                            || templateSetting.Type == (int)PayrollGroupSettingType.BygglosenSalaryFormula
                            || templateSetting.Type == (int)PayrollGroupSettingType.ExperienceMonthsFormula
                            || templateSetting.Type == (int)PayrollGroupSettingType.KPADirektSalaryFormula
                            || templateSetting.Type == (int)PayrollGroupSettingType.SkandiaPensionSalaryFormula
                            )
                        {
                            intData = item.TemplateCompanyTimeDataItem.GetPriceFormula(intData ?? 0)?.PayrollPriceFormulaId;
                        }

                        PayrollGroupSetting setting = new PayrollGroupSetting()
                        {
                            Type = templateSetting.Type,
                            DataType = templateSetting.DataType,
                            Name = templateSetting.Name,
                            StrData = templateSetting.StrData,
                            IntData = intData,
                            DecimalData = templateSetting.DecimalData,
                            BoolData = templateSetting.BoolData,
                            DateData = templateSetting.DateData,
                            TimeData = templateSetting.TimeData,

                            //Set references
                            PayrollGroup = payrollGroup,
                        };
                        SetCreatedProperties(setting);
                        payrollGroup.PayrollGroupSetting.Add(setting);
                    }

                    #endregion

                    #region PayrollGroupAccountStd

                    foreach (var templatePayrollGroupAccountStd in templatePayrollGroup.PayrollGroupAccountStds)
                    {

                        var accountStd = item.TemplateCompanyEconomyDataItem.GetAccount(templatePayrollGroupAccountStd.AccountId);
                        if (accountStd == null)
                            continue;

                        PayrollGroupAccountStd newPayrollGroupAccountStd = new PayrollGroupAccountStd()
                        {
                            Type = templatePayrollGroupAccountStd.Type,
                            Percent = templatePayrollGroupAccountStd.Percent,
                            FromInterval = templatePayrollGroupAccountStd.FromInterval,
                            ToInterval = templatePayrollGroupAccountStd.ToInterval,

                            AccountId = accountStd.AccountId,

                            //Set references
                            PayrollGroup = payrollGroup,
                        };
                        SetCreatedProperties(newPayrollGroupAccountStd);
                        payrollGroup.PayrollGroupAccountStd.Add(newPayrollGroupAccountStd);
                    }

                    #endregion

                    #region PayrollGroupPriceType

                    foreach (var templatePayrollGroupPriceType in templatePayrollGroup.PayrollGroupPriceTypes)
                    {
                        PayrollPriceType payrollPriceType = item.TemplateCompanyTimeDataItem.GetPriceType(templatePayrollGroupPriceType.PayrollPriceTypeId);
                        if (payrollPriceType == null)
                            continue;

                        PayrollGroupPriceType payrollGroupPriceType = new PayrollGroupPriceType()
                        {
                            Sort = templatePayrollGroupPriceType.Sort,
                            ShowOnEmployee = templatePayrollGroupPriceType.ShowOnEmployee,
                            ReadOnlyOnEmployee = templatePayrollGroupPriceType.ReadOnlyOnEmployee,

                            //Set references
                            PayrollGroup = payrollGroup,
                            PayrollPriceTypeId = payrollPriceType.PayrollPriceTypeId,
                        };


                        if (!templatePayrollGroupPriceType.PriceTypePeriods.IsNullOrEmpty())
                        {
                            foreach (var period in templatePayrollGroupPriceType.PriceTypePeriods)
                            {
                                payrollGroupPriceType.PayrollGroupPriceTypePeriod.Add(new PayrollGroupPriceTypePeriod()
                                {
                                    Amount = period.Amount,
                                    FromDate = period.FromDate,
                                    Created = DateTime.Now
                                });
                            }
                        }

                        SetCreatedProperties(payrollGroupPriceType);
                        payrollGroup.PayrollGroupPriceType.Add(payrollGroupPriceType);
                    }

                    #endregion

                    #region PayrollGroupPriceFormula

                    foreach (var templatePayrollGroupPriceFormula in templatePayrollGroup.PayrollGroupPriceFormulas)
                    {
                        PayrollPriceFormula payrollPriceFormula = item.TemplateCompanyTimeDataItem.GetPriceFormula(templatePayrollGroupPriceFormula.PayrollPriceFormulaId);
                        if (payrollPriceFormula == null)
                            continue;

                        PayrollGroupPriceFormula payrollGroupPriceFormula = new PayrollGroupPriceFormula()
                        {
                            FromDate = templatePayrollGroupPriceFormula.FromDate,
                            ToDate = templatePayrollGroupPriceFormula.ToDate,
                            ShowOnEmployee = templatePayrollGroupPriceFormula.ShowOnEmployee,

                            //Set references
                            PayrollGroup = payrollGroup,
                            PayrollPriceFormulaId = payrollPriceFormula.PayrollPriceFormulaId,
                        };

                        SetCreatedProperties(payrollGroupPriceFormula);
                        payrollGroup.PayrollGroupPriceFormula.Add(payrollGroupPriceFormula);
                    }

                    #endregion

                    #region PayrollGroupVacationGroup

                    foreach (var templatePayrollGroupVacationGroup in templatePayrollGroup.PayrollGroupVacationGroups)
                    {
                        VacationGroup vacationGroup = item.TemplateCompanyTimeDataItem.GetVacationGroup(templatePayrollGroupVacationGroup.VacationGroupId);
                        if (vacationGroup == null)
                            continue;

                        PayrollGroupVacationGroup payrollGroupVacationGroup = new PayrollGroupVacationGroup()
                        {
                            Default = templatePayrollGroupVacationGroup.Default,

                            //Set references
                            PayrollGroup = payrollGroup,
                            VacationGroupId = vacationGroup.VacationGroupId,
                        };

                        SetCreatedProperties(payrollGroupVacationGroup);
                        payrollGroup.PayrollGroupVacationGroup.Add(payrollGroupVacationGroup);
                    }

                    #endregion

                    #region PayrollGroupPayrollProduct

                    //Moved to own method "CopyPayrollGroupPayrollProductFromTemplateCompany" to be able to load payroll products first

                    #endregion

                    #region PayrollGroupReport

                    foreach (var templatePayrollGroupReport in templatePayrollGroup.PayrollGroupReports)
                    {
                        Report report = item.TemplateCompanyCoreDataItem.GetReport(templatePayrollGroupReport.ReportId);
                        if (report == null)
                            continue;

                        PayrollGroupReport payrollGroupReport = new PayrollGroupReport()
                        {
                            SysReportTemplateTypeId = templatePayrollGroupReport.SysReportTemplateTypeId,

                            //Set FK
                            ActorCompanyId = item.DestinationActorCompanyId,

                            //Set references
                            PayrollGroup = payrollGroup,
                            ReportId = report.ReportId,
                        };
                        SetCreatedProperties(payrollGroupReport);
                        payrollGroup.PayrollGroupReport.Add(payrollGroupReport);
                    }

                    #endregion
                }

                var saveResult = SaveChanges(entities);
                if (!saveResult.Success)
                    saveResult = companyTemplateManager.LogCopyError("PayrollGroup", item, saved: true);
                templateResult.ActionResults.Add(saveResult);

                return templateResult;
            }
        }
        public TemplateResult CopyPayrollGroupPayrollProductFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                #endregion   

                List<PayrollGroup> existingPayrollGroups = PayrollManager.GetPayrollGroups(entities, item.DestinationActorCompanyId, loadPriceTypes: true, loadTimePeriods: true, loadSettings: true, loadAccountStd: true, loadPayrollGroupVacationGroup: true, includePayrollGroupPayrollProduct: true);

                foreach (var templatePayrollGroup in item.TemplateCompanyTimeDataItem.PayrollGroupCopyItems)
                {
                    #region PayrollGroup

                    PayrollGroup payrollGroup = existingPayrollGroups.FirstOrDefault(pt => pt.Name == templatePayrollGroup.Name);
                    if (payrollGroup == null)
                        continue;

                    #endregion

                    #region PayrollGroupPayrollProduct

                    foreach (var templatePayrollGroupPayrollProduct in templatePayrollGroup.PayrollGroupPayrollProducts)
                    {
                        PayrollProduct payrollProduct = item.TemplateCompanyTimeDataItem.GetPayrollProduct(templatePayrollGroupPayrollProduct.ProductId);

                        if (payrollProduct == null)
                            continue;

                        var product = entities.Product.FirstOrDefault(f => f.ProductId == payrollProduct.ProductId) as PayrollProduct;

                        PayrollGroupPayrollProduct payrollGroupPayrollProduct = new PayrollGroupPayrollProduct()
                        {
                            Distribute = templatePayrollGroupPayrollProduct.Distribute,

                            //Set references
                            PayrollGroup = payrollGroup,
                            PayrollProduct = product,
                        };

                        SetCreatedProperties(payrollGroupPayrollProduct);
                        payrollGroup.PayrollGroupPayrollProduct.Add(payrollGroupPayrollProduct);
                    }

                    #endregion

                }

                var saveResult = SaveChanges(entities);
                if (!saveResult.Success)
                    saveResult = companyTemplateManager.LogCopyError("PayrollGroupPayrollProduct", item, saved: true);
                templateResult.ActionResults.Add(saveResult);

                return templateResult;
            }
        }
        public TemplateResult CopyPayrollPriceTypesFromTemplateCompany(TemplateCompanyDataItem item, bool onlyVacationGroup)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<PayrollPriceType> existingPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, item.DestinationActorCompanyId, null, false);
                List<int> validPriceTypeIds = onlyVacationGroup ? new List<int>() : item.TemplateCompanyTimeDataItem.PayrollPriceTypeCopyItems.Select(s => s.PayrollPriceTypeId).ToList();

                if (onlyVacationGroup)
                {
                    foreach (var vacationGroup in item.TemplateCompanyTimeDataItem.VacationGroupCopyItems)
                    {

                        validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.VacationDayPercentPriceTypeId ?? 0);
                        validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.VacationDayAdditionPercentPriceTypeId ?? 0);
                        validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.VacationVariablePercentPriceTypeId ?? 0);
                        validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.GuaranteeAmountPerDayPriceTypeId ?? 0);
                        validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.GuaranteeAmountJuvenilePerDayPriceTypeId ?? 0);
                    }

                    validPriceTypeIds = validPriceTypeIds.Where(w => w != 0).Distinct().ToList();
                }

                #endregion

                foreach (var templatePriceType in item.TemplateCompanyTimeDataItem.PayrollPriceTypeCopyItems.Where(w => validPriceTypeIds.Contains(w.PayrollPriceTypeId)))
                {
                    PayrollPriceType priceType = existingPriceTypes.FirstOrDefault(pt => pt.Code == templatePriceType.Code);
                    if (priceType == null)
                    {
                        priceType = new PayrollPriceType()
                        {
                            //Set FK
                            ActorCompanyId = item.DestinationActorCompanyId,
                        };
                        SetCreatedProperties(priceType, user);
                        entities.PayrollPriceType.AddObject(priceType);
                    }
                    else
                    {
                        item.TemplateCompanyTimeDataItem.AddPriceTypeMapping(templatePriceType.PayrollPriceTypeId, priceType);
                        //accourding to rickard - too dangerous
                        continue;
                    }

                    priceType.Type = (int)templatePriceType.Type;
                    priceType.Code = templatePriceType.Code;
                    priceType.Name = templatePriceType.Name;
                    priceType.Description = templatePriceType.Description;
                    priceType.ConditionEmployedMonths = templatePriceType.ConditionEmployedMonths;
                    priceType.ConditionExperienceMonths = templatePriceType.ConditionExperienceMonths;
                    priceType.ConditionAgeYears = templatePriceType.ConditionAgeYears;

                    if (templatePriceType.PayrollPriceTypePeriods != null)
                    {
                        foreach (var period in templatePriceType.PayrollPriceTypePeriods)
                        {
                            priceType.PayrollPriceTypePeriod.Add(new PayrollPriceTypePeriod()
                            {
                                Amount = period.Amount,
                                FromDate = period.FromDate,
                                Created = DateTime.Now
                            });
                        }
                    }

                    item.TemplateCompanyTimeDataItem.AddPriceTypeMapping(templatePriceType.PayrollPriceTypeId, priceType);
                }

                var saveResult = SaveChanges(entities);
                if (!saveResult.Success)
                    saveResult = companyTemplateManager.LogCopyError("PayrollPriceTypes", item, saved: true);

                templateResult.ActionResults.Add(saveResult);
            }

            return templateResult;
        }

        public TemplateResult CopyPayrollPriceFormulasFromTemplateCompany(TemplateCompanyDataItem item, bool onlyVacationGroup)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                List<PayrollPriceFormula> addedPayrollPriceFormulas = new List<PayrollPriceFormula>();
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<PayrollPriceFormula> existingPayrollPriceFormulas = PayrollManager.GetPayrollPriceFormulas(entities, item.DestinationActorCompanyId, false);

                List<int> validFormulaIds = onlyVacationGroup ? new List<int>() : item.TemplateCompanyTimeDataItem.PayrollPriceFormulaCopyItems.Select(s => s.PayrollPriceFormulaId).ToList();

                if (onlyVacationGroup)
                {
                    foreach (var vacationGroup in item.TemplateCompanyTimeDataItem.VacationGroupCopyItems)
                    {
                        validFormulaIds.Add(vacationGroup.VacationGroupSE?.MonthlySalaryFormulaId ?? 0);
                        validFormulaIds.Add(vacationGroup.VacationGroupSE?.HourlySalaryFormulaId ?? 0);
                    }

                    validFormulaIds = validFormulaIds.Where(w => w != 0).Distinct().ToList();
                }

                #endregion

                foreach (var templatePayrollPriceFormula in item.TemplateCompanyTimeDataItem.PayrollPriceFormulaCopyItems.Where(w => validFormulaIds.Contains(w.PayrollPriceFormulaId)))
                {
                    PayrollPriceFormula priceFormula = existingPayrollPriceFormulas.FirstOrDefault(pf => pf.Code == templatePayrollPriceFormula.Code);

                    if (priceFormula == null)
                    {
                        priceFormula = new PayrollPriceFormula()
                        {
                            //Set FK
                            ActorCompanyId = item.DestinationActorCompanyId,
                        };
                        SetCreatedProperties(priceFormula, user);
                        entities.PayrollPriceFormula.AddObject(priceFormula);
                    }
                    else
                    {
                        item.TemplateCompanyTimeDataItem.AddPriceFormulaMapping(templatePayrollPriceFormula.PayrollPriceFormulaId, priceFormula);
                        //accourding to rickard - too dangerous
                        continue;
                    }

                    priceFormula.Code = templatePayrollPriceFormula.Code;
                    priceFormula.Name = templatePayrollPriceFormula.Name;
                    priceFormula.Description = templatePayrollPriceFormula.Description;
                    priceFormula.Formula = string.Empty; //will be calculated last
                    priceFormula.FormulaPlain = templatePayrollPriceFormula.FormulaPlain;
                    SetCreatedProperties(priceFormula);
                    item.TemplateCompanyTimeDataItem.AddPriceFormulaMapping(templatePayrollPriceFormula.PayrollPriceFormulaId, priceFormula);
                    addedPayrollPriceFormulas.Add(priceFormula);
                }


                var saveResult = SaveChanges(entities);
                if (!saveResult.Success)
                    saveResult = companyTemplateManager.LogCopyError("PayrollPriceFormulas", item, saved: true);
                templateResult.ActionResults.Add(saveResult);


                foreach (var addedPayrollPriceFormula in addedPayrollPriceFormulas)
                {
                    addedPayrollPriceFormula.Formula = PayrollManager.ConvertPayrollPriceFormulaToDB(item.DestinationActorCompanyId, addedPayrollPriceFormula.FormulaPlain);
                    SetModifiedProperties(addedPayrollPriceFormula, user);
                }

                var saveResult2 = SaveChanges(entities);
                if (!saveResult2.Success)
                    saveResult2 = companyTemplateManager.LogCopyError("PayrollPriceFormulasToDB", item, saved: true);
                templateResult.ActionResults.Add(saveResult2);
            }

            return templateResult;
        }

        public List<PayrollPriceTypeCopyItem> GetPayrollPriceTypeCopyItems(int actorCompanyId)
        {
            List<PayrollPriceTypeCopyItem> payrollPriceTypeCopyItems = new List<PayrollPriceTypeCopyItem>();

            // Retrieve payroll price types based on actorCompanyId
            var payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(actorCompanyId, null, loadPeriods: true);

            foreach (var templatePriceType in payrollPriceTypes)
            {
                PayrollPriceTypeCopyItem item = new PayrollPriceTypeCopyItem
                {
                    PayrollPriceTypeId = templatePriceType.PayrollPriceTypeId,
                    Type = (TermGroup_SysPayrollPriceType)templatePriceType.Type,
                    Code = templatePriceType.Code,
                    Name = templatePriceType.Name,
                    Description = templatePriceType.Description,
                    ConditionEmployedMonths = templatePriceType.ConditionEmployedMonths,
                    ConditionExperienceMonths = templatePriceType.ConditionExperienceMonths,
                    ConditionAgeYears = templatePriceType.ConditionAgeYears,
                    PayrollPriceTypePeriods = new List<PayrollPriceTypePeriodCopyItem>()
                };

                if (templatePriceType.PayrollPriceTypePeriod != null)
                {
                    foreach (var period in templatePriceType.PayrollPriceTypePeriod)
                    {
                        item.PayrollPriceTypePeriods.Add(new PayrollPriceTypePeriodCopyItem
                        {
                            Amount = period.Amount,
                            FromDate = period.FromDate,
                        });
                    }
                }

                payrollPriceTypeCopyItems.Add(item);
            }

            return payrollPriceTypeCopyItems;
        }

        public List<PayrollPriceTypeCopyItem> GetPayrollPriceTypeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetPayrollPriceTypeCopyItems(actorCompanyId);

            return timeTemplateConnector.GetPayrollPriceTypeCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<PayrollPriceFormulaCopyItem> GetPayrollPriceFormulaCopyItems(int actorCompanyId)
        {
            List<PayrollPriceFormulaCopyItem> payrollPriceFormulaCopyItems = new List<PayrollPriceFormulaCopyItem>();

            // Retrieve payroll price formulas based on actorCompanyId
            var payrollPriceFormulas = PayrollManager.GetPayrollPriceFormulas(actorCompanyId);

            foreach (var templatePayrollPriceFormula in payrollPriceFormulas)
            {
                PayrollPriceFormulaCopyItem item = new PayrollPriceFormulaCopyItem
                {
                    PayrollPriceFormulaId = templatePayrollPriceFormula.PayrollPriceFormulaId,
                    Code = templatePayrollPriceFormula.Code,
                    Name = templatePayrollPriceFormula.Name,
                    Description = templatePayrollPriceFormula.Description,
                    FormulaPlain = templatePayrollPriceFormula.FormulaPlain
                };

                payrollPriceFormulaCopyItems.Add(item);
            }

            return payrollPriceFormulaCopyItems;
        }

        public List<PayrollPriceFormulaCopyItem> GetPayrollPriceFormulaCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetPayrollPriceFormulaCopyItems(actorCompanyId);

            return timeTemplateConnector.GetPayrollPriceFormulaCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<PayrollGroupCopyItem> GetPayrollGroupCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetPayrollGroupCopyItems(actorCompanyId);

            return timeTemplateConnector.GetPayrollGroupCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<PayrollGroupCopyItem> GetPayrollGroupCopyItems(int actorCompanyId)
        {
            List<PayrollGroupCopyItem> payrollGroupCopyItems = new List<PayrollGroupCopyItem>();

            var payrollGroups = PayrollManager.GetPayrollGroups(actorCompanyId, loadAccountStd: true, loadExternalCode: true, loadPriceTypes: true, loadSettings: true, loadTimePeriods: true, loadPayrollGroupVacationGroup: true, includePayrollGroupPayrollProduct: true, includePriceFormulas: true);

            foreach (var templatePayrollGroup in payrollGroups)
            {
                PayrollGroupCopyItem item = new PayrollGroupCopyItem()
                {
                    PayrollGroupId = templatePayrollGroup.PayrollGroupId,
                    Name = templatePayrollGroup.Name,
                    TimePeriodHeadId = templatePayrollGroup.TimePeriodHeadId,
                    OneTimeTaxFormulaId = templatePayrollGroup.OneTimeTaxFormulaId,
                };


                foreach (var templateSetting in templatePayrollGroup.PayrollGroupSetting)
                {
                    PayrollGroupSettingCopyItem settingItem = new PayrollGroupSettingCopyItem()
                    {
                        Type = templateSetting.Type,
                        DataType = templateSetting.DataType,
                        Name = templateSetting.Name,
                        StrData = templateSetting.StrData,
                        IntData = templateSetting.IntData,
                        DecimalData = templateSetting.DecimalData,
                        BoolData = templateSetting.BoolData,
                        DateData = templateSetting.DateData,
                        TimeData = templateSetting.TimeData,
                    };

                    item.PayrollGroupSettings.Add(settingItem);
                }

                foreach (var templateAccountStd in templatePayrollGroup.PayrollGroupAccountStd)
                {
                    PayrollGroupAccountStdCopyItem accountStdItem = new PayrollGroupAccountStdCopyItem()
                    {
                        Type = templateAccountStd.Type,
                        Percent = templateAccountStd.Percent,
                        FromInterval = templateAccountStd.FromInterval,
                        ToInterval = templateAccountStd.ToInterval,
                        AccountId = templateAccountStd.AccountId,
                    };
                    item.PayrollGroupAccountStds.Add(accountStdItem);
                }

                foreach (var templatePriceType in templatePayrollGroup.PayrollGroupPriceType.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    PayrollGroupPriceTypeCopyItem priceTypeItem = new PayrollGroupPriceTypeCopyItem()
                    {
                        Sort = templatePriceType.Sort,
                        ShowOnEmployee = templatePriceType.ShowOnEmployee,
                        ReadOnlyOnEmployee = templatePriceType.ReadOnlyOnEmployee,
                        PayrollPriceTypeId = templatePriceType.PayrollPriceTypeId,
                    };

                    foreach (var templatePriceTypePeriod in templatePriceType.PayrollGroupPriceTypePeriod.Where(w => w.State == (int)SoeEntityState.Active))
                    {
                        PayrollGroupPriceTypePeriodCopyItem priceTypePeriodItem = new PayrollGroupPriceTypePeriodCopyItem()
                        {
                            Amount = templatePriceTypePeriod.Amount,
                            FromDate = templatePriceTypePeriod.FromDate,
                        };
                        priceTypeItem.PriceTypePeriods.Add(priceTypePeriodItem);
                    }

                    item.PayrollGroupPriceTypes.Add(priceTypeItem);
                }

                foreach (var templatePriceFormula in templatePayrollGroup.PayrollGroupPriceFormula.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    PayrollGroupPriceFormulaCopyItem priceFormulaItem = new PayrollGroupPriceFormulaCopyItem()
                    {
                        FromDate = templatePriceFormula.FromDate,
                        ToDate = templatePriceFormula.ToDate,
                        ShowOnEmployee = templatePriceFormula.ShowOnEmployee,
                        PayrollPriceFormulaId = templatePriceFormula.PayrollPriceFormulaId,
                    };
                    item.PayrollGroupPriceFormulas.Add(priceFormulaItem);
                }

                foreach (var templateVacationGroup in templatePayrollGroup.PayrollGroupVacationGroup.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    PayrollGroupVacationGroupCopyItem vacationGroupItem = new PayrollGroupVacationGroupCopyItem()
                    {
                        Default = templateVacationGroup.Default,
                        VacationGroupId = templateVacationGroup.VacationGroupId,
                    };
                    item.PayrollGroupVacationGroups.Add(vacationGroupItem);
                }

                foreach (var templatePayrollProduct in templatePayrollGroup.PayrollGroupPayrollProduct.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    PayrollGroupPayrollProductCopyItem payrollProductItem = new PayrollGroupPayrollProductCopyItem()
                    {
                        Distribute = templatePayrollProduct.Distribute,
                        ProductId = templatePayrollProduct.ProductId,
                    };
                    item.PayrollGroupPayrollProducts.Add(payrollProductItem);
                }

                foreach (var templateReport in templatePayrollGroup.PayrollGroupReport.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    PayrollGroupReportCopyItem reportItem = new PayrollGroupReportCopyItem()
                    {
                        SysReportTemplateTypeId = templateReport.SysReportTemplateTypeId,
                        ReportId = templateReport.ReportId,
                    };
                    item.PayrollGroupReports.Add(reportItem);
                }

                // Add the item to the list of payroll group copy items
                payrollGroupCopyItems.Add(item);
            }

            return payrollGroupCopyItems;
        }


        #endregion

        #region VacationGroup

        public TemplateResult CopyVacationGroupsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();
            companyTemplateManager.CreateChildCopyItemRequest(item, ChildCopyItemRequestType.TimeDeviationCause, item.TemplateCompanyTimeDataItem.VacationGroupCopyItems.Select(a => a?.VacationGroupSE?.ReplacementTimeDeviationCauseId).Where(w => w.HasValue).Distinct().Select(w => w.Value).ToList());

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<VacationGroup> existingVacationGroups = PayrollManager.GetVacationGroupsWithVacationGroupSE(entities, item.DestinationActorCompanyId);

                #endregion

                foreach (var templateVacationGroup in item.TemplateCompanyTimeDataItem.VacationGroupCopyItems)
                {
                    #region VacationGroup

                    VacationGroup vacationGroup = existingVacationGroups.FirstOrDefault(v => v.Name == templateVacationGroup.Name);
                    if (vacationGroup == null)
                    {
                        vacationGroup = new VacationGroup()
                        {
                            //Set FK
                            ActorCompanyId = item.DestinationActorCompanyId,
                        };
                        SetCreatedProperties(vacationGroup, user);
                        entities.VacationGroup.AddObject(vacationGroup);
                    }
                    else
                    {
                        item.TemplateCompanyTimeDataItem.AddVacationGroupMapping(templateVacationGroup.VacationGroupId, vacationGroup);
                        //accourding to rickard - too dangerous
                        continue;
                    }

                    vacationGroup.Type = templateVacationGroup.Type;
                    vacationGroup.Name = templateVacationGroup.Name;
                    vacationGroup.FromDate = templateVacationGroup.FromDate;
                    vacationGroup.VacationDaysPaidByLaw = templateVacationGroup.VacationDaysPaidByLaw;

                    #endregion

                    #region VacationGroupSE

                    var templateVacationGroupSE = templateVacationGroup.VacationGroupSE;
                    if (templateVacationGroupSE != null)
                    {
                        VacationGroupSE vacationGroupSE = vacationGroup.VacationGroupSE?.FirstOrDefault();
                        if (vacationGroupSE == null)
                        {
                            vacationGroupSE = new VacationGroupSE()
                            {

                                //Set reference
                                VacationGroup = vacationGroup,
                            };
                            SetCreatedProperties(vacationGroupSE);
                            entities.VacationGroupSE.AddObject(vacationGroupSE);
                        }
                        else
                        {

                            //accourding to rickard - too dangerous
                            continue;
                        }

                        //set properties           
                        vacationGroupSE.CalculationType = (int)templateVacationGroupSE.CalculationType;
                        vacationGroupSE.UseAdditionalVacationDays = templateVacationGroupSE.UseAdditionalVacationDays;
                        vacationGroupSE.NbrOfAdditionalVacationDays = templateVacationGroupSE.NbrOfAdditionalVacationDays;
                        vacationGroupSE.AdditionalVacationDaysFromAge1 = templateVacationGroupSE.AdditionalVacationDaysFromAge1;
                        vacationGroupSE.AdditionalVacationDays1 = templateVacationGroupSE.AdditionalVacationDays1;
                        vacationGroupSE.AdditionalVacationDaysFromAge2 = templateVacationGroupSE.AdditionalVacationDaysFromAge2;
                        vacationGroupSE.AdditionalVacationDays2 = templateVacationGroupSE.AdditionalVacationDays2;
                        vacationGroupSE.AdditionalVacationDaysFromAge3 = templateVacationGroupSE.AdditionalVacationDaysFromAge3;
                        vacationGroupSE.AdditionalVacationDays3 = templateVacationGroupSE.AdditionalVacationDays3;
                        vacationGroupSE.VacationHandleRule = (int)templateVacationGroupSE.VacationHandleRule;
                        vacationGroupSE.VacationDaysHandleRule = (int)templateVacationGroupSE.VacationDaysHandleRule;
                        vacationGroupSE.VacationDaysGrossUseFiveDaysPerWeek = templateVacationGroupSE.VacationDaysGrossUseFiveDaysPerWeek;
                        vacationGroupSE.RemainingDaysRule = (int)templateVacationGroupSE.RemainingDaysRule;
                        vacationGroupSE.UseMaxRemainingDays = templateVacationGroupSE.UseMaxRemainingDays;
                        vacationGroupSE.MaxRemainingDays = templateVacationGroupSE.MaxRemainingDays;
                        vacationGroupSE.RemainingDaysPayoutMonth = templateVacationGroupSE.RemainingDaysPayoutMonth;
                        vacationGroupSE.EarningYearAmountFromDate = templateVacationGroupSE.EarningYearAmountFromDate;
                        vacationGroupSE.EarningYearVariableAmountFromDate = templateVacationGroupSE.EarningYearVariableAmountFromDate;
                        vacationGroupSE.VacationDayPercent = templateVacationGroupSE.VacationDayPercent;
                        vacationGroupSE.VacationDayAdditionPercent = templateVacationGroupSE.VacationDayAdditionPercent;
                        vacationGroupSE.VacationVariablePercent = templateVacationGroupSE.VacationVariablePercent;
                        vacationGroupSE.UseGuaranteeAmount = templateVacationGroupSE.UseGuaranteeAmount;
                        vacationGroupSE.GuaranteeAmountAccordingToHandels = templateVacationGroupSE.GuaranteeAmountAccordingToHandels;
                        vacationGroupSE.GuaranteeAmountMaxNbrOfDaysRule = (int)templateVacationGroupSE.GuaranteeAmountMaxNbrOfDaysRule;
                        vacationGroupSE.GuaranteeAmountEmployedNbrOfYears = templateVacationGroupSE.GuaranteeAmountEmployedNbrOfYears;
                        vacationGroupSE.GuaranteeAmountJuvenile = templateVacationGroupSE.GuaranteeAmountJuvenile;
                        vacationGroupSE.GuaranteeAmountJuvenileAgeLimit = templateVacationGroupSE.GuaranteeAmountJuvenileAgeLimit;
                        vacationGroupSE.VacationAbsenceCalculationRule = (int)templateVacationGroupSE.VacationAbsenceCalculationRule;
                        vacationGroupSE.VacationSalaryPayoutRule = (int)templateVacationGroupSE.VacationSalaryPayoutRule;
                        vacationGroupSE.VacationSalaryPayoutDays = templateVacationGroupSE.VacationSalaryPayoutDays;
                        vacationGroupSE.VacationSalaryPayoutMonth = templateVacationGroupSE.VacationSalaryPayoutMonth;
                        vacationGroupSE.VacationVariablePayoutRule = (int)templateVacationGroupSE.VacationVariablePayoutRule;
                        vacationGroupSE.VacationVariablePayoutDays = templateVacationGroupSE.VacationVariablePayoutDays;
                        vacationGroupSE.VacationVariablePayoutMonth = templateVacationGroupSE.VacationVariablePayoutMonth;
                        vacationGroupSE.YearEndRemainingDaysRule = (int)templateVacationGroupSE.YearEndRemainingDaysRule;
                        vacationGroupSE.YearEndOverdueDaysRule = (int)templateVacationGroupSE.YearEndOverdueDaysRule;
                        vacationGroupSE.YearEndVacationVariableRule = (int)templateVacationGroupSE.YearEndVacationVariableRule;
                        vacationGroupSE.ValueDaysAccountInternalOnDebit = templateVacationGroupSE.ValueDaysAccountInternalOnDebit;
                        vacationGroupSE.ValueDaysAccountInternalOnCredit = templateVacationGroupSE.ValueDaysAccountInternalOnCredit;
                        vacationGroupSE.UseEmploymentTaxAcccount = templateVacationGroupSE.UseEmploymentTaxAcccount;
                        vacationGroupSE.EmploymentTaxAccountInternalOnDebit = templateVacationGroupSE.EmploymentTaxAccountInternalOnDebit;
                        vacationGroupSE.EmploymentTaxAccountInternalOnCredit = templateVacationGroupSE.EmploymentTaxAccountInternalOnCredit;
                        vacationGroupSE.UseSupplementChargeAccount = templateVacationGroupSE.UseSupplementChargeAccount;
                        vacationGroupSE.SupplementChargeAccountInternalOnDebit = templateVacationGroupSE.SupplementChargeAccountInternalOnDebit;
                        vacationGroupSE.SupplementChargeAccountInternalOnCredit = templateVacationGroupSE.SupplementChargeAccountInternalOnCredit;


                        //FK Accounts
                        vacationGroupSE.ValueDaysDebitAccountId = item.TemplateCompanyEconomyDataItem.GetAccount(templateVacationGroupSE.ValueDaysDebitAccountId ?? 0)?.AccountId;
                        vacationGroupSE.ValueDaysCreditAccountId = item.TemplateCompanyEconomyDataItem.GetAccount(templateVacationGroupSE.ValueDaysCreditAccountId ?? 0)?.AccountId;
                        vacationGroupSE.EmploymentTaxDebitAccountId = item.TemplateCompanyEconomyDataItem.GetAccount(templateVacationGroupSE.EmploymentTaxDebitAccountId ?? 0)?.AccountId;
                        vacationGroupSE.EmploymentTaxCredidAccountId = item.TemplateCompanyEconomyDataItem.GetAccount(templateVacationGroupSE.EmploymentTaxCredidAccountId ?? 0)?.AccountId;
                        vacationGroupSE.SupplementChargeDebitAccountId = item.TemplateCompanyEconomyDataItem.GetAccount(templateVacationGroupSE.SupplementChargeDebitAccountId ?? 0)?.AccountId;
                        vacationGroupSE.SupplementChargeCreditAccountId = item.TemplateCompanyEconomyDataItem.GetAccount(templateVacationGroupSE.SupplementChargeCreditAccountId ?? 0)?.AccountId;

                        //TimeDeviationCause
                        vacationGroupSE.ReplacementTimeDeviationCauseId = item.TemplateCompanyTimeDataItem.GetTimeDeviationCause(templateVacationGroupSE.ReplacementTimeDeviationCauseId ?? 00)?.TimeDeviationCauseId;

                        //FK
                        vacationGroupSE.MonthlySalaryFormulaId = item.TemplateCompanyTimeDataItem.GetPriceFormula(templateVacationGroupSE.MonthlySalaryFormulaId ?? 0)?.PayrollPriceFormulaId;
                        vacationGroupSE.HourlySalaryFormulaId = item.TemplateCompanyTimeDataItem.GetPriceFormula(templateVacationGroupSE.HourlySalaryFormulaId ?? 0)?.PayrollPriceFormulaId;
                        vacationGroupSE.VacationDayPercentPriceTypeId = item.TemplateCompanyTimeDataItem.GetPriceType(templateVacationGroupSE.VacationDayPercentPriceTypeId ?? 0)?.PayrollPriceTypeId;
                        vacationGroupSE.VacationDayAdditionPercentPriceTypeId = item.TemplateCompanyTimeDataItem.GetPriceType(templateVacationGroupSE.VacationDayAdditionPercentPriceTypeId ?? 0)?.PayrollPriceTypeId;
                        vacationGroupSE.VacationVariablePercentPriceTypeId = item.TemplateCompanyTimeDataItem.GetPriceType(templateVacationGroupSE.VacationVariablePercentPriceTypeId ?? 0)?.PayrollPriceTypeId;
                        vacationGroupSE.GuaranteeAmountPerDayPriceTypeId = item.TemplateCompanyTimeDataItem.GetPriceType(templateVacationGroupSE.GuaranteeAmountPerDayPriceTypeId ?? 0)?.PayrollPriceTypeId;
                        vacationGroupSE.GuaranteeAmountJuvenilePerDayPriceTypeId = item.TemplateCompanyTimeDataItem.GetPriceType(templateVacationGroupSE.GuaranteeAmountJuvenilePerDayPriceTypeId ?? 0)?.PayrollPriceTypeId;
                        item.TemplateCompanyTimeDataItem.AddVacationGroupMapping(templateVacationGroup.VacationGroupId, vacationGroup);
                    }
                }

                #endregion

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("PayrollPrice", item, saved: true);

                templateResult.ActionResults.Add(result);

                return templateResult;
            }
        }


        public List<VacationGroupCopyItem> GetVacationGroupCopyItems(int actorCompanyId)
        {
            List<VacationGroupCopyItem> vacationGroupCopyItems = new List<VacationGroupCopyItem>();

            // Retrieve vacation groups based on actorCompanyId
            var vacationGroups = PayrollManager.GetVacationGroups(actorCompanyId).ToDTOs();

            foreach (var templateVacationGroup in vacationGroups)
            {
                VacationGroupCopyItem item = new VacationGroupCopyItem
                {
                    VacationGroupId = templateVacationGroup.VacationGroupId,
                    Name = templateVacationGroup.Name,
                    FromDate = templateVacationGroup.FromDate,
                    Type = (int)templateVacationGroup.Type,
                    VacationDaysPaidByLaw = templateVacationGroup.VacationDaysPaidByLaw,
                    VacationGroupSE = new VacationGroupSECopyItem
                    {
                        CalculationType = templateVacationGroup.VacationGroupSE.CalculationType,
                        UseAdditionalVacationDays = templateVacationGroup.VacationGroupSE.UseAdditionalVacationDays,
                        NbrOfAdditionalVacationDays = templateVacationGroup.VacationGroupSE.NbrOfAdditionalVacationDays,
                        AdditionalVacationDaysFromAge1 = templateVacationGroup.VacationGroupSE.AdditionalVacationDaysFromAge1,
                        AdditionalVacationDays1 = templateVacationGroup.VacationGroupSE.AdditionalVacationDays1,
                        AdditionalVacationDaysFromAge2 = templateVacationGroup.VacationGroupSE.AdditionalVacationDaysFromAge2,
                        AdditionalVacationDays2 = templateVacationGroup.VacationGroupSE.AdditionalVacationDays2,
                        AdditionalVacationDaysFromAge3 = templateVacationGroup.VacationGroupSE.AdditionalVacationDaysFromAge3,
                        AdditionalVacationDays3 = templateVacationGroup.VacationGroupSE.AdditionalVacationDays3,
                        VacationHandleRule = templateVacationGroup.VacationGroupSE.VacationHandleRule,
                        VacationDaysHandleRule = templateVacationGroup.VacationGroupSE.VacationDaysHandleRule,
                        VacationDaysGrossUseFiveDaysPerWeek = templateVacationGroup.VacationGroupSE.VacationDaysGrossUseFiveDaysPerWeek,
                        RemainingDaysRule = templateVacationGroup.VacationGroupSE.RemainingDaysRule,
                        UseMaxRemainingDays = templateVacationGroup.VacationGroupSE.UseMaxRemainingDays,
                        MaxRemainingDays = templateVacationGroup.VacationGroupSE.MaxRemainingDays,
                        RemainingDaysPayoutMonth = templateVacationGroup.VacationGroupSE.RemainingDaysPayoutMonth,
                        EarningYearAmountFromDate = templateVacationGroup.VacationGroupSE.EarningYearAmountFromDate,
                        EarningYearVariableAmountFromDate = templateVacationGroup.VacationGroupSE.EarningYearVariableAmountFromDate,
                        VacationDayPercent = templateVacationGroup.VacationGroupSE.VacationDayPercent,
                        VacationDayAdditionPercent = templateVacationGroup.VacationGroupSE.VacationDayAdditionPercent,
                        VacationVariablePercent = templateVacationGroup.VacationGroupSE.VacationVariablePercent,
                        UseGuaranteeAmount = templateVacationGroup.VacationGroupSE.UseGuaranteeAmount,
                        GuaranteeAmountAccordingToHandels = templateVacationGroup.VacationGroupSE.GuaranteeAmountAccordingToHandels,
                        GuaranteeAmountMaxNbrOfDaysRule = templateVacationGroup.VacationGroupSE.GuaranteeAmountMaxNbrOfDaysRule,
                        GuaranteeAmountEmployedNbrOfYears = templateVacationGroup.VacationGroupSE.GuaranteeAmountEmployedNbrOfYears,
                        GuaranteeAmountJuvenile = templateVacationGroup.VacationGroupSE.GuaranteeAmountJuvenile,
                        GuaranteeAmountJuvenileAgeLimit = templateVacationGroup.VacationGroupSE.GuaranteeAmountJuvenileAgeLimit,
                        VacationAbsenceCalculationRule = templateVacationGroup.VacationGroupSE.VacationAbsenceCalculationRule,
                        VacationSalaryPayoutRule = templateVacationGroup.VacationGroupSE.VacationSalaryPayoutRule,
                        VacationSalaryPayoutDays = templateVacationGroup.VacationGroupSE.VacationSalaryPayoutDays,
                        VacationSalaryPayoutMonth = templateVacationGroup.VacationGroupSE.VacationSalaryPayoutMonth,
                        VacationVariablePayoutRule = templateVacationGroup.VacationGroupSE.VacationVariablePayoutRule,
                        VacationVariablePayoutDays = templateVacationGroup.VacationGroupSE.VacationVariablePayoutDays,
                        VacationVariablePayoutMonth = templateVacationGroup.VacationGroupSE.VacationVariablePayoutMonth,
                        YearEndRemainingDaysRule = templateVacationGroup.VacationGroupSE.YearEndRemainingDaysRule,
                        YearEndOverdueDaysRule = templateVacationGroup.VacationGroupSE.YearEndOverdueDaysRule,
                        YearEndVacationVariableRule = templateVacationGroup.VacationGroupSE.YearEndVacationVariableRule,
                        ValueDaysAccountInternalOnDebit = templateVacationGroup.VacationGroupSE.ValueDaysAccountInternalOnDebit,
                        ValueDaysAccountInternalOnCredit = templateVacationGroup.VacationGroupSE.ValueDaysAccountInternalOnCredit,
                        UseEmploymentTaxAcccount = templateVacationGroup.VacationGroupSE.UseEmploymentTaxAcccount,
                        EmploymentTaxAccountInternalOnDebit = templateVacationGroup.VacationGroupSE.EmploymentTaxAccountInternalOnDebit,
                        EmploymentTaxAccountInternalOnCredit = templateVacationGroup.VacationGroupSE.EmploymentTaxAccountInternalOnCredit,
                        UseSupplementChargeAccount = templateVacationGroup.VacationGroupSE.UseSupplementChargeAccount,
                        SupplementChargeAccountInternalOnDebit = templateVacationGroup.VacationGroupSE.SupplementChargeAccountInternalOnDebit,
                        SupplementChargeAccountInternalOnCredit = templateVacationGroup.VacationGroupSE.SupplementChargeAccountInternalOnCredit,
                        ReplacementTimeDeviationCauseId = templateVacationGroup.VacationGroupSE.ReplacementTimeDeviationCauseId,
                        MonthlySalaryFormulaId = templateVacationGroup.VacationGroupSE.MonthlySalaryFormulaId,
                        HourlySalaryFormulaId = templateVacationGroup.VacationGroupSE.HourlySalaryFormulaId,
                        VacationDayPercentPriceTypeId = templateVacationGroup.VacationGroupSE.VacationDayPercentPriceTypeId,
                        VacationDayAdditionPercentPriceTypeId = templateVacationGroup.VacationGroupSE.VacationDayAdditionPercentPriceTypeId,
                        VacationVariablePercentPriceTypeId = templateVacationGroup.VacationGroupSE.VacationVariablePercentPriceTypeId,
                        GuaranteeAmountPerDayPriceTypeId = templateVacationGroup.VacationGroupSE.GuaranteeAmountPerDayPriceTypeId,
                        GuaranteeAmountJuvenilePerDayPriceTypeId = templateVacationGroup.VacationGroupSE.GuaranteeAmountJuvenilePerDayPriceTypeId,
                        ValueDaysDebitAccountId = templateVacationGroup.VacationGroupSE.ValueDaysDebitAccountId,
                        ValueDaysCreditAccountId = templateVacationGroup.VacationGroupSE.ValueDaysCreditAccountId,
                        EmploymentTaxDebitAccountId = templateVacationGroup.VacationGroupSE.EmploymentTaxDebitAccountId,
                        EmploymentTaxCredidAccountId = templateVacationGroup.VacationGroupSE.EmploymentTaxCredidAccountId,
                        SupplementChargeDebitAccountId = templateVacationGroup.VacationGroupSE.SupplementChargeDebitAccountId,
                        SupplementChargeCreditAccountId = templateVacationGroup.VacationGroupSE.SupplementChargeCreditAccountId
                    }
                };


                vacationGroupCopyItems.Add(item);
            }

            return vacationGroupCopyItems;
        }

        public List<VacationGroupCopyItem> GetVacationGroupCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetVacationGroupCopyItems(actorCompanyId);

            return timeTemplateConnector.GetVacationGroupCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region ShiftTypeAndTimeScheduleType

        public TemplateResult CopyShiftTypeAndTimeScheduleTypeFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();


            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<TimeScheduleType> existingTimeScheduleTypes = TimeScheduleManager.GetTimeScheduleTypes(entities, item.DestinationActorCompanyId, getAll: true, onlyActive: true);
                List<ShiftType> existingShiftTypes = TimeScheduleManager.GetShiftTypes(entities, item.DestinationActorCompanyId, loadAccounts: true);

                #endregion

                foreach (var timeScheduleTypeCopyItem in item.TemplateCompanyTimeDataItem.TimeScheduleTypeCopyItems)
                {
                    TimeScheduleType timeScheduleType = existingTimeScheduleTypes.FirstOrDefault(i => i.Name == timeScheduleTypeCopyItem.Name && i.Code == timeScheduleTypeCopyItem.Code);
                    if (timeScheduleType == null)
                    {
                        timeScheduleType = new TimeScheduleType()
                        {
                            //Set FK
                            ActorCompanyId = item.DestinationActorCompanyId,
                        };
                        SetCreatedProperties(timeScheduleType, user);
                        entities.TimeScheduleType.AddObject(timeScheduleType);
                    }
                    else
                    {
                        SetModifiedProperties(timeScheduleType, user);
                    }

                    timeScheduleType.Code = timeScheduleTypeCopyItem.Code;
                    timeScheduleType.Name = timeScheduleTypeCopyItem.Name;
                    timeScheduleType.Description = timeScheduleTypeCopyItem.Description;
                    timeScheduleType.IsAll = timeScheduleTypeCopyItem.IsAll;
                    timeScheduleType.IsNotScheduleTime = timeScheduleTypeCopyItem.IsNotScheduleTime;
                    timeScheduleType.IgnoreIfExtraShift = timeScheduleTypeCopyItem.IgnoreIfExtraShift;
                    timeScheduleType.IsBilagaJ = timeScheduleTypeCopyItem.IsBilagaJ;
                    timeScheduleType.ShowInTerminal = timeScheduleTypeCopyItem.ShowInTerminal;
                    timeScheduleType.UseScheduleTimeFactor = timeScheduleTypeCopyItem.UseScheduleTimeFactor;
                    timeScheduleType.TimeDeviationCauseId = item.TemplateCompanyTimeDataItem.GetTimeDeviationCause(timeScheduleTypeCopyItem.TimeDeviationCauseId ?? 0)?.TimeDeviationCauseId;

                    item.TemplateCompanyTimeDataItem.AddTimeScheduleTypeMapping(timeScheduleTypeCopyItem.TimeScheduleTypeId, timeScheduleType);
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("TimeScheduleType", item, saved: true);
                templateResult.ActionResults.Add(result);

                foreach (var shiftTypeCopyItems in item.TemplateCompanyTimeDataItem.ShiftTypeCopyItems)
                {
                    ShiftType shiftType = existingShiftTypes.FirstOrDefault(i => i.Name == shiftTypeCopyItems.Name);
                    if (shiftType == null)
                    {
                        shiftType = new ShiftType()
                        {
                            ActorCompanyId = item.DestinationActorCompanyId
                        };
                        SetCreatedProperties(shiftType, user);
                        entities.ShiftType.AddObject(shiftType);
                    }
                    else
                    {
                        SetModifiedProperties(shiftType, user);
                    }

                    shiftType.Name = shiftTypeCopyItems.Name;
                    shiftType.Description = shiftTypeCopyItems.Description;
                    shiftType.TimeScheduleTemplateBlockType = shiftTypeCopyItems.TimeScheduleTemplateBlockType;
                    shiftType.Color = shiftTypeCopyItems.Color;
                    shiftType.ExternalId = shiftTypeCopyItems.ExternalId;
                    shiftType.ExternalCode = shiftTypeCopyItems.ExternalCode;
                    shiftType.DefaultLength = shiftTypeCopyItems.DefaultLength;
                    shiftType.StartTime = shiftTypeCopyItems.StartTime;
                    shiftType.StopTime = shiftTypeCopyItems.StopTime;
                    shiftType.NeedsCode = shiftTypeCopyItems.NeedsCode;
                    shiftType.HandlingMoney = shiftTypeCopyItems.HandlingMoney;
                    shiftType.TimeScheduleTypeId = item.TemplateCompanyTimeDataItem.GetTimeScheduleType(shiftTypeCopyItems.TimeScheduleTypeId ?? 0)?.TimeScheduleTypeId;

                    if (shiftTypeCopyItems.AccountId.HasValue)
                        shiftType.AccountId = item.TemplateCompanyEconomyDataItem.GetAccount(shiftTypeCopyItems.AccountId.Value)?.AccountId;

                    if (shiftType.ShiftTypeId == 0)
                    {
                        //AccountInternals
                        foreach (int templateAccountInternalId in shiftTypeCopyItems.AccountInternals.Select(a => a.AccountId))
                        {
                            var accountId = item.TemplateCompanyEconomyDataItem.GetAccount(templateAccountInternalId)?.AccountId;
                            AccountInternal accountInternal = null;
                            if (accountId.HasValue)
                                accountInternal = AccountManager.GetAccountInternal(entities, accountId.Value, item.DestinationActorCompanyId);
                            if (accountInternal != null)
                                shiftType.AccountInternal.Add(accountInternal);
                        }
                    }

                    item.TemplateCompanyTimeDataItem.AddShiftTypeMapping(shiftTypeCopyItems.ShiftTypeId, shiftType);
                }

                ActionResult result2 = SaveChanges(entities);
                if (!result2.Success)
                    result2 = companyTemplateManager.LogCopyError("TimeScheduleType", item, saved: true);
                templateResult.ActionResults.Add(result2);
            }

            return templateResult;
        }

        #region TimeScheduleType

        public List<TimeScheduleTypeCopyItem> GetTimeScheduleTypeCopyItems(int actorCompanyId)
        {
            List<TimeScheduleTypeCopyItem> timeScheduleTypeCopyItems = new List<TimeScheduleTypeCopyItem>();

            // Retrieve time schedule types based on actorCompanyId
            var timeScheduleTypes = TimeScheduleManager.GetTimeScheduleTypes(actorCompanyId);

            foreach (var templateTimeScheduleType in timeScheduleTypes)
            {
                TimeScheduleTypeCopyItem item = new TimeScheduleTypeCopyItem
                {
                    TimeScheduleTypeId = templateTimeScheduleType.TimeScheduleTypeId,
                    Code = templateTimeScheduleType.Code,
                    Name = templateTimeScheduleType.Name,
                    Description = templateTimeScheduleType.Description,
                    IsAll = templateTimeScheduleType.IsAll,
                    ShowInTerminal = templateTimeScheduleType.ShowInTerminal,
                    IsNotScheduleTime = templateTimeScheduleType.IsNotScheduleTime,
                    IsBilagaJ = templateTimeScheduleType.IsBilagaJ,
                    IgnoreIfExtraShift = templateTimeScheduleType.IgnoreIfExtraShift,
                    UseScheduleTimeFactor = templateTimeScheduleType.UseScheduleTimeFactor,
                    State = templateTimeScheduleType.State,
                    TimeDeviationCauseId = templateTimeScheduleType.TimeDeviationCauseId
                };

                timeScheduleTypeCopyItems.Add(item);
            }

            return timeScheduleTypeCopyItems;
        }

        public List<TimeScheduleTypeCopyItem> GetTimeScheduleTypeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeScheduleTypeCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeScheduleTypeCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region ShiftType

        public List<ShiftTypeCopyItem> GetShiftTypeCopyItems(int actorCompanyId)
        {
            List<ShiftTypeCopyItem> shiftTypeCopyItems = new List<ShiftTypeCopyItem>();

            // Retrieve shift types based on actorCompanyId
            var shiftTypes = TimeScheduleManager.GetShiftTypes(actorCompanyId);

            foreach (var templateShiftType in shiftTypes)
            {
                ShiftTypeCopyItem item = new ShiftTypeCopyItem
                {
                    ShiftTypeId = templateShiftType.ShiftTypeId,
                    Name = templateShiftType.Name,
                    Description = templateShiftType.Description,
                    TimeScheduleTemplateBlockType = templateShiftType.TimeScheduleTemplateBlockType,
                    Color = templateShiftType.Color,
                    ExternalId = templateShiftType.ExternalId,
                    ExternalCode = templateShiftType.ExternalCode,
                    DefaultLength = templateShiftType.DefaultLength,
                    StartTime = templateShiftType.StartTime,
                    StopTime = templateShiftType.StopTime,
                    NeedsCode = templateShiftType.NeedsCode,
                    HandlingMoney = templateShiftType.HandlingMoney,
                    AccountId = templateShiftType.AccountId,
                    TimeScheduleTypeId = templateShiftType.TimeScheduleTypeId,
                    AccountInternals = templateShiftType.AccountInternal != null ? templateShiftType.AccountInternal.Select(s => new AccountInternalCopyItem() { AccountId = s.AccountId }).ToList() : new List<AccountInternalCopyItem>()
                };

                shiftTypeCopyItems.Add(item);
            }

            return shiftTypeCopyItems;
        }

        public List<ShiftTypeCopyItem> GetShiftTypeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetShiftTypeCopyItems(actorCompanyId);

            return timeTemplateConnector.GetShiftTypeCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #endregion

        #region Skill

        public TemplateResult CopySkillsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));


                List<SkillType> existingSkillTypes = TimeScheduleManager.GetSkillTypes(entities, item.DestinationActorCompanyId);
                List<Skill> existingSkills = TimeScheduleManager.GetSkills(entities, item.DestinationActorCompanyId);

                #endregion

                foreach (var skillCopyItems in item.TemplateCompanyTimeDataItem.SkillCopyItems)
                {
                    if (existingSkills.Any(a => a.Name == skillCopyItems.Name))
                        continue;

                    Skill skill = new Skill()
                    {
                        Name = skillCopyItems.Name,
                        Description = skillCopyItems.Description,
                        ActorCompanyId = item.DestinationActorCompanyId,
                    };
                    entities.Skill.AddObject(skill);
                    existingSkills.Add(skill);

                    if (existingSkillTypes.Any(a => a.Name == skillCopyItems.SkillType.Name))
                    {
                        skill.SkillType = existingSkillTypes.First(a => a.Name == skillCopyItems.SkillType.Name);
                        continue;
                    }

                    SkillType skillType = new SkillType()
                    {
                        Name = skillCopyItems.SkillType.Name,
                        Description = skillCopyItems.SkillType.Description,
                        ActorCompanyId = item.DestinationActorCompanyId,
                    };
                    skill.SkillType = skillType;
                    existingSkillTypes.Add(skillType);
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("TimeScheduleType", item, saved: true);
                templateResult.ActionResults.Add(result);
            }

            return templateResult;
        }

        public List<SkillCopyItem> GetSkillCopyItems(int actorCompanyId)
        {
            List<SkillCopyItem> skillCopyItems = new List<SkillCopyItem>();
            var skillTypes = TimeScheduleManager.GetSkillTypes(actorCompanyId);
            var skills = TimeScheduleManager.GetSkills(actorCompanyId);

            Dictionary<int, SkillTypeCopyItem> skillTypeMappingDict = new Dictionary<int, SkillTypeCopyItem>();

            foreach (var templateSkillType in skillTypes)
            {
                SkillTypeCopyItem skillTypeCopyItem = new SkillTypeCopyItem()
                {
                    Name = templateSkillType.Name,
                    Description = templateSkillType.Description
                };
                skillTypeMappingDict.Add(templateSkillType.SkillTypeId, skillTypeCopyItem);
            }

            foreach (var templateSkill in skills)
            {
                SkillCopyItem skillCopyItem = new SkillCopyItem()
                {
                    Name = templateSkill.Name,
                    Description = templateSkill.Description,
                    SkillType = skillTypeMappingDict[templateSkill.SkillTypeId]
                };
                skillCopyItems.Add(skillCopyItem);
            }

            return skillCopyItems;
        }

        public List<SkillCopyItem> GetSkillCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetSkillCopyItems(actorCompanyId);

            return timeTemplateConnector.GetSkillCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region ScheduleCycle

        public TemplateResult CopyScheduleCyclesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<ScheduleCycleRuleType> existingCycleRuleTypes = new List<ScheduleCycleRuleType>();
                List<ScheduleCycleRule> existingCycleRules = new List<ScheduleCycleRule>();
                List<ScheduleCycle> existingScheduleCycles = TimeScheduleManager.GetScheduleCycleWithRulesAndRuleTypesFromCompany(entities, item.DestinationActorCompanyId);
                foreach (ScheduleCycle existingScheduleCycle in existingScheduleCycles)
                {
                    foreach (ScheduleCycleRule existingCycleRule in existingScheduleCycle.ScheduleCycleRule)
                    {
                        if (!existingCycleRules.Any(w => w.ScheduleCycleRuleId == existingCycleRule.ScheduleCycleRuleId))
                            existingCycleRules.Add(existingCycleRule);

                        if (!existingCycleRuleTypes.Any(w => w.ScheduleCycleRuleTypeId == existingCycleRule.ScheduleCycleRuleTypeId))
                            existingCycleRuleTypes.Add(existingCycleRule.ScheduleCycleRuleType);
                    }
                }

                #endregion

                foreach (var scheduleCycleCopyItem in item.TemplateCompanyTimeDataItem.ScheduleCycleCopyItems)
                {
                    #region ScheduleCycle

                    ScheduleCycle newScheduleCycle = existingScheduleCycles.FirstOrDefault(p => p.Name == scheduleCycleCopyItem.Name);
                    if (newScheduleCycle != null)
                        continue;

                    newScheduleCycle = new ScheduleCycle()
                    {
                        Name = scheduleCycleCopyItem.Name,
                        Description = scheduleCycleCopyItem.Description,
                        NbrOfWeeks = scheduleCycleCopyItem.NbrOfWeeks,
                        ActorCompanyId = item.DestinationActorCompanyId,
                    };
                    SetCreatedProperties(newScheduleCycle, user);
                    entities.ScheduleCycle.AddObject(newScheduleCycle);

                    #endregion

                    #region ScheduleCycleRule

                    foreach (var scheduleCycleRuleCopyItem in scheduleCycleCopyItem.ScheduleCycleRules)
                    {
                        ScheduleCycleRule existingScheduleCycleRule = existingCycleRules.FirstOrDefault(f => f.MinOccurrences == scheduleCycleRuleCopyItem.MinOccurrences && f.MaxOccurrences == scheduleCycleRuleCopyItem.MinOccurrences && f.ScheduleCycleRuleType.Name == scheduleCycleRuleCopyItem.ScheduleCycleRuleType.Name);
                        if (existingScheduleCycleRule != null)
                            continue;

                        ScheduleCycleRule newScheduleCycleRule = new ScheduleCycleRule()
                        {
                            MaxOccurrences = scheduleCycleRuleCopyItem.MaxOccurrences,
                            MinOccurrences = scheduleCycleRuleCopyItem.MinOccurrences
                        };

                        if (scheduleCycleRuleCopyItem.ScheduleCycleRuleType != null)
                        {
                            ScheduleCycleRuleType existingScheduleCycleRuleType = existingCycleRuleTypes.FirstOrDefault(f => f.Name == scheduleCycleRuleCopyItem.ScheduleCycleRuleType.Name);
                            if (existingScheduleCycleRuleType != null)
                            {
                                newScheduleCycleRule.ScheduleCycleRuleType = existingScheduleCycleRuleType;
                            }
                            else
                            {
                                ScheduleCycleRuleType newScheduleCycleRuleType = new ScheduleCycleRuleType()
                                {
                                    Name = scheduleCycleRuleCopyItem.ScheduleCycleRuleType.Name,
                                    DayOfWeeks = scheduleCycleRuleCopyItem.ScheduleCycleRuleType.DayOfWeeks,
                                    StartTime = scheduleCycleRuleCopyItem.ScheduleCycleRuleType.StartTime,
                                    StopTime = scheduleCycleRuleCopyItem.ScheduleCycleRuleType.StopTime,
                                    ActorCompanyId = item.DestinationActorCompanyId
                                };

                                existingCycleRuleTypes.Add(newScheduleCycleRuleType);
                                newScheduleCycleRule.ScheduleCycleRuleType = newScheduleCycleRuleType;
                                entities.ScheduleCycleRuleType.AddObject(newScheduleCycleRuleType);
                            }
                        }

                        existingCycleRules.Add(newScheduleCycleRule);
                        newScheduleCycle.ScheduleCycleRule.Add(newScheduleCycleRule);
                    }

                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("ScheduleCycles", item, saved: true);
                    templateResult.ActionResults.Add(result);

                    #endregion
                }
            }

            return templateResult;
        }

        public List<ScheduleCycleCopyItem> GetScheduleCycleCopyItems(int actorCompanyId)
        {
            List<ScheduleCycleCopyItem> scheduleCycleCopyItems = new List<ScheduleCycleCopyItem>();

            // Retrieve existing schedule cycles with rules and rule types based on actorCompanyId
            var existingScheduleCycles = TimeScheduleManager.GetScheduleCycles(actorCompanyId);

            foreach (var existingScheduleCycle in existingScheduleCycles)
            {
                ScheduleCycleCopyItem item = new ScheduleCycleCopyItem
                {
                    Name = existingScheduleCycle.Name,
                    Description = existingScheduleCycle.Description,
                    NbrOfWeeks = existingScheduleCycle.NbrOfWeeks,
                    ScheduleCycleRules = new List<ScheduleCycleRuleCopyItem>()
                };

                foreach (var existingCycleRule in existingScheduleCycle.ScheduleCycleRule)
                {
                    ScheduleCycleRuleCopyItem ruleItem = new ScheduleCycleRuleCopyItem
                    {
                        MaxOccurrences = existingCycleRule.MaxOccurrences,
                        MinOccurrences = existingCycleRule.MinOccurrences,
                        ScheduleCycleRuleType = new ScheduleCycleRuleTypeCopyItem
                        {
                            Name = existingCycleRule.ScheduleCycleRuleType.Name,
                            DayOfWeeks = existingCycleRule.ScheduleCycleRuleType.DayOfWeeks,
                            StartTime = existingCycleRule.ScheduleCycleRuleType.StartTime,
                            StopTime = existingCycleRule.ScheduleCycleRuleType.StopTime
                        }
                    };

                    item.ScheduleCycleRules.Add(ruleItem);
                }

                scheduleCycleCopyItems.Add(item);
            }

            return scheduleCycleCopyItems;
        }

        public List<ScheduleCycleCopyItem> GetScheduleCycleCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetScheduleCycleCopyItems(actorCompanyId);

            return timeTemplateConnector.GetScheduleCycleCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region FollowUpType

        public TemplateResult CopyFollowUpTypesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<FollowUpType> existingFollowUpTypes = EmployeeManager.GetFollowUpTypes(entities, item.DestinationActorCompanyId, true);

                #endregion

                foreach (var followUpTypeCopyItem in item.TemplateCompanyTimeDataItem.FollowUpTypeCopyItems)
                {
                    if (existingFollowUpTypes.Any(a => a.Name == followUpTypeCopyItem.Name))
                        continue;

                    FollowUpType followUpType = new FollowUpType()
                    {
                        Type = followUpTypeCopyItem.Type,
                        Name = followUpTypeCopyItem.Name,
                        State = followUpTypeCopyItem.State,
                        ActorCompanyId = item.DestinationActorCompanyId
                    };

                    entities.FollowUpType.AddObject(followUpType);
                    existingFollowUpTypes.Add(followUpType);
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("FollowUpType", item, saved: true);
                templateResult.ActionResults.Add(result);
            }

            return templateResult;
        }


        public List<FollowUpTypeCopyItem> GetFollowUpTypeCopyItems(int actorCompanyId)
        {
            List<FollowUpTypeCopyItem> followUpTypeCopyItems = new List<FollowUpTypeCopyItem>();

            var followUpTypes = EmployeeManager.GetFollowUpTypes(actorCompanyId, true);

            foreach (var followUpType in followUpTypes)
            {
                FollowUpTypeCopyItem item = new FollowUpTypeCopyItem
                {
                    Type = followUpType.Type,
                    Name = followUpType.Name,
                };

                followUpTypeCopyItems.Add(item);
            }

            return followUpTypeCopyItems;
        }

        public List<FollowUpTypeCopyItem> GetFollowUpTypeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetFollowUpTypeCopyItems(actorCompanyId);

            return timeTemplateConnector.GetFollowUpTypeCopyItems(sysCompDbId, actorCompanyId);
        }


        #endregion

        #region PayRollProduct and TimeCode

        #region PayrollProduct

        private PayrollProduct GetExistingPayrollProduct(List<PayrollProduct> existingProducts, List<PayrollProductCopyItem> templateProducts, int templateProductId)
        {
            var templateProduct = templateProducts.FirstOrDefault(i => i.PayrollProductId == templateProductId);
            if (templateProduct == null)
                return null;

            return existingProducts.FirstOrDefault(p => p.Number.Trim().ToLower().Equals(templateProduct.Number.Trim().ToLower())) ??
                   existingProducts.FirstOrDefault(p => p.Name.Equals(templateProduct.Name, StringComparison.OrdinalIgnoreCase));
        }

        public List<PayrollProductCopyItem> GetPayrollProductCopyItems(int actorCompanyId)
        {
            List<PayrollProductCopyItem> payrollProductCopyItems = new List<PayrollProductCopyItem>();

            var payrollProducts = ProductManager.GetPayrollProducts(actorCompanyId, true, true, true, true);

            foreach (var payrollProduct in payrollProducts)
            {
                PayrollProductCopyItem item = new PayrollProductCopyItem
                {
                    SysPayrollProductId = payrollProduct.SysPayrollProductId,
                    PayrollProductId = payrollProduct.ProductId,
                    ProductGroupId = null,
                    ProductUnitId = null,

                    PayrollType = payrollProduct.PayrollType,
                    ResultType = payrollProduct.ResultType,
                    SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                    SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                    SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                    SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,
                    Number = payrollProduct.Number,
                    Name = payrollProduct.Name,
                    ShortName = payrollProduct.ShortName,
                    Description = payrollProduct.Description,
                    ExternalNumber = payrollProduct.ExternalNumber,
                    AccountingPrio = payrollProduct.AccountingPrio,
                    Factor = payrollProduct.Factor,
                    AverageCalculated = payrollProduct.AverageCalculated,
                    DontUseFixedAccounting = payrollProduct.DontUseFixedAccounting,
                    Export = payrollProduct.Export,
                    ExcludeInWorkTimeSummary = payrollProduct.ExcludeInWorkTimeSummary,
                    IncludeAmountInExport = payrollProduct.IncludeAmountInExport,
                    Payed = payrollProduct.Payed,
                    UseInPayroll = payrollProduct.UseInPayroll,
                    State = (int)SoeEntityState.Active,
                };

                foreach (var setting in payrollProduct.PayrollProductSetting.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    PayrollProductSettingCopyItem settingCopyItem = new PayrollProductSettingCopyItem
                    {
                        ChildProductId = setting.ChildProductId,
                        PayrollGroupId = setting.PayrollGroupId,

                        QuantityRoundingMinutes = setting.QuantityRoundingMinutes,
                        QuantityRoundingType = setting.QuantityRoundingType,
                        CentRoundingType = setting.CentRoundingType,
                        CentRoundingLevel = setting.CentRoundingLevel,
                        TaxCalculationType = setting.TaxCalculationType,
                        TimeUnit = setting.TimeUnit,
                        PensionCompany = setting.PensionCompany,
                        AccountingPrio = setting.AccountingPrio,
                        CalculateSicknessSalary = setting.CalculateSicknessSalary,
                        CalculateSupplementCharge = setting.CalculateSupplementCharge,
                        DontPrintOnSalarySpecificationWhenZeroAmount = setting.DontPrintOnSalarySpecificationWhenZeroAmount,
                        DontIncludeInRetroactivePayroll = setting.DontIncludeInRetroactivePayroll,
                        DontIncludeInAbsenceCost = setting.DontIncludeInAbsenceCost,
                        PrintOnSalarySpecification = setting.PrintOnSalarySpecification,
                        PrintDate = setting.PrintDate,
                        UnionFeePromoted = setting.UnionFeePromoted,
                        VacationSalaryPromoted = setting.VacationSalaryPromoted,
                        WorkingTimePromoted = setting.WorkingTimePromoted,
                    };

                    foreach (var account in setting.PayrollProductAccountStd)
                    {
                        PayrollProductAccountStdCopyItem accountCopyItem = new PayrollProductAccountStdCopyItem
                        {
                            Type = account.Type,
                            Percent = account.Percent,
                            AccountId = account.AccountId,
                            AccountInternalIds = !account.AccountInternal.IsNullOrEmpty() ? account.AccountInternal.Select(s => s.AccountId).ToList() : new List<int>()
                        };
                        settingCopyItem.PayrollProductAccountStdCopyItems.Add(accountCopyItem);
                    }

                    foreach (var formula in setting.PayrollProductPriceFormula.Where(w => w.State == (int)SoeEntityState.Active))
                    {
                        PayrollProductPriceFormulaCopyItem formulaCopyItem = new PayrollProductPriceFormulaCopyItem
                        {
                            FromDate = formula.FromDate,
                            ToDate = formula.ToDate,
                            PayrollPriceFormulaId = formula.PayrollPriceFormulaId
                        };
                        settingCopyItem.PayrollProductPriceFormulaCopyItems.Add(formulaCopyItem);
                    }

                    foreach (var priceType in setting.PayrollProductPriceType.Where(w => w.State == (int)SoeEntityState.Active))
                    {
                        PayrollProductPriceTypeCopyItem priceTypeCopyItem = new PayrollProductPriceTypeCopyItem
                        {
                            PayrollPriceTypeId = priceType.PayrollPriceTypeId
                        };
                        foreach (var period in priceType.PayrollProductPriceTypePeriod.Where(w => w.State == (int)SoeEntityState.Active))
                        {
                            PayrollProductPriceTypePeriodCopyItem periodCopyItem = new PayrollProductPriceTypePeriodCopyItem
                            {
                                Amount = period.Amount,
                                FromDate = period.FromDate
                            };
                            priceTypeCopyItem.PayrollProductPriceTypePeriodCopyItems.Add(periodCopyItem);
                        }

                        settingCopyItem.PayrollProductPriceTypeCopyItems.Add(priceTypeCopyItem);
                    }

                    item.PayrollProductSettings.Add(settingCopyItem);
                }

                payrollProductCopyItems.Add(item);
            }

            return payrollProductCopyItems;
        }

        public List<PayrollProductCopyItem> GetPayrollProductCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetPayrollProductCopyItems(actorCompanyId);

            return timeTemplateConnector.GetPayrollProductCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyPayrollProductsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                //Existing for new Company
                List<PayrollProduct> existingPayrollProducts = item.Update ? ProductManager.GetPayrollProducts(entities, item.DestinationActorCompanyId, active: true, loadPriceTypesAndPriceFormulas: true, loadPayrollProductSettingAccounts: true) : new List<PayrollProduct>();

                #endregion

                #region PayrollProduct

                foreach (var templatePayrollProduct in item.TemplateCompanyTimeDataItem.PayrollProductCopyItems)
                {
                    #region PayrollProduct

                    PayrollProduct payrollProduct = null;
                    if (item.Update)
                        payrollProduct = GetExistingPayrollProduct(existingPayrollProducts, item.TemplateCompanyTimeDataItem.PayrollProductCopyItems, templatePayrollProduct.PayrollProductId);

                    if (payrollProduct == null)
                    {
                        payrollProduct = new PayrollProduct();
                        SetCreatedProperties(payrollProduct);
                    }
                    else if (payrollProduct != null && !templatePayrollProduct.PayrollProductSettings.IsNullOrEmpty() && payrollProduct.PayrollProductSetting.IsNullOrEmpty())
                    {
                        SetModifiedProperties(payrollProduct);
                    }
                    else
                    {
                        item.TemplateCompanyTimeDataItem.AddPayrollProductMapping(templatePayrollProduct.PayrollProductId, payrollProduct);
                        continue; //according to rickard - too dangerous
                    }

                    payrollProduct.SetProperties(templatePayrollProduct);

                    if (payrollProduct.ProductId == 0)
                        newCompany.Product.Add(payrollProduct);

                    item.TemplateCompanyTimeDataItem.AddPayrollProductMapping(templatePayrollProduct.PayrollProductId, payrollProduct);


                    #endregion
                }

                #region Save

                var result1 = SaveChanges(entities);
                if (!result1.Success)
                {
                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("PayrollProduct", item, saved: true));
                    return templateResult;
                }
                templateResult.ActionResults.Add(result1);
                #endregion

                #region PayrollProductSetting

                foreach (var templatePayrollProduct in item.TemplateCompanyTimeDataItem.PayrollProductCopyItems)
                {
                    PayrollProduct payrollProduct = item.TemplateCompanyTimeDataItem.GetPayrollProduct(templatePayrollProduct.PayrollProductId);
                    if (payrollProduct == null)
                        continue;

                    if (payrollProduct.PayrollProductSetting.Count > 0)
                        continue;

                    bool usedGenericSetting = false;
                    foreach (var templatePayrollProductSetting in templatePayrollProduct.PayrollProductSettings)
                    {
                        #region PayrollProductSetting (Can only have one PayrollProductSetting without PayrollGroup)

                        PayrollGroup payrollGroup = item.TemplateCompanyTimeDataItem.GetPayrollGroup(templatePayrollProductSetting.PayrollGroupId ?? 0);
                        if (payrollGroup == null)
                        {
                            if (usedGenericSetting)
                                continue;
                            usedGenericSetting = true;
                        }

                        PayrollProduct childPayrollProduct = item.TemplateCompanyTimeDataItem.GetPayrollProduct(templatePayrollProductSetting.ChildProductId ?? 0);
                        PayrollProductSetting payrollProductSetting = ProductManager.CreatePayrollProductSetting(payrollProduct, templatePayrollProductSetting, payrollGroup?.PayrollGroupId, childPayrollProduct?.ProductId);
                        entities.PayrollProductSetting.AddObject(payrollProductSetting);

                        #endregion

                        #region PayrollProductAccountStd

                        foreach (var templatePayrollProductAccountStd in templatePayrollProductSetting.PayrollProductAccountStdCopyItems)
                        {
                            var accountStd = item.TemplateCompanyEconomyDataItem.GetAccount(templatePayrollProductAccountStd.AccountId ?? 0);

                            PayrollProductAccountStd payrollProductAccountStd = new PayrollProductAccountStd()
                            {
                                Type = templatePayrollProductAccountStd.Type,
                                Percent = templatePayrollProductAccountStd.Percent,

                                //Set FK
                                AccountId = accountStd?.AccountId,

                                //Set reference
                                PayrollProductSetting = payrollProductSetting,
                            };
                            entities.PayrollProductAccountStd.AddObject(payrollProductAccountStd);

                            #region PayrollProductAccountInternal

                            foreach (var accountId in templatePayrollProductAccountStd.AccountInternalIds)
                            {
                                var account = item.TemplateCompanyEconomyDataItem.GetAccount(accountId);
                                if (account != null)
                                {
                                    var accountInternal = AccountManager.GetAccountInternal(entities, account.AccountId, item.DestinationActorCompanyId);
                                    if (accountInternal != null)
                                        payrollProductAccountStd.AccountInternal.Add(accountInternal);
                                }
                            }

                            #endregion
                        }

                        #endregion

                        #region PayrollProductPriceFormula

                        foreach (var templatePayrollProductPriceFormula in templatePayrollProductSetting.PayrollProductPriceFormulaCopyItems)
                        {
                            PayrollPriceFormula payrollPriceFormula = item.TemplateCompanyTimeDataItem.GetPriceFormula(templatePayrollProductPriceFormula.PayrollPriceFormulaId);

                            if (payrollPriceFormula == null)
                                continue;

                            PayrollProductPriceFormula payrollProductPriceFormula = new PayrollProductPriceFormula()
                            {
                                FromDate = templatePayrollProductPriceFormula.FromDate,
                                ToDate = templatePayrollProductPriceFormula.ToDate,

                                //Set FK
                                PayrollPriceFormulaId = payrollPriceFormula.PayrollPriceFormulaId,

                                //Set reference
                                PayrollProductSetting = payrollProductSetting,
                            };
                            SetCreatedProperties(payrollProductPriceFormula);
                            entities.PayrollProductPriceFormula.AddObject(payrollProductPriceFormula);
                        }

                        #endregion

                        #region PayrollProductPriceType

                        foreach (var templatePayrollProductPriceType in templatePayrollProductSetting.PayrollProductPriceTypeCopyItems)
                        {
                            PayrollPriceType payrollPriceType = item.TemplateCompanyTimeDataItem.GetPriceType(templatePayrollProductPriceType.PayrollPriceTypeId);

                            if (payrollPriceType == null)
                                continue;

                            PayrollProductPriceType payrollProductPriceType = new PayrollProductPriceType()
                            {
                                //Set FK
                                PayrollPriceTypeId = payrollPriceType.PayrollPriceTypeId,

                                //Set reference
                                PayrollProductSetting = payrollProductSetting,
                            };
                            SetCreatedProperties(payrollProductPriceType);
                            entities.PayrollProductPriceType.AddObject(payrollProductPriceType);

                            #region PayrollProductPriceTypePeriod

                            foreach (var templatePayrollProductPriceTypePeriod in templatePayrollProductPriceType.PayrollProductPriceTypePeriodCopyItems)
                            {
                                PayrollProductPriceTypePeriod payrollProductPriceTypePeriod = new PayrollProductPriceTypePeriod()
                                {
                                    Amount = templatePayrollProductPriceTypePeriod.Amount,
                                    FromDate = templatePayrollProductPriceTypePeriod.FromDate,

                                    //Set references
                                    PayrollProductPriceType = payrollProductPriceType,
                                };
                                SetCreatedProperties(payrollProductPriceType);
                                entities.PayrollProductPriceTypePeriod.AddObject(payrollProductPriceTypePeriod);
                            }

                            #endregion
                        }

                        #endregion
                    }
                }

                #endregion

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("PayrollProduct", item, saved: true);
                templateResult.ActionResults.Add(result);

                #endregion
            }


            return templateResult;
        }

        #endregion

        #region InvoiceProduct

        public List<InvoiceProductCopyItem> GetInvoiceProductCopyItems(int actorCompanyId)
        {
            BillingTemplateManager billingTemplateManager = new BillingTemplateManager(base.parameterObject);

            var result = billingTemplateManager.GetInvoiceProductCopyItems(actorCompanyId);
            return result;
        }

        public List<InvoiceProductCopyItem> GetInvoiceProductCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetInvoiceProductCopyItems(actorCompanyId);

            return timeTemplateConnector.GetInvoiceProductCopyItems(sysCompDbId, actorCompanyId);
        }
        public TemplateResult CopyInvoiceProductsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();



            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                List<InvoiceProduct> existingInvoiceProducts = ProductManager.GetInvoiceProducts(entities, item.DestinationActorCompanyId, active: true);

                #endregion

                #region InvoiceProduct

                var copyItems = item.TemplateCompanyBillingDataItem.InvoiceProductCopyItems.IsNullOrEmpty() ? item.TemplateCompanyTimeDataItem.InvoiceProductCopyItems : item.TemplateCompanyBillingDataItem.InvoiceProductCopyItems;
                List<ProductUnit> existingProductUnits = ProductManager.GetProductUnits(entities, item.DestinationActorCompanyId).ToList() ?? new List<ProductUnit>();
                List<ProductGroup> existingProductGroups = ProductGroupManager.GetProductGroups(entities, item.DestinationActorCompanyId).ToList() ?? new List<ProductGroup>();

                foreach (InvoiceProductCopyItem templateInvoiceProduct in copyItems)
                {

                    InvoiceProduct invoiceProduct = existingInvoiceProducts.FirstOrDefault(f => f.Name.ToLower() == templateInvoiceProduct.Name.ToLower() && f.Number.ToLower() == templateInvoiceProduct.Number.ToLower());// GetExistingInvoiceProduct(existingInvoiceProducts, templateInvoiceProducts, templateInvoiceProduct.ProductId);
                    invoiceProduct = new BillingTemplateManager(base.parameterObject).CopyInvoiceProduct(entities, invoiceProduct, templateInvoiceProduct, item, newCompany, false, existingProductUnits, existingProductGroups);

                    #region Save

                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                    {
                        result = companyTemplateManager.LogCopyError("InvoiceProduct", item, saved: true);
                    }
                    else
                    {
                        item.TemplateCompanyTimeDataItem.AddInvoiceProductMapping(templateInvoiceProduct.ProductId, invoiceProduct);
                    }
                    templateResult.ActionResults.Add(result);

                    #endregion
                }

                #endregion
            }

            return templateResult;
        }

        #endregion

        #region TimeCode

        public TemplateResult CopyTimeCodesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                ActionResult result;
                List<TimeCodeBreakGroup> existingTimeCodeBreakGroups = TimeCodeManager.GetTimeCodeBreakGroups(entities, item.DestinationActorCompanyId);
                List<TimeCode> existingTimeCodes = item.Update ? TimeCodeManager.GetTimeCodes(entities, item.DestinationActorCompanyId, true, true) : new List<TimeCode>();

                #region TimeCodeBreakGroup

                foreach (TimeCodeBreakGroupCopyItem templateTimeCodeBreakGroup in item.TemplateCompanyTimeDataItem.TimeCodeBreakGroupCopyItems)
                {
                    #region TimeCodeBreakGroup

                    TimeCodeBreakGroup timeCodeBreakGroup = item.Update ? existingTimeCodeBreakGroups.FirstOrDefault(t => t.Name == templateTimeCodeBreakGroup.Name) : null;
                    if (timeCodeBreakGroup != null)
                        continue;
                    //according to rickard - too dangerous

                    timeCodeBreakGroup = new TimeCodeBreakGroup()
                    {
                        Name = templateTimeCodeBreakGroup.Name,
                        Description = templateTimeCodeBreakGroup.Description,

                        //Set references
                        Company = newCompany,
                    };
                    SetCreatedProperties(timeCodeBreakGroup);
                    existingTimeCodeBreakGroups.Add(timeCodeBreakGroup);

                    result = SaveChanges(entities);
                    if (!result.Success)
                    {
                        companyTemplateManager.LogCopyError("TimeCodeBreakGroup", item, saved: true);
                        break;
                    }

                    item.TemplateCompanyTimeDataItem.AddTimeCodeBreakGroupMapping(templateTimeCodeBreakGroup.TimeCodeBreakGroupId, timeCodeBreakGroup);

                    #endregion
                }

                #endregion

                #region TimeCode

                foreach (TimeCodeCopyItem templateTimeCode in item.TemplateCompanyTimeDataItem.TimeCodeCopyItems)
                {
                    #region TimeCode

                    TimeCode timeCode = item.Update ? existingTimeCodes.FirstOrDefault(t => t.Name == templateTimeCode.Name && t.Code == templateTimeCode.Code && t.Type == templateTimeCode.Type) : null;
                    if (timeCode == null)
                    {
                        timeCode = templateTimeCode.CopyTimeCode(item.TemplateCompanyTimeDataItem, newCompany);
                        if (timeCode == null)
                            continue;

                        SetCreatedProperties(timeCode);
                        entities.TimeCode.AddObject(timeCode);
                        existingTimeCodes.Add(timeCode);

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("TimeCode", item, saved: true));
                            break;
                        }
                    }

                    item.TemplateCompanyTimeDataItem.AddTimeCodeMapping(templateTimeCode.TimeCodeId, timeCode);

                    #endregion

                    #region TimeCodePayrollProduct

                    if (timeCode.TimeCodePayrollProduct != null && timeCode.TimeCodePayrollProduct.Count == 0)
                    {
                        foreach (var templateTimeCodePayrollProduct in templateTimeCode.TimeCodePayrollProducts)
                        {
                            PayrollProduct payrollProduct = item.TemplateCompanyTimeDataItem.GetPayrollProduct(templateTimeCodePayrollProduct.ProductId);
                            if (payrollProduct != null)
                            {
                                TimeCodePayrollProduct timeCodePayrollProduct = new TimeCodePayrollProduct()
                                {
                                    Factor = templateTimeCodePayrollProduct.Factor,

                                    //Set references
                                    TimeCode = timeCode,
                                    PayrollProduct = entities.Product.FirstOrDefault(f => f.ProductId == payrollProduct.ProductId) as PayrollProduct,
                                };
                                SetCreatedProperties(timeCodePayrollProduct);
                                entities.TimeCodePayrollProduct.AddObject(timeCodePayrollProduct);
                            }
                        }

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("TimeCodePayrollProduct", item, saved: true));
                            break;
                        }
                    }

                    #endregion

                    #region TimeCodeInvoiceProduct

                    if (timeCode.TimeCodeInvoiceProduct != null && timeCode.TimeCodeInvoiceProduct.Count == 0)
                    {
                        foreach (var templateTimeCodeInvoiceProduct in templateTimeCode.TimeCodeInvoiceProducts)
                        {
                            InvoiceProduct invoiceProduct = item.TemplateCompanyTimeDataItem.GetInvoiceProduct(templateTimeCodeInvoiceProduct.ProductId);
                            if (invoiceProduct != null)
                            {
                                TimeCodeInvoiceProduct timeCodeInvoiceProduct = new TimeCodeInvoiceProduct()
                                {
                                    Factor = templateTimeCodeInvoiceProduct.Factor,

                                    //Set references
                                    TimeCode = timeCode,
                                    InvoiceProduct = entities.Product.FirstOrDefault(f => f.ProductId == invoiceProduct.ProductId) as InvoiceProduct,
                                };
                                SetCreatedProperties(timeCodeInvoiceProduct);
                                entities.TimeCodeInvoiceProduct.AddObject(timeCodeInvoiceProduct);
                            }
                        }

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("TimeCodeInvoiceProduct", item, saved: true));
                            break;
                        }
                    }

                    #endregion
                }

                #region Rounding / Adjustment

                foreach (var templateTimeCode in item.TemplateCompanyTimeDataItem.TimeCodeCopyItems.Where(x => x.RoundingTimeCodeId.HasValue || x.RoundingInterruptionTimeCodeId.HasValue || x.AdjustQuantityTimeCodeId.HasValue || x.AdjustQuantityTimeScheduleTypeId.HasValue))
                {
                    var timeCode = item.TemplateCompanyTimeDataItem.GetTimeCode(templateTimeCode.TimeCodeId);
                    if (timeCode == null)
                        continue;

                    if (templateTimeCode.RoundingTimeCodeId.HasValue && !timeCode.RoundingTimeCodeId.HasValue)
                        timeCode.RoundingTimeCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(templateTimeCode.RoundingTimeCodeId.Value)?.TimeCodeId;
                    if (templateTimeCode.RoundingInterruptionTimeCodeId.HasValue && !timeCode.RoundingInterruptionTimeCodeId.HasValue)
                        timeCode.RoundingInterruptionTimeCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(templateTimeCode.RoundingInterruptionTimeCodeId.Value)?.TimeCodeId;
                    if (templateTimeCode.AdjustQuantityTimeCodeId.HasValue && !timeCode.AdjustQuantityTimeCodeId.HasValue)
                        timeCode.AdjustQuantityTimeCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(templateTimeCode.AdjustQuantityTimeCodeId.Value)?.TimeCodeId;
                    if (templateTimeCode.AdjustQuantityTimeScheduleTypeId.HasValue && !timeCode.AdjustQuantityTimeScheduleTypeId.HasValue)
                        timeCode.AdjustQuantityTimeScheduleTypeId = item.TemplateCompanyTimeDataItem.GetTimeScheduleType(templateTimeCode.AdjustQuantityTimeScheduleTypeId.Value)?.TimeScheduleTypeId;
                }

                #endregion

                #region TimeCodeRule / AdjustQuantityTimeCodeId (Must be done after all timeCodes are copied)

                foreach (var templateTimeCode in item.TemplateCompanyTimeDataItem.TimeCodeCopyItems.Where(x =>
                    PayrollRulesUtil.IsBreak((SoeTimeCodeType)x.Type) ||
                    PayrollRulesUtil.IsWork((SoeTimeCodeType)x.Type) ||
                    PayrollRulesUtil.IsAbsence((SoeTimeCodeType)x.Type)
                    ))
                {
                    #region TimeCodeRule

                    TimeCode timeCode = item.TemplateCompanyTimeDataItem.GetTimeCode(templateTimeCode.TimeCodeId);
                    if (timeCode == null)
                        continue;

                    while (timeCode.TimeCodeRule.Any())
                    {
                        entities.DeleteObject(timeCode.TimeCodeRule.First());
                    }
                    timeCode.TimeCodeRule.Clear();

                    foreach (var templateTimeCodeRule in templateTimeCode.TimeCodeRules)
                    {
                        TimeCode timeCodeForRule = item.TemplateCompanyTimeDataItem.GetTimeCode(templateTimeCodeRule.Value);
                        if (timeCodeForRule == null)
                            continue;

                        TimeCodeRule rule = new TimeCodeRule()
                        {
                            Type = templateTimeCodeRule.Type,
                            Value = timeCodeForRule.TimeCodeId,
                            Time = templateTimeCodeRule.Time,

                            //Set reference
                            TimeCode = existingTimeCodes.FirstOrDefault(f => f.TimeCodeId == timeCode.TimeCodeId),

                        };
                        entities.TimeCodeRule.AddObject(rule);
                    }

                    #endregion
                }

                #endregion

                #endregion

                result = SaveChanges(entities);
                if (!result.Success)
                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("TimeCode", item, saved: true));

            }

            return templateResult;
        }
        public TemplateResult CopyTimeCodeRankingFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();
            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                ActionResult result;
                List<TimeCodeRankingGroup> existingTimeCodeRankingGroups = TimeCodeManager.GetTimeCodeRankingGroups(entities, actorCompanyId: item.DestinationActorCompanyId);

                #region TimeCodeRanking

                foreach (TimeCodeRankingGroupCopyItem timeCodeRankingGroupCopyitem in item.TemplateCompanyTimeDataItem.TimeCodeRankingGroupCopyItems)
                {
                    TimeCodeRankingGroup timeCodeRankingGroup = item.Update ? existingTimeCodeRankingGroups.FirstOrDefault(t => t.StartDate == timeCodeRankingGroupCopyitem.StartDate && t.StopDate == timeCodeRankingGroupCopyitem.StopDate && t.State == (int)SoeEntityState.Active) : null;
                    if (timeCodeRankingGroup != null)
                        continue;
                    //according to rickard - too dangerous

                    TimeCodeRankingGroup newTimeCodeRankingGroup = new TimeCodeRankingGroup()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        StartDate = timeCodeRankingGroupCopyitem.StartDate,
                        StopDate = timeCodeRankingGroupCopyitem.StopDate,
                        Description = timeCodeRankingGroupCopyitem.Description,
                    };
                    SetCreatedProperties(newTimeCodeRankingGroup);
                    entities.TimeCodeRankingGroup.AddObject(newTimeCodeRankingGroup);

                    foreach (var timeCodeRankingCopyitem in timeCodeRankingGroupCopyitem.TimeCodeRankings)
                    {
                        TimeCodeRanking newTimeCodeRanking = new TimeCodeRanking()
                        {
                            LeftTimeCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(timeCodeRankingCopyitem.LeftTimeCodeId)?.TimeCodeId ?? 0,
                            RightTimeCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(timeCodeRankingCopyitem.RightTimeCodeId)?.TimeCodeId ?? 0,
                            OperatorType = timeCodeRankingCopyitem.OperatorType,
                            ActorCompanyId = item.DestinationActorCompanyId,
                            TimeCodeRankingGroup = newTimeCodeRankingGroup,
                        };

                        SetCreatedProperties(newTimeCodeRanking);
                        entities.TimeCodeRanking.AddObject(newTimeCodeRanking);
                    }

                }
                result = SaveChanges(entities);
                if (!result.Success)
                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("TimeCodeRanking", item, saved: true));

                templateResult.ActionResults.Add(result);

                #endregion

                return templateResult;
            } 
        }
        public List<TimeCodeCopyItem> GetTimeCodeCopyItems(int actorCompanyId)
        {
            List<TimeCodeCopyItem> timeCodeCopyItems = new List<TimeCodeCopyItem>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var timeCodes = entitiesReadOnly.TimeCode
                .Include("TimeCodePayrollProduct")
                .Include("TimeCodeInvoiceProduct")
                .Include("TimeCodeRule")
                .Where(tc => tc.ActorCompanyId == actorCompanyId && tc.State == (int)SoeEntityState.Active)
                .ToList();

            foreach (var timeCode in timeCodes)
            {
                TimeCodeCopyItem item = new TimeCodeCopyItem
                {
                    TimeCodeId = timeCode.TimeCodeId,
                    Code = timeCode.Code,
                    Name = timeCode.Name,
                    Description = timeCode.Description,
                    Type = timeCode.Type,
                    RegistrationType = timeCode.RegistrationType,
                    Classification = timeCode.Classification,
                    Payed = timeCode.Payed,
                    MinutesByConstantRules = timeCode.MinutesByConstantRules,
                    FactorBasedOnWorkPercentage = timeCode.FactorBasedOnWorkPercentage,

                    //Rounding
                    RoundingType = timeCode.RoundingType,
                    RoundingValue = timeCode.RoundingValue,
                    RoundingTimeCodeId = timeCode.RoundingTimeCodeId,
                    RoundingInterruptionTimeCodeId = timeCode.RoundingInterruptionTimeCodeId,
                    RoundingGroupKey = timeCode.RoundingGroupKey,
                    RoundStartTime = timeCode.RoundStartTime,

                    //Adjustment
                    AdjustQuantityByBreakTime = timeCode.AdjustQuantityByBreakTime,
                    AdjustQuantityTimeCodeId = timeCode.AdjustQuantityTimeCodeId,
                    AdjustQuantityTimeScheduleTypeId = timeCode.AdjustQuantityTimeScheduleTypeId,
                };

                switch (timeCode.Type)
                {
                    case (int)SoeTimeCodeType.Work:
                        if (timeCode is TimeCodeWork timeCodeWork)
                        {
                            item.IsWorkOutsideSchedule = timeCodeWork.IsWorkOutsideSchedule;
                        }
                        break;
                    case (int)SoeTimeCodeType.Absense:
                        if (timeCode is TimeCodeAbsense timeCodeAbsense)
                        {
                            item.IsAbsence = timeCodeAbsense.IsAbsence;
                            item.KontekId = timeCodeAbsense.KontekId;
                        }
                        break;
                    case (int)SoeTimeCodeType.Break:
                        if (timeCode is TimeCodeBreak timeCodeBreak)
                        {
                            item.MinMinutes = timeCodeBreak.MinMinutes;
                            item.MaxMinutes = timeCodeBreak.MaxMinutes;
                            item.DefaultMinutes = timeCodeBreak.DefaultMinutes;
                            item.StartType = timeCodeBreak.StartType;
                            item.StopType = timeCodeBreak.StopType;
                            item.StartTimeMinutes = timeCodeBreak.StartTimeMinutes;
                            item.StopTimeMinutes = timeCodeBreak.StopTimeMinutes;
                            item.StartTime = timeCodeBreak.StartTime;
                            item.TimeCodeBreakGroupId = timeCodeBreak.TimeCodeBreakGroupId;
                        }
                        break;
                    case (int)SoeTimeCodeType.AdditionDeduction:
                        if (timeCode is TimeCodeAdditionDeduction timeCodeAdditionDeduction)
                        {
                            item.ExpenseType = (TermGroup_ExpenseType)timeCodeAdditionDeduction.ExpenseType;
                            item.Comment = timeCodeAdditionDeduction.Comment;
                            item.StopAtDateStart = timeCodeAdditionDeduction.StopAtDateStart;
                            item.StopAtDateStop = timeCodeAdditionDeduction.StopAtDateStop;
                            item.StopAtPrice = timeCodeAdditionDeduction.StopAtPrice;
                            item.StopAtVat = timeCodeAdditionDeduction.StopAtVat;
                            item.StopAtAccounting = timeCodeAdditionDeduction.StopAtAccounting;
                            item.StopAtComment = timeCodeAdditionDeduction.StopAtComment;
                            item.CommentMandatory = timeCodeAdditionDeduction.CommentMandatory;
                            item.HideForEmployee = timeCodeAdditionDeduction.HideForEmployee;
                            item.ShowInTerminal = timeCodeAdditionDeduction.ShowInTerminal;
                            item.FixedQuantity = timeCodeAdditionDeduction.FixedQuantity;
                        }
                        break;
                    case (int)SoeTimeCodeType.Material:
                        if (timeCode is TimeCodeMaterial timeCodeMaterial)
                        {
                            item.Note = timeCodeMaterial.Note;
                        }
                        break;
                }

                if (timeCode.TimeCodePayrollProduct != null)
                {
                    foreach (var payrollProduct in timeCode.TimeCodePayrollProduct)
                    {
                        item.TimeCodePayrollProducts.Add(new TimeCodePayrollProductCopyItem()
                        {
                            ProductId = payrollProduct.ProductId,
                            Factor = payrollProduct.Factor,
                        });
                    }
                }
                if (timeCode.TimeCodeInvoiceProduct != null)
                {
                    foreach (var invoiceProduct in timeCode.TimeCodeInvoiceProduct)
                    {
                        item.TimeCodeInvoiceProducts.Add(new TimeCodeInvoiceProductCopyItem()
                        {
                            ProductId = invoiceProduct.ProductId,
                            Factor = invoiceProduct.Factor,
                        });
                    }
                }
                if (timeCode.TimeCodeRule != null)
                {
                    foreach (var timeCodeRule in timeCode.TimeCodeRule)
                    {
                        item.TimeCodeRules.Add(new TimeCodeRuleCopyItem()
                        {
                            Type = timeCodeRule.Type,
                            Value = timeCodeRule.Value,
                            Time = timeCodeRule.Time,
                        });
                    }
                }

                timeCodeCopyItems.Add(item);
            }

            return timeCodeCopyItems;
        }

        public List<TimeCodeCopyItem> GetTimeCodeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeCodeCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeCodeCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<TimeBreakTemplateCopyItem> GetTimeBreakTemplateCopyItems(int actorCompanyId)
        {
            List<TimeBreakTemplateCopyItem> timeBreakTemplateCopyItems = new List<TimeBreakTemplateCopyItem>();

            var timeBreakTemplateList = TimeScheduleManager.GetTimeBreakTemplates(actorCompanyId);

            foreach (var timeBreakTemplate in timeBreakTemplateList)
            {
                TimeBreakTemplateCopyItem item = new TimeBreakTemplateCopyItem
                {
                    TimeBreakTemplateId = timeBreakTemplate.TimeBreakTemplateId,
                    StartDate = timeBreakTemplate.StartDate,
                    StopDate = timeBreakTemplate.StopDate,
                    UseMaxWorkTimeBetweenBreaks = timeBreakTemplate.UseMaxWorkTimeBetweenBreaks,
                    ShiftLength = timeBreakTemplate.ShiftLength,
                    ShiftStartFromTime = timeBreakTemplate.ShiftStartFromTime,
                    MinTimeBetweenBreaks = timeBreakTemplate.MinTimeBetweenBreaks,
                    DayOfWeeks = timeBreakTemplate.DayOfWeeks
                };

                foreach (var shiftType in timeBreakTemplate.ShiftTypes)
                {
                    ShiftTypeCopyItem shiftTypeCopyItem = new ShiftTypeCopyItem
                    {
                        ShiftTypeId = shiftType.ShiftTypeId,
                    };

                    item.ShiftTypes.Add(shiftTypeCopyItem);
                }

                foreach (var templateRow in timeBreakTemplate.TimeBreakTemplateRow)
                {
                    TimeBreakTemplateRowCopyItem rowCopyItem = new TimeBreakTemplateRowCopyItem
                    {
                        Type = templateRow.Type,
                        MinTimeAfterStart = templateRow.MinTimeAfterStart,
                        MinTimeBeforeEnd = templateRow.MinTimeBeforeEnd,
                        TimeCodeBreakGroupId = templateRow.TimeCodeBreakGroupId
                    };

                    item.TimeBreakTemplateRows.Add(rowCopyItem);
                }

                timeBreakTemplateCopyItems.Add(item);
            }


            return timeBreakTemplateCopyItems;
        }

        public List<TimeBreakTemplateCopyItem> GetTimeBreakTemplateCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeBreakTemplateCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeBreakTemplateCopyItems(sysCompDbId, actorCompanyId);  // Implement this method
        }

        public List<TimeCodeBreakGroupCopyItem> GetTimeCodeBreakGroupCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeCodeBreakGroupCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeCodeBreakGroupCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<TimeCodeRankingGroupCopyItem> GetTimeCodeRankingGroupCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeCodeRankingGroupCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeCodeRankingGroupCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<TimeCodeRankingGroupCopyItem> GetTimeCodeRankingGroupCopyItems(int actorCompanyId)
        {
            List<TimeCodeRankingGroupCopyItem> timeCodeRankingGroupCopyItems = new List<TimeCodeRankingGroupCopyItem>();

            var timeCodeRankingGroup = TimeCodeManager.GetTimeCodeRankingGroups(actorCompanyId: actorCompanyId, loadRankings: true);
            foreach (var group in timeCodeRankingGroup)
            {
                TimeCodeRankingGroupCopyItem item = new TimeCodeRankingGroupCopyItem
                {
                    TimeCodeRankingGroupId = group.TimeCodeRankingGroupId,
                    ActorCompanyId = group.ActorCompanyId,
                    StartDate = group.StartDate,
                    StopDate = group.StopDate,
                    Description = group.Description,

                    TimeCodeRankings = group.TimeCodeRanking.Where(w=> w.State == (int)SoeEntityState.Active).Select(ranking => new TimeCodeRankingCopyItem
                    {
                        TimeCodeRankingId = ranking.TimeCodeRankingId,
                        ActorCompanyId = ranking.ActorCompanyId,
                        LeftTimeCodeId = ranking.LeftTimeCodeId,
                        RightTimeCodeId = ranking.RightTimeCodeId,
                        OperatorType = ranking.OperatorType,
                        TimeCodeRankingGroupId = group.TimeCodeRankingGroupId
                    }).ToList()

                };
                timeCodeRankingGroupCopyItems.Add(item);

            }
            return timeCodeRankingGroupCopyItems;
        }

        public List<TimeCodeBreakGroupCopyItem> GetTimeCodeBreakGroupCopyItems(int actorCompanyId)
        {
            List<TimeCodeBreakGroupCopyItem> timeCodeBreakGroupCopyItems = new List<TimeCodeBreakGroupCopyItem>();

            var timeCodeBreakGroupList = TimeCodeManager.GetTimeCodeBreakGroups(actorCompanyId);

            foreach (var timeCodeBreakGroup in timeCodeBreakGroupList)
            {
                TimeCodeBreakGroupCopyItem item = new TimeCodeBreakGroupCopyItem
                {
                    TimeCodeBreakGroupId = timeCodeBreakGroup.TimeCodeBreakGroupId,
                    Name = timeCodeBreakGroup.Name,
                    Description = timeCodeBreakGroup.Description
                };

                timeCodeBreakGroupCopyItems.Add(item);
            }

            return timeCodeBreakGroupCopyItems;
        }

        public TemplateResult CopyTimeBreakTemplatesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                List<TimeBreakTemplate> existingTimeBreakTemplates = TimeScheduleManager.GetTimeBreakTemplates(entities, item.DestinationActorCompanyId);

                foreach (TimeBreakTemplate existingTimeBreakTemplate in existingTimeBreakTemplates)
                    ChangeEntityState(existingTimeBreakTemplate, SoeEntityState.Deleted);

                foreach (TimeBreakTemplateCopyItem templateTimeBreakTemplate in item.TemplateCompanyTimeDataItem.TimeBreakTemplateCopyItems)
                {
                    #region TimeBreakTemplate

                    TimeBreakTemplate timeBreakTemplate = new TimeBreakTemplate()
                    {
                        StartDate = templateTimeBreakTemplate.StartDate,
                        StopDate = templateTimeBreakTemplate.StopDate,
                        UseMaxWorkTimeBetweenBreaks = templateTimeBreakTemplate.UseMaxWorkTimeBetweenBreaks,
                        ShiftLength = templateTimeBreakTemplate.ShiftLength,
                        ShiftStartFromTime = templateTimeBreakTemplate.ShiftStartFromTime,
                        MinTimeBetweenBreaks = templateTimeBreakTemplate.MinTimeBetweenBreaks,
                        DayOfWeeks = templateTimeBreakTemplate.DayOfWeeks,

                        // Set FK
                        ActorCompanyId = item.DestinationActorCompanyId,
                    };
                    SetCreatedProperties(timeBreakTemplate);
                    entities.TimeBreakTemplate.AddObject(timeBreakTemplate);

                    #endregion

                    #region ShiftTypes

                    if (!templateTimeBreakTemplate.ShiftTypes.IsNullOrEmpty())
                    {
                        foreach (var templateShiftType in templateTimeBreakTemplate.ShiftTypes)
                        {
                            ShiftType existingShiftType = item.TemplateCompanyTimeDataItem.GetShiftType(templateShiftType.ShiftTypeId);
                            if (existingShiftType != null)
                                timeBreakTemplate.ShiftTypes.Add(entities.ShiftType.First(f => f.ShiftTypeId == existingShiftType.ShiftTypeId));
                        }
                    }

                    #endregion

                    #region TimeBreakTemplateRows

                    if (!templateTimeBreakTemplate.TimeBreakTemplateRows.IsNullOrEmpty())
                    {
                        foreach (var templateTimeBreakTemplateRow in templateTimeBreakTemplate.TimeBreakTemplateRows)
                        {
                            TimeCodeBreakGroup existingTimeCodeBreakGroup = item.TemplateCompanyTimeDataItem.GetTimeCodeBreakGroup(templateTimeBreakTemplateRow.TimeCodeBreakGroupId ?? 0);

                            if (existingTimeCodeBreakGroup == null)
                                continue;

                            TimeBreakTemplateRow timeBreakTemplateRow = new TimeBreakTemplateRow()
                            {
                                Type = templateTimeBreakTemplateRow.Type,
                                MinTimeAfterStart = templateTimeBreakTemplateRow.MinTimeAfterStart,
                                MinTimeBeforeEnd = templateTimeBreakTemplateRow.MinTimeBeforeEnd,

                                // References
                                TimeCodeBreakGroup = entities.TimeCodeBreakGroup.FirstOrDefault(f => existingTimeCodeBreakGroup.TimeCodeBreakGroupId == f.TimeCodeBreakGroupId),
                            };
                            SetCreatedProperties(timeBreakTemplateRow);
                            timeBreakTemplate.TimeBreakTemplateRow.Add(timeBreakTemplateRow);
                        }
                    }

                    #endregion

                    #region Save

                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                    {
                        result = companyTemplateManager.LogCopyError("TimeBreakTemplate", item, saved: true);
                        templateResult.ActionResults.Add(result);
                        break;
                    }

                    #endregion
                }

            }
            return templateResult;
        }

        #endregion

        #endregion

        #region TimeDeviationCause

        public TemplateResult CopyTimeDeviationCausesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<TimeDeviationCause> existingTimeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, item.DestinationActorCompanyId);
                List<int> filterIds = item.GetIdsFromChildCopyItemRequest(ChildCopyItemRequestType.TimeDeviationCause);

                #endregion

                foreach (var timeDeviationCauseCopyItem in item.TemplateCompanyTimeDataItem.TimeDeviationCauseCopyItems.Where(w => filterIds == null || filterIds.Contains(w.TimeDeviationCauseId)))
                {
                    TimeDeviationCause timeDeviationCause = existingTimeDeviationCauses.FirstOrDefault(t => t.Name == timeDeviationCauseCopyItem.Name ||
                        (!string.IsNullOrEmpty(t.ExtCode) && !string.IsNullOrEmpty(timeDeviationCauseCopyItem.ExtCode) && t.ExtCode == timeDeviationCauseCopyItem.ExtCode));

                    if (timeDeviationCause == null)
                    {
                        timeDeviationCause = new TimeDeviationCause() { Company = newCompany };
                        SetCreatedProperties(timeDeviationCause);
                    }
                    else
                    {
                        if (!item.Update)
                        {
                            item.TemplateCompanyTimeDataItem.AddTimeDeviationCauseMapping(timeDeviationCauseCopyItem.TimeDeviationCauseId, timeDeviationCause);
                            continue;
                        }

                        SetModifiedProperties(timeDeviationCause);
                    }
                    item.TemplateCompanyTimeDataItem.AddTimeDeviationCauseMapping(timeDeviationCauseCopyItem.TimeDeviationCauseId, timeDeviationCause);
                    timeDeviationCause.Type = timeDeviationCauseCopyItem.Type;
                    timeDeviationCause.Name = timeDeviationCauseCopyItem.Name;
                    timeDeviationCause.Description = timeDeviationCauseCopyItem.Description;
                    timeDeviationCause.ExtCode = timeDeviationCauseCopyItem.ExtCode;
                    timeDeviationCause.ImageSource = timeDeviationCauseCopyItem.ImageSource;
                    timeDeviationCause.EmployeeRequestPolicyNbrOfDaysBefore = timeDeviationCauseCopyItem.EmployeeRequestPolicyNbrOfDaysBefore;
                    timeDeviationCause.EmployeeRequestPolicyNbrOfDaysBeforeCanOverride = timeDeviationCauseCopyItem.EmployeeRequestPolicyNbrOfDaysBeforeCanOverride;
                    timeDeviationCause.AttachZeroDaysNbrOfDaysBefore = timeDeviationCauseCopyItem.AttachZeroDaysNbrOfDaysBefore;
                    timeDeviationCause.AttachZeroDaysNbrOfDaysAfter = timeDeviationCauseCopyItem.AttachZeroDaysNbrOfDaysAfter;
                    timeDeviationCause.ChangeDeviationCauseAccordingToPlannedAbsence = timeDeviationCauseCopyItem.ChangeDeviationCauseAccordingToPlannedAbsence;
                    timeDeviationCause.ChangeCauseOutsideOfPlannedAbsence = timeDeviationCauseCopyItem.ChangeCauseOutsideOfPlannedAbsence;
                    timeDeviationCause.ChangeCauseInsideOfPlannedAbsence = timeDeviationCauseCopyItem.ChangeCauseInsideOfPlannedAbsence;
                    timeDeviationCause.AdjustTimeOutsideOfPlannedAbsence = timeDeviationCauseCopyItem.AdjustTimeOutsideOfPlannedAbsence;
                    timeDeviationCause.AdjustTimeInsideOfPlannedAbsence = timeDeviationCauseCopyItem.AdjustTimeInsideOfPlannedAbsence;
                    timeDeviationCause.AllowGapToPlannedAbsence = timeDeviationCauseCopyItem.AllowGapToPlannedAbsence;
                    timeDeviationCause.ShowZeroDaysInAbsencePlanning = timeDeviationCauseCopyItem.ShowZeroDaysInAbsencePlanning;
                    timeDeviationCause.IsVacation = timeDeviationCauseCopyItem.IsVacation;
                    timeDeviationCause.Payed = timeDeviationCauseCopyItem.Payed;
                    timeDeviationCause.NotChargeable = timeDeviationCauseCopyItem.NotChargeable;
                    timeDeviationCause.OnlyWholeDay = timeDeviationCauseCopyItem.OnlyWholeDay;
                    timeDeviationCause.SpecifyChild = timeDeviationCauseCopyItem.SpecifyChild;
                    timeDeviationCause.ExcludeFromPresenceWorkRules = timeDeviationCauseCopyItem.ExcludeFromPresenceWorkRules;
                    timeDeviationCause.ExcludeFromScheduleWorkRules = timeDeviationCauseCopyItem.ExcludeFromScheduleWorkRules;
                    timeDeviationCause.ValidForHibernating = timeDeviationCauseCopyItem.ValidForHibernating;
                    timeDeviationCause.ValidForStandby = timeDeviationCauseCopyItem.ValidForStandby;
                    timeDeviationCause.CandidateForOvertime = timeDeviationCauseCopyItem.CandidateForOvertime;
                    timeDeviationCause.MandatoryNote = timeDeviationCauseCopyItem.MandatoryNote;
                    timeDeviationCause.MandatoryTime = timeDeviationCauseCopyItem.MandatoryTime;
                    timeDeviationCause.State = timeDeviationCauseCopyItem.State;
                    timeDeviationCause.TimeCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(timeDeviationCauseCopyItem.TimeCodeId ?? 0)?.TimeCodeId;
                    timeDeviationCause.CalculateAsOtherTimeInSales = timeDeviationCauseCopyItem.CalculateAsOtherTimeInSales;
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("TimeDeviationCause", item, saved: true);
                templateResult.ActionResults.Add(result);
            }

            return templateResult;
        }

        public List<TimeDeviationCauseCopyItem> GetTimeDeviationCauseCopyItems(int actorCompanyId)
        {
            List<TimeDeviationCauseCopyItem> timeDeviationCauseCopyItems = new List<TimeDeviationCauseCopyItem>();

            var timeDeviationCauseList = TimeDeviationCauseManager.GetTimeDeviationCauses(actorCompanyId); // You might need to adjust this call

            foreach (var timeDeviationCause in timeDeviationCauseList)
            {
                var item = new TimeDeviationCauseCopyItem
                {
                    TimeDeviationCauseId = timeDeviationCause.TimeDeviationCauseId,
                    Type = timeDeviationCause.Type,
                    Name = timeDeviationCause.Name,
                    Description = timeDeviationCause.Description,
                    ExtCode = timeDeviationCause.ExtCode,
                    ImageSource = timeDeviationCause.ImageSource,
                    EmployeeRequestPolicyNbrOfDaysBefore = timeDeviationCause.EmployeeRequestPolicyNbrOfDaysBefore,
                    EmployeeRequestPolicyNbrOfDaysBeforeCanOverride = timeDeviationCause.EmployeeRequestPolicyNbrOfDaysBeforeCanOverride,
                    AttachZeroDaysNbrOfDaysBefore = timeDeviationCause.AttachZeroDaysNbrOfDaysBefore,
                    AttachZeroDaysNbrOfDaysAfter = timeDeviationCause.AttachZeroDaysNbrOfDaysAfter,
                    ChangeDeviationCauseAccordingToPlannedAbsence = timeDeviationCause.ChangeDeviationCauseAccordingToPlannedAbsence,
                    ChangeCauseOutsideOfPlannedAbsence = timeDeviationCause.ChangeCauseOutsideOfPlannedAbsence,
                    ChangeCauseInsideOfPlannedAbsence = timeDeviationCause.ChangeCauseInsideOfPlannedAbsence,
                    AdjustTimeOutsideOfPlannedAbsence = timeDeviationCause.AdjustTimeOutsideOfPlannedAbsence,
                    AdjustTimeInsideOfPlannedAbsence = timeDeviationCause.AdjustTimeInsideOfPlannedAbsence,
                    AllowGapToPlannedAbsence = timeDeviationCause.AllowGapToPlannedAbsence,
                    ShowZeroDaysInAbsencePlanning = timeDeviationCause.ShowZeroDaysInAbsencePlanning,
                    IsVacation = timeDeviationCause.IsVacation,
                    Payed = timeDeviationCause.Payed,
                    NotChargeable = timeDeviationCause.NotChargeable,
                    OnlyWholeDay = timeDeviationCause.OnlyWholeDay,
                    SpecifyChild = timeDeviationCause.SpecifyChild,
                    ExcludeFromPresenceWorkRules = timeDeviationCause.ExcludeFromPresenceWorkRules,
                    ExcludeFromScheduleWorkRules = timeDeviationCause.ExcludeFromScheduleWorkRules,
                    ValidForHibernating = timeDeviationCause.ValidForHibernating,
                    ValidForStandby = timeDeviationCause.ValidForStandby,
                    CandidateForOvertime = timeDeviationCause.CandidateForOvertime,
                    MandatoryNote = timeDeviationCause.MandatoryNote,
                    MandatoryTime = timeDeviationCause.MandatoryTime,
                    State = timeDeviationCause.State,
                    TimeCodeId = timeDeviationCause.TimeCodeId,
                    CalculateAsOtherTimeInSales = timeDeviationCause.CalculateAsOtherTimeInSales,
                };

                timeDeviationCauseCopyItems.Add(item);
            }

            return timeDeviationCauseCopyItems;
        }

        public List<TimeDeviationCauseCopyItem> GetTimeDeviationCauseCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeDeviationCauseCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeDeviationCauseCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region EmployeGroup

        public TemplateResult CopyEmployeeGroupsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<TermGroup_AttestEntity> entitys = new List<TermGroup_AttestEntity>()
                {
                    TermGroup_AttestEntity.InvoiceTime,
                    TermGroup_AttestEntity.PayrollTime,
                };

                List<EmployeeGroup> existingEmployeeGroups = EmployeeManager.GetEmployeeGroups(entities, item.DestinationActorCompanyId, loadTimeDeviationCauseMappings: true, loadTimeDeviationCauses: true, loadDayTypes: true, loadAttestTransitions: true, loadTimeCodeBreaks: true, loadTimeStampRounding: true).ToList();
                List<DayType> existingDayTypes = CalendarManager.GetDayTypesByCompany(entities, item.DestinationActorCompanyId);
                List<TimeCode> existingTimeCodes = TimeCodeManager.GetTimeCodes(entities, item.DestinationActorCompanyId);
                List<TimeCodeBreak> existingTimeCodeBreaks = TimeCodeManager.GetTimeCodeBreaks(entities, item.DestinationActorCompanyId);
                List<AttestTransition> existingAttestTransitions = AttestManager.GetAttestTransitions(entities, entitys, SoeModule.Time, false, item.DestinationActorCompanyId);
                List<TimeDeviationCause> existingTimeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, item.DestinationActorCompanyId, loadTimeCode: true);

                foreach (var templateEmployeeGroup in item.TemplateCompanyTimeDataItem.EmployeeGroupCopyItems)
                {
                    EmployeeGroup newEmployeeGroup = existingEmployeeGroups.FirstOrDefault(eg => eg.Name == templateEmployeeGroup.Name);
                    if (newEmployeeGroup == null)
                    {
                        newEmployeeGroup = new EmployeeGroup()
                        {
                            // References
                            Company = newCompany,
                        };
                        SetCreatedProperties(newEmployeeGroup);
                        entities.EmployeeGroup.AddObject(newEmployeeGroup);
                        newCompany.EmployeeGroup.Add(newEmployeeGroup);
                    }
                    else
                    {
                        SetModifiedProperties(newEmployeeGroup);
                    }

                    newEmployeeGroup.Name = templateEmployeeGroup.Name;
                    newEmployeeGroup.TimeDeviationCauseId = item.TemplateCompanyTimeDataItem.GetTimeDeviationCause(templateEmployeeGroup.DefaultTimeDeviationCauseId ?? 0)?.TimeDeviationCauseId;
                    newEmployeeGroup.TimeCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(templateEmployeeGroup.TimeCodeId ?? 0)?.TimeCodeId;
                    newEmployeeGroup.DeviationAxelStartHours = templateEmployeeGroup.DeviationAxelStartHours;
                    newEmployeeGroup.DeviationAxelStopHours = templateEmployeeGroup.DeviationAxelStopHours;
                    newEmployeeGroup.PayrollProductAccountingPrio = templateEmployeeGroup.PayrollProductAccountingPrio;
                    newEmployeeGroup.InvoiceProductAccountingPrio = templateEmployeeGroup.InvoiceProductAccountingPrio;
                    newEmployeeGroup.AutogenTimeblocks = templateEmployeeGroup.AutogenTimeblocks;
                    newEmployeeGroup.AutogenBreakOnStamping = templateEmployeeGroup.AutogenBreakOnStamping;
                    newEmployeeGroup.BreakDayMinutesAfterMidnight = templateEmployeeGroup.BreakDayMinutesAfterMidnight;
                    newEmployeeGroup.KeepStampsTogetherWithinMinutes = templateEmployeeGroup.KeepStampsTogetherWithinMinutes;
                    newEmployeeGroup.RuleWorkTimeWeek = templateEmployeeGroup.RuleWorkTimeWeek;
                    newEmployeeGroup.RuleRestTimeDay = templateEmployeeGroup.RuleRestTimeDay;
                    newEmployeeGroup.AlwaysDiscardBreakEvaluation = templateEmployeeGroup.AlwaysDiscardBreakEvaluation;
                    newEmployeeGroup.MergeScheduleBreaksOnDay = templateEmployeeGroup.MergeScheduleBreaksOnDay;
                    newEmployeeGroup.ReminderAttestStateId = item.TemplateCompanyAttestDataItem.GetAttestState(templateEmployeeGroup.ReminderAttestStateId ?? 0)?.AttestStateId;
                    newEmployeeGroup.ReminderNoOfDays = templateEmployeeGroup.ReminderNoOfDays;
                    newEmployeeGroup.ReminderPeriodType = templateEmployeeGroup.ReminderPeriodType;
                    newEmployeeGroup.RuleRestTimeWeek = templateEmployeeGroup.RuleRestTimeWeek;
                    newEmployeeGroup.RuleWorkTimeYear2014 = templateEmployeeGroup.RuleWorkTimeYear2014;
                    newEmployeeGroup.RuleWorkTimeYear2015 = templateEmployeeGroup.RuleWorkTimeYear2015;
                    newEmployeeGroup.RuleWorkTimeYear2016 = templateEmployeeGroup.RuleWorkTimeYear2016;
                    newEmployeeGroup.RuleWorkTimeYear2017 = templateEmployeeGroup.RuleWorkTimeYear2017;
                    newEmployeeGroup.RuleWorkTimeYear2018 = templateEmployeeGroup.RuleWorkTimeYear2018;
                    newEmployeeGroup.RuleWorkTimeYear2019 = templateEmployeeGroup.RuleWorkTimeYear2019;
                    newEmployeeGroup.RuleWorkTimeYear2020 = templateEmployeeGroup.RuleWorkTimeYear2020;
                    newEmployeeGroup.RuleWorkTimeYear2021 = templateEmployeeGroup.RuleWorkTimeYear2021;
                    newEmployeeGroup.MaxScheduleTimeFullTime = templateEmployeeGroup.MaxScheduleTimeFullTime;
                    newEmployeeGroup.MaxScheduleTimePartTime = templateEmployeeGroup.MaxScheduleTimePartTime;
                    newEmployeeGroup.MinScheduleTimeFullTime = templateEmployeeGroup.MinScheduleTimeFullTime;
                    newEmployeeGroup.MinScheduleTimePartTime = templateEmployeeGroup.MinScheduleTimePartTime;
                    newEmployeeGroup.RuleWorkTimeDayMinimum = templateEmployeeGroup.RuleWorkTimeDayMinimum;
                    newEmployeeGroup.RuleWorkTimeDayMaximumWorkDay = templateEmployeeGroup.RuleWorkTimeDayMaximumWorkDay;
                    newEmployeeGroup.RuleWorkTimeDayMaximumWeekend = templateEmployeeGroup.RuleWorkTimeDayMaximumWeekend;
                    newEmployeeGroup.MaxScheduleTimeWithoutBreaks = templateEmployeeGroup.MaxScheduleTimeWithoutBreaks;
                    newEmployeeGroup.TimeReportType = templateEmployeeGroup.TimeReportType;
                    newEmployeeGroup.QualifyingDayCalculationRule = templateEmployeeGroup.QualifyingDayCalculationRule;
                    newEmployeeGroup.AutoGenTimeAndBreakForProject = templateEmployeeGroup.AutoGenTimeAndBreakForProject;
                    newEmployeeGroup.BreakRoundingUp = templateEmployeeGroup.BreakRoundingUp;
                    newEmployeeGroup.BreakRoundingDown = templateEmployeeGroup.BreakRoundingDown;
                    newEmployeeGroup.NotifyChangeOfDeviations = templateEmployeeGroup.NotifyChangeOfDeviations;
                    newEmployeeGroup.RuleRestDayIncludePresence = templateEmployeeGroup.RuleRestDayIncludePresence;
                    newEmployeeGroup.RuleRestWeekIncludePresence = templateEmployeeGroup.RuleRestWeekIncludePresence;
                    newEmployeeGroup.AllowShiftsWithoutAccount = templateEmployeeGroup.AllowShiftsWithoutAccount;
                    newEmployeeGroup.AlsoAttestAdditionsFromTime = templateEmployeeGroup.AlsoAttestAdditionsFromTime;
                    newEmployeeGroup.RuleScheduleFreeWeekendsMinimumYear = templateEmployeeGroup.RuleScheduleFreeWeekendsMinimumYear;
                    newEmployeeGroup.RuleScheduledDaysMaximumWeek = templateEmployeeGroup.RuleScheduledDaysMaximumWeek;
                    newEmployeeGroup.CandidateForOvertimeOnZeroDayExcluded = templateEmployeeGroup.CandidateForOvertimeOnZeroDayExcluded;
                    newEmployeeGroup.RuleRestTimeWeekStartDayNumber = templateEmployeeGroup.RuleRestTimeWeekStartDayNumber;
                    newEmployeeGroup.RuleRestTimeDayStartTime = templateEmployeeGroup.RuleRestTimeDayStartTime;
                    newEmployeeGroup.ExtraShiftAsDefault = templateEmployeeGroup.ExtraShiftAsDefault;
                    newEmployeeGroup.QualifyingDayCalculationRuleLimitFirstDay = templateEmployeeGroup.QualifyingDayCalculationRuleLimitFirstDay;
                    newEmployeeGroup.TimeWorkReductionCalculationRule = templateEmployeeGroup.TimeWorkReductionCalculationRule;
                    newEmployeeGroup.SwapShiftToShorterText = templateEmployeeGroup.SwapShiftToShorterText;
                    newEmployeeGroup.SwapShiftToLongerText = templateEmployeeGroup.SwapShiftToLongerText;

                    item.TemplateCompanyTimeDataItem.AddEmployeeGroupMapping(templateEmployeeGroup.EmployeeGroupId, newEmployeeGroup);

                    foreach (var timeCodeCopyItem in templateEmployeeGroup.TimeCodeCopyItems)
                    {
                        var timeCode = item.TemplateCompanyTimeDataItem.GetTimeCode(timeCodeCopyItem.TimeCodeId);
                        if (timeCode != null)
                        {
                            var existingTimeCode = existingTimeCodes.FirstOrDefault(x => x.TimeCodeId == timeCode.TimeCodeId);
                            if (existingTimeCode != null && !newEmployeeGroup.TimeCodes.Any(a => a.TimeCodeId == existingTimeCode.TimeCodeId))
                                newEmployeeGroup.TimeCodes.Add(existingTimeCode);
                        }
                    }

                    foreach (var timeCodeBreakCopyItem in templateEmployeeGroup.TimeCodeBreakCopyItems)
                    {
                        var timeCodeBreak = item.TemplateCompanyTimeDataItem.GetTimeCode(timeCodeBreakCopyItem.TimeCodeId);
                        if (timeCodeBreak != null)
                        {
                            var existingTimeCodeBreak = existingTimeCodeBreaks.FirstOrDefault(x => x.TimeCodeId == timeCodeBreak.TimeCodeId);
                            if (existingTimeCodeBreak != null && !newEmployeeGroup.TimeCodeBreak.Any(a => a.TimeCodeId == existingTimeCodeBreak.TimeCodeId))
                                newEmployeeGroup.TimeCodeBreak.Add(existingTimeCodeBreak);
                        }
                    }

                    foreach (var dayTypeCopyItems in templateEmployeeGroup.DayTypeCopyItems)
                    {
                        var dayType = item.TemplateCompanyTimeDataItem.GetDayType(dayTypeCopyItems.DayTypeId);
                        if (dayType != null)
                        {
                            var existingDayType = existingDayTypes.FirstOrDefault(x => x.DayTypeId == dayType.DayTypeId);
                            if (existingDayType != null && !newEmployeeGroup.DayType.Any(a => a.DayTypeId == existingDayType.DayTypeId))
                                newEmployeeGroup.DayType.Add(existingDayType);
                        }
                    }

                    foreach (var attestationTransitionCopyItem in templateEmployeeGroup.AttestTransitionCopyItems)
                    {
                        var attestationTransition = item.TemplateCompanyAttestDataItem.GetAttestTransition(attestationTransitionCopyItem.AttestTransitionId);
                        if (attestationTransition != null)
                        {
                            var existingAttestTransition = existingAttestTransitions.FirstOrDefault(x => x.AttestTransitionId == attestationTransition.AttestTransitionId);
                            if (existingAttestTransition != null && !newEmployeeGroup.AttestTransition.Any(a => a.AttestTransitionId == existingAttestTransition.AttestTransitionId))
                                newEmployeeGroup.AttestTransition.Add(existingAttestTransition);
                        }
                    }

                    foreach (var timeStampRoundingCopyItem in templateEmployeeGroup.TimeStampRoundingCopyItems)
                    {
                        if (newEmployeeGroup.TimeStampRounding.IsNullOrEmpty())
                            newEmployeeGroup.TimeStampRounding.Add(new TimeStampRounding()
                            {
                                RoundInNeg = timeStampRoundingCopyItem.RoundInNeg,
                                RoundInPos = timeStampRoundingCopyItem.RoundInPos,
                                RoundOutNeg = timeStampRoundingCopyItem.RoundOutNeg,
                                RoundOutPos = timeStampRoundingCopyItem.RoundOutPos
                            });
                    }

                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                        companyTemplateManager.LogCopyError("EmployeeGroup", item, saved: true);
                    templateResult.ActionResults.Add(result);

                    if (result.Success)
                    {
                        foreach (var timeDeviationCauseCopyItem in templateEmployeeGroup.TimeDeviationCauseCopyItems)
                        {
                            var timeDeviationCause = item.TemplateCompanyTimeDataItem.GetTimeDeviationCause(timeDeviationCauseCopyItem.TimeDeviationCauseId);

                            if (timeDeviationCause != null)
                            {
                                var existingTimeDeviationCause = existingTimeDeviationCauses.FirstOrDefault(x => x.TimeDeviationCauseId == timeDeviationCause.TimeDeviationCauseId);
                                if (existingTimeDeviationCause != null && !newEmployeeGroup.EmployeeGroupTimeDeviationCause.Any(a => a.TimeDeviationCauseId == existingTimeDeviationCause.TimeDeviationCauseId))
                                    newEmployeeGroup.EmployeeGroupTimeDeviationCause.Add(new EmployeeGroupTimeDeviationCause() { ActorCompanyId = item.DestinationActorCompanyId, TimeDeviationCauseId = existingTimeDeviationCause.TimeDeviationCauseId, UseInTimeTerminal = timeDeviationCauseCopyItem.UseInTimeTerminal });
                            }
                        }

                        foreach (var timeDeviationCauseTimeCodeCopyItem in templateEmployeeGroup.TimeDeviationCauseTimeCodeCopyItems)
                        {
                            var timeDeviationCause = item.TemplateCompanyTimeDataItem.GetTimeDeviationCause(timeDeviationCauseTimeCodeCopyItem.TimeDeviationCauseId);
                            var timeCode = item.TemplateCompanyTimeDataItem.GetTimeCode(timeDeviationCauseTimeCodeCopyItem.TimeCodeId);

                            if (timeDeviationCause != null && timeCode != null)
                            {
                                var existingTimeDeviationCause = existingTimeDeviationCauses.FirstOrDefault(x => x.TimeDeviationCauseId == timeDeviationCause.TimeDeviationCauseId);
                                var existingTimeCode = existingTimeCodes.FirstOrDefault(x => x.TimeCodeId == timeCode.TimeCodeId);
                                if (existingTimeDeviationCause != null && existingTimeCode != null && !newEmployeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Any(a => a.TimeDeviationCauseId == existingTimeDeviationCause.TimeDeviationCauseId && a.TimeCodeId == existingTimeCode.TimeCodeId))
                                    newEmployeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Add(new EmployeeGroupTimeDeviationCauseTimeCode() { TimeDeviationCauseId = existingTimeDeviationCause.TimeDeviationCauseId, TimeCodeId = existingTimeCode.TimeCodeId });
                            }
                        }

                        foreach (var timeDeviationCauseAbsenceAnnouncementCopyItem in templateEmployeeGroup.TimeDeviationCauseAbsenceAnnouncementCopyItems)
                        {
                            var timeDeviationCause = item.TemplateCompanyTimeDataItem.GetTimeDeviationCause(timeDeviationCauseAbsenceAnnouncementCopyItem.TimeDeviationCauseId);

                            if (timeDeviationCause != null)
                            {
                                var existingTimeDeviationCause = existingTimeDeviationCauses.FirstOrDefault(x => x.TimeDeviationCauseId == timeDeviationCause.TimeDeviationCauseId);
                                if (existingTimeDeviationCause != null && !newEmployeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Any(a => a.TimeDeviationCauseId == existingTimeDeviationCause.TimeDeviationCauseId))
                                    newEmployeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Add(new EmployeeGroupTimeDeviationCauseAbsenceAnnouncement() { TimeDeviationCauseId = existingTimeDeviationCause.TimeDeviationCauseId });
                            }
                        }

                        foreach (var timeDeviationCauseCopyItemsRequest in templateEmployeeGroup.TimeDeviationCauseCopyItemsRequest)
                        {
                            var timeDeviationCause = item.TemplateCompanyTimeDataItem.GetTimeDeviationCause(timeDeviationCauseCopyItemsRequest.TimeDeviationCauseId);

                            if (timeDeviationCause != null)
                            {
                                var existingTimeDeviationCause = existingTimeDeviationCauses.FirstOrDefault(x => x.TimeDeviationCauseId == timeDeviationCause.TimeDeviationCauseId);
                                if (existingTimeDeviationCause != null && !newEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.Any(a => a.TimeDeviationCauseId == existingTimeDeviationCause.TimeDeviationCauseId))
                                    newEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.Add(new EmployeeGroupTimeDeviationCauseRequest() { TimeDeviationCauseId = existingTimeDeviationCause.TimeDeviationCauseId });
                            }
                        }

                        ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.EmployeeGroup, newEmployeeGroup.EmployeeGroupId, templateEmployeeGroup.ExternalCodesString, newEmployeeGroup.ActorCompanyId);

                        ActionResult result2 = SaveChanges(entities);
                        if (!result2.Success)
                            companyTemplateManager.LogCopyError("EmployeeGroup", item, saved: true);
                        templateResult.ActionResults.Add(result2);

                    }
                }
            }

            return templateResult;
        }

        public List<EmployeeGroupCopyItem> GetEmployeeGroupCopyItems(int actorCompanyId)
        {
            List<EmployeeGroupCopyItem> employeeGroupCopyItems = new List<EmployeeGroupCopyItem>();

            var employeeGroups = EmployeeManager.GetEmployeeGroups(actorCompanyId, loadTimeDeviationCauseMappings: true, loadTimeDeviationCauses: true, loadDayTypes: true, loadAttestTransitions: true, loadTimeCodeBreaks: true, loadTimeStampRounding: true, loadTimeCodes: true);

            foreach (var templateEmployeeGroup in employeeGroups)
            {
                EmployeeGroupCopyItem employeeGroupCopyItem = new EmployeeGroupCopyItem
                {
                    EmployeeGroupId = templateEmployeeGroup.EmployeeGroupId,
                    Name = templateEmployeeGroup.Name,
                    DefaultTimeDeviationCauseId = templateEmployeeGroup.TimeDeviationCauseId,
                    TimeCodeId = templateEmployeeGroup.TimeCodeId,
                    DeviationAxelStartHours = templateEmployeeGroup.DeviationAxelStartHours,
                    DeviationAxelStopHours = templateEmployeeGroup.DeviationAxelStopHours,
                    PayrollProductAccountingPrio = templateEmployeeGroup.PayrollProductAccountingPrio,
                    InvoiceProductAccountingPrio = templateEmployeeGroup.InvoiceProductAccountingPrio,
                    AutogenTimeblocks = templateEmployeeGroup.AutogenTimeblocks,
                    AutogenBreakOnStamping = templateEmployeeGroup.AutogenBreakOnStamping,
                    BreakDayMinutesAfterMidnight = templateEmployeeGroup.BreakDayMinutesAfterMidnight,
                    KeepStampsTogetherWithinMinutes = templateEmployeeGroup.KeepStampsTogetherWithinMinutes,
                    RuleWorkTimeWeek = templateEmployeeGroup.RuleWorkTimeWeek,
                    RuleRestTimeDay = templateEmployeeGroup.RuleRestTimeDay,
                    AlwaysDiscardBreakEvaluation = templateEmployeeGroup.AlwaysDiscardBreakEvaluation,
                    MergeScheduleBreaksOnDay = templateEmployeeGroup.MergeScheduleBreaksOnDay,
                    ReminderAttestStateId = templateEmployeeGroup.ReminderAttestStateId,
                    ReminderNoOfDays = templateEmployeeGroup.ReminderNoOfDays,
                    ReminderPeriodType = templateEmployeeGroup.ReminderPeriodType,
                    RuleRestTimeWeek = templateEmployeeGroup.RuleRestTimeWeek,
                    RuleWorkTimeYear2014 = templateEmployeeGroup.RuleWorkTimeYear2014,
                    RuleWorkTimeYear2015 = templateEmployeeGroup.RuleWorkTimeYear2015,
                    RuleWorkTimeYear2016 = templateEmployeeGroup.RuleWorkTimeYear2016,
                    RuleWorkTimeYear2017 = templateEmployeeGroup.RuleWorkTimeYear2017,
                    RuleWorkTimeYear2018 = templateEmployeeGroup.RuleWorkTimeYear2018,
                    RuleWorkTimeYear2019 = templateEmployeeGroup.RuleWorkTimeYear2019,
                    RuleWorkTimeYear2020 = templateEmployeeGroup.RuleWorkTimeYear2020,
                    RuleWorkTimeYear2021 = templateEmployeeGroup.RuleWorkTimeYear2021,
                    MaxScheduleTimeFullTime = templateEmployeeGroup.MaxScheduleTimeFullTime,
                    MaxScheduleTimePartTime = templateEmployeeGroup.MaxScheduleTimePartTime,
                    MinScheduleTimeFullTime = templateEmployeeGroup.MinScheduleTimeFullTime,
                    MinScheduleTimePartTime = templateEmployeeGroup.MinScheduleTimePartTime,
                    RuleWorkTimeDayMinimum = templateEmployeeGroup.RuleWorkTimeDayMinimum,
                    RuleWorkTimeDayMaximumWorkDay = templateEmployeeGroup.RuleWorkTimeDayMaximumWorkDay,
                    RuleWorkTimeDayMaximumWeekend = templateEmployeeGroup.RuleWorkTimeDayMaximumWeekend,
                    MaxScheduleTimeWithoutBreaks = templateEmployeeGroup.MaxScheduleTimeWithoutBreaks,
                    TimeReportType = templateEmployeeGroup.TimeReportType,
                    QualifyingDayCalculationRule = templateEmployeeGroup.QualifyingDayCalculationRule,
                    AutoGenTimeAndBreakForProject = templateEmployeeGroup.AutoGenTimeAndBreakForProject,
                    BreakRoundingUp = templateEmployeeGroup.BreakRoundingUp,
                    BreakRoundingDown = templateEmployeeGroup.BreakRoundingDown,
                    NotifyChangeOfDeviations = templateEmployeeGroup.NotifyChangeOfDeviations,
                    RuleRestDayIncludePresence = templateEmployeeGroup.RuleRestDayIncludePresence,
                    RuleRestWeekIncludePresence = templateEmployeeGroup.RuleRestWeekIncludePresence,
                    AllowShiftsWithoutAccount = templateEmployeeGroup.AllowShiftsWithoutAccount,
                    AlsoAttestAdditionsFromTime = templateEmployeeGroup.AlsoAttestAdditionsFromTime,
                    RuleScheduleFreeWeekendsMinimumYear = templateEmployeeGroup.RuleScheduleFreeWeekendsMinimumYear,
                    RuleScheduledDaysMaximumWeek = templateEmployeeGroup.RuleScheduledDaysMaximumWeek,
                    CandidateForOvertimeOnZeroDayExcluded = templateEmployeeGroup.CandidateForOvertimeOnZeroDayExcluded,
                    RuleRestTimeWeekStartDayNumber = templateEmployeeGroup.RuleRestTimeWeekStartDayNumber,
                    RuleRestTimeDayStartTime = templateEmployeeGroup.RuleRestTimeDayStartTime,
                    ExtraShiftAsDefault = templateEmployeeGroup.ExtraShiftAsDefault,
                    QualifyingDayCalculationRuleLimitFirstDay = templateEmployeeGroup.QualifyingDayCalculationRuleLimitFirstDay,
                    TimeWorkReductionCalculationRule = templateEmployeeGroup.TimeWorkReductionCalculationRule,
                    ExternalCodesString = templateEmployeeGroup.ExternalCodesString,
                    SwapShiftToShorterText = templateEmployeeGroup.SwapShiftToShorterText,
                    SwapShiftToLongerText = templateEmployeeGroup.SwapShiftToLongerText,
                };

                foreach (var templateTimeCodeBreak in templateEmployeeGroup.TimeCodeBreak.Where(w => w.State == (int)SoeEntityState.Active))
                    employeeGroupCopyItem.TimeCodeBreakCopyItems.Add(new EmployeeGroupTimeCodeBreakCopyItem { TimeCodeId = templateTimeCodeBreak.TimeCodeId });

                foreach (var templateTimeCode in templateEmployeeGroup.TimeCodes.Where(w => w.State == (int)SoeEntityState.Active))
                    employeeGroupCopyItem.TimeCodeCopyItems.Add(new EmployeeGroupTimeCodeCopyItem { TimeCodeId = templateTimeCode.TimeCodeId });

                foreach (var dayType in templateEmployeeGroup.DayType.Where(w => w.State == (int)SoeEntityState.Active))
                    employeeGroupCopyItem.DayTypeCopyItems.Add(new DayTypeCopyItem { DayTypeId = dayType.DayTypeId });

                foreach (var templateAttestTransition in templateEmployeeGroup.AttestTransition.Where(w => w.State == (int)SoeEntityState.Active))
                    employeeGroupCopyItem.AttestTransitionCopyItems.Add(new EmployeeGroupAttestTransitionCopyItem { AttestTransitionId = templateAttestTransition.AttestTransitionId });

                foreach (var templateTimeStampRounding in templateEmployeeGroup.TimeStampRounding)
                {
                    employeeGroupCopyItem.TimeStampRoundingCopyItems.Add(new EmployeeGroupTimeStampRoundingCopyItem
                    {
                        RoundInNeg = templateTimeStampRounding.RoundInNeg,
                        RoundInPos = templateTimeStampRounding.RoundInPos,
                        RoundOutNeg = templateTimeStampRounding.RoundOutNeg,
                        RoundOutPos = templateTimeStampRounding.RoundOutPos
                    });
                }

                if (!templateEmployeeGroup.TimeDeviationCauseReference.IsLoaded)
                    templateEmployeeGroup.TimeDeviationCauseReference.Load();

                foreach (var item in templateEmployeeGroup.EmployeeGroupTimeDeviationCause.Where(w => w.State == (int)SoeEntityState.Active))
                    employeeGroupCopyItem.TimeDeviationCauseCopyItems.Add(new EmployeeGroupTimeDeviationCauseCopyItem { TimeDeviationCauseId = item.TimeDeviationCauseId, UseInTimeTerminal = item.UseInTimeTerminal });

                foreach (var item in templateEmployeeGroup.EmployeeGroupTimeDeviationCauseTimeCode)
                    employeeGroupCopyItem.TimeDeviationCauseTimeCodeCopyItems.Add(new EmployeeGroupTimeDeviationCauseTimeCodeCopyItem { TimeDeviationCauseId = item.TimeDeviationCauseId, TimeCodeId = item.TimeCodeId });

                foreach (var item in templateEmployeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement)
                    employeeGroupCopyItem.TimeDeviationCauseAbsenceAnnouncementCopyItems.Add(new EmployeeGroupTimeDeviationCauseAbsenceAnnouncementCopyItem { TimeDeviationCauseId = item.TimeDeviationCauseId });

                if (!templateEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.IsLoaded)
                    templateEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.Load();

                foreach (var item in templateEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest)
                    employeeGroupCopyItem.TimeDeviationCauseCopyItemsRequest.Add(new EmployeeGroupTimeDeviationCauseRequestCopyItem { TimeDeviationCauseId = item.TimeDeviationCauseId });

                employeeGroupCopyItems.Add(employeeGroupCopyItem);
            }

            return employeeGroupCopyItems;
        }

        public List<EmployeeGroupCopyItem> GetEmployeeGroupCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetEmployeeGroupCopyItems(actorCompanyId);

            return timeTemplateConnector.GetEmployeeGroupCopyItems(sysCompDbId, actorCompanyId);
        }


        #endregion

        #region TimeDeviationCauses

        public TemplateResult CopyEmploymentTypesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<EmploymentType> existingEmploymentTypes = entities.EmploymentType
                    .Where(e => e.ActorCompanyId == newCompany.ActorCompanyId && e.State != (int)SoeEntityState.Deleted).ToList();

                foreach (var employmentTypeCopyItem in item.TemplateCompanyTimeDataItem.EmploymentTypeCopyItems)
                {
                    EmploymentType newEmploymentType = existingEmploymentTypes.FirstOrDefault(t => t.Name == employmentTypeCopyItem.Name);
                    if (newEmploymentType == null)
                    {
                        newEmploymentType = new EmploymentType()
                        {
                            // References
                            Company = newCompany,
                        };
                        SetCreatedProperties(newEmploymentType);
                        entities.EmploymentType.AddObject(newEmploymentType);
                        newCompany.EmploymentType.Add(newEmploymentType);
                    }
                    else
                    {
                        SetModifiedProperties(newEmploymentType);
                    }

                    newEmploymentType.Description = employmentTypeCopyItem.Description;
                    newEmploymentType.Type = employmentTypeCopyItem.Type;
                    newEmploymentType.Name = employmentTypeCopyItem.Name;
                    newEmploymentType.Code = employmentTypeCopyItem.Code;

                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                        companyTemplateManager.LogCopyError("EmploymentType", item, saved: true);
                    templateResult.ActionResults.Add(result);
                }
            }

            return templateResult;
        }

        public List<EmploymentTypeCopyItem> GetEmploymentTypeCopyItems(int actorCompanyId)
        {
            List<EmploymentTypeCopyItem> employmentTypeCopyItems = new List<EmploymentTypeCopyItem>();

            // Retrieve existing employment types based on actorCompanyId
            var existingEmploymentTypes = EmployeeManager.GetEmploymentTypes(actorCompanyId);

            foreach (var existingEmploymentType in existingEmploymentTypes)
            {
                EmploymentTypeCopyItem item = new EmploymentTypeCopyItem
                {
                    EmploymentTypeId = existingEmploymentType.EmploymentTypeId,
                    Name = existingEmploymentType.Name,
                    Description = existingEmploymentType.Description,
                    Type = existingEmploymentType.Type,
                    Code = existingEmploymentType.Code,
                };

                employmentTypeCopyItems.Add(item);
            }

            return employmentTypeCopyItems;
        }

        public List<EmploymentTypeCopyItem> GetEmploymentTypeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetEmploymentTypeCopyItems(actorCompanyId);

            return timeTemplateConnector.GetEmploymentTypeCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region TimeAccumulator

        public TemplateResult CopyTimeAccumulatorsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<TimeAccumulator> existingTimeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(entities, item.DestinationActorCompanyId, loadEmployeeGroupRule: true, loadTimeCode: true, loadPayrollProduct: true, loadTimeWorkReductionEarning: true);
                List<PayrollProduct> existingPayrollProducts = ProductManager.GetPayrollProducts(entities, item.DestinationActorCompanyId, active: null);

                foreach (var timeAccumulatorCopyItem in item.TemplateCompanyTimeDataItem.TimeAccumulatorCopyItems)
                {
                    if (existingTimeAccumulators.Any(i => i.Name.Trim().ToLower().Equals(timeAccumulatorCopyItem.Name.Trim().ToLower())))
                        continue;

                    TimeAccumulator newTimeAccumulator = new TimeAccumulator
                    {
                        Name = timeAccumulatorCopyItem.Name,
                        Description = timeAccumulatorCopyItem.Description,
                        ShowInTimeReports = timeAccumulatorCopyItem.ShowInTimeReports,
                        Type = timeAccumulatorCopyItem.Type,
                        ActorCompanyId = newCompany.ActorCompanyId,
                        FinalSalary = timeAccumulatorCopyItem.FinalSalary,
                        UseTimeWorkAccount = timeAccumulatorCopyItem.UseTimeWorkAccount,
                        UseTimeWorkReductionWithdrawal = timeAccumulatorCopyItem.UseTimeWorkReductionWithdrawal,
                    };
                    if (timeAccumulatorCopyItem.TimeCodeId.HasValue)
                        newTimeAccumulator.TimeCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(timeAccumulatorCopyItem.TimeCodeId.Value)?.TimeCodeId ?? null;

                    SetCreatedProperties(newTimeAccumulator, user);
                    entities.TimeAccumulator.AddObject(newTimeAccumulator);

                    foreach (var ruleCopyItem in timeAccumulatorCopyItem.EmployeeGroupRules)
                    {
                        var minTimeTimeCode = item.TemplateCompanyTimeDataItem.GetTimeCode(ruleCopyItem.MinTimeCodeId ?? 0);
                        var maxTimeTimeCode = item.TemplateCompanyTimeDataItem.GetTimeCode(ruleCopyItem.MaxTimeCodeId ?? 0);
                        var employeeGroup = item.TemplateCompanyTimeDataItem.GetEmployeeGroup(ruleCopyItem.EmployeeGroupId);
                        if (employeeGroup != null)
                        {
                            TimeAccumulatorEmployeeGroupRule newRule = new TimeAccumulatorEmployeeGroupRule
                            {
                                EmployeeGroupId = ruleCopyItem.EmployeeGroupId,
                                Type = ruleCopyItem.Type,
                                MinMinutes = ruleCopyItem.MinMinutes,
                                MaxMinutes = ruleCopyItem.MaxMinutes,
                                MaxMinutesWarning = ruleCopyItem.MaxMinutesWarning,
                                MinMinutesWarning = ruleCopyItem.MinMinutesWarning,
                                MinTimeCodeId = minTimeTimeCode?.TimeCodeId,
                                MaxTimeCodeId = maxTimeTimeCode?.TimeCodeId,
                                ShowOnPayrollSlip = ruleCopyItem.ShowOnPayrollSlip,
                                EmployeeGroup = entities.EmployeeGroup.First(f => f.EmployeeGroupId == employeeGroup.EmployeeGroupId),
                                ThresholdMinutes = ruleCopyItem.ThresholdMinutes
                            };

                            newTimeAccumulator.TimeAccumulatorEmployeeGroupRule.Add(newRule);
                        }
                    }

                    foreach (var codeCopyItem in timeAccumulatorCopyItem.TimeCodes)
                    {
                        var existingTimeCode = item.TemplateCompanyTimeDataItem.GetTimeCode(codeCopyItem.TimeCodeId);

                        if (existingTimeCode != null)
                        {
                            TimeAccumulatorTimeCode newCode = new TimeAccumulatorTimeCode
                            {
                                TimeCodeId = existingTimeCode.TimeCodeId,
                                Factor = codeCopyItem.Factor,
                                IsHeadTimeCode = codeCopyItem.IsHeadTimeCode,
                                ImportDefault = codeCopyItem.ImportDefault
                            };

                            newTimeAccumulator.TimeAccumulatorTimeCode.Add(newCode);
                        }
                    }

                    foreach (var payrollCopyItem in timeAccumulatorCopyItem.PayrollProducts)
                    {
                        var existingPayrollProduct = GetExistingPayrollProduct(existingPayrollProducts, item.TemplateCompanyTimeDataItem.PayrollProductCopyItems, payrollCopyItem.PayrollProductId);

                        if (existingPayrollProduct != null)
                        {
                            TimeAccumulatorPayrollProduct newPayrollProduct = new TimeAccumulatorPayrollProduct
                            {
                                PayrollProductId = existingPayrollProduct.ProductId,
                                Factor = payrollCopyItem.Factor,
                            };

                            newTimeAccumulator.TimeAccumulatorPayrollProduct.Add(newPayrollProduct);
                        }
                    }

                    foreach (var invoiceCopyItem in timeAccumulatorCopyItem.InvoiceProducts)
                    {
                        var existingInvoiceProduct = item.TemplateCompanyBillingDataItem.GetInvoiceProduct(invoiceCopyItem.InvoiceProductId);

                        TimeAccumulatorInvoiceProduct newInvoiceProduct = new TimeAccumulatorInvoiceProduct
                        {
                            InvoiceProductId = existingInvoiceProduct.ProductId,
                            Factor = invoiceCopyItem.Factor
                        };

                        newTimeAccumulator.TimeAccumulatorInvoiceProduct.Add(newInvoiceProduct);
                    }

                    if (timeAccumulatorCopyItem.TimeWorkReductionEarning != null)
                    {
                        var newTimeWorkReductionEarning = TimeWorkReductionEarning.Create(
                            timeAccumulatorCopyItem.TimeWorkReductionEarning.MinutesWeight,
                            timeAccumulatorCopyItem.TimeWorkReductionEarning.PeriodType,
                            newTimeAccumulator
                        );
                        SetCreatedProperties(newTimeWorkReductionEarning, user);

                        foreach (var templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup in timeAccumulatorCopyItem.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.Where(w => w.TimeWorkReductionEarningId == timeAccumulatorCopyItem.TimeWorkReductionEarning.TimeWorkReductionEarningId))
                        {
                            var employeeGroup = item.TemplateCompanyTimeDataItem.GetEmployeeGroup(templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.EmployeeGroupId);
                            if (employeeGroup == null)
                                continue;

                            var newTimeAccumulatorTimeWorkReductionEarningEmployeeGroup = new TimeAccumulatorTimeWorkReductionEarningEmployeeGroup()
                            {
                                EmployeeGroupId = employeeGroup.EmployeeGroupId,
                                DateFrom = templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.DateFrom,
                                DateTo = templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.DateTo,

                                //Set FK
                                TimeWorkReductionEarningId = templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.TimeWorkReductionEarningId,
                            };
                            entities.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.AddObject(newTimeAccumulatorTimeWorkReductionEarningEmployeeGroup);
                            newTimeAccumulator.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.Add(newTimeAccumulatorTimeWorkReductionEarningEmployeeGroup);
                        }
                    }

                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                        companyTemplateManager.LogCopyError("TimeAccumulator", item, saved: true);
                    templateResult.ActionResults.Add(result);
                }
            }

            return templateResult;
        }

        public List<TimeAccumulatorCopyItem> GetTimeAccumulatorCopyItems(int actorCompanyId)
        {
            List<TimeAccumulatorCopyItem> timeAccumulatorCopyItems = new List<TimeAccumulatorCopyItem>();

            using (CompEntities entities = new CompEntities())
            {
                var timeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(entities, actorCompanyId, loadEmployeeGroupRule: true, loadTimeCode: true, loadPayrollProduct: true, loadTimeWorkReductionEarning: true);

                foreach (var existingTimeAccumulator in timeAccumulators)
                {
                    TimeAccumulatorCopyItem timeAccumulatorCopyItem = new TimeAccumulatorCopyItem
                    {
                        TimeAccumulatorId = existingTimeAccumulator.TimeAccumulatorId,
                        Name = existingTimeAccumulator.Name,
                        Description = existingTimeAccumulator.Description,
                        ShowInTimeReports = existingTimeAccumulator.ShowInTimeReports,
                        Type = existingTimeAccumulator.Type,
                        FinalSalary = existingTimeAccumulator.FinalSalary,
                        UseTimeWorkAccount = existingTimeAccumulator.UseTimeWorkAccount,
                        UseTimeWorkReductionWithdrawal = existingTimeAccumulator.UseTimeWorkReductionWithdrawal,
                        TimeCodeId = existingTimeAccumulator.TimeCodeId,
                        EmployeeGroupRules = new List<TimeAccumulatorEmployeeGroupRuleCopyItem>(),
                        TimeCodes = new List<TimeAccumulatorTimeCodeCopyItem>(),
                        PayrollProducts = new List<TimeAccumulatorPayrollProductCopyItem>(),
                        InvoiceProducts = new List<TimeAccumulatorInvoiceProductCopyItem>(),
                        TimeWorkReductionEarning = null,
                    };

                    foreach (var existingEmployeeGroupRule in existingTimeAccumulator.TimeAccumulatorEmployeeGroupRule)
                    {
                        TimeAccumulatorEmployeeGroupRuleCopyItem ruleCopyItem = new TimeAccumulatorEmployeeGroupRuleCopyItem
                        {
                            EmployeeGroupId = existingEmployeeGroupRule.EmployeeGroupId,
                            Type = existingEmployeeGroupRule.Type,
                            MinMinutes = existingEmployeeGroupRule.MinMinutes,
                            MaxMinutes = existingEmployeeGroupRule.MaxMinutes,
                            MaxMinutesWarning = existingEmployeeGroupRule.MaxMinutesWarning,
                            MinMinutesWarning = existingEmployeeGroupRule.MinMinutesWarning,
                            MinTimeCodeId = existingEmployeeGroupRule.MinTimeCodeId,
                            MaxTimeCodeId = existingEmployeeGroupRule.MaxTimeCodeId,
                            ShowOnPayrollSlip = existingEmployeeGroupRule.ShowOnPayrollSlip,
                            ThresholdMinutes = existingEmployeeGroupRule.ThresholdMinutes
                        };

                        timeAccumulatorCopyItem.EmployeeGroupRules.Add(ruleCopyItem);
                    }

                    foreach (var existingTimeCode in existingTimeAccumulator.TimeAccumulatorTimeCode)
                    {
                        TimeAccumulatorTimeCodeCopyItem codeCopyItem = new TimeAccumulatorTimeCodeCopyItem
                        {
                            TimeCodeId = existingTimeCode.TimeCodeId,
                            Factor = existingTimeCode.Factor,
                            IsHeadTimeCode = existingTimeCode.IsHeadTimeCode,
                            ImportDefault = existingTimeCode.ImportDefault
                        };

                        timeAccumulatorCopyItem.TimeCodes.Add(codeCopyItem);
                    }

                    foreach (var existingPayrollProduct in existingTimeAccumulator.TimeAccumulatorPayrollProduct)
                    {
                        TimeAccumulatorPayrollProductCopyItem payrollCopyItem = new TimeAccumulatorPayrollProductCopyItem
                        {
                            PayrollProductId = existingPayrollProduct.PayrollProductId,
                            Factor = existingPayrollProduct.Factor
                        };

                        timeAccumulatorCopyItem.PayrollProducts.Add(payrollCopyItem);
                    }

                    foreach (var existingInvoiceProduct in existingTimeAccumulator.TimeAccumulatorInvoiceProduct)
                    {
                        TimeAccumulatorInvoiceProductCopyItem invoiceCopyItem = new TimeAccumulatorInvoiceProductCopyItem
                        {
                            InvoiceProductId = existingInvoiceProduct.InvoiceProductId,
                            Factor = existingInvoiceProduct.Factor
                        };

                        timeAccumulatorCopyItem.InvoiceProducts.Add(invoiceCopyItem);
                    }
                    if (existingTimeAccumulator.TimeWorkReductionEarning != null)
                    {

                        TimeWorkReductionEarningCopyItem newTimeWorkReductionEarningCopyItem = new TimeWorkReductionEarningCopyItem
                        {
                            TimeWorkReductionEarningId = existingTimeAccumulator.TimeWorkReductionEarning.TimeWorkReductionEarningId,
                            MinutesWeight = existingTimeAccumulator.TimeWorkReductionEarning.MinutesWeight,
                            PeriodType = existingTimeAccumulator.TimeWorkReductionEarning.PeriodType,
                            TimeAccumulatorTimeWorkReductionEarningEmployeeGroup = new List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroupCopyItem>()
                        };

                        timeAccumulatorCopyItem.TimeWorkReductionEarning = newTimeWorkReductionEarningCopyItem;

                        if (existingTimeAccumulator.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup != null)
                        {

                            foreach (var existingTimeAccumulatorTimeWorkReductionEarningEmployeeGroup in existingTimeAccumulator.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup)
                            {
                                TimeAccumulatorTimeWorkReductionEarningEmployeeGroupCopyItem timeAccumulatorTimeWorkReductionEarningEmployeeGroupCopyItem = new TimeAccumulatorTimeWorkReductionEarningEmployeeGroupCopyItem
                                {
                                    TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId = existingTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId,
                                    EmployeeGroupId = existingTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.EmployeeGroupId,
                                    TimeWorkReductionEarningId = existingTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.TimeWorkReductionEarningId,
                                    DateFrom = existingTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.DateFrom,
                                    DateTo = existingTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.DateTo,
                                };
                                timeAccumulatorCopyItem.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.Add(timeAccumulatorTimeWorkReductionEarningEmployeeGroupCopyItem);
                            }
                        }
                    }
                    timeAccumulatorCopyItems.Add(timeAccumulatorCopyItem);
                }
            }

            return timeAccumulatorCopyItems;
        }

        public List<TimeAccumulatorCopyItem> GetTimeAccumulatorCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeAccumulatorCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeAccumulatorCopyItems(sysCompDbId, actorCompanyId);
        }


        #endregion

        #region TimeRule

        public List<TimeRuleCopyItem> GetTimeRuleCopyItems(int actorCompanyId)
        {
            List<TimeRuleCopyItem> timeRuleCopyItems = new List<TimeRuleCopyItem>();

            List<TimeRule> timeRules = TimeRuleManager.GetAllTimeRulesRecursive(actorCompanyId);
            foreach (TimeRule timeRule in timeRules)
            {
                TimeRuleCopyItem timeRuleCopyItem = new TimeRuleCopyItem
                {
                    TimeRuleId = timeRule.TimeRuleId,
                    Type = timeRule.Type,
                    Name = timeRule.Name,
                    Description = timeRule.Description,
                    StartDate = timeRule.StartDate,
                    StopDate = timeRule.StopDate,
                    RuleStartDirection = timeRule.RuleStartDirection,
                    RuleStopDirection = timeRule.RuleStopDirection,
                    Factor = timeRule.Factor,
                    BelongsToGroup = timeRule.BelongsToGroup,
                    IsInconvenientWorkHours = timeRule.IsInconvenientWorkHours,
                    TimeCodeMaxLength = timeRule.TimeCodeMaxLength,
                    TimeCodeMaxPerDay = timeRule.TimeCodeMaxPerDay,
                    Sort = timeRule.Sort,
                    Internal = timeRule.Internal,
                    StandardMinutes = timeRule.StandardMinutes,
                    BreakIfAnyFailed = timeRule.BreakIfAnyFailed,
                    AdjustStartToTimeBlockStart = timeRule.AdjustStartToTimeBlockStart,
                    TimeCodeId = timeRule.TimeCodeId,

                    TimeRuleRows = new List<TimeRuleRowCopyItem>()
                };
                foreach (var timeRuleRow in timeRule.TimeRuleRow)
                {
                    TimeRuleRowCopyItem rowCopyItem = new TimeRuleRowCopyItem
                    {
                        TimeDeviationCauseId = timeRuleRow.TimeDeviationCauseId,
                        EmployeeGroupId = timeRuleRow.EmployeeGroupId,
                        TimeScheduleTypeId = timeRuleRow.TimeScheduleTypeId,
                        DayTypeId = timeRuleRow.DayTypeId
                    };
                    timeRuleCopyItem.TimeRuleRows.Add(rowCopyItem);
                }

                // Map TimeRuleExpressions
                timeRuleCopyItem.TimeRuleExpressions = new List<TimeRuleExpressionCopyItem>();
                foreach (var timeRuleExpression in timeRule.TimeRuleExpression)
                {
                    TimeRuleExpressionCopyItem expressionCopyItem = new TimeRuleExpressionCopyItem
                    {
                        IsStart = timeRuleExpression.IsStart,
                        Operands = new List<TimeRuleOperandCopyItem>()
                    };

                    foreach (var timeRuleOperand in timeRuleExpression.TimeRuleOperand)
                    {
                        TimeRuleOperandCopyItem operandCopyItem = new TimeRuleOperandCopyItem
                        {
                            OperatorType = timeRuleOperand.OperatorType,
                            LeftValueType = timeRuleOperand.LeftValueType,
                            RightValueType = timeRuleOperand.RightValueType,
                            Minutes = timeRuleOperand.Minutes,
                            ComparisonOperator = timeRuleOperand.ComparisonOperator,
                            OrderNbr = timeRuleOperand.OrderNbr,
                            LeftValueId = timeRuleOperand.LeftValueId,
                            RightValueId = timeRuleOperand.RightValueId
                        };
                        expressionCopyItem.Operands.Add(operandCopyItem);
                    }

                    timeRuleCopyItem.TimeRuleExpressions.Add(expressionCopyItem);
                }

                timeRuleCopyItems.Add(timeRuleCopyItem);
            }

            return timeRuleCopyItems;
        }


        public List<TimeRuleCopyItem> GetTimeRuleCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeRuleCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeRuleCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyTimeRulesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<TimeRule> existingTimeRules = TimeRuleManager.GetAllTimeRules(item.DestinationActorCompanyId);

                foreach (TimeRuleCopyItem timeRuleCopyItem in item.TemplateCompanyTimeDataItem.TimeRuleCopyItems)
                {
                    if (existingTimeRules.Any(a => a.Name == timeRuleCopyItem.Name))
                        continue;

                    TimeCode timeCode = item.TemplateCompanyTimeDataItem.GetTimeCode(timeRuleCopyItem.TimeCodeId);
                    if (timeCode == null)
                        continue;

                    try
                    {
                        TimeRule newTimeRule = new TimeRule
                        {
                            Type = timeRuleCopyItem.Type,
                            Name = timeRuleCopyItem.Name,
                            Description = timeRuleCopyItem.Description,
                            StartDate = timeRuleCopyItem.StartDate,
                            StopDate = timeRuleCopyItem.StopDate,
                            RuleStartDirection = timeRuleCopyItem.RuleStartDirection,
                            RuleStopDirection = timeRuleCopyItem.RuleStopDirection,
                            Factor = timeRuleCopyItem.Factor,
                            BelongsToGroup = timeRuleCopyItem.BelongsToGroup,
                            IsInconvenientWorkHours = timeRuleCopyItem.IsInconvenientWorkHours,
                            TimeCodeMaxLength = timeRuleCopyItem.TimeCodeMaxLength,
                            TimeCodeMaxPerDay = timeRuleCopyItem.TimeCodeMaxPerDay,
                            Sort = timeRuleCopyItem.Sort,
                            Internal = timeRuleCopyItem.Internal,
                            StandardMinutes = timeRuleCopyItem.StandardMinutes,
                            BreakIfAnyFailed = timeRuleCopyItem.BreakIfAnyFailed,
                            AdjustStartToTimeBlockStart = timeRuleCopyItem.AdjustStartToTimeBlockStart,
                            Company = newCompany,
                            TimeCodeId = timeCode.TimeCodeId
                        };
                        SetCreatedProperties(newTimeRule);

                        entities.TimeRule.AddObject(newTimeRule);

                        foreach (var timeRuleRowCopyItem in timeRuleCopyItem.TimeRuleRows)
                        {
                            var timeDeviationCause = item.TemplateCompanyTimeDataItem.GetTimeDeviationCause(timeRuleRowCopyItem.TimeDeviationCauseId);

                            if (timeDeviationCause == null) // Rule connected to deleted TimeDeviationCause in source company
                                continue;

                            TimeRuleRow newRow = new TimeRuleRow
                            {
                                TimeDeviationCauseId = timeDeviationCause.TimeDeviationCauseId,
                                EmployeeGroupId = item.TemplateCompanyTimeDataItem.GetEmployeeGroup(timeRuleRowCopyItem.EmployeeGroupId ?? 0)?.EmployeeGroupId,
                                TimeScheduleTypeId = item.TemplateCompanyTimeDataItem.GetTimeScheduleType(timeRuleRowCopyItem.TimeScheduleTypeId ?? 0)?.TimeScheduleTypeId,
                                DayTypeId = item.TemplateCompanyTimeDataItem.GetDayType(timeRuleRowCopyItem.DayTypeId ?? 0)?.DayTypeId,
                                Company = newCompany,
                            };
                            entities.TimeRuleRow.AddObject(newRow);
                            newTimeRule.TimeRuleRow.Add(newRow);
                        }

                        foreach (var timeRuleExpressionCopyItem in timeRuleCopyItem.TimeRuleExpressions)
                        {
                            TimeRuleExpression newExpression = new TimeRuleExpression
                            {
                                IsStart = timeRuleExpressionCopyItem.IsStart,
                            };
                            entities.TimeRuleExpression.AddObject(newExpression);
                            newTimeRule.TimeRuleExpression.Add(newExpression);

                            foreach (var operandCopyItem in timeRuleExpressionCopyItem.Operands)
                            {
                                var leftValueType = (SoeTimeRuleValueType)(operandCopyItem.LeftValueType ?? 0);
                                var rightValueType = (SoeTimeRuleValueType)(operandCopyItem.RightValueType ?? 0);

                                TimeRuleOperand newOperand = new TimeRuleOperand
                                {
                                    OperatorType = operandCopyItem.OperatorType,
                                    LeftValueType = operandCopyItem.LeftValueType,
                                    RightValueType = operandCopyItem.RightValueType,
                                    Minutes = operandCopyItem.Minutes,
                                    ComparisonOperator = operandCopyItem.ComparisonOperator,
                                    OrderNbr = operandCopyItem.OrderNbr,
                                    LeftValueId = leftValueType == SoeTimeRuleValueType.TimeCodeLeft ? item.TemplateCompanyTimeDataItem.GetTimeCode(operandCopyItem.LeftValueId ?? 0)?.TimeCodeId : operandCopyItem.LeftValueId,
                                    RightValueId = rightValueType == SoeTimeRuleValueType.TimeCodeRight ? item.TemplateCompanyTimeDataItem.GetTimeCode(operandCopyItem.RightValueId ?? 0)?.TimeCodeId : operandCopyItem.RightValueId
                                };
                                entities.TimeRuleOperand.AddObject(newOperand);
                                newExpression.TimeRuleOperand.Add(newOperand);
                            }
                        }

                        ActionResult result = SaveChanges(entities);
                        if (!result.Success)
                            companyTemplateManager.LogCopyError("TimeRule", item, saved: true);
                        templateResult.ActionResults.Add(result);
                    }
                    catch (Exception ex)
                    {
                        templateResult.ActionResults.Add(new ActionResult(ex));
                    }
                }
            }

            return templateResult;
        }
        #endregion

        #region TimeAbsenceRule

        public List<TimeAbsenceRuleCopyItem> GetTimeAbsenceRuleCopyItems(int actorCompanyId)
        {
            List<TimeAbsenceRuleCopyItem> timeAbsenceRuleCopyItems = new List<TimeAbsenceRuleCopyItem>();

            var timeAbsenceRules = TimeRuleManager.GetTimeAbsenceRules(new GetTimeAbsenceRulesInput(actorCompanyId) { LoadRows = true, LoadRowProducts = true, LoadEmployeeGroups = true });

            foreach (var timeAbsenceRule in timeAbsenceRules.Where(w => w.State == (int)SoeEntityState.Active))
            {
                TimeAbsenceRuleCopyItem timeAbsenceRuleCopyItem = new TimeAbsenceRuleCopyItem
                {
                    TimeAbsenceRuleId = timeAbsenceRule.TimeAbsenceRuleHeadId,
                    Type = timeAbsenceRule.Type,
                    Name = timeAbsenceRule.Name,
                    Description = timeAbsenceRule.Description,
                    TimeCodeId = timeAbsenceRule.TimeCodeId,

                    TimeAbsenceRuleHeadEmployeeGroupCopyItems = timeAbsenceRule.TimeAbsenceRuleHeadEmployeeGroup.Select(group => new TimeAbsenceRuleHeadEmployeeGroupCopyItem
                    {
                        EmployeeGroupId = group.EmployeeGroupId
                    }).ToList(),

                    TimeAbsenceRuleRows = timeAbsenceRule.TimeAbsenceRuleRow.Where(w => w.State == (int)SoeEntityState.Active).Select(row => new TimeAbsenceRuleRowCopyItem
                    {
                        HasMultiplePayrollProducts = row.HasMultiplePayrollProducts,
                        Type = row.Type,
                        Scope = row.Scope,
                        Start = row.Start,
                        Stop = row.Stop,
                        PayrollProductId = row.PayrollProductId,

                        TimeAbsenceRuleRowPayrollProducts = row.TimeAbsenceRuleRowPayrollProducts.Select(product => new TimeAbsenceRuleRowPayrollProductsCopyItem
                        {
                            SourcePayrollProductId = product.SourcePayrollProductId,
                            TargetPayrollProductId = product.TargetPayrollProductId
                        }).ToList()
                    }).ToList()
                };

                timeAbsenceRuleCopyItems.Add(timeAbsenceRuleCopyItem);
            }

            return timeAbsenceRuleCopyItems;
        }


        public List<TimeAbsenceRuleCopyItem> GetTimeAbsenceRuleCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeAbsenceRuleCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeAbsenceRuleCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyTimeAbsenceRulesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                var newInput = new GetTimeAbsenceRulesInput(item.DestinationActorCompanyId)
                {
                    LoadTimeCode = true,
                    LoadCompany = true,
                    LoadEmployeeGroups = true,
                    LoadRows = true,
                    LoadRowProducts = true,
                };

                List<TimeAbsenceRuleHead> existingTimeAbsenceRuleHeads = TimeRuleManager.GetTimeAbsenceRules(newInput);
                List<TimeCode> existingTimeCodes = TimeCodeManager.GetTimeCodes(item.DestinationActorCompanyId);
                List<EmployeeGroup> existingEmployeeGroups = EmployeeManager.GetEmployeeGroups(item.DestinationActorCompanyId);

                foreach (var timeAbsenceRuleCopyItems in item.TemplateCompanyTimeDataItem.TimeAbsenceRuleCopyItems)
                {
                    if (existingTimeAbsenceRuleHeads.Any(r => r.Type == timeAbsenceRuleCopyItems.Type && r.Name.Trim().ToLower().Equals(timeAbsenceRuleCopyItems.Name.Trim().ToLower())))
                        continue;

                    var templateTimeCode = item.TemplateCompanyTimeDataItem.TimeCodeCopyItems.FirstOrDefault(p => p.TimeCodeId == timeAbsenceRuleCopyItems.TimeCodeId);
                    if (templateTimeCode == null)
                    {
                        string error = $"TimeCode with id {timeAbsenceRuleCopyItems.TimeCodeId} not found in absence rule {timeAbsenceRuleCopyItems.Name}";
                        LogError("CopyTimeAbsenseRulesFromTemplateCompany " + error);
                        continue;
                    }

                    TimeAbsenceRuleHead newTimeAbsenceRuleHead = new TimeAbsenceRuleHead()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        Type = timeAbsenceRuleCopyItems.Type,
                        Name = timeAbsenceRuleCopyItems.Name,
                        Description = timeAbsenceRuleCopyItems.Description,
                    };
                    SetCreatedProperties(newTimeAbsenceRuleHead);
                    entities.TimeAbsenceRuleHead.AddObject(newTimeAbsenceRuleHead);

                    #region TimeCode

                    TimeCode newTimeCode = item.TemplateCompanyTimeDataItem.GetTimeCode(timeAbsenceRuleCopyItems.TimeCodeId ?? 0) ?? existingTimeCodes.FirstOrDefault(p => p.Code.Trim().ToLower().Equals(templateTimeCode.Code.Trim().ToLower()));
                    if (newTimeCode != null)
                    {
                        newTimeAbsenceRuleHead.TimeCodeId = newTimeCode.TimeCodeId;
                    }
                    else
                    {
                        string error = $"TimeCode not found {templateTimeCode.Code} {templateTimeCode.Name} in absence rule {timeAbsenceRuleCopyItems.Name}";
                        LogError("CopyTimeAbsenseRulesFromTemplateCompany " + error);
                        continue;
                    }

                    #endregion

                    #region EmployeeGroup

                    List<int> templateEmployeeGroupIds = timeAbsenceRuleCopyItems.TimeAbsenceRuleHeadEmployeeGroupCopyItems.Select(s => s.EmployeeGroupId).ToList();
                    if (!templateEmployeeGroupIds.IsNullOrEmpty() && existingEmployeeGroups != null)
                    {
                        foreach (var templateEmployeeGroupId in templateEmployeeGroupIds)
                        {
                            var templateEmployeeGroup = item.TemplateCompanyTimeDataItem.EmployeeGroupCopyItems.FirstOrDefault(p => p.EmployeeGroupId == templateEmployeeGroupId);
                            if (templateEmployeeGroup == null)
                                continue;

                            EmployeeGroup newEmployeeGroup = item.TemplateCompanyTimeDataItem.GetEmployeeGroup(templateEmployeeGroupId) ?? existingEmployeeGroups.FirstOrDefault(p => p.Name.Trim().ToLower().Equals(templateEmployeeGroup.Name.Trim().ToLower()));
                            if (newEmployeeGroup == null)
                            {
                                LogError($"CopyTimeAbsenseRulesFromTemplateCompany: EmployeeGroup not found {templateEmployeeGroup.Name}");
                                continue;
                            }

                            TimeRuleManager.CreateTimeAbsenceRuleHeadEmployeeGroup(entities, newTimeAbsenceRuleHead, newEmployeeGroup.EmployeeGroupId);
                        }
                    }

                    #endregion

                    #region TimeAbsenceRuleRow

                    foreach (var templateTimeAbsenceRuleRow in timeAbsenceRuleCopyItems.TimeAbsenceRuleRows)
                    {
                        TimeAbsenceRuleRow newTimeAbsenceRuleRow = new TimeAbsenceRuleRow()
                        {
                            HasMultiplePayrollProducts = templateTimeAbsenceRuleRow.HasMultiplePayrollProducts,
                            Type = templateTimeAbsenceRuleRow.Type,
                            Scope = templateTimeAbsenceRuleRow.Scope,
                            Start = templateTimeAbsenceRuleRow.Start,
                            Stop = templateTimeAbsenceRuleRow.Stop,
                            PayrollProductId = item.TemplateCompanyTimeDataItem.GetPayrollProduct(templateTimeAbsenceRuleRow.PayrollProductId ?? 0)?.ProductId,
                        };
                        SetCreatedProperties(newTimeAbsenceRuleRow);

                        #region TimeAbsenceRuleRowPayrollProducts

                        if (templateTimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts != null)
                        {
                            foreach (var templateTimeAbsenceRuleRowPayrollProducts in templateTimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts)
                            {
                                var sourcePayrollProduct = item.TemplateCompanyTimeDataItem.GetPayrollProduct(templateTimeAbsenceRuleRowPayrollProducts.SourcePayrollProductId);
                                var targetPayrollProduct = item.TemplateCompanyTimeDataItem.GetPayrollProduct(templateTimeAbsenceRuleRowPayrollProducts.TargetPayrollProductId ?? 0);

                                if (sourcePayrollProduct != null)
                                {
                                    newTimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.Add(new TimeAbsenceRuleRowPayrollProducts()
                                    {
                                        SourcePayrollProductId = sourcePayrollProduct.ProductId,
                                        TargetPayrollProductId = targetPayrollProduct?.ProductId,
                                    });
                                }
                            }
                        }

                        #endregion

                        newTimeAbsenceRuleHead.TimeAbsenceRuleRow.Add(newTimeAbsenceRuleRow);
                    }

                    entities.TimeAbsenceRuleHead.AddObject(newTimeAbsenceRuleHead);

                    #endregion
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    companyTemplateManager.LogCopyError("TimeAbsenceRule", item, saved: true);
                templateResult.ActionResults.Add(result);
            }

            return templateResult;
        }

        #endregion

        #region TimeAttestRules

        public TemplateResult CopyTimeAttestRulesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                User user = UserManager.GetUser(entities, item.UserId);
                if (user == null)
                    return new TemplateResult(new ActionResult((int)ActionResultSave.EntityNotFound, "User"));

                List<AttestRuleHead> existingAttestRuleHeads = AttestManager.GetAttestRuleHeads(entities, SoeModule.Time, item.DestinationActorCompanyId, true, loadEmployeeGroups: true, loadRows: true);
                List<EmployeeGroup> existingEmployeeGroups = EmployeeManager.GetEmployeeGroups(entities, item.DestinationActorCompanyId);
                List<PayrollProduct> existingPayrollProducts = ProductManager.GetPayrollProducts(entities, item.DestinationActorCompanyId, active: null);

                foreach (var timeAttestRuleCopyItem in item.TemplateCompanyTimeDataItem.TimeAttestRuleCopyItems)
                {
                    if (existingAttestRuleHeads.Any(r => r.Module == (int)timeAttestRuleCopyItem.Module && r.Name.Trim().ToLower().Equals(timeAttestRuleCopyItem.Name.Trim().ToLower())))
                        continue;

                    var dayTypeId = item.TemplateCompanyTimeDataItem.GetDayType(timeAttestRuleCopyItem.DayTypeId ?? 0)?.DayTypeId;

                    if (timeAttestRuleCopyItem.DayTypeId.HasValue && !dayTypeId.HasValue)
                    {
                        var match = item.TemplateCompanyTimeDataItem.DayTypeCopyItems.FirstOrDefault(p => p.Name == timeAttestRuleCopyItem.Name);

                        if (match != null)
                            dayTypeId = entities.DayType.FirstOrDefault(w => w.ActorCompanyId == item.DestinationActorCompanyId && w.State == (int)SoeEntityState.Active && w.Name == match.Name)?.DayTypeId;
                        else
                            continue;
                    }

                    AttestRuleHead newAttestRuleHead = new AttestRuleHead()
                    {
                        Module = (int)timeAttestRuleCopyItem.Module,
                        Name = timeAttestRuleCopyItem.Name,
                        Description = timeAttestRuleCopyItem.Description,

                        //Set FK
                        ActorCompanyId = item.DestinationActorCompanyId,
                    };
                    SetCreatedProperties(newAttestRuleHead, user);
                    entities.AttestRuleHead.AddObject(newAttestRuleHead);

                    if (timeAttestRuleCopyItem.DayTypeId.HasValue)
                        newAttestRuleHead.DayTypeId = dayTypeId;

                    if (timeAttestRuleCopyItem.EmployeeGroups.Any())
                    {
                        foreach (var templateEmployeeGroup in timeAttestRuleCopyItem.EmployeeGroups)
                        {
                            var employeeGroupId = item.TemplateCompanyTimeDataItem.GetEmployeeGroup(templateEmployeeGroup.EmployeeGroupId)?.EmployeeGroupId;

                            if (employeeGroupId == null)
                            {
                                if (!item.TemplateCompanyTimeDataItem.EmployeeGroupCopyItems.Any())
                                    item.TemplateCompanyTimeDataItem.EmployeeGroupCopyItems = GetEmployeeGroupCopyItemsFromApi(item.SysCompDbId, item.SourceActorCompanyId);

                                var match = item.TemplateCompanyTimeDataItem.EmployeeGroupCopyItems.FirstOrDefault(p => p.EmployeeGroupId == timeAttestRuleCopyItem.EmployeeGroupId);

                                if (match != null)
                                    employeeGroupId = entities.EmployeeGroup.FirstOrDefault(w => w.ActorCompanyId == item.DestinationActorCompanyId && w.State == (int)SoeEntityState.Active && w.Name == match.Name)?.EmployeeGroupId;
                                else
                                    continue;
                            }

                            EmployeeGroup newEmployeeGroup = existingEmployeeGroups.FirstOrDefault(i => i.EmployeeGroupId == employeeGroupId);
                            if (newEmployeeGroup == null)
                                continue;

                            newAttestRuleHead.EmployeeGroup.Add(newEmployeeGroup);
                        }
                    }

                    #region Rows

                    foreach (var attestRuleRow in timeAttestRuleCopyItem.AttestRuleRows)
                    {
                        AttestRuleRow newAttestRuleRow = new AttestRuleRow()
                        {
                            LeftValueType = attestRuleRow.LeftValueType,
                            LeftValueId = 0,
                            ComparisonOperator = attestRuleRow.ComparisonOperator,
                            RightValueType = attestRuleRow.RightValueType,
                            RightValueId = 0,
                            Minutes = attestRuleRow.Minutes,
                        };
                        SetCreatedProperties(newAttestRuleRow, user);

                        switch (attestRuleRow.LeftValueType)
                        {
                            case (int)TermGroup_AttestRuleRowLeftValueType.TimeCode:
                                newAttestRuleRow.LeftValueId = item.TemplateCompanyTimeDataItem.GetTimeCode(attestRuleRow.LeftValueId)?.TimeCodeId ?? 0;
                                break;
                            case (int)TermGroup_AttestRuleRowLeftValueType.PayrollProduct:
                                newAttestRuleRow.LeftValueId = GetExistingPayrollProduct(existingPayrollProducts, item.TemplateCompanyTimeDataItem.PayrollProductCopyItems, attestRuleRow.LeftValueId)?.ProductId ?? 0;
                                break;
                            case (int)TermGroup_AttestRuleRowLeftValueType.InvoiceProduct:
                                //newAttestRuleRow.LeftValueId = GetExistingInvoiceProduct(existingInvoiceProducts, item.TemplateCompanyBillingDataItem.InvoiceProductCopyItems, templateAttestRuleRow.LeftValueId)?.ProductId ?? 0;
                                break;
                        }

                        switch (attestRuleRow.RightValueType)
                        {
                            case (int)TermGroup_AttestRuleRowRightValueType.TimeCode:
                                newAttestRuleRow.RightValueId = item.TemplateCompanyTimeDataItem.GetTimeCode(attestRuleRow.RightValueId)?.TimeCodeId ?? 0;
                                break;
                            case (int)TermGroup_AttestRuleRowRightValueType.PayrollProduct:
                                newAttestRuleRow.RightValueId = GetExistingPayrollProduct(existingPayrollProducts, item.TemplateCompanyTimeDataItem.PayrollProductCopyItems, attestRuleRow.RightValueId)?.ProductId ?? 0;
                                break;
                            case (int)TermGroup_AttestRuleRowRightValueType.InvoiceProduct:
                                //newAttestRuleRow.RightValueId = GetExistingInvoiceProduct(existingInvoiceProducts, item.TemplateCompanyBillingDataItem.InvoiceProductCopyItems, templateAttestRuleRow.RightValueId)?.ProductId ?? 0;
                                break;
                        }

                        newAttestRuleHead.AttestRuleRow.Add(newAttestRuleRow);

                        #endregion

                    }
                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                        companyTemplateManager.LogCopyError("TimeRule", item, saved: true);
                    templateResult.ActionResults.Add(result);
                }

            }


            return templateResult;
        }


        public List<TimeAttestRuleCopyItem> GetTimeAttestRuleCopyItems(int actorCompanyId)
        {
            List<TimeAttestRuleCopyItem> timeAttestRuleCopyItems = new List<TimeAttestRuleCopyItem>();
            List<AttestRuleHead> attestRuleHeads = AttestManager.GetAttestRuleHeads(SoeModule.Time, actorCompanyId, true, loadEmployeeGroups: true, loadRows: true);

            foreach (var attestRuleHead in attestRuleHeads)
            {
                TimeAttestRuleCopyItem timeAttestRuleCopyItem = new TimeAttestRuleCopyItem
                {
                    AttestRuleId = attestRuleHead.AttestRuleHeadId,
                    Module = (SoeModule)attestRuleHead.Module,
                    Name = attestRuleHead.Name,
                    Description = attestRuleHead.Description
                };

                attestRuleHead.EmployeeGroup.ToList().ForEach(p => timeAttestRuleCopyItem.EmployeeGroups.Add(new EmployeeGroupCopyItem { EmployeeGroupId = p.EmployeeGroupId, Name = p.Name }));

                // Map AttestRuleRows
                timeAttestRuleCopyItem.AttestRuleRows = new List<TimeAttestRuleRowCopyItem>();
                foreach (var attestRuleRow in attestRuleHead.AttestRuleRow)
                {
                    TimeAttestRuleRowCopyItem rowCopyItem = new TimeAttestRuleRowCopyItem
                    {
                        LeftValueType = attestRuleRow.LeftValueType,
                        LeftValueId = attestRuleRow.LeftValueId,
                        ComparisonOperator = attestRuleRow.ComparisonOperator,
                        RightValueType = attestRuleRow.RightValueType,
                        RightValueId = attestRuleRow.RightValueId,
                        Minutes = attestRuleRow.Minutes
                    };
                    timeAttestRuleCopyItem.AttestRuleRows.Add(rowCopyItem);
                }

                timeAttestRuleCopyItems.Add(timeAttestRuleCopyItem);
            }

            return timeAttestRuleCopyItems;
        }

        public List<TimeAttestRuleCopyItem> GetTimeAttestRuleCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetTimeAttestRuleCopyItems(actorCompanyId);

            return timeTemplateConnector.GetTimeAttestRuleCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region Collective Agreement

        public List<EmployeeCollectiveAgreementCopyItem> GetEmployeeCollectiveAgreementCopyItems(int actorCompanyId)
        {
            List<EmployeeCollectiveAgreementCopyItem> employeeCollectiveAgreementCopyItems = new List<EmployeeCollectiveAgreementCopyItem>();
            var employeeCollectiveAgreements = EmployeeManager.GetEmployeeCollectiveAgreements(actorCompanyId);

            foreach (var templateEmployeeCollectiveAgreement in employeeCollectiveAgreements)
            {
                EmployeeCollectiveAgreementCopyItem employeeCollectiveAgreementCopyItem = new EmployeeCollectiveAgreementCopyItem()
                {
                    EmployeeCollectiveAgreementId = templateEmployeeCollectiveAgreement.EmployeeCollectiveAgreementId,
                    ActorCompanyId = templateEmployeeCollectiveAgreement.ActorCompanyId,
                    Code = templateEmployeeCollectiveAgreement.Code,
                    ExternalCode = templateEmployeeCollectiveAgreement.ExternalCode,
                    Name = templateEmployeeCollectiveAgreement.Name,
                    Description = templateEmployeeCollectiveAgreement.Description,
                    EmployeeGroupId = templateEmployeeCollectiveAgreement.EmployeeGroupId,
                    PayrollGroupId = templateEmployeeCollectiveAgreement.PayrollGroupId,
                    VacationGroupId = templateEmployeeCollectiveAgreement.VacationGroupId
                };
                employeeCollectiveAgreementCopyItems.Add(employeeCollectiveAgreementCopyItem);
            }

            return employeeCollectiveAgreementCopyItems;
        }

        public List<EmployeeCollectiveAgreementCopyItem> GetEmployeeCollectiveAgreementCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetEmployeeCollectiveAgreementCopyItems(actorCompanyId);

            return timeTemplateConnector.GetEmployeeCollectiveAgreementCopyItems(sysCompDbId, actorCompanyId);
        }


        #endregion

        #region EmployeeTemplates

        public List<EmployeeTemplateCopyItem> GetEmployeeTemplateCopyItems(int actorCompanyId)
        {
            List<EmployeeTemplateCopyItem> employeeTemplateCopyItems = new List<EmployeeTemplateCopyItem>();
            var templateEmployeeTemplates = EmployeeManager.GetEmployeeTemplates(actorCompanyId, loadCollectiveAgreement: true, loadGroups: true, loadRows: true, onlyActive: true);
            var collectiveAgreements = GetEmployeeCollectiveAgreementCopyItems(actorCompanyId);

            foreach (var templateEmployeeTemplate in templateEmployeeTemplates)
            {
                EmployeeTemplateCopyItem employeeTemplateCopyItem = new EmployeeTemplateCopyItem()
                {
                    Code = templateEmployeeTemplate.Code,
                    ExternalCode = templateEmployeeTemplate.ExternalCode,
                    Name = templateEmployeeTemplate.Name,
                    Description = templateEmployeeTemplate.Description,
                    Title = templateEmployeeTemplate.Title,
                    EmployeeCollectiveAgreement = collectiveAgreements.FirstOrDefault(f => f.EmployeeCollectiveAgreementId == (templateEmployeeTemplate.EmployeeCollectiveAgreement?.EmployeeCollectiveAgreementId ?? 0))
                };

                foreach (var group in templateEmployeeTemplate.EmployeeTemplateGroup.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    EmployeeTemplateGroupCopyItem groupCopyItem = new EmployeeTemplateGroupCopyItem()
                    {
                        Code = group.Code,
                        Description = group.Description,
                        Name = group.Name,
                        SortOrder = group.SortOrder,
                    };

                    foreach (var row in group.EmployeeTemplateGroupRow)
                    {
                        EmployeeTemplateGroupRowCopyItem rowCopyItem = new EmployeeTemplateGroupRowCopyItem()
                        {
                            Type = row.Type,
                            MandatoryLevel = row.MandatoryLevel,
                            RegistrationLevel = row.RegistrationLevel,
                            Title = row.Title,
                            DefaultValue = row.DefaultValue,
                            Comment = row.Comment,
                            Row = row.Row,
                            StartColumn = row.StartColumn,
                            SpanColumns = row.SpanColumns,
                            Format = row.Format,
                            HideInReport = row.HideInReport,
                            HideInReportIfEmpty = row.HideInReportIfEmpty,
                            HideInRegistration = row.HideInRegistration,
                            HideInEmploymentRegistration = row.HideInEmploymentRegistration,
                            Entity = row.Entity,
                        };
                        groupCopyItem.EmployeeTemplateGroupRows.Add(rowCopyItem);
                    }
                }

                employeeTemplateCopyItems.Add(employeeTemplateCopyItem);
            }

            return employeeTemplateCopyItems;
        }

        public List<EmployeeTemplateCopyItem> GetEmployeeTemplateCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetEmployeeTemplateCopyItems(actorCompanyId);

            return timeTemplateConnector.GetEmployeeTemplateCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion
    }
}

using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class CalendarManager : ManagerBase
    {
        #region Ctor

        public CalendarManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Date functions

        public static DateTime GetFirstDayInMonth(DateTime date)
        {
            return date.AddDays(-(date.Day - 1));
        }

        public static DateTime GetLastDayInMonth(DateTime date)
        {
            return date.AddMonths(1).AddDays(-(date.Day));
        }

        public string GetDayOfWeek(DayOfWeek dayOfWeek)
        {
            string name = "";
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return GetText(2, (int)TermGroup.StandardDayOfWeek, "Måndag");
                case DayOfWeek.Tuesday:
                    return GetText(3, (int)TermGroup.StandardDayOfWeek, "Tisdag");
                case DayOfWeek.Wednesday:
                    return GetText(4, (int)TermGroup.StandardDayOfWeek, "Onsdag");
                case DayOfWeek.Thursday:
                    return GetText(5, (int)TermGroup.StandardDayOfWeek, "Torsdag");
                case DayOfWeek.Friday:
                    return GetText(6, (int)TermGroup.StandardDayOfWeek, "Fredag");
                case DayOfWeek.Saturday:
                    return GetText(7, (int)TermGroup.StandardDayOfWeek, "Lördag");
                case DayOfWeek.Sunday:
                    return GetText(1, (int)TermGroup.StandardDayOfWeek, "Söndag");
            }
            return name;
        }

        #endregion

        #region DayType

        public List<DayType> GetWeekendSalayDayTypes(CompEntities entities, int actorCompanyId)
        {
            List<DayType> dayTypes = (from dt in entities.DayType
                                      where dt.ActorCompanyId == actorCompanyId &&
                                      dt.State == (int)SoeEntityState.Active
                                      select dt).ToList();

            return dayTypes.Where(x => x.WeekendSalary).ToList();
        }

        public List<DayType> GetDayTypesBySearch(int actorCompanyId, string search, int take)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DayType.NoTracking();
            return (from d in entities.DayType
                    where d.ActorCompanyId == actorCompanyId &&
                    d.Name.ToLower().Contains(search.ToLower()) &&
                    d.State == (int)SoeEntityState.Active
                    orderby d.Name ascending
                    select d).Take(take).ToList();
        }

        public List<DayType> GetDayTypesWithStandardWeekDay(CompEntities entities, int actorCompanyId)
        {
            return (from dt in entities.DayType
                    where dt.ActorCompanyId == actorCompanyId &&
                    dt.StandardWeekdayTo.HasValue
                    select dt).ToList();
        }

        public List<DayType> GetDayTypesByCompany(int actorCompanyId, bool includeEmployeeGroups = false, int? dayTypeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DayType.NoTracking();
            return GetDayTypesByCompany(entities, actorCompanyId, includeEmployeeGroups, dayTypeId);
        }

        public List<DayType> GetDayTypesByCompany(CompEntities entities, int actorCompanyId, bool includeEmployeeGroups = false, int? dayTypeId = null)
        {
            IQueryable<DayType> query = (from d in entities.DayType
                                         where d.ActorCompanyId == actorCompanyId &&
                                         d.State == (int)SoeEntityState.Active
                                         select d);

            if (includeEmployeeGroups)
                query = query.Include("EmployeeGroup");

            if (dayTypeId.HasValue)
                query = query.Where(t => t.DayTypeId == dayTypeId);

            List<DayType> dayTypes = query.ToList();
            return dayTypes.OrderBy(d => d.Name).ToList();
        }

        public List<DayType> FilterCompanyDayTypes(List<HolidayDTO> holidaysForCompany, List<DayType> dayTypesForCompany, DateTime date)
        {
            List<DayType> dayTypes = new List<DayType>();

            List<HolidayDTO> holidays = (from h in holidaysForCompany
                                         where h.Date.Year == date.Year &&
                                         h.Date.Month == date.Month &&
                                         h.Date.Day == date.Day &&
                                         h.State == (int)SoeEntityState.Active
                                         select h).ToList();

            foreach (HolidayDTO holiday in holidays.Where(holiday => holiday.DayType != null))
            {
                DayType dayType = dayTypesForCompany.FirstOrDefault(w => w.DayTypeId == holiday.DayTypeId);
                if (dayType != null)
                    dayTypes.Add(dayType);
            }

            if (!dayTypes.Any())
            {
                // TODO: Check localization
                int dayOfWeek = CalendarUtility.GetDayNrFromCulture(date);

                dayTypes = (from dt in dayTypesForCompany
                            where dt.StandardWeekdayFrom.HasValue &&
                            dt.StandardWeekdayTo.HasValue &&
                            dt.StandardWeekdayFrom.Value <= dayOfWeek &&
                            dt.StandardWeekdayTo.Value >= dayOfWeek &&
                            dt.State == (int)SoeEntityState.Active
                            select dt).ToList();
            }

            return dayTypes;
        }

        public Dictionary<int, string> GetDayTypesByCompanyDict(int actorCompanyId, bool addEmptyRow, int? employeeGroupId = null, bool onlyHolidaySalary = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DayType.NoTracking();
            return GetDayTypesByCompanyDict(entities, actorCompanyId, addEmptyRow, employeeGroupId, onlyHolidaySalary);
        }

        public Dictionary<int, string> GetDayTypesByCompanyDict(CompEntities entities, int actorCompanyId, bool addEmptyRow, int? employeeGroupId = null, bool onlyHolidaySalary = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<DayType> dayTypes = GetDayTypesByCompany(entities, actorCompanyId, employeeGroupId.HasValue);
            foreach (DayType daytype in dayTypes)
            {
                // If employee group parameter is specified, only return day types for specified employee group
                if (employeeGroupId.HasValue && (daytype.EmployeeGroup == null || daytype.EmployeeGroup.Any(e => e.EmployeeGroupId != employeeGroupId.Value)))
                    continue;

                if (onlyHolidaySalary && !daytype.WeekendSalary)
                    continue;

                if (!dict.ContainsKey(daytype.DayTypeId))
                    dict.Add(daytype.DayTypeId, daytype.Name);
            }

            return dict.Sort();
        }

        public Dictionary<int, string> GetDaysOfWeekDict(bool addEmptyRow = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(-1, "");

            var terms = base.GetTermGroupContent(TermGroup.StandardDayOfWeek, sortById: true);
            foreach (var term in terms)
            {
                if (!dict.ContainsKey(term.Id))
                    dict.Add(term.Id, term.Name);
            }

            return dict;
        }

        public Dictionary<int, string> GetDayTypeClassificationsDict()
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

            foreach (string e in Enum.GetNames(typeof(SoeDayTypeClassification)))
            {
                if (e == Enum.GetName(typeof(SoeDayTypeClassification), SoeDayTypeClassification.Undefined))
                    result.Add((int)SoeDayTypeClassification.Undefined, string.Empty);
                else if (e == Enum.GetName(typeof(SoeDayTypeClassification), SoeDayTypeClassification.Weekday))
                    result.Add((int)SoeDayTypeClassification.Weekday, GetText(4374, "Vardag"));
                else if (e == Enum.GetName(typeof(SoeDayTypeClassification), SoeDayTypeClassification.Saturday))
                    result.Add((int)SoeDayTypeClassification.Saturday, GetText(4375, "Lördag"));
                else if (e == Enum.GetName(typeof(SoeDayTypeClassification), SoeDayTypeClassification.Sunday))
                    result.Add((int)SoeDayTypeClassification.Sunday, GetText(4376, "Söndag"));
            }
            return result;
        }

        public DayType GetDayType(int dayTypeId, int actorCompanyId, bool loadEmployeeGroup = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DayType.NoTracking();
            return GetDayType(entities, dayTypeId, actorCompanyId, loadEmployeeGroup);
        }

        public DayType GetDayType(CompEntities entities, int dayTypeId, int actorCompanyId, bool includeEmployeeGroup = false)
        {
            IQueryable<DayType> query = (from d in entities.DayType
                                         where d.DayTypeId == dayTypeId &&
                                         d.ActorCompanyId == actorCompanyId &&
                                         d.State == (int)SoeEntityState.Active
                                         select d);

            if (includeEmployeeGroup)
                query = query.Include("EmployeeGroup");

            return query.FirstOrDefault();
        }

        public DayType GetDayType(DateTime date, EmployeeGroup employeeGroup, List<HolidayDTO> holidaysForCompany, List<DayType> dayTypesForCompany)
        {
            if (employeeGroup?.DayType == null)
                return null;

            List<DayType> dayTypesForDate = FilterCompanyDayTypes(holidaysForCompany, dayTypesForCompany, date);
            foreach (DayType dayType in dayTypesForDate)
            {
                List<DayType> dayTypesForEmployeeGroup = employeeGroup.DayType.Where(i => i.DayTypeId == dayType.DayTypeId).ToList();
                if (dayTypesForEmployeeGroup.Any())
                    return dayTypesForEmployeeGroup.First();//Take first
            }

            return null;
        }

        public DayType GetDayType(string name, int actorCompanyId, bool loadEmployeeGroup = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DayType.NoTracking();
            return GetDayType(entities, name, actorCompanyId, loadEmployeeGroup);
        }

        public DayType GetDayType(CompEntities entities, string name, int actorCompanyId, bool loadEmployeeGroup = false)
        {
            if (loadEmployeeGroup)
            {
                return (from dt in entities.DayType
                            .Include("EmployeeGroup")
                        where dt.Name == name &&
                        dt.ActorCompanyId == actorCompanyId &&
                        dt.State == (int)SoeEntityState.Active
                        select dt).FirstOrDefault();
            }
            else
            {
                return (from d in entities.DayType
                        where d.Name == name &&
                        d.ActorCompanyId == actorCompanyId &&
                        d.State == (int)SoeEntityState.Active
                        select d).FirstOrDefault();
            }
        }

        public ActionResult SaveDayType(DayTypeDTO dayTypeInput, int actorCompanyId)
        {
            if (dayTypeInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DayType");

            // Default result is successful
            ActionResult result = new ActionResult();

            int dayTypeId = dayTypeInput.DayTypeId;
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region DayType

                        // Get existing
                        DayType dayType = GetDayType(entities, dayTypeId, actorCompanyId);
                        if (dayType == null)
                        {
                            #region Add
                            dayType = new DayType()
                            {
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(dayType);
                            entities.DayType.AddObject(dayType);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(dayType);

                            #endregion
                        }
                        dayType.DayTypeId = dayTypeInput.DayTypeId;
                        dayType.Description = dayTypeInput.Description;
                        dayType.Name = dayTypeInput.Name;
                        dayType.StandardWeekdayFrom = dayTypeInput.StandardWeekdayFrom;
                        dayType.StandardWeekdayTo = dayTypeInput.StandardWeekdayTo;
                        dayType.State = (int)dayTypeInput.State;
                        dayType.WeekendSalary = dayTypeInput.WeekendSalary;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            dayTypeId = dayType.DayTypeId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {

                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = dayTypeId;
                    }


                    entities.Connection.Close();
                }

                return result;
            }
        }

        public DayType GetPrevNextDayType(int dayTypeId, int actorCompanyId, SoeFormMode mode)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DayType.NoTracking();
            return GetPrevNextDayType(entities, dayTypeId, actorCompanyId, mode);
        }

        public DayType GetPrevNextDayType(CompEntities entities, int dayTypeId, int actorCompanyId, SoeFormMode mode)
        {
            DayType dayType = null;

            if (mode == SoeFormMode.Next)
            {
                dayType = (from d in entities.DayType
                           where d.Company.ActorCompanyId == actorCompanyId &&
                           d.DayTypeId > dayTypeId &&
                           d.State == (int)SoeEntityState.Active
                           orderby d.DayTypeId ascending
                           select d).FirstOrDefault();
            }
            else
            {
                dayType = (from d in entities.DayType
                           where d.DayTypeId < dayTypeId &&
                           d.State == (int)SoeEntityState.Active &&
                           d.Company.ActorCompanyId == actorCompanyId
                           orderby d.DayTypeId descending
                           select d).FirstOrDefault();
            }

            return dayType;
        }

        public bool IsDateHalfDay(List<HolidayDTO> holidaysForDate, DayType dayTypeForDate)
        {
            if (dayTypeForDate == null)
                return false;

            foreach (HolidayDTO holidayForDate in holidaysForDate)
            {
                if (holidayForDate.DayType != null && !holidayForDate.DayType.TimeHalfdays.IsNullOrEmpty() && holidayForDate.DayTypeId == dayTypeForDate.DayTypeId)
                    return true;
            }

            return false;
        }

        public bool DayTypeExists(string dayTypeName, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DayType.NoTracking();
            return DayTypeExists(entities, dayTypeName, actorCompanyId);
        }

        private bool DayTypeExists(CompEntities entities, string dayTypeName, int actorCompanyId)
        {
            return entities.DayType.Any(d => d.Name == dayTypeName && d.ActorCompanyId == actorCompanyId && d.State == (int)SoeEntityState.Active);
        }

        private bool DayTypeHasHolidays(int dayTypeId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.Holiday.Any(h => h.DayTypeId == dayTypeId && h.State == (int)SoeEntityState.Active);
        }

        private bool DayTypeHasTimeHalfdays(int dayTypeId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.TimeHalfday.Any(h => h.DayTypeId == dayTypeId && h.State == (int)SoeEntityState.Active);
        }

        private bool DayTypeHasTimeRules(int dayTypeId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.TimeRule.Any(t => t.TimeRuleRow.Any(r => r.DayTypeId == dayTypeId) && t.State == (int)SoeEntityState.Active);
        }

        private bool DayTypeHasAttestRuleHeads(int dayTypeId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.AttestRuleHead.Any(a => a.DayTypeId == dayTypeId && a.State == (int)SoeEntityState.Active);
        }

        private bool DayTypeHasEmployeeGroups(int dayTypeId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from e in entitiesReadOnly.EmployeeGroup
                    where e.DayType.Any(d => d.DayTypeId == dayTypeId) &&
                    e.State == (int)SoeEntityState.Active
                    select e).Any();
        }

        public ActionResult AddDayType(DayType dayTypeInput, int actorCompanyId)
        {
            return AddDayType(dayTypeInput.ToDTO(), actorCompanyId);
        }

        public ActionResult AddDayType(DayTypeDTO dayTypeInput, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return AddDayType(entities, dayTypeInput, actorCompanyId);
            }
        }

        public ActionResult AddDayType(CompEntities entities, DayTypeDTO dayTypeInput, int actorCompanyId)
        {
            #region Prereq

            if (dayTypeInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DayType");

            Company company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (company == null)
                return new ActionResult(false, (int)ActionResultSave.EntityNotFound, "Company");

            if (DayTypeExists(entities, dayTypeInput.Name, actorCompanyId))
                return new ActionResult((int)ActionResultSave.DayTypeExists);

            #endregion

            DayType dayType = new DayType()
            {
                SysDayTypeId = dayTypeInput.SysDayTypeId,
                Type = (int)dayTypeInput.Type,
                Name = dayTypeInput.Name,
                Description = dayTypeInput.Description,
                StandardWeekdayFrom = dayTypeInput.StandardWeekdayFrom,
                StandardWeekdayTo = dayTypeInput.StandardWeekdayTo,

                //Set FK
                ActorCompanyId = actorCompanyId,
            };
            SetCreatedProperties(dayType);
            entities.DayType.AddObject(dayType);

            return SaveChanges(entities);
        }

        public ActionResult SaveDayTypeEmployeeGroups(Collection<FormIntervalEntryItem> employeeGroupItems, int dayTypeId, int actorCompanyId, bool saveForAllGroups)
        {
            using (CompEntities entities = new CompEntities())
            {
                DayType dayType = GetDayType(entities, dayTypeId, actorCompanyId, true);

                if (saveForAllGroups)
                {
                    List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId);
                    foreach (EmployeeGroup employeeGroup in employeeGroups)
                    {
                        if (dayType.EmployeeGroup.Any(i => i.EmployeeGroupId == employeeGroup.EmployeeGroupId))
                            continue;

                        dayType.EmployeeGroup.Add(employeeGroup);
                    }
                }
                else
                {
                    if (dayType.EmployeeGroup == null)
                        dayType.EmployeeGroup = new EntityCollection<EmployeeGroup>();

                    List<int> employeeGroupIds = new List<int>();
                    foreach (EmployeeGroup employeeGroup in dayType.EmployeeGroup)
                    {
                        employeeGroupIds.Add(employeeGroup.EmployeeGroupId);
                    }

                    foreach (var employeeGroupItem in employeeGroupItems)
                    {
                        int itemId = Convert.ToInt32(employeeGroupItem.From);
                        if (!dayType.EmployeeGroup.Any(i => i.EmployeeGroupId == itemId))
                        {
                            EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(entities, Convert.ToInt32(itemId), false, true, true, false);
                            if (employeeGroup != null)
                                dayType.EmployeeGroup.Add(employeeGroup);
                        }
                    }

                    foreach (int employeeGroupId in employeeGroupIds)
                    {
                        if (!employeeGroupItems.Any(i => i.From == employeeGroupId.ToString()))
                        {
                            EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(entities, Convert.ToInt32(employeeGroupId), false, true, true, false);
                            if (employeeGroup != null)
                                dayType.EmployeeGroup.Remove(employeeGroup);
                        }
                    }
                }
                return UpdateEntityItem(entities, dayType, dayType, "DayType");
            }
        }

        public ActionResult UpdateDayType(DayType dayType, int actorCompanyId)
        {
            if (dayType == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DayType");

            using (CompEntities entities = new CompEntities())
            {
                DayType originalDayType = GetDayType(entities, dayType.DayTypeId, actorCompanyId);
                if (originalDayType == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "DayType");

                originalDayType.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (originalDayType.Company == null)
                    return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "Company");

                return UpdateEntityItem(entities, originalDayType, dayType, "DayType");
            }
        }

        public ActionResult DeleteDayType(int dayTypeId)
        {
            CompEntities entities = new CompEntities();
            var actorCompanyId = base.ActorCompanyId;
            DayType dayType = GetDayType(entities, dayTypeId, actorCompanyId);
            return DeleteDayType(dayType, actorCompanyId);

        }

        public ActionResult DeleteDayType(DayType dayType, int actorCompanyId)
        {
            if (dayType == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "DayType");

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                // Check relations
                if (DayTypeHasHolidays(dayType.DayTypeId))
                    return new ActionResult((int)ActionResultDelete.DayTypeHasHolidays);
                if (DayTypeHasTimeHalfdays(dayType.DayTypeId))
                    return new ActionResult((int)ActionResultDelete.DayTypeHasTimeHalfdays);
                if (DayTypeHasTimeRules(dayType.DayTypeId))
                    return new ActionResult((int)ActionResultDelete.DayTypeHasTimeRules);
                if (DayTypeHasAttestRuleHeads(dayType.DayTypeId))
                    return new ActionResult((int)ActionResultDelete.DayTypeHasAttestRuleHeads);
                if (DayTypeHasEmployeeGroups(dayType.DayTypeId))
                    return new ActionResult((int)ActionResultDelete.DayTypeHasEmployeeGroups);

                #endregion

                DayType originalDayType = GetDayType(entities, dayType.DayTypeId, actorCompanyId);
                if (originalDayType == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "DayType");

                return ChangeEntityState(entities, originalDayType, SoeEntityState.Deleted, true);
            }
        }

        public List<DayTypeAndWeekdayDTO> GetDayTypesAndWeekdays(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DayType.NoTracking();
            List<DayType> types = (from d in entities.DayType
                                   where d.ActorCompanyId == actorCompanyId &&
                                   d.State == (int)SoeEntityState.Active
                                   select d).ToList();

            List<DayTypeAndWeekdayDTO> list = new List<DayTypeAndWeekdayDTO>();

            // Add day types
            foreach (DayType type in types)
            {
                list.Add(new DayTypeAndWeekdayDTO() { DayTypeId = type.DayTypeId, Name = type.Name });
            }

            // Add weekdays
            list.Add(new DayTypeAndWeekdayDTO() { WeekdayNr = (int)DayOfWeek.Monday, Name = CalendarUtility.GetDayName(DayOfWeek.Monday, Thread.CurrentThread.CurrentCulture, true) });
            list.Add(new DayTypeAndWeekdayDTO() { WeekdayNr = (int)DayOfWeek.Tuesday, Name = CalendarUtility.GetDayName(DayOfWeek.Tuesday, Thread.CurrentThread.CurrentCulture, true) });
            list.Add(new DayTypeAndWeekdayDTO() { WeekdayNr = (int)DayOfWeek.Wednesday, Name = CalendarUtility.GetDayName(DayOfWeek.Wednesday, Thread.CurrentThread.CurrentCulture, true) });
            list.Add(new DayTypeAndWeekdayDTO() { WeekdayNr = (int)DayOfWeek.Thursday, Name = CalendarUtility.GetDayName(DayOfWeek.Thursday, Thread.CurrentThread.CurrentCulture, true) });
            list.Add(new DayTypeAndWeekdayDTO() { WeekdayNr = (int)DayOfWeek.Friday, Name = CalendarUtility.GetDayName(DayOfWeek.Friday, Thread.CurrentThread.CurrentCulture, true) });
            list.Add(new DayTypeAndWeekdayDTO() { WeekdayNr = (int)DayOfWeek.Saturday, Name = CalendarUtility.GetDayName(DayOfWeek.Saturday, Thread.CurrentThread.CurrentCulture, true) });
            list.Add(new DayTypeAndWeekdayDTO() { WeekdayNr = (int)DayOfWeek.Sunday, Name = CalendarUtility.GetDayName(DayOfWeek.Sunday, Thread.CurrentThread.CurrentCulture, true) });

            return list;
        }

        #endregion

        #region Holiday
        public List<HolidayDTO> GetHolidaySalaryHolidays(CompEntities entities, DateTime dateFrom, DateTime dateTo, int actorCompanyId)
        {
            var weekendSalaryDayTypeIds = GetWeekendSalayDayTypes(entities, actorCompanyId).Select(x => x.DayTypeId).ToList();
            if (!weekendSalaryDayTypeIds.Any())
                return new List<HolidayDTO>();

            var holidays = (from h in entities.Holiday
                            where h.ActorCompanyId == actorCompanyId &&
                            weekendSalaryDayTypeIds.Contains(h.DayTypeId) &&
                            h.State == (int)SoeEntityState.Active
                            select h).ToList();

            var holidayDtos = AddDatesFromSysHoliday(holidays.ToDTOs(false).ToList());
            holidayDtos = holidayDtos.Where(x => x.Date >= dateFrom && x.Date <= dateTo).ToList();
            return holidayDtos;
        }

        public List<HolidayDTO> GetHolidaysByCompany(int actorCompanyId, int? year = null, bool onlyRedDay = false, bool onlyHistorical = false, bool loadDayType = false, int? holidayId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Holiday.NoTracking();
            return GetHolidaysByCompany(entities, actorCompanyId, year, onlyRedDay, onlyHistorical, loadDayType, holidayId);
        }

        public List<HolidayDTO> GetHolidaysByCompany(CompEntities entities, int actorCompanyId, int? year = null, bool onlyRedDay = false, bool onlyHistorical = false, bool loadDayType = false, int? holidayId = null)
        {
            List<Holiday> holidays = null;

            if (loadDayType)
            {
                holidays = (from h in entities.Holiday
                                .Include("DayType")
                            where h.ActorCompanyId == actorCompanyId &&
                            h.State == (int)SoeEntityState.Active
                            select h).ToList();
            }
            else
            {
                holidays = (from h in entities.Holiday
                            where h.ActorCompanyId == actorCompanyId &&
                            h.State == (int)SoeEntityState.Active
                            select h).ToList();
            }

            if (holidayId.HasValue)
                holidays = holidays.Where(h => h.HolidayId == holidayId.Value).ToList();

            var dtos = AddDatesFromSysHoliday(holidays.ToDTOs(loadDayType, false).ToList()).ToList();

            if (year.HasValue && year.Value > 0)
                dtos = dtos.Where(i => i.Date.Year == year).ToList();
            if (onlyRedDay)
                dtos = dtos.Where(i => i.IsRedDay).ToList();
            if (onlyHistorical)
                dtos = dtos.Where(i => i.Date <= DateTime.Today).ToList();

            return dtos;
        }

        public List<HolidayDTO> GetHolidaysByCompany(int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Holiday.NoTracking();
            List<Holiday> holidays = (from h in entities.Holiday
                                      where h.ActorCompanyId == actorCompanyId &&
                                      h.State == (int)SoeEntityState.Active
                                      select h).ToList();

            List<HolidayDTO> dtos = holidays.ToDTOs(true, false).Where(d => d.Date == CalendarUtility.DATETIME_DEFAULT || (d.Date >= dateFrom && d.Date <= dateTo)).ToList();

            dtos = AddDatesFromSysHoliday(dtos);

            // Need to filter on date again since sys holidays have 1900-01-01 in first query
            // and get actual date after AddDatesFromSysHoliday
            return dtos.Where(d => d.Date >= dateFrom && d.Date <= dateTo).ToList();
        }

        public List<HolidayDTO> GetHolidaysByCompanyWithDayTypeAndHalfDay(int actorCompanyId, DateTime? fromDate = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetHolidaysByCompanyWithDayTypeAndHalfDay(entities, actorCompanyId, fromDate);
        }

        public List<HolidayDTO> GetHolidaysByCompanyWithDayTypeAndHalfDay(CompEntities entities, int actorCompanyId, DateTime? fromDate = null)
        {
            if (fromDate.HasValue)
                fromDate = fromDate.Value.AddYears(-1); //do not fetch holidays older than one year

            var holidays = (from h in entities.Holiday
                                .Include("DayType.TimeHalfday.TimeCodeBreak")
                                .Include("DayType.EmployeeGroup")
                            where h.ActorCompanyId == actorCompanyId &&
                            h.State == (int)SoeEntityState.Active &&
                            (!fromDate.HasValue || (h.Date >= fromDate.Value || h.SysHolidayTypeId.HasValue))
                            select h).ToList();

            return AddDatesFromSysHoliday(holidays.ToDTOs(true).ToList());
        }

        public List<HolidayDTO> AddDatesFromSysHoliday(IEnumerable<HolidayDTO> holidaysInput)
        {
            List<HolidayDTO> holidays = holidaysInput.Where(w => !w.SysHolidayTypeId.HasValue).ToList();
            List<HolidayDTO> holidaysFromSys = holidaysInput.Where(w => w.SysHolidayTypeId.HasValue).ToList();
            if (holidaysFromSys.Count > 0)
            {
                List<SysHolidayTypeDTO> types = SysDbCache.Instance.SysHolidayTypeDTOs;

                foreach (HolidayDTO holidayFromSys in holidaysFromSys)
                {
                    List<SysHolidayDTO> sysHolidays = SysDbCache.Instance.SysHolidayDTOs.Where(w => w.SysHolidayTypeId.HasValue && w.SysHolidayTypeId.Value == holidayFromSys.SysHolidayTypeId.Value).ToList();
                    if (sysHolidays.Any())
                    {
                        foreach (SysHolidayDTO sysHoliday in sysHolidays)
                        {
                            if (holidays.Select(s => s.Date).Contains(sysHoliday.Date))
                                continue;

                            SysHolidayTypeDTO type = null;
                            if (sysHoliday.SysHolidayTypeId.HasValue)
                                type = types.FirstOrDefault(t => t.SysHolidayTypeId == sysHoliday.SysHolidayTypeId);

                            HolidayDTO holidayClone = holidayFromSys.CloneDTO();
                            holidayClone.Date = sysHoliday.Date;
                            holidayClone.Name = string.Concat(holidayClone.Name, "*");
                            holidayClone.SysHolidayTypeName = type != null ? GetText(type.SysTermId, type.SysTermGroupId, "") : "";

                            if (!holidayFromSys.Created.HasValue || holidayFromSys.Created.Value.Year >= 2018 || (holidayFromSys.Modified.HasValue && holidayFromSys.Modified.Value.Year >= 2018 && holidayFromSys.Modified.Value > new DateTime(2018, 3, 31)))
                                holidays.Add(holidayClone);
                        }
                    }
                    else
                    {
                        holidays.Add(holidayFromSys);
                    }
                }
            }

            return holidays;
        }

        public List<HolidaySmallDTO> GetHolidaysByCompanySmall(int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            List<HolidaySmallDTO> dtos = GetHolidaysByCompany(actorCompanyId, dateFrom, dateTo).ToSmallDTOs().ToList();

            return dtos;
        }

        public Dictionary<int, string> GetHolidaysByCompanyDict(int actorCompanyId, bool addEmptyRow)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Holiday.NoTracking();
            return GetHolidaysByCompanyDict(entities, actorCompanyId, addEmptyRow);
        }

        public Dictionary<int, string> GetHolidaysByCompanyDict(CompEntities entities, int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<HolidayDTO> holidays = GetHolidaysByCompany(entities, actorCompanyId);
            foreach (HolidayDTO holiday in holidays)
            {
                if (!dict.ContainsKey(holiday.HolidayId))
                    dict.Add(holiday.HolidayId, holiday.Name);
            }

            return dict;
        }

        public Holiday GetHoliday(string name, int actorCompanyId, bool loadDayType = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Holiday.NoTracking();
            return GetHoliday(entities, name, actorCompanyId, loadDayType);
        }

        public Holiday GetHoliday(CompEntities entities, string name, int actorCompanyId, bool loadDayType = false)
        {
            if (loadDayType)
            {
                return (from h in entities.Holiday
                            .Include("DayType")
                        where h.ActorCompanyId == actorCompanyId &&
                        h.Name == name &&
                        h.State == (int)SoeEntityState.Active
                        select h).FirstOrDefault();
            }
            else
            {
                return (from h in entities.Holiday
                        where h.ActorCompanyId == actorCompanyId &&
                        h.Name == name &&
                        h.State == (int)SoeEntityState.Active
                        select h).FirstOrDefault();
            }
        }

        public Holiday GetHoliday(DateTime date, int dayTypeId, int actorCompanyId, int? sysHolidayTypeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Holiday.NoTracking();
            return GetHoliday(entities, date, dayTypeId, actorCompanyId, sysHolidayTypeId);
        }

        public Holiday GetHoliday(CompEntities entities, DateTime date, int dayTypeId, int actorCompanyId, int? sysHolidayTypeId)
        {
            return (from h in entities.Holiday
                    where h.DayTypeId == dayTypeId &&
                    h.ActorCompanyId == actorCompanyId &&
                    h.Date == date &&
                    h.SysHolidayTypeId == sysHolidayTypeId &&
                    h.State == (int)SoeEntityState.Active
                    select h).FirstOrDefault();
        }

        public Holiday GetHoliday(int holidayId, int actorCompanyId, bool loadDayType = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Holiday.NoTracking();
            return GetHoliday(entities, holidayId, actorCompanyId, loadDayType);
        }

        public Holiday GetHoliday(CompEntities entities, int holidayId, int actorCompanyId, bool loadDayType = false)
        {
            return (from h in entities.Holiday
                        .Include("DayType")
                    where h.ActorCompanyId == actorCompanyId &&
                    h.HolidayId == holidayId &&
                    h.State == (int)SoeEntityState.Active
                    select h).FirstOrDefault();
        }

        public HolidayDTO GetHoliday(int holidayId, int actorCompanyId, int year, bool loadDayType = false)
        {
            var holiday = GetHoliday(holidayId, actorCompanyId, loadDayType);
            if (holiday != null)
            {
                var dtos = AddDatesFromSysHoliday(new List<HolidayDTO>() { holiday.ToDTO(true) });
                if (dtos.Count > 1)
                    return dtos.FirstOrDefault(w => w.Date.Year == year);
                else
                    return dtos.FirstOrDefault();
            }

            return null;
        }

        public Holiday GetPrevNextHoliday(int holidayId, int actorCompanyId, SoeFormMode mode)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Holiday.NoTracking();
            return GetPrevNextHoliday(entities, holidayId, actorCompanyId, mode);
        }

        public Holiday GetPrevNextHoliday(CompEntities entities, int holidayId, int actorCompanyId, SoeFormMode mode)
        {
            Holiday holiday = null;

            if (mode == SoeFormMode.Next)
            {
                holiday = (from h in entities.Holiday
                           where h.HolidayId > holidayId &&
                           h.ActorCompanyId == actorCompanyId &&
                           h.State == (int)SoeEntityState.Active
                           orderby h.HolidayId ascending
                           select h).FirstOrDefault();
            }
            else
            {
                holiday = (from h in entities.Holiday
                           where h.HolidayId < holidayId &&
                           h.ActorCompanyId == actorCompanyId &&
                           h.State == (int)SoeEntityState.Active
                           orderby h.HolidayId descending
                           select h).FirstOrDefault();
            }

            return holiday;
        }


        public bool HolidayExists(DateTime date, int dayTypeId, int actorCompanyId, int? sysHolidayTypeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Holiday.NoTracking();
            return HolidayExists(entities, date, dayTypeId, actorCompanyId, sysHolidayTypeId);
        }

        public bool HolidayExists(CompEntities entities, DateTime date, int dayTypeId, int actorCompanyId, int? sysHolidayTypeId)
        {
            return (from h in entities.Holiday
                    where h.DayTypeId == dayTypeId &&
                    h.ActorCompanyId == actorCompanyId &&
                    h.Date == date &&
                    h.SysHolidayTypeId == sysHolidayTypeId &&
                    h.State == (int)SoeEntityState.Active
                    select h).Count() > 0;
        }

        public bool IsHoliday(DateTime date, int actorCompanyId, bool includeWeekend)
        {
            // Check if date is a holiday stored in the database
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Holiday.NoTracking();
            return IsHoliday(entities, date, actorCompanyId, includeWeekend);
        }

        public bool IsHoliday(CompEntities entities, DateTime date, int actorCompanyId, bool includeWeekend)
        {
            // Check if date is a holiday stored in the database
            var counter = (from h in entities.Holiday
                           where h.Date == date &&
                           h.ActorCompanyId == actorCompanyId &&
                           h.State == (int)SoeEntityState.Active
                           select h).Count();

            if (counter > 0)
                return true;

            // No holiday, check saturday or sunday
            if (includeWeekend)
                return (CalendarUtility.IsSaturday(date) || CalendarUtility.IsSunday(date));

            // This is not a holiday, nor a weekend
            return false;
        }

        public ActionResult AddHoliday(Holiday holiday, int actorCompanyId, int dayTypeId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return AddHoliday(entities, holiday, actorCompanyId, dayTypeId);
            }
        }

        public ActionResult AddHoliday(HolidayDTO holiday, int actorCompanyId, int dayTypeId)
        {
            using (CompEntities entities = new CompEntities())
            {
                if (holiday.ActorCompanyId == 0)
                    holiday.ActorCompanyId = actorCompanyId;
                return AddHoliday(entities, holiday.FromDTO(), actorCompanyId, dayTypeId);
            }
        }

        public ActionResult AddHoliday(CompEntities entities, Holiday holiday, int actorCompanyId, int dayTypeId)
        {
            if (holiday == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Holiday");

            if (entities.Holiday.Any(w => w.HolidayId == holiday.HolidayId))
                return UpdateHoliday(holiday, dayTypeId, actorCompanyId);

            if (HolidayExists(entities, holiday.Date, dayTypeId, actorCompanyId, holiday.SysHolidayTypeId))
            {
                if (holiday.HolidayId == 0)
                {
                    Holiday existing = GetHoliday(entities, holiday.Date, dayTypeId, actorCompanyId, holiday.SysHolidayTypeId);
                    if (existing != null)
                        holiday.HolidayId = existing.HolidayId;
                }
                return UpdateHoliday(holiday, dayTypeId, actorCompanyId);
            }

            holiday.Company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (holiday.Company == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "Company");

            holiday.DayType = GetDayType(entities, dayTypeId, actorCompanyId);
            if (holiday.DayType == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "DayType");

            ActionResult result = AddEntityItem(entities, holiday, "Holiday");
            if (result.Success)
                result.IntegerValue = holiday.HolidayId;

            return result;
        }

        public ActionResult UpdateHoliday(Holiday holiday, int dayTypeId, int actorCompanyId)
        {
            if (holiday == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Holiday");

            using (CompEntities entities = new CompEntities())
            {
                Holiday originalHoliday = GetHoliday(entities, holiday.HolidayId, actorCompanyId, true);
                if (originalHoliday == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Holiday");

                originalHoliday.DayType = GetDayType(entities, dayTypeId, actorCompanyId);
                if (originalHoliday.DayType == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "DayType");

                ActionResult result = UpdateEntityItem(entities, originalHoliday, holiday, "Holiday");
                if (result.Success)
                    result.IntegerValue = holiday.HolidayId;
                return result;
            }
        }

        public ActionResult DeleteHoliday(int holidayId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                Holiday originalHoliday = GetHoliday(entities, holidayId, actorCompanyId);
                if (originalHoliday == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Holiday");

                // Set the Product to deleted if no other Companies use it
                return ChangeEntityState(entities, originalHoliday, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeleteHoliday(Holiday holiday, int actorCompanyId)
        {
            if (holiday == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "Holiday");

            using (CompEntities entities = new CompEntities())
            {
                Holiday originalHoliday = GetHoliday(entities, holiday.HolidayId, actorCompanyId);
                if (originalHoliday == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Holiday");

                // Set the Product to deleted if no other Companies use it
                return ChangeEntityState(entities, originalHoliday, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region SchoolHoliday

        public List<SchoolHoliday> GetSchoolHolidays(int actorCompanyId, int? schoolHolidayId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SchoolHoliday.NoTracking();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
            IQueryable<SchoolHoliday> query = (from sh in entities.SchoolHoliday
                                               where sh.ActorCompanyId == actorCompanyId &&
                                               sh.State == (int)SoeEntityState.Active
                                               select sh);
            if (schoolHolidayId.HasValue)
                query = query.Where(sh => sh.SchoolHolidayId == schoolHolidayId.Value);

            if (useAccountHierarchy)
            {
                query = query.Include("Account");

                var accountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(actorCompanyId, base.RoleId, base.UserId, DateTime.Today, DateTime.Today, null);

                if (!accountIds.IsNullOrEmpty())
                    query = query.Where(t => !t.AccountId.HasValue || accountIds.Contains(t.AccountId.Value));
                else
                    query = query.Where(t => !t.AccountId.HasValue);
            }
            return query.ToList();
        }

        public SchoolHoliday GetSchoolHoliday(int schoolHolidayId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SchoolHoliday.NoTracking();
            return GetSchoolHoliday(entities, schoolHolidayId, actorCompanyId);
        }

        public SchoolHoliday GetSchoolHoliday(CompEntities entities, int schoolHolidayId, int actorCompanyId)
        {
            using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, actorCompanyId);
            entitiesReadonly.SchoolHoliday.NoTracking();
            IQueryable<SchoolHoliday> query = (from sh in entitiesReadonly.SchoolHoliday
                                               where sh.ActorCompanyId == actorCompanyId &&
                                               sh.SchoolHolidayId == schoolHolidayId &&
                                               sh.State == (int)SoeEntityState.Active
                                               select sh);
            if (useAccountHierarchy)
            {
                query = query.Include("Account");

                var accountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(actorCompanyId, base.RoleId, base.UserId, DateTime.Today, DateTime.Today, null);

                if (!accountIds.IsNullOrEmpty())
                    query = query.Where(t => !t.AccountId.HasValue || accountIds.Contains(t.AccountId.Value));
                else
                    query = query.Where(t => !t.AccountId.HasValue);
            }

            return query.FirstOrDefault();
        }

        public bool SchoolHolidayExists(string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DeliveryCondition.NoTracking();
            return SchoolHolidayExists(entities, name, actorCompanyId);
        }

        public bool SchoolHolidayExists(CompEntities entities, string name, int actorCompanyId)
        {
            using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, actorCompanyId);
            IQueryable<SchoolHoliday> query = (from sh in entities.SchoolHoliday
                                               where sh.ActorCompanyId == actorCompanyId &&
                                               sh.Name == name &&
                                               sh.State == (int)SoeEntityState.Active
                                               select sh);
            if (useAccountHierarchy)
            {
                query = query.Include("Account");

                var accountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(actorCompanyId, base.RoleId, base.UserId, DateTime.Today, DateTime.Today, null);

                if (!accountIds.IsNullOrEmpty())
                    query = query.Where(t => !t.AccountId.HasValue || accountIds.Contains(t.AccountId.Value));
                else
                    query = query.Where(t => !t.AccountId.HasValue);
            }
            int counter = query.Count();

            return counter > 0;
        }

        public bool SchoolHolidaySummerExistsForYear(int year, int actorCompanyId, int? excludeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DeliveryCondition.NoTracking();
            return SchoolHolidaySummerExistsForYear(entities, year, actorCompanyId, excludeId);
        }

        public bool SchoolHolidaySummerExistsForYear(CompEntities entities, int year, int actorCompanyId, int? excludeId = null)
        {
            using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, actorCompanyId);
            IQueryable<SchoolHoliday> query = (from sh in entities.SchoolHoliday
                                               where sh.ActorCompanyId == actorCompanyId &&
                                               sh.IsSummerHoliday &&
                                               sh.State == (int)SoeEntityState.Active
                                               select sh);
            if (useAccountHierarchy)
            {
                query = query.Include("Account");

                var accountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(actorCompanyId, base.RoleId, base.UserId, DateTime.Today, DateTime.Today, null);

                if (!accountIds.IsNullOrEmpty())
                    query = query.Where(t => !t.AccountId.HasValue || accountIds.Contains(t.AccountId.Value));
                else
                    query = query.Where(t => !t.AccountId.HasValue);
            }
            var schoolHolidaysSummer = query.ToList();

            return schoolHolidaysSummer.Any(i => i.DateFrom.Year == year && i.DateTo.Year == year && (!excludeId.HasValue || excludeId.Value != i.SchoolHolidayId));
        }

        public ActionResult SaveSchoolHoliday(SchoolHolidayDTO schoolHolidayInput, int actorCompanyId)
        {
            ActionResult result = null;

            if (schoolHolidayInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SchoolHoliday");

            using (CompEntities entities = new CompEntities())
            {
                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                if (schoolHolidayInput.IsSummerHoliday)
                {
                    if (schoolHolidayInput.DateFrom.Year != schoolHolidayInput.DateTo.Year)
                        return new ActionResult((int)ActionResultSave.SchoolHolidaySummerMustBeSameYear, GetText(1061502, "Sommarlov får inte gå över årsskiftet"));
                    if (SchoolHolidaySummerExistsForYear(schoolHolidayInput.DateFrom.Year, actorCompanyId, excludeId: schoolHolidayInput.SchoolHolidayId > 0 ? schoolHolidayInput.SchoolHolidayId : (int?)null))
                        return new ActionResult((int)ActionResultSave.SchoolHolidaySummerAlreadyExists, String.Format(GetText(91900, "Sommarlov finns redan för {0}"), schoolHolidayInput.DateFrom.Year.ToString()));
                }

                // Get original condition
                SchoolHoliday schoolHoliday = GetSchoolHoliday(entities, schoolHolidayInput.SchoolHolidayId, actorCompanyId);
                if (schoolHoliday == null)
                {
                    schoolHoliday = new SchoolHoliday()
                    {
                        Company = company,
                    };
                    SetCreatedProperties(schoolHoliday);
                    entities.SchoolHoliday.AddObject(schoolHoliday);
                }
                else
                {
                    SetModifiedProperties(schoolHoliday);
                }

                schoolHoliday.Name = schoolHolidayInput.Name;
                schoolHoliday.DateFrom = schoolHolidayInput.DateFrom;
                schoolHoliday.DateTo = schoolHolidayInput.DateTo;
                schoolHoliday.IsSummerHoliday = schoolHolidayInput.IsSummerHoliday;
                schoolHoliday.AccountId = schoolHolidayInput.AccountId.HasValue && schoolHolidayInput.AccountId != 0 ? schoolHolidayInput.AccountId : (int?)null;
                result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                result.IntegerValue = schoolHoliday.SchoolHolidayId;
            }

            return result;
        }

        public ActionResult DeleteSchoolHoliday(int schoolHolidayId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                SchoolHoliday schoolHoliday = GetSchoolHoliday(entities, schoolHolidayId, actorCompanyId);
                if (schoolHoliday == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "SchoolHoliday");

                return DeleteEntityItem(entities, schoolHoliday);
            }
        }

        #endregion

        #region SysDayType

        /// <summary>
        /// Get all SysDayType's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysDayType> GetSysDayTypes()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysDayType
                            .ToList<SysDayType>();
        }

        public SysDayType GetSysDayType(int sysDayTypeId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysDayType.Where(s => s.SysDayTypeId == sysDayTypeId).FirstOrDefault();
        }

        #endregion

        #region SysHoliday

        public List<SysHolidayDTO> GetSysHolidayDTOs()
        {
            return SysServiceManager.GetSysHolidayDTOs();
        }

        public List<SysHolidayTypeDTO> GetSysHolidayTypeDTOs()
        {
            return SysServiceManager.GetSysHolidayTypeDTOs();
        }

        /// <summary>
        /// Get all SysHoliday's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysHoliday> GetSysHolidays()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysHoliday.ToList<SysHoliday>();
        }

        public List<SysHoliday> GetSysHolidaysAndTypes()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysHoliday.Include("SysHolidayType").ToList<SysHoliday>();
        }

        #endregion

        #region SysTimeInterval

        public Dictionary<int, string> GetSysTimeIntervalsDict()
        {
            return GetSysTimeIntervals(true).ToDictionary(i => i.SysTimeIntervalId, i => i.Name);
        }

        public List<SysTimeInterval> GetSysTimeIntervals(bool setNames)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<SysTimeInterval> intervals = sysEntitiesReadOnly.SysTimeInterval.OrderBy(i => i.Sort).ToList();

            if (setNames)
            {
                foreach (SysTimeInterval interval in intervals)
                {
                    interval.Name = GetSysTimeIntervalName(interval);
                }
            }

            return intervals;
        }

        public SysTimeInterval GetSysTimeInterval(int sysTimeIntervalId, bool setName)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            SysTimeInterval interval = sysEntitiesReadOnly.SysTimeInterval.FirstOrDefault(i => i.SysTimeIntervalId == sysTimeIntervalId);

            if (interval != null && setName)
                interval.Name = GetSysTimeIntervalName(interval);

            return interval;
        }

        private string GetSysTimeIntervalName(SysTimeInterval interval)
        {
            return GetText(interval.SysTermId, TermGroup.SysTimeInterval);
        }

        public DateRangeDTO GetSysTimeIntervalDateRange(int sysTimeIntervalId, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            SysTimeInterval interval = GetSysTimeInterval(sysTimeIntervalId, false);
            if (interval != null)
                return CalendarUtility.GetTimeInterval((TermGroup_TimeIntervalPeriod)interval.Period, (TermGroup_TimeIntervalStart)interval.Start, interval.StartOffset, (TermGroup_TimeIntervalStop)interval.Stop, interval.StopOffset, date);

            return new DateRangeDTO(date.Value, date.Value);
        }

        #endregion

        #region TimeHalfday

        public List<TimeHalfday> GetTimeHalfdays(int actorCompanyId, bool setTypeName, int? halfDayId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeHalfday.NoTracking();
            return GetTimeHalfdays(entities, actorCompanyId, setTypeName, halfDayId);
        }

        public List<TimeHalfday> GetTimeHalfdays(CompEntities entities, int actorCompanyId, bool setTypeName, int? halfDayId = null)
        {
            List<TimeHalfday> days = (from hd in entities.TimeHalfday
                                        .Include("DayType")
                                        .Include("TimeCodeBreak")
                                      where hd.DayType.ActorCompanyId == actorCompanyId &&
                                      hd.State == (int)SoeEntityState.Active
                                      select hd).ToList();
            if (halfDayId.HasValue)
                days = days.Where(d => d.TimeHalfdayId == halfDayId.Value).ToList();

            if (setTypeName)
            {
                Dictionary<int, string> types = GetTimeHalfdayTypesDict(false);
                foreach (var day in days)
                {
                    if (types.ContainsKey(day.Type))
                        day.TypeName = types[day.Type];
                }
            }

            return days;
        }

        public List<int> GetTimeHalfdayIds(int actorCompanyId, int dayTypeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeHalfday.NoTracking();
            return (from hd in entities.TimeHalfday
                    where hd.DayType.DayTypeId == dayTypeId &&
                    hd.DayType.ActorCompanyId == actorCompanyId &&
                    hd.State == (int)SoeEntityState.Active
                    select hd.TimeHalfdayId).ToList();
        }

        public List<int> GetTimeHalfdayIds(DateTime date)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeScheduleTemplateBlock.NoTracking();
            return (from tb in entities.TimeScheduleTemplateBlock
                    where !tb.TimeScheduleScenarioHeadId.HasValue &&
                    tb.Date == date &&
                    tb.TimeHalfday != null
                    select tb.TimeHalfday.TimeHalfdayId).Distinct().ToList();
        }

        public TimeHalfday GetPrevNextTimeHalfday(int timeHalfdayId, int actorCompanyId, SoeFormMode mode)
        {
            TimeHalfday TimeHalfday = null;
            List<TimeHalfday> TimeHalfdays = GetTimeHalfdays(actorCompanyId, false);

            if (mode == SoeFormMode.Next)
            {
                TimeHalfday = (from hd in TimeHalfdays
                               where hd.TimeHalfdayId > timeHalfdayId &&
                               hd.State == (int)SoeEntityState.Active
                               orderby hd.TimeHalfdayId ascending
                               select hd).FirstOrDefault();
            }
            else
            {
                TimeHalfday = (from hd in TimeHalfdays
                               where hd.TimeHalfdayId < timeHalfdayId &&
                               hd.State == (int)SoeEntityState.Active
                               orderby hd.TimeHalfdayId descending
                               select hd).FirstOrDefault();
            }

            return TimeHalfday;
        }

        public TimeHalfday GetTimeHalfday(int timeHalfdayId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeHalfday.NoTracking();
            return GetTimeHalfday(entities, timeHalfdayId, actorCompanyId);
        }

        public TimeHalfday GetTimeHalfday(CompEntities entities, int timeHalfdayId, int actorCompanyId)
        {
            return (from hd in entities.TimeHalfday
                        .Include("TimeCodeBreak")
                    where hd.TimeHalfdayId == timeHalfdayId &&
                    hd.DayType.ActorCompanyId == actorCompanyId &&
                    hd.State == (int)SoeEntityState.Active
                    select hd).FirstOrDefault();
        }

        public Dictionary<int, string> GetTimeHalfdayTypesDict(bool addEmptyRow)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            if (addEmptyRow)
                result.Add(0, " ");

            result.Add((int)SoeTimeHalfdayType.RelativeStartValue, GetText(4421, "Relativt startvärde i minuter"));
            result.Add((int)SoeTimeHalfdayType.RelativeEndValue, GetText(4422, "Relativt stoppvärde i minuter"));
            result.Add((int)SoeTimeHalfdayType.RelativeStartPercentage, GetText(4424, "Relativt stoppvärde i procent"));
            result.Add((int)SoeTimeHalfdayType.RelativeEndPercentage, GetText(4423, "Relativt startvärde i procent"));
            result.Add((int)SoeTimeHalfdayType.ClockInMinutes, GetText(4425, "Stoppvärde i klockslag"));

            return result;
        }

        public bool TimeHalfdayExists(string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeHalfday.NoTracking();
            return (from hd in entities.TimeHalfday
                    where hd.Name == name &&
                    hd.DayType.ActorCompanyId == actorCompanyId &&
                    hd.State == (int)SoeEntityState.Active
                    select hd).Any();
        }

        public bool TimeHalfdayExists(string name, int dayTypeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeHalfday.NoTracking();
            return (from hd in entities.TimeHalfday
                    where (hd.Name == name || hd.DayTypeId == dayTypeId) &&
                    hd.DayType.ActorCompanyId == actorCompanyId &&
                    hd.State == (int)SoeEntityState.Active
                    select hd).Any();
        }

        public bool TimeHalfdayExists(string name, int dayTypeId, int excludedHalfdayId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeHalfday.NoTracking();
            return (from hd in entities.TimeHalfday
                    where (hd.Name == name || hd.DayTypeId == dayTypeId) &&
                    hd.DayType.ActorCompanyId == actorCompanyId &&
                    hd.TimeHalfdayId != excludedHalfdayId &&
                    hd.State == (int)SoeEntityState.Active
                    select hd).Any();
        }

        public ActionResult SaveTimeHalfday(TimeHalfdayEditDTO input, int actorCompanyId)
        {
            if (input == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeHalfday");

            ActionResult result = new ActionResult();
            int timeHalfdayId = input.TimeHalfdayId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region TimeHalfday

                        TimeHalfday timeHalfday = GetTimeHalfday(entities, timeHalfdayId, actorCompanyId);
                        if (timeHalfday == null)
                        {
                            timeHalfday = new TimeHalfday();
                            SetCreatedProperties(timeHalfday);
                            entities.TimeHalfday.AddObject(timeHalfday);
                        }
                        else
                        {
                            SetModifiedProperties(timeHalfday);
                        }

                        timeHalfday.DayTypeId = input.DayTypeId;
                        timeHalfday.Name = input.Name;
                        timeHalfday.Description = input.Description;
                        timeHalfday.Type = (int)input.Type;
                        timeHalfday.Value = input.Value;
                        timeHalfday.State = (int)input.State;

                        #endregion

                        #region TimeCodeBreak

                        #region Delete

                        foreach (TimeCodeBreak existingTimeCodeBreak in timeHalfday.TimeCodeBreak.ToList())
                        {
                            if (input.TimeCodeBreakIds.IsNullOrEmpty() || !input.TimeCodeBreakIds.Contains(existingTimeCodeBreak.TimeCodeId))
                                timeHalfday.TimeCodeBreak.Remove(existingTimeCodeBreak);
                        }

                        #endregion

                        #region Add/update

                        if (!input.TimeCodeBreakIds.IsNullOrEmpty())
                        {
                            foreach (int timeCodeId in input.TimeCodeBreakIds)
                            {
                                if (timeHalfday.TimeCodeBreak == null)
                                    timeHalfday.TimeCodeBreak = new EntityCollection<TimeCodeBreak>();

                                if (!timeHalfday.TimeCodeBreak.Select(t => t.TimeCodeId).Contains(timeCodeId))
                                {
                                    TimeCodeBreak timeCodeBreak = TimeCodeManager.GetTimeCodeBreak(entities, timeCodeId, actorCompanyId);
                                    if (timeCodeBreak != null)
                                    {
                                        timeHalfday.TimeCodeBreak.Add(timeCodeBreak);
                                    }
                                }
                            }
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities);
                        if (result.Success)
                        {
                            transaction.Complete();
                            timeHalfdayId = timeHalfday.TimeHalfdayId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                        result.IntegerValue = timeHalfdayId;

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult AddTimeHalfday(TimeHalfday timeHalfday, int dayTypeId, int actorCompanyId)
        {
            if (timeHalfday == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeHalfday");

            using (CompEntities entities = new CompEntities())
            {
                timeHalfday.DayType = GetDayType(entities, dayTypeId, actorCompanyId);
                if (timeHalfday.DayType == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "DayType");

                return AddEntityItem(entities, timeHalfday, "TimeHalfday");
            }
        }

        public ActionResult UpdateTimeHalfday(TimeHalfday TimeHalfday, int dayTypeId, int actorCompanyId)
        {
            if (TimeHalfday == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeHalfday");

            using (CompEntities entities = new CompEntities())
            {
                TimeHalfday originalTimeHalfday = GetTimeHalfday(entities, TimeHalfday.TimeHalfdayId, actorCompanyId);
                if (originalTimeHalfday == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeHalfday");

                //Only update DayType if it has changed
                if (originalTimeHalfday.DayTypeId != dayTypeId)
                    originalTimeHalfday.DayTypeId = dayTypeId;

                return UpdateEntityItem(entities, originalTimeHalfday, TimeHalfday, "TimeHalfday");
            }
        }

        public ActionResult DeleteTimeHalfday(TimeHalfday TimeHalfday, int actorCompanyId)
        {
            if (TimeHalfday == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "TimeHalfday");

            using (CompEntities entities = new CompEntities())
            {
                TimeHalfday originalTimeHalfday = GetTimeHalfday(entities, TimeHalfday.TimeHalfdayId, actorCompanyId);
                if (originalTimeHalfday == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeHalfday");

                return ChangeEntityState(entities, originalTimeHalfday, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeleteTimeHalfday(int timeHalfdayId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeHalfday originalTimeHalfday = GetTimeHalfday(entities, timeHalfdayId, actorCompanyId);
                if (originalTimeHalfday == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeHalfday");

                return ChangeEntityState(entities, originalTimeHalfday, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult SaveTimeHalfdayBreakReductions(Collection<FormIntervalEntryItem> breakItems, int timeHalfdayId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeHalfday timeHalfday = GetTimeHalfday(entities, timeHalfdayId, actorCompanyId);
                if (timeHalfday == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeHalfday");

                if (timeHalfday.TimeCodeBreak == null)
                    timeHalfday.TimeCodeBreak = new EntityCollection<TimeCodeBreak>();

                foreach (var item in breakItems)
                {
                    int itemId = Convert.ToInt32(item.From);
                    if (!timeHalfday.TimeCodeBreak.Any(i => i.TimeCodeId == itemId))
                    {
                        TimeCodeBreak timeCodeBreak = TimeCodeManager.GetTimeCodeBreak(entities, Convert.ToInt32(itemId), actorCompanyId);
                        if (timeCodeBreak != null)
                            timeHalfday.TimeCodeBreak.Add(timeCodeBreak);
                    }
                }

                List<int> timeCodeIds = timeHalfday.TimeCodeBreak.Select(i => i.TimeCodeId).ToList();
                foreach (int timeCodeId in timeCodeIds)
                {
                    if (!breakItems.Any(i => i.From == timeCodeId.ToString()))
                    {
                        TimeCodeBreak timeCodeBreak = TimeCodeManager.GetTimeCodeBreak(entities, timeCodeId, actorCompanyId);
                        if (timeCodeBreak != null)
                            timeHalfday.TimeCodeBreak.Remove(timeCodeBreak);
                    }
                }

                return UpdateEntityItem(entities, timeHalfday, timeHalfday, "TimeHalfday");
            }
        }

        #endregion

        #region OpeningHours

        public List<OpeningHours> GetOpeningHoursForCompany(int actorCompanyId, DateTime? fromDate = null, DateTime? toDate = null, int? openingHoursId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.OpeningHours.NoTracking();
            return GetOpeningHoursForCompany(entities, actorCompanyId, fromDate, toDate, openingHoursId);
        }

        public List<OpeningHours> GetOpeningHoursForCompany(CompEntities entities, int actorCompanyId, DateTime? fromDate = null, DateTime? toDate = null, int? openingHoursId = null)
        {
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);

            IQueryable<OpeningHours> query = (from oh in entities.OpeningHours
                                              where oh.ActorCompanyId == actorCompanyId &&
                                              oh.State == (int)SoeEntityState.Active
                                              select oh);

            if (useAccountHierarchy)
            {
                query = query.Include("Account");

                var accountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(entities, actorCompanyId, base.RoleId, base.UserId, DateTime.Today, DateTime.Today, null);

                if (!accountIds.IsNullOrEmpty())
                    query = query.Where(t => !t.AccountId.HasValue || accountIds.Contains(t.AccountId.Value));
                else
                    query = query.Where(t => !t.AccountId.HasValue);
            }

            if (fromDate.HasValue)
                query = query.Where(oh => !oh.SpecificDate.HasValue || oh.SpecificDate.Value >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(oh => (!oh.SpecificDate.HasValue || oh.SpecificDate.Value <= toDate.Value) && (!oh.FromDate.HasValue || oh.FromDate.Value <= toDate.Value));

            if (openingHoursId.HasValue)
                query = query.Where(oh => oh.OpeningHoursId == openingHoursId.Value);

            List<OpeningHours> allHours = query.ToList();

            if (fromDate.HasValue)
            {
                // If date interaval is specified, return only hours relevant for interval
                List<OpeningHours> hours = new List<OpeningHours>();

                hours.AddRange(allHours.Where(oh => oh.SpecificDate.HasValue));

                var groupedHours = allHours.Where(oh => !oh.SpecificDate.HasValue).GroupBy(oh => new { oh.StandardWeekDay }).Select(gh => new { gh.Key.StandardWeekDay, Hours = gh.ToList() });
                foreach (var group in groupedHours)
                {
                    if (group.Hours.Count == 1 || !group.Hours.Any(c => c.FromDate.HasValue))
                    {
                        // Only one, or all are without date
                        hours.AddRange(group.Hours);
                    }
                    else
                    {
                        // Add all that starts after from date (starts after to date is already removed in main query)
                        hours.AddRange(group.Hours.Where(c => c.FromDate.HasValue && c.FromDate.Value >= fromDate.Value));

                        // If none starts exactly on from date, we need to add the one starting before from date also
                        bool startsOnFromDate = group.Hours.Any(c => c.FromDate.HasValue && c.FromDate.Value == fromDate.Value);
                        if (!startsOnFromDate)
                        {
                            OpeningHours hour = group.Hours.Where(h => h.FromDate.HasValue && h.FromDate.Value < fromDate.Value).OrderByDescending(c => c.FromDate).FirstOrDefault();
                            if (hour != null)
                                hours.Add(hour);
                            else
                                hours.AddRange(group.Hours.Where(c => !c.FromDate.HasValue));
                        }
                    }
                }

                return hours.OrderBy(h => h.Name).ThenBy(h => h.FromDate).ToList();
            }
            else
            {
                return allHours.OrderBy(h => h.Name).ThenBy(h => h.FromDate).ToList();
            }
        }

        public OpeningHours GetOpeningHours(int openingHoursId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.OpeningHours.NoTracking();
            return GetOpeningHours(entities, openingHoursId, actorCompanyId);
        }

        public OpeningHours GetOpeningHours(CompEntities entities, int openingHoursId, int actorCompanyId)
        {
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);

            IQueryable<OpeningHours> query = (from oh in entities.OpeningHours
                                              where oh.ActorCompanyId == actorCompanyId &&
                                              oh.OpeningHoursId == openingHoursId &&
                                              oh.State == (int)SoeEntityState.Active
                                              select oh);

            if (useAccountHierarchy)
            {
                query = query.Include("Account");

                var accountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(entities, actorCompanyId, base.RoleId, base.UserId, DateTime.Today, DateTime.Today, null);

                if (!accountIds.IsNullOrEmpty())
                    query = query.Where(t => !t.AccountId.HasValue || accountIds.Contains(t.AccountId.Value));
                else
                    query = query.Where(t => !t.AccountId.HasValue);
            }

            return query.FirstOrDefault();
        }

        public ActionResult SaveOpeningHours(OpeningHoursDTO openingHoursInput, int actorCompanyId)
        {
            if (openingHoursInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "OpeningHours");

            using (CompEntities entities = new CompEntities())
            {
                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                // Get original
                OpeningHours openingHours = GetOpeningHours(entities, openingHoursInput.OpeningHoursId, actorCompanyId);
                if (openingHours == null)
                {
                    openingHours = new OpeningHours()
                    {
                        Company = company,
                    };
                    SetCreatedProperties(openingHours);
                    entities.OpeningHours.AddObject(openingHours);
                }
                else
                {
                    SetModifiedProperties(openingHours);
                }

                openingHours.Name = openingHoursInput.Name;
                openingHours.Description = StringUtility.NullToEmpty(openingHoursInput.Description);
                openingHours.StandardWeekDay = openingHoursInput.StandardWeekDay;
                openingHours.SpecificDate = openingHoursInput.SpecificDate;
                openingHours.OpeningTime = openingHoursInput.OpeningTime.HasValue ? CalendarUtility.ClearSeconds(openingHoursInput.OpeningTime.Value) : (DateTime?)null;
                openingHours.ClosingTime = openingHoursInput.ClosingTime.HasValue ? CalendarUtility.ClearSeconds(openingHoursInput.ClosingTime.Value) : (DateTime?)null;
                openingHours.FromDate = openingHoursInput.FromDate;
                openingHours.AccountId = openingHoursInput.AccountId.HasValue && openingHoursInput.AccountId != 0 ? openingHoursInput.AccountId : (int?)null;

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                result.IntegerValue = openingHours.OpeningHoursId;
                return result;
            }
        }

        public ActionResult DeleteOpeningHours(int openingHoursId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                OpeningHours openingHours = GetOpeningHours(entities, openingHoursId, actorCompanyId);
                if (openingHours == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "OpeningHours");

                if (!openingHours.DistributionCodeHead.IsLoaded)
                    openingHours.DistributionCodeHead.Load();

                if (openingHours.DistributionCodeHead != null && openingHours.DistributionCodeHead.Any())
                    return new ActionResult((int)ActionResultDelete.EntityInUse, GetText(11653, 1, "Öppettiden används i fördelningskoder för budget och kan därför inte tas bort."));

                return DeleteEntityItem(entities, openingHours);
            }
        }

        public Dictionary<int, string> GetOpeningHoursDict(int actorCompanyId, bool addEmptyRow, bool includeDateInName)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<OpeningHours> openinghours = GetOpeningHoursForCompany(actorCompanyId);
            foreach (OpeningHours openinghour in openinghours)
            {
                if (!dict.ContainsKey(openinghour.OpeningHoursId))
                {
                    string name = openinghour.Name;
                    if (includeDateInName && openinghour.FromDate.HasValue)
                        name += String.Format(" ({0})", openinghour.FromDate.Value.ToShortDateString());
                    dict.Add(openinghour.OpeningHoursId, name);
                }
            }

            return dict;
        }

        #endregion

        #region RecurrenceInterval

        public string GetRecurrenceIntervalText(string recurrenceInterval)
        {
            string text = string.Empty;

            Dictionary<string, List<int>> parts = SchedulerUtility.ParseCrontabExpression(recurrenceInterval);
            List<int> minutes = parts[SchedulerUtility.CRONTAB_MINUTES];
            List<int> hours = parts[SchedulerUtility.CRONTAB_HOURS];
            List<int> days = parts[SchedulerUtility.CRONTAB_DAYS];
            List<int> months = parts[SchedulerUtility.CRONTAB_MONTHS];
            List<int> weekdays = parts[SchedulerUtility.CRONTAB_WEEKDAYS];

            if (hours.Any())
                text += String.Format("{0}: {1}", GetText(355, 1000, "Timme"), hours.JoinToString(", "));
            if (minutes.Any())
                text += String.Format(" {0}: {1}", GetText(353, 1000, "Minut"), minutes.JoinToString(", "));
            if (days.Any())
                text += String.Format(" {0}: {1}", GetText(357, 1000, "Dag"), days.JoinToString(", "));

            if (months.Any())
            {
                List<string> monthNames = new List<string>();
                months.ForEach(m => monthNames.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)));
                text += String.Format(" {0}: {1}", GetText(361, 1000, "Månad"), monthNames.JoinToString(", "));
            }

            if (weekdays.Any())
            {
                List<string> weekdayNames = new List<string>();
                weekdays.ForEach(w => weekdayNames.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetDayName((DayOfWeek)w)));
                text += String.Format(" {0}: {1}", GetText(114, 1002, "Veckodag"), weekdayNames.JoinToString(", "));
            }

            return text;
        }

        #endregion
    }
}

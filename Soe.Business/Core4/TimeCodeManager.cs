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
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class TimeCodeManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimeCodeManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region TimeCode

        public List<TimeCode> GetTimeCodes(int actorCompanyId, SoeTimeCodeType timeCodeType, bool onlyActive = true, bool loadPayrollProducts = false, bool onlyWithInvoiceProduct = false, int? timeCodeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCodes(entities, actorCompanyId, timeCodeType, onlyActive, loadPayrollProducts, onlyWithInvoiceProduct, timeCodeId);
        }

        public List<TimeCode> GetTimeCodes(CompEntities entities, int actorCompanyId, SoeTimeCodeType timeCodeType, bool onlyActive = true, bool loadPayrollProducts = false, bool onlyWithInvoiceProduct = false, int? timeCodeId = null)
        {
            List<TimeCode> timeCodes = null;

            if (timeCodeType == SoeTimeCodeType.None)
            {
                #region None
                if (loadPayrollProducts)
                {
                    timeCodes = (from tc in entities.TimeCode
                                   .Include("TimeCodePayrollProduct.PayrollProduct")
                                 where tc.ActorCompanyId == actorCompanyId &&
                                 tc.State != (int)SoeEntityState.Deleted
                                 orderby tc.Code
                                 select tc).ToList();
                }
                else
                {
                    timeCodes = (from tc in entities.TimeCode
                                 where tc.ActorCompanyId == actorCompanyId &&
                                 tc.State != (int)SoeEntityState.Deleted
                                 orderby tc.Code
                                 select tc).ToList();
                }


                #endregion
            }
            else if (timeCodeType == SoeTimeCodeType.Break)
            {
                #region Break

                timeCodes = (from tc in entities.TimeCode.OfType<TimeCodeBreak>()
                                .Include("TimeCodeBreakGroup")
                                .Include("EmployeeGroupsForBreak")
                             where tc.ActorCompanyId == actorCompanyId &&
                             tc.Type == (int)SoeTimeCodeType.Break &&
                             tc.State != (int)SoeEntityState.Deleted
                             orderby tc.Code
                             select tc).ToList<TimeCode>();

                #endregion
            }
            else if (timeCodeType == SoeTimeCodeType.AdditionDeduction)
            {
                #region AdditionAndDeduction

                timeCodes = (from tc in entities.TimeCode
                                 .Include("TimeCodeInvoiceProduct.InvoiceProduct")
                                 .Include("TimeCodePayrollProduct.PayrollProduct")
                             where tc.ActorCompanyId == actorCompanyId &&
                             tc.Type == (int)SoeTimeCodeType.AdditionDeduction &&
                             tc.State != (int)SoeEntityState.Deleted
                             orderby tc.Type, tc.Code
                             select tc).ToList();

                #endregion
            }
            else if (timeCodeType == SoeTimeCodeType.WorkAndAbsense)
            {
                #region WorkAndAbsence (abstract type)

                if (loadPayrollProducts)
                {
                    timeCodes = (from tc in entities.TimeCode
                                    .Include("TimeCodePayrollProduct.PayrollProduct")
                                 where tc.ActorCompanyId == actorCompanyId &&
                                 (tc.Type == (int)SoeTimeCodeType.Work || tc.Type == (int)SoeTimeCodeType.Absense) &&
                                 tc.State != (int)SoeEntityState.Deleted
                                 orderby tc.Type, tc.Code
                                 select tc).ToList();
                }
                else
                {
                    timeCodes = (from tc in entities.TimeCode
                                 where tc.ActorCompanyId == actorCompanyId &&
                                 (tc.Type == (int)SoeTimeCodeType.Work || tc.Type == (int)SoeTimeCodeType.Absense) &&
                                 tc.State != (int)SoeEntityState.Deleted
                                 orderby tc.Type, tc.Code
                                 select tc).ToList();
                }

                #endregion
            }
            else if (timeCodeType == SoeTimeCodeType.WorkAndAbsenseAndAdditionDeduction)
            {
                #region WorkAndAbsenseAndAdditionDeduction (abstract type)

                timeCodes = (from tc in entities.TimeCode
                             where tc.ActorCompanyId == actorCompanyId &&
                             (tc.Type == (int)SoeTimeCodeType.Work || tc.Type == (int)SoeTimeCodeType.Absense || tc.Type == (int)SoeTimeCodeType.AdditionDeduction) &&
                             tc.State != (int)SoeEntityState.Deleted
                             orderby tc.Type, tc.Code
                             select tc).ToList();

                #endregion
            }
            else if (timeCodeType == SoeTimeCodeType.WorkAndMaterial)
            {
                #region WorkAndMaterial (abstract type)

                timeCodes = (from tc in entities.TimeCode
                             where tc.ActorCompanyId == actorCompanyId &&
                             (tc.Type == (int)SoeTimeCodeType.Work || tc.Type == (int)SoeTimeCodeType.Material) &&
                             tc.State != (int)SoeEntityState.Deleted
                             orderby tc.Type, tc.Code
                             select tc).ToList();

                #endregion
            }
            else
            {
                #region Others

                if (loadPayrollProducts)
                {
                    timeCodes = (from tc in entities.TimeCode
                                    .Include("TimeCodePayrollProduct.PayrollProduct")
                                 where tc.ActorCompanyId == actorCompanyId &&
                                 tc.Type == (int)timeCodeType &&
                                 tc.State != (int)SoeEntityState.Deleted
                                 orderby tc.Code
                                 select tc).ToList();
                }
                else if (onlyWithInvoiceProduct)
                {
                    timeCodes = (from tc in entities.TimeCode
                                 where tc.ActorCompanyId == actorCompanyId &&
                                 tc.TimeCodeInvoiceProduct.Any() &&
                                 tc.Type == (int)timeCodeType &&
                                 tc.State != (int)SoeEntityState.Deleted
                                 orderby tc.Code
                                 select tc).ToList();
                }
                else
                {
                    timeCodes = (from tc in entities.TimeCode
                                 where tc.ActorCompanyId == actorCompanyId &&
                                 tc.Type == (int)timeCodeType &&
                                 tc.State != (int)SoeEntityState.Deleted
                                 orderby tc.Code
                                 select tc).ToList();
                }

                #endregion
            }

            if (timeCodeId.HasValue)
                timeCodes = timeCodes.Where(t => t.TimeCodeId == timeCodeId.Value).ToList();

            if (onlyActive)
                timeCodes = timeCodes.Where(t => t.State == (int)SoeEntityState.Active).ToList();

            return timeCodes;
        }

        public List<TimeCode> GetTimeCodes(int actorCompanyId, bool loadPayrollProducts, bool loadInvoiceProducts)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCodes(entities, actorCompanyId, loadPayrollProducts, loadInvoiceProducts);
        }

        public List<TimeCode> GetTimeCodes(CompEntities entities, int actorCompanyId, bool loadPayrollProducts, bool loadInvoiceProducts)
        {
            var query = from tc in entities.TimeCode
                        where tc.ActorCompanyId == actorCompanyId &&
                        tc.State == (int)SoeEntityState.Active && tc.State != (int)SoeEntityState.Deleted
                        select tc;

            if (loadInvoiceProducts)
                query = query.Include("TimeCodeInvoiceProduct");
            if (loadPayrollProducts)
                query = query.Include("TimeCodePayrollProduct");

            List<TimeCode> timeCodes = query.ToList();
            return timeCodes.OrderBy(t => t.Name).ToList();
        }

        public List<TimeCode> GetTimeCodes(int actorCompanyId, params int[] timeCodeTypes)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCodes(entities, actorCompanyId, timeCodeTypes);
        }

        public List<TimeCode> GetTimeCodes(CompEntities entities, int actorCompanyId, params int[] timeCodeTypes)
        {
            var query = from tc in entities.TimeCode
                        where tc.ActorCompanyId == actorCompanyId &&
                        tc.State == (int)SoeEntityState.Active
                        select tc;

            if (!timeCodeTypes.IsNullOrEmpty())
                query = query.Where(t => timeCodeTypes.Contains(t.Type));

            List<TimeCode> timeCodes = query.ToList();
            return timeCodes.OrderBy(t => t.Name).ToList();
        }

        public List<TimeCode> GetTimeCodesForEmployeeGroup(int actorCompanyId, int employeeGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCodesForEmployeeGroup(entities, actorCompanyId, employeeGroupId);
        }

        public List<TimeCode> GetTimeCodesForEmployeeGroup(CompEntities entities, int actorCompanyId, int employeeGroupId)
        {
            return (from tc in entities.TimeCode
                    where tc.ActorCompanyId == actorCompanyId &&
                    tc.State != (int)SoeEntityState.Deleted &&
                    tc.EmployeeGroups.Any(e => e.EmployeeGroupId == employeeGroupId)
                    orderby tc.Code
                    select tc).ToList();
        }

        public List<TimeCode> GetTimeCodesBySearch(int actorCompanyId, string search, int no)
        {
            //New function for Quicksearch in Time module
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            var timeCodes = (from tc in entities.TimeCode
                             where tc.Company.ActorCompanyId == actorCompanyId &&
                             (tc.Code.ToLower().Contains(search.ToLower()) || tc.Name.ToLower().Contains(search.ToLower())) &&
                             tc.State == (int)SoeEntityState.Active
                             orderby tc.Code
                             select tc).Take(no).ToList<TimeCode>();

            return timeCodes;
        }

        public Dictionary<int, string> GetTimeCodesDict(int actorCompanyId, bool addEmptyRow, bool concatCodeAndName, bool includeType, params int[] timeCodeTypes)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var timeCodes = GetTimeCodes(actorCompanyId, timeCodeTypes);
            foreach (var timeCode in timeCodes.OrderBy(i => i.Name))
            {
                string name = concatCodeAndName ? timeCode.Code + " - " + timeCode.Name : timeCode.Name;

                if (includeType)
                {
                    switch (timeCode.Type)
                    {
                        case (int)SoeTimeCodeType.Work:
                            name = String.Format("{0} ({1})", name, GetText(5988, "Närvaro"));
                            break;
                        case (int)SoeTimeCodeType.Absense:
                            name = String.Format("{0} ({1})", name, GetText(5989, "Frånvaro"));
                            break;
                        case (int)SoeTimeCodeType.Break:
                            name = String.Format("{0} ({1})", name, GetText(5990, "Rast"));
                            break;
                        case (int)SoeTimeCodeType.AdditionDeduction:
                            name = String.Format("{0} ({1}/{2})", name, GetText(5991, "Tillägg"), GetText(5992, "Avdrag"));
                            break;
                        case (int)SoeTimeCodeType.Material:
                            name = String.Format("{0} ({1})", name, GetText(5993, "Materialkod"));
                            break;
                    }
                }

                dict.Add(timeCode.TimeCodeId, name);
            }

            return dict.Sort();
        }

        public Dictionary<int, string> GetTimeCodesDict(int actorCompanyId, bool addEmptyRow, bool concatCodeAndName, SoeTimeCodeType timeCodeType)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var timeCodes = GetTimeCodes(actorCompanyId, timeCodeType);
            foreach (var timeCode in timeCodes.OrderBy(i => i.Name))
            {
                dict.Add(timeCode.TimeCodeId, concatCodeAndName ? timeCode.Code + " - " + timeCode.Name : timeCode.Name);
            }

            return dict;
        }

        public Dictionary<int, string> GetTimeCodesDictDiscardState(int actorCompanyId, List<int> ids)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeDeviationCause.NoTracking();
            return GetTimeCodesDictDiscardState(entities, actorCompanyId, ids);
        }

        public Dictionary<int, string> GetTimeCodesDictDiscardState(CompEntities entities, int actorCompanyId, IEnumerable<int> ids)
        {
            if (ids.IsNullOrEmpty())
                return new Dictionary<int, string>();

            ids = ids.Distinct();

            var timeDeviationCauses = (from t in entities.TimeCode
                                       where t.ActorCompanyId == actorCompanyId &&
                                       ids.Contains(t.TimeCodeId)
                                       select t).ToList();

            return timeDeviationCauses.ToDictionary(k => k.TimeCodeId, v => v.Name);
        }

        public T GetTimeCode<T>(int actorCompanyId, int timeCodeId) where T : TimeCode
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeCode.NoTracking();
            return GetTimeCode<T>(entitiesReadOnly, actorCompanyId, timeCodeId);
        }

        public T GetTimeCode<T>(CompEntities entities, int actorCompanyId, int timeCodeId) where T : TimeCode
        {
            return (from tc in entities.TimeCode.OfType<T>()
                    where tc.TimeCodeId == timeCodeId &&
                    tc.ActorCompanyId == actorCompanyId &&
                    tc.State == (int)SoeEntityState.Active
                    select tc).FirstOrDefault<T>();
        }

        public TimeCode GetTimeCode(int timeCodeId, int actorCompanyId, bool onlyActive, bool loadInvoiceProducts = false, bool loadPayrollProducts = false, bool loadTimeCodeRules = false, bool loadTimeCodeDeviationCauses = false, bool loadEmployeeGroups = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCode(entities, timeCodeId, actorCompanyId, onlyActive, loadInvoiceProducts, loadPayrollProducts, loadTimeCodeRules, loadTimeCodeDeviationCauses, loadEmployeeGroups);
        }

        public TimeCode GetTimeCode(CompEntities entities, int timeCodeId, int actorCompanyId, bool onlyActive, bool loadInvoiceProducts = false, bool loadPayrollProducts = false, bool loadTimeCodeRules = false, bool loadTimeCodeDeviationCauses = false, bool loadEmployeeGroups = false)
        {
            IQueryable<TimeCode> query = entities.TimeCode;
            if (loadInvoiceProducts)
                query = query.Include("TimeCodeInvoiceProduct");
            if (loadPayrollProducts)
                query = query.Include("TimeCodePayrollProduct");
            if (loadTimeCodeRules)
                query = query.Include("TimeCodeRule");

            TimeCode timeCode = (from tc in query
                                 where tc.TimeCodeId == timeCodeId &&
                                 tc.ActorCompanyId == actorCompanyId &&
                                 (!onlyActive || tc.State == (int)SoeEntityState.Active)
                                 select tc).FirstOrDefault();

            if (timeCode is TimeCodeBreak timeCodeBreak)
            {
                if (loadTimeCodeDeviationCauses)
                {
                    if (!timeCodeBreak.TimeCodeBreakTimeCodeDeviationCauses.IsLoaded)
                        timeCodeBreak.TimeCodeBreakTimeCodeDeviationCauses.Load();

                    foreach (var mapping in timeCodeBreak.TimeCodeBreakTimeCodeDeviationCauses)
                    {
                        mapping.TimeCodeReference.Load();
                        mapping.TimeDeviationCauseReference.Load();
                    }
                }

                if (loadEmployeeGroups && !timeCodeBreak.EmployeeGroupsForBreak.IsLoaded)
                    timeCodeBreak.EmployeeGroupsForBreak.Load();
            }

            return timeCode;
        }

        public TimeCode GetTimeCode(string name, string code, int type, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCode(entities, name, code, type, actorCompanyId);
        }

        public TimeCode GetTimeCode(CompEntities entities, string name, string code, int type, int actorCompanyId)
        {
            return (from tc in entities.TimeCode
                    where tc.Name.ToLower() == name.ToLower() &&
                    tc.Type == type &&
                    tc.Code == code &&
                    tc.ActorCompanyId == actorCompanyId &&
                    tc.State == (int)SoeEntityState.Active
                    select tc).FirstOrDefault();
        }

        public TimeCode GetTimeCode(string name, int type, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCode(entities, name, type, actorCompanyId);
        }

        public TimeCode GetTimeCode(CompEntities entities, string name, int type, int actorCompanyId)
        {
            return (from tc in entities.TimeCode
                    where tc.Name.ToLower() == name.ToLower() &&
                    tc.Type == type &&
                    tc.ActorCompanyId == actorCompanyId &&
                    tc.State == (int)SoeEntityState.Active
                    select tc).FirstOrDefault();
        }

        public T GetPrevNextTimeCodeWork<T>(int timeCodeId, SoeFormMode mode) where T : TimeCode
        {
            T timeCode = null;
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (mode == SoeFormMode.Next)
            {
                timeCode = (from tc in entitiesReadOnly.TimeCode.OfType<T>()
                            where tc.TimeCodeId > timeCodeId &&
                            tc.State == (int)SoeEntityState.Active
                            orderby tc.TimeCodeId ascending
                            select tc).FirstOrDefault<T>();
            }
            else
            {
                timeCode = (from tc in entitiesReadOnly.TimeCode.OfType<T>()
                            where tc.TimeCodeId < timeCodeId &&
                            tc.State == (int)SoeEntityState.Active
                            orderby tc.TimeCodeId descending
                            select tc).FirstOrDefault<T>();
            }

            return timeCode;
        }

        public TimeCode GetTimeCodeWithProducts(CompEntities entities, int timeCodeId, int actorCompanyId)
        {
            return (from tc in entities.TimeCode
                        .Include("TimeCodeInvoiceProduct")
                        .Include("TimeCodePayrollProduct")
                        .Include("TimeCodeInvoiceProduct.InvoiceProduct")
                        .Include("TimeCodePayrollProduct.PayrollProduct")
                    where tc.TimeCodeId == timeCodeId &&
                    tc.ActorCompanyId == actorCompanyId
                    select tc).FirstOrDefault();
        }

        public TimeCode GetTimeCodeWithPayrollProducts(CompEntities entities, int timeCodeId, int actorCompanyId, bool? active = null)
        {
            var query = from tc in entities.TimeCode
                            .Include("TimeCodePayrollProduct.PayrollProduct")
                        where tc.TimeCodeId == timeCodeId &&
                        tc.ActorCompanyId == actorCompanyId
                        select tc;

            return query.FilterActive(active).FirstOrDefault();
        }

        public TimeCode GetTimeCodeWithInvoiceProduct(int timeCodeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCodeWithInvoiceProduct(entities, timeCodeId, actorCompanyId);
        }

        public TimeCode GetTimeCodeWithInvoiceProduct(CompEntities entities, int timeCodeId, int actorCompanyId)
        {
            return (from tc in entities.TimeCode
                        .Include("TimeCodeInvoiceProduct")
                        .Include("TimeCodeInvoiceProduct.InvoiceProduct")
                    where tc.TimeCodeId == timeCodeId &&
                    tc.ActorCompanyId == actorCompanyId
                    select tc).FirstOrDefault<TimeCode>();
        }

        public int GetDefaultTimeCodeId(int actorCompanyId, int employeeId, DateTime? date = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetDefaultTimeCodeId(entities, actorCompanyId, employeeId, date);
        }

        public int GetDefaultTimeCodeId(CompEntities entities, int actorCompanyId, int employeeId, DateTime? date = null)
        {
            Employee employee = EmployeeManager.GetEmployeeIgnoreState(entities, actorCompanyId, employeeId, loadEmployment: true);
            int companySettingTimeCodeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);
            return GetDefaultTimeCodeId(employee, companySettingTimeCodeId, date);
        }

        public int GetDefaultTimeCodeId(Employee employee, int companySettingTimeCodeId, DateTime? date = null)
        {
            int timeCodeId = 0;

            if (employee != null)
            {
                //1. Try get TimeCodeId from Employee
                if (employee.TimeCodeId.HasValue)
                    timeCodeId = employee.TimeCodeId.Value;

                //2. Try get TimeCodeId from EmployeeGroup
                if (timeCodeId == 0)
                {
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(employee.ActorCompanyId)));
                    if (employeeGroup != null && employeeGroup.TimeCodeId.HasValue)
                        timeCodeId = employeeGroup.TimeCodeId.Value;
                }
            }

            //3. Get from Company setting
            if (timeCodeId == 0)
                timeCodeId = companySettingTimeCodeId;

            return timeCodeId;
        }

        public ActionResult IsOkToDeleteTimeCode(CompEntities entities, int timeCodeId)
        {
            ActionResult result = new ActionResult(true);
            int actorCompanyId = base.ActorCompanyId;

            //Employee
            if (entities.Employee.Any(i => i.TimeCodeId.HasValue && i.TimeCodeId.Value == timeCodeId && i.ActorCompanyId == actorCompanyId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasEmployees, GetText(5018, "Anställda"));
            //EmployeeGroup
            else if (entities.EmployeeGroup.Any(i => i.TimeCodeId.HasValue && i.TimeCodeId.Value == timeCodeId && i.ActorCompanyId == actorCompanyId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasEmployeeGroups, GetText(4408, "Tidavtal"));
            else if (entities.EmployeeGroupTimeDeviationCauseTimeCode.Any(i => i.TimeCodeId == timeCodeId))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasEmployeeGroupTimeDeviationMappings, GetText(4408, "Tidavtal"));
            //TimeScheduleTemplateBlock
            else if (entities.TimeScheduleTemplateBlock.Any(i => i.TimeCodeId == timeCodeId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasTemplateBlocks, GetText(5004, "Schema"));
            //TimeBlock
            else if (entities.TimeBlock.Any(i => i.State == (int)SoeEntityState.Active && i.TimeCode.Any(tc => tc.TimeCodeId == timeCodeId)))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasTimeBlocks, GetText(12120, "Tider"));
            //TimeCodeRanking
            else if (entities.TimeCodeRanking.Any(i => (i.LeftTimeCodeId == timeCodeId || i.RightTimeCodeId == timeCodeId) && i.ActorCompanyId == actorCompanyId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasTimeCodeRanking, GetText(8957, "Tidkodsviktning"));
            //TimeDeviationCause
            else if (entities.TimeDeviationCause.Any(i => i.TimeCodeId == timeCodeId && i.ActorCompanyId == actorCompanyId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasDeviationCauses, GetText(4304, "Avvikelseorsaker"));
            else if (entities.TimeCodeBreakTimeCodeDeviationCause.Any(i => i.TimeCode.TimeCodeId == timeCodeId))
                result = new ActionResult((int)ActionResultDelete.TimeCodeBreakHasTimeCodeDeviationCauses, GetText(4304, "Avvikelseorsaker"));
            //TimeAccumulator
            else if (entities.TimeAccumulatorTimeCode.Any(i => i.TimeCodeId == timeCodeId))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasTimeAccumulators, GetText(12122, "Saldon"));
            else if (entities.TimeAccumulatorEmployeeGroupRule.Any(i => i.MinTimeCodeId == timeCodeId || i.MaxTimeCodeId == timeCodeId))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasTimeAccumulatorEmployeeGroupRuleMappings, GetText(12122, "Saldon"));
            //TimeCodeTransaction
            else if (entities.TimeCodeTransaction.Any(i => i.TimeCodeId == timeCodeId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasTransactions, GetText(12121, "Transaktioner"));
            //TimeAbsenceRuleHead
            else if (entities.TimeAbsenceRuleHead.Any(i => i.TimeCodeId == timeCodeId && i.ActorCompanyId == actorCompanyId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasTimeAbsenceRuleHeads, GetText(12118, "Frånvaroregler"));
            //TimeRule
            else if (entities.TimeRule.Where(i => i.TimeCodeId == timeCodeId && i.ActorCompanyId == actorCompanyId && i.State == (int)SoeEntityState.Active).Any())
                result = new ActionResult((int)ActionResultDelete.TimeCodeHasTimeRules, GetText(12119, "Tidsregler"));

            if (!result.Success)
                result.ErrorMessage = $"{GetText(5664, "Kontrollera att den inte är kopplad till")} {result.ErrorMessage.ToLower()}. ({GetText(5654, "Felkod:")}{result.ErrorNumber})";

            return result;
        }

        public ActionResult SaveTimeCode(TimeCodeSaveDTO timeCodeInput, int actorCompanyId, int roleId)
        {
            if (timeCodeInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeCodeDTO");

            int defualtTimeCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);
            if (timeCodeInput.State == SoeEntityState.Inactive && timeCodeInput.TimeCodeId == defualtTimeCodeId)
                return new ActionResult((int)ActionResultSave.EntityNotUpdated, GetText(12234, "Tidkoden är satt som standard tidkod under företagsinställningar och kan inte inaktiveras"));

            // Default result is successful
            ActionResult result = new ActionResult();

            int timeCodeId = timeCodeInput.TimeCodeId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        bool changeAffectTerminal = false;
                        WebPubSubMessageAction action = WebPubSubMessageAction.Undefined;

                        #region TimeCode

                        List<TimeCodeRuleDTO> timeCodeRuleInputs = null;

                        TimeCode timeCode = GetTimeCode(entities, timeCodeId, actorCompanyId, false, true, true, true, true, true);
                        if (timeCode == null)
                        {
                            #region Add

                            switch (timeCodeInput.Type)
                            {
                                case SoeTimeCodeType.Absense:
                                    timeCode = new TimeCodeAbsense();
                                    break;
                                case SoeTimeCodeType.AdditionDeduction:
                                    timeCode = new TimeCodeAdditionDeduction();
                                    break;
                                case SoeTimeCodeType.Break:
                                    timeCode = new TimeCodeBreak();
                                    break;
                                case SoeTimeCodeType.Material:
                                    timeCode = new TimeCodeMaterial();
                                    break;
                                case SoeTimeCodeType.Work:
                                    timeCode = new TimeCodeWork();
                                    break;
                            }

                            timeCode.Type = (int)timeCodeInput.Type;
                            timeCode.ActorCompanyId = actorCompanyId;

                            SetCreatedProperties(timeCode);
                            entities.TimeCode.AddObject(timeCode);

                            if (timeCodeInput.Type == SoeTimeCodeType.AdditionDeduction && timeCodeInput.ShowInTerminal)
                            {
                                changeAffectTerminal = true;
                                action = WebPubSubMessageAction.Insert;
                            }

                            #endregion
                        }
                        else
                        {
                            if (timeCode.Classification != (int)timeCodeInput.Classification && IsTimeCodeUsedInRankings(entities, timeCodeId))
                            {
                                return new ActionResult((int)ActionResultSave.TimeCodeNotSaved, GetText(110694, "Tidkoden är kopplad till en viktningsregel och kan därför inte ändra typ av ersättning. Ta först bort tidkoden från viktningsregeln."));
                            }

                            #region Update

                            SetModifiedProperties(timeCode);

                            #endregion
                        }

                        // Update time code (base)
                        bool nameChanged = timeCode.Name != timeCodeInput.Name;
                        timeCode.Code = timeCodeInput.Code;
                        timeCode.Name = timeCodeInput.Name;
                        timeCode.Description = timeCodeInput.Description;
                        timeCode.RegistrationType = (int)timeCodeInput.RegistrationType;
                        timeCode.Classification = (int)timeCodeInput.Classification;
                        timeCode.State = (int)timeCodeInput.State;

                        // Update type specific fields
                        switch (timeCodeInput.Type)
                        {
                            case SoeTimeCodeType.Absense:
                            case SoeTimeCodeType.Work:
                                #region Absence/Work

                                timeCode.Payed = timeCodeInput.Payed;
                                timeCode.MinutesByConstantRules = timeCodeInput.MinutesByConstantRules;
                                timeCode.FactorBasedOnWorkPercentage = timeCodeInput.FactorBasedOnWorkPercentage;

                                //Rounding
                                timeCode.RoundingType = (int)timeCodeInput.RoundingType;
                                timeCode.RoundingValue = timeCodeInput.RoundingValue;
                                timeCode.RoundingTimeCodeId = timeCodeInput.RoundingTimeCodeId;
                                timeCode.RoundingInterruptionTimeCodeId = timeCode.RoundingType == (int)TermGroup_TimeCodeRoundingType.RoundUp && timeCode.RoundingValue > 0 ? timeCodeInput.RoundingInterruptionTimeCodeId.ToNullable() : null;
                                timeCode.RoundingGroupKey = timeCodeInput.RoundingGroupKey;
                                timeCode.RoundStartTime = timeCodeInput.RoundStartTime;

                                //Adjustment
                                timeCode.AdjustQuantityByBreakTime = (int)timeCodeInput.AdjustQuantityByBreakTime;
                                timeCode.AdjustQuantityTimeCodeId = timeCodeInput.AdjustQuantityByBreakTime == TermGroup_AdjustQuantityByBreakTime.Add ? timeCodeInput.AdjustQuantityTimeCodeId.ToNullable() : null;
                                timeCode.AdjustQuantityTimeScheduleTypeId = timeCodeInput.AdjustQuantityTimeScheduleTypeId.ToNullable();

                                timeCodeRuleInputs = new List<TimeCodeRuleDTO>();
                                if (timeCodeInput.TimeCodeRuleType == TermGroup_TimeCodeRuleType.AdjustQuantityOnTime || timeCodeInput.TimeCodeRuleType == TermGroup_TimeCodeRuleType.AdjustQuantityOnScheduleInNextDay)
                                    timeCodeRuleInputs.Add(new TimeCodeRuleDTO(timeCodeInput.TimeCodeRuleType, timeCodeInput.TimeCodeRuleValue ?? 0, timeCodeInput.TimeCodeRuleTime));

                                if (timeCode is TimeCodeAbsense timeCodeAbsence)
                                {
                                    timeCodeAbsence.KontekId = timeCodeInput.KontekId;
                                    timeCodeAbsence.IsAbsence = timeCodeInput.IsAbsence;
                                }
                                else if (timeCode is TimeCodeWork timeCodeWork)
                                {
                                    timeCodeWork.IsWorkOutsideSchedule = timeCodeInput.IsWorkOutsideSchedule;
                                }

                                #endregion
                                break;
                            case SoeTimeCodeType.AdditionDeduction:
                                #region AdditionDeduction

                                if (timeCode is TimeCodeAdditionDeduction timeCodeAdditionDeduction)
                                {
                                    decimal? fixedQuantityInput = timeCode.RegistrationType == (int)TermGroup_TimeCodeRegistrationType.Quantity && timeCodeInput.FixedQuantity > 0 ? timeCodeInput.FixedQuantity : null;

                                    if (timeCode.TimeCodeId > 0 && (timeCodeInput.ShowInTerminal || timeCodeAdditionDeduction.ShowInTerminal))
                                    {
                                        // Only Name, FixedQuantity or showInTerminal is a relevant change for the terminal
                                        if (nameChanged || timeCodeAdditionDeduction.FixedQuantity != fixedQuantityInput || timeCodeAdditionDeduction.ShowInTerminal != timeCodeInput.ShowInTerminal)
                                        {
                                            changeAffectTerminal = true;
                                            action = WebPubSubMessageAction.Update;
                                        }
                                    }

                                    timeCodeAdditionDeduction.ExpenseType = (int)timeCodeInput.ExpenseType;
                                    timeCodeAdditionDeduction.Comment = StringUtility.NullToEmpty(timeCodeInput.Comment);
                                    timeCodeAdditionDeduction.StopAtDateStart = timeCodeInput.StopAtDateStart;
                                    timeCodeAdditionDeduction.StopAtDateStop = timeCodeInput.StopAtDateStop;
                                    timeCodeAdditionDeduction.StopAtPrice = timeCodeInput.StopAtPrice;
                                    timeCodeAdditionDeduction.StopAtVat = timeCodeInput.StopAtVat;
                                    timeCodeAdditionDeduction.StopAtAccounting = timeCodeInput.StopAtAccounting;
                                    timeCodeAdditionDeduction.StopAtComment = timeCodeInput.StopAtComment;
                                    timeCodeAdditionDeduction.CommentMandatory = timeCodeInput.CommentMandatory;
                                    timeCodeAdditionDeduction.HideForEmployee = timeCodeInput.HideForEmployee;
                                    timeCodeAdditionDeduction.ShowInTerminal = timeCodeInput.ShowInTerminal;
                                    timeCodeAdditionDeduction.FixedQuantity = fixedQuantityInput;
                                }

                                #endregion
                                break;
                            case SoeTimeCodeType.Break:
                                #region Break

                                if (timeCodeInput.StartType == timeCodeInput.StopType && timeCodeInput.StartTimeMinutes == timeCodeInput.StopTimeMinutes && timeCodeInput.StartType != (int)SoeTimeCodeBreakTimeType.Clock)
                                    return new ActionResult((int)ActionResultSave.TimeCodeNotSaved, GetText(10134, "Felaktigt rastfönster. In och ut har samma händelse"));
                                if (timeCodeInput.StartType == (int)SoeTimeCodeBreakTimeType.Clock && timeCodeInput.StopType == (int)SoeTimeCodeBreakTimeType.Clock && (timeCodeInput.StopTimeMinutes - timeCodeInput.StartTimeMinutes) < timeCodeInput.DefaultMinutes)
                                    return new ActionResult((int)ActionResultSave.TimeCodeNotSaved, GetText(10135, "Felaktigt rastfönster. Fönster är kortare än standardrast"));

                                if (timeCode is TimeCodeBreak timeCodeBreak)
                                {
                                    timeCodeBreak.MinMinutes = timeCodeInput.MinMinutes;
                                    timeCodeBreak.MaxMinutes = timeCodeInput.MaxMinutes;
                                    timeCodeBreak.DefaultMinutes = timeCodeInput.DefaultMinutes;
                                    timeCodeBreak.StartType = timeCodeInput.StartType;
                                    timeCodeBreak.StopType = timeCodeInput.StopType;
                                    timeCodeBreak.StartTimeMinutes = timeCodeInput.StartTimeMinutes;
                                    timeCodeBreak.StopTimeMinutes = timeCodeInput.StopTimeMinutes;
                                    timeCodeBreak.StartTime = timeCodeInput.StartTime.HasValue ? CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, timeCodeInput.StartTime.Value) : (DateTime?)null;
                                    timeCodeBreak.Template = timeCodeInput.Template;
                                    timeCodeBreak.TimeCodeBreakGroupId = timeCodeInput.TimeCodeBreakGroupId;
                                }

                                timeCodeRuleInputs = new List<TimeCodeRuleDTO>()
                                {
                                    new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeEarlierThanStart, timeCodeInput.GetTimeCodeRuleValue(TermGroup_TimeCodeRuleType.TimeCodeEarlierThanStart)),
                                    new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeLaterThanStop, timeCodeInput.GetTimeCodeRuleValue(TermGroup_TimeCodeRuleType.TimeCodeLaterThanStop)),
                                    new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeLessThanMin, timeCodeInput.GetTimeCodeRuleValue(TermGroup_TimeCodeRuleType.TimeCodeLessThanMin)),
                                    new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd, timeCodeInput.GetTimeCodeRuleValue(TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd)),
                                    new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeStd, timeCodeInput.GetTimeCodeRuleValue(TermGroup_TimeCodeRuleType.TimeCodeStd)),
                                    new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax, timeCodeInput.GetTimeCodeRuleValue(TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax)),
                                    new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeMoreThanMax, timeCodeInput.GetTimeCodeRuleValue(TermGroup_TimeCodeRuleType.TimeCodeMoreThanMax)),
                                    new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.AdjustQuantityOnTime, timeCodeInput.GetTimeCodeRuleValue(TermGroup_TimeCodeRuleType.AdjustQuantityOnTime)),
                                    new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.AdjustQuantityOnScheduleInNextDay, timeCodeInput.GetTimeCodeRuleValue(TermGroup_TimeCodeRuleType.AdjustQuantityOnScheduleInNextDay))
                                };

                                #endregion
                                break;
                            case SoeTimeCodeType.Material:
                                #region Material

                                if (timeCode is TimeCodeMaterial timeCodeMaterial)
                                {
                                    timeCodeMaterial.Note = timeCodeInput.Note;
                                }

                                #endregion
                                break;
                        }

                        #endregion

                        #region Relations

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        result = SaveTimeCodeRules(entities, transaction, timeCodeRuleInputs, timeCode);
                        if (!result.Success)
                            return result;

                        result = SetTimeCodeInvoiceProducts(timeCodeInput, entities, timeCode);
                        if (!result.Success)
                            return result;

                        result = SetTimeCodePayrollProducts(timeCodeInput, entities, timeCode, out var timeCodePayrollProducts);
                        if (!result.Success)
                            return result;

                        // For absence, only one type of SysPayrollTypeLevel3 may exist
                        if (timeCode.IsAbsence())
                        {
                            foreach (TimeCodePayrollProduct timeCodePayrollProduct in timeCodePayrollProducts)
                            {
                                if (timeCodePayrollProduct.PayrollProduct.IsAbsence() &&
                                    timeCodePayrollProducts.Any(tcpp => tcpp.PayrollProduct.IsAbsence() && tcpp.PayrollProduct.SysPayrollTypeLevel3 != timeCodePayrollProduct.PayrollProduct.SysPayrollTypeLevel3))
                                    return new ActionResult((int)ActionResultSave.TimeCodeAbsenceCannotHaveMultipleSysPayrollTypeLevel3, GetText(92033, "Frånvarotidkod kan inte kopplas till flera lönearter med olika typ av lön nivå 3"));
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Break - post save relations

                        if (timeCodeInput.Type == SoeTimeCodeType.Break)
                        {
                            #region TimeCodeBreakTimeCodeDeviationCauses

                            // Delete all current EmployeeGroupTimeDeviationCauseTimeCode
                            while ((timeCode as TimeCodeBreak).TimeCodeBreakTimeCodeDeviationCauses.Count > 0)
                            {
                                result = DeleteEntityItem(entities, (timeCode as TimeCodeBreak).TimeCodeBreakTimeCodeDeviationCauses.FirstOrDefault(), useBulkSaveChanges: false);
                                if (!result.Success)
                                    return result;
                            }

                            // Add new mapping, from the input collection
                            if (timeCodeInput.TimeCodeDeviationCauses != null)
                            {
                                foreach (TimeCodeBreakTimeCodeDeviationCauseDTO item in timeCodeInput.TimeCodeDeviationCauses)
                                {
                                    // Empty
                                    if (item.TimeCodeId == 0 || item.TimeCodeDeviationCauseId == 0)
                                        continue;

                                    // Prevent duplicates
                                    if ((timeCode as TimeCodeBreak).TimeCodeBreakTimeCodeDeviationCauses.Any(a => a.TimeCode.TimeCodeId == item.TimeCodeId && a.TimeDeviationCause.TimeDeviationCauseId == item.TimeCodeDeviationCauseId))
                                        continue;

                                    // TODO: Add foreign keys to model
                                    TimeCodeBreakTimeCodeDeviationCause mapping = new TimeCodeBreakTimeCodeDeviationCause();
                                    mapping.TimeDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(entities, item.TimeCodeDeviationCauseId, actorCompanyId, false);
                                    mapping.TimeCode = TimeCodeManager.GetTimeCode(entities, item.TimeCodeId, actorCompanyId, false);
                                    mapping.TimeCodeBreak = (timeCode as TimeCodeBreak);
                                    result = AddEntityItem(entities, mapping, "TimeCodeBreakTimeCodeDeviationCause");
                                    if (!result.Success)
                                        return result;
                                }
                            }

                            #endregion

                            #region TimeCodeBreakGroup/EmployeeGroup

                            if (FeatureManager.HasRolePermission(Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup, Permission.Modify, roleId, actorCompanyId, entities: entities))
                            {
                                //Determine which to connect
                                List<int> idsToConnect = new List<int>();
                                if (timeCodeInput.EmployeeGroupIds != null)
                                {
                                    foreach (int id in timeCodeInput.EmployeeGroupIds)
                                    {
                                        if (!(timeCode as TimeCodeBreak).EmployeeGroupsForBreak.Any(i => i.EmployeeGroupId == id))
                                            idsToConnect.Add(id);
                                    }
                                }

                                if (idsToConnect.Any() && !timeCodeInput.TimeCodeBreakGroupId.HasValue)
                                    return new ActionResult((int)ActionResultSave.TimeCodeBreakEmployeeGroupMappedToOtherBreakWithSameBreakGroup, GetText(12114, "En rastgrupp måste väljas om tidavtal ska kopplas till rasttypen"));

                                //Determine which to disconnect
                                List<int> idsToDisconnect = new List<int>();
                                foreach (int employeeGroupId in (timeCode as TimeCodeBreak).EmployeeGroupsForBreak.Select(e => e.EmployeeGroupId))
                                {
                                    if (!timeCodeInput.EmployeeGroupIds.Contains(employeeGroupId))
                                        idsToDisconnect.Add(employeeGroupId);
                                }

                                //Connect
                                foreach (int id in idsToConnect)
                                {
                                    EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(entities, id);
                                    if (employeeGroup != null)
                                    {
                                        //Validate that EmployeeGroup is not connected to other TimeCodeBreak with same TimeCodeBreakGroup
                                        bool exists = (from tc in entities.TimeCode.OfType<TimeCodeBreak>()
                                                       where tc.ActorCompanyId == actorCompanyId &&
                                                       tc.TimeCodeId != timeCode.TimeCodeId && //Different TimeCodeBreak
                                                       tc.TimeCodeBreakGroupId.HasValue && tc.TimeCodeBreakGroupId.Value == timeCodeInput.TimeCodeBreakGroupId.Value && //Same TimeCodeBreakGroup
                                                       tc.EmployeeGroupsForBreak.Any(i => i.EmployeeGroupId == employeeGroup.EmployeeGroupId) //Same EmployeeGroup
                                                       select tc).Any();

                                        if (exists)
                                            return new ActionResult((int)ActionResultSave.TimeCodeBreakEmployeeGroupMappedToOtherBreakWithSameBreakGroup, GetText(5927, "Det finns minst en annan rasttyp med samma tidavtal och rastgrupp"));

                                        (timeCode as TimeCodeBreak).EmployeeGroupsForBreak.Add(employeeGroup);
                                    }
                                }

                                //Disconnect
                                foreach (int id in idsToDisconnect)
                                {
                                    EmployeeGroup employeeGroup = (timeCode as TimeCodeBreak).EmployeeGroupsForBreak.FirstOrDefault(t => t.EmployeeGroupId == id);
                                    if (employeeGroup != null)
                                        (timeCode as TimeCodeBreak).EmployeeGroupsForBreak.Remove(employeeGroup);
                                }

                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                    return result;
                            }

                            #endregion
                        }

                        #endregion

                        if (result.Success)
                        {
                            #region WebPubSub

                            if (changeAffectTerminal)
                                SendWebPubSubMessage(entities, base.ActorCompanyId, timeCode, action);

                            #endregion

                            //Commit transaction
                            transaction.Complete();
                            timeCodeId = timeCode.TimeCodeId;
                            result.IntegerValue = timeCodeId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = timeCodeId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private ActionResult SetTimeCodeInvoiceProducts(TimeCodeSaveDTO timeCodeInput, CompEntities entities, TimeCode timeCode)
        {
            List<TimeCodeInvoiceProduct> timeCodeInvoiceProducts = new List<TimeCodeInvoiceProduct>();

            if (timeCodeInput.InvoiceProducts != null)
            {
                if (timeCodeInput.InvoiceProducts.Any(p => p.InvoiceProductId <= 0))
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(110691, "Artikel måste anges"));

                timeCodeInvoiceProducts.AddRange(GetTimeCodeInvoiceProducts(entities, timeCode.TimeCodeId));

                foreach (var timeCodeInvoiceProduct in timeCodeInvoiceProducts.ToList())
                {
                    var item = timeCodeInput.InvoiceProducts.FirstOrDefault(i => i.InvoiceProductId == timeCodeInvoiceProduct.InvoiceProduct.ProductId);
                    if (item == null)
                    {
                        #region Delete

                        entities.DeleteObject(timeCodeInvoiceProduct);
                        timeCodeInvoiceProducts.Remove(timeCodeInvoiceProduct);

                        #endregion
                    }
                    else if (
                        item.InvoiceProductId != timeCodeInvoiceProduct.ProductId ||
                        item.Factor != timeCodeInvoiceProduct.Factor)
                    {
                        #region Update

                        var invoiceProduct = ProductManager.GetInvoiceProduct(entities, item.InvoiceProductId);
                        timeCodeInvoiceProduct.Update(invoiceProduct, item.Factor);
                        timeCodeInput.InvoiceProducts.Remove(item);

                        #endregion
                    }
                }

                foreach (var item in timeCodeInput.InvoiceProducts)
                {
                    #region Add

                    // Prevent duplicates (if added 2 of same products in gui, only one was handled above)
                    // InvoiceProduct can be null if timeCodeInvoiceProduct has been deleted above
                    var timeCodeInvoiceProduct = timeCodeInvoiceProducts.FirstOrDefault(i => i.InvoiceProduct != null && i.InvoiceProduct.ProductId == item.InvoiceProductId);
                    if (timeCodeInvoiceProduct != null)
                        continue;

                    var invoiceProduct = ProductManager.GetInvoiceProduct(entities, item.InvoiceProductId);

                    timeCodeInvoiceProduct = TimeCodeInvoiceProduct.Create(timeCode, invoiceProduct, item.Factor);
                    if (timeCodeInvoiceProduct == null)
                        continue;

                    entities.TimeCodeInvoiceProduct.AddObject(timeCodeInvoiceProduct);
                    timeCodeInvoiceProducts.Add(timeCodeInvoiceProduct);

                    #endregion
                }
            }

            return new ActionResult(true);
        }

        private ActionResult SetTimeCodePayrollProducts(TimeCodeSaveDTO timeCodeInput, CompEntities entities, TimeCode timeCode, out List<TimeCodePayrollProduct> timeCodePayrollProducts)
        {
            timeCodePayrollProducts = new List<TimeCodePayrollProduct>();

            if (timeCodeInput.PayrollProducts != null)
            {
                if (timeCodeInput.PayrollProducts.Any(p => p.PayrollProductId <= 0))
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(110690, "Löneart måste anges"));

                timeCodePayrollProducts.AddRange(GetTimeCodePayrollProducts(entities, timeCode.TimeCodeId));

                foreach (var timeCodePayrollProduct in timeCodePayrollProducts.ToList())
                {
                    var item = timeCodeInput.PayrollProducts.FirstOrDefault(i => i.PayrollProductId == timeCodePayrollProduct.PayrollProduct.ProductId);
                    if (item == null)
                    {
                        #region Delete

                        entities.DeleteObject(timeCodePayrollProduct);
                        timeCodePayrollProducts.Remove(timeCodePayrollProduct);

                        #endregion
                    }
                    else if (
                        item.PayrollProductId != timeCodePayrollProduct.ProductId ||
                        item.Factor != timeCodePayrollProduct.Factor)
                    {
                        #region Update

                        var payrollProduct = ProductManager.GetPayrollProduct(entities, item.PayrollProductId);
                        timeCodePayrollProduct.Update(payrollProduct, item.Factor);
                        timeCodeInput.PayrollProducts.Remove(item);

                        #endregion
                    }

                }

                foreach (var item in timeCodeInput.PayrollProducts)
                {
                    #region Add

                    // Prevent duplicates (if added 2 of same products in gui, only one was handled above)
                    // PayrollProduct can be null if timeCodeInvoiceProduct has been deleted above
                    var timeCodePayrollProduct = timeCodePayrollProducts.FirstOrDefault(i => i.PayrollProduct != null && i.PayrollProduct.ProductId == item.PayrollProductId);
                    if (timeCodePayrollProduct != null)
                        continue;

                    var payrollProduct = ProductManager.GetPayrollProduct(entities, item.PayrollProductId);

                    timeCodePayrollProduct = TimeCodePayrollProduct.Create(timeCode, payrollProduct, item.Factor);
                    if (timeCodePayrollProduct == null)
                        continue;

                    entities.TimeCodePayrollProduct.AddObject(timeCodePayrollProduct);
                    timeCodePayrollProducts.Add(timeCodePayrollProduct);

                    #endregion
                }
            }

            return new ActionResult(true);
        }

        public ActionResult SaveTimeCodeRules(CompEntities entities, TransactionScope transaction, List<TimeCodeRuleDTO> timeCodeRuleInputs, TimeCode timeCode)
        {
            if (timeCodeRuleInputs == null || timeCode == null)
                return new ActionResult(true);

            if (timeCode.TimeCodeRule == null)
                timeCode.TimeCodeRule = new EntityCollection<TimeCodeRule>();

            bool saveNeeded = false;

            if (timeCode.IsBreak())
                TrySaveBreakRules();
            else
                TrySaveQuantityRules();

            return saveNeeded ? SaveChanges(entities, transaction) : new ActionResult(true);

            void TrySaveBreakRules()
            {
                foreach (TimeCodeRuleDTO timeCodeRuleInput in timeCodeRuleInputs.Where(i => i.IsBreakRule()))
                {
                    TimeCodeRule timeCodeRule = timeCode.TimeCodeRule.FirstOrDefault(r => r.Type == timeCodeRuleInput.Type && r.State == (int)SoeEntityState.Active);
                    if (timeCodeRule == null)
                        Add(timeCodeRuleInput);
                    else if (!timeCodeRuleInput.IsValid())
                        Delete(timeCodeRule);
                    else if (timeCodeRuleInput.IsModified(timeCodeRule.Value, timeCodeRule.Time))
                        Update(timeCodeRule, timeCodeRuleInput);
                }
            }
            void TrySaveQuantityRules()
            {
                TimeCodeRuleDTO timeCodeRuleInput = timeCodeRuleInputs.FirstOrDefault(r => r.IsQuantityRule());
                List<TimeCodeRule> timeCodeRules = timeCode.TimeCodeRule.Where(r => r.IsQuantityRule() && r.State == (int)SoeEntityState.Active).ToList();
                TimeCodeRule timeCodeRule = timeCodeRules.FirstOrDefault();
                if (timeCodeRuleInput == null && timeCodeRule == null)
                    return;

                if (timeCodeRuleInput != null && timeCodeRule == null)
                    Add(timeCodeRuleInput);
                else if (timeCodeRuleInput == null && timeCodeRule != null)
                    Delete(timeCodeRule);
                else if (timeCodeRuleInput.IsModified(timeCodeRule.Value, timeCodeRule.Time))
                    Update(timeCodeRule, timeCodeRuleInput);

                //Precaution: Delete any other quantity rule
                if (timeCodeRules.Count > 1)
                    timeCodeRules.Where(r => r.TimeCodeRuleId != timeCodeRule.TimeCodeRuleId).ToList().ForEach(r => Delete(r));

            }
            void Add(TimeCodeRuleDTO timeCodeRuleInput)
            {
                if (!timeCodeRuleInput.IsValid())
                    return;

                TimeCodeRule timeCodeRule = new TimeCodeRule()
                {
                    Type = timeCodeRuleInput.Type,
                    Value = timeCodeRuleInput.Value,
                    State = (int)SoeEntityState.Active,

                    //Set references
                    TimeCode = timeCode,
                };
                SetTime(timeCodeRule, timeCodeRuleInput);
                SetCreatedProperties(timeCodeRule);
                timeCode.TimeCodeRule.Add(timeCodeRule);
                saveNeeded = true;
            }
            void Delete(TimeCodeRule timeCodeRule)
            {
                timeCodeRule.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(timeCodeRule);
                saveNeeded = true;
            }
            void Update(TimeCodeRule timeCodeRule, TimeCodeRuleDTO timeCodeRuleInput)
            {
                if (timeCodeRule.IsQuantityRule())
                    timeCodeRule.Type = timeCodeRuleInput.Type;
                timeCodeRule.Value = timeCodeRuleInput.Value;
                SetTime(timeCodeRule, timeCodeRuleInput);
                SetModifiedProperties(timeCodeRule);
                saveNeeded = true;
            }
            void SetTime(TimeCodeRule timeCodeRule, TimeCodeRuleDTO timeCodeRuleInput)
            {
                if (timeCodeRuleInput.Time.HasValue)
                    timeCodeRule.Time = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, timeCodeRuleInput.Time.Value);
            }
        }

        public ActionResult DeleteTimeCode(int timeCodeId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ActionResult result = IsOkToDeleteTimeCode(entities, timeCodeId);
                if (!result.Success)
                {
                    result.ErrorMessage = result.ErrorMessage.Insert(0, $"{GetText(5112, "Tidkod kunde inte tas bort")}. ");
                    return result;
                }

                TimeCode originalTimeCode = GetTimeCode(entities, timeCodeId, actorCompanyId, false);
                if (originalTimeCode == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

                result = ChangeEntityState(entities, originalTimeCode, SoeEntityState.Deleted, true);
                if (result.Success && originalTimeCode is TimeCodeAdditionDeduction && (originalTimeCode as TimeCodeAdditionDeduction).ShowInTerminal)
                    SendWebPubSubMessage(entities, originalTimeCode.ActorCompanyId, originalTimeCode, WebPubSubMessageAction.Delete);

                return result;
            }
        }

        public ActionResult UpdateTimeCodeState(Dictionary<int, bool> timeCodes, int actorCompanyId)
        {
            int defualtTimeCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);
            using (CompEntities entities = new CompEntities())
            {
                foreach (KeyValuePair<int, bool> timeCode in timeCodes)
                {
                    TimeCode originalTimeCode = GetTimeCode(entities, timeCode.Key, actorCompanyId, false);
                    if (originalTimeCode == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

                    else if (!timeCode.Value && defualtTimeCodeId == timeCode.Key)
                        return new ActionResult((int)ActionResultSave.EntityNotUpdated, GetText(12234, "Tidkoden är satt som standard tidkod under företagsinställningar och kan inte inaktiveras"));

                    ChangeEntityState(originalTimeCode, timeCode.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult SaveTimeCodeProducts(TimeCode timeCode, Collection<FormIntervalEntryItem> invoiceProductItems, Collection<FormIntervalEntryItem> payrollProductItems, int actorCompanyId)
        {
            if (timeCode == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeCode");

            using (CompEntities entities = new CompEntities())
            {
                TimeCode originalTimeCode = GetTimeCode(entities, timeCode.TimeCodeId, actorCompanyId, false);

                #region TimeCodeInvoiceProduct

                if (invoiceProductItems != null)
                {
                    List<TimeCodeInvoiceProduct> timeCodeInvoiceProducts = new List<TimeCodeInvoiceProduct>();

                    #region Update / delete

                    //Add existing
                    timeCodeInvoiceProducts.AddRange(GetTimeCodeInvoiceProducts(entities, originalTimeCode.TimeCodeId));

                    foreach (var timeCodeInvoiceProduct in timeCodeInvoiceProducts.ToList()) //Use ToList to be able to remove in loop
                    {
                        var item = invoiceProductItems.FirstOrDefault(i => i.LabelType == timeCodeInvoiceProduct.InvoiceProduct.ProductId);
                        if (item != null)
                        {
                            #region Update

                            timeCodeInvoiceProduct.Factor = NumberUtility.ToDecimalWithComma(item.From, 2);

                            //Remove from input collection
                            invoiceProductItems.Remove(item);

                            #endregion
                        }
                        else
                        {
                            #region Delete

                            entities.DeleteObject(timeCodeInvoiceProduct);

                            //Remove from existing collection
                            timeCodeInvoiceProducts.Remove(timeCodeInvoiceProduct);

                            #endregion
                        }
                    }

                    #endregion

                    #region Add

                    //Add the TimeCodeInvocieProducts remaining in the input collection
                    foreach (var item in invoiceProductItems)
                    {
                        #region Prereq

                        int productId = item.LabelType;
                        decimal factor = NumberUtility.ToDecimalWithComma(item.From, 2);

                        //Prevent duplicates (if added 2 of same products in gui, only one was handled above)
                        //InvoiceProduct can be null if timeCodeInvoiceProduct has been deleted above
                        var timeCodeInvoiceProduct = timeCodeInvoiceProducts.FirstOrDefault(i => i.InvoiceProduct != null && i.InvoiceProduct.ProductId == item.LabelType);
                        if (timeCodeInvoiceProduct != null)
                            continue;

                        InvoiceProduct invoiceProduct = ProductManager.GetInvoiceProduct(entities, productId);
                        if (invoiceProduct == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "InvoiceProduct");

                        #endregion

                        timeCodeInvoiceProduct = new TimeCodeInvoiceProduct()
                        {
                            Factor = factor,

                            //References
                            TimeCode = originalTimeCode,
                            InvoiceProduct = invoiceProduct,
                        };
                        entities.TimeCodeInvoiceProduct.AddObject(timeCodeInvoiceProduct);
                        timeCodeInvoiceProducts.Add(timeCodeInvoiceProduct);
                    }

                    #endregion
                }

                #endregion

                #region TimeCodePayrollProduct

                List<TimeCodePayrollProduct> timeCodePayrollProducts = new List<TimeCodePayrollProduct>();

                if (payrollProductItems != null)
                {
                    #region Update / delete

                    //Add existing
                    timeCodePayrollProducts.AddRange(GetTimeCodePayrollProducts(entities, originalTimeCode.TimeCodeId));

                    foreach (TimeCodePayrollProduct timeCodePayrollProduct in timeCodePayrollProducts.ToList()) //Use ToList to be able to remove in loop
                    {
                        var item = payrollProductItems.FirstOrDefault(i => i.LabelType == timeCodePayrollProduct.PayrollProduct.ProductId);
                        if (item != null)
                        {
                            #region Update

                            timeCodePayrollProduct.Factor = NumberUtility.ToDecimalWithComma(item.From, 2);

                            //Remove from input collection
                            payrollProductItems.Remove(item);

                            #endregion
                        }
                        else
                        {
                            #region Delete

                            entities.DeleteObject(timeCodePayrollProduct);

                            //Remove from existing collection
                            timeCodePayrollProducts.Remove(timeCodePayrollProduct);

                            #endregion
                        }
                    }

                    #endregion

                    #region Add

                    //Add the TimeCodeInvoiceProducts remaining in the input collection
                    foreach (var item in payrollProductItems)
                    {
                        #region Prereq

                        int productId = item.LabelType;
                        decimal factor = NumberUtility.ToDecimalWithComma(item.From, 2);

                        //Prevent duplicates (if added 2 of same products in gui, only one was handled above)
                        //PayrollProduct can be null if timeCodeInvoiceProduct has been deleted above
                        TimeCodePayrollProduct timeCodePayrollProduct = timeCodePayrollProducts.FirstOrDefault(i => i.PayrollProduct != null && i.PayrollProduct.ProductId == item.LabelType);
                        if (timeCodePayrollProduct != null)
                            continue;

                        PayrollProduct payrollProduct = ProductManager.GetPayrollProduct(entities, productId);
                        if (payrollProduct == null)
                            continue;

                        #endregion

                        timeCodePayrollProduct = new TimeCodePayrollProduct()
                        {
                            Factor = factor,

                            //References
                            TimeCode = originalTimeCode,
                            PayrollProduct = payrollProduct,
                        };
                        entities.TimeCodePayrollProduct.AddObject(timeCodePayrollProduct);
                        timeCodePayrollProducts.Add(timeCodePayrollProduct);
                    }

                    #endregion
                }

                List<PayrollProduct> payrollProducts = timeCodePayrollProducts.Select(i => i.PayrollProduct).ToList();
                foreach (TimeCodePayrollProduct timeCodePayrollProduct in timeCodePayrollProducts)
                {
                    //For absence, only one type of SysPayrollTypeLevel3 may exist
                    if (timeCode.IsAbsence() && timeCodePayrollProduct.PayrollProduct.IsAbsence() && payrollProducts.Any(i => i.IsAbsence() && i.SysPayrollTypeLevel3 != timeCodePayrollProduct.PayrollProduct.SysPayrollTypeLevel3))
                        return new ActionResult((int)ActionResultSave.TimeCodeAbsenceCannotHaveMultipleSysPayrollTypeLevel3);
                }

                #endregion

                return SaveChanges(entities);
            }
        }

        private void SendWebPubSubMessage(CompEntities entities, int actorCompanyId, TimeCode timeCode, WebPubSubMessageAction action)
        {
            List<int> terminalIds = TimeStampManager.GetTimeTerminalIdsForPubSub(entities, actorCompanyId);
            foreach (int terminalId in terminalIds)
            {
                base.WebPubSubSendMessage(GoTimeStampExtensions.GetTerminalPubSubKey(actorCompanyId, terminalId), timeCode.GetUpdateMessage(action));
            }
        }

        #endregion

        #region TimeCodeBreak

        public List<TimeCodeBreak> GetTimeCodeBreaks(int actorCompanyId, bool addEmptyRow = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCodeBreaks(entities, actorCompanyId, addEmptyRow);
        }

        public List<TimeCodeBreak> GetTimeCodeBreaks(CompEntities entities, int actorCompanyId, bool addEmptyRow = false)
        {
            List<TimeCodeBreak> timeCodeBreaks = (from tc in entities.TimeCode.OfType<TimeCodeBreak>()
                                                  where tc.ActorCompanyId == actorCompanyId &&
                                                  tc.Type == (int)SoeTimeCodeType.Break &&
                                                  tc.State == (int)SoeEntityState.Active
                                                  select tc).ToList();

            if (addEmptyRow)
                timeCodeBreaks.Add(new TimeCodeBreak() { TimeCodeId = 0, Code = " ", Name = " " });

            return timeCodeBreaks.OrderBy(tc => tc.DefaultMinutes).ThenBy(tc => tc.Code).ToList();
        }

        public List<TimeCodeBreak> GetTimeCodeBreaksForEmployee(int actorCompanyId, int employeeId, DateTime? date, bool addEmptyRow = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Employee.NoTracking();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCodeBreaksForEmployee(entities, actorCompanyId, employeeId, date, addEmptyRow);
        }

        public List<TimeCodeBreak> GetTimeCodeBreaksForEmployee(CompEntities entities, int actorCompanyId, int employeeId, DateTime? date, bool addEmptyRow = false)
        {
            EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroupForEmployee(entities, employeeId, actorCompanyId, date ?? DateTime.Today, getFirstIfNotFound: true);
            return GetTimeCodeBreaksForEmployeeGroup(entities, actorCompanyId, employeeGroup?.EmployeeGroupId, addEmptyRow);
        }

        public List<TimeCodeBreak> GetTimeCodeBreaksForEmployeePost(int actorCompanyId, int employeePostId, bool addEmptyRow = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Employee.NoTracking();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCodeBreaksForEmployeePost(entities, actorCompanyId, employeePostId, addEmptyRow);
        }

        public List<TimeCodeBreak> GetTimeCodeBreaksForEmployeePost(CompEntities entities, int actorCompanyId, int employeePostId, bool addEmptyRow = false)
        {
            EmployeeGroup employeeGroup = TimeScheduleManager.GetEmployeePost(entities, employeePostId, loadEmployeeGroup: true)?.EmployeeGroup;
            return GetTimeCodeBreaksForEmployeeGroup(entities, actorCompanyId, employeeGroup?.EmployeeGroupId, addEmptyRow);
        }

        private List<TimeCodeBreak> GetTimeCodeBreaksForEmployeeGroup(CompEntities entities, int actorCompanyId, int? employeeGroupId, bool addEmptyRow = false)
        {
            List<TimeCodeBreak> timeCodeBreaks = (from tc in entities.TimeCode.OfType<TimeCodeBreak>()
                                                    .Include("EmployeeGroupsForBreak")
                                                  where tc.ActorCompanyId == actorCompanyId &&
                                                  tc.Type == (int)SoeTimeCodeType.Break &&
                                                  tc.State == (int)SoeEntityState.Active
                                                  select tc).ToList();

            List<int> timeCodeBreakIdsForEmployeeGroup = employeeGroupId.HasValue ? timeCodeBreaks
                .Where(tc => tc.EmployeeGroupsForBreak.Any(eg => eg.EmployeeGroupId == employeeGroupId.Value))
                .Select(tc => tc.TimeCodeId)
                .ToList() : null;

            foreach (TimeCodeBreak timeCodeBreak in timeCodeBreaks.ToList())
            {
                if (!timeCodeBreak.EmployeeGroupsForBreak.IsNullOrEmpty() && !timeCodeBreakIdsForEmployeeGroup.IsNullOrEmpty() && !timeCodeBreakIdsForEmployeeGroup.Any(timeCodeId => timeCodeId == timeCodeBreak.TimeCodeId))
                    timeCodeBreaks.Remove(timeCodeBreak);
            }

            if (addEmptyRow)
                timeCodeBreaks.Add(new TimeCodeBreak() { TimeCodeId = 0, DefaultMinutes = 0, Code = " ", Name = " " });

            return timeCodeBreaks.OrderBy(tc => tc.DefaultMinutes).ThenBy(tc => tc.Code).ToList();
        }

        public TimeCodeBreak GetTimeCodeBreak(int timeCodeId, int actorCompanyId, bool loadTimeCodeRules = false, bool loadEmployeeGroups = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCode.NoTracking();
            return GetTimeCodeBreak(entities, timeCodeId, actorCompanyId, loadTimeCodeRules, loadEmployeeGroups);
        }

        public TimeCodeBreak GetTimeCodeBreak(CompEntities entities, int timeCodeId, int actorCompanyId, bool loadTimeCodeRules = false, bool loadEmployeeGroups = false)
        {
            var timeCodeBreak = (from tc in entities.TimeCode.OfType<TimeCodeBreak>()
                                 where tc.TimeCodeId == timeCodeId &&
                                 tc.ActorCompanyId == actorCompanyId &&
                                 tc.State == (int)SoeEntityState.Active
                                 select tc).FirstOrDefault<TimeCodeBreak>();

            if (timeCodeBreak != null)
            {
                if (loadTimeCodeRules && !timeCodeBreak.TimeCodeRule.IsLoaded)
                    timeCodeBreak.TimeCodeRule.Load();
                if (loadEmployeeGroups && !timeCodeBreak.EmployeeGroupsForBreak.IsLoaded)
                    timeCodeBreak.EmployeeGroupsForBreak.Load();
            }

            return timeCodeBreak;
        }

        public TimeCodeBreak GetPrevNextTimeCodeBreak(int timeCodeId, SoeFormMode mode)
        {
            TimeCodeBreak timeCode = null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeCode.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                timeCode = (from tc in entitiesReadOnly.TimeCode.OfType<TimeCodeBreak>()
                            where tc.TimeCodeId > timeCodeId &&
                            tc.State == (int)SoeEntityState.Active
                            orderby tc.TimeCodeId ascending
                            select tc).FirstOrDefault<TimeCodeBreak>();
            }
            else
            {
                timeCode = (from tc in entitiesReadOnly.TimeCode.OfType<TimeCodeBreak>()
                            where tc.TimeCodeId < timeCodeId &&
                            tc.State == (int)SoeEntityState.Active
                            orderby tc.TimeCodeId descending
                            select tc).FirstOrDefault<TimeCodeBreak>();
            }

            return timeCode;
        }

        #endregion

        #region TimeCodeAdditionDeduction

        public List<TimeCodeAdditionDeduction> GetTimeCodeAdditionDeductions(int actorCompanyId, bool checkInvoiceProduct = false, bool isMySelf = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeCodeAdditionDeductions(entities, actorCompanyId, checkInvoiceProduct, isMySelf);
        }

        public List<TimeCodeAdditionDeduction> GetTimeCodeAdditionDeductions(CompEntities entities, int actorCompanyId, bool checkInvoiceProduct = false, bool isMySelf = false)
        {
            if (checkInvoiceProduct)
            {
                var timeCodeAdditionDeductions = (from tc in entities.TimeCode.OfType<TimeCodeAdditionDeduction>()
                                                  .Include("TimeCodeInvoiceProduct.InvoiceProduct")
                                                  where tc.Type == (int)SoeTimeCodeType.AdditionDeduction &&
                                                  tc.ActorCompanyId == actorCompanyId &&
                                                  (!isMySelf || !tc.HideForEmployee) &&
                                                  tc.State == (int)SoeEntityState.Active

                                                  select tc).ToList();

                foreach (var timeCode in timeCodeAdditionDeductions)
                {
                    timeCode.HasInvoiceProducts = timeCode.TimeCodeInvoiceProduct != null && timeCode.TimeCodeInvoiceProduct.Count > 0;
                }

                return timeCodeAdditionDeductions;
            }
            else
            {
                var timeCodeAdditionDeductions = (from tc in entities.TimeCode.OfType<TimeCodeAdditionDeduction>()
                                                  where tc.Type == (int)SoeTimeCodeType.AdditionDeduction &&
                                                  tc.ActorCompanyId == actorCompanyId &&
                                                  (!isMySelf || !tc.HideForEmployee) &&
                                                  tc.State == (int)SoeEntityState.Active
                                                  select tc).ToList();

                return timeCodeAdditionDeductions;
            }

        }

        #endregion

        #region TimeCodeBreakGroup

        public TimeCodeBreakDTO GetTimeCodeBreakDTOForEmployeeGroup(int employeeGroupId, int timeCodeBreakGroupId)
        {
            string key = $"GetTimeCodeBreakForEmployeeGroup#{employeeGroupId}#{timeCodeBreakGroupId}";

            var value = BusinessMemoryCache<TimeCodeBreakDTO>.Get(key);

            if (value != null)
                return value;

            var code = GetTimeCodeBreakForEmployeeGroup(employeeGroupId, timeCodeBreakGroupId);

            if (code != null)
            {
                var dto = code.ToBreakDTO();
                BusinessMemoryCache<TimeCodeBreakDTO>.Set(key, dto);
                return dto;
            }
            return null;
        }

        public TimeCodeBreak GetTimeCodeBreakForEmployeeGroup(int employeeGroupId, int timeCodeBreakGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodeBreakGroup.NoTracking();
            return GetTimeCodeBreakForEmployeeGroup(entities, employeeGroupId, timeCodeBreakGroupId);
        }

        public TimeCodeBreak GetTimeCodeBreakForEmployeeGroup(CompEntities entities, int employeeGroupId, int timeCodeBreakGroupId)
        {
            return (from g in entities.TimeCode.OfType<TimeCodeBreak>()
                    where g.TimeCodeBreakGroupId == timeCodeBreakGroupId &&
                    g.EmployeeGroupsForBreak.Any(i => i.EmployeeGroupId == employeeGroupId)
                    select g).FirstOrDefault();
        }

        public TimeCodeBreak GetTimeCodeBreakForLength(int actorCompanyId, int employeeGroupId, int length, List<TimeCodeBreak> timeCodeBreaks = null, List<TimeCodeBreakGroup> timeCodeBreakGroups = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodeBreakGroup.NoTracking();
            return GetTimeCodeBreakForLength(entities, actorCompanyId, employeeGroupId, length, timeCodeBreaks, timeCodeBreakGroups);
        }

        public TimeCodeBreak GetTimeCodeBreakForLength(CompEntities entities, int actorCompanyId, int employeeGroupId, int length, List<TimeCodeBreak> timeCodeBreaks = null, List<TimeCodeBreakGroup> timeCodeBreakGroups = null)
        {
            if (timeCodeBreakGroups == null)
                timeCodeBreakGroups = GetTimeCodeBreakGroups(entities, actorCompanyId);

            if (timeCodeBreaks == null)
            {
                foreach (TimeCodeBreakGroup timeCodeBreakGroup in timeCodeBreakGroups)
                {
                    TimeCodeBreak timeCodeBreak = (from g in entities.TimeCode.OfType<TimeCodeBreak>()
                                                   where g.TimeCodeBreakGroupId == timeCodeBreakGroup.TimeCodeBreakGroupId &&
                                                   g.DefaultMinutes == length &&
                                                   g.EmployeeGroupsForBreak.Any(i => i.EmployeeGroupId == employeeGroupId)
                                                   select g).FirstOrDefault();

                    if (timeCodeBreak != null)
                        return timeCodeBreak;
                }
            }
            else
            {
                foreach (TimeCodeBreakGroup timeCodeBreakGroup in timeCodeBreakGroups)
                {
                    TimeCodeBreak timeCodeBreak = (from g in timeCodeBreaks
                                                   where g.TimeCodeBreakGroupId == timeCodeBreakGroup.TimeCodeBreakGroupId &&
                                                   g.DefaultMinutes == length &&
                                                   g.EmployeeGroupsForBreak.Any(i => i.EmployeeGroupId == employeeGroupId)
                                                   select g).FirstOrDefault();

                    if (timeCodeBreak != null)
                        return timeCodeBreak;
                }
            }

            return null;
        }

        public List<TimeCodeBreakGroup> GetTimeCodeBreakGroups(int actorCompanyId, int? timeCodeBreakId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodeBreakGroup.NoTracking();
            return GetTimeCodeBreakGroups(entities, actorCompanyId, timeCodeBreakId);
        }

        public List<TimeCodeBreakGroup> GetTimeCodeBreakGroups(CompEntities entities, int actorCompanyId, int? timeCodeBreakId = null)
        {
            IQueryable<TimeCodeBreakGroup> query = (from g in entities.TimeCodeBreakGroup
                                                    .Include("TimeCodeBreak")
                                                    where g.ActorCompanyId == actorCompanyId &&
                                                    g.State == (int)SoeEntityState.Active
                                                    select g);
            if (timeCodeBreakId.HasValue)
                query = query.Where(tcbg => tcbg.TimeCodeBreakGroupId == timeCodeBreakId.Value);

            return query.OrderBy(g => g.Name).ToList();
        }

        public Dictionary<int, string> GetTimeCodeBreakGroupsDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var timeCodeBreakGroups = GetTimeCodeBreakGroups(actorCompanyId);
            foreach (var timeCodeBreakGroup in timeCodeBreakGroups.OrderBy(i => i.Name))
            {
                dict.Add(timeCodeBreakGroup.TimeCodeBreakGroupId, timeCodeBreakGroup.Name);
            }

            return dict.Sort();
        }

        public TimeCodeBreakGroup GetTimeCodeBreakGroup(int timeCodeBreakGroupId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodeBreakGroup.NoTracking();
            return GetTimeCodeBreakGroup(entities, timeCodeBreakGroupId, actorCompanyId);
        }

        public TimeCodeBreakGroup GetTimeCodeBreakGroup(CompEntities entities, int timeCodeBreakGroupId, int actorCompanyId)
        {
            return (from g in entities.TimeCodeBreakGroup
                    where g.ActorCompanyId == actorCompanyId &&
                    g.TimeCodeBreakGroupId == timeCodeBreakGroupId
                    select g).FirstOrDefault();
        }

        public ActionResult SaveTimeCodeBreakGroup(TimeCodeBreakGroupDTO timeCodeBreakGroupInput, int actorCompanyId)
        {
            if (timeCodeBreakGroupInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeCodeBreakGroupDTO");

            // Default result is successful
            ActionResult result = new ActionResult();

            int timeCodeBreakGroupId = timeCodeBreakGroupInput.TimeCodeBreakGroupId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region TimeCodeBreakGroup

                        // Get existing shift type
                        TimeCodeBreakGroup timeCodeBreakGroup = GetTimeCodeBreakGroup(entities, timeCodeBreakGroupId, actorCompanyId);

                        if (timeCodeBreakGroup == null)
                        {
                            #region Add

                            timeCodeBreakGroup = new TimeCodeBreakGroup()
                            {
                                Name = timeCodeBreakGroupInput.Name,
                                Description = timeCodeBreakGroupInput.Description,
                                State = (int)timeCodeBreakGroupInput.State,

                                //Set FK
                                ActorCompanyId = actorCompanyId,
                            };

                            SetCreatedProperties(timeCodeBreakGroup);
                            entities.TimeCodeBreakGroup.AddObject(timeCodeBreakGroup);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            // Update ShiftType
                            timeCodeBreakGroup.Name = timeCodeBreakGroupInput.Name;
                            timeCodeBreakGroup.Description = timeCodeBreakGroupInput.Description;
                            timeCodeBreakGroup.State = (int)timeCodeBreakGroupInput.State;

                            SetModifiedProperties(timeCodeBreakGroup);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            timeCodeBreakGroupId = timeCodeBreakGroup.TimeCodeBreakGroupId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = timeCodeBreakGroupId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteTimeCodeBreakGroup(int timeCodeBreakGroupId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeCodeBreakGroup timeCodeBreakGroup = GetTimeCodeBreakGroup(entities, timeCodeBreakGroupId, actorCompanyId);
                if (timeCodeBreakGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeCodeBreakGroup");

                return ChangeEntityState(entities, timeCodeBreakGroup, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region TimeCodeInvoiceProduct

        public IEnumerable<TimeCodeInvoiceProduct> GetTimeCodeInvoiceProducts(int timeCodeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodeInvoiceProduct.NoTracking();
            return GetTimeCodeInvoiceProducts(entities, timeCodeId);
        }

        public IEnumerable<TimeCodeInvoiceProduct> GetTimeCodeInvoiceProducts(CompEntities entities, int timeCodeId)
        {
            return (from tcip in entities.TimeCodeInvoiceProduct.Include("InvoiceProduct")
                    where tcip.TimeCode.TimeCodeId == timeCodeId
                    select tcip).ToList<TimeCodeInvoiceProduct>();
        }

        #endregion

        #region TimeCodePayrollProduct

        public List<TimeCodePayrollProduct> GetTimeCodePayrollProducts(int timeCodeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodePayrollProduct.NoTracking();
            return GetTimeCodePayrollProducts(entities, timeCodeId);
        }

        public List<TimeCodePayrollProduct> GetTimeCodePayrollProducts(CompEntities entities, int timeCodeId)
        {
            return (from tcip in entities.TimeCodePayrollProduct.Include("PayrollProduct")
                    where tcip.TimeCode.TimeCodeId == timeCodeId
                    select tcip).ToList<TimeCodePayrollProduct>();
        }

        #endregion

        #region TimeCodeRankingGroup

        public List<TimeCodeRankingGroup> GetTimeCodeRankingGroups(int? actorCompanyId = null, int? timeCodeRankingGroupId = null, bool loadRankings = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodeRankingGroup.NoTracking();
            return GetTimeCodeRankingGroups(entities, actorCompanyId, timeCodeRankingGroupId, loadRankings);
        }

        public List<TimeCodeRankingGroup> GetTimeCodeRankingGroups(CompEntities entities, int? actorCompanyId = null, int? timeCodeRankingGroupId = null, bool loadRankings = false)
        {
            if (!actorCompanyId.HasValue)
                actorCompanyId = ActorCompanyId;

            var query = entities.TimeCodeRankingGroup
                .Where(w =>
                    w.ActorCompanyId == actorCompanyId.Value
                    && w.State == (int)SoeEntityState.Active &&
                    (!timeCodeRankingGroupId.HasValue || w.TimeCodeRankingGroupId == timeCodeRankingGroupId.Value));

            if (loadRankings)
                query = query.Include("TimeCodeRanking");

            return query.ToList();
        }

        public TimeCodeRankingGroupDTO GetTimeCodeRankingGroupDTO(int timeCodeRankingGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodeRankingGroup.NoTracking();
            return GetTimeCodeRankingGroupDTO(entities, timeCodeRankingGroupId);
        }

        public TimeCodeRankingGroupDTO GetTimeCodeRankingGroupDTO(CompEntities entities, int timeCodeRankingGroupId)
        {
            var rankingGroup = GetTimeCodeRankingGroup(entities, timeCodeRankingGroupId, loadTimeCodeRanking: true);
            if (rankingGroup == null)
                return null;

            var timeCodeRankingsGrouped = GetTimeCodeRankingsGrouped(rankingGroup);
            return rankingGroup.ToDTO(timeCodeRankingsGrouped);
        }

        public TimeCodeRankingGroup GetTimeCodeRankingGroup(CompEntities entities, int timeCodeRankingGroupId, bool loadTimeCodeRanking = false)
        {
            var query = entities.TimeCodeRankingGroup
                .Where(tcrg =>
                    tcrg.TimeCodeRankingGroupId == timeCodeRankingGroupId &&
                    tcrg.ActorCompanyId == ActorCompanyId &&
                    tcrg.State == (int)SoeEntityState.Active);

            if (loadTimeCodeRanking)
                query = query.Include("TimeCodeRanking");

            return query.FirstOrDefault();
        }

        #endregion

        #region TimeCodeRanking

        public List<TimeCodeRanking> GetTimeCodeRankingsFromGroup(CompEntities entities, int timeCodeRankingGroupId, int actorCompanyId)
        {
            return entities.TimeCodeRanking
                .Where(tcr => 
                    tcr.ActorCompanyId == actorCompanyId && 
                    tcr.TimeCodeRankingGroupId == timeCodeRankingGroupId && 
                    tcr.State == (int)SoeEntityState.Active)
                .ToList();
        }

        public List<TimeCodeRankingDTO> GetTimeCodeRankingsGrouped(TimeCodeRankingGroup existingRankingGroup)
        {
            var existingRankings = existingRankingGroup?.TimeCodeRanking.Where(w => w.State == (int)SoeEntityState.Active).ToList();
            if (existingRankings.IsNullOrEmpty())
                return new List<TimeCodeRankingDTO>();

            var leftIds = existingRankings.Select(r => r.LeftTimeCodeId);
            var rightIds = existingRankings.Select(r => r.RightTimeCodeId);
            var allIds = leftIds.Concat(rightIds).Distinct().ToList();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var timeCodeDict = entitiesReadOnly.TimeCode
                .Where(tc => allIds.Contains(tc.TimeCodeId))
                .Select(tc => new { tc.TimeCodeId, tc.Name, tc.Code })
                .ToDictionary(x => x.TimeCodeId, x => new { x.Name, x.Code });

            var grouped = existingRankings
                .GroupBy(r => new { r.LeftTimeCodeId, r.OperatorType })
                .Select(g =>
                {
                    var first = g.First();
                    if (!timeCodeDict.TryGetValue(g.Key.LeftTimeCodeId, out var leftName))
                        leftName = new { Name = string.Empty, Code = string.Empty };

                    var rightNames = g
                        .Select(r =>
                        {
                            return timeCodeDict.TryGetValue(r.RightTimeCodeId, out var rn) ? rn.Code : null;
                        })
                        .Where(n => !string.IsNullOrEmpty(n))
                        .Distinct()
                        .OrderBy(n => n)
                        .ToList();

                    return new TimeCodeRankingDTO
                    {
                        TimeCodeRankingId = first.TimeCodeRankingId,
                        ActorCompanyId = ActorCompanyId,
                        LeftTimeCodeId = g.Key.LeftTimeCodeId,
                        LeftTimeCodeName = leftName.Name,
                        OperatorType = (TermGroup_TimeCodeRankingOperatorType)first.OperatorType,
                        RightTimeCodeNames = rightNames,
                        RightTimeCodeIds = g.Select(r => r.RightTimeCodeId)
                                            .Where(id => id > 0)
                                            .Distinct()
                                            .OrderBy(id => id)
                                            .ToList(),
                    };
                })
                .OrderBy(d => d.LeftTimeCodeName)
                .ThenBy(d => d.OperatorType)
                .ToList();

            return grouped;
        }

        /// <summary>
        /// Determines which TimeCodeRanking rows will be deleted based on the incoming DTOs.
        /// A ranking row is deleted if:
        /// 1. Its LeftTimeCodeId is not present in any incoming DTO.
        /// 2. Its (LeftTimeCodeId, RightTimeCodeId) pair is not represented among the incoming DTO's RightTimeCodeIds for that Left.
        /// OperatorType differences do NOT trigger deletion (they are handled as updates elsewhere).
        /// </summary>
        private List<TimeCodeRanking> GetTimeCodeRankingsToDelete(TimeCodeRankingGroup existingRankingGroup, TimeCodeRankingGroupDTO inputRankingGroup, bool isDelete)
        {
            if (existingRankingGroup == null)
                return new List<TimeCodeRanking>();

            var inputRankings = isDelete
                ? inputRankingGroup.TimeCodeRankings
                : new List<TimeCodeRankingDTO>();

            var inputLeftIds = inputRankings
                .Where(d => d.LeftTimeCodeId > 0)
                .Select(d => d.LeftTimeCodeId)
                .Distinct()
                .ToHashSet();

            // Map of Left -> set of valid Right ids from input
            var inputLeftRightMap = inputRankings
                .Where(d => d.LeftTimeCodeId > 0 && !d.RightTimeCodeIds.IsNullOrEmpty())
                .GroupBy(d => d.LeftTimeCodeId)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .SelectMany(d => d.RightTimeCodeIds.Where(id => id > 0))
                        .Distinct()
                        .ToHashSet()
                );

            var toDelete = new List<TimeCodeRanking>();

            foreach (var existingRanking in existingRankingGroup.TimeCodeRanking.Where(r => r.State == (int)SoeEntityState.Active))
            {
                // Left removed entirely
                if (!inputLeftIds.Contains(existingRanking.LeftTimeCodeId))
                {
                    toDelete.Add(existingRanking);
                    continue;
                }

                // Left still exists; check Right pair
                if (!inputLeftRightMap.TryGetValue(existingRanking.LeftTimeCodeId, out var rightSet) || !rightSet.Contains(existingRanking.RightTimeCodeId))
                {
                    toDelete.Add(existingRanking);
                }
            }

            return toDelete;
        }

        public bool IsTimeCodeUsedInRankings(CompEntities entities, int timeCodeId)
        {
            int actorCompanyId = ActorCompanyId;
            return entities.TimeCodeRanking
                .Any(tcr =>
                    tcr.ActorCompanyId == actorCompanyId &&
                    tcr.State == (int)SoeEntityState.Active &&
                    (tcr.LeftTimeCodeId == timeCodeId || tcr.RightTimeCodeId == timeCodeId));
        }

        public bool IsTimeCodeRankingGroupsOverlapping(TimeCodeRankingGroupDTO inputRankingGroup)
        {
            var existingRankingGroups = GetTimeCodeRankingGroups();
            foreach (var existingRankingGroup in existingRankingGroups)
            {
                if (existingRankingGroup.TimeCodeRankingGroupId != inputRankingGroup.TimeCodeRankingGroupId && CalendarUtility.IsDatesOverlappingNullable(
                    inputRankingGroup.StartDate,
                    inputRankingGroup.StopDate,
                    existingRankingGroup.StartDate,
                    existingRankingGroup.StopDate,
                    validateDatesAreTouching: true))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Pre-save validation for TimeCodeRanking changes.
        /// Only checks TimeCodeTransactions that reference (FK TimeCodeRankingId) the ranking rows that would be deleted.
        /// Blocking condition: Active TimeCodeTransaction exists with TimeCodeRankingId pointing to a ranking slated for deletion.
        /// </summary>
        public ActionResult ValidateTimeCodeRankingGroup(TimeCodeRankingGroupDTO inputRankingGroup, bool isDelete)
        {
            const int maxDateIntervalsPerEmployee = 10;
            string defaultTextIfMoreThanMax = string.Format(GetText(110689, "Det finns fler än {0} datumintervall för denna anställd"), maxDateIntervalsPerEmployee);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (IsTimeCodeRankingGroupsOverlapping(inputRankingGroup))
                        return new ActionResult(GetText(110696, "Tidskodsviktningar får ej överlappa varandra"));

                    var existingRankingGroup = GetTimeCodeRankingGroup(entities, inputRankingGroup.TimeCodeRankingGroupId, loadTimeCodeRanking: true);
                    if (existingRankingGroup == null)
                        return new ActionResult(true);

                    var existingRankingsToDelete = GetTimeCodeRankingsToDelete(existingRankingGroup, inputRankingGroup, isDelete);
                    if (existingRankingsToDelete.IsNullOrEmpty())
                        return new ActionResult(true);

                    var existingRankingIdsToDelete = existingRankingsToDelete
                        .Select(r => r.TimeCodeRankingId)
                        .Distinct()
                        .ToList();

                    // Only look at transactions referencing the ranking rows to be deleted
                    var existingTransactions = entities.TimeCodeTransaction
                        .Where(tct =>
                            tct.State == (int)SoeEntityState.Active &&
                            tct.TimeCodeRankingId.HasValue &&
                            existingRankingIdsToDelete.Contains(tct.TimeCodeRankingId.Value)
                        )
                        .Include(tct => tct.TimeBlockDate)
                        .ToList();

                    if (existingTransactions.IsNullOrEmpty())
                        return new ActionResult(true);

                    var result = new ActionResult(GetText(110689, "Det finns ersättnings transaktioner kopplade till tidskodsviktningen. Dessa dagar behöver räknas om manuellt om du genomför ändringarna"))
                    {
                        Strings = new List<string>(),
                    };

                    foreach (var existingTransactionsByEmployee in existingTransactions.Where(w => w.TimeBlockDate != null).GroupBy(tct => tct.TimeBlockDate.EmployeeId))
                    {
                        var employee = EmployeeManager.GetEmployee(entities, existingTransactionsByEmployee.Key, ActorCompanyId, loadContactPerson: true);
                        if (employee == null)
                            continue;

                        string intervals = existingTransactionsByEmployee
                            .Select(tct => tct.TimeBlockDate.Date)
                            .GetCohereDateRangesText(maxDateIntervalsPerEmployee, defaultTextIfMoreThanMax);

                        result.Strings.Add($"{employee.EmployeeNrAndName} {intervals}");
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    LogError(ex, this.log);
                    return new ActionResult(ex);
                }
            }
        }

        /// <summary>
        /// Persists TimeCodeRanking changes:
        /// - Deletes rankings whose LeftTimeCodeId was removed.
        /// - Deletes individual (Left,Right) pairs no longer present.
        /// - Adds new (Left,Right) pairs.
        /// - Updates OperatorType for existing pairs if changed.
        /// Uses same deletion candidate logic as validation for consistency.
        /// </summary>
        public ActionResult SaveTimeCodeRankingsGroup(TimeCodeRankingGroupDTO inputRankingGroup)
        {
            if (inputRankingGroup == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeCodeRankingDTO");

            int actorCompanyId = ActorCompanyId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var rankingGroup = GetTimeCodeRankingGroup(entities, inputRankingGroup.TimeCodeRankingGroupId, loadTimeCodeRanking: true);
                        if (rankingGroup == null)
                        {
                            #region Add

                            rankingGroup = new TimeCodeRankingGroup()
                            {
                                State = (int)SoeEntityState.Active,
                            };
                            SetCreatedProperties(rankingGroup);
                            entities.AddObject("TimeCodeRankingGroup", rankingGroup);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(rankingGroup);

                            #endregion
                        }

                        rankingGroup.Description = inputRankingGroup.Description;
                        rankingGroup.ActorCompanyId = actorCompanyId;
                        rankingGroup.StartDate = inputRankingGroup.StartDate;
                        rankingGroup.StopDate = inputRankingGroup.StopDate;

                        #region TimeCodeRanking

                        var flattened = inputRankingGroup.TimeCodeRankings
                            .Where(d => d.LeftTimeCodeId > 0 && d.RightTimeCodeIds != null)
                            .SelectMany(d => d.RightTimeCodeIds
                                .Where(rid => rid > 0)
                                .Select(rid => new
                                {
                                    LeftId = d.LeftTimeCodeId,
                                    RightId = rid,
                                    OperatorType = (int)d.OperatorType
                                }))
                            .Distinct()
                            .ToList();

                        var inputLeftIds = inputRankingGroup.TimeCodeRankings
                            .Where(d => d.LeftTimeCodeId > 0)
                            .Select(d => d.LeftTimeCodeId)
                            .Distinct()
                            .ToList();

                        var existingRankings = GetTimeCodeRankingsFromGroup(entities, rankingGroup.TimeCodeRankingGroupId, actorCompanyId);

                        var leftToDelete = existingRankings
                            .Where(r => !inputLeftIds.Contains(r.LeftTimeCodeId))
                            .ToList();

                        foreach (var ranking in leftToDelete)
                        {
                            ranking.Delete();
                            SetModifiedProperties(ranking);
                        }

                        var targetMap = flattened
                            .GroupBy(x => new { x.LeftId, x.RightId })
                            .ToDictionary(g => g.Key, g => g.First().OperatorType);

                        var existingMap = existingRankings
                            .Where(r => inputLeftIds.Contains(r.LeftTimeCodeId))
                            .ToDictionary(r => new { LeftId = r.LeftTimeCodeId, RightId = r.RightTimeCodeId }, r => r);

                        foreach (var kvp in targetMap)
                        {
                            if (!existingMap.TryGetValue(kvp.Key, out TimeCodeRanking timeCodeRanking))
                            {
                                var newTimeCodeRanking = TimeCodeRanking.Create(actorCompanyId, kvp.Key.LeftId, kvp.Key.RightId, kvp.Value, rankingGroup.TimeCodeRankingGroupId);
                                SetCreatedProperties(newTimeCodeRanking);
                                entities.TimeCodeRanking.AddObject(newTimeCodeRanking);
                            }
                            else if (timeCodeRanking.OperatorType != kvp.Value)
                            {
                                timeCodeRanking.Update(kvp.Value);
                                SetModifiedProperties(timeCodeRanking);
                            }
                        }

                        var rightToDelete = existingMap
                            .Where(e => !targetMap.ContainsKey(e.Key))
                            .Select(e => e.Value)
                            .ToList();

                        foreach (var ranking in rightToDelete)
                        {
                            ranking.Delete();
                            SetModifiedProperties(ranking);
                        }

                        #endregion

                        var result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            if (result != null && result.Success)
                                result.IntegerValue = rankingGroup.TimeCodeRankingGroupId;

                            transaction.Complete();
                        }

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, this.log);
                    return new ActionResult(ex);
                }
                finally
                {
                    entities.Connection.Close();
                }
            }
        }

        public ActionResult DeleteTimeCodeRankingGroup(int timeCodeRankingGroupId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ActionResult result;

                var inputRankingGroup = GetTimeCodeRankingGroupDTO(entities, timeCodeRankingGroupId);
                if (inputRankingGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(110695, "Tidskodviktning hittades inte"));

                var existingRankingGroup = GetTimeCodeRankingGroup(entities, timeCodeRankingGroupId, loadTimeCodeRanking: true);
                if (existingRankingGroup?.TimeCodeRanking == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(110695, "Tidskodviktning hittades inte"));

                foreach (var existingTimeCodeRanking in existingRankingGroup.TimeCodeRanking)
                {
                    result = ChangeEntityState(entities, existingTimeCodeRanking, SoeEntityState.Deleted, saveChanges: false);
                    if (!result.Success)
                        return result;
                }
                result = ChangeEntityState(entities, existingRankingGroup, SoeEntityState.Deleted, saveChanges: false);
                if (!result.Success)
                    return result;

                result = SaveChanges(entities);

                return result;
            }
        }

        #endregion
    }
}
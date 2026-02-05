using SoftOne.Soe.Business.Core.TimerConvert.TimerDTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimerConvert
{
    public class TimerConvertManager : ManagerBase
    {
        #region Enums

        private enum TimerDayTypes
        {
            Vardag = 0,
            LördagAfton = 1,
            SöndagHelgdag = 2,
            Egen = 3
        }

        private enum XeType
        {
            EmployeeCategory = 1,
            EmployeeGroup = 2,
            Employee = 3,
            TimeTerminal = 4,
            TimeScheduleTemplateHead = 5,
            TimeScheduleTemplatePeriod = 6,
            TimeCode = 7,
            DayType = 8,
            AttestState = 9,
            PayrollProduct = 10,
        }

        #endregion

        #region Constants

        //Stored procedures located in wt database
        /*
        private const string UPDATE_CONVERT_STATUS_TO_SUCCESS = "Xe_UpdateConvertedStatus";
        private const string GET_ATTESTROLEUSERS = "Xe_GetAttestRoleUsers";
        private const string GET_ATTESTSTATES = "Xe_GetAttestStates";
        private const string GET_COMPANY = "Xe_GetCompany";
        private const string GET_COMPANY_ID = "Xe_GetCompanyIdToConvert";
        private const string GET_COUPLEDCOMPANIES = "Xe_GetCoupledCompanies";
        private const string GET_CATEGORIES = "Xe_GetCategories";
        private const string GET_DAYTYPES = "Xe_GetDayTypes";
        private const string GET_EMPLOYEES = "Xe_GetEmployees";
        private const string GET_EMPLOYEESCHEDULE = "Xe_GetEmployeeSchedule";
        private const string GET_EMPLOYEE_GROUPS = "Xe_GetEmployeeGroups";
        private const string GET_EMPLOYEE_GROUP_TIMEDEVIATIONCAUSES = "Xe_GetEmployeeGroupTimeDeviationCauses";
        private const string GET_EMPLOYEES_OBID = "Xe_GetEmployeeOBId";
        private const string GET_FLEXFORCE_APIKEY = "XE_GetFlexForceAPIKey";
        private const string GET_FLEXFORCE_SYNC = "Xe_GetFlexForceSync";
        private const string GET_HOLIDAYS = "Xe_GetHolidays";
        private const string GET_PAYROLLPRODUCTS = "Xe_GetPayrollProducts";
        private const string GET_SALARY_EXPORTS = "Xe_GetSalaryExports";
        private const string GET_SALARY_EXPORT_ROWS = "Xe_GetSalaryExportRows";
        private const string GET_TERMINALS = "Xe_GetTerminals";
        private const string GET_TIMEACCUMULATOR = "Xe_GetTimeAccumulator";
        private const string GET_TIMEACCUMULATOR_TRANSACTIONS = "Xe_GetTimeAccumulatorTransactions";
        private const string GET_TIMEBLOCKS = "Xe_GetTimeBlocks";
        private const string GET_TIMECODEBREAKS = "Xe_GetTimeCodeBreaks";
        private const string GET_TIMEDEVIATIONCAUSES_ABSENCE = "Xe_GetAbsenceDeviationCauses";
        private const string GET_TIMEDEVIATIONCAUSES_PRESENCE = "Xe_GetPresenceDeviationCauses";
        private const string GET_TIMEPERIODS = "Xe_GetTimePeriods";
        private const string GET_TIMESCHEDULETEMPLATEHEAD = "Xe_GetTimeScheduleTemplateHead";
        private const string GET_TIMESCHEDULETEMPLATEPERIOD = "Xe_GetTimeScheduleTemplatePeriod";
        private const string GET_TIMESCHEDULETEMPLATEBLOCKS = "Xe_GetTimeTemplateBlocks";
        private const string GET_TIMESCHEDULETEMPLATEBLOCKS_EMPLOYEE = "Xe_GetTimeTemplateBlocksEmployee";
        private const string GET_TIMESTAMPS = "Xe_GetTimeStamps";
        private const string GET_TIMEPAYROLLTRANSACTIONS = "Xe_GetTimePayrollTransactions";
        private const string GET_UNITS = "Xe_GetUnits";

        private const string WT_ATTESTSTATE_REGISTERED = "Registerad";
        private const string WT_ATTESTSTATE_DONE = "Klar";
        private const string WT_ATTESTSTATE_APPROVED = "Godkänd";
        private const string WT_ATTESTSTATE_ATTESTED = "Attesterad";
        private const string WT_ATTESTSTATE_DOWNLOADED = "Lön";

        private const int WT_TIMECODE_BREAK = -1;
        private const int WT_TIMECODE_WORK = -2;
        */
        #endregion

        #region Variables

        private int? batchNr = null;
        private int? sysScheduledJobId = null;
        private NameStandard nameStandard = NameStandard.Unknown;

        #region Default/Standard entity's
        /*
        /// <summary>Contains the AcountDim standard for the Company</summary>
        private AccountDim accountDimStd = null;
        /// <summary>Contains the default EmployeeGroup, to be able to set generated p-key to settings</summary>
        private EmployeeGroup defaultEmployeeGroup = null;
        //Sys User
        private User sysUser = null;
        //Admin AttestRole
        private AttestRole adminAttestRole = null;
        */
        #endregion

        #region Dictionaries
        /*
        private readonly Dictionary<int, EmployeeGroup> exportedEmployeeGroupsDict = new Dictionary<int, EmployeeGroup>();
        private readonly Dictionary<int, Employee> exportedEmployeesDict = new Dictionary<int, Employee>();
        private readonly Dictionary<int, Category> exportedEmployeeCategoriesDict = new Dictionary<int, Category>();
        private readonly Dictionary<int, DayType> exportedDayTypesDict = new Dictionary<int, DayType>();
        private readonly Dictionary<int, AttestState> exportedAttestStatesDict = new Dictionary<int, AttestState>();
        private readonly Dictionary<int, PayrollProduct> exportedPayrollProductsDict = new Dictionary<int, PayrollProduct>();
        private readonly Dictionary<int, TimeCode> exportedTimeCodesDict = new Dictionary<int, TimeCode>();
        private readonly Dictionary<string, TimeCodeWork> exportedTimeCodesTimeAccumulatorDict = new Dictionary<string, TimeCodeWork>();
        private readonly Dictionary<string, TimeAccumulator> exportedTimeAccumulatorsDict = new Dictionary<string, TimeAccumulator>();
        private readonly Dictionary<int, TimeTerminal> exportedTimeTerminalsDict = new Dictionary<int, TimeTerminal>();
        private readonly Dictionary<TimeSalaryExport, List<int>> exportedTimeSalaryExportTransactionsDict = new Dictionary<TimeSalaryExport, List<int>>();
        private readonly Dictionary<int, TimeScheduleTemplateHead> exportedTimeScheduleTemplateHeadsDict = new Dictionary<int, TimeScheduleTemplateHead>();
        private readonly Dictionary<int, TimeScheduleTemplatePeriod> exportedTimeScheduleTemplatePeriodsDict = new Dictionary<int, TimeScheduleTemplatePeriod>();
        private readonly Dictionary<int, Dictionary<DateTime, TimeBlockDate>> exportedTimeBlockDatesDict = new Dictionary<int, Dictionary<DateTime, TimeBlockDate>>();
        */
        #endregion

        #endregion

        #region Ctor

        public TimerConvertManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Flow

        public ActionResult ExecuteTimerConversion(string timerConnectionString, User serviceUser, string siteName, int actorCompanyId, int? batchNr = null, int? sysScheduledJobId = null, bool noTransactions = false, bool noSchedules = false, bool limitholidays = false, bool staffing = false, NameStandard nameStandard = NameStandard.FirstNameThenLastName)
        {
            #region Init

            var result = new ActionResult(false);

            this.batchNr = batchNr;
            this.sysScheduledJobId = sysScheduledJobId;
            this.nameStandard = nameStandard;
            //DateTime conversionDate = DateTime.Now;

            base.parameterObject = ParameterObject.Create(user: new Common.DTO.UserDTO
            {
                Name = serviceUser.Name,
                LoginName = serviceUser.LoginName,
                UserId = serviceUser.UserId
            });

            #endregion

            #region Get Sys data

            #endregion

            //Connect to XE database
            using (CompEntities entities = new CompEntities())
            {
                // Get dictionaries

                Tbl2AvtalFactory tbl2AvtalFactory = new Tbl2AvtalFactory(timerConnectionString, entities, actorCompanyId);
                List<Tbl2AvtalDTO> avtList = tbl2AvtalFactory.GetAll();


                // Import data from Timer to GO

                TblButikFactory tblButikFactory = new TblButikFactory(timerConnectionString, entities, actorCompanyId);
                var accountSaveResult = tblButikFactory.SaveGOAccounts(false, out List<TblButikDTO> tblButikDTOs);
                if (!accountSaveResult.Success)
                    return accountSaveResult;

                
                TblPersonalFactory tblPersonalFactory = new TblPersonalFactory(timerConnectionString, entities, actorCompanyId);
                var employeeSaveResult = tblPersonalFactory.SaveGOEmployees(false, out List<TblPersonalDTO> tblPersonalDTOs);
                if (!employeeSaveResult.Success)
                    return employeeSaveResult;
                

                Tbl2AnstArkivFactory tblAnstArkivFactory = new Tbl2AnstArkivFactory(timerConnectionString, entities, actorCompanyId);

                foreach (TblPersonalDTO tblPersonalDTO in tblPersonalDTOs)
                {
                    foreach (PersonalButikItem personalButikRelation in tblPersonalDTO.PersonalButikRelations)
                    {
                        // List<Tbl2AnstArkivDTO> anstList = tblAnstArkivFactory.GetForPersonalAndButik(4, 1, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31), avtList);
                        List<Tbl2AnstArkivDTO> anstList = tblAnstArkivFactory.GetForPersonalAndButik(personalButikRelation.PersonalID, personalButikRelation.ButikID, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31), avtList);

                        // TODO: Try to sort and order the employments. Perhaps just store the home-units emplyments?

                        int GOAccountId = tblButikDTOs.FirstOrDefault(a => a.ButikID == personalButikRelation.ButikID).GOAccountId;
                        int GOEmployeeId = tblPersonalDTO.GOEmployeeId;

                        // Continue here...

                    }
                }

                return result;
            }
        }

        #endregion

        #region Logging

        protected void CreateLogEntry(string message)
        {
            if (this.batchNr.HasValue && this.sysScheduledJobId.HasValue)
                SysScheduledJobManager.CreateLogEntry((int)this.sysScheduledJobId, (int)this.batchNr, ScheduledJobLogLevel.Information, message);
        }

        #endregion
    }
}

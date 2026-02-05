using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SoftOne.Soe.Business.Core
{
    public class WtConvertManager : ManagerBase
    {
        #region Enums

        private enum WtDayTypes
        {
            Helgdag = 1,
            Vardag = 2,
            Lördag = 3,
            Söndag = 4
        }

        private enum WtHalfDayRules
        {
            Clock = 0,
            RelativeScheduleIn = 1,
            RelativeScheduleOut = 2,
        }

        /// <summary>
        /// tblWTAnv.anv_niva
        /// </summary>
        private enum WtRoles
        {
            Employee = 0,
            Approval = 1,
            Attest = 2,
            Admin = 3,
            SuperAdmin = 10,
        }

        /// <summary>
        /// tblWTStatus.status_nr
        /// </summary>
        private enum WtAttestStates
        {
            Registered = 10,
            Done = 15,
            Approved = 20,
            Attested = 25,
            Downloaded = 30,
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

        #endregion

        #region Variables

        private int? batchNr = null;
        private int? sysScheduledJobId = null;
        private NameStandard nameStandard = NameStandard.Unknown;

        #region Default/Standard entity's

        /// <summary>Contains the AcountDim standard for the Company</summary>
        private AccountDim accountDimStd = null;
        /// <summary>Contains the default EmployeeGroup, to be able to set generated p-key to settings</summary>
        private EmployeeGroup defaultEmployeeGroup = null;
        //Sys User
        private User sysUser = null;
        //Admin AttestRole
        private AttestRole adminAttestRole = null;

        #endregion

        #region Dictionaries

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

        #endregion

        #region Lists

        //DayType
        private readonly List<DayType> exportedDayTypesSpecial = new List<DayType>();
        private readonly List<DayType> exportedDayTypes = new List<DayType>();
        private readonly List<UserCompanyRole> exportedUserCompanyRoles = new List<UserCompanyRole>();

        /// <summary>Contains the relation between AttestRole and its Category, to be able map User to correct AttestRole</summary>
        private readonly List<AttestRoleCategoryStateObject> attestRole_Category_States = new List<AttestRoleCategoryStateObject>();
        /// <summary>Contains the relation between Employee and its Category, to be able to set generated p-keys to settings</summary>
        private readonly List<EmployeeCategoryStateObject> employee_Category_States = new List<EmployeeCategoryStateObject>();

        #endregion

        #endregion

        #region Ctor

        public WtConvertManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Flow

        public ActionResult ExecuteWTConversion(string wtConnectionString, int wtCompanyId, User serviceUser, string siteName, DateTime fromDate, DateTime toDate, int? actorCompanyId, int? batchNr = null, int? sysScheduledJobId = null, bool noTransactions = false, bool noSchedules = false, bool limitholidays = false, bool staffing = false, NameStandard nameStandard = NameStandard.FirstNameThenLastName)
        {
            #region Init

            var result = new ActionResult(false);

            this.batchNr = batchNr;
            this.sysScheduledJobId = sysScheduledJobId;
            this.nameStandard = nameStandard;
            DateTime conversionDate = DateTime.Now;

            base.parameterObject = ParameterObject.Create(user: new Common.DTO.UserDTO {
                Name = serviceUser.Name,
                LoginName = serviceUser.LoginName,
                UserId = serviceUser.UserId
            });

            #endregion

            #region Get Sys data

            //SysCurrency
            SysCurrency sysCurrency = GetSysCurrency();
            if (sysCurrency == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

            //SysXEArticle
            List<SoeXeArticle> xeArticles = new List<SoeXeArticle>()
            {
                SoeXeArticle.TimeStart,
                SoeXeArticle.TimeTerminal,
            };

            //SysFeature
            List<SysFeatureDTO> sysFeatures = new List<SysFeatureDTO>();
            foreach (var sysFeature in FeatureManager.GetSysFeatures(xeArticles))
            {
                if (!sysFeatures.Any(i => i.SysFeatureId == sysFeature.SysFeatureId))
                    sysFeatures.Add(sysFeature);
            }

            #endregion

            //Connect to XE database
            using (CompEntities entities = new CompEntities())
            {
                entities.Connection.Open();

                //Connect to WT database
                using (SqlConnection wtConnection = new SqlConnection(wtConnectionString))
                {
                    //Open sql connection
                    wtConnection.Open();

                    //Collections needed in all conversions
                    List<EmployeeSchedule> employeeSchedules = null;
                    List<TimeCodeBreak> timeCodeBreaks = null;
                    List<TimeDeviationCause> timeDeviationCauses = null;

                    //Instances needed in all conversions
                    Company company = null;

                    if (!actorCompanyId.HasValue)
                    {
                        #region 1st conversion

                        #region Core

                        //License
                        License license = GetWtLicense(entities, wtConnection, wtCompanyId);
                        if (license == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11889, "Licensen hittades inte"));

                        CreateLogEntry("Konvertering: Licens skapad");

                        //Company
                        company = GetWtCompany(entities, wtConnection, wtCompanyId, license, sysCurrency);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        CreateLogEntry("Konvertering: Företag skapat");

                        //CompanyFlexForce
                        CompanyFlexForce companyFlexForce = GetWtFlexForceAPIKey(entities, wtConnection, wtCompanyId, company);
                        if (companyFlexForce == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "CompanyFlexForce");

                        CreateLogEntry("Konvertering: FlexForce API nycklar konverterade");

                        //Role
                        List<Role> roles = GetWtRoles(entities, company, license);
                        if (roles == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "Role");

                        CreateLogEntry(String.Format("Konvertering: {0} roller skapade", roles.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Category

                        //Category
                        List<Category> employeeCategories = GetWtCategories(entities, wtConnection, wtCompanyId, company);
                        if (employeeCategories == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "Category");

                        CreateLogEntry(String.Format("Konvertering: {0} kategorier skapade", employeeCategories.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Product

                        //ProductUnit
                        List<ProductUnit> productUnits = GetWtProductUnits(entities, wtConnection, wtCompanyId, company);
                        if (productUnits == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "ProductUnit");

                        CreateLogEntry(String.Format("Konvertering: {0} enheter skapade", productUnits.Count));

                        //PayrollProduct
                        List<PayrollProduct> payrollProducts = GetWtPayrollProducts(entities, wtConnection, wtCompanyId, company, productUnits);
                        if (payrollProducts == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91923, "Löneart hittades inte"));

                        CreateLogEntry(String.Format("Konvertering: {0} lönearter skapade", payrollProducts.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimeCode (Depending on: Product)

                        //TimeCode
                        List<TimeCode> timeCodes = GetWtTimeCodes(entities, company);
                        if (timeCodes == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

                        CreateLogEntry(String.Format("Konvertering: {0} tidkoder skapade", timeCodes.Count));

                        //TimeCodeBreak
                        timeCodeBreaks = GetWtTimeCodeBreaks(entities, wtConnection, wtCompanyId, company);
                        if (timeCodeBreaks == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeCodeBreak");

                        CreateLogEntry(String.Format("Konvertering: {0} rasttidkoder skapade", timeCodeBreaks.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimeCode settings (Depending on TimeCode)

                        //TimeCode settings
                        SetTimeCodeSettings(entities, company, timeCodes);

                        CreateLogEntry("Konvertering: Standardtidkod satt");

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimeDeviationCause (Depending on TimeCode)

                        //TimeDeviationCause
                        timeDeviationCauses = GetWtTimeDeviationCauses(entities, wtConnection, wtCompanyId, company, timeCodes);
                        if (timeDeviationCauses == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeDeviationCause");

                        CreateLogEntry(String.Format("Konvertering: {0} frånvaro-orsaker skapade", timeDeviationCauses));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region DayType

                        //DayType
                        List<DayType> dayTypes = GetWtDayTypes(entities, wtConnection, wtCompanyId, company);
                        if (dayTypes == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "DayType");

                        CreateLogEntry(String.Format("Konvertering: {0} dagtyper skapade", dayTypes.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Holiday/TimeHalfDay (Depending on: DayType)

                        //Holiday 


                        List<Holiday> holidays = GetWtHolidays(entities, wtConnection, wtCompanyId, company, fromDate, limitholidays);
                        if (holidays == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "Holiday");

                        CreateLogEntry(String.Format("Konvertering: {0} lediga dagar skapade", holidays.Count));

                        //TimeHalfday
                        List<TimeHalfday> halfdays = GetWtHalfDays(entities, wtConnection, wtCompanyId, company, fromDate, limitholidays);
                        if (halfdays == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeHalfday");

                        CreateLogEntry(String.Format("Konvertering: {0} halvdagar skapade", halfdays.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region EmployeeGroup (Depending on: TimeDeviationCause, DayType)

                        //EmployeeGroup
                        List<EmployeeGroup> employeeGroups = GetWtEmployeeGroups(wtConnection, wtCompanyId, company, timeDeviationCauses);
                        if (employeeGroups == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

                        CreateLogEntry(String.Format("Konvertering: {0} tidavtal skapade", employeeGroups.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region EmployeeGroup settings (Depending on: EmployeeGroup)

                        //EmployeeGroupTimeDeviationCauses
                        AddEmployeeGroupTimeDeviationCauses(entities, wtConnection, wtCompanyId, timeDeviationCauses);

                        CreateLogEntry("Konvertering: Frånvaroorsaker kopplade till tidavtal");

                        //EmployeeGroup standard
                        SetEmployeeGroupSettings(entities, company);

                        CreateLogEntry("Konvertering: Standardagrupp satt");

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Employee (Depending on: EmployeeGroup, TimeDeviationCauses)

                        //Employee
                        List<Employee> employees = GetWtEmployees(entities, wtConnection, wtCompanyId, company, license, roles, timeDeviationCauses);
                        if (employees == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "Employee");

                        EmployeeManager.AddHiddenEmployee(company.ActorCompanyId);

                        CreateLogEntry(String.Format("Konvertering: {0} anställda skapade", employees.Count));

                        //FlexForce
                        GetWtFlexForceSync(wtConnection, wtCompanyId);

                        CreateLogEntry("Konvertering: UseFlexForce-parametern satt på anställda");

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Employee settings (Depending on: Employee, Category

                        //Employee
                        SetEmployeeCategories(company, employees, roles);

                        CreateLogEntry("Konvertering: Anställdainställningar satta");

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Attest (Depending on: Category)

                        //AttestRole
                        List<AttestRole> attestRoles = GetWtAttestRoles(entities, company);
                        if (attestRoles == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestRole");

                        CreateLogEntry(String.Format("Konvertering: {0} attestroller skapade", attestRoles.Count));

                        //AttestRoleUser
                        List<AttestRoleUser> attestRoleUsers = GetWtAttestRoleUsers(entities, wtConnection, wtCompanyId);
                        if (attestRoleUsers == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestRoleUsers");

                        CreateLogEntry(String.Format("Konvertering: {0} attestrollanvändare skapade", attestRoleUsers.Count));

                        //AttestState
                        List<AttestState> attestStates = GetWtAttestStates(entities, wtConnection, wtCompanyId, company);
                        if (attestStates == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestState");

                        CreateLogEntry(String.Format("Konvertering: {0} attestnivåer skapade", attestStates.Count));

                        //AttestTransition
                        List<AttestTransition> attestTransition = GetWtAttestTransitions(entities, company, attestStates, attestRoles, employeeGroups);
                        if (attestTransition == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AttestTransition");

                        CreateLogEntry(String.Format("Konvertering: {0} attestövergångar skapade", attestTransition.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Attest settings (Depending on: Attest, Category)

                        //AttestState
                        SetAttestSettings(entities, company, attestStates, attestRoles, employeeCategories);

                        CreateLogEntry("Konvertering: Attestinställningar satta");

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimePeriod (Depending on: EmployeeGroup)

                        //TimePeriodHead
                        TimePeriodHead timePeriodHead = GetWtTimePeriodHead(entities, wtConnection, wtCompanyId, company);
                        if (timePeriodHead == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimePeriodHead");

                        CreateLogEntry("Konvertering: 1 perioduppsättning skapad");

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimeAccumulator (Depending on: EmployeeGroup)

                        //TimeAccumulator
                        List<TimeAccumulator> timeAccumulator = GetWtTimeAccumulators(entities, wtConnection, wtCompanyId, company);
                        if (timeAccumulator == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeAccumulator");

                        CreateLogEntry(String.Format("Konvertering: {0} ackar skapade", timeAccumulator.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        if (!noSchedules)
                        {
                            #region TimeSchedule

                            //TimeScheduleTemplateHead
                            List<TimeScheduleTemplateHead> timeScheduleTemplateHead = GetWtTimeScheduleTemplateHead(entities, wtConnection, wtCompanyId, company);
                            if (timeScheduleTemplateHead == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplateHead");

                            CreateLogEntry(String.Format("Konvertering: {0} schemamallshuvuden skapade", timeScheduleTemplateHead.Count));

                            List<TimeScheduleTemplatePeriod> timeScheduleTemplatePeriod = GetWtTimeScheduleTemplatePeriod(entities, wtConnection, wtCompanyId);
                            if (timeScheduleTemplatePeriod == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplatePeriod");

                            CreateLogEntry(String.Format("Konvertering: {0} schemamallsperioder skapade", timeScheduleTemplatePeriod.Count));

                            result = Save(entities);
                            if (!result.Success)
                                return result;
                            #endregion

                            #region TimeScheduleBlocks (Depending on: TimeSchedule)

                            //TimeScheduleTemplateBlock
                            List<TimeScheduleTemplateBlock> timeScheduleTemplateBlocks = GetWtTimeScheduleTemplateBlocks(entities, wtConnection, wtCompanyId, timeCodeBreaks);
                            if (timeScheduleTemplateBlocks == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplateBlock");

                            CreateLogEntry(String.Format("Konvertering: {0} schemamallsblock skapade", timeScheduleTemplateBlocks.Count));

                            result = Save(entities);
                            if (!result.Success)
                                return result;

                            #endregion

                            #region EmployeeSchedule (Depending on: TimeSchedule, Employee)

                            if (!noTransactions)
                            {
                                employeeSchedules = GetWtEmployeeSchedule(entities, wtConnection, wtCompanyId);
                                if (timeScheduleTemplateBlocks == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeSchedule");

                                CreateLogEntry(String.Format("Konvertering: {0} anställdascheman skapade", employeeSchedules.Count));

                                result = Save(entities);
                                if (!result.Success)
                                    return result;
                            }
                            else
                            {
                                CreateLogEntry("Aktivering av scheman hoppas över pga av inställning - notransactions");
                            }
                        }
                        else
                        {
                            CreateLogEntry("Aktivering av scheman hoppas över pga av inställning - noschedules");
                        }
                        #endregion

                        #region Articles/Features (Depending on: License)

                        List<LicenseArticle> licenseArticles = AddLicenseArticle(entities, license, xeArticles);
                        if (licenseArticles == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "LicenseArticle");

                        CreateLogEntry(String.Format("Konvertering: {0} XE-artiklar skapade", licenseArticles.Count));

                        List<EntityObject> features = null;// AddFeatures(entities, company, sysFeatures, license, roles);
                        if (features == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "SysFeature");

                        CreateLogEntry(String.Format("Konvertering: {0} behörigheter skapade", features.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimeTerminal

                        //TimeTerminal
                        List<TimeTerminal> terminals = GetWtTerminals(entities, wtConnection, wtCompanyId, company);
                        if (terminals == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "Terminal");

                        CreateLogEntry(String.Format("Konvertering: {0} terminaler skapade", terminals.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region SalaryExport

                        //TimeSalaryExport
                        List<TimeSalaryExport> exportedFiles = GetWtSalaryExports(entities, wtConnection, wtCompanyId, company, siteName);
                        if (exportedFiles == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeSalaryExport");

                        CreateLogEntry(String.Format("Konvertering: {0} löneexporter skapade", exportedFiles.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Reports

                        AddReports(company);
                        if (exportedFiles == null)
                            return new ActionResult(false, 0, "Konvertering: Rapporter kunde ej skapas");

                        CreateLogEntry("Konvertering: Rapporter skapade");

                        //Save not needed

                        #endregion

                        #region Account settings

                        //Account
                        SetAccountSettings(entities, company);

                        CreateLogEntry("Konvertering: Kontoinställningar satta");

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Add WtConvertMapping

                        AddWtConvertMapping(entities, company);

                        CreateLogEntry("Konvertering: Koppling mellan XEid och WTid skapad");

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #endregion
                    }
                    else
                    {
                        #region 2nd+ conversion

                        company = CompanyManager.GetCompany(entities, actorCompanyId.Value);

                        #region Populate WtConvertMapping

                        PopulateWtConvertMapping(entities, company);

                        CreateLogEntry("Konvertering: Listor populerade");

                        #endregion

                        #region EmployeeSchedules

                        employeeSchedules = TimeScheduleManager.GetEmployeeSchedules(entities, actorCompanyId.Value, true);

                        #endregion

                        #region TimeCodeBreaks

                        timeCodeBreaks = TimeCodeManager.GetTimeCodeBreaks(entities, actorCompanyId.Value).ToList();

                        #endregion

                        #region TimeDeviationCause

                        timeDeviationCauses = new List<TimeDeviationCause>();

                        #endregion

                        #endregion
                    }

                    #region All conversions

                    if (!noTransactions || !noSchedules)
                    {
                        #region TimeStampEntry (Depends on: Employee, TimeDeviationCause, TimeTerminal)

                        List<TimeStampEntry> timeStampEntries = GetWtTimeStampEntries(entities, wtConnection, wtCompanyId, company, timeDeviationCauses, fromDate, toDate);
                        if (timeStampEntries == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeStampEntry");

                        CreateLogEntry(String.Format("Konvertering: {0} stämplingar skapade", timeStampEntries.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimeScheduleTemplateBlock (Depends on: Employee, TimeSchedule, EmployeeSchedule)

                        List<TimeScheduleTemplateBlock> templateBlocksEmployee = GetWtTimeScheduleTemplateBlocksEmployee(entities, wtConnection, wtCompanyId, company, employeeSchedules, timeCodeBreaks, fromDate, toDate);
                        if (templateBlocksEmployee == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeScheduleTemplateBlock");

                        CreateLogEntry(String.Format("Konvertering: {0} anställdaschemamallsblock skapade", templateBlocksEmployee.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimeBlock (Depends on: Employee, TimeCode, EmployeeSchedule, TimeBlockDate, TimeStampEntry)

                        List<TimeBlock> timeBlocks = GetWtTimeBlocks(entities, wtConnection, wtCompanyId, company, employeeSchedules, templateBlocksEmployee, timeStampEntries, fromDate, toDate);
                        if (timeBlocks == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlock");

                        CreateLogEntry(String.Format("Konvertering: {0} tidblock skapade", timeBlocks.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimeCodeTransaction (Depends: TimeAccumulator, TimeBlock)

                        List<TimeCodeTransaction> timeCodeTransactionsForAcc = GetWtTimeAccumulatorTransactions(entities, wtConnection, wtCompanyId, company, timeBlocks, fromDate, toDate);
                        if (timeCodeTransactionsForAcc == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimePayrollTransaction");

                        CreateLogEntry(String.Format("Konvertering: {0} tidkodstransaktioner från ackar skapade", timeCodeTransactionsForAcc.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region TimeCodeTransaction

                        List<TimeCodeTransaction> timeCodeTransactions = GetWtTimeCodeTransactions(entities, wtConnection, wtCompanyId, company, timeBlocks, fromDate, toDate);
                        if (timeCodeTransactions == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeCodeTransaction");

                        CreateLogEntry(String.Format("Konvertering: {0} lönetransaktioner skapade", timeCodeTransactions.Count));

                        result = Save(entities);
                        if (!result.Success)
                            return result;

                        #endregion
                    }
                    else
                    {
                        CreateLogEntry("Aktivering av scheman hoppas över pga av inställning - notransactions eller noschedules");
                    }
                    #endregion

                    #region Save and complete convertion

                    result = Save(entities);
                    if (result.Success)
                        result = SetConvertionFlags(wtConnectionString, wtCompanyId, conversionDate, company);

                    #endregion

                    entities.Connection.Close();
                }

                #region Bulk copy
                /*
                 * some convertions should be made outside of the transaction
                 * i.e. transactions can be ~500k tuples
                 * better to insert such amounts with sql bulk copy to temporary tables where they can be safely handled in their own transactions
                */
                #endregion

                return result;
            }
        }

        #endregion

        #region Entity convert-methods

        #region Attest

        private List<AttestRole> GetWtAttestRoles(CompEntities entities, Company company)
        {
            List<AttestRole> attestRoles = new List<AttestRole>();

            foreach (var pair in this.exportedEmployeeCategoriesDict)
            {
                int wtEmployeeCategoryId = pair.Key;
                Category category = pair.Value;

                #region AttestRole

                AttestRole attestRole = new AttestRole()
                {
                    Name = StringUtility.Left(category.Name, 100),
                    Description = "",
                    DefaultMaxAmount = 0,
                    Module = (int)SoeModule.Time,

                    //Set references
                    Company = company,
                };
                SetCreatedProperties(attestRole);
                entities.AttestRole.AddObject(attestRole);

                attestRoles.Add(attestRole);

                #endregion

                #region AttestRoleUser

                AttestRoleUser attestRoleUser = new AttestRoleUser()
                {
                    DateFrom = null,
                    DateTo = null,
                    MaxAmount = 0,

                    //Set references
                    AttestRole = attestRole,
                    User = this.sysUser,
                };
                SetCreatedProperties(attestRoleUser);
                entities.AttestRoleUser.AddObject(attestRoleUser);

                //Save AttestRole/Category to add AttestRoleUser for Employee later
                this.attestRole_Category_States.Add(new AttestRoleCategoryStateObject()
                {
                    WtEmployeeCategoryId = wtEmployeeCategoryId,
                    Category = category,
                    AttestRole = attestRole,
                });

                #endregion
            }

            #region AttestRole Admin

            this.adminAttestRole = new AttestRole()
            {
                Name = "Admin",
                Description = "",
                DefaultMaxAmount = 0,
                Module = (int)SoeModule.Time,
                ShowAllCategories = true,

                //Set references
                Company = company,
            };
            SetCreatedProperties(this.adminAttestRole);
            entities.AttestRole.AddObject(this.adminAttestRole);

            #endregion

            #region AttestRoleUser Admin

            AttestRoleUser sysAttestRoleUser = new AttestRoleUser()
            {
                DateFrom = null,
                DateTo = null,
                MaxAmount = 0,

                //Set references
                AttestRole = this.adminAttestRole,
                User = this.sysUser,
            };
            SetCreatedProperties(sysAttestRoleUser);
            entities.AttestRoleUser.AddObject(sysAttestRoleUser);

            attestRoles.Add(this.adminAttestRole);

            #endregion

            return attestRoles;
        }

        private List<AttestRoleUser> GetWtAttestRoleUsers(CompEntities entities, SqlConnection wtConnection, int wtCompanyId)
        {
            List<AttestRoleUser> attestRoleUsers = new List<AttestRoleUser>();

            SqlCommand cmd = GetCommand(wtConnection, GET_ATTESTROLEUSERS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeCategoryId = dr.GetInt32(0);
                    int wtEmployeeId = dr.GetInt32(1);

                    var stateObj = this.attestRole_Category_States.FirstOrDefault(i => i.WtEmployeeCategoryId == wtEmployeeCategoryId);

                    #region AttestRoleUser

                    AttestRoleUser attestRoleUser = new AttestRoleUser()
                    {
                        DateFrom = null,
                        DateTo = null,
                        MaxAmount = stateObj.AttestRole.DefaultMaxAmount,

                        //Set references
                        User = GetExportedEmployeeUser(wtEmployeeId),
                        AttestRole = stateObj.AttestRole,
                    };
                    SetCreatedProperties(attestRoleUser);
                    entities.AttestRoleUser.AddObject(attestRoleUser);

                    //Add to collection
                    attestRoleUsers.Add(attestRoleUser);

                    #endregion
                }
            }

            return attestRoleUsers;
        }

        private List<AttestState> GetWtAttestStates(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            List<AttestState> attestStates = new List<AttestState>();

            SqlCommand cmd = GetCommand(wtConnection, GET_ATTESTSTATES, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtAttestStateId = dr.GetInt32(0);
                    string name = dr.GetString(1); //value from db not used
                    int wtStatusNr = Convert.ToInt32(dr.GetString(2));
                    int sort = dr.GetInt32(3);

                    #region AttestState

                    AttestState attestState = new AttestState()
                    {
                        Entity = (int)TermGroup_AttestEntity.PayrollTime,
                        Description = "",
                        Sort = sort,
                        Name = StringUtility.Left(name, 100),
                        Initial = false,
                        State = (int)SoeEntityState.Active,
                        Module = (int)SoeModule.Time,

                        //Set references
                        Company = company
                    };
                    SetCreatedProperties(attestState);
                    entities.AttestState.AddObject(attestState);

                    //Settings
                    switch (wtStatusNr)
                    {
                        case (int)WtAttestStates.Registered:
                            attestState.Name = WT_ATTESTSTATE_REGISTERED;
                            attestState.Color = "#B0B0B0";
                            attestState.Initial = true;
                            break;
                        case (int)WtAttestStates.Done:
                            attestState.Name = WT_ATTESTSTATE_DONE;
                            attestState.Color = "#1500FF";
                            break;
                        case (int)WtAttestStates.Approved:
                            attestState.Name = WT_ATTESTSTATE_APPROVED;
                            attestState.Color = "#F6FF00";
                            break;
                        case (int)WtAttestStates.Attested:
                            attestState.Name = WT_ATTESTSTATE_ATTESTED;
                            attestState.Color = "#2EA300";
                            break;
                        case (int)WtAttestStates.Downloaded:
                            attestState.Name = WT_ATTESTSTATE_DOWNLOADED;
                            attestState.Color = "#000000";
                            break;
                    }

                    //Add to collection
                    attestStates.Add(attestState);

                    //Add to dict
                    this.exportedAttestStatesDict.Add(wtAttestStateId, attestState);

                    #endregion
                }
            }

            return attestStates;
        }

        private List<AttestTransition> GetWtAttestTransitions(CompEntities entities, Company company, List<AttestState> attestStates, List<AttestRole> attestRoles, List<EmployeeGroup> employeeGroups)
        {
            List<AttestTransition> attestTransitions = new List<AttestTransition>();

            #region AttestStates

            AttestState registered = attestStates.FirstOrDefault(i => i.Name == WT_ATTESTSTATE_REGISTERED);
            AttestState done = attestStates.FirstOrDefault(i => i.Name == WT_ATTESTSTATE_DONE);
            AttestState attested = attestStates.FirstOrDefault(i => i.Name == WT_ATTESTSTATE_ATTESTED);
            AttestState payroll = attestStates.FirstOrDefault(i => i.Name == WT_ATTESTSTATE_DOWNLOADED);

            #endregion

            #region AttestTransitions

            //REG - KLAR
            AttestTransition registeredToDone = GetWtAttestTransition(entities, company, registered, done);
            if (registeredToDone != null)
            {
                ConnectAttestTransitionToRoles(registeredToDone, attestRoles);
                ConnectAttestTransitionToEmployeeGroups(registeredToDone, employeeGroups);
                attestTransitions.Add(registeredToDone);
            }

            //REG - ATTEST
            AttestTransition registeredToAttested = GetWtAttestTransition(entities, company, registered, attested);
            if (registeredToAttested != null)
            {
                ConnectAttestTransitionToRoles(registeredToAttested, attestRoles);
                attestTransitions.Add(registeredToAttested);
            }

            //KLAR - REG
            AttestTransition doneToRegistered = GetWtAttestTransition(entities, company, done, registered);
            if (doneToRegistered != null)
            {
                ConnectAttestTransitionToRoles(doneToRegistered, attestRoles);
                ConnectAttestTransitionToEmployeeGroups(doneToRegistered, employeeGroups);
                attestTransitions.Add(doneToRegistered);
            }

            //KLAR - ATTEST
            AttestTransition doneToAttested = GetWtAttestTransition(entities, company, done, attested);
            if (doneToAttested != null)
            {
                ConnectAttestTransitionToRoles(doneToAttested, attestRoles);
                attestTransitions.Add(doneToAttested);
            }

            //ATTEST - REG
            AttestTransition attestedToRegistered = GetWtAttestTransition(entities, company, attested, registered);
            if (attestedToRegistered != null)
            {
                ConnectAttestTransitionToRoles(attestedToRegistered, attestRoles);
                attestTransitions.Add(attestedToRegistered);
            }

            //ATTEST - KLAR
            AttestTransition attestedToDone = GetWtAttestTransition(entities, company, attested, done);
            if (attestedToDone != null)
            {
                ConnectAttestTransitionToRoles(attestedToDone, attestRoles);
                attestTransitions.Add(attestedToDone);
            }

            //ATTEST - LÖN
            AttestTransition attestedToPayroll = GetWtAttestTransition(entities, company, attested, payroll);
            if (attestedToPayroll != null)
            {
                ConnectAttestTransitionToRole(attestedToPayroll, this.adminAttestRole);
                attestTransitions.Add(attestedToPayroll);
            }

            //LÖN - ATTEST
            AttestTransition payrollToAttested = GetWtAttestTransition(entities, company, payroll, attested);
            if (payrollToAttested != null)
            {
                ConnectAttestTransitionToRole(payrollToAttested, this.adminAttestRole);
                attestTransitions.Add(payrollToAttested);
            }

            #endregion

            return attestTransitions;
        }

        private AttestTransition GetWtAttestTransition(CompEntities entities, Company company, AttestState attestStateFrom, AttestState attestStateTo)
        {
            AttestTransition attestTransition = null;

            if (attestStateFrom != null && attestStateTo != null)
            {
                #region AttestTransition

                attestTransition = new AttestTransition()
                {
                    Name = StringUtility.Left(attestStateFrom.Name + " - " + attestStateTo.Name, 255),
                    Module = (int)SoeModule.Time,

                    //Set references
                    AttestStateFrom = attestStateFrom,
                    AttestStateTo = attestStateTo,
                    Company = company,
                };
                SetCreatedProperties(attestTransition);
                entities.AttestTransition.AddObject(attestTransition);

                #endregion
            }

            return attestTransition;
        }

        #endregion

        #region Category

        private List<Category> GetWtCategories(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            List<Category> categories = new List<Category>();

            #region Prereq

            //AccountDim KSK
            AccountDim accountDimCostCentre = company.AccountDim.FirstOrDefault(i => i.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);

            #endregion

            SqlCommand cmd = GetCommand(wtConnection, GET_CATEGORIES, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtCategoryId = dr.GetInt32(0);
                    string categoryName = dr.GetString(2);
                    string categoryCode = dr.GetString(3);
                    string accountInternalName = dr.GetString(4);
                    string accountInternalNr = dr.GetString(5);

                    //Remove ; (ex: ;11 --> 11)
                    if (!String.IsNullOrEmpty(accountInternalNr) && accountInternalNr.StartsWith(";"))
                        accountInternalNr = accountInternalNr.Substring(1, accountInternalNr.Length - 1);

                    #region Category

                    Category category = new Category()
                    {
                        Name = StringUtility.Left(categoryName, 100),
                        Code = StringUtility.Left(categoryCode, 50),
                        Type = (int)SoeCategoryType.Employee,

                        //Set references
                        Company = company,
                    };
                    SetCreatedProperties(category);
                    entities.Category.AddObject(category);

                    //Add to Company
                    company.Category.Add(category);

                    //Add to collection
                    categories.Add(category);

                    //Add to dict
                    this.exportedEmployeeCategoriesDict.Add(wtCategoryId, category);

                    #endregion

                    #region AccountInternal

                    //No need to handle the return value
                    AccountInternal accountInternal = GetAccountInternal(entities, accountInternalNr, accountInternalName, accountDimCostCentre);

                    #endregion

                    #region Shifttypes

                    var random = new Random();
                    ShiftType shiftType = new ShiftType()
                    {
                        Name = StringUtility.Left(categoryName, 100),
                        ActorCompanyId = company.ActorCompanyId,
                        Color = String.Format("#{0:X6}", random.Next(0x1000000)),
                        ExternalCode = StringUtility.Left(categoryCode, 10),


                    };
                    shiftType.AccountInternal.Add(accountInternal);
                    SetCreatedProperties(shiftType);
                    entities.ShiftType.AddObject(shiftType);

                    #endregion
                }
            }

            return categories;
        }

        #endregion

        #region Company

        private Company GetWtCompany(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, License license, SysCurrency sysCurrency)
        {
            Company company = null;

            string exportId = "";
            string salaryExportTargetWT = "";

            SqlCommand cmd = GetCommand(wtConnection, GET_COMPANY, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region Company

                    company = new Company()
                    {
                        Name = StringUtility.Left(dr.GetString(0), 100),
                        // pos 1 not used here
                        ShortName = StringUtility.Left(dr.GetString(2), 10),
                        OrgNr = StringUtility.Left(dr.GetString(3), 50),
                        CompanyNr = 1,
                    };
                    SetCreatedProperties(company);
                    entities.Company.AddObject(company);

                    exportId = dr.GetString(4);
                    salaryExportTargetWT = dr.GetString(5);

                    #endregion
                }
            }

            if (company == null)
                return null;

            #region Actor

            Actor actor = new Actor()
            {
                ActorType = (int)SoeActorType.Company,
            };
            SetCreatedProperties(actor);

            //Add to Company
            company.Actor = actor;

            #endregion

            #region Currency

            //Base Currency
            Currency baseCurrency = new Currency()
            {
                SysCurrencyId = sysCurrency.SysCurrencyId,
                IntervalType = Constants.CURRENCY_INTERVALTYPE_DEFAULT,
                UseSysRate = Constants.CURRENCY_USESYSRATE_DEFAULT,
            };
            SetCreatedProperties(baseCurrency);

            //Add to Company
            company.Currency.Add(baseCurrency);
            company.License = license;

            #endregion

            #region AccountDim

            //AccountDim std
            this.accountDimStd = new AccountDim()
            {
                AccountDimNr = Constants.ACCOUNTDIM_STANDARD,
                Name = GetText(1258, "Konto"),
                ShortName = GetText(3776, "Std"),
                SysSieDimNr = null,
                MinChar = null,
                MaxChar = null,
            };
            SetCreatedProperties(this.accountDimStd);

            //Add to Company
            company.AccountDim.Add(this.accountDimStd);

            //Create AccountDim ksk
            AccountDim accountDimCostCentre = new AccountDim()
            {
                AccountDimNr = Constants.ACCOUNTDIM_STANDARD + 1,
                Name = "Kostnadsställe",
                ShortName = "KSK",
                SysSieDimNr = (int)TermGroup_SieAccountDim.CostCentre,
                MinChar = null,
                MaxChar = null,
            };
            SetCreatedProperties(accountDimCostCentre);

            //Add to Company
            company.AccountDim.Add(accountDimCostCentre);

            #endregion

            #region Settings

            UserCompanySetting settingTimeDefaultStartOnFirstDayOfWeek = new UserCompanySetting()
            {
                SettingTypeId = (int)CompanySettingType.TimeDefaultStartOnFirstDayOfWeek,
                BoolData = true,
                DataTypeId = (int)SettingDataType.Boolean,

                //Set references
                Company = company,
            };
            entities.UserCompanySetting.AddObject(settingTimeDefaultStartOnFirstDayOfWeek);
            UserCompanySetting settingTimeMaxNoOfBrakes = new UserCompanySetting()
            {
                SettingTypeId = (int)CompanySettingType.TimeMaxNoOfBrakes,
                IntData = 1,
                DataTypeId = (int)SettingDataType.Integer,

                //Set references
                Company = company,
            };
            entities.UserCompanySetting.AddObject(settingTimeMaxNoOfBrakes);

            if (exportId.Length > 0)
            {
                UserCompanySetting settingSalaryExportExternalExportID = new UserCompanySetting()
                {
                    SettingTypeId = (int)CompanySettingType.SalaryExportExternalExportID,
                    StrData = exportId,
                    DataTypeId = (int)SettingDataType.String,

                    //Set references
                    Company = company,
                };
                entities.UserCompanySetting.AddObject(settingSalaryExportExternalExportID);
            }

            if (salaryExportTargetWT.Length > 0)
            {
                UserCompanySetting settingSalaryExportTarget = new UserCompanySetting()
                {
                    SettingTypeId = (int)CompanySettingType.SalaryExportTarget,
                    DataTypeId = (int)SettingDataType.Integer,

                    //Set references
                    Company = company,
                };
                entities.UserCompanySetting.AddObject(settingSalaryExportTarget);

                #region Salary export

                switch (salaryExportTargetWT)
                {
                    case "1":  //Carat 2000
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.Carat2000;
                        break;
                    case "2": //SoftOne Lön
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.SoftOne;
                        break;
                    case "3":  //Svensk Lön
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.SvenskLon;
                        break;
                    case "4":  //Kontek lön
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.KontekLon;
                        break;
                    case "5":  //Agda lön
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.AgdaLon;
                        break;
                    case "6":  //Hogia 214006
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.Hogia214006;
                        break;
                    case "7":  //Hogia 214002
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.Hogia214002;
                        break;
                    case "8":  //Spcs
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.Spcs;
                        break;
                    case "9":  //Personec
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.Personec;
                        break;
                    default:
                        settingSalaryExportTarget.IntData = (int)SoeTimeSalaryExportTarget.Undefined;
                        break;
                }

                #endregion
            }

            #endregion

            entities.Company.AddObject(company);

            return company;
        }

        #endregion

        #region Currency

        private SysCurrency GetSysCurrency()
        {
            List<SysCurrency> sysCurrencies = CountryCurrencyManager.GetSysCurrencies();
            return sysCurrencies.FirstOrDefault(i => i.Code.ToUpper() == TermGroup_Currency.SEK.ToString());
        }

        #endregion

        #region Calendar

        #region DayType

        private List<DayType> GetWtDayTypes(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            List<DayType> dayTypes = new List<DayType>();

            SqlCommand cmd = GetCommand(wtConnection, GET_DAYTYPES, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtDayTypeId = dr.GetInt32(0);
                    string name = dr.GetString(1);
                    string specialDay = dr.GetValue(3).ToString();

                    #region DayType

                    DayType dayType = new DayType()
                    {
                        Name = StringUtility.Left(name, 100),

                        //Set references
                        Company = company,
                    };
                    SetCreatedProperties(dayType);
                    entities.DayType.AddObject(dayType);

                    //Settings
                    if (dayType.Name.Equals("Helgdag"))
                    {
                        dayType.Type = (int)SoeDayTypeClassification.Holiday;
                        dayType.SysDayTypeId = (int)WtDayTypes.Helgdag;
                    }
                    else if (dayType.Name.Equals("Vardag"))
                    {
                        dayType.Type = (int)SoeDayTypeClassification.Weekday;
                        dayType.SysDayTypeId = (int)WtDayTypes.Vardag;
                        dayType.StandardWeekdayFrom = 1;
                        dayType.StandardWeekdayTo = 5;
                    }
                    else if (dayType.Name.Equals("Lördag"))
                    {
                        dayType.Type = (int)SoeDayTypeClassification.Saturday;
                        dayType.SysDayTypeId = (int)WtDayTypes.Lördag;
                        dayType.StandardWeekdayFrom = 6;
                        dayType.StandardWeekdayTo = 6;
                    }
                    else if (dayType.Name.Equals("Söndag"))
                    {
                        dayType.Type = (int)SoeDayTypeClassification.Sunday;
                        dayType.SysDayTypeId = (int)WtDayTypes.Söndag;
                        dayType.StandardWeekdayFrom = 0;
                        dayType.StandardWeekdayTo = 0;
                    }
                    else
                    {
                        dayType.Type = (int)SoeDayTypeClassification.Undefined;
                    }

                    //Add do dict
                    this.exportedDayTypesDict.Add(wtDayTypeId, dayType);

                    //Add to collection
                    dayTypes.Add(dayType);
                    if (StringUtility.GetBool(specialDay))
                        this.exportedDayTypesSpecial.Add(dayType);
                    else
                        this.exportedDayTypes.Add(dayType);

                    #endregion
                }
            }

            return dayTypes;
        }

        #endregion

        #region TimeHalfDay

        private List<TimeHalfday> GetWtHalfDays(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, DateTime fromDate, bool limitholidays)
        {
            List<TimeHalfday> timeHalfdays = new List<TimeHalfday>();

            if (!limitholidays)
                fromDate = new DateTime(2001, 1, 1, 0, 0, 0, 000);

            //Cache
            Dictionary<int, TimeHalfday> halfDaydict = new Dictionary<int, TimeHalfday>();

            SqlCommand cmd = GetCommand(wtConnection, GET_HOLIDAYS, wtCompanyId, fromDate);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int value = dr.GetInt32(1);

                    //Only time halfdays
                    if (value > 0)
                    {
                        DateTime date = dr.GetDateTime(0);
                        int minutes = dr.GetInt32(2);
                        int type = dr.GetInt32(3);
                        int wtDayTypeId = dr.GetInt32(4);

                        DayType dayType = GetExportedDayType(wtDayTypeId);

                        #region Holiday

                        Holiday holiday = new Holiday()
                        {
                            Date = date.AddMinutes(minutes),
                            Name = dayType != null ? StringUtility.Left(dayType.Name + " " + date.ToShortDateString(), 100) : "",
                            Description = String.Empty,

                            //Set references
                            Company = company,
                            DayType = dayType,
                        };
                        SetCreatedProperties(holiday);
                        entities.Holiday.AddObject(holiday);

                        #endregion

                        if (!halfDaydict.ContainsKey(wtDayTypeId))
                        {
                            #region TimeHalfDay

                            TimeHalfday timeHalfDay = new TimeHalfday()
                            {
                                Name = "Halvdag " + value.ToString(),
                                Description = String.Empty,
                                Value = value,

                                //Set references
                                DayType = dayType,
                            };
                            SetCreatedProperties(timeHalfDay);
                            entities.TimeHalfday.AddObject(timeHalfDay);

                            //Settings
                            switch ((WtHalfDayRules)type)
                            {
                                case WtHalfDayRules.Clock:
                                    timeHalfDay.Type = (int)SoeTimeHalfdayType.ClockInMinutes;
                                    break;
                                case WtHalfDayRules.RelativeScheduleIn:
                                    timeHalfDay.Type = (int)SoeTimeHalfdayType.RelativeStartValue;
                                    break;
                                case WtHalfDayRules.RelativeScheduleOut:
                                    timeHalfDay.Type = (int)SoeTimeHalfdayType.RelativeEndValue;
                                    break;
                            }

                            //Add to dict
                            halfDaydict.Add(wtDayTypeId, timeHalfDay);

                            //Add to collection
                            timeHalfdays.Add(timeHalfDay);

                            #endregion
                        }
                    }
                }
            }

            return timeHalfdays;
        }

        #endregion

        #region Holiday

        private List<Holiday> GetWtHolidays(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, DateTime fromDate, bool limitholidays)
        {
            List<Holiday> holidays = new List<Holiday>();

            if (!limitholidays)
                fromDate = new DateTime(2001, 1, 1, 0, 0, 0, 000);

            SqlCommand cmd = GetCommand(wtConnection, GET_HOLIDAYS, wtCompanyId, fromDate);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int value = dr.GetInt32(1);

                    //Only holidays
                    if (value == 0)
                    {
                        DateTime date = dr.GetDateTime(0);
                        int wtDayTypeId = dr.GetInt32(4);

                        DayType dayType = GetExportedDayType(wtDayTypeId);

                        #region Holiday

                        Holiday holiday = new Holiday()
                        {
                            Date = date,
                            Name = dayType != null ? StringUtility.Left(dayType.Name, 100) : "",
                            Description = String.Empty,

                            //Set references
                            Company = company,
                            DayType = dayType,
                        };
                        SetCreatedProperties(holiday);
                        entities.Holiday.AddObject(holiday);

                        //Add to collection
                        holidays.Add(holiday);

                        #endregion
                    }
                }
            }

            return holidays;
        }

        #endregion

        #endregion

        #region Export

        private List<TimeSalaryExport> GetWtSalaryExports(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, string siteName)
        {
            List<TimeSalaryExport> timeSalaryExports = new List<TimeSalaryExport>();

            SqlCommand cmd = GetCommand(wtConnection, GET_SALARY_EXPORTS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    string fileName = dr.GetString(0);
                    string companyName = dr.GetString(1);
                    string extension = fileName.Substring(fileName.Length - 3);
                    DateTime startInterval = dr.GetDateTime(3);
                    DateTime stopInterval = dr.GetDateTime(4);
                    DateTime exportDate = dr.GetDateTime(5);

                    #region TimeSalaryExport

                    //Files
                    byte[] file1 = GetWtTimeSalaryExportFileFromWS(siteName, companyName, fileName);

                    TimeSalaryExport timeSalaryExport = new TimeSalaryExport()
                    {
                        Extension = StringUtility.Left(extension, 3),
                        StartInterval = startInterval,
                        StopInterval = stopInterval,
                        ExportDate = exportDate,
                        ExportFormat = (int)SoeTimeSalaryExportFormat.Text,
                        File1 = file1 ?? new byte[0],
                        File2 = new byte[0],

                        //Set references
                        Company = company,
                    };
                    SetCreatedProperties(timeSalaryExport);
                    entities.TimeSalaryExport.AddObject(timeSalaryExport);

                    //ExportTarget
                    int exportTarget = Convert.ToInt32(dr.GetString(6));
                    switch (exportTarget)
                    {
                        case 1:
                            //"Carat 2000"
                            timeSalaryExport.ExportTarget = (int)SoeTimeSalaryExportTarget.Carat2000;
                            break;
                        case 2:
                            //"SoftOne Lön"
                            timeSalaryExport.ExportTarget = (int)SoeTimeSalaryExportTarget.SoftOne;
                            break;
                        case 3:
                            //"Svensk Lön"
                            timeSalaryExport.ExportTarget = (int)SoeTimeSalaryExportTarget.SvenskLon;
                            break;
                        case 4:
                            //"Kontek lön"
                            timeSalaryExport.ExportTarget = (int)SoeTimeSalaryExportTarget.KontekLon;
                            break;
                        case 5:
                            //"Agda lön"
                            timeSalaryExport.ExportTarget = (int)SoeTimeSalaryExportTarget.AgdaLon;
                            break;
                        case 6:
                            //"Hogia A"
                            timeSalaryExport.ExportTarget = (int)SoeTimeSalaryExportTarget.Hogia214006;
                            break;
                        case 7:
                            //"Hogia B"
                            timeSalaryExport.ExportTarget = (int)SoeTimeSalaryExportTarget.Hogia214002;
                            break;
                        case 8:
                            //"Spcs"
                            timeSalaryExport.ExportTarget = (int)SoeTimeSalaryExportTarget.Spcs;
                            break;
                    }

                    //Get transactions, inner select
                    List<int> transactionIds = GetWtTimeSalaryExportRows(wtConnection, dr.GetInt32(7));

                    //Add to dict
                    this.exportedTimeSalaryExportTransactionsDict.Add(timeSalaryExport, transactionIds);

                    //Add to collection
                    timeSalaryExports.Add(timeSalaryExport);

                    //Add to collection
                    timeSalaryExports.Add(timeSalaryExport);

                    #endregion
                }
            }

            return timeSalaryExports;
        }

        private List<int> GetWtTimeSalaryExportRows(SqlConnection wtConnection, int wtCompanyId)
        {
            List<int> transactionIds = new List<int>();

            //Get transactions, inner select
            SqlCommand cmd = GetCommand(wtConnection, GET_SALARY_EXPORT_ROWS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    transactionIds.Add(dr.GetInt32(0));
                }
            }

            return transactionIds;
        }

        private byte[] GetWtTimeSalaryExportFileFromWS(string siteName, string companyName, string fileName)
        {
            byte[] data = null;

            try
            {
                string fileUrl = @"http://www.webbtid.se/" + siteName + @"/" + companyName + @"/" + fileName;

                //Open from uri
                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(fileUrl))
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            while (reader.Peek() >= 0)
                            {
                                memoryStream.WriteByte((byte)reader.Read());
                            }
                        }

                        data = memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return data;
        }

        #endregion

        #region Employee

        private List<Employee> GetWtEmployees(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, License license, List<Role> roles, List<TimeDeviationCause> timeDeviationCauses)
        {
            List<Employee> employees = new List<Employee>();

            #region Prereq

            AccountDim accountDimCostCentre = company.AccountDim.FirstOrDefault(i => i.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);
            TimeDeviationCause timeDeviationCauseStd = GetTimeDeviationCauseStandard(timeDeviationCauses);

            Dictionary<int, int> employeeObDict = new Dictionary<int, int>();

            //Get the OB-rules that should apply for each Employee
            SqlCommand cmd = GetCommand(wtConnection, GET_EMPLOYEES_OBID, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeId = dr.GetInt32(0);
                    int wtObId = dr.GetInt32(1);

                    if (!employeeObDict.ContainsKey(wtEmployeeId))
                        employeeObDict.Add(wtEmployeeId, wtObId);
                }
            }

            #endregion

            cmd = GetCommand(wtConnection, GET_EMPLOYEES, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeId = dr.GetInt32(0);
                    int wtEmployeeGroupId = dr.GetInt32(1);
                    int wtEmployeeCategoryId = dr.GetInt32(2);
                    string name = dr.GetString(3);
                    string employeeNr = dr.GetString(4);
                    DateTime? employmentDate = dr.GetDateTime(5).CompareTo(CalendarUtility.DATETIME_DEFAULT) > 0 ? dr.GetDateTime(5) : (DateTime?)null;
                    DateTime? endDate = dr.GetDateTime(6).CompareTo(CalendarUtility.DATETIME_DEFAULT) > 0 ? dr.GetDateTime(6) : (DateTime?)null;
                    string email = dr.GetString(7);
                    int wtRoleId = dr.GetInt32(8);
                    string socialSec = dr.GetString(9);
                    string address = dr.GetString(10);
                    string postalCode = dr.GetString(11);
                    string postalAddress = dr.GetString(12);
                    string phoneHome = dr.GetString(13);
                    string phoneMobile = dr.GetString(14);
                    string phoneJob = dr.GetString(15);
                    string accountInternalNr = !dr.IsDBNull(16) ? dr.GetString(16) : "";

                    string obNumber = "";
                    if (employeeObDict.ContainsKey(wtEmployeeId))
                        obNumber = employeeObDict[wtEmployeeId].ToString();

                    StringUtility.GetName(name, out string firstName, out string lastName, this.nameStandard);

                    #region ContactPerson

                    Actor actor = new Actor()
                    {
                        ActorType = (int)SoeActorType.ContactPerson,
                    };
                    SetCreatedProperties(actor);

                    //Add ContactPerson
                    ContactPerson contactPerson = new ContactPerson()
                    {
                        FirstName = StringUtility.Left(firstName, 100),
                        LastName = StringUtility.Left(lastName, 100),
                        SocialSec = StringUtility.Left(socialSec, 50),
                        Position = 0,

                        //Set references
                        Actor = actor,
                    };
                    SetCreatedProperties(contactPerson);
                    entities.ContactPerson.AddObject(contactPerson);

                    //Map ContactPerson to Company
                    if (contactPerson.Actors == null)
                        contactPerson.Actors = new EntityCollection<Actor>();
                    contactPerson.Actors.Add(company.Actor);

                    #endregion

                    #region Contact

                    Contact contact = new Contact()
                    {
                        //Set references
                        Actor = contactPerson.Actor,
                    };
                    SetCreatedProperties(contact);

                    #endregion

                    #region ContactECom

                    if (!String.IsNullOrEmpty(email))
                    {
                        ContactECom contactECom = new ContactECom()
                        {
                            SysContactEComTypeId = (int)TermGroup_SysContactEComType.Email,
                            Text = StringUtility.Left(email, 100),

                            //Set references
                            Contact = contact,
                        };
                        SetCreatedProperties(contactECom);
                    }

                    //Add PhoneHome
                    if (!String.IsNullOrEmpty(phoneHome))
                    {
                        ContactECom contactECom = new ContactECom()
                        {
                            SysContactEComTypeId = (int)TermGroup_SysContactEComType.PhoneHome,
                            Text = StringUtility.Left(phoneHome, 100),

                            //Set references
                            Contact = contact,
                        };
                        SetCreatedProperties(contactECom);
                    }

                    //Add PhoneMobile
                    if (!String.IsNullOrEmpty(phoneMobile))
                    {
                        ContactECom contactECom = new ContactECom()
                        {
                            SysContactEComTypeId = (int)TermGroup_SysContactEComType.PhoneMobile,
                            Text = StringUtility.Left(phoneMobile, 100),

                            //Set references
                            Contact = contact,
                        };
                        SetCreatedProperties(contactECom);
                    }

                    //Add PhoneJob
                    if (!String.IsNullOrEmpty(phoneJob))
                    {
                        ContactECom contactECom = new ContactECom()
                        {
                            SysContactEComTypeId = (int)TermGroup_SysContactEComType.PhoneJob,
                            Text = StringUtility.Left(phoneJob, 100),

                            //Set references
                            Contact = contact,
                        };
                        SetCreatedProperties(contactECom);
                    }

                    #endregion

                    #region ContactAddress

                    if (!String.IsNullOrEmpty(address) || !String.IsNullOrEmpty(postalCode) || !String.IsNullOrEmpty(postalAddress))
                    {
                        //Distribution
                        ContactAddress contactAddress = new ContactAddress()
                        {
                            SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Distribution,

                            //Set references
                            Contact = contact,
                        };
                        SetCreatedProperties(contactAddress);

                        //Address
                        if (!String.IsNullOrEmpty(address))
                        {
                            ContactAddressRow contactAddressRow = new ContactAddressRow()
                            {
                                SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.Address,
                                Text = StringUtility.Left(address, 100),
                            };
                            SetCreatedProperties(contactAddressRow);

                            //Add to ContactAddress
                            contactAddressRow.ContactAddress = contactAddress;
                        }

                        //ZipCode
                        if (!String.IsNullOrEmpty(postalCode))
                        {
                            ContactAddressRow contactAddressRow = new ContactAddressRow()
                            {
                                SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalCode,
                                Text = StringUtility.Left(postalCode, 100),
                            };
                            SetCreatedProperties(contactAddressRow);

                            //Add to ContactAddress
                            contactAddressRow.ContactAddress = contactAddress;
                        }

                        //City
                        if (!String.IsNullOrEmpty(postalAddress))
                        {
                            ContactAddressRow contactAddressRow = new ContactAddressRow()
                            {
                                SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalAddress,
                                Text = StringUtility.Left(postalAddress, 100),
                            };
                            SetCreatedProperties(contactAddressRow);

                            //Add to ContactAddress
                            contactAddressRow.ContactAddress = contactAddress;
                        }
                    }

                    #endregion

                    #region Employee

                    Employee employee = new Employee()
                    {
                        EmployeeNr = employeeNr,
                        EmploymentDate = employmentDate,
                        EndDate = endDate,

                        //Set FK
                        ActorCompanyId = company.ActorCompanyId,

                        //Set references
                        TimeDeviationCause = timeDeviationCauseStd,
                        ContactPerson = contactPerson,

                    };
                    SetCreatedProperties(employee);
                    entities.Employee.AddObject(employee);

                    //Add to ContactPerson
                    contactPerson.Employee.Add(employee);

                    #endregion

                    #region EmployeeGroup

                    //If the EmployeeGroup is empty or if it has same OB-rules as the one we are adding to the EmployeeGroup, then it is ok - otherwise we must make a copy of the EmployeeGroups
                    EmployeeGroup employeeGroup = GetExportedEmployeeGroup(wtEmployeeGroupId);
                    bool hasEmployeeGroupAnyEmployee = employeeGroup != null && employees.Any(eg => eg.Employment != null && eg.Employment.Any(e => e.GetEmployeeGroupId() == employeeGroup.EmployeeGroupId));
                    if (employeeGroup != null && (hasEmployeeGroupAnyEmployee || employeeGroup.ModifiedBy == obNumber))
                    {
                        employeeGroup.ModifiedBy = StringUtility.Left(obNumber, 50);

                        //Add to dict
                        this.exportedEmployeeGroupsDict[wtEmployeeGroupId] = employeeGroup;
                    }
                    else
                    {
                        //Look for a EmployeeGroup that fits
                        foreach (EmployeeGroup exportedEmployeeGroup in this.exportedEmployeeGroupsDict.Values)
                        {
                            if (exportedEmployeeGroup.ModifiedBy == StringUtility.Left(obNumber, 50) && exportedEmployeeGroup.CreatedBy == wtEmployeeGroupId.ToString())
                            {
                                employeeGroup = exportedEmployeeGroup;
                                break;
                            }
                        }

                        //If we dont find any EmployeeGroup that fits, we create a new
                        if (employeeGroup == null)
                        {
                            //TODO: Doesnt seem like a new EmployeeGroup is created below?
                            employeeGroup = GetExportedEmployeeGroup(wtEmployeeGroupId);
                            if (employeeGroup != null)
                            {
                                employeeGroup.ModifiedBy = StringUtility.Left(obNumber, 50);
                                employeeGroup.CreatedBy = wtEmployeeGroupId.ToString();
                            }

                            this.exportedEmployeeGroupsDict.Add(this.exportedEmployeeGroupsDict.Keys.Max() + 1, employeeGroup);
                        }
                    }

                    #region Deprecated
                    /*
                    //Eftersom Rickard ändrar sig 1336534236534 gånger innan en punkt är slutförd så kommenterar jag bara ut det här, utifall att han vill att det ska kommenteras igen
                    if (employee.EmployeeGroup != null)
                    {
                        if (employee.EmployeeGroup.ModifiedBy.Length > 0 && employee.EmployeeGroup.TimeRule.Count == 0)
                        {
                            //Hämta ut de olika obreglerna som ska gälla för respektive anställd
                            SqlCommand trcmd = GetCommand(conn, FETCH_EMPLOYEESOBID, wtCompanyId);

                            int recentObId = 0;

                            TimeRule timeRule = new TimeRule
                            {
                                BelongsToGroup = true,
                                Description = "",
                                Factor = 1,
                                Name = "",
                                RuleStartDirection = (int)SoeTimeRuleDirection.Forward,
                                RuleStopDirection = (int)SoeTimeRuleDirection.Forward,
                                Internal = true,
                                TimeCode = timeCode,
                                TimeDeviationCause = timeDeviationCause,
                                IsInconvenientWorkHours = true,
                                Type = (int)SoeTimeRuleType.Presence,
                                State = (int)SoeEntityState.Active

                                //Set Fk
                     *          ActorCompanyId = company.ActorCompanyId,
                            };

                            TimeRuleGroup trg = new TimeRuleGroup
                            {
                                Description = "Export from WT",
                                State = (int)SoeEntityState.Active
                            };

                            TimeRuleExpression tre = new TimeRuleExpression
                            {
                                IsStart = true
                            };

                            timeRule.TimeRuleExpression.Add(tre);

                            using (SqlDataReader tr = trcmd.ExecuteReader())
                            {
                                while (tr.Read())
                                {
                                    if (dr.GetInt32(0) == wtUserId)
                                    {
                                        if (dr.GetInt32(1) != recentObId)
                                        {

                                            timeRule.Name = Left(tr.GetString(3), 100);
                                            timeRule.DayType = dayTypes[tr.GetInt32(2)];

                                            trg.Name = Left(tr.GetString(3), 255);

                                            timeRule.TimeRuleGroup.Add(trg);

                                            employee.EmployeeGroup.TimeRule.Add(timeRule);

                                        }

                                        //Om jag förstått det rätt så om start är sant så ska regeln gälla mellan 00 och tidpunkten, annars 00-tiden och fram till 00.

                                        int startTime = 0;
                                        int endTime = 0;

                                        if (tr.GetBoolean(4))
                                        {
                                            endTime = tr.GetInt32(6);
                                        }
                                        else
                                        {
                                            endTime = 1440;
                                            startTime = endTime - tr.GetInt32(6);
                                        }

                                        TimeRuleOperand startOperand = new TimeRuleOperand
                                        {
                                            OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorScheduleIn,
                                            Minutes = startTime
                                        };
                                        TimeRuleOperand stopOperand = new TimeRuleOperand
                                        {
                                            OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorScheduleOut,
                                            Minutes = endTime
                                        };


                                        tre.TimeRuleOperand.Add(startOperand);
                                        tre.TimeRuleOperand.Add(stopOperand);

                                        recentObId = tr.GetInt32(1);
                                    }
                                }
                            }
                        }
                    }
                    */
                    #endregion

                    //Save Employee/Category to add CompanyCategoryRecord for it when Employee.EmployeeId is available/generated
                    if (this.exportedEmployeeCategoriesDict.ContainsKey(wtEmployeeCategoryId))
                    {
                        this.employee_Category_States.Add(new EmployeeCategoryStateObject()
                        {
                            WtEmployeeCategoryId = wtEmployeeCategoryId,
                            Employee = employee,
                            Category = this.exportedEmployeeCategoriesDict[wtEmployeeCategoryId],
                        });
                    }

                    #endregion

                    #region Employment

                    Employment employment = EmployeeManager.AddEmploymentIfNotExists(entities, null, null, employee, employeeGroup, workTimeWeek: employeeGroup.RuleWorkTimeWeek);
                    if (employment != null)
                    {
                        #region EmploymentAccountStd

                        EmploymentAccountStd employmentAccountStd = null;

                        //Remove ; (ex: ;11 --> 11)
                        if (!String.IsNullOrEmpty(accountInternalNr) && accountInternalNr.StartsWith(";"))
                            accountInternalNr = accountInternalNr.Substring(1, accountInternalNr.Length - 1);

                        //AccountInternal
                        if (!String.IsNullOrEmpty(accountInternalNr) && !accountInternalNr.Contains(';'))
                        {
                            //AccountInternals for PayrollTransactions that not corresponds to a Category (i.e. was converted in GetWtCategories) will be skipped
                            AccountInternal accountInternal = accountDimCostCentre.Account.Where(i => i.AccountNr == accountInternalNr).Select(i => i.AccountInternal).FirstOrDefault();
                            if (accountInternal != null)
                            {
                                employmentAccountStd = new EmploymentAccountStd()
                                {
                                    Type = (int)EmploymentAccountType.Cost,
                                    Percent = 0,

                                    //Set references
                                    AccountStd = GetAccountStdPayroll(entities, company.ActorCompanyId),
                                };

                                //Add AccountInternal
                                employmentAccountStd.AccountInternal.Add(accountInternal);
                            }
                        }

                        if (employmentAccountStd != null)
                            employmentAccountStd.Employment = employment;

                        #endregion
                    }

                    #endregion

                    #region User

                    //Set LoginName to WT anstnr regardless if it is numeric or not
                    string loginName = StringUtility.Left(employeeNr, 50);

                    User user = license.User.FirstOrDefault(i => i.LoginName == loginName);
                    if (user == null)
                    {
                        user = new User()
                        {
                            Name = StringUtility.Left(name, 100),
                            LoginName = loginName,
                            passwordhash = LoginManager.GetPasswordHash(loginName, Path.GetRandomFileName().Replace(".", "")),
                            ChangePassword = true,
                            LangId = Constants.SYSLANGUAGE_SYSLANGUAGEID_DEFAULT,

                            //Set references
                            License = license,
                            ContactPerson = contactPerson,
                        };
                        SetCreatedProperties(user);

                        //Add Employee
                        user.Employee.Add(employee);
                    }

                    #endregion

                    #region UserCompanyRole

                    int termId = GetRoleTermIdFromWTRoleId(wtRoleId);
                    UserCompanyRole userCompanyRole = new UserCompanyRole()
                    {
                        //Set references
                        User = user,
                        Company = company,
                        Role = roles.FirstOrDefault(i => i.TermId == termId),
                    };
                    entities.UserCompanyRole.AddObject(userCompanyRole);

                    //Save UserCompanyRole's to update User.DefaultRole later when Role.RoleId is available/generated
                    this.exportedUserCompanyRoles.Add(userCompanyRole);

                    #endregion

                    //Add to collection
                    employees.Add(employee);

                    //Add to dict
                    this.exportedEmployeesDict.Add(wtEmployeeId, employee);
                }
            }

            return employees;
        }

        private void SetEmployeeCategories(Company company, List<Employee> employees, List<Role> roles)
        {
            if (employees == null || roles == null || company == null)
                return;

            List<CompanyCategoryRecord> records = new List<CompanyCategoryRecord>();

            foreach (Employee employee in employees)
            {
                #region Employee

                //Set default Company
                employee.User.DefaultActorCompanyId = company.ActorCompanyId;

                //Set default Role
                var userCompanyRole = this.exportedUserCompanyRoles.FirstOrDefault(i => i.UserId == employee.User.UserId);
                if (userCompanyRole != null)
                    userCompanyRole.Default = true;

                //Add CompanyCategoryRecord
                var stateObj = this.employee_Category_States.FirstOrDefault(i => i.Employee.EmployeeId == employee.EmployeeId);
                if (stateObj != null)
                {
                    CompanyCategoryRecord record = new CompanyCategoryRecord()
                    {
                        Entity = (int)SoeCategoryRecordEntity.Employee,
                        Default = true,
                        DateFrom = null,
                        DateTo = null,

                        //Set references
                        Company = company,
                        Category = stateObj.Category,

                        //Set FK
                        RecordId = employee.EmployeeId,
                    };

                    records.Add(record);
                }

                #endregion
            }
        }

        #endregion

        #region EmployeeGroup

        private List<EmployeeGroup> GetWtEmployeeGroups(SqlConnection wtConnection, int wtCompanyId, Company company, List<TimeDeviationCause> timeDeviationCauses)
        {
            List<EmployeeGroup> employeeGroups = new List<EmployeeGroup>();

            #region Prereq

            int? timeDeviationCauseStdId = GetTimeDeviationCauseStandardId(timeDeviationCauses);

            #endregion

            SqlCommand cmd = GetCommand(wtConnection, GET_EMPLOYEE_GROUPS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeGroupId = dr.GetInt32(0);
                    string name = dr.GetString(1);
                    bool autogenTimeBlocks = dr.GetBoolean(3);
                    bool specialDay = dr.GetBoolean(4);
                    bool isDefaultEmployeeGroup = dr.GetBoolean(5);

                    #region EmployeeGroup

                    EmployeeGroup employeeGroup = new EmployeeGroup()
                    {
                        Name = StringUtility.Left(name, 100),
                        AutogenTimeblocks = autogenTimeBlocks,
                        PayrollProductAccountingPrio = "0,0,0,0,0",
                        InvoiceProductAccountingPrio = "0,0,0,0,0",
                        DeviationAxelStartHours = 5,
                        DeviationAxelStopHours = 5,

                        //Set FK
                        TimeDeviationCauseId = timeDeviationCauseStdId,
                    };
                    SetCreatedProperties(employeeGroup);

                    //Add to Company
                    company.EmployeeGroup.Add(employeeGroup);

                    //Set default
                    if (isDefaultEmployeeGroup)
                        this.defaultEmployeeGroup = employeeGroup;

                    //Couple to DayTypes depending on wether they work on "specialdays" or not
                    if (specialDay)
                    {
                        foreach (DayType dayType in this.exportedDayTypesSpecial)
                        {
                            employeeGroup.DayType.Add(dayType);
                        }
                    }
                    else
                    {
                        foreach (DayType dayType in this.exportedDayTypes)
                        {
                            employeeGroup.DayType.Add(dayType);
                        }
                    }

                    //Add to collection
                    employeeGroups.Add(employeeGroup);

                    //Add to dict
                    this.exportedEmployeeGroupsDict.Add(wtEmployeeGroupId, employeeGroup);

                    #endregion
                }
            }

            return employeeGroups;
        }

        #endregion

        #region EmployeeSchedule

        private List<EmployeeSchedule> GetWtEmployeeSchedule(CompEntities entities, SqlConnection wtConnection, int wtCompanyId)
        {
            List<EmployeeSchedule> employeeSchedules = new List<EmployeeSchedule>();

            SqlCommand cmd = GetCommand(wtConnection, GET_EMPLOYEESCHEDULE, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtTemplateHeadId = dr.GetInt32(0);
                    int wtEmployeeId = dr.GetInt32(1);
                    DateTime startDate = dr.GetDateTime(2);
                    DateTime stopDate = dr.GetDateTime(3);
                    int startDayNumber = dr.GetInt32(4);

                    int dayNumber = CalendarUtility.GetDayNr(startDate);

                    Employee employee = GetExportedEmployee(wtEmployeeId);

                    TimeScheduleTemplateHead templateHead = GetExportedTimeScheduleTemplateHead(wtTemplateHeadId);

                    if (employee != null && templateHead != null)
                    {
                        #region EmployeeSchedule

                        EmployeeSchedule employeeSchedule = new EmployeeSchedule
                        {
                            StartDate = startDate,
                            StopDate = stopDate,
                            StartDayNumber = startDayNumber + dayNumber - 1, //Rickard said it is ok. Proc returns (week*7)-6
                            State = (int)SoeEntityState.Active,

                            //Set FK
                            EmployeeId = employee.EmployeeId,
                            TimeScheduleTemplateHeadId = templateHead.TimeScheduleTemplateHeadId,
                        };
                        SetCreatedProperties(employeeSchedule);
                        entities.EmployeeSchedule.AddObject(employeeSchedule);

                        //Add to collection
                        employeeSchedules.Add(employeeSchedule);

                        #endregion
                    }
                }
            }

            return employeeSchedules;
        }

        #endregion

        #region Feature

        private List<LicenseArticle> AddLicenseArticle(CompEntities entities, License license, List<SoeXeArticle> xeArticles)
        {
            List<LicenseArticle> licenseArticles = new List<LicenseArticle>();
            if (xeArticles == null || license == null)
                return licenseArticles;

            if (license.LicenseArticle == null)
                license.LicenseArticle = new EntityCollection<LicenseArticle>();

            foreach (SoeXeArticle xeArticle in xeArticles)
            {
                #region XeArticle

                int sysXEArticleId = (int)xeArticle;

                if (!license.LicenseArticle.Any(i => i.SysXEArticleId == sysXEArticleId))
                {
                    LicenseArticle licenseArticle = new LicenseArticle()
                    {
                        SysXEArticleId = sysXEArticleId,
                    };
                    SetCreatedProperties(licenseArticle);
                    entities.LicenseArticle.AddObject(licenseArticle);

                    //Add to License
                    license.LicenseArticle.Add(licenseArticle);

                    //Add to collection
                    licenseArticles.Add(licenseArticle);
                }

                #endregion
            }

            return licenseArticles;
        }

        #endregion

        #region FlexForce

        private CompanyFlexForce GetWtFlexForceAPIKey(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            StringBuilder apiKeys = new StringBuilder();

            SqlCommand cmd = GetCommand(wtConnection, GET_FLEXFORCE_APIKEY, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (apiKeys.Length > 0)
                        apiKeys.Append(",");
                    apiKeys.Append(dr.GetString(0));
                }
            }

            #region CompanyFlexForce

            CompanyFlexForce companyFlexForce = new CompanyFlexForce()
            {
                ApiKeys = apiKeys.ToString(),

                //Set references
                Company = company,
            };
            SetCreatedProperties(companyFlexForce);
            if (apiKeys.Length > 0)
            {
                entities.CompanyFlexForce.AddObject(companyFlexForce);
            }
            #endregion

            return companyFlexForce;
        }

        private void GetWtFlexForceSync(SqlConnection wtConnection, int wtCompanyId)
        {
            SqlCommand cmd = GetCommand(wtConnection, GET_FLEXFORCE_SYNC, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeId = dr.GetInt32(0);

                    Employee employee = GetExportedEmployee(wtEmployeeId);
                    if (employee != null)
                        employee.UseFlexForce = true;
                }
            }
        }

        #endregion

        #region License

        private License GetWtLicense(CompEntities entities, SqlConnection wtConnection, int wtCompanyId)
        {
            License license = null;

            int? actorCompanyId = null;

            //Check if we should use an old license in Xe, or create a new one
            SqlCommand cmd = GetCommand(wtConnection, GET_COUPLEDCOMPANIES, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    actorCompanyId = dr.GetInt32(0);
                }
            }

            if (actorCompanyId.HasValue)
            {
                #region Get License

                license = LicenseManager.GetLicenseByCompany(entities, actorCompanyId.Value, true, true, true);

                #endregion
            }
            else
            {
                #region Add License

                cmd = GetCommand(wtConnection, GET_COMPANY, wtCompanyId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        #region License

                        license = new License()
                        {
                            Name = StringUtility.Left(dr.GetString(0), 100),
                            LicenseNr = LicenseManager.GetNextLicenseNr(),
                            ConcurrentUsers = dr.GetInt32(1),
                            MaxNrOfEmployees = dr.GetInt32(1),
                            MaxNrOfUsers = dr.GetInt32(1),
                            NrOfCompanies = 1,
                            OrgNr = StringUtility.Left(dr.GetString(3), 50),
                            IsAccountingOffice = false,
                            AccountingOfficeId = 0,
                            AccountingOfficeName = String.Empty
                        };
                        SetCreatedProperties(license);
                        entities.License.AddObject(license);

                        #endregion
                    }
                }

                #endregion
            }

            return license;
        }

        #endregion

        #region Products

        private List<PayrollProduct> GetWtPayrollProducts(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, List<ProductUnit> productUnits)
        {
            List<PayrollProduct> payrollProducts = new List<PayrollProduct>();

            SqlCommand cmd = GetCommand(wtConnection, GET_PAYROLLPRODUCTS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtPayrollProductId = dr.GetInt32(0);
                    string name = dr.GetString(1);
                    string number = dr.GetString(2);
                    bool isAbsence = dr.GetBoolean(3);
                    decimal factor = dr.GetDecimal(4);
                    bool hide = dr.GetBoolean(5);
                    string unitName = dr.GetString(6);
                    string accountNr = dr.GetString(7);

                    //unitName != Tim --> Addition and Quantity
                    bool addition = unitName != null && unitName.ToLower() != "tim";
                    ProductUnit productUnit = GetProductUnit(unitName, productUnits);

                    #region PayrollProduct

                    PayrollProduct payrollProduct = new PayrollProduct()
                    {
                        Name = StringUtility.Left(name, 256),
                        Number = StringUtility.Left(number, 100),
                        Type = (int)SoeProductType.PayrollProduct,
                        Factor = factor,
                        State = (hide ? (int)SoeEntityState.Inactive : (int)SoeEntityState.Active),
                        ShortName = StringUtility.Left(name, 10),
                        AccountingPrio = "0,0,0,0,0,0,0",
                        Export = true,

                        //Set references
                        ProductUnit = productUnit,
                    };
                    SetCreatedProperties(payrollProduct);

                    //PayrollType
                    if (isAbsence)
                    {
                        payrollProduct.SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary;
                        payrollProduct.SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence;
                    }
                    else
                    {
                        if (addition)
                        {
                            payrollProduct.SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_Compensation;
                        }
                        else
                        {
                            if (name.ToLower().StartsWith("övertid"))
                            {
                                payrollProduct.SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary;
                                payrollProduct.SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation;

                            }
                            else if (name.ToLower().StartsWith("mertid"))
                            {
                                payrollProduct.SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary;
                                payrollProduct.SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime;
                            }
                            else if (name.ToLower().StartsWith("ob"))
                            {
                                payrollProduct.SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary;
                                payrollProduct.SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition;
                            }
                            else
                            {
                                payrollProduct.SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_GrossSalary;
                                payrollProduct.SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary;
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(accountNr) && !accountNr.Equals("0"))
                    {
                        #region ProductAccountStd

                        payrollProduct.ProductAccountStd.Add(new ProductAccountStd()
                        {
                            Percent = 100,
                            Type = (int)ProductAccountType.Purchase,

                            //Set references
                            AccountStd = GetAccountStd(entities, accountNr, name, company.ActorCompanyId),
                        });

                        #endregion
                    }

                    //Add to Company
                    company.Product.Add(payrollProduct);

                    //Add to collection
                    payrollProducts.Add(payrollProduct);

                    //Add to dict
                    this.exportedPayrollProductsDict.Add(wtPayrollProductId, payrollProduct);

                    #endregion
                }
            }

            return payrollProducts;
        }

        #endregion

        #region ProductUnits

        private List<ProductUnit> GetWtProductUnits(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            List<ProductUnit> productUnits = new List<ProductUnit>();

            SqlCommand cmd = GetCommand(wtConnection, GET_UNITS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region ProductUnit

                    ProductUnit productUnit = new ProductUnit()
                    {
                        Name = StringUtility.Left(dr.GetString(0), 100),
                        Code = StringUtility.Left(dr.GetString(0), 20),

                        //Set references
                        Company = company,
                    };
                    SetCreatedProperties(productUnit);
                    entities.ProductUnit.AddObject(productUnit);

                    //Add to collection
                    productUnits.Add(productUnit);

                    #endregion
                }
            }

            return productUnits;
        }

        #endregion

        #region Reports

        private void AddReports(Company company)
        {
            //Tid grundmall
            int templateCompanyId = 30472;

            Dictionary<int, Report> reportMapping = new Dictionary<int, Report>();
            Dictionary<int, Role> roleMapping = new Dictionary<int, Role>();
            CompanyManager.CopyReportsFromTemplateCompany(company.ActorCompanyId, templateCompanyId, false, false, false, false, false, ref reportMapping, ref roleMapping);
        }

        #endregion

        #region Role

        private List<Role> GetWtRoles(CompEntities entities, Company company, License license)
        {
            List<Role> roles = new List<Role>();

            #region Role

            //Systemadmin
            Role roleAdmin = new Role()
            {
                TermId = (int)TermGroup_Roles.Systemadmin,

                //Set references
                Company = company,
            };
            SetCreatedProperties(roleAdmin);
            roles.Add(roleAdmin);

            //Employee
            Role roleEmployee = new Role()
            {
                TermId = (int)TermGroup_Roles.Employee,

                //Set references
                Company = company,
            };
            SetCreatedProperties(roleEmployee);
            roles.Add(roleEmployee);

            //Approval
            Role roleApproval = new Role()
            {
                TermId = (int)TermGroup_Roles.Approval,

                //Set references
                Company = company,
            };
            SetCreatedProperties(roleApproval);
            roles.Add(roleApproval);

            //Attest
            Role roleAttest = new Role()
            {
                TermId = (int)TermGroup_Roles.Attest,

                //Set references
                Company = company,
            };
            SetCreatedProperties(roleAttest);
            roles.Add(roleAttest);

            #endregion

            #region SysUser

            string loginName = Constants.APPLICATION_LICENSEADMIN_LOGINNAME;

            this.sysUser = license.User.FirstOrDefault(u => u.LoginName == loginName);
            if (this.sysUser == null)
            {
                this.sysUser = new User()
                {
                    LoginName = loginName,
                    Name = StringUtility.Left("Admin", 100),
                    passwordhash = LoginManager.GetPasswordHash(loginName, Path.GetRandomFileName().Replace(".", "")),
                    LangId = Constants.SYSLANGUAGE_SYSLANGUAGEID_DEFAULT,
                    SysUser = true,

                    //References
                    License = license,
                };
                SetCreatedProperties(this.sysUser);
            }

            #endregion

            #region UserCompanyRole

            //Map default user to admin Role
            UserCompanyRole userCompanyRole = new UserCompanyRole()
            {
                //Set references
                User = this.sysUser,
                Company = company,
                Role = roleAdmin,
            };
            entities.UserCompanyRole.AddObject(userCompanyRole);

            #endregion

            return roles;
        }

        #endregion

        #region Schedule

        private List<TimeScheduleTemplateHead> GetWtTimeScheduleTemplateHead(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            List<TimeScheduleTemplateHead> templateHeads = new List<TimeScheduleTemplateHead>();

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMESCHEDULETEMPLATEHEAD, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtTemplateHeadId = dr.GetInt32(0);
                    string name = dr.GetString(1);
                    int noOfDays = dr.GetInt32(3);

                    #region TimeScheduleTemplateHead

                    TimeScheduleTemplateHead templateHead = new TimeScheduleTemplateHead()
                    {
                        Name = name,
                        Description = String.Empty,
                        StartOnFirstDayOfWeek = true,
                        NoOfDays = noOfDays,
                        State = (int)SoeEntityState.Active,

                        //Set references
                        Company = company,
                    };
                    SetCreatedProperties(templateHead);
                    entities.TimeScheduleTemplateHead.AddObject(templateHead);

                    //Add to collection
                    templateHeads.Add(templateHead);

                    //Add to dict
                    this.exportedTimeScheduleTemplateHeadsDict.Add(wtTemplateHeadId, templateHead);

                    #endregion
                }
            }

            return templateHeads;
        }

        private List<TimeScheduleTemplatePeriod> GetWtTimeScheduleTemplatePeriod(CompEntities entities, SqlConnection wtConnection, int wtCompanyId)
        {
            List<TimeScheduleTemplatePeriod> templatePeriods = new List<TimeScheduleTemplatePeriod>();

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMESCHEDULETEMPLATEPERIOD, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtTemplatePeriodId = dr.GetInt32(0);
                    int wtTemplateHeadId = dr.GetInt32(1);
                    int dayNumber = dr.GetInt32(2);

                    #region TimeScheduleTemplatePeriod

                    TimeScheduleTemplatePeriod templatePeriod = new TimeScheduleTemplatePeriod()
                    {
                        DayNumber = dayNumber,
                        State = (int)SoeEntityState.Active,

                        //Set references
                        TimeScheduleTemplateHead = GetExportedTimeScheduleTemplateHead(wtTemplateHeadId),
                    };
                    SetCreatedProperties(templatePeriod);
                    entities.TimeScheduleTemplatePeriod.AddObject(templatePeriod);

                    //Add to collection
                    templatePeriods.Add(templatePeriod);

                    //Add to dict
                    this.exportedTimeScheduleTemplatePeriodsDict.Add(wtTemplatePeriodId, templatePeriod);

                    #endregion
                }
            }

            return templatePeriods;
        }

        private List<TimeScheduleTemplateBlock> GetWtTimeScheduleTemplateBlocks(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, List<TimeCodeBreak> timeCodeBreaks)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = new List<TimeScheduleTemplateBlock>();

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMESCHEDULETEMPLATEBLOCKS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtTemplatePeriodId = dr.GetInt32(0);
                    DateTime startTime = dr.GetDateTime(2);
                    DateTime stopTime = dr.GetDateTime(3);
                    int breakLength = dr.GetInt32(4);


                    TimeScheduleTemplatePeriod templatePeriod = GetExportedTimeScheduleTemplatePeriod(wtTemplatePeriodId);
                    TimeCode timeCode = GetExportedTimeCode(WT_TIMECODE_WORK);


                    #region TimeScheduleTemplateBlock

                    TimeScheduleTemplateBlock templateBlock = new TimeScheduleTemplateBlock()
                    {
                        StartTime = startTime,
                        StopTime = stopTime,
                        BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.None,
                        State = (int)SoeEntityState.Active,

                        //Set references
                        TimeScheduleTemplatePeriod = templatePeriod,
                        TimeCode = timeCode,
                    };
                    SetCreatedProperties(templateBlock);
                    entities.TimeScheduleTemplateBlock.AddObject(templateBlock);

                    //Add to collection
                    templateBlocks.Add(templateBlock);

                    #endregion

                    #region TimeScheduleTemplateBlock break

                    if (breakLength > 0)
                    {
                        TimeCodeBreak timeCodeBreak = timeCodeBreaks.FirstOrDefault(i => i.DefaultMinutes == breakLength);

                        //Calculate break in middle of day
                        int startTimeMinutes = CalendarUtility.TimeToMinutes(startTime);
                        int stopTimeMinutes = CalendarUtility.TimeToMinutes(stopTime);
                        CalendarUtility.GetTimeInMiddle(breakLength, startTimeMinutes, stopTimeMinutes, out startTime, out stopTime);

                        TimeScheduleTemplateBlock templateBlockBreak = new TimeScheduleTemplateBlock()
                        {
                            StartTime = startTime,
                            StopTime = stopTime,
                            BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.NormalBreak,
                            State = (int)SoeEntityState.Active,

                            //Set references
                            TimeScheduleTemplatePeriod = templatePeriod,
                            TimeCode = timeCodeBreak,
                        };
                        SetCreatedProperties(templateBlockBreak);
                        entities.TimeScheduleTemplateBlock.AddObject(templateBlockBreak);

                        //Add to collection
                        templateBlocks.Add(templateBlockBreak);
                    }

                    #endregion
                }
            }

            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetWtTimeScheduleTemplateBlocksEmployee(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, List<EmployeeSchedule> employeeSchedules, List<TimeCodeBreak> timeCodeBreaks, DateTime fromDate, DateTime toDate)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = new List<TimeScheduleTemplateBlock>();

            AccountDim accountDimCostCentre = company.AccountDim.FirstOrDefault(i => i.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMESCHEDULETEMPLATEBLOCKS_EMPLOYEE, wtCompanyId, fromDate, toDate);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeId = dr.GetInt32(1);
                    DateTime startTime = dr.GetDateTime(2);
                    DateTime stopTime = dr.GetDateTime(3);
                    int breakLength = dr.GetInt32(4);
                    DateTime date = dr.GetDateTime(5);
                    string accountInternalNr = dr.GetString(6);

                    Employee employee = GetExportedEmployee(wtEmployeeId);

                    if (employee == null)
                        return templateBlocks;


                    TimeScheduleTemplatePeriod templatePeriod = GetTemplatePeriod(employee, employeeSchedules, date);
                    TimeCode timeCode = GetExportedTimeCode(WT_TIMECODE_WORK);

                    // Find shifttype
                    if (!String.IsNullOrEmpty(accountInternalNr) && accountInternalNr.StartsWith(";"))
                        accountInternalNr = accountInternalNr.Substring(1, accountInternalNr.Length - 1);
                    ShiftType shiftType = company.ShiftType.FirstOrDefault(i => i.ExternalCode == accountInternalNr);
                    if (shiftType == null)
                        shiftType = TimeScheduleManager.GetShiftTypeFromExtCode(entities, accountInternalNr, company.ActorCompanyId);

                    EmployeeSchedule employeeSchedule = GetEmployeeSchedule(employee, employeeSchedules, date);

                    if (templatePeriod != null)
                    {
                        #region TimeScheduleEmployeePeriod



                        TimeScheduleEmployeePeriod timeScheduleEmployeePeriod = new TimeScheduleEmployeePeriod()
                        {
                            EmployeeId = employee.EmployeeId,                            
                            ActorCompanyId = company.ActorCompanyId,
                            Date = date.Date,                            
                        };
                        SetCreatedProperties(timeScheduleEmployeePeriod);
                        entities.TimeScheduleEmployeePeriod.AddObject(timeScheduleEmployeePeriod);

                        #endregion

                        #region TimeScheduleTemplateBlock

                        TimeScheduleTemplateBlock templateBlock = new TimeScheduleTemplateBlock()
                        {
                            Date = date.Date,
                            StartTime = startTime,
                            StopTime = stopTime,
                            BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.None,
                            State = (int)SoeEntityState.Active,
                            ShiftStatus = (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned,
                            ShiftUserStatus = (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted,


                            //Set references
                            TimeScheduleTemplatePeriod = templatePeriod,
                            TimeScheduleEmployeePeriod = timeScheduleEmployeePeriod,
                            ShiftType = shiftType,

                            //Set FK
                            EmployeeId = employee.EmployeeId,
                            TimeCodeId = timeCode.TimeCodeId,
                        };

                        #region AccountInternal

                        //Remove ; (ex: ;11 --> 11)
                        if (!String.IsNullOrEmpty(accountInternalNr) && accountInternalNr.StartsWith(";"))
                            accountInternalNr = accountInternalNr.Substring(1, accountInternalNr.Length - 1);

                        //AccountInternal
                        if (!String.IsNullOrEmpty(accountInternalNr) && !accountInternalNr.Contains(';'))
                        {
                            //AccountInternals for PayrollTransactions that not corresponds to a Category (i.e. was converted in GetWtCategories) will be skipped
                            AccountInternal accountInternal = accountDimCostCentre.Account.Where(i => i.AccountNr == accountInternalNr).Select(i => i.AccountInternal).FirstOrDefault();
                            if (accountInternal != null)
                            {
                                //Add AccountInternal
                                templateBlock.AccountInternal.Add(accountInternal);
                            }
                        }

                        #endregion

                        SetCreatedProperties(templateBlock);
                        entities.TimeScheduleTemplateBlock.AddObject(templateBlock);

                        //Add to collection
                        templateBlocks.Add(templateBlock);

                        #endregion

                        #region TimeScheduleTemplateBlock break

                        if (breakLength > 0)
                        {
                            TimeCodeBreak timeCodeBreak = timeCodeBreaks.FirstOrDefault(i => i.DefaultMinutes == breakLength);

                            //Calculate break in middle of day
                            int startTimeMinutes = CalendarUtility.TimeToMinutes(startTime);
                            int stopTimeMinutes = CalendarUtility.TimeToMinutes(stopTime);
                            CalendarUtility.GetTimeInMiddle(breakLength, startTimeMinutes, stopTimeMinutes, out startTime, out stopTime);

                            TimeScheduleTemplateBlock templateBlockBreak = new TimeScheduleTemplateBlock()
                            {
                                Date = date,
                                StartTime = startTime,
                                StopTime = stopTime,
                                BreakType = (int)SoeTimeScheduleTemplateBlockBreakType.NormalBreak,
                                State = (int)SoeEntityState.Active,

                                //Set references
                                TimeScheduleTemplatePeriod = templatePeriod,
                                TimeCode = timeCodeBreak,
                                TimeScheduleEmployeePeriod = timeScheduleEmployeePeriod,
                                //Employee = employee,

                                // set FK
                                EmployeeId = employee.EmployeeId
                            };
                            SetCreatedProperties(templateBlockBreak);
                            entities.TimeScheduleTemplateBlock.AddObject(templateBlockBreak);

                            //Add to collection
                            templateBlocks.Add(templateBlockBreak);
                        }

                        #endregion
                    }
                }
            }

            return templateBlocks;
        }

        #endregion

        #region Stamping

        private List<TimeTerminal> GetWtTerminals(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            List<TimeTerminal> timeTerminals = new List<TimeTerminal>();

            SqlCommand cmd = GetCommand(wtConnection, GET_TERMINALS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    string name = dr.GetString(0);
                    string macAddress = dr.GetString(1);
                    string macName = dr.GetString(2);
                    int macNumber = dr.GetInt32(3);

                    #region TimeTerminal

                    TimeTerminal timeTerminal = new TimeTerminal()
                    {
                        Name = StringUtility.Left(name, 50),
                        MacAddress = StringUtility.Left(macAddress, 100),
                        MacName = StringUtility.Left(macName, 100),
                        MacNumber = macNumber,
                        Type = (int)TimeTerminalType.TimeSpot,
                        TimeTerminalGuid = Guid.NewGuid(),

                        //Set references
                        Company = company,
                    };
                    SetCreatedProperties(timeTerminal);
                    entities.TimeTerminal.AddObject(timeTerminal);

                    //Add to collection
                    timeTerminals.Add(timeTerminal);

                    //Add to dict
                    this.exportedTimeTerminalsDict.Add(dr.GetInt32(6), timeTerminal);

                    #endregion
                }
            }

            return timeTerminals;
        }

        private List<TimeStampEntry> GetWtTimeStampEntries(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, List<TimeDeviationCause> timeDeviationCauses, DateTime fromDate, DateTime toDate)
        {
            List<TimeStampEntry> timeStampEntries = new List<TimeStampEntry>();

            AccountDim accountDimCostCentre = company.AccountDim.FirstOrDefault(i => i.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMESTAMPS, wtCompanyId, fromDate, toDate);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeId = dr.GetInt32(0);
                    int type = dr.GetInt32(1);
                    int wtTimeTerminalId = dr.GetInt32(2);
                    DateTime time = dr.GetDateTime(3);
                    string deviationCauseName = dr.GetString(4);
                    DateTime WTtimeBlockDate = dr.GetDateTime(5);
                    string accountInternalNr = !dr.IsDBNull(6) ? dr.GetString(6) : "";


                    TermGroup_TimeStampEntryType timeStampEntryType = TermGroup_TimeStampEntryType.Unknown;
                    if (type == 0)
                        timeStampEntryType = TermGroup_TimeStampEntryType.In;
                    else if (type == 1)
                        timeStampEntryType = TermGroup_TimeStampEntryType.Out;

                    Employee employee = GetExportedEmployee(wtEmployeeId);

                    if (employee == null)
                        return timeStampEntries;

                    TimeTerminal timeTerminal = GetExportedTimeTerminal(wtTimeTerminalId);
                    if (timeTerminal == null)
                        timeTerminal = GetFirstExportedTimeTerminal();
                    TimeBlockDate timeBlockDate = GetTimeBlockDate(entities, WTtimeBlockDate.Date, wtEmployeeId, wtCompanyId);
                    TimeDeviationCause timeDeviationCause = GetTimeDeviationCause(timeDeviationCauses, deviationCauseName);
                    if (timeDeviationCause == null)
                        timeDeviationCause = GetTimeDeviationCauseStandard(timeDeviationCauses);

                    #region TimeStampEntry

                    TimeStampEntry timeStampEntry = new TimeStampEntry()
                    {
                        Type = (int)timeStampEntryType,
                        Status = (int)TermGroup_TimeStampEntryStatus.Processed,
                        OriginType = (int)TermGroup_TimeStampEntryOriginType.TerminalByEmployeeNumber,
                        Time = time,
                        OriginalTime = time,
                        ManuallyAdjusted = false,
                        EmployeeManuallyAdjusted = false,
                        Note = "",
                        State = (int)SoeEntityState.Active,

                        //Set references
                        Company = company,
                        TimeBlockDate = timeBlockDate,
                        TimeDeviationCause = timeDeviationCause,
                        TimeTerminal = timeTerminal,

                        //Set FK
                        EmployeeId = employee.EmployeeId,
                    };

                    #region AccountInternal

                    //Remove ; (ex: ;11 --> 11)
                    if (!String.IsNullOrEmpty(accountInternalNr) && accountInternalNr.StartsWith(";"))
                        accountInternalNr = accountInternalNr.Substring(1, accountInternalNr.Length - 1);

                    //AccountInternal
                    if (!String.IsNullOrEmpty(accountInternalNr) && !accountInternalNr.Contains(';'))
                    {
                        //AccountInternals for PayrollTransactions that not corresponds to a Category (i.e. was converted in GetWtCategories) will be skipped
                        AccountInternal accountInternal = accountDimCostCentre.Account.Where(i => i.AccountNr == accountInternalNr).Select(i => i.AccountInternal).FirstOrDefault();
                        if (accountInternal != null)
                        {
                            //Add AccountInternal
                            timeStampEntry.AccountId = accountInternal.AccountId;
                        }
                    }
                    #endregion

                    SetCreatedProperties(timeStampEntry);
                    entities.TimeStampEntry.AddObject(timeStampEntry);

                    //Add to collection
                    timeStampEntries.Add(timeStampEntry);

                    #endregion
                }
            }

            return timeStampEntries;
        }

        #endregion

        #region TimeAccumulator

        private List<TimeAccumulator> GetWtTimeAccumulators(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            List<TimeAccumulator> timeAccumulators = new List<TimeAccumulator>();

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMEACCUMULATOR, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeGroupId = dr.GetInt32(0);
                    string name = dr.GetString(1);

                    #region TimeCodeWork

                    TimeCodeWork timeCodeWork = GetExportedTimeCodeTimeAccumulator(name);
                    if (timeCodeWork == null)
                    {
                        timeCodeWork = new TimeCodeWork
                        {
                            Name = name,
                            Description = String.Format(name + " {0}", "Konverterat från webbtid"),
                            Type = (int)SoeTimeCodeType.Work,
                            Code = StringUtility.Left(name, 20),
                            RoundingType = (int)TermGroup_TimeCodeRoundingType.None,
                            RoundingValue = 0,
                            IsWorkOutsideSchedule = false,

                            //Set references
                            Company = company,
                        };
                        SetCreatedProperties(timeCodeWork);
                        entities.TimeCode.AddObject(timeCodeWork);

                        //Add to dict
                        this.exportedTimeCodesTimeAccumulatorDict.Add(name, timeCodeWork);
                    }

                    #endregion

                    #region TimeAccumulator

                    TimeAccumulator timeAccumulator = GetExportedTimeAccumulator(name);
                    if (timeAccumulator == null)
                    {
                        timeAccumulator = new TimeAccumulator()
                        {
                            Name = name,
                            Description = String.Format(name + " {0}", "Konverterad från webbtid"),
                            ShowInTimeReports = true,
                            Type = 1,
                            State = (int)SoeEntityState.Active,

                            //Set references
                            Company = company,
                        };
                        entities.TimeAccumulator.AddObject(timeAccumulator);
                        timeAccumulators.Add(timeAccumulator);

                        //Add to dict
                        this.exportedTimeAccumulatorsDict.Add(name, timeAccumulator);
                    }

                    #endregion

                    #region TimeAccumulatorEmployeeGroupRule

                    TimeAccumulatorEmployeeGroupRule employeeGroupRule = new TimeAccumulatorEmployeeGroupRule()
                    {
                        Type = (int)TermGroup_AccumulatorTimePeriodType.AccToday,
                        State = (int)SoeEntityState.Active,

                        //Set references
                        EmployeeGroup = GetExportedEmployeeGroup(wtEmployeeGroupId),
                        TimeAccumulator = timeAccumulator,
                    };
                    SetCreatedProperties(employeeGroupRule);
                    entities.TimeAccumulatorEmployeeGroupRule.AddObject(employeeGroupRule);

                    #endregion
                }
            }

            return timeAccumulators;
        }



        private List<TimeCodeTransaction> GetWtTimeAccumulatorTransactions(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, List<TimeBlock> timeBlocks, DateTime fromDate, DateTime toDate)
        {
            List<TimeCodeTransaction> timeCodeTransactions = new List<TimeCodeTransaction>();

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMEACCUMULATOR_TRANSACTIONS, wtCompanyId, fromDate, toDate);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeId = dr.GetInt32(1);
                    string name = dr.GetString(2);
                    int quantity = dr.GetInt32(3);
                    DateTime date = dr.GetDateTime(4);

                    Employee employee = GetExportedEmployee(wtEmployeeId);

                    if (employee == null)
                        return timeCodeTransactions;


                    TimeCode timeCode = GetExportedTimeCodeTimeAccumulator(name);

                    if (timeCode != null)
                    {
                        TimeBlock timeBlock = (from tb in timeBlocks
                                               where tb.EmployeeId == employee.EmployeeId &&
                                               tb.TimeBlockDate.Date == date &&
                                               !tb.IsBreak
                                               orderby tb.StartTime
                                               select tb).FirstOrDefault();

                        #region TimeCodeTransaction

                        TimeCodeTransaction timeCodeTransaction = TimeTransactionManager.CreateTimeCodeTransaction(entities, company.ActorCompanyId, timeCode, timeBlock, "Konverterat från webbbtid", quantity);

                        //Add to collection
                        timeCodeTransactions.Add(timeCodeTransaction);

                        #endregion
                    }
                }
            }

            return timeCodeTransactions;
        }

        #endregion

        #region TimeBlock

        private List<TimeBlock> GetWtTimeBlocks(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, List<EmployeeSchedule> employeeSchedules, List<TimeScheduleTemplateBlock> timeScheduleTemplateBlocksEmployee, List<TimeStampEntry> timeStampEntries, DateTime fromDate, DateTime toDate)
        {
            List<TimeBlock> timeBlocks = new List<TimeBlock>();

            #region Prereq

            AccountDim accountDimCostCentre = company.AccountDim.FirstOrDefault(i => i.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);

            #endregion

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMEBLOCKS, wtCompanyId, fromDate, toDate);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtEmployeeId = dr.GetInt32(0);
                    DateTime startTime = dr.GetDateTime(1);
                    DateTime endTime = dr.GetDateTime(2);
                    DateTime date = dr.GetDateTime(3);
                    string timeCodeName = dr.GetString(4);
                    bool isBreak = dr.GetBoolean(5);
                    string accountInternalNr = !dr.IsDBNull(6) ? dr.GetString(6) : "";

                    Employee employee = GetExportedEmployee(wtEmployeeId);

                    if (employee == null)
                        return timeBlocks;


                    TimeBlockDate timeBlockDate = GetTimeBlockDate(entities, date, wtEmployeeId, wtCompanyId);
                    TimeScheduleTemplatePeriod templatePeriod = GetTemplatePeriod(employee, employeeSchedules, date);

                    if (templatePeriod != null)
                    {
                        #region TimeBlock

                        TimeBlock timeBlock = new TimeBlock()
                        {
                            StartTime = startTime,
                            StopTime = endTime,
                            IsBreak = isBreak,
                            ManuallyAdjusted = false,
                            State = (int)SoeEntityState.Active,

                            //Set references
                            TimeBlockDate = timeBlockDate,

                            //Set FK
                            EmployeeId = GetExportedEmployeeId(wtEmployeeId),
                            TimeScheduleTemplatePeriodId = templatePeriod.TimeScheduleTemplatePeriodId,
                        };
                        SetCreatedProperties(timeBlock);
                        entities.TimeBlock.AddObject(timeBlock);

                        #region TimeCode

                        int wtTimeCodeId;
                        if (!Int32.TryParse(timeCodeName, out wtTimeCodeId))
                            wtTimeCodeId = WT_TIMECODE_WORK;

                        TimeCode timeCode = null;
                        if (isBreak)
                        {
                            //Get TimeCode from TimeScheduleTemplateBlock break
                            TimeScheduleTemplateBlock templateBlockBreak = timeScheduleTemplateBlocksEmployee.FirstOrDefault(i => i.EmployeeId == employee.EmployeeId && i.Date == date && i.TimeCode.IsBreak());
                            if (templateBlockBreak != null)
                                timeCode = templateBlockBreak.TimeCode;
                            else
                                timeCode = this.exportedTimeCodesDict[WT_TIMECODE_BREAK];

                            //TimeScheduleTemplateBlockBreak
                            if (templateBlockBreak != null)
                                timeBlock.TimeScheduleTemplateBlockBreakId = templateBlockBreak.TimeScheduleTemplateBlockId;
                        }
                        else
                        {
                            //Get exported TimeBlock or default
                            if (this.exportedTimeCodesDict.ContainsKey(wtTimeCodeId))
                                timeCode = GetExportedTimeCode(wtTimeCodeId);
                            else
                                timeCode = GetExportedTimeCode(WT_TIMECODE_WORK);
                        }

                        timeBlock.TimeCode.Add(timeCode);

                        #endregion

                        #region TimeStampEntry

                        TimeStampEntry timeStampEntry = timeStampEntries.Where(t => t.EmployeeId == GetExportedEmployeeId(wtEmployeeId) && t.Time.Date == timeBlockDate.Date).OrderBy(i => i.Time).FirstOrDefault();
                        if (timeStampEntry != null)
                        {
                            if (timeBlock.TimeStampEntry == null)
                                timeBlock.TimeStampEntry = new EntityCollection<TimeStampEntry>();
                            timeBlock.TimeStampEntry.Add(timeStampEntry);
                        }

                        #endregion

                        #region Accounts

                        //Remove ; (ex: ;11 --> 11)
                        if (!String.IsNullOrEmpty(accountInternalNr) && accountInternalNr.StartsWith(";"))
                            accountInternalNr = accountInternalNr.Substring(1, accountInternalNr.Length - 1);

                        //AccountInternal
                        if (!String.IsNullOrEmpty(accountInternalNr) && !accountInternalNr.Contains(';') && accountDimCostCentre != null)
                        {
                            //AccountInternals for PayrollTransactions that not corresponds to a Category (i.e. was converted in GetWtCategories) will be skipped
                            AccountInternal accountInternal = accountDimCostCentre.Account.Where(i => i.AccountNr == accountInternalNr).Select(i => i.AccountInternal).FirstOrDefault();
                            if (accountInternal != null)
                                timeBlock.AccountInternal.Add(accountInternal);
                        }

                        #endregion

                        //Add to collection
                        timeBlocks.Add(timeBlock);

                        #endregion
                    }
                }
            }

            return timeBlocks;
        }

        #endregion

        #region TimeCode

        private List<TimeCode> GetWtTimeCodes(CompEntities entities, Company company)
        {
            List<TimeCode> timeCodes = new List<TimeCode>();

            foreach (KeyValuePair<int, PayrollProduct> pair in this.exportedPayrollProductsDict)
            {
                PayrollProduct payrollProduct = pair.Value;

                if (payrollProduct.IsAbsence())
                {
                    #region TimeCodeAbsense

                    TimeCodeAbsense timeCodeAbsence = new TimeCodeAbsense()
                    {
                        Code = StringUtility.Left(payrollProduct.Name, 20),
                        Name = StringUtility.Left(payrollProduct.Name, 100),
                        Type = (int)SoeTimeCodeType.Absense,
                        RegistrationType = (int)TermGroup_TimeCodeRegistrationType.Time,

                        //Set references
                        Company = company
                    };
                    SetCreatedProperties(timeCodeAbsence);
                    entities.TimeCode.AddObject(timeCodeAbsence);

                    //Add to TimeCode
                    timeCodes.Add(timeCodeAbsence);

                    //Add to dict
                    this.exportedTimeCodesDict.Add(pair.Key, timeCodeAbsence);

                    #endregion

                    #region TimeCodePayrollProduct

                    TimeCodePayrollProduct timeCodePayrollProduct = new TimeCodePayrollProduct()
                    {
                        Factor = 1,

                        //Set references
                        TimeCode = timeCodeAbsence,
                        PayrollProduct = payrollProduct,
                    };
                    entities.TimeCodePayrollProduct.AddObject(timeCodePayrollProduct);

                    #endregion
                }
                else
                {
                    #region TimeCodeWork

                    SoeTimeCodeType type = SoeTimeCodeType.Work;
                    TermGroup_TimeCodeRegistrationType registrationType = TermGroup_TimeCodeRegistrationType.Time;

                    //Addition --> Addition and Quantity instead of Work and Time
                    if (payrollProduct.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation)
                    {
                        type = SoeTimeCodeType.AdditionDeduction;
                        registrationType = TermGroup_TimeCodeRegistrationType.Quantity;
                    }

                    TimeCodeWork timeCodeWork = new TimeCodeWork()
                    {
                        Code = StringUtility.Left(payrollProduct.Name, 20),
                        Name = StringUtility.Left(payrollProduct.Name, 100),
                        Type = (int)type,
                        RegistrationType = (int)registrationType,

                        //Set references
                        Company = company
                    };
                    SetCreatedProperties(timeCodeWork);
                    entities.TimeCode.AddObject(timeCodeWork);

                    //Add to TimeCode
                    timeCodes.Add(timeCodeWork);

                    //Add to dict
                    this.exportedTimeCodesDict.Add(pair.Key, timeCodeWork);

                    #endregion

                    #region TimeCodePayrollProduct

                    TimeCodePayrollProduct timeCodePayrollProduct = new TimeCodePayrollProduct()
                    {
                        Factor = 1,

                        //Set references
                        TimeCode = timeCodeWork,
                        PayrollProduct = payrollProduct,
                    };
                    entities.TimeCodePayrollProduct.AddObject(timeCodePayrollProduct);

                    #endregion
                }
            }

            #region TimeCodeAbsense break

            TimeCodeAbsense timeCodeBreak = new TimeCodeAbsense()
            {
                Name = "Rast",
                Code = "Rast",
                Type = (int)SoeTimeCodeType.Absense,
                RegistrationType = (int)TermGroup_TimeCodeRegistrationType.Time,
                RoundingType = (int)TermGroup_TimeCodeRoundingType.RoundUp,
                RoundingValue = 1,

                //Set references
                Company = company,
            };
            SetCreatedProperties(timeCodeBreak);
            entities.TimeCode.AddObject(timeCodeBreak);

            //Add to collection
            timeCodes.Add(timeCodeBreak);

            //Add to dict
            this.exportedTimeCodesDict.Add(WT_TIMECODE_BREAK, timeCodeBreak);

            #endregion

            #region TimeCodeWork work

            TimeCode timeCode = timeCodes.FirstOrDefault(t => t.Name.ToLower() == "arbetad tid");
            if (timeCode != null)
            {
                //Add to dict
                this.exportedTimeCodesDict.Add(WT_TIMECODE_WORK, timeCode);
            }
            else
            {
                TimeCodeWork timeCodeWork = new TimeCodeWork()
                {
                    Name = StringUtility.Left("Arbetad tid", 100),
                    Code = StringUtility.Left("Arbetad tid", 20),
                    Type = (int)SoeTimeCodeType.Work,
                    RegistrationType = (int)TermGroup_TimeCodeRegistrationType.Time,

                    //Set references
                    Company = company
                };
                SetCreatedProperties(timeCodeWork);
                entities.TimeCode.AddObject(timeCodeWork);

                //Add to collection
                timeCodes.Add(timeCodeWork);

                //Add to dict
                this.exportedTimeCodesDict.Add(WT_TIMECODE_WORK, timeCodeWork);
            }

            #endregion

            return timeCodes;
        }

        private List<TimeCodeBreak> GetWtTimeCodeBreaks(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            List<TimeCodeBreak> timeCodeBreaks = new List<TimeCodeBreak>();

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMECODEBREAKS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region TimeCodeBreak

                    int timeCodeLength = dr.GetInt32(0);

                    TimeCodeBreak timeCodeBreak = new TimeCodeBreak()
                    {
                        Name = timeCodeLength.ToString() + " min rast",
                        Code = timeCodeLength.ToString(),
                        Type = (int)SoeTimeCodeType.Break,
                        RegistrationType = (int)TermGroup_TimeCodeRegistrationType.Time,
                        MinMinutes = timeCodeLength,
                        MaxMinutes = timeCodeLength,
                        DefaultMinutes = timeCodeLength,
                        StartType = (int)SoeTimeCodeBreakTimeType.ScheduleIn,
                        StopType = (int)SoeTimeCodeBreakTimeType.ScheduleOut,
                        StartTimeMinutes = 0,
                        StopTimeMinutes = 0,
                        RoundingType = (int)TermGroup_TimeCodeRoundingType.RoundUp,
                        RoundingValue = 1,
                        State = (int)SoeEntityState.Active,

                        //Set references
                        Company = company,
                    };
                    SetCreatedProperties(timeCodeBreak);
                    entities.TimeCode.AddObject(timeCodeBreak);

                    //Add to collection
                    timeCodeBreaks.Add(timeCodeBreak);

                    #endregion
                }
            }

            return timeCodeBreaks;
        }

        #endregion

        #region TimeDeviationCause

        private List<TimeDeviationCause> GetWtTimeDeviationCauses(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, List<TimeCode> timecodes)
        {
            List<TimeDeviationCause> timeDeviationCauses = new List<TimeDeviationCause>();

            //Presence
            timeDeviationCauses.AddRange(GetWtPresenceDeviationCauses(entities, wtConnection, wtCompanyId, company));

            //Absence
            timeDeviationCauses.AddRange(GetWtAbsenceDevationCauses(entities, wtConnection, wtCompanyId, company, timecodes));

            return timeDeviationCauses;
        }

        private List<TimeDeviationCause> GetWtPresenceDeviationCauses(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            List<TimeDeviationCause> presenceDeviationCauses = new List<TimeDeviationCause>();

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMEDEVIATIONCAUSES_PRESENCE, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    string name = dr.GetString(0);

                    #region TimeDeviationCause

                    TimeDeviationCause timeDeviationCause = new TimeDeviationCause()
                    {
                        Name = StringUtility.Left(name, 100),
                        Description = StringUtility.Left(name, 512),
                        Type = (int)TermGroup_TimeDeviationCauseType.Presence,

                        //Set FK
                        ActorCompanyId = company.ActorCompanyId,
                    };
                    SetCreatedProperties(timeDeviationCause);
                    entities.TimeDeviationCause.AddObject(timeDeviationCause);

                    //Settings
                    if (IsTimeDeviationCauseStandard(timeDeviationCause))
                        timeDeviationCause.Type = (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence;

                    //Add to collection
                    presenceDeviationCauses.Add(timeDeviationCause);

                    #endregion
                }
            }

            return presenceDeviationCauses;
        }

        private List<TimeDeviationCause> GetWtAbsenceDevationCauses(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, List<TimeCode> timeCodes)
        {
            List<TimeDeviationCause> absenceDeviationCauses = new List<TimeDeviationCause>();

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMEDEVIATIONCAUSES_ABSENCE, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    string name = dr.GetString(0);

                    #region TimeDeviationCause

                    TimeDeviationCause timeDeviationCause = new TimeDeviationCause()
                    {
                        Name = StringUtility.Left(name, 100),
                        Description = StringUtility.Left(name, 512),
                        Type = (int)TermGroup_TimeDeviationCauseType.Absence,

                        //Set FK
                        ActorCompanyId = company.ActorCompanyId,
                        TimeCodeId = timeCodes.FirstOrDefault(i => i.Name.Equals(name))?.TimeCodeId,
                    };
                    SetCreatedProperties(timeDeviationCause);
                    entities.TimeDeviationCause.AddObject(timeDeviationCause);

                    //Settings
                    if (IsTimeDeviationCauseStandard(timeDeviationCause))
                        timeDeviationCause.Type = (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence;

                    //Add to collection
                    absenceDeviationCauses.Add(timeDeviationCause);

                    #endregion
                }
            }

            return absenceDeviationCauses;
        }

        private void AddTimeDeviationCauseToCollection(List<TimeDeviationCause> timeDeviationCauses, TimeDeviationCause timeDeviationCause)
        {
            if (timeDeviationCauses == null || timeDeviationCause == null || timeDeviationCauses.Any(i => i.TimeDeviationCauseId == timeDeviationCause.TimeDeviationCauseId))
                return;

            timeDeviationCauses.Add(timeDeviationCause);
        }

        private int? GetTimeDeviationCauseStandardId(List<TimeDeviationCause> timeDeviationCauses)
        {
            int? timeDeviationCauseStdId = null;

            TimeDeviationCause timeDeviationCauseStd = GetTimeDeviationCauseStandard(timeDeviationCauses);
            if (timeDeviationCauseStd != null)
                timeDeviationCauseStdId = timeDeviationCauseStd.TimeDeviationCauseId;

            return timeDeviationCauseStdId;
        }

        private TimeDeviationCause GetTimeDeviationCauseStandard(List<TimeDeviationCause> timeDeviationCauses)
        {
            return GetTimeDeviationCause(timeDeviationCauses, "standard");
        }

        private TimeDeviationCause GetTimeDeviationCause(List<TimeDeviationCause> timeDeviationCauses, string name)
        {
            if (timeDeviationCauses == null || name == null)
                return null;

            return timeDeviationCauses.FirstOrDefault(i => i.Name.Trim().ToLower() == name.Trim().ToLower());
        }

        private bool IsTimeDeviationCauseStandard(TimeDeviationCause timeDeviationCause)
        {
            return timeDeviationCause != null && timeDeviationCause.Name.Trim().ToLower() == "standard";
        }

        private void AddEmployeeGroupTimeDeviationCauses(CompEntities entities, SqlConnection wtConnection, int wtCompanyid, List<TimeDeviationCause> timeDeviationCauses)
        {
            if (timeDeviationCauses == null)
                return;

            #region Prereq

            int? timeDeviationCauseStdId = GetTimeDeviationCauseStandardId(timeDeviationCauses);

            #endregion

            foreach (var pair in this.exportedEmployeeGroupsDict)
            {
                List<TimeDeviationCause> timeDeviationCausesToAdd = new List<TimeDeviationCause>();

                int wtEmployeeGroupId = pair.Key;
                EmployeeGroup employeeGroup = pair.Value;

                #region Standard TimeDeviationCause

                if (employeeGroup.TimeDeviationCauseId.HasValue)
                {
                    TimeDeviationCause timeDeviationCauseStandard = timeDeviationCauses.FirstOrDefault(i => i.TimeDeviationCauseId == employeeGroup.TimeDeviationCauseId.Value);
                    AddTimeDeviationCauseToCollection(timeDeviationCausesToAdd, timeDeviationCauseStandard);
                }

                #endregion

                #region Absence TimeDeviationCause's

                List<TimeDeviationCause> timeDeviationCausesAbsence = timeDeviationCauses.Where(i => i.Type == (int)TermGroup_TimeDeviationCauseType.Absence).ToList();
                foreach (TimeDeviationCause timeDeviationCause in timeDeviationCausesAbsence)
                {
                    AddTimeDeviationCauseToCollection(timeDeviationCausesToAdd, timeDeviationCause);
                }

                #endregion

                #region EmployeeGroup TimeDeviationCause's

                SqlCommand cmd = GetCommand(wtConnection, GET_EMPLOYEE_GROUP_TIMEDEVIATIONCAUSES, wtEmployeeGroupId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string deviationCauseName = StringUtility.Left(dr.GetString(0), 100);

                        TimeDeviationCause timeDeviationCause = (from tdc in timeDeviationCauses
                                                                 where tdc.Name == deviationCauseName &&
                                                                 (tdc.TimeDeviationCauseId != timeDeviationCauseStdId) &&
                                                                 (tdc.Type == (int)TermGroup_TimeDeviationCauseType.Presence || tdc.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence)
                                                                 select tdc).FirstOrDefault();

                        AddTimeDeviationCauseToCollection(timeDeviationCausesToAdd, timeDeviationCause);
                    }
                }

                #endregion

                #region Add EmployeeGroupTimeDeviationCauses

                foreach (TimeDeviationCause timeDeviationCause in timeDeviationCausesToAdd)
                {
                    EmployeeManager.CreateEmployeeGroupTimeDeviationCause(entities, employeeGroup, timeDeviationCause.TimeDeviationCauseId, wtCompanyid, true);
                }

                #endregion
            }
        }

        #endregion

        #region TimePeriods

        private TimePeriodHead GetWtTimePeriodHead(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company)
        {
            TimePeriodHead timePeriodHead = null;

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMEPERIODS, wtCompanyId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    DateTime startdate = dr.GetDateTime(1);
                    DateTime stopDate = dr.GetDateTime(2);
                    string name = dr.GetString(3);

                    if (timePeriodHead == null)
                    {
                        #region TimePeriodHead

                        timePeriodHead = new TimePeriodHead()
                        {
                            Name = StringUtility.Left("Löneperioder", 50),
                            Description = "",
                            TimePeriodType = (int)TermGroup_TimePeriodType.Payroll,

                            //Set references
                            Company = company,
                        };
                        SetCreatedProperties(timePeriodHead);
                        entities.TimePeriodHead.AddObject(timePeriodHead);

                        #endregion
                    }

                    if (startdate > CalendarUtility.DATETIME_DEFAULT && stopDate > CalendarUtility.DATETIME_DEFAULT)
                    {
                        #region TimePeriod

                        TimePeriod timePeriod = new TimePeriod()
                        {
                            Name = StringUtility.Left(name, 100),
                            StartDate = startdate,
                            StopDate = stopDate,
                        };
                        SetCreatedProperties(timePeriod);

                        //Add to TimePeriodHead
                        timePeriodHead.TimePeriod.Add(timePeriod);

                        #endregion
                    }
                }

                //RowNr
                int rowNr = 1;
                if (timePeriodHead != null)
                {
                    foreach (TimePeriod timeperiod in timePeriodHead.TimePeriod.Where(x => x.State == (int)SoeEntityState.Active).OrderBy(t => t.StartDate))
                    {
                        timeperiod.RowNr = rowNr;
                        rowNr++;
                    }
                }
            }

            return timePeriodHead;
        }

        #endregion

        #region TimeCodeTransactions

        private List<TimeCodeTransaction> GetWtTimeCodeTransactions(CompEntities entities, SqlConnection wtConnection, int wtCompanyId, Company company, List<TimeBlock> timeBlocks, DateTime fromDate, DateTime toDate)
        {
            List<TimeCodeTransaction> timeCodeTransactions = new List<TimeCodeTransaction>();
            #region Prereq

            //AccountDim KSK
            AccountDim accountDimCostCentre = company.AccountDim.FirstOrDefault(i => i.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);
            AccountStd accountStdPayroll = null;

            #endregion

            SqlCommand cmd = GetCommand(wtConnection, GET_TIMEPAYROLLTRANSACTIONS, wtCompanyId, fromDate, toDate);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int wtPayrollProductId = dr.GetInt32(1);
                    int wtEmployeeId = dr.GetInt32(2);
                    DateTime date = dr.GetDateTime(3);
                    string accountInternalNr = !dr.IsDBNull(4) ? dr.GetString(4) : "";
                    int quantity = dr.GetInt32(5);
                    int wtAttestStateId = dr.GetInt32(6);
                    decimal amount = dr.GetDecimal(8);
                    int wtTimeCodeId = dr.GetInt32(1);
                    DateTime dateFrom = dr.GetDateTime(10);
                    DateTime dateTo = dr.GetDateTime(11);

                    Employee employee = GetExportedEmployee(wtEmployeeId);

                    if (employee == null)
                        return timeCodeTransactions;

                    PayrollProduct payrollProduct = GetExportedPayrollProduct(wtPayrollProductId);
                    AttestState attestState = GetExportedAttestState(wtAttestStateId);
                    TimeBlock timeBlock = timeBlocks.FirstOrDefault(t => t.EmployeeId == employee.EmployeeId && t.TimeBlockDate.Date == date && !t.IsBreak);
                    TimeCode timeCode = GetExportedTimeCode(wtTimeCodeId);

                    //Only if the PayrollProduct exists, otherwise it is old and dont need to be converted
                    if (payrollProduct != null)
                    {
                        #region Prereq

                        //Set category nr if empty
                        if (String.IsNullOrEmpty(accountInternalNr.Trim()))
                            accountInternalNr = !dr.IsDBNull(7) ? dr.GetString(7) : "";

                        //Remove ; (ex: ;11 --> 11)
                        if (!String.IsNullOrEmpty(accountInternalNr) && accountInternalNr.StartsWith(";"))
                            accountInternalNr = accountInternalNr.Substring(1, accountInternalNr.Length - 1);


                        #endregion

                        #region TimeCodeTransaction

                        TimeCodeTransaction timeCodeTransaction = new TimeCodeTransaction()
                        {
                            TimeCodeId = timeCode.TimeCodeId,
                            Amount = amount,
                            Vat = 0,
                            Quantity = Convert.ToDecimal(quantity),
                            Type = 1,
                            TimeBlock = timeBlock,
                            Comment = "Converted from WT",
                            Start = dateFrom,
                            Stop = dateTo,

                        };
                        SetCreatedProperties(timeCodeTransaction);

                        if (amount != 0)
                            timeCodeTransaction.TimeBlockId = null;

                        //Set currency amounts
                        CountryCurrencyManager.SetCurrencyAmounts(entities, company.ActorCompanyId, timeCodeTransaction);

                        #endregion

                        #region TimePayrollTransaction

                        TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction()
                        {
                            Amount = amount,
                            VatAmount = 0,
                            Quantity = Convert.ToDecimal(quantity),
                            IsPreliminary = false,
                            ManuallyAdded = false,
                            Exported = false,
                            AutoAttestFailed = false,
                            Comment = null,
                            SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                            SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                            SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                            SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,
                            //TimeCodeTransactionId = timeCodeTransaction.TimeCodeTransactionId,

                            //Set FK
                            ActorCompanyId = company.ActorCompanyId,
                            EmployeeId = employee.EmployeeId,
                            AttestStateId = attestState.AttestStateId,

                            //Refereces
                            PayrollProduct = payrollProduct,
                            TimeBlock = timeBlock,
                            TimeCodeTransaction = timeCodeTransaction,
                        };

                        CountryCurrencyManager.SetCurrencyAmounts(entities, company.ActorCompanyId, timePayrollTransaction);

                        //TimeBlockDate
                        TimeBlockDate timeBlockDate = timeBlock?.TimeBlockDate;
                        if (timeBlockDate == null)
                        {
                            timeBlockDate = GetTimeBlockDate(entities, date, wtEmployeeId, wtCompanyId);
                            // SET FK
                            timePayrollTransaction.TimeBlockDateId = timeBlockDate.TimeBlockDateId;
                        }
                        timePayrollTransaction.TimeBlockDate = timeBlockDate;

                        // Adjust when it's not a timetransactions
                        if (amount != 0)
                        {
                            timePayrollTransaction.TimeBlockId = null;
                            // Set timeblockdate as startdate
                            timeBlockDate = GetTimeBlockDate(entities, fromDate, wtEmployeeId, wtCompanyId);
                            timePayrollTransaction.TimeBlockDate = timeBlockDate;
                        }

                        SetCreatedProperties(timePayrollTransaction);

                        #region Accounts

                        //AccountStd. First try get it from the PayrollProduct
                        timePayrollTransaction.AccountStd = timePayrollTransaction.PayrollProduct.ProductAccountStd.Select(i => i.AccountStd).FirstOrDefault();
                        if (timePayrollTransaction.AccountStd == null)
                        {
                            if (accountStdPayroll == null)
                                accountStdPayroll = GetAccountStdPayroll(entities, company.ActorCompanyId);

                            timePayrollTransaction.AccountStd = accountStdPayroll;
                        }

                        //AccountInternal
                        if (!String.IsNullOrEmpty(accountInternalNr) && !accountInternalNr.Contains(';'))
                        {
                            //AccountInternals for PayrollTransactions that not corresponds to a Category (i.e. was converted in GetWtCategories) will be skipped
                            AccountInternal accountInternal = accountDimCostCentre.Account.Where(i => i.AccountNr == accountInternalNr).Select(i => i.AccountInternal).FirstOrDefault();
                            if (accountInternal != null)
                                timePayrollTransaction.AccountInternal.Add(accountInternal);
                        }

                        #endregion

                        //Add to collection
                        timeCodeTransaction.TimePayrollTransaction.Add(timePayrollTransaction);
                        timeCodeTransactions.Add(timeCodeTransaction);

                        #endregion
                    }
                }
            }

            return timeCodeTransactions;
        }

        #endregion

        #endregion

        #region Entity help-methods

        #region Account

        private AccountStd GetAccountStdPayroll(CompEntities entities, int actorCompanyId)
        {
            return GetAccountStd(entities, "7010", "Löner", actorCompanyId);
        }

        private AccountStd GetAccountStd(CompEntities entities, string accountNr, string name, int actorCompanyId)
        {
            Account account = this.accountDimStd.Account.FirstOrDefault(i => i.AccountNr == accountNr);
            if (account == null)
            {
                #region Account

                account = new Account()
                {
                    AccountNr = accountNr,
                    Name = name,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                };
                SetCreatedProperties(account);

                //Add AccountStd
                account.AccountStd = new AccountStd()
                {
                    AccountTypeSysTermId = (int)TermGroup_AccountType.Cost,
                    AmountStop = (int)TermGroup_AmountStop.Debit,
                    Unit = null,
                    UnitStop = false,
                };

                //Add to AccountDim
                this.accountDimStd.Account.Add(account);

                #endregion

                #region AccountHistory

                //Add AccountHistory
                AccountHistory accountHistory = new AccountHistory()
                {
                    Name = account.Name,
                    AccountNr = account.AccountNr,
                    Date = DateTime.Now,
                    SysAccountStdTypeId = null,
                    SieKpTyp = null,

                    //Set references
                    User = this.sysUser, //parameterObject.SoeUser,
                    Account = account,
                };
                SetCreatedProperties(accountHistory);
                entities.AccountHistory.AddObject(accountHistory);

                #endregion
            }

            return account.AccountStd;
        }

        private AccountInternal GetAccountInternal(CompEntities entities, string accountNr, string name, AccountDim accountDim)
        {
            Account account = accountDim.Account.FirstOrDefault(i => i.AccountNr == accountNr);
            if (account == null)
            {
                #region Account

                account = new Account()
                {
                    AccountNr = accountNr,
                    Name = name,

                    //Set FK
                    ActorCompanyId = accountDim.ActorCompanyId,
                };
                SetCreatedProperties(account);

                //Add AccountInternal
                account.AccountInternal = new AccountInternal();

                //Add to AccountDim
                accountDim.Account.Add(account);

                #endregion

                #region AccountHistory

                AccountHistory accountHistory = new AccountHistory()
                {
                    Name = account.Name,
                    AccountNr = account.AccountNr,
                    Date = DateTime.Now,
                    SysAccountStdTypeId = null,
                    SieKpTyp = null,

                    //Set references
                    User = this.sysUser,
                    Account = account,
                };
                SetCreatedProperties(accountHistory);
                entities.AccountHistory.AddObject(accountHistory);

                #endregion
            }

            return account.AccountInternal;
        }

        #endregion

        #region AttestState

        private AttestState GetExportedAttestState(int wtAttestStateId)
        {
            if (!this.exportedAttestStatesDict.ContainsKey(wtAttestStateId))
                return null;
            return this.exportedAttestStatesDict[wtAttestStateId];
        }

        #endregion

        #region AttestTransition

        private void ConnectAttestTransitionToRoles(AttestTransition attestTransition, List<AttestRole> attestRoles)
        {
            foreach (AttestRole attestRole in attestRoles)
            {
                ConnectAttestTransitionToRole(attestTransition, attestRole);
            }
        }

        private void ConnectAttestTransitionToRole(AttestTransition attestTransition, AttestRole attestRole)
        {
            attestRole.AttestTransition.Add(attestTransition);
        }

        private void ConnectAttestTransitionToEmployeeGroups(AttestTransition attestTransition, List<EmployeeGroup> employeeGroups)
        {
            foreach (EmployeeGroup employeeGroup in employeeGroups)
            {
                ConnectAttestTransitionToEmployeeGroups(attestTransition, employeeGroup);
            }
        }

        private void ConnectAttestTransitionToEmployeeGroups(AttestTransition attestTransition, EmployeeGroup employeeGroup)
        {
            employeeGroup.AttestTransition.Add(attestTransition);
        }

        #endregion

        #region Employee

        private Employee GetExportedEmployee(int wtEmployeeId)
        {
            if (!this.exportedEmployeesDict.ContainsKey(wtEmployeeId))
                return null;
            return this.exportedEmployeesDict[wtEmployeeId];
        }

        private int GetExportedEmployeeId(int wtEmployeeId)
        {
            return GetExportedEmployee(wtEmployeeId)?.EmployeeId ?? 0;
        }

        private User GetExportedEmployeeUser(int wtEmployeeId)
        {
            return GetExportedEmployee(wtEmployeeId)?.User;
        }

        #endregion

        #region EmployeeGroup

        private EmployeeGroup GetExportedEmployeeGroup(int wtEmployeeGroupId)
        {
            if (!this.exportedEmployeeGroupsDict.ContainsKey(wtEmployeeGroupId))
                return null;
            return this.exportedEmployeeGroupsDict[wtEmployeeGroupId];
        }

        #endregion

        #region EmployeeSchedule

        private EmployeeSchedule GetEmployeeSchedule(Employee employee, List<EmployeeSchedule> employeeSchedules, DateTime date)
        {
            if (employee == null || employeeSchedules == null)
                return null;

            return employeeSchedules.FirstOrDefault(i => i.EmployeeId == employee.EmployeeId && i.StartDate <= date && i.StopDate >= date);
        }

        #endregion

        #region DayType

        private DayType GetExportedDayType(int wtDayTypeId)
        {
            if (!this.exportedDayTypesDict.ContainsKey(wtDayTypeId))
                return null;
            return this.exportedDayTypesDict[wtDayTypeId];
        }

        #endregion

        #region TimeAccumulator

        private TimeAccumulator GetExportedTimeAccumulator(string name)
        {
            if (!this.exportedTimeAccumulatorsDict.ContainsKey(name))
                return null;
            return this.exportedTimeAccumulatorsDict[name];
        }

        #endregion

        #region TimeBlockDate

        private TimeBlockDate GetTimeBlockDate(CompEntities entities, DateTime date, int wtEmployeeId, int wtCompanyId)
        {
            TimeBlockDate timeBlockDate;

            if (this.exportedTimeBlockDatesDict.ContainsKey(wtEmployeeId))
            {
                if (this.exportedTimeBlockDatesDict[wtEmployeeId].ContainsKey(date))
                {
                    timeBlockDate = this.exportedTimeBlockDatesDict[wtEmployeeId][date];
                }
                else
                {
                    timeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, wtCompanyId, GetExportedEmployeeId(wtEmployeeId), date, createfNotExist: true);
                    this.exportedTimeBlockDatesDict[wtEmployeeId].Add(date, timeBlockDate);
                }
            }
            else
            {
                timeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, wtCompanyId, GetExportedEmployeeId(wtEmployeeId), date, createfNotExist: true);
                Dictionary<DateTime, TimeBlockDate> timeBlockDateDict = new Dictionary<DateTime, TimeBlockDate>();
                timeBlockDateDict.Add(date, timeBlockDate);
                this.exportedTimeBlockDatesDict.Add(wtEmployeeId, timeBlockDateDict);
            }

            return timeBlockDate;
        }

        #endregion

        #region TimeCode

        private TimeCode GetExportedTimeCode(int wtTimeCodeId)
        {
            if (!this.exportedTimeCodesDict.ContainsKey(wtTimeCodeId))
                return null;
            return this.exportedTimeCodesDict[wtTimeCodeId];
        }

        #endregion

        #region TimeCodeWork

        private TimeCodeWork GetExportedTimeCodeTimeAccumulator(string name)
        {
            if (!this.exportedTimeCodesTimeAccumulatorDict.ContainsKey(name))
                return null;
            return this.exportedTimeCodesTimeAccumulatorDict[name];
        }

        #endregion

        #region TimeScheduleTemplateHead

        private TimeScheduleTemplateHead GetExportedTimeScheduleTemplateHead(int wtTemplateHeadId)
        {
            if (!this.exportedTimeScheduleTemplateHeadsDict.ContainsKey(wtTemplateHeadId))
                return null;
            return this.exportedTimeScheduleTemplateHeadsDict[wtTemplateHeadId];
        }

        #endregion

        #region TimeScheduleTemplatePeriod

        private TimeScheduleTemplatePeriod GetExportedTimeScheduleTemplatePeriod(int wtTemplatePeriodId)
        {
            if (!this.exportedTimeScheduleTemplatePeriodsDict.ContainsKey(wtTemplatePeriodId))
                return null;
            return this.exportedTimeScheduleTemplatePeriodsDict[wtTemplatePeriodId];
        }

        private TimeScheduleTemplatePeriod GetTemplatePeriod(Employee employee, List<EmployeeSchedule> employeeSchedules, DateTime date)
        {
            if (employee == null || employeeSchedules == null)
                return null;

            TimeScheduleTemplatePeriod templatePeriod = null;

            EmployeeSchedule employeeSchedule = GetEmployeeSchedule(employee, employeeSchedules, date);
            if (employeeSchedule != null)
            {
                int dayNumber = CalendarUtility.GetScheduleDayNumber(date, employeeSchedule.StartDate, employeeSchedule.StartDayNumber, employeeSchedule.TimeScheduleTemplateHead.NoOfDays);
                templatePeriod = GetTemplatePeriod(employee, employeeSchedule, dayNumber);
            }

            return templatePeriod;
        }

        private TimeScheduleTemplatePeriod GetTemplatePeriod(Employee employee, EmployeeSchedule employeeSchedule, int dayNumber)
        {
            if (employee == null || employeeSchedule == null)
                return null;

            return this.exportedTimeScheduleTemplatePeriodsDict.Values.FirstOrDefault(i => i.TimeScheduleTemplateHeadId == employeeSchedule.TimeScheduleTemplateHeadId && i.DayNumber == dayNumber);
        }

        #endregion

        #region TimeTerminal

        private TimeTerminal GetExportedTimeTerminal(int wtTimeTerminalId)
        {
            if (this.exportedTimeTerminalsDict != null && this.exportedTimeTerminalsDict.ContainsKey(wtTimeTerminalId))
                return this.exportedTimeTerminalsDict[wtTimeTerminalId];
            return null;
        }

        private TimeTerminal GetFirstExportedTimeTerminal()
        {
            //Get first if only one, if more than one return null
            if (this.exportedTimeTerminalsDict != null && this.exportedTimeTerminalsDict.Count == 1)
                return exportedTimeTerminalsDict.First().Value;
            return null;
        }

        #endregion

        #region PayrollProduct

        private PayrollProduct GetExportedPayrollProduct(int wtPayrollProductId)
        {
            if (!this.exportedPayrollProductsDict.ContainsKey(wtPayrollProductId))
                return null;
            return this.exportedPayrollProductsDict[wtPayrollProductId];
        }

        #endregion

        #region ProductUnit

        private ProductUnit GetProductUnit(string unitName, List<ProductUnit> units)
        {
            if (String.IsNullOrEmpty(unitName) || unitName.Equals("0") || unitName.Equals("1") || unitName.Equals("-1"))
                unitName = "Ingen enhet";

            return units.FirstOrDefault(i => i.Name.Equals(unitName));
        }

        #endregion

        #endregion

        #region UserCompanySettings

        private void SetAccountSettings(CompEntities entities, Company company)
        {
            AccountStd accountStd = AccountManager.GetAccountStdByNr(entities, "7010", company.ActorCompanyId);
            if (accountStd == null)
                return;

            UserCompanySetting settingAccountEmployeeCost = new UserCompanySetting()
            {
                SettingTypeId = (int)CompanySettingType.AccountEmployeeCost,
                DataTypeId = (int)SettingDataType.Integer,
                IntData = accountStd.AccountId,

                //Set references
                Company = company,
            };
            entities.UserCompanySetting.AddObject(settingAccountEmployeeCost);

            UserCompanySetting settingAccountEmployeeGroupCost = new UserCompanySetting()
            {
                SettingTypeId = (int)CompanySettingType.AccountEmployeeGroupCost,
                DataTypeId = (int)SettingDataType.Integer,
                IntData = accountStd.AccountId,

                //Set references
                Company = company,
            };
            entities.UserCompanySetting.AddObject(settingAccountEmployeeGroupCost);

            UserCompanySetting settingAccountEmployeeIncome = new UserCompanySetting()
            {
                SettingTypeId = (int)CompanySettingType.AccountEmployeeIncome,
                DataTypeId = (int)SettingDataType.Integer,
                IntData = accountStd.AccountId,

                //Set references
                Company = company,
            };
            entities.UserCompanySetting.AddObject(settingAccountEmployeeIncome);

            UserCompanySetting settingAccountEmployeeGroupIncome = new UserCompanySetting()
            {
                SettingTypeId = (int)CompanySettingType.AccountEmployeeGroupIncome,
                DataTypeId = (int)SettingDataType.Integer,
                IntData = accountStd.AccountId,

                //Set references
                Company = company,
            };
            entities.UserCompanySetting.AddObject(settingAccountEmployeeGroupIncome);
        }

        private void SetAttestSettings(CompEntities entities, Company company, List<AttestState> atteststates, List<AttestRole> attestRoles, List<Category> categories)
        {
            AttestState attestStateHome = atteststates.FirstOrDefault(t => t.Name.ToLower() == "lön");
            if (attestStateHome != null)
            {
                UserCompanySetting settingSalaryExportPayrollResultingAttestStatus = new UserCompanySetting()
                {
                    SettingTypeId = (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus,
                    DataTypeId = (int)SettingDataType.Integer,
                    IntData = attestStateHome.AttestStateId,

                    //Set references
                    Company = company,
                };
                entities.UserCompanySetting.AddObject(settingSalaryExportPayrollResultingAttestStatus);
            }

            AttestState attestStateAttested = atteststates.FirstOrDefault(t => t.Name.ToLower() == "attesterad");
            if (attestStateAttested != null)
            {
                UserCompanySetting settingSalaryExportPayrollMinimumAttestStatus = new UserCompanySetting()
                {
                    SettingTypeId = (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus,
                    DataTypeId = (int)SettingDataType.Integer,
                    IntData = attestStateAttested.AttestStateId,

                    //Set references
                    Company = company,
                };
                entities.UserCompanySetting.AddObject(settingSalaryExportPayrollMinimumAttestStatus);
            }

            foreach (AttestRole attestRole in attestRoles)
            {
                #region AttestRole

                Category category = categories.FirstOrDefault(c => c.Name == attestRole.Name);
                if (category != null)
                {
                    CompanyCategoryRecord companyCategoryRecord = new CompanyCategoryRecord
                    {
                        Entity = (int)SoeCategoryRecordEntity.AttestRole,
                        RecordId = attestRole.AttestRoleId,
                        DateFrom = null,
                        DateTo = null,

                        //Set referenecs
                        Company = company,
                        Category = category,
                    };
                    entities.CompanyCategoryRecord.AddObject(companyCategoryRecord);
                }

                #endregion
            }
        }

        private void SetEmployeeGroupSettings(CompEntities entities, Company company)
        {
            if (this.defaultEmployeeGroup != null && this.defaultEmployeeGroup.EmployeeGroupId != 0)
            {
                UserCompanySetting settingDefaultEmployeeGroup = new UserCompanySetting()
                {
                    SettingTypeId = (int)CompanySettingType.TimeDefaultEmployeeGroup,
                    DataTypeId = (int)SettingDataType.Integer,
                    IntData = this.defaultEmployeeGroup.EmployeeGroupId,

                    //Set references
                    Company = company,
                };
                entities.UserCompanySetting.AddObject(settingDefaultEmployeeGroup);
            }
        }

        private void SetTimeCodeSettings(CompEntities entities, Company company, List<TimeCode> timeCodes)
        {
            TimeCode timeCode = timeCodes.FirstOrDefault(t => t.Name.ToLower() == "arbetad tid");
            if (timeCode != null)
            {
                UserCompanySetting settingTimeDefaultTimeCode = new UserCompanySetting()
                {
                    SettingTypeId = (int)CompanySettingType.TimeDefaultTimeCode,
                    DataTypeId = (int)SettingDataType.Integer,
                    IntData = timeCode.TimeCodeId,

                    //Set references
                    Company = company,
                };
                entities.UserCompanySetting.AddObject(settingTimeDefaultTimeCode);
            }
        }

        #endregion

        #region WT meta data

        public WebbTidCompanyConvert GetWtCompanyToConvert(string wtConnectionString)
        {
            WebbTidCompanyConvert company = new WebbTidCompanyConvert();

            using (SqlConnection wtConnection = new SqlConnection(wtConnectionString))
            {
                //Open sql connection
                wtConnection.Open();

                SqlCommand cmd = GetCommand(wtConnection, GET_COMPANY_ID);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        company.WebbtidCompanyId = dr.GetInt32(0);
                        company.ActorCompanyId = dr.GetInt32(1);
                        if (company.ActorCompanyId == 0)
                            company.ActorCompanyId = null;
                        company.StartDateTime = dr.GetDateTime(2);
                        company.EndDateTime = dr.GetDateTime(3);
                    }
                }
            }

            return company;
        }

        public ActionResult SetConvertionFlags(string wtConnectionString, int wtCompanyId, DateTime conversionDate, Company company)
        {
            ActionResult result = new ActionResult(false);

            using (SqlConnection wtConnection = new SqlConnection(wtConnectionString))
            {
                SqlCommand cmd = GetCommand(wtConnection, UPDATE_CONVERT_STATUS_TO_SUCCESS);

                SqlParameter paramId = new SqlParameter("id", System.Data.SqlDbType.Int);
                paramId.Value = wtCompanyId;
                cmd.Parameters.Add(paramId);

                SqlParameter paramActorCompanyId = new SqlParameter("ActorCompanyId", System.Data.SqlDbType.Int);
                paramActorCompanyId.Value = company.ActorCompanyId;
                cmd.Parameters.Add(paramActorCompanyId);

                SqlParameter paramDate = new SqlParameter("date", System.Data.SqlDbType.DateTime);
                paramDate.Value = conversionDate;
                cmd.Parameters.Add(paramDate);

                //Open sql connection
                wtConnection.Open();

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    result = new ActionResult(true);
            }

            return result;
        }

        #endregion

        #region Help-methods

        #region Data

        private SqlCommand GetCommand(SqlConnection connection, string commandText)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = commandText;
            return cmd;
        }

        /// <summary>
        /// Adds the companyId as a parameter
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="wtCompanyId"></param>
        /// <returns></returns>
        private SqlCommand GetCommand(SqlConnection conn, string storedProcedureName, int wtCompanyId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            SqlCommand cmd = GetCommand(conn, storedProcedureName);
            SqlParameter parameterCompanyId = new SqlParameter("id", System.Data.SqlDbType.Int);
            parameterCompanyId.Value = wtCompanyId;
            cmd.Parameters.Add(parameterCompanyId);

            if (fromDate != null)
            {
                SqlParameter parameterFromDate = new SqlParameter("fromDate", System.Data.SqlDbType.DateTime);
                parameterFromDate.Value = fromDate;
                cmd.Parameters.Add(parameterFromDate);
            }

            if (toDate != null)
            {
                SqlParameter parameterToDate = new SqlParameter("toDate", System.Data.SqlDbType.DateTime);
                parameterToDate.Value = toDate;
                cmd.Parameters.Add(parameterToDate);
            }

            return cmd;
        }

        private ActionResult Save(CompEntities entities)
        {
            ActionResult result = SaveChanges(entities);

            CreateLogEntry(String.Format("Konvertering: Sparning - {0}", (result.Success ? "lyckades" : "misslyckades")));

            return result;
        }

        private void AddWtConvertMapping(CompEntities entities, Company company)
        {
            #region 1. Employee Category

            foreach (var pair in this.exportedEmployeeCategoriesDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.EmployeeCategory,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.CategoryId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion

            #region 2. EmployeeGroup

            foreach (var pair in this.exportedEmployeeGroupsDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.EmployeeGroup,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.EmployeeGroupId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion

            #region 3. Employee

            foreach (var pair in exportedEmployeesDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.Employee,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.EmployeeId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion

            #region 4. TimeTerminal

            foreach (var pair in this.exportedTimeTerminalsDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.TimeTerminal,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.TimeTerminalId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion

            #region 5. TimeScheduleTemplateHead

            foreach (var pair in this.exportedTimeScheduleTemplateHeadsDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.TimeScheduleTemplateHead,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.TimeScheduleTemplateHeadId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion

            #region 6. TimeScheduleTemplatePeriod

            foreach (var pair in this.exportedTimeScheduleTemplatePeriodsDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.TimeScheduleTemplatePeriod,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.TimeScheduleTemplatePeriodId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion

            #region 7. TimeCode

            foreach (var pair in this.exportedTimeCodesDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.TimeCode,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.TimeCodeId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion

            #region 8. DayType

            foreach (var pair in this.exportedDayTypesDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.DayType,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.DayTypeId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion

            #region 9. AttestState

            foreach (var pair in this.exportedAttestStatesDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.AttestState,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.AttestStateId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion

            #region 10. PayrollProduct

            foreach (var pair in this.exportedPayrollProductsDict)
            {
                WtConvertMapping mapping = new WtConvertMapping()
                {
                    Type = (int)XeType.PayrollProduct,

                    //Set FK
                    ActorCompanyId = company.ActorCompanyId,
                    WTId = pair.Key,
                    XEId = pair.Value.ProductId,
                };
                entities.WtConvertMapping.AddObject(mapping);
            }

            #endregion
        }

        private void PopulateWtConvertMapping(CompEntities entities, Company company)
        {
            List<WtConvertMapping> mappings = (from m in entities.WtConvertMapping
                                               where m.ActorCompanyId == company.ActorCompanyId
                                               select m).ToList();

            #region Company

            //Make sure AccountDim is loaded
            if (!company.AccountDim.IsLoaded)
                company.AccountDim.Load();

            foreach (AccountDim accountDim in company.AccountDim)
            {
                //Make sure Account is loaded
                if (!accountDim.Account.IsLoaded)
                    accountDim.Account.Load();

                foreach (Account account in accountDim.Account)
                {
                    //Make sure AccountInternal is loaded
                    if (!account.AccountInternalReference.IsLoaded)
                        account.AccountInternalReference.Load();
                }
            }

            #endregion

            #region AccountDim

            this.accountDimStd = AccountManager.GetAccountDimStd(entities, company.ActorCompanyId, true);

            #endregion

            #region EmployeeCategory (Key only)

            foreach (WtConvertMapping mapping in mappings.Where(w => w.Type == (int)XeType.EmployeeCategory))
            {
                Category entity = new Category()
                {
                    CategoryId = mapping.XEId,
                };
                this.exportedEmployeeCategoriesDict.Add(mapping.WTId, entity);
            }

            #endregion

            #region EmployeeGroup (Key only)

            foreach (WtConvertMapping mapping in mappings.Where(w => w.Type == (int)XeType.EmployeeGroup))
            {
                EmployeeGroup entity = new EmployeeGroup()
                {
                    EmployeeGroupId = mapping.XEId,
                };
                this.exportedEmployeeGroupsDict.Add(mapping.WTId, entity);
            }

            #endregion

            #region Employee (Key only)

            foreach (WtConvertMapping mapping in mappings.Where(w => w.Type == (int)XeType.Employee))
            {
                Employee entity = new Employee()
                {
                    EmployeeId = mapping.XEId,
                };
                this.exportedEmployeesDict.Add(mapping.WTId, entity);
            }

            #endregion

            #region TimeTerminal (Key only)

            foreach (WtConvertMapping mapping in mappings.Where(w => w.Type == (int)XeType.TimeTerminal))
            {
                TimeTerminal entity = new TimeTerminal()
                {
                    TimeTerminalId = mapping.XEId,
                    TimeTerminalGuid = Guid.NewGuid()
                };
                this.exportedTimeTerminalsDict.Add(mapping.WTId, entity);
            }

            #endregion

            #region TimeScheduleTemplateHead and TimeScheduleTemplateHead.EmployeeSchedule (Key only)

            foreach (WtConvertMapping mapping in mappings.Where(w => w.Type == (int)XeType.TimeScheduleTemplateHead))
            {
                TimeScheduleTemplateHead entity = new TimeScheduleTemplateHead()
                {
                    TimeScheduleTemplateHeadId = mapping.XEId,
                };
                this.exportedTimeScheduleTemplateHeadsDict.Add(mapping.WTId, entity);
            }

            #endregion

            #region TimeScheduleTemplatePeriod

            List<TimeScheduleTemplatePeriod> templatePeriods = TimeScheduleManager.GetTimeScheduleTemplatePeriodsForCompany(entities, company.ActorCompanyId, true);

            foreach (WtConvertMapping mapping in mappings.Where(w => w.Type == (int)XeType.TimeScheduleTemplatePeriod))
            {
                TimeScheduleTemplatePeriod templatePeriod = templatePeriods.FirstOrDefault(i => i.TimeScheduleTemplatePeriodId == mapping.XEId);
                if (templatePeriod != null)
                    this.exportedTimeScheduleTemplatePeriodsDict.Add(mapping.WTId, templatePeriod);
            }

            #endregion

            #region TimeCode

            List<TimeCode> timeCodes = TimeCodeManager.GetTimeCodes(entities, company.ActorCompanyId, SoeTimeCodeType.None);

            foreach (WtConvertMapping mapping in mappings.Where(w => w.Type == (int)XeType.TimeCode))
            {
                TimeCode timeCode = timeCodes.FirstOrDefault(i => i.TimeCodeId == mapping.XEId);
                if (timeCode != null)
                    this.exportedTimeCodesDict.Add(mapping.WTId, timeCode);
            }

            foreach (TimeCode timeCode in timeCodes)
            {
                if (timeCode is TimeCodeWork timeCodeWork)
                    this.exportedTimeCodesTimeAccumulatorDict.Add(timeCode.Name, timeCodeWork);
            }

            #endregion

            #region DayType

            #endregion

            #region AttestState (Key only)

            foreach (WtConvertMapping mapping in mappings.Where(w => w.Type == (int)XeType.AttestState))
            {
                AttestState entity = new AttestState()
                {
                    AttestStateId = mapping.XEId,
                };
                this.exportedAttestStatesDict.Add(mapping.WTId, entity);
            }

            #endregion

            #region PayrollProduct

            List<PayrollProduct> payrollProducts = ProductManager.GetPayrollProducts(entities, company.ActorCompanyId, active: true, loadAccounts: true);

            foreach (WtConvertMapping mapping in mappings.Where(w => w.Type == (int)XeType.PayrollProduct))
            {
                PayrollProduct payrollProduct = payrollProducts.FirstOrDefault(i => i.ProductId == mapping.XEId);
                if (payrollProduct != null)
                    this.exportedPayrollProductsDict.Add(mapping.WTId, payrollProduct);
            }

            #endregion
        }

        private void CreateLogEntry(string message)
        {
            if (this.batchNr.HasValue && this.sysScheduledJobId.HasValue)
                SysScheduledJobManager.CreateLogEntry((int)this.sysScheduledJobId, (int)this.batchNr, ScheduledJobLogLevel.Information, message);
        }

        #endregion

        #region Misc

        private int GetRoleTermIdFromWTRoleId(int wtRoleId)
        {
            int roleId = 0;

            switch (wtRoleId)
            {
                case (int)WtRoles.Employee:
                    roleId = (int)TermGroup_Roles.Employee;
                    break;
                case (int)WtRoles.Approval:
                    roleId = (int)TermGroup_Roles.Approval;
                    break;
                case (int)WtRoles.Attest:
                    roleId = (int)TermGroup_Roles.Attest;
                    break;
                case (int)WtRoles.Admin:
                case (int)WtRoles.SuperAdmin:
                    roleId = (int)TermGroup_Roles.Systemadmin;
                    break;
            }

            return roleId;
        }

        #endregion

        #endregion

        #region Help-classes

        public class WebbTidCompanyConvert
        {
            public int WebbtidCompanyId { get; set; }
            public int? ActorCompanyId { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime EndDateTime { get; set; }
        }

        #region State objects

        private class EmployeeCategoryStateObject
        {
            public int WtEmployeeCategoryId { get; set; }
            public Employee Employee { get; set; }
            public Category Category { get; set; }
        }

        private class AttestRoleCategoryStateObject
        {
            public int WtEmployeeCategoryId { get; set; }
            public AttestRole AttestRole { get; set; }
            public Category Category { get; set; }
        }

        #endregion

        #endregion
    }
}

using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class ApiConfig
    {
        public int CacheSeconds { get; private set; }
        public List<ApiSettingDTO> Settings { get; private set; }

        public ApiConfig(int cacheSeconds, List<ApiSettingDTO> settings)
        {
            this.CacheSeconds = cacheSeconds;
            this.Settings = settings;
        }
    }

    public abstract class ApiLookup
    {
        public ApiConfig Config { get; private set; }
        public int ActorCompanyId { get; private set; }

        public List<ApiSettingDTO> Settings
        {
            get
            {
                return this.Config?.Settings;
            }
        }

        protected ApiLookup(ApiConfig config, int actorCompanyId)
        {
            this.Config = config;
            this.ActorCompanyId = actorCompanyId;
        }
    }

    public class ApiLookupEmployee : ApiLookup
    {
        public int DefaultRoleId { get; private set; }
        public int DefaultEmployeeGroupId { get; private set; }
        public int DefaultPayrollGroupId { get; private set; }
        public int DefaultVacationGroupId { get; private set; }
        public TermGroup_Country CmpanyCountryId { get; private set; }
        public Dictionary<TermGroup, List<GenericType>> Terms { get; private set; }

        public Dictionary<string, List<ContactAddressItem>> ContactAddressItemsByEmployee { get; private set; }
        public Dictionary<string, List<EmployeePositionDTO>> EmployeePositionsByEmployee { get; private set; }
        public Dictionary<string, List<ExtraFieldRecordDTO>> ExtraFieldRecordsByEmployee { get; private set; }
        public List<AccountDimDTO> AccountDims { get; private set; }
        public List<AccountDTO> AccountInternals { get; private set; }
        public List<AttestRoleDTO> AttestRoles { get; private set; }
        public List<GenericType> ContactAddresses { get; private set; }
        public List<GenericType> ContactEcoms { get; private set; }
        public List<EmployeeGroupDTO> EmployeeGroups { get; private set; }
        public List<EndReasonDTO> EmploymentEndReasons { get; set; }
        public List<PositionDTO> EmployeePositions { get; set; }
        public List<EmploymentTypeDTO> EmploymentTypes { get; private set; }
        public List<ExtraFieldDTO> ExtraFields { get; private set; }
        public List<ExtraFieldRecordDTO> ExtraFieldRecords { get; private set; }        
        public List<PayrollGroupDTO> Payrollgroups { get; private set; }
        public List<PayrollPriceTypeDTO> PayrollPriceTypes { get; private set; }
        public List<PayrollLevelDTO> PayrollLevels { get; private set; }
        public List<PayrollPriceFormulaDTO> PayrollPriceFormulas { get; private set; }
        public List<RoleDTO> UserRoles { get; private set; }
        public List<TimeDeviationCauseDTO> TimeDeviationCauses { get; set; }
        public List<TimeWorkAccountDTO> TimeWorkAccounts { get; private set; }
        public List<VacationGroupDTO> VacationsGroups { get; private set; }
        public List<AnnualLeaveGroupDTO> AnnualLeaveGroups { get; private set; }

        public EmployeeGroupDTO DefaultEmployeeGroup
        {
            get
            {
                return this.DefaultEmployeeGroupId > 0 ? this.EmployeeGroups?.FirstOrDefault(f => f.EmployeeGroupId == this.DefaultEmployeeGroupId) : null;
            }
        }
        public PayrollGroupDTO DefaultPayrollGroup
        {
            get
            {
                return this.DefaultPayrollGroupId > 0 ? this.Payrollgroups?.FirstOrDefault(f => f.PayrollGroupId == this.DefaultPayrollGroupId) : null;
            }
        }
        public VacationGroupDTO DefaultVacationGroup
        {
            get
            {
                return this.DefaultVacationGroupId > 0 ? this.VacationsGroups?.FirstOrDefault(f => f.VacationGroupId == this.DefaultVacationGroupId) : null;
            }
        }
        public DateTime MinDate
        {
            get
            {
                return DateTime.Today.AddYears(-200);
            }
        }
        public DateTime MaxDate
        {
            get
            {
                return DateTime.Today.AddYears(200);
            }
        }

        public ApiLookupEmployee(
            ApiConfig config,
            int actorCompanyId,
            int defaultRoleId, 
            int defaultEmployeeGroupId, 
            int defaultPayrollGroupId, 
            int defaultVacationGroupId,
            TermGroup_Country companyCountryId,
            List<EmployeeGroupDTO> employeeGroups,
            List<PayrollGroupDTO> payrollgroups,
            List<VacationGroupDTO> vacationsGroups,
            List<AnnualLeaveGroupDTO> annualLeaveGroups)
            : base(config, actorCompanyId)
        {
            this.DefaultRoleId = defaultRoleId;
            this.DefaultEmployeeGroupId = defaultEmployeeGroupId;
            this.DefaultPayrollGroupId = defaultPayrollGroupId;
            this.DefaultVacationGroupId = defaultVacationGroupId;
            this.CmpanyCountryId = companyCountryId;
            this.EmployeeGroups = employeeGroups;
            this.Payrollgroups = payrollgroups;
            this.VacationsGroups = vacationsGroups;
            this.AnnualLeaveGroups = annualLeaveGroups;
            this.ContactAddressItemsByEmployee = new Dictionary<string, List<ContactAddressItem>>();
            this.EmployeePositionsByEmployee = new Dictionary<string, List<EmployeePositionDTO>>();
            this.ExtraFieldRecordsByEmployee = new Dictionary<string, List<ExtraFieldRecordDTO>>();

            if (this.DefaultEmployeeGroupId == 0)
                this.DefaultEmployeeGroupId = this.EmployeeGroups?.FirstOrDefault()?.EmployeeGroupId ?? 0;
            if (this.DefaultPayrollGroupId == 0)
                this.DefaultPayrollGroupId = this.Payrollgroups?.FirstOrDefault()?.PayrollGroupId ?? 0;
            if (this.DefaultVacationGroupId == 0)
                this.DefaultVacationGroupId = this.VacationsGroups?.FirstOrDefault()?.VacationGroupId ?? 0;
        }

        public void SetOptionalLookups(
            Dictionary<TermGroup, List<GenericType>> terms,
            List<AccountDimDTO> accountDims,
            List<AccountDTO> accountInternals,
            List<AttestRoleDTO> attestRoles,
            List<GenericType> contactAddresses,
            List<GenericType> contactEcoms,
            List<PositionDTO> employeePositions,
            List<EndReasonDTO> employmentEndReasons,
            List<EmploymentTypeDTO> employmentTypes,
            List<ExtraFieldDTO> extraFields,
            List<ExtraFieldRecordDTO> extraFieldRecords,
            List<PayrollPriceTypeDTO> payrollPriceTypes,
            List<PayrollLevelDTO> payrollLevels,
            List<PayrollPriceFormulaDTO> payrollPriceFormulas,
            List<RoleDTO> userRoles,
            List<TimeDeviationCauseDTO> timeDeviationCauses,
            List<TimeWorkAccountDTO> timeWorkAccounts)
        {
            this.Terms = terms;
            this.AccountDims = accountDims;
            this.AccountInternals = accountInternals;
            this.AttestRoles = attestRoles;
            this.ContactAddresses = contactAddresses;
            this.ContactEcoms = contactEcoms;
            this.EmploymentEndReasons = employmentEndReasons;
            this.EmployeePositions = employeePositions;
            this.EmploymentTypes = employmentTypes;
            this.ExtraFields = extraFields;
            this.ExtraFieldRecords = extraFieldRecords;
            this.PayrollPriceTypes = payrollPriceTypes;
            this.PayrollLevels = payrollLevels;
            this.PayrollPriceFormulas = payrollPriceFormulas;
            this.UserRoles = userRoles;
            this.TimeDeviationCauses = timeDeviationCauses;
            this.TimeWorkAccounts = timeWorkAccounts;

            this.SynchAccountsInternalsWithDim();
        }

        private void SynchAccountsInternalsWithDim()
        {
            if (this.AccountDims.IsNullOrEmpty() || this.AccountInternals.IsNullOrEmpty())
                return;

            foreach (AccountDTO account in this.AccountInternals.Where(a => a.AccountDim == null))
            {
                account.AccountDim = this.AccountDims.FirstOrDefault(i => i.AccountDimId == account.AccountDimId);
            }
        }
    }
}

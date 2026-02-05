using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soe.Business.Tests.Business.Mock
{
    public enum EmployeeUserMockSetting
    {
        NewEmployee = 1,

        MultipleEmployments = 11,
        ClosedEmployments = 12,
        CompanyEndReason = 13,

        MultipleUserRoles = 21,
        ClosedUserRoles = 22,
    }

    public static class EmployeeUserMock
    {
        #region Members

        private static List<AccountDTO> _accounts = null;
        private static List<AttestRoleDTO> _attestRoles = null;
        private static List<EmployeeGroupDTO> _employeeGroups = null;
        private static List<EndReasonDTO> _employmentEndReasons = null;
        private static List<PayrollGroupDTO> _payrollGroups = null;
        private static List<PayrollPriceFormulaDTO> _payrollPriceFormula = null;
        private static List<VacationGroupDTO> _vacationGroups = null;
        private static List<RoleDTO> _userRoles = null;

        private static List<string> _ssnMale = null;
        private static List<string> _ssnFemale = null;
        private static List<string> _domains = null;

        private static int currentEmployeeId = 1;
        private static List<EmployeeUserDTO> currentEmployees;
        private static string nextEmployeeNr
        {
            get
            {
                return (currentEmployees.Count + 1).ToString();
            }
        }

        #endregion

        #region Public methods

        public static List<EmployeeUserDTO> Mock()
        {
            Setup();

            GenerateEmployee(TermGroup_Sex.Female);
            GenerateEmployee(TermGroup_Sex.Female, EmployeeUserMockSetting.ClosedEmployments);
            GenerateEmployee(TermGroup_Sex.Male, EmployeeUserMockSetting.MultipleEmployments);
            GenerateEmployee(TermGroup_Sex.Male, EmployeeUserMockSetting.MultipleEmployments, EmployeeUserMockSetting.ClosedEmployments, EmployeeUserMockSetting.CompanyEndReason);
            GenerateEmployee(TermGroup_Sex.Male, EmployeeUserMockSetting.NewEmployee);
            
            return currentEmployees;
        }

        #endregion

        #region Help-methods

        private static void Setup()
        {
            currentEmployeeId = 1;

            if (_accounts == null)
                _accounts = AccountMock.Mock();
            if (_attestRoles == null)
                _attestRoles = AttestRoleMock.Mock();
            if (_employeeGroups == null)
                _employeeGroups = EmployeeGroupMock.Mock();
            if (_employmentEndReasons == null)
                _employmentEndReasons = EmploymentEndReasonMock.Mock();
            if (_payrollGroups == null)
                _payrollGroups = PayrollGroupMock.Mock();
            if (_payrollPriceFormula == null)
                _payrollPriceFormula = PayrollPriceFormulaMock.Mock();
            if (_vacationGroups == null)
                _vacationGroups = VacationGroupMock.Mock();
            if (_userRoles == null)
                _userRoles = RoleMock.Mock();

            if (_ssnMale == null)
                _ssnMale = SsnMock.Mock(TermGroup_Sex.Male);
            if (_ssnFemale == null)
                _ssnFemale = SsnMock.Mock(TermGroup_Sex.Female);
            if (_domains == null)
                _domains = EmailDomainMock.Mock();

            currentEmployees = new List<EmployeeUserDTO>();
        }

        private static void GenerateEmployee(TermGroup_Sex sex, params EmployeeUserMockSetting[] settings)
        {
            EmployeeUserDTO e = Create(sex, settings);

            int year = DateTime.Today.Year;
            DateTime startOfYear = new DateTime(year, 01, 01);
            DateTime endOfYear = CalendarUtility.GetEndOfYear(startOfYear).Date;

            #region Employments

            bool multipleEmployments = HasSetting(EmployeeUserMockSetting.MultipleEmployments, settings);
            bool closedEmployments = HasSetting(EmployeeUserMockSetting.ClosedEmployments, settings);
            bool companyEndReason = HasSetting(EmployeeUserMockSetting.CompanyEndReason, settings);
            AddFirstEmployment();
            if (multipleEmployments)
                AddSecondEmployment();

            #endregion

            #region UserRoles

            bool multipleUserRoles = HasSetting(EmployeeUserMockSetting.MultipleUserRoles, settings);
            bool closedUserRoles = HasSetting(EmployeeUserMockSetting.ClosedUserRoles, settings);
            AddFirstUserRole();
            if (multipleUserRoles)
                AddSecondUserRole();

            #endregion

            currentEmployees.Add(e);

            void AddFirstEmployment()
            {
                int id = 1;
                DateTime dateFrom1 = startOfYear;
                DateTime? dateTo1 = closedEmployments ? endOfYear : (DateTime?)null;

                AddEmployment(e, 1, dateFrom1, dateTo1, employeeGroupId: id, payrollPriceTypeId: id, payrollLevelId: id, amount: id * 1000, companyEndReason: companyEndReason);
                AddEmployeeAccount(e, dateFrom1, dateTo1, accountId: id);
            }
            void AddSecondEmployment()
            {
                int id = 2;
                DateTime dateFrom2 = startOfYear.AddYears(1);
                DateTime? dateTo2 = closedEmployments ? endOfYear.AddYears(1) : (DateTime?)null;
                DateTime dateTo1 = dateFrom2.AddDays(-1);
                
                if (!closedEmployments)
                    e.Employments.First().DateTo = dateTo1;
                AddEmployment(e, 2, dateFrom2, dateTo2, employeeGroupId: id, payrollGroupId: id, vacationGroupId: id, payrollPriceTypeId: id, payrollLevelId: id, amount: id * 1000, companyEndReason: companyEndReason);

                if (!closedEmployments)
                    e.Accounts.First().DateTo = dateTo1;
                AddEmployeeAccount(e, dateFrom2, dateTo2, accountId: id);
            }
            void AddFirstUserRole()
            {
                int id = 1;
                DateTime dateFrom1 = startOfYear;
                DateTime? dateTo1 = closedEmployments ? endOfYear : (DateTime?)null;

                AddUserRoles(e, dateFrom1, dateTo1, id: id);
            }
            void AddSecondUserRole()
            {
                int id = 2;
                DateTime dateFrom2 = startOfYear.AddYears(1);
                DateTime? dateTo2 = closedEmployments ? endOfYear.AddYears(1) : (DateTime?)null;
                DateTime dateTo1 = dateFrom2.AddDays(-1);

                if (!closedUserRoles)
                    e.Employments.First().DateTo = dateTo1;
                AddUserRoles(e, dateFrom2, dateTo2, id: id);
            }
        }

        private static (string First, string Last) GenerateName(EmployeeUserMockSetting[] settings)
        {
            StringBuilder firstName = new StringBuilder();
            firstName.Append(HasSetting(EmployeeUserMockSetting.NewEmployee, settings) ? "New" : "Existing");
            firstName.Append(nextEmployeeNr);

            StringBuilder lastName = new StringBuilder();
            lastName.Append(HasSetting(EmployeeUserMockSetting.MultipleEmployments, settings) ? "Multiple" : "Single");
            lastName.Append(HasSetting(EmployeeUserMockSetting.ClosedEmployments, settings) ? "Closed" : "Open");

            return (firstName.ToString(), lastName.ToString());
        }

        private static string GenerateSSn(TermGroup_Sex sex)
        {
            int maleCounter = currentEmployees.Count(e => e.Sex == TermGroup_Sex.Male);
            int femaleCounter = currentEmployees.Count(e => e.Sex == TermGroup_Sex.Female);

            if (sex == TermGroup_Sex.Male)
                return maleCounter < _ssnMale.Count ? _ssnMale[maleCounter] : _ssnMale.LastOrDefault();
            else if (sex == TermGroup_Sex.Female)
                return femaleCounter < _ssnFemale.Count ? _ssnFemale[femaleCounter ] : _ssnFemale.LastOrDefault();
            else
                return string.Empty;
        }

        private static string GenerateDomain()
        {
            if (_domains.IsNullOrEmpty())
                return "se";

            return _domains[new Random().Next(0, _domains.Count - 1)];
        }

        private static EmployeeUserDTO Create(TermGroup_Sex sex, EmployeeUserMockSetting[] settings)
        {
            int employeeId = HasSetting(EmployeeUserMockSetting.NewEmployee, settings) ? 0 : currentEmployeeId;
            var name = GenerateName(settings);

            return new EmployeeUserDTO()
            {
                EmployeeId = employeeId,
                EmployeeNr = nextEmployeeNr,
                FirstName = name.First,
                LastName = name.Last,
                SocialSec = GenerateSSn(sex),
                Sex = sex,
                Email = $"{name.First}.{name.Last}@test.{GenerateDomain()}",
                ExternalCode = ExternalCodesMock.Mock().Skip(employeeId).FirstOrDefault(),
                ExternalAuthId = ExternalCodesMock.Mock().Skip(employeeId + 1).FirstOrDefault(),
                Employments = new List<EmploymentDTO>(),
                Accounts = new List<EmployeeAccountDTO>(),
                PayrollReportsPersonalCategory = (int)TermGroup_PayrollExportPersonalCategory.Unknown,
                PayrollReportsWorkTimeCategory = (int)TermGroup_PayrollExportWorkTimeCategory.Unknown,
                PayrollReportsSalaryType = (int)TermGroup_PayrollExportSalaryType.Unknown,
                PayrollReportsWorkPlaceNumber = 1,
                PayrollReportsCFARNumber = 1,
                WorkPlaceSCB = string.Empty,
                PartnerInCloseCompany = false,
                BenefitAsPension = false,
                AFACategory = (int)TermGroup_AfaCategory.None,
                AFASpecialAgreement = (int)TermGroup_AfaSpecialAgreement.None,
                AFAWorkplaceNr = string.Empty,
                AFAParttimePensionCode = false,
                CollectumITPPlan = (int)TermGroup_PayrollReportsCollectumITPplan.None,
                CollectumAgreedOnProduct = string.Empty,
                CollectumCancellationDate = CalendarUtility.DATETIME_DEFAULT,
                CollectumCancellationDateIsLeaveOfAbsence = false,
                CollectumCostPlace = string.Empty,
                KpaRetirementAge = 1,
                KpaBelonging = (int)KpaBelonging.Unknown,
                KpaEndCode = (int)KpaEndCode.U1,
                KpaAgreementType = (int)KpaAgreementType.Unknown,
                BygglosenAgreementArea = string.Empty,
                BygglosenAllocationNumber = string.Empty,
                BygglosenSalaryFormula = _payrollPriceFormula?.FirstOrDefault()?.PayrollPriceFormulaId ?? 0,
                BygglosenSalaryFormulaName = _payrollPriceFormula?.FirstOrDefault()?.Name ?? "N/A",
                BygglosenMunicipalCode = string.Empty,
            };
        }
        
        private static void AddEmployment(EmployeeUserDTO employee, int employmentId, DateTime dateFrom, DateTime? dateTo, int employeeGroupId, int? payrollGroupId = null, int? vacationGroupId = null, int? payrollPriceTypeId = null, int? payrollLevelId = null, decimal? amount = null, bool companyEndReason = false)
        {
            if (employee == null)
                return;

            EmploymentDTO employment = new EmploymentDTO
            {
                EmploymentId = employmentId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                ExternalCode = ExternalCodesMock.Mock().Skip(employmentId).FirstOrDefault(),
                EmployeeGroupId = employeeGroupId,
                EmployeeGroupName = _employeeGroups?.FirstOrDefault(i => i.EmployeeGroupId == employeeGroupId)?.Name ?? string.Empty,
                PayrollGroupId = payrollGroupId,
                PayrollGroupName = _payrollGroups?.FirstOrDefault(i => i.PayrollGroupId == payrollGroupId)?.Name ?? string.Empty,
                EmploymentVacationGroup = new List<EmploymentVacationGroupDTO>(),
                PriceTypes = new List<EmploymentPriceTypeDTO>(),
            };

            if (vacationGroupId.HasValue)
            {
                VacationGroupDTO vacationGroup = _vacationGroups?.FirstOrDefault(i => i.VacationGroupId == vacationGroupId.Value);

                employment.EmploymentVacationGroup.Add(new EmploymentVacationGroupDTO
                {
                    EmploymentVacationGroupId = 1,
                    EmploymentId = employmentId,
                    VacationGroupId = vacationGroupId.Value,
                    Name = vacationGroup?.Name.NullToEmpty(),
                    Type = vacationGroup != null ? (int)vacationGroup.Type : 0,
                    FromDate = dateFrom,
                    State = SoeEntityState.Active,
                });
            }    
            if (payrollPriceTypeId.HasValue)
            {
                employment.PriceTypes.Add(new EmploymentPriceTypeDTO
                {
                    PayrollPriceTypeId = payrollPriceTypeId.Value,
                    Periods = new List<EmploymentPriceTypePeriodDTO>
                    {
                        new EmploymentPriceTypePeriodDTO
                        {
                            PayrollLevelId = payrollLevelId,
                            Amount = amount ?? 0,
                            FromDate = dateFrom,
                        }
                    }
                });
            }

            if (employment.DateTo.HasValue)
            {
                EndReasonDTO endReason = _employmentEndReasons?.FirstOrDefault(i => i.EndReasonId != 0 && i.SystemEndReson == !companyEndReason);
                if (endReason != null)
                {
                    employment.EmploymentEndReason = endReason.EndReasonId;
                    employment.EmploymentEndReasonName = endReason.Name;
                }
            }

            employee.Employments.Add(employment);
        }

        private static void AddEmployeeAccount(EmployeeUserDTO employee, DateTime dateFrom, DateTime? dateTo, int accountId)
        {
            if (employee == null)
                return;

            EmployeeAccountDTO employeeAccount = new EmployeeAccountDTO()
            {
                EmployeeId = employee.EmployeeId,
                AccountId = accountId,
                DateFrom = dateFrom,
                DateTo = dateTo,
            };

            employee.Accounts.Add(employeeAccount);
        }

        private static void AddUserRoles(EmployeeUserDTO employee, DateTime dateFrom, DateTime? dateTo, int id)
        {
            if (employee == null)
                return;

            UserRolesDTO userRoles = new UserRolesDTO()
            {
                ActorCompanyId = employee.ActorCompanyId,
                Roles = new List<UserCompanyRoleDTO>()
                {
                    new UserCompanyRoleDTO
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        RoleId = id,
                        Name = _userRoles.FirstOrDefault(i => i.RoleId == id)?.Name.NullToEmpty(),
                        Default = id == 1,
                        State = SoeEntityState.Active,
                    }
                },
                AttestRoles = new List<UserAttestRoleDTO>()
                {
                    new UserAttestRoleDTO
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        AttestRoleId = id,
                        Name = _attestRoles.FirstOrDefault(i => i.AttestRoleId == id)?.Name.NullToEmpty(),
                        AccountId = id,
                        AccountName = _accounts.FirstOrDefault(i => i.AccountId == id)?.Name.NullToEmpty(),
                    }
                }
            };

            if (employee.UserRoles == null)
                employee.UserRoles = new List<UserRolesDTO>();
            employee.UserRoles.Add(userRoles);
        }

        private static bool HasSetting(EmployeeUserMockSetting setting, EmployeeUserMockSetting[] settings)
        {
            return !settings.IsNullOrEmpty() && settings.Contains(setting);
        }

        #endregion
    }
}

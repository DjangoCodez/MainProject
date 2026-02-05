using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.API;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using SoftOne.Soe.Shared.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class EmployeeManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public EmployeeManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region CardNumber

        public List<CardNumberGridDTO> GetCardNumbers(int actorCompanyId, int roleId, int userId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Employee.NoTracking();
            List<int> employeeIds = (from e in entitiesReadOnly.Employee
                                     where e.ActorCompanyId == actorCompanyId &&
                                     !e.Hidden &&
                                     !e.Vacant &&
                                     e.State == (int)SoeEntityState.Active
                                     select e.EmployeeId).ToList();

            // Account hierarchy
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, actorCompanyId);
            if (useAccountHierarchy)
            {
                Employee currentEmployee = GetEmployeeByUser(actorCompanyId, userId);
                employeeIds = GetValidEmployeeByAccountHierarchy(entitiesReadOnly, actorCompanyId, roleId, userId, employeeIds, currentEmployee, DateTime.Today, DateTime.Today, getHidden: true, onlyDefaultAccounts: false);
            }

            List<CardNumberGridDTO> numbers = (from e in entitiesReadOnly.Employee
                                               where e.ActorCompanyId == actorCompanyId &&
                                               e.CardNumber != null && e.CardNumber != String.Empty &&
                                               employeeIds.Contains(e.EmployeeId)
                                               select new CardNumberGridDTO()
                                               {
                                                   EmployeeId = e.EmployeeId,
                                                   CardNumber = e.CardNumber,
                                                   EmployeeNumber = e.EmployeeNr,
                                                   EmployeeName = e.ContactPerson.FirstName + " " + e.ContactPerson.LastName
                                               }).ToList();

            return numbers.OrderBy(n => n.EmployeeNrSort).ToList();
        }

        public ActionResult ClearCardNumber(int employeeId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                Employee employee = GetEmployee(entities, employeeId, actorCompanyId, onlyActive: false);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Employee");

                employee.CardNumber = null;
                SetModifiedProperties(employee);

                ActionResult result = SaveChanges(entities);

                #region WebPubSub

                if (result.Success)
                    SendWebPubSubMessage(entities, base.ActorCompanyId, employeeId, WebPubSubMessageAction.Update);

                #endregion

                return result;
            }
        }

        public ActionResult CardNumberExists(int actorCompanyId, string cardNumber, int? excludeEmployeeId)
        {
            ActionResult result = new ActionResult();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            entities.ContactPerson.NoTracking();
            Employee employee = (from e in entities.Employee.Include("ContactPerson")
                                 where e.ActorCompanyId == actorCompanyId &&
                                 e.CardNumber == cardNumber &&
                                 e.State == (int)SoeEntityState.Active
                                 select e).FirstOrDefault();

            if (employee != null && excludeEmployeeId.HasValue && excludeEmployeeId.Value == employee.EmployeeId)
                employee = null;
            if (employee != null)
            {
                result.BooleanValue = true;
                result.IntegerValue = employee.EmployeeId;
                result.StringValue = employee.EmployeeNrAndName;
            }

            return result;
        }

        public Employee GetEmployeeByCardNumber(int actorCompanyId, string cardNumber, bool loadEmployment = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.AsNoTracking();
            entities.ContactPerson.NoTracking();
            return GetEmployeeByCardNumber(entities, actorCompanyId, cardNumber, loadEmployment);
        }

        public Employee GetEmployeeByCardNumber(CompEntities entities, int actorCompanyId, string cardNumber, bool loadEmployment = false)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return null;

            IQueryable<Employee> query = entities.Employee.Include("ContactPerson");
            if (loadEmployment)
                query = query.Include("Employment.EmploymentChangeBatch.EmploymentChange");

            return (from e in query
                    where e.ActorCompanyId == actorCompanyId &&
                    e.CardNumber == cardNumber &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        #endregion

        #region CSR

        public CsrResponseDTO CsrInquiry(int actorCompanyId, int employeeId, int year)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return CsrInquiry(entities, null, actorCompanyId, employeeId, year);
        }

        public CsrResponseDTO CsrInquiry(CompEntities entities, TransactionScope transaction, int actorCompanyId, int employeeId, int year)
        {
            // Validate year
            int currentYear = DateTime.Today.Year;
            if (year != currentYear)
            {
                // Som inkomstår accepteras
                // innevarande år -1 fr.o.m. den 1 t.o.m. den 17 januari
                // innevarande år +1 fr.o.m. mitten av december t.o.m. den 31 december
                // https://anypoint.mulesoft.com/apiplatform/skatteverket-4/#/portals/organizations/46326a92-50b7-40fc-bc22-99a19adfbf46/apis/36068452/versions/2049120/pages/287501

                if (year > currentYear)
                {
                    DateTime minDate = new DateTime(currentYear, 12, 15);
                    if (DateTime.Today < minDate)
                        return new CsrResponseDTO(String.Format(GetText(11718, "Förfrågan för inkomstår {0} kan göras tidigast {1}."), year.ToString(), minDate.ToShortDateString()));
                }
                else
                {
                    DateTime maxDate = new DateTime(currentYear, 1, 17);
                    if (DateTime.Today > maxDate)
                        return new CsrResponseDTO(String.Format(GetText(11719, "Förfrågan för inkomstår {0} kan göras senast {1}."), year.ToString(), maxDate.ToShortDateString()));
                }
            }

            // Get org. number from company
            Company company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (company == null)
                return new CsrResponseDTO(GetText(11720, "Kan ej hitta den anställdes företag."));

            if (company.OrgNr.IsNullOrEmpty())
                return new CsrResponseDTO(GetText(11724, "Organisationsnummer saknas på den anställdes företag."));

            string orgNr = StringUtility.OrgNrWith16(company.OrgNr);
            if (orgNr.IsNullOrEmpty() || orgNr.Length != 12)
                return new CsrResponseDTO(String.Format(GetText(11721, "Felaktigt organisationsnummer ({0}) på den anställdes företag."), StringUtility.NullToEmpty(company.OrgNr)));

            // Get social security number from emp
            Employee employee = GetEmployee(entities, employeeId, actorCompanyId, loadContactPerson: true);
            if (employee == null)
                return new CsrResponseDTO(GetText(11722, "Kan ej hitta anställd."));

            if (employee.SocialSec.IsNullOrEmpty())
                return new CsrResponseDTO(GetText(11725, "Personnummer saknas på den anställde."));

            string socialSecNbr = StringUtility.SocialSecYYYYMMDDXXXX(employee.SocialSec);
            if (socialSecNbr.IsNullOrEmpty() || socialSecNbr.Length != 12)
                return new CsrResponseDTO(String.Format(GetText(11723, "Felaktigt personnummer ({0}) på den anställde."), StringUtility.NullToEmpty(employee.SocialSec)));

            //CsrResponseDTO response = SkatteverketConnector.GetSkatteAvdrag(orgNr, socialSecNbr, year);
            CsrResponseDTO response = SkatteverketConnector.GetFOS(orgNr, socialSecNbr, year);

            if (response.ErrorMessage.IsNullOrEmpty())
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                ActionResult result = transaction == null ? HandleCsrInquiry(entitiesReadOnly, null, actorCompanyId, employeeId, response) : HandleCsrInquiry(entities, transaction, actorCompanyId, employeeId, response);
                if (!result.Success)
                    response.ErrorMessage = result.ErrorMessage;
            }

            return response;
        }

        public List<CsrResponseDTO> CsrInquiries(int actorCompanyId, List<int> employeeIds, int year)
        {
            List<CsrResponseDTO> responseDTOs = new List<CsrResponseDTO>();

            // Validate year
            int currentYear = DateTime.Today.Year;
            if (year != currentYear)
            {
                // Som inkomstår accepteras
                // innevarande år -1 fr.o.m. den 1 t.o.m. den 17 januari
                // innevarande år +1 fr.o.m. mitten av december t.o.m. den 31 december
                // https://anypoint.mulesoft.com/apiplatform/skatteverket-4/#/portals/organizations/46326a92-50b7-40fc-bc22-99a19adfbf46/apis/36068452/versions/2049120/pages/287501

                if (year > currentYear)
                {
                    DateTime minDate = new DateTime(currentYear, 12, 15);
                    if (DateTime.Today < minDate)
                        return new List<CsrResponseDTO>() { new CsrResponseDTO(String.Format(GetText(11718, "Förfrågan för inkomstår {0} kan göras tidigast {1}."), year.ToString(), minDate.ToShortDateString())) };
                }
                else if (year < currentYear)
                {
                    DateTime maxDate = new DateTime(currentYear, 1, 17);
                    if (DateTime.Today > maxDate)
                        return new List<CsrResponseDTO>() { new CsrResponseDTO(String.Format(GetText(11719, "Förfrågan för inkomstår {0} kan göras senast {1}."), year.ToString(), maxDate.ToShortDateString())) };
                }
            }

            // Get org. number from company
            Company company = CompanyManager.GetCompany(actorCompanyId);
            if (company == null)
                return new List<CsrResponseDTO>() { new CsrResponseDTO(GetText(11720, "Kan ej hitta den anställdes företag.")) };

            if (company.OrgNr.IsNullOrEmpty())
                return new List<CsrResponseDTO>() { new CsrResponseDTO(GetText(11724, "Organisationsnummer saknas på den anställdes företag.")) };

            string orgNr = StringUtility.OrgNrWith16(company.OrgNr);
            if (orgNr.IsNullOrEmpty() || orgNr.Length != 12)
                return new List<CsrResponseDTO>() { new CsrResponseDTO(String.Format(GetText(11721, "Felaktigt organisationsnummer ({0}) på den anställdes företag."), StringUtility.NullToEmpty(company.OrgNr))) };

            // Get social security number from emp
            List<Employee> employees = GetAllEmployeesByIds(actorCompanyId, employeeIds);
            employees = employees.Where(w => !string.IsNullOrEmpty(w.SocialSec)).ToList();

            SkatteAvdragFleraPersoner request = new SkatteAvdragFleraPersoner();
            Dictionary<string, int> employeePersonnummerDict = new Dictionary<string, int>();

            foreach (var employee in employees)
            {
                string personnummer = StringUtility.SocialSecYYYYMMDDXXXX(employee.SocialSec);

                if (personnummer.IsNullOrEmpty() || personnummer.Length != 12)
                    responseDTOs.Add(new CsrResponseDTO(String.Format(GetText(11723, "Felaktigt personnummer ({0}) på den anställde."), StringUtility.NullToEmpty(employee.SocialSec))));
                else
                {
                    if (!employeePersonnummerDict.ContainsKey(personnummer))
                    {
                        employeePersonnummerDict.Add(personnummer, employee.EmployeeId);
                        request.personnummer.Add(personnummer);
                    }
                }
            }

            //responseDTOs.AddRange(SkatteverketConnector.GetFleraSkatteavdrag(orgNr, request, year));

            responseDTOs.AddRange(SkatteverketConnector.GetFleraFOS(orgNr, request, year));

            foreach (var response in responseDTOs.Where(x => x.personnummer != null))
            {
                if (employeePersonnummerDict.TryGetValue(response.personnummer, out int employeeId))
                {
                    response.EmployeeId = employeeId;
                    if (!response.felmeddelande.IsNullOrEmpty() && response.felmeddelande != "OK")
                        response.ErrorMessage = response.felmeddelande;
                }
            }

            return HandleCsrInquiries(actorCompanyId, responseDTOs);
        }

        public List<CsrResponseDTO> HandleCsrInquiries(int actorCompanyId, List<CsrResponseDTO> responses)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<int> employeeIds = responses.Where(w => string.IsNullOrEmpty(w.ErrorMessage)).Select(s => s.EmployeeId).ToList();
                List<Employee> employees = GetAllEmployeesByIds(entities, actorCompanyId, employeeIds);

                foreach (var response in responses.Where(w => string.IsNullOrEmpty(w.ErrorMessage)))
                {
                    var employee = employees.FirstOrDefault(f => f.EmployeeId == response.EmployeeId);

                    if (employee == null)
                        continue;

                    // Get emp tax for specified year, create new if it does not exist
                    EmployeeTaxSE tax = GetEmployeeTaxSE(entities, employee.EmployeeId, response.Year);
                    if (tax == null)
                    {
                        tax = CreateEmployeeTaxSE(entities, employee, TermGroup_EmployeeTaxType.NoTax, response.Year, null, null, null);

                        // Copy some values from previous year
                        EmployeeTaxSE prevTax = GetEmployeeTaxSE(entities, employee.EmployeeId, response.Year - 1);
                        if (prevTax != null)
                        {
                            tax.SalaryDistressReservedAmount = prevTax.SalaryDistressReservedAmount;
                            tax.SalaryDistressAmountType = prevTax.SalaryDistressAmountType;
                            tax.SalaryDistressAmount = prevTax.SalaryDistressAmount;
                            tax.SalaryDistressCase = prevTax.SalaryDistressCase;
                        }
                    }

                    if (tax != null && tax.MainEmployer)
                    {
                        string taxForm = response.skatteform;
                        string taxRate = response.skattetabell;
                        int adjPercent = response.procentbeslut;

                        // Tax type
                        if (!taxForm.IsNullOrEmpty())
                        {
                            switch (taxForm)
                            {
                                case "A":
                                    tax.Type = (int)TermGroup_EmployeeTaxType.TableTax;
                                    break;

                                case "F":
                                    tax.Type = (int)TermGroup_EmployeeTaxType.NoTax;
                                    break;

                                case "FA":
                                    tax.Type = (int)TermGroup_EmployeeTaxType.TableTax;
                                    break;
                            }
                        }


                        // Tax rate
                        if (!taxRate.IsNullOrEmpty())
                            tax.TaxRate = Convert.ToInt32(taxRate);

                        // Adjustment
                        if (adjPercent != 0)
                        {
                            tax.Type = (int)TermGroup_EmployeeTaxType.Adjustment;
                            tax.AdjustmentType = (int)TermGroup_EmployeeTaxAdjustmentType.PercentTax;
                            tax.AdjustmentValue = Convert.ToDecimal(adjPercent);
                        }

                        tax.TaxRateColumn = 1;
                        using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                        tax.OneTimeTaxPercent = GetDefaultOneTimeTaxRate(entitiesReadOnly, tax.Year, actorCompanyId);
                        tax.CSRExportDate = tax.CSRImportDate = DateTime.Now;

                        var result = SaveEmployeeTaxSE(tax.ToDTO(), actorCompanyId, TermGroup_TrackChangesActionMethod.Employee_CsrInquiry);

                        if (!result.Success)
                            response.ErrorMessage = result.ErrorMessage;
                    }
                }
            }

            return responses;
        }

        public ActionResult HandleCsrInquiry(CompEntities entities, TransactionScope transaction, int actorCompanyId, int employeeId, CsrResponseDTO response)
        {
            ActionResult result = new ActionResult();

            if (!response.ErrorMessage.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.Unknown, response.ErrorMessage);

            // Get emp
            Employee employee = GetEmployee(entities, employeeId, actorCompanyId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11722, "Kan ej hitta anställd."));

            // Get emp tax for specified year, create new if it does not exist
            EmployeeTaxSE tax = GetEmployeeTaxSE(entities, employeeId, response.Year);
            if (tax == null)
            {
                tax = CreateEmployeeTaxSE(entities, employee, TermGroup_EmployeeTaxType.NoTax, response.Year, null, null, null);

                // Copy some values from previous year
                EmployeeTaxSE prevTax = GetEmployeeTaxSE(entities, employee.EmployeeId, response.Year - 1);
                if (prevTax != null)
                {
                    tax.SalaryDistressReservedAmount = prevTax.SalaryDistressReservedAmount;
                    tax.SalaryDistressAmountType = prevTax.SalaryDistressAmountType;
                    tax.SalaryDistressAmount = prevTax.SalaryDistressAmount;
                    tax.SalaryDistressCase = prevTax.SalaryDistressCase;
                }
            }

            if (tax != null && tax.MainEmployer)
            {
                string taxForm = response.skatteform;
                string taxRate = response.skattetabell;
                int adjPercent = response.procentbeslut;

                // Tax type
                if (!taxForm.IsNullOrEmpty())
                {
                    switch (taxForm)
                    {
                        case "A":
                            tax.Type = (int)TermGroup_EmployeeTaxType.TableTax;
                            break;

                        case "F":
                            tax.Type = (int)TermGroup_EmployeeTaxType.NoTax;
                            break;

                        case "FA":
                            tax.Type = (int)TermGroup_EmployeeTaxType.TableTax;
                            break;
                    }
                }

                // Tax rate
                if (!taxRate.IsNullOrEmpty())
                    tax.TaxRate = Convert.ToInt32(taxRate);

                // Adjustment
                if (adjPercent != 0)
                {
                    tax.Type = (int)TermGroup_EmployeeTaxType.Adjustment;
                    tax.AdjustmentType = (int)TermGroup_EmployeeTaxAdjustmentType.PercentTax;
                    tax.AdjustmentValue = Convert.ToDecimal(adjPercent);
                }

                tax.TaxRateColumn = 1;
                tax.OneTimeTaxPercent = GetDefaultOneTimeTaxRate(entities, tax.Year, actorCompanyId);
                tax.CSRExportDate = tax.CSRImportDate = DateTime.Now;

                result = transaction != null ? SaveEmployeeTaxSE(entities, transaction, tax.ToDTO(), actorCompanyId, TermGroup_TrackChangesActionMethod.Employee_CsrInquiry) : SaveEmployeeTaxSE(tax.ToDTO(), actorCompanyId, TermGroup_TrackChangesActionMethod.Employee_CsrInquiry);
            }

            return result;
        }

        public List<EmployeeCSRExportDTO> GetEmployeesForCSRExport(int actorCompanyId, int year)
        {
            List<EmployeeCSRExportDTO> employeeCSRExportDTO = new List<EmployeeCSRExportDTO>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<Employee> employees = (from e in entitiesReadOnly.Employee.Include("ContactPerson").Include("EmployeeTaxSE")
                                        where e.ActorCompanyId == actorCompanyId &&
                                        e.State == (int)SoeEntityState.Active &&
                                        !e.Hidden
                                        orderby e.EmployeeNr
                                        select e).ToList();

            foreach (Employee employee in employees)
            {
                EmployeeTaxSE taxSE = employee.EmployeeTaxSE.FirstOrDefault(s => s.Year == year && s.State == (int)SoeEntityState.Active);
                if (taxSE == null)
                    taxSE = employee.EmployeeTaxSE.FirstOrDefault(s => s.Year == year - 1 && s.State == (int)SoeEntityState.Active);
                if (taxSE != null && !taxSE.MainEmployer)
                    continue;

                EmployeeCSRExportDTO employeeCSRExport = new EmployeeCSRExportDTO
                {
                    EmployeeTaxId = taxSE != null ? taxSE.EmployeeTaxId : 0,
                    EmployeeId = employee.EmployeeId,
                    EmployeeNr = string.IsNullOrEmpty(employee.EmployeeNr) ? string.Empty : employee.EmployeeNr,
                    EmployeeName = employee.Name,
                    EmployeeSocialSec = string.IsNullOrEmpty(employee.ContactPerson.SocialSec) ? string.Empty : employee.ContactPerson.SocialSec,
                    CsrImportDate = taxSE?.CSRImportDate ?? null,
                    CsrExportDate = taxSE?.CSRExportDate ?? null,
                    Year = taxSE != null ? taxSE.Year : 0,
                };

                employeeCSRExportDTO.Add(employeeCSRExport);
            }

            return employeeCSRExportDTO;
        }

        #endregion

        #region EmployeeGroup

        public List<EmployeeGroup> GetEmployeeGroups(int actorCompanyId, bool loadTimeDeviationCauseMappings = false, bool loadTimeDeviationCauses = false, bool loadDayTypes = false, bool loadAttestTransitions = false, bool onlyActive = true, bool loadExternalCode = false, bool loadTimeCodeBreaks = false, bool loadTimeStampRounding = false, int? employeeGroupId = null, bool loadTimeCodes = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return GetEmployeeGroups(entities, actorCompanyId, loadTimeDeviationCauseMappings, loadTimeDeviationCauses, loadDayTypes, loadAttestTransitions, onlyActive, loadExternalCode, loadTimeCodeBreaks, loadTimeStampRounding, employeeGroupId, loadTimeCodes);
        }

        public List<EmployeeGroup> GetEmployeeGroups(CompEntities entities, int actorCompanyId, bool loadTimeDeviationCauseMappings = false, bool loadTimeDeviationCauses = false, bool loadDayTypes = false, bool loadAttestTransitions = false, bool onlyActive = true, bool loadExternalCode = false, bool loadTimeCodeBreaks = false, bool loadTimeStampRounding = false, int? employeeGroupId = null, bool loadTimeCodes = false)
        {
            List<EmployeeGroup> employeeGroups = null;
            IQueryable<EmployeeGroup> query = entities.EmployeeGroup;

            if (loadTimeDeviationCauseMappings || loadTimeDeviationCauses)
            {
                query = query.Include("TimeDeviationCause")
                             .Include("EmployeeGroupTimeDeviationCause")
                             .Include("EmployeeGroupTimeDeviationCauseRequest")
                             .Include("EmployeeGroupTimeDeviationCauseTimeCode")
                             .Include("EmployeeGroupTimeDeviationCauseAbsenceAnnouncement");

                if (loadTimeDeviationCauses)
                    query = query.Include("EmployeeGroupTimeDeviationCause.TimeDeviationCause")
                                 .Include("EmployeeGroupTimeDeviationCauseRequest.TimeDeviationCause");
            }
            if (loadDayTypes)
            {
                query = query.Include("DayType");
                query = query.Include("EmployeeGroupDayType");
            }
            if (loadAttestTransitions)
                query = query.Include("AttestTransition");
            if (loadTimeCodes)
                query = query.Include("TimeCodes");
            if (loadTimeCodeBreaks)
                query = query.Include("TimeCodeBreak");
            if (loadTimeStampRounding)
                query = query.Include("TimeStampRounding");
            if (employeeGroupId.HasValue)
                query = query.Where(t => t.EmployeeGroupId == employeeGroupId);
            employeeGroups = (from eg in query
                              where eg.ActorCompanyId == actorCompanyId &&
                              (onlyActive ? eg.State == (int)SoeEntityState.Active : eg.State != (int)SoeEntityState.Deleted)
                              select eg).ToList();

            if (employeeGroups.IsNullOrEmpty())
                return new List<EmployeeGroup>();

            if (loadTimeDeviationCauses || loadDayTypes || loadAttestTransitions || loadExternalCode)
            {
                foreach (EmployeeGroup employeeGroup in employeeGroups)
                {
                    if (loadTimeDeviationCauses)
                    {
                        employeeGroup.TimeDeviationCausesNames = employeeGroup.EmployeeGroupTimeDeviationCause.Where(i => i.TimeDeviationCause != null).Select(i => i.TimeDeviationCause.Name).ToCommaSeparated();
                        employeeGroup.TimeDeviationCausesRequestNames = employeeGroup.EmployeeGroupTimeDeviationCauseRequest.Where(i => i.TimeDeviationCause != null).Select(i => i.TimeDeviationCause.Name).ToCommaSeparated();
                    }
                    if (loadDayTypes)
                        employeeGroup.DayTypesNames = employeeGroup.DayType.Select(i => i.Name).ToCommaSeparated();

                    if (loadAttestTransitions)
                        employeeGroup.AttestTransitionNames = employeeGroup.AttestTransition?.Select(i => i.Name).ToCommaSeparated();

                    if (loadExternalCode)
                        employeeGroup.ExternalCodes = new List<string>();
                }
            }

            if (loadExternalCode)
                LoadEmployeeGroupExternalCodes(entities, employeeGroups, actorCompanyId);

            return employeeGroups.OrderBy(e => e.Name).ToList();
        }

        public List<EmployeeGroup> GetEmployeeGroups(CompEntities entities, List<int> employeeGroupIds)
        {
            employeeGroupIds = employeeGroupIds?.Distinct().ToList() ?? new List<int>();
            return entities.EmployeeGroup.Where(eg => employeeGroupIds.Contains(eg.EmployeeGroupId)).ToList();
        }

        public List<EmployeeGroup> GetEmployeeGroupsBySearch(string search, int actorCompanyId, int take)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return (from eg in entities.EmployeeGroup
                    where eg.Name.ToLower().Contains(search.ToLower()) &&
                    eg.ActorCompanyId == actorCompanyId &&
                    eg.State == (int)SoeEntityState.Active
                    orderby eg.Name ascending
                    select eg).Take(take).ToList();
        }

        public List<EmployeeGroup> GetEmployeeGroupsDictForAttestRule(int attestRuleHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return (from eg in entities.EmployeeGroup
                    where eg.AttestRuleHead.Any(a => a.AttestRuleHeadId == attestRuleHeadId) &&
                    eg.State == (int)SoeEntityState.Active
                    select eg).ToList();
        }

        public void LoadEmployeeGroupExternalCodes(CompEntities entities, List<EmployeeGroup> employeeGroups, int actorCompanyId)
        {
            if (employeeGroups.IsNullOrEmpty() || employeeGroups.All(eg => eg.ExternalCodesIsLoaded))
                return;

            var allExternalCodes = ActorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.EmployeeGroup, actorCompanyId);

            foreach (var employeeGroup in employeeGroups.Where(eg => !eg.ExternalCodesIsLoaded))
            {
                employeeGroup.ExternalCodes = new List<string>();

                var externalCodes = allExternalCodes.Where(eg => eg.RecordId == employeeGroup.EmployeeGroupId).ToList();
                if (!externalCodes.IsNullOrEmpty())
                {
                    employeeGroup.ExternalCodes.AddRange(externalCodes.Select(s => s.ExternalCode));
                    employeeGroup.ExternalCodesString = StringUtility.GetSeparatedString(externalCodes.Select(s => s.ExternalCode), Constants.Delimiter, true, false);
                }

                employeeGroup.ExternalCodesIsLoaded = true;
            }
        }

        public Dictionary<int, string> GetEmployeeGroupsDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = GetEmployeeGroups(actorCompanyId).ToDictionary(k => k.EmployeeGroupId, v => v.Name);
            if (addEmptyRow)
                dict.Add(0, " ");
            return dict.Sort();
        }

        public EmployeeGroup GetEmployeeGroup(int employeeGroupId, bool loadTransitions = false, bool loadDayType = false, bool loadTimeDeviationCause = false, bool loadTimeCode = false, bool loadRuleWorkTimePeriod = false, bool loadExternalCode = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return GetEmployeeGroup(entities, employeeGroupId, loadTransitions, loadDayType, loadTimeDeviationCause, loadTimeCode, loadRuleWorkTimePeriod, loadExternalCode);
        }

        public EmployeeGroup GetEmployeeGroup(CompEntities entities, int employeeGroupId, bool loadTransitions = false, bool loadDayType = false, bool loadTimeDeviationCause = false, bool loadTimeCode = false, bool loadRuleWorkTimePeriod = false, bool loadExternalCode = false)
        {
            if (employeeGroupId <= 0)
                return null;

            int actorCompanyId = base.ActorCompanyId;
            EmployeeGroup employeeGroup = (from eg in entities.EmployeeGroup
                                           where eg.EmployeeGroupId == employeeGroupId
                                           && eg.ActorCompanyId == actorCompanyId
                                           select eg).FirstOrDefault();

            if (employeeGroup == null)
                return null;

            if (loadTransitions)
            {
                if (!employeeGroup.AttestTransition.IsLoaded)
                    employeeGroup.AttestTransition.Load();

                foreach (AttestTransition transition in employeeGroup.AttestTransition)
                {
                    if (!transition.AttestStateFromReference.IsLoaded)
                        transition.AttestStateFromReference.Load();
                }
            }

            if (loadDayType && !employeeGroup.DayType.IsLoaded)
                employeeGroup.DayType.Load();

            if (loadDayType && !employeeGroup.EmployeeGroupDayType.IsLoaded)
                employeeGroup.EmployeeGroupDayType.Load();

            if (loadTimeDeviationCause)
            {
                if (!employeeGroup.TimeDeviationCauseReference.IsLoaded)
                    employeeGroup.TimeDeviationCauseReference.Load();
                if (employeeGroup.TimeDeviationCause != null && !employeeGroup.TimeDeviationCause.TimeCodeReference.IsLoaded)
                    employeeGroup.TimeDeviationCause.TimeCodeReference.Load();
                if (!employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.IsLoaded)
                    employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Load();

                employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.ToList().ForEach(i => i.TimeCodeReference.Load());
            }

            if (loadTimeCode && !employeeGroup.TimeCodeReference.IsLoaded)
                employeeGroup.TimeCodeReference.Load();

            if (loadRuleWorkTimePeriod)
            {
                if (!employeeGroup.EmployeeGroupRuleWorkTimePeriod.IsLoaded)
                    employeeGroup.EmployeeGroupRuleWorkTimePeriod.Load();
                foreach (var period in employeeGroup.EmployeeGroupRuleWorkTimePeriod)
                {
                    if (!period.TimePeriodReference.IsLoaded)
                        period.TimePeriodReference.Load();
                }
            }

            #region ExternalCode

            employeeGroup.ExternalCodes = new List<string>();

            if (loadExternalCode)
            {
                List<CompanyExternalCode> externalCodes = ActorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.EmployeeGroup, employeeGroup.EmployeeGroupId, employeeGroup.ActorCompanyId);
                if (!externalCodes.IsNullOrEmpty())
                {
                    employeeGroup.ExternalCodes.AddRange(externalCodes.Select(s => s.ExternalCode));
                    employeeGroup.ExternalCodesString = StringUtility.GetSeparatedString(externalCodes.Select(s => s.ExternalCode), Constants.Delimiter, true, false);
                }
            }

            #endregion

            return employeeGroup;
        }

        //New For Angular
        public EmployeeGroup GetEmployeeGroupNew(int employeeGroupId, int actorCompanyId, bool loadTimeDeviationCauseTimeCode = false, bool loadDayTypes = false, bool loadTimeAccumulators = false, bool loadTimeDeviationCauseRequests = false, bool loadTimeDeviationCauseAbsenceAnnouncements = false, bool loadLinkedTimeCodes = false, bool loadTimeDeviationCauses = false, bool loadTimeStampRounding = false, bool loadAttestTransitions = false, bool loadRuleWorkTimePeriod = false, bool loadStdAccounts = false, bool loadExternalCode = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return GetEmployeeGroupNew(entities, actorCompanyId, employeeGroupId, loadTimeDeviationCauseTimeCode, loadDayTypes, loadTimeAccumulators, loadTimeDeviationCauseRequests, loadTimeDeviationCauseAbsenceAnnouncements, loadLinkedTimeCodes, loadTimeDeviationCauses, loadTimeStampRounding, loadAttestTransitions, loadRuleWorkTimePeriod, loadStdAccounts, loadExternalCode);
        }
        public EmployeeGroup GetEmployeeGroupNew(CompEntities entities, int actorCompanyId, int employeeGroupId, bool loadTimeDeviationCauseTimeCode, bool loadDayTypes, bool loadTimeAccumulators, bool loadTimeDeviationCauseRequests, bool loadTimeDeviationCauseAbsenceAnnouncements, bool loadLinkedTimeCodes, bool loadTimeDeviationCauses, bool loadTimeStampRounding, bool loadAttestTransitions, bool loadRuleWorkTimePeriod, bool loadStdAccounts, bool loadExternalCode)
        {
            if (employeeGroupId <= 0)
                return null;

            IQueryable<EmployeeGroup> query = entities.EmployeeGroup;


            if (loadTimeDeviationCauseTimeCode)
            {
                query = query.Include("EmployeeGroupTimeDeviationCauseTimeCode");
            }
            if (loadDayTypes)
            {
                query = query.Include("DayType");
                query = query.Include("EmployeeGroupDayType");
            }
            if (loadTimeAccumulators)
            {
                query = query.Include("TimeAccumulatorEmployeeGroupRule");
            }
            if (loadTimeDeviationCauseRequests)
            {
                query = query.Include("EmployeeGroupTimeDeviationCauseRequest");
            }
            if (loadTimeDeviationCauseAbsenceAnnouncements)
            {
                query = query.Include("EmployeeGroupTimeDeviationCauseAbsenceAnnouncement");
            }
            if (loadLinkedTimeCodes)
            {
                query = query.Include("TimeCodes");
            }
            if (loadTimeDeviationCauses)
            {
                query = query.Include("EmployeeGroupTimeDeviationCause");
            }
            if (loadTimeStampRounding)
            {
                query = query.Include("TimeStampRounding");
            }
            if (loadAttestTransitions)
            {
                query = query.Include("AttestTransition.AttestStateFrom");
            }
            if (loadRuleWorkTimePeriod)
            {
                query = query.Include("EmployeeGroupRuleWorkTimePeriod");
            }
            if (loadStdAccounts)
            {
                query = query.Include("EmployeeGroupAccountStd.AccountInternal");
            }

            EmployeeGroup employeeGroup = (from t in query
                                           where t.EmployeeGroupId == employeeGroupId //ID enough as many tables dont have ActorCompanyId? Add this
                                                                                      //&& t.ActorCompanyId == actorCompanyId // If checking actorCompanyId, cant find ids belonging to tables without actorcompanyid
                                           && t.State == (int)SoeEntityState.Active
                                           select t).FirstOrDefault();

            #region ExternalCode

            employeeGroup.ExternalCodes = new List<string>();

            if (loadExternalCode)
            {
                List<CompanyExternalCode> externalCodes = ActorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.EmployeeGroup, employeeGroup.EmployeeGroupId, employeeGroup.ActorCompanyId);
                if (!externalCodes.IsNullOrEmpty())
                {
                    employeeGroup.ExternalCodes.AddRange(externalCodes.Select(s => s.ExternalCode));
                    employeeGroup.ExternalCodesString = StringUtility.GetSeparatedString(externalCodes.Select(s => s.ExternalCode), Constants.Delimiter, true, false);
                }
            }

            #endregion

            #region StdAccounts
            if (loadStdAccounts)
            {

                if (employeeGroup != null)
                {
                    var dims = AccountManager.GetAccountDimsByCompany(
                        entities,
                        actorCompanyId,
                        onlyStandard: false,
                        onlyInternal: true,
                        active: true,
                        loadAccounts: true,
                        loadInternalAccounts: true,
                        loadParentOrCalculateLevels: true
                        )
                    .ToSmallDTOs(true, true, false)
                    .ToList();


                    AccountManager.FilterAccountsOnAccountDims(entities, dims, actorCompanyId, base.UserId, includeParentAccounts: true);


                    var accountIdToDimMap = new Dictionary<int, int>();
                    for (int dimIndex = 0; dimIndex < dims.Count; dimIndex++)
                    {
                        var dim = dims[dimIndex];
                        if (dim.Accounts != null)
                        {
                            foreach (var account in dim.Accounts)
                            {
                                accountIdToDimMap[account.AccountId] = dimIndex + 1;
                            }
                        }
                    }

                    var costAccountStd = employeeGroup.EmployeeGroupAccountStd.FirstOrDefault(e => e.Type == (int)EmployeeGroupAccountType.Cost);

                    if (costAccountStd != null)
                    {

                        foreach (var accountInternal in costAccountStd.AccountInternal)
                        {
                            if (accountIdToDimMap.TryGetValue(accountInternal.AccountId, out int dimValue))
                            {
                                int level = dimValue + 1;

                                switch (level)
                                {
                                    case 1:
                                        employeeGroup.DefaultDim1CostAccountId = accountInternal.AccountId;
                                        break;
                                    case 2:
                                        employeeGroup.DefaultDim2CostAccountId = accountInternal.AccountId;
                                        break;
                                    case 3:
                                        employeeGroup.DefaultDim3CostAccountId = accountInternal.AccountId;
                                        break;
                                    case 4:
                                        employeeGroup.DefaultDim4CostAccountId = accountInternal.AccountId;
                                        break;
                                    case 5:
                                        employeeGroup.DefaultDim5CostAccountId = accountInternal.AccountId;
                                        break;
                                    case 6:
                                        employeeGroup.DefaultDim6CostAccountId = accountInternal.AccountId;
                                        break;
                                }
                            }
                        }
                    }
                    var incomeAccountStd = employeeGroup.EmployeeGroupAccountStd.FirstOrDefault(e => e.Type == (int)EmployeeGroupAccountType.Income);

                    if (incomeAccountStd != null)
                    {
                        foreach (var accountInternal in incomeAccountStd.AccountInternal)
                        {
                            if (accountIdToDimMap.TryGetValue(accountInternal.AccountId, out int dimValue))
                            {
                                int level = dimValue + 1;

                                switch (level)
                                {
                                    case 1:
                                        employeeGroup.DefaultDim1IncomeAccountId = accountInternal.AccountId;
                                        break;
                                    case 2:
                                        employeeGroup.DefaultDim2IncomeAccountId = accountInternal.AccountId;
                                        break;
                                    case 3:
                                        employeeGroup.DefaultDim3IncomeAccountId = accountInternal.AccountId;
                                        break;
                                    case 4:
                                        employeeGroup.DefaultDim4IncomeAccountId = accountInternal.AccountId;
                                        break;
                                    case 5:
                                        employeeGroup.DefaultDim5IncomeAccountId = accountInternal.AccountId;
                                        break;
                                    case 6:
                                        employeeGroup.DefaultDim6IncomeAccountId = accountInternal.AccountId;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return employeeGroup;
        }

        public EmployeeGroup GetFirstEmployeeGroup(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return GetFirstEmployeeGroup(entities, actorCompanyId);
        }

        public EmployeeGroup GetFirstEmployeeGroup(CompEntities entities, int actorCompanyId)
        {
            return (from eg in entities.EmployeeGroup
                    where eg.ActorCompanyId == actorCompanyId
                    orderby eg.EmployeeGroupId
                    select eg).FirstOrDefault();
        }

        public EmployeeGroup GetEmployeeGroupByName(string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return GetEmployeeGroupByName(entities, name, actorCompanyId);
        }

        public EmployeeGroup GetEmployeeGroupByName(CompEntities entities, string name, int actorCompanyId)
        {
            return (from eg in entities.EmployeeGroup
                    where eg.Name == name &&
                    eg.ActorCompanyId == actorCompanyId &&
                    eg.State == (int)SoeEntityState.Active
                    select eg).FirstOrDefault();
        }

        public EmployeeGroup GetEmployeeGroupWithConnectedDeviationCauseRequests(int employeeGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return GetEmployeeGroupWithConnectedDeviationCauseRequests(entities, employeeGroupId);
        }

        public EmployeeGroup GetEmployeeGroupWithConnectedDeviationCauseRequests(CompEntities entities, int employeeGroupId)
        {
            return (from eg in entities.EmployeeGroup
                        .Include("EmployeeGroupTimeDeviationCauseRequest.TimeDeviationCause")
                    where eg.EmployeeGroupId == employeeGroupId
                    select eg).FirstOrDefault();
        }

        public EmployeeGroup GetEmployeeGroupWithConnectedDeviationCauseAbsenceAnnouncements(CompEntities entities, int employeeGroupId)
        {
            return (from eg in entities.EmployeeGroup
                    .Include("EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.TimeDeviationCause")
                    where eg.EmployeeGroupId == employeeGroupId
                    select eg).FirstOrDefault();
        }

        public EmployeeGroup GetEmployeeGroupWithConnectedDeviationCauseMapping(int employeeGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return GetEmployeeGroupWithDeviationCause(entities, employeeGroupId);
        }

        public EmployeeGroup GetEmployeeGroupWithDeviationCause(CompEntities entities, int employeeGroupId)
        {
            return (from eg in entities.EmployeeGroup
                        .Include("EmployeeGroupTimeDeviationCause")
                    where eg.EmployeeGroupId == employeeGroupId
                    select eg).FirstOrDefault();
        }

        public EmployeeGroup GetPrevNextEmployeeGroup(int employeeGroupId, int actorCompanyId, SoeFormMode mode)
        {
            EmployeeGroup employeeGroup = GetEmployeeGroup(employeeGroupId);
            if (employeeGroup != null)
            {
                List<EmployeeGroup> employeeGroups = GetEmployeeGroups(actorCompanyId);

                if (mode == SoeFormMode.Next)
                {
                    employeeGroup = (from eg in employeeGroups
                                     where eg.Name.CompareTo(employeeGroup.Name) > 0 &&
                                     eg.State == (int)SoeEntityState.Active
                                     orderby eg.Name ascending
                                     select eg).FirstOrDefault();
                }
                else
                {
                    employeeGroup = (from eg in employeeGroups
                                     where eg.Name.CompareTo(employeeGroup.Name) < 0 &&
                                     eg.State == (int)SoeEntityState.Active
                                     orderby eg.Name descending
                                     select eg).FirstOrDefault();
                }
            }

            return employeeGroup;
        }

        public EmployeeGroup GetDefaultOrFirstEmployeeGroup(CompEntities entities, int actorCompanyId)
        {
            int employeeGroupId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeDefaultEmployeeGroup, 0, actorCompanyId, 0);
            return GetEmployeeGroup(entities, employeeGroupId) ?? GetFirstEmployeeGroup(entities, actorCompanyId);
        }

        public EmployeeGroup GetEmployeeGroupForEmployee(int employeeId, int actorCompanyId, DateTime date, List<EmployeeGroup> employeeGroups = null, bool getFirstIfNotFound = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employment.NoTracking();
            return GetEmployeeGroupForEmployee(entities, employeeId, actorCompanyId, date, employeeGroups, getFirstIfNotFound);
        }

        public EmployeeGroup GetEmployeeGroupForEmployee(CompEntities entities, int employeeId, int actorCompanyId, DateTime date, List<EmployeeGroup> employeeGroups = null, bool getFirstIfNotFound = false)
        {
            Employee employee = GetEmployee(entities, employeeId, actorCompanyId, loadEmployment: true);
            if (employee == null)
                return null;

            if (employeeGroups == null)
                employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(actorCompanyId));

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, employeeGroups);
            if (employeeGroup == null && getFirstIfNotFound)
                employeeGroup = employee.GetFirstEmployeeGroup(employeeGroups);

            return employeeGroup;
        }

        public Dictionary<DateTime, int> GetEmployeeGroupIdsInRange(Employee employee, DateTime dateFrom, DateTime dateTo)
        {
            Dictionary<DateTime, int> dict = new Dictionary<DateTime, int>();

            DateTime date = dateFrom;
            int prevEmployeeGroupId = 0;
            while (date <= dateTo)
            {
                int employeeGroupId = employee.GetEmployeeGroupId(date);
                if (employeeGroupId != prevEmployeeGroupId)
                    dict.Add(date, employeeGroupId);

                prevEmployeeGroupId = employeeGroupId;
                date = date.AddDays(1);
            }

            return dict;
        }
        public bool EmployeeGroupExists(string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroup.NoTracking();
            return EmployeeGroupExists(entities, name, actorCompanyId);
        }
        public bool EmployeeGroupExists(CompEntities entities, string name, int actorCompanyId)
        {
            return (from eg in entities.EmployeeGroup
                    where eg.Name == name &&
                    eg.ActorCompanyId == actorCompanyId &&
                    eg.State == (int)SoeEntityState.Active
                    select eg).Any();
        }

        public ActionResult IsOkToDeleteEmployeeGroup(int employeeGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return IsOkToDeleteEmployeeGroup(entities, employeeGroupId);
        }

        public ActionResult IsOkToDeleteEmployeeGroup(CompEntities entities, int employeeGroupId)
        {
            ActionResult result = new ActionResult(true);

            //Employment
            if (entities.Employment.Any(i => i.OriginalEmployeeGroupId == employeeGroupId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.EmployeeGroupHasEmployments, GetText(11910, "Anställning"));
            //EmploymentChange
            else if (entities.EmploymentChange.Any(i => i.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmployeeGroupId && (i.FromValue == employeeGroupId.ToString() || i.ToValue == employeeGroupId.ToString())))
                result = new ActionResult((int)ActionResultDelete.EmployeeGroupHasEmploymentChanges, GetText(11910, "Anställning"));
            //EmploymentPost
            else if (entities.EmployeePost.Any(i => i.EmployeeGroupId == employeeGroupId && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.EmployeeGroupHasEmployeePosts, GetText(11910, "Tjänster"));
            //AttestRuleHead
            else if (entities.AttestRuleHead.Any(i => i.EmployeeGroup.Any(eg => eg.EmployeeGroupId == employeeGroupId) && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.EmployeeGroupHasAttestRules, GetText(12117, "Automatattestregler"));
            //TimeAbsenceRuleHead
            else if (entities.TimeAbsenceRuleHead.Any(i => i.TimeAbsenceRuleHeadEmployeeGroup.Any(eg => eg.EmployeeGroupId == employeeGroupId && eg.State == (int)SoeEntityState.Active) && i.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.EmployeeGroupHasTimeAbsenceRules, GetText(12118, "Frånvaroregler"));
            //TimeRule
            else if (entities.TimeRuleRow.Any(i => i.EmployeeGroupId == employeeGroupId && i.TimeRule.State == (int)SoeEntityState.Active))
                result = new ActionResult((int)ActionResultDelete.EmployeeGroupHasTimeRules, GetText(12119, "Tidsregler"));

            if (!result.Success)
                result.ErrorMessage = $"{GetText(5664, "Kontrollera att det inte är kopplat till")} {result.ErrorMessage.ToLower()}. ({GetText(5654, "Felkod:")}{result.ErrorNumber})";

            return result;
        }

        public ActionResult AddEmployeeGroup(EmployeeGroup employeeGroup)
        {
            if (employeeGroup == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

            using (CompEntities entities = new CompEntities())
            {

                if (string.IsNullOrEmpty(employeeGroup.ExternalCodesString))
                {
                    return AddEntityItem(entities, employeeGroup, "EmployeeGroup");
                }
                else
                {
                    AddEntityItem(entities, employeeGroup, "EmployeeGroup");
                    return ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.EmployeeGroup, employeeGroup.EmployeeGroupId, employeeGroup.ExternalCodesString, employeeGroup.ActorCompanyId);
                }
            }
        }

        public ActionResult UpdateEmployeeGroup(EmployeeGroup employeeGroup, bool updateWorkPercentageOnEmployments)
        {
            if (employeeGroup == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

            using (CompEntities entities = new CompEntities())
            {
                EmployeeGroup originalEmployeeGroup = GetEmployeeGroup(entities, employeeGroup.EmployeeGroupId, false, true, true, false);
                if (originalEmployeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

                var result = UpdateEntityItem(entities, originalEmployeeGroup, employeeGroup, "EmployeeGroup");
                if (result.Success && updateWorkPercentageOnEmployments)
                    result = UpdateWorkPercentageOnEmployments(employeeGroup);

                ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.EmployeeGroup, employeeGroup.EmployeeGroupId, employeeGroup.ExternalCodesString, employeeGroup.ActorCompanyId);

                return result;
            }
        }

        public ActionResult DeleteEmployeeGroupNew(int actorCompanyId, int employeeGroupId)
        {
            ActionResult result;

            using (CompEntities entities = new CompEntities())
            {
                result = IsOkToDeleteEmployeeGroup(entities, employeeGroupId);
                if (!result.Success)
                    return result;
                EmployeeGroup employeeGroup = GetEmployeeGroupNew(entities, actorCompanyId, employeeGroupId, true, true, true, true, true, true, true, true, true, true, true, true);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, String.Format(GetText(5039, "Tidavtal hittades inte")));

                result = ChangeEntityState(entities, employeeGroup, SoeEntityState.Deleted, true);
            }

            return result;
        }

        public ActionResult DeleteEmployeeGroup(EmployeeGroup employeeGroup)
        {
            if (employeeGroup == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "EmployeeGroup");

            ActionResult result;

            using (CompEntities entities = new CompEntities())
            {
                result = IsOkToDeleteEmployeeGroup(entities, employeeGroup.EmployeeGroupId);
                if (!result.Success)
                    return result;

                EmployeeGroup originalEmployeeGroup = GetEmployeeGroup(entities, employeeGroup.EmployeeGroupId, false, true, true, false);
                if (originalEmployeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

                result = ChangeEntityState(entities, originalEmployeeGroup, SoeEntityState.Deleted, true);
            }

            return result;
        }

        #region SaveEmployeeGroup

        public ActionResult SaveEmployeeGroup(EmployeeGroupDTO employeeGroupInput, int actorCompanyId)
        {
            if (employeeGroupInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, String.Format(GetText(5039, "Tidavtal hittades inte")));

            ActionResult result = null;

            int employeeGroupId = employeeGroupInput.EmployeeGroupId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        bool changeAffectTerminal = false;

                        #region Validation

                        #region Name
                        if (employeeGroupInput.AlwaysDiscardBreakEvaluation && (employeeGroupInput.AutogenBreakOnStamping || employeeGroupInput.MergeScheduleBreaksOnDay))
                        {
                            return new ActionResult((int)ActionResultSave.InvalidBreakSettings, String.Format(GetText(5756, "Ogiltig uppsättning av rasthantering för stämplingar")));
                        }
                        #endregion

                        #region TimeDeviationCause
                        if (employeeGroupInput.TimeDeviationCauseId == null || employeeGroupInput.TimeDeviationCauseId == 0)
                        {
                            return new ActionResult((int)ActionResultSave.StandardDeviationCauseMissing, String.Format(GetText(5522, "Du måste ange standardorsak")));
                        }

                        if (!employeeGroupInput.TimeDeviationCauses.Any(td => td.TimeDeviationCauseId == (int)employeeGroupInput.TimeDeviationCauseId))
                        {
                            return new ActionResult((int)ActionResultSave.StandardDeviationCauseNotInEmployeeGroup, String.Format(GetText(5524, "Standardorsak är inte kopplad till tidavtalet")));
                        }

                        #endregion

                        #endregion

                        #region EmployeeGroup
                        EmployeeGroup employeeGroup = GetEmployeeGroupNew(entities, actorCompanyId, employeeGroupId, true, true, true, true, true, true, true, true, true, true, true, true);

                        if (employeeGroup == null)
                        {
                            #region Add
                            employeeGroup = new EmployeeGroup()
                            {
                                ActorCompanyId = actorCompanyId,
                            };

                            #region Validate Name
                            if (EmployeeGroupExists(entities, employeeGroupInput.Name, actorCompanyId))
                            {
                                return new ActionResult((int)ActionResultSave.EmployeeGroupDuplicate, String.Format(GetText(5044, "Tidavtal finns redan")));
                            }
                            #endregion

                            SetCreatedProperties(employeeGroup);
                            entities.EmployeeGroup.AddObject(employeeGroup);
                            #endregion
                        }
                        else
                        {
                            #region Update

                            #region Validate Name
                            if (employeeGroupInput.Name != employeeGroup.Name && EmployeeGroupExists(entities, employeeGroupInput.Name, actorCompanyId))
                            {
                                return new ActionResult((int)ActionResultSave.EmployeeGroupDuplicate, String.Format(GetText(5044, "Tidavtal finns redan")));
                            }
                            #endregion

                            SetModifiedProperties(employeeGroup);
                            #endregion
                        }

                        #region Set fields

                        // If ruleWorkTimeWeek changed then update workPercentageOnEmployees
                        bool updateWorkPercentageOnEmployees = employeeGroup.RuleWorkTimeWeek != employeeGroupInput.RuleWorkTimeWeek;

                        employeeGroup.TimeDeviationCauseId = employeeGroupInput.TimeDeviationCauseId;
                        employeeGroup.TimeCodeId = employeeGroupInput.TimeCodeId > 0 ? employeeGroupInput.TimeCodeId : (int?)null;
                        employeeGroup.Name = employeeGroupInput.Name;
                        employeeGroup.DeviationAxelStartHours = employeeGroupInput.DeviationAxelStartHours;
                        employeeGroup.DeviationAxelStopHours = employeeGroupInput.DeviationAxelStopHours;
                        employeeGroup.PayrollProductAccountingPrio = employeeGroupInput.PayrollProductAccountingPrio;
                        employeeGroup.InvoiceProductAccountingPrio = employeeGroupInput.InvoiceProductAccountingPrio;
                        employeeGroup.AutogenTimeblocks = employeeGroupInput.TimeReportType == (int)TermGroup_TimeReportType.Deviation;
                        employeeGroup.AutogenBreakOnStamping = employeeGroupInput.AutogenBreakOnStamping;
                        employeeGroup.AlwaysDiscardBreakEvaluation = employeeGroupInput.AlwaysDiscardBreakEvaluation;
                        employeeGroup.MergeScheduleBreaksOnDay = employeeGroupInput.MergeScheduleBreaksOnDay;
                        employeeGroup.BreakDayMinutesAfterMidnight = employeeGroupInput.BreakDayMinutesAfterMidnight;
                        employeeGroup.KeepStampsTogetherWithinMinutes = employeeGroupInput.KeepStampsTogetherWithinMinutes;
                        employeeGroup.RuleWorkTimeWeek = employeeGroupInput.RuleWorkTimeWeek;
                        employeeGroup.RuleWorkTimeYear = employeeGroupInput.RuleWorkTimeYear;
                        employeeGroup.RuleRestTimeDay = employeeGroupInput.RuleRestTimeDay;
                        employeeGroup.RuleRestTimeWeek = employeeGroupInput.RuleRestTimeWeek;
                        employeeGroup.MaxScheduleTimeFullTime = employeeGroupInput.MaxScheduleTimeFullTime;
                        employeeGroup.MinScheduleTimeFullTime = employeeGroupInput.MinScheduleTimeFullTime;
                        employeeGroup.MaxScheduleTimePartTime = employeeGroupInput.MaxScheduleTimePartTime;
                        employeeGroup.MinScheduleTimePartTime = employeeGroupInput.MinScheduleTimePartTime;
                        employeeGroup.MaxScheduleTimeWithoutBreaks = employeeGroupInput.MaxScheduleTimeWithoutBreaks;
                        employeeGroup.RuleWorkTimeDayMinimum = employeeGroupInput.RuleWorkTimeDayMinimum;
                        employeeGroup.RuleWorkTimeDayMaximumWorkDay = employeeGroupInput.RuleWorkTimeDayMaximumWorkDay;
                        employeeGroup.RuleWorkTimeDayMaximumWeekend = employeeGroupInput.RuleWorkTimeDayMaximumWeekend;
                        employeeGroup.QualifyingDayCalculationRule = (int)employeeGroupInput.QualifyingDayCalculationRule;
                        employeeGroup.TimeWorkReductionCalculationRule = (int)employeeGroupInput.TimeWorkReductionCalculationRule;
                        employeeGroup.QualifyingDayCalculationRuleLimitFirstDay = employeeGroupInput.QualifyingDayCalculationRuleLimitFirstDay;
                        employeeGroup.ExtraShiftAsDefault = employeeGroupInput.ExtraShiftAsDefault;
                        employeeGroup.AlsoAttestAdditionsFromTime = employeeGroupInput.AlsoAttestAdditionsFromTime;
                        employeeGroup.BreakRoundingUp = employeeGroupInput.BreakRoundingUp;
                        employeeGroup.BreakRoundingDown = employeeGroupInput.BreakRoundingDown;
                        employeeGroup.RuleRestDayIncludePresence = employeeGroupInput.RuleRestDayIncludePresence;
                        employeeGroup.RuleRestWeekIncludePresence = employeeGroupInput.RuleRestWeekIncludePresence;
                        employeeGroup.RuleScheduleFreeWeekendsMinimumYear = employeeGroupInput.RuleScheduleFreeWeekendsMinimumYear;
                        employeeGroup.RuleScheduledDaysMaximumWeek = employeeGroupInput.RuleScheduledDaysMaximumWeek;
                        employeeGroup.RuleRestTimeWeekStartTime = employeeGroupInput.RuleRestTimeWeekStartTime;
                        employeeGroup.RuleRestTimeDayStartTime = employeeGroupInput.RuleRestTimeDayStartTime;
                        employeeGroup.RuleRestTimeWeekStartDayNumber = employeeGroupInput.RuleRestTimeWeekStartDayNumber;
                        employeeGroup.NotifyChangeOfDeviations = employeeGroupInput.NotifyChangeOfDeviations;
                        employeeGroup.CandidateForOvertimeOnZeroDayExcluded = employeeGroupInput.CandidateForOvertimeOnZeroDayExcluded;
                        employeeGroup.AutoGenTimeAndBreakForProject = employeeGroupInput.AutoGenTimeAndBreakForProject;
                        employeeGroup.ReminderAttestStateId = employeeGroupInput.ReminderAttestStateId > 0
                            ? employeeGroupInput.ReminderAttestStateId
                            : (int?)null;
                        employeeGroup.ReminderNoOfDays = employeeGroupInput.ReminderNoOfDays > 0
                            ? employeeGroupInput.ReminderNoOfDays
                            : (int?)null;
                        employeeGroup.ReminderPeriodType = employeeGroupInput.ReminderPeriodType > 0
                            ? employeeGroupInput.ReminderPeriodType
                            : (int?)null;
                        employeeGroup.SwapShiftToShorterText = employeeGroupInput.SwapShiftToShorterText;
                        employeeGroup.SwapShiftToLongerText = employeeGroupInput.SwapShiftToLongerText;
                        employeeGroup.State = (int)employeeGroupInput.State;
                        //Extensions
                        employeeGroup.TimeReportType = employeeGroupInput.TimeReportType;


                        #endregion

                        #endregion

                        #region EmployeeGroupTimeDeviationCauseTimeCode

                        if (employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode != null)
                        {
                            SetEmployeeGroupTimeDeviationCauseTimeCode(entities, employeeGroup, employeeGroupInput.EmployeeGroupTimeDeviationCauseTimeCode);
                        }
                        #endregion

                        #region EmployeeGroupDayTypeHoliday

                        if (employeeGroup.EmployeeGroupDayType != null)
                        {
                            SetEmployeeGroupDayTypeHoliday(entities, employeeGroup, employeeGroupInput.EmployeeGroupDayType);
                        }

                        #endregion

                        #region EmployeeGroupAttestTransition

                        if (employeeGroup.AttestTransition != null)
                        {
                            UpdateEmployeeGroupAttestTransition(entities, employeeGroup, employeeGroupInput.AttestTransition);
                        }

                        #endregion

                        #region EmployeeGroupTimeDeviationCauses

                        if (employeeGroup.EmployeeGroupTimeDeviationCause != null)
                        {
                            changeAffectTerminal = SetEmployeeGroupTimeDeviationCauses(entities, employeeGroup, employeeGroupInput.TimeDeviationCauses, actorCompanyId);
                        }

                        #endregion

                        #region TimeStampRounding
                        var stampRounding = employeeGroup.TimeStampRounding.FirstOrDefault();
                        if (stampRounding == null)
                        {
                            #region Add
                            stampRounding = new TimeStampRounding();
                            employeeGroup.TimeStampRounding.Add(stampRounding);
                            SetCreatedProperties(stampRounding);
                            #endregion
                        }
                        else
                        {
                            // Update
                            SetModifiedProperties(stampRounding);
                        }

                        stampRounding.RoundInNeg = employeeGroupInput.RoundInNeg;
                        stampRounding.RoundOutNeg = employeeGroupInput.RoundOutNeg;
                        stampRounding.RoundInPos = employeeGroupInput.RoundInPos;
                        stampRounding.RoundOutPos = employeeGroupInput.RoundOutPos;
                        #endregion

                        #region EmployeeGroupDayTypeWorking

                        if (employeeGroup.DayType != null)
                        {
                            SetEmployeeGroupDayTypeWorking(entities, employeeGroup, employeeGroupInput.DayTypeIds, actorCompanyId);
                        }

                        #endregion

                        #region EmployeeGroupTimeDeviationCauseRequest

                        if (employeeGroup.EmployeeGroupTimeDeviationCauseRequest != null)
                        {
                            SetEmployeeGroupTimeDeviationCauseRequest(entities, employeeGroup, employeeGroupInput.TimeDeviationCauseRequestIds);
                        }

                        #endregion

                        #region EmployeeGroupTimeDeviationCauseAbsenceAnnouncement

                        if (employeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement != null)
                        {
                            SetEmployeeGroupTimeDeviationCauseAbsenceAnnouncement(entities, employeeGroup, employeeGroupInput.TimeDeviationCauseAbsenceAnnouncementIds);
                        }

                        #endregion

                        #region EmployeeGroupTimeCodes

                        if (employeeGroup.TimeCodes != null)
                        {
                            SetEmployeeGroupTimeCodes(entities, employeeGroup, employeeGroupInput.TimeCodeIds, actorCompanyId);
                        }

                        #endregion

                        #region EmployeeGroupRuleWorkTimePeriods

                        if (employeeGroup.EmployeeGroupRuleWorkTimePeriod != null)
                        {
                            SetEmployeeGroupRuleWorkTimePeriod(entities, employeeGroup, employeeGroupInput.RuleWorkTimePeriods, actorCompanyId);
                        }

                        #endregion

                        #region TimeAccumulatorEmployeeGroupRule

                        if (employeeGroup.TimeAccumulatorEmployeeGroupRule != null)
                        {
                            SetTimeAccumulatorEmployeeGroupRule(entities, employeeGroup, employeeGroupInput.TimeAccumulatorEmployeeGroupRules, actorCompanyId);
                        }

                        #endregion

                        #region EmployeeGroupAccount

                        if (employeeGroup.EmployeeGroupAccountStd != null)
                        {
                            SetEmployeeGroupAccountInternals(entities, employeeGroup, employeeGroupInput, actorCompanyId);
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);

                        #region CompanyExternalCode
                        ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.EmployeeGroup, employeeGroup.EmployeeGroupId, employeeGroupInput.ExternalCodesString, employeeGroup.ActorCompanyId);
                        #endregion

                        if (result.Success)
                        {
                            #region WebPubSub
                            if (changeAffectTerminal)
                                SendWebPubSubMessage(entities, employeeGroup, WebPubSubMessageAction.Update);
                            #endregion

                            if (updateWorkPercentageOnEmployees)
                                result = UpdateWorkPercentageOnEmployments(entities, employeeGroup);

                            transaction.Complete();

                            employeeGroupId = employeeGroup.EmployeeGroupId;

                            base.FlushFromCache<List<EmployeeGroup>>(CacheConfig.Company(actorCompanyId), BusinessCacheType.EmployeeGroups);
                        }
                    }

                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                }
                finally
                {
                    if (result != null && result.Success)
                    {
                        result.IntegerValue = employeeGroupId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
                return result;

            }
        }

        #region EmployeeGroupTimeDeviationCauseTimeCode
        private void SetEmployeeGroupTimeDeviationCauseTimeCode(CompEntities entities, EmployeeGroup employeeGroup, List<EmployeeGroupTimeDeviationCauseTimeCodeDTO> employeeGroupTimeDeviationCauseTimeCodeInputs)
        {
            if (employeeGroup == null || employeeGroupTimeDeviationCauseTimeCodeInputs == null)
                return;

            // Delete all current EmployeeGroupTimeDeviationCauseTimeCode
            employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Clear();
            #region Add
            foreach (EmployeeGroupTimeDeviationCauseTimeCodeDTO timeDeviationCauseTimeCodeInput in employeeGroupTimeDeviationCauseTimeCodeInputs)
            {
                // Skip duplicates
                if (employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Any(e => e.TimeDeviationCauseId == timeDeviationCauseTimeCodeInput.TimeDeviationCauseId && e.TimeCodeId == timeDeviationCauseTimeCodeInput.TimeCodeId))
                {
                    continue;
                }
                EmployeeGroupTimeDeviationCauseTimeCode timeDeviationCauseTimeCode = new EmployeeGroupTimeDeviationCauseTimeCode()
                {
                    TimeDeviationCauseId = timeDeviationCauseTimeCodeInput.TimeDeviationCauseId,
                    TimeCodeId = timeDeviationCauseTimeCodeInput.TimeCodeId
                };
                employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Add(timeDeviationCauseTimeCode);
            }
            #endregion
        }
        #endregion

        #region EmployeeGroupDayTypeHoliday
        private void SetEmployeeGroupDayTypeHoliday(CompEntities entities, EmployeeGroup employeeGroup, List<EmployeeGroupDayTypeDTO> employeeGroupDayTypeInputs)
        {
            if (employeeGroup == null || employeeGroupDayTypeInputs == null)
                return;

            foreach (EmployeeGroupDayType employeeGroupDayType in employeeGroup.EmployeeGroupDayType.ToList())
            {
                EmployeeGroupDayTypeDTO employeeGroupDayTypeInput = employeeGroupDayTypeInputs.FirstOrDefault(i => i.EmployeeGroupDayTypeId == employeeGroupDayType.EmployeeGroupDayTypeId);
                if (employeeGroupDayTypeInput != null && employeeGroupDayTypeInput.DayTypeId > 0)
                {
                    #region Update 
                    employeeGroupDayType.IsHolidaySalary = employeeGroupDayTypeInput.IsHolidaySalary; //Always true from Frontend
                    employeeGroupDayTypeInputs.Remove(employeeGroupDayTypeInput);
                    #endregion
                }
                else
                {
                    #region Delete
                    employeeGroupDayType.State = (int)SoeEntityState.Deleted;
                    #endregion
                }
                SetModifiedProperties(employeeGroupDayType);
            }
            #region Add
            foreach (EmployeeGroupDayTypeDTO employeeGroupDayTypeInput in employeeGroupDayTypeInputs)
            {
                // Skip empty
                if (employeeGroupDayTypeInput.DayTypeId == 0)
                    continue;
                // Skip duplicates
                if (employeeGroup.EmployeeGroupDayType.Any(d => d.DayTypeId == employeeGroupDayTypeInput.DayTypeId && d.State == (int)SoeEntityState.Active))
                {
                    continue;
                }
                EmployeeGroupDayType employeeGroupDayType = new EmployeeGroupDayType()
                {
                    DayTypeId = employeeGroupDayTypeInput.DayTypeId,
                    IsHolidaySalary = employeeGroupDayTypeInput.IsHolidaySalary,
                };
                employeeGroup.EmployeeGroupDayType.Add(employeeGroupDayType);
                SetCreatedProperties(employeeGroupDayType);
            }
            #endregion
        }
        #endregion

        #region EmployeeGroupAttestTransition
        private void UpdateEmployeeGroupAttestTransition(CompEntities entities, EmployeeGroup employeeGroup, List<EmployeeGroupAttestTransitionDTO> employeeGroupAttestTransitionInputs)
        {
            if (employeeGroup == null || employeeGroupAttestTransitionInputs == null)
                return;

            employeeGroup.AttestTransition.Clear();

            foreach (EmployeeGroupAttestTransitionDTO attestTransitionInput in employeeGroupAttestTransitionInputs)
            {
                // Skip id = 0
                if (attestTransitionInput.AttestTransitionId == 0)
                {
                    continue;
                }
                // Prevent duplicates
                if (employeeGroup.AttestTransition.Any(a => a.AttestTransitionId == attestTransitionInput.AttestTransitionId))
                    continue;
                AttestTransition attestTransition = AttestManager.GetAttestTransition(entities, attestTransitionInput.AttestTransitionId);
                if (attestTransition != null)
                {
                    employeeGroup.AttestTransition.Add(attestTransition);
                }
            }
        }
        #endregion

        #region EmployeeGroupTimeDeviationCauses
        private bool SetEmployeeGroupTimeDeviationCauses(CompEntities entities, EmployeeGroup employeeGroup, List<EmployeeGroupTimeDeviationCauseDTO> employeeGroupTimeDeviationCausesInputs, int actorCompanyId)
        {
            bool changeAffectTerminal = false;

            if (employeeGroup == null || employeeGroupTimeDeviationCausesInputs == null)
                return false;

            foreach (EmployeeGroupTimeDeviationCause timeDeviationCause in employeeGroup.EmployeeGroupTimeDeviationCause.ToList())
            {
                EmployeeGroupTimeDeviationCauseDTO timeDeviationCauseInput = employeeGroupTimeDeviationCausesInputs.FirstOrDefault(i => i.EmployeeGroupTimeDeviationCauseId == timeDeviationCause.EmployeeGroupTimeDeviationCauseId);
                if (timeDeviationCauseInput != null)
                {
                    #region Update
                    if (!employeeGroup.AutogenTimeblocks)
                    {
                        if ((timeDeviationCause.UseInTimeTerminal != timeDeviationCauseInput.UseInTimeTerminal) || (timeDeviationCause.TimeDeviationCauseId != timeDeviationCauseInput.TimeDeviationCauseId))
                        {
                            changeAffectTerminal = true;
                        }
                    }
                    timeDeviationCause.TimeDeviationCauseId = timeDeviationCauseInput.TimeDeviationCauseId;
                    timeDeviationCause.UseInTimeTerminal = timeDeviationCauseInput.UseInTimeTerminal;
                    employeeGroupTimeDeviationCausesInputs.Remove(timeDeviationCauseInput);
                    #endregion
                }
                else
                {
                    #region Delete
                    if (!employeeGroup.AutogenTimeblocks && timeDeviationCause.UseInTimeTerminal)
                    {
                        changeAffectTerminal = true;
                    }
                    timeDeviationCause.State = (int)SoeEntityState.Deleted;
                    #endregion
                }
                SetModifiedProperties(timeDeviationCause);
            }
            #region Add
            foreach (EmployeeGroupTimeDeviationCauseDTO timeDeviationCauseInput in employeeGroupTimeDeviationCausesInputs)
            {
                // Skip id 0
                if (timeDeviationCauseInput.TimeDeviationCauseId == 0)
                    continue;
                // Skip duplicates
                if (employeeGroup.EmployeeGroupTimeDeviationCause.Any(t => t.TimeDeviationCauseId == timeDeviationCauseInput.TimeDeviationCauseId && t.State == (int)SoeEntityState.Active))
                {
                    continue;
                }
                EmployeeGroupTimeDeviationCause timeDeviationCause = new EmployeeGroupTimeDeviationCause()
                {
                    TimeDeviationCauseId = timeDeviationCauseInput.TimeDeviationCauseId,
                    UseInTimeTerminal = timeDeviationCauseInput.UseInTimeTerminal,
                    ActorCompanyId = actorCompanyId
                };
                employeeGroup.EmployeeGroupTimeDeviationCause.Add(timeDeviationCause);
                SetCreatedProperties(timeDeviationCause);
                if (!employeeGroup.AutogenTimeblocks && timeDeviationCauseInput.UseInTimeTerminal)
                {
                    changeAffectTerminal = true;
                }
            }
            #endregion
            return changeAffectTerminal;
        }
        #endregion

        #region EmployeeGroupDayTypeWorking
        private void SetEmployeeGroupDayTypeWorking(CompEntities entities, EmployeeGroup employeeGroup, List<int> dayTypeIdsInput, int actorCompanyId)
        {
            if (employeeGroup == null || dayTypeIdsInput == null)
                return;

            employeeGroup.DayType.Clear(); // This will delete from DayTypeEmployeeGroupMapping

            foreach (int dayTypeId in dayTypeIdsInput)
            {
                // Skip duplicates
                if (employeeGroup.DayType.Any(d => d.DayTypeId == dayTypeId))
                {
                    continue;
                }
                DayType dayType = CalendarManager.GetDayType(entities, dayTypeId, actorCompanyId);
                if (dayType != null)
                {
                    employeeGroup.DayType.Add(dayType);
                }
            }
        }
        #endregion

        #region EmployeeGroupTimeDeviationCauseRequest
        private void SetEmployeeGroupTimeDeviationCauseRequest(CompEntities entities, EmployeeGroup employeeGroup, List<int> timeDeviationCauseRequestIdsInputs)
        {
            if (employeeGroup == null || timeDeviationCauseRequestIdsInputs == null)
                return;

            var timeDeviationCauseRequests = employeeGroup.EmployeeGroupTimeDeviationCauseRequest.ToList();

            foreach (var timeDeviationCauseRequest in timeDeviationCauseRequests)
            {
                if (timeDeviationCauseRequestIdsInputs.Contains(timeDeviationCauseRequest.TimeDeviationCauseId))
                {
                    #region Update
                    timeDeviationCauseRequestIdsInputs.Remove(timeDeviationCauseRequest.TimeDeviationCauseId);
                    #endregion
                }
                else
                {
                    #region Delete
                    employeeGroup.EmployeeGroupTimeDeviationCauseRequest.Remove(timeDeviationCauseRequest);
                    entities.DeleteObject(timeDeviationCauseRequest);
                    #endregion
                }
            }

            #region Add
            foreach (int timeDeviationCauseRequestIdsInput in timeDeviationCauseRequestIdsInputs)
            {
                // Skip id 0
                if (timeDeviationCauseRequestIdsInput == 0) continue;
                // Skip duplicates
                if (employeeGroup.EmployeeGroupTimeDeviationCauseRequest.Any(t => t.TimeDeviationCauseId == timeDeviationCauseRequestIdsInput)) continue;
                var timeDeviationCauseRequest = new EmployeeGroupTimeDeviationCauseRequest()
                {
                    TimeDeviationCauseId = timeDeviationCauseRequestIdsInput,
                };
                employeeGroup.EmployeeGroupTimeDeviationCauseRequest.Add(timeDeviationCauseRequest);
            }
            #endregion
        }
        #endregion

        #region EmployeeGroupTimeDeviationCauseAbsenceAnnouncement
        private void SetEmployeeGroupTimeDeviationCauseAbsenceAnnouncement(CompEntities entities, EmployeeGroup employeeGroup, List<int> timeDeviationCauseAbsenceAnnouncementIdsInputs)
        {
            if (employeeGroup == null || timeDeviationCauseAbsenceAnnouncementIdsInputs == null)
                return;

            var timeDeviationCauseAbsenceAnnouncements = employeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.ToList();

            foreach (var timeDeviationCauseAbsenceAnnouncement in timeDeviationCauseAbsenceAnnouncements)
            {
                if (timeDeviationCauseAbsenceAnnouncementIdsInputs.Contains(timeDeviationCauseAbsenceAnnouncement.TimeDeviationCauseId))
                {
                    #region Update
                    timeDeviationCauseAbsenceAnnouncementIdsInputs.Remove(timeDeviationCauseAbsenceAnnouncement.TimeDeviationCauseId);
                    #endregion
                }
                else
                {
                    #region Delete
                    employeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Remove(timeDeviationCauseAbsenceAnnouncement);
                    entities.DeleteObject(timeDeviationCauseAbsenceAnnouncement);
                    #endregion
                }
            }

            #region Add
            foreach (int timeDeviationCauseAbsenceAnnouncementIdsInput in timeDeviationCauseAbsenceAnnouncementIdsInputs)
            {
                // Skip id 0
                if (timeDeviationCauseAbsenceAnnouncementIdsInput == 0) continue;
                // Skip duplicates
                if (employeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Any(t => t.TimeDeviationCauseId == timeDeviationCauseAbsenceAnnouncementIdsInput)) continue;
                EmployeeGroupTimeDeviationCauseAbsenceAnnouncement timeDeviationCauseAbsenceAnnouncement = new EmployeeGroupTimeDeviationCauseAbsenceAnnouncement()
                {
                    TimeDeviationCauseId = timeDeviationCauseAbsenceAnnouncementIdsInput,
                };
                employeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Add(timeDeviationCauseAbsenceAnnouncement);
            }
            #endregion
        }
        #endregion

        #region EmployeeGroupTimeCodes
        private void SetEmployeeGroupTimeCodes(CompEntities entities, EmployeeGroup employeeGroup, List<int> timeCodeIdsInput, int actorCompanyId)
        {
            if (employeeGroup == null || timeCodeIdsInput == null)
                return;

            employeeGroup.TimeCodes.Clear(); // This will delete from DayTypeEmployeeGroupMapping

            foreach (int timeCodeIdInput in timeCodeIdsInput)
            {
                TimeCode timeCode = TimeCodeManager.GetTimeCode<TimeCode>(entities, actorCompanyId, timeCodeIdInput);
                // Skip duplicates
                if (employeeGroup.TimeCodes.Any(t => t.TimeCodeId == timeCodeIdInput)) continue;

                if (timeCode != null)
                {
                    employeeGroup.TimeCodes.Add(timeCode);
                }
            }
        }
        #endregion

        #region EmployeeGroupRuleWorkTimePeriod
        private void SetEmployeeGroupRuleWorkTimePeriod(CompEntities entities, EmployeeGroup employeeGroup, List<EmployeeGroupRuleWorkTimePeriodDTO> employeeGroupRuleWorkTimePeriodInputs, int actorCompanyId)
        {
            if (employeeGroup == null || employeeGroupRuleWorkTimePeriodInputs == null)
                return;

            foreach (EmployeeGroupRuleWorkTimePeriod ruleWorkTimePeriod in employeeGroup.EmployeeGroupRuleWorkTimePeriod.ToList())
            {
                EmployeeGroupRuleWorkTimePeriodDTO ruleWorkTimePeriodInput = employeeGroupRuleWorkTimePeriodInputs.FirstOrDefault(i => i.EmployeeGroupRuleWorkTimePeriodId == ruleWorkTimePeriod.EmployeeGroupRuleWorkTimePeriodId);
                if (ruleWorkTimePeriodInput != null)
                {
                    #region Update
                    ruleWorkTimePeriod.TimePeriodId = ruleWorkTimePeriodInput.TimePeriodId;
                    ruleWorkTimePeriod.RuleWorkTime = ruleWorkTimePeriodInput.RuleWorkTime;
                    employeeGroupRuleWorkTimePeriodInputs.Remove(ruleWorkTimePeriodInput);
                    #endregion
                }
                else
                {
                    #region Delete
                    employeeGroup.EmployeeGroupRuleWorkTimePeriod.Remove(ruleWorkTimePeriod);
                    entities.DeleteObject(ruleWorkTimePeriod);
                    #endregion
                }
            }

            #region Add
            foreach (EmployeeGroupRuleWorkTimePeriodDTO ruleWorkTimePeriodInput in employeeGroupRuleWorkTimePeriodInputs)
            {
                // Empty
                if (ruleWorkTimePeriodInput.TimePeriodId == 0) continue;

                // Prevent duplicates
                if (employeeGroup.EmployeeGroupRuleWorkTimePeriod.Any(i => i.TimePeriodId == ruleWorkTimePeriodInput.TimePeriodId)) continue;

                EmployeeGroupRuleWorkTimePeriod ruleWorkTimePeriod = new EmployeeGroupRuleWorkTimePeriod()
                {
                    TimePeriodId = ruleWorkTimePeriodInput.TimePeriodId,
                    RuleWorkTime = ruleWorkTimePeriodInput.RuleWorkTime,
                };
                employeeGroup.EmployeeGroupRuleWorkTimePeriod.Add(ruleWorkTimePeriod);
            }
            #endregion
        }
        #endregion

        #region TimeAccumulatorEmployeeGroupRule
        private void SetTimeAccumulatorEmployeeGroupRule(CompEntities entities, EmployeeGroup employeeGroup, List<TimeAccumulatorEmployeeGroupRuleDTO> timeAccumulatorEmployeeGroupRuleInputs, int actorCompanyId)
        {
            if (employeeGroup == null || timeAccumulatorEmployeeGroupRuleInputs == null)
                return;

            foreach (TimeAccumulatorEmployeeGroupRule timeAccumulatorEmployeeGroupRule in employeeGroup.TimeAccumulatorEmployeeGroupRule.ToList())
            {
                TimeAccumulatorEmployeeGroupRuleDTO timeAccumulatorEmployeeGroupRuleInput = timeAccumulatorEmployeeGroupRuleInputs.FirstOrDefault(i => i.TimeAccumulatorEmployeeGroupRuleId == timeAccumulatorEmployeeGroupRule.TimeAccumulatorEmployeeGroupRuleId);
                if (timeAccumulatorEmployeeGroupRuleInput != null)
                {
                    #region Update
                    timeAccumulatorEmployeeGroupRule.TimeAccumulatorId = timeAccumulatorEmployeeGroupRuleInput.TimeAccumulatorId;
                    timeAccumulatorEmployeeGroupRule.Type = (int)timeAccumulatorEmployeeGroupRuleInput.Type;
                    timeAccumulatorEmployeeGroupRule.MinMinutes = timeAccumulatorEmployeeGroupRuleInput.MinMinutes;
                    timeAccumulatorEmployeeGroupRule.MinTimeCodeId = timeAccumulatorEmployeeGroupRuleInput.MinTimeCodeId;
                    timeAccumulatorEmployeeGroupRule.MaxMinutes = timeAccumulatorEmployeeGroupRuleInput.MaxMinutes;
                    timeAccumulatorEmployeeGroupRule.MaxTimeCodeId = timeAccumulatorEmployeeGroupRuleInput.MaxTimeCodeId;
                    timeAccumulatorEmployeeGroupRule.ShowOnPayrollSlip = timeAccumulatorEmployeeGroupRuleInput.ShowOnPayrollSlip;
                    timeAccumulatorEmployeeGroupRule.MinMinutesWarning = timeAccumulatorEmployeeGroupRuleInput.MinMinutesWarning;
                    timeAccumulatorEmployeeGroupRule.MaxMinutesWarning = timeAccumulatorEmployeeGroupRuleInput.MaxMinutesWarning;
                    timeAccumulatorEmployeeGroupRule.ScheduledJobHeadId = timeAccumulatorEmployeeGroupRuleInput.ScheduledJobHeadId.HasValue && timeAccumulatorEmployeeGroupRuleInput.ScheduledJobHeadId.Value != 0 ? timeAccumulatorEmployeeGroupRuleInput.ScheduledJobHeadId : (int?)null;

                    timeAccumulatorEmployeeGroupRuleInputs.Remove(timeAccumulatorEmployeeGroupRuleInput);
                    #endregion
                }
                else
                {
                    #region Delete
                    timeAccumulatorEmployeeGroupRule.State = (int)SoeEntityState.Deleted;
                    #endregion
                }
                SetModifiedProperties(timeAccumulatorEmployeeGroupRule);
            }
            #region Add
            foreach (TimeAccumulatorEmployeeGroupRuleDTO timeAccumulatorEmployeeGroupRuleInput in timeAccumulatorEmployeeGroupRuleInputs)
            {
                // Skip empty
                if (timeAccumulatorEmployeeGroupRuleInput.TimeAccumulatorId == 0) continue;
                // Skip duplicates
                if (employeeGroup.TimeAccumulatorEmployeeGroupRule.Any(t => t.TimeAccumulatorId == timeAccumulatorEmployeeGroupRuleInput.TimeAccumulatorId && t.State == (int)SoeEntityState.Active)) continue;
                TimeAccumulatorEmployeeGroupRule timeAccumulatorEmployeeGroupRule = new TimeAccumulatorEmployeeGroupRule()
                {
                    TimeAccumulatorId = timeAccumulatorEmployeeGroupRuleInput.TimeAccumulatorId,
                    Type = (int)timeAccumulatorEmployeeGroupRuleInput.Type,
                    MinMinutes = timeAccumulatorEmployeeGroupRuleInput.MinMinutes,
                    MinTimeCodeId = timeAccumulatorEmployeeGroupRuleInput.MinTimeCodeId,
                    MaxMinutes = timeAccumulatorEmployeeGroupRuleInput.MaxMinutes,
                    MaxTimeCodeId = timeAccumulatorEmployeeGroupRuleInput.MaxTimeCodeId,
                    ShowOnPayrollSlip = timeAccumulatorEmployeeGroupRuleInput.ShowOnPayrollSlip,
                    MinMinutesWarning = timeAccumulatorEmployeeGroupRuleInput.MinMinutesWarning,
                    MaxMinutesWarning = timeAccumulatorEmployeeGroupRuleInput.MaxMinutesWarning,
                    ScheduledJobHeadId = timeAccumulatorEmployeeGroupRuleInput.ScheduledJobHeadId.HasValue && timeAccumulatorEmployeeGroupRuleInput.ScheduledJobHeadId.Value != 0 ? timeAccumulatorEmployeeGroupRuleInput.ScheduledJobHeadId : (int?)null,
                };
                SetCreatedProperties(timeAccumulatorEmployeeGroupRule);
                employeeGroup.TimeAccumulatorEmployeeGroupRule.Add(timeAccumulatorEmployeeGroupRule);
            }
            #endregion
        }
        #endregion

        #region EmployeeGroupAccount
        private void SetEmployeeGroupAccountInternals(CompEntities entities, EmployeeGroup employeeGroup, EmployeeGroupDTO employeeGroupInput, int actorCompanyId)
        {
            if (employeeGroup == null)
                return;

            // Define the dimension-to-account mapping
            var costDimAccountIds = new Dictionary<int, int?>()
            {
                { 1, employeeGroupInput.DefaultDim1CostAccountId },
                { 2, employeeGroupInput.DefaultDim2CostAccountId },
                { 3, employeeGroupInput.DefaultDim3CostAccountId },
                { 4, employeeGroupInput.DefaultDim4CostAccountId },
                { 5, employeeGroupInput.DefaultDim5CostAccountId },
                { 6, employeeGroupInput.DefaultDim6CostAccountId },
            };

            var incomeDimAccountIds = new Dictionary<int, int?>()
            {
                { 1, employeeGroupInput.DefaultDim1IncomeAccountId },
                { 2, employeeGroupInput.DefaultDim2IncomeAccountId },
                { 3, employeeGroupInput.DefaultDim3IncomeAccountId },
                { 4, employeeGroupInput.DefaultDim4IncomeAccountId },
                { 5, employeeGroupInput.DefaultDim5IncomeAccountId },
                { 6, employeeGroupInput.DefaultDim6IncomeAccountId },
            };

            // Create or update Cost
            SetGroupAccountInternalByType(employeeGroup, costDimAccountIds, EmployeeGroupAccountType.Cost, actorCompanyId, entities);

            // Create or update Income
            SetGroupAccountInternalByType(employeeGroup, incomeDimAccountIds, EmployeeGroupAccountType.Income, actorCompanyId, entities);
        }

        private void SetGroupAccountInternalByType(EmployeeGroup employeeGroup, Dictionary<int, int?> dimToAccountIdMap, EmployeeGroupAccountType accountType, int actorCompanyId, CompEntities entities)
        {
            // Try to find the existing EmployeeGroupAccountStd
            var accountStd = employeeGroup.EmployeeGroupAccountStd.FirstOrDefault(e => e.Type == (int)accountType && e.EmployeeGroupId == employeeGroup.EmployeeGroupId);

            if (accountStd == null)
            {
                accountStd = new EmployeeGroupAccountStd
                {
                    Type = (int)accountType,
                    Percent = 100
                };
                employeeGroup.EmployeeGroupAccountStd.Add(accountStd);
            }

            // Clear previous AccountInternal entries
            accountStd.AccountInternal.Clear();

            foreach (var kvp in dimToAccountIdMap)
            {
                int dim = kvp.Key;
                int? accountId = kvp.Value;
                if (accountId != null && accountId.Value > 0)
                {
                    var existingAccountInternal = entities.AccountInternal.FirstOrDefault(ai => ai.AccountId == accountId.Value);

                    if (existingAccountInternal != null)
                    {
                        accountStd.AccountInternal.Add(existingAccountInternal);
                    }
                    else
                    {
                        var newInternal = new AccountInternal
                        {
                            AccountId = accountId.Value
                        };
                        accountStd.AccountInternal.Add(newInternal);
                    }
                }
            }
            // Remove accountStd if has no AccountInternals
            if (!accountStd.AccountInternal.Any())
            {
                employeeGroup.EmployeeGroupAccountStd.Remove(accountStd);
                entities.DeleteObject(accountStd);
            }
        }


        #endregion

        #endregion


        public ActionResult SaveEmployeeGroupDayTypesWorking(Collection<FormIntervalEntryItem> items, int employeeGroupId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                EmployeeGroup employeeGroup = GetEmployeeGroup(entities, employeeGroupId, false, true, true, false);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

                EmployeeGroup originalEmployeeGroup = GetEmployeeGroup(entities, employeeGroupId, false, true, true, false);
                if (originalEmployeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

                if (employeeGroup.DayType == null)
                    employeeGroup.DayType = new EntityCollection<DayType>();

                #endregion

                #region Connect

                foreach (var item in items)
                {
                    int itemId = Convert.ToInt32(item.From);
                    if (!employeeGroup.DayType.Any(i => i.DayTypeId == itemId))
                    {
                        DayType dayType = CalendarManager.GetDayType(entities, Convert.ToInt32(itemId), actorCompanyId);
                        if (dayType != null)
                            employeeGroup.DayType.Add(dayType);
                    }
                }

                #endregion

                #region Disconnect

                List<int> dayTypeIds = employeeGroup.DayType.Select(i => i.DayTypeId).ToList();
                foreach (int dayTypeId in dayTypeIds)
                {
                    if (!items.Any(i => i.From == dayTypeId.ToString()))
                    {
                        DayType dayType = CalendarManager.GetDayType(entities, Convert.ToInt32(dayTypeId), actorCompanyId);
                        if (dayType != null)
                            employeeGroup.DayType.Remove(dayType);
                    }
                }

                #endregion

                return UpdateEntityItem(entities, originalEmployeeGroup, employeeGroup, "EmployeeGroup");
            }
        }

        public ActionResult SaveEmployeeGroupDayTypesHolidaySalary(Collection<FormIntervalEntryItem> items, int employeeGroupId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                EmployeeGroup employeeGroup = GetEmployeeGroup(entities, employeeGroupId, false, true, false, false);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

                List<EmployeeGroupDayType> employeeGroupDaytypesHolidaySalary = employeeGroup.EmployeeGroupDayType.Where(i => i.State == (int)SoeEntityState.Active && i.IsHolidaySalary).ToList();

                #endregion

                #region Add

                foreach (var item in items)
                {
                    int dayTypeId = Convert.ToInt32(item.From);
                    if (dayTypeId == 0)
                        continue;

                    if (!employeeGroupDaytypesHolidaySalary.Any(i => i.DayTypeId == dayTypeId))
                    {
                        EmployeeGroupDayType employeeGroupDaytypeHolidaySalary = CreateEmployeeGroupDayType(entities, employeeGroup, dayTypeId, holidaySalary: true);
                        if (employeeGroupDaytypeHolidaySalary == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroupDayType");
                    }
                }

                #endregion

                #region Update/Remove

                foreach (EmployeeGroupDayType employeeGroupDaytypeHolidaySalary in employeeGroupDaytypesHolidaySalary.Where(i => i.EmployeeGroupDayTypeId > 0))
                {
                    var item = items.FirstOrDefault(i => i.From == employeeGroupDaytypeHolidaySalary.DayTypeId.ToString());
                    if (item == null)
                        ChangeEntityState(employeeGroupDaytypeHolidaySalary, SoeEntityState.Deleted);
                }

                #endregion

                return SaveChanges(entities);

            }
        }

        public ActionResult SaveEmployeeGroupTimeAccumulators(Collection<FormIntervalEntryItem> items, int employeeGroupId, int userId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "User");

                EmployeeGroup employeeGroup = GetEmployeeGroup(entities, employeeGroupId);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

                List<int> timeAccumulatorsToAdd = new List<int>();
                List<int> timeAccumulatorsToRemove = new List<int>();

                List<TimeAccumulatorEmployeeGroupRule> connectedAccumulators = TimeAccumulatorManager.GetTimeAccumulatorEmployeeGroupRules(entities, employeeGroupId, loadOnlyActive: false);

                #endregion

                #region Determine which to connect

                foreach (var accumulator in items)
                {
                    int timeAccumulatorId = Convert.ToInt32(accumulator.From);
                    if (timeAccumulatorId == 0)
                        continue;

                    TimeAccumulatorEmployeeGroupRule rule = connectedAccumulators.FirstOrDefault(i => i.TimeAccumulator.TimeAccumulatorId == timeAccumulatorId);
                    bool accumulatorAlreadyConnectedToGroup = rule != null;
                    if (accumulatorAlreadyConnectedToGroup)
                    {
                        if (rule.State == (int)SoeEntityState.Deleted)
                        {
                            //Activate
                            TimeAccumulatorEmployeeGroupRule originalRule = TimeAccumulatorManager.GetEmployeeGroupAccumulatorSetting(entities, timeAccumulatorId, employeeGroupId, false);
                            if (originalRule != null)
                            {
                                ActionResult result = ChangeEntityState(originalRule, SoeEntityState.Active, user);
                                if (!result.Success)
                                    return result;
                            }
                        }
                    }
                    else
                    {
                        //Add
                        timeAccumulatorsToAdd.Add(timeAccumulatorId);
                    }
                }

                #endregion

                #region Determine which to disconnect

                foreach (TimeAccumulatorEmployeeGroupRule accumulator in connectedAccumulators)
                {
                    if (!items.Any(i => i.From == accumulator.TimeAccumulatorId.ToString()))
                        timeAccumulatorsToRemove.Add(accumulator.TimeAccumulatorId);
                }

                #endregion

                #region Connect

                foreach (int timeAccumulatorId in timeAccumulatorsToAdd)
                {
                    TimeAccumulatorEmployeeGroupRule newRule = new TimeAccumulatorEmployeeGroupRule()
                    {
                        EmployeeGroupId = employeeGroupId,
                        TimeAccumulatorId = timeAccumulatorId,
                    };
                    SetCreatedProperties(newRule);

                    employeeGroup.TimeAccumulatorEmployeeGroupRule.Add(newRule);
                }

                #endregion

                #region Disconnect

                foreach (int accumulatorId in timeAccumulatorsToRemove)
                {
                    TimeAccumulatorEmployeeGroupRule originalRule = TimeAccumulatorManager.GetEmployeeGroupAccumulatorSetting(entities, accumulatorId, employeeGroupId);
                    if (originalRule != null)
                        ChangeEntityState(originalRule, SoeEntityState.Deleted, user);
                }

                #endregion

                return SaveChanges(entities);
            }
        }

        public ActionResult SaveEmployeeGroupTimeDeviationCauses(Collection<FormIntervalEntryItem> items, int employeeGroupId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                EmployeeGroup employeeGroup = GetEmployeeGroupWithDeviationCause(entities, employeeGroupId);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

                List<EmployeeGroupTimeDeviationCause> employeeGroupTimeDeviationCauses = employeeGroup.EmployeeGroupTimeDeviationCause.Where(i => i.State == (int)SoeEntityState.Active).ToList();

                bool changeAffectTerminal = false;

                #endregion

                #region Add

                foreach (var item in items)
                {
                    int timeDeviationCauseId = Convert.ToInt32(item.From);
                    if (timeDeviationCauseId == 0)
                        continue;

                    if (!employeeGroupTimeDeviationCauses.Any(i => i.TimeDeviationCauseId == timeDeviationCauseId))
                    {
                        EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCause = CreateEmployeeGroupTimeDeviationCause(entities, employeeGroup, timeDeviationCauseId, actorCompanyId, useInTimeTerminal: item.Checked);
                        if (employeeGroupTimeDeviationCause == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroupTimeDeviationCause");

                        if (!employeeGroup.AutogenTimeblocks)
                            changeAffectTerminal = true;
                    }
                }

                #endregion

                #region Update/Remove

                foreach (EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCause in employeeGroupTimeDeviationCauses.Where(i => i.EmployeeGroupTimeDeviationCauseId > 0))
                {
                    var item = items.FirstOrDefault(i => i.From == employeeGroupTimeDeviationCause.TimeDeviationCauseId.ToString());
                    if (item != null)
                    {
                        if (employeeGroupTimeDeviationCause.UseInTimeTerminal != item.Checked)
                        {
                            employeeGroupTimeDeviationCause.UseInTimeTerminal = item.Checked;
                            SetModifiedProperties(employeeGroupTimeDeviationCause);

                            if (!employeeGroup.AutogenTimeblocks)
                                changeAffectTerminal = true;
                        }
                    }
                    else
                    {
                        ChangeEntityState(employeeGroupTimeDeviationCause, SoeEntityState.Deleted);

                        if (!employeeGroup.AutogenTimeblocks)
                            changeAffectTerminal = true;
                    }
                }

                #endregion

                ActionResult result = SaveChanges(entities);

                #region WebPubSub

                if (result.Success && changeAffectTerminal)
                    SendWebPubSubMessage(entities, employeeGroup, WebPubSubMessageAction.Update);

                #endregion

                return result;
            }
        }

        public ActionResult SaveEmployeeGroupTimeDeviationCauseRequests(Collection<FormIntervalEntryItem> items, int employeeGroupId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                EmployeeGroup employeeGroup = GetEmployeeGroupWithConnectedDeviationCauseRequests(entities, employeeGroupId);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

                List<int> idsToConnect = new List<int>();
                List<int> idsToDisconnect = new List<int>();

                #endregion

                #region Determine which to connect

                foreach (var item in items)
                {
                    int id = Convert.ToInt32(item.From);
                    if (id == 0)
                        continue;

                    bool isConnected = employeeGroup.EmployeeGroupTimeDeviationCauseRequest.Any(i => i.TimeDeviationCause.TimeDeviationCauseId == id);
                    if (!isConnected)
                        idsToConnect.Add(id);
                }

                #endregion

                #region Determine which to disconnect

                foreach (var mapping in employeeGroup.EmployeeGroupTimeDeviationCauseRequest)
                {
                    if (!items.Any(i => i.From == mapping.TimeDeviationCauseId.ToString()))
                        idsToDisconnect.Add(mapping.TimeDeviationCauseId);
                }

                #endregion

                #region Connect

                foreach (int id in idsToConnect)
                {
                    employeeGroup.EmployeeGroupTimeDeviationCauseRequest.Add(EmployeeGroupTimeDeviationCauseRequest.CreateEmployeeGroupTimeDeviationCauseRequest(employeeGroupId, id));
                }

                #endregion

                #region Disconnect

                foreach (int id in idsToDisconnect)
                {
                    EmployeeGroupTimeDeviationCauseRequest mapping = GetEmployeeGroupTimeDeviationCauseRequest(entities, id, employeeGroupId);
                    if (mapping != null)
                        employeeGroup.EmployeeGroupTimeDeviationCauseRequest.Remove(mapping);
                }

                #endregion

                return SaveChanges(entities);
            }
        }

        public ActionResult SaveEmployeeGroupTimeDeviationCauseAbsenceAnnouncements(Collection<FormIntervalEntryItem> items, int employeeGroupId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                EmployeeGroup employeeGroup = GetEmployeeGroupWithConnectedDeviationCauseAbsenceAnnouncements(entities, employeeGroupId);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

                List<int> idsToConnect = new List<int>();
                List<int> idsToDisconnect = new List<int>();

                #endregion

                #region Determine which to connect

                foreach (var item in items)
                {
                    int id = Convert.ToInt32(item.From);
                    if (id == 0)
                        continue;

                    bool isConnected = employeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Any(i => i.TimeDeviationCause.TimeDeviationCauseId == id);
                    if (!isConnected)
                        idsToConnect.Add(id);
                }

                #endregion

                #region Determine which to disconnect

                foreach (var mapping in employeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement)
                {
                    if (!items.Any(i => i.From == mapping.TimeDeviationCauseId.ToString()))
                        idsToDisconnect.Add(mapping.TimeDeviationCauseId);
                }

                #endregion

                #region Connect

                foreach (int id in idsToConnect)
                {
                    employeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Add(EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.CreateEmployeeGroupTimeDeviationCauseAbsenceAnnouncement(employeeGroupId, id));
                }

                #endregion

                #region Disconnect

                foreach (int id in idsToDisconnect)
                {
                    EmployeeGroupTimeDeviationCauseAbsenceAnnouncement mapping = GetEmployeeGroupTimeDeviationCauseAbsenceAnnouncements(entities, id, employeeGroupId);
                    if (mapping != null)
                        employeeGroup.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Remove(mapping);
                }

                #endregion

                return SaveChanges(entities);
            }
        }

        public ActionResult SaveEmployeeGroupTimeCodeTimeDeviationCauseMappings(Collection<FormIntervalEntryItem> items, int employeeGroupId, int actorCompanyId)
        {
            if (items == null)
                return new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                EmployeeGroup employeeGroup = GetEmployeeGroup(entities, employeeGroupId, true, true, true, false);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

                if (!employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.IsLoaded)
                    employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Load();

                #endregion

                // Delete all current EmployeeGroupTimeDeviationCauseTimeCode
                employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Clear();

                // Add new mapping, from the input collection
                foreach (FormIntervalEntryItem item in items)
                {
                    int timeCodeId = Convert.ToInt32(item.To);
                    int timeDeviationCauseId = Convert.ToInt32(item.From);
                    if (timeCodeId == 0 || timeDeviationCauseId == 0)
                        continue;
                    if (employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Any(i => i.TimeCodeId == timeCodeId && i.TimeDeviationCauseId == timeDeviationCauseId))
                        continue;

                    TimeDeviationCause timeDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(entities, timeDeviationCauseId, actorCompanyId, false);
                    if (timeDeviationCause == null)
                        continue;

                    TimeCode timeCode = TimeCodeManager.GetTimeCode(entities, timeCodeId, actorCompanyId, false);
                    if (timeCode == null)
                        continue;

                    employeeGroup.EmployeeGroupTimeDeviationCauseTimeCode.Add(new EmployeeGroupTimeDeviationCauseTimeCode()
                    {
                        TimeDeviationCause = timeDeviationCause,
                        TimeCode = timeCode,
                    });
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult SaveEmployeeGroupTimeCodes(Collection<FormIntervalEntryItem> items, int employeeGroupId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                EmployeeGroup employeeGroup = GetEmployeeGroup(entities, employeeGroupId);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

                if (!employeeGroup.TimeCodes.IsLoaded)
                    employeeGroup.TimeCodes.Load();

                List<int> idsToConnect = new List<int>();
                List<int> idsToDisconnect = new List<int>();

                #endregion

                #region Determine which to connect

                foreach (var item in items)
                {
                    int id = Convert.ToInt32(item.From);
                    if (id == 0)
                        continue;

                    if (!employeeGroup.TimeCodes.Any(i => i.TimeCodeId == id))
                        idsToConnect.Add(id);
                }

                #endregion

                #region Determine which to disconnect

                foreach (TimeCode timeCode in employeeGroup.TimeCodes)
                {
                    if (!items.Any(i => i.From == timeCode.TimeCodeId.ToString()))
                        idsToDisconnect.Add(timeCode.TimeCodeId);
                }

                #endregion

                #region Connect

                foreach (int id in idsToConnect)
                {
                    TimeCode timeCode = TimeCodeManager.GetTimeCode(entities, id, actorCompanyId, false);
                    if (timeCode != null)
                        employeeGroup.TimeCodes.Add(timeCode);
                }

                #endregion

                #region Disconnect

                foreach (int id in idsToDisconnect)
                {
                    TimeCode timeCode = employeeGroup.TimeCodes.FirstOrDefault(t => t.TimeCodeId == id);
                    if (timeCode != null)
                        employeeGroup.TimeCodes.Remove(timeCode);
                }

                #endregion

                return SaveChanges(entities);
            }
        }

        public ActionResult SaveEmployeeGroupTimeCodeBreaks(Collection<FormIntervalEntryItem> items, int timeCodeBreakId, int? timeCodeBreakGroupId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                TimeCodeBreak timeCodeBreak = TimeCodeManager.GetTimeCodeBreak(entities, timeCodeBreakId, actorCompanyId, false, true);
                if (timeCodeBreak == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeCode");

                List<int> idsToConnect = new List<int>();
                List<int> idsToDisconnect = new List<int>();

                #endregion

                #region Determine which to connect

                foreach (var item in items)
                {
                    int id = Convert.ToInt32(item.From);
                    if (id == 0)
                        continue;

                    if (!timeCodeBreak.EmployeeGroupsForBreak.Any(i => i.EmployeeGroupId == id))
                        idsToConnect.Add(id);
                }

                #endregion

                #region Determine which to disconnect

                foreach (EmployeeGroup employeeGroup in timeCodeBreak.EmployeeGroupsForBreak)
                {
                    if (!items.Any(i => i.From == employeeGroup.EmployeeGroupId.ToString()))
                        idsToDisconnect.Add(employeeGroup.EmployeeGroupId);
                }

                #endregion

                #region Connect

                foreach (int id in idsToConnect)
                {
                    EmployeeGroup employeeGroup = GetEmployeeGroup(entities, id);
                    if (employeeGroup != null && timeCodeBreakGroupId.HasValue)
                    {
                        //Validate that EmployeeGroup is not connected to other TimeCodeBreak with same TimeCodeBreakGroup
                        bool exists = (from tc in entities.TimeCode.OfType<TimeCodeBreak>()
                                       where tc.ActorCompanyId == actorCompanyId &&
                                       tc.TimeCodeId != timeCodeBreakId && //Different TimeCodeBreak
                                       tc.TimeCodeBreakGroupId.HasValue && tc.TimeCodeBreakGroupId.Value == timeCodeBreakGroupId.Value && //Same TimeCodeBreakGroup
                                       tc.EmployeeGroupsForBreak.Any(i => i.EmployeeGroupId == employeeGroup.EmployeeGroupId) //Same EmployeeGroup
                                       select tc).Any();

                        if (exists)
                            return new ActionResult((int)ActionResultSave.TimeCodeBreakEmployeeGroupMappedToOtherBreakWithSameBreakGroup);

                        timeCodeBreak.EmployeeGroupsForBreak.Add(employeeGroup);
                    }
                }

                #endregion

                #region Disconnect

                foreach (int id in idsToDisconnect)
                {
                    EmployeeGroup employeeGroup = timeCodeBreak.EmployeeGroupsForBreak.FirstOrDefault(t => t.EmployeeGroupId == id);
                    if (employeeGroup != null)
                        timeCodeBreak.EmployeeGroupsForBreak.Remove(employeeGroup);
                }

                #endregion

                return SaveChanges(entities);
            }
        }

        public ActionResult SaveEmployeeGroupRuleWorkTimePeriod(Collection<FormIntervalEntryItem> items, int employeeGroupId)
        {
            if (items == null)
                return new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                EmployeeGroup employeeGroup = GetEmployeeGroup(entities, employeeGroupId, false, false, false, false, true);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

                #endregion

                // Delete all current EmployeeGroupRuleWorkTimePeriod
                foreach (EmployeeGroupRuleWorkTimePeriod mapping in employeeGroup.EmployeeGroupRuleWorkTimePeriod.ToList())
                {
                    entities.DeleteObject(mapping);
                }

                // Add new mapping, from the input collection
                foreach (FormIntervalEntryItem item in items)
                {
                    #region Prereq

                    int timePeriodId = Convert.ToInt32(item.LabelType);
                    int ruleWorkTime = Convert.ToInt32(CalendarUtility.GetMinutes(item.From));

                    // Empty
                    if (timePeriodId == 0)
                        continue;

                    // Prevent duplicates
                    if (employeeGroup.EmployeeGroupRuleWorkTimePeriod.Any(i => i.TimePeriodId == timePeriodId))
                        continue;

                    #endregion

                    EmployeeGroupRuleWorkTimePeriod mapping = new EmployeeGroupRuleWorkTimePeriod()
                    {
                        EmployeeGroupId = employeeGroupId,
                        TimePeriodId = timePeriodId,
                        RuleWorkTime = ruleWorkTime
                    };

                    entities.EmployeeGroupRuleWorkTimePeriod.AddObject(mapping);
                }

                ActionResult result = SaveChanges(entities);
                return result;
            }
        }

        #endregion

        #region EmployeeGroupDayType

        public List<EmployeeGroupDayType> GetEmployeeGroupDayTypes(int employeeGroupId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.EmployeeGroupDayType.NoTracking();
            return (from egdt in entitiesReadOnly.EmployeeGroupDayType
                        .Include("DayType")
                    where egdt.EmployeeGroupId == employeeGroupId &&
                    egdt.State == (int)SoeEntityState.Active
                    select egdt).ToList();
        }

        public EmployeeGroupDayType CreateEmployeeGroupDayType(CompEntities entities, EmployeeGroup employeeGroup, int dayTypeId, bool holidaySalary)
        {
            if (employeeGroup == null)
                return null;

            EmployeeGroupDayType employeeGroupDayType = new EmployeeGroupDayType()
            {
                IsHolidaySalary = holidaySalary,

                //Set FK
                EmployeeGroupId = employeeGroup.EmployeeGroupId,
                DayTypeId = dayTypeId,
            };
            SetCreatedProperties(employeeGroupDayType);
            entities.EmployeeGroupDayType.AddObject(employeeGroupDayType);
            employeeGroup.EmployeeGroupDayType.Add(employeeGroupDayType);

            return employeeGroupDayType;
        }

        #endregion

        #region EmployeeGroupTimeDeviationCause

        public List<EmployeeGroupTimeDeviationCause> GetEmployeeGroupTimeDeviationCauses(int employeeGroupId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.EmployeeGroupTimeDeviationCause.NoTracking();
            return (from egtdc in entitiesReadOnly.EmployeeGroupTimeDeviationCause
                        .Include("TimeDeviationCause")
                    where egtdc.EmployeeGroupId == employeeGroupId &&
                    egtdc.ActorCompanyId == actorCompanyId &&
                    egtdc.State == (int)SoeEntityState.Active
                    orderby egtdc.TimeDeviationCause.Name
                    select egtdc).ToList();
        }

        public EmployeeGroupTimeDeviationCause CreateEmployeeGroupTimeDeviationCause(CompEntities entities, EmployeeGroup employeeGroup, int timeDeviationCauseId, int actorCompanyId, bool useInTimeTerminal = true)
        {
            if (employeeGroup == null)
                return null;

            EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCause = new EmployeeGroupTimeDeviationCause()
            {
                UseInTimeTerminal = useInTimeTerminal,

                //Set FK
                EmployeeGroupId = employeeGroup.EmployeeGroupId,
                TimeDeviationCauseId = timeDeviationCauseId,
                ActorCompanyId = actorCompanyId,
            };
            SetCreatedProperties(employeeGroupTimeDeviationCause);
            entities.EmployeeGroupTimeDeviationCause.AddObject(employeeGroupTimeDeviationCause);
            employeeGroup.EmployeeGroupTimeDeviationCause.Add(employeeGroupTimeDeviationCause);

            return employeeGroupTimeDeviationCause;
        }

        #endregion

        #region EmployeeGroupTimeDeviationCauseRequest

        private EmployeeGroupTimeDeviationCauseRequest GetEmployeeGroupTimeDeviationCauseRequest(CompEntities entities, int deviationCauseId, int employeeGroupId)
        {
            return (from r in entities.EmployeeGroupTimeDeviationCauseRequest
                    where r.TimeDeviationCause.TimeDeviationCauseId == deviationCauseId &&
                    r.EmployeeGroupId == employeeGroupId
                    select r).FirstOrDefault();
        }

        #endregion

        #region EmployeeGroupTimeDeviationCauseAbsenceAnnouncement

        private EmployeeGroupTimeDeviationCauseAbsenceAnnouncement GetEmployeeGroupTimeDeviationCauseAbsenceAnnouncements(CompEntities entities, int deviationCauseId, int employeeGroupId)
        {
            return (from r in entities.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement
                    where r.TimeDeviationCause.TimeDeviationCauseId == deviationCauseId &&
                    r.EmployeeGroupId == employeeGroupId
                    select r).FirstOrDefault();
        }

        #endregion

        #region EmployeeGroupAccountStd

        public IEnumerable<EmployeeGroupAccountStd> GetEmployeeGroupAccounts(int employeeGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroupAccountStd.NoTracking();
            return GetEmployeeGroupAccounts(entities, employeeGroupId);
        }

        public IEnumerable<EmployeeGroupAccountStd> GetEmployeeGroupAccounts(CompEntities entities, int employeeGroupId)
        {
            return (from egas in entities.EmployeeGroupAccountStd
                        .Include("AccountStd")
                        .Include("AccountStd.Account")
                        .Include("AccountInternal")
                        .Include("AccountInternal.Account")
                        .Include("AccountInternal.Account.AccountDim")
                    where egas.EmployeeGroupId == employeeGroupId
                    select egas).ToList<EmployeeGroupAccountStd>();
        }

        public EmployeeGroupAccountStd GetEmployeeGroupAccount(int employeeGroupId, EmployeeGroupAccountType employeeGroupAccountType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroupAccountStd.NoTracking();
            return GetEmployeeGroupAccount(entities, employeeGroupId, employeeGroupAccountType);
        }

        public EmployeeGroupAccountStd GetEmployeeGroupAccount(CompEntities entities, int employeeGroupId, EmployeeGroupAccountType employeeGroupAccountType)
        {
            int type = (int)employeeGroupAccountType;
            return (from egas in entities.EmployeeGroupAccountStd
                        .Include("AccountStd")
                        .Include("AccountStd.Account")
                        .Include("AccountInternal")
                        .Include("AccountInternal.Account")
                        .Include("AccountInternal.Account.AccountDim")
                    where egas.EmployeeGroupId == employeeGroupId &&
                    egas.Type == type
                    select egas).FirstOrDefault<EmployeeGroupAccountStd>();
        }

        public EmployeeGroupAccountStd GetEmployeeGroupAccount(int employeeGroupdAccountStdId, bool loadAccounts)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroupAccountStd.NoTracking();
            return GetEmployeeGroupAccount(entities, employeeGroupdAccountStdId, loadAccounts);
        }

        public EmployeeGroupAccountStd GetEmployeeGroupAccount(CompEntities entities, int employeeGroupdAccountStdId, bool loadAccounts)
        {
            if (loadAccounts)
            {
                return (from egas in entities.EmployeeGroupAccountStd
                            .Include("AccountStd")
                            .Include("AccountStd.Account")
                            .Include("AccountInternal")
                            .Include("AccountInternal.Account")
                        where egas.EmployeeGroupdAccountStdId == employeeGroupdAccountStdId
                        select egas).FirstOrDefault();
            }
            else
            {
                return (from egas in entities.EmployeeGroupAccountStd
                        where egas.EmployeeGroupdAccountStdId == employeeGroupdAccountStdId
                        select egas).FirstOrDefault();
            }
        }

        public Dictionary<int, int> GetEmployeeGroupInternalsDict(EmployeeGroupAccountStd employeeGroupAccountStd, IEnumerable<AccountDim> accountDims)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();

            if (employeeGroupAccountStd == null || employeeGroupAccountStd.AccountInternal == null || accountDims == null)
                return dict;

            // Get all AccountInternals for EmployeeGroupAccountStd
            var accountInternals = employeeGroupAccountStd.AccountInternal.ToList<AccountInternal>();

            foreach (AccountDim accountDim in accountDims)
            {
                bool exists = false;

                // Check if the current AccountDim exists for specified AccountInternal
                foreach (AccountInternal accountInternal in accountInternals)
                {
                    if (!accountInternal.AccountReference.IsLoaded)
                        accountInternal.AccountReference.Load();
                    if (!accountInternal.Account.AccountDimReference.IsLoaded)
                        accountInternal.Account.AccountDimReference.Load();

                    if (accountDim.AccountDimId == accountInternal.Account.AccountDimId)
                    {
                        exists = true;
                        dict.Add(accountDim.AccountDimId, accountInternal.AccountId);
                        break;
                    }
                }

                if (!exists)
                    dict.Add(accountDim.AccountDimId, 0);
            }

            return dict;
        }

        public ActionResult AddEmployeeGroupAccountStd(EmployeeGroupAccountStd employeeGroupAccountStd, int employeeGroupId)
        {
            if (employeeGroupAccountStd == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroupAccountStd");

            using (CompEntities entities = new CompEntities())
            {
                //Get EmployeeGroup
                employeeGroupAccountStd.EmployeeGroup = GetEmployeeGroup(entities, employeeGroupId, false, true, true, false);
                if (employeeGroupAccountStd.EmployeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

                return AddEntityItem(entities, employeeGroupAccountStd, "EmployeeGroupAccountStd");
            }
        }

        public ActionResult UpdateEmployeeGroupAccountStd(EmployeeGroupAccountStd employeeGroupAccountStd)
        {
            if (employeeGroupAccountStd == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroupAccountStd");

            using (CompEntities entities = new CompEntities())
            {
                // Get EmployeeGroupAccountStd
                EmployeeGroupAccountStd originalEmployeeGroupAccountStd = GetEmployeeGroupAccount(entities, employeeGroupAccountStd.EmployeeGroupdAccountStdId, false);
                if (originalEmployeeGroupAccountStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroupAccountStd");

                // Check if update is needed
                if (originalEmployeeGroupAccountStd.AccountId != employeeGroupAccountStd.AccountId)
                {
                    originalEmployeeGroupAccountStd.AccountId = employeeGroupAccountStd.AccountId;
                    return SaveEntityItem(entities, originalEmployeeGroupAccountStd);
                }
                else
                    return new ActionResult(true);
            }
        }

        public ActionResult DeleteEmployeeGroupAccountStd(EmployeeGroupAccountStd employeeGroupAccountStd)
        {
            if (employeeGroupAccountStd == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "EmployeeGroupAccountStd");

            using (CompEntities entities = new CompEntities())
            {
                // Get EmployeeGroupAccountStd
                EmployeeGroupAccountStd originalEmployeeGroupAccountStd = GetEmployeeGroupAccount(entities, employeeGroupAccountStd.EmployeeGroupdAccountStdId, false);
                if (originalEmployeeGroupAccountStd == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "EmployeeGroupAccountStd");

                // Make sure AccountInternal is loaded
                if (!originalEmployeeGroupAccountStd.AccountInternal.IsLoaded)
                    originalEmployeeGroupAccountStd.AccountInternal.Load();

                // Clear AccountInternals
                originalEmployeeGroupAccountStd.AccountInternal.Clear();

                return DeleteEntityItem(entities, originalEmployeeGroupAccountStd);
            }
        }

        public ActionResult AddEmployeeGroupAccountInternals(EmployeeGroupAccountStd employeeGroupAccountStd, List<int> accountInternalIds, int actorCompanyId)
        {
            if (employeeGroupAccountStd == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroupAccountStd");

            using (CompEntities entities = new CompEntities())
            {
                // Get EmployeeGroupAccountStd
                EmployeeGroupAccountStd originalEmployeeGroupAccountStd = GetEmployeeGroupAccount(entities, employeeGroupAccountStd.EmployeeGroupdAccountStdId, false);
                if (originalEmployeeGroupAccountStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroupAccountStd");

                // Make sure AccountInternal is loaded
                if (!originalEmployeeGroupAccountStd.AccountInternal.IsLoaded)
                    originalEmployeeGroupAccountStd.AccountInternal.Load();

                // Clear existing
                originalEmployeeGroupAccountStd.AccountInternal.Clear();

                // Add new
                foreach (int accountId in accountInternalIds)
                {
                    // Get AccountInternal
                    AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                    if (accountInternal != null)
                        originalEmployeeGroupAccountStd.AccountInternal.Add(accountInternal);
                }

                return SaveEntityItem(entities, originalEmployeeGroupAccountStd);
            }
        }

        #endregion

        #region Employee

        public IQueryable<Employee> GetEmployeesWithEmploymentLoadingsQuery(CompEntities entities, bool onlyActive = true, bool getHidden = true)
        {
            //Always load all references on Employment (to prevent lazy loadings on changes)
            //Always load ContactPerson
            IQueryable<Employee> query = entities.Employee;
            query = query
                 .Include("ContactPerson")
                 .Include("Employment.EmploymentChangeBatch.EmploymentChange")
                 .Include("Employment.OriginalEmployeeGroup")
                 .Include("Employment.OriginalPayrollGroup")
                 .Include("Employment.OriginalAnnualLeaveGroup");
            query = query.Where(e =>
                (getHidden || !e.Hidden) &&
                (!onlyActive || e.State == (int)SoeEntityState.Active));
            return query;
        }

        #region All Employees

        public List<Employee> GetAllEmployees(int actorCompanyId, bool? active = null, bool loadEmployment = false, bool loadUser = false, bool loadContact = false, bool getHidden = false, bool getVacant = true, bool orderByName = false, bool loadEmployeeVactionSE = false, bool loadEmploymentVacactionGroup = false, bool loadScheduleAndTemplateHead = false, bool includeDeleted = false, bool loadEmploymentPriceType = false, bool loadEmploymentAccounts = false, bool loadEmployeeAccounts = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetAllEmployees(entities, actorCompanyId, active, loadEmployment, loadUser, loadContact, getHidden, getVacant, orderByName, loadEmployeeVactionSE: loadEmployeeVactionSE, loadEmploymentVacactionGroup: loadEmploymentVacactionGroup, loadScheduleAndTemplateHead: loadScheduleAndTemplateHead, includeDeleted: includeDeleted, loadEmploymentPriceType: loadEmploymentPriceType, loadEmploymentAccounts: loadEmploymentAccounts, loadEmployeeAccounts: loadEmployeeAccounts);
        }

        public List<Employee> GetAllEmployees(CompEntities entities, int actorCompanyId, bool? active = null, bool loadEmployment = false, bool loadUser = false, bool loadContact = false, bool getHidden = false, bool getVacant = true, bool orderByName = false, bool loadEmployeeVactionSE = false, bool loadEmploymentVacactionGroup = false, bool loadScheduleAndTemplateHead = false, bool includeDeleted = false, bool loadEmploymentPriceType = false, bool loadEmploymentAccounts = false, bool loadEmployeeAccounts = false)
        {
            // Always load ContactPerson, needed for name
            entities.CommandTimeout = 200;
            IQueryable<Employee> query = entities.Employee.Include("ContactPerson");
            if (loadEmployment)
                query = query.Include("Employment.EmploymentChangeBatch.EmploymentChange")
                              .Include("Employment.OriginalEmployeeGroup")
                              .Include("Employment.OriginalPayrollGroup")
                              .Include("Employment.OriginalAnnualLeaveGroup");
            if (loadEmployment && loadEmploymentPriceType)
                query = query.Include("Employment.EmploymentPriceType.EmploymentPriceTypePeriod");
            if (loadEmploymentVacactionGroup)
                query = query.Include("Employment.EmploymentVacationGroup");
            if (loadUser)
                query = query.Include("User");
            if (loadContact)
                query = query.Include("ContactPerson.Actor.Contact.ContactECom")
                               .Include("ContactPerson.Actor.Contact.ContactAddress.ContactAddressRow");
            if (loadEmployeeVactionSE)
                query = query.Include("EmployeeVacationSE");
            if (loadScheduleAndTemplateHead)
                query = query.Include("EmployeeSchedule.TimeScheduleTemplateHead");
            if (loadEmploymentAccounts)
            {
                query = query.Include("Employment.EmploymentAccountStd");
                query = query.Include("Employment.EmploymentAccountStd.AccountInternal");
            }
            if (loadEmployeeAccounts)
                query = query.Include("EmployeeAccount");

            List<Employee> employees = null;
            if (!includeDeleted)
            {
                query = (from e in query
                         where e.ActorCompanyId == actorCompanyId &&
                         (getHidden || !e.Hidden) &&
                         e.State != (int)SoeEntityState.Deleted
                         select e);

                if (active == true)
                    query = query.Where(i => i.State == (int)SoeEntityState.Active);
                else if (active == false)
                    query = query.Where(i => i.State == (int)SoeEntityState.Inactive);

                if (!getVacant)
                    query = query.Where(i => !i.Vacant);

                employees = query.ToList();
            }
            else
            {
                query = (from e in query
                         where (getHidden || !e.Hidden) &&
                         e.ActorCompanyId == actorCompanyId
                         select e);

                if (active == true)
                    query = query.Where(i => i.State == (int)SoeEntityState.Active);
                else if (active == false)
                    query = query.Where(i => i.State == (int)SoeEntityState.Inactive);

                employees = query.ToList();
            }

            if (orderByName)
                return employees.OrderByDescending(e => e.Hidden).ThenBy(e => e.Vacant).ThenBy(e => e.ContactPerson.FirstName).ThenBy(e => e.ContactPerson.LastName).ToList();
            else
                return employees.OrderBy(e => e.EmployeeNrSort).ToList();
        }

        public Dictionary<int, int> GetAllEmployeesUserIds(CompEntities entities, int actorCompanyId)
        {
            return (from e in entities.Employee
                    where e.ActorCompanyId == actorCompanyId &&
                    e.UserId.HasValue &&
                    e.State != (int)SoeEntityState.Deleted
                    select e).ToDictionary(o => o.UserId.Value, o => o.EmployeeId);
        }

        public List<Employee> GetAllEmployeesByIds(int actorCompanyId, IEnumerable<int> employeeIds, bool? active = null, bool orderByName = false, bool loadEmployment = false, bool loadUser = false, bool loadContact = false, bool getHidden = false, bool getVacant = true, bool loadEmployeeVactionSE = false, bool loadEmploymentVacactionGroup = false, bool loadScheduleAndTemplateHead = false, bool loadEmploymentPriceType = false, bool loadEmploymentAccounts = false, bool loadEmployeeAccounts = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetAllEmployeesByIds(entities, actorCompanyId, employeeIds, active, orderByName, loadEmployment, loadUser, loadContact, getHidden, getVacant, loadEmployeeVactionSE, loadEmploymentVacactionGroup, loadScheduleAndTemplateHead, loadEmploymentPriceType, loadEmploymentAccounts, loadEmployeeAccounts);
        }

        public List<Employee> GetAllEmployeesByIds(CompEntities entities, int actorCompanyId, IEnumerable<int> employeeIds, bool? active = null, bool orderByName = false, bool loadEmployment = false, bool loadUser = false, bool loadContact = false, bool getHidden = false, bool getVacant = true, bool loadEmployeeVactionSE = false, bool loadEmploymentVacactionGroup = false, bool loadScheduleAndTemplateHead = false, bool loadEmploymentPriceType = false, bool loadEmploymentAccounts = false, bool loadEmployeeAccounts = false)
        {
            int batchSize = 1000;
            List<Employee> employees = new List<Employee>();

            if (employeeIds.IsNullOrEmpty())
                return employees;

            List<int> batch = new List<int>();
            List<int> unhandledEmployeeId = employeeIds.ToList();

            while (unhandledEmployeeId.Any())
            {
                batch = unhandledEmployeeId.Take(batchSize).ToList();
                unhandledEmployeeId = unhandledEmployeeId.Skip(batchSize).ToList();

                employees.AddRange(GetAllEmployeesByIdsForBatch(entities, actorCompanyId, batch, active, orderByName, loadEmployment, loadUser, loadContact, getHidden, getVacant, loadEmployeeVactionSE, loadEmploymentVacactionGroup, loadScheduleAndTemplateHead, loadEmploymentPriceType, loadEmploymentAccounts, loadEmployeeAccounts));
            }

            if (orderByName)
                return employees.OrderByDescending(e => e.Hidden).ThenBy(e => e.Vacant).ThenBy(e => e.ContactPerson.FirstName).ThenBy(e => e.ContactPerson.LastName).ToList();
            else
                return employees.OrderBy(e => e.EmployeeNrSort).ToList();
        }

        private List<Employee> GetAllEmployeesByIdsForBatch(CompEntities entities, int actorCompanyId, IEnumerable<int> employeeIds, bool? active = null, bool orderByName = false, bool loadEmployment = false, bool loadUser = false, bool loadContact = false, bool getHidden = false, bool getVacant = true, bool loadEmployeeVactionSE = false, bool loadEmploymentVacactionGroup = false, bool loadScheduleAndTemplateHead = false, bool loadEmploymentPriceType = false, bool loadEmploymentAccounts = false, bool loadEmployeeAccounts = false)
        {
            if (employeeIds.IsNullOrEmpty())
                return new List<Employee>();

            // Always load ContactPerson, needed for name
            IQueryable<Employee> query = entities.Employee.Include("ContactPerson");
            if (loadEmployment)
                query = query.Include("Employment.EmploymentChangeBatch.EmploymentChange")
                             .Include("Employment.OriginalEmployeeGroup")
                             .Include("Employment.OriginalPayrollGroup")
                             .Include("Employment.OriginalAnnualLeaveGroup");
            if (loadEmploymentVacactionGroup)
                query = query.Include("Employment.EmploymentVacationGroup.VacationGroup");
            if (loadUser)
                query = query.Include("User");
            if (loadContact)
                query = query.Include("ContactPerson.Actor.Contact.ContactECom")
                             .Include("ContactPerson.Actor.Contact.ContactAddress.ContactAddressRow");
            if (loadEmployeeVactionSE)
                query = query.Include("EmployeeVacationSE");
            if (loadScheduleAndTemplateHead)
                query = query.Include("EmployeeSchedule.TimeScheduleTemplateHead");
            if (loadEmploymentPriceType)
                query = query.Include("Employment.EmploymentPriceType.PayrollPriceType")
                             .Include("Employment.EmploymentPriceType.EmploymentPriceTypePeriod");
            if (loadEmploymentAccounts)
                query = query.Include("Employment.EmploymentAccountStd.AccountInternal.Account");
            if (loadEmployeeAccounts)
                query = query.Include("EmployeeAccount");

            query = (from e in query
                     where e.ActorCompanyId == actorCompanyId &&
                     e.State != (int)SoeEntityState.Deleted &&
                     employeeIds.Contains(e.EmployeeId)
                     select e);

            if (active == true)
                query = query.Where(i => i.State == (int)SoeEntityState.Active);
            else if (active == false)
                query = query.Where(i => i.State == (int)SoeEntityState.Inactive);

            if (!getVacant)
                query = query.Where(i => !i.Vacant);

            List<Employee> employees = query.ToList();

            if (orderByName)
                return employees.OrderByDescending(e => e.Hidden).ThenBy(e => e.Vacant).ThenBy(e => e.ContactPerson.FirstName).ThenBy(e => e.ContactPerson.LastName).ToList();
            else
                return employees.OrderBy(e => e.EmployeeNrSort).ToList();
        }

        public List<Employee> GetAllEmployeesWithChanges(DateTime changeDate, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetAllEmployeesWithChanges(entities, changeDate, actorCompanyId);
        }

        public List<Employee> GetAllEmployeesWithChanges(CompEntities entities, DateTime changeDate, int actorCompanyId)
        {
            return (from e in entities.Employee
                    where !e.Hidden &&
                    e.ActorCompanyId == actorCompanyId &&
                    ((e.Created.HasValue && e.Created >= changeDate) || (e.Modified.HasValue && e.Modified.Value >= changeDate))
                    select e).ToList();
        }

        public List<Employee> GetAllEmployeesByNumber(int actorCompanyId, string employeeNr, int take = 100)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetAllEmployeesByNumber(entities, actorCompanyId, employeeNr, take);
        }

        public List<Employee> GetAllEmployeesByNumber(CompEntities entities, int actorCompanyId, string employeeNr, int take = 100)
        {
            List<Employee> employees = GetAllEmployees(entities, actorCompanyId, true);

            return (from e in employees
                    where (String.IsNullOrEmpty(employeeNr) || (e.EmployeeNr != null && e.EmployeeNr.ToLower().Contains(employeeNr.ToLower())) || (e.Name != null && e.Name.ToLower().Contains(employeeNr.ToLower())))
                    orderby e.Name ascending
                    select e).Take(take).ToList();
        }

        public List<Employee> GetAllEmployeesByNumbers(int actorCompanyId, List<string> employeeNrs)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetAllEmployeesByNumbers(entities, actorCompanyId, employeeNrs);
        }

        public List<Employee> GetAllEmployeesByNumbers(CompEntities entities, int actorCompanyId, List<string> employeeNrs)
        {
            List<Employee> employees = GetAllEmployees(entities, actorCompanyId, true);

            return (from e in employees
                    where employeeNrs.Contains(e.EmployeeNr)
                    orderby e.Name ascending
                    select e).ToList();
        }

        public List<Employee> GetAllEmployeesByExternalCodes(CompEntities entities, int actorCompanyId, List<string> externalCodes)
        {
            List<Employee> employees = GetAllEmployees(entities, actorCompanyId, true);

            return (from e in entities.Employee
                    where e.ActorCompanyId == actorCompanyId &&
                    !e.Hidden &&
                    !e.Vacant &&
                    e.State == (int)SoeEntityState.Active &&
                    externalCodes.Contains(e.ExternalCode)
                    select e).ToList();
        }

        public List<Employee> GetAllEmployeesByGroup(int actorCompanyId, int employeeGroupId, DateTime? date = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetAllEmployeesByGroup(entities, actorCompanyId, employeeGroupId, date);
        }

        public List<Employee> GetAllEmployeesByGroup(CompEntities entities, int actorCompanyId, int employeeGroupId, DateTime? date = null)
        {
            List<Employee> employeesForEmployeeGroup = new List<Employee>();

            if (!date.HasValue)
                date = DateTime.Today;

            List<Employee> employees = GetAllEmployees(entities, actorCompanyId, active: true, loadEmployment: true, loadUser: true);
            foreach (Employee employee in employees)
            {
                if (employee.GetEmployeeGroupId(date) == employeeGroupId)
                    employeesForEmployeeGroup.Add(employee);
            }

            return employeesForEmployeeGroup;
        }

        public List<Employee> GetAllEmployeesWithUser(CompEntities entities, int actorCompanyId)
        {
            return (from e in entities.Employee
                    where e.ActorCompanyId == actorCompanyId &&
                    e.UserId.HasValue &&
                    !e.Hidden &&
                    !e.Vacant &&
                    e.State == (int)SoeEntityState.Active
                    select e).ToList();
        }

        public List<Employee> GetAllEmployeesWithPayrollToExport(int actorCompanyId, int userId, int roleId, int timePeriodId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            entities.EmployeeTimePeriod.NoTracking();
            return GetAllEmployeesWithPayrollToExport(entities, actorCompanyId, userId, roleId, timePeriodId);
        }

        public List<Employee> GetAllEmployeesWithPayrollToExport(CompEntities entities, int actorCompanyId, int userId, int roleId, int timePeriodId)
        {
            List<AttestRole> currentAttestRoles = AttestManager.GetAttestRolesForUser(actorCompanyId, userId, DateTime.Today, SoeModule.Time);
            if (!currentAttestRoles.Any(x => x.ShowAllCategories))
                return new List<Employee>();

            var employeeIds = (from e in entities.EmployeeTimePeriod
                               where (e.ActorCompanyId == actorCompanyId &&
                               e.TimePeriodId == timePeriodId &&
                               e.Status == (int)SoeEmployeeTimePeriodStatus.Locked &&
                               e.State == (int)SoeEntityState.Active)
                               select e.EmployeeId).ToList();

            return (from e in entities.Employee
                        .Include("ContactPerson")
                        .Include("Employment.EmploymentChangeBatch.EmploymentChange")
                        .Include("Employment.OriginalEmployeeGroup")
                        .Include("Employment.OriginalPayrollGroup")
                        .Include("Employment.OriginalAnnualLeaveGroup")
                    where (e.ActorCompanyId == actorCompanyId &&
                    employeeIds.Contains(e.EmployeeId))
                    select e).ToList();
        }

        public List<EmployeeSmallDTO> GetEmployeesForPayrollPaymentSelection(int actorCompanyId, int userId, int roleId, int timePeriodId, int payrollGroupId, bool exportingExtendSelection = false)
        {
            List<EmployeeSmallDTO> employees = new List<EmployeeSmallDTO>();
            List<Employee> allEmployees = GetAllEmployeesWithPayrollToExport(actorCompanyId, userId, roleId, timePeriodId);
            if (exportingExtendSelection)
                allEmployees = allEmployees.Where(x => x.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_NorweiganAccount).ToList();
            else
                allEmployees = allEmployees.Where(x => x.DisbursementMethod != (int)TermGroup_EmployeeDisbursementMethod.SE_NorweiganAccount).ToList();

            if (payrollGroupId == 0)
            {
                employees = allEmployees.ToSmallDTOs().ToList();
            }
            else
            {
                List<EmployeeDTO> dtos = allEmployees.ToDTOs(includeEmployments: true, includeEmployeeGroup: true, includePayrollGroup: true);
                TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(timePeriodId, actorCompanyId);
                if (timePeriod != null)
                {
                    dtos = dtos.GetEmployeesInPayrollGroup(payrollGroupId, timePeriod.StartDate, timePeriod.StopDate);
                    foreach (EmployeeDTO dto in dtos)
                    {
                        employees.Add(new EmployeeSmallDTO()
                        {
                            EmployeeId = dto.EmployeeId,
                            EmployeeNr = dto.EmployeeNr,
                            Name = dto.Name
                        });
                    }
                }
                else
                {
                    employees = allEmployees.ToSmallDTOs().ToList();
                }
            }

            return employees;
        }

        public List<Employee> GetEmployeesWithSalarySpecification(int actorCompanyId, List<int> timePeriodIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            entities.EmployeeTimePeriod.NoTracking();
            return GetEmployeesWithSalarySpecification(entities, actorCompanyId, timePeriodIds);
        }

        public List<Employee> GetEmployeesWithSalarySpecification(CompEntities entities, int actorCompanyId, List<int> timePeriodIds)
        {
            List<int> employeeIds = (from e in entities.EmployeeTimePeriod
                                     where (e.ActorCompanyId == actorCompanyId &&
                                     timePeriodIds.Any(x => x == e.TimePeriodId) &&
                                     (e.Status == (int)SoeEmployeeTimePeriodStatus.Open || e.Status == (int)SoeEmployeeTimePeriodStatus.Locked || e.Status == (int)SoeEmployeeTimePeriodStatus.Paid) &&
                                     e.State == (int)SoeEntityState.Active)
                                     select e.EmployeeId).Distinct().ToList();

            List<Employee> employees = (from e in entities.Employee
                                        .Include("ContactPerson")
                                        .Include("Employment.EmploymentChangeBatch.EmploymentChange")
                                        .Include("Employment.OriginalEmployeeGroup")
                                        .Include("Employment.OriginalPayrollGroup")
                                        .Include("Employment.EmploymentVacationGroup")
                                        .Include("Employment.OriginalAnnualLeaveGroup")
                                        where (e.ActorCompanyId == actorCompanyId &&
                                        employeeIds.Contains(e.EmployeeId))
                                        select e).ToList();

            foreach (Employee employee in employees)
            {
                employee.ActiveString = employee.State == (int)SoeEntityState.Active ? GetText(5713, "Ja") : GetText(5714, "Nej");
            }

            return employees.OrderBy(w => w.EmployeeNrSort).ToList();
        }

        public List<Employee> GetEmployeesForCompanyWithEmployment(CompEntities entities, int actorCompanyId, int userId, bool useAccountHierarchy, DateTime dateFrom, DateTime dateTo, bool forceDefaultDim = false)
        {
            List<int> currentHierarchyAccountIds = useAccountHierarchy ? AccountManager.GetAccountHierarchySettingAccounts(entities, useAccountHierarchy, actorCompanyId, userId, dateFrom, dateTo, forceDefaultDim) : null;
            return GetEmployeesForCompanyWithEmployment(entities, actorCompanyId, accountIds: currentHierarchyAccountIds, employeeAccountDate: dateFrom).RemoveEmployeesWithoutEmployment(dateTo);
        }

        public List<Employee> GetEmployeesForCompanyWithEmployment(CompEntities entities, int actorCompanyId, List<int> accountIds = null, DateTime? employeeAccountDate = null, List<int> excludeEmployeeIds = null)
        {
            var query = GetEmployeesWithEmploymentLoadingsQuery(entities, true, false).Where(e => e.ActorCompanyId == actorCompanyId);

            if (!accountIds.IsNullOrEmpty() && employeeAccountDate.HasValue)
            {
                query = query.Where(e => e.EmployeeAccount.Any(ea =>
                    ea.State == (int)SoeEntityState.Active &&
                    ea.AccountId.HasValue &&
                    accountIds.Contains(ea.AccountId.Value) &&
                    ea.DateFrom <= employeeAccountDate &&
                    (!ea.DateTo.HasValue || ea.DateTo >= employeeAccountDate)));
            }
            if (!excludeEmployeeIds.IsNullOrEmpty())
                query = query.Where(i => !excludeEmployeeIds.Contains(i.EmployeeId));

            List<Employee> employees = query.ToList();
            return employees.OrderBy(e => e.EmployeeNrSort).ToList();
        }

        public List<int> GetAllEmployeeIds(int actorCompanyId, bool? active = true, bool getHidden = false, bool getVacant = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetAllEmployeeIds(entities, actorCompanyId, active, getHidden, getVacant);
        }

        public List<int> GetAllEmployeeIds(CompEntities entities, int actorCompanyId, bool? active = true, bool getHidden = false, bool getVacant = true)
        {
            if (active == true)
            {
                return (from e in entities.Employee
                        where e.ActorCompanyId == actorCompanyId &&
                        (getHidden || !e.Hidden) &&
                        (getVacant || !e.Vacant) &&
                        e.State == (int)SoeEntityState.Active
                        select e.EmployeeId).ToList();
            }
            else if (active == false)
            {
                return (from e in entities.Employee
                        where e.ActorCompanyId == actorCompanyId &&
                        (getHidden || !e.Hidden) &&
                        (getVacant || !e.Vacant) &&
                        e.State == (int)SoeEntityState.Inactive
                        select e.EmployeeId).ToList();
            }
            else
            {
                return (from e in entities.Employee
                        where e.ActorCompanyId == actorCompanyId &&
                        (getHidden || !e.Hidden) &&
                        (getVacant || !e.Vacant) &&
                        e.State != (int)SoeEntityState.Deleted
                        select e.EmployeeId).ToList();
            }
        }

        public List<int> GetAllEmployeeIdsByEmployeeNr(int actorCompanyId, List<string> employeeNrs)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetAllEmployeeIdsByEmployeeNr(entities, actorCompanyId, employeeNrs);
        }

        public List<int> GetAllEmployeeIdsByEmployeeNr(CompEntities entities, int actorCompanyId, List<string> employeeNrs)
        {
            return (from e in entities.Employee
                    where employeeNrs.Contains(e.EmployeeNr) &&
                    e.ActorCompanyId == actorCompanyId &&
                    e.State != (int)SoeEntityState.Deleted
                    select e.EmployeeId).ToList();
        }

        public List<int> GetAllEmployeeIdsByEmployeeNrFromTo(int actorCompanyId, string employeeNrFrom, string employeeNrTo)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            var employees = entitiesReadOnly.Employee.Where(e => e.ActorCompanyId == actorCompanyId && e.State != (int)SoeEntityState.Deleted).Select(e => new SmallGenericType() { Id = e.EmployeeId, Name = e.EmployeeNr }).ToList();

            if (StringUtility.IsNumeric(employeeNrFrom) && StringUtility.IsNumeric(employeeNrTo))
            {
                employees = employees.Where(p => Validator.ValidateStringInterval(p.Name, employeeNrFrom, employeeNrTo)).ToList();
            }
            else
            {
                employees = employees.Where(d => d.Name.CompareTo(employeeNrFrom) >= 0 && d.Name.CompareTo(employeeNrTo) <= 0).ToList();
            }

            return employees.Select(e => e.Id).ToList();
        }
        public List<EmployeeSmallDTO> GetAllEmployeeSmallDTOs(int actorCompanyId, bool addEmptyRow, bool concatNumberAndName, bool getHidden = false, bool orderByName = false)
        {
            var employees = GetAllEmployees(actorCompanyId, active: true, getHidden: getHidden, orderByName: orderByName);
            return employees.ToSmallDTOs().ToList();
        }

        public Dictionary<int, string> GetAllEmployeesDict(int actorCompanyId, bool addEmptyRow, bool concatNumberAndName, bool getHidden = false, bool orderByName = false)
        {
            List<Employee> employees = GetAllEmployees(actorCompanyId, active: true, getHidden: getHidden, orderByName: orderByName);
            return employees.ToDict(addEmptyRow, concatNumberAndName);
        }

        public int GetAllEmployeesCount(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from e in entitiesReadOnly.Employee
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active
                    select e).Count();
        }

        public Dictionary<int, string> GetScheduleSwapAvailableEmployees(int actorCompanyId, int userId, int roleId, int initiatorEmployeeId, DateTime initiatorShiftDate, DateTime swapShiftDate)
        {
            Dictionary<int, string> employeesDict = new Dictionary<int, string>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            Employee currentEmployee = GetEmployee(initiatorEmployeeId, actorCompanyId, loadContactPerson: true);

            // Account hierarchy
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
            if (useAccountHierarchy)
            {
                #region AccountHierarchy

                entitiesReadOnly.Employee.NoTracking();
                entitiesReadOnly.ContactPerson.NoTracking();
                List<int> employeeIds = (from e in entitiesReadOnly.Employee
                                         where e.ActorCompanyId == actorCompanyId &&
                                         !e.Hidden &&
                                         !e.Vacant &&
                                         e.State == (int)SoeEntityState.Active
                                         select e.EmployeeId).ToList();

                List<int> initiatorShiftDateEmployeeIds = GetValidEmployeeByAccountHierarchy(entitiesReadOnly, actorCompanyId, roleId, userId, employeeIds, currentEmployee, initiatorShiftDate, initiatorShiftDate, getHidden: false, useShowOtherEmployeesPermission: true, onlyDefaultAccounts: false, ignoreAttestRoles: true);
                List<int> swapShiftDateEmployeeIds = GetValidEmployeeByAccountHierarchy(entitiesReadOnly, actorCompanyId, roleId, userId, employeeIds, currentEmployee, swapShiftDate, swapShiftDate, getHidden: false, useShowOtherEmployeesPermission: true, onlyDefaultAccounts: false, ignoreAttestRoles: true);
                employeeIds = initiatorShiftDateEmployeeIds.Intersect(swapShiftDateEmployeeIds).ToList();

                List<Employee> employees = (from e in entitiesReadOnly.Employee
                                            .Include("ContactPerson")
                                            where e.ActorCompanyId == actorCompanyId &&
                                            employeeIds.Contains(e.EmployeeId) &&
                                            e.EmployeeId != initiatorEmployeeId
                                            select e).ToList();

                foreach (var employee in employees)
                {
                    employeesDict.Add(employee.EmployeeId, employee.EmployeeNrAndName);
                }

                #endregion
            }
            else
            {
                #region Categories

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_SeeOtherEmployeesShifts, Permission.Readonly, roleId, actorCompanyId))
                {
                    Dictionary<int, string> categoriesDateFrom = CategoryManager.GetCategoriesForRoleFromTypeDict(actorCompanyId, userId, initiatorEmployeeId, SoeCategoryType.Employee, false, true, false, initiatorShiftDate, initiatorShiftDate);
                    var initiatorShiftDateEmployeesDict = GetEmployeesDictByCategories(actorCompanyId, categoriesDateFrom.Select(x => x.Key).ToList(), false);

                    Dictionary<int, string> categoriesDateTo = CategoryManager.GetCategoriesForRoleFromTypeDict(actorCompanyId, userId, initiatorEmployeeId, SoeCategoryType.Employee, false, true, false, swapShiftDate, swapShiftDate);
                    var swapShiftDateEmployeesDict = GetEmployeesDictByCategories(actorCompanyId, categoriesDateTo.Select(x => x.Key).ToList(), false);

                    List<int> intersectIds = initiatorShiftDateEmployeesDict.Select(x => x.Key).Intersect(swapShiftDateEmployeesDict.Select(x => x.Key)).ToList();
                    foreach (int intersectId in intersectIds)
                    {
                        employeesDict.Add(intersectId, swapShiftDateEmployeesDict[intersectId]);
                    }
                }

                #endregion
            }

            return employeesDict;
        }

        #endregion

        #region Employees for Attestroles (Category / AccountHierarchy)

        public List<Employee> GetEmployeesForUsersAttestRoles(
            out EmployeeAuthModelRepository repository,
            int actorCompanyId,
            int userId,
            int roleId,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            List<int> employeeFilter = null,
            List<int> employeeAuthModelFilter = null,
            string employeeSearchPattern = "",
            bool discardCache = false,
            bool? active = true,
            bool getVacant = true,
            bool getHidden = false,
            bool includeEnded = false,
            bool includeNotStarted = false,
            bool discardLimitSettingForEnded = false,
            bool doValidateEmployment = false,
            bool? onlyDefaultEmployeeAuthModel = null,
            bool useShowOtherEmployeesPermission = false,
            bool addEmployeeAuthModelInfo = false,
            bool addStateInfo = false,
            bool addRoleInfo = false,
            bool addEmployeeGroupInfo = false,
            bool addPayrollGroupInfo = false,
            bool addProjectDefaultTimeCode = false,
            bool addContactInfo = false,
            bool loadEmployment = true,
            bool loadEmployeeVactionSE = false,
            bool loadEmploymentVacactionGroup = false,
            bool loadEmploymentPriceType = false,
            bool loadScheduleAndTemplateHead = false,
            bool orderByName = false,
            bool loadEmployeeAccounts = false,
            bool useDefaultEmployeeAccountDimEmployee = false,
            bool loadUser = false,
            bool isReportSelection = false,
            bool addAnnualLeaveGroupInfo = false
            )
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            entitiesReadOnly.Employee.NoTracking();
            return GetEmployeesForUsersAttestRoles(entitiesReadOnly,
                out repository,
                actorCompanyId,
                userId,
                roleId,
                dateFrom,
                dateTo,
                employeeFilter,
                employeeAuthModelFilter,
                employeeSearchPattern,
                discardCache,
                active,
                getVacant,
                getHidden,
                includeEnded,
                includeNotStarted,
                discardLimitSettingForEnded,
                doValidateEmployment,
                onlyDefaultEmployeeAuthModel,
                useShowOtherEmployeesPermission,
                addEmployeeAuthModelInfo,
                addStateInfo,
                addRoleInfo,
                addEmployeeGroupInfo,
                addPayrollGroupInfo,
                addProjectDefaultTimeCode,
                addContactInfo,
                loadEmployment,
                loadEmployeeVactionSE,
                loadEmploymentVacactionGroup,
                loadEmploymentPriceType,
                loadScheduleAndTemplateHead,
                orderByName,
                loadEmployeeAccounts,
                useDefaultEmployeeAccountDimEmployee,
                loadUser,
                isReportSelection,
                addAnnualLeaveGroupInfo
                );
        }

        public List<Employee> GetEmployeesForUsersAttestRoles(
            CompEntities entities,
            out EmployeeAuthModelRepository repository,
            int actorCompanyId,
            int userId,
            int roleId,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            List<int> employeeFilter = null,
            List<int> employeeAuthModelFilter = null,
            string employeeSearchPattern = "",
            bool discardCache = false,
            bool? active = true,
            bool getVacant = true,
            bool getHidden = false,
            bool includeEnded = false,
            bool includeNotStarted = false,
            bool discardLimitSettingForEnded = false,
            bool doValidateEmployment = false,
            bool? onlyDefaultEmployeeAuthModel = null,
            bool useShowOtherEmployeesPermission = false,
            bool addEmployeeAuthModelInfo = false,
            bool addStateInfo = false,
            bool addRoleInfo = false,
            bool addEmployeeGroupInfo = false,
            bool addPayrollGroupInfo = false,
            bool addProjectDefaultTimeCode = false,
            bool addContactInfo = false,
            bool loadEmployment = true,
            bool loadEmployeeVactionSE = false,
            bool loadEmploymentVacactionGroup = false,
            bool loadEmploymentPriceType = false,
            bool loadScheduleAndTemplateHead = false,
            bool orderByName = false,
            bool loadEmployeeAccounts = false,
            bool useDefaultEmployeeAccountDimEmployee = false,
            bool loadUser = false,
            bool isReportSelection = false,
            bool addAnnualLeaveGroupInfo = false
            )
        {
            #region Prereq

            repository = null;
            dateFrom = dateFrom.ToValueOrToday();
            dateTo = dateTo.ToValueOrToday();

            if (employeeFilter.IsNullOrEmpty() && !employeeSearchPattern.IsNullOrEmpty())
                employeeFilter = GetEmployeesIdsBySearch(actorCompanyId, employeeSearchPattern);

            List<int> employeeIdsForCompany = employeeFilter ?? GetAllEmployeeIds(entities, actorCompanyId, active: active, getHidden: getHidden).ToList();
            if (employeeIdsForCompany.IsNullOrEmpty())
                return new List<Employee>();

            //Current Employee
            Employee currentEmployee = GetEmployeeForUser(entities, userId, actorCompanyId);

            //Vacation
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<VacationGroup> vacationGroups = loadEmployeeVactionSE || loadEmploymentVacactionGroup ? GetVacationGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId)) : null;
            if (vacationGroups.IsNullOrEmpty())
            {
                loadEmployeeVactionSE = false;
                loadEmploymentVacactionGroup = false;
            }

            #endregion

            #region Load and filter Employees

            int totalEmployees = employeeIdsForCompany.Count;
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
            if (useAccountHierarchy)
            {
                AccountHierarchyInput accountHierarchyInput = AccountHierarchyInput.GetInstance();
                accountHierarchyInput.AddParamValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimEmployee, useDefaultEmployeeAccountDimEmployee);
                employeeIdsForCompany = GetValidEmployeesByAccountHierarchy(
                    entities,
                    out AccountRepository accountRepository,
                    actorCompanyId,
                    roleId,
                    userId,
                    employeeIdsForCompany,
                    currentEmployee,
                    dateFrom.Value,
                    dateTo.Value,
                    includeEnded: includeEnded,
                    includeNotStarted: includeNotStarted,
                    discardLimitSettingForEnded: discardLimitSettingForEnded,
                    getHidden: getHidden,
                    useShowOtherEmployeesPermission: useShowOtherEmployeesPermission,
                    addAccountInfo: addEmployeeAuthModelInfo,
                    onlyDefaultAccounts: onlyDefaultEmployeeAuthModel,
                    discardCache: discardCache,
                    isReportSelection: isReportSelection,
                    filteredAccountIds: employeeAuthModelFilter,
                    input: accountHierarchyInput
                    );
                repository = accountRepository;
            }
            else
            {
                employeeIdsForCompany = GetValidEmployeesByCategories(
                    entities,
                    out CategoryRepository categoryRepository,
                    actorCompanyId,
                    roleId,
                    userId,
                    employeeIdsForCompany,
                    currentEmployee,
                    dateFrom.Value,
                    dateTo.Value,
                    includeEnded: includeEnded,
                    includeNotStarted: includeNotStarted,
                    discardLimitSettingForEnded: discardLimitSettingForEnded,
                    useShowOtherEmployeesPermission: useShowOtherEmployeesPermission,
                    getHidden: getHidden,
                    addCategoryInfo: addEmployeeAuthModelInfo,
                    categoryIds: employeeAuthModelFilter,
                    onlyDefaultCategories: onlyDefaultEmployeeAuthModel,
                    discardCache: discardCache
                    );
                repository = categoryRepository;
            }

            List<Employee> validEmployees;
            if (employeeFilter == null && totalEmployees == employeeIdsForCompany.Count)
                validEmployees = GetAllEmployees(entities, actorCompanyId, active: active, loadEmployment: loadEmployment, loadUser: loadUser, getHidden: getHidden, getVacant: getVacant, orderByName: orderByName, loadContact: addContactInfo, loadEmployeeVactionSE: loadEmployeeVactionSE, loadEmploymentVacactionGroup: loadEmploymentVacactionGroup, loadScheduleAndTemplateHead: loadScheduleAndTemplateHead, loadEmploymentPriceType: loadEmploymentPriceType, loadEmployeeAccounts: useAccountHierarchy);
            else
                validEmployees = GetAllEmployeesByIds(entities, actorCompanyId, employeeIdsForCompany, active: active, loadUser: loadUser, getHidden: getHidden, getVacant: getVacant, orderByName: orderByName, loadEmployment: loadEmployment, loadContact: addContactInfo, loadEmployeeVactionSE: loadEmployeeVactionSE, loadEmploymentVacactionGroup: loadEmploymentVacactionGroup, loadScheduleAndTemplateHead: loadScheduleAndTemplateHead, loadEmploymentPriceType: loadEmploymentPriceType, loadEmployeeAccounts: loadEmployeeAccounts);

            if (doValidateEmployment)
                validEmployees = FilterEmployeeByEmployment(entities, validEmployees, dateFrom.Value, dateTo.Value, includeEnded, includeNotStarted, discardLimitSettingForEnded);
            if (validEmployees.IsNullOrEmpty())
                return new List<Employee>();

            #endregion

            #region Set optional info

            if (addEmployeeAuthModelInfo)
                repository.SetEmployeeAuthNames(validEmployees);

            if (addStateInfo || addRoleInfo || addProjectDefaultTimeCode || addEmployeeGroupInfo || addPayrollGroupInfo || addAnnualLeaveGroupInfo)
            {
                Dictionary<int, List<UserCompanyRole>> dictUserCompanyRolesForCompany = null;
                if (addRoleInfo)
                {
                    List<UserCompanyRole> userCompanyRolesForCompany = addRoleInfo ? UserManager.GetUserCompanyRolesForCompany(actorCompanyId, loadRole: true) : null;
                    foreach (var userCompanyRoleGrouping in userCompanyRolesForCompany.GroupBy(i => i.RoleId))
                    {
                        string name = RoleManager.GetRoleNameText(userCompanyRoleGrouping.First().Role);
                        foreach (UserCompanyRole userCompanyRole in userCompanyRoleGrouping)
                        {
                            userCompanyRole.Role.ActualName = name;
                        }
                    }
                    dictUserCompanyRolesForCompany = userCompanyRolesForCompany.GroupBy(g => g.UserId).ToDictionary(k => k.Key, v => v.ToList());
                }

                string yes = addStateInfo ? GetText(5713, "Ja") : string.Empty;
                string no = addStateInfo ? GetText(5714, "Nej") : string.Empty;
                int? defaultTimeCodeId = addProjectDefaultTimeCode ? SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0) : (int?)null;
                List<TimeCode> timeCodes = addProjectDefaultTimeCode ? new List<TimeCode>() : null;
                List<EmployeeGroup> employeeGroups = addEmployeeGroupInfo ? GetEmployeeGroupsFromCache(entities, CacheConfig.Company(actorCompanyId)) : null;
                List<PayrollGroup> payrollGroups = addPayrollGroupInfo ? GetPayrollGroupsFromCache(entities, CacheConfig.Company(actorCompanyId)) : null;
                List<AnnualLeaveGroup> annualLeaveGroups = addAnnualLeaveGroupInfo ? GetAnnualLeaveGroupsFromCache(entities, CacheConfig.Company(actorCompanyId)) : null;

                foreach (Employee employee in validEmployees)
                {
                    if (addStateInfo)
                        employee.ActiveString = employee.State == (int)SoeEntityState.Active ? yes : no;

                    if (addRoleInfo && employee.UserId.HasValue && dictUserCompanyRolesForCompany.ContainsKey(employee.UserId.Value))
                        employee.RoleNames = dictUserCompanyRolesForCompany[employee.UserId.Value].Select(s => s.Role.ActualName).Distinct().ToList();

                    if (addProjectDefaultTimeCode)
                    {
                        int timeCodeId = TimeCodeManager.GetDefaultTimeCodeId(employee, defaultTimeCodeId.Value);
                        if (timeCodeId > 0)
                        {
                            TimeCode timeCode = timeCodes.FirstOrDefault(i => i.TimeCodeId == timeCodeId) ?? TimeCodeManager.GetTimeCode(entities, timeCodeId, actorCompanyId, true);
                            if (timeCode != null)
                            {
                                employee.ProjectDefaultTimeCodeId = timeCode.TimeCodeId;
                                employee.ProjectDefaultTimeCodeName = timeCode.Name;

                                if (!timeCodes.Any(i => i.TimeCodeId == timeCode.TimeCodeId))
                                    timeCodes.Add(timeCode);
                            }
                        }
                    }

                    if (addEmployeeGroupInfo || addPayrollGroupInfo || addAnnualLeaveGroupInfo)
                    {
                        Employment employment = employee.GetEmployment(dateFrom.Value, dateTo.Value);
                        if (employment != null)
                        {
                            if (addEmployeeGroupInfo)
                                employee.EmployeeGroupNames = employment.GetEmployeeGroups(dateFrom.Value, dateTo.Value, employeeGroups).Select(i => i.Name).Distinct().ToList();
                            if (addPayrollGroupInfo)
                                employee.PayrollGroupNames = employment.GetPayrollGroups(dateFrom.Value, dateTo.Value, payrollGroups).Select(i => i.Name).Distinct().ToList();
                            if (addAnnualLeaveGroupInfo)
                                employee.AnnualLeaveGroupNames = employment.GetAnnualLeaveGroups(dateFrom.Value, dateTo.Value, annualLeaveGroups).Select(i => i.Name).Distinct().ToList();
                        }
                    }
                }
            }

            #endregion

            if (SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseHibernatingEmployment, 0, base.ActorCompanyId, 0))
                validEmployees.SetHibernatingPeriods();

            return validEmployees;
        }

        public List<Employee> GetEmployeesByFilter(int actorCompanyId, int userId, int roleId, DateTime dateFrom, DateTime dateTo, List<int> timePeriodIds, SoeReportTemplateType reportTemplateType, TermGroup_EmployeeSelectionAccountingType accountingType, List<int> accountIds = null, List<int> employeeCategoryIds = null, List<int> employeeGroupIds = null, List<int> payrollGroupIds = null, List<int> vacationGroupIds = null, bool includeInactive = false, bool onlyInactive = false, bool includeEnded = false, bool includeHidden = false, bool includeVacant = false, bool doValidateEmployment = false, bool includeSecondary = false, int? timeScheduleScenarioHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
            bool filterByAccounts = useAccountHierarchy && !accountIds.IsNullOrEmpty();

            #region Get Employees

            bool? active = null;
            if (!includeInactive && !onlyInactive)
                active = true;
            else if (onlyInactive)
                active = false;

            bool fetchByUserAttestRoles = true;
            List<Employee> employees = null;
            List<TimePeriod> timePeriods = null;

            if (reportTemplateType == SoeReportTemplateType.EmployeeVacationDebtReport)
                dateFrom = dateTo.AddYears(-1).Date;
            if (reportTemplateType == SoeReportTemplateType.PayrollVacationAccountingReport)//RD said it is OK
            {
                dateFrom = dateFrom.AddDays(-180);
                dateTo = dateTo.AddDays(90);
            }

            if (!timePeriodIds.IsNullOrEmpty())
            {
                timePeriods = TimePeriodManager.GetTimePeriods(timePeriodIds, actorCompanyId);
                dateFrom = timePeriods.Min(t => t.StartDate);
                dateTo = timePeriods.Max(t => t.StopDate);

                if (reportTemplateType.IsPayrollReport() && AttestManager.GetAttestRolesForUser(actorCompanyId, userId, DateTime.Today, SoeModule.Time).Any(e => e.ShowAllCategories))
                {
                    if (reportTemplateType.IsPayrollSlip())
                    {
                        fetchByUserAttestRoles = false;
                        employees = GetEmployeesWithSalarySpecification(actorCompanyId, timePeriodIds);
                    }
                    else if (reportTemplateType.IsPayrollReportWithPeriods())
                    {
                        fetchByUserAttestRoles = false;
                        employees = GetValidEmployeesByTimePeriods(actorCompanyId, timePeriodIds, active);
                        if (filterByAccounts && accountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeAccount || accountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeCategory)
                        {
                            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                            employees = FilterEmployeesByHasAnyValidAccount(entitiesReadOnly, employees, dateFrom, dateTo, filterAccountIds: accountIds, isReportSelection: true);
                            filterByAccounts = false;
                        }
                    }
                }
            }

            if (fetchByUserAttestRoles)
            {
                bool skipFilter = filterByAccounts && accountingType != TermGroup_EmployeeSelectionAccountingType.EmployeeAccount && accountingType != TermGroup_EmployeeSelectionAccountingType.EmployeeCategory;
                List<int> filterIds = useAccountHierarchy ? accountIds.ToNullable() : employeeCategoryIds;
                base.DoInlcudeInactiveAccounts();

                employees = GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, dateFrom: dateFrom, dateTo: dateTo,
                    employeeAuthModelFilter: skipFilter ? null : filterIds,
                    active: active,
                    getHidden: false,
                    includeEnded: includeEnded,
                    onlyDefaultEmployeeAuthModel: !includeSecondary,
                    loadEmployeeVactionSE: vacationGroupIds != null || reportTemplateType.LoadEmployeeVacationData(),
                    loadEmploymentVacactionGroup: vacationGroupIds != null || reportTemplateType.LoadEmployeeVacationData(),
                    loadEmployeeAccounts: accountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeAccount,
                    doValidateEmployment: doValidateEmployment,
                    isReportSelection: true
                    );
            }

            if (employees.IsNullOrEmpty())
                return employees;

            #endregion

            #region Filter Employees by groups

            if (employeeGroupIds != null)
            {
                employees = employees.Where(e => employeeGroupIds.Contains(e.GetEmployeeGroupId(dateFrom, dateTo))).ToList();
            }
            if (payrollGroupIds != null)
            {
                employees = employees.Where(e =>
                {
                    int? foundPayrollGroupId = e.GetPayrollGroupId(dateFrom, dateTo, forward: false);
                    return foundPayrollGroupId.HasValue && payrollGroupIds.Contains(foundPayrollGroupId.Value);
                }).ToList();
            }
            if (vacationGroupIds != null)
            {
                employees = employees.Where(e =>
                {
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    List<VacationGroup> vacationGroups = GetVacationGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
                    VacationGroup foundVacationGroup = e.GetVacationGroup(dateFrom, dateTo, vacationGroups, forward: false);
                    return foundVacationGroup != null && vacationGroupIds.Contains(foundVacationGroup.VacationGroupId);
                }).ToList();
            }
            if (employeeCategoryIds != null)
            {
                employees = employees.Where(e =>
                {
                    List<int> catIds = GetEmployeeCategoryIds(e.EmployeeId, actorCompanyId, false, dateFrom, dateTo);
                    return catIds.Intersect(employeeCategoryIds).Any();
                }).ToList();
            }

            #endregion

            #region Filter Employees by accountingtype

            if (filterByAccounts)
            {
                List<AccountInternalDTO> validAccountInternals = !accountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(actorCompanyId, null).Where(w => accountIds.Contains(w.AccountId)).ToDTOs() : null;

                switch (accountingType)
                {
                    case TermGroup_EmployeeSelectionAccountingType.EmployeeCategory:
                        break;
                    case TermGroup_EmployeeSelectionAccountingType.EmployeeAccount:
                        employees = FilterEmployeesFromEmployeeAccount(employees, dateFrom, dateTo, validAccountInternals);
                        break;
                    case TermGroup_EmployeeSelectionAccountingType.EmploymentAccountInternal:
                        employees = FilterEmployeesFromEmploymentAccount(employees, dateFrom, dateTo, validAccountInternals);
                        break;
                    case TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock:
                        employees = FilterEmployeesFromTimeScheduleTemplateBlock(employees, dateFrom, dateTo, validAccountInternals, timeScheduleScenarioHeadId);
                        break;
                    case TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlockAccount:
                        employees = FilterEmployeesFromTimeScheduleTemplateBlockAccount(employees, dateFrom, dateTo, validAccountInternals);
                        break;
                    case TermGroup_EmployeeSelectionAccountingType.TimePayrollTransactionAccount:
                        employees = FilterEmployeesFromTimePayrollTransactions(actorCompanyId, employees, dateFrom, dateTo, timePeriods, validAccountInternals);
                        break;
                    default:
                        break;
                }
            }

            #endregion

            #region Filter Employees by hidden/vacant

            if (includeHidden && !reportTemplateType.IsPayrollReport())
            {
                Employee hiddenEmployee = GetHiddenEmployee(actorCompanyId, loadContact: true);
                if (hiddenEmployee != null)
                    employees.Insert(0, hiddenEmployee);
            }
            if (!includeHidden)
                employees = employees.Where(e => !e.Hidden).ToList();
            if (!includeVacant)
                employees = employees.Where(e => !e.Vacant).ToList();

            #endregion

            return employees;
        }

        public List<Employee> FilterEmployeesByHasAnyValidAccount(CompEntities entities, List<Employee> employees, DateTime dateFrom, DateTime dateTo, List<int> filterAccountIds, bool onlyDefaultAccounts = true, bool isReportSelection = false)
        {
            if (filterAccountIds.IsNullOrEmpty())
                return employees;

            List<Employee> valid = new List<Employee>();
            CalculateValid();

            void CalculateValid()
            {
                if (employees.IsNullOrEmpty())
                    return;

                AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.OnlyDefaultAccounts);
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                AccountRepository accountRepository = AccountManager.GetAccountHierarchyRepositoryByIds(entitiesReadOnly, filterAccountIds, base.ActorCompanyId, base.UserId, dateFrom, dateTo, input);
                if (accountRepository == null)
                    return;

                if (accountRepository.EmployeeAccounts.IsNullOrEmpty())
                    accountRepository.SetEmployeeAccounts(GetEmployeeAccountsFromCache(entities, CacheConfig.Company(base.ActorCompanyId, 60), employeeIds: employees.GetIds()));
                if (accountRepository.EmployeeAccounts.IsNullOrEmpty())
                    return;

                List<AccountDTO> possibleValidAccounts = accountRepository.GetAccounts(true);
                if (possibleValidAccounts.IsNullOrEmpty())
                    return;

                foreach (Employee employee in employees)
                {
                    List<EmployeeAccount> employeeAccountsForEmployee = accountRepository.GetEmployeeAccounts(employee.EmployeeId, dateFrom, dateTo, onlyDefaultAccounts: onlyDefaultAccounts);
                    if (employeeAccountsForEmployee.HasAnyValidAccount(employee.EmployeeId, dateFrom, dateTo, filterAccountIds, possibleValidAccounts, accountRepository.AllAccountDims, accountRepository.AllAccountInternals, onlyDefaultAccounts, doNotValidateAsHiearchy: isReportSelection && base.IsMartinServera(entities)))
                        valid.Add(employee);
                }
            }

            return valid;
        }

        private List<Employee> FilterEmployeesFromEmployeeAccount(List<Employee> employees, DateTime dateFrom, DateTime dateTo, List<AccountInternalDTO> validAccountInternals)
        {
            if (validAccountInternals.IsNullOrEmpty())
                return employees;

            int defaultEmployeeAccountDimEmployee = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, base.ActorCompanyId, 0);
            if (validAccountInternals.All(a => a.AccountDimId != defaultEmployeeAccountDimEmployee))
                return employees;

            List<int> accountIds = validAccountInternals.Select(a => a.AccountId).ToList();
            List<int> validEmployeeIds = new List<int>();

            foreach (Employee employee in employees)
            {
                if (!employee.EmployeeAccount.IsLoaded)
                    employee.EmployeeAccount.Load();

                if (employee.EmployeeAccount != null && employee.EmployeeAccount.Any(ea => ea.State == (int)SoeEntityState.Active &&
                     ea.AccountId.HasValue && accountIds.Contains(ea.AccountId.Value) &&
                     (!ea.DateTo.HasValue || ea.DateTo.Value >= dateFrom) && ea.DateFrom <= dateTo))
                {
                    validEmployeeIds.Add(employee.EmployeeId);
                }
            }

            return employees.Where(e => validEmployeeIds.Contains(e.EmployeeId)).ToList();
        }

        private List<Employee> FilterEmployeesFromEmploymentAccount(List<Employee> employees, DateTime dateFrom, DateTime dateTo, List<AccountInternalDTO> validAccountInternals)
        {
            var employments = new List<Employment>();
            foreach (var employee in employees)
            {
                var employment = employee.GetEmployment(dateFrom, dateTo);
                if (employment != null)
                    employments.Add(employment);
            }

            var accountstd = GetEmploymentAccounts(employments.Select(s => s.EmploymentId).ToList(), EmploymentAccountType.Cost);
            var filteredEmploymentIds = accountstd.Where(a => a.AccountInternal.ValidOnFiltered(validAccountInternals)).Select(s => s.EmploymentId).Distinct().ToList();
            return employments.Where(w => filteredEmploymentIds.Contains(w.EmploymentId)).Select(s => employees.FirstOrDefault(f => f.EmployeeId == s.EmployeeId)).Where(w => w != null).Distinct().ToList();
        }

        private List<Employee> FilterEmployeesFromTimeScheduleTemplateBlock(List<Employee> employees, DateTime dateFrom, DateTime dateTo, List<AccountInternalDTO> validAccountInternals, int? timeScheduleScenarioHeadId = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var blocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entitiesReadOnly, employees.Select(s => s.EmployeeId).ToList(), dateFrom, dateTo, false, timeScheduleScenarioHeadId);
            return blocks.Where(w => w.AccountId.HasValue && validAccountInternals.Select(s => s.AccountId).Contains(w.AccountId.Value)).Select(s => employees.FirstOrDefault(f => f.EmployeeId == s.EmployeeId)).Where(w => w != null).Distinct().ToList();
        }

        private List<Employee> FilterEmployeesFromTimeScheduleTemplateBlockAccount(List<Employee> employees, DateTime dateFrom, DateTime dateTo, List<AccountInternalDTO> validAccountInternals)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var blocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entitiesReadOnly, employees.Select(s => s.EmployeeId).ToList(), dateFrom, dateTo, true);
            return blocks.Where(b => b.AccountInternal.ValidOnFiltered(validAccountInternals)).Select(s => employees.FirstOrDefault(f => f.EmployeeId == s.EmployeeId)).Where(w => w != null).Distinct().ToList();
        }

        private List<Employee> FilterEmployeeByEmployment(CompEntities entities, List<Employee> employees, DateTime dateFrom, DateTime dateTo, bool includeEnded, bool includeNotStarted, bool discardLimitSettingForEnded)
        {
            List<Employee> validEmployees = new List<Employee>();
            if (!employees.IsNullOrEmpty())
            {
                DateTime limitStartDate = GetEmployeeStartDateByEmployeeeEndedSetting(entities, includeEnded, dateFrom, discardLimitSettingForEnded);

                foreach (Employee employee in employees)
                {
                    List<DateTime> employmentDates = employee.GetEmploymentDates(dateFrom, dateTo);
                    Employment employment = !employmentDates.IsNullOrEmpty() ? employee.GetEmployment(employmentDates.Last()) : null;
                    if (employment == null && includeNotStarted)
                    {
                        employment = employee.GetFirstEmployment();
                        if (employment != null && employment.DateTo < dateFrom)
                            employment = employee.GetNextEmployment(dateTo);
                    }
                    if (employment == null && includeEnded)
                    {
                        employment = employee.GetLastEmployment(limitStartDate);
                        if (employment?.DateFrom > dateTo)
                            employment = null; //not started
                    }
                    if (employment != null)
                        validEmployees.Add(employee);
                }
            }
            return validEmployees;
        }

        private List<Employee> FilterEmployeesFromTimePayrollTransactions(int actorCompanyId, List<Employee> employees, DateTime dateFrom, DateTime dateTo, List<TimePeriod> timePeriods, List<AccountInternalDTO> validAccountInternals)
        {
            List<int> employeeIds = employees.Select(s => s.EmployeeId).ToList();
            var transactions = new List<FilterEmployeeFromTimePayrollTransactionStruct>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            if (employeeIds.Count > 100)
                entitiesReadOnly.CommandTimeout = 200;

            if (!timePeriods.IsNullOrEmpty())
            {
                foreach (var period in timePeriods)
                {
                    List<int> remainingEmployeeIdsInPeriod = new List<int>(employeeIds);

                    int batchSize = 300;
                    while (remainingEmployeeIdsInPeriod.Any())
                    {
                        var batch = remainingEmployeeIdsInPeriod.Take(batchSize);
                        remainingEmployeeIdsInPeriod = remainingEmployeeIdsInPeriod.Skip(batchSize).ToList();
                        transactions.AddRange(entitiesReadOnly.TimePayrollTransaction.Include(i => i.AccountInternal).Where(w => w.ActorCompanyId == actorCompanyId && batch.Contains(w.EmployeeId) && ((w.TimeBlockDate.Date >= period.StartDate && w.TimeBlockDate.Date <= period.StopDate && w.TimePeriodId == null) || w.TimePeriodId == period.TimePeriodId) && w.State == (int)SoeEntityState.Active).Select(s => new FilterEmployeeFromTimePayrollTransactionStruct() { TimePeriodId = s.TimePeriodId, EmployeeId = s.EmployeeId, AccountInternal = s.AccountInternal }).ToList());
                    }
                }
            }
            else
            {
                List<int> remainingEmployeeIdsInInterval = new List<int>(employeeIds);

                int batchSize = 300;
                while (remainingEmployeeIdsInInterval.Any())
                {
                    var batch = remainingEmployeeIdsInInterval.Take(batchSize);
                    remainingEmployeeIdsInInterval = remainingEmployeeIdsInInterval.Skip(batchSize).ToList();
                    transactions.AddRange(entitiesReadOnly.TimePayrollTransaction.Include(i => i.AccountInternal).Where(w => w.ActorCompanyId == actorCompanyId && batch.Contains(w.EmployeeId) && w.TimeBlockDate.Date >= dateFrom && w.TimeBlockDate.Date <= dateTo && w.State == (int)SoeEntityState.Active).Select(s => new FilterEmployeeFromTimePayrollTransactionStruct() { TimePeriodId = s.TimePeriodId, EmployeeId = s.EmployeeId, AccountInternal = s.AccountInternal }).ToList());
                }
            }

            var filteredEmployeeIds = transactions.Where(t => t.AccountInternal.ValidOnFiltered(validAccountInternals)).Select(s => s.EmployeeId).Distinct().ToList();
            var remainingEmployeeIds = employeeIds.Where(w => !filteredEmployeeIds.Contains(w)).ToList();
            var scheduleTransactions = entitiesReadOnly.TimePayrollScheduleTransaction.Include(i => i.AccountInternal).Where(w => w.ActorCompanyId == actorCompanyId && remainingEmployeeIds.Contains(w.EmployeeId) && w.TimeBlockDate.Date >= dateFrom && w.TimeBlockDate.Date <= dateTo && w.State == (int)SoeEntityState.Active).Select(s => new { s.EmployeeId, s.AccountInternal }).ToList();
            filteredEmployeeIds.AddRange(scheduleTransactions.Where(t => t.AccountInternal.ValidOnFiltered(validAccountInternals)).Select(s => s.EmployeeId).Distinct().ToList());
            return filteredEmployeeIds.Select(s => employees.FirstOrDefault(f => f.EmployeeId == s)).Where(w => w != null).Distinct().ToList();
        }

        private sealed class FilterEmployeeFromTimePayrollTransactionStruct
        {
            public int? TimePeriodId { get; set; }
            public int EmployeeId { get; set; }
            public EntityCollection<AccountInternal> AccountInternal { get; set; }
        }

        public List<Employee> GetValidEmployeesByTimePeriods(int actorCompanyId, List<int> timePeriodIds, bool? active, List<PayrollGroup> payrollGroups = null)
        {
            List<EmployeeTimePeriod> employeeTimePeriods = TimePeriodManager.GetEmployeeTimePeriods(actorCompanyId, timePeriodIds);
            List<Employee> employeesForTimePeriods = GetAllEmployeesByIds(actorCompanyId, employeeTimePeriods.Select(x => x.EmployeeId).Distinct().ToList(), active: active, loadEmployment: true);
            Dictionary<int, List<int>> validEmployeesByPeriod = PayrollManager.GetValidEmployeesForTimePeriod(actorCompanyId, timePeriodIds, employeesForTimePeriods, payrollGroups: payrollGroups, employeeTimePeriods: employeeTimePeriods);
            List<int> validEmployeeIds = validEmployeesByPeriod.SelectMany(e => e.Value).Distinct().ToList();

            return employeesForTimePeriods.Where(x => validEmployeeIds.Contains(x.EmployeeId)).ToList();
        }

        public List<int> GetEmployeesIdsBySearch(int actorCompanyId, string search)
        {
            return search
                .Split(',')
                .Select(s => s.ToLower())
                .SelectMany(token => SearchByToken(token))
                .Distinct()
                .ToList();

            List<int> SearchByToken(string token)
            {
                token = token.Trim();
                return FilterByNumberOrName().Concat(FilterBySocialSec()).ToList();

                List<int> FilterByNumberOrName()
                {
                    string[] name = token.Trim().Split(' ');
                    bool isNameSearch = name.Length > 1;
                    string firstName = isNameSearch ? name[0] : token;
                    string lastName = isNameSearch ? name[1] : token;

                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    var query = entitiesReadOnly.Employee.Where(e => e.ActorCompanyId == actorCompanyId && e.State == (int)SoeEntityState.Active);
                    if (isNameSearch)
                        query = query.Where(e => e.ContactPerson.FirstName.ToLower().Contains(firstName) && e.ContactPerson.LastName.ToLower().Contains(lastName));
                    else
                        query = query.Where(e => e.EmployeeNr.ToLower().Contains(token) || e.ContactPerson.FirstName.ToLower().Contains(token) || e.ContactPerson.LastName.ToLower().Contains(token));

                    return query.Select(e => e.EmployeeId).ToList();
                }
                List<int> FilterBySocialSec()
                {
                    bool valid = token?.Length >= 6 && token.ToCharArray().Take(6).All(c => Char.IsDigit(c));
                    if (!valid)
                        return new List<int>();

                    string socialSec = StringUtility.SocialSecYYYYMMDD_Dash_XXXX(token);
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    return entitiesReadOnly.Employee
                        .Where(e =>
                            e.ActorCompanyId == actorCompanyId &&
                            e.State == (int)SoeEntityState.Active &&
                            e.ContactPerson.SocialSec != null &&
                            e.ContactPerson.SocialSec.StartsWith(socialSec))
                        .Select(e => e.EmployeeId)
                        .ToList();
                }
            }
        }

        public List<Employee> GetEmployeesBySearch(int actorCompanyId, int userId, int roleId, string search, int no)
        {
            List<Employee> employees = GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, useShowOtherEmployeesPermission: true, orderByName: true);
            return employees.Filter(search).Take(no).ToList();
        }

        public List<EmployeeGridDTO> GetEmployeesForGrid(int actorCompanyId, int userId, int roleId, DateTime date, List<int> employeeFilter = null, bool showInactive = false, bool showEnded = false, bool showNotStarted = false, bool setAge = false, bool loadPayrollGroups = false, bool loadAnnualLeaveGroups = false)
        {
            List<Employee> employees = GetEmployeesForUsersAttestRoles(
                out _,
                actorCompanyId, userId, roleId,
                dateFrom: date, dateTo: date,
                employeeFilter: employeeFilter,
                discardCache: true,
                active: !showInactive ? true : (bool?)null,
                includeEnded: showEnded,
                includeNotStarted: showNotStarted,
                discardLimitSettingForEnded: true, //Show all ended validEmployees, no matter when they ended
                doValidateEmployment: true, //Always validate Employment for choosen default (or pass showEnded)
                useShowOtherEmployeesPermission: true,
                addEmployeeAuthModelInfo: true,
                addRoleInfo: true,
                addEmployeeGroupInfo: true,
                addPayrollGroupInfo: loadPayrollGroups,
                loadEmploymentVacactionGroup: true,
                loadUser: true,
                addAnnualLeaveGroupInfo: loadAnnualLeaveGroups);

            if (employees.IsNullOrEmpty())
                return new List<EmployeeGridDTO>();

            if (setAge)
                employees.ForEach(e => e.Age = GetEmployeeAge(e));

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<VacationGroup> vacationGroups = GetVacationGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId), setTypeName: true);
            List<EmploymentTypeDTO> employmentTypes = GetEmploymentTypes(base.ActorCompanyId);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_SocialSec_Show, Permission.Readonly, roleId, actorCompanyId) && FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, roleId, actorCompanyId);

            return employees.ToGridDTOs(date, showSocialSec, vacationGroups, employmentTypes).ToList();
        }

        public List<EmployeeSmallDTO> GetEmployeesForGridSmall(int actorCompanyId, int userId, int roleId, DateTime date, List<int> employeeFilter = null, bool showInactive = false, bool showEnded = false, bool showNotStarted = false)
        {
            List<Employee> employees = GetEmployeesForUsersAttestRoles(
                out _,
                actorCompanyId, userId, roleId,
                dateFrom: date, dateTo: date,
                employeeFilter: employeeFilter,
                discardCache: true,
                active: !showInactive ? true : (bool?)null,
                includeEnded: showEnded,
                includeNotStarted: showNotStarted,
                discardLimitSettingForEnded: true, //Show all ended validEmployees, no matter when they ended
                useShowOtherEmployeesPermission: true);

            return employees.ToSmallDTOs().ToList();
        }

        public Dictionary<int, string> GetEmployeesForGridDict(int actorCompanyId, int userId, int roleId, DateTime dateFrom, DateTime dateTo, List<int> employeeFilter = null, bool showInactive = false, bool showEnded = false, bool showNotStarted = false, bool addEmptyRow = false, bool concatNrAndName = true, bool showInactiveSymbol = false, bool filterOnAnnualLeaveAgreement = false)
        {
            List<Employee> employees = GetEmployeesForUsersAttestRoles(
                out _,
                actorCompanyId, userId, roleId,
                dateFrom: dateFrom, dateTo: dateTo,
                employeeFilter: employeeFilter,
                discardCache: true,
                active: !showInactive ? true : (bool?)null,
                includeEnded: showEnded,
                includeNotStarted: showNotStarted,
                discardLimitSettingForEnded: true, //Show all ended validEmployees, no matter when they ended
                useShowOtherEmployeesPermission: true,
                addAnnualLeaveGroupInfo: filterOnAnnualLeaveAgreement);

            if (filterOnAnnualLeaveAgreement)
                employees = employees.Where(e => e.AnnualLeaveGroupNames?.Any() ?? false).ToList();

            return employees.ToDict(addEmptyRow, concatNrAndName, showInactiveSymbol);
        }

        public List<AttestEmployeeListDTO> GetEmployeesForTree(List<int> filterIds, int timePeriodId)
        {
            TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(timePeriodId, base.ActorCompanyId, loadTimePeriodHead: true);
            if (timePeriod == null)
                return new List<AttestEmployeeListDTO>();

            return GetEmployeesForTree(filterIds, timePeriod.StartDate, timePeriod.StopDate);
        }

        public List<AttestEmployeeListDTO> GetEmployeesForTree(List<int> filterIds, DateTime dateFrom, DateTime dateTo)
        {
            List<AttestEmployeeListDTO> employeeList;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId);
            if (useAccountHierarchy)
                employeeList = GetEmployeesForTreeByAccount(dateFrom, dateTo, filterIds);
            else
                employeeList = GetEmployeesForTreeByCategories(dateFrom, dateTo, filterIds);

            return employeeList.OrderBy(e => e.Name).ToList();
        }

        public Dictionary<int, string> GetEmployeesWithoutUsersDict(int actorCompanyId, int userId, int roleId, bool? active, bool addEmptyRow, bool concatNumberAndName)
        {
            List<Employee> employees = GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, active: active);
            employees = employees.Where(i => !i.UserId.HasValue).ToList();

            return employees.ToDict(addEmptyRow, concatNumberAndName);
        }

        public Dictionary<int, string> GetEmployeesDictForProject(int actorCompanyId, int userId, int roleId, bool addEmptyRow, bool getHidden, bool addNoReplacementEmployee, int? includeEmployeeId)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");
            if (addNoReplacementEmployee)
                dict.Add(Constants.NO_REPLACEMENT_EMPLOYEEID, GetText(8262, "Ingen ersättare"));

            List<Employee> employees = GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, getHidden: getHidden);
            if (includeEmployeeId.HasValue && !employees.Any(i => i.EmployeeId == includeEmployeeId.Value))
            {
                Employee employee = GetEmployee(includeEmployeeId.Value, actorCompanyId, onlyActive: true);
                if (employee != null)
                    employees.Add(employee);
            }

            foreach (Employee employee in employees)
            {
                if (!dict.ContainsKey(employee.EmployeeId))
                    dict.Add(employee.EmployeeId, employee.EmployeeNrAndName);
            }

            return dict;
        }

        public List<EmployeeTimeCodeDTO> GetEmployeesForProjectWithTimeCode(int actorCompanyId, int userId, int roleId, bool addEmptyRow, bool getHidden, bool addNoReplacementEmployee, int? includeEmployeeId, DateTime? dateFrom = null, DateTime? dateTo = null, bool? active = true)
        {
            var dtos = new List<EmployeeTimeCodeDTO>();

            if (addEmptyRow)
                dtos.Add(new EmployeeTimeCodeDTO() { EmployeeId = 0, EmployeeNr = " ", Name = " ", DefaultTimeCodeId = 0 });
            if (addNoReplacementEmployee)
                dtos.Add(new EmployeeTimeCodeDTO() { EmployeeId = Constants.NO_REPLACEMENT_EMPLOYEEID, EmployeeNr = " ", Name = GetText(8262, "Ingen ersättare"), DefaultTimeCodeId = 0 });

            var extendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, actorCompanyId, 0);

            List<Employee> employees = GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, active: active, getHidden: getHidden, addEmployeeGroupInfo: extendedTimeRegistration, loadEmployment: true);
            if (extendedTimeRegistration)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
                DateTime usedPayrollSince = SettingManager.GetDateSetting(SettingMainType.Company, (int)CompanySettingType.UsedPayrollSince, userId, actorCompanyId, 0);
                if (dateTo.HasValue && dateTo.Value > usedPayrollSince)
                {
                    employees = employees.Where(e1 => e1.GetEmployeeGroup(dateFrom ?? usedPayrollSince, dateTo.Value, employeeGroups, forward: false)?.IsTimeReportTypeERP() == true).ToList();
                }
                else if (DateTime.Today.Date > usedPayrollSince)
                {
                    employees = employees.Where(e1 => e1.GetEmployeeGroup(null, employeeGroups)?.IsTimeReportTypeERP() == true).ToList();
                }
            }

            if (includeEmployeeId.HasValue && !employees.Any(i => i.EmployeeId == includeEmployeeId.Value))
            {
                Employee employee = GetEmployee(includeEmployeeId.Value, actorCompanyId, loadEmployment: true, loadContactPerson: true, onlyActive: true);
                if (employee != null)
                    employees.Add(employee);
            }

            foreach (Employee employee in employees.OrderByDescending(e => e.Hidden).ThenBy(e => e.Vacant).ThenBy(e => e.Name))
            {
                if (dateFrom.HasValue && dateTo.HasValue)
                {
                    var employeement = employee.GetEmployment(dateFrom.Value, dateTo.Value);
                    if (employeement == null)
                    {
                        continue;
                    }
                }

                if (employee.TimeCodeId.HasValue && employee.ProjectDefaultTimeCodeId == 0)
                    employee.ProjectDefaultTimeCodeId = employee.TimeCodeId.Value;

                if (employee.ProjectDefaultTimeCodeId == 0)
                    employee.ProjectDefaultTimeCodeId = TimeCodeManager.GetDefaultTimeCodeId(base.ActorCompanyId, employee.EmployeeId);

                if (!employee.TimeDeviationCauseId.HasValue)
                    employee.TimeDeviationCauseId = TimeDeviationCauseManager.GetTimeDeviationCauseIdFromPrio(employee);

                dtos.Add(new EmployeeTimeCodeDTO() { EmployeeId = employee.EmployeeId, EmployeeNr = employee.EmployeeNr, Name = employee.Name, DefaultTimeCodeId = employee.ProjectDefaultTimeCodeId, TimeDeviationCauseId = employee.TimeDeviationCauseId });
            }

            return dtos;
        }

        #endregion

        #region Employees by Accounts

        public List<T> GetValidEmployeeByAccountHierarchy<T>(
            CompEntities entities,
            int actorCompanyId,
            int roleId,
            int userId,
            List<T> employees,
            Employee currentEmployee,
            DateTime dateFrom,
            DateTime dateTo,
            bool includeEnded = false,
            bool includeNotStarted = false,
            bool discardLimitSettingForEnded = false,
            bool getHidden = false,
            bool useShowOtherEmployeesPermission = false,
            bool addAccountHierarchyInfo = false,
            bool? onlyDefaultAccounts = null,
            bool discardCache = false,
            bool includeEmployeesWithSameAccountOnAttestRole = false,
            bool includeEmployeesWithSameAccount = false,
            bool ignoreAttestRoles = false,
            bool isReportSelection = false,
            List<int> accountIds = null,
            AccountHierarchyInput input = null
            )
        {
            return GetValidEmployeesByAccountHierarchy(
                entities,
                out _,
                actorCompanyId,
                roleId,
                userId,
                employees,
                currentEmployee,
                dateFrom,
                dateTo,
                includeEnded,
                includeNotStarted,
                discardLimitSettingForEnded,
                getHidden,
                useShowOtherEmployeesPermission,
                addAccountHierarchyInfo,
                onlyDefaultAccounts,
                discardCache,
                includeEmployeesWithSameAccountOnAttestRole,
                includeEmployeesWithSameAccount,
                ignoreAttestRoles,
                isReportSelection,
                accountIds,
                input
                );
        }

        public List<T> GetValidEmployeesByAccountHierarchy<T>(
            CompEntities entities,
            out AccountRepository accountRepository,
            int actorCompanyId,
            int roleId,
            int userId,
            List<T> employees,
            Employee currentUserEmployee,
            DateTime dateFrom,
            DateTime dateTo,
            bool includeEnded = false,
            bool includeNotStarted = false,
            bool discardLimitSettingForEnded = false,
            bool getHidden = false,
            bool useShowOtherEmployeesPermission = false,
            bool addAccountInfo = false,
            bool? onlyDefaultAccounts = null,
            bool discardCache = false,
            bool includeEmployeesWithSameAccountOnAttestRole = false,
            bool includeEmployeesWithSameAccount = false,
            bool ignoreAttestRoles = false,
            bool isReportSelection = false,
            List<int> filteredAccountIds = null,
            AccountHierarchyInput input = null
            )
        {
            if (employees.IsNullOrEmpty())
            {
                accountRepository = null;
                return new List<T>();
            }

            DateTime employeeStartDate = GetEmployeeStartDateByEmployeeeEndedSetting(entities, includeEnded, dateFrom, discardLimitSettingForEnded);
            DateTime employeeStopDate = includeNotStarted ? DateTime.MaxValue : dateTo;

            accountRepository = AccountManager.GetAccountHierarchyRepositoryByUserSetting(entities, actorCompanyId, roleId, userId, dateFrom, dateTo, input, ignoreAttestRoles);
            if (accountRepository == null)
            {
                accountRepository = AccountManager.CreateAccountRepository(entities, actorCompanyId, userId, dateFrom, dateTo, input, ignoreAttestRoles, addAccountInfo, discardCache);
            }
            else
            {
                if (accountRepository.EmployeeAccounts.IsNullOrEmpty())
                    accountRepository.SetEmployeeAccounts(GetEmployeeAccountsFromCache(entities, CacheConfig.Company(actorCompanyId, 60)));
                accountRepository.SetAddAccountInfo(addAccountInfo);
            }

            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(actorCompanyId));
            List<EmployeeAccount> currentUserEmployeeAccounts = currentUserEmployee != null ? accountRepository.GetEmployeeAccounts(currentUserEmployee.EmployeeId, dateFrom, dateTo) : null;
            onlyDefaultAccounts = onlyDefaultAccounts ?? !(accountRepository.AttestRoleUsers != null && accountRepository.AttestRoleUsers.ShowAll(employeeStartDate));

            bool hasShowOtherEmployeesPermission =
                useShowOtherEmployeesPermission &&
                accountRepository.AttestRoleUsers.IsNullOrEmpty() &&
                FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees, Permission.Readonly, roleId, actorCompanyId, entities: entities);

            List<int> validEmployeeIdsByAttestRoleUserAccountIds = new List<int>();
            if (includeEmployeesWithSameAccountOnAttestRole && !currentUserEmployeeAccounts.IsNullOrEmpty() && (hasShowOtherEmployeesPermission || includeEmployeesWithSameAccount))
            {
                // No attest roles, get by emp account
                List<int> currentEmployeeAccountIds = currentUserEmployeeAccounts.Where(a => a.AccountId.HasValue).Select(a => a.AccountId.Value).ToList();
                validEmployeeIdsByAttestRoleUserAccountIds = GetEmployeeIdsByAttestRoleUserAccountIds(entities, currentEmployeeAccountIds);
            }

            Dictionary<int, T> validEmployeesDict = new Dictionary<int, T>();

            foreach (T employee in employees)
            {
                int employeeId = 0;
                if (employee is Employee emp)
                    employeeId = emp.EmployeeId;
                else if (employee is int id)
                    employeeId = id;

                List<EmployeeAccount> employeeAccounts = accountRepository.GetEmployeeAccounts(employeeId, employeeStartDate, employeeStopDate, onlyDefaultAccounts.Value);

                if (!accountRepository.IsUserPermittedToSeeEmployee(
                    employeeId,
                    employeeAccounts,
                    currentUserEmployee,
                    currentUserEmployeeAccounts,
                    employeeStartDate,
                    employeeStopDate,
                    validEmployeeIdsByAttestRoleUserAccountIds,
                    hasShowOtherEmployeesPermission,
                    onlyDefaultAccounts ?? false,
                    getHidden,
                    hiddenEmployeeId
                    )
                )
                    continue; //Not permitted to see employee

                // Check filter
                if (!filteredAccountIds.IsNullOrEmpty() && !employeeAccounts.HasAnyValidAccount(
                    employeeId,
                    startDate: employeeStartDate,
                    stopDate: employeeStopDate,
                    filteredAccountIds: filteredAccountIds,
                    possibleValidAccounts: accountRepository.GetAccounts(true),
                    allAccountDims: accountRepository.AllAccountDims,
                    allAccountInternals: accountRepository.AllAccountInternals,
                    onlyDefaultAccounts: onlyDefaultAccounts.Value,
                    doNotValidateAsHiearchy: isReportSelection && base.IsMartinServera(entities))
                )
                    continue; //Invalid by filter

                accountRepository.AddEmployeeAuthInfo(employeeId, employeeAccounts, dateFrom, dateTo);
                validEmployeesDict.Add(employeeId, employee);
            }

            if (validEmployeesDict.Any())
                accountRepository.TrimEmployeeAccounts(validEmployeesDict.Keys.ToList());

            return validEmployeesDict.Values.ToList();
        }

        public List<int> GetEmployeeIdsByAttestRoleUserAccountIds(CompEntities entities, List<int> accountIds, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            List<int> userIds = (from a in entities.AttestRoleUser
                                 where a.State == (int)SoeEntityState.Active &&
                                 a.AccountId.HasValue &&
                                 accountIds.Contains(a.AccountId.Value) &&
                                 (!a.DateFrom.HasValue || a.DateFrom <= date.Value) &&
                                 (!a.DateTo.HasValue || a.DateTo >= date.Value)
                                 select a.UserId).ToList();

            return (from e in entities.Employee where e.UserId.HasValue && userIds.Contains(e.UserId.Value) && e.State == (int)SoeEntityState.Active select e.EmployeeId).ToList();
        }

        public List<int> GetEmployeeIdsByAccount(int actorCompanyId, List<int> accountIds, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (!dateFrom.HasValue)
                dateFrom = DateTime.Today;
            if (!dateTo.HasValue)
                dateTo = DateTime.Today;

            // Get validEmployees with specified account
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from ea in entitiesReadOnly.EmployeeAccount
                    where ea.ActorCompanyId == actorCompanyId &&
                    ea.AccountId.HasValue && accountIds.Contains(ea.AccountId.Value) &&
                    ea.State == (int)SoeEntityState.Active &&
                    ea.DateFrom <= dateTo.Value &&
                    (!ea.DateTo.HasValue || ea.DateTo.Value >= dateFrom.Value)
                    select ea.EmployeeId).ToList();
        }

        public List<AttestEmployeeListDTO> GetEmployeesForTreeByAccount(DateTime dateFrom, DateTime dateTo, List<int> accountIds)
        {
            List<int> employeeIdsForCompany = GetAllEmployeeIds(base.ActorCompanyId, active: true);
            if (employeeIdsForCompany.IsNullOrEmpty())
                return new List<AttestEmployeeListDTO>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            List<int> validEmployeeIds = GetValidEmployeesByAccountHierarchy(
                entitiesReadOnly,
                out AccountRepository accountRepository,
                base.ActorCompanyId,
                base.RoleId,
                base.UserId,
                employees: employeeIdsForCompany,
                currentUserEmployee: GetEmployeeForUser(base.UserId, base.ActorCompanyId),
                dateFrom: dateFrom,
                dateTo: dateTo,
                input: AccountHierarchyInput.GetInstance(AccountHierarchyParamType.UseDefaultEmployeeAccountDimEmployee)
                );
            if (validEmployeeIds.IsNullOrEmpty() || accountRepository == null)
                return new List<AttestEmployeeListDTO>();

            List<Employee> validEmployees = GetAllEmployeesByIds(base.ActorCompanyId, validEmployeeIds, getVacant: false);
            if (validEmployees.IsNullOrEmpty())
                return new List<AttestEmployeeListDTO>();

            ConcurrentBag<AttestEmployeeListDTO> dtos = new ConcurrentBag<AttestEmployeeListDTO>();

            List<AccountDTO> accountsForAttestRole = accountRepository.GetAccounts(true);
            List<AccountDTO> accounts = accountsForAttestRole.GetFilteredAccounts(accountIds);

            if (accountRepository.EmployeeAccounts.IsNullOrEmpty())
                accountRepository.SetEmployeeAccounts(GetEmployeeAccountsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId, 60)));

            bool hasAccountIds = !accountIds.IsNullOrEmpty();

            Parallel.ForEach(validEmployees, GetDefaultParallelOptions(), employee =>
            {
                if (accountRepository.EmployeeAccounts.ContainsKey(employee.EmployeeId))
                {
                    List<EmployeeAccount> employeeAccountsForEmployee = accountRepository.EmployeeAccounts.GetList(employee.EmployeeId);
                    List<AccountDTO> validAccounts = employeeAccountsForEmployee.GetValidAccounts(employee.EmployeeId, dateFrom, dateTo, accountRepository.AllAccountInternals, accountsForAttestRole, onlyDefaultAccounts: true);

                    bool isValid = hasAccountIds ? accounts.Any(account => validAccounts.Any(validAccount => validAccount.HierachyId == account.HierachyId)) : validAccounts.Any();
                    if (isValid)
                    {
                        dtos.Add(new AttestEmployeeListDTO()
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeNr = employee.EmployeeNr,
                            Name = employee.Name,
                        });
                    }
                }
            });

            return dtos.ToList();
        }

        public bool IsEmployeeAccountValid(EmployeeAccount empAccount, DateTime dateFrom, DateTime dateTo)
        {
            // Check default interval
            if (empAccount.DateFrom <= dateTo && (!empAccount.DateTo.HasValue || empAccount.DateTo.Value >= dateFrom))
            {
                // Check children
                if (empAccount.Children.IsNullOrEmpty())
                {
                    // No children, parent was valid so it's OK
                    return true;
                }
                else
                {
                    // Recursively check each child account
                    // If one is valid it's OK
                    bool childValid = false;
                    foreach (EmployeeAccount child in empAccount.Children)
                    {
                        if (this.IsEmployeeAccountValid(child, dateFrom, dateTo))
                        {
                            childValid = true;
                            break;
                        }
                    }
                    if (childValid)
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region Employees by Categories

        public List<T> GetValidEmployeesByCategories<T>(CompEntities entities, out CategoryRepository categoryRepository, int actorCompanyId, int roleId, int userId, List<T> employeesForCompany, Employee currentEmployee, DateTime dateFrom, DateTime dateTo, List<int> categoryIds = null, bool? onlyDefaultCategories = null, bool includeEnded = false, bool includeNotStarted = false, bool discardLimitSettingForEnded = false, bool getHidden = false, bool useShowOtherEmployeesPermission = false, bool addCategoryInfo = false, bool discardCache = false)
        {
            if (employeesForCompany.IsNullOrEmpty())
            {
                categoryRepository = null;
                return new List<T>();
            }

            List<T> validEmployees = new List<T>();
            DateTime employeeStartDate = GetEmployeeStartDateByEmployeeeEndedSetting(entities, includeEnded, dateFrom, discardLimitSettingForEnded);
            DateTime employeeStopDate = includeNotStarted ? DateTime.MaxValue : dateTo;

            //Tracker
            categoryRepository = new CategoryRepository(
                AttestManager.GetAttestRoleUsers(entities, actorCompanyId, userId, dateFrom, dateTo, includeAttestRole: true),
                GetCompanyCategoryRecordsFromCache(entities, CacheConfig.Company(actorCompanyId, discardCache: discardCache)),
                addCategoryInfo
            );

            bool showOtherEmployeesPermission = useShowOtherEmployeesPermission && categoryRepository.GetAttestRolesForUser().IsNullOrEmpty() && FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees, Permission.Readonly, roleId, actorCompanyId);
            using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entitiesReadonly, CacheConfig.Company(actorCompanyId));

            Dictionary<int, List<CompanyCategoryRecord>> categoryRecordsByEmployeeDict = categoryRepository.CategoryRecords.GetCategoryRecordsDiscardDates(SoeCategoryRecordEntity.Employee, onlyDefaultCategories ?? false).ToDict();
            List<CompanyCategoryRecord> categoryRecordsForCurrentEmployee = currentEmployee != null && categoryRecordsByEmployeeDict.ContainsKey(currentEmployee.EmployeeId) ? categoryRecordsByEmployeeDict[currentEmployee.EmployeeId].GetCategoryRecords(SoeCategoryRecordEntity.Employee, currentEmployee.EmployeeId, dateFrom, dateTo) : new List<CompanyCategoryRecord>();

            foreach (T employee in employeesForCompany)
            {
                int employeeId = 0;
                if (employee is Employee employeeEntity)
                    employeeId = employeeEntity.EmployeeId;
                else if (employee is int)
                    employeeId = Convert.ToInt32(employee);

                bool isCurrentEmployee = currentEmployee != null && currentEmployee.EmployeeId == employeeId;
                bool isHiddenEmployee = getHidden && hiddenEmployeeId == employeeId;
                List<CompanyCategoryRecord> categoryRecordsForEmployee = categoryRecordsByEmployeeDict.GetCategoryRecords(employeeId, employeeStartDate, employeeStopDate);

                bool valid =
                    isCurrentEmployee ||
                    isHiddenEmployee ||
                    categoryRepository.HasAnyAttestRoleAnyCategory(categoryRecordsForEmployee);

                //Check otherEmployeesPermission
                if (!valid && showOtherEmployeesPermission)
                    valid = categoryRecordsForCurrentEmployee.ContainsAny(categoryRecordsForEmployee);

                //Check filter
                if (valid && !categoryIds.IsNullOrEmpty() && !categoryRecordsByEmployeeDict.HasAnyCategory(employeeId, categoryIds, employeeStartDate, employeeStopDate))
                    valid = false;

                if (valid)
                {
                    categoryRepository.AddEmployeeAuthInfo(employeeId, categoryRecordsForEmployee);
                    validEmployees.Add(employee);
                }
            }

            return validEmployees;
        }

        public List<Employee> GetEmployeesByCategory(int categoryId, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeesByCategory(entities, categoryId, actorCompanyId, dateFrom, dateTo);
        }

        public List<Employee> GetEmployeesByCategory(CompEntities entities, int categoryId, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            List<Employee> employees = new List<Employee>();

            Category category = CategoryManager.GetCategory(entities, categoryId, actorCompanyId, loadCategoryRecords: true);
            if (category != null)
            {
                //Filter dates
                List<CompanyCategoryRecord> categoryRecords = category.CompanyCategoryRecord.GetCategoryRecords(dateFrom, dateTo);
                if (categoryRecords.Count > 0)
                {
                    List<int> recordIds = categoryRecords.Select(i => i.RecordId).ToList();

                    employees = (from e in entities.Employee
                                    .Include("ContactPerson")
                                    .Include("User")
                                 where recordIds.Contains(e.EmployeeId) &&
                                 e.ActorCompanyId == actorCompanyId &&
                                 !e.Hidden &&
                                 e.State == (int)SoeEntityState.Active
                                 select e).ToList();
                }
            }

            return employees;
        }

        public List<AttestEmployeeListDTO> GetEmployeesForTreeByCategories(DateTime dateFrom, DateTime dateTo, List<int> categoryIds)
        {
            List<Employee> employeesForCompany = GetAllEmployees(base.ActorCompanyId, active: true);
            if (employeesForCompany.IsNullOrEmpty())
                return new List<AttestEmployeeListDTO>();

            List<CompanyCategoryRecord> categoryRecordsForCompany = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, base.ActorCompanyId);
            if (categoryRecordsForCompany.IsNullOrEmpty())
                return new List<AttestEmployeeListDTO>();

            if (categoryIds == null)
                categoryIds = CategoryManager.GetCategoriesForRoleFromTypeDict(base.ActorCompanyId, base.UserId, 0, SoeCategoryType.Employee, true, true, false).Select(c => c.Key).ToList();

            ConcurrentBag<AttestEmployeeListDTO> employeeList = new ConcurrentBag<AttestEmployeeListDTO>();
            Parallel.ForEach(employeesForCompany, GetDefaultParallelOptions(), employee =>
            {
                bool isValid = true;
                if (!categoryIds.IsNullOrEmpty())
                {
                    isValid = false;
                    foreach (int id in categoryIds)
                    {
                        isValid = categoryRecordsForCompany.GetCategoryRecords(employee.EmployeeId, id, dateFrom, dateTo).Any();
                        if (isValid)
                            break;
                    }
                }

                if (isValid)
                {
                    employeeList.Add(new AttestEmployeeListDTO
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeNr = employee.EmployeeNr,
                        Name = employee.ContactPerson.Name,
                        CategoryRecords = categoryRecordsForCompany.GetCategoryRecords(employee.EmployeeId, discardDateIfEmpty: true).ToDTOs(false).ToList()
                    });
                }
            });

            return employeeList.ToList();
        }

        public Dictionary<int, string> GetEmployeesDictByCategories(int actorCompanyId, List<int> categoryIds, bool addEmptyRow, bool getHidden = false, bool addNoReplacementEmployee = false, int? includeEmployeeId = null)
        {
            Dictionary<int, string> employeeDict = new Dictionary<int, string>();

            if (addEmptyRow)
                employeeDict.Add(0, " ");

            if (addNoReplacementEmployee)
                employeeDict.Add(Constants.NO_REPLACEMENT_EMPLOYEEID, GetText(8262, "Ingen ersättare"));

            // CompanyCategoryRecords for Company
            List<CompanyCategoryRecord> categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, actorCompanyId);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            List<Employee> employees = (from e in entities.Employee
                                            .Include("ContactPerson")
                                        where (!e.Hidden || getHidden) &&
                                        e.ActorCompanyId == actorCompanyId &&
                                        e.State == (int)SoeEntityState.Active
                                        select e).ToList();

            foreach (var employee in employees.OrderByDescending(e => e.Hidden).ThenBy(e => e.Vacant).ThenBy(e => e.Name))
            {
                bool match = true;
                if (!categoryIds.IsNullOrEmpty())
                {
                    if (employee.Hidden && getHidden)
                        match = true; // Get hidden even if not in current category
                    else
                    {
                        match = false;
                        foreach (int categoryId in categoryIds)
                        {
                            match = categoryRecords.GetCategoryRecords(employee.EmployeeId, categoryId).Any();
                            if (match)
                                break;
                        }
                    }
                }

                if (!match && includeEmployeeId.HasValue && includeEmployeeId.Value == employee.EmployeeId)
                    match = true;

                if (match)
                    employeeDict.Add(employee.EmployeeId, employee.EmployeeNrAndName);
            }

            return employeeDict;
        }

        public List<int> GetEmployeeIdsByCategoryIds(int actorCompanyId, IEnumerable<int> categoryIds, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeeIdsByCategoryIds(entities, actorCompanyId, categoryIds, dateFrom, dateTo);
        }

        public List<int> GetEmployeeIdsByCategoryIds(CompEntities entities, int actorCompanyId, IEnumerable<int> categoryIds, DateTime? dateFrom = null, DateTime? dateTo = null, bool? onlyActive = true)
        {
            var query = (from e in entities.Employee
                         join c in entities.CompanyCategoryRecord
                         on e.EmployeeId equals c.RecordId
                         where !e.Hidden &&
                         e.ActorCompanyId == actorCompanyId &&
                         e.State != (int)SoeEntityState.Deleted &&
                         c.Entity == (int)SoeCategoryRecordEntity.Employee &&
                         categoryIds.Contains(c.CategoryId) &&
                         (!c.DateFrom.HasValue || !dateFrom.HasValue || DbFunctions.TruncateTime(c.DateFrom.Value) <= dateFrom) &&
                         (!c.DateTo.HasValue || !dateTo.HasValue || DbFunctions.TruncateTime(c.DateTo.Value) >= dateTo)
                         select e);

            if (onlyActive == true)
                query = query.Where(e => e.State == (int)SoeEntityState.Active);

            return query.Select(e => e.EmployeeId).ToList();
        }

        public List<int> GetEmployeeCategoryIds(int employeeId, int actorCompanyId, bool onlyDefaultCategories = false, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyCategoryRecord.NoTracking();
            return GetEmployeeCategoryIds(entities, employeeId, actorCompanyId, onlyDefaultCategories, dateFrom, dateTo);
        }

        public List<int> GetEmployeeCategoryIds(CompEntities entities, int employeeId, int actorCompanyId, bool onlyDefaultCategories = false, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            List<CompanyCategoryRecord> categoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employeeId, actorCompanyId, onlyDefaultCategories, dateFrom, dateTo);
            List<int> categoryIds = categoryRecords.Select(i => i.CategoryId).ToList();
            return categoryIds;
        }

        #endregion

        #region Linq rules

        public bool UseEmployeeIdsInQuery(CompEntities entities, List<int> employeeIds)
        {
            return employeeIds != null && UseEmployeeIdsInQuery(entities, employeeIds.Count);
        }

        public bool UseEmployeeIdsInQuery(CompEntities entities, int totalEmployees)
        {
            if (totalEmployees <= Constants.LINQMAXCOUNTCONTAINS)
                return true;
            if (totalEmployees <= Constants.LINQMAXCOUNTCONTAINS_EXTENDED && base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId))
                return true;
            return false;
        }

        public int GetEmployeeIdsForQueryLength(CompEntities entities)
        {
            return base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId)
                ?
                Constants.LINQMAXCOUNTCONTAINS_EXTENDED
                :
                Constants.LINQMAXCOUNTCONTAINS;
        }

        public List<int> GetEmployeeIdsForQuery(List<int> employeeIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeIdsForQuery(entities, employeeIds);
        }

        public List<int> GetEmployeeIdsForQuery(CompEntities entities, List<int> employeeIds)
        {
            return employeeIds != null && UseEmployeeIdsInQuery(entities, employeeIds.Count) ? employeeIds : null;
        }

        public List<int> GetEmployeeIdsForQuery(CompEntities entities, IEnumerable<IEmployee> employees)
        {
            return employees != null && UseEmployeeIdsInQuery(entities, employees.Count()) ? employees.Select(i => i.EmployeeId).ToList() : null;
        }

        #endregion

        public Tuple<int, int> GetEmployeeIdAndGroupIdForUser(int userId, int actorCompanyId, bool getHidden = false)
        {
            int employeeGroupId = 0;

            int employeeId = GetEmployeeIdForUser(userId, actorCompanyId, getHidden);
            if (employeeId != 0)
            {
                EmployeeGroup employeeGroup = GetEmployeeGroupForEmployee(employeeId, actorCompanyId, DateTime.Today);
                if (employeeGroup != null)
                    employeeGroupId = employeeGroup.EmployeeGroupId;
            }

            return new Tuple<int, int>(employeeId, employeeGroupId);
        }

        public List<Employee> GetEmployees(int actorCompanyId, List<int> employeeIds, bool onlyActive = true, bool loadEmployment = false, bool loadContactPerson = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployees(entities, actorCompanyId, employeeIds, onlyActive, loadEmployment, loadContactPerson);
        }

        public List<Employee> GetEmployees(CompEntities entities, int actorCompanyId, List<int> employeeIds, bool onlyActive = true, bool loadEmployment = false, bool loadContactPerson = false)
        {
            List<Employee> employees = new List<Employee>();
            foreach (int employeeId in employeeIds)
            {
                Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, onlyActive: onlyActive, loadEmployment: loadEmployment, loadContactPerson: loadContactPerson);
                if (employee != null)
                    employees.Add(employee);
            }
            return employees;
        }

        public Employee GetEmployee(int employeeId, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyActive = true, bool loadEmployment = false, bool loadVacationGroup = false, bool loadContactPerson = false, bool loadUser = false, bool loadTimeDeviationCause = false, bool loadTimeCode = false, bool loadEmployeeTax = false, bool getHidden = false, bool loadEmployeeAccount = false, bool setHibernatingText = false, bool loadEmployeeVacation = false, bool loadEmployeeSkill = false, bool loadEmployeeSetting = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployee(entities, employeeId, actorCompanyId, dateFrom, dateTo, onlyActive, loadEmployment, loadVacationGroup, loadContactPerson, loadUser, loadTimeDeviationCause, loadTimeCode, loadEmployeeTax, getHidden, loadEmployeeAccount, setHibernatingText, loadEmployeeVacation, loadEmployeeSkill, loadEmployeeSetting);
        }

        public Employee GetEmployee(CompEntities entities, int employeeId, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyActive = true, bool loadEmployment = false, bool loadVacationGroup = false, bool loadContactPerson = false, bool loadUser = false, bool loadTimeDeviationCause = false, bool loadTimeCode = false, bool loadEmployeeTax = false, bool getHidden = false, bool loadEmployeeAccount = false, bool setHibernatingText = false, bool loadEmployeeVacation = false, bool loadEmployeeSkill = false, bool loadEmployeeSetting = false)
        {
            IQueryable<Employee> query = entities.Employee;
            if (loadEmployment)
            {
                query = query.Include("Employment.EmploymentChangeBatch.EmploymentChange")
                             .Include("Employment.OriginalEmployeeGroup")
                             .Include("Employment.OriginalPayrollGroup")
                             .Include("Employment.OriginalAnnualLeaveGroup");

                if (loadVacationGroup)
                    query = query.Include(e => e.Employment.Select(em => em.EmploymentVacationGroup.Select(evg => evg.VacationGroup.VacationGroupSE)));
            }
            if (loadContactPerson)
                query = query.Include("ContactPerson.Actor");
            if (loadUser)
                query = query.Include("User");
            if (loadTimeDeviationCause)
                query = query.Include("TimeDeviationCause");
            if (loadTimeCode)
                query = query.Include("TimeCode");
            if (loadEmployeeTax)
                query = query.Include("EmployeeTaxSE");
            if (loadEmployeeAccount)
                query = query.Include("EmployeeAccount");
            if (loadEmployeeVacation)
                query = query.Include("EmployeeVacationSE");
            if (loadEmployeeSkill)
                query = query.Include("EmployeeSkill.Skill");
            if (loadEmployeeSetting)
                query = query.Include("EmployeeSetting");

            query = (from e in query
                     where e.EmployeeId == employeeId &&
                     e.ActorCompanyId == actorCompanyId &&
                     (!e.Hidden || getHidden) &&
                     e.State != (int)SoeEntityState.Deleted
                     select e);

            if (onlyActive)
                query = query.Where(e => e.State == (int)SoeEntityState.Active);

            Employee employee = query.FirstOrDefault();
            if (employee != null && setHibernatingText && dateFrom.HasValue && dateTo.HasValue)
                SetEmployeeHibernatingText(employee, dateFrom.Value, dateTo.Value);

            return employee;
        }

        public Employee GetEmployeeWithEmploymentAndEmploymentChangeBatch(int employeeId, int actorCompanyId, bool loadContactPerson = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeeWithEmploymentAndEmploymentChangeBatch(entities, employeeId, actorCompanyId, loadContactPerson);
        }

        public Employee GetEmployeeWithEmploymentAndEmploymentChangeBatch(CompEntities entities, int employeeId, int actorCompanyId, bool loadContactPerson = false)
        {
            IQueryable<Employee> query = entities.Employee
                .Include("Employment.EmploymentChangeBatch.EmploymentChange")
                .Include("Employment.OriginalEmployeeGroup")
                .Include("Employment.OriginalPayrollGroup")
                .Include("Employment.OriginalAnnualLeaveGroup");

            if (loadContactPerson)
                query = query.Include("ContactPerson.Actor");

            query = (from e in query
                     where e.EmployeeId == employeeId &&
                     e.ActorCompanyId == actorCompanyId
                     select e);

            return query.FirstOrDefault();
        }

        public Employee GetEmployeeByExternalCode(string externalCode, int actorCompanyId, bool onlyActive = true, bool loadEmployment = false, bool loadContactPerson = false, bool loadUser = false, bool loadTimeDeviationCause = false, bool loadTimeCode = false, bool loadFactors = false, bool loadEmployeeVacation = false, bool getHidden = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeeByExternalCode(entities, externalCode, actorCompanyId, onlyActive, loadEmployment, loadContactPerson, loadUser, loadTimeDeviationCause, loadTimeCode, loadFactors, loadEmployeeVacation, getHidden);
        }

        public Employee GetEmployeeByExternalCode(CompEntities entities, string externalCode, int actorCompanyId, bool onlyActive = true, bool loadEmployment = false, bool loadContactPerson = false, bool loadUser = false, bool loadTimeDeviationCause = false, bool loadTimeCode = false, bool loadFactors = false, bool loadEmployeeVacation = false, bool getHidden = false, bool loadAccount = false)
        {
            if (string.IsNullOrEmpty(externalCode))
                return null;

            IQueryable<Employee> query = entities.Employee;
            if (loadEmployment)
                query = query.Include("Employment.EmploymentChangeBatch.EmploymentChange");
            if (loadContactPerson)
                query = query.Include("ContactPerson.Actor");
            if (loadUser)
                query = query.Include("User");
            if (loadTimeDeviationCause)
                query = query.Include("TimeDeviationCause");
            if (loadTimeCode)
                query = query.Include("TimeCode");
            if (loadFactors)
                query = query.Include("EmployeeFactor.VacationGroup");
            if (loadEmployeeVacation)
                query = query.Include("EmployeeVacationSE");
            if (loadAccount)
                query = query.Include("EmployeeAccount.Account.AccountInternal");

            var employees = (from e in query
                             where e.ExternalCode == externalCode &&
                             e.ActorCompanyId == actorCompanyId &&
                             (!e.Hidden || getHidden) &&
                             ((!onlyActive || e.State == (int)SoeEntityState.Active) &&
                             e.State != (int)SoeEntityState.Deleted)
                             select e).ToList();

            if (employees.Count > 1)
                LogInfo($"Multiple employees found with external code: {externalCode} on actorCompanyId {actorCompanyId}");

            return employees.FirstOrDefault();
        }

        public Employee GetEmployeeByNr(string employeeNr, int actorCompanyId, bool onlyActive = true, bool loadEmployment = false, bool loadContactPerson = false, bool loadUser = false, bool loadTimeDeviationCause = false, bool loadTimeCode = false, bool loadFactors = false, bool loadEmployeeVacation = false, bool getHidden = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeeByNr(entities, employeeNr, actorCompanyId, onlyActive, loadEmployment, loadContactPerson, loadUser, loadTimeDeviationCause, loadTimeCode, loadFactors, loadEmployeeVacation, getHidden);
        }

        public Employee GetEmployeeByNr(CompEntities entities, string employeeNr, int actorCompanyId, bool onlyActive = true, bool loadEmployment = false, bool loadContactPerson = false, bool loadUser = false, bool loadTimeDeviationCause = false, bool loadTimeCode = false, bool loadFactors = false, bool loadEmployeeVacation = false, bool getHidden = false, bool loadAccount = false)
        {
            if (string.IsNullOrEmpty(employeeNr))
                return null;

            IQueryable<Employee> query = entities.Employee;
            if (loadEmployment)
                query = query.Include("Employment.EmploymentChangeBatch.EmploymentChange");
            if (loadContactPerson)
                query = query.Include("ContactPerson.Actor");
            if (loadUser)
                query = query.Include("User");
            if (loadTimeDeviationCause)
                query = query.Include("TimeDeviationCause");
            if (loadTimeCode)
                query = query.Include("TimeCode");
            if (loadFactors)
                query = query.Include("EmployeeFactor.VacationGroup");
            if (loadEmployeeVacation)
                query = query.Include("EmployeeVacationSE");
            if (loadAccount)
                query = query.Include("EmployeeAccount.Account.AccountInternal");

            return (from e in query
                    where e.EmployeeNr == employeeNr &&
                    e.ActorCompanyId == actorCompanyId &&
                    (!e.Hidden || getHidden) &&
                    ((!onlyActive || e.State == (int)SoeEntityState.Active) &&
                    e.State != (int)SoeEntityState.Deleted)
                    select e).FirstOrDefault();
        }

        public Employee GetEmployeeBySocialSec(string socialSec, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeeBySocialSec(entities, socialSec, actorCompanyId);
        }

        public Employee GetEmployeeBySocialSec(CompEntities entities, string socialSec, int actorCompanyId)
        {
            return (from e in entities.Employee
                    where e.ActorCompanyId == actorCompanyId &&
                    e.SocialSec.Replace("-", "") == socialSec.Replace("-", "")
                    select e).FirstOrDefault();
        }

        public Employee GetEmployeeByUser(int actorCompanyId, int userId, bool loadUser = false, bool loadContactPerson = false, bool loadEmployeeAccount = false, bool ignoreState = false, bool loadEmployment = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeeByUser(entities, actorCompanyId, userId, loadUser, loadContactPerson, loadEmployeeAccount, ignoreState, loadEmployment);
        }

        public Employee GetEmployeeByUser(CompEntities entities, int actorCompanyId, int userId, bool loadUser = false, bool loadContactPerson = false, bool loadEmployeeAccount = false, bool ignoreState = false, bool loadEmployment = false)
        {
            IQueryable<Employee> query = entities.Employee;
            if (loadUser)
                query = query.Include("User");
            if (loadContactPerson)
                query = query.Include("ContactPerson");
            if (loadEmployeeAccount)
                query = query.Include("EmployeeAccount");
            if (loadEmployment)
            {
                query = query.Include("Employment.EmploymentChangeBatch.EmploymentChange");
                query = query.Include("Employment.OriginalEmployeeGroup");
                query = query.Include("Employment.OriginalPayrollGroup");
                query = query.Include("Employment.OriginalAnnualLeaveGroup");
            }

            return (from e in query
                    where e.ActorCompanyId == actorCompanyId &&
                    e.UserId.HasValue &&
                    e.UserId.Value == userId &&
                    (e.State == (int)SoeEntityState.Active || ignoreState)
                    select e).FirstOrDefault();
        }

        public Employee GetEmployeeIgnoreState(int actorCompanyId, int employeeId,
            bool loadEmployment = false,
            bool loadEmploymentAccounting = false,
            bool loadEmploymentPriceType = false,
            bool loadEmploymentVacationGroupSE = false,
            bool loadContactPerson = false,
            bool loadUser = false,
            bool loadTimeCode = false,
            bool loadTimeDeviationCause = false,
            bool loadEmployeeAccounts = false,
            bool loadEmployeeFactors = false,
            bool loadEmployeeSettings = false,
            bool loadEmployeeSkill = true,
            bool loadEmployeeTemplate = false,
            bool loadEmployeeVacation = false,
            bool getHidden = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Employee.NoTracking();
            return GetEmployeeIgnoreState(entitiesReadOnly, actorCompanyId, employeeId,
                loadEmployment,
                loadEmploymentAccounting,
                loadEmploymentPriceType,
                loadEmploymentVacationGroupSE,
                loadContactPerson,
                loadUser,
                loadTimeCode,
                loadTimeDeviationCause,
                loadEmployeeAccounts,
                loadEmployeeFactors,
                loadEmployeeSettings,
                loadEmployeeSkill,
                loadEmployeeTemplate,
                loadEmployeeVacation,
                getHidden);
        }

        public Employee GetEmployeeIgnoreState(CompEntities entities, int actorCompanyId, int employeeId,
            bool loadEmployment = false,
            bool loadEmploymentAccounting = false,
            bool loadEmploymentPriceType = false,
            bool loadEmploymentVacationGroupSE = false,
            bool loadContactPerson = false,
            bool loadUser = false,
            bool loadTimeCode = false,
            bool loadTimeDeviationCause = false,
            bool loadEmployeeAccounts = false,
            bool loadEmployeeFactors = false,
            bool loadEmployeeSettings = false,
            bool loadEmployeeSkill = true,
            bool loadEmployeeTemplate = false,
            bool loadEmployeeVacation = false,
            bool getHidden = false)
        {
            IQueryable<Employee> query = entities.Employee;

            if (loadContactPerson)
                query = query.Include("ContactPerson.Actor.ActorConsent");
            if (loadUser)
                query = query.Include("User");
            if (loadTimeCode)
                query = query.Include("TimeCode");
            if (loadTimeDeviationCause)
                query = query.Include("TimeDeviationCause");

            if (loadEmployment)
                query = query.Include("Employment.EmploymentChangeBatch.EmploymentChange")
                             .Include("Employment.OriginalEmployeeGroup")
                             .Include("Employment.OriginalPayrollGroup")
                             .Include("Employment.EmploymentVacationGroup.VacationGroup")
                             .Include("Employment.OriginalAnnualLeaveGroup");
            if (loadEmploymentAccounting)
                query = query.Include("Employment.EmploymentAccountStd.AccountStd.Account.AccountDim")
                             .Include("Employment.EmploymentAccountStd.AccountInternal.Account.AccountDim");
            if (loadEmploymentPriceType)
                query = query.Include("Employment.EmploymentPriceType.PayrollPriceType")
                             .Include("Employment.EmploymentPriceType.EmploymentPriceTypePeriod");
            if (loadEmploymentVacationGroupSE)
                query = query.Include("Employment.EmploymentVacationGroup.VacationGroup.VacationGroupSE");
            if (loadEmployeeAccounts)
                query = query.Include("EmployeeAccount.Children.Children");
            if (loadEmployeeFactors)
                query = query.Include("EmployeeFactor.VacationGroup");
            if (loadEmployeeSettings)
                query = query.Include("EmployeeSetting");
            if (loadEmployeeSkill)
                query = query.Include("EmployeeSkill");
            if (loadEmployeeTemplate)
                query = query.Include("EmployeeTemplate");
            if (loadEmployeeVacation)
                query = query.Include("EmployeeVacationSE");

            return (from e in query
                    where e.EmployeeId == employeeId &&
                    e.ActorCompanyId == actorCompanyId &&
                    (!e.Hidden || getHidden) &&
                    e.State != (int)SoeEntityState.Deleted //Ignore active or inactive, but not deleted
                    select e).FirstOrDefault();
        }

        public Employee GetEmployeeForUser(int userId, int actorCompanyId, bool getHidden = false, bool loadEmployment = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeeForUser(entities, userId, actorCompanyId, getHidden, loadEmployment);
        }

        public Employee GetEmployeeForUser(CompEntities entities, int userId, int actorCompanyId, bool getHidden = false, bool loadEmployment = false)
        {
            Employee employee = null;

            User user = UserManager.GetUser(entities, userId, loadEmployee: true, loadEmployment: loadEmployment);
            if (user != null && user.Employee != null && user.Employee.Any())
                employee = GetEmployeeForUser(user, actorCompanyId, getHidden);

            return employee;
        }

        public Employee GetEmployeeForUser(User user, int actorCompanyId, bool getHidden = false)
        {
            if (user == null)
                return null;

            if (user.Employee.IsNullOrEmpty())
                return null;

            Employee employee = null;

            //Find Employee for given Company
            foreach (Employee employeeForUser in user.Employee)
            {
                if (employeeForUser.Hidden && !getHidden)
                    continue;

                if (employeeForUser.ActorCompanyId == actorCompanyId)
                {
                    employee = employeeForUser;
                    break;
                }
            }

            return employee;
        }

        public Employee GetProjectEmployeeForUser(int userId, int actorCompanyId, bool getHidden = false, bool loadEmployment = false)
        {
            var employee = GetEmployeeForUser(userId, actorCompanyId, getHidden, loadEmployment);
            if (employee != null)
                SetEmployeeProjectDefaultTimeCodeId(employee);

            return employee;
        }

        public void SetEmployeeProjectDefaultTimeCodeId(Employee employee, List<EmployeeGroup> employeeGroups = null)
        {
            if (employee == null)
                return;

            if (employeeGroups == null)
                employeeGroups = GetEmployeeGroupsFromCache(employee.ActorCompanyId);

            if (employee.TimeCodeId.HasValue && employee.ProjectDefaultTimeCodeId == 0)
                employee.ProjectDefaultTimeCodeId = employee.TimeCodeId.Value;

            if (employee.ProjectDefaultTimeCodeId == 0)
            {
                var employeeGroup = employee.GetEmployeeGroup(DateTime.Now, employeeGroups);
                if (employeeGroup != null && employeeGroup.TimeCodeId.HasValue)
                    employee.ProjectDefaultTimeCodeId = employeeGroup.TimeCodeId.Value;

                if (employee.ProjectDefaultTimeCodeId == 0)
                    employee.ProjectDefaultTimeCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, base.ActorCompanyId, 0);
            }
        }

        public EmployeeDTO GetEmployeeForPayrollCalculation(int actorCompanyId, int employeeId, int timePeriodId, DateTime? dateFrom, DateTime? dateTo, int? taxYear)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeeForPayrollCalculation(entities, actorCompanyId, employeeId, timePeriodId, dateFrom, dateTo, taxYear);
        }

        public EmployeeDTO GetEmployeeForPayrollCalculation(CompEntities entities, int actorCompanyId, int employeeId, int timePeriodId, DateTime? dateFrom, DateTime? dateTo, int? taxYear)
        {
            List<EmploymentTypeDTO> employmentTypeTerms = GetEmploymentTypes(base.ActorCompanyId);
            List<GenericType> disbursementMethodTerms = base.GetTermGroupContent(TermGroup.EmployeeDisbursementMethod);
            List<GenericType> employeeTaxTypeTerms = base.GetTermGroupContent(TermGroup.EmployeeTaxType);
            EmployeeDTO employee = GetEmployee(entities, employeeId, actorCompanyId, loadEmployment: true, loadVacationGroup: true, loadContactPerson: true, loadEmployeeTax: true, loadEmployeeVacation: true).ToDTO(includeEmployeeGroup: true, includePayrollGroup: true, includeVacationGroup: true, includeEmployeeTax: true, isPayrollCalculation: true, employmentTypes: employmentTypeTerms, disbursementMethodTerms: disbursementMethodTerms, employeeTaxTypes: employeeTaxTypeTerms, taxYear: taxYear, dateFrom: dateFrom, dateTo: dateTo);
            if (employee != null && employee.FinalSalaryEndDateApplied.HasValue)
                employee.FinalSalaryAppliedTimePeriodId = PayrollManager.GetVacationYearEndRow(entities, employeeId, timePeriodId, SoeVacationYearEndType.FinalSalary)?.TimePeriodId;

            return employee;
        }

        public int GetEmployeeIdForUser(int userId, int actorCompanyId, bool getHidden = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeIdForUser(entities, userId, actorCompanyId, getHidden);
        }

        public int GetEmployeeIdForUser(CompEntities entities, int userId, int actorCompanyId, bool getHidden = false)
        {
            return GetEmployeeForUser(entities, userId, actorCompanyId, getHidden)?.EmployeeId ?? 0;
        }

        public decimal? GetPrelPayedDaysYear1(int employeeId, int actorCompanyId)
        {
            decimal? prelPayedDaysYear1 = null;

            Employee employee = GetEmployee(employeeId, actorCompanyId, loadEmployeeVacation: true);
            if (employee != null)
            {
                if (!employee.EmployeeVacationSE.IsLoaded)
                    employee.EmployeeVacationSE.Load();

                EmployeeVacationSE employeeVacationSE = employee.EmployeeVacationSE.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);
                if (employeeVacationSE != null && employeeVacationSE.PrevEmployeeVacationSEId.HasValue)
                {
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    var timePayrollTransactionsVacationYearEnd = (from tpt in entitiesReadOnly.TimePayrollTransaction
                                                                    .Include("TimeBlockDate")
                                                                  where tpt.EmployeeId == employeeId &&
                                                                  tpt.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear1 &&
                                                                  tpt.State == (int)SoeEntityState.Active &&
                                                                  (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                                                                  select tpt).ToList();

                    if (timePayrollTransactionsVacationYearEnd.Count > 0)
                    {
                        List<int> lockedAttestStateIds = new List<int>()
                        {
                            SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentLockedAttestStateId, 0, actorCompanyId, 0),
                            SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentApproved1AttestStateId, 0, actorCompanyId, 0),
                            SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentApproved2AttestStateId, 0, actorCompanyId, 0),
                            SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, 0, actorCompanyId, 0),
                        };

                        List<TimePayrollTransaction> timePayrollTransactionsVacationYearEndPrel = timePayrollTransactionsVacationYearEnd.Where(i => !lockedAttestStateIds.Contains(i.AttestStateId) && i.VacationYearEndRowId.HasValue).ToList();
                        foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactionsVacationYearEndPrel)
                        {
                            if (!timePayrollTransaction.VacationYearEndRowReference.IsLoaded)
                                timePayrollTransaction.VacationYearEndRowReference.Load();
                            if (timePayrollTransaction.VacationYearEndRow != null)
                            {
                                if (!timePayrollTransaction.VacationYearEndRow.VacationYearEndHeadReference.IsLoaded)
                                    timePayrollTransaction.VacationYearEndRow.VacationYearEndHeadReference.Load();

                                if (timePayrollTransaction.VacationYearEndRow.VacationYearEndHead.Date <= timePayrollTransaction.TimeBlockDate.Date)
                                {
                                    if (!prelPayedDaysYear1.HasValue)
                                        prelPayedDaysYear1 = 0;
                                    prelPayedDaysYear1 += timePayrollTransaction.Quantity;
                                }
                            }
                        }
                    }
                }
            }

            return prelPayedDaysYear1;
        }

        public string GetEmployeeNr(CompEntities entities, int? employeeId, int hiddenEmployeeId, int actorCompanyId, ref List<Employee> employees)
        {
            if (!employeeId.HasValue)
                return String.Empty;
            if (hiddenEmployeeId > 0 && hiddenEmployeeId == employeeId.Value)
                return Constants.HIDDENEMPLOYEENR;
            if (employeeId.Value == Constants.NO_REPLACEMENT_EMPLOYEEID)
                return Constants.NO_REPLACEMENT_EMPLOYEENR;

            Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId.Value);
            if (employee == null)
            {
                employee = GetEmployee(entities, employeeId.Value, actorCompanyId);
                if (employee != null)
                    employees.Add(employee);
            }

            return employee != null ? employee.EmployeeNr : String.Empty;
        }

        public string GetNextEmployeeNr(int actorCompanyId, bool findFirstGap = true)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Employee.NoTracking();
            var query = (from e in entitiesReadOnly.Employee
                         where e.ActorCompanyId == actorCompanyId
                         select e).ToList();

            IOrderedEnumerable<Employee> employeeNrs;
            if (findFirstGap)
                employeeNrs = query.OrderBy(e => e.EmployeeNrSort);
            else
                employeeNrs = query.OrderByDescending(e => e.EmployeeNrSort);

            int employeeNrNext = 0;
            int employeeNrPrev = 0;

            foreach (var item in employeeNrs)
            {
                if (int.TryParse(item.EmployeeNr, out employeeNrNext))
                {
                    if (!findFirstGap)
                        return (employeeNrNext + 1).ToString();
                    else if (employeeNrPrev != 0 && (employeeNrPrev + 1) != employeeNrNext && employeeNrPrev != employeeNrNext)
                        return (employeeNrPrev + 1).ToString();
                    else
                        employeeNrPrev = employeeNrNext;
                }
            }

            return string.Empty;
        }

        public string GetEmployeeName(CompEntities entities, int employeeId)
        {
            return (from e in entities.Employee
                    where e.EmployeeId == employeeId
                    select e.ContactPerson.FirstName + " " + e.ContactPerson.LastName).FirstOrDefault() ??
                   string.Empty;
        }

        public string GetDefaultEmployeeAccountDimName()
        {
            return AccountManager.GetDefaultEmployeeAccountDim(base.ActorCompanyId)?.Name?.ToLower() ?? string.Empty;
        }

        public void SetEmployeeHibernatingText(Employee employee, DateTime startDate, DateTime stopDate)
        {
            List<DateRangeDTO> hibernatingPeriods = employee.GetHibernatingPeriods(startDate, stopDate);
            if (hibernatingPeriods.IsNullOrEmpty())
                return;

            employee.HibernatingText = $"{GetText(91978, "Ordinarie anställning vilande")} {hibernatingPeriods.GetIntervals()}";
        }

        public int GetNrOfEmployeesByLicense(CompEntities entities, int licenseId, bool? active = true)
        {
            var employees = (from e in entities.Employee
                             where e.Company.LicenseId == licenseId &&
                             !e.Hidden &&
                             !e.Vacant &&
                             !e.Company.Demo &&
                             e.State != (int)SoeEntityState.Deleted
                             select e).ToList();

            if (active == true)
                employees = employees.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                employees = employees.Where(i => i.State == (int)SoeEntityState.Inactive).ToList();

            return employees.Count;
        }

        public int GetEmployeeAge(CompEntities entities, int employeeId, int actorCompanyId, DateTime? stopDate = null)
        {
            // Get birth default
            DateTime? birthDate = GetEmployeeBirthDate(entities, employeeId, actorCompanyId);
            if (!birthDate.HasValue)
                return 0;

            // Calculate age
            return CalendarUtility.GetYearsBetweenDates(birthDate.ToValueOrToday(), stopDate.ToValueOrToday());
        }

        public int GetEmployeeAge(Employee employee, DateTime? stopDate = null)
        {
            if (employee == null)
                return 0;

            // Get birth default
            DateTime? birthDate = GetEmployeeBirthDate(employee);
            if (!birthDate.HasValue)
                return 0;

            // Calculate age
            return CalendarUtility.GetYearsBetweenDates(birthDate.ToValueOrToday(), stopDate.ToValueOrToday());
        }

        public int GetLastUsedEmployeeSequenceNumber(int actorCompanyId)
        {
            return SequenceNumberManager.GetLastUsedSequenceNumber(actorCompanyId, "Employee");
        }

        public bool IsEmployeeCurrentUser(int employeeId, int userId, bool getHidden = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.User.NoTracking();
            return IsEmployeeCurrentUser(entities, employeeId, userId, getHidden);
        }

        public bool IsEmployeeCurrentUser(CompEntities entities, int employeeId, int userId, bool getHidden = false)
        {
            User user = (from e in entities.Employee
                         where e.EmployeeId == employeeId &&
                         (!e.Hidden || getHidden) &&
                         e.State == (int)SoeEntityState.Active &&
                         e.User != null
                         select e.User).FirstOrDefault();

            return user != null && user.UserId == userId;
        }

        public bool EmployeeExists(string employeeNr, int actorCompanyId, bool getHidden = false, int? excludeEmployeeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return EmployeeExists(entities, employeeNr, actorCompanyId, getHidden, excludeEmployeeId);
        }

        public bool EmployeeExists(CompEntities entities, string employeeNr, int actorCompanyId, bool getHidden = false, int? excludeEmployeeId = null)
        {
            if (String.IsNullOrEmpty(employeeNr))
                return false;

            // Deleted or temporary validEmployees is not checked
            return (from e in entities.Employee
                    where e.ActorCompanyId == actorCompanyId &&
                    e.EmployeeNr == employeeNr &&
                    (!e.Hidden || getHidden) &&
                    (!excludeEmployeeId.HasValue || e.EmployeeId != excludeEmployeeId.Value) &&
                    (e.State == (int)SoeEntityState.Active || e.State == (int)SoeEntityState.Inactive)
                    select e).Any();
        }

        public ActionResult ValidateEmployeeSocialSecNumberNotExists(string socialSecNr, int actorCompanyId, int? excludeEmployeeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return ValidateEmployeeSocialSecNumberNotExists(entities, socialSecNr, actorCompanyId, excludeEmployeeId);
        }

        public ActionResult ValidateEmployeeSocialSecNumberNotExists(CompEntities entities, string socialSecNr, int actorCompanyId, int? excludeEmployeeId = null)
        {
            ActionResult result = new ActionResult();

            if (!String.IsNullOrEmpty(socialSecNr))
            {
                socialSecNr = StringUtility.SocialSecYYYYMMDDXXXX(socialSecNr);


                var employees = (from e in entities.Employee.Include("ContactPerson")
                                 where e.ActorCompanyId == actorCompanyId &&
                                 e.ContactPerson.SocialSec != null && e.ContactPerson.SocialSec.Length > 9 &&
                                 (!excludeEmployeeId.HasValue || e.EmployeeId != excludeEmployeeId.Value) &&
                                 (e.State == (int)SoeEntityState.Active || e.State == (int)SoeEntityState.Inactive)
                                 select new { e.EmployeeId, e.ContactPerson.SocialSec, e.EmployeeNr, e.ContactPerson.FirstName, e.ContactPerson.LastName, e.State }).ToList();

                var employeesForSocialSec = employees.Where(e => StringUtility.SocialSecYYYYMMDDXXXX(e.SocialSec) == socialSecNr).ToList();
                if (employeesForSocialSec.Any())
                {
                    string inactive = $" ({GetText(156, 1002, "Inaktiv")})";

                    StringBuilder message = new StringBuilder();
                    foreach (var employee in employeesForSocialSec.OrderBy(e => e.EmployeeNr))
                    {
                        message.Append($"({employee.EmployeeNr}) {employee.FirstName} {employee.LastName} {(employee.State == (int)SoeEntityState.Inactive ? inactive : string.Empty)}\n");
                    }

                    result.Success = false;
                    result.ErrorMessage = message.ToString();
                }
            }

            return result;
        }

        public DateTime GetEmployeeStopDateBasedOnIncludeNotStarted(bool includeNotStarted)
        {
            return includeNotStarted ? DateTime.MaxValue : DateTime.Today;
        }

        public DateTime GetEmployeeStartDateBasedOnIncludeEnded(bool includeEnded, DateTime? date = null, bool discardLimitSettingForEnded = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeStartDateByEmployeeeEndedSetting(entities, includeEnded, date, discardLimitSettingForEnded);
        }

        public DateTime GetEmployeeStartDateByEmployeeeEndedSetting(CompEntities entities, bool includeEnded, DateTime? date, bool discardLimitSettingForEnded = false)
        {
            date = date ?? DateTime.Today;

            if (!includeEnded)
                return date.Value;
            if (discardLimitSettingForEnded)
                return DateTime.MinValue;

            int nbrOfMonthsAfterEnded = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.EmployeeIncludeNbrOfMonthsAfterEnded, 0, base.ActorCompanyId, 0);
            if (nbrOfMonthsAfterEnded <= 0)
                nbrOfMonthsAfterEnded = 3;
            return date.Value.AddMonths(-nbrOfMonthsAfterEnded);
        }

        public DateTime? GetEmployeeBirthDate(CompEntities entities, int employeeId, int actorCompanyId)
        {
            Employee employee = GetEmployee(entities, employeeId, actorCompanyId, loadContactPerson: true);
            return GetEmployeeBirthDate(employee);
        }

        public DateTime? GetEmployeeBirthDate(Employee employee)
        {
            DateTime? birthDate = null;
            if (employee != null && !employee.ContactPersonReference.IsLoaded)
                employee.ContactPersonReference.Load();

            if (employee != null && employee.ContactPerson != null && !String.IsNullOrEmpty(employee.ContactPerson.SocialSec))
                birthDate = CalendarUtility.GetBirthDateFromSecurityNumber(employee.ContactPerson.SocialSec);

            return birthDate;
        }

        public Contact GetEmployeeContact(int employeeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetEmployeeContact(entities, employeeId);
        }

        public Contact GetEmployeeContact(CompEntities entities, int employeeId)
        {
            Employee employee = (from e in entities.Employee
                                     .Include("ContactPerson")
                                 where e.EmployeeId == employeeId
                                 select e).FirstOrDefault();

            Contact contact = null;
            if (employee != null && employee.ContactPerson != null)
                contact = ContactManager.GetContactAndEcomFromActor(entities, employee.ContactPerson.ActorContactPersonId);

            return contact;
        }

        public GenericType<int, int> GetNrOfEmployeesAndMaxByLicense(int licenseId, bool onlyActive = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            entities.License.NoTracking();
            return GetNrOfEmployeesAndMaxByLicense(entities, licenseId, onlyActive);
        }

        public GenericType<int, int> GetNrOfEmployeesAndMaxByLicense(CompEntities entities, int licenseId, bool onlyActive = true)
        {
            var nrOfUsers = new GenericType<int, int>();

            var license = LicenseManager.GetLicense(licenseId);
            if (license == null)
                return nrOfUsers;

            var employees = from e in entities.Employee
                            where e.Company.LicenseId == licenseId &&
                            e.Company.State == (int)SoeEntityState.Active &&
                            !e.Company.Demo &&
                            !e.Hidden &&
                            !e.Vacant &&
                            e.State != (int)SoeEntityState.Deleted
                            select e;

            if (onlyActive)
                employees = employees.Where(e => e.State == (int)SoeEntityState.Active);

            nrOfUsers.Field1 = employees.Count();
            nrOfUsers.Field2 = license.MaxNrOfEmployees;

            return nrOfUsers;
        }

        public List<GenericType<int, string>> GetExternalCodes(List<int> employeeIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return (from e in entities.Employee
                    where employeeIds.Contains(e.EmployeeId) &&
                    e.ExternalCode != null
                    select new GenericType<int, string>() { Field1 = e.EmployeeId, Field2 = e.ExternalCode }).ToList();
        }

        public ActionResult SaveEmployeeFromTemplate(SaveEmployeeFromTemplateHeadDTO input, int actorCompanyId)
        {
            if (input == null || input.Rows.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(91926, "Inga fält valda"));
            if (input.Rows.Any(b => b.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentWorkTimeWeek) && input.Rows.Any(b => b.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentPercent))
                return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(91927, "Ej tillåtet att ändra både veckoarbetstid och sysselsättningsgrad samtidigt"));

            ActionResult result = new ActionResult();
            var employeeErrors = string.Empty;

            int companyCountryId = CompanyManager.GetCompanySysCountryId(actorCompanyId);

            EmployeeChangeIODTO employeeChange = ConvertToEmployeeChangeDTO(input, (TermGroup_Country)companyCountryId);

            if (!input.EmployeeId.HasValue)
            {
                employeeChange.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO()
                {
                    EmployeeChangeType = EmployeeChangeType.EmployeeTemplateId,
                    Value = input.EmployeeTemplateId.ToString()
                });
            }

            if (!input.EmployeeId.HasValue && input.Rows.Any(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmployeeNr))
            {
                string employeeNumber = input.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmployeeNr)?.Value;
                if (!string.IsNullOrEmpty(employeeNumber))
                {
                    var emp = GetEmployeeByNr(employeeNumber, base.ActorCompanyId);
                    if (emp != null)
                    {
                        employeeErrors += GetText(12159, "Anställningsnummer finns redan " + employeeNumber.ToString()) + Environment.NewLine;
                        result.ErrorMessage += employeeErrors;
                        result.Success = false;
                        return result;
                    }
                }
            }

            if (employeeChange.HasRows())
            {
                bool dontValidateSocialSecNbr = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeDontValidateSocialSecNbr, 0, actorCompanyId, 0);
                if (dontValidateSocialSecNbr && employeeChange.EmployeeChangeRowIOs.Any(a => a.EmployeeChangeType == EmployeeChangeType.SocialSec))
                    employeeChange.EmployeeChangeRowIOs.Where(a => a.EmployeeChangeType == EmployeeChangeType.SocialSec && string.IsNullOrEmpty(a.OptionalExternalCode)).ToList().ForEach(f => f.OptionalExternalCode = "force");

                List<GenericType> terms = GetTermGroupContent(TermGroup.ApiEmployee);
                employeeChange.EmployeeChangeRowIOs.ForEach(f => f.Validate(terms, true, (TermGroup_Country)companyCountryId));

                foreach (var row in employeeChange.EmployeeChangeRowIOs.Where(w => !w.IsValid && w.IsInvalidToSaveFromEmployeeTemplateWithErrors()))
                {
                    if (row.EmployeeChangeType == EmployeeChangeType.Email)
                        employeeErrors += GetText(11672, "Angiven epost är inte korrekt" + " " + row.Value) + Environment.NewLine;
                    else if (!dontValidateSocialSecNbr && (row.EmployeeChangeType == EmployeeChangeType.SocialSec))
                        employeeErrors += String.Format(GetText(11723, "Felaktigt personnummer ({0}) på den anställde."), StringUtility.NullToEmpty(row.Value)) + Environment.NewLine;
                    else if ((row.EmployeeChangeType == EmployeeChangeType.DisbursementAccountNr))
                        employeeErrors += GetText(9972, "Utbetalningskontot har ett felaktigt format." + " " + row.Value) + Environment.NewLine;
                }
            }

            if (!string.IsNullOrEmpty(employeeErrors))
            {
                result.ErrorMessage += employeeErrors;
                result.Success = false;
            }

            if (!result.Success)
                return result;

            EmployeeChangeResult employeeChangeResult = ApiManager.ImportEmployeeChangesFromEmployeeTemplate(employeeChange, out result);

            // Check for errors in the batch
            EmployeeUserImportBatch batch = result.Value as EmployeeUserImportBatch;
            EmployeeUserImport import = batch.Imports.FirstOrDefault();
            if (import != null && !string.IsNullOrEmpty(import.Result.ErrorMessage))
            {
                result.Success = false;
                result.ErrorMessage = import.Result.ErrorMessage;
            }

            if (result.Success)
            {
                // Check for validation errors
                List<string> validationErrors = new List<string>();
                EmployeeChangeIODTO empChange = employeeChangeResult.Employees.FirstOrDefault();
                if (empChange != null && !empChange.ValidationErrors.IsNullOrEmpty())
                {
                    foreach (EmployeeChangeRowValidation err in empChange.ValidationErrors)
                    {
                        validationErrors.Add(err.Message);
                    }
                    result.ErrorMessage = validationErrors.JoinToString("\r\n");
                    result.Success = false;
                }
            }

            if (result.Success && employeeChangeResult.NrOfReceivedEmployees > 0 && result.Value is EmployeeUserImportBatch && batch.IsAlreadyUpdated)
                result.InfoMessage += $"{GetText(12054, "Inga förändringar. Alla uppgifter är redan uppdaterade")}";

            Employee employee = input.EmployeeId.HasValue ? GetEmployee(input.EmployeeId.Value, base.ActorCompanyId) : null;
            string employeeNr = employee?.EmployeeNr ?? input.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmployeeNr)?.Value;

            if (employee == null && !string.IsNullOrEmpty(employeeNr))
                employee = GetEmployeeByNr(employeeNr, base.ActorCompanyId, loadContactPerson: true, loadEmployment: true);

            if (employee != null)
            {
                result.IntegerValue = employee.EmployeeId;
                result.IntegerValue2 = employee.UserId ?? 0;
            }

            if (result.Success && input.PrintEmploymentContract && employee != null)
            {
                ActionResult printResult = PrintEmploymentContractFromTemplate(employee, input.EmployeeTemplateId, new List<DateTime>() { input.Date }, false, actorCompanyId);
                if (printResult.Success)
                {
                    result.DecimalValue = printResult.DecimalValue;
                }
                else if (!string.IsNullOrEmpty(printResult.ErrorMessage))
                {
                    result.ErrorMessage = printResult.ErrorMessage;
                    result.ErrorNumber = (int)ActionResultSave.PrintEmploymentContractFromTemplateFailed;
                }
            }

            return result;
        }

        private EmployeeChangeIODTO ConvertToEmployeeChangeDTO(SaveEmployeeFromTemplateHeadDTO input, TermGroup_Country companyCountry)
        {
            EmployeeChangeIODTO employeeChange = new EmployeeChangeIODTO();

            if (input?.Rows != null)
            {
                var template = GetEmployeeTemplate(base.ActorCompanyId, input.EmployeeTemplateId);
                if (template == null)
                    return null;

                var agreement = GetEmployeeCollectiveAgreement(base.ActorCompanyId, template.EmployeeCollectiveAgreementId);
                if (agreement == null)
                    return null;

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var employeeGroup = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId)).FirstOrDefault(f => f.EmployeeGroupId == agreement.EmployeeGroupId);
                var payrollGroup = agreement.PayrollGroupId.HasValue ? GetPayrollGroupsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId)).FirstOrDefault(f => f.PayrollGroupId == agreement.PayrollGroupId) : null;
                var vacationGroup = agreement.VacationGroupId.HasValue ? GetVacationGroupsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId)).FirstOrDefault(f => f.VacationGroupId == agreement.VacationGroupId) : null;
                var annualLeaveGroup = agreement.AnnualLeaveGroupId.HasValue ? GetAnnualLeaveGroupsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId)).FirstOrDefault(f => f.AnnualLeaveGroupId == agreement.AnnualLeaveGroupId) : null;

                Employee employee = input.EmployeeId.HasValue ? GetEmployee(input.EmployeeId.Value, base.ActorCompanyId) : null;
                string employeeNr = employee?.EmployeeNr ?? input.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmployeeNr)?.Value;

                employeeChange = new EmployeeChangeIODTO
                {
                    EmployeeNr = employeeNr,
                    EmployeeChangeRowIOs = new List<EmployeeChangeRowIODTO>(),
                };

                if (employeeGroup != null)
                {
                    employeeChange.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO()
                    {
                        EmployeeChangeType = EmployeeChangeType.EmployeeGroup,
                        Value = employeeGroup.Name,
                        FromDate = input.Date,
                    });
                }

                if (payrollGroup != null)
                {
                    employeeChange.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO()
                    {
                        EmployeeChangeType = EmployeeChangeType.PayrollGroup,
                        Value = payrollGroup.Name,
                        FromDate = input.Date,
                    });
                }

                if (vacationGroup != null)
                {
                    employeeChange.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO()
                    {
                        EmployeeChangeType = EmployeeChangeType.VacationGroup,
                        Value = vacationGroup.Name,
                        FromDate = input.Date,
                    });
                }

                if (annualLeaveGroup != null)
                {
                    employeeChange.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO()
                    {
                        EmployeeChangeType = EmployeeChangeType.AnnualLeaveGroup,
                        Value = annualLeaveGroup.Name,
                        FromDate = input.Date,
                    });
                }

                employeeChange = input.ToEmployeeChangeIODTO(employeeChange, GetTermGroupContent(TermGroup.ApiEmployee), companyCountry);
            }

            return employeeChange;
        }

        public ActionResult PrintEmploymentContractFromTemplate(int employeeId, int employeeTemplateId, List<DateTime> substituteDates, bool isPrintedFromSchedulePlanning, int actorCompanyId)
        {
            Employee employee = GetEmployee(employeeId, actorCompanyId);
            if (employee != null)
                return PrintEmploymentContractFromTemplate(employee, employeeTemplateId, substituteDates, isPrintedFromSchedulePlanning, actorCompanyId);

            return new ActionResult(false);
        }

        private ActionResult PrintEmploymentContractFromTemplate(Employee employee, int employeeTemplateId, List<DateTime> substituteDates, bool isPrintedFromSchedulePlanning, int actorCompanyId)
        {
            ActionResult result = new ActionResult(false);

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int? reportId = PayrollManager.GetReportForEmployeeTemplateId(entitiesReadOnly, actorCompanyId, employeeTemplateId);
            if (reportId.HasValue)
            {
                ReportPrintoutDTO reportPrintout = ReportDataManager.PrintTimeEmploymentDynamicContract(reportId.Value, employee.EmployeeId, employee.GetEmploymentId(substituteDates.First()) ?? 0, employeeTemplateId, substituteDates, isPrintedFromSchedulePlanning);
                if (reportPrintout?.Data != null)
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        DataStorage storage = SaveEmploymentContractOnPrint(entities, reportPrintout.ReportId ?? 0, employee.EmployeeId, reportPrintout.Data, reportPrintout.XML, reportPrintout.ReportName);
                        if (storage != null && storage.DataStorageRecord != null && storage.DataStorageRecord.Any())
                        {
                            result.Success = true;
                            result.IntegerValue = employee.EmployeeId;
                            result.IntegerValue2 = employee.UserId ?? 0;
                            result.DecimalValue = storage.DataStorageRecord.First().DataStorageRecordId;
                        }
                    }
                }
            }
            else
            {
                result.ErrorMessage = GetText(12162, 1, "Rapport saknas!\nSe till att rapport 'Anställningsavtalsmall' är upplagd och kopplad mot den anställdes löneavtal.");
            }

            return result;
        }

        public ActionResult UpdateEmployee(Employee employee, int actorCompanyId)
        {
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Employee");

            using (CompEntities entities = new CompEntities())
            {
                Employee originalEmployee = GetEmployee(entities, employee.EmployeeId, actorCompanyId);
                if (originalEmployee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Employee");

                return UpdateEntityItem(entities, originalEmployee, employee, "Employee");
            }
        }

        public ActionResult UpdateWorkPercentageOnEmployments(EmployeeGroup employeeGroup)
        {
            CompEntities entities = new CompEntities();
            return UpdateWorkPercentageOnEmployments(entities, employeeGroup);
        }

        public ActionResult UpdateWorkPercentageOnEmployments(CompEntities entities, EmployeeGroup employeeGroup)
        {
            if (employeeGroup == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

            //using (CompEntities entities = new CompEntities())
            //using (CompEntities entities)
            //{
            DateTime batchCreated = DateTime.Now;
            string batchComment = GetText(5987, "Förändring av Arbetstid (timmar per vecka) på tidavtal");

            //All Employments on Company
            List<Employment> allEmployments = GetEmployments(entities, employeeGroup.ActorCompanyId, onlyActiveEmployments: false, onlyActiveEmployees: false);

            //Affected Employments
            List<Employment> employments = new List<Employment>();
            employments.AddRange(allEmployments.Where(e => e.OriginalEmployeeGroupId == employeeGroup.EmployeeGroupId));
            employments.AddRange(allEmployments.Where(e => e.EmploymentChange.Any(ec => ec.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek)));

            List<int> handledEmploymentIds = new List<int>();

            //Update Employments with given EmployeeGroup from start
            foreach (Employment employment in employments)
            {
                if (handledEmploymentIds.Contains(employment.EmploymentId))
                    continue;

                if (employment.OriginalEmployeeGroupId == employeeGroup.EmployeeGroupId)
                {
                    //Add change from start of Employment
                    decimal from = employment.GetPercent(employment.DateFrom);
                    decimal to = employeeGroup.RuleWorkTimeWeek > 0 ? Decimal.Divide(employment.GetWorkTimeWeek(employment.DateFrom), employeeGroup.RuleWorkTimeWeek) * 100 : 0;
                    AddEmploymentChangeAndBatch(TermGroup_EmploymentChangeType.DataChange, TermGroup_EmploymentChangeFieldType.Percent, employment, employment.DateFrom, employment.DateTo, batchComment, batchCreated, from.ToString(), to.ToString());
                }

                List<EmploymentChange> changes = employment.EmploymentChange.Where(i => i.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek).ToList();
                foreach (EmploymentChange change in changes)
                {
                    if (change.EmploymentChangeId == 0)
                        continue;

                    //Check that change is for WorkTimeWeek and correct EmployeeGroup
                    if (employment.GetEmployeeGroupId(change.EmploymentChangeBatch.FromDate) != employeeGroup.EmployeeGroupId)
                        continue;

                    //Add change from current change default
                    var from = employment.GetPercent(change.EmploymentChangeBatch.FromDate);
                    var to = employeeGroup.RuleWorkTimeWeek > 0 ? Decimal.Divide(employment.GetWorkTimeWeek(change.EmploymentChangeBatch.FromDate), employeeGroup.RuleWorkTimeWeek) * 100 : 0;
                    AddEmploymentChangeAndBatch(TermGroup_EmploymentChangeType.DataChange, TermGroup_EmploymentChangeFieldType.Percent, employment, change.EmploymentChangeBatch.FromDate, change.EmploymentChangeBatch.ToDate, batchComment, batchCreated, from.ToString(), to.ToString());
                }

                handledEmploymentIds.Add(employment.EmploymentId);
            }

            return SaveChanges(entities);
            //}
        }

        public ActionResult SaveEmployeeNote(string note, int employeeId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                Employee employee = GetEmployee(entities, employeeId, actorCompanyId);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Employee");

                if (employee.Note != note)
                {
                    employee.Note = note;
                    SetModifiedProperties(employee);
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult SaveEmployeeUserSettingsFromMobile(int employeeId, int userId, int actorCompanyId, bool wantExtraShifts, bool modifyWantExtraShifts)
        {
            using (CompEntities entities = new CompEntities())
            {
                Employee employee = GetEmployee(entities, employeeId, actorCompanyId);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Employee");

                if (modifyWantExtraShifts && (employee.WantsExtraShifts != wantExtraShifts))
                {
                    employee.WantsExtraShifts = wantExtraShifts;
                    SetModifiedProperties(employee);
                    return SaveChanges(entities);
                }
            }

            return new ActionResult();
        }

        public ActionResult MarkEmployeesAsVacant(List<int> employeeIds, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<Employee> employees = (from e in entities.Employee
                                            where e.ActorCompanyId == actorCompanyId &&
                                            employeeIds.Contains(e.EmployeeId)
                                            select e).ToList();

                foreach (Employee employee in employees)
                {
                    employee.Vacant = true;
                    SetModifiedProperties(employee);
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult DeleteEmployee(DeleteEmployeeDTO input)
        {
            ActionResult result = new ActionResult();

            #region Validation

            if (input.Action == DeleteEmployeeAction.Inactivate || input.Action == DeleteEmployeeAction.RemoveInfo)
            {
                result = ActorManager.ValidateInactivateEmployee(input.EmployeeId);
                if (!result.Success)
                {
                    result.ErrorMessage = result.Strings.JoinToString("\n");
                    return result;
                }
            }
            else if (input.Action == DeleteEmployeeAction.Unidentify)
            {
                result = ActorManager.ValidateDeleteEmployee(input.EmployeeId);
                if (!result.Success)
                {
                    result.ErrorMessage = result.Strings.JoinToString("\n");
                    return result;
                }
            }
            else if (input.Action == DeleteEmployeeAction.Delete)
            {
                result = ActorManager.ValidateImmediateDeleteEmployee(input.EmployeeId);
                if (!result.Success)
                {
                    result.ErrorMessage = result.Strings.JoinToString("\n");
                    return result;
                }
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Prereq

                    Employee employee = GetEmployee(entities, input.EmployeeId, ActorCompanyId, onlyActive: false, loadUser: true);
                    if (employee == null)
                        return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(10083, "Anställd hittades inte"));

                    if (input.Action == DeleteEmployeeAction.Cancel)
                        return new ActionResult((int)ActionResultDelete.InsufficientInput, GetText(2099, "Felaktig indata"));

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Perform

                        List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();

                        switch (input.Action)
                        {
                            case DeleteEmployeeAction.Inactivate:
                                DeleteEmployeeInactivate(employee);
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, ActorCompanyId, TermGroup_TrackChangesActionMethod.DeleteEmployee_Inactivate, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.Employee, employee.EmployeeId));
                                break;
                            case DeleteEmployeeAction.RemoveInfo:
                                DeleteEmployeeRemoveInfo(entities, employee, input, false);
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, ActorCompanyId, TermGroup_TrackChangesActionMethod.DeleteEmployee_RemoveInfo, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.Employee, employee.EmployeeId));
                                break;
                            case DeleteEmployeeAction.Unidentify:
                                DeleteEmployeeUnidentify(entities, employee, input);
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, ActorCompanyId, TermGroup_TrackChangesActionMethod.DeleteEmployee_Unidentify, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.Employee, employee.EmployeeId));
                                break;
                            case DeleteEmployeeAction.Delete:
                                DeleteEmployeeDelete(entities, employee, input);
                                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, ActorCompanyId, TermGroup_TrackChangesActionMethod.DeleteEmployee_Delete, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.Employee, employee.EmployeeId));
                                break;
                        }

                        // Common for all actions
                        employee.CardNumber = null;
                        SetModifiedProperties(employee);

                        if (trackChangesItems.Any())
                            TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        private void DeleteEmployeeInactivate(Employee employee)
        {
            // Inactivate emp
            employee.State = (int)SoeEntityState.Inactive;

            // Inactivate user
            if (employee.UserId.HasValue)
            {
                if (employee.User == null && !employee.UserReference.IsLoaded)
                    employee.UserReference.Load();

                if (employee.User != null)
                    employee.User.State = (int)SoeEntityState.Inactive;
            }
        }

        private void DeleteEmployeeRemoveInfo(CompEntities entities, Employee employee, DeleteEmployeeDTO input, bool forceAll)
        {
            // First inactivate emp
            DeleteEmployeeInactivate(employee);

            // Remove personal information based on user selections

            if (input.RemoveInfoSalaryDistress || forceAll)
            {
                #region Löneutmätning

                // Get all tax records and clear salary distress data

                if (!employee.EmployeeTaxSE.IsLoaded)
                    employee.EmployeeTaxSE.Load();

                foreach (EmployeeTaxSE tax in employee.EmployeeTaxSE)
                {
                    tax.SalaryDistressAmount = 0;
                    tax.SalaryDistressAmountType = (int)TermGroup_EmployeeTaxSalaryDistressAmountType.NotSelected;
                    tax.SalaryDistressReservedAmount = 0;
                    tax.SalaryDistressCase = "";
                    SetModifiedProperties(tax);
                }

                #endregion
            }
            if (input.RemoveInfoUnionFee || forceAll)
            {
                #region Fackavgift

                // Delete all union fee records

                if (!employee.EmployeeUnionFee.IsLoaded)
                    employee.EmployeeUnionFee.Load();

                foreach (EmployeeUnionFee unionFee in employee.EmployeeUnionFee.ToList())
                {
                    entities.DeleteObject(unionFee);
                }

                #endregion
            }
            if (input.RemoveInfoAbsenceSick || forceAll)
            {
                #region Sjuk

                // Clear absence sick fields

                employee.HighRiskProtection = false;
                employee.HighRiskProtectionTo = null;
                employee.MedicalCertificateReminder = false;
                employee.MedicalCertificateDays = null;
                employee.Absence105DaysExcluded = false;
                employee.Absence105DaysExcludedDays = null;

                #endregion
            }
            if (input.RemoveInfoAbsenceParentalLeave || forceAll)
            {
                #region Föräldraledighet och barn

                // Get all children
                // Can not delete them, clear fields

                if (!employee.EmployeeChild.IsLoaded)
                    employee.EmployeeChild.Load();

                foreach (EmployeeChild child in employee.EmployeeChild)
                {
                    child.FirstName = GetText(2244, "Borttagen");
                    child.LastName = GetText(2244, "Borttagen");
                    child.BirthDate = null;
                    child.SingelCustody = false;
                    child.OpeningBalanceUsedDays = 0;
                    child.State = (int)SoeEntityState.Deleted;
                    SetModifiedProperties(child);
                }

                #endregion
            }
            if (input.RemoveInfoMeeting || forceAll)
            {
                #region Samtal

                // Load all emp meetings and delete them

                if (!employee.EmployeeMeeting.IsLoaded)
                    employee.EmployeeMeeting.Load();
                foreach (EmployeeMeeting meeting in employee.EmployeeMeeting)
                {
                    if (!meeting.Participant.IsLoaded)
                        meeting.Participant.Load();
                    if (!meeting.AttestRole.IsLoaded)
                        meeting.AttestRole.Load();
                }

                foreach (EmployeeMeeting meeting in employee.EmployeeMeeting.ToList())
                {
                    meeting.Participant.Clear();
                    meeting.AttestRole.Clear();
                    entities.DeleteObject(meeting);
                }

                #endregion
            }
            if (input.RemoveInfoNote || forceAll)
            {
                #region Noteringar

                // Clear note

                employee.Note = null;

                #endregion
            }
            if (input.RemoveInfoAddress || forceAll)
            {
                #region Adresser

                if (!employee.ContactPersonReference.IsLoaded)
                    employee.ContactPersonReference.Load();

                Contact contact = entities.Contact.Include("ContactAddress.ContactAddressRow").FirstOrDefault(c => c.Actor.ActorId == employee.ContactPerson.ActorContactPersonId);
                if (contact != null)
                {
                    foreach (ContactAddress address in contact.ContactAddress.ToList())
                    {
                        foreach (ContactAddressRow row in address.ContactAddressRow.ToList())
                        {
                            entities.DeleteObject(row);
                        }
                        entities.DeleteObject(address);
                    }
                }

                #endregion
            }
            if (input.RemoveInfoPhone || input.RemoveInfoEmail || input.RemoveInfoClosestRelative || input.RemoveInfoOtherContactInfo || forceAll)
            {
                #region Telefonnummer, E-postadresser, Närmast anhöriga, Övrig kontaktinformation

                if (!employee.ContactPersonReference.IsLoaded)
                    employee.ContactPersonReference.Load();

                List<int> ecomTypes = new List<int>();
                if (input.RemoveInfoPhone || forceAll)
                {
                    ecomTypes.Add((int)TermGroup_SysContactEComType.PhoneHome);
                    ecomTypes.Add((int)TermGroup_SysContactEComType.PhoneJob);
                    ecomTypes.Add((int)TermGroup_SysContactEComType.PhoneMobile);
                    ecomTypes.Add((int)TermGroup_SysContactEComType.Fax);
                }
                if (input.RemoveInfoEmail || forceAll)
                {
                    ecomTypes.Add((int)TermGroup_SysContactEComType.Email);
                    ecomTypes.Add((int)TermGroup_SysContactEComType.CompanyAdminEmail);
                }
                if (input.RemoveInfoClosestRelative || forceAll)
                {
                    ecomTypes.Add((int)TermGroup_SysContactEComType.ClosestRelative);
                }
                if (input.RemoveInfoOtherContactInfo || forceAll)
                {
                    ecomTypes.Add((int)TermGroup_SysContactEComType.Web);
                    ecomTypes.Add((int)TermGroup_SysContactEComType.Coordinates);
                    ecomTypes.Add((int)TermGroup_SysContactEComType.IndividualTaxNumber);
                    ecomTypes.Add((int)TermGroup_SysContactEComType.GlnNumber);
                }

                Contact contact = entities.Contact.Include("ContactECom").FirstOrDefault(c => c.Actor.ActorId == employee.ContactPerson.ActorContactPersonId);
                if (contact != null)
                {
                    foreach (ContactECom ecom in contact.ContactECom.Where(c => ecomTypes.Contains(c.SysContactEComTypeId)).ToList())
                    {
                        entities.DeleteObject(ecom);
                    }
                }

                #endregion
            }
            if (input.RemoveInfoImage || forceAll)
            {
                #region Bild

                Images image = GraphicsManager.GetImage(entities, ActorCompanyId, SoeEntityImageType.EmployeePortrait, SoeEntityType.Employee, input.EmployeeId, false);
                if (image != null)
                    entities.DeleteObject(image);

                #endregion
            }
            if (input.RemoveInfoBankAccount || forceAll)
            {
                #region Bankkonton

                // Clear bank account fields

                employee.DisbursementMethod = (int)TermGroup_EmployeeDisbursementMethod.Unknown;
                employee.DisbursementClearingNr = null;
                employee.DisbursementAccountNr = null;
                employee.DontValidateDisbursementAccountNr = false;

                #endregion
            }
            if (input.RemoveInfoSkill || forceAll)
            {
                #region Kompetenser

                // Delete all skill records

                if (!employee.EmployeeSkill.IsLoaded)
                    employee.EmployeeSkill.Load();

                foreach (EmployeeSkill skill in employee.EmployeeSkill.ToList())
                {
                    entities.DeleteObject(skill);
                }

                // Delete all position records

                if (!employee.EmployeePosition.IsLoaded)
                    employee.EmployeePosition.Load();

                foreach (EmployeePosition position in employee.EmployeePosition.ToList())
                {
                    entities.DeleteObject(position);
                }

                #endregion
            }
        }

        private void DeleteEmployeeUnidentify(CompEntities entities, Employee employee, DeleteEmployeeDTO input)
        {
            // First remove personal information
            DeleteEmployeeRemoveInfo(entities, employee, input, true);

            // Set emp as deleted
            employee.State = (int)SoeEntityState.Deleted;
            SetDeletedProperties(employee);

            // A job will do the actual unidentify
            employee.JobDate = null;

            // Set user as deleted
            if (employee.UserId.HasValue)
            {
                if (employee.User == null && !employee.UserReference.IsLoaded)
                    employee.UserReference.Load();

                if (employee.User != null)
                    employee.User.State = (int)SoeEntityState.Deleted;
            }
        }

        private void DeleteEmployeeDelete(CompEntities entities, Employee employee, DeleteEmployeeDTO input)
        {
            // Currently same as unidentify
            DeleteEmployeeUnidentify(entities, employee, input);
        }

        public ActionResult UnidentifyEmployees()
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Perform

                        Dictionary<int, int> unidentified = new Dictionary<int, int>();

                        // Get validEmployees to unidentify
                        List<Employee> employees = (from e in entities.Employee
                                                    where e.State == (int)SoeEntityState.Deleted &&
                                                    !e.JobDate.HasValue
                                                    select e).ToList();

                        foreach (Employee employee in employees)
                        {
                            UnidentifyEmployee(employee);

                            if (!unidentified.ContainsKey(employee.ActorCompanyId))
                                unidentified.Add(employee.ActorCompanyId, 0);

                            unidentified[employee.ActorCompanyId]++;
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                            result.IntDict = unidentified;
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        private void UnidentifyEmployee(Employee employee)
        {
            // Run by job

            // Unidentify personal identifiable information

            if (!employee.ContactPersonReference.IsLoaded)
                employee.ContactPersonReference.Load();

            ContactPerson contact = employee.ContactPerson;
            contact.FirstName = GetText(2244, "Borttagen");
            contact.LastName = GetText(2244, "Borttagen");
            contact.Position = 0;
            contact.SocialSec = null;
            contact.Sex = 0;
            contact.State = (int)SoeEntityState.Deleted;
            SetModifiedProperties(contact);

            employee.JobDate = DateTime.Now;
            SetModifiedProperties(employee);
        }

        #endregion

        #region Hidden Employee (staffing)

        public Employee GetHiddenEmployee(int actorCompanyId, bool loadContact = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetHiddenEmployee(entities, actorCompanyId, loadContact);
        }

        public Employee GetHiddenEmployee(CompEntities entities, int actorCompanyId, bool loadContact = false)
        {
            Employee employee = (from e in entities.Employee
                                 where e.Hidden &&
                                 e.ActorCompanyId == actorCompanyId &&
                                 e.State == (int)SoeEntityState.Active && !e.Vacant
                                 select e).FirstOrDefault();

            if (employee != null && loadContact && !employee.ContactPersonReference.IsLoaded)
                employee.ContactPersonReference.Load();

            return employee;
        }

        public int GetHiddenEmployeeId(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetHiddenEmployeeId(entities, actorCompanyId);
        }

        public int GetHiddenEmployeeId(CompEntities entities, int actorCompanyId)
        {
            return GetHiddenEmployee(entities, actorCompanyId)?.EmployeeId ?? 0;
        }

        private bool HiddenEmployeeExists(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return HiddenEmployeeExists(entities, actorCompanyId);
        }

        private bool HiddenEmployeeExists(CompEntities entities, int actorCompanyId)
        {
            return GetHiddenEmployee(entities, actorCompanyId) != null;
        }

        public ActionResult AddHiddenEmployee(int actorCompanyId)
        {
            if (HiddenEmployeeExists(actorCompanyId))
                return new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                EmployeeGroup employeeGroup = GetDefaultOrFirstEmployeeGroup(entities, actorCompanyId);
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeGroup");

                Employee employee = new Employee()
                {
                    EmployeeNr = "0",
                    Hidden = true,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                };
                SetCreatedProperties(employee);
                entities.Employee.AddObject(employee);

                //ContactPerson
                employee.ContactPerson = new ContactPerson()
                {
                    FirstName = GetText(5580, "Ledigt"),
                    LastName = GetText(5581, "pass"),
                    Position = 0,
                    SocialSec = null,
                };
                SetCreatedProperties(employee.ContactPerson);

                //Actor
                employee.ContactPerson.Actor = new Actor()
                {
                    ActorType = (int)SoeActorType.ContactPerson,
                };
                SetCreatedProperties(employee.ContactPerson);

                //Employment
                AddEmploymentIfNotExists(entities, null, null, employee, employeeGroup);

                return SaveChanges(entities);
            }
        }

        #endregion

        #region Vacant Employee (staffing)

        public List<Employee> GetVacantEmployees(int actorCompanyId, bool loadContact = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetVacantEmployees(entities, actorCompanyId, loadContact);
        }

        public List<Employee> GetVacantEmployees(CompEntities entities, int actorCompanyId, bool loadContact = false)
        {
            IQueryable<Employee> query = entities.Employee;
            if (loadContact)
                query = query.Include("ContactPerson");

            return (from e in query
                    where e.ActorCompanyId == actorCompanyId &&
                    e.Vacant
                    select e).ToList();
        }

        public List<int> GetVacantEmployeeIds(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employee.NoTracking();
            return GetVacantEmployeeIds(entities, actorCompanyId);
        }

        public List<int> GetVacantEmployeeIds(CompEntities entities, int actorCompanyId)
        {
            return (from e in entities.Employee
                    where e.ActorCompanyId == actorCompanyId &&
                    e.Vacant
                    select e.EmployeeId).ToList();
        }

        #endregion

        #region Employee LAS

        public int GetLasDays(CompEntities entities, int actorCompanyId, Employee employee, DateTime startDate, DateTime stopDate)
        {
            if (employee == null)
                return 0;

            var employeefactorsForCompany = base.GetEmployeeFactorsFromCache(entities, CacheConfig.Company(actorCompanyId));
            var employeefactors = GetEmployeeFactors(entities, employee.EmployeeId, new List<TermGroup_EmployeeFactorType>() { TermGroup_EmployeeFactorType.BalanceLasDays, TermGroup_EmployeeFactorType.CurrentLasDays }, stopDate);
            var employeeFactorCurrent = employeefactors.OrderByDescending(o => o.FromDate).FirstOrDefault(f => f.Type == (int)TermGroup_EmployeeFactorType.CurrentLasDays);

            if (employeeFactorCurrent != null)
                return Convert.ToInt32(employeeFactorCurrent.Factor);

            var employeeFactorBalance = employeefactorsForCompany.FirstOrDefault(f => f.Type == (int)TermGroup_EmployeeFactorType.BalanceLasDays);
            var balance = 0;
            if (employeeFactorBalance?.FromDate != null)
            {
                startDate = employeeFactorBalance.FromDate.Value;
                balance = Convert.ToInt32(employeeFactorBalance.Factor);
            }

            var lasInformation = GetLasInformation(entities, actorCompanyId, new List<Employee>() { employee }, startDate, stopDate);
            return balance + lasInformation.Sum(s => s.NumberOfLASDays);
        }

        public int GetLasDays(CompEntities entities, int actorCompanyId, Employee employee, DateTime stopDate)
        {
            return GetLasDays(entities, actorCompanyId, employee, CalendarUtility.DATETIME_DEFAULT, stopDate);
        }

        public List<EmploymentLASDTO> GetLasInformation(CompEntities entities, int actorCompanyId, List<Employee> employees, DateTime startDate, DateTime stopDate)
        {
            List<EmploymentLASDTO> dtos = new List<EmploymentLASDTO>();
            var calenders = GetEmploymentCalenderDTOs(entities, employees, startDate, stopDate, base.GetEmployeeGroupsFromCache(entities, CacheConfig.Company(actorCompanyId)), base.GetPayrollGroupsFromCache(entities, CacheConfig.Company(actorCompanyId)), base.GetPayrollPriceTypesFromCache(entities, CacheConfig.Company(actorCompanyId)), base.GetVacationGroupsFromCache(entities, CacheConfig.Company(actorCompanyId)), base.GetEmploymentTypesFromCache(entities, CacheConfig.Company(actorCompanyId), TermGroup_Languages.Swedish), base.GetAnnualLeaveGroupsFromCache(entities, CacheConfig.Company(actorCompanyId)));

            foreach (var item in calenders.GroupBy(g => g.EmployeeId))
            {
                dtos.AddRange(GetLasInformationForEmployee(entities, item.Key, item.ToList()));
            }

            return dtos;
        }

        public List<EmploymentLASDTO> GetLasInformationForEmployee(CompEntities entities, int employeeId, List<EmploymentCalenderDTO> employmentCalenderDTOs)
        {
            employmentCalenderDTOs = employmentCalenderDTOs.OrderBy(o => o.Date).ToList();

            if (!employmentCalenderDTOs.Any(a => a.EmploymentId != 0))
                return new List<EmploymentLASDTO>();

            var currentDate = employmentCalenderDTOs.LastOrDefault(f => f.EmploymentId != 0).Date;
            var stopDate = employmentCalenderDTOs.FirstOrDefault(f => f.EmploymentId != 0).Date;
            var dict = employmentCalenderDTOs.GroupBy(g => g.Date).ToDictionary(k => k.Key, v => v.First());
            List<EmploymentLASDTO> dtos = new List<EmploymentLASDTO>();
            EmploymentLASDTO previousDTO = null;
            bool isOneDay = currentDate == stopDate;

            if (currentDate <= stopDate)
            {
                var onDate = dict[currentDate];
                previousDTO = new EmploymentLASDTO()
                {
                    StartDate = currentDate,
                    StopDate = currentDate,
                    EmploymentLASType = (TermGroup_EmploymentType)onDate.EmploymentType == TermGroup_EmploymentType.SE_SpecialFixedTerm ? EmploymentLASType.Sva :
                        (TermGroup_EmploymentType)onDate.EmploymentType == TermGroup_EmploymentType.SE_FixedTerm ? EmploymentLASType.Ava :
                        (TermGroup_EmploymentType)onDate.EmploymentType == TermGroup_EmploymentType.SE_Substitute || (TermGroup_EmploymentType)onDate.EmploymentType == TermGroup_EmploymentType.SE_SubstituteVacation ? EmploymentLASType.Vik :
                        EmploymentLASType.Unknown,
                    EmploymentType = (TermGroup_EmploymentType)onDate.EmploymentType,
                    EmploymentTypeId = onDate.EmploymentTypeId,
                    EmploymentTypeName = onDate.EmploymentTypeName,
                    CountOnlyOnSpecificType = false,
                    NumberOfLASDays = 1,
                    NumberOfCalenderDays = 1,
                    EmploymentId = onDate.EmploymentId,
                };
                dtos.Add(previousDTO);
            }
            else
            {
                while (currentDate >= stopDate)
                {
                    var onDate = dict[currentDate];

                    if ((TermGroup_EmploymentType)onDate.EmploymentType == TermGroup_EmploymentType.Unknown)
                    {
                        var unknownDTO = new EmploymentLASDTO()
                        {
                            StartDate = currentDate,
                            StopDate = currentDate,
                            EmploymentLASType = EmploymentLASType.Unknown,
                            EmploymentType = (TermGroup_EmploymentType)onDate.EmploymentType,
                            EmploymentTypeId = onDate.EmploymentTypeId,
                            EmploymentTypeName = onDate.EmploymentTypeName,
                            CountOnlyOnSpecificType = false,
                            NumberOfLASDays = 1,
                            NumberOfCalenderDays = 1,
                            EmploymentId = onDate.EmploymentId,
                        };
                        dtos.Add(unknownDTO);

                        currentDate = currentDate.AddDays(-1);
                        continue;
                    }

                    bool createNew = false;
                    if (previousDTO == null)
                    {
                        createNew = true;
                    }
                    else if (previousDTO.EmploymentLASType == EmploymentLASType.Ava && previousDTO.EmploymentId == onDate.EmploymentId && currentDate == new DateTime(2022, 10, 1))
                    {
                        previousDTO.StartDate = currentDate;
                        previousDTO.NumberOfCalenderDays++;
                        previousDTO.NumberOfLASDays++;
                        previousDTO.EmploymentLASType = EmploymentLASType.Sva;
                        dtos.Add(previousDTO.CloneDTO());
                        previousDTO = null;
                    }
                    else if (previousDTO.EmploymentLASType == EmploymentLASType.Ava && previousDTO.EmploymentId == onDate.EmploymentId && currentDate == new DateTime(2022, 3, 1))
                    {
                        previousDTO.StartDate = currentDate;
                        previousDTO.NumberOfCalenderDays++;
                        previousDTO.NumberOfLASDays++;
                        dtos.Add(previousDTO.CloneDTO());
                        previousDTO = null;
                    }
                    else if (previousDTO.EmploymentType == (TermGroup_EmploymentType)onDate.EmploymentType && previousDTO.EmploymentId == onDate.EmploymentId)
                    {
                        previousDTO.NumberOfCalenderDays++;
                        previousDTO.NumberOfLASDays++;
                        previousDTO.StartDate = currentDate;
                    }
                    else
                    {
                        var clone = previousDTO.CloneDTO();
                        dtos.Add(clone);
                        previousDTO = null;
                        createNew = true;
                    }

                    if (createNew)
                    {
                        previousDTO = new EmploymentLASDTO()
                        {
                            StartDate = currentDate,
                            StopDate = currentDate,
                            EmploymentLASType = (TermGroup_EmploymentType)onDate.EmploymentType == TermGroup_EmploymentType.SE_SpecialFixedTerm ? EmploymentLASType.Sva :
                        (TermGroup_EmploymentType)onDate.EmploymentType == TermGroup_EmploymentType.SE_FixedTerm ? EmploymentLASType.Ava :
                        (TermGroup_EmploymentType)onDate.EmploymentType == TermGroup_EmploymentType.SE_Substitute || (TermGroup_EmploymentType)onDate.EmploymentType == TermGroup_EmploymentType.SE_SubstituteVacation ? EmploymentLASType.Vik :
                        EmploymentLASType.Unknown,
                            EmploymentType = (TermGroup_EmploymentType)onDate.EmploymentType,
                            EmploymentTypeId = onDate.EmploymentTypeId,
                            EmploymentTypeName = onDate.EmploymentTypeName,
                            CountOnlyOnSpecificType = false,
                            NumberOfLASDays = 1,
                            NumberOfCalenderDays = 1,
                            EmploymentId = onDate.EmploymentId,
                        };
                    }

                    currentDate = currentDate.AddDays(-1);
                }
            }
            if (previousDTO != null && !isOneDay)
                dtos.Add(previousDTO.CloneDTO());

            dtos = dtos.Where(w => w.EmploymentId != 0).ToList();

            List<EmploymentLASDTO> extraSvas = new List<EmploymentLASDTO>();
            List<EmploymentLASDTO> removeAvas = new List<EmploymentLASDTO>();
            foreach (var avaDTO in dtos.Where(w => w.EmploymentLASType == EmploymentLASType.Ava))
            {
                avaDTO.NumberOfLASDays = avaDTO.NumberOfCalenderDays;

                if (avaDTO.StartDate >= new DateTime(2022, 3, 1))
                {
                    avaDTO.CountOnlyOnSpecificType = true;
                    var extraSva = avaDTO.CloneDTO();
                    extraSva.EmploymentLASType = EmploymentLASType.Sva;
                    extraSva.NumberOfLASDays = extraSva.NumberOfCalenderDays;
                    extraSva.CountOnlyOnSpecificType = false;
                    extraSvas.Add(extraSva);

                    //add the original ava segment for removal
                    removeAvas.Add(avaDTO);
                }
            }
            dtos.AddRange(extraSvas);

            //remove any ava segment
            foreach (var avaDTO in removeAvas)
                dtos.Remove(avaDTO);


            var currentSvaDate = new DateTime(2022, 3, 1);
            var stopSvaDate = dtos.OrderBy(o => o.StopDate).Last().StopDate;
            while (currentSvaDate <= stopSvaDate)
            {
                var startMonth = CalendarUtility.GetBeginningOfMonth(currentSvaDate);
                var endMonth = CalendarUtility.GetEndOfMonth(currentSvaDate);
                var dtosMonth = dtos.Where(w => w.EmploymentLASType == EmploymentLASType.Sva && CalendarUtility.GetOverlappingMinutes(w.StartDate, CalendarUtility.GetEndOfDay(w.StopDate), startMonth, endMonth) != 0).ToList();
                if (dtosMonth.Count > 2)
                {
                    int daysMonth = CalendarUtility.GetTotalDays(startMonth, endMonth) + 1;

                    var innerStart = dtosMonth.OrderBy(o => o.StartDate).First().StartDate;

                    if (innerStart < startMonth)
                        innerStart = startMonth;

                    var innerStop = dtosMonth.OrderBy(o => o.StartDate).Last().StopDate;

                    if (innerStop > endMonth)
                        innerStop = endMonth;

                    var innerCurrent = innerStart;

                    // if 30-day-rule should be checked (full month from edge to edge then always 30 LAS-days)
                    bool check30DaysRule = innerStart == startMonth && innerStop == endMonth;

                    while (innerCurrent <= innerStop)
                    {
                        var innerDTO = dtosMonth.FirstOrDefault(w => w.EmploymentLASType == EmploymentLASType.Sva && CalendarUtility.GetOverlappingMinutes(w.StartDate, CalendarUtility.GetEndOfDay(w.StopDate), innerCurrent, CalendarUtility.GetEndOfDay(innerCurrent)) != 0);

                        if (innerDTO != null)
                        {
                            var extraSva = innerDTO.CloneDTO();
                            extraSva.StartDate = innerDTO.StopDate.AddDays(1);
                            extraSva.StopDate = innerCurrent;
                            extraSva.Info = "Utfyllnad";

                            var nextDTO = dtosMonth.OrderBy(o => o.StartDate).FirstOrDefault(w => w.StartDate > innerDTO.StopDate);

                            if (nextDTO != null)
                                extraSva.StopDate = nextDTO.StartDate.AddDays(-1);

                            innerCurrent = extraSva.StopDate;

                            if (extraSva.StopDate >= extraSva.StartDate)
                            {
                                extraSva.NumberOfLASDays = Convert.ToInt32((extraSva.StopDate - extraSva.StartDate).TotalDays + 1);
                                extraSva.NumberOfCalenderDays = extraSva.NumberOfLASDays;
                                dtos.Add(extraSva);
                            }
                        }

                        innerCurrent = innerCurrent.AddDays(1);
                    }

                    // check amount of LAS days and adjust to 30
                    if (check30DaysRule && daysMonth != 30)
                    {
                        int adjustment30DaysRule = 30 - daysMonth;
                        var lastFillerDTO = dtos.LastOrDefault(e => e.Info?.StartsWith("Utfyllnad") ?? false);
                        if (lastFillerDTO != null)
                        {
                            lastFillerDTO.NumberOfLASDays += adjustment30DaysRule;
                            lastFillerDTO.Info = "Utfyllnad med justering";
                        }
                    }
                }
                currentSvaDate = currentSvaDate.AddMonths(1);

            }

            return dtos.OrderBy(o => o.StartDate).ToList();
        }

        #endregion

        #region EmployeeAccount

        public List<EmployeeAccount> GetEmployeeAccounts(int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeAccounts(entities, actorCompanyId, employeeIds, dateFrom, dateTo);
        }

        public List<EmployeeAccount> GetEmployeeAccounts(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo, List<EmployeeAccount> inputEmployeeAccounts = null)
        {
            List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
            int batchSize = inputEmployeeAccounts.IsNullOrEmpty() ? 2000 : inputEmployeeAccounts.Count;

            if (batchSize == 0)
                return employeeAccounts;

            List<int> unhandledEmployeeIds = employeeIds;
            List<int> batchIds = new List<int>();

            while (unhandledEmployeeIds.Any())
            {
                batchIds = unhandledEmployeeIds.Take(batchSize).ToList();
                unhandledEmployeeIds = unhandledEmployeeIds.Skip(batchSize).ToList();
                employeeAccounts.AddRange(GetEmployeeAccountsBatch(entities, actorCompanyId, batchIds, dateFrom, dateTo, inputEmployeeAccounts));
            }

            return employeeAccounts.Where(ea => ea.AccountId.HasValue).ToList();
        }

        private List<EmployeeAccount> GetEmployeeAccountsBatch(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo, List<EmployeeAccount> inputEmployeeAccounts = null)
        {
            List<EmployeeAccount> employeeAccounts = null;

            if (inputEmployeeAccounts == null)
            {

                employeeAccounts = (from ea in entities.EmployeeAccount
                                    .Include("Account.AccountDim")
                                    .Include("Parent.Parent.Parent.Parent")
                                    where ea.State == (int)SoeEntityState.Active &&
                                    employeeIds.Contains(ea.EmployeeId) &&
                                    (!ea.DateTo.HasValue || ea.DateTo.Value >= dateFrom) &&
                                    ea.DateFrom <= dateTo
                                    select ea).ToList();
            }
            else
            {
                employeeAccounts = (from ea in inputEmployeeAccounts
                                    where ea.ActorCompanyId == actorCompanyId &&
                                    ea.State == (int)SoeEntityState.Active &&
                                    employeeIds.Contains(ea.EmployeeId) &&
                                    (!ea.DateTo.HasValue || ea.DateTo.Value >= dateFrom) &&
                                    ea.DateFrom <= dateTo
                                    select ea).ToList();
            }

            //Post query filtering
            return employeeAccounts.Where(ea => ea.AccountId.HasValue).ToList();
        }

        public List<EmployeeAccount> GetEmployeeAccounts(CompEntities entities, int actorCompanyId, List<int> employeeIds = null)
        {
            List<EmployeeAccount> employeeAccounts = null;

            var query = (from ea in entities.EmployeeAccount
                         where ea.ActorCompanyId == actorCompanyId &&
                         ea.State == (int)SoeEntityState.Active
                         select ea);

            bool useEmployeeIdsInQuery = UseEmployeeIdsInQuery(entities, employeeIds);
            if (useEmployeeIdsInQuery)
                query = query.Where(ea => employeeIds.Contains(ea.EmployeeId));

            employeeAccounts = query.ToList();

            if (employeeIds != null && !useEmployeeIdsInQuery)
                employeeAccounts = employeeAccounts.Where(ea => employeeIds.Contains(ea.EmployeeId)).ToList();

            //Post query filtering
            employeeAccounts = employeeAccounts.Where(i => i.AccountId.HasValue).ToList();

            return employeeAccounts;
        }

        public List<EmployeeAccount> GetEmployeeAccounts(int actorCompanyId, int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeAccount.NoTracking();
            return GetEmployeeAccounts(entities, actorCompanyId, employeeId, dateFrom, dateTo);
        }

        public List<EmployeeAccount> GetEmployeeAccounts(CompEntities entities, int actorCompanyId, int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var employeeAccounts = (from ea in entities.EmployeeAccount
                                    where ea.ActorCompanyId == actorCompanyId &&
                                    ea.EmployeeId == employeeId &&
                                    ea.AccountId.HasValue &&
                                    ea.State == (int)SoeEntityState.Active
                                    select ea).ToList();

            return employeeAccounts.GetEmployeeAccounts(dateFrom, dateTo);
        }

        public List<int> GetEmployeeAccountIds(int actorCompanyId, int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeAccountIds(entities, actorCompanyId, employeeId, dateFrom, dateTo);
        }

        public List<int> GetCachedEmployeeAccountIds(CompEntities entities, int actorCompanyId, int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null, bool useCache = true)
        {
            string key = $"GetEmployeeAccountIds#employeeId{employeeId}{dateFrom}{dateTo}";
            List<int> accountIds = useCache ? BusinessMemoryCache<List<int>>.Get(key) : null;
            if (accountIds == null)
            {
                accountIds = GetEmployeeAccountIds(entities, actorCompanyId, employeeId, dateFrom, dateTo);
                BusinessMemoryCache<List<int>>.Set(key, accountIds, 5);
            }
            return accountIds;
        }

        public List<int> GetEmployeeAccountIds(CompEntities entities, int actorCompanyId, int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            return GetEmployeeAccounts(entities, actorCompanyId, employeeId, dateFrom, dateTo).Select(e => e.AccountId.Value).ToList();
        }

        public int GetDefaultEmployeeAccountId(int actorCompanyId, int employeeId, DateTime? date)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.EmployeeAccount.NoTracking();
            return (from ea in entitiesReadOnly.EmployeeAccount
                    where ea.ActorCompanyId == actorCompanyId &&
                    ea.EmployeeId == employeeId &&
                    ea.AccountId.HasValue &&
                    ea.Default &&
                    !ea.ParentEmployeeAccountId.HasValue &&
                    ea.State == (int)SoeEntityState.Active &&
                    (!date.HasValue || (ea.DateFrom <= date.Value && (!ea.DateTo.HasValue || ea.DateTo.Value >= date)))
                    select ea.AccountId.Value).FirstOrDefault();
        }

        public List<int> GetEmployeeAccountIds(int actorCompanyId, int employeeId, DateTime? date)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeAccount.NoTracking();
            return GetEmployeeAccountIds(entities, actorCompanyId, employeeId, date);
        }

        public List<int> GetEmployeeAccountIds(CompEntities entities, int actorCompanyId, int employeeId, DateTime? date)
        {
            List<int> accountIds = (from ea in entities.EmployeeAccount
                                    where ea.ActorCompanyId == actorCompanyId &&
                                    ea.EmployeeId == employeeId &&
                                    ea.AccountId.HasValue &&
                                    ea.State == (int)SoeEntityState.Active &&
                                    (!date.HasValue || (ea.DateFrom <= date.Value && (!ea.DateTo.HasValue || ea.DateTo.Value >= date)))
                                    select ea.AccountId.Value).ToList();

            return accountIds;
        }

        public bool HasMultipelEmployeeAccounts(int actorCompanyId, int employeeId, DateTime? dateFrom, DateTime? dateTo, List<EmployeeAccount> employeeAccounts = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return HasMultipelEmployeeAccounts(entities, actorCompanyId, employeeId, dateFrom, dateTo, employeeAccounts);
        }

        public bool HasMultipelEmployeeAccounts(CompEntities entities, int actorCompanyId, int employeeId, DateTime? dateFrom, DateTime? dateTo, List<EmployeeAccount> employeeAccounts = null)
        {
            dateFrom = dateFrom.HasValue ? dateFrom.Value.Date : DateTime.MinValue;
            dateTo = dateTo.HasValue ? dateTo.Value.Date : DateTime.MaxValue;

            if (employeeAccounts == null)
                employeeAccounts = GetHighestEmployeeAccounts(entities, actorCompanyId, employeeId);

            if (employeeAccounts.Count < 2)
                return false;

            int counter = 0;
            foreach (EmployeeAccount employeeAccount in employeeAccounts)
            {
                if (CalendarUtility.IsDatesOverlapping(employeeAccount.DateFrom, employeeAccount.DateTo ?? DateTime.MaxValue, dateFrom.Value, dateTo.Value))
                    counter++;
            }

            return counter > 1;
        }

        public List<EmployeeAccount> GetHighestEmployeeAccounts(int actorCompanyId, int employeeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetHighestEmployeeAccounts(entities, actorCompanyId, employeeId);
        }

        public List<EmployeeAccount> GetHighestEmployeeAccounts(CompEntities entities, int actorCompanyId, int employeeId)
        {
            return (from ea in entities.EmployeeAccount
                    where ea.ActorCompanyId == actorCompanyId &&
                    ea.EmployeeId == employeeId &&
                    ea.AccountId.HasValue &&
                    !ea.ParentEmployeeAccountId.HasValue &&
                    ea.State == (int)SoeEntityState.Active
                    select ea).ToList();
        }

        public List<EmployeeAccount> GetHighestEmployeeAccountOnAccount(List<EmployeeAccount> employeeAccounts, int accountId, DateTime? date)
        {
            if (employeeAccounts == null)
                return new List<EmployeeAccount>();

            return (from ea in employeeAccounts
                    where ea.AccountId.HasValue &&
                    ea.AccountId == accountId &&
                    !ea.ParentEmployeeAccountId.HasValue &&
                    (!date.HasValue || (ea.DateFrom <= date.Value && (!ea.DateTo.HasValue || ea.DateTo.Value >= date))) &&
                    ea.State == (int)SoeEntityState.Active
                    select ea).ToList();
        }


        #endregion

        #region EmployeeChild

        public EmployeeChild GetEmployeeChild(CompEntities entities, int employeeChildId)
        {
            return (from es in entities.EmployeeChild
                    where es.EmployeeChildId == employeeChildId &&
                          es.State == (int)SoeEntityState.Active
                    select es).FirstOrDefault();
        }

        public Dictionary<int, string> GetEmployeeChildsDict(int employeeId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var childs = GetEmployeeChilds(employeeId, base.ActorCompanyId);
            foreach (EmployeeChild child in childs.OrderBy(eg => eg.FirstName))
            {
                if (!dict.ContainsKey(child.EmployeeChildId))
                    dict.Add(child.EmployeeChildId, child.Name);
            }

            return dict;
        }

        public List<EmployeeChild> GetEmployeeChildsForCompnay(CompEntities entities, int actorCompanyId)
        {
            return (from es in entities.EmployeeChild
                    where es.Employee.ActorCompanyId == actorCompanyId &&
                          es.State == (int)SoeEntityState.Active
                    select es).ToList();
        }

        public List<EmployeeChild> GetEmployeeChilds(int employeeId, int actorCompanyId = 0, bool includeUsedDays = false, AttestState attestStateResultingPayroll = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeChild.NoTracking();
            return GetEmployeeChilds(entities, employeeId, actorCompanyId, includeUsedDays, attestStateResultingPayroll);
        }

        public List<EmployeeChild> GetEmployeeChilds(CompEntities entities, int employeeId, int actorCompanyId, bool includeUsedDays = false, AttestState attestStateResultingPayroll = null)
        {
            List<EmployeeChild> childs = (from es in entities.EmployeeChild
                                          where es.EmployeeId == employeeId &&
                                          es.State == (int)SoeEntityState.Active
                                          select es).ToList();

            if (includeUsedDays)
            {
                List<TimePayrollTransaction> parentalLeaveTransactions = TimeTransactionManager.GetTimePayrollTransactionsForEmployee(entities, employeeId, sysPayrollTypeLevel3: TermGroup_SysPayrollType.SE_GrossSalary_Absence_ParentalLeave).Where(x => x.EmployeeChildId.HasValue).ToList();
                if (attestStateResultingPayroll == null)
                    attestStateResultingPayroll = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, 0, actorCompanyId, 0));

                foreach (EmployeeChild child in childs)
                {
                    int nrOfAbsenceDays = 0;
                    int nrOfAbsenceDaysPayroll = 0;
                    foreach (var timePayrollTransactionsByTimeBlockDate in parentalLeaveTransactions.Where(x => x.EmployeeChildId.HasValue && x.EmployeeChildId.Value == child.EmployeeChildId).GroupBy(i => i.TimeBlockDateId))
                    {
                        decimal quantity = timePayrollTransactionsByTimeBlockDate.Sum(i => i.Quantity);
                        if (quantity > 0)
                        {
                            nrOfAbsenceDays++;
                            if (attestStateResultingPayroll != null && timePayrollTransactionsByTimeBlockDate.Any(x => x.AttestStateId == attestStateResultingPayroll.AttestStateId && !x.IsReversed))
                                nrOfAbsenceDaysPayroll++;
                        }
                        else if (quantity == 0 && !timePayrollTransactionsByTimeBlockDate.Any(i => i.IsReversed))
                        {
                            nrOfAbsenceDays++;
                            if (attestStateResultingPayroll != null && timePayrollTransactionsByTimeBlockDate.Any(x => x.AttestStateId == attestStateResultingPayroll.AttestStateId))
                                nrOfAbsenceDaysPayroll++;
                        }
                    }

                    child.UsedDays = nrOfAbsenceDays;
                    child.UsedDaysPayroll = nrOfAbsenceDaysPayroll;
                }
            }
            return childs;
        }

        public List<EmployeeChildCareDTO> GetEmployeeChildCareDTOs(int employeeId, int actorCompanyId, AttestState attestStateResultingPayroll = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeChild.NoTracking();
            return GetEmployeeChildCareDTOs(entities, employeeId, actorCompanyId, attestStateResultingPayroll);
        }

        public List<EmployeeChildCareDTO> GetEmployeeChildCareDTOs(CompEntities entities, int employeeId, int actorCompanyId, AttestState attestStateResultingPayroll = null)
        {
            var result = new List<EmployeeChildCareDTO>();

            var employee = GetEmployee(entities, employeeId, actorCompanyId, loadEmployment: true, loadVacationGroup: true);
            if (employee == null)
                return result;

            var vacationGroup = employee.GetVacationGroup().ToDTO();
            if (vacationGroup == null)
                return result;

            var transactions = TimeTransactionManager
                .GetTimePayrollTransactionsForEmployee(entities, employeeId, sysPayrollTypeLevel3: TermGroup_SysPayrollType.SE_GrossSalary_Absence_TemporaryParentalLeave)
                .Where(t => t.TimeBlockDate != null)
                .ToList();
            if (transactions.IsNullOrEmpty())
                return result;

            var startDate = transactions.Min(t => t.TimeBlockDate.Date);
            var stopDate = transactions.Max(t => t.TimeBlockDate.Date);
            var vacationYearIntervals = GetVacationYearIntervals(vacationGroup, startDate, stopDate);
            if (vacationYearIntervals.IsNullOrEmpty())
                return result;

            var childs = GetEmployeeChilds(employeeId, actorCompanyId);
            int nbrOfChildCareDaysPerYear = childs.Any(child => child.SingelCustody) ? 180 : 120;

            if (attestStateResultingPayroll == null)
                attestStateResultingPayroll = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, 0, actorCompanyId, 0));

            foreach (var vacationYearInterval in vacationYearIntervals)
            {
                var timePayrollTransactionsByInterval = transactions.Where(t => t.TimeBlockDate.Date >= vacationYearInterval.Start && t.TimeBlockDate.Date <= vacationYearInterval.Stop).ToList();
                if (timePayrollTransactionsByInterval.Count > 0)
                {
                    foreach (var timePayrollTransactionsByChild in timePayrollTransactionsByInterval.GroupBy(x => x.EmployeeChildId))
                    {
                        var employeeChild = timePayrollTransactionsByChild.Key.HasValue ? childs.FirstOrDefault(x => x.EmployeeChildId == timePayrollTransactionsByChild.Key.Value) : null;

                        int nrOfAbsenceDays = 0;
                        int nrOfAbsenceDaysPayroll = 0;
                        foreach (var timePayrollTransactionsByTimeBlockDate in timePayrollTransactionsByChild.GroupBy(i => i.TimeBlockDateId))
                        {
                            decimal quantity = timePayrollTransactionsByTimeBlockDate.Sum(i => i.Quantity);
                            if (quantity > 0)
                            {
                                nrOfAbsenceDays++;
                                if (attestStateResultingPayroll != null && timePayrollTransactionsByTimeBlockDate.Any(x => x.AttestStateId == attestStateResultingPayroll.AttestStateId && !x.IsReversed))
                                    nrOfAbsenceDaysPayroll++;
                            }
                            else if (quantity == 0 && !timePayrollTransactionsByTimeBlockDate.Any(i => i.IsReversed))
                            {
                                nrOfAbsenceDays++;
                                if (attestStateResultingPayroll != null && timePayrollTransactionsByTimeBlockDate.Any(x => x.AttestStateId == attestStateResultingPayroll.AttestStateId))
                                    nrOfAbsenceDaysPayroll++;
                            }
                        }

                        result.Add(new EmployeeChildCareDTO()
                        {
                            Name = employeeChild?.Name ?? GetText(9971, "Barn ej angivet"),
                            DateFrom = vacationYearInterval.Start,
                            DateTo = vacationYearInterval.Stop,
                            NbrOfDays = nbrOfChildCareDaysPerYear,
                            OpeningBalanceUsedDays = 0,
                            UsedDays = nrOfAbsenceDays,
                            UsedDaysPayroll = nrOfAbsenceDaysPayroll,
                        });
                    }
                }
                else
                {
                    result.Add(new EmployeeChildCareDTO()
                    {
                        Name = GetText(9971, "Barn ej angivet"),
                        DateFrom = vacationYearInterval.Start,
                        DateTo = vacationYearInterval.Stop,
                        NbrOfDays = nbrOfChildCareDaysPerYear,
                        OpeningBalanceUsedDays = 0,
                        UsedDays = 0,
                        UsedDaysPayroll = 0,
                    });
                }
            }

            return result.OrderByDescending(r => r.DateFrom).ToList();
        }

        public static List<(DateTime Start, DateTime Stop)> GetVacationYearIntervals(VacationGroupDTO vacationGroup, DateTime startDate, DateTime stopDate)
        {
            var vacationGroupFromDate = vacationGroup.CalculateFromDate(DateTime.Today);
            var intervals = new List<(DateTime Start, DateTime Stop)>();

            var first = new DateTime(startDate.Year, vacationGroupFromDate.Month, vacationGroupFromDate.Day);
            if (first > startDate)
                first = new DateTime(startDate.Year - 1, vacationGroupFromDate.Month, vacationGroupFromDate.Day);

            DateTime current = first;
            while (current <= CalendarUtility.GetLatestDate(stopDate, DateTime.Today))
            {
                Add(current);
                current = GetNext(current);
            }

            void Add(DateTime intervalStartDate) => intervals.Add((intervalStartDate, GetStop(intervalStartDate)));
            DateTime GetNext(DateTime interval) => interval.AddYears(1);
            DateTime GetStop(DateTime interval) => GetNext(interval).AddDays(-1);

            return intervals;
        }

        public ActionResult SaveEmployeeChilds(CompEntities entities, TransactionScope transaction, Employee employee, List<EmployeeChildDTO> employeeChildsInput)
        {
            List<EmployeeChild> existingChilds = GetEmployeeChilds(entities, employee.EmployeeId, employee.ActorCompanyId);

            foreach (var employeeChildInput in employeeChildsInput)
            {
                if (employeeChildInput.EmployeeChildId == 0 && string.IsNullOrEmpty(employeeChildInput.FirstName) && string.IsNullOrEmpty(employeeChildInput.LastName) && !employeeChildInput.BirthDate.HasValue)
                    continue;

                EmployeeChild child = existingChilds.FirstOrDefault(x => x.EmployeeChildId == employeeChildInput.EmployeeChildId);

                if (child == null)
                {
                    //add
                    child = new EmployeeChild()
                    {
                        EmployeeId = employee.EmployeeId,
                        State = (int)SoeEntityState.Active,
                    };

                    SetCreatedProperties(child);
                    entities.EmployeeChild.AddObject(child);
                }
                else
                {
                    //update
                    child.State = (int)employeeChildInput.State;
                    SetModifiedProperties(child);
                }

                child.FirstName = !string.IsNullOrEmpty(employeeChildInput.FirstName) ? employeeChildInput.FirstName : "";
                child.LastName = !string.IsNullOrEmpty(employeeChildInput.LastName) ? employeeChildInput.LastName : "";
                child.BirthDate = employeeChildInput.BirthDate;
                child.SingelCustody = employeeChildInput.SingleCustody;
                child.OpeningBalanceUsedDays = employeeChildInput.OpeningBalanceUsedDays;
            }

            return SaveChanges(entities, transaction);
        }

        #endregion

        #region EmployeeCollectiveAgreement

        public List<EmployeeCollectiveAgreement> GetEmployeeCollectiveAgreements(int actorCompanyId, bool onlyActive = false, bool loadEmployeeGroup = false, bool loadPayrollGroup = false, bool loadVacationGroup = false, bool loadAnnualLeaveGroup = false, bool loadEmployeeTemplatesString = false, int? payrollGroupId = null, int? collectiveAgreementId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeCollectiveAgreement.NoTracking();
            return GetEmployeeCollectiveAgreements(entities, actorCompanyId, onlyActive, loadEmployeeGroup, loadPayrollGroup, loadVacationGroup, loadAnnualLeaveGroup, loadEmployeeTemplatesString, payrollGroupId, collectiveAgreementId);
        }

        public List<EmployeeCollectiveAgreement> GetEmployeeCollectiveAgreements(CompEntities entities, int actorCompanyId, bool onlyActive = false, bool loadEmployeeGroup = false, bool loadPayrollGroup = false, bool loadVacationGroup = false, bool loadAnnualLeaveGroup = false, bool loadEmployeeTemplatesString = false, int? payrollGroupId = null, int? collectiveAgreementId = null)
        {
            List<EmployeeCollectiveAgreement> collectiveAgreements = null;
            IQueryable<EmployeeCollectiveAgreement> query = entities.EmployeeCollectiveAgreement;

            if (loadEmployeeGroup)
                query = query.Include("EmployeeGroup");
            if (loadPayrollGroup)
                query = query.Include("PayrollGroup");
            if (loadVacationGroup)
                query = query.Include("VacationGroup");
            if (loadAnnualLeaveGroup)
                query = query.Include("AnnualLeaveGroup");
            if (loadEmployeeTemplatesString)
                query = query.Include("EmployeeTemplate");

            collectiveAgreements = (from ca in query
                                    where ca.ActorCompanyId == actorCompanyId &&
                                    (onlyActive ? ca.State == (int)SoeEntityState.Active : ca.State == (int)SoeEntityState.Active || ca.State == (int)SoeEntityState.Inactive)
                                    orderby ca.Name
                                    select ca).ToList();

            if (payrollGroupId.HasValue)
                collectiveAgreements = collectiveAgreements.Where(w => w.PayrollGroupId == payrollGroupId.Value).ToList();


            if (collectiveAgreementId != null)
                collectiveAgreements = collectiveAgreements.Where(c => c.EmployeeCollectiveAgreementId == collectiveAgreementId).ToList();

            return collectiveAgreements;
        }

        public Dictionary<int, string> GetEmployeeCollectiveAgreementsDict(int actorCompanyId, bool addEmptyRow, bool onlyActive = true)
        {
            Dictionary<int, string> dict = GetEmployeeCollectiveAgreements(actorCompanyId, onlyActive).ToDictionary(k => k.EmployeeCollectiveAgreementId, v => v.Name);
            if (addEmptyRow)
                dict.Add(0, " ");

            return dict.Sort();
        }

        public EmployeeCollectiveAgreement GetEmployeeCollectiveAgreement(int actorCompanyId, int employeeCollectiveAgreementId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeCollectiveAgreement.NoTracking();
            return GetEmployeeCollectiveAgreement(entities, actorCompanyId, employeeCollectiveAgreementId);
        }

        public EmployeeCollectiveAgreement GetEmployeeCollectiveAgreement(CompEntities entities, int actorCompanyId, int employeeCollectiveAgreementId)
        {
            return entities.EmployeeCollectiveAgreement.Include("EmployeeGroup").Include("PayrollGroup").Include("VacationGroup").Include("AnnualLeaveGroup").FirstOrDefault(w => w.ActorCompanyId == actorCompanyId && w.EmployeeCollectiveAgreementId == employeeCollectiveAgreementId && (w.State == (int)SoeEntityState.Active || w.State == (int)SoeEntityState.Inactive));
        }

        public ActionResult DeleteEmployeeCollectiveAgreement(int employeeCollectiveAgreementId)
        {
            using (CompEntities entities = new CompEntities())
            {
                EmployeeCollectiveAgreement employeeCollectiveAgreement = null;
                if (employeeCollectiveAgreementId != 0)
                {
                    employeeCollectiveAgreement = GetEmployeeCollectiveAgreement(entities, base.ActorCompanyId, employeeCollectiveAgreementId);

                    if (employeeCollectiveAgreement != null)
                    {
                        employeeCollectiveAgreement.State = (int)SoeEntityState.Deleted;
                        SetModifiedProperties(employeeCollectiveAgreement);
                        return SaveChanges(entities);
                    }
                }
            }

            return new ActionResult(false);
        }

        public ActionResult UpsertEmployeeCollectiveAgreement(EmployeeCollectiveAgreementDTO employeeCollectiveAgreementDTO, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            int employeeCollectiveAgreementId = employeeCollectiveAgreementDTO.EmployeeCollectiveAgreementId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        EmployeeCollectiveAgreement employeeCollectiveAgreement = null;
                        if (employeeCollectiveAgreementDTO.EmployeeCollectiveAgreementId != 0)
                            employeeCollectiveAgreement = GetEmployeeCollectiveAgreement(entities, actorCompanyId, employeeCollectiveAgreementDTO.EmployeeCollectiveAgreementId);

                        if (employeeCollectiveAgreement == null)
                        {
                            employeeCollectiveAgreement = new EmployeeCollectiveAgreement()
                            {
                                ActorCompanyId = actorCompanyId
                            };
                            entities.EmployeeCollectiveAgreement.AddObject(employeeCollectiveAgreement);
                            SetCreatedProperties(employeeCollectiveAgreement);
                        }
                        else
                            SetModifiedProperties(employeeCollectiveAgreement);

                        employeeCollectiveAgreement.EmployeeCollectiveAgreementId = employeeCollectiveAgreementDTO.EmployeeCollectiveAgreementId;
                        employeeCollectiveAgreement.Code = employeeCollectiveAgreementDTO.Code;
                        employeeCollectiveAgreement.ExternalCode = employeeCollectiveAgreementDTO.ExternalCode;
                        employeeCollectiveAgreement.Name = employeeCollectiveAgreementDTO.Name;
                        employeeCollectiveAgreement.Description = employeeCollectiveAgreementDTO.Description;
                        employeeCollectiveAgreement.EmployeeGroupId = employeeCollectiveAgreementDTO.EmployeeGroupId;
                        employeeCollectiveAgreement.PayrollGroupId = employeeCollectiveAgreementDTO.PayrollGroupId;
                        employeeCollectiveAgreement.VacationGroupId = employeeCollectiveAgreementDTO.VacationGroupId;
                        employeeCollectiveAgreement.AnnualLeaveGroupId = employeeCollectiveAgreementDTO.AnnualLeaveGroupId == 0 ? null : employeeCollectiveAgreementDTO.AnnualLeaveGroupId;
                        employeeCollectiveAgreement.State = (int)employeeCollectiveAgreementDTO.State;

                        result = SaveChanges(entities);

                        if (result.Success)
                        {
                            transaction.Complete();
                            employeeCollectiveAgreementId = employeeCollectiveAgreement.EmployeeCollectiveAgreementId;
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
                        // Set success properties
                        result.IntegerValue = employeeCollectiveAgreementId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }

        #endregion

        #region EmployeeFactor

        public List<EmployeeFactor> GetEmployeesFactors(List<int> employeeIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeFactor.NoTracking();
            return GetEmployeesFactors(entities, employeeIds);
        }

        public List<EmployeeFactor> GetEmployeesFactors(CompEntities entities, List<int> employeeIds)
        {
            var factors = (from f in entities.EmployeeFactor
                           where employeeIds.Contains(f.EmployeeId) &&
                           f.State == (int)SoeEntityState.Active
                           orderby f.FromDate descending
                           select f).ToList();

            return factors;
        }

        public List<EmployeeFactor> GetEmployeesFactorsForCompany(CompEntities entities, int actorCompanyId)
        {
            var factors = (from f in entities.EmployeeFactor
                           where f.Employee.ActorCompanyId == actorCompanyId &&
                           f.State == (int)SoeEntityState.Active
                           orderby f.FromDate descending
                           select f).ToList();

            return factors;
        }

        public List<EmployeeFactor> GetEmployeeFactors(int employeeId, TermGroup_EmployeeFactorType type)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeFactors(entities, employeeId, type);
        }

        public List<EmployeeFactor> GetEmployeeFactors(CompEntities entities, int employeeId, TermGroup_EmployeeFactorType type)
        {
            return (from f in entities.EmployeeFactor
                    where f.EmployeeId == employeeId &&
                    f.Type == (int)type &&
                    f.State == (int)SoeEntityState.Active
                    select f).ToList();
        }

        public List<EmployeeFactor> GetEmployeeFactors(CompEntities entities, int employeeId, List<TermGroup_EmployeeFactorType> factorTypes, DateTime date, List<EmployeeFactor> employeeFactors = null)
        {
            List<EmployeeFactor> filteredEmployeeFactors = new List<EmployeeFactor>();
            var intTypes = factorTypes.Select(x => (int)x).ToList();
            List<EmployeeFactor> factors = employeeFactors == null ?
                                                                    (from f in entities.EmployeeFactor
                                                                     where f.EmployeeId == employeeId &&
                                                                     intTypes.Contains(f.Type) &&
                                                                     f.State == (int)SoeEntityState.Active
                                                                     select f).ToList() :
                                                                    (from f in employeeFactors
                                                                     where f.EmployeeId == employeeId &&
                                                                     intTypes.Contains(f.Type) &&
                                                                     f.State == (int)SoeEntityState.Active
                                                                     select f).ToList();

            foreach (var factorType in factorTypes)
            {
                var factorsForType = (from f in factors
                                      where f.EmployeeId == employeeId &&
                                      f.Type == (int)factorType &&
                                     (f.FromDate <= date || !f.FromDate.HasValue) &&
                                      f.State == (int)SoeEntityState.Active
                                      orderby f.FromDate descending
                                      select f).FirstOrDefault();

                if (factorsForType != null)
                    filteredEmployeeFactors.Add(factorsForType);
            }

            return filteredEmployeeFactors;
        }

        public EmployeeFactor GetEmployeeFactor(List<EmployeeFactor> employeeFactors, DateTime date)
        {
            return (from f in employeeFactors
                    where (f.FromDate <= date || !f.FromDate.HasValue)
                    orderby f.FromDate descending
                    select f).FirstOrDefault();
        }
        public EmployeeFactor GetEmployeeFactor(TermGroup_EmployeeFactorType type, int employeeId, DateTime? date = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeFactor.NoTracking();
            return GetEmployeeFactor(entities, type, employeeId, date);
        }

        public EmployeeFactor GetEmployeeFactor(CompEntities entities, TermGroup_EmployeeFactorType type, int employeeId, DateTime? date = null)
        {
            var factors = (from f in entities.EmployeeFactor
                           where f.EmployeeId == employeeId &&
                           f.Type == (int)type &&
                          (f.FromDate <= date.Value || !f.FromDate.HasValue) &&
                           f.State == (int)SoeEntityState.Active
                           orderby f.FromDate descending
                           select f).FirstOrDefault();

            return factors;
        }

        public decimal GetEmployeeFactor(int employeeId, TermGroup_EmployeeFactorType type, DateTime? date = null, List<EmployeeFactor> employeeFactors = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeFactor.NoTracking();
            return GetEmployeeFactor(entities, employeeId, type, date, employeeFactors);
        }

        public decimal GetEmployeeFactor(CompEntities entities, int employeeId, TermGroup_EmployeeFactorType type, DateTime? date = null, List<EmployeeFactor> employeeFactors = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            EmployeeFactor factor = null;

            if (!employeeFactors.IsNullOrEmpty())
            {
                factor = (from f in employeeFactors
                          where f.EmployeeId == employeeId &&
                              f.Type == (int)type &&
                              (f.FromDate <= date.Value || !f.FromDate.HasValue) &&
                              f.State == (int)SoeEntityState.Active
                          orderby f.FromDate descending
                          select f).FirstOrDefault();
            }
            else
            {
                factor = (from f in entities.EmployeeFactor
                          where f.EmployeeId == employeeId &&
                          f.Type == (int)type &&
                          (f.FromDate <= date.Value || !f.FromDate.HasValue) &&
                          f.State == (int)SoeEntityState.Active
                          orderby f.FromDate descending
                          select f).FirstOrDefault();
            }

            //Dont use for now
            //if (factor != null && factor.TimeWorkAccountYearEmployeeId.HasValue)
            //{
            //    TimeWorkAccountYearDTO timeWorkAccountYear = GetWorkAccountYear(entities, factor.TimeWorkAccountYearEmployeeId.Value, employeeId);
            //    if(timeWorkAccountYear != null)
            //    {
            //        if (default > timeWorkAccountYear.PaidAbsenceStopDate)
            //            factor = null;
            //    }                
            //}

            return factor != null ? factor.Factor : 0;
        }


        public TimeWorkAccountYearDTO GetWorkAccountYear(CompEntities entities, int timeWorkAccountYearEmployeeId, int employeeId)
        {
            string key = $"GetWorkAccountYearByTimeWorkAccountYearEmployeeId#{timeWorkAccountYearEmployeeId}#{employeeId}";

            var dto = BusinessMemoryCache<TimeWorkAccountYearDTO>.Get(key);
            if (dto == null)
            {
                TimeWorkAccountYearEmployee timeWorkAccountYearEmployee = TimeWorkAccountManager.GetTimeWorkAccountYearEmployee(entities, timeWorkAccountYearEmployeeId, employeeId, includeTimeWorkAccountYear: true);
                if (timeWorkAccountYearEmployee != null && timeWorkAccountYearEmployee.TimeWorkAccountYear != null)
                {
                    dto = timeWorkAccountYearEmployee.TimeWorkAccountYear.ToDTO();
                    if (dto != null)
                        BusinessMemoryCache<TimeWorkAccountYearDTO>.Set(key, dto, 30);
                }
            }

            return dto;
        }

        public decimal GetEmployeeFactor(List<EmployeeFactor> employeeFactors, int employeeId, TermGroup_EmployeeFactorType type, DateTime? date = null)
        {
            decimal factor = 0;

            if (!date.HasValue)
                date = DateTime.Today;

            if (!employeeFactors.IsNullOrEmpty())
            {
                var employeeFactor = (from f in employeeFactors
                                      where f.EmployeeId == employeeId &&
                                      f.Type == (int)type &&
                                      (f.FromDate <= date.Value || !f.FromDate.HasValue) &&
                                      f.State == (int)SoeEntityState.Active
                                      orderby f.FromDate descending
                                      select f).FirstOrDefault();

                factor = employeeFactor?.Factor ?? 0;
            }

            return factor;

        }

        public void SetEmployeeFactorNames(IEnumerable<EmployeeFactor> employeeFactors)
        {
            if (employeeFactors.IsNullOrEmpty())
                return;

            List<GenericType> factors = base.GetTermGroupContent(TermGroup.EmployeeFactorType);
            foreach (EmployeeFactor employeeFactor in employeeFactors)
            {
                GenericType factor = factors.FirstOrDefault(f => f.Id == employeeFactor.Type);
                employeeFactor.TypeName = factor != null ? factor.Name : String.Empty;
            }
        }

        #endregion

        #region EmployeeEmployer

        public List<EmployeeEmployer> GetEmployeeEmployers(int employeeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeEmployer.NoTracking();
            return GetEmployeeEmployers(entities, employeeId, actorCompanyId);
        }

        public List<EmployeeEmployer> GetEmployeeEmployers(CompEntities entities, int employeeId, int actorCompanyId)
        {
            return (from ee in entities.EmployeeEmployer
                    where ee.EmployeeId == employeeId &&
                    ee.State == (int)SoeEntityState.Active
                    select ee).ToList();
        }

        public List<EmployeeEmployer> GetEmployeeEmployersForCompany(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeEmployer.NoTracking();
            return GetEmployeeEmployersForCompany(entities, actorCompanyId);
        }

        public List<EmployeeEmployer> GetEmployeeEmployersForCompany(CompEntities entities, int actorCompanyId)
        {

            return (from ee in entities.EmployeeEmployer
                    where ee.ActorCompanyId == actorCompanyId &&
                    ee.State == (int)SoeEntityState.Active
                    select ee).ToList();
        }

        #endregion

        #region Employer

        public List<Employer> GetEmployers(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employer.NoTracking();
            return GetEmployers(entities, actorCompanyId);
        }

        public List<Employer> GetEmployers(CompEntities entities, int actorCompanyId)
        {
            return (from e in entities.Employer
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active
                    orderby e.Name
                    select e).ToList();
        }

        #endregion

        #region Template groups

        public List<TimeScheduleTemplateGroupEmployee> GetEmployeeTemplateGroups(CompEntities entities, int actorCompanyId, int employeeId)
        {
            return (from t in entities.TimeScheduleTemplateGroupEmployee
                    where t.TimeScheduleTemplateGroup.ActorCompanyId == actorCompanyId &&
                    t.EmployeeId == employeeId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public ActionResult SaveEmployeeTemplateGroups(CompEntities entities, TransactionScope transaction, Employee employee, List<TimeScheduleTemplateGroupEmployeeDTO> templateGroupsInput, int actorCompanyId)
        {
            List<TimeScheduleTemplateGroupEmployee> existingGroups = GetEmployeeTemplateGroups(entities, actorCompanyId, employee.EmployeeId);

            #region Delete groups that exists in db but not in input.

            foreach (TimeScheduleTemplateGroupEmployee existingGroup in existingGroups)
            {
                if (!templateGroupsInput.Any(g => g.TimeScheduleTemplateGroupId == existingGroup.TimeScheduleTemplateGroupId))
                    ChangeEntityState(existingGroup, SoeEntityState.Deleted);
            }

            #endregion

            #region Add/update

            foreach (TimeScheduleTemplateGroupEmployeeDTO templateGroupInput in templateGroupsInput.ToList())
            {
                TimeScheduleTemplateGroupEmployee templateGroup = existingGroups.FirstOrDefault(t => t.TimeScheduleTemplateGroupEmployeeId == templateGroupInput.TimeScheduleTemplateGroupEmployeeId);
                if (templateGroup == null)
                {
                    templateGroup = new TimeScheduleTemplateGroupEmployee()
                    {
                        TimeScheduleTemplateGroupId = templateGroupInput.TimeScheduleTemplateGroupId,
                        EmployeeId = employee.EmployeeId,
                        State = (int)SoeEntityState.Active,
                    };

                    SetCreatedProperties(templateGroup);
                    entities.AddObject("TimeScheduleTemplateGroupEmployee", templateGroup);
                }
                else
                {
                    SetModifiedProperties(templateGroup);
                }

                templateGroup.FromDate = templateGroupInput.FromDate;
                templateGroup.ToDate = templateGroupInput.ToDate;
            }

            #endregion

            return SaveChanges(entities, transaction);
        }

        #endregion

        #region EmployeeMeeting

        public List<EmployeeMeeting> GetEmployeeMeetings(int employeeId, int actorCompanyId, int? userId, bool checkPermissions)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeMeeting.NoTracking();
            return GetEmployeeMeetings(entities, employeeId, actorCompanyId, userId, checkPermissions);
        }

        public List<EmployeeMeeting> GetEmployeeMeetings(CompEntities entities, int employeeId, int actorCompanyId, int? userId = null, bool checkPermissions = false)
        {
            List<EmployeeMeeting> validMeetings = new List<EmployeeMeeting>();

            var allMeetings = (from es in entities.EmployeeMeeting
                            .Include("FollowUpType")
                            .Include("AttestRole")
                            .Include("Participant.ContactPerson")
                               where es.EmployeeId == employeeId &&
                                     es.State == (int)SoeEntityState.Active
                               select es).ToList();


            if (userId.HasValue && checkPermissions)
            {
                var userAttestRoles = AttestManager.GetAttestRolesForUser(entities, userId.Value, actorCompanyId);
                foreach (var meeting in allMeetings)
                {
                    if (!meeting.AttestRole.Any())
                    {
                        validMeetings.Add(meeting);
                        continue;
                    }

                    foreach (var userAttestRole in userAttestRoles)
                    {
                        if (meeting.AttestRole.Select(x => x.AttestRoleId).Contains(userAttestRole.AttestRoleId))
                        {
                            validMeetings.Add(meeting);
                            break;
                        }
                    }
                }
            }
            else
            {
                validMeetings.AddRange(allMeetings);
            }

            return validMeetings;
        }

        public ActionResult SaveEmployeeMeetings(CompEntities entities, TransactionScope transaction, Employee employee, List<EmployeeMeetingDTO> employeeMeetingsInput, int actorCompanyId)
        {
            List<EmployeeMeeting> existingMeetings = GetEmployeeMeetings(entities, employee.EmployeeId, actorCompanyId, base.UserId, true);

            foreach (var employeeMeetingInput in employeeMeetingsInput)
            {
                if (employeeMeetingInput.FollowUpTypeId == 0 || !employeeMeetingInput.StartTime.HasValue)
                    continue;

                EmployeeMeeting meeting = existingMeetings.FirstOrDefault(x => x.EmployeeMeetingId == employeeMeetingInput.EmployeeMeetingId);
                if (meeting == null)
                {
                    //add
                    meeting = new EmployeeMeeting()
                    {
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = employee.EmployeeId,
                        State = (int)SoeEntityState.Active,
                    };

                    SetCreatedProperties(meeting);
                    entities.EmployeeMeeting.AddObject(meeting);
                }
                else
                {
                    //update
                    meeting.State = (int)employeeMeetingInput.State;
                    SetModifiedProperties(meeting);
                }

                meeting.FollowUpTypeId = employeeMeetingInput.FollowUpTypeId;
                meeting.StartTime = employeeMeetingInput.StartTime.Value;
                meeting.Reminder = employeeMeetingInput.Reminder;
                meeting.EmployeeCanEdit = employeeMeetingInput.EmployeeCanEdit;
                meeting.Note = employeeMeetingInput.Note;
                meeting.OtherParticipants = employeeMeetingInput.OtherParticipants;
                meeting.Completed = employeeMeetingInput.Completed;

                #region Participants

                #region Remove if existing participant not exists in input

                foreach (var participant in meeting.Participant.ToList())
                {
                    if (!(employeeMeetingInput.ParticipantIds.Contains(participant.EmployeeId)))
                        meeting.Participant.Remove(participant);
                }

                #endregion

                #region Add

                foreach (var participantId in employeeMeetingInput.ParticipantIds)
                {
                    var existingParticipant = meeting.Participant.FirstOrDefault(x => x.EmployeeId == participantId);
                    if (existingParticipant == null)
                    {
                        var newParticipant = GetEmployee(entities, participantId, actorCompanyId, onlyActive: false);
                        meeting.Participant.Add(newParticipant);
                    }
                }

                #endregion

                #endregion

                #region Permissions

                #region Remove if existing permissions not exists in input

                foreach (var attestRole in meeting.AttestRole.ToList())
                {
                    if (!(employeeMeetingInput.AttestRoleIds.Contains(attestRole.AttestRoleId)))
                        meeting.AttestRole.Remove(attestRole);
                }

                #endregion

                #region Add

                foreach (var attestRoleId in employeeMeetingInput.AttestRoleIds)
                {
                    var existingAttestRole = meeting.AttestRole.FirstOrDefault(x => x.AttestRoleId == attestRoleId);
                    if (existingAttestRole == null)
                    {
                        var newAttestRole = AttestManager.GetAttestRole(entities, attestRoleId, actorCompanyId);
                        meeting.AttestRole.Add(newAttestRole);
                    }
                }

                #endregion

                #endregion

            }

            return SaveChanges(entities, transaction);
        }

        #endregion

        #region EmployeeCalculatedCosts

        public List<EmployeeCalculatedCostDTO> GetEmployeeCalculatedCostDTOs(int employeeId, int actorCompanyId, bool incProject)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeCalculatedCostDTOs(entities, employeeId, actorCompanyId, incProject);
        }

        public List<EmployeeCalculatedCostDTO> GetEmployeeCalculatedCostDTOs(CompEntities entities, int employeeId, int actorCompanyId, bool incProject)
        {

            string key = $"GetEmployeeCalculatedCosts#employeeId{employeeId}#incproject{incProject}";
            List<EmployeeCalculatedCostDTO> employeeCalculatedCosts = BusinessMemoryCache<List<EmployeeCalculatedCostDTO>>.Get(key);

            if (employeeCalculatedCosts == null)
            {
                employeeCalculatedCosts = (from ec in entities.EmployeeCalculatedCost
                                           where ec.EmployeeId == employeeId &&
                                                 ec.State == (int)SoeEntityState.Active
                                           orderby ec.FromDate ascending
                                           select new EmployeeCalculatedCostDTO
                                           {
                                               EmployeeCalculatedCostId = ec.EmployeeCalculatedCostId,
                                               CalculatedCostPerHour = ec.CalculatedCostPerHour,
                                               EmployeeId = ec.EmployeeId,
                                               fromDate = ec.FromDate,
                                               ProjectId = ec.ProjectId
                                           }).ToList();

                if (!incProject)
                {
                    employeeCalculatedCosts = employeeCalculatedCosts.Where(x => x.ProjectId == null).ToList();
                }

                BusinessMemoryCache<List<EmployeeCalculatedCostDTO>>.Set(key, employeeCalculatedCosts, 100);
            }
            return employeeCalculatedCosts;
        }


        public List<EmployeeCalculatedCost> GetEmployeeCalculatedCosts(CompEntities entities, int employeeId, int actorCompanyId, int? projectId = null)
        {
            if (projectId.GetValueOrDefault() > 0)
                return (from ec in entities.EmployeeCalculatedCost
                        where ec.EmployeeId == employeeId &&
                              ec.State == (int)SoeEntityState.Active &&
                              ec.ProjectId == projectId.Value
                        orderby ec.FromDate ascending
                        select ec).ToList();
            else
                return (from ec in entities.EmployeeCalculatedCost
                        where ec.EmployeeId == employeeId &&
                              ec.State == (int)SoeEntityState.Active
                        orderby ec.FromDate ascending
                        select ec).ToList();
        }

        public ActionResult SaveEmployeeCalculatedCosts(CompEntities entities, TransactionScope transaction, Employee employee, List<EmployeeCalculatedCostDTO> calculatedCostsInput)
        {
            var existingCosts = GetEmployeeCalculatedCosts(entities, employee.EmployeeId, employee.ActorCompanyId);

            foreach (var cost in calculatedCostsInput)
            {
                EmployeeCalculatedCost employeeCalculatedCost;
                if (cost.EmployeeCalculatedCostId == 0)
                {
                    employeeCalculatedCost = new EmployeeCalculatedCost
                    {
                        EmployeeId = employee.EmployeeId,
                        CalculatedCostPerHour = cost.CalculatedCostPerHour,
                        FromDate = cost.fromDate ?? DateTime.MinValue,
                        ProjectId = cost.ProjectId
                    };
                    SetCreatedProperties(employeeCalculatedCost);
                    entities.EmployeeCalculatedCost.AddObject(employeeCalculatedCost);
                }
                else if (cost.IsDeleted)
                {
                    employeeCalculatedCost = existingCosts.FirstOrDefault(c => c.EmployeeCalculatedCostId == cost.EmployeeCalculatedCostId);
                    if (employeeCalculatedCost != null)
                    {
                        employeeCalculatedCost.State = (int)SoeEntityState.Deleted;
                        SetModifiedProperties(employeeCalculatedCost);
                    }
                }
                else if (cost.IsModified)
                {
                    employeeCalculatedCost = existingCosts.FirstOrDefault(c => c.EmployeeCalculatedCostId == cost.EmployeeCalculatedCostId);
                    if (employeeCalculatedCost != null)
                    {
                        employeeCalculatedCost.CalculatedCostPerHour = cost.CalculatedCostPerHour;
                        employeeCalculatedCost.FromDate = cost.fromDate.Value;
                    }
                    SetModifiedProperties(employeeCalculatedCost);
                }
            }

            var result = SaveChanges(entities, transaction);
            if (result.Success)
            {
                string key = $"GetEmployeeCalculatedCosts#employeeId{employee.EmployeeId}#incproject{false}";
                BusinessMemoryCache<List<EmployeeCalculatedCostDTO>>.Delete(key);
                key = $"GetEmployeeCalculatedCosts#employeeId{employee.EmployeeId}#incproject{true}";
                BusinessMemoryCache<List<EmployeeCalculatedCostDTO>>.Delete(key);
            }

            return result;
        }

        public decimal GetEmployeeCalculatedCost(Employee employee, DateTime date, int? projectId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeCalculatedCost(entities, employee, date, projectId);
        }

        public decimal GetEmployeeCalculatedCost(CompEntities entities, Employee employee, DateTime date, int? projectId)
        {
            if (employee == null)
                return 0;

            var calculatedCosts = GetEmployeeCalculatedCostDTOs(entities, employee.EmployeeId, employee.ActorCompanyId, true);
            var currentCost = calculatedCosts.Where(x => x.fromDate <= date && x.ProjectId == null).OrderByDescending(x => x.fromDate).FirstOrDefault();
            if (projectId.GetValueOrDefault() > 0)
            {
                var currentProjectCost = calculatedCosts.Where(x => x.ProjectId == projectId && x.fromDate <= date).OrderByDescending(x => x.fromDate).FirstOrDefault();
                if (currentProjectCost != null)
                {
                    currentCost = currentProjectCost;
                }
            }

            return currentCost?.CalculatedCostPerHour ?? 0;
        }

        #endregion

        #region EmployeeSalaryType

        public TermGroup_PayrollExportSalaryType GetEmployeeSalaryType(Employee employee, DateTime startDate, DateTime stopDate)
        {
            TermGroup_PayrollExportSalaryType salaryType = TermGroup_PayrollExportSalaryType.Unknown;

            if (employee == null)
                return salaryType;

            if (employee.PayrollStatisticsSalaryType.HasValue && employee.PayrollStatisticsSalaryType.Value > 0)
            {
                salaryType = (TermGroup_PayrollExportSalaryType)employee.PayrollStatisticsSalaryType.Value;
            }
            else
            {
                int? payrollGroupId = employee.GetPayrollGroupId(startDate, stopDate);
                if (payrollGroupId.HasValue)
                {
                    PayrollGroup payrollGroup = PayrollManager.GetPayrollGroup(payrollGroupId.Value, includeSettings: true);
                    if (payrollGroup != null)
                    {
                        PayrollGroupSetting payrollGroupSetting = payrollGroup.PayrollGroupSetting.FirstOrDefault(i => i.Type == (int)PayrollGroupSettingType.PayrollReportsSalaryType);
                        if (payrollGroupSetting != null && payrollGroupSetting.IntData.HasValue)
                            salaryType = (TermGroup_PayrollExportSalaryType)payrollGroupSetting.IntData;

                    }
                }
            }

            return salaryType;
        }

        #endregion

        #region EmployeeSetting

        public List<EmployeeSetting> GetEmployeeSettings(int actorCompanyId, int employeeId, DateTime dateFrom, DateTime dateTo, TermGroup_EmployeeSettingType? area = null, TermGroup_EmployeeSettingType? group = null, TermGroup_EmployeeSettingType? type = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            entitiesReadOnly.EmployeeSetting.MergeOption = MergeOption.NoTracking;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeSettings(entities, actorCompanyId, employeeId, dateFrom, dateTo, area: area, group: group, type: type);
        }

        public List<EmployeeSetting> GetEmployeeSettings(CompEntities entities, int actorCompanyId, int employeeId, DateTime dateFrom, DateTime dateTo, TermGroup_EmployeeSettingType? area = null, TermGroup_EmployeeSettingType? group = null, TermGroup_EmployeeSettingType? type = null)
        {
            return GetEmployeeSettings(entities, actorCompanyId, employeeId.ObjToList(), dateFrom, dateTo, area: area, group: group, type: type);
        }

        public List<EmployeeSetting> GetEmployeeSettings(int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo, TermGroup_EmployeeSettingType? area = null, TermGroup_EmployeeSettingType? group = null, TermGroup_EmployeeSettingType? type = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            entitiesReadOnly.EmployeeSetting.MergeOption = MergeOption.NoTracking;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeSettings(entities, actorCompanyId, employeeIds, dateFrom, dateTo, area: area, group: group, type: type);
        }

        public List<EmployeeSetting> GetEmployeeSettings(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo, TermGroup_EmployeeSettingType? area = null, TermGroup_EmployeeSettingType? group = null, TermGroup_EmployeeSettingType? type = null)
        {
            var employeeSettings = entities.EmployeeSetting
                .Where(es =>
                    es.ActorCompanyId == actorCompanyId &&
                    employeeIds.Contains(es.EmployeeId) &&
                    es.State == (int)SoeEntityState.Active)
                .ToList();

            return employeeSettings
                .FilterByDates(dateFrom, dateTo)
                .FilterByTypes(area, group, type);
        }

        public List<EmployeeSettingTypeDTO> GetAvailableEmployeeSettingsByArea(int actorCompanyId, TermGroup_EmployeeSettingType area)
        {
            List<EmployeeSettingTypeDTO> settings = new List<EmployeeSettingTypeDTO>();

            switch (area)
            {
                case TermGroup_EmployeeSettingType.WorkTimeRule:
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.None, (int)TermGroup_EmployeeSettingType.None, SettingDataType.Undefined));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest, (int)TermGroup_EmployeeSettingType.None, SettingDataType.Undefined));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest, (int)TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest_Weekday, SettingDataType.Integer)
                    {
                        Options = CalendarUtility.GetDayOfWeekNames().ToSmallGenericTypes()
                    });
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest, (int)TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest_TimeOfDay, SettingDataType.Time));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.WorkTimeRule_DailyRest, (int)TermGroup_EmployeeSettingType.None, SettingDataType.Undefined));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.WorkTimeRule_DailyRest, (int)TermGroup_EmployeeSettingType.WorkTimeRule_DailyRest_TimeOfDay, SettingDataType.Time));
                    break;
                case TermGroup_EmployeeSettingType.Reporting:
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.None, (int)TermGroup_EmployeeSettingType.None, SettingDataType.Undefined));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_Fora, (int)TermGroup_EmployeeSettingType.None, SettingDataType.Undefined));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_Fora, (int)TermGroup_EmployeeSettingType.Reporting_Fora_FokId, SettingDataType.String));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_Fora, (int)TermGroup_EmployeeSettingType.Reporting_Fora_Option1, SettingDataType.String));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_Fora, (int)TermGroup_EmployeeSettingType.Reporting_Fora_Option2, SettingDataType.String));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_SEF, (int)TermGroup_EmployeeSettingType.None, SettingDataType.Undefined));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_SEF, (int)TermGroup_EmployeeSettingType.Reporting_SEF_AssociationNumber, SettingDataType.String)
                    {
                        MaxLength = 2
                    });
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_SEF, (int)TermGroup_EmployeeSettingType.Reporting_SEF_WorkPlace, SettingDataType.String)
                    {
                        MaxLength = 4
                    });
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_SEF, (int)TermGroup_EmployeeSettingType.Reporting_SEF_PaymentCode, SettingDataType.Integer)
                    {
                        Options = TermCacheManager.Instance.GetTermGroupContent(TermGroup.SEFPaymentCode).ToSmallGenericTypes()
                    });
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_SN, (int)TermGroup_EmployeeSettingType.None, SettingDataType.Undefined));
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Reporting_SN, (int)TermGroup_EmployeeSettingType.Reporting_SN_Jobstatus, SettingDataType.Integer)
                    {
                        Options = TermCacheManager.Instance.GetTermGroupContent(TermGroup.PayrollReportsJobStatus).ToSmallGenericTypes()
                    });
                    break;
                case TermGroup_EmployeeSettingType.Additions:
                    settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.None, (int)TermGroup_EmployeeSettingType.None, SettingDataType.Undefined));
                    var types = TimeScheduleManager.GetTimeScheduleTypesDict(actorCompanyId).ToSmallGenericTypes();

                    if (types.Any())
                    {
                        foreach (var type in types)
                        {
                            settings.Add(new EmployeeSettingTypeDTO(area, TermGroup_EmployeeSettingType.Additions_ScheduleType, type.Id, SettingDataType.Undefined, type.Name));
                        }
                    }

                    break;
            }

            SetEmployeeSettingNames(settings);

            return settings;
        }

        private void SetEmployeeSettingNames(List<EmployeeSettingTypeDTO> settings)
        {
            List<GenericType> types = TermCacheManager.Instance.GetTermGroupContent(TermGroup.EmployeeSettingType);

            foreach (EmployeeSettingTypeDTO setting in settings)
            {
                if (setting.EmployeeSettingType == (int)TermGroup_EmployeeSettingType.None)
                {
                    if (setting.EmployeeSettingGroupType == TermGroup_EmployeeSettingType.None)
                    {
                        // Area
                        setting.Name = types.FirstOrDefault(t => t.Id == (int)setting.EmployeeSettingAreaType)?.Name;
                    }
                    else
                    {
                        // Group
                        setting.Name = types.FirstOrDefault(t => t.Id == (int)setting.EmployeeSettingGroupType)?.Name;
                    }
                }
                else
                {
                    // Type
                    if (setting.Name.IsNullOrEmpty())
                        setting.Name = types.FirstOrDefault(t => t.Id == setting.EmployeeSettingType)?.Name;
                }
            }
        }

        #endregion

        #region EmployeeStatistics

        #region EmployeeData

        public List<EmployeeStatisticsChartData> GetEmployeeStatisticsEmployeeData(int employeeId, DateTime startDate, DateTime stopDate, TermGroup_EmployeeStatisticsType type)
        {
            List<EmployeeStatisticsChartData> data = new List<EmployeeStatisticsChartData>();

            switch (type)
            {
                case TermGroup_EmployeeStatisticsType.Arrival:
                case TermGroup_EmployeeStatisticsType.GoHome:
                    data = GetEmployeeStatisticsArrivalAndGoHomeData(employeeId, startDate, stopDate, type);
                    break;
            }

            return data;
        }

        private List<EmployeeStatisticsChartData> GetEmployeeStatisticsArrivalAndGoHomeData(int employeeId, DateTime startDate, DateTime stopDate, TermGroup_EmployeeStatisticsType type)
        {
            List<EmployeeStatisticsChartData> data = new List<EmployeeStatisticsChartData>();

            Dictionary<DateTime, DateTime> schedules;
            Dictionary<DateTime, DateTime> stamps;

            using (CompEntities entities = new CompEntities())
            {
                if (type == TermGroup_EmployeeStatisticsType.Arrival)
                {
                    // Get schedule in for specified emp and default range
                    schedules = TimeScheduleManager.GetScheduleIn(entities, employeeId, startDate, stopDate);
                    // Get first stamp in for specified emp and default range
                    stamps = TimeStampManager.GetFirstStampIn(entities, employeeId, startDate, stopDate);
                }
                else
                {
                    // Get schedule out for specified emp and default range
                    schedules = TimeScheduleManager.GetScheduleOut(entities, employeeId, startDate, stopDate);
                    // Get last stamp out for specified emp and default range
                    stamps = TimeStampManager.GetLastStampOut(entities, employeeId, startDate, stopDate);
                }
            }

            DateTime currentDate = startDate;
            while (currentDate <= stopDate)
            {
                decimal value = 0;
                KeyValuePair<DateTime, DateTime>? schedule = schedules.FirstOrDefault(s => s.Key == currentDate);
                KeyValuePair<DateTime, DateTime>? stamp = stamps.FirstOrDefault(s => s.Key == currentDate);
                if (schedule.HasValue && stamp.HasValue && stamp.Value.Value != DateTime.MinValue)
                {
                    // Schedule time is in 1900-01-01 format
                    value = (decimal)(CalendarUtility.MergeDateAndTime(currentDate, schedule.Value.Value) - stamp.Value.Value).TotalMinutes;
                    if (type == TermGroup_EmployeeStatisticsType.GoHome)
                        value = -value;
                }

                string color;
                if (type == TermGroup_EmployeeStatisticsType.Arrival)
                    color = (value > 0 ? "GradientOrange" : "InvertedGradientRed");
                else
                    color = (value > 0 ? "GradientDarkOrange" : "InvertedGradientDarkRed");

                data.Add(new EmployeeStatisticsChartData()
                {
                    Date = currentDate,
                    Value = value,
                    Color = color
                });
                currentDate = currentDate.AddDays(1);
            }

            return data;
        }

        #endregion

        #endregion

        #region EmployeeTaxSE      

        public EmployeeTaxSE GetEmployeeTaxSE(int employeeTaxId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeTaxSE.NoTracking();
            return GetEmployeeTaxSE(entities, employeeTaxId);
        }

        public EmployeeTaxSE GetEmployeeTaxSE(CompEntities entities, int employeeTaxId)
        {
            int actorCompanyId = base.ActorCompanyId;
            return (from e in entities.EmployeeTaxSE
                    where e.EmployeeTaxId == employeeTaxId &&
                    e.Employee.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        public EmployeeTaxSE GetEmployeeTaxSE(int employeeId, int year)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeTaxSE.NoTracking();
            return GetEmployeeTaxSE(entities, employeeId, year);
        }

        public EmployeeTaxSE GetEmployeeTaxSE(CompEntities entities, int employeeId, int year)
        {
            int actorCompanyId = base.ActorCompanyId;
            return (from e in entities.EmployeeTaxSE
                    where e.EmployeeId == employeeId &&
                    e.Year == year && e.Employee.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        public EmployeeTaxSEDTO GetEmployeeTaxSEDTO(CompEntities entities, int employeeId, int year)
        {
            string key = $"GetEmployeeTaxSEDTO#employeeId{employeeId}#year{year}";
            EmployeeTaxSEDTO dto = BusinessMemoryCache<EmployeeTaxSEDTO>.Get(key);
            if (dto == null)
            {
                dto = GetEmployeeTaxSE(entities, employeeId, year)?.ToDTO();
                BusinessMemoryCache<EmployeeTaxSEDTO>.Set(key, dto, 30);
            }

            return dto;
        }

        public List<int> GetEmployeeTaxSEYears(int employeeId)
        {
            int actorCompanyId = base.ActorCompanyId;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.EmployeeTaxSE.NoTracking();
            return (from e in entitiesReadOnly.EmployeeTaxSE
                    where e.EmployeeId == employeeId &&
                    e.Employee.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active
                    orderby e.Year
                    select e.Year).ToList();
        }

        public EmployeeTaxSE CreateEmployeeTaxSE(CompEntities entities, Employee employee, TermGroup_EmployeeTaxType type, int year, int? taxRate, int? taxRateColumn, decimal? oneTimeTaxPercent, bool mainEmployer = true, TermGroup_EmployeeTaxEmploymentTaxType employmentTaxType = TermGroup_EmployeeTaxEmploymentTaxType.EmploymentTax)
        {
            if (!employee.IsAdded() && !employee.EmployeeTaxSE.IsLoaded)
                employee.EmployeeTaxSE.Load();

            EmployeeTaxSE employeeTaxSE = employee.EmployeeTaxSE.FirstOrDefault(i => i.Type == (int)type && i.Year == year);
            if (employeeTaxSE == null)
            {
                employeeTaxSE = new EmployeeTaxSE()
                {
                    Type = (int)TermGroup_EmployeeTaxType.TableTax,
                    MainEmployer = mainEmployer,
                    Year = year,
                    TaxRate = taxRate,
                    TaxRateColumn = taxRateColumn,
                    OneTimeTaxPercent = oneTimeTaxPercent,
                    EmploymentTaxType = (int)employmentTaxType
                };
                entities.EmployeeTaxSE.AddObject(employeeTaxSE);
                SetCreatedProperties(employeeTaxSE);

                employee.EmployeeTaxSE.Add(employeeTaxSE);
            }
            else
            {
                employeeTaxSE.TaxRate = taxRate;
                SetModifiedProperties(employeeTaxSE);
            }

            return employeeTaxSE;
        }

        public ActionResult GetEmployeeTaxSEPrintURL(int actorCompanyId, int userId, List<int> employeeIds, int year)
        {
            // Default result is false
            ActionResult result = new ActionResult(false);

            Dictionary<string, string> employeeCsrData = new Dictionary<string, string>();
            foreach (int employeeId in employeeIds)
            {
                Employee emp = GetEmployee(employeeId, actorCompanyId, onlyActive: true, loadContactPerson: true);
                EmployeeTaxSEDTO previousEmployeeTaxSE = GetEmployeeTaxSE(employeeId, year - 1).ToDTO();
                EmployeeTaxSEDTO taxSeDTO = GetEmployeeTaxSE(employeeId, year).ToDTO();
                if (taxSeDTO == null)
                {
                    //Create new
                    taxSeDTO = new EmployeeTaxSEDTO()
                    {
                        EmployeeId = employeeId,
                        Year = year,
                    };
                }

                taxSeDTO.CsrExportDate = DateTime.Now;
                taxSeDTO.EmploymentTaxType = previousEmployeeTaxSE?.EmploymentTaxType ?? TermGroup_EmployeeTaxEmploymentTaxType.EmploymentTax;
                taxSeDTO.EmploymentAbroadCode = previousEmployeeTaxSE?.EmploymentAbroadCode ?? TermGroup_EmployeeTaxEmploymentAbroadCode.None;
                taxSeDTO.RegionalSupport = previousEmployeeTaxSE?.RegionalSupport ?? false;
                taxSeDTO.FirstEmployee = previousEmployeeTaxSE?.FirstEmployee ?? false;
                taxSeDTO.SecondEmployee = previousEmployeeTaxSE?.SecondEmployee ?? false;
                taxSeDTO.SalaryDistressAmount = previousEmployeeTaxSE?.SalaryDistressAmount;
                taxSeDTO.SalaryDistressAmountType = previousEmployeeTaxSE?.SalaryDistressAmountType ?? TermGroup_EmployeeTaxSalaryDistressAmountType.NotSelected;
                taxSeDTO.SalaryDistressReservedAmount = previousEmployeeTaxSE?.SalaryDistressReservedAmount;

                #region MainEmployer

                if (previousEmployeeTaxSE != null && !previousEmployeeTaxSE.MainEmployer)
                {
                    taxSeDTO.MainEmployer = false;
                    taxSeDTO.Type = TermGroup_EmployeeTaxType.SideIncomeTax;
                }
                else
                {
                    taxSeDTO.MainEmployer = true;
                }

                #endregion

                result = SaveEmployeeTaxSE(taxSeDTO, actorCompanyId, TermGroup_TrackChangesActionMethod.Employee_CsrInquiry);
                if (result.Success && result.IntegerValue != 0)
                {
                    employeeCsrData.Add(string.Format("{0}-{1}", emp.EmployeeNr, result.IntegerValue.ToString()), emp.ContactPerson.SocialSec.Trim().Replace("-", String.Empty));
                }
            }

            //Create reportxml and return datastorage Id
            byte[] data = ImportExportManager.CreateCSRExportFile(employeeCsrData, actorCompanyId, userId, year.ToString());
            if (data != null)
            {
                using (CompEntities entities = new CompEntities())
                {
                    string description = "CSR_EXPORT_FIL" + Constants.SOE_SERVER_FILE_XML_SUFFIX;
                    DataStorage dataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.TimeSalaryExportCSR, null, data, null, null, actorCompanyId, description);
                    result = SaveChanges(entities);
                    if (result.Success)
                        result.IntegerValue = dataStorage.DataStorageId;
                }
            }

            return result;

        }

        public ActionResult SaveEmployeeTaxSE(int taxInputId, int employeeId, int actorCompanyId, TermGroup_TrackChangesActionMethod actionMethod)
        {
            EmployeeTaxSEDTO taxinputRecord;
            if (taxInputId != 0)
            {
                //Update existing
                taxinputRecord = GetEmployeeTaxSE(taxInputId).ToDTO();
                //Update Year
                taxinputRecord.CsrExportDate = DateTime.Now;
            }
            else
            {
                //create new
                taxinputRecord = new EmployeeTaxSEDTO()
                {
                    EmployeeId = employeeId,
                    CsrExportDate = DateTime.Now,
                    Year = DateTime.Now.Year,
                };
            }
            return SaveEmployeeTaxSE(taxinputRecord, actorCompanyId, actionMethod);
        }

        public ActionResult SaveEmployeeTaxSE(EmployeeTaxSEDTO taxInput, int actorCompanyId, TermGroup_TrackChangesActionMethod actionMethod)
        {
            if (taxInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeTaxSE");

            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveEmployeeTaxSE(entities, transaction, taxInput, actorCompanyId, actionMethod);
                        if (result.Success)
                        {
                            // Commit transaction
                            transaction.Complete();
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
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SaveEmployeeTaxSE(CompEntities entities, TransactionScope transaction, EmployeeTaxSEDTO taxInput, int actorCompanyId, TermGroup_TrackChangesActionMethod actionMethod, EmployeeUserChangesRepositoryDTO changesRepository = null)
        {
            if (taxInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeTaxSE");

            ActionResult validateResult = ValidateFirstEmployee(entities, taxInput, actorCompanyId);
            if (!validateResult.Success)
                return validateResult;

            validateResult = ValidateSecondEmployee(entities, taxInput, actorCompanyId);
            if (!validateResult.Success)
                return validateResult;

            bool saveTrackChanges = false;
            if (changesRepository == null)
            {
                saveTrackChanges = true;
                changesRepository = TrackChangesManager.CreateEmployeeUserChangesRepository(actorCompanyId, Guid.NewGuid(), actionMethod, SoeEntityType.Employee);
                if (taxInput.EmployeeTaxId != 0)
                    changesRepository.SetBeforeValue(new List<EmployeeTaxSE>() { GetEmployeeTaxSE(entities, taxInput.EmployeeTaxId) });
            }

            int employeeTaxId = taxInput.EmployeeTaxId;
            List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();

            #region EmployeeTaxSE

            // Get existing record
            EmployeeTaxSE tax = GetEmployeeTaxSE(entities, employeeTaxId);
            if (tax == null)
            {
                TrackChangesDTO tcDTO = TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Insert, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE);
                tcDTO.ParentEntity = SoeEntityType.Employee;
                tcDTO.ParentRecordId = taxInput.EmployeeId;
                trackChangesItems.Add(tcDTO);

                #region EmployeeTaxSE Add

                tax = new EmployeeTaxSE()
                {
                    EmployeeId = taxInput.EmployeeId,
                    Year = taxInput.Year,
                    MainEmployer = taxInput.MainEmployer,
                    Type = (int)taxInput.Type,
                    TaxRate = taxInput.TaxRate,
                    TaxRateColumn = taxInput.TaxRateColumn,
                    OneTimeTaxPercent = taxInput.OneTimeTaxPercent,
                    EstimatedAnnualSalary = taxInput.EstimatedAnnualSalary,
                    AdjustmentType = (int)taxInput.AdjustmentType,
                    AdjustmentValue = taxInput.AdjustmentValue,
                    AdjustmentPeriodFrom = taxInput.AdjustmentPeriodFrom,
                    AdjustmentPeriodTo = taxInput.AdjustmentPeriodTo,
                    SchoolYouthLimitInitial = taxInput.SchoolYouthLimitInitial,
                    SinkType = (int)taxInput.SinkType,
                    EmploymentTaxType = (int)taxInput.EmploymentTaxType,
                    EmploymentAbroadCode = (int)taxInput.EmploymentAbroadCode,
                    RegionalSupport = taxInput.RegionalSupport,
                    FirstEmployee = taxInput.FirstEmployee,
                    SecondEmployee = taxInput.SecondEmployee,
                    SalaryDistressAmount = taxInput.SalaryDistressAmount,
                    SalaryDistressAmountType = (int)taxInput.SalaryDistressAmountType,
                    SalaryDistressReservedAmount = taxInput.SalaryDistressReservedAmount,
                    CSRExportDate = taxInput.CsrExportDate,
                    CSRImportDate = taxInput.CsrImportDate,
                    TinNumber = taxInput.TinNumber,
                    CountryCode = taxInput.CountryCode,
                    ApplyEmploymentTaxMinimumRule = taxInput.ApplyEmploymentTaxMinimumRule,
                    State = (int)taxInput.State,
                    SalaryDistressCase = taxInput.SalaryDistressCase,
                    BirthPlace = taxInput.BirthPlace,
                    CountryCodeBirthPlace = taxInput.CountryCodeBirthPlace,
                    CountryCodeCitizen = taxInput.CountryCodeCitizen,
                };

                SetCreatedProperties(tax);
                entities.EmployeeTaxSE.AddObject(tax);

                #endregion
            }
            else
            {
                #region EmployeeTaxSE Update

                if (tax.MainEmployer != taxInput.MainEmployer)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Boolean, "MainEmployer", TermGroup_TrackChangesColumnType.EmployeeTaxSE_MainEmployer, tax.MainEmployer.ToString(), taxInput.MainEmployer.ToString()));
                    tax.MainEmployer = taxInput.MainEmployer;
                }
                if (tax.Type != (int)taxInput.Type)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Integer, "Type", TermGroup_TrackChangesColumnType.EmployeeTaxSE_Type, tax.Type.ToString(), ((int)taxInput.Type).ToString(), ((TermGroup_EmployeeTaxType)tax.Type).ToString(), taxInput.Type.ToString()));
                    tax.Type = (int)taxInput.Type;
                }
                if (tax.TaxRate != taxInput.TaxRate)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Integer, "TaxRate", TermGroup_TrackChangesColumnType.EmployeeTaxSE_TaxRate, tax.TaxRate.ToString(), taxInput.TaxRate.ToString()));
                    tax.TaxRate = taxInput.TaxRate;
                }
                if (tax.TaxRateColumn != taxInput.TaxRateColumn)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Integer, "TaxRateColumn", TermGroup_TrackChangesColumnType.EmployeeTaxSE_TaxRateColumn, tax.TaxRateColumn.ToString(), taxInput.TaxRateColumn.ToString()));
                    tax.TaxRateColumn = taxInput.TaxRateColumn;
                }
                if (tax.OneTimeTaxPercent != taxInput.OneTimeTaxPercent)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Decimal, "OneTimeTaxPercent", TermGroup_TrackChangesColumnType.EmployeeTaxSE_OneTimeTaxPercent, tax.OneTimeTaxPercent.ToString(), taxInput.OneTimeTaxPercent.ToString()));
                    tax.OneTimeTaxPercent = taxInput.OneTimeTaxPercent;
                }
                if (tax.EstimatedAnnualSalary != taxInput.EstimatedAnnualSalary)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Decimal, "EstimatedAnnualSalary", TermGroup_TrackChangesColumnType.EmployeeTaxSE_EstimatedAnnualSalary, tax.EstimatedAnnualSalary.ToString(), taxInput.EstimatedAnnualSalary.ToString()));
                    tax.EstimatedAnnualSalary = taxInput.EstimatedAnnualSalary;
                }
                if (tax.AdjustmentType != (int)taxInput.AdjustmentType)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Integer, "AdjustmentType", TermGroup_TrackChangesColumnType.EmployeeTaxSE_AdjustmentType, tax.AdjustmentType.ToString(), ((int)taxInput.AdjustmentType).ToString(), ((TermGroup_EmployeeTaxAdjustmentType)tax.AdjustmentType).ToString(), taxInput.AdjustmentType.ToString()));
                    tax.AdjustmentType = (int)taxInput.AdjustmentType;
                }
                if (tax.AdjustmentValue != taxInput.AdjustmentValue)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Decimal, "AdjustmentValue", TermGroup_TrackChangesColumnType.EmployeeTaxSE_AdjustmentValue, tax.AdjustmentValue.ToString(), taxInput.AdjustmentValue.ToString()));
                    tax.AdjustmentValue = taxInput.AdjustmentValue;
                }
                if (tax.AdjustmentPeriodFrom != taxInput.AdjustmentPeriodFrom)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Date, "AdjustmentPeriodFrom", TermGroup_TrackChangesColumnType.EmployeeTaxSE_AdjustmentPeriodFrom, tax.AdjustmentPeriodFrom.HasValue ? tax.AdjustmentPeriodFrom.Value.ToShortDateString() : null, taxInput.AdjustmentPeriodFrom.HasValue ? taxInput.AdjustmentPeriodFrom.Value.ToShortDateString() : null));
                    tax.AdjustmentPeriodFrom = taxInput.AdjustmentPeriodFrom;
                }
                if (tax.AdjustmentPeriodTo != taxInput.AdjustmentPeriodTo)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Date, "AdjustmentPeriodTo", TermGroup_TrackChangesColumnType.EmployeeTaxSE_AdjustmentPeriodTo, tax.AdjustmentPeriodTo.HasValue ? tax.AdjustmentPeriodTo.Value.ToShortDateString() : null, taxInput.AdjustmentPeriodTo.HasValue ? taxInput.AdjustmentPeriodTo.Value.ToShortDateString() : null));
                    tax.AdjustmentPeriodTo = taxInput.AdjustmentPeriodTo;
                }
                if (tax.SchoolYouthLimitInitial != taxInput.SchoolYouthLimitInitial)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Decimal, "SchoolYouthLimitInitial", TermGroup_TrackChangesColumnType.EmployeeTaxSE_SchoolYouthLimitInitial, tax.SchoolYouthLimitInitial.ToString(), taxInput.SchoolYouthLimitInitial.ToString()));
                    tax.SchoolYouthLimitInitial = taxInput.SchoolYouthLimitInitial;
                }
                if (tax.SinkType != (int)taxInput.SinkType)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Integer, "SinkType", TermGroup_TrackChangesColumnType.EmployeeTaxSE_SinkType, tax.SinkType.ToString(), ((int)taxInput.SinkType).ToString(), ((TermGroup_EmployeeTaxSinkType)tax.SinkType).ToString(), taxInput.SinkType.ToString()));
                    tax.SinkType = (int)taxInput.SinkType;
                }
                if (tax.EmploymentTaxType != (int)taxInput.EmploymentTaxType)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Integer, "EmploymentTaxType", TermGroup_TrackChangesColumnType.EmployeeTaxSE_EmploymentTaxType, tax.EmploymentTaxType.ToString(), ((int)taxInput.EmploymentTaxType).ToString(), ((TermGroup_EmployeeTaxEmploymentTaxType)tax.EmploymentTaxType).ToString(), taxInput.EmploymentTaxType.ToString()));
                    tax.EmploymentTaxType = (int)taxInput.EmploymentTaxType;
                }
                if (tax.EmploymentAbroadCode != (int)taxInput.EmploymentAbroadCode)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Integer, "EmploymentAbroadCode", TermGroup_TrackChangesColumnType.EmployeeTaxSE_EmploymentAbroadCode, tax.EmploymentAbroadCode.ToString(), ((int)taxInput.EmploymentAbroadCode).ToString(), ((TermGroup_EmployeeTaxEmploymentAbroadCode)tax.EmploymentAbroadCode).ToString(), taxInput.EmploymentAbroadCode.ToString()));
                    tax.EmploymentAbroadCode = (int)taxInput.EmploymentAbroadCode;
                }
                if (tax.RegionalSupport != taxInput.RegionalSupport)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Boolean, "RegionalSupport", TermGroup_TrackChangesColumnType.EmployeeTaxSE_RegionalSupport, tax.RegionalSupport.ToString(), taxInput.RegionalSupport.ToString()));
                    tax.RegionalSupport = taxInput.RegionalSupport;
                }
                if (tax.FirstEmployee != taxInput.FirstEmployee)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Boolean, "FirstEmployee", TermGroup_TrackChangesColumnType.EmployeeTaxSE_FirstEmployee, tax.FirstEmployee.ToString(), taxInput.FirstEmployee.ToString()));
                    tax.FirstEmployee = taxInput.FirstEmployee;
                }
                if (tax.SecondEmployee != taxInput.SecondEmployee)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Boolean, "SecondEmployee", TermGroup_TrackChangesColumnType.EmployeeTaxSE_SecondEmployee, tax.SecondEmployee.ToString(), taxInput.SecondEmployee.ToString()));
                    tax.SecondEmployee = taxInput.SecondEmployee;
                }
                if (tax.SalaryDistressCase != taxInput.SalaryDistressCase)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Boolean, "SalaryDistressCase", TermGroup_TrackChangesColumnType.EmployeeTaxSE_SalaryDistressCase, tax.SalaryDistressCase, taxInput.SalaryDistressCase));
                    tax.SalaryDistressCase = taxInput.SalaryDistressCase.Left(20);
                }
                if (tax.SalaryDistressAmount != taxInput.SalaryDistressAmount)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Decimal, "SalaryDistressAmount", TermGroup_TrackChangesColumnType.EmployeeTaxSE_SalaryDistressAmount, tax.SalaryDistressAmount.ToString(), taxInput.SalaryDistressAmount.ToString()));
                    tax.SalaryDistressAmount = taxInput.SalaryDistressAmount;
                }
                if (tax.SalaryDistressAmountType != (int)taxInput.SalaryDistressAmountType)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Integer, "SalaryDistressAmountType", TermGroup_TrackChangesColumnType.EmployeeTaxSE_SalaryDistressAmountType, tax.SalaryDistressAmountType.ToString(), ((int)taxInput.SalaryDistressAmountType).ToString(), ((TermGroup_EmployeeTaxSalaryDistressAmountType)tax.SalaryDistressAmountType).ToString(), taxInput.SalaryDistressAmountType.ToString()));
                    tax.SalaryDistressAmountType = (int)taxInput.SalaryDistressAmountType;
                }
                if (tax.SalaryDistressReservedAmount != taxInput.SalaryDistressReservedAmount)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Decimal, "SalaryDistressReservedAmount", TermGroup_TrackChangesColumnType.EmployeeTaxSE_SalaryDistressReserveAmount, tax.SalaryDistressReservedAmount.ToString(), taxInput.SalaryDistressReservedAmount.ToString()));
                    tax.SalaryDistressReservedAmount = taxInput.SalaryDistressReservedAmount;
                }
                if (!CalendarUtility.IsDatesEqual(tax.CSRExportDate, taxInput.CsrExportDate))
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Date, "CSRExportDate", TermGroup_TrackChangesColumnType.EmployeeTaxSE_CSRExportDate, tax.CSRExportDate.HasValue ? tax.CSRExportDate.Value.ToShortDateString() : null, taxInput.CsrExportDate.HasValue ? taxInput.CsrExportDate.Value.ToShortDateString() : null));
                    tax.CSRExportDate = taxInput.CsrExportDate;
                }
                if (!CalendarUtility.IsDatesEqual(tax.CSRImportDate, taxInput.CsrImportDate))
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Date, "CSRImportDate", TermGroup_TrackChangesColumnType.EmployeeTaxSE_CSRImportDate, tax.CSRImportDate.HasValue ? tax.CSRImportDate.Value.ToShortDateString() : null, taxInput.CsrImportDate.HasValue ? taxInput.CsrImportDate.Value.ToShortDateString() : null));
                    tax.CSRImportDate = taxInput.CsrImportDate;
                }
                if (tax.TinNumber != taxInput.TinNumber)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.String, "TinNumber", TermGroup_TrackChangesColumnType.EmployeeTaxSE_TinNumber, tax.TinNumber, taxInput.TinNumber));
                    tax.TinNumber = taxInput.TinNumber;
                }
                if (tax.CountryCode != taxInput.CountryCode)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.String, "CountryCode", TermGroup_TrackChangesColumnType.EmployeeTaxSE_CountryCode, tax.CountryCode, taxInput.CountryCode));
                    tax.CountryCode = taxInput.CountryCode;
                }
                if (tax.ApplyEmploymentTaxMinimumRule != taxInput.ApplyEmploymentTaxMinimumRule)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Boolean, "ApplyEmploymentTaxMinimumRule", TermGroup_TrackChangesColumnType.EmployeeTaxSE_ApplyEmploymentTaxMinimumRule, tax.ApplyEmploymentTaxMinimumRule.ToString(), taxInput.ApplyEmploymentTaxMinimumRule.ToString()));
                    tax.ApplyEmploymentTaxMinimumRule = taxInput.ApplyEmploymentTaxMinimumRule;
                }
                if (tax.BirthPlace != taxInput.BirthPlace)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.String, "BirthPlace", TermGroup_TrackChangesColumnType.EmployeeTaxSE_BirthPlace, tax.BirthPlace, taxInput.BirthPlace));
                    tax.BirthPlace = taxInput.BirthPlace;
                }
                if (tax.CountryCodeBirthPlace != taxInput.CountryCodeBirthPlace)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.String, "CountryCodeBirthPlace", TermGroup_TrackChangesColumnType.EmployeeTaxSE_CountryCodeBirthPlace, tax.CountryCodeBirthPlace, taxInput.CountryCodeBirthPlace));
                    tax.CountryCodeBirthPlace = taxInput.CountryCodeBirthPlace;
                }
                if (tax.CountryCodeCitizen != taxInput.CountryCodeCitizen)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.String, "CountryCodeCitizen", TermGroup_TrackChangesColumnType.EmployeeTaxSE_CountryCodeCitizen, tax.CountryCodeCitizen, taxInput.CountryCodeCitizen));
                    tax.CountryCodeCitizen = taxInput.CountryCodeCitizen;
                }
                if (tax.State != (int)taxInput.State)
                {
                    trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, taxInput.EmployeeId, SoeEntityType.EmployeeTaxSE, tax.EmployeeTaxId, SettingDataType.Integer, "State", TermGroup_TrackChangesColumnType.EmployeeTaxSE_State, tax.State.ToString(), ((int)taxInput.State).ToString(), ((SoeEntityState)tax.State).ToString(), taxInput.State.ToString()));
                    tax.State = (int)taxInput.State;
                }

                SetModifiedProperties(tax);

                #endregion
            }

            #endregion

            ActionResult result = SaveChanges(entities, transaction);
            if (result.Success)
            {
                if (changesRepository != null)
                {
                    changesRepository.SetAfterValue(new List<EmployeeTaxSE>() { tax });
                    if (saveTrackChanges)
                        TrackChangesManager.SaveEmployeeUserChanges(entities, transaction, changesRepository);
                }

                if (trackChangesItems.Any())
                {
                    foreach (TrackChangesDTO trackChangesItem in trackChangesItems)
                    {
                        trackChangesItem.RecordId = tax.EmployeeTaxId;
                        trackChangesItem.ParentEntity = SoeEntityType.Employee;
                        trackChangesItem.ParentRecordId = taxInput.EmployeeId;
                    }

                    result = TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);
                }

                employeeTaxId = tax.EmployeeTaxId;

                // Set success properties
                result.IntegerValue = employeeTaxId;
            }

            return result;
        }

        public ActionResult ValidateFirstEmployee(CompEntities entities, EmployeeTaxSEDTO taxInput, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (taxInput.FirstEmployee)
            {
                int count = (from t in entities.EmployeeTaxSE
                             where t.Employee.ActorCompanyId == actorCompanyId &&
                             t.FirstEmployee &&
                             t.State == (int)SoeEntityState.Active &&
                             t.EmployeeId != taxInput.EmployeeId &&
                             t.Employee.State == (int)SoeEntityState.Active
                             select t).Count();

                // Corona
                // Tillfälligt sänkta arbetsgivaravgifter under perioden 1 mars – 30 juni 2020 så att enbart ålderspensionsavgiften betalas.
                // Nedsättningen ska gälla för upp till 30 anställda och på den del av lönen för den anställde som inte överstiger 25 000 kronor per månad.
                int limit = DateTime.Today < new DateTime(2020, 7, 1) ? 30 : 1;
                if (count >= limit)
                    result = new ActionResult(GetText(4080, String.Format("Det får max finnas {0} anställd(a) som har inställningen 'Växa stöd' ikryssad", limit)));
            }

            return result;
        }

        public ActionResult ValidateSecondEmployee(CompEntities entities, EmployeeTaxSEDTO taxInput, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (taxInput.SecondEmployee)
            {
                int count = (from t in entities.EmployeeTaxSE
                             where t.Employee.ActorCompanyId == actorCompanyId &&
                             t.SecondEmployee &&
                             t.State == (int)SoeEntityState.Active &&
                             t.EmployeeId != taxInput.EmployeeId &&
                             t.Employee.State == (int)SoeEntityState.Active
                             select t).Count();

                int limit = 1;
                if (count >= limit)
                    result = new ActionResult(GetText(4088, String.Format("Det får max finnas {0} anställd(a) som har inställningen 'Utvidgat växa stöd' ikryssad", limit)));
            }

            return result;
        }

        public List<string> ImportEmployeeTaxSEDataFromDataStorage(int dataStorageId, int actorCompanyId, int userId)
        {
            DataStorage data = GeneralManager.GetDataStorage(dataStorageId, actorCompanyId);
            if (data == null)
                return new List<string>();

            var stream = new MemoryStream(data.Data);
            XDocument xdoc = new XDocument(stream);

            return ImportEmployeeTaxSEDataFromXML(xdoc, "CSRUtdrag", actorCompanyId, userId);
        }

        public List<string> ImportEmployeeTaxSEDataFromXML(XDocument xdoc, string rootName, int actorCompanyId, int userId)
        {
            List<string> responseMessages = new List<string>();

            if (xdoc != null && xdoc.Root != null && xdoc.Root.Name.LocalName == rootName)
            {
                XElement labelElement = XmlUtil.GetChildElement(xdoc, "Etikett");
                if (labelElement == null)
                {
                    responseMessages.Add(GetText(2062, "Filen är ej giltig!"));
                    return responseMessages;
                }

                XElement answerTag = XmlUtil.GetChildElement(labelElement, "Svarsetikett");
                if (answerTag == null)
                {
                    responseMessages.Add(GetText(2062, "Filen är ej giltig!"));
                    return responseMessages;
                }

                XElement transcript = XmlUtil.GetChildElement(xdoc, "Registerutdrag");
                XElement employer = XmlUtil.GetChildElement(transcript, "Arbetsgivare");

                List<XElement> employees = XmlUtil.GetChildElements(employer, "Anstalld");

                int i = 0;
                foreach (XElement employee in employees)
                {
                    if (employee.Attribute("felorsak") != null)
                    {
                        string errorCode = employee.Attribute("felorsak").Value;
                        responseMessages.Add(string.Format("Fel vid import av {0} , felkod {1} ({2})", employee.Attribute("personNr").Value, errorCode, GetEmployeeTaxErrorMessage(errorCode)));
                    }

                    string employeeTaxId = XmlUtil.GetChildElementValue(employee, "EgenReferens");
                    string parsedEmployeeTaxID = employeeTaxId.Split('-')[1].Trim();
                    bool isNum = double.TryParse(parsedEmployeeTaxID, out _);

                    if (parsedEmployeeTaxID != "" && isNum)
                    {
                        EmployeeTaxSEDTO employeeTaxSEDTO = GetEmployeeTaxSE(Convert.ToInt32(parsedEmployeeTaxID)).ToDTO();
                        if (employeeTaxSEDTO == null)  // If they exported from another system, we need to fix this
                        {
                            string employeeSocSec = employee.Attribute("personNr").Value;
                            if (string.IsNullOrEmpty(employeeSocSec))
                                continue; //Should never happen

                            string parsedemployeeSocSec = employeeSocSec.Replace("-", "").Trim();
                            Employee e = GetEmployeeBySocialSec(parsedemployeeSocSec, actorCompanyId);
                            if (e == null)
                                continue;

                            employeeTaxSEDTO = GetEmployeeTaxSE(e.EmployeeId, DateTime.Now.Year).ToDTO();

                            if (employeeTaxSEDTO == null)
                            {
                                using (CompEntities entities = new CompEntities())
                                {
                                    Employee e2 = GetEmployeeBySocialSec(entities, parsedemployeeSocSec, actorCompanyId);
                                    CreateEmployeeTaxSE(entities, e2, TermGroup_EmployeeTaxType.NoTax, DateTime.Now.Year, null, null, null);
                                }

                                employeeTaxSEDTO = GetEmployeeTaxSE(e.EmployeeId, DateTime.Now.Year).ToDTO();
                            }
                        }

                        if (employeeTaxSEDTO != null && employeeTaxSEDTO.MainEmployer)
                        {
                            XElement provisionalTaxInformation = XmlUtil.GetChildElement(employee, "Preliminarskatteuppgift");
                            XElement SBGDecision = XmlUtil.GetChildElement(provisionalTaxInformation, "SBGBeslut");

                            string taxForm = XmlUtil.GetChildElementValue(provisionalTaxInformation, "Skatteform");
                            string taxRate = XmlUtil.GetChildElementValue(provisionalTaxInformation, "Tabell");
                            string sbgProcent = XmlUtil.GetChildElementValue(SBGDecision, "Procent");
                            string sbgAmount = XmlUtil.GetChildElementValue(SBGDecision, "Belopp");

                            /* Import */
                            if (taxForm != "")
                            {
                                switch (taxForm)
                                {
                                    case "A":
                                        employeeTaxSEDTO.Type = TermGroup_EmployeeTaxType.TableTax;
                                        break;

                                    case "F":
                                        employeeTaxSEDTO.Type = TermGroup_EmployeeTaxType.NoTax;
                                        break;

                                    case "FA":
                                        employeeTaxSEDTO.Type = TermGroup_EmployeeTaxType.TableTax;
                                        break;

                                    default:
                                        break;
                                }
                            }

                            if (taxRate != "")
                                employeeTaxSEDTO.TaxRate = Convert.ToInt32(taxRate.Remove(taxRate.Length - 1));

                            /* jämkning - procent */
                            if (sbgProcent != "")
                            {
                                employeeTaxSEDTO.Type = TermGroup_EmployeeTaxType.Adjustment;
                                employeeTaxSEDTO.AdjustmentType = TermGroup_EmployeeTaxAdjustmentType.PercentTax;
                                employeeTaxSEDTO.AdjustmentValue = Convert.ToDecimal(sbgProcent);
                            }

                            /* jämkning - belopp */
                            if (sbgAmount != "")
                            {
                                employeeTaxSEDTO.Type = TermGroup_EmployeeTaxType.Adjustment;

                                if (int.TryParse(sbgAmount, out int x) && x > 0)
                                {
                                    employeeTaxSEDTO.AdjustmentType = TermGroup_EmployeeTaxAdjustmentType.IncreasedTaxBase;
                                    employeeTaxSEDTO.AdjustmentValue = Convert.ToDecimal(sbgAmount);
                                }
                                else
                                {
                                    employeeTaxSEDTO.AdjustmentType = TermGroup_EmployeeTaxAdjustmentType.DecreasedTaxBase;
                                    employeeTaxSEDTO.AdjustmentValue = Convert.ToDecimal(sbgAmount.Replace("-", ""));
                                }
                            }

                            employeeTaxSEDTO.TaxRateColumn = 1;
                            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                            employeeTaxSEDTO.OneTimeTaxPercent = GetDefaultOneTimeTaxRate(entitiesReadOnly, employeeTaxSEDTO.Year, actorCompanyId);
                            employeeTaxSEDTO.CsrImportDate = DateTime.Now;

                            SaveEmployeeTaxSE(employeeTaxSEDTO, actorCompanyId, TermGroup_TrackChangesActionMethod.Employee_CsrInquiry);
                        }
                        responseMessages.Add(string.Format(GetText(2063, "Anställd med personnummer") + " {0} " + GetText(2142, "är importerad"), employee.Attribute("personNr").Value));
                        i++;
                    }
                }
            }
            else
            {
                responseMessages.Add(GetText(2062, "Filen är ej giltig!"));
                return responseMessages;
            }

            return responseMessages;
        }

        private string GetEmployeeTaxErrorMessage(string errorCode)
        {
            switch (errorCode)
            {
                case "1":
                    return GetText(2143, "Person-/organisationsnummer är felaktigt");
                case "2":
                    return GetText(2144, "Person-/organisationsnummer saknas i skatteregistrett");
                case "3":
                    return GetText(2145, "Övriga fel");
                default:
                    return GetText(2146, "Fel inträffade!");
            }
        }

        private decimal GetDefaultOneTimeTaxRate(CompEntities entities, int year, int ActorCompanyId)
        {
            decimal payrollPriceAmount = PayrollManager.GetSysPayrollPriceAmount(entities, ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_DefaultOneTimeTax, CalendarUtility.GetEndOfYear(year));
            return Decimal.Multiply(payrollPriceAmount, 100M);
        }

        #endregion

        #region EmployeeTemplate

        public bool HasEmployeeTemplates(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            return entitiesReadOnly.EmployeeTemplate.Any(t => t.ActorCompanyId == actorCompanyId && t.State == (int)SoeEntityState.Active);
        }

        public bool HasEmployeeTemplatesOfTypeSubstituteShifts(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            return entitiesReadOnly.EmployeeTemplate.Any(t => t.ActorCompanyId == actorCompanyId && t.State == (int)SoeEntityState.Active && t.EmployeeTemplateGroup.Any(g => g.Type == (int)TermGroup_EmployeeTemplateGroupType.SubstituteShifts && g.State == (int)SoeEntityState.Active));
        }

        public List<EmployeeTemplate> GetEmployeeTemplates(int actorCompanyId, bool loadCollectiveAgreement = false, bool loadGroups = false, bool loadRows = false, bool onlyActive = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeTemplate.NoTracking();
            return GetEmployeeTemplates(entities, actorCompanyId, loadCollectiveAgreement, loadGroups, loadRows, onlyActive);
        }

        public List<EmployeeTemplate> GetEmployeeTemplates(CompEntities entities, int actorCompanyId, bool loadCollectiveAgreement = false, bool loadGroups = false, bool loadRows = false, bool onlyActive = false, int? payrollGroupId = null)
        {
            IQueryable<EmployeeTemplate> query = entities.EmployeeTemplate;
            if (loadCollectiveAgreement || payrollGroupId.HasValue)
                query = query.Include("EmployeeCollectiveAgreement");
            if (loadGroups)
                query = query.Include("EmployeeTemplateGroup");
            if (loadRows)
                query = query.Include("EmployeeTemplateGroup.EmployeeTemplateGroupRow");

            List<EmployeeTemplate> templates = (from t in query
                                                where t.ActorCompanyId == actorCompanyId &&
                                                (onlyActive ? t.State == (int)SoeEntityState.Active : t.State == (int)SoeEntityState.Active || t.State == (int)SoeEntityState.Inactive)
                                                orderby t.Name
                                                select t).ToList();

            if (payrollGroupId.HasValue)
                templates = templates.Where(w => w.EmployeeCollectiveAgreement.PayrollGroupId == payrollGroupId.Value).ToList();

            return templates;
        }

        public Dictionary<int, string> GetEmployeeTemplatesDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = GetEmployeeTemplates(actorCompanyId, false, false, false, true).ToDictionary(k => k.EmployeeTemplateId, v => v.Name);
            if (addEmptyRow)
                dict.Add(0, " ");

            return dict.Sort();
        }

        public Dictionary<int, string> GetEmployeeTemplatesOfTypeSubstituteShifts(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeTemplate.NoTracking();
            List<EmployeeTemplate> templates = (from t in entities.EmployeeTemplate
                                                where t.ActorCompanyId == actorCompanyId &&
                                                t.State == (int)SoeEntityState.Active &&
                                                t.EmployeeTemplateGroup.Any(g => g.Type == (int)TermGroup_EmployeeTemplateGroupType.SubstituteShifts && g.State == (int)SoeEntityState.Active)
                                                orderby t.Name
                                                select t).ToList();

            Dictionary<int, string> dict = templates.ToDictionary(k => k.EmployeeTemplateId, v => v.Name);

            return dict;
        }

        public EmployeeTemplate GetEmployeeTemplate(int actorCompanyId, int employeeTemplateId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeTemplate.NoTracking();
            return GetEmployeeTemplate(entities, actorCompanyId, employeeTemplateId);
        }

        public EmployeeTemplate GetEmployeeTemplate(CompEntities entities, int actorCompanyId, int employeeTemplateId)
        {
            return entities.EmployeeTemplate.Include("EmployeeTemplateGroup.EmployeeTemplateGroupRow").FirstOrDefault(w => w.ActorCompanyId == actorCompanyId && w.EmployeeTemplateId == employeeTemplateId && (w.State == (int)SoeEntityState.Active || w.State == (int)SoeEntityState.Inactive));
        }

        public ActionResult DeleteEmployeeTemplate(int employeeTemplateId)
        {
            using (CompEntities entities = new CompEntities())
            {
                EmployeeTemplate employeeTemplate = GetEmployeeTemplate(entities, base.ActorCompanyId, employeeTemplateId);
                if (employeeTemplate == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "EmployeeTemplate");

                return ChangeEntityState(entities, employeeTemplate, SoeEntityState.Deleted, true);

            }
        }

        public ActionResult SaveEmployeeTemplate(EmployeeTemplateDTO employeeTemplateInput, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            int employeeTemplateId = employeeTemplateInput.EmployeeTemplateId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region EmployeeTemplate

                        EmployeeTemplate employeeTemplate = null;
                        if (employeeTemplateInput.EmployeeTemplateId != 0)
                            employeeTemplate = GetEmployeeTemplate(entities, actorCompanyId, employeeTemplateInput.EmployeeTemplateId);

                        if (employeeTemplate == null)
                        {
                            employeeTemplate = new EmployeeTemplate()
                            {
                                ActorCompanyId = actorCompanyId
                            };
                            entities.EmployeeTemplate.AddObject(employeeTemplate);
                            SetCreatedProperties(employeeTemplate);
                        }
                        else
                            SetModifiedProperties(employeeTemplate);

                        employeeTemplate.EmployeeCollectiveAgreementId = employeeTemplateInput.EmployeeCollectiveAgreementId;
                        employeeTemplate.Code = employeeTemplateInput.Code;
                        employeeTemplate.ExternalCode = employeeTemplateInput.ExternalCode;
                        employeeTemplate.Name = employeeTemplateInput.Name;
                        employeeTemplate.Description = employeeTemplateInput.Description;
                        employeeTemplate.Title = employeeTemplateInput.Title;
                        employeeTemplate.State = (int)employeeTemplateInput.State;

                        #endregion

                        #region EmployeeTemplateGroups and rows

                        #region Update/delete

                        foreach (EmployeeTemplateGroup group in employeeTemplate.EmployeeTemplateGroup.Where(g => g.State == (int)SoeEntityState.Active).ToList())
                        {
                            EmployeeTemplateGroupDTO groupInput = !employeeTemplateInput.EmployeeTemplateGroups.IsNullOrEmpty() ? employeeTemplateInput.EmployeeTemplateGroups.FirstOrDefault(g => g.EmployeeTemplateGroupId == group.EmployeeTemplateGroupId) : null;
                            if (groupInput != null)
                            {
                                #region Update

                                UpdateEmployeeTemplateGroup(group, groupInput, true);
                                employeeTemplateInput.EmployeeTemplateGroups.Remove(groupInput);

                                #endregion
                            }
                            else
                            {
                                #region Delete

                                DeleteEmployeeTemplateGroup(group);

                                #endregion
                            }
                        }

                        #endregion

                        #region Add

                        if (!employeeTemplateInput.EmployeeTemplateGroups.IsNullOrEmpty())
                        {
                            foreach (EmployeeTemplateGroupDTO groupInput in employeeTemplateInput.EmployeeTemplateGroups)
                            {
                                EmployeeTemplateGroup newGroup = AddEmployeeTemplateGroup(groupInput);
                                newGroup.EmployeeTemplate = employeeTemplate;
                            }
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            transaction.Complete();
                            employeeTemplateId = employeeTemplate.EmployeeTemplateId;
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
                        // Set success properties
                        result.IntegerValue = employeeTemplateId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private EmployeeTemplateGroup AddEmployeeTemplateGroup(EmployeeTemplateGroupDTO groupInput)
        {
            EmployeeTemplateGroup group = new EmployeeTemplateGroup();
            UpdateEmployeeTemplateGroup(group, groupInput, false);
            SetCreatedProperties(group);

            return group;
        }

        private void UpdateEmployeeTemplateGroup(EmployeeTemplateGroup group, EmployeeTemplateGroupDTO groupInput, bool setAsModified)
        {
            group.Type = (int)groupInput.Type;
            group.Code = groupInput.Code;
            group.Name = groupInput.Name;
            group.Description = groupInput.Description;
            group.SortOrder = groupInput.SortOrder;
            group.NewPageBefore = groupInput.NewPageBefore;
            group.State = (int)groupInput.State;

            if (setAsModified)
                SetModifiedProperties(group);

            if (group.EmployeeTemplateGroupRow != null)
            {
                foreach (EmployeeTemplateGroupRow row in group.EmployeeTemplateGroupRow.Where(r => r.State == (int)SoeEntityState.Active).ToList())
                {
                    EmployeeTemplateGroupRowDTO rowInput = groupInput.EmployeeTemplateGroupRows.FirstOrDefault(r => r.EmployeeTemplateGroupRowId == row.EmployeeTemplateGroupRowId);
                    if (rowInput != null)
                    {
                        UpdateEmployeeTemplateGroupRow(row, rowInput, true);
                        groupInput.EmployeeTemplateGroupRows.Remove(rowInput);
                    }
                    else
                    {
                        DeleteEmployeeTemplateGroupRow(row);
                    }
                }

                if (!groupInput.EmployeeTemplateGroupRows.IsNullOrEmpty())
                {
                    if (group.EmployeeTemplateGroupRow == null)
                        group.EmployeeTemplateGroupRow = new EntityCollection<EmployeeTemplateGroupRow>();

                    foreach (EmployeeTemplateGroupRowDTO rowInput in groupInput.EmployeeTemplateGroupRows)
                    {
                        group.EmployeeTemplateGroupRow.Add(AddEmployeeTemplateGroupRow(rowInput));
                    }
                }
            }
        }

        private void DeleteEmployeeTemplateGroup(EmployeeTemplateGroup group)
        {
            ChangeEntityState(group, SoeEntityState.Deleted);

            if (!group.EmployeeTemplateGroupRow.IsNullOrEmpty())
            {
                foreach (EmployeeTemplateGroupRow row in group.EmployeeTemplateGroupRow)
                {
                    DeleteEmployeeTemplateGroupRow(row);
                }
            }
        }

        private EmployeeTemplateGroupRow AddEmployeeTemplateGroupRow(EmployeeTemplateGroupRowDTO rowInput)
        {
            EmployeeTemplateGroupRow row = new EmployeeTemplateGroupRow();
            UpdateEmployeeTemplateGroupRow(row, rowInput, false);
            SetCreatedProperties(row);

            return row;
        }

        private void UpdateEmployeeTemplateGroupRow(EmployeeTemplateGroupRow row, EmployeeTemplateGroupRowDTO rowInput, bool setAsModified)
        {
            row.Type = (int)rowInput.Type;
            row.MandatoryLevel = rowInput.MandatoryLevel;
            row.RegistrationLevel = rowInput.RegistrationLevel;
            row.Title = rowInput.Title;
            row.DefaultValue = rowInput.DefaultValue;
            row.Comment = rowInput.Comment;
            row.Row = rowInput.Row;
            row.StartColumn = rowInput.StartColumn;
            row.SpanColumns = rowInput.SpanColumns;
            row.Format = rowInput.Format;
            row.HideInReport = rowInput.HideInReport;
            row.HideInReportIfEmpty = rowInput.HideInReportIfEmpty;
            row.HideInRegistration = rowInput.HideInRegistration;
            row.HideInEmploymentRegistration = rowInput.HideInEmploymentRegistration;
            row.Entity = (int?)rowInput.Entity;
            row.RecordId = rowInput.RecordId;
            row.State = (int)rowInput.State;

            if (setAsModified)
                SetModifiedProperties(row);
        }

        private void DeleteEmployeeTemplateGroupRow(EmployeeTemplateGroupRow row)
        {
            ChangeEntityState(row, SoeEntityState.Deleted);
        }

        #endregion

        #region EmployeeVacationSE

        public List<EmployeeVacationSE> GetEmployeeVacationSEs(int employeeId, bool onlyActive)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeVacationSE.NoTracking();
            return GetEmployeeVacationSEs(entities, employeeId, onlyActive);
        }

        public List<EmployeeVacationSE> GetEmployeeVacationSEs(CompEntities entities, int employeeId, bool onlyActive)
        {
            return (from e in entities.EmployeeVacationSE
                    where e.EmployeeId == employeeId &&
                    (onlyActive ? e.State == (int)SoeEntityState.Active : e.State != (int)SoeEntityState.Deleted)
                    orderby e.Created, e.Modified
                    select e).ToList();
        }

        public List<EmployeeVacationSE> GetEmployeeVacationSEs(List<int> employeeIds, bool onlyActive)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeVacationSE.NoTracking();
            return GetEmployeeVacationSEs(entities, employeeIds, onlyActive);
        }

        public List<EmployeeVacationSE> GetEmployeeVacationSEs(CompEntities entities, List<int> employeeIds, bool onlyActive)
        {
            return (from e in entities.EmployeeVacationSE
                    where employeeIds.Contains(e.EmployeeId) &&
                    (onlyActive ? e.State == (int)SoeEntityState.Active : e.State != (int)SoeEntityState.Deleted)
                    orderby e.Created, e.Modified
                    select e).ToList();
        }

        public List<EmployeeVacationSE> GetEmployeeVacationSEs(int employeeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeVacationSE.NoTracking();
            return GetEmployeeVacationSEs(entities, employeeId);
        }

        public List<EmployeeVacationSE> GetEmployeeVacationSEs(CompEntities entities, int employeeId)
        {
            return (from e in entities.EmployeeVacationSE
                    where e.EmployeeId == employeeId &&
                    e.State == (int)SoeEntityState.Active
                    orderby e.Created, e.Modified
                    select e).ToList();
        }

        public EmployeeVacationSE GetLatestEmployeeVacationSE(int employeeId, DateTime? createdBefore = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeVacationSE.NoTracking();
            return GetLatestEmployeeVacationSE(entities, employeeId, createdBefore);
        }

        public EmployeeVacationSE GetLatestEmployeeVacationSE(CompEntities entities, int employeeId, DateTime? createdBefore = null)
        {
            int actorCompanyId = base.ActorCompanyId;

            if (!createdBefore.HasValue)
                return (from e in entities.EmployeeVacationSE
                        where e.EmployeeId == employeeId &&
                        e.Employee.ActorCompanyId == actorCompanyId &&
                        e.State == (int)SoeEntityState.Active
                        orderby e.Created descending, e.Modified descending
                        select e).FirstOrDefault();
            else // Ignore state
                return (from e in entities.EmployeeVacationSE
                        where e.EmployeeId == employeeId &&
                        e.Employee.ActorCompanyId == actorCompanyId &&
                        e.Created < createdBefore.Value
                        orderby e.Created descending, e.Modified descending
                        select e).FirstOrDefault();
        }

        #endregion

        #region EmployeeVehicle

        public List<EmployeeVehicle> GetEmployeeVehicles(int actorCompanyId, bool loadEmployee = false, bool loadDeduction = false, bool loadEquipment = false, bool loadTax = false, bool calculateTaxableValue = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeVehicle.NoTracking();
            return GetEmployeeVehicles(entities, actorCompanyId, loadEmployee, loadDeduction, loadEquipment, loadTax, calculateTaxableValue);
        }

        public List<EmployeeVehicle> GetEmployeeVehicles(CompEntities entities, int actorCompanyId, bool loadEmployee = false, bool loadDeduction = false, bool loadEquipment = false, bool loadTax = false, bool calculateTaxableValue = false)
        {
            IQueryable<EmployeeVehicle> query = entities.EmployeeVehicle;
            if (loadEmployee)
                query = query.Include("Employee.ContactPerson");
            if (loadDeduction)
                query = query.Include("EmployeeVehicleDeduction");
            if (loadEquipment)
                query = query.Include("EmployeeVehicleEquipment");
            if (loadTax)
                query = query.Include("EmployeeVehicleTax");

            var vehicles = (from ev in query
                            where ev.ActorCompanyId == actorCompanyId &&
                            ev.State == (int)SoeEntityState.Active
                            select ev).ToList();

            if (calculateTaxableValue)
            {
                var dtos = this.CalculateEmployeeVehiclesAmounts(entities, vehicles, DateTime.Today);
                foreach (var dto in dtos)
                {
                    var vehicle = vehicles.FirstOrDefault(x => x.EmployeeVehicleId == dto.EmployeeVehicleId);
                    if (vehicle != null)
                        vehicle.TaxableValue = dto.TaxableValue;
                }
            }
            return vehicles;
        }

        public List<EmployeeVehicle> GetEmployeeVehicles(int employeeId, int actorCompanyId, bool loadEmployee = false, bool loadDeduction = false, bool loadEquipment = false, bool loadTax = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeVehicle.NoTracking();
            return GetEmployeeVehicles(entities, employeeId, actorCompanyId, loadEmployee, loadDeduction, loadEquipment, loadTax);
        }

        public List<EmployeeVehicle> GetEmployeeVehicles(CompEntities entities, int employeeId, int actorCompanyId, bool loadEmployee = false, bool loadDeduction = false, bool loadEquipment = false, bool loadTax = false)
        {
            IQueryable<EmployeeVehicle> query = entities.EmployeeVehicle;
            if (loadEmployee)
                query = query.Include("Employee.ContactPerson");
            if (loadDeduction)
                query = query.Include("EmployeeVehicleDeduction");
            if (loadEquipment)
                query = query.Include("EmployeeVehicleEquipment");
            if (loadTax)
                query = query.Include("EmployeeVehicleTax");

            return (from ev in query
                    where ev.ActorCompanyId == actorCompanyId &&
                    ev.EmployeeId == employeeId &&
                    ev.State == (int)SoeEntityState.Active
                    select ev).ToList();
        }

        public EmployeeVehicle GetEmployeeVehicle(int employeeVehicleId, bool loadEmployee, bool loadDeduction, bool loadEquipment, bool loadTax)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeVehicle.NoTracking();
            return GetEmployeeVehicle(entities, employeeVehicleId, loadEmployee, loadDeduction, loadEquipment, loadTax);
        }

        public EmployeeVehicle GetEmployeeVehicle(CompEntities entities, int employeeVehicleId, bool loadEmployee, bool loadDeduction, bool loadEquipment, bool loadTax)
        {
            int actorCompanyId = base.ActorCompanyId;

            IQueryable<EmployeeVehicle> query = entities.EmployeeVehicle;
            if (loadEmployee)
                query = query.Include("Employee.ContactPerson");
            if (loadDeduction)
                query = query.Include("EmployeeVehicleDeduction");
            if (loadEquipment)
                query = query.Include("EmployeeVehicleEquipment");
            if (loadTax)
                query = query.Include("EmployeeVehicleTax");

            return (from ev in query
                    where ev.EmployeeVehicleId == employeeVehicleId &&
                    ev.ActorCompanyId == actorCompanyId
                    select ev).FirstOrDefault();
        }

        public ActionResult SaveEmployeeVehicle(EmployeeVehicleDTO employeeVehicleInput)
        {
            if (employeeVehicleInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeVehicle");

            // Default result is successful
            ActionResult result = new ActionResult();

            int employeeVehicleId = employeeVehicleInput.EmployeeVehicleId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region EmployeeVehicle

                        EmployeeVehicle employeeVehicle = GetEmployeeVehicle(entities, employeeVehicleId, false, true, true, true);
                        if (employeeVehicle == null)
                        {
                            #region Add

                            #region EmployeeVehicle

                            employeeVehicle = new EmployeeVehicle()
                            {
                                ActorCompanyId = parameterObject.SoeCompany.ActorCompanyId,
                                EmployeeId = employeeVehicleInput.EmployeeId
                            };
                            SetCreatedProperties(employeeVehicle);
                            entities.EmployeeVehicle.AddObject(employeeVehicle);

                            #endregion

                            #region Deduction

                            if (employeeVehicleInput.Deduction != null)
                                AddEmployeeVehicleDeduction(employeeVehicle, employeeVehicleInput.Deduction);

                            #endregion

                            #region Equipment

                            if (employeeVehicleInput.Equipment != null)
                                AddEmployeeVehicleEquipment(employeeVehicle, employeeVehicleInput.Equipment);

                            #endregion

                            #region Tax

                            if (employeeVehicleInput.Tax != null)
                                AddEmployeeVehicleTax(employeeVehicle, employeeVehicleInput.Tax);

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            #region EmployeeVehicle

                            SetModifiedProperties(employeeVehicle);

                            #endregion

                            #region Deduction

                            if (employeeVehicleInput.Deduction != null)
                                UpdateEmployeeVehicleDeduction(employeeVehicle, employeeVehicleInput.Deduction);

                            #endregion

                            #region Equipment

                            if (employeeVehicleInput.Equipment != null)
                                UpdateEmployeeVehicleEquipment(employeeVehicle, employeeVehicleInput.Equipment);

                            #endregion

                            #region Tax

                            if (employeeVehicleInput.Tax != null)
                                UpdateEmployeeVehicleTax(employeeVehicle, employeeVehicleInput.Tax);

                            #endregion

                            #endregion
                        }

                        employeeVehicle.Year = employeeVehicleInput.Year;
                        employeeVehicle.FromDate = employeeVehicleInput.FromDate;
                        employeeVehicle.ToDate = employeeVehicleInput.ToDate;
                        employeeVehicle.SysVehicleTypeId = employeeVehicleInput.SysVehicleTypeId;
                        employeeVehicle.Type = (int)employeeVehicleInput.Type;
                        employeeVehicle.LicensePlateNumber = employeeVehicleInput.LicensePlateNumber;
                        employeeVehicle.ModelCode = employeeVehicleInput.ModelCode;
                        employeeVehicle.VehicleMake = employeeVehicleInput.VehicleMake;
                        employeeVehicle.VehicleModel = employeeVehicleInput.VehicleModel;
                        employeeVehicle.RegisteredDate = employeeVehicleInput.RegisteredDate;
                        employeeVehicle.FuelType = (int)employeeVehicleInput.FuelType;
                        employeeVehicle.HasExtensiveDriving = employeeVehicleInput.HasExtensiveDriving;
                        employeeVehicle.Price = employeeVehicleInput.Price;
                        employeeVehicle.PriceAdjustment = employeeVehicleInput.PriceAdjustment;
                        employeeVehicle.CodeForComparableModel = employeeVehicleInput.CodeForComparableModel;
                        employeeVehicle.ComparablePrice = employeeVehicleInput.ComparablePrice;
                        employeeVehicle.BenefitValueAdjustment = employeeVehicleInput.BenefitValueAdjustment;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            employeeVehicleId = employeeVehicle.EmployeeVehicleId;
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
                        // Set success properties
                        result.IntegerValue = employeeVehicleId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private void AddEmployeeVehicleDeduction(EmployeeVehicle employeeVehicle, List<EmployeeVehicleDeductionDTO> deductions)
        {
            foreach (EmployeeVehicleDeductionDTO deduction in deductions)
            {
                EmployeeVehicleDeduction employeeVehicleDeduction = new EmployeeVehicleDeduction()
                {
                    FromDate = deduction.FromDate,
                    Price = deduction.Price
                };
                SetCreatedProperties(employeeVehicleDeduction);
                employeeVehicle.EmployeeVehicleDeduction.Add(employeeVehicleDeduction);
            }
        }

        private void UpdateEmployeeVehicleDeduction(EmployeeVehicle employeeVehicle, List<EmployeeVehicleDeductionDTO> deductions)
        {
            // Loop through existing deduction
            foreach (EmployeeVehicleDeduction employeeVehicleDeduction in employeeVehicle.EmployeeVehicleDeduction.ToList())
            {
                EmployeeVehicleDeductionDTO existsInInput = deductions.FirstOrDefault(e => e.EmployeeVehicleDeductionId == employeeVehicleDeduction.EmployeeVehicleDeductionId);
                if (existsInInput != null)
                {
                    // Deduction still exists in input, update it if it has changed and remove from input
                    if (employeeVehicleDeduction.FromDate != existsInInput.FromDate || employeeVehicleDeduction.Price != existsInInput.Price)
                    {
                        employeeVehicleDeduction.FromDate = existsInInput.FromDate;
                        employeeVehicleDeduction.Price = existsInInput.Price;
                        SetModifiedProperties(employeeVehicleDeduction);
                    }
                    deductions.Remove(existsInInput);
                }
                else
                {
                    // Deduction does not exist in input, set as deleted
                    ChangeEntityState(employeeVehicleDeduction, SoeEntityState.Deleted);
                }
            }

            // Add new deduction (remaining in input)
            AddEmployeeVehicleDeduction(employeeVehicle, deductions);
        }

        private void AddEmployeeVehicleEquipment(EmployeeVehicle employeeVehicle, List<EmployeeVehicleEquipmentDTO> equipments)
        {
            foreach (EmployeeVehicleEquipmentDTO equipment in equipments)
            {
                EmployeeVehicleEquipment employeeVehicleEquipment = new EmployeeVehicleEquipment()
                {
                    Description = equipment.Description,
                    FromDate = equipment.FromDate,
                    ToDate = equipment.ToDate,
                    Price = equipment.Price
                };
                SetCreatedProperties(employeeVehicleEquipment);
                employeeVehicle.EmployeeVehicleEquipment.Add(employeeVehicleEquipment);
            }
        }

        private void UpdateEmployeeVehicleEquipment(EmployeeVehicle employeeVehicle, List<EmployeeVehicleEquipmentDTO> equipments)
        {
            // Loop through existing equipment
            foreach (EmployeeVehicleEquipment employeeVehicleEquipment in employeeVehicle.EmployeeVehicleEquipment.ToList())
            {
                EmployeeVehicleEquipmentDTO existsInInput = equipments.FirstOrDefault(e => e.EmployeeVehicleEquipmentId == employeeVehicleEquipment.EmployeeVehicleEquipmentId);
                if (existsInInput != null)
                {
                    // Equipment still exists in input, update it if it has changed and remove from input
                    if (employeeVehicleEquipment.Description != existsInInput.Description ||
                        employeeVehicleEquipment.FromDate != existsInInput.FromDate ||
                        employeeVehicleEquipment.ToDate != existsInInput.ToDate ||
                        employeeVehicleEquipment.Price != existsInInput.Price)
                    {
                        employeeVehicleEquipment.Description = existsInInput.Description;
                        employeeVehicleEquipment.FromDate = existsInInput.FromDate;
                        employeeVehicleEquipment.ToDate = existsInInput.ToDate;
                        employeeVehicleEquipment.Price = existsInInput.Price;
                        SetModifiedProperties(employeeVehicleEquipment);
                    }
                    equipments.Remove(existsInInput);
                }
                else
                {
                    // Equipment does not exist in input, set as deleted
                    ChangeEntityState(employeeVehicleEquipment, SoeEntityState.Deleted);
                }
            }

            // Add new equipment (remaining in input)
            AddEmployeeVehicleEquipment(employeeVehicle, equipments);
        }

        private void AddEmployeeVehicleTax(EmployeeVehicle employeeVehicle, List<EmployeeVehicleTaxDTO> taxes)
        {
            foreach (EmployeeVehicleTaxDTO tax in taxes)
            {
                EmployeeVehicleTax employeeVehicleTax = new EmployeeVehicleTax()
                {
                    FromDate = tax.FromDate,
                    Amount = tax.Amount
                };
                SetCreatedProperties(employeeVehicleTax);
                employeeVehicle.EmployeeVehicleTax.Add(employeeVehicleTax);
            }
        }

        private void UpdateEmployeeVehicleTax(EmployeeVehicle employeeVehicle, List<EmployeeVehicleTaxDTO> taxes)
        {
            // Loop through existing tax
            foreach (EmployeeVehicleTax employeeVehicleTax in employeeVehicle.EmployeeVehicleTax.ToList())
            {
                EmployeeVehicleTaxDTO existsInInput = taxes.FirstOrDefault(e => e.EmployeeVehicleTaxId == employeeVehicleTax.EmployeeVehicleTaxId);
                if (existsInInput != null)
                {
                    // Tax still exists in input, update it if it has changed and remove from input
                    if (employeeVehicleTax.FromDate != existsInInput.FromDate || employeeVehicleTax.Amount != existsInInput.Amount)
                    {
                        employeeVehicleTax.FromDate = existsInInput.FromDate;
                        employeeVehicleTax.Amount = existsInInput.Amount;
                        SetModifiedProperties(employeeVehicleTax);
                    }
                    taxes.Remove(existsInInput);
                }
                else
                {
                    // Tax does not exist in input, set as deleted
                    ChangeEntityState(employeeVehicleTax, SoeEntityState.Deleted);
                }
            }

            // Add new tax (remaining in input)
            AddEmployeeVehicleTax(employeeVehicle, taxes);
        }

        public ActionResult DeleteEmployeeVehicle(int employeeVehicleId)
        {
            using (CompEntities entities = new CompEntities())
            {
                EmployeeVehicle employeeVehicle = GetEmployeeVehicle(entities, employeeVehicleId, false, false, false, false);
                if (employeeVehicle == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "EmployeeVehicle");

                return ChangeEntityState(entities, employeeVehicle, SoeEntityState.Deleted, true);
            }
        }

        public List<EmployeeVehiclePayrollCalculationDTO> CalculateEmployeeVehiclesAmounts(CompEntities entities, int employeeId, int actorCompanyId, DateTime date)
        {
            List<EmployeeVehiclePayrollCalculationDTO> calculationDtos = new List<EmployeeVehiclePayrollCalculationDTO>();

            #region Prereq

            //Dates for current month
            DateTime dateFrom = CalendarUtility.GetBeginningOfMonth(date);
            DateTime dateTo = CalendarUtility.GetEndOfMonth(date);

            //Get EmployeeVechicles for Employee in correct daterange
            List<EmployeeVehicle> employeeVechicles = GetEmployeeVehicles(entities, employeeId, actorCompanyId, loadDeduction: true, loadEquipment: true, loadTax: true);
            employeeVechicles = employeeVechicles.Where(i => i.CalculatedFromDate <= dateTo && i.CalculatedToDate >= dateFrom).ToList();
            if (employeeVechicles.Count == 0)
                return calculationDtos;

            #endregion

            #region Build EmployeeVechicle structure

            //employeeVehicleId, days in month, startDate in month, stopDate in month
            var employeeVechicleTuples = new List<Tuple<int, int, DateTime, DateTime>>();

            foreach (var employeeVechicle in employeeVechicles)
            {
                //Dates for current month and EmployeeVehicle
                DateTime employeeVechicleDateFrom = CalendarUtility.GetLatestDate(employeeVechicle.CalculatedFromDate, dateFrom);
                DateTime employeeVechicleDateTo = CalendarUtility.GetEarliestDate(employeeVechicle.CalculatedToDate, dateTo);

                employeeVechicleTuples.Add(Tuple.Create(employeeVechicle.EmployeeVehicleId, (int)employeeVechicleDateTo.Subtract(employeeVechicleDateFrom).TotalDays + 1, employeeVechicleDateFrom, employeeVechicleDateTo));
            }

            #endregion

            #region Evaluate EmployeeVechicle structure

            var employeeVechicleTuplesLongest = new List<Tuple<int, int, DateTime, DateTime>>();
            var employeeVechicleTuplesOverlapping = new List<Tuple<int, int, DateTime, DateTime>>();

            if (employeeVechicleTuples.Count > 0)
            {
                if (employeeVechicleTuples.Count == 1)
                {
                    //One vehicle found
                    employeeVechicleTuplesLongest.Add(employeeVechicleTuples.First());
                }
                else
                {
                    int longestDays = employeeVechicleTuples.Max(i => i.Item2);

                    //Add into longest or overlapping
                    foreach (var tuple in employeeVechicleTuples.Where(i => i.Item2 == longestDays))
                    {
                        bool overlapping = false;
                        foreach (var tupleLongest in employeeVechicleTuplesLongest)
                        {
                            if (CalendarUtility.IsDatesOverlapping(tuple.Item3, tuple.Item4, tupleLongest.Item3, tupleLongest.Item4))
                                overlapping = true;
                        }

                        if (overlapping)
                            employeeVechicleTuplesOverlapping.Add(tuple);
                        else
                            employeeVechicleTuplesLongest.Add(tuple);
                    }

                    //Add into overlapping any other vehicle that overlaps with any one of the longest                    
                    foreach (var tuple in employeeVechicleTuples.Where(i => i.Item2 < longestDays))
                    {
                        bool overlapping = false;
                        foreach (var tupleLongest in employeeVechicleTuplesLongest)
                        {
                            if (CalendarUtility.IsDatesOverlapping(tuple.Item3, tuple.Item4, tupleLongest.Item3, tupleLongest.Item4))
                                overlapping = true;
                        }

                        if (overlapping)
                            employeeVechicleTuplesOverlapping.Add(tuple);
                    }
                }
            }

            #endregion

            #region Calculate

            //Get EmployeeVehicles that is longest or overlaps
            List<EmployeeVehicle> employeeVehiclesLongest = employeeVechicles.Get(employeeVechicleTuplesLongest.Select(i => i.Item1));
            List<EmployeeVehicle> employeeVehiclesOverlapping = employeeVechicles.Get(employeeVechicleTuplesOverlapping.Select(i => i.Item1));

            //Calculate for all EmployeeVehicles that is longest or overlaps
            calculationDtos = CalculateEmployeeVehiclesAmounts(entities, employeeVehiclesLongest.Concat(employeeVehiclesOverlapping).ToList(), date);

            //Calculate dispense for all EmployeeVehicles that is longest
            List<EmployeeVehiclePayrollCalculationDTO> calculationDtosLongest = calculationDtos.Where(i => employeeVehiclesLongest.Exists(j => j.EmployeeVehicleId == i.EmployeeVehicleId)).ToList();
            if (calculationDtosLongest.Count > 1)
            {
                decimal dispenseTaxableValue = calculationDtosLongest.Sum(i => i.TaxableValue) / employeeVechicleTuplesLongest.Count;
                foreach (var calculationDto in calculationDtosLongest)
                {
                    calculationDto.TaxableValue = dispenseTaxableValue;
                }
            }

            #endregion

            return calculationDtos;
        }

        public List<EmployeeVehiclePayrollCalculationDTO> CalculateEmployeeVehiclesAmounts(CompEntities entities, List<EmployeeVehicle> employeeVehicles, DateTime date)
        {
            var dtos = new List<EmployeeVehiclePayrollCalculationDTO>();

            decimal baseAmount = PayrollManager.GetSysPayrollPriceAmount(entities, ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_BaseAmount, CalendarUtility.GetFirstDateOfYear(DateTime.Today));
            decimal governmentLoanInterest = PayrollManager.GetSysPayrollPriceAmount(entities, ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_GovernmentLoanInterest, CalendarUtility.GetFirstDateOfYear(DateTime.Today));

            foreach (EmployeeVehicle employeeVehicle in employeeVehicles)
            {
                if (dtos.Any(i => i.EmployeeVehicleId == employeeVehicle.EmployeeVehicleId))
                    continue;

                var dto = CalculateEmployeeVehiclesAmounts(employeeVehicle, date, baseAmount, governmentLoanInterest);
                if (dto != null)
                    dtos.Add(dto);
            }

            return dtos;
        }

        public EmployeeVehiclePayrollCalculationDTO CalculateEmployeeVehiclesAmounts(EmployeeVehicle employeeVehicle, DateTime date, decimal baseAmount, decimal governmentLoanInterest)
        {
            if (employeeVehicle == null)
                return null;


            EmployeeVehiclePayrollCalculationDTO dto = new EmployeeVehiclePayrollCalculationDTO()
            {
                EmployeeVehicleId = employeeVehicle.EmployeeVehicleId,
            };

            bool useCalculationAfter20210701 = employeeVehicle.RegisteredDate.HasValue && employeeVehicle.RegisteredDate.Value >= new DateTime(2021, 07, 01);
            bool useCalculationAfter20220701 = employeeVehicle.RegisteredDate.HasValue && employeeVehicle.RegisteredDate.Value >= new DateTime(2022, 07, 01);

            var employeeVehicleEquipments = employeeVehicle.EmployeeVehicleEquipment?.Where(i => i.CalculatedFromDate <= date && i.CalculatedToDate >= date && i.State == (int)SoeEntityState.Active).ToList() ?? new List<EmployeeVehicleEquipment>();
            var employeeVehicleDeductions = employeeVehicle.EmployeeVehicleDeduction?.Where(i => i.CalculatedFromDate <= date && i.State == (int)SoeEntityState.Active).ToList() ?? new List<EmployeeVehicleDeduction>();
            var employeeVehicleTaxes = employeeVehicle.EmployeeVehicleTax?.Where(i => i.CalculatedFromDate <= date && i.State == (int)SoeEntityState.Active).ToList() ?? new List<EmployeeVehicleTax>();

            decimal equipmentSum = employeeVehicleEquipments.Sum(eq => eq.Price);
            decimal deductionSum = employeeVehicleDeductions.Sum(ed => ed.Price);
            decimal taxAmount = employeeVehicleTaxes.Any() ? employeeVehicleTaxes.OrderBy(t => t.FromDate).Last().Amount : 0;

            decimal baseAmountMultiplier = Decimal.Multiply(baseAmount, 7.5M);
            decimal baseAmountPart = Decimal.Multiply(baseAmount, taxAmount > 0 ? 0.29M : 0.317M);

            decimal totalSum = (employeeVehicle.ComparablePrice != 0 && !useCalculationAfter20220701 ? employeeVehicle.ComparablePrice : employeeVehicle.Price) + equipmentSum + employeeVehicle.PriceAdjustment;
            decimal interestPart;
            decimal pricePart;
            decimal pricePart1;
            decimal pricePart2;

            // 220701 employeeVehicle.PriceAdjustment is adjusted according to the new rules see GUI

            if (useCalculationAfter20210701)
            {
                interestPart = Math.Floor(Decimal.Multiply(Decimal.Multiply(governmentLoanInterest, 0.7M) + Decimal.Divide(1, 100), totalSum));
                pricePart = Math.Floor(Decimal.Multiply(totalSum, 0.13M));
                pricePart1 = 0;
                pricePart2 = 0;
            }
            else
            {
                interestPart = Math.Floor(Decimal.Multiply(Decimal.Multiply(totalSum, 0.75M), governmentLoanInterest));
                pricePart = 0;
                pricePart1 = Math.Floor(Decimal.Multiply(totalSum > baseAmountMultiplier ? baseAmountMultiplier : totalSum, 0.09M));
                pricePart2 = Math.Floor(Decimal.Multiply(totalSum > baseAmountMultiplier ? totalSum - baseAmountMultiplier : 0, 0.2M));

            }

            decimal taxableValuePerYearSum = baseAmountPart + interestPart + pricePart + pricePart1 + pricePart2 + GetEcoCarDeduction(employeeVehicle.Price, employeeVehicle.ComparablePrice, totalSum, date, (TermGroup_SysVehicleFuelType)employeeVehicle.FuelType) + taxAmount;

            //Benefit Adjustment
            decimal benefitAdjustment = 0;
            decimal benefitValueAdjustment = employeeVehicle.BenefitValueAdjustment;

            if (benefitValueAdjustment == 0)
                benefitValueAdjustment = 100;

            benefitValueAdjustment = 100 - benefitValueAdjustment;

            if (benefitValueAdjustment > 0)
                benefitAdjustment = Math.Round(taxableValuePerYearSum * benefitValueAdjustment / 100, 2);

            //ExtensibleDriving
            decimal extensiveDrivingPart = 0;
            if (employeeVehicle.HasExtensiveDriving && taxableValuePerYearSum > 0)
                extensiveDrivingPart = Math.Floor(taxableValuePerYearSum * 0.25M);
            taxableValuePerYearSum = taxableValuePerYearSum - extensiveDrivingPart - benefitAdjustment;

            dto.TaxableValue = Math.Floor(Decimal.Divide(taxableValuePerYearSum, 12M) - deductionSum);
            dto.NetSalaryDeduction = Math.Floor(deductionSum);

            return dto;
        }

        private bool IsDeductableEcoCar(DateTime date, TermGroup_SysVehicleFuelType fuelType)
        {
            return (date.Year <= 2020 &&
                (fuelType == TermGroup_SysVehicleFuelType.Electricity ||
                fuelType == TermGroup_SysVehicleFuelType.PlugInHybrid ||
                fuelType == TermGroup_SysVehicleFuelType.Gas));
        }

        private decimal GetEcoCarDeduction(decimal price, decimal comparablePrice, decimal totalSum, DateTime date, TermGroup_SysVehicleFuelType fuelType)
        {
            // El- och laddhybridbilar, som kan laddas från elnätet, samt gasbilar (ej gasol) justeras först till en jämförbar bil utan miljöteknik.
            // Därefter sätts förmånsvärdet ner med 40 procent, max 16 000 kronor för inkomstår 2012-2016 och max 10 000 kronor från och med inkomstår 2017.
            // Detta gäller endast om bilen har ett nybilspris som är högre än närmast jämförbara bil.

            // Etanolbilar, elhybridbilar, som inte kan laddas från elnätet, och bilar som kan köras på gasol, rapsmetylester
            // samt övriga typer av miljöanpassade drivmedel justeras enbart ner till jämförbar bil.

            // Reglerna är tidsbegränsade och gäller till och med inkomståret 2020.

            decimal value = 0;

            if (price >= comparablePrice && IsDeductableEcoCar(date, fuelType))
            {
                value = Decimal.Multiply(totalSum, 0.4M);
                if (value > 10000)
                    value = 10000;
            }

            return -value;
        }

        #endregion

        #region Employment

        public List<EmploymentCalenderDTO> GetEmploymentCalenderDTOs(List<Employee> employees, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null, List<VacationGroup> vacationGroups = null, List<EmploymentTypeDTO> employmentTypes = null, List<AnnualLeaveGroup> annualLeaveGroups = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmploymentCalenderDTOs(entities, employees, dateFrom, dateTo, employeeGroups, payrollGroups, payrollPriceTypes, vacationGroups, employmentTypes, annualLeaveGroups);
        }

        public List<EmploymentCalenderDTO> GetEmploymentCalenderDTOs(CompEntities entities, List<Employee> employees, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null, List<VacationGroup> vacationGroups = null, List<EmploymentTypeDTO> employmentTypes = null, List<AnnualLeaveGroup> annualLeaveGroups = null)
        {
            List<EmploymentCalenderDTO> dtos = new List<EmploymentCalenderDTO>();

            foreach (Employee employee in employees)
            {
                dtos.AddRange(GetEmploymentCalenderDTOs(entities, employee, dateFrom, dateTo, employeeGroups, payrollGroups, payrollPriceTypes, vacationGroups, employmentTypes: employmentTypes, annualLeaveGroups: annualLeaveGroups));
            }

            return dtos;
        }

        public List<EmploymentCalenderDTO> GetEmploymentCalenderDTOs(Employee employee, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null, List<VacationGroup> vacationGroups = null, List<HolidayDTO> companyHolidays = null, List<DayType> dayTypesForCompany = null, bool ignoreEmploymentIfNotLoaded = false, List<EmploymentTypeDTO> employmentTypes = null, List<AnnualLeaveGroup> annualLeaveGroups = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmploymentCalenderDTOs(entities, employee, dateFrom, dateTo, employeeGroups, payrollGroups, payrollPriceTypes, vacationGroups, companyHolidays, dayTypesForCompany, ignoreEmploymentIfNotLoaded, employmentTypes, annualLeaveGroups);
        }

        public List<EmploymentCalenderDTO> GetEmploymentCalenderDTOs(CompEntities entities, Employee employee, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null, List<VacationGroup> vacationGroups = null, List<HolidayDTO> companyHolidays = null, List<DayType> dayTypesForCompany = null, bool ignoreEmploymentIfNotLoaded = false, List<EmploymentTypeDTO> employmentTypes = null, List<AnnualLeaveGroup> annualLeaveGroups = null)
        {
            List<EmploymentCalenderDTO> dtos = new List<EmploymentCalenderDTO>();

            var payrollGroupDTOs = payrollGroups.ToDTOs().ToList();
            var employeeGroupDTOs = employeeGroups.ToDTOs().ToList();
            var annualLeaveGroupDTOs = annualLeaveGroups.ToDTOs().ToList();

            DateTime currentDate = dateFrom;
            var firstEmployment = employee.GetFirstEmployment();
            var firstEmploymentType = firstEmployment?.GetEmploymentTypeDTO(employmentTypes, currentDate)?.Type ?? (int)TermGroup_EmploymentType.Unknown;
            var firstEmploymentDate = firstEmployment?.GetEmploymentDate();

            if (firstEmploymentDate.HasValue && firstEmploymentDate.Value > currentDate)
                currentDate = firstEmploymentDate.Value;

            if (currentDate < DateTime.Today.AddYears(-60))
                currentDate = DateTime.Today.AddYears(-60);

            if (dateTo > DateTime.Today.AddYears(20))
                dateTo = DateTime.Today.AddYears(20);

            var lastEmployment = employee.GetLastEmployment();

            if (lastEmployment.GetEndDate() != null && lastEmployment.GetEndDate() < dateFrom)
                return dtos;

            while (currentDate <= dateTo)
            {
                EmploymentCalenderDTO dto = new EmploymentCalenderDTO()
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeNr = employee.EmployeeNr,
                    EmployeeName = employee.Name,
                    Date = currentDate
                };

                if ((firstEmploymentDate.HasValue && firstEmploymentDate > currentDate) || currentDate <= CalendarUtility.DATETIME_DEFAULT)
                {
                    dto.EmploymentType = firstEmploymentType;
                }
                else if (employee.Employment != null || !ignoreEmploymentIfNotLoaded)
                {
                    var employment = employee.GetEmployment(currentDate);
                    if (employment != null)
                    {
                        dto.VacationGroupId = vacationGroups != null ? employment.GetVacationGroup(currentDate, vacationGroups)?.VacationGroupId : null;
                        var employmentDTO = employment?.ToDTO(employeeGroups: employeeGroups, payrollGroups: payrollGroups, vacationGroups: vacationGroups, payrollPriceTypes: payrollPriceTypes);
                        employmentDTO.ApplyEmploymentChanges(currentDate, employeeGroupDTOs, payrollGroupDTOs, annualLeaveGroupDTOs, employmentTypes, null);

                        var employmentType = employmentTypes.GetEmploymentType(employmentDTO.EmploymentType);
                        dto.EmploymentTypeId = employmentType?.EmploymentTypeId ?? 0;
                        dto.EmploymentType = employmentType?.Type ?? 0;
                        dto.EmploymentTypeName = employmentType?.Name ?? "";

                        dto.EmploymentId = employmentDTO.EmploymentId;
                        dto.EmployeeGroupId = employmentDTO.EmployeeGroupId;
                        dto.PayrollGroupId = employmentDTO.PayrollGroupId;
                        dto.AnnualLeaveGroupId = employmentDTO.AnnualLeaveGroupId;
                        dto.Percent = employmentDTO.Percent;

                        if (!companyHolidays.IsNullOrEmpty() && !dayTypesForCompany.IsNullOrEmpty())
                        {
                            var employeeGroup = employeeGroups.FirstOrDefault(f => f.EmployeeGroupId == employmentDTO.EmployeeGroupId);
                            var dayType = CalendarManager.GetDayType(currentDate, employeeGroup, companyHolidays, dayTypesForCompany);
                            dto.DayTypeId = dayType?.DayTypeId;
                            dto.DayTypeName = dayType?.Name;
                        }
                    }
                }

                dtos.Add(dto);

                if (currentDate <= CalendarUtility.DATETIME_DEFAULT && dtos.Any())
                {
                    var firstChange = employee.GetEmployment(currentDate)?.EmploymentChange.OrderBy(g => g.EmploymentChangeBatch?.FromDate).FirstOrDefault()?.EmploymentChangeBatch?.FromDate;
                    if (!firstChange.HasValue)
                        firstChange = DateTime.Today.AddYears(-40);

                    var newCurrent = currentDate.AddDays(1);
                    var lastDTO = dtos.Last();

                    while (newCurrent < firstChange)
                    {
                        currentDate = newCurrent;
                        var clone = lastDTO.CloneDTO();
                        clone.Date = newCurrent;
                        newCurrent = newCurrent.AddDays(1);
                        dtos.Add(clone);
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return dtos;
        }

        public List<Employment> GetEmployments(int actorCompanyId, List<int> employeeIds = null, bool onlyActiveEmployments = true, bool onlyActiveEmployees = true, bool loadEmploymentPriceType = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employment.NoTracking();
            return GetEmployments(entities, actorCompanyId, employeeIds, onlyActiveEmployments, onlyActiveEmployees, loadEmploymentPriceType);
        }

        public List<Employment> GetEmployments(CompEntities entities, int actorCompanyId, List<int> employeeIds = null, bool onlyActiveEmployments = true, bool onlyActiveEmployees = true, bool loadEmploymentPriceType = false, bool loadEmployee = false, bool loadContactPerson = false)
        {
            IQueryable<Employment> query = entities.Employment.Include("EmploymentChangeBatch.EmploymentChange");

            if (loadEmploymentPriceType)
                query = query.Include("EmploymentPriceType.EmploymentPriceTypePeriod");

            if (loadEmployee)
            {
                query = query.Include("Employee");
                if (loadContactPerson)
                    query = query.Include("Employee.ContactPerson");
            }

            var employments = (from em in query
                               .Include("OriginalPayrollGroup")
                               .Include("OriginalEmployeeGroup")
                               where em.ActorCompanyId == actorCompanyId &&
                               em.Employee.ActorCompanyId == actorCompanyId
                               select em);

            if (onlyActiveEmployments)
                employments = employments.Where(em => em.State == (int)SoeEntityState.Active);
            if (onlyActiveEmployees)
                employments = employments.Where(em => em.Employee.State == (int)SoeEntityState.Active);
            if (employeeIds != null)
                employments = employments.Where(em => employeeIds.Contains(em.EmployeeId));

            return employments.ToList();
        }

        public List<Employment> GetEmployments(int employeeId, int actorCompanyId, bool loadEmploymentVacationGroup = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employment.NoTracking();
            return GetEmployments(entities, employeeId, actorCompanyId, loadEmploymentVacationGroup);
        }

        public List<Employment> GetEmployments(CompEntities entities, int employeeId, int actorCompanyId, bool loadEmploymentVacationGroup = false)
        {
            IQueryable<Employment> query = (from e in entities.Employment
                                            .Include("EmploymentChangeBatch.EmploymentChange")
                                            where e.EmployeeId == employeeId &&
                                            e.ActorCompanyId == actorCompanyId &&
                                            e.State == (int)SoeEntityState.Active
                                            select e);

            if (loadEmploymentVacationGroup)
                query = query.Include("EmploymentVacationGroup.VacationGroup.VacationGroupSE");

            return query.ToList();
        }

        public Employment GetEmployment(int employmentId, int employeeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employment.NoTracking();
            return GetEmployment(entities, employmentId, employeeId, actorCompanyId);
        }

        public Employment GetEmployment(CompEntities entities, int employmentId, int employeeId, int actorCompanyId)
        {
            return (from e in entities.Employment
                    .Include("EmploymentChangeBatch.EmploymentChange")
                    where e.EmploymentId == employmentId &&
                    e.EmployeeId == employeeId &&
                    e.ActorCompanyId == actorCompanyId
                    select e).FirstOrDefault();
        }

        public Employment GetEmployment(int employmentId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Employment.NoTracking();
            return GetEmployment(entities, employmentId);
        }

        public Employment GetEmployment(CompEntities entities, int employmentId)
        {
            return (from e in entities.Employment
                    .Include("EmploymentChangeBatch.EmploymentChange")
                    where e.EmploymentId == employmentId
                    select e).FirstOrDefault();
        }

        public bool IsEmploymentValid(Employment employment, List<Employment> allEmployments)
        {
            if (employment == null || allEmployments == null || allEmployments.Count == 0)
                return false;

            bool hasFrom = employment.DateFrom.HasValue;
            bool hasTo = employment.DateTo.HasValue;
            bool hasOnlyFrom = hasFrom && !hasTo;
            bool hasOnlyTo = !hasFrom && hasTo;
            bool hasEmploymentAfter = allEmployments.Any(i => i.EmploymentId != employment.EmploymentId && (i.DateFrom > employment.DateFrom || i.DateTo > employment.DateFrom));

            //Employment CANNOT have DateTo without DateFrom. Employment CAN have DateFrom without DateTo
            if (hasOnlyTo)
                return false;

            //Employment CANNOT have empty DateTo if another later Employent exists
            if (hasOnlyFrom && hasEmploymentAfter)
                return false;

            return true;
        }

        public bool IsEmploymentValidToKeepPriceTypes(Employment prevEmployment, EmploymentDTO newEmployment)
        {
            if (prevEmployment == null || newEmployment == null)
                return false;

            DateTime? prevDate = prevEmployment.DateTo ?? newEmployment.DateFrom?.AddDays(-1);
            if (!prevDate.HasValue)
                return false;

            if (!Int32.Equals(prevEmployment.GetEmployeeGroupId(prevDate.Value), newEmployment.EmployeeGroupId))
                return false;
            if (!Nullable.Equals(prevEmployment.GetPayrollGroupId(prevDate.Value), newEmployment.PayrollGroupId))
                return false;
            if (!Nullable.Equals(prevEmployment.EmploymentVacationGroup.GetLastVacationGroupId(), newEmployment.EmploymentVacationGroup.GetLastVacationGroupId()))
                return false;

            return true;
        }

        public int GetAnnualWorkTimeMinutes(DateTime startDate, DateTime stopDate, int employeeId, int actorCompanyId, List<int> employeeGroupsWithPlanningPeriods = null, int? clockRounding = null, int? timePeriodHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAnnualWorkTimeMinutes(entities, startDate, stopDate, employeeId, actorCompanyId, employeeGroupsWithPlanningPeriods, clockRounding, timePeriodHeadId);
        }

        public int GetAnnualWorkTimeMinutes(CompEntities entities, DateTime startDate, DateTime stopDate, int employeeId, int actorCompanyId, List<int> employeeGroupsWithPlanningPeriods = null, int? clockRounding = null, int? timePeriodHeadId = null)
        {
            Employee employee = GetEmployee(entities, employeeId, actorCompanyId, loadEmployment: true);
            return GetAnnualWorkTimeMinutes(entities, startDate, stopDate, employee, employeeGroupsWithPlanningPeriods, clockRounding, timePeriodHeadId);
        }

        public int GetAnnualWorkTimeMinutes(DateTime startDate, DateTime stopDate, Employee employee, List<int> employeeGroupsWithPlanningPeriods = null, int? clockRounding = null, int? timePeriodHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAnnualWorkTimeMinutes(entities, startDate, stopDate, employee, employeeGroupsWithPlanningPeriods, clockRounding, timePeriodHeadId);
        }

        public int GetAnnualWorkTimeMinutes(CompEntities entities, DateTime startDate, DateTime stopDate, Employee employee, List<int> employeeGroupsWithPlanningPeriods = null, int? clockRounding = null, int? timePeriodHeadId = null)
        {
            if (employee == null)
                return 0;

            if (employeeGroupsWithPlanningPeriods == null)
                employeeGroupsWithPlanningPeriods = TimePeriodManager.GetEmployeeGroupsWithRuleWorkTimePeriods(entities, employee.ActorCompanyId, startDate, stopDate);
            if (!clockRounding.HasValue)
                clockRounding = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningClockRounding, 0, employee.ActorCompanyId, 0);

            decimal minutes = 0;
            bool employeeGroupUsed = false;

            startDate = CalendarUtility.GetBeginningOfDay(startDate);
            stopDate = CalendarUtility.GetBeginningOfDay(stopDate);

            Dictionary<DateTime, int> employeeGroupIds = GetEmployeeGroupIdsInRange(employee, startDate, stopDate);

            // If emp only have one employment for specified year and it ranges over the whole year,
            // with no modifications made to emp group or work time,
            // use the work time from the emp group directly
            List<Employment> employments = employee.GetEmployments(startDate, stopDate);
            if (employments.Count == 1)
            {
                // Only one employment, check default range
                Employment employment = employments[0];
                if ((!employment.DateFrom.HasValue || employment.DateFrom.Value <= startDate) && (!employment.DateTo.HasValue || employment.DateTo.Value >= stopDate))
                {
                    // Check modifications
                    List<EmploymentChange> employmentChanges = employment.GetAllChanges();
                    if (!employmentChanges.Any(ec =>
                        ec.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmployeeGroupId ||
                        ec.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek ||
                        ec.FieldType == (int)TermGroup_EmploymentChangeFieldType.Percent))
                    {
                        int employeeGroupId = employeeGroupIds.ContainsKey(startDate) ? employeeGroupIds[startDate] : 0;
                        if (employeeGroupId != 0)
                        {
                            if (employeeGroupsWithPlanningPeriods != null && employeeGroupsWithPlanningPeriods.Contains(employeeGroupId))
                            {
                                minutes = TimePeriodManager.GetEmployeeGroupRuleWorkTime(entities, employee.ActorCompanyId, employeeGroupId, startDate, timePeriodHeadId);
                                decimal percent = employment.CalculatePercent(startDate);
                                minutes = minutes * decimal.Divide(percent, 100);
                            }
                            else
                            {
                                minutes = employment.GetWorkTimeWeek(startDate);
                                decimal nbrOfWeeks = decimal.Divide((decimal)(stopDate - startDate).TotalDays + 1, 7);
                                minutes *= nbrOfWeeks;
                            }

                            employeeGroupUsed = true;
                        }
                    }
                }
            }

            if (!employeeGroupUsed)
            {
                int prevEmployeeGroupId = 0;
                Dictionary<DateTime, decimal> workTimePeriods = null;

                DateTime date = startDate;
                int employeeGroupId = 0;
                while (date <= stopDate)
                {
                    if (employeeGroupIds.ContainsKey(date))
                        employeeGroupId = employeeGroupIds[date];

                    if (employeeGroupId != 0)
                    {
                        Employment employment = employee.GetEmployment(date);
                        if (employment != null)
                        {
                            #region Calculate for day

                            decimal minutesPerDay = 0;
                            if (!employeeGroupsWithPlanningPeriods.IsNullOrEmpty() && employeeGroupsWithPlanningPeriods.Contains(employeeGroupId))
                            {
                                if (prevEmployeeGroupId == 0 || prevEmployeeGroupId != employeeGroupId)
                                {
                                    workTimePeriods = TimePeriodManager.GetEmployeeGroupRuleWorkTimes(employee.ActorCompanyId, employeeGroupId, date, stopDate, timePeriodHeadId);
                                    prevEmployeeGroupId = employeeGroupId;
                                }

                                // Calculate number of work minutes per day for current emp group
                                if (workTimePeriods != null && workTimePeriods.ContainsKey(date))
                                    minutesPerDay = workTimePeriods[date];

                                decimal percent = employment.CalculatePercent(date);
                                minutesPerDay = minutesPerDay * Decimal.Divide(percent, 100);
                            }
                            else
                            {
                                minutesPerDay = Decimal.Divide(employment.GetWorkTimeWeek(date), 7);
                            }
                            minutes += minutesPerDay;

                            #endregion
                        }
                    }

                    date = date.AddDays(1);
                }
            }

            // Rounding
            minutes = Decimal.Round(minutes);
            if (clockRounding.Value > 0)
                minutes = ((int)minutes).RoundTime(clockRounding.Value);

            return (int)minutes;
        }

        public List<AnnualScheduledTimeSummary> GetScheduledTimeSummary(int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo, int? timePeriodHeadId = null)
        {
            List<AnnualScheduledTimeSummary> sum = new List<AnnualScheduledTimeSummary>();

            List<Employee> employees = GetAllEmployeesByIds(actorCompanyId, employeeIds, loadEmployment: true);
            if (employees.IsNullOrEmpty())
                return sum;

            List<int> employeeGroupsWithPlanningPeriods = TimePeriodManager.EmployeeGroupsWithRuleWorkTimePeriods(actorCompanyId, dateFrom, dateTo);

            // Rounding
            int clockRounding = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningClockRounding, 0, actorCompanyId, 0);

            foreach (Employee employee in employees)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                int annualScheduledTimeMinutes = TimeScheduleManager.GetScheduledTimeSummaryTotalWithinEmployments(entitiesReadOnly, actorCompanyId, employee, dateFrom, dateTo, TimeScheduledTimeSummaryType.Both);
                int annualWorkTimeMinutes = GetAnnualWorkTimeMinutes(dateFrom, dateTo, employee, employeeGroupsWithPlanningPeriods, clockRounding, timePeriodHeadId);

                sum.Add(new AnnualScheduledTimeSummary()
                {
                    EmployeeId = employee.EmployeeId,
                    AnnualScheduledTimeMinutes = annualScheduledTimeMinutes,
                    AnnualWorkTimeMinutes = annualWorkTimeMinutes
                });
            }

            return sum;
        }


        public List<Tuple<int, int, int>> GetCyclePlannedMinutes(int? timeScheduleScenarioHeadId, DateTime date, List<int> employeeIds)
        {
            List<Tuple<int, int, int>> result = new List<Tuple<int, int, int>>();

            DateTime startDate = date.Date;
            DateTime stopDate = CalendarUtility.GetEndOfDay(startDate.AddDays(6));
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeScheduleTemplateBlock.NoTracking();
            entitiesReadOnly.TimeScheduleTemplatePeriod.NoTracking();
            entitiesReadOnly.TimeScheduleTemplateHead.NoTracking();

            foreach (int employeeId in employeeIds)
            {
                int minutes = 0;
                int noOfDays = 1;

                var list = (from tb in entitiesReadOnly.TimeScheduleTemplateBlock
                                                                 .Include("TimeScheduleTemplatePeriod.TimeScheduleTemplateHead")
                            where tb.EmployeeId == employeeId &&
                            tb.Date >= startDate && tb.Date <= stopDate &&
                            tb.StartTime != tb.StopTime &&
                            tb.State == (int)SoeEntityState.Active
                            orderby tb.Date
                            select tb).ToList();

                TimeScheduleTemplateBlock firstBlock = list.FirstOrDefault(tb => timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue);

                if (firstBlock != null)
                {
                    int dayNumber = firstBlock.TimeScheduleTemplatePeriod.DayNumber;
                    noOfDays = firstBlock.TimeScheduleTemplatePeriod.TimeScheduleTemplateHead.NoOfDays;

                    DateTime cycleStartDate = startDate.AddDays(-(dayNumber - 1));
                    DateTime cycleStopDate = CalendarUtility.GetEndOfDay(cycleStartDate.AddDays(noOfDays - 1));

                    List<TimeScheduleTemplateBlock> blocks = (from tb in entitiesReadOnly.TimeScheduleTemplateBlock
                                                              where tb.EmployeeId == employeeId &&
                                                              tb.Date >= cycleStartDate && tb.Date <= cycleStopDate &&
                                                              tb.StartTime != tb.StopTime &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

                    blocks = blocks.FilterScenario(timeScheduleScenarioHeadId);

                    foreach (TimeScheduleTemplateBlock block in blocks)
                    {
                        if (block.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None)
                            minutes += (int)(block.StopTime - block.StartTime).TotalMinutes;    // Shift
                        else
                            minutes -= (int)(block.StopTime - block.StartTime).TotalMinutes;    // Break
                    }
                }

                if (noOfDays == 0)
                    noOfDays = 7;

                // Create cycle from average to make rounding correct
                int average = (minutes * 7 / noOfDays);
                int cycle = average * noOfDays / 7;
                Tuple<int, int, int> item = new Tuple<int, int, int>(employeeId, cycle, average);

                result.Add(item);
            }

            return result;
        }

        private ActionResult ValidateTemporaryPrimaryEmploymentWholeInterval(Employee employee)
        {
            ActionResult result = new ActionResult(true);
            if (employee == null || !employee.Employment.ContainsTemporaryPrimary())
                return result;

            foreach (Employment employment in employee.Employment.GetTemporaryPrimary())
            {
                List<DateRangeDTO> hibernatingPeriods = employee.GetHibernatingPeriods(employment.GetDateFromOrMin(), employment.GetDateToOrMax());
                if (employment.Days > hibernatingPeriods.Sum(d => d.Days))
                {
                    result = new ActionResult((int)ActionResultSave.TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval);
                    TimeHibernatingManager.TrySetHibernatingErrorMessage(ref result);
                    return result;
                }
            }

            return result;
        }

        public ActionResult ValidateTemporaryPrimaryEmployment(CompEntities entities, Employee employee, Employment employment, EmploymentDTO employmentInput, bool doAcceptAttestedTemporaryEmployments)
        {
            if (employee == null || employment == null || employmentInput == null || !employment.IsNew())
                return new ActionResult(true);

            var result = employee.GetActiveEmployments().ValidateTemporaryPrimaryEmployment(employment.DateFrom, employment.DateTo, employmentInput.IsSecondaryEmployment);
            if (!result.Success)
                return result;

            List<TimePayrollTransaction> timePayrollTransactions = TimeTransactionManager.GetTimePayrollTransactionsForEmployee(entities, employee.EmployeeId, employment.GetDateFromOrMin(), employment.GetDateToOrMax());
            if (timePayrollTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            var payrollLockedAttestStateIds = TimeTransactionManager.GetPayrollLockedAttestStateIds(entities);
            if (timePayrollTransactions.Select(t => t.AttestStateId).IsEqualToAny(payrollLockedAttestStateIds))
                return new ActionResult((int)ActionResultSave.TemporaryPrimaryExistsLockedTransactions);

            if (!doAcceptAttestedTemporaryEmployments)
            {
                int initialAttestStateId = AttestManager.GetInitialAttestStateId(entities, base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
                if (initialAttestStateId > 0 && timePayrollTransactions.Any(t => t.AttestStateId != initialAttestStateId))
                    return new ActionResult((int)ActionResultSave.TemporaryPrimaryExistsAttestedTransactions);
            }

            return new ActionResult(true);
        }

        public ActionResult SaveEmployments(CompEntities entities, TransactionScope transaction, Employee employee, List<EmploymentDTO> employmentInputs, bool generateCurrentChanges = false, bool doAcceptAttestedTemporaryEmployments = false, bool doDeleteVacationGroups = true)
        {
            ActionResult result = null;

            if (employmentInputs == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmploymentInputs");

            try
            {
                result = ValidateEmployments(employmentInputs);
                if (!result.Success)
                    return result;

                DateTime batch = DateTime.Now;
                employmentInputs.EnsureDateFrom();

                if (!employee.Employment.IsLoaded && CanEntityLoadReferences(entities, employee))
                    employee.Employment.Load();

                result = UpdateEmployments(entities, employee, employmentInputs, batch, generateCurrentChanges, doDeleteVacationGroups);
                if (!result.Success)
                    return result;

                result = AddEmployments(entities, employee, employmentInputs, batch, doAcceptAttestedTemporaryEmployments);
                if (!result.Success)
                    return result;

                result = ValidateTemporaryPrimaryEmploymentWholeInterval(employee);
                if (!result.Success)
                    return result;

                result = SaveChanges(entities, transaction);
            }
            finally
            {
                if (result?.Success == false)
                    TimeHibernatingManager.TrySetHibernatingErrorMessage(ref result);
            }

            return result;
        }

        public Employment SaveEmployment(CompEntities entities, Employee employee, EmployeeGroup employeeGroup, PayrollGroup payrollGroup = null, VacationGroup vacationGroup = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? employmentType = null, int? workTimeWeek = null, decimal? workPercentage = null, int? experienceMonths = null, bool? experienceAgreedOrEstablished = null, string workTasks = null, string workPlace = null, bool doNotModifyWithEmpty = false, List<EmployeeGroup> employeeGroups = null)
        {
            if (employee == null)
                return null;

            Employment employment = employee.GetLastEmployment();
            if (employment == null)
                AddEmploymentIfNotExists(entities, dateFrom, dateTo, employee, employeeGroup, payrollGroup, vacationGroup, employmentType, workTimeWeek, workPercentage, experienceMonths, experienceAgreedOrEstablished, workTasks, workPlace);
            else
                SetEmploymentOriginalValuesIfEmpty(employment, employeeGroup ?? employment.GetEmployeeGroup(DateTime.Today, employeeGroups), payrollGroup, vacationGroup, employmentType: employmentType, workTimeWeek: workTimeWeek, workPercentage: workPercentage, experienceMonths: experienceMonths, experienceAgreedOrEstablished: experienceAgreedOrEstablished, workPlace: workPlace, doNotModifyWithEmpty: doNotModifyWithEmpty);
            return employment;
        }

        public Employment AddEmploymentIfNotExists(CompEntities entities, DateTime? dateFrom, DateTime? dateTo, Employee employee, EmployeeGroup employeeGroup, PayrollGroup payrollGroup = null, VacationGroup vacationGroup = null, int? employmenType = null, int? workTimeWeek = null, decimal? workPercentage = null, int? experienceMonths = null, bool? experienceAgreedOrEstablished = null, string workTasks = null, string workPlace = null)
        {
            if (employee == null || employeeGroup == null)
                return null;

            Employment employment = dateFrom.HasValue && dateTo.HasValue ? employee.GetEmployment(dateFrom.Value, dateTo.Value) : employee.GetEmployment(dateFrom);
            if (employment == null)
            {
                employment = Employment.Create(employee, dateFrom, dateTo, GetUserDetails());
                SetEmploymentOriginalValues(employment, employeeGroup, payrollGroup, vacationGroup, employmenType, workTimeWeek, workPercentage, experienceMonths, experienceAgreedOrEstablished, workTasks, workPlace);
                entities.Employment.AddObject(employment);
            }
            return employment;
        }

        #region Help-methods

        private void CreateEmploymentChanges(Employee employee, Employment employment, EmploymentDTO employmentInput, DateTime batchCreated)
        {
            if (employment == null || employmentInput?.CurrentChanges == null)
                return;

            //Data changes that only cause EmploymentChanges
            List<EmploymentChangeDTO> inputChanges = employmentInput.CurrentChanges.Where(change => change.FromDate.HasValue || !change.DoRequireDate()).ToList();
            if (employment.EmploymentId == 0)
                inputChanges = inputChanges.Where(i => i.FromDate > employment.DateFrom).ToList();
            if (!inputChanges.Any())
                return;

            //Only save changes on new Employment if different dates exists (should only be from API)
            foreach (var inputChangesByDate in inputChanges.GroupBy(change => change.FromToDescription).ToList())
            {
                EmploymentChangeDTO firstChange = inputChangesByDate.FirstOrDefault();
                if (firstChange == null)
                    continue;

                EmploymentChangeBatch batch = EmploymentChangeBatch.Create(employee, employment, firstChange.FromDate, firstChange.ToDate, firstChange.Comment, GetUserDetails(), batchCreated);
                foreach (EmploymentChangeDTO inputChange in inputChangesByDate)
                {
                    EmploymentChange.Create(employee, employment, batch, inputChange);
                }
            }
        }

        private void SetEmploymentOriginalValues(CompEntities entities, Employee employee, Employment employment, EmploymentDTO employmentInput)
        {
            if (employee == null || employment == null || employmentInput == null)
                return;

            employment.SetOriginalValues(
                state: employmentInput.State,
                employeeGroup: GetEmployeeGroupsFromCache(entities, CacheConfig.Company(employee.ActorCompanyId))?.Find(eg => eg.EmployeeGroupId == employmentInput.EmployeeGroupId),
                payrollGroupId: employmentInput.PayrollGroupId,
                employmentType: employmentInput.EmploymentType,
                baseWorkTimeWeek: employmentInput.BaseWorkTimeWeek,
                workTimeWeek: employmentInput.WorkTimeWeek,
                workPercentage: employmentInput.Percent,
                name: employmentInput.Name,
                experienceMonths: employmentInput.ExperienceMonths,
                experienceAgreedOrEstablished: employmentInput.ExperienceAgreedOrEstablished,
                workTasks: employmentInput.WorkTasks,
                workPlace: employmentInput.WorkPlace,
                specialConditions: employmentInput.SpecialConditions,
                substituteFor: employmentInput.SubstituteFor,
                substituteForDueTo: employmentInput.SubstituteForDueTo,
                endReason: employmentInput.EmploymentEndReason,
                externalCode: employmentInput.ExternalCode,
                isSecondaryEmployment: employmentInput.IsSecondaryEmployment,
                isFixedAccounting: employmentInput.FixedAccounting,
                updateExperienceMonthsReminder: employmentInput.UpdateExperienceMonthsReminder,
                finalSalaryStatus: employmentInput.FinalSalaryStatus,
                fullTimeWorkTimeWeek: employmentInput.FullTimeWorkTimeWeek,
                excludeFromWorkTimeWeekCalculationOnSecondaryEmployment: employmentInput.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment,
                annualLeaveGroupId: employmentInput.AnnualLeaveGroupId
            );
        }

        private void SetEmploymentOriginalValues(Employee employee, Employment employment, Employment employmentPrototype, DateTime date, SoeEntityState? state = null)
        {
            if (employee == null || employment == null || employmentPrototype == null)
                return;

            employment.SetOriginalValues(
                state: state ?? (SoeEntityState)employmentPrototype.State,
                employeeGroup: employmentPrototype.GetEmployeeGroup(date, GetEmployeeGroupsFromCache(employee.ActorCompanyId)),
                payrollGroupId: employmentPrototype.GetPayrollGroupId(date),
                employmentType: employmentPrototype.GetEmploymentType(date),
                baseWorkTimeWeek: employmentPrototype.GetBaseWorkTimeWeek(date),
                workTimeWeek: employmentPrototype.GetWorkTimeWeek(date),
                workPercentage: employmentPrototype.GetPercent(date),
                name: employmentPrototype.GetName(date),
                experienceMonths: employmentPrototype.GetExperienceMonths(date),
                experienceAgreedOrEstablished: employmentPrototype.GetExperienceAgreedOrEstablished(date),
                workTasks: employmentPrototype.GetWorkTasks(date),
                workPlace: employmentPrototype.GetWorkPlace(date),
                specialConditions: employmentPrototype.GetSpecialConditions(date),
                substituteFor: employmentPrototype.GetSubstituteFor(date),
                substituteForDueTo: employmentPrototype.GetSubstituteForDueTo(date),
                endReason: employmentPrototype.GetEndReason(date),
                externalCode: employmentPrototype.GetExternalCode(date),
                isSecondaryEmployment: employmentPrototype.IsSecondaryEmployment,
                isFixedAccounting: employmentPrototype.FixedAccounting,
                updateExperienceMonthsReminder: employmentPrototype.UpdateExperienceMonthsReminder,
                finalSalaryStatus: (SoeEmploymentFinalSalaryStatus)employmentPrototype.FinalSalaryStatus,
                fullTimeWorkTimeWeek: employmentPrototype.GetFullTimeWorkTimeWeek(date),
                excludeFromWorkTimeWeekCalculationOnSecondaryEmployment: employmentPrototype.GetExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(date),
                annualLeaveGroupId: employmentPrototype.GetAnnualLeaveGroupId(date)
            );

            int? vacationGroupId = employmentPrototype.EmploymentVacationGroup?.FirstOrDefault()?.VacationGroupId;
            if (vacationGroupId.HasValue)
                EmploymentVacationGroup.Create(employment, vacationGroupId.Value, employment.DateFrom, GetUserDetails());
        }

        private void SetEmploymentOriginalValues(Employment employment, EmployeeGroup employeeGroup, PayrollGroup payrollGroup, VacationGroup vacationGroup, int? employmentType = null, int? workTimeWeek = null, decimal? workPercentage = null, int? experienceMonths = null, bool? experienceAgreedOrEstablished = null, string workTasks = null, string workPlace = null)
        {
            if (employment == null || employeeGroup == null)
                return;

            employment.SetOriginalValues(
                state: SoeEntityState.Active,
                employeeGroup: employeeGroup,
                payrollGroupId: payrollGroup?.PayrollGroupId,
                employmentType: employmentType,
                workTimeWeek: workTimeWeek,
                workPercentage: workPercentage,
                experienceMonths: experienceMonths,
                experienceAgreedOrEstablished: experienceAgreedOrEstablished,
                workTasks: workTasks,
                workPlace: workPlace
            );
            if (vacationGroup != null)
                EmploymentVacationGroup.Create(employment, vacationGroup.VacationGroupId, employment.DateFrom, GetUserDetails());
        }

        private void SetEmploymentOriginalValuesIfEmpty(Employment employment, EmployeeGroup employeeGroup, PayrollGroup payrollGroup, VacationGroup vacationGroup, int? employmentType = null, int? baseWorkTimeWeek = null, int? workTimeWeek = null, decimal? workPercentage = null, int? experienceMonths = null, bool? experienceAgreedOrEstablished = null, string workPlace = null, bool doNotModifyWithEmpty = false)
        {
            if (employment == null || employeeGroup == null)
                return;

            employment.SetOriginalValuesIfEmpty(employeeGroup, payrollGroup, vacationGroup, employmentType, baseWorkTimeWeek, workTimeWeek, workPercentage, experienceMonths, experienceAgreedOrEstablished, workPlace, forceExperienceMonths: !doNotModifyWithEmpty);
            if (vacationGroup != null && GetEmploymentVacationGroups(employment.EmploymentId).IsNullOrEmpty())
                EmploymentVacationGroup.Create(employment, vacationGroup.VacationGroupId, employment.DateFrom, GetUserDetails());
        }

        private ActionResult HandleEmploymentDateChanges(CompEntities entities, Employee employee, Employment employment, EmploymentDTO employmentInput, EmploymentDateChange employmentDateChange, DateTime batchCreated)
        {
            if (employmentDateChange == null)
                return new ActionResult(true);
            if (employee == null || employment == null || employmentInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            ActionResult result = new ActionResult(true);

            if (employment.IsTemporaryPrimary && employmentDateChange.Type != SoeEmploymentDateChangeType.None && !FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment_CreateDeleteShortenExtendHibernating, Permission.Modify, this.RoleId, this.ActorCompanyId, this.LicenseId))
                return new ActionResult((int)ActionResultSave.TemporaryPrimaryHasNoPerissionToCreateDeleteShortenExtend);

            switch (employmentDateChange.Type)
            {
                case SoeEmploymentDateChangeType.Delete:
                    if (employment.IsTemporaryPrimary)
                    {
                        result = TimeHibernatingManager.ShortenHibernatingAbsence(out TimeHibernatingAbsenceHead hibernatingHead, employee, employment, batchCreated, true);
                        if (result.Success)
                            result = CreateEmploymenChangeOnHibernatingEmployment(entities, employee, employment, hibernatingHead?.TimeDeviationCauseId, null, null, null, GetText(2244, "Borttagen"), batchCreated);
                    }
                    break;
                case SoeEmploymentDateChangeType.ShortenStart:
                    UpdateEmploymentChangeBatchesFromDate();
                    if (employment.IsTemporaryPrimary)
                    {
                        result = TimeHibernatingManager.ShortenHibernatingAbsence(out TimeHibernatingAbsenceHead hibernatingHead, employee, employment, batchCreated, false);
                        if (result.Success)
                            result = CreateEmploymenChangeOnHibernatingEmployment(entities, employee, employment, hibernatingHead?.TimeDeviationCauseId, employmentDateChange.BeforeDateFrom, employmentDateChange.AfterDateFrom, null, GetText(91970, "Förkortad"), batchCreated);
                    }
                    break;
                case SoeEmploymentDateChangeType.ShortenStop:
                    UpdateEmploymentChangeBatchesToDate();
                    if (employment.IsTemporaryPrimary)
                    {
                        result = TimeHibernatingManager.ShortenHibernatingAbsence(out TimeHibernatingAbsenceHead hibernatingHead, employee, employment, batchCreated, false);
                        if (result.Success)
                            result = CreateEmploymenChangeOnHibernatingEmployment(entities, employee, employment, hibernatingHead?.TimeDeviationCauseId, employmentDateChange.BeforeDateTo, employmentDateChange.AfterDateTo, null, GetText(91970, "Förkortad"), batchCreated);
                    }
                    break;
                case SoeEmploymentDateChangeType.ExtendStop:
                    UpdateEmploymentChangeBatchesToDate();
                    if (employment.IsTemporaryPrimary)
                    {
                        result = TimeHibernatingManager.ExtendHibernatingAbsence(entities, out TimeHibernatingAbsenceHead hibernatingHead, employmentDateChange, employee, employment, batchCreated);
                        if (result.Success)
                            result = CreateEmploymenChangeOnHibernatingEmployment(entities, employee, employment, hibernatingHead?.TimeDeviationCauseId, employmentDateChange.BeforeDateTo, employmentDateChange.AfterDateTo, null, GetText(91971, "Förlängd"), batchCreated);
                    }
                    break;
            }

            void UpdateEmploymentChangeBatchesFromDate()
            {
                List<EmploymentChangeBatch> changeBatches = GetEmploymentChangeBatchesWithFromDate(entities, employee.EmployeeId, employment.EmploymentId, employmentDateChange.BeforeDateFrom);
                if (changeBatches.IsNullOrEmpty())
                    return;

                changeBatches.UpdateDateFrom(employmentDateChange.AfterDateFrom);
            }

            void UpdateEmploymentChangeBatchesToDate()
            {
                if (!employmentDateChange.BeforeDateTo.HasValue && employmentDateChange.AfterDateTo.HasValue)
                    return;

                List<EmploymentChangeBatch> changeBatches = GetEmploymentChangeBatchesWithToDate(entities, employee.EmployeeId, employment.EmploymentId, employmentDateChange.BeforeDateTo.Value);
                if (changeBatches.IsNullOrEmpty())
                    return;

                changeBatches.UpdateDateTo(employmentDateChange.AfterDateTo);
            }

            return result;
        }

        private ActionResult CloseTemporaryPrimaryEmployment(Employee employee, Employment temporaryPrimaryEmployment, EmploymentDTO employmentInput, DateTime batchCreated)
        {
            if (employmentInput == null || !employmentInput.IsTemporaryPrimary || !employmentInput.IsChangingToNotTemporary || !employmentInput.CurrentChangeDateFrom.HasValue)
                return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(92010, "Kunde inte ändra till ordinarie anställning"));

            DateTime newDateTo = employmentInput.CurrentChangeDateFrom.Value.AddDays(-1);

            if (temporaryPrimaryEmployment.TryShortenStop(newDateTo))
            {
                ActionResult result = TimeHibernatingManager.ShortenHibernatingAbsence(out _, employee, temporaryPrimaryEmployment, batchCreated, false);
                if (!result.Success)
                    return result;
            }

            employee.GetHibernatingEmployments(temporaryPrimaryEmployment).ForEach(hibernatingEmployment => hibernatingEmployment.TryShortenStop(newDateTo));

            Employment newPrimaryEmployment = Employment.Create(employee, employmentInput.CurrentChangeDateFrom, employmentInput.CurrentChangeDateTo, GetUserDetails(), batchCreated);
            SetEmploymentOriginalValues(employee, newPrimaryEmployment, temporaryPrimaryEmployment, temporaryPrimaryEmployment.DateTo.Value, SoeEntityState.Active);

            return new ActionResult(true);
        }

        private ActionResult CreateTemporaryPrimaryAndHibernatingEmployment(CompEntities entities, Employee employee, Employment employment, EmploymentDTO employmentInput, DateTime batchCreated, bool doAcceptAttestedTemporaryEmployments = false)
        {
            if (employee == null || employment == null || employmentInput == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound);

            employment.IsTemporaryPrimary = true;

            var result = ValidateTemporaryPrimaryEmployment(entities, employee, employment, employmentInput, doAcceptAttestedTemporaryEmployments);
            if (!result.Success)
                return result;

            result = CreateEmploymenChangeOnHibernatingEmployment(entities, employee, employment, employmentInput.HibernatingTimeDeviationCauseId, null, null, GetText(5714, "Nej"), GetText(5713, "Ja"), batchCreated);
            if (!result.Success)
                return result;

            return TimeHibernatingManager.CreateTimeHibernatingAbsence(entities, employee, employment, employmentInput.HibernatingTimeDeviationCauseId, batchCreated);
        }

        private ActionResult CreateEmploymenChangeOnHibernatingEmployment(CompEntities entities, Employee employee, Employment temporaryPrimaryEmployment, int? timeDeviationCauseId, DateTime? dateFrom, DateTime? dateTo, string fromValue, string toValue, DateTime? batchCreated)
        {
            List<Employment> hibernatingEmployments = employee.GetHibernatingEmployments(temporaryPrimaryEmployment);
            if (hibernatingEmployments.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval);

            TimeDeviationCause timeDeviationCause = timeDeviationCauseId.HasValue ? TimeDeviationCauseManager.GetTimeDeviationCause(entities, timeDeviationCauseId.Value, employee.ActorCompanyId) : null;

            foreach (Employment hibernatingEmployment in hibernatingEmployments)
            {
                string currrentFromValue = fromValue;
                string currrentToValue = toValue;

                if (dateFrom.HasValue)
                {
                    if (dateFrom.Value == hibernatingEmployment.DateTo)
                        continue;

                    DateTime currentDateFrom = CalendarUtility.GetLatestDate(hibernatingEmployment.GetDateFromOrMin(), dateFrom.Value);
                    if (!currrentFromValue.IsNullOrEmpty())
                        currrentFromValue = $"{currentDateFrom.ToShortDateString()} ({currrentFromValue})";
                    else
                        currrentFromValue = currentDateFrom.ToShortDateString();
                }

                if (dateTo.HasValue)
                {
                    DateTime currentDateTo = CalendarUtility.GetEarliestDate(hibernatingEmployment.GetDateToOrMax(), dateTo.Value);
                    if (!currrentToValue.IsNullOrEmpty())
                        currrentToValue = $"{currentDateTo.ToShortDateString()} ({currrentToValue})";
                    else
                        currrentToValue = currentDateTo.ToShortDateString();
                }

                DateTime batchFromDate = CalendarUtility.GetLatestDate(hibernatingEmployment.GetDateFromOrMin(), temporaryPrimaryEmployment.GetDateFromOrMin());
                DateTime batchToDate = CalendarUtility.GetEarliestDate(hibernatingEmployment.GetDateToOrMax(), temporaryPrimaryEmployment.GetDateToOrMax());

                AddEmploymentChangeAndBatch(
                    TermGroup_EmploymentChangeType.Information, TermGroup_EmploymentChangeFieldType.Hibernating,
                    hibernatingEmployment,
                    batchFromDate, batchToDate,
                    timeDeviationCause?.CodeAndName,
                    batchCreated,
                    currrentFromValue, currrentToValue);
            }

            return new ActionResult(true);
        }

        private ActionResult ValidateEmployments(List<EmploymentDTO> employmentInputs)
        {
            ActionResult result = employmentInputs.ValidateEmployments(validateFixedTerm14days: true);
            if (!result.Success)
            {
                if (result.ErrorNumber == (int)ActionResultSave.DatesInvalid)
                    result.ErrorMessage = GetText(11053, "Anställning har startdatum större än stoppdatum");
                else if (result.ErrorNumber == (int)ActionResultSave.DatesOverlapping)
                    result.ErrorMessage = GetText(11054, "En anställning överlappar en annan anställning");
                else if (result.ErrorNumber == (int)ActionResultSave.EmployeeGroupMandatory)
                    result.ErrorMessage = GetText(8539, "Tidavtal hittades inte");
                else if (result.ErrorNumber == (int)ActionResultSave.EmployeeEmploymentsInvalidFixedTerm14days)
                    result.ErrorMessage = GetText(11528, "Anställning med anställningsform 'Allmän visstidsanställning 14 dagar' måste vara 14 dagar");
            }
            return result;
        }

        private ActionResult AddEmployments(CompEntities entities, Employee employee, List<EmploymentDTO> employmentInputs, DateTime batch, bool doAcceptAttestedTemporaryEmployments = false)
        {
            if (employmentInputs.IsNullOrEmpty())
                return new ActionResult(true);

            foreach (EmploymentDTO employmentInput in employmentInputs.Where(e => e.IsNew()).ToList())
            {
                ActionResult result = AddEmployment(entities, employee, employmentInput, batch, doAcceptAttestedTemporaryEmployments);
                if (!result.Success)
                    return result;
            }

            return new ActionResult(true);
        }

        private ActionResult AddEmployment(CompEntities entities, Employee employee, EmploymentDTO employmentInput, DateTime batch, bool doAcceptAttestedTemporaryEmployments = false)
        {
            if (employee == null || employmentInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            Employment employment = Employment.Create(employee, employmentInput, GetUserDetails(), batch);
            entities.Employment.AddObject(employment);

            if (employmentInput.IsTemporaryPrimary)
            {
                ActionResult result = CreateTemporaryPrimaryAndHibernatingEmployment(entities, employee, employment, employmentInput, batch, doAcceptAttestedTemporaryEmployments);
                if (!result.Success)
                    return result;
            }

            SetEmploymentOriginalValues(entities, employee, employment, employmentInput);
            CreateEmploymentVacationGroups(employment, employmentInput, batch);
            CreateEmploymentChanges(employee, employment, employmentInput, batch);

            return new ActionResult(true);
        }

        private ActionResult UpdateEmployments(CompEntities entities, Employee employee, List<EmploymentDTO> employmentInputs, DateTime batch, bool generateCurrentChanges = false, bool doDeleteVacationGroups = true)
        {
            if (employee == null || employee.Employment.IsNullOrEmpty())
                return new ActionResult(true);

            List<Employment> employments = employee.Employment.GetActiveOrHidden();
            for (int i = 0; i < employments.Count; i++)
            {
                Employment employment = employments[i];
                if (employment == null)
                    continue;

                EmploymentDTO employmentInput = employmentInputs.FirstOrDefault(e => e.EmploymentId == employment.EmploymentId);
                if (employmentInput == null)
                    continue;

                if (generateCurrentChanges)
                    GenerateEmploymentInputCurrentChanges(entities, employee, employment, employmentInput);

                ActionResult result = UpdateEmployment(entities, employee, employment, employmentInput, batch, doDeleteVacationGroups);
                if (!result.Success)
                    return result;
            }

            return new ActionResult(true);
        }

        private ActionResult UpdateEmployment(CompEntities entities, Employee employee, Employment employment, EmploymentDTO employmentInput, DateTime batch, bool doDeleteVacationGroups = true)
        {
            if (employee == null || employment == null || employmentInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);
            if (employment.FinalSalaryStatus != (int)employmentInput.FinalSalaryStatus && employment.HasAppliedFinalSalary() && !employment.HasChangedFromAppliedManually(employmentInput.FinalSalaryStatus))
                return new ActionResult((int)ActionResultSave.EmploymentFinalSalaryIsApplied, String.Format(GetText(11529, "Anställningen med slutdatum {0} har slutavräknats och kan inte förändras förrens slutlönen är backad"), employment.DateTo.HasValue ? employment.DateTo.Value.ToString("yyyyMMdd") : ""));
            if (employmentInput.IsChangingToNotTemporary && employmentInput.DateTo < employment.DateTo)
                return new ActionResult((int)ActionResultSave.EmployeeDateToCannotBeShortendWhenChangingToNotTemporary, GetText(11529, "Slutdatum på anställning som ändrats till ordinarie får inte vara tidigare än den tillfälligt aktiv anställningens slutdatum"));

            if (employmentInput.IsChangingToNotTemporary)
            {
                ActionResult result = CloseTemporaryPrimaryEmployment(employee, employment, employmentInput, batch);
                if (!result.Success)
                    return result;
            }
            else
            {
                EmploymentDateChange dateChangeInterval = employment.Update(employmentInput);
                if (dateChangeInterval != null)
                {
                    ActionResult result = HandleEmploymentDateChanges(entities, employee, employment, employmentInput, dateChangeInterval, batch);
                    if (!result.Success)
                        return result;
                }

                SetEmploymentOriginalValues(entities, employee, employment, employmentInput);
                SetEmploymentVacationGroups(employment, employmentInput, doDeleteVacationGroups, batch);
                CreateEmploymentChanges(employee, employment, employmentInput, batch);
            }

            return new ActionResult(true);
        }

        private void GenerateEmploymentInputCurrentChanges(CompEntities entities, Employee employee, Employment employment, EmploymentDTO inputEmployment)
        {
            if (inputEmployment == null || !inputEmployment.IsEditing)
                return;

            List<EmployeeGroup> employeeGroups = GetEmployeeGroups(entities, employee.ActorCompanyId);
            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(entities, employee.ActorCompanyId);
            List<PayrollPriceType> payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, employee.ActorCompanyId, null, false);
            List<VacationGroup> vacationGroups = PayrollManager.GetVacationGroups(entities, employee.ActorCompanyId, setTypeName: true);
            List<AnnualLeaveGroup> annualLeaveGroups = AnnualLeaveManager.GetAnnualLeaveGroups(entities, employee.ActorCompanyId);

            EmploymentDTO dto = employment.ToDTO(employeeGroups: employeeGroups, payrollGroups: payrollGroups, payrollPriceTypes: payrollPriceTypes, vacationGroups: vacationGroups);
            dto.IsChangingEmployment = inputEmployment.IsChangingEmployment;
            dto.IsChangingEmploymentDates = inputEmployment.IsChangingEmploymentDates;
            dto.IsChangingToNotTemporary = inputEmployment.IsChangingToNotTemporary;
            dto.IsAddingEmployment = inputEmployment.IsAddingEmployment;
            dto.IsDeletingEmployment = inputEmployment.IsDeletingEmployment;
            dto.CurrentChangeDateFrom = inputEmployment.CurrentChangeDateFrom;
            dto.CurrentChangeDateTo = inputEmployment.CurrentChangeDateTo;
            if (dto.CurrentChangeDateFrom.HasValue)
                dto.ApplyEmploymentChanges(dto.CurrentChangeDateFrom.Value);
            if (!string.IsNullOrEmpty(inputEmployment.Comment))
                dto.Comment = inputEmployment.Comment;

            if (dto.IsChangingEmploymentDates)
            {
                dto.UpdateDateFrom(inputEmployment.DateFrom);
                dto.UpdateDateTo(inputEmployment.DateTo);
            }
            else
            {
                dto.UpdateState(inputEmployment.State, GetText(3273, "Aktiv"), GetText(2244, "Borttagen"));
                dto.UpdateEmployeeGroup(inputEmployment.EmployeeGroupId, employeeGroups.ToDictionary(i => i.EmployeeGroupId, i => i.Name));
                dto.UpdatePayrollGroup(inputEmployment.PayrollGroupId, payrollGroups.ToDictionary(i => i.PayrollGroupId, i => i.Name));
                dto.UpdateAnnualLeaveGroup(inputEmployment.AnnualLeaveGroupId, annualLeaveGroups.ToDictionary(i => i.AnnualLeaveGroupId, i => i.Name));
                dto.UpdateBaseWorkTimeWeek(inputEmployment.BaseWorkTimeWeek);
                dto.UpdateEmploymentType(inputEmployment.EmploymentType, GetEmploymentTypes(entities, employee.ActorCompanyId));
                dto.UpdateExperienceMonths(inputEmployment.ExperienceMonths);
                dto.UpdateExperienceAgreedOrEstablished(inputEmployment.ExperienceAgreedOrEstablished, GetText(11707, "Överrenskommen"), GetText(11708, "Konstaterad"));
                dto.UpdateExternalCode(inputEmployment.ExternalCode);
                dto.UpdateName(inputEmployment.Name);
                bool updatedPercent = dto.UpdatePercent(inputEmployment.Percent);
                dto.UpdateSpecialConditions(inputEmployment.SpecialConditions);
                dto.UpdateSubstituteFor(inputEmployment.SubstituteFor);
                dto.UpdateSubstituteForDueTo(inputEmployment.SubstituteForDueTo);
                dto.UpdateWorkTasks(inputEmployment.WorkTasks);
                dto.UpdateWorkTimeWeek(inputEmployment.WorkTimeWeek, employeeGroups.FirstOrDefault(i => i.EmployeeGroupId == inputEmployment.EmployeeGroupId)?.ToDTO(), forceFromPercentIfUnchanged: updatedPercent ? dto.Percent : (decimal?)null);
                dto.UpdateWorkPlace(inputEmployment.WorkPlace);
                dto.UpdateFullTimeWorkTimeWeek(inputEmployment.FullTimeWorkTimeWeek);
                dto.UpdateExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(inputEmployment.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment);
            }

            if (inputEmployment.DateTo.HasValue)
                dto.UpdateEmploymentEndReason(inputEmployment.EmploymentEndReason, GetTermGroupContent(TermGroup.EmploymentEndReason).ToDictionary());

            if (inputEmployment.CurrentChanges == null)
                inputEmployment.CurrentChanges = new List<EmploymentChangeDTO>();
            inputEmployment.CurrentChanges.AddRange(dto.CurrentChanges);
        }

        public Dictionary<int, Employment> GetFirstEmploymentForEachEmployee(int actorCompanyId, List<int> employeeIds)
        {
            List<Employment> employments = GetEmployments(actorCompanyId, employeeIds, true);

            var result = new Dictionary<int, Employment>();
            foreach (var employeeId in employeeIds)
            {
                var firstEmployment = employments
                    .Where(e => e.EmployeeId == employeeId)
                    .OrderBy(e => e.DateFrom)
                    .FirstOrDefault();
                if (firstEmployment != null)
                {
                    result[employeeId] = firstEmployment;
                }
            }
            return result;
        }

        #endregion

        #endregion

        #region EmploymentType

        public List<EmploymentTypeDTO> GetEmploymentTypes(int actorCompanyId, bool addEmptyRow = false, bool skipUnknown = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmploymentTypes(entities, actorCompanyId, (TermGroup_Languages)GetLangId(), addEmptyRow, skipUnknown);
        }

        public List<EmploymentTypeDTO> GetEmploymentTypes(CompEntities entities, int actorCompanyId, bool addEmptyRow = false, bool skipUnknown = false)
        {
            return GetEmploymentTypes(entities, actorCompanyId, (TermGroup_Languages)GetLangId(), addEmptyRow, skipUnknown);
        }

        public List<EmploymentTypeDTO> GetEmploymentTypes(int actorCompanyId, TermGroup_Languages language, bool addEmptyRow = false, bool skipUnknown = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmploymentTypes(entities, actorCompanyId, language, addEmptyRow, skipUnknown);
        }

        public List<EmploymentTypeDTO> GetEmploymentTypes(CompEntities entities, int actorCompanyId, TermGroup_Languages language, bool addEmptyRow = false, bool skipUnknown = false)
        {
            List<EmploymentTypeDTO> employmentTypes = base.GetEmploymentTypesFromCache(entities, CacheConfig.Company(actorCompanyId), language);
            if (skipUnknown)
                employmentTypes = employmentTypes.Where(i => i.Type != 0).ToList();
            else if (addEmptyRow)
                employmentTypes.Add(new EmploymentTypeDTO(0, " "));
            return employmentTypes;
        }

        public List<EmploymentTypeDTO> GetEmploymentTypesFromDBForGrid(int actorCompanyId, int? employmentTypeId = null)
        {
            List<EmploymentTypeDTO> standardTypes = GetStandardEmploymentTypes(actorCompanyId, TermCacheManager.Instance.GetLang());
            List<EmploymentTypeDTO> employmentTypes = GetEmploymentTypesFromDB(actorCompanyId);

            if (employmentTypeId.HasValue)
            {
                // If loading only one record for the grid it means we have just saved it.
                // If it's an overridden standard just reload whole grid, since it gets problematic with ids otherwise.
                EmploymentTypeDTO type = employmentTypes.FirstOrDefault(e => e.EmploymentTypeId == employmentTypeId.Value);
                if (type == null || !type.SettingOnly)
                    employmentTypes = employmentTypes.Where(e => e.EmploymentTypeId == employmentTypeId).ToList();
            }

            foreach (EmploymentTypeDTO employmentType in employmentTypes.ToList())
            {
                if (employmentType.SettingOnly)
                {
                    // If employment type is marked as SettingOnly, it is actually an overridden copy of a standard one.
                    // Find the standard, copy some properties to it and remove the copy from the list.
                    EmploymentTypeDTO std = employmentTypes.FirstOrDefault(t => t.Type == employmentType.Type && !t.SettingOnly);
                    if (std != null)
                    {
                        std.Code = employmentType.Code;
                        std.ExternalCode = employmentType.ExternalCode;
                        std.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = employmentType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment;

                        employmentTypes.Remove(employmentType);
                    }
                }
                else
                {
                    if (employmentType.Standard)
                        employmentType.IsActive = employmentType.Active;

                    employmentType.TypeName = standardTypes.FirstOrDefault(t => t.Type == employmentType.Type)?.Name;
                }
            }

            return employmentTypes;
        }

        public List<EmploymentTypeDTO> GetEmploymentTypesFromDB(int actorCompanyId, bool? active = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmploymentTypesFromDB(entities, actorCompanyId, TermCacheManager.Instance.GetLang(), active);
        }

        public List<EmploymentTypeDTO> GetEmploymentTypesFromDB(CompEntities entities, int actorCompanyId, TermGroup_Languages language, bool? active = null)
        {
            List<EmploymentTypeDTO> employmentTypes = GetStandardEmploymentTypes(entities, actorCompanyId, language);

            #region Company EmploymentTypes

            List<EmploymentType> companyEmploymentTypes = entities.EmploymentType.Where(e => e.ActorCompanyId == actorCompanyId && e.State != (int)SoeEntityState.Deleted).ToList();
            if (active == true)
                companyEmploymentTypes = companyEmploymentTypes.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                companyEmploymentTypes = companyEmploymentTypes.Where(i => i.State == (int)SoeEntityState.Inactive).ToList();

            foreach (EmploymentType companyEmploymentType in companyEmploymentTypes)
            {
                employmentTypes.Add(companyEmploymentType.ToDTO());
            }

            #endregion

            return employmentTypes.OrderBy(i => i.Name).ToList();
        }

        public List<EmploymentTypeDTO> GetStandardEmploymentTypes(int actorCompanyId, TermGroup_Languages language)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetStandardEmploymentTypes(entities, actorCompanyId, language);
        }

        public List<EmploymentTypeDTO> GetStandardEmploymentTypes(CompEntities entities, int actorCompanyId, TermGroup_Languages language)
        {
            List<EmploymentTypeDTO> employmentTypes = new List<EmploymentTypeDTO>();
            var terms = base.GetTermGroupContent(TermGroup.EmploymentType, (int)language);

            // TODO: Should not check language. Instead check country set on company
            // For now, a temporary fix is to set English and Finnish same as Swedish to get filtering on company setting work
            switch (language)
            {
                case TermGroup_Languages.Swedish:
                case TermGroup_Languages.English:
                case TermGroup_Languages.Finnish:
                    #region Swedish, English, Finnish

                    var settingsDict = SettingManager.GetCompanySettingsDict(entities, (int)CompanySettingTypeGroup.PayrollEmploymentTypes_SE, actorCompanyId);
                    foreach (var term in terms)
                    {
                        bool use = false;
                        if (term.Id == (int)TermGroup_EmploymentType.Unknown)
                        {
                            use = true;
                        }
                        else
                        {
                            CompanySettingType settingType = GetCompanySettingTypeByEmploymentTypeTerm((TermGroup_EmploymentType)term.Id);
                            if (settingType != CompanySettingType.Unknown)
                                use = SettingManager.GetBoolSettingFromDict(settingsDict, (int)settingType);
                        }

                        if (!employmentTypes.Exists(term.Id))
                            employmentTypes.Add(new EmploymentTypeDTO(term.Id, term.Name, use));
                    }

                    #endregion
                    break;
                case TermGroup_Languages.Norwegian:
                    #region Norwegian

                    foreach (var term in terms)
                    {
                        bool use = false;
                        switch (term.Id)
                        {
                            case (int)TermGroup_EmploymentType.Unknown:
                                use = true;
                                break;
                        }

                        if (use && !employmentTypes.Exists(term.Id))
                            employmentTypes.Add(new EmploymentTypeDTO(term.Id, term.Name));
                    }

                    #endregion
                    break;
                default:
                    foreach (var term in terms)
                    {
                        if (!employmentTypes.Exists(term.Id))
                            employmentTypes.Add(new EmploymentTypeDTO(term.Id, term.Name));
                    }

                    break;
            }

            return employmentTypes;
        }

        private CompanySettingType GetCompanySettingTypeByEmploymentTypeTerm(TermGroup_EmploymentType employmentTypeTerm)
        {
            CompanySettingType settingType = CompanySettingType.Unknown;

            switch (employmentTypeTerm)
            {
                case TermGroup_EmploymentType.SE_Probationary: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_Probationary; break;
                case TermGroup_EmploymentType.SE_Substitute: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_Substitute; break;
                case TermGroup_EmploymentType.SE_SubstituteVacation: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_SubstituteVacation; break;
                case TermGroup_EmploymentType.SE_Permanent: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_Permanent; break;
                case TermGroup_EmploymentType.SE_FixedTerm: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_FixedTerm; break;
                case TermGroup_EmploymentType.SE_Seasonal: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_Seasonal; break;
                case TermGroup_EmploymentType.SE_SpecificWork: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_SpecificWork; break;
                case TermGroup_EmploymentType.SE_Trainee: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_Trainee; break;
                case TermGroup_EmploymentType.SE_NormalRetirementAge: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_NormalRetirementAge; break;
                case TermGroup_EmploymentType.SE_CallContract: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_CallContract; break;
                case TermGroup_EmploymentType.SE_LimitedAfterRetirementAge: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_LimitedAfterRetirementAge; break;
                case TermGroup_EmploymentType.SE_FixedTerm14days: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_FixedTerm14days; break;
                case TermGroup_EmploymentType.SE_Apprentice: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_Apprentice; break;
                case TermGroup_EmploymentType.SE_SpecialFixedTerm: settingType = CompanySettingType.PayrollEmploymentTypeUse_SE_SpecialFixedTerm; break;
            }

            return settingType;
        }

        public EmploymentType GetCompanyEmploymentType(int actorCompanyId, int employmentTypeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanyEmploymentType(entities, actorCompanyId, employmentTypeId);
        }

        public EmploymentType GetCompanyEmploymentType(CompEntities entities, int actorCompanyId, int employmentTypeId)
        {
            return (from et in entities.EmploymentType
                    where et.ActorCompanyId == actorCompanyId &&
                    et.EmploymentTypeId == employmentTypeId
                    select et).FirstOrDefault();
        }

        public EmploymentType GetCompanyEmploymentTypeByCode(CompEntities entities, int actorCompanyId, string code)
        {
            if (code.IsNullOrEmpty())
                return null;

            return (from et in entities.EmploymentType
                    where et.ActorCompanyId == actorCompanyId &&
                    et.Code.ToLower() == code.ToLower()
                    select et).FirstOrDefault();
        }

        public EmploymentTypeDTO GetEmploymentType(int actorCompanyId, int type)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmploymentType(entities, actorCompanyId, type);
        }

        public EmploymentTypeDTO GetEmploymentType(CompEntities entities, int actorCompanyId, int type)
        {
            return GetEmploymentTypes(entities, actorCompanyId, (TermGroup_Languages)GetLangId())
                .GetEmploymentType(type);
        }

        /// <summary>
        /// Insert or update an EmploymentType
        /// </summary>    
        /// <returns>ActionResult</returns>
        public ActionResult SaveEmploymentType(EmploymentTypeDTO employmentTypeInput, int actorCompanyId)
        {
            if (employmentTypeInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmploymentType");

            int employmentTypeId = employmentTypeInput.EmploymentTypeId.FromNullable();
            bool isNew = employmentTypeId == 0;
            bool isStandard = employmentTypeInput.Standard;

            if (!isStandard && !employmentTypeInput.SettingOnly && EmploymentTypeDTO.IsStandard(employmentTypeInput.Code, out _, out int max))
                return new ActionResult((int)ActionResultSave.IncorrectInput, string.Format(GetText(91962, "Om kod är numeriskt måste det vara högre än standard anställningsformerna {0}"), max));

            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region EmploymentType

                        EmploymentType employmentType = null;
                        if (!isNew)
                        {
                            // Get existing
                            employmentType = GetCompanyEmploymentType(entities, actorCompanyId, employmentTypeId);
                        }

                        if (employmentType == null)
                        {
                            // Check if there is an overridden type already
                            employmentType = (from et in entities.EmploymentType
                                              where et.ActorCompanyId == actorCompanyId &&
                                              et.Type == employmentTypeInput.Type &&
                                              et.SettingOnly
                                              select et).FirstOrDefault();
                        }

                        if (employmentType == null)
                        {
                            #region EmploymentType Add

                            employmentType = new EmploymentType()
                            {
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(employmentType);
                            entities.EmploymentType.AddObject(employmentType);

                            if (isStandard)
                            {
                                // If updating a standard employment type,
                                // create a copy and set property SettingOnly to true.
                                employmentType.SettingOnly = true;
                            }

                            #endregion
                        }
                        else
                        {
                            #region EmploymentType Update

                            SetModifiedProperties(employmentType);

                            #endregion
                        }

                        employmentType.Name = employmentTypeInput.Name;
                        employmentType.Code = employmentTypeInput.Code;
                        employmentType.Description = employmentTypeInput.Description;
                        employmentType.Type = employmentTypeInput.Type;
                        employmentType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = employmentTypeInput.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment;
                        employmentType.ExternalCode = employmentTypeInput.ExternalCode;
                        employmentType.State = employmentTypeInput.Active ? (int)SoeEntityState.Active : (int)SoeEntityState.Inactive;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            employmentTypeId = employmentType.EmploymentTypeId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                }
                finally
                {
                    if (result != null && result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = employmentTypeId;

                        FlushEmploymentTypesFromCache(CacheConfig.Company(actorCompanyId));
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Sets an employmentType state to Deleted
        /// </summary>       
        /// <returns>ActionResult</returns>
        public ActionResult DeleteEmploymentType(int actorCompanyId, int employmentTypeId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                // Check relations
                if (EmploymentTypeIsUsed(actorCompanyId, employmentTypeId))
                    return new ActionResult((int)ActionResultDelete.EntityInUse, GetText(92042, "Anställningsform används och kan därför inte tas bort."));

                #endregion

                EmploymentType employmentType = GetCompanyEmploymentType(entities, actorCompanyId, employmentTypeId);
                if (employmentType == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "EmploymentType");

                return ChangeEntityState(entities, employmentType, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult UpdateEmploymentTypesState(Dictionary<int, bool> employmentTypes, int actorCompanyId, int? roleId = null)
        {
            using (CompEntities entities = new CompEntities())
            {
                bool hasTimePreferencesCompSettingsModifyPermission = roleId.HasValue ? FeatureManager.HasRolePermission(Feature.Time_Preferences_CompSettings, Permission.Modify, roleId.Value, actorCompanyId) : false;
                Dictionary<int, bool> companySettingValues = new Dictionary<int, bool>();

                foreach (KeyValuePair<int, bool> employmentType in employmentTypes)
                {
                    if (!EmploymentTypeDTO.IsStandard(employmentType.Key))
                    {
                        EmploymentType originalEmploymentType = GetCompanyEmploymentType(entities, actorCompanyId, employmentType.Key);
                        if (originalEmploymentType == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "EmploymentType");

                        ChangeEntityState(originalEmploymentType, employmentType.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                    }
                    else
                    {
                        if (!hasTimePreferencesCompSettingsModifyPermission)
                            return new ActionResult((int)ActionResultSave.InsufficienPermissionToSave, "No permission to save the activity state of a standard employment type.");

                        CompanySettingType settingType = GetCompanySettingTypeByEmploymentTypeTerm((TermGroup_EmploymentType)employmentType.Key);

                        if (settingType == CompanySettingType.Unknown)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "EmploymentType");

                        companySettingValues.Add((int)settingType, employmentType.Value);
                    }
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                if (companySettingValues.Count > 0)
                {
                    ActionResult resultCompanySettings = SettingManager.UpdateInsertBoolSettings(SettingMainType.Company, companySettingValues, UserId, actorCompanyId, 0);
                    if (!resultCompanySettings.Success)
                        return resultCompanySettings;

                    result.ObjectsAffected += resultCompanySettings.ObjectsAffected;

                    if (result.ObjectsAffected > 0 && result.ErrorNumber == (int)ActionResultSave.NothingSaved)
                        result.ErrorNumber = (int)ActionResultSave.Unknown;
                }

                return result;
            }
        }

        private bool EmploymentTypeIsUsed(int actorCompanyId, int employmentTypeId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Employment.NoTracking();
            bool usedOnEmployment = (from s in entitiesReadOnly.Employment
                                     where s.ActorCompanyId == actorCompanyId &&
                                     s.OriginalType == employmentTypeId
                                     select s).Any();

            if (usedOnEmployment)
                return true;

            entitiesReadOnly.EmploymentChange.NoTracking();
            string employmentTypeString = employmentTypeId.ToString();
            bool usedOnEmploymentChange = (from s in entitiesReadOnly.EmploymentChange
                                           where s.EmploymentChangeBatch.ActorCompanyId == actorCompanyId &&
                                           s.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmploymentType &&
                                           (s.FromValue == employmentTypeString || s.ToValue == employmentTypeString)
                                           select s).Any();


            if (usedOnEmploymentChange)
                return true;

            return false;

        }

        #endregion

        #region EmploymentContract

        public DataStorage SaveEmploymentContractOnPrint(CompEntities entities, int reportId, int employeeId, byte[] data, string xml, string reportName)
        {
            string employeeNr = (from e in entities.Employee
                                 where e.EmployeeId == employeeId
                                 select e.EmployeeNr).FirstOrDefault();

            DataStorage dataStorage = new DataStorage()
            {
                ActorCompanyId = base.ActorCompanyId,
                Data = data,
                XML = xml,
                EmployeeId = employeeId,
                OriginType = (int)SoeDataStorageOriginType.Data,
                Type = (int)SoeDataStorageRecordType.TimeEmploymentContract,
                FileSize = data.Length,
                FileName = employeeNr + "_" + CalendarUtility.ToFileFriendlyDateTime(DateTime.Now) + ".pdf",
                Description = reportName + ".pdf",
                Extension = ".pdf"
            };
            SetCreatedProperties(dataStorage);

            GeneralManager.CompressStorage(entities, dataStorage, false, false);
            entities.DataStorage.AddObject(dataStorage);
            DataStorageRecord dataStorageRecord = GeneralManager.CreateDataStorageRecord(entities, SoeDataStorageRecordType.TimeEmploymentContract, employeeId, reportName + ".pdf", SoeEntityType.Employee, dataStorage);

            // Get roles from report
            if (reportId == 0)
                reportId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmploymentContractShortSubstituteReport, 0, base.ActorCompanyId, 0);

            List<int> roleIds = (from r in entities.ReportRolePermission
                                 where r.ReportId == reportId &&
                                 r.State == (int)SoeEntityState.Active
                                 select r.RoleId).ToList();

            foreach (int roleId in roleIds)
            {
                DataStorageRecordRolePermission perm = new DataStorageRecordRolePermission()
                {
                    RoleId = roleId,
                    ActorCompanyId = base.ActorCompanyId,
                };
                SetCreatedProperties(perm);
                dataStorageRecord.DataStorageRecordRolePermission.Add(perm);
            }

            SaveChanges(entities);

            return dataStorage;
        }

        #endregion

        #region EmploymentChangeBatch

        public List<EmploymentChangeBatch> GetEmploymentChangeBatches(int employmentId, int employeeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmploymentChangeBatch.NoTracking();
            return GetEmploymentChangeBatches(entities, employmentId, employeeId, actorCompanyId);
        }

        public List<EmploymentChangeBatch> GetEmploymentChangeBatches(CompEntities entities, int employmentId, int employeeId, int actorCompanyId)
        {
            return (from b in entities.EmploymentChangeBatch
                        .Include("EmploymentChange")
                    where b.EmploymentId == employmentId &&
                    b.EmployeeId == employeeId &&
                    b.ActorCompanyId == actorCompanyId
                    select b).ToList();
        }

        public List<EmploymentChangeBatch> GetEmploymentChangeBatchesWithFromDate(CompEntities entities, int employeeId, int employmentId, DateTime fromDate)
        {
            return (from b in entities.EmploymentChangeBatch
                    where b.EmployeeId == employeeId &&
                    b.EmploymentId == employmentId &&
                    b.FromDate == fromDate
                    select b).ToList();
        }

        public List<EmploymentChangeBatch> GetEmploymentChangeBatchesWithToDate(CompEntities entities, int employeeId, int employmentId, DateTime toDate)
        {
            return (from b in entities.EmploymentChangeBatch
                    where b.EmployeeId == employeeId &&
                    b.EmploymentId == employmentId &&
                    b.ToDate == toDate
                    select b).ToList();
        }

        public void AddEmploymentChangeAndBatch(TermGroup_EmploymentChangeType changeType, TermGroup_EmploymentChangeFieldType fieldType, Employment employment, DateTime? batchFromDate, DateTime? batchToDate, string batchComment, DateTime? batchCreated, string fromValue, string toValue, string fromValueName = "", string toValueName = "")
        {
            var batch = new EmploymentChangeBatch()
            {
                FromDate = batchFromDate,
                ToDate = batchToDate,
                Comment = batchComment,

                //Set FK
                EmploymentId = employment.EmploymentId,
                EmployeeId = employment.EmployeeId,
                ActorCompanyId = employment.ActorCompanyId,
            };
            SetCreatedProperties(batch, created: batchCreated);
            employment.EmploymentChangeBatch.Add(batch);

            var change = new EmploymentChange()
            {
                Type = (int)changeType,
                FieldType = (int)fieldType,

                FromValue = fromValue,
                ToValue = toValue,
                FromValueName = fromValueName,
                ToValueName = toValueName,

                //Set FK
                EmploymentId = employment.EmploymentId,
                EmployeeId = employment.EmployeeId,
            };
            SetCreatedProperties(change, created: batchCreated);
            batch.EmploymentChange.Add(change);
        }

        #endregion

        #region EmploymentChange

        public void ApplyEmployment(EmployeeUserDTO dto, DateTime changesForDate, int actorCompanyId)
        {
            if (dto == null || dto.Employments == null)
                return;

            int langId = GetLangId();
            Dictionary<int, string> fieldTypesDict = base.GetTermGroupDict(TermGroup.EmploymentChangeFieldType, langId);
            Dictionary<int, string> payrollPriceTypesDict = PayrollManager.GetPayrollPriceTypesDict(actorCompanyId, null);
            Dictionary<int, string> employmentEndReasonsDict = GetSystemEndReasons(actorCompanyId, language: langId, includeCompanyEndReasons: true);
            List<EmploymentTypeDTO> employmentTypes = GetEmploymentTypes(actorCompanyId, (TermGroup_Languages)langId);
            List<EmployeeGroupDTO> employeeGroups = GetEmployeeGroups(actorCompanyId, onlyActive: false).ToDTOs().ToList();
            List<PayrollGroupDTO> payrollGroups = PayrollManager.GetPayrollGroups(actorCompanyId, onlyActive: true).ToDTOs().ToList();
            List<AnnualLeaveGroupDTO> annualLeaveGroups = AnnualLeaveManager.GetAnnualLeaveGroups(actorCompanyId).ToDTOs().ToList();

            dto.Employments.ApplyEmploymentHistory(fieldTypesDict, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict, payrollPriceTypesDict, annualLeaveGroups);
            dto.Employments.ApplyEmploymentChanges(changesForDate, employeeGroups, payrollGroups, annualLeaveGroups, employmentTypes, employmentEndReasonsDict);
            dto.Employments.SetEmploymentTypeNames(employmentTypes);
            dto.Employments.SetHibernatingPeriods();
        }

        #endregion

        #region EmploymentPriceType

        public List<EmploymentPriceType> GetEmploymentPriceTypesForCompany(CompEntities entities, int actorCompanyId, List<int> employeeIds = null)
        {
            return employeeIds.IsNullOrEmpty() ? (from p in entities.EmploymentPriceType
                                                .Include("EmploymentPriceTypePeriod")
                                                .Include("PayrollPriceType")
                                                .Include("Employment")
                                                  where p.PayrollPriceType.ActorCompanyId == actorCompanyId &&
                                                  p.State == (int)SoeEntityState.Active
                                                  select p).ToList()
                                                 :
                                                 (from p in entities.EmploymentPriceType
                                                 .Include("EmploymentPriceTypePeriod")
                                                .Include("PayrollPriceType")
                                                 .Include("Employment")
                                                  where p.PayrollPriceType.ActorCompanyId == actorCompanyId &&
                                                  employeeIds.Contains(p.Employment.EmployeeId) &&
                                                  p.State == (int)SoeEntityState.Active
                                                  select p).ToList();
        }

        public List<EmploymentPriceTypeDTO> GetEmploymentPriceTypesForEmployee(CompEntities entities, int employeeId, Dictionary<int, List<EmploymentPriceTypeDTO>> employmentPriceTypesForCompanyDict = null)
        {
            if (employmentPriceTypesForCompanyDict == null)
            {
                List<EmploymentPriceType> employmentPriceTypes =
                    (from p in entities.EmploymentPriceType
                        .Include("PayrollPriceType")
                        .Include("EmploymentPriceTypePeriod")
                        .Include("Employment")
                     where p.Employment.EmployeeId == employeeId &&
                     p.State == (int)SoeEntityState.Active
                     select p).ToList();

                return employmentPriceTypes.ToDTOs(true, false).ToList();
            }
            else
            {
                if (employmentPriceTypesForCompanyDict.TryGetValue(employeeId, out List<EmploymentPriceTypeDTO> value))
                    return value;
            }

            return new List<EmploymentPriceTypeDTO>();
        }

        public Dictionary<int, List<EmploymentPriceTypeChangeDTO>> GetEmploymentPriceTypeChangesForEmployees(int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmploymentPriceTypeChangesForEmployees(entities, actorCompanyId, employeeIds, dateFrom, dateTo);
        }

        public Dictionary<int, List<EmploymentPriceTypeChangeDTO>> GetEmploymentPriceTypeChangesForEmployees(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            Dictionary<int, List<EmploymentPriceTypeChangeDTO>> outList = new Dictionary<int, List<EmploymentPriceTypeChangeDTO>>();
            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(actorCompanyId, loadPriceTypes: true, loadSettings: true);
            List<PayrollGroupPriceTypeDTO> companyPayrollGroupPriceTypes = base.GetPayrollGroupPriceTypesForCompanyFromCache(entities, CacheConfig.Company(actorCompanyId)).ToDTOs(true).ToList();
            Dictionary<int, List<EmploymentPriceTypeDTO>> employmentPriceTypesForCompanyDict = GetEmploymentPriceTypesForCompany(entities, actorCompanyId).ToDTOs(true, false).GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, v => v.ToList());

            int? onePayrollGroup = null;

            List<Employee> employees = GetAllEmployeesByIds(actorCompanyId, employeeIds, loadEmployment: true, loadEmploymentPriceType: true);
            foreach (Employee employee in employees)
            {
                if (employee != null && CalendarUtility.GetTotalDays(dateFrom, dateTo) > 3)
                {
                    var ids = employee.GetPayrollGroupIds(dateFrom, dateTo, payrollGroups);

                    if (!ids.IsNullOrEmpty())
                    {
                        onePayrollGroup = ids.Count == 1 ? ids.First() : (int?)null;
                    }
                }

                DateTime currentDate = dateFrom;
                Employment employment = null;
                List<EmploymentPriceTypeChangeDTO> prevPriceTypeChanges = new List<EmploymentPriceTypeChangeDTO>();

                while (currentDate <= dateTo)
                {
                    if (employee != null)
                    {
                        employment = employee.GetEmployment(currentDate);
                    }

                    if (employment != null)
                    {
                        List<EmploymentPriceTypeChangeDTO> currentPriceTypeChanges = new List<EmploymentPriceTypeChangeDTO>();

                        int? payrollGroupId = null;
                        int employmentId = 0;

                        if (employment != null)
                        {
                            if (onePayrollGroup.HasValue)
                                payrollGroupId = onePayrollGroup.Value;
                            else
                                payrollGroupId = employment.GetPayrollGroupId(currentDate);

                            employmentId = employment.EmploymentId;
                        }

                        if (payrollGroupId.HasValue)
                        {
                            List<EmploymentPriceTypeDTO> priceTypes = GetEmploymentPriceTypes(employmentId, payrollGroupId, currentDate, employmentPriceTypesForCompanyDict.ContainsKey(employee.EmployeeId) ? employmentPriceTypesForCompanyDict[employee.EmployeeId] : new List<EmploymentPriceTypeDTO>(), companyPayrollGroupPriceTypes);
                            foreach (var priceTypeItem in priceTypes)
                            {
                                decimal amount = 0;
                                decimal? priceTypeItemAmount = priceTypeItem.GetAmount(currentDate);
                                //if (priceTypeItemAmount.HasValue && priceTypeItemAmount.Value != 0)
                                if (priceTypeItemAmount.HasValue)
                                    amount = priceTypeItemAmount.Value;
                                else if (priceTypeItem.PayrollGroupAmount.HasValue && priceTypeItem.PayrollGroupAmount.Value != 0)
                                    amount = priceTypeItem.PayrollGroupAmount.Value;


                                #region New item

                                //Get correct FromDate
                                DateTime? dateF = currentDate;
                                if (currentDate == dateFrom)
                                {
                                    DateTime? tempFrom = priceTypeItem.GetPeriod(currentDate)?.FromDate;
                                    if (tempFrom != null)
                                    {
                                        dateF = tempFrom < employment.DateFrom ? employment.DateFrom : tempFrom;
                                    }
                                }

                                EmploymentPriceTypeChangeDTO newChange = new EmploymentPriceTypeChangeDTO()
                                {
                                    PayrollPriceTypeId = priceTypeItem.PayrollPriceTypeId,
                                    PayrollPriceType = priceTypeItem.PayrollPriceType,
                                    FromDate = dateF,
                                    Amount = amount,
                                    IsPayrollGroupPriceType = priceTypeItem.IsPayrollGroupPriceType,
                                    IsSecondaryEmployment = employment.IsSecondaryEmployment,
                                    PayrollLevelId = priceTypeItem.CurrentPayrollLevelId,
                                    PayrollLevelName = priceTypeItem.CurrentPayrollLevelName,
                                    PayrollGroupAmount = priceTypeItem.PayrollGroupAmount ?? decimal.Zero
                                };
                                #endregion

                                //Add all items on first day
                                if (currentDate == dateFrom)
                                {
                                    if (outList.ContainsKey(employee.EmployeeId))
                                        outList[employee.EmployeeId].Add(newChange);
                                    else
                                        outList.Add(employee.EmployeeId, new List<EmploymentPriceTypeChangeDTO> { newChange });
                                }
                                else
                                {
                                    //Check PayrollPriceType on previous day
                                    bool isEqual = false;
                                    EmploymentPriceTypeChangeDTO prevChange = prevPriceTypeChanges.FirstOrDefault(i => i.PayrollPriceTypeId == newChange.PayrollPriceTypeId);
                                    if (prevChange != null)
                                    {
                                        isEqual = prevChange.IsEqual(newChange);
                                    }

                                    if (!isEqual)
                                    {
                                        if (outList.ContainsKey(employee.EmployeeId))
                                            outList[employee.EmployeeId].Add(newChange);
                                        else
                                            outList.Add(employee.EmployeeId, new List<EmploymentPriceTypeChangeDTO> { newChange });
                                    }
                                }
                                currentPriceTypeChanges.Add(newChange);
                            }
                        }
                        prevPriceTypeChanges = currentPriceTypeChanges;

                    }
                    currentDate = currentDate.AddDays(1);
                }

            }
            return outList;
        }

        public List<EmploymentPriceType> GetEmploymentPriceTypes(CompEntities entities, int employmentId)
        {
            return (from p in entities.EmploymentPriceType
                    .Include("PayrollPriceType")
                    .Include("Employment")
                    .Include("EmploymentPriceTypePeriod")
                    where p.EmploymentId == employmentId &&
                    p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }

        public List<EmploymentPriceTypeDTO> GetEmploymentPriceTypes(int employmentId, int? payrollGroupId, DateTime date, List<EmploymentPriceTypeDTO> employmentPriceTypes = null, List<PayrollGroupPriceTypeDTO> payrollGroupPriceTypes = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmploymentPriceTypes(entities, employmentId, payrollGroupId, date, employmentPriceTypes, payrollGroupPriceTypes: payrollGroupPriceTypes);
        }

        public List<EmploymentPriceTypeDTO> GetEmploymentPriceTypes(CompEntities entities, int employmentId, int? payrollGroupId, DateTime date, List<EmploymentPriceTypeDTO> employmentPriceTypes = null, List<PayrollGroupPriceTypeDTO> payrollGroupPriceTypes = null)
        {
            List<EmploymentPriceTypeDTO> priceTypes = new List<EmploymentPriceTypeDTO>();

            #region EmploymentPriceTypes

            if (employmentPriceTypes == null)
                employmentPriceTypes = GetEmploymentPriceTypes(entities, employmentId).ToDTOs(true, false).ToList();

            priceTypes.AddRange(employmentPriceTypes.Where(e => e.EmploymentId == employmentId));

            #endregion

            #region PayrollGroupPriceTypes

            if (payrollGroupId.HasValue)
            {
                if (payrollGroupPriceTypes == null)
                    payrollGroupPriceTypes = PayrollManager.GetPayrollGroupPriceTypes(entities, payrollGroupId.Value).ToDTOs(true).ToList();

                foreach (PayrollGroupPriceTypeDTO payrollGroupPriceType in payrollGroupPriceTypes.Where(w => w.PayrollGroupId == payrollGroupId.Value))
                {
                    decimal? payrollGroupAmount = null;
                    DateTime? payrollGroupAmountDate = null;

                    PayrollGroupPriceTypePeriodDTO payrollGroupPriceTypePeriod = payrollGroupPriceType.Periods?
                        .Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date))
                        .OrderBy(p => p.FromDate)
                        .LastOrDefault();

                    if (payrollGroupPriceTypePeriod != null)
                    {
                        payrollGroupAmount = payrollGroupPriceTypePeriod.Amount;
                    }
                    else
                    {
                        // No periods on payroll group price type, get amount from price type
                        PayrollPriceTypePeriodDTO payrollPriceTypePeriod = payrollGroupPriceType.PayrollPriceType?.Periods?
                            .Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date))
                            .OrderBy(p => p.FromDate)
                            .LastOrDefault();

                        if (payrollPriceTypePeriod != null)
                        {
                            payrollGroupAmountDate = payrollPriceTypePeriod.FromDate;
                            payrollGroupAmount = payrollPriceTypePeriod.Amount;
                        }
                    }

                    // If price type is already added from employment, only update the PayrollGroupAmount field
                    EmploymentPriceTypeDTO existingPriceType = priceTypes.FirstOrDefault(p => p.PayrollPriceTypeId == payrollGroupPriceType.PayrollPriceTypeId);
                    if (existingPriceType != null)
                    {
                        EmploymentPriceTypePeriodDTO currentPeriod = existingPriceType.GetPeriod(date);
                        if (currentPeriod == null || (currentPeriod != null && currentPeriod.PayrollLevelId != payrollGroupPriceType.PayrollLevelId))
                            continue;

                        existingPriceType.CurrentPayrollLevelId = currentPeriod?.PayrollLevelId;
                        existingPriceType.CurrentPayrollLevelName = currentPeriod?.PayrollLevelName;
                        existingPriceType.PayrollGroupAmount = payrollGroupAmount;
                    }
                    else
                    {
                        //payrollgroup pricetypes with levels dont automatictly "follows" the emp  (they should/need to exist as an EmploymentPriceType)
                        if (payrollGroupPriceType.PayrollLevelId.HasValue)
                            continue;

                        priceTypes.Add(new EmploymentPriceTypeDTO()
                        {
                            EmploymentPriceTypeId = 0,
                            EmploymentId = 0,
                            PayrollPriceTypeId = payrollGroupPriceType.PayrollPriceTypeId,
                            PayrollGroupAmount = payrollGroupAmount,
                            PayrollGroupAmountDate = payrollGroupAmountDate,
                            Code = payrollGroupPriceType.PriceTypeCode,
                            Name = payrollGroupPriceType.PriceTypeName,
                            CurrentPayrollLevelId = payrollGroupPriceType.PayrollLevelId,
                            CurrentPayrollLevelName = payrollGroupPriceType.PayrollLevelName,
                            PayrollPriceType = payrollGroupPriceType.PayrollPriceType != null ? (TermGroup_SoePayrollPriceType)payrollGroupPriceType.PayrollPriceType.Type : TermGroup_SoePayrollPriceType.Misc,
                            ReadOnly = payrollGroupPriceType.ReadOnlyOnEmployee,
                            IsPayrollGroupPriceType = true,
                            Type = payrollGroupPriceType.PayrollPriceType
                        });
                    }
                }
            }

            #endregion

            return priceTypes.OrderBy(p => p.Name).ToList();
        }

        public List<EmploymentPriceTypeView> GetEmploymentPriceTypeViews(CompEntities entities, int actorCompanyId)
        {
            return (from epp in entities.EmploymentPriceTypeView
                    where epp.ActorCompanyId == actorCompanyId
                    select epp).ToList();
        }

        public EmploymentPriceType GetEmploymentPriceType(CompEntities entities, int employmentId, int payrollPriceTypeId)
        {
            return (from e in entities.EmploymentPriceType.Include("EmploymentPriceTypePeriod").Include("Employment")
                    where e.EmploymentId == employmentId &&
                    e.PayrollPriceTypeId == payrollPriceTypeId &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        public EmploymentPriceType GetEmploymentPriceTypeByPriceType(CompEntities entities, int employmentId, int payrollPriceTypeId)
        {
            return (from e in entities.EmploymentPriceType.Include("PayrollPriceType").Include("EmploymentPriceTypePeriod").Include("Employment")
                    where e.EmploymentId == employmentId &&
                    e.PayrollPriceTypeId == payrollPriceTypeId &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        public EmploymentPriceType CreateEmploymentPriceType(CompEntities entities, Employee employee, Employment employment, EmploymentPriceTypeDTO priceTypeInput, List<TrackChangesDTO> trackChangesItems, TermGroup_TrackChangesActionMethod actionMethod, int recordId)
        {
            if (employee == null || employment == null || priceTypeInput == null || trackChangesItems == null)
                return null;

            trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, employee.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Insert, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.EmploymentPriceType, recordId, SoeEntityType.Employment, employment.EmploymentId));

            EmploymentPriceType priceType = new EmploymentPriceType()
            {
                PayrollPriceTypeId = priceTypeInput.PayrollPriceTypeId,
            };
            employment.EmploymentPriceType.Add(priceType);
            return priceType;
        }

        public void DeleteEmploymentPriceType(CompEntities entities, Employee employee, Employment employment, EmploymentPriceType priceType, List<TrackChangesDTO> trackChangesItems, TermGroup_TrackChangesActionMethod actionMethod)
        {
            if (employee == null || employment == null || priceType == null || trackChangesItems == null)
                return;

            trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, employee.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.EmploymentPriceType, priceType.EmploymentPriceTypeId, SoeEntityType.Employment, employment.EmploymentId));
            priceType.State = (int)SoeEntityState.Deleted;
            SetModifiedProperties(priceType);
        }

        public void DeleteEmploymentPriceTypeIfAllPeriodsAreDeleted(CompEntities entities, Employee employee, Employment employment, EmploymentPriceType priceType, EmploymentPriceTypeDTO priceTypeInput, List<TrackChangesDTO> trackChangesItems)
        {
            if (employee == null || employment == null || priceType == null || priceTypeInput == null || trackChangesItems == null)
                return;

            // If all periods are removed, remove pricetype on employment (will revert back to the payroll group's price type)
            if (!priceType.EmploymentPriceTypePeriod.Any(p => p.State == (int)SoeEntityState.Active) && !priceTypeInput.Periods.Any() && priceType.State != (int)SoeEntityState.Deleted)
            {
                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, employee.ActorCompanyId, TermGroup_TrackChangesActionMethod.Employee_Save, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.EmploymentPriceType, priceType.EmploymentPriceTypeId, SoeEntityType.Employment, employment.EmploymentId));
                priceType.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(priceType);
            }
        }

        #endregion

        #region EmploymentPriceTypePeriod

        public EmploymentPriceTypePeriod CreateEmploymentPriceTypePeriod(CompEntities entities, Employee employee, EmploymentPriceType priceType, EmploymentPriceTypePeriodDTO periodInput, List<TrackChangesDTO> trackChangesItems, TermGroup_TrackChangesActionMethod actionMethod, int recordId, int? parentRecordId = null)
        {
            if (employee == null || priceType == null || periodInput == null || trackChangesItems == null)
                return null;

            trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, employee.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Insert, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.EmploymentPriceTypePeriod, recordId, SoeEntityType.EmploymentPriceType, parentRecordId));

            EmploymentPriceTypePeriod period = new EmploymentPriceTypePeriod()
            {
                FromDate = periodInput.FromDate,
                Amount = periodInput.Amount,
                PayrollLevelId = periodInput.PayrollLevelId,
            };
            SetCreatedProperties(period);
            priceType.EmploymentPriceTypePeriod.Add(period);
            return period;
        }

        public void UpdateEmploymentPriceTypePeriod(CompEntities entities, Employee employee, EmploymentPriceType priceType, EmploymentPriceTypePeriod period, EmploymentPriceTypePeriodDTO periodInput, List<TrackChangesDTO> trackChangesItems, TermGroup_TrackChangesActionMethod actionMethod)
        {
            if (employee == null || priceType == null || period == null || periodInput == null || trackChangesItems == null)
                return;
            if (period.FromDate == periodInput.FromDate && period.Amount == periodInput.Amount && period.PayrollLevelId == periodInput.PayrollLevelId)
                return;

            if (period.FromDate != periodInput.FromDate)
            {
                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, employee.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.EmploymentPriceTypePeriod, period.EmploymentPriceTypePeriodId, SettingDataType.Date, "FromDate", TermGroup_TrackChangesColumnType.EmploymentPriceTypePeriod_FromDate, SoeEntityType.EmploymentPriceType, priceType.EmploymentPriceTypeId, period.FromDate.HasValue ? period.FromDate.Value.ToShortDateString() : null, periodInput.FromDate.HasValue ? periodInput.FromDate.Value.ToShortDateString() : null));
                period.FromDate = periodInput.FromDate;
            }
            if (period.Amount != periodInput.Amount)
            {
                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, employee.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.EmploymentPriceTypePeriod, period.EmploymentPriceTypePeriodId, SettingDataType.Decimal, "Amount", TermGroup_TrackChangesColumnType.EmploymentPriceTypePeriod_Amount, SoeEntityType.EmploymentPriceType, priceType.EmploymentPriceTypeId, period.Amount.ToString("N2"), periodInput.Amount.ToString("N2")));
                period.Amount = periodInput.Amount;
            }
            if (period.PayrollLevelId != periodInput.PayrollLevelId)
            {
                PayrollLevel oldPayrollLevel = period.PayrollLevelId.HasValue ? PayrollManager.GetPayrollLevel(entities, employee.ActorCompanyId, period.PayrollLevelId.Value) : null;
                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, employee.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.EmploymentPriceTypePeriod, period.EmploymentPriceTypePeriodId, SettingDataType.String, "PayrollLevelId", TermGroup_TrackChangesColumnType.EmploymentPriceTypePeriod_Level, SoeEntityType.EmploymentPriceType, priceType.EmploymentPriceTypeId, oldPayrollLevel?.Name, $"{periodInput.PayrollLevelName} ({periodInput.FromDate?.ToShortDateString()})"));
                period.PayrollLevelId = periodInput.PayrollLevelId;
            }
            SetModifiedProperties(period);
        }

        public void DeleteEmploymentPriceTypePeriod(CompEntities entities, Employee employee, EmploymentPriceType priceType, EmploymentPriceTypePeriod period, List<TrackChangesDTO> trackChangesItems, TermGroup_TrackChangesActionMethod actionMethod)
        {
            if (employee == null || priceType == null || period == null || trackChangesItems == null)
                return;

            if (period.State != (int)SoeEntityState.Deleted)
            {
                trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, employee.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, employee.EmployeeId, SoeEntityType.EmploymentPriceTypePeriod, period.EmploymentPriceTypePeriodId, SoeEntityType.EmploymentPriceType, priceType.EmploymentPriceTypeId));
                period.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(period);
            }
        }

        #endregion

        #region EmploymentAccountStd

        public List<EmploymentAccountStd> GetEmploymentAccounts(int employeeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmploymentAccountStd.NoTracking();
            return GetEmploymentAccounts(entities, employeeId);
        }

        public List<EmploymentAccountStd> GetEmploymentAccounts(List<int> employmentIds, EmploymentAccountType employeeAccountType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmploymentAccountStd.NoTracking();
            return GetEmploymentAccounts(entities, employmentIds, employeeAccountType);
        }

        public List<EmploymentAccountStd> GetEmploymentAccounts(CompEntities entities, List<int> employmentIds, EmploymentAccountType employeeAccountType = EmploymentAccountType.Unknown, bool loadAccounts = true)
        {
            List<EmploymentAccountStd> employmentAccountStds = new List<EmploymentAccountStd>();
            List<int> batch = new List<int>();
            int batchSize = 2000;
            List<int> unhandledEmploymentIds = employmentIds;

            while (unhandledEmploymentIds.Any())
            {
                batch = unhandledEmploymentIds.Take(batchSize).ToList();
                unhandledEmploymentIds = unhandledEmploymentIds.Skip(batchSize).ToList();
                employmentAccountStds.AddRange(GetEmploymentAccountsBatch(entities, batch, employeeAccountType, loadAccounts));
            }

            return employmentAccountStds;
        }

        private List<EmploymentAccountStd> GetEmploymentAccountsBatch(CompEntities entities, List<int> employmentIds, EmploymentAccountType employeeAccountType = EmploymentAccountType.Unknown, bool loadAccounts = true)
        {

            var query = from eas in entities.EmploymentAccountStd
                        .Include("AccountInternal")
                        where
                        employmentIds.Contains(eas.EmploymentId) &&
                       eas.Employment.State == (int)SoeEntityState.Active
                        select eas;

            if (loadAccounts)
            {
                query = query.Include("AccountStd.Account.AccountDim")
                             .Include("AccountInternal.Account.AccountDim");
            }

            if (employeeAccountType != EmploymentAccountType.Unknown)
            {
                int type = (int)employeeAccountType;
                query = query.Where(eas => eas.Type == type);
            }

            return query.ToList();
        }

        public List<EmploymentAccountStd> GetEmploymentAccounts(CompEntities entities, int employeeId)
        {
            return (from eas in entities.EmploymentAccountStd
                        .Include("AccountStd.Account")
                        .Include("AccountInternal.Account.AccountDim")
                    where eas.Employment.EmployeeId == employeeId &&
                    eas.Employment.State == (int)SoeEntityState.Active
                    select eas).ToList();
        }

        public List<EmploymentAccountStd> GetEmploymentAccountsFromEmployee(CompEntities entities, int employeeId, EmploymentAccountType employeeAccountType)
        {
            int type = (int)employeeAccountType;
            return (from eas in entities.EmploymentAccountStd
                        .Include("Employment")
                        .Include("AccountStd.Account")
                        .Include("AccountInternal.Account.AccountDim")
                    where eas.Type == type &&
                    eas.Employment.EmployeeId == employeeId &&
                    eas.Employment.State == (int)SoeEntityState.Active
                    select eas).ToList();
        }

        public EmploymentAccountStd GetEmploymentAccount(int employmentId, EmploymentAccountType employeeAccountType, DateTime? date = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmploymentAccountStd.NoTracking();
            return GetEmploymentAccount(entities, employmentId, employeeAccountType, date);
        }

        public EmploymentAccountStd GetEmploymentAccount(CompEntities entities, int employmentId, EmploymentAccountType employeeAccountType, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            int type = (int)employeeAccountType;
            var employmentAccountStds = (from eas in entities.EmploymentAccountStd
                                            .Include("AccountStd.Account")
                                            .Include("AccountInternal.Account.AccountDim")
                                            .Include("Employment")
                                         where eas.Type == type &&
                                         eas.EmploymentId == employmentId &&
                                         eas.Employment.State == (int)SoeEntityState.Active
                                         select eas).ToList();

            return employmentAccountStds.GetAccount(date.Value);
        }

        public EmploymentAccountStd GetEmploymentAccount(int employmentAccountStdId, bool loadAccounts)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmploymentAccountStd.NoTracking();
            return GetEmploymentAccount(entities, employmentAccountStdId, loadAccounts);
        }

        public EmploymentAccountStd GetEmploymentAccount(CompEntities entities, int employmentAccountStdId, bool loadAccounts)
        {
            if (loadAccounts)
            {
                return (from eas in entities.EmploymentAccountStd
                            .Include("AccountStd")
                            .Include("AccountStd.Account")
                            .Include("AccountInternal")
                            .Include("AccountInternal.Account")
                        where eas.EmploymentAccountStdId == employmentAccountStdId &&
                        eas.Employment.State == (int)SoeEntityState.Active
                        select eas).FirstOrDefault();
            }
            else
            {
                return (from eas in entities.EmploymentAccountStd
                        where eas.EmploymentAccountStdId == employmentAccountStdId &&
                        eas.Employment.State == (int)SoeEntityState.Active
                        select eas).FirstOrDefault();
            }
        }

        public EmploymentAccountStd GetEmploymentAccountFromEmployeeWithDim(int employeeId, EmploymentAccountType employeeAccountType, DateTime? date = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmploymentAccountStd.NoTracking();
            return GetEmploymentAccountFromEmployeeWithDim(entities, employeeId, employeeAccountType, date);
        }

        public EmploymentAccountStd GetEmploymentAccountFromEmployeeWithDim(CompEntities entities, int employeeId, EmploymentAccountType employeeAccountType, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            int type = (int)employeeAccountType;
            var employmentAccountStds = (from eas in entities.EmploymentAccountStd
                                            .Include("AccountStd.Account")
                                            .Include("AccountInternal.Account.AccountDim")
                                            .Include("Employment")
                                         where eas.Type == type &&
                                         eas.Employment.EmployeeId == employeeId &&
                                         eas.Employment.State == (int)SoeEntityState.Active
                                         select eas).ToList();

            return employmentAccountStds.GetAccount(date.Value);
        }

        public (EmploymentAccountStd Cost, EmploymentAccountStd Income) GetEmploymentAccountCostAndIncome(CompEntities entities, int employmentId, DateTime date)
        {
            var employmentAccountStds = (from eas in entities.EmploymentAccountStd
                                            .Include("AccountStd.Account")
                                            .Include("AccountInternal.Account")
                                            .Include("Employment")
                                         where (eas.Type == (int)EmploymentAccountType.Cost || eas.Type == (int)EmploymentAccountType.Income) &&
                                         eas.EmploymentId == employmentId
                                         select eas).ToList();

            return (
                employmentAccountStds.Where(eas => eas.Type == (int)EmploymentAccountType.Cost).GetAccount(date),
                employmentAccountStds.Where(eas => eas.Type == (int)EmploymentAccountType.Income).GetAccount(date)
            );
        }

        public Account GetAccountFromEmploymentAccountStd(EmploymentAccountStd employmentAccountStd)
        {
            Account account = null;

            if (employmentAccountStd != null)
            {
                if (!employmentAccountStd.AccountStdReference.IsLoaded)
                    employmentAccountStd.AccountStdReference.Load();

                if (employmentAccountStd.AccountStd != null)
                {
                    if (!employmentAccountStd.AccountStd.AccountReference.IsLoaded)
                        employmentAccountStd.AccountStd.AccountReference.Load();

                    account = employmentAccountStd.AccountStd.Account;
                }
            }

            return account;
        }

        public List<AccountInternal> GetAccountInternalsFromEmploymentAccountStd(EmploymentAccountStd employmentAccountStd)
        {
            var accountInternals = new List<AccountInternal>();

            if (employmentAccountStd != null)
            {
                if (!employmentAccountStd.AccountInternal.IsLoaded)
                    employmentAccountStd.AccountInternal.Load();

                if (employmentAccountStd.AccountInternal != null)
                {
                    foreach (AccountInternal accountInternal in employmentAccountStd.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                            accountInternal.AccountReference.Load();
                    }

                    accountInternals.AddRange(employmentAccountStd.AccountInternal.ToList());
                }
            }

            return accountInternals.OrderBy(i => i.Account.Created).ToList();
        }

        public (string AccountNr, List<string> AccountInternalNames) GetAccountStdInfo(EmploymentAccountStd employmentAccount)
        {
            if (employmentAccount == null)
                return (null, null);

            string accountNr = EmployeeManager.GetAccountFromEmploymentAccountStd(employmentAccount)?.AccountNr;
            List<string> accountInternals = EmployeeManager.GetAccountInternalsFromEmploymentAccountStd(employmentAccount)
                .Where(a => a.Account != null)
                .Select(a => a.Account.Name)
                .ToList();

            return (accountNr, accountInternals);
        }

        #endregion

        #region EmploymentVacationGroup

        public List<EmploymentVacationGroup> GetEmploymentVacationGroups(int employmentId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmploymentVacationGroup.NoTracking();
            return GetEmploymentVacationGroups(entities, employmentId);
        }

        public List<EmploymentVacationGroup> GetEmploymentVacationGroups(CompEntities entities, int employmentId)
        {
            return (from evg in entities.EmploymentVacationGroup
                        .Include("VacationGroup")
                    where evg.EmploymentId == employmentId &&
                    evg.State == (int)SoeEntityState.Active
                    select evg).ToList();
        }

        public Dictionary<int, string> GetEmploymentVacationGroupsDict(int employmentId, bool addEmptyRow = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<EmploymentVacationGroup> employmentVacationGroups = GetEmploymentVacationGroups(employmentId);
            foreach (EmploymentVacationGroup employmentVacationGroup in employmentVacationGroups)
            {
                if (dict.ContainsKey(employmentVacationGroup.VacationGroupId))
                    dict.Add(employmentVacationGroup.VacationGroupId, employmentVacationGroup.VacationGroup.Name);
            }

            return dict;
        }

        private void CreateEmploymentVacationGroups(Employment employment, EmploymentDTO employmentInput, DateTime? batchCreated = null)
        {
            if (employment == null || employmentInput?.EmploymentVacationGroup == null)
                return;

            foreach (EmploymentVacationGroupDTO input in employmentInput.EmploymentVacationGroup.Where(e => e.VacationGroupId > 0 && (e.State == SoeEntityState.Active || e.State == SoeEntityState.Hidden)).ToList())
            {
                EmploymentVacationGroup.Create(employment, input.VacationGroupId, input.FromDate, GetUserDetails(), batchCreated);
            }
        }

        private void SetEmploymentVacationGroups(Employment employment, EmploymentDTO employmentInput, bool doDeleteVacationGroups, DateTime? batchCreated = null)
        {
            if (employment == null || employmentInput?.EmploymentVacationGroup == null)
                return;
            if (!employmentInput.IsActiveOrHidden())
                return;

            if (!employment.EmploymentVacationGroup.IsLoaded)
                employment.EmploymentVacationGroup.Load();

            List<int> handledEmploymentVacationGroupIds = new List<int>();
            List<EmploymentVacationGroup> employmentVacationGroupsExisting = employment.EmploymentVacationGroup.Where(e => e.State == (int)SoeEntityState.Active || e.State == (int)SoeEntityState.Hidden).ToList();

            foreach (EmploymentVacationGroupDTO employmentVacationGroupInput in employmentInput.EmploymentVacationGroup.Where(e => e.VacationGroupId > 0 && (e.State == SoeEntityState.Active || e.State == SoeEntityState.Hidden)).ToList())
            {
                if (employmentVacationGroupInput.EmploymentVacationGroupId <= 0)
                {
                    EmploymentVacationGroup.Create(employment, employmentVacationGroupInput.VacationGroupId, employmentVacationGroupInput.FromDate, GetUserDetails(), batchCreated);
                }
                else
                {
                    EmploymentVacationGroup employmentVacationGroup = employmentVacationGroupsExisting.FirstOrDefault(e => e.EmploymentVacationGroupId == employmentVacationGroupInput.EmploymentVacationGroupId);
                    if (employmentVacationGroup != null)
                    {
                        employmentVacationGroup.Update(employmentVacationGroupInput.VacationGroupId, employmentVacationGroupInput.FromDate, GetUserDetails());
                        handledEmploymentVacationGroupIds.Add(employmentVacationGroup.EmploymentVacationGroupId);
                    }
                }
            }

            if (doDeleteVacationGroups)
            {
                foreach (EmploymentVacationGroup employmentVacationGroup in employmentVacationGroupsExisting.Where(e => e.EmploymentVacationGroupId > 0))
                {
                    if (!handledEmploymentVacationGroupIds.Contains(employmentVacationGroup.EmploymentVacationGroupId))
                        ChangeEntityState(employmentVacationGroup, SoeEntityState.Deleted);
                }
            }
        }

        #endregion

        #region EmployeeListDTO

        /// <summary>
        /// Get validEmployees for specified company, used in TimeSchedulePlanning
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="roleId">Logged in role ID</param>
        /// <param name="userId">Logged in user ID</param>
        /// <param name="employeeIds">If specified, only load these validEmployees</param>
        /// <param name="categoryIds">If specified, only load validEmployees with these categories</param>
        /// <param name="getHidden">If true, hidden emp is returned</param>
        /// <param name="loadSkills">If true, skills are loaded</param>
        /// <param name="loadAvailability">If true, availability is loaded</param>
        /// <param name="loadImage">If true, emp image is loaded</param>
        /// <param name="loadScheduledTimeSummary">If true schedule time is loaded</param>
        /// <param name="loadSchedules">If true, emp schedules and template schedules are loaded</param>
        /// <param name="dateFrom">Date range start, used for employments, template schedules and placements</param>
        /// <param name="dateTo">Date range stop, used for employments, template schedules and placements</param>
        /// <param name="addNoReplacementEmployee">If true, add no replacement emp</param>
        /// <returns></returns>
        public List<EmployeeListDTO> GetEmployeeList(int actorCompanyId, int roleId, int userId, List<int> employeeIds, List<int> categoryIds, bool getHidden = false, bool getInactive = false, bool loadSkills = false, bool loadAvailability = false, bool loadImage = false, bool loadScheduledTimeSummary = false, bool loadSchedules = false, DateTime? dateFrom = null, DateTime? dateTo = null, bool useOnlySpecifiedDateInterval = false, bool addNoReplacementEmployee = false, bool ignoreAttestRoleDates = false, int mandatoryEmployeeId = 0, bool includeSecondaryCategoriesOrAccounts = false, TimeSchedulePlanningDisplayMode displayMode = TimeSchedulePlanningDisplayMode.Admin, bool excludeCurrentUserEmployee = false)
        {
            List<EmployeeListDTO> employeeList = new List<EmployeeListDTO>();

            #region Prereq

            if (!useOnlySpecifiedDateInterval)
            {
                dateFrom = CalendarUtility.GetFirstDateOfYear(dateFrom);
                dateTo = CalendarUtility.GetLastDateOfYear(dateTo);
            }
            else
            {
                if (!dateFrom.HasValue)
                    dateFrom = DateTime.Today;
                if (!dateTo.HasValue)
                    dateTo = DateTime.Today;
            }
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            List<int> employeeGroupsWithPlanningPeriods = TimePeriodManager.EmployeeGroupsWithRuleWorkTimePeriods(actorCompanyId, dateFrom.Value, dateTo.Value);
            List<PayrollGroup> payrollGroups = GetPayrollGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            List<PayrollPriceType> payrollPriceTypes = GetPayrollPriceTypesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));

            // Account hierarchy
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);

            // Rounding
            int clockRounding = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningClockRounding, 0, actorCompanyId, 0);

            // Load EmployeeTypes with SettingOnly
            bool loadEmploymentTypesSettingOnly = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.IncludeSecondaryEmploymentInWorkTimeWeek, 0, base.ActorCompanyId, 0, true);

            // Get hidden emp ID
            int hiddenEmployeeId = 0;
            if (getHidden)
                hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));

            // If employeeIds is not specified, load all validEmployees in company
            entitiesReadOnly.Employee.NoTracking();
            if (employeeIds == null)
                employeeIds = GetAllEmployeeIds(actorCompanyId, getInactive ? (bool?)null : true, getHidden: getHidden);

            Dictionary<string, List<CompanyCategoryRecord>> categoryRecordsFullKeyDict = null;
            Dictionary<string, List<CompanyCategoryRecord>> categoryRecordsKeyDict = null;
            Employee currentEmployee = GetEmployeeForUser(entitiesReadOnly, userId, actorCompanyId);

            if (useAccountHierarchy)
            {
                bool onlyDefaultAccounts = !includeSecondaryCategoriesOrAccounts;

                AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
                input.AddParamValue(AccountHierarchyParamType.IgnoreAttestRoleDates, ignoreAttestRoleDates);
                input.AddParamValue(AccountHierarchyParamType.OnlyDefaultAccounts, onlyDefaultAccounts);
                input.AddParamValue(AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole, true);
                employeeIds = GetValidEmployeeByAccountHierarchy(entitiesReadOnly, actorCompanyId, roleId, userId, employeeIds, currentEmployee, dateFrom.Value, dateTo.Value, getHidden: getHidden, useShowOtherEmployeesPermission: true, addAccountHierarchyInfo: false, onlyDefaultAccounts: onlyDefaultAccounts, ignoreAttestRoles: displayMode == TimeSchedulePlanningDisplayMode.User, input: input);
            }
            else
            {
                // Get all categories in company
                var categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, actorCompanyId);
                categoryRecordsKeyDict = CategoryManager.GetCompanyCategoryRecordsRecordKeyDict(categoryRecords, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, actorCompanyId);
                categoryRecordsFullKeyDict = CategoryManager.GetCompanyCategoryRecordsFullKeyDict(categoryRecords, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, actorCompanyId);

                // If categoryIds is not specified, get categories for users role
                if (categoryIds == null)
                {
                    // If looking at myself, set isAdmin to false
                    bool isAdmin = true;
                    if (employeeIds.Count == 1 && UserManager.GetUserIdByEmployeeId(entitiesReadOnly, employeeIds[0], actorCompanyId) == userId)
                        isAdmin = false;
                    Dictionary<int, string> categories = CategoryManager.GetCategoriesForRoleFromTypeDict(actorCompanyId, userId, 0, SoeCategoryType.Employee, isAdmin, true, false);
                    categoryIds = categories.Select(c => c.Key).ToList();
                    if (categoryIds.Any())
                    {
                        var filteredRecordIds = categoryRecords.Where(w => categoryIds.Contains(w.CategoryId)).Select(s => s.RecordId);
                        employeeIds = employeeIds.Where(w => filteredRecordIds.Contains(w)).ToList();
                    }
                }
            }

            if (excludeCurrentUserEmployee && currentEmployee != null)
                employeeIds = employeeIds.Where(e => e != currentEmployee.EmployeeId).ToList();

            // Make sure hidden emp is included
            if (getHidden && !employeeIds.Contains(hiddenEmployeeId))
                employeeIds.Add(hiddenEmployeeId);

            // Make sure mandatory emp is included
            if (mandatoryEmployeeId != 0 && !employeeIds.Contains(mandatoryEmployeeId))
                employeeIds.Add(mandatoryEmployeeId);

            if (addNoReplacementEmployee)
            {
                employeeList.Add(new EmployeeListDTO()
                {
                    EmployeeId = Constants.NO_REPLACEMENT_EMPLOYEEID,
                    FirstName = GetText(8262, "Ingen ersättare")
                });
            }

            #endregion

            List<Employee> employees = new List<Employee>();
            Dictionary<int, List<EmployeeAccount>> employeeAccounts = new Dictionary<int, List<EmployeeAccount>>();
            Dictionary<int, List<EmployeeSkill>> employeeSkills = new Dictionary<int, List<EmployeeSkill>>();
            Dictionary<int, List<EmployeeRequest>> employeeRequests = new Dictionary<int, List<EmployeeRequest>>();
            Dictionary<int, List<EmployeeSchedule>> employeeSchedules = new Dictionary<int, List<EmployeeSchedule>>();
            Dictionary<int, List<Employment>> employments = new Dictionary<int, List<Employment>>();
            List<EmploymentType> employmentTypesSettingOnly = new List<EmploymentType>();

            Parallel.Invoke(() =>
            {
                using (CompEntities eentities = new CompEntities())
                {
                    IQueryable<Employee> query = eentities.Employee.Include("ContactPerson").Include("Employment");
                    if (loadSchedules)
                        query.Include("TimeScheduleTemplateHead");

                    if (employeeIds.Count < 1000)
                    {
                        // If running this on multiple thousands of validEmployees, the following exception occurs:
                        // SqlException: The query processor ran out of internal resources and could not produce a query plan.
                        // This is a rare event and only expected for extremely complex queries or queries that reference a very large number of tables or partitions.
                        // Please simplify the query.

                        employees = (from e in query
                                     where e.ActorCompanyId == actorCompanyId &&
                                     employeeIds.Contains(e.EmployeeId)
                                     select e).ToList();
                    }
                    else
                    {
                        employees = (from e in query
                                     where e.ActorCompanyId == actorCompanyId
                                     select e).ToList();
                        employees = employees.Where(e => employeeIds.Contains(e.EmployeeId)).ToList();
                    }

                }
            },
            () =>
            {
                using (CompEntities eentities = new CompEntities())
                {
                    DateTime fr = dateFrom.Value;
                    DateTime to = dateTo.Value;
                    employments = (from e in eentities.Employment
                                        .Include("EmploymentChangeBatch.EmploymentChange")
                                        .Include("OriginalEmployeeGroup")
                                        .Include("TimeHibernatingAbsenceHead")
                                   where employeeIds.Contains(e.EmployeeId) &&
                                   (e.State == (int)SoeEntityState.Active || e.State == (int)SoeEntityState.Hidden) &&
                                   (!e.DateFrom.HasValue || e.DateFrom.Value <= to) &&
                                   (!e.DateTo.HasValue || e.DateTo.Value >= fr)
                                   select e).ToList().GroupBy(g => g.EmployeeId).ToDictionary(o => o.Key, o => o.ToList()); //NOSONAR
                }
            },
            () =>
            {
                if (useAccountHierarchy)
                    using (CompEntities eentities = new CompEntities())
                    {
                        employeeAccounts = eentities.EmployeeAccount.Include("Children").Where(w => employeeIds.Contains(w.EmployeeId) && w.State == (int)SoeEntityState.Active).ToList().GroupBy(g => g.EmployeeId).ToDictionary(o => o.Key, o => o.ToList()); //NOSONAR
                    }
            },
            () =>
            {
                if (loadSkills)
                    using (CompEntities eentities = new CompEntities())
                    {
                        employeeSkills = eentities.EmployeeSkill.Include("Skill.SkillType").Where(w => employeeIds.Contains(w.EmployeeId)).ToList().GroupBy(g => g.EmployeeId).ToDictionary(o => o.Key, o => o.ToList()); //NOSONAR
                    }
            },
            () =>
            {
                if (loadAvailability)
                    using (CompEntities eentities = new CompEntities())
                    {
                        employeeRequests = eentities.EmployeeRequest.Where(w => employeeIds.Contains(w.EmployeeId) && w.State == (int)SoeEntityState.Active).ToList().GroupBy(g => g.EmployeeId).ToDictionary(o => o.Key, o => o.ToList()); //NOSONAR
                    }
            },
            () =>
            {
                if (loadSchedules)
                    using (CompEntities entities = new CompEntities())
                    {
                        employeeSchedules = entities.EmployeeSchedule.Include("TimeScheduleTemplateHead").Where(w => employeeIds.Contains(w.EmployeeId) && w.State == (int)SoeEntityState.Active).ToList().GroupBy(g => g.EmployeeId).ToDictionary(o => o.Key, o => o.ToList()); //NOSONAR
                    }
            },
            () =>
            {
                if (loadEmploymentTypesSettingOnly)
                    using (CompEntities entities = new CompEntities())
                    {
                        employmentTypesSettingOnly = entities.EmploymentType.Where(t => t.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment && t.State == (int)SoeEntityState.Active).ToList();
                    }
            });

            foreach (Employee employee in employees)
            {
                int employeeId = employee.EmployeeId;
                bool isValid = false;
                if (useAccountHierarchy)
                {
                    #region Filter on account hierarchy

                    // GetValidEmployeeByAccountHierarchy only checks accounts not dates
                    // Check that emp account dates are within specified year
                    // Only check main level and for the whole year, more detailed checks are made on the client
                    if (employeeAccounts.TryGetValue(employeeId, out List<EmployeeAccount> values))
                    {
                        foreach (EmployeeAccount account in values)
                        {
                            if (account.DateFrom <= dateTo.Value && (!account.DateTo.HasValue || account.DateTo.Value >= dateFrom.Value))
                            {
                                isValid = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        isValid = true;
                    }

                    #endregion
                }
                else
                {
                    #region Filter on categories

                    if (categoryIds.IsNullOrEmpty())
                    {
                        isValid = true;
                    }
                    else
                    {
                        if (getHidden && employeeId == hiddenEmployeeId)
                        {
                            // Get hidden even if not in current category
                            isValid = true;
                        }
                        else
                        {
                            // Check if any of the specified categories exist
                            foreach (int categoryId in categoryIds)
                            {
                                isValid = CategoryManager.GetCompanyCategoryRecordsFromEmployeeKeyDict(categoryRecordsFullKeyDict, employeeId, categoryId, discardDateIfEmpty: true).Any();
                                if (isValid)
                                    break;
                            }
                        }
                    }

                    #endregion
                }

                if (!isValid)
                    continue;

                SetEmployeeHibernatingText(employee, dateFrom.Value, dateTo.Value);

                EmployeeListDTO dto = CreateEmployeeListDTO(employee);

                // Employments
                if (employments.TryGetValue(employeeId, out List<Employment> employeeEmployments))
                    AddEmploymentsToEmployeeListDTO(dto, employee, employeeEmployments, dateFrom.Value, dateTo.Value, employeeGroups, payrollGroups, payrollPriceTypes, employmentTypesSettingOnly);
                if (dto.Employments.IsNullOrEmpty() && mandatoryEmployeeId != employeeId)
                    continue;

                // Gender
                AddGenderImageToEmployeeListDTO(dto);
                // Accounts/Categories
                if (useAccountHierarchy)
                {
                    if (employeeAccounts.TryGetValue(employeeId, out List<EmployeeAccount> empAccounts))
                        AddEmployeeAccountsToEmployeeListDTO(dto, empAccounts, dateFrom.Value, dateTo.Value);
                }
                else
                {
                    AddCategoriesToEmployeeListDTO(dto, employee, categoryRecordsKeyDict);
                }
                // Skills
                if (loadSkills && employeeSkills.TryGetValue(employeeId, out List<EmployeeSkill> empSkills))
                    AddSkillsToEmployeeListDTO(dto, empSkills);
                // Availability
                if (loadAvailability && employeeRequests.TryGetValue(employeeId, out List<EmployeeRequest> empRequests))
                    AddAvailabilityToEmployeeListDTO(dto, empRequests);
                // Image
                if (loadImage)
                    AddImageToEmployeeListDTO(entitiesReadOnly, actorCompanyId, dto, employeeId);
                // Scheduled time summary
                if (loadScheduledTimeSummary)
                    AddScheduledTimeSummaryToEmployeeListDTO(entitiesReadOnly, actorCompanyId, dto, employee, employeeEmployments, employeeGroupsWithPlanningPeriods, dateFrom.Value, clockRounding);
                // Placements
                if (loadSchedules && employeeSchedules.TryGetValue(employeeId, out List<EmployeeSchedule> empSchedules))
                    AddEmployeeSchedulesToEmployeeListDTO(dto, empSchedules, dateFrom.Value);

                employeeList.Add(dto);
            }

            // Template schedules
            if (loadSchedules)
                AddTemplateSchedulesToEmployeeListDTO(employeeList, employees, dateFrom.Value, dateTo.Value, actorCompanyId);

            return employeeList.OrderByDescending(e => e.Hidden).ThenBy(e => e.Vacant).ThenBy(e => e.Name).ToList();
        }

        public List<EmployeeListSmallDTO> GetEmployeeListSmall(int actorCompanyId, int roleId, int userId, DateTime dateFrom, DateTime dateTo, bool includeSecondary = true, bool employedInCurrentYear = true, List<int> employeeIds = null)
        {
            List<EmployeeListSmallDTO> employeeList = new List<EmployeeListSmallDTO>();

            #region Prereq

            DateTime yearDateFrom = employedInCurrentYear ? CalendarUtility.GetFirstDateOfYear(dateFrom) : dateFrom;
            DateTime yearDateTo = employedInCurrentYear ? CalendarUtility.GetLastDateOfYear(dateTo) : dateTo;
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            // Account hierarchy
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);

            // Get hidden emp ID
            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(actorCompanyId));

            List<CompanyCategoryRecord> categoryRecords = null;
            List<int> categoryIds = null;
            if (useAccountHierarchy)
            {
                if (employeeIds == null)
                {
                    // Load all validEmployees in company
                    employeeIds = (from e in entitiesReadOnly.Employee
                                   where e.ActorCompanyId == actorCompanyId &&
                                   !e.Hidden &&
                                   e.State == (int)SoeEntityState.Active
                                   select e.EmployeeId).ToList();

                    // Get validEmployees based on account hierarchy
                    Employee currentEmployee = GetEmployeeForUser(entitiesReadOnly, userId, actorCompanyId);
                    employeeIds = GetValidEmployeeByAccountHierarchy(entitiesReadOnly, actorCompanyId, roleId, userId, employeeIds, currentEmployee, dateFrom, dateTo, useShowOtherEmployeesPermission: true, addAccountHierarchyInfo: false, onlyDefaultAccounts: false);

                    // Always show hidden emp
                    if (hiddenEmployeeId > 0)
                        employeeIds.Add(hiddenEmployeeId);
                }
            }
            else
            {
                // Get all categories in company
                categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, actorCompanyId);

                // Get categories for users role
                Dictionary<int, string> categories = CategoryManager.GetCategoriesForRoleFromTypeDict(actorCompanyId, userId, 0, SoeCategoryType.Employee, true, includeSecondary, false);
                categoryIds = categories.Select(c => c.Key).ToList();
            }


            #endregion

            // Load all validEmployees in company
            entitiesReadOnly.Employee.NoTracking();
            IQueryable<Employee> query = entitiesReadOnly.Employee
                    .Include("ContactPerson")
                    .Include("Employment");

            if (useAccountHierarchy)
                query = query.Include("EmployeeAccount.Children");

            if (useAccountHierarchy || !employeeIds.IsNullOrEmpty())
                query = query.Where(e => employeeIds.Contains(e.EmployeeId));

            List<Employee> employees = (from e in query
                                        where e.ActorCompanyId == actorCompanyId &&
                                        e.State == (int)SoeEntityState.Active
                                        select e).ToList();

            foreach (var employee in employees)
            {
                bool isValid = true;
                if (!useAccountHierarchy && !categoryIds.IsNullOrEmpty())
                {
                    #region Filter on categories

                    if (employee.EmployeeId == hiddenEmployeeId)
                    {
                        // Get hidden even if not in current category
                        isValid = true;
                    }
                    else
                    {
                        isValid = false;

                        // Check if any of the specified categories exist
                        foreach (int categoryId in categoryIds)
                        {
                            isValid = categoryRecords.GetCategoryRecords(employee.EmployeeId, categoryId, discardDateIfEmpty: true).Any();
                            if (isValid)
                                break;
                        }
                    }

                    #endregion
                }

                if (isValid)
                {
                    List<Employment> employments = employee.GetEmployments(yearDateFrom, yearDateTo);
                    if (employments.Any())
                    {
                        EmployeeListSmallDTO dto = CreateEmployeeListSmallDTO(employee);

                        if (useAccountHierarchy && employee.EmployeeAccount != null)
                            dto.Accounts = employee.EmployeeAccount.Where(a => a.State == (int)SoeEntityState.Active && !a.ParentEmployeeAccountId.HasValue).ToDTOs().ToList();

                        employeeList.Add(dto);
                    }
                }
            }

            return employeeList.OrderByDescending(e => e.Hidden).ThenBy(e => e.Vacant).ThenBy(e => e.Name).ToList();
        }

        public List<EmployeeListDTO> GetEmployeeListAvailability(int actorCompanyId, List<int> employeeIds)
        {
            List<EmployeeListDTO> employeeList = new List<EmployeeListDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            #region Prereq

            // If employeeIds is not specified, load all validEmployees in company
            entitiesReadOnly.Employee.NoTracking();
            if (employeeIds == null)
                employeeIds = (from e in entitiesReadOnly.Employee
                               where e.ActorCompanyId == actorCompanyId &&
                               !e.Hidden &&
                               e.State == (int)SoeEntityState.Active
                               select e.EmployeeId).ToList();

            #endregion

            if (employeeIds.IsNullOrEmpty())
                return employeeList;

            entitiesReadOnly.EmployeeRequest.NoTracking();
            List<Employee> employees = (from e in entitiesReadOnly.Employee.Include("EmployeeRequest")
                                        where e.ActorCompanyId == actorCompanyId &&
                                        employeeIds.Contains(e.EmployeeId) &&
                                        e.EmployeeRequest.Any()
                                        select e).ToList();

            foreach (var employee in employees)
            {
                #region Availability

                if (employee.EmployeeRequest != null)
                {
                    List<EmployeeRequest> requests = employee.EmployeeRequest.Where(r => (r.Start >= DateTime.Today || r.Stop >= DateTime.Today) && r.State == (int)SoeEntityState.Active && (r.Type == (int)TermGroup_EmployeeRequestType.InterestRequest || r.Type == (int)TermGroup_EmployeeRequestType.NonInterestRequest)).ToList();
                    if (requests.Any())
                    {
                        // Create Employee DTO
                        EmployeeListDTO dto = new EmployeeListDTO()
                        {
                            EmployeeId = employee.EmployeeId,
                            Available = new List<EmployeeListAvailabilityDTO>(),
                            Unavailable = new List<EmployeeListAvailabilityDTO>()
                        };

                        foreach (EmployeeRequest request in requests)
                        {
                            EmployeeListAvailabilityDTO range = new EmployeeListAvailabilityDTO(request.Start, request.Stop, request.Comment);
                            if (request.Type == (int)TermGroup_EmployeeRequestType.InterestRequest)
                                dto.Available.Add(range);
                            else if (request.Type == (int)TermGroup_EmployeeRequestType.NonInterestRequest)
                                dto.Unavailable.Add(range);
                        }

                        employeeList.Add(dto);
                    }
                }

                #endregion
            }

            return employeeList;
        }

        private EmployeeListDTO CreateEmployeeListDTO(Employee employee)
        {
            return new EmployeeListDTO()
            {
                EmployeeId = employee.EmployeeId,
                EmployeeNr = employee.EmployeeNr,
                FirstName = employee.ContactPerson.FirstName,
                LastName = employee.ContactPerson.LastName,
                Sex = (TermGroup_Sex)employee.ContactPerson.Sex,
                Active = employee.State == (int)SoeEntityState.Active,
                Hidden = employee.Hidden,
                Vacant = employee.Vacant,
                HibernatingText = employee.HibernatingText
            };
        }

        private EmployeeListSmallDTO CreateEmployeeListSmallDTO(Employee employee)
        {
            EmployeeListSmallDTO dto = null;

            if (employee != null)
            {
                dto = new EmployeeListSmallDTO()
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeNr = employee.EmployeeNr,
                    FirstName = employee.ContactPerson.FirstName,
                    LastName = employee.ContactPerson.LastName,
                    Hidden = employee.Hidden,
                    Vacant = employee.Vacant,
                    UserId = employee.UserId
                };
            }

            return dto;
        }

        private void AddEmploymentsToEmployeeListDTO(EmployeeListDTO dto, Employee employee, List<Employment> employments, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups, List<PayrollGroup> payrollGroups, List<PayrollPriceType> payrollPriceTypes, List<EmploymentType> employmentTypesSettingOnly)
        {
            dto.Employments = new List<EmployeeListEmploymentDTO>();

            foreach (Employment employment in employments.Where(w => w.State == (int)SoeEntityState.Active).GetEmployments(dateFrom, dateTo).SortByDateAndTemporaryPrimary())
            {
                decimal latestPercent = employment.OriginalPercent;
                int latestWorkTimeWeek = employment.OriginalWorkTimeWeek;
                int latestEmployeeGroupId = employment.OriginalEmployeeGroupId;

                TryAddEmployment(employment.DateFrom, employment.DateTo, employment.IsTemporaryPrimary);

                if (employee.Hidden)
                    continue;

                DateTime? lastDateTo = employment.DateTo;
                DateTime? prevBatchFromDate = null;
                DateTime? prevBatchToDate = null;
                List<int> prevBatchFieldTypeChanges = new List<int>();

                foreach (EmploymentChangeBatch batch in employment.GetOrderedEmploymentChangeBatches())
                {
                    bool oneChangeChecked = false;

                    List<EmploymentChange> dateChanges = batch.GetInformationChanges(TermGroup_EmploymentChangeFieldType.DateFrom, TermGroup_EmploymentChangeFieldType.DateTo);
                    
                    List<EmploymentChange> changes = batch.GetDataChanges(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, TermGroup_EmploymentChangeFieldType.Percent, TermGroup_EmploymentChangeFieldType.EmployeeGroupId);

                    if (!prevBatchFromDate.HasValue || prevBatchFromDate != batch.FromDate || prevBatchToDate != batch.ToDate)
                    {
                        if (changes.Any(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek))
                            latestWorkTimeWeek = int.Parse(changes.FirstOrDefault(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek).FromValue);
                        if (changes.Any(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.Percent))
                            latestPercent = NumberUtility.ToDecimal(changes.FirstOrDefault(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.Percent).FromValue);
                        if (changes.Any(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmployeeGroupId))
                        {
                            int empGroupId = int.Parse(changes.FirstOrDefault(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmployeeGroupId).FromValue);
                            if (empGroupId > 0)
                                latestEmployeeGroupId = empGroupId;
                        }
                    }

                    foreach (EmploymentChange change in changes)
                    {
                        if (prevBatchFromDate.HasValue && prevBatchFromDate == batch.FromDate && prevBatchFieldTypeChanges.Contains(change.FieldType) || oneChangeChecked)
                            continue;

                        DateTime? fromDate = change.EmploymentChangeBatch.FromDate;
                        DateTime? toDate = CalendarUtility.GetEarliestDate(change.EmploymentChangeBatch.ToDate, employment.DateTo);

                        prevBatchFromDate = batch.FromDate;
                        prevBatchToDate = batch.ToDate;
                        DateTime? prevDateTo = dto.Employments.Last().DateTo;
                        bool adjustPrevious = false;
                        bool skipAdd = false;
                        bool needsSorting = false;

                        if (change.EmploymentChangeBatch.FromDate.HasValue)
                        {
                            DateTime newEndDate = change.EmploymentChangeBatch.FromDate.Value.AddDays(-1);
                            if (!prevDateTo.HasValue || prevDateTo.Value >= change.EmploymentChangeBatch.FromDate)
                            {
                                // Previous change generated 2 segments which need some adjustments
                                if (dto.Employments.Count > 1 && change.EmploymentChangeBatch.FromDate < dto.Employments.Last().DateFrom && dto.Employments.Last().DateTo.HasValue)
                                {
                                    dto.Employments[dto.Employments.Count - 2].DateTo = newEndDate;
                                    toDate = dto.Employments.Last().DateFrom.Value.AddDays(-1);
                                    adjustPrevious = true;
                                }
                                // Finish previous employment one day before current change                                
                                else if ((employment.DateFrom.HasValue && employment.DateFrom.Value < newEndDate) || !dto.Employments.Last().DateTo.HasValue)
                                    dto.Employments.Last().DateTo = newEndDate;

                                // change has same start default as employment
                                else if (employment.DateFrom == fromDate)
                                {
                                    skipAdd = true;
                                    bool skipAddChange = false;
                                    if (toDate == dto.Employments.Last().DateTo)
                                        skipAddChange = true;


                                    if (!prevBatchFieldTypeChanges.Contains(change.FieldType))
                                    {
                                        dto.Employments.Last().DateTo = toDate;

                                        // Handle all fieldTypeChanges spanning whole employment right here as they will be added directly to employment
                                        List<EmploymentChange> fieldTypeChanges = changes.Where(c => c.EmploymentChangeBatch.FromDate == change.EmploymentChangeBatch.FromDate && c.EmploymentChangeBatch.ToDate == change.EmploymentChangeBatch.ToDate).ToList();

                                        foreach (EmploymentChange fieldTypeChange in fieldTypeChanges)
                                        {
                                            if (fieldTypeChange.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek)
                                            {
                                                int workTimeWeekMinutes = 0;
                                                Int32.TryParse(fieldTypeChange.ToValue, out workTimeWeekMinutes);
                                                dto.Employments.Last().WorkTimeWeekMinutes = workTimeWeekMinutes;
                                            }
                                            else if (fieldTypeChange.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmployeeGroupId)
                                            {
                                                int employeeGroupId = 0;
                                                Int32.TryParse(fieldTypeChange.ToValue, out employeeGroupId);
                                                dto.Employments.Last().EmployeeGroupId = employeeGroupId;
                                            }
                                            else if (fieldTypeChange.FieldType == (int)TermGroup_EmploymentChangeFieldType.Percent)
                                            {
                                                decimal percent = 0;
                                                Decimal.TryParse(fieldTypeChange.ToValue.Replace('.', ','), out percent);
                                                dto.Employments.Last().Percent = percent;
                                            }
                                        }
                                    }
                                    if (!skipAddChange)
                                        TryAddEmployment(toDate.Value.AddDays(1), employment.DateTo, false);
                                }
                            }
                            else if (prevDateTo <= newEndDate)
                            {
                                //Handle hole between changes
                                TryAddEmployment(prevDateTo.Value.AddDays(1), newEndDate, false, changes);
                            }
                        }

                        if (!skipAdd)
                            TryAddEmployment(fromDate, toDate, false, changes);

                        if (needsSorting)
                            dto.Employments = dto.Employments.OrderBy(e => e.DateFrom).ToList();

                        // Remove previous with the same datespan as the current change
                        if (dto.Employments.Count > 1)
                        {
                            if (dto.Employments.Last().DateFrom == dto.Employments[dto.Employments.Count - 2].DateFrom && dto.Employments.Last().DateTo == dto.Employments[dto.Employments.Count - 2].DateTo)
                                dto.Employments.Remove(dto.Employments[dto.Employments.Count - 2]);
                        }

                        if (adjustPrevious)
                        {
                            lastDateTo = dto.Employments[dto.Employments.Count - 2].DateTo;
                            if (!changes.IsNullOrEmpty() && !changes.Any(x => x.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmployeeGroupId))
                                dto.Employments.Last().EmployeeGroupId = dto.Employments[dto.Employments.Count - 3].EmployeeGroupId;
                            if (!changes.IsNullOrEmpty() && !changes.Any(x => x.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek))
                                dto.Employments.Last().WorkTimeWeekMinutes = dto.Employments[dto.Employments.Count - 3].WorkTimeWeekMinutes;
                            if (!changes.IsNullOrEmpty() && !changes.Any(x => x.FieldType == (int)TermGroup_EmploymentChangeFieldType.Percent))
                                dto.Employments.Last().Percent = dto.Employments[dto.Employments.Count - 3].Percent;

                            dto.Employments.Remove(dto.Employments[dto.Employments.Count - 2]);
                        }

                        if (!lastDateTo.HasValue || lastDateTo < toDate)
                            lastDateTo = toDate;

                        // If employment changes are within active employment, add a new employment with same values as active employment after last change
                        if (lastDateTo.HasValue && (!employment.DateTo.HasValue || (employment.DateTo.HasValue && employment.DateTo.Value > lastDateTo.Value)) && dto.Employments.Last().DateTo.HasValue)
                        {
                            toDate = employment.DateTo;
                            TryAddEmployment(lastDateTo.Value.AddDays(1), toDate);
                            lastDateTo = toDate;
                        }

                        oneChangeChecked = true;
                    }
                    if (changes.Any())
                        prevBatchFieldTypeChanges = changes.Select(c => c.FieldType).ToList();

                    // check dateFrom/dateTo changes and apply them to the employments
                    // This solution is not perfect. It will just change the dateFrom/dateTo on the first employment found with that date, and if segments with dateFrom > dateTo exists it will be removed further down.
                    if (dateChanges.Any())
                    {
                        foreach (EmploymentChange dateChange in dateChanges.OrderBy(c => c.EmploymentChangeBatch?.Created))
                        {
                            // from date changes
                            if (dateChange.FieldType == (int)TermGroup_EmploymentChangeFieldType.DateFrom)
                            {
                                if (DateTime.TryParse(dateChange.FromValue, out DateTime dateFromOld) && DateTime.TryParse(dateChange.ToValue, out DateTime dateFromNew))
                                {
                                    EmployeeListEmploymentDTO empItem = dto.Employments.FirstOrDefault(e => e.DateFrom == dateFromOld);
                                    if (empItem != null)
                                        empItem.DateFrom = dateFromNew;
                                }
                            }
                            // to date changes
                            else if (dateChange.FieldType == (int)TermGroup_EmploymentChangeFieldType.DateTo)
                            {
                                if (DateTime.TryParse(dateChange.FromValue, out DateTime dateToOld) && DateTime.TryParse(dateChange.ToValue, out DateTime dateToNew))
                                {
                                    EmployeeListEmploymentDTO empItem = dto.Employments.FirstOrDefault(e => e.DateTo == dateToOld);
                                    if (empItem != null)
                                        empItem.DateTo = dateToNew;
                                }
                            }
                        }
                    }
                }

                // If employment changes are within active employment, add a new employment with same values as active employment after last change
                if (lastDateTo.HasValue && (!employment.DateTo.HasValue || (employment.DateTo.HasValue && employment.DateTo.Value > lastDateTo.Value)) && dto.Employments.Last().DateTo.HasValue)
                {
                    if (!dto.Employments.Last().DateTo.HasValue)
                        dto.Employments.Last().DateTo = lastDateTo;
                    TryAddEmployment(lastDateTo.Value.AddDays(1), employment.DateTo);
                }

                // Make sure to fill with employments if lastDateTo has Value and no employments has that default. This could happen when changes exist and 
                if (lastDateTo.HasValue && dto.Employments.All(s => s.DateTo.HasValue) && !dto.Employments.Any(s => s.DateTo.Value == lastDateTo))
                {
                    DateTime? previousLastDate = dto.Employments.Where(w => w.DateTo.HasValue).OrderByDescending(o => o.DateTo.Value).FirstOrDefault()?.DateTo;
                    if (previousLastDate != null)
                    {
                        TryAddEmployment(previousLastDate.Value.AddDays(1), lastDateTo);

                        // Ugly fix. Assume that the previous agreement should take over 
                        if (dto.Employments.Count > 2)
                        {
                            // Check valid default ranges (maybe has to check more steps back)
                            if (dto.Employments[dto.Employments.Count - 3].DateFrom <= dto.Employments[dto.Employments.Count - 3].DateTo)
                            {
                                dto.Employments.Last().EmployeeGroupId = dto.Employments[dto.Employments.Count - 3].EmployeeGroupId;
                                dto.Employments.Last().WorkTimeWeekMinutes = dto.Employments[dto.Employments.Count - 3].WorkTimeWeekMinutes;
                                dto.Employments.Last().Percent = dto.Employments[dto.Employments.Count - 3].Percent;
                            }
                            else if (dto.Employments.Count > 3)
                            {
                                dto.Employments.Last().EmployeeGroupId = dto.Employments[dto.Employments.Count - 4].EmployeeGroupId;
                                dto.Employments.Last().WorkTimeWeekMinutes = dto.Employments[dto.Employments.Count - 4].WorkTimeWeekMinutes;
                                dto.Employments.Last().Percent = dto.Employments[dto.Employments.Count - 4].Percent;
                            }
                        }
                    }
                }

                void TryAddEmployment(DateTime? employmentDateFrom, DateTime? employmentDateTo, bool isTemporaryPrimary = false, List<EmploymentChange> changes = null)
                {
                    EmployeeListEmploymentDTO emp = CreateEmployeeListEmploymentDTO(employmentDateFrom, employmentDateTo, employee.Hidden, isTemporaryPrimary, employment, employeeGroups, payrollGroups, payrollPriceTypes, latestEmployeeGroupId);
                    bool hasChanges = !changes.IsNullOrEmpty();
                    bool hasEmployeeGroupChanges = hasChanges && changes.Any(x => x.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmployeeGroupId);
                    bool hasWorkTimeWeekChanges = hasChanges && changes.Any(x => x.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek);
                    bool hasPercentChanges = hasChanges && changes.Any(x => x.FieldType == (int)TermGroup_EmploymentChangeFieldType.Percent);

                    if (hasEmployeeGroupChanges && emp.EmployeeGroupId.HasValidValue())
                        latestEmployeeGroupId = emp.EmployeeGroupId.ToInt();
                    // Not sure we want to do this
                    //else
                    //    emp.EmployeeGroupId = latestEmployeeGroupId;

                    if (hasWorkTimeWeekChanges)
                        latestWorkTimeWeek = emp.WorkTimeWeekMinutes;
                    // Not sure we want to do this
                    //else
                    //    emp.WorkTimeWeekMinutes = latestWorkTimeWeek;

                    if (hasPercentChanges)
                        latestPercent = emp.Percent;
                    else
                        emp.Percent = latestPercent;

                    if (!ExistsEmployeeListEmployment(dto.Employments, emp))
                        dto.Employments.Add(emp);
                }
            }

            // Remove invalid default ranges
            foreach (EmployeeListEmploymentDTO employment in dto.Employments.ToList())
            {
                if (employment.DateFrom.HasValue && employment.DateTo.HasValue && employment.DateTo < employment.DateFrom)
                    dto.Employments.Remove(employment);
            }
            if (SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.IncludeSecondaryEmploymentInWorkTimeWeek, 0, base.ActorCompanyId, 0, true))
                AdjustForSecondaryEmployments(employments.GetEmployments(dateFrom.AddYears(-2), dateTo, includeSecondary: true), dto, dateFrom, dateTo, employmentTypesSettingOnly);
        }

        private void AdjustForSecondaryEmployments(List<Employment> employments, EmployeeListDTO employeeListDTO, DateTime dateFrom, DateTime dateTo, List<EmploymentType> employmentTypesSettingOnly)
        {
            var secondaryEmployments = employments.Where(e => e.IsSecondaryEmployment).ToList();

            if (!secondaryEmployments.Any())
                return;

            secondaryEmployments = secondaryEmployments.Where(w => w.DateFrom <= dateTo && (!w.DateTo.HasValue || w.DateTo >= dateFrom)).ToList();

            if (!secondaryEmployments.Any())
                return;

            var primaryEmployments = employeeListDTO.Employments;

            List<EmployeeListEmploymentDTO> secondaryEmploymentDTOs = new List<EmployeeListEmploymentDTO>();
            // Get all secondary Employments, and split on changes
            foreach (Employment employment in secondaryEmployments.OrderBy(f => f.DateFrom))
            {
                // Get setting for excluding worktimeweek, get it from employment type if not set on the employment
                bool excludeFromWorkTimeWeekCalculationOnType = false;
                bool? excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = employment.GetExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(employment.DateFrom);
                if (excludeFromWorkTimeWeekCalculationOnSecondaryEmployment == null)
                {
                    int employmentType = employment.GetEmploymentType(employment.DateFrom);
                    if (EmploymentTypeDTO.IsStandard(employmentType))
                        excludeFromWorkTimeWeekCalculationOnType = employmentTypesSettingOnly.FirstOrDefault(t => t.Type == employmentType && t.SettingOnly)?.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment ?? false;
                    else
                        excludeFromWorkTimeWeekCalculationOnType = employmentTypesSettingOnly.FirstOrDefault(t => t.EmploymentTypeId == employmentType)?.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment ?? false;
                }
                bool excludeFromWorkTimeWeekCalculation = excludeFromWorkTimeWeekCalculationOnSecondaryEmployment == null ? excludeFromWorkTimeWeekCalculationOnType : excludeFromWorkTimeWeekCalculationOnSecondaryEmployment.Value;

                var dto = employment.ToListDTO();
                dto.IsSecondaryEmployment = true;
                dto.WorkTimeWeekMinutes = !excludeFromWorkTimeWeekCalculation ? employment.GetWorkTimeWeek(employment.DateFrom) : 0;
                secondaryEmploymentDTOs.Add(dto);

                if (!dto.DateTo.HasValue)
                    dto.DateTo = dateTo;

                foreach (EmploymentChangeBatch batch in employment.GetOrderedEmploymentChangeBatches())
                {
                    bool excludeFromWorkTimeWeekCalculationOnTypeChange = false;
                    bool excludeFromWorkTimeWeekCalculationChange = false;

                    List<EmploymentChange> changes = batch.GetDataChanges(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, TermGroup_EmploymentChangeFieldType.EmploymentType, TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment);

                    // check new setting value for employment type
                    if (changes.Any(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmploymentType))
                    {
                        var employmentTypeChange = changes.Last(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmploymentType);
                        excludeFromWorkTimeWeekCalculationOnTypeChange = employmentTypesSettingOnly.FirstOrDefault(t => t.Type == int.Parse(employmentTypeChange.ToValue))?.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment ?? false;
                        excludeFromWorkTimeWeekCalculationChange = excludeFromWorkTimeWeekCalculationOnTypeChange;
                    }
                    // check new value for excluding worktimeweek
                    if (changes.Any(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment))
                    {
                        var excludeChange = changes.Last(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment);
                        if (string.IsNullOrWhiteSpace(excludeChange.ToValue))
                            excludeFromWorkTimeWeekCalculationChange = excludeFromWorkTimeWeekCalculationOnTypeChange;
                        else
                            excludeFromWorkTimeWeekCalculationChange = bool.Parse(excludeChange.ToValue);
                    }

                    // Pick one set of changes according to prio in the same batch (to avoiding duplicate segments)
                    var newChanges = changes.Where(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek);
                    if (newChanges.IsNullOrEmpty())
                        newChanges = changes.Where(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment);
                    if (newChanges.IsNullOrEmpty())
                        newChanges = changes.Where(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmploymentType);


                    foreach (EmploymentChange change in newChanges)
                    {
                        DateTime? fromDate = change.EmploymentChangeBatch.FromDate;
                        DateTime? toDate = CalendarUtility.GetEarliestDate(change.EmploymentChangeBatch.ToDate, employment.DateTo);

                        if (fromDate != employment.DateFrom)
                        {
                            dto.DateTo = fromDate.Value.AddDays(-1);

                            var clone = employment.ToListDTO();

                            int newWorkTimeMinutes = change.FieldType == (int)TermGroup_EmploymentChangeFieldType.WorkTimeWeek ? int.Parse(change.ToValue) : employment.GetWorkTimeWeek(fromDate);

                            clone.WorkTimeWeekMinutes = !excludeFromWorkTimeWeekCalculationChange ? newWorkTimeMinutes : 0;
                            clone.DateFrom = fromDate;
                            clone.DateTo = toDate;
                            clone.IsSecondaryEmployment = true;
                            secondaryEmploymentDTOs.Add(clone);

                            if (clone.DateTo != toDate)
                            {
                                var cloneAfter = employment.ToListDTO();
                                cloneAfter.WorkTimeWeekMinutes = dto.WorkTimeWeekMinutes;
                                cloneAfter.DateTo = toDate.Value.Date.AddDays(1);
                                cloneAfter.IsSecondaryEmployment = true;
                                secondaryEmploymentDTOs.Add(cloneAfter);
                            }
                        }
                    }
                }
            }

            var filteredList = new List<EmployeeListEmploymentDTO>();
            foreach (EmployeeListEmploymentDTO employment in secondaryEmploymentDTOs)
            {
                if (employment.DateFrom.HasValue && employment.DateTo.HasValue && employment.DateTo < employment.DateFrom)
                    continue;

                filteredList.Add(employment);
            }

            filteredList = filteredList.OrderBy(e => e.DateFrom).ToList();

            if (!filteredList.Any())
                return;

            filteredList.AddRange(primaryEmployments);

            // Create unique employments for each default based on WorkTimeWeek changes
            List<EmployeeListEmploymentDTO> uniqueEmployments = new List<EmployeeListEmploymentDTO>();

            // Fix for employments without a start default
            foreach (EmployeeListEmploymentDTO empl in filteredList.Where(g => !g.DateFrom.HasValue).ToList())
            {
                empl.DateFrom = CalendarUtility.DATETIME_DEFAULT;
            }

            var startDate = new List<DateTime>() { dateFrom, filteredList.OrderBy(g => g.DateFrom.Value).First().DateFrom.Value }.OrderBy(f => f).FirstOrDefault();
            var stopDate = new List<DateTime>() { dateTo, filteredList.OrderByDescending(g => g.DateTo ?? DateTime.MinValue).First().DateTo.Value }.OrderByDescending(f => f).FirstOrDefault();

            if (stopDate > dateTo)
                stopDate = dateTo;

            DateTime currentDate = startDate;

            while (currentDate <= stopDate)
            {
                // Find all employments that are active on the current default
                var activeEmployments = filteredList
                    .Where(e => e.DateFrom <= currentDate && (!e.DateTo.HasValue || e.DateTo >= currentDate))
                    .ToList();

                if (activeEmployments.Count > 0)
                {
                    // Sum the work week times for employments on the current default
                    int totalWorkTimeWeek = activeEmployments.Sum(e => e.WorkTimeWeekMinutes);
                    var primaryEmployment = activeEmployments.FirstOrDefault(w => !w.IsSecondaryEmployment);

                    if (primaryEmployment != null)
                    {
                        var clone = primaryEmployment.CloneDTO();
                        clone.DateFrom = currentDate;
                        clone.DateTo = currentDate;
                        clone.WorkTimeWeekMinutes = totalWorkTimeWeek;
                        uniqueEmployments.Add(clone);
                    }
                }

                // Move to the next default
                currentDate = currentDate.AddDays(1);
            }

            // Merge the unique employments where WorkTimeWeek is the same
            List<EmployeeListEmploymentDTO> mergedEmployments = new List<EmployeeListEmploymentDTO>();
            EmployeeListEmploymentDTO currentMerge = null;

            foreach (var employment in uniqueEmployments.OrderBy(e => e.DateFrom))
            {
                if (currentMerge == null)
                {
                    currentMerge = employment;
                }
                else
                {
                    // If the work week time changes, finalize the current merge
                    if (currentMerge.WorkTimeWeekMinutes != employment.WorkTimeWeekMinutes)
                    {
                        mergedEmployments.Add(currentMerge);
                        currentMerge = employment;
                    }
                    else
                    {
                        // Extend the current employment if the work week time is the same
                        currentMerge.DateTo = employment.DateTo;
                    }
                }
            }

            // Add the last merge if not null
            if (currentMerge != null)
            {
                mergedEmployments.Add(currentMerge);
            }


            employeeListDTO.Employments = mergedEmployments;
        }

        private EmployeeListEmploymentDTO CreateEmployeeListEmploymentDTO(DateTime? dateFrom, DateTime? dateTo, bool isHidden, bool isTemporaryPrimary, Employment employment, List<EmployeeGroup> employeeGroups, List<PayrollGroup> payrollGroups, List<PayrollPriceType> payrollPriceTypes, int? latestEmployeeGroupId = null)
        {
            EmployeeListEmploymentDTO dto = new EmployeeListEmploymentDTO()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                IsTemporaryPrimary = isTemporaryPrimary,
            };

            EmployeeGroup employeeGroup = employment.GetEmployeeGroup(dateFrom, employeeGroups, latestEmployeeGroupId);
            if (employeeGroup != null)
            {
                dto.EmployeeGroupId = employeeGroup.EmployeeGroupId;
                dto.EmployeeGroupName = employeeGroup.Name;
                dto.BreakDayMinutesAfterMidnight = employeeGroup.BreakDayMinutesAfterMidnight;
                dto.AllowShiftsWithoutAccount = employeeGroup.AllowShiftsWithoutAccount;
                dto.ExtraShiftAsDefault = employeeGroup.ExtraShiftAsDefault;

                dto.AnnualLeaveGroupId = employment.GetAnnualLeaveGroupId(dateFrom);
            }

            if (!isHidden)
            {
                EmploymentDTO employmentDTO = employment.ApplyEmploymentChanges(dateFrom, employeeGroups, payrollGroups, payrollPriceTypes);
                if (employmentDTO != null)
                {
                    dto.WorkTimeWeekMinutes = employmentDTO.WorkTimeWeek;
                    dto.Percent = employmentDTO.Percent;
                    if (employeeGroup != null)
                    {
                        dto.MinScheduleTime = employmentDTO.Percent == 100 ? employeeGroup.MinScheduleTimeFullTime : employeeGroup.MinScheduleTimePartTime;
                        dto.MaxScheduleTime = employmentDTO.Percent == 100 ? employeeGroup.MaxScheduleTimeFullTime : employeeGroup.MaxScheduleTimePartTime;
                    }
                }
            }

            return dto;
        }

        private bool ExistsEmployeeListEmployment(List<EmployeeListEmploymentDTO> employments, EmployeeListEmploymentDTO employment)
        {
            return employments?.Any(e =>
                e.DateFrom == employment.DateFrom &&
                e.DateTo == employment.DateTo &&
                e.WorkTimeWeekMinutes == employment.WorkTimeWeekMinutes &&
                e.Percent == employment.Percent &&
                e.MinScheduleTime == employment.MinScheduleTime &&
                e.MaxScheduleTime == employment.MaxScheduleTime &&
                e.EmployeeGroupId == employment.EmployeeGroupId &&
                e.EmployeeGroupName == employment.EmployeeGroupName &&
                e.BreakDayMinutesAfterMidnight == employment.BreakDayMinutesAfterMidnight &&
                e.AllowShiftsWithoutAccount == employment.AllowShiftsWithoutAccount) ?? false;
        }

        private void AddGenderImageToEmployeeListDTO(EmployeeListDTO dto)
        {
            switch (dto.Sex)
            {
                case TermGroup_Sex.Unknown:
                    dto.ImageSource = "user";
                    break;
                case TermGroup_Sex.Male:
                    dto.ImageSource = "male";
                    break;
                case TermGroup_Sex.Female:
                    dto.ImageSource = "female";
                    break;
            }
        }

        private void AddEmployeeAccountsToEmployeeListDTO(EmployeeListDTO dto, List<EmployeeAccount> employeeAccounts, DateTime dateFrom, DateTime dateTo)
        {
            // Add active parent accounts within specified default interval
            if (!employeeAccounts.IsNullOrEmpty())
                dto.Accounts = employeeAccounts.Where(a => !a.ParentEmployeeAccountId.HasValue && (!a.DateTo.HasValue || a.DateTo.Value >= dateFrom) && a.DateFrom <= dateTo).ToDTOs().ToList();
        }

        private void AddEmployeeAccountChildrenToEmployeeListDTO(List<Account> allAccounts, EmployeeListDTO dto, EmployeeAccountDTO employeeAccount)
        {
            List<Account> children = (from a in allAccounts
                                      where a.ParentAccountId == employeeAccount.AccountId &&
                                      a.State == (int)SoeEntityState.Active
                                      select a).ToList();
            foreach (Account child in children)
            {
                EmployeeAccountDTO childEmployeeAccount = new EmployeeAccountDTO()
                {
                    AccountId = child.AccountId,
                    EmployeeId = employeeAccount.EmployeeId,
                    DateFrom = employeeAccount.DateFrom,
                    DateTo = employeeAccount.DateTo,
                    Default = employeeAccount.Default
                };
                dto.Accounts.Add(childEmployeeAccount);

                // Add recursive
                AddEmployeeAccountChildrenToEmployeeListDTO(allAccounts, dto, childEmployeeAccount);
            }
        }

        private void AddCategoriesToEmployeeListDTO(EmployeeListDTO dto, Employee employee, Dictionary<string, List<CompanyCategoryRecord>> employeeKeyDict)
        {
            string key = CompanyCategoryRecord.ConstructKey((int)SoeCategoryRecordEntity.Employee, employee.EmployeeId);
            employeeKeyDict.TryGetValue(key, out List<CompanyCategoryRecord> records);
            if (records != null)
                dto.CategoryRecords = records.GetCategoryRecords(employee.EmployeeId, discardDateIfEmpty: true).ToDTOs(false).ToList();
        }

        private void AddSkillsToEmployeeListDTO(EmployeeListDTO dto, List<EmployeeSkill> employeeSkills)
        {
            if (employeeSkills != null)
                dto.EmployeeSkills = employeeSkills.ToDTOs(false);
        }

        private void AddAvailabilityToEmployeeListDTO(EmployeeListDTO dto, List<EmployeeRequest> employeeRequests)
        {
            if (!employeeRequests.IsNullOrEmpty())
            {
                dto.AbsenceRequest = new List<DateRangeDTO>();
                dto.AbsenceApproved = new List<DateRangeDTO>();
                dto.Available = new List<EmployeeListAvailabilityDTO>();
                dto.Unavailable = new List<EmployeeListAvailabilityDTO>();
                if (employeeRequests.Any(r => (r.Start >= DateTime.Today || r.Stop >= DateTime.Today) && (r.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest || r.Type == (int)TermGroup_EmployeeRequestType.InterestRequest || r.Type == (int)TermGroup_EmployeeRequestType.NonInterestRequest)))
                {
                    foreach (EmployeeRequest request in employeeRequests.Where(r => (r.Start >= DateTime.Today || r.Stop >= DateTime.Today)).OrderBy(r => r.Start).ThenBy(r => r.Stop))
                    {
                        if (request.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest)
                        {
                            DateRangeDTO range = new DateRangeDTO(request.Start, request.Stop);
                            if (request.ResultStatus == (int)TermGroup_EmployeeRequestResultStatus.FullyGranted)
                                dto.AbsenceApproved.Add(range);
                            else
                                dto.AbsenceRequest.Add(range);
                        }
                        else if (request.Type == (int)TermGroup_EmployeeRequestType.InterestRequest)
                        {
                            dto.Available.Add(new EmployeeListAvailabilityDTO(request.Start, request.Stop, request.Comment));
                        }
                        else if (request.Type == (int)TermGroup_EmployeeRequestType.NonInterestRequest)
                        {
                            dto.Unavailable.Add(new EmployeeListAvailabilityDTO(request.Start, request.Stop, request.Comment));
                        }
                    }
                }
            }
        }

        private void AddImageToEmployeeListDTO(CompEntities entities, int actorCompanyId, EmployeeListDTO dto, int employeeId)
        {
            Images image = GraphicsManager.GetImage(entities, actorCompanyId, SoeEntityImageType.EmployeePortrait, SoeEntityType.Employee, employeeId, true);
            if (image != null)
                dto.Image = image.Thumbnail;
        }

        private void AddScheduledTimeSummaryToEmployeeListDTO(CompEntities entities, int actorCompanyId, EmployeeListDTO dto, Employee employee, List<Employment> employments, List<int> employeeGroupsWithPlanningPeriods, DateTime dateFrom, int clockRounding)
        {
            DateTime yearDateFrom = CalendarUtility.GetFirstDateOfYear(dateFrom);
            DateTime yearDateTo = CalendarUtility.GetLastDateOfYear(dateFrom);

            dto.AnnualScheduledTimeMinutes = TimeScheduleManager.GetScheduledTimeSummaryTotalWithinEmployments(entities, actorCompanyId, employee, yearDateFrom, yearDateTo, TimeScheduledTimeSummaryType.Both, employments);
            dto.AnnualWorkTimeMinutes = GetAnnualWorkTimeMinutes(yearDateFrom, yearDateTo, employee, employeeGroupsWithPlanningPeriods, clockRounding);
        }

        private void AddEmployeeSchedulesToEmployeeListDTO(EmployeeListDTO dto, List<EmployeeSchedule> employeeSchedules, DateTime dateFrom)
        {
            dto.EmployeeSchedules = new List<DateRangeDTO>();
            if (employeeSchedules != null)
            {
                foreach (EmployeeSchedule schedule in employeeSchedules.Where(s => (s.StartDate >= dateFrom || s.StopDate >= dateFrom)))
                {
                    dto.EmployeeSchedules.Add(new DateRangeDTO(schedule.StartDate, schedule.StopDate));
                }
            }
        }

        private void AddTemplateSchedulesToEmployeeListDTO(List<EmployeeListDTO> dtos, List<Employee> employees, DateTime dateFrom, DateTime dateTo, int actorCompanyId)
        {
            List<TimeScheduleTemplateHead> templatesHeads = employees.SelectMany(s => s.TimeScheduleTemplateHead).ToList();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            TimeScheduleTemplateHeadsRangeDTO rangeDTO = TimeScheduleManager.GetTimeScheduleTemplateHeadsRangeForEmployees(entitiesReadOnly, employees.Select(s => s.EmployeeId).ToList(), actorCompanyId, CalendarUtility.GetBeginningOfYear(dateFrom).AddYears(-1), CalendarUtility.GetEndOfYear(dateTo), false, 0, templatesHeads);

            foreach (var employeeHeads in rangeDTO.Heads.Where(s => s.NoOfDays > 0).GroupBy(g => g.EmployeeId))
            {
                EmployeeListDTO dto = dtos.FirstOrDefault(f => f.EmployeeId == employeeHeads.Key);
                if (dto != null)
                {
                    foreach (var head in employeeHeads.OrderByDescending(h => h.StartDate))
                    {
                        if (dto.TemplateSchedules == null)
                            dto.TemplateSchedules = new List<TimeScheduleTemplateHeadSmallDTO>();

                        dto.TemplateSchedules.Add(new TimeScheduleTemplateHeadSmallDTO()
                        {
                            Name = head.TemplateName ?? dto.Name,
                            StartDate = head.StartDate,
                            StopDate = head.StopDate,
                            EmployeeId = head.EmployeeId,
                            NoOfDays = head.NoOfDays,
                            FirstMondayOfCycle = head.FirstMondayOfCycle,
                            VirtualStopDate = head.StopDate,
                            TimeScheduleTemplateHeadId = head.TimeScheduleTemplateHeadId,
                            TimeScheduleTemplateGroupId = head.TimeScheduleTemplateGroupId,
                            TimeScheduleTemplateGroupName = head.TimeScheduleTemplateGroupName
                        });
                    }
                }
            }
        }

        public List<EmployeeListDTO> GetEmployeesForDefToFromPrelShiftDialog(bool prelToDef, DateTime dateFrom, DateTime dateTo, int actorCompanyId, int employeeId, int roleId, int userId, List<int> filteredEmployeeIds)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            // Account hierarchy
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);

            // Get users primary categories
            Dictionary<int, string> primaryCategories = null;
            if (!useAccountHierarchy)
                primaryCategories = CategoryManager.GetCategoriesForRoleFromTypeDict(actorCompanyId, userId, employeeId, SoeCategoryType.Employee, true, false, false);

            // Get validEmployees with shifts in specified interval
            List<int> employeeIds = TimeScheduleManager.GetEmployeeIdsWithShifts(actorCompanyId, dateFrom, dateTo, preliminary: prelToDef, isTemplate: false, includeZeroSchedule: true, filteredEmployeeIds);
            if (filteredEmployeeIds != null)
                employeeIds = employeeIds.Where(e => filteredEmployeeIds.Contains(e)).ToList();

            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(actorCompanyId));

            // Get all validEmployees within primary categories
            List<EmployeeListDTO> employeeList = GetEmployeeList(actorCompanyId, roleId, userId, employeeIds, primaryCategories?.Keys.ToList(), getHidden: filteredEmployeeIds.Contains(hiddenEmployeeId), dateFrom: dateFrom, dateTo: dateTo, useOnlySpecifiedDateInterval: true);

            // Filter validEmployees (remove validEmployees not in specified list)
            return employeeList;
        }

        #endregion

        #region EmployeePosition

        public List<EmployeePosition> GetEmployeePositionsForCompany(int actorCompanyId, bool loadSysPosition = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeePosition.NoTracking();
            return GetEmployeePositionsForCompany(entities, actorCompanyId, loadSysPosition);
        }

        public List<EmployeePosition> GetEmployeePositionsForCompany(CompEntities entities, int actorCompanyId, bool loadSysPosition = false)
        {
            var employeePositions = (from e in entities.EmployeePosition.Include("Position")
                                     where e.Position.ActorCompanyId == actorCompanyId &&
                                     e.Position.State == (int)SoeEntityState.Active
                                     orderby e.Position.Name
                                     select e).ToList();

            if (loadSysPosition)
            {
                List<SysPosition> sysPositions = GetAllSysPositions();
                foreach (var employeePosition in employeePositions)
                {
                    if (!employeePosition.Position.SysPositionId.HasValue)
                        continue;

                    var sysPosition = sysPositions.FirstOrDefault(s => s.SysPositionId == employeePosition.Position.SysPositionId.Value);
                    if (sysPosition != null)
                    {
                        employeePosition.Position.SysPositionCode = sysPosition.Code;
                        employeePosition.Position.SysPositionName = sysPosition.Name;
                        employeePosition.Position.SysPositionDescription = sysPosition.Description;
                    }
                }
            }

            return employeePositions;
        }

        public List<EmployeePosition> GetEmployeePositions(int employeeId, bool loadSysPosition = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeePosition.NoTracking();
            return GetEmployeePositions(entities, employeeId, loadSysPosition);
        }

        public List<EmployeePosition> GetEmployeePositions(CompEntities entities, int employeeId, bool loadSysPosition = false)
        {
            int actorCompanyId = base.ActorCompanyId;
            List<EmployeePosition> employeePositions = (from e in entities.EmployeePosition.Include("Position")
                                                        where e.EmployeeId == employeeId &&
                                                        e.Employee.ActorCompanyId == actorCompanyId &&
                                                        e.Position.State == (int)SoeEntityState.Active
                                                        orderby e.Position.Name
                                                        select e).ToList();

            if (loadSysPosition && employeePositions.Any(ep => ep.Position.SysPositionId.HasValue))
            {
                List<SysPosition> sysPositions = GetAllSysPositions();
                foreach (EmployeePosition employeePosition in employeePositions.Where(ep => ep.Position.SysPositionId.HasValue))
                {
                    SysPosition sysPosition = sysPositions.FirstOrDefault(s => s.SysPositionId == employeePosition.Position.SysPositionId.Value);
                    if (sysPosition != null)
                    {
                        employeePosition.Position.SysPositionCode = sysPosition.Code;
                        employeePosition.Position.SysPositionName = sysPosition.Name;
                        employeePosition.Position.SysPositionDescription = sysPosition.Description;
                    }
                }
            }

            return employeePositions;
        }

        public List<EmployeePosition> GetEmployeePositions(List<int> employeeIds, bool loadSysPosition = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeePosition.NoTracking();
            return GetEmployeePositions(entities, employeeIds, loadSysPosition);
        }

        public List<EmployeePosition> GetEmployeePositions(CompEntities entities, List<int> employeeIds, bool loadSysPosition = false)
        {
            int actorCompanyId = base.ActorCompanyId;
            var employeePositions = (from e in entities.EmployeePosition.Include("Position")
                                     where employeeIds.Contains(e.EmployeeId) &&
                                     e.Employee.ActorCompanyId == actorCompanyId &&
                                     e.Position.State == (int)SoeEntityState.Active
                                     orderby e.Position.Name
                                     select e).ToList();

            if (loadSysPosition)
            {
                List<SysPosition> sysPositions = GetAllSysPositions();
                foreach (var employeePosition in employeePositions)
                {
                    if (!employeePosition.Position.SysPositionId.HasValue)
                        continue;

                    var sysPosition = sysPositions.FirstOrDefault(s => s.SysPositionId == employeePosition.Position.SysPositionId.Value);
                    if (sysPosition != null)
                    {
                        employeePosition.Position.SysPositionCode = sysPosition.Code;
                        employeePosition.Position.SysPositionName = sysPosition.Name;
                        employeePosition.Position.SysPositionDescription = sysPosition.Description;
                    }
                }
            }

            return employeePositions;
        }

        public ActionResult SaveEmployeePositions(CompEntities entities, TransactionScope transaction, List<EmployeePositionDTO> employeePositionsInput, Employee employee)
        {
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Employee");
            if (employeePositionsInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeePosition");

            // Make sure EmployeePosition is loaded
            if (!employee.EmployeePosition.IsLoaded)
                employee.EmployeePosition.Load();

            EmployeePosition employeePosition = null;

            #region Update/Delete EmployeePosition

            List<EmployeePosition> employeePositions = employee.EmployeePosition.ToList();
            for (int r = employeePositions.Count - 1; r >= 0; r--)
            {
                employeePosition = employeePositions[r];

                EmployeePositionDTO employeePositionInput = employeePositionsInput.FirstOrDefault(p => p.PositionId == employeePosition.PositionId);
                if (employeePositionInput != null)
                {
                    // Update values
                    employeePosition.Default = employeePositionInput.Default;

                    // Remove from input collection
                    employeePositionsInput.Remove(employeePositionInput);
                }
                else
                {
                    // Delete existing
                    entities.DeleteObject(employeePosition);
                    employeePositions.Remove(employeePosition);
                }
            }

            #endregion

            #region Add EmployeePosition

            foreach (EmployeePositionDTO employeePositionInput in employeePositionsInput)
            {
                employee.EmployeePosition.Add(new EmployeePosition()
                {
                    PositionId = employeePositionInput.PositionId,
                    Default = employeePositionInput.Default,
                });
            }

            #endregion

            return SaveChanges(entities, transaction);
        }

        #endregion

        #region EmployeeSkill

        public ActionResult SaveEmployeeSkills(CompEntities entities, TransactionScope transaction, Employee employee, List<EmployeeSkillDTO> employeeSkillsInput)
        {
            // Make sure EmployeeSkill is loaded
            if (!employee.EmployeeSkill.IsLoaded)
                employee.EmployeeSkill.Load();

            // Loop through existing skills
            foreach (EmployeeSkill employeeSkill in employee.EmployeeSkill.ToList())
            {
                EmployeeSkillDTO existsInInput = employeeSkillsInput.FirstOrDefault(s => s.SkillId == employeeSkill.SkillId);
                if (existsInInput != null)
                {
                    // Skill still exists in input, update existing skill and remove from input
                    employeeSkill.SkillLevel = existsInInput.SkillLevel;
                    employeeSkill.DateTo = existsInInput.DateTo;
                    employeeSkillsInput.Remove(existsInInput);
                }
                else
                {
                    // Skill does not exist in input, remove from emp
                    employee.EmployeeSkill.Remove(employeeSkill);
                    entities.DeleteObject(employeeSkill);
                }
            }

            // Add new skills (remaining in input)
            AddEmployeeSkills(employee, employeeSkillsInput);

            return SaveChanges(entities, transaction);
        }

        private void AddEmployeeSkills(Employee employee, List<EmployeeSkillDTO> employeeSkills)
        {
            foreach (EmployeeSkillDTO employeeSkillDTO in employeeSkills)
            {
                employee.EmployeeSkill.Add(new EmployeeSkill()
                {
                    SkillId = employeeSkillDTO.SkillId,
                    SkillLevel = employeeSkillDTO.SkillLevel,
                    DateTo = employeeSkillDTO.DateTo
                });
            }
        }

        public List<EmployeeSkill> GetEndingEmployeeSkills(int actorCompanyId, DateTime endDate)
        {
            using (CompEntities entities = new CompEntities())
            {
                return (from es in entities.EmployeeSkill
                            .Include("Skill")
                            .Include("Employee.ContactPerson")
                        where es.Employee.ActorCompanyId == actorCompanyId &&
                        es.DateTo > DateTime.Now &&
                        es.DateTo < endDate
                        select es).ToList();
            }
        }

        #endregion

        #region EmployeeUnionFee

        public List<EmployeeUnionFee> GetEmployeeUnionFees(int employeeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeUnionFee.NoTracking();
            return GetEmployeeUnionFees(entities, employeeId);
        }

        public List<EmployeeUnionFee> GetEmployeeUnionFees(CompEntities entities, int employeeId)
        {
            return (from e in entities.EmployeeUnionFee
                    .Include("UnionFee")
                    where e.EmployeeId == employeeId &&
                    e.State == (int)SoeEntityState.Active
                    select e).ToList();
        }

        public ActionResult SaveEmployeeUnionFees(CompEntities entities, TransactionScope transaction, Employee employee, List<EmployeeUnionFeeDTO> employeeUnionFeeInput)
        {
            List<EmployeeUnionFee> existingEmployeeUnionFees = GetEmployeeUnionFees(entities, employee.EmployeeId);

            // Loop through existing employeeunionfees
            foreach (EmployeeUnionFee employeeUnionFee in existingEmployeeUnionFees)
            {
                EmployeeUnionFeeDTO existsInInput = employeeUnionFeeInput.Where(x => x.EmployeeUnionFeeId != 0).FirstOrDefault(s => s.EmployeeUnionFeeId == employeeUnionFee.EmployeeUnionFeeId);
                if (existsInInput != null)
                {
                    //Update
                    employeeUnionFee.UnionFeeId = existsInInput.UnionFeeId;
                    employeeUnionFee.FromDate = existsInInput.FromDate;
                    employeeUnionFee.ToDate = existsInInput.ToDate;
                    employeeUnionFee.State = (int)existsInInput.State;
                }
                else
                {
                    employeeUnionFee.State = (int)SoeEntityState.Deleted;
                }

                SetModifiedProperties(employeeUnionFee);
            }

            foreach (var newItemInput in employeeUnionFeeInput.Where(x => x.EmployeeUnionFeeId == 0 && x.State == (int)SoeEntityState.Active))
            {
                if (newItemInput.UnionFeeId == 0)
                    continue;

                var newItem = new EmployeeUnionFee()
                {
                    UnionFeeId = newItemInput.UnionFeeId,
                    FromDate = newItemInput.FromDate,
                    ToDate = newItemInput.ToDate,
                    State = (int)newItemInput.State
                };

                SetCreatedProperties(newItem);
                employee.EmployeeUnionFee.Add(newItem);
            }

            return SaveChanges(entities, transaction);
        }

        #endregion

        #region EmployeeTimeWorkAccount

        public List<EmployeeTimeWorkAccount> GetEmployeeTimeWorkAccounts(int employeeId, bool loadAccount = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeTimeWorkAccounts(entities, employeeId, loadAccount);
        }

        public List<EmployeeTimeWorkAccount> GetEmployeeTimeWorkAccounts(CompEntities entities, int employeeId, bool loadAccount = false)
        {
            int actorCompanyId = base.ActorCompanyId;

            IQueryable<EmployeeTimeWorkAccount> query = entities.EmployeeTimeWorkAccount;

            if (loadAccount)
                query = query.Include("TimeWorkAccount");

            return (from w in query
                    where w.ActorCompanyId == actorCompanyId &&
                    w.EmployeeId == employeeId &&
                    w.State == (int)SoeEntityState.Active
                    select w).ToList();
        }

        public ActionResult SaveEmployeeTimeWorkAccounts(CompEntities entities, TransactionScope transaction, Employee employee, List<EmployeeTimeWorkAccountDTO> inputEmployeeTimeWorkAccounts)
        {
            var result = ValidateEmployeeTimeWorkAccounts(inputEmployeeTimeWorkAccounts);
            if (!result.Success)
                return result;

            List<EmployeeTimeWorkAccount> existingEmployeeTimeWorkAccount = GetEmployeeTimeWorkAccounts(entities, employee.EmployeeId);
            foreach (EmployeeTimeWorkAccount employeeTimeWorkAccount in existingEmployeeTimeWorkAccount)
            {
                EmployeeTimeWorkAccountDTO inputEmployeeTimeWorkAccount = inputEmployeeTimeWorkAccounts.FirstOrDefault(e => e.EmployeeTimeWorkAccountId == employeeTimeWorkAccount.EmployeeTimeWorkAccountId);
                if (inputEmployeeTimeWorkAccount != null)
                {
                    employeeTimeWorkAccount.TimeWorkAccountId = inputEmployeeTimeWorkAccount.TimeWorkAccountId;
                    employeeTimeWorkAccount.DateFrom = inputEmployeeTimeWorkAccount.DateFrom;
                    employeeTimeWorkAccount.DateTo = inputEmployeeTimeWorkAccount.DateTo;
                    employeeTimeWorkAccount.State = (int)inputEmployeeTimeWorkAccount.State;
                }
                else
                {
                    employeeTimeWorkAccount.State = (int)SoeEntityState.Deleted;
                }
                SetModifiedProperties(employeeTimeWorkAccount);
            }

            foreach (var inputEmployeeTimeWorkAccount in inputEmployeeTimeWorkAccounts.Where(e => e.EmployeeTimeWorkAccountId == 0 && e.TimeWorkAccountId > 0))
            {
                EmployeeTimeWorkAccount employeeTimeWorkAccount = new EmployeeTimeWorkAccount()
                {
                    DateFrom = inputEmployeeTimeWorkAccount.DateFrom,
                    DateTo = inputEmployeeTimeWorkAccount.DateTo,
                    State = (int)SoeEntityState.Active,

                    //Set FK
                    TimeWorkAccountId = inputEmployeeTimeWorkAccount.TimeWorkAccountId,
                    EmployeeId = employee.EmployeeId,
                    ActorCompanyId = base.ActorCompanyId,
                };
                SetCreatedProperties(employeeTimeWorkAccount);
                employee.EmployeeTimeWorkAccount.Add(employeeTimeWorkAccount);
            }

            result = SaveChanges(entities, transaction);
            if (!result.Success)
            {
                result.ErrorNumber = (int)ActionResultSave.EmployeeTimeWorkAccountsNotSaved;
                result.ErrorMessage = GetText(11061, "Arbetstidskonton kunde inte sparas");
            }

            return result;
        }

        public ActionResult ValidateEmployeeTimeWorkAccounts(List<EmployeeTimeWorkAccountDTO> employeeTimeWorkAccounts)
        {
            employeeTimeWorkAccounts.ForEach(a => a.Key = Guid.NewGuid());
            if (employeeTimeWorkAccounts.Any(ea => employeeTimeWorkAccounts.IsOverlapping(ea.Key, ea.DateFrom, ea.DateTo)))
                return new ActionResult(false, (int)ActionResultSave.TimeWorkAccountOverlapping, GetText(91991, "Ett arbetstidskonto får inte överlappa annat arbetstidskonto"));
            return new ActionResult(true);
        }

        #endregion

        #region EmploymentEndReason

        public List<EndReasonDTO> GetEndReasons(int actorCompanyId, int? language = null, int? endReasonId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEndReasons(entities, actorCompanyId, language, endReasonId);
        }

        public List<EndReasonDTO> GetEndReasons(CompEntities entities, int actorCompanyId, int? language = null, int? endReasonId = null)
        {
            Dictionary<int, string> systemEndReasons = new Dictionary<int, string>();
            List<EndReason> companyEndReasons = GetCompanyEndReasons(entities, actorCompanyId, null, endReasonId);

            if (!endReasonId.HasValue)
                systemEndReasons = GetSystemEndReasons(entities, actorCompanyId, language);

            return companyEndReasons.ToDTOs(systemEndReasons).ToList();
        }

        public List<EndReason> GetCompanyEndReasons(int actorCompanyId, bool? active = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EndReason.NoTracking();
            return GetCompanyEndReasons(entities, actorCompanyId, active);
        }

        public List<EndReason> GetCompanyEndReasons(CompEntities entities, int actorCompanyId, bool? active = null, int? endReasonId = null)
        {
            var endReasons = (from b in entities.EndReason
                              where b.ActorCompanyId == actorCompanyId &&
                              b.State != (int)SoeEntityState.Deleted
                              select b);

            if (active == true)
                endReasons = endReasons.Where(i => i.State == (int)SoeEntityState.Active);
            else if (active == false)
                endReasons = endReasons.Where(i => i.State == (int)SoeEntityState.Inactive);

            if (endReasonId.HasValue)
                endReasons = endReasons.Where(er => er.EndReasonId == endReasonId.Value);

            return endReasons.ToList(); ;
        }

        public Dictionary<int, string> GetSystemEndReasons(int actorCompanyId, int? language = null, bool includeCompanyEndReasons = false, bool? active = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSystemEndReasons(entities, actorCompanyId, language, includeCompanyEndReasons, active);
        }

        public Dictionary<int, string> GetSystemEndReasons(CompEntities entities, int actorCompanyId, int? language = null, bool includeCompanyEndReasons = false, bool? active = null)
        {
            var validTermsDict = new Dictionary<int, string>();

            List<GenericType> terms = base.GetTermGroupContent(TermGroup.EmploymentEndReason, GetLangId(), addEmptyRow: true);
            TermGroup_Languages endReasonLang = language.HasValue ? (TermGroup_Languages)language.Value : TermGroup_Languages.Swedish;
            switch (endReasonLang)
            {
                case TermGroup_Languages.Swedish:
                    #region Swedish

                    foreach (GenericType term in terms)
                    {
                        bool use = false;

                        switch (term.Id)
                        {
                            case (int)TermGroup_EmploymentEndReason.None:
                            case (int)TermGroup_EmploymentEndReason.SE_EmploymentChanged:
                            case (int)TermGroup_EmploymentEndReason.SE_CompanyChanged:
                            case (int)TermGroup_EmploymentEndReason.SE_Deceased:
                            case (int)TermGroup_EmploymentEndReason.SE_OwnRequest:
                            case (int)TermGroup_EmploymentEndReason.SE_Retirement:
                            case (int)TermGroup_EmploymentEndReason.SE_TemporaryEmploymentEnds:
                            case (int)TermGroup_EmploymentEndReason.SE_LaidOfDueToRedundancy:
                            case (int)TermGroup_EmploymentEndReason.SE_Fired:
                                use = true;
                                break;
                        }

                        if (use)
                            validTermsDict.Add(term.Id, term.Name);
                    }

                    #endregion
                    break;
                case TermGroup_Languages.English:
                case TermGroup_Languages.Finnish:
                case TermGroup_Languages.Norwegian:
                case TermGroup_Languages.Danish:
                    foreach (GenericType term in terms.Where(t => t.Id > 0))
                    {
                        if (!validTermsDict.ContainsKey(term.Id))
                            validTermsDict.Add(term.Id, term.Name);
                    }
                    break;
            }

            if (includeCompanyEndReasons)
            {
                List<EndReason> companyEndReasons = GetCompanyEndReasons(entities, actorCompanyId, active);
                foreach (EndReason companyEndReason in companyEndReasons)
                {
                    if (!validTermsDict.ContainsKey(companyEndReason.EndReasonId))
                        validTermsDict.Add(companyEndReason.EndReasonId, companyEndReason.Name);
                }
            }

            return validTermsDict.Sort();
        }

        public EndReason GetCompanyEndReason(int actorCompanyId, int endReasonId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EndReason.NoTracking();
            return GetCompanyEndReason(entities, actorCompanyId, endReasonId);
        }

        public EndReason GetCompanyEndReason(CompEntities entities, int actorCompanyId, int endReasonId)
        {
            return (from s in entities.EndReason
                    where s.ActorCompanyId == actorCompanyId &&
                    s.EndReasonId == endReasonId
                    select s).FirstOrDefault();
        }

        private bool IsEndReasonUsed(int actorCompanyId, int endReasonId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Employment.NoTracking();
            bool usedOnEmployment = (from s in entitiesReadOnly.Employment
                                     where s.ActorCompanyId == actorCompanyId &&
                                     s.OriginalEndReason == endReasonId
                                     select s).Any();

            if (usedOnEmployment)
                return true;

            string endReasonString = endReasonId.ToString();
            entitiesReadOnly.EmploymentChange.NoTracking();
            bool usedOnEmploymentChange = (from s in entitiesReadOnly.EmploymentChange
                                           where s.EmploymentChangeBatch.ActorCompanyId == actorCompanyId &&
                                           s.FieldType == (int)TermGroup_EmploymentChangeFieldType.EmploymentEndReason &&
                                           (s.FromValue == endReasonString || s.ToValue == endReasonString)
                                           select s).Any();


            if (usedOnEmploymentChange)
                return true;

            return false;

        }

        public ActionResult SaveEndReason(EndReasonDTO endReasonInput, int actorCompanyId)
        {
            if (endReasonInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EndReason");

            ActionResult result = null;

            int endReasonId = endReasonInput.EndReasonId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region EndReason

                        EndReason endReason = GetCompanyEndReason(entities, actorCompanyId, endReasonId);
                        if (endReason == null)
                        {

                            endReason = new EndReason()
                            {
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(endReason);
                            entities.EndReason.AddObject(endReason);
                        }
                        else
                        {
                            SetModifiedProperties(endReason);
                        }

                        endReason.Name = endReasonInput.Name;
                        endReason.Code = endReasonInput.Code;
                        endReason.State = (int)endReasonInput.State;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            transaction.Complete();
                            endReasonId = endReason.EndReasonId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                }
                finally
                {
                    if (result != null && result.Success)
                        result.IntegerValue = endReasonId;
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteEndReason(int actorCompanyId, int endReasonId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                // Check relations
                if (IsEndReasonUsed(actorCompanyId, endReasonId))
                    return new ActionResult((int)ActionResultDelete.EndReasonInUse);

                #endregion

                EndReason endReason = GetCompanyEndReason(entities, actorCompanyId, endReasonId);
                if (endReason == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "EndReason");

                return ChangeEntityState(entities, endReason, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult UpdateEndReasonsState(Dictionary<int, bool> endReasons, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                foreach (KeyValuePair<int, bool> endReason in endReasons)
                {
                    EndReason originalEndReason = GetCompanyEndReason(entities, actorCompanyId, endReason.Key);
                    if (originalEndReason == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "EndReason");

                    ChangeEntityState(originalEndReason, endReason.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                }

                return SaveChanges(entities);
            }
        }

        #endregion

        #region Experience Months

        public int GetExperienceMonthsForEmployee(int actorCompanyId, int employeeId, DateTime? stopDate = null, bool useResultAsNewInputValue = true)
        {
            var employee = GetEmployee(employeeId, actorCompanyId, loadEmployment: true);
            if (employee != null)
            {
                bool useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, actorCompanyId, 0);
                Employment employment = employee.GetNearestEmployment(stopDate ?? DateTime.Today);

                DateTime dateFrom = employment.DateTo ?? DateTime.Today;
                DateTime dateTo = stopDate ?? DateTime.Today;

                if (useExperienceMonthsOnEmploymentAsStartValue && useResultAsNewInputValue && dateTo.Month == dateFrom.Month && dateTo.Year == dateFrom.Year)
                    dateTo = CalendarUtility.GetFirstDateOfMonth(dateTo).AddDays(-1);

                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                return GetExperienceMonths(entities, employee.ActorCompanyId, employment, useExperienceMonthsOnEmploymentAsStartValue, dateTo);
            }

            return 0;
        }

        public int GetExperienceMonthsForEmployment(int actorCompanyId, int employmentId, DateTime? stopDate = null)
        {
            var employment = GetEmployment(employmentId);

            if (employment != null)
            {
                var employee = GetEmployee(employment.EmployeeId, actorCompanyId, loadEmployment: true);

                if (employee != null)
                {
                    bool useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, actorCompanyId, 0);

                    using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                    return GetExperienceMonths(entities, actorCompanyId, employee.GetEmployment(employment.EmploymentId), useExperienceMonthsOnEmploymentAsStartValue, stopDate);
                }
            }

            return 0;
        }

        public int GetExperienceMonthsForPreviousEmployent(int actorCompanyId, int currentEmploymentId, bool useResultAsNewInputValue = true)
        {
            var currentEmployment = GetEmployment(currentEmploymentId);
            if (currentEmployment != null && currentEmployment.DateFrom.HasValue)
            {
                Employee employee = GetEmployee(currentEmployment.EmployeeId, actorCompanyId, loadEmployment: true);
                if (employee != null)
                {
                    Employment previousEmployment = employee.GetPrevEmployment(currentEmployment.DateFrom.Value);
                    if (previousEmployment != null)
                    {
                        bool useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, actorCompanyId, 0);

                        DateTime dateFrom = currentEmployment.DateFrom.Value;
                        DateTime dateTo = previousEmployment.DateTo ?? DateTime.Today;
                        if (useExperienceMonthsOnEmploymentAsStartValue && useResultAsNewInputValue && dateFrom.Month == dateTo.Month && dateFrom.Year == dateTo.Year)
                            dateTo = CalendarUtility.GetFirstDateOfMonth(dateTo).AddDays(-1);

                        using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                        return GetExperienceMonths(entities, actorCompanyId, previousEmployment, useExperienceMonthsOnEmploymentAsStartValue, dateTo);
                    }
                }
            }

            return 0;
        }

        public int GetExperienceMonths(CompEntities entities, int actorCompanyId, Employment employment, bool useExperienceMonthsOnEmploymentAsStartValue, DateTime? stopDate = null)
        {
            if (employment == null)
                return 0;

            DateTime calculateExperienceFrom = SettingManager.GetDateSetting(entities, SettingMainType.Company, (int)CompanySettingType.CalculateExperienceFrom, 0, actorCompanyId, 0);
            int experienceMonths = 0;

            PayrollGroupSetting setting = null;
            List<PayrollGroup> payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(actorCompanyId), loadSettings: true);
            PayrollGroup payrollGroup = employment.GetPayrollGroup(stopDate, payrollGroups);
            if (payrollGroup != null)
            {
                if (!payrollGroup.PayrollGroupSetting.IsLoaded)
                    payrollGroup.PayrollGroupSetting.Load();

                setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.ExperienceMonthsFormula);
            }

            if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
            {
                DateTime? startDate = employment.GetEmploymentDate();
                if (!startDate.HasValue)
                    return experienceMonths;

                if (calculateExperienceFrom > CalendarUtility.DATETIME_DEFAULT && calculateExperienceFrom > startDate.Value)
                    startDate = calculateExperienceFrom;

                stopDate = stopDate ?? DateTime.Today;
                if (stopDate > employment.GetEndDate())
                    stopDate = employment.GetEndDate();

                experienceMonths = employment.GetExperienceMonths(stopDate);

                if (startDate.Value > DateTime.Today)
                    return experienceMonths;

                if (startDate == CalendarUtility.DATETIME_DEFAULT)
                    startDate = new DateTime(2000, 1, 1);

                List<Tuple<DateTime, DateTime>> months = CalendarUtility.GetMonths(startDate.Value, stopDate.Value);
                foreach (Tuple<DateTime, DateTime> month in months)
                {
                    PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employment.EmployeeId, employment, null, month.Item1, null, null, setting.IntData.Value);
                    if (result != null && result.Amount == 1)
                        experienceMonths++;
                }

                return experienceMonths;
            }
            else
            {
                experienceMonths = employment.GetTotalExperienceMonths(useExperienceMonthsOnEmploymentAsStartValue, calculateExperienceFrom, stopDate);
            }

            return experienceMonths;
        }

        #endregion

        #region FollowUpType

        public FollowUpType GetFollowUpType(int actorCompanyId, int followUpTypeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.FollowUpType.NoTracking();
            return GetFollowUpType(entities, actorCompanyId, followUpTypeId);
        }

        public FollowUpType GetFollowUpType(CompEntities entities, int actorCompanyId, int followUpTypeId)
        {
            return (from s in entities.FollowUpType
                    where s.ActorCompanyId == actorCompanyId &&
                    s.FollowUpTypeId == followUpTypeId
                    select s).FirstOrDefault();
        }

        public ActionResult SaveFollowUpType(FollowUpTypeDTO followUpTypeInput, int actorCompanyId)
        {
            if (followUpTypeInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "FollowUpType");

            // Default result is successful
            ActionResult result = new ActionResult();

            int followUpTypeId = followUpTypeInput.FollowUpTypeId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region FollowUpType

                        FollowUpType followUpType = GetFollowUpType(entities, actorCompanyId, followUpTypeId);
                        if ((followUpType == null || followUpTypeInput.Name != followUpType.Name) && ExistsFollowUpType(entities, followUpTypeInput.Name, actorCompanyId, followUpType?.FollowUpTypeId ?? null))
                            return new ActionResult((int)ActionResultSave.EntityExists);

                        if (followUpType == null)
                        {
                            #region Add

                            followUpType = new FollowUpType()
                            {
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(followUpType);
                            entities.FollowUpType.AddObject(followUpType);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(followUpType);

                            #endregion
                        }

                        followUpType.Name = followUpTypeInput.Name;
                        followUpType.Type = (int)followUpTypeInput.Type;
                        followUpType.State = (int)followUpTypeInput.State;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            followUpTypeId = followUpType.FollowUpTypeId;
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
                        result.IntegerValue = followUpTypeId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteFollowUpType(int actorCompanyId, int followUpTypeId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                //Check relations
                if (FollowUpTypeIdIsUsed(entities, actorCompanyId, followUpTypeId))
                    return new ActionResult((int)ActionResultDelete.FollowUpTypeInUse);

                #endregion

                FollowUpType followUpType = GetFollowUpType(entities, actorCompanyId, followUpTypeId);
                if (followUpType == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "FollowUpType");

                return ChangeEntityState(entities, followUpType, SoeEntityState.Deleted, true);
            }
        }

        public List<FollowUpType> GetFollowUpTypes(int actorCompanyId, bool? active = null, int? followUpTypeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.FollowUpType.NoTracking();
            return GetFollowUpTypes(entities, actorCompanyId, active, followUpTypeId);
        }

        public List<FollowUpType> GetFollowUpTypes(CompEntities entities, int actorCompanyId, bool? active = null, int? followUpTypeId = null)
        {
            var followUpTypes = (from b in entities.FollowUpType
                                 where b.ActorCompanyId == actorCompanyId &&
                                       b.State != (int)SoeEntityState.Deleted
                                 select b);


            if (active == true)
                followUpTypes = followUpTypes.Where(i => i.State == (int)SoeEntityState.Active);
            else if (active == false)
                followUpTypes = followUpTypes.Where(i => i.State == (int)SoeEntityState.Inactive);

            if (followUpTypeId.HasValue)
                followUpTypes = followUpTypes.Where(ft => ft.FollowUpTypeId == followUpTypeId.Value);

            return followUpTypes.ToList();
        }

        public ActionResult UpdateFollowUpTypesState(Dictionary<int, bool> followUpTypes, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                foreach (KeyValuePair<int, bool> followUpType in followUpTypes)
                {
                    FollowUpType originalFollowUpType = GetFollowUpType(entities, actorCompanyId, followUpType.Key);
                    if (originalFollowUpType == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "FollowUpType");

                    ChangeEntityState(originalFollowUpType, followUpType.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                }

                return SaveChanges(entities);
            }
        }

        private bool FollowUpTypeIdIsUsed(CompEntities entities, int actorCompanyId, int followUpTypeId)
        {
            return (from es in entities.EmployeeMeeting
                    where es.ActorCompanyId == actorCompanyId &&
                          es.FollowUpTypeId == followUpTypeId &&
                          es.State == (int)SoeEntityState.Active
                    select es).Any();

        }

        public bool ExistsFollowUpType(CompEntities entities, string name, int actorCompanyId, int? followupTypeId = null)
        {
            IQueryable<FollowUpType> query = (from tc in entities.FollowUpType
                                              where tc.ActorCompanyId == actorCompanyId &&
                                              tc.Name.ToLower() == name.ToLower() &&
                                              tc.State == (int)SoeEntityState.Active
                                              select tc);

            if (followupTypeId.HasValue)
                query = query.Where(ft => ft.FollowUpTypeId != followupTypeId.Value);

            return query.Any();
        }

        #endregion

        #region FixedPayrollRow

        public Dictionary<int, List<EmployeeFixedPayrollRowChangesDTO>> GetEmployeeFixedPayrollRowsChanges(int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            int? onePayrollGroup = null;
            Dictionary<int, List<EmployeeFixedPayrollRowChangesDTO>> outList = new Dictionary<int, List<EmployeeFixedPayrollRowChangesDTO>>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<EmploymentTypeDTO> employmentTypes = GetEmploymentTypes(entitiesReadOnly, actorCompanyId, (TermGroup_Languages)GetLangId());
            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(actorCompanyId, loadPriceTypes: false, loadSettings: true);
            List<Employee> employees = GetAllEmployeesByIds(actorCompanyId, employeeIds, loadEmployment: true);

            foreach (Employee employee in employees)
            {
                if (employee != null && CalendarUtility.GetTotalDays(dateFrom, dateTo) > 3)
                {
                    var ids = employee.GetPayrollGroupIds(dateFrom, dateTo, payrollGroups);
                    if (!ids.IsNullOrEmpty())
                    {
                        onePayrollGroup = ids.Count == 1 ? ids.First() : (int?)null;
                    }
                }
                string employmentName = "";
                string payrollGroupName = "";
                string oldEmploymentNames = "";
                DateTime? employmentStartDate = null;
                DateTime currentDate = dateFrom;
                Employment employment = null;
                int? payrollGroupId = null;
                int? previousPayrollGroupId = null;
                int changes = 0;
                while (currentDate <= dateTo)
                {
                    if (employee != null)
                        employment = employee.GetEmployment(currentDate);
                    if (employment != null)
                    {
                        employmentStartDate = employment.DateFrom;
                        if (employmentStartDate == currentDate)
                        {
                            oldEmploymentNames = employmentName;
                            employmentName = "";
                        }

                        if (employmentName == "")
                            employmentName = $"{employment.GetEmploymentTypeName(employmentTypes, employment.DateFrom)}";
                        else
                        {
                            if (!employmentName.Contains(employment.GetEmploymentTypeName(employmentTypes, employment.DateFrom)))
                                employmentName += $", {employment.GetEmploymentTypeName(employmentTypes, employment.DateFrom)}";
                        }

                        if (onePayrollGroup.HasValue)
                            payrollGroupId = onePayrollGroup.Value;
                        else
                            payrollGroupId = employment.GetPayrollGroupId(currentDate);
                        if (payrollGroupId.HasValue && payrollGroupId != previousPayrollGroupId)
                        {
                            if (changes > 0 && employee != null && outList.ContainsKey(employee.EmployeeId))
                            {
                                List<EmployeeFixedPayrollRowChangesDTO> oldItems = outList[employee.EmployeeId].Where(w => w.FromPayrollGroup && w.FixedPayrollRowId == changes - 1).ToList();
                                foreach (EmployeeFixedPayrollRowChangesDTO oldItem in oldItems)
                                {
                                    oldItem.ToDate = currentDate.AddDays(-1);
                                    oldItem.EmploymentTypeName = oldEmploymentNames;
                                }
                                oldEmploymentNames = "";
                            }
                            payrollGroupName = payrollGroups.FirstOrDefault(w => w.PayrollGroupId == payrollGroupId)?.Name ?? string.Empty;
                            List<FixedPayrollRowDTO> payrollRowsFromPayrollGroup = GetPayrollGroupEmployeeFixedPayrollRows(payrollGroupId.Value);
                            foreach (var row in payrollRowsFromPayrollGroup)
                            {
                                EmployeeFixedPayrollRowChangesDTO item = new EmployeeFixedPayrollRowChangesDTO
                                {
                                    ProductNr = row.ProductNr,
                                    ProductName = row.ProductName,
                                    PayrollGroupName = payrollGroupName,
                                    FixedPayrollRowId = changes,
                                    EmployeeId = employee.EmployeeId,
                                    FromDate = currentDate,
                                    ToDate = dateTo,
                                    UnitPrice = row.UnitPrice,
                                    Quantity = 1,
                                    IsSpecifiedUnitPrice = row.IsSpecifiedUnitPrice,
                                    Distribute = row.Distribute,
                                    Amount = row.Amount,
                                    VatAmount = row.VatAmount,
                                    EmploymentStartDate = employmentStartDate,
                                    EmploymentTypeName = employmentName,
                                    FromPayrollGroup = true
                                };

                                if (outList.ContainsKey(employee.EmployeeId))
                                    outList[employee.EmployeeId].Add(item);
                                else
                                    outList.Add(employee.EmployeeId, new List<EmployeeFixedPayrollRowChangesDTO> { item });

                            }
                            changes++;
                        }
                        else
                        {
                            if (oldEmploymentNames != "")
                            {
                                if (!oldEmploymentNames.Contains(employmentName))
                                    employmentName = oldEmploymentNames + $", {employmentName}";
                                else
                                    employmentName = oldEmploymentNames;
                                oldEmploymentNames = "";
                            }
                        }

                        previousPayrollGroupId = payrollGroupId;
                    }
                    if (onePayrollGroup.HasValue)
                        currentDate = dateTo.AddDays(1);
                    else
                        currentDate = currentDate.AddDays(1);
                }
                if (changes > 0 && employee != null && outList.ContainsKey(employee.EmployeeId))
                {
                    List<EmployeeFixedPayrollRowChangesDTO> oldItemsOut = outList[employee.EmployeeId]?.Where(w => w.FromPayrollGroup && w.FixedPayrollRowId == changes - 1).ToList();
                    if (oldItemsOut != null)
                    {
                        foreach (EmployeeFixedPayrollRowChangesDTO oldItem in oldItemsOut)
                        {
                            oldItem.EmploymentTypeName = employmentName;
                        }
                    }
                }
            }
            List<FixedPayrollRowDTO> fixedRows = GetEmployeeFixedPayrollRows(actorCompanyId, employeeIds, dateFrom, dateTo);
            foreach (var fixedRow in fixedRows)
            {
                EmployeeFixedPayrollRowChangesDTO item = new EmployeeFixedPayrollRowChangesDTO
                {
                    ProductNr = fixedRow.ProductNr,
                    ProductName = fixedRow.ProductName,
                    PayrollGroupName = string.Empty,
                    FixedPayrollRowId = fixedRow.FixedPayrollRowId,
                    EmployeeId = fixedRow.EmployeeId,
                    FromDate = fixedRow.FromDate,
                    ToDate = fixedRow.ToDate,
                    UnitPrice = fixedRow.UnitPrice,
                    Quantity = fixedRow.Quantity,
                    IsSpecifiedUnitPrice = fixedRow.IsSpecifiedUnitPrice,
                    Distribute = fixedRow.Distribute,
                    Amount = fixedRow.Amount,
                    VatAmount = fixedRow.VatAmount,
                    EmploymentStartDate = null,
                    EmploymentTypeName = string.Empty,
                    FromPayrollGroup = false
                };
                if (outList.ContainsKey(fixedRow.EmployeeId))
                    outList[fixedRow.EmployeeId].Add(item);
                else
                    outList.Add(fixedRow.EmployeeId, new List<EmployeeFixedPayrollRowChangesDTO> { item });
            }
            return outList;
        }

        public List<FixedPayrollRowDTO> GetPayrollGroupEmployeeFixedPayrollRows(int payrollGroupId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.PayrollGroupPayrollProduct.NoTracking();
            var payrollGroupProducts = (from pr in entitiesReadOnly.PayrollGroupPayrollProduct
                                            .Include("PayrollProduct")
                                        where pr.PayrollGroupId == payrollGroupId &&
                                        pr.State == (int)SoeEntityState.Active
                                        select pr).ToList();

            return payrollGroupProducts.ToDTOFixedPayrollRowDTOs(true).ToList();
        }

        public List<FixedPayrollRowDTO> GetEmployeeFixedPayrollRows(int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeFixedPayrollRows(entities, actorCompanyId, employeeIds, dateFrom, dateTo);
        }

        public List<FixedPayrollRowDTO> GetEmployeeFixedPayrollRows(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            List<FixedPayrollRowDTO> rows = new List<FixedPayrollRowDTO>();
            List<FixedPayrollRowDTO> validRows = new List<FixedPayrollRowDTO>();

            var fixedPayrollRowsAllEmployees = (from pr in entities.FixedPayrollRow
                                                    .Include("PayrollProduct")
                                                where pr.ActorCompanyId == actorCompanyId &&
                                                employeeIds.Contains(pr.EmployeeId) &&
                                                pr.State == (int)SoeEntityState.Active
                                                select pr).ToList();

            rows.AddRange(fixedPayrollRowsAllEmployees.ToDTOs(true));
            foreach (var row in rows)
            {
                if (!row.FromDate.HasValue && !row.ToDate.HasValue)
                    validRows.Add(row);
                else if (!row.FromDate.HasValue && row.ToDate.HasValue && row.ToDate.Value >= dateFrom)
                    validRows.Add(row);
                else if (!row.ToDate.HasValue && row.FromDate.HasValue && row.FromDate.Value <= dateTo)
                    validRows.Add(row);
                else if (row.FromDate.HasValue && row.ToDate.HasValue && row.FromDate.Value <= dateTo && row.ToDate >= dateFrom)
                    validRows.Add(row);
                else if (row.FromDate.HasValue && row.ToDate.HasValue && row.ToDate.Value <= dateTo && row.FromDate >= dateFrom)
                    validRows.Add(row);
                else if (row.FromDate.HasValue && row.ToDate.HasValue && row.FromDate.Value >= dateFrom && row.FromDate <= dateTo)
                    validRows.Add(row);
                else if (row.FromDate.HasValue && row.ToDate.HasValue && row.ToDate.Value >= dateFrom && row.FromDate <= dateFrom)
                    validRows.Add(row);
            }

            return validRows;
        }

        public List<FixedPayrollRowDTO> GetEmployeeFixedPayrollRows(int actorCompanyId, int employeeId, int timePeriodId, bool performCalculation = false)
        {
            List<FixedPayrollRowDTO> rows = new List<FixedPayrollRowDTO>();
            List<PayrollProduct> products = new List<PayrollProduct>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            var employee = GetEmployeeWithEmploymentAndEmploymentChangeBatch(employeeId, actorCompanyId, false);
            if (employee == null)
                return new List<FixedPayrollRowDTO>();

            entitiesReadOnly.FixedPayrollRow.NoTracking();
            var fixedPayrollRows = (from pr in entitiesReadOnly.FixedPayrollRow
                                        .Include("PayrollProduct")
                                    where pr.ActorCompanyId == actorCompanyId &&
                                    pr.EmployeeId == employee.EmployeeId &&
                                    pr.State == (int)SoeEntityState.Active
                                    select pr).ToList();

            products.AddRange(fixedPayrollRows.Select(x => x.PayrollProduct));
            rows.AddRange(fixedPayrollRows.ToDTOs(true));

            var timePeriod = TimePeriodManager.GetTimePeriod(timePeriodId, actorCompanyId);
            if (timePeriod != null)
            {
                Employment employment = employee.GetEmployment(timePeriod.StopDate) ?? employee.GetEmployments(timePeriod.StartDate, timePeriod.StopDate).GetLastEmployment();
                if (employment != null)
                {
                    int? payrollGroupId = employment.GetPayrollGroupId(timePeriod.StopDate);
                    if (payrollGroupId.HasValue)
                    {
                        var payrollGroupProducts = (from pr in entitiesReadOnly.PayrollGroupPayrollProduct
                                                    .Include("PayrollProduct")
                                                    where pr.PayrollGroupId == payrollGroupId.Value &&
                                                            pr.State == (int)SoeEntityState.Active
                                                    select pr).ToList();

                        products.AddRange(payrollGroupProducts.Select(x => x.PayrollProduct));
                        rows.AddRange(payrollGroupProducts.ToDTOFixedPayrollRowDTOs(true).ToList());
                    }
                }
            }

            if (performCalculation)
            {
                DateTime date = DateTime.Now;

                foreach (var row in rows)
                {
                    var product = products.FirstOrDefault(x => x.ProductId == row.ProductId);
                    if (row.IsSpecifiedUnitPrice || (product != null && product.AverageCalculated))
                        continue;

                    PayrollPriceFormulaResultDTO formulaResult = PayrollManager.EvaluatePayrollPriceFormula(actorCompanyId, employeeId, row.ProductId, date);
                    if (formulaResult != null)
                    {
                        //Set amounts
                        row.UnitPrice = Decimal.Round(formulaResult.Amount, 2, MidpointRounding.AwayFromZero);
                        row.Amount = Decimal.Round((row.Quantity * row.UnitPrice), 2, MidpointRounding.AwayFromZero);//quantity from FixedPayrollRow is never time
                    }
                }
            }

            return rows.OrderBy(x => x.ProductNr).ToList();
        }

        public List<FixedPayrollRowDTO> GetEmployeeFixedPayrollRows(int actorCompanyId, List<Employee> employees, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeFixedPayrollRows(entities, actorCompanyId, employees, dateFrom, dateTo);
        }

        private int getEmployeeFixedPayrollRowsCount = 0;

        public List<FixedPayrollRowDTO> GetEmployeeFixedPayrollRows(CompEntities entities, int actorCompanyId, List<Employee> employees, DateTime dateFrom, DateTime dateTo, EvaluatePayrollPriceFormulaInputDTO iDTO = null)
        {
            getEmployeeFixedPayrollRowsCount++;
            List<int> employeeIds = employees.Select(x => x.EmployeeId).ToList();
            var fixedPayrollRows = new List<FixedPayrollRow>();
            List<int> remainingEmployeeIds = employeeIds;
            bool hasFixedRowsOnCompany = true;

            if (employeeIds.Count > 3000)
                hasFixedRowsOnCompany = entities.FixedPayrollRow.AsNoTracking().Where(w => w.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active).Any();

            if (hasFixedRowsOnCompany)
            {
                while (remainingEmployeeIds.Any())
                {
                    var batchIds = remainingEmployeeIds.Take(1000).ToList();
                    var inBatch = (from pr in entities.FixedPayrollRow
                       .Include("PayrollProduct")
                                   where pr.ActorCompanyId == actorCompanyId &&
                                   batchIds.Contains(pr.EmployeeId) &&
                                   pr.State == (int)SoeEntityState.Active
                                   select pr).ToList();

                    fixedPayrollRows.AddRange(inBatch);
                    remainingEmployeeIds = remainingEmployeeIds.Where(w => !batchIds.Contains(w)).ToList();
                }
            }

            List<PayrollGroupPayrollProduct> allPayrollGroupProducts = new List<PayrollGroupPayrollProduct>();

            // Only log every 1000 call to avoid flooding the logs
            if ((getEmployeeFixedPayrollRowsCount % 1000) == 0m)
            {
                try
                {
                    StackTrace stackTrace = new StackTrace();
                    StackFrame[] stackFrames = stackTrace.GetFrames();
                    StringBuilder sb = new StringBuilder();
                    foreach (StackFrame stackFrame in stackFrames)
                    {
                        sb.AppendLine(stackFrame.GetMethod().Name);
                    }
                    LogCollector.LogError($"GetEmployeeFixedPayrollRows call count:{getEmployeeFixedPayrollRowsCount} actorCompanyId:{actorCompanyId} employeeIds:{string.Join(",", employeeIds)} dateFrom:{dateFrom} dateTo:{dateTo} time:{DateTime.Now.ToString()} trace: {sb.ToString()}");
                }
                catch (Exception ex)
                {
                    LogCollector.LogError(ex, $"GetEmployeeFixedPayrollRows call count:{getEmployeeFixedPayrollRowsCount} actorCompanyId:{actorCompanyId} employeeIds:{string.Join(",", employeeIds)} dateFrom:{dateFrom} dateTo:{dateTo}");
                }
            }

            var key = "GetEmployeeFixedPayrollRowsallPayrollGroupProducts";
            var cache = BusinessMemoryCache<List<PayrollGroupPayrollProduct>>.Get(key);

            if (!cache.IsNullOrEmpty())
                allPayrollGroupProducts = cache;
            else
            {
                allPayrollGroupProducts = (from pr in entities.PayrollGroupPayrollProduct
                                                .Include("PayrollProduct")
                                           where pr.State == (int)SoeEntityState.Active
                                           select pr).ToList();
                BusinessMemoryCache<List<PayrollGroupPayrollProduct>>.Set(key, allPayrollGroupProducts, seconds: 10);
            }

            List<FixedPayrollRowDTO> rows = new List<FixedPayrollRowDTO>();
            List<PayrollProduct> products = new List<PayrollProduct>();

            products.AddRange(fixedPayrollRows.Select(x => x.PayrollProduct));
            rows.AddRange(fixedPayrollRows.ToDTOs(true));

            foreach (var employee in employees)
            {
                Employment employment = employee.GetEmployments(dateFrom, dateTo)?.GetLastEmployment();
                if (employment != null)
                {
                    int? payrollGroupId = employment.GetPayrollGroupId(dateTo);
                    if (payrollGroupId.HasValue)
                    {
                        var payrollGroupProducts = (from pr in allPayrollGroupProducts
                                                    where pr.PayrollGroupId == payrollGroupId.Value &&
                                                            pr.State == (int)SoeEntityState.Active
                                                    select pr).ToList();

                        products.AddRange(payrollGroupProducts.Select(x => x.PayrollProduct));
                        rows.AddRange(payrollGroupProducts.ToDTOFixedPayrollRowDTOs(true).ToList());
                    }
                }

                DateTime date = dateTo;

                foreach (var row in rows.Where(w => w.EmployeeId == employee.EmployeeId))
                {
                    var product = products.FirstOrDefault(x => x.ProductId == row.ProductId);
                    if (row.IsSpecifiedUnitPrice || (product != null && product.AverageCalculated))
                        continue;

                    PayrollPriceFormulaResultDTO formulaResult = PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employee, employment, product, date, iDTO: iDTO);
                    if (formulaResult != null)
                    {
                        //Set amounts
                        row.UnitPrice = Decimal.Round(formulaResult.Amount, 2, MidpointRounding.AwayFromZero);
                        row.Amount = Decimal.Round((row.Quantity * row.UnitPrice), 2, MidpointRounding.AwayFromZero);//quantity from FixedPayrollRow is never time
                    }
                }
            }

            return rows.OrderBy(x => x.ProductNr).ToList();
        }

        #endregion

        #region PaymentMethod

        public Dictionary<int, string> GetEmployeeDisbursementMethods(int langId, bool addEmptyRow = false, bool skipUnknown = false)
        {

            var validTermsDict = new Dictionary<int, string>();
            var termsDict = base.GetTermGroupContent(TermGroup.EmployeeDisbursementMethod, langId, addEmptyRow: addEmptyRow, skipUnknown: skipUnknown);
            var useExtendedSelection = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportUseExtendedCurrencyNOK, 0, base.ActorCompanyId, 0);


            switch (langId)
            {
                case (int)TermGroup_Languages.Swedish:
                    #region Swedish

                    foreach (var pair in termsDict)
                    {
                        bool use = false;
                        switch (pair.Id)
                        {
                            case (int)TermGroup_EmployeeDisbursementMethod.Unknown:
                                use = !skipUnknown;
                                break;
                            case (int)TermGroup_EmployeeDisbursementMethod.SE_CashDeposit:
                                use = true;
                                break;
                            case (int)TermGroup_EmployeeDisbursementMethod.SE_PersonAccount:
                                use = true;
                                break;
                            case (int)TermGroup_EmployeeDisbursementMethod.SE_AccountDeposit:
                                use = true;
                                break;
                            case (int)TermGroup_EmployeeDisbursementMethod.SE_NorweiganAccount:
                                if (useExtendedSelection)
                                    use = true;
                                break;

                                //case (int)TermGroup_EmployeeDisbursementMethod.SE_IBAN:
                                //    use = true;
                                //    break;
                        }

                        if (use)
                            validTermsDict.Add(pair.Id, pair.Name);
                    }

                    #endregion
                    break;
                case (int)TermGroup_Languages.Finnish:
                    #region Finnish

                    //just get Finnish terms until Finnish payroll is implemented
                    foreach (var term in termsDict)
                    {
                        if (term.Id > 0)
                            validTermsDict.Add(term.Id, term.Name);
                    }

                    #endregion
                    break;
                case (int)TermGroup_Languages.Norwegian:
                    #region Norwegian

                    foreach (var pair in termsDict)
                    {
                        bool use = false;
                        switch (pair.Id)
                        {
                            case (int)TermGroup_EmployeeDisbursementMethod.Unknown:
                                use = !skipUnknown;
                                break;
                        }

                        if (use)
                            validTermsDict.Add(pair.Id, pair.Name);
                    }

                    #endregion
                    break;
                default:
                    //Not supported
                    break;
            }

            return validTermsDict.Sort();
        }

        #endregion

        #region SysPosition

        public List<SysPosition> GetAllSysPositions()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from p in sysEntitiesReadOnly.SysPosition
                    where p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }

        public Dictionary<int, string> GetSysPositionsDict(int actorCompanyId, int sysCountryId, int sysLanguageId, bool addEmptyRow)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DeliveryCondition.NoTracking();
            return GetSysPositionsDict(entities, actorCompanyId, sysCountryId, sysLanguageId, addEmptyRow);
        }

        public Dictionary<int, string> GetSysPositionsDict(CompEntities entities, int actorCompanyId, int sysCountryId, int sysLanguageId, bool addEmptyRow)
        {
            var dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var SystemPositions = GetSysPositions(actorCompanyId, sysCountryId, sysLanguageId);
            foreach (var sp in SystemPositions)
            {
                if (sp.Code.IsNullOrEmpty())
                    dict.Add(sp.SysPositionId, sp.Name);
                else
                    dict.Add(sp.SysPositionId, sp.Code + ' ' + sp.Name);
            }

            return dict;
        }

        public List<SysPosition> GetSysPositions(int? actorCompanyId = null, int? sysCountryId = null, int? sysLanguageId = null, int? positionId = null)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var query = (from p in sysEntitiesReadOnly.SysPosition.Include("SysCountry").Include("SysLanguage")
                         where p.State == (int)SoeEntityState.Active
                         select p);

            if (sysCountryId.HasValue)
                query = query.Where(p => p.SysCountryId == sysCountryId.Value);

            if (sysLanguageId.HasValue)
                query = query.Where(p => p.SysLanguageId == sysLanguageId.Value);

            if (positionId.HasValue)
                query = query.Where(p => p.SysPositionId == positionId.Value);

            query = query.OrderBy(p => p.SysCountry.Code).ThenBy(p => p.SysLanguage.LangCode).ThenBy(p => p.Code).ThenBy(p => p.Name);

            var sysPositions = query.ToList();

            // If ActorCompanyId is specified, check if position is linked
            if (actorCompanyId.HasValue)
            {
                // Get linked positions
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                entities.Position.NoTracking();
                var linkedPositions = (from p in entities.Position
                                       where p.ActorCompanyId == actorCompanyId &&
                                       p.SysPositionId.HasValue &&
                                       p.State == (int)SoeEntityState.Active
                                       select p);

                // Mark the SysPositions that are linked
                foreach (var position in linkedPositions)
                {
                    var sysPosition = sysPositions.FirstOrDefault(p => p.SysPositionId == position.SysPositionId.Value);
                    if (sysPosition != null)
                        sysPosition.IsLinked = true;
                }
            }

            return sysPositions;
        }

        public IEnumerable<SysPosition> GetLinkedSysPositions(int actorCompanyId)
        {
            // Get linked Positions
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Position.NoTracking();
            List<int> sysPositionsId = (from p in entities.Position
                                        where p.ActorCompanyId == actorCompanyId &&
                                        p.SysPositionId.HasValue &&
                                        p.State == (int)SoeEntityState.Active
                                        select p.SysPositionId.Value).ToList();

            // Get linked SysPositions
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var sysPositions = (from p in sysEntitiesReadOnly.SysPosition
                                where sysPositionsId.Contains(p.SysPositionId) &&
                                p.State == (int)SoeEntityState.Active
                                select p);

            return sysPositions;
        }

        public SysPosition GetSysPosition(int sysPositionId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysPosition(sysEntitiesReadOnly, sysPositionId);
        }

        public SysPosition GetSysPosition(SOESysEntities entities, int sysPositionId)
        {
            return (from p in entities.SysPosition.Include("SysCountry").Include("SysLanguage")
                    where p.SysPositionId == sysPositionId &&
                    p.State == (int)SoeEntityState.Active
                    select p).FirstOrDefault();
        }

        public ActionResult SaveSysPosition(SysPositionDTO sysPositionInput)
        {
            if (sysPositionInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysPosition");

            // Default result is successful
            ActionResult result = new ActionResult();

            bool updateLinked = false;
            int sysPositionId = sysPositionInput.SysPositionId;

            #region Prereq

            // Check existance
            if (SysPositionExists(sysPositionId, sysPositionInput.SysCountryId, sysPositionInput.SysLanguageId, sysPositionInput.Code, sysPositionInput.Name))
                return new ActionResult((int)ActionResultSave.SysPositionExists);

            #endregion

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Perform

                        // Get existing position
                        SysPosition sysPosition = GetSysPosition(entities, sysPositionId);
                        if (sysPosition == null)
                        {
                            #region Add

                            sysPosition = new SysPosition()
                            {
                                SysCountryId = sysPositionInput.SysCountryId,
                                SysLanguageId = sysPositionInput.SysLanguageId,
                                Code = sysPositionInput.Code,
                                Name = sysPositionInput.Name,
                                Description = sysPositionInput.Description,
                                State = (int)sysPositionInput.State
                            };

                            SetCreatedPropertiesOnEntity(sysPosition);
                            entities.SysPosition.Add(sysPosition);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            if (sysPosition.Code != sysPositionInput.Code ||
                                sysPosition.Name != sysPositionInput.Name ||
                                sysPosition.Description != sysPositionInput.Description)
                                updateLinked = true;

                            sysPosition.SysCountryId = sysPositionInput.SysCountryId;
                            sysPosition.SysLanguageId = sysPositionInput.SysLanguageId;
                            sysPosition.Code = sysPositionInput.Code;
                            sysPosition.Name = sysPositionInput.Name;
                            sysPosition.Description = sysPositionInput.Description;

                            SetModifiedPropertiesOnEntity(sysPosition);

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        transaction.Complete();
                        sysPositionId = sysPosition.SysPositionId;
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
                        result.IntegerValue = sysPositionId;
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                if (result.Success && updateLinked)
                {
                    ActionResult linkedResult = UpdateLinkedPositions(sysPositionId);
                    if (!linkedResult.Success)
                        return linkedResult;
                }

                return result;
            }
        }

        private bool SysPositionExists(int sysPositionId, int sysCountryId, int sysLanguageId, string code, string name)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from p in sysEntitiesReadOnly.SysPosition
                    where p.SysPositionId != sysPositionId &&
                    p.SysCountryId == sysCountryId &&
                    p.SysLanguageId == sysLanguageId &&
                    p.Code == code &&
                    p.Name == name &&
                    p.State != (int)SoeEntityState.Deleted
                    select p).Any();
        }

        public ActionResult DeleteSysPosition(int sysPositionId)
        {
            // Default result is successful
            ActionResult result;

            using (SOESysEntities entities = new SOESysEntities())
            {
                #region Perform

                SysPosition sysPosition = GetSysPosition(entities, sysPositionId);
                if (sysPosition == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "SysPosition");

                if (SysPositionHasCompPositions(sysPositionId))
                    result = new ActionResult((int)ActionResultDelete.SysPositionHasCompPositions);
                else
                    result = ChangeEntityStateOnEntity(entities, sysPosition, SoeEntityState.Deleted, true);

                #endregion
            }

            return result;
        }

        private bool SysPositionHasCompPositions(int sysPositionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Position.NoTracking();
            return SysPositionHasCompPositions(entities, sysPositionId);
        }

        private bool SysPositionHasCompPositions(CompEntities entities, int sysPositionId)
        {
            return entities.Position.Any(p => p.SysPositionId == sysPositionId);
        }

        /// <summary>
        /// Copy (and link) SysPositions to Position (comp)
        /// </summary>
        /// <param name="sysPositions">List of SysPositions to copy</param>
        /// <param name="link">If true, the position will copied to comp and have a link to the SysPostion, if false, no link is created</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>ActionResult</returns>
        public ActionResult CopyAndLinkSysPositions(List<SysPositionGridDTO> sysPositions, bool link, int actorCompanyId)
        {
            if (sysPositions == null || sysPositions.Count == 0)
                return new ActionResult((int)ActionResultSave.NoItemsToProcess, "SysPosition");

            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (SysPositionGridDTO sysPosition in sysPositions)
                        {
                            #region Check if position already exists and is linked

                            Position position = GetPositionBySys(entities, sysPosition.SysPositionId, actorCompanyId);
                            if (position != null)
                            {
                                // Possibly update position
                                if (position.Code != sysPosition.Code ||
                                    position.Name != sysPosition.Name ||
                                    (position.Description != sysPosition.Description && !String.IsNullOrEmpty(sysPosition.Description)))
                                {
                                    position.Code = sysPosition.Code;
                                    position.Name = sysPosition.Name;
                                    if (!String.IsNullOrEmpty(sysPosition.Description))
                                        position.Description = sysPosition.Description;
                                    SetModifiedProperties(position);
                                }
                                continue;
                            }

                            #endregion

                            #region Check if position already exists but is not linked

                            if (link)
                            {
                                position = GetPosition(entities, actorCompanyId, sysPosition.Code, sysPosition.Name);
                                if (position != null)
                                {
                                    position.SysPositionId = sysPosition.SysPositionId;
                                    SetModifiedProperties(position);
                                    continue;
                                }
                            }

                            #endregion

                            #region Create new position

                            position = new Position()
                            {
                                ActorCompanyId = actorCompanyId,
                                SysPositionId = link ? sysPosition.SysPositionId : (int?)null,
                                Code = sysPosition.Code,
                                Name = sysPosition.Name,
                                Description = sysPosition.Description
                            };
                            SetCreatedProperties(position);
                            entities.Position.AddObject(position);

                            #endregion
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Update all linked comp positions for specified company
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>ActionResult</returns>
        public ActionResult UpdateAllLinkedPositions(int actorCompanyId)
        {
            var sysPositions = GetLinkedSysPositions(actorCompanyId);
            return CopyAndLinkSysPositions(sysPositions.ToGridDTOs().ToList(), true, actorCompanyId);
        }

        /// <summary>
        /// Update all comp positions linked to specified sys position
        /// </summary>
        /// <param name="sysPositionId">SysPosition ID</param>
        /// <returns>ActionResult</returns>
        public ActionResult UpdateLinkedPositions(int sysPositionId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            #region Prereq

            // Get SysPosition
            SysPosition sysPosition = GetSysPosition(sysPositionId);
            if (sysPosition == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "SysPosition");

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Get comp positions linked to the sys position
                        var positions = GetPositionsBySys(entities, sysPositionId);
                        foreach (Position position in positions)
                        {
                            // Possibly update position
                            if (position.Code != sysPosition.Code ||
                                position.Name != sysPosition.Name ||
                                (position.Description != sysPosition.Description && !String.IsNullOrEmpty(sysPosition.Description)))
                            {
                                position.Code = sysPosition.Code;
                                position.Name = sysPosition.Name;
                                if (!String.IsNullOrEmpty(sysPosition.Description))
                                    position.Description = sysPosition.Description;
                                SetModifiedProperties(position);
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        #endregion

        #region Position

        public List<Position> GetPositions(int actorCompanyId, bool loadSkills = false, int? positionId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SkillType.NoTracking();
            return GetPositions(entities, actorCompanyId, loadSkills, positionId);
        }

        public List<Position> GetPositions(CompEntities entities, int actorCompanyId, bool loadSkills = false, int? positionId = null)
        {
            IQueryable<Position> query = entities.Position;
            if (loadSkills)
                query = query.Include("PositionSkill.Skill.SkillType");

            query = (from p in query
                     where p.ActorCompanyId == actorCompanyId &&
                     p.State == (int)SoeEntityState.Active
                     orderby p.Name
                     select p);

            if (positionId.HasValue)
                query = query.Where(p => p.PositionId == positionId.Value);

            return query.ToList();
        }

        public Dictionary<int, string> GetPositionsDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<Position> positions = GetPositions(actorCompanyId);
            foreach (Position position in positions)
            {
                dict.Add(position.PositionId, position.Name);
            }

            return dict;
        }

        public Position GetPositionIncludingSkill(CompEntities entities, int positionId)
        {
            return (from p in entities.Position.Include("PositionSkill.Skill")
                    where p.PositionId == positionId
                    select p).FirstOrDefault();
        }

        public Position GetPositionIncludingSkill(int positionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Position.NoTracking();
            return GetPositionIncludingSkill(entities, positionId);
        }

        public Position GetPosition(int positionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Position.NoTracking();
            return GetPosition(entities, positionId);
        }

        public Position GetPosition(CompEntities entities, int positionId)
        {
            return (from p in entities.Position.Include("PositionSkill")
                    where p.PositionId == positionId
                    select p).FirstOrDefault();
        }

        public Position GetPosition(CompEntities entities, int actorCompanyId, string code, string name)
        {
            return (from p in entities.Position
                    where p.ActorCompanyId == actorCompanyId &&
                    p.Code == code &&
                    p.Name == name &&
                    p.State == (int)SoeEntityState.Active
                    select p).FirstOrDefault();
        }

        public IEnumerable<Position> GetPositionsBySys(CompEntities entities, int sysPositionId)
        {
            return (from p in entities.Position
                    where p.SysPositionId == sysPositionId &&
                    p.State == (int)SoeEntityState.Active
                    select p);
        }

        public Position GetPositionBySys(CompEntities entities, int sysPositionId, int actorCompanyId)
        {
            return (from p in entities.Position
                    where p.SysPositionId == sysPositionId &&
                    p.ActorCompanyId == actorCompanyId &&
                    p.State == (int)SoeEntityState.Active
                    select p).FirstOrDefault();
        }

        /// <summary>
        /// Insert or update a position
        /// </summary>
        /// <param name="positionInput">Position DTO</param>
        /// <param name="positionSkills">List of PositionSkill DTOs</param>
        /// <param name="actorCompanyId">Company Id</param>
        /// <returns>ActionResult</returns>
        public ActionResult SavePosition(PositionDTO positionInput, List<PositionSkillDTO> positionSkills, int actorCompanyId)
        {
            if (positionInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Position");

            // Default result is successful
            ActionResult result = new ActionResult();

            int positionId = positionInput.PositionId;

            #region Prereq

            // Check existance
            if (!String.IsNullOrEmpty(positionInput.Code) && PositionExists(actorCompanyId, positionId, positionInput.Code, positionInput.Name))
                return new ActionResult((int)ActionResultSave.PositionExists);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Position

                        // Get existing position
                        Position position = GetPosition(entities, positionId);

                        if (position == null)
                        {
                            #region Position Add

                            position = new Position()
                            {
                                ActorCompanyId = actorCompanyId,
                                SysPositionId = positionInput.SysPositionId.HasValue && positionInput.SysPositionId.Value != 0 ? positionInput.SysPositionId : (int?)null,
                                Code = positionInput.Code,
                                Name = positionInput.Name,
                                Description = positionInput.Description,
                                State = (int)positionInput.State
                            };

                            SetCreatedProperties(position);

                            entities.Position.AddObject(position);

                            #endregion

                            #region Skills

                            if (positionSkills != null)
                                AddPositionSkills(position, positionSkills);

                            #endregion
                        }
                        else
                        {
                            #region Position Update

                            // Update Position
                            position.SysPositionId = positionInput.SysPositionId.HasValue && positionInput.SysPositionId.Value != 0 ? positionInput.SysPositionId : (int?)null;
                            position.Code = positionInput.Code;
                            position.Name = positionInput.Name;
                            position.Description = positionInput.Description;
                            position.State = (int)positionInput.State;

                            SetModifiedProperties(position);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;

                            #endregion

                            #region Skills

                            if (positionSkills != null)
                                UpdatePositionSkills(entities, position, positionSkills);

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            positionId = position.PositionId;
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
                        result.IntegerValue = positionId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private bool PositionExists(int actorCompanyId, int positionId, string code, string name)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Position.NoTracking();
            return (from p in entities.Position
                    where p.ActorCompanyId == actorCompanyId &&
                    p.PositionId != positionId &&
                    p.Code == code &&
                    p.Name == name &&
                    p.State != (int)SoeEntityState.Deleted
                    select p).Any();
        }

        /// <summary>
        /// Sets a positions state to Deleted
        /// </summary>
        /// <param name="positionId">Position to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeletePosition(int positionId)
        {
            using (CompEntities entities = new CompEntities())
            {
                Position position = GetPosition(entities, positionId);
                if (position == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "Position");

                return ChangeEntityState(entities, position, SoeEntityState.Deleted, true);
            }
        }

        private void AddPositionSkills(Position position, List<PositionSkillDTO> positionSkills)
        {
            foreach (PositionSkillDTO positionSkill in positionSkills)
            {
                position.PositionSkill.Add(new PositionSkill()
                {
                    SkillId = positionSkill.SkillId,
                    SkillLevel = positionSkill.SkillLevel
                });
            }
        }

        private void UpdatePositionSkills(CompEntities entities, Position position, List<PositionSkillDTO> positionSkills)
        {
            // Loop through existing skills
            foreach (PositionSkill positionSkill in position.PositionSkill.ToList())
            {
                PositionSkillDTO existsInInput = positionSkills.FirstOrDefault(s => s.SkillId == positionSkill.SkillId);
                if (existsInInput != null)
                {
                    // Skill still exists in input, update it and remove from input
                    positionSkill.SkillLevel = existsInInput.SkillLevel;
                    positionSkills.Remove(existsInInput);
                }
                else
                {
                    // Skill does not exist in input, remove from position
                    position.PositionSkill.Remove(positionSkill);
                    entities.DeleteObject(positionSkill);
                }
            }

            // Add new skills (remaining in input)
            AddPositionSkills(position, positionSkills);
        }

        #endregion

        #region EmployeeOrganisationInformation

        public List<EmployeeOrganisationInformation> GetEmployeeOrganisationInformations(int actorCompanyId, DateTime fromDate, DateTime toDate)
        {
            List<EmployeeOrganisationInformation> employeeOrganisationInformations = new List<EmployeeOrganisationInformation>();
            var employees = GetEmployeesForUsersAttestRoles(out _, actorCompanyId, base.UserId, base.RoleId, dateFrom: fromDate, dateTo: toDate, addContactInfo: true, addRoleInfo: true);

            if (employees.Any())
            {
                List<int> employeeIds = employees.Select(s => s.EmployeeId).ToList();
                List<int> userIds = employees.Where(w => w.UserId.HasValue).Select(s => s.UserId.Value).ToList();
                var employeeAccounts = GetEmployeeAccounts(actorCompanyId, employeeIds, fromDate, toDate);
                var employeePositions = GetEmployeePositions(employeeIds, true);
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var accounts = GetAccountInternalsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));

                var attestRoleUsers = entitiesReadOnly.AttestRoleUser.Include("Account.AccountDim").Include("AttestRole").Include("User.Employee.ContactPerson").Include("Children").Include("Parent.Parent.Parent.Parent").Where(w => userIds.Contains(w.UserId) && w.State == (int)SoeEntityState.Active && (!w.DateFrom.HasValue || w.DateFrom <= fromDate) && (!w.DateTo.HasValue || w.DateTo >= toDate)).ToList();
                foreach (var employee in employees)
                {
                    employee.UserReference.Load();
                    EmployeeOrganisationInformation information = new EmployeeOrganisationInformation()
                    {
                        Email = employee.User.Email,
                        EmployeeNr = employee.EmployeeNr,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName
                    };

                    var employeeEmployeePositions = employeePositions.Where(w => w.EmployeeId == employee.EmployeeId).ToList();
                    var employeeEmployeeAccounts = employeeAccounts.Where(w => w.EmployeeId == employee.EmployeeId).ToList();

                    foreach (var employeePosition in employeeEmployeePositions)
                    {
                        var pos = new EmployeeOrganisationPosition()
                        {
                            Code = employeePosition.Position.Code,
                            Default = employeePosition.Default,
                            Title = employeePosition.Position.Name,
                            Description = employeePosition.Position.Description,
                            SysTitle = employeePosition.Position.SysPositionName,
                            SysDescription = employeePosition.Position.SysPositionDescription,
                        };
                        information.EmployeeOrganisationPositions.Add(pos);
                    }

                    foreach (var employeeEmployeeAccount in employeeEmployeeAccounts.Where(w => w.AccountId.HasValue))
                    {
                        var executiveAccounts = attestRoleUsers.Where(w => w.AccountId == employeeEmployeeAccount.AccountId.Value);

                        if (executiveAccounts.IsNullOrEmpty())
                        {
                            AttestRoleUser attestRoleUser = null;
                            var childAccount = accounts.FirstOrDefault(f => f.AccountId == employeeEmployeeAccount.AccountId.Value);

                            if (childAccount?.ParentAccountId != null)
                            {
                                var parentAccount = accounts.FirstOrDefault(f => f.AccountId == childAccount.ParentAccountId.Value);

                                while (parentAccount != null && attestRoleUser == null)
                                {
                                    attestRoleUser = attestRoleUsers.FirstOrDefault(f => f.AccountId == parentAccount.AccountId);

                                    if (attestRoleUser == null && parentAccount.ParentAccountId.HasValue)
                                        parentAccount = accounts.FirstOrDefault(f => f.AccountId == parentAccount.ParentAccountId.Value);
                                    else
                                        parentAccount = null;
                                }
                            }
                            if (attestRoleUser != null)
                                executiveAccounts = new List<AttestRoleUser>() { attestRoleUser };
                        }

                        if (executiveAccounts.Any())
                        {
                            foreach (var executiveAccount in executiveAccounts)
                            {
                                var execEmployee = employees.FirstOrDefault(f => f.UserId == executiveAccount.UserId);
                                var execEmployeePosition = execEmployee != null ? employeePositions.FirstOrDefault(w => w.EmployeeId == execEmployee.EmployeeId) : null;

                                EmployeeOrganisationAccount acc = new EmployeeOrganisationAccount()
                                {
                                    AccountDimNr = employeeEmployeeAccount.Account.AccountDim.AccountDimNr.ToString(),
                                    AccountName = employeeEmployeeAccount.Account.Name,
                                    AccountNumber = employeeEmployeeAccount.Account.AccountNr,
                                    Default = employeeEmployeeAccount.Default,
                                    FromDate = employeeEmployeeAccount.DateFrom,
                                    ToDate = employeeEmployeeAccount.DateTo,
                                    ExecutiveFirstName = execEmployee != null ? execEmployee.FirstName : string.Empty,
                                    ExecutiveLastName = execEmployee != null ? execEmployee.LastName : string.Empty,
                                    AccountDim = employeeEmployeeAccount.Account.AccountDim.Name,
                                    ExecutivePositionTitle = execEmployeePosition != null ? execEmployeePosition.Position.Name : string.Empty,
                                    ExecutivePositionCode = execEmployeePosition != null ? execEmployeePosition.Position.Code : string.Empty,
                                    ExecutivePositionRole = executiveAccount.AttestRole.Name,
                                    ExecutiveAccountDimNr = executiveAccount.Account?.AccountDim?.AccountDimNr.ToString() ?? string.Empty,
                                    ExecutiveAccountName = executiveAccount.Account?.Name ?? string.Empty,
                                    ExecutiveAccountNumber = executiveAccount.Account?.AccountNr ?? string.Empty,
                                };

                                information.EmployeeOrganisationAccounts.Add(acc);
                            }
                        }
                        else
                        {
                            EmployeeOrganisationAccount acc = new EmployeeOrganisationAccount()
                            {
                                AccountDimNr = employeeEmployeeAccount.Account.AccountDim.AccountDimNr.ToString(),
                                AccountName = employeeEmployeeAccount.Account.Name,
                                AccountNumber = employeeEmployeeAccount.Account.AccountNr,
                                Default = employeeEmployeeAccount.Default,
                                FromDate = employeeEmployeeAccount.DateFrom,
                                ToDate = employeeEmployeeAccount.DateTo,
                                ExecutiveFirstName = string.Empty,
                                ExecutiveLastName = string.Empty,
                                AccountDim = employeeEmployeeAccount.Account.AccountDim.Name,
                                ExecutivePositionTitle = string.Empty,
                                ExecutivePositionCode = string.Empty,
                                ExecutivePositionRole = string.Empty,
                                ExecutiveAccountDimNr = string.Empty,
                                ExecutiveAccountName = string.Empty,
                                ExecutiveAccountNumber = string.Empty,
                            };

                            information.EmployeeOrganisationAccounts.Add(acc);
                        }
                    }
                    employeeOrganisationInformations.Add(information);
                }
            }

            return employeeOrganisationInformations;
        }

        #endregion

        #region WebPubSub

        private void SendWebPubSubMessage(CompEntities entities, int actorCompanyId, int employeeId, WebPubSubMessageAction action)
        {
            List<int> terminalIds = TimeStampManager.GetTimeTerminalIdsForPubSub(entities, actorCompanyId);
            bool useCache = false;
            foreach (int terminalId in terminalIds)
            {
                if (terminalIds.Count == 1 || TimeStampManager.IsEmployeeConnectedToTimeTerminal(actorCompanyId, terminalId, employeeId, null, useCache))
                    base.WebPubSubSendMessage(GoTimeStampExtensions.GetTerminalPubSubKey(actorCompanyId, terminalId), GoTimeStampExtensions.GetEmployeeUpdateMessage(actorCompanyId, employeeId, action));

                useCache = true;
            }
        }

        private void SendWebPubSubMessage(CompEntities entities, EmployeeGroup employeeGroup, WebPubSubMessageAction action)
        {
            List<int> terminalIds = TimeStampManager.GetTimeTerminalIdsForPubSub(entities, employeeGroup.ActorCompanyId);
            foreach (int terminalId in terminalIds)
            {
                base.WebPubSubSendMessage(GoTimeStampExtensions.GetTerminalPubSubKey(employeeGroup.ActorCompanyId, terminalId), employeeGroup.GetUpdateMessage(action));
            }
        }

        #endregion
    }
}

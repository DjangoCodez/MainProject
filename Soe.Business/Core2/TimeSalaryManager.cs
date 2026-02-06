using log4net;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.SalaryAdapters;
using SoftOne.Soe.Business.Util.SalaryPaymentAdapters;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class TimeSalaryManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimeSalaryManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region TimeSalaryExport

        #region Selection

        public TimeSalaryExportSelectionDTO GetTimeSalaryExportSelection(int actorCompanyId, DateTime dateFrom, DateTime dateTo, int accountDimId)
        {
            bool groupOnCategory = accountDimId == -1;
            bool groupOnEmployeeGroup = accountDimId == -2;
            bool groupOnPayrollGroup = accountDimId == -3;
            bool groupOnPayrollCompany = accountDimId == -4;

            TimeSalaryExportSelectionDTO selectionDTO = new TimeSalaryExportSelectionDTO()
            {
                ActorCompanyId = actorCompanyId,
                DateFrom = dateFrom,
                DateTo = dateTo,
            };

            List<TimeSalaryExportSelectionTransactionDTO> transactionsOnCompany = new List<TimeSalaryExportSelectionTransactionDTO>();
            List<int> accountIdsWithTransactions = new List<int>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Employee.NoTracking();
            entitiesReadOnly.ContactPerson.NoTracking();
            List<Employee> allEmployees = entitiesReadOnly.Employee.Include("ContactPerson").Where(e => e.ActorCompanyId == actorCompanyId && !e.ExcludeFromPayroll && !e.Vacant && !e.Hidden).ToList();
            List<int> allEmployeeIds = allEmployees.Select(s => s.EmployeeId).ToList(); 
            Dictionary<int, List<TimeSalaryExportSelectionTransactionDTO>> transDict = GetTimeSalaryExportSelectionTransactions(actorCompanyId, dateFrom, dateTo, accountDimId != 0);
            transDict = transDict.Where(x => allEmployeeIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in transDict)
            {
                transactionsOnCompany.AddRange(item.Value);

                foreach (var trans in item.Value)
                    accountIdsWithTransactions.AddRange(trans.AccountInternals.Select(s => s.AccountId));

            }

            accountIdsWithTransactions = accountIdsWithTransactions.Distinct().ToList();
            List<int> addedAccountIds = new List<int>();

            List<AttestState> attestStates = AttestManager.GetAttestStates(actorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
            List<AccountInternal> accounts = AccountManager.GetAccountInternalsByDim(accountDimId, actorCompanyId);
            AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.IncludeOnlyChildrenOneLevel);

            if (accountDimId > 0)
            {
                foreach (AccountInternal accInt in accounts.Where(w => accountIdsWithTransactions.Contains(w.AccountId)).OrderBy(o => o.Account.Name))
                {
                    List<AccountDTO> subAccounts = AccountManager.GetAccountsFromHierarchyById(actorCompanyId, accInt.AccountId, input);
                    subAccounts = subAccounts.Where(w => w.ParentAccountId.HasValue).ToList();
                    List<int> ids = subAccounts.Select(s => s.AccountId).ToList();
                    ids.Add(accInt.AccountId);
                    ids = ids.Distinct().ToList();

                    List<TimeSalaryExportSelectionTransactionDTO> transactionsOnAccount = transactionsOnCompany.Where(w => !w.AssignedToGroup && w.ContainsAnyAccount(ids)).ToList();
                    if (transactionsOnAccount.Any())
                    {
                        addedAccountIds.AddRange(ids);
                        TimeSalaryExportSelectionGroupDTO group = CreateTimeSalaryExportSelectionGroupDTO(accInt, attestStates, allEmployees, transactionsOnAccount);
                        selectionDTO.TimeSalaryExportSelectionGroups.Add(group);
                    }
                }
            }

            List<TimeSalaryExportSelectionTransactionDTO> transactionsOnCompanyOnNoGroup = transactionsOnCompany.Where(w => !w.AssignedToGroup && !w.ContainsAnyAccount(addedAccountIds)).ToList();
            if (transactionsOnCompanyOnNoGroup.Any())
            {
                if (accountDimId < 0 && transactionsOnCompanyOnNoGroup.Count == transactionsOnCompany.Count)
                {
                    if (groupOnCategory)
                    {
                        var categories = CategoryManager.GetCategories(SoeCategoryType.Employee, actorCompanyId);

                        if (categories.Any())
                        {
                            foreach (var category in categories.OrderBy(o => o.Name))
                            {
                                var employees = EmployeeManager.GetEmployeesByCategory(category.CategoryId, actorCompanyId, dateFrom, dateTo);
                                if (employees.Any())
                                {
                                    List<int> employeeIds = employees.Select(s => s.EmployeeId).ToList();
                                    List<TimeSalaryExportSelectionTransactionDTO> transactionsOnCategory = transactionsOnCompany.Where(w => employeeIds.Contains(w.EmployeeId)).ToList();
                                    if (transactionsOnCategory.Any())
                                    {
                                        TimeSalaryExportSelectionGroupDTO group = CreateTimeSalaryExportSelectionGroupDTO(null, attestStates, allEmployees, transactionsOnCategory, category.Name);
                                        selectionDTO.TimeSalaryExportSelectionGroups.Add(group);
                                    }
                                }
                            }
                        }
                    }
                    else if (groupOnEmployeeGroup)
                    {
                        var employeeGroups = EmployeeManager.GetEmployeeGroups(actorCompanyId);
                        if (employeeGroups.Any())
                        {
                            var employees = EmployeeManager.GetAllEmployees(actorCompanyId, true, true);
                            var employeeCalenderDTOs = EmployeeManager.GetEmploymentCalenderDTOs(employees, dateFrom, dateTo, employeeGroups: employeeGroups);
                            foreach (var employeeGroup in employeeGroups.OrderBy(o => o.Name))
                            {
                                List<int> employeeIds = employeeCalenderDTOs.Where(w => w.EmployeeGroupId == employeeGroup.EmployeeGroupId).Select(s => s.EmployeeId).ToList();
                                if (employeeIds.Any())
                                {
                                    List<TimeSalaryExportSelectionTransactionDTO> transactionsOnEmployeeGroup = transactionsOnCompany.Where(w => employeeIds.Contains(w.EmployeeId)).ToList();
                                    if (transactionsOnEmployeeGroup.Any())
                                    {
                                        TimeSalaryExportSelectionGroupDTO group = CreateTimeSalaryExportSelectionGroupDTO(null, attestStates, allEmployees, transactionsOnEmployeeGroup, employeeGroup.Name);
                                        selectionDTO.TimeSalaryExportSelectionGroups.Add(group);
                                    }
                                }
                            }
                        }
                    }
                    else if (groupOnPayrollGroup)
                    {
                        var payrollGroups = PayrollManager.GetPayrollGroups(actorCompanyId);
                        if (payrollGroups.Any())
                        {
                            var employees = EmployeeManager.GetAllEmployees(actorCompanyId, true, true);
                            var employeeCalenderDTOs = EmployeeManager.GetEmploymentCalenderDTOs(employees, dateFrom, dateTo, payrollGroups: payrollGroups);
                            foreach (var payrollGroup in payrollGroups.OrderBy(o => o.Name))
                            {
                                List<int> employeeIds = employeeCalenderDTOs.Where(w => w.PayrollGroupId == payrollGroup.PayrollGroupId).Select(s => s.EmployeeId).ToList();
                                if (employeeIds.Any())
                                {
                                    List<TimeSalaryExportSelectionTransactionDTO> transactionsOnPayrollGroup = transactionsOnCompany.Where(w => employeeIds.Contains(w.EmployeeId)).ToList();
                                    if (transactionsOnPayrollGroup.Any())
                                    {
                                        TimeSalaryExportSelectionGroupDTO group = CreateTimeSalaryExportSelectionGroupDTO(null, attestStates, allEmployees, transactionsOnPayrollGroup, payrollGroup.Name);
                                        selectionDTO.TimeSalaryExportSelectionGroups.Add(group);
                                    }
                                }
                            }
                        }
                    }
                    else if (groupOnPayrollCompany)
                    {
                        var transactionsOnEmployeeDict = transactionsOnCompany.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());
                        var employees = EmployeeManager.GetAllEmployees(actorCompanyId, true, true);
                        var employeeAccounts = EmployeeManager.GetEmployeeAccounts(actorCompanyId, employees.Select(s => s.EmployeeId).ToList(), dateFrom, dateTo);
                        var employeeAccountsOnEmployeeDict = employeeAccounts.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());

                        if (employeeAccounts.Any())
                        {
                            var externalCodes = ActorManager.GetCompanyExternalCodes(TermGroup_CompanyExternalCodeEntity.AccountHierachyPayrollExport, actorCompanyId);

                            if (externalCodes.Any())
                            {
                                var recordIds = externalCodes.Select(s => s.RecordId).ToList();
                                var accountsWithCode = entitiesReadOnly.Account.Where(w => w.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active && recordIds.Contains(w.AccountId)).ToList();
                                accountsWithCode.ForEach(f => f.AccountHierachyPayrollExportExternalCode = externalCodes.FirstOrDefault(fd => fd.RecordId == f.AccountId)?.ExternalCode ?? "");
                                foreach (var accountGroup in accountsWithCode.GroupBy(g => g.AccountHierachyPayrollExportExternalCode).OrderBy(o => o.Key))
                                {
                                    var accountsWithCodeOnGroup = accountsWithCode.Where(w => w.AccountHierachyPayrollExportExternalCode == accountGroup.Key).ToList();
                                    List<int> employeeIds = new List<int>();
                                    foreach (var employee in employees)
                                    {
                                        var allAmployeeAccountsOnEmployee = employeeAccountsOnEmployeeDict.ContainsKey(employee.EmployeeId) ? employeeAccountsOnEmployeeDict.GetValue(employee.EmployeeId) : new List<EmployeeAccount>();

                                        var employeeAccountsOnEmployee = allAmployeeAccountsOnEmployee.Where(w => w.AccountId.HasValue && accountsWithCodeOnGroup.Select(s => s.AccountId).ToList().Contains(w.AccountId.Value)).ToList();

                                        if (employeeAccountsOnEmployee.Any())
                                            employeeIds.Add(employee.EmployeeId);
                                    }

                                    if (employeeIds.Any())
                                    {
                                        List<TimeSalaryExportSelectionTransactionDTO> transactionsOnPayrollGroup = transactionsOnEmployeeDict.GetList(employeeIds);

                                        if (transactionsOnPayrollGroup.Any())
                                        {
                                            TimeSalaryExportSelectionGroupDTO group = CreateTimeSalaryExportSelectionGroupDTO(null, attestStates, allEmployees, transactionsOnPayrollGroup, accountGroup.Key);
                                            selectionDTO.TimeSalaryExportSelectionGroups.Add(group);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    transactionsOnCompanyOnNoGroup = transactionsOnCompanyOnNoGroup.Where(w => !w.AssignedToGroup).ToList();
                    TimeSalaryExportSelectionGroupDTO restGroup = CreateTimeSalaryExportSelectionGroupDTO(null, attestStates, allEmployees, transactionsOnCompanyOnNoGroup);
                    selectionDTO.TimeSalaryExportSelectionGroups.Add(restGroup);
                }
                else
                {
                    TimeSalaryExportSelectionGroupDTO group = CreateTimeSalaryExportSelectionGroupDTO(null, attestStates, allEmployees, transactionsOnCompanyOnNoGroup);
                    selectionDTO.TimeSalaryExportSelectionGroups.Add(group);
                }
            }

            string mixedStateName = GetText(3468, "Flera status");
            AttestState attestState = null;
            var groupOnAttestState = selectionDTO.TimeSalaryExportSelectionGroups.GroupBy(g => g.AttestStateId);

            if (groupOnAttestState.Count() == 1 && groupOnAttestState.First().Key != 0)
            {
                attestState = attestStates.First(f => f.AttestStateId == groupOnAttestState.First().Key);
                selectionDTO.AttestStateId = attestState != null ? attestState.AttestStateId : 0;
                selectionDTO.AttestStateColor = attestState != null ? attestState.Color : "#FFFFFF";
            }
            else
            {
                List<int> attestStateIdsOnTransactions = selectionDTO.TimeSalaryExportSelectionGroups.Select(s => s.AttestStateId).Distinct().ToList();
                int? attestStateId = attestStates.Where(w => attestStateIdsOnTransactions.Contains(w.AttestStateId)).GetAttestStateIdLowest();
                selectionDTO.AttestStateId = attestStateId.HasValue ? attestStateId.Value : 0;
                selectionDTO.AttestStateColor = attestStates.Where(w => attestStateIdsOnTransactions.Contains(w.AttestStateId)).GetAttestStateColorLowest();
            }

            selectionDTO.EntirePeriodValidForExport = !selectionDTO.TimeSalaryExportSelectionGroups.Any(a => !a.EntirePeriodValidForExport);
            selectionDTO.AttestStateName = attestState != null ? attestState.Name : mixedStateName;
            return selectionDTO;
        }

        private TimeSalaryExportSelectionGroupDTO CreateTimeSalaryExportSelectionGroupDTO(AccountInternal accInt, List<AttestState> attestStates, List<Employee> allEmployees, List<TimeSalaryExportSelectionTransactionDTO> transactions, string name = "")
        {
            string mixedStateName = GetText(3468, "Flera status");
            transactions.ForEach(f => f.AssignedToGroup = true);

            TimeSalaryExportSelectionGroupDTO group = new TimeSalaryExportSelectionGroupDTO()
            {
                Id = accInt != null ? accInt.AccountId : 0,
                Name = accInt != null ? accInt.Account.Name : string.IsNullOrEmpty(name) ? GetText(3671, "Ogrupperade") : name,
            };

            Dictionary<int, List<TimeSalaryExportSelectionTransactionDTO>> timePayrollTransactionsDict = transactions.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());

            List<int> employeeIds = transactions.Select(s => s.EmployeeId).Distinct().ToList();

            foreach (Employee employee in allEmployees.Where(w => employeeIds.Contains(w.EmployeeId)))
            {
                List<TimeSalaryExportSelectionTransactionDTO> transOnEmployee = timePayrollTransactionsDict.FirstOrDefault(f => f.Key == employee.EmployeeId).Value;

                TimeSalaryExportSelectionEmployeeDTO employeeDTO = new TimeSalaryExportSelectionEmployeeDTO()
                {
                    EmployeeId = employee.EmployeeId,
                    Name = employee.Name,
                    EmployeeNr = employee.EmployeeNr,
                    AttestStateId = 0,
                    EntirePeriodValidForExport = false,
                };

                AttestState employeeAttestState = null;
                var groupOnEmployeeAttestState = transOnEmployee.GroupBy(g => g.AttestStateId);

                if (groupOnEmployeeAttestState.Count() == 1 && groupOnEmployeeAttestState.First().Key != 0)
                {
                    employeeAttestState = attestStates.First(f => f.AttestStateId == groupOnEmployeeAttestState.First().Key);
                    employeeDTO.AttestStateId = employeeAttestState != null ? employeeAttestState.AttestStateId : 0;
                    employeeDTO.EntirePeriodValidForExport = true;
                    employeeDTO.AttestStateColor = employeeAttestState != null ? employeeAttestState.Color : "#FFFFFF";
                }
                else
                {
                    List<int> attestStateIdsOnTransactions = transOnEmployee.Select(s => s.AttestStateId).Distinct().ToList();
                    int? attestStateId = attestStates.Where(w => attestStateIdsOnTransactions.Contains(w.AttestStateId)).GetAttestStateIdLowest();
                    employeeDTO.AttestStateId = attestStateId.HasValue ? attestStateId.Value : 0;
                    employeeDTO.AttestStateColor = attestStates.Where(w => attestStateIdsOnTransactions.Contains(w.AttestStateId)).GetAttestStateColorLowest();
                    employeeDTO.EntirePeriodValidForExport = false;
                }

                employeeDTO.AttestStateName = employeeAttestState != null ? employeeAttestState.Name : mixedStateName;
                group.TimeSalaryExportSelectionEmployees.Add(employeeDTO);
            }

            AttestState attestState = null;
            var groupOnAttestState = transactions.GroupBy(g => g.AttestStateId);
            if (groupOnAttestState.Count() == 1 && groupOnAttestState.First().Key != 0)
                attestState = attestStates.First(f => f.AttestStateId == groupOnAttestState.First().Key);

            if (groupOnAttestState.Count() == 1 && groupOnAttestState.First().Key != 0)
            {
                attestState = attestStates.First(f => f.AttestStateId == groupOnAttestState.First().Key);
                group.AttestStateId = attestState != null ? attestState.AttestStateId : 0;
                group.EntirePeriodValidForExport = true;
                group.AttestStateColor = attestState != null ? attestState.Color : "#FFFFFF";
            }
            else
            {
                List<int> attestStateIdsOnTransactions = transactions.Select(s => s.AttestStateId).Distinct().ToList();
                int? attestStateId = attestStates.Where(w => attestStateIdsOnTransactions.Contains(w.AttestStateId)).GetAttestStateIdLowest();
                group.AttestStateId = attestStateId.HasValue ? attestStateId.Value : 0;
                group.EntirePeriodValidForExport = false;
                group.AttestStateColor = attestStates.Where(w => attestStateIdsOnTransactions.Contains(w.AttestStateId)).GetAttestStateColorLowest();
            }

            group.AttestStateName = attestState != null ? attestState.Name : mixedStateName;

            return group;
        }

        private Dictionary<int, List<TimeSalaryExportSelectionTransactionDTO>> GetTimeSalaryExportSelectionTransactions(int actorCompanyId, DateTime dateFrom, DateTime dateTo, bool applyAccounting)
        {
            Dictionary<int, List<TimeSalaryExportSelectionTransactionDTO>> companyTimePayrollTransactionDTOs = new Dictionary<int, List<TimeSalaryExportSelectionTransactionDTO>>();
            List<TimeSalaryExportSelectionTransactionDTO> dtos = new List<TimeSalaryExportSelectionTransactionDTO>();
            //List<PayrollProduct> payrollProducts = ProductManager.GetPayrollProducts(actorCompanyId, null);
            //List<int> excludeProductIds = payrollProducts.Where(w => !w.Export).Select(s => s.ProductId).ToList();

            using (SqlConnection connection = new SqlConnection(FrownedUponSQLClient.GetADOConnectionString(300)))
            {
                var sql = $@"	
	             	        SELECT 		                                
		                        tpt.EmployeeId, 
                                tpt.PayrollProductId, 
                                tpt.AttestStateId, 
                                tpta.AccountId ,
                                tpt.timepayrolltransactionid as id,
                                LEFT(o.list,LEN(o.list) - 1) AS AccountInternalsStr
                                from 
                                TimePayrollTransaction tpt  WITH (NOLOCK) 
                                inner join TimeBlockDate tbd  WITH (NOLOCK) on tbd.TimeBlockDateId = tpt.TimeBlockDateId 
                                left outer join TimePayrollTransactionAccount tpta  WITH (NOLOCK) on tpta.TimePayrollTransactionId = tpt.TimePayrollTransactionId
                                inner join Employee e on tpt.EmployeeId = e.EmployeeId
                                CROSS APPLY 
		                                 (
			                                SELECT   
				                                ' ' + '|' + CONVERT(VARCHAR(100),ta.AccountId) + '|' + ' ' + '|' + ' '  + ',' AS [text()]
			                                FROM
				                                dbo.TimePayrollTransactionAccount AS ta WITH (NOLOCK)
				                                WHERE    
				                                ta.TimePayrollTransactionId = tpt.TimePayrollTransactionId
			                                FOR XML PATH('')
		                                 ) o(list)
                                WHERE
		                                (tbd.Date BETWEEN '{CalendarUtility.ToSqlFriendlyDateTime(dateFrom)}' AND '{CalendarUtility.ToSqlFriendlyDateTime(dateTo)}') AND
                                        e.ExcludeFromPayroll = 0 AND
		                                tpt.ActorCompanyId = {actorCompanyId} and
		                                tpt.[State] = 0 and (tpt.Amount <> 0 or tpt.Quantity <> 0)
	                                    ORDER BY
		                                tbd.EmployeeId, tbd.date";

                var reader = FrownedUponSQLClient.ExcuteQuery(connection, sql, 300);

                if (reader != null)
                {
                    int prevEmployeeId = 0;

                    while (reader.Read())
                    {
                        int currentEmployeeId = (int)reader["employeeid"];

                        if (currentEmployeeId != prevEmployeeId)
                        {
                            companyTimePayrollTransactionDTOs.Add(prevEmployeeId, dtos);
                            dtos = new List<TimeSalaryExportSelectionTransactionDTO>();
                            prevEmployeeId = currentEmployeeId;
                        }

                        var dto = new TimeSalaryExportSelectionTransactionDTO();

                        dto.AttestStateId = (int)reader["atteststateid"];
                        dto.EmployeeId = (int)reader["employeeid"];
                        dto.PayrollProductId = (int)reader["payrollproductid"];
                        dto.Id = (int)reader["id"];
                        dto.AccountInternals = applyAccounting ? reader["accountinternalsstr"].ToString() != "" ? TimeTransactionManager.GetAccountInternalDTOs(reader["accountinternalsstr"].ToString(), ignoreAccountDimNr: true) : new List<AccountInternalDTO>() : new List<AccountInternalDTO>();
                        dto.AccountInternalIds = dto.AccountInternals.Select(s => s.AccountId).ToList();
                        dtos.Add(dto);
                    }

                    if (dtos.Any())
                    {
                        //dtos = dtos.Where(w => !excludeProductIds.Contains(w.PayrollProductId)).ToList();
                        companyTimePayrollTransactionDTOs.Add(prevEmployeeId, dtos);
                    }
                }
            }

            return companyTimePayrollTransactionDTOs;
        }

        #endregion

        #region Import

        public Dictionary<int, string> GetTimeSalaryImportTypes()
        {
            var result = new Dictionary<int, string>();

            foreach (string e in Enum.GetNames(typeof(SoeDataStorageRecordType)))
            {
                if (e == Enum.GetName(typeof(SoeDataStorageRecordType), SoeDataStorageRecordType.TimeSalaryExportEmployee))
                    result.Add((int)SoeDataStorageRecordType.TimeSalaryExportEmployee, GetText(9092, "SoftOne Lön Lönespecifikation"));
                else if (e == Enum.GetName(typeof(SoeDataStorageRecordType), SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee))
                    result.Add((int)SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee, GetText(9093, "SoftOne Lön Kontrolluppgift"));
                else if (e == Enum.GetName(typeof(SoeDataStorageRecordType), SoeDataStorageRecordType.TimeSalaryExportSaumaPdf))
                    result.Add((int)SoeDataStorageRecordType.TimeSalaryExportSaumaPdf, "SoftOne SAUMA PDF");
                else if (e == Enum.GetName(typeof(SoeDataStorageRecordType), SoeDataStorageRecordType.TimeKU10ExportEmployee))
                    result.Add((int)SoeDataStorageRecordType.TimeKU10ExportEmployee, GetText(11021, "SoftOne GO Kontrolluppgift"));
            }

            return result;
        }

        public ActionResult ImportTimeSalary(XDocument xdoc, int timePeriodId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Structure of XML

                        /**
                         * <lonespec_export>
                         *      
                         *      //Parent (common)
                         *      <company>
                         *          ...
                         *      </company> 
                         *      <period>
                         *          ...
                         *      </period>
                         *      
                         *      //Children (employees)
                         *      <lonespecar>     
                         *          <person>
                         *              <period>
                         *                  ...
                         *              </period>
                         *              <person>
                         *                  ...
                         *              </person>
                         *              <employee>
                         *                  ...
                         *              </employee>
                         *              <holiday>
                         *                  ...
                         *              </holiday>
                         *              <benefit>
                         *                  ...
                         *              </benefit>
                         *              <summary>
                         *                  ...
                         *              </summary>
                         *              <rows>
                         *                  ...
                         *              </rows>
                         *          </person>
                         *      </lonespecar> 
                         * </lonespec_export>
                         * 
                         **/

                        #endregion

                        #region Parent

                        //company
                        XElement elementCompany = XmlUtil.GetChildElement(xdoc, "company");
                        if (elementCompany == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "company");

                        //period
                        XElement elementPeriod = XmlUtil.GetChildElement(xdoc, "period");
                        if (elementPeriod == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "period");

                        DataStorage parentDataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.TimeSalaryExport, xdoc.ToString(), null, timePeriodId, null, actorCompanyId);

                        #endregion

                        #region Children

                        //lonespecar
                        XElement elementSalarys = XmlUtil.GetChildElement(xdoc, "lonespecar");
                        if (elementSalarys != null)
                        {
                            #region Employees

                            //person
                            List<XElement> elementsPerson = XmlUtil.GetChildElements(elementSalarys, "person");
                            foreach (XElement elementPerson in elementsPerson)
                            {
                                DataStorage employeeDataStorage = CreateEmployeeSalaryDataStorage(entities, elementPerson, timePeriodId, actorCompanyId);
                                if (employeeDataStorage != null)
                                {
                                    #region Build XML

                                    XDocument xdocEmployee = new XDocument();

                                    //Create Element root
                                    XElement rootEmployee = new XElement(xdoc.Root.Name.LocalName);

                                    //Create Element "company" - copy whole content from original XML
                                    rootEmployee.Add(elementCompany);

                                    //Create Element "period" - copy whole content from original XML
                                    rootEmployee.Add(elementPeriod);

                                    //Create Element "lonespecar"
                                    XElement elementSalarysEmployee = new XElement(elementSalarys.Name.LocalName);

                                    //Create Element "person" - copy whole content from original XML
                                    elementSalarysEmployee.Add(elementPerson);

                                    //Close document
                                    rootEmployee.Add(elementSalarysEmployee);
                                    xdocEmployee.Add(rootEmployee);

                                    #endregion

                                    //Set XML on DataStorage
                                    employeeDataStorage.XML = xdocEmployee.ToString();

                                    //Add to parent
                                    parentDataStorage.Children.Add(employeeDataStorage);
                                }
                            }

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
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
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult ImportTimeSalaryControlInfo(XDocument xdoc, int timePeriodId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Structure of XML

                        /**
                         * <table>
                         *      <ku>
                         *         <ku_row>
                         *      
                         *      //One row per employee

                         *         </ku_row>
                         *      </ku> 
                         * </table>
                         * 
                         **/

                        #endregion

                        #region Parent

                        ////company
                        //XElement elementCompany = XmlUtil.GetElement(xdoc, "company");
                        //if (elementCompany == null)
                        //    return new ActionResult((int)ActionResultSave.EntityNotFound, "company");

                        ////period
                        //XElement elementPeriod = XmlUtil.GetElement(xdoc, "period");
                        //if (elementPeriod == null)
                        //    return new ActionResult((int)ActionResultSave.EntityNotFound, "period");

                        DataStorage parentDataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.TimeSalaryExportControlInfo, xdoc.ToString(), null, timePeriodId, null, actorCompanyId);

                        #endregion

                        #region Children

                        //lonespecar
                        XElement elementku = XmlUtil.GetChildElement(xdoc, "ku");
                        XElement elementku_row = null;
                        if (elementku == null)
                        {
                            elementku_row = XmlUtil.GetChildElement(xdoc, "ku_row");
                        }
                        else
                        {
                            elementku_row = XmlUtil.GetChildElement(elementku, "ku_row");
                        }

                        if (elementku_row != null)
                        {
                            #region Employees

                            //person
                            List<XElement> elementsPerson = XmlUtil.GetChildElements(elementku_row, "ku_group1");
                            foreach (XElement elementPerson in elementsPerson)
                            {
                                DataStorage employeeDataStorage = CreateEmployeeSalaryControlInfoDataStorage(entities, elementPerson, timePeriodId, actorCompanyId);
                                if (employeeDataStorage != null)
                                {
                                    #region Build XML

                                    XDocument xdocEmployee = new XDocument();

                                    //Create Element root
                                    XElement rootEmployee = new XElement("ku");

                                    ////Create Element "company" - copy whole content from original XML
                                    //rootEmployee.Add(elementCompany);

                                    ////Create Element "period" - copy whole content from original XML
                                    //rootEmployee.Add(elementPeriod);

                                    //Create Element "lonespecar"
                                    XElement elementSalarysEmployee = new XElement(elementku_row.Name.LocalName);

                                    //Create Element "person" - copy whole content from original XML
                                    elementSalarysEmployee.Add(elementPerson);

                                    //Close document
                                    rootEmployee.Add(elementSalarysEmployee);
                                    xdocEmployee.Add(rootEmployee);

                                    #endregion

                                    //Set XML on DataStorage
                                    employeeDataStorage.XML = xdocEmployee.ToString();

                                    //Add to parent
                                    parentDataStorage.Children.Add(employeeDataStorage);
                                }
                            }

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
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
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult ImportKU10(XDocument xdoc, int timePeriodId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Parent

                        //KU10Report
                        XElement elementKU10Report = XmlUtil.GetChildElement(xdoc, "KU10Report");
                        if (elementKU10Report == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportHeader");

                        //ReportHeader
                        XElement elementReportHeader = XmlUtil.GetChildElement(elementKU10Report, "ReportHeader");
                        if (elementReportHeader == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportHeader");

                        DataStorage parentDataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.TimeKU10Export, xdoc.ToString(), null, timePeriodId, null, actorCompanyId);

                        #endregion

                        #region Children

                        #region Employees

                        //person
                        List<XElement> elementsEmployee = XmlUtil.GetChildElements(elementKU10Report, "Employee");
                        foreach (XElement elementEmployee in elementsEmployee)
                        {
                            DataStorage employeeDataStorage = CreateEmployeeKU10DataStorage(entities, elementEmployee, timePeriodId, actorCompanyId);
                            if (employeeDataStorage != null)
                            {
                                #region Build XML

                                XDocument xdocEmployee = new XDocument();

                                //Create Element root
                                XElement rootEmployee = new XElement(elementKU10Report.Name.LocalName);

                                //Create Element "ReportHeader" - copy whole content from original XML
                                rootEmployee.Add(elementReportHeader);

                                //Create Element "person" - copy whole content from original XML
                                rootEmployee.Add(elementEmployee);

                                //Close document
                                xdocEmployee.Add(rootEmployee);

                                #endregion

                                //Set XML on DataStorage
                                employeeDataStorage.XML = xdocEmployee.ToString();

                                //Add to parent
                                parentDataStorage.Children.Add(employeeDataStorage);
                            }
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
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
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult ImportTimeSalarySaumaSpecification(byte[] data, string employeeNr, int timePeriodId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Prereq

                    Employee employee = EmployeeManager.GetEmployeeByNr(employeeNr, actorCompanyId);
                    if (employee == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Employee");

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.TimeSalaryExportSaumaPdf, null, data, timePeriodId, employee.EmployeeId, actorCompanyId, "Palkkalaskelma.pdf");

                        if (data != null)
                        {
                            result = SaveChanges(entities, transaction);

                            //Commit transaction
                            if (result.Success)
                                transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #region Privat methods

        private DataStorage CreateEmployeeSalaryDataStorage(CompEntities entities, XElement personElement, int timePeriodId, int actorCompanyId)
        {
            #region Prereq

            //period
            //TODO: Parse when needed

            //person
            //TODO: Parse when needed

            //employee
            XElement elementEmployee = XmlUtil.GetChildElement(personElement, "employee");
            if (elementEmployee == null)
                return null;

            string employeeNr = XmlUtil.GetChildElementValue(elementEmployee, "e_enum");
            Employee employee = EmployeeManager.GetEmployeeByNr(entities, employeeNr, actorCompanyId);
            if (employee == null)
                return null;

            #endregion

            return GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.TimeSalaryExportEmployee, null, null, timePeriodId, employee.EmployeeId, actorCompanyId);
        }

        private DataStorage CreateEmployeeSalaryControlInfoDataStorage(CompEntities entities, XElement personElement, int timePeriodId, int actorCompanyId)
        {
            #region Prereq

            //Employee
            string employeeNr = XmlUtil.GetChildElementValue(personElement, "xs_emp_num");
            Employee employee = EmployeeManager.GetEmployeeByNr(entities, employeeNr, actorCompanyId);
            if (employee == null)
                return null;

            #endregion

            return GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee, null, null, timePeriodId, employee.EmployeeId, actorCompanyId);
        }

        private DataStorage CreateEmployeeKU10DataStorage(CompEntities entities, XElement elementEmployee, int timePeriodId, int actorCompanyId)
        {
            #region Prereq

            //employee
            if (elementEmployee == null)
                return null;

            string employeeNr = XmlUtil.GetChildElementValue(elementEmployee, "EmployeeNr");
            Employee employee = EmployeeManager.GetEmployeeByNr(entities, employeeNr, actorCompanyId);
            if (employee == null)
                return null;

            #endregion

            return GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.TimeKU10ExportEmployee, null, null, timePeriodId, employee.EmployeeId, actorCompanyId);
        }

        #endregion

        #endregion

        #region Export

        public List<TimeSalaryExportDTO> GetTimeSalaryExportDTOs(int actorCompanyId, bool removeData)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<TimeSalaryExportDTO> exports = (from tse in entitiesReadOnly.TimeSalaryExport
                                                 where tse.State == (int)SoeEntityState.Active &&
                                                 tse.Company.ActorCompanyId == actorCompanyId
                                                 orderby tse.ExportDate descending
                                                 select new TimeSalaryExportDTO()
                                                 {
                                                     TimeSalaryExportId = tse.TimeSalaryExportId,
                                                     ActorCompanyId = tse.Company != null ? tse.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                                                     StartInterval = tse.StartInterval,
                                                     StopInterval = tse.StopInterval,
                                                     ExportDate = tse.ExportDate,
                                                     ExportTarget = (SoeTimeSalaryExportTarget)tse.ExportTarget,
                                                     ExportFormat = (SoeTimeSalaryExportFormat)tse.ExportFormat,
                                                     Extension = tse.Extension,
                                                     Created = tse.Created,
                                                     CreatedBy = tse.CreatedBy,
                                                     Modified = tse.Modified,
                                                     ModifiedBy = tse.ModifiedBy,
                                                     State = (SoeEntityState)tse.State,
                                                     Comment = tse.Comment,
                                                     IsPreliminary = tse.IsPreliminary
                                                 }).ToList();

            List<GenericType> yesNo = GetTermGroupContent(TermGroup.YesNo, skipUnknown: true);
            string yesText = yesNo.FirstOrDefault(x => x.Id == (int)TermGroup_YesNo.Yes)?.Name ?? string.Empty;
            string noText = yesNo.FirstOrDefault(x => x.Id == (int)TermGroup_YesNo.No)?.Name ?? string.Empty;

            foreach (var export in exports)
            {
                export.IsPreliminaryText = export.IsPreliminary ? yesText : noText;

                switch (export.ExportTarget)
                {
                    case SoeTimeSalaryExportTarget.KontekLon:
                        export.TargetName = GetText(4502, "Kontek lön");
                        break;
                    case SoeTimeSalaryExportTarget.SoeXe:
                        export.TargetName = GetText(4501, "SoftOne");
                        break;
                    case SoeTimeSalaryExportTarget.SoftOne:
                        export.TargetName = GetText(4511, "SoftOne lön");
                        break;
                    case SoeTimeSalaryExportTarget.Hogia214006:
                        export.TargetName = GetText(8208, "Hogia typ 214006");
                        break;
                    case SoeTimeSalaryExportTarget.Hogia214002:
                        export.TargetName = GetText(8221, "Hogia typ 214002");
                        break;
                    case SoeTimeSalaryExportTarget.Hogia214007:
                        export.TargetName = GetText(11768, "Hogia typ 214007");
                        break;
                    case SoeTimeSalaryExportTarget.Sauma:
                        export.TargetName = GetText(8152, "Sauma");
                        break;
                    case SoeTimeSalaryExportTarget.Personec:
                        export.TargetName = GetText(8174, "Personec");
                        break;
                    case SoeTimeSalaryExportTarget.AgdaLon:
                        export.TargetName = GetText(8225, "Agda Lön");
                        break;
                    case SoeTimeSalaryExportTarget.Spcs:
                        export.TargetName = GetText(8226, "Visma 600");
                        break;
                    case SoeTimeSalaryExportTarget.PAxml:
                        export.TargetName = GetText(9077, "PAXml");
                        break;
                    case SoeTimeSalaryExportTarget.PAxml2_1:
                        export.TargetName = GetText(12512, "PAXml 2.1");
                        break;
                    case SoeTimeSalaryExportTarget.DiLonn:
                        export.TargetName = GetText(9078, "DI Lönn");
                        break;
                    case SoeTimeSalaryExportTarget.Flex:
                        export.TargetName = GetText(9113, "Flex Lön");
                        break;
                    case SoeTimeSalaryExportTarget.Tikon:
                        export.TargetName = "Tikon";
                        break;
                    case SoeTimeSalaryExportTarget.TikonCSV:
                        export.TargetName = "TikonCSV";
                        break;
                    case SoeTimeSalaryExportTarget.DLPrime3000:
                        export.TargetName = "DLPrime3000";
                        break;
                    case SoeTimeSalaryExportTarget.BlueGarden:
                        export.TargetName = "BlueGarden";
                        break;
                    case SoeTimeSalaryExportTarget.Orkla:
                        export.TargetName = "Orkla";
                        break;
                    case SoeTimeSalaryExportTarget.SvenskLon:
                        export.TargetName = GetText(8036, "Svensk Lön");
                        break;
                    case SoeTimeSalaryExportTarget.Fivaldi:
                        export.TargetName = GetText(4942, "Fivaldi");
                        break;
                    case SoeTimeSalaryExportTarget.AditroL1:
                        export.TargetName = GetText(11781, "Aditro L");
                        break;
                    case SoeTimeSalaryExportTarget.HuldtOgLillevik:
                        export.TargetName = GetText(11918, "Huldt og Lillevik");
                        break;
                    case SoeTimeSalaryExportTarget.Netvisor:
                        export.TargetName = "Netvisor";
                        break;
                    case SoeTimeSalaryExportTarget.Pol:
                        export.TargetName = "Pol";
                        break;
                    case SoeTimeSalaryExportTarget.SDWorx:
                        export.TargetName = GetText(12388, "SD Worx");   
                        break;
                    case SoeTimeSalaryExportTarget.Lessor:
                        export.TargetName = GetText(12389, "Lessor");
                        break;
                }

                if (removeData)
                {
                    export.File1 = new byte[0];
                    export.File2 = new byte[0];
                }
            }

            return exports;
        }

        public TimeSalaryExport GetTimeSalaryExport(int timeSalaryExportId, int actorCompanyId, bool includeRows)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeSalaryExport.NoTracking();
            return GetTimeSalaryExport(entities, timeSalaryExportId, actorCompanyId, includeRows);
        }

        public TimeSalaryExport GetTimeSalaryExport(CompEntities entities, int timeSalaryExportId, int actorCompanyId, bool includeRows)
        {
            if (includeRows)
            {
                return (from tse in entities.TimeSalaryExport
                            .Include("TimeSalaryExportRow")
                        where tse.State == (int)SoeEntityState.Active &&
                        tse.TimeSalaryExportId == timeSalaryExportId &&
                        tse.Company.ActorCompanyId == actorCompanyId
                        select tse).FirstOrDefault();
            }
            else
            {
                return (from tse in entities.TimeSalaryExport
                        where tse.State == (int)SoeEntityState.Active &&
                        tse.TimeSalaryExportId == timeSalaryExportId &&
                        tse.Company.ActorCompanyId == actorCompanyId
                        select tse).FirstOrDefault();
            }
        }

        public DateTime? GetPayedDate(XDocument xdoc)
        {
            DateTime? payedDate = null;

            if (xdoc != null)
            {
                XElement elementPeriod = XmlUtil.GetChildElement(xdoc, "period");
                if (elementPeriod != null)
                {
                    //PayedDate
                    DateTime d;
                    if (DateTime.TryParse(XmlUtil.GetChildElementValue(elementPeriod, "w_period_payed_date"), out d))
                        payedDate = d;
                }
            }

            return payedDate;
        }

        public List<Tuple<TermGroup_AttestEntity, int, int?>> GetTimeSalaryExportRowIds(CompEntities entities, int timeSalaryExportId, int actorCompanyId)
        {
            List<Tuple<TermGroup_AttestEntity, int, int?>> exportRows = new List<Tuple<TermGroup_AttestEntity, int, int?>>();

            var rows = (from tse in entities.TimeSalaryExportRow
                        where tse.TimeSalaryExport.TimeSalaryExportId == timeSalaryExportId
                        select new
                        {
                            TermGroup_AttestEntity = (TermGroup_AttestEntity)tse.Entity,
                            transactionId = tse.RecordId,
                            EmployeeId = tse.EmployeeId,
                        }).ToList();

            foreach (var item in rows)
            {
                exportRows.Add(Tuple.Create(item.TermGroup_AttestEntity, item.transactionId, item.EmployeeId));
            }

            return exportRows;
        }

        public Dictionary<int, string> GetExportTargetsDict(bool addEmptyRow)
        {
            var result = new Dictionary<int, string>();

            if (addEmptyRow)
                result.Add(0, " ");

            foreach (string e in Enum.GetNames(typeof(SoeTimeSalaryExportTarget)))
            {
                if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.SoeXe))
                    result.Add((int)SoeTimeSalaryExportTarget.SoeXe, GetText(8229, "SoftOne"));
                // Kontek is replaced by PaXML so for now this is inactivated / Rickard
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.SoftOne))
                    result.Add((int)SoeTimeSalaryExportTarget.SoftOne, GetText(4511, "SoftOne lön"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.SvenskLon))
                    result.Add((int)SoeTimeSalaryExportTarget.SvenskLon, GetText(8036, "Svensk Lön"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Sauma))
                    result.Add((int)SoeTimeSalaryExportTarget.Sauma, GetText(8152, "Sauma"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Hogia214006))
                    result.Add((int)SoeTimeSalaryExportTarget.Hogia214006, GetText(8203, "Hogia 214006"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Hogia214002))
                    result.Add((int)SoeTimeSalaryExportTarget.Hogia214002, GetText(8222, "Hogia 214002"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Hogia214007)) //Open in March 2019 according to order from Håkan Lord
                    result.Add((int)SoeTimeSalaryExportTarget.Hogia214007, GetText(11769, "Hogia 214007"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Personec))
                    result.Add((int)SoeTimeSalaryExportTarget.Personec, GetText(8224, "Personec"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.AgdaLon))
                    result.Add((int)SoeTimeSalaryExportTarget.AgdaLon, GetText(8228, "Agda Lön"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Spcs))
                    result.Add((int)SoeTimeSalaryExportTarget.Spcs, GetText(8226, "Visma 600"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.PAxml))
                    result.Add((int)SoeTimeSalaryExportTarget.PAxml, GetText(8233, "PAXml"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.PAxml2_1))
                    result.Add((int)SoeTimeSalaryExportTarget.PAxml2_1, GetText(9371, "PAXml 2.1"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.DiLonn))
                    result.Add((int)SoeTimeSalaryExportTarget.DiLonn, GetText(8232, "Dilonn"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Flex))
                    result.Add((int)SoeTimeSalaryExportTarget.Flex, GetText(5953, "Flex Lön"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Tikon))
                    result.Add((int)SoeTimeSalaryExportTarget.Tikon, "Tikon");
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.TikonCSV))
                    result.Add((int)SoeTimeSalaryExportTarget.TikonCSV, "TikonCSV");
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Orkla))
                    result.Add((int)SoeTimeSalaryExportTarget.Orkla, "Orkla");
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.DLPrime3000))
                    result.Add((int)SoeTimeSalaryExportTarget.DLPrime3000, "DLPrime3000");
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.BlueGarden))
                    result.Add((int)SoeTimeSalaryExportTarget.BlueGarden, "BlueGarden");
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Fivaldi))
                    result.Add((int)SoeTimeSalaryExportTarget.Fivaldi, "Fivaldi");
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.AditroL1))
                    result.Add((int)SoeTimeSalaryExportTarget.AditroL1, GetText(11781, "Aditro L"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.HuldtOgLillevik))
                    result.Add((int)SoeTimeSalaryExportTarget.HuldtOgLillevik, GetText(11918, "Huldt og Lillevik"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Netvisor))
                    result.Add((int)SoeTimeSalaryExportTarget.Netvisor, "Netvisor");
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Pol))
                    result.Add((int)SoeTimeSalaryExportTarget.Pol, "Pol");
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.SDWorx))
                    result.Add((int)SoeTimeSalaryExportTarget.SDWorx, GetText(12388, "SD Worx"));
                else if (e == Enum.GetName(typeof(SoeTimeSalaryExportTarget), SoeTimeSalaryExportTarget.Lessor))
                    result.Add((int)SoeTimeSalaryExportTarget.Lessor, GetText(12389, "Lessor"));

            }

            return result;
        }

        public ActionResult ValidateExport(int actorCompanyId, List<int> employeeIds, DateTime startDate, DateTime stopDate)
        {
            ActionResult result = new ActionResult();

            return result;
        }

        public ActionResult Export(DateTime startDate, DateTime stopDate, int actorCompanyId, int userId, bool lockPeriod, bool isPreliminary)
        {
            var employees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, base.RoleId, startDate, stopDate).Where(w => !w.ExcludeFromPayroll && !w.Vacant && !w.Hidden).ToList();
            var exportTarget = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportTarget, userId, actorCompanyId, 0);
            using (CompEntities entities = new CompEntities())
            {
                return Export(entities, employees.Select(s => s.EmployeeId).ToList(), startDate, stopDate, exportTarget, actorCompanyId, userId, SoeModule.Time, lockPeriod, isPreliminary);
            }
        }

        public ActionResult Export(List<int> employeeIds, DateTime startDate, DateTime stopDate, int exportTarget, int actorCompanyId, int userId, SoeModule module, bool lockPeriod, bool isPreliminary)
        {
            employeeIds = employeeIds.Distinct().ToList();
            using (CompEntities entities = new CompEntities())
            {
                return Export(entities, employeeIds, startDate, stopDate, exportTarget, actorCompanyId, userId, module, lockPeriod, isPreliminary);
            }
        }

        public ActionResult Export(CompEntities entities, List<int> employeeIds, DateTime startDate, DateTime stopDate, int exportTarget, int actorCompanyId, int userId, SoeModule module, bool lockPeriod, bool isPreliminary)
        {
            var salaryExportResult = new SalaryExportResult();
            employeeIds = employeeIds.Distinct().ToList();
            entities.CommandTimeout = 240; // 4 minutes

            if (employeeIds.Count > 3000)
                entities.CommandTimeout = 600; // 10 minutes

            string employeeIdsString = employeeIds.JoinToString(",");
            var taskWatchLogId = StartTask(
                "ExportTimeSalary", 
                MethodBase.GetCurrentMethod().DeclaringType.ToString(), 
                Guid.NewGuid().ToString(), 
                $"StartDate {startDate.ToShortDateString()} stopDate {stopDate.ToShortDateString()} exportTarget {exportTarget} ", 
                employeeIds.Count, 
                Convert.ToInt32((stopDate - startDate).TotalDays)
                );

            try
            {
                LogInfo("TimeSalaryExport: Begin Export " + actorCompanyId);

                DateTime selectionStartDate = startDate;
                DateTime selectionStopDate = stopDate;

                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (company == null)
                    return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8162, "Företag kunde inte hittas"));

                #region Init

                bool useBulk = true; // SettingManager.GetBoolSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.UseBulkInSalaryExport, 0, 0);
                bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, base.RoleId, base.ActorCompanyId, entities: entities);
                string externalExportId = TimeSalaryManager.GetExternalExportId(entities, actorCompanyId);
                string externalExportId2 = TimeSalaryManager.GetExternalExportSubId(entities, actorCompanyId);
                var includeSettings = false;
                
                if ((SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.Lessor)
                    includeSettings = true;

                AttestTransition attestPayrollTransition = null;
                AttestTransition attestInvoiceTransition = null;
                AttestState attestStateMinInvoice = null;
                AttestState attestStateMinPayroll = null;
                AttestState attestStateResultingInvoice = null;
                AttestState attestStateResultingPayroll = null;
                List<TimeScheduleTemplateBlock> employeeScheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
                List<TimeStampEntryDTO> timeStampEntries = new List<TimeStampEntryDTO>();

                if (!isPreliminary)
                {
                    attestStateMinInvoice = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportInvoiceMinimumAttestStatus, 0, actorCompanyId, 0));
                    attestStateMinPayroll = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, 0, actorCompanyId, 0));
                    attestStateResultingInvoice = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportInvoiceResultingAttestStatus, 0, actorCompanyId, 0));
                    attestStateResultingPayroll = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, 0, actorCompanyId, 0));

                    if (attestStateResultingInvoice != null && attestStateMinInvoice == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8158, "Lägsta status för export av fakturatransaktioner kunde inte hittas"));
                    if (attestStateResultingPayroll != null && attestStateMinPayroll == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8159, "Lägsta status för export av löneartstransaktioner kunde inte hittas"));
                    if (attestStateMinInvoice != null && attestStateResultingInvoice == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8156, "Status efter export för fakturatransaktioner kunde inte hittas"));
                    if (attestStateMinPayroll != null && attestStateResultingPayroll == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8157, "Status efter export för löneartstransaktioner kunde inte hittas"));

                    attestPayrollTransition = AttestManager.GetUserAttestTransitionForState(entities, TermGroup_AttestEntity.PayrollTime, attestStateMinPayroll.AttestStateId, attestStateResultingPayroll.AttestStateId, actorCompanyId, userId);
                    if (attestPayrollTransition == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8160, "Giltig attestövergång för löneartstransaktioner kunde inte hittas"));
                }

                //AccountDim
                List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId);

                //TimeCode
                List<TimeCode> timeCodes = TimeCodeManager.GetTimeCodes(entities, actorCompanyId, SoeTimeCodeType.None).ToList();

                //TemplateSchedules
                List<TimeSchedulePlanningDayDTO> templateTimeScheduleTemplateBlocks = new List<TimeSchedulePlanningDayDTO>();
                bool getTemplateSchedules = false;
                if ((SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.AditroL1)
                {
                    var employeeGroups = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId);

                    if (employeeGroups.Any(a => (TermGroup_QualifyingDayCalculationRule)a.QualifyingDayCalculationRule == TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeekPlusAdditionalContract ||
                                                (TermGroup_QualifyingDayCalculationRule)a.QualifyingDayCalculationRule == TermGroup_QualifyingDayCalculationRule.UseAverageCalculationInTimePeriod))
                    {
                        selectionStartDate = CalendarUtility.GetBeginningOfWeek(startDate);
                        selectionStopDate = CalendarUtility.GetEndOfWeek(stopDate);
                        getTemplateSchedules = true;
                    }
                }

                //EmployeeSchedule
                List<EmployeeSchedule> employeeSchedules = GetEmployeeSchedule(entities, employeeIds, selectionStartDate, selectionStopDate, actorCompanyId);


                //ScheduleTypes (IsNotScheduleTime)
                var scheduleTypes = TimeScheduleManager.GetTimeScheduleTypesIgnoreState(entities, actorCompanyId, true);
                List<int> scheduleTypeIsNotScheduleTimeIds = scheduleTypes.Where(x => x.IsNotScheduleTime).Select(x => x.TimeScheduleTypeId).ToList();
                scheduleTypeIsNotScheduleTimeIds.AddRange(scheduleTypes.Where(x => x.UseScheduleTimeFactor && x.TimeScheduleTypeFactor != null && x.TimeScheduleTypeFactor.Any(a => a.State == (int)SoeEntityState.Active) && x.TimeScheduleTypeFactor.Where(a => a.State == (int)SoeEntityState.Active).All(a => a.Factor == 0)).Select(x => x.TimeScheduleTypeId).ToList());

                //Format
                TermGroup_SalaryExportUseSocSecFormat salaryExportUseSocSecFormat = (TermGroup_SalaryExportUseSocSecFormat)SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportUseSocSecFormat, 0, actorCompanyId, 0);

                //Accounts
                List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompanyIgnoreState(entities, actorCompanyId);

                #endregion

                BlueGardenCompany blueGardenCompany = BlueGardenCompany.ICA;

                if ((SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.BlueGarden)
                {
                    if (entities.Company.Any(f => f.ActorCompanyId == actorCompanyId && f.LicenseId == 489 && f.License.LicenseNr == "564"))
                        blueGardenCompany = BlueGardenCompany.Tele2;
                    else if (entities.Company.Include("License").FirstOrDefault(f => f.ActorCompanyId == actorCompanyId)?.License?.LicenseNr?.StartsWith("400") ?? false)
                        blueGardenCompany = BlueGardenCompany.Coop;
                }
                List<TimeAbsenceDetailDTO> timeAbsenceDetails = new List<TimeAbsenceDetailDTO>();
                if (blueGardenCompany == BlueGardenCompany.ICA && (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.BlueGarden)
                {
                    timeAbsenceDetails = TimeBlockManager.GetTimeAbsenceDetails(entities, employeeIds, startDate, stopDate);
                    var employeeidsWithAbsenceDetails = timeAbsenceDetails.Where(w => w.Ratio != 0).Select(s => s.EmployeeId).Distinct().ToList();
                    timeStampEntries = entities.TimeStampEntry.Where(w => employeeidsWithAbsenceDetails.Contains(w.EmployeeId) && w.State == (int)SoeEntityState.Active && w.TimeBlockDate.Date >= startDate && w.TimeBlockDate.Date <= stopDate).ToList().ToDTOs().ToList();
                }

                #region Fetch data

                #region TimeScheduleTemplateBlock

                List<TimeScheduleTemplateBlock> allEmployeeScheduleTemplateBlocks = GetEmployeeScheduleTemplateBlocks(entities, employeeIds, selectionStartDate.Date, selectionStopDate.Date, actorCompanyId, (blueGardenCompany == BlueGardenCompany.Coop && (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.BlueGarden));
                foreach (var groupByEmployeeId in allEmployeeScheduleTemplateBlocks.Where(w => w.IsSchedule()).GroupBy(g => g.EmployeeId))
                {
                    var onEmployee = groupByEmployeeId.ToList();
                    var overlappingBreaksOnNotScheduleTime = new List<TimeScheduleTemplateBlock>();

                    foreach (var item in onEmployee)
                    {
                        if (!item.IsBreak && !item.IsZero && item.TimeScheduleTypeId.HasValue && scheduleTypeIsNotScheduleTimeIds.Contains(item.TimeScheduleTypeId.Value))
                        {
                            if (((SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.AditroL1 || (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.HuldtOgLillevik || (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.AgdaLon) && item.Date.HasValue && item.EmployeeId.HasValue)
                                item.IsNotScheduleTimeShift = true;
                            else if (!(item.TimeDeviationCauseId.HasValue && blueGardenCompany == BlueGardenCompany.Coop && (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.BlueGarden))
                            {
                                //Find overlapping breaks
                                overlappingBreaksOnNotScheduleTime.AddRange(onEmployee.Where(x => x.IsBreak && item.Date == x.Date && CalendarUtility.GetOverlappingMinutes(item.StartTime, item.StopTime, x.StartTime, x.StopTime) != 0));
                                continue;
                            }
                        }

                        if (((SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.AditroL1 || (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.HuldtOgLillevik || (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.AgdaLon) && item.IsBreak && item.Date.HasValue && item.ActualStartTime.HasValue && item.ActualStopTime.HasValue && item.EmployeeId.HasValue)
                        {
                            var notScheduleTimeShiftsOnDate = onEmployee.Where(x => !x.IsBreak && x.EmployeeId.HasValue && x.Date.HasValue && x.ActualStartTime.HasValue && x.ActualStopTime.HasValue && x.EmployeeId.Value == item.EmployeeId.Value && x.Date.Value == item.Date.Value && x.TimeScheduleTypeId.HasValue && scheduleTypeIsNotScheduleTimeIds.Contains(x.TimeScheduleTypeId.Value));
                            foreach (var shift in notScheduleTimeShiftsOnDate)
                            {
                                if (CalendarUtility.IsNewOverlappedByCurrent(item.ActualStartTime.Value, item.ActualStopTime.Value, shift.ActualStartTime.Value, shift.ActualStopTime.Value))
                                    item.BreakIsOverlappedByNotScheduleTimeShift = true;
                            }
                        }

                        employeeScheduleTemplateBlocks.Add(item);
                    }

                    if (blueGardenCompany == BlueGardenCompany.Coop && (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.BlueGarden && overlappingBreaksOnNotScheduleTime.Any())
                        employeeScheduleTemplateBlocks = employeeScheduleTemplateBlocks.Except(overlappingBreaksOnNotScheduleTime).ToList();
                }

                #endregion

                LogInfo("TimeSalaryExport: Done get scheduleblocks");

                List<Employee> companyEmployees = EmployeeManager.GetAllEmployees(entities, actorCompanyId, active: null, loadEmployment: true);
                List<PayrollProduct> payrollProducts = ProductManager.GetPayrollProductsIgnoreState(entities, actorCompanyId, includeSettings: includeSettings);

                LogInfo("TimeSalaryExport: Fetching transactions");

                #region TimePayrollTransactions

                List<TimePayrollTransaction> payrollTransactionsToExport = new List<TimePayrollTransaction>();
                List<Tuple<int, DateTime>> removeScheduleDates = new List<Tuple<int, DateTime>>();

                List<TimePayrollTransaction> allPayrollTransactions = GetPayrollTransactions(entities, employeeIds, selectionStartDate, selectionStopDate, actorCompanyId, attestStateMinPayroll, isPreliminary);
                foreach (var item in allPayrollTransactions)
                {
                    var payrollProduct = payrollProducts.FirstOrDefault(x => x.ProductId == item.ProductId);
                    if (payrollProduct != null && !payrollProduct.IsTaxAndNotOptional() && !payrollProduct.IsEmploymentTax() && !payrollProduct.IsSupplementCharge() && !payrollProduct.IsNetSalary())
                    {
                        if (payrollProduct.Export)
                            payrollTransactionsToExport.Add(item);

                        if (payrollProduct.IsAbsenceVacationNoVacationDaysDeducted() && !payrollProduct.Export)
                            removeScheduleDates.Add(Tuple.Create(item.EmployeeId, item.TimeBlockDate.Date));
                    }
                }


                if (!removeScheduleDates.IsNullOrEmpty())
                {
                    var tempEmployeeScheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();

                    foreach (var item in employeeScheduleTemplateBlocks)
                    {
                        if (!removeScheduleDates.Any(w => w.Item1 == item.EmployeeId && w.Item2 == item.Date))
                            tempEmployeeScheduleTemplateBlocks.Add(item);
                    }

                    employeeScheduleTemplateBlocks = tempEmployeeScheduleTemplateBlocks;
                }

                if (!isPreliminary && !allPayrollTransactions.Any())
                    return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, "No transactions found");

                List<int> timePayrollTransactionIds = new List<int>();
                timePayrollTransactionIds.AddRange(payrollTransactionsToExport.Select(i => i.TimePayrollTransactionId).Distinct().ToList());

                if (getTemplateSchedules)
                {
                    var withAbsenceEmployeeIds = allPayrollTransactions.Where(w => w.IsAbsenceSick()).Select(s => s.EmployeeId).Distinct().ToList();
                    templateTimeScheduleTemplateBlocks = TimeScheduleManager.GetTimeSchedulePlanningDaysFromTemplate(entities, actorCompanyId, base.RoleId, base.UserId, selectionStartDate, selectionStopDate, null, withAbsenceEmployeeIds);
                }

                #endregion

                #region TimeInvoiceTransactions
                List<TimeInvoiceTransaction> invoiceTransactions = new List<TimeInvoiceTransaction>();

                //DONT INCLUDE INVOICETRANSACTIONS FOR NOW; ACCOURDING TO RICHARD
                //if (attestStateMinInvoice != null)
                //    invoiceTransactions = GetInvoiceTransactions(entities, employeeIds, startDate, stopDate, actorCompanyId, attestStateMinInvoice, module);

                //if (invoiceTransactions.Count > 0)
                //{
                //    attestInvoiceTransition = AttestManager.GetUserAttestTransitionForState(entities, TermGroup_AttestEntity.InvoiceTime, attestStateMinInvoice.AttestStateId, attestStateResultingInvoice.AttestStateId, actorCompanyId, userId);
                //    if (attestInvoiceTransition == null)
                //        return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8161, "Giltig attestövergång för fakturatransaktioner kunde inte hittas"));
                //}

                //List<int> timeInvoiceTransactionIds = new List<int>();
                //timeInvoiceTransactionIds.AddRange(invoiceTransactions.Select(i => i.TimeInvoiceTransactionId).Distinct().ToList());

                #endregion

                LogInfo("TimeSalaryExport: Done fetching transactions");

                #region TimePayrollScheduleTransactions

                List<TimePayrollScheduleTransaction> scheduleTransactions = new List<TimePayrollScheduleTransaction>();
                if (isPreliminary)
                {
                    LogInfo("TimeSalaryExport: Calculate scheduletransactions");

                    Dictionary<int, List<DateTime>> employeesAndDates = new Dictionary<int, List<DateTime>>();
                    Dictionary<int, List<TimePayrollTransaction>> timePayrollTransactionsDict = payrollTransactionsToExport.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());
                    foreach (var scheduleGrouped in employeeScheduleTemplateBlocks.Where(x => !x.IsZero && x.EmployeeId.HasValue && x.Date.HasValue && x.Date.Value >= DateTime.Today).GroupBy(x => x.EmployeeId.Value))
                    {
                        var employeeDates = scheduleGrouped.Where(x => x.Date.HasValue).Select(x => x.Date.Value).Distinct();
                        var employeeTransactions = timePayrollTransactionsDict.ContainsKey(scheduleGrouped.Key) ? timePayrollTransactionsDict.FirstOrDefault(f => f.Key == scheduleGrouped.Key).Value : new List<TimePayrollTransaction>();
                        List<DateTime> validDates = new List<DateTime>();
                        foreach (var date in employeeDates)
                        {
                            if (employeeTransactions.Any(x => x.TimeBlockDate != null && x.TimeBlockDate.Date == date))
                                continue;

                            validDates.Add(date);
                        }

                        employeesAndDates.Add(scheduleGrouped.Key, validDates);
                    }
                    LogInfo("TimeSalaryExport: Done Calculate scheduletransactions");

                    LogInfo("TimeSalaryExport: Generate scheduletransactions");

                    ActionResult result = TimeEngineManager(ActorCompanyId, UserId).SavePayrollScheduleTransactions(employeesAndDates);
                    if (!result.Success)
                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetText(8943, "Transaktioner för prognos kunde inte skapas."));

                    LogInfo("TimeSalaryExport: Done Generate scheduletransactions");

                    if (result.Success)
                    {
                        LogInfo("TimeSalaryExport: Get scheduletransactions");

                        scheduleTransactions = GetScheduleTransactions(entities, actorCompanyId, employeesAndDates, payrollProducts.Where(x => !x.Export).Select(x => x.ProductId).ToList());

                        LogInfo("TimeSalaryExport: Done Get scheduletransactions");
                    }
                }

                #endregion

                #region TimeBlockDate (lock period)

                List<TimeBlockDate> timeBlockDates = null;
                if (lockPeriod && !isPreliminary)
                {
                    LogInfo("TimeSalaryExport: Fetching timeblockdates");

                    timeBlockDates = TimeBlockManager.GetTimeBlockDates(entities, actorCompanyId, employeeIds, startDate, stopDate);

                    LogInfo("TimeSalaryExport: Done fetching timeblockdates");
                }

                #endregion

                #region HibernatingAbsence

                LogInfo("TimeSalaryExport: Get hibernatingabsence");
                List<TimeHibernatingAbsenceRow> hibernatingAbsenceRows = new List<TimeHibernatingAbsenceRow>();
                List<TimeDeviationCause> timeDeviationCauses = new List<TimeDeviationCause>();
                if ((SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.BlueGarden && blueGardenCompany == BlueGardenCompany.ICA)
                {
                    timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, actorCompanyId, loadTimeCode: true, loadPayrollProduct: true);
                    hibernatingAbsenceRows = TimeHibernatingManager.GetHibernatingAbsenceRows(entities, actorCompanyId, startDate, stopDate, employeeIds);
                }
                LogInfo("TimeSalaryExport: Done Get hibernatingabsence");

                #endregion

                #region TimeDevationCauses

                if (((SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.AgdaLon && externalExportId == "C2") || ((SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.Pol ||  ((SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.Lessor)))
                {
                    timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, actorCompanyId);
                }

                #endregion

                #endregion

                #region Create XML

                //Extract all unique employees 
                var employeeIdsToExport = employeeSchedules.Select(i => i.EmployeeId).ToList();
                employeeIdsToExport.AddRange(payrollTransactionsToExport.Select(i => i.EmployeeId).ToList());
                employeeIdsToExport.AddRange(invoiceTransactions.Where(x => x.EmployeeId.HasValue).Select(i => i.EmployeeId.Value).ToList());
                employeeIdsToExport = employeeIdsToExport.Distinct().ToList();
                var employees = companyEmployees.Where(x => employeeIdsToExport.Contains(x.EmployeeId)).Where(w => !w.Hidden && !w.Vacant).ToList();

                //Sort
                employees = (from x in employees
                             orderby x.EmployeeNr ascending
                             select x).ToList();

                payrollTransactionsToExport = (from t in payrollTransactionsToExport
                                               orderby (t.TimeBlockDate != null ? t.TimeBlockDate.Date : DateTime.Now)
                                               select t).ToList();

                invoiceTransactions = (from t in invoiceTransactions
                                       orderby (t.TimeBlockDate != null ? t.TimeBlockDate.Date : DateTime.Now)
                                       select t).ToList();

                if (blueGardenCompany == BlueGardenCompany.Coop && (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.BlueGarden)
                {
                    var filteredPayrollTransactions = new List<TimePayrollTransaction>();
                    var timeTransactionsOnEmployeeDict = payrollTransactionsToExport.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());

                    foreach (var employee in employees)
                    {
                        var lastEmployment = employee.GetLastEmployment();
                        var endDate = lastEmployment?.GetEndDate();

                        if (endDate != null && endDate < stopDate && timeTransactionsOnEmployeeDict.TryGetValue(employee.EmployeeId, out var employeeTransactions))
                        {
                            if (employeeTransactions.Any())
                            {
                                var filteredTransactions = employeeTransactions.Where(w => w.TimeBlockDate.Date <= endDate).ToList();
                                filteredPayrollTransactions.AddRange(filteredTransactions);
                            }
                        }
                        else
                        {
                            if (timeTransactionsOnEmployeeDict.TryGetValue(employee.EmployeeId, out var employeeTransactions2))
                            {
                                filteredPayrollTransactions.AddRange(employeeTransactions2);
                            }
                        }
                    }

                    payrollTransactionsToExport = filteredPayrollTransactions;
                }


                //Create base xml
                var baseAdapter = new SoeXeAdapter();
                XDocument baseXml = baseAdapter.CreateXml(entities, (SoeTimeSalaryExportTarget)exportTarget, accountDims, employees, employeeSchedules, employeeScheduleTemplateBlocks, payrollTransactionsToExport, invoiceTransactions, scheduleTransactions, timeCodes, payrollProducts, accountStds, actorCompanyId, startDate, stopDate, showSocialSec, salaryExportUseSocSecFormat);
                LogInfo("TimeSalaryExport: Done Creating base XML");

                //Get salary adapter
                ISalaryAdapter transformationAdapter;
                bool usesSeparateFiles = false;
                salaryExportResult.Format = SoeTimeSalaryExportFormat.XML;
                salaryExportResult.Extension = "xml";

                //Comment
                bool doNotIncludeComments = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportNoComments, 0, actorCompanyId, 0);





                switch ((SoeTimeSalaryExportTarget)exportTarget)
                {
                    case SoeTimeSalaryExportTarget.KontekLon:
                        transformationAdapter = new KontekAdapter();
                        usesSeparateFiles = true;
                        break;
                    case SoeTimeSalaryExportTarget.SoftOne:
                        transformationAdapter = new SoftOneAdapter(entities, actorCompanyId);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "dat";
                        break;
                    case SoeTimeSalaryExportTarget.SvenskLon:
                        transformationAdapter = new SvenskLonAdapter(entities, externalExportId, startDate, stopDate, baseAdapter.ScheduleItemsOutput);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "dat";
                        break;
                    case SoeTimeSalaryExportTarget.Sauma:
                        transformationAdapter = new SaumaAdapter(entities);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "txt";
                        break;
                    case SoeTimeSalaryExportTarget.Hogia214006:
                    case SoeTimeSalaryExportTarget.Hogia214007:
                        transformationAdapter = new Hogia214007Adapter(baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, (SoeTimeSalaryExportTarget)exportTarget == SoeTimeSalaryExportTarget.Hogia214006);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "WLI";
                        usesSeparateFiles = true;
                        break;
                    case SoeTimeSalaryExportTarget.Flex:
                        transformationAdapter = new FlexLonAdapter(baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, startDate, stopDate);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "dta";
                        usesSeparateFiles = true;
                        break;
                    case SoeTimeSalaryExportTarget.Hogia214002:
                        transformationAdapter = new Hogia214002Adapter(baseAdapter.PayrollTransactionItemsOutput, doNotIncludeComments);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "WLI";
                        usesSeparateFiles = false;
                        break;
                    case SoeTimeSalaryExportTarget.Personec:
                        transformationAdapter = new PersonecAdapter(entities, externalExportId, baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, employeeIds);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "dat";
                        break;
                    case SoeTimeSalaryExportTarget.AgdaLon:
                        transformationAdapter = new AgdaLonAdapter(baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, externalExportId, doNotIncludeComments, timeDeviationCauses);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "dat";
                        usesSeparateFiles = true;
                        break;
                    case SoeTimeSalaryExportTarget.Spcs:
                        transformationAdapter = new Visma600Adapter(company, payrollProducts.Where(x => x.State == (int)SoeEntityState.Active).ToList(), baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, employeeIds, startDate, stopDate);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "tlu";
                        break;
                    case SoeTimeSalaryExportTarget.PAxml:
                        transformationAdapter = new PAxmaladapter(company, payrollProducts.Where(x => x.State == (int)SoeEntityState.Active).ToList(), baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, employeeIds, startDate, stopDate, externalExportId);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "xml";
                        break;
                    case SoeTimeSalaryExportTarget.PAxml2_1:
                        transformationAdapter = new PAxmaladapter2_1(payrollProducts.Where(x => x.State == (int)SoeEntityState.Active).ToList(), baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, employeeIds, startDate, stopDate, externalExportId);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "xml";
                        break;
                    case SoeTimeSalaryExportTarget.DiLonn:
                        transformationAdapter = new DiLonnAdapter(baseAdapter.PayrollTransactionItemsOutput);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "txt";
                        break;
                    case SoeTimeSalaryExportTarget.Tikon:
                        transformationAdapter = new TikonAdapter(entities, externalExportId, baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, employeeIds);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "txt";
                        break;
                    case SoeTimeSalaryExportTarget.TikonCSV:
                        transformationAdapter = new TikonCSVAdapter(entities, ProductManager.GetPayrollProducts(actorCompanyId, true, true, true, true, true).ToDTOs(true, true, true, true, true).ToList());
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "csv";
                        break;
                    case SoeTimeSalaryExportTarget.DLPrime3000:
                        transformationAdapter = new DLPrime3000Adapter(baseAdapter.PayrollTransactionItemsOutput);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "TXT";
                        break;
                    case SoeTimeSalaryExportTarget.BlueGarden:
                        List<CompanyExternalCode> externalCodes = blueGardenCompany != BlueGardenCompany.Coop ? new List<CompanyExternalCode>() : ActorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.AccountHierachyPayrollExport, actorCompanyId);
                        List<CompanyExternalCode> externalCodeUnits = blueGardenCompany != BlueGardenCompany.Coop ? new List<CompanyExternalCode>() : ActorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.AccountHierachyPayrollExportUnit, actorCompanyId);

                        var dimsWithAccount = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true, loadAccounts: true, loadInternalAccounts: true);

                        foreach (var externalCode in externalCodes)
                        {
                            foreach (var dim in dimsWithAccount)
                            {
                                foreach (var acc in dim.Account.Where(w => w.AccountId == externalCode.RecordId))
                                {
                                    acc.AccountHierachyPayrollExportExternalCode = externalCode.ExternalCode;
                                }
                            }
                        }

                        foreach (var externalCode in externalCodeUnits)
                        {
                            foreach (var dim in dimsWithAccount)
                            {
                                foreach (var acc in dim.Account.Where(w => w.AccountId == externalCode.RecordId))
                                {
                                    acc.AccountHierachyPayrollExportUnitExternalCode = externalCode.ExternalCode;
                                }
                            }
                        }

                        string externalExportIdSetting = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportExternalExportID, 0, actorCompanyId, 0);

                        // if external export id field has a '#' first, for being able to send to payroll (#NNNN) then externalExportId is empty string, check the second part
                        if (externalExportIdSetting.Contains("#") && string.IsNullOrEmpty(externalExportId) && blueGardenCompany == BlueGardenCompany.ICA)
                            externalExportId = TimeSalaryManager.GetSecondExternalExportId(entities, actorCompanyId);

                        transformationAdapter = new BlueGardenAdapter(entities, externalExportId, baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, employees, startDate.Date, dimsWithAccount, blueGardenCompany, hibernatingAbsenceRows, timeDeviationCauses, timeAbsenceDetails, timeStampEntries, externalExportId2);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "txt";
                        break;
                    case SoeTimeSalaryExportTarget.Orkla:
                        transformationAdapter = new OrklaAdapter(entities, externalExportId, baseAdapter.PayrollTransactionItemsOutput, employees);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "txt";
                        break;
                    case SoeTimeSalaryExportTarget.Fivaldi:
                        transformationAdapter = new FivaldiAdapter(entities);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        //To Do check if csv
                        salaryExportResult.Extension = "csv";
                        break;
                    case SoeTimeSalaryExportTarget.AditroL1:
                        var employeeGroups = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId);
                        transformationAdapter = new AdritoL1Adapter(baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, externalExportId, doNotIncludeComments, company.Name, employees, employeeGroups, startDate, stopDate, entities.EmployeeAccount.Include("Account.AccountInternal").Where(w => w.ActorCompanyId == actorCompanyId && !w.ParentEmployeeAccountId.HasValue && w.Account.AccountDim.SysSieDimNr == (int?)TermGroup_SieAccountDim.CostCentre).ToList(), templateTimeScheduleTemplateBlocks, SettingManager.SiteType == TermGroup_SysPageStatusSiteType.Test);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Zip;
                        salaryExportResult.Extension = "zip";
                        break;
                    case SoeTimeSalaryExportTarget.HuldtOgLillevik:
                        var employeeGroupsForHL = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId);
                        transformationAdapter = new HLAdapter(baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, externalExportId, doNotIncludeComments, company.Name, employees, employeeGroupsForHL, startDate, stopDate);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Zip;
                        salaryExportResult.Extension = "zip";
                        break;
                    case SoeTimeSalaryExportTarget.Netvisor:
                        transformationAdapter = new NetvisorAdapter(entities, externalExportId, baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, employeeIds, stopDate.Date);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Text;
                        salaryExportResult.Extension = "txt";
                        break;
                    case SoeTimeSalaryExportTarget.Pol:
                        List<EmployeeChildDTO> children = entities.EmployeeChild.Where(x => employeeIds.Contains(x.EmployeeId)).ToDTOs().ToList();
                        transformationAdapter = new PolAdapter(startDate, stopDate, externalExportId, company.Name, baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, employees, children, timeDeviationCauses);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Zip;
                        salaryExportResult.Extension = "zip";
                        break;
                    case SoeTimeSalaryExportTarget.SDWorx:
                        var employeeGroupsSDWorx = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId);
                        transformationAdapter = new SDWorxAdapter(baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, externalExportId, doNotIncludeComments, company.Name, employees, employeeGroupsSDWorx, startDate, stopDate, entities.EmployeeAccount.Include("Account.AccountInternal").Where(w => w.ActorCompanyId == actorCompanyId && !w.ParentEmployeeAccountId.HasValue && w.Account.AccountDim.SysSieDimNr == (int?)TermGroup_SieAccountDim.CostCentre).ToList(), templateTimeScheduleTemplateBlocks);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Zip;
                        salaryExportResult.Extension = "zip";
                        break;
                    case SoeTimeSalaryExportTarget.Lessor:
                        transformationAdapter = new LessorAdapter(baseAdapter.PayrollTransactionItemsOutput, baseAdapter.ScheduleItemsOutput, externalExportId, startDate.Date, employees, payrollProducts.Where(x => x.State == (int)SoeEntityState.Active).ToList(), timeDeviationCauses);
                        salaryExportResult.Format = SoeTimeSalaryExportFormat.Zip;
                        salaryExportResult.Extension = "zip";
                        break;
                    default:
                        transformationAdapter = new SoeXeAdapter();
                        break;
                }

                salaryExportResult.UsesSeparateFiles = usesSeparateFiles;

                //Transform Xml
                if (usesSeparateFiles)
                {
                    var extendedAdapter = transformationAdapter as ISalarySplittedFormatAdapter;
                    if (extendedAdapter != null)
                    {
                        salaryExportResult.Salary = extendedAdapter.TransformSalary(baseXml);
                        salaryExportResult.Schedule = extendedAdapter.TransformSchedule(baseXml);
                    }
                }
                else
                {
                    salaryExportResult.SalaryAndSchedule = transformationAdapter.TransformSalary(baseXml);
                }

                LogInfo("TimeSalaryExport: Done creating file");

                #endregion

                using (TransactionScope transaction = CreateTransactionScope(new TimeSpan(2, 0, 0)))
                {
                    //Open connection
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    #region Save Export entries

                    //Save information about the export
                    var timeSalary = new TimeSalaryExport
                    {
                        Company = company,
                        ExportDate = DateTime.Now,
                        StartInterval = startDate,
                        StopInterval = stopDate,
                        ExportFormat = (int)salaryExportResult.Format,
                        ExportTarget = exportTarget,
                        File1 = salaryExportResult.UsesSeparateFiles ? salaryExportResult.Salary : salaryExportResult.SalaryAndSchedule,
                        Extension = salaryExportResult.Extension,
                        File2 = new byte[0],
                        IsPreliminary = isPreliminary,
                    };

                    if (salaryExportResult.UsesSeparateFiles)
                        timeSalary.File2 = salaryExportResult.Schedule;

                    SetCreatedProperties(timeSalary);

                    #endregion

                    #region Update Transactions

                    if (isPreliminary)
                    {

                        entities.TimeSalaryExport.AddObject(timeSalary);
                        salaryExportResult.Result = SaveChanges(entities, transaction);
                        if (!salaryExportResult.Result.Success)
                            return salaryExportResult.Result;

                        LogInfo("TimeSalaryExport: Done creating export as preliminary " + actorCompanyId);
                    }
                    else
                    {
                        //Update transaction statuses to exported
                        salaryExportResult.Result = ChangeTransactionStatusToExportForNotPreliminaryExport(entities, timeSalary, employeeIds, allPayrollTransactions, invoiceTransactions, timeBlockDates, attestStateResultingInvoice, attestInvoiceTransition, attestStateResultingPayroll, attestPayrollTransition, actorCompanyId, userId, transaction, useBulk, lockPeriod);
                        if (!salaryExportResult.Result.Success)
                            return salaryExportResult.Result;

                        LogInfo("TimeSalaryExport: Done creating exportrows and updating transactions " + actorCompanyId);
                    }

                    #endregion

                    //Commit transaction
                    if (salaryExportResult.Result.Success)
                    {
                        salaryExportResult.Result.IntegerValue2 = timeSalary.TimeSalaryExportId;
                        transaction.Complete();
                    }

                }
            }
            catch (Exception ex)
            {
                salaryExportResult.Result.Exception = ex;
                salaryExportResult.Result.Success = false;
                LogError(ex, log);
                LogInfo("TimeSalaryExport: Failed in transactionscope " + actorCompanyId);
            }
            finally
            {
                if (salaryExportResult.Result.Success)
                {
                    //Set success properties
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                entities.Connection.Close();

                base.CompleteTask(taskWatchLogId);
            }

            return salaryExportResult.Result;
        }


        public ActionResult UndoExportInBatch(int timeSalaryExportId, int actorCompanyId, int userId)
        {
            ActionResult result = new ActionResult();

            var taskWatchLogId = StartTask(
                "UndoExportInBatch", 
                MethodBase.GetCurrentMethod().DeclaringType.ToString(), 
                Guid.NewGuid().ToString(),
                $"timeSalaryExportId {timeSalaryExportId} userId {userId}"
                );
            bool isPreliminary = false;

            using (var entities = new CompEntities())
            {
                TimeSalaryExport timeSalaryExport = GetTimeSalaryExport(entities, timeSalaryExportId, actorCompanyId, false);
                if (timeSalaryExport == null)
                    return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8163, "Exporterad lön kunde inte hittas"));

                isPreliminary = timeSalaryExport.IsPreliminary;

                if (isPreliminary)
                {
                    #region Undo preliminary export

                    result = ChangeEntityState(entities, timeSalaryExport, SoeEntityState.Deleted, true);

                    #endregion

                }
                else
                {
                    #region Undo regular/not preliminary export                    

                    List<AttestTransitionLog> attestTransitionLogInserts = new List<AttestTransitionLog>();
                    List<TimeSalaryExportRow> timeSalaryExportRowInserts = new List<TimeSalaryExportRow>();
                    List<TimePayrollTransaction> timePayrollTransactionUpdates = new List<TimePayrollTransaction>();
                    List<TimeInvoiceTransaction> timeInvoiceTransactionUpdates = new List<TimeInvoiceTransaction>();
                    List<TimeBlockDate> timeBlockDateUpdates = new List<TimeBlockDate>();

                    AttestState attestStateMinInvoice = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportInvoiceMinimumAttestStatus, 0, actorCompanyId, 0));
                    AttestState attestStateMinPayroll = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, 0, actorCompanyId, 0));
                    AttestState attestStateResultingInvoice = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportInvoiceResultingAttestStatus, 0, actorCompanyId, 0));
                    AttestState attestStateResultingPayroll = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, 0, actorCompanyId, 0));

                    List<Tuple<TermGroup_AttestEntity, int, int?>> exportRows = GetTimeSalaryExportRowIds(entities, timeSalaryExportId, actorCompanyId);

                    if (exportRows.Any(r => r.Item1 == TermGroup_AttestEntity.InvoiceTime))
                    {
                        if (attestStateMinInvoice == null)
                            return new ActionResult(false, (int)ActionResultSelect.EntityNotFound, GetText(8164, "Lägsta status för fakturatransaktioner kunde inte hittas"));

                        if (attestStateResultingInvoice == null)
                            return new ActionResult(false, (int)ActionResultSelect.EntityNotFound, GetText(8169, "Status för exporterade fakturatransaktioner kunde inte hittas"));

                        AttestTransition attestInvoiceTransition = AttestManager.GetUserAttestTransitionForState(entities, TermGroup_AttestEntity.InvoiceTime, attestStateResultingInvoice.AttestStateId, attestStateMinInvoice.AttestStateId, actorCompanyId, userId);
                        if (attestInvoiceTransition == null)
                            return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8165, "Giltig attestövergång för fakturatransaktioner kunde inte hittas"));
                    }

                    AttestTransition attestPayrollTransition = null;

                    if (exportRows.Any(r => r.Item1 == TermGroup_AttestEntity.PayrollTime))
                    {
                        if (attestStateMinPayroll == null)
                            return new ActionResult(false, (int)ActionResultSelect.EntityNotFound, GetText(8166, "Lägsta status för löneartstransaktioner kunde inte hittas"));

                        if (attestStateResultingPayroll == null)
                            return new ActionResult(false, (int)ActionResultSelect.EntityNotFound, GetText(8168, "Status för exporterade löneartstransaktioner kunde inte hittas"));

                        attestPayrollTransition = AttestManager.GetUserAttestTransitionForState(entities, TermGroup_AttestEntity.PayrollTime, attestStateResultingPayroll.AttestStateId, attestStateMinPayroll.AttestStateId, actorCompanyId, userId);
                        if (attestPayrollTransition == null)
                            return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8167, "Giltig attestövergång för löneartstransaktioner kunde inte hittas"));
                    }

                    //Update state
                    ChangeEntityState(timeSalaryExport, SoeEntityState.Deleted);
                    List<int> payrollTransactionIds = exportRows.Where(x => x.Item1 == TermGroup_AttestEntity.PayrollTime).Select(x => x.Item2).ToList();
                    List<int> invoiceTransactionIds = exportRows.Where(x => x.Item1 == TermGroup_AttestEntity.InvoiceTime).Select(x => x.Item2).ToList();
                    List<int> employeeIds = exportRows.Where(x => x.Item3.HasValue).Select(x => x.Item3.Value).Distinct().ToList();

                    entities.CommandTimeout = 180; // 3 minutes
                    List<TimePayrollTransaction> timePayrollTransactions = TimeTransactionManager.GetTimePayrollTransactionsForCompany(entities, actorCompanyId, timeSalaryExport.StartInterval, timeSalaryExport.StopInterval);
                    List<TimeInvoiceTransaction> timeInvoiceTransactions = TimeTransactionManager.GetTimeInvoiceTransactions(entities, actorCompanyId, timeSalaryExport.StartInterval, timeSalaryExport.StopInterval);
                    List<TimeBlockDate> lockedTimeBlockDates = TimeBlockManager.GetTimeBlockDates(entities, actorCompanyId, employeeIds, timeSalaryExport.StartInterval, timeSalaryExport.StopInterval)
                                                                                            .Where(x => x.Status == (int)SoeTimeBlockDateStatus.Locked).ToList();

                    Dictionary<int, List<TimeBlockDate>> timeBlockDatesGrouped = lockedTimeBlockDates?.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList()) ?? new Dictionary<int, List<TimeBlockDate>>();
                    foreach (var item in timeBlockDatesGrouped)
                    {
                        (List<TimeBlockDate> timeBlockDateInsertsForEmployee, List<TimeBlockDate> timeBlockDateUpdatesForEmployee) = SetTimeBlockDateStatus(entities, item.Key, actorCompanyId, item.Value, timeSalaryExport.StartInterval, timeSalaryExport.StopInterval, SoeTimeBlockDateStatus.None, false, true);
                        timeBlockDateUpdates.AddRange(timeBlockDateUpdatesForEmployee);
                    }

                    foreach (var transactionId in payrollTransactionIds)
                    {
                        TimePayrollTransaction timePayrollTransaction = timePayrollTransactions.FirstOrDefault(x => x.TimePayrollTransactionId == transactionId);
                        if (timePayrollTransaction == null)
                            timePayrollTransaction = TimeTransactionManager.GetTimePayrollTransactionDiscardState(entities, transactionId);
                        if (timePayrollTransaction == null)
                            continue;

                        timePayrollTransaction.Exported = false;
                        timePayrollTransaction.AttestStateId = attestStateMinPayroll.AttestStateId;
                        timePayrollTransactionUpdates.Add(timePayrollTransaction);

                        if (timePayrollTransaction.AttestStateId > 0 && attestStateResultingPayroll != null && attestPayrollTransition != null)
                        {
                            //Add AttestTransitionLog
                            AttestTransitionLog attestTransitionLog = new AttestTransitionLog
                            {
                                Date = DateTime.Now,
                                Entity = (int)TermGroup_AttestEntity.PayrollTime,
                                RecordId = timePayrollTransaction.TimePayrollTransactionId,
                                UserId = userId,

                                //Set FK
                                ActorCompanyId = actorCompanyId,
                                AttestTransitionId = attestPayrollTransition.AttestTransitionId,
                            };
                            attestTransitionLogInserts.Add(attestTransitionLog);
                        }
                    }

                    foreach (var transactionId in invoiceTransactionIds)
                    {
                        TimeInvoiceTransaction timeInvoiceTransaction = timeInvoiceTransactions.FirstOrDefault(x => x.TimeInvoiceTransactionId == transactionId);
                        if (timeInvoiceTransaction == null)
                            timeInvoiceTransaction = TimeTransactionManager.GetTimeInvoiceTransaction(entities, transactionId);
                        if (timeInvoiceTransaction == null)
                            continue;

                        timeInvoiceTransaction.Exported = false;
                        timeInvoiceTransaction.AttestState = attestStateMinInvoice;
                        timeInvoiceTransactionUpdates.Add(timeInvoiceTransaction);
                    }

                    //Try save changes
                    using (TransactionScope transaction = CreateTransactionScope(new TimeSpan(2, 0, 0)))
                    {
                        entities.Connection.Open();

                        try
                        {
                            result = BulkUpdate(entities, timePayrollTransactionUpdates, transaction);

                            if (result.Success)
                                result = BulkUpdate(entities, timeInvoiceTransactions, transaction);

                            if (result.Success)
                                result = BulkUpdate(entities, timeBlockDateUpdates, transaction);

                            if (result.Success)
                                result = BulkInsert(entities, attestTransitionLogInserts, transaction);

                            if (result.Success)
                                result = SaveChanges(entities, transaction);

                            if (result.Success)
                                transaction.Complete();

                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessage = "Failed on update";
                            result.Success = false;
                            LogError(ex, log);
                        }
                        finally
                        {
                            entities.Connection.Close();
                        }
                    }

                    #endregion

                }
            }

            if (result.Success && !isPreliminary)
                CommunicationManager.TryDeletePayrollfromICASftp(actorCompanyId, timeSalaryExportId);

            base.CompleteTask(taskWatchLogId);

            return result;
        }

        public string GetExternalExportId(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetExternalExportId(entities, actorCompanyId);
        }

        public string GetExternalExportId(CompEntities entities, int actorCompanyId)
        {
            string externalExportId = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportExternalExportID, 0, actorCompanyId, 0);

            if (externalExportId == null || !externalExportId.Contains("#"))
                return externalExportId;

            var array = externalExportId.Split('#');

            return array[0];
        }

        public string GetExternalExportSubId(CompEntities entities, int actorCompanyId)
        {
            string externalExportId = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportExternalExportSubId, 0, actorCompanyId, 0);

            if (externalExportId == null || !externalExportId.Contains("#"))
                return externalExportId;

            var array = externalExportId.Split('#');

            return array[0];
        }

        public List<int> GetAxfoodsNotInAxfoodDatabase(CompEntities entities)
        {
            return entities.UserCompanySetting.Where(w => w.ActorCompanyId.HasValue && w.SettingTypeId == (int)CompanySettingType.SalaryExportExternalExportID && !string.IsNullOrEmpty(w.StrData) && w.StrData.Contains("#Axfood")).Select(s => s.ActorCompanyId.Value).Distinct().ToList();
        }

        public string GetSecondExternalExportId(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSecondExternalExportId(entities, actorCompanyId);
        }

        public string GetSecondExternalExportId(CompEntities entities, int actorCompanyId)
        {
            string externalExportId = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportExternalExportID, 0, actorCompanyId, 0);

            if (externalExportId == null || !externalExportId.Contains("#"))
                return string.Empty;

            var array = externalExportId.Split('#');

            if (array.Any())
                return array[1];

            return string.Empty;
        }

        #region Private methods

        private List<EmployeeSchedule> GetEmployeeSchedule(CompEntities entities, List<int> employeeIds, DateTime startDate, DateTime stopDate, int actorCompanyId)
        {
            var companyEmployeeSchedules = (from es in entities.EmployeeSchedule
                                                //.Include("Employee")                                                
                                                .Include("TimeScheduleTemplateHead.TimeScheduleTemplatePeriod")
                                            where
                                            es.Employee.ActorCompanyId == actorCompanyId &&
                                            ((es.StartDate >= startDate && es.StopDate <= stopDate) ||
                                            (es.StartDate < startDate && es.StopDate <= stopDate) ||
                                            (es.StartDate >= startDate && es.StopDate > stopDate) ||
                                            (es.StartDate < startDate && es.StopDate > stopDate)) &&
                                            es.State == (int)SoeEntityState.Active
                                            select es).ToList();

            return companyEmployeeSchedules.Where(schedule => employeeIds.Contains(schedule.EmployeeId)).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetEmployeeScheduleTemplateBlocks(CompEntities entities, List<int> employeeIds, DateTime startDate, DateTime stopDate, int actorCompanyId, bool includeAccount = true)
        {
            List<TimeScheduleTemplateBlock> employeeScheduleTemplateBlocks = null;
            if (employeeIds.Count > 1000)
            {
                IQueryable<TimeScheduleTemplateBlock> q = (from tb in entities.TimeScheduleTemplateBlock
                                                           .Include("TimeCode.TimeCodePayrollProduct")
                                                           where !tb.TimeScheduleScenarioHeadId.HasValue &&
                                                           tb.Employee.ActorCompanyId == actorCompanyId &&
                                                           tb.EmployeeId.HasValue &&
                                                           tb.Date.HasValue && tb.Date.Value >= startDate && tb.Date.Value <= stopDate &&
                                                           tb.State == (int)SoeEntityState.Active
                                                           select tb);

                if (includeAccount)
                    q = q.Include("AccountInternal.Account");

                employeeScheduleTemplateBlocks = q.ToList();
                employeeScheduleTemplateBlocks = employeeScheduleTemplateBlocks.Where(w => employeeIds.Contains(w.EmployeeId.Value)).ToList();
            }
            else
            {
                IQueryable<TimeScheduleTemplateBlock> q = (from tb in entities.TimeScheduleTemplateBlock
                                              .Include("TimeCode.TimeCodePayrollProduct")
                                                           where !tb.TimeScheduleScenarioHeadId.HasValue &&
                                                           tb.Employee.ActorCompanyId == actorCompanyId &&
                                                           tb.EmployeeId.HasValue && employeeIds.Contains(tb.EmployeeId.Value) &&
                                                           tb.Date.HasValue && tb.Date.Value >= startDate && tb.Date.Value <= stopDate &&
                                                           tb.State == (int)SoeEntityState.Active
                                                           select tb);

                if (includeAccount)
                    q = q.Include("AccountInternal.Account");

                employeeScheduleTemplateBlocks = q.ToList();
            }
            return employeeScheduleTemplateBlocks;
        }

        private List<AttestState> GetValidAttestStates(AttestState minState, List<AttestState> previousAttestStates, IEnumerable<AttestTransition> attestTransitions)
        {
            List<AttestState> attestStates = previousAttestStates;
            foreach (AttestTransition attestTransition in attestTransitions)
            {
                AttestTransition attestTransition1 = attestTransition;
                if (attestTransition.AttestStateFromId == minState.AttestStateId && !previousAttestStates.Any(i => i.AttestStateId == attestTransition1.AttestStateFromId))
                {
                    attestStates.Add(attestTransition.AttestStateFrom);
                    attestStates = (GetValidAttestStates(attestTransition.AttestStateTo, attestStates, attestTransitions));
                }
            }

            return attestStates;
        }

        private List<TimePayrollTransaction> GetPayrollTransactions(CompEntities entities, List<int> employeeIds, DateTime startDate, DateTime stopDate, int actorCompanyId, AttestState minState, bool isPreliminaryExport)
        {
            List<TimePayrollTransaction> filteredTransactions = null;
            if (employeeIds.Count > 500)
            {
                filteredTransactions = new List<TimePayrollTransaction>();
                List<int> remainingIds = employeeIds;
                List<int> currentIds = new List<int>();

                while (remainingIds.Any())
                {
                    currentIds = remainingIds.Take(300).ToList();
                    filteredTransactions.AddRange((from p in entities.TimePayrollTransaction
                                                                       .Include("TimeBlock")
                                                                       .Include("TimeCodeTransaction")
                                                                       .Include("AccountInternal")
                                                                       .Include("TimeBlockDate")
                                                   where p.ActorCompanyId == actorCompanyId &&
                                                                  currentIds.Contains(p.EmployeeId) &&
                                                   //!p.Exported &&
                                                   p.State == (int)SoeEntityState.Active &&
                                                   (p.TimeBlock == null || p.TimeBlock.State == (int)SoeEntityState.Active) &&
                                                   p.TimeBlockDate.Date >= startDate &&
                                                   p.TimeBlockDate.Date <= stopDate
                                                   select p).ToList());

                    remainingIds = remainingIds.Where(w => !currentIds.Contains(w)).ToList();
                }

                filteredTransactions = filteredTransactions.Where(w => employeeIds.Contains(w.EmployeeId)).ToList();
            }
            else
            {
                filteredTransactions = (from p in entities.TimePayrollTransaction
                                                                   .Include("TimeBlock")
                                                                   .Include("TimeCodeTransaction")
                                                                   .Include("AccountInternal")
                                                                   .Include("TimeBlockDate")
                                        where p.ActorCompanyId == actorCompanyId &&
                                                       employeeIds.Contains(p.EmployeeId) &&
                                        //!p.Exported &&
                                        p.State == (int)SoeEntityState.Active &&
                                        (p.TimeBlock == null || p.TimeBlock.State == (int)SoeEntityState.Active) &&
                                        p.TimeBlockDate.Date >= startDate &&
                                        p.TimeBlockDate.Date <= stopDate
                                        select p).ToList();
            }

            return isPreliminaryExport ? filteredTransactions : filteredTransactions.Where(i => i.AttestStateId == minState.AttestStateId && !i.Exported).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetScheduleTransactions(CompEntities entities, int actorCompanyId, Dictionary<int, List<DateTime>> employeesAndDates, List<int> payrollProductsToExclude)
        {
            if (employeesAndDates.IsNullOrEmpty())
                return new List<TimePayrollScheduleTransaction>();

            List<int> employeeIds = employeesAndDates.Select(x => x.Key).ToList();
            List<DateTime> dates = employeesAndDates.SelectMany(x => x.Value).Distinct().ToList();

            if (!dates.Any())
                return new List<TimePayrollScheduleTransaction>();

            DateTime fromDate = dates.OrderBy(x => x.Date).First();
            DateTime toDate = dates.OrderBy(x => x.Date).Last();

            List<int> remainingIds = employeeIds;
            List<int> currentIds = new List<int>();

            List<TimePayrollScheduleTransaction> scheduleTransactions = new List<TimePayrollScheduleTransaction>();
            while (remainingIds.Any())
            {
                currentIds = remainingIds.Take(300).ToList();
                scheduleTransactions.AddRange((from tpt in entities.TimePayrollScheduleTransaction
                                                                       .Include("TimeBlockDate")
                                                                       .Include("AccountInternal")
                                               where tpt.ActorCompanyId == actorCompanyId &&
                                               currentIds.Contains(tpt.EmployeeId) &&
                                               tpt.TimeBlockDate.Date >= fromDate &&
                                               tpt.TimeBlockDate.Date <= toDate &&
                                               tpt.State == (int)SoeEntityState.Active &&
                                               tpt.Type == (int)SoeTimePayrollScheduleTransactionType.Schedule
                                               select tpt).ToList());

                remainingIds = remainingIds.Where(w => !currentIds.Contains(w)).ToList();
            }

            scheduleTransactions = scheduleTransactions.Where(w => employeeIds.Contains(w.EmployeeId) && !payrollProductsToExclude.Contains(w.ProductId)).ToList();

            List<TimePayrollScheduleTransaction> filteredTimePayrollScheduleTransactions = new List<TimePayrollScheduleTransaction>();
            Dictionary<int, List<TimePayrollScheduleTransaction>> timePayrollScheduleTransactionsDict = scheduleTransactions.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var employeeAndDates in employeesAndDates)
            {
                var employeeTransactions = timePayrollScheduleTransactionsDict.ContainsKey(employeeAndDates.Key) ? timePayrollScheduleTransactionsDict.FirstOrDefault(f => f.Key == employeeAndDates.Key).Value : new List<TimePayrollScheduleTransaction>();
                var filteredEmployeeTransactions = employeeTransactions.Where(x => employeeAndDates.Value.Contains(x.TimeBlockDate.Date)).ToList();

                filteredTimePayrollScheduleTransactions.AddRange(filteredEmployeeTransactions);
            }

            return filteredTimePayrollScheduleTransactions;
        }


        /// <summary>
        /// Updates transaction status
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="timeSalary"></param>
        /// <param name="employeeIds"></param>
        /// <param name="timePayrollTransactions"></param>
        /// <param name="timeInvoiceTransactions"></param>
        /// <param name="attestStateResultingInvoice"></param>
        /// <param name="attestTransitionInvoice"></param>
        /// <param name="attestStateResultingPayroll"></param>
        /// <param name="attestTransitionPayroll"></param>
        /// <param name="actorCompanyId"></param>
        /// <param name="userId"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private ActionResult ChangeTransactionStatusToExportForNotPreliminaryExport(CompEntities entities, TimeSalaryExport timeSalary, List<int> employeeIds, List<TimePayrollTransaction> timePayrollTransactions, List<TimeInvoiceTransaction> timeInvoiceTransactions, List<TimeBlockDate> timeBlockDates, AttestState attestStateResultingInvoice, AttestTransition attestTransitionInvoice, AttestState attestStateResultingPayroll, AttestTransition attestTransitionPayroll, int actorCompanyId, int userId, TransactionScope transaction, bool useBulk, bool lockPeriod)
        {
            if (timeSalary.IsPreliminary)
                return new ActionResult(true);

            Dictionary<int, List<TimePayrollTransaction>> timePayrollTransactionsGrouped = timePayrollTransactions.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<int, List<TimeInvoiceTransaction>> timeInvoiceTransactionsGrouped = timeInvoiceTransactions.Where(x => x.EmployeeId.HasValue).GroupBy(g => g.EmployeeId.Value).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<int, List<TimeBlockDate>> timeBlockDatesGrouped = timeBlockDates?.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList()) ?? new Dictionary<int, List<TimeBlockDate>>();

            ActionResult result = new ActionResult();

            entities.TimeSalaryExport.AddObject(timeSalary);
            if (useBulk)
            {
                result = SaveChanges(entities, transaction);
                if (!result.Success)
                    return result;
            }

            int commitCount = SettingManager.GetIntSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.CommitCountInSalaryExport, 0, 0, 0);
            if (commitCount == 0)
                commitCount = 2;

            int unsavedCount = 0;
            int employeeCount = employeeIds.Count;
            int totalCount = 0;
            List<AttestTransitionLog> attestTransitionLogInserts = new List<AttestTransitionLog>();
            List<TimeSalaryExportRow> timeSalaryExportRowInserts = new List<TimeSalaryExportRow>();
            List<TimePayrollTransaction> timePayrollTransactionUpdates = new List<TimePayrollTransaction>();
            List<TimeInvoiceTransaction> timeInvoiceTransactionUpdates = new List<TimeInvoiceTransaction>();
            List<TimeBlockDate> timeBlockDateInserts = new List<TimeBlockDate>();
            List<TimeBlockDate> timeBlockDateUpdates = new List<TimeBlockDate>();

            foreach (int employeeId in employeeIds)
            {
                List<TimePayrollTransaction> timePayrollTransactionsForEmployee = timePayrollTransactionsGrouped.ContainsKey(employeeId) ? timePayrollTransactionsGrouped[employeeId] : new List<TimePayrollTransaction>();
                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactionsForEmployee)
                {
                    #region TimePayrollTransaction

                    if (timePayrollTransaction.AttestStateId > 0 && attestStateResultingPayroll != null)
                    {
                        //Add AttestTransitionLog
                        AttestTransitionLog attestTransitionLog = new AttestTransitionLog
                        {
                            Date = DateTime.Now,
                            Entity = (int)TermGroup_AttestEntity.PayrollTime,
                            RecordId = timePayrollTransaction.TimePayrollTransactionId,
                            UserId = userId,

                            //Set FK
                            ActorCompanyId = actorCompanyId,
                            AttestTransitionId = attestTransitionPayroll.AttestTransitionId,
                        };
                        if (!useBulk)
                        {

                            entities.AttestTransitionLog.AddObject(attestTransitionLog);
                        }
                        else
                            attestTransitionLogInserts.Add(attestTransitionLog);
                    }

                    //Update TimePayrolLTransaction
                    if (attestStateResultingPayroll != null)
                        timePayrollTransaction.AttestStateId = attestStateResultingPayroll.AttestStateId;
                    timePayrollTransaction.Exported = true;
                    SetModifiedProperties(timePayrollTransaction);

                    if (useBulk)
                        timePayrollTransactionUpdates.Add(timePayrollTransaction);

                    var row = new TimeSalaryExportRow
                    {
                        Entity = (int)TermGroup_AttestEntity.PayrollTime,
                        RecordId = timePayrollTransaction.TimePayrollTransactionId,
                        EmployeeId = timePayrollTransaction.EmployeeId,
                        TimeSalaryExportId = timeSalary.TimeSalaryExportId
                    };
                    if (!useBulk)
                    {
                        entities.TimeSalaryExportRow.AddObject(row);
                        timeSalary.TimeSalaryExportRow.Add(row);
                    }
                    else
                        timeSalaryExportRowInserts.Add(row);

                    #endregion
                }

                List<TimeInvoiceTransaction> timeInvoiceTransactionsForEmployee = timeInvoiceTransactionsGrouped.ContainsKey(employeeId) ? timeInvoiceTransactionsGrouped[employeeId] : new List<TimeInvoiceTransaction>();
                foreach (TimeInvoiceTransaction timeInvoiceTransaction in timeInvoiceTransactionsForEmployee)
                {
                    #region TimeInvoiceTransaction

                    if (timeInvoiceTransaction.AttestStateId > 0 && attestStateResultingInvoice != null)
                    {
                        //Add AttestTransitionLog
                        AttestTransitionLog attestTransitionLog = new AttestTransitionLog
                        {
                            Date = DateTime.Now,
                            Entity = (int)TermGroup_AttestEntity.InvoiceTime,
                            RecordId = timeInvoiceTransaction.TimeInvoiceTransactionId,
                            UserId = userId,

                            //Set FK
                            ActorCompanyId = actorCompanyId,
                        };
                        if (attestTransitionInvoice != null)
                            attestTransitionLog.AttestTransitionId = attestTransitionInvoice.AttestTransitionId;

                        if (!useBulk)
                            entities.AttestTransitionLog.AddObject(attestTransitionLog);
                        else
                            attestTransitionLogInserts.Add(attestTransitionLog);
                    }

                    //Update TimeInvoiceTransaction
                    timeInvoiceTransaction.AttestStateId = attestStateResultingInvoice?.AttestStateId;
                    timeInvoiceTransaction.Exported = true;
                    SetModifiedProperties(timeInvoiceTransaction);
                    if (useBulk)
                        timeInvoiceTransactionUpdates.Add(timeInvoiceTransaction);

                    var row = new TimeSalaryExportRow
                    {
                        Entity = (int)TermGroup_AttestEntity.InvoiceTime,
                        RecordId = timeInvoiceTransaction.TimeInvoiceTransactionId,
                        EmployeeId = timeInvoiceTransaction.EmployeeId,
                        TimeSalaryExportId = timeSalary.TimeSalaryExportId
                    };
                    if (!useBulk)
                    {
                        timeSalary.TimeSalaryExportRow.Add(row);
                        entities.TimeSalaryExportRow.AddObject(row);
                    }
                    else
                        timeSalaryExportRowInserts.Add(row);

                    #endregion
                }

                if (lockPeriod)
                {
                    (List<TimeBlockDate> timeBlockDateInsertsForEmployee, List<TimeBlockDate> timeBlockDateUpdatesForEmployee) = SetTimeBlockDateStatus(entities, employeeId, actorCompanyId, timeBlockDatesGrouped.ContainsKey(employeeId) ? timeBlockDatesGrouped[employeeId] : new List<TimeBlockDate>(), timeSalary.StartInterval, timeSalary.StopInterval, SoeTimeBlockDateStatus.Locked, true, useBulk);
                    timeBlockDateInserts.AddRange(timeBlockDateInsertsForEmployee);
                    timeBlockDateUpdates.AddRange(timeBlockDateUpdatesForEmployee);
                }

                totalCount++;
                unsavedCount++;

                if (!useBulk)
                {
                    if (unsavedCount == commitCount || totalCount == employeeCount)
                    {
                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                        {
                            result.ErrorNumber = (int)ActionResultSave.TimeSalaryExportTransactionUpdateFailed;
                            return result;
                        }
                        unsavedCount = 0;
                    }
                }
            }

            if (useBulk)
            {
                if (employeeIds.Count > 300)
                    LogInfo("TimeSalaryExport: Bulk - Begin bulk update timePayrollTransactionUpdates");
                result = BulkUpdate(entities, timePayrollTransactionUpdates, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryExport: Bulk - Done bulk update timePayrollTransactionUpdates");

                if (employeeIds.Count > 300)
                    LogInfo("TimeSalaryExport: Bulk - Begin bulk update timeInvoiceTransactionUpdates");
                result = BulkUpdate(entities, timeInvoiceTransactionUpdates, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryExport: Bulk - Done bulk update timeInvoiceTransactionUpdates");

                if (employeeIds.Count > 300)
                    LogInfo("TimeSalaryExport: Bulk - Begin bulk update timeblockdates");
                result = BulkUpdate(entities, timeBlockDateUpdates, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryExport: Bulk - Done bulk update timeblockdates");

                if (employeeIds.Count > 300)
                    LogInfo("TimeSalaryExport: Bulk - Begin bulk insert timeblockDateInserts");
                result = BulkInsert(entities, timeBlockDateInserts, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryExport: Bulk - Done bulk insert timeblockDateInserts");

                if (employeeIds.Count > 300)
                    LogInfo("TimeSalaryExport: Bulk - Begin bulk insert attestTransitionLogInserts");
                result = BulkInsert(entities, attestTransitionLogInserts, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryExport: Bulk - Done bulk insert attestTransitionLogInserts");

                if (employeeIds.Count > 300)
                    LogInfo("TimeSalaryExport: Bulk - Begin bulk insert timeSalaryExportRowInserts");
                result = BulkInsert(entities, timeSalaryExportRowInserts, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryExport: Bulk - Done bulk insert timeSalaryExportRowInserts");
            }

            return result;
        }

        private (List<TimeBlockDate>, List<TimeBlockDate>) SetTimeBlockDateStatus(CompEntities entities, int employeeId, int actorCompanyId, List<TimeBlockDate> existingTimeBlockDates, DateTime startDate, DateTime stopDate, SoeTimeBlockDateStatus newStatus, bool createIfMissing, bool useBulk)
        {
            List<TimeBlockDate> timeBlockDateInserts = new List<TimeBlockDate>();
            List<TimeBlockDate> timeBlockDateUpdates = new List<TimeBlockDate>();

            DateTime currentDate = startDate.Date;
            while (currentDate <= stopDate.Date)
            {
                TimeBlockDate timeBlockDate = existingTimeBlockDates.FirstOrDefault(x => x.Date == currentDate.Date);
                if (createIfMissing && timeBlockDate == null)
                {
                    timeBlockDate = TimeBlockManager.CreateTimeBlockDate(entities, currentDate.Date, employeeId, actorCompanyId, addtoEntitiesAndSaveChanges: !useBulk);
                    if (timeBlockDate != null && useBulk)
                        timeBlockDateInserts.Add(timeBlockDate);
                }
                else
                {
                    if (timeBlockDate != null && useBulk)
                        timeBlockDateUpdates.Add(timeBlockDate);
                }

                if (timeBlockDate != null)
                    timeBlockDate.Status = (int)newStatus;

                currentDate = currentDate.AddDays(1);
            }

            return (timeBlockDateInserts, timeBlockDateUpdates);
        }

        #endregion

        #endregion

        #endregion

        #region TimeSalaryPaymentExport

        #region Export

        public ActionResult ExportSalaryPaymentExtendedSelection(int actorCompanyId, int userId, int roleId, int basedOnTimeSalarPaymentExportId, DateTime currencyDate, decimal currencyRate, TermGroup_Currency currency)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var timeSalaryPaymentExport = GetTimeSalaryPaymentExport(entitiesReadOnly, basedOnTimeSalarPaymentExportId, actorCompanyId, false);
            if (timeSalaryPaymentExport == null)
                return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8163, "Exporterat lön kunde inte hittas"));

            var timePeriod = TimePeriodManager.GetTimePeriod(timeSalaryPaymentExport.TimePeriodId, actorCompanyId, loadTimePeriodHead: true);
            if (timePeriod == null || timePeriod.TimePeriodHead == null)
                return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8163, "Exporterat lön kunde inte hittas"));

            DateTime? salarySpecificationPublishDate = GetSalarySpecificationPublishDate(actorCompanyId, timeSalaryPaymentExport.TimeSalaryPaymentExportId, timePeriod.TimePeriodId);
            var employees = EmployeeManager.GetEmployeesForPayrollPaymentSelection(actorCompanyId, userId, roleId, timePeriod.TimePeriodId, 0, true);
            if (!employees.Any())
                return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8743, "Inga anställda hittades"));

            List<Employee> validEmployees;
            ActionResult result = ExportSalaryPayment(actorCompanyId, timePeriod.TimePeriodHead.TimePeriodHeadId, timePeriod.TimePeriodId, employees.Select(x => x.EmployeeId).ToList(), salarySpecificationPublishDate, timeSalaryPaymentExport.DebitDate, userId, true, currencyRate, currencyDate, currency, out validEmployees);
            if (result.Success && !validEmployees.IsNullOrEmpty() && salarySpecificationPublishDate.HasValue)
                CommunicationManager.SendXEMailPayrollSlipPublishedWhenCreatingBankfile(actorCompanyId, validEmployees.Where(x => x.UserId.HasValue).Select(x => x.UserId.Value).ToList(), salarySpecificationPublishDate, userId, roleId);

            return result;
        }

        public ActionResult ExportSalaryPayment(int actorCompanyId, int timePeriodHeadId, int timePeriodId, List<int> employeeIds, DateTime? salarySpecificationPublishDate, DateTime? debitDate, int userId, int roleId)
        {
            List<Employee> validEmployees;
            ActionResult result = ExportSalaryPayment(actorCompanyId, timePeriodHeadId, timePeriodId, employeeIds, salarySpecificationPublishDate, debitDate, userId, false, null, null, TermGroup_Currency.SEK, out validEmployees);
            if (result.Success && !validEmployees.IsNullOrEmpty() && salarySpecificationPublishDate.HasValue)
                CommunicationManager.SendXEMailPayrollSlipPublishedWhenCreatingBankfile(actorCompanyId, validEmployees.Where(x => x.UserId.HasValue).Select(x => x.UserId.Value).ToList(), salarySpecificationPublishDate, userId, roleId);

            return result;
        }

        public ActionResult ExportSalaryPayment(int actorCompanyId, int timePeriodHeadId, int timePeriodId, List<int> employeeIds, DateTime? salarySpecificationPublishDate, DateTime? debitDate, int userId, bool useExtendSelection, decimal? currencyRate, DateTime? currencyDate, TermGroup_Currency currency, out List<Employee> validEmployees)
        {
            validEmployees = new List<Employee>();
            if (useExtendSelection && (!currencyRate.HasValue || currency == TermGroup_Currency.SEK))
                return new ActionResult(false, (int)ActionResultSave.Unknown, GetText(9323, "Felaktiga inparametrar"));

            if (!useExtendSelection && (currencyRate.HasValue || currency != TermGroup_Currency.SEK))
                return new ActionResult(false, (int)ActionResultSave.Unknown, GetText(9323, "Felaktiga inparametrar"));

            using (CompEntities entities = new CompEntities())
            {
                ActionResult result = new ActionResult();
                LogInfo("TimeSalaryPaymentExport: Begin Export " + actorCompanyId);

                try
                {
                    #region Prereq

                    var dataStorages = (from d in entities.DataStorage
                                        where d.EmployeeId.HasValue &&
                                        d.ActorCompanyId == actorCompanyId &&
                                        employeeIds.Contains(d.EmployeeId.Value) &&
                                        d.TimePeriodId == timePeriodId
                                        && d.State == (int)SoeEntityState.Active
                                        select new
                                        {
                                            EmployeeId = d.EmployeeId,
                                            TimePeriodId = d.TimePeriodId,
                                        }).ToList();


                    Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                    if (company == null)
                        return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8162, "Företag kunde inte hittas"));

                    SysCountryDTO sysCountry = company.SysCountryId.HasValue ? CountryCurrencyManager.GetSysCountry(company.SysCountryId.Value) : null;
                    int paymentInformationRowId = SettingManager.GetIntSetting(entities, SettingMainType.Company, useExtendSelection ? (int)CompanySettingType.SalaryPaymentExportExtendedPaymentAccount : (int)CompanySettingType.SalaryPaymentExportPaymentAccount, 0, actorCompanyId, 0);
                    PaymentInformationRowDTO paymentInformation = paymentInformationRowId != 0 ? PaymentManager.GetPaymentInformationRow(entities, paymentInformationRowId, actorCompanyId).ToDTO() : null;
                    int exportTarget = useExtendSelection ? (int)TermGroup_TimeSalaryPaymentExportType.ISO20022 : SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportType, 0, actorCompanyId, 0);
                    if (exportTarget <= 0)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8607, "Företagsinställning för exportformat saknas"));

                    bool usePaymentDateAsExecutionDate = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportUsePaymentDateAsExecutionDate, 0, actorCompanyId, 0);
                    var timePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, actorCompanyId, true);
                    if (timePeriod == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8605, "Kan inte hitta vald period"));
                    if (timePeriod.TimePeriodHead != null && timePeriod.TimePeriodHead.TimePeriodHeadId != timePeriodHeadId)
                        return new ActionResult(false, (int)ActionResultSave.IncorrectInput, GetText(8604, "Vald period och perioduppsättning matchar inte."));
                    if (!timePeriod.PaymentDate.HasValue)
                        return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(8606, "Vald period saknar utbetalningsdatum"));

                    if (exportTarget == (int)TermGroup_TimeSalaryPaymentExportType.ISO20022)
                    {
                        if (!usePaymentDateAsExecutionDate && !debitDate.HasValue)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(9969, "Debiteringsdatum måste anges"));
                        if (debitDate.HasValue && debitDate.Value > timePeriod.PaymentDate.Value)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(9970, "Debiteringsdatum kan inte vara senare än utbetalningsdatum"));
                        if (paymentInformation == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(9968, "Utbetalningskonto ej angivet"));
                        if (!paymentInformation.IsValidForISO20022SalaryPayment())
                        {
                            string message = GetText(9972, "Utbetalningskontot har ett felaktigt format.") + Environment.NewLine;
                            if (paymentInformation.IsPaymentTypeBIC())
                                message += GetText(9973, "BIC koden ska anges i BIC fältet och IBAN numret ska anges i konto fältet. Gå till betalningsuppgifter på företagssidan (redigera företag)");
                            if (paymentInformation.IsPaymentTypeBANK())
                                message += GetText(9974, "Giltigt format: Clearingnr/Kontonr.");

                            return new ActionResult((int)ActionResultSave.EntityNotFound, message);
                        }
                    }

                    var attestStateTimeMin = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, 0, actorCompanyId, 0));
                    var attestStatePayrollMin = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentApproved2AttestStateId, 0, actorCompanyId, 0), true);
                    if (attestStatePayrollMin == null || attestStateTimeMin == null)
                        return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8159, "Lägsta status för export av löneartstransaktioner kunde inte hittas"));

                    var attestStatePayrollResulting = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, 0, actorCompanyId, 0));
                    if (attestStatePayrollResulting == null)
                        return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8157, "Status efter export för löneartstransaktioner kunde inte hittas"));

                    var attestTransitionPayroll = AttestManager.GetUserAttestTransitionForState(entities, TermGroup_AttestEntity.PayrollTime, attestStatePayrollMin.AttestStateId, attestStatePayrollResulting.AttestStateId, actorCompanyId, userId);
                    if (attestTransitionPayroll == null)
                        return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8160, "Giltig attestövergång för löneartstransaktioner kunde inte hittas"));

                    var selectedEmployees = EmployeeManager.GetAllEmployeesByIds(entities, actorCompanyId, employeeIds, loadEmployment: true);
                    List<int> payrollProductNotUseInPayrollIds = ProductManager.GetPayrollProductsIgnoreState(entities, actorCompanyId).Where(x => !x.UseInPayroll).Select(x => x.ProductId).ToList();

                    #endregion

                    #region Prereq

                    DateTime date = DateTime.Now;
                    entities.CommandTimeout = 600;

                    #endregion

                    #region TimePayrollTransactions

                    LogInfo("TimeSalaryPaymentExport: Fetching transactions");

                    List<TimePayrollTransaction> transactions = GetPayrollTransactionsForSalaryPaymentExport(entities, employeeIds, timePeriod, actorCompanyId);
                    List<TimePayrollTransaction> payrollTransactions = transactions.Where(x => x.AttestStateId == attestStatePayrollMin.AttestStateId).ToList();
                    List<TimePayrollTransaction> timeTransactionsNotUseInPayroll = transactions.Where(x => x.AttestStateId == attestStateTimeMin.AttestStateId && payrollProductNotUseInPayrollIds.Contains(x.ProductId)).ToList();

                    LogInfo("TimeSalaryPaymentExport: Done fetching transactions");

                    //Extract all unique employees 
                    var employeeIdsToExport = payrollTransactions.Select(i => i.EmployeeId).ToList();
                    employeeIdsToExport = employeeIdsToExport.Distinct().ToList();
                    var employees = selectedEmployees.Where(x => employeeIdsToExport.Contains(x.EmployeeId)).ToList();

                    LogInfo("TimeSalaryPaymentExport: Fetching employeeTimePeriods");

                    var employeeTimePeriods = GetEmployeeTimePeriods(entities, actorCompanyId, timePeriod.TimePeriodId, employeeIdsToExport).Where(x => x.Status == (int)SoeEmployeeTimePeriodStatus.Locked).ToList();

                    LogInfo("TimeSalaryPaymentExport: Done Fetching employeeTimePeriods");

                    foreach (var employee in employees)
                    {
                        if (!employeeTimePeriods.Any(x => x.EmployeeId == employee.EmployeeId))
                            continue;

                        decimal netSalaryAmount = payrollTransactions.Where(x => x.EmployeeId == employee.EmployeeId && x.IsNetSalaryPaid() && x.Amount.HasValue).Sum(i => i.Amount.Value);
                        if (netSalaryAmount < 0)
                            continue;


                        if ((employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_AccountDeposit || employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_NorweiganAccount) && string.IsNullOrEmpty(employee.DisbursementAccountNr))
                            continue;

                        if (useExtendSelection)
                        {
                            if (employee.DisbursementMethod != (int)TermGroup_EmployeeDisbursementMethod.SE_NorweiganAccount)
                                continue;
                        }
                        else
                        {
                            if (employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_NorweiganAccount)
                                continue;
                        }

                        validEmployees.Add(employee);
                    }

                    if (SettingManager.SiteType == TermGroup_SysPageStatusSiteType.Live)
                    {
                        string missingPayrollSlips = string.Empty;
                        foreach (var validEmployee in validEmployees)
                        {
                            if (!dataStorages.Any(d => d.EmployeeId == validEmployee.EmployeeId))
                                missingPayrollSlips += validEmployee.EmployeeNrAndName + Environment.NewLine;
                        }

                        if (!string.IsNullOrEmpty(missingPayrollSlips))
                            return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "Lönespecifikation saknas för anställd/anställda: " + Environment.NewLine + missingPayrollSlips);
                    }

                    #endregion

                    #region Settings

                    bool isSenderRegisterHolder = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportCompanyIsRegisterHolder, 0, actorCompanyId, 0);
                    string bankGiro = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportSenderBankGiro, 0, actorCompanyId, 0);

                    string customerId = SettingManager.GetStringSetting(entities, SettingMainType.Company, useExtendSelection ? (int)CompanySettingType.SalaryPaymentExportExtendedSenderIdentification : (int)CompanySettingType.SalaryPaymentExportSenderIdentification, 0, actorCompanyId, 0);
                    string agreementNumber = SettingManager.GetStringSetting(entities, SettingMainType.Company, useExtendSelection ? (int)CompanySettingType.SalaryPaymentExportExtendedAgreementNumber : (int)CompanySettingType.SalaryPaymentExportAgreementNumber, 0, actorCompanyId, 0);
                    string divisionName = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportDivisionName, 0, actorCompanyId, 0);

                    bool useIBANonEmployee = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportUseIBANOnEmployee, 0, actorCompanyId, 0);

                    #endregion

                    #region Create File

                    LogInfo("TimeSalaryPaymentExport: Creating file");

                    SalaryPaymentBaseAdapter salaryPaymentAdapter = null;
                    switch ((TermGroup_TimeSalaryPaymentExportType)exportTarget)
                    {
                        case TermGroup_TimeSalaryPaymentExportType.SUS:
                            salaryPaymentAdapter = new SUSAdapter(validEmployees, payrollTransactions, timePeriod, isSenderRegisterHolder, customerId, company.OrgNr);
                            break;
                        case TermGroup_TimeSalaryPaymentExportType.Nordea:
                            salaryPaymentAdapter = new NordeaAdapter(validEmployees, payrollTransactions, timePeriod, customerId);
                            break;
                        case TermGroup_TimeSalaryPaymentExportType.BGCLB:
                            salaryPaymentAdapter = new BGCLBAdapter(validEmployees, payrollTransactions, timePeriod);
                            break;
                        case TermGroup_TimeSalaryPaymentExportType.BGCKI:
                            salaryPaymentAdapter = new BGCKIAdapter(validEmployees, payrollTransactions, timePeriod, customerId, bankGiro, "SEK");
                            break;
                        case TermGroup_TimeSalaryPaymentExportType.ISO20022:
                            if (!debitDate.HasValue)
                                debitDate = usePaymentDateAsExecutionDate ? timePeriod.PaymentDate.Value : timePeriod.PaymentDate.Value.AddDays(-1);

                            salaryPaymentAdapter = new ISO20022Adapter(validEmployees, payrollTransactions, timePeriod, company, paymentInformation, customerId, sysCountry?.Code ?? "SE", debitDate.Value, agreementNumber, currencyRate, currency.ToString(), useIBANonEmployee, divisionName);
                            break;
                        default:
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8607, "Företagsinställning för exportformat saknas"));
                    }

                    byte[] salary = salaryPaymentAdapter.CreateFile();

                    LogInfo("TimeSalaryPaymentExport: Done creating file");

                    #endregion

                    using (TransactionScope transaction = CreateTransactionScope(TimeSpan.FromHours(1)))
                    {
                        LogInfo("TimeSalaryPaymentExport: Begin transaction");



                        #region Init

                        //Open connection
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #endregion

                        #region TimeSalaryPaymentExport/TimeSalaryPaymentExportEmployee

                        //Save information about the export
                        var timeSalaryPaymentExport = new TimeSalaryPaymentExport
                        {
                            ActorCompanyId = actorCompanyId,
                            TimePeriodId = timePeriodId,
                            ExportDate = DateTime.Now,
                            ExportType = exportTarget,
                            ExportFormat = (int)salaryPaymentAdapter.ExportFormat,
                            ExportFile = salary,
                            Extension = salaryPaymentAdapter.Extension,
                            UniqueMsgKey = salaryPaymentAdapter.UniqueMsgKey.EmptyToNull(),
                            UniquePaymentKey = salaryPaymentAdapter.UniquePaymentKey.EmptyToNull(),
                            DebitDate = debitDate,
                            PaymentDate = timePeriod.PaymentDate,
                            CurrencyCode = currency.ToString(),
                            CurrencyRate = currencyRate,
                            CurrencyDate = currencyDate,
                        };
                        entities.TimeSalaryPaymentExport.AddObject(timeSalaryPaymentExport);
                        SetCreatedProperties(timeSalaryPaymentExport);

                        LogInfo("TimeSalaryPaymentExport: Start savechanges1");

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        LogInfo("TimeSalaryPaymentExport: done savechanges1");

                        LogInfo("TimeSalaryPaymentExport: start changeTransactionStatusToExport");

                        if (SettingManager.GetBoolSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.UseStoredProcedureInSalaryExport, 0, 0, 0))
                            result = ChangeTransactionStatusToExportNew(transaction, entities, salaryPaymentAdapter, timeSalaryPaymentExport, employeeTimePeriods, timeTransactionsNotUseInPayroll, timePeriod, attestStatePayrollResulting, attestTransitionPayroll, salarySpecificationPublishDate, actorCompanyId, userId, date);
                        else
                            result = ChangeTransactionStatusToExport(transaction, entities, salaryPaymentAdapter, timeSalaryPaymentExport, employeeTimePeriods, timeTransactionsNotUseInPayroll, timePeriod, attestStatePayrollResulting, attestTransitionPayroll, salarySpecificationPublishDate, actorCompanyId, userId, date);

                        if (!result.Success)
                            return result;

                        LogInfo("TimeSalaryPaymentExport: done changeTransactionStatusToExport");

                        //LogInfo("TimeSalaryPaymentExport: Start savechanges2");

                        //result = SaveChanges(entities, transaction);
                        //if (!result.Success)
                        //    return result;

                        //LogInfo("TimeSalaryPaymentExport: done savechanges2");

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        LogInfo("TimeSalaryPaymentExport: Done transaction");
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.Success = false;
                    LogError(ex, log);
                    LogInfo("TimeSalaryPaymentExport: Failed " + actorCompanyId);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult UndoSalaryPaymentExport(int actorCompanyId, int timeSalaryPaymentExportId, int userId)
        {
            ActionResult result = new ActionResult();
            bool useBulk = true;

            using (var entities = new CompEntities())
            {
                try
                {
                    LogInfo("TimeSalaryPaymentExport Undo: Begin " + actorCompanyId);

                    #region Prereq

                    entities.CommandTimeout = 600;

                    var skipAttestTransition = SettingManager.GetBoolSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.SkipAttestTransition, 0, 0, 0);
                    var attestStateTimeMin = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, 0, actorCompanyId, 0));
                    var attestStatePayrollMin = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentApproved2AttestStateId, 0, actorCompanyId, 0));
                    if (attestStatePayrollMin == null || attestStateTimeMin == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityNotFound, GetText(8166, "Lägsta status för löneartstransaktioner kunde inte hittas"));

                    var attestStateResulting = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, 0, actorCompanyId, 0));
                    if (attestStateResulting == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityNotFound, GetText(8168, "Status för exporterade löneartstransaktioner kunde inte hittas"));

                    var attestPayrollTransition = AttestManager.GetUserAttestTransitionForState(entities, TermGroup_AttestEntity.PayrollTime, attestStateResulting.AttestStateId, attestStatePayrollMin.AttestStateId, actorCompanyId, userId);
                    if (attestPayrollTransition == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8167, "Giltig attestövergång för löneartstransaktioner kunde inte hittas"));

                    LogInfo("TimeSalaryPaymentExport Undo: Get timeSalaryPaymentExport and rows");
                    var timeSalaryPaymentExport = GetTimeSalaryPaymentExport(entities, timeSalaryPaymentExportId, actorCompanyId, true);
                    if (timeSalaryPaymentExport == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8163, "Exporterat lön kunde inte hittas"));

                    List<int> timeSalaryPaymentExportEmployeeIds = timeSalaryPaymentExport.TimeSalaryPaymentExportEmployee.Where(x => x.State == (int)SoeEntityState.Active).Select(x => x.TimeSalaryPaymentExportEmployeeId).ToList();
                    List<int> employeeIds = timeSalaryPaymentExport.TimeSalaryPaymentExportEmployee.Where(x => x.State == (int)SoeEntityState.Active).Select(x => x.EmployeeId).ToList();
                    List<int> payrollProductNotUseInPayrollIds = ProductManager.GetPayrollProductsIgnoreState(entities, actorCompanyId).Where(x => !x.UseInPayroll).Select(x => x.ProductId).ToList();

                    LogInfo("TimeSalaryPaymentExport Undo: Get transactions");
                    List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactions(entities, actorCompanyId, timeSalaryPaymentExportEmployeeIds);

                    LogInfo("TimeSalaryPaymentExport Undo: Get periods");
                    List<EmployeeTimePeriod> employeeTimePeriods = GetEmployeeTimePeriods(entities, actorCompanyId, timeSalaryPaymentExport.TimePeriodId, employeeIds);

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        LogInfo("TimeSalaryPaymentExport Undo: Start transaction");

                        #region Init

                        //Open connection
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #endregion

                        #region Prereq

                        DateTime date = DateTime.Now;

                        #endregion

                        #region TimeSalaryPaymentExport

                        ChangeEntityState(timeSalaryPaymentExport, SoeEntityState.Deleted);

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        List<EmployeeTimePeriod> updatedEmployeeTimePeriods = new List<EmployeeTimePeriod>();
                        List<TimeSalaryPaymentExportEmployee> updatedTimeSalaryPaymentExportEmployees = new List<TimeSalaryPaymentExportEmployee>();
                        List<TimePayrollTransaction> updatedTimePayrollTransactions = new List<TimePayrollTransaction>();
                        List<AttestTransitionLog> createdAttestTransitionLogs = new List<AttestTransitionLog>();

                        foreach (var timeSalaryPaymentExportEmployee in timeSalaryPaymentExport.TimeSalaryPaymentExportEmployee)
                        {
                            #region TimeSalaryPaymentExportEmployee

                            //Update state
                            ChangeEntityState(timeSalaryPaymentExportEmployee, SoeEntityState.Deleted);
                            SetModifiedProperties(timeSalaryPaymentExportEmployee);
                            updatedTimeSalaryPaymentExportEmployees.Add(timeSalaryPaymentExportEmployee);


                            //Update status on EmployeeTimePeriod
                            var employeeTimePeriod = employeeTimePeriods.FirstOrDefault(x => x.EmployeeId == timeSalaryPaymentExportEmployee.EmployeeId);
                            if (employeeTimePeriod != null)
                            {
                                employeeTimePeriod.Status = (int)SoeEmployeeTimePeriodStatus.Locked;
                                SetModifiedProperties(employeeTimePeriod);
                                updatedEmployeeTimePeriods.Add(employeeTimePeriod);
                            }

                            #endregion

                            #region AttestTransitionLogs

                            foreach (var timePayrollTransaction in timePayrollTransactions.Where(x => x.TimeSalaryPaymentExportEmployeeId.HasValue && x.TimeSalaryPaymentExportEmployeeId.Value == timeSalaryPaymentExportEmployee.TimeSalaryPaymentExportEmployeeId))
                            {
                                AttestTransitionLog attestTransitionLog = new AttestTransitionLog
                                {
                                    UserId = parameterObject != null && parameterObject.SupportUserId.HasValue ? parameterObject.SupportUserId.Value : userId,
                                    Entity = (int)TermGroup_AttestEntity.PayrollTime,
                                    Date = date,
                                    RecordId = timePayrollTransaction.TimePayrollTransactionId,

                                    //Set FK
                                    ActorCompanyId = actorCompanyId,
                                    AttestTransitionId = attestPayrollTransition.AttestTransitionId,
                                };
                                createdAttestTransitionLogs.Add(attestTransitionLog);
                                if (!useBulk)
                                    entities.AttestTransitionLog.AddObject(attestTransitionLog);

                                if (payrollProductNotUseInPayrollIds.Contains(timePayrollTransaction.ProductId))
                                    timePayrollTransaction.AttestStateId = attestStateTimeMin.AttestStateId;
                                else
                                    timePayrollTransaction.AttestStateId = attestStatePayrollMin.AttestStateId;

                                timePayrollTransaction.TimeSalaryPaymentExportEmployeeId = null;
                                SetModifiedProperties(timePayrollTransaction);
                                updatedTimePayrollTransactions.Add(timePayrollTransaction);
                            }

                            #endregion

                            if (!useBulk)
                            {
                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                    return result;
                            }
                        }

                        if (useBulk)
                        {
                            LogInfo("TimeSalaryPaymentExport Undo: Bulk - Begin");

                            result = BulkUpdate(entities, updatedEmployeeTimePeriods, transaction);
                            if (!result.Success)
                                return result;
                            LogInfo("TimeSalaryPaymentExport Undo: Bulk - Done bulk update updatedEmployeeTimePeriods");

                            result = BulkUpdate(entities, updatedTimeSalaryPaymentExportEmployees, transaction);
                            if (!result.Success)
                                return result;
                            LogInfo("TimeSalaryPaymentExport Undo: Bulk - Done bulk update updatedTimeSalaryPaymentExportEmployees");
                            int counter = 0;
                            int employeeCount = updatedTimePayrollTransactions.GroupBy(g => g.EmployeeId).Count();

                            foreach (var groupedTransactionsByEmployee in updatedTimePayrollTransactions.GroupBy(g => g.EmployeeId))
                            {
                                DateTime? modified = groupedTransactionsByEmployee.First().Modified;
                                string modifiedBy = groupedTransactionsByEmployee.First().ModifiedBy;

                                counter++;
                                if (counter % 300 == 0)
                                    LogInfo($"TimeSalaryPaymentExport Undo: loop {counter}/ {employeeCount}");

                                foreach (var groupedTransactionsByEmployeeAndAttestStateId in groupedTransactionsByEmployee.GroupBy(g => g.AttestStateId))
                                {

                                    List<int> timePayrollTransactionIds = groupedTransactionsByEmployeeAndAttestStateId.Select(s => s.TimePayrollTransactionId).ToList();
                                    List<SqlParameter> sqlParameters = new List<SqlParameter>();
                                    sqlParameters.Add(new SqlParameter("@modified", CalendarUtility.ToSqlFriendlyDateTime(modified)));
                                    sqlParameters.Add(new SqlParameter("@modifiedby", modifiedBy));
                                    sqlParameters.Add(new SqlParameter("@attestStateId", groupedTransactionsByEmployeeAndAttestStateId.Key));
                                    var paramnTimeSalaryPaymentExportEmployee = new SqlParameter("@timeSalaryPaymentExportEmployeeId", SqlDbType.Int);
                                    paramnTimeSalaryPaymentExportEmployee.Value = DBNull.Value;
                                    sqlParameters.Add(paramnTimeSalaryPaymentExportEmployee);
                                    string sql = $"update timepayrollTransaction set modified = @modified, modifiedby = @modifiedby, attestStateId = @attestStateId,timeSalaryPaymentExportEmployeeId = @timeSalaryPaymentExportEmployeeId where timepayrollTransactionId in ({timePayrollTransactionIds.JoinToString<int>(",")})";

                                    result = SqlCommandUtil.ExecuteSqlUpsertCommand(entities, sql, sqlParameters);
                                    if (!result.Success)
                                        return result;

                                    try
                                    {
                                        if (!skipAttestTransition)
                                        {
                                            string sqlInsert = SqlCommandUtil.CreateAttestTransitionLogInsertStatement(actorCompanyId, attestPayrollTransition.AttestTransitionId, timePayrollTransactionIds, TermGroup_AttestEntity.PayrollTime, userId, modified ?? DateTime.Now);
                                            SqlCommandUtil.ExecuteSqlUpsertCommand(entities, sqlInsert);
                                            //Dont break if it fails
                                        }
                                    }
                                    catch (Exception exp)
                                    {
                                        LogError(exp, log);
                                    }
                                }
                            }
                            LogInfo("TimeSalaryPaymentExport Undo: Bulk - Done bulk update updatedTimePayrollTransactions and insert attesttransitionlog");

                            LogInfo("TimeSalaryPaymentExport Undo: Bulk - Done");
                        }
                        else
                        {
                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }

                    LogInfo("TimeSalaryPaymentExport Undo: Done " + actorCompanyId);
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.Success = false;
                    LogError(ex, log);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

            }

            return result;
        }

        private ActionResult ChangeTransactionStatusToExport(TransactionScope transaction, CompEntities entities, SalaryPaymentBaseAdapter adapter, TimeSalaryPaymentExport timeSalaryPayment, List<EmployeeTimePeriod> employeeTimePeriods, List<TimePayrollTransaction> timeTransactionsNotUseInPayroll, TimePeriod timePeriod, AttestState attestStateResultingPayroll, AttestTransition attestTransitionPayroll, DateTime? salarySpecificationPublishDate, int actorCompanyId, int userId, DateTime date)
        {
            ActionResult result = new ActionResult();
            bool useBulk = true;

            if (attestStateResultingPayroll == null)
                return new ActionResult(false);

            bool skipAttestTransition = SettingManager.GetBoolSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.SkipAttestTransition, 0, 0, 0);
            List<EmployeeTimePeriod> updatedEmployeeTimePeriods = new List<EmployeeTimePeriod>();
            List<TimeSalaryPaymentExportEmployee> createdTimeSalaryPaymentExportEmployees = new List<TimeSalaryPaymentExportEmployee>();
            List<TimePayrollTransaction> updatedTimePayrollTransactions = new List<TimePayrollTransaction>();
            List<AttestTransitionLog> createdAttestTransitionLogs = new List<AttestTransitionLog>();
            foreach (var employeeItem in adapter.EmployeeItems)
            {
                #region EmployeeTimePeriod

                var employeeTimePeriod = employeeTimePeriods.FirstOrDefault(x => x.EmployeeId == employeeItem.EmployeeId);
                if (employeeTimePeriod == null)
                    continue;

                employeeTimePeriod.Status = (int)SoeEmployeeTimePeriodStatus.Paid;
                employeeTimePeriod.SalarySpecificationPublishDate = salarySpecificationPublishDate;
                SetModifiedProperties(employeeTimePeriod);
                updatedEmployeeTimePeriods.Add(employeeTimePeriod);

                #endregion

                #region TimeSalaryPaymentExportEmployee

                var timeSalaryPaymentExportEmployee = new TimeSalaryPaymentExportEmployee
                {
                    TimeSalaryPaymentExport = timeSalaryPayment,
                    EmployeeId = employeeItem.EmployeeId,
                    PaymentDate = timePeriod.PaymentDate.Value,
                    DisbursementAccountNr = employeeItem.FormattedRecieverAccountNr,
                    NetAmount = employeeItem.GetNetAmount(),
                    NetAmountCurrency = adapter.CurrencyRate.HasValue ? employeeItem.GetNetAmount(adapter.CurrencyRate.Value) : (decimal?)null,
                    DisbursementMethod = employeeItem.DisbursementMethod,
                    UniquePaymentRowKey = employeeItem.UniquePaymentRowKey.EmptyToNull(),
                };
                if (!useBulk)
                    entities.TimeSalaryPaymentExportEmployee.AddObject(timeSalaryPaymentExportEmployee);

                SetCreatedProperties(timeSalaryPaymentExportEmployee);
                createdTimeSalaryPaymentExportEmployees.Add(timeSalaryPaymentExportEmployee);

                #endregion

                var employeeTransactions = employeeItem.Transactions;
                employeeTransactions.AddRange(timeTransactionsNotUseInPayroll.Where(x => x.EmployeeId == employeeItem.EmployeeId).ToList());
                foreach (TimePayrollTransaction timePayrollTransaction in employeeTransactions)
                {
                    #region AttestTransitionLog

                    AttestTransitionLog log = new AttestTransitionLog
                    {
                        UserId = parameterObject != null && parameterObject.SupportUserId.HasValue ? parameterObject.SupportUserId.Value : userId,
                        RecordId = timePayrollTransaction.TimePayrollTransactionId,
                        Date = date,
                        Entity = (int)TermGroup_AttestEntity.PayrollTime,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        AttestTransitionId = attestTransitionPayroll.AttestTransitionId,
                    };

                    createdAttestTransitionLogs.Add(log);
                    if (!useBulk)
                        entities.AttestTransitionLog.AddObject(log);

                    #endregion

                    #region TimePayrollTransaction

                    timePayrollTransaction.AttestStateId = attestStateResultingPayroll.AttestStateId;
                    timePayrollTransaction.TimeSalaryPaymentExportEmployee = timeSalaryPaymentExportEmployee;
                    SetModifiedProperties(timePayrollTransaction);
                    updatedTimePayrollTransactions.Add(timePayrollTransaction);

                    #endregion
                }
                if (!useBulk)
                {
                    result = SaveChanges(entities, transaction);
                    if (!result.Success)
                        return result;
                }
            }

            if (useBulk)
            {
                if (adapter.EmployeeItems.Count > 300)
                    LogInfo("TimeSalaryPaymentExport: Bulk - Begin bulk update updatedEmployeeTimePeriods");
                result = BulkUpdate(entities, updatedEmployeeTimePeriods, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryPaymentExport: Bulk - Done bulk update updatedEmployeeTimePeriods");

                if (adapter.EmployeeItems.Count > 300)
                    LogInfo("TimeSalaryPaymentExport: Bulk - Begin bulk insert createdTimeSalaryPaymentExportEmployees");
                result = BulkInsert(entities, createdTimeSalaryPaymentExportEmployees, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryPaymentExport: Bulk - Done bulk insert createdTimeSalaryPaymentExportEmployees");

                foreach (var timePayrollTransaction in updatedTimePayrollTransactions)
                    timePayrollTransaction.TimeSalaryPaymentExportEmployeeId = timePayrollTransaction.TimeSalaryPaymentExportEmployee.TimeSalaryPaymentExportEmployeeId;


                foreach (var groupedTransactions in updatedTimePayrollTransactions.GroupBy(g => g.TimeSalaryPaymentExportEmployeeId))
                {
                    var first = groupedTransactions.First();
                    List<int> timePayrollTransactionIds = groupedTransactions.Select(s => s.TimePayrollTransactionId).ToList();

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();
                    sqlParameters.Add(new SqlParameter("@modified", CalendarUtility.ToSqlFriendlyDateTime(first.Modified)));
                    sqlParameters.Add(new SqlParameter("@modifiedby", first.ModifiedBy));
                    sqlParameters.Add(new SqlParameter("@attestStateId", first.AttestStateId));
                    sqlParameters.Add(new SqlParameter("@timeSalaryPaymentExportEmployeeId", first.TimeSalaryPaymentExportEmployeeId));
                    string sqlUpdate = $"update timepayrollTransaction set modified = @modified, modifiedby = @modifiedby, attestStateId = @attestStateId,timeSalaryPaymentExportEmployeeId = @timeSalaryPaymentExportEmployeeId where timepayrollTransactionId in ({timePayrollTransactionIds.JoinToString<int>(",")})";

                    result = SqlCommandUtil.ExecuteSqlUpsertCommand(entities, sqlUpdate, sqlParameters);
                    if (!result.Success)
                        return result;


                    try
                    {

                        if (!skipAttestTransition)
                        {
                            string sqlInsert = SqlCommandUtil.CreateAttestTransitionLogInsertStatement(actorCompanyId, attestTransitionPayroll.AttestTransitionId, timePayrollTransactionIds, TermGroup_AttestEntity.PayrollTime, userId, first.Modified ?? DateTime.Now);
                            SqlCommandUtil.ExecuteSqlUpsertCommand(entities, sqlInsert);
                            //Dont break if it fails
                        }
                    }
                    catch (Exception exp)
                    {
                        LogError(exp, log);
                    }
                }

                LogInfo("TimeSalaryPaymentExport: Bulk - Done bulk insert logs and update transations");

            }

            return result;
        }

        private ActionResult ChangeTransactionStatusToExportNew(TransactionScope transaction, CompEntities entities, SalaryPaymentBaseAdapter adapter, TimeSalaryPaymentExport timeSalaryPayment, List<EmployeeTimePeriod> employeeTimePeriods, List<TimePayrollTransaction> timeTransactionsNotUseInPayroll, TimePeriod timePeriod, AttestState attestStateResultingPayroll, AttestTransition attestTransitionPayroll, DateTime? salarySpecificationPublishDate, int actorCompanyId, int userId, DateTime date)
        {
            ActionResult result = new ActionResult();
            bool useBulk = true;

            if (attestStateResultingPayroll == null)
                return new ActionResult(false);

            bool skipAttestTransition = SettingManager.GetBoolSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.SkipAttestTransition, 0, 0, 0);
            List<EmployeeTimePeriod> updatedEmployeeTimePeriods = new List<EmployeeTimePeriod>();
            List<TimeSalaryPaymentExportEmployee> createdTimeSalaryPaymentExportEmployees = new List<TimeSalaryPaymentExportEmployee>();
            List<TimePayrollTransaction> updatedTimePayrollTransactions = new List<TimePayrollTransaction>();
            List<AttestTransitionLog> createdAttestTransitionLogs = new List<AttestTransitionLog>();

            string createProcedureSql = @"
            CREATE PROCEDURE #temp_sp_UpdateTimePayrollTransactions
            (
                @modified datetime,
                @modifiedby nvarchar(max),
                @attestStateId int,
                @timeSalaryPaymentExportEmployeeId int,
                @timePayrollTransactionIds nvarchar(max)
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @sql NVARCHAR(MAX);

                SET @sql = N'UPDATE timepayrollTransaction SET modified = @modified, modifiedby = @modifiedby, attestStateId = @attestStateId, timeSalaryPaymentExportEmployeeId = @timeSalaryPaymentExportEmployeeId WHERE timepayrollTransactionId IN (' + @timePayrollTransactionIds + N')';

                EXEC sp_executesql @sql, N'@modified datetime, @modifiedby nvarchar(max), @attestStateId int, @timeSalaryPaymentExportEmployeeId int', @modified, @modifiedby, @attestStateId, @timeSalaryPaymentExportEmployeeId;
            END";


            SqlCommandUtil.ExecuteSqlUpsertCommand(entities, createProcedureSql);


            foreach (var employeeItem in adapter.EmployeeItems)
            {
                #region EmployeeTimePeriod

                var employeeTimePeriod = employeeTimePeriods.FirstOrDefault(x => x.EmployeeId == employeeItem.EmployeeId);
                if (employeeTimePeriod == null)
                    continue;

                employeeTimePeriod.Status = (int)SoeEmployeeTimePeriodStatus.Paid;
                employeeTimePeriod.SalarySpecificationPublishDate = salarySpecificationPublishDate;
                SetModifiedProperties(employeeTimePeriod);
                updatedEmployeeTimePeriods.Add(employeeTimePeriod);

                #endregion

                #region TimeSalaryPaymentExportEmployee

                var timeSalaryPaymentExportEmployee = new TimeSalaryPaymentExportEmployee
                {
                    TimeSalaryPaymentExport = timeSalaryPayment,
                    EmployeeId = employeeItem.EmployeeId,
                    PaymentDate = timePeriod.PaymentDate.Value,
                    DisbursementAccountNr = employeeItem.FormattedRecieverAccountNr,
                    NetAmount = employeeItem.GetNetAmount(),
                    NetAmountCurrency = adapter.CurrencyRate.HasValue ? employeeItem.GetNetAmount(adapter.CurrencyRate.Value) : (decimal?)null,
                    DisbursementMethod = employeeItem.DisbursementMethod,
                    UniquePaymentRowKey = employeeItem.UniquePaymentRowKey.EmptyToNull(),
                };
                if (!useBulk)
                    entities.TimeSalaryPaymentExportEmployee.AddObject(timeSalaryPaymentExportEmployee);

                SetCreatedProperties(timeSalaryPaymentExportEmployee);
                createdTimeSalaryPaymentExportEmployees.Add(timeSalaryPaymentExportEmployee);

                #endregion

                var employeeTransactions = employeeItem.Transactions;
                employeeTransactions.AddRange(timeTransactionsNotUseInPayroll.Where(x => x.EmployeeId == employeeItem.EmployeeId).ToList());
                foreach (TimePayrollTransaction timePayrollTransaction in employeeTransactions)
                {
                    #region AttestTransitionLog

                    AttestTransitionLog log = new AttestTransitionLog
                    {
                        UserId = parameterObject != null && parameterObject.SupportUserId.HasValue ? parameterObject.SupportUserId.Value : userId,
                        RecordId = timePayrollTransaction.TimePayrollTransactionId,
                        Date = date,
                        Entity = (int)TermGroup_AttestEntity.PayrollTime,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        AttestTransitionId = attestTransitionPayroll.AttestTransitionId,
                    };

                    createdAttestTransitionLogs.Add(log);
                    if (!useBulk)
                        entities.AttestTransitionLog.AddObject(log);

                    #endregion

                    #region TimePayrollTransaction

                    timePayrollTransaction.AttestStateId = attestStateResultingPayroll.AttestStateId;
                    timePayrollTransaction.TimeSalaryPaymentExportEmployee = timeSalaryPaymentExportEmployee;
                    SetModifiedProperties(timePayrollTransaction);
                    updatedTimePayrollTransactions.Add(timePayrollTransaction);

                    #endregion
                }
                if (!useBulk)
                {
                    result = SaveChanges(entities, transaction);
                    if (!result.Success)
                        return result;
                }
            }

            if (useBulk)
            {
                if (adapter.EmployeeItems.Count > 300)
                    LogInfo("TimeSalaryPaymentExport: Bulk - Begin bulk update updatedEmployeeTimePeriods");
                result = BulkUpdate(entities, updatedEmployeeTimePeriods, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryPaymentExport: Bulk - Done bulk update updatedEmployeeTimePeriods");

                if (adapter.EmployeeItems.Count > 300)
                    LogInfo("TimeSalaryPaymentExport: Bulk - Begin bulk insert createdTimeSalaryPaymentExportEmployees");
                result = BulkInsert(entities, createdTimeSalaryPaymentExportEmployees, transaction);
                if (!result.Success)
                    return result;
                LogInfo("TimeSalaryPaymentExport: Bulk - Done bulk insert createdTimeSalaryPaymentExportEmployees");

                foreach (var timePayrollTransaction in updatedTimePayrollTransactions)
                    timePayrollTransaction.TimeSalaryPaymentExportEmployeeId = timePayrollTransaction.TimeSalaryPaymentExportEmployee.TimeSalaryPaymentExportEmployeeId;


                foreach (var groupedTransactions in updatedTimePayrollTransactions.GroupBy(g => g.TimeSalaryPaymentExportEmployeeId))
                {
                    var first = groupedTransactions.First();
                    List<int> timePayrollTransactionIds = groupedTransactions.Select(s => s.TimePayrollTransactionId).ToList();

                    var table = new DataTable();
                    table.Columns.Add("Id", typeof(int));
                    foreach (var id in timePayrollTransactionIds)
                    {
                        table.Rows.Add(id);
                    }


                    List<SqlParameter> sqlParameters = new List<SqlParameter>();
                    sqlParameters.Add(new SqlParameter("@modified", dbType: SqlDbType.DateTime) { Value = first.Modified.Value });
                    sqlParameters.Add(new SqlParameter("@modifiedby", dbType: SqlDbType.NVarChar) { Value = first.ModifiedBy });
                    sqlParameters.Add(new SqlParameter("@attestStateId", dbType: SqlDbType.Int) { Value = first.AttestStateId });
                    sqlParameters.Add(new SqlParameter("@timeSalaryPaymentExportEmployeeId", dbType: SqlDbType.Int) { Value = first.TimeSalaryPaymentExportEmployeeId });
                    sqlParameters.Add(new SqlParameter("@timePayrollTransactionIds", dbType: SqlDbType.NVarChar) { Value = string.Join(",", timePayrollTransactionIds) });

                    result = SqlCommandUtil.ExecuteStoredProcedure(entities, "#temp_sp_UpdateTimePayrollTransactions", sqlParameters);


                    if (!result.Success)
                        return result;

                    try
                    {

                        if (!skipAttestTransition)
                        {
                            string sqlInsert = SqlCommandUtil.CreateAttestTransitionLogInsertStatement(actorCompanyId, attestTransitionPayroll.AttestTransitionId, timePayrollTransactionIds, TermGroup_AttestEntity.PayrollTime, userId, first.Modified ?? DateTime.Now);
                            SqlCommandUtil.ExecuteSqlUpsertCommand(entities, sqlInsert);
                            //Dont break if it fails
                        }
                    }
                    catch (Exception exp)
                    {
                        LogError(exp, log);
                    }
                }

                LogInfo("TimeSalaryPaymentExport: Bulk - Done bulk insert logs and update transations");

            }

            SqlCommandUtil.ExecuteSqlUpsertCommand(entities, "drop procedure #temp_sp_UpdateTimePayrollTransactions");

            return result;
        }

        public List<TimeSalaryPaymentExportGridDTO> GetTimeSalaryPaymentExportsForGrid(int actorCompanyId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeSalaryExport.NoTracking();
            return GetTimeSalaryPaymentExportsForGrid(entities, actorCompanyId, userId);
        }

        public TimeSalaryPaymentExport GetTimeSalaryPaymentExport(int timeSalaryPaymentExportId, int actorCompanyId, bool includeRows)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeSalaryPaymentExport.NoTracking();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeSalaryPaymentExportEmployee.NoTracking();
            return GetTimeSalaryPaymentExport(entities, timeSalaryPaymentExportId, actorCompanyId, includeRows);
        }

        public TimeSalaryPaymentExport GetTimeSalaryPaymentExport(CompEntities entities, int timeSalaryPaymentExportId, int actorCompanyId, bool includeRows)
        {
            if (includeRows)
            {
                return (from tspe in entities.TimeSalaryPaymentExport
                        .Include("TimeSalaryPaymentExportEmployee")
                        where tspe.State == (int)SoeEntityState.Active &&
                        tspe.TimeSalaryPaymentExportId == timeSalaryPaymentExportId &&
                        tspe.ActorCompanyId == actorCompanyId
                        select tspe).FirstOrDefault();
            }
            else
            {
                return (from tspe in entities.TimeSalaryPaymentExport
                        where tspe.State == (int)SoeEntityState.Active &&
                        tspe.TimeSalaryPaymentExportId == timeSalaryPaymentExportId &&
                        tspe.ActorCompanyId == actorCompanyId
                        select tspe).FirstOrDefault();
            }
        }

        public List<TimeSalaryPaymentExport> GetTimeSalaryPaymentExports(List<int> timePeriodIds, int actorCompanyId, bool includeRows)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeSalaryPaymentExport.NoTracking();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeSalaryPaymentExportEmployee.NoTracking();
            return GetTimeSalaryPaymentExports(entities, timePeriodIds, actorCompanyId, includeRows);
        }

        public List<TimeSalaryPaymentExport> GetTimeSalaryPaymentExports(CompEntities entities, List<int> timePeriodIds, int actorCompanyId, bool includeRows)
        {
            if (includeRows)
            {
                return (from tspe in entities.TimeSalaryPaymentExport
                        .Include("TimeSalaryPaymentExportEmployee")
                        where tspe.State == (int)SoeEntityState.Active &&
                        timePeriodIds.Contains(tspe.TimePeriodId) &&
                        tspe.ActorCompanyId == actorCompanyId &&
                        tspe.State == (int)SoeEntityState.Active
                        select tspe).ToList();
            }
            else
            {
                return (from tspe in entities.TimeSalaryPaymentExport
                        where tspe.State == (int)SoeEntityState.Active &&
                        timePeriodIds.Contains(tspe.TimePeriodId) &&
                        tspe.ActorCompanyId == actorCompanyId &&
                        tspe.State == (int)SoeEntityState.Active
                        select tspe).ToList();
            }
        }

        public List<EmployeeTimePeriod> GetEmployeeTimePeriods(CompEntities entities, int actorCompanyId, int timePeriodId, List<int> employeeIds)
        {
            return (from etp in entities.EmployeeTimePeriod
                    where etp.ActorCompanyId == actorCompanyId &&
                        etp.TimePeriodId == timePeriodId &&
                        employeeIds.Contains(etp.EmployeeId) &&
                        etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactions(CompEntities entities, int actorCompanyId, List<int> timeSalayPaymentExportEmployeeIds)
        {
            return (from tpt in entities.TimePayrollTransaction
                    where tpt.ActorCompanyId == actorCompanyId &&
                    tpt.TimeSalaryPaymentExportEmployeeId.HasValue &&
                     timeSalayPaymentExportEmployeeIds.Contains(tpt.TimeSalaryPaymentExportEmployeeId.Value) &&
                     tpt.State == (int)SoeEntityState.Active
                    select tpt).ToList();
        }

        private List<TimePayrollTransaction> GetPayrollTransactionsForSalaryPaymentExport(CompEntities entities, List<int> employeeIds, TimePeriod timePeriod, int actorCompanyId)
        {
            List<TimePayrollTransaction> transactions = null;

            if (timePeriod.ExtraPeriod)
            {
                transactions = (from p in entities.TimePayrollTransaction
                                where p.ActorCompanyId == actorCompanyId &&
                                employeeIds.Contains(p.EmployeeId) &&
                                p.State == (int)SoeEntityState.Active &&
                                (p.TimeBlock == null || p.TimeBlock.State == (int)SoeEntityState.Active) &&
                                p.TimePeriodId == timePeriod.TimePeriodId
                                select p).ToList();
            }
            else
            {
                DateTime startDate = timePeriod.StartDate.Date;
                DateTime stopDate = timePeriod.StopDate.Date;

                //transactions = (from p in entities.TimePayrollTransaction
                //                where p.ActorCompanyId == actorCompanyId &&
                //                employeeIds.Contains(p.EmployeeId) &&
                //                p.State == (int)SoeEntityState.Active &&
                //                (p.TimeBlock == null || p.TimeBlock.State == (int)SoeEntityState.Active) &&
                //                ((p.TimeBlockDate.Date >= startDate && p.TimeBlockDate.Date <= stopDate && !p.TimePeriodId.HasValue) ||
                //                (p.TimePeriodId.HasValue && p.TimePeriodId.Value == timePeriod.TimePeriodId))
                //                select p).ToList();

                transactions = (from p in entities.TimePayrollTransaction
                                where p.ActorCompanyId == actorCompanyId &&
                                employeeIds.Contains(p.EmployeeId) &&
                                p.State == (int)SoeEntityState.Active &&
                                (p.TimeBlock == null || p.TimeBlock.State == (int)SoeEntityState.Active) &&
                                p.TimeBlockDate.Date >= startDate && p.TimeBlockDate.Date <= stopDate && !p.TimePeriodId.HasValue
                                select p).ToList();

                transactions.AddRange((from p in entities.TimePayrollTransaction
                                       where p.ActorCompanyId == actorCompanyId &&
                                       employeeIds.Contains(p.EmployeeId) &&
                                       p.State == (int)SoeEntityState.Active &&
                                       (p.TimeBlock == null || p.TimeBlock.State == (int)SoeEntityState.Active) &&
                                       p.TimePeriodId.HasValue && p.TimePeriodId.Value == timePeriod.TimePeriodId
                                       select p).ToList());
            }

            return transactions.Where(x => x.TimeSalaryPaymentExportEmployeeId == null).ToList();
        }

        private List<TimeSalaryPaymentExportGridDTO> GetTimeSalaryPaymentExportsForGrid(CompEntities entities, int actorCompanyId, int userId)
        {
            List<TimeSalaryPaymentExportGridDTO> dtos = new List<TimeSalaryPaymentExportGridDTO>();

            List<AttestRole> currentAttestRoles = AttestManager.GetAttestRolesForUser(actorCompanyId, userId, DateTime.Today, SoeModule.Time);

            if (currentAttestRoles.Any(x => x.ShowAllCategories))
            {
                List<GenericType> timeSalaryPaymentExportTypes = base.GetTermGroupContent(TermGroup.TimeSalaryPaymentExportType);
                List<GenericType> disbursementMethods = base.GetTermGroupContent(TermGroup.EmployeeDisbursementMethod);
                List<TimePeriodHead> timePeriodHeads = TimePeriodManager.GetTimePeriodHeadsIncludingPeriods(actorCompanyId);
                List<TimePeriod> timePeriods = new List<TimePeriod>();
                timePeriodHeads.ForEach(x => timePeriods.AddRange(x.TimePeriod.Where(p => p.State == (int)SoeEntityState.Active)));
                List<Employee> employees = EmployeeManager.GetAllEmployees(actorCompanyId);

                var exports = (from tse in entities.TimeSalaryPaymentExport
                               where tse.State == (int)SoeEntityState.Active &&
                               tse.ActorCompanyId == actorCompanyId
                               orderby tse.ExportDate descending
                               select new
                               {
                                   tse.TimeSalaryPaymentExportId,
                                   tse.ActorCompanyId,
                                   tse.TimePeriodId,
                                   tse.ExportDate,
                                   tse.ExportType,
                                   tse.ExportFormat,
                                   tse.DebitDate,
                                   tse.PaymentDate,
                                   tse.UniqueMsgKey,
                                   tse.UniquePaymentKey,
                                   tse.CurrencyCode,
                                   tse.CurrencyDate,
                                   tse.CurrencyRate
                               }).ToList();

                List<int> exportsId = exports.Select(x => x.TimeSalaryPaymentExportId).ToList();
                var exportEmployees = (from tsee in entities.TimeSalaryPaymentExportEmployee
                                       where exportsId.Contains(tsee.TimeSalaryPaymentExportId)
                                       select tsee).ToList();

                foreach (var export in exports)
                {
                    List<TimeSalaryPaymentExportEmployee> currentExportEmployees = exportEmployees.Where(x => x.TimeSalaryPaymentExportId == export.TimeSalaryPaymentExportId && x.State == (int)SoeEntityState.Active).ToList();

                    String typeName = string.Empty;
                    var term = timeSalaryPaymentExportTypes.FirstOrDefault(x => x.Id == export.ExportType);
                    if (term != null)
                        typeName = term.Name;

                    DateTime? salarySpecificationPublishDate = null;
                    var timeSalaryPaymentExportEmployee = currentExportEmployees.FirstOrDefault();
                    if (timeSalaryPaymentExportEmployee != null)
                    {
                        var employeeTimePeriod = TimePeriodManager.GetEmployeeTimePeriod(timeSalaryPaymentExportEmployee.EmployeeId, export.TimePeriodId, actorCompanyId);
                        if (employeeTimePeriod != null)
                            salarySpecificationPublishDate = employeeTimePeriod.SalarySpecificationPublishDate;
                    }

                    TimeSalaryPaymentExportGridDTO dto = new TimeSalaryPaymentExportGridDTO()
                    {
                        TimeSalaryPaymentExportId = export.TimeSalaryPaymentExportId,
                        ExportDate = export.ExportDate,
                        ExportType = (TermGroup_TimeSalaryPaymentExportType)export.ExportType,
                        TypeName = typeName,
                        SalarySpecificationPublishDate = salarySpecificationPublishDate,
                        Employees = new List<TimeSalaryPaymentExportEmployeeDTO>(),
                        DebitDate = export.DebitDate,
                        MsgKey = export.UniqueMsgKey,
                        PaymentKey = export.UniquePaymentKey,
                        CurrencyCode = export.CurrencyCode,
                        CurrencyDate = export.CurrencyDate,
                        CurrencyRate = export.CurrencyRate,
                    };

                    TimePeriod timePeriod = timePeriods.FirstOrDefault(x => x.TimePeriodId == export.TimePeriodId);
                    if (timePeriod != null)
                    {
                        dto.TimePeriodName = timePeriod.Name;
                        if (export.PaymentDate.HasValue)
                            dto.PaymentDate = export.PaymentDate.Value;
                        else
                            dto.PaymentDate = timePeriod.PaymentDate.HasValue ? timePeriod.PaymentDate.Value : CalendarUtility.DATETIME_DEFAULT;

                        if (timePeriod.PayrollStartDate.HasValue && timePeriod.PayrollStopDate.HasValue)
                            dto.PayrollDateInterval = timePeriod.PayrollStartDate.Value.ToShortDateString() + " - " + timePeriod.PayrollStopDate.Value.ToShortDateString();

                        if (timePeriod.TimePeriodHead != null)
                            dto.TimePeriodHeadName = timePeriod.TimePeriodHead.Name;
                    }

                    foreach (var currentExportEmployee in currentExportEmployees)
                    {
                        var employee = employees.FirstOrDefault(x => x.EmployeeId == currentExportEmployee.EmployeeId);
                        var employeeDto = currentExportEmployee.ToDTO(true, employee);
                        if (employeeDto != null)
                        {
                            var disbursementMethodTerm = disbursementMethods.FirstOrDefault(x => x.Id == currentExportEmployee.DisbursementMethod);
                            if (disbursementMethodTerm != null)
                                employeeDto.DisbursementMethodName = disbursementMethodTerm.Name;

                            dto.Employees.Add(employeeDto);
                        }
                    }

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        private List<TimeSalaryPaymentExport> GetTimeSalaryPaymentExports(int actorCompanyId, int timePeriodId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeSalaryPaymentExport.NoTracking();
            entitiesReadOnly.TimeSalaryPaymentExport.NoTracking();
            List<TimeSalaryPaymentExport> result = (from tse in entitiesReadOnly.TimeSalaryPaymentExport
                                                    .Include("TimeSalaryPaymentExportEmployee")
                                                    where tse.TimePeriodId == timePeriodId &&
                                                    tse.ActorCompanyId == actorCompanyId &&
                                                    tse.State == (int)SoeEntityState.Active
                                                    select tse).ToList();
            return result;
        }

        public string GetSalaryPaymentExportWarnings(int actorCompanyId, List<int> employeeIds, int timePeriodId)
        {
            string message = string.Empty;
            try
            {
                var employees = EmployeeManager.GetAllEmployeesByIds(actorCompanyId, employeeIds);

                #region Disbursement method

                var employeesWithDisbursementMethodCash = employees.Where(x => x.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_CashDeposit).ToList();
                if (employeesWithDisbursementMethodCash.Any())
                {
                    message += GetText(8710, "Följande anställda:") + " ";
                    int count = 0;
                    foreach (var employee in employeesWithDisbursementMethodCash)
                    {
                        message += (count > 0 ? ", " : "") + employee.Name + " (" + employee.EmployeeNr + ")";
                        count++;
                    }
                    message += " " + GetText(8711, "har utbetalningssätt satt till kontant.") + "\n" + "\n";
                }

                var employeesWithDisbursementMethodUnknown = employees.Where(x => x.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.Unknown).ToList();
                if (employeesWithDisbursementMethodUnknown.Any())
                {
                    message += GetText(8710, "Följande anställda:") + " ";
                    int count = 0;
                    foreach (var employee in employeesWithDisbursementMethodUnknown)
                    {
                        message += (count > 0 ? ", " : "") + employee.Name + " (" + employee.EmployeeNr + ")";
                        count++;
                    }
                    message += " " + GetText(8712, "har utbetalningssätt satt till okänd.") + "\n" + "\n";
                }

                #endregion

                #region Open periods

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var openEmployeeTimePeriods = GetEmployeeTimePeriods(entitiesReadOnly, actorCompanyId, timePeriodId, employeeIds).Where(x => x.Status == (int)SoeEmployeeTimePeriodStatus.Open).ToList();
                if (openEmployeeTimePeriods.Any())
                {

                    message += GetText(8708, "Vald period för") + " ";
                    int count = 0;
                    foreach (var openEmployeeTimePeriod in openEmployeeTimePeriods)
                    {
                        var employee = employees.FirstOrDefault(x => x.EmployeeId == openEmployeeTimePeriod.EmployeeId);
                        if (employee != null)
                        {
                            message += (count > 0 ? ", " : "") + employee.Name + " (" + employee.EmployeeNr + ")";
                            count++;
                        }
                    }

                    message += " " + GetText(8709, "är inte låst.") + "\n" + "\n";
                }

                #endregion

                #region Already exported

                var salaryPaymentExports = GetTimeSalaryPaymentExports(actorCompanyId, timePeriodId);
                if (salaryPaymentExports.Any())
                {
                    List<Employee> exportedEmployees = new List<Employee>();
                    foreach (var employee in employees)
                    {
                        foreach (var salaryExport in salaryPaymentExports)
                        {
                            if (exportedEmployees.Any(x => x.EmployeeId == employee.EmployeeId))
                                continue;

                            foreach (var exportRow in salaryExport.TimeSalaryPaymentExportEmployee)
                            {
                                if (exportRow.EmployeeId == employee.EmployeeId)
                                {
                                    exportedEmployees.Add(employee);
                                    break;
                                }
                            }
                        }
                    }

                    if (exportedEmployees.Count > 0)
                    {
                        message += GetText(8534, "I vald period för") + " ";
                        int count = 0;
                        foreach (var exportedEmployee in exportedEmployees)
                        {
                            message += (count > 0 ? ", " : "") + exportedEmployee.Name + " (" + exportedEmployee.EmployeeNr + ")";
                            count++;
                        }

                        message += " " + GetText(8535, "är lönefil redan skapad.") + "\n";

                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(message))
                    message += "\n" + GetText(8494, "Vill du fortsätta?");
            }
            catch (Exception)
            {
                message = string.Empty;
            }

            return message;
        }

        #endregion

        #region EmployeeTimePeriod

        public ActionResult SetSalarySpecificationPublishDate(int actorCompanyId, int timeSalaryPaymentExportId, DateTime? salarySpecificationPublishDate, int userId, int roleId)
        {
            ActionResult result = new ActionResult();
            List<int> employeeIds = new List<int>();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    var export = GetTimeSalaryPaymentExport(timeSalaryPaymentExportId, actorCompanyId, false);
                    if (export == null)
                        return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8163, "Exporterat lön kunde inte hittas"));

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Init

                        //Open connection
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #endregion

                        employeeIds = (from tspe in entities.TimeSalaryPaymentExportEmployee
                                       where tspe.State == (int)SoeEntityState.Active &&
                                       tspe.TimeSalaryPaymentExportId == timeSalaryPaymentExportId
                                       select tspe).Select(x => x.EmployeeId).ToList();

                        List<EmployeeTimePeriod> employeeTimePeriods = GetEmployeeTimePeriods(entities, actorCompanyId, export.TimePeriodId, employeeIds);
                        foreach (var employeeTimePeriod in employeeTimePeriods)
                        {
                            employeeTimePeriod.SalarySpecificationPublishDate = salarySpecificationPublishDate;
                            SetModifiedProperties(employeeTimePeriod);
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.Success = false;
                    LogError(ex, log);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            if (result.Success && salarySpecificationPublishDate.HasValue && !employeeIds.IsNullOrEmpty())
            {
                List<int> userIds = UserManager.GetUserIdsByEmployeesIds(actorCompanyId, employeeIds);
                CommunicationManager.SendXEMailPayrollSlipPublishedWhenCreatingBankfile(actorCompanyId, userIds, salarySpecificationPublishDate, userId, roleId);
            }

            return result;
        }

        public DateTime? GetSalarySpecificationPublishDate(int actorCompanyId, int timeSalaryPaymentExportId, int timePeriodId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var timeSalaryPaymentExportEmployee = (from tspe in entitiesReadOnly.TimeSalaryPaymentExportEmployee
                                                   where tspe.State == (int)SoeEntityState.Active &&
                                                         tspe.TimeSalaryPaymentExportId == timeSalaryPaymentExportId
                                                   select tspe).FirstOrDefault();

            if (timeSalaryPaymentExportEmployee == null)
                return null;

            var employeeTimePeriod = (from etp in entitiesReadOnly.EmployeeTimePeriod
                                      where etp.ActorCompanyId == actorCompanyId &&
                                          etp.TimePeriodId == timePeriodId &&
                                          etp.EmployeeId == timeSalaryPaymentExportEmployee.EmployeeId &&
                                          etp.State == (int)SoeEntityState.Active
                                      select etp).FirstOrDefault();

            return employeeTimePeriod?.SalarySpecificationPublishDate;
        }
        #endregion

        public Dictionary<int, string> GetPaymentInformationViewsDictForISO20022(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            var items = PaymentManager.GetPaymentInformationViews(actorCompanyId);
            foreach (var item in items)
            {
                if (item.SysPaymentTypeId == (int)TermGroup_SysPaymentType.Bank || item.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
                    dict.Add(item.PaymentInformationRowId, item.Name + " " + item.PaymentNr);
            }

            return dict;
        }

        #endregion
    }
}

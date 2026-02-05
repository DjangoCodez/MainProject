using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class ProjectManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ProjectManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region CaseProject

        public CaseProject GetCaseProject(CompEntities entities, int projectId, bool includeNotes)
        {
            IQueryable<CaseProject> query = entities.Project.OfType<CaseProject>();

            if (includeNotes)
                query = query.Include("CaseProjectNote");

            query = query.Where(b => b.ProjectId == projectId);

            return query.FirstOrDefault();
        }

        #endregion

        #region Project

        public List<EmployeeTimeCodeDTO> GetEmployeesForProjectWithTimeCode(int actorCompanyId, int userId, int roleId, bool addEmptyRow, bool getHidden, bool addNoReplacementEmployee, int? includeEmployeeId, DateTime? fromDateString, DateTime? toDateString, List<int> employeeCategories)
        {
            var employees = EmployeeManager.GetEmployeesForProjectWithTimeCode(actorCompanyId, userId, roleId, addEmptyRow, getHidden, addNoReplacementEmployee, includeEmployeeId, fromDateString, toDateString, null);
            if (!employeeCategories.IsNullOrEmpty())
            {
                var employeeIds = EmployeeManager.GetEmployeeIdsByCategoryIds(actorCompanyId, employeeCategories);
                return employees.Where(e => employeeIds.Contains(e.EmployeeId)).ToList();
            }

            return employees;
        }


        public List<Project> GetProjectsForInvoice(CompEntities entities, int invoiceId)
        {
            List<Project> projects = new List<Project>();

            var invoice = (from i in entities.Invoice
                               .Include("Origin")
                               .Include("Project")
                           where i.InvoiceId == invoiceId &&
                           i.State == (int)SoeEntityState.Active
                           select i).OfType<CustomerInvoice>().FirstOrDefault();

            if (invoice != null)
            {
                //Add Invoice's Project
                if (invoice.Project != null && invoice.Project.State == (int)SoeEntityState.Active)
                    projects.Add(invoice.Project);

                if (invoice.Origin != null && invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                {
                    #region InvoiceTracing (to find Order's)

                    var invoiceTraceViews = InvoiceManager.GetInvoiceTraceViews(invoice.InvoiceId, 0, isOrder: true);
                    foreach (var invoiceTraceView in invoiceTraceViews.Where(i => i.IsOrder))
                    {
                        var orderProject = (from i in entities.Invoice.OfType<CustomerInvoice>()
                                            where i.InvoiceId == invoiceTraceView.OrderId &&
                                            i.State == (int)SoeEntityState.Active &&
                                            i.PrintTimeReport == true &&
                                            i.Project != null &&
                                            i.Project.State == (int)SoeEntityState.Active
                                            select i.Project).FirstOrDefault();

                        //Add Order's Project
                        if (orderProject != null && orderProject.State == (int)SoeEntityState.Active && !projects.Any(x => x.ProjectId == orderProject.ProjectId))
                            projects.Add(orderProject);
                    }

                    #endregion
                }
            }

            return projects;
        }

        public Project GetProject(int projectId, bool setStatusName, bool setCustomerName = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            Project project = GetProject(entitiesReadOnly, projectId);

            if (project != null)
            {
                if (setStatusName)
                {
                    // Set StatusName extension
                    List<GenericType> statuses = base.GetTermGroupContent(TermGroup.ProjectStatus, skipUnknown: true);
                    GenericType status = statuses.FirstOrDefault(s => s.Id == project.Status);
                    project.StatusName = status != null ? status.Name : string.Empty;
                }

                if (setCustomerName && !project.CustomerReference.IsLoaded)
                    project.CustomerReference.Load();
            }

            return project;
        }

        public Project GetProject(CompEntities entities, int projectId, bool includeChildProjects = false)
        {
            if (includeChildProjects)
            {
                return (from p in entities.Project
                        .Include("ChildProjects")
                        where p.ProjectId == projectId && p.ActorCompanyId == ActorCompanyId
                        select p).FirstOrDefault();
            }
            else
            {
                return (from p in entities.Project
                        where p.ProjectId == projectId && p.ActorCompanyId == ActorCompanyId
                        select p).FirstOrDefault();
            }
        }

        public Project GetProjectByNumber(CompEntities entities, int actorCompanyId, string projectNr)
        {
            return (from p in entities.Project
                    where p.Number == projectNr &&
                    p.ActorCompanyId == actorCompanyId
                    select p).FirstOrDefault();
        }

        public int GetProjectId(int invoiceId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetProjectId(entities, invoiceId);
        }

        public int GetProjectId(CompEntities entities, int invoiceId)
        {
            int? projectId = (from i in entities.Invoice
                              where i.InvoiceId == invoiceId
                              select i.ProjectId).FirstOrDefault();

            if (!projectId.HasValue)
                projectId = -1;

            return projectId.Value;
        }

        public ProjectTinyDTO GetProjectSmall(int actorCompanyId, int projectId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetProjectSmall(entities, actorCompanyId, projectId);
        }

        public ProjectTinyDTO GetProjectSmall(CompEntities entities, int actorCompanyId, int projectId)
        {
            string cacheKey = $"GetProjectSmall#projectId{projectId}";
            ProjectTinyDTO projectTinyDTO = BusinessMemoryCache<ProjectTinyDTO>.Get(cacheKey);

            if (projectTinyDTO == null)
            {
                projectTinyDTO = (from p in entities.Project
                                  where p.ActorCompanyId == actorCompanyId &&
                                  p.ProjectId == projectId
                                  select new ProjectTinyDTO { ProjectId = p.ProjectId, Number = p.Number, Name = p.Name, Status = (TermGroup_ProjectStatus)p.Status, ParentProjectId = p.ParentProjectId, UseAccounting = p.UseAccounting }).FirstOrDefault();

                if (projectTinyDTO != null)
                {
                    BusinessMemoryCache<ProjectTinyDTO>.Set(cacheKey, projectTinyDTO, 30);
                }
            }

            return projectTinyDTO;
        }

        public ProjectTinyDTO GetProjectSmall(CompEntities entities, int projectId)
        {
            string cacheKey = $"GetProjectSmall#projectId{projectId}";
            ProjectTinyDTO projectTinyDTO = BusinessMemoryCache<ProjectTinyDTO>.Get(cacheKey);

            if (projectTinyDTO == null)
            {
                projectTinyDTO = (from p in entities.Project
                                  where
                                  p.ProjectId == projectId
                                  select new ProjectTinyDTO { ProjectId = p.ProjectId, Number = p.Number, Name = p.Name, Status = (TermGroup_ProjectStatus)p.Status, ParentProjectId = p.ParentProjectId, UseAccounting = p.UseAccounting }).FirstOrDefault();

                if (projectTinyDTO != null)
                {
                    BusinessMemoryCache<ProjectTinyDTO>.Set(cacheKey, projectTinyDTO, 30);
                }
            }

            return projectTinyDTO;
        }

        public string GetLastProjectNumber(CompEntities entities, int actorCompanyId)
        {
            Project project = (from p in entities.Project
                               where p.ActorCompanyId == actorCompanyId
                               select p).OrderByDescending(s => s.ProjectId).FirstOrDefault();

            return project != null ? project.Number : "";
        }

        /// <summary>
        /// Returns all projects ids for a project family, including the main project id.
        /// </summary>
        public List<int> GetProjectIdsFromMain(CompEntities entities, int actorCompanyId, int mainProjectId)
        {
            return entities.GetProjectHierarchy(mainProjectId, actorCompanyId)
                .Where(p => p.ProjectId != null)
                .Select(p => p.ProjectId.Value)
                .ToList();
        }

        //[Obsolete("Use GetProjectIdsFromMain instead")]
        public List<int> GetChildProjectsIds(CompEntities entities, int actorCompanyId, int projectId)
        {
            var projectCentral = new ProjectCentralManager(parameterObject);

            List<Project> allProjects = projectCentral.GetProjectsForProjectCentralStatus(entities, new List<int>() { projectId }, actorCompanyId);
            if (!allProjects.IsNullOrEmpty())
            {
                #region Child projects

                // Get child projects recursively
                List<Project> currentLevelProjects = new List<Project>();
                currentLevelProjects.AddRange(allProjects);

                // Loop until no further child projects are found
                while (true)
                {
                    List<Project> childProjects = projectCentral.GetProjectsForProjectCentralStatus(currentLevelProjects.FirstOrDefault().ChildProjects.Select(p => p.ProjectId).ToList(), actorCompanyId);
                    if (childProjects.Count == 0)
                        break;

                    // Add found projects to current level projects to be used for next level search
                    currentLevelProjects.Clear();
                    currentLevelProjects.AddRange(childProjects);

                    // Add found projects to all projects list
                    allProjects.AddRange(childProjects);
                }

                #endregion  
            }

            var idList = allProjects.Select(s => s.ProjectId).Distinct();
            return idList.ToList();
        }

        public IEnumerable<string> GetAllProjectNumbers(CompEntities entities, int actorCompanyId)
        {
            return (from p in entities.Project
                    where p.ActorCompanyId == actorCompanyId
                    select p.Number);

        }

        public string GetProjectNumberCheckExisting(IEnumerable<string> numbers, string orderNr, int actorCompanyId)
        {
            bool isNumeric = Int32.TryParse(orderNr, out int number);
            if (numbers.Contains(orderNr))
            {
                if (isNumeric)
                {
                    number = number + 1;
                    return GetProjectNumberCheckExisting(numbers, number.ToString(), actorCompanyId);
                }
                else
                {
                    orderNr = orderNr + "1";
                    return GetProjectNumberCheckExisting(numbers, orderNr, actorCompanyId);
                }
            }
            else
            {
                return orderNr;
            }
        }

        public ActionResult DeleteProject(int projectId, int actorCompanyId)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = DeleteProject(entities, transaction, projectId, actorCompanyId);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
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

                return result;
            }
        }

        public ActionResult DeleteProject(CompEntities entities, TransactionScope transaction, int projectId, int actorCompanyId, int invoiceId = 0)
        {
            var result = new ActionResult();
            Project originalProject = GetProject(entities, projectId);

            if (originalProject == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Project");

            // Check if project is connected to any orders/invoices
            var hasInvoices = invoiceId > 0 ? (from i in entities.Invoice
                                               where i.ProjectId == projectId &&
                                               i.State == (int)SoeEntityState.Active &&
                                               i.InvoiceId != invoiceId
                                               select i).Any()
                               :
                               (from i in entities.Invoice
                                where i.ProjectId == projectId &&
                                i.State == (int)SoeEntityState.Active
                                select i).Any();
            if (hasInvoices)
                return new ActionResult(false, 0, GetText(7461, "Projektet används och kan ej tas bort"));


            // Check if any account dim is linked to project
            AccountDim dim = AccountManager.GetProjectAccountDim(entities, actorCompanyId);
            if (dim != null)
            {
                // Find account linked to the project
                List<Account> accounts = AccountManager.GetAccountsByAccountNr(entities, originalProject.Number, dim.AccountDimId, true);
                if (accounts.Count == 1)
                    result = ChangeEntityState(entities, accounts.First(), SoeEntityState.Deleted, true);
            }

            // Set the Project to deleted
            if (result.Success)
                result = ChangeEntityState(entities, originalProject, SoeEntityState.Deleted, true);

            return result;
        }

        public ActionResult ChangeProjectStatus(List<int> projects, int newStatus)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                foreach (int project in projects)
                {
                    // Update project
                    Project originalProject = GetProject(entities, project);
                    if (originalProject == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Project");

                    originalProject.Status = newStatus;
                    SetModifiedProperties(originalProject);
                }

                result = SaveChanges(entities);
            }

            return result;
        }

        public ActionResult ChangeStopDateForHiddenProject(CompEntities entities, CustomerInvoice order)
        {
            if (order.ProjectId.GetValueOrDefault() != 0 && order.Origin != null)
            {
                var clear = !(order.Origin.Status == (int)SoeOriginStatus.OrderFullyInvoice || order.Origin.Status == (int)SoeOriginStatus.OrderClosed);
                var project = GetProject(entities, order.ProjectId.Value);
                if (project == null)
                    return new ActionResult("Missing project!");

                if (project.Status == (int)TermGroup_ProjectStatus.Hidden)
                {
                    project.StopDate = clear ? (DateTime?)null : DateTime.Today;
                }
            }
            return new ActionResult(true);
        }

        #endregion

        #region Project - SupplierInvoice

        public List<SupplierInvoiceProjectRowDTO> GetSupplierInvoiceProjectRows(int invoiceId, int actorCompanyId, bool loadOrders = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSupplierInvoiceProjectRows(entities, invoiceId, actorCompanyId, false);
        }


        public List<SupplierInvoiceProjectRowDTO> GetSupplierInvoiceProjectRows(CompEntities entities, int invoiceId, int actorCompanyId, bool loadOrders = false)
        {
            List<SupplierInvoiceProjectRowDTO> dtos = new List<SupplierInvoiceProjectRowDTO>();

            List<TimeCodeTransaction> timeCodeTransactions = (from tct in entities.TimeCodeTransaction
                                                              .Include("TimeInvoiceTransaction.Employee.ContactPerson")
                                                              .Include("TimeInvoiceTransaction.TimeBlockDate")
                                                              //.Include("Project.Invoice")
                                                              .Include("TimeCode")
                                                              where (tct.SupplierInvoiceId.HasValue && tct.SupplierInvoiceId == invoiceId) &&
                                                              tct.ProjectId != null &&
                                                              tct.State == (int)SoeEntityState.Active
                                                              select tct).ToList();

            var projectIds = timeCodeTransactions.Select(t => t.ProjectId.Value).Distinct();
            List<Project> projectsForTransactions = (from p in entities.Project
                                                     .Include("Invoice")
                                                     where projectIds.Contains(p.ProjectId)
                                                     select p).ToList();

            foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactions)
            {
                #region Prereq

                /*if (timeCodeTransaction.State != (int)SoeEntityState.Active)
                    continue;*/

                //TimeInvoiceTransaction (mandatory)
                TimeInvoiceTransaction timeInvoiceTransaction = timeCodeTransaction.TimeInvoiceTransaction.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);

                //Project (mandatory)
                /*if (timeCodeTransaction.Project == null)
                    continue;

                //TimeCode (mandatory)
                if (timeCodeTransaction.TimeCode == null)
                    continue;*/

                IEnumerable<Invoice> orders = null;
                var project = projectsForTransactions.FirstOrDefault(p => p.ProjectId == timeCodeTransaction.ProjectId.Value);
                if (project != null)
                    orders = project.Invoice.Where(i => i.IsOrder);

                #region SupplierInvoiceProjectRowDTO

                SupplierInvoiceProjectRowDTO dto = new SupplierInvoiceProjectRowDTO()
                {
                    //General
                    State = (int)SoeEntityState.Active,
                    //TimeCodeTransaction
                    TimeCodeTransactionId = timeCodeTransaction.TimeCodeTransactionId,
                    Amount = timeCodeTransaction.Amount.HasValue ? timeCodeTransaction.Amount.Value : 0,
                    AmountCurrency = timeCodeTransaction.Amount.HasValue ? timeCodeTransaction.AmountCurrency.Value : 0,
                    AmountLedgerCurrency = timeCodeTransaction.Amount.HasValue ? timeCodeTransaction.AmountLedgerCurrency.Value : 0,
                    AmountEntCurrency = timeCodeTransaction.Amount.HasValue ? timeCodeTransaction.AmountEntCurrency.Value : 0,
                    //TimeInvoiceTransaction
                    TimeInvoiceTransactionId = timeInvoiceTransaction != null ? timeInvoiceTransaction.TimeInvoiceTransactionId : 0,
                    //SupplierInvoice
                    SupplierInvoiceId = timeCodeTransaction.SupplierInvoiceId.Value,
                    //Orders
                    OrderNr = orders?.Select(o => o.InvoiceNr).JoinToString(", "),
                    OrderIds = orders?.Select(o => o.InvoiceId).ToList(),
                    //TimeCode
                    TimeCodeId = timeCodeTransaction.TimeCode.TimeCodeId,
                    TimeCodeCode = timeCodeTransaction.TimeCode.Code,
                    TimeCodeName = timeCodeTransaction.TimeCode.Name,
                    TimeCodeDescription = String.Format("{0} {1}", timeCodeTransaction.TimeCode.Code, timeCodeTransaction.TimeCode.Name),
                    //Employee
                    EmployeeId = timeInvoiceTransaction != null ? timeInvoiceTransaction.EmployeeId : 0,
                    EmployeeNr = timeInvoiceTransaction != null && timeInvoiceTransaction.Employee != null ? timeInvoiceTransaction.Employee.EmployeeNr : String.Empty,
                    EmployeeName = timeInvoiceTransaction != null && timeInvoiceTransaction.Employee != null ? timeInvoiceTransaction.Employee.Name : String.Empty,
                    EmployeeDescription = timeInvoiceTransaction != null && timeInvoiceTransaction.Employee != null ? String.Format("{0} {1}", timeInvoiceTransaction.Employee.EmployeeNr, timeInvoiceTransaction.Employee.Name) : String.Empty,
                    //TimeBlockDate
                    TimeBlockDateId = timeInvoiceTransaction != null && timeInvoiceTransaction.TimeBlockDateId.HasValue ? timeInvoiceTransaction.TimeBlockDateId.Value : 0,
                    Date = timeInvoiceTransaction != null && timeInvoiceTransaction.TimeBlockDate != null ? timeInvoiceTransaction.TimeBlockDate.Date : (DateTime?)null,
                    ChargeCostToProject = timeCodeTransaction.DoNotChargeProject != null ? !(bool)timeCodeTransaction.DoNotChargeProject : true,
                    IncludeSupplierInvoiceImage = timeCodeTransaction.IncludeSupplierInvoiceImage,
                };

                if (project != null)
                {
                    dto.ProjectId = project.ProjectId;
                    dto.ProjectNr = project.Number;
                    dto.ProjectName = project.Number + " " + project.Name;
                    dto.ProjectDescription = String.Format("{0} {1}", project.Number, project.Name);
                }

                if (timeCodeTransaction.CustomerInvoiceId != null && timeCodeTransaction.CustomerInvoiceId != 0)
                {
                    Invoice invoice = InvoiceManager.GetCustomerInvoice(entities, (int)timeCodeTransaction.CustomerInvoiceId, loadActor: true);
                    if (invoice != null)
                    {
                        dto.CustomerInvoiceId = (int)timeCodeTransaction.CustomerInvoiceId;
                        dto.CustomerInvoiceCustomerName = invoice.Actor.Customer.CustomerNr + " " + invoice.Actor.Customer.Name;
                        dto.CustomerInvoiceNumberName = invoice.InvoiceNr + " " + invoice.Actor.Customer.Name;
                        dto.CustomerInvoiceNr = invoice.InvoiceNr;
                    }
                    else
                        dto.CustomerInvoiceId = 0;
                }
                else
                    dto.CustomerInvoiceId = 0;

                dtos.Add(dto);

                #endregion

                #endregion
            }

            return dtos;
        }
        public SupplierInvoiceCostTransferDTO GetSupplierInvoiceProjectRows(CompEntities entities, int timeCodeTransactionId)
        {
            entities.Connection.Open();
            TimeCodeTransaction timeCodeTransaction = (from tct in entities.TimeCodeTransaction
                                                              .Include("TimeInvoiceTransaction.Employee.ContactPerson")
                                                              .Include("TimeInvoiceTransaction.TimeBlockDate")
                                                              //.Include("Project.Invoice")
                                                              .Include("TimeCode")
                                                       where tct.TimeCodeTransactionId == timeCodeTransactionId &&
                                                       tct.ProjectId != null &&
                                                       tct.State == (int)SoeEntityState.Active
                                                       select tct).FirstOrDefault();

            if (timeCodeTransaction == null)
            {
                return new SupplierInvoiceCostTransferDTO();
            }
            TimeInvoiceTransaction timeInvoiceTransaction = timeCodeTransaction.TimeInvoiceTransaction.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);

            Project project = (from p in entities.Project
                                                     .Include("Invoice")
                               where p.ProjectId == timeCodeTransaction.ProjectId
                               select p).FirstOrDefault();

            /*if (timeCodeTransaction.State != (int)SoeEntityState.Active)
                continue;*/

            //TimeInvoiceTransaction (mandatory)


            SupplierInvoiceCostTransferDTO dto = new SupplierInvoiceCostTransferDTO()
            {
                //General
                State = (int)SoeEntityState.Active,
                //TimeCodeTransaction
                Type = SupplierInvoiceCostLinkType.ProjectRow,
                RecordId = timeInvoiceTransaction.TimeCodeTransactionId.Value,
                TimeCodeTransactionId = timeCodeTransaction.TimeCodeTransactionId,
                Amount = timeCodeTransaction.Amount.HasValue ? timeCodeTransaction.Amount.Value : 0,
                AmountCurrency = timeCodeTransaction.Amount.HasValue ? timeCodeTransaction.AmountCurrency.Value : 0,
                AmountLedgerCurrency = timeCodeTransaction.Amount.HasValue ? timeCodeTransaction.AmountLedgerCurrency.Value : 0,
                AmountEntCurrency = timeCodeTransaction.Amount.HasValue ? timeCodeTransaction.AmountEntCurrency.Value : 0,
                SumAmount = timeCodeTransaction.Amount.HasValue ? timeCodeTransaction.Amount.Value : 0,
                //TimeInvoiceTransaction
                TimeInvoiceTransactionId = timeInvoiceTransaction != null ? timeInvoiceTransaction.TimeInvoiceTransactionId : 0,
                //SupplierInvoice
                SupplierInvoiceId = timeCodeTransaction.SupplierInvoiceId.Value,
                //TimeCode
                TimeCodeId = timeCodeTransaction.TimeCode.TimeCodeId,
                TimeCodeCode = timeCodeTransaction.TimeCode.Code,
                TimeCodeName = timeCodeTransaction.TimeCode.Name,
                TimeCodeDescription = String.Format("{0} {1}", timeCodeTransaction.TimeCode.Code, timeCodeTransaction.TimeCode.Name),
                //Employee
                EmployeeId = timeInvoiceTransaction != null && timeInvoiceTransaction.EmployeeId != null ? timeInvoiceTransaction.EmployeeId.Value : 0,
                EmployeeNr = timeInvoiceTransaction != null && timeInvoiceTransaction.Employee != null ? timeInvoiceTransaction.Employee.EmployeeNr : String.Empty,
                EmployeeName = timeInvoiceTransaction != null && timeInvoiceTransaction.Employee != null ? timeInvoiceTransaction.Employee.Name : String.Empty,
                EmployeeDescription = timeInvoiceTransaction != null && timeInvoiceTransaction.Employee != null ? String.Format("{0} {1}", timeInvoiceTransaction.Employee.EmployeeNr, timeInvoiceTransaction.Employee.Name) : String.Empty,
                //TimeBlockDate
                TimeBlockDateId = timeInvoiceTransaction != null && timeInvoiceTransaction.TimeBlockDateId.HasValue ? timeInvoiceTransaction.TimeBlockDateId.Value : 0,
                Date = timeInvoiceTransaction != null && timeInvoiceTransaction.TimeBlockDate != null ? timeInvoiceTransaction.TimeBlockDate.Date : (DateTime?)null,
                ChargeCostToProject = timeCodeTransaction.DoNotChargeProject != null ? !(bool)timeCodeTransaction.DoNotChargeProject : true,
                IncludeSupplierInvoiceImage = timeCodeTransaction.IncludeSupplierInvoiceImage,
            };

            if (project != null)
            {
                dto.ProjectId = project.ProjectId;
                dto.ProjectNr = project.Number;
                dto.ProjectName = project.Number + " " + project.Name;
                dto.ProjectDescription = String.Format("{0} {1}", project.Number, project.Name);
            }

            if (timeCodeTransaction.CustomerInvoiceId != null && timeCodeTransaction.CustomerInvoiceId != 0)
            {
                Invoice invoice = InvoiceManager.GetCustomerInvoice(entities, (int)timeCodeTransaction.CustomerInvoiceId, loadActor: true);
                if (invoice != null)
                {
                    dto.CustomerInvoiceId = (int)timeCodeTransaction.CustomerInvoiceId;
                    dto.CustomerInvoiceCustomerName = invoice.Actor.Customer.CustomerNr + " " + invoice.Actor.Customer.Name;
                    dto.CustomerInvoiceNumberName = invoice.InvoiceNr + " " + invoice.Actor.Customer.Name;
                    dto.CustomerInvoiceNr = invoice.InvoiceNr;
                }
                else
                    dto.CustomerInvoiceId = 0;
            }
            else
                dto.CustomerInvoiceId = 0;


            entities.Connection.Close();
            return dto;
        }
        public SupplierInvoiceCostTransferDTO GetSupplierInvoiceProjectRow(CompEntities entities, int timeCodeTransactionId)
        {
            return GetSupplierInvoiceProjectRows(entities, timeCodeTransactionId);
        }

        public ActionResult SaveSupplierInvoiceProjectRows(CompEntities entities, TransactionScope transaction, SupplierInvoice supplierInvoice, List<SupplierInvoiceProjectRowDTO> projectRowsInput, int actorCompanyId, bool saveChanges = false, bool keepUnchanged = false, bool fromApp = false)
        {
            ActionResult result = new ActionResult(true);
            List<int> orderIds = new List<int>();

            if (supplierInvoice == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SupplierInvoice");

            try
            {
                #region Prereq

                //No rows to save
                if (projectRowsInput == null)
                    return new ActionResult(true);

                //Amount cannot exceed total amount on invoice
                if (!SupplierInvoiceProjectRowDTO.IsAmountValid(supplierInvoice.VatType, supplierInvoice.TotalAmountCurrency, supplierInvoice.VATAmountCurrency, projectRowsInput))
                    return new ActionResult((int)ActionResultSave.ProjectRowsAmountInvalid, GetText(5741, "Belopp för projektrader får ej överstiga fakturans totalbelopp"));

                #endregion

                #region Project rows

                List<int> handledTimeCodeTransactionIds = new List<int>();
                List<TimeCodeTransaction> timeCodeTransactions = supplierInvoice.InvoiceId > 0 ? TimeTransactionManager.GetTimeCodeTransactionsForSupplierInvoice(entities, supplierInvoice.InvoiceId, actorCompanyId) : new List<TimeCodeTransaction>();
                TimeCodeTransaction timeCodeTransaction = null;

                foreach (SupplierInvoiceProjectRowDTO projectRowInput in projectRowsInput)
                {
                    #region Project row

                    if (projectRowInput.ProjectId <= 0 || projectRowInput.TimeCodeId <= 0)
                        continue;
                    if (projectRowInput.State != (int)SoeEntityState.Active)
                    {

                        continue;
                    }

                    #region Prereq

                    if (projectRowInput.ProjectId <= 0)
                        return new ActionResult((int)ActionResultSave.ProjectRowsAccountInvalid, GetText(5742, "Projekt måste anges på projektrader"));

                    int accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountSupplierPurchase, 0, actorCompanyId, 0);
                    if (accountId <= 0)
                        return new ActionResult((int)ActionResultSave.ProjectRowsAccountInvalid, GetText(5738, "Inställning för standardkonto levreskontra inköp saknas"));

                    TimeCode timeCode = TimeCodeManager.GetTimeCode(entities, projectRowInput.TimeCodeId, actorCompanyId, true, true, false);
                    if (timeCode == null)
                        return new ActionResult((int)ActionResultSave.ProjectRowsTimeCodeInvalid, GetText(5739, "Tidkod måste anges på projektrader"));
                    if ((timeCode.TimeCodeInvoiceProduct == null || timeCode.TimeCodeInvoiceProduct.Count == 0) && (projectRowInput.EmployeeId != null && projectRowInput.EmployeeId != 0))
                        return new ActionResult((int)ActionResultSave.ProjectRowsInvoiceProductInvalid, String.Format(GetText(5740, "Tidkod {0} har inga artiklar kopplade"), timeCode.Code + "." + timeCode.Name));

                    #endregion

                    #region Add/Update TimeCodeTransaction

                    timeCodeTransaction = timeCodeTransactions.FirstOrDefault(i => i.TimeCodeTransactionId == projectRowInput.TimeCodeTransactionId);
                    if (timeCodeTransaction == null)
                    {
                        #region Add

                        timeCodeTransaction = new TimeCodeTransaction()
                        {
                            TimeInvoiceTransaction = new EntityCollection<TimeInvoiceTransaction>(),
                        };
                        SetCreatedProperties(timeCodeTransaction);
                        entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        if (!timeCodeTransaction.TimeInvoiceTransaction.IsLoaded)
                            timeCodeTransaction.TimeInvoiceTransaction.Load();

                        SetModifiedProperties(timeCodeTransaction);
                        handledTimeCodeTransactionIds.Add(timeCodeTransaction.TimeCodeTransactionId);

                        #endregion
                    }

                    timeCodeTransaction.Quantity = 1;
                    timeCodeTransaction.InvoiceQuantity = 1;
                    timeCodeTransaction.Amount = projectRowInput.Amount;
                    timeCodeTransaction.Start = CalendarUtility.DATETIME_DEFAULT;
                    timeCodeTransaction.Stop = CalendarUtility.DATETIME_DEFAULT;
                    timeCodeTransaction.Comment = null;
                    timeCodeTransaction.State = (int)SoeEntityState.Active;
                    timeCodeTransaction.Type = (int)TimeCodeTransactionType.TimeProject;
                    timeCodeTransaction.DoNotChargeProject = !projectRowInput.ChargeCostToProject;
                    timeCodeTransaction.IncludeSupplierInvoiceImage = projectRowInput.IncludeSupplierInvoiceImage;

                    //Set FK
                    timeCodeTransaction.SupplierInvoiceId = supplierInvoice.InvoiceId;
                    timeCodeTransaction.ProjectId = projectRowInput.ProjectId;
                    timeCodeTransaction.TimeCodeId = projectRowInput.TimeCodeId;
                    timeCodeTransaction.CustomerInvoiceId = projectRowInput.CustomerInvoiceId != 0 ? projectRowInput.CustomerInvoiceId : (int?)null;

                    //Set currency amounts
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

                    if (projectRowInput.CustomerInvoiceId != 0 && projectRowInput.IncludeSupplierInvoiceImage && !orderIds.Contains(projectRowInput.CustomerInvoiceId))
                        orderIds.Add(projectRowInput.CustomerInvoiceId);

                    #endregion

                    #region Add/Update TimeInvoiceTransaction

                    List<int> handledTimeInvoiceTransactionIds = new List<int>();
                    List<TimeInvoiceTransaction> timeInvoiceTransactions = timeCodeTransaction.TimeInvoiceTransaction.ToList();
                    List<TimeCodeInvoiceProduct> invoiceProducts = timeCode.TimeCodeInvoiceProduct.ToList();

                    if (invoiceProducts.Count > 0)
                    {
                        Dictionary<int, decimal> invoiceProductAmountDict = CalculateProjectInvoiceProductAmounts(invoiceProducts, projectRowInput.Amount);

                        foreach (TimeCodeInvoiceProduct invoiceProduct in invoiceProducts)
                        {
                            decimal amount = 0;
                            if (invoiceProductAmountDict.ContainsKey(invoiceProduct.TimeCodeInvoiceProductId))
                                amount = invoiceProductAmountDict[invoiceProduct.TimeCodeInvoiceProductId];

                            TimeInvoiceTransaction timeInvoiceTransaction = timeInvoiceTransactions.FirstOrDefault(i => i.ProductId == invoiceProduct.ProductId);
                            if (timeInvoiceTransaction == null)
                            {
                                #region Add

                                timeInvoiceTransaction = new TimeInvoiceTransaction();
                                SetCreatedProperties(timeInvoiceTransaction);
                                entities.TimeInvoiceTransaction.AddObject(timeInvoiceTransaction);

                                #endregion
                            }
                            else
                            {
                                #region Update

                                SetModifiedProperties(timeInvoiceTransaction);
                                handledTimeInvoiceTransactionIds.Add(timeInvoiceTransaction.TimeInvoiceTransactionId);

                                #endregion
                            }

                            timeInvoiceTransaction.Quantity = 1;
                            timeInvoiceTransaction.InvoiceQuantity = 1;
                            timeInvoiceTransaction.Amount = amount;
                            timeInvoiceTransaction.ManuallyAdded = false;
                            timeInvoiceTransaction.Exported = false;
                            timeInvoiceTransaction.State = (int)SoeEntityState.Active;

                            //Set currency amounts
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeInvoiceTransaction);

                            //Set FK
                            timeInvoiceTransaction.ActorCompanyId = actorCompanyId;
                            timeInvoiceTransaction.ProductId = invoiceProduct.ProductId;
                            timeInvoiceTransaction.AccountStdId = accountId;

                            //EmployeeId
                            if (projectRowInput.EmployeeId.HasValue && projectRowInput.EmployeeId.Value > 0)
                                timeInvoiceTransaction.EmployeeId = projectRowInput.EmployeeId.Value;

                            //TimeBlockDate
                            if (projectRowInput.TimeBlockDateId.HasValue && projectRowInput.TimeBlockDateId.Value > 0)
                                timeInvoiceTransaction.TimeBlockDateId = projectRowInput.TimeBlockDateId.Value;
                            else if (projectRowInput.EmployeeId.HasValue && projectRowInput.EmployeeId.Value > 0)
                            {
                                DateTime date = projectRowInput.Date.HasValue ? projectRowInput.Date.Value : DateTime.Today;
                                timeInvoiceTransaction.TimeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, actorCompanyId, projectRowInput.EmployeeId.Value, date, true);
                            }

                            //TimeCodeTransaction
                            timeInvoiceTransaction.TimeCodeTransaction = timeCodeTransaction;
                        }
                    }

                    #endregion

                    #region Delete TimeInvoiceTransaction
                    //Delete existing transactions not handled
                    foreach (TimeInvoiceTransaction timeInvoiceTransaction in timeInvoiceTransactions)
                    {
                        if (timeInvoiceTransaction.TimeInvoiceTransactionId == 0 || handledTimeInvoiceTransactionIds.Contains(timeInvoiceTransaction.TimeInvoiceTransactionId))
                            continue;

                        ChangeEntityState(timeInvoiceTransaction, SoeEntityState.Deleted);
                    }

                    #endregion

                    #endregion
                }

                #region Delete TimeCodeTransaction/TimeInvoiceTransaction
                if (keepUnchanged)
                {
                    timeCodeTransactions.ForEach(t => handledTimeCodeTransactionIds.Add(t.TimeCodeTransactionId));
                }
                if (fromApp)
                {
                    foreach (var inRow in projectRowsInput)
                    {
                        if (inRow.State == SoeEntityState.Deleted && inRow.TimeCodeTransactionId != 0)
                        {
                            handledTimeCodeTransactionIds.Remove(inRow.TimeCodeTransactionId);
                        }
                    }
                }
                //Delete existing transactions not handled
                foreach (TimeCodeTransaction tct in timeCodeTransactions)
                {
                    if (tct.TimeCodeTransactionId == 0 || handledTimeCodeTransactionIds.Contains(tct.TimeCodeTransactionId))
                        continue;

                    ChangeEntityState(tct, SoeEntityState.Deleted);

                    if (!tct.TimeInvoiceTransaction.IsLoaded)
                        tct.TimeInvoiceTransaction.Load();

                    foreach (TimeInvoiceTransaction timeInvoiceTransaction in tct.TimeInvoiceTransaction)
                    {
                        ChangeEntityState(timeInvoiceTransaction, SoeEntityState.Deleted);
                    }
                }

                #endregion

                #endregion

                if (saveChanges)
                {
                    result = SaveChanges(entities, transaction);
                    if (result.Success && timeCodeTransaction != null)
                        result.IntegerValue = timeCodeTransaction.TimeCodeTransactionId;
                }


            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result = new ActionResult(ex);
            }

            result.Value = orderIds;

            return result;
        }

        private Dictionary<int, decimal> CalculateProjectInvoiceProductAmounts(List<TimeCodeInvoiceProduct> invoiceProducts, decimal totalAmount)
        {
            Dictionary<int, decimal> dict = new Dictionary<int, decimal>();
            if (invoiceProducts == null)
                return dict;

            decimal factorTotal = invoiceProducts.Sum(i => i.Factor);
            decimal amountShare = totalAmount / factorTotal;
            decimal totalAmountShared = 0;

            for (int current = 1; current <= invoiceProducts.Count; current++)
            {
                TimeCodeInvoiceProduct invoiceProduct = invoiceProducts[current - 1];

                //Amount for current product
                decimal amount = Decimal.Round(invoiceProduct.Factor * amountShare, 2);

                //Tota amount shared between products
                totalAmountShared = Decimal.Round(totalAmountShared + amount, 2);

                //Add rounding for last product
                if (current == invoiceProducts.Count)
                {
                    decimal rounding = Decimal.Round(totalAmount - totalAmountShared);
                    if (rounding > 0)
                    {
                        amount = Decimal.Round(amount + rounding, 2);
                        totalAmountShared = totalAmount;
                    }
                }

                dict.Add(invoiceProduct.TimeCodeInvoiceProductId, amount);
            }

            return dict;
        }

        #endregion

        #region ProjectAccount

        public ProjectAccountStd GetProjectAccount(CompEntities entities, int projectId, ProjectAccountType projectAccountType)
        {
            int currentProjectId = 0;
            var project = GetProjectSmall(entities, projectId);

            if (project != null && project.ParentProjectId != null && !project.UseAccounting)
            {
                currentProjectId = FindTopProject(entities, (int)project.ParentProjectId);
            }
            else
            {
                currentProjectId = projectId;
            }

            var type = (int)projectAccountType;
            return (from a in entities.ProjectAccountStd
                        .Include("AccountStd.Account")
                        .Include("AccountInternal.Account.AccountDim")
                    where a.ProjectId == currentProjectId &&
                    a.Type == type
                    select a).FirstOrDefault();
        }

        public int FindTopProject(CompEntities entities, int projectId, int startProjectId = 0)
        {
            var project = GetProjectSmall(entities, projectId);

            if (project != null && (project.ParentProjectId == null || project.ParentProjectId == 0))
            {
                return projectId;
            }
            else if (project != null)
            {
                if (project.ProjectId == startProjectId)
                {
                    //we have a recursive project dependency so stop here....
                    log.Error($"FindTopProject: Recursive parent projects found for projectId {projectId}");
                    return (int)project.ParentProjectId;
                }
                else if (startProjectId == 0)
                {
                    startProjectId = projectId;
                }

                return FindTopProject(entities, (int)project.ParentProjectId, startProjectId);
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region ProjectTimeBlock

        public ProjectTimeBlock GetProjectTimeBlock(int projectTimeBlockId, bool includeTimeBlockDate = false, bool includeTimeDeviationCause = false, bool includeTransactions = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetProjectTimeBlock(entities, projectTimeBlockId, includeTimeBlockDate, includeTimeDeviationCause, includeTransactions);
        }

        public ProjectTimeBlock GetProjectTimeBlock(CompEntities entities, int projectTimeBlockId, bool includeTimeBlockDate = false, bool includeTimeDeviationCause = false, bool includeTransactions = false)
        {
            if (projectTimeBlockId == 0)
                return null;

            int actorCompanyId = base.ActorCompanyId;
            IQueryable<ProjectTimeBlock> query = entities.ProjectTimeBlock;

            if (includeTimeBlockDate)
                query = query.Include("TimeBlockDate");

            if (includeTimeDeviationCause)
                query = query.Include("TimeDeviationCause");

            if (includeTransactions)
                query = query.Include("TimeCodeTransaction.TimeInvoiceTransaction").Include("TimeCodeTransaction.TimePayrollTransaction");

            return (from t in query
                    where t.ProjectTimeBlockId == projectTimeBlockId &&
                    t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        public DateTime GetProjectTimeBlocksLastDate(int projectId, int recordId, int recordType, int employeeId = 0)
        {
            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            if (useProjectTimeBlocks)
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                entities.ProjectTimeBlock.NoTracking();
                IQueryable<ProjectTimeBlock> query = (from p in entities.ProjectTimeBlock.Include("TimeBlockDate")
                                                      where p.ProjectId == projectId &&
                                                      p.CustomerInvoiceId == recordId &&
                                                      p.State == (int)SoeEntityState.Active
                                                      orderby p.TimeBlockDate.Date
                                                      select p);

                if (employeeId != 0)
                    query = query.Where(p => p.EmployeeId == employeeId);

                var blocks = query.ToList();

                return blocks.Any() ? blocks.Last().TimeBlockDate.Date : DateTime.Today;
            }
            else
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                entities.ProjectInvoiceWeek.NoTracking();
                IQueryable<ProjectInvoiceWeek> query = (from p in entities.ProjectInvoiceWeek
                                                        where p.ProjectId == projectId &&
                                                        p.RecordId == recordId &&
                                                        p.RecordType == recordType &&
                                                        p.State == (int)SoeEntityState.Active
                                                        orderby p.Date
                                                        select p);

                if (employeeId != 0)
                    query = query.Where(p => p.EmployeeId == employeeId);

                var weeks = query.ToList();

                return weeks.Any() ? weeks.Last().Date : DateTime.Today;
            }
        }

        public List<ProjectTimeBlockView> GetProjectTimeBlocksView(CompEntities entities, int timeBlockDateTid, int employeeId)
        {
            return (from p in entities.ProjectTimeBlockView
                    where p.ActorCompanyId == ActorCompanyId && p.EmployeeId == employeeId && p.TimeBlockDateId == timeBlockDateTid
                    select p).ToList();

        }

        public List<ProjectTimeBlock> GetProjectTimeBlocks(CompEntities entities, int timeBlockDateTid, bool includeTimeCodeTransactions = true, bool includeTimeDeviationCause = false)
        {
            IQueryable<ProjectTimeBlock> query = (from p in entities.ProjectTimeBlock select p);
            if (includeTimeCodeTransactions)
                query = query.Include("TimeCodeTransaction");
            if (includeTimeDeviationCause)
                query = query.Include("TimeDeviationCause");

            return query.Where(w => w.State == (int)SoeEntityState.Active && w.TimeBlockDateId == timeBlockDateTid).ToList();
        }

        public List<ProjectTimeBlock> GetProjectTimeBlocks(CompEntities entities, List<int> projectTimeBlockIds, int actorCompanyId, bool loadTimeBlockDate = false)
        {
            IQueryable<ProjectTimeBlock> oQuery = entities.ProjectTimeBlock;
            if (loadTimeBlockDate)
                oQuery = oQuery.Include("TimeBlockDate");

            return oQuery.Where(ptb => ptb.ActorCompanyId == actorCompanyId && ptb.State == (int)SoeEntityState.Active && projectTimeBlockIds.Contains(ptb.ProjectTimeBlockId)).ToList();
        }

        public List<ProjectTimeBlock> GetProjectTimeBlocksForProject(CompEntities entities, int projectId, int? invoiceId, DateTime? fromDate, DateTime? toDate)
        {
            IQueryable<ProjectTimeBlock> query = (from p in entities.ProjectTimeBlock
                    .Include("TimeCodeTransaction")
                    .Include("Employee.ContactPerson")
                    .Include("TimeBlockDate")
                                                  where p.ProjectId == projectId &&
                                                        p.State == (int)SoeEntityState.Active
                                                  select p);

            if (invoiceId.HasValue)
                query = query.Where(p => p.CustomerInvoiceId == invoiceId.Value);

            if (fromDate.HasValue)
                query = query.Where(p => p.TimeBlockDate.Date >= fromDate);

            if (toDate.HasValue)
                query = query.Where(p => p.TimeBlockDate.Date <= toDate);

            return query.ToList();
        }

        public List<ProjectTimeBlock> GetProjectTimeBlocksWithOrderAndProject(int timeBlockDateTid)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProjectTimeBlock.NoTracking();
            return GetProjectTimeBlocksWithOrderAndProject(entities, timeBlockDateTid);
        }

        public List<ProjectTimeBlock> GetProjectTimeBlocksWithOrderAndProject(CompEntities entities, int timeBlockDateTid)
        {
            return entities.ProjectTimeBlock.Include("CustomerInvoice").Include("Project").Where(w => w.State == (int)SoeEntityState.Active && w.TimeBlockDateId == timeBlockDateTid).ToList();
        }

        public List<ProjectTimeBlockDTO> GetProjectTimeBlocksForInvoiceRow(int invoiceId, int customerInvoiceRowId, DateTime? from, DateTime? to)
        {
            var blocks = GetProjectTimeBlocks(0, invoiceId, (int)SoeProjectRecordType.Order, 0, false, from, to, false, customerInvoiceRowId);
            return blocks;
        }

        public List<ProjectTimeBlockDTO> GetProjectTimeBlocks(int projectId, int recordId, int recordType, int employeeId, bool loadOnlyForEmployee, DateTime? from, DateTime? to, bool sortDescending, int customerInvoiceRowId = 0, bool incPlannedAbsence = false, bool setAttestStates = false)
        {
            var blocks = new List<ProjectTimeBlockDTO>();
            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            if (recordId == 0 && !from.HasValue && !to.HasValue)
            {
                return blocks;
            }

            if (recordId != 0 && incPlannedAbsence)
            {
                incPlannedAbsence = false;
            }

            using (var entities = new CompEntities())
            {
                entities.Connection.Open();
                entities.CommandTimeout = 180;

                if (useProjectTimeBlocks)
                {
                    blocks = GetProjectTimeBlockDTOs(entities, from, to,
                            loadOnlyForEmployee ? new List<int> { employeeId } : null,
                            projectId > 0 ? new List<int> { projectId } : null,
                            recordId > 0 ? new List<int> { recordId } : null,
                            false, customerInvoiceRowId, incPlannedAbsence);
                }
                else
                {
                    IQueryable<ProjectInvoiceWeek> weeksQuery = (from p in entities.ProjectInvoiceWeek
                                                        .Include("Employee.ContactPerson")
                                                        .Include("ProjectInvoiceDay")
                                                        .Include("ProjectInvoiceDay.TimeCodeTransaction.TimeCode")
                                                        .Include("ProjectInvoiceDay.TimeCodeTransaction.TimeInvoiceTransaction")
                                                        .Include("ProjectInvoiceDay.TimeCodeTransaction.TimePayrollTransaction")
                                                                 where
                                                                 p.ActorCompanyId == this.ActorCompanyId &&
                                                                 p.RecordType == recordType &&
                                                                 p.State == (int)SoeEntityState.Active
                                                                 select p);
                    if (customerInvoiceRowId > 0)
                        weeksQuery = weeksQuery.Include("Project");

                    if (recordId > 0)
                    {
                        weeksQuery = weeksQuery.Where(p => p.RecordId == recordId);
                    }

                    if (projectId > 0)
                    {
                        weeksQuery = weeksQuery.Where(p => p.ProjectId == projectId);
                    }

                    if (loadOnlyForEmployee)
                        weeksQuery = weeksQuery.Where(r => r.EmployeeId == employeeId);

                    if (from != null)
                    {
                        var firstDateOfWeek = CalendarUtility.GetBeginningOfDay(CalendarUtility.GetFirstDateOfWeek(from.Value));
                        weeksQuery = weeksQuery.Where(r => r.Date >= firstDateOfWeek);
                    }

                    if (to != null)
                    {
                        var lastDateOfWeek = CalendarUtility.GetBeginningOfDay(to.Value.AddDays(1));
                        weeksQuery = weeksQuery.Where(r => r.Date < lastDateOfWeek);
                    }

                    List<CustomerInvoice> invoices = null;

                    if (customerInvoiceRowId > 0)
                    {
                        var invoice = InvoiceManager.GetCustomerInvoice(recordId);
                        if (invoice != null)
                        {
                            invoices = new List<CustomerInvoice> { invoice };
                        }

                        weeksQuery = weeksQuery.Where(w => w.ProjectInvoiceDay.Any(p => p.TimeCodeTransaction.Any(c => c.State == (int)SoeEntityState.Active && c.TimeInvoiceTransaction.Any(i => i.State == (int)SoeEntityState.Active && i.CustomerInvoiceRowId == customerInvoiceRowId))));
                    }

                    var attestStates = new List<AttestStateDTO>();
                    if (setAttestStates)
                    {
                        attestStates.AddRange(AttestManager.GetAttestStates(entities, this.ActorCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing).ToDTOs());
                    }

                    foreach (ProjectInvoiceWeek projectInvoiceWeek in weeksQuery.ToList())
                    {
                        foreach (ProjectInvoiceDay projectInvoiceDay in projectInvoiceWeek.ProjectInvoiceDay)
                        {
                            if (from.HasValue && (projectInvoiceDay.Date < from.Value))
                                continue;

                            if (from.HasValue && (projectInvoiceDay.Date > to.Value))
                                continue;

                            var timeCodeTransactions = projectInvoiceDay.TimeCodeTransaction.Where(t => t.State == (int)SoeEntityState.Active);

                            if (timeCodeTransactions.IsNullOrEmpty())
                            {
                                //Default to handle zero working in invoicing
                                ProjectTimeBlockDTO projectTimeBlock = SetProjectInvoiceWeekEmptyValues(projectInvoiceWeek, projectInvoiceDay, invoices);
                                if (projectTimeBlock != null)
                                    blocks.Add(projectTimeBlock);
                            }
                            else
                            {
                                if (customerInvoiceRowId > 0 && !timeCodeTransactions.IsNullOrEmpty())
                                {
                                    if (!timeCodeTransactions.Any(t => t.TimeInvoiceTransaction.Any(i => i.CustomerInvoiceRowId == customerInvoiceRowId)))
                                    {
                                        continue;
                                    }
                                }

                                foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactions)
                                {
                                    ProjectTimeBlockDTO projectTimeBlock = SetProjectInvoiceWeekValues(projectInvoiceWeek, projectInvoiceDay, timeCodeTransaction, invoices, attestStates, customerInvoiceRowId: customerInvoiceRowId);
                                    if (projectTimeBlock != null)
                                        blocks.Add(projectTimeBlock);
                                }
                            }
                        }
                    }

                    /*
                    IQueryable<ProjectInvoiceDayView> query = (from p in entities.ProjectInvoiceDayView
                                                                 where p.ProjectId == projectId &&
                                                                 p.RecordId == recordId &&
                                                                 p.RecordType == recordType &&
                                                                 p.ActorCompanyId == this.ActorCompanyId
                                                               select p);

                    

                    if (loadOnlyForEmployee)
                        query = query.Where(r => r.EmployeeId == employeeId);

                    if (from != null)
                    {
                        var firstDateOfWeek = CalendarUtility.GetBeginningOfDay(from.Value);
                        query = query.Where(r => r.ProjectInvoiceDayDate >= firstDateOfWeek);
                    }

                    if (to != null)
                    {
                        var lastDateOfWeek = CalendarUtility.GetBeginningOfDay(to.Value.AddDays(1));
                        query = query.Where(r => r.ProjectInvoiceDayDate < lastDateOfWeek);
                    }

                    foreach (ProjectInvoiceDayView dayItem in query.ToList())
                    {
                        ProjectTimeBlockDTO projectTimeBlock = SetProjectInvoiceWeekValues(dayItem);
                        if (projectTimeBlock != null)
                            blocks.Add(projectTimeBlock);
                    }
                    */
                }
            }

            return (sortDescending) ? blocks.OrderByDescending(x => x.Date).ToList() : blocks.OrderBy(x => x.Date).ToList();
        }

        public bool HasProjectTimeBlocks(CompEntities entities, int invoiceId, int projectId, int actorCompanyId)
        {
            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, actorCompanyId, 0, false);
            if (useProjectTimeBlocks)
            {
                return entities.ProjectTimeBlock.Any(r => r.ActorCompanyId == actorCompanyId && r.ProjectId == projectId && r.CustomerInvoiceId == invoiceId && r.State == (int)SoeEntityState.Active);
            }
            else
            {
                return (from p in entities.ProjectInvoiceWeek
                        join day in entities.ProjectInvoiceDay
                            on p.ProjectInvoiceWeekId equals day.ProjectInvoiceWeekId
                        where p.ProjectId == projectId && p.RecordId == invoiceId && p.State == (int)SoeEntityState.Active && p.ActorCompanyId == actorCompanyId
                        select p).Any();
            }
        }

        public List<DateTime> GetProjectTimeBlockStopTimes(CompEntities entities, int employeeId, DateTime forDate)
        {
            IQueryable<ProjectTimeBlockView> query = (from p in entities.ProjectTimeBlockView select p);
            //only rows with working hours...
            return query.Where(w => w.ActorCompanyId == ActorCompanyId && w.EmployeeId == employeeId && w.Date == forDate && w.StartTime != w.StopTime && !w.MandatoryTime && !w.CalculateAsOtherTimeInSales).Select(p => p.StopTime).ToList();
        }

        public ActionResult CreateProjectTimeBlock(CompEntities entities, TransactionScope transaction, TimeCodeTransaction timeCodeTransaction, DateTime date, int employeeId, int projectId, int recordId, int recordType, int invoiceTimeInMinutes, int workTimeInMinutes, string externalNote, string note)
        {
            ActionResult result = new ActionResult(true);
            List<ProjectTimeBlock> projectTimeBlocks = new List<ProjectTimeBlock>();

            try
            {
                //Get day start time setting
                int dayStartTime = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, UserId, ActorCompanyId, 0);

                if (timeCodeTransaction.ProjectTimeBlockId.HasValue)
                {
                    //Update existing
                    ProjectTimeBlock existingProjectTimeBlock = GetProjectTimeBlock(entities, timeCodeTransaction.ProjectTimeBlockId.Value, true, true, true);

                    if (existingProjectTimeBlock == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound);

                    List<ProjectTimeBlock> projectTimeBlocksOnDate = GetProjectTimeBlocks(entities, existingProjectTimeBlock.TimeBlockDateId).Where(p => p.ProjectTimeBlockId != existingProjectTimeBlock.ProjectTimeBlockId).ToList();

                    existingProjectTimeBlock.EmployeeId = employeeId;
                    existingProjectTimeBlock.ProjectId = projectId;
                    existingProjectTimeBlock.CustomerInvoiceId = recordId;
                    existingProjectTimeBlock.InvoiceQuantity = invoiceTimeInMinutes;
                    existingProjectTimeBlock.ExternalNote = externalNote;
                    existingProjectTimeBlock.InternalNote = note;

                    if (workTimeInMinutes > 0 && workTimeInMinutes != CalendarUtility.TimeSpanToMinutes(existingProjectTimeBlock.StopTime, existingProjectTimeBlock.StartTime))
                    {
                        ProjectTimeBlock previousProjectTimeBlock = projectTimeBlocksOnDate.Where(p => p.TimeBlockDateId == existingProjectTimeBlock.TimeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();
                        if (previousProjectTimeBlock != null)
                        {
                            existingProjectTimeBlock.StartTime = previousProjectTimeBlock.StopTime;
                            existingProjectTimeBlock.StopTime = existingProjectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                        }
                        else
                        {
                            //Set times from schedule
                            TimeScheduleTemplateBlock timeScheduleTemplateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(entities, employeeId, existingProjectTimeBlock.TimeBlockDate.Date, true).OrderBy(t => t.StartTime).FirstOrDefault();
                            if (timeScheduleTemplateBlock != null)
                            {
                                existingProjectTimeBlock.StartTime = new DateTime(timeCodeTransaction.ProjectInvoiceDay.Date.Year, timeCodeTransaction.ProjectInvoiceDay.Date.Month, timeCodeTransaction.ProjectInvoiceDay.Date.Day, timeScheduleTemplateBlock.StartTime.Hour, timeScheduleTemplateBlock.StartTime.Minute, timeScheduleTemplateBlock.StartTime.Second);
                                existingProjectTimeBlock.StopTime = existingProjectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                            }
                            else
                            {
                                if (dayStartTime != 0)
                                {
                                    existingProjectTimeBlock.StartTime = existingProjectTimeBlock.TimeBlockDate.Date.AddMinutes(dayStartTime);
                                    existingProjectTimeBlock.StopTime = existingProjectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                }
                                else
                                {
                                    existingProjectTimeBlock.StartTime = existingProjectTimeBlock.TimeBlockDate.Date.AddHours(7);
                                    existingProjectTimeBlock.StopTime = existingProjectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                }
                            }
                        }
                    }
                    else
                    {
                        existingProjectTimeBlock.StartTime = existingProjectTimeBlock.TimeBlockDate.Date;
                        existingProjectTimeBlock.StopTime = existingProjectTimeBlock.TimeBlockDate.Date;
                    }

                    SetModifiedProperties(existingProjectTimeBlock);
                    projectTimeBlocks.Add(existingProjectTimeBlock);
                }
                else
                {
                    //Load causes
                    IEnumerable<TimeDeviationCauseDTO> timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, base.ActorCompanyId, loadTimeCode: true).ToDTOs();

                    //Load Employee
                    EmployeeDTO employee = EmployeeManager.GetEmployee(entities, employeeId, base.ActorCompanyId, onlyActive: true, loadEmployment: true, loadTimeDeviationCause: true, loadTimeCode: true).ToDTO(includeEmployeeGroup: true, useLastEmployment: true);

                    //Get TimeBlockDate
                    TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, base.ActorCompanyId, employeeId, date, true);

                    //Load ProjectTimeBlocks on same date
                    List<ProjectTimeBlock> projectTimeBlocksOnDate = GetProjectTimeBlocks(entities, timeBlockDate.TimeBlockDateId);

                    TimeDeviationCauseDTO deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeCode != null && t.TimeCode.TimeCodeId == timeCodeTransaction.TimeCodeId);
                    if (deviationCause == null)
                    {
                        if (employee.TimeDeviationCauseId.HasValue)
                        {
                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.TimeDeviationCauseId.Value);
                            if (deviationCause == null && employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                        }
                        else if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                        {
                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                        }
                    }

                    if (timeBlockDate != null && deviationCause != null)
                    {
                        ProjectTimeBlock projectTimeBlock = new ProjectTimeBlock()
                        {
                            ActorCompanyId = base.ActorCompanyId,
                            CustomerInvoiceId = recordId,
                            EmployeeId = employeeId,
                            ExternalNote = externalNote,
                            InternalNote = note,
                            InvoiceQuantity = invoiceTimeInMinutes,
                            ProjectId = projectId,
                            TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                            TimeDeviationCauseId = deviationCause.TimeDeviationCauseId,
                            RecordType = recordType,
                        };

                        if (workTimeInMinutes > 0)
                        {
                            ProjectTimeBlock previousProjectTimeBlock = projectTimeBlocksOnDate.Where(p => p.TimeBlockDateId == timeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();
                            if (previousProjectTimeBlock != null)
                            {
                                projectTimeBlock.StartTime = previousProjectTimeBlock.StopTime;
                                projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                            }
                            else
                            {
                                //Set times from schedule
                                TimeScheduleTemplateBlock timeScheduleTemplateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(entities, employee.EmployeeId, timeBlockDate.Date, true).OrderBy(t => t.StartTime).FirstOrDefault();
                                if (timeScheduleTemplateBlock != null)
                                {
                                    projectTimeBlock.StartTime = new DateTime(timeCodeTransaction.ProjectInvoiceDay.Date.Year, timeCodeTransaction.ProjectInvoiceDay.Date.Month, timeCodeTransaction.ProjectInvoiceDay.Date.Day, timeScheduleTemplateBlock.StartTime.Hour, timeScheduleTemplateBlock.StartTime.Minute, timeScheduleTemplateBlock.StartTime.Second);
                                    projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                }
                                else
                                {
                                    if (dayStartTime != 0)
                                    {
                                        projectTimeBlock.StartTime = timeBlockDate.Date.AddMinutes(dayStartTime);
                                        projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                    }
                                    else
                                    {
                                        projectTimeBlock.StartTime = timeBlockDate.Date.AddHours(7);
                                        projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                    }
                                }
                            }
                        }
                        else
                        {
                            projectTimeBlock.StartTime = timeBlockDate.Date;
                            projectTimeBlock.StopTime = timeBlockDate.Date;
                        }

                        //Create projectTimeBlock
                        SetCreatedProperties(projectTimeBlock);
                        entities.ProjectTimeBlock.AddObject(projectTimeBlock);

                        //Connect to transaction
                        timeCodeTransaction.ProjectTimeBlock = projectTimeBlock;
                        SetModifiedProperties(timeCodeTransaction);
                    }
                }

                result = SaveChanges(entities, transaction);
                if (!result.Success)
                    return result;

                result.Value = projectTimeBlocks;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorMessage = ex.Message;
                result.IntegerValue = 0;
                return result;
            }

            return result;
        }

        #endregion

        #region ProjectInvoiceWeek

        public ProjectInvoiceWeek GetProjectInvoiceWeek(CompEntities entities, int actorCompanyId, DateTime date, int projectId, int recordId, int recordType, int employeeId, int timecodeId, bool timeCodeHasChanged = false)
        {
            return (from piw in entities.ProjectInvoiceWeek
                    where piw.ProjectId == projectId &&
                    piw.EmployeeId == employeeId &&
                    piw.ActorCompanyId == actorCompanyId &&
                    (timeCodeHasChanged || (piw.TimeCodeId.HasValue && piw.TimeCodeId.Value == timecodeId)) &&
                    piw.RecordId == recordId &&
                    piw.RecordType == recordType &&
                    (piw.Date.Year == date.Year && piw.Date.Month == date.Month && piw.Date.Day == date.Day) &&
                    piw.State == (int)SoeEntityState.Active
                    select piw).FirstOrDefault();
        }

        public ProjectInvoiceWeek GetProjectInvoiceWeek(CompEntities entities, int actorCompanyId, int projectInvoiceWeekId)
        {
            return (from piw in entities.ProjectInvoiceWeek
                    where piw.ProjectInvoiceWeekId == projectInvoiceWeekId &&
                    piw.ActorCompanyId == actorCompanyId &&
                    piw.State == (int)SoeEntityState.Active
                    select piw).FirstOrDefault();
        }

        public ProjectTotalsDTO GetProjectTotals(int actorCompanyId, int projectId, int recordId, int recordType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetProjectTotals(entities, actorCompanyId, projectId, recordId, recordType);
        }

        public ProjectTotalsDTO GetProjectTotals(CompEntities entities, int actorCompanyId, int projectId, int recordId, int recordType)
        {
            var totals = new ProjectTotalsDTO();

            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            var totalResult = entities.GetProjectTotals(actorCompanyId, projectId, recordId, recordType, useProjectTimeBlocks ? 1 : 0).FirstOrDefault();
            if (totalResult != null)
            {
                totals.InvoiceTime = Convert.ToInt32(totalResult.SumInvoiceTime);
                totals.WorkTime = Convert.ToInt32(totalResult.SumWorkTime);
                totals.OtherTime = Convert.ToInt32(totalResult.SumOtherTime);
            }

            return totals;
        }

        public ActionResult CreateProjectTimeBlocksFromProjectInvoiceWeek(CompEntities entities, TransactionScope transaction, List<ProjectInvoiceWeek> projectInvoiceWeeks)
        {
            ActionResult result = new ActionResult();

            int dayStartTime = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, UserId, ActorCompanyId, 0);

            List<EmployeeDTO> usedEmployees = new List<EmployeeDTO>();
            List<ProjectTimeBlock> projectTimeBlocks = new List<ProjectTimeBlock>();

            try
            {
                //Load causes
                IEnumerable<TimeDeviationCauseDTO> timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, base.ActorCompanyId, loadTimeCode: true).ToDTOs();

                #region ProjectInvoiceDay

                foreach (ProjectInvoiceWeek week in projectInvoiceWeeks)
                {
                    EmployeeDTO employee = usedEmployees.FirstOrDefault(e => e.EmployeeId == week.EmployeeId);
                    if (employee == null)
                    {
                        employee = EmployeeManager.GetEmployee(entities, week.EmployeeId, base.ActorCompanyId, onlyActive: true, loadEmployment: true, loadTimeDeviationCause: true, loadTimeCode: true).ToDTO(includeEmployeeGroup: true, useLastEmployment: true);
                        usedEmployees.Add(employee);
                    }

                    foreach (ProjectInvoiceDay day in week.ProjectInvoiceDay)
                    {
                        TimeCodeTransaction timeCodeTransaction = TimeTransactionManager.GetTimeCodeTransactionsForProjectInvoiceDay(entities, day.ProjectInvoiceDayId).FirstOrDefault();
                        if (timeCodeTransaction != null)
                        {
                            if (timeCodeTransaction.ProjectTimeBlockId.HasValue)
                            {
                                //Update existing
                                ProjectTimeBlock existingProjectTimeBlock = GetProjectTimeBlock(entities, timeCodeTransaction.ProjectTimeBlockId.Value, true, true, true);

                                if (existingProjectTimeBlock == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound);

                                List<ProjectTimeBlock> projectTimeBlocksOnDate = GetProjectTimeBlocks(entities, existingProjectTimeBlock.TimeBlockDateId).Where(p => p.ProjectTimeBlockId != existingProjectTimeBlock.ProjectTimeBlockId).ToList();

                                existingProjectTimeBlock.EmployeeId = employee.EmployeeId;
                                existingProjectTimeBlock.ProjectId = week.ProjectId;
                                existingProjectTimeBlock.CustomerInvoiceId = week.RecordId;
                                existingProjectTimeBlock.InvoiceQuantity = day.InvoiceTimeInMinutes;
                                existingProjectTimeBlock.ExternalNote = day.CommentExternal;
                                existingProjectTimeBlock.InternalNote = day.Note;

                                if (day.WorkTimeInMinutes > 0 && day.WorkTimeInMinutes != CalendarUtility.TimeSpanToMinutes(existingProjectTimeBlock.StopTime, existingProjectTimeBlock.StartTime))
                                {
                                    ProjectTimeBlock previousProjectTimeBlock = projectTimeBlocksOnDate.Where(p => p.TimeBlockDateId == existingProjectTimeBlock.TimeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();
                                    if (previousProjectTimeBlock == null)
                                        previousProjectTimeBlock = projectTimeBlocks.Where(p => p.TimeBlockDateId == existingProjectTimeBlock.TimeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();

                                    if (previousProjectTimeBlock != null)
                                    {
                                        existingProjectTimeBlock.StartTime = previousProjectTimeBlock.StopTime;
                                        existingProjectTimeBlock.StopTime = existingProjectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                    }
                                    else
                                    {
                                        //Set times from schedule
                                        TimeScheduleTemplateBlock timeScheduleTemplateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(entities, employee.EmployeeId, existingProjectTimeBlock.TimeBlockDate.Date, true).OrderBy(t => t.StartTime).FirstOrDefault();
                                        if (timeScheduleTemplateBlock != null)
                                        {
                                            existingProjectTimeBlock.StartTime = new DateTime(timeCodeTransaction.ProjectInvoiceDay.Date.Year, timeCodeTransaction.ProjectInvoiceDay.Date.Month, timeCodeTransaction.ProjectInvoiceDay.Date.Day, timeScheduleTemplateBlock.StartTime.Hour, timeScheduleTemplateBlock.StartTime.Minute, timeScheduleTemplateBlock.StartTime.Second);
                                            existingProjectTimeBlock.StopTime = existingProjectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                        }
                                        else
                                        {
                                            if (dayStartTime != 0)
                                            {
                                                existingProjectTimeBlock.StartTime = existingProjectTimeBlock.TimeBlockDate.Date.AddMinutes(dayStartTime);
                                                existingProjectTimeBlock.StopTime = existingProjectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                            }
                                            else
                                            {
                                                existingProjectTimeBlock.StartTime = existingProjectTimeBlock.TimeBlockDate.Date.AddHours(7);
                                                existingProjectTimeBlock.StopTime = existingProjectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    existingProjectTimeBlock.StartTime = existingProjectTimeBlock.TimeBlockDate.Date;
                                    existingProjectTimeBlock.StopTime = existingProjectTimeBlock.TimeBlockDate.Date;
                                }

                                SetModifiedProperties(existingProjectTimeBlock);
                                projectTimeBlocks.Add(existingProjectTimeBlock);
                            }
                            else
                            {
                                //Create new
                                TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, base.ActorCompanyId, week.EmployeeId, day.Date, true);
                                TimeDeviationCauseDTO deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeCode != null && t.TimeCode.TimeCodeId == timeCodeTransaction.TimeCodeId);
                                if (deviationCause == null)
                                {
                                    if (employee.TimeDeviationCauseId.HasValue)
                                    {
                                        deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.TimeDeviationCauseId.Value);
                                        if (deviationCause == null && employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                    }
                                    else if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                    {
                                        deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                    }
                                }

                                if (timeBlockDate != null && deviationCause != null)
                                {
                                    ProjectTimeBlock projectTimeBlock = new ProjectTimeBlock()
                                    {
                                        ActorCompanyId = base.ActorCompanyId,
                                        CustomerInvoiceId = week.RecordId,
                                        EmployeeId = employee.EmployeeId,
                                        ExternalNote = day.CommentExternal,
                                        InternalNote = day.Note,
                                        InvoiceQuantity = day.InvoiceTimeInMinutes,
                                        ProjectId = week.ProjectId,
                                        TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                                        TimeDeviationCauseId = deviationCause.TimeDeviationCauseId,
                                        RecordType = week.RecordType,
                                    };

                                    List<ProjectTimeBlock> projectTimeBlocksOnDate = GetProjectTimeBlocks(entities, timeBlockDate.TimeBlockDateId);

                                    if (day.WorkTimeInMinutes > 0)
                                    {
                                        ProjectTimeBlock previousProjectTimeBlock = projectTimeBlocksOnDate.Where(p => p.TimeBlockDateId == timeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();
                                        if (previousProjectTimeBlock == null)
                                            previousProjectTimeBlock = projectTimeBlocks.Where(p => p.TimeBlockDateId == timeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();

                                        if (previousProjectTimeBlock != null)
                                        {
                                            projectTimeBlock.StartTime = previousProjectTimeBlock.StopTime;
                                            projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                        }
                                        else
                                        {
                                            //Set times from schedule
                                            TimeScheduleTemplateBlock timeScheduleTemplateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(entities, employee.EmployeeId, timeBlockDate.Date, true).OrderBy(t => t.StartTime).FirstOrDefault();
                                            if (timeScheduleTemplateBlock != null)
                                            {
                                                projectTimeBlock.StartTime = new DateTime(timeCodeTransaction.ProjectInvoiceDay.Date.Year, timeCodeTransaction.ProjectInvoiceDay.Date.Month, timeCodeTransaction.ProjectInvoiceDay.Date.Day, timeScheduleTemplateBlock.StartTime.Hour, timeScheduleTemplateBlock.StartTime.Minute, timeScheduleTemplateBlock.StartTime.Second);
                                                projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                            }
                                            else
                                            {
                                                if (dayStartTime != 0)
                                                {
                                                    projectTimeBlock.StartTime = timeBlockDate.Date.AddMinutes(dayStartTime);
                                                    projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                                }
                                                else
                                                {
                                                    projectTimeBlock.StartTime = timeBlockDate.Date.AddHours(7);
                                                    projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        projectTimeBlock.StartTime = timeBlockDate.Date;
                                        projectTimeBlock.StopTime = timeBlockDate.Date;
                                    }

                                    //Create projectTimeBlock
                                    projectTimeBlocks.Add(projectTimeBlock);
                                    SetCreatedProperties(projectTimeBlock);
                                    entities.ProjectTimeBlock.AddObject(projectTimeBlock);

                                    //Connect to transaction
                                    timeCodeTransaction.ProjectTimeBlock = projectTimeBlock;
                                    SetModifiedProperties(timeCodeTransaction);
                                }
                            }
                        }
                    }
                }

                #endregion

                result = SaveChanges(entities, transaction);
                if (!result.Success)
                    return result;

                result.Value = projectTimeBlocks;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorMessage = ex.Message;
                result.IntegerValue = 0;
                return result;
            }

            return result;
        }

        public ActionResult ChangeProjectOnInvoice(int actorCompanyId, int projectId, int recordId, int recordType, bool owerwriteDefaultDims = false)
        {
            using (CompEntities entities = new CompEntities())
            {
                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    return ChangeProjectOnInvoice(entities, transaction, actorCompanyId, projectId, recordId, recordType, owerwriteDefaultDims);
                }
            }
        }

        public ActionResult ChangeProjectOnInvoice(CompEntities entities, TransactionScope transaction, int actorCompanyId, int projectId, int recordId, int recordType, bool owerwriteDefaultDims)
        {
            ActionResult result = new ActionResult();

            try
            {
                #region Prereq

                if (projectId == 0)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8326, "Projekt saknas"));

                if (recordId == 0)
                {
                    if (recordType == (int)SoeProjectRecordType.Invoice)
                        return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8371, "Faktura saknas"));
                    else
                        return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8370, "Order saknas"));
                }

                CustomerInvoice invoice = InvoiceManager.GetCustomerInvoiceAndRows(entities, recordId);

                if (invoice == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "CustomerInvoice");

                Project project = GetProject(entities, projectId);

                if (project == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Project");

                int oldProjectId = (int)invoice.ProjectId;

                #endregion

                result = PerformChangeProjectOnInvoice(entities, transaction, project, invoice, oldProjectId, owerwriteDefaultDims);

                #region Child invoices

                // Check if invoice has children
                var mappings = InvoiceManager.GetChildInvoices(entities, recordId);
                foreach (var mapping in mappings)
                {
                    invoice = InvoiceManager.GetCustomerInvoiceAndRows(entities, recordId);

                    if (invoice == null)
                        continue;

                    result = PerformChangeProjectOnInvoice(entities, transaction, project, invoice, oldProjectId, owerwriteDefaultDims);
                }

                #endregion

                if (result.Success)
                    transaction.Complete();
            }
            catch (Exception ex)
            {
                return new ActionResult(false, 0, ex.Message);
            }

            return result;
        }

        public ActionResult DeleteProjectInvoiceWeek(CompEntities entities, TransactionScope transaction, int employeeId, int timeCodeId, int projectId, DateTime date, int recordId, int recordType, bool deleteProjectTimeBlocks = false)
        {
            var week = (from piw in entities.ProjectInvoiceWeek
                            .Include("ProjectInvoiceDay")
                            .Include("ProjectInvoiceDay.TimeCodeTransaction.TimeInvoiceTransaction")
                            .Include("ProjectInvoiceDay.TimeCodeTransaction.TimePayrollTransaction")
                        where piw.ProjectId == projectId &&
                        piw.EmployeeId == employeeId &&
                        piw.RecordId == recordId &&
                        piw.RecordType == recordType &&
                        piw.TimeCodeId.HasValue && piw.TimeCodeId.Value == timeCodeId &&
                        (piw.Date.Year == date.Year && piw.Date.Month == date.Month && piw.Date.Day == date.Day) &&
                        piw.State == (int)SoeEntityState.Active
                        select piw).FirstOrDefault();

            if (week == null)
                return new ActionResult(true); //nothing to save

            return DeleteProjectInvoiceWeek(entities, transaction, week, true, deleteProjectTimeBlocks);
        }

        public ActionResult DeleteProjectInvoiceWeeks(CompEntities entities, TransactionScope transaction, int projectId, int recordId, int recordType, bool deleteProjectTimeBlocks = false)
        {
            ActionResult result = new ActionResult();

            var weeks = (from piw in entities.ProjectInvoiceWeek
                            .Include("ProjectInvoiceDay")
                            .Include("ProjectInvoiceDay.TimeCodeTransaction.TimeInvoiceTransaction")
                            .Include("ProjectInvoiceDay.TimeCodeTransaction.TimePayrollTransaction")
                         where piw.ProjectId == projectId &&
                         piw.RecordId == recordId &&
                         piw.RecordType == recordType &&
                         piw.State == (int)SoeEntityState.Active
                         select piw);

            foreach (ProjectInvoiceWeek week in weeks)
            {
                result = DeleteProjectInvoiceWeek(entities, transaction, week, true, deleteProjectTimeBlocks);
                if (!result.Success)
                    return result;
            }

            return result;
        }

        private ActionResult DeleteProjectInvoiceWeek(CompEntities entities, TransactionScope transaction, ProjectInvoiceWeek week, bool saveChanges, bool deleteProjectTimeBlocks = false)
        {
            ActionResult result = new ActionResult();

            #region Prereq

            if (week == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "ProjectInvoiceWeek");

            if (!week.ProjectInvoiceDay.IsLoaded)
                week.ProjectInvoiceDay.Load();

            foreach (var day in week.ProjectInvoiceDay)
            {
                if (!day.TimeCodeTransaction.IsLoaded)
                    day.TimeCodeTransaction.Load();

                if (day.TimeCodeTransaction != null)
                {
                    foreach (var timeCodeTransaction in day.TimeCodeTransaction)
                    {
                        if (!timeCodeTransaction.TimePayrollTransaction.IsLoaded)
                            timeCodeTransaction.TimePayrollTransaction.Load();
                        if (!timeCodeTransaction.TimeInvoiceTransaction.IsLoaded)
                            timeCodeTransaction.TimeInvoiceTransaction.Load();
                        if (deleteProjectTimeBlocks && !timeCodeTransaction.ProjectTimeBlockReference.IsLoaded)
                            timeCodeTransaction.ProjectTimeBlockReference.Load();
                    }
                }
            }

            #endregion

            #region Delete

            ChangeEntityState(week, SoeEntityState.Deleted);

            foreach (var day in week.ProjectInvoiceDay)
            {
                if (day.TimeCodeTransaction != null)
                {
                    foreach (var timeCodeTransaction in day.TimeCodeTransaction)
                    {
                        ChangeEntityState(timeCodeTransaction, SoeEntityState.Deleted);

                        if (timeCodeTransaction.TimePayrollTransaction != null)
                        {
                            foreach (var item in timeCodeTransaction.TimePayrollTransaction)
                                ChangeEntityState(item, SoeEntityState.Deleted);
                        }

                        if (timeCodeTransaction.TimeInvoiceTransaction != null)
                        {
                            foreach (var item in timeCodeTransaction.TimeInvoiceTransaction)
                                ChangeEntityState(item, SoeEntityState.Deleted);
                        }

                        if (deleteProjectTimeBlocks && timeCodeTransaction.ProjectTimeBlock != null)
                            ChangeEntityState(timeCodeTransaction.ProjectTimeBlock, SoeEntityState.Deleted);
                    }
                }
            }

            #endregion

            if (saveChanges)
                result = SaveChanges(entities, transaction);

            return result;
        }

        public ActionResult PerformChangeProjectOnInvoice(CompEntities entities, TransactionScope transaction, Project project, CustomerInvoice invoice, int oldProjectId, bool overwriteDefaultDims)
        {
            invoice.Project = project;
            invoice.ProjectId = project.ProjectId;

            if (overwriteDefaultDims)
            {
                if (project.DefaultDim1AccountId > 0 || project.DefaultDim2AccountId > 0 || project.DefaultDim3AccountId > 0 || project.DefaultDim4AccountId > 0 || project.DefaultDim5AccountId > 0 || project.DefaultDim6AccountId > 0)
                {
                    invoice.DefaultDim1AccountId = project.DefaultDim1AccountId;
                    invoice.DefaultDim2AccountId = project.DefaultDim2AccountId;
                    invoice.DefaultDim3AccountId = project.DefaultDim3AccountId;
                    invoice.DefaultDim4AccountId = project.DefaultDim4AccountId;
                    invoice.DefaultDim5AccountId = project.DefaultDim5AccountId;
                    invoice.DefaultDim6AccountId = project.DefaultDim6AccountId;
                }
            }

            SetModifiedProperties(invoice);

            #region Weeks and days / Project timeblocks

            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            if (useProjectTimeBlocks)
            {
                var projectTimeBlocks = (from p in entities.ProjectTimeBlock
                                         .Include("TimeCodeTransaction")
                                         where p.CustomerInvoiceId == invoice.InvoiceId
                                         select p);

                foreach (var projectTimeBlock in projectTimeBlocks)
                {
                    projectTimeBlock.Project = project;
                    projectTimeBlock.ProjectId = project.ProjectId;
                    SetModifiedProperties(projectTimeBlock);

                    foreach (var timeCodeTransaction in projectTimeBlock.TimeCodeTransaction)
                    {
                        timeCodeTransaction.Project = project;
                        timeCodeTransaction.ProjectId = project.ProjectId;
                        SetModifiedProperties(timeCodeTransaction);
                    }
                }
            }
            else
            {
                List<ProjectInvoiceWeek> weeks = GetProjectInvoiceWeeks(entities, oldProjectId, invoice.InvoiceId);

                foreach (ProjectInvoiceWeek week in weeks)
                {
                    week.Project = project;
                    week.ProjectId = project.ProjectId;
                    SetModifiedProperties(week);

                    List<ProjectInvoiceDay> days = GetProjectInvoiceDays(entities, week.ProjectInvoiceWeekId);

                    foreach (ProjectInvoiceDay day in days)
                    {
                        List<TimeCodeTransaction> transactions = TimeTransactionManager.GetTimeCodeTransactionsForProjectInvoiceDay(entities, day.ProjectInvoiceDayId);

                        foreach (TimeCodeTransaction timeTransaction in transactions)
                        {
                            timeTransaction.Project = project;
                            timeTransaction.ProjectId = project.ProjectId;
                            SetModifiedProperties(timeTransaction);
                        }
                    }
                }
            }

            #endregion

            #region Invoice rows

            if (!invoice.CustomerInvoiceRow.IsLoaded)
                invoice.CustomerInvoiceRow.Load();

            foreach (CustomerInvoiceRow row in invoice.CustomerInvoiceRow)
            {
                List<TimeCodeTransaction> transactions = TimeTransactionManager.GetTimeCodeTransactionsFromInvoiceRow(entities, row.CustomerInvoiceRowId);

                foreach (TimeCodeTransaction timeTransaction in transactions)
                {
                    timeTransaction.Project = project;
                    timeTransaction.ProjectId = project.ProjectId;
                    SetModifiedProperties(timeTransaction);
                }
            }

            #endregion

            return SaveChanges(entities, transaction);
        }

        #endregion

        #region ProjectInvoiceDay

        public ProjectInvoiceDay GetProjectInvoiceDay(CompEntities entities, int projectInvoiceWeekId, SoeProjectDayType dayType, bool loadInvoiceAndPayRollTransactions = false, bool loadOnlyTimeCodeTransaction = false)
        {
            var dayTypeId = (int)dayType;
            IQueryable<ProjectInvoiceDay> query = entities.ProjectInvoiceDay;

            if (loadInvoiceAndPayRollTransactions)
            {
                query = query.Include("TimeCodeTransaction.TimePayrollTransaction")
                              .Include("TimeCodeTransaction.TimeInvoiceTransaction");
            }
            else if (loadOnlyTimeCodeTransaction)
            {
                query = query.Include("TimeCodeTransaction");
            }

            return (from pid in query
                    where
                        pid.ProjectInvoiceWeekId == projectInvoiceWeekId &&
                        pid.DayType == dayTypeId &&
                        pid.ProjectInvoiceWeek.State == (int)SoeEntityState.Active
                    select pid).FirstOrDefault();
        }

        public ProjectInvoiceDay GetProjectInvoiceDay(CompEntities entities, int projectInvoiceWeekId, DateTime date, bool loadInvoiceAndPayRollTransactions = false, bool loadOnlyTimeCodeTransaction = false)
        {
            SoeProjectDayType dayType = SoeProjectDayType.Monday;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dayType = SoeProjectDayType.Sunday;
                    break;
                case DayOfWeek.Monday:
                    dayType = SoeProjectDayType.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    dayType = SoeProjectDayType.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    dayType = SoeProjectDayType.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    dayType = SoeProjectDayType.Thursday;
                    break;
                case DayOfWeek.Friday:
                    dayType = SoeProjectDayType.Friday;
                    break;
                case DayOfWeek.Saturday:
                    dayType = SoeProjectDayType.Saturday;
                    break;
                default:
                    break;
            }

            return GetProjectInvoiceDay(entities, projectInvoiceWeekId, dayType, loadInvoiceAndPayRollTransactions, loadOnlyTimeCodeTransaction);
        }

        public ProjectInvoiceDay GetProjectInvoiceDay(int projectInvoiceDayId, bool includeInvoiceWeek = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProjectInvoiceDay.NoTracking();
            return GetProjectInvoiceDay(entities, projectInvoiceDayId, includeInvoiceWeek);
        }

        public ProjectInvoiceDay GetProjectInvoiceDay(CompEntities entities, int projectInvoiceDayId, bool includeInvoiceWeek = false, bool loadInvoiceAndPayRollTransactions = false, bool loadOnlyTimeCodeTransaction = false)
        {
            IQueryable<ProjectInvoiceDay> query = entities.ProjectInvoiceDay;

            if (includeInvoiceWeek)
                query = query.Include("ProjectInvoiceWeek");

            if (loadInvoiceAndPayRollTransactions)
            {
                query = query
                            .Include("TimeCodeTransaction.TimePayrollTransaction")
                            .Include("TimeCodeTransaction.TimeInvoiceTransaction");
            }
            else if (loadOnlyTimeCodeTransaction)
            {
                query = query.Include("TimeCodeTransaction");
            }

            return (from pid in query
                    where pid.ProjectInvoiceDayId == projectInvoiceDayId
                    select pid).FirstOrDefault();
        }

        public List<ProjectInvoiceDay> GetProjectInvoiceDays(CompEntities entities, List<int> projectInvoiceDayIds, bool includeInvoiceWeek = false, bool loadInvoiceAndPayRollTransactions = false, bool loadOnlyTimeCodeTransaction = false)
        {
            IQueryable<ProjectInvoiceDay> query = entities.ProjectInvoiceDay;

            if (includeInvoiceWeek)
                query = query.Include("ProjectInvoiceWeek");

            if (loadInvoiceAndPayRollTransactions)
            {
                query = query
                            .Include("TimeCodeTransaction.TimePayrollTransaction")
                            .Include("TimeCodeTransaction.TimeInvoiceTransaction");
            }
            else if (loadOnlyTimeCodeTransaction)
            {
                query = query.Include("TimeCodeTransaction");
            }

            return (from pid in query
                    where projectInvoiceDayIds.Contains(pid.ProjectInvoiceDayId)
                    select pid).ToList();
        }
        #endregion

        #region ProjectTraceView

        public List<ProjectTraceViewDTO> GetProjectTraceViews(int projectId, int baseSysCurrencyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProjectTraceView.NoTracking();
            return GetProjectTraceViews(entities, projectId, baseSysCurrencyId);
        }

        public List<ProjectTraceViewDTO> GetProjectTraceViews(CompEntities entities, int projectId, int baseSysCurrencyId)
        {
            List<ProjectTraceViewDTO> dtos = new List<ProjectTraceViewDTO>();

            var items = (from v in entities.ProjectTraceView
                         where v.ProjectId == projectId
                         select v).ToList();

            if (!items.IsNullOrEmpty())
            {
                int langId = GetLangId();
                var originTypes = base.GetTermGroupDict(TermGroup.OriginType, langId);
                var originStatuses = base.GetTermGroupDict(TermGroup.OriginStatus, langId);
                var paymentStatuses = base.GetTermGroupDict(TermGroup.PaymentStatus, langId);

                foreach (var item in items)
                {
                    var dto = item.ToDTO();
                    dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);

                    if (dto.IsPayment)
                    {
                        dto.OriginTypeName = dto.OriginType != 0 ? originTypes[(int)dto.OriginType] : "";
                        dto.OriginStatusName = dto.OriginStatus != 0 ? originStatuses[(int)dto.OriginStatus] : "";
                        dto.BillingTypeName = dto.BillingType != 0 ? originStatuses[(int)dto.BillingType] : "";
                        dto.PaymentStatusName = dto.PaymentStatusId != 0 ? paymentStatuses[(int)dto.PaymentStatusId] : "";
                    }
                    else
                    {
                        dto.OriginTypeName = dto.OriginType != 0 ? originTypes[(int)dto.OriginType] : "";
                        dto.OriginStatusName = dto.OriginStatus != 0 ? originStatuses[(int)dto.OriginStatus] : "";
                        dto.BillingTypeName = dto.BillingType != 0 ? originStatuses[(int)dto.BillingType] : "";
                    }

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        #endregion

        #region ProjectUser

        public IEnumerable<ProjectUser> GetProjectUsers(int projectId, bool setTypeName)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProjectUser.NoTracking();
            return GetProjectUsers(entities, projectId, setTypeName);
        }

        public IEnumerable<ProjectUser> GetProjectUsers(CompEntities entities, int projectId, bool setTypeName)
        {
            IEnumerable<ProjectUser> users = (from p in entities.ProjectUser
                                              where p.ProjectId == projectId &&
                                              p.State == (int)SoeEntityState.Active
                                              orderby p.Type, p.DateFrom, p.DateTo
                                              select p);

            // Set TypeName extension
            if (setTypeName)
            {
                // Get all types in term group once
                List<GenericType> types = base.GetTermGroupContent(TermGroup.ProjectUserType, skipUnknown: false);
                foreach (var user in users)
                {
                    GenericType type = types.FirstOrDefault(t => t.Id == user.Type);
                    user.TypeName = type != null ? type.Name : String.Empty;
                }
            }

            return users;
        }

        public List<ProjectUserDTO> GetProjectUsersForAngular(int projectId, int actorCompanyId, bool setTypeName)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProjectUser.NoTracking();
            return GetProjectUsersForAngular(entities, projectId, actorCompanyId, setTypeName);
        }

        public List<ProjectUserDTO> GetProjectUsersForAngular(CompEntities entities, int projectId, int actorCompanyId, bool setTypeName)
        {
            bool calculatedCostPermission = FeatureManager.HasRolePermission(Feature.Billing_Project_EmployeeCalculateCost, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId, entities);

            List<ProjectUserDTO> users = (from p in entities.ProjectUser
                                          where
                                           p.ProjectId == projectId &&
                                           p.State == (int)SoeEntityState.Active &&
                                           p.Project.ActorCompanyId == actorCompanyId
                                          orderby p.Type, p.DateFrom, p.DateTo
                                          select new ProjectUserDTO()
                                          {
                                              ProjectUserId = p.ProjectUserId,
                                              ProjectId = p.ProjectId,
                                              UserId = p.UserId,
                                              Name = p.User.ContactPerson != null ? p.User.ContactPerson.FirstName + " " + p.User.ContactPerson.LastName : p.User.Name,
                                              TimeCodeId = p.TimeCodeId,
                                              TimeCodeName = p.TimeCode.Name,
                                              Type = (TermGroup_ProjectUserType)p.Type,
                                              DateFrom = p.DateFrom,
                                              DateTo = p.DateTo
                                          }).ToList();

            // Set TypeName extension
            if (setTypeName)
            {
                // Get all types in term group once
                List<GenericType> types = base.GetTermGroupContent(TermGroup.ProjectUserType);
                users.ForEach(u =>
               {
                   u.TypeName = types.FirstOrDefault(t => t.Id == (int)u.Type)?.Name ?? string.Empty;

               });
            }

            if (calculatedCostPermission)
            {
                users.ForEach(u =>
                {
                    if (u.DateFrom.HasValue)
                    {
                        var employee = EmployeeManager.GetEmployeeForUser(entities, u.UserId, actorCompanyId);
                        if (employee != null)
                        {
                            u.EmployeeCalculatedCost = EmployeeManager.GetEmployeeCalculatedCost(employee, u.DateFrom.Value, u.ProjectId);
                        }
                    }
                });
            }

            return users;
        }

        #endregion

        #region TimeProject

        private bool UserIsProjectUser(CompEntities entities, int projectId, int userId)
        {
            return (from p in entities.ProjectUser
                    where p.ProjectId == projectId &&
                    p.UserId == userId &&
                    p.State == (int)SoeEntityState.Active
                    select p).Any();
        }

        private int GetProjectUsersCount(CompEntities entities, int projectId)
        {
            return (from p in entities.ProjectUser
                    where p.ProjectId == projectId &&
                    p.State == (int)SoeEntityState.Active
                    select p).Count();
        }

        public List<Employee> GetEmployeesForTimeProjectRegistration(int actorCompanyId, int userId, int roleId, int projectId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            List<Employee> employees = new List<Employee>();

            bool limitOrderToProjectUsers = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectLimitOrderToProjectUsers, userId, actorCompanyId, 0, defaultValue: true);
            bool extendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, actorCompanyId, 0);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            List<Employee> employeesForUsersAttestRole = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, addEmployeeGroupInfo: extendedTimeRegistration);

            if (extendedTimeRegistration)
            {
                if (fromDate.HasValue && toDate.HasValue)
                    employeesForUsersAttestRole = employeesForUsersAttestRole.Where(e1 => e1.GetEmployeeGroup(fromDate.Value, toDate.Value, employeeGroups, forward: false).IsTimeReportTypeERP()).ToList();
                else
                    employeesForUsersAttestRole = employeesForUsersAttestRole.Where(e1 => e1.GetEmployeeGroup(null, employeeGroups).IsTimeReportTypeERP()).ToList();
            }

            int count = GetProjectUsersCount(entitiesReadOnly, projectId);

            #region Decide Which employees are allowed to registrate time

            if (count > 0 && limitOrderToProjectUsers)
            {
                var projectUsers = (from p in entitiesReadOnly.ProjectUser
                                    where p.ProjectId == projectId &&
                                    p.State == (int)SoeEntityState.Active
                                    select new { UserId = p.UserId, TimeCodeId = p.TimeCodeId, StartDate = p.DateFrom, StopDate = p.DateTo }).ToList();


                foreach (var employee in employeesForUsersAttestRole.Where(e => e.UserId.HasValue))
                {
                    var projectUser = projectUsers.FirstOrDefault(x => x.UserId == employee.UserId);
                    if (projectUser != null)
                    {
                        if ((projectUser.StartDate.HasValue && projectUser.StartDate > DateTime.Today) || (projectUser.StopDate.HasValue && projectUser.StopDate < DateTime.Today))
                            continue;

                        int? defaultTimeCodeId = projectUsers.FirstOrDefault(x => x.UserId == employee.UserId)?.TimeCodeId;
                        if (defaultTimeCodeId.HasValue)
                            employee.ProjectDefaultTimeCodeId = defaultTimeCodeId.Value;

                        employees.Add(employee);
                    }
                }

            }
            else
                employees.AddRange(employeesForUsersAttestRole);

            #endregion

            #region Registration Decide TimeCode

            foreach (Employee employee in employees.Where(e => e.ProjectDefaultTimeCodeId == 0))
            {
                EmployeeManager.SetEmployeeProjectDefaultTimeCodeId(employee, employeeGroups);
            }

            #endregion

            return employees.OrderBy(e => e.Name).ToList();
        }

        public IEnumerable<EmployeeDTO> GetEmployeesForTimeProjectRegistration(int roleId, int projectId)
        {
            ProjectManager pm = new ProjectManager(this.parameterObject);
            TimeCodeManager tcm = new TimeCodeManager(this.parameterObject);
            PayrollManager pam = new PayrollManager(this.parameterObject);
            EmployeeManager em = new EmployeeManager(this.parameterObject);

            List<EmployeeGroup> employeeGroups = null;
            List<PayrollGroup> payrollGroups = null;
            List<PayrollPriceType> payrollPriceTypes = null;

            DateTime start = DateTime.Now;
            var employees = pm.GetEmployeesForTimeProjectRegistration(base.ActorCompanyId, base.UserId, roleId, projectId);

            if (employees.Count > 3)
            {
                payrollGroups = pam.GetPayrollGroups(base.ActorCompanyId);
                employeeGroups = em.GetEmployeeGroups(base.ActorCompanyId);
                payrollPriceTypes = pam.GetPayrollPriceTypes(base.ActorCompanyId, null, false);
            }

            var dtos = employees.ToDTOs(includeEmployments: true, includeEmployeeGroup: true, employeeGroups: employeeGroups, payrollGroups: payrollGroups, payrollPriceTypes: payrollPriceTypes);


            foreach (EmployeeDTO employee in dtos)
            {
                foreach (EmploymentDTO employment in employee.Employments)
                {
                    List<TimeCode> codes = tcm.GetTimeCodesForEmployeeGroup(base.ActorCompanyId, employment.EmployeeGroupId);

                    if (codes != null && codes.Count > 0)
                        employment.EmployeeGroupTimeCodes = codes.Select(c => c.TimeCodeId).ToList();
                }
            }

            return dtos;
        }

        public Dictionary<int, string> GetEmployeesForTimeProjectRegistrationDict(int roleId, int projectId)
        {
            return GetEmployeesForTimeProjectRegistration(base.ActorCompanyId, base.UserId, roleId, projectId).ToDictionary(e => e.EmployeeId, e => e.FirstName + " " + e.LastName);
        }

        public List<EmployeeTimeCodeDTO> GetEmployeesForTimeProjectRegistrationSmall(int roleId, int projectId, DateTime? fromDate, DateTime? toDate)
        {
            return GetEmployeesForTimeProjectRegistration(base.ActorCompanyId, base.UserId, roleId, projectId, fromDate, toDate).ToEmployeeTimeCodeDTOs(base.GetEmployeeGroupsFromCache(base.ActorCompanyId));
        }

        public int ProjectInvoicesCount(CompEntities entities, int projectId)
        {
            return (from i in entities.Invoice
                    where (i.ProjectId != null && (int)i.ProjectId == projectId) &&
                    i.State == 0
                    select i).Count();
        }

        public TimeProject GetTimeProject(int projectId, bool includeAccountSettings)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.OfType<TimeProject>().AsNoTracking();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeProject(entities, projectId, includeAccountSettings, true);
        }

        public TimeProject GetTimeProject(CompEntities entities, int projectId, bool includeAccountSettings, bool loadBudgets = false)
        {
            TimeProject project;
            if (includeAccountSettings)
            {
                project = (from p in entities.Project.OfType<TimeProject>()
                           .Include("Customer")
                           .Include("ParentProject")
                           .Include("ProjectAccountStd.AccountStd.Account")
                           .Include("ProjectAccountStd.AccountInternal.Account.AccountDim")
                           where p.ProjectId == projectId && p.ActorCompanyId == ActorCompanyId
                           select p).FirstOrDefault();
            }
            else
            {
                project = (from p in entities.Project.OfType<TimeProject>()
                           .Include("Customer")
                           .Include("ParentProject")
                           where p.ProjectId == projectId && p.ActorCompanyId == ActorCompanyId
                           select p).FirstOrDefault();
            }

            if (project != null && loadBudgets)
            {
                if (!project.BudgetHead.IsLoaded)
                    project.BudgetHead.Load();

                foreach (BudgetHead head in project.BudgetHead)
                {
                    if (!head.BudgetRow.IsLoaded)
                    {
                        head.BudgetRow.Load();
                        foreach (BudgetRow row in head.BudgetRow)
                        {
                            if (row.TimeCodeId != null)
                            {
                                row.TimeCodeReference.Load();
                            }
                        }
                    }

                }
            }

            // Set status name extension
            if (project != null)
                project.StatusName = GetText(project.Status, (int)TermGroup.ProjectStatus);

            return project;
        }

        public List<ProjectInvoiceDay> GetProjectInvoiceDaysWithWeek(int actorCompanyId, int orderId, int userId)
        {
            int projectId = GetProjectId(orderId);
            if (projectId == -1)
                return new List<ProjectInvoiceDay>();

            Employee employee = EmployeeManager.GetEmployeeByUser(actorCompanyId, userId);
            if (employee == null)
                return new List<ProjectInvoiceDay>();

            List<ProjectInvoiceDay> days = GetProjectInvoiceDaysWithWeekByProjectId(projectId, orderId, employee.EmployeeId);

            return days;
        }

        public ActionResult SaveProjectTimeBlockSaveDTO(ProjectTimeBlockSaveDTO item)
        {
            ActionResult result = new ActionResult();

            var fixedItem = FixProjectTimeBlockSaveDTO(item);

            if (fixedItem == null)
            {
                result.Success = false;
                result.ErrorMessage = "Insufficient information";
                return result;
            }

            try
            {
                if (item.CustomerInvoiceId.HasValue)
                {
                    if (item.State == SoeEntityState.Deleted)
                        result = SaveProjectInvoiceDay(item.ActorCompanyId, parameterObject.RoleId, item.CustomerInvoiceId.Value, item.Date.Value, 0, 0, "", item.TimeCodeId, commentExternal: "", employeeId: item.EmployeeId);
                    // SaveProjectInvoiceDayFromTimeSheet(transaction, entities, true, true, item.ActorCompanyId, item.EmployeeId, item.CustomerInvoiceId.Value, item.Date.Value, 0, 0, item.InternalNote, item.TimeCodeId, item.ExternalNote);                        
                    else
                        //External and Internal note is switch because of CreateTransactionsForTimeProjectRegistration that is called later will switch it back...
                        result = SaveProjectInvoiceDay(item.ActorCompanyId, parameterObject.RoleId, item.CustomerInvoiceId.Value, item.Date.Value, Convert.ToInt32(item.InvoiceQuantity), Convert.ToInt32(item.TimePayrollQuantity), item.ExternalNote, item.TimeCodeId, commentExternal: item.InternalNote, employeeId: item.EmployeeId, ignoreExternalComment: false);
                    // SaveProjectInvoiceDayFromTimeSheet(transaction, entities, true, true, item.ActorCompanyId, item.EmployeeId, item.CustomerInvoiceId.Value, item.Date.Value, Convert.ToInt32(item.InvoiceQuantity), Convert.ToInt32(item.TimePayrollQuantity), item.InternalNote, item.TimeCodeId, item.ExternalNote);

                    if (!result.Success)
                        return result;
                }
                else if (item.isFromTimeSheet)
                {
                    if (item.State != SoeEntityState.Deleted)
                    {
                        TimeSheetDTO dto = new TimeSheetDTO()
                        {
                            ProjectInvoiceWeekId = item.ProjectInvoiceWeekId,
                            RowNr = 0,
                            WeekStartDate = CalendarUtility.GetFirstDateOfWeek(item.Date.Value),
                            ProjectId = item.ProjectId.HasValue ? item.ProjectId.Value : 0,
                            CustomerId = 0,
                            InvoiceId = item.CustomerInvoiceId.HasValue ? item.CustomerInvoiceId.Value : 0,
                            TimeCodeId = item.TimeCodeId,
                        };

                        switch (CalendarUtility.GetDayOfWeek(item.Date.Value))
                        {
                            case DayOfWeek.Monday:
                                dto.MondayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                dto.Monday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                dto.MondayNote = item.InternalNote;
                                dto.MondayNoteExternal = item.ExternalNote;
                                break;
                            case DayOfWeek.Tuesday:
                                dto.TuesdayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                dto.Tuesday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                dto.TuesdayNote = item.InternalNote;
                                dto.TuesdayNoteExternal = item.ExternalNote;
                                break;
                            case DayOfWeek.Wednesday:
                                dto.WednesdayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                dto.Wednesday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                dto.WednesdayNote = item.InternalNote;
                                dto.WednesdayNoteExternal = item.ExternalNote;
                                break;
                            case DayOfWeek.Thursday:
                                dto.ThursdayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                dto.Thursday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                dto.ThursdayNote = item.InternalNote;
                                dto.ThursdayNoteExternal = item.ExternalNote;
                                break;
                            case DayOfWeek.Friday:
                                dto.FridayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                dto.Friday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                dto.FridayNote = item.InternalNote;
                                dto.FridayNoteExternal = item.ExternalNote;
                                break;
                            case DayOfWeek.Saturday:
                                dto.SaturdayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                dto.Saturday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                dto.SaturdayNote = item.InternalNote;
                                dto.SaturdayNoteExternal = item.ExternalNote;
                                break;
                            case DayOfWeek.Sunday:
                                dto.SundayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                dto.Sunday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                dto.SundayNote = item.InternalNote;
                                dto.SundayNoteExternal = item.ExternalNote;
                                break;
                        }

                        result = SaveTimeSheet(new List<TimeSheetDTO>() { dto }, item.EmployeeId, item.ActorCompanyId, 0);

                        if (!result.Success)
                        {
                            result.Success = false;
                            result.ErrorMessage = "SaveTimeSheet failed";
                            return result;
                        }
                    }
                    else
                    {
                        using (CompEntities entities = new CompEntities())
                        {
                            TimeSheetWeek week = GetTimeSheetWeekWithTransactions(entities, item.TimeSheetWeekId.Value);
                            DeleteTimeSheetWeekTransactions(week, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = "SaveProjectTimeBlockSaveDTOs failed";
                LogError(ex, log);
                return result;

            }

            return result;
        }

        public ActionResult SaveProjectTimeBlockSaveDTOs(List<ProjectTimeBlockSaveDTO> projectTimeBlockSaveDTOs)
        {
            var result = new ActionResult();

            projectTimeBlockSaveDTOs = MergeProjectTimeBlockSaveDTOs(projectTimeBlockSaveDTOs);

            foreach (var item in projectTimeBlockSaveDTOs.ToList())
            {
                if (item.EmployeeId == 0)
                {
                    return new ActionResult { Success = false, ErrorMessage = GetText(5027, "Anställd hittades inte") };
                }

                var fixedItem = FixProjectTimeBlockSaveDTO(item);

                if (fixedItem == null)
                {
                    return new ActionResult { Success = false, ErrorMessage = "Insufficient information" };
                }
            }

            try
            {
                foreach (var item in projectTimeBlockSaveDTOs)
                {
                    if (item.CustomerInvoiceId.HasValue && item.CustomerInvoiceId.Value > 0)
                    {
                        if (item.ProjectInvoiceDayId == 0 && item.ProjectInvoiceWeekId > 0 && item.ProjectTimeBlockId > 0)
                        {
                            item.ProjectInvoiceDayId = item.ProjectTimeBlockId;
                        }

                        if (item.State == SoeEntityState.Deleted)
                            result = SaveProjectInvoiceDay(item.ActorCompanyId, parameterObject.RoleId, item.CustomerInvoiceId.Value, item.Date.Value, 0, 0, "", item.TimeCodeId, "", item.EmployeeId, true, false, item.ProjectInvoiceWeekId, item.ProjectInvoiceDayId);
                        else
                            //External and Internal note is switch because of CreateTransactionsForTimeProjectRegistration that is called later will switch it back...
                            result = SaveProjectInvoiceDay(item.ActorCompanyId, parameterObject.RoleId, item.CustomerInvoiceId.Value, item.Date.Value, Convert.ToInt32(item.InvoiceQuantity), Convert.ToInt32(item.TimePayrollQuantity), item.ExternalNote, item.TimeCodeId, item.InternalNote, item.EmployeeId, false, false, item.ProjectInvoiceWeekId, item.ProjectInvoiceDayId);

                        if (!result.Success)
                            return result;
                    }
                    else if (item.isFromTimeSheet)
                    {
                        if (item.State == SoeEntityState.Deleted)
                        {
                            using (CompEntities entities = new CompEntities())
                            {
                                TimeSheetWeek week = GetTimeSheetWeekWithTransactions(entities, item.TimeSheetWeekId.Value);
                                DeleteTimeSheetWeekTransactions(week, false);
                                entities.SaveChanges();
                            }
                        }
                        else
                        {
                            TimeSheetDTO dto = new TimeSheetDTO()
                            {
                                ProjectInvoiceWeekId = item.ProjectInvoiceWeekId,
                                RowNr = 0,
                                WeekStartDate = CalendarUtility.GetFirstDateOfWeek(item.Date.Value),
                                ProjectId = item.ProjectId.HasValue ? item.ProjectId.Value : 0,
                                CustomerId = 0,
                                InvoiceId = item.CustomerInvoiceId.HasValue ? item.CustomerInvoiceId.Value : 0,
                                TimeCodeId = item.TimeCodeId,
                            };

                            switch (CalendarUtility.GetDayOfWeek(item.Date.Value))
                            {
                                case DayOfWeek.Monday:
                                    dto.MondayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                    dto.Monday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                    dto.MondayNote = item.InternalNote;
                                    dto.MondayNoteExternal = item.ExternalNote;
                                    break;
                                case DayOfWeek.Tuesday:
                                    dto.TuesdayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                    dto.Tuesday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                    dto.TuesdayNote = item.InternalNote;
                                    dto.TuesdayNoteExternal = item.ExternalNote;
                                    break;
                                case DayOfWeek.Wednesday:
                                    dto.WednesdayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                    dto.Wednesday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                    dto.WednesdayNote = item.InternalNote;
                                    dto.WednesdayNoteExternal = item.ExternalNote;
                                    break;
                                case DayOfWeek.Thursday:
                                    dto.ThursdayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                    dto.Thursday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                    dto.ThursdayNote = item.InternalNote;
                                    dto.ThursdayNoteExternal = item.ExternalNote;
                                    break;
                                case DayOfWeek.Friday:
                                    dto.FridayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                    dto.Friday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                    dto.FridayNote = item.InternalNote;
                                    dto.FridayNoteExternal = item.ExternalNote;
                                    break;
                                case DayOfWeek.Saturday:
                                    dto.SaturdayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                    dto.Saturday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                    dto.SaturdayNote = item.InternalNote;
                                    dto.SaturdayNoteExternal = item.ExternalNote;
                                    break;
                                case DayOfWeek.Sunday:
                                    dto.SundayActual = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.TimePayrollQuantity));
                                    dto.Sunday = CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(item.InvoiceQuantity));
                                    dto.SundayNote = item.InternalNote;
                                    dto.SundayNoteExternal = item.ExternalNote;
                                    break;
                            }

                            result = SaveTimeSheet(new List<TimeSheetDTO>() { dto }, item.EmployeeId, item.ActorCompanyId, 0);

                            if (!result.Success)
                            {
                                result.Success = false;
                                result.ErrorMessage = "SaveTimeSheet failed";
                                return result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = "SaveProjectTimeBlockSaveDTOs failed";
                LogError(ex, log);
                return result;

            }


            return result;

        }

        private bool ValidateStartStopDate(DateTime startTime, DateTime stopTime, bool allowZero, bool blockTimeBlockWithZeroStartTime)
        {
            if ((startTime.Year != 1900) || (stopTime.Year != 1900) ||
                    (startTime.Month != 1) || (stopTime.Month != 1) ||
                    (startTime.Day > 2) || (stopTime.Day > 2)
                )
            {
                return false;
            }

            if (!allowZero && startTime.Day == stopTime.Day && startTime.Hour == stopTime.Hour && startTime.Minute == stopTime.Minute)
            {
                return false;
            }

            if (blockTimeBlockWithZeroStartTime && startTime.Hour == 0 && startTime.Minute == 0)
            {
                return false;
            }

            return true;
        }

        public ActionResult ValidateProjectTimeBlockSaveData(List<ProjectTimeBlockSaveDTO> saveDTOs)
        {
            var validateBlocks = new List<ValidateProjectTimeBlockSaveDTO>();

            foreach (var dtoByEmployee in saveDTOs.Where(x => x.State != SoeEntityState.Deleted).GroupBy(g => g.EmployeeId))
            {
                var first = dtoByEmployee.FirstOrDefault();

                var saveValidateDto = new ValidateProjectTimeBlockSaveDTO
                {
                    EmployeeId = first.EmployeeId,
                    AutoGenTimeAndBreakForProject = first.AutoGenTimeAndBreakForProject,
                    Rows = new List<ValidateProjectTimeBlockSaveRowDTO>()
                };

                foreach (var dto in dtoByEmployee)
                {
                    var validateProjectTimeBlockRowDTO = new ValidateProjectTimeBlockSaveRowDTO
                    {
                        Id = dto.ProjectTimeBlockId,
                        StartTime = dto.From.Value,
                        StopTime = dto.To.Value.AddMinutes(decimal.ToDouble(dto.TimePayrollQuantity)),
                        WorkDate = dto.Date.GetValueOrDefault(),
                        TimeDeviationCauseId = dto.TimeDeviationCauseId.GetValueOrDefault(),
                        EmployeeChildId = dto.EmployeeChildId,

                        //OriginalStartTime = dto.From,
                        //OriginalStopTime = existingProjectTimeBlock?.StopTime,
                        //OriginalTimeDeviationCauseId = existingProjectTimeBlock?.TimeDeviationCauseId
                    };

                    saveValidateDto.Rows.Add(validateProjectTimeBlockRowDTO);
                }

                validateBlocks.Add(saveValidateDto);
            }

            return ValidateSaveProjectTimeBlocks(validateBlocks, false);
        }

        public ActionResult ValidateProjectTimeBlockData(ProjectTimeBlock existingProjectTimeBlock, int employeeId, DateTime date, DateTime startTime, DateTime stopTime, int timeDeviationCauseId, int? employeeChildId, bool autoGenTimeAndBreakForProject, bool fromApp)
        {

            var validateProjectTimeBlockRowDTO = new ValidateProjectTimeBlockSaveRowDTO
            {
                Id = existingProjectTimeBlock == null ? 0 : existingProjectTimeBlock.ProjectTimeBlockId,
                StartTime = startTime,
                StopTime = stopTime,
                WorkDate = date,
                TimeDeviationCauseId = timeDeviationCauseId,
                EmployeeChildId = employeeChildId,
                OriginalStartTime = existingProjectTimeBlock?.StartTime,
                OriginalStopTime = existingProjectTimeBlock?.StopTime,
                OriginalTimeDeviationCauseId = existingProjectTimeBlock?.TimeDeviationCauseId
            };

            var saveDto = new ValidateProjectTimeBlockSaveDTO
            {
                EmployeeId = employeeId,
                AutoGenTimeAndBreakForProject = autoGenTimeAndBreakForProject,
                Rows = new List<ValidateProjectTimeBlockSaveRowDTO> { validateProjectTimeBlockRowDTO }
            };

            return ValidateSaveProjectTimeBlocks(new List<ValidateProjectTimeBlockSaveDTO>() { saveDto }, fromApp);
        }

        public ActionResult ValidateSaveProjectTimeBlocks(List<ValidateProjectTimeBlockSaveDTO> items, bool fromApp)
        {
            var result = new ActionResult();
            var errorList = new List<string>();
            var warningList = new List<string>();

            using (var entities = new CompEntities())
            {
                var timeDeviationCauses = base.GetTimeDeviationCausesFromCache(entities, CacheConfig.Company(ActorCompanyId));
                var additionTimeDeviationCauseIds = timeDeviationCauses.Where(x => x.CalculateAsOtherTimeInSales).Select(x => x.TimeDeviationCauseId).ToList();

                foreach (ValidateProjectTimeBlockSaveDTO item in items)
                {
                    var employee = EmployeeManager.GetEmployee(entities, item.EmployeeId, base.ActorCompanyId, loadContactPerson: true);
                    if (employee == null)
                    {
                        errorList.Add(GetText(5027, "Anställd hittades inte"));
                        break;
                    }

                    foreach (var timeItem in item.Rows)
                    {

                        var timeDiviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == timeItem.TimeDeviationCauseId);
                        if (timeDiviationCause == null)
                        {
                            errorList.Add(GetText(10119, "Orsak hittades inte") + ":" + timeItem.TimeDeviationCauseId.ToString());
                            result.Success = false;
                            break;
                        }

                        if (!item.AutoGenTimeAndBreakForProject || timeDiviationCause.MandatoryTime)
                        {
                            if (!ValidateStartStopDate(timeItem.StartTime, timeItem.StopTime, false, false))
                            {
                                errorList.Add($"Ogiltigt datum för start eller stopp tid {timeItem.StartTime.ToShortDateTime()} {timeItem.StopTime.ToShortDateTime()}");
                                result.Success = false;
                                break;
                            }

                            //Validate overlapping times - check input but not additionaltimes since time dosent matter
                            if (item.Rows.Where(x => !additionTimeDeviationCauseIds.Contains(x.TimeDeviationCauseId)).Any(i => i.Id != timeItem.Id && i.WorkDate == timeItem.WorkDate && ((i.StartTime >= timeItem.StartTime && i.StartTime < timeItem.StopTime) || (i.StopTime > timeItem.StartTime && i.StopTime <= timeItem.StopTime) || (i.StartTime <= timeItem.StartTime && i.StopTime >= timeItem.StopTime) || (i.StartTime >= timeItem.StartTime && i.StopTime <= timeItem.StopTime))))
                            {
                                errorList.Add(string.Format(GetText(11699, "En eller flera rader {0} för {1} har tider som överlappar varandra.\nKontrollera tiderna och försök spara igen."), timeItem.WorkDate.ToShortDateString(), "(" + employee.EmployeeNr + ") " + employee.FirstName + " " + employee.LastName));
                                result.Success = false;
                                break;
                            }
                        }

                        //Check for children...
                        if (timeDiviationCause.SpecifyChild && timeItem.EmployeeChildId.GetValueOrDefault() == 0)
                        {
                            errorList.Add(timeDiviationCause.Name + ": " + GetText(8814, "Du måste ange barn"));
                            result.Success = false;
                            break;
                        }

                        if (timeDiviationCause.IsAbsence && !timeDiviationCause.IsPresence && timeItem.StartTime == timeItem.StopTime)
                        {
                            errorList.Add(GetText(7487, "Arbetad tid kan inte vara 0 vid frånvaro"));
                            result.Success = false;
                            break;
                        }
                    }

                    if (result.Success)
                    {
                        var dates = item.Rows.Select(x => x.WorkDate).Distinct().ToList();

                        var timeBlockDates = TimeBlockManager.GetTimeBlockDates(entities, item.EmployeeId, dates);

                        if (timeBlockDates.Any())
                        {
                            var attestStateInitialPayrollId = AttestManager.GetInitialAttestStateId(entities, base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
                            var payrollTransactionsForAllDates = TimeTransactionManager.GetTimePayrollTransactionsForEmployee(entities, item.EmployeeId, timeBlockDates.Select(d => d.TimeBlockDateId).ToList());
                            foreach (var timeBlockDate in timeBlockDates)
                            {
                                var payrollTransactions = payrollTransactionsForAllDates.Where(t => t.TimeBlockDateId == timeBlockDate.TimeBlockDateId);

                                if (result.Success)
                                {
                                    var currentProjectTimeBlocks = GetProjectTimeBlocksView(entities, timeBlockDate.TimeBlockDateId, item.EmployeeId);

                                    foreach (var timeItem in item.Rows.Where(i => i.WorkDate == timeBlockDate.Date))
                                    {
                                        // Validate if reporting absence outside of schedule
                                        if (
                                                 (timeItem.OriginalStartTime.HasValue && DateTime.Compare(timeItem.OriginalStartTime.Value, timeItem.StartTime) != 0) ||
                                                (timeItem.OriginalStopTime.HasValue && DateTime.Compare(timeItem.OriginalStopTime.Value, timeItem.StopTime) != 0) ||
                                                (!timeItem.OriginalStartTime.HasValue && (timeItem.StartTime != timeItem.StopTime)) ||
                                                (timeItem.OriginalTimeDeviationCauseId.HasValue && timeItem.TimeDeviationCauseId != timeItem.OriginalTimeDeviationCauseId)
                                            )
                                        {
                                            // Validate if dates are attested
                                            if (payrollTransactions.Any(p => p.AttestStateId != attestStateInitialPayrollId && p.TimeBlockId.HasValue))
                                            {
                                                result.Success = false;
                                                errorList.Add(timeBlockDate.Date.ToShortDateString() + " " + GetText(11698, "är attesterad för anställd") + " " + employee.EmployeeNr + " " + employee.FirstName + " " + employee.LastName);
                                            }
                                        }

                                        var timeDiviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == timeItem.TimeDeviationCauseId);

                                        //Warn if reporting over planned schedule
                                        var existingProjectTimeBlocks = currentProjectTimeBlocks.Where(t => t.ProjectTimeBlockId != timeItem.Id && !t.CalculateAsOtherTimeInSales);
                                        var existingFixedProjectTimeBlocks = existingProjectTimeBlocks.Where(p => p.MandatoryTime);

                                        DateTime timeFrom = timeItem.StartTime.RemoveSeconds();
                                        DateTime timeTo = timeItem.StopTime.RemoveSeconds();

                                        if (!timeDiviationCause.CalculateAsOtherTimeInSales)
                                        {
                                            var totalScheduledWorkMinutesForDate = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(employee.EmployeeId, timeItem.WorkDate, false, null).GetWorkMinutes();
                                            var totalPreviousReportedMinutes = existingProjectTimeBlocks.Select(p => CalendarUtility.TimeSpanToMinutes(p.StopTime, p.StartTime)).Sum(y => y);

                                            var itemReportedMinutes = CalendarUtility.TimeSpanToMinutes(timeItem.StopTime, timeItem.StartTime);
                                            if ((totalPreviousReportedMinutes + itemReportedMinutes) > totalScheduledWorkMinutesForDate)
                                            {
                                                var totalTimeString = CalendarUtility.ToTime(DateTime.MinValue.AddMinutes(totalScheduledWorkMinutesForDate));
                                                var previousTimeString = CalendarUtility.ToTime(DateTime.MinValue.AddMinutes(totalPreviousReportedMinutes + itemReportedMinutes));
                                                warningList.Add(string.Format(GetText(7459, "Totalt rapporterad tid ({0}) för {1} är större än planerad tid ({2}) för dagen"), previousTimeString, timeItem.WorkDate.ToShortDateString(), totalTimeString));
                                            }
                                        }

                                        if (timeDiviationCause.OnlyWholeDay)
                                        {
                                            var totalScheduledWorkMinutesForDate = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(employee.EmployeeId, timeItem.WorkDate, false, null).Where(b => !b.IsBooking()).ToList().GetWorkMinutes();
                                            var itemReportedMinutes = CalendarUtility.TimeSpanToMinutes(timeItem.StopTime, timeItem.StartTime);

                                            if (itemReportedMinutes < totalScheduledWorkMinutesForDate)
                                                errorList.Add(GetText(9650, "Vald orsak kräver att arbetad tid avser hela dagen"));
                                        }

                                        if (!item.AutoGenTimeAndBreakForProject || timeDiviationCause.MandatoryTime)
                                        {
                                            //Check Absence diviation code...
                                            if (timeDiviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                                            {

                                                var userEmployeeId = EmployeeManager.GetEmployeeIdForUser(entities, base.UserId, ActorCompanyId);
                                                var timeBlockDate2 = timeItem.WorkDate;
                                                var timeBlockDateTimeStart = timeBlockDate2.Add(timeItem.StartTime.TimeOfDay);
                                                var timeBlockDataTimeStop = timeBlockDate2.Add(timeItem.StopTime.TimeOfDay);
                                                var scheduledShifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(entities, ActorCompanyId, base.UserId, userEmployeeId, base.RoleId, timeBlockDate2,
                                                    timeBlockDate2, new List<int> { item.EmployeeId }, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, false, false, false, false, false, false, false, false, null);

                                                var nonMatchedShifts = scheduledShifts.Where(x => x.StartTime > timeBlockDateTimeStart || x.StopTime < timeBlockDataTimeStop);
                                                if (nonMatchedShifts.Any())
                                                {
                                                    var shift = nonMatchedShifts.First();
                                                    errorList.Add(string.Format(GetText(7428, "Rapporterad frånvarotid ligger utanför schemat {0} - {1}"), shift.StartTime.ToShortTimeString(), shift.StopTime.ToShortTimeString()));
                                                    break;
                                                }
                                            }

                                            if (timeFrom != timeTo)
                                            {
                                                if (existingProjectTimeBlocks.Any(i => ((i.StartTime >= timeFrom && i.StartTime < timeTo) || (i.StopTime > timeFrom && i.StopTime <= timeTo) || (i.StartTime <= timeFrom && i.StopTime >= timeTo) || (i.StartTime >= timeFrom && i.StopTime <= timeTo))))
                                                {
                                                    result.BooleanValue2 = true;
                                                    if (fromApp)
                                                        errorList.Add(string.Format(GetText(7529, "En eller flera rader {0} för {1} har tider som överlappar tidigare registrerade tider."), timeBlockDate.Date.ToShortDateString(), "(" + employee.EmployeeNr + ") " + employee.FirstName + " " + employee.LastName));
                                                    else
                                                        errorList.Add(string.Format(GetText(91934, "En eller flera rader {0} för {1} har tider som överlappar tidigare registrerade tider.\nKontrollera tiderna i informationsdialogen ({2}) och försök spara igen."), timeBlockDate.Date.ToShortDateString(), "(" + employee.EmployeeNr + ") " + employee.FirstName + " " + employee.LastName, "<i class=\"fa fa-info\"></i>"));
                                                    break;
                                                }
                                            }
                                        }
                                        else if (existingFixedProjectTimeBlocks.Any())
                                        {
                                            if (existingFixedProjectTimeBlocks.Any(i => ((i.StartTime >= timeFrom && i.StartTime < timeTo) || (i.StopTime > timeFrom && i.StopTime <= timeTo) || (i.StartTime <= timeFrom && i.StopTime >= timeTo) || (i.StartTime >= timeFrom && i.StopTime <= timeTo))))
                                            {
                                                result.BooleanValue2 = true;
                                                if (fromApp)
                                                    errorList.Add(string.Format(GetText(7529, "En eller flera rader {0} för {1} har tider som överlappar tidigare registrerade tider."), timeBlockDate.Date.ToShortDateString(), "(" + employee.EmployeeNr + ") " + employee.FirstName + " " + employee.LastName));
                                                else
                                                    errorList.Add(string.Format(GetText(91934, "En eller flera rader {0} för {1} har tider som överlappar tidigare registrerade tider.\nKontrollera tiderna i informationsdialogen ({2}) och försök spara igen."), timeBlockDate.Date.ToShortDateString(), "(" + employee.EmployeeNr + ") " + employee.FirstName + " " + employee.LastName, "<i class=\"fa fa-info\"></i>"));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var workDateGroup in item.Rows.GroupBy(x => x.WorkDate))
                            {
                                var workDate = workDateGroup.Key;
                                var totalScheduledWorkMinutesForDate = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(employee.EmployeeId, workDate, false, null).GetWorkMinutes();
                                var totalReportedMinutes = workDateGroup.Select(p => CalendarUtility.TimeSpanToMinutes(p.StopTime, p.StartTime)).Sum(y => y);
                                var timeDeviationCauseId = workDateGroup.First().TimeDeviationCauseId;

                                if (totalReportedMinutes > totalScheduledWorkMinutesForDate)
                                {
                                    var totalTimeString = CalendarUtility.ToTime(DateTime.MinValue.AddMinutes(totalScheduledWorkMinutesForDate));
                                    var previousTimeString = CalendarUtility.ToTime(DateTime.MinValue.AddMinutes(totalReportedMinutes));

                                    var timeDiviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == timeDeviationCauseId);
                                    if (timeDiviationCause.IsAbsence && !timeDiviationCause.IsPresence)
                                    {
                                        errorList.Add(string.Format(GetText(7569, "Totalt rapporterad frånvaro ({0}) för {1} är större än planerad tid ({2}) för dagen"), previousTimeString, workDate.ToShortDateString(), totalTimeString));
                                    }
                                    else
                                    {
                                        warningList.Add(string.Format(GetText(7459, "Totalt rapporterad tid ({0}) för {1} är större än planerad tid ({2}) för dagen"), previousTimeString, workDate.ToShortDateString(), totalTimeString));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            result.Success = errorList.Count == 0;
            result.Strings = errorList;

            if (errorList.Any())
            {
                result.ErrorMessage = string.Join("\n", errorList.ToArray());
            }

            if (warningList.Any())
            {
                result.InfoMessage = string.Join("\n", warningList.ToArray());
            }
            return result;
        }

        public ActionResult SaveProjectTimeBlock(ProjectTimeBlockSaveDTO item)
        {
            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
            if (useProjectTimeBlock)
                return SaveProjectTimeBlocks(new List<ProjectTimeBlockSaveDTO> { item }, true);
            else
                return SaveProjectTimeBlockSaveDTO(item);

            /*
            ActionResult result = new ActionResult();
            ProjectTimeBlock projectTimeBlock = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (item.ProjectTimeBlockId != 0)
                        {
                            #region Existing

                            int previousInvoiceQuantity = 0;
                            int previousWorkTimeQuantity = 0;
                            ProjectTimeBlock existingProjectTimeBlock = GetProjectTimeBlock(entities, item.ProjectTimeBlockId, true, true, true);

                            if (existingProjectTimeBlock == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound);

                            if (item.State == SoeEntityState.Deleted)
                            {
                                existingProjectTimeBlock.State = (int)SoeEntityState.Deleted;
                            }
                            else
                            {
                                previousInvoiceQuantity = existingProjectTimeBlock.InvoiceQuantity;
                                previousWorkTimeQuantity = CalendarUtility.TimeSpanToMinutes(existingProjectTimeBlock.StopTime, existingProjectTimeBlock.StartTime);

                                existingProjectTimeBlock.EmployeeId = item.EmployeeId;
                                existingProjectTimeBlock.ProjectId = item.ProjectId;
                                existingProjectTimeBlock.CustomerInvoiceId = item.CustomerInvoiceId;
                                existingProjectTimeBlock.TimeDeviationCauseId = item.TimeDeviationCauseId.Value;
                                existingProjectTimeBlock.StartTime = item.From.Value.RemoveSeconds();
                                existingProjectTimeBlock.StopTime = item.To.Value.RemoveSeconds();
                                existingProjectTimeBlock.InvoiceQuantity = (int)item.InvoiceQuantity; //Should always be integer since it´s minutes
                                existingProjectTimeBlock.ExternalNote = item.ExternalNote;
                                existingProjectTimeBlock.InternalNote = item.InternalNote;

                                if ((item.TimeBlockDateId == null || item.TimeBlockDateId == 0)
                                    && item.Date.HasValue && existingProjectTimeBlock.TimeBlockDate != null
                                    && item.Date.Value.Date != existingProjectTimeBlock.TimeBlockDate.Date.Date)
                                    existingProjectTimeBlock.TimeBlockDateId = TimeBlockManager.GetTimeBlockDate(entities, item.Date.Value, item.EmployeeId, base.ActorCompanyId, true).TimeBlockDateId;
                            }

                            SetModifiedProperties(existingProjectTimeBlock);
                            result = SaveChanges(entities, transaction);

                            if (!result.Success)
                                return result;

                            //Update transactions for projecttimeblock
                            result = CreateTransactionsForProjectTimeBlock(entities, transaction, existingProjectTimeBlock, item, previousInvoiceQuantity, previousWorkTimeQuantity);

                            #endregion
                        }
                        else
                        {
                            #region New

                            #region Check

                            var project = GetProject(entities, item.ProjectId.Value);
                            if (project != null)
                            {
                                if ((project.Status == (int)TermGroup_ProjectStatus.Locked) || (project.Status == (int)TermGroup_ProjectStatus.Finished))
                                {
                                    result.Success = false;
                                    result.ErrorMessage = "Project is locked or closed";
                                    return result;
                                }
                            }
                            else
                            {
                                result.Success = false;
                                result.ErrorMessage = "Project not found";
                                return result;
                            }

                            if (item.CustomerInvoiceId.HasValue && item.CustomerInvoiceId.Value > 0)
                            {
                                var order = InvoiceManager.GetCustomerInvoice(entities, item.CustomerInvoiceId.Value);
                                if (order == null)
                                {
                                    result.Success = false;
                                    result.ErrorMessage = "Order not found";
                                    return result;
                                }
                            }

                            #endregion

                            //Timevcode, project och deviation saknas, kolla sparametod på klientsidan
                            projectTimeBlock = new ProjectTimeBlock()
                            {
                                ActorCompanyId = base.ActorCompanyId,
                                EmployeeId = item.EmployeeId,
                                ProjectId = item.ProjectId,
                                CustomerInvoiceId = item.CustomerInvoiceId,
                                TimeDeviationCauseId = item.TimeDeviationCauseId.Value,
                                StartTime = item.From.Value.RemoveSeconds(),
                                StopTime = item.To.Value.RemoveSeconds(),
                                InvoiceQuantity = (int)item.InvoiceQuantity, //Should always be integer since it´s minutes
                                ExternalNote = item.ExternalNote,
                                InternalNote = item.InternalNote,
                                RecordType = item.isFromTimeSheet ? (int)SoeProjectRecordType.TimeSheet : (int)SoeProjectRecordType.Order,
                            };

                            if ((item.TimeBlockDateId == null || item.TimeBlockDateId == 0) && item.Date.HasValue)
                                projectTimeBlock.TimeBlockDateId = TimeBlockManager.GetTimeBlockDate(entities, item.Date.Value, item.EmployeeId, base.ActorCompanyId, true).TimeBlockDateId;

                            SetCreatedProperties(projectTimeBlock);

                            result = SaveChanges(entities, transaction);

                            if (!result.Success)
                                return result;

                            //Create transactions for projecttimeblock
                            result = CreateTransactionsForProjectTimeBlock(entities, transaction, projectTimeBlock, item, null, null);

                            if (!result.Success)
                                return result;

                            #endregion
                        }

                        result = SaveChanges(entities, transaction);

                        if (!result.Success)
                            return result;

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception exp)
                {
                    base.LogError(exp, log);
                    return result = new ActionResult(exp);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.Value = projectTimeBlock;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            //Create transactions for salary
            if (projectTimeBlock != null)
            {
                TimeEngineManager tem = new TimeEngineManager(parameterObject, base.ActorCompanyId, base.UserId);
                var salaryResult = tem.GenerateTimeBlocksBasedOnProjectTimeBlocks(new List<ProjectTimeBlock>() { projectTimeBlock });
            }

            return result;
            */
        }

        public ActionResult SaveProjectTimeBlocks(List<ProjectTimeBlockSaveDTO> projectTimeBlockSaveDTOs, bool returnCreatedBlock)
        {
            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
            if (!useProjectTimeBlock)
                return SaveProjectTimeBlockSaveDTOs(projectTimeBlockSaveDTOs);

            ActionResult result = new ActionResult();
            var projectTimeBlocks = new List<ProjectTimeBlock>();
            var projectTimeBlocksWithAutoGen = new List<ProjectTimeBlock>();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveProjectTimeBlocks(entities, transaction, projectTimeBlockSaveDTOs, ref projectTimeBlocks, ref projectTimeBlocksWithAutoGen);

                        //Commit transaction
                        if (result.Success)
                        {
                            transaction.Complete();
                        }
                    }
                }
                catch (Exception exp)
                {
                    base.LogError(exp, log);
                    result = new ActionResult(exp);
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

            if (result.Success && (projectTimeBlocks.Any() || projectTimeBlocksWithAutoGen.Any()))
            {
                //Create transactions for salary
                TimeEngineManager tem = new TimeEngineManager(parameterObject, base.ActorCompanyId, base.UserId);

                var firstProjectTimeBlock = projectTimeBlocks.Any() ? projectTimeBlocks.FirstOrDefault() : projectTimeBlocksWithAutoGen.FirstOrDefault();

                if (projectTimeBlocksWithAutoGen.Any())
                    result = tem.GenerateTimeBlocksBasedOnProjectTimeBlocks(projectTimeBlocksWithAutoGen, true);

                if (projectTimeBlocks.Any())
                    result = tem.GenerateTimeBlocksBasedOnProjectTimeBlocks(projectTimeBlocks, false);

                if (returnCreatedBlock)
                    result.Value = firstProjectTimeBlock;
            }

            return result;
        }

        private List<ProjectTimeBlock> RecalculateProjectTimeBlockStartDates(CompEntities entities, int employeeId, int timeBlockDateId, DateTime date)
        {
            var result = new List<ProjectTimeBlock>();
            var firstTemplateBlockDate = GetFirstTemplateBlockDate(entities, employeeId, date, true);
            if (firstTemplateBlockDate.HasValue)
            {
                var projectTimeBlocks = GetProjectTimeBlocks(entities, timeBlockDateId, false, true).OrderBy(k => k.StartTime);
                var firstProjectTimeBlock = projectTimeBlocks.First();
                if (firstProjectTimeBlock != null && firstProjectTimeBlock.StartTime != firstTemplateBlockDate.Value && !firstProjectTimeBlock.TimeDeviationCause.MandatoryTime)
                {
                    var diffMinutes = CalendarUtility.TimeSpanToMinutes(firstTemplateBlockDate.Value, firstProjectTimeBlock.StartTime);
                    foreach (var projectTimeBlock in projectTimeBlocks)
                    {
                        var lastBlock = result.LastOrDefault();
                        var currentBlockSpan = CalendarUtility.TimeSpanToMinutes(projectTimeBlock.StopTime, projectTimeBlock.StartTime);

                        projectTimeBlock.StartTime = lastBlock == null ? projectTimeBlock.StartTime.AddMinutes(diffMinutes) : lastBlock.StopTime;
                        projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(currentBlockSpan);
                        result.Add(projectTimeBlock);
                    }
                }
                else
                {
                    //check for holes....
                    ProjectTimeBlock previousBlock = null;
                    int runningDiffMinutes = 0;

                    foreach (var projectTimeBlock in projectTimeBlocks.Where(p => !p.TimeDeviationCause.MandatoryTime))
                    {
                        bool setPreviousBlock = true;
                        if (previousBlock != null && (projectTimeBlock.StartTime != previousBlock.StopTime))
                        {
                            runningDiffMinutes += CalendarUtility.TimeSpanToMinutes(previousBlock.StopTime, projectTimeBlock.StartTime);
                            setPreviousBlock = false;
                            result.Add(projectTimeBlock);
                        }

                        if (runningDiffMinutes != 0)
                        {
                            projectTimeBlock.StartTime = projectTimeBlock.StartTime.AddMinutes(runningDiffMinutes);
                            projectTimeBlock.StopTime = projectTimeBlock.StopTime.AddMinutes(runningDiffMinutes);
                        }

                        previousBlock = setPreviousBlock ? projectTimeBlock : null;
                    }
                }
            }

            return result;
        }

        public ActionResult MoveTimeRowsToOrderRow(int customerInvoiceId, int customerInvoiceRowId, List<int> projectTimeBlockIds)
        {
            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            if (useProjectTimeBlock)
                return MoveProjectTimeBlocksToOrderRow(customerInvoiceId, customerInvoiceRowId, projectTimeBlockIds);
            else
                return MoveProjectInvoiceDaysToOrderRow(customerInvoiceId, customerInvoiceRowId, projectTimeBlockIds);
        }

        private ActionResult MoveProjectTimeBlocksToOrderRow(int customerInvoiceId, int customerInvoiceRowId, List<int> projectTimeBlockIds)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.CommandTimeout = 120;
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var moveToOrder = InvoiceManager.GetCustomerInvoice(entities, customerInvoiceId);
                        if (moveToOrder == null)
                            return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8321, "Order kunde inte hittas"));

                        var projectTimeBlocks = new List<ProjectTimeBlock>();

                        //Much better for performance to load them one by one than a contains...
                        foreach (var id in projectTimeBlockIds)
                        {
                            var projectTimeBlock = GetProjectTimeBlock(entities, id, true, true, true);
                            if (projectTimeBlock != null)
                            {
                                projectTimeBlocks.Add(projectTimeBlock);
                            }
                        }

                        //var projectTimeBlocks = GetProjectTimeBlocks(entities, projectTimeBlockIds, base.ActorCompanyId, true, true);
                        var date = projectTimeBlocks.Max(p => p.TimeBlockDate.Date);

                        foreach (var projectTimeBlock in projectTimeBlocks)
                        {
                            if (projectTimeBlock.CustomerInvoiceId.HasValue && !projectTimeBlock.TimeCodeTransaction.IsNullOrEmpty())
                            {
                                result = CreateTimeBillingInvoiceRow(entities, transaction, projectTimeBlock.TimeCodeTransaction.ToList(), projectTimeBlock.InvoiceQuantity, projectTimeBlock.EmployeeId, date, projectTimeBlock.TimeBlockDate.Date, moveToOrder, customerInvoiceRowId, base.ActorCompanyId);
                                if (!result.Success)
                                {
                                    return result;
                                }
                                else if (result.Success && customerInvoiceRowId == 0)
                                {
                                    customerInvoiceRowId = result.IntegerValue;
                                }
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, log);
                    return new ActionResult(ex);
                }
            }

            return result;
        }

        private ActionResult MoveProjectInvoiceDaysToOrderRow(int customerInvoiceId, int customerInvoiceRowId, List<int> projectInvoiceDayIds)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    entities.CommandTimeout = 180;

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION(new TimeSpan(0, 10, 0))))
                    {
                        var moveToOrder = InvoiceManager.GetCustomerInvoice(entities, customerInvoiceId);
                        if (moveToOrder == null)
                            return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8321, "Order kunde inte hittas"));

                        var days = new List<ProjectInvoiceDay>();

                        foreach (var ids in projectInvoiceDayIds.SplitList(10))
                        {
                            var loopDays = GetProjectInvoiceDays(entities, ids, true, true);
                            if (!loopDays.IsNullOrEmpty())
                            {
                                days.AddRange(loopDays);
                            }
                        }

                        foreach (var daygroup in days.GroupBy(d => d.TimeCodeTransaction.FirstOrDefault(x => x.State == (int)SoeEntityState.Active && x.Type == (int)TimeCodeTransactionType.TimeProject)?.TimeInvoiceTransaction.FirstOrDefault(x => x.State == (int)SoeEntityState.Active)?.CustomerInvoiceRowId))
                        {
                            var sourceCustomerInvoiceRow = InvoiceManager.GetCustomerInvoiceRow(entities, daygroup.Key.GetValueOrDefault(), true);
                            if (sourceCustomerInvoiceRow == null)
                            {
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8531, "Orderrad kunde inte hittas"));
                            }

                            foreach (var day in daygroup)
                            {
                                if (day != null && day.ProjectInvoiceWeek.RecordId > 0 && !day.TimeCodeTransaction.IsNullOrEmpty())
                                {
                                    result = CreateTimeBillingInvoiceRow(entities, transaction, day.TimeCodeTransaction.ToList(), day.InvoiceTimeInMinutes, day.ProjectInvoiceWeek.EmployeeId, day.Date, day.Date, moveToOrder, customerInvoiceRowId, base.ActorCompanyId, sourceCustomerInvoiceRow);
                                    if (!result.Success)
                                    {
                                        return result;
                                    }
                                    else if (result.Success && customerInvoiceRowId == 0)
                                    {
                                        customerInvoiceRowId = result.IntegerValue;
                                    }
                                }
                            }
                        }

                        /*
                        foreach (var id in projectInvoiceDayIds)
                        {
                            var day = GetProjectInvoiceDay(entities, id, true, true);
                            if (day != null && day.ProjectInvoiceWeek.RecordId > 0 && !day.TimeCodeTransaction.IsNullOrEmpty())
                            {
                                result = CreateTimeBillingInvoiceRow(entities, transaction, day.TimeCodeTransaction.ToList(), day.InvoiceTimeInMinutes, day.ProjectInvoiceWeek.EmployeeId, day.Date, day.Date, moveToOrder, customerInvoiceRowId, base.ActorCompanyId);
                                if (!result.Success)
                                {
                                    return result;
                                }
                                else if (result.Success && customerInvoiceRowId == 0)
                                {
                                    customerInvoiceRowId = result.IntegerValue;
                                }
                            }
                        }

                        */

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, log);
                    return new ActionResult(ex);
                }
            }

            return result;
        }

        public ActionResult MoveTimeRowsToNewCustomerInvoiceRow(List<GenericType<int, int>> items, DateTime from, DateTime to)
        {
            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            if (useProjectTimeBlock)
                return MoveProjectTimeBlocksToNewCustomerInvoiceRow(items, from, to);
            else
                return MoveProjectInvoiceDaysToNewCustomerInvoiceRow(items, from, to);
        }

        private ActionResult MoveProjectTimeBlocksToNewCustomerInvoiceRow(List<GenericType<int, int>> items, DateTime fromDate, DateTime toDate)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    var groupedItems = items.GroupBy(p => p.Field1).ToList();
                    foreach (var group in groupedItems)
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            var order = InvoiceManager.GetCustomerInvoice(entities, group.Key);
                            if (order == null)
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8321, "Order kunde inte hittas"));

                            var collectedTimeBlocks = new List<ProjectTimeBlock>();
                            foreach (var item in group)
                            {

                                var customerInvoiceRowId = 0;

                                var projectTimeBlocks = collectedTimeBlocks.Where(p => p.CustomerInvoiceId == item.Field1).ToList();

                                if (!projectTimeBlocks.Any())
                                {
                                    projectTimeBlocks = (from p in entities.ProjectTimeBlock
                                                             .Include("TimeBlockDate")
                                                             .Include("TimeCodeTransaction.TimeInvoiceTransaction")
                                                         where p.CustomerInvoiceId == item.Field1 &&
                                                            p.TimeBlockDate.Date >= fromDate &&
                                                            p.TimeBlockDate.Date <= toDate &&
                                                            p.State == (int)SoeEntityState.Active
                                                         select p).ToList();

                                    if (!projectTimeBlocks.Any())
                                        continue;

                                    // Add to temp collection
                                    collectedTimeBlocks.AddRange(projectTimeBlocks);
                                }

                                var date = projectTimeBlocks.Max(p => p.TimeBlockDate.Date);

                                foreach (var ptb in projectTimeBlocks)
                                {
                                    var valid = false;
                                    foreach (var tc in ptb.TimeCodeTransaction.Where(t => t.State == (int)SoeEntityState.Active))
                                    {
                                        if (tc.TimeInvoiceTransaction.Any(i => i.CustomerInvoiceRowId == item.Field2 && i.State == (int)SoeEntityState.Active))
                                        {
                                            valid = true;
                                            break;
                                        }
                                    }

                                    if (valid)
                                    {
                                        result = CreateTimeBillingInvoiceRow(entities, transaction, ptb.TimeCodeTransaction.ToList(), ptb.InvoiceQuantity, ptb.EmployeeId, date, ptb.TimeBlockDate.Date, order, customerInvoiceRowId, base.ActorCompanyId);
                                        if (!result.Success)
                                            return result;
                                        else
                                            customerInvoiceRowId = result.IntegerValue;
                                    }
                                }
                            }

                            result = SaveChanges(entities, transaction);
                            if (result.Success)
                                transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, log);
                    return new ActionResult(ex);
                }
            }

            return result;
        }

        private ActionResult MoveProjectInvoiceDaysToNewCustomerInvoiceRow(List<GenericType<int, int>> items, DateTime from, DateTime to)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var collectedProjectInvoiceDays = new List<ProjectInvoiceDay>();
                        foreach (var item in items)
                        {
                            var order = InvoiceManager.GetCustomerInvoice(entities, item.Field1);
                            if (order == null)
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8321, "Order kunde inte hittas"));

                            var projectInvoiceDays = collectedProjectInvoiceDays.Where(p => p.ProjectInvoiceWeek.RecordId == item.Field1);
                            if (!projectInvoiceDays.Any())
                            {
                                projectInvoiceDays = (from p in entities.ProjectInvoiceDay
                                                    .Include("ProjectInvoiceWeek")
                                                    .Include("TimeCodeTransaction.TimeInvoiceTransaction")
                                                      where p.ProjectInvoiceWeek.RecordId == item.Field1 &&
                                                      p.ProjectInvoiceWeek.State == (int)SoeEntityState.Active
                                                      orderby p.Date descending
                                                      select p);

                                if (!projectInvoiceDays.Any())
                                    continue;

                                // Add to temp collection
                                collectedProjectInvoiceDays.AddRange(projectInvoiceDays);
                            }

                            if (!projectInvoiceDays.Any(p => p.Date < from || p.Date > to))
                                continue;

                            //var date = filteredProjectTimeBlocks.Max(p => p.TimeBlockDate.Date);
                            foreach (var pid in projectInvoiceDays.Where(p => p.Date >= from && p.Date <= to))
                            {
                                if (!pid.TimeCodeTransaction.IsLoaded)
                                    pid.TimeCodeTransaction.Load();

                                var valid = false;
                                foreach (var tc in pid.TimeCodeTransaction.Where(t => t.State == (int)SoeEntityState.Active))
                                {
                                    if (!tc.TimeInvoiceTransaction.IsLoaded)
                                        tc.TimeInvoiceTransaction.Load();

                                    if (tc.TimeInvoiceTransaction.Any(i => i.CustomerInvoiceRowId == item.Field2 && i.State == (int)SoeEntityState.Active))
                                    {
                                        valid = true;
                                        break;
                                    }
                                }

                                if (valid)
                                {
                                    result = CreateTimeBillingInvoiceRow(entities, transaction, pid.TimeCodeTransaction.ToList(), pid.InvoiceTimeInMinutes, pid.ProjectInvoiceWeek.EmployeeId, pid.Date, pid.Date, order, 0, base.ActorCompanyId);
                                    if (!result.Success)
                                        return result;
                                }
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, log);
                    return new ActionResult(ex);
                }
            }

            return result;
        }

        public ActionResult MoveTimeRowsToDate(DateTime? selectedDate, List<int> projectTimeBlockIds, bool fromApp)
        {
            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            if (useProjectTimeBlock)
            {
                return MoveProjectTimeBlocksToDate(selectedDate.Value, projectTimeBlockIds, fromApp);
            }

            return new ActionResult(false);
        }

        private ActionResult MoveProjectTimeBlocksToDate(DateTime selectedDate, List<int> projectTimeBlockIds, bool fromApp)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();


                    //Create transactions for salary
                    TimeEngineManager tem = new TimeEngineManager(parameterObject, base.ActorCompanyId, base.UserId);
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId));

                    foreach (var projectTimeBlockId in projectTimeBlockIds)
                    {
                        var projectTimeBlock = GetProjectTimeBlock(entities, projectTimeBlockId, true, true, true);
                        if (projectTimeBlock == null)
                        {
                            return new ActionResult("Faild finding time block!");
                        }
                        var newTimeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, base.ActorCompanyId, projectTimeBlock.EmployeeId, selectedDate, true);

                        //new values
                        var newStart = projectTimeBlock.StartTime;
                        var newStop = projectTimeBlock.StopTime;
                        if (!projectTimeBlock.TimeDeviationCause.MandatoryTime)
                        {
                            var nextTime = GetEmployeeFirstEligableTime(entities, projectTimeBlock.EmployeeId, newTimeBlockDate.Date, base.ActorCompanyId, base.UserId);
                            var durationInMinutes = (projectTimeBlock.StopTime - projectTimeBlock.StartTime).TotalMinutes;

                            newStart = durationInMinutes > 0 ? new DateTime(1900, 1, 1, nextTime.Hour, nextTime.Minute, 0) : new DateTime(1900, 1, 1, 0, 0, 0);
                            newStop = newStart.AddMinutes(durationInMinutes);
                        }

                        #region Validate new block

                        var employee = EmployeeManager.GetEmployee(entities, projectTimeBlock.EmployeeId, base.ActorCompanyId);
                        var autoGenTimeAndBreakForProject = employee.GetAutoGenTimeAndBreakForProject(selectedDate, employeeGroups);
                        if (autoGenTimeAndBreakForProject == null)
                        {
                            return new ActionResult(GetText(8539, "Tidavtal hittades inte"));
                        }

                        result = ValidateProjectTimeBlockData(null, projectTimeBlock.EmployeeId, selectedDate, newStart, newStop, projectTimeBlock.TimeDeviationCauseId, projectTimeBlock.EmployeeChildId, autoGenTimeAndBreakForProject.GetValueOrDefault(), fromApp);
                        if (!result.Success)
                        {
                            return result;
                        }

                        #endregion

                        projectTimeBlock.State = (int)SoeEntityState.Deleted;
                        result = SaveChanges(entities);
                        if (!result.Success)
                            return result;
                        result = tem.GenerateTimeBlocksBasedOnProjectTimeBlocks(new List<ProjectTimeBlock>() { projectTimeBlock }, autoGenTimeAndBreakForProject.GetValueOrDefault());
                        if (!result.Success)
                            return result;

                        projectTimeBlock.State = (int)SoeEntityState.Active;
                        projectTimeBlock.TimeBlockDate = newTimeBlockDate;
                        projectTimeBlock.StartTime = newStart;
                        projectTimeBlock.StopTime = newStop;

                        result = SaveChanges(entities);
                        if (!result.Success)
                            return result;

                        result = tem.GenerateTimeBlocksBasedOnProjectTimeBlocks(new List<ProjectTimeBlock>() { projectTimeBlock }, true);
                        if (!result.Success)
                            return result;

                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            foreach (var erpTimeCodeTransaction in projectTimeBlock.TimeCodeTransaction.Where(t => t.State == (int)SoeEntityState.Active && (t.TimeBlockId == null || t.TimeBlockId == 0)))
                            {
                                erpTimeCodeTransaction.TimeBlockDate = newTimeBlockDate;

                                foreach (var erpInvoiceTransaction in erpTimeCodeTransaction.TimeInvoiceTransaction.Where(tit => tit.State == (int)SoeEntityState.Active))
                                {
                                    var oldTimeBlockDate = erpInvoiceTransaction.TimeBlockDate;
                                    erpInvoiceTransaction.TimeBlockDate = newTimeBlockDate;

                                    if (!erpInvoiceTransaction.CustomerInvoiceRowReference.IsLoaded)
                                    {
                                        erpInvoiceTransaction.CustomerInvoiceRowReference.Load();
                                    }

                                    var customerInvoiceRow = erpInvoiceTransaction.CustomerInvoiceRow;
                                    if (customerInvoiceRow != null)
                                    {
                                        CalculatePurchasePrice(entities, customerInvoiceRow, employee, customerInvoiceRow.ProductId.GetValueOrDefault(), -projectTimeBlock.InvoiceQuantity, oldTimeBlockDate.Date, projectTimeBlock.ProjectId);
                                        CalculatePurchasePrice(entities, customerInvoiceRow, employee, customerInvoiceRow.ProductId.GetValueOrDefault(), projectTimeBlock.InvoiceQuantity, newTimeBlockDate.Date, projectTimeBlock.ProjectId);
                                    }
                                }
                            }

                            result = SaveChanges(entities, transaction);
                            if (result.Success)
                                transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, log);
                    return new ActionResult(ex);
                }
            }
            return result;
        }

        private decimal CalculatePurchasePrice(CompEntities entities, int productId, Employee employee, DateTime date, int? projectId)
        {
            var product = ProductManager.GetInvoiceProductSmall(entities, productId, base.ActorCompanyId);
            var unitPurchasePrice = product.PurchasePrice;
            var employeeCalculatedCost = employee != null && product.UseCalculatedCost.GetValueOrDefault() ? EmployeeManager.GetEmployeeCalculatedCost(entities, employee, date, projectId) : 0;
            if (employeeCalculatedCost > 0)
            {
                unitPurchasePrice = employeeCalculatedCost;
            }

            return unitPurchasePrice;

        }

        private void CalculatePurchasePrice(CompEntities entities, CustomerInvoiceRow row, Employee employee, int productId, decimal quantityDiff, DateTime date, int? projectId)
        {
            var unitPurchasePrice = CalculatePurchasePrice(entities, productId, employee, date, projectId);

            if (!row.ProductUnitReference.IsLoaded)
                row.ProductUnitReference.Load();

            quantityDiff = ConvertMinutesToQuantity(row.ProductUnit, quantityDiff);

            decimal newTotalPurchasePrice = (quantityDiff * unitPurchasePrice);
            decimal oldTotalPurchasePrice = row.Quantity.GetValueOrDefault() * row.PurchasePrice;
            decimal totalQty = row.Quantity.HasValue ? row.Quantity.Value + quantityDiff : quantityDiff;
            row.Quantity = totalQty;

            if (row.Quantity.GetValueOrDefault() != 0)
                unitPurchasePrice = Math.Round((oldTotalPurchasePrice + newTotalPurchasePrice) / totalQty, 2);
            else
                unitPurchasePrice = 0;

            row.PurchasePriceCurrency = row.PurchasePrice = unitPurchasePrice;
        }

        public ActionResult MoveTimeRowsToOrder(int customerInvoiceId, List<int> projectTimeBlockIds)
        {
            var moveToOrder = InvoiceManager.GetCustomerInvoice(customerInvoiceId);
            if (moveToOrder == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8321, "Order kunde inte hittas"));

            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            if (useProjectTimeBlock)
                return MoveProjectTimeBlocksToOrder(moveToOrder, projectTimeBlockIds);
            else
                return MoveProjectInvoiceDaysToOrder(moveToOrder, projectTimeBlockIds);
        }

        private ActionResult MoveProjectInvoiceDaysToOrder(CustomerInvoice moveToOrder, List<int> projectInvoiceDayIds)
        {
            var result = new ActionResult();
            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (var id in projectInvoiceDayIds)
                        {
                            var day = GetProjectInvoiceDay(entities, id, true, false, true);
                            if (day != null)
                            {
                                var employeeId = day.ProjectInvoiceWeek.EmployeeId;
                                var timeCodeId = day.ProjectInvoiceWeek.TimeCodeId.GetValueOrDefault();
                                var internalComment = day.TimeCodeTransaction.FirstOrDefault()?.Comment;
                                result = SaveProjectInvoiceDay(entities, transaction, ActorCompanyId, parameterObject.RoleId, day.ProjectInvoiceWeek.RecordId, day.Date, 0, 0, "", timeCodeId, commentExternal: "", employeeId: employeeId);
                                if (!result.Success)
                                {
                                    return result;
                                }

                                result = SaveProjectInvoiceDay(entities, transaction, ActorCompanyId, parameterObject.RoleId, moveToOrder.InvoiceId, day.Date, day.InvoiceTimeInMinutes, day.WorkTimeInMinutes, day.Note, timeCodeId, commentExternal: internalComment, employeeId: employeeId, ignoreExternalComment: false);
                                if (!result.Success)
                                {
                                    return result;
                                }
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, log);
                    return new ActionResult(ex);
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            return result;
        }

        private ActionResult MoveProjectTimeBlocksToOrder(CustomerInvoice moveToOrder, List<int> projectTimeBlockIds)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (var projectTimeBlockId in projectTimeBlockIds)
                        {
                            var projectTimeBlock = GetProjectTimeBlock(entities, projectTimeBlockId, true, true, true);
                            if (projectTimeBlock.CustomerInvoiceId.HasValue)
                            {
                                projectTimeBlock.State = (int)SoeEntityState.Deleted;
                                DeleteTimeCodeTransactions(projectTimeBlock, out int timeCodeId, out int previousCustomerInvoiceRowId);

                                //remove from old
                                result = CreateTransactionsForProjectTimeBlock(entities, transaction, projectTimeBlock, timeCodeId, projectTimeBlock.TimeBlockDate.Date, projectTimeBlock.InvoiceQuantity, null, true, timeCodeId, previousCustomerInvoiceRowId);
                                if (!result.Success)
                                {
                                    return result;
                                }

                                projectTimeBlock.State = (int)SoeEntityState.Active;
                                projectTimeBlock.CustomerInvoiceId = moveToOrder.InvoiceId;
                                projectTimeBlock.ProjectId = moveToOrder.ProjectId;

                                //add to new order
                                result = CreateTransactionsForProjectTimeBlock(entities, transaction, projectTimeBlock, timeCodeId, projectTimeBlock.TimeBlockDate.Date, null, null, true, 0);
                                if (!result.Success)
                                {
                                    return result;
                                }
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, log);
                    return new ActionResult(ex);
                }
            }

            return result;
        }

        public ActionResult RecalculateWorkTime(List<ProjectTimeBlockSaveDTO> projectTimeBlockSaveDTOs)
        {
            var result = new ActionResult();

            if (projectTimeBlockSaveDTOs != null && projectTimeBlockSaveDTOs.Count > 0)
            {
                TimeEngineManager tem = new TimeEngineManager(parameterObject, base.ActorCompanyId, base.UserId);
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId));

                foreach (var projectTimeBlockDTOByEmployee in projectTimeBlockSaveDTOs.GroupBy(g => g.EmployeeId))
                {
                    var autoGenTimeAndBreakForProject = false;
                    var employeeId = projectTimeBlockDTOByEmployee.First().EmployeeId;

                    using (var entities = new CompEntities())
                    {
                        var employee = EmployeeManager.GetEmployee(entities, employeeId, base.ActorCompanyId);

                        var projectTimeBlockIds = projectTimeBlockDTOByEmployee.Select(p => p.ProjectTimeBlockId).ToList();

                        var projectTimeBlocks = GetProjectTimeBlocks(entities, projectTimeBlockIds, base.ActorCompanyId, loadTimeBlockDate: true);

                        foreach (var projectTimeBlockByDate in projectTimeBlocks.GroupBy(g2 => g2.TimeBlockDateId))
                        {
                            var projectTimeBlockDate = projectTimeBlockByDate.First().TimeBlockDate.Date;
                            var timeBlockDateId = projectTimeBlockByDate.First().TimeBlockDateId;

                            autoGenTimeAndBreakForProject = employee.GetAutoGenTimeAndBreakForProject(projectTimeBlockDate, employeeGroups).GetValueOrDefault();

                            List<ProjectTimeBlock> changedProjectTimeBlocks = projectTimeBlockByDate.ToList();
                            if (autoGenTimeAndBreakForProject)
                            {
                                var reaArrangedProjectTimeBlocks = RecalculateProjectTimeBlockStartDates(entities, employeeId, timeBlockDateId, projectTimeBlockDate);
                                if (reaArrangedProjectTimeBlocks.Any())
                                {
                                    changedProjectTimeBlocks = reaArrangedProjectTimeBlocks;
                                    entities.SaveChanges();
                                }
                            }

                            if (projectTimeBlockByDate.Any())
                            {
                                //Create transactions for salary
                                result = tem.GenerateTimeBlocksBasedOnProjectTimeBlocks(changedProjectTimeBlocks, autoGenTimeAndBreakForProject);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public ActionResult SaveNotesForProjectTimeBlock(ProjectTimeBlockSaveDTO projectTimeBlockSaveDTO)
        {
            ActionResult result = new ActionResult();

            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (useProjectTimeBlock)
                            result = SaveNotesForProjectTimeBlockUsingProjectTimeBlock(entities, transaction, projectTimeBlockSaveDTO);
                        else
                            result = SaveNotesForProjectTimeBlockUsingProjectInvoiceDay(entities, transaction, projectTimeBlockSaveDTO);

                        //Commit transaction
                        if (result.Success)
                        {
                            transaction.Complete();
                        }
                    }

                }
                catch (Exception exp)
                {
                    base.LogError(exp, log);
                    return result = new ActionResult(exp);
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

        public ActionResult SaveNotesForProjectTimeBlockUsingProjectTimeBlock(CompEntities entities, TransactionScope transaction, ProjectTimeBlockSaveDTO projectTimeBlockSaveDTO)
        {
            var projectTimeBlock = GetProjectTimeBlock(entities, projectTimeBlockSaveDTO.ProjectTimeBlockId);

            if (projectTimeBlock == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound);

            projectTimeBlock.ExternalNote = projectTimeBlockSaveDTO.ExternalNote;
            projectTimeBlock.InternalNote = projectTimeBlockSaveDTO.InternalNote;

            SetModifiedProperties(projectTimeBlock);

            return SaveChanges(entities, transaction);
        }

        public ActionResult SaveNotesForProjectTimeBlockUsingProjectInvoiceDay(CompEntities entities, TransactionScope transaction, ProjectTimeBlockSaveDTO projectTimeBlockSaveDTO)
        {
            ProjectInvoiceDay projectInvoiceDay = null;
            if (projectTimeBlockSaveDTO.ProjectInvoiceWeekId.HasValue && projectTimeBlockSaveDTO.Date.HasValue)
                projectInvoiceDay = GetProjectInvoiceDay(entities, projectTimeBlockSaveDTO.ProjectInvoiceWeekId.Value, projectTimeBlockSaveDTO.Date.Value, false, true);
            if (projectInvoiceDay == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound);

            if (projectInvoiceDay.Note != projectTimeBlockSaveDTO.ExternalNote)
            {
                projectInvoiceDay.Note = projectTimeBlockSaveDTO.ExternalNote;
                SetModifiedProperties(projectInvoiceDay);
            }

            var timeCodeTransaction = projectInvoiceDay.TimeCodeTransaction.FirstOrDefault(tct => tct.State == (int)SoeEntityState.Active);
            if (timeCodeTransaction != null && (timeCodeTransaction.ExternalComment != projectTimeBlockSaveDTO.ExternalNote || timeCodeTransaction.Comment != projectTimeBlockSaveDTO.InternalNote))
            {
                timeCodeTransaction.ExternalComment = projectTimeBlockSaveDTO.ExternalNote;
                timeCodeTransaction.Comment = projectTimeBlockSaveDTO.InternalNote;
                SetModifiedProperties(projectInvoiceDay);
            }

            return SaveChanges(entities, transaction);
        }

        private void AdjustProjectTimeBlockForAdditionalTime(CompEntities entities, ProjectTimeBlock currentBlock, ProjectTimeBlockSaveDTO saveBlock, TimeDeviationCause saveItemTimeDiviationCause)
        {
            if (saveItemTimeDiviationCause.CalculateAsOtherTimeInSales)
                return;

            var previousWorkTimeQuantity = CalendarUtility.TimeSpanToMinutes(currentBlock.StopTime, currentBlock.StartTime);
            var newWorkTimeQuantity = saveBlock.TimePayrollQuantity;
            var diffMinutes = saveBlock.State == SoeEntityState.Deleted ? -previousWorkTimeQuantity : Convert.ToDouble(newWorkTimeQuantity - previousWorkTimeQuantity);

            var timeBlocksForDate = GetProjectTimeBlocks(entities, currentBlock.TimeBlockDateId, false, true).Where(y => y.ProjectTimeBlockId != saveBlock.ProjectTimeBlockId).OrderBy(x => x.StartTime);
            var timeBlockdiffMinutes = 0;

            foreach (var timeBlock in timeBlocksForDate)
            {
                if (timeBlock.TimeDeviationCause.CalculateAsOtherTimeInSales && (saveBlock.From > timeBlock.StartTime && timeBlock.StopTime >= saveBlock.From))
                {
                    if (saveItemTimeDiviationCause.MandatoryTime)
                    {
                        //since we have a fixed block....also move forward the differense in start time to make sure the additional time is put last in the day and dont overlapp
                        //(as long as mandatory not start and stop completly fitt wihout moving anything).
                        var startDiff = CalendarUtility.TimeSpanToMinutes(saveBlock.From.Value, timeBlock.StartTime);
                        timeBlock.StartTime = timeBlock.StartTime.AddMinutes(startDiff);
                        timeBlock.StopTime = timeBlock.StopTime.AddMinutes(startDiff);
                    }
                    //Make room for the new time by moving the additional time block
                    timeBlock.StartTime = timeBlock.StartTime.AddMinutes(diffMinutes);
                    timeBlock.StopTime = timeBlock.StopTime.AddMinutes(diffMinutes);

                    timeBlockdiffMinutes += CalendarUtility.TimeSpanToMinutes(timeBlock.StopTime, timeBlock.StartTime);
                }
            }

            //Move back the current block with the addional time blocks time if its not fixed
            if (timeBlockdiffMinutes > 0 && !saveItemTimeDiviationCause.MandatoryTime)
            {
                saveBlock.From = saveBlock.From?.AddMinutes(-timeBlockdiffMinutes);
                saveBlock.To = saveBlock.To?.AddMinutes(-timeBlockdiffMinutes);
            }
        }

        private void AdjustProjectTimeBlockForDayWithFixedBlock(CompEntities entities, ProjectTimeBlock currentBlock, ProjectTimeBlockSaveDTO saveBlock)
        {
            var previousWorkTimeQuantity = CalendarUtility.TimeSpanToMinutes(currentBlock.StopTime, currentBlock.StartTime);
            var newWorkTimeQuantity = saveBlock.TimePayrollQuantity;
            var diffMinutes = saveBlock.State == SoeEntityState.Deleted ? -previousWorkTimeQuantity : Convert.ToDouble(newWorkTimeQuantity - previousWorkTimeQuantity);

            var scheduleStart = saveBlock.MandatoryTime ? GetFirstTemplateBlockDate(entities, saveBlock.EmployeeId, saveBlock.Date.Value, true) : null;

            var timeBlocksForDate = GetProjectTimeBlocks(entities, currentBlock.TimeBlockDateId, false, true).Where(y => y.StopTime > currentBlock.StartTime && y.ProjectTimeBlockId != saveBlock.ProjectTimeBlockId).OrderBy(x => x.StartTime);
            var firstBlockForDate = timeBlocksForDate.FirstOrDefault();

            //fill if there is a gap in scheduled time between the fixed block and the outadjusted scheduled times....
            if (firstBlockForDate != null && currentBlock.TimeDeviationCause.MandatoryTime)
            {
                diffMinutes = CalendarUtility.TimeSpanToMinutes(saveBlock.To.Value, firstBlockForDate.StartTime);

                if (scheduleStart.HasValue && firstBlockForDate.StartTime.AddMinutes(diffMinutes) < scheduleStart.Value)
                {
                    diffMinutes = CalendarUtility.TimeSpanToMinutes(scheduleStart.Value, firstBlockForDate.StartTime);
                }
            }

            //if we have a fixed block, check if its anywere nere the next block....
            if (saveBlock.MandatoryTime && firstBlockForDate != null && saveBlock.From > firstBlockForDate.StopTime)
            {
                diffMinutes = 0;
            }

            if (diffMinutes != 0)
            {
                foreach (var timeBlock in timeBlocksForDate)
                {
                    if (timeBlock.TimeDeviationCause != null && timeBlock.TimeDeviationCause.MandatoryTime)
                    {
                        diffMinutes += CalendarUtility.TimeSpanToMinutes(timeBlock.StopTime, timeBlock.StartTime);
                    }
                    else
                    {
                        timeBlock.StartTime = timeBlock.StartTime.AddMinutes(diffMinutes);
                        timeBlock.StopTime = timeBlock.StopTime.AddMinutes(diffMinutes);
                    }
                }
            }

            //Now we have room for the new timespan....
            if (!currentBlock.TimeDeviationCause.MandatoryTime)
            {
                saveBlock.From = currentBlock.StartTime;
                saveBlock.To = currentBlock.StartTime.AddMinutes(Convert.ToDouble(newWorkTimeQuantity));
            }
        }

        private ActionResult DeleteProjectTimeBlock(CompEntities entities, ProjectTimeBlock projectTimeBlock, out int existingTimeCodeId, out int existingCustomerInvoiceRowId)
        {
            var attestStateInitialPayrollId = AttestManager.GetInitialAttestStateId(entities, base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
            var hrTimeCodeTransactions = projectTimeBlock.TimeCodeTransaction.Where(t => t.State == (int)SoeEntityState.Active && t.TimeBlockId > 0);
            existingTimeCodeId = 0;
            existingCustomerInvoiceRowId = 0;
            //Check that we are allow to delete
            foreach (var hrTimeCodeTransaction in hrTimeCodeTransactions)
            {
                if (!hrTimeCodeTransaction.TimePayrollTransaction.IsLoaded)
                {
                    hrTimeCodeTransaction.TimePayrollTransaction.Load();
                }
                var attestedTimePayrollTransactions = hrTimeCodeTransaction.TimePayrollTransaction.Where(p => p.AttestStateId != 0 && p.AttestStateId != attestStateInitialPayrollId && p.State == (int)SoeEntityState.Active);
                if (attestedTimePayrollTransactions.Any())
                {
                    return new ActionResult { Success = false, ErrorMessage = "Kan inte ta bort attesterade tidrader" };
                }
            }

            DeleteTimeCodeTransactions(projectTimeBlock, out existingTimeCodeId, out existingCustomerInvoiceRowId);

            projectTimeBlock.State = (int)SoeEntityState.Deleted;
            return new ActionResult();
        }

        private void DeleteTimeCodeTransactions(ProjectTimeBlock projectTimeBlock, out int existingTimeCodeId, out int existingCustomerInvoiceRowId)
        {
            existingTimeCodeId = 0;
            existingCustomerInvoiceRowId = 0;
            var erpTimeCodeTransactions = projectTimeBlock.TimeCodeTransaction.Where(t => t.State == (int)SoeEntityState.Active && (t.TimeBlockId == null || t.TimeBlockId == 0));
            //TimeCodeId == 0 because we want to remove only ERPs TimeCodeTransactions and not those created from HR
            foreach (var timeCodeTransaction in erpTimeCodeTransactions)
            {
                ChangeEntityState(timeCodeTransaction, SoeEntityState.Deleted);

                existingTimeCodeId = timeCodeTransaction.TimeCodeId;
                foreach (var timeTnvoiceTransaction in timeCodeTransaction.TimeInvoiceTransaction)
                {
                    timeTnvoiceTransaction.State = (int)SoeEntityState.Deleted;
                    existingCustomerInvoiceRowId = timeTnvoiceTransaction.CustomerInvoiceRowId.GetValueOrDefault();
                }
            }
        }

        private ActionResult ValidateStateOnConnectedInvoiceRow(CompEntities entities, ProjectTimeBlock projectTimeBlock)
        {
            if (projectTimeBlock != null)
            {
                var invoiceAttestStates = new InvoiceAttestStates(entities, SoeOriginType.Order, SettingManager, AttestManager, base.ActorCompanyId, base.UserId, false);

                var existingInvoiceTransaction = GetTimeInvoiceTransaction(projectTimeBlock.TimeCodeTransaction.ToList());

                if (existingInvoiceTransaction != null)
                {
                    if (!existingInvoiceTransaction.CustomerInvoiceRowReference.IsLoaded)
                    {
                        existingInvoiceTransaction.CustomerInvoiceRowReference.Load();
                    }

                    if (
                        existingInvoiceTransaction.CustomerInvoiceRow != null &&
                        existingInvoiceTransaction.CustomerInvoiceRow.AttestStateId.HasValue &&
                        invoiceAttestStates.IsAttestStateReadonly(existingInvoiceTransaction.CustomerInvoiceRow.AttestStateId.Value)
                        )
                    {

                        return new ActionResult { Success = false, ErrorMessage = GetText(7573, "Fakturerad tid kan inte ändras när kopplad artikelrad är låst") };
                    }
                }
            }

            return new ActionResult();
        }

        public ActionResult SaveProjectTimeBlocks(CompEntities entities, TransactionScope transaction, List<ProjectTimeBlockSaveDTO> projectTimeBlockSaveDTOs, ref List<ProjectTimeBlock> projectTimeBlocks, ref List<ProjectTimeBlock> projectTimeBlocksWithAutoGen)
        {
            var result = new ActionResult();

            bool invoicedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId, entities);
            bool invoiceTimeAsWorkTime = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectInvoiceTimeAsWorkTime, 0, base.ActorCompanyId, 0);
            bool blockTimeBlockWithZeroStartTime = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectBlockTimeBlockWithZeroStartTime, 0, this.ActorCompanyId, 0, false);

            bool updateInvoicedTime = invoicedTimePermission || (!invoicedTimePermission && invoiceTimeAsWorkTime);
            bool updatePayroll = false;
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
            var deviationCauses = base.GetTimeDeviationCausesFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var usesAdditionalTime = deviationCauses.Any(x => x.CalculateAsOtherTimeInSales);

            foreach (var item in projectTimeBlockSaveDTOs)
            {
                ProjectTimeBlock projectTimeBlock = null;
                int? previousInvoiceQuantity = null;
                int? previousWorkTimeQuantity = null;
                int previousTimeCodeId = 0;
                int previousCustomerInvoiceRowId = 0;
                var autoGenTimeAndBreakForProject = item.AutoGenTimeAndBreakForProject;


                var saveItemTimeDiviationCause = deviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == item.TimeDeviationCauseId.GetValueOrDefault());
                item.MandatoryTime = saveItemTimeDiviationCause?.MandatoryTime ?? false;
                item.AdditionalTime = saveItemTimeDiviationCause?.CalculateAsOtherTimeInSales ?? false;

                if (item.ProjectTimeBlockId != 0)
                {
                    #region Existing

                    projectTimeBlock = GetProjectTimeBlock(entities, item.ProjectTimeBlockId, true, true, true);

                    if (projectTimeBlock == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound);

                    previousInvoiceQuantity = projectTimeBlock.CustomerInvoiceId.GetValueOrDefault() == 0 && item.CustomerInvoiceId.GetValueOrDefault() != 0 ? 0 : projectTimeBlock.InvoiceQuantity;
                    previousWorkTimeQuantity = CalendarUtility.TimeSpanToMinutes(projectTimeBlock.StopTime, projectTimeBlock.StartTime);
                    TimeCodeTransaction timeCodeTransactionForInvoice = projectTimeBlock.TimeCodeTransaction.FirstOrDefault(tct => tct.State == (int)SoeEntityState.Active && tct.TimeInvoiceTransaction != null && tct.TimeInvoiceTransaction.Count > 0);
                    previousTimeCodeId = timeCodeTransactionForInvoice != null ? timeCodeTransactionForInvoice.TimeCodeId : 0;

                    if (item.State == SoeEntityState.Deleted)
                    {
                        //not set form delete functions...
                        Employee employee = EmployeeManager.GetEmployee(entities, projectTimeBlock.EmployeeId, base.ActorCompanyId);
                        autoGenTimeAndBreakForProject = employee.GetAutoGenTimeAndBreakForProject(projectTimeBlock.TimeBlockDate.Date, employeeGroups).GetValueOrDefault(autoGenTimeAndBreakForProject);
                    }

                    //Have we changed anything that should result in payroll calucations in timeengine...change only invoice quantity should not...
                    if (
                            (!item.From.HasValue) ||
                            (!item.To.HasValue) ||
                            (!autoGenTimeAndBreakForProject && DateTime.Compare(projectTimeBlock.StartTime, item.From.Value) != 0) ||
                            (!autoGenTimeAndBreakForProject && DateTime.Compare(projectTimeBlock.StopTime, item.To.Value) != 0) ||
                            (autoGenTimeAndBreakForProject && previousWorkTimeQuantity != item.TimePayrollQuantity) ||
                            (projectTimeBlock.TimeDeviationCauseId != item.TimeDeviationCauseId) ||
                            (item.State == SoeEntityState.Deleted && previousWorkTimeQuantity != 0)
                    )
                    {
                        updatePayroll = true;
                    }

                    if (item.State == SoeEntityState.Deleted)
                    {
                        var deleteResult = DeleteProjectTimeBlock(entities, projectTimeBlock, out int existingTimeCodeId, out previousCustomerInvoiceRowId);
                        if (!deleteResult.Success)
                            return deleteResult;

                        if (item.TimeCodeId == 0)
                            item.TimeCodeId = existingTimeCodeId;

                        //To get through later code...
                        item.TimeDeviationCauseId = projectTimeBlock.TimeDeviationCauseId;

                        previousWorkTimeQuantity = null;
                    }
                    else
                    {
                        var existingInvoiceTransaction = GetTimeInvoiceTransaction(projectTimeBlock.TimeCodeTransaction.ToList());
                        previousCustomerInvoiceRowId = existingInvoiceTransaction == null ? 0 : existingInvoiceTransaction.CustomerInvoiceRowId.GetValueOrDefault();
                    }

                    if (item.Date.HasValue && projectTimeBlock.TimeBlockDate != null && item.Date.Value.Date != projectTimeBlock.TimeBlockDate.Date.Date)
                    {
                        return new ActionResult { Success = false, ErrorMessage = "Kan inte uppdatera datum på redan sparade tidrader" };
                    }

                    if ((item.TimeBlockDateId == null || item.TimeBlockDateId == 0)
                                   && item.Date.HasValue && projectTimeBlock.TimeBlockDate != null
                                   && item.Date.Value.Date != projectTimeBlock.TimeBlockDate.Date.Date)
                    {
                        projectTimeBlock.TimeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, base.ActorCompanyId, item.EmployeeId, item.Date.Value, true);
                    }

                    //Make sure the row has correct state if invoice time is chaning...
                    if (item.InvoiceQuantity != projectTimeBlock.InvoiceQuantity)
                    {
                        var invoiceRowStateResult = ValidateStateOnConnectedInvoiceRow(entities, projectTimeBlock);
                        if (!invoiceRowStateResult.Success)
                        {
                            return invoiceRowStateResult;
                        }
                    }

                    if (autoGenTimeAndBreakForProject)
                    {
                        AdjustProjectTimeBlockForDayWithFixedBlock(entities, projectTimeBlock, item);
                    }

                    //Going from working to absence? Then remove invoice times...
                    if (item.TimeCodeId == 0 && (timeCodeTransactionForInvoice != null && timeCodeTransactionForInvoice.TimeCodeId != 0))
                    {
                        ChangeEntityState(timeCodeTransactionForInvoice, SoeEntityState.Deleted);
                        foreach (var timeTnvoiceTransaction in timeCodeTransactionForInvoice.TimeInvoiceTransaction)
                            ChangeEntityState(timeTnvoiceTransaction, SoeEntityState.Deleted);
                    }

                    SetModifiedProperties(projectTimeBlock);

                    #endregion
                }
                else
                {
                    #region New

                    #region Check

                    var project = GetProjectSmall(entities, base.ActorCompanyId, item.ProjectId.Value);
                    if (project != null)
                    {
                        if ((project.Status == TermGroup_ProjectStatus.Locked) || (project.Status == TermGroup_ProjectStatus.Finished))
                        {
                            return new ActionResult { Success = false, ErrorMessage = GetText(7696, "Projektet är låst eller avslutat") + " (" + project.Number + ":" + project.Name + ")" };
                        }
                    }
                    else
                    {
                        return new ActionResult { Success = false, ErrorMessage = GetText(8326, "Projekt saknas") };
                    }

                    if (item.CustomerInvoiceId.HasValue && item.CustomerInvoiceId.Value > 0)
                    {
                        var order = InvoiceManager.GetCustomerInvoiceSmall(entities, item.CustomerInvoiceId.Value);
                        if (order == null)
                        {
                            return new ActionResult { Success = false, ErrorMessage = GetText(5605, "Ordern hittades inte") };
                        }
                    }

                    if (item.Date.HasValue && !TimeScheduleManager.HasEmployeeSchedule(entities, item.EmployeeId, item.Date.Value))
                    {
                        return new ActionResult { Success = false, ErrorMessage = GetText(8501, "Anställd har ej aktiverat schema för dagen") + " " + item.Date.Value.ToShortDateString() };
                    }

                    #endregion

                    updatePayroll = (item.TimePayrollQuantity > 0 || item.From.Value != item.To.Value);

                    //Timevcode, project och deviation saknas, kolla sparametod på klientsidan
                    projectTimeBlock = new ProjectTimeBlock
                    {
                        ActorCompanyId = base.ActorCompanyId,
                        RecordType = item.isFromTimeSheet ? (int)SoeProjectRecordType.TimeSheet : (int)SoeProjectRecordType.Order
                    };

                    if ((item.TimeBlockDateId == null || item.TimeBlockDateId == 0) && item.Date.HasValue)
                    {
                        projectTimeBlock.TimeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, base.ActorCompanyId, item.EmployeeId, item.Date.Value, true);
                    }

                    SetCreatedProperties(projectTimeBlock);
                    entities.ProjectTimeBlock.AddObject(projectTimeBlock);

                    //Calculate to and from because we dont use times in interface...exept for certain deviation codes...
                    if (item.TimePayrollQuantity > 0 && !item.MandatoryTime && (autoGenTimeAndBreakForProject || item.From.Value == item.To.Value))
                    {
                        var nextTime = GetEmployeeFirstEligableTime(entities, item.EmployeeId, (DateTime)item.Date, base.ActorCompanyId, base.UserId);
                        item.From = new DateTime(1900, 1, 1, nextTime.Hour, nextTime.Minute, 0);
                        item.To = item.From.Value.AddMinutes(decimal.ToDouble(item.TimePayrollQuantity));
                    }

                    if (autoGenTimeAndBreakForProject && usesAdditionalTime)
                    {
                        AdjustProjectTimeBlockForAdditionalTime(entities, projectTimeBlock, item, saveItemTimeDiviationCause);
                    }

                    #endregion
                }

                //Get standard deviation code if not set (App old way)...
                if (item.TimeDeviationCauseId.GetValueOrDefault() == 0)
                {
                    item.TimeDeviationCauseId = TimeDeviationCauseManager.GetTimeDeviationCauseIdFromPrio(entities, item.EmployeeId, base.ActorCompanyId, item.Date);
                    if (item.TimeDeviationCauseId.Value == 0)
                    {
                        return new ActionResult { Success = false, ErrorMessage = "TimeDeviationCause is missing" };
                    }
                }

                if (item.State != SoeEntityState.Deleted)
                {
                    if (!item.From.HasValue || !item.To.HasValue || !ValidateStartStopDate(item.From.Value, item.To.Value, !item.MandatoryTime, blockTimeBlockWithZeroStartTime && !item.AdditionalTime))
                    {
                        return new ActionResult { Success = false, ErrorMessage = $"Ogiltigt datum för start eller stopp tid {item.From?.ToShortDateTime()} {item.To?.ToShortDateTime()}" };
                    }

                    projectTimeBlock.EmployeeId = item.EmployeeId;
                    projectTimeBlock.EmployeeChildId = item.EmployeeChildId.GetValueOrDefault() > 0 ? item.EmployeeChildId.GetValueOrDefault() : (int?)null;
                    projectTimeBlock.ProjectId = item.ProjectId;
                    projectTimeBlock.CustomerInvoiceId = item.CustomerInvoiceId.HasValue && item.CustomerInvoiceId.Value > 0 ? item.CustomerInvoiceId : null;
                    projectTimeBlock.TimeDeviationCauseId = item.TimeDeviationCauseId.Value;
                    projectTimeBlock.StartTime = item.From.Value.RemoveSeconds();
                    projectTimeBlock.StopTime = item.To.Value.RemoveSeconds();
                    projectTimeBlock.ExternalNote = string.IsNullOrEmpty(item.ExternalNote) ? null : item.ExternalNote;
                    projectTimeBlock.InternalNote = string.IsNullOrEmpty(item.InternalNote) ? null : item.InternalNote;

                    if (!invoicedTimePermission && invoiceTimeAsWorkTime)
                    {
                        var invoiceRowStateResult = ValidateStateOnConnectedInvoiceRow(entities, projectTimeBlock);
                        if (invoiceRowStateResult.Success)
                        {
                            var timeDiviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(entities, projectTimeBlock.TimeDeviationCauseId, ActorCompanyId, false);
                            projectTimeBlock.InvoiceQuantity = (timeDiviationCause.Type != (int)TermGroup_TimeDeviationCauseType.Absence && !timeDiviationCause.NotChargeable) ? (int)item.TimePayrollQuantity : 0;
                        }
                    }
                    else
                    {
                        projectTimeBlock.InvoiceQuantity = (int)item.InvoiceQuantity; ////Should always be integer since it´s minutes
                    }

                    if (projectTimeBlock.InvoiceQuantity != 0 && item.TimeCodeId == 0)
                    {
                        return new ActionResult { Success = false, ErrorMessage = GetText(7476, "Debiteringstyp saknas") };
                    }

                    //only updateing work time and not invoicetime?
                    if (updateInvoicedTime && projectTimeBlock.ProjectTimeBlockId != 0 && previousInvoiceQuantity == projectTimeBlock.InvoiceQuantity && item.TimeCodeId == previousTimeCodeId)
                    {
                        updateInvoicedTime = false;
                    }
                }

                result = SaveChanges(entities, transaction);

                if (!result.Success)
                    return result;

                //Update transactions for projecttimeblock
                result = CreateTransactionsForProjectTimeBlock(entities, transaction, projectTimeBlock, item.TimeCodeId, item.Date, previousInvoiceQuantity, previousWorkTimeQuantity, updateInvoicedTime, previousTimeCodeId, previousCustomerInvoiceRowId);

                if (!result.Success)
                    return result;

                if (updatePayroll)
                {
                    if (autoGenTimeAndBreakForProject)
                        projectTimeBlocksWithAutoGen.Add(projectTimeBlock);
                    else
                        projectTimeBlocks.Add(projectTimeBlock);
                }

                result = SaveChanges(entities, transaction);

                if (!result.Success)
                    return result;
            }

            return result;
        }

        public List<ProjectTimeBlockSaveDTO> MergeProjectTimeBlockSaveDTOs(List<ProjectTimeBlockSaveDTO> projectTimeBlockSaveDTOs)
        {
            List<ProjectTimeBlockSaveDTO> mergedProjectTimeBlockSaveDTOs = new List<ProjectTimeBlockSaveDTO>();

            var grouped = projectTimeBlockSaveDTOs.GroupBy(p => $"{p.EmployeeId}#{p.ProjectId}#{p.CustomerInvoiceId}#{p.TimeCodeId}#{p.Date}");

            foreach (var group in grouped)
            {
                ProjectTimeBlockSaveDTO dto = group.FirstOrDefault().CloneDTO();

                dto.ExternalNote = string.Empty;
                dto.InternalNote = string.Empty;

                foreach (var row in group)
                {
                    dto.ExternalNote += string.IsNullOrEmpty(dto.ExternalNote) ? row.ExternalNote : $" {row.ExternalNote}";
                    dto.InternalNote += string.IsNullOrEmpty(dto.InternalNote) ? row.InternalNote : $" {row.InternalNote}";
                }

                dto.TimePayrollQuantity = group.Sum(g => g.TimePayrollQuantity);
                dto.InvoiceQuantity = group.Sum(g => g.InvoiceQuantity);

                mergedProjectTimeBlockSaveDTOs.Add(dto);
            }

            return mergedProjectTimeBlockSaveDTOs;
        }

        public ProjectTimeBlockSaveDTO FixProjectTimeBlockSaveDTO(ProjectTimeBlockSaveDTO projectTimeBlockSaveDTO)
        {

            if (projectTimeBlockSaveDTO.EmployeeId == 0 || projectTimeBlockSaveDTO.ActorCompanyId == 0 || !projectTimeBlockSaveDTO.Date.HasValue)
                return null;

            if (projectTimeBlockSaveDTO.TimeBlockDateId == 0 && projectTimeBlockSaveDTO.Date.HasValue)
                projectTimeBlockSaveDTO.TimeBlockDateId = TimeBlockManager.GetTimeBlockDate(projectTimeBlockSaveDTO.ActorCompanyId, projectTimeBlockSaveDTO.EmployeeId, projectTimeBlockSaveDTO.Date.Value, true).TimeBlockDateId;

            return projectTimeBlockSaveDTO;
        }

        private ActionResult CreateTimeBillingInvoiceRow(CompEntities entities, TransactionScope transaction, List<TimeCodeTransaction> timeCodeTransactions, int invoiceQuantity, int employeeId, DateTime newOrderRowdate, DateTime timeRowdate, CustomerInvoice order, int targetCustomerInvoiceRowId, int actorCompanyId, CustomerInvoiceRow sourceCustomerInvoiceRow = null)
        {
            ActionResult result;

            Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8319, "Ingen anställd hittades"));

            var erpTimeCodeTransactions = timeCodeTransactions.Where(t => t.State == (int)SoeEntityState.Active && (t.TimeBlockId == null || t.TimeBlockId == 0)).ToList();
            var timeCodeTransaction = erpTimeCodeTransactions.FirstOrDefault();
            if (timeCodeTransaction == null)
            {
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(10073, "Inga transaktioner hittades"));
            }
            var timeInvoiceTransaction = timeCodeTransaction.TimeInvoiceTransaction.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);
            if (sourceCustomerInvoiceRow == null)
            {
                sourceCustomerInvoiceRow = InvoiceManager.GetCustomerInvoiceRow(entities, timeInvoiceTransaction?.CustomerInvoiceRowId.GetValueOrDefault() ?? 0, true);
            }

            if (sourceCustomerInvoiceRow == null)
            {
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8531, "Orderrad kunde inte hittas"));
            }

            var quantity = ConvertMinutesToQuantity(sourceCustomerInvoiceRow.ProductUnit, invoiceQuantity);


            //Remove time from old invoice row
            result = UpdateInvoiceRowFromTime(entities, transaction, order, employee, timeCodeTransaction.TimeCodeId, true, actorCompanyId, 0, invoiceQuantity, timeCodeTransaction.TimeCodeId, false, timeRowdate, 0, 0, sourceCustomerInvoiceRow);
            if (!result.Success)
                return result;

            //Add new invoice row...
            if (targetCustomerInvoiceRowId > 0)
            {
                result = UpdateInvoiceRowFromTime(entities, transaction, order, employee, timeCodeTransaction.TimeCodeId, true, actorCompanyId, invoiceQuantity, null, timeCodeTransaction.TimeCodeId, false, timeRowdate, 0, targetCustomerInvoiceRowId);
                if (!result.Success)
                    return result;

                timeInvoiceTransaction.CustomerInvoiceRowId = targetCustomerInvoiceRowId;
                var row = result.Value as CustomerInvoiceRow;
                if (row != null && row.Date < newOrderRowdate)
                {
                    row.Date = newOrderRowdate;
                }
            }
            else
            {
                var purchasePrice = CalculatePurchasePrice(entities, sourceCustomerInvoiceRow.ProductId.Value, employee, timeRowdate, order.ProjectId);
                if (purchasePrice == 0)
                {
                    purchasePrice = sourceCustomerInvoiceRow.PurchasePrice;
                }

                result = InvoiceManager.SaveCustomerInvoiceRow(transaction, entities, actorCompanyId, order, 0, sourceCustomerInvoiceRow.ProductId.Value, quantity, sourceCustomerInvoiceRow.Amount, string.Empty, SoeInvoiceRowType.ProductRow, null, null, true, true, false, true, productPurchasePrice: purchasePrice, productRowType: SoeProductRowType.TimeBillingRow);
                if (!result.Success)
                    return result;

                var row = result.Value as CustomerInvoiceRow;

                row.Date = newOrderRowdate;

                timeInvoiceTransaction.CustomerInvoiceRowId = row.CustomerInvoiceRowId;

                result.IntegerValue = row.CustomerInvoiceRowId;
            }

            return result;
        }

        private TimeInvoiceTransaction GetTimeInvoiceTransaction(List<TimeCodeTransaction> timeCodeTransactions)
        {
            if (timeCodeTransactions == null)
                return null;

            int state = (int)SoeEntityState.Active;
            var timeinvoiceTransactions = timeCodeTransactions.FirstOrDefault(tct => tct.TimeInvoiceTransaction.Any(x => x.State == state))?.TimeInvoiceTransaction;
            return timeinvoiceTransactions?.FirstOrDefault(tit => tit.State == state);
        }

        public ActionResult CreateTransactionsForProjectTimeBlock(CompEntities entities, TransactionScope transaction, ProjectTimeBlock projectTimeBlock, int timeCodeId, DateTime? date, int? previousInvoiceTimeInMinutes, int? oldWorkTimeInMinutes, bool updateInvoicedTime, int previousTimeCodeId, int previousCustomerInvoiceRowId = 0)
        {
            bool delete = projectTimeBlock.State == (int)SoeEntityState.Deleted; // || (input.InvoiceQuantity == 0 && input.TimePayrollQuantity == 0 && string.IsNullOrEmpty(input.ExternalNote) && string.IsNullOrEmpty(input.InternalNote));

            #region Permissions and settings

            if ((!FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_Edit, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId, entities)))
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8318, "Projektbehörighet saknas"));

            bool projectLimitOrderToProjectUsers = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectLimitOrderToProjectUsers, this.UserId, this.ActorCompanyId, 0);

            #endregion

            #region Prereq

            #region See if user is allowed to registrate time

            if (projectTimeBlock.ProjectId.HasValue)
            {
                if (projectLimitOrderToProjectUsers && projectTimeBlock.CustomerInvoiceId.HasValue && projectTimeBlock.CustomerInvoiceId.Value > 0)
                {
                    if (GetProjectUsersCount(entities, projectTimeBlock.ProjectId.Value) > 0)
                    {
                        bool userExits = UserIsProjectUser(entities, projectTimeBlock.ProjectId.Value, base.UserId);
                        if (!userExits)
                            return new ActionResult((int)ActionResultSave.NothingSaved, GetText(9075, "Du finns ej upplagd som projektdeltagare och kan därför inte registrera tid."));
                    }
                }
            }

            #endregion

            #region Get employee

            Employee employee = projectTimeBlock.EmployeeId == 0 ? EmployeeManager.GetEmployeeByUser(entities, base.ActorCompanyId, base.UserId) : EmployeeManager.GetEmployee(entities, projectTimeBlock.EmployeeId, base.ActorCompanyId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8319, "Ingen anställd hittades"));

            #endregion

            #region Get InvoiceProduct and TimeCode

            TimeCode timecode = TimeCodeManager.GetTimeCodeWithInvoiceProduct(entities, timeCodeId, base.ActorCompanyId);

            #endregion

            #region Get Order

            CustomerInvoice order = null;

            if (projectTimeBlock.CustomerInvoiceId.HasValue)
            {
                order = InvoiceManager.GetCustomerInvoice(entities, projectTimeBlock.CustomerInvoiceId.Value, loadOrigin: true, loadActor: true, loadInvoiceRow: true, loadInvoiceAccountRow: true, loadProject: true);

                if (order == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8321, "Order kunde inte hittas"));

                if (order.Project == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8446, "Ordern är inte kopplad till ett projekt"));

                if (!order.ProjectReference.IsLoaded)
                    order.ProjectReference.Load();
                if (!order.CurrencyReference.IsLoaded)
                    order.CurrencyReference.Load();

                var customer = CustomerManager.GetCustomerSmall(entities, order.ActorId.Value);
                if (customer == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8324, "Kund kunde inte hittas"));
            }

            #endregion

            #endregion

            int invoiceTimeInMinutes = delete ? 0 : projectTimeBlock.InvoiceQuantity;

            var existingTimeInvoiceTransaction = GetTimeInvoiceTransaction(projectTimeBlock.TimeCodeTransaction.ToList());

            var result = UpdateInvoiceRowFromTime(entities, transaction, order, employee, timeCodeId, updateInvoicedTime, base.ActorCompanyId, invoiceTimeInMinutes, previousInvoiceTimeInMinutes, previousTimeCodeId, delete, projectTimeBlock.TimeBlockDate.Date, previousCustomerInvoiceRowId);
            if (!result.Success)
            {
                return result;
            }
            CustomerInvoiceRow row = result.Value as CustomerInvoiceRow;
            var quantityDifference = result.IntegerValue2;

            if (order == null)
            {
                quantityDifference = invoiceTimeInMinutes;
            }

            #region Transactions

            if ((!delete && timecode != null) && !((order == null && quantityDifference == 0)))
            {
                //check if new TimeInvoiceTransaction will be created by CreateTransactionsForTimeProjectRegistration so mark existing one for delete
                var deleteTimeInvoiceTransaction = (row != null) &&
                                                    (existingTimeInvoiceTransaction != null) &&
                                                    (existingTimeInvoiceTransaction.CustomerInvoiceRowId != row.CustomerInvoiceRowId) &&
                                                    (timeCodeId != previousTimeCodeId);
                if (deleteTimeInvoiceTransaction)
                {
                    existingTimeInvoiceTransaction.State = (int)SoeEntityState.Deleted;
                }
                result = CreateTransactionsForTimeProjectRegistration(entities, projectTimeBlock, timecode, (int)projectTimeBlock.ProjectId, order != null && order.ActorId != null ? (int)order.ActorId : 0, date.Value, row, updateInvoicedTime);
            }

            #endregion

            return result;

        }

        public ProjectInvoiceWeek UpdateProjectInvoiceWeekOnInvoiceDay(int projectInvoiceDayId, DateTime beginningOfWeek, int orderId, int employeeId, int actorCompanyId, int timeCodeId, int projectId = 0)
        {
            using (CompEntities entities = new CompEntities())
            {
                if (projectId == 0)
                {
                    var order = InvoiceManager.GetCustomerInvoice(entities, orderId);
                    projectId = order.ProjectId.HasValue ? order.ProjectId.Value : 0;
                }
                var projectInvoiceDay = GetProjectInvoiceDay(entities, projectInvoiceDayId);
                return UpdateProjectInvoiceWeekOnInvoiceDay(entities, projectInvoiceDay, beginningOfWeek, orderId, projectId, employeeId, actorCompanyId, timeCodeId, true);
            }
        }

        public ProjectInvoiceWeek UpdateProjectInvoiceWeekOnInvoiceDay(CompEntities entities, ProjectInvoiceDay projectInvoiceDay, DateTime beginningOfWeek, int orderId, int projectId, int employeeId, int actorCompanyId, int timeCodeId, bool saveChanges)
        {

            var projectInvoiceWeek = GetProjectInvoiceWeek(entities, actorCompanyId, beginningOfWeek, projectId, orderId, (int)SoeProjectRecordType.Order, employeeId, timeCodeId);

            if (projectInvoiceWeek == null)
            {
                projectInvoiceWeek = ProjectManager.CreateProjectInvoiceWeekForOrder(beginningOfWeek, orderId, employeeId, actorCompanyId, timeCodeId, projectId);
                entities.ProjectInvoiceWeek.AddObject(projectInvoiceWeek);
                entities.SaveChanges();
            }

            if (projectInvoiceDay != null)
            {
                projectInvoiceDay.ProjectInvoiceWeekId = projectInvoiceWeek.ProjectInvoiceWeekId;

                if (saveChanges)
                    entities.SaveChanges();
            }

            return projectInvoiceWeek;
        }

        private ProjectInvoiceWeek CreateProjectInvoiceWeekForOrder(DateTime beginningOfWeek, int orderId, int employeeId, int actorCompanyId, int timeCodeId, int projectId)
        {
            return new ProjectInvoiceWeek()
            {
                Date = beginningOfWeek,
                RecordId = orderId,
                RecordType = (int)SoeProjectRecordType.Order,

                //Set FK
                ProjectId = projectId,
                EmployeeId = employeeId,
                UserId = base.UserId,
                ActorCompanyId = actorCompanyId,
                TimeCodeId = timeCodeId,
            };
        }

        private InvoiceProduct GetInvoiceProductFromTimeCode(CompEntities entities, int timeCodeId, int actorCompanyId)
        {
            TimeCode timecode = TimeCodeManager.GetTimeCodeWithInvoiceProduct(entities, timeCodeId, actorCompanyId);
            if (timecode != null && timecode.TimeCodeInvoiceProduct != null)
            {
                var timecodeinvoiceproduct = timecode.TimeCodeInvoiceProduct.FirstOrDefault();
                if (timecodeinvoiceproduct != null)
                    return timecodeinvoiceproduct.InvoiceProduct;
            }
            return null;
        }

        private decimal ConvertMinutesToQuantity(ProductUnit productUnit, decimal? invoiceTimeInMinutes)
        {
            if (productUnit != null && productUnit.Code.Trim().ToLower().StartsWith("min"))
                return invoiceTimeInMinutes.Value;
            else
                return Convert.ToDecimal(invoiceTimeInMinutes.Value / 60M);
        }

        private decimal ConvertQuantityToMinutes(ProductUnit productUnit, decimal? quantity)
        {
            if (productUnit != null && productUnit.Code.Trim().ToLower().StartsWith("min"))
                return quantity.Value;
            else
                return Convert.ToDecimal(quantity.Value * 60M);
        }

        private void DeleteTimeCodeTransactions(ProjectInvoiceDay projectInvoiceDay, bool createProjectTimeBlock, out int previousCustomerInvoiceRowId)
        {
            previousCustomerInvoiceRowId = 0;
            if (projectInvoiceDay.TimeCodeTransaction != null)
            {
                foreach (var tct in projectInvoiceDay.TimeCodeTransaction.Where(x => x.State == (int)SoeEntityState.Active).ToList())
                {
                    tct.ProjectInvoiceDayId = null;
                    tct.State = (int)SoeEntityState.Deleted;

                    foreach (var tpt in tct.TimePayrollTransaction.ToList())
                    {
                        ChangeEntityState(tpt, SoeEntityState.Deleted);
                    }

                    foreach (var tit in tct.TimeInvoiceTransaction.Where(x => x.State == (int)SoeEntityState.Active).ToList())
                    {
                        ChangeEntityState(tit, SoeEntityState.Deleted);
                        previousCustomerInvoiceRowId = tit.CustomerInvoiceRowId.GetValueOrDefault();
                    }

                    if (createProjectTimeBlock && tct.ProjectTimeBlockId != null)
                    {
                        if (!tct.ProjectTimeBlockReference.IsLoaded)
                            tct.ProjectTimeBlockReference.Load();

                        ChangeEntityState(tct.ProjectTimeBlock, SoeEntityState.Deleted);
                    }
                }
            }
        }

        public ActionResult SaveProjectInvoiceDay(int actorCompanyId, int roleId, int orderId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId, string commentExternal = null, int? employeeId = null, bool ignoreExternalComment = true, bool createProjectTimeBlock = false, int? projectInvoiceWeekId = 0, int? projectInvoiceDayId = 0)
        {
            var result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveProjectInvoiceDay(entities, transaction, actorCompanyId, roleId, orderId, date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId, commentExternal, employeeId, ignoreExternalComment, createProjectTimeBlock, projectInvoiceWeekId, projectInvoiceDayId);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception exp)
                {
                    LogError($"SaveProjectInvoiceDay exception: actorCompanyId:{actorCompanyId} orderId:{orderId} date:{date}");
                    base.LogError(exp, log);
                    result = new ActionResult(exp);
                }
                finally
                {
                    if (!result.Success)
                    {
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }
                    entities.Connection.Close();
                }
            }

            if (result.Success && result.IntegerValue > 0)
            {
                result.Value = GetProjectInvoiceDay(result.IntegerValue);
            }
            return result;
        }

        private ActionResult SaveProjectInvoiceDay(CompEntities entities, TransactionScope transaction, int actorCompanyId, int roleId, int orderId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId, string commentExternal = null, int? employeeId = null, bool ignoreExternalComment = true, bool createProjectTimeBlock = false, int? projectInvoiceWeekId = 0, int? projectInvoiceDayId = 0)
        {
            ProjectInvoiceDay projectInvoiceDay = null;
            int? previousInvoiceTimeInMinutes = null;
            var result = new ActionResult();
            bool delete = invoiceTimeInMinutes == 0 && workTimeInMinutes == 0 && string.IsNullOrEmpty(commentExternal) && string.IsNullOrEmpty(note);

            bool workedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_WorkedTime, Permission.Readonly, roleId, actorCompanyId, entities: entities);
            bool invoicedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, roleId, actorCompanyId, entities: entities);
            bool invoiceTimeAsWorkTime = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectInvoiceTimeAsWorkTime, 0, actorCompanyId, 0);

            bool updateInvoicedTime = invoicedTimePermission || (!invoicedTimePermission && invoiceTimeAsWorkTime);
            invoiceTimeInMinutes = (!invoicedTimePermission && invoiceTimeAsWorkTime) ? workTimeInMinutes : invoiceTimeInMinutes;

            bool projectLimitOrderToProjectUsers = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectLimitOrderToProjectUsers, this.UserId, this.ActorCompanyId, 0);

            #region Project permission

            if ((!FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_Edit, Permission.Modify, roleId, actorCompanyId, entities: entities)))
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8318, "Projektbehörighet saknas"));

            #endregion

            int previousTimeCodeId = 0;
            int previousCustomerInvoiceRowId = 0;

            #region Prereq

            #region Decide SoeProjectDayType

            SoeProjectDayType dayType = SoeProjectDayType.Monday;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dayType = SoeProjectDayType.Sunday;
                    break;
                case DayOfWeek.Monday:
                    dayType = SoeProjectDayType.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    dayType = SoeProjectDayType.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    dayType = SoeProjectDayType.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    dayType = SoeProjectDayType.Thursday;
                    break;
                case DayOfWeek.Friday:
                    dayType = SoeProjectDayType.Friday;
                    break;
                case DayOfWeek.Saturday:
                    dayType = SoeProjectDayType.Saturday;
                    break;
                default:
                    break;
            }

            #endregion

            #region Get employee

            Employee employee = employeeId == null || employeeId == 0 ? EmployeeManager.GetEmployeeByUser(entities, actorCompanyId, base.UserId) : EmployeeManager.GetEmployee(entities, employeeId.Value, actorCompanyId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8319, "Ingen anställd hittades"));

            #endregion

            #region Get Order

            CustomerInvoice order = InvoiceManager.GetCustomerInvoice(entities, orderId, loadOrigin: true, loadActor: true, loadInvoiceRow: true, loadInvoiceAccountRow: true, loadProject: true);
            if (order == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8321, "Order kunde inte hittas"));

            if (order.Project == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8446, "Ordern är inte kopplad till ett projekt"));

            int projectId = order.Project.ProjectId;
            if (projectId == 0)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8326, "Projekt saknas"));

            if (!order.ProjectReference.IsLoaded)
                order.ProjectReference.Load();
            if (!order.CurrencyReference.IsLoaded)
                order.CurrencyReference.Load();

            #endregion

            #region See if user is allowed to registrate time

            if (order.ProjectId.HasValue && projectLimitOrderToProjectUsers == true)
            {
                if (GetProjectUsersCount(entities, order.ProjectId.Value) > 0)
                {
                    bool userExits = UserIsProjectUser(entities, order.ProjectId.Value, base.UserId);
                    if (!userExits)
                        return new ActionResult((int)ActionResultSave.NothingSaved, GetText(9075, "Du finns ej upplagd som projektdeltagare och kan därför inte registrera tid."));
                }
            }

            #endregion

            #region Get Customer
            var customer = CustomerManager.GetCustomerSmall(entities, order.ActorId.Value);
            if (customer == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8324, "Kund kunde inte hittas"));

            #endregion

            #endregion

            #region Update project time

            #region ProjectInvoiceWeek/ProjectInvoiceDay

            DateTime beginningOfWeek = CalendarUtility.GetFirstDateOfWeek(date, offset: DayOfWeek.Monday);//we need to store the week on a monday
            ProjectInvoiceWeek projectInvoiceweek = null;
            if (projectInvoiceWeekId.HasValue && projectInvoiceWeekId.Value > 0)
            {
                projectInvoiceweek = GetProjectInvoiceWeek(entities, actorCompanyId, projectInvoiceWeekId.Value);
            }
            else
            {
                projectInvoiceweek = GetProjectInvoiceWeek(entities, actorCompanyId, beginningOfWeek, projectId, orderId, (int)SoeProjectRecordType.Order, employee.EmployeeId, timeCodeId);
            }

            if (projectInvoiceweek == null && delete)
            {
                LogError($"SaveProjectInvoiceDay:Delete has projectInvoiceweek=null actorCompanyId:{actorCompanyId} orderId:{orderId} employeeId:{employee.EmployeeId} date:{date}");
                return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(7438, "Veckorapport kunde inte hittas för datum {0} och tidkod {1}"), beginningOfWeek, timeCodeId));
            }
            if (projectInvoiceweek == null && !delete)
            {
                #region Add

                projectInvoiceweek = new ProjectInvoiceWeek()
                {
                    Date = beginningOfWeek,
                    RecordId = orderId,
                    RecordType = (int)SoeProjectRecordType.Order,

                    //Set FK
                    ProjectId = projectId,
                    EmployeeId = employee.EmployeeId,
                    UserId = base.UserId,
                    ActorCompanyId = actorCompanyId,
                    TimeCodeId = timeCodeId,
                };


                projectInvoiceDay = new ProjectInvoiceDay
                {
                    DayType = (int)dayType,
                    InvoiceTimeInMinutes = updateInvoicedTime ? invoiceTimeInMinutes : 0,
                    WorkTimeInMinutes = workedTimePermission ? workTimeInMinutes : 0,
                    Note = note,
                    Date = date,
                };

                if (commentExternal != null)
                    projectInvoiceDay.CommentExternal = commentExternal;

                SetCreatedProperties(projectInvoiceweek);

                projectInvoiceweek.ProjectInvoiceDay.Add(projectInvoiceDay);

                entities.ProjectInvoiceWeek.AddObject(projectInvoiceweek);
                entities.ProjectInvoiceDay.AddObject(projectInvoiceDay);

                #endregion

            }
            else
            {

                var chaningTimeCode = timeCodeId != projectInvoiceweek.TimeCodeId;
                if (projectInvoiceDayId.HasValue && projectInvoiceDayId.Value > 0)
                {
                    projectInvoiceDay = GetProjectInvoiceDay(entities, projectInvoiceDayId.Value, false, true);
                }
                else
                {
                    projectInvoiceDay = GetProjectInvoiceDay(entities, projectInvoiceweek.ProjectInvoiceWeekId, dayType, delete);
                }

                if (delete)
                {
                    #region Delete

                    if (projectInvoiceDay == null)
                    {
                        LogError($"SaveProjectInvoiceDay:Delete has projectInvoiceDay=null actorCompanyId:{actorCompanyId} orderId:{orderId} employeeId:{employee.EmployeeId} date:{date}");
                        return new ActionResult((int)ActionResultSave.NothingSaved, string.Format("Rapporteringsdag kunde inte hittas för datum {0}", date));
                    }
                    else
                    {
                        previousInvoiceTimeInMinutes = projectInvoiceDay.InvoiceTimeInMinutes;

                        DeleteTimeCodeTransactions(projectInvoiceDay, createProjectTimeBlock, out previousCustomerInvoiceRowId);

                        entities.ProjectInvoiceDay.DeleteObject(projectInvoiceDay);
                    }
                    #endregion
                }
                else
                {
                    #region Update

                    SetModifiedProperties(projectInvoiceweek);

                    if (projectInvoiceDay == null)
                    {
                        projectInvoiceDay = new ProjectInvoiceDay
                        {
                            DayType = (int)dayType,
                            Date = date,
                            WorkTimeInMinutes = workedTimePermission ? workTimeInMinutes : 0,
                            InvoiceTimeInMinutes = updateInvoicedTime ? invoiceTimeInMinutes : 0,
                            Note = note,

                            //Set FK
                            ProjectInvoiceWeekId = projectInvoiceweek.ProjectInvoiceWeekId,
                        };

                        entities.ProjectInvoiceDay.AddObject(projectInvoiceDay);
                    }
                    else
                    {
                        previousTimeCodeId = projectInvoiceweek.TimeCodeId.GetValueOrDefault();
                        if (chaningTimeCode)
                        {
                            projectInvoiceweek = UpdateProjectInvoiceWeekOnInvoiceDay(entities, projectInvoiceDay, beginningOfWeek, order.InvoiceId, order.ProjectId.HasValue ? order.ProjectId.Value : 0, employee.EmployeeId, actorCompanyId, timeCodeId, false);
                            DeleteTimeCodeTransactions(projectInvoiceDay, createProjectTimeBlock, out previousCustomerInvoiceRowId);
                        }
                        else
                        {
                            if (updateInvoicedTime && projectInvoiceDay.TimeCodeTransaction != null)
                            {
                                var existingTimeInvoiceTransaction = GetTimeInvoiceTransaction(projectInvoiceDay.TimeCodeTransaction.ToList());
                                previousCustomerInvoiceRowId = existingTimeInvoiceTransaction != null ? existingTimeInvoiceTransaction.CustomerInvoiceRowId.GetValueOrDefault() : 0;
                            }
                        }
                        previousInvoiceTimeInMinutes = projectInvoiceDay.InvoiceTimeInMinutes;

                        if (updateInvoicedTime && projectInvoiceDay.InvoiceTimeInMinutes == invoiceTimeInMinutes && !chaningTimeCode)
                        {
                            updateInvoicedTime = false;
                        }

                        if (updateInvoicedTime)
                        {
                            projectInvoiceDay.InvoiceTimeInMinutes = invoiceTimeInMinutes;
                        }
                        else
                        {
                            invoiceTimeInMinutes = previousInvoiceTimeInMinutes.Value;
                        }

                        if (workedTimePermission)
                            projectInvoiceDay.WorkTimeInMinutes = workTimeInMinutes;

                        projectInvoiceDay.Note = note;
                    }

                    if (commentExternal != null)
                        projectInvoiceDay.CommentExternal = commentExternal;

                    #endregion
                }
            }

            #endregion

            result = UpdateInvoiceRowFromTime(entities, transaction, order, employee, timeCodeId, updateInvoicedTime, actorCompanyId, invoiceTimeInMinutes, previousInvoiceTimeInMinutes, previousTimeCodeId, delete, date, previousCustomerInvoiceRowId);
            if (!result.Success)
            {
                return result;
            }

            var quantityDifference = result.IntegerValue2;
            CustomerInvoiceRow row = result.Value as CustomerInvoiceRow;

            #region Transactions

            if (!delete)
            {
                var days = new List<ProjectInvoiceDay>() { projectInvoiceDay };
                var invoiceRowMapping = new List<ProjectInvoiceDayInvoiceRowMappingDTO>();
                var invoiceRowIds = new List<Tuple<int, int>>();

                if (row != null)
                {
                    invoiceRowMapping.Add(new ProjectInvoiceDayInvoiceRowMappingDTO() { InvoiceRowTempId = row.CustomerInvoiceRowId, ProjectInvoiceDayTempId = projectInvoiceDay.ProjectInvoiceDayTempId, QuantityDifference = quantityDifference });
                    invoiceRowIds.Add(new Tuple<int, int>(row.CustomerInvoiceRowId, row.CustomerInvoiceRowId));
                }
                result = CreateTransactionsForTimeProjectRegistration(entities, days, invoiceRowMapping, invoiceRowIds, actorCompanyId, employee.EmployeeId, timeCodeId, projectInvoiceweek.ProjectId, order.ActorId != null ? (int)order.ActorId : 0, date, ignoreExternalComment);
                if (!result.Success)
                    return result;
            }

            #endregion

            #endregion

            SetModifiedProperties(order);
            result = SaveChanges(entities, transaction);

            if (!result.Success)
                return result;

            if (createProjectTimeBlock && !delete)
            {
                TimeCodeTransaction timeCodeTransaction = TimeTransactionManager.GetTimeCodeTransactionsForProjectInvoiceDay(entities, projectInvoiceDay.ProjectInvoiceDayId).FirstOrDefault();
                if (timeCodeTransaction != null)
                    result = CreateProjectTimeBlock(entities, transaction, timeCodeTransaction, projectInvoiceDay.Date, projectInvoiceweek.EmployeeId, projectInvoiceweek.ProjectId, projectInvoiceweek.RecordId, projectInvoiceweek.RecordType, projectInvoiceDay.InvoiceTimeInMinutes, projectInvoiceDay.WorkTimeInMinutes, projectInvoiceDay.CommentExternal, projectInvoiceDay.Note);
            }


            //}

            if (result.Success && createProjectTimeBlock && result.Value != null && result.Value.GetType() == typeof(List<ProjectTimeBlock>))
            {
                List<ProjectTimeBlock> projectTimeBlocks = result.Value as List<ProjectTimeBlock>;

                //Create transactions for salary
                TimeEngineManager tem = new TimeEngineManager(parameterObject, base.ActorCompanyId, base.UserId);
                result = tem.GenerateTimeBlocksBasedOnProjectTimeBlocks(projectTimeBlocks, false);
            }

            if (!delete && result.Success && projectInvoiceDay != null)
            {
                result.IntegerValue = projectInvoiceDay.ProjectInvoiceDayId;
            }

            return result;
        }

        private ActionResult UpdateInvoiceRowFromTime(CompEntities entities, TransactionScope transaction, CustomerInvoice order, Employee employee, int timeCodeId, bool updateInvoicedTime, int actorCompanyId, int invoiceTimeInMinutes, int? previousInvoiceTimeInMinutes, int previousTimeCodeId, bool delete, DateTime date, int previousCustomerInvoiceRowId = 0, int addToCustomerInvoiceRowId = 0, CustomerInvoiceRow sourceCustomerInvoiceRow = null)
        {
            var result = new ActionResult();
            decimal productPrice = 0;
            var changeTimeCode = previousTimeCodeId != timeCodeId;

            if (order == null)
                return new ActionResult(GetText(8321, "Order kunde inte hittas"));

            InvoiceProduct invoiceProduct = GetInvoiceProductFromTimeCode(entities, timeCodeId, actorCompanyId);
            InvoiceProduct previousinvoiceProduct = (previousTimeCodeId > 0) && (changeTimeCode) ? GetInvoiceProductFromTimeCode(entities, previousTimeCodeId, actorCompanyId) : null;

            if (!delete && changeTimeCode)
            {
                //Change from not invoiced timecode or no timecode at all to one that does
                if (previousInvoiceTimeInMinutes.HasValue && previousinvoiceProduct == null && invoiceProduct != null)
                {
                    previousInvoiceTimeInMinutes = 0;
                }
                //Change from invoiced timecode to not invoiced
                else if (previousInvoiceTimeInMinutes.HasValue && previousinvoiceProduct != null && invoiceProduct == null)
                {
                    invoiceTimeInMinutes = 0;
                }
            }

            CustomerInvoiceRow row = null;
            int quantityDifference = previousInvoiceTimeInMinutes.HasValue ? invoiceTimeInMinutes - previousInvoiceTimeInMinutes.Value : invoiceTimeInMinutes;

            if ((invoiceProduct != null || previousinvoiceProduct != null) && updateInvoicedTime)
            {
                decimal quantity = 0;
                int orderRowId = 0;
                var previousRowWasTimeBillingRow = false;

                int defaultInvoiceProductUnitId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultInvoiceProductUnit, 0, actorCompanyId, 0);
                ProductUnit defaultInvoiceProductUnit = ProductManager.GetProductUnit(entities, defaultInvoiceProductUnitId);

                List<CustomerInvoiceRow> invoiceProductRows = null;
                if (sourceCustomerInvoiceRow != null)
                {
                    invoiceProductRows = new List<CustomerInvoiceRow>() { sourceCustomerInvoiceRow };
                }
                else if (invoiceProduct != null)
                {
                    invoiceProductRows = InvoiceManager.GetUnAttestedCustomerInvoiceRows(entities, actorCompanyId, order.InvoiceId, invoiceProduct.ProductId, TermGroup_AttestEntity.Order, true, true).Where(i => i.IsTimeProjectRow).ToList();
                }

                if (delete && quantityDifference != 0 && (invoiceProductRows == null || invoiceProductRows.Count == 0))
                {
                    // Continue with delete of block and transactions
                    return new ActionResult();
                    //return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8531, "Orderrad kunde inte hittas"));
                }

                if (previousCustomerInvoiceRowId > 0 && invoiceProductRows != null)
                {
                    var existingRow = invoiceProductRows.FirstOrDefault(x => x.CustomerInvoiceRowId == previousCustomerInvoiceRowId);
                    if (existingRow != null && (existingRow.IsTimeBillingRow() || (!changeTimeCode && addToCustomerInvoiceRowId == 0)))
                    {
                        invoiceProductRows.Clear();
                        invoiceProductRows.Add(existingRow);
                    }
                }

                //If changing timecode to a one with a new productId we need to remove the time from the old timerow
                if (previousTimeCodeId > 0)
                {
                    if (previousinvoiceProduct != null && ((invoiceProduct == null) || (invoiceProduct != null && previousinvoiceProduct.ProductId != invoiceProduct.ProductId)))
                    {
                        var prevInvoiceProductRow = InvoiceManager.GetCustomerInvoiceRow(entities, previousCustomerInvoiceRowId);
                        if (prevInvoiceProductRow != null)
                        {
                            previousRowWasTimeBillingRow = prevInvoiceProductRow.IsTimeBillingRow();
                            prevInvoiceProductRow.Quantity = prevInvoiceProductRow.Quantity.HasValue ? prevInvoiceProductRow.Quantity - ConvertMinutesToQuantity(prevInvoiceProductRow.ProductUnit, previousInvoiceTimeInMinutes.HasValue ? previousInvoiceTimeInMinutes.Value : invoiceTimeInMinutes) : 0;

                            if (prevInvoiceProductRow.Quantity == 0)
                            {
                                InvoiceManager.DeleteCustomerInvoiceRow(transaction, entities, actorCompanyId, order, prevInvoiceProductRow.CustomerInvoiceRowId, false, false, false);
                            }
                            else
                            {
                                result = InvoiceManager.SaveCustomerInvoiceRow(transaction, entities, actorCompanyId, order, prevInvoiceProductRow.CustomerInvoiceRowId, prevInvoiceProductRow.ProductId.GetValueOrDefault(), prevInvoiceProductRow.Quantity.GetValueOrDefault(), prevInvoiceProductRow.AmountCurrency, string.Empty, SoeInvoiceRowType.ProductRow, null, prevInvoiceProductRow.ProductUnitId, true, true, false, true, productPurchasePrice: prevInvoiceProductRow.PurchasePrice, productRowType: (SoeProductRowType)prevInvoiceProductRow.ProductRowType);
                                if (!result.Success)
                                {
                                    return result;
                                }
                            }

                            quantityDifference = invoiceTimeInMinutes;
                            if (previousRowWasTimeBillingRow && invoiceProductRows != null)
                            {
                                //We will always want a new product row for this one....
                                invoiceProductRows.Clear();
                            }
                            else if (!previousRowWasTimeBillingRow && invoiceProductRows != null)
                            {
                                //We dont want any TimeBillingRows get used when we swich timecode on an TimeRow
                                invoiceProductRows = invoiceProductRows.Where(i => !i.IsTimeBillingRow()).ToList();
                            }
                        }
                    }
                }

                decimal purchasePrice = invoiceProduct == null ? 0 : invoiceProduct.PurchasePrice;
                var useCalculatedCost = invoiceProduct?.UseCalculatedCost.GetValueOrDefault() ?? false;

                if (invoiceProductRows != null && invoiceProductRows.Count > 0)
                {
                    #region Update

                    row = addToCustomerInvoiceRowId > 0 ? invoiceProductRows.FirstOrDefault(r => r.CustomerInvoiceRowId == addToCustomerInvoiceRowId) : invoiceProductRows.FirstOrDefault();

                    if (row == null)
                    {
                        return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8531, "Orderrad kunde inte hittas"));
                    }

                    #region Purchase price

                    //Check if calculated cost should be set as purchase price

                    var employeeCalculatedCost = employee != null && useCalculatedCost ? EmployeeManager.GetEmployeeCalculatedCost(entities, employee, date, order.ProjectId) : 0;

                    if (!row.ProductUnitReference.IsLoaded)
                        row.ProductUnitReference.Load();

                    decimal quantityDiff = ConvertMinutesToQuantity(row.ProductUnit, quantityDifference);

                    decimal newTotalPurchasePrice = useCalculatedCost ? (quantityDiff * employeeCalculatedCost) : (quantityDiff * invoiceProduct.PurchasePrice);
                    decimal oldTotalPurchasePrice = row.Quantity.HasValue ? row.Quantity.Value * row.PurchasePrice : 0;
                    decimal totalQuantity = row.Quantity.HasValue ? row.Quantity.Value + quantityDiff : quantityDiff;

                    if (totalQuantity != 0)
                        purchasePrice = Math.Round((oldTotalPurchasePrice + newTotalPurchasePrice) / totalQuantity, 2);

                    #endregion

                    row.Quantity += ConvertMinutesToQuantity(row.ProductUnit, quantityDifference);

                    //Fix for possible prevous problems
                    if (Math.Abs(row.Quantity.GetValueOrDefault()) < 0.00001M)
                    {
                        row.Quantity = 0;
                    }

                    quantity = row.Quantity.GetValueOrDefault();

                    if (row.Quantity.Value == 0)
                    {
                        orderRowId = 0;
                        InvoiceManager.DeleteCustomerInvoiceRow(transaction, entities, actorCompanyId, order, row.CustomerInvoiceRowId, false, true, true);
                    }
                    else
                    {
                        orderRowId = row.CustomerInvoiceRowId;
                        //keep existing price on row...
                        if (row.AmountCurrency > 0)
                            productPrice = row.AmountCurrency;
                    }

                    #endregion
                }
                else
                {
                    #region Purchase price

                    //Check if calculated cost should be set as purchase price
                    if (useCalculatedCost)
                    {
                        purchasePrice = employee != null ? EmployeeManager.GetEmployeeCalculatedCost(entities, employee, date, order.ProjectId) : 0;
                    }

                    #endregion

                    #region Create new Invoicerow

                    quantity = ConvertMinutesToQuantity(defaultInvoiceProductUnit, quantityDifference);
                    orderRowId = 0;

                    #endregion
                }

                bool rowdeleted = (orderRowId == 0 && quantity == 0);

                #region Get productprice

                bool useQuantityPrices = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingUseQuantityPrices, this.UserId, this.ActorCompanyId, 0);
                if (order.PriceListTypeId.HasValue && invoiceProduct != null && !rowdeleted && (useQuantityPrices || productPrice == 0))
                {
                    InvoiceProductPriceResult priceResult = ProductManager.GetProductPrice(entities, actorCompanyId, new ProductPriceRequestDTO { PriceListTypeId = order.PriceListTypeId.Value, ProductId = invoiceProduct.ProductId, CustomerId = order.ActorId.Value, CurrencyId = order.CurrencyId, Quantity = quantity }, includeCustomerPrices: true);
                    if (priceResult != null)
                    {
                        productPrice = priceResult.SalesPrice;
                        if (priceResult.CurrencyDiffer)
                            productPrice = (productPrice / order.CurrencyRate);
                    }
                }

                #endregion

                if (!rowdeleted)
                {
                    var productRowType = previousRowWasTimeBillingRow ? SoeProductRowType.TimeBillingRow : SoeProductRowType.TimeRow;
                    result = InvoiceManager.SaveCustomerInvoiceRow(transaction, entities, actorCompanyId, order, orderRowId, invoiceProduct.ProductId, quantity, productPrice, string.Empty, SoeInvoiceRowType.ProductRow, null, null, true, true, false, true, productPurchasePrice: purchasePrice, productRowType: productRowType, orderRow: row);

                    if (!result.Success)
                        return result;

                    row = result.Value as CustomerInvoiceRow;
                }
                else if (previousInvoiceTimeInMinutes != 0 && invoiceTimeInMinutes == 0)
                {
                    int sysCurrencyIdInvoice = CountryCurrencyManager.GetSysCurrencyId(entities, order.CurrencyId);
                    int sysCurrencyIdBase = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, base.ActorCompanyId);
                    InvoiceManager.UpdateInvoiceAfterRowModification(entities, order, order.ActiveCustomerInvoiceRows.Count + 1, actorCompanyId, order.Origin.Type != (int)SoeOriginType.CustomerInvoice, sysCurrencyIdInvoice != sysCurrencyIdBase);
                }
            }

            result.Value = row;
            result.IntegerValue = row == null ? 0 : row.CustomerInvoiceRowId;
            result.IntegerValue2 = quantityDifference;

            return result;
        }

        public ActionResult RecalculateTimeRow(int customerInvoiceRowId, int actorCompanyId)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                entities.Connection.Open();

                bool useProjectTimeBlocks = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

                using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    var invoiceTransactionsQuery = entities.TimeInvoiceTransaction.
                                Where(t => t.ActorCompanyId == actorCompanyId && t.CustomerInvoiceRowId == customerInvoiceRowId && t.State == (int)SoeEntityState.Active);

                    if (useProjectTimeBlocks)
                    {
                        invoiceTransactionsQuery = invoiceTransactionsQuery.Include("TimeCodeTransaction.ProjectTimeBlock");
                    }
                    else
                    {
                        invoiceTransactionsQuery = invoiceTransactionsQuery.Include("TimeCodeTransaction.ProjectInvoiceDay");
                    }

                    var invoiceTransactions = invoiceTransactionsQuery.ToList();

                    if (useProjectTimeBlocks)
                    {
                        //Check for differences between invoicequantity in projectimeblock and TimeInvoiceTransaction.....
                        var invoiceQuantityDiscrepancies = invoiceTransactionsQuery.Where(t => t.TimeCodeTransaction.ProjectTimeBlock != null && t.InvoiceQuantity != t.TimeCodeTransaction.ProjectTimeBlock.InvoiceQuantity).ToList();
                        foreach (var invTrans in invoiceQuantityDiscrepancies)
                        {
                            if (invTrans.TimeCodeTransaction.ProjectTimeBlock.State == (int)SoeEntityState.Active)
                            {
                                invTrans.InvoiceQuantity = invTrans.TimeCodeTransaction.ProjectTimeBlock.InvoiceQuantity;
                                SetModifiedProperties(invTrans);
                            }
                        }
                    }
                    else
                    {
                        //Check for differences between invoicequantity in ProjectInvoiceDay and TimeInvoiceTransaction.....
                        var invoiceQuantityDiscrepancies = invoiceTransactionsQuery.Where(t => t.TimeCodeTransaction.ProjectInvoiceDay != null && t.InvoiceQuantity != t.TimeCodeTransaction.ProjectInvoiceDay.InvoiceTimeInMinutes).ToList();
                        foreach (var invTransGrup in invoiceQuantityDiscrepancies.GroupBy(x => x.TimeCodeTransaction.ProjectInvoiceDayId))
                        {
                            var invday = invTransGrup.FirstOrDefault()?.TimeCodeTransaction.ProjectInvoiceDay;
                            if (invday != null)
                            {
                                var invTranslist = invoiceTransactionsQuery.Where(x => x.TimeCodeTransaction.ProjectInvoiceDayId == invday.ProjectInvoiceDayId).ToList();
                                var invTrans = invTranslist.FirstOrDefault();
                                invTrans.InvoiceQuantity = invday.InvoiceTimeInMinutes;
                                SetModifiedProperties(invTrans);
                                //in therory there could be several invoice transactions connected to ta projectinvoiceday so delete the rest
                                foreach (var item in invTransGrup.Where(x => x.TimeInvoiceTransactionId != invTrans.TimeInvoiceTransactionId))
                                {
                                    item.State = (int)SoeEntityState.Deleted;
                                    SetModifiedProperties(item);
                                }
                            }
                        }
                    }

                    var customerInvoiceRow = InvoiceManager.GetCustomerInvoiceRow(entities, customerInvoiceRowId);
                    var transactionQuantity = invoiceTransactions.Any() ? Convert.ToInt32(invoiceTransactions.Sum(t => t.InvoiceQuantity)) : 0;
                    var invoiceRowQuantity = Convert.ToInt32(ConvertQuantityToMinutes(customerInvoiceRow.ProductUnit, customerInvoiceRow.Quantity));
                    var timeCodeId = invoiceTransactions.FirstOrDefault()?.TimeCodeTransaction?.TimeCodeId ?? 0;

                    if (!invoiceTransactions.Any())
                    {
                        //All rows has been deleted....
                        var invoiceTransactionsDelted = entities.TimeInvoiceTransaction.
                            Include("TimeCodeTransaction").
                            Where(t => t.ActorCompanyId == actorCompanyId && t.CustomerInvoiceRowId == customerInvoiceRowId && t.State == (int)SoeEntityState.Deleted);
                        timeCodeId = invoiceTransactionsDelted.FirstOrDefault()?.TimeCodeTransaction?.TimeCodeId ?? 0;
                    }

                    if (transactionQuantity != invoiceRowQuantity)
                    {
                        var customerInvoice = InvoiceManager.GetCustomerInvoice(entities, customerInvoiceRow.InvoiceId);
                        result = UpdateInvoiceRowFromTime(entities, transaction, customerInvoice, null, timeCodeId, true, actorCompanyId, transactionQuantity, invoiceRowQuantity, timeCodeId, false, DateTime.Today, customerInvoiceRow.CustomerInvoiceRowId);
                        if (result.Success)
                        {
                            result = SaveChanges(entities, transaction);
                        }
                        if (result.Success)
                        {
                            transaction.Complete();
                        }
                    }
                }
            }

            return result;
        }

        public ActionResult SaveProjectInvoiceDayFromTimeSheet(TransactionScope transaction, CompEntities entities, bool workedTimePermission, bool invoicedTimePermission, int actorCompanyId, int employeeId, int orderId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId, string externalComment)
        {
            int rowId = 0;
            bool deleteRow = false;
            ProjectInvoiceDay projectInvoiceDay = null;
            int? oldInvoiceTimeInMinutes = null;
            ActionResult result = new ActionResult();

            try
            {
                decimal productPrice = 0;
                CustomerInvoiceRow row = null;

                #region Prereq

                bool projectLimitOrderToProjectUsers = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectLimitOrderToProjectUsers, this.UserId, this.ActorCompanyId, 0);

                #region Decide SoeProjectDayType

                SoeProjectDayType dayType = SoeProjectDayType.Monday;
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        dayType = SoeProjectDayType.Sunday;
                        break;
                    case DayOfWeek.Monday:
                        dayType = SoeProjectDayType.Monday;
                        break;
                    case DayOfWeek.Tuesday:
                        dayType = SoeProjectDayType.Tuesday;
                        break;
                    case DayOfWeek.Wednesday:
                        dayType = SoeProjectDayType.Wednesday;
                        break;
                    case DayOfWeek.Thursday:
                        dayType = SoeProjectDayType.Thursday;
                        break;
                    case DayOfWeek.Friday:
                        dayType = SoeProjectDayType.Friday;
                        break;
                    case DayOfWeek.Saturday:
                        dayType = SoeProjectDayType.Saturday;
                        break;
                    default:
                        break;
                }

                #endregion

                #region Get employee

                Employee employee = EmployeeManager.GetEmployeeIgnoreState(entities, actorCompanyId, employeeId);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8319, "Ingen anställd hittades"));

                #endregion

                #region Get InvoiceProduct and TimeCode

                InvoiceProduct invoiceProduct = null; //GetProjectDefaultInvoiceProduct(entities, actorCompanyId, employee.EmployeeId, true, false);
                TimeCode timecode = TimeCodeManager.GetTimeCodeWithInvoiceProduct(entities, timeCodeId, actorCompanyId);
                if (timecode != null && timecode.TimeCodeInvoiceProduct != null)
                {
                    var timecodeinvoiceproduct = timecode.TimeCodeInvoiceProduct.FirstOrDefault();
                    if (timecodeinvoiceproduct != null)
                        invoiceProduct = timecodeinvoiceproduct.InvoiceProduct;
                }

                #endregion

                #region Get Order

                CustomerInvoice order = InvoiceManager.GetCustomerInvoice(entities, orderId, loadOrigin: true, loadActor: true, loadInvoiceRow: true, loadInvoiceAccountRow: true, loadProject: true);
                if (order == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8321, "Order kunde inte hittas"));

                if (!order.ProjectReference.IsLoaded)
                    order.ProjectReference.Load();
                if (!order.CurrencyReference.IsLoaded)
                    order.CurrencyReference.Load();

                #endregion

                #region See if user is allowed to registrate time

                if (order.ProjectId.HasValue && projectLimitOrderToProjectUsers == true)
                {
                    if (GetProjectUsersCount(entities, order.ProjectId.Value) > 0)
                    {
                        bool userExits = UserIsProjectUser(entities, order.ProjectId.Value, base.UserId);
                        if (!userExits)
                            return new ActionResult((int)ActionResultSave.NothingSaved, GetText(9075, "Du finns ej upplagd som projektdeltagare och kan därför inte registrera tid."));
                    }
                }

                #endregion

                #region Get productprice
                if (order.PriceListTypeId.HasValue && invoiceProduct != null)
                {
                    //PriceList productPriceList = ProductManager.GetPriceList(entities, invoiceProduct.ProductId, order.PriceListTypeId.Value);
                    //if (productPriceList != null)
                    //    productPrice = productPriceList.Price;
                    InvoiceProductPriceResult priceResult = ProductManager.GetProductPrice(entities, actorCompanyId, new ProductPriceRequestDTO { PriceListTypeId = order.PriceListTypeId.Value, ProductId = invoiceProduct.ProductId, CustomerId = order.ActorId.Value, CurrencyId = order.CurrencyId });
                    if (priceResult != null)
                        productPrice = priceResult.SalesPrice;
                }
                #endregion

                #region AttestState

                AttestStateDTO initialAttestState = AttestManager.GetInitialAttestState(entities, actorCompanyId, TermGroup_AttestEntity.Order).ToDTO();
                if (initialAttestState == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8323, "Attestnivå för order saknas"));

                #endregion

                #region Get Customer
                Customer customer = CustomerManager.GetCustomer(entities, order.ActorId.Value);
                if (customer == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8324, "Kund kunde inte hittas"));
                #endregion

                if (order.Project == null)
                {
                    string suggestedProjectNr;

                    if (SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectSuggestOrderNumberAsProjectNumber, 0, actorCompanyId, 0) && !string.IsNullOrEmpty(order.InvoiceNr))
                    {
                        var numbers = GetAllProjectNumbers(entities, actorCompanyId);
                        suggestedProjectNr = GetProjectNumberCheckExisting(numbers, order.InvoiceNr, actorCompanyId);
                    }
                    else
                    {
                        suggestedProjectNr = GetLastProjectNumber(entities, actorCompanyId);
                        int.TryParse(suggestedProjectNr, out int nr);
                        nr = nr + 1;
                        suggestedProjectNr = nr.ToString();
                    }

                    TimeProject project = CreateTimeProject(entities, order, suggestedProjectNr, customer, actorCompanyId);

                    result = SaveChanges(entities);
                    if (!result.Success)
                        return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8325, "Kunde inte skapa projekt"));
                }

                int projectId = order.Project.ProjectId;
                if (projectId == 0)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8326, "Projekt saknas"));

                #endregion

                #region Update project time

                #region ProjectInvoiceWeek/ProjectInvoiceDay

                DateTime beginningOfWeek = CalendarUtility.GetFirstDateOfWeek(date);
                ProjectInvoiceWeek projectInvoiceweek = GetProjectInvoiceWeek(entities, actorCompanyId, beginningOfWeek, projectId, orderId, (int)SoeProjectRecordType.Order, employee.EmployeeId, timeCodeId);
                if (projectInvoiceweek == null)
                {
                    #region Add

                    projectInvoiceweek = new ProjectInvoiceWeek()
                    {
                        Date = beginningOfWeek,
                        RecordId = orderId,
                        RecordType = (int)SoeProjectRecordType.Order,

                        //Set FK
                        ProjectId = projectId,
                        EmployeeId = employee.EmployeeId,
                        UserId = base.UserId,
                        ActorCompanyId = actorCompanyId,
                        TimeCodeId = timeCodeId,
                    };

                    projectInvoiceDay = new ProjectInvoiceDay
                    {
                        DayType = (int)dayType,
                        InvoiceTimeInMinutes = invoicedTimePermission ? invoiceTimeInMinutes : 0,
                        WorkTimeInMinutes = workedTimePermission ? workTimeInMinutes : 0,
                        Note = note,
                        CommentExternal = externalComment,
                        Date = date,
                    };

                    SetCreatedProperties(projectInvoiceweek);

                    projectInvoiceweek.ProjectInvoiceDay.Add(projectInvoiceDay);

                    entities.ProjectInvoiceWeek.AddObject(projectInvoiceweek);
                    entities.ProjectInvoiceDay.AddObject(projectInvoiceDay);
                    #endregion

                }
                else
                {
                    #region Update

                    SetModifiedProperties(projectInvoiceweek);
                    projectInvoiceDay = GetProjectInvoiceDay(entities, projectInvoiceweek.ProjectInvoiceWeekId, dayType);
                    if (projectInvoiceDay == null)
                    {
                        projectInvoiceDay = new ProjectInvoiceDay
                        {
                            DayType = (int)dayType,
                            Date = date,
                            WorkTimeInMinutes = workedTimePermission ? workTimeInMinutes : 0,
                            InvoiceTimeInMinutes = invoicedTimePermission ? invoiceTimeInMinutes : 0,
                            Note = note,
                            CommentExternal = externalComment,

                            //Set FK
                            ProjectInvoiceWeekId = projectInvoiceweek.ProjectInvoiceWeekId,
                        };

                        entities.ProjectInvoiceDay.AddObject(projectInvoiceDay);
                    }
                    else
                    {
                        oldInvoiceTimeInMinutes = projectInvoiceDay.InvoiceTimeInMinutes;

                        if (!invoicedTimePermission)
                            invoiceTimeInMinutes = oldInvoiceTimeInMinutes.HasValue ? oldInvoiceTimeInMinutes.Value : 0;

                        if (workedTimePermission)
                            projectInvoiceDay.WorkTimeInMinutes = workTimeInMinutes;

                        projectInvoiceDay.InvoiceTimeInMinutes = invoiceTimeInMinutes;
                        projectInvoiceDay.Note = note;
                        projectInvoiceDay.CommentExternal = externalComment;
                    }
                    #endregion
                }

                #endregion

                int quantityDifference = oldInvoiceTimeInMinutes.HasValue ? invoiceTimeInMinutes - oldInvoiceTimeInMinutes.Value : invoiceTimeInMinutes;

                #region Update OrderRows

                if (invoiceProduct != null && invoicedTimePermission)
                {
                    decimal quantity = 0;
                    int orderRowId = 0;

                    int defaultInvoiceProductUnitId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultInvoiceProductUnit, 0, actorCompanyId, 0);
                    ProductUnit defaultInvoiceProductUnit = ProductManager.GetProductUnit(entities, defaultInvoiceProductUnitId);

                    List<CustomerInvoiceRow> invoiceProductRows = InvoiceManager.GetUnAttestedCustomerInvoiceRows(entities, actorCompanyId, orderId, invoiceProduct.ProductId, TermGroup_AttestEntity.Order, true, true);
                    invoiceProductRows = invoiceProductRows.Where(i => i.IsTimeProjectRow).ToList();

                    decimal purchasePrice = 0;
                    if (invoiceProductRows != null && invoiceProductRows.Count > 0)
                    {
                        #region Update

                        row = invoiceProductRows.FirstOrDefault();

                        #region Purchase price

                        //Check if calculated cost should be set as purchase price
                        var employeeCalculateCost = invoiceProduct.UseCalculatedCost.GetValueOrDefault() ? EmployeeManager.GetEmployeeCalculatedCost(entities, employee, date, order.ProjectId) : 0;

                        if (!row.ProductUnitReference.IsLoaded)
                            row.ProductUnitReference.Load();

                        decimal quantityDiff = quantityDifference;
                        if (!row.ProductUnit.Name.Trim().ToLower().StartsWith("min"))
                            quantityDiff /= 60;

                        decimal newTotal = employeeCalculateCost > 0 ? (quantityDiff * employeeCalculateCost) : (quantityDiff * invoiceProduct.PurchasePrice);
                        decimal oldTotal = row.Quantity.HasValue ? row.Quantity.Value * row.PurchasePrice : 0;
                        decimal totalQuantity = row.Quantity.HasValue ? row.Quantity.Value + quantityDiff : quantityDiff;
                        if (totalQuantity != 0) // can't divide by zero                        
                            purchasePrice = (oldTotal + newTotal) / totalQuantity;
                        else
                            purchasePrice = (oldTotal + newTotal);

                        #endregion

                        //Remove the old value on the reported day from the invoicesum
                        if (oldInvoiceTimeInMinutes.HasValue)
                        {
                            if (row.ProductUnit != null && row.ProductUnit.Code.Trim().ToLower().StartsWith("min"))
                                row.Quantity -= oldInvoiceTimeInMinutes.Value;
                            else
                                row.Quantity -= Convert.ToDecimal((oldInvoiceTimeInMinutes.Value) / 60d);
                        }

                        //Now add the new value
                        if (row.ProductUnit != null && row.ProductUnit.Code.Trim().ToLower().StartsWith("min"))
                            row.Quantity += projectInvoiceDay.InvoiceTimeInMinutes;
                        else
                            row.Quantity += Convert.ToDecimal(projectInvoiceDay.InvoiceTimeInMinutes / 60d);

                        quantity = row.Quantity.HasValue ? row.Quantity.Value : 0;
                        orderRowId = row.CustomerInvoiceRowId;

                        #endregion
                    }
                    else
                    {
                        #region Purchase price

                        //Check if calculated cost should be set as purchase price
                        if (invoiceProduct.UseCalculatedCost.GetValueOrDefault())
                        {
                            var employeeCalculateCost = invoiceProduct.UseCalculatedCost.GetValueOrDefault() ? EmployeeManager.GetEmployeeCalculatedCost(entities, employee, date, order.ProjectId) : 0;
                            purchasePrice = employeeCalculateCost > 0 ? employeeCalculateCost : invoiceProduct.PurchasePrice;
                        }

                        #endregion

                        #region Create new Invoicerow

                        if (defaultInvoiceProductUnit != null && defaultInvoiceProductUnit.Code.Trim().ToLower().StartsWith("min"))
                            quantity = quantityDifference;
                        else
                            quantity = Convert.ToDecimal(quantityDifference / 60d);

                        orderRowId = 0;
                        #endregion
                    }

                    result = InvoiceManager.SaveCustomerInvoiceRow(transaction, entities, actorCompanyId, order, orderRowId, invoiceProduct.ProductId, quantity, productPrice, string.Empty, SoeInvoiceRowType.ProductRow, null, null, true, true, false, true, true, productPurchasePrice: purchasePrice);

                    if (!result.Success)
                        return result;

                    row = result.Value as CustomerInvoiceRow;
                }

                if (row != null && row.Quantity == 0)
                {
                    rowId = row.CustomerInvoiceRowId; //Row to delete
                    deleteRow = true; //Delete flag
                }

                #endregion

                #region Transactions

                var days = new List<ProjectInvoiceDay>();
                days.Add(projectInvoiceDay);
                var invoiceRowMapping = new List<ProjectInvoiceDayInvoiceRowMappingDTO>();
                List<Tuple<int, int>> invoiceRowIds = new List<Tuple<int, int>>();

                if (row != null)
                {
                    invoiceRowMapping.Add(new ProjectInvoiceDayInvoiceRowMappingDTO() { InvoiceRowTempId = row.CustomerInvoiceRowId, ProjectInvoiceDayTempId = projectInvoiceDay.ProjectInvoiceDayTempId, QuantityDifference = quantityDifference });
                    invoiceRowIds.Add(new Tuple<int, int>(row.CustomerInvoiceRowId, row.CustomerInvoiceRowId));
                }

                result = CreateTransactionsForTimeProjectRegistration(entities, days, invoiceRowMapping, invoiceRowIds, actorCompanyId, employee.EmployeeId, timeCodeId, projectInvoiceweek.ProjectId, order.ActorId != null ? (int)order.ActorId : 0, date, false);
                if (!result.Success)
                    return result;

                #endregion

                #endregion

                SetModifiedProperties(order);
            }
            catch (Exception exp)
            {
                base.LogError(exp, log);
                result = new ActionResult(exp);
            }
            finally
            {
                if (result.Success && deleteRow)
                {
                    //Set success properties
                    result.IntegerValue = rowId;
                    result.BooleanValue = deleteRow;
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

            }

            return result;
        }

        public TimeProject CreateTimeProject(CompEntities entities, CustomerInvoice invoice, String projectNr, Customer customer, int actorCompanyId)
        {
            TimeProject project = new TimeProject()
            {
                Number = projectNr,
                Name = customer != null && !String.IsNullOrEmpty(customer.Name) ? customer.Name.Replace("\n", "").Replace("\r", "") : DateTime.Now.ToString("yyyyMMddhhmm"),
                Type = (int)TermGroup_ProjectType.TimeProject,
                InvoiceProductAccountingPrio = "0,0,0,0,0",
                PayrollProductAccountingPrio = "0,0,0,0,0",
                Description = String.Empty,
                Status = (int)TermGroup_ProjectStatus.Active,
                UseAccounting = true,
                AllocationType = (int)TermGroup_ProjectAllocationType.External,

                //Set FK
                ActorCompanyId = actorCompanyId,
                CustomerId = invoice.ActorId,
            };
            SetCreatedProperties(project);

            bool includeTimeReport = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectIncludeTimeProjectReport, 0, actorCompanyId, 0);
            if (includeTimeReport)
                invoice.PrintTimeReport = true;

            invoice.Project = project;

            return project;
        }

        public ActionResult CreateTransactionsForTimeProjectRegistration(CompEntities entities, ProjectTimeBlock projectTimeBlock, TimeCode timeCode, int projectId, int customerId, DateTime date, CustomerInvoiceRow customerInvoiceRow, bool updateInvoicedTime)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                #region Prereq

                int accountId = 0;

                accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupIncome, 0, base.ActorCompanyId, 0);
                if (accountId == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8332, "Standardkonto för tidavtal saknas"));

                int attestStateIdPayroll = AttestManager.GetInitialAttestStateId(entities, base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
                if (attestStateIdPayroll == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(4880, "Startnivå för attestnivå med typ löneart saknas"));

                int attestStateIdInvoice = AttestManager.GetInitialAttestStateId(entities, base.ActorCompanyId, TermGroup_AttestEntity.InvoiceTime);
                if (attestStateIdInvoice == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(4881, "Startnivå för attestnivå med typ artikel saknas"));

                bool createTransactionBasedOnTimeRules = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectCreateTransactionsBaseOnTimeRules, 0, base.ActorCompanyId, 0);

                #endregion

                #region Products


                #region Get Payrollproduct

                PayrollProduct payrollProduct = null;

                if (!timeCode.TimeCodePayrollProduct.IsLoaded)
                    timeCode.TimeCodePayrollProduct.Load();

                TimeCodePayrollProduct tcpProduct = null;
                if (timeCode.TimeCodePayrollProduct != null)
                    tcpProduct = timeCode.TimeCodePayrollProduct.FirstOrDefault();

                if (tcpProduct != null)
                {
                    if (!tcpProduct.PayrollProductReference.IsLoaded)
                        tcpProduct.PayrollProductReference.Load();

                    if (tcpProduct.PayrollProduct != null)
                        payrollProduct = tcpProduct.PayrollProduct;
                }

                AccountingPrioDTO dto = null;
                if (payrollProduct != null)
                {
                    dto = AccountManager.GetPayrollProductAccount(entities, ProductAccountType.Purchase, base.ActorCompanyId, projectTimeBlock.EmployeeId, payrollProduct.ProductId, projectId, customerId, true, date);
                    if (dto != null)
                        accountId = dto.AccountId.HasValue ? dto.AccountId.Value : accountId;
                }

                #endregion

                #endregion

                #region Transactions

                #region Prereq

                if (!projectTimeBlock.TimeBlockDateReference.IsLoaded)
                    projectTimeBlock.TimeBlockDateReference.Load();

                if (!projectTimeBlock.IsAdded())
                {
                    if (!projectTimeBlock.TimeCodeTransaction.IsLoaded)
                        projectTimeBlock.TimeCodeTransaction.Load();

                    if (projectTimeBlock.TimeCodeTransaction != null)
                    {
                        foreach (TimeCodeTransaction timeCodeTransaction in projectTimeBlock.TimeCodeTransaction.Where(x => x.State == (int)SoeEntityState.Active))
                        {
                            if (!timeCodeTransaction.TimeInvoiceTransaction.IsLoaded)
                                timeCodeTransaction.TimeInvoiceTransaction.Load();

                            if (!timeCodeTransaction.TimePayrollTransaction.IsLoaded)
                                timeCodeTransaction.TimePayrollTransaction.Load();
                        }
                    }
                }

                #endregion

                #region TimeCodeTransaction

                TimeCodeTransaction timeCodeTransactionForInvoice = projectTimeBlock.TimeCodeTransaction.FirstOrDefault(tct => tct.State == (int)SoeEntityState.Active && tct.TimeInvoiceTransaction != null && tct.TimeInvoiceTransaction.Count > 0);
                if (timeCodeTransactionForInvoice != null)
                {
                    #region Update TimeCodeTransaction

                    timeCodeTransactionForInvoice.TimeCodeId = timeCode.TimeCodeId;
                    timeCodeTransactionForInvoice.Quantity = CalendarUtility.TimeSpanToMinutes(projectTimeBlock.StopTime, projectTimeBlock.StartTime);
                    timeCodeTransactionForInvoice.InvoiceQuantity = projectTimeBlock.InvoiceQuantity;
                    timeCodeTransactionForInvoice.Start = projectTimeBlock.StartTime;
                    timeCodeTransactionForInvoice.Stop = projectTimeBlock.StopTime;
                    timeCodeTransactionForInvoice.ExternalComment = projectTimeBlock.ExternalNote;
                    timeCodeTransactionForInvoice.Comment = projectTimeBlock.InternalNote;

                    #endregion
                }
                else
                {
                    #region Add TimeCodeTransaction

                    if (projectTimeBlock.TimeCodeTransaction != null && (CalendarUtility.TimeSpanToMinutes(projectTimeBlock.StopTime, projectTimeBlock.StartTime) != 0 || projectTimeBlock.InvoiceQuantity != 0))
                    {
                        timeCodeTransactionForInvoice = TimeTransactionManager.CreateTimeCodeTransaction(entities, base.ActorCompanyId, projectTimeBlock, timeCode.TimeCodeId, projectId);
                        if (timeCodeTransactionForInvoice != null)
                        {
                            projectTimeBlock.TimeCodeTransaction.Add(timeCodeTransactionForInvoice);
                        }
                    }

                    #endregion
                }

                #endregion

                if (projectTimeBlock.TimeCodeTransaction != null && projectTimeBlock.TimeCodeTransaction.Any(i => i.State == (int)SoeEntityState.Active))
                {
                    TimeCodeTransaction timeCodeTransaction = projectTimeBlock.TimeCodeTransaction.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);
                    if (timeCodeTransaction != null)
                    {
                        #region TimePayrollTransaction

                        if (!createTransactionBasedOnTimeRules)
                        {
                            if (timeCodeTransaction.TimePayrollTransaction != null && timeCodeTransaction.TimePayrollTransaction.Any(i => i.State == (int)SoeEntityState.Active))
                            {
                                #region Update TimePayrollTransaction (Currently we only have 1 TimePayrollTransaction)

                                TimePayrollTransaction timePayrollTransaction = timeCodeTransaction.TimePayrollTransaction.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);
                                if (timePayrollTransaction != null)
                                {
                                    timePayrollTransaction.Quantity = CalendarUtility.TimeSpanToMinutes(projectTimeBlock.StopTime, projectTimeBlock.StartTime);

                                    // AccountStd
                                    if (dto != null && dto.AccountId.HasValue)
                                        timePayrollTransaction.AccountStdId = dto.AccountId.Value;

                                    // AccountInternal
                                    if (dto != null && dto.AccountInternals != null && dto.AccountInternals.Count > 0)
                                    {
                                        if (!timePayrollTransaction.AccountInternal.IsLoaded)
                                            timePayrollTransaction.AccountInternal.Load();

                                        // Clear existing accounting
                                        timePayrollTransaction.AccountInternal.Clear();

                                        if (timePayrollTransaction.AccountInternal == null)
                                            timePayrollTransaction.AccountInternal = new EntityCollection<AccountInternal>();

                                        foreach (var accountInternal in dto.AccountInternals)
                                        {
                                            AccountInternal accInt = AccountManager.GetAccountInternal(entities, accountInternal.AccountId, base.ActorCompanyId);
                                            if (accInt != null)
                                                timePayrollTransaction.AccountInternal.Add(accInt);
                                        }
                                    }
                                }

                                #endregion
                            }
                            else
                            {
                                #region Add TimePayrollTransaction

                                int worktimeQuantity = CalendarUtility.TimeSpanToMinutes(projectTimeBlock.StopTime, projectTimeBlock.StartTime);

                                if (worktimeQuantity != 0 && payrollProduct != null)
                                {
                                    TimePayrollTransaction timePayrollTransaction = TimeTransactionManager.CreateTimePayrollTransaction(entities, base.ActorCompanyId, payrollProduct, projectTimeBlock, worktimeQuantity, accountId, attestStateIdPayroll);
                                    if (timePayrollTransaction != null)
                                    {
                                        // AccountStd
                                        if (dto != null && dto.AccountId.HasValue)
                                            timePayrollTransaction.AccountStdId = dto.AccountId.Value;

                                        // AccountInternal
                                        if (dto != null && dto.AccountInternals != null && dto.AccountInternals.Count > 0)
                                        {
                                            foreach (var accountInternal in dto.AccountInternals)
                                            {
                                                AccountInternal accInt = AccountManager.GetAccountInternal(entities, accountInternal.AccountId, base.ActorCompanyId);
                                                if (accInt != null)
                                                    timePayrollTransaction.AccountInternal.Add(accInt);
                                            }
                                        }

                                        //Add to TimeCodeTransaction
                                        timeCodeTransaction.TimePayrollTransaction.Add(timePayrollTransaction);
                                    }
                                }

                                #endregion
                            }
                        }

                        #endregion

                        #region TimeInvoiceTransaction
                        if (timeCodeTransactionForInvoice != null && updateInvoicedTime)
                        {
                            CustomerInvoiceAccountRow row = (customerInvoiceRow != null) ? InvoiceManager.GetCustomerInvoiceAccountRow(entities, customerInvoiceRow.CustomerInvoiceRowId, false, true) : null;

                            TimeInvoiceTransaction timeInvoiceTransaction = null;
                            if (timeCodeTransactionForInvoice.TimeInvoiceTransaction != null)
                            {
                                if (customerInvoiceRow != null)
                                {
                                    timeInvoiceTransaction = timeCodeTransactionForInvoice.TimeInvoiceTransaction.FirstOrDefault(x => x.CustomerInvoiceRowId == customerInvoiceRow.CustomerInvoiceRowId && x.State == (int)SoeEntityState.Active);
                                }

                                //no invoicerow found connection because we have only work time or we are going from only work time to also include invoice time
                                if (timeInvoiceTransaction == null)
                                {
                                    timeInvoiceTransaction = timeCodeTransactionForInvoice.TimeInvoiceTransaction.FirstOrDefault(x => !x.CustomerInvoiceRowId.HasValue && x.State == (int)SoeEntityState.Active);
                                }
                            }

                            if (timeInvoiceTransaction != null)
                            {
                                #region Update TimeInvoiceTransaction

                                timeInvoiceTransaction.Quantity = projectTimeBlock.InvoiceQuantity;
                                timeInvoiceTransaction.InvoiceQuantity = projectTimeBlock.InvoiceQuantity;

                                //Going from only worktime to also include invoice time... 
                                if (!timeInvoiceTransaction.CustomerInvoiceRowId.HasValue && customerInvoiceRow != null)
                                {
                                    timeInvoiceTransaction.CustomerInvoiceRowId = customerInvoiceRow.CustomerInvoiceRowId;
                                    SetModifiedProperties(timeInvoiceTransaction);
                                }

                                if (timeInvoiceTransaction.InvoiceQuantity == 0 && customerInvoiceRow != null)
                                {
                                    timeInvoiceTransaction.State = (int)SoeEntityState.Deleted;
                                }

                                if (row != null)
                                {
                                    if (row.AccountInternal != null)
                                    {
                                        if (!timeInvoiceTransaction.AccountInternal.IsLoaded)
                                            timeInvoiceTransaction.AccountInternal.Load();

                                        // Clear existing accounting
                                        timeInvoiceTransaction.AccountInternal.Clear();

                                        if (timeInvoiceTransaction.AccountInternal == null)
                                            timeInvoiceTransaction.AccountInternal = new EntityCollection<AccountInternal>();

                                        timeInvoiceTransaction.AccountInternal.AddRange(row.AccountInternal);
                                    }
                                    timeInvoiceTransaction.AccountStd = row.AccountStd;
                                    timeInvoiceTransaction.AccountStdId = row.AccountId;
                                }


                                #endregion
                            }
                            else
                            {
                                #region Get InvoiceProduct

                                InvoiceProduct invoiceProduct = null;

                                if (!timeCode.TimeCodeInvoiceProduct.IsLoaded)
                                    timeCode.TimeCodeInvoiceProduct.Load();

                                TimeCodeInvoiceProduct tciProduct = timeCode?.TimeCodeInvoiceProduct.FirstOrDefault();

                                if (tciProduct != null)
                                {
                                    if (!tciProduct.InvoiceProductReference.IsLoaded)
                                        tciProduct.InvoiceProductReference.Load();

                                    if (tciProduct.InvoiceProduct != null)
                                        invoiceProduct = tciProduct.InvoiceProduct;
                                }

                                #endregion

                                #region Add TimeInvoiceTransaction

                                if (invoiceProduct != null && !(customerInvoiceRow == null && projectTimeBlock.InvoiceQuantity == 0))
                                {
                                    timeInvoiceTransaction = TimeTransactionManager.CreateTimeInvoiceTransaction(entities, invoiceProduct, projectTimeBlock, row?.AccountId ?? accountId, attestStateIdInvoice);

                                    timeInvoiceTransaction.CustomerInvoiceRow = customerInvoiceRow;

                                    if (row != null)
                                    {
                                        if (row.AccountInternal != null)
                                            timeInvoiceTransaction.AccountInternal.AddRange(row.AccountInternal);
                                        timeInvoiceTransaction.AccountStd = row.AccountStd;
                                    }

                                    //Add to TimeCodeTransaction
                                    timeCodeTransactionForInvoice.TimeInvoiceTransaction.Add(timeInvoiceTransaction);
                                }

                                #endregion
                            }
                        }

                        #endregion
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                result = new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8341, "Transaktioner kunde inte skapas"));
            }

            return result;
        }

        public ActionResult CreateTransactionsForTimeProjectRegistration(CompEntities entities, List<ProjectInvoiceDay> days, List<ProjectInvoiceDayInvoiceRowMappingDTO> invoiceRowMapping, List<Tuple<int, int>> invoiceRowIds, int actorCompanyId, int employeeId, int timeCodeId, int projectId, int customerId, DateTime? date = null, bool ignoreExternal = true)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                #region Prereq

                int accountId = 0;

                accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupIncome, 0, actorCompanyId, 0);
                if (accountId == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8332, "Standardkonto för tidavtal saknas"));

                int attestStateIdPayroll = AttestManager.GetInitialAttestStateId(entities, actorCompanyId, TermGroup_AttestEntity.PayrollTime);
                if (attestStateIdPayroll == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(5752, "Startnivå för attestnivå med typ löneart saknas"));

                int attestStateIdInvoice = AttestManager.GetInitialAttestStateId(entities, actorCompanyId, TermGroup_AttestEntity.InvoiceTime);
                if (attestStateIdInvoice == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(5753, "Startnivå för attestnivå med typ artikel saknas"));

                #endregion

                #region Products

                TimeCode timeCode = TimeCodeManager.GetTimeCodeWithProducts(entities, timeCodeId, actorCompanyId);
                if (timeCode == null)
                    return new ActionResult("Tidkod saknas:" + timeCodeId.ToString());

                InvoiceProduct invoiceProduct = timeCode.TimeCodeInvoiceProduct?.FirstOrDefault()?.InvoiceProduct;
                PayrollProduct payrollProduct = timeCode.TimeCodePayrollProduct?.FirstOrDefault()?.PayrollProduct;

                AccountingPrioDTO dto = null;
                if (payrollProduct != null)
                {
                    dto = AccountManager.GetPayrollProductAccount(entities, ProductAccountType.Purchase, actorCompanyId, employeeId, payrollProduct.ProductId, projectId, customerId, true, date);
                    if (dto != null)
                        accountId = dto.AccountId.HasValue ? dto.AccountId.Value : accountId;
                }

                #endregion

                #region Transactions

                foreach (ProjectInvoiceDay day in days)
                {
                    #region Prereq

                    List<ProjectInvoiceDayInvoiceRowMappingDTO> invoiceRowMappingChanges = invoiceRowMapping.Where(x => x.ProjectInvoiceDayTempId == day.ProjectInvoiceDayTempId).ToList();
                    var invoiceRowMappingChangesGroupedByInvoiceRow = invoiceRowMappingChanges.GroupBy(x => x.InvoiceRowTempId).ToList();

                    TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, actorCompanyId, employeeId, day.Date, true);
                    if (timeBlockDate == null)
                        return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8335, "Timeblockdate kunde inte skapas"));

                    if (!day.IsAdded())
                    {
                        if (!day.TimeCodeTransaction.IsLoaded)
                            day.TimeCodeTransaction.Load();

                        if (day.TimeCodeTransaction != null)
                        {
                            foreach (TimeCodeTransaction timeCodeTransaction in day.TimeCodeTransaction)
                            {
                                if (!timeCodeTransaction.TimeInvoiceTransaction.IsLoaded)
                                    timeCodeTransaction.TimeInvoiceTransaction.Load();

                                if (!timeCodeTransaction.TimePayrollTransaction.IsLoaded)
                                    timeCodeTransaction.TimePayrollTransaction.Load();
                            }
                        }
                    }

                    #endregion

                    #region TimeCodeTransaction

                    if (day.TimeCodeTransaction != null && day.TimeCodeTransaction.Where(x => x.State == (int)SoeEntityState.Active).ToList().Count > 0)
                    {
                        #region Update TimeCodeTransaction

                        TimeCodeTransaction timeCodeTransaction = day.TimeCodeTransaction.FirstOrDefault(x => x.State == (int)SoeEntityState.Active); // it can only be one
                        if (timeCodeTransaction != null)
                        {
                            timeCodeTransaction.TimeCodeId = timeCodeId;
                            timeCodeTransaction.Quantity = day.WorkTimeInMinutes;
                            timeCodeTransaction.InvoiceQuantity = day.InvoiceTimeInMinutes;
                            timeCodeTransaction.Start = CalendarUtility.DATETIME_DEFAULT;
                            timeCodeTransaction.Stop = CalendarUtility.DATETIME_DEFAULT;
                            timeCodeTransaction.ExternalComment = day.Note;

                            if (!ignoreExternal)
                                timeCodeTransaction.Comment = day.CommentExternal != null ? day.CommentExternal : String.Empty;
                            //Removed 2014-10-10 due to task 12664 
                            /*timeCodeTransaction.Comment = day.Note;
                            timeCodeTransaction.ExternalComment = day.CommentExternal != null && !ignoreExternal ? day.CommentExternal : String.Empty;*/
                        }

                        #endregion
                    }
                    else
                    {
                        #region Add TimeCodeTransaction

                        if (day.TimeCodeTransaction != null && (day.WorkTimeInMinutes != 0 || day.InvoiceTimeInMinutes != 0))
                        {
                            TimeCodeTransaction timeCodeTransaction = TimeTransactionManager.CreateTimeCodeTransaction(entities, actorCompanyId, day, timeCodeId, projectId);
                            if (timeCodeTransaction != null)
                            {
                                day.TimeCodeTransaction.Add(timeCodeTransaction);
                            }
                        }

                        #endregion
                    }

                    #endregion

                    if (day.TimeCodeTransaction != null && day.TimeCodeTransaction.Any(i => i.State == (int)SoeEntityState.Active))
                    {
                        TimeCodeTransaction timeCodeTransaction = day.TimeCodeTransaction.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);
                        if (timeCodeTransaction != null)
                        {
                            #region TimePayrollTransaction

                            if (timeCodeTransaction.TimePayrollTransaction != null && timeCodeTransaction.TimePayrollTransaction.Any(i => i.State == (int)SoeEntityState.Active))
                            {
                                #region Update TimePayrollTransaction (Currently we only have 1 TimePayrollTransaction)

                                TimePayrollTransaction timePayrollTransaction = timeCodeTransaction.TimePayrollTransaction.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);
                                if (timePayrollTransaction != null)
                                {
                                    timePayrollTransaction.Quantity = day.WorkTimeInMinutes;

                                    // AccountStd
                                    if (dto != null && dto.AccountId.HasValue)
                                        timePayrollTransaction.AccountStdId = dto.AccountId.Value;

                                    // AccountInternal
                                    if (dto != null && dto.AccountInternals != null && dto.AccountInternals.Count > 0)
                                    {
                                        if (!timePayrollTransaction.AccountInternal.IsLoaded)
                                            timePayrollTransaction.AccountInternal.Load();

                                        // Clear existing accounting
                                        timePayrollTransaction.AccountInternal.Clear();

                                        if (timePayrollTransaction.AccountInternal == null)
                                            timePayrollTransaction.AccountInternal = new EntityCollection<AccountInternal>();

                                        foreach (var accountInternal in dto.AccountInternals)
                                        {
                                            AccountInternal accInt = AccountManager.GetAccountInternal(entities, accountInternal.AccountId, actorCompanyId);
                                            if (accInt != null)
                                                timePayrollTransaction.AccountInternal.Add(accInt);
                                        }
                                    }
                                }

                                #endregion
                            }
                            else
                            {
                                #region Add TimePayrollTransaction

                                if (day.WorkTimeInMinutes != 0 && payrollProduct != null)
                                {
                                    TimePayrollTransaction timePayrollTransaction = TimeTransactionManager.CreateTimePayrollTransaction(entities, actorCompanyId, payrollProduct, day, timeBlockDate, employeeId, accountId, attestStateIdPayroll);
                                    if (timePayrollTransaction != null)
                                    {
                                        // AccountStd
                                        if (dto != null && dto.AccountId.HasValue)
                                            timePayrollTransaction.AccountStdId = dto.AccountId.Value;

                                        // AccountInternal
                                        if (dto != null && dto.AccountInternals != null && dto.AccountInternals.Count > 0)
                                        {
                                            foreach (var accountInternal in dto.AccountInternals)
                                            {
                                                AccountInternal accInt = AccountManager.GetAccountInternal(entities, accountInternal.AccountId, actorCompanyId);
                                                if (accInt != null)
                                                    timePayrollTransaction.AccountInternal.Add(accInt);
                                            }
                                        }

                                        //Add to TimeCodeTransaction
                                        timeCodeTransaction.TimePayrollTransaction.Add(timePayrollTransaction);
                                    }
                                }

                                #endregion
                            }

                            #endregion

                            #region TimeInvoiceTransaction

                            foreach (var itemGroup in invoiceRowMappingChangesGroupedByInvoiceRow)
                            {
                                int customerInvoiceRowId = 0;
                                int invoiceQuantityDifference = 0;
                                bool changeRow = false;
                                CustomerInvoiceAccountRow row = null;

                                //itemGroup.Key is customerinvoicerow.temprowid
                                if (!invoiceRowIds.Any(i => i.Item1 == itemGroup.Key))
                                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8372, "Koppla tidrader med artikelrader kunde inte slutföras"));

                                ProjectInvoiceDayInvoiceRowMappingDTO mapping = itemGroup.FirstOrDefault();

                                if (mapping.PreviousRowTempId != 0)
                                {
                                    changeRow = true;
                                    customerInvoiceRowId = mapping.PreviousRowTempId;
                                    invoiceQuantityDifference = invoiceRowMappingChanges.Where(i => i.InvoiceRowTempId == itemGroup.Key).ToList().Sum(k => k.QuantityDifference);
                                    row = InvoiceManager.GetCustomerInvoiceAccountRow(entities, invoiceRowIds.Where(i => i.Item1 == itemGroup.Key).FirstOrDefault().Item2, false, true);
                                }
                                else
                                {
                                    customerInvoiceRowId = invoiceRowIds.FirstOrDefault(i => i.Item1 == itemGroup.Key).Item2;
                                    invoiceQuantityDifference = invoiceRowMappingChanges.Where(i => i.InvoiceRowTempId == itemGroup.Key).ToList().Sum(k => k.QuantityDifference);
                                    row = InvoiceManager.GetCustomerInvoiceAccountRow(entities, customerInvoiceRowId, false, true);
                                }

                                if (timeCodeTransaction.TimeInvoiceTransaction != null && timeCodeTransaction.TimeInvoiceTransaction.Where(x => x.CustomerInvoiceRowId == customerInvoiceRowId && x.State == (int)SoeEntityState.Active).ToList().Count > 0)
                                {
                                    #region Update TimeInvoiceTransaction

                                    TimeInvoiceTransaction timeInvoiceTransaction = timeCodeTransaction.TimeInvoiceTransaction.FirstOrDefault(x => x.CustomerInvoiceRowId == customerInvoiceRowId && x.State == (int)SoeEntityState.Active);
                                    if (timeInvoiceTransaction != null)
                                    {
                                        if (row != null)
                                        {
                                            if (changeRow)
                                            {
                                                if (!row.CustomerInvoiceRowReference.IsLoaded)
                                                    row.CustomerInvoiceRowReference.Load();

                                                if (row.CustomerInvoiceRow != null)
                                                    timeInvoiceTransaction.CustomerInvoiceRow = row.CustomerInvoiceRow;
                                            }

                                            timeInvoiceTransaction.Quantity += invoiceQuantityDifference;
                                            timeInvoiceTransaction.InvoiceQuantity += invoiceQuantityDifference;
                                            if (row.AccountInternal != null)
                                            {
                                                if (!timeInvoiceTransaction.AccountInternal.IsLoaded)
                                                    timeInvoiceTransaction.AccountInternal.Load();

                                                // Clear existing accounting
                                                timeInvoiceTransaction.AccountInternal.Clear();

                                                if (timeInvoiceTransaction.AccountInternal == null)
                                                    timeInvoiceTransaction.AccountInternal = new EntityCollection<AccountInternal>();

                                                timeInvoiceTransaction.AccountInternal.AddRange(row.AccountInternal);
                                            }
                                            timeInvoiceTransaction.AccountStd = row.AccountStd;
                                            timeInvoiceTransaction.AccountStdId = row.AccountId;
                                        }
                                        else
                                        {
                                            timeInvoiceTransaction.Quantity += invoiceQuantityDifference;
                                            timeInvoiceTransaction.InvoiceQuantity += invoiceQuantityDifference;
                                        }
                                    }

                                    #endregion
                                }
                                else
                                {
                                    #region Add TimeInvoiceTransaction

                                    if (invoiceProduct != null)
                                    {
                                        TimeInvoiceTransaction timeInvoiceTransaction = TimeTransactionManager.CreateTimeInvoiceTransaction(entities, actorCompanyId, invoiceProduct, timeBlockDate, invoiceQuantityDifference, employeeId, row != null ? row.AccountId : accountId, attestStateIdInvoice);
                                        if (timeInvoiceTransaction != null)
                                        {
                                            timeInvoiceTransaction.CustomerInvoiceRowId = customerInvoiceRowId;

                                            if (row != null)
                                            {
                                                if (row.AccountInternal != null)
                                                    timeInvoiceTransaction.AccountInternal.AddRange(row.AccountInternal);
                                                timeInvoiceTransaction.AccountStd = row.AccountStd;
                                            }

                                            //Add to TimeCodeTransaction
                                            timeCodeTransaction.TimeInvoiceTransaction.Add(timeInvoiceTransaction);
                                        }
                                    }

                                    #endregion
                                }
                            }

                            #endregion
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                result = new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8341, "Transaktioner kunde inte skapas"));
            }

            return result;
        }

        public List<ProjectInvoiceDay> GetProjectInvoiceDaysWithWeekByProjectId(int projectId, int orderId, int employeeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetProjectInvoiceDaysWithWeekByProjectId(entities, projectId, orderId, employeeId);
        }

        public List<ProjectInvoiceDay> GetProjectInvoiceDaysWithWeekByProjectId(CompEntities entities, int projectId, int orderId, int employeeId)
        {
            return (from p in entities.ProjectInvoiceDay
                    .Include("ProjectInvoiceWeek")
                    where p.ProjectInvoiceWeek.ProjectId == projectId &&
                    p.ProjectInvoiceWeek.RecordId == orderId &&
                    p.ProjectInvoiceWeek.EmployeeId == employeeId &&
                    p.ProjectInvoiceWeek.State == (int)SoeEntityState.Active
                    orderby p.Date descending
                    select p).ToList();
        }

        public List<ProjectInvoiceDay> GetProjectInvoiceDays(CompEntities entities, int projectInvoiceWeekId, DateTime? fromDate = null, DateTime? toDate = null, bool includeInvoiceTransactions = false)
        {
            IQueryable<ProjectInvoiceDay> projectInvoiceDays = (from p in entities.ProjectInvoiceDay
                                                                where p.ProjectInvoiceWeekId == projectInvoiceWeekId
                                                                orderby p.Date
                                                                select p);

            if (includeInvoiceTransactions)
            {
                projectInvoiceDays = projectInvoiceDays.Include("TimeCodeTransaction.TimeInvoiceTransaction");
            }

            if (fromDate.HasValue)
            {
                projectInvoiceDays = projectInvoiceDays.Where(x => x.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                projectInvoiceDays = projectInvoiceDays.Where(x => x.Date <= toDate.Value);
            }

            return projectInvoiceDays.ToList();
        }

        public List<ProjectInvoiceWeek> GetProjectInvoiceWeeks(CompEntities entities, int projectId, int recordId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            IQueryable<ProjectInvoiceWeek> projectInvoiceWeeks = (from p in entities.ProjectInvoiceWeek
                                                                    .Include("Employee")
                                                                    .Include("Employee.ContactPerson")
                                                                  where p.ProjectId == projectId &&
                                                                  p.RecordId == recordId &&
                                                                  p.State == (int)SoeEntityState.Active
                                                                  select p);

            if (fromDate.HasValue)
            {
                fromDate = CalendarUtility.GetFirstDateOfWeek(fromDate.Value);
                projectInvoiceWeeks = projectInvoiceWeeks.Where(x => x.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                toDate = CalendarUtility.GetFirstDateOfWeek(toDate.Value);
                projectInvoiceWeeks = projectInvoiceWeeks.Where(x => x.Date <= toDate.Value);
            }

            return projectInvoiceWeeks.ToList();
        }

        public List<ProjectInvoiceWeek> GetProjectInvoiceWeeks(CompEntities entities, int projectId)
        {

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ProjectInvoiceWeek.NoTracking();
            return (from p in entitiesReadOnly.ProjectInvoiceWeek
                    .Include("Employee")
                    .Include("Employee.ContactPerson")
                    where p.ProjectId == projectId &&
                    p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }

        public List<ProjectGridDTO> GetProjectList(int actorCompanyId, int projectStatus, bool onlyMine)
        {
            var projectStatuses = new List<TermGroup_ProjectStatus> { (TermGroup_ProjectStatus)projectStatus };
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            return this.GetProjectList(entitiesReadOnly, actorCompanyId, base.UserId, projectStatuses, onlyMine);
        }

        public List<ProjectGridDTO> GetProjectList(int actorCompanyId, int[] projectStatuses, bool onlyMine)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            return this.GetProjectList(entitiesReadOnly, actorCompanyId, base.UserId, projectStatuses.Select(s => (TermGroup_ProjectStatus)s).ToList(), onlyMine);
        }

        public List<ProjectGridDTO> GetProjectList(CompEntities entities, int actorCompanyId, int userId, List<TermGroup_ProjectStatus> projectStatuses, bool onlyMine)
        {
            bool finalOnlyMine = FeatureManager.HasRolePermission(Feature.Billing_Project_ProjectsUser, Permission.Modify, base.RoleId, actorCompanyId) == true ? true : onlyMine;
            
            var result = new List<ProjectGridDTO>();
            var statusNumbers = projectStatuses.Select(s => (int)s).ToList();
            var projectStatusTerms = base.GetTermGroupDict(TermGroup.ProjectStatus, GetLangId());

            if (projectStatuses.Count == 1 && projectStatuses.Contains((int)TermGroup_ProjectStatus.Unknown))
                statusNumbers = projectStatusTerms.Select(t => t.Key).ToList();

            var projects = entities.GetProjectsForList(actorCompanyId, userId, (int?)null, finalOnlyMine)
                                   .Where(p => statusNumbers.Contains(p.ProjectStatus));

            foreach (var project in projects)
            {
                result.Add(new ProjectGridDTO
                {
                    ProjectId = project.ProjectId,
                    Name = project.Name,
                    Number = project.Number,
                    Description = project.Description,
                    CustomerName = project.CustomerName,
                    CustomerNr = project.CustomerNr,
                    ChildProjects = string.IsNullOrEmpty(project.ChildProjects) ? "" : project.ChildProjects.Trim().TrimEnd(','),
                    Categories = string.IsNullOrEmpty(project.ProjectCategories) ? "" : project.ProjectCategories.Trim().TrimEnd(','),
                    ManagerName = project.ManagerName,
                    StartDate = project.ProjectStartDate,
                    StopDate = project.ProjectStopDate,
                    DefaultDim2AccountName = project.DefaultDim2AccountName,
                    DefaultDim3AccountName = project.DefaultDim3AccountName,
                    DefaultDim4AccountName = project.DefaultDim4AccountName,
                    DefaultDim5AccountName = project.DefaultDim5AccountName,
                    DefaultDim6AccountName = project.DefaultDim6AccountName,
                    Status = (TermGroup_ProjectStatus)project.ProjectStatus,
                    StatusName = project.ProjectStatus == 0 ? String.Empty : projectStatusTerms[project.ProjectStatus],
                }
               );
            }

            return result;
        }

        public List<Project> GetProjects(int actorCompanyId, TermGroup_ProjectType type, bool? active, bool? getHidden, bool setStatusName, bool includeManagerName, bool loadOrders, int maxNrOfRecords = -1, int projectStatus = 0)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            return this.GetProjects(entitiesReadOnly, actorCompanyId, type, active, getHidden, setStatusName, includeManagerName, loadOrders, maxNrOfRecords, projectStatus);
        }

        public List<Project> GetProjects(CompEntities entities, int actorCompanyId, TermGroup_ProjectType type, bool? active, bool? getHidden, bool setStatusName, bool includeManagerName, bool loadOrders, int maxNrOfRecords = -1, int projectStatus = 0)
        {
            IQueryable<Project> query = entities.Project;
            query = query.Include("Customer");
            if (includeManagerName)
                query = query.Include("ProjectUser.User.ContactPerson");
            if (loadOrders)
                query = query.Include("Invoice.Origin");

            query = query.Where(p => p.ActorCompanyId == actorCompanyId && p.Type == (int)type && p.State != (int)SoeEntityState.Deleted);

            if (active == true)
                query = query.Where(p => p.State == (int)SoeEntityState.Active);
            else if (active == false)
                query = query.Where(p => p.State == (int)SoeEntityState.Inactive);

            if (projectStatus > 0)
                query = query.Where(p => p.Status == projectStatus);
            else if (getHidden != null && getHidden == false)
                query = query.Where(p => p.Status != (int)TermGroup_ProjectStatus.Hidden);

            List<Project> projects = null;
            if (maxNrOfRecords != -1)
                projects = query.Take(maxNrOfRecords).ToList();
            else
                projects = query.ToList();

            if (!projects.IsNullOrEmpty() && setStatusName)
            {
                // Set StatusName extension
                List<int> projectIds = projects.Select(p => p.ProjectId).ToList();
                List<CompanyCategoryRecord> categoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Project, SoeCategoryRecordEntity.Project, projectIds, actorCompanyId);
                List<GenericType> statuses = base.GetTermGroupContent(TermGroup.ProjectStatus, skipUnknown: true);

                foreach (Project project in projects)
                {
                    GenericType status = statuses.FirstOrDefault(s => s.Id == project.Status);
                    project.StatusName = status?.Name ?? string.Empty;
                    project.Categories = string.Join(", ", categoryRecords.GetCategoryRecords(project.ProjectId).Select(c => c.Category.Name));
                }
            }

            return projects;
        }

        public List<ProjectTinyDTO> GetProjectsSmall(int actorCompanyId, TermGroup_ProjectType type, bool? active, bool? getHidden, bool sortOnNumber = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            return this.GetProjectsSmall(entitiesReadOnly, actorCompanyId, type, active, getHidden, sortOnNumber);
        }

        public List<ProjectTinyDTO> GetProjectsSmall(CompEntities entities, int actorCompanyId, TermGroup_ProjectType type, bool? active, bool? getHidden, bool sortOnNumber = false)
        {
            IQueryable<Project> query = entities.Project
                .Where(p => p.ActorCompanyId == actorCompanyId && p.Type == (int)type && p.State != (int)SoeEntityState.Deleted);

            if (active == true)
                query = query.Where(p => p.State == (int)SoeEntityState.Active);
            else if (active == false)
                query = query.Where(p => p.State == (int)SoeEntityState.Inactive);

            if (getHidden != null && getHidden == false)
                query = query.Where(p => p.Status != (int)TermGroup_ProjectStatus.Hidden);

            var projects = query.Select(p => new ProjectTinyDTO { Name = p.Name, Number = p.Number, ProjectId = p.ProjectId, Status = (TermGroup_ProjectStatus)p.Status }).ToList();

            if (sortOnNumber)
                projects = projects.OrderBy(p => p.Number).ToList();

            return projects;
        }

        public IEnumerable<Project> GetProjectsList(int actorCompanyId, TermGroup_ProjectType type, bool? active, bool? getHidden, bool? getFinished)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            var query = (from p in entities.Project
                         where p.ActorCompanyId == actorCompanyId &&
                         p.Type == (int)type &&
                         p.State != (int)SoeEntityState.Deleted
                         select p);

            if (active == true)
                query = query.Where(p => p.State == (int)SoeEntityState.Active);
            else if (active == false)
                query = query.Where(p => p.State == (int)SoeEntityState.Inactive);

            if (getHidden.HasValue && getHidden == false)
                query = query.Where(p => p.Status != (int)TermGroup_ProjectStatus.Hidden);

            if (getFinished.HasValue && getFinished == false)
                query = query.Where(p => p.Status != (int)TermGroup_ProjectStatus.Finished);

            return query.ToList();
        }

        public List<ProjectGridDTO> GetProjectsBySearch(int actorCompanyId, List<int> statusIds, List<int> categoryIds, DateTime? stopDate, bool withoutStopDate, bool setStatusName, bool includeManagerName)
        {
            using (var entities = new CompEntities())
            {
                IQueryable<Project> query = (from p in entities.Project
                                             where p.ActorCompanyId == actorCompanyId &&
                                                p.State == (int)SoeEntityState.Active
                                             orderby p.Number ascending
                                             select p);

                if (!statusIds.IsNullOrEmpty())
                {
                    query = query.Where(p => statusIds.Contains(p.Status));
                }

                if (withoutStopDate)
                {
                    query = query.Where(p => p.StopDate.Equals(null));
                }
                else if (stopDate.HasValue)
                {
                    query = query.Where(p => p.StopDate < stopDate);
                }

                if (!categoryIds.IsNullOrEmpty())
                {
                    query = query.Where(p => entities.CompanyCategoryRecord.Any(c => c.RecordId == p.ProjectId && c.Entity == (int)SoeCategoryRecordEntity.Project && categoryIds.Contains(c.CategoryId)));
                }

                List<ProjectGridDTO> dtos = null;
                Dictionary<int, string> projectManagers = null;

                if (includeManagerName)
                {
                    dtos = query.Select(p => new ProjectGridDTO
                    {
                        ProjectId = p.ProjectId,
                        Number = p.Number,
                        Name = p.Name,
                        CustomerName = p.Customer.Name,
                        CustomerNr = p.Customer.CustomerNr,
                        StopDate = p.StopDate,
                        Status = (TermGroup_ProjectStatus)p.Status,
                        ManagerUserId = p.ProjectUser.FirstOrDefault(u => u.State == (int)SoeEntityState.Active && u.Type == (int)TermGroup_ProjectUserType.Manager).UserId
                    }).ToList();

                    var userIds = dtos.Where(y => y.ManagerUserId.HasValue).Select(x => x.ManagerUserId).Distinct().ToList();

                    if (userIds.Any())
                    {
                        projectManagers = (from pu in entities.User
                                           where userIds.Contains(pu.UserId)
                                           select pu).Select(u => new { u.UserId, Name = u.ContactPerson.FirstName + " " + u.ContactPerson.LastName })
                                             .GroupBy(l => l.UserId)
                                             .ToDictionary(g => g.Key, g => g.FirstOrDefault().Name);
                    }
                }
                else
                {
                    dtos = query.Select(p => new ProjectGridDTO
                    {
                        ProjectId = p.ProjectId,
                        Number = p.Number,
                        Name = p.Name,
                        CustomerName = p.Customer.Name,
                        CustomerNr = p.Customer.CustomerNr,
                        StopDate = p.StopDate,
                        Status = (TermGroup_ProjectStatus)p.Status
                    }).ToList();
                }


                if (setStatusName || includeManagerName)
                {
                    foreach (var dto in dtos)
                    {
                        if (setStatusName)
                        {
                            var statuses = base.GetTermGroupContent(TermGroup.ProjectStatus, skipUnknown: true);
                            GenericType status = statuses.FirstOrDefault(s => s.Id == (int)dto.Status);
                            dto.StatusName = status?.Name ?? string.Empty;
                        }

                        if (dto.ManagerUserId.HasValue && projectManagers != null && projectManagers.Any())
                        {
                            projectManagers.TryGetValue(dto.ManagerUserId.Value, out string managerName);
                            dto.ManagerName = managerName;
                        }
                    }
                }

                return dtos;
            }
        }

        public List<ProjectSmallDTO> GetProjectsBySearch(int actorCompanyId, string project, string customer, bool includeClosed)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            IQueryable<Project> query = (from p in entitiesReadOnly.Project
                                      .Include("Customer")
                                         where p.ActorCompanyId == actorCompanyId &&
                                                 p.State == (int)SoeEntityState.Active
                                         orderby p.Number ascending
                                         select p);

            if (!string.IsNullOrEmpty(project))
            {
                query = query.Where(p => p.Name.ToLower().Contains(project.ToLower()) || p.Number.ToLower().Contains(project.ToLower()));
            }
            if (!string.IsNullOrEmpty(customer))
            {
                query = query.Where(p => p.Customer.Name.ToLower().Contains(customer.ToLower()) || p.Customer.CustomerNr.ToLower().Contains(customer.ToLower()));
            }
            if (includeClosed)
            {
                query = query.Where(p => p.Status == (int)TermGroup_ProjectStatus.Active || p.Status == (int)TermGroup_ProjectStatus.Planned || p.Status == (int)TermGroup_ProjectStatus.Hidden || p.Status == (int)TermGroup_ProjectStatus.Guarantee);
            }
            else
            {
                query = query.Where(p => p.Status == (int)TermGroup_ProjectStatus.Active || p.Status == (int)TermGroup_ProjectStatus.Planned || p.Status == (int)TermGroup_ProjectStatus.Guarantee);
            }

            return query.Select(p => new ProjectSmallDTO
            {
                ProjectId = p.ProjectId,
                Number = p.Number,
                Name = p.Name,
                CustomerName = p.Customer.Name,
                CustomerNumber = p.Customer.CustomerNr
            }).ToList();
        }

        public List<ProjectTinyDTO> GetProjects(int actorCompanyId, int roleId, int customerId, string searchText, bool LoadOnlyPlannedAndActive)
        {
            int maxHits = 250;
            var showProjectsWithoutCustomer = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer, Permission.Modify, roleId, actorCompanyId);

            using (var entities = new CompEntities())
            {

                IQueryable<Project> query = (from p in entities.Project
                                             where
                                                p.ActorCompanyId == actorCompanyId &&
                                                p.State == (int)SoeEntityState.Active
                                             select p);

                if (showProjectsWithoutCustomer)
                    query = query.Where(p => p.CustomerId == customerId || p.CustomerId == null);
                else
                    query = query.Where(p => p.CustomerId == customerId);

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(p => p.Number.Contains(searchText) || p.Name.Contains(searchText));
                    maxHits = 500;
                }

                if (LoadOnlyPlannedAndActive)
                    query = query.Where(item => item.Status == (int)TermGroup_ProjectStatus.Active || item.Status == (int)TermGroup_ProjectStatus.Guarantee || item.Status == (int)TermGroup_ProjectStatus.Planned);

                return query.Take(maxHits).OrderBy(y => y.Created).
                    Select(p => new ProjectTinyDTO { Name = p.Name, Number = p.Number, ProjectId = p.ProjectId, Status = (TermGroup_ProjectStatus)p.Status }).OrderBy(z => z.Number).ToList();
            }
        }

        public bool HasRightToViewProject(int projectId)
        {
            bool permissionOnlyMine = FeatureManager.HasRolePermission(Feature.Billing_Project_ProjectsUser, Permission.Modify, base.RoleId, base.ActorCompanyId);


            if (permissionOnlyMine)
            {
                using (var entities = new CompEntities())
                {
                    int userId = base.UserId;
                    DateTime dateNow = DateTime.Now;
                    var result = (from pu in entities.ProjectUser
                                  where pu.ProjectId == projectId &&
                                  pu.UserId == userId &&
                                  pu.State == (int)SoeEntityState.Active &&
                                  (pu.DateFrom <= dateNow || pu.DateFrom == null) &&
                                  (pu.DateTo >= dateNow || pu.DateTo == null)
                                  select pu.ProjectUserId).FirstOrDefault();
                    bool x = result != 0;
                    return x;
                }
            }
            else
            {
                return true;
            }
        }

        public List<ProjectSearchResultDTO> GetProjectsBySearch2(string number, string name, string customerNr, string customerName, string managerName, string orderNr, bool active, bool hidden, bool showWithoutCustomer, bool loadMine, int? customerId = null, bool showAllProjects = false)
        {
            int actorCompanyId = base.ActorCompanyId;
            int userId = base.UserId;
            DateTime dateNow = DateTime.Now;
            bool permissionOnlyMine = loadMine ? true : FeatureManager.HasRolePermission(Feature.Billing_Project_ProjectsUser, Permission.Modify, base.RoleId, actorCompanyId);
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            IQueryable<Project> query = (from p in entities.Project
                                         where p.ActorCompanyId == actorCompanyId &&
                                         p.State == (int)SoeEntityState.Active
                                         select p);

            if (!showAllProjects && customerId.HasValue)
            {
                if (showWithoutCustomer)
                {
                    query = query.Where(p => (p.CustomerId == customerId.Value || p.CustomerId == null));
                }
                else
                {
                    query = query.Where(p => p.CustomerId == customerId.Value);
                }
            }

            if (!string.IsNullOrEmpty(number))
            {
                query = query.Where(p => p.Number.Contains(number));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(orderNr))
            {
                query = query.Where(p => p.Invoice.Any(i => i.InvoiceNr.Contains(orderNr)));
            }

            if (permissionOnlyMine)
            {
                query = query.Where(p => p.ProjectUser.Any(u => u.UserId == userId && u.State == (int)SoeEntityState.Active && (u.DateFrom <= dateNow || u.DateFrom == null) && (u.DateTo >= dateNow || u.DateTo == null)));
            }

            if (active)
            {
                if (hidden)
                    query = query.Where(p => p.Status == (int)TermGroup_ProjectStatus.Active || p.Status == (int)TermGroup_ProjectStatus.Hidden || p.Status == (int)TermGroup_ProjectStatus.Guarantee);
                else
                    query = query.Where(p => p.Status == (int)TermGroup_ProjectStatus.Active || p.Status == (int)TermGroup_ProjectStatus.Guarantee);
            }
            else if (!hidden)
            {
                query = query.Where(p => p.Status != (int)TermGroup_ProjectStatus.Hidden);
            }

            if (!string.IsNullOrEmpty(customerNr) || !string.IsNullOrEmpty(customerName))
            {
                query = query.Where(p => p.Customer != null &&
                           (string.IsNullOrEmpty(customerName) || p.Customer.Name.Contains(customerName)) &&
                           (string.IsNullOrEmpty(customerNr) || p.Customer.CustomerNr.Contains(customerNr)));
            }
            else if (!showWithoutCustomer)
            {
                query = query.Where(p => p.CustomerId != null);
            }

            Dictionary<int, string> projectManagers = null;
            var result = new List<ProjectSearchResultDTO>();

            if (!string.IsNullOrEmpty(managerName))
            {
                List<Project> temp = query.Include("Customer").Include("Invoice.Origin").Include("ProjectUser.User.ContactPerson").Where(p => p.ProjectUser.Any(pu => pu.Type == (int)TermGroup_ProjectUserType.Manager && pu.State == (int)SoeEntityState.Active)).ToList();
                foreach (Project p in temp)
                {
                    var tempUsr = p.ProjectUser.Where(pu => pu.Type == (int)TermGroup_ProjectUserType.Manager && pu.State == (int)SoeEntityState.Active).Select(u => u.User).ToList();
                    foreach (var usr in tempUsr)
                    {
                        var loopManagerName = usr.ContactPerson != null ? usr.ContactPerson.FirstName + " " + usr.ContactPerson.LastName : usr.Name;
                        if (!string.IsNullOrEmpty(loopManagerName) && loopManagerName.ToLower().Contains(managerName.ToLower()))
                        {
                            var dto = p.ToSearchResultDTO();
                            dto.ManagerName = loopManagerName;
                            result.Add(dto);
                            break;
                        }
                    }
                }
            }
            else
            {
                result = query.Select(EntityExtensions.ProjectSearchResultDTO).ToList();

                var userIds = result.Where(y => y.ManagerUserId.HasValue).Select(x => x.ManagerUserId).Distinct().ToList();

                if (userIds.Any())
                {
                    projectManagers = (from pu in entitiesReadOnly.User
                                       where userIds.Contains(pu.UserId)
                                       select pu).Select(u => new { u.UserId, Name = u.ContactPerson.FirstName + " " + u.ContactPerson.LastName })
                                         .GroupBy(l => l.UserId)
                                         .ToDictionary(g => g.Key, g => g.FirstOrDefault().Name);
                }
            }


            List<GenericType> statuses = base.GetTermGroupContent(TermGroup.ProjectStatus, skipUnknown: true);
            foreach (var dto in result)
            {
                GenericType status = statuses.FirstOrDefault(s => s.Id == (int)dto.Status);
                dto.StatusName = status != null ? status.Name : string.Empty;
                if (dto.OrderNbrs.Any())
                {
                    dto.OrderNr = string.Join(", ", dto.OrderNbrs);
                }

                if (dto.ManagerUserId.HasValue && projectManagers != null && projectManagers.Any())
                {
                    projectManagers.TryGetValue(dto.ManagerUserId.Value, out string managerNameOut);
                    dto.ManagerName = managerNameOut;
                }
            }

            return result;
        }

        public List<Project> GetProjectsByNumberSearch(int actorCompanyId, string projectNr, int maxHits = 100)
        {
            List<Project> projects = GetProjects(actorCompanyId, TermGroup_ProjectType.TimeProject, true, true, false, false, false, maxHits);

            return (from p in projects
                    where (String.IsNullOrEmpty(projectNr) || (p.Number != null && p.Number.Contains(projectNr)) || (p.Name != null && p.Name.Contains(projectNr)))
                    orderby p.Name ascending
                    select p).Take(maxHits).ToList();
        }

        public List<TimeProject> GetTimeProjectsFromSelection(EvaluatedSelection es)
        {
            List<TimeProject> projects = null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            entitiesReadOnly.CommandTimeout = 100;

            if (es.SP_ProjectIds != null && es.SP_ProjectIds.Count > 0)
            {
                projects = (from p in entitiesReadOnly.Project
                                .Include("Customer")
                            where
                                //Company
                                (p.ActorCompanyId == es.ActorCompanyId) &&
                                //ProjectId selection
                                (es.SP_ProjectIds.Contains(p.ProjectId)) &&
                                (p.State == (int)SoeEntityState.Active)
                            select p).OfType<TimeProject>().ToList<TimeProject>();
            }
            else
            {
                projects = (from p in entitiesReadOnly.Project
                            .Include("Customer")
                            where
                                //Company
                                (p.ActorCompanyId == es.ActorCompanyId) &&
                                //ProjectNr selection
                                (es.SB_HasProjectNrInterval == false || es.SB_ProjectNrFrom.Length != es.SB_ProjectNrTo.Length || (p.Number.CompareTo(es.SB_ProjectNrFrom) >= 0 && p.Number.CompareTo(es.SB_ProjectNrTo) <= 0)) &&
                                (p.State == (int)SoeEntityState.Active)
                            select p).OfType<TimeProject>().ToList<TimeProject>();


                if (es.SB_ProjectNrFrom != null && es.SB_ProjectNrTo != null)
                    projects = projects.Where(p => Validator.ValidateStringInterval(es.SB_ProjectNrFrom, p.Number) && Validator.ValidateStringInterval(p.Number, es.SB_ProjectNrTo)).OfType<TimeProject>().ToList<TimeProject>();
            }

            return projects;
        }

        public List<TimeProject> GetTimeProjectsFromSelection(CreateReportResult es, BillingReportParamsDTO reportParam)
        {
            List<TimeProject> projects = null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            entitiesReadOnly.CommandTimeout = 100;

            if (reportParam.SP_ProjectIds != null && reportParam.SP_ProjectIds.Count > 0)
            {
                projects = (from p in entitiesReadOnly.Project
                                .Include("Customer")
                            where
                                //Company
                                (p.ActorCompanyId == es.ActorCompanyId) &&
                                //ProjectId selection
                                (reportParam.SP_ProjectIds.Contains(p.ProjectId)) &&
                                (p.State == (int)SoeEntityState.Active)
                            select p).OfType<TimeProject>().ToList<TimeProject>();
            }
            else
            {
                projects = (from p in entitiesReadOnly.Project
                            .Include("Customer")
                            where
                                //Company
                                (p.ActorCompanyId == es.ActorCompanyId) &&
                                //ProjectNr selection
                                (reportParam.SB_HasProjectNrInterval == false || reportParam.SB_ProjectNrFrom.Length != reportParam.SB_ProjectNrTo.Length || (p.Number.CompareTo(reportParam.SB_ProjectNrFrom) >= 0 && p.Number.CompareTo(reportParam.SB_ProjectNrTo) <= 0)) &&
                                (p.State == (int)SoeEntityState.Active)
                            select p).OfType<TimeProject>().ToList<TimeProject>();


                if (reportParam.SB_ProjectNrFrom != null && reportParam.SB_ProjectNrTo != null)
                    projects = projects.Where(p => Validator.ValidateStringInterval(reportParam.SB_ProjectNrFrom, p.Number) && Validator.ValidateStringInterval(p.Number, reportParam.SB_ProjectNrTo)).OfType<TimeProject>().ToList<TimeProject>();
            }

            return projects;
        }

        public void GetProjectInvoiceMappingIds(int actorCompanyId, List<int> projectIds, ref List<int> invoiceIdsAll, ref Dictionary<int, List<int>> projectInvoicesDict)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Invoice.NoTracking();
            List<Tuple<int, int>> mappings = new List<Tuple<int, int>>();

            var mappingsAll = (from i in entitiesReadOnly.Invoice
                               where
                                   //Company
                                   (i.Origin.ActorCompanyId == actorCompanyId) &&
                                   i.Origin.Status != (int)SoeOriginStatus.Cancel &&
                                   i.ProjectId.HasValue &&
                                   (i.State == (int)SoeEntityState.Active)
                               select new { projectId = i.ProjectId.Value, invoiceId = i.InvoiceId }).ToList();

            foreach (var mapping in mappingsAll)
            {
                if (projectIds.Contains(mapping.projectId))
                    mappings.Add(Tuple.Create(mapping.projectId, mapping.invoiceId));
            }

            var mappingsGroupedByProjectId = mappings.GroupBy(x => x.Item1).ToList();
            foreach (var item in mappingsGroupedByProjectId)
            {
                List<int> invoicesForProject = new List<int>();
                foreach (var x in item)
                {
                    invoiceIdsAll.Add(x.Item2);
                    invoicesForProject.Add(x.Item2);
                }
                projectInvoicesDict.Add(item.Key, invoicesForProject);
            }
        }

        public void GetProjectDayMappingIds(int actorCompanyId, List<int> projectIds, ref List<int> projectDayIdsAll, ref Dictionary<int, List<int>> projectDaysDict)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ProjectInvoiceDay.NoTracking();

            List<Tuple<int, int>> mappings = new List<Tuple<int, int>>();

            var mappingsAll = (from p in entitiesReadOnly.ProjectInvoiceDay
                               where
                                   //Company
                                   (p.ProjectInvoiceWeek.ActorCompanyId == actorCompanyId) &&
                                   (p.ProjectInvoiceWeek.State == (int)SoeEntityState.Active)
                               select new { projectId = p.ProjectInvoiceWeek.ProjectId, dayId = p.ProjectInvoiceDayId }).ToList();

            foreach (var mapping in mappingsAll)
            {
                if (projectIds.Contains(mapping.projectId))
                    mappings.Add(Tuple.Create(mapping.projectId, mapping.dayId));
            }


            var mappingsGroupedByProjectId = mappings.GroupBy(x => x.Item1).ToList();
            foreach (var item in mappingsGroupedByProjectId)
            {
                List<int> daysForProject = new List<int>();
                foreach (var x in item)
                {
                    projectDayIdsAll.Add(x.Item2);
                    daysForProject.Add(x.Item2);
                }
                projectDaysDict.Add(item.Key, daysForProject);
            }
        }

        public List<InvoiceDataForProjectStatisticsReportDTO> GetInvoicesForTimeProjectsFromSelection(List<int> invoiceIds, EvaluatedSelection es)
        {
            return GetInvoicesForTimeProjectsFromSelection(invoiceIds, es.ActorCompanyId, es.SB_CustomerNrFrom, es.SB_CustomerNrTo);
        }

        public List<InvoiceDataForProjectStatisticsReportDTO> GetInvoicesForTimeProjectsFromSelection(List<int> invoiceIds, CreateReportResult es, BillingReportParamsDTO reportParam)
        {
            return GetInvoicesForTimeProjectsFromSelection(invoiceIds, es.ActorCompanyId, reportParam.SB_ActorNrFrom, reportParam.SB_ActorNrTo);
        }

        public List<InvoiceDataForProjectStatisticsReportDTO> GetInvoicesForTimeProjectsFromSelection(List<int> invoiceIds, int actorCompanyId, string actorNrFrom, string actorNrTo)
        {
            if (invoiceIds.Count == 0)
                return new List<InvoiceDataForProjectStatisticsReportDTO>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Invoice.NoTracking();
            IQueryable<CustomerInvoice> query = (from i in entitiesReadOnly.Invoice.OfType<CustomerInvoice>()
                                                 where
                                                    i.Origin.ActorCompanyId == actorCompanyId &&
                                                    i.State == (int)SoeEntityState.Active &&
                                                    i.ProjectId.HasValue
                                                 select i);

            if (!string.IsNullOrEmpty(actorNrFrom) && !string.IsNullOrEmpty(actorNrTo) && actorNrFrom == actorNrTo)
            {
                query = query.Where(i => i.Actor.Customer.CustomerNr == actorNrFrom);
            }

            if (invoiceIds.Count < 1000)
            {
                query = query.Where(i => invoiceIds.Contains(i.InvoiceId));
            }

            var allInvoices = query.Select(x => new InvoiceDataForProjectStatisticsReportDTO
            {
                InvoiceId = x.InvoiceId,
                InvoiceNr = x.InvoiceNr,
                Created = x.Created,
                CreatedBy = x.CreatedBy,
                CustomerName = x.Actor.Customer.Name,
                CustomerNr = x.Actor.Customer.CustomerNr,
            }).ToList();


            var invoiceItems = (invoiceIds.Count < 1000) ? allInvoices : allInvoices.Where(x => invoiceIds.Contains(x.InvoiceId)).ToList();

            if (actorNrTo != null && actorNrFrom != null)
                invoiceItems = invoiceItems.Where(i => (Validator.ValidateStringInterval(actorNrFrom, i.CustomerNr) && Validator.ValidateStringInterval(i.CustomerNr, actorNrTo))).ToList();

            return invoiceItems;
        }

        public List<ProjectTimeBlockInvoiceTransactionReportView> GetProjectTimeBlockInvoiceTransactionsFromSelection(EvaluatedSelection es, List<int> projectIds, List<int> employeeIds)
        {
            var transactions = new List<ProjectTimeBlockInvoiceTransactionReportView>();
            using (CompEntities entities = new CompEntities())
            {
                entities.TimeInvoiceTransactionReport.NoTracking();
                entities.CommandTimeout = 300;

                IQueryable<ProjectTimeBlockInvoiceTransactionReportView> projectTimeCodeTransactions = (from d in entities.ProjectTimeBlockInvoiceTransactionReportView
                                                                                                        where
                                                                                                        d.ActorCompanyId == es.ActorCompanyId
                                                                                                        orderby d.Date
                                                                                                        select d);

                if (projectIds != null && projectIds.Any())
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => projectIds.Contains(d.ProjectId));
                if (employeeIds != null && employeeIds.Any())
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => employeeIds.Contains(d.EmployeeId));
                if (es.HasDateInterval)
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.Date >= es.DateFrom && d.Date <= es.DateTo);

                transactions = projectTimeCodeTransactions.ToList();
            }

            return transactions;
        }
        public List<ProjectTimeBlockInvoiceTransactionReportView> GetProjectTimeBlockInvoiceTransactionsFromSelection(CreateReportResult es, BillingReportParamsDTO reportParam, List<int> projectIds, List<int> employeeIds, bool ignoreProjectInterval = false)
        {
            using (CompEntities entities = new CompEntities())
            {
                entities.TimeInvoiceTransactionReport.NoTracking();
                entities.CommandTimeout = 300;

                IQueryable<ProjectTimeBlockInvoiceTransactionReportView> projectTimeCodeTransactions = (from d in entities.ProjectTimeBlockInvoiceTransactionReportView
                                                                                                        where
                                                                                                            d.ActorCompanyId == es.ActorCompanyId
                                                                                                        orderby d.Date
                                                                                                        select d);

                if (!projectIds.IsNullOrEmpty() && projectIds.Count <= Constants.LINQ_TO_SQL_MAXCONTAINS && !ignoreProjectInterval)
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => projectIds.Contains(d.ProjectId));
                }

                if (reportParam.HasDateInterval)
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.Date >= reportParam.DateFrom && d.Date <= reportParam.DateTo);
                }

                if (!employeeIds.IsNullOrEmpty())
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => employeeIds.Contains(d.EmployeeId));
                }

                var transactions = projectTimeCodeTransactions.ToList();
                if (!projectIds.IsNullOrEmpty() && projectIds.Count > Constants.LINQ_TO_SQL_MAXCONTAINS)
                {
                    transactions = transactions.Where(d => projectIds.Contains(d.ProjectId)).ToList();
                }

                return transactions;
            }
        }

        public List<ProjectTimeBlockPayrollTransactionReportView> GetProjectTimeBlockPayrollTransactionsFromSelection(EvaluatedSelection es, List<int> projectIds, List<int> employeeIds)
        {
            var transactions = new List<ProjectTimeBlockPayrollTransactionReportView>();
            using (CompEntities entities = new CompEntities())
            {
                entities.TimeInvoiceTransactionReport.NoTracking();
                entities.CommandTimeout = 300;

                IQueryable<ProjectTimeBlockPayrollTransactionReportView> projectTimeCodeTransactions = (from d in entities.ProjectTimeBlockPayrollTransactionReportView
                                                                                                        where
                                                                                                        d.ActorCompanyId == es.ActorCompanyId
                                                                                                        orderby d.Date
                                                                                                        select d);

                if ((es.SP_ProjectIds != null && es.SP_ProjectIds.Count > 0) || es.SB_HasProjectNrInterval == true)
                {
                    if (projectIds != null && projectIds.Any())
                    {
                        projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => projectIds.Contains(d.ProjectId));
                    }
                }
                if (es.HasDateInterval)
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.Date >= es.DateFrom && d.Date <= es.DateTo);
                }

                if (employeeIds != null && employeeIds.Any())
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => employeeIds.Contains(d.EmployeeId));

                transactions = projectTimeCodeTransactions.ToList();
            }

            return transactions;
        }
        public List<ProjectTimeBlockPayrollTransactionReportView> GetProjectTimeBlockPayrollTransactionsFromSelection(CreateReportResult es, BillingReportParamsDTO reportParams, List<int> projectIds, List<int> employeeIds, bool ignoreProjectInterval = false)
        {
            using (var entities = new CompEntities())
            {
                entities.TimeInvoiceTransactionReport.NoTracking();
                entities.CommandTimeout = 300;

                IQueryable<ProjectTimeBlockPayrollTransactionReportView> projectTimeCodeTransactions = (from d in entities.ProjectTimeBlockPayrollTransactionReportView
                                                                                                        where
                                                                                                        d.ActorCompanyId == es.ActorCompanyId
                                                                                                        orderby d.Date
                                                                                                        select d);

                if (!projectIds.IsNullOrEmpty() && projectIds.Count <= Constants.LINQ_TO_SQL_MAXCONTAINS && !ignoreProjectInterval)
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => projectIds.Contains(d.ProjectId));
                }

                if (reportParams.HasDateInterval)
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.Date >= reportParams.DateFrom && d.Date <= reportParams.DateTo);
                }

                if (!employeeIds.IsNullOrEmpty())
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => employeeIds.Contains(d.EmployeeId));
                }

                var transactions = projectTimeCodeTransactions.ToList();

                if (!projectIds.IsNullOrEmpty() && projectIds.Count > Constants.LINQ_TO_SQL_MAXCONTAINS)
                {
                    transactions = transactions.Where(d => projectIds.Contains(d.ProjectId)).ToList();
                }

                return transactions;
            }
        }

        public List<TimeInvoiceTransactionReport> GetTimeInvoiceTransactionsFromSelection(List<int> projectInvoiceDayIds, EvaluatedSelection es)
        {
            var transactions = new List<TimeInvoiceTransactionReport>();
            using (CompEntities entities = new CompEntities())
            {
                entities.TimeInvoiceTransactionReport.NoTracking();
                entities.CommandTimeout = 200;

                IQueryable<TimeInvoiceTransactionReport> projectTimeCodeTransactions = (from d in entities.TimeInvoiceTransactionReport
                                                                                        where
                                                                                        d.ActorCompanyId == es.ActorCompanyId && d.ProjectInvoiceWeekState == (int)SoeEntityState.Active && (d.InvoiceTimeInMinutes != 0 || d.WorkTimeInMinutes != 0)
                                                                                        orderby d.ProjectInvoiceDayDate
                                                                                        select d);

                if (es.HasDateInterval)
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.ProjectInvoiceDayDate >= es.DateFrom && d.ProjectInvoiceDayDate <= es.DateTo);
                }

                var transactionsFromDb = projectTimeCodeTransactions.ToList();
                if (es.SB_HasEmployeeNrInterval)
                {
                    if (StringUtility.IsNumeric(es.SB_EmployeeNrFrom) && StringUtility.IsNumeric(es.SB_EmployeeNrTo))
                    {
                        transactionsFromDb = transactionsFromDb.Where(p => Validator.ValidateStringInterval(p.EmployeeNr, es.SB_EmployeeNrFrom, es.SB_EmployeeNrTo)).ToList();
                    }
                    else
                    {
                        transactionsFromDb = transactionsFromDb.Where(d => d.EmployeeNr.CompareTo(es.SB_EmployeeNrFrom) >= 0 && d.EmployeeNr.CompareTo(es.SB_EmployeeNrTo) <= 0).ToList();
                    }
                }

                //Validate against passed days
                foreach (var timeCodeTransaction in transactionsFromDb)
                {
                    if (projectInvoiceDayIds.Contains(timeCodeTransaction.ProjectInvoiceDayId))
                        transactions.Add(timeCodeTransaction);
                }
            }

            return transactions;
        }
        public List<TimeInvoiceTransactionReport> GetTimeInvoiceTransactionsFromSelection(List<int> projectInvoiceDayIds, CreateReportResult es, BillingReportParamsDTO reportParams)
        {
            var transactions = new List<TimeInvoiceTransactionReport>();
            using (CompEntities entities = new CompEntities())
            {
                entities.TimeInvoiceTransactionReport.NoTracking();
                entities.CommandTimeout = 200;

                IQueryable<TimeInvoiceTransactionReport> projectTimeCodeTransactions = (from d in entities.TimeInvoiceTransactionReport
                                                                                        where
                                                                                        d.ActorCompanyId == es.ActorCompanyId && d.ProjectInvoiceWeekState == (int)SoeEntityState.Active && (d.InvoiceTimeInMinutes != 0 || d.WorkTimeInMinutes != 0)
                                                                                        orderby d.ProjectInvoiceDayDate
                                                                                        select d);

                if (reportParams.HasDateInterval)
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.ProjectInvoiceDayDate >= reportParams.DateFrom && d.ProjectInvoiceDayDate <= reportParams.DateTo);
                }

                var transactionsFromDb = projectTimeCodeTransactions.ToList();
                if (reportParams.SB_HasEmployeeNrInterval)
                {
                    transactionsFromDb = transactionsFromDb.Where(p => Validator.ValidateStringInterval(p.EmployeeNr, reportParams.SB_EmployeeNrFrom, reportParams.SB_EmployeeNrTo)).ToList();
                }

                //Validate against passed days
                foreach (var timeCodeTransaction in transactionsFromDb)
                {
                    if (projectInvoiceDayIds.Contains(timeCodeTransaction.ProjectInvoiceDayId))
                        transactions.Add(timeCodeTransaction);
                }
            }

            return transactions;
        }

        public List<TimePayrollTransactionReport> GetTimePayrollTransactionsFromSelection(List<int> projectInvoiceDayIds, EvaluatedSelection es)
        {
            var transactions = new List<TimePayrollTransactionReport>();
            using (CompEntities entities = new CompEntities())
            {
                entities.TimeInvoiceTransactionReport.NoTracking();
                entities.CommandTimeout = 200;

                IQueryable<TimePayrollTransactionReport> projectTimeCodeTransactions = (from d in entities.TimePayrollTransactionReport
                                                                                        where
                                                                                        d.ActorCompanyId == es.ActorCompanyId && d.ProjectInvoiceWeekState == (int)SoeEntityState.Active && (d.InvoiceTimeInMinutes != 0 || d.WorkTimeInMinutes != 0)
                                                                                        orderby d.ProjectInvoiceDayDate
                                                                                        select d);
                if (es.HasDateInterval)
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.ProjectInvoiceDayDate >= es.DateFrom && d.ProjectInvoiceDayDate <= es.DateTo);
                }

                var transactionsFromDb = projectTimeCodeTransactions.ToList();
                if (es.SB_HasEmployeeNrInterval)
                {
                    if (StringUtility.IsNumeric(es.SB_EmployeeNrFrom) && StringUtility.IsNumeric(es.SB_EmployeeNrTo))
                    {
                        transactionsFromDb = transactionsFromDb.Where(p => Validator.ValidateStringInterval(p.EmployeeNr, es.SB_EmployeeNrFrom, es.SB_EmployeeNrTo)).ToList();
                    }
                    else
                    {
                        transactionsFromDb = transactionsFromDb.Where(p => p.EmployeeNr.CompareTo(es.SB_EmployeeNrFrom) >= 0 && p.EmployeeNr.CompareTo(es.SB_EmployeeNrTo) <= 0).ToList();
                    }
                }

                //Validate against passed days
                foreach (var timeCodeTransaction in transactionsFromDb)
                {
                    if (projectInvoiceDayIds.Contains(timeCodeTransaction.ProjectInvoiceDayId))
                        transactions.Add(timeCodeTransaction);
                }
            }

            return transactions;
        }
        public List<TimePayrollTransactionReport> GetTimePayrollTransactionsFromSelection(List<int> projectInvoiceDayIds, CreateReportResult es, BillingReportParamsDTO reportParams)
        {
            var transactions = new List<TimePayrollTransactionReport>();
            using (CompEntities entities = new CompEntities())
            {
                entities.TimeInvoiceTransactionReport.NoTracking();
                entities.CommandTimeout = 200;

                IQueryable<TimePayrollTransactionReport> projectTimeCodeTransactions = (from d in entities.TimePayrollTransactionReport
                                                                                        where
                                                                                        d.ActorCompanyId == es.ActorCompanyId && d.ProjectInvoiceWeekState == (int)SoeEntityState.Active && (d.InvoiceTimeInMinutes != 0 || d.WorkTimeInMinutes != 0)
                                                                                        orderby d.ProjectInvoiceDayDate
                                                                                        select d);
                if (reportParams.HasDateInterval)
                {
                    projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.ProjectInvoiceDayDate >= reportParams.DateFrom && d.ProjectInvoiceDayDate <= reportParams.DateTo);
                }

                var transactionsFromDb = projectTimeCodeTransactions.ToList();
                if (reportParams.SB_HasEmployeeNrInterval)
                    transactionsFromDb = transactionsFromDb.Where(p => Validator.ValidateStringInterval(p.EmployeeNr, reportParams.SB_EmployeeNrFrom, reportParams.SB_EmployeeNrTo)).ToList();


                //Validate against passed days
                foreach (var timeCodeTransaction in transactionsFromDb)
                {
                    if (projectInvoiceDayIds.Contains(timeCodeTransaction.ProjectInvoiceDayId))
                        transactions.Add(timeCodeTransaction);
                }
            }

            return transactions;
        }

        public List<TimeCodeTransaction> GetTimeSheetTimeCodeTransactionsFromSelection(CreateReportResult es, BillingReportParamsDTO reportParams)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeInvoiceTransactionReport.NoTracking();
            entitiesReadOnly.CommandTimeout = 200;

            IQueryable<TimeCodeTransaction> projectTimeCodeTransactions = (from t in entitiesReadOnly.TimeCodeTransaction
                                                                                    .Include("TimePayrollTransaction")
                                                                                    .Include("TimeSheetWeek.Employee")
                                                                           where t.TimeSheetWeekId != null &&
                                                                           t.ProjectId != null &&
                                                                           t.TimeSheetWeek.Employee.ActorCompanyId == es.ActorCompanyId &&
                                                                           t.Type == (int)TimeCodeTransactionType.TimeSheet &&
                                                                           t.State == (int)SoeEntityState.Active
                                                                           select t);
            if (reportParams.HasDateInterval)
            {
                projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.Start >= reportParams.DateFrom && d.Stop <= reportParams.DateTo);
            }

            var transactionsFromDb = projectTimeCodeTransactions.ToList();
            if (reportParams.SB_HasEmployeeNrInterval)
                transactionsFromDb = transactionsFromDb.Where(p => Validator.ValidateStringInterval(p.TimeSheetWeek.Employee.EmployeeNr, reportParams.SB_EmployeeNrFrom, reportParams.SB_EmployeeNrTo)).ToList();

            return transactionsFromDb;

        }

        public List<TimeCodeTransaction> GetTimeSheetTimeCodeTransactionsFromSelection(EvaluatedSelection es)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeInvoiceTransactionReport.NoTracking();
            entitiesReadOnly.CommandTimeout = 200;

            IQueryable<TimeCodeTransaction> projectTimeCodeTransactions = (from t in entitiesReadOnly.TimeCodeTransaction
                                                                                    .Include("TimePayrollTransaction")
                                                                                    .Include("TimeSheetWeek.Employee")
                                                                           where t.TimeSheetWeekId != null &&
                                                                           t.ProjectId != null &&
                                                                           t.TimeSheetWeek.Employee.ActorCompanyId == es.ActorCompanyId &&
                                                                           t.Type == (int)TimeCodeTransactionType.TimeSheet &&
                                                                           t.State == (int)SoeEntityState.Active
                                                                           select t);
            if (es.HasDateInterval)
            {
                projectTimeCodeTransactions = projectTimeCodeTransactions.Where(d => d.Start >= es.DateFrom && d.Stop <= es.DateTo);
            }

            var transactionsFromDb = projectTimeCodeTransactions.ToList();
            if (es.SB_HasEmployeeNrInterval)
            {
                if (StringUtility.IsNumeric(es.SB_EmployeeNrFrom) && StringUtility.IsNumeric(es.SB_EmployeeNrTo))
                {
                    transactionsFromDb = transactionsFromDb.Where(p => Validator.ValidateStringInterval(p.TimeSheetWeek.Employee.EmployeeNr, es.SB_EmployeeNrFrom, es.SB_EmployeeNrTo)).ToList();
                }
                else
                {
                    transactionsFromDb = transactionsFromDb.Where(p => p.TimeSheetWeek.Employee.EmployeeNr.CompareTo(es.SB_EmployeeNrFrom) >= 0 && p.TimeSheetWeek.Employee.EmployeeNr.CompareTo(es.SB_EmployeeNrTo) <= 0).ToList();
                }
            }

            return transactionsFromDb;

        }

        public ActionResult SaveTimeProject(TimeProjectDTO projectInput, List<CompanyCategoryRecordDTO> categoryRecords, List<AccountingSettingDTO> accountSettings, int actorCompanyId)
        {
            if (projectInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeProject");

            // Default result is successful
            var result = new ActionResult();
            string warningMsg = string.Empty;
            int projectId = projectInput.ProjectId;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        Invoice invoice = null;

                        // Get company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        // Check if any account dim is linked to project
                        AccountDim dim = AccountManager.GetProjectAccountDim(entities, actorCompanyId);

                        if (projectInput.InvoiceId != null)
                            invoice = InvoiceManager.GetInvoice(entities, (int)projectInput.InvoiceId);

                        #endregion

                        #region Project

                        // Get existing project
                        TimeProject project = GetTimeProject(entities, projectInput.ProjectId, true, true);
                        if (project == null)
                        {
                            #region Project Add

                            project = new TimeProject
                            {
                                // Generic project data
                                Type = (int)TermGroup_ProjectType.TimeProject,
                                ActorCompanyId = actorCompanyId,
                                ParentProjectId = projectInput.ParentProjectId.ToNullable(),
                                CustomerId = projectInput.CustomerId.ToNullable(),
                                Status = (int)projectInput.Status,
                                AllocationType = (int)projectInput.AllocationType,
                                Number = projectInput.Number,
                                Name = projectInput.Name.Replace("\n", "").Replace("\r", ""),
                                Description = projectInput.Description,
                                StartDate = projectInput.StartDate,
                                StopDate = projectInput.StopDate,
                                Note = projectInput.Note,
                                UseAccounting = projectInput.UseAccounting,
                                WorkSiteKey = projectInput.WorkSiteKey,
                                WorkSiteNumber = projectInput.WorkSiteNumber,

                                // TimeProject specific data
                                PayrollProductAccountingPrio = projectInput.PayrollProductAccountingPrio,
                                InvoiceProductAccountingPrio = projectInput.InvoiceProductAccountingPrio,
                                PriceListTypeId = projectInput.PriceListTypeId,
                            };
                            SetCreatedProperties(project);
                            entities.Project.AddObject(project);

                            #region Accounts

                            if (accountSettings != null)
                            {
                                AddProjectAccounts(entities, actorCompanyId, project, accountSettings);
                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProjectAccountsNotSaved;
                                    return result;
                                }
                            }

                            #endregion

                            #endregion

                            #region Account Add

                            // Add linked Account
                            if (dim != null)
                            {
                                var account = new Account
                                {
                                    AccountDim = dim,
                                    Name = projectInput.Name,
                                    AccountNr = projectInput.Number,
                                    ActorCompanyId = actorCompanyId,
                                };

                                SetCreatedProperties(account);
                                entities.Account.AddObject(account);

                                var accountInternal = new AccountInternal
                                {
                                    Account = account,
                                };

                                SetCreatedProperties(accountInternal);
                                entities.AccountInternal.AddObject(accountInternal);
                            }

                            #endregion
                        }
                        else
                        {
                            #region Prereq

                            if (project.Status != (int)projectInput.Status && projectInput.Status != TermGroup_ProjectStatus.Active && projectInput.Status != TermGroup_ProjectStatus.Hidden && projectInput.Status != TermGroup_ProjectStatus.Guarantee)
                            {
                                if (!project.Invoice.IsLoaded)
                                    project.Invoice.Load();

                                foreach (var i in project.Invoice)
                                {
                                    if (!i.OriginReference.IsLoaded)
                                        i.OriginReference.Load();

                                    if (i.Type == (int)SoeInvoiceType.CustomerInvoice &&
                                        (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.OrderPartlyInvoice))
                                    {
                                        warningMsg = GetText(9113, "Varning, det finns en eller flera öppna ordrar/fakturor kopplade till projektet");
                                        break;
                                    }
                                }
                            }

                            #endregion

                            #region Project Update

                            // Only update account if name or state has changed
                            bool updateAccount = dim != null && (project.Name != projectInput.Name || project.State != (int)projectInput.State);

                            // Generic project data
                            project.Number = projectInput.Number;
                            project.ParentProjectId = projectInput.ParentProjectId.ToNullable();
                            project.CustomerId = projectInput.CustomerId.ToNullable();
                            project.Name = projectInput.Name.Replace("\n", "").Replace("\r", "");
                            project.Description = projectInput.Description;
                            project.State = (int)projectInput.State;
                            project.AllocationType = (int)projectInput.AllocationType;
                            project.StartDate = projectInput.StartDate;
                            project.StopDate = projectInput.StopDate;
                            project.Note = projectInput.Note;
                            project.UseAccounting = projectInput.UseAccounting;
                            project.Status = (int)projectInput.Status;
                            project.WorkSiteKey = projectInput.WorkSiteKey;
                            project.WorkSiteNumber = projectInput.WorkSiteNumber;

                            // TimeProject specific data
                            project.PayrollProductAccountingPrio = projectInput.PayrollProductAccountingPrio;
                            project.InvoiceProductAccountingPrio = projectInput.InvoiceProductAccountingPrio;
                            project.PriceListTypeId = projectInput.PriceListTypeId;

                            SetModifiedProperties(project);

                            #region Accounts

                            if (accountSettings != null)
                            {
                                UpdateProjectAccounts(entities, actorCompanyId, project, accountSettings);
                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProjectAccountsNotSaved;
                                    return result;
                                }
                            }

                            #endregion

                            #endregion

                            #region Account Update

                            // Only name and state can be updated on account
                            if (updateAccount)
                            {
                                // Get account linked to current project
                                Account account = AccountManager.GetAccountByDimNr(entities, projectInput.Number, dim.AccountDimNr, actorCompanyId, onlyActive: false);
                                if (account != null)
                                {
                                    account.Name = projectInput.Name;
                                    account.State = (int)projectInput.State;
                                    account.ActorCompanyId = actorCompanyId;
                                    SetModifiedProperties(account);
                                }
                            }

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            projectId = project.ProjectId;

                            #region Categories

                            if (categoryRecords != null)
                            {
                                result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, categoryRecords, actorCompanyId, SoeCategoryType.Project, SoeCategoryRecordEntity.Project, projectId);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProjectCategoryNotSaved;
                                    return result;
                                }
                            }

                            #endregion
                        }

                        #region Project Budget

                        if (projectInput.BudgetHead != null)
                        {
                            projectInput.BudgetHead.ProjectId = projectId;
                            BudgetManager.SaveBudgetHead(transaction, entities, projectInput.BudgetHead);
                        }

                        #endregion

                        //Commit transaction
                        if (result.Success)
                        {
                            if (invoice != null)
                            {
                                invoice.ProjectId = project.ProjectId;
                                SetModifiedProperties(invoice);
                                SaveChanges(entities, transaction);
                            }

                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
                    result.Exception = ex;
                }
                finally
                {
                    result.StringValue = warningMsg;
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = projectId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SaveTimeProject(TimeProjectDTO projectInput, List<CompanyCategoryRecordDTO> categoryRecords, List<AccountingSettingDTO> accountSettings, List<ProjectUserDTO> projectUsers, Dictionary<int, decimal> prices, bool newPricelist, string pricelistName, int actorCompanyId)
        {
            if (projectInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeProject");

            // Default result is successful
            var result = new ActionResult();
            string warningMsg = string.Empty;
            bool reloadAccountDims = false;
            bool reloadBudget = false;
            int projectId = projectInput.ProjectId;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    bool autoUpdateAccountSettings = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectAutoUpdateAccountSettings, this.UserId, actorCompanyId, 0, false);
                    Account ledgerAccount = null;

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        Invoice invoice = null;

                        // Get company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        // Check if any account dim is linked to project
                        AccountDim projectDim = AccountManager.GetProjectAccountDim(entities, actorCompanyId);

                        if (projectInput.InvoiceId != null)
                            invoice = InvoiceManager.GetInvoice(entities, (int)projectInput.InvoiceId);

                        #endregion

                        #region Project

                        // Get existing project
                        TimeProject project = GetTimeProject(entities, projectInput.ProjectId, true, true);
                        if (project == null)
                        {
                            #region Validate

                            if (entities.Project.Any(p => p.ActorCompanyId == ActorCompanyId && p.Number == projectInput.Number && p.State != (int)SoeEntityState.Deleted))
                                return new ActionResult(false, 0, TermCacheManager.Instance.GetText(2609, (int)TermGroup.AngularBilling, "Projektnummer används"));

                            #endregion

                            #region Project Add

                            project = new TimeProject
                            {
                                // Generic project data
                                Type = (int)TermGroup_ProjectType.TimeProject,
                                ActorCompanyId = actorCompanyId,
                                ParentProjectId = projectInput.ParentProjectId.ToNullable(),
                                CustomerId = projectInput.CustomerId.ToNullable(),
                                Status = (int)projectInput.Status,
                                AllocationType = (int)projectInput.AllocationType,
                                Number = projectInput.Number,
                                Name = projectInput.Name.Replace("\n", "").Replace("\r", ""),
                                Description = projectInput.Description,
                                StartDate = projectInput.StartDate,
                                StopDate = projectInput.StopDate,
                                Note = projectInput.Note,
                                UseAccounting = projectInput.UseAccounting,
                                WorkSiteKey = projectInput.WorkSiteKey,
                                WorkSiteNumber = projectInput.WorkSiteNumber,
                                AttestWorkFlowHeadId = projectInput.AttestWorkFlowHeadId,
                                OrderTemplateId = projectInput.OrderTemplateId,
                                // TimeProject specific data
                                PayrollProductAccountingPrio = projectInput.PayrollProductAccountingPrio,
                                InvoiceProductAccountingPrio = projectInput.InvoiceProductAccountingPrio,
                                PriceListTypeId = projectInput.PriceListTypeId,

                                DefaultDim1AccountId = projectInput.DefaultDim1AccountId,
                                DefaultDim2AccountId = projectInput.DefaultDim2AccountId,
                                DefaultDim3AccountId = projectInput.DefaultDim3AccountId,
                                DefaultDim4AccountId = projectInput.DefaultDim4AccountId,
                                DefaultDim5AccountId = projectInput.DefaultDim5AccountId,
                                DefaultDim6AccountId = projectInput.DefaultDim6AccountId
                            };

                            SetCreatedProperties(project);
                            entities.Project.AddObject(project);

                            #region Accounts

                            if (accountSettings != null)
                            {
                                AddProjectAccounts(entities, actorCompanyId, project, accountSettings);
                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProjectAccountsNotSaved;
                                    return result;
                                }
                            }
                            else if (projectInput.AccountingSettings != null && projectInput.AccountingSettings.Count > 0)
                            {

                                if (project.ProjectAccountStd == null || project.ProjectAccountStd.Count == 0)
                                {
                                    #region Add AccountingSettings

                                    AddProjectAccountsByTypes(entities, actorCompanyId, project, projectInput.AccountingSettings);

                                    #endregion

                                }
                                else
                                {
                                    #region Prereq

                                    List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true);

                                    #endregion

                                    #region Update/Delete AccountingSettings

                                    // Loop over existing settings
                                    foreach (ProjectAccountStd projectAccountStd in project.ProjectAccountStd)
                                    {
                                        // Find setting in input
                                        AccountingSettingsRowDTO settingInput = projectInput.AccountingSettings.FirstOrDefault(a => a.Type == projectAccountStd.Type);
                                        if (settingInput != null)
                                        {
                                            // Standard account
                                            if (settingInput.Account1Id == 0)
                                            {
                                                // Delete account
                                                projectAccountStd.AccountInternal.Clear();
                                                project.ProjectAccountStd.Remove(projectAccountStd);
                                                entities.DeleteObject(projectAccountStd);
                                            }
                                            else
                                            {
                                                // Update account
                                                if (projectAccountStd.AccountStd.AccountId != settingInput.Account1Id)
                                                {
                                                    AccountStd accountStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);
                                                    if (accountStd != null)
                                                        projectAccountStd.AccountStd = accountStd;
                                                }

                                                // Remove existing internal accounts
                                                // No way to update them
                                                projectAccountStd.AccountInternal.Clear();

                                                // Internal accounts
                                                int dimCounter = 1;
                                                foreach (AccountDim accDim in dims)
                                                {
                                                    // Get internal account from input
                                                    dimCounter++;
                                                    int accountId = 0;
                                                    if (dimCounter == 2)
                                                        accountId = settingInput.Account2Id;
                                                    else if (dimCounter == 3)
                                                        accountId = settingInput.Account3Id;
                                                    else if (dimCounter == 4)
                                                        accountId = settingInput.Account4Id;
                                                    else if (dimCounter == 5)
                                                        accountId = settingInput.Account5Id;
                                                    else if (dimCounter == 6)
                                                        accountId = settingInput.Account6Id;

                                                    // Add account internal
                                                    AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                                                    if (accountInternal != null)
                                                        projectAccountStd.AccountInternal.Add(accountInternal);
                                                }
                                            }
                                        }
                                        // Remove from input to prevent adding below
                                        projectInput.AccountingSettings.Remove(settingInput);
                                    }

                                    #endregion
                                }
                            }

                            #endregion

                            #endregion

                            #region Account Add

                            // Add linked Account
                            if (projectDim != null)
                            {
                                reloadAccountDims = true;
                                ledgerAccount = GetOrCreateProjectAccount(entities, actorCompanyId, projectDim,
                                    projectName: project.Name,
                                    projectNumber: project.Number);
                            }

                            #endregion
                        }
                        else
                        {
                            #region Validate

                            if (projectInput.Number != project.Number && entities.Project.Any(p => p.ActorCompanyId == actorCompanyId && p.Number == projectInput.Number && p.ProjectId != project.ProjectId && p.State != (int)SoeEntityState.Deleted))
                                return new ActionResult(false, 0, GetText(7461, "Projektnummer används"));

                            #endregion

                            #region Prereq

                            if (project.Status != (int)projectInput.Status && projectInput.Status != TermGroup_ProjectStatus.Active && projectInput.Status != TermGroup_ProjectStatus.Hidden && projectInput.Status != TermGroup_ProjectStatus.Guarantee)
                            {
                                if (!project.Invoice.IsLoaded)
                                    project.Invoice.Load();

                                foreach (var i in project.Invoice)
                                {
                                    if (!i.OriginReference.IsLoaded)
                                        i.OriginReference.Load();

                                    if (i.Type == (int)SoeInvoiceType.CustomerInvoice &&
                                        (i.Status == (int)SoeOriginStatus.Origin || i.Status == (int)SoeOriginStatus.OrderPartlyInvoice))
                                    {
                                        warningMsg = GetText(9113, "Varning, det finns en eller flera öppna ordrar/fakturor kopplade till projektet");
                                        break;
                                    }
                                }
                            }

                            #endregion

                            #region Project Update

                            // Only update account if name or state has changed
                            bool updateAccount = projectDim != null && (project.Name != projectInput.Name || project.State != (int)projectInput.State);

                            // Generic project data
                            project.Number = projectInput.Number;
                            project.ParentProjectId = projectInput.ParentProjectId.ToNullable();
                            project.CustomerId = projectInput.CustomerId.ToNullable();
                            project.Name = projectInput.Name.Replace("\n", "").Replace("\r", "");
                            project.Description = projectInput.Description;
                            project.State = (int)projectInput.State;
                            project.AllocationType = (int)projectInput.AllocationType;
                            project.StartDate = projectInput.StartDate;
                            project.StopDate = projectInput.StopDate;
                            project.Note = projectInput.Note;
                            project.UseAccounting = projectInput.UseAccounting;
                            project.Status = (int)projectInput.Status;
                            project.WorkSiteKey = projectInput.WorkSiteKey;
                            project.WorkSiteNumber = projectInput.WorkSiteNumber;
                            project.AttestWorkFlowHeadId = projectInput.AttestWorkFlowHeadId;

                            // TimeProject specific data
                            project.PayrollProductAccountingPrio = projectInput.PayrollProductAccountingPrio;
                            project.InvoiceProductAccountingPrio = projectInput.InvoiceProductAccountingPrio;
                            project.PriceListTypeId = projectInput.PriceListTypeId;
                            project.OrderTemplateId = projectInput.OrderTemplateId;

                            project.DefaultDim1AccountId = projectInput.DefaultDim1AccountId;
                            project.DefaultDim2AccountId = projectInput.DefaultDim2AccountId;
                            project.DefaultDim3AccountId = projectInput.DefaultDim3AccountId;
                            project.DefaultDim4AccountId = projectInput.DefaultDim4AccountId;
                            project.DefaultDim5AccountId = projectInput.DefaultDim5AccountId;
                            project.DefaultDim6AccountId = projectInput.DefaultDim6AccountId;

                            SetModifiedProperties(project);

                            #region Accounts

                            if (accountSettings != null)
                            {
                                UpdateProjectAccounts(entities, actorCompanyId, project, accountSettings);
                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProjectAccountsNotSaved;
                                    return result;
                                }
                            }
                            else if (projectInput.AccountingSettings != null && projectInput.AccountingSettings.Count > 0)
                            {
                                AddProjectAccountsByTypes(entities, actorCompanyId, project, projectInput.AccountingSettings);
                            }

                            #endregion

                            #endregion

                            #region Account Update

                            // Only name and state can be updated on account
                            if (updateAccount)
                            {
                                // Get account linked to current project
                                Account account = AccountManager.GetAccountByDimNr(entities, projectInput.Number, projectDim.AccountDimNr, actorCompanyId, onlyActive: false);
                                if (account != null)
                                {
                                    account.Name = projectInput.Name;
                                    account.State = (int)projectInput.State;
                                    account.ActorCompanyId = actorCompanyId;
                                    SetModifiedProperties(account);
                                }
                            }

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            projectId = project.ProjectId;

                            #region Categories

                            if (categoryRecords != null)
                            {
                                result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, categoryRecords, actorCompanyId, SoeCategoryType.Project, SoeCategoryRecordEntity.Project, projectId);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProjectCategoryNotSaved;
                                    return result;
                                }
                            }

                            #endregion

                            #region Auto

                            if (autoUpdateAccountSettings && ledgerAccount != null)
                            {
                                var accountSettingRows = projectInput.AccountingSettings.IsNullOrEmpty() ? new List<AccountingSettingsRowDTO>() : projectInput.AccountingSettings;
                                var dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId);
                                var index = dims.FindIndex(d => d.AccountDimId == projectDim.AccountDimId);
                                if (!accountSettingRows.Any())
                                {
                                    accountSettingRows.Add(new AccountingSettingsRowDTO { Type = (int)ProjectAccountType.Debit });
                                    accountSettingRows.Add(new AccountingSettingsRowDTO { Type = (int)ProjectAccountType.Credit });
                                    accountSettingRows.Add(new AccountingSettingsRowDTO { Type = (int)ProjectAccountType.SalesContractor });
                                    accountSettingRows.Add(new AccountingSettingsRowDTO { Type = (int)ProjectAccountType.SalesNoVat });
                                }

                                switch (index)
                                {
                                    case 1:
                                        project.DefaultDim2AccountId = ledgerAccount.AccountId;
                                        accountSettingRows.ForEach(r => r.Account2Id = ledgerAccount.AccountId);
                                        break;
                                    case 2:
                                        project.DefaultDim3AccountId = ledgerAccount.AccountId;
                                        accountSettingRows.ForEach(r => r.Account3Id = ledgerAccount.AccountId);
                                        break;
                                    case 3:
                                        project.DefaultDim4AccountId = ledgerAccount.AccountId;
                                        accountSettingRows.ForEach(r => r.Account4Id = ledgerAccount.AccountId);
                                        break;
                                    case 4:
                                        project.DefaultDim5AccountId = ledgerAccount.AccountId;
                                        accountSettingRows.ForEach(r => r.Account5Id = ledgerAccount.AccountId);
                                        break;
                                    case 5:
                                        project.DefaultDim6AccountId = ledgerAccount.AccountId;
                                        accountSettingRows.ForEach(r => r.Account6Id = ledgerAccount.AccountId);
                                        break;
                                }

                                AddProjectAccountsByTypes(entities, actorCompanyId, project, accountSettingRows);

                                result = SaveChanges(entities, transaction);
                            }

                            #endregion
                        }

                        #region Project Budget

                        if (projectInput.BudgetHead != null)
                        {
                            projectInput.BudgetHead.ProjectId = projectId;
                            var budgetResult = BudgetManager.SaveBudgetHead(transaction, entities, projectInput.BudgetHead);
                            if (budgetResult.Success && budgetResult.IntegerValue > 0 && projectInput.BudgetHead.BudgetHeadId == 0)
                                reloadBudget = true;
                        }

                        #endregion

                        #region Project users

                        if (!projectUsers.IsNullOrEmpty())
                        {
                            bool calculatedCostPermission = FeatureManager.HasRolePermission(Feature.Billing_Project_EmployeeCalculateCost, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId, entities);

                            IEnumerable<ProjectUser> existingUsers = GetProjectUsers(entities, projectId, false);

                            foreach (var projUserDTO in projectUsers)
                            {
                                ProjectUser projUser = projUserDTO.ProjectUserId > 0 ? existingUsers.FirstOrDefault(u => u.ProjectUserId == projUserDTO.ProjectUserId) : null;
                                var orgDateFrom = projUser?.DateFrom;
                                if (projUser == null)
                                {
                                    if (project.ProjectUser == null)
                                        project.ProjectUser = new EntityCollection<ProjectUser>();

                                    projUser = new ProjectUser()
                                    {
                                        ProjectId = projectId,
                                        UserId = projUserDTO.UserId,
                                    };
                                    SetCreatedProperties(projUser);
                                    project.ProjectUser.Add(projUser);
                                }

                                if (projUserDTO.State == SoeEntityState.Deleted)
                                {
                                    ChangeEntityState(projUser, SoeEntityState.Deleted);
                                    SetModifiedProperties(projUser);
                                }
                                else
                                {
                                    projUser.TimeCodeId = projUserDTO.TimeCodeId.GetValueOrDefault().ToNullable();
                                    projUser.Type = (int)projUserDTO.Type;
                                    projUser.DateFrom = projUserDTO.DateFrom;
                                    projUser.DateTo = projUserDTO.DateTo;

                                    SetModifiedProperties(projUser);
                                }

                                //handle finnish special with employee cost on project....DateFrom from must be set for it to work....
                                if (calculatedCostPermission && (projUser.DateFrom.HasValue || orgDateFrom.HasValue))
                                {
                                    var employee = EmployeeManager.GetEmployeeForUser(entities, projUser.UserId, actorCompanyId);
                                    if (employee != null)
                                    {
                                        var calcDto = EmployeeManager.GetEmployeeCalculatedCosts(entities, employee.EmployeeId, actorCompanyId, projectId).FirstOrDefault(x => x.FromDate == orgDateFrom)?.ToDTO();
                                        if (calcDto == null)
                                        {
                                            calcDto = new EmployeeCalculatedCostDTO
                                            {
                                                EmployeeId = employee.EmployeeId,
                                                ProjectId = projectId,
                                                IsModified = (projUserDTO.EmployeeCalculatedCost != 0),
                                            };
                                        }
                                        else
                                        {
                                            calcDto.IsDeleted = (projUserDTO.EmployeeCalculatedCost == 0) || (projUser.State == (int)SoeEntityState.Deleted) || !projUser.DateFrom.HasValue;
                                            calcDto.IsModified = (!orgDateFrom.HasValue || !projUser.DateFrom.HasValue || orgDateFrom.Value != projUser.DateFrom.Value) || calcDto.CalculatedCostPerHour != projUserDTO.EmployeeCalculatedCost;
                                        }

                                        if (calcDto.IsModified)
                                        {
                                            calcDto.fromDate = projUser.DateFrom;
                                            calcDto.CalculatedCostPerHour = projUserDTO.EmployeeCalculatedCost;
                                            EmployeeManager.SaveEmployeeCalculatedCosts(entities, transaction, employee, new List<EmployeeCalculatedCostDTO> { calcDto });
                                        }
                                    }
                                }
                            }
                        }

                        result = SaveChanges(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Pricelist

                        if (newPricelist)
                        {
                            result = ProductPricelistManager.SavePriceList(entities, transaction, 0, pricelistName, prices, base.ActorCompanyId);

                            if (result.Success)
                            {
                                project.PriceListTypeId = result.IntegerValue;
                                result = SaveChanges(entities, transaction);
                            }
                        }
                        else if (project.PriceListTypeId > 0 && prices.Any())
                        {
                            result = ProductPricelistManager.SavePriceList(entities, transaction, (int)project.PriceListTypeId, null, prices, base.ActorCompanyId);
                        }

                        #endregion


                        //Commit transaction
                        if (result.Success)
                        {
                            if (invoice != null)
                            {
                                invoice.ProjectId = project.ProjectId;
                                SetModifiedProperties(invoice);
                                SaveChanges(entities, transaction);
                            }

                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
                    result.Exception = ex;
                }
                finally
                {
                    result.StringValue = warningMsg;
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = projectId;
                        result.BooleanValue = reloadAccountDims;
                        result.BooleanValue2 = reloadBudget;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private Account GetOrCreateProjectAccount(CompEntities entities, int actorCompanyId, AccountDim accountDim, string projectName, string projectNumber)
        {
            Account account = AccountManager.GetAccountByDimNr(entities, projectNumber, accountDim.AccountDimNr, actorCompanyId, onlyActive: false);

            if (account != null)
                return account;


            var ledgerAccount = new Account
            {
                AccountDim = accountDim,
                Name = projectName,
                AccountNr = projectNumber,
                ActorCompanyId = actorCompanyId,
            };

            SetCreatedProperties(ledgerAccount);
            entities.Account.AddObject(ledgerAccount);

            var accountInternal = new AccountInternal
            {
                Account = ledgerAccount,
            };

            SetCreatedProperties(accountInternal);
            entities.AccountInternal.AddObject(accountInternal);

            return ledgerAccount;
        }

        private void AddProjectAccountsByTypes(CompEntities entities, int actorCompanyId, Project project, List<AccountingSettingsRowDTO> accountSettings)
        {
            List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true);

            if (!accountSettings.IsNullOrEmpty())
            {
                if (project.ProjectAccountStd == null || project.ProjectAccountStd.Count == 0)
                {
                    #region Add AccountingSettings

                    if (project.ProjectAccountStd == null)
                        project.ProjectAccountStd = new EntityCollection<ProjectAccountStd>();

                    foreach (AccountingSettingsRowDTO settingInput in accountSettings)
                    {
                        var hasValues = settingInput.Account1Id != 0 || settingInput.Account2Id != 0 || settingInput.Account3Id != 0 || settingInput.Account4Id != 0 || settingInput.Account5Id != 0 || settingInput.Account6Id != 0;

                        if (!hasValues)
                            continue;

                        AccountStd accStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);
                        ProjectAccountStd projectAccountStd = new ProjectAccountStd
                        {
                            Type = settingInput.Type,
                            AccountStd = accStd
                        };

                        // Internal accounts
                        int dimCounter = 1;
                        foreach (AccountDim dimAcc in dims)
                        {
                            // Get internal account from input
                            dimCounter++;
                            int accountId = 0;
                            if (dimCounter == 2)
                                accountId = settingInput.Account2Id;
                            else if (dimCounter == 3)
                                accountId = settingInput.Account3Id;
                            else if (dimCounter == 4)
                                accountId = settingInput.Account4Id;
                            else if (dimCounter == 5)
                                accountId = settingInput.Account5Id;
                            else if (dimCounter == 6)
                                accountId = settingInput.Account6Id;

                            // Add account internal
                            AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                            if (accountInternal != null)
                                projectAccountStd.AccountInternal.Add(accountInternal);
                        }

                        if (projectAccountStd.AccountStd != null || projectAccountStd.AccountInternal.Any())
                        {
                            project.ProjectAccountStd.Add(projectAccountStd);
                        }
                    }

                    #endregion
                }
                else
                {
                    var settingsToDelete = new List<ProjectAccountStd>();
                    // Loop over existing settings
                    foreach (ProjectAccountStd projectAccountStd in project.ProjectAccountStd)
                    {
                        // Find setting in input
                        AccountingSettingsRowDTO settingInput = accountSettings.FirstOrDefault(a => a.Type == projectAccountStd.Type);
                        if (settingInput != null)
                        {
                            // Standard account
                            if (settingInput.Account1Id == 0 && !settingInput.GetAccountInternalIds().Any())
                            {
                                settingsToDelete.Add(projectAccountStd);
                            }
                            else
                            {
                                // Update account
                                if ((projectAccountStd.AccountStd == null && settingInput.Account1Id > 0) || (projectAccountStd.AccountStd != null && projectAccountStd.AccountStd.AccountId != settingInput.Account1Id))
                                {
                                    projectAccountStd.AccountStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);
                                }

                                // Remove existing internal accounts
                                // No way to update them
                                projectAccountStd.AccountInternal.Clear();

                                // Internal accounts
                                int dimCounter = 1;
                                foreach (AccountDim accDim in dims)
                                {
                                    // Get internal account from input
                                    dimCounter++;
                                    int accountId = 0;
                                    if (dimCounter == 2)
                                        accountId = settingInput.Account2Id;
                                    else if (dimCounter == 3)
                                        accountId = settingInput.Account3Id;
                                    else if (dimCounter == 4)
                                        accountId = settingInput.Account4Id;
                                    else if (dimCounter == 5)
                                        accountId = settingInput.Account5Id;
                                    else if (dimCounter == 6)
                                        accountId = settingInput.Account6Id;

                                    // Add account internal
                                    AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                                    if (accountInternal != null)
                                        projectAccountStd.AccountInternal.Add(accountInternal);
                                }
                            }
                        }
                        // Remove from input to prevent adding below
                        accountSettings.Remove(settingInput);
                    }

                    // Add missing
                    foreach (var settingInput in accountSettings)
                    {
                        AccountStd accStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);

                        ProjectAccountStd projectAccountStd = new ProjectAccountStd
                        {
                            Type = settingInput.Type,
                            AccountStd = accStd
                        };

                        // Internal accounts
                        int dimCounter = 1;
                        foreach (AccountDim dimAcc in dims)
                        {
                            // Get internal account from input
                            dimCounter++;
                            int accountId = 0;
                            if (dimCounter == 2)
                                accountId = settingInput.Account2Id;
                            else if (dimCounter == 3)
                                accountId = settingInput.Account3Id;
                            else if (dimCounter == 4)
                                accountId = settingInput.Account4Id;
                            else if (dimCounter == 5)
                                accountId = settingInput.Account5Id;
                            else if (dimCounter == 6)
                                accountId = settingInput.Account6Id;

                            // Add account internal
                            AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                            if (accountInternal != null)
                                projectAccountStd.AccountInternal.Add(accountInternal);
                        }

                        if (projectAccountStd.AccountStd != null || projectAccountStd.AccountInternal.Any())
                        {
                            project.ProjectAccountStd.Add(projectAccountStd);
                        }
                    }

                    foreach (var settingToDelete in settingsToDelete)
                    {
                        // Delete account
                        settingToDelete.AccountInternal.Clear();
                        project.ProjectAccountStd.Remove(settingToDelete);
                        entities.DeleteObject(settingToDelete);
                    }
                }
            }
        }

        private void AddProjectAccounts(CompEntities entities, int actorCompanyId, Project project, List<AccountingSettingDTO> accountItems)
        {
            if (accountItems.Count == 0)
                return;

            // index
            // 1 = Credit = Fordran
            // 2 = Debit = Intäkt
            ProjectAccountStd projectAccountStd;
            for (int i = 1; i <= 2; i++)
            {
                projectAccountStd = AddProjectAccount(entities, actorCompanyId, accountItems, i);
                if (projectAccountStd != null)
                    project.ProjectAccountStd.Add(projectAccountStd);
            }
        }

        private ProjectAccountStd AddProjectAccount(CompEntities entities, int actorCompanyId, List<AccountingSettingDTO> accountItems, int index)
        {
            // index
            // 1 = Credit = Fordran
            // 2 = Debit = Intäkt
            AccountingSettingDTO stdItem = accountItems.FirstOrDefault(a => a.DimNr == Constants.ACCOUNTDIM_STANDARD);
            if (stdItem != null)
            {
                int accountId = 0;
                int stdAccountType = 0;

                if (index == 1)
                {
                    accountId = stdItem.Account1Id;
                    stdAccountType = (int)ProjectAccountType.Credit;
                }
                else if (index == 2)
                {
                    accountId = stdItem.Account2Id;
                    stdAccountType = (int)ProjectAccountType.Debit;
                }
                else if (index == 3)
                {
                    accountId = stdItem.Account2Id;
                    stdAccountType = (int)ProjectAccountType.SalesNoVat;
                }
                else if (index == 4)
                {
                    accountId = stdItem.Account2Id;
                    stdAccountType = (int)ProjectAccountType.SalesContractor;
                }

                // Standard account
                AccountStd accountStd = AccountManager.GetAccountStd(entities, accountId, actorCompanyId, false, false);
                if (accountStd != null)
                {
                    var projectAccountStd = new ProjectAccountStd
                    {
                        Type = stdAccountType,
                        AccountStd = accountStd
                    };

                    // Add internal accounts
                    AddInternalAccountsToStdAccount(entities, actorCompanyId, projectAccountStd, accountItems, index);

                    return projectAccountStd;
                }
            }

            return null;
        }

        private void UpdateProjectAccounts(CompEntities entities, int actorCompanyId, Project project, List<AccountingSettingDTO> accountItems)
        {
            for (int index = 1; index <= 2; index++)
            {
                // Type may differ from index, since index is just the column order in the DataGrid
                // 1 = Credit = Fordran
                // 2 = Debit = Intäkt
                ProjectAccountType type = ProjectAccountType.Credit;
                if (index == 1)
                    type = ProjectAccountType.Credit;
                else if (index == 2)
                    type = ProjectAccountType.Debit;
                else if (index == 3)
                    type = ProjectAccountType.SalesNoVat;
                else if (index == 4)
                    type = ProjectAccountType.SalesContractor;

                ProjectAccountStd projectAccountStd = project.ProjectAccountStd.FirstOrDefault(a => a.Type == (int)type);
                if (projectAccountStd == null)
                {
                    // No accounts exists, call add method instead
                    projectAccountStd = AddProjectAccount(entities, actorCompanyId, accountItems, index);
                    if (projectAccountStd != null)
                        project.ProjectAccountStd.Add(projectAccountStd);
                }
                else
                {
                    // Always remove and add internal accounts
                    projectAccountStd.AccountInternal.Clear();

                    AccountingSettingDTO stdItem = accountItems.FirstOrDefault(a => a.DimNr == Constants.ACCOUNTDIM_STANDARD);
                    if (stdItem != null)
                    {
                        int accountId = 0;
                        if (index == 1)
                            accountId = stdItem.Account1Id;
                        else if (index == 2)
                            accountId = stdItem.Account2Id;

                        // Update standard account
                        AccountStd accountStd = AccountManager.GetAccountStd(entities, accountId, actorCompanyId, false, false);
                        if (accountStd != null)
                        {
                            projectAccountStd.AccountStd = accountStd;

                            // Add internal accounts
                            AddInternalAccountsToStdAccount(entities, actorCompanyId, projectAccountStd, accountItems, index);
                        }
                        else
                        {
                            // Remove standard account
                            project.ProjectAccountStd.Remove(projectAccountStd);
                            entities.DeleteObject(projectAccountStd);
                        }
                    }
                    else
                    {
                        // Remove standard account
                        project.ProjectAccountStd.Remove(projectAccountStd);
                        entities.DeleteObject(projectAccountStd);
                    }
                }
            }
        }

        private void AddInternalAccountsToStdAccount(CompEntities entities, int actorCompanyId, ProjectAccountStd projectAccountStd, List<AccountingSettingDTO> accountItems, int index)
        {
            // Add internal accounts
            foreach (AccountingSettingDTO intItem in accountItems.Where(a => a.DimNr != Constants.ACCOUNTDIM_STANDARD))
            {
                int intAccountId = 0;
                if (index == 1)
                    intAccountId = intItem.Account1Id;
                else if (index == 2)
                    intAccountId = intItem.Account2Id;

                AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, intAccountId, actorCompanyId);
                if (accountInternal != null)
                    projectAccountStd.AccountInternal.Add(accountInternal);
            }
        }

        public ActionResult UpdateProjectCustomer(int actorCompanyId, int projectId, int orderId, int customerId)
        {
            using (var entities = new CompEntities())
            {
                return UpdateProjectCustomer(entities, actorCompanyId, projectId, orderId, customerId, true);
            }
        }

        public ActionResult UpdateProjectCustomer(CompEntities entities, int actorCompanyId, int projectId, int orderId, int customerId, bool saveChanges)
        {
            // Change customer on project
            var project = (from entry in entities.Project.OfType<TimeProject>().Include("Invoice")
                           where entry.ProjectId == projectId &&
                           entry.ActorCompanyId == actorCompanyId
                           select entry).First();

            if ((project.Invoice.Where(i => i.Type == (int)SoeOriginType.CustomerInvoice).ToList().Count > 1) ||
                (project.Invoice.Count == 1 && orderId > 0 && project.Invoice.First().InvoiceId != orderId) ||
                (project.Invoice.Count > 0 && orderId == 0))
            {
                return new ActionResult((int)ActionResultSave.OneToOneRelationshipRequiredToUpdateProjectCustomer, "In order to change a customer on a project only one order/invoice can be connected to the project");
            }

            project.CustomerId = customerId;
            SetModifiedProperties(project);

            return saveChanges ? SaveChanges(entities) : new ActionResult();
        }

        internal ActionResult ConvertToProjectDTO(CompEntities entities, ProjectIO io, List<Account> accounts, int actorCompanyId, out TimeProjectDTO projectDTO)
        {
            var result = new ActionResult();

            int projectId = entities.Project.Where(p => p.Number == io.ProjectNr && p.ActorCompanyId == actorCompanyId && p.State != (int)SoeEntityState.Deleted).Select(p => p.ProjectId).FirstOrDefault();
            int parentProjectId = entities.Project.Where(p => p.Number == io.ParentProjectNr && p.ActorCompanyId == actorCompanyId && p.State != (int)SoeEntityState.Deleted).Select(p => p.ProjectId).FirstOrDefault();

            projectDTO = new TimeProjectDTO()
            {
                ActorCompanyId = actorCompanyId,
                ProjectId = projectId,
                Number = io.ProjectNr,
                ParentProjectId = string.IsNullOrEmpty(io.ParentProjectNr) ? null : parentProjectId.ToNullable<int>(),
                Description = io.Description,
                Note = io.Note,
                Name = io.Name,
                StartDate = io.StartDate,
                StopDate = io.StopDate
                //Status = io.Status
            };

            // Customer
            if (!string.IsNullOrEmpty(io.CustomerNr))
            {
                var customer = CustomerManager.GetCustomerByNr(actorCompanyId, io.CustomerNr);
                if (customer != null)
                    projectDTO.CustomerId = customer.ActorCustomerId;
            }

            return result;
        }

        #endregion

        #region TimeSheet

        #region Project

        public IEnumerable<Project> GetProjectsForTimeSheetWithCustomer(int employeeId, int actorCompanyId, int? userId)
        {
            // Get planned or active projects for specified user
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            var queryList = (from p in entitiesReadOnly.Project.OfType<TimeProject>()
                        .Include("Customer")
                        .Include("ProjectUser")
                        .Include("Invoice.Origin")
                        .Include("Invoice.Actor.Customer")
                             where p.ActorCompanyId == actorCompanyId &&
                             p.State == (int)SoeEntityState.Active &&
                             (p.Status == (int)TermGroup_ProjectStatus.Planned || p.Status == (int)TermGroup_ProjectStatus.Active || p.Status == (int)TermGroup_ProjectStatus.Hidden || p.Status == (int)TermGroup_ProjectStatus.Guarantee) &&
                             (p.Customer == null || p.Customer.State == (int)SoeEntityState.Active)
                             select p).ToList<Project>();

            // memory quering
            var result = (userId != null) ?
                queryList.Where(u => u.ProjectUser.Any(a => a.UserId == userId) || u.ProjectUser.Count == 0).ToList() :
                queryList.Where(u => u.ProjectUser.Count == 0).ToList();

            // Get projects reported on last three weeks
            DateTime date = DateTime.Today.AddDays(-21).Date;
            entitiesReadOnly.TimeCodeTransaction.NoTracking();
            IEnumerable<Project> projects = (from t in entitiesReadOnly.TimeCodeTransaction
                                                 .Include("Project.Customer")
                                                 .Include("Project.ProjectUser")
                                                 .Include("Project.Invoice.Origin")
                                                .Include("Project.Invoice.Actor.Customer")
                                             where t.Type == (int)TimeCodeTransactionType.TimeSheet &&
                                             t.TimeSheetWeek.EmployeeId == employeeId &&
                                             t.Start >= date &&
                                             t.State == (int)SoeEntityState.Active &&
                                             t.Project.State == (int)SoeEntityState.Active &&
                                             (t.Project.Status == (int)TermGroup_ProjectStatus.Planned || t.Project.Status == (int)TermGroup_ProjectStatus.Active || t.Project.Status == (int)TermGroup_ProjectStatus.Hidden || t.Project.Status == (int)TermGroup_ProjectStatus.Guarantee) &&
                                             (t.Project.Customer == null || t.Project.Customer.State == (int)SoeEntityState.Active)
                                             select t.Project).Distinct().ToList();

            foreach (Project project in projects)
            {
                if (!result.Select(p => p.ProjectId).Contains(project.ProjectId))
                    result.Add(project);
            }

            // Get projects connected to orders reported on last three weeks
            entitiesReadOnly.ProjectInvoiceWeek.NoTracking();
            projects = (from p in entitiesReadOnly.ProjectInvoiceWeek
                            .Include("Project.Customer")
                            .Include("Project.ProjectUser")
                            .Include("Project.Invoice.Origin")
                            .Include("Project.Invoice.Actor.Customer")
                        where p.ActorCompanyId == actorCompanyId &&
                        p.EmployeeId == employeeId &&
                        p.RecordType == (int)SoeProjectRecordType.Order &&
                        p.Date >= date &&
                        p.State == (int)SoeEntityState.Active &&
                        p.Project.State == (int)SoeEntityState.Active &&
                        (p.Project.Status == (int)TermGroup_ProjectStatus.Planned || p.Project.Status == (int)TermGroup_ProjectStatus.Active || p.Project.Status == (int)TermGroup_ProjectStatus.Hidden || p.Project.Status == (int)TermGroup_ProjectStatus.Guarantee) &&
                        (p.Project.Customer == null || p.Project.Customer.State == (int)SoeEntityState.Active)
                        select p.Project).Distinct().ToList();

            foreach (Project project in projects)
            {
                if (!result.Select(p => p.ProjectId).Contains(project.ProjectId))
                    result.Add(project);
            }

            return result.OrderBy(p => p.Name);
        }

        public List<EmployeeProjectInvoiceDTO> GetProjectsForTimeSheetEmployees(int[] employeeIds, int? projectId = 0)
        {
            List<EmployeeProjectInvoiceDTO> projectInvoices = new List<EmployeeProjectInvoiceDTO>();

            int actorCompanyId = base.ActorCompanyId;

            using (var entities = new CompEntities())
            {
                entities.CommandTimeout = 120;
                //First case is for getting if it is not connected to a employee
                List<Employee> employees = (employeeIds.Length == 1 && employeeIds[0] == 0) ?
                                    new List<Employee>() { new Employee { UserId = base.UserId } } :
                                    EmployeeManager.GetAllEmployeesByIds(entities, actorCompanyId, employeeIds.ToList());

                var limitOrderToProjectUsers = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectLimitOrderToProjectUsers, this.UserId, actorCompanyId, 0);
                var defaultTimeCodeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);

                IQueryable<TimeProject> query = (from p in entities.Project.OfType<TimeProject>()
                            //.Include("Customer")
                            .Include("ProjectUser")
                            .Include("Invoice.Origin")
                            .Include("Invoice.Actor.Customer")
                                                 where p.ActorCompanyId == actorCompanyId &&
                                                     p.State == (int)SoeEntityState.Active
                                                 select p);

                if (projectId != 0)
                    query = query.Where(p => p.ProjectId == projectId || p.ParentProjectId == projectId);
                else
                    query = query.Where(p => (p.Status == (int)TermGroup_ProjectStatus.Planned || p.Status == (int)TermGroup_ProjectStatus.Active || p.Status == (int)TermGroup_ProjectStatus.Guarantee)
                    ||
                        (p.Status == (int)TermGroup_ProjectStatus.Hidden && p.Invoice.Any(i => i.Origin.Type == (int)SoeOriginType.Order && i.Origin.Status != (int)SoeOriginStatus.OrderClosed && i.Origin.Status != (int)SoeOriginStatus.OrderFullyInvoice)));

                var allProjects = query.ToList<Project>();

                var standardProjects = limitOrderToProjectUsers ? allProjects.Where(p => !p.ProjectUser.Any(pu => pu.State == 0)).ToList() : allProjects;

                foreach (Employee emp in employees)
                {
                    List<Project> projectsForEmployee = standardProjects;

                    if (limitOrderToProjectUsers)
                    {
                        projectsForEmployee.AddRange(allProjects.Where(u => u.ProjectUser.Any(a => a.UserId == emp.UserId.GetValueOrDefault())));
                    }

                    var dto = new EmployeeProjectInvoiceDTO
                    {
                        EmployeeId = emp.EmployeeId,
                        Projects = projectsForEmployee.ToSmallDTOs(emp.UserId, setCustomer: true).ToList(),
                        Invoices = new List<ProjectInvoiceSmallDTO>(),
                    };

                    if (emp.TimeCodeId != null && emp.TimeCodeId != 0)
                        dto.DefaultTimeCodeId = (int)emp.TimeCodeId;
                    else if (defaultTimeCodeId != 0)
                        dto.DefaultTimeCodeId = defaultTimeCodeId;

                    foreach (ProjectSmallDTO p in dto.Projects.Where(p => p.Invoices != null && p.Invoices.Count > 0))
                    {
                        foreach (ProjectInvoiceSmallDTO i in p.Invoices)
                        {
                            if (!dto.Invoices.Select(ii => ii.InvoiceId).Contains(i.InvoiceId))
                                dto.Invoices.Add(i);
                        }
                    }

                    // Sort
                    dto.Invoices = dto.Invoices.Where(o => o.InvoiceId > 0 && o.InvoiceNr.Trim() != string.Empty).OrderBy(o => Convert.ToInt64(o.InvoiceNr)).ToList();

                    projectInvoices.Add(dto);
                }
            }

            return projectInvoices;
        }

        #endregion

        #region Times

        public DateTime GetEmployeeFirstEligableTime(int employeeId, DateTime date, int actorCompanyId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeScheduleTemplateBlock.NoTracking();
            return GetEmployeeFirstEligableTime(entities, employeeId, date, actorCompanyId, userId);
        }

        public DateTime GetEmployeeFirstEligableTime(CompEntities entities, int employeeId, DateTime date, int actorCompanyId, int userId)
        {
            if (employeeId > 0)
            {
                var currentStopTimes = GetProjectTimeBlockStopTimes(entities, employeeId, date);

                if (currentStopTimes.Any())
                {
                    var lastStopTime = currentStopTimes.OrderBy(t => t).LastOrDefault();
                    return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, lastStopTime.Hour, lastStopTime.Minute, lastStopTime.Second);
                }

                //Check schedule
                var firstTemplateBlockDate = GetFirstTemplateBlockDate(entities, employeeId, date, false);
                if (firstTemplateBlockDate.HasValue)
                {
                    return firstTemplateBlockDate.Value;
                }
            }

            //Check setting
            int dayStartTime = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, userId, actorCompanyId, 0);
            if (dayStartTime != 0)
                return DateTime.Today.AddMinutes(dayStartTime);
            else
                return DateTime.Today.AddHours(7);
        }

        private DateTime? GetFirstTemplateBlockDate(CompEntities entities, int employeeId, DateTime date, bool convertToStandardStart)
        {
            DateTime startDate = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(entities, employeeId, date, true).GetScheduleIn();
            if (startDate == CalendarUtility.DATETIME_DEFAULT || (startDate.Hour == 0 && startDate.Minute == 0))
                return null;

            return (convertToStandardStart) ? startDate : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, startDate.Hour, startDate.Minute, startDate.Second);
        }

        public List<TimeSheetWeek> GetAllTimeSheetsForCompany(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeSheetWeek.NoTracking();
            entities.TimeCodeTransaction.NoTracking();
            entities.TimeCode.NoTracking();
            entities.Project.NoTracking();
            entities.Customer.NoTracking();
            entities.TimeInvoiceTransaction.NoTracking();
            entities.AttestState.NoTracking();
            entities.Employee.NoTracking();
            return (from t in entities.TimeSheetWeek
                            .Include("TimeCodeTransaction.TimeCode")
                            .Include("TimeCodeTransaction.Project.Customer")
                            .Include("TimeCodeTransaction.TimeInvoiceTransaction.AttestState")
                            .Include("Employee")
                    where t.Employee.ActorCompanyId == actorCompanyId
                    select t).ToList();
        }

        public List<TimeSheetDTO> ProjectTimeBlocksDTOToTimeSheetDTO(List<ProjectTimeBlockDTO> projectTimeBlocks, DateTime weekStartDate)
        {
            List<TimeSheetDTO> dtos = new List<TimeSheetDTO>();

            foreach (var projectTimeBlock in projectTimeBlocks)
            {
                var dto = new TimeSheetDTO
                {
                    //TimeSheetWeekId = tsw.TimeSheetWeekId,
                    RowNr = 0,
                    WeekStartDate = weekStartDate,
                    WeekNote = projectTimeBlock.InternalNote,
                    WeekNoteExternal = projectTimeBlock.ExternalNote,
                    ProjectId = projectTimeBlock.ProjectId,
                    ProjectNr = projectTimeBlock.ProjectNr,
                    ProjectName = projectTimeBlock.ProjectName,
                    CustomerId = projectTimeBlock.CustomerId,
                    CustomerName = projectTimeBlock.CustomerName,
                    TimeCodeId = projectTimeBlock.TimeCodeId,
                    TimeCodeName = projectTimeBlock.TimeCodeName,
                    IsReadOnly = false, //tct.Project != null ? tct.Project.Status != (int)TermGroup_ProjectStatus.Active && (tct.CustomerInvoiceRowId != null || tct.SupplierInvoiceId != null) : false,
                    InvoiceId = projectTimeBlock.CustomerInvoiceId,
                    InvoiceNr = projectTimeBlock.InvoiceNr,
                    AttestStateId = projectTimeBlock.CustomerInvoiceRowAttestStateId,
                    AttestStateColor = projectTimeBlock.CustomerInvoiceRowAttestStateColor,
                    AttestStateName = projectTimeBlock.CustomerInvoiceRowAttestStateName
                };

                if (projectTimeBlock.Date == weekStartDate)
                {
                    dto.Monday = CalendarUtility.MinutesToTimeSpan(((int)projectTimeBlock.InvoiceQuantity));
                    dto.MondayActual = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan(0) : CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity);
                    dto.MondayOther = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity) : CalendarUtility.MinutesToTimeSpan(0);
                    dto.MondayNote = projectTimeBlock.InternalNote;
                    dto.MondayNoteExternal = projectTimeBlock.ExternalNote;
                }
                else if (projectTimeBlock.Date == weekStartDate.AddDays(1))
                {
                    dto.Tuesday = CalendarUtility.MinutesToTimeSpan(((int)projectTimeBlock.InvoiceQuantity));
                    dto.TuesdayActual = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan(0) : CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity);
                    dto.TuesdayOther = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity) : CalendarUtility.MinutesToTimeSpan(0);
                    dto.TuesdayNote = projectTimeBlock.InternalNote;
                    dto.TuesdayNoteExternal = projectTimeBlock.ExternalNote;
                }
                else if (projectTimeBlock.Date == weekStartDate.AddDays(2))
                {
                    dto.Wednesday = CalendarUtility.MinutesToTimeSpan(((int)projectTimeBlock.InvoiceQuantity));
                    dto.WednesdayActual = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan(0) : CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity);
                    dto.WednesdayOther = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity) : CalendarUtility.MinutesToTimeSpan(0);
                    dto.WednesdayNote = projectTimeBlock.InternalNote;
                    dto.WednesdayNoteExternal = projectTimeBlock.ExternalNote;
                }
                else if (projectTimeBlock.Date == weekStartDate.AddDays(3))
                {
                    dto.Thursday = CalendarUtility.MinutesToTimeSpan(((int)projectTimeBlock.InvoiceQuantity));
                    dto.ThursdayActual = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan(0) : CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity);
                    dto.ThursdayOther = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity) : CalendarUtility.MinutesToTimeSpan(0);
                    dto.ThursdayNote = projectTimeBlock.InternalNote;
                    dto.ThursdayNoteExternal = projectTimeBlock.ExternalNote;
                }
                else if (projectTimeBlock.Date == weekStartDate.AddDays(4))
                {
                    dto.Friday = CalendarUtility.MinutesToTimeSpan(((int)projectTimeBlock.InvoiceQuantity));
                    dto.FridayActual = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan(0) : CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity);
                    dto.FridayOther = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity) : CalendarUtility.MinutesToTimeSpan(0);
                    dto.FridayNote = projectTimeBlock.InternalNote;
                    dto.FridayNoteExternal = projectTimeBlock.ExternalNote;
                }
                else if (projectTimeBlock.Date == weekStartDate.AddDays(5))
                {
                    dto.Saturday = CalendarUtility.MinutesToTimeSpan(((int)projectTimeBlock.InvoiceQuantity));
                    dto.SaturdayActual = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan(0) : CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity);
                    dto.SaturdayOther = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity) : CalendarUtility.MinutesToTimeSpan(0);
                    dto.SaturdayNote = projectTimeBlock.InternalNote;
                    dto.SaturdayNoteExternal = projectTimeBlock.ExternalNote;
                }
                else if (projectTimeBlock.Date == weekStartDate.AddDays(6))
                {
                    dto.Sunday = CalendarUtility.MinutesToTimeSpan(((int)projectTimeBlock.InvoiceQuantity));
                    dto.SundayActual = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan(0) : CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity);
                    dto.SundayOther = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity) : CalendarUtility.MinutesToTimeSpan(0);
                    dto.SundayNote = projectTimeBlock.InternalNote;
                    dto.SundayNoteExternal = projectTimeBlock.ExternalNote;
                }

                if (projectTimeBlock.Date >= weekStartDate && projectTimeBlock.Date <= weekStartDate.AddDays(6))
                {
                    dto.WeekSum = CalendarUtility.MinutesToTimeSpan(((int)projectTimeBlock.InvoiceQuantity));
                    dto.WeekSumActual = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan(0) : CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity);
                    dto.WeekSumOther = projectTimeBlock.AdditionalTime ? CalendarUtility.MinutesToTimeSpan((int)projectTimeBlock.TimePayrollQuantity) : CalendarUtility.MinutesToTimeSpan(0);
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        public List<ProjectTimeBlockDTO> GetProjectTimeBlockDTOs(DateTime? fromDate, DateTime? toDate, List<int> employeeIds, List<int> projects = null, List<int> orders = null, bool skipTimeTransactions = false, List<int> projectTimeBlockIds = null)
        {
            using (CompEntities entities = new CompEntities())
            {
                return GetProjectTimeBlockDTOs(entities, fromDate, toDate, employeeIds, projects, orders, skipTimeTransactions, projectTimeBlockIds: projectTimeBlockIds);
            }
        }

        public List<ProjectTimeBlockDTO> GetProjectTimeBlockDTOs(CompEntities entities, DateTime? fromDate, DateTime? toDate, List<int> employeeIds, List<int> projects = null, List<int> orders = null, bool skipTimeTransactions = false, int customerInvoiceRowId = 0, bool incPlannedAbsence = false, bool incInternOrderText = false, List<int> timeDeviationCauses = null, List<int> projectTimeBlockIds = null)
        {
            var dtos = new List<ProjectTimeBlockDTO>();
            entities.CommandTimeout = 120;

            IQueryable<ProjectTimeBlockView> projectTimeBlockQuery = (from t in entities.ProjectTimeBlockView
                                                                      where t.ActorCompanyId == ActorCompanyId
                                                                      select t);

            if (!projectTimeBlockIds.IsNullOrEmpty())
                projectTimeBlockQuery = projectTimeBlockQuery.Where(t => projectTimeBlockIds.Contains(t.ProjectTimeBlockId));
            if (employeeIds != null && employeeIds.Count > 0)
                projectTimeBlockQuery = projectTimeBlockQuery.Where(t => employeeIds.Contains(t.EmployeeId));
            if (fromDate.HasValue && fromDate.Value != CalendarUtility.DATETIME_DEFAULT)
                projectTimeBlockQuery = projectTimeBlockQuery.Where(t => t.Date >= fromDate.Value);
            if (toDate.HasValue && fromDate.Value != CalendarUtility.DATETIME_DEFAULT)
                projectTimeBlockQuery = projectTimeBlockQuery.Where(t => t.Date <= toDate);
            if (!projects.IsNullOrEmpty() && projects.Count <= Constants.LINQ_TO_SQL_MAXCONTAINS)
                projectTimeBlockQuery = projectTimeBlockQuery.Where(t => t.ProjectId.HasValue && projects.Contains(t.ProjectId.Value));
            if (orders != null && orders.Count > 0)
                projectTimeBlockQuery = projectTimeBlockQuery.Where(t => t.CustomerInvoiceId.HasValue && orders.Contains(t.CustomerInvoiceId.Value));
            if (!timeDeviationCauses.IsNullOrEmpty())
                projectTimeBlockQuery = projectTimeBlockQuery.Where(t => timeDeviationCauses.Contains(t.TimeDeviationCauseId));

            List<ProjectTimeBlockTransactionsView> projectTimeBlockTransactions = null;

            if (!skipTimeTransactions)
            {
                IQueryable<ProjectTimeBlockTransactionsView> projectTimeBlockTransactionsQuery = (from t in entities.ProjectTimeBlockTransactionsView
                                                                                                  where t.ActorCompanyId == ActorCompanyId
                                                                                                  select t);

                if (employeeIds != null && employeeIds.Count > 0)
                    projectTimeBlockTransactionsQuery = projectTimeBlockTransactionsQuery.Where(t => employeeIds.Contains(t.EmployeeId));
                if (fromDate.HasValue)
                    projectTimeBlockTransactionsQuery = projectTimeBlockTransactionsQuery.Where(t => t.Date >= fromDate.Value);
                if (toDate.HasValue)
                    projectTimeBlockTransactionsQuery = projectTimeBlockTransactionsQuery.Where(t => t.Date <= toDate);
                if (!projects.IsNullOrEmpty() && projects.Count <= Constants.LINQ_TO_SQL_MAXCONTAINS)
                    projectTimeBlockTransactionsQuery = projectTimeBlockTransactionsQuery.Where(t => projects.Contains(t.ProjectId));
                if (orders != null && orders.Count > 0)
                    projectTimeBlockTransactionsQuery = projectTimeBlockTransactionsQuery.Where(t => orders.Contains(t.CustomerInvoiceId));

                projectTimeBlockTransactions = projectTimeBlockTransactionsQuery.ToList();

                if (!projects.IsNullOrEmpty() && projects.Count > Constants.LINQ_TO_SQL_MAXCONTAINS)
                {
                    projectTimeBlockTransactions = projectTimeBlockTransactions.Where(t => projects.Contains(t.ProjectId)).ToList();
                }
            }

            var attestStateInitialPayrollId = AttestManager.GetInitialAttestStateId(entities, base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
            //int attestStateTransferredOrderToInvoiceId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, base.ActorCompanyId, 0);
            int initialAttestStateOrder = InvoiceAttestStates.GetInitialAttestStateId(AttestManager, entities, base.ActorCompanyId, SoeOriginType.Order);
            DateTime? usedPayrollSince = SettingManager.GetDateSetting(entities, SettingMainType.Company, (int)CompanySettingType.UsedPayrollSince, this.UserId, this.ActorCompanyId, 0);

            var projectTimeBlocks = projectTimeBlockQuery.ToList();
            if (!projects.IsNullOrEmpty() && projects.Count > Constants.LINQ_TO_SQL_MAXCONTAINS)
            {
                projectTimeBlocks = projectTimeBlocks.Where(t => t.ProjectId.HasValue && projects.Contains(t.ProjectId.Value)).ToList();
            }

            foreach (var projectTimeBlock in projectTimeBlocks)
            {
                #region ProjectTimeBlock

                var dto = new ProjectTimeBlockDTO
                {
                    ProjectTimeBlockId = projectTimeBlock.ProjectTimeBlockId,
                    TimeSheetWeekId = 0,
                    TimeBlockDateId = projectTimeBlock.TimeBlockDateId,
                    TimeDeviationCauseId = projectTimeBlock.TimeDeviationCauseId,
                    TimeDeviationCauseName = projectTimeBlock.TimeDeviationCauseName,
                    EmployeeId = projectTimeBlock.EmployeeId,
                    EmployeeName = projectTimeBlock.EmployeeName,
                    EmployeeNr = projectTimeBlock.EmployeeNr,
                    CustomerId = projectTimeBlock.CustomerId.HasValue ? projectTimeBlock.CustomerId.Value : 0,
                    CustomerName = projectTimeBlock.CustomerName,
                    Date = projectTimeBlock.Date,
                    YearMonth = projectTimeBlock.Date.Year.ToString() + "-" + projectTimeBlock.Date.Month.ToString(),
                    YearWeek = projectTimeBlock.Date.Year.ToString() + "-" + CalendarUtility.GetWeekNr(projectTimeBlock.Date).ToString(),
                    Year = projectTimeBlock.Date.Year.ToString(),
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(projectTimeBlock.Date.Month),
                    Week = CalendarUtility.GetWeekNr(projectTimeBlock.Date).ToString(),
                    WeekDay = CalendarUtility.GetDayName(projectTimeBlock.Date, CultureInfo.CurrentCulture, true),
                    HasComment = false,
                    StartTime = projectTimeBlock.StartTime,
                    StopTime = projectTimeBlock.StopTime,
                    InvoiceQuantity = projectTimeBlock.InvoiceQuantity,
                    InternalNote = !string.IsNullOrEmpty(projectTimeBlock.InternalNote) ? projectTimeBlock.InternalNote : string.Empty,
                    ExternalNote = !string.IsNullOrEmpty(projectTimeBlock.ExternalNote) ? projectTimeBlock.ExternalNote : string.Empty,
                    EmployeeIsInactive = projectTimeBlock.EmployeeState != (int)SoeEntityState.Active,
                    IsEditable = true,
                    IsPayrollEditable = true,
                    IsSalaryPayrollType = true,
                    Created = projectTimeBlock.Created,
                    CreatedBy = projectTimeBlock.CreatedBy,
                    Modified = projectTimeBlock.Modified,
                    ModifiedBy = projectTimeBlock.ModifiedBy,
                    EmployeeChildId = projectTimeBlock.EmployeeChildId.GetValueOrDefault(),
                    EmployeeChildName = projectTimeBlock.EmployeeChildName,
                    MandatoryTime = projectTimeBlock.MandatoryTime,
                    AdditionalTime = projectTimeBlock.CalculateAsOtherTimeInSales
                };

                //Old times when timeblocks dosent exists
                if (usedPayrollSince.HasValue && usedPayrollSince.Value.Year > 1900 && usedPayrollSince.Value > projectTimeBlock.Date)
                {
                    dto.TimePayrollQuantity = Convert.ToDecimal(dto.StopTime.Subtract(dto.StartTime).TotalMinutes);
                }
                else if (dto.AdditionalTime)
                {
                    //Dosent have any timeblocks to calculate from.....
                    dto.TimePayrollQuantity = Convert.ToDecimal(dto.StopTime.Subtract(dto.StartTime).TotalMinutes);
                }
                else
                {
                    dto.TimePayrollQuantity = projectTimeBlock.TimeBlockQuantity;
                }

                //Project
                if (projectTimeBlock.ProjectId != null)
                {
                    dto.ProjectId = projectTimeBlock.ProjectId.HasValue ? projectTimeBlock.ProjectId.Value : 0;
                    dto.ProjectInvoiceWeekId = 0;
                    dto.ProjectNr = projectTimeBlock.ProjectNumber;
                    dto.ProjectName = projectTimeBlock.ProjectName;
                    dto.AllocationType = (TermGroup_ProjectAllocationType)projectTimeBlock.ProjectAllocationType;
                }

                //CustomerInvoice
                if (projectTimeBlock.CustomerInvoiceId != null)
                {
                    dto.CustomerInvoiceId = projectTimeBlock.CustomerInvoiceId.Value;
                    dto.InvoiceNr = projectTimeBlock.InvoiceNrOrSeqNr;
                    dto.OrderClosed = projectTimeBlock.OriginStatus == (int)SoeOriginStatus.OrderClosed || projectTimeBlock.OriginStatus == (int)SoeOriginStatus.OrderFullyInvoice;
                    dto.RegistrationType = (OrderInvoiceRegistrationType)projectTimeBlock.RegistrationType;
                    dto.ReferenceOur = projectTimeBlock.ReferenceOur;
                    if (incInternOrderText)
                    {
                        dto.InternOrderText = projectTimeBlock.OriginDescription;
                    }
                }

                //TimeCodeTransaction/TimeInvoiceTransactions
                if (!skipTimeTransactions)
                {
                    var currentProjectTransactions = projectTimeBlockTransactions.Where(t => t.ProjectTimeBlockId == projectTimeBlock.ProjectTimeBlockId);
                    if (customerInvoiceRowId > 0 && !currentProjectTransactions.Any(x => x.CustomerInvoiceRowId == customerInvoiceRowId))
                    {
                        continue;
                    }

                    //TimeInvoiceTransaction
                    var timeCodeTransactionForInvoice = currentProjectTransactions.FirstOrDefault(t => t.TimeInvoiceTransactionId >= 0 && t.TimePayrollTransactionId == 0 && t.CustomerInvoiceRowId != null);
                    if (timeCodeTransactionForInvoice == null)
                    {
                        timeCodeTransactionForInvoice = currentProjectTransactions.FirstOrDefault(t => t.TimeInvoiceTransactionId >= 0 && t.TimePayrollTransactionId == 0);
                    }

                    if (timeCodeTransactionForInvoice != null)
                    {
                        dto.TimeCodeId = timeCodeTransactionForInvoice.TimeCodeId.Value;
                        dto.TimeCodeName = timeCodeTransactionForInvoice.TimeCodeName;

                        if (timeCodeTransactionForInvoice.TimeInvoiceTransactionId > 0) //using 0 for some old converted timecodes without artikel
                        {
                            dto.TimeInvoiceTransactionId = timeCodeTransactionForInvoice.TimeInvoiceTransactionId;

                            if (timeCodeTransactionForInvoice.CustomerInvoiceRowId.HasValue &&
                                dto.InvoiceQuantity != timeCodeTransactionForInvoice.TimeInvoiceQuantity &&
                                !timeCodeTransactionForInvoice.AttestStateLocked)
                            {
                                // Handle quantity mismatch
                                dto.CustomerInvoiceRowAttestStateName = string.Format(GetText(7781, "Fakturerad tid skiljer sig från ordern {0} <> {1}"), dto.InvoiceQuantity, timeCodeTransactionForInvoice.TimeInvoiceQuantity);
                            }
                            else if (timeCodeTransactionForInvoice.CustomerInvoiceRowId.HasValue)
                            {
                                dto.CustomerInvoiceRowAttestStateId = timeCodeTransactionForInvoice.AttestStateId;
                                dto.CustomerInvoiceRowAttestStateColor = timeCodeTransactionForInvoice.AttestStateColor;
                                dto.CustomerInvoiceRowAttestStateName = timeCodeTransactionForInvoice.AttestStateName;
                                dto.IsEditable = !timeCodeTransactionForInvoice.AttestStateLocked;
                            }
                            else if (dto.InvoiceQuantity != 0)
                            {
                                dto.CustomerInvoiceRowAttestStateName = GetText(7720, "Artikelrad saknas");
                            }
                        }
                    }

                    //TimePayrollTransaction
                    var timeCodeTransactionsForPayroll = currentProjectTransactions.Where(p => p.TimePayrollTransactionId > 0).ToList();
                    if (timeCodeTransactionsForPayroll.Count > 0)
                    {
                        dto.TimePayrollTransactionIds = new List<int>();
                        foreach (var transaction in timeCodeTransactionsForPayroll.OrderByDescending(x => x.AttestStateSort))
                        {
                            dto.TimePayrollTransactionIds.Add(transaction.TimePayrollTransactionId);

                            var attestStateId = transaction.AttestStateId; //should usually be the same atteststate
                            if (attestStateId != dto.TimePayrollAttestStateId)
                            {
                                dto.TimePayrollAttestStateId = attestStateId;
                                if (dto.TimePayrollAttestStateId > 0)
                                {
                                    dto.TimePayrollAttestStateName = transaction.AttestStateName;
                                    dto.TimePayrollAttestStateColor = transaction.AttestStateColor;
                                    dto.ShowPayrollAttestState = true;
                                    dto.IsPayrollEditable = dto.TimePayrollAttestStateId == attestStateInitialPayrollId;
                                }
                            }
                        }
                    }
                    else if (dto.TimePayrollQuantity > 0)
                    {
                        dto.TimePayrollAttestStateName = GetText(7534, "Löneunderlag saknas");
                    }
                }

                dto.HasComment = (dto.ExternalNote.Trim() != string.Empty || dto.InternalNote.Trim() != string.Empty);
                if (dto.IsEditable && dto.CustomerInvoiceRowAttestStateId > 0)
                {
                    dto.IsEditable = dto.CustomerInvoiceRowAttestStateId == initialAttestStateOrder;
                }
                if (dto.IsEditable)
                {
                    dto.IsEditable = !(projectTimeBlock.CustomerInvoiceId.HasValue && projectTimeBlock.CustomerInvoiceId.Value > 0 ? (projectTimeBlock.OriginStatus == (int)SoeOriginStatus.OrderClosed || projectTimeBlock.OriginStatus == (int)SoeOriginStatus.OrderFullyInvoice || (projectTimeBlock.ProjectStatus != (int)TermGroup_ProjectStatus.Active && projectTimeBlock.ProjectStatus != (int)TermGroup_ProjectStatus.Hidden && projectTimeBlock.ProjectStatus != (int)TermGroup_ProjectStatus.Guarantee)) : (projectTimeBlock.ProjectStatus != (int)TermGroup_ProjectStatus.Active && projectTimeBlock.ProjectStatus != (int)TermGroup_ProjectStatus.Hidden));
                }
                dtos.Add(dto);

                #endregion
            }

            if (incPlannedAbsence)
            {
                var moreDtos = GetProjectTimeBlocksWithoutProjectTimeBlocks(employeeIds, fromDate.Value, toDate.Value);
                if (moreDtos.Any())
                {
                    dtos.AddRange(moreDtos);
                }
            }

            return dtos;
        }

        private List<ProjectTimeBlockDTO> GetProjectTimeBlocksWithoutProjectTimeBlocks(List<int> employeeIds, DateTime from, DateTime to)
        {
            var dtos = new List<ProjectTimeBlockDTO>();
            var blocks = TimeBlockManager.GetTimeBlocksWithNoProjectTimeBlock(employeeIds, from, to, true, true, true, true, true);

            foreach (var block in blocks.GroupBy(b => new { b.EmployeeId, b.TimeBlockDateId, b.TimeDeviationCauseStartId }))
            {
                var first = block.First();

                if (first.TimeDeviationCauseStart != null && first.TimeDeviationCauseStart.IsPresence)
                {
                    continue;
                }

                var dto = new ProjectTimeBlockDTO
                {
                    EmployeeId = first.EmployeeId,
                    EmployeeName = first.Employee?.Name,
                    EmployeeNr = first.Employee?.EmployeeNr,
                    Date = first.TimeBlockDate.Date,
                    TimeBlockDateId = first.TimeBlockDateId,
                    TimeDeviationCauseId = first.TimeDeviationCauseStart != null ? first.TimeDeviationCauseStart.TimeDeviationCauseId : 0,
                    TimeDeviationCauseName = first.TimeDeviationCauseStart != null ? first.TimeDeviationCauseStart.Name : "",
                    StartTime = block.Min(b => b.StartTime),
                    StopTime = block.Max(b => b.StopTime),
                    TimePayrollQuantity = Convert.ToDecimal(block.Sum(b => b.StopTime.Subtract(b.StartTime).TotalMinutes)),
                    EmployeeChildId = first.EmployeeChildId.GetValueOrDefault(),
                    EmployeeChildName = first.EmployeeChild?.Name
                };

                dto.YearMonth = dto.Date.Year.ToString() + "-" + dto.Date.Month.ToString();
                dto.YearWeek = dto.Date.Year.ToString() + "-" + CalendarUtility.GetWeekNr(dto.Date).ToString();
                dto.Year = dto.Date.Year.ToString();
                dto.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dto.Date.Month);
                dto.Week = CalendarUtility.GetWeekNr(dto.Date).ToString();
                dto.WeekDay = CalendarUtility.GetDayName(dto.Date, CultureInfo.CurrentCulture, true);
                /*
                var timeCode = first.TimeCode.FirstOrDefault();
                if (timeCode != null)
                {
                    dto.TimeCodeId = timeCode.TimeCodeId;
                    dto.TimeCodeName = timeCode.Name;
                }
                */
                dtos.Add(dto);
            }
            return dtos;
        }

        private List<ProjectTimeBlockDTO> GroupProjectTimeBlockDTOByDate(List<ProjectTimeBlockDTO> dtos)
        {
            var groupedDtos = new List<ProjectTimeBlockDTO>();
            var groupByDates = dtos.GroupBy(x => new { x.EmployeeId, x.TimeBlockDateId });

            foreach (var item in groupByDates)
            {
                var firstDto = item.First();

                if (item.Count() > 1)
                {
                    firstDto.TimePayrollQuantity = item.Sum(x => x.TimePayrollQuantity);
                    firstDto.InvoiceQuantity = item.Sum(x => x.InvoiceQuantity);
                    firstDto.ProjectNr = string.Join(", ", item.Select(x => x.ProjectNr).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList());
                    firstDto.ProjectName = string.Join(", ", item.Select(x => x.ProjectName).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList());
                    firstDto.InvoiceNr = string.Join(", ", item.Select(x => x.InvoiceNr).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList());
                    firstDto.CustomerName = string.Join(", ", item.Select(x => x.CustomerName).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList());
                    firstDto.TimeDeviationCauseName = string.Join(", ", item.Select(x => x.TimeDeviationCauseName).Distinct().ToList());
                    firstDto.TimeCodeName = string.Join(", ", item.Select(x => x.TimeCodeName).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList());
                }

                firstDto.HasComment = false;
                firstDto.InternalNote = "";
                firstDto.ExternalNote = "";
                firstDto.ProjectTimeBlockId = 0;
                firstDto.CustomerInvoiceId = 0;
                firstDto.IsEditable = false;
                firstDto.IsPayrollEditable = false;
                firstDto.ShowInvoiceRowAttestState = false;
                firstDto.ShowPayrollAttestState = false;

                firstDto.TimePayrollAttestStateId = 0;
                firstDto.CustomerInvoiceRowAttestStateId = 0;
                firstDto.TimePayrollAttestStateColor = null;
                firstDto.CustomerInvoiceRowAttestStateColor = null;

                groupedDtos.Add(firstDto);
            }

            return groupedDtos;
        }

        private void SetScheduledTime(CompEntities entities, List<ProjectTimeBlockDTO> projectTimeBlocks, DateTime fromDate, DateTime toDate, List<int> employeeIds)
        {
            var employeeIdsToFilter = (employeeIds != null && employeeIds.Count > 0) ? employeeIds : projectTimeBlocks.Select(p => p.EmployeeId).Distinct().ToList();
            var scheduledBlocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entities, employeeIdsToFilter, fromDate, toDate, false, null);

            foreach (var employeesTimeBlocks in projectTimeBlocks.GroupBy(x => x.EmployeeId))
            {
                var employeeScheduledBlocks = scheduledBlocks.Where(eb => eb.EmployeeId == employeesTimeBlocks.Key);
                foreach (var employeesTimeBlocksForDate in employeesTimeBlocks.GroupBy(y => y.Date))
                {
                    var schedule = employeeScheduledBlocks.Where(ed => ed.Date == employeesTimeBlocksForDate.Key).ToList();
                    var workMinues = schedule.GetWorkMinutes();

                    foreach (var projectTimeBlock in employeesTimeBlocksForDate.ToList())
                    {
                        projectTimeBlock.ScheduledQuantity = workMinues;
                    }
                }
            }
        }

        public ActionResult SaveProjectTimeMatrixBlocks(List<ProjectTimeMatrixSaveDTO> projectTimeMatrixDTOs, int actorCompanyId)
        {
            var dtosToSave = new List<ProjectTimeBlockSaveDTO>();

            bool extendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, actorCompanyId, 0);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId));

            Employee employee = null;
            foreach (var matrixDto in projectTimeMatrixDTOs)
            {
                if (employee == null)
                {
                    employee = EmployeeManager.GetEmployee(matrixDto.EmployeeId, actorCompanyId);
                }
                foreach (var row in matrixDto.Rows)
                {

                    var projectTimeBlockDto = new ProjectTimeBlockSaveDTO
                    {
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = matrixDto.EmployeeId,
                        ProjectId = matrixDto.ProjectId,
                        CustomerInvoiceId = matrixDto.CustomerInvoiceId,
                        TimeDeviationCauseId = matrixDto.TimeDeviationCauseId,
                        TimeCodeId = matrixDto.TimeCodeId,
                        ProjectInvoiceWeekId = matrixDto.ProjectInvoiceWeekId,
                        ProjectInvoiceDayId = 0,
                        InvoiceQuantity = row.InvoiceQuantity,
                        TimePayrollQuantity = row.PayrollQuantity,
                        InternalNote = row.InternalNote,
                        ExternalNote = row.ExternalNote,
                        ProjectTimeBlockId = row.ProjectTimeBlockId,
                        Date = matrixDto.WeekDate.AddDays(row.WeekDay - 1),
                        State = matrixDto.IsDeleted ? SoeEntityState.Deleted : SoeEntityState.Active,
                        From = CalendarUtility.DATETIME_DEFAULT,
                        To = CalendarUtility.DATETIME_DEFAULT,
                        EmployeeChildId = row.EmployeeChildId
                    };

                    if (projectTimeBlockDto.State == SoeEntityState.Active && projectTimeBlockDto.TimePayrollQuantity == 0 && projectTimeBlockDto.InvoiceQuantity == 0 && string.IsNullOrEmpty(projectTimeBlockDto.InternalNote))
                    {
                        projectTimeBlockDto.State = SoeEntityState.Deleted;
                    }

                    if (extendedTimeRegistration)
                    {
                        projectTimeBlockDto.AutoGenTimeAndBreakForProject = employee.GetAutoGenTimeAndBreakForProject(projectTimeBlockDto.Date.Value, employeeGroups).GetValueOrDefault(false);
                    }
                    dtosToSave.Add(projectTimeBlockDto);
                }
            }

            if (extendedTimeRegistration)
            {
                var validationResult = ValidateProjectTimeBlockSaveData(dtosToSave);
                if (!validationResult.Success)
                {
                    return validationResult;
                }
            }

            return SaveProjectTimeBlocks(dtosToSave, false);
        }

        public List<ProjectTimeMatrixDTO> LoadProjectTimeBlockForMatrix(int employeeId, int selectedEmployeeId, DateTime? fromDate, DateTime? toDate, bool isCopying)
        {
            var dtos = new List<ProjectTimeMatrixDTO>();

            var timeBlockGroups = LoadProjectTimeBlockForTimeSheet(fromDate, toDate, selectedEmployeeId, new List<int> { selectedEmployeeId }, null, null, false, false, true).GroupBy(c =>
                     new
                     {
                         c.ProjectId,
                         c.CustomerInvoiceId,
                         //c.TimeBlockDateId,
                         c.TimeDeviationCauseId,
                         c.TimeCodeId,
                         c.Week
                     }
                );

            foreach (var group in timeBlockGroups)
            {
                var first = group.First();

                if (isCopying && first.OrderClosed)
                {
                    continue;
                }

                var dayMatrix = new ProjectTimeMatrixDTO
                {
                    ProjectId = first.ProjectId,
                    ProjectName = first.ProjectName,
                    ProjectNr = first.ProjectNr,
                    InvoiceNr = first.InvoiceNr,
                    CustomerInvoiceId = first.CustomerInvoiceId,
                    CustomerName = first.CustomerName,
                    TimeCodeId = first.TimeCodeId,
                    TimeCodeName = first.TimeCodeName,
                    TimeDeviationCauseId = first.TimeDeviationCauseId,
                    TimeDeviationCauseName = first.TimeDeviationCauseName,
                    EmployeeId = first.EmployeeId,
                    ProjectInvoiceWeekId = first.ProjectInvoiceWeekId,
                    Rows = new List<ProjectTimeMatrixSaveRowDTO>()
                };

                foreach (var day in group.GroupBy(c => c.Date))
                {
                    var dayNr = CalendarUtility.GetDayNr(day.Key);
                    var onlyDay = (day.Count() == 1) ? day.First() : null;
                    dayMatrix.Rows.Add(new ProjectTimeMatrixSaveRowDTO
                    {
                        WeekDay = dayNr,
                        InvoiceQuantity = day.Sum(s => s.InvoiceQuantity),
                        PayrollQuantity = day.Sum(s => s.TimePayrollQuantity),
                        ProjectTimeBlockId = day.First().ProjectTimeBlockId,
                        ExternalNote = onlyDay?.ExternalNote,
                        InternalNote = onlyDay?.InternalNote,
                        IsPayrollEditable = onlyDay != null && !onlyDay.MandatoryTime ? onlyDay.IsPayrollEditable : false,
                        IsInvoiceEditable = onlyDay != null && !onlyDay.MandatoryTime ? onlyDay.IsEditable : false,
                        InvoiceStateColor = onlyDay != null ? onlyDay.CustomerInvoiceRowAttestStateColor : "",
                        PayrollStateColor = onlyDay != null ? onlyDay.TimePayrollAttestStateColor : "",
                        EmployeeChildId = onlyDay != null ? onlyDay.EmployeeChildId : 0
                    });
                }

                dtos.Add(dayMatrix);
            }

            //padleft to get sort to do the right thing..... 
            return dtos.OrderBy(o => o.InvoiceNr?.PadLeft(7, '0')).ThenBy(x => x.TimeDeviationCauseName).ThenBy(x => x.TimeCodeName).ToList();
        }

        public List<ProjectTimeBlockDTO> LoadProjectTimeBlockForTimeSheet(DateTime? fromDate, DateTime? toDate, int employeeId, List<int> selectedEmployees, List<int> projects = null, List<int> orders = null, bool groupByDate = false, bool incPlannedAbsence = false, bool setAttestStates = false, bool incInternOrderText = false, List<int> employeeCategories = null, List<int> timeDeviationCauses = null)
        {
            var dtos = new List<ProjectTimeBlockDTO>();

            int actorCompanyId = base.ActorCompanyId;
            fromDate = fromDate == null ? new DateTime(1, 1, 1) : fromDate;
            toDate = toDate == null ? new DateTime(9999, 1, 1) : toDate;

            if (selectedEmployees == null)
                selectedEmployees = new List<int>();
            if (projects == null)
                projects = new List<int>();
            if (orders == null)
                orders = new List<int>();

            using (var entities = new CompEntities())
            {

                #region Prereq

                if (fromDate.HasValue)
                    fromDate = CalendarUtility.GetBeginningOfDay(fromDate.Value);
                if (toDate.HasValue)
                    toDate = CalendarUtility.GetEndOfDay(toDate.Value);

                List<int> employeeIds = new List<int>();

                if (selectedEmployees.Count > 0)
                    employeeIds.AddRange(selectedEmployees);
                else
                    employeeIds.AddRange(EmployeeManager.GetEmployeesForUsersAttestRoles(out _, base.ActorCompanyId, base.UserId, base.RoleId, active: null).Select(i => i.EmployeeId).ToList());

                if (!employeeCategories.IsNullOrEmpty())
                {
                    var employeeIdsByCategory = EmployeeManager.GetEmployeeIdsByCategoryIds(entities, ActorCompanyId, employeeCategories);
                    employeeIds = employeeIdsByCategory.Where(e => employeeIds.Contains(e)).ToList();
                }

                #endregion

                bool useProjectTimeBlocks = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
                if (useProjectTimeBlocks)
                {
                    if (employeeId != 0)
                    {
                        employeeIds.Clear();
                        employeeIds.Add(employeeId);
                    }

                    dtos = GetProjectTimeBlockDTOs(entities, fromDate, toDate, employeeIds, projects, orders, false, 0, incPlannedAbsence, incInternOrderText, timeDeviationCauses);
                }
                else
                {
                    #region ProjectInvoiceWeek

                    var attestStates = new List<AttestStateDTO>();
                    if (setAttestStates)
                    {
                        attestStates.AddRange(AttestManager.GetAttestStates(entities, actorCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing).ToDTOs());
                        attestStates.AddRange(AttestManager.GetAttestStates(entities, actorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time).ToDTOs());
                    }

                    /*
                    IQueryable<ProjectInvoiceDayView> query = (from p in entities.ProjectInvoiceDayView
                                                               where p.ActorCompanyId == actorCompanyId &&
                                                                (p.ProjectInvoiceDayDate >= fromDate.Value) &&
                                                                (p.ProjectInvoiceDayDate <= toDate.Value) &&
                                                                p.RecordType == (int)SoeProjectRecordType.Order
                                                               select p);

                    query = (employeeId != 0) ? query.Where(p => p.EmployeeId == employeeId) : query.Where(p => employeeIds.Contains(p.EmployeeId));

                    foreach (ProjectInvoiceDayView dayItem in query.ToList())
                    {
                        var projectTimeBlock = SetProjectInvoiceWeekValues(dayItem);
                        if (projectTimeBlock != null)
                            dtos.Add(projectTimeBlock);
                    }
                    */
                    var weekStartDate = CalendarUtility.GetFirstDateOfWeek(fromDate);
                    var weekEndDate = CalendarUtility.GetLastDateOfWeek(toDate);

                    entities.ProjectInvoiceWeek.NoTracking();
                    entities.CommandTimeout = 120;
                    IQueryable<ProjectInvoiceWeek> query = (from p in entities.ProjectInvoiceWeek
                                                  .Include("Project.Customer")
                                                  .Include("Employee.ContactPerson")
                                                  .Include("ProjectInvoiceDay.TimeCodeTransaction.TimeCode")
                                                  .Include("ProjectInvoiceDay.TimeCodeTransaction.TimeInvoiceTransaction.AttestState")
                                                            //.Include("ProjectInvoiceDay.TimeCodeTransaction.TimePayrollTransaction")
                                                            where p.ActorCompanyId == actorCompanyId &&
                                                              (p.Date >= weekStartDate) &&
                                                              (p.Date <= weekEndDate) &&
                                                              p.RecordType == (int)SoeProjectRecordType.Order &&
                                                              p.State == (int)SoeEntityState.Active
                                                            //&& p.Project.State == (int)SoeEntityState.Active
                                                            select p);

                    if (employeeId != 0)
                    {
                        query = query.Where(p => p.EmployeeId == employeeId);
                    }
                    else
                    {
                        query = query.Where(p => employeeIds.Contains(p.EmployeeId));
                    }

                    if (projects.Count > 0)
                        query = query.Where(p => projects.Contains(p.ProjectId));

                    if (orders.Count > 0)
                        query = query.Where(p => orders.Contains(p.RecordId));

                    List<ProjectInvoiceWeek> projectInvoiceWeeks = query.ToList();

                    // Load invoices for weeks
                    List<int> invoiceIds = projectInvoiceWeeks.Select(p => p.RecordId).Distinct().ToList();
                    List<CustomerInvoice> invoices = (from i in entities.Invoice.OfType<CustomerInvoice>()
                                                            .Include("Origin")
                                                            .Include("Actor.Customer")
                                                      where i.Origin.ActorCompanyId == actorCompanyId &&
                                                      invoiceIds.Contains(i.InvoiceId) &&
                                                      i.State == (int)SoeEntityState.Active
                                                      select i).ToList();

                    // Convert to DTOs
                    foreach (ProjectInvoiceWeek projectInvoiceWeek in projectInvoiceWeeks)
                    {
                        foreach (ProjectInvoiceDay projectInvoiceDay in projectInvoiceWeek.ProjectInvoiceDay.Where(d => d.Date >= fromDate.Value && d.Date <= toDate.Value))
                        {
                            foreach (TimeCodeTransaction timeCodeTransaction in projectInvoiceDay.TimeCodeTransaction.Where(t => t.State == (int)SoeEntityState.Active))
                            {
                                ProjectTimeBlockDTO projectTimeBlock = SetProjectInvoiceWeekValues(projectInvoiceWeek, projectInvoiceDay, timeCodeTransaction, invoices, attestStates, incInternOrderText);
                                if (projectTimeBlock != null)
                                    dtos.Add(projectTimeBlock);
                            }
                        }
                    }

                    #endregion

                    #region TimeSheetWeek

                    //List<TimeSheetWeek> timeSheetWeeks = null;

                    IQueryable<ProjectTimeSheetWeekView> timeSheetQuery = (from p in entities.ProjectTimeSheetWeekView
                                                                           where p.ActorCompanyId == actorCompanyId &&
                                                                            (p.TimeSheetWeekDate >= fromDate.Value) &&
                                                                            (p.TimeSheetWeekDate <= toDate.Value)
                                                                           select p);

                    timeSheetQuery = (employeeId != 0) ? timeSheetQuery.Where(p => p.EmployeeId == employeeId) : timeSheetQuery.Where(p => employeeIds.Contains(p.EmployeeId));

                    if (projects.Count > 0)
                        timeSheetQuery = timeSheetQuery.Where(p => p.ProjectId != null && projects.Contains(p.ProjectId.Value));

                    // Convert to DTOs
                    foreach (ProjectTimeSheetWeekView timeSheetWeek in timeSheetQuery.ToList())
                    {
                        ProjectTimeBlockDTO projectTimeBlock = SetTimeSheetWeekValues(timeSheetWeek);
                        if (projectTimeBlock != null)
                            dtos.Add(projectTimeBlock);
                    }

                    #endregion
                }

                if (groupByDate && fromDate.HasValue && toDate.HasValue)
                {
                    dtos = GroupProjectTimeBlockDTOByDate(dtos);
                    SetScheduledTime(entities, dtos, fromDate.Value, toDate.Value, employeeIds);
                }
            }

            return dtos.OrderBy(x => x.Date).ThenBy(y => y.StartTime).ToList();
        }

        public List<ProjectTimeBlockDTO> LoadProjectTimeBlockForTimeSheetByProjectId(DateTime? fromDate, DateTime? toDate, int projectId, bool includeChildProjects, int employeeId)
        {
            var projectIds = new List<int>();
            employeeId = 0;
            if (includeChildProjects)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var projects = entitiesReadOnly.GetProjectHierarchy(projectId, this.ActorCompanyId);
                foreach (var project in projects)
                {
                    if (project.ProjectId != null)
                    {
                        projectIds.Add((int)project.ProjectId);
                    }
                }
            }
            else
            {
                projectIds.Add(projectId);
            }

            return LoadProjectTimeBlockForTimeSheet(fromDate, toDate, employeeId, selectedEmployees: null, projects: projectIds);
        }

        private ProjectTimeBlockDTO SetProjectInvoiceWeekValues(ProjectInvoiceWeek piw, ProjectInvoiceDay pid, TimeCodeTransaction tct, IEnumerable<CustomerInvoice> invoices, List<AttestStateDTO> attestStates = null, bool incInternOrderText = false, int customerInvoiceRowId = 0)
        {
            CustomerInvoice invoice = invoices == null ? null : invoices.FirstOrDefault(i => i.InvoiceId == piw.RecordId);

            var dto = new ProjectTimeBlockDTO
            {
                //Week
                ProjectTimeBlockId = pid.ProjectInvoiceDayId,
                EmployeeId = piw.EmployeeId,
                EmployeeName = piw.Employee.ContactPerson.FirstName + " " + piw.Employee.ContactPerson.LastName,
                EmployeeNr = piw.Employee.EmployeeNr,
                EmployeeIsInactive = piw.Employee.State != (int)SoeEntityState.Active,
                ProjectId = piw.ProjectId,
                CustomerInvoiceId = tct.CustomerInvoiceId.HasValue ? tct.CustomerInvoiceId.Value : piw.RecordId,
                TimeCodeId = tct.TimeCodeId,
                //Day
                Date = pid.Date,
                StartTime = CalendarUtility.DATETIME_DEFAULT,
                StopTime = CalendarUtility.DATETIME_DEFAULT,
                //TimeCodeTransaction
                InternalNote = tct.Comment ?? string.Empty,
                ExternalNote = tct.ExternalComment ?? string.Empty,
                ProjectInvoiceWeekId = piw.ProjectInvoiceWeekId,
                TimeCodeName = tct.TimeCode?.Name,
                TimePayrollQuantity = tct.Quantity,
                IsPayrollEditable = true,
                IsEditable = true
            };

            if (piw.Project != null)
            {
                if (piw.Project.State == (int)SoeEntityState.Deleted)
                {
                    dto.ProjectName = GetText(2199);
                    dto.ProjectId = 0;
                }
                else
                {
                    dto.AllocationType = (TermGroup_ProjectAllocationType)piw.Project.AllocationType;
                    dto.CustomerId = piw.Project.CustomerId.HasValue ? piw.Project.CustomerId.Value : 0;
                    dto.CustomerName = piw.Project.Customer != null ? piw.Project.Customer.Name : string.Empty;
                    dto.ProjectNr = piw.Project.Number;
                    dto.ProjectName = piw.Project.Name;
                }
            }

            var timeInvoiceTransactions = tct.TimeInvoiceTransaction?.Where(t => t.State == (int)SoeEntityState.Active);

            if (!timeInvoiceTransactions.IsNullOrEmpty())
            {
                if (customerInvoiceRowId > 0)
                {
                    timeInvoiceTransactions = timeInvoiceTransactions.Where(t => t.CustomerInvoiceRowId == customerInvoiceRowId).ToList();
                }

                foreach (var transaction in timeInvoiceTransactions)
                {
                    dto.InvoiceQuantity += transaction.InvoiceQuantity;

                    if (transaction.CustomerInvoiceRowId.HasValue && dto.CustomerInvoiceRowAttestStateId != 1)
                    {
                        if (!transaction.CustomerInvoiceRowReference.IsLoaded)
                            transaction.CustomerInvoiceRowReference.Load();

                        if (transaction.CustomerInvoiceRow.State == (int)SoeEntityState.Active)
                        {
                            if (transaction.CustomerInvoiceRow.AttestStateId.HasValue)
                            {
                                if (dto.CustomerInvoiceRowAttestStateId == 0)
                                {
                                    dto.CustomerInvoiceRowAttestStateId = transaction.CustomerInvoiceRow.AttestStateId.Value;
                                }
                                else if (transaction.CustomerInvoiceRow.AttestStateId.Value != dto.CustomerInvoiceRowAttestStateId)
                                {
                                    dto.CustomerInvoiceRowAttestStateId = 1;
                                    dto.CustomerInvoiceRowAttestStateColor = "#402d12";
                                    dto.CustomerInvoiceRowAttestStateName = GetText(7782, "Kopplad till mer än en orderrad");
                                }
                            }
                        }
                    }
                    else if (!transaction.CustomerInvoiceRowId.HasValue && dto.InvoiceQuantity != 0)
                    {
                        dto.CustomerInvoiceRowAttestStateName = GetText(7720, "Artikelrad saknas");
                    }
                }
            }

            if (!tct.TimePayrollTransaction.IsNullOrEmpty())
            {
                var tpt = tct.TimePayrollTransaction.First();
                dto.TimePayrollQuantity = tpt.Quantity;
                dto.TimePayrollAttestStateId = tpt.AttestStateId;
            }

            //Set date columns
            dto.YearMonth = pid.Date.Year.ToString() + "-" + pid.Date.Month.ToString();
            dto.YearWeek = pid.Date.Year.ToString() + "-" + CalendarUtility.GetWeekNr(pid.Date).ToString();
            dto.Year = pid.Date.Year.ToString();
            dto.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(pid.Date.Month);
            dto.Week = CalendarUtility.GetWeekNr(pid.Date).ToString();
            dto.WeekDay = CalendarUtility.GetDayName(pid.Date, CultureInfo.CurrentCulture, true);
            dto.HasComment = (dto.InternalNote.Trim() != string.Empty || dto.ExternalNote.Trim() != string.Empty);

            if (attestStates != null && dto.CustomerInvoiceRowAttestStateId > 0)
            {
                var attestState = attestStates.FirstOrDefault(x => x.AttestStateId == dto.CustomerInvoiceRowAttestStateId);
                if (attestState != null)
                {
                    dto.CustomerInvoiceRowAttestStateColor = attestState.Color;
                    dto.CustomerInvoiceRowAttestStateName = attestState.Name;
                    dto.ShowInvoiceRowAttestState = true;
                    if (dto.IsEditable)
                    {
                        dto.IsEditable = attestState.Initial;
                    }
                }
            }

            // Invoice valus
            if (invoice != null && piw.Project != null)
            {
                //dto.IsEditable = !(invoice.Origin != null ? (invoice.Origin.Status == (int)SoeOriginStatus.OrderClosed || invoice.Origin.Status == (int)SoeOriginStatus.OrderFullyInvoice || (piw.Project.Status != (int)TermGroup_ProjectStatus.Active && piw.Project.Status != (int)TermGroup_ProjectStatus.Hidden)) : (piw.Project.Status != (int)TermGroup_ProjectStatus.Active && piw.Project.Status != (int)TermGroup_ProjectStatus.Hidden));

                if (invoice.Origin != null)
                {
                    dto.IsEditable = (invoice.Origin.Status != (int)SoeOriginStatus.OrderClosed && invoice.Origin.Status != (int)SoeOriginStatus.OrderFullyInvoice);
                }

                if (dto.IsEditable && invoice.OrderType != (int)TermGroup_OrderType.Internal)
                {
                    dto.IsEditable = (piw.Project.Status == (int)TermGroup_ProjectStatus.Active || piw.Project.Status == (int)TermGroup_ProjectStatus.Hidden || piw.Project.Status == (int)TermGroup_ProjectStatus.Guarantee);
                }

                dto.InvoiceNr = !string.IsNullOrEmpty(invoice.InvoiceNr) ? invoice.InvoiceNr : (invoice.SeqNr.HasValue ? invoice.SeqNr.Value.ToString() : string.Empty);
                dto.OrderClosed = invoice.Origin != null && (invoice.Origin.Status == (int)SoeOriginStatus.OrderClosed || invoice.Origin.Status == (int)SoeOriginStatus.OrderFullyInvoice);
                dto.RegistrationType = (OrderInvoiceRegistrationType)invoice.RegistrationType;
                dto.ReferenceOur = invoice.ReferenceOur;
                if (incInternOrderText)
                {
                    dto.InternOrderText = invoice.Origin?.Description;
                }
                if (invoice.Actor != null && invoice.Actor.Customer != null)
                {
                    dto.CustomerName = invoice.Actor.Customer.Name;
                    dto.CustomerId = invoice.Actor.Customer.ActorCustomerId;
                }
            }

            if (dto.IsEditable && dto.InvoiceQuantity != pid.InvoiceTimeInMinutes)
            {
                // Handle quantity mismatch
                dto.CustomerInvoiceRowAttestStateName = string.Format(GetText(7781, "Fakturerad tid skiljer sig från ordern {0} <> {1}"), dto.InvoiceQuantity, pid.InvoiceTimeInMinutes);
                dto.CustomerInvoiceRowAttestStateId = 0;
                dto.CustomerInvoiceRowAttestStateColor = null;
            }

            if (attestStates != null && dto.TimePayrollAttestStateId > 0)
            {
                var attestState = attestStates.FirstOrDefault(x => x.AttestStateId == dto.TimePayrollAttestStateId);
                if (attestState != null)
                {
                    dto.CustomerInvoiceRowAttestStateColor = attestState.Color;
                    dto.CustomerInvoiceRowAttestStateName = attestState.Name;
                    dto.ShowPayrollAttestState = true;
                }
            }

            return dto;
        }

        private ProjectTimeBlockDTO SetProjectInvoiceWeekEmptyValues(ProjectInvoiceWeek piw, ProjectInvoiceDay pid, IEnumerable<CustomerInvoice> invoices, bool incInternOrderText = false)
        {
            CustomerInvoice invoice = invoices == null ? null : invoices.FirstOrDefault(i => i.InvoiceId == piw.RecordId);

            var dto = new ProjectTimeBlockDTO
            {
                //Week
                ProjectTimeBlockId = pid.ProjectInvoiceDayId,
                EmployeeId = piw.EmployeeId,
                EmployeeName = piw.Employee.ContactPerson.FirstName + " " + piw.Employee.ContactPerson.LastName,
                EmployeeNr = piw.Employee.EmployeeNr,
                EmployeeIsInactive = piw.Employee.State != (int)SoeEntityState.Active,
                ProjectId = piw.ProjectId,
                CustomerInvoiceId = piw.RecordId,
                TimeCodeId = piw.TimeCode?.TimeCodeId ?? 0,
                //Day
                Date = pid.Date,
                StartTime = CalendarUtility.DATETIME_DEFAULT,
                StopTime = CalendarUtility.DATETIME_DEFAULT,
                //TimeCodeTransaction
                InternalNote = string.Empty,
                ExternalNote = string.Empty,
                ProjectInvoiceWeekId = piw.ProjectInvoiceWeekId,
                TimeCodeName = piw.TimeCode?.Name,
                TimePayrollQuantity = 0,
                IsPayrollEditable = true,
                IsEditable = true
            };

            if (piw.Project != null)
            {
                if (piw.Project.State == (int)SoeEntityState.Deleted)
                {
                    dto.ProjectName = GetText(2199);
                    dto.ProjectId = 0;
                }
                else
                {
                    dto.AllocationType = (TermGroup_ProjectAllocationType)piw.Project.AllocationType;
                    dto.CustomerId = piw.Project.CustomerId.HasValue ? piw.Project.CustomerId.Value : 0;
                    dto.CustomerName = piw.Project.Customer != null ? piw.Project.Customer.Name : string.Empty;
                    dto.ProjectNr = piw.Project.Number;
                    dto.ProjectName = piw.Project.Name;
                }
            }

            //Set date columns
            dto.YearMonth = pid.Date.Year.ToString() + "-" + pid.Date.Month.ToString();
            dto.YearWeek = pid.Date.Year.ToString() + "-" + CalendarUtility.GetWeekNr(pid.Date).ToString();
            dto.Year = pid.Date.Year.ToString();
            dto.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(pid.Date.Month);
            dto.Week = CalendarUtility.GetWeekNr(pid.Date).ToString();
            dto.WeekDay = CalendarUtility.GetDayName(pid.Date, CultureInfo.CurrentCulture, true);
            dto.HasComment = (dto.InternalNote.Trim() != string.Empty || dto.ExternalNote.Trim() != string.Empty);

            // Invoice valus
            if (invoice != null && piw.Project != null)
            {
                if (invoice.Origin != null)
                {
                    dto.IsEditable = (invoice.Origin.Status != (int)SoeOriginStatus.OrderClosed && invoice.Origin.Status != (int)SoeOriginStatus.OrderFullyInvoice);
                }

                if (dto.IsEditable && invoice.OrderType != (int)TermGroup_OrderType.Internal)
                {
                    dto.IsEditable = (piw.Project.Status == (int)TermGroup_ProjectStatus.Active || piw.Project.Status == (int)TermGroup_ProjectStatus.Hidden || piw.Project.Status == (int)TermGroup_ProjectStatus.Guarantee);
                }

                dto.InvoiceNr = !string.IsNullOrEmpty(invoice.InvoiceNr) ? invoice.InvoiceNr : (invoice.SeqNr.HasValue ? invoice.SeqNr.Value.ToString() : string.Empty);
                dto.OrderClosed = invoice.Origin != null && (invoice.Origin.Status == (int)SoeOriginStatus.OrderClosed || invoice.Origin.Status == (int)SoeOriginStatus.OrderFullyInvoice);
                dto.RegistrationType = (OrderInvoiceRegistrationType)invoice.RegistrationType;
                dto.ReferenceOur = invoice.ReferenceOur;
                if (incInternOrderText)
                {
                    dto.InternOrderText = invoice.Origin?.Description;
                }
                if (invoice.Actor != null && invoice.Actor.Customer != null)
                {
                    dto.CustomerName = invoice.Actor.Customer.Name;
                    dto.CustomerId = invoice.Actor.Customer.ActorCustomerId;
                }
            }

            return dto;
        }

        private ProjectTimeBlockDTO SetTimeSheetWeekValues(ProjectTimeSheetWeekView timeSheet)
        {
            // Create new dto
            ProjectTimeBlockDTO dto = new ProjectTimeBlockDTO()
            {
                //Week
                EmployeeId = timeSheet.EmployeeId,
                EmployeeName = timeSheet.EmployeeName,
                EmployeeNr = timeSheet.EmployeeNr,
                ProjectId = timeSheet.ProjectId.HasValue ? timeSheet.ProjectId.Value : 0,
                TimeCodeId = timeSheet.TimeCodeId,
                //Day
                Date = timeSheet.TimeCodeTransactionStart,
                StartTime = CalendarUtility.DATETIME_DEFAULT,
                StopTime = CalendarUtility.DATETIME_DEFAULT,
                //TimeCodeTransaction
                InternalNote = timeSheet.TimeCodeTransactionComment,
                ExternalNote = timeSheet.TimeCodeTransactionExternalComment,
                ProjectNr = timeSheet.ProjectNumber,
                ProjectName = timeSheet.ProjectName,
                AllocationType = timeSheet.AllocationType.HasValue ? (TermGroup_ProjectAllocationType)timeSheet.AllocationType.Value : TermGroup_ProjectAllocationType.Unknown,
                CustomerId = timeSheet.ProjectCustomerId.HasValue ? timeSheet.ProjectCustomerId.Value : 0,
                CustomerName = timeSheet.ProjectCustomerName,
                TimeCodeName = timeSheet.TimeCodeName,
                IsEditable = !(timeSheet.ProjectStatus != (int)TermGroup_ProjectStatus.Active && timeSheet.ProjectStatus != (int)TermGroup_ProjectStatus.Guarantee && (timeSheet.TimeCodeTransactionCustomerInvoiceId != 0 || timeSheet.TimeCodeTransactionSupplierInvoiceId != 0)),
                IsPayrollEditable = true,
                TimeSheetWeekId = timeSheet.TimeSheetWeekId,
                InvoiceQuantity = timeSheet.InvoiceQuantity
            };

            dto.CustomerInvoiceRowAttestStateId = timeSheet.InvoiceRowMaxAttestStateId > 0 ? timeSheet.InvoiceRowMaxAttestStateId : timeSheet.InvoiceRowMinAttestStateId;
            dto.TimePayrollQuantity = timeSheet.TimePayrollTransactionQuantity;
            dto.TimePayrollAttestStateId = timeSheet.TimePayrollTransactionAttestStateId.HasValue ? timeSheet.TimePayrollTransactionAttestStateId.Value : 0;

            //Set date columns
            var year = timeSheet.TimeCodeTransactionStart.Year;
            dto.YearMonth = year.ToString() + "-" + timeSheet.TimeCodeTransactionStart.Month.ToString();
            dto.YearWeek = year.ToString() + "-" + CalendarUtility.GetWeekNr(timeSheet.TimeCodeTransactionStart).ToString();
            dto.Year = year.ToString();
            dto.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(timeSheet.TimeCodeTransactionStart.Month);
            dto.Week = CalendarUtility.GetWeekNr(timeSheet.TimeCodeTransactionStart).ToString();
            dto.WeekDay = CalendarUtility.GetDayName(timeSheet.TimeCodeTransactionStart, CultureInfo.CurrentCulture, true);
            dto.HasComment = !string.IsNullOrEmpty(dto.InternalNote) || !string.IsNullOrEmpty(dto.ExternalNote);

            return dto;
        }

        #endregion

        #region Schedule

        public TimeSheetScheduleDTO LoadTimeSheetSchedule(int employeeId, DateTime startDate, DateTime stopDate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeScheduleTemplateBlock.NoTracking();
            List<TimeScheduleTemplateBlock> blocks = (from tb in entities.TimeScheduleTemplateBlock
                                                        .Include("TimeScheduleTemplatePeriod")
                                                      where tb.EmployeeId == employeeId &&
                                                      tb.StartTime != tb.StopTime &&
                                                      tb.Date >= startDate && tb.Date <= stopDate &&
                                                      tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule &&
                                                      tb.State == (int)SoeEntityState.Active
                                                      select tb).ToList();

            blocks = blocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();

            TimeSheetScheduleDTO dto = new TimeSheetScheduleDTO()
            {
                WeekStartDate = startDate
            };

            foreach (TimeScheduleTemplateBlock block in blocks)
            {
                DateTime startTime = new DateTime(block.Date.Value.Year, block.Date.Value.Month, block.Date.Value.Day, block.StartTime.Hour, block.StartTime.Minute, block.StartTime.Second);
                DateTime stopTime = new DateTime(block.Date.Value.Year, block.Date.Value.Month, block.Date.Value.Day, block.StopTime.Hour, block.StopTime.Minute, block.StopTime.Second);
                TimeSpan length = stopTime - startTime;
                if (length.TotalMinutes == 0)
                    continue;

                if (block.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.NormalBreak)
                    length = -length;

                if (startTime.Date == startDate.Date)
                    dto.Monday += length;
                if (startTime.Date == startDate.AddDays(1).Date)
                    dto.Tuesday += length;
                if (startTime.Date == startDate.AddDays(2).Date)
                    dto.Wednesday += length;
                if (startTime.Date == startDate.AddDays(3).Date)
                    dto.Thursday += length;
                if (startTime.Date == startDate.AddDays(4).Date)
                    dto.Friday += length;
                if (startTime.Date == startDate.AddDays(5).Date)
                    dto.Saturday += length;
                if (startTime.Date == startDate.AddDays(6).Date)
                    dto.Sunday += length;
            }

            return dto;
        }

        public EmployeeScheduleTransactionInfoDTO LoadEmployeeScheduleAndTransactionInfo(int employeeId, DateTime date)
        {
            var actorCompanyId = base.ActorCompanyId;

            //Get employee
            Employee employee = EmployeeManager.GetEmployee(employeeId, actorCompanyId);

            //Get employee group
            EmployeeGroup group = EmployeeManager.GetEmployeeGroupForEmployee(employeeId, actorCompanyId, date);

            //Get time codes
            List<TimeCode> timeCodes = TimeCodeManager.GetTimeCodes(actorCompanyId);

            //Get deviation causes
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<TimeDeviationCause> timeDeviationCauses = base.GetTimeDeviationCausesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));

            //Get TimeBlocks
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeBlock.NoTracking();
            var timeBlocks = (from tb in entities.TimeBlock
                              where tb.EmployeeId == employeeId &&
                                     tb.TimeBlockDate.Date == date.Date &&
                                     tb.TimeBlockDate.ActorCompanyId == actorCompanyId &&
                                     tb.State == (int)SoeEntityState.Active
                              orderby tb.StartTime
                              select new
                              {
                                  TimeBlockDateId = tb.TimeBlockDateId,
                                  StartTime = tb.StartTime,
                                  StopTime = tb.StopTime,
                                  TimeDeviationCauseStartId = tb.TimeDeviationCauseStartId,
                                  Comment = tb.Comment
                              }).ToList();

            //Get ProjectTimeBlocks
            List<ProjectTimeBlock> projectTimeBlocks = null;
            if (timeBlocks.Any())
                projectTimeBlocks = GetProjectTimeBlocksWithOrderAndProject(timeBlocks.First().TimeBlockDateId);

            //Check schedule
            IEnumerable<TimeScheduleTemplateBlock> timeScheduleTemplateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(employeeId, date, true).OrderBy(t => t.StartTime);

            var dto = new EmployeeScheduleTransactionInfoDTO
            {
                EmployeeId = employeeId,
                EmployeeGroupId = group != null ? group.EmployeeGroupId : 0,
                AutoGenTimeAndBreakForProject = group != null ? group.AutoGenTimeAndBreakForProject : false,
                TimeDeviationCauseId = group != null && group.TimeDeviationCauseId.HasValue ? group.TimeDeviationCauseId.GetValueOrDefault() : 0,
                Date = date,
                TimeBlocks = new List<ProjectTimeBlockDTO>(),
                ScheduleBlocks = new List<ProjectTimeBlockDTO>(),
            };

            foreach (var timeBlock in timeBlocks)
            {
                var projectTimeBlock = new ProjectTimeBlockDTO
                {
                    EmployeeId = employeeId,
                    EmployeeName = employee.Name,
                    StartTime = timeBlock.StartTime,
                    StopTime = timeBlock.StopTime,
                    ExternalNote = timeBlock.Comment,
                };

                //Check for projectTimeBlock
                if (projectTimeBlocks != null)
                {
                    var prb = projectTimeBlocks.FirstOrDefault(p => p.StartTime.Hour == timeBlock.StartTime.Hour && p.StartTime.Minute == timeBlock.StartTime.Minute && p.StopTime.Hour == timeBlock.StopTime.Hour && p.StopTime.Minute == timeBlock.StopTime.Minute);
                    if (prb != null)
                    {
                        projectTimeBlock.InvoiceNr = prb.CustomerInvoice != null ? prb.CustomerInvoice.InvoiceNr : string.Empty;
                        projectTimeBlock.ProjectNr = prb.Project != null ? prb.Project.Number : string.Empty;
                        projectTimeBlock.ExternalNote = prb.ExternalNote;
                    }
                }

                if (timeBlock.TimeDeviationCauseStartId != null)
                {
                    TimeDeviationCause cause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == timeBlock.TimeDeviationCauseStartId);
                    if (cause != null)
                    {
                        projectTimeBlock.TimeDeviationCauseId = cause.TimeDeviationCauseId;
                        projectTimeBlock.TimeDeviationCauseName = cause.Name;
                    }
                }

                dto.TimeBlocks.Add(projectTimeBlock);
            }

            foreach (TimeScheduleTemplateBlock scheduleBlock in timeScheduleTemplateBlock)
            {
                var projectTimeBlock = new ProjectTimeBlockDTO
                {
                    EmployeeId = employeeId,
                    EmployeeName = employee.Name,
                    StartTime = scheduleBlock.StartTime,
                    StopTime = scheduleBlock.StopTime,
                    InternalNote = scheduleBlock.Description,
                };

                TimeCode timeCode = timeCodes.FirstOrDefault(t => t.TimeCodeId == scheduleBlock.TimeCodeId);
                if (timeCode != null)
                {
                    projectTimeBlock.TimeCodeId = timeCode.TimeCodeId;
                    projectTimeBlock.TimeCodeName = timeCode.Name;
                }

                if (scheduleBlock.TimeDeviationCauseId != null)
                {
                    TimeDeviationCause cause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == scheduleBlock.TimeDeviationCauseId);
                    if (cause != null)
                    {
                        projectTimeBlock.TimeDeviationCauseId = cause.TimeDeviationCauseId;
                        projectTimeBlock.TimeDeviationCauseName = cause.Name;
                    }
                }

                dto.ScheduleBlocks.Add(projectTimeBlock);
            }

            return dto;
        }

        #endregion

        #region Save

        public ActionResult SaveTimeSheet(List<TimeSheetDTO> items, int employeeId, int actorCompanyId, int roleId)
        {
            ActionResult result = new ActionResult(true);

            bool workedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_WorkedTime, Permission.Readonly, roleId, actorCompanyId);
            bool invoicedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, roleId, actorCompanyId);
            bool projectInvoiceEditPermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_Edit, Permission.Modify, roleId, actorCompanyId);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (var item in items)
                        {
                            result = CreateTransactionsForTimeSheetRegistration(transaction, entities, workedTimePermission, invoicedTimePermission, projectInvoiceEditPermission, item, employeeId, actorCompanyId);
                            if (!result.Success)
                                return result;

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;
                        }

                        // Commit transaction
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
            }

            return result;
        }

        private ActionResult CreateTransactionsForTimeSheetRegistration(TransactionScope transaction, CompEntities entities, bool workedTimePermission, bool invoicedTimePermission, bool projectInvoiceEditPermission, TimeSheetDTO item, int employeeId, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            try
            {
                #region Prereq

                int accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupIncome, 0, actorCompanyId, 0);
                if (accountId == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8332, "Standardkonto för tidavtal saknas"));

                int attestStateIdInvoiceId = AttestManager.GetInitialAttestStateId(entities, actorCompanyId, TermGroup_AttestEntity.InvoiceTime);
                if (attestStateIdInvoiceId == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(5753, "Startnivå för attestnivå med typ artikel saknas"));

                int attestStateIdPayrollId = AttestManager.GetInitialAttestStateId(entities, actorCompanyId, TermGroup_AttestEntity.PayrollTime);
                if (attestStateIdPayrollId == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(5752, "Startnivå för attestnivå med typ löneart saknas"));


                #endregion

                #region TimeCode

                TimeCode timeCode = TimeCodeManager.GetTimeCode(entities, item.TimeCodeId, actorCompanyId, true);
                if (timeCode == null)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8327, "Tidkod kunde inte hittas"));

                #endregion

                #region InvoiceProduct

                InvoiceProduct invoiceProduct = null;

                if (!timeCode.TimeCodeInvoiceProduct.IsLoaded)
                    timeCode.TimeCodeInvoiceProduct.Load();

                TimeCodeInvoiceProduct tciProduct = null;
                if (timeCode.TimeCodeInvoiceProduct != null)
                    tciProduct = timeCode.TimeCodeInvoiceProduct.FirstOrDefault();

                if (tciProduct != null)
                {
                    if (!tciProduct.InvoiceProductReference.IsLoaded)
                        tciProduct.InvoiceProductReference.Load();

                    if (tciProduct.InvoiceProduct != null)
                        invoiceProduct = tciProduct.InvoiceProduct;
                }

                #endregion

                #region Get Payrollproduct

                PayrollProduct payrollProduct = null;

                if (!timeCode.TimeCodePayrollProduct.IsLoaded)
                    timeCode.TimeCodePayrollProduct.Load();

                TimeCodePayrollProduct tcpProduct = null;
                if (timeCode.TimeCodePayrollProduct != null)
                    tcpProduct = timeCode.TimeCodePayrollProduct.FirstOrDefault();

                if (tcpProduct != null)
                {
                    if (!tcpProduct.PayrollProductReference.IsLoaded)
                        tcpProduct.PayrollProductReference.Load();

                    if (tcpProduct.PayrollProduct != null)
                        payrollProduct = tcpProduct.PayrollProduct;
                }

                #endregion

                #region Transactions

                TimeSheetWeek week = null;

                if (item.TimeSheetWeekId != 0)
                {
                    // Get existing TimeCodeTransactions for current week
                    week = GetTimeSheetWeekWithTransactions(entities, item.TimeSheetWeekId);
                    if (week != null)
                    {
                        if (item.IsDeleted)
                        {
                            #region Delete

                            // Delete all transactions
                            DeleteTimeSheetWeekTransactions(week, false);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            // Delete invoice and payroll transactions for the whole week
                            DeleteTimeSheetWeekTransactions(week, true);

                            week.Comment = item.WeekNote;
                            week.ExternalComment = item.WeekNoteExternal;
                            week.RowNr = item.RowNr;

                            // Loop through all days in week
                            for (int i = 0; i < 7; i++)
                            {
                                #region Prereq

                                int dayNbr = i + 1;
                                if (dayNbr > 6)
                                    dayNbr = 0;
                                decimal minutes = GetTimeSheetDayMinutes(item, (DayOfWeek)dayNbr);
                                string note = GetTimeSheetDayNote(item, (DayOfWeek)dayNbr, false);
                                string noteExternal = GetTimeSheetDayNote(item, (DayOfWeek)dayNbr, true);

                                DateTime date = item.WeekStartDate.AddDays(i).Date;

                                #endregion

                                #region Perform

                                // Update TimeCodeTransaction and create new invoice and payroll transactions
                                TimeCodeTransaction timeCodeTransaction = GetTimeSheetDayTransactions(week.TimeCodeTransaction.ToList(), date);
                                if (timeCodeTransaction == null)
                                {
                                    // Add TimeCodeTransaction and create new invoice and payroll transactions
                                    if (minutes != 0 || !String.IsNullOrEmpty(note) || !String.IsNullOrEmpty(noteExternal))
                                    {
                                        timeCodeTransaction = CreateTransactionsForTimeSheetRegistration(entities, null, item, date, (DayOfWeek)dayNbr, payrollProduct, employeeId, invoiceProduct != null ? invoiceProduct.ProductId : 0, accountId, attestStateIdInvoiceId, attestStateIdPayrollId, actorCompanyId, note, noteExternal);
                                        if (timeCodeTransaction != null)
                                            week.TimeCodeTransaction.Add(timeCodeTransaction);
                                    }
                                }
                                else
                                {
                                    if (minutes == 0 && String.IsNullOrEmpty(note) && String.IsNullOrEmpty(noteExternal))
                                    {
                                        // Minutes and note cleared, delete transaction
                                        ChangeEntityState(timeCodeTransaction, SoeEntityState.Deleted);
                                    }
                                    else
                                    {
                                        // Update TimeCodeTransaction and create new invoice and payroll transactions
                                        CreateTransactionsForTimeSheetRegistration(entities, timeCodeTransaction, item, date, (DayOfWeek)dayNbr, payrollProduct, employeeId, invoiceProduct != null ? invoiceProduct.ProductId : 0, accountId, attestStateIdInvoiceId, attestStateIdPayrollId, actorCompanyId, note, noteExternal);
                                    }
                                }

                                #endregion
                            }

                            #endregion
                        }
                    }
                }
                else if (item.ProjectInvoiceWeekId != null)
                {
                    if (projectInvoiceEditPermission && (workedTimePermission || invoicedTimePermission))
                    {
                        int rowId = 0;
                        bool deleteRow = false;

                        // Loop through all days in week
                        for (int i = 0; i < 7; i++)
                        {
                            #region Prereq

                            int dayNbr = i + 1;
                            if (dayNbr > 6)
                                dayNbr = 0;
                            int minutes = Decimal.ToInt32(GetTimeSheetDayMinutes(item, (DayOfWeek)dayNbr));
                            int workMinutes = Decimal.ToInt32(GetTimeSheetDayWorkMinutes(item, (DayOfWeek)dayNbr));

                            //On ProjectInvoiceDay Note is the actual external comment and vice versa, therefore the comments will be switched
                            string note = GetTimeSheetDayNote(item, (DayOfWeek)dayNbr, true);
                            string noteExternal = GetTimeSheetDayNote(item, (DayOfWeek)dayNbr, false);

                            DateTime date = item.WeekStartDate.AddDays(i).Date;

                            #endregion

                            #region Perform

                            if (item.IsDeleted)
                            {
                                result = SaveProjectInvoiceDayFromTimeSheet(transaction, entities, workedTimePermission, invoicedTimePermission, actorCompanyId, employeeId, item.InvoiceId, date, 0, 0, note, item.TimeCodeId, noteExternal);

                                if (result.Success)
                                {
                                    rowId = result.IntegerValue;
                                    deleteRow = result.BooleanValue;
                                    SaveChanges(entities, transaction);
                                }
                            }
                            else
                            {
                                result = SaveProjectInvoiceDayFromTimeSheet(transaction, entities, workedTimePermission, invoicedTimePermission, actorCompanyId, employeeId, item.InvoiceId, date, minutes, workMinutes, note, item.TimeCodeId, noteExternal);

                                if (result.Success)
                                    SaveChanges(entities, transaction);
                            }

                            #endregion
                        }

                        if (item.IsDeleted)
                        {
                            if (deleteRow)
                            {

                                result = InvoiceManager.DeleteCustomerInvoiceRow(transaction, entities, actorCompanyId, item.InvoiceId, rowId);
                            }

                            result = DeleteProjectInvoiceWeek(entities, transaction, employeeId, item.TimeCodeId, item.ProjectId, item.WeekStartDate.Date, item.InvoiceId, (int)SoeProjectRecordType.Order);

                            if (result.Success)
                                SaveChanges(entities, transaction);
                        }
                    }
                }
                else
                {
                    if (item.InvoiceId != 0)
                    {
                        if (projectInvoiceEditPermission && (workedTimePermission || invoicedTimePermission))
                        {
                            int rowId = 0;
                            bool deleteRow = false;

                            // Loop through all days in week
                            for (int i = 0; i < 7; i++)
                            {
                                #region Prereq

                                int dayNbr = i + 1;
                                if (dayNbr > 6)
                                    dayNbr = 0;
                                int minutes = Decimal.ToInt32(GetTimeSheetDayMinutes(item, (DayOfWeek)dayNbr));
                                int workMinutes = Decimal.ToInt32(GetTimeSheetDayWorkMinutes(item, (DayOfWeek)dayNbr));

                                //On ProjectInvoiceDay Note is the actual external comment and vice versa, therefore the comments will be switched
                                string note = GetTimeSheetDayNote(item, (DayOfWeek)dayNbr, true);
                                string noteExternal = GetTimeSheetDayNote(item, (DayOfWeek)dayNbr, false);

                                DateTime date = item.WeekStartDate.AddDays(i).Date;

                                #endregion

                                #region Perform

                                if (item.IsDeleted)
                                {
                                    result = SaveProjectInvoiceDayFromTimeSheet(transaction, entities, workedTimePermission, invoicedTimePermission, actorCompanyId, employeeId, item.InvoiceId, date, 0, 0, note, item.TimeCodeId, noteExternal);

                                    if (result.Success)
                                    {
                                        rowId = result.IntegerValue;
                                        deleteRow = result.BooleanValue;
                                        SaveChanges(entities, transaction);
                                    }
                                }
                                else
                                {
                                    result = SaveProjectInvoiceDayFromTimeSheet(transaction, entities, workedTimePermission, invoicedTimePermission, actorCompanyId, employeeId, item.InvoiceId, date, minutes, workMinutes, note, item.TimeCodeId, noteExternal);

                                    if (result.Success)
                                        SaveChanges(entities, transaction);
                                }

                                #endregion
                            }

                            if (item.IsDeleted)
                            {
                                if (deleteRow)
                                {

                                    result = InvoiceManager.DeleteCustomerInvoiceRow(transaction, entities, actorCompanyId, item.InvoiceId, rowId);
                                }

                                result = DeleteProjectInvoiceWeek(entities, transaction, employeeId, item.TimeCodeId, item.ProjectId, item.WeekStartDate.Date, item.InvoiceId, (int)SoeProjectRecordType.Order);

                                if (result.Success)
                                    SaveChanges(entities, transaction);
                            }
                        }
                    }
                    else
                    {
                        #region Add

                        week = new TimeSheetWeek()
                        {
                            EmployeeId = employeeId,
                            Date = item.WeekStartDate.Date,
                            RowNr = item.RowNr,
                            Comment = item.WeekNote,
                            ExternalComment = item.WeekNoteExternal
                        };
                        entities.TimeSheetWeek.AddObject(week);

                        // Loop through all days in week
                        for (int i = 0; i < 7; i++)
                        {
                            #region Prereq

                            int dayNbr = i + 1;
                            if (dayNbr > 6)
                                dayNbr = 0;
                            decimal minutes = GetTimeSheetDayMinutes(item, (DayOfWeek)dayNbr);
                            decimal workminutes = GetTimeSheetDayWorkMinutes(item, (DayOfWeek)dayNbr);
                            string note = GetTimeSheetDayNote(item, (DayOfWeek)dayNbr, false);
                            string noteExternal = GetTimeSheetDayNote(item, (DayOfWeek)dayNbr, true);

                            DateTime date = item.WeekStartDate.AddDays(i).Date;

                            #endregion

                            #region Perform

                            // Only add transactions with minutes or notes
                            if (minutes != 0 || workminutes != 0 || !String.IsNullOrEmpty(note) || !String.IsNullOrEmpty(noteExternal))
                            {
                                TimeCodeTransaction tct = CreateTransactionsForTimeSheetRegistration(entities, null, item, date, (DayOfWeek)dayNbr, payrollProduct, employeeId, invoiceProduct != null ? invoiceProduct.ProductId : 0, accountId, attestStateIdInvoiceId, attestStateIdPayrollId, actorCompanyId, note, noteExternal);
                                if (tct != null)
                                    week.TimeCodeTransaction.Add(tct);
                            }

                            #endregion
                        }

                        #endregion
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                result = new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8341, "Transaktioner kunde inte skapas"));
            }

            return result;
        }

        private TimeSheetWeek GetTimeSheetWeekWithTransactions(CompEntities entities, int timeSheetWeekId)
        {
            return (from t in entities.TimeSheetWeek.Include("TimeCodeTransaction.TimeInvoiceTransaction")
                    where t.TimeSheetWeekId == timeSheetWeekId
                    select t).FirstOrDefault();
        }

        private TimeCodeTransaction GetTimeSheetDayTransactions(List<TimeCodeTransaction> week, DateTime date)
        {
            // Get transactions for specified day
            DateTime start = CalendarUtility.GetBeginningOfDay(date);
            DateTime stop = CalendarUtility.GetEndOfDay(date);
            return week.FirstOrDefault(t => t.Start >= start && t.Stop <= stop && t.State == (int)SoeEntityState.Active);
        }

        private void DeleteTimeSheetWeekTransactions(TimeSheetWeek week, bool keepTimeCodeTransactions)
        {
            foreach (TimeCodeTransaction tct in week.TimeCodeTransaction)
            {
                if (!keepTimeCodeTransactions)
                    ChangeEntityState(tct, SoeEntityState.Deleted);

                foreach (var tit in tct.TimeInvoiceTransaction)
                {
                    ChangeEntityState(tit, SoeEntityState.Deleted);
                }

                foreach (var tpt in tct.TimePayrollTransaction)
                {
                    ChangeEntityState(tpt, SoeEntityState.Deleted);
                }
            }
        }

        private decimal GetTimeSheetDayMinutes(TimeSheetDTO item, DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return (decimal)item.Monday.TotalMinutes;
                case DayOfWeek.Tuesday:
                    return (decimal)item.Tuesday.TotalMinutes;
                case DayOfWeek.Wednesday:
                    return (decimal)item.Wednesday.TotalMinutes;
                case DayOfWeek.Thursday:
                    return (decimal)item.Thursday.TotalMinutes;
                case DayOfWeek.Friday:
                    return (decimal)item.Friday.TotalMinutes;
                case DayOfWeek.Saturday:
                    return (decimal)item.Saturday.TotalMinutes;
                case DayOfWeek.Sunday:
                    return (decimal)item.Sunday.TotalMinutes;
            }

            return 0;
        }

        private decimal GetTimeSheetDayWorkMinutes(TimeSheetDTO item, DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return (decimal)item.MondayActual.TotalMinutes;
                case DayOfWeek.Tuesday:
                    return (decimal)item.TuesdayActual.TotalMinutes;
                case DayOfWeek.Wednesday:
                    return (decimal)item.WednesdayActual.TotalMinutes;
                case DayOfWeek.Thursday:
                    return (decimal)item.ThursdayActual.TotalMinutes;
                case DayOfWeek.Friday:
                    return (decimal)item.FridayActual.TotalMinutes;
                case DayOfWeek.Saturday:
                    return (decimal)item.SaturdayActual.TotalMinutes;
                case DayOfWeek.Sunday:
                    return (decimal)item.SundayActual.TotalMinutes;
            }

            return 0;
        }

        private string GetTimeSheetDayNote(TimeSheetDTO item, DayOfWeek dayOfWeek, bool external)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return external ? item.MondayNoteExternal : item.MondayNote;
                case DayOfWeek.Tuesday:
                    return external ? item.TuesdayNoteExternal : item.TuesdayNote;
                case DayOfWeek.Wednesday:
                    return external ? item.WednesdayNoteExternal : item.WednesdayNote;
                case DayOfWeek.Thursday:
                    return external ? item.ThursdayNoteExternal : item.ThursdayNote;
                case DayOfWeek.Friday:
                    return external ? item.FridayNoteExternal : item.FridayNote;
                case DayOfWeek.Saturday:
                    return external ? item.SaturdayNoteExternal : item.SaturdayNote;
                case DayOfWeek.Sunday:
                    return external ? item.SundayNoteExternal : item.SundayNote;
            }

            return null;
        }

        private TimeCodeTransaction CreateTransactionsForTimeSheetRegistration(CompEntities entities, TimeCodeTransaction timeCodeTransaction, TimeSheetDTO item, DateTime date, DayOfWeek day, PayrollProduct payrollProduct, int employeeId, int invoiceProductId, int accountId, int attestStateIdInvoiceId, int attestStateIdPayrollId, int actorCompanyId, string note, string externalNote)
        {
            #region TimeCodeTransaction

            if (timeCodeTransaction == null)
            {
                timeCodeTransaction = new TimeCodeTransaction()
                {
                    Type = (int)TimeCodeTransactionType.TimeSheet,
                    Start = date,
                    Stop = date,
                    Comment = note,
                    ExternalComment = externalNote,
                };
                SetCreatedProperties(timeCodeTransaction);
                entities.TimeCodeTransaction.AddObject(timeCodeTransaction);
            }
            else
            {
                SetModifiedProperties(timeCodeTransaction);
            }

            timeCodeTransaction.ProjectId = item.ProjectId != 0 ? (int?)item.ProjectId : null;
            timeCodeTransaction.TimeCodeId = item.TimeCodeId;

            switch (day)
            {
                case DayOfWeek.Monday:
                    timeCodeTransaction.InvoiceQuantity = (decimal)item.Monday.TotalMinutes;
                    timeCodeTransaction.Quantity = (decimal)item.MondayActual.TotalMinutes;
                    timeCodeTransaction.Start = item.WeekStartDate;
                    timeCodeTransaction.Comment = item.MondayNote;
                    timeCodeTransaction.ExternalComment = item.MondayNoteExternal;
                    break;
                case DayOfWeek.Tuesday:
                    timeCodeTransaction.InvoiceQuantity = (decimal)item.Tuesday.TotalMinutes;
                    timeCodeTransaction.Quantity = (decimal)item.TuesdayActual.TotalMinutes;
                    timeCodeTransaction.Start = item.WeekStartDate.AddDays(1);
                    timeCodeTransaction.Comment = item.TuesdayNote;
                    timeCodeTransaction.ExternalComment = item.TuesdayNoteExternal;
                    break;
                case DayOfWeek.Wednesday:
                    timeCodeTransaction.InvoiceQuantity = (decimal)item.Wednesday.TotalMinutes;
                    timeCodeTransaction.Quantity = (decimal)item.WednesdayActual.TotalMinutes;
                    timeCodeTransaction.Start = item.WeekStartDate.AddDays(2);
                    timeCodeTransaction.Comment = item.WednesdayNote;
                    timeCodeTransaction.ExternalComment = item.WednesdayNoteExternal;
                    break;
                case DayOfWeek.Thursday:
                    timeCodeTransaction.InvoiceQuantity = (decimal)item.Thursday.TotalMinutes;
                    timeCodeTransaction.Quantity = (decimal)item.ThursdayActual.TotalMinutes;
                    timeCodeTransaction.Start = item.WeekStartDate.AddDays(3);
                    timeCodeTransaction.Comment = item.ThursdayNote;
                    timeCodeTransaction.ExternalComment = item.ThursdayNoteExternal;
                    break;
                case DayOfWeek.Friday:
                    timeCodeTransaction.InvoiceQuantity = (decimal)item.Friday.TotalMinutes;
                    timeCodeTransaction.Quantity = (decimal)item.FridayActual.TotalMinutes;
                    timeCodeTransaction.Start = item.WeekStartDate.AddDays(4);
                    timeCodeTransaction.Comment = item.FridayNote;
                    timeCodeTransaction.ExternalComment = item.FridayNoteExternal;
                    break;
                case DayOfWeek.Saturday:
                    timeCodeTransaction.InvoiceQuantity = (decimal)item.Saturday.TotalMinutes;
                    timeCodeTransaction.Quantity = (decimal)item.SaturdayActual.TotalMinutes;
                    timeCodeTransaction.Start = item.WeekStartDate.AddDays(5);
                    timeCodeTransaction.Comment = item.SaturdayNote;
                    timeCodeTransaction.ExternalComment = item.SaturdayNoteExternal;
                    break;
                case DayOfWeek.Sunday:
                    timeCodeTransaction.InvoiceQuantity = (decimal)item.Sunday.TotalMinutes;
                    timeCodeTransaction.Quantity = (decimal)item.SundayActual.TotalMinutes;
                    timeCodeTransaction.Start = item.WeekStartDate.AddDays(6);
                    timeCodeTransaction.Comment = item.SundayNote;
                    timeCodeTransaction.ExternalComment = item.SundayNoteExternal;
                    break;
            }

            #endregion

            #region TimeInvoiceTransaction

            if (invoiceProductId != 0)
            {
                TimeBlockManager.GetTimeBlockDate(entities, actorCompanyId, employeeId, date, createfNotExist: true);

                var timeInvoiceTransaction = new TimeInvoiceTransaction
                {
                    InvoiceQuantity = timeCodeTransaction.InvoiceQuantity != null ? (decimal)timeCodeTransaction.InvoiceQuantity : 0,
                    Quantity = timeCodeTransaction.InvoiceQuantity != null ? (decimal)timeCodeTransaction.InvoiceQuantity : 0,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                    EmployeeId = employeeId,
                    ProductId = invoiceProductId,
                    AccountStdId = accountId,
                    AttestStateId = attestStateIdInvoiceId,

                    //Set reference
                    TimeCodeTransaction = timeCodeTransaction,
                };
                SetCreatedProperties(timeInvoiceTransaction);
            }

            #endregion

            #region TimePayrollTransaction

            if (payrollProduct != null)
            {
                // Get or insert TimeBlockDate
                TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, actorCompanyId, employeeId, date, createfNotExist: true);

                TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction
                {
                    Quantity = timeCodeTransaction.Quantity,
                    IsPreliminary = false,
                    ManuallyAdded = false,
                    Exported = false,
                    AutoAttestFailed = false,
                    SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                    SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                    SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                    SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                    EmployeeId = employeeId,
                    AccountStdId = accountId,
                    AttestStateId = attestStateIdPayrollId,
                    TimeCodeTransaction = timeCodeTransaction,

                    //References
                    TimeBlockDate = timeBlockDate,
                    PayrollProduct = payrollProduct,
                };
                SetCreatedProperties(timePayrollTransaction);
            }

            #endregion

            return timeCodeTransaction;
        }

        #endregion

        #endregion

        #region Migrate ProjectInvoiceDays

        public ActionResult ValidateTimeCodeTransactionsAndCauses(CompEntities entities, Company company)
        {
            ActionResult result = new ActionResult();

            try
            {
                List<int> usedTimeCodeIds = GetTimeCodesUsedForProjectInvoiceWeeks(entities, company.ActorCompanyId);
                List<TimeCode> timeCodes = TimeCodeManager.GetTimeCodes(entities, company.ActorCompanyId);
                List<TimeDeviationCause> timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, company.ActorCompanyId);

                foreach (TimeCode timeCode in timeCodes.Where(t => t.Type == (int)SoeTimeCodeType.Absense && usedTimeCodeIds.Contains(t.TimeCodeId)))
                {
                    TimeDeviationCause timeDeviationCause = timeDeviationCauses.FirstOrDefault(t => t.Type == (int)TermGroup_TimeDeviationCauseType.Absence && t.TimeCodeId == timeCode.TimeCodeId);
                    if (timeDeviationCause == null)
                    {
                        timeDeviationCause = timeDeviationCauses.FirstOrDefault(t => t.Name.ToLower() == timeCode.Name.ToLower());
                        if (timeDeviationCause == null)
                        {
                            timeDeviationCause = new TimeDeviationCause()
                            {
                                Name = timeCode.Name,
                                Description = String.Empty,
                                ExtCode = "ST",
                                ImageSource = null,
                                Type = (int)TermGroup_TimeDeviationCauseType.Absence,
                                OnlyWholeDay = false,
                                ShowZeroDaysInAbsencePlanning = false,
                                IsVacation = false,
                                SpecifyChild = false,
                                Payed = true,
                                ModifiedBy = "Autocreated",

                                //Set FK
                                ActorCompanyId = company.ActorCompanyId,
                                TimeCodeId = timeCode?.TimeCodeId,
                            };
                            SetCreatedProperties(timeDeviationCause);
                            entities.TimeDeviationCause.AddObject(timeDeviationCause);
                        }
                        else
                        {
                            //Found cause of right type
                            timeDeviationCause.TimeCodeId = timeCode.TimeCodeId;
                            SetModifiedProperties(timeDeviationCause);
                        }
                    }
                }

                #region Previous version

                /*foreach (TimeCode timeCode in timeCodes.Where(t => usedTimeCodeIds.Contains(t.TimeCodeId)))
                {
                    switch (timeCode.Type)
                    {
                        case (int)SoeTimeCodeType.Work:
                            TimeDeviationCause tdcWork = timeDeviationCauses.Where(t => (t.Type == (int)TermGroup_TimeDeviationCauseType.Presence || t.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence) && t.TimeCode != null && t.TimeCode.TimeCodeId == timeCode.TimeCodeId).FirstOrDefault();
                            if (tdcWork == null)
                            {
                                tdcWork = timeDeviationCauses.Where(t => t.Name.ToLower() == timeCode.Name.ToLower()).FirstOrDefault();
                                if (tdcWork == null)
                                {
                                    //Create cause
                                    tdcWork = new TimeDeviationCause()
                                    {
                                        Company = company,
                                        Name = timeCode.Name, //TermCacheManager.Instance.GetText(1130, (int)TermGroup.General, "Standard", Thread.CurrentThread.CurrentCulture.Name, this.parameterObject),
                                        Description = String.Empty,
                                        ExtCode = "ST",
                                        ImageSource = null,
                                        Type = (int)TermGroup_TimeDeviationCauseType.Presence,
                                        OnlyWholeDay = false,
                                        ShowZeroDaysInAbsencePlanning = false,
                                        IsVacation = false,
                                        SpecifyChild = false,
                                        Payed = true,
                                        ModifiedBy = "Autocreated",
                                    };

                                    //Set time code
                                    tdcWork.TimeCode = timeCode;
                                    SetCreatedProperties(tdcWork);
                                }
                                else
                                {
                                    //Found cause of right type
                                    tdcWork.TimeCode = timeCode;
                                    SetModifiedProperties(tdcWork);
                                }
                            }
                            break;
                        case (int)SoeTimeCodeType.Absense:
                            TimeDeviationCause tdcAbsense = timeDeviationCauses.Where(t => (t.Type == (int)TermGroup_TimeDeviationCauseType.Absence || t.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence) && t.TimeCode != null && t.TimeCode.TimeCodeId == timeCode.TimeCodeId).FirstOrDefault();
                            if (tdcAbsense == null)
                            {
                                tdcAbsense = timeDeviationCauses.Where(t => t.Name.ToLower() == timeCode.Name.ToLower()).FirstOrDefault();
                                if (tdcAbsense == null)
                                {
                                    //Create cause
                                    tdcAbsense = new TimeDeviationCause()
                                    {
                                        Company = company,
                                        Name = timeCode.Name, //TermCacheManager.Instance.GetText(1130, (int)TermGroup.General, "Standard", Thread.CurrentThread.CurrentCulture.Name, this.parameterObject),
                                        Description = String.Empty,
                                        ExtCode = "ST",
                                        ImageSource = null,
                                        Type = (int)TermGroup_TimeDeviationCauseType.Absence,
                                        OnlyWholeDay = false,
                                        ShowZeroDaysInAbsencePlanning = false,
                                        IsVacation = false,
                                        SpecifyChild = false,
                                        Payed = true,
                                        ModifiedBy = "Autocreated",
                                    };

                                    //Set time code
                                    tdcAbsense.TimeCode = timeCode;
                                    SetCreatedProperties(tdcAbsense);
                                }
                                else
                                {
                                    //Found cause of right type
                                    tdcAbsense.TimeCode = timeCode;
                                    SetModifiedProperties(tdcAbsense);
                                }
                            }
                            break;
                        case (int)SoeTimeCodeType.WorkAndAbsense:
                            TimeDeviationCause tdc = timeDeviationCauses.Where(t => t.TimeCode != null && t.TimeCode.TimeCodeId == timeCode.TimeCodeId).FirstOrDefault();
                            if (tdc == null)
                            {
                                tdc = timeDeviationCauses.Where(t => t.Name.ToLower() == timeCode.Name.ToLower()).FirstOrDefault();
                                if (tdc == null)
                                {
                                    //Create cause
                                    tdc = new TimeDeviationCause()
                                    {
                                        Company = company,
                                        Name = timeCode.Name, //TermCacheManager.Instance.GetText(1130, (int)TermGroup.General, "Standard", Thread.CurrentThread.CurrentCulture.Name, this.parameterObject),
                                        Description = String.Empty,
                                        ExtCode = "ST",
                                        ImageSource = null,
                                        Type = (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence,
                                        OnlyWholeDay = false,
                                        ShowZeroDaysInAbsencePlanning = false,
                                        IsVacation = false,
                                        SpecifyChild = false,
                                        Payed = true,
                                        ModifiedBy = "Autocreated",
                                    };

                                    //Set time code
                                    tdc.TimeCode = timeCode;
                                    SetCreatedProperties(tdc);
                                }
                                else
                                {
                                    //Found cause of right type
                                    tdc.TimeCode = timeCode;
                                    SetModifiedProperties(tdc);
                                }
                            }
                            break;
                        }
                    }*/

                #endregion

                result = SaveChanges(entities);
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorMessage = ex.Message;
                result.IntegerValue = 0;
            }
            finally
            {
                entities.Connection.Close();
            }

            return result;
        }

        public ActionResult ValidateEmployeeGroupTimeDeviationCode(CompEntities entities, Company company)
        {
            ActionResult result = new ActionResult();

            try
            {
                //Load causes
                List<TimeDeviationCause> timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, company.ActorCompanyId, loadTimeCode: true);
                List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(entities, company.ActorCompanyId, true);

                if (employeeGroups.Any())
                {
                    if (!timeDeviationCauses.Any())
                    {
                        //Company has no timedeviationcauses, create new
                        TimeDeviationCause newTimeDeviationCause = new TimeDeviationCause()
                        {
                            Name = GetText(1130, (int)TermGroup.General, "Standard"),
                            Description = String.Empty,
                            ExtCode = "ST",
                            ImageSource = null,
                            Type = (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence,
                            OnlyWholeDay = false,
                            ShowZeroDaysInAbsencePlanning = false,
                            IsVacation = false,
                            SpecifyChild = false,
                            Payed = true,
                            ModifiedBy = "Autocreated",

                            //Set FK
                            ActorCompanyId = company.ActorCompanyId,
                        };
                        SetCreatedProperties(newTimeDeviationCause);
                        entities.TimeDeviationCause.AddObject(newTimeDeviationCause);

                        result = SaveChanges(entities);
                        if (!result.Success)
                            return result;

                        //Loops through all groups
                        foreach (EmployeeGroup employeeGroup in employeeGroups.Where(e => e.TimeDeviationCauseId == null))
                        {
                            EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCause = EmployeeManager.CreateEmployeeGroupTimeDeviationCause(entities, employeeGroup, newTimeDeviationCause.TimeDeviationCauseId, company.ActorCompanyId, useInTimeTerminal: true);
                            if (employeeGroupTimeDeviationCause != null)
                            {
                                employeeGroup.TimeDeviationCause = newTimeDeviationCause;
                                SetModifiedProperties(employeeGroup);
                            }
                        }
                    }
                    else
                    {
                        //Loops groups that has no default timedeviationcauseid
                        foreach (EmployeeGroup employeeGroup in employeeGroups.Where(e => e.TimeDeviationCauseId == null))
                        {
                            List<int> employeeGroupTimeDeviationCauseIds = employeeGroup.EmployeeGroupTimeDeviationCause.Where(i => i.State == (int)SoeEntityState.Active).Select(m => m.TimeDeviationCauseId).ToList();
                            TimeDeviationCause timeDeviationCause = null;

                            //Filter and find PresenceAndAbsence - mapped items
                            timeDeviationCause = timeDeviationCauses.FirstOrDefault(tdc => employeeGroupTimeDeviationCauseIds.Contains(tdc.TimeDeviationCauseId) && tdc.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence);
                            if (timeDeviationCause != null)
                            {
                                employeeGroup.TimeDeviationCause = timeDeviationCause;
                                SetModifiedProperties(employeeGroup);
                                continue;
                            }

                            //Found no Presence, looking for presence - mapped items
                            timeDeviationCause = timeDeviationCauses.FirstOrDefault(tdc => employeeGroupTimeDeviationCauseIds.Contains(tdc.TimeDeviationCauseId) && tdc.Type == (int)TermGroup_TimeDeviationCauseType.Presence);
                            if (timeDeviationCause != null)
                            {
                                employeeGroup.TimeDeviationCause = timeDeviationCause;
                                SetModifiedProperties(employeeGroup);
                                continue;
                            }

                            //Filter and find PresenceAndAbsence - unmapped items
                            timeDeviationCause = timeDeviationCauses.FirstOrDefault(tdc => tdc.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence);
                            if (timeDeviationCause != null)
                            {
                                EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCausePresenceAndAbsence = EmployeeManager.CreateEmployeeGroupTimeDeviationCause(entities, employeeGroup, timeDeviationCause.TimeDeviationCauseId, company.ActorCompanyId, useInTimeTerminal: true);
                                if (employeeGroupTimeDeviationCausePresenceAndAbsence != null)
                                {
                                    employeeGroup.TimeDeviationCause = timeDeviationCause;
                                    SetModifiedProperties(employeeGroup);
                                }
                                continue;
                            }

                            //Found no PresenceAndAbsence, looking for presence - mapped items
                            timeDeviationCause = timeDeviationCauses.FirstOrDefault(d => d.Type == (int)TermGroup_TimeDeviationCauseType.Presence);
                            if (timeDeviationCause != null)
                            {
                                EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCausePresence = EmployeeManager.CreateEmployeeGroupTimeDeviationCause(entities, employeeGroup, timeDeviationCause.TimeDeviationCauseId, company.ActorCompanyId, useInTimeTerminal: true);
                                if (employeeGroupTimeDeviationCausePresence != null)
                                {
                                    employeeGroup.TimeDeviationCause = timeDeviationCause;
                                    SetModifiedProperties(employeeGroup);
                                }
                                continue;
                            }

                            //Found no matching, creating new
                            timeDeviationCause = new TimeDeviationCause()
                            {
                                Name = GetText(1130, (int)TermGroup.General, "Standard"),
                                Description = String.Empty,
                                ExtCode = "ST",
                                ImageSource = null,
                                Type = (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence,
                                OnlyWholeDay = false,
                                ShowZeroDaysInAbsencePlanning = false,
                                IsVacation = false,
                                SpecifyChild = false,
                                Payed = true,
                                ModifiedBy = "Autocreated",

                                //Set FK
                                ActorCompanyId = company.ActorCompanyId
                            };
                            SetCreatedProperties(timeDeviationCause);
                            entities.TimeDeviationCause.AddObject(timeDeviationCause);

                            result = SaveChanges(entities);
                            if (!result.Success)
                                return result;

                            EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCause = EmployeeManager.CreateEmployeeGroupTimeDeviationCause(entities, employeeGroup, timeDeviationCause.TimeDeviationCauseId, company.ActorCompanyId, useInTimeTerminal: true);
                            if (employeeGroupTimeDeviationCause != null)
                            {
                                employeeGroup.TimeDeviationCause = timeDeviationCause;
                                SetModifiedProperties(employeeGroup);
                            }
                        }
                    }

                    result = SaveChanges(entities);
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorMessage = ex.Message;
                result.IntegerValue = 0;
            }
            finally
            {
                entities.Connection.Close();
            }

            return result;
        }

        public List<int> GetTimeCodesUsedForProjectInvoiceWeeks(CompEntities entities, int actorCompanyId)
        {
            return (from t in entities.ProjectInvoiceWeek
                    where t.TimeCode.ActorCompanyId == actorCompanyId &&
                    t.TimeCodeId != null &&
                    t.State == (int)SoeEntityState.Active
                    select t).GroupBy(w => w.TimeCodeId).Select(g => (int)g.Key).ToList();
        }

        public ActionResult MigrateProjectInvoiceDaysToProjectTimeBlocks(int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            int migrated = 0;
            int errors = 0;

            int dayStartTime = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, UserId, ActorCompanyId, 0);
            int defaultTimeDeviationCause = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeDeviationCause, UserId, ActorCompanyId, 0);

            EmployeeDTO employee = null;
            //Project project = null;
            TimeBlockDateDTO timeBlockDate = null;
            TimeDeviationCauseDTO deviationCause = null;
            //CustomerInvoice customerInvoice = null;

            List<ProjectTimeBlock> projectTimeBlocks = new List<ProjectTimeBlock>();
            List<TimeCodeTransaction> timeCodeTransactions = new List<TimeCodeTransaction>();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    entities.CommandTimeout = 300;


                    //Load projectInvoiceDays transactions
                    List<TimeCodeTransaction> timeCodeTransactionsPID = (from t in entities.TimeCodeTransaction
                                                                        .Include("TimeCode")
                                                                        .Include("ProjectInvoiceDay.ProjectInvoiceWeek")
                                                                         where t.TimeCode.ActorCompanyId == actorCompanyId &&
                                                                         t.ProjectInvoiceDayId != null &&
                                                                         t.ProjectTimeBlockId == null &&
                                                                         t.State == (int)SoeEntityState.Active
                                                                         select t).ToList();

                    //Load timesheetweek transactions
                    List<TimeCodeTransaction> timeCodeTransactionsTSW = (from t in entities.TimeCodeTransaction
                                                                        .Include("TimeCode")
                                                                        .Include("TimeSheetWeek")
                                                                         where t.TimeCode.ActorCompanyId == actorCompanyId &&
                                                                         t.TimeSheetWeekId != null &&
                                                                         t.ProjectId != null &&
                                                                         t.ProjectTimeBlockId == null &&
                                                                         t.State == (int)SoeEntityState.Active
                                                                         select t).ToList();

                    if (timeCodeTransactionsPID.Count == 0 && timeCodeTransactionsTSW.Count == 0)
                    {
                        result.InfoMessage += "Företaget saknar transaktioner";
                        return result;
                    }

                    //Load causes
                    IEnumerable<TimeDeviationCauseDTO> timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, actorCompanyId, loadTimeCode: true).ToDTOs();

                    //Load Employees
                    IEnumerable<EmployeeDTO> employees = EmployeeManager.GetAllEmployees(entities, actorCompanyId, loadEmployment: true, includeDeleted: true).ToDTOs(includeEmployeeGroup: true, useLastEmployment: true);

                    //Get all employee ids
                    List<int> employeeIds = new List<int>();
                    employeeIds.AddRange(employees.Select(e => e.EmployeeId));

                    //Load TimeBlockDates
                    List<TimeBlockDateDTO> timeBlockDates = TimeBlockManager.GetTimeBlockDates(entities, actorCompanyId, employeeIds, null, null).ToDTOs().ToList();

                    //Load TimeScheduleTemplateBlocks
                    List<TimeScheduleTemplateBlockDTO> timeScheduleTemplateBlocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entities, employeeIds, true).ToDTOs();

                    #region ProjectInvoiceDay

                    foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactionsPID)
                    {
                        if (timeCodeTransaction.ProjectInvoiceDay != null)
                        {
                            if (timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek != null)
                            {
                                //Zero check
                                if (timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId == 0)
                                    continue;

                                #region Prereq

                                employee = employees.FirstOrDefault(e => e.EmployeeId == timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.EmployeeId);
                                if (employee == null)
                                {
                                    result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - anställd saknas.\n";
                                    errors++;
                                    continue;
                                }

                                timeBlockDate = timeBlockDates.FirstOrDefault(t => t.EmployeeId == employee.EmployeeId && t.Date.Date == timeCodeTransaction.ProjectInvoiceDay.Date.Date);
                                if (timeBlockDate == null)
                                {
                                    timeBlockDate = TimeBlockManager.CreateTimeBlockDate(entities, timeCodeTransaction.ProjectInvoiceDay.Date, employee).ToDTO();
                                    if (timeBlockDate != null)
                                    {
                                        timeBlockDates.Add(timeBlockDate);
                                    }
                                    else
                                    {
                                        result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timeblockdate saknas.\n";
                                        errors++;
                                        continue;
                                    }
                                }

                                /*project = projects.Where(p => p.ProjectId == timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.ProjectId).FirstOrDefault();
                                if (project == null)
                                {
                                    project = ProjectManager.GetProject(entities, timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.ProjectId);
                                    if (project != null)
                                    {
                                        projects.Add(project);
                                    }
                                    else
                                    {
                                        result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - project saknas.\n";
                                        errors++;
                                        continue;
                                    }
                                }

                                customerInvoice = customerInvoices.Where(p => p.InvoiceId == timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId).FirstOrDefault();
                                if (customerInvoice == null)
                                {
                                    customerInvoice = InvoiceManager.GetCustomerInvoice(entities, timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId);
                                    if (customerInvoice != null)
                                    {
                                        customerInvoices.Add(customerInvoice);
                                    }
                                    else
                                    {
                                        result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - customerinvoice saknas.\n";
                                        errors++;
                                        continue;
                                    }
                                }*/

                                deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeCode != null && t.TimeCode.TimeCodeId == timeCodeTransaction.TimeCodeId);
                                if (deviationCause == null)
                                {
                                    if (employee.TimeDeviationCauseId.HasValue)
                                    {
                                        deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.TimeDeviationCauseId.Value);
                                        if (deviationCause == null)
                                        {
                                            if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                            {
                                                deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                                if (deviationCause == null)
                                                {
                                                    if (defaultTimeDeviationCause > 0)
                                                    {
                                                        deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                                        if (deviationCause == null)
                                                        {
                                                            result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                            errors++;
                                                            continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                        errors++;
                                                        continue;
                                                    }
                                                }
                                            }
                                            else if (defaultTimeDeviationCause > 0)
                                            {
                                                deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                                if (deviationCause == null)
                                                {
                                                    result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                    errors++;
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                errors++;
                                                continue;
                                            }
                                        }
                                    }
                                    else if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                    {
                                        deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                        if (deviationCause == null)
                                        {
                                            if (defaultTimeDeviationCause > 0)
                                            {
                                                deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                                if (deviationCause == null)
                                                {
                                                    result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                    errors++;
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                errors++;
                                                continue;
                                            }
                                        }
                                    }
                                    else if (defaultTimeDeviationCause > 0)
                                    {
                                        deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                        if (deviationCause == null)
                                        {
                                            result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                            errors++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                        errors++;
                                        continue;
                                    }
                                }

                                #endregion

                                #region Create ProjectTimeBlock

                                Guid guid = Guid.NewGuid();
                                timeCodeTransaction.SetIdentifier(guid);

                                ProjectTimeBlock projectTimeBlock = new ProjectTimeBlock()
                                {
                                    Guid = guid,
                                    ActorCompanyId = actorCompanyId,
                                    CustomerInvoiceId = timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId,
                                    EmployeeId = employee.EmployeeId,
                                    ExternalNote = timeCodeTransaction.ExternalComment,
                                    InternalNote = timeCodeTransaction.Comment,
                                    InvoiceQuantity = timeCodeTransaction.ProjectInvoiceDay.InvoiceTimeInMinutes,
                                    ProjectId = timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.ProjectId,
                                    RecordType = timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordType,
                                    TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                                    TimeDeviationCauseId = deviationCause.TimeDeviationCauseId,
                                };

                                if (timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes > 0)
                                {
                                    ProjectTimeBlock previousProjectTimeBlock = projectTimeBlocks.Where(p => p.TimeBlockDateId == timeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();
                                    if (previousProjectTimeBlock != null)
                                    {
                                        projectTimeBlock.StartTime = previousProjectTimeBlock.StopTime;
                                        projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                    }
                                    else
                                    {
                                        //Set times from schedule
                                        //TimeScheduleTemplateBlock timeScheduleTemplateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlocks(entities, employee.EmployeeId, timeBlockDate.Date, true).OrderBy(t => t.StartTime).FirstOrDefault();
                                        TimeScheduleTemplateBlockDTO timeScheduleTemplateBlock = timeScheduleTemplateBlocks.FirstOrDefault(t => t.EmployeeId == employee.EmployeeId && t.Date == timeBlockDate.Date);
                                        if (timeScheduleTemplateBlock != null)
                                        {
                                            projectTimeBlock.StartTime = new DateTime(1900, 1, 1, timeScheduleTemplateBlock.StartTime.Hour, timeScheduleTemplateBlock.StartTime.Minute, timeScheduleTemplateBlock.StartTime.Second);
                                            projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                        }
                                        else
                                        {
                                            if (dayStartTime != 0)
                                            {
                                                projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT.AddMinutes(dayStartTime);
                                                projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                            }
                                            else
                                            {
                                                projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(7);
                                                projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT;
                                    projectTimeBlock.StopTime = CalendarUtility.DATETIME_DEFAULT;
                                }

                                //SetCreatedProperties(projectTimeBlock);

                                projectTimeBlocks.Add(projectTimeBlock);
                                timeCodeTransactions.Add(timeCodeTransaction);
                                migrated++;

                                #endregion

                            }
                        }
                    }

                    #endregion

                    #region TimeSheetWeek

                    foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactionsTSW)
                    {
                        if (!timeCodeTransaction.TimeSheetWeekReference.IsLoaded)
                            timeCodeTransaction.TimeSheetWeekReference.Load();

                        if (timeCodeTransaction.TimeSheetWeek != null)
                        {

                            #region Prereq

                            employee = employees.FirstOrDefault(e => e.EmployeeId == timeCodeTransaction.TimeSheetWeek.EmployeeId);
                            if (employee == null)
                            {
                                result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - anställd saknas.\n";
                                errors++;
                                continue;
                            }

                            timeBlockDate = timeBlockDates.FirstOrDefault(t => t.EmployeeId == employee.EmployeeId && t.Date.Date == timeCodeTransaction.Start.Date);
                            if (timeBlockDate == null)
                            {
                                timeBlockDate = TimeBlockManager.CreateTimeBlockDate(entities, timeCodeTransaction.Start.Date, employee).ToDTO();
                                if (timeBlockDate != null)
                                {
                                    timeBlockDates.Add(timeBlockDate);
                                }
                                else
                                {
                                    result.ErrorMessage += "Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timeblockdate saknas.\n";
                                    errors++;
                                    continue;
                                }
                            }

                            /*project = projects.Where(p => p.ProjectId == timeCodeTransaction.ProjectId).FirstOrDefault();
                            if (project == null)
                            {
                                project = ProjectManager.GetProject(entities, (int)timeCodeTransaction.ProjectId);
                                if (project != null)
                                {
                                    projects.Add(project);
                                }
                                else
                                {
                                    result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.TimeSheetWeekId + " - project saknas.\n";
                                    errors++;
                                    continue;
                                }
                            }

                            if (timeCodeTransaction.ProjectId != null)
                            {
                                customerInvoice = customerInvoices.Where(p => p.InvoiceId == timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId).FirstOrDefault();
                                if (customerInvoice == null)
                                {
                                    customerInvoice = InvoiceManager.GetCustomerInvoice(entities, timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId);
                                    if (customerInvoice != null)
                                    {
                                        customerInvoices.Add(customerInvoice);
                                    }
                                    else
                                    {
                                        result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.TimeSheetWeekId + " - customerinvoice saknas.\n";
                                        errors++;
                                        continue;
                                    }
                                }
                            }*/

                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeCode != null && t.TimeCode.TimeCodeId == timeCodeTransaction.TimeCodeId);
                            if (deviationCause == null)
                            {
                                if (employee.TimeDeviationCauseId.HasValue)
                                {
                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.TimeDeviationCauseId.Value);
                                    if (deviationCause == null)
                                    {
                                        if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                        {
                                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                            if (deviationCause == null)
                                            {
                                                if (defaultTimeDeviationCause > 0)
                                                {
                                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                                    if (deviationCause == null)
                                                    {
                                                        result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                        errors++;
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                    errors++;
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (defaultTimeDeviationCause > 0)
                                            {
                                                deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                                if (deviationCause == null)
                                                {
                                                    result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                    errors++;
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                errors++;
                                                continue;
                                            }
                                        }
                                    }
                                }
                                else if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                {
                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                    if (deviationCause == null)
                                    {
                                        if (defaultTimeDeviationCause > 0)
                                        {
                                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                            if (deviationCause == null)
                                            {
                                                result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                                errors++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                            errors++;
                                            continue;
                                        }
                                    }
                                }
                                else if (defaultTimeDeviationCause > 0)
                                {
                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                    if (deviationCause == null)
                                    {
                                        result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                        errors++;
                                        continue;
                                    }
                                }
                                else
                                {
                                    result.ErrorMessage += "Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.\n";
                                    errors++;
                                    continue;
                                }
                            }

                            #endregion

                            #region Create ProjectTimeBlock

                            Guid guid = Guid.NewGuid();
                            timeCodeTransaction.SetIdentifier(guid);

                            ProjectTimeBlock projectTimeBlock = new ProjectTimeBlock()
                            {
                                Guid = guid,
                                ActorCompanyId = actorCompanyId,
                                CustomerInvoiceId = null,
                                EmployeeId = employee.EmployeeId,
                                ExternalNote = timeCodeTransaction.ExternalComment,
                                InternalNote = timeCodeTransaction.Comment,
                                InvoiceQuantity = (int)timeCodeTransaction.InvoiceQuantity,
                                ProjectId = timeCodeTransaction.ProjectId,
                                RecordType = (int)SoeProjectRecordType.TimeSheet,
                                TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                                TimeDeviationCauseId = deviationCause.TimeDeviationCauseId,
                            };

                            if (timeCodeTransaction.Quantity > 0)
                            {
                                ProjectTimeBlock previousProjectTimeBlock = projectTimeBlocks.Where(p => p.TimeBlockDateId == timeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();
                                if (previousProjectTimeBlock != null)
                                {
                                    projectTimeBlock.StartTime = previousProjectTimeBlock.StopTime;
                                    projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes((int)timeCodeTransaction.Quantity);
                                }
                                else
                                {
                                    //Set times from schedule
                                    //TimeScheduleTemplateBlock timeScheduleTemplateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlocks(entities, employee.EmployeeId, timeBlockDate.Date, true).OrderBy(t => t.StartTime).FirstOrDefault();
                                    TimeScheduleTemplateBlockDTO timeScheduleTemplateBlock = timeScheduleTemplateBlocks.FirstOrDefault(t => t.EmployeeId == employee.EmployeeId && t.Date == timeBlockDate.Date);
                                    if (timeScheduleTemplateBlock != null)
                                    {
                                        projectTimeBlock.StartTime = new DateTime(1900, 1, 1, timeScheduleTemplateBlock.StartTime.Hour, timeScheduleTemplateBlock.StartTime.Minute, timeScheduleTemplateBlock.StartTime.Second);
                                        projectTimeBlock.StopTime = timeScheduleTemplateBlock.StartTime.AddMinutes((int)timeCodeTransaction.Quantity);
                                    }
                                    else
                                    {
                                        if (dayStartTime != 0)
                                        {
                                            projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT.AddMinutes(dayStartTime);
                                            projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes((int)timeCodeTransaction.Quantity);
                                        }
                                        else
                                        {
                                            projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(7);
                                            projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes((int)timeCodeTransaction.Quantity);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT;
                                projectTimeBlock.StopTime = CalendarUtility.DATETIME_DEFAULT;
                            }

                            projectTimeBlocks.Add(projectTimeBlock);
                            timeCodeTransactions.Add(timeCodeTransaction);
                            migrated++;

                            #endregion

                        }
                    }

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, TimeSpan.MaxValue))
                    {

                        //result = SaveChanges(entities, transaction, true, true);
                        result = BulkInsert(entities, projectTimeBlocks);

                        if (!result.Success)
                            return result;

                        //Set ids on timecodetransaction
                        List<TimeCodeTransaction> updatedTransactions = new List<TimeCodeTransaction>();
                        foreach (ProjectTimeBlock projectTimeBlock in projectTimeBlocks)
                        {
                            TimeCodeTransaction timeCodeTransaction = timeCodeTransactions.FirstOrDefault(t => t.Guid == projectTimeBlock.Guid);
                            if (timeCodeTransaction != null)
                            {
                                timeCodeTransaction.ProjectTimeBlockId = projectTimeBlock.ProjectTimeBlockId;
                                updatedTransactions.Add(timeCodeTransaction);
                            }
                        }

                        if (updatedTransactions.Count > 0)
                        {
                            result = BulkUpdate(entities, updatedTransactions);

                            if (!result.Success)
                                return result;
                        }

                        // Commit transaction
                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.ErrorMessage = ex.Message;
                    result.IntegerValue = 0;
                }
                finally
                {
                    result.InfoMessage += migrated.ToString() + " migrerades, " + errors.ToString() + " misslyckades";
                    entities.Connection.Close();
                }
            }

            return result;
        }



        public void MigrateProjectInvoiceDaysToProjectTimeBlocks(Guid pollingIdentifier, int actorCompanyId, ref SoeProgressInfo info, SoeMonitor monitor)
        {
            int migrated = 0;
            int errors = 0;
            var sb = new StringBuilder();

            int dayStartTime = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, UserId, ActorCompanyId, 0);
            int defaultTimeDeviationCause = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeDeviationCause, UserId, ActorCompanyId, 0);

            EmployeeDTO employee = null;
            TimeBlockDateDTO timeBlockDate = null;
            TimeDeviationCauseDTO deviationCause = null;

            List<ProjectTimeBlock> projectTimeBlocks = new List<ProjectTimeBlock>();
            List<TimeCodeTransaction> timeCodeTransactions = new List<TimeCodeTransaction>();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    entities.CommandTimeout = 300;

                    info.Message = "Startar migrering - hämtar data";

                    //Load projectInvoiceDays transactions
                    List<TimeCodeTransaction> timeCodeTransactionsPID = (from t in entities.TimeCodeTransaction
                                                                        .Include("TimeCode")
                                                                        .Include("ProjectInvoiceDay.ProjectInvoiceWeek")
                                                                         where t.TimeCode.ActorCompanyId == actorCompanyId &&
                                                                         t.ProjectInvoiceDayId != null &&
                                                                         t.ProjectTimeBlockId == null &&
                                                                         t.State == (int)SoeEntityState.Active
                                                                         select t).ToList();

                    //Load timesheetweek transactions
                    List<TimeCodeTransaction> timeCodeTransactionsTSW = (from t in entities.TimeCodeTransaction
                                                                        .Include("TimeCode")
                                                                        .Include("TimeSheetWeek")
                                                                         where t.TimeCode.ActorCompanyId == actorCompanyId &&
                                                                         t.TimeSheetWeekId != null &&
                                                                         t.ProjectId != null &&
                                                                         t.ProjectTimeBlockId == null &&
                                                                         t.State == (int)SoeEntityState.Active
                                                                         select t).ToList();

                    //Load causes
                    IEnumerable<TimeDeviationCauseDTO> timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, actorCompanyId, loadTimeCode: true).ToDTOs();

                    //Load Employees
                    IEnumerable<EmployeeDTO> employees = EmployeeManager.GetAllEmployees(entities, actorCompanyId, loadEmployment: true, includeDeleted: true).ToDTOs(includeEmployeeGroup: true, useLastEmployment: true);

                    //Get all employee ids
                    List<int> employeeIds = new List<int>();
                    employeeIds.AddRange(employees.Select(e => e.EmployeeId));

                    //Load TimeBlockDates
                    List<TimeBlockDateDTO> timeBlockDates = TimeBlockManager.GetTimeBlockDates(entities, actorCompanyId, employeeIds, null, null).ToDTOs().ToList();

                    //Load TimeScheduleTemplateBlocks
                    List<TimeScheduleTemplateBlockDTO> timeScheduleTemplateBlocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entities, employeeIds, true).ToDTOs();

                    #region ProjectInvoiceDay
                    int dayCounter = 0;
                    int dayTotal = timeCodeTransactionsPID.Count;
                    foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactionsPID)
                    {
                        dayCounter++;
                        info.Message = "Konverterar ProjectInvoiceDay-transaktion " + dayCounter.ToString() + " av " + dayTotal.ToString();

                        if (timeCodeTransaction.ProjectInvoiceDay != null && timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek != null)
                        {
                            //Zero check
                            if (timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId == 0)
                                continue;

                            #region Prereq

                            employee = employees.FirstOrDefault(e => e.EmployeeId == timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.EmployeeId);
                            if (employee == null)
                            {
                                sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - anställd saknas.");
                                errors++;
                                continue;
                            }

                            timeBlockDate = timeBlockDates.FirstOrDefault(t => t.EmployeeId == employee.EmployeeId && t.Date.Date == timeCodeTransaction.ProjectInvoiceDay.Date.Date);
                            if (timeBlockDate == null)
                            {
                                timeBlockDate = TimeBlockManager.CreateTimeBlockDate(entities, timeCodeTransaction.ProjectInvoiceDay.Date, employee).ToDTO();
                                if (timeBlockDate != null)
                                {
                                    timeBlockDates.Add(timeBlockDate);
                                }
                                else
                                {
                                    sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timeblockdate saknas.");
                                    errors++;
                                    continue;
                                }
                            }

                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeCode != null && t.TimeCode.TimeCodeId == timeCodeTransaction.TimeCodeId);
                            if (deviationCause == null)
                            {
                                if (employee.TimeDeviationCauseId.HasValue)
                                {
                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.TimeDeviationCauseId.Value);
                                    if (deviationCause == null)
                                    {
                                        if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                        {
                                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                            if (deviationCause == null)
                                            {
                                                if (defaultTimeDeviationCause > 0)
                                                {
                                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                                    if (deviationCause == null)
                                                    {
                                                        sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.");
                                                        errors++;
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.");
                                                    errors++;
                                                    continue;
                                                }
                                            }
                                        }
                                        else if (defaultTimeDeviationCause > 0)
                                        {
                                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                            if (deviationCause == null)
                                            {
                                                sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.");
                                                errors++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.");
                                            errors++;
                                            continue;
                                        }
                                    }
                                }
                                else if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                {
                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                    if (deviationCause == null)
                                    {
                                        if (defaultTimeDeviationCause > 0)
                                        {
                                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                            if (deviationCause == null)
                                            {
                                                sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.");
                                                errors++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.");
                                            errors++;
                                            continue;
                                        }
                                    }
                                }
                                else if (defaultTimeDeviationCause > 0)
                                {
                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                    if (deviationCause == null)
                                    {
                                        sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.");
                                        errors++;
                                        continue;
                                    }
                                }
                                else
                                {
                                    sb.AppendLine("Kunde ej skapa projecttimeblock för invoiceprojectday med id " + timeCodeTransaction.ProjectInvoiceDayId + " - timedeviatoncause saknas.");
                                    errors++;
                                    continue;
                                }
                            }

                            #endregion

                            #region Create ProjectTimeBlock

                            Guid guid = Guid.NewGuid();
                            timeCodeTransaction.SetIdentifier(guid);

                            ProjectTimeBlock projectTimeBlock = new ProjectTimeBlock()
                            {
                                Guid = guid,
                                ActorCompanyId = actorCompanyId,
                                CustomerInvoiceId = timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId,
                                EmployeeId = employee.EmployeeId,
                                ExternalNote = timeCodeTransaction.ExternalComment,
                                InternalNote = timeCodeTransaction.Comment,
                                InvoiceQuantity = timeCodeTransaction.ProjectInvoiceDay.InvoiceTimeInMinutes,
                                ProjectId = timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.ProjectId,
                                RecordType = timeCodeTransaction.ProjectInvoiceDay.ProjectInvoiceWeek.RecordType,
                                TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                                TimeDeviationCauseId = deviationCause.TimeDeviationCauseId,
                            };

                            if (timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes > 0)
                            {
                                ProjectTimeBlock previousProjectTimeBlock = projectTimeBlocks.Where(p => p.TimeBlockDateId == timeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();
                                if (previousProjectTimeBlock != null)
                                {
                                    projectTimeBlock.StartTime = previousProjectTimeBlock.StopTime;
                                    projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                }
                                else
                                {
                                    //Set times from schedule
                                    TimeScheduleTemplateBlockDTO timeScheduleTemplateBlock = timeScheduleTemplateBlocks.FirstOrDefault(t => t.EmployeeId == employee.EmployeeId && t.Date == timeBlockDate.Date);
                                    if (timeScheduleTemplateBlock != null)
                                    {
                                        projectTimeBlock.StartTime = new DateTime(1900, 1, 1, timeScheduleTemplateBlock.StartTime.Hour, timeScheduleTemplateBlock.StartTime.Minute, timeScheduleTemplateBlock.StartTime.Second);
                                        projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                    }
                                    else
                                    {
                                        if (dayStartTime != 0)
                                        {
                                            projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT.AddMinutes(dayStartTime);
                                            projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                        }
                                        else
                                        {
                                            projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(7);
                                            projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes(timeCodeTransaction.ProjectInvoiceDay.WorkTimeInMinutes);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT;
                                projectTimeBlock.StopTime = CalendarUtility.DATETIME_DEFAULT;
                            }

                            projectTimeBlocks.Add(projectTimeBlock);
                            timeCodeTransactions.Add(timeCodeTransaction);
                            migrated++;

                            #endregion

                        }
                    }

                    #endregion

                    #region TimeSheetWeek

                    int sheetCounter = 0;
                    int sheetTotal = timeCodeTransactionsTSW.Count;
                    foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactionsTSW)
                    {
                        sheetCounter++;
                        info.Message = "Konverterar TimeSheetWeek-transaktion " + sheetCounter.ToString() + " av " + sheetTotal.ToString();

                        if (!timeCodeTransaction.TimeSheetWeekReference.IsLoaded)
                            timeCodeTransaction.TimeSheetWeekReference.Load();

                        if (timeCodeTransaction.TimeSheetWeek != null)
                        {

                            #region Prereq

                            employee = employees.FirstOrDefault(e => e.EmployeeId == timeCodeTransaction.TimeSheetWeek.EmployeeId);
                            if (employee == null)
                            {
                                sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - anställd saknas.");
                                errors++;
                                continue;
                            }

                            timeBlockDate = timeBlockDates.FirstOrDefault(t => t.EmployeeId == employee.EmployeeId && t.Date.Date == timeCodeTransaction.Start.Date);
                            if (timeBlockDate == null)
                            {
                                timeBlockDate = TimeBlockManager.CreateTimeBlockDate(entities, timeCodeTransaction.Start.Date, employee).ToDTO();
                                if (timeBlockDate != null)
                                {
                                    timeBlockDates.Add(timeBlockDate);
                                }
                                else
                                {
                                    sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timeblockdate saknas.");
                                    errors++;
                                    continue;
                                }
                            }

                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeCode != null && t.TimeCode.TimeCodeId == timeCodeTransaction.TimeCodeId);
                            if (deviationCause == null)
                            {
                                if (employee.TimeDeviationCauseId.HasValue)
                                {
                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.TimeDeviationCauseId.Value);
                                    if (deviationCause == null)
                                    {
                                        if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                        {
                                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                            if (deviationCause == null)
                                            {
                                                if (defaultTimeDeviationCause > 0)
                                                {
                                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                                    if (deviationCause == null)
                                                    {
                                                        sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timedeviatoncause saknas.");
                                                        errors++;
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timedeviatoncause saknas.");
                                                    errors++;
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (defaultTimeDeviationCause > 0)
                                            {
                                                deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                                if (deviationCause == null)
                                                {
                                                    sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timedeviatoncause saknas.");
                                                    errors++;
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timedeviatoncause saknas.");
                                                errors++;
                                                continue;
                                            }
                                        }
                                    }
                                }
                                else if (employee.CurrentEmployeeGroupTimeDeviationCauseId.HasValue)
                                {
                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == employee.CurrentEmployeeGroupTimeDeviationCauseId.Value);
                                    if (deviationCause == null)
                                    {
                                        if (defaultTimeDeviationCause > 0)
                                        {
                                            deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                            if (deviationCause == null)
                                            {
                                                sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timedeviatoncause saknas.");
                                                errors++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timedeviatoncause saknas.");
                                            errors++;
                                            continue;
                                        }
                                    }
                                }
                                else if (defaultTimeDeviationCause > 0)
                                {
                                    deviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == defaultTimeDeviationCause);
                                    if (deviationCause == null)
                                    {
                                        sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timedeviatoncause saknas.");
                                        errors++;
                                        continue;
                                    }
                                }
                                else
                                {
                                    sb.AppendLine("Kunde ej skapa projecttimeblock för timesheetweek med id " + timeCodeTransaction.TimeSheetWeekId + " - timedeviatoncause saknas.");
                                    errors++;
                                    continue;
                                }
                            }

                            #endregion

                            #region Create ProjectTimeBlock

                            Guid guid = Guid.NewGuid();
                            timeCodeTransaction.SetIdentifier(guid);

                            ProjectTimeBlock projectTimeBlock = new ProjectTimeBlock()
                            {
                                Guid = guid,
                                ActorCompanyId = actorCompanyId,
                                CustomerInvoiceId = null,
                                EmployeeId = employee.EmployeeId,
                                ExternalNote = timeCodeTransaction.ExternalComment,
                                InternalNote = timeCodeTransaction.Comment,
                                InvoiceQuantity = (int)timeCodeTransaction.InvoiceQuantity,
                                ProjectId = timeCodeTransaction.ProjectId,
                                RecordType = (int)SoeProjectRecordType.TimeSheet,
                                TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                                TimeDeviationCauseId = deviationCause.TimeDeviationCauseId,
                            };

                            if (timeCodeTransaction.Quantity > 0)
                            {
                                ProjectTimeBlock previousProjectTimeBlock = projectTimeBlocks.Where(p => p.TimeBlockDateId == timeBlockDate.TimeBlockDateId).OrderBy(p => p.StopTime).LastOrDefault();
                                if (previousProjectTimeBlock != null)
                                {
                                    projectTimeBlock.StartTime = previousProjectTimeBlock.StopTime;
                                    projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes((int)timeCodeTransaction.Quantity);
                                }
                                else
                                {
                                    //Set times from schedule
                                    TimeScheduleTemplateBlockDTO timeScheduleTemplateBlock = timeScheduleTemplateBlocks.FirstOrDefault(t => t.EmployeeId == employee.EmployeeId && t.Date == timeBlockDate.Date);
                                    if (timeScheduleTemplateBlock != null)
                                    {
                                        projectTimeBlock.StartTime = new DateTime(1900, 1, 1, timeScheduleTemplateBlock.StartTime.Hour, timeScheduleTemplateBlock.StartTime.Minute, timeScheduleTemplateBlock.StartTime.Second);
                                        projectTimeBlock.StopTime = timeScheduleTemplateBlock.StartTime.AddMinutes((int)timeCodeTransaction.Quantity);
                                    }
                                    else
                                    {
                                        if (dayStartTime != 0)
                                        {
                                            projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT.AddMinutes(dayStartTime);
                                            projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes((int)timeCodeTransaction.Quantity);
                                        }
                                        else
                                        {
                                            projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT.AddHours(7);
                                            projectTimeBlock.StopTime = projectTimeBlock.StartTime.AddMinutes((int)timeCodeTransaction.Quantity);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                projectTimeBlock.StartTime = CalendarUtility.DATETIME_DEFAULT;
                                projectTimeBlock.StopTime = CalendarUtility.DATETIME_DEFAULT;
                            }

                            projectTimeBlocks.Add(projectTimeBlock);
                            timeCodeTransactions.Add(timeCodeTransaction);
                            migrated++;

                            #endregion

                        }
                    }

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, TimeSpan.MaxValue))
                    {
                        info.Message = "Sparar " + projectTimeBlocks.Count + " skapade projecttimeblocks";

                        ActionResult result = BulkInsert(entities, projectTimeBlocks);

                        if (result.Success)
                        {
                            //Set ids on timecodetransaction
                            int blockCounter = 0;
                            int blockTotal = timeCodeTransactionsTSW.Count;
                            List<TimeCodeTransaction> updatedTransactions = new List<TimeCodeTransaction>();
                            foreach (ProjectTimeBlock projectTimeBlock in projectTimeBlocks)
                            {
                                info.Message = "Kopplar projecttimeblocks mot transaktioner " + blockCounter.ToString() + " av " + blockTotal.ToString();

                                TimeCodeTransaction timeCodeTransaction = timeCodeTransactions.FirstOrDefault(t => t.Guid == projectTimeBlock.Guid);
                                if (timeCodeTransaction != null)
                                {
                                    timeCodeTransaction.ProjectTimeBlockId = projectTimeBlock.ProjectTimeBlockId;
                                    updatedTransactions.Add(timeCodeTransaction);
                                }
                                blockCounter++;
                            }

                            if (updatedTransactions.Count > 0)
                            {
                                info.Message = "Sparar " + updatedTransactions.Count + " uppdaterade transaktioner";

                                result = BulkUpdate(entities, updatedTransactions);

                                if (!result.Success)
                                    sb.AppendLine(result.ErrorMessage);
                            }
                        }
                        else
                        {
                            sb.AppendLine(result.ErrorMessage);
                        }

                        // Commit transaction
                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);

                    if (sb == null)
                        sb = new StringBuilder();

                    sb.AppendLine(ex.Message);
                }
                finally
                {
                    sb = sb.Insert(0, migrated.ToString() + " migrerades, " + errors.ToString() + " misslyckades");
                    monitor.AddResult(pollingIdentifier, sb.ToString());
                    info.Abort = true;
                    entities.Connection.Close();
                }
            }
        }

        #endregion

    }
}

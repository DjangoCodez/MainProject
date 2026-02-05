using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Business.Core.Reporting.Billing;
using SoftOne.Soe.Business.Util.QR;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Reporting
{
    public class BillingReportDataManager : BaseReportDataManager
    {
        #region Ctor

        public BillingReportDataManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Help-methods

        protected string CreateStockIntervalText(BillingReportParamsDTO reportParams)
        {
            string stockFromText = string.Empty;
            if (reportParams.SB_StockLocationIdFrom > 0)
            {
                Stock stockFrom = StockManager.GetStock(reportParams.SB_StockLocationIdFrom);
                stockFromText = stockFrom.Name;
            }
            string stockToText = string.Empty;
            if (reportParams.SB_StockLocationIdTo > 0)
            {
                Stock stockTo = StockManager.GetStock(reportParams.SB_StockLocationIdTo);
                stockToText = stockTo.Name;
            }
            return stockFromText + " - " + stockToText;

        }
        protected string CreateStockProductIntervalText(BillingReportParamsDTO reportParams)
        {
            string text = reportParams.SB_ProductNrFrom + "-" + reportParams.SB_ProductNrTo;
            return text;
        }
        protected string CreateStockShelfIntervalText(BillingReportParamsDTO reportParams)
        {
            string shelfFromText = String.Empty;
            if (reportParams.SB_StockShelfIdFrom > 0)
            {
                StockShelf shelfFrom = StockManager.GetStockShelf(reportParams.SB_StockShelfIdFrom);
                shelfFromText = shelfFrom.Name;
            }
            string shelfToText = String.Empty;
            if (reportParams.SB_StockShelfIdTo > 0)
            {
                StockShelf shelfTo = StockManager.GetStockShelf(reportParams.SB_StockShelfIdTo);
                shelfToText = shelfTo.Name;
            }
            return shelfFromText + " - " + shelfToText;
        }

        protected string CreateProductGroupIntervalText(BillingReportParamsDTO reportParams)
        {
            string productGroupFromText = string.Empty;
            if (!string.IsNullOrEmpty(reportParams.SB_ProductGroupFrom))
            {
                productGroupFromText = ProductGroupManager.GetProductGroup(reportParams.SB_ProductGroupFrom)?.Name ?? "";
            }
            string productGroupToText = string.Empty;
            if (!string.IsNullOrEmpty(reportParams.SB_ProductGroupTo))
            {
                productGroupToText = ProductGroupManager.GetProductGroup(reportParams.SB_ProductGroupTo)?.Name ?? "";
            }
            return productGroupFromText + " - " + productGroupToText;
        }

        
        protected string CreatePurchaseOrderIntervalText(BillingReportParamsDTO reportParams)
        {
            return reportParams.SB_PurchaseNrFrom + "-" + reportParams.SB_PurchaseNrTo;
        }
        protected string CreateSupplierIntervalText(BillingReportParamsDTO reportParams)
        {
            return reportParams.SB_ActorNrFrom + "-" + reportParams.SB_ActorNrTo;
        }

        protected string GetStockValueDate(BillingReportParamsDTO reportParams)
        {
            string text = "";
            if (reportParams.ReportResult != null)
            {
                text += reportParams.SB_StockValueDate.ToShortDateString();
            }
            return text;
        }

        protected string GetDateIntervalText(BillingReportParamsDTO reportParams)
        {
            string text = "";
            if (reportParams.ReportResult != null && reportParams.HasDateInterval)
            {
                if (reportParams.DateFrom != CalendarUtility.DATETIME_MINVALUE)
                    text += reportParams.DateFrom.ToShortDateString();
                text += "-";
                if (reportParams.DateTo != CalendarUtility.DATETIME_MAXVALUE)
                    text += reportParams.DateTo.ToShortDateString();
            }
            return text;
        }

        protected string GetBillingInvoiceSortOrderText(BillingReportParamsDTO reportParams, int termGroup)
        {
            return GetText(reportParams.SB_SortOrder, termGroup);
        }

        public XDocument CreateOrderContractChangeReportData(CreateReportResult reportResult)
        {

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "OrderContractChange");

            XElement orderContractChangeReportElement = new XElement("OrderContractChanges");

            var am = new AccountManager(parameterObject);
            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            #endregion


            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            orderContractChangeReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            var es = reportResult;
            string companyLogoPath = string.Empty;
            companyLogoPath = GetCompanyLogoFilePath(entitiesReadOnly, es.ActorCompanyId, false);
            Project project = new Project();
            var projectId = reportParams.SP_ProjectIds.FirstOrDefault();
            if (projectId > 0)
            {
                project = ProjectManager.GetProject(projectId, false, false);
            }
            XElement reportHeaderElement = CreateAccountDistributionHeadListReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(reportParams));
            reportHeaderElement.Add(reportParams.CreateEmployeeIntervalElement());
            reportHeaderElement.Add(reportParams.CreateProjectIntervalElement());
            reportHeaderElement.Add(reportParams.CreateCustomerIntervalElement());
            reportHeaderElement.Add(new XElement("ProjectNumber", project.Number));
            reportHeaderElement.Add(new XElement("ProjectName", project.Name));
            reportHeaderElement.Add(new XElement("CompanyLogo", companyLogoPath));
            orderContractChangeReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateProjectReportPageHeaderLabelsElement(pageHeaderLabelsElement, reportResult.ReportTemplateType);
            orderContractChangeReportElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Content

            using (var entities = new CompEntities())
            {
                SoeOriginType originType = SoeOriginType.Order;

                //bool includeChildProjects;
                reportParams.SB_IncludeInvoicedOrders = true;

                // Increase the command timeout
                entities.CommandTimeout = 180; // 3 minutes

                var customerInvoices = InvoiceManager.GetCustomerInvoicesFromSelection(entities, reportResult, reportParams, reportParams.SB_IncludeDrafts, ref originType, reportResult.ReportTemplateType != SoeReportTemplateType.OriginStatisticsReport, false);

                XElement invoiceElements = new XElement("InvoiceElements");
 
                foreach (var customerInvoice in customerInvoices)
                {
                    if (customerInvoice.OrderType != (int)TermGroup_OrderType.ATA)
                        continue;

                    var connectedInvoices = InvoiceManager.GetMappedInvoices(entities, customerInvoice.InvoiceId, SoeOriginInvoiceMappingType.Order, true, false);
                    var invoiceElement = CreateOrderContractChangeElement(entities, reportResult, reportParams, customerInvoice, connectedInvoices);
                    invoiceElements.Add(invoiceElement);
                }
                orderContractChangeReportElement.Add(invoiceElements);
            }
            #endregion

            #region Close document

            rootElement.Add(orderContractChangeReportElement);
            document.Add(rootElement);
            return GetValidatedDocument(document, SoeReportTemplateType.OrderContractChange);

            #endregion
        }


        /// <summary>
        /// Please use TimeProjectDataReportGenerator.cs instead!
        /// </summary>
        protected XElement CreateTimeProjectElement(CompEntities entities, CreateReportResult es, BillingReportParamsDTO reportParams, XElement timeProjectReportElement, List<TimeCode> timeCodes, int invoiceId, int actorCompanyId, bool returnProjectElement, out int nrOfTimeProjectRows)
        {
            #region Content

            XElement projectElement = null;
            int projectXmlId = 1;
            nrOfTimeProjectRows = 0;
            bool showStartStopInTimeReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowStartStopInTimeReport, this.UserId, this.ActorCompanyId, 0, false);

            if (reportParams.SB_IncludeOnlyInvoiced)
            {

                Invoice invoice = InvoiceManager.GetInvoice(entities, invoiceId);

                string invoiceNr = invoice.InvoiceNr;

                List<Project> projects = new List<Project>();
                List<CustomerInvoiceRow> invoiceRows = InvoiceManager.GetCustomerInvoiceRows(entities, invoiceId, false);

                int attestStateTransferredOrderToInvoiceId = 0;
                List<AttestTransition> attestTransitions = null;
                if (invoice.Origin.Type == (int)SoeOriginType.Order)
                {
                    attestTransitions = AttestManager.GetAttestTransitions(entities, new List<TermGroup_AttestEntity> { TermGroup_AttestEntity.Order }, SoeModule.Billing, false, actorCompanyId);
                    attestStateTransferredOrderToInvoiceId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, actorCompanyId, 0);
                }

                foreach (CustomerInvoiceRow row in invoiceRows.Where(r => r.Type == (int)SoeInvoiceRowType.ProductRow && r.IsTimeProjectRow))
                {
                    if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                    {
                        List<CustomerInvoiceRow> parentRows = InvoiceManager.GetParentCustomerInvoiceRows(entities, row.CustomerInvoiceRowId, actorCompanyId);

                        if (parentRows.Count > 0)
                        {
                            foreach (CustomerInvoiceRow parentRow in parentRows)
                            {
                                List<TimeInvoiceTransaction> trans = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, parentRow.CustomerInvoiceRowId);

                                foreach (TimeInvoiceTransaction invTransaction in trans)
                                {
                                    bool addProject = false;
                                    bool addEmployee = false;

                                    if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                                        invTransaction.TimeCodeTransactionReference.Load();

                                    if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                                        continue;

                                    Project p = projects.FirstOrDefault(pr => pr.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);

                                    if (p == null)
                                    {
                                        addProject = true;
                                        p = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
                                    }

                                    if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                                        invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

                                    if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                                        invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

                                    if ((reportParams.HasDateInterval) && !invTransaction.TimeBlockDateReference.IsLoaded)
                                        invTransaction.TimeBlockDateReference.Load();

                                    Employee e = null;

                                    if (p.Employees != null)
                                        e = p.Employees.FirstOrDefault(em => em.EmployeeId == (int)invTransaction.EmployeeId);
                                    else
                                        p.Employees = new List<Employee>();

                                    if (e == null)
                                    {
                                        if (!invTransaction.EmployeeReference.IsLoaded)
                                            invTransaction.EmployeeReference.Load();

                                        addEmployee = true;
                                        e = invTransaction.Employee;
                                    }

                                    if (e.Transactions == null)
                                        e.Transactions = new List<TimeInvoiceTransaction>();

                                    e.Transactions.Add(invTransaction);

                                    if (addEmployee)
                                        p.Employees.Add(e);

                                    if (addProject)
                                        projects.Add(p);
                                }
                            }
                        }
                        else
                        {
                            List<TimeInvoiceTransaction> trans = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, row.CustomerInvoiceRowId);

                            foreach (TimeInvoiceTransaction invTransaction in trans)
                            {
                                bool addProject = false;
                                bool addEmployee = false;

                                if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                                    invTransaction.TimeCodeTransactionReference.Load();

                                if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                                    continue;

                                Project p = projects.FirstOrDefault(pr => pr.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);

                                if (p == null)
                                {
                                    addProject = true;
                                    p = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
                                }

                                if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                                    invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

                                if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                                    invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

                                if ((reportParams.HasDateInterval) && !invTransaction.TimeBlockDateReference.IsLoaded)
                                    invTransaction.TimeBlockDateReference.Load();

                                Employee e = null;

                                if (p.Employees != null)
                                    e = p.Employees.FirstOrDefault(em => em.EmployeeId == (int)invTransaction.EmployeeId);
                                else
                                    p.Employees = new List<Employee>();

                                if (e == null)
                                {
                                    if (!invTransaction.EmployeeReference.IsLoaded)
                                        invTransaction.EmployeeReference.Load();

                                    addEmployee = true;
                                    e = invTransaction.Employee;
                                }

                                if (e.Transactions == null)
                                    e.Transactions = new List<TimeInvoiceTransaction>();

                                e.Transactions.Add(invTransaction);

                                if (addEmployee)
                                    p.Employees.Add(e);

                                if (addProject)
                                    projects.Add(p);
                            }
                        }
                    }
                    else if (invoice.Origin.Type == (int)SoeOriginType.Order)
                    {
                        var rowHasStateToInvoice = attestTransitions.IsNullOrEmpty() || attestTransitions.Any(x => x.AttestStateFromId == row.AttestStateId && x.AttestStateToId == attestStateTransferredOrderToInvoiceId);
                        if (!rowHasStateToInvoice && reportParams.SB_IncludeOnlyInvoiced)
                            continue;

                        List<TimeInvoiceTransaction> trans = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, row.CustomerInvoiceRowId);

                        foreach (TimeInvoiceTransaction invTransaction in trans)
                        {
                            bool addProject = false;
                            bool addEmployee = false;

                            if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                                invTransaction.TimeCodeTransactionReference.Load();

                            if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                                continue;

                            Project p = projects.FirstOrDefault(pr => pr.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);

                            if (p == null)
                            {
                                addProject = true;
                                p = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
                            }

                            if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                                invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

                            if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                                invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

                            if (reportParams.HasDateInterval && !invTransaction.TimeBlockDateReference.IsLoaded)
                                invTransaction.TimeBlockDateReference.Load();

                            Employee e = null;

                            if (p.Employees != null)
                                e = p.Employees.FirstOrDefault(em => em.EmployeeId == (int)invTransaction.EmployeeId);
                            else
                                p.Employees = new List<Employee>();

                            if (e == null)
                            {
                                if (!invTransaction.EmployeeReference.IsLoaded)
                                    invTransaction.EmployeeReference.Load();

                                addEmployee = true;
                                e = invTransaction.Employee;
                            }

                            if (e.Transactions == null)
                                e.Transactions = new List<TimeInvoiceTransaction>();

                            e.Transactions.Add(invTransaction);

                            if (addEmployee)
                                p.Employees.Add(e);

                            if (addProject)
                                projects.Add(p);
                        }
                    }
                }

                if (projects.Count == 0)
                {
                    bool addProject = true;
                    Project p = ProjectManager.GetProject(entities, (int)invoice.ProjectId);
                    if (addProject)
                        projects.Add(p);
                }

                foreach (Project proj in projects)
                {
                    int employeeXmlId = 1;
                    int projectInvoiceDayXmlId = 1;
                    List<XElement> employeeElements = new List<XElement>();

                    List<Employee> employees = new List<Employee>();

                    if (proj.Employees != null)
                    {
                        employees.AddRange(proj.Employees);

                        foreach (Employee emp in employees)
                        {
                            if (!emp.ContactPersonReference.IsLoaded)
                                emp.ContactPersonReference.Load();

                            XElement employeeElement = new XElement("Employee",
                                        new XAttribute("id", employeeXmlId),
                                        new XElement("EmployeeNr", emp.EmployeeNr),
                                        new XElement("EmployeeName", emp.Name));

                            nrOfTimeProjectRows = +emp.Transactions.Count;

                            List<TimeInvoiceTransaction> transactions = new List<TimeInvoiceTransaction>();
                            transactions.AddRange(emp.Transactions.Where(t => t.EmployeeId == emp.EmployeeId && t.TimeCodeTransaction?.ProjectId == proj.ProjectId));

                            if (reportParams.HasDateInterval)
                            {
                                if (reportParams.DateFrom != CalendarUtility.DATETIME_DEFAULT)
                                {
                                    transactions = transactions.Where(t => t.TimeBlockDateId != null && t.TimeBlockDateId != 0 && t.TimeBlockDate.Date >= reportParams.DateFrom).ToList();
                                }

                                if (reportParams.DateTo != CalendarUtility.DATETIME_DEFAULT)
                                {
                                    transactions = transactions.Where(t => t.TimeBlockDateId != null && t.TimeBlockDateId != 0 && t.TimeBlockDate.Date <= reportParams.DateTo).ToList();
                                }
                            }

                            foreach (var invoiceTransactions in transactions.GroupBy(x => x.TimeCodeTransactionId))
                            {
                                var trans = invoiceTransactions.First();

                                if (!trans.TimeCodeTransactionReference.IsLoaded)
                                    trans.TimeCodeTransactionReference.Load();

                                var timeCodeTransaction = trans.TimeCodeTransaction;

                                if (timeCodeTransaction == null)
                                    continue;

                                XElement dayElement = null;

                                if (timeCodeTransaction.ProjectTimeBlockId.HasValue && timeCodeTransaction.ProjectTimeBlockId > 0)
                                {
                                    var projectTimeBlock = timeCodeTransaction.ProjectTimeBlock != null ? timeCodeTransaction.ProjectTimeBlock : ProjectManager.GetProjectTimeBlock(entities, (int)timeCodeTransaction.ProjectTimeBlockId);
                                    if (projectTimeBlock != null)
                                    {
                                        dayElement = CreateProjectInvoiceDayElement(projectInvoiceDayXmlId, projectTimeBlock, timeCodes, es.ActorCompanyId, showStartStopInTimeReport);
                                    }
                                }
                                else if (trans.TimeCodeTransaction.ProjectInvoiceDay != null)
                                {
                                    dayElement = new XElement("ProjectInvoiceDay",
                                                    new XAttribute("id", projectInvoiceDayXmlId),
                                                    new XElement("TCCode", timeCodeTransaction.TimeCode != null ? timeCodeTransaction.TimeCode.Code : string.Empty),
                                                    new XElement("TCName", timeCodeTransaction.TimeCode != null ? timeCodeTransaction.TimeCode.Name : string.Empty),
                                                    new XElement("InvoiceTimeInMinutes", timeCodeTransaction.ProjectInvoiceDay.InvoiceTimeInMinutes),
                                                    new XElement("Date", timeCodeTransaction.ProjectInvoiceDay.Date.ToShortDateString()),
                                                    new XElement("Note", timeCodeTransaction.ProjectInvoiceDay.Note),
                                                    new XElement("ExternalNote", timeCodeTransaction.Comment),
                                                    new XElement("IsoDate", timeCodeTransaction.ProjectInvoiceDay.Date.ToString("yyyy-MM-dd")),
                                                    new XElement("TDName", string.Empty),
                                                    new XElement("TBStartTime", string.Empty),
                                                    new XElement("TBStopTime", string.Empty));
                                }

                                if (dayElement != null)
                                {
                                    employeeElement.Add(dayElement);
                                    projectInvoiceDayXmlId++;
                                }
                            }

                            employeeElements.Add(employeeElement);
                            employeeXmlId++;

                        }
                    }

                    if (proj.Employees == null || employeeElements.Count == 0)
                    {
                        //Add default element
                        XElement defaultEmployeeElement = new XElement("Employee",
                                new XAttribute("id", 1),
                                new XElement("EmployeeNr", 0),
                                new XElement("EmployeeName", ""));

                        XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                            new XAttribute("id", 1),
                            new XElement("InvoiceTimeInMinutes", 0),
                            new XElement("Date", "00:00"),
                            new XElement("Note", "00:00"),
                            new XElement("ExternalNote", string.Empty),
                            new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

                        defaultEmployeeElement.Add(defaultDayElement);
                        employeeElements.Add(defaultEmployeeElement);
                    }

                    projectElement = new XElement("Project",
                        new XAttribute("id", projectXmlId),
                        new XElement("ProjectNumber", proj.Number),
                        new XElement("ProjectName", proj.Name),
                        new XElement("ProjectDescription", proj.Description),
                        new XElement("ProjectInvoiceNr", invoiceNr),
                        new XElement("ProjectCreated", proj.Created.HasValue ? proj.Created.Value.ToShortDateString() : ""),
                        new XElement("ProjectCreatedBy", proj.CreatedBy),
                        new XElement("ProjectState", proj.State),
                        new XElement("ProjectWorkSiteId", proj.WorkSiteKey),
                        new XElement("ProjectWorkSiteNumber", proj.WorkSiteNumber));


                    foreach (XElement employeeElement in employeeElements)
                    {
                        projectElement.Add(employeeElement);
                    }

                    if (timeProjectReportElement == null)
                        timeProjectReportElement = new XElement(projectElement);
                    else
                        timeProjectReportElement.Add(projectElement);
                    projectXmlId++;
                }

                //Detach
                foreach (var proj in projects)
                {
                    if (proj.Employees != null)
                    {
                        foreach (var emp in proj.Employees)
                        {
                            foreach (var trans in emp.Transactions)
                                base.TryDetachEntity(entities, trans);

                            base.TryDetachEntity(entities, emp);
                        }
                    }
                    base.TryDetachEntity(entities, proj);
                }

            }
            else
            {
                bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
                DateTime? fromDate = reportParams.HasDateInterval && reportParams.DateFrom != CalendarUtility.DATETIME_DEFAULT ? reportParams.DateFrom : (DateTime?)null;
                DateTime? toDate = reportParams.HasDateInterval && reportParams.DateTo != CalendarUtility.DATETIME_DEFAULT ? reportParams.DateTo : (DateTime?)null;

                List<Project> projects = ProjectManager.GetProjectsForInvoice(entities, invoiceId);

                if (projects.Count > 0)
                {
                    var invoice = (from i in entities.Invoice
                               .Include("Origin")
                                   where i.InvoiceId == invoiceId &&
                                   i.State == (int)SoeEntityState.Active
                                   select i).OfType<CustomerInvoice>().FirstOrDefault();

                    string invoiceNr = invoice.InvoiceNr;

                    foreach (Project project in projects)
                    {
                        if (useProjectTimeBlock)
                        {
                            List<ProjectTimeBlock> projectTimeBlocks = new List<ProjectTimeBlock>();
                            if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                            {
                                List<int> connectedOrderDitinctIds = InvoiceManager.GetConnectedOrdersForCustomerInvoice(entities, invoice.InvoiceId);
                                var tempBlocks = ProjectManager.GetProjectTimeBlocksForProject(entities, project.ProjectId, null, fromDate, toDate);
                                foreach (var connectedOrderId in connectedOrderDitinctIds)
                                {
                                    if (projectTimeBlocks.Any(x => x.CustomerInvoiceId == connectedOrderId))//just to be sure
                                        continue;

                                    projectTimeBlocks.AddRange(tempBlocks.Where(x => x.CustomerInvoiceId == connectedOrderId).ToList());
                                }
                            }
                            else
                                projectTimeBlocks = ProjectManager.GetProjectTimeBlocksForProject(entities, project.ProjectId, invoiceId, fromDate, toDate);

                            nrOfTimeProjectRows += projectTimeBlocks.Count;

                            //Group the entire list by employeeId
                            List<IGrouping<int, ProjectTimeBlock>> projectTimeBlocksGroupedByEmployeeId = projectTimeBlocks.GroupBy(g => g.EmployeeId).ToList();

                            int employeeXmlId = 1;
                            var employeeElements = new List<XElement>();

                            foreach (IGrouping<int, ProjectTimeBlock> projectTimeBlockGroupedByEmployeeId in projectTimeBlocksGroupedByEmployeeId)
                            {
                                ProjectTimeBlock firstProjectTimeBlock = projectTimeBlockGroupedByEmployeeId.FirstOrDefault();
                                if (firstProjectTimeBlock == null)
                                    continue;

                                Employee employee = firstProjectTimeBlock.Employee;
                                if (employee == null)
                                    continue;

                                List<XElement> projectInvoiceDayElements = new List<XElement>();
                                int projectInvoiceDayXmlId = 1;

                                foreach (ProjectTimeBlock projectTimeBlock in projectTimeBlockGroupedByEmployeeId)
                                {
                                    if (projectTimeBlock.InvoiceQuantity == 0)
                                        continue;

                                    var dayElement = CreateProjectInvoiceDayElement(projectInvoiceDayXmlId, projectTimeBlock, timeCodes, es.ActorCompanyId, showStartStopInTimeReport);

                                    projectInvoiceDayElements.Add(dayElement);

                                    projectInvoiceDayXmlId++;
                                }

                                #region Employee

                                XElement employeeElement = new XElement("Employee",
                                    new XAttribute("id", employeeXmlId),
                                    new XElement("EmployeeNr", employee.EmployeeNr),
                                    new XElement("EmployeeName", employee.Name));

                                foreach (XElement projectInvoiceDayElement in projectInvoiceDayElements)
                                {
                                    employeeElement.Add(projectInvoiceDayElement);
                                }

                                projectInvoiceDayElements.Clear();
                                employeeElements.Add(employeeElement);

                                #endregion

                                employeeXmlId++;
                            }

                            #region Default element Employee

                            if (employeeXmlId == 1)
                            {
                                XElement defaultEmployeeElement = new XElement("Employee",
                                    new XAttribute("id", 1),
                                    new XElement("EmployeeNr", 0),
                                    new XElement("EmployeeName", ""));

                                XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                                    new XAttribute("id", 1),
                                    new XElement("InvoiceTimeInMinutes", 0),
                                    new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("Note", "00:00"),
                                    new XElement("ExternalNote", string.Empty),
                                    new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

                                defaultEmployeeElement.Add(defaultDayElement);
                                employeeElements.Add(defaultEmployeeElement);
                            }

                            #endregion

                            #region Project

                            projectElement = new XElement("Project",
                                new XAttribute("id", projectXmlId),
                                new XElement("ProjectNumber", project.Number),
                                new XElement("ProjectName", project.Name),
                                new XElement("ProjectDescription", project.Description),
                                new XElement("ProjectInvoiceNr", invoiceNr),
                                new XElement("ProjectCreated", project.Created.HasValue ? project.Created.Value.ToShortDateString() : ""),
                                new XElement("ProjectCreatedBy", project.CreatedBy),
                                new XElement("ProjectState", project.State),
                                new XElement("ProjectWorkSiteId", project.WorkSiteKey),
                                new XElement("ProjectWorkSiteNumber", project.WorkSiteNumber));

                            foreach (XElement employeeElement in employeeElements)
                            {
                                projectElement.Add(employeeElement);
                            }

                            employeeElements.Clear();

                            if (timeProjectReportElement == null)
                                timeProjectReportElement = new XElement(projectElement);
                            else
                                timeProjectReportElement.Add(projectElement);

                            #endregion

                            projectXmlId++;

                        }
                        else
                        {
                            List<ProjectInvoiceWeek> projectInvoiceWeeks = new List<ProjectInvoiceWeek>();

                            if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                            {
                                List<int> connectedOrderDitinctIds = InvoiceManager.GetConnectedOrdersForCustomerInvoice(entities, invoice.InvoiceId);
                                var tempWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId);
                                foreach (var connectedOrderId in connectedOrderDitinctIds)
                                {
                                    if (projectInvoiceWeeks.Any(x => x.RecordId == connectedOrderId))//just to be sure
                                        continue;

                                    projectInvoiceWeeks.AddRange(tempWeeks.Where(x => x.RecordId == connectedOrderId).ToList());
                                }
                            }
                            else
                                projectInvoiceWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId, invoiceId, fromDate, toDate);

                            //Group the entire list by employeeId
                            List<IGrouping<int, ProjectInvoiceWeek>> projectInvoiceWeeksGroupedByEmployeeId = projectInvoiceWeeks.GroupBy(g => g.EmployeeId).ToList();

                            int employeeXmlId = 1;
                            List<XElement> employeeElements = new List<XElement>();

                            //Each employeeProjectInvoiceWeekItems contains all ProjectInvoiceWeeks for one employee
                            foreach (IGrouping<int, ProjectInvoiceWeek> projectInvoiceWeekGroupedByEmployeeId in projectInvoiceWeeksGroupedByEmployeeId)
                            {
                                ProjectInvoiceWeek firstProjectInvoiceWeek = projectInvoiceWeekGroupedByEmployeeId.FirstOrDefault();
                                if (firstProjectInvoiceWeek == null)
                                    continue;

                                Employee employee = firstProjectInvoiceWeek.Employee;
                                if (employee == null)
                                    continue;

                                List<XElement> projectInvoiceDayElements = new List<XElement>();
                                int projectInvoiceDayXmlId = 1;

                                //foreach ProjectInvoiceWeek for the employee
                                foreach (ProjectInvoiceWeek projectInvoiceWeek in projectInvoiceWeekGroupedByEmployeeId)
                                {
                                    //projectInvoiceDays contains all ProjectInvoiceDay items in a ProjectInvoiceWeek for the employee
                                    var projectInvoiceDays = ProjectManager.GetProjectInvoiceDays(entities, projectInvoiceWeek.ProjectInvoiceWeekId, fromDate, toDate, true);
                                    TimeCode timeCode = null;
                                    if (projectInvoiceWeek.TimeCodeId.HasValue)
                                        timeCode = timeCodes.FirstOrDefault(x => x.TimeCodeId == projectInvoiceWeek.TimeCodeId.Value);

                                    nrOfTimeProjectRows += projectInvoiceDays.Count(p => p.InvoiceTimeInMinutes > 0);

                                    foreach (ProjectInvoiceDay projectInvoiceDay in projectInvoiceDays)
                                    {
                                        #region ProjectInvoiceDay

                                        var timeCodeTransaction = projectInvoiceDay.TimeCodeTransaction.FirstOrDefault(t => t.State == (int)SoeEntityState.Active && t.TimeInvoiceTransaction.Any(i => i.State == (int)SoeEntityState.Active));
                                        var invoiceTimeInMinutes = timeCodeTransaction != null && timeCodeTransaction.TimeInvoiceTransaction.Any() ? projectInvoiceDay.InvoiceTimeInMinutes : 0;

                                        if (invoiceTimeInMinutes == 0)
                                            continue;

                                        XElement dayElement = new XElement("ProjectInvoiceDay",
                                            new XAttribute("id", projectInvoiceDayXmlId),
                                            new XElement("TCCode", timeCode != null ? timeCode.Code : string.Empty),
                                            new XElement("TCName", timeCode != null ? timeCode.Name : string.Empty),
                                            new XElement("InvoiceTimeInMinutes", invoiceTimeInMinutes),
                                            new XElement("Date", projectInvoiceDay.Date.ToShortDateString()),
                                            new XElement("Note", projectInvoiceDay.Note),
                                            new XElement("ExternalNote", string.Empty),
                                            new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));
                                        projectInvoiceDayElements.Add(dayElement);

                                        #endregion

                                        projectInvoiceDayXmlId++;
                                    }
                                }

                                #region Employee

                                XElement employeeElement = new XElement("Employee",
                                    new XAttribute("id", employeeXmlId),
                                    new XElement("EmployeeNr", employee.EmployeeNr),
                                    new XElement("EmployeeName", employee.Name));

                                foreach (XElement projectInvoiceDayElement in projectInvoiceDayElements)
                                {
                                    employeeElement.Add(projectInvoiceDayElement);
                                }

                                projectInvoiceDayElements.Clear();
                                employeeElements.Add(employeeElement);

                                #endregion

                                employeeXmlId++;
                            }

                            #region Default element Employee

                            if (employeeXmlId == 1)
                            {
                                XElement defaultEmployeeElement = new XElement("Employee",
                                    new XAttribute("id", 1),
                                    new XElement("EmployeeNr", 0),
                                    new XElement("EmployeeName", ""));

                                XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                                    new XAttribute("id", 1),
                                    new XElement("InvoiceTimeInMinutes", 0),
                                    new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("Note", "00:00"),
                                    new XElement("ExternalNote", string.Empty),
                                    new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

                                defaultEmployeeElement.Add(defaultDayElement);
                                employeeElements.Add(defaultEmployeeElement);
                            }

                            #endregion

                            #region Project

                            projectElement = new XElement("Project",
                                new XAttribute("id", projectXmlId),
                                new XElement("ProjectNumber", project.Number),
                                new XElement("ProjectName", project.Name),
                                new XElement("ProjectDescription", project.Description),
                                new XElement("ProjectInvoiceNr", invoiceNr),
                                new XElement("ProjectCreated", project.Created.HasValue ? project.Created.Value.ToShortDateString() : ""),
                                new XElement("ProjectCreatedBy", project.CreatedBy),
                                new XElement("ProjectState", project.State),
                                new XElement("ProjectWorkSiteId", project.WorkSiteKey),
                                new XElement("ProjectWorkSiteNumber", project.WorkSiteNumber));

                            foreach (XElement employeeElement in employeeElements)
                            {
                                projectElement.Add(employeeElement);
                            }

                            employeeElements.Clear();

                            if (timeProjectReportElement == null)
                                timeProjectReportElement = new XElement(projectElement);
                            else
                                timeProjectReportElement.Add(projectElement);

                            #endregion

                            projectXmlId++;
                        }
                    }
                }
                else
                {
                    #region Default element Project

                    projectElement = new XElement("Project",
                        new XAttribute("id", 1),
                        new XElement("ProjectNumber", ""),
                        new XElement("ProjectName", ""),
                        new XElement("ProjectDescription", ""),
                        new XElement("ProjectInvoiceNr", ""),
                        new XElement("ProjectCreated", "00:00"),
                        new XElement("ProjectCreatedBy", ""),
                        new XElement("ProjectState", 0),
                        new XElement("ProjectWorkSiteId", ""),
                        new XElement("ProjectWorkSiteNumber", ""));

                    XElement defaultEmployeeElement = new XElement("Employee",
                        new XAttribute("id", 1),
                        new XElement("EmployeeNr", 0),
                        new XElement("EmployeeName", ""));

                    XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                        new XAttribute("id", 1),
                        new XElement("InvoiceTimeInMinutes", 0),
                        new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("Note", "00:00"));

                    defaultEmployeeElement.Add(defaultDayElement);
                    projectElement.Add(defaultEmployeeElement);
                    timeProjectReportElement.Add(projectElement);

                    #endregion
                }
            }

            #endregion

            return timeProjectReportElement;
        }

        public string CreateInvoiceQR(CompEntities entities, QRCode qrCode, string invoiceReference, string address, DateTime invoiceDate, DateTime dueDate, decimal totalAmountCurreny, decimal vatAmountCurrency, string sellerName, string currency, string orgNr, TermGroup_SysPaymentType paymentType, string paymentNr)
        {
            var path = qrCode.CreateInvoiceQR(invoiceReference, address, invoiceDate, dueDate, totalAmountCurreny, vatAmountCurrency, sellerName, currency, orgNr, paymentType, paymentNr);

            if (!SettingManager.GetBoolSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.UseCrGen, 0, 0, 0))
                return AddToCrGenRequestPicturesDTO(path);

            return path;
        }

        private PayrollProduct GetPayrollProductFromTimeCode(CompEntities entities, int timeCodeId, int actorCompanyId)
        {
            TimeCode timecode = TimeCodeManager.GetTimeCodeWithPayrollProducts(entities, timeCodeId, actorCompanyId);
            return timecode?.TimeCodePayrollProduct?.FirstOrDefault()?.PayrollProduct;
        }
        protected XElement CreateBillingReportHeaderElement(CreateReportResult reportResult, BillingReportParamsDTO reportParams, ReportDataHistoryRepository repository = null)
        {
            #region Prereq
            var es = reportResult;
            int sortOrderTermGroup;

            //From current selection
            string accountYearInterval = reportParams.GetAccountYearIntervalText();
            string accountPeriodInterval = reportParams.GetAccountPeriodIntervalText();
            string invoiceInterval = this.GetBillingInvoiceIntervalText(reportParams);
            string customerInterval = this.GetBillingCustomerIntervalText(reportParams);
            string dateInterval = this.GetBillingDateIntervalText(reportParams);
            string createdDateInterval = this.GetBillingCreatedDateIntervalText(reportParams);
            string paymentDateInterval = this.GetBillingPaymentDateIntervalText(reportParams);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            string companyLogoPath = GetCompanyLogoFilePath(entitiesReadOnly, es.ActorCompanyId, false);

            switch (es.ReportTemplateType)
            {
                case SoeReportTemplateType.BillingContract:
                    accountYearInterval = string.Empty;
                    sortOrderTermGroup = (int)TermGroup.ReportBillingOrderSortOrder;
                    break;
                case SoeReportTemplateType.BillingOffer:
                    sortOrderTermGroup = (int)TermGroup.ReportBillingOfferSortOrder;
                    break;
                case SoeReportTemplateType.BillingOrder:
                case SoeReportTemplateType.BillingOrderOverview:
                    accountYearInterval = string.Empty;
                    sortOrderTermGroup = (int)TermGroup.ReportBillingOrderSortOrder;
                    break;
                case SoeReportTemplateType.BillingInvoice:
                case SoeReportTemplateType.BillingInvoiceInterest:
                case SoeReportTemplateType.BillingInvoiceReminder:
                    sortOrderTermGroup = (int)TermGroup.ReportBillingInvoiceSortOrder;
                    break;

                //should never end up in here
                default:
                    sortOrderTermGroup = (int)TermGroup.ReportBillingInvoiceSortOrder;
                    break;
            }

            string sortOrderName = this.GetBillingInvoiceSortOrderText(reportParams, sortOrderTermGroup);

            //From current selection or repository if used
            string companyName = "", companyOrgNr = "", companyVatNr = "",
                   distributionAddress = "", distributionAddressCO = "", distributionPostalCode = "", distributionPostalAddress = "", distributionCountry = "",
                   boardHqPostalAddress = "", boardHqCountry = "",
                   email = "", phoneHome = "", phoneJob = "", phoneMobile = "", fax = "", webAddress = "",
                   bg = "", pg = "", bank = "", bicNR = "", sepa = "", bicBIC = "", bic = "";

            int paymentConditionId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest, 0, es.ActorCompanyId, 0);
            PaymentCondition paymentCondition = paymentConditionId != 0 ? PaymentManager.GetPaymentCondition(paymentConditionId, es.ActorCompanyId) : null;
            var extendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, es.ActorCompanyId, 0);
            var showStartStopInTimeReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowStartStopInTimeReport, this.UserId, this.ActorCompanyId, 0, false);
            var additionalDiscountInUse = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingUseAdditionalDiscount, this.UserId, this.ActorCompanyId, 0, false);

            #endregion
            XElement paymentInformationElement = new XElement("Paymentinformation");

            if (repository != null && repository.HasSavedHistory)
            {
                #region Repository

                //Company
                companyName = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyName);
                companyOrgNr = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyOrgnr);
                companyVatNr = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyVatNr);
                if (string.IsNullOrEmpty(companyName) && !string.IsNullOrEmpty(companyOrgNr))
                {
                    //Addresses
                    distributionAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionAddress);
                    distributionAddressCO = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionAddressCO);
                    distributionPostalCode = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionPostalCode);
                    distributionPostalAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionPostalAddress);
                    distributionCountry = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionCountry);
                    boardHqPostalAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBoardHQPostalAddress);
                    boardHqCountry = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBoardHQCountry);

                    //ECom
                    email = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyEmail);
                    phoneHome = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneHome);
                    phoneJob = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneJob);
                    phoneMobile = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneMobile);
                    fax = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyFax);
                    webAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyWebAddress);

                    //Payment
                    PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(Company.ActorCompanyId, true, false);
                    bg = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBg);
                    pg = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPg);
                    bank = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBank);
                    bic = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBic);
                    sepa = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanySepa);
                    
                    if (bic.Contains("/"))
                    {
                        bicBIC = bic.Split('/')[0];
                        bicNR = bic.Split('/')[1];

                    }
                    else
                    {
                        bicBIC = PaymentManager.GetPaymentBIC(paymentInformation, TermGroup_SysPaymentType.BIC);
                        bicNR = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                    }

                    var rows = paymentInformation.ActivePaymentInformationRows.Where(i => i.ShownInInvoice == true).ToList();
                    foreach (var row in rows)
                    {
                        XElement paymentInformationRowElement = new XElement("PaymentinformationRow");
                        paymentInformationRowElement.Add(new XElement("BIC", row.BIC));
                        if (row.CurrencyId.HasValue)
                        {
                            var currency = CountryCurrencyManager.GetCurrencyWithCode((int)row.CurrencyId);
                            paymentInformationRowElement.Add(new XElement("CurrencyCode", currency?.Code ?? ""));
                        }
                        else
                        {
                            paymentInformationRowElement.Add(new XElement("CurrencyCode", ""));
                        }
                        paymentInformationRowElement.Add(new XElement("PaymentType", (TermGroup_SysPaymentType)row.SysPaymentTypeId));
                        paymentInformationRowElement.Add(new XElement("PaymentNumber", row.PaymentNr));

                        paymentInformationElement.Add(paymentInformationRowElement);
                    }
                }
                else
                {
                    //Mandatory information missing use current
                    companyVatNr = Company.VatNr;

                    Contact contact = ContactManager.GetContactFromActor(Company.ActorCompanyId);
                    List<ContactAddressRow> contactAddressRows = contact != null ? ContactManager.GetContactAddressRows(contact.ContactId) : null;
                    distributionAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address);
                    distributionAddressCO = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO);
                    distributionPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode);
                    distributionPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress);
                    distributionCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country);
                    boardHqPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.PostalAddress);
                    boardHqCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.Country);

                    List<ContactECom> contactEcoms = contact != null ? ContactManager.GetContactEComs(contact.ContactId) : null;
                    email = contactEcoms.GetEComText(TermGroup_SysContactEComType.Email);
                    phoneHome = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneHome);
                    phoneJob = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneJob);
                    phoneMobile = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneMobile);
                    fax = contactEcoms.GetEComText(TermGroup_SysContactEComType.Fax);
                    webAddress = contactEcoms.GetEComText(TermGroup_SysContactEComType.Web);

                    PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(Company.ActorCompanyId, true, false);
                    bg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BG);
                    pg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.PG);
                    bank = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.Bank);
                    bic = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                    sepa = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.SEPA);
                    
                    if (bic.Contains("/"))
                    {
                        bicBIC = bic.Split('/')[0];
                        bicNR = bic.Split('/')[1];

                    }
                    else
                    {
                        bicBIC = PaymentManager.GetPaymentBIC(paymentInformation, TermGroup_SysPaymentType.BIC);
                        bicNR = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                    }

                    if (paymentInformation != null)
                    {
                        var rows = paymentInformation.ActivePaymentInformationRows.Where(i => i.ShownInInvoice == true).ToList();
                        foreach (var row in rows)
                        {
                            XElement paymentInformationRowElement = new XElement("PaymentinformationRow");
                            paymentInformationRowElement.Add(new XElement("BIC", row.BIC));
                            if (row.CurrencyId.HasValue)
                            {
                                var currency = CountryCurrencyManager.GetCurrencyWithCode((int)row.CurrencyId);
                                paymentInformationRowElement.Add(new XElement("CurrencyCode", currency?.Code ?? ""));
                            }
                            else
                            {
                                paymentInformationRowElement.Add(new XElement("CurrencyCode", ""));
                            }
                            paymentInformationRowElement.Add(new XElement("PaymentType", (TermGroup_SysPaymentType)row.SysPaymentTypeId));
                            paymentInformationRowElement.Add(new XElement("PaymentNumber", row.PaymentNr));

                            paymentInformationElement.Add(paymentInformationRowElement);
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region Current

                if (Company != null)
                {
                    companyName = Company.Name;
                    companyOrgNr = Company.OrgNr;
                    companyVatNr = Company.VatNr;

                    Contact contact = ContactManager.GetContactFromActor(Company.ActorCompanyId);
                    List<ContactAddressRow> contactAddressRows = contact != null ? ContactManager.GetContactAddressRows(contact.ContactId) : null;
                    distributionAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address);
                    distributionAddressCO = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO);
                    distributionPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode);
                    distributionPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress);
                    distributionCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country);
                    boardHqPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.PostalAddress);
                    boardHqCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.Country);

                    List<ContactECom> contactEcoms = contact != null ? ContactManager.GetContactEComs(contact.ContactId) : null;
                    email = contactEcoms.GetEComText(TermGroup_SysContactEComType.Email);
                    phoneHome = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneHome);
                    phoneJob = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneJob);
                    phoneMobile = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneMobile);
                    fax = contactEcoms.GetEComText(TermGroup_SysContactEComType.Fax);
                    webAddress = contactEcoms.GetEComText(TermGroup_SysContactEComType.Web);

                    PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(Company.ActorCompanyId, true, false);
                    bg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BG);
                    pg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.PG);
                    bank = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.Bank);
                    bic = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                    sepa = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.SEPA);

                    if (bic.Contains("/"))
                    {
                        bicBIC = bic.Split('/')[0];
                        bicNR = bic.Split('/')[1];

                    }
                    else
                    {
                        bicBIC = PaymentManager.GetPaymentBIC(paymentInformation, TermGroup_SysPaymentType.BIC);
                        bicNR = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                    }

                    if (paymentInformation != null)
                    {
                        var rows = paymentInformation.ActivePaymentInformationRows.Where(i => i.ShownInInvoice == true).ToList();
                        foreach (var row in rows)
                        {
                            XElement paymentInformationRowElement = new XElement("PaymentinformationRow");
                            paymentInformationRowElement.Add(new XElement("BIC", row.BIC));
                            if (row.CurrencyId.HasValue)
                            {
                                var currency = CountryCurrencyManager.GetCurrencyWithCode((int)row.CurrencyId);
                                paymentInformationRowElement.Add(new XElement("CurrencyCode", currency?.Code ?? ""));
                            }
                            else
                            {
                                paymentInformationRowElement.Add(new XElement("CurrencyCode", ""));
                            }
                            paymentInformationRowElement.Add(new XElement("PaymentType", (TermGroup_SysPaymentType)row.SysPaymentTypeId));
                            paymentInformationRowElement.Add(new XElement("PaymentNumber", row.PaymentNr));

                            paymentInformationElement.Add(paymentInformationRowElement);
                        }
                    }
                }

                #endregion

                #region Add to Repository

                if (repository != null && repository.HasActivatedHistory)
                {
                    //Company
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyName, companyName);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyOrgnr, companyOrgNr);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyVatNr, companyVatNr);

                    //Addresses
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionAddress, distributionAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionAddressCO, distributionAddressCO);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionPostalCode, distributionPostalCode);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionPostalAddress, distributionPostalAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionCountry, distributionCountry);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBoardHQPostalAddress, boardHqPostalAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBoardHQCountry, boardHqCountry);

                    //Ecom
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyEmail, email);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneHome, phoneHome);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneJob, phoneJob);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneMobile, phoneMobile);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyFax, fax);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyWebAddress, webAddress);

                    //Payment
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBg, bg);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPg, pg);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBank, bank);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBic, bic);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBicNR, bicNR);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBicBIC, bicBIC);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanySepa, sepa);
                }

                #endregion
            }

            return new XElement("ReportHeader",
                   this.CreateReportTitleElement(es.ReportName),
                   this.CreateReportDescriptionElement(es.ReportDescription),
                   this.CreateReportNrElement(es.ReportNr.ToString()),
                   this.CreateCompanyElement(),
                   this.CreateCompanyOrgNrElement(),
                   new XElement("AccountYear", accountYearInterval),
                   new XElement("AccountPeriod", accountPeriodInterval),
                   this.CreateLoginNameElement(es.LoginName),
                   new XElement("SortOrderName", sortOrderName),
                   new XElement("InvoiceInterval", invoiceInterval),
                   new XElement("CustomerInterval", customerInterval),
                   new XElement("DateInterval", dateInterval),
                   new XElement("CreatedDateInterval", createdDateInterval),
                   new XElement("CompanyVatNr", companyVatNr),
                   new XElement("CompanyAddress", distributionAddress),
                   new XElement("CompanyAddressCO", distributionAddressCO),
                   new XElement("CompanyPostalCode", distributionPostalCode),
                   new XElement("CompanyPostalAddress", distributionPostalAddress),
                   new XElement("CompanyCountry", distributionCountry),
                   new XElement("CompanyBoardHQPostalAddress", boardHqPostalAddress),
                   new XElement("CompanyBoardHQCountry", boardHqCountry),
                   new XElement("CompanyEmail", email),
                   new XElement("CompanyPhoneHome", phoneHome),
                   new XElement("CompanyPhoneWork", phoneJob),
                   new XElement("CompanyPhoneMobile", phoneMobile),
                   new XElement("CompanyFax", fax),
                   new XElement("CompanyWebAddress", webAddress),
                   new XElement("CompanyBg", bg),
                   new XElement("CompanyPg", pg),
                   new XElement("CompanyBank", bank),
                   new XElement("CompanyBic", bic),
                   new XElement("CompanyBicNR", bicNR),
                   new XElement("CompanyBicBIC", bicBIC),
                   new XElement("CompanySepa", sepa),
                   new XElement("CompanyLogo", companyLogoPath),
                   new XElement("ReminderPaymentCondition", paymentCondition != null ? paymentCondition.Name : ""),
                   new XElement("ExtendedTimeRegistration", extendedTimeRegistration),
                   new XElement("ShowStartStop", showStartStopInTimeReport),
                   new XElement("PaymentDateInterval", paymentDateInterval),
                   new XElement("AdditionalDiscountInUse", additionalDiscountInUse),
                   paymentInformationElement
                   );
        }

        protected  string GetBillingInvoiceIntervalText(BillingReportParamsDTO reportParams)
        {
            string text = "";
            if (reportParams.SB_HasInvoiceNrInterval)
                text = reportParams.SB_InvoiceNrFrom + "-" + reportParams.SB_InvoiceNrTo;
            else if (reportParams.SB_HasInvoiceIds)
                text = StringUtility.GetCommaSeparatedString<int>(reportParams.SB_InvoiceIds);
            return text;
        }
        protected string GetBillingCustomerIntervalText(BillingReportParamsDTO reportParams)
        {
            string text = "";
            if (reportParams.SB_HasActorNrInterval)
                text = reportParams.SB_ActorNrFrom + "-" + reportParams.SB_ActorNrTo;
            return text;
        }
        protected string GetBillingDateIntervalText(BillingReportParamsDTO reportParams)
        {
            string text = "";
            if (reportParams.HasDateInterval)
            {
                if (reportParams.DateFrom != CalendarUtility.DATETIME_MINVALUE)
                    text += reportParams.DateFrom.ToShortDateString();
                text += "-";
                if (reportParams.DateTo != CalendarUtility.DATETIME_MAXVALUE)
                    text += reportParams.DateTo.ToShortDateString();
            }
            return text;
        }
        protected string GetBillingCreatedDateIntervalText(BillingReportParamsDTO reportParams)
        {
            string text = "";
            if (reportParams.HasCreateDateInterval)
            {
                if (reportParams.SB_CreateDateFrom != CalendarUtility.DATETIME_MINVALUE)
                    text += reportParams.SB_CreateDateFrom.ToShortDateString();
                text += "-";
                if (reportParams.SB_CreateDateTo != CalendarUtility.DATETIME_MAXVALUE)
                    text += reportParams.SB_CreateDateTo.ToShortDateString();
            }
            return text;
        }
        protected string GetBillingPaymentDateIntervalText(BillingReportParamsDTO reportParams)
        {
            string text = "";
            if (reportParams.SB_HasPaymentDateInterval)
            {
                if (reportParams.SB_PaymentDateFrom != CalendarUtility.DATETIME_MINVALUE)
                    text += reportParams.SB_PaymentDateFrom.ToShortDateString();
                text += "-";
                if (reportParams.SB_PaymentDateTo != CalendarUtility.DATETIME_MAXVALUE)
                    text += reportParams.SB_PaymentDateTo.ToShortDateString();
            }
            return text;
        }

        protected XElement CreateAccountIntervalElement(CreateReportResult reportResult, BillingReportParamsDTO reportParams, List<AccountDimDTO> accountDims)
        {
            return new XElement("AccountInterval", GetAccountIntervalText(reportResult, reportParams, accountDims));
        }
        protected string GetAccountIntervalText(CreateReportResult reportResult, BillingReportParamsDTO reportParams, List<AccountDimDTO> accountDims)
        {
            if (!reportParams.SA_HasAccountInterval || reportParams.SA_AccountIntervals == null || accountDims == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (AccountIntervalDTO accountInterval in reportParams.SA_AccountIntervals)
            {
                if (accountDims.Any(i => i.AccountDimId == accountInterval.AccountDimId))
                    sb.Append(accountInterval.AccountNrFrom + "-" + accountInterval.AccountNrTo + " ");
            }

            foreach (AccountDimDTO accountDim in accountDims)
            {
                bool first = true;
                foreach (AccountIntervalDTO accountInterval in reportParams.SA_AccountIntervals)
                {
                    if (accountInterval.AccountDimId == accountDim.AccountDimId)
                    {
                        if (first)
                        {
                            if (sb.Length > 0)
                                sb.Append("; ");
                            sb.Append(accountDim.Name + ": ");
                            first = false;
                        }
                        if (accountInterval.AccountNrFrom == accountInterval.AccountNrTo)
                        {
                            if (accountInterval.AccountNrFrom == " ")
                            {
                                sb.Append(" ");
                            }
                            else
                            {
                                Account accountFrom = AccountManager.GetAccountByNr(accountInterval.AccountNrFrom, accountDim.AccountDimId, reportResult.ActorCompanyId, false, false, false);
                                if (accountFrom != null)
                                    sb.Append($"{accountInterval.AccountNrFrom} {accountFrom.Name}");
                            }
                        }
                        else
                        {
                            Account accountFrom = AccountManager.GetAccountByNr(accountInterval.AccountNrFrom, accountDim.AccountDimId, reportResult.ActorCompanyId, false, false, false);
                            if (accountFrom == null)
                                sb.Append(accountInterval.AccountNrFrom);
                            else
                                sb.Append($"{accountInterval.AccountNrFrom} {accountFrom.Name}");

                            Account accountTo = AccountManager.GetAccountByNr(accountInterval.AccountNrTo, accountDim.AccountDimId, reportResult.ActorCompanyId, false, false, false);
                            if (accountTo == null)
                                sb.Append($" - {accountInterval.AccountNrTo}");
                            else
                                sb.Append($" - {accountInterval.AccountNrTo} {accountTo.Name}");
                        }
                    }
                }
            }

            return sb.ToString();
        }
        protected XElement CreateDateIntervalElement(BillingReportParamsDTO reportParams)
        {
            return new XElement("DateInterval", GetDateIntervalText(reportParams));
        }
        protected XElement CreateEmployeeIntervalElement(BillingReportParamsDTO reportParams)
        {
            return new XElement("EmployeeInterval", reportParams.GetStandardEmployeeIntervalText());
        }


        public XDocument CreateHousholdTaxDeductionData(CreateReportResult reportResult, EvaluatedSelection es = null)
        {
            bool isRut = false;
            bool isGreen = false;
            XElement householdTaxDeductionElement = null;

            #region Prereq
            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            List<SysHouseholdType> householdDeductionTypes = ProductManager.GetSysHouseholdType(false);

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement baseElement = new XElement(ROOT + "_" + "HousholdTaxDeductionReport");

            XElement rootElement = new XElement("Begaran");
            #endregion

            using (CompEntities entities = new CompEntities())
            {

                if (es != null)
                {
                    reportParams.SB_HTDCustomerInvoiceRowIds = es.SB_HTDCustomerInvoiceRowIds;
                    reportParams.SB_HTDHasCustomerInvoiceRowIds = es.SB_HTDHasCustomerInvoiceRowIds;
                    reportParams.SB_HTDSeqNbr = es.SB_HTDSeqNbr;
                }
                else
                {
                    if (reportParams.SB_HTDCustomerInvoiceRowIds.Count > 0
                        && !reportParams.SB_HTDUseInputSeqNbr)
                    {
                        HouseholdTaxDeductionRow row = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRow(entities, reportParams.SB_HTDCustomerInvoiceRowIds[0], true, true);
                        var sqm = new SequenceNumberManager(parameterObject);
                        var SeqNr = 0;
                        var entityName = string.Empty;
                        switch (row.HouseHoldTaxDeductionType)
                        {
                            case (int)TermGroup_HouseHoldTaxDeductionType.ROT:
                                entityName = "HouseholdTaxDeduction";
                                break;
                            case (int)TermGroup_HouseHoldTaxDeductionType.RUT:
                                entityName = "RutTaxDeduction";
                                break;
                            case (int)TermGroup_HouseHoldTaxDeductionType.GREEN:
                                entityName = "GreenTaxDeduction";
                                break;
                        }
                        SeqNr = sqm.GetLastUsedSequenceNumber(reportResult.ActorCompanyId, entityName);
                        reportParams.SB_HTDSeqNbr = SeqNr + 1;
                    }
                }
                rootElement.Add(new XElement("BegaranNr", reportParams.SB_HTDSeqNbr));

                #region Content

                #region Head

                Company receiverCompany = CompanyManager.GetCompany(reportParams.SB_HTDCompanyId);
                if (receiverCompany == null)
                    return null;

                int defaultHouseholdDeductionType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultHouseholdDeductionType, 0, reportResult.ActorCompanyId, 0);

                #endregion

                bool first = true;
                int arendenNr = 0;
                int utfortArbetelXmlId = 0;
                foreach (var htdRowId in reportParams.SB_HTDCustomerInvoiceRowIds)
                {
                    #region Row

                    HouseholdTaxDeductionRow row = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRow(entities, htdRowId, true, true);
                    if (row == null || row.CustomerInvoiceRow == null || row.CustomerInvoiceRow.CustomerInvoice == null)
                        continue;

                    var houseHoldDeductionRowsForThisInvoice = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRowsPerInvoiceDict(entities, row.CustomerInvoiceRow.InvoiceId);
                    int numberOfHouseHoldDeductionsForThisInvoice = houseHoldDeductionRowsForThisInvoice.Count;

                    var customerInvoice = InvoiceManager.GetCustomerInvoice(row.CustomerInvoiceRow.CustomerInvoice.InvoiceId, false, false, false, false, true, false, false, true, true, false, false, false);
                    if (customerInvoice == null || customerInvoice.CustomerInvoiceRow == null)
                        continue;

                    if (first)
                    {
                        isRut = row.HouseHoldTaxDeductionType == (int)TermGroup_HouseHoldTaxDeductionType.RUT;
                        isGreen = (row.HouseHoldTaxDeductionType == (int)TermGroup_HouseHoldTaxDeductionType.GREEN);

                        householdTaxDeductionElement = new XElement("RotBegaran");
                        first = false;
                    }

                    XElement housholdTaxDeductionRow = new XElement("Arenden");

                    arendenNr += 1;

                    #region Calculate Amounts and create type tags

                    List<GenericType<int, decimal, decimal, string>> typesList = new List<GenericType<int, decimal, decimal, string>>();
                    int householdTaxDeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, reportResult.ActorCompanyId, 0);
                    int household50TaxDeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeduction, 0, reportResult.ActorCompanyId, 0);
                    List<int> householdDeductionTypeIds = householdDeductionTypes.Where(t => t.SysHouseholdTypeClassification == row.HouseHoldTaxDeductionType).Select(t => t.SysHouseholdTypeId).ToList();
                    int nonValidTypeId = householdDeductionTypes.FirstOrDefault(t => t.SysHouseholdTypeClassification == 0)?.SysHouseholdTypeId ?? 0;

                    //ROT TYPES
                    List<GenericType<int, decimal, decimal, string>> rotTypes = new List<GenericType<int, decimal, decimal, string>>();
                    foreach (SysHouseholdType hht in householdDeductionTypes.Where(t => t.SysHouseholdTypeClassification == 1))
                    {
                        rotTypes.Add(new GenericType<int, decimal, decimal, string>() { Field1 = hht.SysHouseholdTypeId, Field2 = Decimal.Zero, Field3 = Decimal.Zero, Field4 = hht.XMLTagName });
                    }
                    typesList.AddRange(rotTypes);

                    //RUT TYPES
                    List<GenericType<int, decimal, decimal, string>> rutTypes = new List<GenericType<int, decimal, decimal, string>>();
                    foreach (SysHouseholdType hht in householdDeductionTypes.Where(t => t.SysHouseholdTypeClassification == 2))
                    {
                        rutTypes.Add(new GenericType<int, decimal, decimal, string>() { Field1 = hht.SysHouseholdTypeId, Field2 = Decimal.Zero, Field3 = Decimal.Zero, Field4 = hht.XMLTagName });
                    }
                    typesList.AddRange(rutTypes);

                    //GREEN TYPES
                    List<GenericType<int, decimal, decimal, string>> greenTypes = new List<GenericType<int, decimal, decimal, string>>();
                    foreach (SysHouseholdType hht in householdDeductionTypes.Where(t => t.SysHouseholdTypeClassification == 3))
                    {
                        greenTypes.Add(new GenericType<int, decimal, decimal, string>() { Field1 = hht.SysHouseholdTypeId, Field2 = Decimal.Zero, Field3 = Decimal.Zero, Field4 = hht.XMLTagName });
                    }
                    typesList.AddRange(greenTypes);

                    decimal amountTotal = Decimal.Zero;
                    decimal amountTotalNonValid = Decimal.Zero;
                    if (customerInvoice.CustomerInvoiceRow != null)
                    {
                        //All CustomerInvoiceRows of type Product
                        List<CustomerInvoiceRow> customerInvoiceProductRows = customerInvoice.ActiveCustomerInvoiceRows.Where(i => i.Type == (int)SoeInvoiceRowType.ProductRow).ToList();

                        bool noType = !customerInvoiceProductRows.Any(r => r.HouseholdDeductionType.HasValue);
                        foreach (CustomerInvoiceRow customerInvoiceRow in customerInvoiceProductRows)
                        {
                            //Check that Product not is HouseholdTaxDeduction 
                            if (customerInvoiceRow.ProductId.HasValue && customerInvoiceRow.ProductId.Value != householdTaxDeductionProductId && customerInvoiceRow.ProductId.Value != household50TaxDeductionProductId)
                            {
                                //Check that Product has VatType Service
                                var invoiceProduct = customerInvoiceRow.Product as InvoiceProduct;
                                if (invoiceProduct != null)
                                {
                                    if (invoiceProduct.VatType == (int)TermGroup_InvoiceProductVatType.Service)
                                    {
                                        if (noType || (customerInvoiceRow.HouseholdDeductionType.HasValue && householdDeductionTypeIds.Contains((int)customerInvoiceRow.HouseholdDeductionType)) || (isGreen && (!customerInvoiceRow.HouseholdDeductionType.HasValue || customerInvoiceRow.HouseholdDeductionType.Value == nonValidTypeId)))
                                        {
                                            //Accumulate Amount and VatAmount
                                            amountTotal += customerInvoiceRow.SumAmount > 0 ? customerInvoiceRow.SumAmount : Decimal.Negate(customerInvoiceRow.SumAmount);
                                            amountTotal += customerInvoiceRow.VatAmount > 0 ? customerInvoiceRow.VatAmount : Decimal.Negate(customerInvoiceRow.VatAmount);

                                            if (!noType)
                                            {
                                                GenericType<int, decimal, decimal, string> typeRow = typesList.FirstOrDefault(g => g.Field1 == (int)customerInvoiceRow.HouseholdDeductionType);
                                                if (typeRow != null)
                                                {
                                                    if (!customerInvoiceRow.ProductUnitReference.IsLoaded)
                                                        customerInvoiceRow.ProductUnitReference.Load();

                                                    if (customerInvoiceRow.ProductUnit != null)
                                                    {
                                                        if (customerInvoiceRow.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += customerInvoiceRow.Quantity.Value;
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += customerInvoiceRow.Quantity.Value;
                                                        else
                                                            typeRow.Field3 += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                    else
                                                    {
                                                        if (!invoiceProduct.ProductUnitReference.IsLoaded)
                                                            invoiceProduct.ProductUnitReference.Load();

                                                        if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += customerInvoiceRow.Quantity.Value;
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += customerInvoiceRow.Quantity.Value;
                                                        else
                                                            typeRow.Field3 += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                GenericType<int, decimal, decimal, string> typeRow = null;
                                                if (invoiceProduct.HouseholdDeductionType.HasValue)
                                                    typeRow = typesList.FirstOrDefault(g => g.Field1 == (int)invoiceProduct.HouseholdDeductionType);
                                                else
                                                    typeRow = typesList.FirstOrDefault(g => g.Field1 == defaultHouseholdDeductionType);

                                                if (typeRow != null)
                                                {
                                                    if (!customerInvoiceRow.ProductUnitReference.IsLoaded)
                                                        customerInvoiceRow.ProductUnitReference.Load();

                                                    if (customerInvoiceRow.ProductUnit != null)
                                                    {
                                                        if (customerInvoiceRow.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += customerInvoiceRow.Quantity.Value;
                                                        else if (customerInvoiceRow.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += customerInvoiceRow.Quantity.Value;
                                                        else
                                                            typeRow.Field3 += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                    else
                                                    {
                                                        if (!invoiceProduct.ProductUnitReference.IsLoaded)
                                                            invoiceProduct.ProductUnitReference.Load();

                                                        if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "min" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += (customerInvoiceRow.Quantity.Value / 60);
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "tim" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += customerInvoiceRow.Quantity.Value;
                                                        else if (invoiceProduct.ProductUnit != null && invoiceProduct.ProductUnit.Code.ToLower() == "timmar" && customerInvoiceRow.Quantity.HasValue)
                                                            typeRow.Field2 += customerInvoiceRow.Quantity.Value;
                                                        else
                                                            typeRow.Field3 += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                    }
                                                }
                                            }
                                        }
                                        else if (customerInvoiceRow.HouseholdDeductionType.HasValue && customerInvoiceRow.HouseholdDeductionType == nonValidTypeId)
                                        {
                                            amountTotalNonValid += customerInvoiceRow.SumAmount > 0 ? customerInvoiceRow.SumAmount : Decimal.Negate(customerInvoiceRow.SumAmount);
                                            amountTotalNonValid += customerInvoiceRow.VatAmount > 0 ? customerInvoiceRow.VatAmount : Decimal.Negate(customerInvoiceRow.VatAmount);
                                        }
                                    }
                                    else if (invoiceProduct.VatType == (int)TermGroup_InvoiceProductVatType.Merchandise)
                                    {
                                        if (customerInvoiceRow.HouseholdDeductionType.HasValue && householdDeductionTypeIds.Contains((int)customerInvoiceRow.HouseholdDeductionType))
                                        {
                                            GenericType<int, decimal, decimal, string> typeRow = typesList.FirstOrDefault(g => g.Field1 == (int)customerInvoiceRow.HouseholdDeductionType);
                                            if (typeRow != null)
                                            {
                                                typeRow.Field3 += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                if (isGreen)
                                                    amountTotal += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                            }
                                        }
                                        else if (customerInvoiceRow.HouseholdDeductionType.HasValue && customerInvoiceRow.HouseholdDeductionType == nonValidTypeId)
                                        {
                                            amountTotalNonValid += customerInvoiceRow.SumAmount > 0 ? customerInvoiceRow.SumAmount : Decimal.Negate(customerInvoiceRow.SumAmount);
                                            amountTotalNonValid += customerInvoiceRow.VatAmount > 0 ? customerInvoiceRow.VatAmount : Decimal.Negate(customerInvoiceRow.VatAmount);
                                        }
                                        else if (noType || (isGreen && (!customerInvoiceRow.HouseholdDeductionType.HasValue || customerInvoiceRow.HouseholdDeductionType.Value == nonValidTypeId)))
                                        {
                                            GenericType<int, decimal, decimal, string> typeRow = typesList.FirstOrDefault(g => g.Field1 == defaultHouseholdDeductionType);
                                            if (typeRow != null)
                                            {
                                                typeRow.Field3 += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                if (isGreen)
                                                {
                                                    amountTotal += customerInvoiceRow.SumAmountCurrency + customerInvoiceRow.VatAmountCurrency;
                                                }
                                            }
                                        }
                                    }
                                    else if (invoiceProduct.VatType == (int)TermGroup_InvoiceProductVatType.None && customerInvoiceRow.HouseholdDeductionType.HasValue && customerInvoiceRow.HouseholdDeductionType == nonValidTypeId)
                                    {
                                        amountTotalNonValid += customerInvoiceRow.SumAmount > 0 ? customerInvoiceRow.SumAmount : Decimal.Negate(customerInvoiceRow.SumAmount);
                                        amountTotalNonValid += customerInvoiceRow.VatAmount > 0 ? customerInvoiceRow.VatAmount : Decimal.Negate(customerInvoiceRow.VatAmount);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    amountTotal = amountTotal / numberOfHouseHoldDeductionsForThisInvoice;
                    amountTotalNonValid = amountTotalNonValid / numberOfHouseHoldDeductionsForThisInvoice;
                    decimal amountRequest = row.Amount;
                    if (amountRequest == 0 || amountTotal == 0)
                        continue;

                    decimal amountPayed = amountTotal - amountRequest /*/ numberOfHouseHoldDeductionsForThisInvoice*/;
                    DateTime? paymentDate = PaymentManager.GetLastPaymentDateForInvoice(entities, row.CustomerInvoiceRow.CustomerInvoice.InvoiceId);

                    string socialSecNr = row.SocialSecNr;

                    //Rensa bort sånt som inte ska vara med enligt skatteverkets mall
                    socialSecNr = socialSecNr.Replace("-", "");
                    socialSecNr = socialSecNr.Replace(" ", "");

                    //De vill ha tolv siffror, så vi får lägga till om det saknas "19" först
                    if (socialSecNr.Length == 10)
                        socialSecNr = "19" + socialSecNr;

                    string orgNr = row.CooperativeOrgNr;
                    orgNr = orgNr != null ? orgNr.Replace("-", "") : String.Empty;
                    orgNr = orgNr.Replace(" ", "");

                    housholdTaxDeductionRow.Add(
                        new XElement("Id", arendenNr),                                        // ArendeNr
                        new XElement("Kopare", socialSecNr),                                        // Personnummer
                        new XElement("BetalningsDatum", paymentDate.HasValue ? paymentDate.Value.Date.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()), // Betalningsdatum (Betalningens PayDate)
                        new XElement("PrisForArbete", Math.Round(amountTotal)),                         // Pris för arbetet (räknas ut, fakturans andra rader med produkter med typen tjänst)
                        new XElement("BetaltBelopp", Math.Round(amountPayed)),                             // Belopp du fått betalt (AmountTotal - AmountRequest)
                        new XElement("BegartBelopp", Math.Round(row.Amount)),                              // Belopp du begär
                        new XElement("FakturaNr", customerInvoice.InvoiceNr),                       // Fakturans nummer
                        new XElement("Ovrigkostnad", (int)Math.Ceiling(amountTotalNonValid)));                         // Övrig kostnad

                    housholdTaxDeductionRow.Add(
                        new XElement("Fastighetsbeteckning", row.Property),
                        new XElement("LagenhetsNr", row.ApartmentNr),
                        new XElement("BrfOrgNr", orgNr));

                    foreach (var hht in typesList)
                    {
                        if (hht.Field2 == 0 && hht.Field3 == 0)
                            continue;
                        string hhtField2 = ((int)Math.Ceiling(hht.Field2 / numberOfHouseHoldDeductionsForThisInvoice)).ToString();
                        string hhtField3 = ((int)Math.Ceiling(hht.Field3 / numberOfHouseHoldDeductionsForThisInvoice)).ToString();

                        XElement workRow = new XElement("UtfortArbete");
                        utfortArbetelXmlId += 1;
                        workRow.Add(
                            new XElement("Id", utfortArbetelXmlId),
                            new XElement("Typ", hht.Field4),
                            new XElement("AntalTimmar", hhtField2),
                            new XElement("Materialkostnad", hhtField3));
                        housholdTaxDeductionRow.Add(workRow);
                    }
                    householdTaxDeductionElement.Add(housholdTaxDeductionRow);

                    row.SeqNr = reportParams.SB_HTDSeqNbr;

                    #endregion
                }

                entities.SaveChanges();

                #endregion
            }

            // Update latest used sequence number
            if (isRut)
                SequenceNumberManager.UpdateSequenceNumber(reportParams.SB_HTDCompanyId, "RutTaxDeduction", reportParams.SB_HTDSeqNbr);
            else if (isGreen)
                SequenceNumberManager.UpdateSequenceNumber(reportParams.SB_HTDCompanyId, "GreenTaxDeduction", reportParams.SB_HTDSeqNbr);
            else
                SequenceNumberManager.UpdateSequenceNumber(reportParams.SB_HTDCompanyId, "HouseholdTaxDeduction", reportParams.SB_HTDSeqNbr);

            rootElement.Add(householdTaxDeductionElement);
            baseElement.Add(rootElement);
            document.Add(baseElement);

            return GetValidatedDocument(document, SoeReportTemplateType.HousholdTaxDeduction);
        }

        public XDocument CreateTaxReductionBalanceListData(CreateReportResult reportResult)
        {
            if (reportResult.ReportTemplateType != SoeReportTemplateType.TaxReductionBalanceListReport)
                return null;

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "TaxReductionBalanceList");

            //TaxReductionReport
            XElement taxReductionReportElement = new XElement("TaxReductionBalanceList");

            //Header Labels
            XElement headerLabels = new XElement("ReportHeaderLabels",
                new XElement("TaxReductionTypeLabel", GetReportText(0, "Typ av skattereduktion")),
                new XElement("ApplicationStatusLabel", GetReportText(0, "Ansökan status")),
                new XElement("CustomerNrLabel", GetReportText(85, "Kundnr")),
                new XElement("DateSelectionLabel", GetReportText(76, "Datumurval")));

            #endregion

            return document;
        }

        #endregion

        #region Purchase
        protected XElement CreatePurchaseOrderReportHeaderElement(CreateReportResult reportResult, BillingReportParamsDTO reportParams)
        {
           
                string accountYearInterval = reportParams.GetAccountYearIntervalText();
                string accountPeriodInterval = reportParams.GetAccountPeriodIntervalText();
                string sortOrderName = this.GetBillingInvoiceSortOrderText(reportParams, (int)TermGroup.ReportBillingStockSortOrder);
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                string companyLogoPath = GetCompanyLogoFilePath(entitiesReadOnly, reportResult.ActorCompanyId, false);

                var companyVatNr = Company.VatNr;
                

                Contact contact = ContactManager.GetContactFromActor(Company.ActorCompanyId);
                List<ContactAddressRow> contactAddressRows = contact != null ? ContactManager.GetContactAddressRows(contact.ContactId) : null;
                var distributionAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address);
                var distributionAddressCO = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO);
                var distributionPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode);
                var distributionPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress);
                var distributionCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country);
                var boardHqPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.PostalAddress);
                var boardHqCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.Country);

                List<ContactECom> contactEcoms = contact != null ? ContactManager.GetContactEComs(contact.ContactId) : null;
                var email = contactEcoms.GetEComText(TermGroup_SysContactEComType.Email);
                var phoneHome = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneHome);
                var phoneJob = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneJob);
                var phoneMobile = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneMobile);
                var fax = contactEcoms.GetEComText(TermGroup_SysContactEComType.Fax);
                var webAddress = contactEcoms.GetEComText(TermGroup_SysContactEComType.Web);

                PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(Company.ActorCompanyId, true, false);
                var bg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BG);
                var pg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.PG);
                var bank = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.Bank);
                var bic = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                var sepa = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.SEPA);

                var reportTerm = TermManager.GetCompTerm(CompTermsRecordType.ReportName, reportResult.ReportId, reportLanguageId);
                var reportName = reportTerm?.Name ?? reportResult.ReportName;

                return new XElement("ReportHeader",
                    this.CreateReportTitleElement(reportName),
                    this.CreateReportDescriptionElement(reportResult.ReportDescription),
                    this.CreateReportNrElement(reportResult.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    new XElement("AccountYear", accountYearInterval),
                    new XElement("AccountPeriod", accountPeriodInterval),
                    this.CreateLoginNameElement(reportResult.LoginName),
                    new XElement("SortOrder", reportParams.SB_SortOrder),
                    new XElement("SortOrderName", sortOrderName),
                    new XElement("PurchaseInterval", CreatePurchaseOrderIntervalText(reportParams)),
                    new XElement("SupplierInterval", CreateSupplierIntervalText(reportParams)),
                    new XElement("DateInterval", this.GetDateIntervalText(reportParams)),
                    new XElement("CompanyVatNr", companyVatNr),
                    new XElement("CompanyAddress", distributionAddress),
                    new XElement("CompanyAddressCO", distributionAddressCO),
                    new XElement("CompanyPostalCode", distributionPostalCode),
                    new XElement("CompanyPostalAddress", distributionPostalAddress),
                    new XElement("CompanyCountry", distributionCountry),
                    new XElement("CompanyBoardHQPostalAddress", boardHqPostalAddress),
                    new XElement("CompanyBoardHQCountry", boardHqCountry),
                    new XElement("CompanyEmail", email),
                    new XElement("CompanyPhoneHome", phoneHome),
                    new XElement("CompanyPhoneWork", phoneJob),
                    new XElement("CompanyPhoneMobile", phoneMobile),
                    new XElement("CompanyFax", fax),
                    new XElement("CompanyWebAddress", webAddress),
                    new XElement("CompanyBg", bg),
                    new XElement("CompanyPg", pg),
                    new XElement("CompanyBank", bank),
                    new XElement("CompanyBic", bic),
                    new XElement("CompanySepa", sepa),
                    new XElement("CompanyLogo", companyLogoPath)
                    );
        }
        public XDocument CreatePurchaseOrderData(CreateReportResult reportResult)
        {
            #region Prereq

            if (reportResult == null)
                return null;

            #endregion

            #region Init document
            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "PurchaseOrder");

            //
            XElement purchaseOrderElement = new XElement("PurchaseOrder");

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(this.AccountManager, reportResult, entitiesReadOnly, this);
            #endregion

            
             #region Load purchases

            var purchases = new List<PurchaseReportDTO>();

            using (var entities = new CompEntities())
            {

                IQueryable<Purchase> query = entities.Purchase.Where(p => p.State == (int)SoeEntityState.Active && p.Origin.ActorCompanyId == this.ActorCompanyId);

                if (reportParams.SB_HasPurchaseIds)
                {
                    query = query.Where(p => reportParams.SB_PurchaseIds.Contains(p.PurchaseId));
                }
                if (reportParams.SB_HasActorNrInterval)
                {
                    query = query.Where(p => p.Supplier.SupplierNr.CompareTo(reportParams.SB_ActorNrFrom) >= 0 && p.Supplier.SupplierNr.CompareTo(reportParams.SB_ActorNrTo) <= 0);
                }
                if (reportParams.HasDateInterval && !(reportParams.DateFrom == CalendarUtility.DATETIME_MINVALUE && reportParams.DateTo == CalendarUtility.DATETIME_MAXVALUE) && !(reportParams.DateFrom == CalendarUtility.DATETIME_DEFAULT && reportParams.DateTo == CalendarUtility.DATETIME_DEFAULT))
                {
                    query = query.Where(p => p.PurchaseDate >= reportParams.DateFrom && p.PurchaseDate <= reportParams.DateTo);
                }

                purchases = query
                    .Select(PurchaseExtensions.GetPurchaseReportDTOFull)
                    .ToList();

                if (reportParams.SB_HasPurchaseNrInterval)
                {
                    if (reportParams.SB_PurchaseNrFrom != null)
                        purchases = purchases.Where(p => p.PurchaseNr != null && Convert.ToInt32(p.PurchaseNr) >= Convert.ToInt32(reportParams.SB_PurchaseNrFrom)).ToList();

                    if (reportParams.SB_PurchaseNrTo != null)
                        purchases = purchases.Where(p => p.PurchaseNr != null && Convert.ToInt32(p.PurchaseNr) <= Convert.ToInt32(reportParams.SB_PurchaseNrTo)).ToList();
                }

                if (reportParams.SB_SortOrder == (int)TermGroup_PurchaseOrderSortOrder.PurchaseNr)
                {
                    bool canSortAsInt = true;
                    foreach (var head in purchases)
                    {
                        int purchaseNrInt;
                        bool success = int.TryParse(head.PurchaseNr, out purchaseNrInt);
                        if (!success)
                        {
                            canSortAsInt = false;
                            break;
                        }
                        head.PurchaseNrInt = purchaseNrInt;
                    }
                    purchases = canSortAsInt ? purchases.OrderBy(p => p.PurchaseNrInt).ToList() : purchases.OrderBy(p => p.PurchaseNr).ToList();
                }
                else if (reportParams.SB_SortOrder == (int)TermGroup_PurchaseOrderSortOrder.SupplierNr)
                {
                    bool canSortAsInt = true;
                    foreach (var head in purchases)
                    {
                        int supplierNrInt;
                        bool success = int.TryParse(head.SupplierNr, out supplierNrInt);
                        if (!success)
                        {
                            canSortAsInt = false;
                            break;
                        }
                        head.SupplierNumberInt = supplierNrInt;
                    }
                    purchases = canSortAsInt ? purchases.OrderBy(p => p.SupplierNumberInt).ToList() : purchases.OrderBy(p => p.SupplierNr).ToList();
                }

            }

            #endregion

            #region Language

            var groupbyLanguage = purchases.GroupBy(x => x.SysLanguageId).ToList();
            if (groupbyLanguage.Count == 1)
            {
                reportLanguageId = groupbyLanguage[0].First().SysLanguageId ?? 0;
            }

            SetPrintoutLanguage(reportResult, reportLanguageId);

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreatePurchaseOrderReportHeaderLabelsElement();
            purchaseOrderElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreatePurchaseOrderReportHeaderElement(reportResult, reportParams);

            purchaseOrderElement.Add(reportHeaderElement);

                #endregion
            

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreatePurchaseOrderReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            purchaseOrderElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Content

            //Delivery adresses
            Contact contact = ContactManager.GetContactFromActor(Company.ActorCompanyId);
            var adressesCompany = ContactManager.GetContactAddresses(contact.ContactId, TermGroup_SysContactAddressType.Delivery, false, true);
            var currencies = CountryCurrencyManager.GetCurrencies(this.ActorCompanyId);

            foreach (var head in purchases)
            {
                head.PurchaseRows = head.PurchaseRows ?? new List<PurchaseRowReportDTO>();

                var addressDelivery = string.Empty;
                var addressCODelivery = string.Empty;
                var addressPostalCodeDelivery = string.Empty;
                var addressPostalAddressDelivery = string.Empty;
                var addressCountryDelivery = string.Empty;
                var supplierNameDict = new Dictionary<int, string>();
                ContactAddress contactAddressDelivery = adressesCompany.FirstOrDefault(a => a.ContactAddressId == head.DeliveryAddressId.GetValueOrDefault());
                if (contactAddressDelivery != null)
                {
                    addressDelivery = ContactManager.GetContactAddressRowText(contactAddressDelivery, TermGroup_SysContactAddressRowType.Address);
                    addressPostalCodeDelivery = ContactManager.GetContactAddressRowText(contactAddressDelivery, TermGroup_SysContactAddressRowType.PostalCode);
                    addressPostalAddressDelivery = ContactManager.GetContactAddressRowText(contactAddressDelivery, TermGroup_SysContactAddressRowType.PostalAddress);
                    addressCountryDelivery = ContactManager.GetContactAddressRowText(contactAddressDelivery, TermGroup_SysContactAddressRowType.Country);
                    addressCODelivery = ContactManager.GetContactAddressRowText(contactAddressDelivery, TermGroup_SysContactAddressRowType.AddressCO);
                }

                var addressDistribution = string.Empty;
                var addressCODistribution = string.Empty;
                var addressPostalCodeDistribution = string.Empty;
                var addressPostalAddressDistribution = string.Empty;
                var addressCountryDistribution = string.Empty;
                if (head.SupplierId.HasValue)
                {
                    var contactSupplier = ContactManager.GetContactFromActor(head.SupplierId.Value, loadAllContactInfo: true);
                    var contactAddressDistribution = contactSupplier.ContactAddress.FirstOrDefault(a => a.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution);
                    addressDistribution = ContactManager.GetContactAddressRowText(contactAddressDistribution, TermGroup_SysContactAddressRowType.Address);
                    addressPostalCodeDistribution = ContactManager.GetContactAddressRowText(contactAddressDistribution, TermGroup_SysContactAddressRowType.PostalCode);
                    addressPostalAddressDistribution = ContactManager.GetContactAddressRowText(contactAddressDistribution, TermGroup_SysContactAddressRowType.PostalAddress);
                    addressCountryDistribution = ContactManager.GetContactAddressRowText(contactAddressDistribution, TermGroup_SysContactAddressRowType.Country);
                    addressCODistribution = ContactManager.GetContactAddressRowText(contactAddressDistribution, TermGroup_SysContactAddressRowType.AddressCO);
                }

                var currencyName = String.Empty;
                var currencyCode = String.Empty;
                var currency = currencies.FirstOrDefault(c => c.CurrencyId == head.CurrencyId.GetValueOrDefault());
                if (currency != null)
                {
                    currencyName = GetText(currency.SysTermId, (int)TermGroup.SysCurrency);
                    currencyCode = CountryCurrencyManager.GetCurrencyCode(currency.SysCurrencyId);
                }
                var supplier = SupplierManager.GetSupplier((int)head.SupplierId, false, false, false, false);
                //Report name
                int PurchaseNrInt;
                Int32.TryParse(head.PurchaseNr.Trim(), out PurchaseNrInt);
                if (PurchaseNrInt > 0 && !supplierNameDict.ContainsKey(PurchaseNrInt))
                    supplierNameDict.Add(PurchaseNrInt, head.SupplierName.Replace('\n', ' '));

                var purchaseDate = CountryCurrencyManager.GetDateFormatedForCountry(supplier.SysCountryId.GetValueOrDefault(), head.PurchaseDate, GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId));
                var wantedDeliveryDate = CountryCurrencyManager.GetDateFormatedForCountry(supplier.SysCountryId.GetValueOrDefault(), head.WantedDeliveryDate, GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId));
                XElement headEl = new XElement("PurchaseHead");
                headEl.Add(
                    new XAttribute("id", head.PurchaseId),
                    new XElement("SupplierName", head.SupplierName),
                    new XElement("SupplierNr", head.SupplierNr),
                    new XElement("SupplierOurCustomerNr", head.OurCustomerNr),
                    new XElement("SupplierOrgNr", head.SupplierOrgNr),
                    new XElement("SupplierVatNr", head.SupplierVatNr),
                    new XElement("SupplierCountryCode", head.SupplierCountryCode),
                    new XElement("SupplierCountryName", head.SupplierCountryName),
                    new XElement("SupplierAddressDistribution", addressDistribution),
                    new XElement("SupplierAddressCODistribution", addressCODistribution),
                    new XElement("SupplierPostalCodeDistribution", addressPostalCodeDistribution),
                    new XElement("SupplierPostalAddressDistribution", addressPostalAddressDistribution),
                    new XElement("SupplierCountryDistribution", addressCountryDistribution),
                    new XElement("CompanyAddressDelivery", addressDelivery),
                    new XElement("CompanyAddressCODelivery", addressCODelivery),
                    new XElement("CompanyPostalCodeDelivery", addressPostalCodeDelivery),
                    new XElement("CompanyPostalAddressDelivery", addressPostalAddressDelivery),
                    new XElement("CompanyCountryDelivery", addressCountryDelivery),
                    new XElement("PurchaseNr", head.PurchaseNr),
                    new XElement("PurchaseVatType", head.VatType),
                    new XElement("PurchaseSumAmount", head.SumAmountCurrency),
                    new XElement("PurchaseSumAmountBase", head.SumAmount),
                    new XElement("PurchaseVatAmount", head.VatAmountCurrency),
                    new XElement("PurchaseVatAmountBase", head.VatAmount),
                    new XElement("PurchaseTotalAmount", head.TotalAmountCurrency),
                    new XElement("PurchaseTotalAmountBase", head.TotalAmount),
                    new XElement("PurchaseCurrency", currencyName),
                    new XElement("PurchaseCurrencyCode", currencyCode),
                    new XElement("PurchaseCurrencyRate", head.CurrencyRate),
                    new XElement("PurchaseCurrencyDate", head.CurrencyDate),
                    new XElement("PurchaseOrderDate", purchaseDate),
                    new XElement("PurchaseReferenceYour", head.ReferenceYour),
                    new XElement("PurchaseReferenceOur", head.ReferenceOur),
                    new XElement("PurchaseLabel", head.PurchaseLabel),
                    new XElement("PurchaseDeliveryAddress", head.DeliveryAddress),
                    new XElement("PurchaseWantedDeliveryDate", wantedDeliveryDate),
                    new XElement("PurchaseAttention", head.Attention),
                    new XElement("PurchasePaymentCondition", head.PaymentConditionName),
                    new XElement("PurchaseDeliveryConditionCode", head.DeliveryConditionCode),
                    new XElement("PurchaseDeliveryConditionName", head.DeliveryConditionName),
                    new XElement("PurchaseDeliveryTypeCode", head.DeliveryTypeCode),
                    new XElement("PurchaseDeliveryTypeName", head.DeliveryTypeName),
                    new XElement("PurchaseState", head.State),
                    new XElement("PurchaseOriginStatus", head.OriginStatus),
                    new XElement("Created", head.Created),
                    new XElement("CreatedBy", head.CreatedBy),
                    new XElement("Modified", head.Modified),
                    new XElement("ModifiedBy", head.ModifiedBy)
                    );

                if (head.PurchaseRows.Any())
                {
                    foreach (var row in head.PurchaseRows.OrderBy(r => r.RowNr))
                    {
                        var rowDeliveryDate = CountryCurrencyManager.GetDateFormatedForCountry(supplier.SysCountryId.GetValueOrDefault(), row.DeliveryDate, GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId));
                        var rowAccDeliveryDate = CountryCurrencyManager.GetDateFormatedForCountry(supplier.SysCountryId.GetValueOrDefault(), row.AccDeliveryDate, GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId));
                        var rowWantedDeliveryDate = CountryCurrencyManager.GetDateFormatedForCountry(supplier.SysCountryId.GetValueOrDefault(), row.WantedDeliveryDate, GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId));
                        var rowEl = new XElement("PurchaseRow");
                        rowEl.Add(
                            new XAttribute("id", row.PurchaseRowId),
                            new XElement("PurchaseRowType", row.Type),
                            new XElement("PurchaseRowProductNumber", row.SupplierProductNr.Trim() != String.Empty ? row.SupplierProductNr : row.ProductNr),
                            new XElement("PurchaseRowProductName", row.ProductName),
                            new XElement("PurchaseRowProductUnitCode", row.PurchaseUnitCode),
                            new XElement("PurchaseRowProductUnitName", row.PurchaseUnitName),
                            new XElement("PurchaseRowQuantity", row.Quantity),
                            new XElement("PurchaseRowDeliveredQuantity", row.DeliveredQuantity.GetValueOrDefault()),
                            new XElement("PurchaseRowIsFullyDelivered", row.Status == (int)SoeOriginStatus.PurchaseDeliveryCompleted),
                            new XElement("PurchaseRowPurchasePrice", row.PurchasePriceCurrency),
                            new XElement("PurchaseRowPurchasePriceBase", row.PurchasePrice),
                            new XElement("PurchaseRowAmount", row.SumAmountCurrency),
                            new XElement("PurchaseRowAmountBase", row.SumAmount),
                            new XElement("PurchaseRowVatAmount", row.VatAmountCurrency),
                            new XElement("PurchaseRowVatAmountBase", row.VatAmount),
                            new XElement("PurchaseRowSumAmountCurrency", row.SumAmountCurrency),
                            new XElement("PurchaseRowSumAmountBase", row.SumAmount),
                            new XElement("PurchaseRowDiscountPercent", row.DiscountPercent),
                            new XElement("PurchaseRowDiscountAmount", row.DiscountAmountCurrency),
                            new XElement("PurchaseRowDiscountAmountBase", row.DiscountAmountCurrency),
                            new XElement("PurchaseRowText", row.Text),
                            new XElement("PurchaseRowVatRate", row.VatRate),
                            new XElement("PurchaseRowDeliveryDate", rowDeliveryDate),
                            new XElement("PurchaseRowAccDeliveryDate", rowAccDeliveryDate),
                            new XElement("PurchaseRowWantedDeliveryDate", rowWantedDeliveryDate)
                            );
                        headEl.Add(rowEl);
                    }
                }
                else
                {
                    headEl.Add(new XElement("PurchaseRow"));
                }

                purchaseOrderElement.Add(headEl);

                #region Set ReportName

                if (supplierNameDict.Any())
                {
                    int min = supplierNameDict.Keys.Min();
                    int max = supplierNameDict.Keys.Max();
                    string reportPostfix = string.Empty;
                    if (min == max)
                        reportPostfix = $"{min} {supplierNameDict[min]}";
                    else
                        reportPostfix = $"{min} - {max}";

                    if (reportResult.IsMigrated)
                        reportResult.ReportNamePostfix = reportPostfix;
                    else
                        reportResult.EvaluatedSelection.ReportNamePostfix = reportPostfix;
                }

                #endregion

            }
            #endregion

            #region Close document

            rootElement.Add(purchaseOrderElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.PurchaseOrder);

            #endregion
        }

        private void SetPrintoutLanguage(CreateReportResult reportResult, int dataLanguageId)
        {
            if (reportResult.EvaluatedSelection != null && reportResult.EvaluatedSelection.SB_ReportLanguageId > 0)
            {
                reportLanguageId = reportResult.EvaluatedSelection.SB_ReportLanguageId;
            }
            else
            {
                reportLanguageId = dataLanguageId;
            }

            if (reportLanguageId == 0)
            {
                switch (Company.SysCountryId)
                {
                    case (int)TermGroup_Country.SE:
                        reportLanguageId = (int)TermGroup_Languages.Swedish;
                        break;
                    case (int)TermGroup_Country.FI:
                        reportLanguageId = (int)TermGroup_Languages.Finnish;
                        break;
                    case (int)TermGroup_Country.GB:
                        reportLanguageId = (int)TermGroup_Languages.English;
                        break;
                    case (int)TermGroup_Country.NO:
                        reportLanguageId = (int)TermGroup_Languages.Norwegian;
                        break;
                    case (int)TermGroup_Country.DK:
                        reportLanguageId = (int)TermGroup_Languages.Danish;
                        break;
                }
            }

            if (reportLanguageId == 0)
                reportLanguageId = GetLangId();

            SetLanguage(LanguageManager.GetSysLanguageCode(reportLanguageId));
        }

        #endregion

        #region Stock

        protected string GetStockInventoryHeaderText(BillingReportParamsDTO reportParams)
        {//du
            string text = "";
            if (reportParams.SB_StockInventoryId > 0)
            {
                using (CompEntities entities = new CompEntities())
                {
                    StockInventoryHead head = StockManager.GetStockInventory(reportParams.SB_StockInventoryId);
                    if (head != null)
                        text = head.HeaderText;
                }
            }

            return text;
        }

        protected XElement CreateStockInventoryReportHeaderElement(CreateReportResult reportResult, BillingReportParamsDTO reportParams)
        {
            string accountYearInterval = reportParams.GetAccountYearIntervalText();
            string accountPeriodInterval = reportParams.GetAccountPeriodIntervalText();
            string sortOrderName = this.GetBillingInvoiceSortOrderText(reportParams, (int)TermGroup.ReportBillingStockSortOrder);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(reportResult.ReportName),
                    this.CreateReportDescriptionElement(reportResult.ReportDescription),
                    this.CreateReportNrElement(reportResult.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    new XElement("AccountYear", accountYearInterval),
                    new XElement("AccountPeriod", accountPeriodInterval),
                    this.CreateLoginNameElement(reportResult.LoginName),
                    new XElement("SortOrder", reportParams.SB_SortOrder),
                    new XElement("SortOrderName", sortOrderName),
                    new XElement("StockInterval", CreateStockIntervalText(reportParams)),
                    new XElement("StockShelfInterval", CreateStockShelfIntervalText(reportParams)),
                    new XElement("StockProductInterval", CreateStockProductIntervalText(reportParams)),
                    new XElement("DateInterval", this.GetDateIntervalText(reportParams)),
                    new XElement("StockInventoryHeaderText", this.GetStockInventoryHeaderText(reportParams)),
                    new XElement("ProductGroupInterval", this.CreateProductGroupIntervalText(reportParams)),
                     new XElement("StockValueDate", this.GetStockValueDate(reportParams))
                    );
        }

        protected XElement CreateStockAndProductReportHeaderElement(CreateReportResult reportResult, BillingReportParamsDTO reportParams)
        {
            string accountPeriodInterval = reportParams.GetAccountPeriodIntervalText();
            string sortOrderName = GetBillingInvoiceSortOrderText(reportParams, (int)TermGroup.ReportBillingStockSortOrder);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(reportResult.ReportName),
                    this.CreateReportDescriptionElement(reportResult.ReportDescription),
                    this.CreateReportNrElement(reportResult.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    new XElement("AccountYear", string.Empty),
                    new XElement("AccountPeriod", accountPeriodInterval),
                    this.CreateLoginNameElement(reportResult.LoginName),
                    new XElement("SortOrder", reportParams.SB_SortOrder),
                    new XElement("SortOrderName", sortOrderName),
                    new XElement("StockInterval", CreateStockIntervalText(reportParams)),
                    new XElement("StockShelfInterval", CreateStockShelfIntervalText(reportParams)),
                    new XElement("StockProductInterval", CreateStockProductIntervalText(reportParams)),
                    new XElement("DateInterval", this.GetDateIntervalText(reportParams)),
                    new XElement("ProductGroupInterval", this.CreateProductGroupIntervalText(reportParams))
                    );
        }

        public XDocument CreateStockInventoryReportData(CreateReportResult reportResult)
        {
            #region Init document

            XDocument document = XmlUtil.CreateDocument();

            XElement rootElement = new XElement(ROOT + "_" + "StockInventory");

            XElement stockInventoryElement = new XElement("StockInventories");

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateStockInventoryReportHeaderLabelsElement();
            stockInventoryElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateStockInventoryReportHeaderElement(reportResult, reportParams);

            stockInventoryElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateStockInventoryReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            stockInventoryElement.Add(pageHeaderLabelsElement);

            #endregion


            using (var entities = new CompEntities())
            {
                #region ReportSettings

                var reportSettings = ReportManager.GetReportSettings(entities, reportResult.ReportId);
                var stockInventoryExcludeZeroQuantity = reportSettings.FirstOrDefault(x => x.Type == TermGroup_ReportSettingType.StockInventoryExcludeZeroQuantity)?.BoolData ?? false;

                #endregion

                #region Content

                List<StockInventoryHeadDTO> heads;
                if (reportParams.SB_StockInventoryId > 0)
                {
                    heads = new List<StockInventoryHeadDTO>() { StockManager.GetStockInventoryHeadDTO(entities, reportParams.SB_StockInventoryId) };
                }
                else
                {
                    heads = StockManager.GetStockInventoryHeadDTOs(entities, reportResult.ActorCompanyId);
                }

                foreach (var head in heads)
                {
                    #region StockInventoryHead

                    if ((reportParams.SB_StockLocationIdFrom > 0 && reportParams.SB_StockLocationIdTo > 0) && (head.StockId < reportParams.SB_StockLocationIdFrom || head.StockId > reportParams.SB_StockLocationIdTo))
                        continue;

                    XElement stockElement = new XElement("StockInventoryHead",
                        new XElement("StockInventoryHeadId", head.StockInventoryHeadId),
                        new XElement("HeaderText", head.HeaderText),
                        new XElement("StockCode", head.StockCode ?? string.Empty),
                        new XElement("StockName", head.StockName ?? string.Empty),
                        new XElement("StartDate", head.InventoryStart ?? CalendarUtility.DATETIME_DEFAULT),
                        new XElement("StopDate", head.InventoryStop ?? CalendarUtility.DATETIME_DEFAULT),
                        new XElement("Created", head.Created ?? CalendarUtility.DATETIME_DEFAULT),
                        new XElement("CreatedBy", head.CreatedBy),
                        new XElement("Modified", head.Modified ?? CalendarUtility.DATETIME_DEFAULT),
                        new XElement("ModifiedBy", head.ModifiedBy)
                     );

                    #endregion

                    #region StockInventoryRows

                    var stockInventoryRows = StockManager.GetStockInventoryRowDTOs(head.StockInventoryHeadId);

                    if (stockInventoryExcludeZeroQuantity)
                        stockInventoryRows = stockInventoryRows.Where(x => x.StartingSaldo != 0 || x.InventorySaldo != 0).ToList();

                    foreach (StockInventoryRowDTO row in stockInventoryRows)
                    {
                        #region StockInventoryRow

                        if ((reportParams.SB_StockShelfIdFrom > 0 && reportParams.SB_StockShelfIdTo > 0) && (row.ShelfId < reportParams.SB_StockShelfIdFrom || row.ShelfId > reportParams.SB_StockShelfIdTo))
                            continue;
                        if (!string.IsNullOrWhiteSpace(reportParams.SB_ProductNrFrom) && !string.IsNullOrWhiteSpace(reportParams.SB_ProductNrTo) && !StringUtility.IsInInterval(row.ProductNumber, reportParams.SB_ProductNrFrom, reportParams.SB_ProductNrTo))
                            continue;
                        if (reportParams.HasDateInterval && row.Created.HasValue && (string.Compare(row.Created.Value.ToString("yyyyMMdd"), reportParams.DateFrom.Date.ToString("yyyyMMdd")) < 0 || String.Compare(row.Created.Value.ToString("yyyyMMdd"), reportParams.DateTo.Date.ToString("yyyyMMdd")) > 0))
                            continue;

                        // Filter on product group
                        if (!string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupFrom) && !string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupTo) && !StringUtility.IsInInterval(row.ProductGroupCode ?? "", reportParams.SB_ProductGroupFrom, reportParams.SB_ProductGroupTo))
                            continue;
                        if (!string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupFrom) && string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupTo) && !StringUtility.IsGreater(row.ProductGroupCode ?? "", reportParams.SB_ProductGroupFrom))
                            continue;
                        if (string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupFrom) && !string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupTo) && StringUtility.IsGreater(row.ProductGroupCode ?? "", reportParams.SB_ProductGroupTo))
                            continue;

                        var valueDateSaldo = row.StartingSaldo;

                        if (reportParams.SB_StockValueDate != CalendarUtility.DATETIME_MINVALUE && reportParams.SB_StockValueDate != DateTime.MinValue)
                        {
                            var startTransData = head.InventoryStart;
                            if (row.TransactionDate.HasValue && row.TransactionDate.Value < reportParams.SB_StockValueDate)
                            {
                                startTransData = row.TransactionDate.Value;
                                valueDateSaldo = row.InventorySaldo;
                            }
                            var stockTransactions = StockManager.GetStockTransactionDTOs(row.StockProductId, startTransData, reportParams.SB_StockValueDate);
                            valueDateSaldo += StockManager.CalculateTransactionNetSaldo(stockTransactions);
                        }

                        XElement productElement = new XElement("StockInventoryRow",
                            new XElement("StockInventoryRowId", row.StockInventoryRowId),
                            new XElement("StockInventoryHeadId", row.StockInventoryHeadId),
                            new XElement("InvoiceProductNr", row.ProductNumber),
                            new XElement("InvoiceProductName", row.ProductName),
                            new XElement("InvoiceProductUnitName", row.Unit),
                            new XElement("StockShelfCode", row.ShelfCode),
                            new XElement("StockShelfName", row.ShelfName),
                            new XElement("StartingSaldo", row.StartingSaldo),
                            new XElement("InventorySaldo", row.InventorySaldo),
                            new XElement("OrderedQantity", row.OrderedQuantity),
                            new XElement("ReservedQantity", row.ReservedQuantity),
                            new XElement("Difference", row.Difference),
                            new XElement("Price", row.AvgPrice),
                            new XElement("RowCreated", row.Created ?? CalendarUtility.DATETIME_DEFAULT),
                            new XElement("RowCreatedBy", row.CreatedBy),
                            new XElement("RowModified", row.Modified ?? CalendarUtility.DATETIME_DEFAULT),
                            new XElement("RowModifiedBy", row.ModifiedBy),
                            new XElement("ProductGroupCode", row.ProductGroupCode ?? string.Empty),
                            new XElement("ProductGroupName", row.ProductGroupName),
                            new XElement("ValueDateSaldo", valueDateSaldo),
                            new XElement("TransactionDate", row.TransactionDate ?? row.Created)
                            );

                        stockElement.Add(productElement);

                        #endregion
                    }

                    stockInventoryElement.Add(stockElement);

                    #endregion
                }

                #endregion

                #region Close document

                rootElement.Add(stockInventoryElement);
                document.Add(rootElement);

#if DEBUG
                //System.IO.File.WriteAllText(@"c:\Temp\report\CreateStockInventoryReportData.xml", document.ToString());
#endif

                return GetValidatedDocument(document, SoeReportTemplateType.StockInventoryReport);

                #endregion
            }
        }

        public XDocument CreateStockTransactionListReportData(CreateReportResult reportResult)
        {

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "StockTransaction");

            XElement stockAndProductElement = new XElement("StockTransactions");

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateStockAndProjectReportHeaderLabelsElement();
            stockAndProductElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateStockAndProductReportHeaderElement(reportResult, reportParams);
            stockAndProductElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateStockTransactionReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            stockAndProductElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Content

                List<Stock> stocks = StockManager.GetStocks(reportResult.ActorCompanyId);
                foreach (Stock stock in stocks)
                {
                    #region Stock

                    if ((reportParams.SB_StockLocationIdFrom > 0 && reportParams.SB_StockLocationIdTo > 0) && (stock.StockId < reportParams.SB_StockLocationIdFrom || stock.StockId > reportParams.SB_StockLocationIdTo))
                        continue;

                    XElement stockElement = new XElement("StockProduct",
                        new XElement("StockId", stock.StockId),
                        new XElement("Code", stock.Code),
                        new XElement("Name", stock.Name),
                        new XElement("State", stock.State)
                     );

                    #endregion

                    #region StockProducts

                    List<StockProductDTO> stockProducts = StockManager.GetStockProductDTOs(reportResult.ActorCompanyId, null, stock.StockId);
                    foreach (StockProductDTO stockProduct in stockProducts)
                    {
                        #region StockProduct

                        if ((reportParams.SB_StockLocationIdFrom > 0 && reportParams.SB_StockLocationIdTo > 0) && (stockProduct.StockId < reportParams.SB_StockLocationIdFrom || stockProduct.StockId > reportParams.SB_StockLocationIdTo))
                            continue;
                        if ((reportParams.SB_StockShelfIdFrom > 0 && reportParams.SB_StockShelfIdTo > 0) && (stockProduct.StockShelfId < reportParams.SB_StockShelfIdFrom || stockProduct.StockShelfId > reportParams.SB_StockShelfIdTo))
                            continue;
                        if (!string.IsNullOrWhiteSpace(reportParams.SB_ProductNrFrom) && !string.IsNullOrWhiteSpace(reportParams.SB_ProductNrTo) && !StringUtility.IsInInterval(stockProduct.ProductNumber, reportParams.SB_ProductNrFrom, reportParams.SB_ProductNrTo))
                            continue;

                        // Filter on product group
                        if (!string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupFrom) && !string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupTo) && !StringUtility.IsInInterval(stockProduct.ProductGroupCode ?? "", reportParams.SB_ProductGroupFrom, reportParams.SB_ProductGroupTo))
                            continue;
                        if (!string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupFrom) && string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupTo) && !StringUtility.IsGreater(stockProduct.ProductGroupCode ?? "", reportParams.SB_ProductGroupFrom))
                            continue;
                        if (string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupFrom) && !string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupTo) && StringUtility.IsGreater(stockProduct.ProductGroupCode ?? "", reportParams.SB_ProductGroupTo))
                            continue;

                        List<GenericType> actionTypes = base.GetTermGroupContent(TermGroup.StockTransactionType).ToList();
                        List<TermGroup_StockTransactionType> types = null;
                        if (reportParams.SB_StockTransactionType > 0)
                        {
                            types = new List<TermGroup_StockTransactionType>() { (TermGroup_StockTransactionType)reportParams.SB_StockTransactionType };
                        }

                        List<StockTransactionDTO> stockTransactions = StockManager.GetStockTransactionDTOs(stockProduct.StockProductId, reportParams.HasDateInterval ? reportParams.DateFrom.Date : (DateTime?)null, reportParams.HasDateInterval ? reportParams.DateTo.Date : (DateTime?)null, true, types);

                        foreach (StockTransactionDTO stockTransaction in stockTransactions)
                        {
                            #region StockTransaction

                            long voucherNr = 0;
                            if (stockTransaction.VoucherId.GetValueOrDefault() > 0)
                                voucherNr = VoucherManager.GetVoucherHead(stockTransaction.VoucherId.Value)?.VoucherNr ?? 0;

                            XElement productElement = new XElement("StockTransaction",
                                new XElement("InvoiceProductId", stockProduct.InvoiceProductId),
                                new XElement("InvoiceProductNr", stockProduct.ProductNumber),
                                new XElement("InvoiceProductName", stockProduct.ProductName),
                                new XElement("InvoiceProductUnitName", stockProduct.ProductUnit),
                                new XElement("ActionType", (int)stockTransaction.ActionType),
                                new XElement("ActionTypeName", actionTypes.FirstOrDefault(x => x.Id == (int)stockTransaction.ActionType)?.Name ?? string.Empty),
                                new XElement("Quantity", stockTransaction.Quantity),
                                new XElement("Price", stockTransaction.Price),
                                new XElement("Note", stockTransaction.Note),
                                new XElement("StockShelfId", stockProduct.StockShelfId.ToInt()),
                                new XElement("StockShelfName", stockProduct.StockShelfName),
                                new XElement("VoucherId", stockTransaction.VoucherId.GetValueOrDefault()),
                                new XElement("VoucherNr", voucherNr),
                                new XElement("Created", stockTransaction.TransactionDate?.ToShortDateString() ?? string.Empty),
                                new XElement("CreatedBy", stockTransaction.CreatedBy),
                                new XElement("ProductGroupCode", stockProduct.ProductGroupCode ?? string.Empty),
                                new XElement("ProductGroupName", stockProduct.ProductGroupName ?? string.Empty),
                               new XElement("SourceLabel", stockTransaction.SourceLabel),
                               new XElement("SourceNr", stockTransaction.SourceNr)

                            );

                            stockElement.Add(productElement);

                            #endregion
                        }
                        #region Default element

                        if (stockTransactions.Count == 0)
                        {
                            XElement productElement = new XElement("StockTransaction",
                                new XElement("InvoiceProductId", stockProduct.InvoiceProductId),
                                new XElement("InvoiceProductNr", stockProduct.ProductNumber),
                                new XElement("InvoiceProductName", stockProduct.ProductName),
                                new XElement("InvoiceProductUnitName", stockProduct.ProductUnit),
                                new XElement("ActionType", 0),
                                new XElement("ActionTypeName", string.Empty),
                                new XElement("Quantity", 0),
                                new XElement("Price", 0),
                                new XElement("Note", string.Empty),
                                new XElement("StockShelfId", stockProduct.StockShelfId.ToInt()),
                                new XElement("StockShelfName", stockProduct.StockShelfName),
                                new XElement("VoucherId", 0),
                                new XElement("VoucherNr", 0),
                                new XElement("Created", string.Empty),
                                new XElement("CreatedBy", string.Empty),
                                new XElement("ProductGroupCode", stockProduct.ProductGroupCode ?? string.Empty),
                                new XElement("ProductGroupName", stockProduct.ProductGroupName ?? string.Empty)
                                );
                            stockElement.Add(productElement);
                        }
                        #endregion
                        #endregion
                    }

                    stockAndProductElement.Add(stockElement);

                    #endregion
                }

                #endregion

                #region Close document

                rootElement.Add(stockAndProductElement);
                document.Add(rootElement);

#if DEBUG
                //System.IO.File.WriteAllText(@"c:\Temp\report\CreateStockTransactionListReportData.xml", document.ToString());
#endif

                return GetValidatedDocument(document, SoeReportTemplateType.StockTransactionListReport);

                #endregion
            }
        }

        public XDocument CreateStockSaldoListReportData(CreateReportResult reportResult)
        {
            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "StockAndProduct");

            //CustomerBalanceList
            XElement stockAndProductElement = new XElement("StockAndProduct");

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateStockAndProjectReportHeaderLabelsElement();
            stockAndProductElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateStockAndProductReportHeaderElement(reportResult, reportParams);
            stockAndProductElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateStockAndProductReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            stockAndProductElement.Add(pageHeaderLabelsElement);

            #endregion

            using (var entities = new CompEntities())
            {
                #region ReportSettings

                var reportSettings = ReportManager.GetReportSettings(entities, reportResult.ReportId);
                var excludeItemsWithZeroQuantityForSpecificDate = reportSettings.FirstOrDefault(x => x.Type == TermGroup_ReportSettingType.ExcludeItemsWithZeroQuantityForSpecificDate)?.BoolData ?? false;

                #endregion

                #region Content

                List<Stock> stocks = StockManager.GetStocks(reportResult.ActorCompanyId);
                foreach (Stock stock in stocks)
                {
                    #region Stock

                    if ((reportParams.SB_StockLocationIdFrom > 0 && reportParams.SB_StockLocationIdTo > 0) && (stock.StockId < reportParams.SB_StockLocationIdFrom || stock.StockId > reportParams.SB_StockLocationIdTo))
                        continue;

                    XElement stockElement = new XElement("Stock",
                        new XElement("StockId", stock.StockId),
                        new XElement("Code", stock.Code),
                        new XElement("Name", stock.Name),
                        new XElement("State", stock.State)
                     );

                    #endregion

                    #region StockProducts

                    List<StockProductDTO> stockProducts = StockManager.GetStockProductDTOs(reportResult.ActorCompanyId, null, stock.StockId, true);

                    //Sort stockProducts according to given SortOrder
                    switch (reportParams.SB_SortOrder)
                    {
                        case 1:
                            stockProducts = stockProducts.OrderBy(x => x.StockName).ThenBy(x => x.ProductNumber).ToList();
                            break;
                        case 2:
                            stockProducts = stockProducts.OrderBy(x => x.StockShelfName).ThenBy(x => x.ProductNumber).ToList();
                            break;
                        case 3:
                            stockProducts = stockProducts.OrderBy(x => x.ProductNumber).ToList();
                            break;
                        case 4:
                            stockProducts = stockProducts.OrderBy(x => x.ProductName).ToList();
                            break;
                        case 5:
                            stockProducts = stockProducts.OrderBy(x => x.ProductGroupCode).ToList();
                            break;
                    }

                    foreach (StockProductDTO stockProduct in stockProducts)
                    {
                        #region StockProduct

                        if ((reportParams.SB_StockShelfIdFrom > 0 && reportParams.SB_StockShelfIdTo > 0) && (stockProduct.StockShelfId < reportParams.SB_StockShelfIdFrom || stockProduct.StockShelfId > reportParams.SB_StockShelfIdTo))
                            continue;

                        if (!string.IsNullOrWhiteSpace(reportParams.SB_ProductNrFrom) && !string.IsNullOrWhiteSpace(reportParams.SB_ProductNrTo) && !StringUtility.IsInInterval(stockProduct.ProductNumber, reportParams.SB_ProductNrFrom, reportParams.SB_ProductNrTo))
                            continue;

                        // Filter on product group
                        if (!string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupFrom) && !string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupTo) && !StringUtility.IsInInterval(stockProduct.ProductGroupCode ?? "", reportParams.SB_ProductGroupFrom, reportParams.SB_ProductGroupTo))
                            continue;
                        if (!string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupFrom) && string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupTo) && !StringUtility.IsGreater(stockProduct.ProductGroupCode ?? "", reportParams.SB_ProductGroupFrom))
                            continue;
                        if (string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupFrom) && !string.IsNullOrWhiteSpace(reportParams.SB_ProductGroupTo) && StringUtility.IsGreater(stockProduct.ProductGroupCode ?? "", reportParams.SB_ProductGroupTo))
                            continue;

                        if (reportParams.HasDateInterval)
                        {
                            List<StockTransactionDTO> stockTransactions = StockManager.GetStockTransactionDTOs(stockProduct.StockProductId, reportParams.DateTo, null).Where(x => x.TransactionDate > reportParams.DateTo).ToList();
                            foreach (StockTransactionDTO stockTransaction in stockTransactions.OrderByDescending(x => x.TransactionDate))
                            {
                                //quantity changes...
                                switch ((int)stockTransaction.ActionType)
                                {
                                    case ((int)TermGroup_StockTransactionType.Add):
                                    case ((int)TermGroup_StockTransactionType.Correction):
                                        stockProduct.Quantity -= stockTransaction.Quantity;
                                        break;
                                    case ((int)TermGroup_StockTransactionType.Take):
                                    case ((int)TermGroup_StockTransactionType.Loss):
                                        //      case ((int)TermGroup_StockTransactionType.Reserve):
                                        stockProduct.Quantity += stockTransaction.Quantity;
                                        break;
                                }
                            }

                            var lastAvgPriceTransaction = StockManager.GetStockTransactionSmallDTOs(entities, stockProduct.StockProductId, null, reportParams.DateTo, new List<TermGroup_StockTransactionType> { TermGroup_StockTransactionType.Add, TermGroup_StockTransactionType.AveragePriceChange }, true, 1).FirstOrDefault();
                            if (lastAvgPriceTransaction != null && lastAvgPriceTransaction.AvgPrice != 0)
                            {
                                stockProduct.AvgPrice = lastAvgPriceTransaction.AvgPrice;
                            }
                            else if (lastAvgPriceTransaction != null && lastAvgPriceTransaction.Price != 0)
                            {
                                stockProduct.AvgPrice = lastAvgPriceTransaction.Price;
                            }
                      
                            if ((excludeItemsWithZeroQuantityForSpecificDate || stockProduct.ProductState != (int)SoeEntityState.Active) && stockProduct.Quantity <= 0)
                                continue;
                        }

                        #endregion

                        #region StockProduct element

                        XElement productElement = new XElement("StockProduct",
                            new XElement("StockProductId", stockProduct.StockProductId),
                            new XElement("InvoiceProductId", stockProduct.InvoiceProductId),
                            new XElement("InvoiceProductNr", stockProduct.ProductNumber),
                            new XElement("InvoiceProductName", stockProduct.ProductName),
                            new XElement("InvoiceProductUnitName", stockProduct.ProductUnit),
                            new XElement("Quantity", stockProduct.Quantity),
                            new XElement("OrderedQuantity", stockProduct.OrderedQuantity),
                            new XElement("ReservedQuantity", stockProduct.ReservedQuantity),
                            new XElement("IsInInventory", Convert.ToInt32(stockProduct.IsInInventory)),
                            new XElement("WarningLevel", 0),
                            new XElement("AvgPrice", stockProduct.AvgPrice),
                            new XElement("StockShelfId", stockProduct.StockShelfId.ToInt()),
                            new XElement("StockShelfName", stockProduct.StockShelfName),
                            new XElement("ProductGroupCode", stockProduct.ProductGroupCode ?? string.Empty),
                            new XElement("ProductGroupName", stockProduct.ProductGroupName ?? string.Empty)
                        );


                        stockElement.Add(productElement);

                        #endregion
                    }

                    stockAndProductElement.Add(stockElement);

                    #endregion
                }

                #endregion

                #region Close document

                rootElement.Add(stockAndProductElement);
                document.Add(rootElement);

#if DEBUG
                //System.IO.File.WriteAllText(@"c:\Temp\report\CreateStockSaldoListReportData.xml", document.ToString());
#endif

                return GetValidatedDocument(document, SoeReportTemplateType.StockSaldoListReport);

                #endregion
            }
        }

        #endregion

        #region Product report

        public XDocument CreateProductReportData(CreateReportResult reportResult)
        {
            #region Prereq

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "ProductReport");

            //ProductList
            XElement productReportElement = new XElement("ProductReport");

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateActorReportHeaderLabelsElement();
            productReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateProductReportHeaderElement(reportResult, reportParams);
            productReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            productReportElement.Add(pageHeaderLabelsElement);

            #endregion

            #region AccountDimensions

            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(Company.ActorCompanyId, false, false, true);
            foreach (AccountDim accountDim in accountDims)
            {
                XElement accountDimElement = new XElement("AccountDims",
                    new XElement("AccountDimNr", accountDim.AccountDimNr),
                    new XElement("AccountSieDimNr", accountDim.SysSieDimNr),
                    new XElement("AccountDimName", accountDim.Name));

                productReportElement.Add(accountDimElement);
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Content

                int productXmlId = 1;
                var invoiceProducts = ImportExportManager.GetInvoiceProductIODTOs(reportResult.ActorCompanyId, reportParams.SB_ProductNrFrom, reportParams.SB_ProductNrTo, null, null, false, reportParams.SB_ProductIds);
                foreach (var invoiceProduct in invoiceProducts)
                {
                    #region Product element

                    XElement productElement = new XElement("Product",
                        new XAttribute("Id", productXmlId),
                        new XElement("ProductNr", invoiceProduct.Number),
                        new XElement("ProductName", invoiceProduct.Name),
                        new XElement("Unit", invoiceProduct.Unit),
                        new XElement("EAN", invoiceProduct.EAN),
                        new XElement("Description", invoiceProduct.Description),
                        new XElement("Type", invoiceProduct.VatType),
                        new XElement("Weight", invoiceProduct.Weight),
                        new XElement("Purchaseprice", invoiceProduct.PurchasePrice));

                    #endregion

                    #region Salesprices element

                    int salespriceXmlId = 1;
                    if (!invoiceProduct.PriceDTOs.IsNullOrEmpty())
                    {
                        foreach (var priceItem in invoiceProduct.PriceDTOs)
                        {
                            XElement salespriceElement = new XElement("Salesprices",
                                new XAttribute("Id", salespriceXmlId),
                                new XElement("PriceListCode", priceItem.PriceListCode),
                                new XElement("Price", priceItem.Price),
                                new XElement("StartDate", priceItem.StartDate),
                                new XElement("StopDate", priceItem.StopDate));

                            productElement.Add(salespriceElement);
                            salespriceXmlId++;
                        }
                    }

                    #endregion

                    productReportElement.Add(productElement);
                    productXmlId++;
                }

                #endregion

                #region Default Product Element

                if (productXmlId == 1)
                {
                    XElement productElement = new XElement("Product",
                        new XAttribute("Id", 1),
                        new XElement("ProductNr", String.Empty),
                        new XElement("ProductName", String.Empty),
                        new XElement("Unit", String.Empty),
                        new XElement("EAN", String.Empty),
                        new XElement("Description", String.Empty),
                        new XElement("Type", String.Empty),
                        new XElement("Weight", String.Empty),
                        new XElement("Purchaseprice", String.Empty));

                    XElement salespriceElement = new XElement("Salesprices",
                        new XAttribute("Id", 1),
                        new XElement("PriceListCode", String.Empty),
                        new XElement("Price", String.Empty),
                        new XElement("StartDate", String.Empty),
                        new XElement("StopDate", String.Empty));

                    productElement.Add(salespriceElement);
                    productReportElement.Add(productElement);
                }

                #endregion
            }

            #region Close document

            rootElement.Add(productReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.ProductListReport);

            #endregion
        }

        protected string GetLedgerActorIntervalText(BillingReportParamsDTO reportParams)
        {
            string text = "";
            if (reportParams.SL_HasActorNrInterval)
                text = reportParams.SL_ActorNrFrom + "-" + reportParams.SL_ActorNrTo;
            return text;
        }

        protected XElement CreateProductReportHeaderElement(CreateReportResult reportResult, BillingReportParamsDTO reportParams)
        {
            Currency currency = CountryCurrencyManager.GetCurrencyFromType(Company.ActorCompanyId, TermGroup_CurrencyType.EnterpriseCurrency);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(reportResult.ReportName),
                    this.CreateReportDescriptionElement(reportResult.ReportDescription),
                    this.CreateReportNrElement(reportResult.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(reportResult.LoginName),
                    new XElement("DateRegardName", this.GetLedgerDateRegardText(reportParams)),
                    new XElement("SortOrderName", this.GetCustomerSortOrderText(reportParams)),
                    new XElement("InvoiceSelectionName", this.GetLedgerInvoiceSelectionText(reportParams)),
                    new XElement("DateInterval", this.GetDateIntervalText(reportParams)),
                    new XElement("InvoiceInterval", this.GetLedgerInvoiceIntervalText(reportParams)),
                    new XElement("ProductInterval", this.GetLedgerActorIntervalText(reportParams)),
                    new XElement("EntCurrencyName", currency?.Name ?? ""),
                    new XElement("EntCurrencyCode", currency?.Code ?? ""));
        }

        protected string GetLedgerDateRegardText(BillingReportParamsDTO reportParams)
        {
            return GetText(reportParams.SL_DateRegard, (int)TermGroup.ReportLedgerDateRegard);
        }

        protected string GetCustomerSortOrderText(BillingReportParamsDTO reportParams)
        {
            return GetText(reportParams.SL_SortOrder, (int)TermGroup.ReportCustomerLedgerSortOrder);
        }
        protected string GetLedgerInvoiceSelectionText(BillingReportParamsDTO reportParams)
        {
            return GetText(reportParams.SL_InvoiceSelection, (int)TermGroup.ReportLedgerInvoiceSelection);
        }
        protected string GetLedgerInvoiceIntervalText(BillingReportParamsDTO reportParams)
        {
            string text = "";
            if (reportParams.SL_HasInvoiceSeqNrInterval)
                text = reportParams.SL_InvoiceSeqNrFrom + "-" + reportParams.SL_InvoiceSeqNrTo;
            else if (reportParams.SL_HasInvoiceIds)
                using (CompEntities entities = new CompEntities())
                {
                    foreach (int invoiceId in reportParams.SL_InvoiceIds)
                    {
                        var invoiceTinyDTO = SupplierInvoiceManager.GetSupplierInvoiceTiny(entities, invoiceId);
                        if (string.IsNullOrEmpty(text))
                        {
                            text = invoiceTinyDTO.SeqNr.ToString();
                        }
                        else
                        {
                            text = text + "," + invoiceTinyDTO.SeqNr.ToString();
                        }
                    }
                }
            return text;
        }
        #endregion

        #region Project reports

        protected XElement CreateProjectReportHeaderElement(CreateReportResult es)
        {
            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(es.LoginName));
        }

        private List<XElement> CreateProjectTimeReportDataContentFromProjectTimeBlocks(CreateReportResult reportResult, BillingReportParamsDTO reportParam, ref int projectXmlId)
        {
            int timeInvoiceTransactionXmlId = 1;
            int timePayrollTransactionXmlId = 1;
            List<XElement> projectElements = new List<XElement>();

            #region Prereq

            //Get TimeProject's
            List<TimeProject> projects = ProjectManager.GetTimeProjectsFromSelection(reportResult, reportParam);
            List<int> projectIds = projects.Select(x => x.ProjectId).ToList();

            //Build collections
            List<int> invoiceIdsAll = new List<int>();
            List<int> employeeIds;
            Dictionary<int, List<int>> projectInvoicesDict = new Dictionary<int, List<int>>();

            if (string.IsNullOrEmpty(reportParam.SB_EmployeeNrFrom))
                employeeIds = EmployeeManager.GetAllEmployeeIds(reportResult.ActorCompanyId);
            else
                employeeIds = EmployeeManager.GetAllEmployeeIdsByEmployeeNrFromTo(reportResult.ActorCompanyId, reportParam.SB_EmployeeNrFrom, reportParam.SB_EmployeeNrTo);

            var timeInvoiceTransactions = ProjectManager.GetProjectTimeBlockInvoiceTransactionsFromSelection(reportResult, reportParam, projectIds, employeeIds);

            var timePayrollTransactions = ProjectManager.GetProjectTimeBlockDTOs(reportParam.DateFrom, reportParam.DateTo, employeeIds, projectIds, null, false);
            var tempIds = timePayrollTransactions.Select(t => t.ProjectId).ToList();
            tempIds.AddRange(timeInvoiceTransactions.Select(t => t.ProjectId));

            //var tempIds = timePayrollTransactions.Select(t => t.ProjectId).ToList();
            //tempIds.AddRange(timeInvoiceTransactions.Select(t => t.ProjectId));

            //Filter
            projectIds = tempIds.Distinct().ToList();

            // Map invoices
            ProjectManager.GetProjectInvoiceMappingIds(reportResult.ActorCompanyId, projectIds, ref invoiceIdsAll, ref projectInvoicesDict);

            //Get all CustomerInvoices for Projects
            var invoiceItems = ProjectManager.GetInvoicesForTimeProjectsFromSelection(invoiceIdsAll, reportResult, reportParam);

            #endregion

            #region Content

            foreach (TimeProject project in projects.Where(p => projectIds.Contains(p.ProjectId)))
            {
                #region Prereq

                //InvoiceId's for current TimeProject
                List<int> invoiceIdsForProject = (from p in projectInvoicesDict
                                                  where p.Key == project.ProjectId
                                                  select p.Value).FirstOrDefault();

                //if we have made a selection on customernr we only want those projects that has an invoice that is connected to the choosen customers
                if (reportParam.SB_HasActorNrInterval)
                {
                    bool jumpOverProject = true;
                    foreach (var invoice in invoiceItems)
                    {
                        if (invoiceIdsForProject != null && invoiceIdsForProject.Contains(invoice.InvoiceId))
                        {
                            jumpOverProject = false;
                            break;
                        }
                    }

                    if (jumpOverProject)
                        continue;
                }

                #endregion

                #region Invoice

                List<XElement> invoiceElements = new List<XElement>();
                int invoiceXmlId = 1;

                foreach (var invoiceItem in invoiceItems)
                {
                    if (invoiceIdsForProject == null)
                        continue;

                    //Check that CustomerInvoice belongs to current TimeProject
                    if (!invoiceIdsForProject.Contains(invoiceItem.InvoiceId))
                        continue;

                    #region CustomerInvoice

                    #region Invoice

                    XElement invoiceElement = new XElement("Invoice",
                            new XAttribute("id", invoiceXmlId),
                            new XElement("InvoiceNr", invoiceItem.InvoiceNr),
                            new XElement("InvoiceCreated", invoiceItem.Created.HasValue ? invoiceItem.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("InvoiceCreatedBy", invoiceItem.CreatedBy),
                            new XElement("InvoiceCustomerNr", invoiceItem.CustomerNr),
                            new XElement("InvoiceCustomerName", invoiceItem.CustomerName));

                    #endregion

                    #region DefaultInvoiceRow

                    XElement invoiceRowElement = new XElement("CustomerInvoiceRow",
                                new XAttribute("id", 1),
                                new XElement("InvoiceRowCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                new XElement("InvoiceRowCreatedBy", ""),
                                new XElement("InvoiceRowProductNumber", ""),
                                new XElement("InvoiceRowProductName", ""),
                                new XElement("InvoiceRowProductDescription", ""),
                                new XElement("InvoiceRowUnitName", ""));

                    invoiceElement.Add(invoiceRowElement);

                    #endregion

                    invoiceElements.Add(invoiceElement);
                    invoiceXmlId++;

                    #endregion
                }

                #endregion

                if (invoiceXmlId == 1)
                {
                    #region Default element Invoice

                    XElement defInvoiceElement = new XElement("Invoice",
                            new XAttribute("id", 1),
                            new XElement("InvoiceNr", ""),
                            new XElement("InvoiceCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("InvoiceCreatedBy", "")
                            );
                    XElement defInvoiceRowElement = new XElement("CustomerInvoiceRow",
                           new XAttribute("id", 1),
                           new XElement("InvoiceRowCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                           new XElement("InvoiceRowCreatedBy", ""),
                           new XElement("InvoiceRowProductNumber", ""),
                           new XElement("InvoiceRowProductName", ""),
                           new XElement("InvoiceRowProductDescription", ""),
                           new XElement("InvoiceRowUnitName", "")
                           );

                    defInvoiceElement.Add(defInvoiceRowElement);
                    invoiceElements.Add(defInvoiceElement);

                    #endregion
                }

                #region ProjectInvoiceDay

                List<XElement> invoiceTransactionElements = new List<XElement>();
                List<XElement> payrollTransactionElements = new List<XElement>();

                var timePayrollTransactionsForProject = timePayrollTransactions.Where(p => p.ProjectId == project.ProjectId);
                var timeInvoiceTransactionsForProject = timeInvoiceTransactions.Where(p => p.ProjectId == project.ProjectId);

                if (!timePayrollTransactionsForProject.Any() && !timeInvoiceTransactionsForProject.Any())
                    continue;

                #region TimePayrollTransaction

                foreach (var payrollTransaction in timePayrollTransactionsForProject)
                {
                    /*
                    if (!ProjectManager.IsProjectTimeBlockWorkTime(payrollTransaction.SysPayrollTypeLevel1, payrollTransaction.SysPayrollTypeLevel2, payrollTransaction.SysPayrollTypeLevel3, payrollTransaction.SysPayrollTypeLevel4) )
                    {
                        continue;
                    }
                    */
                    int weeknr = CalendarUtility.GetWeekNr(payrollTransaction.Date);

                    XElement payrollTransactionElement = new XElement("TimePayrollTransaction",
                                             new XAttribute("id", timePayrollTransactionXmlId),
                                             new XElement("EmployeeNr", payrollTransaction.EmployeeNr),
                                             new XElement("EmployeeName", payrollTransaction.EmployeeName),
                                             new XElement("Date", payrollTransaction.Date.ToShortDateString()),
                                             new XElement("Note", payrollTransaction.InternalNote),
                                             new XElement("ExternalNote", payrollTransaction.ExternalNote),
                                             new XElement("PayrollTransactionQuantity", payrollTransaction.TimePayrollQuantity),
                                             new XElement("PayrollTransactionCreated", payrollTransaction.Created.HasValue ? payrollTransaction.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                             new XElement("PayrollTransactionCreatedBy", payrollTransaction.CreatedBy),
                                             new XElement("PayrollTransactionExported", 0),
                                             new XElement("PayrollTransactionProductNumber", string.Empty),
                                             new XElement("PayrollTransactionProductName", string.Empty),
                                             new XElement("PayrollTransactionProductDescription", string.Empty),
                                             new XElement("WeekNumber", weeknr),
                                             new XElement("IsoDate", payrollTransaction.Date.ToString("yyyy-MM-dd")),
                                             new XElement("PayrollTransactionTimeCodeCode", payrollTransaction.TimeCodeId),
                                             new XElement("PayrollTransactionTimeCodeName", payrollTransaction.TimeCodeName),
                                             new XElement("PayrollTransactionTimeCodeDescription", string.Empty),
                                             new XElement("PayrollTransactionTimeCodeTransactionId", payrollTransaction.TimePayrollTransactionIds),
                                             new XElement("PayrollTransactionTimeCodeTransactionQuantity", payrollTransaction.TimePayrollQuantity),
                                             new XElement("PayrollTransactionTimeCodeTransactionInvoiceQuantity", payrollTransaction.InvoiceQuantity),
                                             new XElement("PayrollTransactionTimeCodeTransactionProjectId", payrollTransaction.ProjectId),
                                             new XElement("PayrollTransactionTimeCodeTransactionProjectNumber", payrollTransaction.ProjectNr),
                                             new XElement("PayrollTransactionTimeCodeTransactionProjectName", payrollTransaction.ProjectName),
                                             new XElement("PayrollTransactionTimeCodeTransactionProjectNote", string.Empty),
                                             new XElement("PayrollTransactionTimeCodeTransactionParentProjectNumber", 0),
                                             new XElement("PayrollTransactionTimeCodeTransactionParentProjectName", string.Empty),
                                             new XElement("PayrollType", 0),
                                             new XElement("isPayed", 0),
                                             new XElement("InvoiceNr", string.IsNullOrEmpty(payrollTransaction.InvoiceNr) ? "" : payrollTransaction.InvoiceNr),
                                             new XElement("OriginType", 0),
                                             new XElement("PayrollTransactionInvoiceCustomerName", string.IsNullOrEmpty(payrollTransaction.CustomerName) ? "" : payrollTransaction.CustomerName),
                                             new XElement("PayrollTransactionInvoiceCustomerNr", payrollTransaction.CustomerId),
                                             new XElement("TimeDeviationCauseName", string.IsNullOrEmpty(payrollTransaction.TimeDeviationCauseName) ? "" : payrollTransaction.TimeDeviationCauseName));

                    payrollTransactionElements.Add(payrollTransactionElement);
                    timePayrollTransactionXmlId++;
                }

                #endregion 

                #region TimeInvoiceTransaction

                foreach (var invoiceTransaction in timeInvoiceTransactionsForProject.Where(p => p.CustomerInvoiceId > 0))
                {
                    int weeknr = CalendarUtility.GetWeekNr(invoiceTransaction.Date);

                    XElement invoiceTransactionElement = new XElement("TimeInvoiceTransaction",
                                            new XAttribute("id", timeInvoiceTransactionXmlId),
                                            new XElement("EmployeeNr", invoiceTransaction.EmployeeNr),
                                            new XElement("EmployeeName", invoiceTransaction.EmployeeName),
                                            new XElement("Date", invoiceTransaction.Date.ToShortDateString()),
                                            new XElement("Note", invoiceTransaction.ProjectTimeBlockExternalNote),
                                            new XElement("ExternalNote", invoiceTransaction.Comment),
                                            new XElement("InvoiceTransactionQuantity", invoiceTransaction.Quantity),
                                            new XElement("InvoiceTransactionInvoiceQuantity", invoiceTransaction.InvoiceQuantity),
                                            new XElement("InvoiceTransactionCreated", invoiceTransaction.Created.HasValue ? invoiceTransaction.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                            new XElement("InvoiceTransactionCreatedBy", invoiceTransaction.CreatedBy),
                                            new XElement("InvoiceTransactionExported", invoiceTransaction.Exported.ToInt()),
                                            new XElement("InvoiceTransactionProductNumber", invoiceTransaction.ProductNumber),
                                            new XElement("InvoiceTransactionProductName", invoiceTransaction.ProductName),
                                            new XElement("InvoiceTransactionProductDescription", invoiceTransaction.ProductDescription),
                                            new XElement("InvoiceTransactionTimeCodeCode", invoiceTransaction.TimeCode),
                                            new XElement("InvoiceTransactionTimeCodeName", invoiceTransaction.TimeCodeName),
                                            new XElement("InvoiceTransactionTimeCodeDescription", invoiceTransaction.TimeCodeDescription),
                                            new XElement("InvoiceTransactionTimeCodeTransactionId", invoiceTransaction.TimeCodeTransactionId),
                                            new XElement("InvoiceTransactionTimeCodeTransactionQuantity", invoiceTransaction.Quantity),
                                            new XElement("InvoiceTransactionTimeCodeTransactionInvoiceQuantity", invoiceTransaction.InvoiceQuantity),
                                            new XElement("InvoiceTransactionTimeCodeTransactionProjectId", invoiceTransaction.ProjectId),
                                            new XElement("InvoiceTransactionTimeCodeTransactionProjectNumber", invoiceTransaction.ProjectNr),
                                            new XElement("InvoiceTransactionTimeCodeTransactionProjectName", invoiceTransaction.ProjectName),
                                            new XElement("InvoiceTransactionTimeCodeTransactionProjectNote", invoiceTransaction.ProjectNote),
                                            new XElement("InvoiceTransactionTimeCodeTransactionParentProjectNumber", invoiceTransaction.ParentProjectNumber),
                                            new XElement("InvoiceTransactionTimeCodeTransactionParentProjectName", invoiceTransaction.ParentProjectName),
                                            new XElement("WeekNumber", weeknr.ToString()),
                                            new XElement("IsoDate", invoiceTransaction.Date.ToString("yyyy-MM-dd")),
                                            new XElement("InvoiceNr", invoiceTransaction.InvoiceNr),
                                            new XElement("OriginType", invoiceTransaction.OriginType.HasValue ? invoiceTransaction.OriginType.Value : 0),
                                            new XElement("InvoiceTransactionInvoiceCustomerName", invoiceTransaction.CustomerName),
                                            new XElement("InvoiceTransactionInvoiceCustomerNr", invoiceTransaction.CustomerNr));

                    invoiceTransactionElements.Add(invoiceTransactionElement);
                    timeInvoiceTransactionXmlId++;
                }

                #endregion

                #endregion

                #region Project

                XElement projectElement = new XElement("Project",
                               new XAttribute("id", projectXmlId),
                               new XElement("ProjectNumber", project.Number),
                               new XElement("ProjectName", project.Name),
                               new XElement("ProjectDescription", project.Description),
                               new XElement("ProjectCreated", project.Created.HasValue ? project.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                               new XElement("ProjectCreatedBy", project.CreatedBy),
                               new XElement("ProjectState", project.State),
                               new XElement("ProjectCustomerNr", project.Customer == null ? "" : project.Customer.CustomerNr),
                               new XElement("ProjectCustomerName", project.Customer == null ? "" : project.Customer.Name));

                projectXmlId++;

                #endregion

                #region Add Invoices

                foreach (XElement invoice in invoiceElements)
                {
                    projectElement.Add(invoice);
                }

                invoiceElements.Clear();

                #endregion

                #region Add TimePayrollTransaction

                foreach (XElement transaction in payrollTransactionElements)
                {
                    projectElement.Add(transaction);
                }

                #endregion

                #region Add TimeInvoiceTransaction

                foreach (XElement transaction in invoiceTransactionElements)
                {
                    projectElement.Add(transaction);
                }

                #endregion

                #region Create Merged Transactions

                List<XElement> mergedTransactions = new List<XElement>();
                int mergedTransactionXmlId = 1;

                try
                {
                    foreach (XElement transaction in payrollTransactionElements)
                    {
                        XElement mergedTransaction = new XElement("MergedTransaction",
                                new XAttribute("id", mergedTransactionXmlId),
                                new XElement("EmployeeNr", transaction.Element("EmployeeNr").Value),
                                new XElement("EmployeeName", transaction.Element("EmployeeName").Value),
                                new XElement("Date", transaction.Element("Date").Value),
                                new XElement("Note", transaction.Element("Note").Value),
                                new XElement("ExternalNote", transaction.Element("ExternalNote").Value),
                                new XElement("WeekNumber", transaction.Element("WeekNumber").Value),
                                new XElement("IsoDate", transaction.Element("IsoDate").Value),
                                new XElement("PayrollTransactionQuantity", transaction.Element("PayrollTransactionQuantity").Value),
                                new XElement("PayrollTransactionCreated", transaction.Element("PayrollTransactionCreated").Value),
                                new XElement("PayrollTransactionCreatedBy", transaction.Element("PayrollTransactionCreatedBy").Value),
                                new XElement("PayrollTransactionExported", transaction.Element("PayrollTransactionExported").Value),
                                new XElement("PayrollTransactionProductNumber", transaction.Element("PayrollTransactionProductNumber").Value),
                                new XElement("PayrollTransactionProductName", transaction.Element("PayrollTransactionProductName").Value),
                                new XElement("PayrollTransactionProductDescription", transaction.Element("PayrollTransactionProductDescription").Value),
                                new XElement("InvoiceTransactionQuantity", 0),
                                new XElement("InvoiceTransactionInvoiceQuantity", 0),
                                new XElement("InvoiceTransactionCreated", transaction.Element("PayrollTransactionCreated").Value),
                                new XElement("InvoiceTransactionCreatedBy", string.Empty),
                                new XElement("InvoiceTransactionExported", 0),
                                new XElement("InvoiceTransactionProductNumber", string.Empty),
                                new XElement("InvoiceTransactionProductName", string.Empty),
                                new XElement("InvoiceTransactionProductDescription", string.Empty),
                                new XElement("TimeCodeCode", transaction.Element("PayrollTransactionTimeCodeCode").Value),
                                new XElement("TimeCodeName", transaction.Element("PayrollTransactionTimeCodeName").Value),
                                new XElement("TimeCodeDescription", transaction.Element("PayrollTransactionTimeCodeDescription").Value),
                                new XElement("TimeCodeTransactionId", transaction.Element("PayrollTransactionTimeCodeTransactionId").Value),
                                new XElement("TimeCodeTransactionQuantity", transaction.Element("PayrollTransactionTimeCodeTransactionQuantity").Value),
                                new XElement("TimeCodeTransactionInvoiceQuantity", transaction.Element("PayrollTransactionTimeCodeTransactionInvoiceQuantity").Value),
                                new XElement("TimeCodeTransactionProjectId", transaction.Element("PayrollTransactionTimeCodeTransactionProjectId").Value),
                                new XElement("TimeCodeTransactionProjectNumber", transaction.Element("PayrollTransactionTimeCodeTransactionProjectNumber").Value),
                                new XElement("TimeCodeTransactionProjectName", transaction.Element("PayrollTransactionTimeCodeTransactionProjectName").Value),
                                new XElement("TimeCodeTransactionProjectNote", transaction.Element("PayrollTransactionTimeCodeTransactionProjectNote").Value),
                                new XElement("TimeCodeTransactionParentProjectNumber", transaction.Element("PayrollTransactionTimeCodeTransactionParentProjectNumber").Value),
                                new XElement("TimeCodeTransactionParentProjectName", transaction.Element("PayrollTransactionTimeCodeTransactionParentProjectName").Value),
                                new XElement("PayrollType", transaction.Element("PayrollType").Value),
                                new XElement("isPayed", transaction.Element("isPayed").Value),
                                new XElement("InvoiceNr", transaction.Element("InvoiceNr").Value),
                                new XElement("OriginType", transaction.Element("OriginType").Value),
                                new XElement("PayrollTransactionInvoiceCustomerName", transaction.Element("PayrollTransactionInvoiceCustomerName").Value),
                                new XElement("PayrollTransactionInvoiceCustomerNr", transaction.Element("PayrollTransactionInvoiceCustomerNr").Value),
                                new XElement("TimeDeviationCauseName", transaction.Element("TimeDeviationCauseName").Value /*string.Empty*/),
                                new XElement("PayrollTransactionUnitPrice", 0),
                                new XElement("PayrollAttestStateName", string.Empty)
                                );

                        mergedTransactions.Add(mergedTransaction);

                        mergedTransactionXmlId++;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
                }

                payrollTransactionElements.Clear();

                try
                {
                    foreach (XElement transaction in invoiceTransactionElements)
                    {
                        XElement mergedTransaction = new XElement("MergedTransaction",
                                new XAttribute("id", mergedTransactionXmlId),
                                new XElement("EmployeeNr", transaction.Element("EmployeeNr").Value),
                                new XElement("EmployeeName", transaction.Element("EmployeeName").Value),
                                new XElement("Date", transaction.Element("Date").Value),
                                new XElement("Note", transaction.Element("Note").Value),
                                new XElement("ExternalNote", transaction.Element("ExternalNote").Value),
                                new XElement("WeekNumber", transaction.Element("WeekNumber").Value),
                                new XElement("IsoDate", transaction.Element("IsoDate").Value),
                                new XElement("PayrollTransactionQuantity", 0),
                                new XElement("PayrollTransactionCreated", 0),
                                new XElement("PayrollTransactionCreatedBy", string.Empty),
                                new XElement("PayrollTransactionExported", 0),
                                new XElement("PayrollTransactionProductNumber", string.Empty),
                                new XElement("PayrollTransactionProductName", string.Empty),
                                new XElement("PayrollTransactionProductDescription", string.Empty),
                                new XElement("InvoiceTransactionQuantity", transaction.Element("InvoiceTransactionQuantity").Value),
                                new XElement("InvoiceTransactionInvoiceQuantity", transaction.Element("InvoiceTransactionInvoiceQuantity").Value),
                                new XElement("InvoiceTransactionCreated", transaction.Element("InvoiceTransactionCreated").Value),
                                new XElement("InvoiceTransactionCreatedBy", transaction.Element("InvoiceTransactionCreatedBy").Value),
                                new XElement("InvoiceTransactionExported", transaction.Element("InvoiceTransactionExported").Value),
                                new XElement("InvoiceTransactionProductNumber", transaction.Element("InvoiceTransactionProductNumber").Value),
                                new XElement("InvoiceTransactionProductName", transaction.Element("InvoiceTransactionProductName").Value),
                                new XElement("InvoiceTransactionProductDescription", transaction.Element("InvoiceTransactionProductDescription").Value),
                                new XElement("TimeCodeCode", transaction.Element("InvoiceTransactionTimeCodeCode").Value),
                                new XElement("TimeCodeName", transaction.Element("InvoiceTransactionTimeCodeName").Value),
                                new XElement("TimeCodeDescription", transaction.Element("InvoiceTransactionTimeCodeDescription").Value),
                                new XElement("TimeCodeTransactionId", transaction.Element("InvoiceTransactionTimeCodeTransactionId").Value),
                                new XElement("TimeCodeTransactionQuantity", transaction.Element("InvoiceTransactionTimeCodeTransactionQuantity").Value),
                                new XElement("TimeCodeTransactionInvoiceQuantity", transaction.Element("InvoiceTransactionTimeCodeTransactionInvoiceQuantity").Value),
                                new XElement("TimeCodeTransactionProjectId", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectId").Value),
                                new XElement("TimeCodeTransactionProjectNumber", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectNumber").Value),
                                new XElement("TimeCodeTransactionProjectName", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectName").Value),
                                new XElement("TimeCodeTransactionProjectNote", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectNote").Value),
                                new XElement("TimeCodeTransactionParentProjectNumber", transaction.Element("InvoiceTransactionTimeCodeTransactionParentProjectNumber").Value),
                                new XElement("TimeCodeTransactionParentProjectName", transaction.Element("InvoiceTransactionTimeCodeTransactionParentProjectName").Value),
                                new XElement("PayrollType", 0),
                                new XElement("isPayed", 0),
                                new XElement("InvoiceNr", transaction.Element("InvoiceNr").Value),
                                new XElement("OriginType", transaction.Element("OriginType").Value),
                                new XElement("PayrollTransactionInvoiceCustomerName", transaction.Element("InvoiceTransactionInvoiceCustomerName").Value),
                                new XElement("PayrollTransactionInvoiceCustomerNr", transaction.Element("InvoiceTransactionInvoiceCustomerNr").Value),
                                new XElement("TimeDeviationCauseName", string.Empty),
                                new XElement("PayrollTransactionUnitPrice", 0),
                                new XElement("PayrollAttestStateName", string.Empty)
                                );

                        mergedTransactions.Add(mergedTransaction);

                        mergedTransactionXmlId++;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
                }

                invoiceTransactionElements.Clear();

                #endregion

                projectElement.Add(mergedTransactions);

                mergedTransactions.Clear();

                projectElements.Add(projectElement);
            }

            #endregion

            return projectElements;
        }

        public XDocument CreateProjectTransactionsData(CreateReportResult reportResult)
        {
            if (reportResult.ReportTemplateType != SoeReportTemplateType.ProjectTransactionsReport)
                return null;

            var projectCentral = new ProjectCentralManager(parameterObject);

            var accountDims = AccountManager.GetAccountDimsByCompany(reportResult.ActorCompanyId);

            #region Prereq

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            bool overheadCostAsFixedAmount = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsFixedAmount, 0, reportResult.ActorCompanyId, 0);
            bool overheadCostAsAmountPerHour = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsAmountPerHour, 0, reportResult.ActorCompanyId, 0);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId, true);
            List<AccountDimDTO> accountDimInternalsDTOs = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId).ToDTOs();
            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "ProjectTransactionsReport");

            //ChecklistsReport
            XElement projectTransactionsReportElement = new XElement("ProjectTransactionsReport");

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateProjectTransactionsReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateAccountIntervalLabelReportHeaderLabelsElement());
            reportHeaderLabelsElement.Add(new XElement("IncludeChildProjectLabel", GetReportText(9239, "Inkludera underprojekt")));
            reportHeaderLabelsElement.Add(new XElement("ExcludeInternalOrdersLabel", GetReportText(9240, "Exkludera internordrar")));
            projectTransactionsReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;
            if (!reportParams.SP_PayrollTransactionDateFrom.HasValue && !reportParams.SP_PayrollTransactionDateTo.HasValue)
                maxDate = DateTime.MaxValue;
            else if (!reportParams.SP_PayrollTransactionDateFrom.HasValue)
            {
                reportParams.SP_PayrollTransactionDateFrom = minDate;
            }
            else if (!reportParams.SP_PayrollTransactionDateTo.HasValue)
            {
                reportParams.SP_PayrollTransactionDateTo = maxDate;
            }
            if (!reportParams.SP_InvoiceTransactionDateFrom.HasValue && !reportParams.SP_InvoiceTransactionDateTo.HasValue)
                maxDate = DateTime.MaxValue;
            else if (!reportParams.SP_InvoiceTransactionDateFrom.HasValue)
            {
                reportParams.SP_InvoiceTransactionDateFrom = minDate;
            }
            else if (!reportParams.SP_InvoiceTransactionDateTo.HasValue)
            {
                reportParams.SP_InvoiceTransactionDateTo = maxDate;
            }

            XElement reportHeaderElement = CreateProjectTransactionsReportHeaderElement(reportResult, reportParams);
            reportHeaderElement.Add(CreateAccountIntervalElement(reportResult, reportParams, accountDimInternalsDTOs));

            int maxNumberOfdims = 6;
            int dimCounter = 1;

            foreach (var accountDim in accountDims.OrderBy(d => d.AccountDimNr).Take(maxNumberOfdims))
            {
                reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", accountDim.Name));
                reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "ShortName", accountDim.ShortName));
                reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "SieDimNr", accountDim.SysSieDimNr));
                dimCounter++;
            }

            //add tags to max dims....
            while (dimCounter <= maxNumberOfdims)
            {
                reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", string.Empty));
                reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "ShortName", string.Empty));
                reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "SieDimNr", string.Empty));
                dimCounter++;
            }

            projectTransactionsReportElement.Add(reportHeaderElement);


            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateProjectTransactionsReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            projectTransactionsReportElement.Add(pageHeaderLabelsElement);

            #endregion

            using (var entities = new CompEntities())
            {
                entities.TimeCodeTransactionProjectView.NoTracking();

                #region Prereq

                //Get AccountDims
                AccountDim accountDimStds = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId);

                //Accounts
                List<AccountStd> accountStdsInInterval = new List<AccountStd>();
                List<AccountInternal> accountInternalsInInterval = new List<AccountInternal>();

                AccountManager.GetAccountsInInterval(entities, reportResult, reportParams, accountDimStds, false, ref accountStdsInInterval, ref accountInternalsInInterval);

                #endregion

                #region ProjectTransactionReport

                #region Projects

                string projectElementName = "Project";
                string timeCodeTransactionElementName = "TimeCodeTransaction";
                string billingRowElementName = "BillingRow";
                string externalTransactionElementName = "ExternalTransaction";

                //Settings
                int fixedPriceProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductFlatPrice, 0, reportResult.ActorCompanyId, 0);
                int fixedPriceKeepPricesProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductFlatPriceKeepPrices, 0, reportResult.ActorCompanyId, 0);
                List<AttestState> attestStates = AttestManager.GetAttestStates(reportResult.ActorCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing);
                List<CompanyCategoryRecord> categoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Project, SoeCategoryRecordEntity.Project, reportParams.SP_ProjectIds, reportResult.ActorCompanyId);
                int? attestStateTransferredOrderToInvoiceId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, reportResult.ActorCompanyId, 0);
                int productGuaranteeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGuarantee, 0, reportResult.ActorCompanyId, 0);
                var reportSettings = ReportManager.GetReportSettings(entities, reportResult.ReportId);

                var projectOverviewExtendedInfo = reportSettings.FirstOrDefault(x => x.Type == TermGroup_ReportSettingType.ProjectOverviewExtendedInfo)?.BoolData ?? false;
                reportHeaderElement.Add(new XElement("ProjectOverviewExtendedInfo", projectOverviewExtendedInfo ? "1" : "0"));

                // Check date selection
                var fromDate = reportParams.SP_InvoiceTransactionDateFrom.HasValue && reportParams.SP_InvoiceTransactionDateFrom.Value != CalendarUtility.DATETIME_MINVALUE ? reportParams.SP_InvoiceTransactionDateFrom.Value : (DateTime?)null;
                var toDate = reportParams.SP_InvoiceTransactionDateTo.HasValue && reportParams.SP_InvoiceTransactionDateTo.Value != CalendarUtility.DATETIME_MAXVALUE ? reportParams.SP_InvoiceTransactionDateTo.Value : (DateTime?)null;

                var allSelectedProjects = projectCentral.GetProjectsForProjectCentralStatus(entities, reportParams.SP_ProjectIds, reportResult.ActorCompanyId, true).ToLookup(p => p.ProjectId);
                int projectXmlId = 1;

                var bm = new BudgetManager(parameterObject);
                var pbm = new ProjectBudgetManager(parameterObject);

                foreach (int projId in reportParams.SP_ProjectIds)
                {
                    List<int> projects = new List<int>();

                    #region Child projects

                    bool isFirst = true;

                    var allProjects = allSelectedProjects[projId].ToList();

                    Project proj = allProjects.FirstOrDefault();

                    if (reportParams.SA_HasAccountInterval && accountInternalsInInterval.Any())
                    {
                        var accountInternalIds = accountInternalsInInterval.Where(a => a.Account.AccountDim.AccountDimNr == 2).Select(a => a.AccountId).ToList();
                        if (proj.DefaultDim2AccountId.HasValue && proj.DefaultDim2AccountId.Value >= 0 || accountInternalIds.Any())
                        {
                            if (proj.DefaultDim2AccountId.HasValue)
                            {

                                var projectaccountInternalId = proj.DefaultDim2AccountId.Value;
                                if (accountInternalIds.Any() && (!accountInternalIds.Contains(projectaccountInternalId)))
                                {
                                    continue;
                                }
                            }
                            else continue;
                        }
                        accountInternalIds = accountInternalsInInterval.Where(a => a.Account.AccountDim.AccountDimNr == 3).Select(a => a.AccountId).ToList();
                        if (proj.DefaultDim3AccountId.HasValue && proj.DefaultDim3AccountId.Value >= 0 || accountInternalIds.Any())
                        {
                            if (proj.DefaultDim3AccountId.HasValue)
                            {
                                var projectaccountInternalId = proj.DefaultDim3AccountId.Value;
                                if (accountInternalIds.Any() && (!accountInternalIds.Contains(projectaccountInternalId)))
                                {
                                    continue;
                                }
                            }
                            else continue;
                        }
                        accountInternalIds = accountInternalsInInterval.Where(a => a.Account.AccountDim.AccountDimNr == 4).Select(a => a.AccountId).ToList();
                        if (proj.DefaultDim4AccountId.HasValue && proj.DefaultDim4AccountId.Value >= 0 || accountInternalIds.Any())
                        {
                            if (proj.DefaultDim4AccountId.HasValue)
                            {
                                var projectaccountInternalId = proj.DefaultDim4AccountId.Value;
                                if (accountInternalIds.Any() && (!accountInternalIds.Contains(projectaccountInternalId)))
                                {
                                    continue;
                                }
                            }
                            else continue;
                        }
                        accountInternalIds = accountInternalsInInterval.Where(a => a.Account.AccountDim.AccountDimNr == 5).Select(a => a.AccountId).ToList();
                        if (proj.DefaultDim5AccountId.HasValue && proj.DefaultDim5AccountId.Value >= 0 || accountInternalIds.Any())
                        {
                            if (proj.DefaultDim5AccountId.HasValue)
                            {
                                var projectaccountInternalId = proj.DefaultDim5AccountId.Value;
                                if (accountInternalIds.Any() && (!accountInternalIds.Contains(projectaccountInternalId)))
                                {
                                    continue;
                                }
                            }
                            else continue;
                        }
                        accountInternalIds = accountInternalsInInterval.Where(a => a.Account.AccountDim.AccountDimNr == 6).Select(a => a.AccountId).ToList();
                        if (proj.DefaultDim6AccountId.HasValue && proj.DefaultDim6AccountId.Value >= 0 || accountInternalIds.Any())
                        {
                            if (proj.DefaultDim6AccountId.HasValue)
                            {
                                var projectaccountInternalId = proj.DefaultDim6AccountId.Value;
                                if (accountInternalIds.Any() && (!accountInternalIds.Contains(projectaccountInternalId)))
                                {
                                    continue;
                                }
                            }
                            else continue;
                        }
                    }

                    if (proj != null)
                    {
                        if (reportParams.SP_IncludeChildProjects)
                        {
                            List<Project> projectsToSearch = new List<Project>();
                            projectsToSearch.Add(proj);

                            // Loop until no further child projects are found
                            while (projectsToSearch.Count > 0)
                            {
                                Project currentProject = projectsToSearch.FirstOrDefault();

                                List<Project> childProjects = projectCentral.GetProjectsForProjectCentralStatus(entities, currentProject.ChildProjects.Select(p => p.ProjectId).ToList(), reportResult.ActorCompanyId);

                                if (isFirst)
                                    isFirst = false;
                                else
                                    allProjects.Add(currentProject);

                                // Add current to all projects list
                                projects.Add(projectsToSearch.FirstOrDefault().ProjectId);

                                // Remove handled project from search list
                                projectsToSearch.Remove(currentProject);

                                // Add all found child projects to search list
                                if (childProjects.Count != 0)
                                    projectsToSearch.AddRange(childProjects);
                            }

                        }
                        else
                        {
                            projects.Add(proj.ProjectId);
                        }
                    }

                    projects.Reverse();

                    #endregion

                    #region Validate internal orders

                    if (reportParams.SP_ExcludeInternalOrders)
                    {
                        if (reportParams.SP_IncludeChildProjects && projects.Count > 1)
                        {
                            var tempProjects = new List<int>();
                            tempProjects.Add(projId);
                            foreach (var projectId in projects.Where(p => p != projId))
                            {
                                if (projectCentral.ValidateExcludeInternalOrders(projectId))
                                    tempProjects.Add(projectId);
                            }
                            projects = tempProjects;
                        }
                        else
                        {
                            if (!projectCentral.ValidateExcludeInternalOrders(projId))
                                continue;
                        }
                    }

                    #endregion

                    foreach (int projectId in projects)
                    {
                        #region Project                  

                        bool budgetAdded = false;

                        List<TimeCodeTransactionProjectView> timeCodeTransactionItemsForProject = entities.TimeCodeTransactionProjectView.Where(i => i.ProjectId == projectId).ToList();
                        var group = timeCodeTransactionItemsForProject.GroupBy(i => i.TimeCodeTransactionId).FirstOrDefault();
                        var defProject = proj?.ProjectId == projectId ? proj : ProjectManager.GetProject(projectId, false);

                        defProject.Categories = string.Join(", ", categoryRecords.GetCategoryRecords(projectId).Select(c => c.Category.Name));

                        Account accountDim1 = defProject.DefaultDim2AccountId.HasValue ? AccountManager.GetAccount(reportResult.ActorCompanyId, defProject.DefaultDim2AccountId.Value) : null;
                        Account accountDim2 = defProject.DefaultDim3AccountId.HasValue ? AccountManager.GetAccount(reportResult.ActorCompanyId, defProject.DefaultDim3AccountId.Value) : null;
                        Account accountDim3 = defProject.DefaultDim4AccountId.HasValue ? AccountManager.GetAccount(reportResult.ActorCompanyId, defProject.DefaultDim4AccountId.Value) : null;
                        Account accountDim4 = defProject.DefaultDim5AccountId.HasValue ? AccountManager.GetAccount(reportResult.ActorCompanyId, defProject.DefaultDim5AccountId.Value) : null;
                        Account accountDim5 = defProject.DefaultDim6AccountId.HasValue ? AccountManager.GetAccount(reportResult.ActorCompanyId, defProject.DefaultDim6AccountId.Value) : null;

                        TimeCodeTransactionProjectView projectItem = null;

                        XElement projectElement;

                        if (group == null || !group.Any())
                        {
                            projectElement = new XElement(projectElementName,
                            new XAttribute("id", projectXmlId),
                            new XElement(projectElementName + "Nr", defProject.Number),
                            new XElement(projectElementName + "Name", defProject.Name),
                            new XElement(projectElementName + "Description", defProject.Description),
                            new XElement(projectElementName + "Type", defProject.Type),
                            new XElement(projectElementName + "Status", defProject.Status),
                            new XElement(projectElementName + "StatusName", defProject.StatusName),
                            new XElement(projectElementName + "StartDate", defProject.StartDate.HasValue ? defProject.StartDate.Value : CalendarUtility.DATETIME_DEFAULT),
                            new XElement(projectElementName + "StopDate", defProject.StopDate.HasValue ? defProject.StopDate.Value : CalendarUtility.DATETIME_DEFAULT),
                            new XElement(projectElementName + "Created", defProject.Created),
                            new XElement(projectElementName + "CreatedBy", defProject.CreatedBy),
                            new XElement(projectElementName + "CustomerNr", defProject.Customer),
                            new XElement(projectElementName + "CustomerName", defProject.Customer),
                            new XElement(projectElementName + "AccountInternalNr1", accountDim1?.AccountNr ?? string.Empty),
                            new XElement(projectElementName + "AccountInternalNr2", accountDim2?.AccountNr ?? string.Empty),
                            new XElement(projectElementName + "AccountInternalNr3", accountDim3?.AccountNr ?? string.Empty),
                            new XElement(projectElementName + "AccountInternalNr4", accountDim4?.AccountNr ?? string.Empty),
                            new XElement(projectElementName + "AccountInternalNr5", accountDim5?.AccountNr ?? string.Empty),
                            new XElement(projectElementName + "Category", defProject.Categories)
                            );
                        }
                        else
                        {
                            projectItem = group.FirstOrDefault();
                            projectElement = new XElement(projectElementName,
                            new XAttribute("id", projectXmlId),
                            new XElement(projectElementName + "Nr", projectItem.ProjectNumber),
                            new XElement(projectElementName + "Name", projectItem.ProjectName),
                            new XElement(projectElementName + "Description", projectItem.ProjectDescription),
                            new XElement(projectElementName + "Type", projectItem.ProjectType),
                            new XElement(projectElementName + "Status", projectItem.ProjectStatus),
                            new XElement(projectElementName + "StatusName", GetText(projectItem.ProjectStatus, (int)TermGroup.ProjectStatus)),
                            new XElement(projectElementName + "StartDate", projectItem.ProjectStartDate.HasValue ? projectItem.ProjectStartDate.Value : CalendarUtility.DATETIME_DEFAULT),
                            new XElement(projectElementName + "StopDate", projectItem.ProjectStopDate.HasValue ? projectItem.ProjectStopDate.Value : CalendarUtility.DATETIME_DEFAULT),
                            new XElement(projectElementName + "Created", projectItem.ProjectCreated),
                            new XElement(projectElementName + "CreatedBy", projectItem.ProjectCreatedBy),
                            new XElement(projectElementName + "CustomerNr", projectItem.CustomerNr),
                            new XElement(projectElementName + "CustomerName", projectItem.CustomerName),
                            new XElement(projectElementName + "AccountInternalNr1", accountDim1?.AccountNr ?? String.Empty),
                            new XElement(projectElementName + "AccountInternalNr2", accountDim2?.AccountNr ?? String.Empty),
                            new XElement(projectElementName + "AccountInternalNr3", accountDim3?.AccountNr ?? String.Empty),
                            new XElement(projectElementName + "AccountInternalNr4", accountDim4?.AccountNr ?? String.Empty),
                            new XElement(projectElementName + "AccountInternalNr5", accountDim5?.AccountNr ?? String.Empty),
                            new XElement(projectElementName + "Category", defProject.Categories)
                            );
                        }

                        int timeCodeTransactionXmlId = 1;

                        #region Project Budget

                        decimal projectBillableMinutesInvoicedSumIB = 0;
                        decimal projectPersonellIncomeInvoicedSumIB = 0;
                        decimal projectPersonellIncomeInvoicedSumBudget = 0;
                        decimal projectMateriallIncomeInvoicedSumIB = 0;
                        decimal projectMateriallIncomeInvoicedSumBudget = 0;
                        decimal projectPersonellCostSumIB = 0;
                        decimal projectBillableMinutesInvoicedSumBudget = 0;
                        decimal projectPersonellCostSumBudget = 0;
                        decimal projectMaterialCostSumIB = 0;
                        decimal projectMaterialCostSumBudget = 0;
                        decimal projectExpenceCostSumIB = 0;
                        decimal projectExpenceCostSumBudget = 0;
                        decimal projectOverheadCostSumIB = 0;
                        decimal projectOverheadCostPerHour = 0;
                        decimal projectOverheadCostSumBudget = 0;

                        bool isNewBudget = ProjectBudgetManager.HasExtendedProjectBudget(projectId);
                        if (isNewBudget)
                        {
                            BudgetHeadProjectDTO projectBudgetHead = pbm.GetLatestProjectBudgetHeadIncludingRows(entities, projectId, DistributionCodeBudgetType.ProjectBudgetForecast, true);
                            if(projectBudgetHead == null) 
                                projectBudgetHead = pbm.GetLatestProjectBudgetHeadIncludingRows(entities, projectId, DistributionCodeBudgetType.ProjectBudgetExtended, true);

                            BudgetHeadProjectDTO projectBudgetHeadIB = fromDate == null ? pbm.GetLatestProjectBudgetHeadIncludingRows(entities, projectId, DistributionCodeBudgetType.ProjectBudgetIB, true) : null;

                            projectBillableMinutesInvoicedSumIB = projectBudgetHeadIB != null ? projectBudgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell).Sum(r => r.TotalQuantity) : 0;
                            projectBillableMinutesInvoicedSumBudget = projectBudgetHead != null ? projectBudgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell).Sum(r => r.TotalQuantity) : 0;

                            projectPersonellIncomeInvoicedSumIB = projectBudgetHeadIB != null ? projectBudgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal).Sum(r => r.TotalAmount) : 0;
                            projectPersonellIncomeInvoicedSumBudget = projectBudgetHead != null ? projectBudgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal).Sum(r => r.TotalAmount) : 0;

                            projectMateriallIncomeInvoicedSumIB = projectBudgetHeadIB != null ? projectBudgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal).Sum(r => r.TotalAmount) : 0;
                            projectMateriallIncomeInvoicedSumBudget = projectBudgetHead != null ? projectBudgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal).Sum(r => r.TotalAmount) : 0;

                            projectPersonellCostSumIB = projectBudgetHeadIB != null ? projectBudgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell).Sum(r => r.TotalAmount) : 0;
                            projectPersonellCostSumBudget = projectBudgetHead != null ? projectBudgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell).Sum(r => r.TotalAmount) : 0;

                            projectMaterialCostSumIB = projectBudgetHeadIB != null ? projectBudgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial).Sum(r => r.TotalAmount) : 0;
                            projectMaterialCostSumBudget = projectBudgetHead != null ? projectBudgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial).Sum(r => r.TotalAmount) : 0;

                            projectExpenceCostSumIB = projectBudgetHeadIB != null ? projectBudgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense).Sum(r => r.TotalAmount) : 0;
                            projectExpenceCostSumBudget = projectBudgetHead != null ? projectBudgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense).Sum(r => r.TotalAmount) : 0;

                            projectOverheadCostSumIB = projectBudgetHeadIB != null ? projectBudgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost).Sum(r => r.TotalAmount) : 0;
                            projectOverheadCostSumBudget = projectBudgetHead != null ? projectBudgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost).Sum(r => r.TotalAmount) : 0;

                            var budgetOverheadIBPerHourRow = projectBudgetHeadIB != null ? projectBudgetHeadIB.Rows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour) : null;
                            var budgetOverheadPerHourRow = projectBudgetHead != null ? projectBudgetHead.Rows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour) : null;

                            if (budgetOverheadIBPerHourRow != null)
                            {
                                projectOverheadCostSumIB += projectBillableMinutesInvoicedSumIB != 0 ? (projectBillableMinutesInvoicedSumIB / 60) * (budgetOverheadIBPerHourRow.TotalAmount) : 0;
                            }

                            if (budgetOverheadPerHourRow != null)
                            {
                                projectOverheadCostPerHour = projectBudgetHead != null ? projectBudgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour).Sum(r => r.TotalAmount) : 0;
                                projectOverheadCostSumBudget += projectBillableMinutesInvoicedSumBudget != 0 ? (projectBillableMinutesInvoicedSumBudget / 60) * (budgetOverheadPerHourRow.TotalAmount) : 0;
                            }
                        }
                        else
                        {

                            BudgetHead budgetHead = bm.GetBudgetHeadIncludingRowsForProject(entities, projectId);

                            if (budgetHead == null)
                                budgetHead = new BudgetHead();

                            BudgetRow budgetRow = null;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoicedIB);
                            if (budgetRow != null)
                                projectBillableMinutesInvoicedSumIB = budgetRow.TotalAmount;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotalIB);
                            if (budgetRow != null)
                                projectPersonellIncomeInvoicedSumIB = budgetRow.TotalAmount;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                            if (budgetRow != null)
                                projectPersonellIncomeInvoicedSumBudget = budgetRow.TotalAmount;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotalIB);
                            if (budgetRow != null)
                                projectMateriallIncomeInvoicedSumIB = budgetRow.TotalAmount;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                            if (budgetRow != null)
                                projectMateriallIncomeInvoicedSumBudget = budgetRow.TotalAmount;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonellIB);
                            if (budgetRow != null)
                                projectPersonellCostSumIB = budgetRow.TotalAmount;

                            var budgetRows = budgetHead.BudgetRow.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                            foreach (var row in budgetRows)
                            {
                                if (row != null)
                                {
                                    projectPersonellCostSumBudget += row.TotalAmount;
                                    projectBillableMinutesInvoicedSumBudget += row.TotalQuantity;
                                }
                            }

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterialIB);
                            if (budgetRow != null)
                                projectMaterialCostSumIB = budgetRow.TotalAmount;

                            budgetRows = budgetHead.BudgetRow.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                            foreach (var row in budgetRows)
                            {
                                if (row != null)
                                    projectMaterialCostSumBudget += row.TotalAmount;
                            }

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpenseIB);
                            if (budgetRow != null)
                                projectExpenceCostSumIB = budgetRow.TotalAmount;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);
                            if (budgetRow != null)
                                projectExpenceCostSumBudget = budgetRow.TotalAmount;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostIB);
                            if (budgetRow != null)
                                projectOverheadCostSumIB = budgetRow.TotalAmount;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);
                            if (budgetRow != null)
                                projectOverheadCostPerHour = budgetRow.TotalAmount;

                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost);
                            if (budgetRow != null)
                                projectOverheadCostSumBudget = budgetRow.TotalAmount;
                        }

                        #endregion


                        decimal projectBillableMinutesPreliminaryInvoicedSum = 0;
                        decimal projectPersonellIncomeInvoicedSum = 0;
                        decimal projectPersonellIncomePreliminaryInvoicedSum = 0;
                        decimal projectPersonellIncomeNotInvoicedSum = 0;
                        decimal projectMaterialIncomeInvoicedSum = 0;
                        decimal projectMaterialIncomePreliminaryInvoicedSum = 0;
                        decimal projectMaterialIncomeNotInvoicedSum = 0;
                        decimal projectPersonellCostSum = 0;
                        decimal projectMaterialCostSum = 0;
                        decimal projectMaterialCostSumSupplInv = 0;
                        decimal projectExpenceCostSum = 0;
                        decimal projectOverheadCostSum = 0;

                        decimal projectFixedPrice = 0;
                        decimal projectFixedPriceKeepPrices = 0;
                        decimal projectOpenLiftAmount = 0;
                        decimal projectInvoicedLiftAmount = 0;
                        decimal projectGuaranteeAmount = 0;
                        decimal projectPersonellWorkHours = 0;
                        decimal projectPersonellNondebitHours = 0;

                        List<int> processedBillingItems = new List<int>();
                        List<XElement> timeCodeTransactionElements = new List<XElement>();
                        List<int> wrongBillingRows = new List<int>();

                        #region Default Transaction element
                        if (timeCodeTransactionXmlId == 1)
                        {
                            XElement timeCodeTransactionElement = new XElement(timeCodeTransactionElementName);

                            timeCodeTransactionElement.Add(new XElement(timeCodeTransactionElementName + "CustomerInvoiceRowId", 0));

                            timeCodeTransactionElement.Add(new XElement(billingRowElementName, String.Empty));

                            timeCodeTransactionElement.Add(new XElement(externalTransactionElementName, String.Empty));

                            timeCodeTransactionElements.Add(timeCodeTransactionElement);

                        }
                        #endregion

                        var projdtos = projectCentral.GetProjectCentralStatus_v4(reportResult.ActorCompanyId, projectId, fromDate, toDate, false, reportParams.SP_ExcludeInternalOrders, false);

                        var fixedPriceDtos = projdtos.Where(p => p.Type == ProjectCentralBudgetRowType.FixedPriceTotal).ToList();
                        if (fixedPriceDtos.Any())
                        {
                            var fixedDtos = projdtos.Where(p => p.AssociatedId == 0 || fixedPriceDtos.Select(d => d.AssociatedId).Contains(p.AssociatedId) || (p.Type == ProjectCentralBudgetRowType.CostMaterial && p.OriginType == SoeOriginType.SupplierInvoice)).ToList();

                            projectPersonellCostSum = 0;
                            projectPersonellIncomeInvoicedSum = 0;
                            projectPersonellIncomeNotInvoicedSum = 0;
                            projectMaterialCostSum = 0;
                            projectMaterialCostSumSupplInv = 0;
                            projectMaterialIncomeInvoicedSum = 0;
                            projectMaterialIncomeNotInvoicedSum = 0;
                            projectPersonellWorkHours = 0;
                            projectPersonellNondebitHours = 0;
                            projectExpenceCostSum = 0;
                            projectOverheadCostSum = 0;

                            foreach (var projdto in fixedDtos)
                            {
                                //  if (projdto.Name == "Debiterbara timmar, ej fakturerat" || projdto.GroupRowTypeName == "Debiterbara timmar, ej fakturerat")
                                if (projdto.Type == ProjectCentralBudgetRowType.BillableMinutesNotInvoiced)
                                {
                                    projectPersonellNondebitHours += projdto.Value;
                                }
                                //   if (projdto.Name == "Intäkter fakturerat" || projdto.GroupRowTypeName == "Intäkter fakturerat")
                                if (projdto.Type == ProjectCentralBudgetRowType.IncomeInvoiced)
                                {
                                    projectMaterialIncomeInvoicedSum += projdto.Value;
                                    //    projectPersonellIncomeInvoicedSum += projdto.Value2;
                                }

                                //   if (projdto.Name == "Intäkter ofakturerat" || projdto.GroupRowTypeName == "Intäkter ofakturerat")
                                if (projdto.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced)
                                {
                                    projectMaterialIncomeNotInvoicedSum += projdto.Value;
                                }
                                //   if (projdto.Name == "Kostnader material" || projdto.GroupRowTypeName == "Kostnader material")
                                if (projdto.Type == ProjectCentralBudgetRowType.CostMaterial)
                                {
                                    projectMaterialCostSum += projdto.Value;
                                }
                                //     if ((projdto.Name == "Kostnader material" || projdto.GroupRowTypeName == "Kostnader material") && projdto.OriginType == SoeOriginType.SupplierInvoice)
                                if ((projdto.Type == ProjectCentralBudgetRowType.CostMaterial) && projdto.OriginType == SoeOriginType.SupplierInvoice)
                                {
                                    projectMaterialCostSumSupplInv += projdto.Value2;
                                }
                                //   if (projdto.Name == "Kostnader personal" || projdto.GroupRowTypeName == "Kostnader personal")
                                if (projdto.Type == ProjectCentralBudgetRowType.CostPersonell)
                                {
                                    projectPersonellCostSum += projdto.Value;
                                    projectPersonellWorkHours += projdto.Value2 * 60;
                                }
                                //   if (projdto.Name == "FixedPriceTotal" || projdto.GroupRowTypeName == "FixedPriceTotal")
                                if (projdto.Type == ProjectCentralBudgetRowType.FixedPriceTotal)
                                {
                                    projectFixedPrice += projdto.Value;
                                }
                                //   if (projdto.Name == "CostExpense" || projdto.GroupRowTypeName == "CostExpense")
                                if (projdto.Type == ProjectCentralBudgetRowType.CostExpense)
                                {
                                    projectExpenceCostSum += projdto.Value;
                                }
                                if (projdto.Type == ProjectCentralBudgetRowType.OverheadCost)
                                {
                                    projectOverheadCostSum += projdto.Value;
                                }
                            }

                            projectElement.Add(new XElement(projectElementName + "BillableMinutesInvoicedSum", projectPersonellWorkHours),
                            new XElement(projectElementName + "BillableMinutesPreliminaryInvoicedSum", projectBillableMinutesPreliminaryInvoicedSum),
                            new XElement(projectElementName + "BillableMinutesNotInvoicedSum", projectPersonellNondebitHours),
                            new XElement(projectElementName + "BillableMinutesInvoicedSumBudget", projectBillableMinutesInvoicedSumBudget),
                            new XElement(projectElementName + "PersonellIncomeInvoicedSum", projectPersonellIncomeInvoicedSum),
                            new XElement(projectElementName + "PersonellIncomePreliminaryInvoicedSum", projectPersonellIncomePreliminaryInvoicedSum),
                            new XElement(projectElementName + "PersonellIncomeNotInvoicedSum", projectPersonellIncomeNotInvoicedSum),
                            new XElement(projectElementName + "PersonellIncomeInvoicedSumBudget", projectPersonellIncomeInvoicedSumBudget),
                            new XElement(projectElementName + "MaterialIncomeInvoicedSum", projectMaterialIncomeInvoicedSum),
                            new XElement(projectElementName + "MaterialIncomePreliminaryInvoicedSum", projectMaterialIncomePreliminaryInvoicedSum),
                            new XElement(projectElementName + "MaterialIncomeNotInvoicedSum", projectMaterialIncomeNotInvoicedSum),
                            new XElement(projectElementName + "MateriallIncomeInvoicedSumBudget", projectMateriallIncomeInvoicedSumBudget),
                            new XElement(projectElementName + "PersonellCostSum", projectPersonellCostSum),
                            new XElement(projectElementName + "PersonellCostSumBudget", projectPersonellCostSumBudget),
                            new XElement(projectElementName + "MaterialCostSum", projectMaterialCostSum),
                            new XElement(projectElementName + "MaterialCostSumSupplInv", projectMaterialCostSumSupplInv),
                            new XElement(projectElementName + "MaterialCostSumBudget", projectMaterialCostSumBudget),
                            new XElement(projectElementName + "ExpenceCostSum", projectExpenceCostSum),
                            new XElement(projectElementName + "ExpenceCostSumBudget", projectExpenceCostSumBudget),
                            new XElement(projectElementName + "OverheadCostSum", projectOverheadCostSum),
                            new XElement(projectElementName + "OverheadCostSumBudget", projectOverheadCostSumBudget));

                            #region Parent project

                            if (group != null && projectItem.ParentProjectId != null)
                            {
                                Project project = allProjects.FirstOrDefault(p => p.ProjectId == projectItem.ParentProjectId);

                                if (project != null)
                                {
                                    projectElement.Add(new XElement(projectElementName + "ParentProjectId", projectItem.ParentProjectId),
                                    new XElement(projectElementName + "ParentProjectNumber", project.Number));
                                }
                                else
                                {
                                    projectElement.Add(new XElement(projectElementName + "ParentProjectId", 0),
                                    new XElement(projectElementName + "ParentProjectNumber", String.Empty));
                                }
                            }
                            else
                            {
                                projectElement.Add(new XElement(projectElementName + "ParentProjectId", 0),
                                new XElement(projectElementName + "ParentProjectNumber", String.Empty));
                            }

                            #endregion

                            #region Fixed and lift amounts

                            bool fixedPrice = false;
                            bool fixedPriceKeepPrices = false;
                            if (projectFixedPrice != 0)
                            {
                                fixedPrice = true;
                            }
                            else if (projectFixedPriceKeepPrices != 0)
                            {
                                projectFixedPrice = projectFixedPrice + projectFixedPriceKeepPrices; //This for now before the crystal reports is updated
                                fixedPriceKeepPrices = true;
                            }
                            else // Fallback for when amount is zero
                            {
                                fixedPrice = true;
                            }

                            projectElement.Add(
                                new XElement(projectElementName + "IsFixedPriceType1", fixedPrice.ToInt()),
                                new XElement(projectElementName + "IsFixedPriceType2", fixedPriceKeepPrices.ToInt()),
                                //new XElement(projectElementName + "FixedPriceKeepPrices", projectFixedPriceKeepPrices < 0 ? Decimal.Negate(projectFixedPriceKeepPrices) : projectFixedPriceKeepPrices),
                                new XElement(projectElementName + "FixedPrice", projectFixedPrice < 0 ? Decimal.Negate(projectFixedPrice) : projectFixedPrice),
                                new XElement(projectElementName + "OpenLiftAmount", projectOpenLiftAmount < 0 ? Decimal.Negate(projectOpenLiftAmount) : projectOpenLiftAmount),
                                new XElement(projectElementName + "InvoicedLiftAmount", projectInvoicedLiftAmount < 0 ? Decimal.Negate(projectInvoicedLiftAmount) : projectInvoicedLiftAmount),
                                new XElement(projectElementName + "RetainedGuaranteeAmount", projectGuaranteeAmount < 0 ? Decimal.Negate(projectGuaranteeAmount) : projectGuaranteeAmount));

                            #endregion

                            projdtos = projdtos.Where(p => !fixedDtos.Select(d => d.AssociatedId).Contains(p.AssociatedId)).ToList();

                            if (projdtos.Any())
                            {
                                budgetAdded = true;

                                projectFixedPrice = 0;
                                projectFixedPriceKeepPrices = 0;
                                projectOpenLiftAmount = 0;
                                projectInvoicedLiftAmount = 0;
                                projectGuaranteeAmount = 0;
                                projectPersonellWorkHours = 0;
                                projectPersonellNondebitHours = 0;
                                projectExpenceCostSum = 0;
                                projectOverheadCostSum = 0;

                                #region IB
                                projectElement.Add(
                                    new XElement(projectElementName + "BillableMinutesInvoicedSumIB", projectBillableMinutesInvoicedSumIB),
                                    new XElement(projectElementName + "PersonellIncomeInvoicedSumIB", projectPersonellIncomeInvoicedSumIB),
                                    new XElement(projectElementName + "MateriallIncomeInvoicedSumIB", projectMateriallIncomeInvoicedSumIB),
                                    new XElement(projectElementName + "PersonellCostSumIB", projectPersonellCostSumIB),
                                    new XElement(projectElementName + "MaterialCostSumIB", projectMaterialCostSumIB),
                                    new XElement(projectElementName + "ExpenceCostSumIB", projectExpenceCostSumIB),
                                    new XElement(projectElementName + "OverheadCostSumIB", projectOverheadCostSumIB));

                                #endregion

                                projectElement.Add(timeCodeTransactionElements);

                                if (timeCodeTransactionXmlId > 0)
                                {
                                    projectTransactionsReportElement.Add(projectElement);
                                    projectXmlId++;
                                }

                                if (group == null || !group.Any())
                                {
                                    projectElement = new XElement(projectElementName,
                                    new XAttribute("id", projectXmlId),
                                    new XElement(projectElementName + "Nr", defProject.Number),
                                    new XElement(projectElementName + "Name", defProject.Name),
                                    new XElement(projectElementName + "Description", defProject.Description),
                                    new XElement(projectElementName + "Type", defProject.Type),
                                    new XElement(projectElementName + "Status", defProject.Status),
                                    new XElement(projectElementName + "StatusName", defProject.StatusName),
                                    new XElement(projectElementName + "StartDate", defProject.StartDate.HasValue ? defProject.StartDate.Value : CalendarUtility.DATETIME_DEFAULT),
                                    new XElement(projectElementName + "StopDate", defProject.StopDate.HasValue ? defProject.StopDate.Value : CalendarUtility.DATETIME_DEFAULT),
                                    new XElement(projectElementName + "Created", defProject.Created),
                                    new XElement(projectElementName + "CreatedBy", defProject.CreatedBy),
                                    new XElement(projectElementName + "CustomerNr", defProject.Customer),
                                    new XElement(projectElementName + "CustomerName", defProject.Customer),
                                    new XElement(projectElementName + "AccountInternalNr1", accountDim1?.AccountNr ?? string.Empty),
                                    new XElement(projectElementName + "AccountInternalNr2", accountDim2?.AccountNr ?? string.Empty),
                                    new XElement(projectElementName + "AccountInternalNr3", accountDim3?.AccountNr ?? string.Empty),
                                    new XElement(projectElementName + "AccountInternalNr4", accountDim4?.AccountNr ?? string.Empty),
                                    new XElement(projectElementName + "AccountInternalNr5", accountDim5?.AccountNr ?? string.Empty),
                                    new XElement(projectElementName + "Category", defProject.Categories)
                                    );
                                }
                                else
                                {
                                    projectItem = group.FirstOrDefault();
                                    projectElement = new XElement(projectElementName,
                                    new XAttribute("id", projectXmlId),
                                    new XElement(projectElementName + "Nr", projectItem.ProjectNumber),
                                    new XElement(projectElementName + "Name", projectItem.ProjectName),
                                    new XElement(projectElementName + "Description", projectItem.ProjectDescription),
                                    new XElement(projectElementName + "Type", projectItem.ProjectType),
                                    new XElement(projectElementName + "Status", projectItem.ProjectStatus),
                                    new XElement(projectElementName + "StatusName", GetText(projectItem.ProjectStatus, (int)TermGroup.ProjectStatus)),
                                    new XElement(projectElementName + "StartDate", projectItem.ProjectStartDate.HasValue ? projectItem.ProjectStartDate.Value : CalendarUtility.DATETIME_DEFAULT),
                                    new XElement(projectElementName + "StopDate", projectItem.ProjectStopDate.HasValue ? projectItem.ProjectStopDate.Value : CalendarUtility.DATETIME_DEFAULT),
                                    new XElement(projectElementName + "Created", projectItem.ProjectCreated),
                                    new XElement(projectElementName + "CreatedBy", projectItem.ProjectCreatedBy),
                                    new XElement(projectElementName + "CustomerNr", projectItem.CustomerNr),
                                    new XElement(projectElementName + "CustomerName", projectItem.CustomerName),
                                    new XElement(projectElementName + "AccountInternalNr1", accountDim1?.AccountNr ?? String.Empty),
                                    new XElement(projectElementName + "AccountInternalNr2", accountDim2?.AccountNr ?? String.Empty),
                                    new XElement(projectElementName + "AccountInternalNr3", accountDim3?.AccountNr ?? String.Empty),
                                    new XElement(projectElementName + "AccountInternalNr4", accountDim4?.AccountNr ?? String.Empty),
                                    new XElement(projectElementName + "AccountInternalNr5", accountDim5?.AccountNr ?? String.Empty),
                                    new XElement(projectElementName + "Category", defProject.Categories)
                                    );
                                }
                            }
                            else
                            {
                                projectElement.Add(timeCodeTransactionElements);

                                if (timeCodeTransactionXmlId > 0)
                                {
                                    projectTransactionsReportElement.Add(projectElement);
                                    projectXmlId++;
                                }
                            }
                        }

                        projectPersonellCostSum = 0;
                        projectPersonellIncomeInvoicedSum = 0;
                        projectPersonellIncomeNotInvoicedSum = 0;
                        projectMaterialCostSum = 0;
                        projectMaterialCostSumSupplInv = 0;
                        projectMaterialIncomeInvoicedSum = 0;
                        projectMaterialIncomeNotInvoicedSum = 0;
                        projectPersonellWorkHours = 0;
                        projectPersonellNondebitHours = 0;

                        if (projdtos.Any())
                        {
                            foreach (var projdto in projdtos)
                            {
                                //  if (projdto.Name == "Debiterbara timmar, ej fakturerat" || projdto.GroupRowTypeName == "Debiterbara timmar, ej fakturerat")
                                if (projdto.Type == ProjectCentralBudgetRowType.BillableMinutesNotInvoiced)
                                {
                                    projectPersonellNondebitHours += projdto.Value;
                                }
                                //   if (projdto.Name == "Intäkter fakturerat" || projdto.GroupRowTypeName == "Intäkter fakturerat")
                                if (projdto.Type == ProjectCentralBudgetRowType.IncomeInvoiced)
                                {
                                    projectMaterialIncomeInvoicedSum += projdto.Value;
                                    //    projectPersonellIncomeInvoicedSum += projdto.Value2;
                                }

                                //   if (projdto.Name == "Intäkter ofakturerat" || projdto.GroupRowTypeName == "Intäkter ofakturerat")
                                if (projdto.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced)
                                {
                                    projectMaterialIncomeNotInvoicedSum += projdto.Value;
                                }
                                //   if (projdto.Name == "Kostnader material" || projdto.GroupRowTypeName == "Kostnader material")
                                if (projdto.Type == ProjectCentralBudgetRowType.CostMaterial)
                                {
                                    projectMaterialCostSum += projdto.Value;
                                }
                                //     if ((projdto.Name == "Kostnader material" || projdto.GroupRowTypeName == "Kostnader material") && projdto.OriginType == SoeOriginType.SupplierInvoice)
                                if ((projdto.Type == ProjectCentralBudgetRowType.CostMaterial) && projdto.OriginType == SoeOriginType.SupplierInvoice)
                                {
                                    projectMaterialCostSumSupplInv += projdto.Value2;
                                }
                                //   if (projdto.Name == "Kostnader personal" || projdto.GroupRowTypeName == "Kostnader personal")
                                if (projdto.Type == ProjectCentralBudgetRowType.CostPersonell)
                                {
                                    projectPersonellCostSum += projdto.Value;
                                    projectPersonellWorkHours += projdto.Value2 * 60;
                                }
                                //   if (projdto.Name == "FixedPriceTotal" || projdto.GroupRowTypeName == "FixedPriceTotal")
                                if (projdto.Type == ProjectCentralBudgetRowType.FixedPriceTotal)
                                {
                                    projectFixedPrice += projdto.Value;
                                }
                                //   if (projdto.Name == "CostExpense" || projdto.GroupRowTypeName == "CostExpense")
                                if (projdto.Type == ProjectCentralBudgetRowType.CostExpense)
                                {
                                    projectExpenceCostSum += projdto.Value;
                                }
                                if (projdto.Type == ProjectCentralBudgetRowType.OverheadCost)
                                {
                                    projectOverheadCostSum += projdto.Value;
                                }

                            }

                            #region Project header sums

                            projectElement.Add(new XElement(projectElementName + "BillableMinutesInvoicedSum", projectPersonellWorkHours),
                                new XElement(projectElementName + "BillableMinutesPreliminaryInvoicedSum", projectBillableMinutesPreliminaryInvoicedSum),
                                new XElement(projectElementName + "BillableMinutesNotInvoicedSum", projectPersonellNondebitHours),
                                new XElement(projectElementName + "BillableMinutesInvoicedSumBudget", budgetAdded ? 0 : projectBillableMinutesInvoicedSumBudget),
                                new XElement(projectElementName + "PersonellIncomeInvoicedSum", projectPersonellIncomeInvoicedSum),
                                new XElement(projectElementName + "PersonellIncomePreliminaryInvoicedSum", projectPersonellIncomePreliminaryInvoicedSum),
                                new XElement(projectElementName + "PersonellIncomeNotInvoicedSum", projectPersonellIncomeNotInvoicedSum),
                                new XElement(projectElementName + "PersonellIncomeInvoicedSumBudget", budgetAdded ? 0 : projectPersonellIncomeInvoicedSumBudget),
                                new XElement(projectElementName + "MaterialIncomeInvoicedSum", projectMaterialIncomeInvoicedSum),
                                new XElement(projectElementName + "MaterialIncomePreliminaryInvoicedSum", projectMaterialIncomePreliminaryInvoicedSum),
                                new XElement(projectElementName + "MaterialIncomeNotInvoicedSum", projectMaterialIncomeNotInvoicedSum),
                                new XElement(projectElementName + "MateriallIncomeInvoicedSumBudget", budgetAdded ? 0 : projectMateriallIncomeInvoicedSumBudget),
                                new XElement(projectElementName + "PersonellCostSum", projectPersonellCostSum),
                                new XElement(projectElementName + "PersonellCostSumBudget", budgetAdded ? 0 : projectPersonellCostSumBudget),
                                new XElement(projectElementName + "MaterialCostSum", projectMaterialCostSum),
                                new XElement(projectElementName + "MaterialCostSumSupplInv", projectMaterialCostSumSupplInv),
                                new XElement(projectElementName + "MaterialCostSumBudget", budgetAdded ? 0 : projectMaterialCostSumBudget),
                                new XElement(projectElementName + "ExpenceCostSum", projectExpenceCostSum),
                                new XElement(projectElementName + "ExpenceCostSumBudget", budgetAdded ? 0 : projectExpenceCostSumBudget),
                                new XElement(projectElementName + "OverheadCostSum", projectOverheadCostSum),
                                new XElement(projectElementName + "OverheadCostSumBudget", budgetAdded ? 0 : projectOverheadCostSumBudget));

                            #region Parent project

                            if (group != null && projectItem.ParentProjectId != null)
                            {
                                Project project = allProjects.FirstOrDefault(p => p.ProjectId == projectItem.ParentProjectId);

                                if (project != null)
                                {
                                    projectElement.Add(new XElement(projectElementName + "ParentProjectId", projectItem.ParentProjectId),
                                    new XElement(projectElementName + "ParentProjectNumber", project.Number));
                                }
                                else
                                {
                                    projectElement.Add(new XElement(projectElementName + "ParentProjectId", 0),
                                    new XElement(projectElementName + "ParentProjectNumber", String.Empty));
                                }
                            }
                            else
                            {
                                projectElement.Add(new XElement(projectElementName + "ParentProjectId", 0),
                                new XElement(projectElementName + "ParentProjectNumber", String.Empty));
                            }

                            #endregion

                            #region Fixed and lift amounts

                            bool isFixedPrice = false;
                            bool isFixedPriceKeepPrices = false;
                            if (projectFixedPrice != 0)
                            {
                                isFixedPrice = true;
                            }
                            else if (projectFixedPriceKeepPrices != 0)
                            {
                                projectFixedPrice = projectFixedPrice + projectFixedPriceKeepPrices; //This for now before the crystal reports is updated
                                isFixedPriceKeepPrices = true;
                            }

                            projectElement.Add(
                                new XElement(projectElementName + "IsFixedPriceType1", isFixedPrice.ToInt()),
                                new XElement(projectElementName + "IsFixedPriceType2", isFixedPriceKeepPrices.ToInt()),
                                //new XElement(projectElementName + "FixedPriceKeepPrices", projectFixedPriceKeepPrices < 0 ? Decimal.Negate(projectFixedPriceKeepPrices) : projectFixedPriceKeepPrices),
                                new XElement(projectElementName + "FixedPrice", projectFixedPrice < 0 ? Decimal.Negate(projectFixedPrice) : projectFixedPrice),
                                new XElement(projectElementName + "OpenLiftAmount", projectOpenLiftAmount < 0 ? Decimal.Negate(projectOpenLiftAmount) : projectOpenLiftAmount),
                                new XElement(projectElementName + "InvoicedLiftAmount", projectInvoicedLiftAmount < 0 ? Decimal.Negate(projectInvoicedLiftAmount) : projectInvoicedLiftAmount),
                                new XElement(projectElementName + "RetainedGuaranteeAmount", projectGuaranteeAmount < 0 ? Decimal.Negate(projectGuaranteeAmount) : projectGuaranteeAmount));

                            #endregion

                            #region IB
                            projectElement.Add(
                                new XElement(projectElementName + "BillableMinutesInvoicedSumIB", projectBillableMinutesInvoicedSumIB),
                                new XElement(projectElementName + "PersonellIncomeInvoicedSumIB", projectPersonellIncomeInvoicedSumIB),
                                new XElement(projectElementName + "MateriallIncomeInvoicedSumIB", projectMateriallIncomeInvoicedSumIB),
                                new XElement(projectElementName + "PersonellCostSumIB", projectPersonellCostSumIB),
                                new XElement(projectElementName + "MaterialCostSumIB", projectMaterialCostSumIB),
                                new XElement(projectElementName + "ExpenceCostSumIB", projectExpenceCostSumIB),
                                new XElement(projectElementName + "OverheadCostSumIB", projectOverheadCostSumIB));

                            #endregion

                            projectElement.Add(timeCodeTransactionElements);

                            #endregion

                            #region Default TimeCodeTransaction - Removed, no default element should be added

                            /*if (timeCodeTransactionXmlId == 1)
                                projectElement.Add(new XElement(timeCodeTransactionElementName, String.Empty));*/

                            #endregion

                            if (timeCodeTransactionXmlId > 0)
                            {
                                projectTransactionsReportElement.Add(projectElement);
                                projectXmlId++;
                            }
                        }

                        #endregion
                    }
                }

                #region Default element Project

                if (projectXmlId == 1)
                {
                    projectTransactionsReportElement.Add(new XElement(projectElementName,
                        new XAttribute("id", 1),
                        new XElement(projectElementName + "Nr", String.Empty),
                        new XElement(projectElementName + "Name", String.Empty),
                        new XElement(projectElementName + "Description", String.Empty),
                        new XElement(projectElementName + "Type", 0),
                        new XElement(projectElementName + "Status", 0),
                        new XElement(projectElementName + "StatusName", String.Empty),
                        new XElement(projectElementName + "StartDate", CalendarUtility.DATETIME_DEFAULT),
                        new XElement(projectElementName + "StopDate", CalendarUtility.DATETIME_DEFAULT),
                        new XElement(projectElementName + "Created", CalendarUtility.DATETIME_DEFAULT),
                        new XElement(projectElementName + "CreatedBy", String.Empty),
                        new XElement(projectElementName + "CustomerNr", String.Empty),
                        new XElement(projectElementName + "CustomerName", String.Empty)));
                }

                #endregion

                #endregion

                #endregion
            }

            #region Close document

            rootElement.Add(projectTransactionsReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.ProjectTransactionsReport);

            #endregion
        }

        public XDocument CreateProjectTimeReportData(CreateReportResult reportResult)
        {
            if (reportResult == null || reportResult.ReportTemplateType != SoeReportTemplateType.ProjectTimeReport)
                return null;

            #region Prereq

            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
            List<AccountDimDTO> accountDimInternalsDTOs = AccountManager.GetAccountDimInternalsByCompany(this.ActorCompanyId).ToDTOs();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "ProjectTimeReport");

            //VoucherList
            XElement projectTimeReportElement = new XElement("ProjectTimeReport");

            ActionResult result = new ActionResult(true);
            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateProjectReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateAccountIntervalLabelReportHeaderLabelsElement());
            projectTimeReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateProjectReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(reportParams));
            reportHeaderElement.Add(CreateEmployeeIntervalElement(reportParams));
            reportHeaderElement.Add(CreateAccountIntervalElement(reportResult, reportParams, accountDimInternalsDTOs));

            projectTimeReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateProjectReportPageHeaderLabelsElement(pageHeaderLabelsElement, reportResult.ReportTemplateType);
            projectTimeReportElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Content

            int projectXmlId = 1;
            List<XElement> projectElements = new List<XElement>();

            if (useProjectTimeBlocks)
                projectElements.AddRange(CreateProjectTimeReportDataContentFromProjectTimeBlocks(reportResult, reportParams, ref projectXmlId));

            #region Default element Project

            if (projectXmlId == 1)
            {
                XElement defProjectElement = new XElement("Project",
                           new XAttribute("id", 1),
                           new XElement("ProjectNumber", ""),
                           new XElement("ProjectName", ""),
                           new XElement("ProjectDescription", ""),
                           new XElement("ProjectCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                           new XElement("ProjectCreatedBy", ""),
                           new XElement("ProjectState", 0));

                XElement defInvoiceElement = new XElement("Invoice",
                            new XAttribute("id", 1),
                            new XElement("InvoiceNr", ""),
                            new XElement("InvoiceCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("InvoiceCreatedBy", "")
                            );

                XElement defInvoiceRowElement = new XElement("CustomerInvoiceRow",
                           new XAttribute("id", 1),
                           new XElement("InvoiceRowCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                           new XElement("InvoiceRowCreatedBy", ""),
                           new XElement("InvoiceRowProductNumber", ""),
                           new XElement("InvoiceRowProductName", ""),
                           new XElement("InvoiceRowProductDescription", ""),
                           new XElement("InvoiceRowUnitName", "")
                           );

                XElement defPayrollTransactionElement = new XElement("TimePayrollTransaction",
                            new XAttribute("id", 1),
                            new XElement("EmployeeNr", 0),
                            new XElement("EmployeeName", ""),
                            new XElement("Date", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("PayrollTransactionQuantity", 0),
                            new XElement("PayrollTransactionCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("PayrollTransactionCreatedBy", ""),
                            new XElement("PayrollTransactionExported", 0),
                            new XElement("PayrollTransactionProductNumber", ""),
                            new XElement("PayrollTransactionProductName", ""),
                            new XElement("PayrollTransactionProductDescription", ""),
                            new XElement("WeekNumber", ""),
                            new XElement("IsoDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("PayrollTransactionTimeCodeCode", ""),
                            new XElement("PayrollTransactionTimeCodeName", ""),
                            new XElement("PayrollTransactionTimeCodeDescription", ""),
                            new XElement("PayrollTransactionTimeCodeTransactionId", 0),
                            new XElement("PayrollTransactionTimeCodeTransactionQuantity", 0),
                            new XElement("PayrollTransactionTimeCodeTransactionInvoiceQuantity", 0),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectId", 0),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectNumber", string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectName", string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectNote", string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionParentProjectNumber", string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionParentProjectName", string.Empty),
                            new XElement("PayrollType", 0),
                            new XElement("isPayed", 0));

                XElement defInvoiceTransactionElement = new XElement("TimeInvoiceTransaction",
                        new XAttribute("id", 1),
                        new XElement("EmployeeNr", 0),
                        new XElement("EmployeeName", ""),
                        new XElement("Date", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                        new XElement("InvoiceTransactionQuantity", 0),
                        new XElement("InvoiceTransactionInvoiceQuantity", 0),
                        new XElement("InvoiceTransactionCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                        new XElement("InvoiceTransactionCreatedBy", ""),
                        new XElement("InvoiceTransactionExported", 0),
                        new XElement("InvoiceTransactionProductNumber", ""),
                        new XElement("InvoiceTransactionProductName", ""),
                        new XElement("InvoiceTransactionProductDescription", ""),
                        new XElement("InvoiceTransactionTimeCodeCode", ""),
                        new XElement("InvoiceTransactionTimeCodeName", ""),
                        new XElement("InvoiceTransactionTimeCodeDescription", ""),
                        new XElement("InvoiceTransactionTimeCodeTransactionId", 0),
                        new XElement("InvoiceTransactionTimeCodeTransactionQuantity", 0),
                        new XElement("InvoiceTransactionTimeCodeTransactionInvoiceQuantity", 0),
                        new XElement("InvoiceTransactionTimeCodeTransactionProjectId", 0),
                        new XElement("InvoiceTransactionTimeCodeTransactionProjectNumber", string.Empty),
                        new XElement("InvoiceTransactionTimeCodeTransactionProjectName", string.Empty),
                        new XElement("InvoiceTransactionTimeCodeTransactionProjectNote", string.Empty),
                        new XElement("InvoiceTransactionTimeCodeTransactionParentProjectNumber", string.Empty),
                        new XElement("InvoiceTransactionTimeCodeTransactionParentProjectName", string.Empty),
                        new XElement("WeekNumber", string.Empty),
                        new XElement("IsoDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()));

                XElement defMergedTransaction = new XElement("MergedTransaction",
                            new XAttribute("id", 0),
                            new XElement("EmployeeNr", string.Empty),
                            new XElement("EmployeeName", string.Empty),
                            new XElement("Date", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("Note", string.Empty),
                            new XElement("WeekNumber", 0),
                            new XElement("IsoDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("PayrollTransactionQuantity", 0),
                            new XElement("PayrollTransactionCreated", 0),
                            new XElement("PayrollTransactionCreatedBy", string.Empty),
                            new XElement("PayrollTransactionExported", 0),
                            new XElement("PayrollTransactionProductNumber", string.Empty),
                            new XElement("PayrollTransactionProductName", string.Empty),
                            new XElement("PayrollTransactionProductDescription", string.Empty),
                            new XElement("InvoiceTransactionQuantity", 0),
                            new XElement("InvoiceTransactionInvoiceQuantity", 0),
                            new XElement("InvoiceTransactionCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("InvoiceTransactionCreatedBy", string.Empty),
                            new XElement("InvoiceTransactionExported", 0),
                            new XElement("InvoiceTransactionProductNumber", string.Empty),
                            new XElement("InvoiceTransactionProductName", string.Empty),
                            new XElement("InvoiceTransactionProductDescription", string.Empty),
                            new XElement("TimeCodeCode", string.Empty),
                            new XElement("TimeCodeName", string.Empty),
                            new XElement("TimeCodeDescription", string.Empty),
                            new XElement("TimeCodeTransactionId", 0),
                            new XElement("TimeCodeTransactionQuantity", 0),
                            new XElement("TimeCodeTransactionInvoiceQuantity", 0),
                            new XElement("TimeCodeTransactionProjectId", 0),
                            new XElement("TimeCodeTransactionProjectNumber", string.Empty),
                            new XElement("TimeCodeTransactionProjectName", string.Empty),
                            new XElement("TimeCodeTransactionProjectNote", string.Empty),
                            new XElement("TimeCodeTransactionParentProjectNumber", string.Empty),
                            new XElement("TimeCodeTransactionParentProjectName", string.Empty),
                            new XElement("PayrollType", 0),
                            new XElement("isPayed", 0),
                            new XElement("InvoiceNr", string.Empty),
                            new XElement("OriginType", 0),
                            new XElement("PayrollTransactionInvoiceCustomerName", string.Empty),
                            new XElement("PayrollTransactionInvoiceCustomerNr", string.Empty),
                            new XElement("TimeDeviationCauseName", string.Empty),
                            new XElement("PayrollTransactionUnitPrice", 0),
                            new XElement("PayrollAttestStateName", string.Empty)
                            );

                defInvoiceElement.Add(defInvoiceRowElement);
                defProjectElement.Add(defInvoiceElement);
                defProjectElement.Add(defPayrollTransactionElement);
                defProjectElement.Add(defInvoiceTransactionElement);

                projectElements.Add(defProjectElement);
            }

            #endregion

            #region Add Project

            foreach (XElement project in projectElements)
            {
                projectTimeReportElement.Add(project);
            }

            projectElements.Clear();

            #endregion

            #endregion

            #region Close document

            rootElement.Add(projectTimeReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.ProjectTimeReport);

            #endregion
        }

        public XDocument CreateProjectStatisticsData(CreateReportResult es)
        {
            //if (es == null || es.ReportTemplateType != SoeReportTemplateType.ProjectStatisticsReport)
            //    return null;

            #region Prereq

            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
            List<AccountDimDTO> accountDimInternalsDTOs = AccountManager.GetAccountDimInternalsByCompany(this.ActorCompanyId).ToDTOs();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "ProjectStatistics");

            //VoucherList
            XElement projectStatisticsElement = new XElement("ProjectStatistics");

            this.Company = CompanyManager.GetCompany(es.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, es, entitiesReadOnly, this);

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateProjectReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateAccountIntervalLabelReportHeaderLabelsElement());
            projectStatisticsElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateProjectReportHeaderElement(es);
            reportHeaderElement.Add(CreateDateIntervalElement(reportParams));
            reportHeaderElement.Add(reportParams.CreateEmployeeIntervalElement());
            reportHeaderElement.Add(reportParams.CreateProjectIntervalElement());
            reportHeaderElement.Add(reportParams.CreateCustomerIntervalElement());
            reportHeaderElement.Add(CreateAccountIntervalElement(es, reportParams, accountDimInternalsDTOs));

            projectStatisticsElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateProjectReportPageHeaderLabelsElement(pageHeaderLabelsElement, es.ReportTemplateType);
            projectStatisticsElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Content

            int projectXmlId = 1;
            List<XElement> projectElements = new List<XElement>();

            if (useProjectTimeBlocks)
                projectElements.AddRange(CreateProjectStatisticsDataContentFromProjectTimeBlocks(es, reportParams, ref projectXmlId));
            else
                projectElements.AddRange(CreateProjectStatisticsDataContentFromProjectInvoiceDays(es, reportParams, ref projectXmlId));

            #region Default element Project

            if (projectXmlId == 1)
            {
                XElement defProjectElement = new XElement("Project",
                           new XAttribute("id", 1),
                           new XElement("ProjectNumber", ""),
                           new XElement("ProjectName", ""),
                           new XElement("ProjectDescription", ""),
                           new XElement("ProjectCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                           new XElement("ProjectCreatedBy", ""),
                           new XElement("ProjectState", 0));

                XElement defInvoiceElement = new XElement("Invoice",
                            new XAttribute("id", 1),
                            new XElement("InvoiceNr", ""),
                            new XElement("InvoiceCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("InvoiceCreatedBy", "")
                            );

                XElement defInvoiceRowElement = new XElement("CustomerInvoiceRow",
                           new XAttribute("id", 1),
                           new XElement("InvoiceRowCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                           new XElement("InvoiceRowCreatedBy", ""),
                           new XElement("InvoiceRowProductNumber", ""),
                           new XElement("InvoiceRowProductName", ""),
                           new XElement("InvoiceRowProductDescription", ""),
                           new XElement("InvoiceRowUnitName", "")
                           );
                XElement invoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                               new XAttribute("id", 1),
                               new XElement("ProjectNumber", " "),
                               new XElement("ProjectName", " "),
                               new XElement("InvoiceNr", " "),
                               new XElement("InvoiceCustomerName", " "),
                               new XElement("InvoiceExpenseRowEmployeeName", " "),
                               new XElement("InvoiceExpenseRowTimeCodeName", " "),
                               new XElement("InvoiceExpenseRowPayrollAttestStateName", " "),
                               new XElement("InvoiceExpenseRowQuantity", 0),
                               new XElement("InvoiceExpenseRowQuantityType", 0),
                               new XElement("InvoiceExpenseRowUnitPrice", 0),
                               new XElement("InvoiceExpenseRowAmount", 0),
                               new XElement("InvoiceExpenseRowInvoicedAmount", 0),
                               new XElement("InvoiceExpenseRowDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                               new XElement("InvoiceExpenseRowEmployeeNr", " "),
                               new XElement("InvoiceExpenseRowPayrollProductNr", " "),
                               new XElement("InvoiceExpenseRowPayrollProductName", " ")
                               );

                XElement defPayrollTransactionElement = new XElement("TimePayrollTransaction",
                            new XAttribute("id", 1),
                            new XElement("EmployeeNr", 0),
                            new XElement("EmployeeName", ""),
                            new XElement("Date", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("PayrollTransactionQuantity", 0),
                            new XElement("PayrollTransactionCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("PayrollTransactionCreatedBy", ""),
                            new XElement("PayrollTransactionExported", 0),
                            new XElement("PayrollTransactionProductNumber", ""),
                            new XElement("PayrollTransactionProductName", ""),
                            new XElement("PayrollTransactionProductDescription", ""),
                            new XElement("WeekNumber", ""),
                            new XElement("IsoDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("PayrollTransactionTimeCodeCode", ""),
                            new XElement("PayrollTransactionTimeCodeName", ""),
                            new XElement("PayrollTransactionTimeCodeDescription", ""),
                            new XElement("PayrollTransactionTimeCodeTransactionId", 0),
                            new XElement("PayrollTransactionTimeCodeTransactionQuantity", 0),
                            new XElement("PayrollTransactionTimeCodeTransactionInvoiceQuantity", 0),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectId", 0),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectNumber", string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectName", string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectNote", string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionParentProjectNumber", string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionParentProjectName", string.Empty),
                            new XElement("PayrollType", 0),
                            new XElement("isPayed", 0));

                XElement defInvoiceTransactionElement = new XElement("TimeInvoiceTransaction",
                        new XAttribute("id", 1),
                        new XElement("EmployeeNr", 0),
                        new XElement("EmployeeName", ""),
                        new XElement("Date", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                        new XElement("InvoiceTransactionQuantity", 0),
                        new XElement("InvoiceTransactionInvoiceQuantity", 0),
                        new XElement("InvoiceTransactionCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                        new XElement("InvoiceTransactionCreatedBy", ""),
                        new XElement("InvoiceTransactionExported", 0),
                        new XElement("InvoiceTransactionProductNumber", ""),
                        new XElement("InvoiceTransactionProductName", ""),
                        new XElement("InvoiceTransactionProductDescription", ""),
                        new XElement("InvoiceTransactionTimeCodeCode", ""),
                        new XElement("InvoiceTransactionTimeCodeName", ""),
                        new XElement("InvoiceTransactionTimeCodeDescription", ""),
                        new XElement("InvoiceTransactionTimeCodeTransactionId", 0),
                        new XElement("InvoiceTransactionTimeCodeTransactionQuantity", 0),
                        new XElement("InvoiceTransactionTimeCodeTransactionInvoiceQuantity", 0),
                        new XElement("InvoiceTransactionTimeCodeTransactionProjectId", 0),
                        new XElement("InvoiceTransactionTimeCodeTransactionProjectNumber", string.Empty),
                        new XElement("InvoiceTransactionTimeCodeTransactionProjectName", string.Empty),
                        new XElement("InvoiceTransactionTimeCodeTransactionProjectNote", string.Empty),
                        new XElement("InvoiceTransactionTimeCodeTransactionParentProjectNumber", string.Empty),
                        new XElement("InvoiceTransactionTimeCodeTransactionParentProjectName", string.Empty),
                        new XElement("WeekNumber", string.Empty),
                        new XElement("IsoDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()));

                XElement defMergedTransaction = new XElement("MergedTransaction",
                            new XAttribute("id", 0),
                            new XElement("EmployeeNr", string.Empty),
                            new XElement("EmployeeName", string.Empty),
                            new XElement("Date", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("Note", string.Empty),
                            new XElement("WeekNumber", 0),
                            new XElement("IsoDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("PayrollTransactionQuantity", 0),
                            new XElement("PayrollTransactionCreated", 0),
                            new XElement("PayrollTransactionCreatedBy", string.Empty),
                            new XElement("PayrollTransactionExported", 0),
                            new XElement("PayrollTransactionProductNumber", string.Empty),
                            new XElement("PayrollTransactionProductName", string.Empty),
                            new XElement("PayrollTransactionProductDescription", string.Empty),
                            new XElement("InvoiceTransactionQuantity", 0),
                            new XElement("InvoiceTransactionInvoiceQuantity", 0),
                            new XElement("InvoiceTransactionCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("InvoiceTransactionCreatedBy", string.Empty),
                            new XElement("InvoiceTransactionExported", 0),
                            new XElement("InvoiceTransactionProductNumber", string.Empty),
                            new XElement("InvoiceTransactionProductName", string.Empty),
                            new XElement("InvoiceTransactionProductDescription", string.Empty),
                            new XElement("TimeCodeCode", string.Empty),
                            new XElement("TimeCodeName", string.Empty),
                            new XElement("TimeCodeDescription", string.Empty),
                            new XElement("TimeCodeTransactionId", 0),
                            new XElement("TimeCodeTransactionQuantity", 0),
                            new XElement("TimeCodeTransactionInvoiceQuantity", 0),
                            new XElement("TimeCodeTransactionProjectId", 0),
                            new XElement("TimeCodeTransactionProjectNumber", string.Empty),
                            new XElement("TimeCodeTransactionProjectName", string.Empty),
                            new XElement("TimeCodeTransactionProjectNote", string.Empty),
                            new XElement("TimeCodeTransactionParentProjectNumber", string.Empty),
                            new XElement("TimeCodeTransactionParentProjectName", string.Empty),
                            new XElement("PayrollType", 0),
                            new XElement("isPayed", 0),
                            new XElement("InvoiceNr", string.Empty),
                            new XElement("OriginType", 0),
                            new XElement("PayrollTransactionInvoiceCustomerName", string.Empty),
                            new XElement("PayrollTransactionInvoiceCustomerNr", string.Empty),
                            new XElement("TimeDeviationCauseName", string.Empty),
                            new XElement("PayrollTransactionUnitPrice", 0),
                            new XElement("PayrollAttestStateName", string.Empty)
                            );

                defInvoiceElement.Add(defInvoiceRowElement);
                defInvoiceElement.Add(invoiceExpenseRowElement);
                defProjectElement.Add(defInvoiceElement);
                defProjectElement.Add(defPayrollTransactionElement);
                defProjectElement.Add(defInvoiceTransactionElement);

                projectElements.Add(defProjectElement);
            }

            #endregion

            #region Add Project

            foreach (XElement project in projectElements)
            {
                projectStatisticsElement.Add(project);
            }

            projectElements.Clear();

            #endregion

            #endregion

            #region Close document

            rootElement.Add(projectStatisticsElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.ProjectStatisticsReport);

            #endregion
        }

        private List<XElement> CreateProjectStatisticsDataContentFromProjectTimeBlocks(CreateReportResult es, BillingReportParamsDTO reportParam, ref int projectXmlId)
        {
            int timeInvoiceTransactionXmlId = 1;
            int timePayrollTransactionXmlId = 1;
            List<XElement> projectElements = new List<XElement>();

            #region Prereq

            //Get TimeProject's
            var ignoreProjectInterval = (reportParam.SP_ProjectIds == null || reportParam.SP_ProjectIds.Count == 0) && (reportParam.SB_HasProjectNrInterval == false && reportParam.SB_ProjectNrFrom == null && reportParam.SB_ProjectNrTo == null);
            List<TimeProject> projects = ProjectManager.GetTimeProjectsFromSelection(es, reportParam);
            List<int> projectIds = projects.Select(x => x.ProjectId).ToList();

            //Build collections
            List<int> invoiceIdsAll = new List<int>();
            Dictionary<int, List<int>> projectInvoicesDict = new Dictionary<int, List<int>>();

            var employeeids = !string.IsNullOrEmpty(reportParam.SB_EmployeeNrFrom) || !string.IsNullOrEmpty(reportParam.SB_EmployeeNrTo) ?
                EmployeeManager.GetAllEmployeeIdsByEmployeeNrFromTo(es.ActorCompanyId, reportParam.SB_EmployeeNrFrom, reportParam.SB_EmployeeNrTo) :
                EmployeeManager.GetAllEmployeeIds(es.ActorCompanyId, null);

            var timePayrollTransactions = ProjectManager.GetProjectTimeBlockPayrollTransactionsFromSelection(es, reportParam, projectIds, employeeids, ignoreProjectInterval);
            var timeInvoiceTransactions = ProjectManager.GetProjectTimeBlockInvoiceTransactionsFromSelection(es, reportParam, projectIds, employeeids, ignoreProjectInterval);

            var projectTimeBlocks = ProjectManager.GetProjectTimeBlockDTOs(reportParam.DateFrom, reportParam.DateTo, employeeids, projectIds, null, false);
            var tempIds = projectTimeBlocks.Select(t => t.ProjectId).ToList();
            
            //Filter
            projectIds = tempIds.Distinct().ToList();

            // Map invoices
            ProjectManager.GetProjectInvoiceMappingIds(es.ActorCompanyId, projectIds, ref invoiceIdsAll, ref projectInvoicesDict);

            //Get all CustomerInvoices for Projects
            var invoiceItems = ProjectManager.GetInvoicesForTimeProjectsFromSelection(invoiceIdsAll, es, reportParam);

            #endregion

            #region Content

            var projectsToHandle = projects.Where(p => projectIds.Contains(p.ProjectId)).ToList();
            foreach (TimeProject project in projectsToHandle)
            {
                #region Prereq

                //InvoiceId's for current TimeProject
                List<int> invoiceIdsForProject = (from p in projectInvoicesDict
                                                  where p.Key == project.ProjectId
                                                  select p.Value).FirstOrDefault();

                //if we have made a selection on customernr we only want those projects that has an invoice that is connected to the choosen customers
                if (reportParam.SB_HasActorNrInterval)
                {
                    bool jumpOverProject = true;
                    foreach (var invoice in invoiceItems)
                    {
                        if (invoiceIdsForProject != null && invoiceIdsForProject.Contains(invoice.InvoiceId))
                        {
                            jumpOverProject = false;
                            break;
                        }
                    }

                    if (jumpOverProject)
                        continue;
                }

                #endregion

                #region Invoice

                List<XElement> invoiceElements = new List<XElement>();
                int invoiceXmlId = 1;

                foreach (var invoiceItem in invoiceItems)
                {
                    if (invoiceIdsForProject == null)
                        continue;

                    //Check that CustomerInvoice belongs to current TimeProject
                    if (!invoiceIdsForProject.Contains(invoiceItem.InvoiceId))
                        continue;

                    #region CustomerInvoice

                    #region Invoice

                    XElement invoiceElement = new XElement("Invoice",
                            new XAttribute("id", invoiceXmlId),
                            new XElement("InvoiceNr", invoiceItem.InvoiceNr),
                            new XElement("InvoiceCreated", invoiceItem.Created.HasValue ? invoiceItem.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("InvoiceCreatedBy", invoiceItem.CreatedBy),
                            new XElement("InvoiceCustomerNr", invoiceItem.CustomerNr),
                            new XElement("InvoiceCustomerName", invoiceItem.CustomerName));

                    #endregion

                    #region DefaultInvoiceRow

                    XElement invoiceRowElement = new XElement("CustomerInvoiceRow",
                                new XAttribute("id", 1),
                                new XElement("InvoiceRowCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                new XElement("InvoiceRowCreatedBy", ""),
                                new XElement("InvoiceRowProductNumber", ""),
                                new XElement("InvoiceRowProductName", ""),
                                new XElement("InvoiceRowProductDescription", ""),
                                new XElement("InvoiceRowUnitName", ""));

                    invoiceElement.Add(invoiceRowElement);

                    #endregion

                    #region InvoiceExpenseRow

                    int invoiceExpenseRowXmlId = 1;
                    int saveTimeCodeId = 0;
                    //      InvoiceProduct invoiceProduct = null;
                    ProductSmallDTO payrollProduct = null;
                    Dictionary<int, ProductSmallDTO> payrollProducts = new Dictionary<int, ProductSmallDTO>();

                    var expenseRowsForInvoice = ExpenseManager.GetExpenseRowsForReport(invoiceItem.InvoiceId, es.ActorCompanyId, this.UserId, es.RoleId);

                    if (reportParam.HasDateInterval)
                        expenseRowsForInvoice = expenseRowsForInvoice.Where(e => e.From >= reportParam.DateFrom.Date && e.From <= reportParam.DateTo.Date).ToList();

                    foreach (var expenseRow in expenseRowsForInvoice)
                    {
                        if (saveTimeCodeId != expenseRow.TimeCodeId)
                        {
                            saveTimeCodeId = expenseRow.TimeCodeId;

                            if (payrollProducts.Keys.Contains(expenseRow.TimeCodeId))
                                payrollProduct = payrollProducts[expenseRow.TimeCodeId];
                            else
                            {
                                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                                payrollProduct = GetPayrollProductFromTimeCode(entitiesReadOnly, expenseRow.TimeCodeId, es.ActorCompanyId).ToSmallDTO();
                                payrollProducts.Add(saveTimeCodeId, payrollProduct);
                            }
                        }

                        XElement invoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                               new XAttribute("id", invoiceExpenseRowXmlId),
                               new XElement("ProjectNumber", project.Number),
                               new XElement("ProjectName", project.Name),
                               new XElement("InvoiceNr", invoiceItem.InvoiceNr),
                               new XElement("InvoiceCustomerName", invoiceItem.CustomerName),
                               new XElement("InvoiceExpenseRowEmployeeName", expenseRow.EmployeeName),
                               new XElement("InvoiceExpenseRowTimeCodeName", expenseRow.TimeCodeName),
                               new XElement("InvoiceExpenseRowPayrollAttestStateName", expenseRow.PayrollAttestStateName),
                               new XElement("InvoiceExpenseRowQuantity", expenseRow.Quantity),
                               new XElement("InvoiceExpenseRowQuantityType", expenseRow.TimeCodeRegistrationType),
                               new XElement("InvoiceExpenseRowUnitPrice", expenseRow.UnitPrice),
                               new XElement("InvoiceExpenseRowAmount", expenseRow.Amount),
                               new XElement("InvoiceExpenseRowInvoicedAmount", expenseRow.InvoicedAmount),
                               new XElement("InvoiceExpenseRowDate", expenseRow.From.ToShortDateString()),
                               new XElement("InvoiceExpenseRowEmployeeNr", expenseRow.EmployeeNumber),
                               new XElement("InvoiceExpenseRowPayrollProductNr", payrollProduct != null ? payrollProduct.Number : ""),
                               new XElement("InvoiceExpenseRowPayrollProductName", payrollProduct != null ? payrollProduct.Name : "")
                               );

                        invoiceElement.Add(invoiceExpenseRowElement);
                        invoiceExpenseRowXmlId++;
                    }
                    #endregion

                    #region Default element InvoiceExpenseRow

                    if (invoiceExpenseRowXmlId == 1)
                    {
                        XElement invoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                                new XAttribute("id", 1),
                                new XElement("ProjectNumber", " "),
                                new XElement("ProjectName", " "),
                                new XElement("InvoiceNr", " "),
                                new XElement("InvoiceCustomerName", " "),
                                new XElement("InvoiceExpenseRowEmployeeName", " "),
                                new XElement("InvoiceExpenseRowTimeCodeName", " "),
                                new XElement("InvoiceExpenseRowPayrollAttestStateName", " "),
                                new XElement("InvoiceExpenseRowQuantity", 0),
                                new XElement("InvoiceExpenseRowQuantityType", 0),
                                new XElement("InvoiceExpenseRowUnitPrice", 0),
                                new XElement("InvoiceExpenseRowAmount", 0),
                                new XElement("InvoiceExpenseRowInvoicedAmount", 0),
                                new XElement("InvoiceExpenseRowDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                new XElement("InvoiceExpenseRowEmployeeNr", " "),
                                new XElement("InvoiceExpenseRowPayrollProductNr", " "),
                                new XElement("InvoiceExpenseRowPayrollProductName", " ")
                                );
                        invoiceElement.Add(invoiceExpenseRowElement);
                    }
                    #endregion

                    invoiceElements.Add(invoiceElement);
                    invoiceXmlId++;

                    #endregion
                }

                #endregion

                if (invoiceXmlId == 1)
                {
                    #region Default element Invoice

                    XElement defInvoiceElement = new XElement("Invoice",
                            new XAttribute("id", 1),
                            new XElement("InvoiceNr", ""),
                            new XElement("InvoiceCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("InvoiceCreatedBy", "")
                            );
                    XElement defInvoiceRowElement = new XElement("CustomerInvoiceRow",
                           new XAttribute("id", 1),
                           new XElement("InvoiceRowCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                           new XElement("InvoiceRowCreatedBy", ""),
                           new XElement("InvoiceRowProductNumber", ""),
                           new XElement("InvoiceRowProductName", ""),
                           new XElement("InvoiceRowProductDescription", ""),
                           new XElement("InvoiceRowUnitName", "")
                           );
                    XElement defInvoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                                new XAttribute("id", 1),
                                new XElement("ProjectNumber", " "),
                                new XElement("ProjectName", " "),
                                new XElement("InvoiceNr", " "),
                                new XElement("InvoiceCustomerName", " "),
                                new XElement("InvoiceExpenseRowEmployeeName", " "),
                                new XElement("InvoiceExpenseRowTimeCodeName", " "),
                                new XElement("InvoiceExpenseRowPayrollAttestStateName", " "),
                                new XElement("InvoiceExpenseRowQuantity", 0),
                                new XElement("InvoiceExpenseRowQuantityType", 0),
                                new XElement("InvoiceExpenseRowUnitPrice", 0),
                                new XElement("InvoiceExpenseRowAmount", 0),
                                new XElement("InvoiceExpenseRowInvoicedAmount", 0),
                                new XElement("InvoiceExpenseRowDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                new XElement("InvoiceExpenseRowEmployeeNr", " "),
                                new XElement("InvoiceExpenseRowPayrollProductNr", " "),
                                new XElement("InvoiceExpenseRowPayrollProductName", " ")
                                );

                    defInvoiceElement.Add(defInvoiceRowElement);
                    defInvoiceElement.Add(defInvoiceExpenseRowElement);
                    invoiceElements.Add(defInvoiceElement);

                    #endregion
                }

                #region ProjectInvoiceDay
                Employee payrollEmployee = new Employee();

                List<XElement> invoiceTransactionElements = new List<XElement>();
                List<XElement> payrollTransactionElements = new List<XElement>();

                var timePayrollTransactionsForProject = timePayrollTransactions.Where(p => p.ProjectId == project.ProjectId); 
                var timeInvoiceTransactionsForProject = timeInvoiceTransactions.Where(p => p.ProjectId == project.ProjectId);

                if (!timePayrollTransactionsForProject.Any() && !timeInvoiceTransactionsForProject.Any())
                    continue;

                #region TimePayrollTransaction

                foreach (var payrollTransaction in timePayrollTransactionsForProject)
                {

                    decimal empCost = 0;
                    if (payrollEmployee.EmployeeId != payrollTransaction.EmployeeId)
                    {
                        payrollEmployee = EmployeeManager.GetEmployee(payrollTransaction.EmployeeId, es.ActorCompanyId,onlyActive:false);
                    }
                    empCost = EmployeeManager.GetEmployeeCalculatedCost(payrollEmployee, payrollTransaction.Date, payrollTransaction.ProjectId);


                    XElement payrollTransactionElement = new XElement("TimePayrollTransaction",
                                             new XAttribute("id", timePayrollTransactionXmlId),
                                             new XElement("EmployeeNr", payrollTransaction.EmployeeNr),
                                             new XElement("EmployeeName", payrollTransaction.EmployeeName),
                                             new XElement("Date", payrollTransaction.Date.ToShortDateString()),
                                             new XElement("Note", payrollTransaction.ProjectTimeBlockInternalNote),
                                             new XElement("ExternalNote", payrollTransaction.ProjectTimeBlockExternalNote),
                                             new XElement("PayrollTransactionQuantity", payrollTransaction.Quantity),
                                             new XElement("PayrollTransactionCreated", payrollTransaction.Created.HasValue ? payrollTransaction.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                             new XElement("PayrollTransactionCreatedBy", payrollTransaction.CreatedBy),
                                             new XElement("PayrollTransactionExported", payrollTransaction.Exported.ToInt()),
                                             new XElement("PayrollTransactionProductNumber", payrollTransaction.ProductNumber),
                                             new XElement("PayrollTransactionProductName", payrollTransaction.ProductName),
                                             new XElement("PayrollTransactionProductDescription", payrollTransaction.ProductDescription),
                                             new XElement("WeekNumber", CalendarUtility.GetWeekNr(payrollTransaction.Date)),
                                             new XElement("IsoDate", payrollTransaction.Date.ToString("yyyy-MM-dd")),
                                             new XElement("PayrollTransactionTimeCodeCode", payrollTransaction.TimeCode),
                                             new XElement("PayrollTransactionTimeCodeName", payrollTransaction.TimeCodeName),
                                             new XElement("PayrollTransactionTimeCodeDescription", payrollTransaction.TimeCodeDescription),
                                             new XElement("PayrollTransactionTimeCodeTransactionId", payrollTransaction.TimeCodeTransactionId),
                                             new XElement("PayrollTransactionTimeCodeTransactionQuantity", payrollTransaction.Quantity),
                                             new XElement("PayrollTransactionTimeCodeTransactionInvoiceQuantity", payrollTransaction.TimeCodeTransactionInvoiceQuantity.HasValue ? payrollTransaction.TimeCodeTransactionInvoiceQuantity : 0),
                                             new XElement("PayrollTransactionTimeCodeTransactionProjectId", payrollTransaction.ProjectId),
                                             new XElement("PayrollTransactionTimeCodeTransactionProjectNumber", payrollTransaction.ProjectNr),
                                             new XElement("PayrollTransactionTimeCodeTransactionProjectName", payrollTransaction.ProjectName),
                                             new XElement("PayrollTransactionTimeCodeTransactionProjectNote", payrollTransaction.ProjectNote),
                                             new XElement("PayrollTransactionTimeCodeTransactionParentProjectNumber", payrollTransaction.ParentProjectNumber),
                                             new XElement("PayrollTransactionTimeCodeTransactionParentProjectName", payrollTransaction.ParentProjectName),
                                             new XElement("PayrollType", payrollTransaction.PayrollType),
                                             new XElement("isPayed", payrollTransaction.Payed.ToInt()),
                                             new XElement("InvoiceNr", string.IsNullOrEmpty(payrollTransaction.InvoiceNr) ? "" : payrollTransaction.InvoiceNr),
                                             new XElement("OriginType", payrollTransaction.OriginType.HasValue ? payrollTransaction.OriginType : 0),
                                             new XElement("PayrollTransactionInvoiceCustomerName", string.IsNullOrEmpty(payrollTransaction.CustomerName) ? "" : payrollTransaction.CustomerName),
                                             new XElement("PayrollTransactionInvoiceCustomerNr", string.IsNullOrEmpty(payrollTransaction.CustomerNr) ? "" : payrollTransaction.CustomerNr),
                                             new XElement("TimeDeviationCauseName", string.IsNullOrEmpty(payrollTransaction.TimeDeviationCauseName) ? "" : payrollTransaction.TimeDeviationCauseName),
                                             new XElement("PayrollTransactionUnitPrice", empCost),
                                             new XElement("PayrollAttestStateName", payrollTransaction.AttestStateName)
                                             );

                    payrollTransactionElements.Add(payrollTransactionElement);
                    timePayrollTransactionXmlId++;
                }

                #endregion 

                #region TimeInvoiceTransaction

                foreach (var invoiceTransaction in timeInvoiceTransactionsForProject.Where(p => p.CustomerInvoiceId > 0))
                {
                    int weeknr = CalendarUtility.GetWeekNr(invoiceTransaction.Date);

                    XElement invoiceTransactionElement = new XElement("TimeInvoiceTransaction",
                                            new XAttribute("id", timeInvoiceTransactionXmlId),
                                            new XElement("EmployeeNr", invoiceTransaction.EmployeeNr),
                                            new XElement("EmployeeName", invoiceTransaction.EmployeeName),
                                            new XElement("Date", invoiceTransaction.Date.ToShortDateString()),
                                            new XElement("Note", invoiceTransaction.ProjectTimeBlockExternalNote),
                                            new XElement("ExternalNote", invoiceTransaction.Comment),
                                            new XElement("InvoiceTransactionQuantity", invoiceTransaction.Quantity),
                                            new XElement("InvoiceTransactionInvoiceQuantity", invoiceTransaction.InvoiceQuantity),
                                            new XElement("InvoiceTransactionCreated", invoiceTransaction.Created.HasValue ? invoiceTransaction.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                            new XElement("InvoiceTransactionCreatedBy", invoiceTransaction.CreatedBy),
                                            new XElement("InvoiceTransactionExported", invoiceTransaction.Exported.ToInt()),
                                            new XElement("InvoiceTransactionProductNumber", invoiceTransaction.ProductNumber),
                                            new XElement("InvoiceTransactionProductName", invoiceTransaction.ProductName),
                                            new XElement("InvoiceTransactionProductDescription", invoiceTransaction.ProductDescription),
                                            new XElement("InvoiceTransactionTimeCodeCode", invoiceTransaction.TimeCode),
                                            new XElement("InvoiceTransactionTimeCodeName", invoiceTransaction.TimeCodeName),
                                            new XElement("InvoiceTransactionTimeCodeDescription", invoiceTransaction.TimeCodeDescription),
                                            new XElement("InvoiceTransactionTimeCodeTransactionId", invoiceTransaction.TimeCodeTransactionId),
                                            new XElement("InvoiceTransactionTimeCodeTransactionQuantity", invoiceTransaction.Quantity),
                                            new XElement("InvoiceTransactionTimeCodeTransactionInvoiceQuantity", invoiceTransaction.InvoiceQuantity),
                                            new XElement("InvoiceTransactionTimeCodeTransactionProjectId", invoiceTransaction.ProjectId),
                                            new XElement("InvoiceTransactionTimeCodeTransactionProjectNumber", invoiceTransaction.ProjectNr),
                                            new XElement("InvoiceTransactionTimeCodeTransactionProjectName", invoiceTransaction.ProjectName),
                                            new XElement("InvoiceTransactionTimeCodeTransactionProjectNote", invoiceTransaction.ProjectNote),
                                            new XElement("InvoiceTransactionTimeCodeTransactionParentProjectNumber", invoiceTransaction.ParentProjectNumber),
                                            new XElement("InvoiceTransactionTimeCodeTransactionParentProjectName", invoiceTransaction.ParentProjectName),
                                            new XElement("WeekNumber", weeknr.ToString()),
                                            new XElement("IsoDate", invoiceTransaction.Date.ToString("yyyy-MM-dd")),
                                            new XElement("InvoiceNr", invoiceTransaction.InvoiceNr),
                                            new XElement("OriginType", invoiceTransaction.OriginType.HasValue ? invoiceTransaction.OriginType.Value : 0),
                                            new XElement("InvoiceTransactionInvoiceCustomerName", invoiceTransaction.CustomerName),
                                            new XElement("InvoiceTransactionInvoiceCustomerNr", invoiceTransaction.CustomerNr));

                    invoiceTransactionElements.Add(invoiceTransactionElement);
                    timeInvoiceTransactionXmlId++;
                }

                #endregion

                #endregion

                #region Project

                XElement projectElement = new XElement("Project",
                               new XAttribute("id", projectXmlId),
                               new XElement("ProjectNumber", project.Number),
                               new XElement("ProjectName", project.Name),
                               new XElement("ProjectDescription", project.Description),
                               new XElement("ProjectCreated", project.Created.HasValue ? project.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                               new XElement("ProjectCreatedBy", project.CreatedBy),
                               new XElement("ProjectState", project.State),
                               new XElement("ProjectCustomerNr", project.Customer == null ? "" : project.Customer.CustomerNr),
                               new XElement("ProjectCustomerName", project.Customer == null ? "" : project.Customer.Name));

                projectXmlId++;

                #endregion

                #region Add Invoices

                foreach (XElement invoice in invoiceElements)
                {
                    projectElement.Add(invoice);
                }

                invoiceElements.Clear();

                #endregion

                #region Add TimePayrollTransaction

                foreach (XElement transaction in payrollTransactionElements)
                {
                    projectElement.Add(transaction);
                }

                #endregion

                #region Add TimeInvoiceTransaction

                foreach (XElement transaction in invoiceTransactionElements)
                {
                    projectElement.Add(transaction);
                }

                #endregion

                #region Create Merged Transactions

                List<XElement> mergedTransactions = new List<XElement>();
                int mergedTransactionXmlId = 1;

                try
                {
                    foreach (XElement transaction in payrollTransactionElements)
                    {

                        XElement mergedTransaction = new XElement("MergedTransaction",
                                new XAttribute("id", mergedTransactionXmlId),
                                new XElement("EmployeeNr", transaction.Element("EmployeeNr").Value),
                                new XElement("EmployeeName", transaction.Element("EmployeeName").Value),
                                new XElement("Date", transaction.Element("Date").Value),
                                new XElement("Note", transaction.Element("Note").Value),
                                new XElement("ExternalNote", transaction.Element("ExternalNote").Value),
                                new XElement("WeekNumber", transaction.Element("WeekNumber").Value),
                                new XElement("IsoDate", transaction.Element("IsoDate").Value),
                                new XElement("PayrollTransactionQuantity", transaction.Element("PayrollTransactionQuantity").Value),
                                new XElement("PayrollTransactionCreated", transaction.Element("PayrollTransactionCreated").Value),
                                new XElement("PayrollTransactionCreatedBy", transaction.Element("PayrollTransactionCreatedBy").Value),
                                new XElement("PayrollTransactionExported", 0),
                                new XElement("PayrollTransactionProductNumber", transaction.Element("PayrollTransactionProductNumber").Value),
                                new XElement("PayrollTransactionProductName", transaction.Element("PayrollTransactionProductName").Value),
                                new XElement("PayrollTransactionProductDescription", string.Empty),
                                new XElement("InvoiceTransactionQuantity", 0),
                                new XElement("InvoiceTransactionInvoiceQuantity", 0),
                                new XElement("InvoiceTransactionCreated", transaction.Element("PayrollTransactionCreated").Value),
                                new XElement("InvoiceTransactionCreatedBy", string.Empty),
                                new XElement("InvoiceTransactionExported", 0),
                                new XElement("InvoiceTransactionProductNumber", string.Empty),
                                new XElement("InvoiceTransactionProductName", string.Empty),
                                new XElement("InvoiceTransactionProductDescription", string.Empty),
                                new XElement("TimeCodeCode", string.Empty),
                                new XElement("TimeCodeName", transaction.Element("PayrollTransactionTimeCodeName").Value),
                                new XElement("TimeCodeDescription", ""),
                                new XElement("TimeCodeTransactionId", 0),
                                new XElement("TimeCodeTransactionQuantity", transaction.Element("PayrollTransactionTimeCodeTransactionQuantity").Value),
                                new XElement("TimeCodeTransactionInvoiceQuantity", transaction.Element("PayrollTransactionTimeCodeTransactionInvoiceQuantity").Value),
                                new XElement("TimeCodeTransactionProjectId", transaction.Element("PayrollTransactionTimeCodeTransactionProjectId").Value),
                                new XElement("TimeCodeTransactionProjectNumber", transaction.Element("PayrollTransactionTimeCodeTransactionProjectNumber").Value),
                                new XElement("TimeCodeTransactionProjectName", transaction.Element("PayrollTransactionTimeCodeTransactionProjectName").Value),
                                new XElement("TimeCodeTransactionProjectNote", string.Empty),
                                new XElement("TimeCodeTransactionParentProjectNumber", string.Empty),
                                new XElement("TimeCodeTransactionParentProjectName", string.Empty),
                                new XElement("PayrollType", 0),
                                new XElement("isPayed", 0),
                                new XElement("InvoiceNr", transaction.Element("InvoiceNr").Value),
                                new XElement("OriginType", 0),
                                new XElement("PayrollTransactionInvoiceCustomerName", transaction.Element("PayrollTransactionInvoiceCustomerName").Value),
                                new XElement("PayrollTransactionInvoiceCustomerNr", string.Empty),
                                new XElement("TimeDeviationCauseName", transaction.Element("TimeDeviationCauseName").Value),
                                new XElement("PayrollTransactionUnitPrice", 0),
                                new XElement("PayrollAttestStateName", string.Empty)
                                );

                        mergedTransactions.Add(mergedTransaction);

                        mergedTransactionXmlId++;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
                }

                payrollTransactionElements.Clear();

                try
                {
                    foreach (XElement transaction in invoiceTransactionElements)
                    {
                        XElement mergedTransaction = new XElement("MergedTransaction",
                                new XAttribute("id", mergedTransactionXmlId),
                                new XElement("EmployeeNr", transaction.Element("EmployeeNr").Value),
                                new XElement("EmployeeName", transaction.Element("EmployeeName").Value),
                                new XElement("Date", transaction.Element("Date").Value),
                                new XElement("Note", transaction.Element("Note").Value),
                                new XElement("ExternalNote", transaction.Element("ExternalNote").Value),
                                new XElement("WeekNumber", transaction.Element("WeekNumber").Value),
                                new XElement("IsoDate", transaction.Element("IsoDate").Value),
                                new XElement("PayrollTransactionQuantity", 0),
                                new XElement("PayrollTransactionCreated", 0),
                                new XElement("PayrollTransactionCreatedBy", string.Empty),
                                new XElement("PayrollTransactionExported", 0),
                                new XElement("PayrollTransactionProductNumber", string.Empty),
                                new XElement("PayrollTransactionProductName", string.Empty),
                                new XElement("PayrollTransactionProductDescription", string.Empty),
                                new XElement("InvoiceTransactionQuantity", transaction.Element("InvoiceTransactionQuantity").Value),
                                new XElement("InvoiceTransactionInvoiceQuantity", transaction.Element("InvoiceTransactionInvoiceQuantity").Value),
                                new XElement("InvoiceTransactionCreated", transaction.Element("InvoiceTransactionCreated").Value),
                                new XElement("InvoiceTransactionCreatedBy", transaction.Element("InvoiceTransactionCreatedBy").Value),
                                new XElement("InvoiceTransactionExported", transaction.Element("InvoiceTransactionExported").Value),
                                new XElement("InvoiceTransactionProductNumber", transaction.Element("InvoiceTransactionProductNumber").Value),
                                new XElement("InvoiceTransactionProductName", transaction.Element("InvoiceTransactionProductName").Value),
                                new XElement("InvoiceTransactionProductDescription", transaction.Element("InvoiceTransactionProductDescription").Value),
                                new XElement("TimeCodeCode", transaction.Element("InvoiceTransactionTimeCodeCode").Value),
                                new XElement("TimeCodeName", transaction.Element("InvoiceTransactionTimeCodeName").Value),
                                new XElement("TimeCodeDescription", transaction.Element("InvoiceTransactionTimeCodeDescription").Value),
                                new XElement("TimeCodeTransactionId", transaction.Element("InvoiceTransactionTimeCodeTransactionId").Value),
                                new XElement("TimeCodeTransactionQuantity", transaction.Element("InvoiceTransactionTimeCodeTransactionQuantity").Value),
                                new XElement("TimeCodeTransactionInvoiceQuantity", transaction.Element("InvoiceTransactionTimeCodeTransactionInvoiceQuantity").Value),
                                new XElement("TimeCodeTransactionProjectId", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectId").Value),
                                new XElement("TimeCodeTransactionProjectNumber", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectNumber").Value),
                                new XElement("TimeCodeTransactionProjectName", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectName").Value),
                                new XElement("TimeCodeTransactionProjectNote", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectNote").Value),
                                new XElement("TimeCodeTransactionParentProjectNumber", transaction.Element("InvoiceTransactionTimeCodeTransactionParentProjectNumber").Value),
                                new XElement("TimeCodeTransactionParentProjectName", transaction.Element("InvoiceTransactionTimeCodeTransactionParentProjectName").Value),
                                new XElement("PayrollType", 0),
                                new XElement("isPayed", 0),
                                new XElement("InvoiceNr", transaction.Element("InvoiceNr").Value),
                                new XElement("OriginType", transaction.Element("OriginType").Value),
                                new XElement("PayrollTransactionInvoiceCustomerName", transaction.Element("InvoiceTransactionInvoiceCustomerName").Value),
                                new XElement("PayrollTransactionInvoiceCustomerNr", transaction.Element("InvoiceTransactionInvoiceCustomerNr").Value),
                                new XElement("TimeDeviationCauseName", ""),
                                new XElement("PayrollTransactionUnitPrice", 0),
                                new XElement("PayrollAttestStateName", string.Empty));

                        mergedTransactions.Add(mergedTransaction);

                        mergedTransactionXmlId++;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
                }

                invoiceTransactionElements.Clear();

                #endregion

                projectElement.Add(mergedTransactions);

                mergedTransactions.Clear();

                projectElements.Add(projectElement);
            }

            #endregion

            return projectElements;
        }

        private List<XElement> CreateProjectStatisticsDataContentFromProjectInvoiceDays(CreateReportResult es, BillingReportParamsDTO reportParam, ref int projectXmlId)
        {
            try
            {
                int timeInvoiceTransactionXmlId = 1;
                int timePayrollTransactionXmlId = 1;
                List<XElement> projectElements = new List<XElement>();

                #region Prereq

                List<Employee> employeeLocalCache = new List<Employee>();
                List<PayrollProduct> payrollProductLocalCache = new List<PayrollProduct>();
                Dictionary<int, PayrollProduct> timeCodePayrollProductDict = new Dictionary<int, PayrollProduct>();
                Dictionary<int, decimal> employeeCalculatedCostDict = new Dictionary<int, decimal>();

                //Get TimeProject's
                List<TimeProject> projects = ProjectManager.GetTimeProjectsFromSelection(es, reportParam);
                List<int> projectIds = projects.Select(x => x.ProjectId).ToList();

                //Build collections
                List<int> invoiceIdsAll = new List<int>();
                List<int> projectDayIdsAll = new List<int>();
                Dictionary<int, List<int>> projectDaysDict = new Dictionary<int, List<int>>();
                Dictionary<int, List<int>> projectInvoicesDict = new Dictionary<int, List<int>>();

                ProjectManager.GetProjectDayMappingIds(es.ActorCompanyId, projectIds, ref projectDayIdsAll, ref projectDaysDict);

                //Get all ProjectInvoiceDay's for Projects
                var timePayrollTransactions = ProjectManager.GetTimePayrollTransactionsFromSelection(projectDayIdsAll, es, reportParam);
                var timeInvoiceTransactions = ProjectManager.GetTimeInvoiceTransactionsFromSelection(projectDayIdsAll, es, reportParam);
                var timeSheetWeekTransactions = ProjectManager.GetTimeSheetTimeCodeTransactionsFromSelection(es, reportParam);

                var tempIds = timePayrollTransactions.Select(t => t.ProjectId).ToList();
                tempIds.AddRange(timeInvoiceTransactions.Select(t => t.ProjectId));
                tempIds.AddRange(timeSheetWeekTransactions.Select(t => t.ProjectId.Value));

                //Filter
                projectIds = tempIds.Distinct().ToList();

                // Map invoices
                ProjectManager.GetProjectInvoiceMappingIds(es.ActorCompanyId, projectIds, ref invoiceIdsAll, ref projectInvoicesDict);

                //Get all CustomerInvoices for Projects
                var invoiceItems = ProjectManager.GetInvoicesForTimeProjectsFromSelection(invoiceIdsAll, es, reportParam);

                // Get expenses for interval
                var employeeId = EmployeeManager.GetEmployeeIdForUser(base.UserId, base.ActorCompanyId);
                var allExpenseRows = ExpenseManager.GetExpenseRowsForGridFiltered(base.ActorCompanyId, base.UserId, es.RoleId, employeeId, reportParam.DateFrom, reportParam.DateTo, null, projectIds, null, null);

                #endregion

                #region Content

                foreach (TimeProject project in projects.Where(p => projectIds.Contains(p.ProjectId)))
                {
                    #region Prereq

                    //TimeSheet transactions
                    var timeSheetTransactions = timeSheetWeekTransactions.Where(t => t.ProjectId == project.ProjectId);
                    //var timeSheetTransactions = ProjectManager.GetTimeSheetPayrollTransactionForProject(project.ProjectId);

                    //InvoiceId's for current TimeProject
                    List<int> invoiceIdsForProject = (from p in projectInvoicesDict
                                                      where p.Key == project.ProjectId
                                                      select p.Value).FirstOrDefault();

                    if (invoiceIdsForProject == null && timeSheetTransactions == null)
                        continue;

                    //if we have made a selection on customernr we only want those projects that has an invoice that is connected to the choosen customers
                    if (reportParam.SB_HasActorNrInterval)
                    {
                        bool jumpOverProject = true;
                        foreach (var invoice in invoiceItems)
                        {
                            if (invoiceIdsForProject != null && invoiceIdsForProject.Contains(invoice.InvoiceId))
                            {
                                jumpOverProject = false;
                                break;
                            }
                        }

                        if (jumpOverProject)
                            continue;
                    }

                    #endregion

                    #region Invoice

                    List<XElement> invoiceElements = new List<XElement>();
                    int invoiceXmlId = 1;

                    foreach (var invoiceItem in invoiceItems)
                    {
                        if (invoiceIdsForProject == null)
                            continue;

                        //Check that CustomerInvoice belongs to current TimeProject
                        if (!invoiceIdsForProject.Contains(invoiceItem.InvoiceId))
                            continue;

                        #region CustomerInvoice

                        #region Invoice

                        XElement invoiceElement = new XElement("Invoice",
                                new XAttribute("id", invoiceXmlId),
                                new XElement("InvoiceNr", invoiceItem.InvoiceNr),
                                new XElement("InvoiceCreated", invoiceItem.Created.HasValue ? invoiceItem.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                new XElement("InvoiceCreatedBy", invoiceItem.CreatedBy),
                                new XElement("InvoiceCustomerNr", invoiceItem.CustomerNr),
                                new XElement("InvoiceCustomerName", invoiceItem.CustomerName));

                        #endregion

                        #region DefaultInvoiceRow

                        XElement invoiceRowElement = new XElement("CustomerInvoiceRow",
                                    new XAttribute("id", 1),
                                    new XElement("InvoiceRowCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                    new XElement("InvoiceRowCreatedBy", ""),
                                    new XElement("InvoiceRowProductNumber", ""),
                                    new XElement("InvoiceRowProductName", ""),
                                    new XElement("InvoiceRowProductDescription", ""),
                                    new XElement("InvoiceRowUnitName", ""));

                        invoiceElement.Add(invoiceRowElement);

                        #endregion

                        #region InvoiceExpenseRow

                        int invoiceExpenseRowXmlId = 1;
                        //int saveTimeCodeId = 0;
                        //     InvoiceProduct invoiceProduct = null;
                        PayrollProduct payrollProduct = null;
                        var expenseRowsForInvoice = allExpenseRows.Where(r => r.OrderId == invoiceItem.InvoiceId);
                        foreach (var expenseRow in expenseRowsForInvoice)
                        {
                            //Check expensrows 
                            if (expenseRow.InvoiceRowAttestStateId == 0)
                                continue;

                            if (reportParam.HasDateInterval && (expenseRow.From < reportParam.DateFrom.Date || expenseRow.From > reportParam.DateTo.Date))
                                continue;

                            if (timeCodePayrollProductDict.ContainsKey(expenseRow.TimeCodeId))
                            {
                                payrollProduct = timeCodePayrollProductDict.FirstOrDefault(i => i.Key == expenseRow.TimeCodeId).Value;
                            }
                            else
                            {
                                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                                payrollProduct = GetPayrollProductFromTimeCode(entitiesReadOnly, expenseRow.TimeCodeId, es.ActorCompanyId);
                                timeCodePayrollProductDict.Add(expenseRow.TimeCodeId, payrollProduct);
                                if (payrollProduct != null)
                                    payrollProductLocalCache.Add(payrollProduct);
                            }

                            XElement invoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                                   new XAttribute("id", invoiceExpenseRowXmlId),
                                   new XElement("ProjectNumber", project.Number),
                                   new XElement("ProjectName", project.Name),
                                   new XElement("InvoiceNr", invoiceItem.InvoiceNr),
                                   new XElement("InvoiceCustomerName", invoiceItem.CustomerName),
                                   new XElement("InvoiceExpenseRowEmployeeName", expenseRow.EmployeeName),
                                   new XElement("InvoiceExpenseRowTimeCodeName", expenseRow.TimeCodeName),
                                   new XElement("InvoiceExpenseRowPayrollAttestStateName", expenseRow.PayrollAttestStateName),
                                   new XElement("InvoiceExpenseRowQuantity", expenseRow.Quantity),
                                   new XElement("InvoiceExpenseRowQuantityType", expenseRow.TimeCodeRegistrationType),
                                   new XElement("InvoiceExpenseRowUnitPrice", expenseRow.UnitPrice),
                                   new XElement("InvoiceExpenseRowAmount", expenseRow.Amount),
                                   new XElement("InvoiceExpenseRowInvoicedAmount", expenseRow.InvoicedAmount),
                                   new XElement("InvoiceExpenseRowDate", expenseRow.From.ToShortDateString()),
                                   new XElement("InvoiceExpenseRowEmployeeNr", expenseRow.EmployeeNumber),
                                   new XElement("InvoiceExpenseRowPayrollProductNr", payrollProduct != null ? payrollProduct.Number : ""),
                                   new XElement("InvoiceExpenseRowPayrollProductName", payrollProduct != null ? payrollProduct.Name : "")
                                   );

                            invoiceElement.Add(invoiceExpenseRowElement);
                            invoiceExpenseRowXmlId++;
                        }
                        #endregion

                        #region Default element InvoiceExpenseRow

                        if (invoiceExpenseRowXmlId == 1)
                        {
                            XElement invoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                                    new XAttribute("id", 1),
                                    new XElement("ProjectNumber", " "),
                                    new XElement("ProjectName", " "),
                                    new XElement("InvoiceNr", " "),
                                    new XElement("InvoiceCustomerName", " "),
                                    new XElement("InvoiceExpenseRowEmployeeName", " "),
                                    new XElement("InvoiceExpenseRowTimeCodeName", " "),
                                    new XElement("InvoiceExpenseRowPayrollAttestStateName", " "),
                                    new XElement("InvoiceExpenseRowQuantity", 0),
                                    new XElement("InvoiceExpenseRowQuantityType", 0),
                                    new XElement("InvoiceExpenseRowUnitPrice", 0),
                                    new XElement("InvoiceExpenseRowAmount", 0),
                                    new XElement("InvoiceExpenseRowInvoicedAmount", 0),
                                    new XElement("InvoiceExpenseRowDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                    new XElement("InvoiceExpenseRowEmployeeNr", " "),
                                    new XElement("InvoiceExpenseRowPayrollProductNr", " "),
                                    new XElement("InvoiceExpenseRowPayrollProductName", " ")
                                    );
                            invoiceElement.Add(invoiceExpenseRowElement);
                        }
                        #endregion

                        invoiceElements.Add(invoiceElement);
                        invoiceXmlId++;

                        #endregion
                    }

                    #endregion

                    if (invoiceXmlId == 1)
                    {
                        #region Default element Invoice

                        XElement defInvoiceElement = new XElement("Invoice",
                                new XAttribute("id", 1),
                                new XElement("InvoiceNr", ""),
                                new XElement("InvoiceCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                new XElement("InvoiceCreatedBy", "")
                                );
                        XElement defInvoiceRowElement = new XElement("CustomerInvoiceRow",
                               new XAttribute("id", 1),
                               new XElement("InvoiceRowCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                               new XElement("InvoiceRowCreatedBy", ""),
                               new XElement("InvoiceRowProductNumber", ""),
                               new XElement("InvoiceRowProductName", ""),
                               new XElement("InvoiceRowProductDescription", ""),
                               new XElement("InvoiceRowUnitName", "")
                               );

                        defInvoiceElement.Add(defInvoiceRowElement);
                        invoiceElements.Add(defInvoiceElement);

                        #endregion
                    }

                    #region ProjectInvoiceDay

                    List<XElement> invoiceTransactionElements = new List<XElement>();
                    List<XElement> payrollTransactionElements = new List<XElement>();

                    //ProjectInvoiceDay's for current TimeProject
                    List<int> projectDayIdsForProject = (from p in projectDaysDict
                                                         where p.Key == project.ProjectId
                                                         select p.Value).FirstOrDefault();


                    //Order has been deleted
                    //if (day.ProjectInvoiceWeek.RecordType == (int)SoeProjectRecordType.Order && !invoiceIdsForProject.Contains(day.ProjectInvoiceWeek.RecordId))
                    //    continue;

                    if (projectDayIdsForProject != null && invoiceIdsForProject != null)
                    {
                        #region TimePayrollTransaction
                        Employee payrollEmployee = null;

                        foreach (var payrollTransaction in timePayrollTransactions.Where(x => projectDayIdsForProject.Contains(x.ProjectInvoiceDayId)).ToList())
                        {
                            int weeknr = CalendarUtility.GetWeekNr(payrollTransaction.ProjectInvoiceDayDate);
                            decimal empCost = 0;
                            if (employeeLocalCache.Any(x => x.EmployeeId == payrollTransaction.EmployeeId))
                            {
                                payrollEmployee = employeeLocalCache.FirstOrDefault(x => x.EmployeeId == payrollTransaction.EmployeeId);
                            }
                            else
                            {
                                payrollEmployee = EmployeeManager.GetEmployee(payrollTransaction.EmployeeId, es.ActorCompanyId, onlyActive: false, loadContactPerson: true);
                                if (payrollEmployee != null)
                                    employeeLocalCache.Add(payrollEmployee);
                            }

                            if (payrollEmployee == null) //employee has been deleted
                                continue;

                            if (employeeCalculatedCostDict.ContainsKey(payrollEmployee.EmployeeId))
                            {
                                empCost = employeeCalculatedCostDict.FirstOrDefault(i => i.Key == payrollEmployee.EmployeeId).Value;
                            }
                            else
                            {
                                empCost = EmployeeManager.GetEmployeeCalculatedCost(payrollEmployee, payrollTransaction.ProjectInvoiceDayDate, payrollTransaction.TimeCodeTransactionProjectId);
                                employeeCalculatedCostDict.Add(payrollEmployee.EmployeeId, empCost);
                            }


                            Project payrollTransactionTimeCodeTransactionProject = new Project();
                            if (payrollTransaction.TimeCodeTransactionProjectId != null)
                            {
                                payrollTransactionTimeCodeTransactionProject = (from t in projects
                                                                                where t.ProjectId == (int)payrollTransaction.TimeCodeTransactionProjectId
                                                                                select t).FirstOrDefault();
                            }

                            XElement payrollTransactionElement = new XElement("TimePayrollTransaction",
                            new XAttribute("id", timePayrollTransactionXmlId),
                            new XElement("EmployeeNr", payrollTransaction.EmployeeNr),
                            new XElement("EmployeeName", payrollTransaction.EmployeeName),
                            new XElement("Date", payrollTransaction.ProjectInvoiceDayDate.ToShortDateString()),
                            new XElement("Note", payrollTransaction.ProjectInvoiceDayNote),
                            new XElement("ExternalNote", payrollTransaction.Comment),
                            new XElement("PayrollTransactionQuantity", payrollTransaction.Quantity),
                            new XElement("PayrollTransactionCreated", payrollTransaction.Created.HasValue ? payrollTransaction.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                            new XElement("PayrollTransactionCreatedBy", payrollTransaction.CreatedBy),
                            new XElement("PayrollTransactionExported", payrollTransaction.Exported.ToInt()),
                            new XElement("PayrollTransactionProductNumber", payrollTransaction.ProductNumber),
                            new XElement("PayrollTransactionProductName", payrollTransaction.ProductName),
                            new XElement("PayrollTransactionProductDescription", payrollTransaction.ProductDescription),
                            new XElement("WeekNumber", weeknr.ToString()),
                            new XElement("IsoDate", payrollTransaction.ProjectInvoiceDayDate.Date.ToString("yyyy-MM-dd")),
                            new XElement("PayrollTransactionTimeCodeCode", payrollTransaction.TimeCode),
                            new XElement("PayrollTransactionTimeCodeName", payrollTransaction.TimeCodeName),
                            new XElement("PayrollTransactionTimeCodeDescription", payrollTransaction.TimeCodeDescription),
                            new XElement("PayrollTransactionTimeCodeTransactionId", payrollTransaction.TimeCodeTransactionId),
                            new XElement("PayrollTransactionTimeCodeTransactionQuantity", payrollTransaction.TimeCodeTransactionQuantity),
                            new XElement("PayrollTransactionTimeCodeTransactionInvoiceQuantity", payrollTransaction.TimeCodeTransactionInvoiceQuantity),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectId", payrollTransactionTimeCodeTransactionProject != null ? payrollTransactionTimeCodeTransactionProject.ProjectId : 0),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectNumber", payrollTransactionTimeCodeTransactionProject != null ? payrollTransactionTimeCodeTransactionProject.Number : string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectName", payrollTransactionTimeCodeTransactionProject != null ? payrollTransactionTimeCodeTransactionProject.Name : string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionProjectNote", payrollTransactionTimeCodeTransactionProject != null ? payrollTransactionTimeCodeTransactionProject.Note : string.Empty),
                            new XElement("PayrollTransactionTimeCodeTransactionParentProjectNumber", payrollTransaction.ParentProjectNumber),
                            new XElement("PayrollTransactionTimeCodeTransactionParentProjectName", payrollTransaction.ParentProjectName),
                            new XElement("PayrollType", payrollTransaction.PayrollType),
                            new XElement("isPayed", payrollTransaction.Payed.ToInt()),
                            new XElement("InvoiceNr", payrollTransaction.InvoiceNr),
                            new XElement("OriginType", payrollTransaction.OriginType.HasValue ? payrollTransaction.OriginType : 0),
                            new XElement("PayrollTransactionInvoiceCustomerName", payrollTransaction.CustomerName),
                            new XElement("PayrollTransactionInvoiceCustomerNr", payrollTransaction.CustomerNr),
                            new XElement("TimeDeviationCauseName", string.Empty),
                            new XElement("PayrollTransactionUnitPrice", empCost),
                            new XElement("PayrollAttestStateName", string.Empty)
                            //    new XElement("PayrollAttestStateName", payrollTransaction.TimePayrollAttestStateName)
                            );

                            payrollTransactionElements.Add(payrollTransactionElement);
                            timePayrollTransactionXmlId++;
                        }

                        #endregion

                        #region TimeInvoiceTransaction

                        foreach (var invoiceTransaction in timeInvoiceTransactions.Where(x => projectDayIdsForProject.Contains(x.ProjectInvoiceDayId)).ToList())
                        {
                            Project invoiceTransactionTimeCodeTransactionProject = new Project();

                            if (invoiceTransaction.TimeCodeTransactionProjectId.HasValue)
                            {
                                invoiceTransactionTimeCodeTransactionProject = (from t in projects
                                                                                where t.ProjectId == (int)invoiceTransaction.TimeCodeTransactionProjectId
                                                                                select t).FirstOrDefault();
                            }

                            int weeknr = CalendarUtility.GetWeekNr(invoiceTransaction.ProjectInvoiceDayDate);

                            XElement invoiceTransactionElement = new XElement("TimeInvoiceTransaction",
                                        new XAttribute("id", timeInvoiceTransactionXmlId),
                                        new XElement("EmployeeNr", invoiceTransaction.EmployeeNr),
                                        new XElement("EmployeeName", invoiceTransaction.EmployeeName),
                                        new XElement("Date", invoiceTransaction.ProjectInvoiceDayDate.Date.ToShortDateString()),
                                        new XElement("Note", invoiceTransaction.ProjectInvoiceDayNote),
                                        new XElement("ExternalNote", invoiceTransaction.Comment),
                                        new XElement("InvoiceTransactionQuantity", invoiceTransaction.Quantity),
                                        new XElement("InvoiceTransactionInvoiceQuantity", invoiceTransaction.InvoiceQuantity),
                                        new XElement("InvoiceTransactionCreated", invoiceTransaction.Created.HasValue ? invoiceTransaction.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                        new XElement("InvoiceTransactionCreatedBy", invoiceTransaction.CreatedBy),
                                        new XElement("InvoiceTransactionExported", invoiceTransaction.Exported.ToInt()),
                                        new XElement("InvoiceTransactionProductNumber", invoiceTransaction.ProductNumber),
                                        new XElement("InvoiceTransactionProductName", invoiceTransaction.ProductName),
                                        new XElement("InvoiceTransactionProductDescription", invoiceTransaction.ProductDescription),
                                        new XElement("InvoiceTransactionTimeCodeCode", invoiceTransaction.TimeCode),
                                        new XElement("InvoiceTransactionTimeCodeName", invoiceTransaction.TimeCodeName),
                                        new XElement("InvoiceTransactionTimeCodeDescription", invoiceTransaction.TimeCodeDescription),
                                        new XElement("InvoiceTransactionTimeCodeTransactionId", invoiceTransaction.TimeCodeTransactionId),
                                        new XElement("InvoiceTransactionTimeCodeTransactionQuantity", invoiceTransaction.TimeCodeTransactionQuantity),
                                        new XElement("InvoiceTransactionTimeCodeTransactionInvoiceQuantity", invoiceTransaction.TimeCodeTransactionInvoiceQuantity),
                                        new XElement("InvoiceTransactionTimeCodeTransactionProjectId", invoiceTransactionTimeCodeTransactionProject != null ? invoiceTransactionTimeCodeTransactionProject.ProjectId : 0),
                                        new XElement("InvoiceTransactionTimeCodeTransactionProjectNumber", invoiceTransactionTimeCodeTransactionProject != null ? invoiceTransactionTimeCodeTransactionProject.Number : string.Empty),
                                        new XElement("InvoiceTransactionTimeCodeTransactionProjectName", invoiceTransactionTimeCodeTransactionProject != null ? invoiceTransactionTimeCodeTransactionProject.Name : string.Empty),
                                        new XElement("InvoiceTransactionTimeCodeTransactionProjectNote", invoiceTransactionTimeCodeTransactionProject != null ? invoiceTransactionTimeCodeTransactionProject.Note : string.Empty),
                                        new XElement("InvoiceTransactionTimeCodeTransactionParentProjectNumber", invoiceTransaction.ParentProjectNumber),
                                        new XElement("InvoiceTransactionTimeCodeTransactionParentProjectName", invoiceTransaction.ParentProjectName),
                                        new XElement("WeekNumber", weeknr.ToString()),
                                        new XElement("IsoDate", invoiceTransaction.ProjectInvoiceDayDate.Date.ToString("yyyy-MM-dd")),
                                        new XElement("InvoiceNr", invoiceTransaction.InvoiceNr),
                                        new XElement("OriginType", invoiceTransaction.OriginType.HasValue ? invoiceTransaction.OriginType : 0),
                                        new XElement("InvoiceTransactionInvoiceCustomerName", invoiceTransaction.CustomerName),
                                        new XElement("InvoiceTransactionInvoiceCustomerNr", invoiceTransaction.CustomerNr));

                            invoiceTransactionElements.Add(invoiceTransactionElement);
                            timeInvoiceTransactionXmlId++;
                        }

                        #endregion
                    }



                    #endregion

                    #region TimePayrollTransactions from TimeSheet

                    string parProjectNumber = string.Empty;
                    string parProjectName = string.Empty;

                    if (project.ParentProjectId != null)
                    {
                        if (project.ParentProjectReference.IsLoaded)
                        {
                            parProjectNumber = project.ParentProject.Number;
                            parProjectName = project.ParentProject.Name;
                        }
                        else
                        {
                            var parProject = projects.FirstOrDefault(p => p.ProjectId == project.ParentProjectId.Value);
                            if (parProject != null)
                            {
                                parProjectNumber = parProject.Number;
                                parProjectName = parProject.Name;
                            }
                            else
                            {
                                project.ParentProjectReference.Load();

                                parProjectNumber = project.ParentProject.Number;
                                parProjectName = project.ParentProject.Name;
                            }
                        }
                    }


                    foreach (TimeCodeTransaction timeCodeTransaction in timeSheetTransactions)
                    {
                        if (timeCodeTransaction.TimePayrollTransaction != null)
                        {
                            if (reportParam.HasDateInterval && (timeCodeTransaction.Start.Date < reportParam.DateFrom.Date || timeCodeTransaction.Stop.Date > reportParam.DateTo.Date))
                                continue;

                            #region Transactions

                            foreach (var transaction in timeCodeTransaction.TimePayrollTransaction.Where(t => t.State == (int)SoeEntityState.Active))
                            {
                                Employee employee = null;
                                if (employeeLocalCache.Any(x => x.EmployeeId == transaction.EmployeeId))
                                {
                                    employee = employeeLocalCache.FirstOrDefault(x => x.EmployeeId == transaction.EmployeeId);
                                }
                                else
                                {
                                    employee = EmployeeManager.GetEmployee(transaction.EmployeeId, es.ActorCompanyId, onlyActive: false, loadContactPerson: true);
                                    if (employee != null)
                                        employeeLocalCache.Add(employee);
                                }

                                if (employee != null && reportParam.SB_HasEmployeeNrInterval && (employee.EmployeeNr.CompareTo(reportParam.SB_EmployeeNrFrom) < 0 || employee.EmployeeNr.CompareTo(reportParam.SB_EmployeeNrTo) > 0))
                                    continue;

                                PayrollProduct payrollProduct = null;
                                if (payrollProductLocalCache.Any(x => x.ProductId == transaction.ProductId))
                                {
                                    payrollProduct = payrollProductLocalCache.FirstOrDefault(x => x.ProductId == transaction.ProductId);
                                }
                                else
                                {
                                    payrollProduct = ProductManager.GetPayrollProduct(transaction.ProductId);
                                    if (payrollProduct != null)
                                        payrollProductLocalCache.Add(payrollProduct);
                                }

                                int weeknr = CalendarUtility.GetWeekNr(timeCodeTransaction.Start);

                                TimeCode timeCode = new TimeCode();

                                Project payrollTransactionTimeCodeTransactionProject = new Project();

                                if (transaction.TimeCodeTransaction != null && transaction.TimeCodeTransaction.ProjectId != null)
                                {
                                    payrollTransactionTimeCodeTransactionProject = (from t in projects
                                                                                    where t.ProjectId == (int)transaction.TimeCodeTransaction.ProjectId
                                                                                    select t).FirstOrDefault();
                                }

                                XElement payrollTransactionElement = new XElement("TimePayrollTransaction",
                                            new XAttribute("id", timePayrollTransactionXmlId),
                                            new XElement("EmployeeNr", employee != null ? employee.EmployeeNr : ""),
                                            new XElement("EmployeeName", employee != null ? employee.Name : ""),
                                            new XElement("Date", timeCodeTransaction.Start.ToShortDateString()),
                                            new XElement("Note", transaction.Comment),
                                            new XElement("ExternalNote", String.Empty),
                                            new XElement("PayrollTransactionQuantity", transaction.Quantity),
                                            new XElement("PayrollTransactionCreated", transaction.Created.HasValue ? transaction.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                            new XElement("PayrollTransactionCreatedBy", transaction.CreatedBy),
                                            new XElement("PayrollTransactionExported", transaction.Exported.ToInt()),
                                            new XElement("PayrollTransactionProductNumber", payrollProduct != null ? payrollProduct.Number : ""),
                                            new XElement("PayrollTransactionProductName", payrollProduct != null ? payrollProduct.Name : ""),
                                            new XElement("PayrollTransactionProductDescription", payrollProduct != null ? payrollProduct.Description : ""),
                                            new XElement("WeekNumber", weeknr.ToString()),
                                            new XElement("IsoDate", timeCodeTransaction.Start.ToString("yyyy-MM-dd")),
                                            new XElement("PayrollTransactionTimeCodeCode", timeCode != null ? timeCode.Code : ""),
                                            new XElement("PayrollTransactionTimeCodeName", timeCode != null ? timeCode.Name : ""),
                                            new XElement("PayrollTransactionTimeCodeDescription", timeCode != null ? timeCode.Description : ""),
                                            new XElement("PayrollTransactionTimeCodeTransactionId", transaction.TimeCodeTransactionId != null ? transaction.TimeCodeTransactionId : 0),
                                            new XElement("PayrollTransactionTimeCodeTransactionQuantity", timeCodeTransaction != null ? timeCodeTransaction.Quantity : 0),
                                            new XElement("PayrollTransactionTimeCodeTransactionInvoiceQuantity", timeCodeTransaction != null ? timeCodeTransaction.InvoiceQuantity : 0),
                                            new XElement("PayrollTransactionTimeCodeTransactionProjectId", payrollTransactionTimeCodeTransactionProject != null ? payrollTransactionTimeCodeTransactionProject.ProjectId : 0),
                                            new XElement("PayrollTransactionTimeCodeTransactionProjectNumber", payrollTransactionTimeCodeTransactionProject != null ? payrollTransactionTimeCodeTransactionProject.Number : string.Empty),
                                            new XElement("PayrollTransactionTimeCodeTransactionProjectName", payrollTransactionTimeCodeTransactionProject != null ? payrollTransactionTimeCodeTransactionProject.Name : string.Empty),
                                            new XElement("PayrollTransactionTimeCodeTransactionProjectNote", payrollTransactionTimeCodeTransactionProject != null ? payrollTransactionTimeCodeTransactionProject.Note : string.Empty),
                                            new XElement("PayrollTransactionTimeCodeTransactionParentProjectNumber", parProjectNumber),
                                            new XElement("PayrollTransactionTimeCodeTransactionParentProjectName", parProjectName),
                                            new XElement("PayrollType", payrollProduct != null ? payrollProduct.PayrollType : 0),
                                            new XElement("isPayed", payrollProduct != null ? payrollProduct.Payed.ToInt() : 0),
                                            new XElement("InvoiceNr", ""),
                                            new XElement("OriginType", 0),
                                            new XElement("PayrollTransactionInvoiceCustomerName", ""),
                                            new XElement("PayrollTransactionInvoiceCustomerNr", ""),
                                            new XElement("TimeDeviationCauseName", string.Empty),
                                            new XElement("PayrollTransactionUnitPrice", 0),
                                            new XElement("PayrollAttestStateName", string.Empty)
                                            );

                                payrollTransactionElements.Add(payrollTransactionElement);
                                timePayrollTransactionXmlId++;
                            }

                            #endregion
                        }
                    }

                    #endregion

                    #region Project

                    string projectCustomerNr = string.Empty;
                    string projectCustomerName = string.Empty;

                    if (project.Customer != null)
                    {
                        projectCustomerNr = project.Customer.CustomerNr;
                        projectCustomerName = project.Customer.Name;
                    }

                    XElement projectElement = new XElement("Project",
                                   new XAttribute("id", projectXmlId),
                                   new XElement("ProjectNumber", project.Number),
                                   new XElement("ProjectName", project.Name),
                                   new XElement("ProjectDescription", project.Description),
                                   new XElement("ProjectCreated", project.Created.HasValue ? project.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                   new XElement("ProjectCreatedBy", project.CreatedBy),
                                   new XElement("ProjectState", project.State),
                                   new XElement("ProjectCustomerNr", projectCustomerNr),
                                   new XElement("ProjectCustomerName", projectCustomerName));

                    projectXmlId++;

                    #endregion

                    #region Add Invoices

                    foreach (XElement invoice in invoiceElements)
                    {
                        projectElement.Add(invoice);
                    }

                    invoiceElements.Clear();

                    #endregion

                    #region Add TimePayrollTransaction

                    foreach (XElement transaction in payrollTransactionElements)
                    {
                        projectElement.Add(transaction);
                    }

                    #endregion

                    #region Add TimeInvoiceTransaction

                    foreach (XElement transaction in invoiceTransactionElements)
                    {
                        projectElement.Add(transaction);
                    }

                    #endregion

                    #region Create Merged Transactions

                    List<XElement> mergedTransactions = new List<XElement>();
                    int mergedTransactionXmlId = 1;

                    try
                    {
                        foreach (XElement transaction in payrollTransactionElements)
                        {

                            XElement mergedTransaction = new XElement("MergedTransaction",
                                    new XAttribute("id", mergedTransactionXmlId),
                                    new XElement("EmployeeNr", transaction.Element("EmployeeNr").Value),
                                    new XElement("EmployeeName", transaction.Element("EmployeeName").Value),
                                    new XElement("Date", transaction.Element("Date").Value),
                                    new XElement("Note", transaction.Element("Note").Value),
                                    new XElement("ExternalNote", transaction.Element("ExternalNote").Value),
                                    new XElement("WeekNumber", transaction.Element("WeekNumber").Value),
                                    new XElement("IsoDate", transaction.Element("IsoDate").Value),
                                    new XElement("PayrollTransactionQuantity", transaction.Element("PayrollTransactionQuantity").Value),
                                    new XElement("PayrollTransactionCreated", transaction.Element("PayrollTransactionCreated").Value),
                                    new XElement("PayrollTransactionCreatedBy", transaction.Element("PayrollTransactionCreatedBy").Value),
                                    new XElement("PayrollTransactionExported", transaction.Element("PayrollTransactionExported").Value),
                                    new XElement("PayrollTransactionProductNumber", transaction.Element("PayrollTransactionProductNumber").Value),
                                    new XElement("PayrollTransactionProductName", transaction.Element("PayrollTransactionProductName").Value),
                                    new XElement("PayrollTransactionProductDescription", transaction.Element("PayrollTransactionProductDescription").Value),
                                    new XElement("InvoiceTransactionQuantity", 0),
                                    new XElement("InvoiceTransactionInvoiceQuantity", 0),
                                    new XElement("InvoiceTransactionCreated", transaction.Element("PayrollTransactionCreated").Value),
                                    new XElement("InvoiceTransactionCreatedBy", string.Empty),
                                    new XElement("InvoiceTransactionExported", 0),
                                    new XElement("InvoiceTransactionProductNumber", string.Empty),
                                    new XElement("InvoiceTransactionProductName", string.Empty),
                                    new XElement("InvoiceTransactionProductDescription", string.Empty),
                                    new XElement("TimeCodeCode", transaction.Element("PayrollTransactionTimeCodeCode").Value),
                                    new XElement("TimeCodeName", transaction.Element("PayrollTransactionTimeCodeName").Value),
                                    new XElement("TimeCodeDescription", transaction.Element("PayrollTransactionTimeCodeDescription").Value),
                                    new XElement("TimeCodeTransactionId", transaction.Element("PayrollTransactionTimeCodeTransactionId").Value),
                                    new XElement("TimeCodeTransactionQuantity", transaction.Element("PayrollTransactionTimeCodeTransactionQuantity").Value),
                                    new XElement("TimeCodeTransactionInvoiceQuantity", transaction.Element("PayrollTransactionTimeCodeTransactionInvoiceQuantity").Value),
                                    new XElement("TimeCodeTransactionProjectId", transaction.Element("PayrollTransactionTimeCodeTransactionProjectId").Value),
                                    new XElement("TimeCodeTransactionProjectNumber", transaction.Element("PayrollTransactionTimeCodeTransactionProjectNumber").Value),
                                    new XElement("TimeCodeTransactionProjectName", transaction.Element("PayrollTransactionTimeCodeTransactionProjectName").Value),
                                    new XElement("TimeCodeTransactionProjectNote", transaction.Element("PayrollTransactionTimeCodeTransactionProjectNote").Value),
                                    new XElement("TimeCodeTransactionParentProjectNumber", transaction.Element("PayrollTransactionTimeCodeTransactionParentProjectNumber").Value),
                                    new XElement("TimeCodeTransactionParentProjectName", transaction.Element("PayrollTransactionTimeCodeTransactionParentProjectName").Value),
                                    new XElement("PayrollType", transaction.Element("PayrollType").Value),
                                    new XElement("isPayed", transaction.Element("isPayed").Value),
                                    new XElement("InvoiceNr", transaction.Element("InvoiceNr").Value),
                                    new XElement("OriginType", transaction.Element("OriginType").Value),
                                    new XElement("PayrollTransactionInvoiceCustomerName", transaction.Element("PayrollTransactionInvoiceCustomerName").Value),
                                    new XElement("PayrollTransactionInvoiceCustomerNr", transaction.Element("PayrollTransactionInvoiceCustomerNr").Value),
                                    new XElement("TimeDeviationCauseName", transaction.Element("TimeDeviationCauseName").Value),
                                    new XElement("PayrollTransactionUnitPrice", 0),
                                    new XElement("PayrollAttestStateName", string.Empty)
                                    );

                            mergedTransactions.Add(mergedTransaction);

                            mergedTransactionXmlId++;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, log);
                    }

                    payrollTransactionElements.Clear();

                    try
                    {
                        foreach (XElement transaction in invoiceTransactionElements)
                        {
                            XElement mergedTransaction = new XElement("MergedTransaction",
                                    new XAttribute("id", mergedTransactionXmlId),
                                    new XElement("EmployeeNr", transaction.Element("EmployeeNr").Value),
                                    new XElement("EmployeeName", transaction.Element("EmployeeName").Value),
                                    new XElement("Date", transaction.Element("Date").Value),
                                    new XElement("Note", transaction.Element("Note").Value),
                                    new XElement("ExternalNote", transaction.Element("ExternalNote").Value),
                                    new XElement("WeekNumber", transaction.Element("WeekNumber").Value),
                                    new XElement("IsoDate", transaction.Element("IsoDate").Value),
                                    new XElement("PayrollTransactionQuantity", 0),
                                    new XElement("PayrollTransactionCreated", 0),
                                    new XElement("PayrollTransactionCreatedBy", string.Empty),
                                    new XElement("PayrollTransactionExported", 0),
                                    new XElement("PayrollTransactionProductNumber", string.Empty),
                                    new XElement("PayrollTransactionProductName", string.Empty),
                                    new XElement("PayrollTransactionProductDescription", string.Empty),
                                    new XElement("InvoiceTransactionQuantity", transaction.Element("InvoiceTransactionQuantity").Value),
                                    new XElement("InvoiceTransactionInvoiceQuantity", transaction.Element("InvoiceTransactionInvoiceQuantity").Value),
                                    new XElement("InvoiceTransactionCreated", transaction.Element("InvoiceTransactionCreated").Value),
                                    new XElement("InvoiceTransactionCreatedBy", transaction.Element("InvoiceTransactionCreatedBy").Value),
                                    new XElement("InvoiceTransactionExported", transaction.Element("InvoiceTransactionExported").Value),
                                    new XElement("InvoiceTransactionProductNumber", transaction.Element("InvoiceTransactionProductNumber").Value),
                                    new XElement("InvoiceTransactionProductName", transaction.Element("InvoiceTransactionProductName").Value),
                                    new XElement("InvoiceTransactionProductDescription", transaction.Element("InvoiceTransactionProductDescription").Value),
                                    new XElement("TimeCodeCode", transaction.Element("InvoiceTransactionTimeCodeCode").Value),
                                    new XElement("TimeCodeName", transaction.Element("InvoiceTransactionTimeCodeName").Value),
                                    new XElement("TimeCodeDescription", transaction.Element("InvoiceTransactionTimeCodeDescription").Value),
                                    new XElement("TimeCodeTransactionId", transaction.Element("InvoiceTransactionTimeCodeTransactionId").Value),
                                    new XElement("TimeCodeTransactionQuantity", transaction.Element("InvoiceTransactionTimeCodeTransactionQuantity").Value),
                                    new XElement("TimeCodeTransactionInvoiceQuantity", transaction.Element("InvoiceTransactionTimeCodeTransactionInvoiceQuantity").Value),
                                    new XElement("TimeCodeTransactionProjectId", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectId").Value),
                                    new XElement("TimeCodeTransactionProjectNumber", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectNumber").Value),
                                    new XElement("TimeCodeTransactionProjectName", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectName").Value),
                                    new XElement("TimeCodeTransactionProjectNote", transaction.Element("InvoiceTransactionTimeCodeTransactionProjectNote").Value),
                                    new XElement("TimeCodeTransactionParentProjectNumber", transaction.Element("InvoiceTransactionTimeCodeTransactionParentProjectNumber").Value),
                                    new XElement("TimeCodeTransactionParentProjectName", transaction.Element("InvoiceTransactionTimeCodeTransactionParentProjectName").Value),
                                    new XElement("PayrollType", 0),
                                    new XElement("isPayed", 0),
                                    new XElement("InvoiceNr", transaction.Element("InvoiceNr").Value),
                                    new XElement("OriginType", transaction.Element("OriginType").Value),
                                    new XElement("PayrollTransactionInvoiceCustomerName", transaction.Element("InvoiceTransactionInvoiceCustomerName").Value),
                                    new XElement("PayrollTransactionInvoiceCustomerNr", transaction.Element("InvoiceTransactionInvoiceCustomerNr").Value),
                                    new XElement("TimeDeviationCauseName", string.Empty),
                                    new XElement("PayrollTransactionUnitPrice", 0),
                                    new XElement("PayrollAttestStateName", string.Empty)
                                    );

                            mergedTransactions.Add(mergedTransaction);

                            mergedTransactionXmlId++;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, log);
                    }

                    invoiceTransactionElements.Clear();

                    #endregion

                    projectElement.Add(mergedTransactions);

                    mergedTransactions.Clear();

                    projectElements.Add(projectElement);
                }

                #endregion

                return projectElements;
            }
            catch (Exception)
            {
                if (es != null)
                {
                    var parameters = String.Empty;
                    PropertyInfo[] properties = es.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        parameters += prop.Name + ": " + prop.GetValue(es) + ";";
                    }
                    base.LogError("CreateProjectStatisticsDataContentFromProjectInvoiceDays -> " + parameters);
                }
                throw;
            }
        }

        #endregion

        #region Timebook

        public XDocument CreateTimeProjectReportData(CreateReportResult reportResult)
        {
            #region Prereq

            int nrOfTimeProjectRows = 0;

            #endregion

            #region Init document
            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            List<TimeCode> timeCodes = TimeCodeManager.GetTimeCodes(entitiesReadOnly, reportResult.ActorCompanyId);

            
            bool showStartStopInTimeReport = SettingManager.GetBoolSetting(entitiesReadOnly, SettingMainType.Company, (int)CompanySettingType.BillingShowStartStopInTimeReport, this.UserId, reportResult.ActorCompanyId, 0, false);
            var extendedTimeRegistration = SettingManager.GetBoolSetting(entitiesReadOnly, SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, reportResult.ActorCompanyId, 0);
            
            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "TimeProjectReport");

            XElement timeProjectReportElement = new XElement("TimeProjectReport");

            #endregion

            #region ReportHeader

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            timeProjectReportElement.Add(reportHeaderLabelsElement);

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult, reportParams); //TimeProjectReport
            reportHeaderElement.Add(new XElement("ShowStartStop", showStartStopInTimeReport));
            reportHeaderElement.Add(new XElement("ExtendedTimeRegistration", extendedTimeRegistration));
            reportHeaderElement.Add(CreateDateIntervalElement(reportParams));
            timeProjectReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeProjectReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            timeProjectReportElement.Add(pageHeaderLabelsElement);

            #endregion

            //can only be 1 for the moment
            int invoiceId = reportParams.SB_InvoiceIds.Any() ? reportParams.SB_InvoiceIds.First() : 0;

            /*
            if (ActorCompanyId == 7)
            {
                using (CompEntities entities = new CompEntities())
                {
                    var stateUtility = new StateUtility(entities, InvoiceManager);
                    var generator = new TimeProjectDataReportGenerator(InvoiceManager, ProjectManager, AttestManager, TimeCodeManager, TimeDeviationCauseManager, TimeTransactionManager, SettingManager, stateUtility);
                    var timeSheetParams = new TimeProjectReportParams(ActorCompanyId, UserId, RoleId, reportParams.SB_IncludeOnlyInvoiced, reportParams.DateFrom, reportParams.DateTo);

                    timeProjectReportElement = generator.CreateTimeProjectElement(entities, timeSheetParams, timeCodes, invoiceId);
                }
            }
            else
            {
                */
                
                    #region Content

                    CreateTimeProjectElement(entitiesReadOnly, reportParams, timeProjectReportElement, timeCodes, invoiceId, reportResult.ActorCompanyId, out nrOfTimeProjectRows);

                    #endregion
            //}

            #region Close document

            rootElement.Add(timeProjectReportElement);
            document.Add(rootElement);
            return GetValidatedDocument(document, SoeReportTemplateType.TimeProjectReport);

            #endregion
        }

        /// <summary>
        /// Please use TimeProjectDataReportGenerator.cs instead!
        /// </summary>
        protected XElement CreateTimeProjectElement(CompEntities entities, BillingReportParamsDTO reportParams, XElement timeProjectReportElement, List<TimeCode> timeCodes, int invoiceId, int actorCompanyId, out int nrOfTimeProjectRows)
        {
            #region Content

            XElement projectElement = null;
            int projectXmlId = 1;
            nrOfTimeProjectRows = 0;
            bool showStartStopInTimeReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowStartStopInTimeReport, this.UserId, this.ActorCompanyId, 0, false);


            if (reportParams.SB_IncludeOnlyInvoiced)
            {

                Invoice invoice = InvoiceManager.GetInvoice(entities, invoiceId);

                string invoiceNr = invoice.InvoiceNr;

                List<Project> projects = new List<Project>();
                List<CustomerInvoiceRow> invoiceRows = InvoiceManager.GetCustomerInvoiceRows(entities, invoiceId, false);

                int attestStateTransferredOrderToInvoiceId = 0;
                List<AttestTransition> attestTransitions = null;
                if (invoice.Origin.Type == (int)SoeOriginType.Order)
                {
                    attestTransitions = AttestManager.GetAttestTransitions(entities, new List<TermGroup_AttestEntity> { TermGroup_AttestEntity.Order }, SoeModule.Billing, false, actorCompanyId);
                    attestStateTransferredOrderToInvoiceId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, actorCompanyId, 0);
                }

                foreach (CustomerInvoiceRow row in invoiceRows.Where(r => r.Type == (int)SoeInvoiceRowType.ProductRow && r.IsTimeProjectRow))
                {
                    if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                    {
                        List<CustomerInvoiceRow> parentRows = InvoiceManager.GetParentCustomerInvoiceRows(entities, row.CustomerInvoiceRowId, actorCompanyId);

                        if (parentRows.Count > 0)
                        {
                            foreach (CustomerInvoiceRow parentRow in parentRows)
                            {
                                List<TimeInvoiceTransaction> trans = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, parentRow.CustomerInvoiceRowId);

                                foreach (TimeInvoiceTransaction invTransaction in trans)
                                {
                                    bool addProject = false;
                                    bool addEmployee = false;

                                    if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                                        invTransaction.TimeCodeTransactionReference.Load();

                                    if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                                        continue;

                                    Project p = projects.FirstOrDefault(pr => pr.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);

                                    if (p == null)
                                    {
                                        addProject = true;
                                        p = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
                                    }

                                    if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                                        invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

                                    if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                                        invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

                                    if ((reportParams.HasDateInterval) && !invTransaction.TimeBlockDateReference.IsLoaded)
                                        invTransaction.TimeBlockDateReference.Load();

                                    Employee e = null;

                                    if (p.Employees != null)
                                        e = p.Employees.FirstOrDefault(em => em.EmployeeId == (int)invTransaction.EmployeeId);
                                    else
                                        p.Employees = new List<Employee>();

                                    if (e == null)
                                    {
                                        if (!invTransaction.EmployeeReference.IsLoaded)
                                            invTransaction.EmployeeReference.Load();

                                        addEmployee = true;
                                        e = invTransaction.Employee;
                                    }

                                    if (e.Transactions == null)
                                        e.Transactions = new List<TimeInvoiceTransaction>();

                                    e.Transactions.Add(invTransaction);

                                    if (addEmployee)
                                        p.Employees.Add(e);

                                    if (addProject)
                                        projects.Add(p);
                                }
                            }
                        }
                        else
                        {
                            List<TimeInvoiceTransaction> trans = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, row.CustomerInvoiceRowId);

                            foreach (TimeInvoiceTransaction invTransaction in trans)
                            {
                                bool addProject = false;
                                bool addEmployee = false;

                                if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                                    invTransaction.TimeCodeTransactionReference.Load();

                                if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                                    continue;

                                Project p = projects.FirstOrDefault(pr => pr.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);

                                if (p == null)
                                {
                                    addProject = true;
                                    p = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
                                }

                                if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                                    invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

                                if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                                    invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

                                if ((reportParams.HasDateInterval) && !invTransaction.TimeBlockDateReference.IsLoaded)
                                    invTransaction.TimeBlockDateReference.Load();

                                Employee e = null;

                                if (p.Employees != null)
                                    e = p.Employees.FirstOrDefault(em => em.EmployeeId == (int)invTransaction.EmployeeId);
                                else
                                    p.Employees = new List<Employee>();

                                if (e == null)
                                {
                                    if (!invTransaction.EmployeeReference.IsLoaded)
                                        invTransaction.EmployeeReference.Load();

                                    addEmployee = true;
                                    e = invTransaction.Employee;
                                }

                                if (e.Transactions == null)
                                    e.Transactions = new List<TimeInvoiceTransaction>();

                                e.Transactions.Add(invTransaction);

                                if (addEmployee)
                                    p.Employees.Add(e);

                                if (addProject)
                                    projects.Add(p);
                            }
                        }
                    }
                    else if (invoice.Origin.Type == (int)SoeOriginType.Order)
                    {
                        var rowHasStateToInvoice = attestTransitions.IsNullOrEmpty() || attestTransitions.Any(x => x.AttestStateFromId == row.AttestStateId && x.AttestStateToId == attestStateTransferredOrderToInvoiceId);
                        if (!rowHasStateToInvoice && reportParams.SB_IncludeOnlyInvoiced)
                            continue;

                        List<TimeInvoiceTransaction> trans = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, row.CustomerInvoiceRowId);

                        foreach (TimeInvoiceTransaction invTransaction in trans)
                        {
                            bool addProject = false;
                            bool addEmployee = false;

                            if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                                invTransaction.TimeCodeTransactionReference.Load();

                            if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                                continue;

                            Project p = projects.FirstOrDefault(pr => pr.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);

                            if (p == null)
                            {
                                addProject = true;
                                p = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
                            }

                            if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                                invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

                            if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                                invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

                            if (reportParams.HasDateInterval && !invTransaction.TimeBlockDateReference.IsLoaded)
                                invTransaction.TimeBlockDateReference.Load();

                            Employee e = null;

                            if (p.Employees != null)
                                e = p.Employees.FirstOrDefault(em => em.EmployeeId == (int)invTransaction.EmployeeId);
                            else
                                p.Employees = new List<Employee>();

                            if (e == null)
                            {
                                if (!invTransaction.EmployeeReference.IsLoaded)
                                    invTransaction.EmployeeReference.Load();

                                addEmployee = true;
                                e = invTransaction.Employee;
                            }

                            if (e.Transactions == null)
                                e.Transactions = new List<TimeInvoiceTransaction>();

                            e.Transactions.Add(invTransaction);

                            if (addEmployee)
                                p.Employees.Add(e);

                            if (addProject)
                                projects.Add(p);
                        }
                    }
                }

                foreach (Project proj in projects)
                {
                    int employeeXmlId = 1;
                    int projectInvoiceDayXmlId = 1;
                    List<XElement> employeeElements = new List<XElement>();

                    List<Employee> employees = new List<Employee>();
                    employees.AddRange(proj.Employees);

                    foreach (Employee emp in employees)
                    {
                        if (!emp.ContactPersonReference.IsLoaded)
                            emp.ContactPersonReference.Load();

                        XElement employeeElement = new XElement("Employee",
                                    new XAttribute("id", employeeXmlId),
                                    new XElement("EmployeeNr", emp.EmployeeNr),
                                    new XElement("EmployeeName", emp.Name));

                        nrOfTimeProjectRows = +emp.Transactions.Count;

                        List<TimeInvoiceTransaction> transactions = new List<TimeInvoiceTransaction>();
                        transactions.AddRange(emp.Transactions.Where(t => t.EmployeeId == emp.EmployeeId && t.TimeCodeTransaction?.ProjectId == proj.ProjectId));

                        if (reportParams.HasDateInterval)
                        {
                            if (reportParams.DateFrom != CalendarUtility.DATETIME_DEFAULT)
                            {
                                transactions = transactions.Where(t => t.TimeBlockDateId != null && t.TimeBlockDateId != 0 && t.TimeBlockDate.Date >= reportParams.DateFrom).ToList();
                            }

                            if (reportParams.DateTo != CalendarUtility.DATETIME_DEFAULT)
                            {
                                transactions = transactions.Where(t => t.TimeBlockDateId != null && t.TimeBlockDateId != 0 && t.TimeBlockDate.Date <= reportParams.DateTo).ToList();
                            }
                        }

                        foreach (var invoiceTransactions in transactions.GroupBy(x => x.TimeCodeTransactionId))
                        {
                            var trans = invoiceTransactions.First();

                            if (!trans.TimeCodeTransactionReference.IsLoaded)
                                trans.TimeCodeTransactionReference.Load();

                            var timeCodeTransaction = trans.TimeCodeTransaction;

                            if (timeCodeTransaction == null)
                                continue;

                            XElement dayElement = null;

                            if (timeCodeTransaction.ProjectTimeBlockId.HasValue && timeCodeTransaction.ProjectTimeBlockId > 0)
                            {
                                var projectTimeBlock = timeCodeTransaction.ProjectTimeBlock != null ? timeCodeTransaction.ProjectTimeBlock : ProjectManager.GetProjectTimeBlock(entities, (int)timeCodeTransaction.ProjectTimeBlockId);
                                if (projectTimeBlock != null)
                                {
                                    dayElement = CreateProjectInvoiceDayElement(projectInvoiceDayXmlId, projectTimeBlock, timeCodes, this.ActorCompanyId, showStartStopInTimeReport);
                                }
                            }
                            else if (trans.TimeCodeTransaction.ProjectInvoiceDay != null)
                            {
                                dayElement = new XElement("ProjectInvoiceDay",
                                                new XAttribute("id", projectInvoiceDayXmlId),
                                                new XElement("TCCode", timeCodeTransaction.TimeCode != null ? timeCodeTransaction.TimeCode.Code : string.Empty),
                                                new XElement("TCName", timeCodeTransaction.TimeCode != null ? timeCodeTransaction.TimeCode.Name : string.Empty),
                                                new XElement("InvoiceTimeInMinutes", timeCodeTransaction.ProjectInvoiceDay.InvoiceTimeInMinutes),
                                                new XElement("Date", timeCodeTransaction.ProjectInvoiceDay.Date.ToShortDateString()),
                                                new XElement("Note", timeCodeTransaction.ProjectInvoiceDay.Note),
                                                new XElement("ExternalNote", timeCodeTransaction.Comment),
                                                new XElement("IsoDate", timeCodeTransaction.ProjectInvoiceDay.Date.ToString("yyyy-MM-dd")),
                                                new XElement("TDName", string.Empty),
                                                new XElement("TBStartTime", string.Empty),
                                                new XElement("TBStopTime", string.Empty));
                            }

                            if (dayElement != null)
                            {
                                employeeElement.Add(dayElement);
                                projectInvoiceDayXmlId++;
                            }
                        }

                        employeeElements.Add(employeeElement);
                        employeeXmlId++;

                    }

                    if (employeeElements.Count == 0)
                    {
                        //Add default element
                        XElement defaultEmployeeElement = new XElement("Employee",
                                new XAttribute("id", 1),
                                new XElement("EmployeeNr", 0),
                                new XElement("EmployeeName", ""));

                        XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                            new XAttribute("id", 1),
                            new XElement("InvoiceTimeInMinutes", 0),
                            new XElement("Date", "00:00"),
                            new XElement("Note", "00:00"),
                            new XElement("ExternalNote", string.Empty),
                            new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

                        defaultEmployeeElement.Add(defaultDayElement);
                        employeeElements.Add(defaultEmployeeElement);
                    }

                    projectElement = new XElement("Project",
                        new XAttribute("id", projectXmlId),
                        new XElement("ProjectNumber", proj.Number),
                        new XElement("ProjectName", proj.Name),
                        new XElement("ProjectDescription", proj.Description),
                        new XElement("ProjectInvoiceNr", invoiceNr),
                        new XElement("ProjectCreated", proj.Created.HasValue ? proj.Created.Value.ToShortDateString() : ""),
                        new XElement("ProjectCreatedBy", proj.CreatedBy),
                        new XElement("ProjectState", proj.State),
                        new XElement("ProjectWorkSiteId", proj.WorkSiteKey),
                        new XElement("ProjectWorkSiteNumber", proj.WorkSiteNumber));


                    foreach (XElement employeeElement in employeeElements)
                    {
                        projectElement.Add(employeeElement);
                    }

                    if (timeProjectReportElement == null)
                        timeProjectReportElement = new XElement(projectElement);
                    else
                        timeProjectReportElement.Add(projectElement);
                    projectXmlId++;
                }

                //Detach
                foreach (var proj in projects)
                {
                    foreach (var emp in proj.Employees)
                    {
                        foreach (var trans in emp.Transactions)
                            base.TryDetachEntity(entities, trans);

                        base.TryDetachEntity(entities, emp);
                    }

                    base.TryDetachEntity(entities, proj);
                }

            }
            else
            {
                bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
                DateTime? fromDate = reportParams.HasDateInterval && reportParams.DateFrom != CalendarUtility.DATETIME_DEFAULT ? reportParams.DateFrom : (DateTime?)null;
                DateTime? toDate = reportParams.HasDateInterval && reportParams.DateTo != CalendarUtility.DATETIME_DEFAULT ? reportParams.DateTo : (DateTime?)null;

                List<Project> projects = ProjectManager.GetProjectsForInvoice(entities, invoiceId);

                if (projects.Count > 0)
                {
                    var invoice = (from i in entities.Invoice
                               .Include("Origin")
                                   where i.InvoiceId == invoiceId &&
                                   i.State == (int)SoeEntityState.Active
                                   select i).OfType<CustomerInvoice>().FirstOrDefault();

                    string invoiceNr = invoice.InvoiceNr;

                    foreach (Project project in projects)
                    {
                        if (useProjectTimeBlock)
                        {
                            List<ProjectTimeBlock> projectTimeBlocks = new List<ProjectTimeBlock>();
                            if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                            {
                                List<int> connectedOrderDitinctIds = InvoiceManager.GetConnectedOrdersForCustomerInvoice(entities, invoice.InvoiceId);
                                var tempBlocks = ProjectManager.GetProjectTimeBlocksForProject(entities, project.ProjectId, null, fromDate, toDate);
                                foreach (var connectedOrderId in connectedOrderDitinctIds)
                                {
                                    if (projectTimeBlocks.Any(x => x.CustomerInvoiceId == connectedOrderId))//just to be sure
                                        continue;

                                    projectTimeBlocks.AddRange(tempBlocks.Where(x => x.CustomerInvoiceId == connectedOrderId).ToList());
                                }
                            }
                            else
                                projectTimeBlocks = ProjectManager.GetProjectTimeBlocksForProject(entities, project.ProjectId, invoiceId, fromDate, toDate);

                            nrOfTimeProjectRows += projectTimeBlocks.Count;

                            //Group the entire list by employeeId
                            List<IGrouping<int, ProjectTimeBlock>> projectTimeBlocksGroupedByEmployeeId = projectTimeBlocks.GroupBy(g => g.EmployeeId).ToList();

                            int employeeXmlId = 1;
                            var employeeElements = new List<XElement>();

                            foreach (IGrouping<int, ProjectTimeBlock> projectTimeBlockGroupedByEmployeeId in projectTimeBlocksGroupedByEmployeeId)
                            {
                                ProjectTimeBlock firstProjectTimeBlock = projectTimeBlockGroupedByEmployeeId.FirstOrDefault();
                                if (firstProjectTimeBlock == null)
                                    continue;

                                Employee employee = firstProjectTimeBlock.Employee;
                                if (employee == null)
                                    continue;

                                List<XElement> projectInvoiceDayElements = new List<XElement>();
                                int projectInvoiceDayXmlId = 1;

                                foreach (ProjectTimeBlock projectTimeBlock in projectTimeBlockGroupedByEmployeeId)
                                {
                                    var dayElement = CreateProjectInvoiceDayElement(projectInvoiceDayXmlId, projectTimeBlock, timeCodes, this.ActorCompanyId, showStartStopInTimeReport);

                                    projectInvoiceDayElements.Add(dayElement);

                                    projectInvoiceDayXmlId++;
                                }

                                #region Employee

                                XElement employeeElement = new XElement("Employee",
                                    new XAttribute("id", employeeXmlId),
                                    new XElement("EmployeeNr", employee.EmployeeNr),
                                    new XElement("EmployeeName", employee.Name));

                                foreach (XElement projectInvoiceDayElement in projectInvoiceDayElements)
                                {
                                    employeeElement.Add(projectInvoiceDayElement);
                                }

                                projectInvoiceDayElements.Clear();
                                employeeElements.Add(employeeElement);

                                #endregion

                                employeeXmlId++;
                            }

                            #region Default element Employee

                            if (employeeXmlId == 1)
                            {
                                XElement defaultEmployeeElement = new XElement("Employee",
                                    new XAttribute("id", 1),
                                    new XElement("EmployeeNr", 0),
                                    new XElement("EmployeeName", ""));

                                XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                                    new XAttribute("id", 1),
                                    new XElement("InvoiceTimeInMinutes", 0),
                                    new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("Note", "00:00"),
                                    new XElement("ExternalNote", string.Empty),
                                    new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

                                defaultEmployeeElement.Add(defaultDayElement);
                                employeeElements.Add(defaultEmployeeElement);
                            }

                            #endregion

                            #region Project

                            projectElement = new XElement("Project",
                                new XAttribute("id", projectXmlId),
                                new XElement("ProjectNumber", project.Number),
                                new XElement("ProjectName", project.Name),
                                new XElement("ProjectDescription", project.Description),
                                new XElement("ProjectInvoiceNr", invoiceNr),
                                new XElement("ProjectCreated", project.Created.HasValue ? project.Created.Value.ToShortDateString() : ""),
                                new XElement("ProjectCreatedBy", project.CreatedBy),
                                new XElement("ProjectState", project.State),
                                new XElement("ProjectWorkSiteId", project.WorkSiteKey),
                                new XElement("ProjectWorkSiteNumber", project.WorkSiteNumber));

                            foreach (XElement employeeElement in employeeElements)
                            {
                                projectElement.Add(employeeElement);
                            }

                            employeeElements.Clear();

                            if (timeProjectReportElement == null)
                                timeProjectReportElement = new XElement(projectElement);
                            else
                                timeProjectReportElement.Add(projectElement);

                            #endregion

                            projectXmlId++;

                        }
                        else
                        {
                            List<ProjectInvoiceWeek> projectInvoiceWeeks = new List<ProjectInvoiceWeek>();

                            if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                            {
                                List<int> connectedOrderDitinctIds = InvoiceManager.GetConnectedOrdersForCustomerInvoice(entities, invoice.InvoiceId);
                                var tempWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId);
                                foreach (var connectedOrderId in connectedOrderDitinctIds)
                                {
                                    if (projectInvoiceWeeks.Any(x => x.RecordId == connectedOrderId))//just to be sure
                                        continue;

                                    projectInvoiceWeeks.AddRange(tempWeeks.Where(x => x.RecordId == connectedOrderId).ToList());
                                }
                            }
                            else
                                //projectInvoiceWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId, invoiceId);
                                projectInvoiceWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId, invoiceId, fromDate, toDate);

                            //Group the entire list by employeeId
                            List<IGrouping<int, ProjectInvoiceWeek>> projectInvoiceWeeksGroupedByEmployeeId = projectInvoiceWeeks.GroupBy(g => g.EmployeeId).ToList();

                            int employeeXmlId = 1;
                            List<XElement> employeeElements = new List<XElement>();

                            //Each employeeProjectInvoiceWeekItems contains all ProjectInvoiceWeeks for one employee
                            foreach (IGrouping<int, ProjectInvoiceWeek> projectInvoiceWeekGroupedByEmployeeId in projectInvoiceWeeksGroupedByEmployeeId)
                            {
                                ProjectInvoiceWeek firstProjectInvoiceWeek = projectInvoiceWeekGroupedByEmployeeId.FirstOrDefault();
                                if (firstProjectInvoiceWeek == null)
                                    continue;

                                Employee employee = firstProjectInvoiceWeek.Employee;
                                if (employee == null)
                                    continue;

                                List<XElement> projectInvoiceDayElements = new List<XElement>();
                                int projectInvoiceDayXmlId = 1;

                                //foreach ProjectInvoiceWeek for the employee
                                foreach (ProjectInvoiceWeek projectInvoiceWeek in projectInvoiceWeekGroupedByEmployeeId)
                                {
                                    //projectInvoiceDays contains all ProjectInvoiceDay items in a ProjectInvoiceWeek for the employee
                                    var projectInvoiceDays = ProjectManager.GetProjectInvoiceDays(entities, projectInvoiceWeek.ProjectInvoiceWeekId, fromDate, toDate, true);
                                    TimeCode timeCode = null;
                                    if (projectInvoiceWeek.TimeCodeId.HasValue)
                                        timeCode = timeCodes.FirstOrDefault(x => x.TimeCodeId == projectInvoiceWeek.TimeCodeId.Value);

                                    nrOfTimeProjectRows += projectInvoiceDays.Count(p => p.InvoiceTimeInMinutes > 0);

                                    foreach (ProjectInvoiceDay projectInvoiceDay in projectInvoiceDays)
                                    {
                                        #region ProjectInvoiceDay

                                        var timeCodeTransaction = projectInvoiceDay.TimeCodeTransaction.FirstOrDefault(t => t.State == (int)SoeEntityState.Active && t.TimeInvoiceTransaction.Any(i => i.State == (int)SoeEntityState.Active));
                                        var invoiceTimeInMinutes = timeCodeTransaction != null && timeCodeTransaction.TimeInvoiceTransaction.Any() ? projectInvoiceDay.InvoiceTimeInMinutes : 0;

                                        XElement dayElement = new XElement("ProjectInvoiceDay",
                                            new XAttribute("id", projectInvoiceDayXmlId),
                                            new XElement("TCCode", timeCode != null ? timeCode.Code : string.Empty),
                                            new XElement("TCName", timeCode != null ? timeCode.Name : string.Empty),
                                            new XElement("InvoiceTimeInMinutes", invoiceTimeInMinutes),
                                            new XElement("Date", projectInvoiceDay.Date.ToShortDateString()),
                                            new XElement("Note", projectInvoiceDay.Note),
                                            new XElement("ExternalNote", string.Empty),
                                            new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));
                                        projectInvoiceDayElements.Add(dayElement);

                                        #endregion

                                        projectInvoiceDayXmlId++;
                                    }
                                }

                                #region Employee

                                XElement employeeElement = new XElement("Employee",
                                    new XAttribute("id", employeeXmlId),
                                    new XElement("EmployeeNr", employee.EmployeeNr),
                                    new XElement("EmployeeName", employee.Name));

                                foreach (XElement projectInvoiceDayElement in projectInvoiceDayElements)
                                {
                                    employeeElement.Add(projectInvoiceDayElement);
                                }

                                projectInvoiceDayElements.Clear();
                                employeeElements.Add(employeeElement);

                                #endregion

                                employeeXmlId++;
                            }

                            #region Default element Employee

                            if (employeeXmlId == 1)
                            {
                                XElement defaultEmployeeElement = new XElement("Employee",
                                    new XAttribute("id", 1),
                                    new XElement("EmployeeNr", 0),
                                    new XElement("EmployeeName", ""));

                                XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                                    new XAttribute("id", 1),
                                    new XElement("InvoiceTimeInMinutes", 0),
                                    new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("Note", "00:00"),
                                    new XElement("ExternalNote", string.Empty),
                                    new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

                                defaultEmployeeElement.Add(defaultDayElement);
                                employeeElements.Add(defaultEmployeeElement);
                            }

                            #endregion

                            #region Project

                            projectElement = new XElement("Project",
                                new XAttribute("id", projectXmlId),
                                new XElement("ProjectNumber", project.Number),
                                new XElement("ProjectName", project.Name),
                                new XElement("ProjectDescription", project.Description),
                                new XElement("ProjectInvoiceNr", invoiceNr),
                                new XElement("ProjectCreated", project.Created.HasValue ? project.Created.Value.ToShortDateString() : ""),
                                new XElement("ProjectCreatedBy", project.CreatedBy),
                                new XElement("ProjectState", project.State),
                                new XElement("ProjectWorkSiteId", project.WorkSiteKey),
                                new XElement("ProjectWorkSiteNumber", project.WorkSiteNumber));

                            foreach (XElement employeeElement in employeeElements)
                            {
                                projectElement.Add(employeeElement);
                            }

                            employeeElements.Clear();

                            if (timeProjectReportElement == null)
                                timeProjectReportElement = new XElement(projectElement);
                            else
                                timeProjectReportElement.Add(projectElement);

                            #endregion

                            projectXmlId++;
                        }
                    }
                }
                else
                {
                    #region Default element Project

                    projectElement = new XElement("Project",
                        new XAttribute("id", 1),
                        new XElement("ProjectNumber", ""),
                        new XElement("ProjectName", ""),
                        new XElement("ProjectDescription", ""),
                        new XElement("ProjectInvoiceNr", ""),
                        new XElement("ProjectCreated", "00:00"),
                        new XElement("ProjectCreatedBy", ""),
                        new XElement("ProjectState", 0),
                        new XElement("ProjectWorkSiteId", ""),
                        new XElement("ProjectWorkSiteNumber", ""));

                    XElement defaultEmployeeElement = new XElement("Employee",
                        new XAttribute("id", 1),
                        new XElement("EmployeeNr", 0),
                        new XElement("EmployeeName", ""));

                    XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                        new XAttribute("id", 1),
                        new XElement("InvoiceTimeInMinutes", 0),
                        new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("Note", "00:00"));

                    defaultEmployeeElement.Add(defaultDayElement);
                    projectElement.Add(defaultEmployeeElement);
                    timeProjectReportElement.Add(projectElement);

                    #endregion
                }
            }

            #endregion

            return timeProjectReportElement;
        }

        protected XElement CreateTimeReportHeaderElement(CreateReportResult reportResult, BillingReportParamsDTO reportParams)
        {
            TimePeriod timePeriod = null;
            if (reportParams.ST_TimePeriodId.HasValue)
                timePeriod = TimePeriodManager.GetTimePeriod(reportParams.ST_TimePeriodId.Value, reportResult.ActorCompanyId);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(reportResult.ReportName),
                    this.CreateReportDescriptionElement(reportResult.ReportDescription),
                    this.CreateReportNrElement(reportResult.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(reportResult.LoginName),
                    new XElement("TimePeriod", timePeriod?.Name ?? ""),
                    new XElement("SortByLevel1", reportParams.SortByLevel1),
                    new XElement("SortByLevel2", reportParams.SortByLevel2),
                    new XElement("SortByLevel3", reportParams.SortByLevel3),
                    new XElement("SortByLevel4", reportParams.SortByLevel4),
                    new XElement("IsSortAscending", reportParams.IsSortAscending),
                    new XElement("GroupByLevel1", reportParams.GroupByLevel1),
                    new XElement("GroupByLevel2", reportParams.GroupByLevel2),
                    new XElement("GroupByLevel3", reportParams.GroupByLevel3),
                    new XElement("GroupByLevel4", reportParams.GroupByLevel4),
                    new XElement("Special", reportParams.Special));
        }
        #endregion

        #region Invoice
        public XDocument CreateBillingInvoiceData(CreateReportResult reportResult)
        {
            if (reportResult.ReportTemplateType != SoeReportTemplateType.BillingContract &&
                reportResult.ReportTemplateType != SoeReportTemplateType.BillingInvoice &&
                reportResult.ReportTemplateType != SoeReportTemplateType.BillingOrder &&
                reportResult.ReportTemplateType != SoeReportTemplateType.BillingOffer &&
                reportResult.ReportTemplateType != SoeReportTemplateType.BillingInvoiceInterest &&
                reportResult.ReportTemplateType != SoeReportTemplateType.BillingInvoiceReminder &&
                reportResult.ReportTemplateType != SoeReportTemplateType.OriginStatisticsReport &&
                reportResult.ReportTemplateType != SoeReportTemplateType.BillingOrderOverview)
                return null;

            #region Prereq

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            reportParams.SB_IncludeProjectReport2 = true;

            if (reportResult == null)
                return null;

            //Language
            reportLanguageId = reportParams.SB_ReportLanguageId;
            if (reportParams.SB_ReportLanguageId == 0)
            {
                switch (Company.SysCountryId)
                {
                    case (int)TermGroup_Country.SE:
                        reportLanguageId = (int)TermGroup_Languages.Swedish;
                        break;
                    case (int)TermGroup_Country.FI:
                        reportLanguageId = (int)TermGroup_Languages.Finnish;
                        break;
                    case (int)TermGroup_Country.GB:
                        reportLanguageId = (int)TermGroup_Languages.English;
                        break;
                    case (int)TermGroup_Country.NO:
                        reportLanguageId = (int)TermGroup_Languages.Norwegian;
                        break;
                    case (int)TermGroup_Country.DK:
                        reportLanguageId = (int)TermGroup_Languages.Danish;
                        break;
                }
            }

            if (reportLanguageId == 0)
                reportLanguageId = GetLangId();

            SetLanguage(LanguageManager.GetSysLanguageCode(reportLanguageId));

            bool isBillingContractTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingContract);
            bool isBillingOrderTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOrder) || reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOrderOverview);
            bool isBillingOfferTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOffer);
            bool isBillingInvoiceTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoice);
            bool isBillingInvoiceInterestTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoiceInterest);
            bool isBillingInvoiceReminderTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoiceReminder);
            bool isInvoiceTemplate = isBillingInvoiceTemplate || isBillingInvoiceInterestTemplate || isBillingInvoiceReminderTemplate;
            bool hidePriceAndRowSum = false;
            bool groupInvoiceByTaxdeduction = false;
            bool groupOfferByTaxdeduction = false;
            //permissions
            bool showSalesPricePermission = true;  //FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowSalesPrice, Permission.Readonly, base.RoleId, base.ActorCompanyId);

            //Get settings
            int baseSysCurrencyId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreBaseCurrency, 0, reportResult.ActorCompanyId, 0);
            int householdProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, reportResult.ActorCompanyId, 0);
            int household50ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeduction, 0, reportResult.ActorCompanyId, 0);
            int householdRutProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductRUTTaxDeduction, 0, reportResult.ActorCompanyId, 0);
            int householdGreen15ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen15TaxDeduction, 0, reportResult.ActorCompanyId, 0);
            int householdGreen20ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen20TaxDeduction, 0, reportResult.ActorCompanyId, 0);
            int householdGreen50ProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen50TaxDeduction, 0, reportResult.ActorCompanyId, 0);
            decimal interestPercent = SettingManager.GetDecimalSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInterestPercent, 0, reportResult.ActorCompanyId, 0);
            bool showOrdernrOnInvoiceReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowOrdernrOnInvoiceReport, 0, reportResult.ActorCompanyId, 0);
            bool doPrintTaxBillText = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingPrintTaxBillText, 0, reportResult.ActorCompanyId, 0);
            bool showCOLabel = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowCOLabelOnReport, 0, reportResult.ActorCompanyId, 0);
            bool showRemainingAmountInReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingIncludeRemainingAmountOnInvoice, 0, reportResult.ActorCompanyId, 0);
            bool orderIncludeTimeProjectinReport = true; //SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingOrderIncludeTimeProjectinReport, 0, es.ActorCompanyId, 0); - REMOVED ITEM 49453
            bool invoiceIncludeTimeProjectinReport = true; //SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingIncludeTimeProjectinReport, 0, es.ActorCompanyId, 0); -REMOVED ITEM 49453
            bool UseDeliveryAddressAsBillingAddress = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseDeliveryAddressAsInvoiceAddress, 0, reportResult.ActorCompanyId, 0);
            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
            //Disregard copies for interest and reminder, until separate setting exists for them
            if (isBillingInvoiceInterestTemplate || isBillingInvoiceReminderTemplate)
                reportParams.SB_DisableInvoiceCopies = true;

            int nbrOfCopies;
            if (reportParams.SB_DisableInvoiceCopies)
                nbrOfCopies = 0;
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingContract))
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfContractCopies, 0, reportResult.ActorCompanyId, 0);
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOffer))
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfOfferCopies, 0, reportResult.ActorCompanyId, 0);
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOrder))
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfOrderCopies, 0, reportResult.ActorCompanyId, 0);
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoice))
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfCopies, 0, reportResult.ActorCompanyId, 0);
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.OriginStatisticsReport))
                nbrOfCopies = 0;
            else
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfCopies, 0, reportResult.ActorCompanyId, 0);

            // Never send email copys
            if (nbrOfCopies > 0 && reportResult.EmailTemplateId != null && reportResult.EmailTemplateId != 0)
            {
                nbrOfCopies = 0;
            }

            //Get SysCurrencies
            List<SysCurrency> sysCurrencies = CountryCurrencyManager.GetSysCurrencies(true);

            var houseHoldDeductionTypeDict = ProductManager.GetSysHouseholdTypeDict(false);
            //AttestLevels
            string attestLevelInitial = string.Empty;
            string attestLevelToInvoice = string.Empty;
            string attestLevelMobileReady = string.Empty;
            string attestLevelOfferToOrder = string.Empty;

            if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingOrder || reportResult.ReportTemplateType == SoeReportTemplateType.OriginStatisticsReport || reportResult.ReportTemplateType == SoeReportTemplateType.BillingOrderOverview)
            {
                var initialAttestState = AttestManager.GetInitialAttestState(reportResult.ActorCompanyId, TermGroup_AttestEntity.Order);
                attestLevelInitial = initialAttestState != null ? initialAttestState.Name : string.Empty;

                int defaultStatusTransferredOrderToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, reportResult.ActorCompanyId, 0);
                int defaultStatusTransferredOrderReadyMobile = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusOrderReadyMobile, 0, reportResult.ActorCompanyId, 0);

                var orderToInvoiceAttestState = AttestManager.GetAttestState(defaultStatusTransferredOrderToInvoice);
                attestLevelToInvoice = orderToInvoiceAttestState != null ? orderToInvoiceAttestState.Name : string.Empty;

                var orderReadyMobileAttestState = AttestManager.GetAttestState(defaultStatusTransferredOrderReadyMobile);
                attestLevelMobileReady = orderReadyMobileAttestState != null ? orderReadyMobileAttestState.Name : string.Empty;
            }

            if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingOffer)
            {
                var initialAttestState = AttestManager.GetInitialAttestState(reportResult.ActorCompanyId, TermGroup_AttestEntity.Offer);
                attestLevelInitial = initialAttestState != null ? initialAttestState.Name : string.Empty;

                int defaultStatusTransferredOfferToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToInvoice, 0, reportResult.ActorCompanyId, 0);
                int defaultStatusTransferredOfferToOrder = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToOrder, 0, reportResult.ActorCompanyId, 0);

                var offerToInvoiceAttestState = AttestManager.GetAttestState(defaultStatusTransferredOfferToInvoice);
                attestLevelToInvoice = offerToInvoiceAttestState != null ? offerToInvoiceAttestState.Name : string.Empty;

                var offerToOrderAttestState = AttestManager.GetAttestState(defaultStatusTransferredOfferToOrder);
                attestLevelOfferToOrder = offerToOrderAttestState != null ? offerToOrderAttestState.Name : string.Empty;

                var reportSettings = ReportManager.GetReportSettings(entitiesReadOnly, reportResult.ReportId);
                hidePriceAndRowSum = reportSettings.FirstOrDefault(x => x.Type == TermGroup_ReportSettingType.HidePriceAndRowSum)?.BoolData ?? false;
                groupOfferByTaxdeduction = reportSettings.FirstOrDefault(x => x.Type == TermGroup_ReportSettingType.GroupedOfferByTaxDeduction)?.BoolData ?? false;

            }

            if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingInvoice)
            {
                var reportSettings = ReportManager.GetReportSettings(entitiesReadOnly, reportResult.ReportId);
                groupInvoiceByTaxdeduction = reportSettings.FirstOrDefault(x => x.Type == TermGroup_ReportSettingType.GroupedInvoiceByTaxDeduction)?.BoolData ?? false;

            }

            List<TermGroup_AttestEntity> entitys = new List<TermGroup_AttestEntity>()
            {
                TermGroup_AttestEntity.Order,
                TermGroup_AttestEntity.Offer,
            };
            List<AttestState> attestStates = AttestManager.GetAttestStates(reportResult.ActorCompanyId, entitys, SoeModule.Billing);

            List<TimeCode> timeCodes = TimeCodeManager.GetTimeCodes(reportResult.ActorCompanyId);
            List<AccountDimDTO> accountDimInternalsDTOs = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId).ToDTOs();


            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "BillingInvoice");

            //VoucherList
            XElement billingInvoiceElement = new XElement("BillingInvoice");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateBillingReportHeaderElement(reportResult, reportParams);
            reportHeaderElement.Add(CreateAccountIntervalElement(reportResult, reportParams, accountDimInternalsDTOs));
            reportHeaderElement.Add(new XElement("AttestLevelInitial", attestLevelInitial));
            reportHeaderElement.Add(new XElement("AttestLevelInvoiced", attestLevelToInvoice));
            reportHeaderElement.Add(new XElement("AttestLevelMobileReady", attestLevelMobileReady));
            reportHeaderElement.Add(new XElement("AttestLevelOfferToOrder", attestLevelOfferToOrder));

            #region Sort and Specials

            //Special handling - defaulting to order by invoicenr
            var sortByLevel1 = reportParams.SortByLevel1;
            if (sortByLevel1 == TermGroup_ReportGroupAndSortingTypes.Unknown)
            {
                switch (reportParams.SB_SortOrder)
                {
                    case 1:
                        sortByLevel1 = TermGroup_ReportGroupAndSortingTypes.InvoiceNr;
                        break;
                    case 2:
                        sortByLevel1 = TermGroup_ReportGroupAndSortingTypes.CustomerNr;
                        break;
                    case 3:
                        sortByLevel1 = TermGroup_ReportGroupAndSortingTypes.ProjectNr;
                        break;
                    default:
                        sortByLevel1 = TermGroup_ReportGroupAndSortingTypes.InvoiceNr;
                        break;
                }
            }

            reportHeaderElement.Add(new XElement("SortByLevel1", sortByLevel1));
            reportHeaderElement.Add(new XElement("SortByLevel2", reportParams.SortByLevel2));
            reportHeaderElement.Add(new XElement("SortByLevel3", reportParams.SortByLevel3));
            reportHeaderElement.Add(new XElement("SortByLevel4", reportParams.SortByLevel4));
            reportHeaderElement.Add(new XElement("IsSortAscending", reportParams.IsSortAscending));
            reportHeaderElement.Add(new XElement("GroupByLevel1", reportParams.GroupByLevel1));
            reportHeaderElement.Add(new XElement("GroupByLevel2", reportParams.GroupByLevel2));
            reportHeaderElement.Add(new XElement("GroupByLevel3", reportParams.GroupByLevel3));
            reportHeaderElement.Add(new XElement("GroupByLevel4", reportParams.GroupByLevel4));
            reportHeaderElement.Add(new XElement("Special", reportParams.Special));
            reportHeaderElement.Add(new XElement("HidePrice", !showSalesPricePermission));
            if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingOffer)
            {
                reportHeaderElement.Add(new XElement("HidePriceAndRowSum", hidePriceAndRowSum));
                reportHeaderElement.Add(new XElement("GroupInvoiceByTaxdeduction", groupInvoiceByTaxdeduction));
                
            }
            if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingInvoice)
            {
                reportHeaderElement.Add(new XElement("GroupInvoiceByTaxdeduction", groupInvoiceByTaxdeduction));

            }
            #endregion

            #endregion

            var invoiceHeadElements = new List<XElement>();

            using (var entities = new CompEntities())
            {
                #region Prereq

                //Get AccountDims
                AccountDim accountDimStds = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId);

                var accountStdsInInterval = new List<AccountStd>();
                var accountInternalsInInterval = new List<AccountInternal>();
                var customerNameDict = new Dictionary<int, string>();

                AccountManager.GetAccountsInInterval(entities, reportResult, reportParams, accountDimStds, false, ref accountStdsInInterval, ref accountInternalsInInterval);

                SoeOriginType originType = SoeOriginType.None;
                int invoiceHeadXmlId = 1;
                var customerInvoices = InvoiceManager.GetCustomerInvoicesFromSelection(entities, reportResult, reportParams, reportParams.SB_IncludeDrafts, ref originType, reportResult.ReportTemplateType != SoeReportTemplateType.OriginStatisticsReport, true, false);

                // Increase the command timeout
                entities.CommandTimeout = 180; // 3 minutes

                // Get hidden attest states
                List<int> hiddenAttestStateIds = new List<int>();
                if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingOffer)
                    hiddenAttestStateIds = AttestManager.GetHiddenAttestStateIds(entities, reportResult.ActorCompanyId, TermGroup_AttestEntity.Offer, SoeModule.Billing);
                else if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingOrder)
                    hiddenAttestStateIds = AttestManager.GetHiddenAttestStateIds(entities, reportResult.ActorCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing);

                var attestStateTransferredOrderToInvoiceId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, reportResult.ActorCompanyId, 0);

                List<VatCode> vatCodes = AccountManager.GetVatCodes(reportResult.ActorCompanyId);


                List<GenericType> contractGroupPeriodTerms = base.GetTermGroupContent(TermGroup.ContractGroupPeriod);

                #region Information for QRCode

                PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(Company.ActorCompanyId, true, false);
                string bg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BG);
                string pg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.PG);
                Contact contact = ContactManager.GetContactFromActor(entities, Company.ActorCompanyId);
                List<ContactAddressRow> contactAddressRows = contact != null ? ContactManager.GetContactAddressRows(entities, contact.ContactId) : null;
                string distributionPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode) + " " + contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress);

                #endregion

                #endregion

                #region Content

                //Fixed price setting (fixedprice type 1)
                int fixedPriceProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductFlatPrice, 0, reportResult.ActorCompanyId, 0);
                //Fixed price keep prices setting (fixedprice type 2)
                int fixedPriceKeepPricesProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductFlatPriceKeepPrices, 0, reportResult.ActorCompanyId, 0);
                //Guarantee setting
                int productGuaranteeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGuarantee, 0, reportResult.ActorCompanyId, 0);
                //defaultReminderProductId
                int defaultReminderProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductReminderFee, reportResult.UserId, reportResult.ActorCompanyId, 0);
                //defaultPriceListType
                int defaultPriceListType = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultPriceListType, reportResult.UserId, reportResult.ActorCompanyId, 0);

                int defaultInterestAccumulatedBeforeInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInterestAccumulatedBeforeInvoice, reportResult.UserId, reportResult.ActorCompanyId, 0);

                var productRowDescriptionToUpperCase = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingProductRowTextUppercase, reportResult.UserId, reportResult.ActorCompanyId, 0);

                foreach (CustomerInvoice customerInvoice in customerInvoices)
                {
                    #region CustomerInvoice

                    #region Prereq

                    //Customer
                    Customer customer = customerInvoice?.Actor?.Customer;
                    if (customer == null)
                        continue;

                    if (customerInvoices.Count == 1 && customer.SysLanguageId != null && reportParams.SB_ReportLanguageId == 0)
                    {
                        reportLanguageId = (int)customer.SysLanguageId;
                        SetLanguage(LanguageManager.GetSysLanguageCode(reportLanguageId));
                    }

                    ReportDataHistoryRepository repository = ReportDataHistoryRepository.CreateBillingInvoiceRepository(parameterObject, customerInvoice, reportResult.ActorCompanyId);

                    //Customer name
                    bool isOneTimeCustomer = false;
                    isOneTimeCustomer = customer.IsOneTimeCustomer;

                    string customerName = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerName);
                    if (String.IsNullOrEmpty(customerName))
                    {
                        if (isOneTimeCustomer)
                        {
                            customerName = customerInvoice.CustomerName != null ? customerInvoice.CustomerName : String.Empty;
                        }
                        else
                        {
                            customerName = customerInvoice.Actor.Customer.Name != null ? customerInvoice.Actor.Customer.Name : String.Empty;
                        }
                    }
                    //Report name
                    if (customerInvoice.SeqNr.HasValue && !customerNameDict.ContainsKey(customerInvoice.SeqNr.Value))
                        customerNameDict.Add(customerInvoice.SeqNr.Value, customerName.Replace('\n', ' '));

                    //Currenty
                    SysCurrency sysCurrency = sysCurrencies.FirstOrDefault(i => i.SysCurrencyId == customerInvoice.Currency.SysCurrencyId);

                    decimal reminderAmount = 0;
                    decimal interestAmount = 0;
                    if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingInvoiceReminder)
                    {
                        //Get PriceListType from Customer if exists, otherwise Company setting
                        int priceListTypeId = InvoiceManager.GetPriceListTypeIdBasedOnPriority(customerInvoice.Actor.Customer, defaultPriceListType);

                        //Get Product reminder
                        InvoiceProduct productReminder = ProductManager.GetInvoiceProduct(entities, defaultReminderProductId, true, false, false);
                        //if (productReminder == null)
                        //    return new ActionResult((int)ActionResultSave.CustomerInvoiceReminderProductNotFound, "InvoiceProduct");

                        //Claim product amount
                        reminderAmount = InvoiceManager.GetReminderProductPrice(entities, productReminder, priceListTypeId);

                        //Check that totalamount is larger than minimum amount in company setting
                        interestAmount = InvoiceManager.CalculateInterestAmount(entities, customerInvoice.InvoiceId, interestPercent, customerInvoice.TotalAmount, customerInvoice.DueDate, true);
                        if (interestAmount < defaultInterestAccumulatedBeforeInvoice)
                        {
                            interestAmount = 0;
                        }
                    }

                    // Timecodetransactions
                    var transactions = reportResult.ReportTemplateType == SoeReportTemplateType.BillingOrderOverview ? entities.GetTimeCodeTransactionsForOrderOverview(customerInvoice.InvoiceId, useProjectTimeBlock).ToList() : new List<GetTimeCodeTransactionsForOrderOverview_Result>();

                    //Account Internals
                    #region Account Internals Validation
                    bool accountsValid = false;

                    if (accountInternalsInInterval.Count != 0)
                    {
                        if ((customerInvoice.DefaultDim1AccountId == null || customerInvoice.DefaultDim1AccountId == 0) &&
                            (customerInvoice.DefaultDim2AccountId == null || customerInvoice.DefaultDim2AccountId == 0) &&
                            (customerInvoice.DefaultDim3AccountId == null || customerInvoice.DefaultDim3AccountId == 0) &&
                            (customerInvoice.DefaultDim4AccountId == null || customerInvoice.DefaultDim4AccountId == 0) &&
                            (customerInvoice.DefaultDim5AccountId == null || customerInvoice.DefaultDim5AccountId == 0) &&
                            (customerInvoice.DefaultDim6AccountId == null || customerInvoice.DefaultDim6AccountId == 0))
                        {
                            accountsValid = false;
                        }
                        else
                        {
                            var accountInternalIds = accountInternalsInInterval.Select(a => a.AccountId).ToList();
                            if (customerInvoice.DefaultDim1AccountId != null && customerInvoice.DefaultDim1AccountId != 0 && accountInternalIds.Contains((int)customerInvoice.DefaultDim1AccountId))
                                accountsValid = true;
                            if (!accountsValid && customerInvoice.DefaultDim2AccountId != null && customerInvoice.DefaultDim2AccountId != 0 && accountInternalIds.Contains((int)customerInvoice.DefaultDim2AccountId))
                                accountsValid = true;
                            if (!accountsValid && customerInvoice.DefaultDim3AccountId != null && customerInvoice.DefaultDim3AccountId != 0 && accountInternalIds.Contains((int)customerInvoice.DefaultDim3AccountId))
                                accountsValid = true;
                            if (!accountsValid && customerInvoice.DefaultDim4AccountId != null && customerInvoice.DefaultDim4AccountId != 0 && accountInternalIds.Contains((int)customerInvoice.DefaultDim4AccountId))
                                accountsValid = true;
                            if (!accountsValid && customerInvoice.DefaultDim5AccountId != null && customerInvoice.DefaultDim5AccountId != 0 && accountInternalIds.Contains((int)customerInvoice.DefaultDim5AccountId))
                                accountsValid = true;
                            if (!accountsValid && customerInvoice.DefaultDim6AccountId != null && customerInvoice.DefaultDim6AccountId != 0 && accountInternalIds.Contains((int)customerInvoice.DefaultDim6AccountId))
                                accountsValid = true;
                        }
                    }
                    else
                    {
                        accountsValid = true;
                    }

                    if (!accountsValid)
                        continue;

                    if (customerInvoice.ContractGroupId.HasValue && !customerInvoice.ContractGroupReference.IsLoaded)
                        customerInvoice.ContractGroupReference.Load();

                    #endregion

                    #endregion

                    #region Contact

                    Contact customerContact = customerInvoice.Actor?.Contact?.FirstOrDefault() ?? ContactManager.GetContactFromActor(customerInvoice.Actor.ActorId);

                    List<ContactAddressRow> customerContactAddressRows = customerContact != null ? ContactManager.GetContactAddressRows(customerContact.ContactId) : null;

                    if (customerInvoice.DeliveryCustomerId.HasValue && customerInvoice.DeliveryCustomerId.Value != customerInvoice.ActorId)
                    {
                        // Fetch addresses from delivery customer
                        Contact deliveryCustomerContact = ContactManager.GetContactFromActor(customerInvoice.DeliveryCustomerId.Value);
                        if (deliveryCustomerContact != null)
                            customerContactAddressRows.AddRange(ContactManager.GetContactAddressRows(deliveryCustomerContact.ContactId));
                    }

                    #endregion

                    #region Tracing

                    InvoiceTraceViewDTO invoiceOrder = null;
                    InvoiceTraceViewDTO invoiceContract = null;
                    List<OrderTraceViewDTO> previousInvoices = new List<OrderTraceViewDTO>();
                    List<OfferTraceViewDTO> previousOrders = new List<OfferTraceViewDTO>();
                    string creditedInvoicesText = "";

                    if (isBillingInvoiceTemplate)
                    {
                        //Get the Orders the Invoice was created from. Must be only 1 order (Otherwise unable to track, i.e. merged invoices)
                        var invoiceOrders = InvoiceManager.GetInvoiceTraceViews(entities, customerInvoice.InvoiceId, baseSysCurrencyId, isOrder: true);
                        if (invoiceOrders.Count == 1)
                        {
                            invoiceOrder = invoiceOrders.First();

                            //Get other Invoices the Order created. Can be more than one.
                            previousInvoices = InvoiceManager.GetOrderTraceViews(entities, invoiceOrder.OrderId, baseSysCurrencyId, isInvoice: true);

                            //Filter self
                            previousInvoices = previousInvoices.Where(i => i.InvoiceId != customerInvoice.InvoiceId && !i.IsSupplierInvoice).ToList();
                        }

                        //Get the Contracts the Invoice was created from. Must be only 1 contract (Otherwise unable to track, i.e. merged invoices)
                        var invoiceContracts = InvoiceManager.GetInvoiceTraceViews(entities, customerInvoice.InvoiceId, baseSysCurrencyId, isContract: true);
                        if (invoiceContracts.Count == 1)
                            invoiceContract = invoiceContracts.First();

                        if (customerInvoice.IsCredit)
                        {
                            var originalInvoices = InvoiceManager.GetInvoiceTraceViews(entities, customerInvoice.InvoiceId, baseSysCurrencyId, isInvoice: true);
                            if (originalInvoices.Count > 0)
                            {
                                if (originalInvoices.Count == 1)
                                {
                                    creditedInvoicesText = GetReportText(317, "Avser faktura:") + " " + originalInvoices.First().Number;

                                    if (invoiceOrder == null)
                                    {
                                        var invoiceOrdersCredit = InvoiceManager.GetInvoiceTraceViews(entities, originalInvoices.First().MappedInvoiceId, baseSysCurrencyId, isOrder: true);
                                        if (invoiceOrdersCredit.Count == 1)
                                        {
                                            invoiceOrder = invoiceOrdersCredit.First();

                                            //Get other Invoices the Order created. Can be more than one.
                                            /*previousInvoices = InvoiceManager.GetOrderTraceViews(entities, invoiceOrder.OrderId, baseSysCurrencyId, isInvoice: true);

                                            //Filter self
                                            previousInvoices = previousInvoices.Where(i => i.InvoiceId != customerInvoice.InvoiceId).ToList();*/
                                        }
                                    }
                                }
                                else
                                {
                                    creditedInvoicesText = GetReportText(318, "Avser fakturorna:") + " ";

                                    int count = 0;
                                    foreach (var creditedInvoice in originalInvoices)
                                    {
                                        count++;
                                        creditedInvoicesText += creditedInvoice.Number;
                                        if (count != originalInvoices.Count)
                                            creditedInvoicesText += ", ";
                                    }
                                }
                            }
                        }
                    }
                    else if (isBillingOrderTemplate)
                    {
                        //Get the Offers the Order was created from. Must be only 1 offer (Otherwise unable to track, i.e. merged orders)
                        //var orderOffers = InvoiceManager.GetOrderTraceViews(entities, customerInvoice.InvoiceId, baseSysCurrencyId, isOffer: true);
                        var orderOffers = InvoiceManager.GetMappedInvoices(entities, customerInvoice.InvoiceId, SoeOriginInvoiceMappingType.Offer, false, true);
                        if (orderOffers.Count == 1)
                        {
                            var orderOffer = orderOffers.First();

                            //Get other Orders the Offer created. Can be more than one.
                            previousOrders = InvoiceManager.GetOfferTraceViews(entities, orderOffer.InvoiceId, baseSysCurrencyId, isOrder: true);

                            //Filter self
                            previousOrders = previousOrders.Where(i => i.OrderId != customerInvoice.InvoiceId).ToList();
                        }
                    }

                    SoeBillingInvoiceReportType billingInvoiceReportType = SoeBillingInvoiceReportType.Unknown;
                    List<CustomerInvoice> originInvoices = new List<CustomerInvoice>();
                    if (isBillingContractTemplate)
                    {
                        billingInvoiceReportType = SoeBillingInvoiceReportType.Contract;
                    }
                    else if (isBillingOfferTemplate)
                    {
                        billingInvoiceReportType = SoeBillingInvoiceReportType.Offer;
                    }
                    else if (isBillingOrderTemplate)
                    {
                        billingInvoiceReportType = SoeBillingInvoiceReportType.Order;
                    }
                    else if (isBillingInvoiceTemplate)
                    {
                        billingInvoiceReportType = SoeBillingInvoiceReportType.Invoice;
                    }
                    else if (isBillingInvoiceReminderTemplate)
                    {
                        if (customerInvoice.BillingType == (int)TermGroup_BillingType.Debit || customerInvoice.BillingType == (int)TermGroup_BillingType.Credit || customerInvoice.BillingType == (int)TermGroup_BillingType.Interest)
                        {
                            billingInvoiceReportType = SoeBillingInvoiceReportType.ClaimQuick;

                            //Add self
                            originInvoices.Add(customerInvoice);

                            //Increase reminder level
                            if (reportParams.SB_InvoiceReminder)
                                InvoiceManager.IncreaseCustomerInvoiceReminder(entities, customerInvoice, reportResult.ActorCompanyId, true);
                        }
                        else if (customerInvoice.BillingType == (int)TermGroup_BillingType.Reminder)
                        {
                            billingInvoiceReportType = SoeBillingInvoiceReportType.Claim;

                            //Add original CustomerInvoice(s)
                            originInvoices.AddRange(InvoiceManager.GetOriginInvoicesFromReminder(entities, customerInvoice.InvoiceId));
                        }
                    }
                    else if (isBillingInvoiceInterestTemplate)
                    {
                        if (customerInvoice.BillingType == (int)TermGroup_BillingType.Debit)
                        {
                            billingInvoiceReportType = SoeBillingInvoiceReportType.Interest;

                            //Add self
                            originInvoices.Add(customerInvoice);
                        }
                        else if (customerInvoice.BillingType == (int)TermGroup_BillingType.Interest)
                        {
                            billingInvoiceReportType = SoeBillingInvoiceReportType.Interest;

                            //Add original CustomerInvoice(s)
                            originInvoices.AddRange(InvoiceManager.GetOriginInvoicesFromInterest(entities, customerInvoice.InvoiceId));
                        }
                    }

                    #endregion

                    #region OriginUser

                    var originUsers = OriginManager.GetOriginUsers(entities, customerInvoice.InvoiceId);

                    #endregion

                    for (int copy = 0; copy <= nbrOfCopies; copy++)
                    {
                        #region Copy

                        #region ReportHeader (override)

                        if (invoiceHeadXmlId == 1 && customerInvoices.Count == 1 && repository.HasSavedHistory)
                        {
                            //Override ReportHeader if current has history and not multiple reports are printed (in that case keep original ReportHeader)
                            reportHeaderElement = CreateBillingReportHeaderElement(reportResult, reportParams, repository);
                        }

                        #endregion

                        #region InvoiceHead

                        string invoiceCopyText = "";
                        if (customerInvoice.Origin.Status == (int)SoeOriginStatus.Cancel)
                            invoiceCopyText = GetReportText(264, "Makulerad");
                        else if ((isInvoiceTemplate && reportParams.SB_InvoiceCopy && customerInvoice.BillingInvoicePrinted) || copy > 0)
                            invoiceCopyText = GetReportText(144, "Kopia");
                        else if (Company.Demo)
                            invoiceCopyText = GetReportText(283, "Demo");

                        string invoiceParentOrderNrText = showOrdernrOnInvoiceReport ? customerInvoice.OrderNumbers != String.Empty ? customerInvoice.OrderNumbers : (invoiceOrder != null ? invoiceOrder.Number : String.Empty) : String.Empty;
                        string invoiceParentContractNrText = showOrdernrOnInvoiceReport && invoiceContract != null ? invoiceContract.Number : String.Empty;
                        string invoiceBillingInterestText = interestPercent > 0 ? GetReportText(140, "Dröjsmålsränta debiteras med") + " " + Decimal.Round(interestPercent, 2) + GetReportText(141, "% efter förfallodatum") : "";
                        string InvoiceInvertedVatText = "";
                        if (customerInvoice.VatType == (int)TermGroup_InvoiceVatType.Contractor)
                            InvoiceInvertedVatText = GetReportText(143, "Omvänd betalningsskyldighet");
                        else if (customerInvoice.VatType == (int)TermGroup_InvoiceVatType.ExportWithinEU && customer.VatNr != null)
                            InvoiceInvertedVatText = GetReportText(143, "Omvänd betalningsskyldighet");
                        // Reduce amounts on rows with hidden attest states from head
                        if ((billingInvoiceReportType == SoeBillingInvoiceReportType.Order || billingInvoiceReportType == SoeBillingInvoiceReportType.Offer) && hiddenAttestStateIds.Any())
                        {
                            List<CustomerInvoiceRow> customerHiddenInvoiceRows = customerInvoice.ActiveCustomerInvoiceRows.Where(i => i.AttestStateId.HasValue && hiddenAttestStateIds.Contains(i.AttestStateId.Value)).ToList();
                            foreach (CustomerInvoiceRow customerInvoiceRow in customerHiddenInvoiceRows)
                            {
                                customerInvoice.SumAmountCurrency -= customerInvoiceRow.SumAmountCurrency;
                                customerInvoice.SumAmount -= customerInvoiceRow.SumAmount;
                                customerInvoice.VATAmountCurrency -= customerInvoiceRow.VatAmountCurrency;
                                customerInvoice.VATAmount -= customerInvoiceRow.VatAmount;
                                customerInvoice.TotalAmountCurrency -= customerInvoiceRow.SumAmountCurrency;
                                customerInvoice.TotalAmount -= customerInvoiceRow.SumAmount;
                                customerInvoice.TotalAmountCurrency -= customerInvoiceRow.VatAmountCurrency;
                                customerInvoice.TotalAmount -= customerInvoiceRow.VatAmount;
                            }

                        }
                        //If specified in company-settings, add C/O before distributionAddress.
                        XElement invoiceHeadElement = CreateBillingInvoiceHeadElement(invoiceHeadXmlId, billingInvoiceReportType, customerInvoice, customerContactAddressRows, sysCurrency, invoiceCopyText, invoiceParentOrderNrText, invoiceBillingInterestText, InvoiceInvertedVatText, creditedInvoicesText, invoiceParentContractNrText, repository, showCOLabel, originType, UseDeliveryAddressAsBillingAddress, interestPercent);
                        if (invoiceHeadElement == null)
                            continue;

                        #endregion

                        #region InvoiceRow

                        int invoiceRowXmlId = 1;
                        List<int> targetRowIds = new List<int>();
                        List<XElement> invoiceRowElements = new List<XElement>();
                        bool orderOfferHasLiftRows = false;
                        bool allHasLiftRows = false;
                        bool allIsFixedPrice = false;
                        bool isFixedPriceType1 = false;
                        bool isFixedPriceType2 = false;
                        // Calculate all orders remaing invoice amount
                        decimal remainingAmount = 0;
                        decimal remainingVatAmount = 0;
                        decimal remainingAmountIncVat = 0;
                        decimal retainedGuranteeAmount = 0;
                        decimal retainedGuranteeVatAmount = 0;
                        decimal fixedPriceAmount = 0;
                        decimal fixedPriceVatAmount = 0;

                        List<CustomerInvoiceRow> customerInvoiceRows = customerInvoice.ActiveCustomerInvoiceRows.Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.RowNr).ToList();
                        foreach (CustomerInvoiceRow customerInvoiceRow in customerInvoiceRows)
                        {
                            targetRowIds.Add(customerInvoiceRow.CustomerInvoiceRowId);

                            #region CustomerInvoiceRow

                            // Skip rows with hidden attest states
                            if (customerInvoiceRow.AttestStateId.HasValue && hiddenAttestStateIds.Contains(customerInvoiceRow.AttestStateId.Value))
                                continue;

                            #region Product

                            Product product = null;
                            if (customerInvoiceRow.Type == (int)SoeInvoiceRowType.ProductRow)
                            {
                                //valid
                                if (customerInvoiceRow.Product == null)
                                    continue;

                                if (customerInvoiceRow.ProductId.HasValue)
                                {
                                    product = customerInvoiceRow.Product;
                                    if (product == null)
                                    {
                                        product = ProductManager.GetProduct(customerInvoiceRow.ProductId.Value, false, loadPriceList: true, loadProductUnit: true);
                                    }
                                    else if (!product.PriceList.IsLoaded)
                                    {
                                        product.PriceList.Load();
                                    }
                                }
                            }
                            else if (customerInvoiceRow.Type == (int)SoeInvoiceRowType.TextRow || customerInvoiceRow.Type == (int)SoeInvoiceRowType.PageBreakRow || customerInvoiceRow.Type == (int)SoeInvoiceRowType.SubTotalRow)
                            {
                                //valid
                                product = null;
                            }
                            else
                            {
                                //skip
                                continue;
                            }
                            if ((billingInvoiceReportType == SoeBillingInvoiceReportType.Order || billingInvoiceReportType == SoeBillingInvoiceReportType.Offer))
                            {
                                if (customerInvoiceRow.Product != null && fixedPriceProductId != 0 && customerInvoiceRow.ProductId == fixedPriceProductId)
                                {
                                    isFixedPriceType1 = true;
                                    fixedPriceAmount += customerInvoiceRow.Amount;
                                    fixedPriceVatAmount += customerInvoiceRow.VatAmount;
                                }


                                if (customerInvoiceRow.Product != null && fixedPriceKeepPricesProductId != 0 && customerInvoiceRow.ProductId == fixedPriceKeepPricesProductId)
                                {
                                    isFixedPriceType2 = true;
                                    fixedPriceAmount += customerInvoiceRow.Amount;
                                    fixedPriceVatAmount += customerInvoiceRow.VatAmount;
                                }
                            }
                            #endregion
                            int rowHasStateToInvoice = 0;
                            if (customerInvoiceRow.AttestStateId == attestStateTransferredOrderToInvoiceId)
                            {
                                rowHasStateToInvoice = 1;
                            }

                            //XElement invoiceRowElement = CreateBillingInvoiceRowElement(invoiceRowXmlId, reportResult, reportParams, customerInvoiceRow, product, attestStates, vatCodes, householdProductId, household50ProductId, householdRutProductId, householdGreen15ProductId, householdGreen20ProductId, householdGreen50ProductId, fixedPriceProductId, fixedPriceKeepPricesProductId, rowHasStateToInvoice, transactions, useProjectTimeBlock, productRowDescriptionToUpperCase);
                            houseHoldDeductionTypeDict.TryGetValue(customerInvoiceRow.HouseholdDeductionType ?? 0, out var houseHoldDecuctionType);
 
                            XElement invoiceRowElement = CreateBillingInvoiceRowElement(invoiceRowXmlId, reportResult, reportParams, customerInvoiceRow, product, attestStates, vatCodes, householdProductId, household50ProductId, householdRutProductId, householdGreen15ProductId, householdGreen20ProductId, householdGreen50ProductId, fixedPriceProductId, fixedPriceKeepPricesProductId, rowHasStateToInvoice, transactions, useProjectTimeBlock, productRowDescriptionToUpperCase, houseHoldDecuctionType);

                            if (invoiceRowElement == null)
                                continue;

                            if ((customerInvoiceRow.Product != null && ((InvoiceProduct)customerInvoiceRow.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift) && (billingInvoiceReportType == SoeBillingInvoiceReportType.Order || billingInvoiceReportType == SoeBillingInvoiceReportType.Offer))
                                orderOfferHasLiftRows = true;

                            invoiceRowElements.Add(invoiceRowElement);
                            invoiceRowXmlId++;

                            #endregion
                        }

                        #region Default element InvoiceRow

                        if (invoiceRowXmlId == 1)
                            invoiceRowElements.Add(new XElement("InvoiceRow", String.Empty));

                        #endregion

                        #endregion

                        #region Remaining amounts

                        if (isBillingInvoiceTemplate)
                        {
                            var invoiceOrders = InvoiceManager.GetInvoiceTraceViews(entities, customerInvoice.InvoiceId, baseSysCurrencyId, isOrder: true);
                            foreach (var itv in invoiceOrders)
                            {
                                var cusInvoice = InvoiceManager.GetCustomerInvoice(entities, itv.OrderId, loadInvoiceRow: true);

                                if (cusInvoice == null)
                                {
                                    LogWarning($"Printing invoice nr {customerInvoice.InvoiceNr} where orderId {itv.OrderId} is deleted");
                                }
                                else
                                {
                                    if (cusInvoice.FixedPriceOrder)
                                    {
                                        allIsFixedPrice = true;

                                        bool hasLiftRows = false;
                                        foreach (var cusInvRow in cusInvoice.ActiveCustomerInvoiceRows.Where(r => r.TargetRowId != null && targetRowIds.Contains((int)r.TargetRowId)))
                                        {
                                            if (cusInvRow.Product != null && ((InvoiceProduct)cusInvRow.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift)
                                            {
                                                hasLiftRows = true;
                                                allHasLiftRows = true;
                                                break;
                                            }
                                        }

                                        foreach (var cusInvRow in cusInvoice.ActiveCustomerInvoiceRows.Where(r => r.TargetRowId == null))
                                        {
                                            //Product is guaranteeamount 
                                            //if (cusInvRow.Product != null && productGuaranteeId != 0 && cusInvRow.ProductId == productGuaranteeId)
                                            //{
                                            //    retainedGuranteeAmount += cusInvRow.Amount;
                                            //}

                                            if (cusInvRow.Product != null && fixedPriceProductId != 0 && cusInvRow.ProductId == fixedPriceProductId)
                                            {
                                                isFixedPriceType1 = true;
                                                fixedPriceAmount += cusInvRow.Amount;
                                                fixedPriceVatAmount += cusInvRow.VatAmount;
                                            }

                                            if (cusInvRow.Product != null && fixedPriceKeepPricesProductId != 0 && cusInvRow.ProductId == fixedPriceKeepPricesProductId)
                                            {
                                                isFixedPriceType2 = true;
                                                fixedPriceAmount += cusInvRow.Amount;
                                                fixedPriceVatAmount += cusInvRow.VatAmount;
                                            }

                                        }
                                        //Product is guaranteeamount only invoiced rows
                                        foreach (var cusInvRow in cusInvoice.ActiveCustomerInvoiceRows.Where(r => r.Product != null && productGuaranteeId != 0 && r.ProductId == productGuaranteeId))
                                        {
                                            if (cusInvRow.TargetRowId != null)
                                            {
                                                if (!cusInvRow.TargetRowReference.IsLoaded)
                                                {
                                                    cusInvRow.TargetRowReference.Load();
                                                    cusInvRow.TargetRow.CustomerInvoiceReference.Load();
                                                }
                                                if (cusInvRow.TargetRow.CustomerInvoice.InvoiceId > customerInvoice.InvoiceId)
                                                    continue;
                                                else
                                                {
                                                    retainedGuranteeAmount += cusInvRow.Amount;
                                                    retainedGuranteeVatAmount += cusInvRow.VatAmount;
                                                }
                                            }
                                            else
                                            {
                                                retainedGuranteeAmount += cusInvRow.Amount;
                                                retainedGuranteeVatAmount += cusInvRow.VatAmount;
                                            }

                                        }

                                        if (hasLiftRows || showRemainingAmountInReport)
                                        {
                                            remainingAmount += cusInvoice.RemainingAmountExVat != null ? (decimal)cusInvoice.RemainingAmountExVat : 0;
                                            remainingVatAmount += cusInvoice.RemainingAmountVat != null ? (decimal)cusInvoice.RemainingAmountVat : 0;
                                            remainingAmountIncVat += cusInvoice.RemainingAmount != null ? (decimal)cusInvoice.RemainingAmount : 0;
                                        }
                                        else
                                        {
                                            allIsFixedPrice = false;
                                            allHasLiftRows = false;
                                            remainingAmount = 0;
                                            remainingVatAmount = 0;
                                            remainingAmountIncVat = 0;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        bool hasLiftRows = false;
                                        foreach (var cusInvRow in cusInvoice.ActiveCustomerInvoiceRows)
                                        {
                                            if (cusInvRow.Product != null)
                                            {
                                                if (((InvoiceProduct)cusInvRow.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift)
                                                {
                                                    hasLiftRows = true;
                                                    allHasLiftRows = true;
                                                }
                                                else if (cusInvRow.ProductId == fixedPriceProductId)
                                                {
                                                    allIsFixedPrice = true;
                                                    isFixedPriceType1 = true;
                                                    fixedPriceAmount += cusInvRow.Amount;
                                                    fixedPriceVatAmount += cusInvRow.VatAmount;
                                                }
                                                else if (cusInvRow.ProductId == fixedPriceKeepPricesProductId)
                                                {
                                                    allIsFixedPrice = true;
                                                    isFixedPriceType2 = true;
                                                    fixedPriceAmount += cusInvRow.Amount;
                                                    fixedPriceVatAmount += cusInvRow.VatAmount;
                                                }
                                                else
                                                {
                                                    fixedPriceAmount += cusInvRow.SumAmount;
                                                    fixedPriceVatAmount += cusInvRow.VatAmount;
                                                }
                                                //Product is guaranteeamount only invoiced rows
                                                if (cusInvRow.Product != null && productGuaranteeId != 0 && cusInvRow.ProductId == productGuaranteeId && cusInvRow.TargetRowId != null)
                                                {
                                                    if (!cusInvRow.TargetRowReference.IsLoaded)
                                                    {
                                                        cusInvRow.TargetRowReference.Load();
                                                        cusInvRow.TargetRow.CustomerInvoiceReference.Load();
                                                    }
                                                    if (cusInvRow.TargetRow.CustomerInvoice.InvoiceId > customerInvoice.InvoiceId)
                                                        continue;
                                                    else
                                                    {
                                                        retainedGuranteeAmount += cusInvRow.Amount;
                                                        retainedGuranteeVatAmount += cusInvRow.VatAmount;
                                                    }
                                                }

                                            }
                                        }

                                        if (hasLiftRows || showRemainingAmountInReport)
                                        {
                                            remainingAmount += cusInvoice.RemainingAmountExVat != null ? (decimal)cusInvoice.RemainingAmountExVat : 0;
                                            remainingVatAmount += cusInvoice.RemainingAmountVat != null ? (decimal)cusInvoice.RemainingAmountVat : 0;
                                            remainingAmountIncVat += cusInvoice.RemainingAmount != null ? (decimal)cusInvoice.RemainingAmount : 0;
                                        }
                                        else
                                        {
                                            allIsFixedPrice = false;
                                            allHasLiftRows = false;
                                            remainingAmount = 0;
                                            remainingVatAmount = 0;
                                            remainingAmountIncVat = 0;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        //if (billingInvoiceReportType == SoeBillingInvoiceReportType.Invoice)
                        //{
                        //    remainingAmount = customerInvoice.RemainingAmountExVat != null ? (decimal)customerInvoice.RemainingAmountExVat : 0;
                        //    remainingVatAmount = customerInvoice.RemainingAmountVat != null ? (decimal)customerInvoice.RemainingAmountVat : 0;
                        //    remainingAmountIncVat = customerInvoice.RemainingAmount != null ? (decimal)customerInvoice.RemainingAmount : 0;
                        //}

                        if ((billingInvoiceReportType == SoeBillingInvoiceReportType.Order || billingInvoiceReportType == SoeBillingInvoiceReportType.Offer))
                        {
                            remainingAmount = customerInvoice.RemainingAmountExVat != null ? (decimal)customerInvoice.RemainingAmountExVat : 0;
                            remainingVatAmount = customerInvoice.RemainingAmountVat != null ? (decimal)customerInvoice.RemainingAmountVat : 0;
                            remainingAmountIncVat = customerInvoice.RemainingAmount != null ? (decimal)customerInvoice.RemainingAmount : 0;

                            if (orderOfferHasLiftRows)
                            {
                                allHasLiftRows = true;
                            }

                            if (customerInvoice.FixedPriceOrder)
                            {
                                allIsFixedPrice = true;
                            }

                        }

                        int invoiceHasProject = customerInvoice.ProjectId.HasValue ? 1 : 0;
                        int showTimeProject = 0;
                        int showExpenseReport = 0;
                        int showSignatures = 1;

                        #region TimeProjectReport

                        XElement timeProjectReportElement = new XElement("Project");

                        //Deleted as fix for bug 12509
                        /*XElement timeProjectReportElement = new XElement("Project",
                                new XAttribute("id", 1));*/

                        bool includeTimeProjectSetting = false;

                        if ((billingInvoiceReportType == SoeBillingInvoiceReportType.Order && orderIncludeTimeProjectinReport) || (billingInvoiceReportType == SoeBillingInvoiceReportType.Invoice && invoiceIncludeTimeProjectinReport))
                            includeTimeProjectSetting = true;

                        /*if (includeTimeProjectSetting && (customerInvoice.ProjectId.HasValue && (es.SB_IncludeProjectReport || es.SB_IncludeProjectReport2)))
                            timeProjectReportElement = CreateTimeProjectElement(entities, es, null, timeCodes, customerInvoice.InvoiceId, es.ActorCompanyId, true, out nrOfTimeProjectRows);

                        if (nrOfTimeProjectRows != 0)
                            showTimeProject = 1;*/

                        #endregion

                        #region ExpenseReport

                        XElement expenseReportElement = new XElement("Expenses");

                        #endregion

                        invoiceHeadElement.Add(new XElement("InvoiceSumRemainingAmount", remainingAmount));
                        invoiceHeadElement.Add(new XElement("InvoiceSumRemainingAmountVat", remainingVatAmount));
                        invoiceHeadElement.Add(new XElement("InvoiceSumRemainingAmountIncVat", remainingAmountIncVat));
                        invoiceHeadElement.Add(new XElement("InvoiceHasLiftRows", allHasLiftRows));
                        invoiceHeadElement.Add(new XElement("InvoiceHasFixedPriceRows", allIsFixedPrice));
                        invoiceHeadElement.Add(new XElement("InvoiceIsFixedPriceType1", isFixedPriceType1));
                        invoiceHeadElement.Add(new XElement("InvoiceIsFixedPriceType2", isFixedPriceType2));
                        invoiceHeadElement.Add(new XElement("InvoiceRetainedGuaranteeAmount", retainedGuranteeAmount));
                        invoiceHeadElement.Add(new XElement("InvoiceRetainedGuaranteeVatAmount", Math.Abs(retainedGuranteeVatAmount)));
                        invoiceHeadElement.Add(new XElement("InvoiceFixedPriceAmount", fixedPriceAmount));
                        invoiceHeadElement.Add(new XElement("InvoiceFixedPriceVatAmount", fixedPriceVatAmount));
                        invoiceHeadElement.Add(new XElement("InvoiceHasProject", invoiceHasProject));
                        invoiceHeadElement.Add(new XElement("ShowTimeProjectReport", showTimeProject));
                        invoiceHeadElement.Add(new XElement("ShowExpenseReport", showExpenseReport));
                        invoiceHeadElement.Add(new XElement("ShowSignatures", showSignatures));
                        invoiceHeadElement.Add(new XElement("ExistHouseHouldDeduction", customerInvoice.HasHouseholdTaxDeduction));


                        bool hasPreviousInvoices = false;

                        if (previousInvoices.Count > 0 && isBillingInvoiceTemplate)
                        {
                            hasPreviousInvoices = true;
                        }

                        if (hasPreviousInvoices)
                            invoiceHeadElement.Add(new XElement("HasPreviousInvoices", "1"));
                        else
                            invoiceHeadElement.Add(new XElement("HasPreviousInvoices", "0"));

                        invoiceHeadElement.Add(new XElement("ShowRemainingAmountInReport", showRemainingAmountInReport ? "1" : "0"));

                        invoiceHeadElement.Add(new XElement("IsOneTimeCustomer", isOneTimeCustomer ? "1" : "0"));

                        Contact custContact = ContactManager.GetContactFromActor(customer.ActorCustomerId, loadActor: true, loadAllContactInfo: true);
                        string phoneMobile = string.Empty;
                        if (custContact != null && custContact.ContactECom != null)
                        {
                            foreach (var contactEComItem in custContact.ContactECom)
                            {
                                if (contactEComItem.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile)
                                    phoneMobile = contactEComItem.Text;
                            }
                        }

                        invoiceHeadElement.Add(new XElement("CustomerPhoneNr", isOneTimeCustomer ? customerInvoice.CustomerPhoneNr : phoneMobile));
                        #endregion

                        #region Contract values

                        string contractGroupName = "", contractGroupPeriodType = "", contractNextYear = "", contractNextPeriod = "", contractEndYear = "", contractEndPeriod = "", contractCategoryName = "";

                        if (originType == SoeOriginType.Contract)
                        {
                            if (customerInvoice.ContractGroup != null)
                            {
                                contractGroupName = customerInvoice.ContractGroup.Name;
                                GenericType periodType = contractGroupPeriodTerms.FirstOrDefault(p => p.Id == customerInvoice.ContractGroup.Period);
                                if (periodType != null)
                                    contractGroupPeriodType = periodType.Name;

                                contractNextYear = customerInvoice.NextContractPeriodYear.ToString();
                                contractNextPeriod = customerInvoice.NextContractPeriodValue.ToString();

                                if (customerInvoice.DueDate.HasValue)
                                {
                                    Tuple<int, int> tuple = CalendarUtility.CalculateCurrentPeriod((TermGroup_ContractGroupPeriod)customerInvoice.ContractGroup.Period, customerInvoice.DueDate.Value);
                                    contractEndYear = tuple.Item1.ToString();
                                    contractEndPeriod = tuple.Item2.ToString();
                                }
                            }
                            Category category = CategoryManager.GetCategory(SoeCategoryType.Contract, SoeCategoryRecordEntity.Contract, customerInvoice.InvoiceId, (int)customerInvoice.Origin.ActorCompanyId, onlyDefaultCategory: false);
                            if (category != null)
                            {
                                //contractCategoryCode = category.Code;
                                contractCategoryName = category.Name;
                            }
                        }

                        invoiceHeadElement.Add(new XElement("ContractGroupName", contractGroupName));
                        invoiceHeadElement.Add(new XElement("ContractGroupPeriodType", contractGroupPeriodType));
                        invoiceHeadElement.Add(new XElement("ContractNextYear", contractNextYear));
                        invoiceHeadElement.Add(new XElement("ContractNextPeriod", contractNextPeriod));
                        invoiceHeadElement.Add(new XElement("ContractEndYear", contractEndYear));
                        invoiceHeadElement.Add(new XElement("ContractEndPeriod", contractEndPeriod));
                        invoiceHeadElement.Add(new XElement("ContractCategoryName", contractCategoryName));

                        #endregion

                        invoiceHeadElement.Add(new XElement("InvoiceReminderFee", reminderAmount));
                        invoiceHeadElement.Add(new XElement("InvoiceInterestAmount", interestAmount));

                        #region QRCode

                        string qrLink = string.Empty;

                        if (reportResult.GetDetailedInformation && !string.IsNullOrEmpty(customerInvoice.InvoiceNr) && customerInvoice.InvoiceDate.HasValue && customerInvoice.DueDate.HasValue)
                        {
                            QRCode qrCode = new QRCode();

                            string invoiceReference = !string.IsNullOrEmpty(customerInvoice.OCR) ? customerInvoice.OCR : "Nr: " + customerInvoice.InvoiceNr;

                            if (!string.IsNullOrEmpty(bg))
                                qrLink = CreateInvoiceQR(entities, qrCode, invoiceReference, distributionPostalAddress, (DateTime)customerInvoice.InvoiceDate, (DateTime)customerInvoice.DueDate, customerInvoice.TotalAmountCurrency, customerInvoice.VATAmountCurrency, Company.Name, sysCurrency != null ? sysCurrency.Code : "", Company.OrgNr, TermGroup_SysPaymentType.BG, bg);
                            else if (!string.IsNullOrEmpty(pg))
                                qrLink = CreateInvoiceQR(entities, qrCode, invoiceReference, distributionPostalAddress, (DateTime)customerInvoice.InvoiceDate, (DateTime)customerInvoice.DueDate, customerInvoice.TotalAmountCurrency, customerInvoice.VATAmountCurrency, Company.Name, sysCurrency != null ? sysCurrency.Code : "", Company.OrgNr, TermGroup_SysPaymentType.PG, pg);
                        }

                        invoiceHeadElement.Add(new XElement("QrLink", qrLink));

                        #endregion

                        //Add rows
                        foreach (XElement el in invoiceRowElements)
                            invoiceHeadElement.Add(el);

                        #region PreviousInvoiceHead

                        int previousInvoiceHeadXmlId = 1;
                        if (isBillingInvoiceTemplate)
                        {
                            #region Previous invoices

                            foreach (var previousInvoice in previousInvoices)
                            {
                                //Exclude preliminary and invoices with higher number
                                bool valid = !String.IsNullOrEmpty(previousInvoice.Number) && Validator.ValidateStringInterval(previousInvoice.Number, customerInvoice.InvoiceNr);
                                if (!valid)
                                    continue;
                                //Exclude deleted invoices
                                if (previousInvoice.State == SoeEntityState.Deleted)
                                    continue;
                                XElement previousInvoiceHeadElement = new XElement("PreviousInvoiceHead");
                                previousInvoiceHeadElement.Add(
                                    new XAttribute("id", previousInvoiceHeadXmlId),
                                    new XElement("PreviousInvoiceInvoiceNr", previousInvoice.Number),
                                    new XElement("PreviousInvoiceInvoiceVatAmount", previousInvoice.VatAmount),
                                    new XElement("PreviousInvoiceInvoiceTotalAmount", previousInvoice.AmountCurrency),
                                    new XElement("PreviousInvoiceInvoiceTotalAmountBase", previousInvoice.Amount),
                                    new XElement("PreviousInvoiceInvoiceDate", previousInvoice.Date.HasValue ? previousInvoice.Date.Value : CalendarUtility.DATETIME_DEFAULT));

                                invoiceHeadElement.Add(previousInvoiceHeadElement);
                                previousInvoiceHeadXmlId++;
                            }

                            #endregion
                        }
                        else if (isBillingOrderTemplate)
                        {
                            #region Previous orders

                            foreach (var previousOrder in previousOrders)
                            {
                                //Exclude preliminary and orders with higher number
                                bool valid = !String.IsNullOrEmpty(previousOrder.Number) && Validator.ValidateStringInterval(previousOrder.Number, customerInvoice.InvoiceNr);
                                if (!valid)
                                    continue;

                                XElement previousInvoiceHeadElement = new XElement("PreviousInvoiceHead");
                                previousInvoiceHeadElement.Add(
                                    new XAttribute("id", previousInvoiceHeadXmlId),
                                    new XElement("PreviousInvoiceInvoiceNr", previousOrder.Number),
                                    new XElement("PreviousInvoiceInvoiceVatAmount", previousOrder.VatAmount),
                                    new XElement("PreviousInvoiceInvoiceTotalAmount", previousOrder.AmountCurrency),
                                    new XElement("PreviousInvoiceInvoiceTotalAmountBase", previousOrder.Amount),
                                    new XElement("PreviousInvoiceInvoiceDate", previousOrder.Date.HasValue ? previousOrder.Date.Value : CalendarUtility.DATETIME_DEFAULT));

                                invoiceHeadElement.Add(previousInvoiceHeadElement);

                                previousInvoiceHeadXmlId++;
                            }

                            #endregion
                        }

                        #region Default element PreviousInvoiceHead

                        if (previousInvoiceHeadXmlId == 1)
                            invoiceHeadElement.Add(new XElement("PreviousInvoiceHead", String.Empty));

                        #endregion

                        #endregion

                        #region OriginInvoiceHead

                        int originInvoiceHeadXmlId = 1;
                        if (isBillingInvoiceReminderTemplate)
                        {
                            #region Reminder

                            foreach (CustomerInvoice originInvoice in originInvoices)
                            {
                                XElement originInvoiceHeadElement = new XElement("OriginInvoiceHead");
                                originInvoiceHeadElement.Add(
                                    new XAttribute("id", originInvoiceHeadXmlId),
                                    new XElement("InvoiceDate", originInvoice.InvoiceDate),
                                    new XElement("InvoiceDueDate", originInvoice.DueDate),
                                    new XElement("InvoiceNr", originInvoice.InvoiceNr),
                                    new XElement("InvoiceOCR", originInvoice.OCR),
                                    new XElement("InvoiceTotalAmount", originInvoice.TotalAmountCurrency),
                                    new XElement("InvoiceTotalAmountBase", originInvoice.TotalAmount),
                                    new XElement("InvoiceVatAmount", originInvoice.VATAmountCurrency),
                                    new XElement("InvoiceVatAmountBase", originInvoice.VATAmount),
                                    new XElement("InvoicePaidAmount", originInvoice.PaidAmountCurrency),
                                    new XElement("InvoicePaidAmountBase", originInvoice.PaidAmount),
                                    new XElement("OverduedDays", customerInvoice.InvoiceDate.HasValue && originInvoice.DueDate.HasValue ? customerInvoice.InvoiceDate.Value.Subtract(originInvoice.DueDate.Value).TotalDays : 0),
                                    new XElement("ClaimLevel", originInvoice.NoOfReminders),
                                    new XElement("ClaimText", InvoiceManager.GetClaimText(originInvoice, reportResult.ActorCompanyId)));

                                invoiceHeadElement.Add(originInvoiceHeadElement);
                                originInvoiceHeadXmlId++;
                            }

                            #endregion
                        }
                        else if (isBillingInvoiceInterestTemplate)
                        {
                            #region Interest

                            foreach (CustomerInvoice originInvoice in originInvoices)
                            {
                                XElement originInvoiceHeadElement = new XElement("OriginInvoiceHead");
                                originInvoiceHeadElement.Add(
                                    new XAttribute("id", originInvoiceHeadXmlId),
                                    new XElement("InvoiceDate", originInvoice.InvoiceDate),
                                    new XElement("InvoiceDueDate", originInvoice.DueDate),
                                    new XElement("InvoiceNr", originInvoice.InvoiceNr),
                                    new XElement("InvoiceOCR", originInvoice.OCR),
                                    new XElement("InvoiceTotalAmount", originInvoice.TotalAmountCurrency),
                                    new XElement("InvoiceTotalAmountBase", originInvoice.TotalAmount),
                                    new XElement("InvoiceVatAmount", originInvoice.VATAmountCurrency),
                                    new XElement("InvoiceVatAmountBase", originInvoice.VATAmount),
                                    new XElement("InvoicePaidAmount", originInvoice.PaidAmountCurrency),
                                    new XElement("InvoicePaidAmountBase", originInvoice.PaidAmount),
                                    new XElement("OverduedDays", customerInvoice.InvoiceDate.HasValue && originInvoice.DueDate.HasValue ? customerInvoice.InvoiceDate.Value.Subtract(originInvoice.DueDate.Value).TotalDays : 0),
                                    new XElement("ClaimLevel", 0),
                                    new XElement("ClaimText", String.Empty));

                                invoiceHeadElement.Add(originInvoiceHeadElement);
                                originInvoiceHeadXmlId++;
                            }

                            #endregion
                        }

                        #region Default element OriginInvoiceHead

                        if (originInvoiceHeadXmlId == 1)
                        {
                            XElement originInvoiceHeadElement = new XElement("OriginInvoiceHead");
                            originInvoiceHeadElement.Add(
                                new XAttribute("id", originInvoiceHeadXmlId),
                                new XElement("InvoiceDate", CalendarUtility.DATETIME_DEFAULT),
                                new XElement("InvoiceDueDate", CalendarUtility.DATETIME_DEFAULT),
                                new XElement("InvoiceNr", String.Empty),
                                new XElement("InvoiceOCR", String.Empty),
                                new XElement("InvoiceTotalAmount", Decimal.Zero),
                                new XElement("InvoiceTotalAmountBase", Decimal.Zero),
                                new XElement("InvoiceVatAmount", Decimal.Zero),
                                new XElement("InvoiceVatAmountBase", Decimal.Zero),
                                new XElement("InvoicePaidAmount", Decimal.Zero),
                                new XElement("InvoicePaidAmountBase", Decimal.Zero),
                                new XElement("OverduedDays", 0),
                                new XElement("ClaimLevel", 0),
                                new XElement("ClaimText", 0));
                        }

                        #endregion

                        #endregion

                        #region OriginUser

                        int originUserXmlId = 1;
                        foreach (var originUser in originUsers)
                        {
                            XElement originInvoiceHeadElement = new XElement("OriginUser");
                            originInvoiceHeadElement.Add(
                                new XAttribute("id", originUserXmlId),
                                new XElement("Name", originUser.Name),
                                new XElement("LoginName", originUser.LoginName),
                                new XElement("Main", originUser.Main.ToInt()));

                            invoiceHeadElement.Add(originInvoiceHeadElement);
                            originInvoiceHeadXmlId++;
                        }

                        #region Default element OriginUser

                        if (originUserXmlId == 1)
                        {
                            XElement originInvoiceHeadElement = new XElement("OriginUser");
                            originInvoiceHeadElement.Add(
                                new XAttribute("id", originInvoiceHeadXmlId),
                                new XElement("Name", CalendarUtility.DATETIME_DEFAULT),
                                new XElement("LoginName", CalendarUtility.DATETIME_DEFAULT),
                                new XElement("Main", 0));
                        }

                        #endregion

                        #endregion

                        #region Signature

                        List<XElement> invoiceSignatures = null;

                        if (isBillingOrderTemplate)
                        {
                            invoiceSignatures = CreateInvoiceSignatureElements(customerInvoice.InvoiceId);
                            invoiceHeadElement.Add(invoiceSignatures);
                        }

                        if (invoiceSignatures == null && invoiceHeadXmlId == 1)
                        {

                            string description = "";
                            string imagePath = "";
                            string type = "0";

                            XElement invoiceSignatureImage = new XElement("InvoiceSignatureImage",
                                new XAttribute("id", 1),
                                new XElement("ImagePath", imagePath),
                                new XElement("Type", type),
                                new XElement("Description", description));

                            invoiceHeadElement.Add(invoiceSignatureImage);
                        }

                        #endregion

                        #region TimeProjectReport

                        if (includeTimeProjectSetting && customerInvoice.PrintTimeReport && (customerInvoice.ProjectId.HasValue && (reportParams.SB_IncludeProjectReport || reportParams.SB_IncludeProjectReport2)))
                        {
                            // Reset only invoiced
                            reportParams.SB_IncludeOnlyInvoiced = customerInvoice.IncludeOnlyInvoicedTime;

                            //invoiceHeadElement.Add(timeProjectReportElement);

                            /*
                            if (ActorCompanyId == 7)
                            {
                                var stateUtility = new StateUtility(entities, InvoiceManager);
                                var generator = new TimeProjectDataReportGenerator(InvoiceManager, ProjectManager, AttestManager, TimeCodeManager, TimeDeviationCauseManager, TimeTransactionManager, SettingManager, stateUtility);
                                var timeSheetParams = new TimeProjectReportParams(ActorCompanyId, UserId, RoleId, customerInvoice.IncludeOnlyInvoicedTime, reportParams.DateFrom, reportParams.DateTo);
                                int invoiceId = reportParams.SB_InvoiceIds.Any() ? reportParams.SB_InvoiceIds.First() : 0;

                                var (timeSheetXData, foundTimeSheetData) = generator.CreateTimeProjectElement(entities, timeSheetParams, timeCodes, invoiceId);
                                invoiceHeadElement.Add(timeSheetXData);
                                if (foundTimeSheetData)
                                {
                                    invoiceHeadElement.SetElementValue("ShowTimeProjectReport", 1);
                                }
                            }
                            else
                            {
                                */

                            invoiceHeadElement = CreateTimeProjectElement(entities, reportResult, reportParams, invoiceHeadElement, timeCodes, customerInvoice.InvoiceId, reportResult.ActorCompanyId, true, out int nrOfTimeProjectRows);

                            if (nrOfTimeProjectRows != 0)
                            {
                                invoiceHeadElement.SetElementValue("ShowTimeProjectReport", 1);
                            }
                            //}
                        }
                        else if (invoiceHeadXmlId == 1)
                        {
                            #region Default element Project

                            timeProjectReportElement = new XElement("Project",
                                new XAttribute("id", 1),
                                new XElement("ProjectNumber", ""),
                                new XElement("ProjectName", ""),
                                new XElement("ProjectDescription", ""),
                                new XElement("ProjectInvoiceNr", ""),
                                new XElement("ProjectCreated", "00:00"),
                                new XElement("ProjectCreatedBy", ""),
                                new XElement("ProjectState", 0));

                            XElement defaultEmployeeElement = new XElement("Employee",
                                new XAttribute("id", 1),
                                new XElement("EmployeeNr", 0),
                                new XElement("EmployeeName", ""));

                            XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                                new XAttribute("id", 1),
                                new XElement("InvoiceTimeInMinutes", 0),
                                new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                                new XElement("Note", "00:00"),
                                new XElement("ExternalNote", string.Empty),
                                new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")),
                                new XElement("TDName", string.Empty),
                                new XElement("TBStartTime", DateTime.Now.TimeOfDay.ToShortTimeString()),
                                new XElement("TBStopTime", DateTime.Now.TimeOfDay.ToShortTimeString()));

                            defaultEmployeeElement.Add(defaultDayElement);
                            timeProjectReportElement.Add(defaultEmployeeElement);
                            invoiceHeadElement.Add(timeProjectReportElement);
                        }

                        #endregion

                        #endregion

                        #region ExpenseReport

                        expenseReportElement = CreateExpenseElement(entities, reportResult, reportParams, expenseReportElement, customerInvoice, customerInvoice.IncludeExpenseInReport, out int nrOfExpenseRows);
                        invoiceHeadElement.Add(expenseReportElement);
                        if (nrOfExpenseRows != 0)
                        {
                            invoiceHeadElement.SetElementValue("ShowExpenseReport", 1);
                        }

                        #endregion

                        #region InvoicePayments

                        XElement invoicePaymentElements = new XElement("Payments");
                        int invoicePaymentXmlId = 1;
                        List<XElement> paymentElements = new List<XElement>();

                        List<PaymentRow> paymentRows = PaymentManager.GetPaymentRowsByInvoice(customerInvoice.InvoiceId, false, true);

                        if (paymentRows.Count > 0)
                            foreach (var paymentRow in paymentRows)
                            {
                                if (reportParams.SB_HasPaymentDateInterval && paymentRow.PayDate < reportParams.SB_PaymentDateFrom)
                                {
                                    continue;
                                }
                                if (reportParams.SB_HasPaymentDateInterval && paymentRow.PayDate > reportParams.SB_PaymentDateTo.AddHours(23).AddMinutes(59))
                                {
                                    continue;
                                }
                                if (paymentRow.Status == (int)SoePaymentStatus.Cancel)
                                {
                                    continue;
                                }

                                XElement paymentElement = new XElement("Payment");
                                paymentElement.Add(
                                    new XAttribute("id", invoicePaymentXmlId),
                                    new XElement("InvoiceNr", customerInvoice.InvoiceNr),
                                    new XElement("PaymentMethodName", paymentRow.Payment.PaymentMethod.Name),
                                    new XElement("PaymentDate", paymentRow.PayDate.ToShortDateString()),
                                    new XElement("PaymentAmount", paymentRow.Amount));

                                invoicePaymentElements.Add(paymentElement);
                                invoicePaymentXmlId++;
                            }
                        if (invoicePaymentXmlId > 1)
                        {
                            invoiceHeadElement.Add(invoicePaymentElements);
                        }
                        #endregion

                        #region Default element InvoicePayment

                        if (invoicePaymentXmlId == 1)
                        {
                            XElement defInvoiceElement = new XElement("Payments");
                            XElement defpaymentElement = new XElement("Payment",
                                        new XAttribute("id", 1),
                                        new XElement("InvoiceNr", " "),
                                        new XElement("PaymentMethodName", " "),
                                        new XElement("PaymentDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                                        new XElement("PaymentAmount", 0));
                            defInvoiceElement.Add(defpaymentElement);
                            invoiceHeadElement.Add(defInvoiceElement);
                        }
                        //     invoiceHeadElement.Add(new XElement("Payments", String.Empty));

                        #endregion

                        #endregion

                        invoiceHeadElements.Add(invoiceHeadElement);
                        invoiceHeadXmlId++;
                    }

                    repository.SaveHistory();

                    #endregion
                }

                if (invoiceHeadXmlId == 1)
                {
                    var invoiceHeadElement = new XElement("InvoiceHead",
                        new XAttribute("id", 1));

                    invoiceHeadElement.Add(new XElement("InvoiceRow", new XAttribute("id", 1)));
                    invoiceHeadElement.Add(new XElement("PreviousInvoiceHead", new XAttribute("id", 1)));
                    invoiceHeadElement.Add(new XElement("OriginInvoiceHead", new XAttribute("id", 1)));
                    invoiceHeadElement.Add(new XElement("OriginUser", new XAttribute("id", 1)));
                    invoiceHeadElements.Add(invoiceHeadElement);
                }

                #region Set Printed

                if (isInvoiceTemplate && reportResult.ReportTemplateType != SoeReportTemplateType.OriginStatisticsReport)
                    InvoiceManager.SetCustomerInvoiceAsPrinted(entities, customerInvoices);

                #endregion

                #region Set ReportName

                if (customerNameDict.Count > 0)
                {
                    int min = customerNameDict.Keys.Min();
                    int max = customerNameDict.Keys.Max();
                    if (min == max)
                    {
                        reportResult.ReportNamePostfix = min.ToString();
                        reportResult.ReportNamePostfix += " ";
                        reportResult.ReportNamePostfix += customerNameDict[min];
                    }
                    else
                        reportResult.ReportNamePostfix = min.ToString() + "-" + max.ToString();
                }

                #endregion

                #endregion
            }

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = new XElement(CreateBillingReportHeaderLabelsElement(reportResult.ReportTemplateType));
            reportHeaderLabelsElement.Add(CreateAccountIntervalLabelReportHeaderLabelsElement());
            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateBillingInvoiceHeadReportHeaderLabelsElement(pageHeaderLabelsElement, reportResult.ReportTemplateType, doPrintTaxBillText);
            CreateBillingInvoiceRowsReportHeaderLabelsElement(pageHeaderLabelsElement, reportResult.ReportTemplateType);

            #endregion

            #region PreviousInvoices

            string previousInvoicesLabel = "";
            if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOrder))
                previousInvoicesLabel = GetReportText(292, "Tidigare ordrar");
            else
                previousInvoicesLabel = GetReportText(293, "Tidigare fakturor");

            pageHeaderLabelsElement.Add(
                new XElement("PreviousInvoicesLabel", previousInvoicesLabel));

            //Get AccountDims and add AccountDimLabelElements
            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId);
            AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);

            #endregion

            #region Contract labels

            pageHeaderLabelsElement.Add(new XElement("ContractGroupNameLabel", GetReportText(806, "Avtalsgrupp")));
            pageHeaderLabelsElement.Add(new XElement("ContractGroupPeriodTypeLabel", GetReportText(807, "Period")));
            pageHeaderLabelsElement.Add(new XElement("ContractNextYearLabel", GetReportText(808, "Nästa år")));
            pageHeaderLabelsElement.Add(new XElement("ContractNextPeriodLabel", GetReportText(809, "Nästa period")));
            pageHeaderLabelsElement.Add(new XElement("ContractEndYearLabel", GetReportText(810, "Avslutsår")));
            pageHeaderLabelsElement.Add(new XElement("ContractEndPeriodLabel", GetReportText(811, "Avslutsperiod")));
            pageHeaderLabelsElement.Add(new XElement("ContractCategoryNameLabel", GetReportText(815, "Avtalskategori")));

            #endregion

            #region Weight

            //pageHeaderLabelsElement.Add(new XElement("InvoiceRowProductWeightLabel", GetReportText(1047, "Vikt")));
            //pageHeaderLabelsElement.Add(new XElement("InvoiceRowTotalWeightLabel", GetReportText(1048, "Totalvikt")));

            #endregion

            #region Close document

            billingInvoiceElement.Add(reportHeaderLabelsElement);
            billingInvoiceElement.Add(reportHeaderElement);
            billingInvoiceElement.Add(pageHeaderLabelsElement);

            foreach (XElement invoiceHeadElement in invoiceHeadElements)
            {
                billingInvoiceElement.Add(invoiceHeadElement);
            }

            rootElement.Add(billingInvoiceElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.BillingInvoice);

            #endregion
        }

        protected XElement CreateBillingInvoiceRowElement(int invoiceRowXmlId, CreateReportResult es, BillingReportParamsDTO reportParams, CustomerInvoiceRow customerInvoiceRow, Product product, List<AttestState> attestStates, IEnumerable<VatCode> vatCodes, int householdProductId, int household50ProductId, int householdRutProductId, int householdGreen15ProductId, int householdGreen20ProductId, int householdGreen50ProductId, int fixedPriceProductId, int fixedPriceKeepPricesProductId, int rowHasStateToInvoice, List<GetTimeCodeTransactionsForOrderOverview_Result> timeCodeTransactions, bool useProjectTimeBlock, bool productRowDescriptionToUpperCase, string houseHoldDecuctionType = "")
        {
            #region Prereq

            XElement invoiceRowElement = new XElement("InvoiceRow");
            invoiceRowElement.Add(
                new XAttribute("id", invoiceRowXmlId));
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            #endregion

            #region Product
            // Make sure ProductUnit is loaded
            if (!customerInvoiceRow.ProductUnitReference.IsLoaded)
                customerInvoiceRow.ProductUnitReference.Load();
            if (!customerInvoiceRow.CustomerInvoiceReference.IsLoaded)
                customerInvoiceRow.CustomerInvoiceReference.Load();
            if (!customerInvoiceRow.CustomerInvoice.OriginReference.IsLoaded)
                customerInvoiceRow.CustomerInvoice.OriginReference.Load();

            string productName = "";


            int? customerSysLanguageId = (from entry in entitiesReadOnly.Customer
                                          where entry.ActorCustomerId == customerInvoiceRow.CustomerInvoice.ActorId
                                        && entry.SysLanguageId != customerInvoiceRow.CustomerInvoice.Origin.ActorCompanyId
                                          select entry.SysLanguageId).FirstOrDefault();

            if (reportLanguageId == 0 && customerSysLanguageId.HasValue)
                reportLanguageId = customerSysLanguageId.Value;

            if (product != null && reportLanguageId > 0 && StringUtility.IsEqual(customerInvoiceRow.Text, product.Name, true))
            {
                var compTerm = TermManager.GetCompTerm(CompTermsRecordType.ProductName, product.ProductId, reportLanguageId);
                if (compTerm != null && !String.IsNullOrEmpty(compTerm.Name))
                    productName = productRowDescriptionToUpperCase ? compTerm.Name.ToUpper() : compTerm.Name;
            }

            if (String.IsNullOrEmpty(productName) && !String.IsNullOrEmpty(customerInvoiceRow.Text))
                productName = customerInvoiceRow.Text;

            if (String.IsNullOrEmpty(productName) && product != null)
                productName = product.Name;

            // Product unit
            string productUnitCode = string.Empty, productUnitName = string.Empty;
            if (customerInvoiceRow.ProductUnit != null)
            {
                productUnitCode = customerInvoiceRow.ProductUnit.Code;
                productUnitName = customerInvoiceRow.ProductUnit.Name;
            }

            if (reportLanguageId > 0 && customerInvoiceRow.ProductUnitId.HasValue)
            {
                var compTerm = TermManager.GetCompTerm(CompTermsRecordType.ProductUnitName, customerInvoiceRow.ProductUnitId.Value, reportLanguageId);
                if (compTerm != null && !String.IsNullOrEmpty(compTerm.Name))
                    productUnitName = productUnitCode = compTerm.Name;
            }

            int productVatType = -1;
            if (product != null)
                productVatType = ((InvoiceProduct)product).VatType;

            // HouseHoldDeduction
            int isHouseholdBaseProduct = 0;

            if (customerInvoiceRow.ProductId != null && (customerInvoiceRow.ProductId.Value == householdProductId || customerInvoiceRow.ProductId.Value == household50ProductId || customerInvoiceRow.ProductId.Value == householdRutProductId || customerInvoiceRow.ProductId.Value == householdGreen15ProductId || customerInvoiceRow.ProductId.Value == householdGreen20ProductId || customerInvoiceRow.ProductId.Value == householdGreen50ProductId))
                isHouseholdBaseProduct = 1;

            //Find vatcode
            bool isMixedVat = false;
            VatCode rowVatCode = customerInvoiceRow.VatCodeId.HasValue ? vatCodes.FirstOrDefault(v => v.VatCodeId == (int)customerInvoiceRow.VatCodeId) : null;

            //Check if strange VAT Percent
            if (!vatCodes.IsNullOrEmpty())
            {
                foreach (VatCode vatCode in vatCodes)
                {
                    isMixedVat = true;

                    if (vatCode.Percent == customerInvoiceRow.VatRate || customerInvoiceRow.VatRate == 0)
                    {
                        isMixedVat = false;
                        break;
                    }

                }
            }

            if (vatCodes.IsNullOrEmpty())
                isMixedVat = false;

            // Garantee
            int isGuarantee = 0;
            int productGuaranteeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGuarantee, 0, es.ActorCompanyId, 0);
            if (productGuaranteeId != 0 && customerInvoiceRow.ProductId == productGuaranteeId)
                isGuarantee = 1;

            #region Employee calculated cost

            bool useCalculatedCost = false;
            bool recalculateMarginalIncome = false;
            decimal minutes = 0;
            decimal employeeCost = 0;
            decimal calculatedCost = 0;
            decimal calculatedQuantity = 0;
            if (customerInvoiceRow.IsTimeProjectRow)
            {
                if (es.ReportTemplateType == SoeReportTemplateType.BillingOrderOverview && timeCodeTransactions.Count > 0)
                {
                    recalculateMarginalIncome = true;
                    foreach (var timeCodeTrans in timeCodeTransactions.Where(t => t.CustomerInvoiceRowId == customerInvoiceRow.CustomerInvoiceRowId))
                    {
                        if (useProjectTimeBlock)
                        {
                            decimal quantity = Convert.ToDecimal(CalendarUtility.TimeSpanToMinutes(timeCodeTrans.Stop, timeCodeTrans.Start)) / 60;

                            if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                quantity = decimal.Negate(quantity);

                            decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? EmployeeManager.GetEmployeeCalculatedCost(new Employee() { EmployeeId = timeCodeTrans.EmployeeId, ActorCompanyId = base.ActorCompanyId }, timeCodeTrans.TransactionDate.Date, customerInvoiceRow.CustomerInvoice?.ProjectId) : customerInvoiceRow.PurchasePrice;
                            decimal cost = quantity * price;

                            calculatedCost += cost;
                            calculatedQuantity += quantity;
                        }
                        else
                        {
                            decimal quantity = (timeCodeTrans.TransactionQuantity.HasValue && timeCodeTrans.TransactionQuantity.Value > 0 ? timeCodeTrans.TransactionQuantity.Value / 60 : 0);

                            if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                quantity = decimal.Negate(quantity);

                            decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? EmployeeManager.GetEmployeeCalculatedCost(new Employee() { EmployeeId = timeCodeTrans.EmployeeId, ActorCompanyId = base.ActorCompanyId }, timeCodeTrans.TransactionDate.Date, customerInvoiceRow.CustomerInvoice?.ProjectId) : customerInvoiceRow.PurchasePrice;
                            decimal cost = quantity * price;

                            calculatedCost += cost;
                            calculatedQuantity += quantity;
                        }
                    }
                }
                else
                {
                    if (es.GetDetailedInformation)
                    {
                        InvoiceProduct invoiceProduct = product as InvoiceProduct;
                        if (invoiceProduct == null || invoiceProduct.UseCalculatedCost == true)
                        {
                            useCalculatedCost = true;
                            List<TimeInvoiceTransaction> transactions = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(customerInvoiceRow.CustomerInvoiceRowId);

                            foreach (TimeInvoiceTransaction trans in transactions)
                            {
                                if (!trans.EmployeeReference.IsLoaded)
                                    trans.EmployeeReference.Load();

                                var employeeCalculatedCost = EmployeeManager.GetEmployeeCalculatedCost(entitiesReadOnly, trans.Employee, trans.TimeBlockDate.Date, customerInvoiceRow.CustomerInvoice?.ProjectId);
                                if (employeeCalculatedCost > 0)
                                {
                                    minutes += trans.Quantity;
                                    employeeCost += (trans.Quantity * employeeCalculatedCost);
                                }
                                else
                                {
                                    useCalculatedCost = false;
                                }
                            }

                            if (useCalculatedCost)
                                calculatedCost = minutes == 0 ? 0 : (employeeCost / minutes);
                        }
                    }
                }
            }


            decimal marginalIncome = customerInvoiceRow.MarginalIncome;
            decimal marginalIncomeRatio = customerInvoiceRow.MarginalIncomeRatio.HasValue ? customerInvoiceRow.MarginalIncomeRatio.Value : 0;
            if (recalculateMarginalIncome)
            {
                marginalIncome = customerInvoiceRow.SumAmount - (calculatedCost);
                marginalIncomeRatio = (customerInvoiceRow.SumAmount != 0 ? marginalIncome / customerInvoiceRow.SumAmount : 1) * 100;
                if (marginalIncome < 0 && marginalIncomeRatio > 0)
                    marginalIncomeRatio *= -1;
            }

            #endregion

            invoiceRowElement.Add(
                new XElement("InvoiceRowProductNumber", product != null ? product.Number : ""),
                new XElement("InvoiceRowProductName", productName),
                //new XElement("InvoiceRowProductName", product != null ? product.Name : customerInvoiceRow.Text),
                //new XElement("InvoiceRowProductDescription", product != null ? product.Description : ""),
                // 2012-11-23 Håkan: Product description replaced by ShowDescriptionAsTextRow setting on product
                new XElement("InvoiceRowProductDescription", ""),
                new XElement("InvoiceRowProductUnitCode", productUnitCode),
                new XElement("InvoiceRowProductUnitName", productUnitName));

            #endregion

            #region CustomerInvoiceRow


            invoiceRowElement.Add(
                new XElement("InvoiceRowQuantity", recalculateMarginalIncome ? calculatedQuantity : (customerInvoiceRow.Quantity.HasValue ? customerInvoiceRow.Quantity.Value : 0)),
                new XElement("InvoiceRowAmount", customerInvoiceRow.AmountCurrency),
                new XElement("InvoiceRowAmountBase", customerInvoiceRow.Amount),
                new XElement("InvoiceRowVatAmount", customerInvoiceRow.VatAmountCurrency),
                new XElement("InvoiceRowVatAmountBase", customerInvoiceRow.VatAmount),
                new XElement("InvoiceRowSumAmount", customerInvoiceRow.SumAmountCurrency),
                new XElement("InvoiceRowSumAmountCurrency", customerInvoiceRow.SumAmountCurrency), //only to support backwards compability
                new XElement("InvoiceRowSumAmountBase", customerInvoiceRow.SumAmount),
                new XElement("InvoiceRowDiscountPercent", customerInvoiceRow.DiscountPercent),
                new XElement("InvoiceRowDiscountAmount", customerInvoiceRow.DiscountAmountCurrency),
                new XElement("InvoiceRowDiscountAmountBase", customerInvoiceRow.DiscountAmount),
                new XElement("InvoiceRowDiscount2Percent", customerInvoiceRow.Discount2Percent),
                new XElement("InvoiceRowDiscount2Amount", customerInvoiceRow.Discount2AmountCurrency),
                new XElement("InvoiceRowDiscount2AmountBase", customerInvoiceRow.Discount2Amount),
                new XElement("InvoiceRowText", customerInvoiceRow.Text),
                new XElement("InvoiceRowType", customerInvoiceRow.Type),
                new XElement("InvoiceRowVatRate", customerInvoiceRow.VatRate),
                new XElement("InvoiceRowVatCodeName", rowVatCode != null ? rowVatCode.Name : string.Empty),
                new XElement("InvoiceRowVatCodePercent", rowVatCode != null ? rowVatCode.Percent : 0),
                new XElement("InvoiceRowVatCodeCode", rowVatCode != null ? rowVatCode.Code : string.Empty),
                new XElement("InvoiceRowisStockRow", customerInvoiceRow.IsStockRow != null && customerInvoiceRow.IsStockRow == true),
                new XElement("InvoiceRowPurchasePrice", recalculateMarginalIncome ? (calculatedCost / calculatedQuantity) : (useCalculatedCost ? calculatedCost : customerInvoiceRow.PurchasePrice)),
                new XElement("InvoiceRowMarginalIncome", marginalIncome),
                new XElement("InvoiceRowMarginalIncomeRatio", marginalIncomeRatio),
                new XElement("isInvoicedRow", rowHasStateToInvoice),
                new XElement("InvoiceRowInvoiceQuantity", customerInvoiceRow.InvoiceQuantity.HasValue ? customerInvoiceRow.InvoiceQuantity.Value : 0),
                new XElement("InvoiceRowIsHouseholdProduct", isHouseholdBaseProduct),
                new XElement("InvoiceRowIsGuarantee", isGuarantee),
                new XElement("InvoiceRowProductVatType", productVatType),
                new XElement("InvoiceRowSysWholesellerName", customerInvoiceRow.SysWholesellerName),
                 new XElement("InvoiceRowRowNumber", customerInvoiceRow.RowNr),
                new XElement("InvoiceRowHouseholdDeductionType", houseHoldDecuctionType)
                );

            #endregion

            #region Stock

            //Stock
            if (customerInvoiceRow.IsStockRow.HasValue && customerInvoiceRow.IsStockRow.Value && customerInvoiceRow.StockId != null)
            {
                invoiceRowElement.Add(
               new XElement("InvoiceRowStockCode", customerInvoiceRow.Stock.Code != null ? customerInvoiceRow.Stock.Code : ""),
               new XElement("InvoiceRowStockName", customerInvoiceRow.Stock.Name != null ? customerInvoiceRow.Stock.Name : ""));
            }
            else
            {
                invoiceRowElement.Add(
               new XElement("InvoiceRowStockCode", ""),
               new XElement("InvoiceRowStockName", ""));
            }
            //Shelf
            if (customerInvoiceRow.IsStockRow.HasValue && customerInvoiceRow.IsStockRow.Value && customerInvoiceRow.StockId != null)
            {
                if (!customerInvoiceRow.Stock.StockShelf.IsLoaded)
                    customerInvoiceRow.Stock.StockShelf.Load();

                var shelfCode = customerInvoiceRow.Stock.StockShelf.FirstOrDefault();
                if (shelfCode != null)
                {
                    invoiceRowElement.Add(
                   new XElement("InvoiceRowShelfCode", shelfCode.Code),
                   new XElement("InvoiceRowShelfName", shelfCode.Name));
                }
                else
                {
                    invoiceRowElement.Add(
                   new XElement("InvoiceRowShelfCode", ""),
                   new XElement("InvoiceRowShelfName", ""));
                }
            }
            else
            {
                invoiceRowElement.Add(
               new XElement("InvoiceRowShelfCode", ""),
               new XElement("InvoiceRowShelfName", ""));
            }



            #endregion

            #region ProductGroup

            //ProductGroup
            ProductGroup productGr = null;
            if (product != null && product.ProductGroupId != null)
                productGr = ProductGroupManager.GetProductGroup(product.ProductGroupId.Value);

            invoiceRowElement.Add(
                new XElement("InvoiceRowProductGroupCode", productGr != null ? productGr.Code : ""),
                new XElement("InvoiceRowProductGroupName", productGr != null ? productGr.Name : ""));

            #endregion

            string productCategoryCode = "", productCategoryName = "", productCategoryParentCode = "", productCategoryParentName = "";

            #region ProductCategory

            Category category = null;
            if (customerInvoiceRow.ProductId != null && customerInvoiceRow.CustomerInvoice != null && customerInvoiceRow.CustomerInvoice.Origin != null)
                category = CategoryManager.GetCategory(SoeCategoryType.Product, SoeCategoryRecordEntity.Product, (int)customerInvoiceRow.ProductId, customerInvoiceRow.CustomerInvoice.Origin.ActorCompanyId, onlyDefaultCategory: false);

            if (category != null)
            {
                productCategoryCode = category.Code;
                productCategoryName = category.Name;
                productCategoryParentCode = (from c in entitiesReadOnly.Category where c.CategoryId == category.ParentId select c.Code).FirstOrDefault();
                productCategoryParentName = (from c in entitiesReadOnly.Category where c.CategoryId == category.ParentId select c.Name).FirstOrDefault();
            }

            invoiceRowElement.Add(
                new XElement("InvoiceRowProductCategoryCode", productCategoryCode),
                new XElement("InvoiceRowProductCategoryName", productCategoryName),
                new XElement("InvoiceRowProductCategoryParentCode", productCategoryParentCode),
                new XElement("InvoiceRowProductCategoryParentName", productCategoryParentName));

            #endregion

            #region Lift product

            if (product != null && ((InvoiceProduct)product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift)
            {
                invoiceRowElement.Add(
                     new XElement("InvoiceRowProductCalculationType", ((InvoiceProduct)product).CalculationType),
                     new XElement("InvoiceRowLiftDate", customerInvoiceRow.Date.HasValue ? customerInvoiceRow.Date.Value : CalendarUtility.DATETIME_DEFAULT),
                     new XElement("InvoiceRowDate", CalendarUtility.DATETIME_DEFAULT),
                     new XElement("InvoiceRowDeliveryDateText", customerInvoiceRow.DeliveryDateText != null ? customerInvoiceRow.DeliveryDateText : String.Empty),
                     new XElement("isLiftRow", 1));
            }
            else
            {
                if (product != null && ((InvoiceProduct)product).CalculationType != 0)
                    invoiceRowElement.Add(new XElement("InvoiceRowProductCalculationType", ((InvoiceProduct)product).CalculationType));
                else if (fixedPriceProductId == customerInvoiceRow.ProductId || fixedPriceKeepPricesProductId == customerInvoiceRow.ProductId)
                    invoiceRowElement.Add(new XElement("InvoiceRowProductCalculationType", (int)TermGroup_InvoiceProductCalculationType.FixedPrice));
                else
                    invoiceRowElement.Add(new XElement("InvoiceRowProductCalculationType", 0));

                invoiceRowElement.Add(
                new XElement("InvoiceRowLiftDate", CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceRowDate", customerInvoiceRow.Date.HasValue ? customerInvoiceRow.Date.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceRowDeliveryDateText", customerInvoiceRow.DeliveryDateText != null ? customerInvoiceRow.DeliveryDateText : String.Empty),
                new XElement("isLiftRow", 0));

            }

            #endregion

            #region AttestState

            AttestState attestState = null;
            if (customerInvoiceRow.AttestStateId.HasValue)
                attestState = attestStates.FirstOrDefault(o => o.AttestStateId == customerInvoiceRow.AttestStateId.Value);

            invoiceRowElement.Add(
                new XElement("AttestState", attestState?.Name ?? string.Empty));

            invoiceRowElement.Add(
                new XElement("IsClosedRow", attestState != null && attestState.Closed ? 1 : 0));


            #endregion

            #region Created and modified

            invoiceRowElement.Add(
                new XElement("isEDI", customerInvoiceRow.EdiEntryId.HasValue ? 1 : 0),
                new XElement("isTimeProject", customerInvoiceRow.IsTimeProjectRow.ToInt()),
                new XElement("isMixedVat", isMixedVat.ToInt()),
                new XElement("Created", customerInvoiceRow.Created.HasValue ? customerInvoiceRow.Created.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("CreatedBy", customerInvoiceRow.CreatedBy != null ? customerInvoiceRow.CreatedBy : string.Empty),
                new XElement("Modified", customerInvoiceRow.Modified.HasValue ? customerInvoiceRow.Modified.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("ModifiedBy", customerInvoiceRow.ModifiedBy != null ? customerInvoiceRow.ModifiedBy : string.Empty));

            #endregion

            #region Weight

            if (product != null && ((InvoiceProduct)product).Weight.HasValue)
            {
                invoiceRowElement.Add(
                new XElement("InvoiceRowProductWeight", ((InvoiceProduct)product).Weight.Value),
                new XElement("InvoiceRowTotalWeight", ((InvoiceProduct)product).Weight.Value * customerInvoiceRow.Quantity));
            }
            else
            {
                invoiceRowElement.Add(
                new XElement("InvoiceRowProductWeight", 0),
                new XElement("InvoiceRowTotalWeight", 0));
            }

            #endregion

            return invoiceRowElement;
        }

        public XDocument CreateBillingStatisticsData(CreateReportResult reportResult)
        {
            if (reportResult.ReportTemplateType != SoeReportTemplateType.BillingStatisticsReport)
                return null;

            #region Prereq

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            if (reportResult == null)
                return null;

            bool isBillingContractTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingContract);
            bool isBillingOrderTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOrder);
            bool isBillingOfferTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOffer);
            bool isBillingInvoiceTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoice);
            bool isBillingInvoiceInterestTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoiceInterest);
            bool isBillingInvoiceReminderTemplate = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoiceReminder);
            bool isInvoiceTemplate = isBillingInvoiceTemplate || isBillingInvoiceInterestTemplate || isBillingInvoiceReminderTemplate;
            bool isOriginStatistics = reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.OriginStatisticsReport);

            //permissions
            bool showSalesPricePermission = FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowSalesPrice, Permission.Readonly, base.RoleId, base.ActorCompanyId);

            //Get settings
            bool doPrintTaxBillText = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingPrintTaxBillText, 0, reportResult.ActorCompanyId, 0);
            //Disregard copies for interest and reminder, until separate setting exists for them
            if (isBillingInvoiceInterestTemplate || isBillingInvoiceReminderTemplate)
                reportParams.SB_DisableInvoiceCopies = true;

            int nbrOfCopies;
            if (reportParams.SB_DisableInvoiceCopies)
                nbrOfCopies = 0;
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingContract))
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfContractCopies, 0, reportResult.ActorCompanyId, 0);
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOffer))
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfOfferCopies, 0, reportResult.ActorCompanyId, 0);
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOrder))
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfOrderCopies, 0, reportResult.ActorCompanyId, 0);
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoice))
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfCopies, 0, reportResult.ActorCompanyId, 0);
            else if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.OriginStatisticsReport))
                nbrOfCopies = 0;
            else
                nbrOfCopies = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingNbrOfCopies, 0, reportResult.ActorCompanyId, 0);

            // Never send email copys
            if (nbrOfCopies > 0 && reportResult.EmailTemplateId != null && reportResult.EmailTemplateId != 0)
            {
                nbrOfCopies = 0;
            }


            //AttestLevels
            string attestLevelInitial = string.Empty;
            string attestLevelToInvoice = string.Empty;
            string attestLevelMobileReady = string.Empty;
            string attestLevelOfferToOrder = string.Empty;

            if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingOrder || reportResult.ReportTemplateType == SoeReportTemplateType.OriginStatisticsReport)
            {
                var initialAttestState = AttestManager.GetInitialAttestState(reportResult.ActorCompanyId, TermGroup_AttestEntity.Order);
                attestLevelInitial = initialAttestState != null ? initialAttestState.Name : string.Empty;

                int defaultStatusTransferredOrderToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, reportResult.ActorCompanyId, 0);
                int defaultStatusTransferredOrderReadyMobile = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusOrderReadyMobile, 0, reportResult.ActorCompanyId, 0);

                var orderToInvoiceAttestState = AttestManager.GetAttestState(defaultStatusTransferredOrderToInvoice);
                attestLevelToInvoice = orderToInvoiceAttestState != null ? orderToInvoiceAttestState.Name : string.Empty;

                var orderReadyMobileAttestState = AttestManager.GetAttestState(defaultStatusTransferredOrderReadyMobile);
                attestLevelMobileReady = orderReadyMobileAttestState != null ? orderReadyMobileAttestState.Name : string.Empty;
            }

            if (reportResult.ReportTemplateType == SoeReportTemplateType.BillingOffer)
            {
                var initialAttestState = AttestManager.GetInitialAttestState(reportResult.ActorCompanyId, TermGroup_AttestEntity.Offer);
                attestLevelInitial = initialAttestState != null ? initialAttestState.Name : string.Empty;

                int defaultStatusTransferredOfferToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToInvoice, 0, reportResult.ActorCompanyId, 0);
                int defaultStatusTransferredOfferToOrder = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToOrder, 0, reportResult.ActorCompanyId, 0);

                var offerToInvoiceAttestState = AttestManager.GetAttestState(defaultStatusTransferredOfferToInvoice);
                attestLevelToInvoice = offerToInvoiceAttestState != null ? offerToInvoiceAttestState.Name : string.Empty;

                var offerToOrderAttestState = AttestManager.GetAttestState(defaultStatusTransferredOfferToOrder);
                attestLevelOfferToOrder = offerToOrderAttestState != null ? offerToOrderAttestState.Name : string.Empty;
            }

            List<TermGroup_AttestEntity> entitys = new List<TermGroup_AttestEntity>()
            {
                TermGroup_AttestEntity.Order,
                TermGroup_AttestEntity.Offer,
            };
            List<AttestState> attestStates = AttestManager.GetAttestStates(reportResult.ActorCompanyId, entitys, SoeModule.Billing);

            List<TimeCode> timeCodes = TimeCodeManager.GetTimeCodes(reportResult.ActorCompanyId);
            List<AccountDimDTO> accountDimInternalsDTOs = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId).ToDTOs();

            //Language
            reportLanguageId = reportParams.SB_ReportLanguageId;
            if (reportParams.SB_ReportLanguageId == 0)
            {
                switch (Company.SysCountryId)
                {
                    case (int)TermGroup_Country.SE:
                        reportLanguageId = (int)TermGroup_Languages.Swedish;
                        break;
                    case (int)TermGroup_Country.FI:
                        reportLanguageId = (int)TermGroup_Languages.Finnish;
                        break;
                    case (int)TermGroup_Country.GB:
                        reportLanguageId = (int)TermGroup_Languages.English;
                        break;
                    case (int)TermGroup_Country.NO:
                        reportLanguageId = (int)TermGroup_Languages.Norwegian;
                        break;
                    case (int)TermGroup_Country.DK:
                        reportLanguageId = (int)TermGroup_Languages.Danish;
                        break;
                }
            }

            if (reportLanguageId == 0)
                reportLanguageId = GetLangId();

            SetLanguage(LanguageManager.GetSysLanguageCode(reportLanguageId));

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "BillingStatistics");

            //VoucherList
            XElement BillingStatistics = new XElement("BillingStatistic");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateBillingReportHeaderElement(reportResult, reportParams);
            reportHeaderElement.Add(CreateAccountIntervalElement(reportResult, reportParams, accountDimInternalsDTOs));
            reportHeaderElement.Add(new XElement("AttestLevelInitial", attestLevelInitial));
            reportHeaderElement.Add(new XElement("AttestLevelInvoiced", attestLevelToInvoice));
            reportHeaderElement.Add(new XElement("AttestLevelMobileReady", attestLevelMobileReady));
            reportHeaderElement.Add(new XElement("AttestLevelOfferToOrder", attestLevelOfferToOrder));

            #region Sort and Specials

            reportHeaderElement.Add(new XElement("SortByLevel1", reportParams.SortByLevel1));
            reportHeaderElement.Add(new XElement("SortByLevel2", reportParams.SortByLevel2));
            reportHeaderElement.Add(new XElement("SortByLevel3", reportParams.SortByLevel3));
            reportHeaderElement.Add(new XElement("SortByLevel4", reportParams.SortByLevel4));
            reportHeaderElement.Add(new XElement("IsSortAscending", reportParams.IsSortAscending));
            reportHeaderElement.Add(new XElement("GroupByLevel1", reportParams.GroupByLevel1));
            reportHeaderElement.Add(new XElement("GroupByLevel2", reportParams.GroupByLevel2));
            reportHeaderElement.Add(new XElement("GroupByLevel3", reportParams.GroupByLevel3));
            reportHeaderElement.Add(new XElement("GroupByLevel4", reportParams.GroupByLevel4));
            reportHeaderElement.Add(new XElement("Special", reportParams.Special));
            reportHeaderElement.Add(new XElement("HidePrice", !showSalesPricePermission));
            reportHeaderElement.Add(new XElement("PeriodFrom", reportParams.SB_PeriodFrom));
            reportHeaderElement.Add(new XElement("PeriodTo", reportParams.SB_PeriodTo));

            #endregion

            #endregion

            //           List<XElement> billingStatisticsElements = new XElement("BillingStatisticsElements");
            XElement billingStatisticsElements = new XElement("BillingStatisticsElements");

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if ((!String.IsNullOrEmpty(reportParams.SB_ProjectNrFrom)) && (String.IsNullOrEmpty(reportParams.SB_ProductNrFrom)))
                {
                    reportParams.SB_ProductNrFrom = reportParams.SB_ProjectNrFrom + 'B';
                }
                if ((!String.IsNullOrEmpty(reportParams.SB_ProjectNrTo)) && (String.IsNullOrEmpty(reportParams.SB_ProductNrTo)))
                {
                    reportParams.SB_ProductNrTo = reportParams.SB_ProjectNrTo + 'R';
                }

                reportParams.SB_PeriodFrom = reportParams.DateFrom.Date.ToString();
                reportParams.SB_PeriodTo = reportParams.DateTo.Date.ToString();
                reportParams.DateFrom = DateTime.MinValue;

                //Get AccountDims
                AccountDim accountDimStds = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId);

                List<AccountStd> accountStdsInInterval = new List<AccountStd>();
                List<AccountInternal> accountInternalsInInterval = new List<AccountInternal>();
                Dictionary<int, string> customerNameDict = new Dictionary<int, string>();

                var accountDims = AccountManager.GetAccountDimsByCompany(reportResult.ActorCompanyId);
                var projAccountDim = accountDims.FirstOrDefault(x => x.SysSieDimNr == 6);

                if ((!String.IsNullOrEmpty(reportParams.SB_ProjectNrTo)))
                {
                    if (reportParams.SA_AccountIntervals == null)
                    {
                        reportParams.SA_HasAccountInterval = true;
                        reportParams.SA_AccountIntervals = new List<AccountIntervalDTO>();
                    }
                    else
                    {
                        // Cleaning out when there are empty AccountNrFrom and AccountNrTo fields.
                        reportParams.SA_AccountIntervals.Clear();
                    }

                    reportParams.SA_AccountIntervals.Add(
                    new AccountIntervalDTO()
                    {
                        AccountDimId = projAccountDim.AccountDimId,
                        AccountNrFrom = reportParams.SB_ProjectNrFrom,
                        AccountNrTo = reportParams.SB_ProjectNrTo,
                    }
                    );
                }

                bool validSelection = AccountManager.GetAccountsInInterval(entities, reportResult, reportParams, accountDimStds, false, ref accountStdsInInterval, ref accountInternalsInInterval);
                var accountInternalIds = accountInternalsInInterval.Select(a => a.AccountId).ToList();


                int statRowXmlId = 1;
                int year;
                int month;
                int week;
                string yearMonthStr;
                decimal yearMonth;
                decimal yearWeek;
                // Increase the command timeout
                entities.CommandTimeout = 180; // 3 minutes

                //Get invoice rows
                var invoiceRowsGrouped = entities.GetSalesStatisticsFromAccounts(reportResult.ActorCompanyId, reportParams.DateFrom, reportParams.DateTo, reportParams.SB_ProductNrFrom, reportParams.SB_ProductNrTo).GroupBy(r => r.CustomerInvoiceRowId);

                #endregion

                //       #endregion

                #region Content

                //Fixed price setting (fixedprice type 1)
                int fixedPriceProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductFlatPrice, 0, reportResult.ActorCompanyId, 0);
                //Fixed price keep prices setting (fixedprice type 2)
                int fixedPriceKeepPricesProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductFlatPriceKeepPrices, 0, reportResult.ActorCompanyId, 0);
                //Guarantee setting
                int productGuaranteeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGuarantee, 0, reportResult.ActorCompanyId, 0);

                foreach (var groupedRow in invoiceRowsGrouped)
                {
                    // Handle data for common 
                    var mainRow = groupedRow.FirstOrDefault();

                    if (!string.IsNullOrEmpty(reportParams.SB_ProductNrFrom) && (!StringUtility.IsInInterval(mainRow.ProductNumber, reportParams.SB_ProductNrFrom, reportParams.SB_ProductNrTo)))
                    {
                        continue;
                    }

                    week = CalendarUtility.GetWeekNr(mainRow.InvoiceDate.Value);
                    year = mainRow.InvoiceDate.ToValueOrDefault().Year;
                    month = mainRow.InvoiceDate.ToValueOrDefault().Month;
                    yearMonthStr = (year.ToString() + month.ToString());
                    yearMonth = NumberUtility.ToDecimal(yearMonthStr);
                    yearMonthStr = (year.ToString() + week.ToString());
                    yearWeek = NumberUtility.ToDecimal(yearMonthStr);
                    string extraFieldName1 = "";
                    decimal extraFieldValue1 = 0;
                    string extraFieldName2 = "";
                    decimal extraFieldValue2 = 0;
                    string extraFieldName3 = "";
                    decimal extraFieldValue3 = 0;

                    foreach (var row in groupedRow)
                    {
                        //var accountKto = AccountManager.GetAccount(reportResult.ActorCompanyId, mainRow.BaseAccountId, onlyActive: false);
                        var account = accountStdsInInterval.FirstOrDefault(a => a.Account.AccountId == mainRow.BaseAccountId);

                        Account accountKto = null;
                        if (account != null)
                        {
                            accountKto = account.Account;
                        }
                        Account accountInternal = null;
                        AccountInternal aInternal = accountInternalsInInterval.FirstOrDefault(a => a.AccountId == row.InternalAccountId.GetValueOrDefault());
                        // Get info of internal accounts
                        if (aInternal != null)
                        {
                            accountInternal = aInternal.Account;
                            //accountInternal = AccountManager.GetAccount(es.ActorCompanyId, row.InternalAccountId.GetValueOrDefault(), onlyActive: false, loadAccountDim: true);
                        }
                        else
                            continue;

                        //Account Internals
                        bool accountsValid = false;

                        if (accountInternalsInInterval.Count != 0)
                        {
                            if (accountInternal == null)
                            {
                                accountsValid = false;
                            }
                            else
                            {
                                //var accountInternalIds = accountInternalsInInterval.Select(a => a.AccountId).ToList();
                                if (accountInternal != null && accountInternal.AccountId != 0 && accountInternalIds.Contains(accountInternal.AccountId))
                                    accountsValid = true;
                            }
                        }
                        else
                        {
                            accountsValid = true;
                        }

                        if (!accountsValid)
                            continue;

                        AccountDim dim = null;
                        if (accountInternal != null)
                        {
                            //dim = AccountManager.GetAccountDim(accountInternal.AccountDim.AccountDimId, reportResult.ActorCompanyId);
                            dim = accountDims.FirstOrDefault(d => d.AccountDimId == accountInternal.AccountDimId);
                        }
                        else
                        {
                            dim = null;
                        }

                        // Extra fields
                        if (!string.IsNullOrEmpty(mainRow.ExtraFields))
                        {
                            var extraFields = mainRow.ExtraFields.Split(';');

                            foreach (var extraFieldStr in extraFields)
                            {
                                // Should consist of two strings, name and value, after split on ':'
                                var nameAndValue = extraFieldStr.Split(':');

                                foreach (var nValStr in nameAndValue)
                                {
                                    if (string.IsNullOrEmpty(extraFieldName1))
                                    {
                                        extraFieldName1 = nValStr;
                                        extraFieldValue1 = 1;
                                        continue;
                                    }
                                    else if (extraFieldName1 == nValStr)
                                    {
                                        break;
                                    }

                                    if (extraFieldValue1 == 1)
                                    {
                                        extraFieldValue1 = NumberUtility.ToDecimalWithComma(nValStr, 2);
                                        continue;
                                    }
                                    if (string.IsNullOrEmpty(extraFieldName2))
                                    {
                                        extraFieldName2 = nValStr;
                                        extraFieldValue2 = 1;
                                        continue;
                                    }
                                    else if (extraFieldName1 == nValStr)
                                    {
                                        break;
                                    }

                                    if (extraFieldValue2 == 1)
                                    {
                                        extraFieldValue2 = NumberUtility.ToDecimalWithComma(nValStr, 2);
                                        continue;
                                    }
                                    if (string.IsNullOrEmpty(extraFieldName3))
                                    {
                                        extraFieldName3 = nValStr;
                                        extraFieldValue3 = 1;
                                        continue;
                                    }
                                    else if (extraFieldName1 == nValStr)
                                    {
                                        break;
                                    }

                                    if (extraFieldValue3 == 1)
                                    {
                                        extraFieldValue3 = NumberUtility.ToDecimalWithComma(nValStr, 2);
                                        continue;
                                    }
                                }
                            }
                        }
                        XElement StatRowElement = new XElement("StatRow");
                        Decimal rowQantity;
                        rowQantity = (decimal)mainRow.Quantity;
                        if (mainRow.BillingType == 2)
                        {
                            var originalInvoices = InvoiceManager.GetInvoiceTraceViews(mainRow.InvoiceId, 0);
                            foreach (var originaInvoice in originalInvoices)
                            {
                                if (originaInvoice.IsInvoice)
                                {
                                    rowQantity = (decimal)mainRow.Quantity * -1;
                                }
                            }
                        }

                        StatRowElement.Add(
                            new XAttribute("id", statRowXmlId)
                            );

                        StatRowElement.Add(
                             new XElement("CustomerName", mainRow.CustomerName),
                             new XElement("CustomerNr", mainRow.CustomerNr),
                             new XElement("AccountName", accountKto != null ? accountKto.Name : ""),
                             new XElement("AccountNr", accountKto != null ? accountKto.AccountNr : ""),
                             new XElement("AccountInternalName", accountInternal != null ? accountInternal.Name : ""),
                             new XElement("AccountInternalNr", accountInternal != null ? accountInternal.AccountNr : ""),
                             new XElement("AccountDimName", accountInternal != null ? dim.Name : ""),
                             new XElement("AccountDimNr", accountInternal != null ? dim.AccountDimNr.ToString() : ""),
                             new XElement("InvoiceNr", mainRow.InvoiceNr),
                             new XElement("InvoiceRowNr", mainRow.RowNr),
                             new XElement("InvoiceDate", mainRow.InvoiceDate.ToString()),
                             new XElement("VoucherDate", mainRow.VoucherDate.ToString()),
                             new XElement("ProductNumber", mainRow.ProductNumber),
                             new XElement("ProductName", mainRow.ProductName),
                             //   new XElement("RowQuantity", mainRow.Quantity ?? 0),
                             new XElement("RowQuantity", rowQantity),
                             new XElement("RowAmount", mainRow.Amount),
                             new XElement("Year", year),
                             new XElement("YearMonth", yearMonth),
                             new XElement("YearWeek", yearWeek),
                             new XElement("ExtraField1Name", extraFieldName1),
                             new XElement("ExtraField1Value", extraFieldValue1),
                             new XElement("ExtraField2Name", extraFieldName2),
                             new XElement("ExtraField2Value", extraFieldValue2),
                             new XElement("ExtraField3Name", extraFieldName3),
                             new XElement("ExtraField3Value", extraFieldValue3)
                             );
                        billingStatisticsElements.Add(StatRowElement);
                        statRowXmlId++;
                    }

                }
                #region Default element
                if (statRowXmlId == 1)
                {
                    XElement StatRowElement = new XElement("StatRow");
                    StatRowElement.Add(
                        new XAttribute("id", statRowXmlId)
                        );

                    StatRowElement.Add(
                         new XElement("CustomerName", ""),
                         new XElement("CustomerNr", ""),
                         new XElement("AccountName", ""),
                         new XElement("AccountNr", ""),
                         new XElement("AccountInternalName", ""),
                         new XElement("AccountInternalNr", ""),
                         new XElement("AccountDimName", ""),
                         new XElement("AccountDimNr", ""),
                         new XElement("InvoiceNr", ""),
                         new XElement("InvoiceRowNr", ""),
                         new XElement("InvoiceDate", ""),
                         new XElement("VoucherDate", ""),
                         new XElement("ProductNumber", ""),
                         new XElement("ProductName", ""),
                         new XElement("RowQuantity", 0),
                         new XElement("RowAmount", 0),
                         new XElement("ExtraField1Name", ""),
                         new XElement("ExtraField1Value", 0),
                         new XElement("ExtraField2Name", ""),
                         new XElement("ExtraField2Value", 0),
                         new XElement("ExtraField3Name", ""),
                         new XElement("ExtraField3Value", 0)
                         );
                    billingStatisticsElements.Add(StatRowElement);
                }

                #endregion

                #endregion
            }

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = new XElement(CreateBillingReportHeaderLabelsElement(reportResult.ReportTemplateType));
            reportHeaderLabelsElement.Add(CreateAccountIntervalLabelReportHeaderLabelsElement());
            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateBillingInvoiceHeadReportHeaderLabelsElement(pageHeaderLabelsElement, reportResult.ReportTemplateType, doPrintTaxBillText);
            CreateBillingInvoiceRowsReportHeaderLabelsElement(pageHeaderLabelsElement, reportResult.ReportTemplateType);

            #endregion

            #region PreviousInvoices

            string previousInvoicesLabel = "";
            if (reportResult.ReportTemplateType.IsValidIn(SoeReportTemplateType.BillingOrder))
                previousInvoicesLabel = GetReportText(292, "Tidigare ordrar");
            else
                previousInvoicesLabel = GetReportText(293, "Tidigare fakturor");

            pageHeaderLabelsElement.Add(
                new XElement("PreviousInvoicesLabel", previousInvoicesLabel));

            //Get AccountDims and add AccountDimLabelElements
            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId);
            AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);

            #endregion

            #region Contract labels

            pageHeaderLabelsElement.Add(new XElement("ContractGroupNameLabel", GetReportText(806, "Avtalsgrupp")));
            pageHeaderLabelsElement.Add(new XElement("ContractGroupPeriodTypeLabel", GetReportText(807, "Period")));
            pageHeaderLabelsElement.Add(new XElement("ContractNextYearLabel", GetReportText(808, "Nästa år")));
            pageHeaderLabelsElement.Add(new XElement("ContractNextPeriodLabel", GetReportText(809, "Nästa period")));
            pageHeaderLabelsElement.Add(new XElement("ContractEndYearLabel", GetReportText(810, "Avslutsår")));
            pageHeaderLabelsElement.Add(new XElement("ContractEndPeriodLabel", GetReportText(811, "Avslutsperiod")));
            pageHeaderLabelsElement.Add(new XElement("ContractCategoryNameLabel", GetReportText(815, "Avtalskategori")));

            #endregion

            #region Close document

            BillingStatistics.Add(reportHeaderLabelsElement);
            BillingStatistics.Add(reportHeaderElement);
            BillingStatistics.Add(pageHeaderLabelsElement);
            BillingStatistics.Add(billingStatisticsElements);


            rootElement.Add(BillingStatistics);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.BillingStatisticsReport);

            #endregion
        }

        public XDocument CreateIOCustomerInvoiceData(CreateReportResult reportResult, EvaluatedSelection es = null)
        {
            #region Prereq

            if (reportResult == null)
                return null;

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            if (es != null)
            {
                reportParams.SI_CustomerInvoiceHeadIOIds = es.SI_CustomerInvoiceHeadIOIds;
            }

            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId, true);
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(reportResult.ActorCompanyId, true);
            List<CustomerInvoiceHeadIO> customerInvoiceHeadIOs = ImportExportManager.GetCustomerInvoiceHeadIOResult(reportResult.ActorCompanyId, reportParams.SI_CustomerInvoiceHeadIOIds);

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "CustomerInvoiceIO");

            //VoucherList
            XElement customerInvoiceIOElement = new XElement("CustomerInvoiceIO");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement;

            if (es != null)
            {
                reportHeaderElement = CreateBillingReportHeaderElement(es);
            }
            else
            {
                reportHeaderElement = CreateBillingReportHeaderElement(
                    reportResult, reportParams);
            }

            customerInvoiceIOElement.Add(reportHeaderElement);

            #endregion

            #region ReportHeaderLabels

            //TODO set order/offer or invoice type to get the correct terms
            XElement reportHeaderLabelsElement = new XElement(CreateBillingReportHeaderLabelsElement(reportResult.ReportTemplateType));
            customerInvoiceIOElement.Add(reportHeaderLabelsElement);


            #endregion

            #region PageHeaderLabels

            //TODO set order/offer or invoice type to get the correct terms
            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateBillingInvoiceHeadReportHeaderLabelsElement(pageHeaderLabelsElement, reportResult.ReportTemplateType, false);
            CreateBillingInvoiceRowsReportHeaderLabelsElement(pageHeaderLabelsElement, reportResult.ReportTemplateType);

            customerInvoiceIOElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Content

            XElement customerInvoiceHeadIOElement = null;
            var customerInvoiceHeadIOElements = new List<XElement>();
            var accountCache = new List<Account>();

            int customerInvoiceHeadeIOXmlId = 1;
            foreach (var customerInvoiceIO in customerInvoiceHeadIOs)
            {
                #region CustomerInvoiceHead

                #region Prereq

                string customerInvoiceNr = customerInvoiceIO.CustomerInvoiceNr != null ? customerInvoiceIO.CustomerInvoiceNr : string.Empty;
                int seqNr = customerInvoiceIO.SeqNr != null ? (int)customerInvoiceIO.SeqNr : 0;
                string oCR = customerInvoiceIO.OCR != null ? customerInvoiceIO.OCR : string.Empty;
                string batchId = customerInvoiceIO.BatchId != null ? customerInvoiceIO.BatchId : string.Empty;
                int registrationType = customerInvoiceIO.RegistrationType;
                int originType = customerInvoiceIO.OriginType;
                string customerNr = customerInvoiceIO.CustomerNr != null ? customerInvoiceIO.CustomerNr : string.Empty;
                string customerName = customerInvoiceIO.CustomerName != null ? customerInvoiceIO.CustomerName : string.Empty;
                string paymentCondition = customerInvoiceIO.PaymentCondition != null ? customerInvoiceIO.PaymentCondition : string.Empty;
                string invoiceDate = customerInvoiceIO.InvoiceDate != null ? ((DateTime)customerInvoiceIO.InvoiceDate).ToShortDateString() : string.Empty;
                string dueDate = customerInvoiceIO.DueDate != null ? ((DateTime)customerInvoiceIO.DueDate).ToShortDateString() : string.Empty;
                string voucherDate = customerInvoiceIO.VoucherDate != null ? ((DateTime)customerInvoiceIO.VoucherDate).ToShortDateString() : string.Empty;
                string referenceOur = customerInvoiceIO.ReferenceOur != null ? customerInvoiceIO.ReferenceOur : string.Empty;
                string referenceYour = customerInvoiceIO.ReferenceYour != null ? customerInvoiceIO.ReferenceYour : string.Empty;
                decimal currencyRate = customerInvoiceIO.CurrencyRate != null ? (decimal)customerInvoiceIO.CurrencyRate : 1;
                string currencyDate = customerInvoiceIO.CurrencyDate != null ? ((DateTime)customerInvoiceIO.CurrencyDate).ToShortDateString() : string.Empty;
                decimal totalAmount = customerInvoiceIO.TotalAmount != null ? (decimal)customerInvoiceIO.TotalAmount : 0;
                decimal totalAmountCurrency = customerInvoiceIO.TotalAmountCurrency != null ? (decimal)customerInvoiceIO.TotalAmountCurrency : 0;
                decimal vATAmount = customerInvoiceIO.VATAmount != null ? (decimal)customerInvoiceIO.VATAmount : 0;
                decimal vATAmountCurrency = customerInvoiceIO.VATAmountCurrency != null ? (decimal)customerInvoiceIO.VATAmountCurrency : 0;
                decimal paidAmount = customerInvoiceIO.PaidAmount != null ? (decimal)customerInvoiceIO.PaidAmount : 0;
                decimal paidAmountCurrency = customerInvoiceIO.PaidAmountCurrency != null ? (decimal)customerInvoiceIO.PaidAmountCurrency : 0;
                decimal remainingAmount = customerInvoiceIO.RemainingAmount != null ? (decimal)customerInvoiceIO.RemainingAmount : 0;
                decimal freightAmount = customerInvoiceIO.FreightAmount != null ? (decimal)customerInvoiceIO.FreightAmount : 0;
                decimal freightAmountCurrency = customerInvoiceIO.FreightAmountCurrency != null ? (decimal)customerInvoiceIO.FreightAmountCurrency : 0;
                decimal invoiceFee = customerInvoiceIO.InvoiceFee != null ? (decimal)customerInvoiceIO.InvoiceFee : 0;
                decimal invoiceFeeCurrency = customerInvoiceIO.InvoiceFeeCurrency != null ? (decimal)customerInvoiceIO.InvoiceFeeCurrency : 0;
                decimal centRounding = customerInvoiceIO.CentRounding != null ? (decimal)customerInvoiceIO.CentRounding : 0;
                int fullyPayed = customerInvoiceIO.FullyPayed != null ? 1 : 0;
                int createAccountingInXE = customerInvoiceIO.CreateAccountingInXE != null ? 1 : 0;
                string paymentNr = customerInvoiceIO.PaymentNr != null ? customerInvoiceIO.PaymentNr : string.Empty;
                string voucherNr = customerInvoiceIO.VoucherNr != null ? customerInvoiceIO.VoucherNr : string.Empty;
                string note = customerInvoiceIO.Note != null ? customerInvoiceIO.Note : string.Empty;
                int billingType = customerInvoiceIO.BillingType != null ? (int)customerInvoiceIO.BillingType : 0;
                string currency = customerInvoiceIO.Currency != null ? customerInvoiceIO.Currency : string.Empty;
                string transferType = customerInvoiceIO.TransferType != null ? customerInvoiceIO.TransferType : string.Empty;
                string errorMessage = customerInvoiceIO.ErrorMessage != null ? customerInvoiceIO.ErrorMessage : string.Empty;
                string errorMessageDetails = string.Empty;
                string created = customerInvoiceIO.Created != null ? ((DateTime)customerInvoiceIO.Created).ToShortDateString() : string.Empty;
                string createdBy = customerInvoiceIO.CreatedBy != null ? customerInvoiceIO.CreatedBy : string.Empty;
                string modified = customerInvoiceIO.Modified != null ? ((DateTime)customerInvoiceIO.Modified).ToShortDateString() : string.Empty;
                string modifiedBy = customerInvoiceIO.ModifiedBy != null ? customerInvoiceIO.ModifiedBy : string.Empty;
                string billingAddressAddress = customerInvoiceIO.BillingAddressAddress != null ? customerInvoiceIO.BillingAddressAddress : string.Empty;
                string billingAddressCO = customerInvoiceIO.BillingAddressCO != null ? customerInvoiceIO.BillingAddressCO : string.Empty;
                string billingAddressPostNr = customerInvoiceIO.BillingAddressPostNr != null ? customerInvoiceIO.BillingAddressPostNr : string.Empty;
                string billingAddressCity = customerInvoiceIO.BillingAddressCity != null ? customerInvoiceIO.BillingAddressCity : string.Empty;

                #endregion

                #region CustomerInvoiceHeadIO element

                customerInvoiceHeadIOElement = new XElement("CustomerInvoiceHeadIO",
                    new XAttribute("Id", customerInvoiceHeadeIOXmlId),
                    new XElement("CustomerInvoiceNr", customerInvoiceNr),
                    new XElement("SeqNr", seqNr),
                    new XElement("OCR", oCR),
                    new XElement("BatchId", batchId),
                    new XElement("RegistrationType", registrationType),
                    new XElement("OriginType", originType),
                    new XElement("CustomerName", customerName),
                    new XElement("CustomerNr", customerNr),
                    new XElement("PaymentCondition", paymentCondition),
                    new XElement("InvoiceDate", invoiceDate),
                    new XElement("DueDate", dueDate),
                    new XElement("VoucherDate", voucherDate),
                    new XElement("ReferenceOur", referenceOur),
                    new XElement("ReferenceYour", referenceYour),
                    new XElement("CurrencyRate", currencyRate),
                    new XElement("CurrencyDate", currencyDate),
                    new XElement("TotalAmount", totalAmount),
                    new XElement("TotalAmountCurrency", totalAmountCurrency),
                    new XElement("VATAmount", vATAmount),
                    new XElement("VATAmountCurrency", vATAmountCurrency),
                    new XElement("PaidAmount", paidAmount),
                    new XElement("PaidAmountCurrency", paidAmountCurrency),
                    new XElement("RemainingAmount", remainingAmount),
                    new XElement("FreightAmount", freightAmount),
                    new XElement("FreightAmountCurrency", freightAmountCurrency),
                    new XElement("InvoiceFee", invoiceFee),
                    new XElement("InvoiceFeeCurrency", invoiceFeeCurrency),
                    new XElement("CentRounding", centRounding),
                    new XElement("FullyPayed", fullyPayed),
                    new XElement("CreateAccountingInXE", createAccountingInXE),
                    new XElement("PaymentNr", paymentNr),
                    new XElement("VoucherNr", voucherNr),
                    new XElement("Note", note),
                    new XElement("BillingType", billingType),
                    new XElement("Currency", currency),
                    new XElement("TransferType", transferType),
                    new XElement("ErrorMessage", errorMessage),
                    new XElement("ErrorMessageDetails", errorMessageDetails),
                    new XElement("Created", created),
                    new XElement("CreatedBy", createdBy),
                    new XElement("Modified", modified),
                    new XElement("ModifiedBy", modifiedBy),
                    new XElement("BillingAddressAddress", billingAddressAddress),
                    new XElement("BillingAddressCO", billingAddressCO),
                    new XElement("BillingAddressPostNr", billingAddressPostNr),
                    new XElement("BillingAddressCity", billingAddressCity),
                    new XElement("DeliveryAddressAddress", billingAddressCity),
                    new XElement("DeliveryAddressPostNr", billingAddressCity),
                    new XElement("DeliveryAddressCity", billingAddressCity));

                customerInvoiceHeadeIOXmlId++;

                #endregion

                int customerInvoiceRowXmlId = 1;
                foreach (var customerInvoiceRowIO in customerInvoiceIO.CustomerInvoiceRowIO)
                {
                    #region CustomerInvoiceRowIO element

                    #region Prereq

                    string rowbatchId = customerInvoiceRowIO.BatchId;
                    int rowType = customerInvoiceRowIO.CustomerRowType;
                    string rowinvoiceNr = customerInvoiceRowIO.InvoiceNr != null ? customerInvoiceRowIO.InvoiceNr : string.Empty;
                    string rowProductNr = customerInvoiceRowIO.ProductNr != null ? customerInvoiceRowIO.ProductNr : string.Empty;
                    string rowProductName = customerInvoiceRowIO.ProductName != null ? customerInvoiceRowIO.ProductName : string.Empty;
                    decimal rowQuantity = customerInvoiceRowIO.Quantity != null ? (decimal)customerInvoiceRowIO.Quantity : 0;
                    decimal rowUnitPrice = customerInvoiceRowIO.UnitPrice != null ? (decimal)customerInvoiceRowIO.UnitPrice : 0;
                    decimal rowDiscount = customerInvoiceRowIO.Discount != null ? (decimal)customerInvoiceRowIO.Discount : 0;
                    string rowAccountNr = customerInvoiceRowIO.AccountNr != null ? customerInvoiceRowIO.AccountNr : string.Empty;
                    string rowAccountDim2Nr = customerInvoiceRowIO.AccountDim2Nr != null ? customerInvoiceRowIO.AccountDim2Nr : string.Empty;
                    string rowAccountDim3Nr = customerInvoiceRowIO.AccountDim3Nr != null ? customerInvoiceRowIO.AccountDim3Nr : string.Empty;
                    string rowAccountDim4Nr = customerInvoiceRowIO.AccountDim4Nr != null ? customerInvoiceRowIO.AccountDim4Nr : string.Empty;
                    string rowAccountDim5Nr = customerInvoiceRowIO.AccountDim5Nr != null ? customerInvoiceRowIO.AccountDim5Nr : string.Empty;
                    string rowAccountDim6Nr = customerInvoiceRowIO.AccountDim6Nr != null ? customerInvoiceRowIO.AccountDim6Nr : string.Empty;
                    decimal rowPurchasePrice = customerInvoiceRowIO.PurchasePrice != null ? (decimal)customerInvoiceRowIO.PurchasePrice : 0;
                    decimal rowPurchasePriceCurrency = customerInvoiceRowIO.PurchasePriceCurrency != null ? (decimal)customerInvoiceRowIO.PurchasePriceCurrency : 0;
                    decimal rowAmount = customerInvoiceRowIO.Amount != null ? (decimal)customerInvoiceRowIO.Amount : 0;
                    decimal rowAmountCurrency = customerInvoiceRowIO.AmountCurrency != null ? (decimal)customerInvoiceRowIO.AmountCurrency : 0;
                    decimal rowVatAmount = customerInvoiceRowIO.VatAmount != null ? (decimal)customerInvoiceRowIO.VatAmount : 0;
                    decimal rowVatAmountCurrency = customerInvoiceRowIO.VatAmountCurrency != null ? (decimal)customerInvoiceRowIO.VatAmountCurrency : 0;
                    decimal rowDiscountAmount = customerInvoiceRowIO.DiscountAmount != null ? (decimal)customerInvoiceRowIO.DiscountAmount : 0;
                    decimal rowDiscountAmountCurrency = customerInvoiceRowIO.DiscountAmountCurrency != null ? (decimal)customerInvoiceRowIO.DiscountAmountCurrency : 0;
                    decimal rowMarginalIncome = customerInvoiceRowIO.MarginalIncome != null ? (decimal)customerInvoiceRowIO.MarginalIncome : 0;
                    decimal rowMarginalIncomeCurrency = customerInvoiceRowIO.MarginalIncomeCurrency != null ? (decimal)customerInvoiceRowIO.MarginalIncomeCurrency : 0;
                    decimal rowSumAmount = customerInvoiceRowIO.SumAmount != null ? (decimal)customerInvoiceRowIO.SumAmount : 0;
                    decimal rowSumAmountCurrency = customerInvoiceRowIO.SumAmountCurrency != null ? (decimal)customerInvoiceRowIO.SumAmountCurrency : 0;
                    string rowText = customerInvoiceRowIO.Text != null ? customerInvoiceRowIO.Text : string.Empty;
                    string rowErrorMessage = customerInvoiceRowIO.ErrorMessage != null ? customerInvoiceRowIO.ErrorMessage : string.Empty;
                    int rowNr = customerInvoiceRowIO.RowNr != null ? (int)customerInvoiceRowIO.RowNr : 0;
                    string rowCreated = customerInvoiceRowIO.Created != null ? ((DateTime)(customerInvoiceRowIO.Created)).ToShortDateString() : string.Empty;
                    string rowCreatedBy = customerInvoiceRowIO.CreatedBy != null ? customerInvoiceRowIO.CreatedBy : string.Empty;
                    string rowModified = customerInvoiceRowIO.Modified != null ? ((DateTime)(customerInvoiceRowIO.Modified)).ToShortDateString() : string.Empty;
                    string rowModifiedBy = customerInvoiceRowIO.ModifiedBy != null ? customerInvoiceRowIO.ModifiedBy : string.Empty;
                    decimal rowVatRate = customerInvoiceRowIO.VatRate != null ? (decimal)customerInvoiceRowIO.VatRate : 0;
                    string rowUnit = customerInvoiceRowIO.Unit != null ? customerInvoiceRowIO.Unit : string.Empty;

                    #endregion

                    XElement customerInvoiceRowIOElement = new XElement("CustomerInvoiceRowIO",
                        new XAttribute("Id", customerInvoiceRowXmlId),
                        new XElement("rowbatchId", rowbatchId),
                        new XElement("rowType", rowType),
                        new XElement("rowinvoiceNr", rowinvoiceNr),
                        new XElement("rowProductNr", rowProductNr),
                        new XElement("rowProductName", rowProductName),
                        new XElement("rowQuantity", rowQuantity),
                        new XElement("rowUnitPrice", rowUnitPrice),
                        new XElement("rowDiscount", rowDiscount),
                        new XElement("rowAccountNr", rowAccountNr),
                        new XElement("rowAccountDim2Nr", rowAccountDim2Nr),
                        new XElement("rowAccountDim3Nr", rowAccountDim3Nr),
                        new XElement("rowAccountDim4Nr", rowAccountDim4Nr),
                        new XElement("rowAccountDim5Nr", rowAccountDim5Nr),
                        new XElement("rowAccountDim6Nr", rowAccountDim6Nr),
                        new XElement("rowPurchasePrice", rowPurchasePrice),
                        new XElement("rowPurchasePriceCurrency", rowPurchasePriceCurrency),
                        new XElement("rowAmount", rowAmount),
                        new XElement("rowAmountCurrency", rowAmountCurrency),
                        new XElement("rowVatAmount", rowVatAmount),
                        new XElement("rowVatAmountCurrency", rowVatAmountCurrency),
                        new XElement("rowDiscountAmount", rowDiscountAmount),
                        new XElement("rowDiscountAmountCurrency", rowDiscountAmountCurrency),
                        new XElement("rowMarginalIncome", rowMarginalIncome),
                        new XElement("rowMarginalIncomeCurrency", rowMarginalIncomeCurrency),
                        new XElement("rowSumAmount", rowSumAmount),
                        new XElement("rowSumAmountCurrency", rowSumAmountCurrency),
                        new XElement("rowText", rowText),
                        new XElement("rowErrorMessage", rowErrorMessage),
                        new XElement("rowNr", rowNr),
                        new XElement("rowCreated", rowCreated),
                        new XElement("rowCreatedBy", rowCreatedBy),
                        new XElement("rowModified", rowModified),
                        new XElement("rowModifiedBy", rowModifiedBy),
                        new XElement("rowVatRate", rowVatRate),
                        new XElement("rowUnit", rowUnit));

                    customerInvoiceRowXmlId++;

                    List<string> rowErrorMessageDetails = new List<string>();

                    Account account = !string.IsNullOrEmpty(rowAccountNr) ? accountCache.FirstOrDefault(x => x.AccountNr == rowAccountNr && x.AccountDimId == accountDimStd.AccountDimId) : null;
                    if (account == null && !string.IsNullOrEmpty(rowAccountNr))
                    {
                        account = AccountManager.GetAccountByNr(rowAccountNr, accountDimStd.AccountDimId, reportResult.ActorCompanyId);
                        if (account != null)
                            accountCache.Add(account);
                    }
                    if (account == null)
                        rowErrorMessageDetails.Add($"AccountNr not found: {rowAccountNr}");

                    int accountDimNr = 2;
                    foreach (AccountDim dim in accountDimInternals.Where(i => i.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(i => i.AccountDimNr))
                    {
                        if (accountDimNr == 2 && !customerInvoiceRowIO.AccountDim2Nr.IsNullOrEmpty() && !accountInternals.Any(i => i.Account.AccountDimId == dim.AccountDimId && i.Account.AccountNr == customerInvoiceRowIO.AccountDim2Nr))
                            rowErrorMessageDetails.Add($"{dim.Name} not found: {customerInvoiceRowIO.AccountDim2Nr}");
                        else if (accountDimNr == 3 && !customerInvoiceRowIO.AccountDim3Nr.IsNullOrEmpty() && !accountInternals.Any(i => i.Account.AccountDimId == dim.AccountDimId && i.Account.AccountNr == customerInvoiceRowIO.AccountDim3Nr))
                            rowErrorMessageDetails.Add($"{dim.Name} not found: {customerInvoiceRowIO.AccountDim3Nr}");
                        else if (accountDimNr == 4 && !customerInvoiceRowIO.AccountDim4Nr.IsNullOrEmpty() && !accountInternals.Any(i => i.Account.AccountDimId == dim.AccountDimId && i.Account.AccountNr == customerInvoiceRowIO.AccountDim4Nr.NullToEmpty()))
                            rowErrorMessageDetails.Add($"{dim.Name} not found: {customerInvoiceRowIO.AccountDim4Nr}");
                        else if (accountDimNr == 5 && !customerInvoiceRowIO.AccountDim5Nr.IsNullOrEmpty() && !accountInternals.Any(i => i.Account.AccountDimId == dim.AccountDimId && i.Account.AccountNr == customerInvoiceRowIO.AccountDim5Nr))
                            rowErrorMessageDetails.Add($"{dim.Name} not found: {customerInvoiceRowIO.AccountDim5Nr}");
                        else if (accountDimNr == 6 && !customerInvoiceRowIO.AccountDim6Nr.IsNullOrEmpty() && !accountInternals.Any(i => i.Account.AccountDimId == dim.AccountDimId && i.Account.AccountNr == customerInvoiceRowIO.AccountDim6Nr))
                            rowErrorMessageDetails.Add($"{dim.Name} not found: {customerInvoiceRowIO.AccountDim6Nr}");
                    }

                    customerInvoiceRowIOElement.Add(
                         new XElement("rowErrorMessageDetails", rowErrorMessageDetails.ToCommaSeparated()));

                    customerInvoiceHeadIOElement.Add(customerInvoiceRowIOElement);

                    #endregion
                }

                customerInvoiceHeadIOElements.Add(customerInvoiceHeadIOElement);

                #endregion
            }

            customerInvoiceIOElement.Add(customerInvoiceHeadIOElements);

            #endregion

            #region Close document

            rootElement.Add(customerInvoiceIOElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.IOCustomerInvoice);

            #endregion
        }
        #endregion

        #region Checklist

        public XElement CreateChecklistReportHeaderElement(CreateReportResult reportResult)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            string companyLogoPath = GetCompanyLogoFilePath(entitiesReadOnly, ActorCompanyId, false);
            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(reportResult.ReportName),
                    this.CreateReportDescriptionElement(reportResult.ReportDescription),
                    this.CreateReportNrElement(reportResult.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(reportResult.LoginName),
                    new XElement("CompanyLogo", companyLogoPath));
        }

        public XDocument CreateChecklistsReportData(CreateReportResult reportResult, EvaluatedSelection es = null)
        {
            if (reportResult.ReportTemplateType != SoeReportTemplateType.OrderChecklistReport)
                return null;

            #region Prereq

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            if (es != null)
            {
                reportParams.SB_InvoiceIds = es.SB_InvoiceIds;
                reportParams.SB_ChecklistHeadRecordId = es.SB_ChecklistHeadRecordId;
            }

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "ChecklistsReport");

            //ChecklistsReport
            XElement checklistsReportElement = new XElement("ChecklistsReport");

            //Checklists
            XElement checklistsElement = new XElement("Checklists");

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateChecklistReportHeaderLabelsElement();
            checklistsReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateChecklistReportHeaderElement(reportResult);
            checklistsReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateChecklistsReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            checklistsReportElement.Add(pageHeaderLabelsElement);

            #endregion

            List<XElement> checklistElements = new List<XElement>();

            using (CompEntities entities = new CompEntities())
            {
                if (reportResult.ReportTemplateType == SoeReportTemplateType.OrderChecklistReport)
                {
                    #region OrderChecklistReport

                    foreach (int invoiceId in reportParams.SB_InvoiceIds)
                    {
                        #region Order

                        #region Prereq

                        List<ChecklistExtendedRowDTO> rowDTOs = ChecklistManager.GetChecklistRows(SoeEntityType.Order, invoiceId, reportResult.ActorCompanyId);

                        //Filter ChecklistHeadRecord
                        if (reportParams.SB_ChecklistHeadRecordId.HasValue)
                            rowDTOs = rowDTOs.Where(i => i.HeadRecordId == reportParams.SB_ChecklistHeadRecordId.Value).ToList();

                        var heads = (from r in rowDTOs
                                     select new
                                     {
                                         r.HeadRecordId,
                                         r.HeadId,
                                         r.Name,
                                         HeadDescription = r.Description,
                                     }).Distinct();

                        #endregion

                        int headXmlId = 1;
                        int imageXmlId = 1;
                        int rowImageXmlId = 1;
                        bool crGen = SettingManager.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.UseCrGen, 0, 0, 0);
                        foreach (var head in heads)
                        {
                            #region Prereq

                            CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoiceAndProject(entities, invoiceId, true);
                            if (customerInvoice == null)
                                continue;

                            string orderNr = customerInvoice.InvoiceNr;
                            DateTime orderDate = customerInvoice.OrderDate.HasValue ? customerInvoice.OrderDate.Value : CalendarUtility.DATETIME_DEFAULT;
                            string projectNr = customerInvoice.Project != null ? customerInvoice.Project.Number : String.Empty;
                            string projectName = customerInvoice.Project != null ? customerInvoice.Project.Name : String.Empty;
                            string customerNr = customerInvoice.Actor != null && customerInvoice.Actor.Customer != null ? customerInvoice.Actor.Customer.CustomerNr : String.Empty;
                            string customerName = customerInvoice.Actor != null && customerInvoice.Actor.Customer != null ? customerInvoice.Actor.Customer.Name : String.Empty;
                            string referenceYour = customerInvoice.ReferenceYour;
                            string companyName = Company.Name;
                            string referenceOur = customerInvoice.ReferenceOur;

                            #region Addresses

                            #region Company

                            string companyAddress = "";
                            string companyPostalCode = "";
                            string companyPostalAddress = "";

                            Contact companyContact = ContactManager.GetContactFromActor(entities, Company.ActorCompanyId, loadAllContactInfo: true);
                            if (companyContact != null && companyContact.ContactAddress != null)
                            {
                                TermGroup_SysContactAddressType companyBillingAddressType = companyContact.ContactAddress.AddressExists(TermGroup_SysContactAddressType.Billing) ? TermGroup_SysContactAddressType.Billing : TermGroup_SysContactAddressType.Visiting;

                                ContactAddress companyContactAddress = companyContact.ContactAddress.FirstOrDefault(ca => ca.SysContactAddressTypeId == (int)companyBillingAddressType);
                                if (companyContactAddress != null)
                                {
                                    List<ContactAddressRow> companyAddressRows = companyContactAddress.ContactAddressRow.ToList();
                                    companyAddress = companyAddressRows.GetContactAddressRowText(companyBillingAddressType, TermGroup_SysContactAddressRowType.Address);
                                    companyPostalCode = companyAddressRows.GetContactAddressRowText(companyBillingAddressType, TermGroup_SysContactAddressRowType.PostalCode);
                                    companyPostalAddress = companyAddressRows.GetContactAddressRowText(companyBillingAddressType, TermGroup_SysContactAddressRowType.PostalAddress);
                                }
                            }

                            #endregion

                            #region Customer

                            string customerBillingAddress = "";
                            string customerBillingPostalCode = "";
                            string customerBillingPostalAddress = "";
                            string customerDeliveryAddress = "";
                            string customerDeliveryPostalCode = "";
                            string customerDeliveryPostalAddress = "";

                            Contact customerContact = customerInvoice.ActorId.HasValue ? ContactManager.GetContactFromActor(entities, customerInvoice.ActorId.Value, loadAllContactInfo: true) : null;
                            if (customerContact != null && customerContact.ContactAddress != null)
                            {
                                ContactAddress customerBillingContactAddress = customerContact.ContactAddress.FirstOrDefault(ca => ca.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing);
                                if (customerBillingContactAddress != null)
                                {
                                    List<ContactAddressRow> customerBillingAddressRows = customerBillingContactAddress.ContactAddressRow.ToList();
                                    customerBillingAddress = customerBillingAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Address, customerInvoice.BillingAddressId);
                                    customerBillingPostalCode = customerBillingAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalCode, customerInvoice.BillingAddressId);
                                    customerBillingPostalAddress = customerBillingAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalAddress, customerInvoice.BillingAddressId);
                                }
                                var customerDeliveryAddressEntity = customerContact.ContactAddress
                                                        .Where(ca => ca.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery)
                                                        .Where(ca => customerInvoice.DeliveryAddressId == 0 || ca.ContactAddressId == customerInvoice.DeliveryAddressId)
                                                        .FirstOrDefault();

                                if (customerDeliveryAddressEntity != null)
                                {
                                    var customerDeliveryAddressRows = customerDeliveryAddressEntity.ContactAddressRow.ToList();
                                    customerDeliveryAddress = customerDeliveryAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Address, customerInvoice.DeliveryAddressId);
                                    customerDeliveryPostalCode = customerDeliveryAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalCode, customerInvoice.DeliveryAddressId);
                                    customerDeliveryPostalAddress = customerDeliveryAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalAddress, customerInvoice.DeliveryAddressId);
                                }
                            }

                            #endregion

                            #endregion

                            #region OrderChecklists

                            List<int> orderChecklistIds = new List<int>();
                            if (reportParams.SB_ChecklistHeadRecordId.HasValue)
                            {
                                orderChecklistIds.Add(reportParams.SB_ChecklistHeadRecordId.Value);
                            }
                            else
                            {
                                List<ChecklistHeadRecord> orderChecklists = ChecklistManager.GetChecklistHeadRecords(SoeEntityType.Order, customerInvoice.InvoiceId, Company.ActorCompanyId);
                                foreach (ChecklistHeadRecord orderChecklist in orderChecklists)
                                {
                                    orderChecklistIds.Add(orderChecklist.ChecklistHeadRecordId);
                                }
                            }

                            #endregion

                            #region Signatures

                            var imageDTOs = new List<ImagesDTO>();

                            if (reportParams.SB_ChecklistHeadRecordId.HasValue)
                            {
                                imageDTOs.AddRange(ChecklistManager.GetEntityChecklistSignatures(Company.ActorCompanyId, SoeEntityType.ChecklistHeadRecord, reportParams.SB_ChecklistHeadRecordId.Value));
                            }
                            else
                            {
                                imageDTOs.AddRange(ChecklistManager.GetEntityChecklistsSignatures(Company.ActorCompanyId, SoeEntityType.Order, invoiceId));
                            }
                            #endregion

                            #endregion

                            #region Content

                            #region Checklist element

                            string strImagePath = "";
                            string strImageDescription = "";
                            if (imageDTOs.Any())
                            {
                                foreach (var imageDTO in imageDTOs)
                                {

                                    if (imageDTO.DataStorageRecordType == SoeDataStorageRecordType.ChecklistHeadRecordSignatureExecutor)
                                    {
                                        string imagePath = GraphicsManager.GetImageFilePath(imageDTO, reportResult.ActorCompanyId);
                                        if (crGen)
                                            imagePath = AddToCrGenRequestPicturesDTO(imagePath);
                                        strImagePath = imagePath;
                                        strImageDescription = imageDTO.Description;
                                        break;
                                    }
                                }
                            }

                            XElement checklistElement = new XElement("Checklist",
                                new XAttribute("id", headXmlId),
                                new XElement("Name", head.Name),
                                new XElement("Description", head.HeadDescription),
                                new XElement("OrderNr", orderNr),
                                new XElement("OrderDate", orderDate),
                                new XElement("ProjectNr", projectNr),
                                new XElement("ProjectName", projectName),
                                new XElement("CustomerNr", customerNr),
                                new XElement("CustomerName", customerName),
                                new XElement("ReferenceYour", referenceYour),
                                new XElement("ReferenceOur", referenceOur),
                                new XElement("CompanyName", companyName),
                                new XElement("CompanyDeliveryAddress", companyAddress),
                                new XElement("CompanyDeliveryAddressPostalNrCity", String.Format("{0} / {1}", companyPostalCode, companyPostalAddress)),
                                new XElement("CustomerBillingAddress", customerBillingAddress),
                                new XElement("CustomerBillingAddressPostNrCity", String.Format("{0} / {1}", customerBillingPostalCode, customerBillingPostalAddress)),
                                new XElement("DeliveryAddress", customerDeliveryAddress),
                                new XElement("DeliveryAddressPostNrCity", String.Format("{0} / {1}", customerDeliveryPostalCode, customerDeliveryPostalAddress)),
                                new XElement("ImagePath", strImagePath),
                                new XElement("ImageDescription", strImageDescription)
                                );

                            #endregion

                            #region ChecklistRow element

                            int rowXmlId = 1;
                            foreach (ChecklistExtendedRowDTO rowDTO in rowDTOs.Where(i => i.HeadRecordId == head.HeadRecordId))
                            {
                                XElement checklistRowElement = new XElement("ChecklistRow",
                                    new XAttribute("id", rowXmlId),
                                    new XElement("RowNr", rowDTO.RowNr),
                                    new XElement("Mandatory", rowDTO.Mandatory.ToInt()),
                                    new XElement("QuestionType", (int)rowDTO.Type),
                                    new XElement("QuestionText", rowDTO.Text),
                                    new XElement("AnswerDataType", rowDTO.DataTypeId),
                                    new XElement("AnswerValue", rowDTO.Value),
                                    new XElement("Comment", rowDTO.Comment),
                                    new XElement("Date", rowDTO.Date.HasValue ? rowDTO.Date.Value : CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("Created", rowDTO.Created.HasValue ? rowDTO.Created.Value : CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("CreatedBy", rowDTO.CreatedBy),
                                    new XElement("Modified", rowDTO.Modified.HasValue ? rowDTO.Modified.Value : CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("ModifiedBy", rowDTO.ModifiedBy));

                                if (rowDTO.Type == TermGroup_ChecklistRowType.Image)
                                {
                                    var rowImages = ChecklistManager.GetChecklistRowImages(entities, rowDTO.RowRecordId, reportResult.ActorCompanyId);
                                    if (!rowImages.IsNullOrEmpty())
                                    {
                                        var rowImagesElement = new XElement("ChecklistRowImages");
                                        rowImageXmlId = 1;
                                        foreach (var image in rowImages)
                                        {
                                            string imagePath = GraphicsManager.GetImageFilePath(image, reportResult.ActorCompanyId, !crGen);
                                            if (crGen)
                                                imagePath = AddToCrGenRequestPicturesDTO(imagePath, image.Image);
                                            XElement rowImageElement = new XElement("ChecklistRowImage",
                                                new XAttribute("id", rowImageXmlId),
                                                new XElement("ImagePath", imagePath),
                                                new XElement("Description", image.Description)
                                                );
                                            rowImagesElement.Add(rowImageElement);
                                            rowImageXmlId++;
                                        }

                                        checklistRowElement.Add(rowImagesElement);
                                    }
                                }

                                checklistElement.Add(checklistRowElement);
                                rowXmlId++;
                            }

                            #endregion

                            #region ChecklistImage element

                            if (imageDTOs.Any())
                            {
                                foreach (var imageDTO in imageDTOs)
                                {
                                    if (imageDTO.DataStorageRecordType == SoeDataStorageRecordType.ChecklistHeadRecordSignature || imageDTO.DataStorageRecordType == SoeDataStorageRecordType.ChecklistHeadRecordSignatureExecutor)
                                    {
                                        string imagePath = GraphicsManager.GetImageFilePath(imageDTO, reportResult.ActorCompanyId, !crGen);
                                        if (crGen)
                                            imagePath = AddToCrGenRequestPicturesDTO(imagePath, imageDTO.Image);
                                        string type = "1";
                                        XElement checklistImage = new XElement("ChecklistImage",
                                            new XAttribute("id", imageXmlId),
                                            new XElement("ImagePath", imagePath),
                                            new XElement("Type", type),
                                            new XElement("Description", imageDTO.Description));

                                        checklistElement.Add(checklistImage);
                                        imageXmlId++;
                                    }
                                }
                            }

                            #endregion

                            #region Default element ChecklistRow

                            if (rowXmlId == 1)
                                checklistElement.Add(new XElement("ChecklistRow", String.Empty));

                            #endregion

                            checklistElements.Add(checklistElement);
                            headXmlId++;

                            #endregion
                        }

                        #region Default Checklist

                        if (headXmlId == 1)
                        {
                            checklistElements.Add(new XElement("Checklist",
                                new XAttribute("id", 1),
                                new XElement("Name", String.Empty),
                                new XElement("Description", String.Empty),
                                new XElement("OrderNr", String.Empty),
                                new XElement("OrderDate", String.Empty),
                                new XElement("ProjectNr", String.Empty),
                                new XElement("ProjectName", String.Empty),
                                new XElement("CustomerNr", String.Empty),
                                new XElement("CustomerName", String.Empty),
                                new XElement("ReferenceYour", String.Empty),
                                new XElement("ReferenceOur", String.Empty),
                                new XElement("CompanyName", String.Empty),
                                new XElement("CompanyDeliveryAddress", String.Empty),
                                new XElement("CompanyDeliveryAddressPostalNrCity", String.Empty),
                                new XElement("CustomerBillingAddress", String.Empty),
                                new XElement("CustomerBillingAddressPostNrCity", String.Empty),
                                new XElement("DeliveryAddress", String.Empty),
                                new XElement("DeliveryAddressPostNrCity", String.Empty)));

                            XElement ChecklistImage = new XElement("ChecklistImage",
                                new XAttribute("id", 1),
                                new XElement("ImagePath", ""),
                                new XElement("Type", ""),
                                new XElement("Description", ""));

                            checklistElements.Add(ChecklistImage);
                        }

                        #endregion

                        #endregion
                    }

                    #endregion
                }
            }

            #region Close document

            foreach (XElement checklistElement in checklistElements)
            {
                checklistsElement.Add(checklistElement);
            }

            checklistsReportElement.Add(checklistsElement);
            rootElement.Add(checklistsReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.OrderChecklistReport);

            #endregion
        }

        #endregion

        #region Expense

        public XDocument CreateExpenseReportData(CreateReportResult reportResult)
        {
            #region Prereq

            bool showStartStopInTimeReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowStartStopInTimeReport, this.UserId, this.ActorCompanyId, 0, false);

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "ExpensesReport");

            XElement expenseReportElement = new XElement("ExpensesReport");

            this.Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var am = new AccountManager(parameterObject);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportParams = new BillingReportParamsDTO(am, reportResult, entitiesReadOnly, this);

            #endregion


            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            expenseReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateAccountDistributionHeadListReportHeaderElement(reportResult);
            var extendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, reportResult.ActorCompanyId, 0);
            reportHeaderElement.Add(new XElement("ShowStartStop", showStartStopInTimeReport));
            reportHeaderElement.Add(new XElement("ExtendedTimeRegistration", extendedTimeRegistration));
            reportHeaderElement.Add(CreateDateIntervalElement(reportParams));
            reportHeaderElement.Add(reportParams.CreateEmployeeIntervalElement());
            reportHeaderElement.Add(reportParams.CreateProjectIntervalElement());
            reportHeaderElement.Add(reportParams.CreateCustomerIntervalElement());
            expenseReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateProjectReportPageHeaderLabelsElement(pageHeaderLabelsElement, reportResult.ReportTemplateType);
            expenseReportElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Content

            using (CompEntities entities = new CompEntities())
            {

                SoeOriginType originType = SoeOriginType.None;
                List<CustomerInvoice> customerInvoices = InvoiceManager.GetCustomerInvoicesFromSelection(entities, reportResult, reportParams, reportParams.SB_IncludeDrafts, ref originType, true);

                int invoiceId = 0;
                if (reportParams.SB_HasInvoiceIds)
                    invoiceId = reportParams.SB_InvoiceIds.Count > 0 ? reportParams.SB_InvoiceIds.First() : 0;

                expenseReportElement = CreateExpenseElement(entities, reportResult, reportParams, expenseReportElement, customerInvoices.FirstOrDefault(), 0, out int nrOfExpenseRows);

            }
            #endregion

            #region Close document

            rootElement.Add(expenseReportElement);
            document.Add(rootElement);
            return GetValidatedDocument(document, SoeReportTemplateType.ExpenseReport);

            #endregion
        }
        #endregion
    }

    #region BillingReportParamsDTO

    public class BillingReportParamsDTO
    {
        public bool HasDateInterval { get; set; }
        public string SSTD_AccountYearFromText { get; set; }
        public string SSTD_LongAccountYearFromText { get; set; }
        public string LongAccountYearFromText { get; set; }
        public string SSTD_AccountYearToText { get; set; }
        public string SSTD_LongAccountYearToText { get; set; }
        public bool SSTD_HasAccountYearText { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime DateFrom { get; set; }
        public bool SSTD_HasAccountPeriodText { get; set; }
        public string SSTD_AccountPeriodFromText { get; set; }
        public string SSTD_AccountPeriodToText { get; set; }
        public int SB_DateRegard { get; set; }
        public int SB_SortOrder { get; set; }
        public int SB_StockLocationIdFrom { get; set; }
        public int SB_StockLocationIdTo { get; set; }
        public int SB_StockShelfIdFrom { get; set; }
        public int SB_StockShelfIdTo { get; set; }
        public string SB_ProductGroupFrom { get; set; }
        public string SB_ProductGroupTo { get; set; }
        public string SB_ProductNrFrom { get; set; }
        public string SB_ProductNrTo { get; set; }

        public int SB_StockTransactionType { get; set; }
        public CreateReportResult ReportResult { get; set; }

        public AccountYear AccountYear { get; set; }
        public AccountYear AccountYearTo { get; set; }
        public string LongAccountYearToText { get; set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel1 { get; internal set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel2 { get; internal set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel3 { get; internal set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel4 { get; internal set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel1 { get; internal set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel2 { get; internal set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel3 { get; internal set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel4 { get; internal set; }
        public bool IsSortAscending { get; internal set; }
        public string Special { get; internal set; }
        public bool SA_HasAccountInterval { get; internal set; }
        public List<AccountIntervalDTO> SA_AccountIntervals { get; internal set; }
        public bool SB_DisableInvoiceCopies { get; internal set; }
        public int SB_ReportLanguageId { get; internal set; }
        public bool SB_HasInvoiceNrInterval { get; internal set; }
        public string SB_InvoiceNrFrom { get; internal set; }
        public bool SB_HasPurchaseNrInterval { get; internal set; }
        public string SB_InvoiceNrTo { get; internal set; }
        public string SB_PurchaseNrFrom { get; internal set; }
        public string SB_PurchaseNrTo { get; internal set; }
        public bool SB_HasInvoiceIds { get; internal set; }
        public bool SB_HasPurchaseIds { get; internal set; }
        public bool SB_HasSB_ProductIds { get; internal set; }

        public bool SB_HasActorNrInterval { get; internal set; }
        public bool SB_HasCustomerNrInterval { get; internal set; }
        public string SB_ActorNrFrom { get; internal set; }
        public string SB_ActorNrTo { get; internal set; }
        public bool SB_HasPaymentDateInterval { get; internal set; }
        public DateTime SB_PaymentDateFrom { get; internal set; }
        public DateTime SB_PaymentDateTo { get; internal set; }

        public DateTime SB_CreateDateFrom { get; set; }
        public DateTime SB_CreateDateTo { get; set; }
        public bool HasCreateDateInterval { get; set; }
        public List<int> SB_InvoiceIds { get; internal set; }
        public List<int> SB_PurchaseIds { get; internal set; }
        public List<int> SB_ProductIds { get; internal set; }

        public bool SB_ShowNotPrinted { get; internal set; }
        public bool SB_ShowCopy { get; internal set; }
        public bool SB_IncludeClosedOrder { get; internal set; }
        public bool SB_IncludeInvoicedOrders { get; internal set; }
        public int SB_CustomerGroupId { get; internal set; }
        public string ReportNamePostfix { get; internal set; }
        public bool SB_IncludeOnlyInvoiced { get; internal set; }
        public bool SB_IncludeProjectReport { get; internal set; }
        public bool SB_IncludeProjectReport2 { get; internal set; }
        public bool SB_IncludeDrafts { get; internal set; }
        public int? ST_TimePeriodId { get; set; }
        public bool? OnlyActiveAccounts { get; internal set; }
        public bool SB_InvoiceCopy { get; internal set; }
        public bool SB_InvoiceReminder { get; internal set; }
        public string SB_ProjectNrFrom { get; set; }
        public string SB_ProjectNrTo { get; set; }
        public bool SB_HasProjectNrInterval { get; set; }
        public string SB_EmployeeNrFrom { get; set; }
        public string SB_EmployeeNrTo { get; set; }
        public bool SB_HasEmployeeNrInterval { get; set; }
        public List<int> SP_ProjectIds { get; set; }
        public int SB_HTDCompanyId { get; set; }
        public int SB_HTDSeqNbr { get; set; }
        public bool SB_HTDUseInputSeqNbr { get; set; }
        public List<int> SB_HTDCustomerInvoiceRowIds { get; set; }
        public bool SB_HTDHasCustomerInvoiceRowIds { get; set; }
        public int? SB_ChecklistHeadRecordId { get; set; }
        public List<AccountFilterSelectionDTO> accountFilters;
        public int SB_StockInventoryId { get; internal set; }

        public string SB_PeriodFrom { get; set; }
        public string SB_PeriodTo { get; set; }
        public DateTime? SP_PayrollTransactionDateFrom { get; set; }
        public DateTime? SP_PayrollTransactionDateTo { get; set; }
        public DateTime? SP_InvoiceTransactionDateFrom { get; set; }
        public DateTime? SP_InvoiceTransactionDateTo { get; set; }
        public bool SP_IncludeChildProjects { get; set; }
        public bool SP_ExcludeInternalOrders { get; set; }
        public bool? ST_ActiveEmployees { get; set; }
        public bool ST_OnlyActiveEmployees { get; set; }
        public string SP_Dim2From { get; set; }
        public string SP_Dim2To { get; set; }
        public string SP_Dim3From { get; set; }
        public string SP_Dim3To { get; set; }
        public string SP_Dim4From { get; set; }
        public string SP_Dim4To { get; set; }
        public string SP_Dim5From { get; set; }
        public string SP_Dim5To { get; set; }
        public string SP_Dim6From { get; set; }
        public string SP_Dim6To { get; set; }
        public int? SP_Dim2Id { get; set; }
        public int? SP_Dim3Id { get; set; }
        public int? SP_Dim4Id { get; set; }
        public int? SP_Dim5Id { get; set; }
        public int? SP_Dim6Id { get; set; }
        public string SB_CustomerNrFrom { get; internal set; }
        public string SB_CustomerNrTo { get; internal set; }
        public DateTime SB_StockValueDate { get; set; }
        public int? SL_InvoiceSeqNrFrom { get; set; }
        public int? SL_InvoiceSeqNrTo { get; set; }
        public bool SL_HasInvoiceSeqNrInterval { get; set; }
        public int SL_SortOrder { get; set; }
        public int SL_DateRegard { get; set; }
        public int SL_InvoiceSelection { get; set; }

        public bool SL_HasInvoiceIds { get; set; }
        public IEnumerable<int> SL_InvoiceIds { get; set; }
        public bool SL_HasActorNrInterval { get; set; }
        public string SL_ActorNrFrom { get; set; }
        public string SL_ActorNrTo { get; set; }

        public List<int> SI_CustomerInvoiceHeadIOIds { get; set; }

        public bool SB_HTDUseGreen { get; set; }

        public BillingReportParamsDTO(AccountManager am, CreateReportResult reportResult, CompEntities entity, BaseReportDataManager baseReportMgr)
        {
            DateTime selectionDateFrom = DateTime.MinValue;
            DateTime selectionDateTo = DateTime.MinValue;
            DateTime selectionPaymentDateFrom = DateTime.MinValue;
            DateTime selectionPaymentDateTo = DateTime.MinValue;
            DateTime selectionCreateDateFrom = DateTime.MinValue;
            DateTime selectionCreateDateTo = DateTime.MaxValue;
            ReportResult = reportResult;
            string invoiceSeqNrFrom;
            string invoiceSeqNrTo;
            string actorNrIdFrom;
            string actorNrIdTo;
            string customerNrFrom;
            string customerNrTo;
            int? stockTransactionType;

            string purchaseNrFrom;
            string purchaseNrTo;
            string productGroupSelectionFrom;
            string productGroupSelectionTo;
            int? inventoryBalanceSelectionIdFrom;
            int? inventoryBalanceSelectionIdTo;
            int? codeSelectionIdFrom;
            int? codeSelectionIdTo;
            int? dateRegard;
            int? sortOrder;
            int? languageId;
            string actorNrFrom;
            string actorNrTo;
            bool dateSelectionChoosed;
            bool includeclosedorder;
            bool includeInvoicedOrders;
            bool includeDrafts;
            bool showAnyUnprinted;
            bool showCopies;
            bool showCopiesOfOriginal;
            bool asReminder;
            int? customerCategory;
            string projectNrFrom;
            string projectNrTo;
            string employeeNrFrom;
            string employeeNrTo;
            int? stockInventory;
            List<int> purchaseIds;
            List<int> productIds;
            List<int> invoiceIds;

            int? invoiceSelection;
            int? invoiceSeqNoFrom = 0;
            int? invoiceSeqNoTo = 0;

            string dim2From;
            string dim2To;
            string dim3From;
            string dim3To;
            string dim4From;
            string dim4To;
            string dim5From;
            string dim5To;
            string dim6From;
            string dim6To;
            int? dim2Id;
            int? dim3Id;
            int? dim4Id;
            int? dim5Id;
            int? dim6Id;
            List<int> projectIds;

            DateTime? selectionTransactionDateFrom;// = DateTime.MinValue;
            DateTime? selectionTransactionDateTo;// = DateTime.MinValue;
            bool includeChildProjects;
            bool excludeInternalOrders;
            bool printTimeReport;
            bool includeOnlyInvoiced;
            bool mergePdfs;

            int? taxReductionType;
            int? applicationStatus;

            baseReportMgr.TryGetIdFromSelection(reportResult, out languageId, "languageId");
            SB_ReportLanguageId = languageId.HasValue ? languageId.Value : 0;

            baseReportMgr.TryGetBoolFromSelection(reportResult, out dateSelectionChoosed, "dateSelectionChoosen");
            baseReportMgr.TryGetBoolFromSelection(reportResult, out includeclosedorder, "includeclosedorder");
            baseReportMgr.TryGetBoolFromSelection(reportResult, out includeInvoicedOrders, "includeInvoicedOrders");
            baseReportMgr.TryGetBoolFromSelection(reportResult, out includeDrafts, "includedrafts");
            baseReportMgr.TryGetBoolFromSelection(reportResult, out asReminder, "asreminder");

            baseReportMgr.TryGetIdFromSelection(reportResult, out inventoryBalanceSelectionIdFrom, "inventoryBalanceFrom");
            baseReportMgr.TryGetIdFromSelection(reportResult, out inventoryBalanceSelectionIdTo, "inventoryBalanceTo");

            baseReportMgr.TryGetIdFromSelection(reportResult, out codeSelectionIdFrom, "codeSeriesFrom");
            baseReportMgr.TryGetIdFromSelection(reportResult, out codeSelectionIdTo, "codeSeriesTo");

            baseReportMgr.TryGetTextFromSelection(reportResult, out invoiceSeqNrFrom, "invoiceSeqNrFrom");
            baseReportMgr.TryGetTextFromSelection(reportResult, out invoiceSeqNrTo, "invoiceSeqNrTo");

            baseReportMgr.TryGetTextFromSelection(reportResult, out purchaseNrFrom, "purchaseNrFrom");
            baseReportMgr.TryGetTextFromSelection(reportResult, out purchaseNrTo, "purchaseNrTo");

            baseReportMgr.TryGetTextFromSelection(reportResult, out actorNrIdFrom, "actorNrFrom");
            baseReportMgr.TryGetTextFromSelection(reportResult, out actorNrIdTo, "actorNrTo");

            baseReportMgr.TryGetTextFromSelection(reportResult, out actorNrFrom, "actorNrFrom");
            baseReportMgr.TryGetTextFromSelection(reportResult, out actorNrTo, "actorNrTo");

            baseReportMgr.TryGetIdFromSelection(reportResult, out invoiceSeqNoFrom, "invoiceSeqNrFrom");
            baseReportMgr.TryGetIdFromSelection(reportResult, out invoiceSeqNoTo, "invoiceSeqNrTo");

            baseReportMgr.TryGetTextFromSelection(reportResult, out projectNrFrom, "projectNrFrom");
            baseReportMgr.TryGetTextFromSelection(reportResult, out projectNrTo, "projectNrTo");

            baseReportMgr.TryGetTextFromSelection(reportResult, out employeeNrFrom, "employeeNrFrom");
            baseReportMgr.TryGetTextFromSelection(reportResult, out employeeNrTo, "employeeNrTo");

            baseReportMgr.TryGetIdFromSelection(reportResult, out stockTransactionType, "stockTransactionType");
            SB_StockTransactionType = stockTransactionType.HasValue ? stockTransactionType.Value : 0;

            baseReportMgr.TryGetIdFromSelection(reportResult, out invoiceSelection, "invoiceSelection");
            baseReportMgr.TryGetIdFromSelection(reportResult, out dateRegard, "dateRegard");
            baseReportMgr.TryGetIdFromSelection(reportResult, out sortOrder, "sortOrder");

            baseReportMgr.TryGetIdsFromSelection(reportResult, out purchaseIds, "purchaseIds");
            baseReportMgr.TryGetIdsFromSelection(reportResult, out productIds, "productIds");
            baseReportMgr.TryGetIdsFromSelection(reportResult, out invoiceIds, "invoiceIds");


            baseReportMgr.TryGetAccountFilters(reportResult, out accountFilters, "namedFilterRanges");
            SA_AccountIntervals = accountFilters.Select(x => new AccountIntervalDTO() { AccountDimId = x.Id, AccountNrFrom = x.From, AccountNrTo = x.To }).ToList();

            baseReportMgr.TryGetTextFromSelection(reportResult, out productGroupSelectionFrom, "productGroupsFrom");
            baseReportMgr.TryGetTextFromSelection(reportResult, out productGroupSelectionTo, "productGroupsTo");

            baseReportMgr.TryGetTextFromSelection(reportResult, out dim2From, "dim2From");
            baseReportMgr.TryGetTextFromSelection(reportResult, out dim2To, "dim2To");
            baseReportMgr.TryGetTextFromSelection(reportResult, out dim3From, "dim3From");
            baseReportMgr.TryGetTextFromSelection(reportResult, out dim3To, "dim3To");

            baseReportMgr.TryGetTextFromSelection(reportResult, out dim4From, "dim4From");
            baseReportMgr.TryGetTextFromSelection(reportResult, out dim4To, "dim4To");
            baseReportMgr.TryGetTextFromSelection(reportResult, out dim5From, "dim5From");
            baseReportMgr.TryGetTextFromSelection(reportResult, out dim5To, "dim5To");
            baseReportMgr.TryGetTextFromSelection(reportResult, out dim6From, "dim6From");
            baseReportMgr.TryGetTextFromSelection(reportResult, out dim6To, "dim6To");

            baseReportMgr.TryGetIdFromSelection(reportResult, out dim2Id, "dim2Id");
            baseReportMgr.TryGetIdFromSelection(reportResult, out dim3Id, "dim3Id");
            baseReportMgr.TryGetIdFromSelection(reportResult, out dim4Id, "dim4Id");
            baseReportMgr.TryGetIdFromSelection(reportResult, out dim5Id, "dim5Id");
            baseReportMgr.TryGetIdFromSelection(reportResult, out dim6Id, "dim6Id");

            baseReportMgr.TryGetBoolFromSelection(reportResult, out includeChildProjects, "includeChildProjects");
            baseReportMgr.TryGetBoolFromSelection(reportResult, out excludeInternalOrders, "excludeInternalOrders");

            baseReportMgr.TryGetBoolFromSelection(reportResult, out printTimeReport, "printTimeReport");
            baseReportMgr.TryGetBoolFromSelection(reportResult, out includeOnlyInvoiced, "includeOnlyInvoiced");

            baseReportMgr.TryGetBoolFromSelection(reportResult, out mergePdfs, "mergepdfs");

            baseReportMgr.TryGetIdsFromSelection(reportResult, out projectIds, "selectedProjectIds");

            List<int> customerInvoiceHeadIOIds;
            baseReportMgr.TryGetIdsFromSelection(
                reportResult, out customerInvoiceHeadIOIds, "customerInvoiceHeadIOIds");

            baseReportMgr.TryGetIdFromSelection(reportResult, out taxReductionType, "taxReductionType");
            baseReportMgr.TryGetIdFromSelection(reportResult, out applicationStatus, "applicationStatus");

            if (customerInvoiceHeadIOIds.Count > 0)
            {
                SI_CustomerInvoiceHeadIOIds = customerInvoiceHeadIOIds;
            }

            SP_Dim2From = dim2From != null ? dim2From : "";
            SP_Dim2To = dim2To != null ? dim2To : "";
            SP_Dim3From = dim3From != null ? dim3From : "";
            SP_Dim3To = dim3To != null ? dim3To : "";
            SP_Dim4From = dim4From != null ? dim4From : "";
            SP_Dim4To = dim4To != null ? dim4To : "";
            SP_Dim5From = dim5From != null ? dim5From : "";
            SP_Dim5To = dim5To != null ? dim5To : "";
            SP_Dim6From = dim6From != null ? dim6From : "";
            SP_Dim6To = dim6To != null ? dim6To : "";

            SP_Dim2Id = dim2Id;
            SP_Dim3Id = dim3Id;
            SP_Dim4Id = dim4Id;
            SP_Dim5Id = dim5Id;
            SP_Dim6Id = dim6Id;

            SP_IncludeChildProjects = includeChildProjects;
            SP_ExcludeInternalOrders = excludeInternalOrders;
            SP_ProjectIds = projectIds;

            SB_IncludeProjectReport = printTimeReport;
            SB_IncludeOnlyInvoiced = includeOnlyInvoiced;

            SB_ProductNrFrom = invoiceSeqNrFrom;//.HasValue ? inventoryBalanceSelectionIdFrom.Value : 0;
            SB_ProductNrTo = invoiceSeqNrTo;//.HasValue ? inventoryBalanceSelectionIdFrom.Value : 0;

            SB_StockLocationIdFrom = inventoryBalanceSelectionIdFrom.HasValue ? inventoryBalanceSelectionIdFrom.Value : 0;
            SB_StockLocationIdTo = inventoryBalanceSelectionIdTo.HasValue ? inventoryBalanceSelectionIdTo.Value : 0;

            SB_InvoiceNrFrom = invoiceSeqNrFrom;
            SB_InvoiceNrTo = invoiceSeqNrTo;

            SL_InvoiceSelection = invoiceSelection.HasValue ? invoiceSelection.Value : 0;
            SL_DateRegard = dateRegard.HasValue ? dateRegard.Value : 0;
            SL_SortOrder = sortOrder.HasValue ? sortOrder.Value : 0;

            SL_InvoiceSeqNrFrom = invoiceSeqNoFrom.GetValueOrDefault();
            SL_InvoiceSeqNrTo = invoiceSeqNoTo.GetValueOrDefault();
            if (SL_InvoiceSeqNrFrom > 0 || SL_InvoiceSeqNrTo > 0)
            {
                SL_HasInvoiceSeqNrInterval = true;
            }

            if (!String.IsNullOrEmpty(actorNrFrom) && (!String.IsNullOrEmpty(actorNrTo)))
            {
                SL_ActorNrFrom = actorNrFrom;
                SL_ActorNrTo = actorNrTo;
                SL_HasActorNrInterval = true;
            }

            SB_ActorNrFrom = actorNrIdFrom;
            SB_ActorNrTo = actorNrIdTo;

            customerNrFrom = actorNrIdFrom;
            customerNrTo = actorNrIdTo;

            SB_CustomerNrFrom = customerNrFrom;
            SB_CustomerNrTo = customerNrTo;

            SB_PurchaseNrFrom = purchaseNrFrom;
            SB_PurchaseNrTo = purchaseNrTo;

            SB_StockShelfIdFrom = codeSelectionIdFrom.HasValue ? codeSelectionIdFrom.Value : 0;
            SB_StockShelfIdTo = codeSelectionIdTo.HasValue ? codeSelectionIdTo.Value : 0;

            SB_ProductGroupFrom = productGroupSelectionFrom;
            SB_ProductGroupTo = productGroupSelectionTo;

            baseReportMgr.TryGetBoolFromSelection(reportResult, out showAnyUnprinted, "showAnyUnprinted");
            SB_ShowNotPrinted = showAnyUnprinted;
            baseReportMgr.TryGetBoolFromSelection(reportResult, out showCopies, "showCopies");
            SB_ShowCopy = showCopies;
            baseReportMgr.TryGetBoolFromSelection(reportResult, out showCopiesOfOriginal, "showCopiesOfOriginal");
            SB_InvoiceCopy = !showCopiesOfOriginal;

            baseReportMgr.TryGetIdFromSelection(reportResult, out customerCategory, "customerCategory");
            SB_CustomerGroupId = customerCategory.HasValue ? customerCategory.Value : 0;

            SB_IncludeClosedOrder = includeclosedorder;
            SB_IncludeInvoicedOrders = includeInvoicedOrders;
            SB_IncludeDrafts = includeDrafts;
            SB_InvoiceReminder = asReminder;

            SB_SortOrder = sortOrder.HasValue ? sortOrder.Value : 0;
            SB_DateRegard = dateRegard.HasValue ? dateRegard.Value : 0;

            SB_HasPurchaseIds = purchaseIds != null && purchaseIds.Count > 0 ? true : false;
            if (SB_HasPurchaseIds)
            {
                SB_PurchaseIds = purchaseIds;
            }

            SB_HasSB_ProductIds = productIds != null && productIds.Count > 0 ? true : false;
            if (SB_HasSB_ProductIds)
            {
                SB_ProductIds = purchaseIds;
            }

            SB_HasInvoiceIds = invoiceIds != null && invoiceIds.Count > 0 ? true : false;
            if (SB_HasInvoiceIds)
            {
                SB_InvoiceIds = invoiceIds;
            }

            if (baseReportMgr.TryGetDateFromSelection(reportResult, out DateTime selectionStockValueDate, "stockvaluedate"))
            {
                SB_StockValueDate = selectionStockValueDate;
            }

            baseReportMgr.TryGetIdFromSelection(reportResult, out stockInventory, "stockInventory");

            if ((!String.IsNullOrEmpty(actorNrIdFrom)) && (!String.IsNullOrEmpty(actorNrIdTo)))
            {
                SB_HasActorNrInterval = true;
            }

            if ((!String.IsNullOrEmpty(customerNrFrom)) && (!String.IsNullOrEmpty(customerNrTo)))
            {
                SB_HasCustomerNrInterval = true;
            }


            if ((!String.IsNullOrEmpty(invoiceSeqNrFrom)) && (!String.IsNullOrEmpty(invoiceSeqNrTo)))
            {
                SB_HasInvoiceNrInterval = true;
            }

            if ((!String.IsNullOrEmpty(purchaseNrFrom)) && (!String.IsNullOrEmpty(purchaseNrTo)))
            {
                SB_HasPurchaseNrInterval = true;
            }

            SB_ProjectNrFrom = projectNrFrom;
            SB_ProjectNrTo = projectNrTo;
            SB_HasProjectNrInterval = !string.IsNullOrEmpty(projectNrFrom) || !string.IsNullOrEmpty(projectNrTo);

            SB_EmployeeNrFrom = employeeNrFrom;
            SB_EmployeeNrTo = employeeNrTo;
            SB_HasEmployeeNrInterval = !string.IsNullOrEmpty(employeeNrFrom) || !string.IsNullOrEmpty(employeeNrTo);

            SB_StockInventoryId = stockInventory.HasValue ? stockInventory.Value : 0;

            List<int> selectionTimePeriodIds = new List<int>();

            if (dateSelectionChoosed)
            {
                if (baseReportMgr.TryGetDatesFromSelection(reportResult, out selectionDateFrom, out selectionDateTo))
                {
                    AccountYear = am.GetAccountYear(selectionDateFrom, reportResult.ActorCompanyId);
                    AccountYearTo = am.GetAccountYear(selectionDateTo, reportResult.ActorCompanyId);
                }
            }
            else
            {
                AccountIntervalSelectionDTO rangeFrom = null;
                AccountIntervalSelectionDTO rangeTo = null;

                TryGetRange(reportResult, out rangeFrom, "accountPeriodFrom");
                TryGetRange(reportResult, out rangeTo, "accountPeriodTo");
                int rangeFromId = rangeFrom != null ? rangeFrom.Value.Value : 0;
                int rangeToId = rangeTo != null ? rangeTo.Value.Value : 0;
                if (rangeFromId != 0 && rangeToId != 0)
                {
                    selectionTimePeriodIds.Add(rangeFromId);
                    selectionTimePeriodIds.Add(rangeToId);
                    var selectedTimePeriods = entity.AccountPeriod.Where(w => selectionTimePeriodIds.Contains(w.AccountPeriodId)).OrderBy(o => o.From).ToList();
                    selectionDateFrom = selectedTimePeriods.First().From;
                    selectionDateTo = selectedTimePeriods.Last().To;
                    AccountYear = am.GetAccountYear(selectionDateFrom, reportResult.ActorCompanyId);
                    AccountYearTo = am.GetAccountYear(selectionDateTo, reportResult.ActorCompanyId);
                }
                else if (rangeFrom != null && rangeTo != null)
                {
                    selectionTimePeriodIds.Add(rangeFromId);
                    selectionTimePeriodIds.Add(rangeToId);

                    AccountYear = am.GetAccountYear(rangeFrom.YearId.Value, true);
                    AccountYearTo = am.GetAccountYear(rangeTo.YearId.Value, true);
                    var selectedTimePeriods = entity.AccountPeriod.Where(w => selectionTimePeriodIds.Contains(w.AccountPeriodId)).OrderBy(o => o.From).ToList();

                    if (rangeFrom.YearId.Value != 0 && selectedTimePeriods.Any())
                    {
                        selectionDateFrom = selectedTimePeriods.FirstOrDefault().From;
                    }
                    else
                    {
                        if (AccountYear != null)
                        {
                            selectionDateFrom = AccountYear.From;
                        }
                        else
                        {
                            selectionDateFrom = CalendarUtility.DATETIME_DEFAULT;
                        }
                    }
                    if (rangeTo.YearId.Value != 0 && selectedTimePeriods.Any())
                    {
                        selectionDateTo = selectedTimePeriods.Last().To;
                    }
                    else
                    {
                        if (AccountYearTo != null)
                        {
                            selectionDateTo = AccountYearTo.To;
                        }
                        else
                        {
                            selectionDateTo = CalendarUtility.DATETIME_MAXVALUE;
                        }
                    }
                }
                else
                {
                    baseReportMgr.TryGetDatesFromSelection(reportResult, out selectionDateFrom, out selectionDateTo);
                }
            }
            if (!((selectionDateFrom.Date == CalendarUtility.DATETIME_DEFAULT.Date && selectionDateTo.Date == CalendarUtility.DATETIME_DEFAULT.Date) ||
                (selectionDateFrom.Date == CalendarUtility.DATETIME_MINVALUE.Date && selectionDateTo.Date == CalendarUtility.DATETIME_MAXVALUE.Date) ||
                (selectionDateFrom.Date == CalendarUtility.DATETIME_MINVALUE.Date && selectionDateTo.Date == CalendarUtility.DATETIME_0VALUE.Date)
                ))
            {
                HasDateInterval = true;
            }

            DateFrom = selectionDateFrom.Date;
            DateTo = selectionDateTo.Date;

            if (AccountYear != null && AccountYearTo != null)
            {
                SSTD_AccountYearFromText = GetShortDateStringFromCulture(AccountYear.From);
                SSTD_LongAccountYearFromText = GetLongDateStringFromCulture(AccountYear.From);
                LongAccountYearFromText = GetLongDateStringFromCulture(AccountYear.From);

                SSTD_AccountYearToText = GetShortDateStringFromCulture(AccountYearTo.To);
                SSTD_LongAccountYearToText = GetLongDateStringFromCulture(AccountYearTo.To);
                LongAccountYearToText = GetLongDateStringFromCulture(AccountYearTo.To);
            }
            else
            {
                SSTD_AccountYearFromText = GetShortDateStringFromCulture(DateFrom);
                SSTD_LongAccountYearFromText = GetLongDateStringFromCulture(DateFrom);
                LongAccountYearFromText = GetLongDateStringFromCulture(DateFrom);

                SSTD_AccountYearToText = GetShortDateStringFromCulture(DateTo);
                SSTD_LongAccountYearToText = GetLongDateStringFromCulture(DateTo);
                LongAccountYearToText = GetLongDateStringFromCulture(DateTo);
            }

            if (reportResult.ReportTemplateType == SoeReportTemplateType.OriginStatisticsReport || reportResult.ReportTemplateType == SoeReportTemplateType.BillingInvoiceInterest ||
                reportResult.ReportTemplateType == SoeReportTemplateType.StockInventoryReport || reportResult.ReportTemplateType == SoeReportTemplateType.BillingInvoice)
            {
                SSTD_HasAccountYearText = ((AccountYear != null && AccountYearTo != null) && !String.IsNullOrEmpty(SSTD_AccountYearFromText) && !String.IsNullOrEmpty(SSTD_AccountYearToText));
            }
            else
            {
                SSTD_HasAccountYearText = (!String.IsNullOrEmpty(SSTD_AccountYearFromText) && !String.IsNullOrEmpty(SSTD_AccountYearToText)) &&
                (SSTD_AccountYearFromText != GetShortDateStringFromCulture(CalendarUtility.DATETIME_DEFAULT) && SSTD_AccountYearToText != GetShortDateStringFromCulture(CalendarUtility.DATETIME_DEFAULT));
            }

            if (baseReportMgr.TryGetDatesFromSelection(reportResult, out selectionPaymentDateFrom, out selectionPaymentDateTo, "paymentDateRange"))
            {
                if (!((selectionPaymentDateFrom.Date == CalendarUtility.DATETIME_DEFAULT.Date && selectionPaymentDateTo.Date == CalendarUtility.DATETIME_DEFAULT.Date) ||
                (selectionPaymentDateFrom.Date == CalendarUtility.DATETIME_MINVALUE.Date && selectionPaymentDateTo.Date == CalendarUtility.DATETIME_MAXVALUE.Date) ||
                (selectionPaymentDateFrom.Date == CalendarUtility.DATETIME_MINVALUE.Date && selectionPaymentDateTo.Date == CalendarUtility.DATETIME_0VALUE.Date)
                ))
                {
                    SB_HasPaymentDateInterval = true;
                }
            }

            if (baseReportMgr.TryGetDatesFromSelection(reportResult, out selectionCreateDateFrom, out selectionCreateDateTo, "createdDateRange"))
            {
                if (!((selectionCreateDateFrom.Date == CalendarUtility.DATETIME_DEFAULT.Date && selectionCreateDateTo.Date == CalendarUtility.DATETIME_DEFAULT.Date) || (selectionCreateDateFrom.Date == CalendarUtility.DATETIME_MINVALUE.Date && selectionCreateDateTo.Date == CalendarUtility.DATETIME_MAXVALUE.Date)))
                {
                    HasCreateDateInterval = true;
                }
            }

            SB_PaymentDateFrom = selectionPaymentDateFrom.Date;
            SB_PaymentDateTo = selectionPaymentDateTo.Date;

            SB_CreateDateFrom = selectionCreateDateFrom.Date;
            SB_CreateDateTo = selectionCreateDateTo.Date;

            if (baseReportMgr.TryGetDatesFromSelection(reportResult, out selectionTransactionDateFrom, out selectionTransactionDateTo, "transactionDateRange"))
            {
                if ((selectionTransactionDateTo.Value.Date - selectionTransactionDateFrom.Value.Date).Days > 0)
                {
                    this.SP_InvoiceTransactionDateFrom = selectionTransactionDateFrom;
                    this.SP_InvoiceTransactionDateTo = selectionTransactionDateTo;

                }
            }

            if (reportResult.ReportTemplateType == SoeReportTemplateType.ProjectTransactionsReport)
            {
                ST_ActiveEmployees = true;
                ST_OnlyActiveEmployees = true;
            }

            #region Internal accounts

            if (reportResult.ReportTemplateType != SoeReportTemplateType.ProjectTransactionsReport)
            {
                if (SA_AccountIntervals.Count > 0)
                {
                    SA_HasAccountInterval = true;
                }
            }
            else
            {
                this.SA_AccountIntervals = new List<AccountIntervalDTO>();

                if (SP_Dim2From != String.Empty || SP_Dim2To != String.Empty)
                {
                    AccountIntervalDTO accountInterval = new AccountIntervalDTO();
                    accountInterval.AccountDimId = SP_Dim2Id;
                    accountInterval.AccountNrFrom = SP_Dim2From;
                    accountInterval.AccountNrTo = SP_Dim2To;
                    this.SA_HasAccountInterval = true;
                    this.SA_AccountIntervals.Add(accountInterval);
                }

                if (SP_Dim3From != String.Empty || SP_Dim3To != String.Empty)
                {
                    AccountIntervalDTO accountInterval = new AccountIntervalDTO();
                    accountInterval.AccountDimId = SP_Dim3Id;
                    accountInterval.AccountNrFrom = SP_Dim3From;
                    accountInterval.AccountNrTo = SP_Dim3To;
                    this.SA_HasAccountInterval = true;
                    this.SA_AccountIntervals.Add(accountInterval);
                }

                if (SP_Dim4From != String.Empty || SP_Dim4To != String.Empty)
                {
                    AccountIntervalDTO accountInterval = new AccountIntervalDTO();
                    accountInterval.AccountDimId = SP_Dim4Id;
                    accountInterval.AccountNrFrom = SP_Dim4From;
                    accountInterval.AccountNrTo = SP_Dim4To;
                    this.SA_HasAccountInterval = true;
                    this.SA_AccountIntervals.Add(accountInterval);
                }

                if (SP_Dim5From != String.Empty || SP_Dim5To != String.Empty)
                {
                    AccountIntervalDTO accountInterval = new AccountIntervalDTO();
                    accountInterval.AccountDimId = SP_Dim5Id;
                    accountInterval.AccountNrFrom = SP_Dim5From;
                    accountInterval.AccountNrTo = SP_Dim5To;
                    this.SA_HasAccountInterval = true;
                    this.SA_AccountIntervals.Add(accountInterval);
                }

                if (SP_Dim6From != String.Empty || SP_Dim6To != String.Empty)
                {
                    AccountIntervalDTO accountInterval = new AccountIntervalDTO();
                    accountInterval.AccountDimId = SP_Dim6Id;
                    accountInterval.AccountNrFrom = SP_Dim6From;
                    accountInterval.AccountNrTo = SP_Dim6To;
                    this.SA_HasAccountInterval = true;
                    this.SA_AccountIntervals.Add(accountInterval);
                }
            }
            #endregion

            int? companyId;
            baseReportMgr.TryGetIdFromSelection(
                reportResult, out companyId, "companyId");

            if (companyId.HasValue)
            {
                SB_HTDCompanyId = companyId.Value;
            }

            int? sequenceNumber;
            baseReportMgr.TryGetIdFromSelection(
                reportResult, out sequenceNumber, "sequenceNumber");

            if (sequenceNumber.HasValue)
            {
                SB_HTDSeqNbr = sequenceNumber.Value;
            }

            List<int> customerInvoiceRowIds;
            baseReportMgr.TryGetIdsFromSelection(reportResult, out customerInvoiceRowIds, "customerInvoiceRowIds");

            if (customerInvoiceRowIds?.Any() == true)
            {
                SB_HTDCustomerInvoiceRowIds = customerInvoiceRowIds;
                SB_HTDHasCustomerInvoiceRowIds = true;
            }

            bool useGreen;
            baseReportMgr.TryGetBoolFromSelection(reportResult, out useGreen, "useGreen");
            SB_HTDUseGreen = useGreen;

            bool useInputSeqNbr;
            baseReportMgr.TryGetBoolFromSelection(reportResult, out useInputSeqNbr, "useInputSeqNbr");
            SB_HTDUseInputSeqNbr = useInputSeqNbr;
        }

        public string GetAccountYearIntervalText()
        {
            string text = "";
            if (SSTD_HasAccountYearText)
                text = SSTD_AccountYearFromText + "-" + SSTD_AccountYearToText;
            return text;
        }

        public string GetAccountPeriodIntervalText()
        {
            string text = "";
            if (HasDateInterval)
                text = DateFrom.ToShortDateString() + "-" + DateTo.ToShortDateString();
            else if (SSTD_HasAccountPeriodText)
                text = SSTD_AccountPeriodFromText + "-" + SSTD_AccountPeriodToText;
            return text;
        }

        private bool TryGetRange(CreateReportResult reportResult, out AccountIntervalSelectionDTO value, string key)
        {
            var selection = reportResult?.Input?.GetSelection<AccountIntervalSelectionDTO>(key);
            if (selection != null)
                value = selection;
            else
                value = null;
            return selection != null;
        }


        private string GetShortDateStringFromCulture(DateTime dateTime)
        {
            string dateString = dateTime.ToString("yyyyMM");

            if (CultureInfo.CurrentCulture != null)
            {
                switch (CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                {
                    case "da":
                        dateString = dateTime.ToString("MM.yyyy");
                        break;
                    case "fi":
                        dateString = dateTime.ToString("M.yyyy");
                        break;
                    case "nb":
                        dateString = dateTime.ToString("MM.yyyy");
                        break;
                    case "en":
                        dateString = dateTime.ToString("M/yyyy");
                        break;
                }
            }

            return dateString;
        }

        private string GetLongDateStringFromCulture(DateTime dateTime)
        {
            string dateString = dateTime.ToString("yyyyMMdd");

            if (CultureInfo.CurrentCulture != null)
            {
                switch (CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                {
                    case "da":
                        dateString = dateTime.ToString("dd.MM.yyyy");
                        break;
                    case "fi":
                        dateString = dateTime.ToString("dd.MM.yyyy");
                        break;
                    case "nb":
                        dateString = dateTime.ToString("dd.MM.yyyy");
                        break;
                    case "en":
                        dateString = dateTime.ToString("d/M/yyyy");
                        break;
                }
            }

            return dateString;
        }


        public XElement CreateEmployeeIntervalElement()
        {
            return new XElement("EmployeeInterval", GetStandardEmployeeIntervalText());
        }

        public XElement CreateProjectIntervalElement()
        {
            return new XElement("ProjectInterval", GetStandardProjectIntervalText());
        }
        public XElement CreateCustomerIntervalElement()
        {
            return new XElement("CustomerInterval", GetStandardCustomerIntervalText());
        }
        protected string GetStandardCustomerIntervalText()
        {
            string text = "";
            if (SB_HasActorNrInterval)
            {
                text = SB_ActorNrFrom + "-" + SB_ActorNrTo;
            }
            return text;
        }

        public string GetStandardEmployeeIntervalText()
        {
            string text = "";
            if (SB_HasEmployeeNrInterval)
            {
                text = SB_EmployeeNrFrom + "-" + SB_EmployeeNrTo;
            }
            return text;
        }

        protected string GetStandardProjectIntervalText()
        {
            string text = "";
            if (SB_HasProjectNrInterval)
            {
                text = SB_ProjectNrFrom + "-" + SB_ProjectNrTo;
            }
            return text;
        }

    }

    #endregion
}

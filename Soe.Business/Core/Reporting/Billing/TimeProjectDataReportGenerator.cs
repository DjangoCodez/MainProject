using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Billing
{
    internal class TimeProjectDataReportGenerator 
    {
        private readonly TimeProjectXmlGenerator _xmlGenerator = new TimeProjectXmlGenerator();
        private InvoiceManager InvoiceManager;
        private ProjectManager ProjectManager;
        private AttestManager AttestManager;
        private TimeCodeManager TimeCodeManager;
        private TimeDeviationCauseManager TimeDeviationCauseManager;
        private TimeTransactionManager TimeTransactionManager;
        private SettingManager SettingManager;
        private IStateUtility StateUtility;

        public TimeProjectDataReportGenerator(InvoiceManager im, ProjectManager pm, AttestManager am, TimeCodeManager tcm, TimeDeviationCauseManager tdcm, TimeTransactionManager ttm, SettingManager sm, IStateUtility stateUtility)
        {
            InvoiceManager = im;
            ProjectManager = pm;
            AttestManager = am;
            TimeCodeManager = tcm;
            TimeDeviationCauseManager = tdcm;
            TimeTransactionManager = ttm;
            SettingManager = sm;
            StateUtility = stateUtility;
        }

        public XElement CreateTimeProjectElement(CompEntities entities, TimeProjectReportParams reportParams, List<TimeCode> timeCodes, int invoiceId)
        {
            var data = GetTimeProjectReportData(entities, reportParams, timeCodes, invoiceId);

            // Generate and return XML
            return _xmlGenerator.Generate(data);
        }

        public TimeProjectReportData GetTimeProjectReportData(CompEntities entities, TimeProjectReportParams reportParams, List<TimeCode> timeCodes, int invoiceId)
        {
            bool showStartStopInTimeReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowStartStopInTimeReport, reportParams.UserId, reportParams.ActorCompanyId, 0, false);

            // Gather data using appropriate method based on invoice type
            TimeProjectReportData data;

            return (reportParams.IncludeOnlyInvoiced) ?
                data = GetInvoicedTimeProjectData(entities, reportParams, timeCodes, invoiceId, showStartStopInTimeReport) :
                data = GetNonInvoicedTimeProjectData(entities, reportParams, timeCodes, invoiceId, showStartStopInTimeReport);
        }

        private TimeProjectReportData GetInvoicedTimeProjectData(CompEntities entities, TimeProjectReportParams timeParams, List<TimeCode> timeCodes, int invoiceId, bool showStartStopInTimeReport)
        {
            var result = new TimeProjectReportData();

            Invoice invoice = InvoiceManager.GetInvoice(entities, invoiceId);
            string invoiceNr = invoice.InvoiceNr;
            List<Project> projects = new List<Project>();
            List<CustomerInvoiceRow> invoiceRows = InvoiceManager.GetCustomerInvoiceRows(entities, invoiceId, false);

            int attestStateTransferredOrderToInvoiceId = 0;
            List<AttestTransition> attestTransitions = null;
            if (invoice.Origin.Type == (int)SoeOriginType.Order)
            {
                attestTransitions = AttestManager.GetAttestTransitions(entities, new List<TermGroup_AttestEntity> { TermGroup_AttestEntity.Order }, SoeModule.Billing, false, timeParams.ActorCompanyId);
                attestStateTransferredOrderToInvoiceId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, timeParams.ActorCompanyId, 0);
            }

            foreach (CustomerInvoiceRow row in invoiceRows.Where(r => r.Type == (int)SoeInvoiceRowType.ProductRow && r.IsTimeProjectRow))
            {
                if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                {
                    ProcessCustomerInvoiceRow(entities, row, timeParams, projects);
                }
                else if (invoice.Origin.Type == (int)SoeOriginType.Order)
                {
                    ProcessOrderInvoiceRow(entities, row, attestTransitions, attestStateTransferredOrderToInvoiceId, timeParams, projects);
                }
            }

            if (!projects.Any() && invoice.ProjectId.HasValue)
            {
                var fallbackProject = ProjectManager.GetProject(entities, invoice.ProjectId.Value);
                if (fallbackProject != null)
                {
                    projects.Add(fallbackProject);
                }
            }

            // Convert Projects to DTOs
            ConvertProjectsToReportData(projects, invoiceNr, entities, timeParams, timeCodes, showStartStopInTimeReport, result);

            // Detach entities
            DetachProjectEntities(entities, projects);

            return result;
        }

        private TimeProjectReportData GetNonInvoicedTimeProjectData(CompEntities entities, TimeProjectReportParams timeParams, List<TimeCode> timeCodes, int invoiceId, bool showStartStopInTimeReport)
        {
            var result = new TimeProjectReportData();

            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, timeParams.UserId, timeParams.ActorCompanyId, 0, false);

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
                    var projectInfo = new TimeProjectInfo
                    {
                        Number = project.Number,
                        Name = project.Name,
                        Description = project.Description,
                        InvoiceNr = invoiceNr,
                        Created = project.Created,
                        CreatedBy = project.CreatedBy,
                        State = project.State,
                        WorkSiteKey = project.WorkSiteKey,
                        WorkSiteNumber = project.WorkSiteNumber
                    };

                    if (useProjectTimeBlock)
                    {
                        GetProjectTimeBlockData(entities, timeParams, project, invoice, timeParams.DateFrom, timeParams.DateTo, timeCodes, showStartStopInTimeReport, projectInfo, ref result);
                    }
                    else
                    {
                        GetProjectInvoiceWeekData(entities, project, invoice, invoiceId, timeParams.DateFrom, timeParams.DateTo, timeCodes, projectInfo, ref result);
                    }

                    result.Projects.Add(projectInfo);
                }
            }

            return result;
        }

        private void GetProjectTimeBlockData(CompEntities entities, TimeProjectReportParams param, Project project, CustomerInvoice invoice, DateTime? fromDate, DateTime? toDate, List<TimeCode> timeCodes, bool showStartStopInTimeReport, TimeProjectInfo projectInfo, ref TimeProjectReportData result)
        {
            List<ProjectTimeBlock> projectTimeBlocks = new List<ProjectTimeBlock>();
            if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
            {
                List<int> connectedOrderDitinctIds = InvoiceManager.GetConnectedOrdersForCustomerInvoice(entities, invoice.InvoiceId);
                var tempBlocks = ProjectManager.GetProjectTimeBlocksForProject(entities, project.ProjectId, null, fromDate, toDate);
                foreach (var connectedOrderId in connectedOrderDitinctIds)
                {
                    if (projectTimeBlocks.Any(x => x.CustomerInvoiceId == connectedOrderId))
                        continue;

                    projectTimeBlocks.AddRange(tempBlocks.Where(x => x.CustomerInvoiceId == connectedOrderId).ToList());
                }
            }
            else
            {
                projectTimeBlocks = ProjectManager.GetProjectTimeBlocksForProject(entities, project.ProjectId, invoice.InvoiceId, fromDate, toDate);
            }

            result.TotalRows += projectTimeBlocks.Count;

            var projectTimeBlocksGroupedByEmployeeId = projectTimeBlocks.GroupBy(g => g.EmployeeId).ToList();

            foreach (var projectTimeBlockGroupedByEmployeeId in projectTimeBlocksGroupedByEmployeeId)
            {
                ProjectTimeBlock firstProjectTimeBlock = projectTimeBlockGroupedByEmployeeId.FirstOrDefault();
                if (firstProjectTimeBlock == null)
                    continue;

                Employee employee = firstProjectTimeBlock.Employee;
                if (employee == null)
                    continue;

                var employeeInfo = new TimeEmployeeInfo
                {
                    EmployeeNr = employee.EmployeeNr,
                    Name = employee.Name
                };

                foreach (ProjectTimeBlock projectTimeBlock in projectTimeBlockGroupedByEmployeeId)
                {
                    var timeEntry = CreateTimeEntryFromProjectTimeBlock(projectTimeBlock, timeCodes, param.ActorCompanyId, showStartStopInTimeReport);
                    employeeInfo.TimeEntries.Add(timeEntry);
                }

                projectInfo.Employees.Add(employeeInfo);
            }
        }

        private void GetProjectInvoiceWeekData(CompEntities entities, Project project, CustomerInvoice invoice, int invoiceId, DateTime? fromDate, DateTime? toDate, List<TimeCode> timeCodes, TimeProjectInfo projectInfo, ref TimeProjectReportData result)
        {
            List<ProjectInvoiceWeek> projectInvoiceWeeks = new List<ProjectInvoiceWeek>();

            if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
            {
                List<int> connectedOrderDitinctIds = InvoiceManager.GetConnectedOrdersForCustomerInvoice(entities, invoice.InvoiceId);
                var tempWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId);
                foreach (var connectedOrderId in connectedOrderDitinctIds)
                {
                    if (projectInvoiceWeeks.Any(x => x.RecordId == connectedOrderId))
                        continue;

                    projectInvoiceWeeks.AddRange(tempWeeks.Where(x => x.RecordId == connectedOrderId).ToList());
                }
            }
            else
            {
                projectInvoiceWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId, invoiceId, fromDate, toDate);
            }

            var projectInvoiceWeeksGroupedByEmployeeId = projectInvoiceWeeks.GroupBy(g => g.EmployeeId).ToList();

            foreach (var projectInvoiceWeekGroupedByEmployeeId in projectInvoiceWeeksGroupedByEmployeeId)
            {
                ProjectInvoiceWeek firstProjectInvoiceWeek = projectInvoiceWeekGroupedByEmployeeId.FirstOrDefault();
                if (firstProjectInvoiceWeek == null)
                    continue;

                Employee employee = firstProjectInvoiceWeek.Employee;
                if (employee == null)
                    continue;

                var employeeInfo = new TimeEmployeeInfo
                {
                    EmployeeNr = employee.EmployeeNr,
                    Name = employee.Name
                };

                foreach (ProjectInvoiceWeek projectInvoiceWeek in projectInvoiceWeekGroupedByEmployeeId)
                {
                    var projectInvoiceDays = ProjectManager.GetProjectInvoiceDays(entities, projectInvoiceWeek.ProjectInvoiceWeekId, fromDate, toDate, true);
                    TimeCode timeCode = null;
                    if (projectInvoiceWeek.TimeCodeId.HasValue)
                        timeCode = timeCodes.FirstOrDefault(x => x.TimeCodeId == projectInvoiceWeek.TimeCodeId.Value);

                    result.TotalRows += projectInvoiceDays.Count(p => p.InvoiceTimeInMinutes > 0);

                    foreach (ProjectInvoiceDay projectInvoiceDay in projectInvoiceDays)
                    {
                        var timeCodeTransaction = projectInvoiceDay.TimeCodeTransaction.FirstOrDefault(t => t.State == (int)SoeEntityState.Active && t.TimeInvoiceTransaction.Any(i => i.State == (int)SoeEntityState.Active));
                        var invoiceTimeInMinutes = timeCodeTransaction != null && timeCodeTransaction.TimeInvoiceTransaction.Any() ? projectInvoiceDay.InvoiceTimeInMinutes : 0;

                        var timeEntry = new TimeEntryInfo
                        {
                            TimeCodeCode = timeCode != null ? timeCode.Code : string.Empty,
                            TimeCodeName = timeCode != null ? timeCode.Name : string.Empty,
                            InvoiceTimeInMinutes = invoiceTimeInMinutes,
                            Date = projectInvoiceDay.Date,
                            Note = projectInvoiceDay.Note,
                            ExternalNote = string.Empty,
                            TimeDeviationCauseName = string.Empty,
                            StartTime = string.Empty,
                            StopTime = string.Empty
                        };

                        employeeInfo.TimeEntries.Add(timeEntry);
                    }
                }

                projectInfo.Employees.Add(employeeInfo);
            }
        }

        private TimeEntryInfo CreateTimeEntryFromProjectTimeBlock(ProjectTimeBlock projectTimeBlock, List<TimeCode> timeCodes, int actorCompanyId, bool showStartStopInTimeReport)
        {
            TimeCodeTransaction timeCodeTransaction = projectTimeBlock.TimeCodeTransaction.FirstOrDefault(tct => tct.Type == (int)TimeCodeTransactionType.TimeProject);
            TimeCode timeCode = timeCodeTransaction != null ? timeCodes?.FirstOrDefault(x => x.TimeCodeId == timeCodeTransaction.TimeCodeId) : null;

            if (timeCode == null && timeCodeTransaction != null)
            {
                timeCode = TimeCodeManager.GetTimeCode(timeCodeTransaction.TimeCodeId, actorCompanyId, false);
            }

            TimeDeviationCause timeDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(projectTimeBlock.TimeDeviationCauseId, actorCompanyId, false);
            string timeDeviationCauseName = timeDeviationCause?.Name ?? string.Empty;

            return new TimeEntryInfo
            {
                TimeCodeCode = timeCode?.Code ?? string.Empty,
                TimeCodeName = timeCode?.Name ?? string.Empty,
                InvoiceTimeInMinutes = projectTimeBlock.InvoiceQuantity,
                Date = projectTimeBlock.TimeBlockDate?.Date ?? DateTime.MinValue,
                Note = projectTimeBlock.ExternalNote,
                ExternalNote = string.Empty,
                TimeDeviationCauseName = timeDeviationCauseName,
                StartTime = showStartStopInTimeReport ? projectTimeBlock.StartTime.ToShortTimeString() : string.Empty,
                StopTime = showStartStopInTimeReport ? projectTimeBlock.StopTime.ToShortTimeString() : string.Empty
            };
        }

        private void ProcessCustomerInvoiceRow(CompEntities entities, CustomerInvoiceRow row, TimeProjectReportParams timeParams, List<Project> projects)
        {
            List<CustomerInvoiceRow> parentRows = InvoiceManager.GetParentCustomerInvoiceRows(entities, row.CustomerInvoiceRowId, timeParams.ActorCompanyId);

            if (parentRows.Count > 0)
            {
                foreach (CustomerInvoiceRow parentRow in parentRows)
                {
                    ProcessTransactionsForRow(entities, parentRow.CustomerInvoiceRowId, timeParams, projects);
                }
            }
            else
            {
                ProcessTransactionsForRow(entities, row.CustomerInvoiceRowId, timeParams, projects);
            }
        }

        private void ProcessOrderInvoiceRow(CompEntities entities, CustomerInvoiceRow row, List<AttestTransition> attestTransitions, int attestStateTransferredOrderToInvoiceId, TimeProjectReportParams timeParams, List<Project> projects)
        {
            var rowHasStateToInvoice = attestTransitions.IsNullOrEmpty() ||
                attestTransitions.Any(x => x.AttestStateFromId == row.AttestStateId && x.AttestStateToId == attestStateTransferredOrderToInvoiceId);

            if (!rowHasStateToInvoice && timeParams.IncludeOnlyInvoiced)
                return;

            ProcessTransactionsForRow(entities, row.CustomerInvoiceRowId, timeParams, projects);
        }

        private void ProcessTransactionsForRow(CompEntities entities, int customerInvoiceRowId, TimeProjectReportParams timeParams, List<Project> projects)
        {
            List<TimeInvoiceTransaction> transactions = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, customerInvoiceRowId);
            foreach (TimeInvoiceTransaction transaction in transactions)
            {
                ProcessTimeInvoiceTransaction(entities, transaction, timeParams, projects);
            }
        }

        private void ConvertProjectsToReportData(List<Project> projects, string invoiceNr, CompEntities entities, TimeProjectReportParams timeParams, List<TimeCode> timeCodes, bool showStartStopInTimeReport, TimeProjectReportData result)
        {
            foreach (Project project in projects)
            {
                var projectInfo = new TimeProjectInfo
                {
                    Number = project.Number,
                    Name = project.Name,
                    Description = project.Description,
                    InvoiceNr = invoiceNr,
                    Created = project.Created,
                    CreatedBy = project.CreatedBy,
                    State = project.State,
                    WorkSiteKey = project.WorkSiteKey,
                    WorkSiteNumber = project.WorkSiteNumber
                };

                List<Employee> employees = new List<Employee>();
                employees.AddRange(project.Employees ?? new List<Employee>());

                foreach (Employee employee in employees)
                {
                    if (!employee.ContactPersonReference.IsLoaded)
                        employee.ContactPersonReference.Load();

                    var employeeInfo = new TimeEmployeeInfo
                    {
                        EmployeeNr = employee.EmployeeNr,
                        Name = employee.Name
                    };

                    List<TimeInvoiceTransaction> transactions = new List<TimeInvoiceTransaction>();
                    transactions.AddRange(employee.Transactions.Where(t => t.EmployeeId == employee.EmployeeId && t.TimeCodeTransaction?.ProjectId == project.ProjectId));

                    if (timeParams.HasDateInterval)
                    {
                        if (timeParams.DateFrom.HasValue)
                        {
                            transactions = transactions.Where(t => t.TimeBlockDateId != null && t.TimeBlockDateId != 0 && t.TimeBlockDate.Date >= timeParams.DateFrom.Value).ToList();
                        }

                        if (timeParams.DateTo.HasValue)
                        {
                            transactions = transactions.Where(t => t.TimeBlockDateId != null && t.TimeBlockDateId != 0 && t.TimeBlockDate.Date <= timeParams.DateTo.Value).ToList();
                        }
                    }

                    result.TotalRows += transactions.GroupBy(x => x.TimeCodeTransactionId).Count();

                    foreach (var invoiceTransactions in transactions.GroupBy(x => x.TimeCodeTransactionId))
                    {
                        var transaction = invoiceTransactions.First();

                        if (!transaction.TimeCodeTransactionReference.IsLoaded)
                            transaction.TimeCodeTransactionReference.Load();

                        var timeCodeTransaction = transaction.TimeCodeTransaction;

                        if (timeCodeTransaction == null)
                            continue;

                        TimeEntryInfo timeEntry = null;

                        if (timeCodeTransaction.ProjectTimeBlockId.HasValue && timeCodeTransaction.ProjectTimeBlockId > 0)
                        {
                            var projectTimeBlock = timeCodeTransaction.ProjectTimeBlock != null ? timeCodeTransaction.ProjectTimeBlock : ProjectManager.GetProjectTimeBlock(entities, (int)timeCodeTransaction.ProjectTimeBlockId);
                            if (projectTimeBlock != null)
                            {
                                timeEntry = CreateTimeEntryFromProjectTimeBlock(projectTimeBlock, timeCodes, timeParams.ActorCompanyId, showStartStopInTimeReport);
                            }
                        }
                        else if (transaction.TimeCodeTransaction.ProjectInvoiceDay != null)
                        {
                            timeEntry = new TimeEntryInfo
                            {
                                TimeCodeCode = timeCodeTransaction.TimeCode != null ? timeCodeTransaction.TimeCode.Code : string.Empty,
                                TimeCodeName = timeCodeTransaction.TimeCode != null ? timeCodeTransaction.TimeCode.Name : string.Empty,
                                InvoiceTimeInMinutes = timeCodeTransaction.ProjectInvoiceDay.InvoiceTimeInMinutes,
                                Date = timeCodeTransaction.ProjectInvoiceDay.Date,
                                Note = timeCodeTransaction.ProjectInvoiceDay.Note,
                                ExternalNote = timeCodeTransaction.Comment,
                                TimeDeviationCauseName = string.Empty,
                                StartTime = string.Empty,
                                StopTime = string.Empty
                            };
                        }

                        if (timeEntry != null)
                        {
                            employeeInfo.TimeEntries.Add(timeEntry);
                        }
                    }

                    if (employeeInfo.TimeEntries.Count > 0)
                    {
                        projectInfo.Employees.Add(employeeInfo);
                    }
                }

                result.Projects.Add(projectInfo);
            }
        }

        private void DetachProjectEntities(CompEntities entities, List<Project> projects)
        {
            foreach (var project in projects)
            {
                if (project.Employees != null)
                {
                    foreach (var employee in project.Employees)
                    {
                        if (employee.Transactions != null)
                        {
                            foreach (var transaction in employee.Transactions)
                            {
                                StateUtility.DetachObject(transaction);
                            }
                        }
                        StateUtility.DetachObject(employee);
                    }
                }
                StateUtility.DetachObject(project);
            }
        }

        private void ProcessTimeInvoiceTransaction(CompEntities entities, TimeInvoiceTransaction invTransaction, TimeProjectReportParams timeParams, List<Project> projects)
        {
            if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                invTransaction.TimeCodeTransactionReference.Load();

            if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                return;

            Project project = projects.FirstOrDefault(p => p.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);
            bool addProject = false;

            if (project == null)
            {
                addProject = true;
                project = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
            }

            if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

            if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

            if (timeParams.HasDateInterval && !invTransaction.TimeBlockDateReference.IsLoaded)
                invTransaction.TimeBlockDateReference.Load();

            if (project.Employees == null)
                project.Employees = new List<Employee>();

            Employee employee = project.Employees.FirstOrDefault(e => e.EmployeeId == (int)invTransaction.EmployeeId);
            bool addEmployee = false;

            if (employee == null)
            {
                if (!invTransaction.EmployeeReference.IsLoaded)
                    invTransaction.EmployeeReference.Load();

                addEmployee = true;
                employee = invTransaction.Employee;
            }

            if (employee.Transactions == null)
                employee.Transactions = new List<TimeInvoiceTransaction>();

            employee.Transactions.Add(invTransaction);

            if (addEmployee)
                project.Employees.Add(employee);

            if (addProject)
                projects.Add(project);
        }
    }

    internal class TimeProjectXmlGenerator
    {
        public XElement Generate(TimeProjectReportData data)
        {
            if (data.Projects.Count == 0)
            {
                return GenerateDefaultProjectXml();
            }

            XElement timeProjectReportElement = null;
            int projectXmlId = 1;

            foreach (var projectInfo in data.Projects)
            {
                var projectElement = GenerateProjectXml(projectInfo, projectXmlId);

                if (timeProjectReportElement == null)
                    timeProjectReportElement = new XElement(projectElement);
                else
                    timeProjectReportElement.Add(projectElement);

                projectXmlId++;
            }

            return timeProjectReportElement;
        }

        private XElement GenerateProjectXml(TimeProjectInfo projectInfo, int projectXmlId)
        {
            var projectElement = new XElement("Project",
                new XAttribute("id", projectXmlId),
                new XElement("ProjectNumber", projectInfo.Number),
                new XElement("ProjectName", projectInfo.Name),
                new XElement("ProjectDescription", projectInfo.Description),
                new XElement("ProjectInvoiceNr", projectInfo.InvoiceNr),
                new XElement("ProjectCreated", projectInfo.Created.HasValue ? projectInfo.Created.Value.ToShortDateString() : ""),
                new XElement("ProjectCreatedBy", projectInfo.CreatedBy),
                new XElement("ProjectState", projectInfo.State),
                new XElement("ProjectWorkSiteId", projectInfo.WorkSiteKey),
                new XElement("ProjectWorkSiteNumber", projectInfo.WorkSiteNumber));

            int employeeXmlId = 1;

            if (projectInfo.Employees.Count == 0)
            {
                // Add default employee element
                projectElement.Add(GenerateDefaultEmployeeXml());
            }
            else
            {
                foreach (var employeeInfo in projectInfo.Employees)
                {
                    var employeeElement = GenerateEmployeeXml(employeeInfo, employeeXmlId);
                    projectElement.Add(employeeElement);
                    employeeXmlId++;
                }
            }

            return projectElement;
        }

        private XElement GenerateEmployeeXml(TimeEmployeeInfo employeeInfo, int employeeXmlId)
        {
            var employeeElement = new XElement("Employee",
                new XAttribute("id", employeeXmlId),
                new XElement("EmployeeNr", employeeInfo.EmployeeNr),
                new XElement("EmployeeName", employeeInfo.Name));

            int timeEntryXmlId = 1;

            foreach (var timeEntry in employeeInfo.TimeEntries)
            {
                var dayElement = GenerateTimeEntryXml(timeEntry, timeEntryXmlId);
                employeeElement.Add(dayElement);
                timeEntryXmlId++;
            }

            return employeeElement;
        }

        private XElement GenerateTimeEntryXml(TimeEntryInfo timeEntry, int timeEntryXmlId)
        {
            var dayElement = new XElement("ProjectInvoiceDay",
                new XAttribute("id", timeEntryXmlId),
                new XElement("TCCode", timeEntry.TimeCodeCode),
                new XElement("TCName", timeEntry.TimeCodeName),
                new XElement("InvoiceTimeInMinutes", timeEntry.InvoiceTimeInMinutes),
                new XElement("Date", timeEntry.Date != DateTime.MinValue ? timeEntry.Date.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToString()),
                new XElement("Note", timeEntry.Note),
                new XElement("ExternalNote", timeEntry.ExternalNote),
                new XElement("IsoDate", timeEntry.Date != DateTime.MinValue ? timeEntry.Date.ToString("yyyy-MM-dd") : DateTime.Now.Date.ToString("yyyy-MM-dd")));

            // Add optional fields if they're present
            if (!string.IsNullOrEmpty(timeEntry.TimeDeviationCauseName))
            {
                dayElement.Add(new XElement("TDName", timeEntry.TimeDeviationCauseName));
            }

            if (!string.IsNullOrEmpty(timeEntry.StartTime) || !string.IsNullOrEmpty(timeEntry.StopTime))
            {
                dayElement.Add(new XElement("TBStartTime", timeEntry.StartTime));
                dayElement.Add(new XElement("TBStopTime", timeEntry.StopTime));
            }

            return dayElement;
        }

        private XElement GenerateDefaultProjectXml()
        {
            var projectElement = new XElement("Project",
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

            projectElement.Add(GenerateDefaultEmployeeXml());

            return projectElement;
        }

        private XElement GenerateDefaultEmployeeXml()
        {
            var defaultEmployeeElement = new XElement("Employee",
                new XAttribute("id", 1),
                new XElement("EmployeeNr", 0),
                new XElement("EmployeeName", ""));

            var defaultDayElement = new XElement("ProjectInvoiceDay",
                new XAttribute("id", 1),
                new XElement("InvoiceTimeInMinutes", 0),
                new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                new XElement("Note", "00:00"),
                new XElement("ExternalNote", string.Empty),
                new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

            defaultEmployeeElement.Add(defaultDayElement);

            return defaultEmployeeElement;
        }
    }

    #region DTOs for separating data gathering from XML generation

    internal class TimeProjectReportData
    {
        public List<TimeProjectInfo> Projects { get; set; }
        public int TotalRows { get; set; }

        public TimeProjectReportData()
        {
            Projects = new List<TimeProjectInfo>();
            TotalRows = 0;
        }
    }

    internal class TimeProjectInfo
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string InvoiceNr { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public int State { get; set; }
        public string WorkSiteKey { get; set; }
        public string WorkSiteNumber { get; set; }
        public List<TimeEmployeeInfo> Employees { get; set; }

        public TimeProjectInfo()
        {
            Employees = new List<TimeEmployeeInfo>();
        }
    }

    internal class TimeEmployeeInfo
    {
        public string EmployeeNr { get; set; }
        public string Name { get; set; }
        public List<TimeEntryInfo> TimeEntries { get; set; }

        public TimeEmployeeInfo()
        {
            TimeEntries = new List<TimeEntryInfo>();
        }
    }

    internal class TimeEntryInfo
    {
        public string TimeCodeCode { get; set; }
        public string TimeCodeName { get; set; }
        public int InvoiceTimeInMinutes { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public string ExternalNote { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public string StartTime { get; set; }
        public string StopTime { get; set; }
    }

    internal class TimeProjectReportParams
    {
        public int ActorCompanyId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IncludeOnlyInvoiced { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public bool HasDateInterval => DateFrom.HasValue || DateTo.HasValue;
        public TimeProjectReportParams(int actorCompanyId, int userId, int roleId, bool includeOnlyInvoiced, DateTime? dateFrom, DateTime? dateTo)
        {
            ActorCompanyId = actorCompanyId;
            UserId = userId;
            RoleId = roleId;
            IncludeOnlyInvoiced = includeOnlyInvoiced;
            DateFrom = dateFrom.HasValue && dateFrom != CalendarUtility.DATETIME_DEFAULT ? dateFrom : null;
            DateTo = dateTo.HasValue && dateTo != CalendarUtility.DATETIME_DEFAULT ? dateTo : null;
        }
    }

    #endregion
}

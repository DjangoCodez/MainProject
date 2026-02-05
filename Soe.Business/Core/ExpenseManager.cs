using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace SoftOne.Soe.Business.Core
{
    public class ExpenseManager : ManagerBase
    {
        #region Variables

        #endregion

        #region Ctor

        public ExpenseManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region ExpenseRow

        public int GetExpenseRowsCount(int actorCompanyId, int employeeId, DateTime fromDate, DateTime toDate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExpenseRow.NoTracking();
            return GetExpenseRowsCount(entities, actorCompanyId, employeeId, fromDate, toDate);
        }

        public int GetExpenseRowsCount(CompEntities entities, int actorCompanyId, int employeeId, DateTime fromDate, DateTime toDate)
        {
            return (from er in entities.ExpenseRow
                    where er.ActorCompanyId == actorCompanyId &&
                    er.EmployeeId == employeeId &&
                    er.ExpenseHead.Start >= fromDate &&
                    er.ExpenseHead.Stop <= toDate &&
                    er.State == (int)SoeEntityState.Active
                    select er).Count();
        }

        public List<ExpenseRowGridDTO> GetExpenseRowsForGrid(int customerInvoiceId, int actorCompanyId, int userId, int roleId, bool includeComments = false, int customerInvoiceRowId = 0, bool checkFiles = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExpenseRow.NoTracking();
            return GetExpenseRowsForGrid(entities, customerInvoiceId, actorCompanyId, userId, roleId, includeComments, customerInvoiceRowId, checkFiles);
        }

        public List<ExpenseRowGridDTO> GetExpenseRowsForGrid(CompEntities entities, int customerInvoiceId, int actorCompanyId, int userId, int roleId, bool includeComments = false, int customerInvoiceRowId = 0, bool checkFiles = false)
        {
            bool hasOtherEmployeesPermission = FeatureManager.HasRolePermission(Feature.Billing_Project_TimeSheetUser_OtherEmployees, Permission.Modify, roleId, actorCompanyId);

            IQueryable<ExpenseRowTransactionView> query = (from e in entities.ExpenseRowTransactionView
                                                           where e.CustomerInvoiceId == customerInvoiceId && 
                                                                 e.ActorCompanyId == actorCompanyId
                                                           orderby e.Date descending, e.TimeCodeName
                                                           select e);
            if (customerInvoiceRowId > 0)
            {
                query = query.Where(e => e.CustomerInvoiceRowId == customerInvoiceRowId);
            }

            if (!hasOtherEmployeesPermission)
            {
                int employeeId = EmployeeManager.GetEmployeeIdForUser(userId, actorCompanyId);
                query = query.Where(x => x.EmployeeId == employeeId);
            }

            return CreateExpenseRowGridDTOs(query.ToList(), includeComments, checkFiles);
        }

        public List<ExpenseRowProjectOverviewDTO> GetExpenseRowsForProjectOverview(int actorCompanyId, int userId, int roleId, int? customerInvoiceId = 0, int? projectId = 0)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExpenseRow.NoTracking();
            return GetExpenseRowsForProjectOverview(entities, actorCompanyId, userId, roleId, customerInvoiceId, projectId);
        }

        public List<ExpenseRowProjectOverviewDTO> GetExpenseRowsForProjectOverview(CompEntities entities, int actorCompanyId, int userId, int roleId, int? customerInvoiceId = 0, int? projectId = 0)
        {
            if (customerInvoiceId == 0 && projectId == 0)
                throw new ArgumentException("Either customerInvoiceId or projectId must be provided.");

            bool hasOtherEmployeesPermission = FeatureManager.HasRolePermission(Feature.Billing_Project_TimeSheetUser_OtherEmployees, Permission.Modify, roleId, actorCompanyId);

            IQueryable<ExpenseRowTransactionView> query = (from e in entities.ExpenseRowTransactionView
                                                           where e.ActorCompanyId == actorCompanyId
                                                           orderby e.Date descending, e.TimeCodeName
                                                           select e);
            if(projectId > 0)
                query = query.Where(e => e.ProjectId == projectId);

            if (customerInvoiceId > 0)
                query = query.Where(e => e.CustomerInvoiceId == customerInvoiceId);

            if (!hasOtherEmployeesPermission)
            {
                int employeeId = EmployeeManager.GetEmployeeIdForUser(userId, actorCompanyId);
                query = query.Where(x => x.EmployeeId == employeeId);
            }

            var result = query.Select(e => new ExpenseRowProjectOverviewDTO
            {
                ExpenseRowId = e.ExpenseRowId,
                From = e.Date,
                EmployeeId = e.EmployeeId,
                EmployeeName = e.EmployeeName,
                Quantity = e.Quantity,
                Amount = e.Amount,
                PayrollAmount = e.PayRollAmount,
                TimeCodeRegistrationType = e.RegistrationType,
                TimeCodeName = e.TimeCodeName,  
                IsSpecifiedUnitPrice = e.IsSpecifiedUnitPrice,
            }).ToList();

            List<ExpenseRowProjectOverviewDTO> expenseRows = new List<ExpenseRowProjectOverviewDTO>();

            foreach(var group in result.GroupBy(e => e.ExpenseRowId))
            {
                ExpenseRowProjectOverviewDTO current = null;
                foreach(var expenseRow in group)
                {
                    if (current == null)
                    {
                        current = expenseRow;
                    }
                    else
                    {
                        current.PayrollAmount += expenseRow.PayrollAmount;
                    }
                }
                expenseRows.Add(current);
            }

            return expenseRows;
            //return CreateExpenseRowGridDTOs(query.ToList(), includeComments, checkFiles);
        }

        public List<ExpenseRowForReportDTO> GetExpenseRowsForReport(int customerInvoiceId, int actorCompanyId, int userId, int roleId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExpenseRow.NoTracking();
            return GetExpenseRowsForReport(entities, customerInvoiceId, actorCompanyId, userId, roleId);
        }

        public List<ExpenseRowForReportDTO> GetExpenseRowsForReport(CompEntities entities, int customerInvoiceId, int actorCompanyId, int userId, int roleId)
        {
            bool hasOtherEmployeesPermission = FeatureManager.HasRolePermission(Feature.Billing_Project_TimeSheetUser_OtherEmployees, Permission.Modify, roleId, actorCompanyId);

            IQueryable<ExpenseRowTransactionView> query = (from e in entities.ExpenseRowTransactionView
                                                           where e.CustomerInvoiceId == customerInvoiceId &&
                                                                 e.ActorCompanyId == actorCompanyId
                                                           orderby e.Date descending, e.TimeCodeName
                                                           select e);

            if (!hasOtherEmployeesPermission)
            {
                int employeeId = EmployeeManager.GetEmployeeIdForUser(userId, actorCompanyId);
                query = query.Where(x => x.EmployeeId == employeeId);
            }

            var expenseGroups = query.Select(e => new ExpenseRowForReportDTO
            {
                ExpenseRowId = e.ExpenseRowId,
                From = e.Date,
                EmployeeName = e.EmployeeName,
                Quantity = e.Quantity,
                Amount = e.Amount,
                TimeCodeRegistrationType = e.RegistrationType,
                TimeCodeName = e.TimeCodeName,
                TimeCodeId = e.TimeCodeId,
                TimePayrollTransactionId = e.TimePayrollTransactionId,
                PayrollAttestStateName = e.PayrollAttestStateName,
                InvoicedAmount = e.InvoicedAmount.HasValue ? e.InvoicedAmount.Value : 0,
                EmployeeNumber = e.EmployeeNr,
                PayRollAmount = e.PayRollAmount,
            }).GroupBy(i => i.ExpenseRowId).ToList();

            List<ExpenseRowForReportDTO> expenseRows = new List<ExpenseRowForReportDTO>();
            foreach (var group in expenseGroups)
            {
                var model = group.FirstOrDefault();
                if (model == null)
                    continue;

                var timePayrollTransaction = group.FirstOrDefault(i => i.TimePayrollTransactionId != 0);
                if (timePayrollTransaction != null)
                {
                    model.PayrollAttestStateName = timePayrollTransaction.PayrollAttestStateName;
                    model.Amount = model.Amount == 0 ? timePayrollTransaction.PayRollAmount : model.Amount;
                }

                expenseRows.Add(model);
            }

            return expenseRows;
        }

        public ExpenseRowGridDTO CreateExpenseRowGridDTO(IGrouping<int, ExpenseRowTransactionView> group, bool includeComments)
        {
            ExpenseRowTransactionView model = group.FirstOrDefault();

            var dto = new ExpenseRowGridDTO
            {
                ExpenseRowId = model.ExpenseRowId,
                ExpenseHeadId = model.ExpenseHeadId,
                EmployeeId = model.EmployeeId,
                EmployeeNumber = model.EmployeeNr,
                EmployeeName = model.EmployeeName,
                TimeCodeId = model.TimeCodeId,
                TimeCodeName = model.TimeCodeName,
                TimeCodeRegistrationType = model.RegistrationType,
                From = model.Date,
                Quantity = model.Quantity,
                Amount = model.Amount,
                AmountCurrency = model.AmountCurrency,
                Vat = model.Vat,
                VatCurrency = model.VatCurrency,
                AmountExVat = model.Amount - model.Vat,
                InvoicedAmount = model.InvoicedAmount.HasValue ? model.InvoicedAmount.Value : 0,
                InvoicedAmountCurrency = model.InvoicedAmountCurrency.HasValue ? model.InvoicedAmountCurrency.Value : 0,
                IsSpecifiedUnitPrice = model.IsSpecifiedUnitPrice,
                Comment = includeComments ? model.Comment : null,
                ExternalComment = includeComments ? model.ExternalComment : null,
            };

            if (model.ProjectId > 0)
            {
                dto.ProjectId = model.ProjectId;
                dto.ProjectNr = model.ProjectNr;
                dto.ProjectName = model.ProjectName;
            }

            if (model.CustomerInvoiceId.GetValueOrDefault() > 0)
            {
                dto.OrderId = model.CustomerInvoiceId.Value;
                dto.OrderNr = model.OrderNr.ToString();
            }

            if (model.ActorCustomerId > 0)
            {
                dto.ActorCustomerId = model.ActorCustomerId;
                dto.CustomerName = model.CustomerName;
            }

            // Time invoice transaction
            var timeInvoiceModel = group.FirstOrDefault(i => i.TimeInvoiceTransactionId != 0);
            if (timeInvoiceModel != null)
            {
                dto.InvoiceRowAttestStateId = timeInvoiceModel.InvoiceAttestStateId;
                dto.InvoiceRowAttestStateColor = timeInvoiceModel.InvoiceAttestStateColor;
                dto.InvoiceRowAttestStateName = timeInvoiceModel.InvoiceAttestStateName;
            }

            // Time payroll transaction
            var timePayrollTransaction = group.FirstOrDefault(i => i.TimePayrollTransactionId != 0);
            if (timePayrollTransaction != null)
            {
                dto.PayrollAttestStateId = timePayrollTransaction.PayrollAttestStateId.HasValue ? timePayrollTransaction.PayrollAttestStateId.Value : 0;
                dto.PayrollAttestStateColor = timePayrollTransaction.PayrollAttestStateColor;
                dto.PayrollAttestStateName = timePayrollTransaction.PayrollAttestStateName;
                dto.Amount = dto.Amount == 0 ? timePayrollTransaction.PayRollAmount : dto.Amount;
                dto.AmountExVat = (dto.AmountExVat == 0) ? timePayrollTransaction.PayRollAmount - timePayrollTransaction.Vat : dto.AmountExVat;
                dto.AmountCurrency = dto.AmountCurrency == 0 ? timePayrollTransaction.PayRollAmountCurrency : dto.AmountCurrency;

                dto.TimePayrollTransactionIds = group.Where(i => i.TimePayrollTransactionId != 0).Select(i => i.TimePayrollTransactionId).ToList();
            }

            return dto;
        }

        public List<ExpenseRowGridDTO> GetExpenseRowsForGridFiltered(int actorCompanyId, int userId, int roleId, int employeeId, DateTime fromDate, DateTime toDate, List<int> employees, List<int> projects, List<int> orders, List<int> employeeCategories)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExpenseRow.NoTracking();
            return GetExpenseRowsForGridFiltered(entities, actorCompanyId, userId, roleId, employeeId, fromDate, toDate, employees, projects, orders, employeeCategories);
        }

        public List<ExpenseRowGridDTO> GetExpenseRowsForGridFiltered(CompEntities entities, int actorCompanyId, int userId, int roleId, int employeeId, DateTime fromDate, DateTime toDate, List<int> employees, List<int> projects, List<int> orders, List<int> employeeCategories)
        {
            bool hasOtherEmployeesPermission = FeatureManager.HasRolePermission(Feature.Billing_Project_TimeSheetUser_OtherEmployees, Permission.Modify, roleId, actorCompanyId);

            IQueryable<ExpenseRowTransactionView> query = (from e in entities.ExpenseRowTransactionView
                                                           where e.Date >= fromDate && e.Date <= toDate &&
                                                           e.ActorCompanyId == actorCompanyId
                                                           orderby e.Date descending, e.TimeCodeName
                                                           select e) ;
            if (employees != null && employees.Count > 0)
                query = query.Where(e => employees.Contains(e.EmployeeId));

            if (projects != null && projects.Count > 0)
                query = query.Where(e => projects.Contains(e.ProjectId));

            if (orders != null && orders.Count > 0)
                query = query.Where(e => orders.Contains(e.CustomerInvoiceId.Value));

            if (!hasOtherEmployeesPermission)
                query = query.Where(x => x.EmployeeId == employeeId);

            return CreateExpenseRowGridDTOs(query.ToList(), false, false);
        }

        public List<ExpenseRowGridDTO> CreateExpenseRowGridDTOs(List<ExpenseRowTransactionView> rows, bool includeComments, bool checkFiles)
        {
            var dtos = new List<ExpenseRowGridDTO>();

            foreach (var expenseRowGroup in rows.GroupBy(i => i.ExpenseRowId))
            {
                var model = expenseRowGroup.FirstOrDefault();
                if (model == null)
                    continue;

                var dto = new ExpenseRowGridDTO
                {
                    ExpenseRowId = model.ExpenseRowId,
                    ExpenseHeadId = model.ExpenseHeadId,
                    EmployeeId = model.EmployeeId,
                    EmployeeNumber = model.EmployeeNr,
                    EmployeeName = model.EmployeeName,
                    TimeCodeId = model.TimeCodeId,
                    TimeCodeName = model.TimeCodeName,
                    TimeCodeRegistrationType = model.RegistrationType,
                    From = model.Date,
                    Quantity = model.Quantity,
                    Amount = model.Amount,
                    AmountCurrency = model.AmountCurrency,
                    Vat = model.Vat,
                    VatCurrency = model.VatCurrency,
                    AmountExVat = model.Amount - model.Vat,
                    InvoicedAmount = model.InvoicedAmount.HasValue ? model.InvoicedAmount.Value : 0,
                    InvoicedAmountCurrency = model.InvoicedAmountCurrency.HasValue ? model.InvoicedAmountCurrency.Value : 0,
                    IsSpecifiedUnitPrice = model.IsSpecifiedUnitPrice,
                    Comment = includeComments ? model.Comment : null,
                    ExternalComment = includeComments ? model.ExternalComment : null,
                };

                if (model.ProjectId > 0)
                {
                    dto.ProjectId = model.ProjectId;
                    dto.ProjectNr = model.ProjectNr;
                    dto.ProjectName = model.ProjectName;
                }

                if (model.CustomerInvoiceId.GetValueOrDefault() > 0)
                {
                    dto.OrderId = model.CustomerInvoiceId.Value;
                    dto.OrderNr = model.OrderNr.ToString();
                }

                if (model.ActorCustomerId > 0)
                {
                    dto.ActorCustomerId = model.ActorCustomerId;
                    dto.CustomerName = model.CustomerName;
                }

                // Time invoice transaction
                var timeInvoiceModel = expenseRowGroup.FirstOrDefault(i => i.TimeInvoiceTransactionId != 0);
                if (timeInvoiceModel != null)
                {
                    dto.InvoiceRowAttestStateId = timeInvoiceModel.InvoiceAttestStateId;
                    dto.InvoiceRowAttestStateColor = timeInvoiceModel.InvoiceAttestStateColor;
                    dto.InvoiceRowAttestStateName = timeInvoiceModel.InvoiceAttestStateName;
                }

                // Time payroll transaction
                var timePayrollTransaction = expenseRowGroup.FirstOrDefault(i => i.TimePayrollTransactionId != 0);
                if (timePayrollTransaction != null)
                {
                    dto.PayrollAttestStateId = timePayrollTransaction.PayrollAttestStateId.HasValue ? timePayrollTransaction.PayrollAttestStateId.Value : 0;
                    dto.PayrollAttestStateColor = timePayrollTransaction.PayrollAttestStateColor;
                    dto.PayrollAttestStateName = timePayrollTransaction.PayrollAttestStateName;
                    dto.Amount = dto.Amount == 0 ? timePayrollTransaction.PayRollAmount : dto.Amount;
                    dto.AmountExVat = (dto.AmountExVat == 0) ? timePayrollTransaction.PayRollAmount - timePayrollTransaction.Vat : dto.AmountExVat;
                    dto.AmountCurrency = dto.AmountCurrency == 0 ? timePayrollTransaction.PayRollAmountCurrency : dto.AmountCurrency;
                    dto.PayrollTransactionDate = timePayrollTransaction.TransactionDate;

                    dto.TimePayrollTransactionIds = expenseRowGroup.Where(i => i.TimePayrollTransactionId != 0).Select(i => i.TimePayrollTransactionId).ToList();
                }

                dto.HasFiles = checkFiles && GeneralManager.HasDataStorageRecords(ActorCompanyId, SoeDataStorageRecordType.Expense, SoeEntityType.Expense, dto.ExpenseRowId); 

                dtos.Add(dto);
            }

            return dtos;
        }

        public ExpenseRow GetExpenseRow(int expenseRowId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExpenseRow.NoTracking();
            return GetExpenseRow(entities, expenseRowId);
        }

        public ExpenseRow GetExpenseRow(CompEntities entities, int expenseRowId)
        {
            return (from er in entities.ExpenseRow
                    .Include("ExpenseHead")
                    .Include("CustomerInvoiceRow")
                    .Include("TimeCodeTransaction.TimePayrollTransaction")
                    where er.ExpenseRowId == expenseRowId &&
                    er.State == (int)SoeEntityState.Active
                    select er).FirstOrDefault();
        }

        public ExpenseRowDTO GetExpenseRowForDialog(int expenseRowId, bool checkFiles = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExpenseRow.NoTracking();
            return GetExpenseRowForDialog(entities, expenseRowId, checkFiles);
        }

        public ExpenseRowDTO GetExpenseRowForDialog(CompEntities entities, int expenseRowId, bool checkFiles = false)
        {
            var expenseRow = (from er in entities.ExpenseRow
                            .Include("ExpenseHead")
                            .Include("CustomerInvoiceRow")
                            .Include("TimeCodeTransaction.TimePayrollTransaction")
                              where er.ExpenseRowId == expenseRowId &&
                              er.State == (int)SoeEntityState.Active
                              select er).FirstOrDefault();

            if (expenseRow == null)
                return null;

            var dto = expenseRow.ToDTO();
            dto.Start = expenseRow.ExpenseHead.Start ?? expenseRow.Start;
            dto.Stop = expenseRow.ExpenseHead.Stop ?? expenseRow.Stop;
            dto.TransferToOrder = expenseRow.InvoicedAmountCurrency != 0;

            if (expenseRow.CustomerInvoiceRow != null)
            {
                int initialAttestStateId = AttestManager.GetInitialAttestStateId(base.ActorCompanyId, TermGroup_AttestEntity.Order);
                dto.isReadOnly = expenseRow.CustomerInvoiceRow.AttestStateId != initialAttestStateId;
            }
            if (expenseRow.TimeCodeTransaction?.TimePayrollTransaction != null)
            {
                int initialAttestStateId = AttestManager.GetInitialAttestStateId(base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
                dto.isTimeReadOnly = expenseRow.TimeCodeTransaction.TimePayrollTransaction.Any(a => a.AttestStateId != initialAttestStateId);
            }

            dto.HasFiles = checkFiles && GeneralManager.HasDataStorageRecords(ActorCompanyId, SoeDataStorageRecordType.Expense, SoeEntityType.Expense, expenseRowId);

            return dto;
        }


        public bool HasExpenseRows(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExpenseRow.NoTracking();
            return (from er in entities.ExpenseRow
                    where er.ActorCompanyId == actorCompanyId &&
                          er.State == (int)SoeEntityState.Active
                    select er).Any();
        }

        #endregion
    }
}

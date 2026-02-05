using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class TimeTransactionMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "timeTransactionMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<TimeTransactionReportDataReportDataField> filter { get; set; }
        private TimeTransactionReportDataOutput _input { get; set; }

        #endregion

        public TimeTransactionMatrix(InputMatrix inputMatrix, TimeTransactionReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            AccountDims = inputMatrix.AccountDims;
            AccountInternals = inputMatrix.AccountInternals;
            filter = reportDataOutput?.Input?.Columns;
            _input = reportDataOutput;
        }

        List<AccountDimDTO> AccountDims { get; set; }
        List<AccountDTO> AccountInternals { get; set; }


        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            bool hasPayrollPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary);
            bool hasEmploymentPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment);
            bool hasPayrollGroupPermission = base.HasReadPermission(Feature.Time_Employee_PayrollGroups);

            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>
            {
                CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.EmployeeNr),
                CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.EmployeeName)
            };
            if (hasEmploymentPermission)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.EmploymentType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.EmployeeGroup));
            }
            if (hasPayrollGroupPermission)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollGroup));
            }
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.EmployeeExternalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionQuantity));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionQuantityWorkDays));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionQuantityCalendarDays));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionCalenderDayFactor));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionTimeUnit));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionTimeUnitName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeTransactionMatrixColumns.IsRegistrationTypeQuantity));
            if (hasPayrollPermission)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionUnitPrice));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionAmount));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionVATAmount));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.Formula));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.FormulaExtracted));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.FormulaNames));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.FormulaOrigin));
            }
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_TimeTransactionMatrixColumns.Ratio));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionExported));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel1));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel2));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel3));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel4));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.AttestStateName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollProductNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollProductName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollProductExternalNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollProductDescription));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_TimeTransactionMatrixColumns.PayrollType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel1));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel2));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel3));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel4));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeTransactionMatrixColumns.IsAbsence));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.StartTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.StopTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeTransactionMatrixColumns.IsPreliminary));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeTransactionMatrixColumns.IsFixed));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeTransactionMatrixColumns.ScheduleTransaction));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_TimeTransactionMatrixColumns.ScheduleTransactionType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeTransactionMatrixColumns.IsManuallyAddedPayroll));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeTransactionMatrixColumns.IsManuallyAddedTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.CurrencyName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.CurrencyCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.Note));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeTransactionMatrixColumns.PayrollCalculationPerformed));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_TimeTransactionMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_TimeTransactionMatrixColumns.Modified));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.CreatedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.ModfiedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.TimeCodeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.TimeCodeCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.TimeRuleName));
        
            if (base.HasReadPermission(Feature.Manage_Users_Edit))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.UserName));
                if (base.HasReadPermission(Feature.Manage_Users_Edit_Password))
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.ExternalAuthId));
            }

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.AccountNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.AccountName));
            }
            int nbrOfAccountDims = this.inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(265, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(270, "Namn")));
                }
            }


            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts) && AccountInternals.Any(a => a.HierarchyOnly))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.EmployeeAccountInternalNrs, options, dim.Name + " " + GetText(600, "Nummer (Anställd)")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.EmployeeAccountInternalNames, options, dim.Name + " " + GetText(601, "Namn (Anställd)")));
                }
            }

            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts) && AccountInternals.Any(a => a.HierarchyOnly) && AccountInternals.Any(a => a.HierarchyNotOnSchedule))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.EmployeeHierachicalAccountInternalNrs, options, dim.Name + " " + GetText(610, "Nummer (Anställd tillhörighet)")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeTransactionMatrixColumns.EmployeeHierachicalAccountInternalNames, options, dim.Name + " " + GetText(611, "Namn (Anställd tillhörighet)")));
                }
            }


            return possibleColumns;
        }

        public List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            if (definitionColumns.IsNullOrEmpty())
            {
                List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>();
                List<MatrixLayoutColumn> possibleColumns = GetMatrixLayoutColumns();
                if (filter != null)
                {
                    int columnNumber = 0;
                    foreach (var field in filter.OrderBy(o => o.Sort))
                    {
                        var item = possibleColumns.FirstOrDefault(w => w.Field == field.ColumnKey);

                        if (item != null)
                        {
                            columnNumber++;
                            matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item, columnNumber, field.Selection?.Options != null ? field.Selection.Options : item.Options));
                        }
                    }
                }
                else
                {
                    foreach (MatrixLayoutColumn item in possibleColumns)
                    {
                        if (!matrixDefinitionColumns.Any(a => a.Field == item.Field))
                            matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }
                }

                definitionColumns = matrixDefinitionColumns;
            }
            return definitionColumns;
        }

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_TimeTransactionMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, EnumUtility.GetName(column), string.IsNullOrEmpty(overrideTitle) ? GetText((int)column, EnumUtility.GetName(column)) : overrideTitle, options);
            if (IsAccountInternal(column))
            {
                var name = GetAccountInternalName(column, 1);
                if (!string.IsNullOrEmpty(name))
                    matrixLayoutColumn.Title = name;
            }

            return matrixLayoutColumn;
        }

        public MatrixResult GetMatrixResult()
        {
            MatrixResult result = new MatrixResult();
            result.MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = GetMatrixDefinitionColumns() };

            #region Create matrix

            int rowNumber = 1;

            foreach (var transactionsOnEmployee in _input.TimeTransactions)
            {
                var employee = _input.Employees.FirstOrDefault(f => f.EmployeeId == transactionsOnEmployee.Key);

                if (employee != null)
                {
                    foreach (TimeTransactionMatrixItem transaction in transactionsOnEmployee.Value)
                    {
                        List<MatrixField> fields = new List<MatrixField>();

                        foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                            fields.Add(CreateField(rowNumber, column, transaction, employee));

                        if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                            result.MatrixFields.AddRange(fields);
                        else
                            result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                        rowNumber++;
                    }
                }
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, TimeTransactionMatrixItem timeTransactionMatrixItem, EmployeeDTO employee)
        {
            if (base.GetEnumId<TermGroup_TimeTransactionMatrixColumns>(column, out int id))
            {
                var timePayrollTransaction = timeTransactionMatrixItem.TransactionDTO;
                // Get time unit from transaction
                GenericType timeUnit = timePayrollTransaction.Extended != null ? _input.TimeUnits?.FirstOrDefault(f => f.Id == timePayrollTransaction.Extended?.TimeUnit) : null;
                PayrollProductDTO product = null;
                if (timeUnit == null && timePayrollTransaction.PayrollProductId != 0)
                {
                    // Extended transaction does not exist, get time unit from payroll product
                    product = _input.PayrollProducts.FirstOrDefault(p => p.ProductId == timePayrollTransaction.PayrollProductId);
                    if (product != null && product.Settings != null)
                    {
                        // Get product setting, first from employees payroll group, then default
                        int? payrollGroupId = employee.GetEmployment(timePayrollTransaction.Date).PayrollGroupId;
                        PayrollProductSettingDTO setting = product.Settings.FirstOrDefault(s => s.PayrollGroupId == (payrollGroupId.HasValue ? payrollGroupId.Value : (int?)null));
                        if (setting == null)
                            setting = product.Settings.FirstOrDefault(s => s.PayrollGroupId == null);
                        if (setting != null)
                            timeUnit = _input.TimeUnits?.FirstOrDefault(f => f.Id == (int)setting.TimeUnit);
                    }
                }


                switch ((TermGroup_TimeTransactionMatrixColumns)id)
                {
                    case TermGroup_TimeTransactionMatrixColumns.Unknown:
                        break;
                    case TermGroup_TimeTransactionMatrixColumns.EmployeeNr:
                        if (employee != null && column.MatrixDataType == MatrixDataType.String)
                            return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                        else
                            return new MatrixField(rowNumber, column.Key, "");
                    case TermGroup_TimeTransactionMatrixColumns.EmployeeExternalCode:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.EmployeeExternalCode, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.ExternalAuthId:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.ExternalAuthId, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.UserName:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.UserName, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.EmployeeName:
                        if (employee != null && column.MatrixDataType == MatrixDataType.String)
                            return new MatrixField(rowNumber, column.Key, employee.Name, column.MatrixDataType);
                        else
                            return new MatrixField(rowNumber, column.Key, "");
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionQuantity:
                        decimal quantity = timePayrollTransaction.Quantity;
                        if (quantity != 0)
                            quantity /= 60;
                        return new MatrixField(rowNumber, column.Key, quantity, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.EmployeeGroup:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.EmployeeGroup, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollGroup:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.PayrollGroup, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.EmploymentType:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.EmploymentType, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionQuantityWorkDays:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Extended?.QuantityWorkDays, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionQuantityCalendarDays:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Extended?.QuantityCalendarDays, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionCalenderDayFactor:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Extended?.CalenderDayFactor, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionTimeUnit:
                        return new MatrixField(rowNumber, column.Key, timeUnit?.Id, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionTimeUnitName:
                        return new MatrixField(rowNumber, column.Key, timeUnit?.Name, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.IsRegistrationTypeQuantity:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.TimeCodeTransaction?.IsRegistrationTypeQuantity, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.Ratio:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.AbsenceRatio, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionUnitPrice:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.UnitPrice, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionAmount:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Amount, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionVATAmount:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.VatAmount, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionDate:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Date, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionExported:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Exported, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel1:
                        return new MatrixField(rowNumber, column.Key, _input?.PayrollTypes != null && timePayrollTransaction.SysPayrollTypeLevel1.HasValue && _input.PayrollTypes.ContainsKey(timePayrollTransaction.SysPayrollTypeLevel1.Value) ? _input.PayrollTypes.FirstOrDefault(f => f.Key == timePayrollTransaction.SysPayrollTypeLevel1).Value : string.Empty, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel2:
                        return new MatrixField(rowNumber, column.Key, _input?.PayrollTypes != null && timePayrollTransaction.SysPayrollTypeLevel2.HasValue && _input.PayrollTypes.ContainsKey(timePayrollTransaction.SysPayrollTypeLevel2.Value) ? _input.PayrollTypes.FirstOrDefault(f => f.Key == timePayrollTransaction.SysPayrollTypeLevel2).Value : string.Empty, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel3:
                        return new MatrixField(rowNumber, column.Key, _input?.PayrollTypes != null && timePayrollTransaction.SysPayrollTypeLevel3.HasValue && _input.PayrollTypes.ContainsKey(timePayrollTransaction.SysPayrollTypeLevel3.Value) ? _input.PayrollTypes.FirstOrDefault(f => f.Key == timePayrollTransaction.SysPayrollTypeLevel3).Value : string.Empty, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel4:
                        return new MatrixField(rowNumber, column.Key, _input?.PayrollTypes != null && timePayrollTransaction.SysPayrollTypeLevel4.HasValue && _input.PayrollTypes.ContainsKey(timePayrollTransaction.SysPayrollTypeLevel4.Value) ? _input.PayrollTypes.FirstOrDefault(f => f.Key == timePayrollTransaction.SysPayrollTypeLevel4).Value : string.Empty, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.AttestStateName:
                        return new MatrixField(rowNumber, column.Key, _input?.AttestStates?.FirstOrDefault(f => f.AttestStateId == timePayrollTransaction.AttestStateId)?.Name, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollProductNumber:
                    case TermGroup_TimeTransactionMatrixColumns.PayrollProductName:
                    case TermGroup_TimeTransactionMatrixColumns.PayrollProductExternalNumber:
                    case TermGroup_TimeTransactionMatrixColumns.PayrollProductDescription:
                    case TermGroup_TimeTransactionMatrixColumns.PayrollType:
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTypeName:
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel1:
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel2:
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel3:
                    case TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel4:
                        var payrollProduct = _input.PayrollProducts.FirstOrDefault(f => f.ProductId == timePayrollTransaction.PayrollProductId);
                        if (payrollProduct != null)
                        {
                            switch ((TermGroup_TimeTransactionMatrixColumns)id)
                            {
                                case TermGroup_TimeTransactionMatrixColumns.PayrollProductNumber:
                                    return new MatrixField(rowNumber, column.Key, payrollProduct.Number, column.MatrixDataType);
                                case TermGroup_TimeTransactionMatrixColumns.PayrollProductName:
                                    return new MatrixField(rowNumber, column.Key, payrollProduct.Name, column.MatrixDataType);
                                case TermGroup_TimeTransactionMatrixColumns.PayrollProductExternalNumber:
                                    return new MatrixField(rowNumber, column.Key, payrollProduct.ExternalNumber, column.MatrixDataType);
                                case TermGroup_TimeTransactionMatrixColumns.PayrollProductDescription:
                                    return new MatrixField(rowNumber, column.Key, payrollProduct.Description, column.MatrixDataType);
                                case TermGroup_TimeTransactionMatrixColumns.PayrollType:
                                    return new MatrixField(rowNumber, column.Key, (SoeProductType)payrollProduct.Type, column.MatrixDataType);
                                case TermGroup_TimeTransactionMatrixColumns.PayrollTypeName:
                                    return new MatrixField(rowNumber, column.Key, (SoeProductType)payrollProduct.Type, column.MatrixDataType);
                                case TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel1:
                                    return new MatrixField(rowNumber, column.Key, _input?.PayrollTypes != null && payrollProduct.SysPayrollTypeLevel1.HasValue && _input.PayrollTypes.ContainsKey(payrollProduct.SysPayrollTypeLevel1.Value) ? _input.PayrollTypes.FirstOrDefault(f => f.Key == payrollProduct.SysPayrollTypeLevel1).Value : string.Empty, column.MatrixDataType);
                                case TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel2:
                                    return new MatrixField(rowNumber, column.Key, _input?.PayrollTypes != null && payrollProduct.SysPayrollTypeLevel2.HasValue && _input.PayrollTypes.ContainsKey(payrollProduct.SysPayrollTypeLevel2.Value) ? _input.PayrollTypes.FirstOrDefault(f => f.Key == payrollProduct.SysPayrollTypeLevel2).Value : string.Empty, column.MatrixDataType);
                                case TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel3:
                                    return new MatrixField(rowNumber, column.Key, _input?.PayrollTypes != null && payrollProduct.SysPayrollTypeLevel3.HasValue && _input.PayrollTypes.ContainsKey(payrollProduct.SysPayrollTypeLevel3.Value) ? _input.PayrollTypes.FirstOrDefault(f => f.Key == payrollProduct.SysPayrollTypeLevel3).Value : string.Empty, column.MatrixDataType);
                                case TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel4:
                                    return new MatrixField(rowNumber, column.Key, _input?.PayrollTypes != null && payrollProduct.SysPayrollTypeLevel4.HasValue && _input.PayrollTypes.ContainsKey(payrollProduct.SysPayrollTypeLevel4.Value) ? _input.PayrollTypes.FirstOrDefault(f => f.Key == payrollProduct.SysPayrollTypeLevel4).Value : string.Empty, column.MatrixDataType);
                                default:
                                    return new MatrixField(rowNumber, column.Key, string.Empty, column.MatrixDataType);
                            }
                        }
                        return new MatrixField(rowNumber, column.Key, string.Empty, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.StartTime:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.StartTime.ToShortTimeString(), column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.StopTime:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.StopTime.ToShortTimeString(), column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.IsPreliminary:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.IsPreliminary, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.IsFixed:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.IsFixed, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.ScheduleTransaction:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.ScheduleTransaction ?? false, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.ScheduleTransactionType:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.ScheduleTransactionType.HasValue ? timePayrollTransaction.ScheduleTransactionType.Value : 0, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.IsManuallyAddedPayroll:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.ManuallyAdded && timePayrollTransaction.IsAdded, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.IsManuallyAddedTime:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.ManuallyAdded && !timePayrollTransaction.IsAdded, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.CurrencyName:
                        return new MatrixField(rowNumber, column.Key, _input?.Currency?.Name, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.CurrencyCode:
                        return new MatrixField(rowNumber, column.Key, _input?.Currency?.Code, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.Note:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Comment, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.Formula:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Extended?.Formula, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.FormulaExtracted:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Extended?.FormulaExtracted, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.FormulaNames:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Extended?.FormulaNames, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.FormulaOrigin:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Extended?.FormulaOrigin, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.PayrollCalculationPerformed:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Extended?.PayrollCalculationPerformed, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Created, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.Modified:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.Modified, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.CreatedBy, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.ModfiedBy:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.ModifiedBy, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.TimeCodeName:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.TimeCodeTransaction?.TimeCodeName, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.TimeCodeCode:
                        return new MatrixField(rowNumber, column.Key, _input?.TimeCodes?.FirstOrDefault(f => f.TimeCodeId == timePayrollTransaction?.TimeCodeTransaction?.TimeCodeId)?.Code ?? string.Empty, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.TimeRuleName:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.TimeCodeTransaction?.TimeRuleId != null ? _input.TimeRules?.FirstOrDefault(f => f.TimeRuleId == timePayrollTransaction.TimeCodeTransaction?.TimeRuleId.Value)?.Name : string.Empty, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.AccountNr:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.AccountStd?.AccountNr, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.IsAbsence:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.IsAbsence(), column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.AccountName:
                        return new MatrixField(rowNumber, column.Key, timePayrollTransaction.AccountStd?.Name, column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.AccountInternalNames:
                        var accountAnalysisFields1 = timePayrollTransaction.AccountInternals?.Select(s => new AccountAnalysisField(s)).ToList();
                        return new MatrixField(rowNumber, column.Key, accountAnalysisFields1.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.AccountInternalNrs:
                        var accountAnalysisFields2 = timePayrollTransaction.AccountInternals?.Select(s => new AccountAnalysisField(s)).ToList();
                        return new MatrixField(rowNumber, column.Key, accountAnalysisFields2.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.EmployeeAccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.EmployeeAccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.EmployeeAccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.EmployeeAccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.EmployeeHierachicalAccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.EmployeeHierchicalAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_TimeTransactionMatrixColumns.EmployeeHierachicalAccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, timeTransactionMatrixItem.EmployeeHierchicalAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

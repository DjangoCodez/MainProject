using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class EmployeeDocumentMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeDocumentMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeDocumentReportDataField> filter { get; set; }
        EmployeeDocumentReportDataOutput _reportDataOutput { get; set; }
        readonly bool useAccountHierarchy;
        #endregion

        public EmployeeDocumentMatrix(InputMatrix inputMatrix, EmployeeDocumentReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;

            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, actorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Manage_Users_Edit))
                return possibleColumns;

            if (base.HasReadPermission(Feature.Common_Categories_Employee))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.EmployeeNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.EmployeeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.FirstName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.LastName));
            }
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Files))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.FileName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.FileType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.Description));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_EmployeeDocumentMatrixColumns.Created));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeDocumentMatrixColumns.NeedsConfirmation));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_EmployeeDocumentMatrixColumns.Confirmed));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_EmployeeDocumentMatrixColumns.Read));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_EmployeeDocumentMatrixColumns.Answered));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.AnswerType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeDocumentMatrixColumns.ByMessage));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_EmployeeDocumentMatrixColumns.ValidFrom));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_EmployeeDocumentMatrixColumns.ValidTo));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.AttestStatus));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.CurrentAttestUsers));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.AttestState));
            }

            if (!this.useAccountHierarchy)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.CategoryName));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDocumentMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            if (base.HasReadPermission(Feature.Common_ExtraFields_Employee) && !inputMatrix.ExtraFields.IsNullOrEmpty())
            {
                foreach (var extraField in inputMatrix.ExtraFields)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = extraField.ExtraFieldId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(SetMatrixDataType((TermGroup_ExtraFieldType)extraField.Type), TermGroup_EmployeeDocumentMatrixColumns.ExtraFieldEmployee, options, extraField.Text));
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
                        MatrixLayoutColumn item = possibleColumns.FirstOrDefault(w => w.Field == field.ColumnKey);
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
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }
                }

                definitionColumns = matrixDefinitionColumns;
            }
            return definitionColumns;
        }



        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeDocumentMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var employeeDocumentItem in _reportDataOutput.EmployeeDocumentItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, employeeDocumentItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeDocumentItem employeeDocumentItem)
        {
                if (base.GetEnumId<TermGroup_EmployeeDocumentMatrixColumns>(column, out int id))
                {
                    var type = (TermGroup_EmployeeDocumentMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeDocumentMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.FileName:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.FileName, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.FileType:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.FileType, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.Description:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.Description, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.Created, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.NeedsConfirmation:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.NeedsConfirmation, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.Confirmed:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.Confirmed, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.Read:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.Read, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.Answered:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.Answered, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.AnswerType:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.AnswerType, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.ByMessage:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.ByMessage, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.ValidFrom:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.ValidFrom, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.ValidTo:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.ValidTo, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.AttestStatus:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.AttestStatus, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.AttestState:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.AttestState, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.CurrentAttestUsers:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.CurrentAttestUsers, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.CategoryName:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.CategoryName, column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.AccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.AccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_EmployeeDocumentMatrixColumns.ExtraFieldEmployee:
                        return new MatrixField(rowNumber, column.Key, employeeDocumentItem.ExtraFieldAnalysisFields.ExtraFieldAnalysisFieldValue(column), column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

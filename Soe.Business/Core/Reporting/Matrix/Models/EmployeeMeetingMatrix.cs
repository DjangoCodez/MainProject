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
    public class EmployeeMeetingMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeMeetingMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeMeetingReportDataField> filter { get; set; }
        EmployeeMeetingReportDataOutput _reportDataOutput { get; set; }
        readonly bool useAccountHierarchy;
        #endregion

        public EmployeeMeetingMatrix(InputMatrix inputMatrix, EmployeeMeetingReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
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
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.EmployeeNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.EmployeeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.FirstName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.LastName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.Gender));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeMeetingMatrixColumns.BirthDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.MeetingType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeMeetingMatrixColumns.StartDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.StartTime));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.Participants));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.OtherParticipants));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeMeetingMatrixColumns.Completed));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeMeetingMatrixColumns.Reminder));
            }

            if (base.HasReadPermission(Feature.Time_Employee_EmploymentTypes))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.EmploymentTypeName));

            if (base.HasReadPermission(Feature.Time_Employee_Positions))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.Position));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.SSYKCode));
            }

            if (!useAccountHierarchy)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.CategoryName));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeMeetingMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            if (base.HasReadPermission(Feature.Common_ExtraFields_Employee) && !inputMatrix.ExtraFields.IsNullOrEmpty())
            {
                foreach (var extraField in inputMatrix.ExtraFields)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = extraField.ExtraFieldId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(SetMatrixDataType((TermGroup_ExtraFieldType)extraField.Type), TermGroup_EmployeeMeetingMatrixColumns.ExtraFieldEmployee, options, extraField.Text));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeMeetingMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var employeeMeetingItem in _reportDataOutput.EmployeeMeetingItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, employeeMeetingItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeMeetingItem employeeMeetingItem)
        {
            if (base.GetEnumId<TermGroup_EmployeeMeetingMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_EmployeeMeetingMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeMeetingMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.Gender:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.Gender, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.BirthDate:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.BirthDate, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.EmploymentTypeName:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.EmploymentType, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.Position:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.Position, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.SSYKCode:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.SSYKCode, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.StartDate:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.StartTime, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.StartTime:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.StartTime.ToShortTimeString(), column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.Completed:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.Completed, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.Reminder:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.Reminder, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.MeetingType:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.MeetingType, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.Participants:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.Participants, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.OtherParticipants:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.OtherParticipants, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.CategoryName:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.CategoryName, column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.AccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.AccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_EmployeeMeetingMatrixColumns.ExtraFieldEmployee:
                        return new MatrixField(rowNumber, column.Key, employeeMeetingItem.ExtraFieldAnalysisFields.ExtraFieldAnalysisFieldValue(column), column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

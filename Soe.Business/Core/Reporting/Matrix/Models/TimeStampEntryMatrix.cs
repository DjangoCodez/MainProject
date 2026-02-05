using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class TimeStampEntryMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "TimeStampEntryMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<TimeStampEntryReportDataField> filter { get; set; }
        List<TimeStampEntryItem> TimeStampEntryItems { get; set; }
        private List<ShiftTypeDTO> ShiftTypes { get; set; }
        private List<GenericType> OriginTypes { get; set; }
        private List<GenericType> Statuses { get; set; }

        #endregion

        public TimeStampEntryMatrix(InputMatrix inputMatrix, TimeStampEntryReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            TimeStampEntryItems = reportDataOutput != null ? reportDataOutput.TimeStampEntryItems : new List<TimeStampEntryItem>();
            ShiftTypes = reportDataOutput?.ShiftTypes;
            OriginTypes = reportDataOutput?.OriginTypes;
            Statuses = reportDataOutput?.Statuses;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.AccountName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.Time));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.OriginalTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_TimeStampMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeStampMatrixColumns.IsBreak));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_TimeStampMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.CreatedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_TimeStampMatrixColumns.Modified));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.ModifiedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_TimeStampMatrixColumns.ShiftTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.DeviationCauseName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.ScheduleTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.TimeTerminalName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.TypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.Note));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.OriginType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampMatrixColumns.Status));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_TimeStampMatrixColumns.IsRemoved));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_TimeStampMatrixColumns column, MatrixDefinitionColumnOptions options = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, EnumUtility.GetName(column), GetText((int)column, EnumUtility.GetName(column)), options);
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

            foreach (var employee in TimeStampEntryItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, employee));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, TimeStampEntryItem timeStampEntryItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_TimeStampMatrixColumns)))
            {
                var type = (TermGroup_TimeStampMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_TimeStampMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.AccountName:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.AccountName, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.Time:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.Time.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.Date:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.Date.Date, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.IsBreak:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.IsBreak, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.Created, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.CreatedBy, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.Modified:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.Modified, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.ModifiedBy:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.ModifiedBy, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.ShiftTypeName:
                        return new MatrixField(rowNumber, column.Key, ShiftTypes?.FirstOrDefault(f => f.ShiftTypeId == timeStampEntryItem.ShiftTypeId)?.Name, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.DeviationCauseName:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.TimeDeviationCauseName, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.ScheduleTypeName:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.TimeScheduleTypeName, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.TimeTerminalName:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.TimeTerminalName, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.TypeName:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.TypeName, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.Note:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.Note, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.OriginType:
                        return new MatrixField(rowNumber, column.Key, OriginTypes?.FirstOrDefault(f => f.Id == timeStampEntryItem.OriginType)?.Name, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.OriginalTime:
                        return new MatrixField(rowNumber, column.Key, timeStampEntryItem.OriginalTime.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.Status:
                        return new MatrixField(rowNumber, column.Key, Statuses?.FirstOrDefault(f => f.Id == timeStampEntryItem.Status)?.Name, column.MatrixDataType);
                    case TermGroup_TimeStampMatrixColumns.IsRemoved:
                        return new MatrixField(rowNumber, column.Key, (timeStampEntryItem.State == (int)SoeEntityState.Deleted), column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

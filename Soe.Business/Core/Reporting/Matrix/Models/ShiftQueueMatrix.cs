using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class ShiftQueueMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "ShiftQueueMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<ShiftQueueReportDataField> filter { get; set; }
        List<ShiftQueueItem> shiftQueueItems { get; set; }
        #endregion

        public ShiftQueueMatrix(InputMatrix inputMatrix, ShiftQueueReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            shiftQueueItems = reportDataOutput?.ShiftQueueItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_ShiftQueueMatrixColumns.EmployeeId, new MatrixDefinitionColumnOptions() { Hidden = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftQueueMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftQueueMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftQueueMatrixColumns.CurrentEmployee));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_ShiftQueueMatrixColumns.CurrentEmployeeIsHidden));

            if (base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_DayView) || base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_ScheduleView))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ShiftQueueMatrixColumns.Created));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftQueueMatrixColumns.Creator));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftQueueMatrixColumns.Modifier));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_ShiftQueueMatrixColumns.Date));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftQueueMatrixColumns.StartTime));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftQueueMatrixColumns.StopTime));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ShiftQueueMatrixColumns.DateHandled));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_ShiftQueueMatrixColumns.QueueTimeSinceShiftCreatedInHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_ShiftQueueMatrixColumns.QueueTimeBeforeShiftStartInHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_ShiftQueueMatrixColumns.QueueTimeBeforeQueueWasHandledInHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_ShiftQueueMatrixColumns.QueueTimeHandledBeforeShiftStartInHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftQueueMatrixColumns.TypeName));
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
                    // Hidden
                    foreach (MatrixLayoutColumn item in possibleColumns.Where(c => c.IsHidden()))
                    {
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_ShiftQueueMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in shiftQueueItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, ShiftQueueItem shiftQueueItem)
        {
            if (base.GetEnumId<TermGroup_ShiftQueueMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_ShiftQueueMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_ShiftQueueMatrixColumns.EmployeeId:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.EmployeeId, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.Created, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.Creator:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.Creator, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.Modifier:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.Modifier, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.Date:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.Date, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.QueueTimeBeforeShiftStartInHours:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.QueueTimeBeforeShiftStartInHours, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.QueueTimeBeforeQueueWasHandledInHours:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.QueueTimeBeforeQueueWasHandledInHours, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.QueueTimeSinceShiftCreatedInHours:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.QueueTimeSinceShiftCreatedInHours, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.QueueTimeHandledBeforeShiftStartInHours:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.QueueTimeHandledBeforeShiftStartInHours, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.TypeName:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.TypeName, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.StartTime:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.StartTime.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.StopTime:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.StopTime.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.CurrentEmployee:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.CurrentEmployee, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.CurrentEmployeeIsHidden:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.CurrentEmployeeIsHidden, column.MatrixDataType);
                    case TermGroup_ShiftQueueMatrixColumns.DateHandled:
                        return new MatrixField(rowNumber, column.Key, shiftQueueItem.DateHandled, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

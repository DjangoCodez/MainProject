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
    public class ShiftHistoryMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "ShiftHistoryMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<ShiftHistoryReportDataField> filter { get; set; }
        List<ShiftHistoryItem> shiftHistoryItems { get; set; }
        #endregion

        public ShiftHistoryMatrix(InputMatrix inputMatrix, ShiftHistoryReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            shiftHistoryItems = reportDataOutput?.ShiftHistoryItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_ShiftHistoryMatrixColumns.EmployeeId, new MatrixDefinitionColumnOptions() { Hidden = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.TypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.TypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromShiftStatus));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToShiftStatus));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ShiftStatusChanged));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromShiftUserStatus));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToShiftUserStatus));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ShiftUserStatusChanged));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromEmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToEmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromEmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToEmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.EmployeeChanged));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.TimeChanged));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromDateAndTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToDateAndTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.DateAndTimeChanged));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromShiftType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToShiftType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ShiftTypeChanged));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromTimeDeviationCause));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToTimeDeviationCause));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.TimeDeviationCauseChanged));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_ShiftHistoryMatrixColumns.Created)); 
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.CreatedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.AbsenceRequestApprovedText));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromStart));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromStop));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToStart));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToStop));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.OriginEmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.OriginEmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.FromExtraShift));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ToExtraShift));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftHistoryMatrixColumns.ExtraShiftChanged));


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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_ShiftHistoryMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in shiftHistoryItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, ShiftHistoryItem shiftHistoryItem)
        {
            if (base.GetEnumId<TermGroup_ShiftHistoryMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_ShiftHistoryMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_ShiftHistoryMatrixColumns.EmployeeId:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.EmployeeId, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.TimeScheduleTemplateBlockId:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.TimeScheduleTemplateBlockId, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.TypeName:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.TypeName, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromShiftStatus:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromShiftStatus, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToShiftStatus:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToShiftStatus, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ShiftStatusChanged:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ShiftStatusChanged, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromShiftUserStatus:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromShiftUserStatus, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToShiftUserStatus:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToShiftUserStatus, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ShiftUserStatusChanged:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ShiftUserStatusChanged, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromEmployeeName:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromEmployeeName, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToEmployeeName:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToEmployeeName, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromEmployeeNr:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromEmployeeNr, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToEmployeeNr:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToEmployeeNr, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.EmployeeChanged:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.EmployeeChanged, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromTime:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromTime, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToTime:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToTime, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.TimeChanged:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.TimeChanged, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromDateAndTime:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromDateAndTime, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToDateAndTime:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToDateAndTime, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.DateAndTimeChanged:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.DateAndTimeChanged, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromShiftType:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromShiftType, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToShiftType:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToShiftType, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ShiftTypeChanged:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ShiftTypeChanged, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromTimeDeviationCause:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromTimeDeviationCause, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToTimeDeviationCause:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToTimeDeviationCause, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.TimeDeviationCauseChanged:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.TimeDeviationCauseChanged, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.Created:
                        // Assuming 'Created' is a DateTime and needs to be converted to a string or another suitable format
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.Created?.ToString(), column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.CreatedBy, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.AbsenceRequestApprovedText:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.AbsenceRequestApprovedText, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromStart:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromStart, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromStop:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromStop, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToStart:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToStart, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToStop:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToStop, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.OriginEmployeeNr:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.OriginEmployeeNr, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.OriginEmployeeName:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.OriginEmployeeName, column.MatrixDataType);

                    //case TermGroup_ShiftHistoryMatrixColumns.FromEmployeeId:
                    //    return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromEmployeeId?.ToString(), column.MatrixDataType);

                    //case TermGroup_ShiftHistoryMatrixColumns.ToEmployeeId:
                    //    return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToEmployeeId?.ToString(), column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.FromExtraShift:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.FromExtraShift, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ToExtraShift:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ToExtraShift, column.MatrixDataType);

                    case TermGroup_ShiftHistoryMatrixColumns.ExtraShiftChanged:
                        return new MatrixField(rowNumber, column.Key, shiftHistoryItem.ExtraShiftChanged, column.MatrixDataType);

                    default:
                      return new MatrixField(rowNumber, column.Key, "");
                }

            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

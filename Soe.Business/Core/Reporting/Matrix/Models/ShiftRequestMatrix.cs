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
    public class ShiftRequestMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "ShiftRequestMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<ShiftRequestReportDataField> filter { get; set; }
        List<ShiftRequestItem> shiftRequestItems { get; set; }
        readonly bool useAccountHierarchy;
        #endregion

        public ShiftRequestMatrix(InputMatrix inputMatrix, ShiftRequestReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            shiftRequestItems = reportDataOutput?.ShiftRequestItems;

            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, actorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_ShiftRequestMatrixColumns.EmployeeId, new MatrixDefinitionColumnOptions() { Hidden = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.EmployeeName));

            if (base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_DayView) || base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_ScheduleView))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ShiftRequestMatrixColumns.RequestCreated));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.RequestCreatedBy));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.Sender));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ShiftRequestMatrixColumns.SentDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ShiftRequestMatrixColumns.ReadDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.Answer));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ShiftRequestMatrixColumns.AnswerDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.Subject));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.Text));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_ShiftRequestMatrixColumns.ShiftDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.ShiftStartTime));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.ShiftStopTime));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.ShiftTypeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ShiftRequestMatrixColumns.ShiftCreated));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.ShiftCreatedBy));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ShiftRequestMatrixColumns.ShiftModified));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.ShiftModifiedBy));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_ShiftRequestMatrixColumns.ShiftDeleted));

                if (useAccountHierarchy)
                {
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.ShiftAccountNr));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftRequestMatrixColumns.ShiftAccountName));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_ShiftRequestMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in shiftRequestItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, ShiftRequestItem shiftRequestItem)
        {
            if (base.GetEnumId<TermGroup_ShiftRequestMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_ShiftRequestMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_ShiftRequestMatrixColumns.EmployeeId:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.EmployeeId, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.RequestCreated:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.RequestCreated, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.RequestCreatedBy:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.RequestCreatedBy, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.Sender:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.Sender, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.SentDate:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.SentDate, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ReadDate:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ReadDate, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.Answer:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.Answer, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.AnswerDate:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.AnswerDate, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.Subject:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.Subject, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.Text:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.Text, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftDate:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftDate, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftStartTime:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftStartTime.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftStopTime:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftStopTime.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftTypeName:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftTypeName, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftCreated:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftCreated, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftCreatedBy:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftCreatedBy, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftModified:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftModified, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftModifiedBy:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftModifiedBy, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftDeleted:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftDeleted, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftAccountNr:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftAccountNr, column.MatrixDataType);
                    case TermGroup_ShiftRequestMatrixColumns.ShiftAccountName:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.ShiftAccountName, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

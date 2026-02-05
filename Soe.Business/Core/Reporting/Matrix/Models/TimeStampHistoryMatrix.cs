using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class TimeStampHistoryMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "TimeStampHistoryMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<TimeStampHistoryReportDataField> filter { get; set; }
        List<GenericTrackChangesItem> TrackChangesItems { get; set; }
        private List<GenericType> OriginTypes { get; set; }
        private List<GenericType> Statuses { get; set; }

        #endregion

        public TimeStampHistoryMatrix(InputMatrix inputMatrix, TimeStampHistoryReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            TrackChangesItems = reportDataOutput != null ? reportDataOutput.TrackChangesItems : new List<GenericTrackChangesItem>();
            OriginTypes = reportDataOutput?.OriginTypes;
            Statuses = reportDataOutput?.Statuses;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (base.HasReadPermission(Feature.Time_Time_Attest_AdjustTimeStamps) || base.HasReadPermission(Feature.Time_Time_Attest) || base.HasReadPermission(Feature.Manage_GDPR_Logs))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.TopRecordName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.TopEntity1Text));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.TopEntity2Text));
                // possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.EntityText));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.ColumnName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.ActionMethod));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.Action));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.FromValue));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.ToValue));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.RecordName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_TimeStampHistoryMatrixColumns.Created));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.CreatedBy));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.Role));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_TimeStampHistoryMatrixColumns.BatchNr));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_TimeStampHistoryMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var changeRow in TrackChangesItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, changeRow));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, GenericTrackChangesItem trackChangesItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_TimeStampHistoryMatrixColumns)))
            {
                var type = (TermGroup_TimeStampHistoryMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_TimeStampHistoryMatrixColumns.BatchNr:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.BatchNbr, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.TopRecordName:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.TopRecordName, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.TopEntity1Text:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.TopEntity1Text, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.TopEntity2Text:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.TopEntity2Text, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.EntityText:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.EntityText, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.ColumnName:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.Column, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.ActionMethod:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.ActionMethod, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.Action:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.Action, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.FromValue:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.FromValue, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.ToValue:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.ToValue, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.Role:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.Role, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.RecordName:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.RecordName, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.Created, column.MatrixDataType);
                    case TermGroup_TimeStampHistoryMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, trackChangesItem.CreatedBy, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage;
using SoftOne.Soe.Business.Core.Reporting.Models.Status.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class SoftOneStatusEventMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "SoftOneStatusEventMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<SoftOneStatusEventReportDataReportDataField> filter { get; set; }
        SoftOneStatusEventReportDataOutput _reportDataOutput { get; set; }
       
        #endregion

        public SoftOneStatusEventMatrix(InputMatrix inputMatrix, SoftOneStatusEventReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Manage_Contracts_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SoftOneStatusEventMatrixColumns.Unknown));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_SoftOneStatusEventMatrixColumns.Prio));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SoftOneStatusEventMatrixColumns.Url));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SoftOneStatusEventMatrixColumns.StatusServiceTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_SoftOneStatusEventMatrixColumns.Start));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_SoftOneStatusEventMatrixColumns.End));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_SoftOneStatusEventMatrixColumns.Minutes));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_SoftOneStatusEventMatrixColumns.LastMessageSent));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SoftOneStatusEventMatrixColumns.Message));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SoftOneStatusEventMatrixColumns.StatusEventTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SoftOneStatusEventMatrixColumns.JobDescriptionName));
            
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_SoftOneStatusEventMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var softOneStatusEventItem in _reportDataOutput.ResultItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, softOneStatusEventItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, SoftOneStatusEventItem SoftOneStatusEventItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_SoftOneStatusEventMatrixColumns)))
            {
                var type = (TermGroup_SoftOneStatusEventMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_SoftOneStatusEventMatrixColumns.Unknown:
                        break;
                    case TermGroup_SoftOneStatusEventMatrixColumns.Prio:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.Prio, column.MatrixDataType);
                    case TermGroup_SoftOneStatusEventMatrixColumns.Url:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.Url, column.MatrixDataType);
                    case TermGroup_SoftOneStatusEventMatrixColumns.StatusServiceTypeName:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.StatusServiceTypeName, column.MatrixDataType);
                    case TermGroup_SoftOneStatusEventMatrixColumns.Start:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.Start, column.MatrixDataType);
                    case TermGroup_SoftOneStatusEventMatrixColumns.End:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.End, column.MatrixDataType);
                    case TermGroup_SoftOneStatusEventMatrixColumns.Minutes:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.Minutes, column.MatrixDataType);
                    case TermGroup_SoftOneStatusEventMatrixColumns.LastMessageSent:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.LastMessageSent, column.MatrixDataType);
                    case TermGroup_SoftOneStatusEventMatrixColumns.Message:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.Message, column.MatrixDataType);
                    case TermGroup_SoftOneStatusEventMatrixColumns.StatusEventTypeName:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.StatusEventTypeName, column.MatrixDataType);
                    case TermGroup_SoftOneStatusEventMatrixColumns.JobDescriptionName:
                        return new MatrixField(rowNumber, column.Key, SoftOneStatusEventItem.JobDescriptionName, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

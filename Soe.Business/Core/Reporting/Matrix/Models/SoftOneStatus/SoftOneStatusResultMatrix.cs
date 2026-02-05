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
    public class SoftOneStatusResultMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "SoftOneStatusResultMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<SoftOneStatusResultReportDataReportDataField> filter { get; set; }
        SoftOneStatusResultReportDataOutput _reportDataOutput { get; set; }
        int actorCompanyId;
        bool useAccountHierarchy;
        #endregion

        public SoftOneStatusResultMatrix(InputMatrix inputMatrix, SoftOneStatusResultReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            this.actorCompanyId = actorCompanyId;

            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, actorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Manage_Contracts_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SoftOneStatusResultMatrixColumns.ServiceTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_SoftOneStatusResultMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_SoftOneStatusResultMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_SoftOneStatusResultMatrixColumns.From));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_SoftOneStatusResultMatrixColumns.To));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_SoftOneStatusResultMatrixColumns.Hour));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_SoftOneStatusResultMatrixColumns.Failed));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_SoftOneStatusResultMatrixColumns.Succeded));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusResultMatrixColumns.Min));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusResultMatrixColumns.Median));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusResultMatrixColumns.Average));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusResultMatrixColumns.Max));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusResultMatrixColumns.Percential10));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusResultMatrixColumns.Percential90));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_SoftOneStatusResultMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var softOneStatusResultItem in _reportDataOutput.ResultItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, softOneStatusResultItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, SoftOneStatusResultItem softOneStatusResultItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_SoftOneStatusResultMatrixColumns)))
            {
                var type = (TermGroup_SoftOneStatusResultMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_SoftOneStatusResultMatrixColumns.Date:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Date, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Created, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.From:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.From, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.To:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.To, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Hour:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Hour, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Failed:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Failed, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Succeded:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Succeded, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Min:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Min, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Median:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Median, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Average:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Average, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Max:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Max, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Percential10:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Percential10, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.Percential90:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.Percential90, column.MatrixDataType);
                    case TermGroup_SoftOneStatusResultMatrixColumns.ServiceTypeName:
                        return new MatrixField(rowNumber, column.Key, softOneStatusResultItem.ServiceTypeName, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

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
    public class SoftOneStatusUpTimeMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "SoftOneStatusUpTimeMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<SoftOneStatusUpTimeReportDataReportDataField> filter { get; set; }
        SoftOneStatusUpTimeReportDataOutput _reportDataOutput { get; set; }
        int actorCompanyId;
        bool useAccountHierarchy;
        #endregion

        public SoftOneStatusUpTimeMatrix(InputMatrix inputMatrix, SoftOneStatusUpTimeReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
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

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SoftOneStatusUpTimeMatrixColumns.StatusServiceGroupName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_SoftOneStatusUpTimeMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusUpTimeMatrixColumns.UpTimeOnDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusUpTimeMatrixColumns.TotalUpTimeOnDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusUpTimeMatrixColumns.WebUpTimeOnDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_SoftOneStatusUpTimeMatrixColumns.MobileUpTimeOnDate));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_SoftOneStatusUpTimeMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var softOneStatusUpTimeItem in _reportDataOutput.ResultItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, softOneStatusUpTimeItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, SoftOneStatusUpTimeItem softOneStatusUpTimeItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_SoftOneStatusUpTimeMatrixColumns)))
            {
                var type = (TermGroup_SoftOneStatusUpTimeMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_SoftOneStatusUpTimeMatrixColumns.Unknown:
                        break;
                    case TermGroup_SoftOneStatusUpTimeMatrixColumns.StatusServiceGroupName:
                        return new MatrixField(rowNumber, column.Key, softOneStatusUpTimeItem.StatusServiceGroupName, column.MatrixDataType);
                    case TermGroup_SoftOneStatusUpTimeMatrixColumns.Date:
                        return new MatrixField(rowNumber, column.Key, softOneStatusUpTimeItem.Date, column.MatrixDataType);
                    case TermGroup_SoftOneStatusUpTimeMatrixColumns.UpTimeOnDate:
                        return new MatrixField(rowNumber, column.Key, softOneStatusUpTimeItem.UpTimeOnDate, column.MatrixDataType);
                    case TermGroup_SoftOneStatusUpTimeMatrixColumns.TotalUpTimeOnDate:
                        return new MatrixField(rowNumber, column.Key, softOneStatusUpTimeItem.TotalUpTimeOnDate, column.MatrixDataType);
                    case TermGroup_SoftOneStatusUpTimeMatrixColumns.WebUpTimeOnDate:
                        return new MatrixField(rowNumber, column.Key, softOneStatusUpTimeItem.WebUpTimeOnDate, column.MatrixDataType);
                    case TermGroup_SoftOneStatusUpTimeMatrixColumns.MobileUpTimeOnDate:
                        return new MatrixField(rowNumber, column.Key, softOneStatusUpTimeItem.MobileUpTimeOnDate, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}

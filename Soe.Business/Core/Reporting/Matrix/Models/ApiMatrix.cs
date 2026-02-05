using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public static class ApiMatrix
    {
        public static ApiMatrixDataResult GetApiMatrixDataResult(MatrixResult matrixResult)
        {
            var rows = matrixResult.GetMatrixRows();
            ApiMatrixDataResult result = new ApiMatrixDataResult();

            if (matrixResult.MatrixDefinitions.IsNullOrEmpty())
                matrixResult.MatrixDefinitions.Add(matrixResult.MatrixDefinition);

            var columns = matrixResult.MatrixDefinitions.SelectMany(sm => sm.MatrixDefinitionColumns.Where(c => !c.IsHidden()).ToList() ?? new List<MatrixDefinitionColumn>()).ToList();

            foreach (var col in columns)
            {
                ApiMatrixDataColumn column = new ApiMatrixDataColumn()
                {
                    Key = col.Key.ToString(),
                    Title = col.Title,
                    Field = col.Field
                };

                result.ApiMatrixDataColumns.Add(column);
            }

            foreach (var row in rows)
            {
                ApiMatrixDataRow dataRow = new ApiMatrixDataRow();

                foreach (var field in row.MatrixFields)
                {
                    var column = columns.FirstOrDefault(f => f.Key == field.ColumnKey);
                    if (column != null)
                    {
                        ApiMatrixDataField matrixDataField = new ApiMatrixDataField()
                        {
                            ApiMatrixDataColumnKey = column.Key.ToString(),
                            Value = field.Value?.ToString() ?? string.Empty,
                            Field = column.Field,
                        };
                        dataRow.ApiMatrixDataFields.Add(matrixDataField);
                    }
                }

                result.ApiMatrixDataRows.Add(dataRow);
            }

            return result;
        }
    }
}
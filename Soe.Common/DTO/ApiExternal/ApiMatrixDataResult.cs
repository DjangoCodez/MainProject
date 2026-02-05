using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.DTO.ApiExternal
{
    public class ApiMatrixDataResult
    {
        public string ResultMessage { get; set; }
        public ApiMatrixDataResult()
        {
            ApiMatrixDataRows = new List<ApiMatrixDataRow>();
            ApiMatrixDataColumns = new List<ApiMatrixDataColumn>();
        }

        public List<ApiMatrixDataRow> ApiMatrixDataRows { get; set; }
        public List<ApiMatrixDataColumn> ApiMatrixDataColumns { get; set; }
    }

    public class ApiMatrixDataColumn
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Field { get; set; }
    }
    public class ApiMatrixDataRow
    {
        public string RowKey { get; set; }
        public ApiMatrixDataRow()
        {
            ApiMatrixDataFields = new List<ApiMatrixDataField>();
        }
        public List<ApiMatrixDataField> ApiMatrixDataFields { get; set; }
    }

    public class ApiMatrixDataField
    {
        public string ApiMatrixDataColumnKey { get; set; }
        public string Value { get; set; }
        public string Field { get; set; }
    }
}
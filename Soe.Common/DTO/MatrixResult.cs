using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    #region Matrix

    public class MatrixResult
    {
        public MatrixResult()
        {
            MatrixFields = new List<MatrixField>();
            JsonRows = new List<Dictionary<string, object>>();
            MatrixDefinitions = new List<MatrixDefinition>();
        }
        public MatrixDefinition MatrixDefinition { get; set; }
        public List<MatrixDefinition> MatrixDefinitions { get; set; }
        public List<MatrixField> MatrixFields { get; set; }
        public List<Dictionary<string, object>> JsonRows { get; set; }
        public int Key { get; set; }
    }

    public class MatrixDefinition
    {
        public MatrixDefinition()
        {
            MatrixDefinitionColumns = new List<MatrixDefinitionColumn>();
        }
        public int Key { get; set; }
        public List<MatrixDefinitionColumn> MatrixDefinitionColumns { get; set; }
    }

    [TSInclude]
    public class MatrixDefinitionColumn
    {
        public int ColumnNumber { get; set; }
        public Guid Key { get; set; }
        public MatrixDataType MatrixDataType { get; set; }
        public string Field { get; set; }
        public string Title { get; set; }
        public MatrixDefinitionColumnOptions Options { get; set; }
        public MatrixLayoutColumn MatrixLayoutColumn { get; set; }

        public bool IsHidden()
        {
            return Options != null && Options.Hidden;
        }


    }

    public class MatrixDataDefinition
    {
        public Guid ColumnKey { get; set; }
        public List<int> PermissionIds { get; set; }
        public List<MatrixTableType> MatrixTableTypes { get; set; }
        public string FieldName { get; set; }
        public MatrixDataType MatrixDataType { get; set; }
        public MatrixDefinitionColumnOptions Options { get; set; }

        public string GetColumnField()
        {
            string field = string.Empty;
            int count = 0;
            foreach (var type in MatrixTableTypes)
            {
                var name = Enum.GetName(type.GetType(), type);
                if (count == 0)
                    name = name.Substring(0, 1).ToLower() + name.Substring(1);
                field += name;
            }

            return field;
        }

        public MatrixDefinitionColumn GetMatrixDefinitionColumn()
        {
            return new MatrixDefinitionColumn()
            {
                Key = ColumnKey,
                Field = GetColumnField(),
                MatrixDataType = MatrixDataType,
                Title = GetColumnField(),
                Options = Options,

            };
        }

    }

    public enum MatrixTableType
    {
        Unkown = 0,

        //TimeTransaction
        TimePayrollTransactionDTO = 501,
        TimePayrollScheduleTransactionDTO = 502,
    }

    public class MatrixRow
    {
        public MatrixRow()
        {
            MatrixFields = new List<MatrixField>();
        }
        public int RowNumber { get; set; }
        public int Key { get; set; }
        public List<MatrixField> MatrixFields { get; set; }
    }
    public class MatrixField
    {
        public MatrixField(int rowNumber, Guid columnKey, object value, MatrixDataType matrixDataType = MatrixDataType.String)
        {
            RowNumber = rowNumber;
            ColumnKey = columnKey;
            Value = value;
            MatrixDataType = matrixDataType;
            MatrixFieldOptions = new List<MatrixFieldOption>();
        }
        public MatrixDataType MatrixDataType { get; set; }
        public int RowNumber { get; set; }
        public Guid ColumnKey { get; set; }
        public object Value { get; set; }
        public List<MatrixFieldOption> MatrixFieldOptions { get; set; }
        public int Key { get; set; }
    }

    public class MatrixFieldOption
    {
        public MatrixFieldSetting MatrixFieldSetting { get; set; }
        public string StringValue { get; set; }
        public bool IsTrueBool()
        {
            if (bool.TryParse(StringValue, out bool value))
                return value;

            return false;
        }
    }

    [TSInclude]
    public class MatrixDefinitionColumnOptions
    {
        // Visibility
        public bool Hidden { get; set; }

        //Key
        public string Key { get; set; }

        // Formatting
        public bool AlignLeft { get; set; }
        public bool AlignRight { get; set; }
        public bool ClearZero { get; set; }
        public bool Changed { get; set; }
        public int Decimals { get; set; }
        public bool MinutesToDecimal { get; set; }
        public bool MinutesToTimeSpan { get; set; }
        public bool FormatTimeWithSeconds { get; set; }
        public bool FormatTimeWithDays { get; set; }
        public TermGroup_MatrixDateFormatOption DateFormatOption { get; set; }

        public string LabelPostValue { get; set; }

        // Grouping
        public bool GroupBy { get; set; }
        [TsIgnore]
        public bool Aggregate { get { return GroupBy; } }
        public TermGroup_MatrixGroupAggOption GroupOption { get; set; }

        //Excel
        public List<MatrixFieldOption> GetMatrixFieldOptions()
        {
            List<MatrixFieldOption> options = new List<MatrixFieldOption>();

            if (ClearZero)
                options.Add(new MatrixFieldOption() { StringValue = "true", MatrixFieldSetting = MatrixFieldSetting.ClearZero });
            if (AlignLeft)
                options.Add(new MatrixFieldOption() { StringValue = "true", MatrixFieldSetting = MatrixFieldSetting.AlignLeft });
            if (AlignRight)
                options.Add(new MatrixFieldOption() { StringValue = "true", MatrixFieldSetting = MatrixFieldSetting.AlignRight });
            if (Decimals != 0)
                options.Add(new MatrixFieldOption() { StringValue = Decimals.ToString(), MatrixFieldSetting = MatrixFieldSetting.Decimals });
            if (MinutesToDecimal)
                options.Add(new MatrixFieldOption() { StringValue = "true", MatrixFieldSetting = MatrixFieldSetting.MinutesToDecimal });
            if (MinutesToTimeSpan)
                options.Add(new MatrixFieldOption() { StringValue = "true", MatrixFieldSetting = MatrixFieldSetting.MinutesToTimeSpan });

            return options;
        }
    }

    [TSInclude]
    public class MatrixLayoutColumn
    {
        public MatrixDataType MatrixDataType { get; set; }
        public string Field { get; set; }
        public string Title { get; set; }
        public int Sort { get; set; }
        public bool Visible { get; set; }
        public MatrixDefinitionColumnOptions Options { get; set; }

        public MatrixLayoutColumn()
        {

        }

        public MatrixLayoutColumn(MatrixDataType matrixDataType, string field, string title, MatrixDefinitionColumnOptions options = null)
        {
            MatrixDataType = matrixDataType;
            Field = field.FirstCharToLowerCase() + (options?.Key ?? "");
            Title = title;
            Options = options;
        }

        public bool IsHidden()
        {
            return Options != null && Options.Hidden;
        }
    }

    public static class MatrixResultHelper
    {
        public static Dictionary<string, object> CreateRow(this List<MatrixField> matrixFields, List<MatrixDefinitionColumn> matrixDefinitionColumns)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (matrixDefinitionColumns != null)
            {
                foreach (MatrixField item in matrixFields)
                {
                    MatrixDefinitionColumn def = matrixDefinitionColumns.FirstOrDefault(f => f.Key == item.ColumnKey);
                    if (def != null && !dictionary.ContainsKey(def.Field))
                        dictionary.Add(def.Field, item.Value);
                }
            }

            return dictionary;
        }
    }

    #endregion

    #region Insights

    public class Insight
    {
        public int InsightId { get; set; }
        public string Name { get; set; }
        public bool ReadOnly { get; set; }

        public List<MatrixLayoutColumn> PossibleColumns { get; set; }
        public List<TermGroup_InsightChartTypes> PossibleChartTypes { get; set; }
        public TermGroup_InsightChartTypes DefaultChartType { get; set; }

        public Insight(int insightId, string name, bool readOnly = false, List<MatrixLayoutColumn> possibleColumns = null, List<TermGroup_InsightChartTypes> possibleChartTypes = null, TermGroup_InsightChartTypes defaultChartType = TermGroup_InsightChartTypes.Pie)
        {
            InsightId = insightId;
            Name = name;
            ReadOnly = readOnly;
            PossibleColumns = !possibleColumns.IsNullOrEmpty() ? possibleColumns : new List<MatrixLayoutColumn>();
            PossibleChartTypes = !possibleChartTypes.IsNullOrEmpty() ? possibleChartTypes : new List<TermGroup_InsightChartTypes>();
            DefaultChartType = defaultChartType;
        }

        public static List<TermGroup_InsightChartTypes> GetAllChartTypes()
        {
            List<TermGroup_InsightChartTypes> types = new List<TermGroup_InsightChartTypes>();

            // TODO: Currently only these are supported
            types.Add(TermGroup_InsightChartTypes.Pie);
            types.Add(TermGroup_InsightChartTypes.Doughnut);
            types.Add(TermGroup_InsightChartTypes.Line);
            types.Add(TermGroup_InsightChartTypes.Bar);
            types.Add(TermGroup_InsightChartTypes.Column);
            types.Add(TermGroup_InsightChartTypes.Area);
            types.Add(TermGroup_InsightChartTypes.Treemap);

            //EnumUtility.GetValues<TermGroup_InsightChartTypes>().ToList().ForEach(t => types.Add(t));

            return types;
        }
    }

    #endregion
}

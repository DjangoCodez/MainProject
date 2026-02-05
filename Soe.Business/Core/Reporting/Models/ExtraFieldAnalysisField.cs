using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Core.Reporting.Models
{
    public class ExtraFieldAnalysisField
    {
        public ExtraFieldAnalysisField(ExtraFieldRecordDTO extraFieldRecord)
        {
            ExtraFieldRecord = extraFieldRecord;
        }
        public ExtraFieldAnalysisField(ExtraFieldRecordDTO extraFieldRecord, Dictionary<int, string> optionList)
        {
            ExtraFieldRecord = extraFieldRecord;

            if (!extraFieldRecord.ExtraFieldValues.IsNullOrEmpty())
            {
                optionList = new Dictionary<int, string>();
                foreach (ExtraFieldValueDTO item in extraFieldRecord.ExtraFieldValues)
                {
                    optionList.Add(item.ExtraFieldValueId, item.Value);
                }
            }
            OptionList = optionList;
        }

        public ExtraFieldRecordDTO ExtraFieldRecord { get; set; }
        private Dictionary<int, string> OptionList { get; set; }
        public string Value
        {
            get
            {
                if (ExtraFieldRecord != null)
                {

                    if (ExtraFieldRecord.DataTypeId == (int)MatrixDataType.Integer && (ExtraFieldRecord.ExtraFieldType == (int)TermGroup_ExtraFieldType.YesNo || ExtraFieldRecord.ExtraFieldType == (int)TermGroup_ExtraFieldType.SingleChoice))
                    {
                        return ExtraFieldRecord.IntData.HasValue && ExtraFieldRecord.IntData.Value != 0 ? OptionList[ExtraFieldRecord.IntData.Value] : string.Empty;
                    }
                    return ExtraFieldRecord.Value;
                }
                else
                    return string.Empty;
            }
        }

        public MatrixDataType MatrixDataType
        {
            get
            {
                if (ExtraFieldRecord != null)
                {
                    switch ((SettingDataType)ExtraFieldRecord.DataTypeId)
                    {
                        case SettingDataType.Boolean:
                            return MatrixDataType.Boolean;
                        case SettingDataType.Decimal:
                            return MatrixDataType.Decimal;
                        case SettingDataType.Integer:
                            return MatrixDataType.Integer;
                        case SettingDataType.String:
                            return MatrixDataType.String;
                        case SettingDataType.Date:
                            return MatrixDataType.Date;
                        default:
                            return MatrixDataType.String;
                    }
                }
                else
                    return MatrixDataType.String;
            }
        }
    }

    public static class ExtraFieldAnalysisFieldExtensions
    {
        public static object ExtraFieldAnalysisFieldValue(this List<ExtraFieldAnalysisField> fields, MatrixDefinitionColumn column)
        {
            MatrixDefinitionColumnOptions options = column.Options;
            if (int.TryParse(options.Key, out int recordId))
            {
                var matched = fields.FirstOrDefault(w => w.ExtraFieldRecord?.ExtraFieldId == recordId);

                if (matched != null)
                    return ObjectValueFromMatrixDataType(matched.MatrixDataType, matched.Value);

            }

            var input = column.Field;
            Match match = Regex.Match(input, @"(\d+)$");

            if (match.Success)
            {
                string digitsString = match.Groups[1].Value;
                int id;
                if (int.TryParse(digitsString, out id))
                {
                    var matched = fields.FirstOrDefault(w => w.ExtraFieldRecord?.ExtraFieldId == id);

                    if (matched != null)
                        return ObjectValueFromMatrixDataType(matched.MatrixDataType, matched.Value);
                }
            }

            return string.Empty;
        }
        public static object ObjectValueFromMatrixDataType(MatrixDataType matrixDataType, object value)
        {
            switch (matrixDataType)
            {
                case MatrixDataType.Boolean:
                    if (value.GetType().Name == "String")
                        return value.ToString().ToLower() == "true" ? true : false;
                    break;
            }

            return value;
        }
        public static void ChangeExtraFieldKey(this MatrixDefinitionColumnOptions options, List<ExtraFieldDTO> from, List<ExtraFieldDTO> to)
        {
            if (int.TryParse(options.Key, out int recordId))
            {
                var matchFrom = from.FirstOrDefault(w => w.ExtraFieldId == recordId);

                if (matchFrom != null)
                {
                    var matchTo = to.FirstOrDefault(f => f.Text.ToLower() == matchFrom.Text.ToLower());

                    if (matchTo != null)
                    {
                        options.Key = matchTo.ExtraFieldId.ToString();
                    }
                }
            }
        }

        public static void ChangeAccountDimIdKey(this MatrixDefinitionColumnOptions options, List<AccountDimDTO> from, List<AccountDimDTO> to)
        {
            if (int.TryParse(options.Key, out int recordId))
            {
                var matchFrom = from.FirstOrDefault(w => w.AccountDimId == recordId);

                if (matchFrom != null)
                {
                    var matchTo = to.FirstOrDefault(f => f.Name == matchFrom.Name);

                    if (matchTo == null)
                        from.FirstOrDefault(f => f.SysSieDimNr == matchFrom.SysSieDimNr);

                    if (matchTo != null)
                    {
                        options.Key = matchTo.AccountDimId.ToString();
                    }
                }
            }
        }
    }
}

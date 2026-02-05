using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class BaseMatrix
    {
        protected InputMatrix inputMatrix { get; set; }
        public BaseMatrix(InputMatrix inputMatrix)
        {
            this.inputMatrix = inputMatrix;
        }

        protected MatrixDefinitionColumn CreateMatrixDefinitionColumn(MatrixLayoutColumn layoutColumn, int columnNumber, MatrixDefinitionColumnOptions options = null)
        {
            MatrixDefinitionColumn column = new MatrixDefinitionColumn()
            {
                Key = Guid.NewGuid(),
                MatrixDataType = layoutColumn.MatrixDataType,
                Field = layoutColumn.Field,
                Title = layoutColumn.Title,
                Options = options,
                ColumnNumber = columnNumber,
                MatrixLayoutColumn = layoutColumn,
            };

            return column;
        }

        protected MatrixDefinitionColumn CreateMatrixDefinitionColumn(MatrixDataType dataType, string field, string title, MatrixDefinitionColumnOptions options = null)
        {
            return CreateMatrixDefinitionColumn(dataType, field, title, 0, options);
        }

        protected MatrixDefinitionColumn CreateMatrixDefinitionColumn(MatrixDataType dataType, string field, string title, int columnNumber = 0, MatrixDefinitionColumnOptions options = null)
        {
            MatrixDefinitionColumn column = new MatrixDefinitionColumn()
            {
                Key = Guid.NewGuid(),
                MatrixDataType = dataType,
                Field = field,
                Title = title,
                Options = options,
                ColumnNumber = columnNumber
            };

            return column;
        }

        protected bool GetEnumId<T>(MatrixDefinitionColumn column, out int id)
        {
            id = 0;
            var result = EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out id, typeof(T), column.Options?.Key == null ? "" : column.Options.Key);

            if (!result)
            {
                // Check if the string ends with one or more digits
                var match = Regex.Match(column.Field.FirstCharToUpperCase(), @"(\d+)$");
                if (match.Success)
                {
                    // Remove the digits from the end of the string and try to parse again
                    var newValue = column.Field.FirstCharToUpperCase().Substring(0, column.Field.FirstCharToUpperCase().Length - match.Groups[1].Length);
                    return EnumUtility.GetValue(newValue, out id, typeof(T), column.Options?.Key == null ? "" : column.Options.Key);
                }
            }

            return result;
        }

        protected string GetText(int id, string fallBack)
        {
            string text = fallBack;
            if (inputMatrix != null && inputMatrix.Terms != null)
            {
                GenericType term = inputMatrix.Terms.FirstOrDefault(t => t.Id == id);
                if (term != null)
                    text = term.Name;
            }

            return text;
        }

        protected Dictionary<int, string> GetTermGroupDict(TermGroup termGroup, int langId = 0, bool addEmptyRow = false, bool includeKey = false)
        {
            try
            {
                return TermCacheManager.Instance.GetTermGroupDict(termGroup, langId, addEmptyRow, includeKey);
            }
            catch
            {
                return new Dictionary<int, string>();
            }
        }

        protected string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }

        protected string GetAccountInternalName<T>(T obj, int startIndex = 1, string name = null)
        {
            if (this.inputMatrix.AccountDims.IsNullOrEmpty())
                return string.Empty;

            if (name == null)
                name = TermCacheManager.Instance.GetText(23, (int)TermGroup.General, "Namn");

            var value = EnumUtility.GetName<T>(obj);

            value = value.Replace("AccountInternal", "");
            bool isNumber = false;
            if (value.Contains("Nr"))
            {
                value = value.Replace("Nr", "");
                isNumber = true;
            }
            else if (value.Contains("Name"))
            {
                value = value.Replace("Name", "");
            }

            if (int.TryParse(value, out int nr))
            {
                if (!inputMatrix.AccountDims.Any(a => a.Level > 0))
                {
                    this.inputMatrix.AccountDims.CalculateLevels();
                }

                foreach (var dim in this.inputMatrix.AccountDims.Where(w => w.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(o => o.Level).ThenBy(o => o.AccountDimNr))
                {
                    if (startIndex == nr)
                    {
                        string dimName = dim.Name;
                        if (!isNumber)
                            dimName = dimName + " " + name;

                        return dimName;
                    }

                    startIndex++;
                }
            }
            return string.Empty;
        }

        protected string GetAccountInternalStdName<T>(T obj)
        {
            return this.inputMatrix.AccountDims.FirstOrDefault(w => w.AccountDimNr == Constants.ACCOUNTDIM_STANDARD).Name;
        }

        protected string GetAccountStdName<T>(T obj, int defaultEmployeeAccountDimId)
        {
            return this.inputMatrix.AccountDims.FirstOrDefault(w => w.AccountDimId == defaultEmployeeAccountDimId).Name;
        }

        protected AccountInternalDTO GetAccountInternal(List<AccountInternalDTO> accountInternals, int index, int startIndex = 1)
        {
            if (accountInternals.IsNullOrEmpty() || this.inputMatrix.AccountDims.IsNullOrEmpty())
                return null;

            foreach (var dim in this.inputMatrix.AccountDims.Where(w => w.AccountDimNr != 1).OrderBy(o => o.AccountDimNr))
            {
                if (startIndex == index)
                {
                    var accountInternal = accountInternals.FirstOrDefault(f => f.AccountDimId == dim.AccountDimId);

                    if (accountInternal != null)
                        return accountInternal;
                }
                startIndex++;
            }

            return null;
        }

        protected bool IsAccountInternal<T>(T obj)
        {
            if (this.inputMatrix.AccountDims.IsNullOrEmpty())
                return false;

            return EnumUtility.GetName<T>(obj).Contains("AccountInternal");
        }

        protected bool IsExtraField<T>(T obj)
        {
            if (this.inputMatrix.AccountDims.IsNullOrEmpty())
                return false;

            return EnumUtility.GetName<T>(obj).Contains("ExtraField");
        }

        protected bool IsAccountInternalStd<T>(T obj)
        {
            if (this.inputMatrix.AccountDims.IsNullOrEmpty())
                return false;

            return EnumUtility.GetName<T>(obj).Contains("AccountInternalStd");
        }

        protected bool IsAccountStd<T>(T obj)
        {
            if (this.inputMatrix.AccountDims.IsNullOrEmpty())
                return false;

            return EnumUtility.GetName<T>(obj).Contains("AccountStd");
        }

        protected bool HasReadPermission(Feature feature)
        {
            return this.inputMatrix?.PermissionRepository?.HasReadPermission(this.inputMatrix?.PermissionParam, feature) ?? false;
        }

        public MatrixDataType SetMatrixDataType(TermGroup_ExtraFieldType extraFieldTypeId)
        {
            var matrixDataType = MatrixDataType.String;
            switch (extraFieldTypeId)
            {
                case TermGroup_ExtraFieldType.FreeText:
                case TermGroup_ExtraFieldType.YesNo:
                case TermGroup_ExtraFieldType.Decimal:
                case TermGroup_ExtraFieldType.Integer:
                case TermGroup_ExtraFieldType.Date:
                case TermGroup_ExtraFieldType.SingleChoice:
                    break;
                case TermGroup_ExtraFieldType.Checkbox:
                    matrixDataType = MatrixDataType.Boolean;
                    break;
                default:
                    break;
            }
            return matrixDataType;

        }
    }
}

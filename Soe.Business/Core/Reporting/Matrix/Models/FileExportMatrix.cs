using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public static class FileExportMatrix
    {
        public static byte[] GetExportFile(MatrixResult matrixResult, ExportDefinitionDTO exportDefinition, CompanyDTO company)
        {
            var rows = matrixResult.GetMatrixRows();
            string delimiterSetting = exportDefinition.Separator;
            bool hasSkipRowRules = exportDefinition.ExportDefinitionLevels.HasSkipRowRules();
            if (string.IsNullOrEmpty(delimiterSetting))
                return null;
            StringBuilder sb = new StringBuilder();

            if (exportDefinition.ExportDefinitionLevels.FirstOrDefault()?.UseColumnHeaders ?? false)
                rows.Insert(0, rows.First().CloneDTO());

            foreach (var level in exportDefinition.ExportDefinitionLevels.OrderBy(o => o.Level))
            {
                List<MatrixDefinitionColumn> columns = new List<MatrixDefinitionColumn>();

                if (!matrixResult.MatrixDefinitions.IsNullOrEmpty())
                    columns = matrixResult.MatrixDefinitions.Where(w => w.Key == 0 || w.Key == level.ExportDefinitionLevelId)?.SelectMany(sm => sm.MatrixDefinitionColumns.Where(c => !c.IsHidden())).ToList() ?? new List<MatrixDefinitionColumn>();

                if (columns.IsNullOrEmpty())
                    columns = matrixResult.MatrixDefinition?.MatrixDefinitionColumns.Where(c => !c.IsHidden()).ToList() ?? new List<MatrixDefinitionColumn>();

                if (level.ExportDefinitionLevelColumns.IsNullOrEmpty())
                {
                    level.ExportDefinitionLevelColumns = CreatecolumnsFromMatrixFields(columns);
                }
                else
                {
                    int position = 1;
                    foreach (var col in level.ExportDefinitionLevelColumns)
                    {
                        var matrixDefinitionColumn = string.IsNullOrEmpty(col.Key) ? null : columns.FirstOrDefault(f => f.MatrixLayoutColumn.Field.Equals(col.Key.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (matrixDefinitionColumn != null)
                            col.MatrixDefinitionColumn = matrixDefinitionColumn;
                        else
                            col.MatrixDefinitionColumn = new MatrixDefinitionColumn() { ColumnNumber = position, MatrixDataType = MatrixDataType.String, Title = col.Name };

                        position++;
                    }
                }

                foreach (var row in rows.Where(w => w.Key == 0 || w.Key == level.ExportDefinitionLevelId))
                {
                    bool skipRow = false;

                    bool isHeaderRow = rows.First() == row && level.UseColumnHeaders;
                    StringBuilder sbRow = new StringBuilder();
                    var last = level.ExportDefinitionLevelColumns.OrderBy(o => o.Position).Last();

                    foreach (var col in level.ExportDefinitionLevelColumns.OrderBy(o => o.Position))
                    {
                        string delimiter = delimiterSetting;
                        delimiter = last == col ? "" : delimiterSetting;

                        MatrixDefinitionColumnOptions columnOptions = col.MatrixDefinitionColumn?.Options;
                        var field = row.MatrixFields.FirstOrDefault(f => f.ColumnKey == col.MatrixDefinitionColumn.Key);

                        if (isHeaderRow)
                        {
                            if (col.HideColumn())
                                continue;

                            if (!string.IsNullOrEmpty(col.ColumnHeader))
                                sbRow.Append(CreateString(col, field, delimiter, col.ColumnHeader, isHeaderRow));
                            else
                                sbRow.Append(CreateString(col, field, delimiter, col.MatrixDefinitionColumn.Field, isHeaderRow));
                            continue;
                        }

                        if (field == null && !string.IsNullOrEmpty(col.DefaultValue))
                        {
                            sbRow.Append(CreateString(col, field, delimiter, col.DefaultValue));
                            continue;
                        }

                        if (field == null && !string.IsNullOrEmpty(col.GetValueWithKey(company)))
                        {
                            sbRow.Append(CreateString(col, field, delimiter, col.GetValueWithKey(company)));
                            continue;
                        }

                        if (field == null)
                        {
                            sbRow.Append(CreateString(col, field, delimiter, string.Empty));
                            continue;
                        }
                        else
                        {
                            var fieldOptions = field.MatrixFieldOptions;

                            if (fieldOptions.IsNullOrEmpty() && columnOptions != null)
                                fieldOptions = columnOptions.GetMatrixFieldOptions();

                            var value = field.Value;
                            if (value == null)
                                value = col.DefaultValue ?? string.Empty;

                            if (hasSkipRowRules && SkipRowBasedOnColumn(col, value.ToString()))
                                skipRow = true;

                            if ((columnOptions != null && columnOptions.Hidden) || col.HideColumn())
                                continue;

                            if (field.MatrixDataType == MatrixDataType.Time && !value.ToString().Contains(":"))
                            {
                                int minutes = 0;
                                int.TryParse(value.ToString(), out minutes);
                                value = CalendarUtility.FormatTimeSpan(minutes);
                            }

                            if (!string.IsNullOrEmpty(delimiter) && value.ToString().Contains(delimiter))
                                value = value.ToString().Replace(delimiter, " ");

                            switch (field.MatrixDataType)
                            {
                                case MatrixDataType.String:
                                    sbRow.Append(CreateString(col, field, delimiter, value.ToString()));
                                    break;
                                case MatrixDataType.Integer:
                                    sbRow.Append(CreateString(col, field, delimiter, value.ToString()));
                                    break;
                                case MatrixDataType.Boolean:
                                    int boolValue = (value != null && (bool)field.Value ? 1 : 0);
                                    sbRow.Append(CreateString(col, field, delimiter, boolValue.ToString()));
                                    break;
                                case MatrixDataType.Date:
                                    var format = col.FormatDate ?? "yyyy-MM-dd";
                                    if (DateTime.TryParse(value.ToString(), out DateTime date))
                                        sbRow.Append(CreateString(col, field, delimiter, date.ToString(format)));
                                    else
                                        sbRow.Append(CreateString(col, field, delimiter, value.ToString()));
                                    break;
                                case MatrixDataType.Decimal:
                                    var numberOfDecimals = 2;
                                    var option = fieldOptions?.FirstOrDefault(f => f.MatrixFieldSetting == MatrixFieldSetting.Decimals);
                                    if (option != null)
                                    {
                                        int.TryParse(option.StringValue, out int optionNumberOfDecimals);
                                        numberOfDecimals = optionNumberOfDecimals;
                                    }
                                    if (decimal.TryParse(value.ToString(), out decimal dec))
                                    {
                                        dec = decimal.Round(dec, numberOfDecimals);
                                        sbRow.Append(CreateString(col, field, delimiter, dec.ToString()));
                                    }
                                    else
                                        sbRow.Append(CreateString(col, field, delimiter, "0"));
                                    break;
                                case MatrixDataType.Time:
                                    var formatDT = col.FormatDate ?? "HH:mm";
                                    if (DateTime.TryParse(value.ToString(), out DateTime dateT))
                                        sbRow.Append(CreateString(col, field, delimiter, dateT.ToString(formatDT)));
                                    else
                                        sbRow.Append(CreateString(col, field, delimiter, value.ToString()));
                                    break;
                                case MatrixDataType.DateAndTime:
                                    var formatT = col.FormatDate ?? "yyyy-mm-dd HH:mm";
                                    if (DateTime.TryParse(value.ToString(), out DateTime dateDT))
                                        sbRow.Append(CreateString(col, field, delimiter, dateDT.ToString(formatT)));
                                    else
                                        sbRow.Append(CreateString(col, field, delimiter, value.ToString()));
                                    break;
                                default:
                                    sbRow.Append(CreateString(col, field, delimiter, string.Empty));
                                    break;
                            }
                        }

                    }
                    if (!skipRow)
                        sb.Append(sbRow + Environment.NewLine);
                }
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static List<ExportDefinitionLevelColumnDTO> CreatecolumnsFromMatrixFields(List<MatrixDefinitionColumn> columns)
        {
            List<ExportDefinitionLevelColumnDTO> dtos = new List<ExportDefinitionLevelColumnDTO>();
            int position = 1;
            foreach (var col in columns)
            {
                dtos.Add(CreatecolumnFromMatrixFields(col, position));
                position++;
            }

            return dtos;
        }

        private static ExportDefinitionLevelColumnDTO CreatecolumnFromMatrixFields(MatrixDefinitionColumn col, int position)
        {
            return new ExportDefinitionLevelColumnDTO()
            {
                MatrixDefinitionColumn = col,
                Position = position,
                FormatDate = col.MatrixDataType == MatrixDataType.Date ? "yyyy-mm-dd" : col.MatrixDataType == MatrixDataType.DateAndTime ? "yyyy-mm-dd HH:mm" : "HH:mm",
                Key = col.Field
            };
        }

        private static string CreateString(ExportDefinitionLevelColumnDTO exportDefinitionLevelColumn, MatrixField field, string delimiter, string value, bool isHeaderRow = false)
        {
            StringBuilder sbRow = new StringBuilder();
            value = value.Trim();

            if (!isHeaderRow && !string.IsNullOrEmpty(exportDefinitionLevelColumn.ConvertValue))
                value = GetStringValue(value, exportDefinitionLevelColumn.ConvertValue);

            if (exportDefinitionLevelColumn.IsDelimiter)
                sbRow.Append(value + delimiter);
            else if (exportDefinitionLevelColumn.IsPosition)
                sbRow.Append(FillWithChar(exportDefinitionLevelColumn, value));

            return sbRow.ToString();
        }

        private static string FillWithChar(ExportDefinitionLevelColumnDTO exportDefinitionLevelColumn, string originValue)
        {
            var character = exportDefinitionLevelColumn.FillChar ?? " ";
            var beginning = exportDefinitionLevelColumn.FillBeginning ?? false;

            return FillWithChar(character, exportDefinitionLevelColumn.ColumnLength, originValue, true, beginning);
        }

        public static string FillWithChar(string character, int targetSize, string originValue, bool truncate = false, bool beginning = false)
        {
            if (string.IsNullOrEmpty(character))
                character = " ";

            if (targetSize == originValue.Length)
                return originValue;

            if (targetSize > originValue.Length)
            {
                string newChars = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    newChars += character;
                }
                if (beginning)
                    return (newChars + originValue);
                else
                    return (originValue + newChars);
            }
            else if (truncate && targetSize < originValue.Length)
                return originValue.Substring(0, targetSize);
            else
                return originValue;
        }

        private static bool HideColumn(this ExportDefinitionLevelColumnDTO column)
        {
            return !string.IsNullOrEmpty(column.ConvertValue) && column.ConvertValue.Contains("HideColumn");
        }

        private static bool HasSkipRowRules(this List<ExportDefinitionLevelDTO> levelDTOs)
        {
            return !levelDTOs.IsNullOrEmpty() && levelDTOs.Any(a => a.ExportDefinitionLevelColumns.Any(aa => aa.HasSkipRowRules()));
        }

        private static bool HasSkipRowRules(this List<ExportDefinitionLevelColumnDTO> columns)
        {
            return !columns.IsNullOrEmpty() && columns.Any(a => a.HasSkipRowRules());
        }
        private static bool HasSkipRowRules(this ExportDefinitionLevelColumnDTO column)
        {
            return !string.IsNullOrEmpty(column.ConvertValue) && column.ConvertValue.Contains("SkipRow");
        }

        public static bool SkipRowBasedOnColumn(ExportDefinitionLevelColumnDTO column, string value)
        {
            if (!column.HasSkipRowRules())
                return false;

            if (column.ConvertValue.Contains("SkipRowIfEmptyOrZero") && (string.IsNullOrEmpty(value) || (int.TryParse(value, out int intValue) && intValue == 0) || (decimal.TryParse(value, out decimal decValue) && decValue == 0)))
                return true;

            if (column.ConvertValue.Contains("SkipRowIfNowEmpty") && !string.IsNullOrEmpty(value))
                return true;

            if (column.ConvertValue.Contains("SkipRowIfValue|") && !string.IsNullOrEmpty(value))
            {
                var arr = column.ConvertValue.Split('^');

                foreach (var conv in arr.Where(w => w.Contains("SkipRowIfValue|")))
                {
                    var skipIfValueArr = conv.Split('|');

                    if (skipIfValueArr.Length > 1)
                    {
                        if (skipIfValueArr[1].Equals(value, StringComparison.OrdinalIgnoreCase))
                            return true;

                        if (decimal.TryParse(skipIfValueArr[1], out decimal decRule) && decimal.TryParse(value, out decimal decVal) && decRule == decVal)
                            return true;
                    }
                }
            }
            if (column.ConvertValue.Contains("SkipRowIfDateOlder|") && !string.IsNullOrEmpty(value) && DateTime.TryParse(value, out DateTime date))
            {
                var arr = column.ConvertValue.Split('^');

                foreach (var conv in arr.Where(w => w.Contains("SkipRowIfDateOlder|")))
                {
                    var skipIfDateOlderArr = conv.Split('|');

                    if (skipIfDateOlderArr.Length > 1)
                    {
                        var limitDate = GetDate(skipIfDateOlderArr[1]);

                        if (date < limitDate)
                            return true;
                    }
                }
            }
            if (column.ConvertValue.Contains("SkipRowIfContains|") && !string.IsNullOrEmpty(value))
            {
                var arr = column.ConvertValue.Split('^');

                foreach (var conv in arr.Where(w => w.Contains("SkipRowIfContains|")))
                {
                    var skipIfValueArr = conv.Split('|');

                    if (skipIfValueArr.Length > 1)
                    {
                        if (value.Contains(skipIfValueArr[1]))
                            return true;

                        if (decimal.TryParse(skipIfValueArr[1], out decimal decRule) && decimal.TryParse(value, out decimal decVal) && decRule == decVal)
                            return true;
                    }
                }
            }

            return false;
        }

        public static DateTime? GetDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(value, out var date))
                return date;

            if (value.StartsWith("DateTime."))
                value = value.Replace("DateTime.", "");

            if (value.Equals(nameof(DateTime.Now), StringComparison.OrdinalIgnoreCase))
                return DateTime.Now;

            if (value.Equals(nameof(DateTime.Today), StringComparison.OrdinalIgnoreCase))
                return DateTime.Today;

            int punctuationIndex = value.IndexOfAny(new char[] { '.', '(' });
            if (punctuationIndex > 0)
            {
                string timeValue = value.Substring(0, punctuationIndex);
                var timeProp = typeof(DateTime).GetProperty(timeValue);
                if (timeProp != null && value.Length > timeProp.Name.Length + 1)
                {
                    var methodCall = value.Substring(timeProp.Name.Length + 1);
                    if ((methodCall.StartsWith("Add") || methodCall.StartsWith("Subtract")) && methodCall.EndsWith(")"))
                    {
                        var methodParams = methodCall.Substring(methodCall.IndexOf('(') + 1, methodCall.Length - methodCall.IndexOf('(') - 2);
                        if (int.TryParse(methodParams, out var amount))
                        {
                            var methodName = methodCall.Substring(0, methodCall.IndexOf("("));
                            var method = typeof(DateTime).GetMethod(methodName);
                            if (method != null)
                            {
                                var result = method.Invoke(DateTime.Today, new object[] { amount });
                                return (DateTime)result;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static string GetStringValue(string value, string convert)
        {
            var returnValue = value;

            if (!string.IsNullOrEmpty(convert))
            {
                var arr = convert.Split('^');

                foreach (var conv in arr)
                {
                    try
                    {
                        if (conv.Contains("FirstInArray|")) //Example FirstInArray|,|
                        {
                            var firstInArrayArr = conv.Split('|');

                            if (firstInArrayArr.Length > 1)
                            {
                                if (returnValue.Contains(firstInArrayArr[1]) && firstInArrayArr[1].Length == 1)
                                {
                                    char c = char.Parse(firstInArrayArr[1]);
                                    var splittedValue = returnValue.Split(c);

                                    if (splittedValue.Length > 1)
                                    {
                                        returnValue = splittedValue[0];
                                    }
                                }
                            }
                        }
                        if (conv.Contains("Replace|")) //Example Replace|-||
                        {
                            var replaceArr = conv.Split('|');

                            if (replaceArr.Length > 2)
                            {
                                if (replaceArr[1] == "*.*")
                                    returnValue = string.Empty;
                                else
                                    returnValue = returnValue.Replace(replaceArr[1], replaceArr[2]);
                            }
                        }
                        else if (conv.Contains("RemoveBeginning|"))  //Example RemoveBeginning|2| remove first to characters when possible
                        {
                            var removeBeginningArr = conv.Split('|');

                            if (removeBeginningArr.Length > 1)
                            {
                                if (int.TryParse(removeBeginningArr[1], out int removePositions))
                                {
                                    int orginalLength = returnValue.Length;

                                    if (orginalLength >= removePositions)
                                        returnValue = returnValue.Remove(0, removePositions);
                                }
                            }
                        }
                        else if (conv.Contains("TruncateAt|"))  //Example TruncateAt|2|left|  Truncate string remove from left
                        {
                            var truncateArr = conv.Split('|');

                            if (truncateArr.Length > 2)
                            {
                                if (int.TryParse(truncateArr[1], out int length))
                                {
                                    int orginalLength = returnValue.Length;

                                    if (orginalLength > length)
                                        returnValue = FillWithChar(" ", length, returnValue, true, truncateArr[2] == "left");
                                }
                            }
                        }
                        else if (conv.Contains("FillWithChar|"))  //Example FillWithChar|_|10|left|  FillWithChar fill right/left with character
                        {
                            var fillWithCharArr = conv.Split('|');

                            if (fillWithCharArr.Length > 3)
                            {
                                if (int.TryParse(fillWithCharArr[2], out int length))
                                {
                                    int orginalLength = returnValue.Length;
                                    string delimiter = fillWithCharArr[1];

                                    if (delimiter.Length == 0)
                                        delimiter = " ";

                                    if (orginalLength <= length)
                                        returnValue = FillWithChar(delimiter, length, returnValue, true, fillWithCharArr[3] == "left");
                                }
                            }
                        }
                        else if (conv.Contains("WhenContains|"))  //Example WhenContains|Tjä|1|O|  Set string as "1" when string contains "Tjä"
                        {
                            var replaceArr = conv.Split('|');

                            if (replaceArr.Length > 2 && !string.IsNullOrEmpty(replaceArr[1]))
                            {
                                if (returnValue.Contains(replaceArr[1]))
                                    returnValue = replaceArr[2];
                                else if (replaceArr.Length > 3 && !string.IsNullOrEmpty(replaceArr[3]))
                                    returnValue = replaceArr[3];
                            }
                        }
                        else if (conv.Contains("WhenStartsWith|"))  //Example WhenStartsWith|Tjä|1|O|  Set string as "1" when string starts with "Tjä"
                        {
                            var replaceArr = conv.Split('|');

                            if (replaceArr.Length > 2 && !string.IsNullOrEmpty(replaceArr[1]))
                            {
                                if (returnValue.StartsWith(replaceArr[1]))
                                    returnValue = replaceArr[2];
                                else if (replaceArr.Length > 3 && !string.IsNullOrEmpty(replaceArr[3]))
                                    returnValue = replaceArr[3];
                            }
                        }
                        else if (conv.Contains("FillBeginning|"))  //Example FillBeginning|THR| Set THR in beginning of string
                        {
                            var replaceArr = conv.Split('|');

                            if (replaceArr.Length > 1 && !string.IsNullOrEmpty(replaceArr[1]))
                            {
                                returnValue = replaceArr[1] + returnValue;
                            }
                        }
                        else if (conv.Contains("ConvertDecimal|"))
                        {
                            var convertDecimalArr = conv.Split('|');

                            if (convertDecimalArr.Length > 1 && decimal.TryParse(value, out decimal dec))
                            {
                                var syntax = convertDecimalArr[1];
                                var integerPart = Convert.ToInt32(Math.Floor(dec));
                                var decimalPart = dec - integerPart;

                                // Count the number of 'I' and digits in the syntax
                                int integerDigits = syntax.Count(c => c == 'I');
                                int decimalDigits = syntax.Count(c => c == 'D');

                                if (integerDigits == 0 && decimalDigits == 0)
                                {
                                    // Handle incorrect format string
                                    returnValue = "IncorrectFormat"; // Or any other error handling
                                }
                                else
                                {
                                    // Format the integer and decimal parts separately
                                    string integerFormatted = integerPart.ToString("D" + integerDigits);

                                    // Adjust decimal part to have the correct number of digits after the decimal point
                                    decimalPart *= (decimal)Math.Pow(10, decimalDigits);
                                    int decimalPartAsInt = (int)decimalPart; // Cast to int to remove fractional part
                                    string decimalFormatted = decimalPartAsInt.ToString().PadLeft(decimalDigits, '0');

                                    // Find the separator (any character that is not 'I' or a digit)
                                    char separator = '\0';
                                    foreach (char c in syntax)
                                    {
                                        if (!char.IsDigit(c) && c != 'I')
                                        {
                                            separator = c;
                                            break;
                                        }
                                    }

                                    // Combine and format as per the original syntax
                                    string formattedValue;
                                    if (separator != '\0')
                                    {
                                        formattedValue = integerFormatted + separator + (decimalDigits > 0 ? decimalFormatted : string.Empty);
                                    }
                                    else
                                    {
                                        formattedValue = integerFormatted + (decimalDigits > 0 ? decimalFormatted : string.Empty);
                                    }

                                    returnValue = formattedValue;
                                }
                            }
                        }


                    }
                    catch
                    {
                        // Intentionally ignored, safe to continue
                        // NOSONAR
                    }
                }
            }

            return returnValue;
        }

        public static ReportUserSelection TryGetReportSelectionFromExport(ExportDTO export, int actorCompanyId, int userId)
        {
            if (string.IsNullOrEmpty(export?.SpecialFunctionality))
                return null;

            var split = export.SpecialFunctionality.Split('^').ToList();
            var matched = split.FirstOrDefault(f => f.Contains("ReportId:"));

            if (matched != null)
            {
                var splitMatch = matched.Split(':');

                if (splitMatch.Length > 1)
                {
                    if (int.TryParse(splitMatch[1], out int reportId))
                    {
                        return new ReportUserSelection()
                        {
                            ActorCompanyId = actorCompanyId,
                            UserId = userId,
                            ReportId = reportId
                        };
                    }
                }
            }

            return null;
        }

        public static string[] ConvertValues()
        {
            return new string[]
            {
                "SkipRowIfEmptyOrZero",
                "SkipRowIfNowEmpty",
                "SkipRowIfValue|",
                "SkipRowIfContains|",
                "Replace|",
                "RemoveBeginning|",
                "TruncateAt|",
                "FillWithChar|",
                "WhenContains|",
                "WhenStartsWith|",
                "HideColumn",
             };
        }
    }

    public enum FileExportConvertValues
    {
        SkipRowIfEmptyOrZero = 0,
        SkipRowIfNowEmpty = 1,
        SkipRowIfValue = 2,
        Replace = 3,
        RemoveBeginning = 4,
        TruncateAt = 5,
        FillWithChar = 6,
        WhenContains = 7,
        WhenStartsWith = 8,
        HideColumn = 9,
    }
}

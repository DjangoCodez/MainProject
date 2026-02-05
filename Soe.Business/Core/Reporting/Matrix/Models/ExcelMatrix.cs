using MiniExcelLibs;
using OfficeOpenXml;
using OfficeOpenXml.Packaging;
using OfficeOpenXml.Style;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public static class ExcelMatrix
    {
        public static void SaveExcelFile(string path, MatrixResult matrixResult, string sheetName)
        {
            var arr = GetExcelFile(matrixResult, sheetName);

            try
            {
                File.WriteAllBytes(path, arr);
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex.ToString());
            }
        }
        public static byte[] GetTextFile(MatrixResult matrixResult, char delimiter, int startOnRow)
        {
            try
            {
                List<MatrixField> fields = matrixResult.MatrixFields;
                List<MatrixDefinitionColumn> columns = matrixResult.MatrixDefinition.MatrixDefinitionColumns.Where(c => !c.IsHidden()).ToList();
                using (var package = new ExcelPackage())
                {
                    AddSheet(package, "Sheet", fields, columns);
                    byte[] excelBytes = package.ConvertToDelimitedFile(delimiter, 1, startOnRow);
                    return excelBytes;
                }
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex.ToString());
            }

            return null;
        }

        public static byte[] GetExcelFile(MatrixResult matrixResult, string sheetName)
        {
            // Define file paths for the temporary and final files
            var tempFilePath = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + Guid.NewGuid().ToString() + "_FS";
            var finalFilePath = tempFilePath + "_2";

            // Extract fields and columns from the matrix result
            List<MatrixField> fields = matrixResult.MatrixFields;
            List<MatrixDefinitionColumn> columns = matrixResult.MatrixDefinition.MatrixDefinitionColumns
                                                    .Where(c => !c.IsHidden()).ToList();

            try
            {
                // Check if the field count is below 2 million for in-memory processing
                if (fields.Count < 2000000)
                {
                    return CreateExcelInMemory(sheetName, fields, columns);
                }
                else
                {
                    LogCollector.LogInfo("Matrix has more than 2 million cells, switching to file-based processing to avoid memory issues.");
                    return CreateExcelUsingFileStream(sheetName, fields, columns, tempFilePath, finalFilePath);
                }
            }
            catch (Exception ex)
            {
                LogCollector.LogError($"Error generating Excel file. Retrying with filestream. Exception: {ex}");
                return CreateExcelUsingFileStream(sheetName, fields, columns, tempFilePath, tempFilePath + "_2");
            }
            finally
            {
                CleanupTemporaryFiles(tempFilePath, finalFilePath);
            }
        }

        private static byte[] CreateExcelInMemory(string sheetName, List<MatrixField> fields, List<MatrixDefinitionColumn> columns)
        {
            using (var package = new ExcelPackage())
            {
                AddSheet(package, sheetName, fields, columns);
                return package.GetAsByteArray();
            }
        }

        private static byte[] CreateExcelUsingFileStream(string sheetName, List<MatrixField> fields, List<MatrixDefinitionColumn> columns, string tempFilePath, string finalFilePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                using (var package = new ExcelPackage(fileStream))
                {
                    AddSheet(package, sheetName, fields, columns);
                    package.SaveAs(new FileInfo(finalFilePath));
                }

                return File.ReadAllBytes(finalFilePath);
            }
            catch (Exception ex)
            {
                CleanupTemporaryFiles(tempFilePath, finalFilePath);
                LogCollector.LogError($"Error in file-based Excel creation. Exception: {ex}");
                return CreateExcelWithMiniExcel(sheetName, fields, columns, tempFilePath);
            }
            finally
            {
                CleanupTemporaryFiles(tempFilePath, finalFilePath);
            }
        }

        private static byte[] CreateExcelWithMiniExcel(string sheetName, List<MatrixField> fields, List<MatrixDefinitionColumn> columns, string tempFilePath)
        {
            try
            {
                AddSheetWithMiniExcelOptimized(tempFilePath, sheetName, fields, columns);
                return File.ReadAllBytes(tempFilePath);
            }
            catch (Exception miniExcelEx)
            {
                LogCollector.LogError($"MiniExcel file creation failed. Exception: {miniExcelEx}");
                return null;
            }
            finally
            {
                CleanupTemporaryFiles(tempFilePath);
            }
        }

        private static void CleanupTemporaryFiles(params string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                try
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                }
                catch (IOException ex)
                {
                    LogCollector.LogError($"Failed to delete temporary file: {filePath}. Exception: {ex}");
                }
            }
        }

        public static byte[] ConvertToDelimitedFile(ExcelPackage package)
        {
            return package.ConvertToDelimitedFile(startOnRow: 2);
        }

        private static void AddSheet(ExcelPackage package, string sheetName, List<MatrixField> fields, List<MatrixDefinitionColumn> matrixDefinitionColumns)
        {
            ExcelWorksheet sheet = package.Workbook.Worksheets.Add(sheetName);


            int column = 1;
            List<MatrixDefinitionColumnOptions> groupBys = new List<MatrixDefinitionColumnOptions>();
            int outlineLevel = 0;
            fields = fields.OrderBy(o => o.RowNumber).ToList();
            var fieldColumKeyDict = fields.GroupBy(f => f.ColumnKey).ToDictionary(k => k.Key, v => v.ToList());

            foreach (var def in matrixDefinitionColumns)
            {
                MatrixDefinitionColumnOptions columnOptions = def.Options;

                if (columnOptions != null && columnOptions.GroupBy)
                    groupBys.Add(columnOptions);

                if (fieldColumKeyDict.TryGetValue(def.Key, out List<MatrixField> matchingFields))
                {
                    sheet.Cells[1, column].Value = def.Title;
                    sheet.Cells[1, column].Style.Font.Bold = true;

                    foreach (MatrixField field in matchingFields)
                    {
                        int row = field.RowNumber + 1;

                        try
                        {
                            List<MatrixFieldOption> fieldOptions = field.MatrixFieldOptions;

                            if (fieldOptions.IsNullOrEmpty() && columnOptions != null)
                                fieldOptions = columnOptions.GetMatrixFieldOptions();

                            if (columnOptions != null && column == 1 && outlineLevel == 0 && columnOptions.GroupBy)
                                outlineLevel++;

                            var value = field.Value ?? string.Empty;
                            if (field.MatrixDataType == MatrixDataType.Time && !value.ToString().Contains(":"))
                            {
                                int.TryParse(value.ToString(), out int minutes);
                                if (columnOptions.MinutesToDecimal)
                                    value = (float)minutes / 60;
                                else if (columnOptions.MinutesToTimeSpan)
                                    value = CalendarUtility.FormatTimeSpan(minutes);
                                else
                                    value = minutes;
                            }
                            sheet.Cells[row, column].Value = value;

                            if (outlineLevel != 0)
                                sheet.Row(row).OutlineLevel = outlineLevel;

                            switch (field.MatrixDataType)
                            {
                                case MatrixDataType.String:
                                    break;
                                case MatrixDataType.Integer:
                                    sheet.Cells[row, column].Style.Numberformat.Format = "0";
                                    break;
                                case MatrixDataType.Boolean:
                                    sheet.Cells[row, column].Value = (field.Value != null && !string.IsNullOrWhiteSpace(field.Value.ToString()) && (bool)field.Value) ? "1" : "0";
                                    sheet.Cells[row, column].Style.Numberformat.Format = "0";
                                    break;
                                case MatrixDataType.Date:
                                    sheet.Cells[row, column].Style.Numberformat.Format = "yyyy-mm-dd";
                                    break;
                                case MatrixDataType.Decimal:
                                    int numberOfDecimals = 2;
                                    MatrixFieldOption option = fieldOptions?.FirstOrDefault(f => f.MatrixFieldSetting == MatrixFieldSetting.Decimals);
                                    if (option != null)
                                    {
                                        int.TryParse(option.StringValue, out int optionNumberOfDecimals);
                                        numberOfDecimals = optionNumberOfDecimals;
                                    }
                                    sheet.Cells[row, column].Style.Numberformat.Format = "0." + StringUtility.FillWithZerosBeginning(numberOfDecimals, "");
                                    break;
                                case MatrixDataType.Time:
                                    if (columnOptions.MinutesToDecimal)
                                        sheet.Cells[row, column].Style.Numberformat.Format = "0.00";
                                    else if (columnOptions.MinutesToTimeSpan)
                                        sheet.Cells[row, column].Style.Numberformat.Format = "HH:mm";
                                    else
                                        sheet.Cells[row, column].Style.Numberformat.Format = "0";
                                    break;
                                case MatrixDataType.DateAndTime:
                                    sheet.Cells[row, column].Style.Numberformat.Format = "yyyy-mm-dd HH:mm";
                                    break;
                                default:
                                    break;
                            }

                            if (!fieldOptions.IsNullOrEmpty())
                            {
                                foreach (MatrixFieldOption option in fieldOptions)
                                {
                                    switch (option.MatrixFieldSetting)
                                    {
                                        case MatrixFieldSetting.BackgroundColor:
                                            if (string.IsNullOrEmpty(option.StringValue))
                                                continue;

                                            Color colFromHex = ColorTranslator.FromHtml(option.StringValue);
                                            sheet.Cells[row, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            sheet.Cells[row, column].Style.Fill.BackgroundColor.SetColor(colFromHex);
                                            break;
                                        case MatrixFieldSetting.FontColor:
                                            if (string.IsNullOrEmpty(option.StringValue))
                                                continue;

                                            Color colFromH = ColorTranslator.FromHtml(option.StringValue);
                                            sheet.Cells[row, column].Style.Font.Color.SetColor(colFromH);
                                            break;
                                        case MatrixFieldSetting.BoldFont:
                                            sheet.Cells[row, column].Style.Font.Bold = true;
                                            break;
                                        case MatrixFieldSetting.ClearZero:
                                            if (option.IsTrueBool())
                                                sheet.Cells[row, column].Value = null;
                                            break;
                                        case MatrixFieldSetting.AlignLeft:
                                            if (option.IsTrueBool())
                                                sheet.Cells[row, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                            break;
                                        case MatrixFieldSetting.AlignRight:
                                            if (option.IsTrueBool())
                                                sheet.Cells[row, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                            break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogCollector.LogError(ex, $"row {row} col {column} value {field.Value as string} fields {fields.Count}");
                            throw;
                        }
                    }
                }
                column++;
            }

            if (sheet.Cells.IsNullOrEmpty())
                sheet.Cells[1, 1].Value = "No Data";

            //get the workbook as a bytearray
            sheet.Cells[sheet.Dimension.Address].AutoFilter = true;
            sheet.Cells.AutoFitColumns();
        }


        public static void AddSheetWithMiniExcelOptimized(string filePath, string sheetName, List<MatrixField> fields, List<MatrixDefinitionColumn> matrixDefinitionColumns)
        {
            // Step 1: Prepare an initial list of dictionaries to define the structure for rows
            var rows = new List<Dictionary<string, object>>();

            // Create empty dictionaries for the number of rows in the matrix
            var maxRowNumber = fields.Max(f => f.RowNumber);
            for (int i = 0; i <= maxRowNumber; i++)
            {
                var rowDict = new Dictionary<string, object>();
                rows.Add(rowDict);
            }

            // Step 2: Populate columns one at a time
            foreach (var def in matrixDefinitionColumns)
            {
                if (fields.Any(f => f.ColumnKey == def.Key))
                {
                    var matchingFields = fields.Where(f => f.ColumnKey == def.Key).OrderBy(f => f.RowNumber).ToList();

                    foreach (var field in matchingFields)
                    {
                        int rowIndex = field.RowNumber;

                        // Add the value for the current column into the existing row dictionary
                        if (rowIndex < rows.Count)
                        {
                            var rowDict = rows[rowIndex];
                            rowDict[def.Title] = ConvertFieldValue(field);
                        }
                    }
                }
            }
            rows = rows.Where(w => w.Count > 0).ToList();
            MiniExcel.SaveAs(filePath, rows, sheetName: sheetName, excelType: ExcelType.XLSX);
        }
        private static object ConvertFieldValue(MatrixField field)
        {
            if (field.Value == null)
                return string.Empty;

            switch (field.MatrixDataType)
            {
                case MatrixDataType.Integer:
                    if (int.TryParse(field.Value.ToString(), out int intValue))
                        return intValue;
                    break;
                case MatrixDataType.Decimal:
                    if (decimal.TryParse(field.Value.ToString(), out decimal decValue))
                        return decValue;
                    break;
                case MatrixDataType.Date:
                    if (DateTime.TryParse(field.Value.ToString(), out DateTime dateValue))
                        return dateValue.ToString("yyyy-MM-dd"); // Format date
                    break;
                case MatrixDataType.Boolean:
                    if (bool.TryParse(field.Value.ToString(), out bool boolValue))
                        return boolValue ? 1 : 0;
                    break;
                case MatrixDataType.Time:
                    // Handle time as a formatted string
                    return field.Value.ToString();
                case MatrixDataType.String:
                    return field.Value.ToString();
                default:
                    return field.Value.ToString();
            }

            return field.Value.ToString();
        }

        public static MatrixResult MergeMatrixResults(this List<MatrixResult> results)
        {
            if (results.IsNullOrEmpty())
                return null;

            var previous = results.First();

            foreach (var result in results.Where(w => !w.MatrixFields.IsNullOrEmpty()))
            {
                var maxRow = previous.MatrixFields.Max(m => m.RowNumber);

                foreach (var field in result.MatrixFields)
                {
                    if (result != previous)
                        field.RowNumber += maxRow;
                    field.Key = result.Key;
                    result.Key = field.Key;
                }
                result.MatrixDefinition.Key = result.Key;
                results.First().MatrixDefinitions.Add(result.MatrixDefinition);
                previous = result;
            }

            results.First().MatrixFields = results
                .OrderBy(r =>
                    r.MatrixFields?.Any() == true
                        ? r.MatrixFields.Min(f => f.RowNumber)
                        : int.MaxValue)
                .SelectMany(r =>
                    r.MatrixFields?.Any() == true
                        ? r.MatrixFields.OrderBy(f => f.RowNumber)
                        : Enumerable.Empty<MatrixField>())
                .ToList();

            return results.First();
        }


        public static List<MatrixRow> GetMatrixRows(this MatrixResult matrixResult)
        {
            List<MatrixRow> rows = new List<MatrixRow>();

            foreach (var item in matrixResult.MatrixFields.GroupBy(g => g.RowNumber))
            {
                rows.Add(new MatrixRow()
                {
                    RowNumber = item.Key,
                    MatrixFields = item.ToList(),
                    Key = item.First().Key,
                });
            }

            return rows.OrderBy(o => o.RowNumber).ToList();
        }

        private static MatrixRow AggregateRows(List<MatrixRow> rows, int rowNumber, Guid columnKey, string rowValue)
        {
            MatrixRow row = new MatrixRow();
            var matchingRows = rows.Where(w => w.MatrixFields.Any(v => v.ColumnKey == columnKey && (string)v.Value == rowValue));

            foreach (var matchingRow in matchingRows)
            {
                foreach (var field in matchingRow.MatrixFields.GroupBy(g => g.ColumnKey))
                {
                    var first = field.First();
                    MatrixField newField = new MatrixField(rowNumber, field.Key, first.Value, first.MatrixDataType)
                    {
                        MatrixFieldOptions = first.MatrixFieldOptions,
                    };

                    switch (first.MatrixDataType)
                    {
                        case MatrixDataType.String:
                            newField.Value = string.Join(",", field.Select(s => s.Value).Distinct());
                            break;
                        case MatrixDataType.Integer:
                            newField.Value = Convert.ToInt32(field.Where(w => w.Value != null)?.Sum(s => Convert.ToDecimal(s.Value)) ?? 0);
                            break;
                        case MatrixDataType.Boolean:
                            newField.Value = first.Value;
                            break;
                        case MatrixDataType.Date:
                        case MatrixDataType.DateAndTime:
                            newField.Value = !field.Where(w => w.Value != null).IsNullOrEmpty() ? field.Where(w => w.Value != null).Select(s => (DateTime?)s.Value).Distinct().Count() : 0;
                            newField.MatrixDataType = MatrixDataType.Integer;
                            newField.MatrixFieldOptions = null;
                            break;
                        case MatrixDataType.Decimal:
                            newField.Value = field.Where(w => w.Value != null)?.Sum(s => Convert.ToDecimal(s.Value)) ?? 0;
                            break;
                        case MatrixDataType.Time:
                            newField.Value = CalendarUtility.FormatMinutes((int)field.Sum(s => CalendarUtility.GetMinutesFromString(s.Value)));
                            break;
                        default:
                            break;
                    }

                    row.MatrixFields.Add(newField);
                }
            }

            return row;
        }

        public static void AddAggregatedTab(ExcelPackage package, string sheetName, MatrixResult matrixResult)
        {
            var allRows = GetMatrixRows(matrixResult);
            int nr = 1;
            int aggNumber = 1;
            var columns = new List<MatrixDefinitionColumn>();
            columns.AddRange(matrixResult.MatrixDefinition.MatrixDefinitionColumns);
            foreach (var def in matrixResult.MatrixDefinition.MatrixDefinitionColumns.Where(w => w.Options != null && w.Options.Aggregate))
            {
                if (aggNumber > 1)
                    break;

                var matchingFields = GetMatrixRows(matrixResult).SelectMany(s => s.MatrixFields.Where(w => w.ColumnKey == def.Key));
                List<MatrixRow> rows = new List<MatrixRow>();
                foreach (var group in matchingFields.GroupBy(g => (string)g.Value))
                {
                    rows.Add(AggregateRows(allRows, nr, group.First().ColumnKey, group.Key));
                    nr++;
                }

                columns = columns.Where(w => w != def).ToList();
                columns.Insert(0, def);

                AddSheet(package, sheetName, rows.SelectMany(s => s.MatrixFields).ToList(), columns);

                nr = 1;
                aggNumber++;
            }


        }
    }

    public class ExcelExportSettings
    {
        public char Delimiter { get; set; }
        public string Extension { get; set; }
    }
}


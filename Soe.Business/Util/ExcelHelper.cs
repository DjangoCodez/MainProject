using ExcelDataReader;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Business.Util
{
    public class ExcelHelper
    {
        // https://www.nuget.org/packages/EPPlus/4.5.3.3
        // https://github.com/JanKallman/EPPlus

        private readonly ExcelPackage package;
        private readonly ExcelWorksheet worksheet;

        #region Export

        public ExcelHelper(string worksheetName, List<string> headerNames = null)
        {
            package = new ExcelPackage();
            worksheet = package.Workbook.Worksheets.Add(worksheetName);

            if (headerNames != null)
            {
                int columnNr = 0;
                foreach (string headerName in headerNames)
                {
                    SetHeaderLabel(++columnNr, headerName);
                }
            }
        }

        #region Package

        public byte[] GetData()
        {
            return package.GetAsByteArray();
        }

        #endregion

        #region Row

        public void FormatRowAsHeader(int nbrOfColumns)
        {
            FormatRangeAsHeader(worksheet.Cells[1, 1, 1, nbrOfColumns]);
        }

        public void AddDataRow(int rowNr, List<object> data)
        {
            int columnNr = 0;
            foreach (object value in data)
            {
                SetCellValue(rowNr, ++columnNr, value);
            }
        }

        #endregion

        #region Column

        public void AutoFitColumns()
        {
            worksheet.Cells.AutoFitColumns(0);
        }

        public void FormatColumnsAsEditable(List<string> columnLetters, bool skipFirstRow)
        {
            foreach (string columnLetter in columnLetters)
            {
                FormatColumnAsEditable(columnLetter, skipFirstRow);
            }
        }

        public void FormatColumnAsEditable(string columnLetter, bool skipFirstRow)
        {
            FormatRangeAsEditable(GetRangeFromColumnLetter(columnLetter));

            if (skipFirstRow)
                FormatRangeAsHeader(GetHeaderRangeFromColumnLetter(columnLetter));
        }

        public void FormatColumnsAsNumber(List<string> columnLetters, bool skipFirstRow)
        {
            foreach (string columnLetter in columnLetters)
            {
                FormatColumnAsNumber(columnLetter, skipFirstRow);
            }
        }

        public void FormatColumnAsNumber(string columnLetter, bool skipFirstRow)
        {
            FormatRangeAsNumber(GetRangeFromColumnLetter(columnLetter));

            if (skipFirstRow)
                FormatRangeAsHeader(GetHeaderRangeFromColumnLetter(columnLetter));
        }

        public void FormatColumnsAsDate(List<string> columnLetters, bool skipFirstRow)
        {
            foreach (string columnLetter in columnLetters)
            {
                FormatColumnAsDate(columnLetter, skipFirstRow);
            }
        }

        public void FormatColumnAsDate(string columnLetter, bool skipFirstRow)
        {
            FormatRangeAsDate(GetRangeFromColumnLetter(columnLetter));

            if (skipFirstRow)
                FormatRangeAsHeader(GetHeaderRangeFromColumnLetter(columnLetter));
        }

        public void FormatColumnsHorizontalAlignment(List<string> columnLetters, ExcelHorizontalAlignment alignment, bool skipFirstRow)
        {
            foreach (string columnLetter in columnLetters)
            {
                FormatColumnHorizontalAlignment(columnLetter, alignment, skipFirstRow);
            }
        }

        public void FormatColumnHorizontalAlignment(string columnLetter, ExcelHorizontalAlignment alignment, bool skipFirstRow)
        {
            FormatRangeHorizontalAlignment(GetRangeFromColumnLetter(columnLetter), alignment);

            if (skipFirstRow)
                FormatRangeHorizontalAlignment(GetHeaderRangeFromColumnLetter(columnLetter), ExcelHorizontalAlignment.Left);
        }

        #endregion

        #region Range

        public void FormatRangeAsHeader(ExcelRange range)
        {
            range.Style.Numberformat.Format = "";
            range.Style.Font.Italic = false;
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            range.Style.ShrinkToFit = false;
        }

        public void FormatRangeAsEditable(ExcelRange range)
        {
            range.Style.Font.Italic = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
        }

        public void FormatRangeAsNumber(ExcelRange range, int decimals = 2)
        {
            string format = "0";
            if (decimals > 0)
                format += ".".PadRight(decimals + 1, '0');

            range.Style.Numberformat.Format = format;
        }

        public void FormatRangeAsDate(ExcelRange range)
        {
            range.Style.Numberformat.Format = "mm-dd-yy";
        }

        public void FormatRangeHorizontalAlignment(ExcelRange range, ExcelHorizontalAlignment alignment)
        {
            range.Style.HorizontalAlignment = alignment;
        }

        public void MergeRange(ExcelRange range)
        {
            range.Merge = true;
        }

        #endregion

        #region Cell

        public void SetCellValue(int rowNr, int columnNr, object value)
        {
            worksheet.Cells[rowNr, columnNr].Value = value;
        }

        public void SetHeaderLabel(int columnNr, string label)
        {
            SetCellValue(1, columnNr, label);
        }

        public void MergeCells(int fromRow, int fromCol, int toRow, int toCol)
        {
            MergeRange(GetRange(fromRow, fromCol, toRow, toCol));
        }

        #endregion

        #region Validation

        public void AddListValidationValues(string columnLetter, List<string> values, string errorTitle, string errorMessage)
        {
            // add a validation and set values
            var validation = worksheet.DataValidations.AddListValidation(String.Format("{0}2:{0}1048576", columnLetter));
            validation.ShowErrorMessage = true;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            validation.ErrorTitle = errorTitle;
            validation.Error = errorMessage;
            foreach (string value in values)
            {
                validation.Formula.Values.Add(value);
            }
        }

        #endregion

        #region Help-methods

        private ExcelRange GetRange(int fromRow, int fromCol, int toRow, int toCol)
        {
            return worksheet.Cells[fromRow, fromCol, toRow, toCol];
        }

        private ExcelRange GetRangeFromColumnLetter(string columnLetter)
        {
            return worksheet.Cells[String.Format("{0}:{0}", columnLetter)];
        }

        private ExcelRange GetHeaderRangeFromColumnLetter(string columnLetter)
        {
            return worksheet.Cells[String.Format("{0}1", columnLetter)];
        }

        #endregion

        #endregion

        #region Import

        public ExcelHelper()
        {
        }

        public DataSet GetDataSet(byte[] content, bool useHeaderRow)
        {
            try
            {
                MemoryStream stream = new MemoryStream(content);
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                DataSet ds = excelReader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() { UseHeaderRow = useHeaderRow } });
                return ds;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }

        public string GetStringValue(DataRow row, string heading, bool toLower)
        {
            string value = row[heading].ToString().Trim();
            return toLower ? value.ToLowerInvariant() : value;
        }

        public decimal? GetDecimalValue(DataRow row, string heading, bool allowNull)
        {
            string decimalStr = row[heading].ToString();
            decimal value = 0;
            if (decimal.TryParse(decimalStr, out value))
                return value;

            return allowNull ? (decimal?)null : 0;
        }

        public DateTime? GetDateValue(DataRow row, string heading)
        {
            string dateStr = row[heading].ToString();
            DateTime date;
            if (DateTime.TryParse(dateStr, out date))
                return date;

            return null;
        }

        #endregion
    }

    public static class ExcelHelperExtensions
    {
        #region Export

        public static byte[] ConvertToDelimitedFile(this ExcelPackage package, char delimiter = ';', int sheet = 1, int startOnRow = 1)
        {
            var worksheet = package.Workbook.Worksheets[sheet];

            if (startOnRow > 1 && worksheet.Dimension.End.Row >= startOnRow)
            {
                int current = 1;

                while (current < startOnRow)
                {
                    worksheet.DeleteRow(current);
                    current++;                        
                }
            }            

            var maxColumnNumber = worksheet.Dimension.End.Column;
            var currentRow = new List<string>(maxColumnNumber);
            var totalRowCount = worksheet.Dimension.End.Row;
            var currentRowNum = 1;

            var memory = new MemoryStream();

            using (var writer = new StreamWriter(memory, Encoding.UTF8))
            {
                while (currentRowNum <= totalRowCount)
                {
                    BuildRow(worksheet, currentRow, currentRowNum, maxColumnNumber);
                    WriteRecordToFile(currentRow, writer, currentRowNum, totalRowCount, delimiter);
                    currentRow.Clear();
                    currentRowNum++;
                }
            }

            return memory.ToArray();
        }

        private static void WriteRecordToFile(List<string> record, StreamWriter sw, int rowNumber, int totalRowCount, char delimiter = ';')
        {
            var commaDelimitedRecord = ToDelimitedString(record, delimiter.ToString());

            if (rowNumber == totalRowCount)
            {
                sw.Write(commaDelimitedRecord);
            }
            else
            {
                sw.WriteLine(commaDelimitedRecord);
            }
        }

        private static void BuildRow(ExcelWorksheet worksheet, List<string> currentRow, int currentRowNum, int maxColumnNumber, char delimiter = ';')
        {
            for (int i = 1; i <= maxColumnNumber; i++)
            {
                var cell = worksheet.Cells[currentRowNum, i];
                if (cell == null)
                {
                    // add a cell value for empty cells to keep data aligned.
                    AddCellValue(string.Empty, currentRow);
                }
                else
                {
                    AddCellValue(GetCellText(cell), currentRow);
                }
            }
        }

        private static string GetCellText(ExcelRangeBase cell)
        {
            return cell.Value == null ? string.Empty : cell.Value.ToString();
        }

        private static void AddCellValue(string s, List<string> record)
        {
            record.Add(string.Format("{0}{1}{0}", '"', s));
        }

        public static string ToDelimitedString(this List<string> list, string delimiter = ";", bool insertSpaces = false, string qualifier = "")
        {
            var result = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                string initialStr = list[i];
                result.Append((qualifier == string.Empty) ? initialStr : string.Format("{1}{0}{1}", initialStr, qualifier));
                if (i < list.Count - 1)
                {
                    result.Append(delimiter);
                    if (insertSpaces)
                    {
                        result.Append(' ');
                    }
                }
            }
            return result.ToString();
        }

        #endregion
    }
}

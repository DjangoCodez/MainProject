using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.IO;
using System;

namespace SoftOne.Soe.Business.Core
{
    public class ImportDynamicManager : ManagerBase
    {

        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ImportDynamicManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public ActionResult GetFileContent(int fileType, byte[] fileData, string fileName)
        {
            var result = new ActionResult();
            result.Value2 = fileName;
            //to do -> enum
            if (fileData == null || fileData.Length == 0)
            {
                return new ActionResult("Error reading file");
            }

            switch ((TermGroup_ImportDynamicFileType)fileType)
            {
                case TermGroup_ImportDynamicFileType.TextSemicolonSeparated:
                    result.Value = GetFileContent(fileData, ';');
                    break;
                case TermGroup_ImportDynamicFileType.TextTabSeparated:
                    result.Value = GetFileContent(fileData, '\t');
                    break;
                case TermGroup_ImportDynamicFileType.TextCommaSeparated:
                    result.Value = GetFileContent(fileData, ',');
                    break;
            }
            return result;
        }
        public List<string[]> GetFileContentExcel(byte[] fileData)
        {
            //to do...
            var lines = new List<string[]>();

            return lines;
        }
        public List<string[]> GetFileContent(byte[] fileData, char separator)
        {
            var lines = new List<string[]>();
            int cols = 0;
            using (StreamReader reader = new StreamReader(new MemoryStream(fileData), true)) 
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var item = line.Split(separator);
                    if (cols == 0)
                    {
                        cols = item.Length;
                        if (cols == 1)
                        {
                            return new List<string[]>();
                        }
                    }
                    if (item.Length != cols)
                    {
                        //file might not be consistent, uniform lengths are expected
                        item = SetLineLength(item, cols);
                    }
                    if (IsValid(item))
                        lines.Add(item);
                }
            }
            return lines;
        }
        private bool IsValid(string[] line)
        {
            int emptyRows = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (String.IsNullOrEmpty(line[i]))
                    emptyRows++;
            }
            return emptyRows < line.Length;
        }
        private string[] SetLineLength(string[] line, int length)
        {
            var newLine = new string[length];
            for (int i = 0; i < line.Length; i++)
            {
                newLine[i] = line[i];
            }
            return newLine;
        } 

        public List<Dictionary<string, object>> ParseRows(List<ImportFieldDTO> fields, ImportOptionsDTO options, string[][] data)
        {
            //var activeFields = new Dictionary<string, ImportFieldDTO>();
            var activeFields = new List<ImportFieldDTO>();
            var result = new List<Dictionary<string, object>>();
            if (data.Length == 0)
            {
                return null;
            }
            var reference = data[0];

            foreach (var item in fields)
            {
                if (item.Index != -1 && item.Index < reference.Length && item.Index >= 0)
                {
                    if (item.EnableValueMapping && item.ValueMapping != null)
                    {
                        foreach (var key in item.ValueMapping.Keys) {
                            if (item.ValueMapping[key] == null)
                            {
                                item.ValueMapping.Remove(key);
                            }
                        }
                    }
                    else
                    {
                        item.EnableValueMapping = false;
                    }
                    activeFields.Add(item);
                }
                else if (FieldHasDefaultValue(item))
                {
                    //fields with ONLY default values.
                    item.HasDefaultValue = true;
                    item.Index = -1;
                    activeFields.Add(item);
                }
            }
            for (int i = 0; i < data.Length; i++)
            {
                var row = new Dictionary<string, object>();
                if (options.SkipFirstRow && i == 0) continue;
                foreach (var item in activeFields)
                {
                    switch (item.DataType)
                    {
                        case SettingDataType.String:
                            row[item.Field] = GetStringValue(item, data[i]);
                            break;
                        case SettingDataType.Decimal:
                            row[item.Field] = GetDecimalValue(item, data[i]);
                            break;
                        case SettingDataType.Date:
                            row[item.Field] = GetDateValue(item, data[i]);
                            break;
                        case SettingDataType.Integer:
                            row[item.Field] = GetIntValue(item, data[i]);
                            break;
                    }
                }
                result.Add(row);
            }
            return result;
        }

        private string GetStringValue(ImportFieldDTO field, string[] row)
        {
            if (field.HasDefaultValue && field.Index == -1)
            {
                return field.DefaultStringValue;
            }
            string val = row[field.Index].Trim();
            if (field.EnableValueMapping)
            {
                val = GetStringFromMapping(field, val);
            }
            if (String.IsNullOrEmpty(val))
            {
                val = field.DefaultStringValue;
            }
            return val;
        }

        private decimal? GetDecimalValue(ImportFieldDTO field, string[] row)
        {
            if (field.HasDefaultValue && field.Index == -1)
            {
                return field.DefaultDecimalValue;
            }
            string val = row[field.Index].Trim();
            if (String.IsNullOrEmpty(val) && field.DefaultDecimalValue != null)
            {
                return field.DefaultDecimalValue;
            } 
            else
            {
                return NumberUtility.ToDecimal(val);
            }
        }

        private DateTime GetDateValue(ImportFieldDTO field, string[] row)
        {
            if (field.HasDefaultValue && field.Index == -1)
            {
                return field.DefaultDateTimeValue;
            }
            string val = row[field.Index].Trim();
            if (String.IsNullOrEmpty(val))
            {
                return field.DefaultDateTimeValue;
            }
            else
            {
                //TO-DO:
                //Should handle dynamic date formats 
                //This approach is naive, but good enough for now.
                if (val.Contains("T") && val.Contains("Z"))
                    return CalendarUtility.GetDateTime(val); //To handle UTC format
                else if (val.Contains("."))
                    return CalendarUtility.GetDateTime(val, "dd.MM.yyyy"); //FI
                else if (val.Contains("-"))
                    return CalendarUtility.GetDateTime(val, "yyyy-MM-dd"); //Most countries
                else
                    return CalendarUtility.GetDateTime(val, "yyyyMMdd"); //SOP...
            }
        }
        private int GetIntValue(ImportFieldDTO field, string[] row)
        {
            if (field.HasDefaultValue && field.Index == -1)
            {
                return field.DefaultIntValue.GetValueOrDefault();
            }
            string val = row[field.Index];
            if (String.IsNullOrEmpty(val) && field.DefaultIntValue != null)
            {
                return field.DefaultIntValue.Value;
            }
            else
            {
                return NumberUtility.ToInteger(val);
            }
        }
        private bool FieldHasDefaultValue(ImportFieldDTO field)
        {
            return field.DefaultBoolValue != null || 
                field.DefaultStringValue != null || 
                field.DefaultDecimalValue != null || 
                field.DefaultDateTimeValue != null || 
                field.DefaultIntValue != null ||
                field.DefaultGenericTypeValue != null;
        }
        private string GetStringFromMapping(ImportFieldDTO field, string val)
        {
            return field.ValueMapping.ContainsKey(val) && field.ValueMapping[val] != null ? field.ValueMapping[val].Name : val; 
        }

    }
}

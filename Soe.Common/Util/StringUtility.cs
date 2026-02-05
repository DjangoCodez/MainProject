using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Common.Util
{
    public static class StringUtility
    {
        private static List<string> GetBoolValues()
        {
            return GetFalseValues().Concat(GetTrueValues()).ToList();
        }

        private static List<string> GetTrueValues()
        {
            return new List<string>
            {
                "1",
                "true",
                "yes",
                "on",
                "ja",
                "a"
            };
        }

        private static List<string> GetFalseValues()
        {
            return new List<string>
            {
                "0",
                "false",
                "no",
                "off",
                "nej",
                "i"
            };
        }

        #region String to Bool

        public static bool ToBool(string value)
        {
            if (Boolean.TryParse(value, out bool b))
                return b;
            return false;
        }

        public static bool GetBool(int source)
        {
            return source == 1;
        }

        public static bool GetBool(object source)
        {
            if (source != null)
                return GetBool(source.ToString());
            return false;
        }

        public static bool GetBool(string source, bool valueIfEmpty = false)
        {
            if (string.IsNullOrEmpty(source))
                return valueIfEmpty;

            foreach (string trueValue in GetTrueValues())
            {
                if (string.Equals(source, trueValue, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool IsValidBool(string source)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            foreach (string boolValue in GetBoolValues())
            {
                if (string.Equals(source, boolValue, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool? GetNullableBool(object column)
        {
            if (!HasValue(column))
                return (bool?)null;

            return GetNullableBool(column.ToString());
        }

        public static bool? GetNullableBool(string source)
        {
            bool? value = null;
            if (!String.IsNullOrEmpty(source))
            {
                value = GetBool(source);
            }
            return value;
        }

        public static bool HasAnyValue(params object[] columns)
        {
            if (columns == null)
                return false;

            foreach (var column in columns)
            {
                if (HasValue(column))
                    return true;
            }

            return false;
        }

        public static bool HasValue(object source)
        {
            return source != null && !String.IsNullOrEmpty(source.ToString().Trim());
        }

        public static string GetValue(object source)
        {
            if (!HasValue(source))
                return String.Empty;
            return source.ToString().Trim();
        }

        public static string GetValue(int? source)
        {
            if (!HasValue(source))
                return String.Empty;
            return source.Value.ToString().Trim();
        }

        public static string GetValue(decimal? source)
        {
            if (!HasValue(source))
                return String.Empty;
            return source.Value.ToString().Trim();
        }

        public static string GetValue(bool? source)
        {
            if (!HasValue(source))
                return String.Empty;
            return source.Value.ToString().Trim();
        }

        public static bool IsGreater(string value, string compareValue)
        {
            int iValue;
            int iCompareValue;
            if (Int32.TryParse(value, out iValue) && Int32.TryParse(compareValue, out iCompareValue))
            {
                return iValue > iCompareValue;
            }
            else
            {
                return value.CompareTo(compareValue) > 0;
            }
        }

        public static bool IsInInterval(string value, string sourceFrom, string sourceTo)
        {
            //Use correct CompareTo method (int or string)
            int iValue;
            int iSourceFrom;
            int iSourceTo;
            if (Int32.TryParse(value, out iValue) &&
                Int32.TryParse(sourceFrom, out iSourceFrom) &&
                Int32.TryParse(sourceTo, out iSourceTo))
            {
                if (iValue.CompareTo(iSourceFrom) >= 0 &&
                    iValue.CompareTo(iSourceTo) <= 0)
                {
                    return true;
                }
            }
            else
            {
                if (value.CompareTo(sourceFrom) >= 0 &&
                    value.CompareTo(sourceTo) <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsInIntervalLong(string value, string sourceFrom, string sourceTo)
        {
            //Use correct CompareTo method (int or string)
            long iValue;
            long iSourceFrom;
            long iSourceTo;
            if (Int64.TryParse(value, out iValue) &&
                Int64.TryParse(sourceFrom, out iSourceFrom) &&
                Int64.TryParse(sourceTo, out iSourceTo))
            {
                if (iValue.CompareTo(iSourceFrom) >= 0 &&
                    iValue.CompareTo(iSourceTo) <= 0)
                {
                    return true;
                }
            }
            else
            {
                if (value.CompareTo(sourceFrom) >= 0 &&
                    value.CompareTo(sourceTo) <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEqual(string value1, string value2, bool ignoreCase = false)
        {
            if (String.IsNullOrEmpty(value1) && String.IsNullOrEmpty(value2))
                return true;

            if ((!String.IsNullOrEmpty(value1) && String.IsNullOrEmpty(value2)) ||
                (String.IsNullOrEmpty(value1) && !String.IsNullOrEmpty(value2)))
                return false;

            return ignoreCase ? value1.Equals(value2, StringComparison.CurrentCultureIgnoreCase) : value1.Equals(value2);
        }

        #endregion

        #region String to Dictionary

        public static string DictToString(Dictionary<int, string> dict)
        {
            string message = "";
            if (dict != null)
            {
                foreach (var pair in dict)
                {
                    if (!String.IsNullOrEmpty(message))
                        message += ", ";
                    message += pair.Value;
                }
            }
            return message;
        }

        public static string DictToString(Dictionary<int, string> dict, out int noOfItems, out int noOfEmptyItems)
        {
            noOfItems = 0;
            noOfEmptyItems = 0;

            string message = "";
            if (dict != null)
            {
                foreach (var pair in dict)
                {
                    if (String.IsNullOrEmpty(pair.Value))
                    {
                        noOfEmptyItems++;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(message))
                            message += ", ";
                        message += pair.Value;

                        noOfItems++;
                    }
                }
            }
            return message;
        }

        public static string DictToString(Dictionary<int, int> dict, out int noOfItems)
        {
            noOfItems = 0;

            string message = "";
            if (dict != null)
            {
                foreach (var pair in dict)
                {
                    if (!String.IsNullOrEmpty(message))
                        message += ", ";
                    message += pair.Value;

                    noOfItems++;
                }
            }
            return message;
        }

        #endregion

        #region String to Int

        public static bool IsNumeric(string source)
        {
            return int.TryParse(source, out _);
        }

        public static int GetInt(object column, int defaultValue = 0)
        {
            if (!HasValue(column))
                return defaultValue;
            return GetInt(column.ToString(), defaultValue);
        }

        public static int GetInt(string source, int defaultValue = 0)
        {
            if (!Int32.TryParse(source, out int value))
                value = defaultValue;
            return value;
        }

        public static SoeEntityState GetEntityState(object source, int defaultValue = 0)
        {
            if (!HasValue(source) || !Int32.TryParse(source.ToString(), out int value))
                value = defaultValue;
            return (SoeEntityState)value;
        }

        public static int? GetNullableInt(object column)
        {
            if (!HasValue(column))
                return (int?)null;
            return GetNullableInt(column.ToString());
        }

        public static int? GetNullableInt(string source)
        {
            if (!String.IsNullOrEmpty(source) && Int32.TryParse(source, out int sValue))
                return sValue;
            return null;
        }

        public static bool ContainsNumber(string source)
        {
            Regex r = new Regex(@"\d+");
            return r.IsMatch(source);
        }

        public static string RemoveNonNumeric(string source)
        {
            return Regex.Replace(source, "\\D", "");
        }

        public static int CountChars(string str, char c)
        {
            int count = 0;
            foreach (char ch in str)
            {
                if (c == ch)
                    count++;
            }
            return count;
        }

        #endregion

        #region String to Long

        public static long GetLong(string source, long defaultValue)
        {
            long value;
            if (Int64.TryParse(source, out value))
                return value;
            return defaultValue;
        }

        public static long? GetNullableLong(object column)
        {
            if (!HasValue(column))
                return (long?)null;

            return GetNullableLong(column.ToString());
        }

        public static long? GetNullableLong(string source)
        {
            long? value = null;
            if (!String.IsNullOrEmpty(source))
            {
                long sValue = 0;
                if (Int64.TryParse(source, out sValue))
                    value = sValue;
            }
            return value;
        }

        #endregion

        #region String to DateTime

        public static DateTime? GetNullableDateTime(string source)
        {
            if (!String.IsNullOrEmpty(source) && DateTime.TryParse(source, out DateTime dVale))
                return dVale;
            return null;
        }

        #endregion

        #region String to Decimal

        public static decimal GetDecimal(object column, decimal defaultValue = 0)
        {
            if (!HasValue(column))
                return defaultValue;

            return GetDecimal(column.ToString(), defaultValue);
        }

        public static decimal GetDecimal(string source, decimal defaultValue = 0)
        {
            decimal value;
            if (Decimal.TryParse(source, out value))
                return value;
            return defaultValue;
        }

        public static decimal? GetNullableDecimal(object column)
        {
            if (!HasValue(column))
                return (int?)null;

            return GetNullableDecimal(column.ToString());
        }

        public static decimal? GetNullableDecimal(string source)
        {
            decimal? value = null;
            if (!String.IsNullOrEmpty(source))
            {
                decimal sValue = 0;
                if (Decimal.TryParse(source, out sValue))
                    value = sValue;
            }
            return value;
        }

        public static decimal GetAmount(object column, decimal defaultValue = 0)
        {
            if (!HasValue(column))
                return defaultValue;

            return GetDecimal(column.ToString().Replace(".", ","), defaultValue);
        }

        #endregion

        #region String to List

        public static List<DateTime> SplitDateList(string source)
        {
            List<DateTime> result = new List<DateTime>();
            if (String.IsNullOrEmpty(source))
                return result;

            string[] parts = source.Split(',');
            foreach (string part in parts)
            {
                DateTime? dValue = null;

                if (!String.IsNullOrEmpty(part))
                    dValue = CalendarUtility.GetNullableDateTime(part);

                if (dValue.HasValue)
                    result.Add(dValue.Value);
            }

            return result;
        }

        public static List<int> SplitNumericBoolList(string source, bool? filterValue = null)
        {
            List<int> result = new List<int>();
            if (String.IsNullOrEmpty(source))
                return result;

            string[] parts = source.Split(',');
            foreach (string part in parts)
            {
                bool b = part.EndsWith("_1");
                if (filterValue.HasValue && filterValue.Value != b)
                    continue;

                string s = part.Substring(0, part.IndexOf("_"));

                int nr;
                if (Int32.TryParse(s, out nr) && nr > 0)
                    result.Add(nr);
            }

            return result;
        }

        public static List<int> SplitNumericList(string source, bool nullIfEmpty = false, bool skipZero = true)
        {
            List<int> result = new List<int>();

            if (!String.IsNullOrEmpty(source) && source != Constants.SOE_WEBAPI_STRING_EMPTY)
            {
                string[] parts = source.Split(',');
                foreach (string part in parts)
                {
                    string[] partsInterval = part.Split('-');
                    if (partsInterval.Count() > 1)
                    {
                        if (!Int32.TryParse(partsInterval[0], out int from))
                            continue;
                        if (!Int32.TryParse(partsInterval[1], out int to))
                            continue;
                        if (from > to)
                            continue;

                        for (int nr = from; nr <= to; nr++)
                        {
                            result.Add(nr);
                        }
                    }
                    else
                    {
                        if (Int32.TryParse(part, out int nr) && (nr > 0 || !skipZero))
                            result.Add(nr);
                    }
                }
            }

            if (result.Count == 0 && nullIfEmpty)
                return null;

            return result;
        }

        public static List<string> SplitStringList(string source)
        {
            List<string> result = new List<string>();
            if (String.IsNullOrEmpty(source))
                return result;

            string[] parts = source.Split(',');
            foreach (string part in parts)
            {
                result.Add(part);
            }

            return result;
        }

        public static string[] Split(char[] separator, string source)
        {
            if (String.IsNullOrEmpty(source))
                return new string[0];

            return source.Split(separator, StringSplitOptions.None);
        }

        #endregion

        #region String to Name

        public static void GetName(string name, out string firstName, out string lastName, NameStandard nameStandard)
        {
            firstName = "";
            lastName = "";

            if (!String.IsNullOrEmpty(name) && nameStandard != NameStandard.Unknown)
            {
                int delimiterIdx = 0;
                string part1 = "";
                string part2 = "";

                if (name.Contains(","))
                {
                    delimiterIdx = name.IndexOf(',');
                    part1 = name.Substring(0, delimiterIdx).Trim();
                    part2 = name.Substring(delimiterIdx + 1).Trim();
                }
                else if (name.Contains(" "))
                {
                    delimiterIdx = name.IndexOf(' ');
                    part1 = name.Substring(0, delimiterIdx).Trim();
                    part2 = name.Substring(delimiterIdx + 1).Trim();
                }
                else
                {
                    part1 = name;
                    part2 = "";
                }

                if (nameStandard == NameStandard.LastNameThenFirstName)
                {
                    firstName = part2;
                    lastName = part1;
                }
                else if (nameStandard == NameStandard.FirstNameThenLastName)
                {
                    firstName = part1;
                    lastName = part2;
                }
            }
        }

        public static string GetName(string firstName, string lastName, NameStandard nameStandard)
        {
            string name = "";

            if (nameStandard == NameStandard.FirstNameThenLastName)
                name = String.Format("{0} {1}", firstName, lastName);
            if (nameStandard == NameStandard.LastNameThenFirstName)
                name = String.Format("{0} {1}", lastName, firstName);

            return name;
        }

        #endregion

        #region String to String

        public static string TakeFirst(string text, string separator)
        {
            if (text == null)
                return "";

            int index = text.IndexOf(separator);
            if (index != -1)
            {
                return text.Substring(0, index);
            }
            else
            {
                return "";
            }
        }

        public static string RemoveNonTextCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            else
                return Regex.Replace(text, "[^0-9A-Za-z :/,-_()ÖöÄäÅå&]", "");
        }

        public static string NullToEmpty(object obj)
        {
            return obj != null ? obj.ToString() : string.Empty;
        }

        public static string EmptyToNull(object obj)
        {
            return obj != null && !string.IsNullOrEmpty(obj.ToString()) ? obj.ToString() : null;
        }

        public static string WildCardToRegEx(string wildCard)
        {
            int length = !string.IsNullOrEmpty(wildCard) ? wildCard.Length : 0;

            var s = new StringBuilder(length + 2);
            s.Append('^');

            if (!string.IsNullOrEmpty(wildCard)) //not a problem since length is 0 but added so sonarqube understands
            {
                for (int i = 0; i < length; i++)
                {
                    char c = Convert.ToChar(wildCard.Substring(i, 1));
                    switch (c)
                    {
                        case '*':
                            s.Append(".*");
                            break;
                        case '?':
                            s.Append(".");
                            break;
                        // Escape special regexp-characters
                        case '(':
                        case ')':
                        case '[':
                        case ']':
                        case '$':
                        case '^':
                        case '.':
                        case '{':
                        case '}':
                        case '|':
                        case '\\':
                            s.Append("\\");
                            s.Append(c);
                            break;
                        default:
                            s.Append(c);
                            break;
                    }
                }
            }
            s.Append('$');

            return s.ToString();
        }

        /// <summary>
        /// XML encodes a string and removes illegal characters.
        /// </summary>
        /// <param name="source">String to encode</param>
        /// <returns>Encoded string</returns>
        public static string XmlEncode(this string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            StringBuilder s = new StringBuilder();
            foreach (char c in source.ToCharArray())
            {
                if (c == '&') { s.Append("&amp;"); continue; }
                if (c == '<') { s.Append("&lt;"); continue; }
                if (c == '>') { s.Append("&gt;"); continue; }
                if (c == '"') { s.Append("&quot;"); continue; }
                if (c == '\'') { s.Append("&apos;"); continue; }
                //Commented out since blank is ok in XML file!
                //if (c == ' ') { s.Append("&nbsp;"); continue; }

                // All chars x00 - x1F except CR, LF, and TAB are illegal in XML and are therefore ignored.
                byte b = (byte)c;
                if (b <= 0x1F && b != 0x09 && b != 0x0A && b != 0x0D)
                {
                    continue;
                }

                s.Append(c);
            }
            return s.ToString();
        }

        public static string ConvertNewLineToHtml(string text)
        {
            text = text.Replace("\r\n", "<br/>");
            text = text.Replace("\n", "<br/>");

            return text;
        }

        public static string GetValidFilePath(string source)
        {
            if (!source.EndsWith(@"\"))
                return source + @"\";

            return source;
        }

        public static string SurroundString(string source, string enclosingString)
        {
            return enclosingString + source + enclosingString;
        }

        public static string SurroundString(string source, char enclosingChar)
        {
            return SurroundString(source, enclosingChar.ToString());
        }

        public static string GetStringValue(object source, int? maxChars = null)
        {
            if (!HasValue(source))
                return String.Empty;

            return maxChars.HasValue ? Left(source.ToString(), maxChars.Value) : source.ToString();
        }

        public static string Left(string source, int maxChars)
        {
            if (source == null)
                return "";
            source = source.Trim();
            if (source.Length < maxChars)
                return source;
            return source.Substring(0, maxChars);
        }

        public static string ReplaceValue(string text, string oldValue, string newValue)
        {
            if (!String.IsNullOrEmpty(text) && text.Contains(oldValue))
                text = text.Replace(oldValue, newValue);
            return text;
        }

        public static string ReplaceChar(string str, int position, string newChar)
        {
            if (string.IsNullOrEmpty(str) || str.Length < position - 1)
                return str;

            return str.Left(position) + newChar + str.Right(str.Length - position - 1);
        }

        public static string ReplaceNonAscii(string text, string newValue)
        {
            if (!string.IsNullOrEmpty(text))
                return Regex.Replace(text, @"[^\u0000-\u007F]+", newValue);
            return text;
        }

        public static string Merge(string source1, string source2)
        {
            string comment = "";

            comment += source1;

            if (source1 != source2)
            {
                //Delimeter
                if (!String.IsNullOrEmpty(comment))
                {
                    if (!comment.EndsWith(".") || !comment.EndsWith(","))
                        comment += ".";
                    if (!comment.EndsWith(" "))
                        comment += " ";
                }

                comment += source2;
            }

            return comment;
        }

        public static string Concat(string code, string name, bool codeFirst = true)
        {
            string description = "";

            if (codeFirst)
            {
                if (!String.IsNullOrEmpty(code))
                    description += code;

                if (!String.IsNullOrEmpty(name))
                {
                    if (!String.IsNullOrEmpty(description))
                        description += ". ";
                    description += name;
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(name))
                    description += name;

                if (!String.IsNullOrEmpty(code))
                {
                    if (String.IsNullOrEmpty(description))
                        description = code;
                    else
                        description += String.Format(" ({0})", code);

                }
            }

            return description;
        }

        public static string CamelCaseWord(string word)
        {
            return !String.IsNullOrEmpty(word) ? word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower() : String.Empty;
        }

        public static string ExcludeString(string value, string exclude)
        {
            int idx = value.IndexOf(exclude);
            return idx > 0 ? value.Substring(0, idx - 1) : value;
        }

        public static string ModifyValue(string originalValue, string newValue, bool doNotModifyWithEmpty)
        {
            if (String.IsNullOrEmpty(newValue) && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue;
        }

        public static int ModifyValue(int originalValue, int? newValue, bool doNotModifyWithEmpty)
        {
            if (!newValue.HasValue && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue.HasValue ? newValue.Value : 0;
        }

        public static int? ModifyValue(int? originalValue, int? newValue, bool doNotModifyWithEmpty)
        {
            if (!newValue.HasValue && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue;
        }

        public static long? ModifyValue(long? originalValue, long? newValue, bool doNotModifyWithEmpty)
        {
            if (!newValue.HasValue && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue;
        }

        public static decimal ModifyValue(decimal originalValue, decimal? newValue, bool doNotModifyWithEmpty)
        {
            if (!newValue.HasValue && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue.HasValue ? newValue.Value : Decimal.Zero;
        }

        public static decimal? ModifyValue(decimal? originalValue, decimal? newValue, bool doNotModifyWithEmpty)
        {
            if (!newValue.HasValue && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue;
        }

        public static bool ModifyValue(bool originalValue, bool? newValue, bool doNotModifyWithEmpty)
        {
            if (!newValue.HasValue && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue.HasValue && newValue.Value;
        }

        public static bool? ModifyValue(bool? originalValue, bool? newValue, bool doNotModifyWithEmpty)
        {
            if (!newValue.HasValue && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue;
        }

        public static DateTime ModifyValue(DateTime originalValue, DateTime? newValue, bool doNotModifyWithEmpty)
        {
            if (!newValue.HasValue && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue.HasValue ? newValue.Value : CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime? ModifyValue(DateTime? originalValue, DateTime? newValue, bool doNotModifyWithEmpty)
        {
            if (!newValue.HasValue && doNotModifyWithEmpty)
                return originalValue;
            else
                return newValue;
        }

        public static string LeftExtractLetters(string oldString, int letterCount, out string newString)
        {
            if (oldString == null)
            {
                newString = "";
                return "";
            }

            var left = oldString.Length > letterCount ? oldString.Substring(0, letterCount) : oldString;
            if (IsNumeric(left))
            {
                newString = oldString;
                return "";
            }
            else
            {
                newString = oldString.Substring(left.Length);
                return left;
            }
        }

        public static string RemoveConsecutiveCharacters(string input, char character)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Build a regex pattern to match consecutive occurrences of the character
            string pattern = $"{Regex.Escape(character.ToString())}{{2,}}";

            // Replace consecutive occurrences with a single instance of the character
            return Regex.Replace(input, pattern, character.ToString());
        }

        #endregion

        #region String to Tuple

        public static Tuple<int, int> DeserializeTupleString(string str)
        {
            int item1 = 0;
            int item2 = 0;

            string[] result = str.Split('-');
            if (result.Any())
                Int32.TryParse(result[0], out item1);
            if (result.Count() > 1)
                Int32.TryParse(result[1], out item2);

            return new Tuple<int, int>(item1, item2);
        }

        #endregion

        #region String Compare

        public static bool CompareWithoutDash(string string1, string string2)
        {
            var localString1 = string.IsNullOrEmpty(string1) ? "" : string1.Replace("-", "").ToUpper();
            var localString2 = string.IsNullOrEmpty(string2) ? "" : string2.Replace("-", "").ToUpper();
            return (localString1 == localString2);
        }

        #endregion

        #region Bool to String

        public static string GetString(bool value)
        {
            if (value)
                return "1";
            else
                return "0";
        }

        #endregion

        #region DateTime to String
        public static string GetSwedishFormattedTime(this TimeSpan time)
        {
            return time.ToString("t", CultureInfo.GetCultureInfo(Constants.SYSLANGUAGE_LANGCODE_SWEDISH));
        }
        public static string GetSwedishFormattedTime(this DateTime time)
        {
            return time.ToString("t", CultureInfo.GetCultureInfo(Constants.SYSLANGUAGE_LANGCODE_SWEDISH));
        }
        public static string GetSwedishFormattedDate(this DateTime date)
        {
            return date.ToString("d", CultureInfo.GetCultureInfo(Constants.SYSLANGUAGE_LANGCODE_SWEDISH));
        }
        public static string GetSwedishFormattedDateTime(this DateTime date)
        {
            return date.ToString("G", CultureInfo.GetCultureInfo(Constants.SYSLANGUAGE_LANGCODE_SWEDISH));
        }

        #endregion

        #region List<string> to string

        public static string GetCommaSeparatedString<T>(IEnumerable<T> values, bool distinct = true, bool addWhiteSpace = false)
        {
            string result = "";
            if (values.IsNullOrEmpty())
                return result;

            if (distinct)
                values = values.Distinct().ToList();

            int counter = 0;
            foreach (T value in values)
            {
                result += value;

                counter++;
                if (counter < values.Count())
                    result += addWhiteSpace ? ", " : ",";
            }

            return result;
        }

        public static string GetSeparatedString(IEnumerable<string> values, char delimiter, bool distinct = true, bool addWhiteSpace = false)
        {
            string result = "";
            if (values.IsNullOrEmpty())
                return result;

            if (distinct)
                values = values.Distinct().ToList();

            int counter = 0;
            foreach (var value in values)
            {
                result += value;

                counter++;
                if (counter < values.Count())
                    result += addWhiteSpace ? delimiter.ToString() + " " : delimiter.ToString();
            }

            return result;
        }

        public static string GetCommaSeparatedString(List<int> values, bool distinct = true, bool addWhiteSpace = false, bool useInterval = false, bool doNotSort = false)
        {
            string result = "";
            if (values == null || values.Count == 0)
                return result;

            if (distinct)
                values = values.Distinct().ToList();
            //if we dont want to sort list
            if (!doNotSort)
                values.Sort();

            var intervals = new List<Interval<int>>();
            var excludeIds = new List<int>();

            foreach (int value in values)
            {
                var interval = useInterval ? intervals.FirstOrDefault(i => i.To == (value - 1)) : null;
                if (interval == null)
                {
                    interval = new Interval<int>()
                    {
                        From = value,
                        To = value,
                    };
                    intervals.Add(interval);
                }
                else
                {
                    interval.To = value;
                }
            }

            int intervalCounter = 0;
            if (doNotSort)
            {
                foreach (var interval in intervals)
                {
                    if (interval.From != interval.To)
                        result += String.Format("{0}-{1}", interval.From, interval.To);
                    else
                        result += interval.From;

                    intervalCounter++;
                    if (intervalCounter < intervals.Count)
                        result += addWhiteSpace ? ", " : ",";
                }

            }
            else
            {
                foreach (var interval in intervals.OrderBy(i => i.From))
                {
                    if (interval.From != interval.To)
                        result += String.Format("{0}-{1}", interval.From, interval.To);
                    else
                        result += interval.From;

                    intervalCounter++;
                    if (intervalCounter < intervals.Count)
                        result += addWhiteSpace ? ", " : ",";
                }
            }
            return result;
        }

        public static string ToNewLineString(List<string> values, bool distinct = false)
        {
            if (values != null)
            {
                if (distinct)
                    return String.Join("\r\n", values.Distinct());
                else
                    return String.Join("\r\n", values);
            }

            return String.Empty;
        }

        public static string InsertLinebreaks(params string[] lines)
        {
            var linesWithText = from x in lines where !string.IsNullOrWhiteSpace(x) select x;
            if (!linesWithText.Any())
                return "";

            string retval = linesWithText.FirstOrDefault();
            foreach (string line in linesWithText.Skip(1))
            {
                retval += "\r\n" + line;
            }
            return retval;
        }

        public static bool TryGetIntParts(string value, char delimeter, out List<int> keys)
        {
            keys = new List<int>();
            if (String.IsNullOrEmpty(value))
                return false;

            string[] parts = value.Split(delimeter);
            foreach (string part in parts)
            {
                int partInt;
                if (!Int32.TryParse(part, out partInt))
                {
                    keys = new List<int>();
                    return false;
                }
                keys.Add(partInt);
            }
            return keys.Count > 0;
        }

        public static bool TryGetLastIntPart(string value, char delimeter, out int key)
        {
            key = 0;
            if (String.IsNullOrEmpty(value))
                return false;

            string[] parts = value.Split(delimeter);
            if (parts.Length > 0 && Int32.TryParse(parts.Last(), out key))
                return true;
            return false;
        }

        #endregion

        #region List<string> to List<string>

        public static IEnumerable<string> SortByLength(IEnumerable<string> source, bool ascending = true)
        {
            if (ascending)
                return from s in source orderby s.Length ascending select s;
            else
                return from s in source orderby s.Length descending select s;
        }

        #endregion

        #region Dictionary<string, string>

        public static string TryGetStringValue(Dictionary<string, string> dict, string key)
        {
            if (dict.ContainsKey(key))
                return dict[key];
            else
                return String.Empty;
        }

        public static int TryGetIntValue(Dictionary<string, string> dict, string key)
        {
            int iValue = 0;

            string sValue = TryGetStringValue(dict, key);
            if (!String.IsNullOrEmpty(sValue))
                Int32.TryParse(sValue, out iValue);

            return iValue;
        }

        public static bool TryGetBoolValue(Dictionary<string, string> dict, string key, bool valueIfEmpty = false)
        {
            bool bValue = false;

            string sValue = TryGetStringValue(dict, key);
            bValue = GetBool(sValue, valueIfEmpty);

            return bValue;
        }

        public static DateTime? TryGetDateValue(Dictionary<string, string> dict, string key)
        {
            DateTime? dValue = null;

            string sValue = TryGetStringValue(dict, key);
            if (!String.IsNullOrEmpty(sValue))
                dValue = CalendarUtility.GetNullableDateTime(sValue);

            return dValue;
        }

        #endregion

        #region Dictionary<int, string>

        public static string GetDictStringValue(Dictionary<int, string> dict, int key)
        {
            return dict != null && dict.ContainsKey(key) ? dict[key] : String.Empty;
        }

        #endregion

        #region Html

        public static string HTMLToText(string HTMLCode, bool keepLineBreaks = false)
        {
            if (HTMLCode == null)
                return string.Empty;

            // Remove new lines since they are not visible in HTML
            HTMLCode = HTMLCode.Replace("\n", " ");
            HTMLCode = HTMLCode.Replace("<html>", "");
            HTMLCode = HTMLCode.Replace("</html>", "");
            HTMLCode = HTMLCode.Replace("<body>", "");
            HTMLCode = HTMLCode.Replace("</body>", "");

            // Remove tab spaces
            HTMLCode = HTMLCode.Replace("\t", " ");

            // Remove multiple white spaces from HTML
            HTMLCode = Regex.Replace(HTMLCode, "\\s+", " ");

            // Remove HEAD tag
            HTMLCode = Regex.Replace(HTMLCode, "<head.*?</head>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any JavaScript
            HTMLCode = Regex.Replace(HTMLCode, "<script.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Replace special characters like &, <, >, " etc.
            StringBuilder sbHTML = new StringBuilder(HTMLCode);
            // Note: There are many more special characters, these are just
            // most common. You can add new characters in this arrays if needed
            string[] OldWords = { "&nbsp;", "&amp;", "&quot;", "&lt;", "&gt;", "&reg;", "&copy;", "&bull;", "&trade;", "&aring;", "&Aring;", "&auml;", "&Auml;", "&ouml;", "&Ouml;" };
            string[] NewWords = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢", "å", "Å", "ä", "Ä", "ö", "Ö" };
            for (int i = 0; i < OldWords.Length; i++)
            {
                sbHTML.Replace(OldWords[i], NewWords[i]);
            }

            // Check if there are line breaks (<br>) or paragraph (<p>)
            sbHTML.Replace("<br>", "\n<br>");
            sbHTML.Replace("<br ", "\n<br ");
            sbHTML.Replace("<p ", "\n<p ");

            if (keepLineBreaks)
            {
                sbHTML.Replace("</p>", "\n");
                sbHTML.Replace("</br>", "\n");
                sbHTML.Replace("<br/>", "\n");
            }

            // Finally, remove all HTML tags and return plain text
            return System.Text.RegularExpressions.Regex.Replace(sbHTML.ToString(), "<[^>]*>", "");
        }

        public static string CleanStringForJson(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove all control characters except \t (9), \n (10), \r (13)
            string cleaned = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);

            // Remove unpaired surrogates (invalid UTF-16 for JSON)
            // High surrogates: \uD800-\uDBFF, Low surrogates: \uDC00-\uDFFF
            // Remove any high surrogate not followed by a low surrogate, and any low surrogate not preceded by a high surrogate
            return Regex.Replace(cleaned, @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])", string.Empty);
        }

        #endregion

        #region WildCard

        public static string WildCardToString(WildCard wildCard)
        {
            switch (wildCard)
            {
                case WildCard.LessThan:
                    return "<";
                case WildCard.LessThanOrEquals:
                    return "<=";
                case WildCard.Equals:
                    return "=";
                case WildCard.GreaterThan:
                    return ">";
                case WildCard.GreaterThanOrEquals:
                    return ">=";
                case WildCard.NotEquals:
                    return "<>";
            }

            return String.Empty;
        }

        #endregion

        #region Validation

        public static bool ContainsBlank(string source)
        {
            if (String.IsNullOrEmpty(source))
                return false;

            return source.Contains(" ");
        }

        #endregion

        #region Ascii

        public static char GetAsciiCharacter(int code)
        {
            //Convert.ToChar(code).ToString();
            return ((char)code);
        }

        /// <summary>Tab</summary>
        public static char GetAsciiTab()
        {
            return GetAsciiCharacter(9);
        }

        /// <summary>New line/feed (LF)</summary>
        public static char GetAsciiNewLine()
        {
            return GetAsciiCharacter(10);
        }

        /// <summary>Carriage return</summary>
        public static char GetAsciiCarriageReturn()
        {
            return GetAsciiCharacter(13);
        }

        /// <summary>Space</summary>
        public static char GetAsciiSpace()
        {
            return GetAsciiCharacter(32);
        }

        /// <summary>Quote</summary>
        public static char GetAsciiQuote()
        {
            return GetAsciiCharacter(34);
        }

        /// <summary>Double quote</summary>
        public static string GetAsciiDoubleQoute()
        {
            return GetAsciiQuote().ToString() + GetAsciiQuote().ToString();
        }

        /// <summary>Backslash</summary>
        public static char GetAsciiBackslash()
        {
            return GetAsciiCharacter(92);
        }

        #endregion

        #region Misc

        public static string FillWithZerosBeginning(int targetSize, string originValue, bool truncate = false)
        {
            if (originValue == null)
                originValue = string.Empty;

            if (targetSize > originValue.Length)
            {
                StringBuilder sb = new StringBuilder();
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    sb.Append("0");
                }
                return (sb.ToString() + originValue);
            }
            else if (truncate)
                return originValue.Substring(0, targetSize);
            else
                return originValue;
        }

        public static string GetSubDomainFromUrl(string url)
        {
            string domain = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    Uri uri = new Uri(url);
                    var nodes = uri.Host.Split('.');
                    domain = nodes[0];
                }
            }
            catch
            {
                domain = string.Empty;
            }

            return domain;
        }

        public static string GetHostFromUrl(string url)
        {
            string host = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(url))
                    return new Uri(url).Host;                                  
            }
            catch
            {
                host = string.Empty;
            }

            return host;
        }

        public static string CleanString(string value)
        {
            value = string.IsNullOrEmpty(value) ? "" : value;

            return value.Trim();
        }

        public static string SortInvoiceNr(string source)
        {
            if (source.Contains("-"))
            {
                int idx = source.IndexOf('-');
                source = source.Substring(0, idx);
            }

            return source.PadLeft(10, '0');
        }

        public static string GetMessageOnNewLine(string message, bool before, bool after)
        {
            if (before)
                message = "\r\n" + message;
            if (after)
                message = message + "\r\n";

            return message;
        }

        public static string GetMessageWithDot(string message, bool before, bool after)
        {
            if (before)
                message = ". " + message;
            if (after)
                message = message + ". ";

            return message;
        }

        public static string SocialSecYYYYMMDD_Dash_XXXX(string socialSec)
        {
            if (string.IsNullOrEmpty(socialSec))
                return string.Empty;

            string value = socialSec;

            try
            {
                int validLength = 13;
                if (socialSec.Length == validLength && socialSec.Contains("-"))
                    return value;

                if (!socialSec.StartsWith("19") && !socialSec.StartsWith("20"))
                {
                    int year = -1;
                    int.TryParse(socialSec.Substring(0, 2), out year);
                    if (year != -1 && year > Convert.ToInt32(DateTime.Now.Year.ToString().Substring(2, 2)))
                        value = "19" + value;
                    else
                        value = "20" + value;
                }

                if (value.Length > 8 && !value.Contains("-"))
                    value = value.Insert(8, "-");
            }
            catch
            {
                value = socialSec;
            }

            return value;
        }

        public static string SocialSecYYYYMMDDXXXX(string socialSec)
        {
            return RemoveDash(SocialSecYYYYMMDD_Dash_XXXX(socialSec));
        }

        public static string SocialSecYYMMDDXXXX(string socialSec)
        {
            socialSec = RemoveDash(SocialSecYYYYMMDD_Dash_XXXX(socialSec));

            if (socialSec.StartsWith("19") || socialSec.StartsWith("20"))
                return socialSec.Substring(2, socialSec.Length - 2);
            else
                return socialSec;
        }

        public static string SocialSecYYMMDD_Dash_XXXX(string socialSec)
        {
            socialSec = SocialSecYYYYMMDD_Dash_XXXX(socialSec);

            if (socialSec.StartsWith("19") || socialSec.StartsWith("20"))
                return socialSec.Substring(2, socialSec.Length - 2);
            else
                return socialSec;
        }

        public static string SocialSecYYMMDD_Dash_Stars(string socialSec)
        {
            if (string.IsNullOrEmpty(socialSec))
                return string.Empty;

            string socialSecYYYYMMDD_Dash_XXXX = SocialSecYYYYMMDD_Dash_XXXX(socialSec);

            if (socialSecYYYYMMDD_Dash_XXXX.Length == 13 && !socialSecYYYYMMDD_Dash_XXXX.Contains("Y"))
                return socialSecYYYYMMDD_Dash_XXXX.Substring(0, 9) + Constants.SOCIALSEC_ANONYMIZE;
            else
                return string.Empty;
        }

        public static bool IsSamordningsnummer(string socialSec)
        {
            string formattedNumber = SocialSecYYYYMMDD_Dash_XXXX(socialSec);
            if (string.IsNullOrEmpty(formattedNumber) || formattedNumber.Length < 9)
                return false;

            int day = int.Parse(formattedNumber.Substring(6, 2));
            return day > 60;
        }

        public static string GetSalaryExportUseSocSecFormat(string employeeNr, string socialSec, TermGroup_SalaryExportUseSocSecFormat salaryExportUseSocSecFormat)
        {
            if (string.IsNullOrEmpty(socialSec))
                return employeeNr;

            switch (salaryExportUseSocSecFormat)
            {
                case TermGroup_SalaryExportUseSocSecFormat.KeepEmployeeNr:
                    return employeeNr;
                case TermGroup_SalaryExportUseSocSecFormat.YYYYMMDD_dash_XXXX:
                    return StringUtility.SocialSecYYYYMMDD_Dash_XXXX(socialSec);
                case TermGroup_SalaryExportUseSocSecFormat.YYYYMMDDXXXX:
                    return StringUtility.SocialSecYYYYMMDDXXXX(StringUtility.SocialSecYYYYMMDD_Dash_XXXX(socialSec));
                case TermGroup_SalaryExportUseSocSecFormat.YYMMDD_dash_XXXX:
                    return StringUtility.SocialSecYYMMDD_Dash_XXXX(StringUtility.SocialSecYYYYMMDD_Dash_XXXX(socialSec));
                case TermGroup_SalaryExportUseSocSecFormat.YYMMDDXXXX:
                    return StringUtility.SocialSecYYMMDDXXXX(StringUtility.SocialSecYYYYMMDD_Dash_XXXX(socialSec));
                default:
                    break;
            }

            return employeeNr;
        }

        public static string OrgNrWith16(string orgnr, bool removeDash = true)
        {
            if (removeDash)
                return $"16{RemoveDash(orgnr)}";
            else if (!orgnr.StartsWith("16"))
                return $"16{orgnr}";

            return orgnr;
        }

        public static string OrgNrWithout16(string orgnr)
        {
            if (orgnr.StartsWith("16"))
                orgnr = orgnr.Remove(0, 2);

            return RemoveDash(orgnr);
        }

        public static string RemoveDash(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("-", "").Trim();
        }

        public static string Orgnr16XXXXXX_Dash_XXXX(string orgnr)
        {
            if (string.IsNullOrEmpty(orgnr))
                return string.Empty;

            string value = orgnr;

            try
            {
                int validLenght = 13;
                if (orgnr.Length == validLenght && orgnr.Contains("-"))
                    return value;

                value = OrgNrWith16(orgnr, removeDash: false);

                if (!value.Contains("-"))
                    value = value.Insert(8, "-");
            }
            catch
            {
                value = orgnr;
            }

            return value;
        }

        public static string ToCamelCase(string original)
        {
            Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
            Regex whiteSpace = new Regex(@"(?<=\s)");
            Regex startsWithLowerCaseChar = new Regex("^[a-z]");
            Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
            Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
            Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

            // replace white spaces with undescore, then replace all invalid chars with empty string
            var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(original, "_"), string.Empty)
                // split by underscores
                .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                // set first letter to uppercase
                .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
                .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
                .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
                .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()))
                // finally set first letter to lowercase
                .Select(w => w.Substring(0, 1).ToLower() + w.Substring(1));

            return string.Concat(pascalCase);
        }

        public static string CleanPhoneNumber(string phoneNumber)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in phoneNumber)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == '-' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        #endregion

        #region Push Notification

        public static string GetPushNotificationId(int userId, Guid? guid)
        {
            if (guid.HasValue)
                return userId.ToString() + "-" + guid.Value.ToString();
            else
                return userId.ToString();
        }

        #endregion
    }
}

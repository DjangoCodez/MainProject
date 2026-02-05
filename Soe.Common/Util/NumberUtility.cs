using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Common.Util
{
    public static class NumberUtility
    {
        #region Integer

        public static int ToInteger(string source, int defaultValue = 0)
        {
            if (Int32.TryParse(source, out int value))
                return value;
            return defaultValue;
        }

        public static int? ToNullableInteger(string source)
        {
            if (string.IsNullOrEmpty(source))
                return null;
            return ToInteger(source, 0);
        }

        public static bool HasChanged(int? value1, int? value2)
        {
            return !Nullable.Equals(value1, value2);
        }

        public static bool HasAnyValue(params decimal?[] values)
        {
            return values.Any(value => value.HasValue);
        }

        public static int LestCommonMultiple(int[] numbers)
        {
            return numbers.Aggregate(lcm);
        }

        private static int lcm(int a, int b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }

        private static int GCD(int a, int b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }
        
        public static int AmountToInt(decimal amount, bool floor = false, int? maxInclusive = null)
        {
            //return Math.Abs(Convert.ToInt32(amount));

            if(floor)
                amount = decimal.Truncate(amount);

            int roundedAmount = Convert.ToInt32(amount);
            if (maxInclusive.HasValue)
            {
                //TODO: Trim if exceeds maxInclusive
            }
            return roundedAmount;
        }

        #endregion

        #region Decimal

        public static decimal MultiplyPercent(decimal amount, decimal percentage)
        {
            if (percentage == 0 || amount == 0)
                return 0;
            else
                return amount * (percentage / 100);
        }

        public static decimal DividePercentIfAboveOne(decimal percent)
        {
            return percent > 1 ? decimal.Divide(percent, 100) : percent;
        }

        public static decimal ToDecimalSeparatorIndifferent(string decimalStr)
        {
            if (string.IsNullOrEmpty(decimalStr))
                return decimal.Zero;

            var currentLocale = System.Globalization.CultureInfo.CurrentCulture;
            decimalStr = decimalStr.Replace(".", currentLocale.NumberFormat.NumberDecimalSeparator);
            decimalStr = decimalStr.Replace(",", currentLocale.NumberFormat.NumberDecimalSeparator);
            return decimal.Parse(decimalStr);
        }

        public static decimal ToDecimalWithComma(string source, int noOfDecimals = 2)
        {
            if (string.IsNullOrEmpty(source))
                return Decimal.Zero;

            return ToDecimal(source, noOfDecimals);
        }

        public static decimal? ToNullableDecimalWithComma(string source, int noOfDecimals = 2)
        {
            if (String.IsNullOrEmpty(source))
                return null;

            return ToDecimal(source, noOfDecimals);
        }

        public static decimal? ToNullableDecimal(object column, int noOfDecimals = 2)
        {
            if (!StringUtility.HasValue(column))
                return null;

            return ToDecimal(column.ToString(), noOfDecimals);
        }

        public static decimal ToDecimal(string source, int noOfDecimals = 2)
        {
            decimal value = Decimal.Zero;

            if (!string.IsNullOrEmpty(source))
            {
                source = source.ToString();
                source = source.ReplaceDecimalSeparator();

                decimal.TryParse(source, out value);
                if (noOfDecimals > 0)
                    value = Math.Round(value, noOfDecimals);
            }

            return value;
        }

        public static decimal GetDecimalRemoveDuplicateSubtractionSign(string source, int noOfDecimals = 2)
        {
            decimal value = Decimal.Zero;

            try
            {
                source = source.ToString();
                source = source.Replace(".", ",");
                source = source.Replace("--", "-");//some negative values in finvoice xml has multiple subractions signs. Dont know why?

                // TODO: Check localization
                value = String.IsNullOrEmpty(source) ? Decimal.Zero : Decimal.Parse(source, new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_DEFAULT));
                value = Math.Round(value, noOfDecimals);
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }

            return value;
        }

        public static decimal GetDecimalRemovePercentageSign(string source, int noOfDecimals = 2)
        {
            decimal value = Decimal.Zero;

            try
            {
                source = source.ToString();
                source = source.Replace("%", "");

                // TODO: Check localization
                source = source.Replace(".", ",");
                value = String.IsNullOrEmpty(source) ? Decimal.Zero : Decimal.Parse(source, new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_DEFAULT));
                value = Math.Round(value, noOfDecimals);
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }

            return value;
        }

        public static decimal? GetNullableDecimalFromString(string source, int noOfDecimals = 2)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            var value = GetDecimalFromString(source, noOfDecimals);

            if (value == 0 && source != "0" && source != "0,0" && source != "0.0")
                return null;

            return value;
        }

        public static decimal GetDecimalFromString(string source, int noOfDecimals = 2)
        {
            decimal value = Decimal.Zero;
            try
            {
                source = source.ToString();
                source = source.Replace(".", ",");
                value = String.IsNullOrEmpty(source) ? Decimal.Zero : Decimal.Parse(source, new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_DEFAULT));
                value = Math.Round(value, noOfDecimals);
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
            return value;
        }


        public static int GetDecimalCount(decimal n, int decimals = 0)
        {
            return n % 1 != 0 ? NumberUtility.GetDecimalCount(n * 10, decimals + 1) : decimals;
        }

        public static decimal GetFormattedDecimalValue(decimal source, int decimals)
        {
            // TODO: MidpointRounding
            //return Decimal.Round(source, decimals, MidpointRounding.AwayFromZero);
            return Math.Round(source, decimals);
        }

        public static decimal? GetFormattedDecimalValue(decimal? source, int decimals)
        {
            if (source.HasValue)
                return GetFormattedDecimalValue(source.Value, decimals);
            return source;
        }

        public static string GetFormattedDecimalStringValue(decimal source, int decimals, bool removeDecimalsIfInt = false)
        {
            if (removeDecimalsIfInt && source % 1 == 0)
                decimals = 0;

            return GetFormattedDecimalValue(source, decimals).ToString();
        }

        public static string GetFormattedDecimalStringValue(decimal? source, int decimals, bool removeDecimalsIfInt = false)
        {
            if (removeDecimalsIfInt && source % 1 == 0)
                decimals = 0;

            source = GetFormattedDecimalValue(source, decimals);
            if (source.HasValue)
                return source.ToString();
            return String.Empty;
        }

        public static string RoundToIntString(decimal value, double? percentage = null)
        {
            if (percentage.HasValue)
                value = (value * (decimal)percentage.Value);

            return ((int)Math.Abs(Math.Round(value, 0))).ToString();
        }

        public static bool IsValidNumber(string source, string format)
        {
            if (String.IsNullOrEmpty(format))
                return false;

            var parts = format.Split('.');
            if (parts.Count() != 2)
                return false;

            int nrOfNumbers;
            if (!Int32.TryParse(parts[0], out nrOfNumbers))
                return false;

            int nrOfDecimals;
            if (!Int32.TryParse(parts[1], out nrOfDecimals))
                return false;

            nrOfNumbers -= nrOfDecimals;

            return IsValidNumber(source, nrOfNumbers, nrOfDecimals);
        }

        public static bool IsValidNumber(string source, int nrOfNumbers, int nrOfDecimals)
        {
            source = source.Replace(".", ",");

            StringBuilder sb = new StringBuilder();
            sb.Append(@"^-?\d{1,");
            sb.Append(nrOfNumbers);
            sb.Append(@"}(\,\d{1,");
            sb.Append(nrOfDecimals);
            sb.Append(@"})?$");

            Regex regex = new Regex(sb.ToString());
            return regex.IsMatch(source);
        }

        public static bool IsEqual(List<int> values1, List<int> values2)
        {
            bool isValues1NullOrEmpty = values1 == null || values1.Count == 0;
            bool isValues2NullOrEmpty = values2 == null || values2.Count == 0;

            if (isValues1NullOrEmpty && isValues2NullOrEmpty)
                return true;
            if (isValues1NullOrEmpty != isValues2NullOrEmpty)
                return false;
            if (values1.Count != values2.Count)
                return false;
            foreach (int value in values1)
            {
                if (!values2.Contains(value))
                    return false;
            }
            return true;
        }

        public static bool IsEqual(int? value1, int? value2)
        {
            value1 = value1.ToNullable();
            value2 = value2.ToNullable();
            if (!value1.HasValue && !value2.HasValue)
                return true;
            if (value1.HasValue && !value2.HasValue)
                return false;
            if (!value1.HasValue && value2.HasValue)
                return false;
            return value1.Equals(value2);
        }

        public static bool IsEqual(decimal? value1, decimal? value2)
        {
            if (!value1.HasValue && !value2.HasValue)
                return true;
            if (value1.HasValue && !value2.HasValue)
                return false;
            if (!value1.HasValue && value2.HasValue)
                return false;
            return value1.Equals(value2);
        }

        public static decimal? TurnAmount(decimal? amount)
        {
            if (!amount.HasValue || amount == 0)
                return amount;
            return Decimal.Negate(amount.Value);
        }

		public static decimal Power(decimal x, int exp)
		{
            if (x == 0 && exp < 0) throw new ArgumentException($"Undefined result for {x} raised to {exp}.");
			if (exp == int.MinValue) throw new ArgumentOutOfRangeException(nameof(exp), $"Exponent too small {exp}.");
			if (x == 0 && exp > 0) return 0;
            if (exp == 0) return 1;

			if (exp < 0) return 1 / Power(x, -exp);

			decimal result = 1m;
			while (exp > 0)
			{
				if ((exp & 1) == 1)
					result *= x;

				x *= x;
				exp >>= 1;
			}
			return result;
		}

		#endregion

        #region Dictionary

        public static Dictionary<int, int> MergeDictictionary(Dictionary<int, int> dict, Dictionary<int, int> newDict)
        {
            if (newDict == null || newDict.Count == 0)
                return dict;

            if (dict == null)
                dict = new Dictionary<int, int>();

            foreach (var pair in newDict)
            {
                if (dict.ContainsKey(pair.Key))
                    continue;

                dict.Add(pair.Key, pair.Value);
            }

            return dict;
        }

        #endregion

        #region GenericType

        public static GenericType GetGenericType(ObservableCollection<GenericType> coll, int id, bool useFirstIfMissing = false)
        {
            GenericType type = null;
            if (!coll.IsNullOrEmpty())
            {
                type = coll.FirstOrDefault(c => c.Id == id);
                if (type == null && useFirstIfMissing)
                    type = coll.FirstOrDefault();
            }
            return type;
        }

        public static GenericType GetGenericType(ObservableCollection<GenericType> coll, int? id, bool useFirstIfMissing = false)
        {
            GenericType type = null;
            if (!coll.IsNullOrEmpty())
            {
                type = id.HasValue ? coll.FirstOrDefault(c => c.Id == id.Value) : null;
                if (type == null && useFirstIfMissing)
                    type = coll.FirstOrDefault();
            }
            return type;
        }

        #endregion

        #region Math

        public static decimal GetMedianValue(List<decimal> values)
        {
            if (!values.Any())
                return 0;

            var ys = values.OrderBy(x => x).ToList();
            double mid = (ys.Count - 1) / 2.0;
            return (ys[(int)(mid)] + ys[(int)(mid + 0.5)]) / 2;
        }

        #endregion

        #region reflection

        public static bool CheckIfAllIntAndDecimalPropertiesAreZeroOrNull(object obj)
        {
            Type objectType = obj.GetType();
            PropertyInfo[] properties = objectType.GetProperties();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(int) || property.PropertyType == typeof(decimal))
                {
                    var value = property.GetValue(obj);
                    if (value != null)
                    {
                        if (decimal.TryParse(value.ToString(), out decimal dec) && dec != 0)
                            return false;

                        if (int.TryParse(value.ToString(), out int i) && i != 0)
                            return false;
                    }
                }
            }

            return true;
        }

        #endregion

    }
}

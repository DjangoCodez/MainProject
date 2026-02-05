using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Core.PaymentIO
{
    public static class Utilities
    {
        #region Constants

        public const int BGC_LINE_MAX_LENGTH = 80;
        public const int PG_LINE_MAX_LENGTH = 100;
        public const int PG_PRODUCTION_NUMBER_MAX_VALUE = 9;
        public const string DATE_SEND_NOW = "GENAST";
        public const string DATE_EMPTY = "";
        public const string POST_PRODUCT_NAME_LB = "LEVERANTÖRSBETALNINGAR";
        public const string POST_PRODUCT_NAME_BGC = "BGMAX";
        public const string CURRENCY_EMPTY = "";
        public const string TRANSACTION_CODE_CORRECTION = "LB";
        public const string NETS_FORMAT_CODE = "NY";
        public const string NETS_SERVICE_CODE_TRANSMISSION = "00";
        public const string NETS_SERVICE_CODE_OTHER = "04";
        public const string NETS_TRANSMISSION_TYPE = "00";
        public const string NETS_ASSIGNMENT_TYPE = "00";
        //Support only transfer with KID
        public const string NETS_TRANSACTION_TYPE = "12";
        public const string NETS_DATA_RECIPIENT = "00008080";
        public const string PO3_OPENING_RECORD_TYPE = "MH00";
        public const string PO3_ALTERNATE_OPENING_RECORD_TYPE = "MH10";
        public const string PO3_PAYMENT_RECORD_TYPE = "PI00";
        public const string PO3_CLOSING_RECORD_TYPE = "MT00";
        public const string PO3_DEBIT_RECORD_TYPE = "CNDB";
        public const string PO3_CREDIT_RECORD_TYPE = "CNKR";
        public const string PO3_SENDERNOTES_RECORD_TYPE = "BA00";

        public const string DA1_OPENING_RECORD_TYPE = "MH01";
        public const string DA1_ALTERNATE_OPENING_RECORD_TYPE = "MH10";
        public const string DA1_PAYMENT_RECORD_TYPE = "DR00";
        public const string DA1_CLOSING_RECORD_TYPE = "MT01";
        public const string DA1_DEBIT_RECORD_TYPE = "CNDB";
        public const string DA1_CREDIT_RECORD_TYPE = "CNKR";
        public const string DA1_SENDERNOTES_RECORD_TYPE = "BA00";
        public const string DA1_RECEIVER_RECORD_TYPE = "BE01";


        #endregion

        #region Help methods

        internal static int GetNumeric(string item, int startPosition, int endPosition)
        {
            int returnValue;
            int.TryParse(item.SafeSubstring(startPosition, endPosition).Trim(), out returnValue);
            return returnValue;
        }

        internal static object GetPgPostCode(string line)
        {
            var firstChar = line.Substring(0, 1);
            var value = 0;
            if (Int32.TryParse(firstChar, out value))
            {
                return firstChar;
            }
            return string.Empty;
        }

        internal static string GetBgMaxTransactionCode(string line)
        {
            var firstTwoChars = line.Substring(0, 2);
            var value = 0;
            if (Int32.TryParse(firstTwoChars, out value))
            {
                return firstTwoChars;
            }
            return string.Empty;
        }

        internal static string GetLBTransactionCode(string line)
        {
            var value = string.Empty;
            var firstCharAsString = line.Substring(0, 1);
            var twoFirstCharsAsString = line.Substring(0, 2);
            if (twoFirstCharsAsString == "LB")
            {
                value = twoFirstCharsAsString;
            }
            else
            {
                if (IsNumeric(firstCharAsString))
                {
                    switch (Convert.ToInt32(firstCharAsString))
                    {
                        case 0:
                        case 3:
                        case 6:
                        case 7:
                        case 2:
                            if (IsNumeric(twoFirstCharsAsString))
                            {
                                switch (Convert.ToInt32(twoFirstCharsAsString.Substring(1, 1)))
                                {
                                    case 0:
                                    case 1:
                                    case 6:
                                    case 7:
                                    case 9:
                                        value = twoFirstCharsAsString;
                                        break;
                                    default:
                                        value = firstCharAsString;
                                        break;
                                }
                            }
                            else
                            {
                                value = firstCharAsString;
                            }
                            break;
                        case 9:
                            value = firstCharAsString;
                            break;
                        case 1:
                            if (IsNumeric(twoFirstCharsAsString))
                            {
                                value = twoFirstCharsAsString;
                            }
                            break;

                        case 4:
                            if (IsNumeric(twoFirstCharsAsString))
                            {
                                switch (Convert.ToInt32(twoFirstCharsAsString.Substring(1, 1)))
                                {
                                    case 0:
                                        value = twoFirstCharsAsString;
                                        break;
                                    default:
                                        value = firstCharAsString;
                                        break;
                                }
                            }
                            else
                            {
                                value = firstCharAsString;
                            }
                            break;
                        case 5:
                            if (IsNumeric(twoFirstCharsAsString))
                            {
                                switch (Convert.ToInt32(twoFirstCharsAsString.Substring(1, 1)))
                                {
                                    case 4:
                                        value = twoFirstCharsAsString;
                                        break;
                                    default:
                                        value = firstCharAsString;
                                        break;
                                }
                            }
                            else
                            {
                                value = firstCharAsString;
                            }
                            break;
                    }
                }
            }
            return value;
        }

        internal static bool ValidateCurrencyCode(string cc)
        {
            cc = cc.ToUpper();
            if (cc == TermGroup_Currency.SEK.ToString() || cc == TermGroup_Currency.EUR.ToString() || cc == Utilities.CURRENCY_EMPTY)
                return true;
            return false;
        }

        internal static string RemoveLeadingZeros(string item)
        {
            return item.TrimStart("0".ToCharArray());
        }

        internal static bool IsNumeric(string value)
        {
            var regEx = new Regex(@"\b\d{1," + value.Length + "}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = regEx.Match(value).Success ? true : false;
            return match;
        }

        internal static string PaymentNumber(string item, int length, bool checkNum)
        {
            if (checkNum)
            {
                item = RemoveLeadingZeros(item);
                item += " ";
            }
            return AddLeadingZeroes(item, length);
        }

        internal static string PadLeft(string text, int length)
        {
            return AddPadding(text, length, false);
        }

        internal static string AddPadding(string item, int length)
        {
            return AddPadding(item, length, true);
        }

        internal static string AddPadding(string item, int length, bool trail)
        {
            if (item.Length > length)
            {
                item = item.Trim();
            }
            if (item.Length > length)
            {
                item = item.Substring(0, length);
            }
            while (item.Length < length)
            {
                if (trail)
                    item += " ";
                else
                    item = " " + item;
            }
            return item;
        }

        internal static string AddLeadingZeroes(string item, int length)
        {
            if (item.Length > length)
            {
                item = item.Trim();
            }
            if (item.Length > length)
            {
                item = item.Substring(0, length);
            }
            while (item.Length < length)
            {
                item = "0" + item;
            }
            return item;
        }

        internal static bool HasCheckNumber(string bgCodeField)
        {
            var lastPostition = bgCodeField.Substring(bgCodeField.Length - 1, 1);
            if (!string.IsNullOrEmpty(lastPostition))
                return true;
            return false;
        }

        /// <summary>
        /// Special formatting required by foreign payments where negative amounts are replaced on last char
        /// </summary>
        internal static string ConvertAmountField(long amount)
        {
            var tmp = amount.ToString();
            if (amount > 0)
                return tmp;
            var lastChar = Convert.ToInt32(tmp.Substring(tmp.Length - 1, 1));
            var newChar = string.Empty;

            switch (lastChar)
            {
                case 0:
                    newChar = "-";
                    break;
                case 1:
                    newChar = "J";
                    break;
                case 2:
                    newChar = "K";
                    break;
                case 3:
                    newChar = "L";
                    break;
                case 4:
                    newChar = "M";
                    break;
                case 5:
                    newChar = "N";
                    break;
                case 6:
                    newChar = "O";
                    break;
                case 7:
                    newChar = "P";
                    break;
                case 8:
                    newChar = "Q";
                    break;
                case 9:
                    newChar = "R";
                    break;

            }
            tmp = tmp.Substring(0, tmp.Length - 1) + newChar;
            return tmp;
        }

        internal static DateTime GetDate(string item)
        {
            if (item.Length == 6)
            {
                string year = DateTime.Now.Year.ToString().Substring(0, 2) + item.Substring(0, 2);
                return new DateTime(
                            Convert.ToInt32(year),
                            Convert.ToInt32(item.Substring(2, 2)),
                            Convert.ToInt32(item.Substring(4, 2))
                        );
            }
            else if (item.Length == 8)
                return new DateTime(
                                Convert.ToInt32(item.Substring(0, 4)),
                                Convert.ToInt32(item.Substring(4, 2)),
                                Convert.ToInt32(item.Substring(6, 2))
                            );
            else
                return new DateTime();
        }

        internal static DateTime GetDateTime(TermGroup_SysPaymentMethod type, string item)
        {
            var date = new DateTime();
            string year;

            switch ((int)type)
            {
                case (int)TermGroup_SysPaymentMethod.BGMax:
                    date = new DateTime(
                            Convert.ToInt32(item.Substring(0, 4)),
                            Convert.ToInt32(item.Substring(4, 2)),
                            Convert.ToInt32(item.Substring(6, 2)),
                            Convert.ToInt32(item.Substring(8, 2)),
                            Convert.ToInt32(item.Substring(10, 2)),
                            Convert.ToInt32(item.Substring(12, 2)),
                            DateTimeKind.Local
                        );
                    break;

                case (int)TermGroup_SysPaymentMethod.LB:
                    const int dayOffSet = 3;
                    if (item.ToUpper() == DATE_SEND_NOW)
                        return DateTime.Now.AddDays(dayOffSet);
                    year = DateTime.Now.Year.ToString().Substring(0, 2) + item.Substring(0, 2);
                    date = new DateTime(
                                Convert.ToInt32(year),
                                Convert.ToInt32(item.Substring(2, 2)),
                                Convert.ToInt32(item.Substring(4, 2))
                            );
                    break;
                case (int)TermGroup_SysPaymentMethod.PG:
                    year = DateTime.Now.Year.ToString().Substring(0, 2) + item.Substring(0, 2);
                    date = new DateTime(
                                Convert.ToInt32(year),
                                Convert.ToInt32(item.Substring(2, 2)),
                                Convert.ToInt32(item.Substring(4, 2))
                            );
                    break;
            }
            return date;
        }

        internal static decimal GetAmount(string item)
        {
            decimal value = Convert.ToDecimal(item);
            return value /= 100;
        }

        internal static decimal GetAmount(int item)
        {
            return GetAmount(item, false);
        }

        internal static decimal GetAmount(long value)
        {
            return Convert.ToDecimal(value) / 100;
        }

        internal static decimal GetAmount(int item, bool negate)
        {
            var value = Convert.ToDecimal(item);
            value /= 100;
            if (negate)
            {
                value *= -1;
            }
            return value;
        }

        internal static long GetAmount(decimal value)
        {
            return Convert.ToInt64(value * 100);
        }

        internal static int GetLayoutCode(int senderBgc)
        {
            int value = 0;
            switch ((senderBgc.ToString()).Length)
            {
                case 7:
                    value = 1;
                    break;
                case 8:
                    value = 2;
                    break;
            }
            return value;
        }

        internal static bool DateCompareConverter(string d, DateTime date)
        {
            if (string.IsNullOrEmpty(d.Trim()) || d.ToUpper() == DATE_SEND_NOW) { }
            else if (date.Year != Convert.ToInt32(d.Substring(0, 2)) && date.Month == Convert.ToInt32(d.Substring(2, 2)) && date.Day == Convert.ToInt32(d.Substring(4, 2)))
                return true;
            return false;
        }

        internal static string GetDate(DateTime d)
        {
            var returnValue = string.Empty;
            if (d.Year.ToString().Length == 4)
            {
                returnValue += d.Year.ToString().Substring(2, 2);
            }

            else if (d.Year.ToString().Length == 1)
                returnValue += "0" + d.Year;
            else
                returnValue += d.Year;

            if (d.Month.ToString().Length == 1)
                returnValue += "0" + d.Month;
            else
                returnValue += d.Month;

            if (d.Day.ToString().Length == 1)
                returnValue += "0" + d.Day;
            else
                returnValue += d.Day;

            return returnValue;
        }

        internal static TermGroup_SysPaymentType GetPaymentType(int sysPaymentTypeId)
        {
            switch (sysPaymentTypeId)
            {
                case 1:
                    return TermGroup_SysPaymentType.BG;
                case 2:
                    return TermGroup_SysPaymentType.PG;
                case 3:
                    return TermGroup_SysPaymentType.Bank;
                case 4:
                    return TermGroup_SysPaymentType.BIC;
            }
            return TermGroup_SysPaymentType.Unknown;
        }

        internal static string ShortDateString(DateTime Date)
        {
            return Date.ToString("yyyy").Substring(2, 2) + Date.ToString("MMdd");
        }

        internal static int GetSenderAccountNumber(CompEntities entities, Company company, TermGroup_SysPaymentMethod method)
        {
            int number = 0;

            PaymentMethod paymentMethod = GetPaymentMethod(entities, company, method);
            if (paymentMethod != null && paymentMethod.PaymentInformationRow != null)
                number = PaymentNumber(paymentMethod.PaymentInformationRow.PaymentNr);

            return number;
        }
        internal static string GetSenderAccountNumberNets(CompEntities entities, Company company, TermGroup_SysPaymentMethod method)
        {
            string senderAccount = String.Empty;
            PaymentMethod paymentMethod = GetPaymentMethod(entities, company, method);
            if (paymentMethod != null && paymentMethod.PaymentInformationRow != null)
                senderAccount = PaymentNumber(paymentMethod.PaymentInformationRow.PaymentNr,11,false);

            return senderAccount;
        }
        internal static int GetPgPaymentMethod(int value)
        {
            var returnValue = 0;
            switch (value)
            {
                case (int)TermGroup_SysPaymentType.PG:
                    returnValue = 3;
                    break;
                case (int)TermGroup_SysPaymentType.BG:
                    returnValue = 4;
                    break;
                default:
                    throw new ActionFailedException((int)ActionResultSave.PaymentIncorrectPaymentType);
                /*
        case (int)TermGroup_SysPaymentType.BG:
            returnValue = 4;
            break;
                 */
            }
            return returnValue;
        }

        internal static int GetCfpPaymentMethod(int value)
        {
            var returnValue = 99;
            switch (value)
            {
                case (int)TermGroup_SysPaymentType.PG:
                    returnValue = 0;
                    break;
                case (int)TermGroup_SysPaymentType.BG:
                    returnValue = 5;
                    break;
                case (int)TermGroup_SysPaymentType.Bank:
                    returnValue = 9;
                    break;
                default:
                    throw new ActionFailedException((int)ActionResultSave.PaymentIncorrectPaymentType);
            }
            return returnValue;
        }

        internal static int GetNetsPaymentMethod(int value)
        {
            var returnValue = 0;
            switch (value)
            {
                case (int)TermGroup_SysPaymentType.Nets:
                    returnValue = 7;
                    break;
                default:
                    throw new ActionFailedException((int)ActionResultSave.PaymentIncorrectPaymentType);
            }
            return returnValue;
        }

        internal static string GetRiksBankIdentificationCode(Company company)
        {
            if (!company.ActorReference.IsLoaded)
                company.ActorReference.Load();

            if (!company.Actor.SupplierReference.IsLoaded)
                company.Actor.SupplierReference.Load();

            return company.Actor.Supplier.RiksbanksCode == null ? string.Empty : company.Actor.Supplier.RiksbanksCode;
        }

        internal static string GetAddressPart(ContactAddress contactAddress, TermGroup_SysContactAddressRowType type)
        {
            var result = string.Empty;
            foreach (ContactAddressRow car in contactAddress.ContactAddressRow)
            {
                if (car.SysContactAddressRowTypeId == (int)type)
                    result = car.Text.ToUpper();
            }
            return result;
        }

        #region Deprecated
        /*
        internal static string GetCountryCode(Company company, ReceiverType type)
        {
            var sysCountryId = 0;
            if (type == ReceiverType.Customer)
                sysCountryId = company.Actor.Customer.SysCountryId != null ? Convert.ToInt32(company.Actor.Customer.SysCountryId) : -1;
            else if (type == ReceiverType.Supplier)
                sysCountryId = company.Actor.Supplier.SysCountryId != null ? Convert.ToInt32(company.Actor.Supplier.SysCountryId) : -1;

            if (sysCountryId == -1)
                return string.Empty;

            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            return ccm.GetSysCountry(sysCountryId).Code;
        }
        */
        #endregion

        internal static PaymentMethod GetPaymentMethod(CompEntities entities, Company company, TermGroup_SysPaymentMethod paymentMethod)
        {
            if (company == null)
                return null;
            int sysPaymentMethodId = (int)paymentMethod;

            var targetPaymentMethods = (from method in entities.PaymentMethod
                                            .Include("PaymentInformationRow")
                                        where (method.Company.ActorCompanyId == company.ActorCompanyId) &&
                                        (method.SysPaymentMethodId == sysPaymentMethodId) &&
                                        (method.PaymentInformationRow.State == (int)SoeEntityState.Active) &&
                                        (method.State == (int)SoeEntityState.Active)
                                        select method).ToList<PaymentMethod>();

            PaymentMethod targetPaymentMethod = null;
            if (targetPaymentMethods != null)
            {
                if (targetPaymentMethods.Count == 1)
                    targetPaymentMethod = targetPaymentMethods.FirstOrDefault();
                else if (targetPaymentMethods.Count > 1)
                    targetPaymentMethod = targetPaymentMethods.FirstOrDefault(i => i.PaymentInformationRow.Default);

                if (targetPaymentMethod == null)
                    targetPaymentMethod = targetPaymentMethods.FirstOrDefault();
            }

            return targetPaymentMethod;
        }

        internal static string PaymentNumberLong(string p)
        {
            p = p.Replace("-", "").Trim();
            p = p.Replace(" ", "").Trim();
            return p;
        }

        internal static int PaymentNumber(string p)
        {
            int value = 0;
            p = p.Replace("-", "").Trim();
            p = p.Replace(" ", "").Trim();
            Int32.TryParse(p, out value);
            return value;
        }

        /// <summary>
        /// Extracts all numeric sequences in the string that match the length
        /// I.e. "12345A3455" with length 4 would yield 1234,2345,3455
        /// </summary>
        /// <param name="item"></param>
        /// <param name="matchLength"></param>
        /// <returns></returns>
        internal static List<int> GetInvoiceNrSuggestions(string item, int matchLength)
        //internal static List<int> GetInvoiceNrSuggestions(string item,int actorCompanyId)
        {
            var result = new List<int>();
            var previousValues = new List<string>();

            for (int i = 0; i < item.Length; i++)
            {
                int nextInt = 0;
                var nextPost = item.Substring(i, 1);

                if (Int32.TryParse(nextPost, out nextInt))
                {
                    previousValues.Add(nextPost);
                    if (previousValues.Count == matchLength)
                    {
                        var value = string.Empty;
                        foreach (string numeric in previousValues)
                        {
                            value += numeric;
                        }
                        result.Add(Convert.ToInt32(value));
                        previousValues.Remove(previousValues[0]);
                    }
                }
                else
                {
                    previousValues.Clear();
                }
            }
            return result;
        }

        internal static string GetInvoiceNrFromOCR(string ocr)
        {
            //TODO: implement if possible
            return string.Empty;
        }

        #endregion

        #region Public methods

        public static string GetLBFilePathOnServer(string guid)
        {
            return GetFilePathOnServer(GetLBFileNameOnServer(guid));
        }

        public static string GetLBFileNameOnServer(string guid)
        {
            return GetFileNameOnServer(guid, Constants.SOE_SERVER_LB_PREFIX, Constants.SOE_SERVER_FILE_PAYMENT_SUFFIX);
        }

        public static string GetLBFileNameOnClient()
        {
            return GetFileNameOnClient(Constants.SOE_SERVER_FILENAME_PREFIX_OLD, Constants.SOE_SERVER_FILE_PAYMENT_SUFFIX);
        }

        public static string GetPGFilePathOnServer(string guid)
        {
            return GetFilePathOnServer(GetPGFileNameOnServer(guid));
        }

        public static string GetPGFileNameOnServer(string guid)
        {
            return GetFileNameOnServer(guid, Constants.SOE_SERVER_PG_PREFIX, Constants.SOE_SERVER_FILE_PAYMENT_SUFFIX);
        }

        public static string GetPGFileNameOnClient()
        {
            return GetFileNameOnClient(Constants.SOE_SERVER_PG_PREFIX, Constants.SOE_SERVER_FILE_PAYMENT_SUFFIX);
        }

        public static string GetSEPAFilePathOnServer(string guid)
        {
            return GetFilePathOnServer(GetSEPAFileNameOnServer(guid));
        }

        public static string GetSEPAFileNameOnServer(string guid)
        {
            return GetFileNameOnServer(guid, Constants.SOE_SERVER_SEPA_PREFIX, Constants.SOE_SERVER_FILE_XML_SUFFIX);
        }

        public static string GetISO20022FileNameOnServer(string guid)
        {
            return GetFileNameOnServer(guid, Constants.SOE_SERVER_ISO20022_PREFIX, Constants.SOE_SERVER_FILE_XML_SUFFIX);
        }

        public static string GetNetsFileNameOnServer(string guid)
        {
            return GetFileNameOnServer(guid, Constants.SOE_SERVER_NETS_PREFIX, Constants.SOE_SERVER_FILE_PAYMENT_SUFFIX);
        }

        public static string GetCfpFileNameOnServer(string guid)
        {
            return GetFileNameOnServer(guid, Constants.SOE_SERVER_CFP_PREFIX, Constants.SOE_SERVER_FILE_PAYMENT_SUFFIX);
        }

        public static string GetICAFileNameOnServer(string guid)
        {
            return GetFileNameOnServer(guid, Constants.SOE_SERVER_ICA_PREFIX, Constants.SOE_SERVER_FILE_DAT_SUFFIX);
        }

        public static string GetAutogiroFileNameOnServer(string guid)
        {
            return GetFileNameOnServer(guid, Constants.SOE_SERVER_AUTOGIRO_PREFIX, Constants.SOE_SERVER_FILE_TEXT_SUFFIX);
        }

        private static string GetFilePathOnServer(string fileName)
        {
            return ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + @"export/payment/" + fileName;
        }

        private static string GetFileNameOnServer(string guid, string paymentMethodPrefix, string fileSuffix)
        {
            return Constants.SOE_SERVER_FILENAME_PREFIX_OLD + paymentMethodPrefix + guid + fileSuffix;
        }

        private static string GetFileNameOnClient(string paymentMethodPrefix, string fileSuffix)
        {
            return Constants.SOE_SERVER_FILENAME_PREFIX_OLD + paymentMethodPrefix + DateTime.Now.ToString("yyyyMMddHHmmss") + fileSuffix;
        }

        #endregion
    }
}

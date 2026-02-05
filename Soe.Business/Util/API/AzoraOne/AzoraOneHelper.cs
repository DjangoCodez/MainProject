using Newtonsoft.Json;
using SoftOne.Soe.Business.Util.API.AzoraOne.Models;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.API.AzoraOne
{
    public static class AzoraOneHelper
    {
        // The service will append &companyId=? to the callback URL.
        private const string _callbackBaseUrl = "https://devbridge.softone.se/api/azoraone/callback";

        public static string GetCallbackUrl(int scanningEntryId, TermGroup_SysPageStatusSiteType siteType, int? compDbId)
        {
            var dataString = $"scanningEntryId={scanningEntryId}&siteType={(int)siteType}&compDbId={compDbId}";
            var dataEncodedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dataString));
            return $"{_callbackBaseUrl}?params={dataEncodedBase64}";
        }

        public static string DateFormat => "yyyy-MM-dd";
        public static string DateTimeFormat => "yyyy-MM-dd HH:mm:ss";
        public static string TimeFormat => "HH:mm:ss";
        public static DateTime? StringToDate(string dateString) => CalendarUtility.GetNullableDateTime(dateString, DateFormat);
        public static DateTime? StringToDateTime(string dateString) => CalendarUtility.GetNullableDateTime(dateString, DateTimeFormat);
        public static string DateToString(DateTime? dateTime) => CalendarUtility.ToDateTime(dateTime, DateFormat);
        public static decimal? StringToDecimal(string decimalString) => NumberUtility.ToNullableDecimalWithComma(decimalString, 2);
        public static string DecimalToString(decimal value) {
            var asString = NumberUtility.GetFormattedDecimalStringValue(value, 2);
            return asString.Replace(".", ",");
        } 
        public static string ParseString(string value) => string.IsNullOrWhiteSpace(value) ? null : value;
        public static string ResponseToString<T>(AOResponse<T> response)
        {
            return JsonConvert.SerializeObject(response);
        }
        public static (AOResponse<AOSupplierInvoice>, string) InvoiceFromByteArray(byte[] byteArray) {
            var invoiceString = System.Text.Encoding.UTF8.GetString(byteArray);
            return (JsonConvert.DeserializeObject<AOResponse<AOSupplierInvoice>>(invoiceString), invoiceString);
        }

        public static string CleanPaymentNr(string paymentNr)
        {
            if (string.IsNullOrWhiteSpace(paymentNr))
                return string.Empty;

            // Double dash is caught as a SQL injection attempt by AzoraOne, and is not appropiate for BG/PG.
            // Should be validated in UI also.
            paymentNr = paymentNr
                .Replace("--", "-")
                .Replace(" ", "");
            return paymentNr;
        }

        public static string ParseOrgNr(string orgNr)
        {
            if (orgNr == null)
                return string.Empty;

            else if (!orgNr.Contains("-") && orgNr.Length == 10)
                return orgNr.Substring(0, 6) + "-" + orgNr.Substring(6, 4);
            else if (!orgNr.Contains("-"))
                return string.Empty;
            else
                return orgNr;
        }

        public static string ParseBicIban(string paymentNr)
        {
            if (paymentNr.Contains("/"))
                paymentNr = paymentNr.Split('/')[1];
            return paymentNr.Replace(" ", "");
        }
    }
}

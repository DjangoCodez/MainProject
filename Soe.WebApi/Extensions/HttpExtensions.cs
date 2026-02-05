using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Soe.WebApi.Extensions
{
    public static class HttpExtensions
    {
        public const string ACCEPT_GENERIC_TYPE = "SoeGenericType";
        public const string ACCEPT_DTO = "SoeDTO";
        public const string ACCEPT_GRID_DTO = "SoeGridDTO";
        public const string ACCEPT_SMALL_DTO = "SoeSmallDTO";

        public static string GetStringValueFromQS(this HttpRequestMessage message, string parameter)
        {
            return message.RequestUri.ParseQueryString()[parameter];
        }

        public static bool GetBoolValueFromQS(this HttpRequestMessage message, string parameter)
        {
            bool value = false;
            string valueString = message.RequestUri.ParseQueryString()[parameter];
            if (!String.IsNullOrEmpty(valueString))
                bool.TryParse(valueString, out value);

            return value;
        }

        public static bool? GetNullableBoolValueFromQS(this HttpRequestMessage message, string parameter)
        {
            bool value = false;
            string valueString = message.RequestUri.ParseQueryString()[parameter];
            if (valueString == "null" || valueString == "undefined")
                return null;

            if (!String.IsNullOrEmpty(valueString))
                bool.TryParse(valueString, out value);

            return value;
        }

        public static int GetIntValueFromQS(this HttpRequestMessage message, string parameter)
        {
            int value = 0;
            string valueString = message.RequestUri.ParseQueryString()[parameter];
            if (!String.IsNullOrEmpty(valueString))
                Int32.TryParse(valueString, out value);

            return value;
        }

        public static int? GetNullableIntValueFromQS(this HttpRequestMessage message, string parameter)
        {
            string valueString = message.RequestUri.ParseQueryString()[parameter];
            int value = 0;
            if (!String.IsNullOrEmpty(valueString) && Int32.TryParse(valueString, out value))
            {
                return value;
            }

            return null;
        }

        public static DateTime? GetDateValueFromQS(this HttpRequestMessage message, string parameter)
        {
            DateTime? value = null;
            string valueString = message.RequestUri.ParseQueryString()[parameter];
            value = Util.CalendarUtility.BuildDateTimeFromString(valueString, true);

            return value;
        }

        public static List<int> GetIntListValueFromQS(this HttpRequestMessage message, string parameter, bool nullIfEmpty = false)
        {
            string valueString = message.RequestUri.ParseQueryString()[parameter];
            if (!string.IsNullOrEmpty(valueString))
                return StringUtility.SplitNumericList(valueString, nullIfEmpty, false);
            else
                return nullIfEmpty ? null : new List<int>();
        }

        public static bool HasAcceptValue(this HttpRequestMessage message, string acceptType)
        {
            IEnumerable<string> acceptValues = message.Headers.GetValues("Accept");

            return acceptValues.Any(v => v.Equals(acceptType, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class HttpContentExtensions
    {
        public static async Task<HttpPostedData> ParseMultipartAsync(this HttpContent postedContent)
        {
            var provider = await postedContent.ReadAsMultipartAsync();

            var files = new Dictionary<string, HttpPostedFile>(StringComparer.InvariantCultureIgnoreCase);
            var fields = new Dictionary<string, HttpPostedField>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var content in provider.Contents)
            {
                var fieldName = content.Headers.ContentDisposition.Name.Trim('"');
                if (!string.IsNullOrEmpty(content.Headers.ContentDisposition.FileName))
                {
                    var file = await content.ReadAsByteArrayAsync();
                    var fileName = content.Headers.ContentDisposition.FileName.Trim('"');
                    files.Add(fieldName, new HttpPostedFile(fieldName, fileName, file));
                }
                else
                {
                    var data = await content.ReadAsStringAsync();
                    fields.Add(fieldName, new HttpPostedField(fieldName, data));
                }
            }

            return new HttpPostedData(fields, files);
        }
    }

    public class HttpPostedData
    {
        public HttpPostedData(IDictionary<string, HttpPostedField> fields, IDictionary<string, HttpPostedFile> files)
        {
            Fields = fields;
            Files = files;
        }

        public IDictionary<string, HttpPostedField> Fields { get; private set; }
        public IDictionary<string, HttpPostedFile> Files { get; private set; }
    }

    public class HttpPostedFile
    {
        public HttpPostedFile(string name, string filename, byte[] file)
        {
            File = file;
            Name = name;
            Filename = filename;
        }

        public string Name { get; private set; }
        public string Filename { get; private set; }
        public byte[] File { private set; get; }
    }
    public class HttpPostedField
    {
        public HttpPostedField(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
    }
}
using SoftOne.Soe.Business.Util.API.AvionData.Enum;
using SoftOne.Soe.Common.Util;
using System;
using System.Linq;

namespace SoftOne.Soe.Business.Util.API.AvionData.Models
{
    /// <summary>
    /// Filters for querying the YTJ /companies endpoint.
    /// </summary>
    public class CompanySearchFilter
    {
        /// <summary>Company name or part of it.</summary>
        public string Name { get; set; }

        /// <summary>Town or city where the company is located.</summary>
        public string Location { get; set; }

        /// <summary>Business ID (Y-tunnus).</summary>
        public string BusinessId { get; set; }

        /// <summary>Company form (e.g. OY, OYJ, KY, etc.).</summary>
        public CompanyFormType? CompanyForm { get; set; }

        /// <summary>Main business line (TOL code or keyword).</summary>
        public string MainBusinessLine { get; set; }

        /// <summary>Start of registration date range (yyyy-MM-dd).</summary>
        public string RegistrationDateStart { get; set; }

        /// <summary>End of registration date range (yyyy-MM-dd).</summary>
        public string RegistrationDateEnd { get; set; }

        /// <summary>Postal code (can be used to narrow location).</summary>
        public string PostCode { get; set; }

        /// <summary>Start of business ID assignment date range (yyyy-MM-dd).</summary>
        public string BusinessIdAssignmentDateStart { get; set; }

        /// <summary>End of business ID assignment date range (yyyy-MM-dd).</summary>
        public string BusinessIdAssignmentDateEnd { get; set; }

        /// <summary>Pagination: page number (1-based).</summary>
        public int? Page { get; set; }
    }

    public static class CompanySearchFilterExtensions
    {
        public static string ToQueryString(this CompanySearchFilter filter)
        {
            if (filter == null) return "";

            var properties = from prop in typeof(CompanySearchFilter).GetProperties()
                             let value = prop.GetValue(filter)
                             where value != null && !string.IsNullOrWhiteSpace(value.ToString())
                             select $"{Uri.EscapeDataString(ToCamelCase(prop.Name))}={Uri.EscapeDataString(value.ToString())}";

            return string.Join("&", properties);
        }

        private static string ToCamelCase(string word)
        { 
            return char.ToLowerInvariant(word[0]) + word.Substring(1);
        }
    }
}

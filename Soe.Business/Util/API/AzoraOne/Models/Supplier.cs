using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Models
{
    public class AOSuppliers
    {
        public List<AOSupplier> Suppliers { get; set; }
    }
    public class AOSupplier
    {
        public string SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string CorporateIdentityNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string PlusGiroNumber { get; set; }
        public string Iban { get; set; }
        public AOSupplierTags SupplierTags { get; set; }

        public bool Equals(AOSupplier other)
        {
            return SupplierID == other.SupplierID &&
                SupplierName == other.SupplierName &&
                CorporateIdentityNumber == other.CorporateIdentityNumber &&
                BankAccountNumber == other.BankAccountNumber &&
                PlusGiroNumber == other.PlusGiroNumber &&
                Iban == other.Iban &&
                (SupplierTags == null && other.SupplierTags == null || SupplierTags.Equals(other.SupplierTags));
        }

        public bool HasUniqueIdentifier()
        {
            return !string.IsNullOrWhiteSpace(CorporateIdentityNumber) ||
                !string.IsNullOrWhiteSpace(BankAccountNumber) ||
                !string.IsNullOrWhiteSpace(PlusGiroNumber) ||
                !string.IsNullOrWhiteSpace(Iban) ||
                (SupplierTags != null && SupplierTags.HasUniqueIdentifier());
        }
    }

    public class AOSupplierTags
    {
        public string SupplierTag1 { get; set; }
        public string SupplierTag2 { get; set; }
        public string SupplierTag3 { get; set; }
        public string SupplierTag4 { get; set; }

        public bool Equals(AOSupplierTags other)
        {
            return SupplierTag1 == other.SupplierTag1 &&
                SupplierTag2 == other.SupplierTag2 &&
                SupplierTag3 == other.SupplierTag3 &&
                SupplierTag4 == other.SupplierTag4;
        }

        public bool HasUniqueIdentifier()
        {
            return !string.IsNullOrWhiteSpace(SupplierTag1) ||
                !string.IsNullOrWhiteSpace(SupplierTag2) ||
                !string.IsNullOrWhiteSpace(SupplierTag3) ||
                !string.IsNullOrWhiteSpace(SupplierTag4);
        }
    }

    public static class AOSupplierExtensions
    {
        public static AOSupplier ToAOSupplier(this SupplierDistributionDTO supplier)
        {
            return new AOSupplier
            {
                SupplierID = supplier.SupplierId.ToString(),
                SupplierName = supplier.Name,
                CorporateIdentityNumber = AzoraOneHelper.ParseOrgNr(supplier.OrgNr),
                BankAccountNumber = GetBG(supplier.PaymentInformationRows),
                PlusGiroNumber = GetPG(supplier.PaymentInformationRows),
                Iban = GetIBAN(supplier.PaymentInformationRows),
            };
        }
        public static List<AOSupplier> ToAOSuppliers(this List<SupplierDistributionDTO> suppliers)
        {
            var list = new List<AOSupplier>();
            foreach (var supplier in suppliers)
                list.Add(supplier.ToAOSupplier());
            return list;
        }
        public static string GetIBAN(List<PaymentInformationDistributionRowDTO> paymentInformations)
        {
            var paymentNr = GetPaymentNr(paymentInformations, TermGroup_SysPaymentType.BIC);
            return AzoraOneHelper.ParseBicIban(paymentNr);
        }
        public static string GetBG(List<PaymentInformationDistributionRowDTO> paymentInformations)
        {
            return GetPaymentNr(paymentInformations, TermGroup_SysPaymentType.BG);
        }
        public static string GetPG(List<PaymentInformationDistributionRowDTO> paymentInformations)
        {
            return GetPaymentNr(paymentInformations, TermGroup_SysPaymentType.PG);
        }
        public static string GetPaymentNr(List<PaymentInformationDistributionRowDTO> paymentInformations, TermGroup_SysPaymentType type)
        {
            var row = paymentInformations.FirstOrDefault(r => r.SysPaymentTypeId == type);
            return AzoraOneHelper.CleanPaymentNr(row != null ? row.PaymentNr : string.Empty);
        }
    }
}

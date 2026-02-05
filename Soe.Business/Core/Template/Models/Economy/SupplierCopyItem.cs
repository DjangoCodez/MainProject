using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{

    public class SupplierCopyItem
    {
        public SupplierCopyItem() {
            templateContacts = new List<ContactDTO>();
            templateCategoryRecords = new List<CompanyCategoryRecordDTO>();
            templateContactPersons = new Dictionary<int, List<ContactPersonDTO>>();
            templatePaymentInformations = new List<PaymentInformationDTO>();
        }

        public List<SupplierDTO> templateSuppliers { get; set; }

        public List<CompCurrencyDTO> templateCurrencies { get; set; }
        public List<PaymentConditionDTO> templatePaymentConditions { get; set; }
        public List<VatCodeDTO> templateVatCodes { get; set; }
        public List<DeliveryConditionDTO> templateDeliveryConditions { get; set; }
        public List<DeliveryTypeDTO> templateDeliveryTypes { get; set; }
        public List<AttestGroupDTO> templateAttestWorkFlowGroups { get; set; }
        public List<CommodityCodeDTO> templateIntrastatCodes { get; set; }
        public List<AccountInternalDTO> templateAccountInternals { get; set; }
        public List<AccountDimDTO> accountDimsTemplate { get; set; }
        public List<ContactDTO> templateContacts { get; set; }
        public List<CompanyCategoryRecordDTO> templateCategoryRecords { get; set; }
        public Dictionary<int, List<ContactPersonDTO>> templateContactPersons { get; set; }
        public List<PaymentInformationDTO> templatePaymentInformations { get; set; }
    }
}



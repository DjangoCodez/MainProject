using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class CompanyInformationIOItem
    {

        #region Collections

        public List<CompanyInformationIODTO> companyInformationIOs = new List<CompanyInformationIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "CompanyInformationIO";

        public string XML_CompanyNumber_TAG = "CompanyNumber";
        public string XML_CompanyName_TAG = "CompanyName";
        public string XML_CompanyShortName_TAG = "CompanyShortName";
        public string XML_BaseCurrencyCode_TAG = "BaseCurrencyCode";
        public string XML_EntCurrencyCode_TAG = "EntCurrencyCode";
        public string XML_Country_TAG = "Country";
        public string XML_OrgNr_TAG = "OrgNr";
        public string XML_VatNr_TAG = "VATnr";
        public string XML_InvoiceReference_TAG = "InvoiceReference";

        public string XML_BgNumber_TAG = "BgNumber";
        public string XML_PgNumber_TAG = "PgNumber";
        public string XML_BankNumber_TAG = "BankNumber";
        public string XML_CfpNumber_TAG = "CfpNumber";
        public string XML_SepaNumber_TAG = "SepaNumber";
        public string XML_NetsDirectNumber_TAG = "NetsDirectNumber";
        public string XML_IbanNumber_TAG = "IbanNumber";
        public string XML_BicNumber_TAG = "BicNumber";
     
        public string XML_DistributionAddress_TAG = "DistributionAddress";
        public string XML_DistributionCoAddress_TAG = "DistributionCoAddress";
        public string XML_DistributionPostalCode_TAG = "DistributionPostalCode";
        public string XML_DistributionPostalAddress_TAG = "DistributionPostalAddress";
        public string XML_DistributionCountry_TAG = "DistributionCountry";
       
        public string XML_BillingAddress_TAG = "BillingAddress";
        public string XML_BillingCoAddress_TAG = "BillingCoAddress";
        public string XML_BillingPostalCode_TAG = "BillingPostalCode";
        public string XML_BillingPostalAddress_TAG = "BillingPostalAddress";
        public string XML_BillingCountry_TAG = "BillingCountry";

        public string XML_BoardHQAddress_TAG = "BoardHQAddress";
        public string XML_BoardHQCountry_TAG = "BoardHQCountry";

        public string XML_VisitingAddress_TAG = "VisitingAddress";
        public string XML_VisitingCoAddress_TAG = "VisitingCoAddress";
        public string XML_VisitingPostalCode_TAG = "VisitingPostalCode";
        public string XML_VisitingPostalAddress_TAG = "VisitingPostalAddress";
        public string XML_VisitingCountry_TAG = "VisitingCountry";
      
        public string XML_DeliveryAddress_TAG = "DeliveryAddress";
        public string XML_DeliveryCoAddress_TAG = "DeliveryCoAddress";
        public string XML_DeliveryPostalCode_TAG = "DeliveryPostalCode";
        public string XML_DeliveryPostalAddress_TAG = "DeliveryPostalAddress";
        public string XML_DeliveryCountry_TAG = "DeliveryCountry";
 
        public string XML_Email1_TAG = "Email1";
        public string XML_Email2_TAG = "Email2";
        public string XML_PhoneHome_TAG = "PhoneHome";
        public string XML_PhoneMobile_TAG = "PhoneMobile";
        public string XML_PhoneJob_TAG = "PhoneJob";
        public string XML_Fax_TAG = "Fax";
        public string XML_Webpage_TAG = "Webpage";

        #endregion

        #region Constructors

        public CompanyInformationIOItem()
        {
        }

        public CompanyInformationIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public CompanyInformationIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(content, headType, actorCompanyId);
        }

        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType, actorCompanyId);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> elementCompanyInformations = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementCompanyInformations, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementCompanyInformations, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {

            foreach (var elementCompanyInformation in elementCompanyInformations)
            {
                CompanyInformationIODTO companyInformationIODTO = new CompanyInformationIODTO();

                companyInformationIODTO.CompanyNumber = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_CompanyNumber_TAG);
                companyInformationIODTO.CompanyName = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_CompanyName_TAG);
                companyInformationIODTO.CompanyShortName = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_CompanyShortName_TAG);
                companyInformationIODTO.BaseCurrencyCode = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BaseCurrencyCode_TAG);
                companyInformationIODTO.EntCurrencyCode = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_EntCurrencyCode_TAG);
                companyInformationIODTO.Country = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_Country_TAG);
                companyInformationIODTO.OrgNr = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_OrgNr_TAG);
                companyInformationIODTO.VatNr = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_VatNr_TAG);
                companyInformationIODTO.InvoiceReference = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_InvoiceReference_TAG);

                companyInformationIODTO.BgNumber = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BgNumber_TAG);
                companyInformationIODTO.PgNumber = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_PgNumber_TAG);
                companyInformationIODTO.BankNumber = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BankNumber_TAG);
                companyInformationIODTO.CfpNumber = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_CfpNumber_TAG);
                companyInformationIODTO.SepaNumber = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_SepaNumber_TAG);
                companyInformationIODTO.NetsDirectNumber = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_NetsDirectNumber_TAG);
                companyInformationIODTO.IbanNumber = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_IbanNumber_TAG);
                companyInformationIODTO.BicNumber = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BicNumber_TAG);

                companyInformationIODTO.DistributionAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DistributionAddress_TAG);
                companyInformationIODTO.DistributionCoAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DistributionCoAddress_TAG);
                companyInformationIODTO.DistributionPostalCode = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DistributionPostalCode_TAG);
                companyInformationIODTO.DistributionPostalAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DistributionPostalAddress_TAG);
                companyInformationIODTO.DistributionCountry = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DistributionCountry_TAG);

                companyInformationIODTO.BillingAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BillingAddress_TAG);
                companyInformationIODTO.BillingCoAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BillingCoAddress_TAG);
                companyInformationIODTO.BillingPostalCode = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BillingPostalCode_TAG);
                companyInformationIODTO.BillingPostalAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BillingPostalAddress_TAG);
                companyInformationIODTO.BillingCountry = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BillingCountry_TAG);

                companyInformationIODTO.BoardHQAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BoardHQAddress_TAG);
                companyInformationIODTO.BoardHQCountry = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_BoardHQCountry_TAG);

                companyInformationIODTO.VisitingAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_VisitingAddress_TAG);
                companyInformationIODTO.VisitingCoAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_VisitingCoAddress_TAG);
                companyInformationIODTO.VisitingPostalCode = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_VisitingPostalCode_TAG);
                companyInformationIODTO.VisitingPostalAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_VisitingPostalAddress_TAG);
                companyInformationIODTO.VisitingCountry = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_VisitingCountry_TAG);

                companyInformationIODTO.DeliveryAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DeliveryAddress_TAG);
                companyInformationIODTO.DeliveryCoAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DeliveryCoAddress_TAG);
                companyInformationIODTO.DeliveryPostalCode = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DeliveryPostalCode_TAG);
                companyInformationIODTO.DeliveryPostalAddress = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DeliveryPostalAddress_TAG);
                companyInformationIODTO.DeliveryCountry = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_DeliveryCountry_TAG);

                companyInformationIODTO.Email1 = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_Email1_TAG);
                companyInformationIODTO.Email2 = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_Email2_TAG);
                companyInformationIODTO.PhoneHome = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_PhoneHome_TAG);
                companyInformationIODTO.PhoneMobile = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_PhoneMobile_TAG);
                companyInformationIODTO.PhoneJob = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_PhoneJob_TAG);
                companyInformationIODTO.Fax = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_Fax_TAG);
                companyInformationIODTO.Webpage = XmlUtil.GetChildElementValue(elementCompanyInformation, XML_Webpage_TAG);

                companyInformationIOs.Add(companyInformationIODTO);
            }

        }
        #endregion
    }
}
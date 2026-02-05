using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class AccountDistributionRowIOItem
    {

        #region Collections

        public List<AccountDistributionRowIODTO> accountDistributionRowIOs = new List<AccountDistributionRowIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "AccountDistributionRowIO";

        public string XML_RowNbr_TAG = "RowNbr";
        public string XML_CalculateRowNbr_TAG = "CalculateRowNbr";
        public string XML_SameBalance_TAG = "SameBalance";
        public string XML_OppositeBalance_TAG = "OppositeBalance";
        public string XML_Description_TAG = "Description";
        public string XML_Dim1Nr_TAG = "Dim1Nr";
        public string XML_Dim2Nr_TAG = "Dim2Nr";
        public string XML_Dim3Nr_TAG = "Dim3Nr";
        public string XML_Dim4Nr_TAG = "Dim4Nr";
        public string XML_Dim5Nr_TAG = "Dim5Nr";
        public string XML_Dim6Nr_TAG = "Dim6Nr";
        public string XML_Dim1Name_TAG = "Dim1Name";
        public string XML_Dim2Name_TAG = "Dim2Name";
        public string XML_Dim3Name_TAG = "Dim3Name";
        public string XML_Dim4Name_TAG = "Dim4Name";
        public string XML_Dim5Name_TAG = "Dim5Name";
        public string XML_Dim6Name_TAG = "Dim6Name";
        public string XML_Dim1Disabled_TAG = "Dim1Disabled";
        public string XML_Dim2Disabled_TAG = "Dim2Disabled";
        public string XML_Dim3Disabled_TAG = "Dim3Disabled";
        public string XML_Dim4Disabled_TAG = "Dim4Disabled";
        public string XML_Dim5Disabled_TAG = "Dim5Disabled";
        public string XML_Dim6Disabled_TAG = "Dim6Disabled";
        public string XML_Dim1Mandatory_TAG = "Dim1Mandatory";
        public string XML_Dim2Mandatory_TAG = "Dim2Mandatory";
        public string XML_Dim3Mandatory_TAG = "Dim3Mandatory";
        public string XML_Dim4Mandatory_TAG = "Dim4Mandatory";
        public string XML_Dim5Mandatory_TAG = "Dim5Mandatory";
        public string XML_Dim6Mandatory_TAG = "Dim6Mandatory";
        public string XML_Dim1KeepSourceRowAccount_TAG = "Dim1KeepSourceRowAccount";
        public string XML_Dim2KeepSourceRowAccount_TAG = "Dim2KeepSourceRowAccount";
        public string XML_Dim3KeepSourceRowAccount_TAG = "Dim3KeepSourceRowAccount";
        public string XML_Dim4KeepSourceRowAccount_TAG = "Dim4KeepSourceRowAccount";
        public string XML_Dim5KeepSourceRowAccount_TAG = "Dim5KeepSourceRowAccount";
        public string XML_Dim6KeepSourceRowAccount_TAG = "Dim6KeepSourceRowAccount";

        public string XML_DimSieDim1_TAG = "DimSieDim1";
        public string XML_DimNameSieDim1_TAG = "DimNameSieDim1";
        public string XML_DimDisabledSieDim1_TAG = "DimDisabledSieDim1";
        public string XML_DimMandatorySieDim1_TAG = "DimMandatorySieDim1";
        public string XML_DimKeepSourceRowAccountSieDim1_TAG = "DimKeepSourceRowAccountSieDim1";
        public string XML_DimSieDim6_TAG = "DimSieDim6";
        public string XML_DimNameSieDim6_TAG = "DimNameSieDim6";
        public string XML_DimDisabledSieDim6_TAG = "DimDisabledSieDim6";
        public string XML_DimMandatorySieDim6_TAG = "DimMandatorySieDim6";
        public string XML_DimKeepSourceRowAccountSieDim6_TAG = "DimKeepSourceRowAccountSieDim6";


        public string XML_PreviousRowNbr_TAG = "PreviousRowNbr";

        #endregion

        #region Constructors

        public AccountDistributionRowIOItem()
        {
        }

        public AccountDistributionRowIOItem(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(contents, headType);
        }

        public AccountDistributionRowIOItem(string content, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(content, headType);
        }

        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> accountDistributionRowElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(accountDistributionRowElements, headType);

        }

        public void CreateObjects(List<XElement> accountDistributionRowElements, TermGroup_IOImportHeadType headType)
        {
            foreach (var accountDistributionRowElement in accountDistributionRowElements)
            {
                AccountDistributionRowIODTO accountDistributionRowIODTO = new AccountDistributionRowIODTO();

                accountDistributionRowIODTO.RowNbr = XmlUtil.GetElementIntValue(accountDistributionRowElement, XML_RowNbr_TAG);
                accountDistributionRowIODTO.CalculateRowNbr = XmlUtil.GetElementIntValue(accountDistributionRowElement, XML_CalculateRowNbr_TAG);
                accountDistributionRowIODTO.Description = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Description_TAG);
                accountDistributionRowIODTO.OppositeBalance = XmlUtil.GetElementDecimalValue(accountDistributionRowElement, XML_OppositeBalance_TAG); 
                accountDistributionRowIODTO.SameBalance = XmlUtil.GetElementDecimalValue(accountDistributionRowElement, XML_SameBalance_TAG);
                accountDistributionRowIODTO.PreviousRowNbr = XmlUtil.GetElementIntValue(accountDistributionRowElement, XML_PreviousRowNbr_TAG);

                accountDistributionRowIODTO.Dim1Nr = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim1Nr_TAG);
                accountDistributionRowIODTO.Dim2Nr = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim2Nr_TAG);
                accountDistributionRowIODTO.Dim3Nr = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim3Nr_TAG);
                accountDistributionRowIODTO.Dim4Nr = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim4Nr_TAG);
                accountDistributionRowIODTO.Dim5Nr = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim5Nr_TAG);
                accountDistributionRowIODTO.Dim6Nr = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim6Nr_TAG);
                accountDistributionRowIODTO.DimSieDim1 = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_DimSieDim1_TAG);
                accountDistributionRowIODTO.DimSieDim6 = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_DimSieDim6_TAG);
                accountDistributionRowIODTO.Dim1Name = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim1Name_TAG);
                accountDistributionRowIODTO.Dim2Name = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim2Name_TAG);
                accountDistributionRowIODTO.Dim3Name = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim3Name_TAG);
                accountDistributionRowIODTO.Dim4Name = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim4Name_TAG);
                accountDistributionRowIODTO.Dim5Name = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim5Name_TAG);
                accountDistributionRowIODTO.Dim6Name = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_Dim6Name_TAG);
                accountDistributionRowIODTO.DimNameSieDim1 = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_DimNameSieDim1_TAG);
                accountDistributionRowIODTO.DimNameSieDim6 = XmlUtil.GetChildElementValue(accountDistributionRowElement, XML_DimNameSieDim6_TAG);

                accountDistributionRowIODTO.Dim1Disabled = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim1Disabled_TAG);
                accountDistributionRowIODTO.Dim2Disabled = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim2Disabled_TAG);
                accountDistributionRowIODTO.Dim3Disabled = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim3Disabled_TAG);
                accountDistributionRowIODTO.Dim4Disabled = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim4Disabled_TAG);
                accountDistributionRowIODTO.Dim5Disabled = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim5Disabled_TAG);
                accountDistributionRowIODTO.Dim6Disabled = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim6Disabled_TAG);
                accountDistributionRowIODTO.DimDisabledSieDim1 = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_DimDisabledSieDim1_TAG);
                accountDistributionRowIODTO.DimDisabledSieDim6 = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_DimDisabledSieDim6_TAG);

                accountDistributionRowIODTO.Dim1Mandatory = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim1Mandatory_TAG);
                accountDistributionRowIODTO.Dim2Mandatory = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim2Mandatory_TAG);
                accountDistributionRowIODTO.Dim3Mandatory = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim3Mandatory_TAG);
                accountDistributionRowIODTO.Dim4Mandatory = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim4Mandatory_TAG);
                accountDistributionRowIODTO.Dim5Mandatory = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim5Mandatory_TAG);
                accountDistributionRowIODTO.Dim6Mandatory = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim6Mandatory_TAG);
                accountDistributionRowIODTO.DimMandatorySieDim1 = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_DimMandatorySieDim1_TAG);
                accountDistributionRowIODTO.DimMandatorySieDim6 = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_DimMandatorySieDim6_TAG);

                accountDistributionRowIODTO.Dim2KeepSourceRowAccount = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim2KeepSourceRowAccount_TAG);
                accountDistributionRowIODTO.Dim3KeepSourceRowAccount = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim3KeepSourceRowAccount_TAG);
                accountDistributionRowIODTO.Dim4KeepSourceRowAccount = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim4KeepSourceRowAccount_TAG);
                accountDistributionRowIODTO.Dim5KeepSourceRowAccount = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim5KeepSourceRowAccount_TAG);
                accountDistributionRowIODTO.Dim6KeepSourceRowAccount = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_Dim6KeepSourceRowAccount_TAG);
                accountDistributionRowIODTO.DimKeepSourceRowAccountSieDim1 = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_DimKeepSourceRowAccountSieDim1_TAG);
                accountDistributionRowIODTO.DimKeepSourceRowAccountSieDim6 = XmlUtil.GetElementBoolValue(accountDistributionRowElement, XML_DimKeepSourceRowAccountSieDim6_TAG);



                accountDistributionRowIOs.Add(accountDistributionRowIODTO);
            }

        }
        #endregion
    }
}

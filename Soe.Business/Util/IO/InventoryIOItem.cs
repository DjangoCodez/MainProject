using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class InventoryIOItem
    {

        #region Collections

        public List<InventoryIODTO> inventoryIOs = new List<InventoryIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "InventoryIO";
        public const string XML_CHILD_TAG = "InventoryWriteOff";

        #region Parent

        public string XML_InventoryNr_TAG = "InventoryNr";
        public string XML_Name_TAG = "Name";
        public string XML_Description_TAG = "Description";
        public string XML_Notes_TAG = "Notes";

        public string XML_Status_TAG = "Status";
        public string XML_InventoryWriteOffMethodName_TAG = "InventoryWriteOffMethodName";
        public string XML_VoucherSeriesTypeNr_TAG = "VoucherSeriesTypeNr";
        public string XML_SupplierInvoiceNr_TAG = "SupplierInvoiceNr";
        public string XML_CustomerInvoiceNr_TAG = "CustomerInvoiceNr";

        public string XML_PurchaseDate_TAG = "PurchaseDate";
        public string XML_WriteOffDate_TAG = "WriteOffDate";
        public string XML_PurchaseAmount_TAG = "PurchaseAmount";
        public string XML_WriteOffAmount_TAG = "WriteOffAmount";
        public string XML_WriteOffSum_TAG = "WriteOffSum";
        public string XML_WriteOffRemainingAmount_TAG = "WriteOffRemainingAmount";
        public string XML_EndAmount_TAG = "EndAmount";

        public string XML_PeriodType_TAG = "PeriodType";
        public string XML_PeriodValue_TAG = "PeriodValue";
        public string XML_WriteOffPeriods_TAG = "WriteOffPeriods";

        public string XML_State_TAG = "State";

        public string XML_InventoryAccountNr_TAG = "InventoryAccountNr";
        public string XML_InventoryAccountDim2Nr_TAG = "InventoryAccountDim2Nr";
        public string XML_InventoryAccountDim3Nr_TAG = "InventoryAccountDim3Nr";
        public string XML_InventoryAccountDim4Nr_TAG = "InventoryAccountDim4Nr";
        public string XML_InventoryAccountDim5Nr_TAG = "InventoryAccountDim5Nr";
        public string XML_InventoryAccountDim6Nr_TAG = "InventoryAccountDim6Nr";
        public string XML_InventoryAccountSieDim1_TAG = "InventoryAccountSieDim1";
        public string XML_InventoryAccountSieDim6_TAG = "InventoryAccountSieDim6";


        public string XML_AccWriteOffAccountNr_TAG = "AccWriteOffAccountNr";
        public string XML_AccWriteOffAccountDim2Nr_TAG = "AccWriteOffAccountDim2Nr";
        public string XML_AccWriteOffAccountDim3Nr_TAG = "AccWriteOffAccountDim3Nr";
        public string XML_AccWriteOffAccountDim4Nr_TAG = "AccWriteOffAccountDim4Nr";
        public string XML_AccWriteOffAccountDim5Nr_TAG = "AccWriteOffAccountDim5Nr";
        public string XML_AccWriteOffAccountDim6Nr_TAG = "AccWriteOffAccountDim6Nr";
        public string XML_AccWriteOffAccountSieDim1_TAG = "AccWriteOffAccountSieDim1";
        public string XML_AccWriteOffAccountSieDim6_TAG = "AccWriteOffAccountSieDim6";

        public string XML_WriteOffAccountNr_TAG = "WriteOffAccountNr";
        public string XML_WriteOffAccountDim2Nr_TAG = "WriteOffAccountDim2Nr";
        public string XML_WriteOffAccountDim3Nr_TAG = "WriteOffAccountDim3Nr";
        public string XML_WriteOffAccountDim4Nr_TAG = "WriteOffAccountDim4Nr";
        public string XML_WriteOffAccountDim5Nr_TAG = "WriteOffAccountDim5Nr";
        public string XML_WriteOffAccountDim6Nr_TAG = "WriteOffAccountDim6Nr";
        public string XML_WriteOffAccountSieDim1_TAG = "WriteOffAccountSieDim1";
        public string XML_WriteOffAccountSieDim6_TAG = "WriteOffAccountSieDim6";

        public string XML_AccOverWriteOffAccountNr_TAG = "AccOverWriteOffAccountNr";
        public string XML_AccOverWriteOffAccountDim2Nr_TAG = "AccOverWriteOffAccountDim2Nr";
        public string XML_AccOverWriteOffAccountDim3Nr_TAG = "AccOverWriteOffAccountDim3Nr";
        public string XML_AccOverWriteOffAccountDim4Nr_TAG = "AccOverWriteOffAccountDim4Nr";
        public string XML_AccOverWriteOffAccountDim5Nr_TAG = "AccOverWriteOffAccountDim5Nr";
        public string XML_AccOverWriteOffAccountDim6Nr_TAG = "AccOverWriteOffAccountDim6Nr";
        public string XML_AccOverWriteOffAccountSieDim1_TAG = "AccOverWriteOffAccountSieDim1";
        public string XML_AccOverWriteOffAccountSieDim6_TAG = "AccOverWriteOffAccountSieDim6";

        public string XML_OverWriteOffAccountNr_TAG = "OverWriteOffAccountNr";
        public string XML_OverWriteOffAccountDim2Nr_TAG = "OverWriteOffAccountDim2Nr";
        public string XML_OverWriteOffAccountDim3Nr_TAG = "OverWriteOffAccountDim3Nr";
        public string XML_OverWriteOffAccountDim4Nr_TAG = "OverWriteOffAccountDim4Nr";
        public string XML_OverWriteOffAccountDim5Nr_TAG = "OverWriteOffAccountDim5Nr";
        public string XML_OverWriteOffAccountDim6Nr_TAG = "OverWriteOffAccountDim6Nr";
        public string XML_OverWriteOffAccountSieDim1_TAG = "OverWriteOffAccountSieDim1";
        public string XML_OverWriteOffAccountSieDim6_TAG = "OverWriteOffAccountSieDim6";

        public string XML_AccWriteDownAccountNr_TAG = "AccWriteDownAccountNr";
        public string XML_AccWriteDownAccountDim2Nr_TAG = "AccWriteDownAccountDim2Nr";
        public string XML_AccWriteDownAccountDim3Nr_TAG = "AccWriteDownAccountDim3Nr";
        public string XML_AccWriteDownAccountDim4Nr_TAG = "AccWriteDownAccountDim4Nr";
        public string XML_AccWriteDownAccountDim5Nr_TAG = "AccWriteDownAccountDim5Nr";
        public string XML_AccWriteDownAccountDim6Nr_TAG = "AccWriteDownAccountDim6Nr";
        public string XML_AccWriteDownAccountSieDim1_TAG = "AccWriteDownAccountSieDim1";
        public string XML_AccWriteDownAccountSieDim6_TAG = "AccWriteDownAccountSieDim6";

        public string XML_WriteDownAccountNr_TAG = "WriteDownAccountNr";
        public string XML_WriteDownAccountDim2Nr_TAG = "WriteDownAccountDim2Nr";
        public string XML_WriteDownAccountDim3Nr_TAG = "WriteDownAccountDim3Nr";
        public string XML_WriteDownAccountDim4Nr_TAG = "WriteDownAccountDim4Nr";
        public string XML_WriteDownAccountDim5Nr_TAG = "WriteDownAccountDim5Nr";
        public string XML_WriteDownAccountDim6Nr_TAG = "WriteDownAccountDim6Nr";
        public string XML_WriteDownAccountSieDim1_TAG = "WriteDownAccountSieDim1";
        public string XML_WriteDownAccountSieDim6_TAG = "WriteDownAccountSieDim6";

        public string XML_AccWriteUpAccountNr_TAG = "AccWriteUpAccountNr";
        public string XML_AccWriteUpAccountDim2Nr_TAG = "AccWriteUpAccountDim2Nr";
        public string XML_AccWriteUpAccountDim3Nr_TAG = "AccWriteUpAccountDim3Nr";
        public string XML_AccWriteUpAccountDim4Nr_TAG = "AccWriteUpAccountDim4Nr";
        public string XML_AccWriteUpAccountDim5Nr_TAG = "AccWriteUpAccountDim5Nr";
        public string XML_AccWriteUpAccountDim6Nr_TAG = "AccWriteUpAccountDim6Nr";
        public string XML_AccWriteUpAccountSieDim1_TAG = "AccWriteUpAccountSieDim1";
        public string XML_AccWriteUpAccountSieDim6_TAG = "AccWriteUpAccountSieDim6";


        public string XML_WriteUpAccountNr_TAG = "WriteUpAccountNr";
        public string XML_WriteUpAccountDim2Nr_TAG = "WriteUpAccountDim2Nr";
        public string XML_WriteUpAccountDim3Nr_TAG = "WriteUpAccountDim3Nr";
        public string XML_WriteUpAccountDim4Nr_TAG = "WriteUpAccountDim4Nr";
        public string XML_WriteUpAccountDim5Nr_TAG = "WriteUpAccountDim5Nr";
        public string XML_WriteUpAccountDim6Nr_TAG = "WriteUpAccountDim6Nr";
        public string XML_WriteUpAccountSieDim1_TAG = "WriteUpAccountSieDim1";
        public string XML_WriteUpAccountSieDim6_TAG = "WriteUpAccountSieDim6";

        public string XML_ParentName_TAG = "ParentName";

        public string XML_InventoryWriteOffMethodDescription_TAG = "InventoryWriteOffMethodDescription";
        public string XML_InventoryWriteOffMethodPeriodType_TAG = "InventoryWriteOffMethodPeriodType";
        public string XML_InventoryWriteOffMethodPeriodValue_TAG = "InventoryWriteOffMethodPeriodValue";
        public string XML_InventoryWriteOffMethodType_TAG = "InventoryWriteOffMethodType";

        #region Child

        public string XML_DebitAmount_TAG = "DebitAmount";
        public string XML_CreditAmount_TAG = "CreditAmount";
        public string XML_DebitAmountCurrency_TAG = "DebitAmountCurrency";
        public string XML_CreditAmountCurrency_TAG = "CreditAmountCurrency";
        public string XML_DebitAmountEntCurrency_TAG = "DebitAmountEntCurrency";
        public string XML_CreditAmountEntCurrency_TAG = "CreditAmountEntCurrency";
        public string XML_DebitAmountLedgerCurrency_TAG = "DebitAmountLedgerCurrency";
        public string XML_CreditAmountLedgerCurrency_TAG = "CreditAmountLedgerCurrency";

        public string XML_Dim1Id_TAG = "Dim1Id";
        public string XML_Dim1Nr_TAG = "Dim1Nr";
        public string XML_Dim1Name_TAG = "Dim1Name";
        public string XML_Dim1DimName_TAG = "Dim1DimName";


        public string XML_SameBalance_TAG = "SameBalance";
        public string XML_OppositeBalance_TAG = "OppositeBalance";

        public string XML_Dim2Id_TAG = "Dim2Id";
        public string XML_Dim2Nr_TAG = "Dim2Nr";
        public string XML_Dim2Name_TAG = "Dim2Name";
        public string XML_Dim2DimName_TAG = "Dim2DimName";
        public string XML_Dim3Id_TAG = "Dim3Id";
        public string XML_Dim3Nr_TAG = "Dim3Nr";
        public string XML_Dim3Name_TAG = "Dim3Name";
        public string XML_Dim3DimName_TAG = "Dim3DimName";
        public string XML_Dim4Id_TAG = "Dim4Id";
        public string XML_Dim4Nr_TAG = "Dim4Nr";
        public string XML_Dim4Name_TAG = "Dim4Name";
        public string XML_Dim4DimName_TAG = "Dim4DimName";
        public string XML_Dim5Id_TAG = "Dim5Id";
        public string XML_Dim5Nr_TAG = "Dim5Nr";
        public string XML_Dim5Name_TAG = "Dim5Name";
        public string XML_Dim5DimName_TAG = "Dim5DimName";
        public string XML_Dim6Id_TAG = "Dim6Id";
        public string XML_Dim6Nr_TAG = "Dim6Nr";
        public string XML_Dim6Name_TAG = "Dim6Name";
        public string XML_Dim6DimName_TAG = "Dim6DimName";
        public string XML_DimNrSieDim1_TAG = "DimNrSieDim1";
        public string XML_DimNrSieDim6_TAG = "DimNrSieDim6";

        public string XML_TriggerType_TAG = "TriggerType";
        public string XML_Date_TAG = "Date";
        public string XML_SupplierInvoiceNr2_TAG = "SupplierInvoiceNr";
        public string XML_SupplierNr2_TAG = "SupplierNr";
        public string XML_InventoryNr2_TAG = "InventoryNr";
        public string XML_VoucherNr_TAG = "VoucherNr";


        #endregion

        #endregion

        #endregion

        #region Constructors


        public InventoryIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
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

            List<XElement> elementInventorys = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementInventorys, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementInventorys, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementInventory in elementInventorys)
            {
                InventoryIODTO inventoryIODTO = new InventoryIODTO();

                inventoryIODTO.InventoryNr = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryNr_TAG);
                inventoryIODTO.Name = XmlUtil.GetChildElementValue(elementInventory, XML_Name_TAG);
                inventoryIODTO.Description = XmlUtil.GetChildElementValue(elementInventory, XML_Description_TAG);
                inventoryIODTO.Notes = XmlUtil.GetChildElementValue(elementInventory, XML_Notes_TAG);

                inventoryIODTO.Status = XmlUtil.GetElementIntValue(elementInventory, XML_Status_TAG);
                inventoryIODTO.InventoryWriteOffMethodName = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryWriteOffMethodName_TAG);
                inventoryIODTO.VoucherSeriesTypeNr = XmlUtil.GetChildElementValue(elementInventory, XML_VoucherSeriesTypeNr_TAG);
                inventoryIODTO.SupplierInvoiceNr = XmlUtil.GetChildElementValue(elementInventory, XML_SupplierInvoiceNr_TAG);
                inventoryIODTO.CustomerInvoiceNr = XmlUtil.GetChildElementValue(elementInventory, XML_CustomerInvoiceNr_TAG);

                inventoryIODTO.PurchaseDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(elementInventory, XML_PurchaseDate_TAG));
                inventoryIODTO.WriteOffDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(elementInventory, XML_WriteOffDate_TAG));
                inventoryIODTO.PurchaseAmount = XmlUtil.GetElementDecimalValue(elementInventory, XML_PurchaseAmount_TAG);
                inventoryIODTO.WriteOffAmount = XmlUtil.GetElementDecimalValue(elementInventory, XML_WriteOffAmount_TAG);
                inventoryIODTO.WriteOffSum = XmlUtil.GetElementDecimalValue(elementInventory, XML_WriteOffSum_TAG);
                inventoryIODTO.WriteOffRemainingAmount = XmlUtil.GetElementDecimalValue(elementInventory, XML_WriteOffRemainingAmount_TAG);

                if (inventoryIODTO.WriteOffRemainingAmount == 0 && inventoryIODTO.WriteOffSum > 0) 
                    inventoryIODTO.WriteOffRemainingAmount = inventoryIODTO.WriteOffAmount - inventoryIODTO.WriteOffSum;

                inventoryIODTO.EndAmount = XmlUtil.GetElementDecimalValue(elementInventory, XML_EndAmount_TAG);

                inventoryIODTO.PeriodType = XmlUtil.GetElementIntValue(elementInventory, XML_PeriodType_TAG);
                inventoryIODTO.PeriodValue = XmlUtil.GetElementIntValue(elementInventory, XML_PeriodValue_TAG);
                inventoryIODTO.WriteOffPeriods = XmlUtil.GetElementIntValue(elementInventory, XML_WriteOffPeriods_TAG);

                inventoryIODTO.State = XmlUtil.GetElementIntValue(elementInventory, XML_State_TAG);

                inventoryIODTO.InventoryAccountNr = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryAccountNr_TAG);
                inventoryIODTO.InventoryAccountDim2Nr = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryAccountDim2Nr_TAG);
                inventoryIODTO.InventoryAccountDim3Nr = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryAccountDim3Nr_TAG);
                inventoryIODTO.InventoryAccountDim4Nr = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryAccountDim4Nr_TAG);
                inventoryIODTO.InventoryAccountDim5Nr = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryAccountDim5Nr_TAG);
                inventoryIODTO.InventoryAccountDim6Nr = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryAccountDim6Nr_TAG);
                inventoryIODTO.InventoryAccountSieDim1 = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryAccountSieDim1_TAG);
                inventoryIODTO.InventoryAccountSieDim6 = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryAccountSieDim6_TAG);


                inventoryIODTO.AccWriteOffAccountNr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteOffAccountNr_TAG);
                inventoryIODTO.AccWriteOffAccountDim2Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteOffAccountDim2Nr_TAG);
                inventoryIODTO.AccWriteOffAccountDim3Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteOffAccountDim3Nr_TAG);
                inventoryIODTO.AccWriteOffAccountDim4Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteOffAccountDim4Nr_TAG);
                inventoryIODTO.AccWriteOffAccountDim5Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteOffAccountDim5Nr_TAG);
                inventoryIODTO.AccWriteOffAccountDim6Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteOffAccountDim6Nr_TAG);
                inventoryIODTO.AccWriteOffAccountSieDim1 = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteOffAccountSieDim1_TAG);
                inventoryIODTO.AccWriteOffAccountSieDim6 = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteOffAccountSieDim6_TAG);

                inventoryIODTO.WriteOffAccountNr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteOffAccountNr_TAG);
                inventoryIODTO.WriteOffAccountDim2Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteOffAccountDim2Nr_TAG);
                inventoryIODTO.WriteOffAccountDim3Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteOffAccountDim3Nr_TAG);
                inventoryIODTO.WriteOffAccountDim4Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteOffAccountDim4Nr_TAG);
                inventoryIODTO.WriteOffAccountDim5Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteOffAccountDim5Nr_TAG);
                inventoryIODTO.WriteOffAccountDim6Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteOffAccountDim6Nr_TAG);
                inventoryIODTO.WriteOffAccountSieDim1 = XmlUtil.GetChildElementValue(elementInventory, XML_WriteOffAccountSieDim1_TAG);
                inventoryIODTO.WriteOffAccountSieDim6 = XmlUtil.GetChildElementValue(elementInventory, XML_WriteOffAccountSieDim6_TAG);

                inventoryIODTO.AccOverWriteOffAccountNr = XmlUtil.GetChildElementValue(elementInventory, XML_AccOverWriteOffAccountNr_TAG);
                inventoryIODTO.AccOverWriteOffAccountDim2Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccOverWriteOffAccountDim2Nr_TAG);
                inventoryIODTO.AccOverWriteOffAccountDim3Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccOverWriteOffAccountDim3Nr_TAG);
                inventoryIODTO.AccOverWriteOffAccountDim4Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccOverWriteOffAccountDim4Nr_TAG);
                inventoryIODTO.AccOverWriteOffAccountDim5Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccOverWriteOffAccountDim5Nr_TAG);
                inventoryIODTO.AccOverWriteOffAccountDim6Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccOverWriteOffAccountDim6Nr_TAG);
                inventoryIODTO.AccOverWriteOffAccountSieDim1 = XmlUtil.GetChildElementValue(elementInventory, XML_AccOverWriteOffAccountSieDim1_TAG);
                inventoryIODTO.AccOverWriteOffAccountSieDim6 = XmlUtil.GetChildElementValue(elementInventory, XML_AccOverWriteOffAccountSieDim6_TAG);

                inventoryIODTO.OverWriteOffAccountNr = XmlUtil.GetChildElementValue(elementInventory, XML_OverWriteOffAccountNr_TAG);
                inventoryIODTO.OverWriteOffAccountDim2Nr = XmlUtil.GetChildElementValue(elementInventory, XML_OverWriteOffAccountDim2Nr_TAG);
                inventoryIODTO.OverWriteOffAccountDim3Nr = XmlUtil.GetChildElementValue(elementInventory, XML_OverWriteOffAccountDim3Nr_TAG);
                inventoryIODTO.OverWriteOffAccountDim4Nr = XmlUtil.GetChildElementValue(elementInventory, XML_OverWriteOffAccountDim4Nr_TAG);
                inventoryIODTO.OverWriteOffAccountDim5Nr = XmlUtil.GetChildElementValue(elementInventory, XML_OverWriteOffAccountDim5Nr_TAG);
                inventoryIODTO.OverWriteOffAccountDim6Nr = XmlUtil.GetChildElementValue(elementInventory, XML_OverWriteOffAccountDim6Nr_TAG);

                inventoryIODTO.AccWriteDownAccountNr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteDownAccountNr_TAG);
                inventoryIODTO.AccWriteDownAccountDim2Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteDownAccountDim2Nr_TAG);
                inventoryIODTO.AccWriteDownAccountDim3Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteDownAccountDim3Nr_TAG);
                inventoryIODTO.AccWriteDownAccountDim4Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteDownAccountDim4Nr_TAG);
                inventoryIODTO.AccWriteDownAccountDim5Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteDownAccountDim5Nr_TAG);
                inventoryIODTO.AccWriteDownAccountDim6Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteDownAccountDim6Nr_TAG);
                inventoryIODTO.AccWriteDownAccountSieDim1 = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteDownAccountSieDim1_TAG);
                inventoryIODTO.AccWriteDownAccountSieDim6 = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteDownAccountSieDim6_TAG);

                inventoryIODTO.WriteDownAccountNr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteDownAccountNr_TAG);
                inventoryIODTO.WriteDownAccountDim2Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteDownAccountDim2Nr_TAG);
                inventoryIODTO.WriteDownAccountDim3Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteDownAccountDim3Nr_TAG);
                inventoryIODTO.WriteDownAccountDim4Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteDownAccountDim4Nr_TAG);
                inventoryIODTO.WriteDownAccountDim5Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteDownAccountDim5Nr_TAG);
                inventoryIODTO.WriteDownAccountDim6Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteDownAccountDim6Nr_TAG);
                inventoryIODTO.WriteDownAccountSieDim1 = XmlUtil.GetChildElementValue(elementInventory, XML_WriteDownAccountSieDim1_TAG);
                inventoryIODTO.WriteDownAccountSieDim6 = XmlUtil.GetChildElementValue(elementInventory, XML_WriteDownAccountSieDim6_TAG);

                inventoryIODTO.AccWriteUpAccountNr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteUpAccountNr_TAG);
                inventoryIODTO.AccWriteUpAccountDim2Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteUpAccountDim2Nr_TAG);
                inventoryIODTO.AccWriteUpAccountDim3Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteUpAccountDim3Nr_TAG);
                inventoryIODTO.AccWriteUpAccountDim4Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteUpAccountDim4Nr_TAG);
                inventoryIODTO.AccWriteUpAccountDim5Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteUpAccountDim5Nr_TAG);
                inventoryIODTO.AccWriteUpAccountDim6Nr = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteUpAccountDim6Nr_TAG);
                inventoryIODTO.AccWriteUpAccountSieDim1 = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteUpAccountSieDim1_TAG);
                inventoryIODTO.AccWriteUpAccountSieDim6 = XmlUtil.GetChildElementValue(elementInventory, XML_AccWriteUpAccountSieDim6_TAG);

                inventoryIODTO.WriteUpAccountNr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteUpAccountNr_TAG);
                inventoryIODTO.WriteUpAccountDim2Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteUpAccountDim2Nr_TAG);
                inventoryIODTO.WriteUpAccountDim3Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteUpAccountDim3Nr_TAG);
                inventoryIODTO.WriteUpAccountDim4Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteUpAccountDim4Nr_TAG);
                inventoryIODTO.WriteUpAccountDim5Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteUpAccountDim5Nr_TAG);
                inventoryIODTO.WriteUpAccountDim6Nr = XmlUtil.GetChildElementValue(elementInventory, XML_WriteUpAccountDim6Nr_TAG);
                inventoryIODTO.WriteUpAccountSieDim1 = XmlUtil.GetChildElementValue(elementInventory, XML_WriteUpAccountSieDim1_TAG);
                inventoryIODTO.WriteUpAccountSieDim6 = XmlUtil.GetChildElementValue(elementInventory, XML_WriteUpAccountSieDim6_TAG);

                inventoryIODTO.ParentName = XmlUtil.GetChildElementValue(elementInventory, XML_ParentName_TAG);

                inventoryIODTO.InventoryWriteOffMethodDescription = XmlUtil.GetChildElementValue(elementInventory, XML_InventoryWriteOffMethodDescription_TAG);
                inventoryIODTO.InventoryWriteOffMethodPeriodType = XmlUtil.GetElementIntValue(elementInventory, XML_InventoryWriteOffMethodPeriodType_TAG);
                inventoryIODTO.InventoryWriteOffMethodPeriodValue = XmlUtil.GetElementIntValue(elementInventory, XML_InventoryWriteOffMethodPeriodValue_TAG);
                inventoryIODTO.InventoryWriteOffMethodType = XmlUtil.GetElementIntValue(elementInventory, XML_InventoryWriteOffMethodType_TAG);

                //if (inventoryIODTO.Status == 0)
                //    inventoryIODTO.Status = (int)TermGroup_InventoryStatus.Active;

                List<XElement> writeOffElements = XmlUtil.GetChildElements(elementInventory, XML_CHILD_TAG);

                inventoryIODTO.accountDistributionEntryRowIODTOs = new List<AccountDistributionEntryRowIODTO>();


                foreach (var writeOffElement in writeOffElements)
                {
                    AccountDistributionEntryRowIODTO rowIODTO = new AccountDistributionEntryRowIODTO();

                    rowIODTO.Dim1Id = XmlUtil.GetElementIntValue(writeOffElement, XML_Dim1Id_TAG);
                    rowIODTO.Dim1Nr = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim1Nr_TAG);
                    rowIODTO.Dim1Name = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim1Name_TAG);
                    rowIODTO.Dim1DimName = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim1DimName_TAG);

                    rowIODTO.SameBalance = XmlUtil.GetElementDecimalValue(writeOffElement, XML_SameBalance_TAG);
                    rowIODTO.OppositeBalance = XmlUtil.GetElementDecimalValue(writeOffElement, XML_OppositeBalance_TAG);

                    rowIODTO.Dim2Id = XmlUtil.GetElementIntValue(writeOffElement, XML_Dim2Id_TAG);
                    rowIODTO.Dim2Nr = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim2Nr_TAG);
                    rowIODTO.Dim2Name = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim2Name_TAG);
                    rowIODTO.Dim2DimName = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim2DimName_TAG);
                    rowIODTO.Dim3Id = XmlUtil.GetElementIntValue(writeOffElement, XML_Dim3Id_TAG);
                    rowIODTO.Dim3Nr = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim3Nr_TAG);
                    rowIODTO.Dim3Name = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim3Name_TAG);
                    rowIODTO.Dim3DimName = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim3DimName_TAG);
                    rowIODTO.Dim4Id = XmlUtil.GetElementIntValue(writeOffElement, XML_Dim4Id_TAG);
                    rowIODTO.Dim4Nr = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim4Nr_TAG);
                    rowIODTO.Dim4Name = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim4Name_TAG);
                    rowIODTO.Dim4DimName = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim4DimName_TAG);
                    rowIODTO.Dim5Id = XmlUtil.GetElementIntValue(writeOffElement, XML_Dim5Id_TAG);
                    rowIODTO.Dim5Nr = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim5Nr_TAG);
                    rowIODTO.Dim5Name = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim5Name_TAG);
                    rowIODTO.Dim5DimName = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim5DimName_TAG);
                    rowIODTO.Dim6Id = XmlUtil.GetElementIntValue(writeOffElement, XML_Dim6Id_TAG);
                    rowIODTO.Dim6Nr = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim6Nr_TAG);
                    rowIODTO.Dim6Name = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim6Name_TAG);
                    rowIODTO.Dim6DimName = XmlUtil.GetChildElementValue(writeOffElement, XML_Dim6DimName_TAG);
                    rowIODTO.DimNrSieDim1 = XmlUtil.GetChildElementValue(writeOffElement, XML_DimNrSieDim1_TAG);
                    rowIODTO.DimNrSieDim6 = XmlUtil.GetChildElementValue(writeOffElement, XML_DimNrSieDim6_TAG);

                    rowIODTO.TriggerType = XmlUtil.GetElementIntValue(writeOffElement, XML_TriggerType_TAG);
                    rowIODTO.Date = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(writeOffElement, XML_Date_TAG));
                    rowIODTO.SupplierInvoiceNr = XmlUtil.GetChildElementValue(writeOffElement, XML_SupplierInvoiceNr2_TAG);
                    rowIODTO.SupplierNr = XmlUtil.GetChildElementValue(writeOffElement, XML_SupplierNr2_TAG);
                    rowIODTO.InventoryNr = XmlUtil.GetChildElementValue(writeOffElement, XML_InventoryNr2_TAG);
                    rowIODTO.VoucherNr = XmlUtil.GetChildElementValue(writeOffElement, XML_VoucherNr_TAG);

                    inventoryIODTO.accountDistributionEntryRowIODTOs.Add(rowIODTO);
                }

                inventoryIOs.Add(inventoryIODTO);
            }

        }
        #endregion
    }
}
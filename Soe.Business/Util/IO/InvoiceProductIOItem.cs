using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class InvoiceProductIOItem
    {

        #region Collections

        public List<InvoiceProductIODTO> invoiceProductIOs = new List<InvoiceProductIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "Invoiceproduct";
        public string XML_Number_TAG = "Number";
        public string XML_Name_TAG = "Name";
        public string XML_Name2_TAG = "Name2";
        public string XML_Description_TAG = "Description";
        public string XML_Unit_TAG = "Unit";
        public string XML_ShowAsTextRow_TAG = "ShowAsTextRow";
        public string XML_EAN_TAG = "EAN";
        public string XML_Weight_TAG = "Weight";
        public string XML_MaterialCode_TAG = "MaterialCode";
        public string XML_VatType_TAG = "VatType";
        public string XML_PriceType_TAG = "PriceType";
        public string XML_VatCodeNr_TAG = "VatCodeNr";
        public string XML_ProductGroupCode_TAG = "ProductGroupCode";
        public string XML_PurchasePrice_TAG = "PurchasePrice";
        public string XML_SalesPrice_TAG = "SalesPrice";
        public string XML_PriceListType_TAG = "PriceListType";
        public string XML_ClaimAccountNr_TAG = "ClaimAccountNr";
        public string XML_ClaimAccountDim2Nr_TAG = "ClaimAccountDim2Nr";
        public string XML_ClaimAccountDim3Nr_TAG = "ClaimAccountDim3Nr";
        public string XML_ClaimAccountDim4Nr_TAG = "ClaimAccountDim4Nr";
        public string XML_ClaimAccountDim5Nr_TAG = "ClaimAccountDim5Nr";
        public string XML_ClaimAccountDim6Nr_TAG = "ClaimAccountDim6Nr";
        public string XML_ClaimAccountSieDim1_TAG = "ClaimAccountSieDim1";
        public string XML_ClaimAccountSieDim6_TAG = "ClaimAccountSieDim6";
        public string XML_SalesAccountNr_TAG = "SalesAccountNr";
        public string XML_SalesAccountDim2Nr_TAG = "SalesAccountDim2Nr";
        public string XML_SalesAccountDim3Nr_TAG = "SalesAccountDim3Nr";
        public string XML_SalesAccountDim4Nr_TAG = "SalesAccountDim4Nr";
        public string XML_SalesAccountDim5Nr_TAG = "SalesAccountDim5Nr";
        public string XML_SalesAccountDim6Nr_TAG = "SalesAccountDim6Nr";
        public string XML_SalesAccountSieDim1_TAG = "SalesAccountSieDim1";
        public string XML_SalesAccountSieDim6_TAG = "SalesAccountSieDim6";
        public string XML_ReversedVatSalesAccountNr_TAG = "ReversedVatSalesAccountNr";
        public string XML_ReversedVatSalesAccountDim2Nr_TAG = "ReversedVatSalesAccountDim2Nr";
        public string XML_ReversedVatSalesAccountDim3Nr_TAG = "ReversedVatSalesAccountDim3Nr";
        public string XML_ReversedVatSalesAccountDim4Nr_TAG = "ReversedVatSalesAccountDim4Nr";
        public string XML_ReversedVatSalesAccountDim5Nr_TAG = "ReversedVatSalesAccountDim5Nr";
        public string XML_ReversedVatSalesAccountDim6Nr_TAG = "ReversedVatSalesAccountDim6Nr";
        public string XML_ReversedVatSalesAccountSieDim1_TAG = "ReversedVatSalesAccountSieDim1";
        public string XML_ReversedVatSalesAccountSieDim6_TAG = "ReversedVatSalesAccountSieDim6";
        public string XML_VatFreeSalesAccountNr_TAG = "VatFreeSalesAccountNr";
        public string XML_VatAccountNr_TAG = "VatAccountNr";
        public string XML_CategoryCode1_TAG = "CategoryCode1";
        public string XML_CategoryCode2_TAG = "CategoryCode2";
        public string XML_CategoryCode3_TAG = "CategoryCode3";
        public string XML_CategoryCode4_TAG = "CategoryCode4";
        public string XML_CategoryCode5_TAG = "CategoryCode5";
        public string XML_PriceOnPriceList1_TAG = "PriceOnPriceList1";
        public string XML_PriceOnPriceList2_TAG = "PriceOnPriceList2";
        public string XML_PriceOnPriceList3_TAG = "PriceOnPriceList3";
        public string XML_PriceOnPriceList4_TAG = "PriceOnPriceList4";
        public string XML_PriceOnPriceList5_TAG = "PriceOnPriceList5";
        public string XML_PriceOnPriceList6_TAG = "PriceOnPriceList6";
        public string XML_PriceOnPriceList7_TAG = "PriceOnPriceList7";
        public string XML_PriceOnPriceList8_TAG = "PriceOnPriceList8";
        public string XML_PriceOnPriceList9_TAG = "PriceOnPriceList9";
        public string XML_ExtraField1_TAG = "ExtraField1";
        public string XML_ExtraField2_TAG = "ExtraField2";
        public string XML_CreateStockProduct_TAG = "CreateStockProduct";
        public string XML_AvgPrice_TAG = "AvgPrice";
        public string XML_ExtraField3_TAG = "ExtraField3";
        public string XML_IntrastatCode_TAG = "IntrastatCode";

        #endregion

        #region Constructors

        public InvoiceProductIOItem()
        {
        }

        public InvoiceProductIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId, List<PriceListType> pricelists = null)
        {
            CreateObjects(contents, headType, actorCompanyId, pricelists);
        }

        public InvoiceProductIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId, List<PriceListType> pricelists = null)
        {
            CreateObjects(content, headType, actorCompanyId, pricelists);
        }

        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId, List<PriceListType> pricelists = null)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType, actorCompanyId, pricelists);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType, int actorCompanyId, List<PriceListType> pricelists = null)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> accountYearElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(accountYearElements, headType, actorCompanyId, pricelists);

        }

        public void CreateObjects(List<XElement> productElements, TermGroup_IOImportHeadType headType, int actorCompanyId, List<PriceListType> pricelists = null)
        {

            foreach (var productElement in productElements)
            {
                InvoiceProductIODTO invoiceProductIODTO = new InvoiceProductIODTO();

                invoiceProductIODTO.Number = XmlUtil.GetChildElementValue(productElement, XML_Number_TAG);
                invoiceProductIODTO.Name = XmlUtil.GetChildElementValue(productElement, XML_Name_TAG);
                invoiceProductIODTO.Name2 = XmlUtil.GetChildElementValue(productElement, XML_Name2_TAG);
                invoiceProductIODTO.Description = XmlUtil.GetChildElementValue(productElement, XML_Description_TAG);
                invoiceProductIODTO.Unit = XmlUtil.GetChildElementValue(productElement, XML_Unit_TAG);
                invoiceProductIODTO.ShowAsTextRow = XmlUtil.GetChildElementValue(productElement, XML_ShowAsTextRow_TAG) == "1";
                invoiceProductIODTO.EAN = XmlUtil.GetChildElementValue(productElement, XML_EAN_TAG);
                invoiceProductIODTO.Weight = XmlUtil.GetElementDecimalValue(productElement, XML_Weight_TAG);
                invoiceProductIODTO.MaterialCode = XmlUtil.GetChildElementValue(productElement, XML_MaterialCode_TAG);
                invoiceProductIODTO.VatType = XmlUtil.GetElementIntValue(productElement, XML_VatType_TAG);
                //invoiceProductIODTO.PriceType = XmlUtil.GetElementIntValue(productElement, XML_PriceType_TAG);
                invoiceProductIODTO.VatCodeNr = XmlUtil.GetChildElementValue(productElement, XML_VatCodeNr_TAG);
                invoiceProductIODTO.ProductGroupCode = XmlUtil.GetChildElementValue(productElement, XML_ProductGroupCode_TAG);
                invoiceProductIODTO.PurchasePrice = XmlUtil.GetElementDecimalValue(productElement, XML_PurchasePrice_TAG);
                invoiceProductIODTO.SalesPrice = XmlUtil.GetElementDecimalValue(productElement, XML_SalesPrice_TAG);
                //invoiceProductIODTO.PriceListType = XmlUtil.GetElementValue(productElement, XML_PriceListType_TAG);
                invoiceProductIODTO.ClaimAccountNr = XmlUtil.GetChildElementValue(productElement, XML_ClaimAccountNr_TAG);
                invoiceProductIODTO.ClaimAccountDim2Nr = XmlUtil.GetChildElementValue(productElement, XML_ClaimAccountDim2Nr_TAG);
                invoiceProductIODTO.ClaimAccountDim3Nr = XmlUtil.GetChildElementValue(productElement, XML_ClaimAccountDim3Nr_TAG);
                invoiceProductIODTO.ClaimAccountDim4Nr = XmlUtil.GetChildElementValue(productElement, XML_ClaimAccountDim4Nr_TAG);
                invoiceProductIODTO.ClaimAccountDim5Nr = XmlUtil.GetChildElementValue(productElement, XML_ClaimAccountDim5Nr_TAG);
                invoiceProductIODTO.ClaimAccountDim6Nr = XmlUtil.GetChildElementValue(productElement, XML_ClaimAccountDim6Nr_TAG);
                invoiceProductIODTO.ClaimAccountSieDim1 = XmlUtil.GetChildElementValue(productElement, XML_ClaimAccountSieDim1_TAG);
                invoiceProductIODTO.ClaimAccountSieDim6 = XmlUtil.GetChildElementValue(productElement, XML_ClaimAccountSieDim6_TAG);
                invoiceProductIODTO.SalesAccountNr = XmlUtil.GetChildElementValue(productElement, XML_SalesAccountNr_TAG);
                invoiceProductIODTO.SalesAccountDim2Nr = XmlUtil.GetChildElementValue(productElement, XML_SalesAccountDim2Nr_TAG);
                invoiceProductIODTO.SalesAccountDim3Nr = XmlUtil.GetChildElementValue(productElement, XML_SalesAccountDim3Nr_TAG);
                invoiceProductIODTO.SalesAccountDim4Nr = XmlUtil.GetChildElementValue(productElement, XML_SalesAccountDim4Nr_TAG);
                invoiceProductIODTO.SalesAccountDim5Nr = XmlUtil.GetChildElementValue(productElement, XML_SalesAccountDim5Nr_TAG);
                invoiceProductIODTO.SalesAccountDim6Nr = XmlUtil.GetChildElementValue(productElement, XML_SalesAccountDim6Nr_TAG);
                invoiceProductIODTO.SalesAccountSieDim1 = XmlUtil.GetChildElementValue(productElement, XML_SalesAccountSieDim1_TAG);
                invoiceProductIODTO.SalesAccountSieDim6 = XmlUtil.GetChildElementValue(productElement, XML_SalesAccountSieDim6_TAG);
                invoiceProductIODTO.ReversedVatSalesAccountNr = XmlUtil.GetChildElementValue(productElement, XML_ReversedVatSalesAccountNr_TAG);
                invoiceProductIODTO.ReversedVatSalesAccountDim2Nr = XmlUtil.GetChildElementValue(productElement, XML_ReversedVatSalesAccountDim2Nr_TAG);
                invoiceProductIODTO.ReversedVatSalesAccountDim3Nr = XmlUtil.GetChildElementValue(productElement, XML_ReversedVatSalesAccountDim3Nr_TAG);
                invoiceProductIODTO.ReversedVatSalesAccountDim4Nr = XmlUtil.GetChildElementValue(productElement, XML_ReversedVatSalesAccountDim4Nr_TAG);
                invoiceProductIODTO.ReversedVatSalesAccountDim5Nr = XmlUtil.GetChildElementValue(productElement, XML_ReversedVatSalesAccountDim5Nr_TAG);
                invoiceProductIODTO.ReversedVatSalesAccountDim6Nr = XmlUtil.GetChildElementValue(productElement, XML_ReversedVatSalesAccountDim6Nr_TAG);
                invoiceProductIODTO.ReversedVatSalesAccountSieDim1 = XmlUtil.GetChildElementValue(productElement, XML_ReversedVatSalesAccountSieDim1_TAG);
                invoiceProductIODTO.ReversedVatSalesAccountSieDim6 = XmlUtil.GetChildElementValue(productElement, XML_ReversedVatSalesAccountSieDim6_TAG);
                invoiceProductIODTO.VatFreeSalesAccountNr = XmlUtil.GetChildElementValue(productElement, XML_VatFreeSalesAccountNr_TAG);
                invoiceProductIODTO.VatAccountNr = XmlUtil.GetChildElementValue(productElement, XML_VatAccountNr_TAG);
                invoiceProductIODTO.CategoryCode1 = XmlUtil.GetChildElementValue(productElement, XML_CategoryCode1_TAG);
                invoiceProductIODTO.CategoryCode2 = XmlUtil.GetChildElementValue(productElement, XML_CategoryCode2_TAG);
                invoiceProductIODTO.CategoryCode3 = XmlUtil.GetChildElementValue(productElement, XML_CategoryCode3_TAG);
                invoiceProductIODTO.CategoryCode4 = XmlUtil.GetChildElementValue(productElement, XML_CategoryCode4_TAG);
                invoiceProductIODTO.CategoryCode5 = XmlUtil.GetChildElementValue(productElement, XML_CategoryCode5_TAG);
                invoiceProductIODTO.ExtraField1 = XmlUtil.GetChildElementValue(productElement, XML_ExtraField1_TAG);
                invoiceProductIODTO.ExtraField2 = XmlUtil.GetChildElementValue(productElement, XML_ExtraField2_TAG);
                invoiceProductIODTO.ExtraField3 = XmlUtil.GetChildElementValue(productElement, XML_ExtraField3_TAG);
                invoiceProductIODTO.IsStockProduct = XmlUtil.GetChildElementValue(productElement, XML_CreateStockProduct_TAG) == "1";
                invoiceProductIODTO.AvgPriceStockProduct = XmlUtil.GetElementDecimalValue(productElement, XML_AvgPrice_TAG);
                invoiceProductIODTO.IntrastatCode = XmlUtil.GetChildElementValue(productElement, XML_IntrastatCode_TAG);

                #region Prices

                #region Create rows if prices in head

                invoiceProductIODTO.PriceDTOs = new List<InvoiceProductPriceIODTO>();

                for (int i = 1; i < 10; i++)
                {
                    string priceOnPriceList = string.Empty;
                    if (i == 1) priceOnPriceList = XML_PriceOnPriceList1_TAG;
                    if (i == 2) priceOnPriceList = XML_PriceOnPriceList2_TAG;
                    if (i == 3) priceOnPriceList = XML_PriceOnPriceList3_TAG;
                    if (i == 4) priceOnPriceList = XML_PriceOnPriceList4_TAG;
                    if (i == 5) priceOnPriceList = XML_PriceOnPriceList5_TAG;
                    if (i == 6) priceOnPriceList = XML_PriceOnPriceList6_TAG;
                    if (i == 7) priceOnPriceList = XML_PriceOnPriceList7_TAG;
                    if (i == 8) priceOnPriceList = XML_PriceOnPriceList8_TAG;
                    if (i == 9) priceOnPriceList = XML_PriceOnPriceList9_TAG;

                    if (XmlUtil.GetElementDecimalValue(productElement, priceOnPriceList) != 0)
                    {
                        InvoiceProductPriceIODTO priceDTO = new InvoiceProductPriceIODTO();
                        string priceListName = pricelists.FirstOrDefault(p => p.Name.Contains(i.ToString()))?.Name ?? string.Empty;
                        if (!string.IsNullOrEmpty(priceListName))
                        {
                            priceDTO.PriceListCode = priceListName;
                            priceDTO.Price = XmlUtil.GetElementDecimalValue(productElement, priceOnPriceList);
                            invoiceProductIODTO.PriceDTOs.Add(priceDTO);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(XmlUtil.GetChildElementValue(productElement, XML_PriceListType_TAG)) && 
                    XmlUtil.GetElementDecimalValue(productElement, XML_SalesPrice_TAG) != 0)
                {
                    InvoiceProductPriceIODTO priceDTO = new InvoiceProductPriceIODTO();
                    priceDTO.PriceListCode = XmlUtil.GetChildElementValue(productElement, XML_PriceListType_TAG);
                    priceDTO.Price = XmlUtil.GetElementDecimalValue(productElement, XML_SalesPrice_TAG);
                    invoiceProductIODTO.PriceDTOs.Add(priceDTO);
                }

                #endregion

                //TODO: Real Rows in file

                #endregion

                #region Fixes

                if (!string.IsNullOrEmpty(invoiceProductIODTO.Name2))
                    invoiceProductIODTO.Name += "" + invoiceProductIODTO.Name2;

                #endregion

                invoiceProductIOs.Add(invoiceProductIODTO);
            }

        }
        #endregion
    }
}
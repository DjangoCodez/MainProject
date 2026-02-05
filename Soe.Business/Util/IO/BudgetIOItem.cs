using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class BudgetIOItem
    {
        #region Collections

        public List<BudgetHeadIODTO> budgetIOs = new List<BudgetHeadIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "BudgetHeadIO";
        public const string XML_CHILD_TAG = "BudgetRowIO";

        #region Parent

        public string XML_AccountYearStartDate_TAG = "AccountYearStartDate";
        public string XML_AccountYear_TAG = "AccountYear";
        public string XML_AccountYearPeriod_TAG = "AccountYearPeriod";
        public string XML_DistributionCodeHeadName_TAG = "DistributionCodeHeadName";
        public string XML_NoOfPeriods_TAG = "NoOfPeriods";
        public string XML_Status_TAG = "Status";
        public string XML_Type_TAG = "Type";
        public string XML_Name_TAG = "Name";
        public string XML_DimNr2_TAG = "DimNr2";
        public string XML_DimNr3_TAG = "DimNr3";
        public string XML_UseDimNr2_TAG = "UseDimNr2";
        public string XML_UseDimNr3_TAG = "UseDimNr3";

        #endregion

        #region Child Nodes

        public string XML_CHILD_AccountYearStartDate_TAG = "AccountYearStartDate";
        public string XML_CHILD_NoOfPeriods_TAG = "NoOfPeriods";
        public string XML_CHILD_Status_TAG = "Status";
        public string XML_CHILD_Type_TAG = "Type";
        public string XML_CHILD_Name_TAG = "Name";
        public string XML_CHILD_DimNr2_TAG = "DimNr2";
        public string XML_CHILD_DimNr3_TAG = "DimNr3";
        public string XML_CHILD_AccountYear_TAG = "AccountYear";
        public string XML_CHILD_AccountYearPeriod_TAG = "AccountYearPeriod";
        public string XML_CHILD_BudgetRowPeriodType_TAG = "BudgetRowPeriodType";
        public string XML_CHILD_ShiftTypeCode_TAG = "ShiftTypeCode";
        public string XML_CHILD_PeriodNr_TAG = "PeriodNr";
        public string XML_CHILD_PeriodAmount_TAG = "PeriodAmount";
        public string XML_CHILD_PeriodQuantity_TAG = "PeriodQuantity";
        public string XML_CHILD_BudgetRowNr_TAG = "BudgetRowNr";
        public string XML_CHILD_AccountNr_TAG = "AccountNr";
        public string XML_CHILD_AccountDIm2Nr_TAG = "AccountDIm2Nr";
        public string XML_CHILD_AccountDIm3Nr_TAG = "AccountDIm3Nr";
        public string XML_CHILD_AccountDIm4Nr_TAG = "AccountDIm4Nr";
        public string XML_CHILD_AccountDIm5Nr_TAG = "AccountDIm5Nr";
        public string XML_CHILD_AccountDIm6Nr_TAG = "AccountDIm6Nr";
        public string XML_CHILD_AccountSieDim1_TAG = "AccountSieDim1";
        public string XML_CHILD_AccountSieDim6_TAG = "AccountSieDim6";
        public string XML_CHILD_DistributionCodeHeadName_TAG = "DistributionCodeHeadName";
        public string XML_CHILD_TotalAmount_TAG = "TotalAmount";
        public string XML_CHILD_TotalQuantity_TAG = "TotalQuantity";
        public string XML_CHILD_Period1Amount_TAG = "Period1Amount";
        public string XML_CHILD_Period2Amount_TAG = "Period2Amount";
        public string XML_CHILD_Period3Amount_TAG = "Period3Amount";
        public string XML_CHILD_Period4Amount_TAG = "Period4Amount";
        public string XML_CHILD_Period5Amount_TAG = "Period5Amount";
        public string XML_CHILD_Period6Amount_TAG = "Period6Amount";
        public string XML_CHILD_Period7Amount_TAG = "Period7Amount";
        public string XML_CHILD_Period8Amount_TAG = "Period8Amount";
        public string XML_CHILD_Period9Amount_TAG = "Period9Amount";
        public string XML_CHILD_Period10Amount_TAG = "Period10Amount";
        public string XML_CHILD_Period11Amount_TAG = "Period11Amount";
        public string XML_CHILD_Period12Amount_TAG = "Period12Amount";
        public string XML_CHILD_Period13Amount_TAG = "Period13Amount";
        public string XML_CHILD_Period14Amount_TAG = "Period14Amount";
        public string XML_CHILD_Period15Amount_TAG = "Period15Amount";
        public string XML_CHILD_Period16Amount_TAG = "Period16Amount";
        public string XML_CHILD_Period17Amount_TAG = "Period17Amount";
        public string XML_CHILD_Period18Amount_TAG = "Period18Amount";
        public string XML_CHILD_Period19Amount_TAG = "Period19Amount";
        public string XML_CHILD_Period20Amount_TAG = "Period20Amount";
        public string XML_CHILD_Period21Amount_TAG = "Period21Amount";
        public string XML_CHILD_Period22Amount_TAG = "Period22Amount";
        public string XML_CHILD_Period23Amount_TAG = "Period23Amount";
        public string XML_CHILD_Period24Amount_TAG = "Period24Amount";
        public string XML_CHILD_Period1Quantity_TAG = "Period1Quantity";
        public string XML_CHILD_Period2Quantity_TAG = "Period2Quantity";
        public string XML_CHILD_Period3Quantity_TAG = "Period3Quantity";
        public string XML_CHILD_Period4Quantity_TAG = "Period4Quantity";
        public string XML_CHILD_Period5Quantity_TAG = "Period5Quantity";
        public string XML_CHILD_Period6Quantity_TAG = "Period6Quantity";
        public string XML_CHILD_Period7Quantity_TAG = "Period7Quantity";
        public string XML_CHILD_Period8Quantity_TAG = "Period8Quantity";
        public string XML_CHILD_Period9Quantity_TAG = "Period9Quantity";
        public string XML_CHILD_Period10Quantity_TAG = "Period10Quantity";
        public string XML_CHILD_Period11Quantity_TAG = "Period11Quantity";
        public string XML_CHILD_Period12Quantity_TAG = "Period12Quantity";
        public string XML_CHILD_Period13Quantity_TAG = "Period13Quantity";
        public string XML_CHILD_Period14Quantity_TAG = "Period14Quantity";
        public string XML_CHILD_Period15Quantity_TAG = "Period15Quantity";
        public string XML_CHILD_Period16Quantity_TAG = "Period16Quantity";
        public string XML_CHILD_Period17Quantity_TAG = "Period17Quantity";
        public string XML_CHILD_Period18Quantity_TAG = "Period18Quantity";
        public string XML_CHILD_Period19Quantity_TAG = "Period19Quantity";
        public string XML_CHILD_Period20Quantity_TAG = "Period20Quantity";
        public string XML_CHILD_Period21Quantity_TAG = "Period21Quantity";
        public string XML_CHILD_Period22Quantity_TAG = "Period22Quantity";
        public string XML_CHILD_Period23Quantity_TAG = "Period23Quantity";
        public string XML_CHILD_Period24Quantity_TAG = "Period24Quantity";

        #endregion

        #endregion

        #region Constructors

        public BudgetIOItem()
        {
        }

        public BudgetIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId, List<dynamic> objects = null, int? accountYearId = null)
        {
            if (objects == null)
            {
                CreateObjects(contents, headType, actorCompanyId, accountYearId);
            }
            else
            {
                foreach (var item in objects)
                {
                    this.budgetIOs.Add(item as BudgetHeadIODTO);
                }
            }
        }

        public BudgetIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(content, headType, actorCompanyId);
        }

        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId, int? accountYearId = null)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType, actorCompanyId, accountYearId);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType, int actorCompanyId, int? accountYearId = null)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> elementBudgets = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            if (elementBudgets.Count == 0)
            {
                XElement elementBudget = new XElement("BudgetHeadIO");
                List<XElement> children = XmlUtil.GetChildElements(xdoc, XML_CHILD_TAG);
                elementBudget.Add(children);
                elementBudgets.Add(elementBudget);
            }

            CreateObjects(elementBudgets, headType, actorCompanyId, accountYearId);
        }


        public void CreateObjects(List<XElement> elementBudgets, TermGroup_IOImportHeadType headType, int actorCompanyId, int? accountYearId = null)
        {
            #region prereq

            var accountManager = new AccountManager(null);
            var accountYears = accountManager.GetAccountYears(actorCompanyId, false, false);

            #endregion
            foreach (var elementBudget in elementBudgets)
            {
                BudgetHeadIODTO headDTO = new BudgetHeadIODTO();
                List<BudgetRowIODTO> rowDTOs = new List<BudgetRowIODTO>();

                DateTime? startDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(elementBudget, XML_AccountYearStartDate_TAG));

                AccountYear accountYear = null; 
                if (accountYearId.HasValue)
                {
                    accountYear = accountManager.GetAccountYear(accountYearId.Value);
                    if (accountYear != null)
                        headDTO.AccountYearStartDate = accountYear.From;
                }

                if(accountYear == null)
                {
                    headDTO.AccountYearStartDate = startDate.HasValue ? startDate.Value : CalendarUtility.DATETIME_DEFAULT;
                    if (headDTO.AccountYearStartDate == CalendarUtility.DATETIME_DEFAULT && !string.IsNullOrEmpty(XmlUtil.GetChildElementValue(elementBudget, XML_AccountYear_TAG)) && !string.IsNullOrEmpty(XmlUtil.GetChildElementValue(elementBudget, XML_AccountYearPeriod_TAG)))
                        headDTO.AccountYearStartDate = new DateTime(XmlUtil.GetElementIntValue(elementBudget, XML_AccountYear_TAG), XmlUtil.GetElementIntValue(elementBudget, XML_AccountYearPeriod_TAG), 1);
                }

                headDTO.DistributionCodeHeadName = XmlUtil.GetChildElementValue(elementBudget, XML_DistributionCodeHeadName_TAG);
                headDTO.NoOfPeriods = XmlUtil.GetElementIntValue(elementBudget, XML_NoOfPeriods_TAG);
                headDTO.Status = XmlUtil.GetElementIntValue(elementBudget, XML_Status_TAG);
                headDTO.Type = XmlUtil.GetElementIntValue(elementBudget, XML_Type_TAG);
                headDTO.Name = XmlUtil.GetChildElementValue(elementBudget, XML_Name_TAG);
                headDTO.DimNr2 = XmlUtil.GetElementIntValue(elementBudget, XML_DimNr2_TAG);
                headDTO.DimNr3 = XmlUtil.GetElementIntValue(elementBudget, XML_DimNr3_TAG);
                headDTO.UseDimNr2 = XmlUtil.GetElementBoolValue(elementBudget, XML_UseDimNr2_TAG);
                headDTO.UseDimNr3 = XmlUtil.GetElementBoolValue(elementBudget, XML_UseDimNr3_TAG);

                bool headIsEmpty = string.IsNullOrEmpty(headDTO.Name);
                bool oneRowisOnePeriod = false;

                List<XElement> rowElements = XmlUtil.GetChildElements(elementBudget, XML_CHILD_TAG);

                foreach (var rowElement in rowElements)
                {
                    BudgetRowIODTO rowDTO = new BudgetRowIODTO();

                    if (headIsEmpty)
                    {
                        if (accountYear != null)
                        {
                            rowDTO.AccountYearStartDate = accountYear.From;
                        }
                        else
                        {
                            DateTime? startDateChild = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountYearStartDate_TAG));
                            rowDTO.AccountYearStartDate = startDateChild.HasValue ? startDateChild.Value : CalendarUtility.DATETIME_DEFAULT;
                            if (rowDTO.AccountYearStartDate == CalendarUtility.DATETIME_DEFAULT && !string.IsNullOrEmpty(XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountYear_TAG)) && !string.IsNullOrEmpty(XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountYearPeriod_TAG)))
                                rowDTO.AccountYearStartDate = new DateTime(XmlUtil.GetElementIntValue(rowElement, XML_CHILD_AccountYear_TAG), XmlUtil.GetElementIntValue(rowElement, XML_CHILD_AccountYearPeriod_TAG), 1);

                            // Extra year check to match "broken financial year".
                            var accYear = accountYears.FirstOrDefault(a => rowDTO.AccountYearStartDate >= a.From && rowDTO.AccountYearStartDate <= a.To);
                            rowDTO.AccountYearId = accYear != null ? accYear.AccountYearId : 0;
                        }

                        rowDTO.NoOfPeriods = XmlUtil.GetElementIntValue(rowElement, XML_CHILD_NoOfPeriods_TAG);
                        rowDTO.Status = XmlUtil.GetElementIntValue(rowElement, XML_CHILD_Status_TAG);
                        rowDTO.Type = XmlUtil.GetElementIntValue(rowElement, XML_CHILD_Type_TAG);
                        rowDTO.Name = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_Name_TAG);
                        rowDTO.DimNr2 = XmlUtil.GetElementIntValue(rowElement, XML_CHILD_DimNr2_TAG);
                        rowDTO.DimNr3 = XmlUtil.GetElementIntValue(rowElement, XML_CHILD_DimNr3_TAG);
                    }

                    oneRowisOnePeriod = string.IsNullOrEmpty(XmlUtil.GetChildElementValue(elementBudget, XML_CHILD_PeriodNr_TAG));

                    if (oneRowisOnePeriod)
                    {
                        rowDTO.PeriodNr = XmlUtil.GetElementIntValue(rowElement, XML_CHILD_PeriodNr_TAG);
                        rowDTO.PeriodAmount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_PeriodAmount_TAG);
                        rowDTO.PeriodQuantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_PeriodQuantity_TAG);
                    }

                    rowDTO.BudgetRowNr = XmlUtil.GetElementIntValue(rowElement, XML_CHILD_BudgetRowNr_TAG);
                    rowDTO.AccountNr = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountNr_TAG);
                    rowDTO.AccountDIm2Nr = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountDIm2Nr_TAG);
                    rowDTO.AccountDIm3Nr = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountDIm3Nr_TAG);
                    rowDTO.AccountDIm4Nr = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountDIm4Nr_TAG);
                    rowDTO.AccountDIm5Nr = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountDIm5Nr_TAG);
                    rowDTO.AccountDIm6Nr = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountDIm6Nr_TAG);
                    rowDTO.AccountSieDim1 = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountSieDim1_TAG);
                    rowDTO.AccountSieDim6 = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_AccountSieDim6_TAG);
                    rowDTO.DistributionCodeHeadName = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_DistributionCodeHeadName_TAG);
                    rowDTO.TotalAmount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_TotalAmount_TAG);
                    rowDTO.TotalQuantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_TotalQuantity_TAG);
                    rowDTO.TotalAmount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_TotalAmount_TAG);
                    rowDTO.ShiftTypeCode = XmlUtil.GetChildElementValue(rowElement, XML_CHILD_TotalQuantity_TAG);

                    rowDTO.Period1Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period1Amount_TAG);
                    rowDTO.Period2Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period2Amount_TAG);
                    rowDTO.Period3Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period3Amount_TAG);
                    rowDTO.Period4Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period4Amount_TAG);
                    rowDTO.Period5Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period5Amount_TAG);
                    rowDTO.Period6Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period6Amount_TAG);
                    rowDTO.Period7Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period7Amount_TAG);
                    rowDTO.Period8Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period8Amount_TAG);
                    rowDTO.Period9Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period9Amount_TAG);
                    rowDTO.Period10Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period10Amount_TAG);
                    rowDTO.Period11Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period11Amount_TAG);
                    rowDTO.Period12Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period12Amount_TAG);
                    rowDTO.Period13Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period13Amount_TAG);
                    rowDTO.Period14Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period14Amount_TAG);
                    rowDTO.Period15Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period15Amount_TAG);
                    rowDTO.Period16Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period16Amount_TAG);
                    rowDTO.Period17Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period17Amount_TAG);
                    rowDTO.Period18Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period18Amount_TAG);
                    rowDTO.Period19Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period19Amount_TAG);
                    rowDTO.Period20Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period20Amount_TAG);
                    rowDTO.Period21Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period21Amount_TAG);
                    rowDTO.Period22Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period22Amount_TAG);
                    rowDTO.Period23Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period23Amount_TAG);
                    rowDTO.Period24Amount = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period24Amount_TAG);
                    rowDTO.Period1Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period1Quantity_TAG);
                    rowDTO.Period2Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period2Quantity_TAG);
                    rowDTO.Period3Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period3Quantity_TAG);
                    rowDTO.Period4Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period4Quantity_TAG);
                    rowDTO.Period5Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period5Quantity_TAG);
                    rowDTO.Period6Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period6Quantity_TAG);
                    rowDTO.Period7Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period7Quantity_TAG);
                    rowDTO.Period8Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period8Quantity_TAG);
                    rowDTO.Period9Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period9Quantity_TAG);
                    rowDTO.Period10Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period10Quantity_TAG);
                    rowDTO.Period11Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period11Quantity_TAG);
                    rowDTO.Period12Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period12Quantity_TAG);
                    rowDTO.Period13Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period13Quantity_TAG);
                    rowDTO.Period14Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period14Quantity_TAG);
                    rowDTO.Period15Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period15Quantity_TAG);
                    rowDTO.Period16Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period16Quantity_TAG);
                    rowDTO.Period17Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period17Quantity_TAG);
                    rowDTO.Period18Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period18Quantity_TAG);
                    rowDTO.Period19Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period19Quantity_TAG);
                    rowDTO.Period20Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period20Quantity_TAG);
                    rowDTO.Period21Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period21Quantity_TAG);
                    rowDTO.Period22Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period22Quantity_TAG);
                    rowDTO.Period23Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period23Quantity_TAG);
                    rowDTO.Period24Quantity = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Period24Quantity_TAG);
                    rowDTO.AccountString = rowDTO.AccountNr + ";" + rowDTO.AccountDIm2Nr + ";" + rowDTO.AccountDIm3Nr + ";" + rowDTO.AccountDIm4Nr + ";" + rowDTO.AccountDIm5Nr + ";" + rowDTO.AccountDIm6Nr + rowDTO.AccountSieDim1 + rowDTO.AccountSieDim6;

                    if (headIsEmpty)
                        rowDTOs.Add(rowDTO);
                    else
                        headDTO.Rows.Add(rowDTO);
                }

                if (rowDTOs.Count > 0)
                {
                    //Only rows, need to agregate head

                    var groupedonYear = rowDTOs.GroupBy(r => r.AccountYearId);  //rowDTOs.GroupBy(r => r.AccountYearStartDate.Year);

                    foreach (var rowDTO in groupedonYear)
                    {
                        var groupedonName = rowDTO.GroupBy(r => r.Name);

                        foreach (var rows in groupedonName)
                        {
                            var row = rows.FirstOrDefault();
                            BudgetHeadIODTO newHeadDTO = new BudgetHeadIODTO();

                            newHeadDTO.AccountYearStartDate = row.AccountYearStartDate;
                            newHeadDTO.DistributionCodeHeadName = row.DistributionCodeHeadName;
                            newHeadDTO.NoOfPeriods = row.NoOfPeriods;
                            newHeadDTO.Status = row.Status;
                            newHeadDTO.Type = row.Type;
                            newHeadDTO.Name = row.Name;
                            newHeadDTO.DimNr2 = row.DimNr2;
                            newHeadDTO.DimNr3 = row.DimNr3;

                            if (!oneRowisOnePeriod)
                            {
                                foreach (var budgetRow in rows)
                                {
                                    if (newHeadDTO.Rows == null)
                                        newHeadDTO.Rows = new List<BudgetRowIODTO>();
                                    newHeadDTO.Rows.Add(budgetRow);
                                }
                            }
                            else
                            {
                                var groupedOnAccountString = rows.GroupBy(r => r.AccountString);

                                foreach (var groupedOnAccountStringPeriods in groupedOnAccountString)
                                {
                                    BudgetRowIODTO newRowDTO = new BudgetRowIODTO();
                                    int counter = 0;
                                    decimal totalAmount = 0;
                                    foreach (var period in groupedOnAccountStringPeriods)
                                    {
                                        if (counter == 0)
                                            newRowDTO = period.CloneDTO();
                                        switch (period.PeriodNr)
                                        {
                                            case 1:
                                                newRowDTO.Period1Amount = period.PeriodAmount;
                                                newRowDTO.Period1Quantity = period.PeriodQuantity;
                                                break;
                                            case 2:
                                                newRowDTO.Period2Amount = period.PeriodAmount;
                                                newRowDTO.Period2Quantity = period.PeriodQuantity;
                                                break;
                                            case 3:
                                                newRowDTO.Period3Amount = period.PeriodAmount;
                                                newRowDTO.Period3Quantity = period.PeriodQuantity;
                                                break;
                                            case 4:
                                                newRowDTO.Period4Amount = period.PeriodAmount;
                                                newRowDTO.Period4Quantity = period.PeriodQuantity;
                                                break;
                                            case 5:
                                                newRowDTO.Period5Amount = period.PeriodAmount;
                                                newRowDTO.Period5Quantity = period.PeriodQuantity;
                                                break;
                                            case 6:
                                                newRowDTO.Period6Amount = period.PeriodAmount;
                                                newRowDTO.Period6Quantity = period.PeriodQuantity;
                                                break;
                                            case 7:
                                                newRowDTO.Period7Amount = period.PeriodAmount;
                                                newRowDTO.Period7Quantity = period.PeriodQuantity;
                                                break;
                                            case 8:
                                                newRowDTO.Period8Amount = period.PeriodAmount;
                                                newRowDTO.Period8Quantity = period.PeriodQuantity;
                                                break;
                                            case 9:
                                                newRowDTO.Period9Amount = period.PeriodAmount;
                                                newRowDTO.Period9Quantity = period.PeriodQuantity;
                                                break;
                                            case 10:
                                                newRowDTO.Period10Amount = period.PeriodAmount;
                                                newRowDTO.Period10Quantity = period.PeriodQuantity;
                                                break;
                                            case 11:
                                                newRowDTO.Period11Amount = period.PeriodAmount;
                                                newRowDTO.Period11Quantity = period.PeriodQuantity;
                                                break;
                                            case 12:
                                                newRowDTO.Period12Amount = period.PeriodAmount;
                                                newRowDTO.Period12Quantity = period.PeriodQuantity;
                                                break;
                                            case 13:
                                                newRowDTO.Period13Amount = period.PeriodAmount;
                                                newRowDTO.Period13Quantity = period.PeriodQuantity;
                                                break;
                                            case 14:
                                                newRowDTO.Period14Amount = period.PeriodAmount;
                                                newRowDTO.Period14Quantity = period.PeriodQuantity;
                                                break;
                                            case 15:
                                                newRowDTO.Period15Amount = period.PeriodAmount;
                                                newRowDTO.Period15Quantity = period.PeriodQuantity;
                                                break;
                                            case 16:
                                                newRowDTO.Period16Amount = period.PeriodAmount;
                                                newRowDTO.Period16Quantity = period.PeriodQuantity;
                                                break;
                                            case 17:
                                                newRowDTO.Period17Amount = period.PeriodAmount;
                                                newRowDTO.Period17Quantity = period.PeriodQuantity;
                                                break;
                                            case 18:
                                                newRowDTO.Period18Amount = period.PeriodAmount;
                                                newRowDTO.Period18Quantity = period.PeriodQuantity;
                                                break;
                                            case 19:
                                                newRowDTO.Period19Amount = period.PeriodAmount;
                                                newRowDTO.Period19Quantity = period.PeriodQuantity;
                                                break;
                                            case 20:
                                                newRowDTO.Period20Amount = period.PeriodAmount;
                                                newRowDTO.Period20Quantity = period.PeriodQuantity;
                                                break;
                                            case 21:
                                                newRowDTO.Period21Amount = period.PeriodAmount;
                                                newRowDTO.Period21Quantity = period.PeriodQuantity;
                                                break;
                                            case 22:
                                                newRowDTO.Period22Amount = period.PeriodAmount;
                                                newRowDTO.Period22Quantity = period.PeriodQuantity;
                                                break;
                                            case 23:
                                                newRowDTO.Period23Amount = period.PeriodAmount;
                                                newRowDTO.Period23Quantity = period.PeriodQuantity;
                                                break;
                                            case 24:
                                                newRowDTO.Period24Amount = period.PeriodAmount;
                                                newRowDTO.Period24Quantity = period.PeriodQuantity;
                                                break;
                                        }

                                        // Sum total
                                        totalAmount += period.PeriodAmount;

                                        //if (period.PeriodNr == 1) { newRowDTO.Period1Amount = period.PeriodAmount; newRowDTO.Period1Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 2) { newRowDTO.Period2Amount = period.PeriodAmount; newRowDTO.Period2Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 3) { newRowDTO.Period3Amount = period.PeriodAmount; newRowDTO.Period3Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 4) { newRowDTO.Period4Amount = period.PeriodAmount; newRowDTO.Period4Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 5) { newRowDTO.Period5Amount = period.PeriodAmount; newRowDTO.Period5Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 6) { newRowDTO.Period6Amount = period.PeriodAmount; newRowDTO.Period6Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 7) { newRowDTO.Period7Amount = period.PeriodAmount; newRowDTO.Period7Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 8) { newRowDTO.Period8Amount = period.PeriodAmount; newRowDTO.Period8Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 9) { newRowDTO.Period9Amount = period.PeriodAmount; newRowDTO.Period9Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 10) { newRowDTO.Period10Amount = period.PeriodAmount; newRowDTO.Period10Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 11) { newRowDTO.Period11Amount = period.PeriodAmount; newRowDTO.Period11Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 12) { newRowDTO.Period12Amount = period.PeriodAmount; newRowDTO.Period12Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 13) { newRowDTO.Period13Amount = period.PeriodAmount; newRowDTO.Period13Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 14) { newRowDTO.Period14Amount = period.PeriodAmount; newRowDTO.Period14Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 15) { newRowDTO.Period15Amount = period.PeriodAmount; newRowDTO.Period15Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 16) { newRowDTO.Period16Amount = period.PeriodAmount; newRowDTO.Period16Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 17) { newRowDTO.Period17Amount = period.PeriodAmount; newRowDTO.Period17Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 18) { newRowDTO.Period18Amount = period.PeriodAmount; newRowDTO.Period18Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 19) { newRowDTO.Period19Amount = period.PeriodAmount; newRowDTO.Period19Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 20) { newRowDTO.Period20Amount = period.PeriodAmount; newRowDTO.Period20Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 21) { newRowDTO.Period21Amount = period.PeriodAmount; newRowDTO.Period21Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 22) { newRowDTO.Period22Amount = period.PeriodAmount; newRowDTO.Period22Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 23) { newRowDTO.Period23Amount = period.PeriodAmount; newRowDTO.Period23Quantity = period.PeriodQuantity; }
                                        //else if (period.PeriodNr == 24) { newRowDTO.Period24Amount = period.PeriodAmount; newRowDTO.Period24Quantity = period.PeriodQuantity; }  

                                        counter++;

                                    }

                                    if (newHeadDTO.Rows == null)
                                        newHeadDTO.Rows = new List<BudgetRowIODTO>();

                                    // Set total if missing
                                    if (newRowDTO.TotalAmount == 0 && totalAmount != 0)
                                        newRowDTO.TotalAmount = totalAmount;

                                    newHeadDTO.Rows.Add(newRowDTO);
                                }
                            }

                            budgetIOs.Add(newHeadDTO);
                        }
                    }
                }


            }

        }
        #endregion
    }
}
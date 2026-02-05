using SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Economy
{
    public class InventoryReportData : EconomyReportDataManager, IReportDataModel
    {
        private readonly InventoryReportDataOutput _reportDataOutput;

        public InventoryReportData(ParameterObject parameterObject, InventoryReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new InventoryReportDataOutput(reportDataInput);
        }

        public InventoryReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeInactive, "includeInactive");
            TryGetDatesFromSelection(reportResult, out DateTime selectionFromDate, out DateTime selectionToDate);
            using (CompEntities entities = new CompEntities())
            {
                DateTime yearStart;
                int accountYearId = 0;
                bool yearIsOpen = false;

                var accountYear = AccountManager.GetAccountYear(selectionFromDate, base.ActorCompanyId);
                if (accountYear != null)
                {
                    AccountManager.GetAccountYearInfo(accountYear, out accountYearId, out yearIsOpen);
                    yearStart = new DateTime(selectionFromDate.Year, accountYear.From.Month, 1);
                }
                else
                {
                    yearStart = CalendarUtility.GetFirstDateOfYear(selectionFromDate);
                }

                if (yearStart > selectionFromDate)
                    yearStart = yearStart.AddYears(-1);

                List<Inventory> inventories = InventoryManager.GetInventoriesForAnalysis(base.ActorCompanyId,selectionFromDate,selectionToDate,!selectionIncludeInactive);
                var accountDistributionTotals = InventoryManager.GetAccountDistributionTotals(base.ActorCompanyId, yearStart, selectionFromDate, selectionToDate);
                foreach (Inventory inventory in inventories)
                {
                    
                    decimal totalWriteOff = InventoryManager.GetAccountDistributionWriteOffTotal(accountDistributionTotals,inventory.InventoryId);

                    InventoryItem inventoryItem = new InventoryItem()
                    {
                        InventoryNumber = inventory.InventoryNr,
                        InventoryName = inventory.Name,
                        InventoryNumberName =string.Format("{0} {1}", inventory.InventoryNr, inventory.Name),
                        InventoryStatus = inventory.StatusName,
                        InventoryDescription = inventory.Description,
                        InventoryAccount = inventory.InventoryAccountNr +" "+ inventory.InventoryAccountName,
                        AcquisitionDate = inventory.PurchaseDate,
                        AcquisitionValue = inventory.PurchaseAmount,
                        DepreciationValue = inventory.WriteOffAmount,
                        AcquisitionsForThePeriod = (selectionToDate > inventory.PurchaseDate || (selectionFromDate > inventory.PurchaseDate && inventory.WriteOffRemainingAmount == 0)) ? inventory.PurchaseAmount : 0,
                        BookValue = inventory.WriteOffAmount - (totalWriteOff + inventory.WriteOffSum),
                        DepreciationForThePeriod = totalWriteOff,
                        Disposals = (inventory.Status == (int)TermGroup_InventoryStatus.Discarded) ? totalWriteOff : 0,
                        Scrapped = (inventory.Status == (int)TermGroup_InventoryStatus.Sold) ? totalWriteOff : 0,
                        AccumulatedDepreciationTotal = totalWriteOff + inventory.WriteOffSum,
                        DepriciationMethod = inventory.InventoryWriteOffMethod?.Name,
                        InventoryCategories = inventory.CategoryNamesString,
                    };

                    _reportDataOutput.InventoryItems.Add(inventoryItem);

                }


            }
            return new ActionResult();
        }
    }

    public class InventoryReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_InventoryMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public InventoryReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_InventoryMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_InventoryMatrixColumns.Unknown;
        }
    }

    public class InventoryReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<InventoryReportDataReportDataField> Columns { get; set; }

        public InventoryReportDataInput(CreateReportResult reportResult, List<InventoryReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class InventoryReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<InventoryItem> InventoryItems { get; set; }
        public InventoryReportDataInput Input { get; set; }

        public InventoryReportDataOutput(InventoryReportDataInput input)
        {
            this.InventoryItems = new List<InventoryItem>();
            this.Input = input;
        }
    }
}

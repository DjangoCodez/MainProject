using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class PayrollProductsReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly PayrollProductsReportDataInput _reportDataInput;
        private readonly PayrollProductsReportDataOutput _reportDataOutput;

        private bool LoadSettings
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_PayrollProductsMatrixColumns.Payrollgroup ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.CentroundingType ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.CentroundingLevel ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.TaxCalculationType ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.PensionCompany ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.TimeUnit ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.QuantityRoundingType ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.ChildProduct ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.QuantityRoundingMinutes ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.PrintOnSalaryspecification ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.DontPrintOnSalarySpecificationWhenZeroAmount ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.ShowPrintDate ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.DontIncludeInRetroactivePayroll ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.VacationSalaryPromoted ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.UnionFeePromoted ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.WorkingTimePromoted ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.CalculateSupplementCharge ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.CalculateSicknessSalary ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.Payrollpricetypes ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.Payrollpriceformulas ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.AccountingPurchase ||
                        a.Column == TermGroup_PayrollProductsMatrixColumns.AccountingPrioName);
            }
        }

        public PayrollProductsReportData(ParameterObject parameterObject, PayrollProductsReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new PayrollProductsReportDataOutput(reportDataInput);
            _reportDataInput = reportDataInput;
        }

        public static List<PayrollProductsReportDataField> GetPossibleDataFields()
        {
            List<PayrollProductsReportDataField> possibleFields = new List<PayrollProductsReportDataField>();
            EnumUtility.GetValues<TermGroup_PayrollProductsMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new PayrollProductsReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public PayrollProductsReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            #region Prereq

            TryGetPayrollProductIdsFromSelections(reportResult, out List<int> selectionPayrollProductIds);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Terms and dictionaries

                int langId = GetLangId();

                Dictionary<int, string> resultTypes = base.GetTermGroupDict(TermGroup.PayrollPriceFormulaResultType, langId);
                Dictionary<int, string> payrollProductCentRoundingTypes = null;
                Dictionary<int, string> payrollProductCentRoundingLevels = null;
                Dictionary<int, string> pensionCompanys = null;
                Dictionary<int, string> payrollProductTimeUnits = null;
                Dictionary<int, string> taxCalc = null;
                Dictionary<int, string> quantityRoundingTypes = null;
                Dictionary<int, string> payrollProductAccountingPrio = null;

                if (LoadSettings)
                {
                    payrollProductCentRoundingTypes = base.GetTermGroupDict(TermGroup.PayrollProductCentRoundingType, langId);
                    payrollProductCentRoundingLevels = base.GetTermGroupDict(TermGroup.PayrollProductCentRoundingLevel, langId);
                    pensionCompanys = base.GetTermGroupDict(TermGroup.PensionCompany, langId);
                    payrollProductTimeUnits = base.GetTermGroupDict(TermGroup.PayrollProductTimeUnit, langId);
                    taxCalc = base.GetTermGroupDict(TermGroup.PayrollProductTaxCalculationType, langId);
                    quantityRoundingTypes = base.GetTermGroupDict(TermGroup.QuantityRoundingType, langId);
                    payrollProductAccountingPrio = base.GetTermGroupDict(TermGroup.PayrollProductAccountingPrio, langId);

                }
                #endregion

                #region Content
                List<PayrollProductDTO> payrollProducts = ProductManager.GetPayrollProducts(reportResult.Input.ActorCompanyId, true, true, true, true).ToDTOs(true, true, true, true, true).ToList();
                if(!selectionPayrollProductIds.IsNullOrEmpty())
                    payrollProducts = payrollProducts.Where(w => selectionPayrollProductIds.Contains(w.ProductId)).ToList();

                AccountDim accountDimStd = null;
                List<AccountDim> accountDimInternals = null;
                List<PayrollGroup> payrollGroups = null;
                List<PayrollPriceFormula> priceFormulas = null;
                List<PayrollPriceTypeSmallDTO> payrollPricetypes = null;

                if (LoadSettings)
                {
                    accountDimStd = AccountManager.GetAccountDimStd(base.ActorCompanyId);
                    accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(base.ActorCompanyId, true);
                    payrollGroups = PayrollManager.GetPayrollGroups(base.ActorCompanyId, onlyActive: false);
                    priceFormulas = PayrollManager.GetPayrollPriceFormulas(base.ActorCompanyId);
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    payrollPricetypes = PayrollManager.GetPayrollPriceTypesSmall(entitiesReadOnly, base.ActorCompanyId, false);
                }

                List<SelectablePayrollTypeDTO> sysPayrollTypes = ReportManager.GetReportPayrollTypes(ActorCompanyId);

                foreach (var product in payrollProducts)
                {
                    #region Item

                    if (LoadSettings)
                    {
                        foreach(var productSettings in product.Settings)
                        {
                            PayrollProductsItem payrollProductsItem = new PayrollProductsItem();

                            #region Product
                            payrollProductsItem.PayrollProductId = product.ProductId;
                            payrollProductsItem.Number = product.Number;
                            payrollProductsItem.Name = product.Name;
                            payrollProductsItem.ShortName = product.ShortName;
                            payrollProductsItem.ExternalNumber = product.ExternalNumber;
                            payrollProductsItem.syspayrolltypelevel1 = sysPayrollTypes.FirstOrDefault(w => w.Id == product.SysPayrollTypeLevel1)?.Name ?? string.Empty;
                            payrollProductsItem.syspayrolltypelevel2 = sysPayrollTypes.FirstOrDefault(w => w.Id == product.SysPayrollTypeLevel2)?.Name ?? string.Empty;
                            payrollProductsItem.syspayrolltypelevel3 = sysPayrollTypes.FirstOrDefault(w => w.Id == product.SysPayrollTypeLevel3)?.Name ?? string.Empty;
                            payrollProductsItem.syspayrolltypelevel4 = sysPayrollTypes.FirstOrDefault(w => w.Id == product.SysPayrollTypeLevel4)?.Name ?? string.Empty;
                            payrollProductsItem.ProductFactor = product.Factor;
                            payrollProductsItem.ResultType = GetValueFromDict(product.ResultType, resultTypes);
                            payrollProductsItem.PayrollProductPayed = product.Payed;
                            payrollProductsItem.ExcludeInWorkTimeSummary = product.ExcludeInWorkTimeSummary;
                            payrollProductsItem.AverageCalculated = product.AverageCalculated;
                            payrollProductsItem.UseInPayroll = product.UseInPayroll;
                            payrollProductsItem.DontUseFixedAccounting = product.DontUseFixedAccounting;
                            payrollProductsItem.ProductExport = product.Export;
                            payrollProductsItem.IncludeAmountInExport = product.IncludeAmountInExport;

                            #endregion

                            #region Settings

                            payrollProductsItem.Payrollgroup = payrollGroups.FirstOrDefault(w => w.PayrollGroupId == productSettings.PayrollGroupId)?.Name ?? GetText(4366, "Alla");
                            payrollProductsItem.CentroundingType = GetValueFromDict(productSettings.CentRoundingType, payrollProductCentRoundingTypes);
                            payrollProductsItem.CentroundingLevel = GetValueFromDict(productSettings.CentRoundingLevel, payrollProductCentRoundingLevels);
                            payrollProductsItem.TaxCalculationType = GetValueFromDict(productSettings.TaxCalculationType, taxCalc);
                            payrollProductsItem.PensionCompany = GetValueFromDict(productSettings.PensionCompany, pensionCompanys);
                            payrollProductsItem.TimeUnit = GetValueFromDict(productSettings.TimeUnit, payrollProductTimeUnits);
                            payrollProductsItem.QuantityRoundingType = GetValueFromDict(productSettings.QuantityRoundingType, quantityRoundingTypes);
                            payrollProductsItem.QuantityRoundingMinutes = productSettings.QuantityRoundingMinutes;
                            payrollProductsItem.ChildProduct = payrollProducts.FirstOrDefault(w => w.ProductId == productSettings.ChildProductId)?.Name ?? string.Empty;

                            payrollProductsItem.PrintOnSalaryspecification = productSettings.PrintOnSalarySpecification;
                            payrollProductsItem.DontPrintOnSalarySpecificationWhenZeroAmount = productSettings.DontPrintOnSalarySpecificationWhenZeroAmount;
                            payrollProductsItem.ShowPrintDate = productSettings.PrintDate;
                            payrollProductsItem.DontIncludeInRetroactivePayroll = productSettings.DontIncludeInRetroactivePayroll;
                            payrollProductsItem.VacationSalaryPromoted = productSettings.VacationSalaryPromoted;
                            payrollProductsItem.UnionFeePromoted = productSettings.UnionFeePromoted;
                            payrollProductsItem.WorkingTimePromoted = productSettings.WorkingTimePromoted;
                            payrollProductsItem.CalculateSupplementCharge = productSettings.CalculateSupplementCharge;
                            payrollProductsItem.CalculateSicknessSalary = productSettings.CalculateSicknessSalary;

                            var sbf = new StringBuilder();
                            var sbt = new StringBuilder();
                            var sba = new StringBuilder();
                            var sbp = new StringBuilder();

                            if (productSettings.PriceFormulas.Count > 0)
                            {
                                foreach(var formula in productSettings.PriceFormulas)
                                {
                                    if (sbf.Length > 0)
                                        sbf.Append(", ");
                                    sbf.Append(priceFormulas.FirstOrDefault(w => w.PayrollPriceFormulaId == formula.PayrollPriceFormulaId)?.Name ?? string.Empty);
                                }
                            }

                            if (productSettings.PriceTypes.Count > 0)
                            {
                                foreach (var type in productSettings.PriceTypes)
                                {
                                    if (sbt.Length > 0)
                                        sbt.Append(", ");
                                    sbt.Append(payrollPricetypes.FirstOrDefault(w => w.PayrollPriceTypeId == type.PayrollPriceTypeId)?.Name ?? string.Empty);
                                }
                            }

                            if(productSettings.PurchaseAccounts.Count > 0)
                            {
                                foreach(var account in productSettings.PurchaseAccounts.Values)
                                {
                                    if (sba.Length > 0)
                                        sba.Append(", ");

                                    sba.Append(accountDimInternals.FirstOrDefault(w => w.AccountDimId == account.AccountDimId)?.Name ?? string.Empty);
                                    sba.Append(account.Description);

                                }
                            }

                            if(productSettings.AccountingPrio != null)
                            {
                                string[] prioItem;
                                var count = -1;
                                string[] prios = productSettings.AccountingPrio.Split(',');
                                foreach(var prio in prios)
                                {
                                    if (count == -1)
                                    {
                                        prioItem = prio.Split('=');
                                        if (prioItem.Count() < 2) prioItem[1] = "0";
                                        sbp.Append(accountDimStd?.Name  + ": " + GetValueFromDict(Int32.Parse(prioItem[1].IsNullOrEmpty() ? "0" : prioItem[1]), payrollProductAccountingPrio));
                                        count++;
                                        continue;
                                    }

                                    prioItem = prio.Split('=');
                                    if (count >= 0  && accountDimInternals.Count > count && prioItem.Any() && !prioItem[1].IsNullOrEmpty())
                                    {
                                        if (sbp.Length > 0)
                                            sbp.Append(", ");
                                        sbp.Append(accountDimInternals[count].Name + ": " + GetValueFromDict(Int32.Parse(prioItem[1]), payrollProductAccountingPrio));
                                    }
                                    count++;

                                }
                            }

                            payrollProductsItem.Payrollpriceformulas = sbf.ToString();
                            payrollProductsItem.Payrollpricetypes = sbt.ToString();
                            payrollProductsItem.AccountingPurchase = sba.ToString();
                            payrollProductsItem.AccountingPrioName = sbp.ToString();

                            #endregion

                            _reportDataOutput.PayrollProducts.Add(payrollProductsItem);
                        }
                    }
                    else
                    {
                        PayrollProductsItem payrollProductsItem = new PayrollProductsItem();

                        #region Product

                        payrollProductsItem.Number = product.Number;
                        payrollProductsItem.Name = product.Name;
                        payrollProductsItem.ShortName = product.ShortName;
                        payrollProductsItem.ExternalNumber = product.ExternalNumber;
                        payrollProductsItem.syspayrolltypelevel1 = sysPayrollTypes.FirstOrDefault(w => w.Id == product.SysPayrollTypeLevel1)?.Name ?? string.Empty;
                        payrollProductsItem.syspayrolltypelevel2 = sysPayrollTypes.FirstOrDefault(w => w.Id == product.SysPayrollTypeLevel2)?.Name ?? string.Empty;
                        payrollProductsItem.syspayrolltypelevel3 = sysPayrollTypes.FirstOrDefault(w => w.Id == product.SysPayrollTypeLevel3)?.Name ?? string.Empty;
                        payrollProductsItem.syspayrolltypelevel4 = sysPayrollTypes.FirstOrDefault(w => w.Id == product.SysPayrollTypeLevel4)?.Name ?? string.Empty;
                        payrollProductsItem.ProductFactor = product.Factor;
                        payrollProductsItem.ResultType = GetValueFromDict(product.ResultType, resultTypes);
                        payrollProductsItem.PayrollProductPayed = product.Payed;
                        payrollProductsItem.ExcludeInWorkTimeSummary = product.ExcludeInWorkTimeSummary;
                        payrollProductsItem.AverageCalculated = product.AverageCalculated;
                        payrollProductsItem.UseInPayroll = product.UseInPayroll;
                        payrollProductsItem.DontUseFixedAccounting = product.DontUseFixedAccounting;
                        payrollProductsItem.ProductExport = product.Export;
                        payrollProductsItem.IncludeAmountInExport = product.IncludeAmountInExport;

                        #endregion

                        _reportDataOutput.PayrollProducts.Add(payrollProductsItem);
                    }

                    #endregion
                }

                #endregion

                #region Close repository

                base.personalDataRepository.GenerateLogs();
                #endregion

            }

            return new ActionResult();
        }

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }
    }

    public class PayrollProductsReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_PayrollProductsMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public PayrollProductsReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_PayrollProductsMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_PayrollProductsMatrixColumns.Unknown;
        }
    }

    public class PayrollProductsReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<PayrollProductsReportDataField> Columns { get; set; }

        public PayrollProductsReportDataInput(CreateReportResult reportResult, List<PayrollProductsReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class PayrollProductsReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<PayrollProductsItem> PayrollProducts { get; set; }
        public PayrollProductsReportDataInput Input { get; set; }

        public PayrollProductsReportDataOutput(PayrollProductsReportDataInput input)
        {
            this.PayrollProducts = new List<PayrollProductsItem>();
            this.Input = input;
        }
    }
}

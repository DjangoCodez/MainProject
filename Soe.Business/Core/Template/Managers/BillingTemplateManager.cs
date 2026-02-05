using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Template;
using SoftOne.Soe.Business.Core.Template.Managers;
using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Core.Template.Models.Billing;
using SoftOne.Soe.Business.Core.Template.Models.Economy;
using SoftOne.Soe.Business.Core.Template.Models.Time;
using SoftOne.Soe.Business.Template.Managers;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Billing.Template.Managers
{
    public class BillingTemplateManager : ManagerBase
    {
        private readonly CompanyTemplateManager companyTemplateManager;
        private readonly AttestTemplateManager attestTemplateManager;

        public BillingTemplateManager(ParameterObject parameterObject) : base(parameterObject)
        {
            companyTemplateManager = new CompanyTemplateManager(base.parameterObject);
            attestTemplateManager = new AttestTemplateManager(base.parameterObject);
        }

        public TemplateCompanyBillingDataItem GetTemplateCompanyBillingDataItem(CopyFromTemplateCompanyInputDTO inputDTO)
        {
            TemplateCompanyBillingDataItem item = new TemplateCompanyBillingDataItem();
            StringBuilder logInfoBuilder = new StringBuilder();
            logInfoBuilder.AppendLine($"GetTemplateCompanyBillingDataItem - Getting billing template data from company {inputDTO.TemplateCompanyId} for company {inputDTO.TemplateCompanyId} to company {inputDTO.ActorCompanyId}");
            logInfoBuilder.AppendLine("Keys to be copied " + inputDTO.DoCopyKeys());

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyProducts) || inputDTO.DoCopy(TemplateCompanyCopy.BaseProductsBilling))
            {
                item.InvoiceProductCopyItems = GetInvoiceProductCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.ProductGroupCopyItems = item.InvoiceProductCopyItems?.Where(w => w.ProductGroup != null).Select(x => x.ProductGroup).GroupBy(g => g.Code).Select(s => s.First()).ToList();
                item.ProductUnitCopyItems = GetProductUnitCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

                if (item.ProductUnitCopyItems.IsNullOrEmpty())
                    item.InvoiceProductCopyItems?.Where(w => w.ProductUnit != null).Select(x => x.ProductUnit).GroupBy(g => g.Code).Select(s => s.First()).ToList();
            }

            if (inputDTO.DoCopy(TemplateCompanyCopy.PricesLists))
                item.PriceListCopyItems = GetPriceListCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.SupplierAgreements))
                item.SupplierAgreementCopyItems = GetSupplierAgreementCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.Checklists))
                item.ChecklistCopyItems = GetChecklistCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.EmailTemplates))
                item.EmailTemplateCopyItems = GetEmailTemplateCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyWholesellerPricelists))
                item.CompanyWholesellerPricelistCopyItems = GetCompanyWholesellerPriceListCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.PriceRules))
                item.PriceRuleCopyItem = GetPriceRuleCopyItemFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.ProjectSettings))
                item.ProjectSettingsCopyItem = GetProjectSettingsCopyItemFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
            //if (inputDTO.DoCopy(TemplateCompanyCopy.Customers))
            //    item.CustomerCopyItems = GetCustomerCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            //if (inputDTO.DoCopy(TemplateCompanyCopy.Contracts))
            //    item.ContractCopyItems = GetContractCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            //if (inputDTO.DoCopy(TemplateCompanyCopy.Orders))
            //    item.OrderCopyItems = GetOrderCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            logInfoBuilder.AppendLine($"GetTemplateCompanyBillingDataItem - Finished getting billing template data from company {inputDTO.TemplateCompanyId} for company {inputDTO.TemplateCompanyId} to company {inputDTO.ActorCompanyId}");
            LogInfo(logInfoBuilder.ToString());

            return item;
        }

        public List<TemplateResult> CopyTemplateCompanyBillingDataItem(CopyFromTemplateCompanyInputDTO inputDTO, TemplateCompanyDataItem templateCompanyDataItem)
        {
            List<TemplateResult> templateResults = new List<TemplateResult>();

            if (inputDTO.DoCopy(TemplateCompanyCopy.ProjectSettings))
                templateResults.Add(CopyProjectSettingsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.BaseProductsBilling))
            {
                templateResults.Add(CopyProductUnitsFromTemplateCompany(templateCompanyDataItem));
                templateResults.Add(CopyBaseProductsFromTemplateCompany(templateCompanyDataItem));
            }

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyProducts))
                templateResults.Add(CopyCompanyProductsFromTemplateCompany(templateCompanyDataItem, inputDTO.DoCopy(TemplateCompanyCopy.CompanyExternalProducts)));

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestBilling))
                templateResults.Add(attestTemplateManager.CopyAttestRolesFromTemplateCompany(templateCompanyDataItem, SoeModule.Billing));

            if (inputDTO.DoCopy(TemplateCompanyCopy.PricesLists))
                templateResults.Add(CopyPriceListsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.SupplierAgreements))
                templateResults.Add(CopySupplierAgreementsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.Checklists))
                templateResults.Add(CopyChecklistsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.EmailTemplates))
                templateResults.Add(CopyEmailTemplatesFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyWholesellerPricelists))
                templateResults.Add(CopyCompanyWholesellerPricelistsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.PriceRules))
                templateResults.Add(CopyPriceRulesFromTemplateCompany(templateCompanyDataItem));

            //if (inputDTO.DoCopy(TemplateCompanyCopy.Customers))
            //   templateResults.Add(CopyCustomersFromTemplateCompany(templateCompanyDataItem));

            //if (inputDTO.DoCopy(TemplateCompanyCopy.Contracts))
            //   templateResults.Add(CopyContractsFromTemplateCompany(templateCompanyDataItem));

            //if (inputDTO.DoCopy(TemplateCompanyCopy.Orders))
            //    templateResults.Add(CopyOrdersFromTemplateCompany(templateCompanyDataItem));

            return templateResults;
        }

        public TemplateResult CopyBaseProductsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            TemplateResult templateResult = new TemplateResult();
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company company = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                if (company == null)
                {
                    templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                    return templateResult;
                }

                if (!templateCompanyDataItem.TemplateCompanyBillingDataItem.InvoiceProductCopyItems.Any())
                    templateCompanyDataItem.TemplateCompanyBillingDataItem.InvoiceProductCopyItems = GetInvoiceProductCopyItemsFromApi(templateCompanyDataItem.SysCompDbId, templateCompanyDataItem.SourceActorCompanyId);

                Dictionary<int, InvoiceProduct> productSettingsDict = new Dictionary<int, InvoiceProduct>();

                List<ProductUnit> existingProductUnits = templateCompanyDataItem.Update ? ProductManager.GetProductUnits(entities, templateCompanyDataItem.DestinationActorCompanyId).ToList() : new List<ProductUnit>();
                List<ProductGroup> existingProductGroups = templateCompanyDataItem.Update ? ProductGroupManager.GetProductGroups(entities, templateCompanyDataItem.DestinationActorCompanyId).ToList() : new List<ProductGroup>();

                #endregion

                #region InvoiceProducts

                foreach (var templateSetting in templateCompanyDataItem.TemplateCompanyCoreDataItem.CompanySettingCopyItems.Where(w => w.IntData.HasValue && SettingManager.GetCompanySettingTypesForGroup((int)CompanySettingTypeGroup.BaseProducts).Contains(w.SettingTypeId)))
                {
                    var templateProduct = templateCompanyDataItem.TemplateCompanyBillingDataItem.InvoiceProductCopyItems.FirstOrDefault(f => f.ProductId == templateSetting.IntData.Value);
                    if (templateProduct == null)
                        continue;

                    InvoiceProduct product = ProductManager.GetInvoiceProductByProductNr(templateProduct.Number, templateCompanyDataItem.DestinationActorCompanyId, loadAccounts: true, loadProductGroup: true);
                    product = CopyInvoiceProduct(entities, product, templateProduct, templateCompanyDataItem, company, includeExternal: false, existingProductUnits, existingProductGroups);

                    if (product != null)
                        productSettingsDict.Add((int)templateSetting.SettingTypeId, product);

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("InvoiceProduct", templateCompanyDataItem, saved: true);

                #region UserCompanySettings

                if (result.Success)
                {
                    Dictionary<int, int> intSettings = new Dictionary<int, int>();
                    foreach (var pair in productSettingsDict)
                    {
                        intSettings.Add(pair.Key, pair.Value.ProductId);
                    }

                    result = SettingManager.UpdateInsertIntSettings(SettingMainType.Company, intSettings, 0, templateCompanyDataItem.DestinationActorCompanyId, 0);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("UserCompanySetting", templateCompanyDataItem, saved: true);
                }

                #endregion
            }
            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        public TemplateResult CopyProductUnitsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq
                Company company = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                if (company == null)
                {
                    templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                    return templateResult;
                }
                List<ProductUnit> existingProductUnits = templateCompanyDataItem.Update ? ProductManager.GetProductUnits(entities, templateCompanyDataItem.DestinationActorCompanyId) : new List<ProductUnit>();

                #endregion

                #region ProductUnits

                foreach (var productUnit in templateCompanyDataItem.TemplateCompanyBillingDataItem.ProductUnitCopyItems)
                {
                    try
                    {
                        if (existingProductUnits.Any(i => i.Code.ToLower() == productUnit.Code.ToLower()))
                        {
                            if (templateCompanyDataItem.Update)
                            {
                                // Update existing product unit
                                var existingProductUnit = existingProductUnits.First(i => i.Code.ToLower() == productUnit.Code.ToLower());
                                existingProductUnit.Name = productUnit.Name;
                                SetModifiedProperties(existingProductUnit);
                                templateCompanyDataItem.TemplateCompanyBillingDataItem.AddProductUnitMapping(productUnit.ProductUnitId, existingProductUnit);
                            }
                            else
                            {
                                templateCompanyDataItem.TemplateCompanyBillingDataItem.AddProductUnitMapping(productUnit.ProductUnitId, existingProductUnits.First(i => i.Code.ToLower() == productUnit.Code.ToLower()));
                                // Skip existing product unit
                                continue;
                            }
                        }
                        else
                        {

                            ProductUnit newProductUnit = new ProductUnit()
                            {
                                Code = productUnit.Code,
                                Name = productUnit.Name,
                                Company = company
                            };
                            SetCreatedProperties(newProductUnit);
                            entities.ProductUnit.AddObject(newProductUnit);
                            templateCompanyDataItem.TemplateCompanyBillingDataItem.AddProductUnitMapping(productUnit.ProductUnitId, newProductUnit);
                        }

                        SaveChanges(entities);

                    }
                    catch (Exception ex)
                    {
                        LogCollector.LogError(ex, $"Error copying ProductUnit {productUnit.Code} from template company");
                        templateResult.ActionResults.Add(new ActionResult(ex, $"Error copying ProductUnit {productUnit.Code} from template company"));
                    }
                }

                #endregion
                return templateResult;
            }
        }

        public TemplateResult CopyCompanyProductsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem, bool includeExternal)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company company = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                if (company == null)
                {
                    templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                    return templateResult;
                }

                List<ProductUnit> existingProductUnits = templateCompanyDataItem.Update ? ProductManager.GetProductUnits(entities, templateCompanyDataItem.DestinationActorCompanyId) : new List<ProductUnit>();
                List<ProductGroup> existingProductGroups = templateCompanyDataItem.Update ? ProductGroupManager.GetProductGroups(entities, templateCompanyDataItem.DestinationActorCompanyId) : new List<ProductGroup>();
                List<TimeCode> existingTimeCodes = templateCompanyDataItem.Update ? TimeCodeManager.GetTimeCodes(entities, templateCompanyDataItem.DestinationActorCompanyId, SoeTimeCodeType.Material) : new List<TimeCode>();

                #endregion

                #region ProductGroups

                foreach (var productGroup in templateCompanyDataItem.TemplateCompanyBillingDataItem.ProductGroupCopyItems)
                {
                    if (existingProductGroups.Any(i => i.Code.ToLower() == productGroup.Code.ToLower()))
                        continue;

                    ProductGroup newProductGroup = new ProductGroup()
                    {
                        Code = productGroup.Code,
                        Name = productGroup.Name,
                        Company = company,
                    };
                    SetCreatedProperties(newProductGroup);
                    entities.ProductGroup.AddObject(newProductGroup);
                }

                #endregion

                #region ProductUnits

                foreach (var productUnit in templateCompanyDataItem.TemplateCompanyBillingDataItem.ProductUnitCopyItems)
                {
                    if (existingProductUnits.Any(i => i.Code.ToLower() == productUnit.Code.ToLower()))
                        continue;

                    ProductUnit newProductUnit = new ProductUnit()
                    {
                        Code = productUnit.Code,
                        Name = productUnit.Name,
                        Company = company
                    };
                    SetCreatedProperties(newProductUnit);
                    entities.ProductUnit.AddObject(newProductUnit);
                    templateCompanyDataItem.TemplateCompanyBillingDataItem.AddProductUnitMapping(productUnit.ProductUnitId, newProductUnit);
                }

                #endregion

                SaveChanges(entities);
                existingProductUnits = templateCompanyDataItem.Update ? ProductManager.GetProductUnits(entities, templateCompanyDataItem.DestinationActorCompanyId) : new List<ProductUnit>();
                existingProductGroups = templateCompanyDataItem.Update ? ProductGroupManager.GetProductGroups(entities, templateCompanyDataItem.DestinationActorCompanyId) : new List<ProductGroup>();
                var existingsProducts = ProductManager.GetInvoiceProducts(entities, templateCompanyDataItem.DestinationActorCompanyId, true, loadProductUnitAndGroup: true, loadAccounts: true, loadTimeCode: true).ToList();

                #region InvoiceProducts

                foreach (var invoiceProductCopyItem in templateCompanyDataItem.TemplateCompanyBillingDataItem.InvoiceProductCopyItems)
                {
                    InvoiceProduct product = existingsProducts.FirstOrDefault(p => p.Number.ToLower() == invoiceProductCopyItem.Number.ToLower());
                    product = CopyInvoiceProduct(entities, product, invoiceProductCopyItem, templateCompanyDataItem, company, includeExternal, existingProductUnits, existingProductGroups);
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("InvoiceProduct", templateCompanyDataItem.DestinationActorCompanyId, templateCompanyDataItem.DestinationActorCompanyId, saved: true);

                templateResult.ActionResults.Add(result);

                #endregion

                return templateResult;
            }
        }

        public List<InvoiceProductCopyItem> GetInvoiceProductCopyItems(int actorCompanyId)
        {
            List<InvoiceProductCopyItem> invoiceProductCopyItems = new List<InvoiceProductCopyItem>();
            var invoiceProducts = ProductManager.GetInvoiceProducts(actorCompanyId, true, loadProductUnitAndGroup: true, loadAccounts: true, loadCategories: true, loadTimeCode: true);
            var accounts = AccountManager.GetAccounts(actorCompanyId);

            foreach (var invoiceProduct in invoiceProducts)
            {
                var item = new InvoiceProductCopyItem()
                {
                    //Set all available properties
                    ProductId = invoiceProduct.ProductId,
                    Type = invoiceProduct.Type,
                    Number = invoiceProduct.Number,
                    Name = invoiceProduct.Name,
                    Description = invoiceProduct.Description,
                    AccountingPrio = invoiceProduct.AccountingPrio,
                    CalculationType = invoiceProduct.CalculationType,
                    VatType = invoiceProduct.VatType,
                    VatCodeId = invoiceProduct.VatCodeId,
                    PurchasePrice = invoiceProduct.PurchasePrice,
                    ShowDescriptionAsTextRow = invoiceProduct.ShowDescriptionAsTextRow,
                    ShowDescrAsTextRowOnPurchase = invoiceProduct.ShowDescrAsTextRowOnPurchase,
                    HouseholdDeductionPercentage = invoiceProduct.HouseholdDeductionPercentage,
                    HouseholdDeductionType = invoiceProduct.HouseholdDeductionType,

                };

                if (invoiceProduct.ProductUnit != null)
                {
                    item.ProductUnit = new ProductUnitCopyItem()
                    {
                        Code = invoiceProduct.ProductUnit.Code,
                        Name = invoiceProduct.ProductUnit.Name,
                        ProductUnitId = invoiceProduct.ProductUnit.ProductUnitId,
                    };
                }

                if (invoiceProduct.ProductGroup != null)
                {
                    item.ProductGroup = new ProductGroupCopyItem()
                    {
                        Code = invoiceProduct.ProductGroup.Code,
                        Name = invoiceProduct.ProductGroup.Name,
                    };
                }

                if (invoiceProduct.TimeCode != null)
                {
                    item.TimeCode = new TimeCodeCopyItem();
                    item.TimeCode.SetValuesFromTimeCode(invoiceProduct.TimeCode);
                }

                if (invoiceProduct.ProductAccountStd != null && invoiceProduct.ProductAccountStd.Count > 0)
                {
                    foreach (var productAccountStd in invoiceProduct.ProductAccountStd.Where(w => w.AccountStd != null))
                    {

                        var matchingAccount = accounts.FirstOrDefault(a => a.AccountId == productAccountStd.AccountStd.AccountId);

                        if (matchingAccount != null)
                        {
                            item.ProductAccountStds.Add(new ProductAccountStdCopyItem()
                            {
                                Type = productAccountStd.Type,
                                Percent = productAccountStd.Percent,
                                AccountStd = new AccountStdCopyItem()
                                {
                                    AccountNr = matchingAccount.AccountNr,
                                    AccountId = matchingAccount.AccountId,
                                    AccountName = matchingAccount.Name
                                },
                            });
                        }

                    }
                }

                invoiceProductCopyItems.Add(item);
            }

            return invoiceProductCopyItems;
        }

        public InvoiceProduct CopyInvoiceProduct(CompEntities entities, InvoiceProduct product, InvoiceProductCopyItem invoiceProductCopyItem, TemplateCompanyDataItem templateCompanyDataItem, Company company, bool includeExternal, List<ProductUnit> existingProductUnits, List<ProductGroup> existingProductGroups)
        {
            if (product == null)
            {
                #region Invoice Product

                product = new InvoiceProduct()
                {
                    Type = invoiceProductCopyItem.Type,
                    Number = invoiceProductCopyItem.Number,
                    Name = invoiceProductCopyItem.Name,
                    Description = invoiceProductCopyItem.Description,
                    AccountingPrio = invoiceProductCopyItem.AccountingPrio,
                    CalculationType = invoiceProductCopyItem.CalculationType,
                    VatType = invoiceProductCopyItem.VatType,
                    VatCodeId = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetVatCode(invoiceProductCopyItem.VatCodeId ?? 0)?.VatCodeId,
                    HouseholdDeductionPercentage = invoiceProductCopyItem.HouseholdDeductionPercentage,
                    HouseholdDeductionType = invoiceProductCopyItem.HouseholdDeductionType,
                    ShowDescrAsTextRowOnPurchase = invoiceProductCopyItem.ShowDescrAsTextRowOnPurchase,
                    ShowDescriptionAsTextRow = invoiceProductCopyItem.ShowDescriptionAsTextRow,
                    IsStockProduct = invoiceProductCopyItem.IsStockProduct,
                    GuaranteePercentage = invoiceProductCopyItem.GuaranteePercentage,
                    DontUseDiscountPercent = invoiceProductCopyItem.DontUseDiscountPercent,
                    UseCalculatedCost = invoiceProductCopyItem.UseCalculatedCost,
                    Weight = invoiceProductCopyItem.Weight,
                    EAN = invoiceProductCopyItem.EAN,
                    SysWholesellerName = invoiceProductCopyItem.SysWholesellerName,
                    PriceListOrigin = invoiceProductCopyItem.PriceListOrigin,
                    ExternalProductId = invoiceProductCopyItem.ExternalProductId,
                    ExternalPriceListHeadId = invoiceProductCopyItem.ExternalPriceListHeadId,
                    VatFree = invoiceProductCopyItem.VatFree
                };
                SetCreatedProperties(product);
                product.Company.Add(company);

                if (includeExternal)
                {
                    product.ExternalProductId = invoiceProductCopyItem.ExternalProductId;
                    product.ExternalPriceListHeadId = invoiceProductCopyItem.ExternalPriceListHeadId;
                    product.SysWholesellerName = invoiceProductCopyItem.SysWholesellerName;
                }

                #region ProductUnit

                if (invoiceProductCopyItem.ProductUnit != null)
                {
                    product.ProductUnit = existingProductUnits.FirstOrDefault(i => i.Code.ToLower() == invoiceProductCopyItem.ProductUnit.Code.ToLower());
                    if (product.ProductUnit == null)
                    {
                        product.ProductUnit = new ProductUnit()
                        {
                            Code = invoiceProductCopyItem.ProductUnit.Code,
                            Name = invoiceProductCopyItem.ProductUnit.Name,

                            //Set references
                            Company = company,
                        };
                        SetCreatedProperties(product.ProductUnit);
                        existingProductUnits.Add(product.ProductUnit);
                        templateCompanyDataItem.TemplateCompanyBillingDataItem.AddProductUnitMapping(invoiceProductCopyItem.ProductUnit.ProductUnitId, product.ProductUnit);
                    }
                }

                #endregion

                #region ProductGroup

                if (invoiceProductCopyItem.ProductGroup != null)
                {
                    product.ProductGroup = existingProductGroups.FirstOrDefault(i => i.Code.ToLower() == invoiceProductCopyItem.ProductGroup.Code.ToLower());
                    if (product.ProductGroup == null)
                    {
                        product.ProductGroup = new ProductGroup()
                        {
                            Code = invoiceProductCopyItem.ProductGroup.Code,
                            Name = invoiceProductCopyItem.ProductGroup.Name,

                            //Set references
                            Company = company,
                        };
                        SetCreatedProperties(product.ProductGroup);
                        existingProductGroups.Add(product.ProductGroup);
                    }
                }

                #endregion

                #region ProductAccountStd

                if (!invoiceProductCopyItem.ProductAccountStds.IsNullOrEmpty())
                {
                    foreach (var productAccountStd in invoiceProductCopyItem.ProductAccountStds)
                    {
                        AccountStd accountStd = AccountManager.GetAccountStdByNr(entities, productAccountStd.AccountStd.AccountNr, templateCompanyDataItem.DestinationActorCompanyId);
                        if (accountStd == null)
                            continue;

                        ProductAccountStd newProductAccountStd = new ProductAccountStd()
                        {
                            Type = productAccountStd.Type,
                            Percent = productAccountStd.Percent,

                            //Set references
                            AccountStd = accountStd,
                        };
                        SetCreatedProperties(newProductAccountStd);
                        product.ProductAccountStd.Add(newProductAccountStd);
                    }
                }

                #endregion


                #endregion
            }

            #region TimeCode

            if (invoiceProductCopyItem.TimeCode != null)
            {
                product.TimeCodeId = templateCompanyDataItem.TemplateCompanyTimeDataItem.GetTimeCode(invoiceProductCopyItem.TimeCode.TimeCodeId)?.TimeCodeId;
                if (!product.TimeCodeId.HasValue)
                    product.TimeCode = TimeCodeManager.GetTimeCodes(entities, actorCompanyId: templateCompanyDataItem.DestinationActorCompanyId, SoeTimeCodeType.Material).FirstOrDefault(i => i.Code.ToLower() == invoiceProductCopyItem.TimeCode.Code.ToLower());
                else if (product.TimeCode == null)
                    product.TimeCode = TimeCodeManager.GetTimeCodes(entities, actorCompanyId: templateCompanyDataItem.DestinationActorCompanyId, SoeTimeCodeType.Material).FirstOrDefault(i => i.TimeCodeId == product.TimeCodeId);
                
                if (product.TimeCode != null)
                    templateCompanyDataItem.TemplateCompanyTimeDataItem.AddTimeCodeMapping(invoiceProductCopyItem.TimeCode.TimeCodeId, product.TimeCode);
            }

            #endregion

            if (product != null)
                templateCompanyDataItem.TemplateCompanyBillingDataItem.AddInvoiceProductMapping(invoiceProductCopyItem.ProductId, product);

            return product;
        }

        List<ProductUnitCopyItem> GetProductUnitCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetProductUnitCopyItems(actorCompanyId);
            return billingTemplateConnector.GetProductUnitCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<ProductUnitCopyItem> GetProductUnitCopyItems(int actorCompanyId)
        {
            List<ProductUnitCopyItem> productUnitCopyItems = new List<ProductUnitCopyItem>();
            var productUnits = ProductManager.GetProductUnits(actorCompanyId);
            foreach (var productUnit in productUnits)
            {
                var item = new ProductUnitCopyItem()
                {
                    ProductUnitId = productUnit.ProductUnitId,
                    Code = productUnit.Code,
                    Name = productUnit.Name,
                };
                productUnitCopyItems.Add(item);
            }
            return productUnitCopyItems;
        }

        List<InvoiceProductCopyItem> GetInvoiceProductCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetInvoiceProductCopyItems(actorCompanyId);

            return billingTemplateConnector.GetInvoiceProductCopyItems(sysCompDbId, actorCompanyId);
        }

        #region PriceList

        public TemplateResult CopyPriceListsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    Company company = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                    if (company == null)
                    {
                        templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                        return templateResult;
                    }

                    var defaultCurrency = entities.Currency.FirstOrDefault(f => f.ActorCompanyId == item.DestinationActorCompanyId);

                    if (defaultCurrency == null)
                    {
                        entities.Currency.AddObject(new Currency()
                        {
                            Code = "SEK",
                            Name = "Svenska kronor",
                            SysCurrencyId = 1,
                            Company = company
                        });

                        entities.SaveChanges();
                        defaultCurrency = entities.Currency.FirstOrDefault(f => f.ActorCompanyId == item.DestinationActorCompanyId);
                    }

                    if (defaultCurrency == null)
                    {
                        templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Valutan hittades inte")));
                        return templateResult;
                    }


                    List<PriceListType> existingPriceListTypes = ProductPricelistManager.GetPriceListTypes(entities, item.DestinationActorCompanyId, true);
                    foreach (var priceListCopyItemGroup in item.TemplateCompanyBillingDataItem.PriceListCopyItems.GroupBy(g => g.PriceListTypeId))
                    {
                        var type = priceListCopyItemGroup.First();

                        // Check if PriceListType with the same name already exists, if so skip
                        if (existingPriceListTypes.Exists(e => e.Name == type.Name))
                            continue;

                        // Create new PriceListType based on priceListCopyItem
                        var newPriceListType = new PriceListType
                        {
                            Name = type.Name,
                            Company = company,
                            Currency = defaultCurrency,
                        };

                        item.TemplateCompanyBillingDataItem.AddPriceListMapping(type.PriceListTypeId, newPriceListType);

                        foreach (var pl in item.TemplateCompanyBillingDataItem.PriceListCopyItems.Where(w => w.PriceListTypeId == type.PriceListTypeId && w.ProductId > 0))
                        {
                                var product = item.TemplateCompanyBillingDataItem.GetInvoiceProduct(pl.ProductId);

                                if (product == null)
                                    continue;

                                var newPriceList = new PriceList
                                {
                                    Price = pl.Price,
                                    DiscountPercent = pl.DiscountPercent,
                                    StartDate = pl.StartDate,
                                    StopDate = pl.StopDate,
                                    Quantity = pl.Quantity,
                                    ProductId = product.ProductId,
                                };

                                newPriceListType.PriceList.Add(newPriceList);
                        }
                        result = SaveChanges(entities);
                        if (!result.Success)
                            result = companyTemplateManager.LogCopyError("InvoiceProduct", item, saved: true);

                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("priceListCopyItems", item, saved: true);
                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        public List<PriceListCopyItem> GetPriceListCopyItems(int actorCompanyId)
        {
            List<PriceListCopyItem> priceListCopyItems = new List<PriceListCopyItem>();

            try
            {
                List<PriceListType> priceListTypes = ProductPricelistManager.GetPriceListTypes(actorCompanyId, true);

                foreach (var priceListType in priceListTypes)
                {
                    if (priceListType.PriceList.IsNullOrEmpty())
                    {
                        // Pricelisttypes without prices should also be copied
                        PriceListCopyItem copyItem = new PriceListCopyItem()
                        {
                            ActorCompanyId = actorCompanyId,
                            Name = priceListType.Name,
                            PriceListTypeId = priceListType.PriceListTypeId,
                            Price = 0,
                            DiscountPercent = 0,
                            StartDate = DateTime.Now,
                            StopDate = DateTime.Now,
                            Quantity = 0,
                            ProductId = 0,
                        };

                        priceListCopyItems.Add(copyItem);
                    }
                    else
                    {
                        foreach (var priceList in priceListType.PriceList)
                        {

                            PriceListCopyItem copyItem = new PriceListCopyItem()
                            {
                                ActorCompanyId = actorCompanyId,
                                Name = priceListType.Name,
                                PriceListTypeId = priceListType.PriceListTypeId,
                                Price = priceList.Price,
                                DiscountPercent = priceList.DiscountPercent,
                                StartDate = priceList.StartDate,
                                StopDate = priceList.StopDate,
                                Quantity = priceList.Quantity,
                                ProductId = priceList.ProductId,
                            };

                            priceListCopyItems.Add(copyItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return priceListCopyItems;
        }

        public List<PriceListCopyItem> GetPriceListCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<PriceListCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetPriceListCopyItems(actorCompanyId);

            return billingTemplateConnector.GetPriceListCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region SupplierAgreement

        public TemplateResult CopySupplierAgreementsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    Company company = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                    if (company == null)
                    {
                        templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                        return templateResult;
                    }

                    List<SupplierAgreement> existingAgreements = SupplierAgreementManager.GetSupplierAgreements(item.DestinationActorCompanyId);

                    foreach (var agreementCopyItem in item.TemplateCompanyBillingDataItem.SupplierAgreementCopyItems)
                    {
                        if (existingAgreements.Exists(e => e.Code == agreementCopyItem.Code && e.CodeType == agreementCopyItem.CodeType))
                            continue;

                        SupplierAgreement newAgreement = new SupplierAgreement()
                        {
                            CategoryId = agreementCopyItem.CategoryId,
                            CodeType = agreementCopyItem.CodeType,
                            Code = agreementCopyItem.Code,
                            Company = company,
                            Date = agreementCopyItem.Date,
                            DiscountPercent = agreementCopyItem.DiscountPercent,
                            PriceListOrigin = agreementCopyItem.PriceListOrigin,
                            PriceListTypeId = item.TemplateCompanyBillingDataItem.GetPriceList(agreementCopyItem.PriceListTypeId ?? 0)?.PriceListTypeId ?? null,
                            SysWholesellerId = agreementCopyItem.SysWholesellerId,
                        };

                        SetCreatedProperties(newAgreement);
                        entities.SupplierAgreement.AddObject(newAgreement);
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("SupplierAgreements", item, saved: true);
                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }


        public List<SupplierAgreementCopyItem> GetSupplierAgreementCopyItems(int actorCompanyId)
        {
            List<SupplierAgreementCopyItem> supplierAgreementCopyItems = new List<SupplierAgreementCopyItem>();
            try
            {
                List<SupplierAgreement> supplierAgreements = SupplierAgreementManager.GetSupplierAgreements(actorCompanyId);
                foreach (var agreement in supplierAgreements)
                {
                    SupplierAgreementCopyItem copyItem = new SupplierAgreementCopyItem()
                    {
                        ActorCompanyId = actorCompanyId,
                        CategoryId = agreement.CategoryId,
                        CodeType = agreement.CodeType,
                        Code = agreement.Code,
                        Date = agreement.Date,
                        DiscountPercent = agreement.DiscountPercent,
                        PriceListOrigin = agreement.PriceListOrigin,
                        PriceListTypeId = agreement.PriceListTypeId,
                        SysWholesellerId = agreement.SysWholesellerId
                    };
                    supplierAgreementCopyItems.Add(copyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }
            return supplierAgreementCopyItems;
        }

        public List<SupplierAgreementCopyItem> GetSupplierAgreementCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<SupplierAgreementCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetSupplierAgreementCopyItems(actorCompanyId);

            return billingTemplateConnector.GetSupplierAgreementCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region Checklists

        public TemplateResult CopyChecklistsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    Company company = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                    if (company == null)
                    {
                        templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                        return templateResult;
                    }
                    var multipleChoiceHeadMapping = new Dictionary<int, CheckListMultipleChoiceAnswerHead>();
                    List<ChecklistHead> existingChecklists = ChecklistManager.GetChecklistHeadsForType(TermGroup_ChecklistHeadType.Order, item.DestinationActorCompanyId, true);
                    List<CheckListMultipleChoiceAnswerHead> existingMultipleChoiceAnswerHeads = entities.CheckListMultipleChoiceAnswerHead.Include("ChecklistRow").Where(c => c.ActorCompanyId == item.DestinationActorCompanyId).ToList();

                    foreach (var checklistCopyItem in item.TemplateCompanyBillingDataItem.ChecklistCopyItems)
                    {
                        ChecklistHead newChecklistHead = existingChecklists.FirstOrDefault(e => e.Name == checklistCopyItem.Name && e.Type == checklistCopyItem.Type);

                        Report report = item.TemplateCompanyCoreDataItem.GetReport(checklistCopyItem.ReportId);

                        if (newChecklistHead == null)
                        {
                            // Create new ChecklistHead based on checklistCopyItem
                            newChecklistHead = new ChecklistHead
                            {
                                Name = checklistCopyItem.Name,
                                Type = checklistCopyItem.Type,
                                Description = checklistCopyItem.Description,
                                ActorCompanyId = item.DestinationActorCompanyId,
                                ReportId = report != null ? report.ReportId : (int?)null,
                            };

                            foreach (var rowCopyItem in checklistCopyItem.ChecklistRowCopyItems)
                            {
                                ChecklistRow newRow = new ChecklistRow
                                {
                                    Type = (int)rowCopyItem.Type,
                                    RowNr = rowCopyItem.RowNr,
                                    Mandatory = rowCopyItem.Mandatory,
                                    Text = rowCopyItem.Text
                                };

                                if (rowCopyItem.MultipleChoiceAnswerHead != null)
                                {
                                    CheckListMultipleChoiceAnswerHead head = null;
                                    if (multipleChoiceHeadMapping.ContainsKey(rowCopyItem.MultipleChoiceAnswerHead.MultipleChoiceAnswerHeadId))
                                    {
                                        head = multipleChoiceHeadMapping[rowCopyItem.MultipleChoiceAnswerHead.MultipleChoiceAnswerHeadId];
                                    }
                                    else
                                    {
                                        head = new CheckListMultipleChoiceAnswerHead
                                        {
                                            Title = rowCopyItem.MultipleChoiceAnswerHead.Title,
                                            ActorCompanyId = item.DestinationActorCompanyId
                                        };

                                        foreach (var rowCopy in rowCopyItem.MultipleChoiceAnswerHead.CheckListMultipleChoiceAnswerRows)
                                        {
                                            CheckListMultipleChoiceAnswerRow answerRow = new CheckListMultipleChoiceAnswerRow
                                            {
                                                Question = rowCopy.Question
                                            };
                                            head.CheckListMultipleChoiceAnswerRow.Add(answerRow);
                                        }

                                        multipleChoiceHeadMapping.Add(rowCopyItem.MultipleChoiceAnswerHead.MultipleChoiceAnswerHeadId, head);
                                    }
                                    newRow.CheckListMultipleChoiceAnswerHead = head;
                                }
                                newChecklistHead.ChecklistRow.Add(newRow);
                            }

                            SetCreatedProperties(newChecklistHead);
                            entities.ChecklistHead.AddObject(newChecklistHead);
                        }
                        else
                        {
                            newChecklistHead.ReportId = report != null ? report.ReportId : (int?)null;
                        }
                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            result = companyTemplateManager.LogCopyError("Checklist", item, saved: true);
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        public List<ChecklistCopyItem> GetChecklistCopyItems(int actorCompanyId)
        {
            List<ChecklistCopyItem> checklistCopyItems = new List<ChecklistCopyItem>();

            try
            {
                List<ChecklistHead> checklistHeads = ChecklistManager.GetChecklistHeadsForType(TermGroup_ChecklistHeadType.Order, actorCompanyId, true);

                // Convert to ChecklistCopyItems
                foreach (var checklistHead in checklistHeads)
                {
                    ChecklistCopyItem copyItem = new ChecklistCopyItem
                    {
                        ActorCompanyId = actorCompanyId,
                        Name = checklistHead.Name,
                        Type = checklistHead.Type,
                        Description = checklistHead.Description,
                        ChecklistId = checklistHead.ChecklistHeadId,
                        ReportId = checklistHead.ReportId.HasValue ? checklistHead.ReportId.Value : 0,
                        ChecklistRowCopyItems = new List<ChecklistRowCopyItem>()
                    };

                    // Loop through each ChecklistRow in checklistHead to populate the ChecklistRowCopyItems
                    foreach (var checklistRow in checklistHead.ChecklistRow)
                    {
                        ChecklistRowCopyItem rowCopyItem = new ChecklistRowCopyItem
                        {
                            Type = (TermGroup_ChecklistRowType)checklistRow.Type,
                            RowNr = checklistRow.RowNr,
                            Mandatory = checklistRow.Mandatory,
                            Text = checklistRow.Text,
                        };

                        if (checklistRow.CheckListMultipleChoiceAnswerHead != null)
                        {
                            rowCopyItem.MultipleChoiceAnswerHead = new CheckListMultipleChoiceAnswerHeadCopyItem
                            {
                                Title = checklistRow.CheckListMultipleChoiceAnswerHead.Title,
                                CheckListMultipleChoiceAnswerRows = new List<CheckListMultipleChoiceAnswerRowCopyItem>(),
                                MultipleChoiceAnswerHeadId = checklistRow.CheckListMultipleChoiceAnswerHead.CheckListMultipleChoiceAnswerHeadId
                            };

                            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                            var head = entitiesReadOnly.CheckListMultipleChoiceAnswerHead.Include("CheckListMultipleChoiceAnswerRow").FirstOrDefault(f => f.CheckListMultipleChoiceAnswerHeadId == checklistRow.CheckListMultipleChoiceAnswerHead.CheckListMultipleChoiceAnswerHeadId);

                            if (head?.CheckListMultipleChoiceAnswerRow != null)
                            {
                                foreach (var answerRow in head.CheckListMultipleChoiceAnswerRow)
                                {
                                    CheckListMultipleChoiceAnswerRowCopyItem answerRowCopyItem = new CheckListMultipleChoiceAnswerRowCopyItem
                                    {
                                        Question = answerRow.Question
                                    };
                                    rowCopyItem.MultipleChoiceAnswerHead.CheckListMultipleChoiceAnswerRows.Add(answerRowCopyItem);
                                }
                            }
                        }

                        copyItem.ChecklistRowCopyItems.Add(rowCopyItem);
                    }

                    checklistCopyItems.Add(copyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return checklistCopyItems;
        }

        public List<ChecklistCopyItem> GetChecklistCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<ChecklistCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetChecklistCopyItems(actorCompanyId);

            return billingTemplateConnector.GetChecklistCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region EmailTemplate

        public TemplateResult CopyEmailTemplatesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();
            ActionResult result = new ActionResult();
            var emailTemplateMapping = new Dictionary<int, EmailTemplate>();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                    if (newCompany == null)
                    {
                        templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                        return templateResult;
                    }
                    List<EmailTemplate> existingTemplates = EmailManager.GetEmailTemplates(entities, item.DestinationActorCompanyId);

                    foreach (var emailTemplateCopyItem in item.TemplateCompanyBillingDataItem.EmailTemplateCopyItems)
                    {
                        EmailTemplate template = existingTemplates.FirstOrDefault(a => a.Name == emailTemplateCopyItem.Name);

                        if (template == null)
                        {
                            template = new EmailTemplate
                            {
                                Name = emailTemplateCopyItem.Name,
                                Subject = emailTemplateCopyItem.Subject,
                                Body = emailTemplateCopyItem.Body,
                                Company = newCompany,
                                BodyIsHTML = emailTemplateCopyItem.BodyIsHTML,
                                Type = (int)emailTemplateCopyItem.Type
                            };

                            SetCreatedProperties(template);
                            entities.EmailTemplate.AddObject(template);
                            result = SaveChanges(entities);
                            if (!result.Success)
                            {
                                result = companyTemplateManager.LogCopyError("EmailTemplate", item, saved: true);
                            }
                        }

                        item.TemplateCompanyBillingDataItem.AddEmailTemplateMapping(emailTemplateCopyItem.TemplateId, template);
                    }


                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        public List<EmailTemplateCopyItem> GetEmailTemplateCopyItems(int actorCompanyId)
        {
            List<EmailTemplateCopyItem> emailTemplateCopyItems = new List<EmailTemplateCopyItem>();

            try
            {
                // Fetch email templates based on the actor company ID
                List<EmailTemplate> emailTemplates = EmailManager.GetEmailTemplates(actorCompanyId);

                // Convert to EmailTemplateCopyItems
                foreach (var emailTemplate in emailTemplates)
                {
                    EmailTemplateCopyItem copyItem = new EmailTemplateCopyItem
                    {
                        ActorCompanyId = actorCompanyId,
                        Name = emailTemplate.Name,
                        Subject = emailTemplate.Subject,
                        Body = emailTemplate.Body,
                        TemplateId = emailTemplate.EmailTemplateId,
                        BodyIsHTML = emailTemplate.BodyIsHTML,
                        Type = (EmailTemplateType)emailTemplate.Type
                    };

                    emailTemplateCopyItems.Add(copyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return emailTemplateCopyItems;
        }

        public List<EmailTemplateCopyItem> GetEmailTemplateCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<EmailTemplateCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetEmailTemplateCopyItems(actorCompanyId);

            return billingTemplateConnector.GetEmailTemplateCopyItems(sysCompDbId, actorCompanyId);
        }
        #endregion

        #region ProjectSettings

        public TemplateResult CopyProjectSettingsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();
            ActionResult result = new ActionResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                    if (newCompany == null)
                    {
                        templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                        return templateResult;
                    }

                    List<TimeCode> existingTimeCodes = TimeCodeManager.GetTimeCodes(item.DestinationActorCompanyId, (int)SoeTimeCodeType.Work, (int)SoeTimeCodeType.Material);

                    foreach (TimeCodeCopyItem templateTimeCode in item.TemplateCompanyBillingDataItem.ProjectSettingsCopyItem.TimeCodes)
                    {
                        TimeCode timeCode = existingTimeCodes.FirstOrDefault(a => a.Name == templateTimeCode.Name);
                        if (timeCode == null)
                        {
                            timeCode = templateTimeCode.CopyTimeCode(item.TemplateCompanyTimeDataItem, newCompany);
                            if (timeCode == null)
                                continue;

                            SetCreatedProperties(timeCode);
                            entities.TimeCode.AddObject(timeCode);
                            existingTimeCodes.Add(timeCode);

                            result = SaveChanges(entities);
                            if (!result.Success)
                                result = companyTemplateManager.LogCopyError("TimeCode", item, saved: true);
                        }

                        item.TemplateCompanyBillingDataItem.AddTimeCodeMapping(templateTimeCode.TimeCodeId, timeCode);
                    }


                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        #endregion

        #region CompanyWholesellerPriceLists

        public TemplateResult CopyCompanyWholesellerPricelistsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();
            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                    if (newCompany == null)
                    {
                        templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                        return templateResult;
                    }

                    foreach (var copyItem in item.TemplateCompanyBillingDataItem.CompanyWholesellerPricelistCopyItems)
                    {
                        var pricelist = new CompanyWholesellerPricelist()
                        {
                            SysPriceListHeadId = copyItem.SysPriceListHeadId,
                            SysWholesellerId = copyItem.SysWholesellerId,
                            Company = newCompany
                        };
                    }

                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("CompanyWholesellerPricelists", item, saved: true);

                    templateResult.ActionResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return templateResult;
        }

        public List<CompanyWholesellerPriceListCopyItem> GetCompanyWholesellerPriceListCopyItems(int actorCompanyId)
        {
            List<CompanyWholesellerPriceListCopyItem> copyItems = new List<CompanyWholesellerPriceListCopyItem>();

            try
            {
                List<CompanyWholesellerPricelist> priceLists = WholeSellerManager.GetAllCompanyWholesellerPriceLists(actorCompanyId);

                foreach (var priceList in priceLists)
                {
                    CompanyWholesellerPriceListCopyItem copyItem = new CompanyWholesellerPriceListCopyItem
                    {
                        ActorCompanyId = actorCompanyId,
                        SysPriceListHeadId = priceList.SysPriceListHeadId,
                        SysWholesellerId = priceList.SysWholesellerId,
                    };

                    copyItems.Add(copyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return copyItems;
        }

        public List<CompanyWholesellerPriceListCopyItem> GetCompanyWholesellerPriceListCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<CompanyWholesellerPriceListCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetCompanyWholesellerPriceListCopyItems(actorCompanyId);

            return billingTemplateConnector.GetCompanyWholesellerPriceListCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion

        #region PriceRule

        public TemplateResult CopyPriceRulesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();
            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    Company company = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                    if (company == null)
                    {
                        templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                        return templateResult;
                    }

                    // Load data 
                    List<PriceRule> existingPriceRules = PriceRuleManager.GetPriceRules(entities, item.DestinationActorCompanyId, true);
                    List<PriceListType> existingPriceListTypes = ProductPricelistManager.GetPriceListTypes(entities, item.DestinationActorCompanyId);

                    List<PriceRule> templatePriceRules = item.TemplateCompanyBillingDataItem.PriceRuleCopyItem.TemplateCompanyPriceRules;
                    List<PriceListType> templatePriceListTypes = item.TemplateCompanyBillingDataItem.PriceRuleCopyItem.TemplateCompanyPriceListTypes;

                    // Perform copy
                    ActionResult result = new ActionResult();

                    //Get sub rules
                    List<int> subPriceRuleIds = new List<int>();
                    foreach (PriceRule sourcePriceRule in templatePriceRules)
                    {
                        if (sourcePriceRule.RRule != null)
                        {
                            if (!subPriceRuleIds.Contains(sourcePriceRule.RRule.RuleId))
                                subPriceRuleIds.Add(sourcePriceRule.RRule.RuleId);
                        }
                        if (sourcePriceRule.LRule != null)
                        {
                            if (!subPriceRuleIds.Contains(sourcePriceRule.LRule.RuleId))
                                subPriceRuleIds.Add(sourcePriceRule.LRule.RuleId);
                        }
                    }

                    foreach (var templatePriceRule in templatePriceRules)
                    {
                        if (subPriceRuleIds.Contains(templatePriceRule.RuleId))
                            continue;

                        var templatePriceListType = templatePriceListTypes.FirstOrDefault(t => t.PriceListTypeId == templatePriceRule.PriceListType.PriceListTypeId);
                        if (templatePriceListType == null)
                            continue;

                        var mappedPriceListType = existingPriceListTypes.FirstOrDefault(t => t.Name == templatePriceListType.Name);
                        if (mappedPriceListType == null)
                            continue;

                        var existingPriceRule = existingPriceRules.FirstOrDefault(r => r.PriceListType == mappedPriceListType && r.LValueType == templatePriceRule.LValueType && r.RValueType == templatePriceRule.RValueType && r.LValue == templatePriceRule.LValue && r.RValue == templatePriceRule.RValue);

                        if (existingPriceRule == null)
                        {
                            PriceRule newPriceRule = CopyPriceRuleRecursive(templatePriceRule, mappedPriceListType, company, templatePriceRules);
                            if (newPriceRule == null)
                            {
                                templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityIsNull, "PriceRule"));
                                return templateResult;
                            }

                            result = AddEntityItem(entities, newPriceRule, "PriceRule");
                            if (result.Success)
                                result.IntegerValue = newPriceRule.RuleId;
                        }
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("PriceRules", item, saved: true);

                    templateResult.ActionResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return templateResult;
        }

        private PriceRule CopyPriceRuleRecursive(PriceRule priceRule, PriceListType priceListType, Company company, List<PriceRule> templatePriceRules)
        {
            PriceRule leftPriceRule = null;
            if (priceRule.LRule != null)
                leftPriceRule = CopyPriceRuleRecursive(templatePriceRules.FirstOrDefault(r => r.RuleId == priceRule.LRule.RuleId), priceListType, company, templatePriceRules);
            PriceRule rightPriceRule = null;
            if (priceRule.RRule != null)
                rightPriceRule = CopyPriceRuleRecursive(templatePriceRules.FirstOrDefault(r => r.RuleId == priceRule.RRule.RuleId), priceListType, company, templatePriceRules);

            PriceRule newPriceRule = new PriceRule()
            {
                OperatorType = priceRule.OperatorType,
                lExampleType = priceRule.lExampleType,
                rExampleType = priceRule.rExampleType,
                LValue = priceRule.LValue,
                RValue = priceRule.RValue,
                LValueType = priceRule.LValueType,
                RValueType = priceRule.RValueType,

                //References
                LRule = leftPriceRule,
                RRule = rightPriceRule,
                PriceListType = priceListType,
                Company = company,
            };

            return newPriceRule;
        }

        public PriceRuleCopyItem GetPriceRuleCopyItem(int actorCompanyId)
        {
            PriceRuleCopyItem copyItem = null;

            try
            {
                copyItem = new PriceRuleCopyItem
                {
                    TemplateActorCompanyId = actorCompanyId,
                    TemplateCompanyPriceRules = PriceRuleManager.GetPriceRules(actorCompanyId, true),
                    TemplateCompanyPriceListTypes = ProductPricelistManager.GetPriceListTypes(actorCompanyId)
                };
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return copyItem;
        }

        public ProjectSettingsCopyItem GetProjectSettingsCopyItem(int actorCompanyId)
        {
            ProjectSettingsCopyItem copyItem = null;

            try
            {
                var timeCodes = new TimeTemplateManager(parameterObject).GetTimeCodeCopyItems(actorCompanyId);
                copyItem = new ProjectSettingsCopyItem
                {
                    TimeCodes = timeCodes.Where(w => w.Type == (int)SoeTimeCodeType.Work || w.Type == (int)SoeTimeCodeType.Material).ToList()
                };
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return copyItem;
        }

        public PriceRuleCopyItem GetPriceRuleCopyItemFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return null;

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetPriceRuleCopyItem(actorCompanyId);

            return billingTemplateConnector.GetPriceRuleCopyItem(sysCompDbId, actorCompanyId);
        }

        public ProjectSettingsCopyItem GetProjectSettingsCopyItemFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return null;

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetProjectSettingsCopyItem(actorCompanyId);

            return billingTemplateConnector.GetProjectSettingsCopyItem(sysCompDbId, actorCompanyId);
        }

        #endregion
    }
}

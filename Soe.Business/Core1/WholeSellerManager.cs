using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class WholeSellerManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public WholeSellerManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region CompanyWholeSellerPriceList

        public CompanyWholesellerPricelist GetCompanyWholesellerPriceList(CompEntities entities, int? companyWholesellerPriceListId, int actorCompanyId)
        {
            return (from pl in entities.CompanyWholesellerPricelist
                    where pl.Company.ActorCompanyId == actorCompanyId &&
                    (companyWholesellerPriceListId.HasValue == false || companyWholesellerPriceListId.Value == pl.CompanyWholesellerPriceListId)
                    select pl).FirstOrDefault();
        }

        public List<CompanyWholesellerPricelist> GetAllCompanyWholesellerPriceLists(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyWholesellerPricelist.NoTracking();
            return GetAllCompanyWholesellerPriceLists(entities, actorCompanyId);
        }

        public List<CompanyWholesellerPricelist> GetAllCompanyWholesellerPriceLists(CompEntities entities, int actorCompanyId)
        {
            return (from pl in entities.CompanyWholesellerPricelist
                    where pl.Company.ActorCompanyId == actorCompanyId
                    select pl).ToList();
        }

        public int GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(CompEntities entities, int sysWholeSellerId, int actorCompanyId)
        {
            return (from pl in entities.CompanyWholesellerPricelist
                    where pl.Company.ActorCompanyId == actorCompanyId &&
                    pl.SysWholesellerId == sysWholeSellerId
                    orderby pl.CompanyWholesellerPriceListId descending
                    select pl.SysPriceListHeadId).FirstOrDefault();
        }

        public bool ExistsCompanyWholesellerPriceLists(CompEntities entites, int sysPriceListHeadId, int sysWholeSellerId, int actorCompanyId)
        {
            return (from pricelist in entites.CompanyWholesellerPricelist
                    where pricelist.Company.ActorCompanyId == actorCompanyId &&
                    pricelist.CompanyWholesellerPriceListId == sysPriceListHeadId &&
                    pricelist.SysWholesellerId == sysWholeSellerId
                    select pricelist).Count() > 0;
        }

        public bool HasCompanyWholesellerPriceLists(CompEntities entites, int actorCompanyId)
        {
            entites.CompanyWholesellerPricelist.NoTracking();
            var hasPriceLists = (from pl in entites.CompanyWholesellerPricelist
                                 where pl.Company.ActorCompanyId == actorCompanyId
                                 select pl).Any();

            if (!hasPriceLists)
            {
                // Also look for comp pricelists
                hasPriceLists = (from pl in entites.PriceListImportedHead
                                 where pl.ActorCompanyId == actorCompanyId
                                 select pl).Any();
            }

            return hasPriceLists;
        }

        public static bool CanHaveImportedPriceList(int sysWholesellerId)
        {
            return sysWholesellerId == (int)SoeWholeseller.Rexel || sysWholesellerId == (int)SoeWholeseller.Lunda;
        }

        public ActionResult SaveCompanyWholesellerPriceLists(List<CompanyWholesellerPriceListViewDTO> companyPriceListItems, int actorCompanyId)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                #region Prereq

                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //remove any CompPricelists
                companyPriceListItems = companyPriceListItems.Where(i => i.PriceListOrigin != PriceListOrigin.CompDbPriceList && i.ActorCompanyId == actorCompanyId).ToList();

                // Filter used for Company
                #endregion


                // Get existing
                var existingCompanyPriceLists = (from cpl in entities.CompanyWholesellerPricelist
                                                 where cpl.Company.ActorCompanyId == actorCompanyId
                                                 select cpl).ToList();

                var companyPriceListItemsToAdd = companyPriceListItems.Where(p => !p.CompanyWholesellerPriceListId.HasValue && p.IsUsed).ToList();
                var companyPriseListIdsToRemove = companyPriceListItems.Where(p => p.CompanyWholesellerPriceListId.HasValue && !p.IsUsed).Select(x => x.CompanyWholesellerPriceListId).ToList();

                if (companyPriceListItemsToAdd.Any())
                {
                    var sysWholsellerIds = companyPriceListItemsToAdd.Select(x => x.SysWholesellerId).ToList();
                    sysWholsellerIds.AddRange(existingCompanyPriceLists.Select(y => y.SysWholesellerId));

                    var syspriceListHeadIds = companyPriceListItemsToAdd.Select(x => x.SysPriceListHeadId).ToList();
                    syspriceListHeadIds.AddRange(existingCompanyPriceLists.Select(y => y.SysPriceListHeadId));
                    var sysPriceListHeads = SysPriceListManager.GetSysPriceListHeads(syspriceListHeadIds);

                    var duplicateProviderId = sysPriceListHeads.GroupBy(x => x.Provider)
                            .Where(g => g.Count() > 1)
                            .Select(y => y.Key)
                            .FirstOrDefault();

                    if (duplicateProviderId > 0)
                    {
                        var wholseller = GetSysWholesellerDTO(companyPriceListItemsToAdd.FirstOrDefault(x => x.Provider == duplicateProviderId)?.SysWholesellerId ?? 0);
                        if (wholseller != null)
                        {
                            return new ActionResult(GetText(7650, "Endast en prislista kan vara aktiv för en återförsäljare") + ": " + wholseller.Name + ":" + Enum.GetName(typeof(SoeSysPriceListProvider), duplicateProviderId));
                        }
                    }

                    var chainPriceLists = companyPriceListItemsToAdd.Where(x => SysPriceListManager.IsCurrentum((SoeSysPriceListProvider)x.Provider) || SysPriceListManager.IsComfort((SoeSysPriceListProvider)x.Provider)).ToList();
                    if (chainPriceLists.Any())
                    {
                        foreach (var chainSysWholesellerId in chainPriceLists.Select(x => x.SysWholesellerId))
                        {
                            if (existingCompanyPriceLists.Any(x => x.SysWholesellerId == chainSysWholesellerId))
                            {
                                return new ActionResult(GetText(7650, "Endast en prislista kan vara aktiv för en återförsäljare") + ": " + GetSysWholesellerDTO(chainSysWholesellerId)?.Name);
                            }
                        }
                    }
                }

                #region Remove all CompanyWholeSellerPriceLists that is not used anymore

                foreach (var companyPriceListIdToRemove in companyPriseListIdsToRemove)
                {
                    var priceListtoDelete = existingCompanyPriceLists.FirstOrDefault(x => x.CompanyWholesellerPriceListId == companyPriceListIdToRemove);
                    if (priceListtoDelete != null)
                    {
                        if (!priceListtoDelete.PriceRule.IsLoaded)
                            priceListtoDelete.PriceRule.Load();

                        if (priceListtoDelete.PriceRule.Any())
                        {
                            return new ActionResult(GetText(8103, "Ta bort prisregler kopplade till de prislistor som tas bort."));
                        }

                        result = DeleteEntityItem(entities, priceListtoDelete);
                        if (!result.Success)
                        {
                            return result;
                        }
                    }
                }


                #endregion

                #region Save entities

                foreach (var companyPriceListItem in companyPriceListItemsToAdd)
                {
                    if (ExistsCompanyWholesellerPriceLists(entities, companyPriceListItem.SysPriceListHeadId, companyPriceListItem.SysWholesellerId, actorCompanyId))
                        continue;

                    var newCompanyPriceList = new CompanyWholesellerPricelist
                    {
                        SysPriceListHeadId = companyPriceListItem.SysPriceListHeadId,
                        SysWholesellerId = companyPriceListItem.SysWholesellerId,
                        Company = company,
                    };

                    result = AddEntityItem(entities, newCompanyPriceList, "CompanyWholesellerPricelist");
                    if (!result.Success)
                    {
                        return result;
                    }
                }

                #endregion
            }

            return result;
        }

        public ActionResult DeleteCompanyWholesellerPriceList(CompEntities entities, int companyWholesellerPriceListId, int actorCompanyId)
        {
            var companyWholeSellerPriceList = GetCompanyWholesellerPriceList(entities, companyWholesellerPriceListId, actorCompanyId);
            if (companyWholeSellerPriceList == null)
                return new ActionResult((int)ActionResultDelete.EntityNotFound, "CompanyWholeSellerPriceList");

            return DeleteEntityItem(entities, companyWholeSellerPriceList);
        }

        #endregion

        #region CompanyWholeSellerPriceListView

        public List<CompanyWholesellerPriceListSmallDTO> GetCompanySysWholesellerPriceLists(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanySysWholesellerPriceLists(entities, actorCompanyId);
        }
        public List<CompanyWholesellerPriceListSmallDTO> GetCompanySysWholesellerPriceLists(CompEntities entities, int actorCompanyId)
        {
            var activePriceLists = entities.CompanyWholeSellerPriceListUsedView.Where(p => p.ActorCompanyId == actorCompanyId).Select(x => new { x.SysPriceListHeadId, x.SysWholesellerId }).ToList();
            var activePriceListHeadsIds = activePriceLists.Select(x => x.SysPriceListHeadId).ToList();
            List<SysPriceListHead> sysPricelistsHeads;

            using (var sysEntity = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    sysPricelistsHeads = sysEntity.SysPriceListHead.Where(x => activePriceListHeadsIds.Contains(x.SysPriceListHeadId)).ToList();
                }
            }

            var priceListSmallDTOs = activePriceLists.Select(x => new CompanyWholesellerPriceListSmallDTO
            {
                SysPriceListHeadId = x.SysPriceListHeadId,
                SysWholesellerId = x.SysWholesellerId,
                Provider = sysPricelistsHeads.FirstOrDefault(s => s.SysPriceListHeadId == x.SysPriceListHeadId)?.Provider ?? 0,
            }).ToList();

            return priceListSmallDTOs.Where(x => x.Provider != 0).ToList();
        }

        public List<CompanyWholesellerPriceListViewDTO> GetCompanyWholesellerPriceLists(int actorCompanyId, bool? isUsed = null, bool onlySysPricesList = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanyWholesellerPriceLists(entities, actorCompanyId, isUsed, onlySysPricesList);
        }

        public List<CompanyWholesellerPriceListViewDTO> GetCompanyWholesellerPriceLists(CompEntities entities, int actorCompanyId, bool? isUsed = null, bool onlySysPricesList = false)
        {
            var result = new List<CompanyWholesellerPriceListViewDTO>();

            var netText = GetText(7632, "Netto");
            var systemText = GetText(1371, "System");

            var sysCountryId = base.GetCompanySysCountryIdFromCache(entities, actorCompanyId, TermGroup_Languages.Unknown);
            var chainAffiliation = (TermGroup_ChainAffiliation)SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ChainAffiliation, UserId, actorCompanyId, 0);

            using (var sysEntity = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var sysPriceListsQuery = (from pl in sysEntity.SysPriceListHeadView
                                              select pl);

                    if (sysCountryId != (int)TermGroup_Languages.Unknown)
                    {
                        sysPriceListsQuery = sysPriceListsQuery.Where(pl => pl.WholesellerSysCountryId == sysCountryId);
                    }

                    foreach (var sysPriceList in sysPriceListsQuery.ToList())
                    {
                        if (!SysPriceListManager.ValidateChainAffilation((SoeSysPriceListProvider)sysPriceList.Provider, chainAffiliation))
                        {
                            continue;
                        }

                        var dto = new CompanyWholesellerPriceListViewDTO
                        {
                            SysPriceListHeadId = sysPriceList.SysPriceListHeadId,
                            ActorCompanyId = actorCompanyId,
                            Date = sysPriceList.Date,
                            Version = sysPriceList.Version,
                            SysWholesellerCountry = Enum.IsDefined(typeof(TermGroup_Country), sysPriceList.WholesellerSysCountryId) ? (TermGroup_Country)sysPriceList.WholesellerSysCountryId : TermGroup_Country.SE,
                            IsUsed = false,
                            PriceListOrigin = PriceListOrigin.SysDbPriceList,
                            Provider = sysPriceList.Provider,
                            SysWholesellerId = sysPriceList.SysWholesellerId,
                            SysWholesellerName = sysPriceList.Wholesellername,
                            PriceListName = Enum.GetName(typeof(SoeSysPriceListProvider), sysPriceList.Provider),
                            TypeName = systemText
                        };

                        if (string.IsNullOrEmpty(dto.PriceListName))
                        {
                            dto.PriceListName = GetText(2244, "Borttagen");
                        }

                        result.Add(dto);
                    }
                }
            }

            var priceLists = entities.CompanyWholeSellerPriceListUsedView.Where(p => p.ActorCompanyId == actorCompanyId).ToList();

            var sysWholesellers = SysPriceListManager.GetSysWholesellersDict();
            foreach (var item in priceLists)
            {
                CompanyWholesellerPriceListViewDTO dto = item.PriceListOrigin == 1 ? result.FirstOrDefault(x => x.SysPriceListHeadId == item.SysPriceListHeadId) : null;
                if (dto == null && item.PriceListOrigin.GetValueOrDefault() == (int)PriceListOrigin.CompDbPriceList && !onlySysPricesList)
                {
                    SysWholeseller sysWholeseller = null;
                    sysWholesellers.TryGetValue(item.SysWholesellerId, out sysWholeseller);

                    dto = item.ToDTO();
                    dto.SysWholesellerName = sysWholeseller?.Name;
                    dto.PriceListName = sysWholeseller?.Name + " (" + netText + ")";
                    dto.SysWholesellerCountry = sysWholeseller != null && Enum.IsDefined(typeof(TermGroup_Country), sysWholeseller.SysCountryId) ? (TermGroup_Country)sysWholeseller.SysCountryId : TermGroup_Country.SE;
                    dto.TypeName = GetText(1803, "Import");
                    result.Add(dto);
                }
                else if (dto != null && item.PriceListOrigin.GetValueOrDefault() == (int)PriceListOrigin.SysDbPriceList)
                {
                    dto.IsUsed = true;
                    dto.CompanyWholesellerPriceListId = item.CompanyWholesellerPriceListId;
                    dto.CompanyWholesellerId = dto.SysWholesellerId;
                }
            }

            if (isUsed.HasValue)
            {
                result = result.Where(pl => pl.IsUsed == isUsed.Value).ToList();
            }

            foreach (var dto in result.Where(d => d.IsUsed && (d.PriceListOrigin != PriceListOrigin.CompDbPriceList && d.PriceListOrigin != PriceListOrigin.CompDbNetPriceList)).ToList())
            {
                dto.HasNewerVersion = result.Any(i => i.SysWholesellerId == dto.SysWholesellerId && i.Provider == dto.Provider && (i.Date > dto.Date || i.Version > dto.Version));
            }

            if (!onlySysPricesList)
            {
                var imporText = GetText(1803, "Import");
                //show pricelist from new net price handling...but they can exist multiple time if added for specifick sales price lists....
                var netPrices = WholsellerNetPriceManager.GetWholsellerNetPrices(entities, actorCompanyId, WholsellerNetPriceManager.WholesellersWithCompleteNetPriceList()).DistinctBy(y => y.SysWholeSellerId).Select(x => new CompanyWholesellerPriceListViewDTO
                {
                    Date = x.Date,
                    IsUsed = true,
                    PriceListOrigin = PriceListOrigin.CompDbNetPriceList,
                    SysWholesellerId = x.SysWholeSellerId,
                    Version = 0,
                    TypeName = imporText,
                }).ToList();

                if (netPrices.Any())
                {
                    foreach (var dto in netPrices)
                    {
                        SysWholeseller sysWholeseller = null;
                        sysWholesellers.TryGetValue(dto.SysWholesellerId, out sysWholeseller);
                        dto.SysWholesellerName = sysWholeseller?.Name;
                        dto.PriceListName = sysWholeseller?.Name;
                        dto.SysWholesellerCountry = sysWholeseller != null && Enum.IsDefined(typeof(TermGroup_Country), sysWholeseller.SysCountryId) ? (TermGroup_Country)sysWholeseller.SysCountryId : TermGroup_Country.SE;
                    }

                    result.AddRange(netPrices);
                }
            }

            return result;
        }

        /// <summary>
        /// Used for hidden check if to display a notification to the company to ease their update process..
        /// </summary>
        /// <param name="actorCompanyId"></param>
        /// <returns>bool if to display an update notification</returns>
        public bool HasCompanyPriceListUpdates(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return HasCompanyPriceListUpdates(entities, actorCompanyId);
        }

        public bool HasCompanyPriceListUpdates(CompEntities entities, int actorCompanyId)
        {
            var priceListsItems = GetCompanyWholesellerPriceLists(entities, actorCompanyId, null, true);

            return (from view in priceListsItems
                    where
                        view.IsUsed && view.HasNewerVersion
                    select view).Any();
        }

        public List<CompanyWholesellerPriceListViewDTO> GetCompanyWholesellerPriceListsToUpdate(int actorCompanyId)
        {
            var priceListsItems = GetCompanyWholesellerPriceLists(actorCompanyId, null, true);

            var usedPriceListsItems = (from view in priceListsItems
                                       where view.IsUsed && view.HasNewerVersion
                                       select view).ToList();

            return usedPriceListsItems;
        }

        public ActionResult UpgradeCompanyWholeSellerPriceLists(int actorCompanyId, List<int> sysWholesellerIds)
        {
            ActionResult result = new ActionResult();

            int success = 0;
            int notSuccess = 0;

            foreach (var sysWholesellerId in sysWholesellerIds)
            {
                result = UpgradeCompanyWholeSellerPriceLists(actorCompanyId, null, sysWholesellerId);

                if (result.Success)
                    success++;
                else
                    notSuccess++;
            }

            result.IntegerValue = success;
            result.IntegerValue2 = notSuccess;

            return result;
        }

        /// <summary>
        /// Upgrades old version to new, changes price rules, prices and pricelists
        /// </summary>
        /// <param name="actorCompanyId"></param>
        /// <param name="sysPricelistHeadId"></param>
        /// <param name="sysWholesellerId"></param>
        /// <returns></returns>
        public ActionResult UpgradeCompanyWholeSellerPriceLists(int actorCompanyId, int? sysPricelistHeadId = null, int? sysWholesellerId = null)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        var companyPriceListItems = GetCompanyWholesellerPriceLists(entities, actorCompanyId, null, true);

                        // Get used pricelists
                        var usedCompanyPriceListItems = (from cpl in companyPriceListItems
                                                         where cpl.IsUsed &&
                                                         (!sysPricelistHeadId.HasValue || sysPricelistHeadId.Value == cpl.SysPriceListHeadId) &&
                                                         (!sysWholesellerId.HasValue || sysWholesellerId.Value == cpl.SysWholesellerId)
                                                         select cpl).ToList();

                        #endregion

                        var movedCompanyPriceListIds = new List<int>();

                        foreach (var usedPriceList in usedCompanyPriceListItems)
                        {
                            #region CompanyWholesellerPricelist

                            // Get unused pricelists
                            var unusedCompanyPriceListItem = (from pl in companyPriceListItems
                                                              where !pl.IsUsed &&
                                                              pl.Provider == usedPriceList.Provider &&
                                                              pl.SysWholesellerId == usedPriceList.SysWholesellerId &&
                                                              (pl.Date > usedPriceList.Date || pl.Version > usedPriceList.Version)
                                                              orderby pl.Date descending
                                                              select pl).FirstOrDefault();

                            if (unusedCompanyPriceListItem == null)
                                continue;
                            if (unusedCompanyPriceListItem.SysPriceListHeadId == 0 || unusedCompanyPriceListItem.SysWholesellerId == 0)
                                continue;

                            //Add pricelist
                            var newCompanyPriceList = new CompanyWholesellerPricelist
                            {
                                SysPriceListHeadId = unusedCompanyPriceListItem.SysPriceListHeadId,
                                SysWholesellerId = unusedCompanyPriceListItem.SysWholesellerId,

                                //Set references
                                Company = company,
                            };

                            result = AddEntityItem(entities, newCompanyPriceList, "CompanyWholesellerPricelist");
                            if (!result.Success)
                                return result;

                            #endregion

                            #region PriceRules

                            if (usedPriceList.CompanyWholesellerPriceListId.HasValue)
                            {
                                // Get source pricerules
                                var sourcePriceRules = PriceRuleManager.GetPriceRules(entities, usedPriceList.CompanyWholesellerPriceListId.Value, company.ActorCompanyId);

                                // Copy to target pricelist
                                result = PriceRuleManager.CopyPriceRules(entities, sourcePriceRules, company, newCompanyPriceList);
                                if (!result.Success)
                                    return result;

                                // Delete source pricerules
                                result = PriceRuleManager.DeletePriceRules(entities, sourcePriceRules, company.ActorCompanyId);
                                if (!result.Success)
                                    return result;
                            }

                            #endregion

                            #region Product Prices

                            //When upgrading a new product needs to be created in comp and the company refrence to the old product should point to the new one
                            if (!company.Product.IsLoaded)
                                company.Product.Load();

                            //Update in-price
                            var invoiceProducts = ProductManager.GetInvoiceProductsBySysPriceList(entities, company.ActorCompanyId, usedPriceList.SysPriceListHeadId);

                            foreach (var invoiceProduct in invoiceProducts)
                            {
                                invoiceProduct.ExternalPriceListHeadId = newCompanyPriceList.SysPriceListHeadId;
                            }

                            #region Depricated - We do not need to update the product "in-prices"


                            //foreach (var invoiceProduct in invoiceProducts)
                            //{
                            //    #region InvoiceProduct

                            //    var productPrices = (from search in entities.InvoiceProductPriceSearchView
                            //                         where search.ActorCompanyId == company.ActorCompanyId &&
                            //                         search.ProductId == invoiceProduct.ExternalProductId.Value &&
                            //                         search.SysPriceListHeadId == invoiceProduct.ExternalPriceListHeadId.Value
                            //                         select search).ToList();

                            //    if (productPrices.Count > 0)
                            //    {

                            //        //Only update price and sysreferences
                            //        var newPrice = PriceRuleManager.ApplyPriceRule(entities, invoiceProduct, newCompanyPriceList, companyPriceListTypeId, 0, 0, company.ActorCompanyId);
                            //        invoiceProduct.PurchasePrice = newPrice.NetPrice > 0 ? newPrice.NetPrice : invoiceProduct.PurchasePrice;
                            //        invoiceProduct.SalesPrice = newPrice.Value > 0 ? newPrice.Value : invoiceProduct.SalesPrice;

                            //        #endregion

                            //        invoiceProduct.ExternalPriceListHeadId = newCompanyPriceList.SysPriceListHeadId;
                            //    }
                            //    else
                            //    {
                            //        // Remove relation to legacy product (not done anymore)
                            //        //company.Product.Remove(invoiceProduct);
                            //    }
                            //}

                            #endregion

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;

                            #endregion

                            if (usedPriceList.CompanyWholesellerPriceListId.HasValue && !movedCompanyPriceListIds.Contains(usedPriceList.CompanyWholesellerPriceListId.Value))
                                movedCompanyPriceListIds.Add(usedPriceList.CompanyWholesellerPriceListId.Value);
                        }

                        #region Delete old CompanyWholesellerPriceLists

                        foreach (var companyWholesellerPriceListId in movedCompanyPriceListIds)
                        {
                            result = DeleteCompanyWholesellerPriceList(entities, companyWholesellerPriceListId, company.ActorCompanyId);
                            if (!result.Success)
                                return result;
                        }

                        movedCompanyPriceListIds.Clear();

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    LogError(ex, log);
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion

        #region SysWholeseller

        public Dictionary<int, string> GetSysWholesellersByCompanyDict(int actorCompanyId, bool onlyNotUsed, bool addEmptyRow)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyWholesellerPricelist.NoTracking();
            var usedSysWholesellerIds = (from cw in entities.CompanySysWholeseller
                                         where cw.ActorCompanyId == actorCompanyId
                                         select cw.SysWholesellerId).ToList();

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var sysWholesellers = (from sw in sysEntitiesReadOnly.SysWholeseller
                                   where sw.SysCountryId == company.SysCountryId &&
                                   !usedSysWholesellerIds.Contains(sw.SysWholesellerId) &&
                                   sw.IsOnlyInComp == false
                                   orderby sw.Name
                                   select sw);

            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            foreach (var wholeseller in sysWholesellers)
            {
                if (!dict.ContainsKey(wholeseller.SysWholesellerId))
                    dict.Add(wholeseller.SysWholesellerId, wholeseller.Name);
            }

            return dict;
        }

        public List<SysWholeseller> GetSysWholesellersByCompany(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSysWholesellersByCompany(entities, actorCompanyId);
        }

        public List<SysWholeseller> GetSysWholesellersByCompany(CompEntities entities, int actorCompanyId)
        {
            var companyPriceList = WholeSellerManager.GetCompanyWholesellerPriceLists(entities, actorCompanyId, true);
            var sysWholesellers = companyPriceList.Select(s => new SysWholeseller
            {
                SysWholesellerId = s.SysWholesellerId,
                Name = s.SysWholesellerName ?? ""
            }).GroupBy(g => g.SysWholesellerId).Select(s => s.First()).ToList();

            return sysWholesellers;
        }

        public bool TryGetSysWholesellerIdByName(string wholeSellerName, ref int wholeSellerId)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                var q = from entry in sysEntitiesReadOnly.SysWholeseller
                        where
                        entry.Name == wholeSellerName
                        select entry.SysWholesellerId;

                int id = q.FirstOrDefault();

                if (id > 0)
                {
                    wholeSellerId = id;
                }
                else if (wholeSellerName.ToLower() == "elektroskandia")
                {
                    TryGetSysWholesellerIdByName("Sonepar", ref wholeSellerId);
                }

                return id > 0;
            }
        }


        public Dictionary<int, string> GetSysWholesellerDict()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                return (from sysWholeseller in sysEntitiesReadOnly.SysWholeseller
                        select sysWholeseller).ToDictionary(x => x.SysWholesellerId, y => y.Name);
            }
        }

        public SysWholesellerDTO GetSysWholesellerDTO(int sysWholesellerId)
        {
            if (sysWholesellerId <= 0)
                return null;

            string cacheKey = $"GetSysWholesellerDTO#sysWholesellerId{sysWholesellerId}";
            SysWholesellerDTO wholesellerDTO = BusinessMemoryCache<SysWholesellerDTO>.Get(cacheKey);

            if (wholesellerDTO == null)
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                    var wholeseller = (from sysWholeseller in sysEntitiesReadOnly.SysWholeseller
                                       where sysWholeseller.SysWholesellerId == sysWholesellerId
                                       select sysWholeseller).FirstOrDefault();

                    if (wholeseller != null)
                    {
                        wholesellerDTO = wholeseller.ToDTO();
                        BusinessMemoryCache<SysWholesellerDTO>.Set(cacheKey, wholesellerDTO, 15 * 60);
                    }
                }
            }

            return wholesellerDTO;
        }

        public SysWholeseller GetSysWholeseller(int sysWholesellerId, bool loadSysWholesellerEdi, bool loadSysEdiMsg, bool loadSysEdiType)
        {
            if (sysWholesellerId <= 0)
                return null;

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<SysWholeseller> query = sysEntitiesReadOnly.Set<SysWholeseller>();
            if (loadSysEdiType)
                query = query.Include("SysWholesellerEdi.SysEdiMsg.SysEdiType");
            if (loadSysEdiMsg)
                query = query.Include("SysWholesellerEdi.SysEdiMsg");
            if (loadSysWholesellerEdi)
                query = query.Include("SysWholesellerEdi");

            return (from wholeseller in query
                    where wholeseller.SysWholesellerId == sysWholesellerId
                    select wholeseller).FirstOrDefault();
        }

        public SysWholesellerEdi GetSysWholesellerEdi(int sysWholesellerEdiId, bool loadSysEdiMsg = false, bool loadSysWholeseller = false)
        {
            if (sysWholesellerEdiId <= 0)
                return null;

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<SysWholesellerEdi> query = sysEntitiesReadOnly.Set<SysWholesellerEdi>();
            if (loadSysEdiMsg)
                query = query.Include("SysEdiMsg");
            if (loadSysWholeseller)
                query = query.Include("SysWholeseller");

            return (from wholeseller in query
                    where wholeseller.SysWholesellerEdiId == sysWholesellerEdiId
                    select wholeseller).FirstOrDefault();
        }

        public SortedDictionary<int, string> GetWholesellersDictSorted(int actorCompanyId, bool addEmptyRow)
        {
            var dict = new SortedDictionary<int, string>();
            PopulateWholesellerDict(dict, actorCompanyId, addEmptyRow);
            return dict;
        }

        //Duplicate method because Silverlight cant handle SortedDictionary

        public Dictionary<int, string> GetWholesellerDictByCompany(int actorCompanyId, bool addEmptyRow)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetWholesellerDictByCompany(entities, actorCompanyId, addEmptyRow);
        }
        public Dictionary<int, string> GetWholesellerDictByCompany(CompEntities entities, int actorCompanyId, bool addEmptyRow)
        {
            var dict = new Dictionary<int, string>();
            PopulateWholesellerDict(entities, dict, actorCompanyId, addEmptyRow);
            return dict;
        }

        private void PopulateWholesellerDict(IDictionary<int, string> dict, int actorCompanyId, bool addEmptyRow)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            PopulateWholesellerDict(entitiesReadOnly, dict, actorCompanyId, addEmptyRow);
        }

        private void PopulateWholesellerDict(CompEntities entities, IDictionary<int, string> dict, int actorCompanyId, bool addEmptyRow)
        {
            if (addEmptyRow)
                dict.Add(0, " ");

            var wholeSellers = GetSysWholesellersByCompany(entities, actorCompanyId);
            foreach (var wholeSeller in wholeSellers)
            {
                if (!dict.ContainsKey(wholeSeller.SysWholesellerId))
                    dict.Add(wholeSeller.SysWholesellerId, wholeSeller.Name);
            }
        }

        public string GetWholesellerName(int sysWholesellerId)
        {
            return GetSysWholesellerDTO(sysWholesellerId)?.Name ?? string.Empty;
        }
        public SysWholesellerDTO GetWholesellerFromComfortReference(string reference)
        {
            if (string.IsNullOrEmpty(reference))
            {
                log.Error("Invalid comfort wholeseller reference: " + reference, null);
                return null;
            }

            //should be like  Ahlsell Sverige AB|4592387403|5560129206|
            var info = reference.Split('|');
            var orgNr = info.Length > 2 ? info[2] : string.Empty;
            if (string.IsNullOrEmpty(orgNr))
            {
                log.Error("Invalid comfort wholeseller reference: " + reference, null);
                return null;
            }

            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                var sysWholeSellerId = (from sysWholeseller in sysEntitiesReadOnly.SysWholeseller
                                        where sysWholeseller.OrgNr == orgNr
                                        select sysWholeseller.SysWholesellerId).FirstOrDefault();

                if (sysWholeSellerId > 0)
                {
                    return GetSysWholesellerDTO(sysWholeSellerId);
                }
            }

            return null;
        }

        #endregion

        #region CompanyWholeseller

        public IEnumerable<CompanyWholesellerListDTO> GetCompanyWholesellerDTOs(int actorCompanyId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var connections = (from entry in entitiesReadOnly.CompanySysWholeseller
                               where entry.ActorCompanyId == actorCompanyId
                               select entry).ToList();

            var wholesellerIds = connections.Select(c => c.SysWholesellerId).ToList();

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var wholesellers = (from entry in sysEntitiesReadOnly.SysWholeseller
                                where wholesellerIds.Contains(entry.SysWholesellerId)
                                select entry).ToList();

            var retList = (from conn in connections
                           join ws in wholesellers on conn.SysWholesellerId equals ws.SysWholesellerId
                           select new CompanyWholesellerListDTO()
                           {
                               CompanySysWholesellerDtoId = conn.CompanySysWholesellerId,
                               Name = ws.Name,
                           });

            return retList;
        }

        public CompanyWholesellerDTO GetCompanyWholesellerDTO(int companySysWholesellerId, bool isSupportAdmin = false)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var connection = (from entry in entitiesReadOnly.CompanySysWholeseller.Include("Company.EdiConnection")
                              where entry.CompanySysWholesellerId == companySysWholesellerId
                              select entry).FirstOrDefault();

            if (connection == null)
                return null;

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var ws = (from entry in sysEntitiesReadOnly.SysWholeseller.Include("SysWholesellerEdi.SysEdiMsg.SysEdiType")
                      where entry.SysWholesellerId == connection.SysWholesellerId
                      select entry).FirstOrDefault();

            var dto = new CompanyWholesellerDTO()
            {
                CompanySysWholesellerDtoId = connection.CompanySysWholesellerId,
                Name = ws.Name,
                SysWholesellerId = ws.SysWholesellerId,
                Wholeseller = ws.ToDTO(),
                SysWholesellerEdiId = ws.SysWholesellerEdiId ?? 0,
                HasEdiFeature = ws.SysWholesellerEdi != null && ws.SysWholesellerEdi.SysEdiMsg.Count > 0,
                Created = connection.Created,
                CreatedBy = connection.CreatedBy,
                Modified = connection.Modified,
                ModifiedBy = connection.ModifiedBy,
            };

            if (dto.HasEdiFeature)
            {
                dto.UseEdi = connection.Company.EdiConnection.Count > 0;
                dto.EdiConnections = connection.Company.EdiConnection.Where(e => ws.SysWholesellerEdi.SysEdiMsg.Any(m => e.SysEdiMsgId == m.SysEdiMsgId)).ToDTOs().ToList();
                dto.MessageTypes = ws.SysWholesellerEdi.SysEdiMsg.Select(m => m.SysEdiType.TypeName).Distinct().JoinToString(",");
                if (isSupportAdmin)
                    dto.EdiWholesellerSenderNrs = ws.SysWholesellerEdi.SysEdiMsg.Select(m => m.SenderSenderNr).Distinct().JoinToString(",");
            }

            return dto;
        }

        #endregion

        public ActionResult SaveCompanyWholesellerDTO(int actorCompanyId, CompanyWholesellerDTO dto, List<string> customerNbrs, int actorSupplierId)
        {
            ActionResult result = null;

            string errMessage = string.Empty;

            result = SupplierManager.MapSupplierToSysWholeseller(actorSupplierId, dto.SysWholesellerId, base.ActorCompanyId);
            if (!result.Success)
                errMessage = GetText(0, "");

            using (var entities = new CompEntities())
            {
                result = this.SaveCompanySysWholeseller(entities, actorCompanyId, dto.SysWholesellerId);
                if (result.Success)
                {
                    if (errMessage != string.Empty)
                        result.ErrorMessage = errMessage;

                    if (!customerNbrs.Any())
                        return result;

                    result = this.EdiManager.AddEdiConnections(entities, actorCompanyId, dto.SysWholesellerEdiId, customerNbrs.ToArray());
                }
                else
                {
                    if (errMessage != string.Empty)
                        result.ErrorMessage += '\n' + errMessage;
                }

                return result;
            }
        }

        public ActionResult SaveCompanySysWholeseller(CompEntities entities, int actorCompanyId, int sysWholesellerId)
        {
            var result = new ActionResult();
            // Check if connection exists
            var compWholeseller = (from entry in entities.CompanySysWholeseller
                                   where entry.ActorCompanyId == actorCompanyId &&
                                   entry.SysWholesellerId == sysWholesellerId
                                   select entry).FirstOrDefault();

            if (compWholeseller == null)
            {
                compWholeseller = new CompanySysWholeseller()
                {
                    ActorCompanyId = actorCompanyId,
                    SysWholesellerId = sysWholesellerId,
                };

                this.SetCreatedProperties(compWholeseller);

                entities.CompanySysWholeseller.AddObject(compWholeseller);
                result = SaveChanges(entities);
                result.IntegerValue = compWholeseller.CompanySysWholesellerId;
            }
            else
            {
                // Update modified even thought we are not modifying anything yet (connections might be modified)
                this.SetModifiedProperties(compWholeseller);
            }

            return result;
        }

        public ActionResult DeleteCompanyWholesellerDTO(int actorCompanyId, int sysWholesellerId)
        {
            var result = new ActionResult();
            using (var entities = new CompEntities())
            {
                var company = CompanyManager.GetCompany(entities, actorCompanyId, loadEdiConnection: true);

                var compWholeseller = (from entry in entities.CompanySysWholeseller
                                       where entry.ActorCompanyId == actorCompanyId &&
                                       entry.SysWholesellerId == sysWholesellerId
                                       select entry).FirstOrDefault();

                if (compWholeseller != null)
                {
                    result = DeleteEntityItem(entities, compWholeseller);
                    if (!result.Success)
                        return result;
                }

                var sysWholeSeller = GetSysWholesellerDTO(sysWholesellerId);

                if (sysWholeSeller.SysWholesellerEdiId.HasValue)
                {
                    result = EdiManager.DeleteEdiConnections(entities, company, sysWholeSeller.SysWholesellerEdiId.Value);
                }
            }

            return result;
        }

        public int GetMostRecentCompanyWholesellerPriceListEx(CompEntities entities, int actorCompanyId, ref int sysWholesellerId)
        {

            int sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(entities, sysWholesellerId, actorCompanyId);
            // Alvesta has been merged into Solar.
            // If product not found in Solars pricelist, check Alvestas pricelist.
            if (sysPriceListHeadId == 0 && sysWholesellerId == 5)
            {
                sysWholesellerId = 9;
                sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(entities, sysWholesellerId, actorCompanyId);
            }
            // Ahlsell have three pricelists (2, 14 and 15) check them all
            if (sysPriceListHeadId == 0 && sysWholesellerId == 2)
            {
                sysWholesellerId = 14;
                sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(entities, sysWholesellerId, actorCompanyId);
            }
            if (sysPriceListHeadId == 0 && sysWholesellerId == 14)
            {
                sysWholesellerId = 15;
                sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(entities, sysWholesellerId, actorCompanyId);
            }
            // Storel have three pricelists (7, 18 and 19) check them all
            if (sysPriceListHeadId == 0 && sysWholesellerId == 7)
            {
                sysWholesellerId = 18;
                sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(entities, sysWholesellerId, actorCompanyId);
            }
            if (sysPriceListHeadId == 0 && sysWholesellerId == 18)
            {
                sysWholesellerId = 19;
                sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(entities, sysWholesellerId, actorCompanyId);
            }
            // Lunda have three pricelists (20, 21 and 22) check them all
            if (sysPriceListHeadId == 0 && sysWholesellerId == 20)
            {
                sysWholesellerId = 21;
                sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(entities, sysWholesellerId, actorCompanyId);
            }
            if (sysPriceListHeadId == 0 && sysWholesellerId == 21)
            {
                sysWholesellerId = 22;
                sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(entities, sysWholesellerId, actorCompanyId);
                if (sysPriceListHeadId == 0)
                {
                    sysWholesellerId = 20; //keep 20 as original but 21 and 22 should probably be removed in the furture
                }
            }
            if (sysPriceListHeadId == 0 && sysWholesellerId == 37)
            {
                sysWholesellerId = 41;
                sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListSysPriceListHeadId(entities, sysWholesellerId, actorCompanyId);
            }

            return sysPriceListHeadId;
        }
    }
}

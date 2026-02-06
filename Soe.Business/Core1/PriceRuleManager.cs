using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class PriceRuleManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public PriceRuleManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region CompanyPriceRule

        public List<CompanyPriceRuleDTO> GetCompanyPriceRules(int actorCompanyId)
        {

            var dtos = new List<CompanyPriceRuleDTO>();

            using (var entities = new CompEntities())
            {
                var rules = entities.GetCompanyPriceRules(actorCompanyId).ToList();

                if (rules.Any())
                {
                    var sysPriceListHeadsId = rules.Select(x => x.SysPriceListHeadId).Where(y => y > 0).Distinct().ToList();
                    var sysPriceListHeads = SysPriceListManager.GetSysPriceListHeads(sysPriceListHeadsId);

                    foreach (var rule in rules)
                    {
                        var dto = rule.ToDTO();

                        if (rule.SysPriceListHeadId > 0)
                        {
                            var sysPriceList = sysPriceListHeads.FirstOrDefault(x => x.SysPriceListHeadId == rule.SysPriceListHeadId);
                            if (sysPriceList != null)
                            {
                                dto.Date = sysPriceList.Date;
                            }
                        }

                        if (rule.WholesellerPricelistWholsellerId > 0)
                        {
                            dto.SysWholesellerName = SysPriceListManager.GetSysWholesellerFromCache(rule.WholesellerPricelistWholsellerId)?.Name;
                        }
                        else if (rule.PriceListImportedWholsellerId > 0)
                        {
                            dto.SysWholesellerName = SysPriceListManager.GetSysWholesellerFromCache(rule.PriceListImportedWholsellerId)?.Name + " (" + GetText(7632,"Netto") + ")";
                        }
                        else
                        {
                            dto.SysWholesellerName = "Generell";
                        }
                        dtos.Add(dto);
                    }
                }
            }


            return dtos;
        }

        #endregion

        #region PriceRule

        public List<PriceRule> GetPriceRules(CompEntities entities, int companyWholesellerPriceListId, int actorCompanyId)
        {
            return (from rules in entities.PriceRule
                        .Include("LRule")
                        .Include("RRule")
                        .Include("PriceListType")
                    where rules.Company.ActorCompanyId == actorCompanyId &&
                    rules.CompanyWholesellerPricelist.CompanyWholesellerPriceListId == companyWholesellerPriceListId &&
                    rules.PriceListType.State == (int)SoeEntityState.Active
                    select rules).ToList();
        }

        public List<PriceRule> GetPriceRules(int actorCompanyId, bool onlyGeneral = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceRule.NoTracking();
            return GetPriceRules(entities, actorCompanyId, onlyGeneral);
        }

        public List<PriceRule> GetPriceRules(CompEntities entities, int actorCompanyId, bool onlyGeneral = false)
        {
            return (from r in entities.PriceRule
                        .Include("LRule")
                        .Include("RRule")
                        .Include("PriceListType")
                    where r.Company.ActorCompanyId == actorCompanyId &&
                    (onlyGeneral == true ||  r.CompanyWholesellerPricelist != null) &&
                    r.PriceListType.State == (int)SoeEntityState.Active
                    select r).ToList();
        }

        public PriceRule GetPriceRule(int priceRuleId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceRule.NoTracking();
            return GetPriceRule(entities, priceRuleId, actorCompanyId);
        }

        public PriceRule GetPriceRule(CompEntities entities, int priceRuleId, int actorCompanyId)
        {
            PriceRule rule = (from pr in entities.PriceRule
                                  .Include("LRule")
                                  .Include("RRule")
                                  .Include("PriceListType")
                                  .Include("CompanyWholesellerPricelist")
                              where pr.Company.ActorCompanyId == actorCompanyId &&
                              pr.RuleId == priceRuleId
                              select pr).FirstOrDefault();

            if (rule != null)
            {
                LoadRecursive(rule);

                // Error checking, a rule must have a right and a left value or left and right rule linked
                if ((rule.LRule == null && rule.RRule == null))
                {
                    // If not both sides have values we have an error
                    if (!(rule.RValue.HasValue && rule.LValue.HasValue))
                    {
                        // Try to load the rule by it's pricelisttype and companywholesellerpricelist
                        rule = this.GetPriceRule(entities, rule.PriceListType.PriceListTypeId, rule.CompanyWholesellerPricelist == null ? 0 : rule.CompanyWholesellerPricelist.CompanyWholesellerPriceListId, actorCompanyId, false, priceListImportedHeadId: rule.PriceListImportedHeadId);
                    }
                }
            }

            return rule;
        }

        public PriceRule GetPriceRule(CompEntities entities, int priceListTypeId, int companyWholesellerPriceListId, int actorCompanyId, bool getGeneralPriceRuleIfNoResults, int? priceListImportedHeadId = null)
        {
            var query = from pr in entities.PriceRule
                                .Include("LRule")
                                .Include("RRule")
                                .Include("PriceListType")
                                .Include("CompanyWholesellerPricelist")
                        where pr.Company.ActorCompanyId == actorCompanyId &&
                        pr.PriceListType.PriceListTypeId == priceListTypeId
                        select pr;

            if (companyWholesellerPriceListId > 0)
                query = query.Where(pr => pr.CompanyWholesellerPricelist.CompanyWholesellerPriceListId == companyWholesellerPriceListId);
            if (priceListImportedHeadId.HasValue && priceListImportedHeadId > 0)
                query = query.Where(pr => pr.PriceListImportedHeadId == priceListImportedHeadId);

            //Execute query
            var priceRules = query.ToList();

            if (((companyWholesellerPriceListId == 0 && priceListImportedHeadId == 0) || priceRules.IsNullOrEmpty()) && getGeneralPriceRuleIfNoResults)
            {
                priceRules = (from pr in entities.PriceRule
                                .Include("LRule")
                                .Include("RRule")
                                .Include("PriceListType")
                                .Include("CompanyWholesellerPricelist")
                                .Include("PriceListImportedHead")
                              where pr.Company.ActorCompanyId == actorCompanyId &&
                              pr.PriceListType.PriceListTypeId == priceListTypeId &&
                              pr.CompanyWholesellerPricelist == null &&
                              pr.PriceListImportedHead == null
                              select pr).ToList();
            }

            PriceRule rule = null;

            if (!priceRules.IsNullOrEmpty())
            {
                foreach (PriceRule priceRule in priceRules)
                {
                    if (rule == null)
                        rule = priceRule;

                    if (priceRule.RRule != null || priceRule.LRule != null)
                        rule = priceRule;
                }
            }

            if (rule != null)
                LoadRecursive(rule);

            return rule;
        }

        public bool PriceRuleExists(CompEntities entities, int actorCompanyId, int priceListTypeId, int? companyWholesellerPriceListId, int? priceListImportedHeadId)
        {
            companyWholesellerPriceListId = companyWholesellerPriceListId.ToNullable();
            priceListImportedHeadId = priceListImportedHeadId.ToNullable();

            return (from pr in entities.PriceRule
                    where pr.Company.ActorCompanyId == actorCompanyId &&
                    pr.PriceListType.PriceListTypeId == priceListTypeId &&
                    (
                        (companyWholesellerPriceListId == pr.CompanyWholesellerPricelist.CompanyWholesellerPriceListId) ||
                        (companyWholesellerPriceListId == null && pr.CompanyWholesellerPricelist == null)
                    ) 
                    &&
                    (
                        (priceListImportedHeadId == pr.PriceListImportedHeadId) ||
                        (priceListImportedHeadId == null && pr.PriceListImportedHeadId == null)
                    )
                    select pr).Any();
        }

        public List<PriceListTypeMarkupDTO> GetPriceListTypeMarkups(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceRule.NoTracking();
            return GetPriceListTypeMarkupsNaive(entities, actorCompanyId);
        }

        /// <summary>
        /// This method will try to extract the markups used in the formulas for each price list type.
        /// It won't take into account any other rule types, as they in general are applied for wholeseller products,
        /// while we want to apply it for normal product rows.
        /// 
        /// It's not a perfect solution, but it's a start.
        /// </summary>
        public List<PriceListTypeMarkupDTO> GetPriceListTypeMarkupsNaive(CompEntities entities, int actorCompanyId)
        {
            var priceListTypeDTOs = new List<PriceListTypeMarkupDTO>();

            var priceListTypes = ProductPricelistManager.GetPriceListTypes(entities, actorCompanyId);
            priceListTypes.ForEach(plt =>
            {
                var markup = GetMarkupNaive(entities, actorCompanyId, plt.PriceListTypeId);
                priceListTypeDTOs.Add(new PriceListTypeMarkupDTO()
                {
                    PriceListTypeId = plt.PriceListTypeId,
                    Markup = markup
                });
            });

            return priceListTypeDTOs;
        }

        /// <summary>
        /// This method will try to extract the markup used in the formula.
        /// It won't take into account any other rule types, as they in general are applied for wholeseller products,
        /// while we want to apply it for normal product rows.
        /// 
        /// It's not a perfect solution, but it's a start.
        /// </summary>
        public decimal GetMarkupNaive(CompEntities entities, int actorCompanyId, int priceListTypeId)
        {
            var priceRules = (from pr in entities.PriceRule
                              where pr.PriceListType.PriceListTypeId == priceListTypeId
                                && pr.Company.ActorCompanyId == actorCompanyId
                              select pr).ToList();

            return CalculateMarkup(priceRules);
        }

        private decimal CalculateMarkup(List<PriceRule> priceRules)
        {
            var markup = 0m;
            var gainType = (int)PriceRuleItemType.Gain;
            // Prevent getting 1% markup if there are no price rules.
            if (priceRules.Any(r => r.RValueType == gainType || r.LValueType == gainType))
            {
                markup = priceRules
                    .Aggregate(1m, (acc, x) =>
                    {
                        if (x.LValueType == gainType)
                            return acc * x.LValue ?? 0;
                        if (x.RValueType == gainType)
                            return acc * x.RValue ?? 0;
                        return acc;
                    });
            }
            return markup != 0 ? markup / 100 : 0;
        }

        private void LoadRecursive(PriceRule rule)
        {
            if (rule == null)
                return;

            if (!rule.LRuleReference.IsLoaded)
                rule.LRuleReference.Load();
            if (rule.LRule != null)
                LoadRecursive(rule.LRule);

            if (!rule.RRuleReference.IsLoaded)
                rule.RRuleReference.Load();
            if (rule.RRule != null)
                LoadRecursive(rule.RRule);
        }

        public ActionResult AddPriceRule(PriceRuleDTO priceRuleDTO, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return AddPriceRule(entities, priceRuleDTO, company);
            }
        }

        public ActionResult AddPriceRule(CompEntities entities, PriceRuleDTO priceRuleDTO, Company company)
        {
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Company");

            // Delete existing
            if (PriceRuleExists(entities, company.ActorCompanyId, priceRuleDTO.PriceListTypeId, priceRuleDTO.CompanyWholesellerPriceListId, priceRuleDTO.PriceListImportedHeadId))
            {
                var existingPriceRule = GetPriceRule(entities, priceRuleDTO.PriceListTypeId, priceRuleDTO.CompanyWholesellerPriceListId.HasValue ? priceRuleDTO.CompanyWholesellerPriceListId.Value : 0, company.ActorCompanyId, true, priceRuleDTO.PriceListImportedHeadId.FromNullable());
                if (existingPriceRule == null)
                    return new ActionResult(false, (int)ActionResultSave.EntityNotFound, GetText(1948, "Hittade inte den ursprungliga prisregeln"));

                DeletePriceRuleRecursive(entities, existingPriceRule, company.ActorCompanyId);
            }

            PriceListType priceListType = ProductPricelistManager.GetPriceListType(entities, priceRuleDTO.PriceListTypeId, company.ActorCompanyId);
            if (priceListType == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "PriceListType");

            CompanyWholesellerPricelist companyWholesellerPriceList = null;
            if (priceRuleDTO.CompanyWholesellerPriceListId.HasValue)
                companyWholesellerPriceList = WholeSellerManager.GetCompanyWholesellerPriceList(entities, priceRuleDTO.CompanyWholesellerPriceListId.Value, company.ActorCompanyId);

            #region Left PriceRule

            PriceRule leftPriceRule = null;
            if (priceRuleDTO.LRuleId.HasValue)
            {
                leftPriceRule = GetPriceRule(entities, priceRuleDTO.LRuleId.Value, company.ActorCompanyId);
            }
            else if (priceRuleDTO.LRule != null)
            {
                leftPriceRule = new PriceRule()
                {
                    OperatorType = priceRuleDTO.LRule.OperatorType,
                    LValueType = priceRuleDTO.LRule.LValueType,
                    LValue = priceRuleDTO.LRule.LValue,
                    RValueType = priceRuleDTO.LRule.RValueType,
                    RValue = priceRuleDTO.LRule.RValue,
                    UseNetPrice = priceRuleDTO.LRule.UseNetPrice,

                    //Set references
                    PriceListType = priceListType,
                    CompanyWholesellerPricelist = companyWholesellerPriceList,
                    PriceListImportedHeadId = priceRuleDTO.PriceListImportedHeadId,
                    PriceListOrigin = priceRuleDTO.PriceListImportedHeadId.HasValue ? (int)PriceListOrigin.CompDbPriceList : (int)PriceListOrigin.SysDbPriceList,
                    Company = company,
                };
            }

            #endregion

            #region Right PriceRule

            PriceRule rightPriceRule = null;
            if (priceRuleDTO.RRuleId.HasValue)
            {
                rightPriceRule = GetPriceRule(entities, priceRuleDTO.RRuleId.Value, company.ActorCompanyId);
            }
            else if (priceRuleDTO.RRule != null)
            {
                rightPriceRule = new PriceRule()
                {
                    OperatorType = priceRuleDTO.RRule.OperatorType,
                    LValueType = priceRuleDTO.RRule.LValueType,
                    LValue = priceRuleDTO.RRule.LValue,
                    RValueType = priceRuleDTO.RRule.RValueType,
                    RValue = priceRuleDTO.RRule.RValue,
                    UseNetPrice = priceRuleDTO.RRule.UseNetPrice,

                    //Set references
                    PriceListType = priceListType,
                    CompanyWholesellerPricelist = companyWholesellerPriceList,
                    PriceListImportedHeadId = priceRuleDTO.PriceListImportedHeadId,
                    PriceListOrigin = priceRuleDTO.PriceListImportedHeadId.HasValue ? (int)PriceListOrigin.CompDbPriceList : (int)PriceListOrigin.SysDbPriceList,
                    Company = company,
                };
            }

            #endregion

            PriceRule priceRule = new PriceRule()
            {
                OperatorType = priceRuleDTO.OperatorType,
                LValueType = priceRuleDTO.LValueType,
                LValue = priceRuleDTO.LValue,
                RValueType = priceRuleDTO.RValueType,
                RValue = priceRuleDTO.RValue,
                UseNetPrice = priceRuleDTO.UseNetPrice,

                //Set references
                LRule = leftPriceRule,
                RRule = rightPriceRule,
                PriceListType = priceListType,
                CompanyWholesellerPricelist = companyWholesellerPriceList,
                PriceListImportedHeadId = priceRuleDTO.PriceListImportedHeadId,
                PriceListOrigin = priceRuleDTO.PriceListImportedHeadId.HasValue ? (int)PriceListOrigin.CompDbPriceList : (int)PriceListOrigin.SysDbPriceList,
                Company = company,
            };

            var result = AddRecursiveReferences(priceRule, company, priceListType, companyWholesellerPriceList, priceRuleDTO.PriceListImportedHeadId);
            if (!result.Success)
                return result;
            //Set modified properties always, because pricerule doesn't get updated
            SetModifiedProperties(priceRule);
            result = AddEntityItem(entities, priceRule, "PriceRule", useBulkSaveChanges: false);
            if (result.Success)
                result.IntegerValue = priceRule.RuleId;

            return result;
        }

        private ActionResult AddRecursiveReferences(PriceRule rule, Company company, PriceListType priceListType, CompanyWholesellerPricelist companyWholesellerPriceList, int? priceListImportedHeadId)
        {
            ActionResult result = new ActionResult();

            if (rule.LRule != null && result.Success)
                result = AddRecursiveReferences(rule.LRule, company, priceListType, companyWholesellerPriceList, priceListImportedHeadId);

            if (rule.RRule != null && result.Success)
                result = AddRecursiveReferences(rule.RRule, company, priceListType, companyWholesellerPriceList, priceListImportedHeadId);

            try
            {
                rule.Company = company;
                rule.PriceListType = priceListType;
                rule.CompanyWholesellerPricelist = companyWholesellerPriceList;
                rule.PriceListImportedHeadId = priceListImportedHeadId;
                rule.PriceListOrigin = priceListImportedHeadId.HasValue ? (int)PriceListOrigin.CompDbPriceList : (int)PriceListOrigin.SysDbPriceList;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(false);
            }
            return result;
        }

        public ActionResult CopyPriceRules(CompEntities entities, List<PriceRule> sourcePriceRules, Company company, CompanyWholesellerPricelist targetCompanyWholeSellerPriceList)
        {
            ActionResult result = new ActionResult(true);

            if (sourcePriceRules == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceRule");
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Company");

            //Get sub rules
            List<int> subPriceRuleIds = new List<int>();
            foreach (PriceRule sourcePriceRule in sourcePriceRules)
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

            //Get master rules
            foreach (PriceRule sourcePriceRule in sourcePriceRules)
            {
                if (subPriceRuleIds.Contains(sourcePriceRule.RuleId))
                    continue;

                PriceRule newPriceRule = CopyPriceRuleRecursive(sourcePriceRule, sourcePriceRule.PriceListType, company, targetCompanyWholeSellerPriceList);
                if (newPriceRule == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceRule");

                result = AddEntityItem(entities, newPriceRule, "PriceRule");
                if (result.Success)
                    result.IntegerValue = newPriceRule.RuleId;
            }

            return result;
        }

        private PriceRule CopyPriceRuleRecursive(PriceRule priceRule, PriceListType priceListType, Company company, CompanyWholesellerPricelist companyWholeSellerPriceList)
        {
            PriceRule leftPriceRule = null;
            if (priceRule.LRule != null)
                leftPriceRule = CopyPriceRuleRecursive(priceRule.LRule, priceListType, company, companyWholeSellerPriceList);
            PriceRule rightPriceRule = null;
            if (priceRule.RRule != null)
                rightPriceRule = CopyPriceRuleRecursive(priceRule.RRule, priceListType, company, companyWholeSellerPriceList);

            PriceRule newPriceRule = new PriceRule()
            {
                OperatorType = priceRule.OperatorType,
                lExampleType = priceRule.lExampleType,
                rExampleType = priceRule.rExampleType,
                LValue = priceRule.LValue,
                RValue = priceRule.RValue,
                LValueType = priceRule.LValueType,
                RValueType = priceRule.RValueType,
                UseNetPrice = priceRule.UseNetPrice,

                //References
                LRule = leftPriceRule,
                RRule = rightPriceRule,
                PriceListType = priceListType,
                CompanyWholesellerPricelist = companyWholeSellerPriceList,
                Company = company,
            };

            return newPriceRule;
        }

        public ActionResult DeletePriceRule(int priceRuleId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return DeletePriceRule(entities, priceRuleId, actorCompanyId);
            }
        }

        public ActionResult DeletePriceRule(CompEntities entities, int priceRuleId, int actorCompanyId)
        {
            PriceRule priceRule = GetPriceRule(entities, priceRuleId, actorCompanyId);
            if (priceRule == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "PriceRule");

            return DeletePriceRuleRecursive(entities, priceRule, actorCompanyId);
        }

        public ActionResult DeletePriceRules(CompEntities entities, List<PriceRule> priceRules, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            foreach (PriceRule priceRule in priceRules)
            {
                result = DeletePriceRuleRecursive(entities, priceRule, actorCompanyId);
                if (!result.Success)
                    return new ActionResult(false);
            }

            return result;
        }

        public ActionResult DeletePriceRuleRecursive(CompEntities entities, PriceRule priceRule, int actorCompanyId)
        {
            if (priceRule == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceRule");

            ActionResult result;

            if (priceRule.LRule != null)
            {
                result = DeletePriceRuleRecursive(entities, priceRule.LRule, actorCompanyId);
                if (!result.Success)
                    return result;
            }

            if (priceRule.RRule != null)
            {
                result = DeletePriceRuleRecursive(entities, priceRule.RRule, actorCompanyId);
                if (!result.Success)
                    return result;
            }

            result = DeleteEntityItem(entities, priceRule, useBulkSaveChanges: false);

            return result;
        }

        public ActionResult ValidatePriceRule(ValidatePriceRuleDTO input)
        {
            ActionResult result = new ActionResult(true);

            if (input.Items == null || input.Items.Count == 0)
                return result;

            result.Success = false;
            result.ErrorMessage = "Felaktig formel";

            return result;
        }

        public bool PricelistConnectedToPriceRule(CompEntities entities, int priceListImportedHeadId, int actorCompanyId)
        {
            return (from pr in entities.PriceRule
                   where pr.Company.ActorCompanyId == actorCompanyId &&
                   pr.PriceListImportedHeadId == priceListImportedHeadId
                   select pr).Any();
        }

        #endregion

        #region Apply Rule

        /// <summary>
        /// Fetches the prices for a group of products
        /// </summary>
        /// <param name="entities">Company entities</param>
        /// <param name="actorCompanyId">The company that applies the rule</param>
        /// <param name="priceListTypeId">The price list type to use. Fetched from customer or company setting if not specified</param>   
        /// <param name="customerId">The customer that receives the price</param>   
        /// <param name="companyWholesellerPricelistProducts">key = companyWholesellerpricelistId value = list of each internal sysproductid</param>
        /// <returns></returns>
        public List<RuleResult> ApplyPriceRules(CompEntities entities, List<InvoiceProductPriceSearchViewDTO> productPriceItems, int companyPricelistTypeId, int priceListTypeId, int customerId, int actorCompanyId, bool ignoreWholesellerDiscount = false, decimal ediPurchasePrice = 0, bool useMisc = false)
        {
            List<RuleResult> ruleResult = new List<RuleResult>();

            #region Prereq

            // Get price list type from customer or company setting if not specified
            if (priceListTypeId == 0 && customerId != 0)
            {
                priceListTypeId = CustomerManager.GetCustomerPriceListTypeId(entities, customerId, actorCompanyId);
            }

            if (priceListTypeId == 0)
                priceListTypeId = companyPricelistTypeId;

            #endregion

            #region Fetch gnp price for product

            foreach (var productPriceItemGroup in productPriceItems.Where(i => i.PriceListOrigin != (int)PriceListOrigin.CompDbPriceList && i.PriceListOrigin != (int)PriceListOrigin.CompDbNetPriceList && i.CompanyWholesellerPriceListId.HasValue).GroupBy(i => i.CompanyWholesellerPriceListId.Value).ToList())
            {
                #region From Sys DB

                int companyWholeSellerPriceListId = productPriceItemGroup.Key;

                PriceRule priceRule = GetPriceRule(entities, priceListTypeId, companyWholeSellerPriceListId, actorCompanyId, true, 0);

                ruleResult.AddRange(ApplyPriceRules(entities, actorCompanyId, productPriceItemGroup, companyWholeSellerPriceListId, priceListTypeId, priceRule, customerId, ignoreWholesellerDiscount: ignoreWholesellerDiscount, ediPurchasePrice: ediPurchasePrice, useMisc: useMisc));

                #endregion
            }

            foreach (var productPriceItemGroup in productPriceItems.Where(i => i.PriceListOrigin == (int)PriceListOrigin.CompDbPriceList || i.PriceListOrigin == (int)PriceListOrigin.CompDbNetPriceList).GroupBy(i => i.SysPriceListHeadId).ToList())
            {
                #region From Comp DB

                int priceListImportedHeadId = productPriceItemGroup.Key;

                PriceRule priceRule = GetPriceRule(entities, priceListTypeId, 0, actorCompanyId, true, priceListImportedHeadId: priceListImportedHeadId);

                ruleResult.AddRange(ApplyPriceRules(entities, actorCompanyId, productPriceItemGroup, 0, priceListTypeId, priceRule, customerId, ignoreWholesellerDiscount: ignoreWholesellerDiscount, ediPurchasePrice: ediPurchasePrice));

                #endregion
            }

            #endregion

            return ruleResult;
        }

        public string ToFormula(PriceRule rule)
        {
            string op = GetTypeName(rule.OperatorType);
            string result = string.Empty;
            if (rule.RRule != null && rule.LRule != null)
                result = " ( " + ToFormula(rule.LRule) + op + ToFormula(rule.RRule) + " ) ";
            else if (rule.LRule != null)
                result = " ( " + ToFormula(rule.LRule) + op + GetTypeName(rule.RValueType, rule.UseNetPrice) + " ) ";
            else if (rule.RRule != null)
                result = " ( " + GetTypeName(rule.LValueType, rule.UseNetPrice) + op + ToFormula(rule.RRule) + " ) ";
            else
                result = " ( " + GetTypeName(rule.LValueType, rule.UseNetPrice) + op + GetTypeName(rule.RValueType, rule.UseNetPrice) + " ) ";
            return result;
        }

        #region Help methods

        private string GetTypeName(int? id, bool useNetPrice = false)
        {
            if (id == null) return "?";
            PriceRuleItemType type = (PriceRuleItemType)id;

            switch (type)
            {
                case PriceRuleItemType.CustomerDiscount:
                    return GetText(4186, "Kundrabatt");
                case PriceRuleItemType.Gain:
                    return GetText(4184, "Påslag");
                case PriceRuleItemType.GNP:
                    return GetText(4181, "GNP");
                case PriceRuleItemType.SupplierAgreement:
                    if (useNetPrice)
                        return GetText(4188, "Rabattbrev") + "/" + GetText(7759, "Nettopris");
                    else
                        return GetText(4188, "Rabattbrev");
                case PriceRuleItemType.PriceBasedMarkup:
                    return GetText(7607, "Prisbaserat påslag");
                case PriceRuleItemType.Addition:
                    return " + ";
                case PriceRuleItemType.Subtraction:
                    return " - ";
                case PriceRuleItemType.Multiplication:
                case PriceRuleItemType.Or:
                    return " * ";
            }
            return Enum.GetName(typeof(PriceRuleItemType), type);
        }

        public SupplierAgreement GetMatchSupplierAgreementToProduct(CompEntities entities, int actorCompanyId, int sysProductId, int companyWholeSellerPriceListId, int priceListTypeId)
        {
            var companyPriceList = entities.CompanyWholesellerPricelist.Where(x => x.CompanyWholesellerPriceListId == companyWholeSellerPriceListId && x.Company.ActorCompanyId == actorCompanyId).FirstOrDefault();
            if (companyPriceList == null)
                return null;

            var sysProductItem = SysPriceListManager.SearchProductPrice(companyPriceList.SysPriceListHeadId, sysProductId);

            if (sysProductItem == null)
                return null;

            return entities.MatchSupplierAgreementToProduct(actorCompanyId, sysProductItem.ProductNumber, sysProductItem.ProductCode, (int)SoeSupplierAgreemntCodeType.MaterialCode, (int)SoeSupplierAgreemntCodeType.Generic, companyWholeSellerPriceListId, priceListTypeId, (int)SoeSupplierAgreemntCodeType.Product).FirstOrDefault();
        }

        private static bool HasUseNetPrice(PriceRule rule)
        {
            if (rule == null)
                return false;

            if (rule.UseNetPrice)
                return true;
            
            if (HasUseNetPrice(rule.LRule))
                return true;

            if (HasUseNetPrice(rule.RRule))
                return true;

            return false;
        }

        private IEnumerable<RuleResult> ApplyPriceRules(CompEntities entities, int actorCompanyId, IEnumerable<InvoiceProductPriceSearchViewDTO> productPriceItemGroup, int companyWholeSellerPriceListId, int priceListTypeId, PriceRule priceRule, int customerId, bool ignoreWholesellerDiscount = false, decimal ediPurchasePrice = 0, bool useMisc = false)
        {
            foreach (var productPriceItem in productPriceItemGroup)
            {
                #region SysProduct

                int sysProductId = productPriceItem.ProductId;
                InvoiceProductPriceSearchViewDTO productPrice = null;
                decimal supplierAgreementDiscount = 0;
                List<int> categorieIds = null;

                categorieIds = customerId <= 0 ? new List<int>() : CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Customer, SoeCategoryRecordEntity.Customer, customerId, actorCompanyId).Select(c => c.CategoryId).ToList();

                if (!ignoreWholesellerDiscount)
                {
                    //Fetch gnp price for product
                    if (productPriceItem.PriceListOrigin == (int)PriceListOrigin.CompDbPriceList)
                    {
                        productPrice = ProductManager.GetInvoiceProductPrice(entities, actorCompanyId, sysProductId, null, productPriceItem.SysPriceListHeadId, null);

                        var priceListImported = (from p in entities.PriceListImported
                                                 where
                                                 p.PriceListImportedHead.SysWholesellerId == productPrice.SysWholesellerId &&
                                                 p.ProductImportedId == productPrice.ProductId &&
                                                 p.PriceListImportedHead.ActorCompanyId == actorCompanyId
                                                 orderby p.PriceListImportedHeadId descending
                                                 select p).FirstOrDefault();

                        var query = from entry in entities.SupplierAgreement
                                    where
                                    entry.PriceListOrigin == (int)PriceListOrigin.CompDbPriceList
                                    &&
                                    entry.Company.ActorCompanyId == actorCompanyId
                                    &&
                                    (((entry.CodeType == (int)SoeSupplierAgreemntCodeType.MaterialCode || entry.CodeType == (int)SoeSupplierAgreemntCodeType.Generic) && entry.Code == priceListImported.Code) ||
                                    (entry.CodeType == (int)SoeSupplierAgreemntCodeType.Product && entry.Code == productPrice.Number))
                                    &&
                                    (!entry.CategoryId.HasValue || categorieIds.Contains(entry.CategoryId.Value))
                                    orderby entry.RebateListId descending
                                    select entry;

                        SupplierAgreement supplierAgreement = query.FirstOrDefault();

                        supplierAgreementDiscount = supplierAgreement != null && !ignoreWholesellerDiscount ? supplierAgreement.DiscountPercent : 0M;
                    }
                    else if (productPriceItem.PriceListOrigin == (int)PriceListOrigin.CompDbNetPriceList)
                    {
                        productPrice = ProductManager.GetInvoiceProductPrice(entities, actorCompanyId, sysProductId, null, productPriceItem.SysPriceListHeadId, null, productPriceItem.WholsellerNetPriceId);
                    }
                    else
                    {

                        productPrice = ProductManager.GetInvoiceProductPrice(entities, actorCompanyId, sysProductId, companyWholeSellerPriceListId);
                        SupplierAgreement supplierAgreement = GetMatchSupplierAgreementToProduct(entities, actorCompanyId, sysProductId, companyWholeSellerPriceListId, priceListTypeId);
                        supplierAgreementDiscount = supplierAgreement != null && !ignoreWholesellerDiscount ? supplierAgreement.DiscountPercent : 0M;
                    }
                }

                decimal gnp = ignoreWholesellerDiscount ? ediPurchasePrice : productPrice != null ? productPrice.GNP : 0M;
                decimal netPrice = gnp;
                var netPriceFound = false;
                if (  (priceRule == null) || 
                         (productPriceItem.PriceListOrigin == (int)PriceListOrigin.CompDbNetPriceList && WholsellerNetPriceManager.HasCompleteNetPriceList(productPriceItem.SysWholesellerId)) || 
                         HasUseNetPrice(priceRule)
                   )
                {
                    var netPriceDto = WholsellerNetPriceManager.GetNetPrice(entities, actorCompanyId, sysProductId, productPriceItem.SysWholesellerId, priceListTypeId);
                    if (netPriceDto != null)
                    {
                        netPrice = netPriceDto.NetPrice;
                        netPriceFound = true;
                    }
                    else if (productPriceItem.NettoNettoPrice.GetValueOrDefault() > 0)
                    {
                        netPrice = productPriceItem.NettoNettoPrice ?? 0;
                        netPriceFound = true;
                    }
                }
                else if (
                            productPriceItem.NettoNettoPrice.GetValueOrDefault() > 0 &&
                            (
                                (priceRule == null && productPriceItem.PriceListOrigin == (int)PriceListOrigin.CompDbNetPriceList) || 
                                (productPriceItem.PriceListOrigin == (int)PriceListOrigin.SysDbPriceList)
                            )
                        )
                {
                    netPrice = productPriceItem.NettoNettoPrice ?? 0;
                    netPriceFound = true;
                }

                if (supplierAgreementDiscount > 0 && !netPriceFound)
                {
                    decimal discount = Convert.ToDecimal(100M - supplierAgreementDiscount);
                    netPrice = (discount * netPrice);
                    netPrice /= 100M;
                }

                if (!ignoreWholesellerDiscount && netPriceFound && gnp > 0)
                {
                    // Net prices should be used if the price leads to a lower price than the supplier agreement.
                    var discountFromNet = Math.Round(100 * (gnp - netPrice) / gnp, 4);
                    supplierAgreementDiscount = Math.Max(discountFromNet, supplierAgreementDiscount);
                } 

                if (priceRule == null)
                {
                    // No price rule found for specified company and wholeseller, return original price
                    yield return new RuleResult()
                    {
                        Value = productPriceItem.SalesPrice ?? gnp,
                        NetPrice = netPrice,
                        SysProductId = sysProductId,
                        CompanyWholesellerPriceListId = companyWholeSellerPriceListId,
                        SysWholesellerId = productPriceItem.SysWholesellerId,
                        NetPriceFromNetPriceList = netPriceFound,
                    };
                }
                else
                {
                    bool recalculate = false;
                    RuleResult ruleResultRecursive = ApplyPriceRuleRecursive(entities, priceRule, categorieIds, supplierAgreementDiscount, companyWholeSellerPriceListId, customerId, sysProductId, productPriceItem.SysPriceListHeadId, productPriceItem.SysWholesellerId, actorCompanyId, ref recalculate, ediPurchasePrice != 0, ediPurchasePrice, useMisc, netPrice, productPriceItem.WholsellerNetPriceId);

                    if (recalculate)
                    {
                        gnp = productPrice != null ? productPrice.GNP : 0M;
                        netPrice = gnp;
                        if (supplierAgreementDiscount > 0)
                        {
                            decimal discount = Convert.ToDecimal(100M - supplierAgreementDiscount);
                            netPrice = (discount * netPrice);
                            netPrice /= 100M;
                        }
                    }

                    if (productPriceItem.SalesPrice.HasValue)
                    {
                        ruleResultRecursive.Value = productPriceItem.SalesPrice.Value;
                    }

                    ruleResultRecursive.SysProductId = sysProductId;
                    ruleResultRecursive.CompanyWholesellerPriceListId = companyWholeSellerPriceListId;
                    ruleResultRecursive.NetPrice = netPrice;
                    ruleResultRecursive.Formula = ToFormula(priceRule);
                    ruleResultRecursive.SysWholesellerId = productPriceItem.SysWholesellerId;
                    ruleResultRecursive.NetPriceFromNetPriceList = netPriceFound;
                    yield return ruleResultRecursive;
                }

                #endregion
            }
        }

        private RuleResult ApplyPriceRuleRecursive(CompEntities entities, PriceRule rule, List<int> categories, decimal supplierAgreementDiscount, int companyWholesellerPriceListId, int customerId, int sysProductId, int sysPriceListHeadId, int sysWholesellerId, int actorCompanyId, ref bool recalculate, bool useEDIPurchasePriceAsGnp = false, decimal ediPurchasePrice = 0, bool useMisc = false, decimal nettoPrice = 0, int wholsellerNetPriceId = 0)
        {
            RuleResult result = null;
            RuleResult left = null;
            RuleResult right = null;

            if (((rule.LValueType.HasValue && (PriceRuleItemType)rule.LValueType.Value != PriceRuleItemType.SupplierAgreement) && (rule.RValueType.HasValue && (PriceRuleItemType)rule.RValueType.Value != PriceRuleItemType.SupplierAgreement)) && useEDIPurchasePriceAsGnp && !useMisc)
            {
                useEDIPurchasePriceAsGnp = false;
                recalculate = true;
            }

            // Traverse subrules
            if (rule.LRule != null)
                left = ApplyPriceRuleRecursive(entities, rule.LRule, categories, supplierAgreementDiscount, companyWholesellerPriceListId, customerId, sysProductId, sysPriceListHeadId, sysWholesellerId, actorCompanyId, ref recalculate, nettoPrice: nettoPrice, wholsellerNetPriceId: wholsellerNetPriceId);
            if (rule.RRule != null)
                right = ApplyPriceRuleRecursive(entities, rule.RRule, categories, supplierAgreementDiscount, companyWholesellerPriceListId, customerId, sysProductId, sysPriceListHeadId, sysWholesellerId, actorCompanyId, ref recalculate, nettoPrice: nettoPrice, wholsellerNetPriceId: wholsellerNetPriceId);

            // Get values
            if (rule.LValueType != null)
                left = GetRuleValue(entities, rule, (PriceRuleItemType)rule.LValueType, categories, rule.LValue, supplierAgreementDiscount, companyWholesellerPriceListId, customerId, sysProductId, sysPriceListHeadId, sysWholesellerId, actorCompanyId, useEDIPurchasePriceAsGnp, ediPurchasePrice, nettoPrice, wholsellerNetPriceId);
            if (rule.RValueType != null)
                right = GetRuleValue(entities, rule, (PriceRuleItemType)rule.RValueType, categories, rule.RValue, supplierAgreementDiscount, companyWholesellerPriceListId, customerId, sysProductId, sysPriceListHeadId, sysWholesellerId, actorCompanyId, useEDIPurchasePriceAsGnp, ediPurchasePrice, nettoPrice, wholsellerNetPriceId);

            // Apply operation
            switch ((PriceRuleItemType)rule.OperatorType)
            {
                case PriceRuleItemType.Addition:
                    result = CalculateAdd(left, right);
                    break;
                case PriceRuleItemType.Subtraction:
                    result = CalculateSubtraction(left, right);
                    break;
                case PriceRuleItemType.Multiplication:
                    result = CalculateMultiplication(left, right);
                    break;
                case PriceRuleItemType.Or:
                    result = CalculateOr(left, right);
                    break;
            }

            return result;
        }

        private RuleResult GetRuleValue(CompEntities entities, PriceRule rule, PriceRuleItemType type, List<int> categories, decimal? gain, decimal supplierAgreementDiscount, int companyWholesellerPriceListId, int customerId, int sysProductId, int sysPriceListHeadId, int sysWholesellerId, int actorCompanyId, bool useEDIPurchasePriceAsGnp = false, decimal ediPurchasePrice = 0, decimal nettoPrice = 0, int wholsellerNetPriceId = 0)
        {
            RuleResult result = new RuleResult(0M);
            switch (type)
            {
                case PriceRuleItemType.CustomerDiscount:
                    Markup markupDiscount = MarkupManager.GetSingleMarkup(entities, categories, sysProductId, sysPriceListHeadId, sysWholesellerId, actorCompanyId, true);
                    result.Value = markupDiscount != null && markupDiscount.DiscountPercent != null ? (decimal)markupDiscount.DiscountPercent : 0M;
                    result.Type = PriceRuleValueType.NegativePercent;
                    break;
                case PriceRuleItemType.Gain:
                    result.Value = (decimal)gain;
                    result.Type = PriceRuleValueType.PositivePercent;
                    break;
                case PriceRuleItemType.GNP:
                    if (!useEDIPurchasePriceAsGnp || ediPurchasePrice == 0)
                    {
                        //companyWholesellerPriceListId = 0 and sysPriceListHeadId > 0 then probably a PriceListImportedHeadId 
                        InvoiceProductPriceSearchViewDTO productPrice = ProductManager.GetInvoiceProductPrice(entities, actorCompanyId, sysProductId, companyWholesellerPriceListId, companyWholesellerPriceListId == 0 ? sysPriceListHeadId : (int?)null, sysWholesellerId, wholsellerNetPriceId);
                        result.Value = productPrice != null ? productPrice.GNP : 0M;
                    }
                    else
                        result.Value = ediPurchasePrice;

                    result.Type = PriceRuleValueType.Numeric;
                    break;
                case PriceRuleItemType.Markup:
                    Markup markup = MarkupManager.GetSingleMarkup(entities, sysProductId, companyWholesellerPriceListId, actorCompanyId, sysWholesellerId);
                    result.Value = markup?.MarkupPercent ?? 0M;
                    result.Type = PriceRuleValueType.PositivePercent;
                    break;
                case PriceRuleItemType.NetPrice:
                    var price = WholsellerNetPriceManager.GetNetPrice(entities, actorCompanyId, sysProductId, sysWholesellerId, rule.PriceListType?.PriceListTypeId);
                    result.Value = price?.NetPrice ?? 0M;
                    result.Type = PriceRuleValueType.Numeric;
                    break;
                case PriceRuleItemType.PriceBasedMarkup:
                    decimal gnp;
                    if (!useEDIPurchasePriceAsGnp || ediPurchasePrice == 0)
                    {
                        if (nettoPrice > 0)
                        {
                            gnp = nettoPrice;
                        }
                        else
                        {
                            //companyWholesellerPriceListId = 0 and sysPriceListHeadId > 0 then probably a PriceListImportedHeadId 
                            InvoiceProductPriceSearchViewDTO productPrice = ProductManager.GetInvoiceProductPrice(entities, actorCompanyId, sysProductId, companyWholesellerPriceListId, companyWholesellerPriceListId == 0 ? sysPriceListHeadId : (int?)null, sysWholesellerId, wholsellerNetPriceId);
                            gnp = productPrice != null ? productPrice.GNP : 0M;
                        }
                    }
                    else
                        gnp = ediPurchasePrice;

                    result.Value = MarkupManager.GetPriceBasedMarkup(entities, rule.PriceListType?.PriceListTypeId ?? 0, gnp, actorCompanyId) ?? 0M;
                    result.Type = PriceRuleValueType.PositivePercent;
                    break;
                case PriceRuleItemType.SupplierAgreement:
                    /*
                    if (rule.UseNetPrice && !WholsellerNetPriceManager.HasCompleteNetPriceList(sysWholesellerId))
                    {
                        var netPrice = WholsellerNetPriceManager.GetNetPrice(entities, actorCompanyId, sysProductId, sysWholesellerId, rule.PriceListType?.PriceListTypeId)?.NetPrice ?? 0;
                        if (netPrice > 0)
                        {
                            var gnpPrice = ProductManager.GetInvoiceProductPrice(entities, actorCompanyId, sysProductId, companyWholesellerPriceListId, companyWholesellerPriceListId == 0 ? sysPriceListHeadId : (int?)null, sysWholesellerId, wholsellerNetPriceId)?.GNP ?? 0;
                            if (gnpPrice > 0)
                            {
                                supplierAgreementDiscount = Math.Round((gnpPrice - netPrice) / gnpPrice, 4) * 100;
                            }
                        }
                    }
                    */
                    result.Value = supplierAgreementDiscount;
                    result.Type = PriceRuleValueType.NegativePercent;
                    
                    break;
            }
            return result;
        }

        private RuleResult CalculateAdd(RuleResult left, RuleResult right)
        {
            RuleResult result = new RuleResult();
            if (left == null || right == null)
                return result;

            if (left.Type == PriceRuleValueType.Numeric)
                result.Type = right.Type;
            else if (right.Type == PriceRuleValueType.Numeric)
                result.Type = left.Type;
            else if ((left.Type == PriceRuleValueType.NegativePercent && right.Type == PriceRuleValueType.PositivePercent) ||
                     (left.Type == PriceRuleValueType.PositivePercent && right.Type == PriceRuleValueType.NegativePercent))
            {
                if (left.Value > right.Value)
                {
                    result.Type = left.Type;
                    result.Value = left.Value - right.Value;
                }
                else
                {
                    result.Type = right.Type;
                    result.Value = right.Value - left.Value;
                }
                return result;
            }
            else
            {
                result.Type = left.Type;
            }

            result.Value = left.Value + right.Value;

            return result;
        }

        private void GetRuleValues(RuleResult left, RuleResult right, ref decimal leftValue, ref decimal rightValue)
        {
            //LeftValue
            if (left.Type == PriceRuleValueType.NegativePercent)
                leftValue = (1 - (left.Value / 100));
            else if (left.Type == PriceRuleValueType.PositivePercent)
                leftValue = 1 + (left.Value / 100);
            else if (left.Type == PriceRuleValueType.Percent)
                leftValue = (left.Value);
            else
                leftValue = left.Value;

            //RightValue
            if (right.Type == PriceRuleValueType.NegativePercent)
                rightValue = (1 - (right.Value / 100));
            else if (right.Type == PriceRuleValueType.PositivePercent)
                rightValue = 1 + (right.Value / 100);
            else if (right.Type == PriceRuleValueType.Percent)
                rightValue = (right.Value);
            else
                rightValue = right.Value;
        }

        private RuleResult CalculateOr(RuleResult left, RuleResult right)
        {
            RuleResult result = new RuleResult();
            if (left == null || right == null)
                return result;

            decimal leftValue = 0;
            decimal rightValue = 0;
            GetRuleValues(left, right, ref leftValue, ref rightValue);

            switch (left.Type)
            {
                case PriceRuleValueType.Numeric:
                    result.Type = PriceRuleValueType.Numeric;
                    break;
                case PriceRuleValueType.NegativePercent:
                    if (right.Type == PriceRuleValueType.NegativePercent)
                        result.Type = PriceRuleValueType.NegativePercent;
                    else if (right.Type == PriceRuleValueType.PositivePercent)
                        result.Type = PriceRuleValueType.Percent;
                    else
                        result.Type = PriceRuleValueType.Numeric;
                    break;
                case PriceRuleValueType.PositivePercent:
                    if (right.Type == PriceRuleValueType.NegativePercent)
                        result.Type = PriceRuleValueType.Percent;
                    else if (right.Type == PriceRuleValueType.PositivePercent)
                        result.Type = PriceRuleValueType.PositivePercent;
                    else
                        result.Type = PriceRuleValueType.Numeric;
                    break;
            }

            if (left.Type == PriceRuleValueType.Replace)
            {
                result.Value = leftValue;
            }
            else if (right.Type == PriceRuleValueType.Replace)
            {
                result.Value = rightValue;
            }
            else
            {
                result.Value = leftValue > 0 ? leftValue : rightValue;
            }

            return result;
        }

        private RuleResult CalculateMultiplication(RuleResult left, RuleResult right)
        {
            RuleResult result = new RuleResult();
            if (left == null || right == null)
                return result;

            decimal leftValue = 0;
            decimal rightValue = 0;
            GetRuleValues(left,right, ref leftValue, ref rightValue);

            switch (left.Type)
            {
                case PriceRuleValueType.Numeric:
                    result.Type = PriceRuleValueType.Numeric;
                    break;
                case PriceRuleValueType.NegativePercent:
                    if (right.Type == PriceRuleValueType.NegativePercent)
                        result.Type = PriceRuleValueType.NegativePercent;
                    else if (right.Type == PriceRuleValueType.PositivePercent)
                        result.Type = PriceRuleValueType.Percent;
                    else
                        result.Type = PriceRuleValueType.Numeric;
                    break;
                case PriceRuleValueType.PositivePercent:
                    if (right.Type == PriceRuleValueType.NegativePercent)
                        result.Type = PriceRuleValueType.Percent;
                    else if (right.Type == PriceRuleValueType.PositivePercent)
                        result.Type = PriceRuleValueType.PositivePercent;
                    else
                        result.Type = PriceRuleValueType.Numeric;
                    break;
            }

            result.Value = leftValue * rightValue;

            return result;
        }

        private RuleResult CalculateSubtraction(RuleResult left, RuleResult right)
        {
            RuleResult result = new RuleResult();
            if (left == null || right == null)
                return result;

            if (left.Type == PriceRuleValueType.Numeric)
                result.Type = right.Type;
            else if (right.Type == PriceRuleValueType.Numeric)
                result.Type = left.Type;
            else if ((left.Type == PriceRuleValueType.NegativePercent && right.Type == PriceRuleValueType.PositivePercent) ||
                     (left.Type == PriceRuleValueType.PositivePercent && right.Type == PriceRuleValueType.NegativePercent))
            {
                if (left.Value > right.Value)
                {
                    result.Type = left.Type;
                    result.Value = left.Value - right.Value;
                }
                else
                {
                    result.Type = right.Type;
                    result.Value = right.Value - left.Value;
                }
                return result;
            }
            else
            {
                result.Type = left.Type;
            }

            result.Value = left.Value - right.Value;

            return result;
        }

        #endregion

        #endregion
    }
}

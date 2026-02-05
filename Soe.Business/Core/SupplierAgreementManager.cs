using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.SupplierAgreement;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class SupplierAgreementManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public SupplierAgreementManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        #region Import File To Object
        public ActionResult Import(byte[] bytes, string fileName, SoeSupplierAgreementProvider providerType, int priceListTypeId, int actorCompanyId, decimal genericDiscount)
        {
            Stream stream = new MemoryStream(bytes);
            return Import(stream, fileName, providerType, priceListTypeId, actorCompanyId, genericDiscount);
        }

        public ActionResult Import(Stream stream, string fileName, SoeSupplierAgreementProvider providerType, int priceListTypeId, int actorCompanyId, decimal genericDiscount)
        {
            ActionResult result;
            string returnMessage = string.Empty;

            var fileType = FileUtil.GetFileType(fileName);
            var provider = GetProviderAdapter(providerType, fileType);
            if (provider == null)
                return new ActionResult("Supplier agreement provider not found:" + providerType.ToString());

            try
            {
                result = ValidationUtils.ValidateFile(stream, ref provider);
                if (!result.Success)
                    return result;

                result = provider.Read(stream);
                if (!result.Success)
                    return result;

                var genericProvider = provider.ToGeneric();

                result = Save(genericProvider, priceListTypeId, actorCompanyId, providerType, genericDiscount);
                if (!result.Success)
                    return result;
                else
                    returnMessage = GetText(7768, "Antal rabatter") + ": " + result.ObjectsAffected.ToString();

                var netPricesObject = provider as ISupplierAgreementWithNetPrices;

                if ((netPricesObject?.HasNetPrice ?? false) && netPricesObject.SysWholeSeller != SoeWholeseller.Unknown)
                {
                    var netprices = netPricesObject.ToNetPrices();
                    if (netprices.Any())
                    {
                        var wm = new WholsellerNetPriceManager(this.parameterObject);
                        result = wm.Save(ActorCompanyId, (int)netPricesObject.SysWholeSeller, priceListTypeId, netprices);
                        if (!result.Success)
                        {
                            return result;
                        }
                        else
                        {
                            returnMessage += "\n" + GetText(7767, "Antal nettopriser") + ": "+ result.ObjectsAffected.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(ex);
            }

            if (result.Success)
                result.InfoMessage = GetText(4508, "Import av rabattbrev klart") + "\n" + returnMessage;

            return result;
        }

        private ISupplierAgreement GetProviderAdapter(SoeSupplierAgreementProvider providerType, SoeFileType fileType)
        {
            switch (providerType)
            {
                case SoeSupplierAgreementProvider.Ahlsell:
                    return new Ahlsell();
                case SoeSupplierAgreementProvider.SolarVVS:
                    return new SolarVVS();
                case SoeSupplierAgreementProvider.Dahl:
                    return new Dahl();
                case SoeSupplierAgreementProvider.Sonepar:
                    return new Elektroskandia();
                case SoeSupplierAgreementProvider.Onninen:
                    return new Onninen();
                case SoeSupplierAgreementProvider.Rexel:
                  return new Rexel();
                case SoeSupplierAgreementProvider.Solar:
                    return new Solar();
                case SoeSupplierAgreementProvider.Storel:
                    return new Storel();
                case SoeSupplierAgreementProvider.SthlmElgross:
                    return new SthlmElgross();
                //case SoeSupplierAgreementProvider.Moel:
                    //return new Moel();
                //case SoeSupplierAgreementProvider.Elgrossen:
                //    return new Elgrossen();
                case SoeSupplierAgreementProvider.Bragross:
                    return new Bragross();
                case SoeSupplierAgreementProvider.Carpings:
                case SoeSupplierAgreementProvider.VSProdukter:
                    return new Carpings();
                case SoeSupplierAgreementProvider.VVSCentrum:
                    return new VVScentrum();
                //case SoeSupplierAgreementProvider.Lunda:
                //    return new Lunda();
                case SoeSupplierAgreementProvider.RexelFI:
                    return new FinnishAgreement(SoeSupplierAgreementProvider.RexelFI);
                case SoeSupplierAgreementProvider.DahlFI:
                    return new FinnishAgreement(SoeSupplierAgreementProvider.DahlFI);
                case SoeSupplierAgreementProvider.OnninenFI:
                    return new FinnishAgreement(SoeSupplierAgreementProvider.OnninenFI);
                case SoeSupplierAgreementProvider.SoneparFI:
                    return new FinnishAgreement(SoeSupplierAgreementProvider.SoneparFI);
                //case SoeSupplierAgreementProvider.WarlaFI:
                //    return new FinnishAgreement(SoeSupplierAgreementProvider.WarlaFI);
                case SoeSupplierAgreementProvider.AhlsellFI:
                    return new FinnishAgreement(SoeSupplierAgreementProvider.AhlsellFI);
                case SoeSupplierAgreementProvider.AhlsellFIPL:
                    return new FinnishAgreement(SoeSupplierAgreementProvider.AhlsellFIPL);
                //case SoeSupplierAgreementProvider.ElPartsFI:
                //    return new FinnishAgreement(SoeSupplierAgreementProvider.ElPartsFI);
                case SoeSupplierAgreementProvider.LVIWaBeKFIPL:
                    return new FinnishAgreement(SoeSupplierAgreementProvider.LVIWaBeKFIPL);
                case SoeSupplierAgreementProvider.OnninenFIPL:
                case SoeSupplierAgreementProvider.OnninenFI_SE:
                case SoeSupplierAgreementProvider.OnninenFI_SE_PL:
                case SoeSupplierAgreementProvider.PistesarjaFI:
                    return new FinnishAgreement(providerType);
                case SoeSupplierAgreementProvider.RobHolmqvistVVS:
                    return new RobHolmqvistVVS();
                case SoeSupplierAgreementProvider.JohnFredrik:
                    return new JohnFredrik();
                case SoeSupplierAgreementProvider.Elkedjan:
                    return new Elkedjan();
                case SoeSupplierAgreementProvider.E2Teknik:
                    return new E2Elteknik();
                case SoeSupplierAgreementProvider.Gelia:
                    return new Ahlsell();
                case SoeSupplierAgreementProvider.Copiax:
                    return new GenericCSVProvider(providerType, new GenericSupplierAgreementColumnPositions { Code = 0, Name = 1, Discount = 2 }, true);
                case SoeSupplierAgreementProvider.Bevego:
                    return new GenericCSVProvider(providerType, new GenericSupplierAgreementColumnPositions { Code = 2, Name = 1, Discount = 3 }, true);
                case SoeSupplierAgreementProvider.Thermotech:
                    if (fileType == SoeFileType.Excel)
                        return new ThermotechExcel();
                    else if (fileType == SoeFileType.Txt)
                        return new GenericCSVProvider(providerType, new GenericSupplierAgreementColumnPositions { Code = 0, Name = 1, Discount = 2 }, true);
                    else
                        return null;
                case SoeSupplierAgreementProvider.Lindab:
                    return new GenericCSVProvider(providerType, new GenericSupplierAgreementColumnPositions { Code = 0, Name = 1, Discount = 2 }, false, true);
            }
            return null;
        }

        #endregion

        #region Import Save Object To Entity

        private ActionResult Save(GenericProvider provider, int priceListTypeId, int actorCompanyId, SoeSupplierAgreementProvider providerType, decimal genericDiscount)
        {
            var result = new ActionResult(true);

            if (provider == null || provider.supplierAgreements == null || provider.supplierAgreements.Count == 0)
                return new ActionResult(false, 0, GetText(1949, "Kunde inte spara rabattbrev"));

            #region Wholeseller

            SysWholeseller wholeseller = SysPriceListManager.GetWholesellerFromSupplierAgreement(providerType);
            if (wholeseller == null)
                return new ActionResult(false, (int)ActionResultSave.EntityNotFound, GetText(8289, "Grossister kunde inte hittas"));

            int sysWholesellerId = wholeseller.SysWholesellerId;

            #endregion

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Company

                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        #endregion

                        #region Remove Existing

                        if (!DeleteSupplierAgreements(entities, actorCompanyId, sysWholesellerId, priceListTypeId).Success)
                            return new ActionResult(false);

                        #endregion

                        #region Generic Discount

                        var generalDiscount = new SupplierAgreement()
                        {
                            Company = company,
                            Code = "*",
                            CodeType = (int)SoeSupplierAgreemntCodeType.Generic,
                            DiscountPercent = genericDiscount,
                            Date = DateTime.Now,
                            SysWholesellerId = sysWholesellerId
                        };
                        if (priceListTypeId != 0)
                            generalDiscount.PriceListTypeId = priceListTypeId;
                        SetCreatedProperties(generalDiscount);

                        entities.SupplierAgreement.AddObject(generalDiscount);

                        #endregion

                        #region Save Supplier agreements

                        var count = 0;
                        foreach (var supplierAgreementItems in provider.supplierAgreements.GroupBy(x=> new { x.Code, x.Discount, x.CodeType }))
                        {
                            var supplierAgreementItem = supplierAgreementItems.First();
                            //can't save this item
                            if (supplierAgreementItem.Code == null)
                                continue;

                            var supplierAgreement = new SupplierAgreement()
                            {
                                Company = company,
                                Code = supplierAgreementItem.Code,
                                CodeType = (int)supplierAgreementItem.CodeType,
                                DiscountPercent = supplierAgreementItem.Discount,
                                Date = DateTime.Now,
                                SysWholesellerId = sysWholesellerId
                            };
                            if (priceListTypeId != 0)
                                supplierAgreement.PriceListTypeId = priceListTypeId;
                            SetCreatedProperties(supplierAgreement);

                            count++;
                            entities.SupplierAgreement.AddObject(supplierAgreement);
                        }

                        // if (result.Success && entities.SaveChanges(SaveOptions.AcceptAllChangesAfterSave) > 0)
                        if (result.Success )
                        {
                            entities.BulkSaveChanges();
                            //Commit transaction
                            transaction.Complete();
                            result.ObjectsAffected = count;
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion

        #region Set general discount for provider

        public ActionResult SaveDiscount(SupplierAgreementDTO dto, int actorCompanyId)
        {
            ActionResult result;
            var providerType = (SoeSupplierAgreementProvider)dto.SysWholesellerId;
            using (var entities = new CompEntities())
            {
                #region SysWholeseller

                int sysWholesellerId = dto.SysWholesellerId;

                //First save contains Provider...
                if (dto.RebateListId == 0)
                {

                    SysWholeseller wholeseller = SysPriceListManager.GetWholesellerFromSupplierAgreement(providerType);
                    if (wholeseller == null)
                        return new ActionResult("Could not found wholeseller from provider supplier agreement providertyp:" + providerType.ToString());

                    sysWholesellerId = wholeseller.SysWholesellerId;
                }

                if (dto.CodeType == 0)
                {
                    return new ActionResult("Type of discount is not set:");
                }

                #endregion

                #region Company

                Company company = CompanyManager.GetCompany(entities, actorCompanyId);

                #endregion

                #region SupplierAgreement

                var code = dto.CodeType == (int)SoeSupplierAgreemntCodeType.Generic ? "*" : dto.Code;

                SupplierAgreement originalSupplierAgreement = null;

                if (dto.RebateListId > 0)
                {
                    // Get current general discount
                    originalSupplierAgreement = (from sa in entities.SupplierAgreement
                                                where
                                                    sa.Company.ActorCompanyId == actorCompanyId &&
                                                    sa.RebateListId == dto.RebateListId
                                                    select sa).FirstOrDefault(); 
                }
                else
                {
                    IQueryable<SupplierAgreement> query = (from sa in entities.SupplierAgreement
                                                 where
                                                     sa.Company.ActorCompanyId == actorCompanyId &&
                                                     sa.CodeType == dto.CodeType &&
                                                     sa.Code == code &&
                                                     sa.SysWholesellerId == sysWholesellerId
                                                 select sa);

                    if (dto.PriceListTypeId.GetValueOrDefault() != 0)
                        query = query.Where(sa => sa.PriceListTypeId == dto.PriceListTypeId.Value);

                    originalSupplierAgreement = query.FirstOrDefault();

                    if (originalSupplierAgreement != null)
                    {
                        return new ActionResult(GetText(7646, "Rabatt finns redan upplagd för vald grossist/prislista/typ"));
                    }

                }

                if (originalSupplierAgreement == null)
                {

                    originalSupplierAgreement = new SupplierAgreement()
                    {
                        Code = code,
                        CodeType = dto.CodeType,
                        SysWholesellerId = sysWholesellerId,
                        Company = company,
                        DiscountPercent = dto.DiscountPercent
                    };

                    if (dto.PriceListTypeId != 0)
                        originalSupplierAgreement.PriceListTypeId = dto.PriceListTypeId;

                    result = AddEntityItem(entities, originalSupplierAgreement, "SupplierAgreement");
                }
                else if (dto.State == (int)SoeEntityState.Deleted)
                {
                    result = DeleteEntityItem(entities, originalSupplierAgreement);
                }
                else
                {
                    originalSupplierAgreement.DiscountPercent = dto.DiscountPercent;

                    result = SaveChanges(entities);
                }

                if (result.Success)
                    result.InfoMessage = dto.State == (int)SoeEntityState.Deleted ? GetText(1708, "Raden borttagen") :  GetText(2274, "Ny generell rabatt för grossist sparad");
                else
                    result.ErrorMessage = GetText(2275, "Fel vid sparande av generell rabatt");

                #endregion

            }

            return result;
        }

        #endregion

        #endregion

        #region SupplierAgreement

        public List<SmallGenericType> GetSupplierAgreementProviders(int actorCompanyId)
        {
            var result = new List<SmallGenericType>();
            var countryId = CompanyManager.GetCompanySysCountryId(actorCompanyId);

            var enumValues = Enum.GetValues(typeof(SoeSupplierAgreementProvider))
                .Cast<SoeSupplierAgreementProvider>()
                .OrderBy(value => (int)value);
            foreach(var value in enumValues)
            {
                var name = value.ToString();
                switch ((TermGroup_Country) countryId)
                {
                    case TermGroup_Country.FI:
                        if (name.Contains("FI"))
                            result.Add(new SmallGenericType((int)value, name));
                        break;
                    case TermGroup_Country.NO:
                        if (name.Contains("NO"))
                            result.Add(new SmallGenericType((int)value, name));
                        break;
                    case TermGroup_Country.SE:
                        if (!name.Contains("FI") && !name.Contains("NO"))
                            result.Add(new SmallGenericType((int)value, name));
                        break;
                }
                
            }

            return result.OrderBy(x=> x.Name).ToList();
        }

        public List<SupplierAgreement> GetSupplierAgreements(int actorCompanyId, int agreementProvider = 0)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SupplierAgreement.NoTracking();
            return GetSupplierAgreements(entities, actorCompanyId, agreementProvider);
        }

        public List<SupplierAgreement> GetSupplierAgreements(CompEntities entities, int actorCompanyId, int agreementProvider)
        {
            SysWholeseller wholeseller = agreementProvider > 0 ? SysPriceListManager.GetWholesellerFromSupplierAgreement((SoeSupplierAgreementProvider)agreementProvider) : null;
            
            IQueryable<SupplierAgreement> query = (from pl in entities.SupplierAgreement
                                      where pl.Company.ActorCompanyId == actorCompanyId
                                      orderby pl.SysWholesellerId, pl.Date
                                      select pl);
            if (wholeseller != null)
            {
                query = query.Where(x => x.SysWholesellerId == wholeseller.SysWholesellerId);
            }

            var supplierAgreements = query.ToList();
            var wholesellers = SysPriceListManager.GetSysWholesellersDict();
            var priceListTypes = ProductPricelistManager.GetPriceListTypes(entities, actorCompanyId);

            foreach (var supplierAgreement in supplierAgreements)
            {
                PriceListType priceListType = null;
                supplierAgreement.WholesellerName = wholesellers[supplierAgreement.SysWholesellerId].Name;
                if (supplierAgreement.PriceListTypeId.HasValue)
                    priceListType = priceListTypes.FirstOrDefault(p => p.PriceListTypeId == supplierAgreement.PriceListTypeId.Value);
                if (priceListType != null)
                    supplierAgreement.PriceListTypeName = priceListType.Name;
            }

            return supplierAgreements;
        }

        public ActionResult DeleteSupplierAgreements(int actorCompanyId, SoeSupplierAgreementProvider sysWholesellerId, int priceListTypeId)
        {
            SysWholeseller wholeseller = SysPriceListManager.GetWholesellerFromSupplierAgreement(sysWholesellerId);
            if (wholeseller != null)
                return DeleteSupplierAgreements(actorCompanyId, wholeseller.SysWholesellerId, priceListTypeId);
            else
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8289, "Grossister kunde inte hittas"));
        }

        public ActionResult DeleteSupplierAgreements(int actorCompanyId, int sysWholesellerId, int priceListTypeId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return DeleteSupplierAgreements(entities, actorCompanyId, sysWholesellerId, priceListTypeId);
            }
        }

        private ActionResult DeleteSupplierAgreements(CompEntities entities, int actorCompanyId, int sysWholesellerId, int priceListTypeId)
        {
            ActionResult result = new ActionResult();

            try
            {
                entities.DeleteSupplierAgreements(actorCompanyId, sysWholesellerId, priceListTypeId);
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;
        }

        #endregion

    }
}

using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.PricelistProvider;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class SysPriceListManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public SysPriceListManager(ParameterObject parameterObject) : base(parameterObject) { }


        public static bool IsAhlsell(SoeSysPriceListProvider provider)
        {
            return (
                    (provider == SoeSysPriceListProvider.AhlsellIsolering) ||
                    (provider == SoeSysPriceListProvider.AhlsellBygg) ||
                    (provider == SoeSysPriceListProvider.AhlsellVerktyg) ||
                    (provider == SoeSysPriceListProvider.AhlsellKyla) ||
                    (provider == SoeSysPriceListProvider.AhlsellMetall) ||
                    (provider == SoeSysPriceListProvider.AhlsellVentilation) ||
                    (provider == SoeSysPriceListProvider.Comfort_Ahlsell)
                );
        }

        public static bool IsComfort(SoeSysPriceListProvider provider)
        {
            return (provider.ToString().StartsWith("Comfort_"));
        }

        public static bool IsCurrentum(SoeSysPriceListProvider provider)
        {
            return (provider.ToString().StartsWith("Currentum_"));
        }

        public static bool ValidateChainAffilation(SoeSysPriceListProvider provider, TermGroup_ChainAffiliation chainAffiliation)
        {
            if (provider == SoeSysPriceListProvider.Bad_Värme && chainAffiliation != TermGroup_ChainAffiliation.Bad_Varme)
            {
                return false;
            }

            if (IsComfort(provider) && chainAffiliation != TermGroup_ChainAffiliation.Comfort)
            {
                return false;
            }

            if (IsCurrentum(provider) && chainAffiliation != TermGroup_ChainAffiliation.Currentum)
            {
                return false;
            }

            return true;
        }

        public static ExternalProductType GetExternalProductType(SoeSysPriceListProviderType soeSysPriceListProviderType)
        {
            switch (soeSysPriceListProviderType)
            {
                case SoeSysPriceListProviderType.Ahlsell:
                    return ExternalProductType.Ahlsell;
                case SoeSysPriceListProviderType.LockSmith:
                    return ExternalProductType.LockSmith;
                case SoeSysPriceListProviderType.Plumbing:
                    return ExternalProductType.Plumbing;
                case SoeSysPriceListProviderType.Electrician:
                    return ExternalProductType.Electric;
                case SoeSysPriceListProviderType.Bevego:
                    return ExternalProductType.Bevego;
                case SoeSysPriceListProviderType.BadVarme:
                    return ExternalProductType.BadVarme;
                case SoeSysPriceListProviderType.Comfort:
                    return ExternalProductType.Comfort;
                case SoeSysPriceListProviderType.Lindab:
                    return ExternalProductType.Lindab;
                default:
                    return ExternalProductType.Unknown;
            }
        }

        public ExternalProductType GetExternalProductType(SoeSysPriceListProvider provider, int sysWholeSellerId)
        {
            if (IsAhlsell(provider))
            {
                return ExternalProductType.Ahlsell;
            }
            if (provider == SoeSysPriceListProvider.Bad_Värme)
            {
                return ExternalProductType.BadVarme;
            }

            var wholeseller = GetSysWholesellerFromCache(sysWholeSellerId);

            if (wholeseller != null)
            {
                return GetExternalProductType((SoeSysPriceListProviderType)wholeseller.Type);
            }
            else
            {
                return ExternalProductType.Unknown;
            }
        }

        #endregion

        #region SysPriceList

        public SysPriceListDTO GetSysPriceListByProductNumber(string productNr, int sysWholesellerId)
        {
            using (var sysEntites = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return (from spl in sysEntites.SysPriceList
                            join sp in sysEntites.SysProduct on
                            new { spl.SysProductId, ProductId = productNr } equals
                            new { sp.SysProductId, sp.ProductId }
                            where spl.SysWholesellerId == sysWholesellerId
                            orderby spl.SysPriceListId descending
                            select spl).Select(x => new SysPriceListDTO
                            {
                                Code = x.Code,
                                GNP = x.GNP,
                                PackageSize = x.PackageSize,
                                EnvironmentFee = x.EnvironmentFee,
                                Storage = x.Storage,
                                PackageSizeMin = x.PackageSizeMin,
                                PriceChangeDate = x.PriceChangeDate,
                                PriceStatus = x.PriceStatus,
                                ProductLink = x.ProductLink,
                                PurchaseUnit = x.PurchaseUnit,
                                ReplacesProduct = x.ReplacesProduct,
                                SalesUnit = x.SalesUnit,
                                SysPriceListHeadId = x.SysPriceListHeadId,
                                SysPriceListId = x.SysPriceListId,
                                SysProductId = x.SysProductId,
                                SysWholesellerId = x.SysWholesellerId,
                            }).FirstOrDefault();
                }
            }
        }

        public SysPriceList GetSysPriceList(int sysProductId, int syspricelistheadId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from spl in sysEntitiesReadOnly.SysPriceList
                    where spl.SysProductId == sysProductId && spl.SysPriceListHeadId == syspricelistheadId
                    select spl).FirstOrDefault();
        }

        public List<SysProductDTO> SearchSysProduct(string condition)
        {
            using (var sysEntites = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return sysEntites.SysProduct.Where(condition).Select(x => new SysProductDTO
                    {
                        ProductId = x.ProductId,
                        SysCountryId = (TermGroup_Country)x.SysCountryId,
                        Type = x.Type,
                        Name = x.Name,
                        EAN = x.EAN,
                        SysProductId = x.SysProductId,
                        EndAt = x.EndAt,
                    }).ToList();
                }
            }
        }

        public List<InvoiceProductPriceSearchViewDTO> SearchProductPrices(string condition, List<CompanyWholesellerPriceListViewDTO> companyPriceLists)
        {
            var syspriceListHeadIds = companyPriceLists.Select(c => c.SysPriceListHeadId).ToList();
            using (var sysEntites = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var productPricesDtos = sysEntites.SysProductPriceSearchView.Where(condition).Where(p => syspriceListHeadIds.Contains(p.SysPriceListHeadId)).ToDTOs().ToList();
                    foreach (var productPrice in productPricesDtos)
                    {
                        productPrice.CompanyWholesellerPriceListId = companyPriceLists.FirstOrDefault(s => s.SysPriceListHeadId == productPrice.SysPriceListHeadId)?.CompanyWholesellerPriceListId;
                    }

                    return productPricesDtos;
                }
            }
        }

        public InvoiceProductPriceSearchViewDTO SearchProductPrice(string productNr, List<CompanyWholesellerPriceListViewDTO> companyPriceLists)
        {
            var syspriceListHeadIds = companyPriceLists.Select(c => c.SysPriceListHeadId).ToList();
            using (var sysEntites = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var productPrice = sysEntites.SysProductPriceSearchView.Where(p => p.ProductNumber == productNr && syspriceListHeadIds.Contains(p.SysPriceListHeadId)).OrderBy(p => p.SysPriceListHeadId).FirstOrDefault().ToDTO();
                    if (productPrice != null)
                    {
                        productPrice.CompanyWholesellerPriceListId = companyPriceLists.FirstOrDefault(s => s.SysPriceListHeadId == productPrice.SysPriceListHeadId)?.CompanyWholesellerPriceListId;
                    }

                    return productPrice;
                }
            }
        }

        public SysProductPriceSearchView SearchProductPrice(int SysPriceListHeadId, int sysProductId)
        {
            using (var sysEntites = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return sysEntites.SysProductPriceSearchView.Where(p => p.SysPriceListHeadId == SysPriceListHeadId && p.ProductId == sysProductId).FirstOrDefault();
                }
            }
        }

        public List<InvoiceProductPriceSearchViewDTO> SearchProductPriceByEAN(long productEAN, List<CompanyWholesellerPriceListViewDTO> companyPriceLists)
        {
            var syspriceListHeadIds = companyPriceLists.Select(c => c.SysPriceListHeadId).ToList();
            using (var sysEntites = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var productPrices = sysEntites.SysProductPriceSearchView.Where(p => p.EAN == productEAN.ToString() && syspriceListHeadIds.Contains(p.SysPriceListHeadId)).OrderBy(p => p.SysPriceListHeadId).ToDTOs().ToList();


                    return productPrices;
                }
            }
        }

        public List<SysWholsesellerPriceSearchDTO> GetWholsellerPrices(List<int> syspriceListHeadIds, List<int> sysproductIds)
        {

            var productPrices = new List<SysWholsesellerPriceSearchDTO>();

            #region sysprices
            using (var sysEntites = new SOESysEntities())
            {
                productPrices = sysEntites.SysProductPriceSearchView.Where(p => syspriceListHeadIds.Contains(p.SysPriceListHeadId) && sysproductIds.Contains(p.ProductId)).Select(x =>
                    new SysWholsesellerPriceSearchDTO
                    {
                        GNP = x.GNP,
                        SysProductId = x.ProductId,
                        SysWholesellerId = x.SysWholesellerId,
                        Code = x.ProductCode,
                        ProductNr = x.ProductNumber
                    }
                    ).ToList();
            }


            using (CompEntities entities = new CompEntities())
            {
                var companyWholesellerPriceLists = entities.CompanyWholesellerPricelist.Where(c => c.Company.ActorCompanyId == ActorCompanyId).Select(x => new { x.SysWholesellerId, x.CompanyWholesellerPriceListId }).ToList();

                foreach (var productPriceGroupedByWholseller in productPrices.GroupBy(x => x.SysWholesellerId))
                {
                    var first = productPriceGroupedByWholseller.First();
                    var companyWholesellerPriceList = companyWholesellerPriceLists.FirstOrDefault(c => c.SysWholesellerId == first.SysWholesellerId);
                    foreach (var productPrice in productPriceGroupedByWholseller)
                    {
                        var aggrement = entities.MatchSupplierAgreementToProduct(base.ActorCompanyId, productPrice.ProductNr, productPrice.Code, (int)SoeSupplierAgreemntCodeType.MaterialCode, (int)SoeSupplierAgreemntCodeType.Generic, companyWholesellerPriceList?.CompanyWholesellerPriceListId, null, (int)SoeSupplierAgreemntCodeType.Product).FirstOrDefault();

                        if (aggrement != null)
                        {
                            productPrice.GNP = Math.Round(productPrice.GNP * (1 - aggrement.DiscountPercent / 100), 2);
                        }
                    }
                }
            }
            #endregion

            #region Netprices

            var netPrices = WholsellerNetPriceManager.GetNetPrices(base.ActorCompanyId, sysproductIds);

            foreach (var netpricePerWholseller in netPrices.GroupBy(x => new { x.SysProductId, x.SysWholesellerId }))
            {
                var netprice = netpricePerWholseller.OrderBy(x => x.PriceListTypeId).FirstOrDefault();

                productPrices.Add(new SysWholsesellerPriceSearchDTO
                {
                    GNP = netprice.NetPrice,
                    SysProductId = netprice.SysProductId,
                    SysWholesellerId = netprice.SysWholesellerId
                });
            }

            #endregion

            return productPrices;
        }

        #region Import

        public ActionResult SysPriceListImport(int actorCompanyId, int userId, SysPriceListImportDTO dto)
        {

            try
            {
                string fileName = this.GetFileNameFromPriceListImportDTO(dto.File.Name);
                Stream stream = PrepareFileStream(dto.File.Bytes, ref fileName);
                SoeSysPriceListProvider providerType = (SoeSysPriceListProvider)dto.Provider;
                return Import(stream, providerType, actorCompanyId, fileName);
            }
            catch (FormatException)
            {
                return new ActionResult(false, 0, "Kunde inte läsa sökväg till filen");
            }
            catch (InvalidDataException ex)
            {
                base.LogError(new SoeGeneralException("Prislisteimport: zipfilsläsning", ex, this.ToString()), this.log);
                return new ActionResult(false, 0, GetText(1952, "Kunde inte packa upp zipfil"));
            }
            catch (Exception ex)
            {
                base.LogError(new SoeGeneralException("Prislisteimport", ex, this.ToString()), this.log);
                return new ActionResult(false, 0, GetText(1951, "Kunde inte öppna filen"));
            }
        }


        private static Stream PrepareFileStream(byte[] fileBytes, ref string fileName)
        {

            if (FileUtil.GetFileType(fileName) == SoeFileType.Zip)
            {
                var filesInZip = ZipUtility.UnzipFilesInZipFile(fileBytes);

                if (filesInZip.Count != 1)
                {
                    throw new InvalidDataException("Zip file must contain exactly one file.");
                }

                fileName = filesInZip.First().Key;
                return new MemoryStream(filesInZip.First().Value);
            }

            return new MemoryStream(fileBytes);
        }

        private string GetFileNameFromPriceListImportDTO(string fileName)
        {
            try
            {
                return Path.GetFileName(fileName);
            }
            catch (Exception ex)
            {
                base.LogError(new SoeGeneralException($"Prislisteimport: parse filename({fileName})", ex, this.ToString()), this.log);
                throw new FormatException("Kunde inte läsa sökväg till filen", ex);
            }
        }

        public ActionResult ImportLunda(Stream fileStream1, string fileStream1filenName, Stream fileStream2, string fileStream2fileName, int actorCompanyId)
        {
            if ((fileStream1filenName == fileStream2fileName) || !Lunda.ValidFileName(fileStream1filenName) || !Lunda.ValidFileName(fileStream2fileName))
            {
                return new ActionResult(false, 0, "Ogiltigt filnamn för Lunda filerna, ska vara price_list_ea_lundagr.csv och NTO_lundanto.csv");
            }


            var nettoProvider = GetProviderAdapter(SoeCompPriceListProvider.LundaStyckNetto);
            var result = nettoProvider.Read(fileStream1, fileStream1filenName);
            if (!result.Success)
                return result;

            result = nettoProvider.Read(fileStream2, fileStream2fileName);
            if (!result.Success)
                return result;

            var genericProvider = nettoProvider.ToGeneric();
            result = Save(genericProvider, SoeCompPriceListProvider.LundaStyckNetto, actorCompanyId);
            if (!result.Success)
                return result;

            var bruttoProvider = GetProviderAdapter(SoeCompPriceListProvider.LundaBrutto);
            bruttoProvider.Read(fileStream1, fileStream1filenName);
            bruttoProvider.Read(fileStream2, fileStream2fileName);
            genericProvider = bruttoProvider.ToGeneric();
            result = Save(genericProvider, SoeCompPriceListProvider.LundaBrutto, actorCompanyId);
            if (!result.Success)
                return result;

            if (result.Success)
                result.ErrorMessage = GetText(4885, "prislista importerad");

            return result;
        }

        public ActionResult ImportFINetto(Stream fileStream1, Stream fileStream2, SoeCompPriceListProvider providerType, int actorCompanyId)
        {
            var result = new ActionResult();

            try
            {
                var provider = GetProviderAdapter(providerType);
                if (provider == null)
                    return new ActionResult(false);

                result = provider.Read(fileStream1);
                if (!result.Success)
                    return result;

                result = provider.Read(fileStream2);
                if (!result.Success)
                    return result;

                var genericProvider = provider.ToGeneric();

                result = Save(genericProvider, providerType, actorCompanyId);
                if (!result.Success)
                    return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                return result;
            }

            if (result.Success)
                result.ErrorMessage = GetText(4885, "prislista importerad");

            return result;
        }

        public ActionResult Import(Stream stream, SoeCompPriceListProvider providerType, int actorCompanyId, string fileName)
        {
            ActionResult result = new ActionResult();
            try
            {
                var provider = GetProviderAdapter(providerType);
                if (provider == null)
                    return new ActionResult(false);

                result = provider.Read(stream);
                if (!result.Success)
                    return result;

                var genericProvider = provider.ToGeneric();

                result = Save(genericProvider, providerType, actorCompanyId);
                if (!result.Success)
                    return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                return result;
            }

            if (result.Success)
                result.ErrorMessage = GetText(4885, "prislista importerad");

            return result;
        }

        public ActionResult Import(Stream stream, SoeSysPriceListProvider providerType, int actorCompanyId, string fileName)
        {
            ActionResult result = new ActionResult(false);

            try
            {
                var fileType = FileUtil.GetFileType(fileName);

                result = ValidateFileName(providerType, fileName, fileType);
                if (!result.Success)
                    return result;

                var provider = GetProviderAdapter(providerType, fileType);
                if (provider == null)
                    return new ActionResult(false);

                result = ValidationUtils.ValidateFile(stream, ref provider);
                if (!result.Success)
                    return result;

                result = provider.Read(stream, fileName);
                if (!result.Success)
                    return result;

                var genericProvider = provider.ToGeneric();

                result = Save(genericProvider, providerType);
            }

            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                return result;
            }

            if (result.Success)
                result.ErrorMessage = $"{providerType} {GetText(4885, "prislista importerad")} ({result.IntegerValue})";

            return result;
        }

        private ActionResult ValidateFileName(SoeSysPriceListProvider provider, string fileName, SoeFileType soeFileType)
        {
            if (provider == SoeSysPriceListProvider.SLR && fileName.ToUpper() != "ARTREG.SLR")
            {
                return new ActionResult(GetText(7643, "Felaktigt filnamn, borde vara:") + " " + "ARTREG.SLR");
            }
            else if (IsComfort(provider))
            {
                var comfortResult = Comfort.ValidateFileName(provider, fileName);
                if (!comfortResult.Success)
                {
                    return new ActionResult(GetText(7643, "Felaktigt filnamn, borde vara:") + " " + comfortResult.ErrorMessage);
                }
            }
            else if (provider == SoeSysPriceListProvider.Solar || provider == SoeSysPriceListProvider.Currentum_Solar || provider == SoeSysPriceListProvider.SolarVVS)
            {
                var solarResult = SolarCSV.ValidateFileName(fileName);
                if (!solarResult.Success)
                {
                    return new ActionResult(GetText(7643, "Felaktigt filnamn, borde vara:") + " " + solarResult.ErrorMessage);
                }
            }
            else if (provider == SoeSysPriceListProvider.Currentum_Ahlsell && soeFileType != SoeFileType.Excel)
            {
                return new ActionResult(GetText(7643, "Felaktigt filnamn, borde vara:") + " " + SoeFileType.Excel.ToString());
            }

            return new ActionResult(true);
        }

        private IPriceListProvider GetProviderAdapter(SoeSysPriceListProvider providerType, SoeFileType fileType)
        {
            if (IsComfort(providerType))
                return new Comfort(providerType);

            switch (providerType)
            {
                case SoeSysPriceListProvider.AhlsellEl:
                    return new Ahlsell("Ahlsell El");
                case SoeSysPriceListProvider.AhlsellVvs:
                    return new Ahlsell("Ahlsell Vvs");
                case SoeSysPriceListProvider.AhlsellIsolering:
                    return new Ahlsell("Ahlsell Isolering");
                case SoeSysPriceListProvider.AhlsellBygg:
                    return new Ahlsell("Ahlsell Bygg");
                case SoeSysPriceListProvider.AhlsellVerktyg:
                    return new Ahlsell("Ahlsell Verktyg");
                case SoeSysPriceListProvider.AhlsellKyla:
                    return new Ahlsell("Ahlsell Kyla");
                case SoeSysPriceListProvider.AhlsellVentilation:
                    return new Ahlsell("Ahlsell Ventilation");
                case SoeSysPriceListProvider.AhlsellMetall:
                    return new Ahlsell("Ahlsell Plåt");
                case SoeSysPriceListProvider.SolarVVS:
                    return new SolarVVS();
                case SoeSysPriceListProvider.Dahl:
                case SoeSysPriceListProvider.Currentum_Dahl:
                    return new Dahl();
                case SoeSysPriceListProvider.SthlmElgross:
                    return new SthlmElgross();
                //case SoeSysPriceListProvider.ElektroskandiaNetto:
                //    return new ElektroskandiaNetto();
                case SoeSysPriceListProvider.Sonepar:
                    return new Sonepar();
                case SoeSysPriceListProvider.Onninen:
                    return new Onninen();
                case SoeSysPriceListProvider.Rexel:
                    return new Rexel(false);
                case SoeSysPriceListProvider.Solar:
                case SoeSysPriceListProvider.Currentum_Solar:
                    return new SolarCSV(SoeSysPriceListProvider.Solar);
                case SoeSysPriceListProvider.Moel:
                    return new Moel();
#pragma warning disable S112 // General exceptions should never be thrown
                case SoeSysPriceListProvider.Storel7:
                    throw new Exception("SoeSysPriceListProvider.Storel7 not used anymore");
                //case SoeSysPriceListProvider.Storel8:
                //    throw new Exception("SoeSysPriceListProvider.Storel8 not used anymore");
#pragma warning restore S112 // General exceptions should never be thrown
                case SoeSysPriceListProvider.Malmbergs:
                    return new Malmbergs();
                //case SoeSysPriceListProvider.Elgrossen:
                //    return new Elgrossen();
                case SoeSysPriceListProvider.Bragross:
                    return new Bragross();
                case SoeSysPriceListProvider.Carpings:
                    return new Carpings();
                case SoeSysPriceListProvider.VVScentrum:
                    return new VVScentrum();
                case SoeSysPriceListProvider.DahlFI:
                case SoeSysPriceListProvider.RexelFI:
                case SoeSysPriceListProvider.OnninenFI:
                case SoeSysPriceListProvider.OnninenFIPL:
                case SoeSysPriceListProvider.OnninenFI_SE:
                case SoeSysPriceListProvider.OnninenFI_SE_PL:
                case SoeSysPriceListProvider.SoneparFI:
                //case SoeSysPriceListProvider.WarlaFI:
                case SoeSysPriceListProvider.AhlsellFI:
                case SoeSysPriceListProvider.AhlsellFIPL:
                case SoeSysPriceListProvider.PistesarjaFI:
                //case SoeSysPriceListProvider.ElPartsFI:
                case SoeSysPriceListProvider.LVIWaBeKFIPL:
                    return new FinnishProvider(providerType);
                case SoeSysPriceListProvider.RobHolmqvistVVS:
                    return new RobHolmqvistVVS();
                case SoeSysPriceListProvider.JohnFredrik:
                    return new GenericCSVProvider(providerType, new GenericPriceListColumnPositions { ProductId = 0, Name = 1, Price = 2, ArticleGroup = 3, SalesUnit = 4, });
                case SoeSysPriceListProvider.Gelia:
                    return new Gelia();
                case SoeSysPriceListProvider.Elkedjan:
                    return new Elkedjan();
                case SoeSysPriceListProvider.E2Teknik:
                    return new E2Teknik();
                case SoeSysPriceListProvider.ByggOle:
                    return new ByggOle();
                case SoeSysPriceListProvider.VSProdukter:
                    return new VSProdukter();
                case SoeSysPriceListProvider.Copiax:
                    return new GenericCSVProvider(providerType, new GenericPriceListColumnPositions { ProductId = 0, Name = 1, ArticleGroup = 5, EAN = 7, SalesUnit = 11, Price = 18 });
                case SoeSysPriceListProvider.SLR:
                    return new GenericCSVProvider(providerType, new GenericPriceListColumnPositions { ProductId = 1, Name = 2, ArticleGroup = 11, EAN = 16, SalesUnit = 5, Price = 7 }, ',', true);
                case SoeSysPriceListProvider.Bad_Värme:
                    return new BadVarme();
                case SoeSysPriceListProvider.Bevego:
                    return new GenericCSVProvider(providerType, new GenericPriceListColumnPositions { ProductId = 0, Name = 1, ArticleGroup = 5, PurchaseUnit = 2, Price = 7 }, ';', true, true, System.Text.Encoding.UTF8);
                case SoeSysPriceListProvider.Thermotech:
                    if (fileType == SoeFileType.Excel)
                        return new ThermotechExcel();
                    else if (fileType == SoeFileType.Txt)
                        return new GenericCSVProvider(providerType, new GenericPriceListColumnPositions { ProductId = 0, Name = 1, ArticleGroup = 2, PurchaseUnit = 3, Price = 4 }, ';', false, true);
                    break;
                case SoeSysPriceListProvider.Lindab:
                    return new GenericCSVProvider(providerType, new GenericPriceListColumnPositions { ProductId = 0, Name = 1, ArticleGroup = 4, PurchaseUnit = 3, Price = 2, EAN = 7 }, ';', false, false, System.Text.Encoding.UTF8);
                case SoeSysPriceListProvider.Currentum_Ahlsell:
                    return new CurrentumExcel(providerType);
            }

            return null;
        }

        private static IPriceListProvider GetProviderAdapter(SoeCompPriceListProvider providerType)
        {
            switch (providerType)
            {
                case SoeCompPriceListProvider.AhlsellVerktyg:
                    return new Ahlsell("Ahlsell verktyg");
                case SoeCompPriceListProvider.AhlsellBygg:
                    return new Ahlsell("Ahlsell bygg");
                case SoeCompPriceListProvider.TBEl:
                    return new TBEl();
                case SoeCompPriceListProvider.Bevego:
                    return new Bevego();
                case SoeCompPriceListProvider.Trebolit:
                    return new Trebolit();
                case SoeCompPriceListProvider.Etman:
                case SoeCompPriceListProvider.EtmanPipe:
                    return new Etman(providerType);
                case SoeCompPriceListProvider.MalmbergFI:
                    return new MalmbergFI(providerType);
                case SoeCompPriceListProvider.LundaBrutto:
                case SoeCompPriceListProvider.LundaStyckNetto:
                    return new Lunda(providerType);
                case SoeCompPriceListProvider.RexelFINetto:
                case SoeCompPriceListProvider.AhlsellFINetto:
                case SoeCompPriceListProvider.AhlsellFIPLNetto:
                case SoeCompPriceListProvider.SoneparFINetto:
                case SoeCompPriceListProvider.OnninenFINettoS:
                case SoeCompPriceListProvider.DahlFINetto:
                case SoeCompPriceListProvider.OnninenFINettoLVI:
                    return new FinnishProvider(providerType);
                case SoeCompPriceListProvider.Alcadon:
                    return new Alcadon(providerType);
                case SoeCompPriceListProvider.StorelNetto:
                    throw new Exception("StorelNetto används inte mer! Använd RexelNetto");
                //return new GenericCSVProvider(providerType, new GenericPriceListColumnPositions { ProductId = 0, Name = 1, Price = 7, ArticleGroup = 5, SalesUnit = 6, });
                case SoeCompPriceListProvider.GeliaNetto:
                    return new GeliaNetto(providerType);
            }
            return null;
        }

        #endregion

        #region Save

        /// <summary>
        /// Stores whole pricelist into comp temp tables and calls an sp to interpret the data
        /// (much faster to process exists check in sql server than via linq in this case...)
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
        private ActionResult Save(GenericProvider provider, SoeCompPriceListProvider providerType, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            DateTime dateCheck = new DateTime(1900, 1, 1);
            SysWholeseller sysWholeseller = null;

            var companySysCountryId = CompanyManager.GetCompanySysCountryId(actorCompanyId);

            int errorCount = 0;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    //Get Wholeseller before opening the transaction
                    string wholeSellerName = provider.WholeSellerName;

                    if (wholeSellerName == "Elkedjan")
                    {
                        wholeSellerName = "Svenska Elkedjan";
                    }

                    sysWholeseller = GetSysWholeSellerFromName(wholeSellerName, null, true, 0, companySysCountryId);

                    if (sysWholeseller == null)
                        sysWholeseller = GetSysWholeSellerFromName(wholeSellerName, null, false, 0, companySysCountryId);

                    if (sysWholeseller == null)
                        return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(4459, "Import misslyckades: Grossist kunde inte kopplas"));

                    entities.CommandTimeout = 60 * 10;

                    entities.Connection.Open();

                    using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        //Parameters
                        if (provider == null || provider.header == null || provider.products == null || provider.products.Count == 0)
                            return new ActionResult(false, 0, GetText(1950, "Kunde inte spara prislista"));

                        //Existing pricelist
                        if (CompPriceListHeadExists(entities, providerType, actorCompanyId, provider.header.Date, provider.header.Version))
                            return new ActionResult(4458, GetText(4458, "Prislista med samma version och datum redan importerad"));

                        var unitChecked = new List<string>();

                        // Select before inserting
                        List<string> productNumbers = new List<string>(provider.products.Count);
                        for (int i = 0; i < provider.products.Count; i++)
                        {
                            var product = provider.products[i] as GenericProduct;
                            productNumbers.Add(product.ProductId);
                        }

                        List<ProductImported> existingProducts;
                        try
                        {
                            existingProducts = (from entry in entities.ProductImported
                                                where
                                                 entry.Type == sysWholeseller.Type &&
                                                 productNumbers.Contains(entry.ProductId) &&
                                                 entry.PriceListImported.Any(pli => pli.PriceListImportedHead.ActorCompanyId == actorCompanyId)
                                                select entry).ToList();
                        }
                        catch
                        {
                            existingProducts = (from entry in entities.ProductImported
                                                where
                                                 entry.Type == sysWholeseller.Type &&
                                                 entry.PriceListImported.Any(pli => pli.PriceListImportedHead.ActorCompanyId == actorCompanyId)
                                                select entry).ToList();
                        }

                        var head = new PriceListImportedHead
                        {
                            Date = provider.header.Date,
                            Version = provider.header.Version,
                            ActorCompanyId = actorCompanyId,
                            Provider = (int)providerType,
                            SysWholesellerId = sysWholeseller.SysWholesellerId
                        };

                        result = AddEntityItem(entities, head, "PriceListImportedHead", transaction);

                        if (!result.Success)
                            return new ActionResult(false);

                        for (int i = 0; i < provider.products.Count; i++)
                        {
                            var product = provider.products[i] as GenericProduct;
                            try
                            {
                                PriceListImported priceListImported;
                                //Try to fetch existing product
                                var importedProduct = existingProducts.FirstOrDefault(p => p.ProductId == product.ProductId && p.Type == sysWholeseller.Type);

                                if (importedProduct == null)
                                {
                                    importedProduct = new ProductImported()
                                    {
                                        ProductId = product.ProductId,
                                        Name = product.Name,
                                        Type = sysWholeseller.Type,
                                        SysCountryId = sysWholeseller.SysCountryId
                                    };
                                }

                                importedProduct.EAN = string.IsNullOrEmpty(product.EAN) ? null : product.EAN.Trim();

                                priceListImported = new PriceListImported
                                {
                                    EnvironmentFee = product.EnvironmentFee,
                                    Storage = product.Storage,
                                    PriceStatus = (int)product.PriceStatus,
                                    PackageSize = product.PackageSize,
                                    PackageSizeMin = product.PackageSizeMin,
                                    Code = product.Code,
                                    ProductImported = importedProduct,
                                    PriceListImportedHead = head,
                                };

                                if (!string.IsNullOrEmpty(product.PurchaseUnit))
                                    priceListImported.PurchaseUnit = product.PurchaseUnit.Trim();

                                if (product.PriceChangeDate > dateCheck)
                                    priceListImported.PriceChangeDate = product.PriceChangeDate;

                                if (product.Price > 0 || product.PriceStatus == SoeProductPriceStatus.PricedOnRequest)
                                    priceListImported.GNP = product.Price;

                                if (!string.IsNullOrEmpty(product.ProductLink))
                                    priceListImported.ProductLink = product.ProductLink.Trim();

                                if (!string.IsNullOrEmpty(product.ReplacesProduct))
                                    priceListImported.ReplacesProduct = product.ReplacesProduct.Trim();

                                if (!string.IsNullOrEmpty(product.SalesUnit))
                                    priceListImported.SalesUnit = product.SalesUnit.Trim();

                                string unit = string.IsNullOrEmpty(priceListImported.PurchaseUnit) ? priceListImported.SalesUnit : priceListImported.PurchaseUnit;

                                // Check if unit exists
                                if (!unitChecked.Contains(unit) && !string.IsNullOrEmpty(unit))
                                {
                                    var exists = (from pu in entities.ProductUnit
                                                  where pu.Code.ToLower() == unit.ToLower()
                                                  && pu.Company.ActorCompanyId == actorCompanyId
                                                  select pu).Any();

                                    if (!exists)
                                    {
                                        ProductManager.AddProductUnit(new ProductUnit()
                                        {
                                            Code = unit,
                                            Name = unit,
                                        }, actorCompanyId);
                                    }
                                    unitChecked.Add(unit);
                                }

                            }
                            catch
                            {
                                errorCount++;
                            }
                        }

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    //Log
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    base.LogError(ex, this.log);
                }
            }

            return result;
        }



        /// <summary>
        /// Stores whole pricelist into sys temp tables and calls an sp to interpret the data
        /// (much faster to process exists check in sql server than via linq in this case...)
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
        private ActionResult Save(GenericProvider provider, SoeSysPriceListProvider providerType)
        {
            var result = new ActionResult();
            DateTime dateCheck = new DateTime(1900, 1, 1);
            bool updateSysProductName = true;

            SysWholeseller sysWholeseller = null;

            #region Import Temp data

            using (SOESysEntities entities = new SOESysEntities())
            {
                //not necessary to track this data - no merging occuring
                try
                {
                    //Parameters
                    if (provider == null || provider.header == null)
                        return new ActionResult(false, 0, GetText(1950, "Kunde inte spara prislista"));
                    else if (provider.products == null || provider.products.Count == 0)
                        return new ActionResult(false, 0, GetText(9112, "Inga produkter hittades i prislistan"));

                    //Existing pricelist
                    if (SysPriceListHeadExists(entities, provider.header.Date, provider.header.Version, providerType))
                        return new ActionResult(4458, GetText(4458, "Prislista med samma version och datum redan importerad"));

                    //Get Wholeseller
                    sysWholeseller = GetWholesllerFromProvider(providerType);

                    if (sysWholeseller == null)
                    {
                        string wholeSellerName = provider.WholeSellerName;

                        if (wholeSellerName == "Elkedjan")
                        {
                            wholeSellerName = "Svenska Elkedjan";
                        }

                        sysWholeseller = GetSysWholeSellerFromName(entities, wholeSellerName, null);
                        if (sysWholeseller == null)
                            return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(4459, "Import misslyckades: Grossist kunde inte kopplas"));
                        else if (sysWholeseller.SysCountryId != (int)provider.SysCountry)
                            return new ActionResult(false, (int)ActionResultSave.EntityNotFound, GetText(9107, "Import misslyckades: Grossistens land stämmer inte överens med prislistans land"));
                    }

                    if (
                        (
                            IsComfort(providerType) && sysWholeseller.SysWholesellerId != (int)SoeWholeseller.Comfort) ||
                            IsCurrentum(providerType)
                        )
                    {
                        //skip updating when not wholseller own price list
                        updateSysProductName = false;
                    }

                    List<SysPriceListTempItem> items = new List<SysPriceListTempItem>();
                    var tempHead = new SysPriceListTempHead
                    {
                        Date = provider.header.Date,
                        Version = provider.header.Version
                    };

                    result = SaveTempHead(entities, tempHead);
                    if (!result.Success)
                        return new ActionResult(false);

                    var productProviderType = GetProviderProductType(providerType);

                    if (productProviderType == SoeSysPriceListProviderType.Unknown)
                    {
                        productProviderType = (SoeSysPriceListProviderType)sysWholeseller.Type;
                    }

                    for (int i = 0; i < provider.products.Count; i++)
                    {
                        var product = provider.products[i] as GenericProduct;
                        try
                        {
                            var tempItem = new SysPriceListTempItem //merge this with create datatable
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                EnvironmentFee = product.EnvironmentFee,
                                Storage = product.Storage,
                                PriceStatus = (int)product.PriceStatus,
                                PackageSize = product.PackageSize,
                                PackageSizeMin = product.PackageSizeMin,
                                Code = product.Code,
                                Type = product.ProductType == SoeSysPriceListProviderType.Unknown ? (int)productProviderType : (int)product.ProductType,
                                SysCountryId = sysWholeseller.SysCountryId,
                                SalesPrice = product.SalesPrice == 0 ? (decimal?)null : product.SalesPrice,
                                NetPrice = product.NetPrice == 0 ? (decimal?)null : product.NetPrice,
                                EAN = product.EAN == "" ? null : product.EAN,
                                Manufacturer = product.Manufacturer == "" ? null : product.Manufacturer,
                                ExtendedInfo = product.ExtendedInfo == "" ? null : product.ExtendedInfo,
                            };

                            if (!String.IsNullOrEmpty(product.PurchaseUnit))
                                tempItem.PurchaseUnit = product.PurchaseUnit.Trim();

                            if (product.PriceChangeDate > dateCheck)
                                tempItem.PriceChangeDate = product.PriceChangeDate;

                            if (product.Price > 0 || product.PriceStatus == SoeProductPriceStatus.PricedOnRequest)
                                tempItem.GNP = product.Price;

                            if (!String.IsNullOrEmpty(product.ProductLink))
                                tempItem.ProductLink = product.ProductLink.Trim();

                            if (!String.IsNullOrEmpty(product.ReplacesProduct))
                                tempItem.ReplacesProduct = product.ReplacesProduct.Trim();

                            if (!String.IsNullOrEmpty(product.SalesUnit))
                                tempItem.SalesUnit = product.SalesUnit.Trim();

                            tempItem.SysPriceListTempHead = tempHead;
                            items.Add(tempItem);
                        }
                        catch (Exception ex)
                        {
                            //Log and skip item
                            base.LogError(ex, this.log);
                        }
                    }

                    provider = null;

                    //faster approach - need to create view
                    #region Bulk copy

                    var connectionString = SOESysEntities.GetConnectionString();
                    SqlConnection connection = new SqlConnection(connectionString);
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using (SqlCommand com = new SqlCommand("SET IDENTITY_INSERT SysPriceListTempItem OFF", connection))
                    {
                        com.ExecuteNonQuery();
                    }

                    using (SqlBulkCopy s = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.KeepIdentity))
                    {
                        s.BulkCopyTimeout = 120;
                        s.DestinationTableName = "SysPriceListTempItem";
                        //s.SqlRowsCopied += new SqlRowsCopiedEventHandler(s_SqlRowsCopied);

                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SysPriceListTempItemId", "SysPriceListTempItemId"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SysPriceListTempHeadId", "SysPriceListTempHeadId"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ProductId", "ProductId"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Name", "Name"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("GNP", "GNP"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Code", "Code"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PurchaseUnit", "PurchaseUnit"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SalesUnit", "SalesUnit"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EnvironmentFee", "EnvironmentFee"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Storage", "Storage"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EAN", "EAN"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ReplacesProduct", "ReplacesProduct"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PackageSizeMin", "PackageSizeMin"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PackageSize", "PackageSize"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ProductLink", "ProductLink"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PriceChangeDate", "PriceChangeDate"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PriceStatus", "PriceStatus"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("[Type]", "[Type]"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SysCountryId", "SysCountryId"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SalesPrice", "SalesPrice"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NetPrice", "NetPrice"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Manufacturer", "Manufacturer"));
                        s.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ExtendedInfo", "ExtendedInfo"));

                        s.WriteToServer(CreateDataTableFromList(items));
                        s.Close();
                    }

                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using (SqlCommand com = new SqlCommand("SET IDENTITY_INSERT SysPriceListTempItem ON", connection))
                    {
                        com.ExecuteNonQuery();
                    }

                    #endregion

                    result.ErrorMessage = GetText(4460, "Prislisteimport genomförd");
                    result.IntegerValue = items.Count;
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

            #endregion

            #region Process PriceList

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    entities.CommandTimeout = (5 * 60); //increase timeout for this operation should take < 3 minutes

                    var head = entities.Database.SqlQuery<SysPriceListHead>("ImportSysPriceList @sysWholeSellerId = {0}, @createdBy = {1}, @provider = {2}, @updateNames = {3}",
                                                                            sysWholeseller.SysWholesellerId, GetUserDetails(), (int)providerType, updateSysProductName ? 1 : 0).FirstOrDefault();
                    if (head == null)
                        return new ActionResult(false, (int)ActionResultSave.Unknown, GetText(4461, "Misslyckades bearbeta prislistan"));
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
            }

            #endregion

            return result;
        }

        #endregion

        #region Help Methods

        private ActionResult SaveTempHead(SOESysEntities entities, SysPriceListTempHead tempHead)
        {
            if (tempHead == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysPriceListTempHead");

            entities.SysPriceListTempHead.Add(tempHead);
            return SaveChanges(entities);
        }

        private DataTable CreateDataTableFromList(List<SysPriceListTempItem> items)
        {
            DataTable dt = new DataTable("SysPriceListTempItem");
            DataColumn dc;
            DataRow dr;

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Int32");
            dc.ColumnName = "SysPriceListTempItemId";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Int32");
            dc.ColumnName = "SysPriceListTempHeadId";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.MaxLength = 256;
            dc.ColumnName = "ProductId";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.ColumnName = "Name";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Decimal");
            dc.ColumnName = "GNP";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.ColumnName = "Code";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.ColumnName = "PurchaseUnit";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.ColumnName = "SalesUnit";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Boolean");
            dc.ColumnName = "EnvironmentFee";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Boolean");
            dc.ColumnName = "Storage";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.ColumnName = "EAN";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.ColumnName = "ReplacesProduct";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Decimal");
            dc.ColumnName = "PackageSizeMin";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Decimal");
            dc.ColumnName = "PackageSize";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.ColumnName = "ProductLink";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.DateTime");
            dc.ColumnName = "PriceChangeDate";
            dc.Unique = false;
            dt.Columns.Add(dc);


            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Int32");
            dc.ColumnName = "PriceStatus";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Int32");
            dc.ColumnName = "Type";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Int32");
            dc.ColumnName = "SysCountryId";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Decimal");
            dc.ColumnName = "SalesPrice";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.Decimal");
            dc.ColumnName = "NetPrice";
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.ColumnName = "Manufacturer";
            dc.MaxLength = 100;
            dc.Unique = false;
            dt.Columns.Add(dc);

            dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.ColumnName = "ExtendedInfo";
            dc.MaxLength = 2048;
            dc.Unique = false;
            dt.Columns.Add(dc);

            int j = 0;
            foreach (var i in items)
            {
                j++;
                dr = dt.NewRow();
                dr["SysPriceListTempItemId"] = j;
                dr["SysPriceListTempHeadId"] = i.SysPriceListTempHead.SysPriceListTempHeadId;
                dr["ProductId"] = i.ProductId;
                dr["Name"] = i.Name;
                dr["GNP"] = i.GNP;
                dr["Code"] = i.Code;
                dr["PurchaseUnit"] = i.PurchaseUnit;
                dr["SalesUnit"] = i.SalesUnit;
                dr["EnvironmentFee"] = i.EnvironmentFee;
                dr["Storage"] = i.Storage;
                dr["EAN"] = i.EAN;
                dr["ReplacesProduct"] = i.ReplacesProduct;
                dr["PackageSizeMin"] = i.PackageSizeMin;
                dr["PackageSize"] = i.PackageSize;
                dr["ProductLink"] = i.ProductLink;
                if (i.PriceChangeDate != null)
                    dr["PriceChangeDate"] = i.PriceChangeDate;
                dr["PriceStatus"] = i.PriceStatus;
                dr["Type"] = i.Type;
                dr["SysCountryId"] = i.SysCountryId;
                dr["SalesPrice"] = i.SalesPrice ?? (object)DBNull.Value;
                dr["NetPrice"] = i.NetPrice ?? (object)DBNull.Value;
                dr["Manufacturer"] = i.Manufacturer;
                dr["ExtendedInfo"] = i.ExtendedInfo;

                dt.Rows.Add(dr);
            }
            items = null;
            return dt;
        }

        static private SoeSysPriceListProviderType GetProviderProductType(SoeSysPriceListProvider provider)
        {
            if (IsAhlsell(provider))
            {
                return SoeSysPriceListProviderType.Ahlsell;
            }
            else if (provider == SoeSysPriceListProvider.Bad_Värme)
            {
                return SoeSysPriceListProviderType.BadVarme;
            }
            else if (provider == SoeSysPriceListProvider.Bevego)
            {
                return SoeSysPriceListProviderType.Bevego;
            }
            else
            {
                return SoeSysPriceListProviderType.Unknown;
            }
        }

        private SysWholeseller GetWholesllerFromProvider(SoeSysPriceListProvider provider)
        {
            if (IsAhlsell(provider))
            {
                return GetSysWholesellerFromCache(2); //Ahlsell
            }
            else
            {
                return null;
            }
        }

        #endregion

        #endregion

        #region SysWholeseller

        public List<SysWholeseller> GetSysWholesellers()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                return sysEntitiesReadOnly.SysWholeseller.ToList();
            }
        }

        public Dictionary<int, SysWholeseller> GetSysWholesellersDict()
        {
            return SysDbCache.Instance.SysWholesellers;
        }

        public SysWholeseller GetSysWholesellerFromCache(int sysWholeSellerId)
        {
            //Uses SysDbCache
            SysWholeseller result;
            if (SysDbCache.Instance.SysWholesellers.TryGetValue(sysWholeSellerId, out result))
                return result;
            else
                return null;

            //return SysDbCache.Instance.SysWholesellers.Where(s => s.SysWholesellerId == sysWholeSellerId).FirstOrDefault();
        }

        public SysWholeseller GetSysWholeSellerFromName(string name, int? actorCompanyId, bool isOnlyInComp = false, int sysWholesellerEdiId = 0, int? sysCountryId = null)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysWholeSellerFromName(sysEntitiesReadOnly, name, actorCompanyId, isOnlyInComp, sysWholesellerEdiId, sysCountryId);
        }

        public SysWholeseller GetSysWholeSellerFromName(SOESysEntities entities, string name, int? actorCompanyId, bool isOnlyInComp = false, int sysWholesellerEdiId = 0, int? sysCountryId = null)
        {
            SysWholeseller sysWholeSeller = null;
            Supplier supplier = null;

            if (string.IsNullOrEmpty(name))
                return sysWholeSeller;

            List<SysWholeseller> sysWholesellers = new List<SysWholeseller>();

            if (sysWholesellerEdiId > 0)
            {
                sysWholesellers = WholeSellerManager.GetSysWholesellerEdi(sysWholesellerEdiId, loadSysWholeseller: true).SysWholeseller.ToList();
                if (sysWholesellers.Count == 1)
                    return sysWholesellers.First();
            }
            else if (actorCompanyId.HasValue)
            {
                sysWholesellers = WholeSellerManager.GetSysWholesellersByCompany(actorCompanyId.Value).ToList();
            }

            #region Search by complete name

            if (actorCompanyId.HasValue)
            {
                sysWholeSeller = (from ws in sysWholesellers
                                  where ws.Name.ToLower() == name.ToLower() &&
                                  ws.IsOnlyInComp == isOnlyInComp
                                  select ws).FirstOrDefault();
            }
            else
            {
                sysWholeSeller = (from ws in entities.SysWholeseller
                                  where ws.Name.ToLower() == name.ToLower() &&
                                  ws.IsOnlyInComp == isOnlyInComp
                                  select ws).FirstOrDefault();
            }

            #endregion

            #region Search by substring of name

            if (sysWholeSeller == null && (name.Length >= 4))
            {
                string shortName = name.Substring(0, 4).ToLower();
                if (actorCompanyId.HasValue)
                {
                    sysWholeSeller = (from ws in sysWholesellers
                                      where ws.Name.ToLower().StartsWith(shortName) &&
                                      ws.IsOnlyInComp == isOnlyInComp
                                      select ws).FirstOrDefault();
                }
                else
                {
                    sysWholeSeller = (from ws in entities.SysWholeseller
                                      where ws.Name.StartsWith(shortName) &&
                                      ws.IsOnlyInComp == isOnlyInComp &&
                                      (!sysCountryId.HasValue || ws.SysCountryId == sysCountryId)
                                      select ws).FirstOrDefault();
                }
            }

            #endregion

            #region Search by removing whitespace
            if (sysWholeSeller == null)
            {
                string noSpace = name.RemoveWhiteSpace().ToLower();
                if (actorCompanyId.HasValue)
                {
                    sysWholeSeller = (from ws in sysWholesellers
                                      where ws.Name.RemoveWhiteSpace().ToLower().StartsWith(noSpace) &&
                                      ws.IsOnlyInComp == isOnlyInComp
                                      select ws).FirstOrDefault();
                }
                else
                {
                    sysWholeSeller = (from ws in entities.SysWholeseller.ToList()
                                      where ws.Name.RemoveWhiteSpace().ToLower().StartsWith(noSpace) &&
                                      ws.IsOnlyInComp == isOnlyInComp
                                      select ws).FirstOrDefault();
                }
            }

            #endregion Search by removing whitespace

            #region Special cases (mostly EDI since names sometimes differs)
            //TODO Remove later (Check Espoon settings after removing)
            if (sysWholeSeller == null && name.ToUpper().Contains("LVI-DAHL"))
            {
                if (actorCompanyId.HasValue)
                {
                    sysWholeSeller = (from ws in sysWholesellers
                                      where ws.Name.ToUpper() == "DAHLFI" &&
                                      ws.IsOnlyInComp == isOnlyInComp
                                      select ws).FirstOrDefault();
                }
                else
                {
                    sysWholeSeller = (from ws in entities.SysWholeseller.ToList()
                                      where ws.Name.ToUpper() == "DAHLFI" &&
                                      ws.IsOnlyInComp == isOnlyInComp
                                      select ws).FirstOrDefault();
                }
            }

            if (sysWholeSeller == null && name.ToUpper().Contains("MEAB"))
            {
                if (actorCompanyId.HasValue)
                {
                    sysWholeSeller = (from ws in sysWholesellers
                                      where ws.Name.ToUpper() == "SELGA" &&
                                      ws.IsOnlyInComp == isOnlyInComp
                                      select ws).FirstOrDefault();
                }
                else
                {
                    sysWholeSeller = (from ws in entities.SysWholeseller.ToList()
                                      where ws.Name.ToUpper() == "SELGA" &&
                                      ws.IsOnlyInComp == isOnlyInComp
                                      select ws).FirstOrDefault();
                }
            }
            #endregion

            #region Search from compdatabase and supplier table
            //If not found then try to search from supplier table (IsEdiSupplier and SyswholeSellerId)
            if (sysWholeSeller == null)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                entitiesReadOnly.Supplier.NoTracking();
                supplier = (from s in entitiesReadOnly.Supplier
                            where s.Name.ToUpper() == name.ToUpper() &&
                            s.IsEDISupplier == true && s.SysWholeSellerId != null
                            select s).FirstOrDefault();

                if (supplier != null)
                {
                    if (actorCompanyId.HasValue)
                    {
                        sysWholeSeller = (from ws in sysWholesellers
                                          where ws.SysWholesellerId == supplier.SysWholeSellerId &&
                                          ws.IsOnlyInComp == isOnlyInComp
                                          select ws).FirstOrDefault();
                    }
                    else
                    {
                        sysWholeSeller = (from ws in entities.SysWholeseller.ToList()
                                          where ws.SysWholesellerId == supplier.SysWholeSellerId &&
                                          ws.IsOnlyInComp == isOnlyInComp
                                          select ws).FirstOrDefault();
                    }
                }
            }

            #endregion

            return sysWholeSeller;
        }

        public SysWholeseller GetWholesellerFromSupplierAgreement(SoeSupplierAgreementProvider providerType)
        {
            var name = Enum.GetName(typeof(SoeSupplierAgreementProvider), providerType);

            // Ugly workaround for now since Elkedjan has change name
            if (name == "Elkedjan")
                name = "Svenska Elkedjan AB";

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from sws in sysEntitiesReadOnly.SysWholeseller
                    where sws.Name == name
                    select sws).FirstOrDefault();
        }

        #endregion

        #region SysPriceListHead

        public List<SysPriceListHeadGridDTO> GetSysPriceListHeadGridDTOs()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                var heads = sysEntitiesReadOnly.SysPriceListHead
                    .OrderByDescending(e => e.SysPriceListHeadId)
                    .Select(e => new SysPriceListHeadGridDTO
                    {
                        Provider = e.Provider,
                        SysWholesellerId = e.SysWholesellerId,
                        SysWholesellerName = e.SysWholeseller.FirstOrDefault(f => f.SysWholesellerId == e.SysWholesellerId).Name,
                        Created = e.Created,
                        CreatedBy = e.CreatedBy,
                    }
                    ).ToList();

                heads.ForEach(h => h.ProviderName = Enum.GetName(typeof(SoeSysPriceListProvider), h.Provider));

                return heads;
            }
        }

        public List<SysPriceListHeadDTO> GetSysPriceListHeads(List<int> SysPriceListHeadIds)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                return sysEntitiesReadOnly.SysPriceListHead.Where(x => SysPriceListHeadIds.Contains(x.SysPriceListHeadId)).Select(p => new SysPriceListHeadDTO
                {
                    Created = p.Created,
                    Date = p.Date,
                    Provider = p.Provider,
                    SysPriceListHeadId = p.SysPriceListHeadId,
                    CreatedBy = p.CreatedBy,
                    Version = p.Version,
                    SysWholesellerId = p.SysWholesellerId
                }).ToList();
            }
        }


        public Dictionary<int, List<string>> GetSysPricelistCodeBySysWholesellerId(List<int> sysWholesellerIds)
        {
            Dictionary<int, List<string>> dict = new Dictionary<int, List<string>>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.PriceListImported.NoTracking();
            foreach (int sysWholesellerId in sysWholesellerIds)
            {
                List<string> sysPriceListCodes;
                if (sysWholesellerId == 67)
                {
                    sysPriceListCodes = (from pl in entitiesReadOnly.PriceListImported
                                         where pl.PriceListImportedHead.SysWholesellerId == sysWholesellerId
                                         orderby pl.Code
                                         select pl.Code).Distinct().ToList();
                }
                else
                {
                    using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                    sysPriceListCodes = (from pl in sysEntitiesReadOnly.SysPriceList
                                         where pl.SysWholesellerId == sysWholesellerId
                                         orderby pl.Code
                                         select pl.Code).Distinct().ToList();
                }
                dict.Add(sysWholesellerId, sysPriceListCodes);
            }

            return dict;
        }

        public static bool CompPriceListHeadExists(CompEntities entities, SoeCompPriceListProvider provider, int actorCompanyId, DateTime? date = null, int? version = null)
        {
            int providerId = (int)provider;

            IQueryable<PriceListImportedHead> query = (from plh in entities.PriceListImportedHead
                                                       where (plh.Provider == providerId &&
                                                            plh.ActorCompanyId == actorCompanyId)
                                                       select plh);

            if (version.HasValue)
            {
                query = query.Where(plh => plh.Version == version.Value);
            }

            if (date.HasValue)
            {
                query = query.Where(plh => plh.Date == date.Value);
            }

            return query.Any();
        }

        public static bool SysPriceListHeadExists(SOESysEntities entities, DateTime date, int? version, SoeSysPriceListProvider provider)
        {
            int providerId = (int)provider;

            return (from plh in entities.SysPriceListHead
                    where (plh.Provider == providerId) &&
                    (plh.Version == version) &&
                    (plh.Date == date)
                    select plh).Any();
        }

        #endregion

        #region SysProduct

        /// <summary>
        /// Get all SysProduct's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysProduct> GetSysProducts()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                return sysEntitiesReadOnly.SysProduct.ToList();
            }
        }

        public List<GenericType<int, string, string>> GetSysProducts(ExternalProductType type, int? noOfProducts = null)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                var query = (from p in sysEntitiesReadOnly.SysProduct
                             where p.Type == (int)type &&
                             p.SysCountryId == 1
                             orderby p.SysProductId descending
                             select p);

                if (noOfProducts != null)
                    return query.Take(noOfProducts.Value).Select(p => new GenericType<int, string, string>() { Field1 = p.SysProductId, Field2 = p.ProductId, Field3 = p.Name }).ToList();
                else
                    return query.Select(p => new GenericType<int, string, string>() { Field1 = p.SysProductId, Field2 = p.ProductId, Field3 = p.Name }).ToList();
            }
        }

        public Dictionary<int, SysProductDTO> GetSysProductsDTODict(ExternalProductType type, int sysCountry)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                IQueryable<SysProduct> query = (from p in sysEntitiesReadOnly.SysProduct
                                                where p.SysCountryId == sysCountry
                                                select p);

                if (type != ExternalProductType.Unknown)
                {
                    query = query.Where(p => p.Type == (int)type);
                }

                var products = query
                     .Select(p => new { p.SysProductId, p.Name, p.ProductId })
                     .ToList();
                return products.ToDictionary(p => p.SysProductId, p => new SysProductDTO { Name = p.Name, SysProductId = p.SysProductId, ProductId = p.ProductId });
            }
        }


        public Dictionary<int, SysProductDTO> GetSysProductsDTOById(List<int> sysProductIds)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                IQueryable<SysProduct> query = (from p in sysEntitiesReadOnly.SysProduct
                                                where sysProductIds.Contains(p.SysProductId)
                                                select p);

                var products = query
                     .Select(p => new { p.SysProductId, p.Name, p.ProductId, p.Type, p.ExtendedInfo, p.ImageFileName, p.ExternalId })
                     .ToList();
                return products.ToDictionary(p => p.SysProductId, p => new SysProductDTO { Name = p.Name, SysProductId = p.SysProductId, ProductId = p.ProductId, Type = p.Type, ExtendedInfo = p.ExtendedInfo, ImageFileName = p.ImageFileName, ExternalId = p.ExternalId ?? 0 });
            }
        }

        public ILookup<string, SysProductDTO> GetSysProductsNumberDict(ExternalProductType type, int sysCountry)
        {
            using (var soeSysEntity = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    soeSysEntity.SysProduct.AsNoTracking();
                    soeSysEntity.CommandTimeout = 300;
                    IQueryable<SysProduct> query = (from p in soeSysEntity.SysProduct
                                                    where p.SysCountryId == sysCountry &&
                                                          p.Type == (int)type
                                                    select p);

                    return query.Select(x => new SysProductDTO
                    {
                        Name = x.Name,
                        ProductId = x.ProductId,
                        SysProductId = x.SysProductId,
                    }).ToLookup(p => p.ProductId);
                }
            }
        }

        public SysProductDTO GetSysProduct(int sysProductId)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                return sysEntitiesReadOnly.SysProduct.Where(p => p.SysProductId == sysProductId).FirstOrDefault().ToDTO();
            }
        }

        #endregion

        #region SysProductGroups

        public List<GenericType<int, string>> GetSysProductGroupsMapping(ExternalProductType type)
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                return (from p in sysEntitiesReadOnly.SysProductGroup
                        where p.Type == (int)type
                        select p)
                                             .Select(p => new GenericType<int, string>() { Field1 = p.SysProductGroupId, Field2 = p.Identifier }).ToList();
            }
        }

        #endregion

        public static List<SysPricelistProviderDTO> GetSysPriceListProviders()
        {
            var providers = new List<SysPricelistProviderDTO>();
            foreach (var value in Enum.GetValues(typeof(SoeSysPriceListProvider)))
            {
                var id = (int)value;
                var name = Enum.GetName(typeof(SoeSysPriceListProvider), value);
                providers.Add(new SysPricelistProviderDTO() { Id = id, Name = name });
            }
            return providers.OrderBy(x => x.Name).ToList();
        }

        public Dictionary<string, SoeSysPriceListProviderType> GetProductCodesForWholeseller(List<int> sysWholesellerIds)
        {
            var dateFrom = DateTime.Today.AddMonths(-12);
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                return (from p in sysEntitiesReadOnly.SysPriceList
                        where
                            sysWholesellerIds.Contains(p.SysPriceListHead.SysWholesellerId) &&
                            p.SysPriceListHead.Created >= dateFrom
                        select p).Select(p => new { p.Code, p.SysProduct.Type }).DistinctBy(x => x.Code).ToDictionary(p => p.Code, p => (SoeSysPriceListProviderType)p.Type);
            }
        }
    }
}

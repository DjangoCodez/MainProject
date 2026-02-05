using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Linq;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Common
{
    [RoutePrefix("Common/Common")]
    public class CommonController : WebApiExternalBase
    {

        #region Constructor

        public CommonController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            
        }

        #endregion

        #region Methods

        #region Company Information

        /// <summary>
        /// Import Companyinformation
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="companyInformationIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CompanyInformation/")]
        public IHttpActionResult SaveCompanyInformation(Guid companyApiKey, Guid connectApiKey, string token, List<CompanyInformationIODTO> companyInformationIODTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            CompanyInformationIOItem companyInformationIOItem = new CompanyInformationIOItem();

            companyInformationIOItem.companyInformationIOs = new List<CompanyInformationIODTO>();
            companyInformationIOItem.companyInformationIOs.AddRange(companyInformationIODTOs);
            var result = importExportManager.ImportCompanyInformationIO(companyInformationIOItem, TermGroup_IOImportHeadType.CompanyInformation, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region SideDictionary

        /// <summary>
        /// Import Different SideDictionaries, Set type on each item 
        ///        Unkown = 0,
        ///        VatCodes = 1,
        ///        PaymentConditions = 2,
        ///        DeliveryConditions = 3,
        ///        Currency = 4,
        ///        DeliveryTypes = 5,
        ///        PricelistTypes = 6,
        ///        Categories = 7,
        ///        ProductGroup = 8,
        ///        CategoryGroups = 9,
        ///        Stocks = 10,
        ///        PayrollProduct = 11,
        ///        TimeCode = 12,
        ///        PaymentMethod = 13,
        ///        Endreason = 14,
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sideDictionaryIODTOs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SideDictionary/")]
        public IHttpActionResult SaveSideDictionarys(Guid companyApiKey, Guid connectApiKey, string token, List<SideDictionaryIODTO> sideDictionaryIODTOs)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            SideDictionaryIOItem sideDictionaryIOItem = new SideDictionaryIOItem();

            sideDictionaryIOItem.SideDictionaryIOs = new List<SideDictionaryIODTO>();
            sideDictionaryIOItem.SideDictionaryIOs.AddRange(sideDictionaryIODTOs);
            var result = importExportManager.ImportSideDictionaryIO(sideDictionaryIOItem, TermGroup_IOImportHeadType.SideDictionary, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sideDictionaryTypes"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SideDictionary/")]
        [ResponseType(typeof(List<SideDictionaryIODTO>))]
        public IHttpActionResult GetSideDictionaries(Guid companyApiKey, Guid connectApiKey, string token, [FromUri] List<IOImportSideDictionaryType> sideDictionaryTypes)
        {

            #region Validation   

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var sideDictionaryIODTOs = importExportManager.GetSideDictionaryIODTOs(apiManager.ActorCompanyId, sideDictionaryTypes);
            return Content(HttpStatusCode.OK, sideDictionaryIODTOs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="sideDictionaryTypes"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SideDictionary/")]
        [ResponseType(typeof(List<SideDictionaryIODTO>))]
        public IHttpActionResult GetSideDictionaries(Guid companyApiKey, Guid connectApiKey, string token, bool getVatCodes, bool getPaymentConditions, bool getDeliveryConditions, bool getCurrencies, bool getDeliveryTypes, bool getPricelistTypes, bool getCategories, bool getProductGroups, bool getCategoryGroups, bool getStocks, bool getPayrollProducts, bool getTimeCodes, bool getPaymentMethods, bool getEndreasons)
        {
            #region Validation   

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var sideDictionaryTypes = new List<IOImportSideDictionaryType>();
            if (getVatCodes) { sideDictionaryTypes.Add(IOImportSideDictionaryType.VatCodes); }
            if (getPaymentConditions) { sideDictionaryTypes.Add(IOImportSideDictionaryType.PaymentConditions); }
            if (getDeliveryConditions) { sideDictionaryTypes.Add(IOImportSideDictionaryType.DeliveryConditions); }
            if (getCurrencies) { sideDictionaryTypes.Add(IOImportSideDictionaryType.Currency); }
            if (getDeliveryTypes) { sideDictionaryTypes.Add(IOImportSideDictionaryType.DeliveryTypes); }
            if (getPricelistTypes) { sideDictionaryTypes.Add(IOImportSideDictionaryType.PricelistTypes); }
            if (getCategories) { sideDictionaryTypes.Add(IOImportSideDictionaryType.Categories); }
            if (getProductGroups) { sideDictionaryTypes.Add(IOImportSideDictionaryType.ProductGroup); }
            if (getCategoryGroups) { sideDictionaryTypes.Add(IOImportSideDictionaryType.CategoryGroups); }
            if (getStocks) { sideDictionaryTypes.Add(IOImportSideDictionaryType.Stocks); }
            if (getPayrollProducts) { sideDictionaryTypes.Add(IOImportSideDictionaryType.PayrollProduct); }
            if (getTimeCodes) { sideDictionaryTypes.Add(IOImportSideDictionaryType.TimeCode); }
            if (getPaymentMethods) { sideDictionaryTypes.Add(IOImportSideDictionaryType.PaymentMethod); }
            if (getEndreasons) { sideDictionaryTypes.Add(IOImportSideDictionaryType.Endreason); }

            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var sideDictionaryIODTOs = importExportManager.GetSideDictionaryIODTOs(apiManager.ActorCompanyId, sideDictionaryTypes);

            return Content(HttpStatusCode.OK, sideDictionaryIODTOs);
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("AttestStates/")]
        [ResponseType(typeof(List<AttestStateIODTO>))]
        public IHttpActionResult GetAttestStates(Guid companyApiKey, Guid connectApiKey, string token)
        {

            #region Validation   

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var attestManager = new AttestManager(apiManager.GetParameterObject());
            var attestStates = attestManager.GetAttestStates(apiManager.ActorCompanyId, TermGroup_AttestEntity.Unknown, SoeModule.None);
            var attestStatesIO = attestStates.Select(x => new AttestStateIODTO {Name = x.Name,
                                                                                Closed = x.Closed,
                                                                                Hidden = x.Hidden,
                                                                                Locked = x.Locked,
                                                                                Initial = x.Initial,
                                                                                Sort = x.Sort,
                                                                                EntityName = ((TermGroup_AttestEntity)x.Entity).ToString(),
                                                                                ModuleName = ((SoeModule)x.Module).ToString() }).ToList();
            return Content(HttpStatusCode.OK, attestStatesIO);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("VatCodes/")]
        [ResponseType(typeof(List<VatCodeIODTO>))]
        public IHttpActionResult GetVatCodes(Guid companyApiKey, Guid connectApiKey, string token)
        {
            #region Validation   

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var accountManager = new AccountManager(apiManager.GetParameterObject());
            var vatCodes = accountManager.GetVatCodes(apiManager.ActorCompanyId);

            var vatCodesIO = vatCodes.Select(x => new VatCodeIODTO
            {
                Name = x.Name,
                Code = x.Code,
                Percent = x.Percent
            }).ToList();
            return Content(HttpStatusCode.OK, vatCodesIO);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <param name="PaymentType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PaymentMethods/")]
        [ResponseType(typeof(List<PaymentMethodIODTO>))]
        public IHttpActionResult PaymentMethods(Guid companyApiKey, Guid connectApiKey, string token, int PaymentType)
        {
            #region Validation   

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            var paymentManager = new PaymentManager(apiManager.GetParameterObject());
            var methods = paymentManager.GetPaymentMethods( (SoeOriginType)PaymentType, apiManager.ActorCompanyId);

            var methodsIO = methods.Select(x => new PaymentMethodIODTO
            {
                Name = x.Name,
                UseInCashSales = x.UseInCashSales,
                UseRoundingInCashSales = x.UseRoundingInCashSales,
                PaymentType = x.PaymentType
            }).ToList();
            return Content(HttpStatusCode.OK, methodsIO);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Currencies/")]
        [ResponseType(typeof(List<CurrencyIODTO>))]
        public IHttpActionResult Currencies(Guid companyApiKey, Guid connectApiKey, string token)
        {
            #region Validation   

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion
            
            var currencyManager = new CountryCurrencyManager(apiManager.GetParameterObject());
            var currencies = currencyManager.GetCurrencies(apiManager.ActorCompanyId);

            var currenciesIO = currencies.Select(x => new CurrencyIODTO
            {
                Name = currencyManager.GetSysCurrencyCached(x.SysCurrencyId,true)?.Name,
                Code = currencyManager.GetCurrencyCode(x.SysCurrencyId)
            }).ToList();
            return Content(HttpStatusCode.OK, currenciesIO);
        }

        #region BatchCompanyValidation

        [HttpPost]
        [Route("BatchCompany/")]
        public IHttpActionResult GetBatchCompanyResponse(CompanyBatchValidationRequest companyBatchRequest)
        {
            ImportExportManager importExportManager = new ImportExportManager(null);
            var result = importExportManager.GetValidCompanies(companyBatchRequest);
            return Content(HttpStatusCode.OK, result);
        }
        #endregion
    }
}
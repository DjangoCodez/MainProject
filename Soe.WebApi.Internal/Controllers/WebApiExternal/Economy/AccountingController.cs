using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.WebApiExternal.Economy
{
    [RoutePrefix("Economy/Accounting")]
    public class AccountingController : WebApiExternalBase
    {
        

        #region Constructor

        public AccountingController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }

        #endregion

        #region Methods

        #region AccountYear

        /// <summary>
        /// Get AccountYears. You will also get AccountPeriods and VoucherSeries connected to the years. 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("AccountYears/")]
        public IHttpActionResult GetAccountYears(Guid companyApiKey, Guid connectApiKey, string token)
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
            var accountYearIODTOs = importExportManager.GetAccountYearIODTOs(apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, accountYearIODTOs);
        }

        #endregion

        #region VoucherSeries

        /// <summary>
        /// Get VoucherSeries
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>
        /// <param name="token"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("VoucherSeries/")]
        public IHttpActionResult GetVoucherSeries(Guid companyApiKey, Guid connectApiKey, string token, DateTime date)
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
            var accountYearIODTOs = importExportManager.GetVoucherSeriesIODTOs(apiManager.ActorCompanyId,date);
            return Content(HttpStatusCode.OK, accountYearIODTOs);
        }

        #endregion

        #region Voucher

        /// <summary>
        /// Get multiple Vouchers. You can get the voucherSeries and AccountYears from /AccountYears
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="voucherSeriesId"></param>
        /// <param name="voucherHeadId"></param>
        /// <param name="accountYearId"></param>
        /// <param name="voucherNr"></param>
        /// <param name="voucherNrFrom"></param>
        /// <param name="voucherNrTo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Vouchers/")]
        [ResponseType(typeof(List<VoucherHeadIODTO>))]
        public IHttpActionResult GetVouchers(Guid companyApiKey, Guid connectApiKey, string token, DateTime dateFrom, DateTime dateTo, int? voucherSeriesId = null, int? voucherHeadId = null, int? accountYearId = null, int? voucherNr = null, int? voucherNrFrom = null, int? voucherNrTo = null)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Economy_Accounting_Vouchers, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            if (voucherNr.GetValueOrDefault() > 0)
            {
                voucherNrFrom = voucherNr;
                voucherNrTo = voucherNr;
            }
            
            var importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var voucherHeadIODTOs = importExportManager.GetVoucherHeadIODTOs(apiManager.ActorCompanyId, dateFrom, dateTo, voucherSeriesId, voucherHeadId, accountYearId, voucherNrFrom, voucherNrTo);
            return Content(HttpStatusCode.OK, voucherHeadIODTOs);
        }

        /// <summary>
        /// Get one single Voucher.
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="voucherNr"></param>
        /// <param name="accountingYearId"></param>
        /// <param name="voucherSeriesId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Voucher/")]
        [ResponseType(typeof(VoucherHeadIODTO))]
        public IHttpActionResult GetVoucher(Guid companyApiKey, Guid connectApiKey, string token, int? voucherNr, int? accountingYearId, int? voucherSeriesId)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Economy_Accounting_Vouchers, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            var voucherHeadIODTOs = importExportManager.GetVoucherHeadIODTOs(apiManager.ActorCompanyId, CalendarUtility.DATETIME_DEFAULT, CalendarUtility.DATETIME_MAXVALUE, voucherSeriesId, null, accountingYearId, voucherNr, voucherNr);
            return Content(HttpStatusCode.OK, voucherHeadIODTOs.FirstOrDefault());
        }

        /// <summary>
        /// Save multiple Vouchers. You can get the voucherSeries and AccountYears from /AccountYears
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>

        /// <param name="voucherHeadIODTOs"></param>
        /// <param name="accountYearId"></param>
        /// <param name="voucherSeriesId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Vouchers/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveVouchers(Guid companyApiKey, Guid connectApiKey, string token, List<VoucherHeadIODTO> voucherHeadIODTOs, int accountYearId, int voucherSeriesId)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, Feature.Economy_Accounting_Vouchers_Edit, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager importExportManager = new ImportExportManager(apiManager.GetParameterObject());
            VoucherHeadIOItem voucherHeadIOItem = new VoucherHeadIOItem { Vouchers = new List<VoucherHeadIODTO>() };
            voucherHeadIOItem.Vouchers.AddRange(voucherHeadIODTOs);

            var result = importExportManager.ImportVoucherIO(voucherHeadIOItem, TermGroup_IOImportHeadType.Voucher, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId, true, accountYearId, voucherSeriesId);
            return Content(HttpStatusCode.OK, result);
        }
        #endregion

        #region Account

        /// <summary>
        /// Get AccountYears. You will also get AccountPeriods and VoucherSeries connected to the years. 
        /// </summary>
        /// <param name="companyApiKey"></param>
        /// <param name="connectApiKey"></param>
        /// <param name="connectObject"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Account/")]
        [ResponseType(typeof(AccountIODTO))]
        public IHttpActionResult GetAccount(Guid companyApiKey, Guid connectApiKey, string token, string accountNr, int dimNr)
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
            var accountIODTO = importExportManager.GetAccountIODTO(accountNr, dimNr,apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, accountIODTO);
        }

        [HttpPost]
        [Route("Account/")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveAccounts(Guid companyApiKey, Guid connectApiKey, string token, List<AccountIODTO> accountIODTOs)
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
            AccountIOItem accountIOItem = new AccountIOItem();

            accountIOItem.AccountIOs = new List<AccountIODTO>();
            accountIOItem.AccountIOs.AddRange(accountIODTOs);
            
            var result = importExportManager.ImportAccountIO(accountIOItem, TermGroup_IOImportHeadType.Account, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, apiManager.ActorCompanyId);
            return Content(HttpStatusCode.OK, result);
        }

        #endregion

        #region GeneralLedger
        [HttpPost]
        [Route("GeneralLedger")]
        [ResponseType(typeof(GeneralLedgerIO))]
        public IHttpActionResult GeneralLedger(Guid companyApiKey, Guid connectApiKey, string token, GeneralLedgerParams filterParams)
        {
            #region Validation

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }

            #endregion

            ImportExportManager iem = new ImportExportManager(apiManager.GetParameterObject());
            var result = iem.ImportGeneralLedgerIO(filterParams);
            return Content(HttpStatusCode.OK, result);
        }
        #endregion

        #endregion

    }
}
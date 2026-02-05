using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Shared/Customer")]
    public class CustomerV2Controller : SoeApiController
    {
        #region Variables

        private readonly CustomerManager cum;
        private readonly InvoiceManager im;
        private readonly ContactManager com;
        private readonly FeatureManager fm;

        #endregion

        #region Constructor

        public CustomerV2Controller(
            CustomerManager cum, 
            InvoiceManager im, 
            ContactManager com,
            FeatureManager fm)
        {
            this.cum = cum;
            this.im = im;
            this.com = com;
            this.fm = fm;
        }

        #endregion

        #region Customer

        [HttpPost]
        [Route("Search")]
        public IHttpActionResult GetCustomersBySearch(CustomerSearchModel model)
        {
            CustomerSearchDTO dto = new CustomerSearchDTO()
            {
                ActorCustomerId = model.ActorCustomerId,
                CustomerNr = model.CustomerNr,
                Name = model.Name,
                BillingAddress = model.BillingAddress,
                DeliveryAddress = model.DeliveryAddress,
                Note = model.Note
            };
            return Content(HttpStatusCode.OK, cum.GetCustomersBySearch(dto, base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetCustomers(Boolean onlyActive)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomersByCompanySmall(base.ActorCompanyId, onlyActive, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("Grid")]
        public IHttpActionResult GetCustomersForGrid(Boolean onlyActive, int? customerId = null)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomersForGrid(base.ActorCompanyId, onlyActive, base.RoleId, base.UserId, customerId));
        }

        [HttpGet]
        [Route("Dict")]
        public IHttpActionResult GetCustomersByCompanyDict(Boolean onlyActive, Boolean addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomersByCompanyDict(base.ActorCompanyId, onlyActive, addEmptyRow, base.RoleId, base.UserId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{customerId:int}/{loadActor:bool}/{loadAccount:bool}/{loadNote:bool}/{loadCustomerUser:bool}/{loadContactAddresses:bool}/{loadCategories:bool}")]
        public IHttpActionResult GetCustomer(int customerId, bool loadActor, bool loadAccount, bool loadNote, bool loadCustomerUser, bool loadContactAddresses, bool loadCategories)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomer(customerId, loadActor, loadAccount, loadCustomerUser, loadContactAddresses, loadCategories, true).ToDTO(loadContactAddresses, loadAccount, loadNote));
        }

        [HttpGet]
        [Route("Export/{customerId:int}")]
        public IHttpActionResult GetCustomerForExport(int customerId)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomer(customerId, true, true, true, true, true, true).ToDTO(true, true, true));
        }

        [HttpGet]
        [Route("CashCustomer")]
        public IHttpActionResult GetCashCustomer()
        {
            return Content(HttpStatusCode.OK, cum.GetDefaultCashCustomerId(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("CustomerCentralCountersAndBalance/")]
        public List<ChangeStatusGridViewBalanceDTO> GetCustomerCentralCountersAndBalance(GetCustomerCentralCountersAndBalanceModel model)
        {
            return im.GetChangeStatusGridViewsCountersAndBalanceForCustomerCentral(model.CounterTypes, model.CustomerId, base.ActorCompanyId);
        }

        [HttpPost]
        [Route("Statistics")]
        public IHttpActionResult GetCustomerStatistics(CustomerStatisticsModel model)
        {
            return Content(HttpStatusCode.OK, im.GetProductStatisticsPerCustomer(base.ActorCompanyId, model.CustomerId, model.AllItemSelection));
        }

        [HttpPost]
        [Route("StatisticsAllCustomers")]
        public IHttpActionResult getSalesStatisticsGridData(GeneralProductStatisticsModel model)
        {
            return Content(HttpStatusCode.OK, im.GetProductStatistics(model.OriginType, model.FromDate, model.ToDate));
        }

        [HttpGet]
        [Route("NextCustomerNr")]
        public IHttpActionResult GetNextCustomerNr()
        {
            return Content(HttpStatusCode.OK, cum.GetNextCustomerNr(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Reference/{customerId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCustomerReferences(int customerId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, com.GetCustomerReferencesDict(customerId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Email/{customerId:int}/{loadContactPersonsEmails:bool}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCustomerEmailAddresses(int customerId, bool loadContactPersonsEmails, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomerEmailAddresses(customerId, loadContactPersonsEmails, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("GLN/{customerId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCustomerGlnNumbers(int customerId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomerGlnNumbers(customerId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpDelete]
        [Route("{customerId:int}")]
        public IHttpActionResult DeleteCustomer(int customerId)
        {
            return Content(HttpStatusCode.OK, cum.DeleteCustomer(customerId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveCustomer(SaveCustomerModel saveModel)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cum.SaveCustomer(saveModel.Customer, null, saveModel.Customer.ContactPersons, saveModel.HouseHoldTaxApplicants, base.ActorCompanyId, saveModel.ExtraFields));
        }

        [HttpPost]
        [Route("UpdateState")]
        public IHttpActionResult UpdateCustomersState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cum.UpdateCustomersState(model.Dict));
        }

        [HttpPost]
        [Route("UpdateIsPrivatePerson")]
        public IHttpActionResult UpdateIsPrivatePerson(List<UpdateIsPrivatePerson> items)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cum.UpdateCustomersIsPrivatePerson(items.ToDictionary(k => k.id, v => v.isPrivatePerson)));
        }


        [HttpPost]
        [Route("UpdateGrid")]
        public IHttpActionResult UpdateGrid(
            CustomerUpdateGrid customerUpdateGridDto)
        {

            bool isBillingAuthorized = fm.HasRolePermission(
                Feature.Billing_Customer_Customers_Edit,
                Permission.Modify,
                RoleId,
                ActorCompanyId,
                LicenseId);

            bool isEconomyAuthorized = fm.HasRolePermission(
                Feature.Economy_Customer_Customers_Edit,
                Permission.Modify,
                RoleId,
                ActorCompanyId,
                LicenseId);

            if (!ModelState.IsValid)
            {
                return Error(
                    HttpStatusCode.BadRequest, ModelState, null, null);

            }
            else if (!isBillingAuthorized && !isEconomyAuthorized)
            {
                return Content(
                    HttpStatusCode.Forbidden,
                    "The user does not have required permission");
            }
            else
            {
                ActionResult customerStateResult = null;
                ActionResult privatePersonResult = null;

                if (customerUpdateGridDto.model?.Dict.Any() == true)
                {
                    customerStateResult = cum
                        .UpdateCustomersState(customerUpdateGridDto.model.Dict);

                    if (!customerStateResult.Success)
                    {
                        return Content(HttpStatusCode.OK, customerStateResult);
                    }
                }

                if (customerUpdateGridDto.items?.Any() == true
                    && (customerStateResult == null || customerStateResult.Success))
                {
                    privatePersonResult = cum
                        .UpdateCustomersIsPrivatePerson(
                            customerUpdateGridDto.items
                            .ToDictionary(k => k.id, v => v.isPrivatePerson));

                    if (!privatePersonResult.Success)
                    {
                        return Content(HttpStatusCode.OK, privatePersonResult);
                    }

                }

                ActionResult result = new ActionResult();
                result.Success = customerStateResult?.Success == true
                    || privatePersonResult?.Success == true;

                return Content(HttpStatusCode.OK, result);

            }
        }


        #endregion

        #region EInvoice

        [HttpPost]
        [Route("SearchEinvoiceRecipients")]
        public IHttpActionResult GetEInvoiceRecipients(EInvoiceRecipientSearchDTO model)
        {
            var eInvoiceRecipientResponse = cum.GetCustomerEInvoiceRecipients(model);
            return Content(HttpStatusCode.OK, eInvoiceRecipientResponse);
        }

        #endregion
    }
}
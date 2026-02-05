using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.API.InExchange;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.Finvoice;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class CustomerManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public CustomerManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Customer

        public List<Customer> GetCustomersByCompany(int actorCompanyId, bool onlyActive, int? roleId = null, int? userId = null, bool loadContact = false, bool loadCategories = false, bool loadContactAddresses = false, bool loadCustomerProducts = false, bool loadAccount = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Customer.NoTracking();
            return GetCustomersByCompany(entities, actorCompanyId, onlyActive, roleId, userId, loadContact, loadCategories, loadContactAddresses, false, loadCustomerProducts, loadAccount);
        }

        public List<Customer> GetCustomersByCompany(CompEntities entities, int actorCompanyId, bool onlyActive, int? roleId = null, int? userId = null, bool loadContact = false, bool loadCategories = false, bool loadContactAddresses = false, bool onlyOneTime = false, bool loadCustomerProducts = false, bool loadAccount = false)
        {
            IQueryable<Customer> query = entities.Customer;
            if (loadContactAddresses)
                query = query.Include("Actor.Contact.ContactECom").Include("Actor.Contact.ContactAddress.ContactAddressRow");
            else if (loadContact)
                query = query.Include("Actor.Contact");
            else
                query = query.Include("Actor");

            if (loadCustomerProducts)
                query = query.Include("CustomerProduct");

            if (loadAccount)
                query = query.Include("CustomerAccountStd.AccountStd.Account.AccountDim").Include("CustomerAccountStd.AccountInternal.Account.AccountDim");

            List<CompanyCategoryRecord> categoryRecordsForCompany = loadCategories ? CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Customer, actorCompanyId) : null;

            List<Customer> customers;
            if (onlyActive)
                customers = query.Where(c => c.ActorCompanyId == actorCompanyId && c.State == (int)SoeEntityState.Active).ToList();
            else
                customers = query.Where(c => c.ActorCompanyId == actorCompanyId && c.State != (int)SoeEntityState.Deleted).ToList();

            if (onlyOneTime)
                customers = customers.Where(c => c.IsOneTimeCustomer).ToList();
            if (roleId.HasValue && userId.HasValue && roleId.Value > 0 && userId.Value > 0)
                customers = FilterCustomersUsers(entities, customers, actorCompanyId, roleId.Value, userId.Value);

            if (categoryRecordsForCompany != null && categoryRecordsForCompany.Any())
            {
                foreach (Customer customer in customers)
                {
                    customer.CategoryNames = new List<string>();
                    foreach (CompanyCategoryRecord ccr in categoryRecordsForCompany.GetCategoryRecords(SoeCategoryRecordEntity.Customer, customer.ActorCustomerId, date: null, discardDateIfEmpty: true))
                    {
                        customer.CategoryNames.Add(ccr.Category.Name);
                    }

                }
            }

            return customers.OrderBy(c => c.CustomerNrSort).ToList();
        }

        public List<Customer> GetCustomers(int actorCompanyId, bool loadActor = false, bool loadAccount = false, bool loadNote = false, bool loadCustomerUser = false, bool loadContactAddresses = false, bool loadDeliveryAndPaymentCondition = false, bool loadCategories = false, List<int> customerIds = null, string orgNr = null, DateTime? modifiedSince = null, string customerNr = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Customer.NoTracking();
            return GetCustomers(entities, actorCompanyId, loadActor, loadAccount, loadNote, loadCustomerUser, loadContactAddresses, loadDeliveryAndPaymentCondition, loadCategories, customerIds, orgNr, modifiedSince, customerNr);
        }

        public List<Customer> GetCustomers(CompEntities entities, int actorCompanyId, bool loadActor = false, bool loadAccount = false, bool loadNote = false, bool loadCustomerUser = false, bool loadContactAddresses = false, bool loadCategories = false, bool loadDeliveryAndPaymentCondition = false, List<int> customerIds = null, string orgNr = null, DateTime? modifiedSince = null, string customerNr = null)
        {
            IQueryable<Customer> query = (from c in entities.Customer
                                          where (c.ActorCompanyId == actorCompanyId &&
                                          c.State != (int)SoeEntityState.Deleted)
                                          select c);

            query = AddCustomerIncludes(loadActor, loadAccount, loadCustomerUser, loadContactAddresses, loadDeliveryAndPaymentCondition, false, true, query);

            if (!string.IsNullOrEmpty(orgNr))
            {
                query = query.Where(c => c.OrgNr == orgNr);
            }
            else if (!string.IsNullOrEmpty(customerNr))
            {
                query = query.Where(c => c.CustomerNr == customerNr);
            }
            else if (!customerIds.IsNullOrEmpty())
                query = query.Where(c => customerIds.Contains(c.ActorCustomerId));
            else if (modifiedSince.HasValue)
            {
                query = query.Where(c => (c.Modified > modifiedSince.Value || c.Created > modifiedSince.Value));
            }

            var customers = query.ToList();

            if (loadCategories && customers != null)
            {
                customerIds = customers.Select(c => c.ActorCustomerId).ToList();

                var companyCategoryRecord = (from c in entities.CompanyCategoryRecord
                                             where customerIds.Contains(c.RecordId) &&
                                             c.Entity == (int)SoeCategoryRecordEntity.Customer &&
                                             c.Category.Type == (int)SoeCategoryType.Customer &&
                                             c.Category.ActorCompanyId == actorCompanyId &&
                                             c.Category.State == (int)SoeEntityState.Active
                                             select c).ToList();

                foreach (var customer in customers)
                    customer.CategoryIds = companyCategoryRecord.Where(c => c.RecordId == customer.ActorCustomerId).Select(r => r.CategoryId).ToList();

            }

            return customers;
        }

        private static IQueryable<Customer> AddCustomerIncludes(bool loadActor, bool loadAccount, bool loadCustomerUser, bool loadContactAddresses, bool loadDeliveryAndPaymentCondition, bool loadActorConsent, bool loadCustomerProduct, IQueryable<Customer> query)
        {

            if (loadActorConsent)
                query = query.Include("Actor").Include("Actors").Include("Actor.ActorConsent");
            else if (loadActor)
                query = query.Include("Actor").Include("Actors");

            if (loadCustomerProduct)
                query = query.Include("CustomerProduct");

            if (loadAccount)
                query = query.Include("CustomerAccountStd.AccountStd.Account.AccountDim").Include("CustomerAccountStd.AccountInternal.Account.AccountDim");

            if (loadCustomerUser)
                query = query.Include("CustomerUser.User");

            if (loadContactAddresses)
                query = query.Include("Actor.Contact.ContactECom").Include("Actor.Contact.ContactAddress.ContactAddressRow");

            if (loadDeliveryAndPaymentCondition)
                query = query.Include("PaymentCondition").Include("DeliveryType").Include("DeliveryCondition").Include("PriceListType");

            return query;
        }

        public List<CustomerGridDTO> GetCustomersForGrid(int actorCompanyId, bool onlyActive, int? roleId = null, int? userId = null, int? customerId = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var terms = base.GetTermGroupContent(TermGroup.InvoiceDeliveryType);

            var customers = entitiesReadOnly.GetCustomersForList(actorCompanyId, onlyActive ? 0 : 1).Select(c =>
                new CustomerGridDTO
                {
                    ActorCustomerId = c.ActorCustomerId,
                    CustomerNr = c.CustomerNr,
                    OrgNr = c.OrgNr,
                    Name = c.Name,
                    Categories = c.Categories,
                    GridAddressText = c.VisitingAddressText,
                    GridBillingAddressText = c.BilingAddressText,
                    GridDeliveryAddressText = c.DeliveryAddressText,
                    GridEmailText = c.Email,
                    GridHomePhoneText = c.PhoneHome,
                    GridMobilePhoneText = c.PhoneMobile,
                    GridWorkPhoneText = c.PhoneJob,
                    InvoiceReference = c.InvoiceReference,
                    InvoicePaymentService = c.InvoicePaymentService.GetValueOrDefault(),
                    IsActive = c.State == (int)SoeEntityState.Active,
                    IsPrivatePerson = c.IsPrivatePerson,
                    GridPaymentServiceText = c.InvoicePaymentService.GetValueOrDefault() == (int)TermGroup_SysPaymentService.Autogiro ? "Autogiro" : "",
                    InvoiceDeliveryType = c.InvoiceDeliveryType.GetValueOrDefault(),
                }
            ).OrderBy(c => c.CustomerNr).ToList();

            if (roleId.HasValue && userId.HasValue && roleId.Value > 0 && userId.Value > 0)
                customers = FilterCustomersUsers(entitiesReadOnly, customers, actorCompanyId, roleId.Value, userId.Value);

            if (customerId.HasValue)
                customers = customers.Where(c => c.ActorCustomerId == customerId).ToList();

            foreach (var customer in customers.Where(c => c.InvoiceDeliveryType > 0))
            {
                var type = terms.FirstOrDefault(t => t.Id == customer.InvoiceDeliveryType);
                if (type != null)
                    customer.InvoiceDeliveryTypeText = type.Name;
            }

            return customers;
        }

        public List<Customer> GetCustomersByCustomerNumber(int actorCompanyId, string search, int no, int? roleId, int? userId = null)
        {
            List<Customer> customers = GetCustomersByCompany(actorCompanyId, true, roleId, userId);

            return (from c in customers
                    where (c.CustomerNr.ToLower().Contains(search.ToLower()) || c.Name.ToLower().Contains(search.ToLower()))
                    orderby c.CustomerNr ascending
                    select c).Take(no).ToList();
        }

        public Customer GetCustomerByNr(int actorCompanyId, string customerNr, List<Customer> customers = null, bool tryMatchingStringasNumber = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Customer.NoTracking();
            return GetCustomerByNr(entities, actorCompanyId, customerNr, customers, tryMatchingStringasNumber);
        }

        public Customer GetCustomerByNr(CompEntities entities, int actorCompanyId, string customerNr, List<Customer> customers = null, bool tryMatchingStringasNumber = false)
        {
            if (string.IsNullOrEmpty(customerNr))
                return null;

            Customer customer = null;

            if (customers != null)
            {
                customer = customers.FirstOrDefault(c => c.CustomerNr.ToLower() == customerNr.ToLower());
                if (tryMatchingStringasNumber && customer == null)
                {
                    if (int.TryParse(customerNr.Trim(), out int v))
                        customerNr = v.ToString();
                    customer = customers.FirstOrDefault(c => c.CustomerNr.ToLower() == customerNr.ToLower());
                }
            }
            else
            {
                customer = GetCustomerByNr(entities, actorCompanyId, customerNr, false, tryMatchingStringasNumber);
            }

            return customer;
        }

        private Customer GetCustomerByNr(CompEntities entities, int actorCompanyId, string customerNr, bool loadContact, bool tryMatchingStringasNumber)
        {
            IQueryable<Customer> query = entities.Customer;

            if (loadContact)
                query = query.Include("Actor.Contact");
            else
                query = query.Include("Actor");

            string intCustomerNr = null;
            if (tryMatchingStringasNumber && int.TryParse(customerNr.Trim(), out int v))
                intCustomerNr = v.ToString();

            return intCustomerNr == null ? query.FirstOrDefault(c => c.CustomerNr == customerNr && c.ActorCompanyId == actorCompanyId && c.State != (int)SoeEntityState.Deleted) :
                                            query.FirstOrDefault(c => (c.CustomerNr == customerNr || c.CustomerNr == intCustomerNr) && c.ActorCompanyId == actorCompanyId && c.State != (int)SoeEntityState.Deleted);
        }

        public Dictionary<int, string> GetCustomersByCompanyDict(int actorCompanyId, bool onlyActive, bool addEmptyRow, int? roleId = null, int? userId = null, bool onlyOneTime = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Customer.NoTracking();
            return GetCustomersByCompanyDict(entities, actorCompanyId, onlyActive, addEmptyRow, roleId, userId, onlyOneTime);
        }

        public Dictionary<int, string> GetCustomersByCompanyDict(CompEntities entities, int actorCompanyId, bool onlyActive, bool addEmptyRow, int? roleId = null, int? userId = null, bool onlyOneTime = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var customers = GetCustomersByCompanySmall(entities, actorCompanyId, onlyActive, roleId, userId, onlyOneTime: onlyOneTime);
            dict.AddRange(customers.ToDictionary(i => i.ActorCustomerId, i => i.CustomerNr + " " + i.CustomerName));

            return dict;
        }

        public List<CustomerSearchView> GetCustomersBySearch(CustomerSearchDTO search, int actorCompanyId, int maxNrRows = 200 * 20)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<CustomerSearchView> query = (from c in entitiesReadOnly.CustomerSearchView
                                                    where c.ActorCompanyId == actorCompanyId
                                                    select c);

            if (!string.IsNullOrEmpty(search.CustomerNr))
            {
                if (search.CustomerNr.StartsWith("*") && search.CustomerNr.EndsWith("*"))
                    query = query.Where(c => c.CustomerNr.Contains(search.CustomerNr.Replace("*", "")));
                else if (search.CustomerNr.StartsWith("*"))
                    query = query.Where(c => c.CustomerNr.EndsWith(search.CustomerNr.Replace("*", "")));
                else if (search.CustomerNr.EndsWith("*"))
                    query = query.Where(c => c.CustomerNr.StartsWith(search.CustomerNr.Replace("*", "")));
                else
                    query = query.Where(c => c.CustomerNr == search.CustomerNr);
            }

            if (!string.IsNullOrEmpty(search.Name))
            {
                if (search.Name.StartsWith("*") && search.Name.EndsWith("*"))
                    query = query.Where(c => c.Name.Contains(search.Name.Replace("*", "")));
                else if (search.Name.StartsWith("*"))
                    query = query.Where(c => c.Name.EndsWith(search.Name.Replace("*", "")));
                else if (search.Name.EndsWith("*"))
                    query = query.Where(c => c.Name.StartsWith(search.Name.Replace("*", "")));
                else
                    query = query.Where(c => c.Name == search.Name);
            }

            if (!string.IsNullOrEmpty(search.NameOrCustomerNrOrAddress))
            {
                if (search.NameOrCustomerNrOrAddress.StartsWith("*") && search.NameOrCustomerNrOrAddress.EndsWith("*"))
                    query = query.Where(c => c.Name.Contains(search.NameOrCustomerNrOrAddress.Replace("*", "")) || c.CustomerNr.Contains(search.NameOrCustomerNrOrAddress.Replace("*", "")) || ((c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery || c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing) && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address && c.AddressText.Contains(search.NameOrCustomerNrOrAddress.Replace("*", ""))));
                else if (search.NameOrCustomerNrOrAddress.StartsWith("*"))
                    query = query.Where(c => c.Name.EndsWith(search.NameOrCustomerNrOrAddress.Replace("*", "")) || c.CustomerNr.EndsWith(search.NameOrCustomerNrOrAddress.Replace("*", "")) || ((c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery || c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing) && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address && c.AddressText.EndsWith(search.NameOrCustomerNrOrAddress.Replace("*", ""))));
                else if (search.NameOrCustomerNrOrAddress.EndsWith("*"))
                    query = query.Where(c => c.Name.StartsWith(search.NameOrCustomerNrOrAddress.Replace("*", "")) || c.CustomerNr.StartsWith(search.NameOrCustomerNrOrAddress.Replace("*", "")) || ((c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery || c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing) && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address && c.AddressText.StartsWith(search.NameOrCustomerNrOrAddress.Replace("*", ""))));
                else
                    query = query.Where(c => c.Name == search.NameOrCustomerNrOrAddress || c.CustomerNr == search.NameOrCustomerNrOrAddress || ((c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery || c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing) && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address && c.AddressText == search.NameOrCustomerNrOrAddress));
            }

            if (!string.IsNullOrEmpty(search.Email))
            {
                query = query.Where(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);

                if (search.Email.StartsWith("*") && search.Email.EndsWith("*"))
                    query = query.Where(c => c.EcomText.Contains(search.Email.Replace("*", "")));
                else if (search.Email.StartsWith("*"))
                    query = query.Where(c => c.EcomText.EndsWith(search.Email.Replace("*", "")));
                else if (search.Email.EndsWith("*"))
                    query = query.Where(c => c.EcomText.StartsWith(search.Email.Replace("*", "")));
                else
                    query = query.Where(c => c.EcomText == search.Email);
            }

            if (!string.IsNullOrEmpty(search.PhoneNumber))
            {
                query = query.Where(c => ContactManager.PhoneEComTypes.Contains((TermGroup_SysContactEComType)c.SysContactEComTypeId));

                if (search.PhoneNumber.StartsWith("*") && search.PhoneNumber.EndsWith("*"))
                    query = query.Where(c => c.EcomText.Contains(search.PhoneNumber.Replace("*", "")));
                else if (search.PhoneNumber.StartsWith("*"))
                    query = query.Where(c => c.EcomText.EndsWith(search.PhoneNumber.Replace("*", "")));
                else if (search.PhoneNumber.EndsWith("*"))
                    query = query.Where(c => c.EcomText.StartsWith(search.PhoneNumber.Replace("*", "")));
                else
                    query = query.Where(c => c.EcomText == search.PhoneNumber);
            }

            if (!string.IsNullOrEmpty(search.DeliveryAddress))
            {
                query = query.Where(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
                if (search.DeliveryAddress.StartsWith("*") && search.DeliveryAddress.EndsWith("*"))
                    query = query.Where(c => c.AddressText.Contains(search.DeliveryAddress.Replace("*", "")));
                else if (search.DeliveryAddress.StartsWith("*"))
                    query = query.Where(c => c.AddressText.EndsWith(search.DeliveryAddress.Replace("*", "")));
                else if (search.DeliveryAddress.EndsWith("*"))
                    query = query.Where(c => c.AddressText.StartsWith(search.DeliveryAddress.Replace("*", "")));
                else
                    query = query.Where(c => c.AddressText == search.DeliveryAddress);
            }

            if (!string.IsNullOrEmpty(search.PostalCode))
            {
                query = query.Where(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
                if (search.PostalCode.StartsWith("*") && search.PostalCode.EndsWith("*"))
                    query = query.Where(c => c.AddressText.Contains(search.PostalCode.Replace("*", "")));
                else if (search.PostalCode.StartsWith("*"))
                    query = query.Where(c => c.AddressText.EndsWith(search.PostalCode.Replace("*", "")));
                else if (search.PostalCode.EndsWith("*"))
                    query = query.Where(c => c.AddressText.StartsWith(search.PostalCode.Replace("*", "")));
                else
                    query = query.Where(c => c.AddressText == search.PostalCode);
            }

            if (!string.IsNullOrEmpty(search.City))
            {
                query = query.Where(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery && c.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);
                if (search.City.StartsWith("*") && search.City.EndsWith("*"))
                    query = query.Where(c => c.AddressText.Contains(search.City.Replace("*", "")));
                else if (search.City.StartsWith("*"))
                    query = query.Where(c => c.AddressText.EndsWith(search.City.Replace("*", "")));
                else if (search.City.EndsWith("*"))
                    query = query.Where(c => c.AddressText.StartsWith(search.City.Replace("*", "")));
                else
                    query = query.Where(c => c.AddressText == search.City);
            }


            return query.Take(maxNrRows).ToList();
        }

        public List<CustomerSearchDTO> GetCustomersBySearch(CustomerSearchDTO inputDTO, int actorCompanyId, int? roleId = null, int? userId = null, int take = 200)
        {
            var outputDtos = new List<CustomerSearchDTO>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var customers = new List<Customer>();

            if (inputDTO.ActorCustomerId != 0)
            {
                Customer customer = (from c in entitiesReadOnly.Customer
                                        .Include("Actor.Contact")
                                     where c.ActorCustomerId == inputDTO.ActorCustomerId &&
                                     c.State != (int)SoeEntityState.Deleted
                                     select c).FirstOrDefault();

                if (customer != null)
                    customers.Add(customer);
            }
            else
            {
                customers = GetCustomersByCompany(actorCompanyId, true, roleId, userId, loadContact: true);
                customers = (from c in customers
                             where
                             (string.IsNullOrEmpty(inputDTO.Name) || (c.Name != null && c.Name.ToLower().Contains(inputDTO.Name.ToLower()))) &&
                             (string.IsNullOrEmpty(inputDTO.CustomerNr) || (c.CustomerNr != null && c.CustomerNr.ToLower().Contains(inputDTO.CustomerNr.ToLower()))) &&
                             (string.IsNullOrEmpty(inputDTO.Note) || (c.Note != null && c.Note.ToLower().Contains(inputDTO.Note.ToLower())))
                             select c).ToList();
            }

            var validEComTypes = new List<int>
                    {
                        (int)TermGroup_SysContactEComType.PhoneHome,
                        (int)TermGroup_SysContactEComType.PhoneJob,
                        (int)TermGroup_SysContactEComType.PhoneMobile
                    };

            foreach (Customer customer in customers)
            {
                #region Init

                if (customer.Actor == null || customer.Actor.Contact == null)
                    continue;

                CustomerSearchDTO outputDto = new CustomerSearchDTO()
                {
                    ActorCustomerId = customer.ActorCustomerId,
                    Name = customer.Name,
                    CustomerNr = customer.CustomerNr,
                    Note = customer.Note,
                    BillingAddress = string.Empty,
                    DeliveryAddress = string.Empty,
                    PhoneNumber = string.Empty,
                };

                #endregion

                #region Fetch data

                bool foundBillingAddress = false;
                bool foundDeliveryAddress = false;

                foreach (Contact contact in customer.Actor.Contact)
                {
                    #region Addresses

                    if (!contact.ContactAddress.IsLoaded)
                        contact.ContactAddress.Load();

                    foreach (ContactAddress contactAddress in contact.ContactAddress)
                    {
                        if (contactAddress.SysContactAddressTypeId != (int)TermGroup_SysContactAddressType.Billing && contactAddress.SysContactAddressTypeId != (int)TermGroup_SysContactAddressType.Delivery)
                            continue;

                        if (!contactAddress.ContactAddressRow.IsLoaded)
                            contactAddress.ContactAddressRow.Load();

                        if (contactAddress.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing && contactAddress.ContactAddressRow.Count > 0)
                        {
                            outputDto.BillingAddress += FormatAddress(contactAddress, "{0} {1} {2} {3} ", TermGroup_SysContactAddressRowType.Name, TermGroup_SysContactAddressRowType.Address, TermGroup_SysContactAddressRowType.PostalCode, TermGroup_SysContactAddressRowType.PostalAddress);
                            if (AddressContains(contactAddress, inputDTO.BillingAddress, TermGroup_SysContactAddressRowType.Name, TermGroup_SysContactAddressRowType.Address, TermGroup_SysContactAddressRowType.PostalAddress, TermGroup_SysContactAddressRowType.PostalCode))
                                foundBillingAddress = true;
                        }
                        else if (contactAddress.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery && contactAddress.ContactAddressRow.Count > 0)
                        {
                            outputDto.DeliveryAddress += FormatAddress(contactAddress, "{0} {1} {2} {3} ", TermGroup_SysContactAddressRowType.Name, TermGroup_SysContactAddressRowType.Address, TermGroup_SysContactAddressRowType.PostalCode, TermGroup_SysContactAddressRowType.PostalAddress);
                            if (AddressContains(contactAddress, inputDTO.DeliveryAddress, TermGroup_SysContactAddressRowType.Name, TermGroup_SysContactAddressRowType.Address, TermGroup_SysContactAddressRowType.PostalAddress, TermGroup_SysContactAddressRowType.PostalCode))
                                foundDeliveryAddress = true;
                        }
                    }

                    #endregion

                    #region ECom

                    var contactEcomTexts = entitiesReadOnly.ContactECom.Where(e => e.Contact.ContactId == contact.ContactId && validEComTypes.Contains(e.SysContactEComTypeId)).Select(x => x.Text).ToList();
                    foreach (var text in contactEcomTexts)
                    {
                        if (string.IsNullOrEmpty(text?.Trim() ?? ""))
                            continue;

                        outputDto.PhoneNumber = text;
                        break; //Take first
                    }

                    #endregion
                }

                #endregion

                #region Validation

                bool addCustomer = true;
                if (!foundBillingAddress && !String.IsNullOrEmpty(inputDTO.BillingAddress))
                    addCustomer = false;
                if (!foundDeliveryAddress && !String.IsNullOrEmpty(inputDTO.DeliveryAddress))
                    addCustomer = false;

                if (addCustomer)
                {
                    outputDtos.Add(outputDto);
                    if (outputDtos.Count >= take)
                        break;
                }

                #endregion
            }

            return outputDtos;
        }

        #region Customer Small

        public CustomerSmallDTO GetCustomerSmall(CompEntities entities, int actorCustomerId)
        {
            if (actorCustomerId == 0)
                return null;

            return (from c in entities.Customer
                    where c.ActorCustomerId == actorCustomerId &&
                    c.ActorCompanyId == ActorCompanyId &&
                    c.State != (int)SoeEntityState.Deleted
                    select new CustomerSmallDTO
                    {
                        ActorCustomerId = c.ActorCustomerId,
                        CustomerName = c.Name,
                        CustomerNr = c.CustomerNr
                    }).FirstOrDefault();
        }

        public List<CustomerSmallDTO> GetCustomersByCompanySmall(int actorCompanyId, bool onlyActive, int? roleId = null, int? userId = null, bool onlyOneTime = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCustomersByCompanySmall(entities, actorCompanyId, onlyActive, roleId, userId, onlyOneTime);
        }
        public List<CustomerSmallDTO> GetCustomersByCompanySmall(CompEntities entities, int actorCompanyId, bool onlyActive, int? roleId = null, int? userId = null, bool onlyOneTime = false)
        {
            IQueryable<Customer> query = entities.Customer;

            if (onlyActive)
                query = query.Where(c => c.ActorCompanyId == actorCompanyId && c.State == (int)SoeEntityState.Active);
            else
                query = query.Where(c => c.ActorCompanyId == actorCompanyId && c.State != (int)SoeEntityState.Deleted);

            if (onlyOneTime)
                query = query.Where(c => c.IsOneTimeCustomer);

            var customersSmall = query.Select(c => new CustomerSmallDTO
            {
                ActorCustomerId = c.ActorCustomerId,
                CustomerName = c.Name,
                CustomerNr = c.CustomerNr
            }).ToList();

            if (roleId.HasValue && userId.HasValue && roleId.Value > 0 && userId.Value > 0)
                customersSmall = FilterCustomersUsers(entities, customersSmall, actorCompanyId, roleId.Value, userId.Value);

            // Remove new lines in name
            foreach (var customer in customersSmall)
            {
                customer.CustomerName = Regex.Replace(customer.CustomerName, @"\t|\n|\r", " ");
            }

            return customersSmall;
        }

        public static string GenerateCustomerNumber(List<CustomerSmallDTO> customers)
        {
            Regex regex = new Regex("^[0-9]+$");
            var lastNumber = customers.Where(x => regex.IsMatch(x.CustomerNr)).Select(c => long.Parse(c.CustomerNr)).OrderBy(n => n).LastOrDefault();
            return (lastNumber + 1).ToString();
        }

        #endregion

        public Customer GetCustomer(int actorCustomerId, bool loadActor = false, bool loadAccount = false, bool loadCustomerUser = false, bool loadContactAddresses = false, bool loadCategories = false, bool loadProduct = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Customer.NoTracking();
            return GetCustomer(entities, actorCustomerId, loadActor, loadAccount, loadCustomerUser, loadContactAddresses, loadCategories, loadProduct);
        }

        public Customer GetCustomer(CompEntities entities, int actorCustomerId, bool loadActor = false, bool loadAccount = false, bool loadCustomerUser = false, bool loadContactAddresses = false, bool loadCategories = false, bool loadProduct = false)
        {
            IQueryable<Customer> query = (from s in entities.Customer
                                          where s.ActorCustomerId == actorCustomerId &&
                                          s.ActorCompanyId == ActorCompanyId &&
                                          s.State != (int)SoeEntityState.Deleted
                                          select s);

            query = AddCustomerIncludes(loadActor, loadAccount, loadCustomerUser, loadContactAddresses, false, true, loadProduct, query);

            Customer customer = query.FirstOrDefault();

            if (loadCategories)
            {
                customer.CategoryIds = (from c in entities.CompanyCategoryRecord
                                        where c.RecordId == customer.ActorCustomerId &&
                                        c.Entity == (int)SoeCategoryRecordEntity.Customer &&
                                        c.Category.Type == (int)SoeCategoryType.Customer &&
                                        c.Category.ActorCompanyId == ActorCompanyId &&
                                        c.Category.State == (int)SoeEntityState.Active
                                        select c.CategoryId).ToList();
            }

            if (customer != null && customer.SysCountryId.HasValue)
            {
                var countries = CountryCurrencyManager.GetEUSysCountrieIds(DateTime.Today);
                customer.IsEUCountryBased = countries.Contains(customer.SysCountryId.Value);
            }

            return customer;
        }

        public int GetCustomerIdByInvoiceNr(CompEntities entities, string invoiceNr, SoeOriginType originType, int actorCompanyId)
        {
            if (string.IsNullOrEmpty(invoiceNr))
                return 0;

            var result = (from i in entities.Invoice.OfType<CustomerInvoice>()
                          where i.InvoiceNr == invoiceNr &&
                          i.Origin.ActorCompanyId == actorCompanyId &&
                          i.Origin.Type == (int)originType
                          select i.Actor.Customer.ActorCustomerId).FirstOrDefault();

            return result;
        }

        public string GetNextCustomerNr(int actorCompanyId)
        {
            int lastNr = 0;
            List<Customer> customers = GetCustomersByCompany(actorCompanyId, onlyActive: true);
            if (customers.Any())
            {
                Int32.TryParse(customers.Last().CustomerNr, out lastNr);
                // If unable to parse, numeric values are not used
                if (lastNr == 0)
                    return String.Empty;
            }

            lastNr++;

            // Check that number is not used
            if (customers.Any(c => c.CustomerNr == lastNr.ToString()))
                return String.Empty;

            return lastNr.ToString();
        }

        public Dictionary<int, string> GetCustomerGlnNumbers(int actorCustomerId, bool addEmptyRow = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ContactECom.NoTracking();
            return GetCustomerGlnNumbers(entities, actorCustomerId, addEmptyRow);
        }

        public Dictionary<int, string> GetCustomerGlnNumbers(CompEntities entities, int actorCustomerId, bool addEmptyRow = false)
        {
            var dict = new Dictionary<int, string>();
            if (addEmptyRow) dict.Add(0, " ");

            try
            {
                List<ContactECom> ecoms = ContactManager.GetContactEComsFromActor(entities, actorCustomerId, false);
                ecoms.Where(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.GlnNumber).ToList().ForEach(i => dict.Add(i.ContactEComId, $"{i.Name} ( {i.Text} )"));
            }
            catch (Exception ex)
            {
                LogError(ex, log);
            }

            return dict;
        }

        /// <summary>
        /// Returns a dictionary of the email addresses of the customer
        /// </summary>
        /// <param name="actorCustomerId"></param>
        /// <returns></returns>
        public Dictionary<int, string> GetCustomerEmailAddresses(int actorCustomerId, bool loadContactPersonsEmails = false, bool addEmptyRow = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ContactECom.NoTracking();
            return GetCustomerEmailAddresses(entities, actorCustomerId, loadContactPersonsEmails, addEmptyRow);
        }

        /// <summary>
        /// Returns a dictionary of the email addresses of the customer
        /// </summary>
        /// <param name="actorCustomerId"></param>
        /// <returns></returns>
        public Dictionary<int, string> GetCustomerEmailAddresses(CompEntities entities, int actorCustomerId, bool loadContactPersonsEmails = false, bool addEmptyRow = false)
        {
            var dict = new Dictionary<int, string>();
            if (addEmptyRow) dict.Add(0, " ");

            try
            {

                List<ContactECom> ecoms = ContactManager.GetContactEComsFromActor(entities, actorCustomerId, false);
                ecoms.Where(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email).ToList().ForEach(i => dict.Add(i.ContactEComId, i.Text));

                if (loadContactPersonsEmails)
                {
                    List<ContactPerson> contactPersons = ContactManager.GetContactPersons(entities, actorCustomerId);
                    foreach (ContactPerson contactPerson in contactPersons)
                    {
                        List<ContactAddressItem> Rows = ContactManager.GetContactAddressItems(contactPerson.ActorContactPersonId);
                        foreach (var row in Rows)
                        {
                            if (row.ContactAddressItemType == ContactAddressItemType.EComEmail && !dict.ContainsKey(row.ContactEComId))
                            {
                                dict.Add(row.ContactEComId, row.EComText);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
            }

            return dict;
        }

        public List<ContactAddress> GetCustomerDeliveryAddresses(int customerId, bool addEmptyRow)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ContactAddress.NoTracking();
            return GetCustomerDeliveryAddresses(entities, customerId, addEmptyRow);
        }

        public List<ContactAddress> GetCustomerDeliveryAddresses(CompEntities entities, int customerId, bool addEmptyRow)
        {
            int contactId = ContactManager.GetContactIdFromActorId(entities, customerId);
            return ContactManager.GetContactAddresses(entities, contactId, TermGroup_SysContactAddressType.Delivery, addEmptyRow);
        }

        public List<ContactAddress> GetCustomerBillingAddresses(int customerId, bool addEmptyRow)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ContactAddress.NoTracking();
            return GetCustomerBillingAddresses(entities, customerId, addEmptyRow);
        }

        public List<ContactAddress> GetCustomerBillingAddresses(CompEntities entities, int customerId, bool addEmptyRow)
        {
            int contactId = ContactManager.GetContactIdFromActorId(entities, customerId);
            return ContactManager.GetContactAddresses(entities, contactId, TermGroup_SysContactAddressType.Billing, addEmptyRow);
        }

        public int GetCustomerInvoiceReportTemplate(int actorCustomerId, OrderInvoiceRegistrationType type)
        {
            // Get customer
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Customer.NoTracking();
            Customer customer = (from c in entities.Customer
                                 where c.ActorCustomerId == actorCustomerId
                                 select c).FirstOrDefault<Customer>();

            if (customer != null)
            {
                switch (type)
                {
                    case OrderInvoiceRegistrationType.Offer:
                        if (customer.OfferTemplate.HasValue)
                            return customer.OfferTemplate.Value;
                        break;
                    case OrderInvoiceRegistrationType.Order:
                        if (customer.OrderTemplate.HasValue)
                            return customer.OrderTemplate.Value;
                        break;
                    case OrderInvoiceRegistrationType.Invoice:
                        if (customer.BillingTemplate.HasValue)
                            return customer.BillingTemplate.Value;
                        break;
                    case OrderInvoiceRegistrationType.Contract:
                        if (customer.AgreementTemplate.HasValue)
                            return customer.AgreementTemplate.Value;
                        break;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get specified customers default price list type.
        /// If no type exist, det default company setting.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="customerId">Customer ID</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns></returns>
        public int GetCustomerPriceListTypeId(CompEntities entities, int customerId, int actorCompanyId)
        {
            int? priceListTypeId = null;

            if (customerId > 0)
            {
                priceListTypeId = (from c in entities.Customer
                                   where c.ActorCustomerId == customerId
                                   select c.PriceListTypeId).FirstOrDefault();
            }

            if (!priceListTypeId.HasValue)
                priceListTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultPriceListType, 0, actorCompanyId, 0);

            return priceListTypeId.Value;
        }

        public bool CustomerExist(CompEntities entities, Customer customer, int actorCompanyId)
        {
            if (customer == null)
                return false;

            return CustomerExist(entities, customer.CustomerNr, actorCompanyId);
        }

        public bool CustomerExist(CompEntities entities, string customerNr, int actorCompanyId)
        {
            return (from c in entities.Customer
                    where c.CustomerNr == customerNr &&
                    c.State != (int)SoeEntityState.Deleted &&
                    c.ActorCompanyId == actorCompanyId
                    select c).Any();
        }

        public bool HasCustomerOrigins(SoeOriginType originType, int actorCustomerId, int actorCompanyId)
        {
            return InvoiceManager.InvoicesExistForActor(originType, actorCustomerId, actorCompanyId);
        }

        public bool IsCustomerBlocked(CompEntities entities, int? actorCustomerId, Actor actor = null)
        {
            Customer customer = null;

            if (actor != null && actor.Customer != null)
            {
                customer = actor.Customer;
            }
            else if (actorCustomerId.HasValue)
            {
                customer = (from entry in entities.Customer
                            where entry.ActorCustomerId == actorCustomerId.Value &&
                            entry.State == (int)SoeEntityState.Active
                            select entry).FirstOrDefault();
            }

            return customer?.BlockInvoice ?? false;
        }

        public int GetDefaultCashCustomerId(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Customer.NoTracking();
            return GetDefaultCashCustomerId(entities, actorCompanyId);
        }

        public int GetDefaultCashCustomerId(CompEntities entities, int actorCompanyId)
        {
            Customer customer = (from c in entities.Customer
                                 where c.IsCashCustomer.HasValue && c.IsCashCustomer.Value &&
                                 c.State == (int)SoeEntityState.Active &&
                                 c.ActorCompanyId == actorCompanyId
                                 select c).FirstOrDefault();

            return customer != null ? customer.ActorCustomerId : 0;
        }

        public List<HouseholdTaxDeductionApplicant> GetHouseholdTaxDeductionApplicants(int customerId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.HouseholdTaxDeductionApplicant.NoTracking();
            return GetHouseholdTaxDeductionApplicants(entities, customerId);
        }

        public List<HouseholdTaxDeductionApplicant> GetHouseholdTaxDeductionApplicants(CompEntities entities, int customerId)
        {
            return (from h in entities.HouseholdTaxDeductionApplicant
                    where h.State == (int)SoeEntityState.Active &&
                       h.ActorCustomerId == customerId
                    select h).ToList();
        }

        public string GetAutomasterCashCustomerNr(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Customer.NoTracking();
            return GetAutomasterCashCustomerNr(entities, actorCompanyId);
        }

        public string GetAutomasterCashCustomerNr(CompEntities entities, int actorCompanyId)
        {
            string customerNr = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceCashCustomerNumber, 0, actorCompanyId, 0);

            Customer customer = null;

            if (!string.IsNullOrEmpty(customerNr))
            {
                customer = (from c in entities.Customer
                            where c.CustomerNr == customerNr &&
                            c.State == (int)SoeEntityState.Active &&
                            c.ActorCompanyId == actorCompanyId
                            select c).FirstOrDefault();
            }

            if (customer == null)
            {
                customer = (from c in entities.Customer
                            where c.IsCashCustomer.HasValue && c.IsCashCustomer.Value &&
                            c.State == (int)SoeEntityState.Active &&
                            c.ActorCompanyId == actorCompanyId
                            select c).FirstOrDefault();
            }

            //If cash customer is not found, use 56 as customer number (agreed with Automaster)
            return customer != null ? customer.CustomerNr : "56";

        }

        /// <summary>
        /// Checks if the customer credit-limit is reached. Uses customer credit limit prior company setting credit limit.
        /// Will return a decimal nr with the customers current amount of unpayed invoices or return null if the credit
        /// limit is not reached.
        /// </summary>
        /// <param name="actorCompanyId">Must be specified.</param>
        /// <param name="customerId">Must be specified.</param>
        /// <param name="creditLimit">The customers credit limit. If not specified then it will be fetched from the db instead.</param>
        /// <returns>A decimal nr with the customers current amount of unpayed invoices or return null if the credit limit is not reached. </returns>
        public decimal? CheckCustomerCreditLimit(int actorCompanyId, int customerId, int? creditLimit = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (creditLimit == null || creditLimit == 0)
            {
                // Load the customers credit limit
                creditLimit = (from customer in entitiesReadOnly.Customer
                               where customer.CreditLimit.HasValue &&
                               customer.ActorCustomerId == customerId
                               select customer.CreditLimit).FirstOrDefault();

                if (creditLimit == null || creditLimit == 0)
                {
                    // Load company setting credit limit
                    int companyCreditLimit = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerDefaultCreditLimit, 0, actorCompanyId, 0);
                    if (companyCreditLimit == 0)
                        return null;
                    else
                        creditLimit = companyCreditLimit;
                }
            }

            decimal? amount = 0;

            // Preliminära fakturor
            var query = (from invoice in entitiesReadOnly.Invoice.OfType<CustomerInvoice>()
                         .Include("Origin")
                         where
                         invoice.ActorId.Value == customerId &&
                         invoice.Origin.ActorCompanyId == actorCompanyId &&
                         invoice.State == (int)SoeEntityState.Active &&
                         (
                             (
                                 invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice
                                 &&
                                 !invoice.FullyPayed
                                 &&
                                 (invoice.Origin.Status == (int)SoeOriginStatus.Origin || invoice.Origin.Status == (int)SoeOriginStatus.Draft || invoice.Origin.Status == (int)SoeOriginStatus.Voucher)
                             )
                             ||
                             (
                                 invoice.Origin.Type == (int)SoeOriginType.Order
                                 &&
                                 (invoice.Origin.Status == (int)SoeOriginStatus.OrderPartlyInvoice || invoice.Origin.Status == (int)SoeOriginStatus.Origin)
                                 &&
                                 (invoice.RemainingAmount > 0)
                             )
                         )
                         select invoice).ToList();

            foreach (var item in query)
            {
                amount += (item.Origin.Type == (int)SoeOriginType.Order) ? item.RemainingAmount ?? 0 : item.TotalAmount - item.PaidAmount;
            }

            if (creditLimit.HasValue)
            {
                var exceedsLimit = creditLimit.Value <= amount;

                // Return null if the credit limit is not reached.
                //if (!exceedsLimit)
                //amount = null;
            }

            return amount;
        }

        public ActionResult AddCustomer(CompEntities entities, Customer customer, int? deliveryTypeId, int? deliveryConditionId, int? paymentConditionId, int? priceListTypeId, int currencyId, int actorCompanyId)
        {
            if (customer == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Customer");

            if (CustomerExist(entities, customer, actorCompanyId))
                return new ActionResult((int)ActionResultSave.CustomerExists);

            SetCreatedProperties(customer);

            //Set FK
            customer.DeliveryTypeId = deliveryTypeId.ToNullable();
            customer.DeliveryConditionId = deliveryConditionId.ToNullable();
            customer.PaymentConditionId = paymentConditionId.ToNullable();
            customer.PriceListTypeId = priceListTypeId.ToNullable();
            customer.CurrencyId = currencyId;

            Actor actor = new Actor()
            {
                ActorType = (int)SoeActorType.Customer,

                //Set reference
                Customer = customer,
            };

            ActionResult result = AddEntityItem(entities, actor, "Actor");
            if (result.Success)
                result.IntegerValue = customer.ActorCustomerId;
            //if (result.Success == false)
            //    return result;

            //result = MapActorToCustomer(entities, customer, actorCompanyId);

            return result;
        }

        public ActionResult UpdateCustomer(CompEntities entities, Customer customer, int? deliveryTypeId, int? deliveryConditionId, int? paymentConditionId, int? priceListTypeId, int currencyId)
        {
            if (customer == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Customer");

            Customer originalCustomer = GetCustomer(entities, customer.ActorCustomerId);
            if (originalCustomer == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Customer");

            //Set FK
            originalCustomer.DeliveryTypeId = deliveryTypeId.ToNullable();
            originalCustomer.DeliveryConditionId = deliveryConditionId.ToNullable();
            originalCustomer.PaymentConditionId = paymentConditionId.ToNullable();
            originalCustomer.PriceListTypeId = priceListTypeId.ToNullable();
            originalCustomer.CurrencyId = currencyId;

            return UpdateEntityItem(entities, originalCustomer, customer, "Customer");
        }

        public ActionResult UnMapActorFromCustomer(CompEntities entities, Customer customer, int actorId)
        {
            if (customer == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "Customer");

            Actor actor = ActorManager.GetActor(entities, actorId, true);
            if (actor == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");

            if (!customer.Actors.IsLoaded)
                customer.Actors.Load();

            customer.Actors.Remove(actor);

            return SaveEntityItem(entities, customer);
        }

        public ActionResult SaveMobileCustomer(int customerId, string customerNr, string name, string orgNr, string reference, string note, string vatNr, int vatTypeId, int paymentConditionId, int salesPriceListid, int stdWholeSellerId, int currencyId, decimal disccountArticles, decimal disccountServices, int emailId, string email, int phoneHomeId, string phoneHome, int phoneJobId, string phoneJob, int phoneMobileId, string phoneMobile, int faxId, string faxNr, int invoiceAddressId, string invoiceAddress, string iaPostalCode, string iaPostalAddress, string iaCountry, string iaAddressCO, int deliveryAddress1Id, string deliveryAddress1, string da1PostalCode, string da1PostalAddress, string da1Country, string da1AddressCO, string da1Name, int invoiceDeliveryTypeId, int roleId, int actorCompanyId, bool updateInvoiceDeliveryType = false)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            #region FieldSettings

            bool showAllFields = false;

            //Get settings
            List<FieldSetting> fieldSettings = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.CustomerEdit, roleId, actorCompanyId);
            var hasEditPermission = FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit, Permission.Readonly, roleId, actorCompanyId);
            if (!hasEditPermission)
            {
                return new ActionResult((int)ActionResultSave.InsufficienPermissionToSave);
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        CompCurrency baseCurrency = CountryCurrencyManager.GetCompanyBaseCurrency(entities, actorCompanyId);

                        #endregion

                        #region Customer

                        // Get existing customer
                        Customer customer = null;
                        if (customerId > 0)
                            customer = GetCustomer(entities, customerId, loadAccount: true);

                        // Check if customer number already exists
                        if (customer == null || customer.CustomerNr != customerNr)
                        {
                            if (CustomerExist(entities, customerNr, actorCompanyId))
                                return new ActionResult((int)ActionResultSave.CustomerExists);
                        }

                        if (customer == null)
                        {
                            #region Add

                            #region Customer

                            customer = new Customer()
                            {
                                CustomerNr = customerNr,
                                Name = name,
                                OrgNr = orgNr,
                                VatNr = vatNr,
                                VatType = vatTypeId,
                                InvoiceReference = reference,
                                DiscountMerchandise = disccountArticles,
                                DiscountService = disccountServices,
                                SysWholeSellerId = stdWholeSellerId,
                                Note = note,
                                ActorCompanyId = actorCompanyId,
                                AddSupplierInvoicesToEInvoices = false,
                            };
                            SetCreatedProperties(customer);
                            entities.Customer.AddObject(customer);

                            //Set FK
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_Currency, true, showAllFields))
                                customer.CurrencyId = currencyId;
                            else
                                customer.CurrencyId = baseCurrency.CurrencyId;
                            customer.PaymentConditionId = paymentConditionId.ToNullable();
                            customer.PriceListTypeId = salesPriceListid.ToNullable();

                            if (updateInvoiceDeliveryType && FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_InvoiceDeliveryType, true, showAllFields))
                                customer.InvoiceDeliveryType = invoiceDeliveryTypeId;

                            #endregion

                            #region Actor

                            Actor actor = new Actor()
                            {
                                ActorType = (int)SoeActorType.Customer,

                                //Set references
                                Customer = customer,
                            };
                            entities.Actor.AddObject(actor);

                            result = SaveChanges(entities, transaction);

                            if (result.Success)
                            {
                                customerId = customer.ActorCustomerId;
                            }
                            else
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }

                            #region OLD
                            /*if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }

                            result = MapActorToCustomer(entities, customer, actorCompanyId);
                            if (result.Success)
                            {
                                customerId = customer.ActorCustomerId;
                            }
                            else
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }*/
                            #endregion

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            #region Customer

                            SetModifiedProperties(customer);

                            customer.CustomerNr = customerNr;
                            customer.Name = name;
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_OrganisationNr, true, showAllFields))
                                customer.OrgNr = orgNr;
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_VatNr, true, showAllFields))
                                customer.VatNr = vatNr;
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_VatType, true, showAllFields))
                                customer.VatType = vatTypeId;
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_Reference, true, showAllFields))
                                customer.InvoiceReference = reference;
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DiscountArticles, true, showAllFields))
                                customer.DiscountMerchandise = disccountArticles;
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DiscountServices, true, showAllFields))
                                customer.DiscountService = disccountServices;
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_StandardWholeSeller, true, showAllFields))
                                customer.SysWholeSellerId = stdWholeSellerId;
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_Note, true, showAllFields) || customer.ShowNote)
                                customer.Note = note;
                            if (updateInvoiceDeliveryType && FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_InvoiceDeliveryType, true, showAllFields))
                                customer.InvoiceDeliveryType = invoiceDeliveryTypeId;

                            //Set 
                            if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_Currency, true, showAllFields))
                                customer.CurrencyId = currencyId;
                            else
                                customer.CurrencyId = baseCurrency.CurrencyId;
                            customer.PaymentConditionId = paymentConditionId.ToNullable();
                            customer.PriceListTypeId = salesPriceListid.ToNullable();

                            result = SaveChanges(entities, transaction);

                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotUpdated;
                                return result;
                            }

                            #endregion

                            #endregion
                        }

                        #region Contact

                        Contact contact = ContactManager.GetContactFromActor(entities, customerId);
                        if (contact == null)
                        {
                            #region Add

                            // Get actor
                            Actor actor = ActorManager.GetActor(entities, customerId, false);
                            if (actor == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");

                            // Create new Contact
                            contact = new Contact()
                            {
                                Actor = actor,
                                SysContactTypeId = (int)TermGroup_SysContactType.Company
                            };
                            SetCreatedProperties(contact);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(contact);

                            #endregion
                        }

                        #endregion

                        #endregion

                        #region Email

                        if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_EmailAddress, true, showAllFields))
                        {
                            if (emailId <= 0)
                                result = ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.Email, email, transaction);
                            else
                                result = ContactManager.UpdateContactECom(entities, emailId, email, transaction);

                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }
                        }

                        #endregion

                        #region PhoneHome

                        if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_PhoneHome, true, showAllFields))
                        {
                            if (phoneHomeId <= 0)
                                result = ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.PhoneHome, phoneHome, transaction);
                            else
                                result = ContactManager.UpdateContactECom(entities, phoneHomeId, phoneHome, transaction);

                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }
                        }

                        #endregion

                        #region PhoneJob

                        if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_PhoneJob, true, showAllFields))
                        {
                            if (phoneJobId <= 0)
                                result = ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.PhoneJob, phoneJob, transaction);
                            else
                                result = ContactManager.UpdateContactECom(entities, phoneJobId, phoneJob, transaction);

                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }
                        }

                        #endregion

                        #region PhoneMobile

                        if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_PhoneMobile, true, showAllFields))
                        {
                            if (phoneMobileId <= 0)
                                result = ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.PhoneMobile, phoneMobile, transaction);
                            else
                                result = ContactManager.UpdateContactECom(entities, phoneMobileId, phoneMobile, transaction);

                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }
                        }

                        #endregion

                        #region Fax

                        if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_Fax, true, showAllFields))
                        {
                            if (faxId <= 0)
                                result = ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.Fax, faxNr, transaction);
                            else
                                result = ContactManager.UpdateContactECom(entities, faxId, faxNr, transaction);

                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }
                        }

                        #endregion

                        #region Addresses

                        var addresses = ContactManager.GetContactAddresses(entities, contact.ContactId);

                        #region InvoiceAddress

                        if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_InvoiceAddress, true, showAllFields))
                        {
                            if (invoiceAddressId <= 0)
                            {
                                #region Add

                                #region Add ContactAddress

                                //Billing
                                var contactAddress = new ContactAddress()
                                {
                                    SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Billing,
                                    Name = GetText((int)TermGroup_SysContactAddressType.Billing, (int)TermGroup.SysContactAddressType),

                                    //Set references
                                    Contact = contact,
                                };
                                SetCreatedProperties(contactAddress);
                                entities.ContactAddress.AddObject(contactAddress);

                                #endregion

                                #region Add ContactAddressRow

                                //Address
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_InvoiceAddress, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.Address, invoiceAddress, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                //PostalCode
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_IAPostalCode, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalCode, iaPostalCode, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                //PostalAddress
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_IAPostalAddress, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalAddress, iaPostalAddress, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                //Country
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_IACountry, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.Country, iaCountry, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                //Address C/O
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_IAAddressCO, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.AddressCO, iaAddressCO, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                #endregion

                                #endregion
                            }
                            else
                            {
                                #region Update

                                var contactAddress = addresses.FirstOrDefault(i => i.ContactAddressId == invoiceAddressId);
                                if (contactAddress != null)
                                {
                                    #region Update ContactAddress

                                    SetModifiedProperties(contactAddress);

                                    #endregion

                                    #region Update ContactAddressRow

                                    //Address
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_InvoiceAddress, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.Address, contactAddress, invoiceAddress, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    //PostalCode
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_IAPostalCode, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalCode, contactAddress, iaPostalCode, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    //PostalAddress
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_IAPostalAddress, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalAddress, contactAddress, iaPostalAddress, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    //Country
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_IACountry, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.Country, contactAddress, iaCountry, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    //Address C/O
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_IAAddressCO, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.AddressCO, contactAddress, iaAddressCO, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    #endregion
                                }

                                #endregion
                            }
                        }

                        #endregion

                        #region DeliveryAddress

                        if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DeliveryAddress1, true, showAllFields))
                        {
                            if (deliveryAddress1Id <= 0)
                            {
                                #region Add

                                #region Add ContactAddress

                                //Delivery
                                var contactAddress = new ContactAddress()
                                {
                                    SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Delivery,
                                    Name = GetText((int)TermGroup_SysContactAddressType.Delivery, (int)TermGroup.SysContactAddressType),

                                    //Set references
                                    Contact = contact,
                                };
                                SetCreatedProperties(contactAddress);
                                entities.ContactAddress.AddObject(contactAddress);

                                #endregion

                                #region Add ContactAddressRow

                                //Address
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DeliveryAddress1, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.Address, deliveryAddress1, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                //PostalCode
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1PostalCode, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalCode, da1PostalCode, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                //PostalAddress
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1PostalCode, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalAddress, da1PostalAddress, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                //Country
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1Country, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.Country, da1Country, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                //Address C/O
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1AddressCO, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.AddressCO, da1AddressCO, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                //Name
                                if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1Name, true, showAllFields))
                                {
                                    result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.Name, da1Name, contactAddress, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                        return result;
                                    }
                                }

                                #endregion

                                #endregion
                            }
                            else
                            {
                                #region Update

                                var contactAddress = addresses.FirstOrDefault(i => i.ContactAddressId == deliveryAddress1Id);
                                if (contactAddress != null)
                                {
                                    #region Update ContactAddress

                                    SetModifiedProperties(contactAddress);

                                    #endregion

                                    #region Update ContactAddressRow

                                    //Address
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DeliveryAddress1, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.Address, contactAddress, deliveryAddress1, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    //PostalCode
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1PostalCode, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalCode, contactAddress, da1PostalCode, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    //PostalAddress
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1PostalAddress, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalAddress, contactAddress, da1PostalAddress, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    //Country
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1Country, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.Country, contactAddress, da1Country, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    //Address C/O
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1AddressCO, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.AddressCO, contactAddress, da1AddressCO, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    //Name
                                    if (FieldSettingManager.DoShowMobileField(fieldSettings, TermGroup_MobileFields.CustomerEdit_DA1Name, true, showAllFields))
                                    {
                                        result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.Name, contactAddress, da1Name, transaction);
                                        if (!result.Success)
                                        {
                                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                            return result;
                                        }
                                    }

                                    #endregion
                                }

                                #endregion
                            }
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        if (!result.Success)
                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = customerId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SaveCustomer(CustomerDTO customerInput, List<CompanyCategoryRecordDTO> categoryRecords, List<int> contactPersonIds, List<HouseholdTaxDeductionApplicantDTO> householdTaxApplicantInputs, int actorCompanyId, List<ExtraFieldRecordDTO> extraFields = null)
        {
            if (customerInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Customer");

            // Default result is successful
            ActionResult result = new ActionResult();

            int customerId = customerInput.ActorCustomerId;
            // Customer cannot contain "," due to crystal
            if (!string.IsNullOrEmpty(customerInput.Name) && customerInput.Name.Contains(','))
                customerInput.Name = customerInput.Name.Replace(',', ' ');

            using (CompEntities entities = new CompEntities())
            {
                Customer customer = null;
                try
                {
                    entities.Connection.Open();
                        
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Customer

                        customer = GetCustomer(entities, customerId, true, true);

                        if (customer == null || customer.CustomerNr != customerInput.CustomerNr)
                        {
                            if (CustomerExist(entities, customerInput.CustomerNr, actorCompanyId))
                                return new ActionResult((int)ActionResultSave.CustomerExists);
                        }

                        if (customer == null)
                        {
                            #region Add

                            #region Customer

                            customer = new Customer()
                            {
                                VatType = (int)customerInput.VatType,
                                DeliveryConditionId = customerInput.DeliveryConditionId.ToNullable(),
                                DeliveryTypeId = customerInput.DeliveryTypeId.ToNullable(),
                                PaymentConditionId = customerInput.PaymentConditionId.ToNullable(),
                                PriceListTypeId = customerInput.PriceListTypeId.ToNullable(),
                                CurrencyId = customerInput.CurrencyId,
                                SysCountryId = customerInput.SysCountryId,
                                SysLanguageId = customerInput.SysLanguageId,
                                SysWholeSellerId = customerInput.SysWholeSellerId.ToNullable(),
                                CustomerNr = customerInput.CustomerNr?.Trim(),
                                Name = customerInput.Name?.Trim(),
                                OrgNr = customerInput.OrgNr?.Trim(),
                                VatNr = customerInput.VatNr?.Trim(),
                                InvoiceReference = customerInput.InvoiceReference,
                                GracePeriodDays = customerInput.GracePeriodDays,
                                PaymentMorale = customerInput.PaymentMorale,
                                SupplierNr = customerInput.SupplierNr,
                                OfferTemplate = customerInput.OfferTemplate,
                                OrderTemplate = customerInput.OrderTemplate,
                                BillingTemplate = customerInput.BillingTemplate,
                                AgreementTemplate = customerInput.AgreementTemplate,
                                ManualAccounting = customerInput.ManualAccounting,
                                DiscountMerchandise = customerInput.DiscountMerchandise,
                                Discount2Merchandise = customerInput.Discount2Merchandise,
                                DiscountService = customerInput.DiscountService,
                                Discount2Service = customerInput.Discount2Service,
                                DisableInvoiceFee = customerInput.DisableInvoiceFee,
                                Note = customerInput.Note,
                                ShowNote = customerInput.ShowNote,
                                FinvoiceAddress = customerInput.FinvoiceAddress,
                                FinvoiceOperator = customerInput.FinvoiceOperator,
                                IsFinvoiceCustomer = customerInput.IsFinvoiceCustomer,
                                BlockNote = customerInput.BlockNote,
                                BlockOrder = customerInput.BlockOrder,
                                BlockInvoice = customerInput.BlockInvoice,
                                CreditLimit = customerInput.CreditLimit,
                                IsCashCustomer = customerInput.IsCashCustomer,
                                State = (int)customerInput.State,
                                InvoiceDeliveryType = customerInput.InvoiceDeliveryType.ToNullable(),
                                InvoiceDeliveryProvider = customerInput.InvoiceDeliveryProvider.ToNullable(),
                                DepartmentNr = customerInput.DepartmentNr,
                                PayingCustomerId = customerInput.PayingCustomerId,
                                InvoicePaymentService = customerInput.InvoicePaymentService,
                                BankAccountNr = customer != null ? customer.BankAccountNr : customerInput.BankAccountNr,
                                ActorCompanyId = actorCompanyId,
                                AddAttachementsToEInvoice = customerInput.AddAttachementsToEInvoice,
                                ContactEComId = customerInput.ContactEComId,
                                IsPrivatePerson = customerInput.IsPrivatePerson,
                                ContactGLNId = customerInput.ContactGLNId,
                                InvoiceLabel = customerInput.InvoiceLabel,
                                ReminderContactEComId = customerInput.ReminderContactEComId,
                                AddSupplierInvoicesToEInvoices = customerInput.AddSupplierInvoicesToEInvoice,
                                IsOneTimeCustomer = customerInput.IsOneTimeCustomer,
                                ImportInvoicesDetailed = customerInput.ImportInvoicesDetailed,
                                OrderContactEComId = customerInput.OrderContactEComId,
                                TriangulationSales = customerInput.TriangulationSales,
                                ContractNr = customerInput.ContractNr,
                            };
                            SetCreatedProperties(customer);
                            entities.Customer.AddObject(customer);

                            #endregion

                            #region Actor

                            Actor actor = new Actor()
                            {
                                ActorType = (int)SoeActorType.Customer,

                                //Set references
                                Customer = customer,
                            };

                            result = AddEntityItem(entities, actor, "Actor", transaction);

                            if (result.Success)
                            {
                                customerId = customer.ActorCustomerId;
                            }
                            else
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }

                            #region OLD

                            /*if (result.Success == false)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }

                            result = MapActorToCustomer(entities, customer, actorCompanyId);
                            if (result.Success)
                            {
                                customerId = customer.ActorCustomerId;
                            }
                            else
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                                return result;
                            }*/

                            #endregion

                            #endregion

                            #region Paying customer

                            //If paying customer not set, then set to self
                            if (customerInput.PayingCustomerId == 0)
                            {
                                customer.PayingCustomerId = customer.ActorCustomerId;
                            }

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            #region Customer

                            if (customer.OrgNr != customerInput.OrgNr)
                            {
                                ActorManager.DeleteCompanyExternalCode(entities, TermGroup_CompanyExternalCodeEntity.Customer_InexchangeCompanyId, customer.ActorCustomerId, actorCompanyId, false);
                            }

                            // Update Customer
                            customer.VatType = (int)customerInput.VatType;
                            customer.DeliveryConditionId = customerInput.DeliveryConditionId.ToNullable();
                            customer.DeliveryTypeId = customerInput.DeliveryTypeId.ToNullable();
                            customer.PaymentConditionId = customerInput.PaymentConditionId.ToNullable();
                            customer.PriceListTypeId = customerInput.PriceListTypeId.ToNullable();
                            customer.CurrencyId = customerInput.CurrencyId;
                            customer.SysCountryId = customerInput.SysCountryId;
                            customer.SysLanguageId = customerInput.SysLanguageId;
                            customer.SysWholeSellerId = customerInput.SysWholeSellerId.ToNullable();
                            customer.CustomerNr = customerInput.CustomerNr?.Trim();
                            customer.Name = customerInput.Name?.Trim();
                            customer.OrgNr = customerInput.OrgNr?.Trim();
                            customer.VatNr = customerInput.VatNr?.Trim();
                            customer.InvoiceReference = customerInput.InvoiceReference;
                            customer.GracePeriodDays = customerInput.GracePeriodDays;
                            customer.PaymentMorale = customerInput.PaymentMorale;
                            customer.SupplierNr = customerInput.SupplierNr;
                            customer.OfferTemplate = customerInput.OfferTemplate;
                            customer.OrderTemplate = customerInput.OrderTemplate;
                            customer.BillingTemplate = customerInput.BillingTemplate;
                            customer.AgreementTemplate = customerInput.AgreementTemplate;
                            customer.ManualAccounting = customerInput.ManualAccounting;
                            customer.DiscountMerchandise = customerInput.DiscountMerchandise;
                            customer.Discount2Merchandise = customerInput.Discount2Merchandise;
                            customer.DiscountService = customerInput.DiscountService;
                            customer.Discount2Service = customerInput.Discount2Service;
                            customer.DisableInvoiceFee = customerInput.DisableInvoiceFee;
                            customer.Note = customerInput.Note;
                            customer.ShowNote = customerInput.ShowNote;
                            customer.FinvoiceAddress = customerInput.FinvoiceAddress;
                            customer.FinvoiceOperator = customerInput.FinvoiceOperator;
                            customer.IsFinvoiceCustomer = customerInput.IsFinvoiceCustomer;
                            customer.BlockNote = customerInput.BlockNote;
                            customer.BlockOrder = customerInput.BlockOrder;
                            customer.BlockInvoice = customerInput.BlockInvoice;
                            customer.CreditLimit = customerInput.CreditLimit;
                            customer.IsCashCustomer = customerInput.IsCashCustomer;
                            customer.InvoiceDeliveryType = customerInput.InvoiceDeliveryType.ToNullable();
                            customer.InvoiceDeliveryProvider = customerInput.InvoiceDeliveryProvider.ToNullable();
                            customer.State = (int)customerInput.State;
                            customer.DepartmentNr = customerInput.DepartmentNr;
                            customer.PayingCustomerId = customerInput.PayingCustomerId;
                            customer.InvoicePaymentService = customerInput.InvoicePaymentService;
                            customer.BankAccountNr = customerInput.BankAccountNr;
                            customer.AddAttachementsToEInvoice = customerInput.AddAttachementsToEInvoice;
                            customer.ContactEComId = customerInput.ContactEComId;
                            customer.IsPrivatePerson = customerInput.IsPrivatePerson;
                            customer.ContactGLNId = customerInput.ContactGLNId;
                            customer.InvoiceLabel = customerInput.InvoiceLabel;
                            customer.ReminderContactEComId = customerInput.ReminderContactEComId;
                            customer.AddSupplierInvoicesToEInvoices = customerInput.AddSupplierInvoicesToEInvoice;
                            customer.IsOneTimeCustomer = customerInput.IsOneTimeCustomer;
                            customer.ImportInvoicesDetailed = customerInput.ImportInvoicesDetailed;
                            customer.OrderContactEComId = customerInput.OrderContactEComId;
                            customer.TriangulationSales = customerInput.TriangulationSales;
                            customer.ContractNr = customerInput.ContractNr;

                            SetModifiedProperties(customer);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerNotUpdated;
                                return result;
                            }

                            #endregion

                            #region Paying customer

                            //If paying customer not set, then set to self
                            if (customerInput.PayingCustomerId == 0)
                            {
                                customer.PayingCustomerId = customer.ActorCustomerId;
                            }

                            #endregion


                            #endregion
                        }

                        #endregion

                        if (customer.IsPrivatePerson.HasValue && customer.IsPrivatePerson.Value)
                        {
                            var consent = customer.Actor.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.Unspecified);
                            if (consent == null)
                            {
                                consent = new ActorConsent();
                                customer.Actor.ActorConsent.Add(consent);
                            }

                            if ((consent.HasConsent != customerInput.HasConsent) || (consent.ConsentDate != customerInput.ConsentDate))
                            {
                                consent.HasConsent = customerInput.HasConsent;
                                consent.ConsentDate = consent.HasConsent ? customerInput.ConsentDate : null;
                                consent.ConsentModified = DateTime.Now;
                                consent.ConsentModifiedBy = GetUserDetails();
                            }
                        }

                        #region Addresses

                        // Check if GLN numbers charactores are valid
                        if (customerInput.ContactAddresses.Any(c => c.ContactAddressItemType == ContactAddressItemType.GlnNumber && c.EComText != null && c.EComText.Length != 13))
                        {
                            return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(6152, "Det går inte att spara. GLN nummer består av 13 siffror."));
                        }


                        result = ContactManager.SaveContactAddresses(entities, customerInput.ContactAddresses, customerId, TermGroup_SysContactType.Company, entityType: SoeEntityType.Customer);
                        if (!result.Success)
                        {
                            result.ErrorNumber = (int)ActionResultSave.CustomerContactsAndTeleComNotSaved;
                            return result;
                        }

                        #endregion

                        #region Categories

                        if (categoryRecords != null)
                        {
                            // Silverlight
                            result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, categoryRecords, actorCompanyId, SoeCategoryType.Customer, SoeCategoryRecordEntity.Customer, customerId);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerCompanyCategoryNotSaved;
                                result.ErrorMessage = GetText(11012, "Alla kategorier kunde inte sparas");
                                return result;
                            }
                        }
                        else if (customerInput.CategoryIds != null)
                        {
                            // Angular
                            result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, customerInput.CategoryIds, actorCompanyId, SoeCategoryType.Customer, SoeCategoryRecordEntity.Customer, customerId);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.CustomerCompanyCategoryNotSaved;
                                result.ErrorMessage = GetText(11012, "Alla kategorier kunde inte sparas");
                                return result;
                            }
                        }

                        #endregion

                        #region ContactPersons

                        if (contactPersonIds != null)
                        {
                            result = ContactManager.SaveContactPersonMappings(entities, contactPersonIds, customerId);
                            if (!result.Success)
                                return result;
                        }

                        #endregion

                        #region Finvoice

                        if (!customer.IsPrivatePerson.GetValueOrDefault() && customerInput.InvoiceDeliveryType == (int)SoeInvoiceDeliveryType.Electronic)
                        {
                            TermGroup_EInvoiceFormat einvoiceFormat = (TermGroup_EInvoiceFormat)SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceFormat, 0, actorCompanyId, 0);
                            if (FinvoiceBase.IsFinvoice(einvoiceFormat))
                            {
                                if (!customerInput.VatNr.HasValue() || !customerInput.OrgNr.HasValue() || !customerInput.FinvoiceAddress.HasValue() || !customerInput.FinvoiceOperator.HasValue())
                                {
                                    result.Success = false;
                                    result.ErrorMessage = GetText(7358, "Du har valt fakturametod \"E-faktura\". För att kunna spara kundkortet krävs det att du fyller i organisationsnummer, VAT-nummer, Finvoice adress och operatör.");
                                    return result;
                                }
                            }
                            else if (einvoiceFormat != TermGroup_EInvoiceFormat.Intrum && (!customerInput.OrgNr.HasValue() && !customerInput.ContactAddresses.Any(c => c.ContactAddressItemType == ContactAddressItemType.GlnNumber)))
                            {
                                result.Success = false;
                                result.ErrorMessage = GetText(7359, "Du har valt fakturametod \"E-faktura\". För att kunna spara kundkortet krävs det att du fyller i organisationsnummer eller ett GLN nummer under kontakter.");
                                return result;
                            }
                        }

                        #endregion

                        #region AccountingSettings

                        SaveCustomerAccount(entities, customer, customerInput.AccountingSettings, actorCompanyId);

                        #endregion

                        #region CustomerUser

                        result = SaveCustomersUsers(entities, customer, customerInput.CustomerUsers, actorCompanyId);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region HouseholdApplicants

                        if (householdTaxApplicantInputs != null)
                        {
                            #region Update/Delete HouseholdApplicants

                            // Loop over existing
                            if (!customer.HouseholdTaxDeductionApplicant.IsLoaded)
                                customer.HouseholdTaxDeductionApplicant.Load();

                            foreach (HouseholdTaxDeductionApplicant applicant in customer.HouseholdTaxDeductionApplicant.Where(a => a.State == (int)SoeEntityState.Active))
                            {
                                HouseholdTaxDeductionApplicantDTO applicantInput = householdTaxApplicantInputs.FirstOrDefault(h => h.HouseholdTaxDeductionApplicantId == applicant.HouseholdTaxDeductionApplicantId);
                                if (applicantInput != null)
                                {
                                    #region Update

                                    applicant.Name = applicantInput.Name;
                                    applicant.SocialSecNr = applicantInput.SocialSecNr;
                                    applicant.Property = applicantInput.Property;
                                    applicant.ApartmentNr = applicantInput.ApartmentNr;
                                    applicant.CooperativeOrgNr = applicantInput.CooperativeOrgNr;
                                    applicant.Share = applicantInput.Share;
                                    householdTaxApplicantInputs.Remove(applicantInput);

                                    #endregion
                                }
                                else
                                {
                                    #region Delete

                                    applicant.State = (int)SoeEntityState.Deleted;

                                    #endregion

                                    #region handle contact hidden on row

                                    var query = (from h in entities.HouseholdTaxDeductionRow
                                                 where
                                                 h.CustomerInvoiceRow.CustomerInvoice.ActorId == customer.ActorCustomerId && !h.ContactHidden
                                                 select h);

                                    if (!String.IsNullOrEmpty(applicant.Property))
                                        query = query.Where(a => a.Property.ToLower() == applicant.Property.ToLower());

                                    if (!String.IsNullOrEmpty(applicant.Name))
                                        query = query.Where(a => a.Name.ToLower() == applicant.Name.ToLower());

                                    if (!String.IsNullOrEmpty(applicant.CooperativeOrgNr))
                                        query = query.Where(a => a.CooperativeOrgNr == applicant.CooperativeOrgNr);

                                    if (!String.IsNullOrEmpty(applicant.ApartmentNr))
                                        query = query.Where(a => a.ApartmentNr == applicant.ApartmentNr);

                                    if (!String.IsNullOrEmpty(applicant.SocialSecNr))
                                        query = query.Where(a => a.SocialSecNr == applicant.SocialSecNr);

                                    var applicants = query.ToList();

                                    foreach (var app in applicants)
                                    {
                                        app.ContactHidden = true;

                                        SetModifiedProperties(app);
                                    }

                                    #endregion
                                }
                                SetModifiedProperties(applicant);
                            }

                            #endregion

                            #region Handle contacthidden

                            foreach (var applicantRow in householdTaxApplicantInputs.Where(a => a.CustomerInvoiceRowId.HasValue))
                            {
                                var query = (from h in entities.HouseholdTaxDeductionRow
                                             where
                                             h.CustomerInvoiceRow.CustomerInvoice.ActorId == customer.ActorCustomerId && !h.ContactHidden
                                             select h);

                                if (!String.IsNullOrEmpty(applicantRow.Name))
                                    query = query.Where(a => a.Name == applicantRow.Name);

                                if (!String.IsNullOrEmpty(applicantRow.Property))
                                    query = query.Where(a => a.Property == applicantRow.Property);

                                if (!String.IsNullOrEmpty(applicantRow.CooperativeOrgNr))
                                    query = query.Where(a => a.CooperativeOrgNr == applicantRow.CooperativeOrgNr);

                                if (!String.IsNullOrEmpty(applicantRow.ApartmentNr))
                                    query = query.Where(a => a.ApartmentNr == applicantRow.ApartmentNr);

                                if (!String.IsNullOrEmpty(applicantRow.SocialSecNr))
                                    query = query.Where(a => a.SocialSecNr == applicantRow.SocialSecNr);

                                var applicants = query.ToList();

                                foreach (var applicant in applicants)
                                {
                                    applicant.ContactHidden = true;

                                    SetModifiedProperties(applicant);
                                }
                            }

                            #endregion

                            #region Add HouseholdApplicants

                            foreach (HouseholdTaxDeductionApplicantDTO applicantInput in householdTaxApplicantInputs.Where(a => !a.CustomerInvoiceRowId.HasValue))
                            {
                                var applicant = new HouseholdTaxDeductionApplicant
                                {
                                    Name = applicantInput.Name,
                                    SocialSecNr = applicantInput.SocialSecNr,
                                    Property = applicantInput.Property,
                                    ApartmentNr = applicantInput.ApartmentNr,
                                    CooperativeOrgNr = applicantInput.CooperativeOrgNr,
                                    Share = applicantInput.Share,
                                };

                                SetCreatedProperties(applicant);
                                customer.HouseholdTaxDeductionApplicant.Add(applicant);
                            }

                            #endregion
                        }

                        #endregion

                        #region CustomerProducts

                        if (customerInput.CustomerProducts != null)
                        {
                            #region Update/Delete CustomerProducts

                            // Loop over existing
                            if (!customer.CustomerProduct.IsLoaded)
                                customer.CustomerProduct.Load();
                            foreach (CustomerProduct prod in customer.CustomerProduct.ToList())
                            {
                                CustomerProductPriceSmallDTO prodInput = customerInput.CustomerProducts.FirstOrDefault(p => p.CustomerProductId == prod.CustomerProductId);
                                if (prodInput != null)
                                {
                                    #region Update

                                    prod.ProductId = prodInput.ProductId;
                                    prod.Price = prodInput.Price;
                                    customerInput.CustomerProducts.Remove(prodInput);
                                    SetModifiedProperties(prod);

                                    #endregion
                                }
                                else
                                {
                                    #region Delete

                                    entities.DeleteObject(prod);

                                    #endregion
                                }
                            }

                            #endregion

                            #region Add CustomerProducts

                            foreach (CustomerProductPriceSmallDTO prodInput in customerInput.CustomerProducts)
                            {
                                CustomerProduct prod = new CustomerProduct()
                                {
                                    ProductId = prodInput.ProductId,
                                    Price = prodInput.Price
                                };

                                SetCreatedProperties(prod);
                                customer.CustomerProduct.Add(prod);
                            }

                            #endregion
                        }
                        #region Files

                        if (customerInput.Files != null)
                        {
                            var images = customerInput.Files.Where(f => f.ImageId.HasValue);
                            GraphicsManager.UpdateImages(entities, images, customer.ActorCustomerId);

                            var files = customerInput.Files.Where(f => f.Id.HasValue);
                            GeneralManager.UpdateFiles(entities, files, customer.ActorCustomerId, SoeEntityType.Customer);

                            entities.SaveChanges();
                        }

                        #endregion

                        ////int noOfObjects = 0;
                        //foreach (CustomerProductPriceSmallDTO cpp in customerInput.CustomerProducts)
                        //{
                        //    //noOfObjects++;

                        //    if (cpp.CustomerProductId != 0)
                        //    {
                        //        CustomerProduct customerProduct = customer.CustomerProduct.Where(p => p.CustomerProductId == cpp.CustomerProductId).FirstOrDefault(); ;

                        //        if (customerProduct == null)
                        //        {
                        //            #region Add

                        //            customerProduct = new CustomerProduct()
                        //            {
                        //                //CustomerProductId = noOfObjects,
                        //                ActorCustomerId = customer.ActorCustomerId,
                        //                ProductId = cpp.ProductId,
                        //                Price = cpp.Price,
                        //            };

                        //            customer.CustomerProduct.Add(customerProduct);

                        //            #endregion
                        //        }
                        //        else
                        //        {
                        //            #region Update

                        //            if (cpp.State == SoeEntityState.Deleted)
                        //            {
                        //                entities.DeleteObject(customerProduct);
                        //            }
                        //            else
                        //            {
                        //                customerProduct.ProductId = cpp.ProductId;
                        //                customerProduct.Price = cpp.Price;
                        //                SetModifiedProperties(customerProduct);
                        //            }

                        //            #endregion
                        //        }
                        //    }
                        //    else
                        //    {
                        //        #region Add

                        //        CustomerProduct customerProduct = new CustomerProduct()
                        //        {
                        //            //CustomerProductId = noOfObjects,
                        //            ActorCustomerId = customer.ActorCustomerId,
                        //            ProductId = cpp.ProductId,
                        //            Price = cpp.Price,
                        //        };

                        //        customer.CustomerProduct.Add(customerProduct);

                        //        #endregion
                        //    }
                        //}

                        #endregion

                        #region ExtraFields

                        if (extraFields != null && extraFields.Count > 0)
                        {
                            result = ExtraFieldManager.SaveExtraFieldRecords(entities, extraFields, (int)SoeEntityType.Customer, customerId, actorCompanyId);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                return result;
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        if (!result.Success)
                            result.ErrorNumber = (int)ActionResultSave.CustomerNotSaved;
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = customerId;

                        if(ElectronicInvoiceMananger.HasFortnoxVismaIntegration(actorCompanyId, SettingManager))
                            SyncCustomerFortnoxVisma(customer.ActorCustomerId, customerInput);
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private void SyncCustomerFortnoxVisma(int actorCustomerId, CustomerDTO customerInput)
        {
            // Sync customer
            Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() =>
            {
                try
                {
                    var iem = new ElectronicInvoiceMananger(parameterObject);
                    iem.SyncCustomerFortnoxVisma(actorCustomerId, customerInput);
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
                }
            }));
        }

        private void SaveCustomerAccount(CompEntities entities, Customer customer, List<AccountingSettingsRowDTO> accountingSettings, int actorCompanyId)
        {
            if (!accountingSettings.IsNullOrEmpty())
            {
                List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true);

                #region Update AccountingSettings

                if (!customer.CustomerAccountStd.IsNullOrEmpty())
                {
                    foreach (CustomerAccountStd customerAccountStd in customer.CustomerAccountStd.ToList())
                    {
                        // Find setting in input
                        AccountingSettingsRowDTO settingInput = accountingSettings.FirstOrDefault(a => a.Type == customerAccountStd.Type);
                        if (settingInput != null)
                        {
                            // Update account
                            if (settingInput.Account1Id == 0)
                            {
                                customerAccountStd.AccountStd = null;
                            }
                            else if (customerAccountStd.AccountStd?.AccountId != settingInput.Account1Id)
                            {
                                customerAccountStd.AccountStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);
                            }

                            // Remove existing internal accounts
                            // No way to update them
                            customerAccountStd.AccountInternal.Clear();

                            // Internal accounts
                            int dimCounter = 1;
                            foreach (AccountDim dim in dims)
                            {
                                // Get internal account from input
                                dimCounter++;
                                int accountId = 0;
                                if (dimCounter == 2)
                                    accountId = settingInput.Account2Id;
                                else if (dimCounter == 3)
                                    accountId = settingInput.Account3Id;
                                else if (dimCounter == 4)
                                    accountId = settingInput.Account4Id;
                                else if (dimCounter == 5)
                                    accountId = settingInput.Account5Id;
                                else if (dimCounter == 6)
                                    accountId = settingInput.Account6Id;

                                // Add account internal
                                AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                                if (accountInternal != null)
                                    customerAccountStd.AccountInternal.Add(accountInternal);
                            }

                            if (customerAccountStd.AccountStd == null && !customerAccountStd.AccountInternal.Any())
                            {
                                customer.CustomerAccountStd.Remove(customerAccountStd);
                                entities.DeleteObject(customerAccountStd);
                            }

                            // Remove from input to prevent adding below
                            accountingSettings.Remove(settingInput);
                        }
                    }
                }

                #endregion

                #region Add AccountingSettings

                //AccountingSettings can have been removed by update
                if (!accountingSettings.IsNullOrEmpty())
                {
                    if (customer.CustomerAccountStd == null)
                        customer.CustomerAccountStd = new EntityCollection<CustomerAccountStd>();

                    // Only add standard account
                    // Internal accounts will be handled below
                    foreach (AccountingSettingsRowDTO settingInput in accountingSettings)
                    {
                        // Standard account
                        AccountStd accStd = settingInput.Account1Id > 0 ? AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true) : null;

                        var customerAccountStd = new CustomerAccountStd
                        {
                            Type = settingInput.Type,
                            AccountStd = accStd
                        };

                        // Internal accounts
                        int dimCounter = 1;
                        foreach (AccountDim dim in dims)
                        {
                            // Get internal account from input
                            dimCounter++;
                            int accountId = 0;
                            if (dimCounter == 2)
                                accountId = settingInput.Account2Id;
                            else if (dimCounter == 3)
                                accountId = settingInput.Account3Id;
                            else if (dimCounter == 4)
                                accountId = settingInput.Account4Id;
                            else if (dimCounter == 5)
                                accountId = settingInput.Account5Id;
                            else if (dimCounter == 6)
                                accountId = settingInput.Account6Id;

                            // Add account internal
                            AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                            if (accountInternal != null)
                                customerAccountStd.AccountInternal.Add(accountInternal);
                        }

                        if (customerAccountStd.AccountStd != null || customerAccountStd.AccountInternal.Any())
                        {
                            customer.CustomerAccountStd.Add(customerAccountStd);
                        }
                    }
                }

                #endregion
            }
        }

        public ActionResult DeleteCustomer(int customerId, int actorCompanyId)
        {
            //Check relation dependencies
            using (CompEntities entities = new CompEntities())
            {
                return DeleteCustomer(entities, customerId, actorCompanyId, true, false);
            }
        }

        public ActionResult DeleteCustomer(CompEntities entities, int customerId, int actorCompanyId, bool saveChanges = true, bool clearValues = false)
        {
            //Check relation dependencies
            if (HasCustomerOrigins(SoeOriginType.CustomerInvoice, customerId, actorCompanyId))
                return new ActionResult((int)ActionResultDelete.CustomerHasInvoices, GetText(7513, "Kund har fakturor"));
            if (HasCustomerOrigins(SoeOriginType.Order, customerId, actorCompanyId))
                return new ActionResult((int)ActionResultDelete.CustomerHasOrders);
            if (HasCustomerOrigins(SoeOriginType.Offer, customerId, actorCompanyId))
                return new ActionResult((int)ActionResultDelete.CustomerHasOffers);
            if (HasCustomerOrigins(SoeOriginType.Contract, customerId, actorCompanyId))
                return new ActionResult((int)ActionResultDelete.CustomerHasContracts);

            Customer originalCustomer = GetCustomer(entities, customerId, loadActor: true, loadContactAddresses: clearValues);
            if (originalCustomer == null)
                return new ActionResult((int)ActionResultDelete.EntityNotFound, "Customer");

            var result = UnMapActorFromCustomer(entities, originalCustomer, actorCompanyId);
            if (!result.Success)
                return result;

            if (clearValues)
            {
                var deleteText = GetText(7413, "RADERAD UPPGIFT");

                originalCustomer.CustomerNr = " ";
                originalCustomer.Name = deleteText + " " + DateTime.Today.ToShortDateString();
                originalCustomer.OrgNr = " ";
                originalCustomer.VatNr = " ";
                originalCustomer.InvoiceReference = " ";
                originalCustomer.Note = " ";
                originalCustomer.FinvoiceAddress = " ";
                originalCustomer.FinvoiceOperator = " ";
                originalCustomer.BlockNote = " ";
                originalCustomer.DepartmentNr = " ";
                originalCustomer.BankAccountNr = " ";

                var contact = originalCustomer.Actor.Contact.FirstOrDefault();
                if (contact != null)
                {
                    foreach (var ecom in contact.ContactECom)
                    {
                        ecom.Name = deleteText + " " + DateTime.Today.ToShortDateString();
                        ecom.Text = " ";
                        ecom.Description = " ";

                        SetModifiedProperties(ecom);
                    }

                    foreach (var address in contact.ContactAddress)
                    {
                        foreach (var addressRow in address.ContactAddressRow)
                        {
                            addressRow.Text = "RADERAD UPPGIFT " + DateTime.Today.ToShortDateString();

                            SetModifiedProperties(addressRow);
                        }
                        address.Name = "RADERAD UPPGIFT " + DateTime.Today.ToShortDateString();

                        SetModifiedProperties(address);
                    }
                }

                if (!originalCustomer.HouseholdTaxDeductionApplicant.IsLoaded)
                    originalCustomer.HouseholdTaxDeductionApplicant.Load();

                foreach (var applicant in originalCustomer.HouseholdTaxDeductionApplicant)
                {
                    applicant.ApartmentNr = " ";
                    applicant.CooperativeOrgNr = " ";
                    applicant.Name = deleteText + " " + DateTime.Today.ToShortDateString();
                    applicant.Property = " ";
                    applicant.SocialSecNr = " ";
                    applicant.State = (int)SoeEntityState.Deleted;

                    SetModifiedProperties(applicant);
                }

                SetModifiedProperties(originalCustomer);
            }

            //Set the Customer to deleted if no other Companies use it
            if (originalCustomer.Actors.Count == 0)
            {
                result = ChangeEntityState(entities, originalCustomer, SoeEntityState.Deleted, saveChanges);
                if (result.Success)
                {
                    result = ActorManager.DeleteExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.Customer, originalCustomer.ActorCustomerId, actorCompanyId, true);
                    if (!result.Success)
                        return result;

                }
            }

            return result;
        }

        public ActionResult UpdateCustomersState(Dictionary<int, bool> customers)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                foreach (KeyValuePair<int, bool> customer in customers)
                {
                    Customer originalCustomer = GetCustomer(entities, customer.Key, false, false, false, false);
                    if (originalCustomer == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Customer");

                    ChangeEntityState(originalCustomer, customer.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                }

                result = SaveChanges(entities);
            }

            return result;
        }

        public ActionResult UpdateCustomersIsPrivatePerson(Dictionary<int, bool> customers)
        {
            using (CompEntities entities = new CompEntities())
            {
                foreach (KeyValuePair<int, bool> customer in customers)
                {
                    Customer originalCustomer = GetCustomer(entities, customer.Key, false, false, false, false);
                    if (originalCustomer == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Customer");

                    originalCustomer.IsPrivatePerson = customer.Value;
                }

                return SaveChanges(entities);
            }

        }

        #endregion

        #region CustomerUser

        public List<CustomerUser> GetCustomerUsers(CompEntities entities, int actorCompanyId)
        {
            return (from c in entities.CustomerUser
                    where c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    select c).ToList();
        }

        private List<Customer> FilterCustomersUsers(CompEntities entities, List<Customer> customers, int actorCompanyId, int roleId, int userId)
        {
            List<CustomerUser> customerUsers = GetCustomerUsers(entities, actorCompanyId);

            if (FeatureManager.HasRolePermission(Feature.Economy_Customer_Customers_Edit_OnlyPersonal, Permission.Readonly, roleId, actorCompanyId) ||
                FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit_OnlyPersonal, Permission.Readonly, roleId, actorCompanyId))
            {
                List<Customer> validatedCustomers = new List<Customer>();
                foreach (Customer customer in customers)
                {
                    bool valid = false;

                    var users = customerUsers.Where(i => i.ActorCustomerId == customer.ActorCustomerId).ToList();
                    if (users.Count > 0)
                    {
                        //Check if user is valid
                        valid = users.Any(i => i.UserId == userId);
                    }

                    if (valid)
                        validatedCustomers.Add(customer);
                }

                return validatedCustomers.OrderBy(i => i.CustomerNrSort).ToList();
            }
            else
            {
                //Show all for those with roles that has permission to set users on customer
                if (
                    !customerUsers.Any() ||
                    FeatureManager.HasRolePermission(Feature.Economy_Customer_Customers_Edit_Users, Permission.Readonly, roleId, actorCompanyId) ||
                    FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit_Users, Permission.Readonly, roleId, actorCompanyId))
                {
                    return customers;
                }

                List<Customer> validatedCustomers = new List<Customer>();
                foreach (Customer customer in customers)
                {
                    bool valid = false;

                    var users = customerUsers.Where(i => i.ActorCustomerId == customer.ActorCustomerId).ToList();
                    if (users.Count == 0)
                    {
                        //Valid for all users
                        valid = true;
                    }
                    else
                    {
                        //Check if user is valid
                        valid = users.Any(i => i.UserId == userId);
                    }

                    if (valid)
                        validatedCustomers.Add(customer);
                }

                return validatedCustomers.OrderBy(i => i.CustomerNrSort).ToList();
            }
        }

        private List<CustomerSmallDTO> FilterCustomersUsers(CompEntities entities, List<CustomerSmallDTO> customers, int actorCompanyId, int roleId, int userId)
        {
            List<CustomerUser> customerUsers = GetCustomerUsers(entities, actorCompanyId);

            if (FeatureManager.HasRolePermission(Feature.Economy_Customer_Customers_Edit_OnlyPersonal, Permission.Readonly, roleId, actorCompanyId) ||
                FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit_OnlyPersonal, Permission.Readonly, roleId, actorCompanyId))
            {
                var validatedCustomers = new List<CustomerSmallDTO>();
                foreach (var customer in customers)
                {
                    bool valid = false;

                    var users = customerUsers.Where(i => i.ActorCustomerId == customer.ActorCustomerId).ToList();
                    if (users.Count > 0)
                    {
                        //Check if user is valid
                        valid = users.Any(i => i.UserId == userId);
                    }

                    if (valid)
                        validatedCustomers.Add(customer);
                }

                return validatedCustomers;
            }
            else
            {
                //Show all for those with roles that has permission to set users on customer
                if (
                    !customerUsers.Any() ||
                    FeatureManager.HasRolePermission(Feature.Economy_Customer_Customers_Edit_Users, Permission.Readonly, roleId, actorCompanyId) ||
                    FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit_Users, Permission.Readonly, roleId, actorCompanyId))
                {
                    return customers;
                }

                var validatedCustomers = new List<CustomerSmallDTO>();
                foreach (var customer in customers)
                {
                    bool valid = false;

                    var users = customerUsers.Where(i => i.ActorCustomerId == customer.ActorCustomerId).ToList();
                    if (users.Count == 0)
                    {
                        //Valid for all users
                        valid = true;
                    }
                    else
                    {
                        //Check if user is valid
                        valid = users.Any(i => i.UserId == userId);
                    }

                    if (valid)
                        validatedCustomers.Add(customer);
                }

                return validatedCustomers;
            }
        }
        private List<CustomerGridDTO> FilterCustomersUsers(CompEntities entities, List<CustomerGridDTO> customers, int actorCompanyId, int roleId, int userId)
        {
            List<CustomerUser> customerUsers = GetCustomerUsers(entities, actorCompanyId);

            if (FeatureManager.HasRolePermission(Feature.Economy_Customer_Customers_Edit_OnlyPersonal, Permission.Readonly, roleId, actorCompanyId) ||
                FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit_OnlyPersonal, Permission.Readonly, roleId, actorCompanyId))
            {
                var validatedCustomers = new List<CustomerGridDTO>();
                foreach (var customer in customers)
                {
                    bool valid = false;

                    var users = customerUsers.Where(i => i.ActorCustomerId == customer.ActorCustomerId).ToList();
                    if (users.Count > 0)
                    {
                        //Check if user is valid
                        valid = users.Any(i => i.UserId == userId);
                    }

                    if (valid)
                        validatedCustomers.Add(customer);
                }

                return validatedCustomers;
            }
            else
            {
                //Show all for those with roles that has permission to set users on customer
                if (
                    !customerUsers.Any() ||
                    FeatureManager.HasRolePermission(Feature.Economy_Customer_Customers_Edit_Users, Permission.Readonly, roleId, actorCompanyId) ||
                    FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit_Users, Permission.Readonly, roleId, actorCompanyId))
                {
                    return customers;
                }

                var validatedCustomers = new List<CustomerGridDTO>();
                foreach (var customer in customers)
                {
                    bool valid = false;

                    var users = customerUsers.Where(i => i.ActorCustomerId == customer.ActorCustomerId).ToList();
                    if (users.Count == 0)
                    {
                        //Valid for all users
                        valid = true;
                    }
                    else
                    {
                        //Check if user is valid
                        valid = users.Any(i => i.UserId == userId);
                    }

                    if (valid)
                        validatedCustomers.Add(customer);
                }

                return validatedCustomers;
            }
        }

        public ActionResult SaveCustomersUsers(CompEntities entities, Customer customer, List<CustomerUserDTO> customerUserDTOs, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            if (customer == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Customer");

            if (customerUserDTOs == null)
                customerUserDTOs = new List<CustomerUserDTO>();
            if (!customer.CustomerUser.IsLoaded)
                customer.CustomerUser.Load();

            foreach (CustomerUserDTO customerUserDTO in customerUserDTOs)
            {
                CustomerUser customerUser = customer.CustomerUser.FirstOrDefault(u => u.UserId == customerUserDTO.UserId);
                if (customerUser == null)
                {
                    #region Add

                    customerUser = new CustomerUser()
                    {
                        Main = customerUserDTO.Main,

                        //Set FK
                        UserId = customerUserDTO.UserId,
                        ActorCompanyId = actorCompanyId,
                    };
                    SetCreatedProperties(customerUser);
                    customer.CustomerUser.Add(customerUser);

                    #endregion
                }
                else
                {
                    #region Update

                    // User exists on origin
                    if (customerUser.State != (int)SoeEntityState.Active)
                    {
                        // User is deleted or inactive, reactivate it
                        customerUser.State = (int)SoeEntityState.Active;
                        SetModifiedProperties(customerUser);
                    }
                    if (customerUser.Main != customerUserDTO.Main)
                    {
                        customerUser.Main = customerUserDTO.Main;
                        SetModifiedProperties(customerUser);
                    }

                    #endregion
                }
            }

            #region Delete

            // Remove deleted users
            foreach (CustomerUser customerUser in customer.CustomerUser.Where(o => o.State == (int)SoeEntityState.Active))
            {
                CustomerUserDTO customerUserDTO = customerUserDTOs.FirstOrDefault(u => u.UserId == customerUser.UserId && u.State == (int)SoeEntityState.Active);
                if (customerUserDTO == null)
                    ChangeEntityState(customerUser, SoeEntityState.Deleted);
            }

            #endregion

            return result;
        }

        #endregion

        #region CustomerHouseholdApplicants

        public ActionResult SaveHouseholdTaxDeductionApplicant(CompEntities entities, Customer customer, int hdApplicantId, string name, string socSecNr, string property, string apartmentNr, string coopOrgNr)
        {
            if (hdApplicantId != 0)
            {
                if (!customer.HouseholdTaxDeductionApplicant.IsLoaded)
                    customer.HouseholdTaxDeductionApplicant.Load();

                HouseholdTaxDeductionApplicant appl = customer.HouseholdTaxDeductionApplicant.FirstOrDefault(a => a.HouseholdTaxDeductionApplicantId == hdApplicantId && a.State == (int)SoeEntityState.Active);
                if (appl != null)
                {
                    appl.Name = name;
                    appl.SocialSecNr = socSecNr;
                    appl.Property = property;
                    appl.ApartmentNr = apartmentNr;
                    appl.CooperativeOrgNr = coopOrgNr;

                    SetModifiedProperties(appl);
                }
                else
                {
                    return new ActionResult(false);
                }

            }
            else
            {
                HouseholdTaxDeductionApplicant appl = new HouseholdTaxDeductionApplicant()
                {
                    Name = name,
                    SocialSecNr = socSecNr,
                    Property = property,
                    ApartmentNr = apartmentNr,
                    CooperativeOrgNr = coopOrgNr,
                    Share = 0,
                };

                SetCreatedProperties(appl);
                customer.HouseholdTaxDeductionApplicant.Add(appl);
            }

            return SaveChanges(entities);
        }

        public ActionResult DeleteHouseholdTaxDeductionApplicant(int householdTaxDeductionApplicantId)
        {
            using (CompEntities entities = new CompEntities())
            {
                HouseholdTaxDeductionApplicant applicant = (from a in entities.HouseholdTaxDeductionApplicant
                                                            where a.HouseholdTaxDeductionApplicantId == householdTaxDeductionApplicantId
                                                            select a).FirstOrDefault();

                if (applicant != null)
                {
                    applicant.State = (int)SoeEntityState.Deleted;
                    SetModifiedProperties(applicant);
                    return SaveChanges(entities);
                }
                else
                {
                    return new ActionResult(false);
                }
            }
        }


        #endregion

        #region CustomerAccountStd



        public Dictionary<int, string> GetAllCustomerAccounts(int actorCompanyId, CustomerAccountType customerAccountType)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.CustomerAccountStd.NoTracking();
            return (from cas in entitiesReadOnly.CustomerAccountStd
                    where cas.AccountStd.Account.ActorCompanyId == actorCompanyId &&
                    (cas.Type == (int)customerAccountType)
                    select new
                    {
                        AccountNr = cas.AccountStd.Account.AccountNr ?? "",
                        ActorCustomerId = cas.ActorCustomerId
                    }).ToDictionary(a => a.ActorCustomerId, a => a.AccountNr);
        }

        public IEnumerable<CustomerAccountStd> GetCustomerAccounts(int actorCustomerId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CustomerAccountStd.NoTracking();
            return GetCustomerAccounts(entities, actorCustomerId);
        }

        public IEnumerable<CustomerAccountStd> GetCustomerAccounts(CompEntities entities, int actorCustomerId)
        {
            return (from cas in entities.CustomerAccountStd
                        .Include("AccountStd.Account")
                        .Include("AccountInternal.Account.AccountDim")
                    where cas.ActorCustomerId == actorCustomerId
                    select cas).ToList();
        }

        public CustomerAccountStd GetCustomerAccount(CompEntities entities, int actorCustomerId, CustomerAccountType customerAccountType)
        {
            int type = (int)customerAccountType;
            return (from cas in entities.CustomerAccountStd
                        .Include("AccountStd.Account")
                        .Include("AccountInternal.Account.AccountDim")
                    where ((cas.ActorCustomerId == actorCustomerId) &&
                    (cas.Type == type))
                    select cas).FirstOrDefault();
        }

        #region Help-methods

        /// <summary>
        /// Formats the address accourding to the format string with the addressrowtypes as parameters.
        /// </summary>
        /// <param name="contactAddress">The contact address, use filters if neccessary before entering the parameter.</param>
        /// <param name="formatString">The formatString that should contain parameters such as {0}</param>
        /// <param name="addressRowTypes"></param>
        /// <returns></returns>
        private static string FormatAddress(ContactAddress contactAddress, string formatString, params TermGroup_SysContactAddressRowType[] addressRowTypes)
        {
            List<string> texts = new List<string>();

            foreach (TermGroup_SysContactAddressRowType addressRowType in addressRowTypes)
            {
                string text = "";

                ContactAddressRow contactAddressRow = contactAddress.ContactAddressRow.FirstOrDefault(ca => ca.SysContactAddressRowTypeId == (int)addressRowType);
                if (contactAddressRow != null)
                    text = contactAddressRow.Text.Trim();

                texts.Add(text);
            }

            return String.Format(formatString, texts.ToArray());
        }

        private static bool AddressContains(ContactAddress contactAddress, string searchTerm, params TermGroup_SysContactAddressRowType[] addressRowTypes)
        {
            if (searchTerm == string.Empty)
                return true;
            else if (searchTerm == null)
                return false;

            searchTerm = searchTerm.Trim().ToLower();

            foreach (TermGroup_SysContactAddressRowType addressRowType in addressRowTypes)
            {
                ContactAddressRow contactAddressRow = contactAddress.ContactAddressRow.FirstOrDefault(ca => ca.SysContactAddressRowTypeId == (int)addressRowType && ca.Text.ToLower().Contains(searchTerm));
                if (contactAddressRow != null)
                    return true;
            }

            return false;
        }

        #endregion

        #endregion

        internal ActionResult AddCustomerAddress(TermGroup_SysContactAddressType addressType, int customerId, string streetAddress, string city, string coAddress, string postalNr, string country, string Name)
        {
            var result = new ActionResult();
            using (var entities = new CompEntities())
            {
                var contact = ContactManager.GetContactFromActor(entities, customerId);
                var ca = new ContactAddress()
                {
                    Contact = contact,
                    SysContactAddressTypeId = (int)addressType,
                };

                if (ca.ContactAddressRow == null)
                    ca.ContactAddressRow = new EntityCollection<ContactAddressRow>();

                if (!string.IsNullOrEmpty(streetAddress))
                {
                    ca.ContactAddressRow.Add(
                            new ContactAddressRow()
                            {
                                SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.Address,
                                Text = streetAddress,
                            });
                }
                if (!string.IsNullOrEmpty(postalNr))
                {
                    ca.ContactAddressRow.Add(
                        new ContactAddressRow()
                        {
                            SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalCode,
                            Text = postalNr,
                        });
                }
                if (!string.IsNullOrEmpty(coAddress))
                {
                    ca.ContactAddressRow.Add(
                            new ContactAddressRow()
                            {
                                SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.AddressCO,
                                Text = coAddress,
                            });
                }
                if (!string.IsNullOrEmpty(city))
                {
                    ca.ContactAddressRow.Add(
                            new ContactAddressRow()
                            {
                                SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalAddress,
                                Text = city,
                            });
                }

                if (!string.IsNullOrEmpty(country))
                {
                    ca.ContactAddressRow.Add(
                            new ContactAddressRow()
                            {
                                SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.Country,
                                Text = country,
                            });
                }

                if (!string.IsNullOrEmpty(Name))
                {
                    ca.ContactAddressRow.Add(
                            new ContactAddressRow()
                            {
                                SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.Name,
                                Text = Name,
                            });
                }

                result = SaveChanges(entities);
                result.IntegerValue = ca.ContactAddressId;
            }

            return result;
        }


        public ActionResult AddCustomerEcom(int customerId, TermGroup_SysContactEComType type, string text)
        {
            using (var entities = new CompEntities())
            {
                var contact = ContactManager.GetContactFromActor(entities, customerId);
                return ContactManager.AddContactECom(entities, contact, (int)type, text, null, true);
            }
        }

        public List<EInvoiceRecipientSearchResultDTO> GetCustomerEInvoiceRecipients(EInvoiceRecipientSearchDTO model)
        {
            var releaseMode = ElectronicInvoiceMananger.GetInexchangeReleaseMode(SettingManager, base.ActorCompanyId, base.UserId);
            return InExchangeConnector.GetBuyerCompany(releaseMode, base.ActorCompanyId, model);
        }
    }
}

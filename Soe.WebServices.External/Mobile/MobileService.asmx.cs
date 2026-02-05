using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Mobile;
using SoftOne.Soe.Business.Core.Mobile.Objects;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Services;

namespace Soe.WebServices.External.Mobile
{

    [WebService(Description = "Mobile Service", Namespace = "http://xe.softone.se/soe/WebServices/External/Mobile")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class MobileService : WebserviceBase
    {
        #region Constants

        private const string VALIDATION_INCORRECT_PARAMETER_DATATYPE = "Incorrect parameter datatype";
        //private bool authorize = false;

        #endregion

        #region ctor

        public MobileService()
        {
        }

        #endregion

        #region Common

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        #region Delegate

        [WebMethod(Description = "", EnableSession = false)]
        public string SearchTargetUserForDelegation(int type, string cultureCode, string version, int userId, int roleId, int companyId, string userCondition)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SearchTargetUserForDelegation(param, userCondition).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetUserDelegateHistory(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetUserDelegateHistory(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAttestRoleUserDelegation(int type, string cultureCode, string version, int userId, int roleId, int companyId, int targetUserId, int attestRoleUserId, DateTime dateFrom, DateTime dateTo)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveAttestRoleUserDelegation(param, targetUserId, attestRoleUserId, dateFrom, dateTo).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteUserCompanyRoleDelegation(int type, string cultureCode, string version, int userId, int roleId, int companyId, int headId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.DeleteUserCompanyRoleDelegation(param, headId).ToString();
        }

        #endregion


        #region Read

        [WebMethod(Description = "Get softoneproducts", EnableSession = false)]
        public string GetSoftOneProducts(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version, false);
            return m.GetSoftOneProducts(param).ToString();
        }

        [WebMethod(Description = "Login. RETURN: <Login><UserId></UserId><RoleId></RoleId><CompanyId></CompanyId></Login> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string Login(int type, string cultureCode, string version, string licenseNr, string loginName, string password)
        {
            MobileManager m = new MobileManager(GetParameterObject(), type, cultureCode, version);
            var result = m.Login(version, licenseNr, loginName, password, out int userSessionId).ToString();

            AddBrowserInfo(userSessionId);
            return result;
        }

        private void AddBrowserInfo(int userSessionId)
        {
            if (userSessionId == 0)
                return;

            using (CompEntities entities = new CompEntities())
            {
                var userSession = entities.UserSession.FirstOrDefault(f => f.UserSessonId == userSessionId);

                if (userSession != null && System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                {
                    var bc = System.Web.HttpContext.Current.Request.Browser;
                    if (bc != null)
                    {
                        //Browser
                        userSession.Browser += bc.Browser + " ";
                        userSession.Browser += bc.Version + " ";
                        if (!bc.Cookies)
                            userSession.Browser += "Cookies:0" + " ";
                        if (!bc.SupportsCss)
                            userSession.Browser += "CSS:0" + " ";
                        if (bc.Beta)
                            userSession.Screen += "Beta:1" + " ";

                        //Platform
                        userSession.Platform += bc.Platform + " ";
                        if (bc.Win16)
                            userSession.Platform += "Win16:1" + " ";
                        else if (bc.Win32)
                            userSession.Platform += "Win32:1" + " ";

                        //ClientIP
                        userSession.ClientIP += GetClientIP();

                        //Host
                        userSession.Host += GetHostIP() + " ";
                        userSession.Host += GetHostName();

                        //CacheCredentials
                        userSession.CacheCredentials += GetUserEnvironmentInfo();
                        entities.SaveChanges();
                    }
                }

                entities.SaveChanges();
            }
        }


        [WebMethod(Description = "Login. RETURN: <Login><UserId></UserId><RoleId></RoleId><CompanyId></CompanyId></Login> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string ValidateLogin(int type, string cultureCode, string version, int userId, int companyId, string loginName, string password)
        {
            MobileManager m = new MobileManager(GetParameterObject(userId: userId), type, cultureCode, version);
            Thread.Sleep(500);
            var result = m.Login(version, "", loginName, password, out int userSessionId).ToString();

            AddBrowserInfo(userSessionId);
            return result;

        }

        [WebMethod(Description = "Login. RETURN: <Login><UserId></UserId><RoleId></RoleId><CompanyId></CompanyId></Login> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string LoginNew(int type, string cultureCode, string version)
        {
            try
            {
                var parameterObject = GetParameterObject();
                MobileManager m = new MobileManager(parameterObject, type, cultureCode, version);
                var result = m.Login(version, parameterObject.UserId, out int userSessionId).ToString();

                AddBrowserInfo(userSessionId);
                return result;
            }
            catch (Exception ex)
            {
                SysServiceManager ssm = new SysServiceManager(null);
                ssm.LogError("LoginNew " + ex.ToString());
            }

            return MobileMessages.GetErrorMessageDocument("Error in request").ToString();
        }

        [WebMethod(Description = "Login. RETURN: <Success>1/0</Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string Startup(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            var result = m.Startup(param, out int userSessionId).ToString();

            AddBrowserInfo(userSessionId);
            return result;
        }

        #endregion

        #region Modify

        [WebMethod(Description = "Logout. RETURN: <Success>1/0</Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string Logout(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            ParameterObject parameterObject = GetParameterObject(companyId, userId, roleId);
            MobileManager m = new MobileManager(parameterObject, type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            if (parameterObject?.IdLoginGuid != null)
                BusinessMemoryCache<User>.Delete("GetUserForMobileLogin" + parameterObject.IdLoginGuid.ToString());

            return m.Logout(param).ToString();
        }

        [WebMethod(Description = "Register a device for push notifications. RETURN: <Success>1/0</Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string RegisterDeviceForNotifications(int type, string cultureCode, string version, int userId, int roleId, int companyId, string pushToken, string installationId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.RegisterDeviceForNotifications(param, pushToken, installationId).ToString();
        }

        #endregion

        #endregion

        #region Billing

        #region Read

        [WebMethod(Description = "Search customers for company. RETURN (top 50): <Customers><Customer><CustomerId></CustomerId><CustomerNr></CustomerNr><Name></Name></Customer></Customers> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SearchCustomers(int type, string cultureCode, string version, int userId, int roleId, int companyId, string search)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SearchCustomers(param, search).ToString();
        }

        [WebMethod(Description = "Search products for company. RETURN (top 50): <Products><Product><ProductId></ProductId><ProductNr></ProductNr><Name></Name><Price></Price></Product></Products> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SearchProducts(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string search)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SearchProducts(param, orderId, search).ToString();
        }

        [WebMethod(Description = "Search external products for company. RETURN (top 50): <SysProducts><SysProduct><SysProductId></SysProductId><ProductNr></ProductNr><Name></Name></SysProduct></SysProducts> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SearchExternalProducts(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string search)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SearchExternalProducts(param, orderId, search).ToString();
        }

        [WebMethod(Description = "Search external product prices for company.: <SysProductPrices><SysProductPrice><SysProductId></SysProductId><Wholeseller></Wholeseller><WholesellerId></ProductId><GNP></GNP><NettoNettoPrice></NettoNettoPrice><CustomerPrice></CustomerPrice><MarginalIncome></MarginalIncome><MarginalIncomeRatio></MarginalIncomeRatio>", EnableSession = false)]
        public string SearchExternalProductPrices(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int sysProductId, string productNr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SearchExternalProductPrices(param, orderId, sysProductId, productNr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInvoiceProductFromSys(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int sysProductId, int sysWholeSellerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetInvoiceProductFromSys(param, orderId, sysProductId, sysWholeSellerId).ToString();
        }

        [WebMethod(Description = "Get orderrows for specific order. RETURN: <OrderRows><OrderRow><OrderRowId></OrderRowId><ProductId></ProductId><ProductNr></ProductNr><ProductName></ProductName><Amount></Amount><Text></Text><Amount></Amount></OrderRow></OrderRows> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string GetOrderRows(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, bool showAll)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderRows(param, orderId, showAll).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderRowExternalUrl(int type, string cultureCode, string version, int userId, int roleId, int companyId, int productId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderRowExternalUrl(param, productId).ToString();
        }

        [WebMethod(Description = "Get timerows for specific order. RETURN: <TimeRows><TimeRow><Date></Date><InvoiceTimeInMinutes></InvoiceTimeInMinutes><WorkTimeInMinutes></WorkTimeInMinutes><Note></Note></TimeRow></TimeRows> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string GetTimeRows(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParam = GetUserParametersObject();
            var param = new MobileParam(userId, roleId, companyId, version, userParam == null ? MobileDeviceType.Unknown : userParam.MobileDeviceType);
            return m.GetTimeRows(param, orderId).ToString();
        }

        #endregion

        #region Modify

        [WebMethod(Description = "Add new or update existing order. RETURN: <OrderId></OrderId> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SaveOrder(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int customerId, string internalText, string label, string headText, string ourReference, string vatTypeId, string priceListId, string currencyId, string deliveryAddressId, string billingAddressId, string wholeSellerId, string yourReference, string projectId, int templateId, string deliveryDate, int orderTypeId, string workDescription)
        {
            //TODO: Add input parameter for discardConcurrencyCheck
            bool discardConcurrencyCheck = true;

            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            #region Parse String to Int

            if (string.IsNullOrEmpty(vatTypeId))
                vatTypeId = "0";
            if (string.IsNullOrEmpty(priceListId))
                priceListId = "0";
            if (string.IsNullOrEmpty(currencyId))
                currencyId = "0";
            if (string.IsNullOrEmpty(deliveryAddressId))
                deliveryAddressId = "0";
            if (string.IsNullOrEmpty(billingAddressId))
                billingAddressId = "0";
            if (string.IsNullOrEmpty(wholeSellerId))
                wholeSellerId = "0";
            if (string.IsNullOrEmpty(projectId))
                projectId = "0";

            if (!Int32.TryParse(vatTypeId, out int vatTypeIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(priceListId, out int priceListIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(currencyId, out int currencyIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(deliveryAddressId, out int deliveryAddressIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(billingAddressId, out int billingAddressIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(wholeSellerId, out int wholeSellerInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(projectId, out int projectIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;

            #endregion

            return m.SaveOrder(param, orderId, customerId, internalText, label, headText, ourReference, vatTypeIdInt, priceListIdInt, currencyIdInt, deliveryAddressIdInt, billingAddressIdInt, wholeSellerInt, yourReference, projectIdInt, templateId, deliveryDate, orderTypeId, workDescription, discardConcurrencyCheck).ToString();
        }

        [WebMethod(Description = "Set user ready state for order. RETURN: <Success>1/0</Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SetOrderUserIsReady(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SetOrderUserIsReady(param, orderId).ToString();
        }

        [WebMethod(Description = "Set existing order to ready for invoice. RETURN: <Success>1/0</Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SetOrderReady(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SetOrderReady(param, orderId).ToString();
        }

        [WebMethod(Description = "Add new or update existing orderrow. RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SaveOrderRow(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int orderRowId, int productId, int sysProductId, int sysWholesellerId, decimal quantity, decimal amount, string text, decimal quantityToInvoice, bool finalDelivery, string deliveryNote, string accounts, int stockId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            //quantityToInvoice, finalDelivery and deliveryNote are only for professional for the moment
            //See workitem: 9993

            //accounts is only for professional for the moment
            //See workitem 10283

            return m.SaveOrderRow(param, orderId, orderRowId, productId, sysProductId, sysWholesellerId, quantity, amount, text, quantityToInvoice, stockId).ToString();
        }

        [WebMethod(Description = "Add new or update existing timerow. RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SaveTimeRow(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int dayId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveTimeRow(param, orderId, dayId, date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId).ToString();
        }

        [WebMethod(Description = "Delete existing orderrow. RETURN: <Success>1/0</Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string DeleteOrderRow(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int orderRowId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.DeleteOrderRow(param, orderId, orderRowId).ToString();
        }

        [WebMethod(Description = "Delete existing timerow and add new timerow. RETURN: <Success>1/0</Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string DeleteTimeRowAndSaveNew(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int orderRowId, int dayId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.DeleteTimeRowAndSaveNew(param, orderId, dayId, date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId).ToString();
        }


        #region Household

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveHouseholdDeduction(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int orderRowId, string propertyLabel, string socialSecNbr, string name, decimal amount, string apartmentNbr, string cooperativeOrgNbr, bool isHDRut, int mobileDeductionType = 0)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveHouseholdDeduction(param, orderId, orderRowId, propertyLabel, socialSecNbr, name, amount, apartmentNbr, cooperativeOrgNbr, isHDRut, mobileDeductionType).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetHouseholdDeduction(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int orderRowId, bool isHDRut, int mobileDeductionType = 0)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetHouseholdDeduction(param, orderId, orderRowId, isHDRut, mobileDeductionType).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetHouseholdDeductionApplicants(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetHouseholdDeductionApplicants(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetHouseholdDeductionApplicantsForCustomer(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetHouseholdDeductionApplicantsForCustomer(param, customerId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveHouseholdDeductionApplicantsForCustomer(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId, int hdApplicantId, string name, string socSecNr, string property, string apartmentNr, string coopOrgNr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveHouseholdDeductionApplicantsForCustomer(param, customerId, hdApplicantId, name, socSecNr, property, apartmentNr, coopOrgNr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteHouseholdDeductionApplicantsForCustomer(int type, string cultureCode, string version, int userId, int roleId, int companyId, int hdApplicantId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.DeleteHouseholdDeductionApplicantForCustomer(param, hdApplicantId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetHouseholdDeductionTypes(int type, string cultureCode, string version, int userId, int roleId, int companyId, bool incRot, bool incRut, bool incGreenTech)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetHouseholdDeductionTypes(param, incRot, incRut, incGreenTech).ToString();
        }

        #endregion

        #endregion

        #endregion

        #region Time

        #region Read

        [WebMethod(Description = "RETURN: <Employees><Employee><EmployeeId></EmployeeId><EmployeeName></EmployeeName><EmployeeNr></EmployeeNr><EmployeeGroupId></EmployeeGroupId> </Employee></Employees>", EnableSession = false)]
        public string GetEmployee(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetEmployee(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAttestTreeEmployeesForAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime dateFrom, DateTime dateTo, bool includeAdditionalEmployees, bool includeIsAttested, string employeeIdsStr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetAttestTreeEmployeesForAdmin(param, dateFrom, dateTo, includeAdditionalEmployees, includeIsAttested, employeeIdsStr).ToString();
        }

        [WebMethod(Description = "RETURN: <TimePeriods><TimePeriod><TimePeriodId></TimePeriodId><DateStart></DateStart><DateStop></DateStop><ShowAsDefault></ShowAsDefault></TimePeriod></TimePeriods>", EnableSession = false)]
        public string GetPayrollPeriods(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetPayrollPeriods(param, employeeId, MobileDisplayMode.User).ToString();
        }

        [WebMethod(Description = "RETURN: <TimePeriods><TimePeriod><TimePeriodId></TimePeriodId><DateStart></DateStart><DateStop></DateStop><ShowAsDefault></ShowAsDefault></TimePeriod></TimePeriods>", EnableSession = false)]
        public string GetPayrollPeriodsAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetPayrollPeriods(param, 0, MobileDisplayMode.Admin).ToString();
        }

        //TODO: AttestEmployeeDay
        [WebMethod(Description = "RETURN: <AttestEmployeeItems><AttestEmployeeItem><SchedulePeriodId></SchedulePeriodId>"
            + "<Start></Start><Stop></Stop><BreakInMin></BreakInMin><WorkTime></WorkTime><IsReadOnly></IsReadOnly>"
            + "<AttestState></AttestState></AttestEmployeeItem></AttestEmployeeItems>", EnableSession = false)]
        public string GetAttestEmployeeItems(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int timePeriodId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetAttestEmployeeDays(param, employeeId, employeeGroupId, timePeriodId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAttestEmployeePeriods(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime dateFrom, DateTime dateTo, bool includeAdditionalEmployees, string employeeIds = "")
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetAttestEmployeePeriods(param, dateFrom, dateTo, includeAdditionalEmployees, employeeIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAttestEmployeeDaysAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime dateFrom, DateTime dateTo, int employeeId, string filterAccountIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetAttestEmployeeDaysAdmin(param, dateFrom, dateTo, employeeId, filterAccountIds).ToString();
        }
        [WebMethod(Description = "", EnableSession = false)]
        public string GetAttestInfoMessage(int type, string cultureCode, string version, int userId, int roleId, int companyId, string soeAttestInfoIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetAttestInfoMessage(param, soeAttestInfoIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAttestWarningMessage(int type, string cultureCode, string version, int userId, int roleId, int companyId, string soeAttestWarningIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetAttestWarningMessage(param, soeAttestWarningIds).ToString();
        }

        [WebMethod(Description = "RETURN: <DayView><SchedulePeriodId></SchedulePeriodId><Date></Date>"
            + "<ScheduleStart></ScheduleStart><ScheduleStop></ScheduleStop><ScheduleBreakInMin></ScheduleBreakInMin><ScheduleTime></ScheduleTime>"
            + "<ActualStart></ActualStart><ActualStop></ActualStop><ActualBreakInMin></ActualBreakInMin><ActualTime></ActualTime>"
            + "<FirstTimeBlockId></FirstTimeBlockId><LastTimeBlockId></LastTimeBlockId><FirstTimeBlockDeviationCauseId></FirstTimeBlockDeviationCauseId><LastTimeBlockDeviationCauseId></LastTimeBlockDeviationCauseId>"
            + "</DayView>", EnableSession = false)]
        public string GetDayView(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int schedulePeriodId, DateTime date)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetDayView(param, date, employeeId, employeeGroupId, schedulePeriodId, companyId).ToString();
        }
        [WebMethod(Description = "", EnableSession = false)]
        public string GetMyTimeOverview(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetMyTimeOverview(param, employeeId, dateFrom, dateTo).ToString();
        }

        [WebMethod(Description = "RETURN: <DeviationCauses><DeviationCause><DeviationCauseId></DeviationCauseId><DeviationCauseName></DeviationCauseName><IsPresence></IsPresence><IsAbsence></IsAbsence></DeviationCause></DeviationCauses>", EnableSession = false)]
        public string GetDeviationCauses(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParam = GetUserParametersObject();

            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParam == null ? MobileDeviceType.Unknown : userParam.MobileDeviceType);
            return m.GetDeviationCauses(param, employeeId, employeeGroupId).ToString();
        }

        [WebMethod(Description = "RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string GetBreaks(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int schedulePeriodId, DateTime date)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetBreaks(param, date, employeeId, schedulePeriodId, companyId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTimePermissions(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetTimePermissions(param, MobileDisplayMode.User).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTimePermissionsAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetTimePermissions(param, MobileDisplayMode.Admin).ToString();
        }

        #endregion

        #region Modify

        [WebMethod(Description = "RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SaveWholeDayAbsence(int type, string cultureCode, string version, int userId, int roleId, int companyId, string dates, int deviationCauseId, int employeeId, int employeeChildId, string comment)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveWholeDayAbsence(param, dates, deviationCauseId, employeeId, employeeChildId, comment).ToString();
        }

        [WebMethod(Description = "RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SaveIntervalAbsence(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime start, DateTime stop, DateTime displayedDate, int schedulePeriodId, int deviationCauseId, int employeeId, string comment, int employeeChildId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveIntervalAbsence(param, start, stop, displayedDate, schedulePeriodId, deviationCauseId, employeeId, comment, employeeChildId).ToString();
        }

        [WebMethod(Description = "RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SaveIntervalPresence(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime start, DateTime stop, DateTime displayedDate, int schedulePeriodId, int deviationCauseId, int employeeId, string comment)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveIntervalPresence(param, start, stop, displayedDate, schedulePeriodId, deviationCauseId, employeeId, comment).ToString();
        }

        [WebMethod(Description = "RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SaveStopTime(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime newStopTime, DateTime date, int schedulePeriodId, int newDeviationCauseId, int timeBlockId, int employeeId, string comment, int employeeChildId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveStopTime(param, newStopTime, date, schedulePeriodId, newDeviationCauseId, timeBlockId, employeeId, comment, employeeChildId).ToString();
        }

        [WebMethod(Description = "RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SaveStartTime(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime newStartTime, DateTime date, int schedulePeriodId, int newDeviationCauseId, int timeBlockId, int employeeId, string comment, int employeeChildId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveStartTime(param, newStartTime, date, schedulePeriodId, newDeviationCauseId, timeBlockId, employeeId, comment, employeeChildId).ToString();
        }

        [WebMethod(Description = "RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string ModifyBreak(int type, string cultureCode, string version, int userId, int roleId, int companyId, int scheduleBlockId, int employeeId, int employeeGroupId, int schedulePeriodId, int timeCodeBreakId, int totalMinutes, DateTime date)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.ModifyBreak(param, date, totalMinutes, scheduleBlockId, employeeId, schedulePeriodId, timeCodeBreakId, companyId).ToString();
        }

        [WebMethod(Description = "RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SetSchedulePeriodAsReady(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, string idsAndDates, int attestStateToId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SetSchedulePeriodAsReady(param, employeeId, employeeGroupId, idsAndDates, attestStateToId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAttestForPeriod(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveAttestForPeriod(param, employeeId, dateFrom, dateTo, 0).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAttestForEmployees(int type, string cultureCode, string version, int userId, int roleId, int companyId, string employeeIdsStr, int attestStateToId, DateTime dateFrom, DateTime dateTo)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveAttestForEmployees(param, employeeIdsStr, attestStateToId, dateFrom, dateTo).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAttestForEmployeeByAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, string idsAndDates, int attestStateToId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveAttestForEmployee(param, employeeId, idsAndDates, attestStateToId, MobileDisplayMode.Admin).ToString();
        }

        [WebMethod(Description = "RETURN: <Success></Success> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string RestoreToSchedule(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int schedulePeriodId, DateTime date)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.RestoreToSchedule(param, date, employeeId, employeeGroupId, schedulePeriodId, companyId).ToString();
        }

        #endregion

        #endregion

        #region New in Version 3

        [WebMethod(Description = "", EnableSession = false)]
        public string GetNextCustomerNr(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetNextCustomerNr(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCustomerEdit(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParams = GetUserParametersObject();
            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParams == null ? MobileDeviceType.Unknown : userParams.MobileDeviceType);
            return m.GetCustomerEdit(param, customerId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCustomersGrid(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCustomersGrid(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetPaymentConditions(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetPaymentConditions(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderTypes(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderTypes(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetVatTypes(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetVatTypes(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCurrencies(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCurrencies(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetPriceLists(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetPriceLists(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetWholeSellers(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetWholeSellers(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetContactPersons(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetContactPersons(param, customerId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetDeliveryAddress(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetDeliveryAddress(param, customerId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInvoiceAddress(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetInvoiceAddress(param, customerId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetImage(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string imageId, bool isFile)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            if (string.IsNullOrEmpty(imageId))
                imageId = "0";

            if (!Int32.TryParse(imageId, out int imageIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;

            return m.GetImage(param, orderId, imageIdInt, isFile).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetDocuments(int type, string cultureCode, string version, int userId, int roleId, int companyId, int recordId, int entityType, string documentTypes, bool ignoreFileTypes = false)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetDocuments(param, recordId, entityType, documentTypes, ignoreFileTypes).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderThumbNails(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderThumbNails(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderThumbNailsOnChecklist(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int checklistHeadId, int checklistHeadRecordId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderThumbNailsOnChecklist(param, orderId, checklistHeadId, checklistHeadRecordId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AddDocument(int type, string cultureCode, string version, int userId, int roleId, int companyId, int recordId, int entityType, int documentType, byte[] imageData, byte[] data, string description, string fileName)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            // The app has added support to send any filetype, not just images
            // With this update, the app will send data in the data parameter and fileName will include extension
            // imageData and updateExtension can be removed when the app is fully updated to always use data parameter and fileName
            bool updateExtension = imageData != null;

            return m.AddDocument(param, recordId, entityType, documentType, data ?? imageData, description, fileName, updateExtension).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AddImage(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, byte[] imageData, String description, int imageType, String fileName)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.AddImage(param, orderId, imageData, description, imageType, fileName).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AddChecklistImage(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int checklistHeadId, int checklistHeadRecordId, byte[] imageData, String description, int imageType)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.AddChecklistImage(param, orderId, checklistHeadId, checklistHeadRecordId, imageData, description, imageType).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteImage(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string imageId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            if (string.IsNullOrEmpty(imageId))
                imageId = "0";

            if (!Int32.TryParse(imageId, out int imageIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;

            return m.DeleteImage(param, orderId, imageIdInt).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteDocument(int type, string cultureCode, string version, int userId, int roleId, int companyId, int recordId, int documentId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.DeleteDocument(param, recordId, documentId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string EditImage(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string imageId, string description, string fileName)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            if (string.IsNullOrEmpty(imageId))
                imageId = "0";

            if (!Int32.TryParse(imageId, out int imageIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;

            return m.EditImage(param, orderId, imageIdInt, description, fileName).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetChecklists(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetChecklists(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetUnUsedChecklists(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetUnUsedChecklists(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AddCheckList(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string checkListHeadIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.AddChecklist(param, orderId, checkListHeadIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetChecklistContent(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int checklistHeadId, int checklistHeadRecordId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetChecklistContent(param, orderId, checklistHeadId, checklistHeadRecordId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveCheckListAnswers(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int checklistHeadId, int checklistHeadRecordId, string answers)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveCheckListAnswers(param, orderId, checklistHeadId, checklistHeadRecordId, answers).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderUsers(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderUsers(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderCurrentUsers(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderCurrentUsers(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveOrderUsers(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string userIds, int mainUserId, bool sendMail)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveOrderUsers(param, orderId, userIds, mainUserId, sendMail).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveCustomer(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId, string customerNr, string name, string orgNr, string vatNr, string reference, string vatTypeId, string paymentConditionId, string salesPriceListid, string stdWholeSellerId, decimal disccountArticles, decimal disccountServices, string currencyId, string note, string emailAddressId, string emailAddress, string homePhoneId, string homePhone, string jobPhoneId, string jobPhone, string mobilePhoneId, string mobilePhone, string faxId, string fax, string invoiceAddressId, string invoiceAddress, string invoiceAddressPostalCode, string invoiceAddressPostalAddress, string invoiceAddressCountry, string invoiceAddressAddressCO, string deliveryAddressId1, string deliveryAddress1, string deliveryAddress1PostalCode, string deliveryAddress1PostalAddress, string deliveryAddress1Country, string deliveryAddress1AddressCO, string deliveryAddress1Name, int invoiceDeliveryTypeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            #region Parse String to Int

            if (string.IsNullOrEmpty(vatTypeId))
                vatTypeId = "0";
            if (string.IsNullOrEmpty(paymentConditionId))
                paymentConditionId = "0";
            if (string.IsNullOrEmpty(salesPriceListid))
                salesPriceListid = "0";
            if (string.IsNullOrEmpty(stdWholeSellerId))
                stdWholeSellerId = "0";
            if (string.IsNullOrEmpty(currencyId))
                currencyId = "0";
            if (string.IsNullOrEmpty(emailAddressId))
                emailAddressId = "0";
            if (string.IsNullOrEmpty(homePhoneId))
                homePhoneId = "0";
            if (string.IsNullOrEmpty(jobPhoneId))
                jobPhoneId = "0";
            if (string.IsNullOrEmpty(mobilePhoneId))
                mobilePhoneId = "0";
            if (string.IsNullOrEmpty(faxId))
                faxId = "0";
            if (string.IsNullOrEmpty(deliveryAddressId1))
                deliveryAddressId1 = "0";
            if (string.IsNullOrEmpty(invoiceAddressId))
                invoiceAddressId = "0";

            if (!Int32.TryParse(vatTypeId, out int vatTypeIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(paymentConditionId, out int paymentConditionIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(salesPriceListid, out int salesPriceListidInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(stdWholeSellerId, out int stdWholeSellerIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(currencyId, out int currencyIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(emailAddressId, out int emailAddressIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(homePhoneId, out int homePhoneIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(jobPhoneId, out int jobPhoneIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(mobilePhoneId, out int mobilePhoneIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(faxId, out int faxIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(deliveryAddressId1, out int deliveryAddressId1Int))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(invoiceAddressId, out int invoiceAddressIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;

            #endregion

            return m.SaveCustomer(param, customerId, customerNr, name, orgNr, vatNr, reference, vatTypeIdInt, paymentConditionIdInt, salesPriceListidInt, stdWholeSellerIdInt, disccountArticles, disccountServices, currencyIdInt, note, emailAddressIdInt, emailAddress, homePhoneIdInt, homePhone, jobPhoneIdInt, jobPhone, mobilePhoneIdInt, mobilePhone, faxIdInt, fax,
                invoiceAddressIdInt, invoiceAddress, invoiceAddressPostalCode, invoiceAddressPostalAddress, invoiceAddressCountry, invoiceAddressAddressCO, deliveryAddressId1Int, deliveryAddress1, deliveryAddress1PostalCode, deliveryAddress1PostalAddress, deliveryAddress1Country, deliveryAddress1AddressCO, deliveryAddress1Name, invoiceDeliveryTypeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderEdit(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParams = GetUserParametersObject();

            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParams == null ? MobileDeviceType.Unknown : userParams.MobileDeviceType);
            return m.GetOrderEdit(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderTemplateInfo(int type, string cultureCode, string version, int userId, int roleId, int companyId, int templateId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParams = GetUserParametersObject();

            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParams == null ? MobileDeviceType.Unknown : userParams.MobileDeviceType);
            return m.GetOrderTemplateInfo(param, templateId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrdersGrid(int type, string cultureCode, string version, int userId, int roleId, int companyId, bool hideStatusOrderReady = false, bool showMyOrders = false, bool hideUserStateReady = false)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrdersGrid(param, hideStatusOrderReady, showMyOrders, hideUserStateReady).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveMapLocation(int type, string cultureCode, string version, int userId, int roleId, int companyId, Decimal longitude, Decimal latitude, String description)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            DateTime timestamp = DateTime.Now;
            return m.SaveMapLocation(param, longitude, latitude, description, timestamp).ToString();
        }

        #region Field Settings

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderGridFieldSettings(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderGridFieldSettings(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderEditFieldSettings(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderEditFieldSettings(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCustomerGridFieldSettings(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCustomerGridFieldSettings(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCustomerEditFieldSettings(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCustomerEditFieldSettings(param).ToString();
        }

        #endregion

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTimeProjectTimeCodes(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParam = GetUserParametersObject();

            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParam == null ? MobileDeviceType.Unknown : userParam.MobileDeviceType);

            return m.GetTimeProjectTimeCodes(param).ToString();
        }

        #endregion

        #region New in version 4

        [WebMethod(Description = "", EnableSession = false)]
        public string GetModules(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetModules(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetModulesCounter(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime? lastFetchDate = null)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetModulesCounter(param, lastFetchDate).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetShiftAsWanted(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetShiftAsWanted(param, employeeId, employeeGroupId, shiftId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string UpdateShiftSetUndoWanted(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.UpdateShiftSetUndoWanted(param, employeeId, employeeGroupId, shiftId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetShiftAsUnWanted(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetShiftAsUnWanted(param, employeeId, employeeGroupId, shiftId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string UpdateShiftSetUndoUnWanted(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.UpdateShiftSetUndoUnWanted(param, employeeId, employeeGroupId, shiftId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string WantShiftValidateSkills(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.WantShiftValidateSkills(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInterestRequests(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, string dateFrom, string dateTo)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            DateTime? dateFromParsed = null;
            if (!string.IsNullOrEmpty(dateFrom))
                dateFromParsed = CalendarUtility.GetNullableDateTime(dateFrom);

            DateTime? dateToParsed = null;
            if (!string.IsNullOrEmpty(dateTo))
                dateToParsed = CalendarUtility.GetNullableDateTime(dateTo);

            return m.GetInterestRequests(param, employeeId, dateFromParsed, dateToParsed).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInterestRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int interestRequestId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetInterestRequest(param, interestRequestId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveInterestRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int interestRequestId, DateTime dateFrom, DateTime dateTo, bool available, string note, bool isAdmin)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveInterestRequest(param, employeeId, interestRequestId, dateFrom, dateTo, available, note, isAdmin).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteInterestRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, int interestRequestId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.DeleteInterestRequest(param, employeeId, employeeGroupId, interestRequestId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAbsenceRequestCauses(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAbsenceRequestCauses(param, employeeId, employeeGroupId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAbsenceRequests(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAbsenceRequests(param, employeeId, dateFrom, dateTo).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAbsenceRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int absenceRequestId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAbsenceRequest(param, absenceRequestId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAbsenceRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int absenceRequestId, DateTime dateFrom, DateTime dateTo, int deviationCauseId, string note, bool wholeDays, int employeeChildId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveAbsenceRequest(param, employeeId, absenceRequestId, dateFrom, dateTo, deviationCauseId, note, wholeDays, employeeChildId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteAbsenceRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int absenceRequestId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.DeleteAbsenceRequest(param, absenceRequestId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInboxMail(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetInboxMail(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSentMail(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetSentMail(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetMail(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetMail(param, mailId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SendMail(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId, int parentMailId, string subject, string text,
            string userIds, string roleIds, string employeeGroupIds, string categoryIds, string messageGroupIds, byte[] imageData, string imageName, bool forward)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SendMail(param, mailId, parentMailId, subject, text, userIds, roleIds, employeeGroupIds, categoryIds, messageGroupIds, imageData, imageName, forward).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetMailAttachments(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetMailAttachments(param, mailId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetMailAttachment(int type, string cultureCode, string version, int userId, int roleId, int companyId, int attachmentId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetMailAttachment(param, attachmentId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteIncomingMail(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.DeleteIncomingMail(param, mailId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteOutgoingMail(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.DeleteOutgoingMail(param, mailId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AnswerShiftRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId, bool value)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AnswerShiftRequest(param, mailId, value).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string MarkMailAsRead(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.MarkMailAsRead(param, mailId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetReceivers(int type, string cultureCode, string version, int userId, int roleId, int companyId, string searchText)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetReceivers(param, searchText).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetReplyReceiver(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetReplyReceiver(param, mailId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetReplyAllReceivers(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetReplyAllReceivers(param, mailId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSalarySpecifications(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetSalarySpecifications(param, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSalarySpecification(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int salarySpecifikationId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetSalarySpecification(param, employeeId, salarySpecifikationId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetArchivedFiles(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetArchivedFiles(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetArchivedFile(int type, string cultureCode, string version, int userId, int roleId, int companyId, int archivedFileId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetArchivedFile(param, archivedFileId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetMyDocuments(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetMyDocuments(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAllInternalNews(int type, string cultureCode, string version, int userId, int roleId, int companyId, bool isStartPage)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAllInternalNews(param, isStartPage).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInternalNews(int type, string cultureCode, string version, int userId, int roleId, int companyId, int newsId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetInternalNews(param, newsId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInternalNewsAttachments(int type, string cultureCode, string version, int userId, int roleId, int companyId, int newsId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return null;
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInternalNewsAttachment(int type, string cultureCode, string version, int userId, int roleId, int companyId, int newsId, int attachmentId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return null;
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetEmployeeDetails(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetEmployeeDetails(param, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveEmployeeDetails(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId, string firstName, string lastName, string addressId, string address, string postalCode, string postalAddress, string closestRelativeId, string closestRelative, string mobileId, string mobile, string emailId, string email, string closestRelativeName, string closestRelativeRelation, bool closestRelativeIsSecret, string closestRelativeId2, string closestRelative2, string closestRelativeName2, string closestRelativeRelation2, bool closestRelativeIsSecret2)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            if (string.IsNullOrEmpty(addressId))
                addressId = "0";
            if (string.IsNullOrEmpty(mobileId))
                mobileId = "0";
            if (string.IsNullOrEmpty(emailId))
                emailId = "0";
            if (string.IsNullOrEmpty(closestRelativeId))
                closestRelativeId = "0";

            if (!Int32.TryParse(addressId, out int addressIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(mobileId, out int mobileIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(emailId, out int emailIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(closestRelativeId, out int closestRelativeIdInt))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;
            if (!Int32.TryParse(closestRelativeId2, out int closestRelativeIdInt2))
                return VALIDATION_INCORRECT_PARAMETER_DATATYPE;

            return m.SaveEmployeeDetails(param, employeeId, firstName, lastName, addressIdInt, address, postalCode, postalAddress, closestRelativeIdInt, closestRelative, closestRelativeName, closestRelativeRelation, closestRelativeIsSecret, closestRelativeIdInt2, closestRelative2, closestRelativeName2, closestRelativeRelation2, closestRelativeIsSecret2, mobileIdInt, mobile, emailIdInt, email).ToString();
        }

        //[WebMethod(Description = "", EnableSession = false)]
        //public string SetOrderRowsAsReady(int type, string cultureCode, int userId, int roleId, int companyId, string orderRowIds)
        //{
        //    MobileManager m = new MobileManager(GetParameterObject(companyId, userId), type, cultureCode, version);
        //    MobileParam param = new MobileParam(userId, roleId, companyId);

        //    return null;
        //}        

        [WebMethod(Description = "", EnableSession = false)]
        public string ChangeOrderCustomer(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return null;
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetEmployeeList(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetEmployeeList(param).ToString();
        }

        #endregion

        #region New in Version 6

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderWorkDescription(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParam = GetUserParametersObject();
            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParam == null ? MobileDeviceType.Unknown : userParam.MobileDeviceType);
            return m.GetOrderWorkDescription(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveOrderWorkDescription(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string workDesc)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveOrderWorkDescription(param, orderId, workDesc).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCustomerReferences(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCustomerReferences(param, customerId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCompaniesForUser(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCompaniesForUser(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetRolesForUserCompany(int type, string cultureCode, string version, int userId, int roleId, int companyId, int rolesForCompanyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetRolesForUserCompany(param, rolesForCompanyId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string ValidateUserCompanyRole(int type, string cultureCode, string version, int userId, int roleId, int companyId, int newRoleId, int newCompanyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.ValidateUserCompanyRole(param, newRoleId, newCompanyId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetOrderRowsAsReady(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string orderRowIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SetOrderRowsAsReady(param, orderId, orderRowIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetProjectsBySearch(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId, string searchText)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetProjectsBySearch(param, customerId, searchText).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetProjectsBySearch2(int type, string cultureCode, string version, int userId, int roleId, int companyId, string number, string name, string customerNr, string customerName, string managerName, string orderNr, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetProjectsBySearch2(param, number, name, customerNr, customerName, managerName, orderNr, customerId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string ChangeProjectOnOrder(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int newProjectId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.ChangeProjectOnOrder(param, orderId, newProjectId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string PreCreateOrder(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.PreCreateOrder(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSupplierInvoicesAttestWorkFlowMyActive(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetSupplierInvoicesAttestWorkFlowMyActive(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSupplierInvoiceAttestWorkFlowView(int type, string cultureCode, string version, int userId, int roleId, int companyId, int invoiceId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetSupplierInvoiceAttestWorkFlowView(param, invoiceId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAttestWorkFlowAnswer(int type, string cultureCode, string version, int userId, int roleId, int companyId, int invoiceId, int attestWorkFlowHeadId, int attestWorkFlowRowId, bool answer, string comment)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveAttestWorkFlowAnswer(param, invoiceId, attestWorkFlowHeadId, attestWorkFlowRowId, answer, comment).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SupplierInvoiceBlockPayment(int type, string cultureCode, string version, int userId, int roleId, int companyId, int invoiceId, bool blockPayment, string comment)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SupplierInvoiceBlockPayment(param, invoiceId, blockPayment, comment).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteOrderChecklist(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string checkListHeadRecordIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.DeleteOrderChecklists(param, orderId, checkListHeadRecordIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetMultipleChoiceAnswerRows(int type, string cultureCode, string version, int userId, int roleId, int companyId, int multipleChoiceAnswerHeadId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetMultipleChoiceAnswerRows(param, multipleChoiceAnswerHeadId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTextBlockDictionary(int type, string cultureCode, string version, int userId, int roleId, int companyId, int dictionaryType)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetTextBlockDictionary(param, dictionaryType).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetStateAnalysis(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetStateAnalysis(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTimeSheetRows(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime date, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetTimeSheetRows(param, date, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTimeSheetInfo(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime date, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetTimeSheetInfo(param, date, employeeId).ToString();
        }

        [WebMethod(Description = "Search external products for company. RETURN (top 50): <SysProducts><SysProduct><SysProductId></SysProductId><ProductNr></ProductNr><Name></Name></SysProduct></SysProducts> OR <ErrorMessage></ErrorMessage>", EnableSession = false)]
        public string SearchExternalProductsDemo(int type, string cultureCode, string version, int userId, int roleId, int companyId, string search)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SearchExternalProductsDemo(param, search).ToString();
        }

        [WebMethod(Description = "Search external product prices for company.: <SysProductPrices><SysProductPrice><SysProductId></SysProductId><Wholeseller></Wholeseller><WholesellerId></ProductId><GNP></GNP><NettoNettoPrice></NettoNettoPrice><CustomerPrice></CustomerPrice><MarginalIncome></MarginalIncome><MarginalIncomeRatio></MarginalIncomeRatio>", EnableSession = false)]
        public string SearchExternalProductPricesDemo(int type, string cultureCode, string version, int userId, int roleId, int companyId, string productNr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SearchExternalProductPricesDemo(param, productNr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInstructionalVideos(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetInstructionalVideos(param).ToString();
        }


        [WebMethod(Description = "", EnableSession = false)]
        public string GetStaffingPermissions(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetStaffingPermissions(param, MobileDisplayMode.User).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetStaffingSettings(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetStaffingSettings(param, MobileDisplayMode.User).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetStaffingSettingsAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetStaffingSettings(param, MobileDisplayMode.Admin).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAbsenceAnnouncementCauses(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeGroupId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetAbsenceAnnouncementCauses(param, employeeId, employeeGroupId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAbsenceAnnouncement(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime date, int employeeId, int timeDeviationCauseId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveAbsenceAnnouncement(param, date, employeeId, timeDeviationCauseId).ToString();
        }

        #endregion

        #region New in Version 7

        [WebMethod(Description = "", EnableSession = false)]
        public string GetStaffingPermissionsAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetStaffingPermissions(param, MobileDisplayMode.Admin).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetShiftsAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, int employeeId, DateTime actualDate)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetShifts(param, shiftId, employeeId, actualDate).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetBreakTimeCodes(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, string dateStr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            DateTime? date = null;
            if (!string.IsNullOrEmpty(dateStr))
            {
                date = CalendarUtility.GetNullableDateTime(dateStr);
            }

            return m.GetBreakTimeCodes(param, employeeId, date).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetShiftTypesForEditShiftView(int type, string cultureCode, string version, int userId, int roleId, int companyId, string accountId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetShiftTypesForEditShiftView(param, accountId).ToString();
        }
        /// <summary>
        /// Deprecated since 2021-01-04, should not be used anymore. Is replaced by GetShiftTypesForEditShiftView.
        /// Can not be removed right now because of older apps 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cultureCode"></param>
        /// <param name="version"></param>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [WebMethod(Description = "", EnableSession = false)]
        public string GetShiftTypes(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetShiftTypesForEditShiftView(param, "").ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleTypes(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleTypes(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetEditShiftsViewSettings(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetEditShiftsViewSettings(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveShiftsValidateSkills(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime actualDate, string shifts)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveShiftsValidateSkills(param, employeeId, actualDate, shifts).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveShiftsValidateWorkRules(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime actualDate, string shifts)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveShiftsValidateWorkRules(param, employeeId, actualDate, shifts).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveShiftsAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime actualDate, string shifts)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveShifts(param, employeeId, actualDate, shifts).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetShiftRequestUsers(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetShiftRequestUsers(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SendShiftRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, int employeeId, string comment, string userIds, bool overrided, string overrideData)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SendShiftRequest(param, shiftId, employeeId, comment, userIds, overrided, overrideData).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SendShiftRequestValidateWorkRules(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, int employeeId, string userIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SendShiftRequestValidateWorkRules(param, shiftId, employeeId, userIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteShift(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, int employeeId, bool includeLinkedShifts)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.DeleteShift(param, shiftId, employeeId, includeLinkedShifts).ToString();
        }

        #region Accounting on orderrow (only Professional)

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAccountSettings(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAccountSettings(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAccounts(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAccounts(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderRowAccounts(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetOrderRowAccounts(param).ToString();
        }

        #endregion

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAbsencePlanningDeviationCauses(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, bool wholeDay)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAbsencePlanningDeviationCauses(param, employeeId, wholeDay).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetReplaceWithEmployees(int type, string cultureCode, string version, int userId, int roleId, int companyId, bool addSearchEmployee)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetReplaceWithEmployees(param, addSearchEmployee).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAbsenceAffectedShiftsOpenDialog(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int shiftId, DateTime date, bool wholeDay)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAbsenceAffectedShiftsOpenDialog(param, employeeId, shiftId, date, wholeDay).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAbsenceAffectedShifts(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int deviationCauseId, DateTime from, DateTime to, DateTime absenceFrom, DateTime absenceTo, bool wholeDay, bool isTimeModule)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAbsenceAffectedShifts(param, employeeId, deviationCauseId, from, to, absenceFrom, absenceTo, wholeDay, isTimeModule).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AbsencePlanningValidateWorkRules(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, string shifts, bool isTimeModule)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AbsencePlanningValidateWorkRules(param, employeeId, shifts, isTimeModule).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAbsencePlanningAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int timeDeviationCauseId, string shifts, int employeeChildId, bool isTimeModule)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveAbsencePlanning(param, employeeId, timeDeviationCauseId, shifts, employeeChildId, isTimeModule).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAbsencePlanningEmployee(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int timeDeviationCauseId, string shifts, int employeeChildId, bool isTimeModule)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveAbsencePlanning(param, employeeId, timeDeviationCauseId, shifts, employeeChildId, isTimeModule).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AbsencePlanningValidateSkills(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int timeDeviationCauseId, string shifts)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AbsencePlanningValidateSkills(param, employeeId, timeDeviationCauseId, shifts).ToString();
        }

        #endregion

        #region New in Version 8

        [WebMethod(Description = "", EnableSession = false)]
        public string CopyMoveOrderRows(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int newOrderId, string orderRowIdsAndQuantity, int actionType)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.CopyMoveOrderRows(param, orderId, newOrderId, orderRowIdsAndQuantity, actionType).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCopyMoveOrders(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCopyMoveOrders(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSettingsPermissions(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetSettingsPermissions(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetChangePWDPolicies(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetChangePWDPolicies(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string ChangePWD(int type, string cultureCode, string version, int userId, int roleId, int companyId, string oldPWD, string newPWD, string confirmNewPWD)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.ChangePWD(param, oldPWD, newPWD, confirmNewPWD).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTimeStampAttendance(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetTimeStampAttendance(param).ToString();
        }

        #endregion

        #region New in Version 10

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderTemplates(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetOrderInvoiceTemplates(param, companyId).ToString();
        }

        #endregion

        #region New in Version 11

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTimeRow(int type, string cultureCode, string version, int companyId, int userId, int roleId, int orderId, DateTime date, int timeCodeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParam = GetUserParametersObject();

            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParam == null ? MobileDeviceType.Unknown : userParam.MobileDeviceType);

            return m.GetTimeRow(param, orderId, date, timeCodeId).ToString();
        }

        //[WebMethod(Description = "", EnableSession = false)]
        //public string SaveOrResetTimeRow(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, int timeRowId, int dayId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId)
        //{
        //    MobileManager m = new MobileManager(GetParameterObject(companyId, userId), type, cultureCode, version);
        //    MobileParam param = new MobileParam(userId, roleId, companyId, version);
        //    return m.SaveOrResetTimeRow(param, orderId, timeRowId, dayId, date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId).ToString();
        //}

        #endregion

        #region New in version 12

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignAvailableShiftFromQueueValidateWorkRules(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignAvailableShiftFromQueueValidateWorkRules(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignAvailableShiftFromQueueValidateSkills(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignAvailableShiftFromQueueValidateSkills(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignAvailableShiftFromQueue(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignAvailableShiftFromQueue(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetShiftQueue(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetShiftQueue(param, shiftId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignAvailableShiftValidateWorkRules(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignAvailableShiftValidateWorkRules(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignAvailableShiftValidateSkills(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignAvailableShiftValidateSkills(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignAvailableShift(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignAvailableShift(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignOthersShiftValidateWorkRules(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignOthersShiftValidateWorkRules(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignOthersShiftValidateSkills(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignOthersShiftValidateSkills(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignOthersShift(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignOthersShift(param, shiftId, employeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetPlanningData(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetPlanningData(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderShiftTypes(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderShiftTypes(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderShiftOrders(int type, string cultureCode, string version, int userId, int roleId, int companyId, string anyDateInWeek)
        {
            DateTime? anyDateInWeekParsed = null;
            if (!string.IsNullOrEmpty(anyDateInWeek))
                anyDateInWeekParsed = CalendarUtility.GetNullableDateTime(anyDateInWeek);

            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderShiftOrders(param, anyDateInWeekParsed).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string ReloadOrderPlanningSchedule(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime date)
        {

            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.ReloadOrderPlanningSchedule(param, employeeId, date).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOrderShift(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, int orderId, int employeeId, DateTime date)
        {

            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetOrderShift(param, shiftId, orderId, employeeId, date).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveOrderShift(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, int orderId, int employeeId, DateTime startTime, DateTime stopTime, int shiftTypeId)
        {

            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveOrderShift(param, shiftId, orderId, employeeId, startTime, stopTime, shiftTypeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveOrderShiftValidateSkills(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime stopTime, int shiftTypeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveOrderShiftValidateSkills(param, employeeId, stopTime, shiftTypeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveOrderShiftValidateWorkRules(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, int orderId, int employeeId, DateTime startTime, DateTime stopTime, int shiftTypeId)
        {

            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveOrderShiftValidateWorkRules(param, shiftId, orderId, employeeId, startTime, stopTime, shiftTypeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SavePlanningData(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, string shiftTypeId, string plannedStartDate, string plannedStopDate, int estimatedTime, int remainingTime, int priority)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            int? shiftTypeParsed = null;
            if (!string.IsNullOrEmpty(shiftTypeId) && Int32.TryParse(shiftTypeId, out int i) && i > 0)
                shiftTypeParsed = i;

            DateTime? plannedStartDateParsed = null;
            if (!string.IsNullOrEmpty(plannedStartDate))
                plannedStartDateParsed = CalendarUtility.GetNullableDateTime(plannedStartDate);

            DateTime? plannedStopDateParsed = null;
            if (!string.IsNullOrEmpty(plannedStopDate))
                plannedStopDateParsed = CalendarUtility.GetNullableDateTime(plannedStopDate);

            // TODO: KeepAsPlanned not supported in mobile
            return m.SavePlanningData(param, orderId, shiftTypeParsed, plannedStartDateParsed, plannedStopDateParsed, estimatedTime, remainingTime, false, priority).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAttestStatesAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime dateFrom, DateTime dateTo, string attestStateIdsStr = "")
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAttestStates(param, 0, dateFrom, dateTo, MobileDisplayMode.Admin, attestStateIdsStr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAttestStates(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int timePeriodId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAttestStates(param, employeeId, timePeriodId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetSchedulePeriodAsReadyValidation(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int attestStateToId, string idsAndDates, bool standalone, string filterAccountIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetSchedulePeriodAsReadyValidation(param, employeeId, attestStateToId, idsAndDates, standalone, filterAccountIds).ToString();
        }

        #endregion

        #region New in verison 12 - GO

        [WebMethod(Description = "", EnableSession = false)]
        public string GetShiftTasks(int type, string cultureCode, string version, int companyId, int userId, int roleId, int shiftId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetShiftTasks(param, shiftId).ToString();
        }

        #endregion

        #region New in Version 13 (users with missing email)
        //changes where made in login and startup

        #endregion

        #region New in version 14 (ICA phase 1)

        [WebMethod(Description = "", EnableSession = false)]
        public string GetShiftFlow(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime dateFrom)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetShiftFlow(param, employeeId, dateFrom).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleDayViewAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime date, bool includeSecondaryCategories, string employeeIdsStr, string shiftTypeIdsStr, string accountIdsStr, bool includeUnscheduledEmployees)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleDayViewAdmin(param, date, includeSecondaryCategories, employeeIdsStr, shiftTypeIdsStr, string.IsNullOrEmpty(accountIdsStr) ? "" : accountIdsStr, includeUnscheduledEmployees).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTemplateScheduleView(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetTemplateScheduleViewEmployee(param, employeeId, dateFrom, dateTo).ToString();
        }
        [WebMethod(Description = "", EnableSession = false)]
        public string GetTemplateScheduleDay(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime date)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetTemplateScheduleDayEmployee(param, employeeId, date).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleView(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleViewEmployee(param, employeeId, dateFrom, dateTo).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleDay(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime date)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleDayEmployee(param, employeeId, date).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleOverviewGroupedByEmployee(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime dateFrom, DateTime dateTo, bool includeSecondaryCategories, string employeeIdsStr, string shiftTypeIdsStr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleOverviewGroupedByEmployee(param, employeeId, dateFrom, dateTo, includeSecondaryCategories, employeeIdsStr, shiftTypeIdsStr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleOverviewGroupedByEmployeeAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime dateFrom, DateTime dateTo, bool includeSecondaryCategories, string employeeIdsStr, string shiftTypeIdsStr, string accountIdsStr, bool includeUnscheduledEmployees)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleOverviewGroupedByEmployeeAdmin(param, dateFrom, dateTo, includeSecondaryCategories, employeeIdsStr, shiftTypeIdsStr, string.IsNullOrEmpty(accountIdsStr) ? "" : accountIdsStr, includeUnscheduledEmployees).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAvailableShiftsNew(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime date, string link, bool includeSecondaryCategories)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAvailableShiftsNew(param, employeeId, date, link, includeSecondaryCategories, MobileDisplayMode.User).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAvailableShiftsNewAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime date, string link, bool includeSecondaryCategories)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAvailableShiftsNew(param, 0, date, link, includeSecondaryCategories, MobileDisplayMode.Admin).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOthersShiftsNewAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeIdToView, DateTime date, bool includeSecondaryCategories)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetOthersShiftsNew(param, 0, employeeIdToView, date, includeSecondaryCategories, MobileDisplayMode.Admin).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAvailableEmployeesNew(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, bool filterOnShiftType, bool filterOnAvailability, bool filterOnSkills, bool filterOnWorkRules, bool shiftRequest, bool absenceRequest, DateTime absenceStartTime, DateTime absenceStopTime, int? filterOnMessageGroupId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAvailableEmployeesNew(param, shiftId, filterOnShiftType, filterOnAvailability, filterOnSkills, filterOnWorkRules, shiftRequest, absenceRequest, absenceStartTime, absenceStopTime, filterOnMessageGroupId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCompanyHolidays(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime dateFrom, DateTime dateTo)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetCompanyHolidays(param, dateFrom, dateTo).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSchedulePlanningEmployees(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, bool includeSecondaryCategories, string employeeIdsStr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetSchedulePlanningEmployeesForEmployee(param, employeeId, includeSecondaryCategories, string.IsNullOrEmpty(employeeIdsStr) ? "" : employeeIdsStr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSchedulePlanningShiftTypes(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, bool includeSecondaryCategories, string shiftTypeIdsStr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetSchedulePlanningShiftTypesForEmployee(param, employeeId, includeSecondaryCategories, shiftTypeIdsStr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSchedulePlanningEmployeesAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, bool includeSecondaryCategories, string accountIdsStr, string employeeIdsStr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetSchedulePlanningEmployeesForAdmin(param, includeSecondaryCategories, string.IsNullOrEmpty(accountIdsStr) ? "" : accountIdsStr, string.IsNullOrEmpty(employeeIdsStr) ? "" : employeeIdsStr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSchedulePlanningShiftTypesAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, bool includeSecondaryCategories, string accountIdsStr, string shiftTypeIdsStr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetSchedulePlanningShiftTypesForAdmin(param, includeSecondaryCategories, accountIdsStr, shiftTypeIdsStr).ToString();
        }

        #endregion

        #region New in version 15 (Stock handling and softond.online and ICA phase 2 )

        [WebMethod(Description = "", EnableSession = false)]
        public string ValidateAbsenceRequestPolicy(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int absenceRequestId, DateTime dateFrom, DateTime dateTo, int deviationCauseId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.ValidateAbsenceRequestPolicy(param, employeeId, absenceRequestId, dateFrom, dateTo, deviationCauseId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleDayViewEmployee(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime date, bool includeSecondaryCategories, string employeeIdsStr, string shiftTypeIdsStr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleDayViewEmployee(param, employeeId, date, includeSecondaryCategories, employeeIdsStr, shiftTypeIdsStr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCreateNewShiftsEmployeesNew(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime date, string shifts, bool filterOnShiftType, bool filterOnAvailability, bool filterOnSkills, bool filterOnWorkRules)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetCreateNewShiftsEmployeesNew(param, date, shifts, filterOnShiftType, filterOnAvailability, filterOnSkills, filterOnWorkRules).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAbsenceRequestsAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, bool includeDefinitive)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAbsenceRequestsAdmin(param, includeDefinitive).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAbsenceRequestAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, int absenceRequestId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAbsenceRequestAdmin(param, absenceRequestId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetApprovalTypes(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetApprovalTypes(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string ValidateAbsenceRequestPlanningAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, int absenceRequestId, string shifts)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.ValidateAbsenceRequestPlanning(param, absenceRequestId, shifts).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveAbsenceRequestPlanningAdmin(int type, string cultureCode, string version, int userId, int roleId, int companyId, int absenceRequestId, string shifts, string comment)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SaveAbsenceRequestPlanning(param, absenceRequestId, shifts, comment).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetEmployeeTimePeriodYears(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int year)
        {
            var m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var param = new MobileParam(userId, roleId, companyId, version);
            return m.GetEmployeeTimePeriodYears(param, employeeId, year).ToString();
        }
        [WebMethod(Description = "Get EmployeeTimePeriods", EnableSession = false)]
        public string GetEmployeeTimePeriods(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId)
        {
            var m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var param = new MobileParam(userId, roleId, companyId, version);
            return m.GetEmployeeTimePeriods(param, employeeId).ToString();
        }

        [WebMethod(Description = "Get EmployeeTimePeriod Details", EnableSession = false)]
        public string GetEmployeeTimePeriodDetails(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int timePeriodId)
        {
            var m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var param = new MobileParam(userId, roleId, companyId, version);
            return m.GetEmployeeTimePeriodDetails(param, employeeId, timePeriodId).ToString();
        }

        [WebMethod(Description = "Get EmployeeUserSettings", EnableSession = false)]
        public string GetEmployeeUserSettings(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId)
        {
            var m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var param = new MobileParam(userId, roleId, companyId, version);
            return m.GetEmployeeUserSettings(param, employeeId).ToString();
        }

        [WebMethod(Description = "Save EmployeeUserSettings", EnableSession = false)]
        public string SaveEmployeeUserSettings(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, bool wantsExtraShifts)
        {
            var m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveEmployeeUserSettings(param, employeeId, wantsExtraShifts).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SendNeedsConfirmationAnswer(int type, string cultureCode, string version, int userId, int roleId, int companyId, int mailId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SendNeedsConfirmationAnswer(param, mailId).ToString();
        }

        [WebMethod(Description = "Get stock info about a product.", EnableSession = false)]
        public string GetProductStockInfo(int type, string cultureCode, string version, int userId, int roleId, int companyId, int productId)
        {
            var m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var param = new MobileParam(userId, roleId, companyId, version);
            return m.GetProductStockInfo(param, productId).ToString();
        }

        #endregion

        #region New in version 16

        [WebMethod(Description = "", EnableSession = false)]
        public string GetOthersShiftsNew(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int employeeIdToView, DateTime date, bool includeSecondaryCategories)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetOthersShiftsNew(param, employeeId, employeeIdToView, date, includeSecondaryCategories, MobileDisplayMode.User).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetEmployeeChilds(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetEmployeeChilds(param, employeeId).ToString();
        }

        #region ProjectTimeBlock - New way to save time on order

        [WebMethod(Description = "", EnableSession = false)]
        public string GetProjectUseExtendedTimeRegistrationSetting(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            var m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var param = new MobileParam(userId, roleId, companyId, version);
            return m.GetExtendedTimeRegistrationSettings(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetDeviationCausesForTimeRegistration(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            var m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var param = new MobileParam(userId, roleId, companyId, version);
            return m.GetDeviationCausesForTimeRegistration(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetEmployeeFirstEligableTime(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime date)
        {
            var m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var param = new MobileParam(userId, roleId, companyId, version);
            return m.GetEmployeeFirstEligableTime(param, employeeId, date).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetLastProjectTimeBlockOnDate(int type, string cultureCode, string version, int companyId, int userId, int roleId, int orderId, int employeeId, DateTime date, int timeCodeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetLastProjectTimeBlockOnDate(param, orderId, employeeId, date, timeCodeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetProjectTimeBlock(int type, string cultureCode, string version, int companyId, int userId, int roleId, int orderId, int projectTimeBlockId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParam = GetUserParametersObject();
            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParam == null ? MobileDeviceType.Unknown : userParam.MobileDeviceType);
            return m.GetProjectTimeBlock(param, orderId, projectTimeBlockId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetProjectTimeBlocksForOrder(int type, string cultureCode, string version, int companyId, int userId, int roleId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParam = GetUserParametersObject();
            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParam == null ? MobileDeviceType.Unknown : userParam.MobileDeviceType);
            return m.GetProjectTimeBlocks(param, orderId, null, null).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetProjectTimeBlocks(int type, string cultureCode, string version, int companyId, int userId, int roleId, int orderId, DateTime fromTime, DateTime toTime)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            var userParam = GetUserParametersObject();
            MobileParam param = new MobileParam(userId, roleId, companyId, version, userParam == null ? MobileDeviceType.Unknown : userParam.MobileDeviceType);
            return m.GetProjectTimeBlocks(param, orderId, fromTime, toTime).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveProjectTimeBlock(int type, string cultureCode, string version, int companyId, int userId, int roleId, int orderId, int projectTimeBlockId, DateTime date, DateTime startTime, DateTime stopTime, int workTimeInMinutes, int invoiceTimeInMinutes, string note, string internalNote, int timeCodeId, int timeDeviationCauseId, bool hasValidated = false, int employeeChildId = 0)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveProjectTimeBlock(param, orderId, projectTimeBlockId, date, startTime, stopTime, workTimeInMinutes, invoiceTimeInMinutes, note, internalNote, timeCodeId, timeDeviationCauseId, hasValidated, employeeChildId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveProjectTimeBlockValidation(int type, string cultureCode, string version, int companyId, int userId, int roleId, int projectTimeBlockId, DateTime date, DateTime startTime, DateTime stopTime, int timeDeviationCauseId, int employeeChildId = 0)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveProjectTimeBlockValidation(param, projectTimeBlockId, date, startTime, stopTime, timeDeviationCauseId, employeeChildId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteProjectTimeBlock(int type, string cultureCode, string version, int companyId, int userId, int roleId, int projectTimeBlockId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.DeleteProjectTimeBlock(param, projectTimeBlockId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string MoveProjectTimeBlockToDate(int type, string cultureCode, string version, int companyId, int userId, int roleId, int projectTimeBlockId, DateTime date)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.MoveProjectTimeBlockToDate(param, projectTimeBlockId, date).ToString();
        }

        #endregion

        #endregion

        #region New in version 17

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAccountStringsFromHierarchyByUser(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAccountStringsFromHierarchyByUser(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string UpdateSettingAccountHierarchyId(int type, string cultureCode, string version, int userId, int roleId, int companyId, string id)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.UpdateSettingAccountHierarchyId(param, id).ToString();
        }


        [WebMethod(Description = "", EnableSession = false)]
        public string GetAccountHierarchyAccountDims(int type, string cultureCode, string version, int userId, int roleId, int companyId, string selectedAccountIdsStr, bool includeSecondaryAccounts)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAccountHierarchyAccountDims(param, selectedAccountIdsStr, includeSecondaryAccounts).ToString();
        }
        [WebMethod(Description = "", EnableSession = false)]
        public string GetEmployeeShiftAccounts(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, DateTime date)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetEmployeeShiftAccounts(param, employeeId, date).ToString();
        }

        #endregion

        #region New in version 19

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAccumulators(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetAccumulators(param, employeeId).ToString();
        }

        #endregion

        #region New in version 20

        [WebMethod(Description = "", EnableSession = false)]
        public string GetAdditionDeductionTimeCodes(int type, string cultureCode, string version, int companyId, int userId, int roleId, bool isOrder)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetAdditionDeductionTimeCodes(param, isOrder).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetExpense(int type, string cultureCode, string version, int companyId, int userId, int roleId, int expenseRowId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetExpense(param, expenseRowId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetExpensesForOrder(int type, string cultureCode, string version, int companyId, int userId, int roleId, int orderId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetExpenses(param, orderId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetExpensesForEmployeePeriod(int type, string cultureCode, string version, int companyId, int userId, int roleId, int employeeId, DateTime from, DateTime to)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetExpenses(param, employeeId, from, to).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetExpenseProductPrice(int type, string cultureCode, string version, int companyId, int userId, int roleId, int invoiceId, int timeCodeId, decimal quantity)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetExpenseProductPrice(param, invoiceId, timeCodeId, quantity).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveExpense(int type, string cultureCode, string version, int companyId, int userId, int roleId, int orderId, int expenseRowId, int timeCodeId, DateTime from, DateTime to, DateTime startTime, DateTime stopTime, decimal quantity, bool specifiedUnitPrice, decimal unitPrice, decimal amount, decimal vat, bool transferToInvoice, decimal invoiceAmount, string internalComment, string externalComment)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveExpense(param, orderId, expenseRowId, timeCodeId, from, to, startTime, stopTime, quantity, specifiedUnitPrice, unitPrice, amount, vat, transferToInvoice, invoiceAmount, internalComment, externalComment).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteExpense(int type, string cultureCode, string version, int companyId, int userId, int roleId, int expenseRowId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.DeleteExpense(param, expenseRowId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetMailIdsAsRead(int type, string cultureCode, string version, int userId, int roleId, int companyId, string mailIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetMailIdsAsRead(param, mailIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetMailIdsAsUnread(int type, string cultureCode, string version, int userId, int roleId, int companyId, string mailIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetMailIdsAsUnread(param, mailIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteIncomingMailIds(int type, string cultureCode, string version, int userId, int roleId, int companyId, string mailIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.DeleteIncomingMailIds(param, mailIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteOutgoingMailIds(int type, string cultureCode, string version, int userId, int roleId, int companyId, string mailIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.DeleteOutgoingMailIds(param, mailIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCompanyInformation(int type, string cultureCode, string version, int userId, int roleId, int companyId, int informationId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetCompanyInformation(param, informationId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCompanyInformations(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetCompanyInformations(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSysInformation(int type, string cultureCode, string version, int userId, int roleId, int companyId, int informationId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetSysInformation(param, informationId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSysInformations(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetSysInformations(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetUnreadInformations(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetUnreadInformations(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetInformationRead(int type, string cultureCode, string version, int userId, int roleId, int companyId, int informationType, int informationId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetInformationRead(param, informationType, informationId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetInformationConfirmed(int type, string cultureCode, string version, int userId, int roleId, int companyId, int informationType, int informationId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetInformationConfirmed(param, informationType, informationId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetShiftRequestStatus(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetShiftRequestStatus(param, shiftId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string RemoveShiftRequestRecipient(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId, int recipientUserId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.RemoveShiftRequestRecipient(param, shiftId, recipientUserId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string UndoShiftRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int shiftId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.UndoShiftRequest(param, shiftId).ToString();
        }

        #endregion

        #region New in version 21

        [WebMethod(Description = "", EnableSession = false)]
        public string RestoreDatesToSchedule(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, string dates)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.RestoreDatesToSchedule(param, dates, employeeId).ToString();
        }

        #endregion

        #region New in version 22(?)

        [WebMethod(Description = "", EnableSession = false)]
        public string SetDocumentRead(int type, string cultureCode, string version, int userId, int roleId, int companyId, int documentId, bool confirmed)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetDocumentRead(param, documentId, confirmed).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveExpenseValidation(int type, string cultureCode, string version, int companyId, int userId, int roleId, int orderId, int expenseRowId, int timeCodeId, DateTime from, DateTime to, DateTime startTime, DateTime stopTime, decimal quantity, bool specifiedUnitPrice, decimal unitPrice, decimal amount, decimal vat, bool transferToInvoice, decimal invoiceAmount, string internalComment, string externalComment)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveExpenseValidation(param, orderId, expenseRowId, timeCodeId, from, to, startTime, stopTime, quantity, specifiedUnitPrice, unitPrice, amount, vat, transferToInvoice, invoiceAmount, internalComment, externalComment).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSupplierInvoiceCostTransfers(int type, string cultureCode, string version, int companyId, int userId, int roleId, int invoiceId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetSupplierInvoiceCostTransfers(param, invoiceId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetSupplierInvoiceCostTransfer(int type, string cultureCode, string version, int companyId, int userId, int roleId, int recordType, int recordId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetSupplierInvoiceCostTransfer(param, recordType, recordId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveSupplierInvoiceCostTransfer(int type, string cultureCode, string version, int companyId, int userId, int roleId, int invoiceId, int recordType, int recordId, int orderId, int projectId, int timeCodeId, int employeeId, decimal amount, decimal supplementCharge, bool chargeCostToProject, bool includeSupplierInvoiceImage, int state)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveSupplierInvoiceCostTransfer(param, invoiceId, recordType, recordId, orderId, projectId, timeCodeId, employeeId, amount, supplementCharge, chargeCostToProject, includeSupplierInvoiceImage, state).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetMaterialAndTimeTimeCodes(int type, string cultureCode, string version, int companyId, int userId, int roleId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetMaterialAndTimeTimeCodes(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SearchOrder(int type, string cultureCode, string version, int companyId, int userId, int roleId, string orderNumber, string customer, int projectId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SearchOrder(param, orderNumber, customer, projectId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SearchProject(int type, string cultureCode, string version, int companyId, int userId, int roleId, string project, string customer, bool includeClosed)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SearchProject(param, project, customer, includeClosed).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetDimAccounts(int type, string cultureCode, string version, int companyId, int userId, int roleId, int dimNr)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetDimAccounts(param, dimNr).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SaveSupplierAccountRow(int type, string cultureCode, string version, int companyId, int userId, int roleId, int invoiceId, int rowId, int dim1AccountId,
            int dim2AccountId, int dim3AccountId, int dim4AccountId, int dim5AccountId, int dim6AccountId, decimal debetAmount, decimal creditAmount)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SaveSupplierAccountRow(param, invoiceId, rowId, dim1AccountId, dim2AccountId, dim3AccountId, dim4AccountId, dim5AccountId, dim6AccountId, debetAmount, creditAmount).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetFinVoiceHTML(int type, string cultureCode, string version, int userId, int roleId, int companyId, int fInvoiceEdiEntryId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetFinVoiceHTML(param, fInvoiceEdiEntryId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string CopyProjectTimeBlocksDate(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime fromDate, DateTime toDate)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.CopyProjectTimeBlocksDate(param, fromDate, toDate).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string CopyProjectTimeBlocksWeek(int type, string cultureCode, string version, int userId, int roleId, int companyId, DateTime fromDate, DateTime toDate)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.CopyProjectTimeBlocksWeek(param, fromDate, toDate).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string DeleteOrder(int type, string cultureCode, string version, int userId, int roleId, int companyId, int orderId, bool deleteProject)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.DeleteOrder(param, orderId, deleteProject).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetTimeTerminalsForUser(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetTimeTerminalsForUser(param, DateTime.Today).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCommonPermissions(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCommonPermissions(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetEvacuationList(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetEvacuationList(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string UpdateEvacuationListMarkings(int type, string cultureCode, string version, int userId, int roleId, int companyId,string employeeList, int headId = 0)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.UpdateEvacuationListMarkings(param, employeeList, headId).ToString();
        }
        [WebMethod(Description = "", EnableSession = false)]
        public string GetEvacuationListHistory(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetEvacuationListHistory(param, employeeId).ToString();
        }
        
        [WebMethod(Description = "", EnableSession = false)]
        public string GetTimeWorkAccountOptions(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int timeWorkAccountYearEmployeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetTimeWorkAccountOptions(param, employeeId, timeWorkAccountYearEmployeeId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetTimeWorkAccountOption(int type, string cultureCode, string version, int userId, int roleId, int companyId, int employeeId, int timeWorkAccountYearEmployeeId, int selectedId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.SetTimeWorkAccountOption(param, employeeId, timeWorkAccountYearEmployeeId, selectedId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetProductPrice(int type, string cultureCode, string version, int companyId, int userId, int roleId, int invoiceId, int productId, decimal quantity)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetProductPrice(param, invoiceId, productId, quantity).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCustomerCreditLimit(int type, string cultureCode, string version, int companyId, int userId, int roleId, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCustomerCreditLimit(param, customerId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetInvoiceDeliveryTypes(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetInvoiceDeliveryTypes(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetOrdersAsRead(int type, string cultureCode, string version, int userId, int roleId, int companyId, string orderIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetOrdersAsRead(param, orderIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string SetOrdersAsUnRead(int type, string cultureCode, string version, int userId, int roleId, int companyId, string orderIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.SetOrdersAsUnRead(param, orderIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetMessageGroups(int type, string cultureCode, string version, int userId, int roleId, int companyId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetMessageGroups(param).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCustomerNote(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetCustomerNote(param, customerId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetCustomerContactPerson(int type, string cultureCode, string version, int userId, int roleId, int companyId, int customerId, string name)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            return m.GetCustomerContactPerson(param, customerId, name).ToString();
        }

        #endregion

        #region Shift Swap
        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleSwapApproveView(int type, string cultureCode, string version, int userId, int roleId, int companyId, int timeScheduleSwapRequestId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleSwapApproveView(param, timeScheduleSwapRequestId, employeeId, userId).ToString();
        }
        
        [WebMethod(Description = "", EnableSession = false)]
        public string GetShiftsForSwap(int type, string cultureCode, string version, int userId, int roleId, int companyId, string initiatorShiftIds, string swapShiftIds, int employeeId, int employeeIdToView, DateTime initatorShiftDate, DateTime swapWithShiftDate)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);
            
            return m.GetShiftsForSwap(param, initiatorShiftIds, swapShiftIds, employeeId, employeeIdToView, initatorShiftDate, swapWithShiftDate, MobileDisplayMode.User).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string InitiateScheduleSwap(int type, string cultureCode, string version, int userId, int roleId, int companyId, int initiatorEmployeeId, DateTime initiatorShiftDate, string initiatorShiftIdsStr, int swapWithEmployeeId, DateTime swapShiftDate, String swapWithShiftIdsStr, String comment)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.InitiateScheduleSwap(param, initiatorEmployeeId, initiatorShiftDate, initiatorShiftIdsStr, swapWithEmployeeId, swapShiftDate, swapWithShiftIdsStr, comment).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleSwapAvailableEmployees(int type, string cultureCode, string version, int userId, int roleId, int companyId, int initiatorEmployeeId, DateTime initiatorShiftDate, DateTime swapShiftDate)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleSwapAvailableEmployees(param, initiatorEmployeeId, initiatorShiftDate, swapShiftDate).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string ApproveScheduleSwap(int type, string cultureCode, string version, int companyId, int userId, int roleId, int timeScheduleSwapRequestId, bool approved, string comment)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.ApproveScheduleSwap(param, userId, timeScheduleSwapRequestId, approved, comment).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string AssignScheduleSwapValidateWorkRules(int type, string cultureCode, string version, int companyId, int userId, int roleId, int timeScheduleSwapRequestId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.AssignScheduleSwapValidateWorkRules(param, timeScheduleSwapRequestId).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleSwapValidateLengths(int type, string cultureCode, string version, int userId, int roleId, int companyId, string sourceScheduleBlockIds, string targetScheduleBlockIds)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleSwapValidateLengths(param, sourceScheduleBlockIds, targetScheduleBlockIds).ToString();
        }

        [WebMethod(Description = "", EnableSession = false)]
        public string GetScheduleSwapValidateLengthsFromRequest(int type, string cultureCode, string version, int userId, int roleId, int companyId, int timeScheduleSwapRequestId, int employeeId)
        {
            MobileManager m = new MobileManager(GetParameterObject(companyId, userId, roleId), type, cultureCode, version);
            MobileParam param = new MobileParam(userId, roleId, companyId, version);

            return m.GetScheduleSwapValidateLengthsFromRequest(param, timeScheduleSwapRequestId, employeeId).ToString();
        }
        #endregion

        #region SoftOne Status

        [WebMethod]
        public SoftOneStatusDTO Status(Guid guid, string key)
        {

            StatusManager statusManager = new StatusManager();

            if (SoftOneIdConnector.ValidateSuperKey(guid, key))
            {
                return statusManager.GetSoftOneStatusDTO(ServiceType.WebserviceExternal);
            }

            return null;
        }

        #endregion
    }

    public static class JwtSecurityTokenExtensions
    {
        public static Claim Get(this IEnumerable<Claim> claims, string type)
        {
            return claims?.FirstOrDefault(x => x.Type == type);
        }
    }

}

using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core.Mobile.Objects;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Azure;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Mobile
{
    public class MobileManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static private readonly MobileManangerTexts Texts = new MobileManangerTexts();
        static private readonly MobileManagerUtil Utils = new MobileManagerUtil();
        private readonly bool debug;

        #endregion

        #region Ctor

        public MobileManager(ParameterObject parameterObject, int type, string cultureCode, string version, bool debug = false) : base(parameterObject)
        {
            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_28) && cultureCode.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                cultureCode = "sv-SE";

            SetLanguage(cultureCode);

            if (!IsXE(type) && !IsSauma(type))
                throw new NotImplementedException(); //TODO: Handle Professional/Sauma

            this.debug = debug;
        }

        #endregion

        #region Entry points

        #region AccountHierarchy

        public XDocument GetAccountStringsFromHierarchyByUser(MobileParam param)
        {
            //Perform
            MobileDicts dicts = PerformGetAccountStringsFromHierarchyByUser(param);

            return dicts.ToXDocument();
        }

        public XDocument UpdateSettingAccountHierarchyId(MobileParam param, string hierarchyId)
        {
            //Perform
            MobileResult result = PerformUpdateSettingAccountHierarchyId(param, hierarchyId);

            return result.ToXDocument(MobileTask.UpdateSetting);
        }

        public XDocument GetAccountHierarchyAccountDims(MobileParam param, string selectedAccountIdsStr, bool includeSecondaryAccounts)
        {
            //Perform
            MobileAccountDims accountDims = PerformGetAccountHierarchyAccountDims(param, selectedAccountIdsStr, includeSecondaryAccounts);

            return accountDims.ToXDocument();
        }
        public XDocument GetEmployeeShiftAccounts(MobileParam param, int employeeId, DateTime date)
        {
            //Perform
            MobileDicts dicts = PerformGetEmployeeShiftAccounts(param, employeeId, date);

            return dicts.ToXDocument();
        }

        #endregion

        #region Accumulators

        public XDocument GetAccumulators(MobileParam param, int employeeId)
        {
            //Perform            
            MobileAccumulators accumulators = PerformGetAccumulators(param, employeeId);

            return accumulators.ToXDocument();
        }

        #endregion

        #region Common

        #region Delegate
        public XDocument SearchTargetUserForDelegation(MobileParam param, string userCondition)
        {
            //Perform            
            MobileUserCompanyRoleDelegate userCompanyRoleDelegate = PerformSearchTargetUserForDelegation(param, userCondition);

            return userCompanyRoleDelegate.ToXDocument();
        }

        public XDocument GetUserDelegateHistory(MobileParam param)
        {
            //Perform            
            MobileUserDelegateHistory userDelegateHistory = PerformGetUserDelegateHistory(param);

            return userDelegateHistory.ToXDocument();
        }

        public XDocument SaveAttestRoleUserDelegation(MobileParam param, int targetUserId, int attestRoleUserId, DateTime dateFrom, DateTime dateTo)
        {
            //Perform                        
            MobileResult result = PerformSaveAttestRoleUserDelegation(param, targetUserId, attestRoleUserId, dateFrom, dateTo);
            return result.ToXDocument();
        }

        public XDocument DeleteUserCompanyRoleDelegation(MobileParam param, int headId)
        {
            //Perform                        
            MobileResult result = PerformDeleteUserCompanyRoleDelegation(param, headId);
            return result.ToXDocument();
        }

        #endregion

        public XDocument Login(string version, string licenseNr, string loginName, string password, out int userSessionId)
        {
            userSessionId = 0;

            if (debug)
                return XmlUtil.GetXDocument(MobileMessages.GetDefaultLoginXml());

            if (string.IsNullOrEmpty(licenseNr))
            {
                var user = UserManager.GetUser(parameterObject.UserId, loadLicense: true);
                if (user != null)
                    licenseNr = user.License.LicenseNr;
            }

            return PerformLogin(version, licenseNr, loginName, password, out userSessionId);
        }

        public XDocument Login(string version, int userId, out int userSessionId)
        {
            userSessionId = 0;

            if (debug)
                return XmlUtil.GetXDocument(MobileMessages.GetDefaultLoginXml());

            return PerformLogin(version, userId, out userSessionId);
        }

        public XDocument Startup(MobileParam param, out int userSessionId)
        {
            userSessionId = 0;

            if (debug)
                return XmlUtil.GetXDocument(MobileMessages.GetDefaultStartupXml());

            return PerformStartup(param, out userSessionId);
        }

        public XDocument Logout(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileMessages.GetDefaultLogoutXml());

            return PerformLogout(param);
        }

        public XDocument RegisterDeviceForNotifications(MobileParam param, string pushToken, string installationId)
        {
            return PerformRegisterDeviceForNotifications(param, pushToken, installationId).ToXDocument(MobileTask.Save);
        }

        public XDocument GetSoftOneProducts(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(SoftOneProducts.GetDefaultXml());

            SoftOneProducts softOneProducts = new SoftOneProducts(param);

            return softOneProducts.ToXDocument();
        }

        public XDocument GetCompaniesForUser(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileUserCompanies.GetDefaultXml());

            MobileUserCompanies mobileUserCompanies = PerformGetCompaniesForUser(param);

            return mobileUserCompanies.ToXDocument();
        }

        public XDocument GetRolesForUserCompany(MobileParam param, int rolesForCompanyId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileUserCompanyRoles.GetDefaultXml());

            MobileUserCompanyRoles mobileUserCompanyRoles = PerformGetRolesForUserCompany(param, rolesForCompanyId);

            return mobileUserCompanyRoles.ToXDocument();
        }

        public XDocument ValidateUserCompanyRole(MobileParam param, int newRoleId, int newCompanyId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileUserCompanyRole.GetDefaultXml());

            //Perform
            MobileUserCompanyRole result = PerformValidateUserCompanyRole(param, newRoleId, newCompanyId);

            return result.ToXDocument(MobileTask.UserCompanyRoleIsValid);
        }

        public XDocument GetModules(MobileParam param)
        {
            MobileModules modules = null;
            try
            {
                modules = PerformGetModules(param);
            }
            catch (Exception e)
            {

                LogError("GetModules: " + e.Message);
            }

            return modules.ToXDocument();
        }

        public XDocument GetModulesCounter(MobileParam param, DateTime? lastFetchDate)
        {
            MobileModules modules = PerformGetModulesCounter(param, lastFetchDate);

            return modules.ToXDocument();
        }

        public XDocument GetChangePWDPolicies(MobileParam param)
        {
            //Perform
            MobileChangePWD mobileChangePWDPolicies = PerformGetChangePWDPolicies(param);

            return mobileChangePWDPolicies.ToXDocument();
        }

        public XDocument ChangePWD(MobileParam param, string oldPWD, string newPWD, string confirmNewPWD)
        {
            return new MobileChangePWD(param, GetText(8800, "Denna funktion är för tillfället avstängd.")).ToXDocument(MobileTask.ChangePWD);
        }

        public XDocument GetSettingsPermissions(MobileParam param)
        {
            //Perform
            MobilePermissions mobileSettingsPermissions = PerformGetSettingsPermissions(param);

            return mobileSettingsPermissions.ToXDocument();
        }

        #endregion

        #region Billing

        public string GetFinVoiceHTML(MobileParam param, int fInvoiceEdiEntry)
        {
            var htmlview = PerformGetFinVoiceHTML(param, fInvoiceEdiEntry);
            return htmlview.ToString();
        }

        public XDocument SearchCustomers(MobileParam param, string search)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCustomers.GetDefaultXml());

            //Perform
            MobileCustomers mobileCustomers = PerformSearchCustomers(param, search);

            return mobileCustomers.ToXDocument();
        }

        public XDocument SearchProducts(MobileParam param, int orderId, string search)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileProducts.GetDefaultXml());

            //Perform
            MobileProducts mobileProducts = PerformSearchProducts(param, orderId, search);

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_8) && !FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowSalesPrice, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                mobileProducts.SetPriceDisabled();

            return mobileProducts.ToXDocument();
        }

        public XDocument SearchExternalProducts(MobileParam param, int orderId, string search)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileExternalProducts.GetDefaultXml());

            //Perform
            MobileExternalProducts mobileExternalProducts = PerformSearchExternalProducts(param, orderId, search);

            //Permission

            return mobileExternalProducts.ToXDocument();
        }

        public XDocument SearchExternalProductPrices(MobileParam param, int orderId, int sysproductId, string productNr)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileExternalProductPrices.GetDefaultXml());

            //Perform
            MobileExternalProductPrices mobileExternalProductPrices = PerformSearchExternalProductPrices(param, orderId, sysproductId, productNr);

            //Permission

            return mobileExternalProductPrices.ToXDocument();
        }

        public XDocument GetInvoiceProductFromSys(MobileParam param, int orderId, int sysProductId, int sysWholeSellerId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileProduct.GetDefaultXml());

            MobileProduct invoiceProduct = PerformGetInvoiceProductFromSys(param, orderId, sysProductId, sysWholeSellerId);

            return invoiceProduct.ToXDocument();
        }

        public XDocument GetProductStockInfo(MobileParam param, int productId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileStockProductInfos.GetDefaultXml());

            //Perform
            var mobileStockProductInfos = PerformGetProductStockInfos(param, productId);

            return mobileStockProductInfos.ToXDocument();
        }

        public XDocument GetProductPrice(MobileParam param, int invoiceId, int productId, decimal quantity)
        {
            var mobileValueResult = PerformGetProductPrice(param, invoiceId, productId, quantity);

            return mobileValueResult.ToXDocument();
        }

        public XDocument GetOrderRows(MobileParam param, int orderId, bool showAll)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRows.GetDefaultXml());

            //Perform
            MobileOrderRows mobileOrderRows = PerformGetOrderRows(param, orderId, showAll);

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_8) && !FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowSalesPrice, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                mobileOrderRows.SetAmountDisabled();

            return mobileOrderRows.ToXDocument();
        }
        
        public XDocument GetOrderRowExternalUrl(MobileParam param, int productId)
        {
            //Perform
            var mobileResult = PerformGetOrderRowExternalUrl(param, productId);

            return mobileResult.ToXDocument(MobileTask.GetValue);
        }

        public XDocument GetTimeRows(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileTimeRows.GetDefaultXml());

            //Perform
            MobileTimeRows mobileTimeRows = PerformGetTimeRows(param, orderId);

            return mobileTimeRows.ToXDocument();
        }

        public XDocument GetTimeRow(MobileParam param, int orderId, DateTime date, int timeCodeId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileTimeRows.GetDefaultXml());

            //Perform
            MobileTimeRows mobileTimeRows = PerformGetTimeRows(param, orderId);
            MobileTimeRow mobileTimeRow = mobileTimeRows.ToList().FirstOrDefault(m => m.TimeCodeId == timeCodeId && m.Date == date);

            // No entry found
            if (mobileTimeRow == null)
            {
                mobileTimeRow = new MobileTimeRow(param, orderId, new ProjectInvoiceDay(), 0, "", true, true);
            }


            return mobileTimeRow.ToXDocument();
        }

        public XDocument SaveOrder(MobileParam param, int orderId, int customerId, string descriptionText, string label, string headText, string ourReference, int vatType, int priceListId, int currencyId, int deliveryAddressId, int billingAddressId, int wholseSellerId, string yourReference, int projectId, int templateId, string deliveryDate, int orderTypeId, string workDescription, bool discardConcurrencyCheck = false)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderEdit.GetDefaultSaveXml());

            if (vatType == 0)
                vatType = (int)TermGroup_InvoiceVatType.Merchandise;

            //Perform
            MobileOrderEdit mobileOrder = PerformSaveOrder(param, orderId, customerId, descriptionText, label, headText, ourReference, (TermGroup_InvoiceVatType)vatType, priceListId, currencyId, deliveryAddressId, billingAddressId, wholseSellerId, yourReference, projectId, templateId, deliveryDate, orderTypeId, workDescription, discardConcurrencyCheck);

            return mobileOrder.ToXDocument(MobileTask.SaveOrder);
        }

        public XDocument SetOrderUserIsReady(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            var mobileResult = PerformSetOrderUserIsReady(param, orderId);
            return mobileResult.ToXDocument(MobileTask.SetOrderIsReady);
        }

        public XDocument SetOrderReady(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderEdit.GetDefaultSetReadyXml());

            //Perform
            MobileOrderEdit mobileOrder = PerformSetOrderReady(param, orderId);

            return mobileOrder.ToXDocument(MobileTask.SetOrderReady);
        }

        public XDocument SetOrderRowsAsReady(MobileParam param, int orderId, string ids)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderEdit.GetDefaultSetReadyXml());

            //Perform
            MobileOrderEdit mobileOrder = PerformSetOrderRowsAsReady(param, orderId, ids);

            return mobileOrder.ToXDocument(MobileTask.SetOrderRowsAsReady);
        }

        public XDocument SetOrdersAsRead(MobileParam param, string orderIds)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            var mobileResult = PerformSetOrdersAsRead(param, orderIds);
            return mobileResult.ToXDocument(MobileTask.Update);
        }

        public XDocument SetOrdersAsUnRead(MobileParam param, string orderIds)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            var mobileResult = PerformSetOrdersAsUnRead(param, orderIds);
            return mobileResult.ToXDocument(MobileTask.Update);
        }

        public XDocument SaveOrderRow(MobileParam param, int orderId, int orderRowId, int productId, int sysProductId, int sysWholesellerId, decimal quantity, decimal amount, string text, decimal quantityToInvoice, int stockId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            //Perform
            MobileOrderRow mobileOrderRow = PerformSaveOrderRow(param, orderId, orderRowId, productId, sysProductId, sysWholesellerId, quantity, amount, text, quantityToInvoice, stockId);

            return mobileOrderRow.ToXDocument(MobileTask.SaveOrderRow);
        }

        public XDocument SaveHouseholdDeduction(MobileParam param, int orderId, int orderRowId, string propertyLabel, string socialSecNbr, string name, decimal amount, string apartmentNbr, string cooperativeOrgNbr, bool isHDRut, int mobileDeductionType)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileHouseholdDeductionRow.GetDefaultSaveXml());

            //Perform
            MobileHouseholdDeductionRow mobileOrderRow = PerformSaveHouseholdDeduction(param, orderId, orderRowId, propertyLabel, socialSecNbr, name, amount, apartmentNbr, cooperativeOrgNbr, isHDRut, mobileDeductionType);

            return mobileOrderRow.ToXDocument(MobileTask.SaveHouseholdDeductionRow);
        }

        public XDocument GetHouseholdDeduction(MobileParam param, int orderId, int orderRowId, bool isRUT, int mobileDeductionType)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileHouseholdDeductionRow.GetDefaultXml());

            //Perform
            MobileHouseholdDeductionRow mobileOrderRow = PerformGetHouseholdDeduction(param, orderId, orderRowId, isRUT, mobileDeductionType);

            return mobileOrderRow.ToXDocument();
        }

        public XDocument GetHouseholdDeductionApplicants(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileHouseholdDeductionApplicants.GetDefaultXml());

            //Perform
            MobileHouseholdDeductionApplicants mobileHouseholdDeductionApplicants = PerformGetHouseholdDeductionApplicants(param, orderId);
            LogPersonalData(mobileHouseholdDeductionApplicants, TermGroup_PersonalDataActionType.Read, "GetHouseholdDeductionApplicants()");

            return mobileHouseholdDeductionApplicants.ToXDocument();
        }

        public XDocument GetHouseholdDeductionApplicantsForCustomer(MobileParam param, int customerId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileHouseholdDeductionApplicants.GetDefaultXml());

            //Perform
            MobileHouseholdDeductionApplicants mobileHouseholdDeductionApplicants = PerformGetHouseholdDeductionApplicantsForCustomer(param, customerId);

            return mobileHouseholdDeductionApplicants.ToXDocument();
        }

        public XDocument SaveHouseholdDeductionApplicantsForCustomer(MobileParam param, int customerId, int hdApplicantId, string name, string socSecNr, string property, string apartmentNr, string coopOrgNr)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileHouseholdDeductionApplicants.GetDefaultXml());

            //Perform
            var result = PerformSaveHouseholdDeductionApplicantsForCustomer(param, customerId, hdApplicantId, name, socSecNr, property, apartmentNr, coopOrgNr);

            return result.ToXDocument(MobileTask.Save);
        }

        public XDocument DeleteHouseholdDeductionApplicantForCustomer(MobileParam param, int hdApplicantId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileHouseholdDeductionApplicants.GetDefaultXml());

            //Perform
            var result = PerformDeleteHouseholdDeductionApplicantForCustomer(param, hdApplicantId);

            return result.ToXDocument(MobileTask.Delete);
        }

        public XDocument GetHouseholdDeductionTypes(MobileParam param, bool incRot, bool incRut, bool incGreenTech)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileHouseHoldDeductionTypes.GetDefaultXml());

            //Perform
            return PerformGetMobileHouseHoldDeductionTypes(param, incRot, incRut, incGreenTech).ToXDocument();
        }

        public XDocument SaveTimeRow(MobileParam param, int orderId, int dayId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId)
        {
            MobileTimeRow mobileTimeRow;

            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            if (dayId > 0)
            {
                MobileTimeRows mobileTimeRows = PerformGetTimeRows(param, orderId);
                MobileTimeRow existingTimeRowOnDayId = mobileTimeRows.ToList().FirstOrDefault(m => m.Id == dayId);

                if (existingTimeRowOnDayId != null)
                {
                    if (date != existingTimeRowOnDayId.Date)
                    {
                        //The date is changed, reset the time on the existing time row
                        PerformSaveTimeRow(param, orderId, dayId, existingTimeRowOnDayId.Date, 0, 0, note, timeCodeId, true);

                        //Check if there is already timerow on the date that is changed to
                        MobileTimeRow existingTimeRowOnNewDate = mobileTimeRows.ToList().FirstOrDefault(m => m.Date == date);

                        if (existingTimeRowOnNewDate != null)
                        {
                            dayId = existingTimeRowOnNewDate.Id;
                        }
                    }

                    //get projectinvoiceweek or add weekid to mobiletimerow for also checking if the timecode is changed
                    if (existingTimeRowOnDayId.TimeCodeId != timeCodeId)
                    {
                        DateTime beginningOfWeek = CalendarUtility.GetFirstDateOfWeek(date, offset: DayOfWeek.Monday);
                        var employeeId = EmployeeManager.GetEmployeeIdForUser(param.UserId, param.ActorCompanyId);
                        ProjectManager.UpdateProjectInvoiceWeekOnInvoiceDay(dayId, beginningOfWeek, orderId, employeeId, param.ActorCompanyId, timeCodeId);
                    }
                }
            }

            mobileTimeRow = PerformSaveTimeRow(param, orderId, dayId, date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId);

            return mobileTimeRow.ToXDocument(MobileTask.SaveTimeRow);
        }

        public XDocument SaveOrResetTimeRow(MobileParam param, int orderId, int timeRowId, int dayId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId)
        {
            MobileTimeRow mobileTimeRow;

            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            //get existing
            MobileTimeRows mobileTimeRows = PerformGetTimeRows(param, orderId);
            MobileTimeRow existingTimeRow = mobileTimeRows.ToList().FirstOrDefault(m => m.Id == timeRowId);

            // If new, save and return
            if (existingTimeRow == null)
            {
                mobileTimeRow = PerformSaveTimeRow(param, orderId, dayId, date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId);
                return mobileTimeRow.ToXDocument(MobileTask.SaveTimeRow);
            }


            // if date or timecode is changed, clear and update
            if (existingTimeRow.Id != 0 && (existingTimeRow.TimeCodeId != timeCodeId || existingTimeRow.Date != date))
            {
                // Update
                mobileTimeRow = new MobileTimeRow(param, orderId, "Mattias fixar");
            }
            else
            {
                //Save
                mobileTimeRow = PerformSaveTimeRow(param, orderId, dayId, date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId);
            }

            return mobileTimeRow.ToXDocument(MobileTask.SaveTimeRow);

        }

        public XDocument DeleteOrderRow(MobileParam param, int orderId, int orderRowId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultDeleteXml());

            //Perform
            MobileOrderRow mobileOrderRow = PerformDeleteOrderRow(param, orderId, orderRowId);

            return mobileOrderRow.ToXDocument(MobileTask.DeleteOrderRow);
        }

        public XDocument DeleteTimeRowAndSaveNew(MobileParam param, int orderId, int dayId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            //Perform delete            
            //MobileTimeRow mobileTimeRowDelete = PerformSaveTimeRow(param, orderId, dayId, date, 0, 0, note, timeCodeId);

            //Perform add
            MobileTimeRow mobileTimeRow = PerformSaveTimeRow(param, orderId, dayId, date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId);

            return mobileTimeRow.ToXDocument(MobileTask.SaveTimeRow);
        }

        public XDocument GetOrdersGrid(MobileParam param, bool hideStatusOrderReady, bool showMyOrders, bool hideUserStateReady)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrdersGrid.GetDefaultXml());

            //Perform
            MobileOrdersGrid mobileOrdersGrid = PerformGetOrdersGrid(param, hideStatusOrderReady, showMyOrders, hideUserStateReady);

            //Permissions (Showsalesprice not handled in App for the grid, its handled by us)
            if (!FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowSalesPrice, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                mobileOrdersGrid.SetAmountDisabled();

            return mobileOrdersGrid.ToXDocument();
        }

        public XDocument GetOrderEdit(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderEdit.GetDefaultXml());

            //Perform
            MobileOrderEdit mobileOrderEdit = PerformGetOrderEdit(param, orderId);

            //Permissions (Showsalesprice not handled in App for the products expander och in the orderhead, its handled by us)
            if (!FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowSalesPrice, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                mobileOrderEdit.SetAmountDisabled();

            return mobileOrderEdit.ToXDocument();
        }
        
        public XDocument GetOrderTemplateInfo(MobileParam param, int templateId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderTemplateInfo.GetDefaultXml());

            //Perform
            MobileOrderTemplateInfo mobileOrderTemplateInfo = PerformGetOrderTemplateInfo(param, templateId);

            return mobileOrderTemplateInfo.ToXDocument();
        }

        public XDocument GetOrderInvoiceTemplates(MobileParam param, int customerId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileDicts.GetDefaultXml());

            MobileDicts mobileDicts = PerformGetOrderInvoiceTemplates(param, customerId);

            return mobileDicts.ToXDocument();
        }

        public XDocument GetOrderTypes(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderType.GetDefaultXml());

            var mobileOrderTypes = PerformGetOrderTypes(param);

            return mobileOrderTypes.ToXDocument();
        }

        public XDocument GetCustomerCreditLimit(MobileParam param, int customerId)
        {
            var result = PerformGetCustomerCreditLimit(param, customerId);

            return result.ToXDocument();
        }

        public XDocument GetVatTypes(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileVatTypes.GetDefaultXml());

            //Perform
            MobileVatTypes mobileVatTypes = PerformGetVatTypes(param);

            return mobileVatTypes.ToXDocument();
        }

        public XDocument GetCurrencies(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCurrencies.GetDefaultXml());

            //Perform
            MobileCurrencies mobileCurrencies = PerformGetCurrencies(param);

            return mobileCurrencies.ToXDocument();
        }

        public XDocument GetPaymentConditions(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobilePaymentConditions.GetDefaultXml());

            //Perform
            MobilePaymentConditions mobilePaymentConditions = PerformGetPaymentConditions(param);

            return mobilePaymentConditions.ToXDocument();
        }

        public XDocument GetPriceLists(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobilePriceLists.GetDefaultXml());

            //Perform
            MobilePriceLists mobileSalePriceLists = PerformGetPriceLists(param);

            return mobileSalePriceLists.ToXDocument();
        }

        public XDocument GetWholeSellers(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileWholeSellers.GetDefaultXml());

            //Perform
            MobileWholeSellers mobileWholeSellers = PerformGetWholeSellers(param);

            return mobileWholeSellers.ToXDocument();
        }

        public XDocument GetContactPersons(MobileParam param, int customerId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileContactPersons.GetDefaultXml());

            //Perform
            MobileContactPersons mobileContactPersons = PerformGetContactPersons(param, customerId);

            return mobileContactPersons.ToXDocument();
        }

        public XDocument GetCustomerReferences(MobileParam param, int customerId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileContactPersons.GetDefaultXml());

            //Perform
            MobileContactPersons mobileContactPersons = PerformGetCustomerReferences(param, customerId);

            return mobileContactPersons.ToXDocument();
        }

        public XDocument GetCustomerContactPerson(MobileParam param, int customerId, string name)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileContactPersons.GetDefaultXml());

            //Perform
            MobileContactPerson mobileContactPersons = PerformGetCustomerContactPerson(param, customerId, name);

            return mobileContactPersons.ToXDocument();
        }

        public XDocument GetDeliveryAddress(MobileParam param, int customerId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderAddressItems.GetDefaultXml());

            //Perform
            MobileOrderAddressItems mobileDeliveryAddress = PerformGetDeliveryAddress(param, customerId);

            return mobileDeliveryAddress.ToXDocument();
        }

        public XDocument GetInvoiceAddress(MobileParam param, int customerId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderAddressItems.GetDefaultXml());

            //Perform
            MobileOrderAddressItems mobileInvoiceAddress = PerformGetInvoiceAddress(param, customerId);

            return mobileInvoiceAddress.ToXDocument();
        }

        public XDocument GetInvoiceDeliveryTypes(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileDicts.GetDefaultXml());

            //Perform
            var invoiceDeliveryTypes = PerformGetInvoiceDeliveryTypes(param);

            return invoiceDeliveryTypes.ToXDocument();
        }

        public XDocument GetOrderThumbNails(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImages.GetDefaultXml());

            //Perform
            MobileImages mobileImages = PerformGetOrderThumbNails(param, orderId);

            return mobileImages.ToXDocument();
        }

        public XDocument GetOrderThumbNailsOnChecklist(MobileParam param, int orderId, int checklistHeadId, int checklistHeadRecordId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImages.GetDefaultXml());

            //Perform
            MobileImages mobileImages = PerformGetOrderThumbNailsOnChecklist(param, orderId, checklistHeadId, checklistHeadRecordId);

            return mobileImages.ToXDocument();
        }

        public XDocument GetDocuments(MobileParam param, int recordId, int entityType, string documentTypes, bool ignoreFileTypes)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImages.GetDefaultXml());

            //Perform
            MobileImages mobileImages = PerformGetDocuments(param, recordId, entityType, documentTypes, ignoreFileTypes);

            return mobileImages.ToXDocument();
        }

        public XDocument GetImage(MobileParam param, int orderId, int imageId, bool isFile)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImage.GetDefaultXml());

            //Perform
            MobileImage mobileImage = PerformGetImage(param, orderId, imageId, isFile);

            return mobileImage.ToXDocument();
        }

        public XDocument AddDocument(MobileParam param, int recordId, int entityType, int documentType, byte[] data, string description, string fileName, bool updateExtension)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImage.GetDefaultSaveXml());

            //Perform
            MobileResult mobileResult = PerformAddDocument(param, recordId, entityType, documentType, data, description, fileName, updateExtension);

            return mobileResult.ToXDocument(MobileTask.Save);

        }
        public XDocument AddImage(MobileParam param, int orderId, byte[] imageData, String description, int imageType, String fileName)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImage.GetDefaultSaveXml());

            //Perform
            MobileImage mobileImage = PerformAddImage(param, orderId, imageData, description, imageType, fileName);

            return mobileImage.ToXDocument(MobileTask.AddOrderImage);
        }

        public XDocument AddChecklistImage(MobileParam param, int orderId, int checklistHeadId, int checklistHeadRecordId, byte[] imageData, String description, int imageType)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImage.GetDefaultSaveXml());

            //Perform
            MobileImage mobileImage = PerformAddChecklistImage(param, orderId, checklistHeadId, checklistHeadRecordId, imageData, description, imageType);

            return mobileImage.ToXDocument(MobileTask.AddOrderImage);
        }

        public XDocument DeleteImage(MobileParam param, int orderId, int imageId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImage.GetDefaultSaveXml());

            //Perform
            MobileImage mobileImage = PerformDeleteImage(param, orderId, imageId);

            return mobileImage.ToXDocument(MobileTask.DeleteOrderImage);
        }

        public XDocument DeleteDocument(MobileParam param, int recordId, int documentId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImage.GetDefaultSaveXml());

            //Perform
            MobileImage mobileImage = PerformDeleteDocument(param, recordId, documentId);

            return mobileImage.ToXDocument(MobileTask.DeleteOrderImage);
        }

        public XDocument EditImage(MobileParam param, int orderId, int imageId, String description, String fileName)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileImage.GetDefaultSaveXml());

            //Perform
            MobileImage mobileImage = PerformEditImage(param, orderId, imageId, description, fileName);

            return mobileImage.ToXDocument(MobileTask.EditOrderImage);
        }

        public XDocument GetChecklists(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCheckLists.GetDefaultXml());

            //Perform
            MobileCheckLists mobileChecklists = PerformGetCheckLists(param, orderId);

            return mobileChecklists.ToXDocument();
        }

        public XDocument GetUnUsedChecklists(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCheckLists.GetDefaultXml());

            //Perform
            MobileCheckLists mobileChecklists = PerformGetUnUsedCheckLists(param, orderId);

            return mobileChecklists.ToXDocument();
        }

        public XDocument AddChecklist(MobileParam param, int orderId, string checklistHeadIds)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCheckLists.GetDefaultXml());

            //Perform
            MobileCheckList mobileChecklist = PerformAddChecklist(param, orderId, checklistHeadIds);

            return mobileChecklist.ToXDocument(MobileTask.AddCheckList);
        }

        public XDocument GetChecklistContent(MobileParam param, int orderId, int checklistHeadId, int checklistHeadRecordId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCheckListRows.GetDefaultXml());

            //Perform
            MobileCheckListRows mobileCheckListRows = PerformGetCheckListContent(param, orderId, checklistHeadId, checklistHeadRecordId);

            return mobileCheckListRows.ToXDocument();
        }

        public XDocument GetMultipleChoiceAnswerRows(MobileParam param, int multipleChoiceAnswerHeadId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCheckListRows.GetDefaultXml());

            //Perform
            MobileMultipleChoiceAnswerRows answerRows = PerformGetMultipleChoiceAnswerRows(param, multipleChoiceAnswerHeadId);

            return answerRows.ToXDocument();
        }

        public XDocument SaveCheckListAnswers(MobileParam param, int orderId, int checklistHeadId, int checklistHeadRecordId, String inputAnswers)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCheckListRows.GetDefaultXml());

            //Perform
            MobileCheckListRows mobileCheckListRows = PerformSaveCheckListAnswers(param, orderId, checklistHeadId, checklistHeadRecordId, inputAnswers);

            return mobileCheckListRows.ToXDocument(MobileTask.SaveChecklistAnswers);
        }

        public XDocument DeleteOrderChecklists(MobileParam param, int orderId, string checklistHeadRecordIds)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCheckListRows.GetDefaultXml());

            //Perform
            MobileCheckList mobileCheckList = PerformDeleteOrderChecklists(param, orderId, checklistHeadRecordIds);

            return mobileCheckList.ToXDocument(MobileTask.DeleteOrderChecklist);
        }

        public XDocument GetOrderUsers(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderUsers.GetDefaultXml());

            //Perform
            MobileOrderUsers mobileOrderUsers = PerformGetOrderUsers(param, orderId);

            return mobileOrderUsers.ToXDocument();
        }

        public XDocument GetOrderCurrentUsers(MobileParam param, int orderId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderUsers.GetDefaultXml());

            //Perform
            MobileOrderUsers mobileOrderUsers = PerformGetOrderCurrentUsers(param, orderId);

            return mobileOrderUsers.ToXDocument();
        }

        public XDocument SaveMapLocation(MobileParam param, Decimal longitude, Decimal latitude, String description, DateTime timeStamp)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileMapLocation.GetDefaultSaveXml());

            //Perform
            MobileMapLocation mobileMapLocation = PerformSaveMapLocation(param, longitude, latitude, description, timeStamp);

            return mobileMapLocation.ToXDocument(MobileTask.SaveMapLocation);
        }

        public XDocument SaveOrderUsers(MobileParam param, int orderId, String userids, int mainUserId, bool sendMail)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderUsers.GetDefaultSaveXml());

            //Perform
            MobileOrderUsers mobileOrderUsers = PerformSaveOrderUsers(param, orderId, userids, mainUserId, sendMail);

            return mobileOrderUsers.ToXDocument(MobileTask.SaveOriginUsers);
        }

        public XDocument SaveCustomer(MobileParam param, int customerId, string customerNr, string name, string orgNr, string vatNr, string reference, int vatTypeId, int paymentConditionId, int salesPriceListid, int stdWholeSellerId, decimal disccountArticles, decimal disccountServices, int currencyId, string note, int emailAddressId, string emailAddress, int homePhoneId, string homePhone, int jobPhoneId, string jobPhone, int mobilePhoneId, string mobilePhone, int faxId, string fax, int invoiceAddressId, string invoiceAddress, string iaPostalCode, string iaPostalAddress, string iaCountry, string iaAddressCO, int deliveryAddressId1, string deliveryAddress1, string da1PostalCode, string da1PostalAddress, string da1Country, string da1AddressCO, string da1Name, int invoiceDeliveryTypeId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCustomerEdit.GetDefaultSaveXml());

            //Perform
            MobileCustomerEdit mobileCustomer = PerformSaveCustomer(param, customerId, customerNr, name, orgNr, reference, note, vatTypeId, vatNr, paymentConditionId, salesPriceListid, stdWholeSellerId, currencyId, disccountArticles, disccountServices, emailAddressId, emailAddress, homePhoneId, homePhone, jobPhoneId, jobPhone, mobilePhoneId, mobilePhone, faxId, fax, invoiceAddressId, invoiceAddress, iaPostalCode, iaPostalAddress, iaCountry, iaAddressCO, deliveryAddressId1, deliveryAddress1, da1PostalCode, da1PostalAddress, da1Country, da1AddressCO, da1Name, invoiceDeliveryTypeId);

            return mobileCustomer.ToXDocument(MobileTask.SaveCustomer);
        }

        public XDocument GetTimeProjectTimeCodes(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileTimeCodes.GetDefaultXml());

            //Perform
            MobileTimeCodes mobileTimeCodes = PerformGetTimeProjectTimeCodes(param);

            return mobileTimeCodes.ToXDocument();
        }

        public XDocument GetMaterialAndTimeTimeCodes(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileTimeCodes.GetDefaultXml());

            MobileTimeCodes mobileTimeCodes = PerformGetMaterialAndTimeTimeCodes(param);

            return mobileTimeCodes.ToXDocument();
        }

        public XDocument GetNextCustomerNr(MobileParam param)
        {
            //Perform
            MobileNewCustomer mobileNewCustomer = PerformGetNextCustomerNr(param);

            return mobileNewCustomer.ToXDocument();
        }

        public XDocument GetCustomersGrid(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCustomersGrid.GetDefaultXml());

            //Perform
            MobileCustomersGrid mobileCustomersGrid = PerformGetCustomersGrid(param);

            return mobileCustomersGrid.ToXDocument();
        }

        public XDocument GetCustomerEdit(MobileParam param, int customerId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCustomerEdit.GetDefaultXml());

            //Perform
            MobileCustomerEdit mobileCustomerEdit = PerformGetCustomerEdit(param, customerId);

            return mobileCustomerEdit.ToXDocument();
        }

        public XDocument GetCustomerNote(MobileParam param, int customerId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileTextBlock.GetDefaultXml());

            //Perform
            var customerNote = PerformGetCustomerNote(param, customerId);

            return customerNote.ToXDocument();
        }

        public XDocument GetOrderWorkDescription(MobileParam param, int orderId)
        {
            //Perform
            MobileWorkDescription mobileWorkDescription = PerformGetOrderWorkDescription(param, orderId);

            return mobileWorkDescription.ToXDocument();
        }

        public XDocument SaveOrderWorkDescription(MobileParam param, int orderId, string workDesc)
        {
            //Perform
            MobileWorkDescription mobileWorkDescription = PerformSaveOrderWorkDescription(param, orderId, workDesc);

            return mobileWorkDescription.ToXDocument(MobileTask.SaveWorkingDescription);
        }

        public XDocument GetProjectsBySearch(MobileParam param, int customerId, string searchText)
        {
            //Perform
            MobileProjects mobileProjects = PerformGetProjectsBySearch(param, customerId, searchText);

            return mobileProjects.ToXDocument();
        }

        public XDocument GetProjectsBySearch2(MobileParam param, string number, string name, string customerNr, string customerName, string managerName, string orderNr, int customerId)
        {
            //Perform
            MobileProjects mobileProjects = PerformGetProjectsBySearch2(param, number, name, customerNr, customerName, managerName, orderNr, customerId);
            return mobileProjects.ToXDocument();
        }

        public XDocument ChangeProjectOnOrder(MobileParam param, int orderId, int projectId)
        {
            //Perform
            MobileProject mobileProject = PerformChangeProjectOnOrder(param, orderId, projectId);

            return mobileProject.ToXDocument(MobileTask.ChangeProjectOnOrder);
        }

        public XDocument PreCreateOrder(MobileParam param)
        {
            //Perform
            MobilePreCreateOrder mobilePreCreateOrder = PerformPreCreateOrder(param);

            return mobilePreCreateOrder.ToXDocument();
        }

        public XDocument GetSupplierInvoicesAttestWorkFlowMyActive(MobileParam param)
        {
            //Perform
            MobileAttestInvoices mobileAttestInvoices = PerformGetSupplierInvoicesAttestWorkFlowMyActive(param);

            return mobileAttestInvoices.ToXDocument();
        }

        public XDocument GetSupplierInvoiceAttestWorkFlowView(MobileParam param, int invoiceId)
        {
            //Perform
            MobileAttestInvoice mobileAttestInvoice = PerformGetSupplierInvoiceAttestWorkFlowView(param, invoiceId);

            return mobileAttestInvoice.ToXDocument();
        }
        public XDocument GetSupplierInvoiceCostTransfers(MobileParam param, int invoiceId)
        {
            //Perform
            MobileAttestInvoiceCostTransferRows mobileAttestInvoiceOrderProjectRows = PerformGetSupplierInvoiceCostTransfers(param, invoiceId);

            return mobileAttestInvoiceOrderProjectRows.ToXDocument();
        }
        public XDocument GetSupplierInvoiceCostTransfer(MobileParam param, int recordType, int recordId)
        {
            MobileSupplierInvoiceCostTransfer supplierInvoiceCostTransfer = PerformGetSupplierInvoiceCostTransfer(param, recordType, recordId);

            return supplierInvoiceCostTransfer.ToXDocument();
        }
        public XDocument SaveSupplierInvoiceCostTransfer(MobileParam param, int invoiceId, int recordType, int recordId, int orderId, int projectId, int timeCodeId, int employeeId, decimal amount, decimal supplementCharge, bool chargeCostToProject, bool includeSupplierInvoiceImage, int state)
        {
            MobileSupplierInvoiceCostTransfer supplierInvoiceCostTransfer = PerformSaveSupplierInvoiceCostTransfer(param, invoiceId, recordType, recordId, orderId, projectId, timeCodeId, employeeId, amount, supplementCharge, chargeCostToProject, includeSupplierInvoiceImage, state);

            return supplierInvoiceCostTransfer.ToXDocument();
        }

        public XDocument SupplierInvoiceBlockPayment(MobileParam param, int invoiceId, bool blockPayment, string comment)
        {
            //Perform
            var result = PerformSupplierInvoiceBlockPayment(param, invoiceId, blockPayment, comment);

            return result.ToXDocument();
        }

        public XDocument SearchOrder(MobileParam param, string orderNumber, string customer, int projectId)
        {
            MobileOrderRowsSearch mobileOrderRows = PerformSearchOrder(param, orderNumber, customer, projectId);

            return mobileOrderRows.ToXDocument();
        }
        public XDocument SearchProject(MobileParam param, string project, string customer, bool includeClosed)
        {
            MobileProjects mobileProjects = PerformSearchProject(param, project, customer, includeClosed);

            return mobileProjects.ToXDocument();
        }
        public XDocument SaveAttestWorkFlowAnswer(MobileParam param, int invoiceId, int attestWorkFlowHeadId, int attestWorkFlowRowId, bool answer, string comment)
        {
            //Perform
            MobileAttestInvoice mobileAttestInvoice = PerformSaveAttestWorkFlowAnswer(param, invoiceId, attestWorkFlowHeadId, attestWorkFlowRowId, answer, comment);

            return mobileAttestInvoice.ToXDocument(MobileTask.SaveAttestWorkFlowAnswer);
        }

        public XDocument GetDimAccounts(MobileParam param, int dimNr)
        {
            var dimAccounts = PerformGetDimAccounts(param, dimNr);

            return dimAccounts.ToXDocument();
        }

        public XDocument SaveSupplierAccountRow(MobileParam param, int invoiceId, int rowId, int dim1AccountId, int dim2AccountId, int dim3AccountId, int dim4AccountId, int dim5AccountId, int dim6AccountId, decimal debetAmount, decimal creditAmount)
        {
            var result = PerformSaveSupplierAccountRow(param, invoiceId, rowId, dim1AccountId, dim2AccountId, dim3AccountId, dim4AccountId, dim5AccountId, dim6AccountId, debetAmount, creditAmount);

            return result.ToXDocument(MobileTask.SaveAttestAccountRow);
        }

        public XDocument GetTextBlockDictionary(MobileParam param, int dictionaryType)
        {
            //Perform
            MobileTextBlocks mobileTextBlockDictionaries = PerformGetTextBlockDictionary(param, dictionaryType);

            return mobileTextBlockDictionaries.ToXDocument();
        }

        public XDocument CopyMoveOrderRows(MobileParam param, int orderId, int newOrderId, string orderRowIdsAndQuantity, int type)
        {
            //Perform
            MobileOrderRow mobileOrdeRow = PerformCopyMoveOrderRows(param, orderId, newOrderId, orderRowIdsAndQuantity, type);

            return mobileOrdeRow.ToXDocument(MobileTask.CopyOrderRow);
        }

        public XDocument GetCopyMoveOrders(MobileParam param)
        {
            //Perform
            MobileCopyMoveOrders mobileOrders = PerformGetCopyMoveOrders(param);

            return mobileOrders.ToXDocument();
        }

        public XDocument GetPlanningData(MobileParam param, int orderId)
        {
            //Perform
            MobilePlanningData planningData = PerformGetPlanningData(param, orderId);

            return planningData.ToXDocument();
        }

        public XDocument SavePlanningData(MobileParam param, int orderId, int? shiftType, DateTime? plannedStartDate, DateTime? plannedStopDate, int estimatedTime, int remainingTime, bool keepAsPlanned, int? priority)
        {
            //Perform
            MobilePlanningData result = PerformSavePlanningData(param, orderId, shiftType, plannedStartDate, plannedStopDate, estimatedTime, remainingTime, keepAsPlanned, priority);

            return result.ToXDocument(MobileTask.SavePlanningData);
        }

        public XDocument GetOrderShiftTypes(MobileParam param)
        {
            //Perform
            MobileShiftTypes shiftTypes = PerformGetOrderShiftTypes(param);

            return shiftTypes.ToXDocument();
        }

        public XDocument GetOrderShiftOrders(MobileParam param, DateTime? anyDateInWeek)
        {
            //Perform
            MobileDicts result = PerformGetOrderShiftOrders(param, anyDateInWeek);

            return result.ToXDocument();
        }

        public XDocument ReloadOrderPlanningSchedule(MobileParam param, int employeeId, DateTime date)
        {
            //Perform
            MobileReloadOrderPlanningSchedule result = PerformReloadOrderPlanningSchedule(param, employeeId, date);

            return result.ToXDocument();
        }

        public XDocument GetOrderShift(MobileParam param, int shiftId, int orderId, int employeeId, DateTime date)
        {
            //Perform
            MobileOrderShift result = PerformGetOrderShift(param, shiftId, orderId, employeeId, date);

            return result.ToXDocument();
        }

        public XDocument SaveOrderShiftValidateSkills(MobileParam param, int employeeId, DateTime stopTime, int shiftTypeId)
        {
            //Perform
            MobileMessageBox result = PerformSaveOrderShiftValidateSkills(param, employeeId, stopTime, shiftTypeId);

            return result.ToXDocument();
        }

        public XDocument SaveOrderShiftValidateWorkRules(MobileParam param, int shiftId, int orderId, int employeeId, DateTime startTime, DateTime stopTime, int shiftTypeId)
        {
            //Perform
            MobileMessageBox result = PerformSaveOrderShiftValidateWorkRules(param, shiftId, orderId, employeeId, startTime, stopTime, shiftTypeId);

            return result.ToXDocument();
        }

        public XDocument SaveOrderShift(MobileParam param, int shiftId, int orderId, int employeeId, DateTime startTime, DateTime stopTime, int shiftTypeId)
        {
            //Perform
            MobileOrderShift result = PerformSaveOrderShift(param, shiftId, orderId, employeeId, startTime, stopTime, shiftTypeId);

            return result.ToXDocument(MobileTask.SaveOrderShift);
        }

        public XDocument DeleteOrder(MobileParam param, int orderId, bool deleteProject)
        {
            //Perform
            var result = PerformDeleteOrder(param, orderId, deleteProject);

            return result.ToXDocument(MobileTask.Delete);
        }

        #region Accounting on orderrow (only Professional)

        public XDocument GetAccountSettings(MobileParam param)
        {
            MobileAccountSettings settings = PerfromGetAccountSettings(param);

            return settings.ToXDocument();
        }

        public XDocument GetAccounts(MobileParam param)
        {
            MobileAccounts accounts = PerfromGetAccounts(param);

            return accounts.ToXDocument();
        }

        public XDocument GetOrderRowAccounts(MobileParam param)
        {
            MobileAccounts accounts = PerfromGetOrderRowAccounts(param);

            return accounts.ToXDocument();
        }

        #endregion

        #region Settings

        public XDocument GetOrderGridFieldSettings(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderGridFieldSettings.GetDefaultXml());

            //Perform
            MobileOrderGridFieldSettings mobileOrderGridFieldSettings = PerformGetOrderGridFieldSettings(param);

            return mobileOrderGridFieldSettings.ToXDocument();
        }

        public XDocument GetOrderEditFieldSettings(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderEditFieldSettings.GetDefaultXml());

            //Perform
            MobileOrderEditFieldSettings mobileOrderEditFieldSettings = PerformGetOrderEditFieldSettings(param);

            return mobileOrderEditFieldSettings.ToXDocument();
        }

        public XDocument GetCustomerGridFieldSettings(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCustomerGridFieldSettings.GetDefaultXml());

            //Perform
            MobileCustomerGridFieldSettings mobileCustomerGridFieldSettings = PerformGetCustomerGridFieldSettings(param);

            return mobileCustomerGridFieldSettings.ToXDocument();
        }

        public XDocument GetCustomerEditFieldSettings(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileCustomerEditFieldSettings.GetDefaultXml());

            //Perform
            MobileCustomerEditFieldSettings mobileCustomerEditFieldSettings = PerformGetCustomerEditFieldSettings(param);

            return mobileCustomerEditFieldSettings.ToXDocument();
        }

        #endregion

        #endregion

        #region Time

        public XDocument GetEmployee(MobileParam param)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileEmployee.GetDefaultXml());

            //Perform
            MobileEmployee mobileEmployee = PerformGetEmployee(param);
            return mobileEmployee.ToXDocument();
        }

        public XDocument GetAttestTreeEmployeesForAdmin(MobileParam param, DateTime dateFrom, DateTime dateTo, bool includeAdditionalEmployees, bool includeIsAttested, string employeeIdsStr)
        {
            //Perform
            MobileDicts mobileEmployees = PerformGetAttestTreeEmployeesForAdmin(param, dateFrom, dateTo, includeAdditionalEmployees, includeIsAttested, employeeIdsStr);
            return mobileEmployees.ToXDocument();
        }

        public XDocument GetPayrollPeriods(MobileParam param, int employeeId, MobileDisplayMode displayMode)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileTimePeriods.GetDefaultXml());

            //Perform
            MobileTimePeriods mobileTimePeriods = PerformGetPayrollPeriods(param, employeeId, displayMode);
            return mobileTimePeriods.ToXDocument();
        }

        public XDocument GetAttestEmployeeDays(MobileParam param, int employeeId, int employeeGroupId, int timePeriodId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileAttestEmployeeDays.GetDefaultXml());

            //Perform
            MobileAttestEmployeeDays mobileAttestEmployeeDays = PerformGetAttestEmployeeDays(param, employeeId, employeeGroupId, timePeriodId);
            return mobileAttestEmployeeDays.ToXDocument();
        }

        public XDocument GetAttestEmployeePeriods(MobileParam param, DateTime dateFrom, DateTime dateTo, bool includeAdditionalEmployees, string employeeIds)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileAttestEmployeePeriods.GetDefaultXml());

            //Perform
            MobileAttestEmployeePeriods mobileAttestEmployeePeriods = PerformGetAttestEmployeePeriods(param, dateFrom, dateTo, includeAdditionalEmployees, employeeIds);
            return mobileAttestEmployeePeriods.ToXDocument();
        }

        public XDocument GetAttestEmployeeDaysAdmin(MobileParam param, DateTime dateFrom, DateTime dateTo, int employeeId, string filterAccountIds)
        {
            //Perform
            MobileAttestEmployeeDays mobileAttestEmployeeDays = PerformGetAttestEmployeeDaysAdmin(param, dateFrom, dateTo, employeeId, filterAccountIds);
            return mobileAttestEmployeeDays.ToXDocument();
        }
        public XDocument GetAttestInfoMessage(MobileParam param, string info)
        {
            //Perform
            MobileResult result = PerformGetAttestInfoMessage(param, info);
            return result.ToXDocument(MobileTask.GetMessage);
        }

        public XDocument GetAttestWarningMessage(MobileParam param, string warnings)
        {
            //Perform
            MobileResult result = PerformGetAttestWarningMessage(param, warnings);
            return result.ToXDocument(MobileTask.GetMessage);
        }

        public XDocument GetDayView(MobileParam param, DateTime date, int employeeId, int employeeGroupId, int schedulePeriodId, int actorCompanyId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileDayView.GetDefaultXml());

            //Perform
            MobileDayView dayView = PerformGetDayView(param, date, employeeId, employeeGroupId, schedulePeriodId, actorCompanyId);
            return dayView.ToXDocument();
        }

        public XDocument GetMyTimeOverview(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            //Perform
            MobileMyTimeOverview overview = PerformGetMyTimeOverview(param, employeeId, dateFrom, dateTo);
            return overview.ToXDocument();
        }

        public XDocument GetDeviationCauses(MobileParam param, int employeeId, int employeeGroupId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileDeviationCauses.GetDefaultXml());

            //Perform
            MobileDeviationCauses mobileDeviationCauses = PerformGetDeviationCauses(param, employeeId, employeeGroupId);
            return mobileDeviationCauses.ToXDocument();
        }

        public XDocument SaveWholeDayAbsence(MobileParam param, string dates, int deviationCauseId, int employeeId, int employeeChildId, string comment)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileAttestEmployeeDays.GetDefaultSaveXml());

            //Perform
            MobileAttestEmployeeDays mobileAttestEmployeeDays = PerformSaveWholeDayAbsence(param, dates, deviationCauseId, employeeId, employeeChildId, comment);
            return mobileAttestEmployeeDays.ToXDocument(MobileTask.SaveAbsence);
        }

        public XDocument SaveIntervalAbsence(MobileParam param, DateTime start, DateTime stop, DateTime displayedDate, int displayedTimeScheduleTemplatePeriodId, int deviationCauseId, int employeeId, string comment, int employeeChildId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileAttestEmployeeDays.GetDefaultSaveXml());

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_22))
            {
                MobileAttestEmployeeDays mobileAttestEmployeeDays = PerformSaveIntervalAbsenceDeprecated(param, start, stop, displayedDate, displayedTimeScheduleTemplatePeriodId, deviationCauseId, employeeId, comment, employeeChildId);
                return mobileAttestEmployeeDays.ToXDocument(MobileTask.SaveAbsence);
            }
            else
            {
                MobileResult result = PerformSaveIntervalAbsence(param, start, stop, displayedDate, displayedTimeScheduleTemplatePeriodId, deviationCauseId, employeeId, comment, employeeChildId);
                return result.ToXDocument();
            }
        }

        public XDocument SaveIntervalPresence(MobileParam param, DateTime start, DateTime stop, DateTime displayedDate, int displayedTimeScheduleTemplatePeriodId, int deviationCauseId, int employeeId, string comment)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileAttestEmployeeDays.GetDefaultSaveXml());

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_22))
            {

                MobileAttestEmployeeDays mobileAttestEmployeeDays = PerformSaveIntervalPresenceDeprecated(param, start, stop, displayedDate, displayedTimeScheduleTemplatePeriodId, deviationCauseId, employeeId, comment);
                return mobileAttestEmployeeDays.ToXDocument(MobileTask.SavePresence);
            }
            else
            {
                MobileResult result = PerformSaveIntervalPresence(param, start, stop, displayedDate, displayedTimeScheduleTemplatePeriodId, deviationCauseId, employeeId, comment);
                return result.ToXDocument();
            }
        }

        public XDocument SaveStartTime(MobileParam param, DateTime newStartTime, DateTime date, int timeScheduleTemplatePeriodId, int newDeviationCauseId, int timeBlockId, int employeeId, string comment, int employeeChildId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileDayView.GetDefaultSaveXml());

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_22))
            {

                MobileDayView dayView = PerformSaveStartTimeDeprecated(param, newStartTime, date, timeScheduleTemplatePeriodId, newDeviationCauseId, timeBlockId, employeeId, comment, employeeChildId);
                return dayView.ToXDocument(MobileTask.SaveDeviations);
            }
            else
            {
                MobileResult result = PerformSaveStartTime(param, newStartTime, date, timeScheduleTemplatePeriodId, newDeviationCauseId, timeBlockId, employeeId, comment, employeeChildId);
                return result.ToXDocument();
            }
        }

        public XDocument SaveStopTime(MobileParam param, DateTime newStopTime, DateTime date, int timeScheduleTemplatePeriodId, int newDeviationCauseId, int timeBlockId, int employeeId, string comment, int employeeChildId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileDayView.GetDefaultSaveXml());

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_22))
            {
                MobileDayView dayView = PerformSaveStopTimeDeprecated(param, newStopTime, date, timeScheduleTemplatePeriodId, newDeviationCauseId, timeBlockId, employeeId, comment, employeeChildId);
                return dayView.ToXDocument(MobileTask.SaveDeviations);
            }
            else
            {
                MobileResult result = PerformSaveStopTime(param, newStopTime, date, timeScheduleTemplatePeriodId, newDeviationCauseId, timeBlockId, employeeId, comment, employeeChildId);
                return result.ToXDocument();
            }
        }

        public XDocument GetBreaks(MobileParam param, DateTime date, int employeeId, int timeScheduleTemplatePeriodId, int actorCompanyId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileEmployee.GetDefaultXml());

            //Perform
            MobileBreaks mobileBreaks = PerformGetBreaks(param, date, employeeId, timeScheduleTemplatePeriodId, actorCompanyId);
            return mobileBreaks.ToXDocument();
        }

        public XDocument ModifyBreak(MobileParam param, DateTime date, int totalMinutes, int scheduleBlockId, int employeeId, int timeScheduleTemplatePeriodId, int timeCodeBreakId, int actorCompanyId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileEmployee.GetDefaultXml());

            //Perform
            MobileBreak mobileBreak = PerformModifyBreak(param, date, scheduleBlockId, employeeId, timeScheduleTemplatePeriodId, timeCodeBreakId, totalMinutes, actorCompanyId);
            return mobileBreak.ToXDocument(MobileTask.SaveBreak);
        }

        public XDocument GetAttestStates(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo, MobileDisplayMode displayMode, string attestStateIdsStr)
        {
            //Perform
            MobileDicts result = PerformGetAttestStates(param, employeeId, dateFrom, dateTo, displayMode, attestStateIdsStr);
            return result.ToXDocument();
        }

        public XDocument GetAttestStates(MobileParam param, int employeeId, int timePeriodId)
        {
            //Perform
            MobileDicts result = PerformGetAttestStates(param, employeeId, timePeriodId);
            return result.ToXDocument();
        }

        public XDocument SetSchedulePeriodAsReadyValidation(MobileParam param, int employeeId, int attestStateToId, string idsAndDates, bool isMySelf, string filterAccountIds)
        {
            //Perform
            MobileMessageBox result = PerformSetSchedulePeriodAsReadyValidation(param, employeeId, attestStateToId, idsAndDates, isMySelf, filterAccountIds);
            return result.ToXDocument();
        }

        public XDocument SaveAttestForEmployees(MobileParam param, string employeeIdsStr, int attestStateToId, DateTime dateFrom, DateTime dateTo)
        {
            MobileMessageBox result = PerformSaveAttestForEmployees(param, employeeIdsStr, attestStateToId, dateFrom, dateTo);
            return result.ToXDocument();
        }

        public XDocument SaveAttestForEmployee(MobileParam param, int employeeId, string idsAndDates, int attestStateToId, MobileDisplayMode mode)
        {
            MobileMessageBox result = PerformSaveAttestForEmployee(param, employeeId, idsAndDates, attestStateToId, mode);
            return result.ToXDocument();
        }

        public XDocument SetSchedulePeriodAsReady(MobileParam param, int employeeId, int employeeGroupId, string idsAndDates, int attestStateToId = 0)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileEmployee.GetDefaultXml());

            //Perform
            MobileAttestEmployeeDays mobileAttestEmployeeDays = PerformSetSchedulePeriodAsReady(param, employeeId, employeeGroupId, idsAndDates, attestStateToId);
            return mobileAttestEmployeeDays.ToXDocument(MobileTask.SaveAttest);
        }

        public XDocument SaveAttestForPeriod(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo, int attestStateId = 0)
        {
            //Perform
            MobileMessageBox result = PerformSaveAttestForPeriod(param, employeeId, dateFrom, dateTo, attestStateId);
            return result.ToXDocument();
        }

        public XDocument RestoreToSchedule(MobileParam param, DateTime date, int employeeId, int employeeGroupId, int schedulePeriodId, int actorCompanyId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileDayView.GetDefaultSaveXml());

            //Perform
            MobileDayView dayView = PerformRestoreToSchedule(param, date, employeeId, employeeGroupId, schedulePeriodId, actorCompanyId);
            return dayView.ToXDocument(MobileTask.RestoreToSchedule);
        }

        public XDocument RestoreDatesToSchedule(MobileParam param, string dates, int employeeId)
        {
            //Perform
            MobileResult mobileResult = PerformRestoreDatesToSchedule(param, dates, employeeId);
            return mobileResult.ToXDocument(MobileTask.Update);
        }

        public XDocument GetTimeTerminalsForUser(MobileParam param, DateTime date)
        {
            MobileTimeTerminals result = PerformGetTimeTerminalsForUser(param, date);
            return result.ToXDocument();
        }

        public XDocument GetTimeStampAttendance(MobileParam param)
        {
            //Perform
            MobileTimeStampAttendancies attendance = PerformGetTimeStampAttendance(param);

            return attendance.ToXDocument();
        }
        public XDocument GetEvacuationList(MobileParam param)
        {
            //Perform
            EvacuationLists evacuationList = PerformGetEvacuationList(param);

            return evacuationList.ToXDocument();
        }
        public XDocument UpdateEvacuationListMarkings(MobileParam param, string employeeLíst, int headId = 0)
        {
            //Perform
            UpdateEvacuationList evacuationList = PerformUpdateEvacuationListMarkings(param, employeeLíst, headId);

            return evacuationList.ToXDocument();
        }
        public XDocument GetEvacuationListHistory(MobileParam param, int employeeId)
        {
            //Perform
            EvacuationListHistorys evacuationListHistorys = PerformGetEvacuationListHistory(param, employeeId);

            return evacuationListHistorys.ToXDocument();
        }
        
        public XDocument GetTimeWorkAccountOptions(MobileParam param, int employeeId, int timeWorkAccountYearEmployeeId)
        {
            //Perform
            TimeWorkAccountOptions timeWorkAccountOptions = PerformGetTimeWorkAccountOptions(param, employeeId, timeWorkAccountYearEmployeeId);

            return timeWorkAccountOptions.ToXDocument();
        }
        public XDocument SetTimeWorkAccountOption(MobileParam param, int employeeId, int timeWorkAccountYearEmployeeId, int selectedWithdrawalMethod)
        {
            //Perform
            MobileResult result = PerformSetTimeWorkAccountOption(param, employeeId, timeWorkAccountYearEmployeeId, selectedWithdrawalMethod);

            return result.ToXDocument();
        }

        #region Permissions/settings

        public XDocument GetTimePermissions(MobileParam param, MobileDisplayMode mobileDisplayMode)
        {
            //Perform
            MobilePermissions permissions = PerformGetTimePermissions(param, mobileDisplayMode);
            return permissions.ToXDocument();
        }

        public XDocument GetCommonPermissions(MobileParam param)
        {
            //Perform
            MobilePermissions permissions = PerformGetCommonPermissions(param);
            return permissions.ToXDocument();
        }

        #endregion

        #endregion

        #region Staffing

        #region Common collections/lists

        public XDocument GetBreakTimeCodes(MobileParam param, int employeeId, DateTime? date)
        {
            MobileBreakTimeCodes timeCodes = PerformGetBreakTimeCodes(param, employeeId, date);

            return timeCodes.ToXDocument();
        }

        public XDocument GetScheduleTypes(MobileParam param)
        {
            MobileSheduleTypes scheduleTypes = PerformGetScheduleTypes(param);

            return scheduleTypes.ToXDocument();
        }

        #endregion

        #region Shift Actions

        public XDocument DeleteShift(MobileParam param, int shiftId, int employeeId, bool includeLinkedShifts)
        {
            MobileExtendedShift shift = PerformDeleteShift(param, shiftId, employeeId, includeLinkedShifts);

            return shift.ToXDocument(MobileTask.DeleteShift);
        }

        public XDocument SetShiftAsWanted(MobileParam param, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileShift shift = PerformSetShiftAsWanted(param, employeeId, employeeGroupId, shiftId);

            return shift.ToXDocument(MobileTask.SetShiftAsWanted);
        }

        public XDocument UpdateShiftSetUndoWanted(MobileParam param, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileShift shift = PerformUpdateShiftSetUndoWanted(param, employeeId, employeeGroupId, shiftId);

            return shift.ToXDocument(MobileTask.UpdateShiftSetUndoWanted);
        }

        public XDocument SetShiftAsUnWanted(MobileParam param, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileShift shift = PerformSetShiftAsUnWanted(param, employeeId, employeeGroupId, shiftId);

            return shift.ToXDocument(MobileTask.SetShiftAsUnWanted);
        }

        public XDocument UpdateShiftSetUndoUnWanted(MobileParam param, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileShift shift = PerformUpdateShiftSetUndoUnWanted(param, employeeId, employeeGroupId, shiftId);

            return shift.ToXDocument(MobileTask.UpdateShiftSetUndoUnWanted);
        }

        public XDocument WantShiftValidateSkills(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox messageBox = PerformWantShiftValidateSkills(param, shiftId, employeeId);

            return messageBox.ToXDocument();
        }

        public XDocument GetShiftTasks(MobileParam param, int shiftId)
        {
            MobileShiftTasks list = PerformGetShiftTasks(param, shiftId);

            return list.ToXDocument();
        }

        #endregion

        #region Edit shift dialog

        public XDocument GetShiftTypesForEditShiftView(MobileParam param, string selectedAccountIdsStr)
        {
            MobileShiftTypes shiftTypes = PerformGetShiftTypesForEditShiftView(param, selectedAccountIdsStr);

            return shiftTypes.ToXDocument();
        }

        public XDocument SaveShiftsValidateWorkRules(MobileParam param, int employeeId, DateTime actualDate, string shifts)
        {
            MobileMessageBox messageBox = PerformSaveShiftsValidateWorkRules(param, employeeId, actualDate, shifts);

            return messageBox.ToXDocument();
        }

        public XDocument SaveShiftsValidateSkills(MobileParam param, int employeeId, DateTime actualDate, string shifts)
        {
            MobileMessageBox messageBox = PerformSaveShiftsValidateSkills(param, employeeId, actualDate, shifts);

            return messageBox.ToXDocument();
        }

        public XDocument GetShifts(MobileParam param, int shiftId, int employeeId, DateTime date)
        {
            MobileExtendedShifts shifts = PerformGetShifts(param, shiftId, employeeId, date);

            return shifts.ToXDocument();
        }

        public XDocument SaveShifts(MobileParam param, int employeeId, DateTime actualDate, string shifts)
        {
            MobileExtendedShift extendedShift = PerformSaveShifts(param, employeeId, actualDate, shifts);

            return extendedShift.ToXDocument(MobileTask.SaveShifts);
        }

        public XDocument GetCreateNewShiftsEmployeesNew(MobileParam param, DateTime date, string shifts, bool filterOnShiftType, bool filterOnAvailability, bool filterOnSkills, bool filterOnWorkRules)
        {
            MobileEmployeeList employees = PerformGetCreateNewShiftsEmployeesNew(param, date, shifts, filterOnShiftType, filterOnAvailability, filterOnSkills, filterOnWorkRules);

            return employees.ToXDocument();
        }

        #endregion

        #region Swap shift
        public XDocument GetScheduleSwapApproveView(MobileParam param, int timeScheduleSwapRequestId, int employeeId, int userId)
        {
            MobileScheduleSwapApproveView result = PerformMobileScheduleSwapApproveView(param, timeScheduleSwapRequestId, employeeId, userId);
            return result.ToXDocument();
        }
        public XDocument GetShiftsForSwap(MobileParam param, string initiatorShiftIds, string swapShiftIds, int employeeId, int employeeIdToView, DateTime initatorShiftDate, DateTime swapWithShiftDate, MobileDisplayMode mobileDisplayMode)
        {
            MobileShifts shifts = PerformGetShiftsForSwap(param, initiatorShiftIds, swapShiftIds, employeeId, employeeIdToView, initatorShiftDate, swapWithShiftDate, mobileDisplayMode);
            return shifts.ToXDocument();
        }

        public XDocument InitiateScheduleSwap(MobileParam param, int initiatorEmployeeId, DateTime initiatorShiftDate, string initiatorShiftIds, int swapWithEmployeeId, DateTime swapShiftDate, string swapWithShiftIds, string comment)
        {
            MobileResult result = PerformInitiateScheduleSwap(param, initiatorEmployeeId, initiatorShiftDate, initiatorShiftIds, swapWithEmployeeId, swapShiftDate, swapWithShiftIds, comment);
            return result.ToXDocument();
        }

        public XDocument GetScheduleSwapAvailableEmployees(MobileParam param, int initiatorEmployeeId, DateTime initiatorShiftDate, DateTime swapShiftDate)
        {
            MobileDicts result = PerformGetScheduleSwapAvailableEmployees(param, initiatorEmployeeId, initiatorShiftDate, swapShiftDate);
            return result.ToXDocument();
        }

        public XDocument ApproveScheduleSwap(MobileParam param, int userId, int timeScheduleSwapRequestId, bool approved, string comment)
        {
            MobileResult result = PerformApproveScheduleSwap(param, userId, timeScheduleSwapRequestId, approved, comment);
            return result.ToXDocument();
        }
        public XDocument AssignScheduleSwapValidateWorkRules(MobileParam param, int timeScheduleSwapRequestId)
        {
            MobileMessageBox messageBox = PerformAssignScheduleSwapValidateWorkRules(param, timeScheduleSwapRequestId);

            return messageBox.ToXDocument();
        }
        public XDocument GetScheduleSwapValidateLengths(MobileParam param, string sourceScheduleBlockIds, string targetScheduleBlockIds)
        {
            MobileMessageBox messageBox = PerformGetScheduleSwapValidateLengths(param, sourceScheduleBlockIds, targetScheduleBlockIds);

            return messageBox.ToXDocument();
        }
        public XDocument GetScheduleSwapValidateLengthsFromRequest(MobileParam param, int timeScheduleSwapRequestId, int employeeId)
        {
            MobileMessageBox messageBox = PerformGetScheduleSwapValidateLengthsFromRequest(param, timeScheduleSwapRequestId, employeeId);

            return messageBox.ToXDocument();
        }
        #endregion

        #region Views

        #region Employee

        public XDocument GetShiftFlow(MobileParam param, int employeeId, DateTime dateFrom)
        {
            MobileShifts shifts = PerformGetShiftFlow(param, employeeId, dateFrom);
            return shifts.ToXDocument();
        }

        public XDocument GetScheduleViewEmployee(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            MobileScheduleViewMonth scheduleView = PerformGetScheduleViewEmployee(param, employeeId, dateFrom, dateTo);

            return scheduleView.ToXDocument();
        }

        public XDocument GetScheduleOverviewGroupedByEmployee(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo, bool includeSecondaryCategoriesOrAccounts, string employeeIdsStr, string shiftTypeIdsStr)
        {
            MobileScheduleViewWeek view = PerformGetScheduleOverviewGroupedByEmployeeForEmployee(param, employeeId, dateFrom, dateTo, includeSecondaryCategoriesOrAccounts, employeeIdsStr, shiftTypeIdsStr);

            return view.ToXDocument();
        }

        public XDocument GetScheduleDayViewEmployee(MobileParam param, int employeeId, DateTime date, bool includeSecondaryCategoriesOrAccounts, string employeeIdsStr, string shiftTypeIdsStr)
        {
            MobileScheduleViewDay dayView = PerformGetScheduleDayViewEmployee(param, employeeId, date, includeSecondaryCategoriesOrAccounts, employeeIdsStr, shiftTypeIdsStr);

            return dayView.ToXDocument();
        }

        #endregion

        #region Admin

        public XDocument GetScheduleOverviewGroupedByEmployeeAdmin(MobileParam param, DateTime dateFrom, DateTime dateTo, bool includeSecondaryCategoriesOrAccounts, string employeeIdsStr, string shiftTypeIdsStr, string accountIdsStr, bool includeUnscheduledEmployees)
        {
            MobileScheduleViewWeek shiftsOverview = PerformGetScheduleOverviewGroupedByEmployeeAdmin(param, dateFrom, dateTo, includeSecondaryCategoriesOrAccounts, employeeIdsStr, shiftTypeIdsStr, accountIdsStr, includeUnscheduledEmployees);

            return shiftsOverview.ToXDocument();
        }

        public XDocument GetScheduleDayViewAdmin(MobileParam param, DateTime date, bool includeSecondaryCategoriesOrAccounts, string employeeIdsStr, string shiftTypeIdsStr, string accountIdsStr, bool includeUnscheduledEmployees)
        {
            MobileScheduleViewDay dayView = PerformGetScheduleDayViewAdmin(param, date, includeSecondaryCategoriesOrAccounts, employeeIdsStr, shiftTypeIdsStr, accountIdsStr, includeUnscheduledEmployees);

            return dayView.ToXDocument();
        }

        #endregion

        #endregion

        #region Pop-up (Shifts)
        public XDocument GetScheduleDayEmployee(MobileParam param, int employeeId, DateTime date)
        {
            MobileShifts shifts = PerformGetScheduleDayEmployee(param, employeeId, date);

            return shifts.ToXDocument();
        }

        public XDocument GetAvailableShiftsNew(MobileParam param, int employeeId, DateTime date, string link, bool includeSecondaryCategoriesOrAccounts, MobileDisplayMode mobileDisplayMode)
        {
            MobileShifts shifts = PerformGetAvailableShiftsNew(param, employeeId, date, link, includeSecondaryCategoriesOrAccounts, mobileDisplayMode);

            return shifts.ToXDocument();
        }

        public XDocument GetOthersShiftsNew(MobileParam param, int employeeId, int employeeIdToView, DateTime date, bool includeSecondaryCategoriesOrAccounts, MobileDisplayMode mobileDisplayMode)
        {
            MobileShifts shifts = PerformGetOthersShiftsNew(param, employeeId, employeeIdToView, date, includeSecondaryCategoriesOrAccounts, mobileDisplayMode);
            return shifts.ToXDocument();
        }

        #endregion

        #region Filter

        public XDocument GetSchedulePlanningEmployeesForEmployee(MobileParam param, int employeeId, bool includeSecondaryCategoriesOrAccounts, string employeeIds)
        {
            MobileDicts dicts = PerformGetSchedulePlanningEmployeesForEmployee(param, employeeId, includeSecondaryCategoriesOrAccounts, employeeIds);
            return dicts.ToXDocument();
        }

        public XDocument GetSchedulePlanningShiftTypesForEmployee(MobileParam param, int employeeId, bool includeSecondaryCategoriesOrAccounts, string shiftTypeIdsStr)
        {
            MobileDicts dicts = PerformGetSchedulePlanningShiftTypesForEmployee(param, employeeId, includeSecondaryCategoriesOrAccounts, shiftTypeIdsStr);
            return dicts.ToXDocument();
        }

        public XDocument GetSchedulePlanningEmployeesForAdmin(MobileParam param, bool includeSecondaryCategoriesOrAccounts, string accountIdsStr, string employeeIdsStr)
        {
            MobileDicts dicts = PerformGetSchedulePlanningEmployeesForAdmin(param, includeSecondaryCategoriesOrAccounts, accountIdsStr, employeeIdsStr);
            return dicts.ToXDocument();
        }

        public XDocument GetSchedulePlanningShiftTypesForAdmin(MobileParam param, bool includeSecondaryCategoriesOrAccounts, string accountIdsStr, string shiftTypeIdsStr)
        {
            MobileDicts dicts = PerformGetSchedulePlanningShiftTypesForAdmin(param, includeSecondaryCategoriesOrAccounts, accountIdsStr, shiftTypeIdsStr);
            return dicts.ToXDocument();
        }

        public XDocument GetMessageGroups(MobileParam param)
        {
            var messageGroups = PerformGetMessageGroups(param);
            return messageGroups.ToXDocument();
        }

        #endregion

        #region Available employees

        public XDocument GetAvailableEmployeesNew(MobileParam param, int shiftId, bool filterOnShiftType, bool filterOnAvailability, bool filterOnSkills, bool filterOnWorkRules, bool shiftRequest, bool absenceRequest, DateTime absenceStartTime, DateTime absenceStopTime, int? filterOnMessageGroupId)
        {
            MobileEmployeeList list = PerformGetAvailableEmployeesNew(param, shiftId, filterOnShiftType, filterOnAvailability, filterOnSkills, filterOnWorkRules, shiftRequest, absenceRequest, absenceStartTime, absenceStopTime, filterOnMessageGroupId: filterOnMessageGroupId);
            return list.ToXDocument();
        }

        #endregion

        #region AbsenceAnnouncement

        public XDocument GetAbsenceAnnouncementCauses(MobileParam param, int employeeId, int employeeGroupId)
        {
            MobileDeviationCauses causes = PerformGetAbsenceAnnouncementCauses(param, employeeId, employeeGroupId);

            return causes.ToXDocument();
        }

        public XDocument SaveAbsenceAnnouncement(MobileParam param, DateTime date, int employeeId, int timeDeviationCauseId)
        {
            MobileAbsenceAnnouncement announcement = PerformSaveAbsenceAnnouncement(param, date, employeeId, timeDeviationCauseId);

            return announcement.ToXDocument(MobileTask.SaveAbsenceAnnouncement);
        }

        #endregion

        #region Absence planning

        public XDocument GetAbsencePlanningDeviationCauses(MobileParam param, int employeeId, bool wholeDay)
        {
            MobileDeviationCauses causes = PerformGetAbsencePlanningDeviationCauses(param, employeeId, wholeDay);

            return causes.ToXDocument();
        }

        public XDocument GetReplaceWithEmployees(MobileParam param, bool addSearchEmployee)
        {
            MobileReplaceWithEmployees employees = PerformGetReplaceWithEmployees(param, addSearchEmployee);

            return employees.ToXDocument();
        }

        public XDocument GetAbsenceAffectedShiftsOpenDialog(MobileParam param, int employeeId, int shiftId, DateTime date, bool wholeDay)
        {
            MobileExtendedShifts shifts = PerformGetAbsenceAffectedShiftsOpenDialog(param, employeeId, shiftId, date, wholeDay);

            return shifts.ToXDocument();
        }

        public XDocument GetAbsenceAffectedShifts(MobileParam param, int employeeId, int deviationCauseId, DateTime from, DateTime to, DateTime absenceFrom, DateTime absenceTo, bool wholeDay, bool isTimeModule)
        {
            MobileExtendedShifts shifts = PerformGetAbsenceAffectedShifts(param, employeeId, deviationCauseId, from, to, absenceFrom, absenceTo, wholeDay, isTimeModule);

            return shifts.ToXDocument();
        }

        public XDocument AbsencePlanningValidateWorkRules(MobileParam param, int employeeId, string shifts, bool isTimeModule)
        {
            MobileMessageBox messageBox = PerformAbsencePlanningValidateWorkRules(param, employeeId, shifts, isTimeModule);

            return messageBox.ToXDocument();
        }

        public XDocument SaveAbsencePlanning(MobileParam param, int employeeId, int timeDeviationcauseId, string shifts, int employeeChildId, bool isTimeModule)
        {
            MobileExtendedShift shift = PerformSaveAbsencePlanning(param, employeeId, timeDeviationcauseId, employeeChildId, shifts, isTimeModule);

            return shift.ToXDocument(MobileTask.SaveAbsencePlanning);
        }
        public XDocument AbsencePlanningValidateSkills(MobileParam param, int employeeId, int timeDeviationcauseId, string shifts)
        {
            MobileMessageBox messageBox = PerformAbsencePlanningValidateSkills(param, employeeId, timeDeviationcauseId, shifts);

            return messageBox.ToXDocument();
        }

        #endregion

        #region AbsenceRequest/InterestRequest

        #region Employee

        public XDocument GetInterestRequests(MobileParam param, int employeeId, DateTime? dateFrom, DateTime? dateTo)
        {
            MobileRequests causes = PerformGetInterestRequests(param, employeeId, dateFrom, dateTo);

            return causes.ToXDocument();
        }

        public XDocument GetInterestRequest(MobileParam param, int interestRequestId)
        {

            MobileRequest request = PerformGetInterestRequestEmployee(param, interestRequestId);

            return request.ToXDocument();
        }

        public XDocument SaveInterestRequest(MobileParam param, int employeeId, int interestRequestIdint, DateTime dateFrom, DateTime dateTo, bool available, string note, bool isAdmin)
        {
            MobileRequest request = PerformSaveInterestRequest(param, employeeId, interestRequestIdint, dateFrom, dateTo, available, note, isAdmin);

            return request.ToXDocument(MobileTask.SaveEmployeeRequest);
        }

        public XDocument DeleteInterestRequest(MobileParam param, int employeeId, int employeeGroupId, int interestRequestId)
        {
            MobileRequest request = PerformDeleteInterestRequest(param, employeeId, employeeGroupId, interestRequestId);

            return request.ToXDocument(MobileTask.DeleteEmployeeRequest);
        }

        public XDocument GetAbsenceRequestCauses(MobileParam param, int employeeId, int employeeGroupId)
        {
            MobileDeviationCauses causes = PerformGetAbsenceRequestCauses(param, employeeId, employeeGroupId);

            return causes.ToXDocument();
        }

        public XDocument GetAbsenceRequests(MobileParam param, int employeeId, DateTime? dateFrom, DateTime? dateTo)
        {

            MobileRequests requests = PerformGetAbsenceRequestsEmployee(param, employeeId, dateFrom, dateTo);

            return requests.ToXDocument();
        }

        public XDocument GetAbsenceRequest(MobileParam param, int absenceRequestId)
        {
            MobileRequest request = PerformGetAbsenceRequestEmployee(param, absenceRequestId);

            return request.ToXDocument();
        }

        public XDocument SaveAbsenceRequest(MobileParam param, int employeeId, int absenceRequestId, DateTime dateFrom, DateTime dateTo, int deviationCauseId, string note, bool absenceWholeDays, int employeeChildId)
        {
            MobileRequest request = PerformSaveAbsenceRequest(param, employeeId, absenceRequestId, dateFrom, dateTo, deviationCauseId, note, absenceWholeDays, employeeChildId);

            return request.ToXDocument(MobileTask.SaveEmployeeRequest);
        }

        public XDocument ValidateAbsenceRequestPolicy(MobileParam param, int employeeId, int absenceRequestId, DateTime dateFrom, DateTime dateTo, int deviationCauseId)
        {
            MobileMessageBox messageBox = PerformValidateAbsenceRequestPolicy(param, employeeId, absenceRequestId, dateFrom, dateTo, deviationCauseId);

            return messageBox.ToXDocument();
        }

        public XDocument DeleteAbsenceRequest(MobileParam param, int absenceRequestId)
        {
            MobileRequest request = PerformDeleteAbsenceRequest(param, absenceRequestId);

            return request.ToXDocument(MobileTask.DeleteEmployeeRequest);
        }

        #endregion

        #region Admin

        public XDocument GetAbsenceRequestsAdmin(MobileParam param, bool includeDefinitive)
        {
            MobileRequests requests = PerformGetAbsenceRequestsAdmin(param, includeDefinitive);

            return requests.ToXDocument();
        }

        public XDocument GetAbsenceRequestAdmin(MobileParam param, int absenceRequestId)
        {
            MobileRequest request = PerformGetAbsenceRequestAdmin(param, absenceRequestId);

            return request.ToXDocument();
        }

        public XDocument GetApprovalTypes(MobileParam param)
        {
            MobileDicts approvalTypes = PerformGetApprovalTypes(param);

            return approvalTypes.ToXDocument();
        }

        public XDocument ValidateAbsenceRequestPlanning(MobileParam param, int absenceRequestId, string shifts)
        {
            MobileMessageBox messageBox = PerformValidateAbsenceRequestPlanning(param, absenceRequestId, shifts);

            return messageBox.ToXDocument();
        }

        public XDocument SaveAbsenceRequestPlanning(MobileParam param, int absenceRequestId, string shifts, string comment)
        {
            MobileAbsenceRequestShift result = PerformSaveAbsenceRequestPlanning(param, absenceRequestId, shifts, comment);

            return result.ToXDocument(MobileTask.SaveAbsenceRequestPlanning);
        }

        #endregion

        #endregion

        #region AssignAvailableShift from queue

        public XDocument AssignAvailableShiftFromQueueValidateWorkRules(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox messageBox = PerformAssignAvailableShiftFromQueueValidateWorkRules(param, shiftId, employeeId);

            return messageBox.ToXDocument();
        }

        public XDocument AssignAvailableShiftFromQueueValidateSkills(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox messageBox = PerformAssignAvailableShiftFromQueueValidateSkills(param, shiftId, employeeId);

            return messageBox.ToXDocument();
        }

        public XDocument AssignAvailableShiftFromQueue(MobileParam param, int shiftId, int employeeId)
        {
            MobileShift result = PerformAssignAvailableShiftFromQueue(param, shiftId, employeeId);

            return result.ToXDocument(MobileTask.AssignAvailableShift);
        }

        public XDocument GetShiftQueue(MobileParam param, int shiftId)
        {
            MobileShiftQueueList list = PerformGetShiftQueue(param, shiftId);

            return list.ToXDocument();
        }

        #endregion

        #region AssignAvailableShift

        public XDocument AssignAvailableShiftValidateWorkRules(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox messageBox = PerformAssignShiftValidateWorkRules(param, shiftId, employeeId);

            return messageBox.ToXDocument();
        }

        public XDocument AssignAvailableShiftValidateSkills(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox messageBox = PerformAssignShiftValidateSkills(param, shiftId, employeeId);

            return messageBox.ToXDocument();
        }

        public XDocument AssignAvailableShift(MobileParam param, int shiftId, int employeeId)
        {
            MobileShift result = PerformAssignShift(param, shiftId, employeeId);

            return result.ToXDocument(MobileTask.AssignAvailableShift);
        }

        #endregion

        #region AssignOthersShift

        public XDocument AssignOthersShiftValidateWorkRules(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox messageBox = PerformAssignShiftValidateWorkRules(param, shiftId, employeeId);

            return messageBox.ToXDocument();
        }

        public XDocument AssignOthersShiftValidateSkills(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox messageBox = PerformAssignShiftValidateSkills(param, shiftId, employeeId);

            return messageBox.ToXDocument();
        }

        public XDocument AssignOthersShift(MobileParam param, int shiftId, int employeeId)
        {
            MobileShift result = PerformAssignShift(param, shiftId, employeeId);

            return result.ToXDocument(MobileTask.AssignAvailableShift);
        }

        #endregion

        #region ShiftRequest

        public XDocument GetShiftRequestUsers(MobileParam param, int shiftId, int employeeId)
        {
            MobileShiftRequestUsers users = PerformGetShiftRequestUsers(param, shiftId, employeeId);

            return users.ToXDocument();
        }

        public XDocument SendShiftRequest(MobileParam param, int shiftId, int employeeId, string comment, string userIds, bool overrided, string overrideData)
        {
            MobileXeMail xemail = PerformSendShiftRequest(param, shiftId, employeeId, comment, userIds);
            if (overrided && overrideData != "")
            {
                var result = CreateWorkRulesResultFromString(overrideData);
                if (result != null)
                {
                    TimeEngineManager(param.ActorCompanyId, param.UserId).SaveEvaluateAllWorkRulesByPass(result, employeeId);
                }
            }
            return xemail.ToXDocument(MobileTask.SendMail);
        }

        public XDocument SendShiftRequestValidateWorkRules(MobileParam param, int shiftId, int employeeId, string userIds)
        {
            MobileMessageBox messageBox = PerformSendShiftRequestValidateWorkRules(param, shiftId, employeeId, userIds);

            return messageBox.ToXDocument();
        }

        public XDocument GetShiftRequestStatus(MobileParam param, int shiftId)
        {
            var result = PerformGetShiftRequestStatus(param, shiftId);

            return result.ToXDocument();
        }

        public XDocument RemoveShiftRequestRecipient(MobileParam param, int shiftId, int recipientUserId)
        {
            var result = PerformRemoveShiftRequestRecipient(param, shiftId, recipientUserId);

            return result.ToXDocument(MobileTask.Update);
        }

        public XDocument UndoShiftRequest(MobileParam param, int shiftId)
        {
            var result = PerformUndoShiftRequest(param, shiftId);

            return result.ToXDocument(MobileTask.Update);
        }

        #endregion

        #region Settings/Permissions

        public XDocument GetEditShiftsViewSettings(MobileParam param)
        {
            MobileSettings settings = PerformGetEditShiftsViewSettings(param);

            return settings.ToXDocument();
        }

        public XDocument GetStaffingPermissions(MobileParam param, MobileDisplayMode mobileDisplayMode)
        {
            MobilePermissions features = PerformGetStaffingPermissions(param, mobileDisplayMode);

            return features.ToXDocument();
        }

        public XDocument GetStaffingSettings(MobileParam param, MobileDisplayMode mobileDisplayMode)
        {
            var settings = PerformGetStaffingSettings(param, mobileDisplayMode);

            return settings.ToXDocument();
        }

        public XDocument GetCompanyHolidays(MobileParam param, DateTime dateFrom, DateTime dateTo)
        {
            MobileCompanyHolidays holidays = PerformGetCompanyHolidays(param, dateFrom, dateTo);
            return holidays.ToXDocument();
        }

        #endregion

        #region Template schedule

        public XDocument GetTemplateScheduleViewEmployee(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            MobileTemplateScheduleViewMonth templateShcedule = PerformGetTemplateScheduleViewEmployee(param, employeeId, dateFrom, dateTo);

            return templateShcedule.ToXDocument();
        }

        public XDocument GetTemplateScheduleDayEmployee(MobileParam param, int employeeId, DateTime date)
        {
            MobileShifts templateShcedule = PerformGetTemplateScheduleDayEmployee(param, employeeId, date);

            return templateShcedule.ToXDocument();
        }

        #endregion

        #region EmployeeChild

        public XDocument GetEmployeeChilds(MobileParam param, int employeeId)
        {
            MobileDicts childs = PerformGetEmployeeChilds(param, employeeId);

            return childs.ToXDocument();
        }

        #endregion

        #endregion

        #region News

        public XDocument GetAllInternalNews(MobileParam param, bool isStartPage)
        {
            MobileInternalNews news = PerformGetAllInternalNews(param);

            return news.ToXDocument();
        }

        public XDocument GetInternalNews(MobileParam param, int newsId)
        {
            MobileNews singelNews = PerformGetInternalNews(param, newsId);

            return singelNews.ToXDocument();
        }

        #endregion

        #region Information

        public XDocument GetCompanyInformation(MobileParam param, int informationId)
        {
            var information = PerformGetCompanyInformation(param, informationId);

            return information.ToXDocument();
        }

        public XDocument GetCompanyInformations(MobileParam param)
        {
            var informations = PerformGetCompanyInformations(param);

            return informations.ToXDocument();
        }

        public XDocument GetSysInformation(MobileParam param, int informationId)
        {
            var information = PerformGetSysInformation(param, informationId);

            return information.ToXDocument();
        }

        public XDocument GetSysInformations(MobileParam param)
        {
            var informations = PerformGetSysInformations(param);

            return informations.ToXDocument();
        }

        public XDocument GetUnreadInformations(MobileParam param)
        {
            var informations = PerformGetUnreadInformations(param);

            return informations.ToXDocument();
        }

        public XDocument SetInformationRead(MobileParam param, int informationType, int informationId)
        {
            var result = PerformSetInformationRead(param, informationType, informationId);

            return result.ToXDocument(MobileTask.Update);
        }

        public XDocument SetInformationConfirmed(MobileParam param, int informationType, int informationId)
        {
            var result = PerformSetInformationConfirmed(param, informationType, informationId);

            return result.ToXDocument(MobileTask.Update);
        }

        #endregion

        #region XeMail

        public XDocument GetInboxMail(MobileParam param)
        {
            MobileXeMails mail = PerformGetInboxMail(param);

            return mail.ToXDocument();
        }

        public XDocument GetSentMail(MobileParam param)
        {
            MobileXeMails mail = PerformGetSentMail(param);

            return mail.ToXDocument();
        }

        public XDocument GetMail(MobileParam param, int mailId)
        {
            MobileXeMail mail = PerformGetMail(param, mailId);

            return mail.ToXDocument();
        }

        public XDocument SendMail(MobileParam param, int mailId, int parentMailId, string subject, string text,
            string userIds, string roleIds, string employeeGroupIds, string categoryIds, string messageGroupIds, byte[] imageData, string imageName, bool forward)
        {
            MobileXeMail mail = PerformSendMail(param, mailId, parentMailId, subject, text,
            userIds, roleIds, employeeGroupIds, categoryIds, messageGroupIds, imageData, imageName, forward);

            return mail.ToXDocument(MobileTask.SendMail);
        }

        public XDocument GetMailAttachments(MobileParam param, int mailId)
        {
            MobileFiles attachments = PerformGetMailAttachments(param, mailId);

            return attachments.ToXDocument();
        }

        public XDocument GetMailAttachment(MobileParam param, int attachmentId)
        {
            MobileFile attachment = PerformGetMailAttachment(param, attachmentId);

            return attachment.ToXDocument();
        }

        public XDocument DeleteIncomingMail(MobileParam param, int mailId)
        {
            MobileXeMail mail = PerformDeleteIncomingMail(param, mailId);

            return mail.ToXDocument(MobileTask.DeleteIncomingMail);
        }

        public XDocument DeleteOutgoingMail(MobileParam param, int mailId)
        {
            var result = PerformDeleteOutgoingMail(param, mailId);

            return result.ToXDocument(MobileTask.Delete);
        }

        public XDocument AnswerShiftRequest(MobileParam param, int mailId, bool value)
        {
            MobileXeMail mail = PerformAnswerShiftRequest(param, mailId, value);

            return mail.ToXDocument(MobileTask.AnswerShiftRequest);
        }

        public XDocument SendNeedsConfirmationAnswer(MobileParam param, int mailId)
        {
            MobileXeMail mail = PerformSendNeedsConfirmationAnswer(param, mailId);

            return mail.ToXDocument(MobileTask.SendNeedsConfirmationAnswer);
        }

        public XDocument MarkMailAsRead(MobileParam param, int mailId)
        {
            MobileXeMail mail = PerformMarkMailAsRead(param, mailId);

            return mail.ToXDocument(MobileTask.MarkMailAsRead);
        }

        public XDocument SetMailIdsAsRead(MobileParam param, string mailIds)
        {
            MobileXeMail mail = PerformSetMailIdsAsRead(param, mailIds);

            return mail.ToXDocument(MobileTask.MarkMailAsRead);
        }

        public XDocument SetMailIdsAsUnread(MobileParam param, string mailIds)
        {
            MobileXeMail mail = PerformSetMailIdsAsUnread(param, mailIds);

            return mail.ToXDocument(MobileTask.MarkMailAsUnread);
        }
        public XDocument DeleteIncomingMailIds(MobileParam param, string mailIds)
        {
            MobileXeMail mail = PerformDeleteIncomingMailIds(param, mailIds);

            return mail.ToXDocument(MobileTask.DeleteIncomingMail);
        }

        public XDocument DeleteOutgoingMailIds(MobileParam param, string mailIds)
        {
            var result = PerformDeleteOutgoingMailIds(param, mailIds);

            return result.ToXDocument(MobileTask.Delete);
        }

        public XDocument GetReceivers(MobileParam param, string searchText)
        {
            MobileReceivers receivers = PerformGetReceivers(param, searchText);

            return receivers.ToXDocument();
        }

        public XDocument GetReplyReceiver(MobileParam param, int mailId)
        {
            MobileReceiver receiver = PerformGetReplyReceiver(param, mailId);

            return receiver.ToXDocument();
        }

        public XDocument GetReplyAllReceivers(MobileParam param, int mailId)
        {
            MobileReceivers receivers = PerformGetReplyAllReceivers(param, mailId);

            return receivers.ToXDocument();
        }

        #endregion

        #region Archived Files

        public XDocument GetArchivedFiles(MobileParam param)
        {
            MobileFiles archivedFiles = PerformGetArchivedFiles(param);

            return archivedFiles.ToXDocument();
        }

        public XDocument GetArchivedFile(MobileParam param, int dataStorageId)
        {
            MobileFile archivedFile = PerformGetArchivedFile(param, dataStorageId);

            return archivedFile.ToXDocument();
        }

        public XDocument GetMyDocuments(MobileParam param)
        {
            MobileFiles myDocuments = PerformGetMyDocuments(param);

            return myDocuments.ToXDocument();
        }

        public XDocument SetDocumentRead(MobileParam param, int documentId, bool confirmed)
        {
            var result = PerformSetDocumentRead(param, documentId, confirmed);

            return result.ToXDocument(MobileTask.Update);
        }

        #endregion

        #region Employee Details

        public XDocument GetEmployeeDetails(MobileParam param, int employeeId)
        {
            MobileEmployeeDetails employeeDetails = PerformGetEmployeeDetails(param, employeeId);

            return employeeDetails.ToXDocument();
        }

        public XDocument GetEmployeeList(MobileParam param)
        {
            MobileEmployeeList employeeList = PerformGetEmployeeList(param);

            return employeeList.ToXDocument();
        }

        public XDocument SaveEmployeeDetails(MobileParam param, int employeeId, string firstName, string lastName, int addressId, string address, string postalCode, string postalAddress, int closestRelativeId, string closestRelativePhone, string closestRelativeName, string closestRelativeRelation, bool closestRelativeIsSecret, int closestRelativeId2, string closestRelativePhone2, string closestRelativeName2, string closestRelativeRelation2, bool closestRelativeIsSecret2, int mobileId, string mobile, int emailId, string email)
        {
            MobileEmployeeDetails employeeDetails = PerformSaveEmployeeDetails(param, param.ActorCompanyId, employeeId, firstName, lastName, addressId, address, postalCode, postalAddress, closestRelativeId, closestRelativePhone, closestRelativeName, closestRelativeRelation, closestRelativeIsSecret, closestRelativeId2, closestRelativePhone2, closestRelativeName2, closestRelativeRelation2, closestRelativeIsSecret2, mobileId, mobile, emailId, email);

            return employeeDetails.ToXDocument(MobileTask.SaveEmployeeDetails);
        }
        #endregion

        #region Employee TimePeriods
        public XDocument GetEmployeeTimePeriodYears(MobileParam param, int employeeId, int year)
        {
            MobileEmployeeTimePeriodYears employeeTimePeriodYear = PerformGetEmployeeTimePeriodYears(param, employeeId, year);

            return employeeTimePeriodYear.ToXDocument();
        }

        public XDocument GetEmployeeTimePeriods(MobileParam param, int employeeId)
        {
            MobileEmployeeTimePeriods employeeTimePeriods = PerformGetEmployeeTimePeriods(param, employeeId);

            return employeeTimePeriods.ToXDocument();
        }
        public XDocument GetEmployeeTimePeriodDetails(MobileParam param, int employeeId, int timePeriodId)
        {
            MobileEmployeeTimePeriod employeeTimePeriod = PerformGetEmployeeTimePeriodDetails(param, employeeId, timePeriodId);

            return employeeTimePeriod.ToXDocument();
        }
        #endregion

        #region EmployeeUserSettings

        public XDocument SaveEmployeeUserSettings(MobileParam param, int employeeId, bool wantsExtraShifts)
        {
            var employeeDetails = PerformSaveEmployeeUserSettings(param, employeeId, wantsExtraShifts);

            return employeeDetails.ToXDocument(MobileTask.SaveEmployeeUserSettings);
        }

        public XDocument GetEmployeeUserSettings(MobileParam param, int employeeId)
        {
            MobileEmployeeUserSettings settings = PerformGetEmployeeUserSettings(param, employeeId);

            return settings.ToXDocument();
        }

        #endregion

        #region SalarySpecifikations

        public XDocument GetSalarySpecifications(MobileParam param, int employeeId)
        {
            MobileFiles salarySpecifikations = PerformGetSalarySpecifications(param, employeeId);

            return salarySpecifikations.ToXDocument();
        }

        public XDocument GetSalarySpecification(MobileParam param, int employeeId, int dataStorageId)
        {
            MobileFile salarySpecifikation = PerformGetSalarySpecification(param, employeeId, dataStorageId);

            return salarySpecifikation.ToXDocument();
        }

        #endregion

        #region State analysis

        public string GetStateAnalysis(MobileParam param)
        {
            MobileHTMLView htmlview = PerformGetStateAnalysis(param);

            return htmlview.ToString();
        }

        #endregion

        #region TimeSheet

        public XDocument GetTimeSheetRows(MobileParam param, DateTime date, int employeeId)
        {
            MobileTimeSheetRows timeSheets = PerformGetTimeSheetRows(param, date, employeeId);

            return timeSheets.ToXDocument();
        }

        public XDocument GetTimeSheetInfo(MobileParam param, DateTime date, int employeeId)
        {
            MobileTimeSheetInfo timeSheetSchedule = PerformGetTimeSheetInfo(param, date, employeeId);

            return timeSheetSchedule.ToXDocument();
        }

        #endregion

        #region ProjectTimeBlock

        public XDocument GetExtendedTimeRegistrationSettings(MobileParam param)
        {
            MobileSettings settings = PerformGetExtendedTimeRegistrationSettings(param);

            return settings.ToXDocument();
        }

        public XDocument GetDeviationCausesForTimeRegistration(MobileParam param)
        {
            MobileDeviationCauses causes = PerformGetDeviationCausesForTimeRegistration(param);

            return causes.ToXDocument();
        }

        public XDocument GetEmployeeFirstEligableTime(MobileParam param, int employeeId, DateTime date)
        {
            //Perform
            MobileEmployeeFirstEligableTime result = PerformGetEmployeeFirstEligableTime(param, employeeId, date);

            return result.ToXDocument();
        }

        public XDocument GetLastProjectTimeBlockOnDate(MobileParam param, int orderId, int employeeId, DateTime date, int timeCodeId)
        {
            MobileProjectTimeBlock mobileProjectTimeBlock = PerformGetLastProjectTimeBlockOnDate(param, orderId, employeeId, date, timeCodeId);

            return mobileProjectTimeBlock.ToXDocument();
        }

        public XDocument GetProjectTimeBlock(MobileParam param, int orderId, int projectTimeBlockId)
        {
            MobileProjectTimeBlock mobileProjectTimeBlock = PerformGetProjectTimeBlock(param, orderId, projectTimeBlockId);

            return mobileProjectTimeBlock.ToXDocument();
        }

        public XDocument GetProjectTimeBlocks(MobileParam param, int orderId, DateTime? fromDate, DateTime? toDate)
        {
            MobileProjectTimeBlocks mobileProjectTimeBlocks = PerformGetProjectTimeBlocks(param, orderId, fromDate, toDate);

            return mobileProjectTimeBlocks.ToXDocument();
        }

        public XDocument SaveProjectTimeBlock(MobileParam param, int orderId, int projectTimeBlockId, DateTime date, DateTime startTime, DateTime stopTime, int workTimeInMinutes, int invoiceTimeInMinutes, string note, string internalNote, int timeCodeId, int timeDeviationCauseId, bool hasValidated, int employeeChildId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            if (useProjectTimeBlocks)
            {
                var mobileProjectTimeBlock = PerformSaveProjectTimeBlock(param, orderId, projectTimeBlockId, date, startTime, stopTime, invoiceTimeInMinutes, note, internalNote, timeCodeId, timeDeviationCauseId, workTimeInMinutes, hasValidated, employeeChildId);
                return mobileProjectTimeBlock.ToXDocument(MobileTask.SaveProjectTimeBlock);
            }
            else
            {
                var mobileTimeRow = PerformSaveTimeRow(param, orderId, projectTimeBlockId, date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId);
                return mobileTimeRow.ToXDocument(MobileTask.SaveTimeRow);
            }
        }

        public XDocument SaveProjectTimeBlockValidation(MobileParam param, int projectTimeBlockId, DateTime date, DateTime startTime, DateTime stopTime, int timeDeviationCauseId, int employeeChildId)
        {
            MobileProjectTimeBlockValidation mobileProjectTimeBlockValidation = PerformSaveProjectTimeBlockValidation(param, projectTimeBlockId, date, startTime, stopTime, timeDeviationCauseId, employeeChildId);

            return mobileProjectTimeBlockValidation.ToXDocument();
        }

        public XDocument DeleteProjectTimeBlock(MobileParam param, int projectTimeBlockId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            var mobileResult = PerformDeleteProjectTimeBlock(param, projectTimeBlockId);
            return mobileResult.ToXDocument(MobileTask.DeleteProjectTimeBlock);
        }

        public XDocument MoveProjectTimeBlockToDate(MobileParam param, int projectTimeBlockId, DateTime date)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            var mobileResult = PerformMoveProjectTimeBlockToDate(param, projectTimeBlockId, date);
            return mobileResult.ToXDocument(MobileTask.MoveProjectTimeBlockToDate);
        }

        public XDocument CopyProjectTimeBlocksDate(MobileParam param, DateTime fromDate, DateTime toDate)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            var result = PerformCopyProjectTimeBlocksDate(param, fromDate, toDate);
            return result.ToXDocument(MobileTask.Save);
        }

        public XDocument CopyProjectTimeBlocksWeek(MobileParam param, DateTime fromDate, DateTime toDate)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileOrderRow.GetDefaultSaveXml());

            var result = PerformCopyProjectTimeBlocksWeek(param, fromDate, toDate);
            return result.ToXDocument(MobileTask.Save);
        }

        #endregion

        #region Expense

        public XDocument GetAdditionDeductionTimeCodes(MobileParam param, bool isOrder)
        {
            MobileAdditionDeductionTimeCodes mobileAdditionDeductionTimeCodes = PerformGetAdditionDeductionTimeCodes(param, isOrder);

            return mobileAdditionDeductionTimeCodes.ToXDocument();
        }

        public XDocument GetExpense(MobileParam param, int expenseRowId)
        {
            MobileExpense mobileExpense = PerformGetExpense(param, expenseRowId);

            return mobileExpense.ToXDocument();
        }

        public XDocument GetExpenses(MobileParam param, int orderId)
        {
            MobileExpenses mobileExpenses = PerformGetExpenses(param, orderId);

            return mobileExpenses.ToXDocument();
        }

        public XDocument GetExpenses(MobileParam param, int employeeId, DateTime from, DateTime to)
        {
            MobileExpenses mobileExpenses = PerformGetExpenses(param, employeeId, from, to);

            return mobileExpenses.ToXDocument();
        }

        public XDocument GetExpenseProductPrice(MobileParam param, int invoiceId, int timeCodeId, decimal quantity)
        {
            var mobileValueResult = PerformGetExpenseProductPrice(param, invoiceId, timeCodeId, quantity);

            return mobileValueResult.ToXDocument();
        }

        public XDocument DeleteExpense(MobileParam param, int expenseRowId)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileExpense.GetDefaultSaveXml());

            var mobileResult = PerformDeleteExpense(param, expenseRowId);
            return mobileResult.ToXDocument(MobileTask.DeleteExpense);

        }

        public XDocument SaveExpense(MobileParam param, int orderId, int expenseRowId, int timeCodeId, DateTime from, DateTime to, DateTime startTime, DateTime stopTime, decimal quantity, bool specifiedUnitPrice, decimal unitPrice, decimal amount, decimal vat, bool transferToInvoice, decimal invoiceAmount, string internalComment, string externalComment)
        {
            if (debug)
                return XmlUtil.GetXDocument(MobileExpense.GetDefaultSaveXml());

            var mobileExpenseRow = PerformSaveExpense(param, orderId, expenseRowId, timeCodeId, from, to, startTime, stopTime, quantity, specifiedUnitPrice, unitPrice, amount, vat, transferToInvoice, invoiceAmount, internalComment, externalComment);
            mobileExpenseRow.VersionIsOld = Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_23);

            return mobileExpenseRow.ToXDocument(MobileTask.SaveExpense);
        }

        public XDocument SaveExpenseValidation(MobileParam param, int orderId, int expenseRowId, int timeCodeId, DateTime from, DateTime to, DateTime startTime, DateTime stopTime, decimal quantity, bool specifiedUnitPrice, decimal unitPrice, decimal amount, decimal vat, bool transferToInvoice, decimal invoiceAmount, string internalComment, string externalComment)
        {
            var messageBox = PerformSaveExpenseValidation(param, orderId, expenseRowId, timeCodeId, from, to, startTime, stopTime, quantity, specifiedUnitPrice, unitPrice, amount, vat, transferToInvoice, invoiceAmount, internalComment, externalComment);
            return messageBox.ToXDocument();
        }

        #endregion

        #region Demo PriceSearch

        public XDocument SearchExternalProductsDemo(MobileParam param, string search)
        {
            //Perform
            MobileExternalProducts mobileExternalProducts = PerformSearchExternalProductsDemo(param, search);

            return mobileExternalProducts.ToXDocument();
        }

        public XDocument SearchExternalProductPricesDemo(MobileParam param, string productNr)
        {
            //Perform
            MobileExternalProductPrices mobileExternalProductPrices = PerformSearchExternalProductPricesDemo(param, productNr);

            return mobileExternalProductPrices.ToXDocument();
        }

        #endregion

        #region Demo Videos

        public XDocument GetInstructionalVideos(MobileParam param)
        {
            //Perform
            MobileXeVideos videos = PerformGetInstructionalVideos(param);

            return videos.ToXDocument();
        }

        #endregion

        #endregion

        #region Perform methods

        #region AccountHierarchy

        private MobileDicts PerformGetAccountStringsFromHierarchyByUser(MobileParam param)
        {
            try
            {
                Dictionary<string, string> accountHierarchyIds = new Dictionary<string, string>();
                accountHierarchyIds.Add("", GetText(8815, "Alla dina konton"));
                accountHierarchyIds.AddRange(AccountManager.GetAccountHierarchyStringsByUser(param.ActorCompanyId, param.UserId));
                string accountHierarchyId = SettingManager.GetStringSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, param.UserId, param.ActorCompanyId, 0);

                MobileDicts dicts = new MobileDicts(param, accountHierarchyIds);
                dicts.SetSelectedId(accountHierarchyId);
                return dicts;
            }
            catch (Exception e)
            {
                LogError("PerformGetAccountStringsFromHierarchyByUser: " + e.ToString());
                return new MobileDicts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileResult PerformUpdateSettingAccountHierarchyId(MobileParam param, string hierarchyId)
        {
            try
            {
                MobileResult mobileResult = new MobileResult(param);
                ActionResult result = SettingManager.UpdateInsertStringSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, hierarchyId, param.UserId, param.ActorCompanyId, 0);
                if (result.Success)
                    mobileResult.SetTaskResult(MobileTask.UpdateSetting, true);
                else
                    mobileResult = new MobileResult(param, Texts.SaveFailed);

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformUpdateSettingAccountHierarchyId: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileAccountDims PerformGetAccountHierarchyAccountDims(MobileParam param, string selectedAccountIdsStr, bool includeSecondaryAccounts)
        {
            try
            {
                MobileAccountDims mobileAccountDims = GetAccountHierarchyDims(param, selectedAccountIdsStr, includeSecondaryAccounts, false);

                return mobileAccountDims;
            }
            catch (Exception e)
            {
                LogError("PerformGetAccountHierarchyAccounts: " + e.Message);
                return new MobileAccountDims(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileAccountDims GetAccountHierarchyDims(MobileParam param, string selectedAccountIdsStr, bool includeSecondaryAccounts, bool includeAbstractAccounts)
        {
            MobileAccountDims mobileAccountDims = null;
            List<AccountDimSmallDTO> dims = AccountManager.GetAccountDimsForPlanning(param.ActorCompanyId, param.UserId, onlyDefaultAccounts: !includeSecondaryAccounts, includeAbstractAccounts: true, isMobile: true, filterOnHierarchyHideOnSchedule: true).Where(x => x.Accounts.Any()).ToList();
            List<Account> settingAccounts = AccountManager.GetAccountHierarchySettingAccounts(param.UserId, param.ActorCompanyId);
            List<Account> selectedAccountsFromClient = new List<Account>();

            if (!string.IsNullOrEmpty(selectedAccountIdsStr))
            {
                List<int> accountIds = GetIds(selectedAccountIdsStr);
                List<int> selectedAccountIdsFromClient = new List<int>();
                foreach (var id in accountIds)
                {
                    if (!settingAccounts.Any(x => x.AccountId == id))
                        selectedAccountIdsFromClient.Add(id);
                }

                if (selectedAccountIdsFromClient.Any())
                    selectedAccountsFromClient = AccountManager.GetAccounts(selectedAccountIdsFromClient, param.ActorCompanyId);
            }
            List<Account> allSelectedAccounts = new List<Account>();
            allSelectedAccounts.AddRange(selectedAccountsFromClient);
            if (!includeSecondaryAccounts)
                allSelectedAccounts.AddRange(settingAccounts);

            this.FilterAccounts(dims, allSelectedAccounts.ToDTOs());

            #region Remove abstract accounts (for now)
            if (!includeAbstractAccounts) // 2021-05-28, Item 57591
            {
                foreach (var dim in dims)
                {
                    dim.Accounts = dim.Accounts.Where(x => !x.IsAbstract).ToList();
                    dim.CurrentSelectableAccounts = dim.CurrentSelectableAccounts.Where(x => !x.IsAbstract).ToList();
                }
            }
            #endregion

            mobileAccountDims = new MobileAccountDims(param, dims);
            //Set dimensions as locked and preselected accounts accourding to current setting
            if (!includeSecondaryAccounts)
                mobileAccountDims.SetAccountDimAsLocked(settingAccounts);
            //Set user selected accounts as selected
            mobileAccountDims.SetSelectedAccounts(selectedAccountsFromClient);
            return mobileAccountDims;
        }

        private MobileDicts PerformGetEmployeeShiftAccounts(MobileParam param, int employeeId, DateTime date)
        {
            try
            {
                Employee employee = null;
                EmployeeGroup employeeGroup = null;
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                if (employeeId != 0 && employeeId != base.GetHiddenEmployeeIdFromCache(entitiesReadOnly, CacheConfig.Company(param.ActorCompanyId)))
                {
                    employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadEmployment: true);
                    if (employee == null)
                        return new MobileDicts(param, Texts.EmployeeNotFoundMessage);

                    employeeGroup = employee.GetEmployeeGroup(date, GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(param.ActorCompanyId)));
                    if (employeeGroup == null)
                        return new MobileDicts(param, Texts.EmployeeGroupNotFoundMessage);
                }

                Dictionary<int, string> accountsDict = new Dictionary<int, string>();
                string userCurrentAccountHierarchyId = SettingManager.GetStringSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, param.UserId, param.ActorCompanyId, 0);
                List<AccountDTO> accounts = AccountManager.GetSelectableEmployeeShiftAccounts(param.UserId, param.ActorCompanyId, employeeId, date, includeAbstract: true);
                foreach (var account in accounts)
                {
                    if (!accountsDict.ContainsKey(account.AccountId))
                        accountsDict.Add(account.AccountId, account.Name);
                }
                if (employeeGroup != null && employeeGroup.AllowShiftsWithoutAccount)
                {
                    accountsDict.Add(0, "");
                }

                MobileDicts dicts = new MobileDicts(param, accountsDict);
                if (employeeGroup != null && employeeGroup.AllowShiftsWithoutAccount)
                {
                    dicts.SetSelectedId("0");
                }
                else if (dicts.Size() == 1)
                {
                    dicts.SetFirstAsSelected();
                }
                else if (userCurrentAccountHierarchyId.IsNullOrEmpty())
                {
                    int? defaultAccountId = EmployeeManager.GetDefaultEmployeeAccountId(param.ActorCompanyId, employeeId, date);
                    if (defaultAccountId.HasValue)
                        dicts.SetSelectedId(defaultAccountId.Value.ToString());
                }
                else
                {
                    var account = AccountManager.GetAccountHierarchySettingAccount(actorCompanyId: param.ActorCompanyId, userId: param.UserId);
                    if (account != null)
                        dicts.SetSelectedId(account.AccountId.ToString());
                }

                return dicts;
            }
            catch (Exception e)
            {
                LogError("PerformGetEmployeeShiftAccounts: " + e.ToString());
                return new MobileDicts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion

        #region Demo

        #region PriceSearch

        private MobileExternalProducts PerformSearchExternalProductsDemo(MobileParam param, string search)
        {
            if (param == null)
                return new MobileExternalProducts(param, Texts.ProductsNotFoundMessage);

            //Get external products
            var products = ProductManager.SearchInvoiceProducts(param.ActorCompanyId, search, MobileExternalProducts.MAXFETCH);

            return new MobileExternalProducts(param, products);
        }

        private MobileExternalProductPrices PerformSearchExternalProductPricesDemo(MobileParam param, string productnr)
        {
            if (param == null)
                return new MobileExternalProductPrices(param, Texts.ProductsNotFoundMessage);

            //Get external product pricess
            List<InvoiceProductPriceSearchViewDTO> productPricess = ProductManager.SearchInvoiceProductPrices(param.ActorCompanyId, 0, 0, new List<string>() { productnr }, true).ToList();

            return new MobileExternalProductPrices(param, productPricess, null);
        }

        #endregion

        #region Videos

        private MobileXeVideos PerformGetInstructionalVideos(MobileParam param)
        {
            return new MobileXeVideos(param);
        }

        #endregion

        #endregion

        #region Common

        #region Accumulators

        private MobileAccumulators PerformGetAccumulators(MobileParam param, int employeeId)
        {
            MobileAccumulators mobileAccumulators = new MobileAccumulators(param);
            var employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadEmployment: true);
            if (employee == null)
                return new MobileAccumulators(param, Texts.EmployeeNotFoundMessage);

            if (FeatureManager.HasRolePermission(Feature.Time_Time_AttestUser_ShowAccumulators, Permission.Readonly, param.RoleId, param.ActorCompanyId))
            {
                GetTimeAccumulatorItemsInput timeAccInput = GetTimeAccumulatorItemsInput.CreateInput(param.ActorCompanyId, param.UserId, employeeId, DateTime.Today, DateTime.Today, calculateDay: true, calculateAccToday: true);
                List<TimeAccumulatorItem> accItems = TimeAccumulatorManager.GetTimeAccumulatorItems(timeAccInput);
                mobileAccumulators.AddAccumulators(accItems);
            }

            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation, Permission.Readonly, param.RoleId, param.ActorCompanyId))
            {
                EmployeeVacationSEDTO employeeVacation = EmployeeManager.GetLatestEmployeeVacationSE(employeeId).ToDTO();
                if (employeeVacation != null)
                {
                    var vacationGroup = PayrollManager.GetVacationGroupForEmployee(param.ActorCompanyId, employeeId, DateTime.Today).ToDTO();
                    bool showHours = vacationGroup?.VacationGroupSE?.ShowHours ?? false;
                    decimal remaining = showHours ? employeeVacation.TotalRemainingHours : employeeVacation.TotalRemainingDays;
                    decimal prelUsed = PayrollManager.GetEmployeeVacationPrelUsedDays(param.ActorCompanyId, employeeId, DateTime.Today, showHours).Sum;

                    if (!showHours)
                    {
                        mobileAccumulators.AddAccumulator(GetText(12003, "Återstående semesterdagar (inkl planerat uttagna)"), (remaining - prelUsed).ToString());
                        mobileAccumulators.AddAccumulator(GetText(11998, "Prel semesterdagar"), prelUsed.ToString());
                        mobileAccumulators.AddAccumulator(GetText(11999, "Återstående betalda dagar"), employeeVacation.RemainingDaysPaid.ToString());
                        mobileAccumulators.AddAccumulator(GetText(12000, "Återstående obetalda dagar"), employeeVacation.RemainingDaysUnpaid.ToString());

                        if (!employeeVacation.RemainingDaysYear1.IsNullOrEmpty())
                            mobileAccumulators.AddAccumulator(GetText(12001, "Återstående sparade dagar år ") + "1", employeeVacation.RemainingDaysYear1.ToString());

                        if (!employeeVacation.RemainingDaysYear2.IsNullOrEmpty())
                            mobileAccumulators.AddAccumulator(GetText(12001, "Återstående sparade dagar år ") + "2", employeeVacation.RemainingDaysYear2.ToString());

                        if (!employeeVacation.RemainingDaysYear3.IsNullOrEmpty())
                            mobileAccumulators.AddAccumulator(GetText(12001, "Återstående sparade dagar år ") + "3", employeeVacation.RemainingDaysYear3.ToString());

                        if (!employeeVacation.RemainingDaysYear4.IsNullOrEmpty())
                            mobileAccumulators.AddAccumulator(GetText(12001, "Återstående sparade dagar år ") + "4", employeeVacation.RemainingDaysYear4.ToString());

                        if (!employeeVacation.RemainingDaysYear5.IsNullOrEmpty())
                            mobileAccumulators.AddAccumulator(GetText(12001, "Återstående sparade dagar år ") + "5", employeeVacation.RemainingDaysYear5.ToString());

                        if (!employeeVacation.RemainingDaysOverdue.IsNullOrEmpty())
                            mobileAccumulators.AddAccumulator(GetText(12002, "Återstående sparade dagar förfallna"), employeeVacation.RemainingDaysOverdue.ToString());
                    }
                    else
                    {
                        mobileAccumulators.AddAccumulator(GetText(12004, "Återstående semestertimmar (inkl planerat uttagna)"), remaining.ToString());
                        mobileAccumulators.AddAccumulator(GetText(12005, "Prel semestertimmar"), prelUsed.ToString());
                    }
                }
            }

            var breakAcc = TimeAccumulatorManager.GetBreakTimeAccumulatorItem(employee.ActorCompanyId, DateTime.Now, employeeId, employee.GetEmployeeGroupId(DateTime.Today));
            if (breakAcc != null)
            {
                mobileAccumulators.AddAccumulator(breakAcc.Name, breakAcc.SumPeriod.ToString());
            }

            return mobileAccumulators;
        }

        #endregion

        #region Delegate 

        private MobileUserCompanyRoleDelegate PerformSearchTargetUserForDelegation(MobileParam param, string userCondition)
        {
            try
            {
                MobileUserCompanyRoleDelegate mobileUserCompanyRoleDelegate = null;
                UserCompanyRoleDelegateHistoryUserDTO delegateUserDTO = UserManager.SearchTargetUserForDelegation(param.ActorCompanyId, param.UserId, param.UserId, userCondition);
                if (delegateUserDTO != null)
                    mobileUserCompanyRoleDelegate = new MobileUserCompanyRoleDelegate(param, delegateUserDTO);
                else
                    mobileUserCompanyRoleDelegate = new MobileUserCompanyRoleDelegate(param, Texts.UserNotFound);

                return mobileUserCompanyRoleDelegate;
            }
            catch (Exception e)
            {
                LogError("PerformSearchTargetUserForDelegation: " + " (" + userCondition + ") " + e.Message);
                return new MobileUserCompanyRoleDelegate(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileUserDelegateHistory PerformGetUserDelegateHistory(MobileParam param)
        {
            try
            {
                List<UserCompanyRoleDelegateHistoryGridDTO> historyGridDTOs = UserManager.GetUserCompanyRoleDelegateHistoryForUser(param.ActorCompanyId, param.RoleId, param.UserId, param.UserId);
                MobileUserDelegateHistory mobileUserDelegateHistory = new MobileUserDelegateHistory(param, historyGridDTOs);

                return mobileUserDelegateHistory;
            }
            catch (Exception e)
            {
                LogError("PerformGetUserDelegateHistory: " + e.Message);
                return new MobileUserDelegateHistory(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileResult PerformSaveAttestRoleUserDelegation(MobileParam param, int targetUserId, int attestRoleUserId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                if (targetUserId < 0 || attestRoleUserId < 0)
                    return new MobileResult(param, Texts.IncorrectInputMessage);

                var attestRoleUser = AttestManager.GetAttestRoleUser(attestRoleUserId);
                if (attestRoleUser == null)
                    return new MobileResult(param, Texts.IncorrectInputMessage);

                UserCompanyRoleDelegateHistoryUserDTO targetUser = new UserCompanyRoleDelegateHistoryUserDTO()
                {
                    UserId = targetUserId,
                };

                targetUser.TargetAttestRoles = new List<UserAttestRoleDTO>();
                targetUser.TargetRoles = new List<UserCompanyRoleDTO>();
                targetUser.TargetAttestRoles.Add(new UserAttestRoleDTO()
                {
                    AttestRoleUserId = attestRoleUserId,
                    AttestRoleId = attestRoleUser.AttestRoleId,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    IsModified = true,
                });

                ActionResult result = UserManager.SaveUserCompanyRoleDelegation(targetUser, param.ActorCompanyId, param.UserId, param.UserId);
                return new MobileResult(param, result);
            }
            catch (Exception e)
            {
                LogError("PerformSaveAttestRoleUserDelegation: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }

        }

        private MobileResult PerformDeleteUserCompanyRoleDelegation(MobileParam param, int headId)
        {
            try
            {
                if (headId < 0)
                    return new MobileResult(param, Texts.IncorrectInputMessage);

                #region Perform

                ActionResult result = UserManager.DeleteUserCompanyRoleDelegation(headId, param.ActorCompanyId);

                #endregion

                //Set result
                MobileResult mobileResult = new MobileResult(param, result);
                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformDeleteUserCompanyRoleDelegation: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }
        }

        #endregion

        #region Login/Logout

        private XDocument PerformLogin(string version, string licenseNr, string loginName, string password, out int userSessionId)
        {
            userSessionId = 0;
            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_12))
                return MobileMessages.GetErrorMessageDocument(GetText(8793, "Du kör en gammal version av appen.\nVar vänlig att avinstallera din SoftOne app och ladda ner den senaste version av SoftOne GO från Google Play/App Store.\nHälsningar  SoftOne"));

            SoeLoginState loginState = LoginManager.LoginUser(licenseNr, loginName, password, out _, out Company company, out User user, out _, mobileLogin: true);

            return ValidateLogin(ref loginState, user, company, version, out _, out userSessionId);
        }

        private XDocument PerformLogin(string version, int userId, out int userSessionId)
        {
            userSessionId = 0;
            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_12))
                return MobileMessages.GetErrorMessageDocument(GetText(8793, "Du kör en gammal version av appen.\nVar vänlig att avinstallera din SoftOne app och ladda ner den senaste version av SoftOne GO från Google Play/App Store.\nHälsningar  SoftOne"));

            #region Login

            var user = UserManager.GetUser(userId, onlyActive: true, loadLicense: true);
            if (user == null || !user.DefaultActorCompanyId.HasValue)
                return MobileMessages.GetErrorMessageDocument(Texts.UserNotFound);

            var company = CompanyManager.GetCompany(user.DefaultActorCompanyId.Value);
            if (company == null)
                return MobileMessages.GetErrorMessageDocument(Texts.CompanyNotFound);

            LoginManager.BlockedFromDateValidation(user);
            var loginState = LicenseCacheManager.Instance.LoginUser(user, user.License.LicenseNr, true, false, false, parameterObject.ExtendedUserParams.UserEnvironmentInfo, out string detailedMessage);
            if (loginState != SoeLoginState.OK)
                return MobileMessages.GetErrorMessageDocument(detailedMessage);

            #endregion

            return ValidateLogin(ref loginState, user, company, version, out _, out userSessionId);
        }

        private XDocument PerformStartup(MobileParam param, out int userSessionId)
        {
            userSessionId = 0;

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_12))
                return MobileMessages.GetErrorMessageDocument(GetText(8793, "Du kör en gammal version av appen.\nVar vänlig att avinstallera din SoftOne app och ladda ner den senaste version av SoftOne GO från Google Play/App Store.\nHälsningar  SoftOne"));

            try
            {
                User user = UserManager.GetUser(param.UserId, loadLicense: true);
                Company company = CompanyManager.GetCompany(param.ActorCompanyId);
                Role role = RoleManager.GetRole(param.RoleId);
                if (user == null || company == null || role == null)
                    return MobileMessages.GetErrorMessageDocument(GetText(5944, "Inloggningen kunde inte återupprättas, du måste logga in igen"));

                List<UserCompanyRole> userCompanyRoles = UserManager.GetUserCompanyRolesByUser(user.UserId);
                if (!UserManager.HasUserCompanyRole(userCompanyRoles, user, company, role))
                    return MobileMessages.GetErrorMessageDocument(GetText(5945, "Inloggningen kunde inte verifieras, du måste logga in igen"));

                SoeLoginState loginState = SoeLoginState.OK;
                var result = ValidateLogin(ref loginState, user, company, param.Version, out bool success, out userSessionId);
                if (!success)
                    return result;

                if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_12))
                {
                    //when app version - email is mandatory i released, we need to return an error here "Du måste uppdatera din app"......
                    return MobileMessages.GetStartUpSuccessDocument(true, false);
                }
                else
                {
                    using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, param.ActorCompanyId);
                    string accountHierarchySettingAccountNames = AccountManager.GetAccountHierarchySettingAccountNames(param.UserId, param.ActorCompanyId, true);
                    string domain = GetDomain(user);
                    return MobileMessages.GetStartUpDocument(true, false, GetPNId(user), string.IsNullOrEmpty(user.Email), GetSetEmailURL(user), useAccountHierarchy, accountHierarchySettingAccountNames, domain);
                }
            }
            catch (Exception e)
            {
                LogError("PerformStartup failed: " + e.Message);
                return MobileMessages.GetErrorMessageDocument(GetText(8455, "Internt fel"));
            }
        }

        private string GetDomain(User user)
        {
            string domain = "";
            if(!user.LicenseReference.IsLoaded)
                user.LicenseReference.Load();

            if (user.License != null && user.License.SysServerId.HasValue)
            {
                SysServer sysServer = LoginManager.GetSysServer(user.License.SysServerId.Value);
                if (sysServer != null)
                    domain = "https://" + StringUtility.GetHostFromUrl(sysServer.Url);
            }
            
            return domain;
        }

        private XDocument ValidateLogin(ref SoeLoginState state, User user, Company company, string version, out bool success, out int userSessionId)
        {
            success = false;
            userSessionId = 0;

            try
            {
                if (user != null && user.BlockedFromDate.HasValue && user.BlockedFromDate.Value < DateTime.Now)
                    state = SoeLoginState.BlockedFromDatePassed;

                if (user?.License != null && user.License.TerminationDate.HasValue && user.License.TerminationDate.Value < DateTime.Today)
                    state = SoeLoginState.LicenseTerminated;

                if (state != SoeLoginState.OK || user == null || company == null)
                {
                    if (state == SoeLoginState.IsNotMobileUser || state == SoeLoginState.BlockedFromDatePassed)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(state == SoeLoginState.IsNotMobileUser ? "Mobilelogin failed, is not mobileuser." : "Mobilelogin failed, is blocked from date passed.");
                        sb.Append($"[User {user?.UserId.ToString() ?? "?"}.{user?.LoginName ?? "?"}]");
                        sb.Append($"[Company {company?.ActorCompanyId.ToString() ?? "?"}.{company?.Name ?? "?"}]");
                        sb.Append($"[License {company?.LicenseId.ToString() ?? "?"}.{company?.License?.Name ?? "?"}]");
                        SysLogManager.AddSysLogWarningMessage(Environment.MachineName, "WebService", sb.ToString());
                    }

                    return MobileMessages.GetErrorMessageDocument(LoginManager.GetLoginErrorMessage(state));
                }

                var result = UserManager.LoginUserSession(user.UserId, user.LoginName, company.ActorCompanyId, company.Name, mobileLogin: true, mobileApiVersion: version, softOneIdLogin: true);
                if (result.Success)
                {
                    int.TryParse(result.StringValue, out userSessionId);


                    success = true;
                    MobileParam param = new MobileParam(user.UserId, UserManager.GetDefaultRoleId(company.ActorCompanyId, user), company.ActorCompanyId, version);
                    using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, company.ActorCompanyId);
                    string accountHierarchySettingAccountNames = AccountManager.GetAccountHierarchySettingAccountNames(user.UserId, company.ActorCompanyId, true);
                    string domain = GetDomain(user);
                    return MobileMessages.GetLoginMessageDocument(param, false, GetPNId(user), string.IsNullOrEmpty(user.Email), GetSetEmailURL(user), useAccountHierarchy, accountHierarchySettingAccountNames, domain);
                }
                else
                {
                    state = result.IntegerValue > 0 ? (SoeLoginState)result.IntegerValue : SoeLoginState.RoleNotConnectedToCompany;
                    return MobileMessages.GetErrorMessageDocument(LoginManager.GetLoginErrorMessage(state));
                }
            }
            catch (Exception ex)
            {
                success = false;
                SysLogManager.AddSysLogErrorMessage(Environment.MachineName, "WebService", ex);
                return MobileMessages.GetErrorMessageDocument(LoginManager.GetLoginErrorMessage(state));
            }
        }

        private MobileResult PerformRegisterDeviceForNotifications(MobileParam param, string pushToken, string installationId)
        {
            try
            {
                User user = UserManager.GetUser(param.UserId, loadLicense: true);
                if (user == null)
                    return new MobileResult(param, Texts.UserNotFound);

                ActionResult actionResult = CommunicationManager.RegisterDevice(user, GetPNId(user), pushToken, param.MobileDeviceType, installationId);

                var mobileResult = new MobileResult(param);
                if (actionResult.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.Save, true);
                }
                else
                {
                    mobileResult = new MobileResult(param, actionResult.ErrorMessage);
                }

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformRegisterDeviceForNotifications: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }
        }

        private XDocument PerformLogout(MobileParam param)
        {
            bool loggedOut = false;

            User user = UserManager.GetUser(param.UserId, loadLicense: true);
            if (user != null)
                loggedOut = LoginManager.Logout(user.ToDTO(), mobileLogout: true);

            //Failed
            if (!loggedOut)
                return MobileMessages.GetErrorMessageDocument(LoginManager.GetLogoutErrorMessage());

            #region UserSession

            UserManager.LogoutUserSession(user.ToDTO(), description: Constants.MOBILE_WS_CURRENT_VERSION.ToString());

            #endregion

            return MobileMessages.GetSuccessDocument(loggedOut);
        }

        private MobileChangePWD PerformGetChangePWDPolicies(MobileParam param)
        {
            List<Tuple<string, string>> policies = new List<Tuple<string, string>>();

            int passwordMinLength = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CorePasswordMinLength, 0, param.ActorCompanyId, 0);
            if (passwordMinLength == 0)
                passwordMinLength = Constants.PASSWORD_DEFAULT_MIN_LENGTH;

            int passwordMaxLength = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CorePasswordMaxLength, 0, param.ActorCompanyId, 0);
            if (passwordMaxLength == 0)
                passwordMaxLength = Constants.PASSWORD_DEFAULT_MAX_LENGTH;

            string policy1 = String.Format(GetText(8519, "- Måste vara mellan {0} och {1} tecken"), passwordMinLength, passwordMaxLength);
            string policy2 = GetText(8520, "- Måste innehålla både bokstäver och siffror");
            string policy3 = GetText(8521, "- Måste ha minst en stor bokstav");

            policies.Add(Tuple.Create("Policy1", policy1));
            policies.Add(Tuple.Create("Policy2", policy2));
            policies.Add(Tuple.Create("Policy3", policy3));

            MobileChangePWD mobileChangePWDPolicies = new MobileChangePWD(param, policies);
            return mobileChangePWDPolicies;
        }
        #endregion

        #region Change compnay/Role

        private MobileUserCompanies PerformGetCompaniesForUser(MobileParam param)
        {
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileUserCompanies(param, Texts.UserNotFound);

            //Get UserCompanyRoles
            List<UserCompanyRole> userCompanyRoles = UserManager.GetUserCompanyRolesByUser(user.UserId, loadCompany: true);
            Dictionary<int, string> companyDict = new Dictionary<int, string>();


            //Get Company's
            List<Company> companies = (from ucr in userCompanyRoles
                                       where ucr.Company.License.LicenseId == user.LicenseId
                                       select ucr.Company).Distinct().ToList();

            //To make sure each Company only gets listed once
            List<int> listedCompanies = new List<int>();

            #region Populate dictionary

            //Current Company
            if (param.ActorCompanyId != 0)
            {
                int currentCompanyId = param.ActorCompanyId;
                Company currentCompany = companies.FirstOrDefault(i => i.ActorCompanyId == currentCompanyId);
                if (currentCompany != null)
                {
                    listedCompanies.Add(currentCompany.ActorCompanyId);
                    companyDict.Add(currentCompany.ActorCompanyId, currentCompany.ShortName);
                }
            }

            //Other Company's
            foreach (Company company in companies)
            {
                if (!listedCompanies.Contains(company.ActorCompanyId))
                {
                    listedCompanies.Add(company.ActorCompanyId);
                    companyDict.Add(company.ActorCompanyId, company.ShortName);
                }
            }

            #endregion


            MobileUserCompanies userCompanies = new MobileUserCompanies(param, companyDict);
            return userCompanies;
        }

        private MobileUserCompanyRoles PerformGetRolesForUserCompany(MobileParam param, int rolesForCompanyId)
        {
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileUserCompanyRoles(param, Texts.UserNotFound);

            //Get UserCompanyRoles
            List<UserCompanyRole> userCompanyRoles = UserManager.GetUserCompanyRolesByUser(user.UserId, loadRole: true);
            Dictionary<int, string> roleDict = new Dictionary<int, string>();

            //Get Role's
            List<Role> roles = (from ucr in userCompanyRoles
                                where ucr.ActorCompanyId == rolesForCompanyId
                                select ucr.Role).Distinct().ToList();

            //To make sure each Role only gets listed once
            List<int> listedRoles = new List<int>();

            //Current Role
            if (param.RoleId != 0)
            {
                Role currentRole = roles.FirstOrDefault(i => i.RoleId == param.RoleId);
                if (currentRole != null)
                {
                    listedRoles.Add(currentRole.RoleId);
                    roleDict.Add(currentRole.RoleId, RoleManager.GetRoleNameText(currentRole));
                }
            }

            //Other role's
            foreach (Role role in roles)
            {
                if (!listedRoles.Contains(role.RoleId))
                {
                    listedRoles.Add(role.RoleId);
                    roleDict.Add(role.RoleId, RoleManager.GetRoleNameText(role));
                }
            }

            MobileUserCompanyRoles mobileUserCompanyRoles = new MobileUserCompanyRoles(param, roleDict);
            return mobileUserCompanyRoles;
        }

        private MobileUserCompanyRole PerformValidateUserCompanyRole(MobileParam param, int newRoleId, int newCompanyId)
        {
            MobileUserCompanyRole userCompanyRole = new MobileUserCompanyRole(param);

            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileUserCompanyRole(param, Texts.UserNotFound);

            bool isValid = UserManager.ValidateMobileUserCompanyRole(user.LicenseId, user.UserId, newRoleId, newCompanyId);

            if (isValid)
                userCompanyRole.SetTaskResult(MobileTask.UserCompanyRoleIsValid, true);
            else
                userCompanyRole = new MobileUserCompanyRole(param, "Användaren är inte kopplad till vald roll och företag");

            return userCompanyRole;
        }

        #endregion

        #region StartPage

        private MobileModules PerformGetModules(MobileParam param)
        {
            MobileModules mobileModules = new MobileModules(param);

            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileModules(param, Texts.UserNotFound);

            bool showOrder = false;
            bool showTimeUser = false;
            bool showTimeAdmin = false;
            bool showStaffingUser = false;
            bool showStaffingAdmin = false;
            bool showCustomers = false;
            bool showXeMail = false;
            bool showInternalNews = false;
            bool showPreferences = false;
            bool showArchivedFiles = false;
            bool showColleagues = false;
            bool showSalarySpecifikations = false;
            bool showSupplierInvoiceAttestWorkFlowMyActive = false;
            bool showStateAnalysis = false;
            bool showTimeSheet = false;
            bool showDemoPriceSearch = false;
            bool showDemoVideos = false;
            bool showTimeStampAttendance = false;
            bool showApproveAbsenceRequest = false;
            bool showTemplateScheduleForEmployee = false;
            bool showAccumulators = false;
            bool showTimeStampInApp = false;
            bool showEvacuationList = false;

            #region Get Permissions

            //Order
            if (FeatureManager.HasRolePermission(Feature.Billing_Order_OrdersUser, Permission.Readonly, param.RoleId, param.ActorCompanyId) || FeatureManager.HasRolePermission(Feature.Billing_Order_Orders, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showOrder = true;

            //Time user
            if (FeatureManager.HasRolePermission(Feature.Time_Time_AttestUser, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showTimeUser = true;

            //Time admin
            if (FeatureManager.HasRolePermission(Feature.Time_Time_Attest_Overview, Permission.Modify, param.RoleId, param.ActorCompanyId))
                showTimeAdmin = true;

            //Staffing user     
            if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser, Permission.Readonly, param.RoleId, param.ActorCompanyId) || FeatureManager.HasRolePermission(Feature.Billing_Order_PlanningUser, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showStaffingUser = true;

            //Staffing admin                                                              //permission modify is needed after discussion with Håkan
            if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanning, Permission.Modify, param.RoleId, param.ActorCompanyId) || FeatureManager.HasRolePermission(Feature.Billing_Order_Planning, Permission.Modify, param.RoleId, param.ActorCompanyId))
                showStaffingAdmin = true;

            //Satffing admin - absencerequests
            if (FeatureManager.HasRolePermission(Feature.Time_Schedule_AbsenceRequests, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showApproveAbsenceRequest = true;

            //Customers
            if (FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showCustomers = true;

            //XEmail - visible for everybody            
            showXeMail = true;


            //InternalNews - visible for everybody 
            showInternalNews = true;

            //Preferences
            showPreferences = true; //for now            

            //Archived files 
            if (FeatureManager.HasRolePermission(Feature.Time_UploadedFiles, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showArchivedFiles = true;

            //Colleagues                       
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showColleagues = true;

            //Salary specifications
            if (FeatureManager.HasRolePermission(Feature.Time_Time_TimeSalarySpecification, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showSalarySpecifikations = true;

            //Attest supplierInvoice
            if (FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_AttestFlow_MyItems, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showSupplierInvoiceAttestWorkFlowMyActive = true;

            //StateAnalysis
            if (!FeatureManager.HasRolePermission(Feature.Common_HideStateAnalysis, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showStateAnalysis = true;

            //TimeSheet - Billig
            if (FeatureManager.HasRolePermission(Feature.Billing_Project_TimeSheetUser, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showTimeSheet = true;

            //TimeSheet - Time
            if (FeatureManager.HasRolePermission(Feature.Time_Time_TimeSheetUser, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showTimeSheet = true;

            //Demo - pricesearch          
            if (FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowPriceSearch_In_Mobile, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showDemoPriceSearch = true;

            //Demo - videos
            if (FeatureManager.HasRolePermission(Feature.Common_Help_Show_Instructional_Videos_In_Mobile, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showDemoVideos = true;

            //TimeStampAttendance
            if (FeatureManager.HasRolePermission(Feature.Time_TimeStampAttendanceGauge, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showTimeStampAttendance = true;

            //TimeStamp in app
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_StampInApp, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showTimeStampInApp = true;

            if (EmployeeManager.GetEmployeeByUser(param.ActorCompanyId, param.UserId) != null)
                showAccumulators = true;

            if (FeatureManager.HasRolePermission(Feature.Time_Employee_EvacuationList, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                showEvacuationList = true;

            #endregion

            #region Get Settings

            showTemplateScheduleForEmployee = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ShowTemplateScheduleForEmployeesInApp, 0, param.ActorCompanyId, 0);

            #endregion

            #region Set modules

            int moduleOrder = 1;

            if (showOrder)
            {
                mobileModules.AddMobileModule(MobileModuleType.Order, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showTimeUser)
            {
                mobileModules.AddMobileModule(MobileModuleType.TimeUser, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showTimeAdmin)
            {
                mobileModules.AddMobileModule(MobileModuleType.TimeAdmin, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showStaffingUser)
            {
                mobileModules.AddMobileModule(MobileModuleType.StaffingUser, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showStaffingUser && showTemplateScheduleForEmployee)
            {
                mobileModules.AddMobileModule(MobileModuleType.TemplateScheduleEmployee, moduleOrder, 0, true);
                moduleOrder++;
            }

            //old versions of the app cant handle new modules(the bug is fixed from appversion 7)
            if (showStaffingAdmin && !Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_0))
            {
                mobileModules.AddMobileModule(MobileModuleType.StaffingAdmin, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showAccumulators)
            {
                mobileModules.AddMobileModule(MobileModuleType.Accumulators, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showApproveAbsenceRequest)
            {
                mobileModules.AddMobileModule(MobileModuleType.StaffingAdminAbsenceRequests, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showCustomers)
            {
                mobileModules.AddMobileModule(MobileModuleType.Customers, moduleOrder, 0, true);
                moduleOrder++;
            }
            if (showXeMail)
            {
                mobileModules.AddMobileModule(MobileModuleType.XEMail, moduleOrder, 0, true);
                moduleOrder++;
            }
            if (showInternalNews)
            {
                mobileModules.AddMobileModule(MobileModuleType.InternalNews, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showArchivedFiles)
            {
                mobileModules.AddMobileModule(MobileModuleType.FileArchive, moduleOrder, 0, true);
                moduleOrder++;
            }
            if (showColleagues)
            {
                mobileModules.AddMobileModule(MobileModuleType.Collegues, moduleOrder, 0, true);
                moduleOrder++;
            }
            if (showSalarySpecifikations)
            {
                mobileModules.AddMobileModule(MobileModuleType.SalarySpecifikations, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showSupplierInvoiceAttestWorkFlowMyActive)
            {
                mobileModules.AddMobileModule(MobileModuleType.SupplierInvoiceAttest, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showStateAnalysis)
            {
                mobileModules.AddMobileModule(MobileModuleType.StateAnalysis, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showTimeSheet)
            {
                mobileModules.AddMobileModule(MobileModuleType.TimeSheet, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showDemoPriceSearch)
            {
                mobileModules.AddMobileModule(MobileModuleType.DemoPriceSearch, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showDemoVideos)
            {
                mobileModules.AddMobileModule(MobileModuleType.DemoVideos, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showTimeStampAttendance)
            {
                mobileModules.AddMobileModule(MobileModuleType.TimeStampAttendance, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showTimeStampInApp)
            {
                mobileModules.AddMobileModule(MobileModuleType.TimeStampInApp, moduleOrder, 0, true);
                moduleOrder++;
            }

            if (showEvacuationList)
            {
                mobileModules.AddMobileModule(MobileModuleType.EvacuationList, moduleOrder, 0, true);
                moduleOrder++;
            }

            // Always put last in order!
            if (showPreferences)
            {
                mobileModules.AddMobileModule(MobileModuleType.Preferences, moduleOrder, 0, true);
            }

            #endregion

            return mobileModules;
        }

        private MobileModules PerformGetModulesCounter(MobileParam param, DateTime? lastFetchDate)
        {
            try
            {
                MobileModules mobileModules = new MobileModules(param);

                User user = UserManager.GetUser(param.UserId);

                if (user == null)
                    return new MobileModules(param, Texts.UserNotFound);

                bool countUnreadDocuments = FeatureManager.HasRolePermission(Feature.Time_UploadedFiles, Permission.Readonly, param.RoleId, param.ActorCompanyId);

                int incomingXeMailCount = CommunicationManager.GetIncomingMessagesCount(user.LicenseId, user.UserId);
                mobileModules.AddMobileModule(MobileModuleType.XEMail, 0, incomingXeMailCount, false);

                // Older versions of mobile app does not send lastFetchDate
                if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_20))
                {
                    if (lastFetchDate.HasValue)
                    {
                        if (GeneralManager.HasNewInformations(param.ActorCompanyId, lastFetchDate.Value))
                        {
                            int unreadInformationsCount = GeneralManager.GetNbrOfUnreadInformations(user.LicenseId, param.ActorCompanyId, param.RoleId, param.UserId, false, true, false, UserManager.GetUserLangId(param.UserId), ignoreCache: true);
                            mobileModules.AddMobileModule(MobileModuleType.InternalNews, 0, unreadInformationsCount, false);
                        }

                        if (countUnreadDocuments && GeneralManager.HasNewCompanyDocuments(param.ActorCompanyId, lastFetchDate.Value))
                        {
                            int unreadDocumentsCount = GeneralManager.GetNbrOfUnreadCompanyDocuments(param.ActorCompanyId, param.RoleId, param.UserId, ignoreCache: true);
                            mobileModules.AddMobileModule(MobileModuleType.FileArchive, 0, unreadDocumentsCount, false);
                        }
                    }
                    else
                    {
                        int unreadInformationsCount = GeneralManager.GetNbrOfUnreadInformations(user.LicenseId, param.ActorCompanyId, param.RoleId, param.UserId, false, true, false, UserManager.GetUserLangId(param.UserId), ignoreCache: true);
                        mobileModules.AddMobileModule(MobileModuleType.InternalNews, 0, unreadInformationsCount, false);

                        if(countUnreadDocuments)
                        {
                            int unreadDocumentsCount = GeneralManager.GetNbrOfUnreadCompanyDocuments(param.ActorCompanyId, param.RoleId, param.UserId, ignoreCache: true);
                            mobileModules.AddMobileModule(MobileModuleType.FileArchive, 0, unreadDocumentsCount, false);
                        }
                    }
                }

                return mobileModules;
            }
            catch (Exception e)
            {
                return new MobileModules(param, Texts.InternalErrorMessage + e.Message);
            }

        }

        #endregion

        #region News

        private MobileInternalNews PerformGetAllInternalNews(MobileParam param)
        {
            MobileInternalNews mobileNews = new MobileInternalNews(param);

            // First step of getting new information functionality out to the mobile.
            // TODO: Next we also need to add SysInformation
            List<InformationDTO> informations = GeneralManager.GetCompanyInformations(param.ActorCompanyId, param.RoleId, param.UserId, false, true, false, UserManager.GetUserLangId(param.UserId));
            mobileNews.AddMobileInternalNews(informations);

            return mobileNews;
        }

        private MobileNews PerformGetInternalNews(MobileParam param, int newsId)
        {
            MobileNews mobileNews = null;

            InformationDTO information = GeneralManager.GetCompanyInformation(newsId, param.ActorCompanyId, param.UserId, false);
            if (information != null)
            {
                string text = "";
                if (!String.IsNullOrEmpty(information.ShortText))
                {
                    text = information.ShortText;
                    if (!String.IsNullOrEmpty(information.Text))
                        text += "<br><br>";
                }
                text += information.Text;
                string plainText = StringUtility.HTMLToText(text, true);
                mobileNews = new MobileNews(param, information.InformationId, information.Subject, plainText, information.Created, false);
            }
            else
                mobileNews = new MobileNews(param, GetText(8350, "Kunde inte inte hitta nyhet"));

            return mobileNews;
        }

        #endregion

        #region Information

        private MobileInformation PerformGetCompanyInformation(MobileParam param, int informationId)
        {
            try
            {
                MobileInformation mobileInformation;

                InformationDTO information = GeneralManager.GetCompanyInformation(informationId, param.ActorCompanyId, param.UserId, true);
                if (information != null)
                    mobileInformation = new MobileInformation(param, information.InformationId, (int)information.SourceType, information.Created, (int)information.Severity, information.Subject, information.ShortText, StringUtility.HTMLToText(information.Text, true), information.Folder, information.ReadDate, information.AnswerDate, information.NeedsConfirmation);
                else
                    mobileInformation = new MobileInformation(param, GetText(8350, "Kunde inte inte hitta nyhet"));

                return mobileInformation;
            }
            catch (Exception e)
            {
                LogError("PerformGetCompanyInformation: " + " (" + informationId + ") " + e.Message);
                return new MobileInformation(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileInformations PerformGetCompanyInformations(MobileParam param)
        {
            try
            {
                MobileInformations mobileInformations = new MobileInformations(param);
                List<InformationDTO> informations = GeneralManager.GetCompanyInformations(param.ActorCompanyId, param.RoleId, param.UserId, false, true, false, UserManager.GetUserLangId(param.UserId));
                mobileInformations.AddMobileInformations(informations);

                return mobileInformations;
            }
            catch (Exception e)
            {
                LogError("PerformGetCompanyInformations: " + e.Message);
                return new MobileInformations(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileInformation PerformGetSysInformation(MobileParam param, int informationId)
        {
            try
            {
                MobileInformation mobileInformation;

                InformationDTO information = GeneralManager.GetSysInformation(informationId, true);
                if (information != null)
                    mobileInformation = new MobileInformation(param, information.InformationId, (int)information.SourceType, information.Created, (int)information.Severity, information.Subject, information.ShortText, StringUtility.HTMLToText(information.Text, true), information.Folder, information.ReadDate, information.AnswerDate, information.NeedsConfirmation);
                else
                    mobileInformation = new MobileInformation(param, GetText(8350, "Kunde inte inte hitta nyhet"));

                return mobileInformation;
            }
            catch (Exception e)
            {
                LogError("PerformGetSysInformation: " + e.Message);
                return new MobileInformation(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileInformations PerformGetSysInformations(MobileParam param)
        {
            try
            {
                User user = UserManager.GetUser(param.UserId);
                if (user == null)
                    return new MobileInformations(param, Texts.UserNotFound);

                MobileInformations mobileInformations = new MobileInformations(param);
                List<InformationDTO> informations = GeneralManager.GetSysInformations(user.LicenseId, param.ActorCompanyId, param.RoleId, param.UserId, false, true, false, UserManager.GetUserLangId(param.UserId), useCache: true);
                mobileInformations.AddMobileInformations(informations);

                return mobileInformations;
            }
            catch (Exception e)
            {
                LogError("PerformGetSysInformations: " + e.Message);
                return new MobileInformations(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileInformations PerformGetUnreadInformations(MobileParam param)
        {
            try
            {
                User user = UserManager.GetUser(param.UserId);
                if (user == null)
                    return new MobileInformations(param, Texts.UserNotFound);

                MobileInformations mobileInformations = new MobileInformations(param);
                List<InformationDTO> informations = GeneralManager.GetUnreadInformations(user.LicenseId, param.ActorCompanyId, param.RoleId, param.UserId, false, true, false, UserManager.GetUserLangId(param.UserId));
                mobileInformations.AddMobileInformations(informations);

                return mobileInformations;
            }
            catch (Exception e)
            {
                LogError("PerformGetUnreadInformations: " + e.Message);
                return new MobileInformations(param, GetText(8455, "Internt fel"));
            }
        }
        private MobileResult PerformSetInformationRead(MobileParam param, int informationType, int informationId)
        {
            try
            {
                ActionResult actionResult = null;
                if (informationType == (int)SoeInformationSourceType.Company)
                {
                    actionResult = GeneralManager.SetInformationAsRead(informationId, 0, param.UserId, false, false);
                }
                else if (informationType == (int)SoeInformationSourceType.Sys)
                {
                    actionResult = GeneralManager.SetInformationAsRead(0, informationId, param.UserId, false, false);
                }

                var mobileResult = new MobileResult(param);
                if (actionResult == null)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, false);
                }
                else if (actionResult.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, true);
                }
                else
                {
                    mobileResult = new MobileResult(param, actionResult.ErrorMessage);
                }

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformSetInformationRead: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileResult PerformSetInformationConfirmed(MobileParam param, int informationType, int informationId)
        {
            try
            {
                ActionResult actionResult = null;
                if (informationType == (int)SoeInformationSourceType.Company)
                {
                    actionResult = GeneralManager.SetInformationAsRead(informationId, 0, param.UserId, true, false);
                }
                else if (informationType == (int)SoeInformationSourceType.Sys)
                {
                    actionResult = GeneralManager.SetInformationAsRead(0, informationId, param.UserId, true, false);
                }

                var mobileResult = new MobileResult(param);
                if (actionResult == null)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, false);
                }
                else if (actionResult.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, true);
                }
                else
                {
                    mobileResult = new MobileResult(param, actionResult.ErrorMessage);
                }

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformSetInformationConfirmed: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }
        }

        #endregion

        #region XeMail

        private MobileXeMails PerformGetInboxMail(MobileParam param)
        {
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileXeMails(param, Texts.UserNotFound);

            List<MessageGridDTO> messages = CommunicationManager.GetXEMailItems(XEMailType.Incoming, user.LicenseId, includeMessages: true);

            MobileXeMails mail = new MobileXeMails(param, messages, XEMailType.Incoming);

            return mail;
        }

        private MobileXeMails PerformGetSentMail(MobileParam param)
        {
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileXeMails(param, Texts.UserNotFound);

            List<MessageGridDTO> messages = CommunicationManager.GetXEMailItems(XEMailType.Sent, user.LicenseId, includeMessages: true);

            MobileXeMails mail = new MobileXeMails(param, messages, XEMailType.Sent);

            return mail;
        }

        private MobileXeMail PerformGetMail(MobileParam param, int mailId)
        {
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileXeMail(param, Texts.UserNotFound);

            bool hasAbsenceRequestAdminPermission = FeatureManager.HasRolePermission(Feature.Time_Schedule_AbsenceRequests, Permission.Readonly, param.RoleId, param.ActorCompanyId);

            MessageEditDTO xeMail = CommunicationManager.GetIncomingMessage(mailId, user.LicenseId, user.UserId);
            if (xeMail != null)
            {
                // Check company setting if other recipients should be hidden in shift request
                if (xeMail.MessageType == TermGroup_MessageType.PayrollSlip || (xeMail.MessageType == TermGroup_MessageType.ShiftRequest && SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.HideRecipientsInShiftRequest, 0, param.ActorCompanyId, 0)))
                    xeMail.Recievers = xeMail.Recievers.Where(r => r.UserId == param.UserId).ToList();

                return new MobileXeMail(param, xeMail, XEMailType.Incoming, param.UserId, hasAbsenceRequestAdminPermission: hasAbsenceRequestAdminPermission);
            }

            xeMail = CommunicationManager.GetSentMessage(mailId, user.LicenseId, user.UserId);

            if (xeMail != null)
                return new MobileXeMail(param, xeMail, XEMailType.Sent, param.UserId, hasAbsenceRequestAdminPermission: hasAbsenceRequestAdminPermission);

            return new MobileXeMail(param, Texts.MessageNotFound);
        }

        private MobileXeMail PerformDeleteIncomingMail(MobileParam param, int mailId)
        {
            MobileXeMail xemail = new MobileXeMail(param);

            List<int> messageIds = new List<int>();
            messageIds.Add(mailId);

            ActionResult result = CommunicationManager.DeleteIncomingXEMail(messageIds, param.UserId);

            if (result.Success)
                xemail.SetTaskResult(MobileTask.DeleteIncomingMail, true);
            else
            {
                if (result.ErrorNumber > 0)
                    xemail = new MobileXeMail(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    xemail = new MobileXeMail(param, Texts.DeleteIncomingMailFailed);
            }

            return xemail;
        }

        private MobileResult PerformDeleteOutgoingMail(MobileParam param, int mailId)
        {
            try
            {
                var mobileResult = new MobileResult(param);

                var messageIds = new List<int>();
                messageIds.Add(mailId);

                var result = CommunicationManager.DeleteOutgoingXEMail(messageIds);

                if (result.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.Delete, true);
                }
                else
                {
                    if (result.ErrorNumber > 0)
                        mobileResult = new MobileResult(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                    else
                        mobileResult = new MobileResult(param, GetText(8353, "Meddelandet kunde inte tas bort"));
                }

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformDeleteOutgoingMail: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileXeMail PerformMarkMailAsRead(MobileParam param, int mailId)
        {
            MobileXeMail xemail = new MobileXeMail(param);

            ActionResult result = CommunicationManager.SetXEMailReadDate(DateTime.Now, mailId, param.UserId);

            if (result.Success)
                xemail.SetTaskResult(MobileTask.MarkMailAsRead, true);
            else
            {
                if (result.ErrorNumber > 0)
                    xemail = new MobileXeMail(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    xemail = new MobileXeMail(param, Texts.MarkMailAsReadFailed);
            }

            return xemail;
        }

        private MobileXeMail PerformSetMailIdsAsRead(MobileParam param, string mailIds)
        {
            try
            {
                MobileXeMail xemail = new MobileXeMail(param);

                ActionResult result = CommunicationManager.SetXEMailAsRead(GetIds(mailIds), param.UserId);
                if (result.Success)
                    xemail.SetTaskResult(MobileTask.MarkMailAsRead, true);
                else
                {
                    if (result.ErrorNumber > 0)
                        xemail = new MobileXeMail(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                    else
                        xemail = new MobileXeMail(param, result.ErrorMessage);
                }

                return xemail;
            }
            catch (Exception e)
            {
                LogError("PerformSetMailIdsAsRead: " + e.Message);
                return new MobileXeMail(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileXeMail PerformSetMailIdsAsUnread(MobileParam param, string mailIds)
        {
            try
            {
                MobileXeMail xemail = new MobileXeMail(param);

                ActionResult result = CommunicationManager.SetXEMailAsUnread(GetIds(mailIds), param.UserId);
                if (result.Success)
                    xemail.SetTaskResult(MobileTask.MarkMailAsUnread, true);
                else
                {
                    if (result.ErrorNumber > 0)
                        xemail = new MobileXeMail(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                    else
                        xemail = new MobileXeMail(param, result.ErrorMessage);
                }

                return xemail;
            }
            catch (Exception e)
            {
                LogError("PerformSetMailIdsAsUnread: " + e.Message);
                return new MobileXeMail(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileXeMail PerformDeleteIncomingMailIds(MobileParam param, string mailIds)
        {
            try
            {
                MobileXeMail xemail = new MobileXeMail(param);

                ActionResult result = CommunicationManager.DeleteIncomingXEMail(GetIds(mailIds), param.UserId);
                if (result.Success)
                    xemail.SetTaskResult(MobileTask.DeleteIncomingMail, true);
                else
                {
                    if (result.ErrorNumber > 0)
                        xemail = new MobileXeMail(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                    else
                        xemail = new MobileXeMail(param, result.ErrorMessage);
                }

                return xemail;
            }
            catch (Exception e)
            {
                LogError("PerformDeleteIncomingMailIds: " + e.Message);
                return new MobileXeMail(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileResult PerformDeleteOutgoingMailIds(MobileParam param, string mailIds)
        {
            try
            {
                var mobileResult = new MobileResult(param);

                var result = CommunicationManager.DeleteOutgoingXEMail(GetIds(mailIds));
                if (result.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.Delete, true);
                }
                else
                {
                    if (result.ErrorNumber > 0)
                        mobileResult = new MobileResult(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                    else
                        mobileResult = new MobileResult(param, result.ErrorMessage);
                }

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformDeleteOutgoingMailIds: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileReceivers PerformGetReceivers(MobileParam param, string searchtext)
        {
            MobileReceivers mobileReceivers = new MobileReceivers(param);

            bool rolePermission = FeatureManager.HasRolePermission(Feature.Manage_Roles, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            bool employeeGroupPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Groups, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, param.ActorCompanyId);

            if (!String.IsNullOrEmpty(searchtext))
                searchtext = searchtext.ToLower();

            List<User> users = UserManager.GetUsersByCompany(param.ActorCompanyId, param.RoleId, param.UserId, skipNonEmployeeUsers: true, includeEmployeesWithSameAccount: true, includeEmployeesWithSameAccountOnAttestRole: true);
            mobileReceivers.AddMobileReceivers(users.Where(x => (x.Name != null && x.Name.ToLower().Contains(searchtext)) || (x.LoginName != null && x.LoginName.ToLower().Contains(searchtext))).ToList(), XEMailRecipientType.User);

            if (!useAccountHierarchy)
            {
                Dictionary<int, string> categories = CategoryManager.GetCategoriesDict(SoeCategoryType.Employee, param.ActorCompanyId, false);
                mobileReceivers.AddMobileReceivers(categories.Where(x => x.Value != null && x.Value.ToLower().Contains(searchtext)).ToDictionary(x => x.Key, x => x.Value), XEMailRecipientType.Category);
            }

            if (rolePermission)
            {
                Dictionary<int, string> roles = RoleManager.GetRolesByCompanyDict(param.ActorCompanyId, false, false);
                mobileReceivers.AddMobileReceivers(roles.Where(x => x.Value != null && x.Value.ToLower().Contains(searchtext)).ToDictionary(x => x.Key, x => x.Value), XEMailRecipientType.Role);
            }

            if (employeeGroupPermission)
            {
                Dictionary<int, string> empGroups = EmployeeManager.GetEmployeeGroupsDict(param.ActorCompanyId, false);
                mobileReceivers.AddMobileReceivers(empGroups.Where(x => x.Value != null && x.Value.ToLower().Contains(searchtext)).ToDictionary(x => x.Key, x => x.Value), XEMailRecipientType.Group);
            }

            List<MessageGroupDTO> messageGroups = CommunicationManager.GetMessageGroups(param.ActorCompanyId, param.UserId);
            mobileReceivers.AddMobileReceivers(messageGroups.Where(x => x.Name != null && x.Name.ToLower().Contains(searchtext)).ToList(), XEMailRecipientType.MessageGroup);

            return mobileReceivers;
        }

        private MobileReceiver PerformGetReplyReceiver(MobileParam param, int mailId)
        {
            MobileReceiver receiver = null;
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileReceiver(param, Texts.UserNotFound);

            MessageEditDTO xeMail = CommunicationManager.GetXEMail(mailId, user.LicenseId, user.UserId);

            if (xeMail.SenderUserId.HasValue)
                receiver = new MobileReceiver(param, xeMail.SenderUserId.Value, xeMail.SenderName, XEMailRecipientType.User);
            else
                return new MobileReceiver(param, Texts.ReceiverNotFound);

            return receiver;
        }

        private MobileReceivers PerformGetReplyAllReceivers(MobileParam param, int mailId)
        {
            try
            {
                MobileReceivers receivers = new MobileReceivers(param);
                User user = UserManager.GetUser(param.UserId);
                if (user == null)
                    return new MobileReceivers(param, Texts.UserNotFound);

                MessageEditDTO xeMail = CommunicationManager.GetXEMail(mailId, user.LicenseId, user.UserId);

                foreach (var item in xeMail.Recievers.Where(p => p.UserId != param.UserId).ToList())
                {
                    receivers.AddMobileReceiver(item.UserId, item.Name, item.Type);
                }

                if (xeMail.SenderUserId.HasValue)
                    receivers.AddMobileReceiver(xeMail.SenderUserId.Value, xeMail.SenderName, XEMailRecipientType.User);

                return receivers;
            }
            catch (Exception e)
            {
                LogError("PerformGetReplyAllReceivers: " + e.Message);
                return new MobileReceivers(param, Texts.InternalErrorMessage + e.Message);
            }

        }

        private MobileXeMail PerformAnswerShiftRequest(MobileParam param, int mailId, bool value)
        {
            MobileXeMail xemail = new MobileXeMail(param);
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileXeMail(param, Texts.UserNotFound);

            MessageEditDTO message = CommunicationManager.GetXEMail(mailId, user.LicenseId, user.UserId);

            if (message == null)
                return new MobileXeMail(param, Texts.MessageNotFound);

            XEMailAnswerType answer;
            if (value)
                answer = XEMailAnswerType.Yes;
            else
                answer = XEMailAnswerType.No;

            MessageEditDTO messageDto = new MessageEditDTO()
            {
                LicenseId = user.LicenseId,
                MessagePriority = TermGroup_MessagePriority.Normal,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.Text,
                MessageType = message.MessageType,
                RoleId = param.RoleId,
                MarkAsOutgoing = false,
                SenderName = user.Name,
                SenderEmail = string.Empty,
                SenderUserId = user.UserId,
                Entity = message.Entity,
                ActorCompanyId = param.ActorCompanyId,
                RecordId = 0,
                AnswerType = answer,
                Recievers = message.Recievers,
                Subject = message.Subject,
                Text = message.Text,
                ShortText = message.ShortText,
                ParentId = message.MessageId
            };

            ActionResult result = CommunicationManager.SendXEMail(messageDto, param.ActorCompanyId, param.RoleId, param.UserId);

            if (result.Success)
                xemail.SetTaskResult(MobileTask.AnswerShiftRequest, true);
            else
            {
                if (result.ErrorNumber > 0)
                    xemail = new MobileXeMail(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    xemail = new MobileXeMail(param, Texts.SendAnswerFailed);
            }

            return xemail;
        }

        private MobileXeMail PerformSendMail(MobileParam param, int mailId, int parentMailId, string subject, string text,
           string userIds, string roleIds, string employeeGroupIds, string categoryIds, string messageGroupIds, byte[] imageData, string imageName, bool forward)//NOSONAR
        {
            MobileXeMail xemail = new MobileXeMail(param);

            //fix. looks like mailId is parentMailId. i dont know what should be in mailId..it is not even used
            parentMailId = mailId;
            mailId = 0;//NOSONAR

            text = text.Replace("\n", "<br>");
            try
            {
                User user = UserManager.GetUser(param.UserId);
                if (user == null)
                    return new MobileXeMail(param, Texts.UserNotFound);

                #region Parse reciepients

                List<MessageRecipientDTO> receivers = new List<MessageRecipientDTO>();
                char[] separator = new char[1];
                separator[0] = ',';

                #region Users
                string[] separatedUserIds = userIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in separatedUserIds)
                {
                    if (Int32.TryParse(id.Trim(), out int userId))
                        receivers.Add(new MessageRecipientDTO() { Type = XEMailRecipientType.User, UserId = userId });
                }
                #endregion

                #region Roles
                string[] separatedRoleIds = roleIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in separatedRoleIds)
                {
                    if (Int32.TryParse(id.Trim(), out int roleId))
                        receivers.Add(new MessageRecipientDTO() { Type = XEMailRecipientType.Role, UserId = roleId });
                }
                #endregion

                #region EmployeeGroups
                string[] separatedEmployeeGroupIds = employeeGroupIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in separatedEmployeeGroupIds)
                {
                    if (Int32.TryParse(id.Trim(), out int empGroupId))
                        receivers.Add(new MessageRecipientDTO() { Type = XEMailRecipientType.Group, UserId = empGroupId });
                }
                #endregion

                #region CategoryIds
                string[] separatedCategoryIds = categoryIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in separatedCategoryIds)
                {
                    if (Int32.TryParse(id.Trim(), out int categoryId))
                        receivers.Add(new MessageRecipientDTO() { Type = XEMailRecipientType.Category, UserId = categoryId });
                }
                #endregion

                #region MessageGroupIds
                string[] separatedMessageGroupIds = messageGroupIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in separatedMessageGroupIds)
                {
                    if (Int32.TryParse(id.Trim(), out int messageGroupId))
                        receivers.Add(new MessageRecipientDTO() { Type = XEMailRecipientType.MessageGroup, UserId = messageGroupId });
                }
                #endregion

                #region Validate Receivers

                bool validationFailed = false;

                foreach (MessageRecipientDTO dto in receivers)
                {
                    if (dto.Type == XEMailRecipientType.Group)
                    {
                        #region Group

                        int employeeGroupId = dto.UserId;
                        EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(employeeGroupId);
                        if (employeeGroup == null || employeeGroup.ActorCompanyId != param.ActorCompanyId)
                        {
                            validationFailed = true;
                            break;
                        }

                        #endregion
                    }
                    else if (dto.Type == XEMailRecipientType.Role)
                    {
                        #region Role

                        int roleId = dto.UserId;
                        var role = RoleManager.GetRole(roleId, param.ActorCompanyId);
                        if (role == null || role.ActorCompanyId != param.ActorCompanyId)
                        {
                            validationFailed = true;
                            break;
                        }
                        #endregion
                    }
                    else if (dto.Type == XEMailRecipientType.Category)
                    {
                        #region Category

                        int categoryId = dto.UserId;
                        var category = CategoryManager.GetCategory(categoryId, param.ActorCompanyId);
                        if (category == null || category.ActorCompanyId != param.ActorCompanyId)
                        {
                            validationFailed = true;
                            break;
                        }

                        #endregion
                    }
                    else if (dto.Type == XEMailRecipientType.MessageGroup)
                    {
                        #region MessageGroup

                        int messageGroupId = dto.UserId;
                        var messageGroup = CommunicationManager.GetMessageGroup(messageGroupId);
                        if (messageGroup == null || messageGroup.ActorCompanyId != param.ActorCompanyId)
                        {
                            validationFailed = true;
                            break;
                        }

                        #endregion
                    }
                    else
                    {
                        #region User

                        User tempuser = UserManager.GetUser(dto.UserId, loadUserCompanyRole: true);
                        if (tempuser != null && !tempuser.UserCompanyRole.IsLoaded)
                            tempuser.UserCompanyRole.Load();

                        if (tempuser == null || !tempuser.UserCompanyRole.Any(x => x.ActorCompanyId == param.ActorCompanyId))
                        {
                            validationFailed = true;
                            break;
                        }

                        #endregion
                    }
                }

                if (validationFailed)
                {
                    string msg = string.Format("Version: {0}, UserId: {1}, RoleId: {2}, CompanyId: {3}. ", param.Version, param.UserId, param.RoleId, param.ActorCompanyId);
                    msg += String.Format("ParentMailId: {0}, UserIds: {1} - RoleIds: {2} - EmployeeGroupIds: {3} - CategoryIds: {4} - MessageGroupIds: {5}", parentMailId, userIds, roleIds, employeeGroupIds, categoryIds, messageGroupIds);
                    LogError("PerformSendMail - validationFailed: " + msg);

                    return new MobileXeMail(param, "Felaktigt data: mottagare ej giltig.");
                }

                #endregion

                #endregion

                var attachments = new List<MessageAttachmentDTO>();

                if (parentMailId > 0)
                {

                    MessageEditDTO parentMail = CommunicationManager.GetIncomingMessage(parentMailId, user.LicenseId, user.UserId);
                    if (parentMail != null)
                    {
                        if (forward)
                        {
                            attachments.AddRange(parentMail.Attachments);
                            attachments.ForEach(c => { c.MessageAttachmentId = 0; });
                        }
                        else
                        {
                            //Its a reply
                            string recievers = "";

                            foreach (var reciever in parentMail.Recievers)
                            {
                                recievers += reciever.Name + "; ";
                            }

                            string replyHeader =
                                               GetText(5241, "Från") + ": " + user.Name + "<br>" +
                                               GetText(8653, "Skickat") + ": " + (parentMail.SentDate.HasValue ? parentMail.SentDate.Value.ToShortDateShortTimeString() : "") + "<br>" +
                                               GetText(5242, "Till") + ": " + recievers + "<br>" +
                                               GetText(8654, "Ämne") + ": " + parentMail.Subject + "<br>" + "<br>";


                            text = text + "<br>" + "<br>" + replyHeader + parentMail.ShortText;

                            if (string.IsNullOrEmpty(subject))
                            {
                                subject = "SV: " + parentMail.Subject;
                            }
                        }
                    }
                }

                if (imageData != null)
                {
                    var messageAttachmentDTO = new MessageAttachmentDTO();

                    messageAttachmentDTO.Data = imageData;
                    messageAttachmentDTO.Filesize = imageData.LongLength;
                    messageAttachmentDTO.Name = !string.IsNullOrEmpty(imageName) ? imageName : Guid.NewGuid().ToString();
                    messageAttachmentDTO.Name += ".jpg";

                    attachments.Add(messageAttachmentDTO);
                }

                #region Create message dto
                MessageEditDTO messageDto = new MessageEditDTO()
                {
                    LicenseId = user.LicenseId,
                    MessagePriority = TermGroup_MessagePriority.Normal,
                    MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                    MessageTextType = TermGroup_MessageTextType.Text,
                    MessageType = TermGroup_MessageType.UserInitiated,
                    Recievers = receivers,
                    RoleId = param.RoleId,
                    MarkAsOutgoing = false,
                    SenderName = user.Name,
                    SenderEmail = string.Empty,
                    Subject = subject,
                    Text = text,
                    ShortText = text,
                    ParentId = parentMailId == 0 ? null : (int?)parentMailId,
                    ReplyDate = parentMailId == 0 || forward ? (DateTime?)null : DateTime.Now,
                    ForwardDate = forward ? DateTime.Now : (DateTime?)null,
                    AnswerType = XEMailAnswerType.None,
                    Entity = 0,
                    RecordId = 0,
                    ActorCompanyId = param.ActorCompanyId,
                    Attachments = attachments
                };

                #endregion

                ActionResult result = CommunicationManager.SendXEMail(messageDto, param.ActorCompanyId, param.RoleId, param.UserId);

                if (result.Success)
                    xemail.SetTaskResult(MobileTask.SendMail, true);
                else
                {
                    if (result.ErrorNumber > 0)
                        xemail = new MobileXeMail(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                    else
                        xemail = new MobileXeMail(param, Texts.SendMailFailed);
                }
            }
            catch (Exception e)
            {
                LogError("PerformSendMail: " + e.Message);
                xemail = new MobileXeMail(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
            return xemail;
        }

        private MobileFiles PerformGetMailAttachments(MobileParam param, int mailId)
        {
            MobileFiles mobileFiles = new MobileFiles(param);
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileFiles(param, Texts.UserNotFound);

            MessageEditDTO xeMail = CommunicationManager.GetXEMail(mailId, user.LicenseId, user.UserId);
            if (xeMail != null)
                mobileFiles.AddMobileFiles(xeMail.Attachments);
            else
                return new MobileFiles(param, Texts.MessageNotFound);

            return mobileFiles;
        }

        private MobileFile PerformGetMailAttachment(MobileParam param, int attachmentId)
        {
            MobileFile mobileFile = null;
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileFile(param, Texts.UserNotFound);

            MessageAttachmentDTO attachment = GeneralManager.GetAttachment(attachmentId);

            if (attachment == null || attachment.Data == null)
                return new MobileFile(param, Texts.FileNotFound);

            string path = CreateFileOnServer(param, attachment.MessageAttachmentId, attachment.Name, attachment.Data);
            mobileFile = new MobileFile(param, attachment.MessageAttachmentId, attachment.Name, path, "", null, false, null, "");

            return mobileFile;
        }

        #endregion

        #region Archived Files

        private MobileFiles PerformGetArchivedFiles(MobileParam param)
        {
            MobileFiles mobileFiles = new MobileFiles(param);

            List<DataStorageDTO> documents = GeneralManager.GetCompanyDocuments(param.ActorCompanyId, param.RoleId, param.UserId, addDataStorageRecipients: true, addDataStorageRecords: false, includeUserUploaded: true);
            mobileFiles.AddMobileFiles(documents);

            return mobileFiles;
        }

        private MobileFile PerformGetArchivedFile(MobileParam param, int dataStorageId)
        {
            MobileFile mobileFile = null;


            DataStorage file = GeneralManager.GetDataStorage(dataStorageId, param.ActorCompanyId, false, true);
            if (file == null || file.Data == null)
                return new MobileFile(param, Texts.FileNotFound);

            DataStorageRecipient recipient = file.DataStorageRecipient.FirstOrDefault(x => x.UserId == param.UserId);

            bool useAzurestorage = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_24);

            string path = useAzurestorage ? GeneralManager.GetDocumentUrl(file.DataStorageId, param.ActorCompanyId) : CreateFileOnServer(param, file.DataStorageId, file.FileName, file.Data);
            mobileFile = new MobileFile(param, file.DataStorageId, file.FileName, path, file.Folder, recipient?.ReadDate, false, recipient?.ConfirmedDate, "");

            return mobileFile;
        }

        private MobileFiles PerformGetMyDocuments(MobileParam param)
        {
            MobileFiles mobileFiles = new MobileFiles(param);

            var myDocuments = GeneralManager.GetMyDocuments(param.ActorCompanyId, param.RoleId, param.UserId);
            mobileFiles.AddMobileFiles(myDocuments);

            return mobileFiles;
        }
        private MobileResult PerformSetDocumentRead(MobileParam param, int documentId, bool confirmed)
        {
            try
            {
                if (documentId == 0)
                    return new MobileResult(param, GetText(8918, "Dokumentet kunde inte sättas som läst"));

                ActionResult actionResult = GeneralManager.SetDocumentAsRead(documentId, param.UserId, confirmed);

                var mobileResult = new MobileResult(param);
                if (actionResult == null)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, false);
                }
                else if (actionResult.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, true);
                }
                else
                {
                    mobileResult = new MobileResult(param, actionResult.ErrorMessage);
                }

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformSetDocumentRead: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }
        }

        #endregion

        #region EmployeeDetails
        /// <summary>
        /// Used to show colleagues for an employee
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>

        private MobileEmployeeList PerformGetEmployeeList(MobileParam param)
        {
            MobileEmployeeList employeeList = new MobileEmployeeList(param);
            List<Employee> employees;
            bool contactModifyPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact, Permission.Modify, param.RoleId, param.ActorCompanyId);
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SeAllEmployees, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                employees = EmployeeManager.GetAllEmployees(param.ActorCompanyId, active: true, getVacant: false);
            else
                employees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, param.ActorCompanyId, param.UserId, param.RoleId, getVacant: false, useShowOtherEmployeesPermission: true).OrderBy(x => x.FirstName).ToList();

            List<ContactEcomView> contactEcoms = ContactManager.GetContactEcoms(param.ActorCompanyId);
            employeeList.AddToMobileEmployeeList(employees, contactEcoms, contactModifyPermission);

            return employeeList;
        }

        private MobileEmployeeDetails PerformGetEmployeeDetails(MobileParam param, int employeeId)
        {
            MobileEmployeeDetails employeeDetails = null;
            List<ContactECom> employeeEcoms = new List<ContactECom>();
            List<ContactAddress> employeeAddress = new List<ContactAddress>();
            int contactId = 0;

            Employee employee = EmployeeManager.GetEmployeeIgnoreState(param.ActorCompanyId, employeeId, loadContactPerson: true, loadUser: true);
            if (employee == null)
                return new MobileEmployeeDetails(param, Texts.EmployeeNotFoundMessage);

            if (employee.User == null)
                return new MobileEmployeeDetails(param, Texts.UserNotFound);

            ContactPerson contactPerson = employee.ContactPerson;
            if (contactPerson != null)
            {
                contactId = ContactManager.GetContactIdFromActorId(contactPerson.ActorContactPersonId);
                employeeEcoms = ContactManager.GetContactEComs(contactId);
                employeeAddress = ContactManager.GetContactAddresses(contactId);
            }

            employeeDetails = new MobileEmployeeDetails(param, employee, contactPerson, employeeEcoms, employeeAddress);
            LogPersonalData(employeeDetails, TermGroup_PersonalDataActionType.Read, "PerformGetEmployeeDetails()");
            return employeeDetails;
        }

        private MobileEmployeeDetails PerformSaveEmployeeDetails(MobileParam param, int actorCompanyId, int employeeId, string firstName, string lastName, int addressId, string address, string postalCode, string postalAddress, int closestRelativeId, string closestRelativePhone, string closestRelativeName, string closestRelativeRelation, bool? closestRelativeIsSecret, int closestRelativeId2, string closestRelative2, string closestRelativeName2, string closestRelativeRelation2, bool closestRelativeIsSecret2, int mobileId, string mobile, int emailId, string email)
        {
            MobileEmployeeDetails employeeDetails = new MobileEmployeeDetails(param);
            if (email == "" || !Validator.ValidateEmail(email))
                return new MobileEmployeeDetails(param, GetText(1523, "Du måste ange en korrekt e-postadress"));

            if (!FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Contact, Permission.Modify, param.RoleId, param.ActorCompanyId))
                return new MobileEmployeeDetails(param, GetText(8652, "Otillåten ändring, behörighet saknas"));

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_14))
            {
                closestRelativeName = null;
                closestRelativeRelation = null;
                closestRelativeIsSecret = null;
            }

            ActionResult result = UpdateMobileEmployee(actorCompanyId, employeeId, firstName, lastName, addressId, address, postalCode, postalAddress, closestRelativeId, closestRelativePhone, closestRelativeName, closestRelativeRelation, closestRelativeIsSecret, closestRelativeId2, closestRelative2, closestRelativeName2, closestRelativeRelation2, closestRelativeIsSecret2, mobileId, mobile, emailId, email);
            if (result.Success)
                employeeDetails.SetTaskResult(MobileTask.SaveEmployeeDetails, true);
            else
            {
                if (result.ErrorNumber > 0)
                    employeeDetails = new MobileEmployeeDetails(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    employeeDetails = new MobileEmployeeDetails(param, Texts.SaveEmployeeFailed);
            }

            return employeeDetails;
        }

        #endregion

        #region StateAnalysis

        private MobileHTMLView PerformGetFinVoiceHTML(MobileParam param, int fInvoiceEdiEntry)
        {
            var edientry = EdiManager.GetEdiEntry(fInvoiceEdiEntry, param.ActorCompanyId, ignoreState: true);

            var htmlData = SoftOne.Soe.Business.Util.Finvoice.FInvoiceFileGen.GetHTML(edientry);
            if (string.IsNullOrEmpty(htmlData))
            {
                return new MobileHTMLView(param, "Falied Generating HTML");
            }

            return new MobileHTMLView(param, new StringBuilder(htmlData));
        }

        //Code has been copied from StateAnalysis.xaml.cs
        private MobileHTMLView PerformGetStateAnalysis(MobileParam param)
        {
            try
            {
                #region Perform

                List<SoeStatesAnalysis> statesToAnalyse = new List<SoeStatesAnalysis>();
                StringBuilder htmlData = new StringBuilder();

                #region InitPermission() in  StateAnalysis.xaml.cs

                #region General

                // No permission required for these
                bool showRoleInfo = true;
                bool showUserInfo = true;
                bool showEmployeeInfo = true;
                bool showCustomerInfo = true;
                bool showSupplierInfo = true;
                bool showInvoiceProductInfo = true;

                bool showGeneralExpander = true;

                bool showSalesPrice = FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowSalesPrice, Permission.Readonly, param.RoleId, param.ActorCompanyId);

                #endregion

                #region Billing

                bool showOfferInfo = FeatureManager.HasRolePermission(Feature.Billing_Offer_Status, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                bool showContractInfo = FeatureManager.HasRolePermission(Feature.Billing_Contract_Status, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                bool showOrderInfo = FeatureManager.HasRolePermission(Feature.Billing_Order_Status, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                bool showInvoiceInfo = FeatureManager.HasRolePermission(Feature.Billing_Invoice_Status, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                bool showOrderRemainingAmountInfo = FeatureManager.HasRolePermission(Feature.Billing_Order_Status, Permission.Readonly, param.RoleId, param.ActorCompanyId);

                bool showBillingExpander = showOfferInfo || showOrderInfo || showContractInfo || showInvoiceInfo;

                #endregion

                #region CustomerLedger

                bool showUnpayedCustomerInvoicesInfo = FeatureManager.HasRolePermission(Feature.Economy_Customer_Invoice, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                bool showOverduedCustomerInvoicesInfo = FeatureManager.HasRolePermission(Feature.Economy_Customer_Invoice, Permission.Readonly, param.RoleId, param.ActorCompanyId);

                bool showCustomerLedgerExpander = showUnpayedCustomerInvoicesInfo || showOverduedCustomerInvoicesInfo;

                #endregion

                #region SupplierLedger

                bool showUnpayedSupplierInvoicesInfo = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                bool showOverduedSupplierInvoicesInfo = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice, Permission.Readonly, param.RoleId, param.ActorCompanyId);

                bool showSupplierLedgerExpander = showUnpayedSupplierInvoicesInfo || showOverduedSupplierInvoicesInfo;

                #endregion

                #region HouseholdTaxDeduction

                bool showHouseHoldTaxdeductionInfo = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, param.ActorCompanyId, 0) > 0 || SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductRUTTaxDeduction, 0, param.ActorCompanyId, 0) > 0;
                bool showHouseHoldTaxdeductionExpander = showHouseHoldTaxdeductionInfo;

                #endregion

                #endregion

                #region InitStateAnalysis() in StateAnalysis.xaml.cs

                #region General

                statesToAnalyse.Add(SoeStatesAnalysis.InActiveTerminals);

                if (showRoleInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.Role);
                }
                if (showUserInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.User);
                }
                if (showEmployeeInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.Employee);
                }
                if (showCustomerInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.Customer);
                }
                if (showSupplierInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.Supplier);
                }
                if (showInvoiceProductInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.InvoiceProduct);
                }

                #endregion

                #region Billing

                if (showOfferInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.Offer);
                }
                if (showContractInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.Contract);
                }
                if (showOrderInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.Order);
                }
                if (showInvoiceInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.Invoice);
                }
                if (showOrderRemainingAmountInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.OrderRemaingAmount);
                }

                #endregion

                #region CustomerLedgerExpander

                //Not implemented here, but needed in StartPage
                statesToAnalyse.Add(SoeStatesAnalysis.CustomerInvoicesOpen);

                if (showUnpayedCustomerInvoicesInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.CustomerPaymentsUnpayed);
                }
                if (showOverduedCustomerInvoicesInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.CustomerInvoicesOverdued);
                }

                #endregion

                #region SupplierLegderExpander

                //Not implemented here, but needed in StartPage
                statesToAnalyse.Add(SoeStatesAnalysis.SupplierInvoicesOpen);

                if (showUnpayedSupplierInvoicesInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.SupplierInvoicesUnpayed);
                }
                if (showOverduedSupplierInvoicesInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.SupplierInvoicesOverdued);
                }

                #endregion

                #region Edi

                //Not implemented here, but needed in StartPage
                statesToAnalyse.Add(SoeStatesAnalysis.EdiError);
                statesToAnalyse.Add(SoeStatesAnalysis.EdiOrderError);
                statesToAnalyse.Add(SoeStatesAnalysis.EdiInvoicError);

                #endregion

                #region Scanning

                //Not implemented here, but needed in StartPage
                statesToAnalyse.Add(SoeStatesAnalysis.ScanningError);
                statesToAnalyse.Add(SoeStatesAnalysis.ScanningInvoiceError);
                statesToAnalyse.Add(SoeStatesAnalysis.ScanningUnprocessedArrivals);

                #endregion

                #region Communication

                statesToAnalyse.Add(SoeStatesAnalysis.NewMessages);

                #endregion

                #region HouseholdTaxDeduction

                if (showHouseHoldTaxdeductionInfo)
                {
                    statesToAnalyse.Add(SoeStatesAnalysis.HouseHoldTaxDeductionApplied);
                    statesToAnalyse.Add(SoeStatesAnalysis.HouseHoldTaxDeductionApply);
                    statesToAnalyse.Add(SoeStatesAnalysis.HouseHoldTaxDeductionDenied);
                    statesToAnalyse.Add(SoeStatesAnalysis.HouseHoldTaxDeductionReceived);
                }

                #endregion

                #endregion

                #region LoadStateAnalysis() in StateAnalysis.xaml.cs

                //Split states into groups and make one call per group
                var groupGeneral = GetStatesAnalysisGroup(SoeStatesAnalysisGroup.General, statesToAnalyse);
                var groupBillingAndLedger = GetStatesAnalysisGroup(SoeStatesAnalysisGroup.BillingAndLedger, statesToAnalyse);
                var groupHousehold = GetStatesAnalysisGroup(SoeStatesAnalysisGroup.HouseholdTaxDeduction, statesToAnalyse);

                List<StateAnalysisDTO> groupGeneralResultDtos = AnalysisManager.GetStateAnalysis(groupGeneral, param.ActorCompanyId, param.RoleId);
                List<StateAnalysisDTO> groupBillingAndLedgerResultDtos = AnalysisManager.GetStateAnalysis(groupBillingAndLedger, param.ActorCompanyId, param.RoleId);
                List<StateAnalysisDTO> groupHouseholdResultDtos = AnalysisManager.GetStateAnalysis(groupHousehold, param.ActorCompanyId, param.RoleId);

                #endregion

                #region Genereate HTML

                htmlData.Append("<!DOCTYPE html>");
                htmlData.Append("<html>");
                htmlData.Append("<head>");
                htmlData.Append("<meta name=\"format-detection\" content=\"telephone=no\">");

                #region Style/css

                //if we need 1 more column we maybe can set the padding to zero??
                htmlData.Append("<style TYPE=\"text/css\">");
                htmlData.Append(".header { border: 1px solid #D3D3D3; font-size: 12px; font-weight: bold; background: gray; color: #FFFFFF; letter-spacing: 2px; text-transform: uppercase;}");
                htmlData.Append("td { padding: 5px 5px 5px 10px; text-transform: uppercase;font-size: 8px;}");
                htmlData.Append(".col_head { font-weight: bold; text-transform: uppercase; font-size: 8px;}");

                htmlData.Append("</style>");

                #endregion

                #region Headline

                htmlData.Append("<h5>");
                htmlData.Append(GetText(8454, "Hur är läget?"));
                htmlData.Append("</h5>");

                #endregion

                htmlData.Append("</head>");

                #region Body
                htmlData.Append("<body>");

                #region Table

                htmlData.Append("<table cellspacing =\"0\" width = \"100%\" >");

                #region Generate groups/sections

                #region Genereate Group General

                if (showGeneralExpander)
                    GenerateGroupGeneralHtml(ref htmlData, groupGeneralResultDtos);

                #endregion

                #region Generate Group Billing

                if (showBillingExpander)
                    GenerateGroupBillingHtml(ref htmlData, groupBillingAndLedgerResultDtos, showSalesPrice);

                #endregion

                #region Generate Group Household

                if (showHouseHoldTaxdeductionExpander)
                    GenerateGroupHouseHoldHtml(ref htmlData, groupHouseholdResultDtos);

                #endregion


                #region Generate Group CustomerLedger

                if (showCustomerLedgerExpander)
                    GenerateGroupCustomerLedgerHtml(ref htmlData, groupBillingAndLedgerResultDtos);


                #endregion

                #region Generate Group SupllierLedger

                if (showSupplierLedgerExpander)
                    GenerateGroupSupplierLedgerHtml(ref htmlData, groupBillingAndLedgerResultDtos);


                #endregion

                #region Generate Group Edi

                //Not implemented in XE

                #endregion

                #region Generate Group Scanning

                //Not implemented in XE

                #endregion

                #region Generate Group Communication

                //Not implemented in XE

                #endregion

                #endregion

                htmlData.Append("</table>");

                #endregion

                htmlData.Append("</body>");

                #endregion

                htmlData.Append("</html>");
                #endregion

                #endregion

                MobileHTMLView htmlView = new MobileHTMLView(param, htmlData);

                return htmlView;
            }
            catch (Exception e)
            {
                StringBuilder htmlData = new StringBuilder();

                string errorMsg = GetText(8455, "Internt fel") + ": " + "\n";

                if (!string.IsNullOrEmpty(e.Message))
                    errorMsg += e.Message;

                htmlData.Append("<!DOCTYPE html>");
                htmlData.Append("<html>");
                htmlData.Append("<head>");
                htmlData.Append("</head>");
                htmlData.Append("<body>");
                htmlData.Append("<b>");
                htmlData.Append(errorMsg);
                htmlData.Append("</b>");
                htmlData.Append("</body>");
                htmlData.Append("</html>");
                MobileHTMLView htmlView = new MobileHTMLView(param, htmlData);
                return htmlView;
            }
        }

        #endregion

        #region EmployeeUserSettings

        private MobileEmployeeUserSettings PerformGetEmployeeUserSettings(MobileParam param, int employeeId)
        {
            MobileEmployeeUserSettings settings;

            try
            {
                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadUser: true);
                if (employee == null)
                    return new MobileEmployeeUserSettings(param, Texts.EmployeeNotFoundMessage);

                if (employee.User == null)
                    return new MobileEmployeeUserSettings(param, Texts.UserNotFound);

                bool showWantExtraShifts = !FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Contact_NoExtraShift, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                settings = new MobileEmployeeUserSettings(param, employee.EmployeeId, employee.WantsExtraShifts, showWantExtraShifts);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileEmployeeUserSettings(param, GetText(8778, "Ett fel inträffade"));
            }

            return settings;
        }

        private MobileEmployeeUserSettings PerformSaveEmployeeUserSettings(MobileParam param, int employeeId, bool wantsExtraShifts)
        {
            try
            {
                bool showWantExtraShifts = !FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Contact_NoExtraShift, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                MobileEmployeeUserSettings settings = new MobileEmployeeUserSettings(param, employeeId, wantsExtraShifts, showWantExtraShifts);

                if (settings == null || settings.EmployeeId == 0)
                    return new MobileEmployeeUserSettings(param, Texts.EmployeeNotFoundMessage);

                var result = EmployeeManager.SaveEmployeeUserSettingsFromMobile(settings.EmployeeId, param.UserId, param.ActorCompanyId, settings.WantsExtraShifts, settings.ShowWantsExtraShifts);

                if (result.Success)
                    settings.SetTaskResult(MobileTask.SaveEmployeeUserSettings, true);
                else
                    settings = new MobileEmployeeUserSettings(param, Texts.SaveEmployeeFailed);

                return settings;
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileEmployeeUserSettings(param, GetText(8778, "Ett fel inträffade"));
            }

        }

        #endregion

        #region Settings

        private MobilePermissions PerformGetSettingsPermissions(MobileParam param)
        {
            List<Tuple<string, int>> features = new List<Tuple<string, int>>();
            features.Add(Tuple.Create("showChangePWD", 0));

            features.Add(Tuple.Create("showDelegateMySelf", FeatureManager.HasRolePermission(Feature.Manage_Users_Edit_Delegate_MySelf, Permission.Readonly, param.RoleId, param.ActorCompanyId) ? 1 : 0));
            features.Add(Tuple.Create("canDelegateMyRolesAndAttestRoles", FeatureManager.HasRolePermission(Feature.Manage_Users_Edit_Delegate_MySelf_OwnRolesAndAttestRoles, Permission.Readonly, param.RoleId, param.ActorCompanyId) ? 1 : 0));

            // We currently only have one setting hide the entire settings option when when the permission is off
            features.Add(Tuple.Create("showUserSettings", !FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Contact_NoExtraShift, Permission.Readonly, param.RoleId, param.ActorCompanyId) ? 1 : 0));

            MobilePermissions mobileFeatures = new MobilePermissions(param, features);
            return mobileFeatures;
        }

        #endregion

        #endregion

        #region Billing

        #region Product

        private MobileProducts PerformSearchProducts(MobileParam param, int orderId, string search)
        {
            if (param == null)
                return new MobileProducts(param, Texts.ProductsNotFoundMessage);

            //Get Order
            if (orderId == 0)
                return new MobileProducts(param, Texts.OrderNotFoundMessage);

            var order = InvoiceManager.GetCustomerInvoiceSmallEx(orderId);
            if (order == null || !order.PriceListTypeId.HasValue)
                return new MobileProducts(param, Texts.OrderNotFoundMessage);

            //Get internal products
            List<InvoiceProductPriceSearchDTO> products = ProductManager.GetInvoiceProductsBySearch(param.ActorCompanyId, search, MobileProducts.MAXFETCH, order.PriceListTypeId.Value, order.ActorId.Value, order.CurrencyId, order.SysWholesellerId ?? 0, true, true, includeCustomerProducts: true).ToList();
            List<CompanySettingType> baseProductsToExclude = new List<CompanySettingType>();
            baseProductsToExclude.Add(CompanySettingType.ProductHouseholdTaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductHouseholdTaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductHousehold50TaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductHousehold50TaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductRUTTaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductRUTTaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen15TaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen15TaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen20TaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen20TaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen50TaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen50TaxDeductionDenied);

            var useExtendSearchInfo = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowExtendedInfoInExternalSearch, 0, param.ActorCompanyId, 0);

            ProductManager.ExcludeBaseProducts(ref products, baseProductsToExclude, param.ActorCompanyId, param.UserId);

            return new MobileProducts(param, products, useExtendSearchInfo);
        }

        private MobileExternalProducts PerformSearchExternalProducts(MobileParam param, int orderId, string search)
        {
            if (param == null)
                return new MobileExternalProducts(param, Texts.ProductsNotFoundMessage);

            //Get Order
            if (orderId == 0)
                return new MobileExternalProducts(param, Texts.OrderNotFoundMessage);

            var order = InvoiceManager.GetCustomerInvoiceSmall(orderId);
            if (order == null || !order.PriceListTypeId.HasValue)
                return new MobileExternalProducts(param, Texts.OrderNotFoundMessage);

            //Get external products
            var products = ProductManager.SearchInvoiceProducts(param.ActorCompanyId, search, MobileExternalProducts.MAXFETCH);

            var useExtendSearchInfo = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowExtendedInfoInExternalSearch, 0, param.ActorCompanyId, 0);

            return new MobileExternalProducts(param, products, useExtendSearchInfo);
        }

        private MobileExternalProductPrices PerformSearchExternalProductPrices(MobileParam param, int orderId, int sysproductId, string productnr)
        {
            if (param == null)
                return new MobileExternalProductPrices(param, Texts.ProductsNotFoundMessage);

            //Get Order
            if (orderId == 0)
                return new MobileExternalProductPrices(param, Texts.OrderNotFoundMessage);

            CustomerInvoice order = InvoiceManager.GetCustomerInvoice(orderId, false, true, false, false, false, false, false, false, false, false, false, false);
            if (order == null || !order.PriceListTypeId.HasValue || !order.ActorId.HasValue)
                return new MobileExternalProductPrices(param, Texts.OrderNotFoundMessage);

            //Get external product pricess
            List<InvoiceProductPriceSearchViewDTO> productPricess = ProductManager.SearchInvoiceProductPrices(param.ActorCompanyId, order.PriceListTypeId.Value, order.ActorId.Value, new List<string>() { productnr }, true).ToList();

            return new MobileExternalProductPrices(param, productPricess, order.SysWholeSellerId);
        }

        private MobileProduct PerformGetInvoiceProductFromSys(MobileParam param, int orderId, int sysProductId, int sysWholeSellerId)
        {
            if (param == null)
                return new MobileProduct(param, Texts.ProductsNotFoundMessage);

            //Get Order
            if (orderId == 0)
                return new MobileProduct(param, Texts.OrderNotFoundMessage);

            CustomerInvoice order = InvoiceManager.GetCustomerInvoice(orderId, false, true, false, false, false, false, false, false, false, false, false, true);
            if (order == null || !order.PriceListTypeId.HasValue || !order.ActorId.HasValue)
                return new MobileProduct(param, Texts.OrderNotFoundMessage);

            var wholeSeller = WholeSellerManager.GetSysWholesellerDTO(sysWholeSellerId);
            if (wholeSeller == null)
                return new MobileProduct(param, "Wholeseller not found");

            InvoiceProductPriceSearchViewDTO productPrice = null;
            try
            {
                productPrice = ProductManager.SearchInvoiceProductPrice(param.ActorCompanyId, order.PriceListTypeId.Value, order.ActorId.Value, order.CurrencyId, sysProductId, sysWholeSellerId, true);
            }
            catch (Exception ex)
            {
                LogError("MobileManager.GetInvoiceProductFromSys.PerformGetInvoiceProductFromSys :" + ex.Message);
            }

            if (productPrice == null)
                return new MobileProduct(param, GetText(8331, "Artikelrad kunde inte hittas"));

            PriceListOrigin priceListOrigin = productPrice.PriceListOrigin > 0 ? (PriceListOrigin)productPrice.PriceListOrigin : wholeSeller.IsOnlyInComp ? PriceListOrigin.CompDbPriceList : PriceListOrigin.SysDbPriceList;

            decimal salesPrice = productPrice.CustomerPrice ?? 0;
            decimal purchasePrice = productPrice.NettoNettoPrice ?? 0;

            InvoiceProduct invoiceProduct = ProductManager.GetInvoiceProductFromSys(sysProductId, sysWholeSellerId, param.ActorCompanyId, salesPrice, purchasePrice, priceListOrigin, productPrice.PurchaseUnit);
            if (invoiceProduct == null)
            {
                return new MobileProduct(param);
            }
            else
            {
                return new MobileProduct(param, invoiceProduct);
            }
        }

        private MobileStockProductInfos PerformGetProductStockInfos(MobileParam param, int productId)
        {
            if (param == null)
                return new MobileStockProductInfos(param, Texts.ProductsNotFoundMessage);

            var stocks = StockManager.GetStocksForInvoiceProduct(param.ActorCompanyId, productId);

            var userDefaultStockId = SettingManager.GetIntSetting(SettingMainType.UserAndCompany, (int)UserSettingType.BillingDefaultStockPlace, param.UserId, param.ActorCompanyId, 0);

            return new MobileStockProductInfos(param, stocks, userDefaultStockId);
        }

        private MobileValueResult PerformGetProductPrice(MobileParam param, int invoiceId, int productId, decimal quantity)
        {
            try
            {
                decimal price = ProductManager.GetProductPriceForCustomerInvoice(productId, invoiceId, param.ActorCompanyId, quantity);

                return new MobileValueResult(param, price);
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformGetProductPrice: Error= {0} ", e.Message));
                return new MobileValueResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion

        #region Order

        #region Field settings

        private MobileOrderEditFieldSettings PerformGetOrderEditFieldSettings(MobileParam param)
        {
            //Get settings
            List<FieldSetting> fieldSettings = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.OrderEdit, param.RoleId, param.ActorCompanyId);

            MobileOrderEditFieldSettings settings = new MobileOrderEditFieldSettings(param, fieldSettings);
            return settings;
        }

        private MobileOrderGridFieldSettings PerformGetOrderGridFieldSettings(MobileParam param)
        {
            //Get settings
            List<FieldSetting> fieldSettings = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.OrderGrid, param.RoleId, param.ActorCompanyId);

            bool hideStatusOrderReady = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingHideStatusOrderReadyForMobile, param.UserId, param.ActorCompanyId, 0);
            bool showMyOrders = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            MobileOrderGridFieldSettings settings = new MobileOrderGridFieldSettings(param, fieldSettings, hideStatusOrderReady, showMyOrders);
            return settings;
        }
        #endregion
        private MobileOrdersGrid PerformGetOrdersGrid(MobileParam param, bool hideStatusOrderReady, bool showMyOrders, bool hideUserStateReady)
        {
            if (param == null)
                return new MobileOrdersGrid(param, Texts.OrdersNotFoundMessage);

            //Get settings
            List<FieldSetting> fieldSettings = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.OrderGrid, param.RoleId, param.ActorCompanyId);

            //Permission
            SoeOriginStatusClassification classification = SoeOriginStatusClassification.OrdersOpenUser;
            if (!showMyOrders && FeatureManager.HasRolePermission(Feature.Billing_Order_Orders, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                classification = SoeOriginStatusClassification.OrdersOpen;

            //Fetch orders

            var items2 = InvoiceManager.GetCustomerInvoicesForGrid(classification, (int)SoeOriginType.Order, param.ActorCompanyId, param.UserId, true, false, classification == SoeOriginStatusClassification.OrdersOpenUser, false, TermGroup_ChangeStatusGridAllItemsSelection.All, true, skipForeign: true);

            #region Check if orders ready for invoice should be hidden

            if (hideStatusOrderReady || SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingHideStatusOrderReadyForMobile, param.UserId, param.ActorCompanyId, 0))
            {
                int mobileAttestStateReadyForOrder = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusOrderReadyMobile, param.UserId, param.ActorCompanyId, 0);
                items2 = items2.Where(o => o.AttestStates == null || o.AttestStates.Count != 1 || o.AttestStates.First().AttestStateId != mobileAttestStateReadyForOrder).ToList();
            }

            #endregion

            #region Hide orders that the user has set as ready

            if (hideUserStateReady)
            {
                items2 = items2.Where(o => o.MyReadyState != 2).ToList();
            }

            #endregion

            return new MobileOrdersGrid(param, items2, GetText(8308, "Innehåller skattereduktion"), GetText(5726, "Inga rader"), fieldSettings);
        }

        private MobileOrderEdit PerformGetOrderEdit(MobileParam param, int orderId)
        {
            try
            {

                if (param == null || orderId == 0)
                    return new MobileOrderEdit(param, Texts.OrderNotFoundMessage);

                //Get settings
                List<FieldSetting> fieldSettings = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.OrderEdit, param.RoleId, param.ActorCompanyId);
                fieldSettings.AddRange(FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.OrderOrderRow, param.RoleId, param.ActorCompanyId));

                bool showExternalProductInfoLink = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingUseExternalProductInfoLink, 0, param.ActorCompanyId, 0);

                //Fetch Order
                var item = InvoiceManager.GetCustomerInvoice(orderId, true, true, false, false, false, false, false, false, false, false, false, true);
                if (item == null || !item.ActorId.HasValue)
                    return new MobileOrderEdit(param, Texts.OrderNotFoundMessage);

                #region Order info

                var originUser = InvoiceManager.GetOriginUser(item.InvoiceId, param.UserId);
                var userReadyState = originUser == null ? 0 : originUser.ReadyDate.HasValue ? 2 : 1;
                var myOriginUserStatus = originUser == null ? 0 : originUser.Status;

                //Billing adress
                string billingAddress = string.Empty;
                if (item.BillingAddressId > 0)
                {
                    var orderBillingAddress = ContactManager.GetContactAddress(item.BillingAddressId, false, true);
                    if (orderBillingAddress != null)
                    {
                        FormatAddress(orderBillingAddress);
                        billingAddress = orderBillingAddress.Address;
                    }
                }

                //Delivery adress
                string deliveryAddress = string.Empty;
                if (!string.IsNullOrEmpty(item.InvoiceHeadText))
                {
                    deliveryAddress = item.InvoiceHeadText;
                }
                else if (item.DeliveryAddressId > 0)
                {
                    ContactAddress orderDeliveryAddress = ContactManager.GetContactAddress(item.DeliveryAddressId, false, true);
                    if (orderDeliveryAddress != null)
                    {
                        FormatAddress(orderDeliveryAddress);
                        deliveryAddress = orderDeliveryAddress.Address;
                    }
                }

                //PriceListType
                string priceListTypeName = string.Empty;
                if (item.PriceListTypeId.HasValue)
                {
                    PriceListType priceListType = ProductPricelistManager.GetPriceListType(item.PriceListTypeId.Value, param.ActorCompanyId);
                    if (priceListType != null)
                        priceListTypeName = priceListType.Name;
                }

                //PriceListType
                string wholeSellerName = string.Empty;
                if (item.SysWholeSellerId.HasValue)
                {
                    wholeSellerName = WholeSellerManager.GetWholesellerName(item.SysWholeSellerId.Value);
                }

                //Currency
                string currencyName = string.Empty;
                Dictionary<int, string> currencyDict = CountryCurrencyManager.GetCompCurrenciesDict(param.ActorCompanyId, false);
                if (currencyDict.ContainsKey(item.CurrencyId))
                    currencyName = currencyDict[item.CurrencyId];

                string orderTypeName = GetText(item.OrderType, (int)TermGroup.OrderType);

                // Customer note
                var customerId = item.ActorId ?? 0;
                var showCustomerNote = false;
                if (customerId > 0)
                {
                    var fieldSettingsCustomerEdit = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.CustomerEdit, param.RoleId, param.ActorCompanyId);
                    var customer = CustomerManager.GetCustomer(customerId);

                    showCustomerNote = (customer?.ShowNote ?? false) && !string.IsNullOrEmpty(customer.Note) && FieldSettingManager.DoShowMobileField(fieldSettingsCustomerEdit, TermGroup_MobileFields.CustomerEdit_Note, true, false);
                }

                #endregion

                #region Permissions

                bool timeReportPermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_Edit, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool projectPermission = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_Project, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool autoGenerateProject = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectAutoGenerateOnNewInvoice, 0, param.ActorCompanyId, 0);

                bool showProducts = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_ProductRows, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool showTP = timeReportPermission && item.ProjectId > 0;
                bool showChecklists = FeatureManager.HasRolePermission(Feature.Billing_Order_Checklists, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool answerChecklists = FeatureManager.HasRolePermission(Feature.Billing_Order_Checklists_AnswerChecklists, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool addChecklists = FeatureManager.HasRolePermission(Feature.Billing_Order_Checklists_AddChecklists, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool showImages = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_Images, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool addProject = projectPermission && !autoGenerateProject && !item.ProjectId.HasValue;
                bool changeProject = projectPermission && item.ProjectId.HasValue && item.Origin.Status == (int)SoeOriginStatus.Origin;
                bool showPartDelivery = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingUsePartialInvoicingOnOrderRow, 0, param.ActorCompanyId, 0);
                bool showAccounts = false; //set to false before release
                bool copyRows = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_ProductRows_Copy, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool moveRows = copyRows;
                bool showSalesPrice = FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowSalesPrice, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                bool showOrderPlanning = FeatureManager.HasRolePermission(Feature.Billing_Order_Planning, Permission.Modify, param.RoleId, param.ActorCompanyId) || FeatureManager.HasRolePermission(Feature.Billing_Order_PlanningUser, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool showInvoicedTime = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                bool showStockPlace = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_ProductRows_Stock, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                bool onlyChangeRowStateIfOwner = FeatureManager.HasRolePermission(Feature.Billing_Order_Only_ChangeRowState_IfOwner, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool allowMarkReady = !onlyChangeRowStateIfOwner || (onlyChangeRowStateIfOwner && originUser != null && originUser.Main);
                bool showExpense = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_Expenses, Permission.Modify, param.RoleId, param.ActorCompanyId);
                bool showHouseHoldDeduction = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, param.ActorCompanyId, 0) > 0 || SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductRUTTaxDeduction, 0, param.ActorCompanyId, 0) > 0;
                bool showChangeCustomer = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_Customer, Permission.Modify, param.RoleId, param.ActorCompanyId) && item.Status != (int)SoeOriginStatus.OrderFullyInvoice && item.Status != (int)SoeOriginStatus.OrderPartlyInvoice && item.Status != (int)SoeOriginStatus.OrderClosed;
                bool deleteOrder = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit, Permission.Modify, param.RoleId, param.ActorCompanyId) && FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_Delete, Permission.Modify, param.RoleId, param.ActorCompanyId) && item.Status != (int)SoeOriginStatus.Cancel && item.Status != (int)SoeOriginStatus.OrderPartlyInvoice && item.Status != (int)SoeOriginStatus.OrderFullyInvoice;

                if (showExpense)
                {
                    InvoiceManager.SetPriceListTypeInclusiveVat(item, base.ActorCompanyId);
                }

                #endregion

                #region Order overview

                int workedTime = 0;
                int otherTime = 0;
                int invoicedTime = 0;
                int productsCount = 0;

                if (showTP)
                {
                    bool workedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_WorkedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                    bool invoicedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);

                    if (item.ProjectId.HasValue && (workedTimePermission || invoicedTimePermission))
                    {
                        var totals = ProjectManager.GetProjectTotals(param.ActorCompanyId, item.ProjectId.Value, item.InvoiceId, (int)SoeProjectRecordType.Order);

                        workedTime = totals.WorkTime;
                        invoicedTime = totals.InvoiceTime;
                        otherTime = totals.OtherTime;

                        if (!workedTimePermission)
                        {
                            workedTime = 0;
                            otherTime = 0;
                        }

                        if (!invoicedTimePermission)
                            invoicedTime = 0;
                    }
                }

                if (showProducts)
                {
                    productsCount = InvoiceManager.GetNrOfCustomerInvoiceRowsByInvoice(orderId);
                }

                bool isClosed = item.Status == (int)SoeOriginStatus.OrderFullyInvoice || item.Status == (int)SoeOriginStatus.OrderClosed;

                #endregion

                #region Dynamic order head info

                string dynamicData = string.Empty;

                //Get settings
                List<FieldSetting> fieldSettingsOrderHead = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.OrderHead, param.RoleId, param.ActorCompanyId);

                if (FieldSettingManager.DoShowMobileField(fieldSettingsOrderHead, TermGroup_MobileFields.OrderHead_OrderNr, true, false))
                {
                    dynamicData += GetText((int)TermGroup_MobileFields.OrderHead_OrderNr, (int)TermGroup.MobileFields) + ": " + item.InvoiceNr;
                    dynamicData += "\n";
                }

                if (FieldSettingManager.DoShowMobileField(fieldSettingsOrderHead, TermGroup_MobileFields.OrderHead_Customer, true, false))
                {
                    dynamicData += GetText((int)TermGroup_MobileFields.OrderHead_Customer, (int)TermGroup.MobileFields) + ": " + item.ActorNr + ", " + item.ActorName;
                    dynamicData += "\n";
                }

                if (FieldSettingManager.DoShowMobileField(fieldSettingsOrderHead, TermGroup_MobileFields.OrderHead_ProjectNr, true, false))
                {
                    dynamicData += GetText((int)TermGroup_MobileFields.OrderHead_ProjectNr, (int)TermGroup.MobileFields) + ": " + (item.Project != null ? item.Project.Number : "");
                    dynamicData += "\n";
                }

                if (FieldSettingManager.DoShowMobileField(fieldSettingsOrderHead, TermGroup_MobileFields.OrderHead_VatType, true, false))
                {
                    dynamicData += GetText((int)TermGroup_MobileFields.OrderHead_VatType, (int)TermGroup.MobileFields) + ": " + GetText(item.VatType, (int)TermGroup.InvoiceVatType);
                    dynamicData += "\n";
                }

                if (FieldSettingManager.DoShowMobileField(fieldSettingsOrderHead, TermGroup_MobileFields.OrderHead_YourReference, true, false))
                {
                    dynamicData += GetText((int)TermGroup_MobileFields.OrderHead_YourReference, (int)TermGroup.MobileFields) + ": " + (item.ReferenceYour != null ? item.ReferenceYour : "");
                    dynamicData += "\n";
                }

                if (FieldSettingManager.DoShowMobileField(fieldSettingsOrderHead, TermGroup_MobileFields.OrderHead_DeliveryAddress, true, false))
                {
                    dynamicData += GetText((int)TermGroup_MobileFields.OrderHead_DeliveryAddress, (int)TermGroup.MobileFields) + ": " + deliveryAddress;
                    dynamicData += "\n";
                }

                if (FieldSettingManager.DoShowMobileField(fieldSettingsOrderHead, TermGroup_MobileFields.OrderHead_BillingAddress, true, false))
                {
                    dynamicData += GetText((int)TermGroup_MobileFields.OrderHead_BillingAddress, (int)TermGroup.MobileFields) + ": " + billingAddress;
                    dynamicData += "\n";
                }

                if (FieldSettingManager.DoShowMobileField(fieldSettingsOrderHead, TermGroup_MobileFields.OrderHead_OrderDescription, true, false))
                {
                    dynamicData += GetText((int)TermGroup_MobileFields.OrderHead_OrderDescription, (int)TermGroup.MobileFields) + ": " + item.Origin.Description;
                    dynamicData += "\n";
                }

                if (FieldSettingManager.DoShowMobileField(fieldSettingsOrderHead, TermGroup_MobileFields.OrderHead_InvoiceLabel, true, false))
                {
                    dynamicData += GetText((int)TermGroup_MobileFields.OrderHead_InvoiceLabel, (int)TermGroup.MobileFields) + ": " + (item.InvoiceLabel ?? "");
                    dynamicData += "\n";
                }

                #endregion

                return new MobileOrderEdit(param, item, workedTime, otherTime, invoicedTime, productsCount, billingAddress, deliveryAddress, currencyName, GetText(item.VatType, (int)TermGroup.InvoiceVatType), priceListTypeName, wholeSellerName, orderTypeName, GetText(8308, "Innehåller skattereduktion"), showTP, showProducts, showChecklists, showImages, answerChecklists, addChecklists, addProject, changeProject, showPartDelivery, showAccounts, fieldSettings, dynamicData, copyRows, moveRows, false, showSalesPrice, showOrderPlanning, showInvoicedTime, showStockPlace, allowMarkReady, userReadyState, showExpense, showHouseHoldDeduction, showChangeCustomer, deleteOrder, isClosed, showExternalProductInfoLink, myOriginUserStatus, showCustomerNote);
            }
            catch (Exception e)
            {
                LogError("PerformGetorderedit failed: " + e.Message);

                if (e.InnerException != null)
                    LogError("PerformGetorderedit failed, innerexception: " + e.InnerException.Message);

                return new MobileOrderEdit(param, "Error");
            }
        }

        private MobileOrderTemplateInfo PerformGetOrderTemplateInfo(MobileParam param, int templateId)
        {
            try
            {

                if (param == null || templateId == 0)
                    return new MobileOrderTemplateInfo(param, Texts.OrderNotFoundMessage);

                var item = InvoiceManager.GetCustomerInvoice(templateId);
                if (item == null || !item.ActorId.HasValue)
                    return new MobileOrderTemplateInfo(param, Texts.OrderNotFoundMessage);

                string orderTypeName = GetText(item.OrderType, (int)TermGroup.OrderType);

                return new MobileOrderTemplateInfo(param, item, orderTypeName: orderTypeName);
            }
            catch (Exception e)
            {
                LogError("PerformGetOrderTemplateInfo failed: " + e.Message);

                if (e.InnerException != null)
                    LogError("PerformGetOrderTemplateInfo failed, innerexception: " + e.InnerException.Message);

                return new MobileOrderTemplateInfo(param, "Error");
            }
        }

        private MobileCustomerCreditLimit PerformGetCustomerCreditLimit(MobileParam param, int customerId)
        {
            try
            {
                var customer = CustomerManager.GetCustomer(customerId);
                if (customer == null)
                    return new MobileCustomerCreditLimit(param, Texts.CustomerNotFoundMessage);

                var limit = CustomerManager.CheckCustomerCreditLimit(param.ActorCompanyId, customerId);
                if(customer.CreditLimit != null && customer.CreditLimit > 0 && limit != null)
                {
                    return new MobileCustomerCreditLimit(param, true, (decimal)customer.CreditLimit, (decimal)limit);
                }

                return new MobileCustomerCreditLimit(param, false, 0, 0);
            }
            catch (Exception e)
            {
                LogError("PerformGetCustomerCreditLimit failed: " + e.Message);

                if (e.InnerException != null)
                    LogError("PerformGetCustomerCreditLimit failed, innerexception: " + e.InnerException.Message);

                return new MobileCustomerCreditLimit(param, GetText(8778, "Ett fel inträffade"));
            }
        }

        private MobileOrderEdit PerformSaveOrder(MobileParam param, int orderId, int customerId, string descriptionText, string label, string headText, string ourReference, TermGroup_InvoiceVatType vatType, int priceListId, int currencyId, int deliveryAddressId, int billingAddressId, int wholeSellerId, string yourReference, int projectId, int templateId, string deliveryDateString, int orderTypeId, string workDescription, bool discardConcurrencyCheck = false)
        {

            if (descriptionText == "(null)")
                descriptionText = string.Empty;

            //Save MobileOrder
            DateTime? deliveryDate = CalendarUtility.GetNullableDateTime(deliveryDateString);

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_26) ||
                (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_44) && orderId <= 0))
            {
                orderTypeId = -1;
            }

            CustomerInvoice templateInvoice = null;
            if (templateId != 0)
            {
                templateInvoice = InvoiceManager.GetCustomerInvoice(templateId, true, true, true, true, true, true, true, true, true, true, true, true);
                if (templateInvoice != null && templateInvoice.ProjectId.HasValue)
                {
                    projectId = (int)templateInvoice.ProjectId;
                }
            }
            

            var result = InvoiceManager.SaveMobileOrder(param.UserId, param.ActorCompanyId, param.RoleId, orderId, customerId, descriptionText, label, headText, ourReference, vatType, priceListId, currencyId, deliveryAddressId, billingAddressId, wholeSellerId, yourReference, projectId, deliveryDate, orderTypeId, workDescription, discardConcurrencyCheck, templateId);
            var mobileOrder = new MobileOrderEdit(param, result.IntegerValue);

            if (templateInvoice != null && result.Success)
            {
                //Invoice Manager - Orderid, templateid, actorcompany... metod som hämtar allt
                InvoiceManager.MergeMobileOrderWithTemplate(param.ActorCompanyId, result.IntegerValue, templateInvoice);
            }

            //Set result
            if (result.Success && result.IntegerValue > 0) //we have saved an order...
                mobileOrder.SetTaskResult(MobileTask.SaveOrder, true);
            else if (result.ErrorNumber == (int)ActionResultSave.MobileOrderIllegalSave)
                mobileOrder = new MobileOrderEdit(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
            else if (result.ErrorNumber == (int)ActionResultSave.MobileOrderCustomerBlocked)
                mobileOrder = new MobileOrderEdit(param, result.ErrorMessage);
            else
                mobileOrder = new MobileOrderEdit(param, FormatMessage(Texts.OrderNotSavedMessage, result.ErrorNumber) + ": " + result.ErrorMessage);

            return mobileOrder;
        }

        private MobileResult PerformSetOrderUserIsReady(MobileParam param, int orderId)
        {
            var saveResult = InvoiceManager.UpdateOrderReadyState(orderId, param.UserId, false);

            var mobileResult = new MobileResult(param);
            if (saveResult.Success)
                mobileResult.SetTaskResult(MobileTask.SetOrderIsReady, saveResult.BooleanValue); //sends back current state
            else
                return new MobileResult(param, Texts.SaveFailed + ": " + saveResult.ErrorMessage);

            return mobileResult;
        }

        private MobileOrderEdit PerformSetOrderReady(MobileParam param, int orderId)
        {
            MobileOrderEdit mobileOrder = new MobileOrderEdit(param, orderId);

            //Get Order
            var item = InvoiceManager.GetCustomerInvoice(orderId);
            if (item == null)
                return new MobileOrderEdit(param, Texts.OrderNotFoundMessage);

            bool mandatoryChecklist = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingMandatoryChecklist, 0, param.ActorCompanyId, 0);

            if (mandatoryChecklist)
            {
                List<ChecklistHeadRecord> checklistHeadRecords = ChecklistManager.GetChecklistHeadRecords(SoeEntityType.Order, orderId, param.ActorCompanyId, true);

                foreach (var checklistHeadRecord in checklistHeadRecords)
                {
                    var checklistRows = ChecklistManager.GetChecklistRows(checklistHeadRecord.ChecklistHeadId, SoeEntityType.Order, orderId, param.ActorCompanyId, checklistHeadRecord.ChecklistHeadRecordId);
                    bool isValid = GetNbrOfInvalidAnswers(checklistRows) == 0;

                    if (!isValid)
                        return new MobileOrderEdit(param, GetText(8453, "En eller flera checklistor innehåller obligatoriska frågor som inte är ifyllda."));
                }
            }

            //Set Order ready
            ActionResult result = InvoiceManager.SetOrderReady(orderId, param.ActorCompanyId, param.UserId);

            //Set result
            if (result.Success)
                mobileOrder.SetTaskResult(MobileTask.SetOrderReady, true);
            else
                mobileOrder = new MobileOrderEdit(param, FormatMessage(Texts.OrderNotSetToReadyMessage, result.ErrorNumber));

            return mobileOrder;
        }

        private MobileOrderEdit PerformSetOrderRowsAsReady(MobileParam param, int orderId, string ids)
        {
            MobileOrderEdit mobileOrder = new MobileOrderEdit(param, orderId);

            //Get Order
            var item = InvoiceManager.GetCustomerInvoice(orderId);
            if (item == null)
                return new MobileOrderEdit(param, Texts.OrderNotFoundMessage);

            List<int> orderRowIds = new List<int>();

            #region Parse ids
            char[] separator = new char[1];
            separator[0] = ',';

            string[] separatedIds = ids.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var separatedId in separatedIds)
            {
                if (Int32.TryParse(separatedId.Trim(), out int id))
                    orderRowIds.Add(id);
            }

            #endregion

            //Set Orderrows as ready
            ActionResult result = InvoiceManager.SetOrderRowsAsReady(orderId, param.ActorCompanyId, param.UserId, orderRowIds);

            //Set result
            if (result.Success)
                mobileOrder.SetTaskResult(MobileTask.SetOrderRowsAsReady, true);
            else
                mobileOrder = new MobileOrderEdit(param, FormatMessage(Texts.OrderNotSetToReadyMessage, result.ErrorNumber));

            return mobileOrder;
        }

        private MobileResult PerformSetOrdersAsRead(MobileParam param, string orderIds)
        {
            try
            {
                var ids = GetIds(orderIds);
                var saveResult = InvoiceManager.SetOrderMyOriginStatus(ids, param.UserId, OriginUserStatus.Read);

                var mobileResult = new MobileResult(param);
                if (saveResult.Success)
                    mobileResult.SetTaskResult(MobileTask.Update, true);
                else
                    return new MobileResult(param, Texts.SaveFailed + ": " + saveResult.ErrorMessage);

                return mobileResult;

            }
            catch (Exception ex)
            {
                LogError("Mobilemanager: PerformSetOrdersAsRead failed: " + ex.Message);

                return new MobileResult(param, Texts.SaveFailed);
            }
        }

        private MobileResult PerformSetOrdersAsUnRead(MobileParam param, string orderIds)
        {
            try
            {
                var ids = GetIds(orderIds);
                var saveResult = InvoiceManager.SetOrderMyOriginStatus(ids, param.UserId, OriginUserStatus.UnRead);

                var mobileResult = new MobileResult(param);
                if (saveResult.Success)
                    mobileResult.SetTaskResult(MobileTask.Update, true);
                else
                    return new MobileResult(param, Texts.SaveFailed + ": " + saveResult.ErrorMessage);

                return mobileResult;

            }
            catch (Exception ex)
            {
                LogError("Mobilemanager: PerformSetOrdersAsUnRead failed: " + ex.Message);

                return new MobileResult(param, Texts.SaveFailed);
            }
        }

        private MobileOrderRow PerformCopyMoveOrderRows(MobileParam param, int orderId, int newOrderId, string orderRowIdsAndQuantity, int type)
        {
            //Type: 1 = copy, 2 = move            

            try
            {
                List<CustomerInvoiceRowDTO> rows = new List<CustomerInvoiceRowDTO>();

                CustomerInvoice order = InvoiceManager.GetCustomerInvoice(orderId, loadInvoiceRow: true);
                if (order == null)
                    return new MobileOrderRow(param, orderId, Texts.OrderNotFoundMessage);

                CustomerInvoice newOrder = InvoiceManager.GetCustomerInvoice(newOrderId);
                if (newOrder == null)
                    return new MobileOrderRow(param, orderId, Texts.OrderNotFoundMessage);

                AttestState initialAttestState = AttestManager.GetInitialAttestState(param.ActorCompanyId, TermGroup_AttestEntity.Order);
                if (initialAttestState == null)
                    return new MobileOrderRow(param, orderId, GetText(8517, "Atteststatus - lägsta nivå saknas"));

                #region Parse orderRowIdsAndQuantity

                Dictionary<int, decimal> copyOrderRows = new Dictionary<int, decimal>();

                string[] rowSeparator = new string[1];
                rowSeparator[0] = "[##]";
                string[] elementSeparator = new string[1];
                elementSeparator[0] = "[#]";

                string[] separatedInputs = orderRowIdsAndQuantity.Split(rowSeparator, StringSplitOptions.RemoveEmptyEntries);

                foreach (string separatedInput in separatedInputs)
                {
                    string[] separatedElements = separatedInput.Trim().Split(elementSeparator, StringSplitOptions.None);
                    if (separatedElements.Count() != 2)
                    {
                        return new MobileOrderRow(param, orderId, "Parse error: elementcount");
                    }

                    string orderRowIdStr = separatedElements[0].Trim();
                    string quantityStr = separatedElements[1].Trim();

                    if (!Int32.TryParse(orderRowIdStr, out int orderRowId))
                        return new MobileOrderRow(param, orderId, "Parse error: elementdatatype");
                    if (!Decimal.TryParse(quantityStr, out decimal quantity))
                        return new MobileOrderRow(param, orderId, "Parse error: elementdatatype");

                    copyOrderRows.Add(orderRowId, quantity);
                }

                #endregion

                MobileOrderRow mobileOrderRow = null;

                foreach (var item in copyOrderRows)
                {
                    var row = order.CustomerInvoiceRow.FirstOrDefault(x => x.CustomerInvoiceRowId == item.Key);
                    if (row == null)
                        return new MobileOrderRow(param, orderId, GetText(8531, "Orderrad kunde inte hittas"));

                    var rowDto = row.ToCustomerInvoiceRowDTO(order, null, false);

                    rowDto.Quantity = item.Value;
                    rows.Add(rowDto);
                }

                //copied from invoiceproductrows.xaml.cs
                rows = GetUnattestedRows(rows, initialAttestState);
                rows = rows.Where(r => !r.IsTimeProjectRow && r.Type != SoeInvoiceRowType.AccountingRow && r.Type != SoeInvoiceRowType.BaseProductRow && r.Type != SoeInvoiceRowType.SubTotalRow && String.IsNullOrEmpty(r.HouseholdProperty)).ToList();

                bool updateOriginalInvoice = false;
                if (type == 2)
                    updateOriginalInvoice = true;

                //Save MobileOrderRow
                ActionResult result = InvoiceManager.CopyCustomerInvoiceRows(rows, param.ActorCompanyId, newOrderId, originalOrderId: orderId, updateOriginalInvoice: updateOriginalInvoice);
                if (result.Success)
                    mobileOrderRow = new MobileOrderRow(param, orderId);

                //Set result
                if (mobileOrderRow != null)
                    mobileOrderRow.SetTaskResult(MobileTask.CopyOrderRow, true);
                else if (string.IsNullOrEmpty(result.ErrorMessage))
                    mobileOrderRow = new MobileOrderRow(param, orderId, FormatMessage(GetText(8518, "Gick inte att kopiera/flytta orderrader"), result.ErrorNumber));
                else
                    mobileOrderRow = new MobileOrderRow(param, orderId, FormatMessage(GetText(8518, "Gick inte att kopiera/flytta orderrader") + " ( " + result.ErrorMessage + " ) ", result.ErrorNumber));

                return mobileOrderRow;

            }
            catch (Exception ex)
            {
                LogError("Mobilemanager: PerformCopyMoveOrderRows failed: " + ex.Message);

                return new MobileOrderRow(param, orderId, GetText(8518, "Gick inte att kopiera/flytta orderrader") + ": " + (string.IsNullOrEmpty(ex.Message) ? "" : ex.Message));
            }
        }

        private MobileCopyMoveOrders PerformGetCopyMoveOrders(MobileParam param)
        {
            if (param == null)
                return new MobileCopyMoveOrders(param, Texts.OrdersNotFoundMessage);

            //Permission
            SoeOriginStatusClassification classification = SoeOriginStatusClassification.OrdersOpenUser;
            if (FeatureManager.HasRolePermission(Feature.Billing_Order_Orders, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                classification = SoeOriginStatusClassification.OrdersOpen;

            var items = InvoiceManager.GetCustomerInvoicesForGrid(classification, (int)SoeOriginType.Order, param.ActorCompanyId, param.UserId, true, false, classification == SoeOriginStatusClassification.OrdersOpenUser, false, TermGroup_ChangeStatusGridAllItemsSelection.All, true, skipForeign: true).ToList();
            items = items.Where(x => !string.IsNullOrEmpty(x.InvoiceNr)).OrderByDescending(i => Convert.ToInt32(i.InvoiceNr)).ToList();

            return new MobileCopyMoveOrders(param, items);
        }

        private MobileResult PerformDeleteOrder(MobileParam param, int orderId, bool deleteProject)
        {
            try
            {
                if (param == null || orderId == 0)
                    return new MobileResult(param, Texts.OrderNotFoundMessage);

                if (!FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_Delete, Permission.Modify, param.RoleId, param.ActorCompanyId))
                    return new MobileResult(param, GetText(8652, "Otillåten ändring, behörighet saknas"));

                var mobileResult = new MobileResult(param);
                var result = InvoiceManager.DeleteInvoice(orderId, param.ActorCompanyId, deleteProject, true);

                if (result.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.Delete, true);
                }
                else
                {
                    if (result.ErrorNumber > 0)
                        mobileResult = new MobileResult(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                    else
                        mobileResult = new MobileResult(param, GetText(7602, "Ordern kunde inte tas bort"));
                }

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformDeleteOrder: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion

        #region Templates

        private MobileDicts PerformGetOrderInvoiceTemplates(MobileParam param, int actorCompanyId)
        {
            Dictionary<int, string> result = InvoiceManager.GetInvoiceTemplatesDict(actorCompanyId, SoeOriginType.Order, SoeInvoiceType.CustomerInvoice);

            return new MobileDicts(param, result);
        }

        #endregion

        #region OrderRow

        private MobileOrderRows PerformGetOrderRows(MobileParam param, int orderId, bool showAll)
        {
            if (param == null)
                return new MobileOrderRows(param, orderId, Texts.OrderRowsNotFoundMessage);

            //Get Orderrows
            List<CustomerInvoiceRow> customerInvoiceRows = InvoiceManager.GetCustomerInvoiceRows(orderId, true, true).OrderBy(x => x.RowNr).ToList();
            customerInvoiceRows = customerInvoiceRows.Where(i => i.Type == (int)SoeInvoiceRowType.ProductRow || i.Type == (int)SoeInvoiceRowType.TextRow).ToList();

            List<ProductUnit> productUnits = ProductManager.GetProductUnits(param.ActorCompanyId).ToList();

            int productMiscId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductMisc, 0, param.ActorCompanyId, 0);
            int attestStateTransferredOrderToInvoiceId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, param.ActorCompanyId, 0);
            bool warningOnReducedQuantity = FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_ProductRows_QuantityWarning, Permission.Modify, param.RoleId, param.ActorCompanyId);
            bool showPartDelivery = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingUsePartialInvoicingOnOrderRow, 0, param.ActorCompanyId, 0);

            List<AttestState> attestStates = AttestManager.GetAttestStates(param.ActorCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing);

            //Get AttestState initial
            AttestState attestStateInitial = AttestManager.GetInitialAttestState(param.ActorCompanyId, TermGroup_AttestEntity.Order);
            if (attestStateInitial != null && !showAll)
                customerInvoiceRows = customerInvoiceRows.Where(i => i.AttestStateId == attestStateInitial.AttestStateId).ToList();

            return new MobileOrderRows(param, orderId, customerInvoiceRows, productUnits, attestStates, attestStateInitial, attestStateTransferredOrderToInvoiceId, showPartDelivery, warningOnReducedQuantity, productMiscId);
        }
     
        private MobileResult PerformGetOrderRowExternalUrl(MobileParam param, int productId)
        {
            try
            {
                var externalUrl = ProductManager.GetProductExternalUrls(param.ActorCompanyId, new List<int>() { productId }).FirstOrDefault();
                if (string.IsNullOrEmpty(externalUrl))
                {
                    return new MobileResult(param, GetText(4207, "Artikeln saknar extern produktinformation"));
                }


                var mobileResult = new MobileResult(param);
                mobileResult.SetMessage(externalUrl);
                
                return mobileResult;

            }
            catch (Exception e)
            {
                LogError("PerformGetOrderRowExternalUrl: " + e.Message);
                return new MobileResult(param, GetText(8778, "Ett fel inträffade"));
            }
        }

        private MobileOrderRow PerformSaveOrderRow(MobileParam param, int orderId, int orderRowId, int productId, int sysProductId, int sysWholeSellerId, decimal quantity, decimal amount, string text, decimal quantityToInvoice, int stockId)
        {
            //Get MobileOrder
            CustomerInvoice order = InvoiceManager.GetCustomerInvoice(orderId, false, true, false, false, false, false, false, false, false, false, false, true);
            if (order == null || !order.PriceListTypeId.HasValue || !order.ActorId.HasValue)
                return new MobileOrderRow(param, orderId, Texts.OrderNotFoundMessage);

            if (order.ProjectId.HasValue && order.Project != null && ((order.Project.Status == (int)TermGroup_ProjectStatus.Locked) || (order.Project.Status == (int)TermGroup_ProjectStatus.Finished)))
            {
                string msg = GetProjectLockedOrFinishedMessage(order.Project);
                return new MobileOrderRow(param, orderId, msg);
            }

            MobileOrderRow mobileOrderRow = null;

            if (sysProductId > 0 && sysWholeSellerId > 0)
            {

                InvoiceProductPriceSearchViewDTO productPrice = null;
                try
                {
                    productPrice = ProductManager.SearchInvoiceProductPrice(param.ActorCompanyId, order.PriceListTypeId.Value, order.ActorId.Value, order.CurrencyId, sysProductId, sysWholeSellerId, true);
                }
                catch (Exception ex)
                {
                    LogError("MobileManager.PerformSaveOrderRow.SearchInvoiceProductPrice :" + ex.Message);
                }

                if (productPrice == null)
                    return new MobileOrderRow(param, orderId, GetText(8331, "Artikelrad kunde inte hittas"));

                decimal salesPrice = productPrice.CustomerPrice ?? 0;
                decimal purchasePrice = productPrice.NettoNettoPrice ?? 0;

                var wholeSeller = WholeSellerManager.GetSysWholesellerDTO(sysWholeSellerId);
                if (wholeSeller == null)
                    return new MobileOrderRow(param, orderId, "Wholeseller not found");

                PriceListOrigin dbOrigin = productPrice.PriceListOrigin > 0 ? (PriceListOrigin)productPrice.PriceListOrigin : wholeSeller.IsOnlyInComp ? PriceListOrigin.CompDbPriceList : PriceListOrigin.SysDbPriceList;
                InvoiceProduct product = ProductManager.CopyExternalInvoiceProduct(sysProductId, purchasePrice, salesPrice, productPrice.PurchaseUnit, order.PriceListTypeId.Value, productPrice.SysPriceListHeadId, productPrice.Wholeseller, order.ActorId.Value, param.ActorCompanyId, true, dbOrigin);

                if (product != null)
                    productId = product.ProductId;
            }

            InvoiceProduct invoiceProduct = ProductManager.GetInvoiceProduct(productId);
            if (invoiceProduct != null && invoiceProduct.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift)
            {
                return new MobileOrderRow(param, orderId, GetText(7642, "Lyftrader stöds inte i SoftOne GO mobilapp. Använd SoftOne GO webb för hantering av lyftartiklar."));
            }

            // Check lift
            /*if (invoiceProduct.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift || invoiceProduct.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Clearing)
            {
                if (quantity < 0)
                    quantity = quantity * -1;

                if ((order.Origin.Type == (int)SoeOriginType.CustomerInvoice && amountCurrency < 0) || amountCurrency > 0)
                    amountCurrency = amountCurrency * -1;
            }*/

            //Save MobileOrderRow
            var extraTextRow = invoiceProduct == null || !invoiceProduct.ShowDescriptionAsTextRow ? "" : invoiceProduct.Description ?? "";

            CustomerInvoiceRow invoiceRow = InvoiceManager.GetCustomerInvoiceRow(orderRowId);
            if (invoiceRow != null && invoiceRow.IsTimeProjectRow)
            {
                // Only amount is allowed to be changed
                productId = invoiceRow.ProductId ?? 0;
                quantity = invoiceRow.Quantity ?? 0;
                text = invoiceRow.Text;
                quantityToInvoice = invoiceRow.InvoiceQuantity ?? 0;
                stockId = invoiceRow.StockId ?? 0;
            }

            ActionResult result = InvoiceManager.SaveOrderRow(param.ActorCompanyId, orderId, orderRowId, productId, param.RoleId, quantity, amount, text, quantityToInvoice, stockId, extraTextRow);
            Stock stock = null;
            if (stockId != 0)
            {
                stock = StockManager.GetStock(stockId);
            }
            if (result.Success && result.Value != null)
                mobileOrderRow = new MobileOrderRow(param, orderId, result.Value as CustomerInvoiceRow, new List<ProductUnit>(), false, false, "", "", false, false, false, false, 0, stock);

            //Set result
            if (mobileOrderRow != null)
                mobileOrderRow.SetTaskResult(MobileTask.SaveOrderRow, true);
            else if (string.IsNullOrEmpty(result.ErrorMessage))
                mobileOrderRow = new MobileOrderRow(param, orderId, FormatMessage(Texts.OrderRowNotSavedMessage, result.ErrorNumber));
            else
                mobileOrderRow = new MobileOrderRow(param, orderId, FormatMessage(Texts.OrderRowNotSavedMessage + " ( " + result.ErrorMessage + " ) ", result.ErrorNumber));

            return mobileOrderRow;
        }

        private MobileHouseholdDeductionRow PerformSaveHouseholdDeduction(MobileParam param, int orderId, int orderRowId, string propertyLabel, string socialSecNbr, string name, decimal amount, string apartmentNbr, string cooperativeOrgNbr, bool isHDRut, int mobileDeductionType)
        {
            #region Validation

            #region Only ROT

            if (!isHDRut && (mobileDeductionType != (int)TermGroup_MobileHouseHoldTaxDeductionType.RUT))
            {
                if (string.IsNullOrEmpty(apartmentNbr) && string.IsNullOrEmpty(propertyLabel))
                    return new MobileHouseholdDeductionRow(param, orderId, GetText(7402, "Du måste ange fastighetsbeteckning eller lägenhetsnummer"));

                if (!string.IsNullOrEmpty(cooperativeOrgNbr) && !CalendarUtility.IsValidSocialSecurityNumber(cooperativeOrgNbr, false, false, true))
                    return new MobileHouseholdDeductionRow(param, orderId, GetText(9073, "Felaktigt organisationsnummer"));
            }

            #endregion

            if (string.IsNullOrEmpty(name))
                return new MobileHouseholdDeductionRow(param, orderId, GetText(9070, "Du måste ange namn"));

            if (string.IsNullOrEmpty(socialSecNbr))
                return new MobileHouseholdDeductionRow(param, orderId, GetText(9071, "Du måste ange personnummer"));

            if (!CalendarUtility.IsValidSocialSecurityNumber(socialSecNbr, true, true, true))
                return new MobileHouseholdDeductionRow(param, orderId, GetText(9072, "Felaktigt personnummer"));

            #endregion

            //Get MobileOrder
            var order = InvoiceManager.GetCustomerInvoiceSmall(orderId);
            if (order == null || !order.PriceListTypeId.HasValue || !order.ActorId.HasValue)
                return new MobileHouseholdDeductionRow(param, orderId, Texts.OrderNotFoundMessage);

            MobileHouseholdDeductionRow mobileOrderRow = null;

            //Save household
            var result = HouseholdTaxDeductionManager.SaveHouseholdDeductionRow(param.ActorCompanyId, param.RoleId, orderId, orderRowId, propertyLabel, socialSecNbr, name, amount, apartmentNbr, cooperativeOrgNbr, isHDRut, mobileDeductionType);
            if (result.Success)
                mobileOrderRow = new MobileHouseholdDeductionRow(param, orderId);

            //Set result
            if (mobileOrderRow != null)
                mobileOrderRow.SetTaskResult(MobileTask.SaveHouseholdDeductionRow, true);
            else if (string.IsNullOrEmpty(result.ErrorMessage))
                mobileOrderRow = new MobileHouseholdDeductionRow(param, orderId, FormatMessage(Texts.OrderRowNotSavedMessage, result.ErrorNumber));
            else
                mobileOrderRow = new MobileHouseholdDeductionRow(param, orderId, FormatMessage(Texts.OrderRowNotSavedMessage + " ( " + result.ErrorMessage + " ) ", result.ErrorNumber));

            return mobileOrderRow;
        }

        /// <summary>
        /// This method is called when the user clicks on a invoicerow that is either rot or rut or chooses in the menu rot or rut
        /// </summary>
        /// <param name="param"></param>
        /// <param name="orderId"></param>
        /// <param name="orderRowId"></param>
        /// <returns></returns>
        private MobileHouseholdDeductionRow PerformGetHouseholdDeduction(MobileParam param, int orderId, int orderRowId, bool isRUT, int mobileDeductionType)
        {
            MobileHouseholdDeductionRow mobileOrderRow = null;

            if (orderRowId != 0)
            {
                var deductionRow = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRow(orderRowId);
                mobileOrderRow = new MobileHouseholdDeductionRow(param, orderId, deductionRow, deductionRow == null ? "" : GetText(deductionRow.HouseHoldTaxDeductionType, (int)TermGroup.HouseHoldTaxDeductionType));
            }
            else
            {
                var suggestedhouseholdAmount = InvoiceManager.GetSuggestedHouseholdAmount(param.ActorCompanyId, orderId, isRUT, (TermGroup_MobileHouseHoldTaxDeductionType)mobileDeductionType);
                var houseHoldTaxDeductionType = HouseholdTaxDeductionManager.GetHouseHoldTaxDeductionType(isRUT, (TermGroup_MobileHouseHoldTaxDeductionType)mobileDeductionType);
                if (mobileDeductionType == 0)
                {
                    mobileDeductionType = isRUT ? (int)TermGroup_MobileHouseHoldTaxDeductionType.RUT : (int)TermGroup_MobileHouseHoldTaxDeductionType.ROT;
                }
                mobileOrderRow = new MobileHouseholdDeductionRow(param, orderId, suggestedhouseholdAmount, houseHoldTaxDeductionType, GetText(mobileDeductionType, (int)TermGroup.MobileHouseHoldTaxDeductionType));
            }

            return mobileOrderRow;
        }

        private MobileHouseholdDeductionApplicants PerformGetHouseholdDeductionApplicants(MobileParam param, int orderId)
        {
            CustomerInvoice order = InvoiceManager.GetCustomerInvoice(orderId);
            if (order == null || !order.ActorId.HasValue)
                return new MobileHouseholdDeductionApplicants(param, Texts.OrderNotFoundMessage);

            var applicants = HouseholdTaxDeductionManager.GetHouseholdTaxDeductionRows(param.ActorCompanyId, order.ActorId.Value, true);

            return new MobileHouseholdDeductionApplicants(param, applicants);
        }

        private MobileHouseholdDeductionApplicants PerformGetHouseholdDeductionApplicantsForCustomer(MobileParam param, int customerId)
        {
            Customer customer = CustomerManager.GetCustomer(customerId);
            if (customer == null)
                return new MobileHouseholdDeductionApplicants(param, Texts.CustomerNotFoundMessage);

            if (!customer.HouseholdTaxDeductionApplicant.IsLoaded)
                customer.HouseholdTaxDeductionApplicant.Load();

            List<HouseholdTaxDeductionApplicantDTO> applicants = new List<HouseholdTaxDeductionApplicantDTO>();
            foreach (HouseholdTaxDeductionApplicant app in customer.HouseholdTaxDeductionApplicant.Where(a => a.State == (int)SoeEntityState.Active))
            {
                applicants.Add(new HouseholdTaxDeductionApplicantDTO()
                {
                    HouseholdTaxDeductionApplicantId = app.HouseholdTaxDeductionApplicantId,
                    Property = app.Property,
                    Name = app.Name,
                    CooperativeOrgNr = app.CooperativeOrgNr,
                    ApartmentNr = app.ApartmentNr,
                    SocialSecNr = app.SocialSecNr,
                    Share = app.Share ?? Decimal.Zero,
                    ShowButton = true,
                    IdentifierString = app.Name + ";" + app.SocialSecNr,
                    Hidden = false,
                });
            }

            var mobileHouseholdDeductionApplicants = new MobileHouseholdDeductionApplicants(param, applicants);
            LogPersonalData(mobileHouseholdDeductionApplicants, TermGroup_PersonalDataActionType.Read, "PerformGetHouseholdDeductionApplicantsForCustomer()");

            return mobileHouseholdDeductionApplicants;
        }

        private MobileResult PerformSaveHouseholdDeductionApplicantsForCustomer(MobileParam param, int customerId, int hdApplicantId, string name, string socSecNr, string property, string apartmentNr, string coopOrgNr)
        {
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (!FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit_HouseholdTaxDeductionApplicants, Permission.Modify, param.RoleId, param.ActorCompanyId))
                        return new MobileResult(param, GetText(8652, "Otillåten ändring, behörighet saknas"));

                    Customer customer = CustomerManager.GetCustomer(entities, customerId);
                    if (customer == null)
                        return new MobileResult(param, Texts.CustomerNotFoundMessage);

                    if (string.IsNullOrEmpty(name))
                        return new MobileResult(param, GetText(9070, "Du måste ange namn"));

                    if (string.IsNullOrEmpty(socSecNr))
                        return new MobileResult(param, GetText(9071, "Du måste ange personnummer"));

                    if (!CalendarUtility.IsValidSocialSecurityNumber(socSecNr, true, true, true))
                        return new MobileResult(param, GetText(9072, "Felaktigt personnummer"));

                    ActionResult result = CustomerManager.SaveHouseholdTaxDeductionApplicant(entities, customer, hdApplicantId, name, socSecNr, property, apartmentNr, coopOrgNr);

                    var mobileResult = new MobileResult(param);

                    if (result.Success)
                    {
                        var applicantDTO = new HouseholdTaxDeductionApplicantDTO()
                        {
                            HouseholdTaxDeductionApplicantId = hdApplicantId,
                            Property = property,
                            Name = name,
                            CooperativeOrgNr = coopOrgNr,
                            ApartmentNr = apartmentNr,
                            SocialSecNr = socSecNr,
                            IdentifierString = name + ";" + socSecNr,
                        };

                        LogPersonalData(new MobileHouseholdDeductionApplicant(param, applicantDTO), TermGroup_PersonalDataActionType.Modify, "PerformSaveHouseholdDeductionApplicantsForCustomer()");

                        mobileResult.SetTaskResult(MobileTask.Save, true);
                    }
                    else
                    {
                        return new MobileResult(param, FormatMessage(Texts.CustomerHouseholdApplicantNotSaved, result.ErrorNumber));
                    }

                    return mobileResult;
                }
                catch (Exception)
                {
                    return new MobileResult(param, Texts.CustomerHouseholdApplicantNotSaved);
                }
            }
        }

        private MobileResult PerformDeleteHouseholdDeductionApplicantForCustomer(MobileParam param, int hdApplicantId)
        {
            if (!FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit_HouseholdTaxDeductionApplicants, Permission.Modify, param.RoleId, param.ActorCompanyId))
                return new MobileResult(param, GetText(8652, "Otillåten ändring, behörighet saknas"));

            ActionResult result = CustomerManager.DeleteHouseholdTaxDeductionApplicant(hdApplicantId);

            var mobileResult = new MobileResult(param);

            if (result.Success)
                mobileResult.SetTaskResult(MobileTask.Delete, true);
            else
                return new MobileResult(param, FormatMessage(Texts.CustomerHouseholdApplicantNotDeleted, result.ErrorNumber));


            return mobileResult;
        }


        private MobileHouseHoldDeductionTypes PerformGetMobileHouseHoldDeductionTypes(MobileParam param, bool incRot, bool incRut, bool incGreenTech)
        {
            var hm = new HouseholdTaxDeductionManager(this.parameterObject);

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_41))
            {
                incRot = false;
                incRut = false;
                incGreenTech = true;
            }

            return new MobileHouseHoldDeductionTypes(param, hm.GetMobileHouseHoldDeductionTypes(incRot, incRut, incGreenTech));
        }

        private MobileOrderRow PerformDeleteOrderRow(MobileParam param, int orderId, int orderRowId)
        {
            //Get Order
            var order = InvoiceManager.GetCustomerInvoice(orderId);
            if (order == null)
                return new MobileOrderRow(param, orderId, Texts.OrderNotFoundMessage);

            CustomerInvoiceRow invoiceRow = InvoiceManager.GetCustomerInvoiceRow(orderRowId);
            if (invoiceRow != null && invoiceRow.IsTimeProjectRow)
            {
                //LogInfo("User trying to delete timeprojectrow");
                return new MobileOrderRow(param, orderId, GetText(8546, "Ej tillåtet att ta bort artikelrad skapat från tidboken. Ta bort tidraden i tidboken istället."));
            }

            // Check permission
            if (FeatureManager.HasRolePermission(Feature.Billing_Order_Orders_Edit_ProductRows_NoDeletion, Permission.Modify, param.RoleId, param.ActorCompanyId))
                return new MobileOrderRow(param, orderId, Texts.OrderRowNotAllowedToDeleteMessage);

            MobileOrderRow mobileOrderRow = null;

            //Delete OrderRow
            ActionResult result = InvoiceManager.DeleteCustomerInvoiceRow(param.ActorCompanyId, orderId, orderRowId);
            if (result.Success)
                mobileOrderRow = new MobileOrderRow(param, orderId);

            //Set result
            if (mobileOrderRow != null)
                mobileOrderRow.SetTaskResult(MobileTask.DeleteOrderRow, true);
            else
                mobileOrderRow = new MobileOrderRow(param, orderId, FormatMessage(Texts.OrderRowNotDeletedMessage, result.ErrorNumber));

            return mobileOrderRow;
        }

        #endregion

        #region WorkingDescription
        private MobileWorkDescription PerformGetOrderWorkDescription(MobileParam param, int orderId)
        {
            var invoice = InvoiceManager.GetCustomerInvoice(orderId);
            if (invoice == null)
                return new MobileWorkDescription(param, Texts.OrderNotFoundMessage);

            string workDesc = string.IsNullOrEmpty(invoice.WorkingDescription) ? string.Empty : invoice.WorkingDescription;

            MobileWorkDescription mobileWorkDescription = new MobileWorkDescription(param, orderId, workDesc);
            return mobileWorkDescription;
        }

        private MobileWorkDescription PerformSaveOrderWorkDescription(MobileParam param, int orderId, string workDesc)
        {
            MobileWorkDescription mobileWorkDescription = new MobileWorkDescription(param);

            ActionResult result = InvoiceManager.SaveCustomerInvoiceWorkingDescription(orderId, workDesc);

            if (result.Success)
                mobileWorkDescription.SetTaskResult(MobileTask.SaveWorkingDescription, true);
            else
            {
                string errorMsg = Texts.WorkingDescriptionNotSavedMessage + ": " + "\n";

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    errorMsg += result.ErrorMessage;

                mobileWorkDescription = new MobileWorkDescription(param, FormatMessage(errorMsg, result.ErrorNumber));
            }

            return mobileWorkDescription;
        }

        #endregion

        #region Your reference (on order)

        private MobileContactPersons PerformGetCustomerReferences(MobileParam param, int customerId)
        {
            var customer = CustomerManager.GetCustomer(customerId);
            Dictionary<int, string> references = ContactManager.GetCustomerReferencesDict(customerId, false);

            if (customer != null && !String.IsNullOrEmpty(customer.InvoiceReference) && !references.Any(r => r.Key == 0))
                references.Add(0, customer.InvoiceReference);

            MobileContactPersons customerReferences = new MobileContactPersons(param, references);
            return customerReferences;
        }

        #endregion

        #region TimeRow

        private MobileTimeRows PerformGetTimeRows(MobileParam param, int orderId)
        {
            if (param == null)
                return new MobileTimeRows(param, orderId, Texts.TimeRowsNotFoundMessage);

            //Get ProjectInvoiceDays
            List<ProjectInvoiceDay> projectInvoiceDays = ProjectManager.GetProjectInvoiceDaysWithWeek(param.ActorCompanyId, orderId, param.UserId);
            projectInvoiceDays = projectInvoiceDays.Where(d => (d.InvoiceTimeInMinutes != 0 || d.WorkTimeInMinutes != 0)).ToList();

            List<TimeCode> timecodes = TimeCodeManager.GetTimeCodes(param.ActorCompanyId);

            bool workedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_WorkedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            bool invoicedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);

            return new MobileTimeRows(param, orderId, projectInvoiceDays, timecodes, workedTimePermission, invoicedTimePermission);
        }

        private MobileTimeRow PerformSaveTimeRow(MobileParam param, int orderId, int dayId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, string note, int timeCodeId, bool isReset = false)
        {
            try
            {
                //LogInfo("PerformSaveTimeRow date: " + date.ToShortDateString());
                //LogInfo("PerformSaveTimeRow invoiceTimeInMinutes: " + invoiceTimeInMinutes);
                //LogInfo("PerformSaveTimeRow workTimeInMinutes: " + workTimeInMinutes);

                //if (param.ActorCompanyId == 115354 || param.ActorCompanyId == 111082 || param.ActorCompanyId == 50845)
                //    LogInfo("PerformSaveTimeRow companyId: " + param.ActorCompanyId + " roleId: " + param.RoleId + " userId: " + param.UserId + " orderId: " + orderId + " timecodeId: " + timeCodeId + " invoiceTimeInMinutes: " + invoiceTimeInMinutes + " date: " + date.ToShortDateShortTimeString());

                bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

                User user = UserManager.GetUser(param.UserId);
                Company company = CompanyManager.GetCompany(param.ActorCompanyId);
                Role role = RoleManager.GetRole(param.RoleId);

                if (user == null || company == null || role == null)
                {
                    LogInfo("PerformSaveTimeRow: company, role or user is missing");
                    return new MobileTimeRow(param, orderId, GetText(8547, "Företag, roll eller användare saknas"));
                }

                //Get MobileOrder
                var order = InvoiceManager.GetCustomerInvoice(orderId, loadProject: true);
                if (order == null)
                    return new MobileTimeRow(param, orderId, Texts.OrderNotFoundMessage);

                if (order.ProjectId.HasValue && order.Project != null && ((order.Project.Status == (int)TermGroup_ProjectStatus.Locked) || (order.Project.Status == (int)TermGroup_ProjectStatus.Finished)))
                    return new MobileTimeRow(param, orderId, GetProjectLockedOrFinishedMessage(order.Project));

                //Save MobileTimeRow
                #region Ugly fix

                //I xe kan man ange ett negativt värde i tidboken, eftersom man inte kan göra det i mobilen så ska denna fix simulera det.            
                if (timeCodeId != 0)
                {
                    var timeCode = TimeCodeManager.GetTimeCodeWithInvoiceProduct(timeCodeId, param.ActorCompanyId);
                    if (timeCode != null && timeCode.TimeCodeInvoiceProduct != null)
                    {
                        var timecodeinvoiceproduct = timeCode.TimeCodeInvoiceProduct.FirstOrDefault();
                        if (timecodeinvoiceproduct != null && timecodeinvoiceproduct.Factor == -1 && invoiceTimeInMinutes > 0)
                            invoiceTimeInMinutes = (invoiceTimeInMinutes * -1);
                    }
                }

                #endregion

                ActionResult result = ProjectManager.SaveProjectInvoiceDay(param.ActorCompanyId, param.RoleId, orderId, date.Date, invoiceTimeInMinutes, workTimeInMinutes, note, timeCodeId, createProjectTimeBlock: (isReset ? false : useProjectTimeBlock));
                bool success = result.Success;

                ProjectInvoiceDay projectInvoiceDay = (success && result.Value != null) ? result.Value as ProjectInvoiceDay : null;
                MobileTimeRow mobileTimeRow = projectInvoiceDay != null ? new MobileTimeRow(param, orderId, projectInvoiceDay, 0, "", false, false) : null;

                if (mobileTimeRow == null)
                {
                    string errorMsg = result.ErrorMessage;
                    if (result.Success)
                        mobileTimeRow = new MobileTimeRow(param, orderId, "");
                    else
                        mobileTimeRow = new MobileTimeRow(param, orderId, string.IsNullOrEmpty(errorMsg) ? Texts.TimeRowNotSavedMessage : errorMsg);
                }

                //Set result
                mobileTimeRow.SetTaskResult(MobileTask.SaveTimeRow, success);

                return mobileTimeRow;
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: PerformSaveTimeRow failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: PerformSaveTimeRow failed, innerexception: " + exp.InnerException.Message);

                return new MobileTimeRow(param, orderId, Texts.TimeRowNotSavedMessage + " : " + exp.Message);
            }
        }

        #endregion

        #region  ProjectTimeBlock - New way to save time on order

        private MobileSettings PerformGetExtendedTimeRegistrationSettings(MobileParam param)
        {
            List<Tuple<string, string>> settings = new List<Tuple<string, string>>();

            //Use project time blocks. If false, extended should always be false.
            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, 0, param.ActorCompanyId, 0);

            //Use extended time registration
            bool useExtendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, param.ActorCompanyId, 0);
            settings.Add(Tuple.Create("UseExtendedTimeRegistration", useExtendedTimeRegistration && useProjectTimeBlocks ? "1" : "0"));

            bool invoicedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            settings.Add(Tuple.Create("ShowInvoicedTime", invoicedTimePermission ? "1" : "0"));

            //Default time deviation cause
            Employee employee = EmployeeManager.GetEmployeeByUser(param.ActorCompanyId, param.UserId, loadEmployment: true);
            settings.Add(Tuple.Create("DefaultTimeDeviationCauseId", employee != null && employee.TimeDeviationCauseId != null ? employee.TimeDeviationCauseId.ToString() : "0"));

            //DoNotUserTime?
            if (employee != null)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(null, GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(param.ActorCompanyId)));
                bool autoGenTimeAndBreakForProject = employeeGroup?.AutoGenTimeAndBreakForProject ?? false;
                settings.Add(Tuple.Create("UseStartStopTime", ((!useExtendedTimeRegistration) || (autoGenTimeAndBreakForProject)) ? "0" : "1"));
            }

            MobileSettings mobileSettings = new MobileSettings(param, settings);
            return mobileSettings;

        }

        private MobileDeviationCauses PerformGetDeviationCausesForTimeRegistration(MobileParam param)
        {
            if (param == null)
                return new MobileDeviationCauses(param, Texts.PayrollPeriodNotFoundMessage);

            Employee employee = EmployeeManager.GetEmployeeByUser(param.ActorCompanyId, param.UserId, loadEmployment: true);

            if (employee == null)
                return new MobileDeviationCauses(param, Texts.EmployeeNotFoundMessage);

            int employeeGroupId = employee.GetEmployeeGroupId();

            List<TimeDeviationCause> deviationCauses = TimeDeviationCauseManager.GetTimeDeviationCausesByEmployeeGroup(param.ActorCompanyId, employeeGroupId, sort: true, loadTimeCode: true, onlyUseInTimeTerminal: true, setTimeDeviationTypeName: true).ToList();
            return new MobileDeviationCauses(param, deviationCauses);
        }

        private MobileEmployeeFirstEligableTime PerformGetEmployeeFirstEligableTime(MobileParam param, int employeeId, DateTime date)
        {
            try
            {
                ProjectManager pm = new ProjectManager(this.parameterObject);
                return new MobileEmployeeFirstEligableTime(param, pm.GetEmployeeFirstEligableTime(employeeId, date, param.ActorCompanyId, param.UserId));
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileEmployeeFirstEligableTime(param, GetText(8778, "Ett fel inträffade"));
            }
        }

        private MobileProjectTimeBlock PerformGetLastProjectTimeBlockOnDate(MobileParam param, int orderId, int employeeId, DateTime date, int timeCodeId)
        {
            MobileProjectTimeBlock mobileProjectTimeBlock = null;

            AttestState initialAttestStateOrder = AttestManager.GetInitialAttestState(base.ActorCompanyId, TermGroup_AttestEntity.Order);
            AttestState initialAttestStatePayroll = AttestManager.GetInitialAttestState(base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);

            ProjectManager pm = new ProjectManager(this.parameterObject);
            List<ProjectTimeBlockDTO> projectTimeBlocks = pm.GetProjectTimeBlocks(0, orderId, (int)SoeProjectRecordType.Order, employeeId, true, date, date, true);
            projectTimeBlocks = projectTimeBlocks.Where(p => p.TimeCodeId == timeCodeId).ToList();

            if (projectTimeBlocks.Any())
                mobileProjectTimeBlock = new MobileProjectTimeBlock(param, orderId, projectTimeBlocks.LastOrDefault(), initialAttestStateOrder != null ? initialAttestStateOrder.AttestStateId : 0, initialAttestStatePayroll != null ? initialAttestStatePayroll.AttestStateId : 0, true, true);
            else
                mobileProjectTimeBlock = new MobileProjectTimeBlock(param, orderId, null, 0, "", 0, "", true, true);

            return mobileProjectTimeBlock;
        }

        private MobileProjectTimeBlock PerformGetProjectTimeBlock(MobileParam param, int orderId, int projectTimeBlockId)
        {
            MobileProjectTimeBlock mobileProjectTimeBlock = null;

            AttestState initialAttestStateOrder = AttestManager.GetInitialAttestState(base.ActorCompanyId, TermGroup_AttestEntity.Order);
            AttestState initialAttestStatePayroll = AttestManager.GetInitialAttestState(base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);

            var pm = new ProjectManager(this.parameterObject);
            int projectId = pm.GetProjectId(orderId);
            int employeeId = EmployeeManager.GetEmployeeIdForUser(param.UserId, param.ActorCompanyId);
            List<ProjectTimeBlockDTO> projectTimeBlocks = pm.GetProjectTimeBlocks(projectId, orderId, (int)SoeProjectRecordType.Order, employeeId, true, null, null, false);
            ProjectTimeBlockDTO projectTimeBlock = projectTimeBlocks.FirstOrDefault(p => p.ProjectTimeBlockId == projectTimeBlockId);

            // No entry found
            if (projectTimeBlock == null)
                mobileProjectTimeBlock = new MobileProjectTimeBlock(param, orderId, null, 0, "", 0, "", true, true);
            else
                mobileProjectTimeBlock = new MobileProjectTimeBlock(param, orderId, projectTimeBlock, initialAttestStateOrder != null ? initialAttestStateOrder.AttestStateId : 0, initialAttestStatePayroll != null ? initialAttestStatePayroll.AttestStateId : 0, true, true);

            return mobileProjectTimeBlock;
        }

        private MobileProjectTimeBlocks PerformGetProjectTimeBlocks(MobileParam param, int orderId, DateTime? fromDate, DateTime? toDate)
        {
            if (param == null)
                return new MobileProjectTimeBlocks(param, orderId, Texts.TimeRowsNotFoundMessage);

            if (fromDate.HasValue && fromDate.Value == DateTime.MinValue)
            {
                fromDate = null;
            }

            if (toDate.HasValue && toDate.Value == DateTime.MinValue)
            {
                toDate = null;
            }

            var pm = new ProjectManager(this.parameterObject);
            int projectId = pm.GetProjectId(orderId);
            int employeeId = EmployeeManager.GetEmployeeIdForUser(param.UserId, param.ActorCompanyId);

            if (employeeId == 0)
            {
                return new MobileProjectTimeBlocks(param, orderId, GetText(5283, "Ingen anställd kopplad till inloggad användare"));
            }

            List<ProjectTimeBlockDTO> projectTimeBlocks;
            if (orderId == 0)
            {
                projectTimeBlocks = pm.LoadProjectTimeBlockForTimeSheet(fromDate, toDate, employeeId, null, null, null, false, true);
            }
            else
            {
                projectTimeBlocks = pm.GetProjectTimeBlocks(projectId, orderId, (int)SoeProjectRecordType.Order, employeeId, true, fromDate, toDate, true, setAttestStates: true);
            }

            AttestState initialAttestStatePayroll = AttestManager.GetInitialAttestState(param.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var initialAttestStateOrder = InvoiceAttestStates.GetInitialAttestStateId(AttestManager, entitiesReadOnly, param.ActorCompanyId, SoeOriginType.Order);
            bool workedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_WorkedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            bool invoicedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);

            return new MobileProjectTimeBlocks(param, orderId, projectTimeBlocks, initialAttestStateOrder, initialAttestStatePayroll?.AttestStateId ?? 0, workedTimePermission, invoicedTimePermission);
        }

        private MobileResult PerformDeleteProjectTimeBlock(MobileParam param, int projectTimeBlockId)
        {
            var saveResult = new ActionResult();
            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
            if (useProjectTimeBlocks)
            {
                var pm = new ProjectManager(this.parameterObject);

                // Newer versions of the app blocks deletion in the UI, no need to make this expensive check here.
                if(Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_39))
                {
                    var timeBlock = pm.GetProjectTimeBlockDTOs(null, null, new List<int> { }, projectTimeBlockIds: new List<int> { projectTimeBlockId }).FirstOrDefault();
                    if (timeBlock != null && (!timeBlock.IsPayrollEditable || !timeBlock.IsEditable))
                    {
                        return new MobileResult(param, Texts.SaveFailed + ": " + GetText(9603, "Det går inte att ta bort rader som är attesterade eller fakturerade."));
                    }
                }

                var itemToSave = new ProjectTimeBlockSaveDTO
                {
                    ProjectTimeBlockId = projectTimeBlockId,
                    State = SoeEntityState.Deleted
                };
                saveResult = pm.SaveProjectTimeBlock(itemToSave);
            }
            else
            {
                var projectInvoiceDay = ProjectManager.GetProjectInvoiceDay(projectTimeBlockId, true);
                if (projectInvoiceDay == null)
                {
                    return new MobileResult(param, Texts.SaveFailed + ": Hittade inte angiven veckodag att ta bort " + projectTimeBlockId.ToString());
                }
                else
                {
                    saveResult = ProjectManager.SaveProjectInvoiceDay(param.ActorCompanyId, param.RoleId, projectInvoiceDay.ProjectInvoiceWeek.RecordId, projectInvoiceDay.Date, 0, 0, null, (int)projectInvoiceDay.ProjectInvoiceWeek.TimeCodeId);
                }
            }

            var mobileResult = new MobileResult(param);

            if (saveResult.Success)
                mobileResult.SetTaskResult(MobileTask.DeleteProjectTimeBlock, true);
            else
                return new MobileResult(param, Texts.SaveFailed + ": " + saveResult.ErrorMessage);

            return mobileResult;
        }

        private MobileResult PerformMoveProjectTimeBlockToDate(MobileParam param, int projectTimeBlockId, DateTime date)
        {
            var saveResult = ProjectManager.MoveTimeRowsToDate(date, new List<int> { projectTimeBlockId }, true);
            var mobileResult = new MobileResult(param);

            if (saveResult.Success)
                mobileResult.SetTaskResult(MobileTask.MoveProjectTimeBlockToDate, true);
            else
                return new MobileResult(param, Texts.SaveFailed + ": " + saveResult.ErrorMessage);

            return mobileResult;
        }

        private MobileProjectTimeBlock PerformSaveProjectTimeBlock(MobileParam param, int orderId, int projectTimeBlockId, DateTime date, DateTime startTime, DateTime stopTime, int invoiceTimeInMinutes, string note, string internalNote, int timeCodeId, int timeDeviationCauseId, int workTimeInMinutes, bool hasValidated, int employeeChildId)
        {
            try
            {
                User user = UserManager.GetUser(param.UserId);
                Company company = CompanyManager.GetCompany(param.ActorCompanyId);
                Role role = RoleManager.GetRole(param.RoleId);

                //Make sure app is not sending any hours and minues
                date = date.Date;

                if (user == null || company == null || role == null)
                {
                    LogInfo("PerformSaveProjectTimeBlock: company, role or user is missing");
                    return new MobileProjectTimeBlock(param, orderId, GetText(8547, "Företag, roll eller användare saknas"));
                }

                //Get MobileOrder
                var order = InvoiceManager.GetCustomerInvoice(orderId, loadProject: true);
                if (order == null)
                    return new MobileProjectTimeBlock(param, orderId, Texts.OrderNotFoundMessage);

                if (order.ProjectId.HasValue && order.Project != null && ((order.Project.Status == (int)TermGroup_ProjectStatus.Locked) || (order.Project.Status == (int)TermGroup_ProjectStatus.Finished)))
                {
                    string msg = GetProjectLockedOrFinishedMessage(order.Project);
                    return new MobileProjectTimeBlock(param, orderId, msg);
                }

                //Get employee
                Employee employee = EmployeeManager.GetEmployeeByUser(base.ActorCompanyId, base.UserId, loadEmployment: true);
                if (employee == null)
                    return new MobileProjectTimeBlock(param, orderId, Texts.EmployeeNotFoundMessage);

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var autoGenTimeAndBreakForProject = employee.GetAutoGenTimeAndBreakForProject(date, GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(param.ActorCompanyId)));
                if (autoGenTimeAndBreakForProject == null)
                {
                    return new MobileProjectTimeBlock(param, orderId, Texts.TimeRowNotSavedMessage + " : " + GetText(8539, "Tidavtal hittades inte"));
                }

                if(invoiceTimeInMinutes == 0 && workTimeInMinutes == 0 && string.IsNullOrEmpty(internalNote))
                {
                    return new MobileProjectTimeBlock(param, orderId, GetText(8234, "Tidraden kunde inte sparas"));
                }

                if (invoiceTimeInMinutes != 0 && timeCodeId == 0)
                {
                    return new MobileProjectTimeBlock(param, orderId, GetText(7476, "Debiteringstyp saknas"));
                }

                //Validate date and times 
                var validationResult = ValidateSaveProjectTimeBlocksForMobile(param, employee.EmployeeId, date, startTime, stopTime, projectTimeBlockId, timeDeviationCauseId, autoGenTimeAndBreakForProject.Value, employeeChildId);
                if (!hasValidated && validationResult.ValidationError)
                {
                    return new MobileProjectTimeBlock(param, orderId, true, validationResult.ValidationErrorData);
                }

                #region Ugly fix

                //I xe kan man ange ett negativt värde i tidboken, eftersom man inte kan göra det i mobilen så ska denna fix simulera det.            
                if (timeCodeId != 0)
                {
                    var timeCode = TimeCodeManager.GetTimeCodeWithInvoiceProduct(timeCodeId, param.ActorCompanyId);
                    if (timeCode != null && timeCode.TimeCodeInvoiceProduct != null)
                    {
                        var timecodeinvoiceproduct = timeCode.TimeCodeInvoiceProduct.FirstOrDefault();
                        if (timecodeinvoiceproduct != null && timecodeinvoiceproduct.Factor == -1 && invoiceTimeInMinutes > 0)
                            invoiceTimeInMinutes = (invoiceTimeInMinutes * -1);
                    }
                }

                #endregion

                var itemToSave = new ProjectTimeBlockSaveDTO
                {
                    ProjectTimeBlockId = projectTimeBlockId,
                    CustomerInvoiceId = order.InvoiceId,
                    ProjectId = order.ProjectId,
                    EmployeeId = employee.EmployeeId,
                    Date = date,
                    From = startTime,
                    To = stopTime,
                    InvoiceQuantity = invoiceTimeInMinutes,
                    ExternalNote = note,
                    InternalNote = internalNote,
                    TimeCodeId = timeCodeId,
                    TimeDeviationCauseId = timeDeviationCauseId != 0 ? timeDeviationCauseId : (int?)null,
                    TimePayrollQuantity = workTimeInMinutes,
                    AutoGenTimeAndBreakForProject = autoGenTimeAndBreakForProject.Value,
                    EmployeeChildId = employeeChildId
                };

                bool success = false;
                MobileProjectTimeBlock mobileProjectTimeBlock = null;

                var saveResult = ProjectManager.SaveProjectTimeBlock(itemToSave);

                if (itemToSave != null && saveResult.Success)
                {
                    var projectTimeBlock = (saveResult != null && saveResult.Value != null) ? saveResult.Value as ProjectTimeBlock : null;
                    mobileProjectTimeBlock = new MobileProjectTimeBlock(param, orderId, projectTimeBlock, 0, "", 0, "", false, false);
                    success = true;
                }

                //Set result
                if (mobileProjectTimeBlock != null)
                    mobileProjectTimeBlock.SetTaskResult(MobileTask.SaveProjectTimeBlock, success);
                else
                    mobileProjectTimeBlock = new MobileProjectTimeBlock(param, orderId, Texts.TimeRowNotSavedMessage);

                return mobileProjectTimeBlock;
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: PerformSaveProjectTimeBlock failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: PerformSaveProjectTimeBlock failed, innerexception: " + exp.InnerException.Message);

                return new MobileProjectTimeBlock(param, orderId, Texts.TimeRowNotSavedMessage + " : " + exp.Message);
            }
        }

        private MobileProjectTimeBlockValidation PerformSaveProjectTimeBlockValidation(MobileParam param, int projectTimeBlockId, DateTime date, DateTime startTime, DateTime stopTime, int timeDeviationCauseId, int employeeChildId)
        {
            try
            {
                User user = UserManager.GetUser(param.UserId);
                Company company = CompanyManager.GetCompany(param.ActorCompanyId);
                Role role = RoleManager.GetRole(param.RoleId);

                if (user == null || company == null || role == null)
                {
                    LogInfo("MobileProjectTimeBlockValidation: company, role or user is missing");
                    return new MobileProjectTimeBlockValidation(param, GetText(8547, "Företag, roll eller användare saknas"));
                }

                //Get employee
                Employee employee = EmployeeManager.GetEmployeeByUser(base.ActorCompanyId, base.UserId, loadEmployment: true);
                if (employee == null)
                {
                    return new MobileProjectTimeBlockValidation(param, Texts.EmployeeNotFoundMessage);
                }

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var autoGenTimeAndBreakForProject = employee.GetAutoGenTimeAndBreakForProject(date, GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(param.ActorCompanyId)));
                if (autoGenTimeAndBreakForProject == null)
                {
                    return new MobileProjectTimeBlockValidation(param, GetText(8539, "Tidavtal hittades inte"));
                }

                //Validate date and times 
                return ValidateSaveProjectTimeBlocksForMobile(param, employee.EmployeeId, date, startTime, stopTime, projectTimeBlockId, timeDeviationCauseId, autoGenTimeAndBreakForProject.GetValueOrDefault(), employeeChildId);
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: PerformSaveProjectTimeBlockValidation failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: PerformSaveProjectTimeBlockValidation failed, innerexception: " + exp.InnerException.Message);

                return new MobileProjectTimeBlockValidation(param, exp.Message);
            }
        }

        private MobileProjectTimeBlockValidation ValidateSaveProjectTimeBlocksForMobile(MobileParam param, int employeeId, DateTime date, DateTime startTime, DateTime stopTime, int projectTimeBlockId, int timeDeviationCauseId, bool autoGenTimeAndBreakForProject, int employeeChildId)
        {
            bool useProjectTimeBlocks = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);

            if (useProjectTimeBlocks)
            {
                var projectTimeBlock = projectTimeBlockId > 0 ? ProjectManager.GetProjectTimeBlock(projectTimeBlockId) : null;

                var result = ProjectManager.ValidateProjectTimeBlockData(projectTimeBlock, employeeId, date, startTime, stopTime, timeDeviationCauseId, employeeChildId, autoGenTimeAndBreakForProject, true);

                if (!result.Success)
                {
                    if (result.BooleanValue2)
                    {
                        List<XElement> overlappingItems = new List<XElement>();
                        List<ProjectTimeBlockDTO> projectTimeBlocks = ProjectManager.GetProjectTimeBlockDTOs(date, date, new List<int> { employeeId }, skipTimeTransactions: true);
                        if (projectTimeBlocks.Any(i => (i.ProjectTimeBlockId != projectTimeBlockId) && ((i.StartTime >= startTime && i.StartTime < stopTime) || (i.StopTime > startTime && i.StopTime <= stopTime) || (i.StartTime <= startTime && i.StopTime >= stopTime) || (i.StartTime >= startTime && i.StopTime <= stopTime))))
                        {
                            foreach (var timeBlock in projectTimeBlocks)
                            {
                                XElement overlappingItem = new XElement("TimeBlock");
                                overlappingItem.Add(new XElement("Time", timeBlock.StartTime.ToShortTimeString() + " - " + timeBlock.StopTime.ToShortTimeString()));
                                overlappingItem.Add(new XElement("Order", timeBlock.InvoiceNr));
                                overlappingItem.Add(new XElement("Cause", timeBlock.TimeDeviationCauseName));
                                overlappingItems.Add(overlappingItem);
                            }
                        }

                        List<XElement> errorMessage = new List<XElement>();
                        errorMessage.Add(new XElement("Success", "0"));
                        errorMessage.Add(new XElement("ValidationMessage", GetText(8807, "Tiden du angivit kolliderar med tidigare registrerad tid.\nKontrollera dagen och försök igen.")));
                        errorMessage.Add(new XElement("TimeBlocks", overlappingItems));
                        return new MobileProjectTimeBlockValidation(param, true, errorMessage, false, null);
                    }
                    else
                    {
                        List<XElement> errorMessage = new List<XElement>();
                        foreach (var errorString in result.Strings)
                        {
                            errorMessage.Add(new XElement("Success", "0"));
                            errorMessage.Add(new XElement("ValidationMessage", errorString));
                        }
                        return new MobileProjectTimeBlockValidation(param, true, errorMessage, false, null);
                    }
                }
                else
                {
                    if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_19))
                    {
                        bool hasInfoMessage = !string.IsNullOrEmpty(result.InfoMessage);
                        return new MobileProjectTimeBlockValidation(param, false, null, hasInfoMessage, hasInfoMessage ? result.InfoMessage : null);
                    }
                    else
                    {
                        return new MobileProjectTimeBlockValidation(param, false, null, false, null);
                    }
                }
            }
            else
            {
                return new MobileProjectTimeBlockValidation(param, false, null, false, null);
            }
        }

        private MobileResult PerformCopyProjectTimeBlocksDate(MobileParam param, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var pm = new ProjectManager(this.parameterObject);

                Employee employee = EmployeeManager.GetEmployeeByUser(param.ActorCompanyId, param.UserId, loadEmployment: true);
                if (employee == null)
                    return new MobileResult(param, Texts.EmployeeNotFoundMessage);

                // Target date must be empty
                if (pm.LoadProjectTimeBlockForTimeSheet(toDate, toDate, employee.EmployeeId, null, null, null, false, true).Any())
                    return new MobileResult(param, GetText(10269, "Måldatum måste vara tom för att kopiera"));

                int daysDifference = (toDate - fromDate).Days;

                var mobileResult = new MobileResult(param);
                var result = CopyProjectTimeBlocksDatesAddDays(param, employee, fromDate, fromDate, daysDifference);

                if (result.Success)
                    mobileResult.SetTaskResult(MobileTask.Save, true);
                else
                    mobileResult = new MobileResult(param, result.ErrorMessage);

                return mobileResult;
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: PerformCopyProjectTimeBlocksDate failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: PerformCopyProjectTimeBlocksDate failed, innerexception: " + exp.InnerException.Message);

                return new MobileResult(param, Texts.SaveFailed);
            }
        }

        private MobileResult PerformCopyProjectTimeBlocksWeek(MobileParam param, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var pm = new ProjectManager(this.parameterObject);

                DateTime firstDateInSourceWeek = CalendarUtility.GetFirstDateOfWeek(fromDate, offset: DayOfWeek.Monday);
                DateTime lastDateInSourceWeek = CalendarUtility.GetLastDateOfWeek(fromDate);
                DateTime firstDateInTargetWeek = CalendarUtility.GetFirstDateOfWeek(toDate, offset: DayOfWeek.Monday);
                DateTime lastDateInTargetWeek = CalendarUtility.GetLastDateOfWeek(toDate);

                Employee employee = EmployeeManager.GetEmployeeByUser(param.ActorCompanyId, param.UserId, loadEmployment: true);
                if (employee == null)
                    return new MobileResult(param, Texts.EmployeeNotFoundMessage);

                // Target week must be empty
                if (pm.LoadProjectTimeBlockForTimeSheet(firstDateInTargetWeek, lastDateInTargetWeek, employee.EmployeeId, null, null, null, false, true).Any())
                    return new MobileResult(param, GetText(10267, "Målveckan måste vara tom för att kopiera"));

                int daysDifference = (firstDateInTargetWeek - firstDateInSourceWeek).Days;

                var mobileResult = new MobileResult(param);
                var result = CopyProjectTimeBlocksDatesAddDays(param, employee, firstDateInSourceWeek, lastDateInSourceWeek, daysDifference);

                if (result.Success)
                    mobileResult.SetTaskResult(MobileTask.Save, true);
                else
                    mobileResult = new MobileResult(param, result.ErrorMessage);

                return mobileResult;
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: PerformCopyProjectTimeBlocksWeek failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: PerformCopyProjectTimeBlocksWeek failed, innerexception: " + exp.InnerException.Message);

                return new MobileResult(param, Texts.SaveFailed);
            }
        }

        #endregion

        #region Expense

        private MobileAdditionDeductionTimeCodes PerformGetAdditionDeductionTimeCodes(MobileParam param, bool isOrder)
        {
            if (param == null)
                return new MobileAdditionDeductionTimeCodes(param, Texts.TimeCodesNotFoundMessage);

            //Fetch             
            Employee employee = EmployeeManager.GetEmployeeByUser(param.ActorCompanyId, param.UserId, loadEmployment: true);
            if (employee == null)
                return new MobileAdditionDeductionTimeCodes(param, Texts.EmployeeNotFoundMessage);

            List<TimeCodeAdditionDeduction> timeCodes = TimeCodeManager.GetTimeCodeAdditionDeductions(base.ActorCompanyId, true, true);

            // Reomve expense types not supported by mobile app
            if(timeCodes != null && (isOrder || Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_40)))
            {
                timeCodes = timeCodes.Where(x => x.ExpenseType != (int)TermGroup_ExpenseType.Time).ToList();
            }

            return new MobileAdditionDeductionTimeCodes(param, timeCodes);
        }

        private MobileExpense PerformGetExpense(MobileParam param, int expenseRowId)
        {
            MobileExpense mobileExpense = null;

            var expenseRow = ExpenseManager.GetExpenseRowForDialog(expenseRowId);
            if (expenseRow != null)
            {
                if (expenseRow.CustomerInvoiceId > 0)
                {
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    var invoice = InvoiceManager.GetCustomerInvoiceDistribution(entitiesReadOnly, SoeOriginType.Order, expenseRow.CustomerInvoiceId, param.ActorCompanyId);
                    expenseRow.CustomerInvoiceNr = StringUtility.NullToEmpty(invoice?.InvoiceNr);
                }

                var timeCode = TimeCodeManager.GetTimeCode(expenseRow.TimeCodeId, base.ActorCompanyId, true, loadInvoiceProducts: true);
                if (timeCode != null)
                    mobileExpense = new MobileExpense(param, expenseRow, timeCode.Name, timeCode.InvoiceProduct.Any());
                else
                    mobileExpense = new MobileExpense(param, null, "", false);
            }
            else
            {
                mobileExpense = new MobileExpense(param, null, "", false);
            }

            return mobileExpense;
        }

        private MobileExpenses PerformGetExpenses(MobileParam param, int orderId)
        {
            if (param == null)
                return new MobileExpenses(param, orderId, Texts.ExpensesFoundMessage);

            // Get attest state 
            AttestState initialAttestStateOrder = AttestManager.GetInitialAttestState(param.ActorCompanyId, TermGroup_AttestEntity.Order);
            AttestState initialAttestStatePayroll = AttestManager.GetInitialAttestState(param.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);

            MobileExpenses mobileExpenses = new MobileExpenses(param, orderId, string.Empty);

            var expenseRows = ExpenseManager.GetExpenseRowsForGrid(orderId, param.ActorCompanyId, param.UserId, param.RoleId, checkFiles: true, includeComments: true);
            if (!expenseRows.IsNullOrEmpty())
            {
                var timeCodes = TimeCodeManager.GetTimeCodes(param.ActorCompanyId, SoeTimeCodeType.AdditionDeduction, true, true);

                foreach (var expenseRow in expenseRows)
                {
                    var timeCode = timeCodes.FirstOrDefault(t => t.TimeCodeId == expenseRow.TimeCodeId);
                    if (timeCode != null)
                        mobileExpenses.AddExpenseRow(new MobileExpense(param, orderId, expenseRow, initialAttestStateOrder.AttestStateId, initialAttestStatePayroll.AttestStateId, timeCode.Name, timeCode.InvoiceProduct.Count > 0, true));
                }
            }

            return mobileExpenses;
        }

        private MobileExpenses PerformGetExpenses(MobileParam param, int employeeId, DateTime from, DateTime to)
        {
            try
            {
                if (param == null)
                    return new MobileExpenses(param, 0, Texts.ExpensesFoundMessage);

                var expenses = TimeTreeAttestManager.GetAttestEmployeeAdditionDeductions(employeeId, from, to, null, true);
                MobileExpenses mobileExpenses = new MobileExpenses(param, 0, string.Empty);
                foreach (var expenseRow in expenses)
                {
                    mobileExpenses.AddExpenseRow(new MobileExpense(param, expenseRow));
                }

                return mobileExpenses;
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: PerformGetExpenses employee failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: PerformGetExpenses employee failed, innerexception: " + exp.InnerException.Message);

                return new MobileExpenses(param, 0, Texts.ExpensesFoundMessage);
            }
        }

        private MobileValueResult PerformGetExpenseProductPrice(MobileParam param, int invoiceId, int timeCodeId, decimal quantity)
        {
            try
            {
                var timeCode = TimeCodeManager.GetTimeCode(timeCodeId, param.ActorCompanyId, true, true);

                if (timeCode == null || timeCode.TimeCodeInvoiceProduct.IsNullOrEmpty())
                {
                    return new MobileValueResult(param, GetText(9500, "Pris hittades inte"));
                }

                var products = timeCode.TimeCodeInvoiceProduct.ToList();

                decimal price = 0;
                foreach (TimeCodeInvoiceProduct product in products)
                {
                    price += product.Factor * ProductManager.GetProductPriceForCustomerInvoice(product.ProductId, invoiceId, param.ActorCompanyId, quantity);
                }

                return new MobileValueResult(param, price);
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformGetExpenseProductPrice: Error= {0} ", e.Message));
                return new MobileValueResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileExpense PerformSaveExpense(MobileParam param, int orderId, int expenseRowId, int timeCodeId, DateTime from, DateTime to, DateTime startTime, DateTime stopTime, decimal quantity, bool specifiedUnitPrice, decimal unitPrice, decimal amount, decimal vat, bool transferToInvoice, decimal invoiceAmount, string internalComment, string externalComment)
        {
            try
            {
                User user = UserManager.GetUser(param.UserId);
                Company company = CompanyManager.GetCompany(param.ActorCompanyId);
                Role role = RoleManager.GetRole(param.RoleId);

                if (user == null || company == null || role == null)
                {
                    LogInfo("PerformSaveExpense: company, role or user is missing");
                    return new MobileExpense(param, GetText(8547, "Företag, roll eller användare saknas"));
                }

                if (vat > amount)
                {
                    return new MobileExpense(param, GetText(7470, "Momsbeloppet kan inte vara större än totalbeloppet"));
                }

                var timeCode = TimeCodeManager.GetTimeCode(timeCodeId, param.ActorCompanyId, false);
                if (timeCode is TimeCodeAdditionDeduction && ((TimeCodeAdditionDeduction)timeCode).CommentMandatory && string.IsNullOrEmpty(internalComment))
                {
                    return new MobileExpense(param, GetText(147, "Du måsta ange kommentar"));
                }

                //Get MobileOrder - expenses could be saved without order?
                CustomerInvoice order = null;
                if (orderId > 0)
                {
                    order = InvoiceManager.GetCustomerInvoice(orderId, loadProject: true);
                    if (order == null)
                        return new MobileExpense(param, Texts.OrderNotFoundMessage);
                }

                if (order != null && order.ProjectId.HasValue && order.Project != null && (order.Project.Status == (int)TermGroup_ProjectStatus.Locked || order.Project.Status == (int)TermGroup_ProjectStatus.Finished))
                {
                    string msg = GetProjectLockedOrFinishedMessage(order.Project);
                    return new MobileExpense(param, msg);
                }

                //Get employee
                Employee employee = EmployeeManager.GetEmployeeByUser(base.ActorCompanyId, base.UserId, loadEmployment: true);
                if (employee == null)
                    return new MobileExpense(param, Texts.EmployeeNotFoundMessage);

                bool success = false;
                ExpenseRowDTO expenseRow = (expenseRowId > 0) ? ExpenseManager.GetExpenseRowForDialog(expenseRowId) : new ExpenseRowDTO();
                MobileExpense mobileExpense = null;

                // Get existing
                if (expenseRowId > 0)
                {
                    expenseRow.CustomerInvoiceId = orderId;
                    expenseRow.ProjectId = order != null && order.ProjectId.HasValue ? order.ProjectId.Value : 0;
                }
                else
                {
                    expenseRow.EmployeeId = employee.EmployeeId;
                    expenseRow.CustomerInvoiceId = orderId;
                    expenseRow.ProjectId = order != null && order.ProjectId.HasValue ? order.ProjectId.Value : 0;
                }

                expenseRow.TimeCodeId = timeCodeId;
                expenseRow.Start = new DateTime(from.Year, from.Month, from.Day, startTime.Hour, startTime.Minute, startTime.Second);
                expenseRow.Stop = new DateTime(to.Year, to.Month, to.Day, stopTime.Hour, stopTime.Minute, stopTime.Second);
                expenseRow.Quantity = quantity;
                expenseRow.IsSpecifiedUnitPrice = specifiedUnitPrice;
                expenseRow.UnitPrice = unitPrice;
                expenseRow.Amount = amount;
                expenseRow.Vat = vat;
                expenseRow.TransferToOrder = transferToInvoice;
                expenseRow.InvoicedAmount = invoiceAmount;
                expenseRow.Comment = internalComment;
                expenseRow.ExternalComment = externalComment;

                // Set values
                ActionResult saveResult = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveExpense(expenseRow, orderId, true);
                if (saveResult.Success)
                {
                    var savedExpenseRow = saveResult.Value as ExpenseRow;
                    mobileExpense = new MobileExpense(param, savedExpenseRow.ToDTO(), "", false);
                    success = true;
                }

                //Set result
                if (mobileExpense != null)
                    mobileExpense.SetTaskResult(MobileTask.SaveExpense, success);
                else
                    mobileExpense = new MobileExpense(param, Texts.ExpenseNotSavedMessage);

                return mobileExpense;
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: PerformSaveExpense failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: PerformSaveExpense failed, innerexception: " + exp.InnerException.Message);

                return new MobileExpense(param, Texts.ExpenseNotSavedMessage + " : " + exp.Message);
            }
        }

        private MobileResult PerformDeleteExpense(MobileParam param, int expenseRowId)
        {
            var saveResult = TimeEngineManager(param.ActorCompanyId, param.UserId).DeleteExpense(expenseRowId);

            var mobileResult = new MobileResult(param);

            if (saveResult.Success)
                mobileResult.SetTaskResult(MobileTask.DeleteExpense, true);
            else
                return new MobileResult(param, Texts.SaveFailed + ": " + saveResult.ErrorMessage);

            return mobileResult;
        }

        private MobileMessageBox PerformSaveExpenseValidation(MobileParam param, int orderId, int expenseRowId, int timeCodeId, DateTime from, DateTime to, DateTime startTime, DateTime stopTime, decimal quantity, bool specifiedUnitPrice, decimal unitPrice, decimal amount, decimal vat, bool transferToInvoice, decimal invoiceAmount, string internalComment, string externalComment)
        {
            try
            {
                User user = UserManager.GetUser(param.UserId);
                Company company = CompanyManager.GetCompany(param.ActorCompanyId);
                Role role = RoleManager.GetRole(param.RoleId);

                if (user == null || company == null || role == null)
                {
                    LogInfo("PerformSaveExpenseValidation: company, role or user is missing");
                    return new MobileMessageBox(param, GetText(8547, "Företag, roll eller användare saknas"));
                }

                var timeCode = TimeCodeManager.GetTimeCode(timeCodeId, param.ActorCompanyId, false);
                if (timeCode is TimeCodeAdditionDeduction && ((TimeCodeAdditionDeduction)timeCode).CommentMandatory && string.IsNullOrEmpty(internalComment))
                {
                    return new MobileMessageBox(param, GetText(147, "Du måsta ange kommentar"));
                }

                //Get MobileOrder - expenses could be saved without order?
                CustomerInvoice order = null;
                if (orderId > 0)
                {
                    order = InvoiceManager.GetCustomerInvoice(orderId, loadProject: true);
                    if (order == null)
                        return new MobileMessageBox(param, Texts.OrderNotFoundMessage);
                }

                //Get employee
                Employee employee = EmployeeManager.GetEmployeeByUser(base.ActorCompanyId, base.UserId, loadEmployment: true);
                if (employee == null)
                    return new MobileMessageBox(param, Texts.EmployeeNotFoundMessage);

                ExpenseRowDTO expenseRow = (expenseRowId > 0) ? ExpenseManager.GetExpenseRowForDialog(expenseRowId) : new ExpenseRowDTO();

                // Get existing
                if (expenseRowId > 0)
                {
                    expenseRow.CustomerInvoiceId = orderId;
                    expenseRow.ProjectId = order != null && order.ProjectId.HasValue ? order.ProjectId.Value : 0;
                }
                else
                {
                    expenseRow.EmployeeId = employee.EmployeeId;
                    expenseRow.CustomerInvoiceId = orderId;
                    expenseRow.ProjectId = order != null && order.ProjectId.HasValue ? order.ProjectId.Value : 0;
                }

                expenseRow.TimeCodeId = timeCodeId;
                expenseRow.Start = new DateTime(from.Year, from.Month, from.Day, startTime.Hour, startTime.Minute, startTime.Second);
                expenseRow.Stop = new DateTime(to.Year, to.Month, to.Day, stopTime.Hour, stopTime.Minute, stopTime.Second);
                expenseRow.Quantity = quantity;
                expenseRow.IsSpecifiedUnitPrice = specifiedUnitPrice;
                expenseRow.UnitPrice = unitPrice;
                expenseRow.Amount = amount;
                expenseRow.Vat = vat;
                expenseRow.TransferToOrder = transferToInvoice;
                expenseRow.InvoicedAmount = invoiceAmount;
                expenseRow.Comment = internalComment;
                expenseRow.ExternalComment = externalComment;

                var result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveExpenseValidation(expenseRow);
                return new MobileMessageBox(param, result.Success, result.CanOverride, true, true, result.Title, result.Message);
            }
            catch (Exception e)
            {
                LogError("Mobilemanager: PerformSaveExpenseValidation failed: " + e.Message);

                return new MobileMessageBox(param, Texts.InternalErrorMessage + e.Message);
            }
        }

        #endregion

        #region TimePeriod

        private MobileTimePeriods PerformGetPayrollPeriods(MobileParam param, int employeeId, MobileDisplayMode displayMode)
        {
            if (param == null)
                return new MobileTimePeriods(param, Texts.PayrollPeriodNotFoundMessage);

            try
            {
                bool showPreviousTimePeriodAsDefault = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultPreviousTimePeriod, 0, param.ActorCompanyId, 0);
                List<TimePeriod> timePeriods = new List<TimePeriod>();
                if (displayMode == MobileDisplayMode.User)
                {
                    Employee employee = EmployeeManager.GetEmployeeIgnoreState(param.ActorCompanyId, employeeId, loadEmployment: true);
                    if (employee == null)
                        return new MobileTimePeriods(param, Texts.PayrollPeriodNotFoundMessage);

                    timePeriods = TimePeriodManager.GetDefaultTimePeriods(TermGroup_TimePeriodType.Payroll, false, employee.GetPayrollGroupId(), employee.EmployeeId, param.ActorCompanyId).ToList();
                }
                else if (displayMode == MobileDisplayMode.Admin)
                {
                    timePeriods = TimePeriodManager.GetDefaultTimePeriods(TermGroup_TimePeriodType.Payroll, false, null, null, param.ActorCompanyId).ToList();
                }

                timePeriods = timePeriods.Where(x => !x.ExtraPeriod).OrderByDescending(x => x.StartDate).ToList();
                TimePeriod currentTimePeriod = timePeriods.FirstOrDefault(tp => tp.StartDate.Date <= DateTime.Now.Date && tp.StopDate >= DateTime.Now.Date);
                if (currentTimePeriod != null)
                {
                    TimePeriod previousPeriod = null;
                    if (showPreviousTimePeriodAsDefault)
                    {
                        var currentTimePeriodIndex = timePeriods.IndexOf(currentTimePeriod);
                        previousPeriod = currentTimePeriodIndex < timePeriods.Count - 1 ? timePeriods[currentTimePeriodIndex + 1] : null;
                    }

                    if (previousPeriod != null)
                        previousPeriod.ShowAsDefault = true;
                    else
                        currentTimePeriod.ShowAsDefault = true;
                }

                return new MobileTimePeriods(param, timePeriods);
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformGetPayrollPeriods: Error= {0} ", e.Message));
                return new MobileTimePeriods(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion

        #region Customer

        private MobileNewCustomer PerformGetNextCustomerNr(MobileParam param)
        {
            string nextCustomerNr = CustomerManager.GetNextCustomerNr(param.ActorCompanyId);
            MobileNewCustomer newCustomer = new MobileNewCustomer(param);
            newCustomer.customerNr = nextCustomerNr;
            return newCustomer;
        }

        private MobileCustomersGrid PerformGetCustomersGrid(MobileParam param)
        {
            //Get settings
            List<FieldSetting> fieldSettings = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.CustomerGrid, param.RoleId, param.ActorCompanyId);

            //Fetch customers
            //List<Customer> customers = CustomerManager.GetCustomersByCompany(param.ActorCompanyId, onlyActive: true).ToList();
            var customers = CustomerManager.GetCustomersBySearch(new CustomerSearchDTO { }, param.ActorCompanyId, 100000);

            MobileCustomersGrid customerGrids = new MobileCustomersGrid(param, customers, fieldSettings);

            return customerGrids;
        }

        private MobileCustomerEdit PerformGetCustomerEdit(MobileParam param, int customerId)
        {
            //Get settings
            List<FieldSetting> fieldSettings = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.CustomerEdit, param.RoleId, param.ActorCompanyId);

            //Fetch customer
            Customer customer = CustomerManager.GetCustomer(customerId);
            if (customer == null)
                return new MobileCustomerEdit(param, Texts.CustomerNotFoundMessage);

            //Make sure references are loaded
            if (!customer.PaymentConditionReference.IsLoaded)
                customer.PaymentConditionReference.Load();
            if (!customer.PriceListTypeReference.IsLoaded)
                customer.PriceListTypeReference.Load();

            //Fetch Currency
            Dictionary<int, string> currencyDict = CountryCurrencyManager.GetCompCurrenciesDict(param.ActorCompanyId, false);
            currencyDict.TryGetValue(customer.CurrencyId, out string currencyName);

            //Fetch WholeSeller
            string wholeSellerName = string.Empty;
            if (customer.SysWholeSellerId.HasValue && customer.SysWholeSellerId.Value >= 0)
            {
                Dictionary<int, string> wholeSellersDict = WholeSellerManager.GetWholesellerDictByCompany(param.ActorCompanyId, false);
                wholeSellersDict.TryGetValue(customer.SysWholeSellerId.Value, out wholeSellerName);
            }

            //Fetch VatType
            string vatTypename = GetText(customer.VatType, (int)TermGroup.InvoiceVatType);

            // Fetch InvoiceDeliveryType
            string invoiceDeliveryTypeName = customer.InvoiceDeliveryType.HasValue ? GetText((int)customer.InvoiceDeliveryType, (int)TermGroup.InvoiceDeliveryType) : "";

            //Fetch Contact
            int contactId = ContactManager.GetContactIdFromActorId(customerId);
            List<ContactECom> customerEcoms = ContactManager.GetContactEComs(contactId);
            List<ContactAddress> customerAddress = ContactManager.GetContactAddresses(contactId);

            return new MobileCustomerEdit(param, customer, customerEcoms, customerAddress, vatTypename, wholeSellerName, currencyName, invoiceDeliveryTypeName, fieldSettings);
        }

        private MobileTextBlock PerformGetCustomerNote(MobileParam param, int customerId)
        {
            try
            {
                if (customerId > 0)
                {
                    var fieldSettingsCustomerEdit = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.CustomerEdit, param.RoleId, param.ActorCompanyId);
                    var customer = CustomerManager.GetCustomer(customerId);

                    if((customer?.ShowNote ?? false) && FieldSettingManager.DoShowMobileField(fieldSettingsCustomerEdit, TermGroup_MobileFields.CustomerEdit_Note, true, false))
                    {
                        var textBlock = new Textblock
                        {
                            TextblockId = customerId,
                            Text = customer.Note
                        };

                        return new MobileTextBlock(param, textBlock);
                    }
                }

                var emptyTextBlock = new Textblock
                {
                    TextblockId = 0,
                    Text = ""
                };

                return new MobileTextBlock(param, emptyTextBlock);
            }
            catch (Exception e)
            {
                LogError("PerformGetCustomerNote: " + e.Message);
                return new MobileTextBlock(param, GetText(8455, "Internt fel"));
            } 
        }

        private MobileCustomerEdit PerformSaveCustomer(MobileParam param, int customerId, string customerNr, string name, string orgNr, string reference, string note, int vatTypeId, string vatNr, int paymentConditionId, int salesPriceListId, int stdWholeSellerId, int currencyId, decimal disccountArticles, decimal disccountServices, int emailAddressId, String emailAddress, int homePhoneId, string homePhone, int jobPhoneId, string jobPhone, int mobilePhoneId, string mobilePhone, int faxId, string fax, int invoiceAddressId, string invoiceAddress, string iaPostalCode, string iaPostalAddress, string iaCountry, string iaAddressCO, int deliveryAddress1Id, string deliveryAddress1, string da1PostalCode, string da1PostalAddress, string da1Country, string da1AddressCO, string da1Name, int invoiceDeliveryTypeId)
        {
            MobileCustomerEdit mobileCustomer = new MobileCustomerEdit(param);

            if (string.IsNullOrEmpty(customerNr) || string.IsNullOrEmpty(name))
                return new MobileCustomerEdit(param, GetText(8301, "Både kundnr och namn måste anges"));

            if (currencyId <= 0)
            {
                var currency = CountryCurrencyManager.GetCompanyBaseCurrency(param.ActorCompanyId);
                if (currency == null)
                    return new MobileCustomerEdit(param, GetText(8307, "Du måste välja valuta"));

                currencyId = currency.CurrencyId;
            }

            if (vatTypeId == -1)
                vatTypeId = 0;
            if (stdWholeSellerId == -1)
                stdWholeSellerId = 0;

            var updateInvoiceDeliveryType = false;
            if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_37))
                updateInvoiceDeliveryType = true;

            ActionResult result = CustomerManager.SaveMobileCustomer(customerId, customerNr, name, orgNr, reference, note, vatNr, vatTypeId, paymentConditionId, salesPriceListId, stdWholeSellerId, currencyId, disccountArticles, disccountServices, emailAddressId, emailAddress, homePhoneId, homePhone, jobPhoneId, jobPhone, mobilePhoneId, mobilePhone, faxId, fax, invoiceAddressId, invoiceAddress, iaPostalCode, iaPostalAddress, iaCountry, iaAddressCO, deliveryAddress1Id, deliveryAddress1, da1PostalCode, da1PostalAddress, da1Country, da1AddressCO, da1Name, invoiceDeliveryTypeId, param.RoleId, param.ActorCompanyId, updateInvoiceDeliveryType: updateInvoiceDeliveryType);
            if (result.Success)
            {
                if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_27))
                    mobileCustomer = new MobileCustomerEdit(param, result.IntegerValue);

                mobileCustomer.SetTaskResult(MobileTask.SaveCustomer, true);
            }
            else
            {
                // Error saving customer
                if (result.ErrorNumber == (int)ActionResultSave.CustomerExists)
                    mobileCustomer = new MobileCustomerEdit(param, FormatMessage(GetText(8297, "En kund med angivet kundnummer finns redan"), result.ErrorNumber));
                else if (result.ErrorNumber == (int)ActionResultSave.CustomerNotSaved)
                    mobileCustomer = new MobileCustomerEdit(param, FormatMessage(GetText(8298, "Kund kunde inte sparas"), result.ErrorNumber));
                else if (result.ErrorNumber == (int)ActionResultSave.CustomerNotUpdated)
                    mobileCustomer = new MobileCustomerEdit(param, FormatMessage(GetText(8299, "Kund kunde inte uppdateras"), result.ErrorNumber));
                else if (result.ErrorNumber == (int)ActionResultSave.InsufficienPermissionToSave)
                    mobileCustomer = new MobileCustomerEdit(param, FormatMessage(GetText(8652, "Otillåten ändring, behörighet saknas"), result.ErrorNumber));
                else
                    mobileCustomer = new MobileCustomerEdit(param, FormatMessage(GetText(8300, "Ett fel uppstod, kunden kunde inte sparas"), result.ErrorNumber));
            }

            return mobileCustomer;
        }

        private MobileCustomers PerformSearchCustomers(MobileParam param, string search)
        {
            if (param == null)
                return new MobileCustomers(param, Texts.CustomersNotFoundMessage);

            //Get Customers
            String searchParam;
            if(String.IsNullOrEmpty(search))
            {
                searchParam = "";
            }
            else
            {
                searchParam = search.Contains("*") ? search : "*" + search + "*";
            }

            var customers = CustomerManager.GetCustomersBySearch(new CustomerSearchDTO
            {
                NameOrCustomerNrOrAddress = searchParam,
            },
            param.ActorCompanyId, MobileCustomers.MAXFETCH);

            return new MobileCustomers(param, customers);
        }

        private MobileContactPerson PerformGetCustomerContactPerson(MobileParam param, int customerId, string name)
        {
            if (param == null)
                return new MobileContactPerson(param, GetText(9604, "Referens kunde inte hittas."));

            try
            {
                var contactPersons = ContactManager.GetContactPersons(customerId);
                var contactPerson = contactPersons.FirstOrDefault(c => c.Name == name);

                if(contactPerson == null)
                    return new MobileContactPerson(param, GetText(9604, "Referens kunde inte hittas."));

                
                var addresses = ContactManager.GetContactAddressItemsDict(contactPerson.ActorContactPersonId);
                var email = addresses.ContainsKey((int)TermGroup_SysContactEComType.Email) ? addresses[(int)TermGroup_SysContactEComType.Email] : "";
                var phoneNumber = addresses.ContainsKey((int)TermGroup_SysContactEComType.PhoneMobile) ? addresses[(int)TermGroup_SysContactEComType.PhoneMobile] : "";

                return new MobileContactPerson(param, contactPerson.ActorContactPersonId, contactPerson.Name, email, phoneNumber);
            }
            catch (Exception e)
            {
                LogError("PerformGetCustomerContactPerson: " + e.Message);
                return new MobileContactPerson(param, GetText(8455, "Internt fel"));
            }
        }

        #region Field Settings

        private MobileCustomerEditFieldSettings PerformGetCustomerEditFieldSettings(MobileParam param)
        {
            //Get settings
            List<FieldSetting> fieldSettings = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.CustomerEdit, param.RoleId, param.ActorCompanyId);
            bool editCustomerPermission = FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit, Permission.Modify, param.RoleId, param.ActorCompanyId);
            bool editHHTDApplicantsPermission = FeatureManager.HasRolePermission(Feature.Billing_Customer_Customers_Edit_HouseholdTaxDeductionApplicants, Permission.Modify, param.RoleId, param.ActorCompanyId) && !SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingCustomerHideTaxDeductionContacts, 0, param.ActorCompanyId, 0);

            MobileCustomerEditFieldSettings settings = new MobileCustomerEditFieldSettings(param, fieldSettings, editCustomerPermission, editHHTDApplicantsPermission);
            return settings;
        }

        private MobileCustomerGridFieldSettings PerformGetCustomerGridFieldSettings(MobileParam param)
        {
            //Get settings
            List<FieldSetting> fieldSettings = FieldSettingManager.GetFieldSettingsForMobileForm(TermGroup_MobileForms.CustomerGrid, param.RoleId, param.ActorCompanyId);

            MobileCustomerGridFieldSettings settings = new MobileCustomerGridFieldSettings(param, fieldSettings);
            return settings;
        }

        #endregion

        #endregion

        #region Order Type

        private MobileOrderTypes PerformGetOrderTypes(MobileParam param)
        {
            if (param == null)
                return new MobileOrderTypes(param, GetText(7591, "Ordertyper kunde inte hittas"));

            try
            {
                var terms = base.GetTermGroupDict(TermGroup.OrderType, GetLangId(), addEmptyRow: true);
                return new MobileOrderTypes(param, terms);
            }
            catch (Exception e)
            {
                LogError("PerformGetOrderTypes: " + e.Message);
                return new MobileOrderTypes(param, GetText(8455, "Internt fel"));
            }
        }

        #endregion

        #region Vattype

        private MobileVatTypes PerformGetVatTypes(MobileParam param)
        {
            if (param == null)
                return new MobileVatTypes(param, Texts.VatTypesNotFoundMessage);

            var terms = base.GetTermGroupDict(TermGroup.InvoiceVatType, GetLangId(), addEmptyRow: true);
            return new MobileVatTypes(param, terms);
        }

        #endregion

        #region Payment Conditions

        private MobilePaymentConditions PerformGetPaymentConditions(MobileParam param)
        {
            if (param == null)
                return new MobilePaymentConditions(param, Texts.PriceListTypeNotFoundMessage);

            //Fetch 
            Dictionary<int, string> paymentconditions = PaymentManager.GetPaymentConditionsDict(param.ActorCompanyId, true);

            return new MobilePaymentConditions(param, paymentconditions);
        }

        #endregion

        #region Currency

        private MobileCurrencies PerformGetCurrencies(MobileParam param)
        {
            if (param == null)
                return new MobileCurrencies(param, Texts.CurrenciesNotFoundMessage);

            //Fetch 
            Dictionary<int, string> currencyDict = CountryCurrencyManager.GetCompCurrenciesDict(param.ActorCompanyId, false);

            return new MobileCurrencies(param, currencyDict);
        }

        #endregion

        #region PriceList

        private MobilePriceLists PerformGetPriceLists(MobileParam param)
        {
            if (param == null)
                return new MobilePriceLists(param, Texts.PriceListTypeNotFoundMessage);

            //Fetch 
            Dictionary<int, string> pricelistTypes = ProductPricelistManager.GetPriceListTypesDict(param.ActorCompanyId, true);

            return new MobilePriceLists(param, pricelistTypes);
        }

        #endregion

        #region WholeSeller

        private MobileWholeSellers PerformGetWholeSellers(MobileParam param)
        {
            if (param == null)
                return new MobileWholeSellers(param, Texts.WholeSellersNotFoundMessage);

            //Fetch 
            Dictionary<int, string> wholeSellers = WholeSellerManager.GetWholesellerDictByCompany(param.ActorCompanyId, true);

            return new MobileWholeSellers(param, wholeSellers);
        }

        #endregion

        #region ContactPerson

        private MobileContactPersons PerformGetContactPersons(MobileParam param, int customerId)
        {
            #region wrong
            List<ContactPerson> contacts = new List<ContactPerson>();
            //MobileContactPerson customerReference = null;

            ////Fetch 
            //Customer customer = CustomerManager.GetCustomer(customerId);
            //if (customer == null)
            //    return new MobileContactPersons(param, CustomerNotFoundMessage);

            //List<ContactPerson> contactForCustomer = ContactManager.GetContactPersons(customerId);
            //List<ContactPerson> contactForCompany = ContactManager.GetContactPersons(param.ActorCompanyId);

            //contacts.AddRange(contactForCustomer);
            //contacts.AddRange(contactForCompany);

            //MobileContactPersons contactPersons = new MobileContactPersons(param, contacts);
            //if (!String.IsNullOrEmpty(customer.InvoiceReference))
            //{
            //    customerReference = new MobileContactPerson(param, 0, customer.InvoiceReference);
            //    contactPersons.AddMobileContactPerson(customerReference);
            //}

            #endregion

            var users = UserManager.GetUsersByCompanyDict(param.ActorCompanyId, param.RoleId, param.UserId, false, false, true, false);
            MobileContactPersons contactPersons = new MobileContactPersons(param, users);

            return contactPersons;
        }

        #endregion

        #region Delivery/Invoice Address

        private MobileOrderAddressItems PerformGetDeliveryAddress(MobileParam param, int customerId)
        {
            int contactId = ContactManager.GetContactIdFromActorId(customerId);
            List<ContactAddress> contactsAddress = ContactManager.GetContactAddresses(contactId, TermGroup_SysContactAddressType.Delivery, true);
            FormatAddresses(contactsAddress);
            MobileOrderAddressItems address = new MobileOrderAddressItems(param, contactsAddress);

            return address;
        }

        private MobileOrderAddressItems PerformGetInvoiceAddress(MobileParam param, int customerId)
        {
            int contactId = ContactManager.GetContactIdFromActorId(customerId);
            List<ContactAddress> contactsAddress = ContactManager.GetContactAddresses(contactId, TermGroup_SysContactAddressType.Billing, true);
            FormatAddresses(contactsAddress);
            MobileOrderAddressItems address = new MobileOrderAddressItems(param, contactsAddress);

            return address;
        }

        #endregion

        
        #region InvoiceDeliveryType

        private MobileDicts PerformGetInvoiceDeliveryTypes(MobileParam param)
        {
            try
            {
                if (param == null)
                    return new MobileDicts(param, Texts.InvoiceDeliveryTypesNotFoundMessage);

                var terms = base.GetTermGroupDict(TermGroup.InvoiceDeliveryType, GetLangId(), addEmptyRow: false);
                terms = terms.ToDictionary(p => p.Key, p => p.Value);

                var eInvoiceFormat = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceFormat, 0, param.ActorCompanyId, 0);
                if (eInvoiceFormat != (int)TermGroup_EInvoiceFormat.Intrum)
                {
                    terms = terms.Where((x) => x.Key != (int)SoeInvoiceDeliveryType.EDI).ToDictionary(p => p.Key, p => p.Value);
                }

                return new MobileDicts(param, terms);
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: PerformGetInvoiceDeliveryTypes failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: PerformGetInvoiceDeliveryTypes failed, innerexception: " + exp.InnerException.Message);

                return new MobileDicts(param, Texts.InternalErrorMessage);
            }
        }

        #endregion

        #region General Documents
        private MobileResult PerformAddDocument(MobileParam param, int recordId, int entityType, int documentType, byte[] data, string description, string fileName, bool updateExtension = true)
        {

            if (recordId == 0)
                return new MobileResult(param, Texts.RecordIdMissing);

            if (documentType == 0)
                return new MobileResult(param, Texts.DocumentTypeMissing);

            var dataStorageRecordType = (SoeDataStorageRecordType)documentType;
            var soeEntityType = (SoeEntityType)entityType;

            // Remove when mobile app is updated to always send fileName with extension
            fileName = string.IsNullOrEmpty(fileName) ? Guid.NewGuid().ToString() : fileName;
            if (updateExtension && !fileName.ToLower().EndsWith(".jpg"))
            {
                fileName += ".jpg";
            }

            var fileDto = new DataStorageRecordExtendedDTO
            {
                Data = data,
                Type = dataStorageRecordType,
                Entity = GeneralManager.EntityNoneDataStorageTypes.Contains(dataStorageRecordType) ? SoeEntityType.None : soeEntityType,
                Description = description,
                FileName = fileName,
                RecordId = recordId
            };

            if (dataStorageRecordType == SoeDataStorageRecordType.OrderInvoiceSignature && string.IsNullOrEmpty(fileDto.Description))
                fileDto.Description = GetText(8401, "Ordersignatur") + " " + DateTime.Now.Date.ToShortDateString();

            var result = soeEntityType == SoeEntityType.Order ? 
                InvoiceManager.SaveCustomerInvoiceAttachment(recordId, fileDto, param.ActorCompanyId) :
                GeneralManager.SaveDataStorageRecord(param.ActorCompanyId, fileDto, false);

            var mobileResult = new MobileResult(param);

            if (result.Success)
                mobileResult.SetTaskResult(MobileTask.Save, true);
            else
                mobileResult = new MobileResult(param, FormatMessage(Texts.ImageNotSaved, result.ErrorNumber));

            return mobileResult;
        }

        private MobileImages PerformGetDocuments(MobileParam param, int recordId, int entityType, string documentTypes, bool ignoreFileTypes)
        {
            if (recordId == 0)
                return new MobileImages(param, Texts.RecordIdMissing);

            if (string.IsNullOrEmpty(documentTypes))
                return new MobileImages(param, Texts.DocumentTypeMissing);

            var imageFileExtensions = new List<string>() { ".png", ".jpg", ".jpeg" };
            var fileExtensions = new List<string>() { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".xml" };

            List<SoeDataStorageRecordType> parsedDocumentTypes = documentTypes.Split(',').Select(i => (SoeDataStorageRecordType)Convert.ToInt32(i)).ToList();

            var fileDTOs = GeneralManager.GetDataStorageRecords(param.ActorCompanyId, param.RoleId, recordId, (SoeEntityType)entityType, parsedDocumentTypes, false, true).ToImagesDTOs(true, null, true, false).ToList();

            //Order/Invoice that is stored without entity type
            var currentNonEntityDataStorageTypes = parsedDocumentTypes.Where(x => GeneralManager.EntityNoneDataStorageTypes.Contains(x)).ToList();
            if (currentNonEntityDataStorageTypes.Any())
            {
                fileDTOs.AddRange(GeneralManager.GetDataStorageRecords(param.ActorCompanyId, param.RoleId, recordId, SoeEntityType.None, currentNonEntityDataStorageTypes, false, true).ToImagesDTOs(true, null, true, false).ToList());
            }

            //images
            var images = new List<Tuple<int, string, string, int, string, String>>();
            var imagesDTOs = fileDTOs.Where(x => imageFileExtensions.Contains(Path.GetExtension(x.FileName).ToLower())).ToList();

            if (entityType == (int)SoeEntityType.Order && parsedDocumentTypes.Contains(SoeDataStorageRecordType.ChecklistHeadRecordSignature))
            {
                imagesDTOs.AddRange(ChecklistManager.GetEntityChecklistsSignatures(param.ActorCompanyId, SoeEntityType.Order, recordId));
            }

            foreach (var imageDTO in imagesDTOs)
            {
                images.Add(GetSendImageProperties(param, imageDTO, true));
            }

            //Files
            var files = new List<Tuple<int, string, string, int, string, bool>>();
            if (ignoreFileTypes)
                fileDTOs = fileDTOs.Where(x => !imageFileExtensions.Contains(Path.GetExtension(x.FileName).ToLower())).ToList();
            else
                fileDTOs = fileDTOs.Where(x => fileExtensions.Contains(Path.GetExtension(x.FileName).ToLower())).ToList();

            foreach (var fileDTO in fileDTOs)
            {
                string path = CreateFileOnServer(param, fileDTO.ImageId, fileDTO.FileName, fileDTO.Image);
                files.Add(Tuple.Create<int, string, string, int, string, bool>(fileDTO.ImageId, path, fileDTO.Description, (int)fileDTO.DataStorageRecordType, Path.GetExtension(fileDTO.FileName), fileDTO.CanDelete));
            }

            //Add the images
            MobileImages mobileOrderImagesAndFiles = new MobileImages(param, images);
            mobileOrderImagesAndFiles.AddMobileFiles(files);
            return mobileOrderImagesAndFiles;
        }

        private MobileImage PerformDeleteDocument(MobileParam param, int recordId, int documentId)
        {
            var result = new ActionResult(false);

            if (recordId == 0)
                return new MobileImage(param, Texts.RecordIdMissing);

            //Hängslen och livrem so app realy removes right stuff
            var dataStorageRecord = GeneralManager.GetDataStorageRecord(param.ActorCompanyId, documentId);
            if (dataStorageRecord != null && dataStorageRecord.RecordId == recordId)
            {
                result = GeneralManager.DeleteDataStorageRecord(param.ActorCompanyId, dataStorageRecord);
            }

            var mobileImage = new MobileImage(param, 0, null, "", 0, "");
            if (result.Success)
                mobileImage.SetTaskResult(MobileTask.DeleteOrderImage, true);
            else
                mobileImage = new MobileImage(param, FormatMessage(string.IsNullOrEmpty(result.ErrorMessage) ? Texts.FileNotFound : result.ErrorMessage, result.ErrorNumber));

            return mobileImage;
        }

        #endregion

        #region Image


        //Used to show images and files for order
        private MobileImages PerformGetOrderThumbNails(MobileParam param, int orderId)
        {
            var images = new List<Tuple<int, string, string, int, string, string>>();
            //Fetch 

            #region Images

            var imageDTOs = new List<ImagesDTO>();

            imageDTOs.AddRange(GraphicsManager.GetImages(param.ActorCompanyId, SoeEntityImageType.OrderInvoice, SoeEntityType.Order, orderId).ToDTOs(true).ToList());
            imageDTOs.AddRange(GraphicsManager.GetImages(param.ActorCompanyId, SoeEntityImageType.OrderInvoiceSignature, SoeEntityType.Order, orderId).ToDTOs(true).ToList());

            imageDTOs.AddRange(ChecklistManager.GetEntityChecklistsSignatures(param.ActorCompanyId, SoeEntityType.Order, orderId));

            #endregion

            #region Files

            //Show files only for apps that calls the WB with expected version 9 or higher
            List<ImagesDTO> fileDTOs = GeneralManager.GetDataStorageRecords(param.ActorCompanyId, param.RoleId, orderId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceSignature, SoeDataStorageRecordType.OrderInvoiceFileAttachment }, false, true).ToImagesDTOs(true, null, true, false).ToList();
            fileDTOs.AddRange(GraphicsManager.GetImagesFromOrderRows(base.ActorCompanyId, orderId, true));

            var imageFileExtensions = new List<string>() { ".png", ".jpg", ".jpeg" };
            var fileExtensions = new List<string>() { ".pdf" };

            if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_18))
            {
                fileExtensions.AddRange(new List<string>() { ".doc", ".docx", ".xls", ".xlsx", ".txt", ".xml" });
            }

            imageDTOs.AddRange(fileDTOs.Where(x => imageFileExtensions.Contains(Path.GetExtension(x.FileName).ToLower())).ToList());

            if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_29))
                fileDTOs = fileDTOs.Where(x => !imageFileExtensions.Contains(Path.GetExtension(x.FileName).ToLower())).ToList();
            else
                fileDTOs = fileDTOs.Where(x => fileExtensions.Contains(Path.GetExtension(x.FileName).ToLower())).ToList();

            foreach (var imageDTO in imageDTOs)
            {
                images.Add(GetSendImageProperties(param, imageDTO, false));
            }
            
            var files = new List<Tuple<int, string, string, int, string, bool>>();

            foreach (var fileDTO in fileDTOs)
            {
                string path = CreateFileOnServer(param, fileDTO.ImageId, fileDTO.FileName, fileDTO.Image);

                files.Add(Tuple.Create<int, string, string, int, string, bool>(fileDTO.ImageId, path, fileDTO.Description, (int)fileDTO.Type, Path.GetExtension(fileDTO.FileName), fileDTO.CanDelete));
            }

            //Add the images
            MobileImages mobileOrderImagesAndFiles = new MobileImages(param, images);
            mobileOrderImagesAndFiles.AddMobileFiles(files);

            #endregion

            return mobileOrderImagesAndFiles;
        }

        private MobileImages PerformGetOrderThumbNailsOnChecklist(MobileParam param, int orderId, int checklistHeadId, int checklistHeadRecordId)
        {
            List<Tuple<int, string, string, int, string, string>> images = new List<Tuple<int, string, string, int, string, string>>();

            //Fetch 
            #region Images

            var imageDTOs = new List<ImagesDTO>();

            /*
            ChecklistHeadRecord orderChecklist = ChecklistManager.GetChecklistHeadRecord(checklistHeadRecordId, param.ActorCompanyId);

            if (orderChecklist == null)
                return new MobileImages(param, Texts.ImageNotFound);

            imageDTOs.AddRange(GraphicsManager.GetImages(param.ActorCompanyId, SoeEntityImageType.ChecklistHeadRecordSignature, SoeEntityType.ChecklistHeadRecord, orderChecklist.ChecklistHeadRecordId).ToDTOs(true).ToList());
            imageDTOs.AddRange(GraphicsManager.GetImages(param.ActorCompanyId, SoeEntityImageType.ChecklistHeadRecordSignatureExecutor, SoeEntityType.ChecklistHeadRecord, orderChecklist.ChecklistHeadRecordId).ToDTOs(true).ToList());
            */

            imageDTOs.AddRange(ChecklistManager.GetEntityChecklistsSignatures(param.ActorCompanyId, SoeEntityType.Order, orderId));

            foreach (var imageDTO in imageDTOs)
            {
                images.Add(GetSendImageProperties(param, imageDTO, false));
            }

            #endregion

            //Add the images
            MobileImages mobileCheckListImages = new MobileImages(param, images);

            return mobileCheckListImages;
        }

        private MobileImage PerformGetImage(MobileParam param, int orderId, int imageId, bool isFile)
        {
            MobileImage mobileImage;

            if (!isFile)
            {
                ImagesDTO image = GraphicsManager.GetImage(imageId).ToDTO(false);

                if (image == null)
                {
                    image = GeneralManager.GetDataStorageRecord(param.ActorCompanyId, imageId, param.RoleId).ToImagesDTO(true);
                }

                if (image != null)
                {
                    Tuple<int, string, string, int, string, string> imageProperties = GetSendImageProperties(param, image, false);
                    mobileImage = new MobileImage(param, imageProperties.Item1, imageProperties.Item2, imageProperties.Item3, imageProperties.Item4, imageProperties.Item5, false, imageProperties.Item6);
                }
                else
                {
                    mobileImage = new MobileImage(param, Texts.ImageNotFound);
                }
            }
            else
            {
                ImagesDTO file = GeneralManager.GetDataStorageRecord(param.ActorCompanyId, imageId, param.RoleId).ToImagesDTO(true);
                if (file != null)
                {
                    string path = CreateFileOnServer(param, file.ImageId, file.FileName, file.Image);
                    mobileImage = new MobileImage(param, file.ImageId, path, file.Description, 0, Path.GetExtension(file.FileName), true);
                }
                else
                {
                    mobileImage = new MobileImage(param, GetText(8542, "Fil kunde inte hittas"));
                }
            }

            return mobileImage;
        }

        //Use AddDocument for future needs
        private MobileImage PerformAddImage(MobileParam param, int orderId, byte[] imageData, string description, int soeEntityImageType, string fileName)
        {

            if (orderId == 0)
                return new MobileImage(param, Texts.OrderNotFoundMessage);

            if (soeEntityImageType != (int)SoeEntityImageType.OrderInvoice && soeEntityImageType != (int)SoeEntityImageType.OrderInvoiceSignature)
                soeEntityImageType = (int)SoeEntityImageType.OrderInvoice;

            SoeDataStorageRecordType dataStorageRecordType = SoeDataStorageRecordType.OrderInvoiceFileAttachment;
            switch (soeEntityImageType)
            {
                case (int)SoeEntityImageType.OrderInvoice:
                    dataStorageRecordType = SoeDataStorageRecordType.OrderInvoiceFileAttachment;
                    break;
                case (int)SoeEntityImageType.OrderInvoiceSignature:
                    dataStorageRecordType = SoeDataStorageRecordType.OrderInvoiceSignature;
                    break;
            }

            fileName = string.IsNullOrEmpty(fileName) ? Guid.NewGuid().ToString() : fileName;
            if (!fileName.ToLower().EndsWith(".jpg"))
            {
                fileName += ".jpg";
            }

            var record = new DataStorageRecordExtendedDTO
            {
                Data = imageData,
                Type = dataStorageRecordType,
                Entity = SoeEntityType.None, //--None pga konstigt beslut för länge sedan
                Description = description,
                FileName = fileName,
                RecordId = orderId
            };

            if (soeEntityImageType == (int)SoeEntityImageType.OrderInvoiceSignature)
                record.Description = GetText(8401, "Ordersignatur") + " " + DateTime.Now.Date.ToShortDateString() + " " + record.Description;

            var result = InvoiceManager.SaveOrderInvoiceAttachment(orderId, record, param.ActorCompanyId);

            var mobileImage = new MobileImage(param, 0, null, "", 0, "");

            if (result.Success)
                mobileImage.SetTaskResult(MobileTask.AddOrderImage, true);
            else
                mobileImage = new MobileImage(param, FormatMessage(Texts.ImageNotSaved, result.ErrorNumber));

            return mobileImage;
        }

        private MobileImage PerformAddChecklistImage(MobileParam param, int orderId, int checklistHeadId, int checklistHeadRecordId, byte[] imageData, string description, int soeEntityImageType)
        {

            if (soeEntityImageType != (int)SoeEntityImageType.ChecklistHeadRecordSignature && soeEntityImageType != (int)SoeEntityImageType.ChecklistHeadRecordSignatureExecutor)
                return new MobileImage(param, "Error: wrong type, type kan only be 6 or 8"); //this should never happen in production

            var soeDataStorageType = soeEntityImageType == (int)SoeEntityImageType.ChecklistHeadRecordSignature ?
                                        SoeDataStorageRecordType.ChecklistHeadRecordSignature :
                                        SoeDataStorageRecordType.ChecklistHeadRecordSignatureExecutor;

            description = GetText(8402, "Checklistesignatur") + " " + DateTime.Now.Date.ToShortDateString() + " " + description;

            var result = ChecklistManager.SaveChecklistSignature(param.ActorCompanyId, orderId, checklistHeadRecordId, soeDataStorageType, imageData, description);

            /*
            ChecklistHeadRecord orderChecklist = ChecklistManager.GetChecklistHeadRecordWithCheckListHead(SoeEntityType.Order, orderId, checklistHeadRecordId, param.ActorCompanyId);
            if (orderChecklist == null)
                return new MobileImage(param, GetText(9074, "Checklista är ej kopplad till order"));

            ActionResult result = new ActionResult();

            SaveImagesDTO saveDTO = new SaveImagesDTO();
            saveDTO.RecordId = orderChecklist.ChecklistHeadRecordId;
            saveDTO.ActorCompanyId = param.ActorCompanyId;
            saveDTO.Type = (SoeEntityImageType)soeEntityImageType;
            saveDTO.Entity = SoeEntityType.ChecklistHeadRecord;

            var newImage = new ImagesDTO
            {
                Image = imageData != null ? imageData : null,
                FormatType = ImageFormatType.JPG,
                Description = description,
                Type = (SoeEntityImageType)soeEntityImageType,
            };

            saveDTO.NewImages = new List<ImagesDTO>();
            saveDTO.NewImages.Add(newImage);
            
            result = GraphicsManager.SaveImagesDTO(saveDTO);
            */

            MobileImage mobileImage = new MobileImage(param, 0, null, "", 0, "");
            if (result.Success)
                mobileImage.SetTaskResult(MobileTask.AddOrderImage, true);
            else
                mobileImage = new MobileImage(param, FormatMessage(Texts.ImageNotSaved, result.ErrorNumber));

            return mobileImage;
        }


        private MobileImage PerformDeleteImage(MobileParam param, int orderId, int imageId)
        {
            var result = new ActionResult(false);

            var image = GraphicsManager.GetImage(imageId);
            if (image != null && image.RecordId == orderId)
            {
                var saveDTO = new SaveImagesDTO
                {
                    RecordId = orderId,
                    ActorCompanyId = param.ActorCompanyId,
                    Entity = SoeEntityType.Order
                };
                saveDTO.DeletedImages = new List<int>();
                saveDTO.DeletedImages.Add(imageId);

                result = GraphicsManager.SaveImagesDTO(saveDTO);
            }
            else
            {
                //Hängslen och livrem so app realy removes right stuff
                var dataStorageRecord = GeneralManager.GetDataStorageRecord(param.ActorCompanyId, imageId);
                if (dataStorageRecord != null)
                {
                    if (
                            ((dataStorageRecord.Type == (int)SoeDataStorageRecordType.OrderInvoiceFileAttachment || dataStorageRecord.Type == (int)SoeDataStorageRecordType.OrderInvoiceSignature) && dataStorageRecord.RecordId == orderId) ||
                            (dataStorageRecord.Type == (int)SoeDataStorageRecordType.ChecklistHeadRecordSignature) ||
                            (dataStorageRecord.Type == (int)SoeDataStorageRecordType.ChecklistHeadRecordSignatureExecutor)
                        )
                    {
                        result = GeneralManager.DeleteDataStorageRecord(param.ActorCompanyId, dataStorageRecord);
                    }
                }
            }

            var mobileImage = new MobileImage(param, 0, null, "", 0, "");
            if (result.Success)
                mobileImage.SetTaskResult(MobileTask.DeleteOrderImage, true);
            else
                mobileImage = new MobileImage(param, FormatMessage(string.IsNullOrEmpty(result.ErrorMessage) ? Texts.ImageNotDeleted : result.ErrorMessage, result.ErrorNumber));

            return mobileImage;
        }

        private MobileImage PerformEditImage(MobileParam param, int orderId, int imageId, string description, string fileName)
        {
            if(!string.IsNullOrEmpty(fileName) && !fileName.ToLower().EndsWith(".jpg"))
            {
                fileName += ".jpg";
            }

            var result = GeneralManager.UpdateDataStorageByRecord(param.ActorCompanyId, imageId, orderId, description, fileName);

            //New way failed so try to find the image the old way
            if (!result.Success)
            {
                SaveImagesDTO saveDTO = new SaveImagesDTO();
                saveDTO.RecordId = orderId;
                saveDTO.ActorCompanyId = param.ActorCompanyId;
                //saveDTO.Type = SoeEntityImageType.OrderInvoice;
                saveDTO.Entity = SoeEntityType.Order;

                saveDTO.UpdatedDescriptions = new Dictionary<int, string>();
                saveDTO.UpdatedDescriptions.Add(imageId, description);

                result = GraphicsManager.SaveImagesDTO(saveDTO);
            }

            var mobileImage = new MobileImage(param, 0, null, "", 0, "");
            if (result.Success)
                mobileImage.SetTaskResult(MobileTask.EditOrderImage, true);
            else
                mobileImage = new MobileImage(param, FormatMessage(string.IsNullOrEmpty(result.ErrorMessage) ? Texts.ImageNotSaved : result.ErrorMessage, result.ErrorNumber));

            return mobileImage;
        }

        #endregion

        #region Checklists

        private MobileCheckLists PerformGetCheckLists(MobileParam param, int orderId)
        {
            //List<ChecklistHead> checkListHeads = new List<ChecklistHead>();

            //Checklists connected to a specific order
            List<ChecklistHeadRecord> checklistHeadRecords = ChecklistManager.GetChecklistHeadRecords(SoeEntityType.Order, orderId, param.ActorCompanyId, true);

            //foreach (var checklistHeadRecord in checklistHeadRecords)
            //{
            //    checkListHeads.Add(checklistHeadRecord.ChecklistHead);
            //}

            MobileCheckLists orderCheckLists = new MobileCheckLists(param, checklistHeadRecords);

            return orderCheckLists;
        }

        private MobileCheckLists PerformGetUnUsedCheckLists(MobileParam param, int orderId)
        {
            var availableChecklistHeads = ChecklistManager.GetChecklistHeadsForType(TermGroup_ChecklistHeadType.Order, param.ActorCompanyId, false);

            //Checklists connected to a specific order
            //List<ChecklistHeadRecord> connectedChecklistHeadRecords = ChecklistManager.GetChecklistHeadRecords(SoeEntityType.Order, orderId, param.ActorCompanyId, false);

            //foreach (var checklistHeadRecord in connectedChecklistHeadRecords)
            //{
            //    var checklist = availableChecklistHeads.Where(x => x.ChecklistHeadId == checklistHeadRecord.ChecklistHeadId).FirstOrDefault();
            //    if (checklist != null)
            //        availableChecklistHeads.Remove(checklist);
            //}

            MobileCheckLists orderCheckLists = new MobileCheckLists(param, availableChecklistHeads);

            return orderCheckLists;
        }


        private MobileCheckList PerformAddChecklist(MobileParam param, int orderId, string checklistHeadIds)
        {
            MobileCheckList mobileChecklist = new MobileCheckList(param);

            List<int> checklistIds = new List<int>();

            char[] separator = new char[1];
            separator[0] = ',';

            string[] separatedIds = checklistHeadIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var checklistHeadId in separatedIds)
            {
                if (Int32.TryParse(checklistHeadId.Trim(), out int id))
                    checklistIds.Add(id);
            }

            if (checklistIds.Count == 0)
                return new MobileCheckList(param, Texts.ChecklistNotSaved);


            ActionResult result = ChecklistManager.AddChecklistHeadRecords(SoeEntityType.Order, orderId, param.ActorCompanyId, checklistIds, true);

            if (result.Success)
                mobileChecklist.SetTaskResult(MobileTask.AddCheckList, true);
            else
                mobileChecklist = new MobileCheckList(param, Texts.ChecklistNotSaved);

            return mobileChecklist;
        }

        private MobileCheckList PerformDeleteOrderChecklists(MobileParam param, int orderId, string checklistHeadRecordIds)
        {
            MobileCheckList mobileChecklist = new MobileCheckList(param);

            List<int> checklistIds = new List<int>();

            char[] separator = new char[1];
            separator[0] = ',';

            string[] separatedIds = checklistHeadRecordIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var checklistHeadRecordId in separatedIds)
            {
                if (Int32.TryParse(checklistHeadRecordId.Trim(), out int id))
                    checklistIds.Add(id);
            }

            if (checklistIds.Count == 0)
                return new MobileCheckList(param, Texts.ChecklistNotSaved);

            ActionResult result = ChecklistManager.DeleteChecklistHeadRecords(SoeEntityType.Order, orderId, param.ActorCompanyId, checklistIds, param.UserId);

            if (result.Success)
                mobileChecklist.SetTaskResult(MobileTask.DeleteOrderChecklist, true);
            else
                mobileChecklist = new MobileCheckList(param, Texts.ChecklistNotSaved);

            return mobileChecklist;
        }

        private MobileCheckListRows PerformGetCheckListContent(MobileParam param, int orderId, int checkListHeadId, int checkListHeadRecordId)
        {
            ChecklistHeadRecord checklistHeadRecord = ChecklistManager.GetChecklistHeadRecord(checkListHeadRecordId, param.ActorCompanyId);
            List<ChecklistExtendedRowDTO> checklistRows = new List<ChecklistExtendedRowDTO>();

            if (checklistHeadRecord != null)
            {
                checklistRows = ChecklistManager.GetChecklistRows(checklistHeadRecord.ChecklistHeadId, SoeEntityType.Order, orderId, param.ActorCompanyId, checkListHeadRecordId).ToList();
            }

            MobileCheckListRows checklistContent = new MobileCheckListRows(param, checklistRows);
            return checklistContent;
        }

        private MobileCheckListRows PerformSaveCheckListAnswers(MobileParam param, int orderId, int checkListHeadId, int checkListHeadRecordId, string inputAnswers)
        {
            try
            {
                ChecklistHeadRecord checklistHeadRecord = ChecklistManager.GetChecklistHeadRecord(checkListHeadRecordId, param.ActorCompanyId);
                MobileCheckListRows mobileChecklistRows = new MobileCheckListRows(param);
                MobileCheckListAnswers mobileChecklistAnswers = new MobileCheckListAnswers(inputAnswers);

                //Fectch questions from db
                List<ChecklistExtendedRowDTO> checklistRows = ChecklistManager.GetChecklistRows(checklistHeadRecord.ChecklistHeadId, SoeEntityType.Order, orderId, param.ActorCompanyId, checkListHeadRecordId);

                //Update questions from answers
                foreach (MobileCheckListAnswer answer in mobileChecklistAnswers.ParsedAnswers)
                {
                    var question = checklistRows.FirstOrDefault(r => r.RowId == answer.CheckListRowId);
                    if (question == null)
                        return new MobileCheckListRows(param, Texts.SaveFailed);

                    question.Date = answer.Date;
                    question.Comment = answer.Comment;

                    #region Update Answer

                    if (question.Type == TermGroup_ChecklistRowType.MultipleChoice)
                    {
                        if (!String.IsNullOrEmpty(answer.Answer) && Int32.TryParse(answer.Answer, out int checklistMultipleChoiceAnswerRowId))
                        {
                            // Fetch the string value of the answer
                            var multiChoiceRow = ChecklistManager.GetChecklistMultipleChoiceAnswerRow(checklistMultipleChoiceAnswerRowId);

                            question.StrData = multiChoiceRow.Question;
                            question.IntData = checklistMultipleChoiceAnswerRowId;
                        }
                        else
                            question.IntData = null;
                    }
                    else
                    {
                        switch (question.DataTypeId)
                        {
                            case (int)SettingDataType.Boolean:
                                if (!String.IsNullOrEmpty(answer.Answer))
                                    question.BoolData = Boolean.Parse(answer.Answer);
                                else
                                    question.BoolData = null;
                                break;
                            case (int)SettingDataType.Date:
                            case (int)SettingDataType.Time:
                                if (!String.IsNullOrEmpty(answer.Answer))
                                    question.DateData = CalendarUtility.GetNullableDateTime(answer.Answer);
                                else
                                    question.DateData = null;
                                break;
                            case (int)SettingDataType.Decimal:
                                if (!String.IsNullOrEmpty(answer.Answer))
                                    question.DecimalData = NumberUtility.ToDecimal(answer.Answer, 0);
                                else
                                    question.DecimalData = null;
                                break;
                            case (int)SettingDataType.Integer:
                                int i = 0;
                                if (!String.IsNullOrEmpty(answer.Answer) && Int32.TryParse(answer.Answer, out i))
                                    question.IntData = i;
                                else
                                    question.IntData = null;
                                break;
                            case (int)SettingDataType.String:
                                question.StrData = answer.Answer;
                                break;
                            default:
                                break;
                        }
                    }

                    #endregion
                }

                //LogError("PerformSaveCheckListAnswers before save");
                ActionResult result = ChecklistManager.SaveChecklistRecords(checklistRows, SoeEntityType.Order, orderId, param.ActorCompanyId, true);
                if (result.Success)
                    mobileChecklistRows.SetTaskResult(MobileTask.SaveChecklistAnswers, true);
                else
                    mobileChecklistRows = new MobileCheckListRows(param, Texts.SaveFailed);

                return mobileChecklistRows;
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: PerformSaveCheckListAnswers failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: PerformSaveCheckListAnswers failed, innerexception: " + exp.InnerException.Message);

                return new MobileCheckListRows(param, Texts.SaveFailed + " : " + exp.Message);
            }
        }

        #endregion

        #region OrderUser

        private MobileOrderUsers PerformGetOrderUsers(MobileParam param, int orderId)
        {
            var selectedUsers = new List<OriginUser>();

            //Company Users
            var companyUsers = UserManager.GetUsersByCompany(param.ActorCompanyId, param.RoleId, param.UserId).OrderBy(u => u.Name).ToList();

            //Invoice Users
            Origin origin = OriginManager.GetOrigin(orderId, true);
            if (origin != null)
            {
                foreach (OriginUser originUser in origin.OriginUser.Where(o => o.State == (int)SoeEntityState.Active))
                {
                    selectedUsers.Add(originUser);
                }
            }

            return new MobileOrderUsers(param, selectedUsers, companyUsers);
        }

        private MobileOrderUsers PerformGetOrderCurrentUsers(MobileParam param, int orderId)
        {
            var users = InvoiceManager.GetOriginUsers(param.ActorCompanyId, orderId);

            return new MobileOrderUsers(param, users);
        }

        private MobileOrderUsers PerformSaveOrderUsers(MobileParam param, int orderId, String userids, int mainUserId, bool sendMail)
        {
            List<OriginUserDTO> selectedUsers = new List<OriginUserDTO>();
            ActionResult result = new ActionResult();
            char[] separator = new char[1];
            separator[0] = ',';

            string[] separatedUserids = userids.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var separatedUserid in separatedUserids)
            {
                if (Int32.TryParse(separatedUserid.Trim(), out int userId))
                {
                    selectedUsers.Add(new OriginUserDTO()
                    {
                        UserId = userId,
                        Main = (userId == mainUserId)
                    });
                }
            }

            License license = LicenseManager.GetLicenseByCompany(param.ActorCompanyId);
            if (license == null)
                return new MobileOrderUsers(param, Texts.LicenseNotFound);

            string subject = String.Empty;
            string text = String.Empty;
            if (sendMail)
            {
                var invoiceNr = InvoiceManager.GetInvoiceNr(orderId);
                if (string.IsNullOrEmpty(invoiceNr)) 
                { 
                    return new MobileOrderUsers(param, Texts.OrderNotFoundMessage);
                }

                subject = GetText(7715, "Tilldelad order");
                text = GetText(7716, "Du har blivit tilldelad order med nummer") + ": " + invoiceNr;
            }

            result = InvoiceManager.SaveOriginUsers(orderId, selectedUsers, param.ActorCompanyId, license.LicenseId, sendMail, subject, text);

            MobileOrderUsers mobileOrderUsers = new MobileOrderUsers(param);
            if (result.Success)
            {
                mobileOrderUsers.SetTaskResult(MobileTask.SaveOriginUsers, true);
            }
            else
            {
                if (result.ErrorNumber > 0)
                    mobileOrderUsers = new MobileOrderUsers(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    mobileOrderUsers = new MobileOrderUsers(param, FormatMessage(Texts.OriginUsersNotSaved, result.ErrorNumber));
            }

            return mobileOrderUsers;
        }

        #endregion

        #region MapLocation

        private MobileMapLocation PerformSaveMapLocation(MobileParam param, Decimal longitude, Decimal latitude, String description, DateTime timeStamp)
        {
            ActionResult result = new ActionResult();
            MobileMapLocation mobileMapLocation = new MobileMapLocation(param);

            result = GraphicsManager.SaveMapLocation(param.ActorCompanyId, param.UserId, MapLocationType.GPSLocation, SoeEntityType.User, longitude, latitude, description, timeStamp);

            if (result.Success)
                mobileMapLocation.SetTaskResult(MobileTask.SaveMapLocation, true);
            else
                mobileMapLocation = new MobileMapLocation(param, Texts.MapLocationNotSaved);

            return mobileMapLocation;
        }

        #endregion

        #region Project timecodes

        private MobileTimeCodes PerformGetTimeProjectTimeCodes(MobileParam param)
        {
            if (param == null)
                return new MobileTimeCodes(param, Texts.TimeCodesNotFoundMessage);

            //Fetch             
            Employee employee = EmployeeManager.GetEmployeeByUser(param.ActorCompanyId, param.UserId, loadEmployment: true);
            if (employee == null)
                return new MobileTimeCodes(param, Texts.EmployeeNotFoundMessage);

            int companySettingTimeCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, param.ActorCompanyId, 0);
            int defaultTimeCodeId = TimeCodeManager.GetDefaultTimeCodeId(employee, companySettingTimeCodeId);
            TimeCode defaultTimeCode = defaultTimeCodeId > 0 ? TimeCodeManager.GetTimeCode(defaultTimeCodeId, param.ActorCompanyId, true) : null;
            if (defaultTimeCode == null)
                return new MobileTimeCodes(param, GetText(8316, "Standard tidkod ej angiven"));

            List<TimeCode> timeCodes = TimeCodeManager.GetTimeCodesForEmployeeGroup(param.ActorCompanyId, employee.GetEmployeeGroupId()).OrderBy(x => x.Name).ToList();
            if (timeCodes.Any())
            {
                if (!timeCodes.Any(x => x.TimeCodeId == defaultTimeCode.TimeCodeId))
                    timeCodes.Add(defaultTimeCode);
            }
            else
            {
                var useExtendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, param.UserId, param.ActorCompanyId, 0, false);
                timeCodes = TimeCodeManager.GetTimeCodes(param.ActorCompanyId, useExtendedTimeRegistration ? SoeTimeCodeType.Work : SoeTimeCodeType.WorkAndAbsense, true, false, useExtendedTimeRegistration).OrderBy(x => x.Name).ToList();
            }

            return new MobileTimeCodes(param, timeCodes, defaultTimeCode);
        }

        private MobileTimeCodes PerformGetMaterialAndTimeTimeCodes(MobileParam param)
        {
            if (param == null)
                return new MobileTimeCodes(param, Texts.TimeCodesNotFoundMessage);

            int companySettingDefaultMaterialCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStandardMaterialCode, 0, param.ActorCompanyId, 0);

            List<TimeCode> timeCodes = TimeCodeManager.GetTimeCodes(param.ActorCompanyId, SoeTimeCodeType.WorkAndMaterial, true, false, false);
            TimeCode defaultTimeCode = companySettingDefaultMaterialCodeId > 0 ? timeCodes.FirstOrDefault(t => t.TimeCodeId == companySettingDefaultMaterialCodeId) : null;
            return new MobileTimeCodes(param, timeCodes, defaultTimeCode);
        }

        #endregion

        #region Project
        private MobileProjects PerformGetProjectsBySearch(MobileParam param, int customerId, string searchText)
        {
            if (customerId == 0)
                return new MobileProjects(param, GetText(8444, "Felaktig inparameter: kundid får ej vara 0"));

            var projects = ProjectManager.GetProjects(param.ActorCompanyId, param.RoleId, customerId, searchText, true);

            return new MobileProjects(param, projects);
        }

        private MobileProjects PerformGetProjectsBySearch2(MobileParam param, string number, string name, string customerNr, string customerName, string managerName, string orderNr, int customerId)
        {
            if (customerId == 0)
                return new MobileProjects(param, GetText(8444, "Felaktig inparameter: kundid får ej vara 0"));

            var projects = ProjectManager.GetProjectsBySearch2(number, name, customerNr, customerName, managerName, orderNr, true, false, true, false, customerId: customerId);

            return new MobileProjects(param, projects);
        }

        private MobileProject PerformChangeProjectOnOrder(MobileParam param, int orderId, int projectId)
        {
            MobileProject mobileProject = new MobileProject(param);

            ActionResult result = ProjectManager.ChangeProjectOnInvoice(param.ActorCompanyId, projectId, orderId, (int)SoeProjectRecordType.Order);

            if (result.Success)
                mobileProject.SetTaskResult(MobileTask.ChangeProjectOnOrder, true);
            else
            {
                string errorMsg = GetText(8445, "Projekt kunde inte bytas") + ": " + "\n";

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    errorMsg += result.ErrorMessage;

                mobileProject = new MobileProject(param, FormatMessage(errorMsg, result.ErrorNumber));
            }

            return mobileProject;
        }

        private MobilePreCreateOrder PerformPreCreateOrder(MobileParam param)
        {
            bool projectPermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_Edit, Permission.Modify, param.RoleId, param.ActorCompanyId);
            bool autoGenerateProject = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectAutoGenerateOnNewInvoice, 0, param.ActorCompanyId, 0);
            bool addTemplatePermission = FeatureManager.HasRolePermission(Feature.Billing_Preferences_InvoiceSettings_Templates, Permission.Modify, param.RoleId, param.ActorCompanyId);

            bool addProject = projectPermission && !autoGenerateProject;

            MobilePreCreateOrder preCreateOrder = new MobilePreCreateOrder(param, addProject, addTemplatePermission);

            return preCreateOrder;
        }

        #endregion

        #region SupplierInvoices AttestFlow

        private MobileAttestInvoices PerformGetSupplierInvoicesAttestWorkFlowMyActive(MobileParam param)
        {
            var items = SupplierInvoiceManager.GetAttestWorkFlowOverview(SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyActive, null);
            MobileAttestInvoices attestInvoices = new MobileAttestInvoices(param, items);

            return attestInvoices;
        }

        private MobileAttestInvoice PerformGetSupplierInvoiceAttestWorkFlowView(MobileParam param, int invoiceId)
        {
            List<AttestWorkFlowRowDTO> attestWorkFlowRows = new List<AttestWorkFlowRowDTO>();

            int attestWorkFlowHeadId = 0;
            int attestWorkFlowRowId = 0;
            string path = string.Empty;

            AttestWorkFlowHeadDTO attestWorkFlowHeadDTO = AttestManager.GetAttestWorkFlowHeadFromInvoiceId(invoiceId, false, false, false).ToDTO(false, false);
            List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(param.ActorCompanyId);
            var invoice = SupplierInvoiceManager.GetSupplierInvoice(invoiceId, false, false, false, false, false, true, false, false).ToSupplierInvoiceDTO(false, true, false, false, dims);

            if (attestWorkFlowHeadDTO != null)
                attestWorkFlowHeadId = attestWorkFlowHeadDTO.AttestWorkFlowHeadId;

            if (attestWorkFlowHeadId != 0)
                attestWorkFlowRows = AttestManager.GetAttestWorkFlowRowDTOs(attestWorkFlowHeadId, param.RoleId, param.UserId, true).Where(x => !x.IsDeleted).ToList();

            bool hasRejectedAttestFlowRow = (attestWorkFlowRows.Any(r => r.Answer.HasValue && !r.Answer.Value && r.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Processed && r.State == TermGroup_AttestFlowRowState.Handled));

            var changeAccountPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_AttestFlow_Overview_APP_ChangeAccount, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            var changeAccountInternalPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_AttestFlow_Overview_APP_ChangeInternalAccount, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            var finvoicePermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Finvoice, Permission.Modify, param.RoleId, param.ActorCompanyId);
            var linkToProjectPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Project, Permission.Modify, param.RoleId, param.ActorCompanyId);
            var linkToOrderPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_Order, Permission.Modify, param.RoleId, param.ActorCompanyId);

            if (finvoicePermission)
            {
                invoice.EdiEntryId = EdiManager.GetEdiEntryIdFromInvoice(invoiceId, param.ActorCompanyId, TermGroup_EDISourceType.Finvoice);
            }

            #region Decide row to attest

            if (!hasRejectedAttestFlowRow)
            {
                foreach (var attestWorkFlowRow in attestWorkFlowRows)
                {
                    bool showAnswerButton =

                       invoice.AttestStateId == attestWorkFlowRow.AttestStateFromId &&
                       attestWorkFlowRow.IsCurrentUser &&
                       (attestWorkFlowRow.ProcessType == TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess || attestWorkFlowRow.ProcessType == TermGroup_AttestWorkFlowRowProcessType.Returned) &&
                       !hasRejectedAttestFlowRow;

                    if (showAnswerButton)
                    {
                        attestWorkFlowRowId = attestWorkFlowRow.AttestWorkFlowRowId;
                        break;
                    }

                }
            }
            else
            {
                attestWorkFlowRowId = 0;
            }

            #endregion

            #region Decide imagePath
            GenericImageDTO image = SupplierInvoiceManager.GetSupplierInvoiceImage(param.ActorCompanyId, invoiceId);

            //if pdf its not already created....try generating it now
            if (image == null)
            {
                var ediEntry = EdiManager.GetEdiEntryFromInvoice(invoiceId, true);
                if (ediEntry != null)
                {
                    var result = ReportDataManager.GenerateReportForEdi(new List<int> { ediEntry.EdiEntryId }, param.ActorCompanyId);
                    if (result.Success)
                    {
                        image = SupplierInvoiceManager.GetSupplierInvoiceImage(param.ActorCompanyId, invoiceId);
                    }
                }
            }

            String fileType;
            if (image != null && (image.ImageFormatType == SoeDataStorageRecordType.InvoiceBitmap || image.ImageFormatType == SoeDataStorageRecordType.InvoicePdf))
            {
                if (image.ImageFormatType == SoeDataStorageRecordType.InvoiceBitmap)
                    fileType = ".jpg";
                else if (image.ImageFormatType == SoeDataStorageRecordType.InvoicePdf)
                    fileType = ".pdf";
                else
                    fileType = "";

                path = CreateFileOnServer(param, 0, image.Id + fileType, image.Image);
            }
            else
            {
                //image dosn't exists or type is not bitmap or pdf
                path = "";
                fileType = "";
            }

            #endregion

            MobileAttestInvoice mobileAttestInvoice = new MobileAttestInvoice(param, invoice, dims, attestWorkFlowHeadId, attestWorkFlowRowId, path, fileType, changeAccountPermission, changeAccountInternalPermission, linkToOrderPermission, linkToProjectPermission);

            return mobileAttestInvoice;
        }

        private MobileAttestInvoice PerformSaveAttestWorkFlowAnswer(MobileParam param, int invoiceId, int attestWorkFlowHeadId, int attestWorkFlowRowId, bool answer, string comment)
        {
            MobileAttestInvoice attestInvoice = new MobileAttestInvoice(param);

            #region PreReq

            if (invoiceId == 0)
                return new MobileAttestInvoice(param, GetText(8452, "Felaktig inparamter: orderid får inte vara 0"));

            if (attestWorkFlowRowId == 0)
                return new MobileAttestInvoice(param, GetText(8451, "Felaktig inparamter : attestradid får inte vara 0"));

            if (attestWorkFlowHeadId == 0)
                return new MobileAttestInvoice(param, GetText(8450, "Felaktig inparamter : attestFlödesId får inte vara 0"));

            var attestWorkFlowRows = AttestManager.GetAttestWorkFlowRowDTOs(attestWorkFlowHeadId, param.RoleId, param.UserId, true).Where(x => !x.IsDeleted).ToList();

            var rowToAttest = attestWorkFlowRows.FirstOrDefault(x => x.AttestWorkFlowRowId == attestWorkFlowRowId);

            if (rowToAttest == null)
                return new MobileAttestInvoice(param, GetText(8449, "Hittade ingen attestrad"));

            #endregion

            #region Perform

            ActionResult result = new ActionResult();

            if (answer) // user has approved invoice
            {
                // This user is temporary replacing another
                // We need to update the row with the new user
                if (rowToAttest.WorkFlowRowIdToReplace > 0)
                {
                    rowToAttest.Comment = comment;
                    result = ReplaceUserAndSaveAnswerToAttestFlow(param, AttestFlow_ReplaceUserReason.Remove, rowToAttest, true, comment, invoiceId);
                }
                else
                {
                    result = SaveAnswerToAttestFlow(param, rowToAttest.AttestWorkFlowRowId, true, comment);
                }
            }
            else // user has denied invoice
            {
                if (String.IsNullOrEmpty(comment))
                {
                    return new MobileAttestInvoice(param, GetText(8448, "Kan ej avslå, du måste ange kommentar"));
                }
                else
                {
                    if (rowToAttest.WorkFlowRowIdToReplace > 0)
                    {
                        rowToAttest.Comment = comment;
                        result = ReplaceUserAndSaveAnswerToAttestFlow(param, AttestFlow_ReplaceUserReason.Remove, rowToAttest, false, comment, invoiceId);
                    }
                    else
                    {
                        // Update AttestFlowRow with Answer (comment mandatory)
                        result = SaveAnswerToAttestFlow(param, rowToAttest.AttestWorkFlowRowId, false, comment);
                    }
                }
            }
            #endregion

            if (result.Success)
                attestInvoice.SetTaskResult(MobileTask.SaveAttestWorkFlowAnswer, true);
            else
            {
                string errorMsg = GetText(8447, "Kunde inte genomföra attest") + ": " + "\n";

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    errorMsg += result.ErrorMessage;

                attestInvoice = new MobileAttestInvoice(param, FormatMessage(errorMsg, result.ErrorNumber));
            }

            return attestInvoice;
        }

        private MobileAttestInvoiceCostTransferRows PerformGetSupplierInvoiceCostTransfers(MobileParam param, int invoiceId)
        {
            if (invoiceId == 0)
                return new MobileAttestInvoiceCostTransferRows(param, GetText(12069, "Felaktig inparamter: invoiceid får inte vara 0"));


            var rows = SupplierInvoiceManager.GetSupplierInvoiceCostTransfersForGrid(ActorCompanyId, param.RoleId, invoiceId);
            var attestInvoiceOrderProjectRows = new MobileAttestInvoiceCostTransferRows(param, rows);
            return attestInvoiceOrderProjectRows;
        }
        private MobileSupplierInvoiceCostTransfer PerformGetSupplierInvoiceCostTransfer(MobileParam param, int recordType, int recordId)
        {
            if (recordType == 0)
                return new MobileSupplierInvoiceCostTransfer(param, GetText(12070, "Felaktig inparamter: recordtype får inte vara 0"));
            if (recordId == 0)
                return new MobileSupplierInvoiceCostTransfer(param, GetText(12074, "Felaktig inparamter: recordid får inte vara 0"));


            var dto = SupplierInvoiceManager.GetSupplierInvoiceCostTransfer(ActorCompanyId, recordType, recordId);
            var supplierInvoiceCostTransfer = new MobileSupplierInvoiceCostTransfer(param, dto);
            return supplierInvoiceCostTransfer;
        }
        private MobileSupplierInvoiceCostTransfer PerformSaveSupplierInvoiceCostTransfer(MobileParam param, int supplierInvoiceId, int recordType, int recordId, int orderId, int projectId, int timeCodeId, int employeeId, decimal amount, decimal supplementCharge, bool chargeCostToProject, bool includeSupplierInvoiceImage, int state)
        {
            if (supplierInvoiceId == 0)
                return new MobileSupplierInvoiceCostTransfer(param, GetText(12069, "Felaktig inparamter: supplierInvoiceId får inte vara 0"));
            if (recordType == 0)
                return new MobileSupplierInvoiceCostTransfer(param, GetText(12070, "Felaktig inparamter: recordtype får inte vara 0"));
            if (recordType == (int)SupplierInvoiceCostLinkType.OrderRow && orderId == 0)
            {
                return new MobileSupplierInvoiceCostTransfer(param, GetText(12071, "Felaktig inparamter: orderid får inte vara 0"));
            }
            if (recordType == (int)SupplierInvoiceCostLinkType.ProjectRow)
            {
                if (projectId == 0)
                    return new MobileSupplierInvoiceCostTransfer(param, GetText(12072, "Felaktig inparamter: projectid får inte vara 0"));
                if (timeCodeId == 0)
                    return new MobileSupplierInvoiceCostTransfer(param, GetText(12073, "Ange kostnadsslag"));
            }
            var dto = new SupplierInvoiceCostTransferDTO()
            {
                Type = (SupplierInvoiceCostLinkType)recordType,
                RecordId = recordId,
                SupplierInvoiceId = supplierInvoiceId,
                CustomerInvoiceId = orderId,
                ProjectId = projectId,
                TimeCodeId = timeCodeId,
                EmployeeId = employeeId,
                AmountCurrency = amount,
                SupplementCharge = supplementCharge,
                SumAmountCurrency = supplementCharge != 0 ? amount * (1 + (supplementCharge / 100)) : amount,
                ChargeCostToProject = chargeCostToProject,
                IncludeSupplierInvoiceImage = includeSupplierInvoiceImage,
                State = (SoeEntityState)state,
            };
            var result = SupplierInvoiceManager.SaveSupplierInvoiceCostTransfer(param.ActorCompanyId, param.RoleId, dto);
            if (result.Success && result.IntegerValue > 0)
            {
                dto = SupplierInvoiceManager.GetSupplierInvoiceCostTransfer(param.ActorCompanyId, recordType, result.IntegerValue);
                return new MobileSupplierInvoiceCostTransfer(param, dto);
            }
            else if (result.Success)
            {
                return new MobileSupplierInvoiceCostTransfer(param, dto);
            }
            else
            {
                string errorMessage = result.ErrorMessage != String.Empty ? result.ErrorMessage : GetText(8455, "Internt fel");
                return new MobileSupplierInvoiceCostTransfer(param, errorMessage);
            }
        }

        private MobileResult PerformSupplierInvoiceBlockPayment(MobileParam param, int invoiceId, bool blockPayment, string comment)
        {
            try
            {
                if (blockPayment && String.IsNullOrEmpty(comment))
                    return new MobileResult(param, Texts.IncorrectInputMessage);

                var result = SupplierInvoiceManager.SupplierInvoiceSaveInvoiceTextAction(invoiceId, InvoiceTextType.SupplierInvoiceBlockReason, blockPayment, comment);
                return new MobileResult(param, result);
            }
            catch (Exception e)
            {
                LogError("PerformSupplierInvoiceBlockPayment: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileOrderRowsSearch PerformSearchOrder(MobileParam param, string orderNumber, string customerName, int projectId)
        {
            var dtos = InvoiceManager.GetOrdersBySearch(ActorCompanyId, orderNumber, customerName, projectId);
            var mobileOrderRowsSearch = new MobileOrderRowsSearch(param, dtos);
            return mobileOrderRowsSearch;
        }
        private MobileProjects PerformSearchProject(MobileParam param, string project, string customer, bool includeClosed)
        {
            var dtos = ProjectManager.GetProjectsBySearch(ActorCompanyId, project, customer, includeClosed);
            var mobileProjects = new MobileProjects(param, dtos);
            return mobileProjects;
        }

        private MobileDimAccounts PerformGetDimAccounts(MobileParam param, int dimNr)
        {
            var dtos = new List<AccountDimSmallDTO>();
            if (dimNr == 0)
            {
                var dims = AccountManager.GetAccountDimsByCompany(false, true, true, true, true);
                foreach (var dim in dims)
                {
                    dtos.Add(dim.ToSmallDTO(true, true, false));
                }
            }
            else
            {
                var dim = AccountManager.GetAccountDimByNr(dimNr, param.ActorCompanyId, true);
                if (dim == null)
                {
                    return new MobileDimAccounts(param, GetText(1279, "Konteringsnivå hittades inte"));
                }

                dtos.Add(dim.ToSmallDTO(true, true, false));
            }


            return new MobileDimAccounts(param, dtos);
        }

        private MobileResult PerformSaveSupplierAccountRow(MobileParam param, int invoiceId, int rowId, int dim1AccountId, int dim2AccountId, int dim3AccountId, int dim4AccountId, int dim5AccountId, int dim6AccountId, decimal debetAmount, decimal creditAmount)
        {

            var internalAccounts = new List<int>() { dim2AccountId, dim3AccountId, dim4AccountId, dim5AccountId, dim6AccountId }.Where(n => n > 0).ToList();
            var changeAccountPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_AttestFlow_Overview_APP_ChangeAccount, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            var changeAccountInternalPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Invoice_AttestFlow_Overview_APP_ChangeInternalAccount, Permission.Readonly, param.RoleId, param.ActorCompanyId);

            using (var entities = new CompEntities())
            {
                var supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, invoiceId, true);

                if (supplierInvoice == null)
                {
                    return new MobileResult(param, GetText(8371, "Faktura saknas"));
                }

                if (!supplierInvoice.IsDraftOrOrigin())
                {
                    return new MobileResult(param, GetText(4875, "Fakturan har ogiltig status"));
                }

                var supplierAccountingRow = SupplierInvoiceManager.GetSupplierInvoiceAccountRow(entities, rowId, true);

                if (supplierAccountingRow == null)
                {
                    return new MobileResult(param, GetText(1610, "Konteringsrad hittades inte"));
                }

                if (changeAccountPermission)
                {

                    if (dim1AccountId == 0)
                    {
                        return new MobileResult(param, GetText(1203, "Obligatoriska fältet kontonr saknas på transaktion"));
                    }

                    supplierAccountingRow.AccountId = dim1AccountId;

                }

                if (changeAccountInternalPermission)
                {
                    List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, param.ActorCompanyId, true);
                    supplierAccountingRow.AccountInternal.Clear();

                    foreach (var accountId in internalAccounts)
                    {
                        AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == accountId);
                        if (accountInternal == null)
                            return new MobileResult(param, GetText(4874, "Internkonto saknas"));
                        else
                            supplierAccountingRow.AccountInternal.Add(accountInternal);
                    }
                }

                var result = SaveChanges(entities);
                var mobileResult = result.Success ? new MobileResult(param) : new MobileResult(param, result);
                if (result.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.SaveAttestAccountRow, result.Success);
                }

                return mobileResult;
            }
        }

        #endregion

        #region Checklists

        private MobileMultipleChoiceAnswerRows PerformGetMultipleChoiceAnswerRows(MobileParam param, int multipleChoiceAnswerHeadId)
        {
            List<CheckListMultipleChoiceAnswerRow> answerRows = ChecklistManager.GetChecklistMultipleChoiceRows(multipleChoiceAnswerHeadId);

            MobileMultipleChoiceAnswerRows mobileAnswerRows = new MobileMultipleChoiceAnswerRows(param, answerRows);
            return mobileAnswerRows;
        }

        private MobileTextBlocks PerformGetTextBlockDictionary(MobileParam param, int dictionaryType)
        {
            if (dictionaryType != (int)TextBlockDictType.Task && dictionaryType != (int)TextBlockDictType.Where && dictionaryType != (int)TextBlockDictType.How)
                return new MobileTextBlocks(param, "Error: wrong type, type kan only be 1,2 or 3"); //this should never happen in production

            List<Textblock> dictionary = GeneralManager.GetTextblockDictionary(param.ActorCompanyId, (TextBlockDictType)dictionaryType);

            MobileTextBlocks mobileDictionary = new MobileTextBlocks(param, dictionary);
            return mobileDictionary;
        }

        #endregion

        #region TimeSheet

        private MobileTimeSheetRows PerformGetTimeSheetRows(MobileParam param, DateTime date, int employeeId)
        {
            DateTime firstDateInWeek = CalendarUtility.GetFirstDateOfWeek(date, offset: DayOfWeek.Monday);
            DateTime lastDateInWeek = CalendarUtility.GetLastDateOfWeek(date);

            bool invoicedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            bool useExtendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, param.ActorCompanyId, 0);

            var projectTimeBlocks = ProjectManager.LoadProjectTimeBlockForTimeSheet(firstDateInWeek, lastDateInWeek, employeeId, new List<int> { employeeId }, null, null, false, useExtendedTimeRegistration);
            var timeSheetDtos = ProjectManager.ProjectTimeBlocksDTOToTimeSheetDTO(projectTimeBlocks, firstDateInWeek);

            MobileTimeSheetRows timeSheets = new MobileTimeSheetRows(param, timeSheetDtos.Where(x => !x.IsDeleted).ToList(), invoicedTimePermission);
            return timeSheets;
        }

        private MobileTimeSheetInfo PerformGetTimeSheetInfo(MobileParam param, DateTime date, int employeeId)
        {
            DateTime firstDateInWeek = CalendarUtility.GetFirstDateOfWeek(date, offset: DayOfWeek.Monday);
            DateTime lastDateInWeek = CalendarUtility.GetLastDateOfWeek(date);

            bool invoicedTimePermission = FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_InvoicedTime, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            bool useExtendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, param.ActorCompanyId, 0);

            TimeSheetScheduleDTO timeSheetScheduleDto = ProjectManager.LoadTimeSheetSchedule(employeeId, firstDateInWeek, lastDateInWeek);
            var projectTimeBlocks = ProjectManager.LoadProjectTimeBlockForTimeSheet(firstDateInWeek, lastDateInWeek, employeeId, new List<int> { employeeId }, null, null, false, useExtendedTimeRegistration);
            var timeSheetDtos = ProjectManager.ProjectTimeBlocksDTOToTimeSheetDTO(projectTimeBlocks, firstDateInWeek);

            bool showExpenses = FeatureManager.HasRolePermission(Feature.Time_Time_AttestUser_AdditionAndDeduction, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            int expensesCount = 0;
            if (showExpenses)
                expensesCount = ExpenseManager.GetExpenseRowsCount(param.ActorCompanyId, employeeId, CalendarUtility.GetBeginningOfDay(firstDateInWeek), CalendarUtility.GetEndOfDay(lastDateInWeek));

            MobileTimeSheetInfo timeSheets = new MobileTimeSheetInfo(param, timeSheetScheduleDto, timeSheetDtos.Where(x => !x.IsDeleted).ToList(), invoicedTimePermission, showExpenses, expensesCount);
            return timeSheets;
        }

        #endregion

        #region Accounting on orderrow (only Professional)

        private MobileAccountSettings PerfromGetAccountSettings(MobileParam param)
        {
            return new MobileAccountSettings(param);
        }

        private MobileAccounts PerfromGetAccounts(MobileParam param)
        {
            return new MobileAccounts(param);
        }

        private MobileAccounts PerfromGetOrderRowAccounts(MobileParam param)
        {
            return new MobileAccounts(param);
        }

        #endregion

        #endregion

        #region Time

        #region Employee

        private MobileEmployee PerformGetEmployee(MobileParam param)
        {
            if (param == null)
                return new MobileEmployee(param, Texts.EmployeeNotFoundMessage);

            Employee employee = EmployeeManager.GetEmployeeByUser(param.ActorCompanyId, param.UserId, loadContactPerson: true, loadEmployment: true);

            if (employee == null)
                return new MobileEmployee(param, Texts.EmployeeNotFoundMessage);

            if (!employee.ContactPersonReference.IsLoaded)
                employee.ContactPersonReference.Load();

            return new MobileEmployee(param, employee);
        }

        private MobileDicts PerformGetAttestTreeEmployeesForAdmin(MobileParam param, DateTime dateFrom, DateTime dateTo, bool includeAdditionalEmployees, bool includeIsAttested, string employeeIds)
        {
            try
            {
                TimeEmployeeTreeSettings settings = new TimeEmployeeTreeSettings();
                settings.IncludeAdditionalEmployees = includeAdditionalEmployees;
                settings.LoadMode = includeIsAttested ? SoeAttestTreeLoadMode.OnlyEmployeesAndIsAttested : SoeAttestTreeLoadMode.OnlyEmployees;

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                List<Employee> employees = TimeTreeAttestManager.GetAttestTreeEmployees(entitiesReadOnly, dateFrom, dateTo, null, settings: settings).OrderBy(x => x.FirstName).ToList();
                MobileDicts mobileDicts = new MobileDicts(param, employees.ToDictionary(x => x.EmployeeId, x => x.EmployeeNrAndName));
                if (includeIsAttested)
                    mobileDicts.AddProperties("IA", employees.ToDictionary(x => x.EmployeeId, x => x.IsAttested));
                if (includeAdditionalEmployees)
                    mobileDicts.AddProperties("AACC", employees.Where(i => i.AdditionalOnAccountIds != null && i.AdditionalOnAccountIds.Any()).ToDictionary(x => x.EmployeeId, x => x.AdditionalOnAccountIds.ToCommaSeparated()));
                mobileDicts.SetSelectedIds(GetIds(employeeIds));
                return mobileDicts;
            }
            catch (Exception e)
            {
                LogError("PerformGetAttestTreeEmployeesForAdmin: " + e.Message);
                return new MobileDicts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion

        #region DeviationCause

        private MobileDeviationCauses PerformGetDeviationCauses(MobileParam param, int employeeId, int employeeGroupId)
        {
            if (param == null)
                return new MobileDeviationCauses(param, Texts.DeviationCausesNotFoundMessage);

            Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId);
            if (employee == null)
                return new MobileDeviationCauses(param, Texts.EmployeeNotFoundMessage);

            Employee userEmployee = EmployeeManager.GetEmployeeForUser(param.UserId, param.ActorCompanyId);
            bool isMySelf = employee.IsMySelf(userEmployee);

            List<TimeDeviationCause> deviationCauses = TimeDeviationCauseManager.GetTimeDeviationCausesByEmployeeGroup(param.ActorCompanyId, employeeGroupId, onlyUseInTimeTerminal: isMySelf, setTimeDeviationTypeName: true).ToList();
            return new MobileDeviationCauses(param, deviationCauses);
        }

        #endregion

        #region My Time

        private bool HasOwnAbsencePermission(MobileParam param, int employeeId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int currentUserId = UserManager.GetUserIdByEmployeeId(entitiesReadOnly, employeeId, param.ActorCompanyId);
            if (currentUserId == param.UserId && !FeatureManager.HasRolePermission(Feature.Time_Time_AttestUser_EditAbsence, Permission.Modify, param.RoleId, param.ActorCompanyId))
                return false;
            return true;
        }

        private MobileAttestEmployeeDays PerformSaveWholeDayAbsence(MobileParam param, string datesAsString, int deviationCauseId, int employeeId, int employeeChildId, string comment)
        {
            try
            {
                if (param == null)
                    return new MobileAttestEmployeeDays(param, Texts.AbsenceNotSavedMessage);

                // Check permission
                if (!HasOwnAbsencePermission(param, employeeId))
                    return new MobileAttestEmployeeDays(param, Texts.NoPermissionForOwnAbsenceMessage);

                //temp fix for bessmanet
                // TODO: Could be replaced by new permission above?
                if (param.ActorCompanyId == 30449)
                    return new MobileAttestEmployeeDays(param, "Ej tillåtet att rapportera heldagsfrånvaro.");

                if (deviationCauseId == 0)
                    return new MobileAttestEmployeeDays(param, GetText(8544, "Orsak måste anges"));

                var deviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(deviationCauseId, param.ActorCompanyId, false);
                if (deviationCause == null)
                    return new MobileAttestEmployeeDays(param, GetText(8813, "Orsak kunde inte hittas"));

                if (deviationCause.SpecifyChild && employeeChildId <= 0)
                    return new MobileAttestEmployeeDays(param, GetText(8814, "Du måste ange barn"));

                if (deviationCause.MandatoryNote && string.IsNullOrEmpty(comment))
                    return new MobileAttestEmployeeDays(param, Texts.DeviationCauseMandatoryNoteMessage);


                #region Prereq

                List<DateTime> dates = new List<DateTime>();

                char[] separator = new char[1];
                separator[0] = ',';

                string[] separatedStringDates = datesAsString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var dateAsString in separatedStringDates)
                {
                    DateTime? date = CalendarUtility.GetNullableDateTime(dateAsString.Trim());
                    if (date.HasValue)
                        dates.Add(date.Value);
                }

                if (dates.Count == 0)
                    return new MobileAttestEmployeeDays(param, Texts.AbsenceNotSavedMessage);

                List<TimeBlockDTO> timeBlocks = GetTimeBlocksFromDates(dates);

                #endregion

                #region Perform

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveWholedayDeviations(timeBlocks, comment ?? string.Empty, deviationCauseId, deviationCauseId, employeeChildId.ToNullable(), employeeId);

                #endregion

                //Set result
                MobileAttestEmployeeDays mobileAttestEmployeeDays = null;
                if (result.Success)
                {
                    mobileAttestEmployeeDays = new MobileAttestEmployeeDays(param, new List<AttestEmployeeDayDTO>());
                    mobileAttestEmployeeDays.SetTaskResult(MobileTask.SaveAbsence, true);
                }
                else
                    mobileAttestEmployeeDays = new MobileAttestEmployeeDays(param, FormatMessage(Texts.AbsenceNotSavedMessage, result.ErrorNumber));

                return mobileAttestEmployeeDays;
            }
            catch (Exception e)
            {
                LogError("PerformSaveWholeDayAbsence: " + e.Message);
                return new MobileAttestEmployeeDays(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #region Deprecated since version 23(These should not be maintained unless absolutely necessary)

        private MobileAttestEmployeeDays PerformSaveIntervalAbsenceDeprecated(MobileParam param, DateTime start, DateTime stop, DateTime displayedDate, int displayedTimeScheduleTemplatePeriodId, int deviationCauseId, int employeeId, string comment, int employeeChildId)
        {
            try
            {
                if (param == null)
                    return new MobileAttestEmployeeDays(param, Texts.AbsenceNotSavedMessage);

                if (stop < start)
                    return new MobileAttestEmployeeDays(param, GetText(8828, "Sluttiden är före starttiden"));

                // Check permission
                if (!HasOwnAbsencePermission(param, employeeId))
                    return new MobileAttestEmployeeDays(param, Texts.NoPermissionForOwnAbsenceMessage);

                #region Perform

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).GenerateDeviationsFromInterval(start, stop, displayedDate, comment ?? string.Empty, displayedTimeScheduleTemplatePeriodId, deviationCauseId, deviationCauseId, employeeChildId.ToNullable(), employeeId, TermGroup_TimeDeviationCauseType.Absence);

                #endregion

                //Set result
                MobileAttestEmployeeDays mobileAttestEmployeeDays = null;
                if (result.Success)
                {
                    mobileAttestEmployeeDays = new MobileAttestEmployeeDays(param, new List<AttestEmployeeDayDTO>());
                    mobileAttestEmployeeDays.SetTaskResult(MobileTask.SaveAbsence, true);
                }
                else
                    mobileAttestEmployeeDays = new MobileAttestEmployeeDays(param, FormatMessage(Texts.AbsenceNotSavedMessage, result.ErrorNumber));

                return mobileAttestEmployeeDays;
            }
            catch (Exception e)
            {
                LogError("PerformSaveIntervalAbsenceDeprecated: " + e.Message);
                return new MobileAttestEmployeeDays(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileAttestEmployeeDays PerformSaveIntervalPresenceDeprecated(MobileParam param, DateTime start, DateTime stop, DateTime displayedDate, int displayedTimeScheduleTemplatePeriodId, int deviationCauseId, int employeeId, string comment)
        {
            try
            {
                if (param == null)
                    return new MobileAttestEmployeeDays(param, Texts.PresenceNotSavedMessage);

                if (stop < start)
                    return new MobileAttestEmployeeDays(param, GetText(8828, "Sluttiden är före starttiden"));

                #region Perform

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).GenerateDeviationsFromInterval(start, stop, displayedDate, comment ?? string.Empty, displayedTimeScheduleTemplatePeriodId, deviationCauseId, deviationCauseId, null, employeeId, TermGroup_TimeDeviationCauseType.Presence);

                #endregion

                //Set result
                MobileAttestEmployeeDays mobileAttestEmployeeDays = null;
                if (result.Success)
                {
                    TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateDeviationsAgainstWorkRules(employeeId, displayedDate);

                    mobileAttestEmployeeDays = new MobileAttestEmployeeDays(param, new List<AttestEmployeeDayDTO>());
                    mobileAttestEmployeeDays.SetTaskResult(MobileTask.SavePresence, true);
                }
                else
                    mobileAttestEmployeeDays = new MobileAttestEmployeeDays(param, FormatMessage(Texts.PresenceNotSavedMessage, result.ErrorNumber));

                return mobileAttestEmployeeDays;
            }
            catch (Exception e)
            {
                LogError("PerformSaveIntervalPresenceDeprecated: " + e.Message);
                return new MobileAttestEmployeeDays(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileDayView PerformSaveStartTimeDeprecated(MobileParam param, DateTime newStartTime, DateTime date, int timeScheduleTemplatePeriodId, int newDeviationCauseId, int timeBlockId, int employeeId, string comment, int employeeChildId)
        {
            try
            {
                #region Prereq

                TimeBlock originalTimeBlock = TimeBlockManager.GetTimeBlockDiscardState(timeBlockId);
                if (originalTimeBlock == null || originalTimeBlock.State != (int)SoeEntityState.Active)
                    return new MobileDayView(param, FormatMessage(Texts.DeviationsNotSavedMessage, 0));

                List<TimeBlock> timeBlocks = new List<TimeBlock>();
                TimeBlock newTimeBlock = new TimeBlock();
                DateTime start = CalendarUtility.DATETIME_DEFAULT;
                DateTime stop = CalendarUtility.DATETIME_DEFAULT;
                TimeDeviationCause newDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(newDeviationCauseId, param.ActorCompanyId, false);
                if (newDeviationCause == null)
                    return new MobileDayView(param, GetText(8813, "Orsak kunde inte hittas"));

                if (newDeviationCause.SpecifyChild && employeeChildId <= 0)
                    return new MobileDayView(param, GetText(8814, "Du måste ange barn"));

                #region Schedule

                //TimeScheduleTemplatePeriod
                TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = TimeEngineManager(param.ActorCompanyId, param.UserId).GetSequentialSchedule(date, timeScheduleTemplatePeriodId, employeeId, true);
                if (timeScheduleTemplatePeriod == null)
                    return new MobileDayView(param, Texts.DeviationsNotSavedMessage);

                //TimeScheduleTemplateBlock
                List<TimeScheduleTemplateBlock> scheduleBlocks = timeScheduleTemplatePeriod.TimeScheduleTemplateBlock.OrderBy(t => t.StartTime).ToList();
                TimeScheduleTemplateBlock scheduleStart = scheduleBlocks.FirstOrDefault();

                //Schedule in
                DateTime actualScheduleIn = CalendarUtility.DATETIME_DEFAULT;
                if (scheduleStart != null)
                    actualScheduleIn = date + scheduleStart.StartTime.TimeOfDay;

                #endregion

                newTimeBlock.TimeDeviationCauseStartId = newDeviationCauseId;
                newTimeBlock.TimeDeviationCauseStopId = newDeviationCauseId;
                newTimeBlock.Comment = comment ?? string.Empty;
                newTimeBlock.EmployeeChildId = employeeChildId.ToNullable();

                if (newStartTime > actualScheduleIn)
                {
                    //User has arrived to work later then schedule, only absence blocks can be created

                    newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(actualScheduleIn);
                    newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(newStartTime, actualScheduleIn, newStartTime.Date);
                    originalTimeBlock.StartTime = newTimeBlock.StopTime;
                    originalTimeBlock.Comment = comment ?? string.Empty;

                    if (newDeviationCause.Type != (int)TermGroup_TimeDeviationCauseType.Absence)
                        newTimeBlock.CreateAsBlank = true;

                    if (originalTimeBlock.StartTime > originalTimeBlock.StopTime)
                    {
                        originalTimeBlock.StopTime = originalTimeBlock.StartTime;
                        ChangeEntityState(originalTimeBlock, SoeEntityState.Deleted);
                    }
                }
                else
                {
                    newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(newStartTime);
                    newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(actualScheduleIn, newStartTime.Date, actualScheduleIn.Date);
                    originalTimeBlock.StartTime = newTimeBlock.StopTime;
                }

                timeBlocks.Add(newTimeBlock);
                timeBlocks.Add(originalTimeBlock);

                #endregion

                #region Perform

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).AddModifyTimeBlocks(timeBlocks, date, timeScheduleTemplatePeriodId, employeeId, newDeviationCauseId);

                #endregion

                //Set result
                MobileDayView dayview = null;
                if (result.Success)
                {
                    if (!newDeviationCause.ExcludeFromPresenceWorkRules)
                        TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateDeviationsAgainstWorkRules(employeeId, date);

                    dayview = new MobileDayView(param);
                    dayview.SetTaskResult(MobileTask.SaveDeviations, true);
                }
                else
                    dayview = new MobileDayView(param, FormatMessage(Texts.DeviationsNotSavedMessage, result.ErrorNumber));

                return dayview;

            }
            catch (Exception e)
            {
                LogError("PerformSaveStartTimeDeprecated: " + e.Message);
                return new MobileDayView(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileDayView PerformSaveStopTimeDeprecated(MobileParam param, DateTime newStopTime, DateTime date, int timeScheduleTemplatePeriodId, int newDeviationCauseId, int timeBlockId, int employeeId, string comment, int employeeChildId)
        {
            try
            {
                #region Prereq

                TimeBlock originalTimeBlock = TimeBlockManager.GetTimeBlockDiscardState(timeBlockId);
                if (originalTimeBlock == null || originalTimeBlock.State != (int)SoeEntityState.Active)
                    return new MobileDayView(param, FormatMessage(Texts.DeviationsNotSavedMessage, 0));

                List<TimeBlock> timeBlocks = new List<TimeBlock>();
                TimeBlock newTimeBlock = new TimeBlock();
                DateTime start = CalendarUtility.DATETIME_DEFAULT;
                DateTime stop = CalendarUtility.DATETIME_DEFAULT;
                TimeDeviationCause newDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(newDeviationCauseId, param.ActorCompanyId, false);
                if (newDeviationCause == null)
                    return new MobileDayView(param, GetText(8813, "Orsak kunde inte hittas"));

                if (newDeviationCause.SpecifyChild && employeeChildId <= 0)
                    return new MobileDayView(param, GetText(8814, "Du måste ange barn"));

                #region Schedule

                //TimeScheduleTemplatePeriod
                TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = TimeEngineManager(param.ActorCompanyId, param.UserId).GetSequentialSchedule(date, timeScheduleTemplatePeriodId, employeeId, true);
                if (timeScheduleTemplatePeriod == null)
                    return new MobileDayView(param, Texts.AttestEmployeeDayNotFoundMessage);

                //TimeScheduleTemplateBlock
                List<TimeScheduleTemplateBlock> scheduleBlocks = timeScheduleTemplatePeriod.TimeScheduleTemplateBlock.OrderBy(t => t.StartTime).ToList();
                TimeScheduleTemplateBlock scheduleStop = scheduleBlocks.LastOrDefault();

                //Schedule out
                DateTime actualScheduleOut = CalendarUtility.DATETIME_DEFAULT;
                if (scheduleStop != null)
                    actualScheduleOut = date.AddDays((scheduleStop.StopTime - CalendarUtility.DATETIME_DEFAULT.Date).Days) + scheduleStop.StopTime.TimeOfDay;

                #endregion

                newTimeBlock.TimeDeviationCauseStartId = newDeviationCauseId;
                newTimeBlock.TimeDeviationCauseStopId = newDeviationCauseId;
                newTimeBlock.Comment = comment ?? string.Empty;
                newTimeBlock.EmployeeChildId = employeeChildId.ToNullable();

                if (newStopTime < actualScheduleOut)
                {
                    // User has left work earlier then scheduled, only absence blocks should be created

                    newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(newStopTime).AddDays((newStopTime.Date - date.Date).Days);
                    //newTimeBlock.StopTime = CalendarUtility.GetStopScheduleTime(actualScheduleOut, newStopTime.Date, actualScheduleOut.Date);
                    newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(actualScheduleOut).AddDays((actualScheduleOut.Date - date.Date).Days);

                    if (newDeviationCause.Type != (int)TermGroup_TimeDeviationCauseType.Absence)
                        newTimeBlock.CreateAsBlank = true;

                    originalTimeBlock.StopTime = newTimeBlock.StartTime;
                    originalTimeBlock.Comment = comment ?? string.Empty;
                    if (originalTimeBlock.StopTime < originalTimeBlock.StartTime)
                    {
                        originalTimeBlock.StartTime = originalTimeBlock.StopTime;
                        ChangeEntityState(originalTimeBlock, SoeEntityState.Deleted);
                    }
                }
                else
                {
                    newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(actualScheduleOut).AddDays((actualScheduleOut.Date - date.Date).Days);
                    //newTimeBlock.StopTime = CalendarUtility.GetStopScheduleTime(newStopTime, actualScheduleOut.Date, newStopTime.Date);
                    newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(newStopTime).AddDays((newStopTime.Date - date.Date).Days);

                    originalTimeBlock.StopTime = newTimeBlock.StartTime;
                }

                timeBlocks.Add(newTimeBlock);
                timeBlocks.Add(originalTimeBlock);

                #endregion

                #region Perform

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).AddModifyTimeBlocks(timeBlocks, date, timeScheduleTemplatePeriodId, employeeId, newDeviationCauseId);

                #endregion

                //Set result
                MobileDayView dayview = null;
                if (result.Success)
                {
                    if (!newDeviationCause.ExcludeFromPresenceWorkRules)
                        TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateDeviationsAgainstWorkRules(employeeId, date);

                    dayview = new MobileDayView(param);
                    dayview.SetTaskResult(MobileTask.SaveDeviations, true);
                }
                else
                    dayview = new MobileDayView(param, FormatMessage(Texts.DeviationsNotSavedMessage, result.ErrorNumber));

                return dayview;
            }
            catch (Exception e)
            {
                LogError("PerformSaveStopTimeDeprecated: " + e.Message);
                return new MobileDayView(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion

        private MobileResult PerformSaveIntervalAbsence(MobileParam param, DateTime start, DateTime stop, DateTime displayedDate, int displayedTimeScheduleTemplatePeriodId, int deviationCauseId, int employeeId, string comment, int employeeChildId)//NOSONAR
        {
            try
            {
                if (param == null)
                    return new MobileResult(param, Texts.AbsenceNotSavedMessage);

                if (stop < start)
                    return new MobileResult(param, GetText(8828, "Sluttiden är före starttiden"));

                var deviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(deviationCauseId, param.ActorCompanyId, false);
                if (deviationCause == null)
                    return new MobileResult(param, GetText(8813, "Orsak kunde inte hittas"));

                if (deviationCause.MandatoryNote && string.IsNullOrEmpty(comment))
                    return new MobileResult(param, Texts.DeviationCauseMandatoryNoteMessage);

                // Check permission
                if (!HasOwnAbsencePermission(param, employeeId))
                    return new MobileResult(param, Texts.NoPermissionForOwnAbsenceMessage);

                #region Perform

                //ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).GenerateDeviationsFromInterval(start, stop, displayedDate, comment ?? string.Empty, displayedTimeScheduleTemplatePeriodId, deviationCauseId, deviationCauseId, employeeChildId.ToNullable(), employeeId, TermGroup_TimeDeviationCauseType.Absence);

                ExtendedAbsenceSetting extendedAbsenceSettings = new ExtendedAbsenceSetting()
                {
                    AbsenceFirstAndLastDay = true,
                    AbsenceFirstDayStart = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, start),
                    AbsenceLastDayStart = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, stop),
                };

                var affectedShifts = TimeScheduleManager.GetAbsenceAffectedShifts(param.ActorCompanyId, param.UserId, employeeId, null, CalendarUtility.GetBeginningOfDay(start), CalendarUtility.GetEndOfDay(stop), deviationCauseId, extendedAbsenceSettings, true).ToList();
                affectedShifts = affectedShifts.Where(x => !x.IsLended).ToList();
                foreach (var affectedShift in affectedShifts)
                {
                    affectedShift.ApprovalTypeId = (int)TermGroup_YesNo.Yes;
                    affectedShift.EmployeeId = Constants.NO_REPLACEMENT_EMPLOYEEID;
                }

                //Save shifts
                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).GenerateAndSaveAbsenceFromStaffing(new EmployeeRequestDTO(employeeId, deviationCauseId, employeeChildId, comment), affectedShifts, true, false, null);

                #endregion

                //Set result
                MobileResult mobileAttestEmployeeDays = null;
                if (result.Success)
                {
                    mobileAttestEmployeeDays = new MobileResult(param);
                }
                else
                    mobileAttestEmployeeDays = new MobileResult(param, FormatMessage(Texts.AbsenceNotSavedMessage, result.ErrorNumber));

                return mobileAttestEmployeeDays;
            }
            catch (Exception e)
            {
                LogError("PerformSaveIntervalAbsence: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileResult PerformSaveIntervalPresence(MobileParam param, DateTime start, DateTime stop, DateTime displayedDate, int displayedTimeScheduleTemplatePeriodId, int deviationCauseId, int employeeId, string comment)
        {
            try
            {
                if (param == null)
                    return new MobileResult(param, Texts.PresenceNotSavedMessage);

                if (stop < start)
                    return new MobileResult(param, GetText(8828, "Sluttiden är före starttiden"));

                var deviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(deviationCauseId, param.ActorCompanyId, false);
                if (deviationCause == null)
                    return new MobileResult(param, GetText(8813, "Orsak kunde inte hittas"));

                if (deviationCause.MandatoryNote && string.IsNullOrEmpty(comment))
                    return new MobileResult(param, Texts.DeviationCauseMandatoryNoteMessage);

                #region Perform

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).GenerateDeviationsFromInterval(start, stop, displayedDate, comment ?? string.Empty, displayedTimeScheduleTemplatePeriodId, deviationCauseId, deviationCauseId, null, employeeId, TermGroup_TimeDeviationCauseType.Presence);

                #endregion

                //Set result
                MobileResult mobileResult = null;
                if (result.Success)
                {
                    mobileResult = new MobileResult(param);
                    if (!deviationCause.ExcludeFromPresenceWorkRules)
                    {
                        EvaluateDeviationsAgainstWorkRules evaluateDeviationsAgainstWorkRulesResult = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateDeviationsAgainstWorkRules(employeeId, displayedDate);
                        if (!string.IsNullOrEmpty(evaluateDeviationsAgainstWorkRulesResult.InfoMessage))
                            mobileResult.SetSuccessMessage(evaluateDeviationsAgainstWorkRulesResult.InfoMessage);
                    }
                }
                else
                    mobileResult = new MobileResult(param, FormatMessage(Texts.PresenceNotSavedMessage, result.ErrorNumber));

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformSaveIntervalPresence: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileResult PerformSaveStartTime(MobileParam param, DateTime newStartTime, DateTime date, int timeScheduleTemplatePeriodId, int newDeviationCauseId, int timeBlockId, int employeeId, string comment, int employeeChildId)
        {
            try
            {
                #region Prereq

                TimeBlock originalTimeBlock = TimeBlockManager.GetTimeBlockDiscardState(timeBlockId);
                if (originalTimeBlock == null || originalTimeBlock.State != (int)SoeEntityState.Active)
                    return new MobileResult(param, FormatMessage(Texts.DeviationsNotSavedMessage, 0));

                List<TimeBlock> timeBlocks = new List<TimeBlock>();
                TimeBlock newTimeBlock = new TimeBlock();
                TimeDeviationCause newDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(newDeviationCauseId, param.ActorCompanyId, false);
                if (newDeviationCause == null)
                    return new MobileResult(param, GetText(8813, "Orsak kunde inte hittas"));
                if (newDeviationCause.SpecifyChild && employeeChildId <= 0)
                    return new MobileResult(param, GetText(8814, "Du måste ange barn"));
                if (newDeviationCause.MandatoryNote && string.IsNullOrEmpty(comment))
                    return new MobileResult(param, Texts.DeviationCauseMandatoryNoteMessage);

                #region Schedule

                //TimeScheduleTemplatePeriod
                TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = TimeEngineManager(param.ActorCompanyId, param.UserId).GetSequentialSchedule(date, timeScheduleTemplatePeriodId, employeeId, true);
                if (timeScheduleTemplatePeriod == null)
                    return new MobileResult(param, Texts.DeviationsNotSavedMessage);

                //TimeScheduleTemplateBlock
                List<TimeScheduleTemplateBlock> scheduleBlocks = timeScheduleTemplatePeriod.TimeScheduleTemplateBlock.OrderBy(t => t.StartTime).ToList();
                TimeScheduleTemplateBlock scheduleStart = scheduleBlocks.FirstOrDefault();

                //Schedule in
                DateTime actualScheduleIn = CalendarUtility.DATETIME_DEFAULT;
                if (scheduleStart != null)
                    actualScheduleIn = date + scheduleStart.StartTime.TimeOfDay;

                #endregion

                newTimeBlock.TimeDeviationCauseStartId = newDeviationCauseId;
                newTimeBlock.TimeDeviationCauseStopId = newDeviationCauseId;
                newTimeBlock.Comment = comment ?? string.Empty;
                newTimeBlock.EmployeeChildId = employeeChildId.ToNullable();

                if (newStartTime > actualScheduleIn)
                {
                    //User has arrived to work later then schedule, only absence blocks can be created
                    newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(actualScheduleIn);
                    newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(newStartTime, actualScheduleIn, newStartTime.Date);
                    originalTimeBlock.StartTime = newTimeBlock.StopTime;
                    originalTimeBlock.Comment = comment ?? string.Empty;

                    if (newDeviationCause.Type != (int)TermGroup_TimeDeviationCauseType.Absence)
                        newTimeBlock.CreateAsBlank = true;

                    if (originalTimeBlock.StartTime > originalTimeBlock.StopTime)
                    {
                        originalTimeBlock.StopTime = originalTimeBlock.StartTime;
                        ChangeEntityState(originalTimeBlock, SoeEntityState.Deleted);
                    }
                }
                else
                {
                    newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(newStartTime);
                    newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(actualScheduleIn, newStartTime.Date, actualScheduleIn.Date);
                    originalTimeBlock.StartTime = newTimeBlock.StopTime;
                }

                timeBlocks.Add(newTimeBlock);
                timeBlocks.Add(originalTimeBlock);

                #endregion

                #region Perform

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).AddModifyTimeBlocks(timeBlocks, date, timeScheduleTemplatePeriodId, employeeId, newDeviationCauseId);

                //Set result
                MobileResult mobileResult = null;
                if (result.Success)
                {
                    mobileResult = new MobileResult(param);
                    if (!newDeviationCause.ExcludeFromPresenceWorkRules)
                    {
                        EvaluateDeviationsAgainstWorkRules evaluateDeviationsAgainstWorkRulesResult = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateDeviationsAgainstWorkRules(employeeId, date);
                        if (!string.IsNullOrEmpty(evaluateDeviationsAgainstWorkRulesResult.InfoMessage))
                            mobileResult.SetSuccessMessage(evaluateDeviationsAgainstWorkRulesResult.InfoMessage);
                    }
                }
                else
                    mobileResult = new MobileResult(param, FormatMessage(Texts.DeviationsNotSavedMessage, result.ErrorNumber));

                #endregion

                return mobileResult;

            }
            catch (Exception e)
            {
                LogError("PerformSaveStartTime: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileResult PerformSaveStopTime(MobileParam param, DateTime newStopTime, DateTime date, int timeScheduleTemplatePeriodId, int newDeviationCauseId, int timeBlockId, int employeeId, string comment, int employeeChildId)
        {
            try
            {
                #region Prereq

                TimeBlock originalTimeBlock = TimeBlockManager.GetTimeBlockDiscardState(timeBlockId);
                if (originalTimeBlock == null || originalTimeBlock.State != (int)SoeEntityState.Active)
                    return new MobileResult(param, FormatMessage(Texts.DeviationsNotSavedMessage, 0));

                List<TimeBlock> timeBlocks = new List<TimeBlock>();
                TimeBlock newTimeBlock = new TimeBlock();
                TimeDeviationCause newDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(newDeviationCauseId, param.ActorCompanyId, false);
                if (newDeviationCause == null)
                    return new MobileResult(param, GetText(8813, "Orsak kunde inte hittas"));

                if (newDeviationCause.SpecifyChild && employeeChildId <= 0)
                    return new MobileResult(param, GetText(8814, "Du måste ange barn"));

                if (newDeviationCause.MandatoryNote && string.IsNullOrEmpty(comment))
                    return new MobileResult(param, Texts.DeviationCauseMandatoryNoteMessage);

                #region Schedule

                //TimeScheduleTemplatePeriod
                TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = TimeEngineManager(param.ActorCompanyId, param.UserId).GetSequentialSchedule(date, timeScheduleTemplatePeriodId, employeeId, true);
                if (timeScheduleTemplatePeriod == null)
                    return new MobileResult(param, Texts.AttestEmployeeDayNotFoundMessage);

                //TimeScheduleTemplateBlock
                List<TimeScheduleTemplateBlock> scheduleBlocks = timeScheduleTemplatePeriod.TimeScheduleTemplateBlock.OrderBy(t => t.StartTime).ToList();
                TimeScheduleTemplateBlock scheduleStop = scheduleBlocks.LastOrDefault();

                //Schedule out
                DateTime actualScheduleOut = CalendarUtility.DATETIME_DEFAULT;
                if (scheduleStop != null)
                    actualScheduleOut = date.AddDays((scheduleStop.StopTime - CalendarUtility.DATETIME_DEFAULT.Date).Days) + scheduleStop.StopTime.TimeOfDay;

                #endregion

                newTimeBlock.TimeDeviationCauseStartId = newDeviationCauseId;
                newTimeBlock.TimeDeviationCauseStopId = newDeviationCauseId;
                newTimeBlock.Comment = comment ?? string.Empty;
                newTimeBlock.EmployeeChildId = employeeChildId.ToNullable();

                if (newStopTime < actualScheduleOut)
                {
                    // User has left work earlier then scheduled, only absence blocks should be created

                    newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(newStopTime).AddDays((newStopTime.Date - date.Date).Days);
                    newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(actualScheduleOut).AddDays((actualScheduleOut.Date - date.Date).Days);

                    if (newDeviationCause.Type != (int)TermGroup_TimeDeviationCauseType.Absence)
                        newTimeBlock.CreateAsBlank = true;

                    originalTimeBlock.StopTime = newTimeBlock.StartTime;
                    originalTimeBlock.Comment = comment ?? string.Empty;
                    if (originalTimeBlock.StopTime < originalTimeBlock.StartTime)
                    {
                        originalTimeBlock.StartTime = originalTimeBlock.StopTime;
                        ChangeEntityState(originalTimeBlock, SoeEntityState.Deleted);
                    }
                }
                else
                {
                    newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(actualScheduleOut).AddDays((actualScheduleOut.Date - date.Date).Days);
                    newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(newStopTime).AddDays((newStopTime.Date - date.Date).Days);

                    originalTimeBlock.StopTime = newTimeBlock.StartTime;
                }

                timeBlocks.Add(newTimeBlock);
                timeBlocks.Add(originalTimeBlock);

                #endregion

                #region Perform

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).AddModifyTimeBlocks(timeBlocks, date, timeScheduleTemplatePeriodId, employeeId, newDeviationCauseId);

                #endregion

                //Set result
                MobileResult mobileResult = null;
                if (result.Success)
                {
                    mobileResult = new MobileResult(param);
                    if (!newDeviationCause.ExcludeFromPresenceWorkRules)
                    {
                        EvaluateDeviationsAgainstWorkRules evaluateDeviationsAgainstWorkRulesResult = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateDeviationsAgainstWorkRules(employeeId, date);
                        if (!string.IsNullOrEmpty(evaluateDeviationsAgainstWorkRulesResult.InfoMessage))
                            mobileResult.SetSuccessMessage(evaluateDeviationsAgainstWorkRulesResult.InfoMessage);
                    }
                }
                else
                    mobileResult = new MobileResult(param, FormatMessage(Texts.DeviationsNotSavedMessage, result.ErrorNumber));

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformSaveStopTime: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileDayView PerformRestoreToSchedule(MobileParam param, DateTime date, int employeeId, int employeeGroupId, int schedulePeriodId, int actorCompanyId)
        {
            try
            {
                #region Prereq

                TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(actorCompanyId, employeeId, date, true);
                if (timeBlockDate == null)
                    return new MobileDayView(param, Texts.RestoreToScheduleFailedMessage);

                List<AttestEmployeeDaySmallDTO> items = new AttestEmployeeDaySmallDTO(employeeId, date, timeBlockDate.TimeBlockDateId, schedulePeriodId).ObjToList();

                #endregion

                #region Perform

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).RestoreDaysToSchedule(items);

                #endregion

                //Set result
                MobileDayView dayview = null;
                if (result.Success)
                {
                    dayview = new MobileDayView(param);
                    dayview.SetTaskResult(MobileTask.RestoreToSchedule, true);
                }
                else
                    dayview = new MobileDayView(param, FormatMessage(Texts.RestoreToScheduleFailedMessage, result.ErrorNumber));

                return dayview;
            }
            catch (Exception e)
            {
                LogError("PerformRestoreToSchedule: " + e.Message);
                return new MobileDayView(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileResult PerformRestoreDatesToSchedule(MobileParam param, string datesAsString, int employeeId)
        {
            try
            {
                #region Prereq

                List<DateTime> dates = new List<DateTime>();

                char[] separator = new char[1];
                separator[0] = ',';

                string[] separatedStringDates = datesAsString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var dateAsString in separatedStringDates)
                {
                    DateTime? date = CalendarUtility.GetNullableDateTime(dateAsString.Trim());
                    if (date.HasValue)
                        dates.Add(date.Value);
                }

                if (dates.Count == 0)
                    return new MobileResult(param, Texts.RestoreToScheduleFailedMessage);

                List<AttestEmployeeDaySmallDTO> items = new List<AttestEmployeeDaySmallDTO>();
                foreach (var date in dates)
                {
                    items.Add(new AttestEmployeeDaySmallDTO
                    {
                        Date = date,
                        EmployeeId = employeeId,
                    });
                }

                #endregion

                #region Perform

                var result = TimeEngineManager(param.ActorCompanyId, param.UserId).RestoreDaysToSchedule(items);

                #endregion

                // Set result
                var mobileResult = new MobileResult(param);
                if (result.Success)
                    mobileResult.SetTaskResult(MobileTask.Update, true);
                else
                    mobileResult = new MobileResult(param, FormatMessage(Texts.RestoreToScheduleFailedMessage, result.ErrorNumber));

                return mobileResult;
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileResult(param, GetText(8778, "Ett fel inträffade"));
            }
        }

        #endregion

        #region Views

        #region Admin

        /// <summary>
        /// Returns data for Attest - Group view
        /// </summary>
        /// <param name="param"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="employeeIds"></param>
        /// <returns></returns>
        private MobileAttestEmployeePeriods PerformGetAttestEmployeePeriods(MobileParam param, DateTime dateFrom, DateTime dateTo, bool includeAdditionalEmployees, string employeeIds)
        {
            try
            {
                var input = GetAttestEmployeePeriodsInput.CreateInputForMobile(param.ActorCompanyId, param.UserId, dateFrom, dateTo, employeeIds.IsNullOrEmpty() ? null : GetIds(employeeIds), includeAdditionalEmployees);
                var employeePeriods = TimeTreeAttestManager.GetAttestEmployeePeriods(input);

                return new MobileAttestEmployeePeriods(param, employeePeriods);

            }
            catch (Exception e)
            {
                LogError("PerformGetAttestEmployeePeriods: " + e.Message);
                return new MobileAttestEmployeePeriods(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        /// <summary>
        /// Returns data for Attest - Detail view
        /// </summary>
        /// <param name="param"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        private MobileAttestEmployeeDays PerformGetAttestEmployeeDaysAdmin(MobileParam param, DateTime dateFrom, DateTime dateTo, int employeeId, string filterAccountIds)
        {
            try
            {
                var input = GetAttestEmployeeInput.CreateInputForMobile(param.ActorCompanyId, param.UserId, param.RoleId, employeeId, dateFrom, dateTo);
                if (!string.IsNullOrEmpty(filterAccountIds))
                    input.SetOptionalParameters(filterAccountIds: StringUtility.SplitNumericList(filterAccountIds, skipZero: true));
                var attestEmployeeDays = TimeTreeAttestManager.GetAttestEmployeeDays(input);

                return new MobileAttestEmployeeDays(param, attestEmployeeDays, null, null, MobileDisplayMode.Admin);
            }
            catch (Exception e)
            {
                LogError("PerformGetAttestEmployeeDaysAdmin: " + e.Message);
                return new MobileAttestEmployeeDays(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion

        #region Employee

        private MobileAttestEmployeeDays PerformGetAttestEmployeeDays(MobileParam param, int employeeId, int employeeGroupId, int timePeriodId)
        {
            if (param == null)
                return new MobileAttestEmployeeDays(param, Texts.AttestEmployeeDayNotFoundMessage);

            #region Prereq

            TimePeriod period = TimePeriodManager.GetTimePeriod(timePeriodId, param.ActorCompanyId);
            if (period == null)
                return new MobileAttestEmployeeDays(param, Texts.PayrollPeriodNotFoundMessage);

            var input = GetAttestEmployeeInput.CreateAttestInputForWeb(param.ActorCompanyId, param.UserId, param.RoleId, employeeId, period.StartDate.Date, period.StopDate.Date);
            List<AttestEmployeeDayDTO> mobileAttestEmployeeDays = TimeTreeAttestManager.GetAttestEmployeeDays(input);

            EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(employeeGroupId);
            bool appVersionIs11OrOlder = Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_11);

            List<EmployeeRequest> requests = TimeEngineManager(param.ActorCompanyId, param.UserId).GetEmployeeRequests(employeeId, null, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }, period.StartDate.Date, period.StopDate.Date);
            requests = requests.Where(e => e.Status != (int)TermGroup_EmployeeRequestStatus.Definate).ToList();

            #endregion

            //Set result
            return new MobileAttestEmployeeDays(param, mobileAttestEmployeeDays.Where(x => !x.IsPrel).ToList(), employeeGroup, appVersionIs11OrOlder, MobileDisplayMode.User, requests);
        }

        private MobileDayView PerformGetDayView(MobileParam param, DateTime date, int employeeId, int employeeGroupId, int timeScheduleTemplatePeriodId, int actorCompanyId)
        {
            if (param == null)
                return new MobileDayView(param, Texts.AttestEmployeeDayNotFoundMessage);

            return GetDayView(param, date, employeeId, timeScheduleTemplatePeriodId, actorCompanyId);
        }

        private MobileDayView GetDayView(MobileParam param, DateTime date, int employeeId, int timeScheduleTemplatePeriodId, int actorCompanyId)
        {
            #region Perform

            var input = GetAttestEmployeeInput.CreateAttestInputForWeb(param.ActorCompanyId, param.UserId, param.RoleId, employeeId, date.Date, date.Date);
            AttestEmployeeDayDTO mobileAttestEmployeeDay = TimeTreeAttestManager.GetAttestEmployeeDays(input).FirstOrDefault(x => x.TimeScheduleTemplatePeriodId == timeScheduleTemplatePeriodId && x.Date == date.Date);
            if (mobileAttestEmployeeDay != null)
                return new MobileDayView(param, mobileAttestEmployeeDay);
            else
                return new MobileDayView(param, Texts.AttestEmployeeDayNotFoundMessage);

            #endregion
        }

        private MobileMyTimeOverview PerformGetMyTimeOverview(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadEmployment: true);
                Employment employment = employee?.GetEmployment(dateFrom, dateTo);
                bool showSetPeriodAsReadyButton = false;
                if (employment != null)
                {
                    List<AttestTransitionDTO> attestTransitions = AttestManager.GetAttestTransitionsForEmployeeGroup(TermGroup_AttestEntity.PayrollTime, employment.GetEmployeeGroupId()).ToDTOs(true).ToList();
                    if (!attestTransitions.IsNullOrEmpty())
                    {
                        int attestStateIdAfterMobileAttest = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.MobileTimeAttestResultingAttestStatus, 0, param.ActorCompanyId, 0);
                        showSetPeriodAsReadyButton = attestTransitions.Any(x => x.AttestStateToId == attestStateIdAfterMobileAttest);
                    }
                }

                var input = GetAttestEmployeeInput.CreateInputForMobile(param.ActorCompanyId, param.UserId, param.RoleId, employeeId, dateFrom, dateTo, mobileMyTime:true);
                input.SetLoading(InputLoadType.PresenceAbsenceDetails, true);

                AttestEmployeeOverviewDTO overview = TimeTreeAttestManager.GetAttestEmployeeOverview(input);
                if (overview == null)
                {
                    return new MobileMyTimeOverview(param, Texts.AttestEmployeeDayNotFoundMessage);
                }

                // Absence requests for period
                List<EmployeeRequest> requests = TimeEngineManager(param.ActorCompanyId, param.UserId).GetEmployeeRequests(employeeId, null, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }, dateFrom, dateTo);
                requests = requests.Where(e => e.Status != (int)TermGroup_EmployeeRequestStatus.Definate).ToList();

                return new MobileMyTimeOverview(param, overview, showSetPeriodAsReadyButton, requests.Count);
            }
            catch (Exception e)
            {
                LogError("PerformGetMyTimeOverview: " + e.Message);
                return new MobileMyTimeOverview(param, Texts.InternalErrorMessage + e.Message);
            }
        }

        #endregion

        #endregion

        #region Breaks

        private MobileBreaks PerformGetBreaks(MobileParam param, DateTime date, int employeeId, int timeScheduleTemplatePeriodId, int actorCompanyId)
        {
            #region Prereq

            List<Tuple<int, TimeCodeBreak, List<TimeBlock>, int>> tuples = new List<Tuple<int, TimeCodeBreak, List<TimeBlock>, int>>();

            TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = TimeEngineManager(param.ActorCompanyId, param.UserId).GetSequentialSchedule(date, timeScheduleTemplatePeriodId, employeeId, true);
            if (timeScheduleTemplatePeriod == null)
                return new MobileBreaks(param, Texts.BreaksNotFoundMessage);

            TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(actorCompanyId, employeeId, date);
            if (timeBlockDate == null)
                return new MobileBreaks(param, Texts.BreaksNotFoundMessage);

            List<TimeScheduleTemplateBlock> scheduleBlocks = timeScheduleTemplatePeriod.TimeScheduleTemplateBlock.OrderBy(t => t.StartTime).ToList();
            List<TimeScheduleTemplateBlock> scheduleBreaks = scheduleBlocks.GetBreaks();

            #endregion

            #region Perform

            int i = 1;
            foreach (TimeScheduleTemplateBlock scheduleBreak in scheduleBreaks)
            {
                if (tuples.Any(b => b.Item2.TimeCodeId == scheduleBreak.TimeCodeId))
                    continue;

                List<TimeBlock> actualBreaks = TimeBlockManager.GetBreakBlocksForGivenScheduleBreakBlock(scheduleBreak, timeBlockDate);

                TimeCodeBreak timeCodeBreak = null;
                if (scheduleBreak.TimeCodeReference.IsLoaded)
                    timeCodeBreak = scheduleBreak.TimeCode as TimeCodeBreak;
                else
                    timeCodeBreak = TimeCodeManager.GetTimeCodeBreak(scheduleBreak.TimeCodeId, param.ActorCompanyId);

                if (timeCodeBreak != null)
                {
                    tuples.Add(Tuple.Create(scheduleBreak.TimeScheduleTemplateBlockId, timeCodeBreak, actualBreaks, i));
                    i++;
                }
            }

            #endregion

            //Set result
            return new MobileBreaks(param, tuples);
        }

        private MobileBreak PerformModifyBreak(MobileParam param, DateTime date, int scheduleBreakBlockId, int employeeId, int timeScheduleTemplatePeriodId, int timeCodeBreakId, int totalMinutes, int actorCompanyId)
        {
            TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(actorCompanyId, employeeId, date, true);
            if (timeBlockDate == null)
                return new MobileBreak(param, Texts.BreaksNotFoundMessage);
            MobileBreak mobileBreak = null;

            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).MobileModifyBreak(date, scheduleBreakBlockId, employeeId, timeScheduleTemplatePeriodId, timeCodeBreakId, totalMinutes);

            //Set result
            if (result.Success)
            {
                mobileBreak = new MobileBreak(param);
                mobileBreak.SetTaskResult(MobileTask.SaveBreak, true);
            }
            else
                mobileBreak = new MobileBreak(param, FormatMessage(Texts.DeviationsNotSavedMessage, result.ErrorNumber));

            return mobileBreak;
        }

        #endregion

        #region Attest

        private MobileDicts PerformGetAttestStates(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo, MobileDisplayMode displayMode, string attestStateIdsStr)//NOSONAR
        {
            int employeegroupId = 0; //TODO: Future use: decide employeegroupid from employee, dateFrom and dateTo;

            #region Perform

            try
            {
                List<AttestStateDTO> attestStates = AttestManager.GetUserValidAttestStates(TermGroup_AttestEntity.PayrollTime, dateFrom, dateTo, true, displayMode == MobileDisplayMode.Admin ? (int?)null : employeegroupId);
                MobileDicts dict = new MobileDicts(param, attestStates.ToDictionary(i => i.AttestStateId, i => i.Name));
                dict.AddProperties("Sort", attestStates.ToDictionary(i => i.AttestStateId, i => i.Sort));
                dict.AddProperties("Color", attestStates.ToDictionary(i => i.AttestStateId, i => i.Color));
                dict.SetSelectedIds(GetIds(attestStateIdsStr));
                return dict;
            }
            catch (Exception e)
            {

                LogError(string.Format("PerformGetAttestStates: Error= {0} ", e.Message));
                return new MobileDicts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            #endregion
        }

        private MobileDicts PerformGetAttestStates(MobileParam param, int employeeId, int timePeriodId)
        {
            #region Prereq

            #endregion

            #region Perform

            try
            {
                List<AttestStateDTO> attestStates = AttestManager.GetValidAttestStatesForEmployee(employeeId, param.UserId, param.ActorCompanyId, timePeriodId, true);
                return new MobileDicts(param, attestStates.ToDictionary(i => i.AttestStateId, i => i.Name));
            }
            catch (Exception e)
            {

                LogError(string.Format("PerformGetAttestStates: Error= {0} ", e.Message));
                return new MobileDicts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            #endregion
        }

        /// <summary>
        /// Used when admin changes atteststates for employees from Attest - Group view 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeIdsStr"></param>
        /// <param name="attestStateToId"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        private MobileMessageBox PerformSaveAttestForEmployees(MobileParam param, string employeeIdsStr, int attestStateToId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                List<int> employeeIds = GetIds(employeeIdsStr);
                if (!employeeIds.Any())
                    return new MobileMessageBox(param, GetText(8836, "Du måste välja minst 1 anställd"));

                Employee currentEmployee = EmployeeManager.GetEmployeeForUser(param.UserId, param.ActorCompanyId);

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveAttestForEmployees(currentEmployee != null ? currentEmployee.EmployeeId : 0, employeeIds, attestStateToId, dateFrom, dateTo, false);
                if (!result.Success)
                    return new MobileMessageBox(param, result.ErrorMessage);

                var attestState = AttestManager.GetAttestState(attestStateToId);
                return new MobileMessageBox(param, result.IntegerValue > 0 && result.IntegerValue2 == 0, false, true, false, GetText(8835, "Attest"), GetSaveAttestMessage(result, attestState));
            }
            catch (Exception e)
            {

                LogError(string.Format("PerformSaveAttestForEmployees: Error= {0} ", e.Message));
                return new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        /// <summary>
        /// Used when admin changes atteststates for employee from Attest - Detail view 
        /// In the future it could also replace PerformSetSchedulePeriodAsReady
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="idsAndDates"></param>
        /// <param name="attestStateToId"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        private MobileMessageBox PerformSaveAttestForEmployee(MobileParam param, int employeeId, string idsAndDates, int attestStateToId, MobileDisplayMode mode)
        {
            try
            {
                if (attestStateToId == 0)
                    return new MobileMessageBox(param, Texts.AttestNotSavedMessage);

                List<SaveAttestEmployeeDayDTO> attestItems = ParseTimeScheduleTemplatePeriodsAndDates(employeeId, idsAndDates);
                if (!attestItems.Any())
                    return new MobileMessageBox(param, Texts.AttestNotSavedMessage);

                int initialAttestStateId = AttestManager.GetInitialAttestStateId(param.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
                if (initialAttestStateId == 0)
                    return new MobileMessageBox(param, Texts.AttestNotSavedMessage);

                Employee userEmployee = EmployeeManager.GetEmployeeForUser(param.UserId, param.ActorCompanyId);
                bool isMySelf = (mode == MobileDisplayMode.User) || (userEmployee != null && userEmployee.EmployeeId == employeeId);

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveAttestForEmployee(attestItems, employeeId, attestStateToId, isMySelf);
                if (!result.Success)
                    return new MobileMessageBox(param, result.ErrorMessage);

                var attestState = AttestManager.GetAttestState(attestStateToId);
                return new MobileMessageBox(param, result.IntegerValue > 0 && result.IntegerValue2 == 0, false, true, false, GetText(8835, "Attest"), GetSaveAttestMessage(result, attestState));
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformSaveAttestForEmployee: Error= {0} ", e.Message));
                return new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        /// <summary>
        /// Used for validation, it i done before PerformSetSchedulePeriodAsReady
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="attestStateToId"></param>
        /// <param name="idsAndDates"></param>
        /// <param name="isMySelf"></param>
        /// <returns></returns>
        private MobileMessageBox PerformSetSchedulePeriodAsReadyValidation(MobileParam param, int employeeId, int attestStateToId, string idsAndDates, bool isMySelf, string filterAccountIds)
        {
            #region Prereq

            List<SaveAttestEmployeeDayDTO> attestItems = ParseTimeScheduleTemplatePeriodsAndDates(employeeId, idsAndDates);
            if (!attestItems.Any())
                return new MobileMessageBox(param, Texts.AttestNotSavedMessage);

            List<DateTime> dates = attestItems.Select(i => i.Date).OrderBy(i => i.Date).ToList();
            List<AttestEmployeeDayDTO> dayItems = new List<AttestEmployeeDayDTO>();
            if (dates.Any())
            {
                var input = GetAttestEmployeeInput.CreateAttestInputForWeb(param.ActorCompanyId, param.UserId, param.RoleId, employeeId, dates.First(), dates.Last());
                if (!string.IsNullOrEmpty(filterAccountIds))
                    input.SetOptionalParameters(filterAccountIds: StringUtility.SplitNumericList(filterAccountIds, skipZero: true));
                dayItems.AddRange(TimeTreeAttestManager.GetAttestEmployeeDays(input));
            }

            #endregion

            #region Perform

            try
            {
                SaveAttestEmployeeValidationDTO validation = TimeTreeAttestManager.SaveAttestForEmployeeValidation(dayItems, attestStateToId, isMySelf, employeeId, param.ActorCompanyId, param.RoleId, param.UserId);
                return new MobileMessageBox(param, validation.Success, validation.CanOverride, true, true, validation.Title, validation.Message, ParseTimeScheduleTemplatePeriodsAndDates(validation.ValidItems));
            }
            catch (Exception e)
            {

                LogError(string.Format("PerformSetSchedulePeriodAsReadyValidation: Error= {0} ", e.Message));
                return new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            #endregion
        }

        /// <summary>
        /// Used when a user changes atteststate from My time
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="employeeGroupId"></param>
        /// <param name="idsAndDates"></param>
        /// <param name="attestStateId"></param>
        /// <returns></returns>
        private MobileAttestEmployeeDays PerformSetSchedulePeriodAsReady(MobileParam param, int employeeId, int employeeGroupId, string idsAndDates, int attestStateId = 0)
        {
            #region Prereq

            List<SaveAttestEmployeeDayDTO> attestItems = ParseTimeScheduleTemplatePeriodsAndDates(employeeId, idsAndDates);
            if (attestItems.IsNullOrEmpty())
                return new MobileAttestEmployeeDays(param, Texts.AttestNotSavedMessage);

            int initialAttestStateId = AttestManager.GetInitialAttestStateId(param.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
            if (initialAttestStateId == 0)
                return new MobileAttestEmployeeDays(param, Texts.AttestNotSavedMessage);

            if (attestStateId == 0)
            {
                int attestStateIdAfterMobileAttest = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.MobileTimeAttestResultingAttestStatus, 0, param.ActorCompanyId, 0);
                if (attestStateIdAfterMobileAttest == 0)
                    return new MobileAttestEmployeeDays(param, Texts.AttestNotSavedMessage);

                attestStateId = attestStateIdAfterMobileAttest;
            }

            List<AttestTransition> attestTransitions = AttestManager.GetAttestTransitionsForEmployeeGroup(TermGroup_AttestEntity.PayrollTime, employeeGroupId);
            if (attestTransitions.IsNullOrEmpty())
                return new MobileAttestEmployeeDays(param, Texts.AttestNotSavedMessage);

            #endregion

            #region Perform

            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveAttestForEmployee(attestItems, employeeId, attestStateId, isMySelf: true);

            #endregion

            //Set result
            MobileAttestEmployeeDays mobileAttestEmployeeDays = null;
            if (result.Success)
            {
                mobileAttestEmployeeDays = new MobileAttestEmployeeDays(param, new List<AttestEmployeeDayDTO>());
                mobileAttestEmployeeDays.SetTaskResult(MobileTask.SaveAttest, true);
            }
            else
                mobileAttestEmployeeDays = new MobileAttestEmployeeDays(param, FormatMessage(Texts.AttestNotSavedMessage, result.ErrorNumber));

            return mobileAttestEmployeeDays;
        }

        /// <summary>
        /// Used when a user changes atteststate from My time for a date range
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="employeeGroupId"></param>
        /// <param name="idsAndDates"></param>
        /// <param name="attestStateId"></param>
        /// <returns></returns>
        private MobileMessageBox PerformSaveAttestForPeriod(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo, int attestStateId = 0)
        {
            try
            {
                #region Prereq

                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadEmployment: true);
                if (employee == null)
                    return new MobileMessageBox(param, Texts.EmployeeNotFoundMessage);

                if (attestStateId == 0)
                {
                    int attestStateIdAfterMobileAttest = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.MobileTimeAttestResultingAttestStatus, 0, param.ActorCompanyId, 0);
                    if (attestStateIdAfterMobileAttest == 0)
                        return new MobileMessageBox(param, Texts.AttestNotSavedMessage);

                    attestStateId = attestStateIdAfterMobileAttest;
                }

                var attestState = AttestManager.GetAttestState(attestStateId);
                if (attestState == null)
                    return new MobileMessageBox(param, Texts.AttestNotSavedMessage);

                #endregion

                List<TimeBlockDate> timeBlockDates = TimeBlockManager.GetTimeBlockDates(employeeId, CalendarUtility.GetDates(dateFrom, dateTo));
                if (!timeBlockDates.Any())
                    return new MobileMessageBox(param, Texts.AttestNotSavedMessage);

                List<SaveAttestEmployeeDayDTO> attestItems = new List<SaveAttestEmployeeDayDTO>();
                foreach (var timeBlockDate in timeBlockDates)
                {
                    attestItems.Add(
                        new SaveAttestEmployeeDayDTO
                        {
                            TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                            Date = timeBlockDate.Date,
                        });
                }

                Employee userEmployee = EmployeeManager.GetEmployeeForUser(param.UserId, param.ActorCompanyId);
                bool isMySelf = (userEmployee != null && userEmployee.EmployeeId == employeeId);

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveAttestForEmployee(attestItems, employeeId, attestStateId, isMySelf);
                if (!result.Success)
                    return new MobileMessageBox(param, result.ErrorMessage);

                return new MobileMessageBox(param, result.IntegerValue > 0 && result.IntegerValue2 == 0, false, true, false, GetText(8835, "Attest"), GetSaveAttestMessage(result, attestState));
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformSaveAttestForEmployee: Error= {0} ", e.Message));
                return new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        /// <summary>
        /// Parse periods and dates
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="idsAndDates">EX: idsAndDates=158,2011-12-01;159,2011-12-02; </param>
        /// <returns></returns>
        private List<SaveAttestEmployeeDayDTO> ParseTimeScheduleTemplatePeriodsAndDates(int employeeId, string idsAndDates)
        {
            var attestItems = new List<SaveAttestEmployeeDayDTO>();

            char[] separator = new char[1];

            separator[0] = ';';
            string[] firstSplit = idsAndDates.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in firstSplit)
            {
                separator[0] = ',';
                string[] secondSplit = part.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                if (secondSplit.Count() == 2)
                {
                    string value2 = secondSplit[1].Trim();

                    DateTime? date = CalendarUtility.GetNullableDateTime(value2);
                    if (date.HasValue)
                    {
                        attestItems.Add(new SaveAttestEmployeeDayDTO
                        {
                            Date = date.Value.Date,
                        });
                    }
                }
            }

            List<TimeBlockDate> timeBlockDates = TimeBlockManager.GetTimeBlockDates(employeeId, attestItems.Select(x => x.Date).ToList());
            foreach (var item in attestItems)
            {
                TimeBlockDate timeBlockDate = timeBlockDates.FirstOrDefault(x => x.Date == item.Date && x.EmployeeId == employeeId);
                if (timeBlockDate == null)
                    continue;

                item.TimeBlockDateId = timeBlockDate.TimeBlockDateId;
            }

            return attestItems;
        }

        /// <summary>
        /// Parse periods and dates
        /// </summary>
        /// <param name="items">EX: idsAndDates=158,2011-12-01;159,2011-12-02; </param>
        /// <returns></returns>
        private string ParseTimeScheduleTemplatePeriodsAndDates(List<SaveAttestEmployeeDayDTO> items)
        {
            if (items == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (var item in items)
            {
                if (sb.Length > 0)
                    sb.Append(";");
                sb.Append(String.Format("{0},{1}", 0, item.Date.ToString("yyyy-MM-dd")));
            }

            return sb.ToString();
        }

        private string GetSaveAttestMessage(ActionResult result, AttestState attestState)
        {
            string message = string.Format(GetText(8837, "{0} transaktioner fick attestnivå {1}"), result.IntegerValue.ToString(), StringUtility.NullToEmpty(attestState?.Name));

            if (result.IntegerValue == 0 || result.IntegerValue2 > 0)
            {
                //show if valid is zero or invalid is over zero                        
                message += "\n" + string.Format(GetText(8838, "{0} transaktioner kunde inte få attestnivå {1}"), result.IntegerValue2.ToString(), StringUtility.NullToEmpty(attestState?.Name));
            }

            return message;
        }

        #endregion

        #region Info

        private MobileResult PerformGetAttestInfoMessage(MobileParam param, string info)
        {
            try
            {
                MobileResult result = new MobileResult(param);
                if (info.IsNullOrEmpty())
                    return new MobileResult(param); //Empty message

                List<int> ids = GetIds(info);
                result.SetMessage(TimeTreeAttestManager.GetInfoMessage(ids.Cast<SoeTimeAttestInformation>().ToList())); 

                return result;
            }
            catch (Exception e)
            {
                LogError("GetAttestInfoMessages: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion
        #region TimeStamps

        private MobileTimeTerminals PerformGetTimeTerminalsForUser(MobileParam param, DateTime date)
        {
            try
            {
                List<TimeTerminalDTO> timeTerminals = GoTimeStampManager.GetTimeTerminalsForUser(param.UserId, param.ActorCompanyId, date);
                return new MobileTimeTerminals(param, timeTerminals);
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformGetTimeTerminalsForUser: Error= {0} ", e.Message));
                return new MobileTimeTerminals(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion

        #region Warnings

        private MobileResult PerformGetAttestWarningMessage(MobileParam param, string warnings)
        {
            try
            {
                MobileResult result = new MobileResult(param);
                if (warnings.IsNullOrEmpty())
                    return new MobileResult(param); //Empty message

                List<int> ids = GetIds(warnings);
                result.SetMessage(TimeTreeAttestManager.GetWarningMessage(ids.Cast<SoeTimeAttestWarning>().ToList()));

                return result;
            }
            catch (Exception e)
            {
                LogError("GetAttestWarningMessages: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        #endregion

        #region Permissions/settings

        /// <summary>
        /// OBS! EmployeeId is sent from the client
        /// </summary>
        /// <param name="param"></param>
        /// <param name="mobileDisplayMode"></param>
        /// <returns></returns>

        private MobilePermissions PerformGetTimePermissions(MobileParam param, MobileDisplayMode mobileDisplayMode)
        {
            List<Tuple<string, int>> features = new List<Tuple<string, int>>();

            if (mobileDisplayMode == MobileDisplayMode.User)
            {
                #region User/Employee

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_AbsenceRequestsUser, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("editAbsenceRequest", 1));
                else
                    features.Add(Tuple.Create("editAbsenceRequest", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Time_AttestUser_EditAbsence, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("editAbsence", 1));
                else
                    features.Add(Tuple.Create("editAbsence", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Time_AttestUser_AdditionAndDeduction, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("editExpense", 1));
                else
                    features.Add(Tuple.Create("editExpense", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Time_AttestUser_RestoreToSchedule, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("restoreToSchedule", 1));
                else
                    features.Add(Tuple.Create("restoreToSchedule", 0));

                #endregion

            }
            else if (mobileDisplayMode == MobileDisplayMode.Admin)
            {
                #region Admin

                if (FeatureManager.HasRolePermission(Feature.Time_Time_Attest_RestoreToSchedule, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("restoreToSchedule", 1));
                else
                    features.Add(Tuple.Create("restoreToSchedule", 0));

                if (SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeAttestTreeIncludeAdditionalEmployees, 0, param.ActorCompanyId, 0))
                    features.Add(Tuple.Create("includeAdditionalEmployees", 1));
                else
                    features.Add(Tuple.Create("includeAdditionalEmployees", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Time_Attest_AdditionAndDeduction, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("editExpense", 1));
                else
                    features.Add(Tuple.Create("editExpense", 0));

                #endregion
            }

            MobilePermissions mobileFeatures = new MobilePermissions(param, features);
            return mobileFeatures;
        }

        private MobilePermissions PerformGetCommonPermissions(MobileParam param)
        {
            List<Tuple<string, int>> features = new List<Tuple<string, int>>();

            if (FeatureManager.HasRolePermission(Feature.Communication_XEmail, Permission.Modify, param.RoleId, param.ActorCompanyId))
                features.Add(Tuple.Create("messageModify", 1));
            else
                features.Add(Tuple.Create("messageModify", 0));

            if (FeatureManager.HasRolePermission(Feature.Communication_XEmail_Send, Permission.Modify, param.RoleId, param.ActorCompanyId))
                features.Add(Tuple.Create("messageSend", 1));
            else
                features.Add(Tuple.Create("messageSend", 0));

            if (FeatureManager.HasRolePermission(Feature.Communication_XEmail_Delete, Permission.Modify, param.RoleId, param.ActorCompanyId))
                features.Add(Tuple.Create("messageDelete", 1));
            else
                features.Add(Tuple.Create("messageDelete", 0));

            bool billingUseQuantityPrices = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingUseQuantityPrices, 0, param.ActorCompanyId, 0);
            features.Add(Tuple.Create("billingUseQuantityPrices", billingUseQuantityPrices ? 1 : 0));

            MobilePermissions mobileFeatures = new MobilePermissions(param, features);
            return mobileFeatures;
        }

        #endregion

        #endregion

        #region Schedule      

        #region Registry
        private MobileBreakTimeCodes PerformGetBreakTimeCodes(MobileParam param, int employeeId, DateTime? date)
        {
            List<TimeCodeBreak> timeCodeBreaks = null;

            if (employeeId == 0)
                timeCodeBreaks = TimeCodeManager.GetTimeCodeBreaks(param.ActorCompanyId, true);
            else
                timeCodeBreaks = TimeCodeManager.GetTimeCodeBreaksForEmployee(param.ActorCompanyId, employeeId, date, true);

            return new MobileBreakTimeCodes(param, timeCodeBreaks);
        }

        private MobileSheduleTypes PerformGetScheduleTypes(MobileParam param)
        {
            List<TimeScheduleTypeDTO> scheduleTypes = TimeScheduleManager.GetTimeScheduleTypes(param.ActorCompanyId, getAll: false).ToDTOs(false).ToList();
            scheduleTypes.Insert(0, new TimeScheduleTypeDTO() { TimeScheduleTypeId = 0, Code = String.Empty, Name = "" });

            return new MobileSheduleTypes(param, scheduleTypes);
        }


        /// <summary>
        /// bool isAdmin is sent from the client - Future use, maybe
        /// </summary>
        /// <param name="param"></param>
        /// <param name="shiftId"></param>
        /// <param name="isAdmin"></param>
        /// <returns></returns>
        private MobileShiftTasks PerformGetShiftTasks(MobileParam param, int shiftId)
        {
            List<TimeScheduleTemplateBlockTaskDTO> shiftTasks = TimeScheduleManager.GetShiftTasks(param.ActorCompanyId, new List<int> { shiftId }).ToDTOs().ToList();
            return new MobileShiftTasks(param, shiftTasks);
        }

        /// <summary>
        /// return deviationcauses for regular absence
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId">the employee on who the user will report absence on </param>
        /// <param name="wholeDay"></param>
        /// <returns></returns>
        private MobileDeviationCauses PerformGetAbsencePlanningDeviationCauses(MobileParam param, int employeeId, bool wholeDay)
        {
            MobileDeviationCauses mobileCauses = null;
            try
            {
                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadEmployment: true);
                if (employee == null)
                    return new MobileDeviationCauses(param, Texts.EmployeeNotFoundMessage);

                bool isMySelf = this.IsMySelf(param, employeeId, employee);
                var causes = TimeDeviationCauseManager.GetTimeDeviationCausesByEmployeeGroup(param.ActorCompanyId, employeeGroupId: employee.GetEmployeeGroupId(), onlyUseInTimeTerminal: isMySelf, setTimeDeviationTypeName: true);
                causes = causes.Where(i => i.Type == (int)TermGroup_TimeDeviationCauseType.Absence || i.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence).ToList();

                if (!wholeDay)
                {
                    causes = causes.Where(x => !x.OnlyWholeDay).ToList();
                }

                mobileCauses = new MobileDeviationCauses(param, causes);
            }
            catch (Exception e)
            {
                LogError("PerformGetAbsencePlanningDeviationCauses: " + e.Message);
                mobileCauses = new MobileDeviationCauses(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileCauses;
        }

        /// <summary>
        /// Returns deviationcauses for absencerequest (Employee)
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="employeeGroupId"></param>
        /// <returns></returns>

        private MobileDeviationCauses PerformGetAbsenceRequestCauses(MobileParam param, int employeeId, int employeeGroupId)
        {
            List<TimeDeviationCause> causes = TimeDeviationCauseManager.GetTimeDeviationCausesEmployeeRequests(param.ActorCompanyId, employeeGroupId, employeeId);

            MobileDeviationCauses mobileCauses = new MobileDeviationCauses(param, causes);

            return mobileCauses;
        }

        private MobileDicts PerformGetEmployeeChilds(MobileParam param, int employeeId)
        {
            try
            {
                Dictionary<int, string> dicts = EmployeeManager.GetEmployeeChildsDict(employeeId, false);
                return new MobileDicts(param, dicts);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileDicts(param, GetText(8778, "Ett fel inträffade"));
            }
        }

        #endregion

        #region Wanted/Unwanted

        private MobileShift PerformSetShiftAsWanted(MobileParam param, int employeeId, int employeeGroupId, int shiftId)
        {
            bool preventpermission = false;
            MobileShift mobileShift = new MobileShift(param);
            TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, false, true);

            if (!FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftWanted, Permission.Modify, param.RoleId, param.ActorCompanyId))
                return new MobileShift(param, GetText(8652, "Otillåten ändring, behörighet saknas"));

            if (shift.IsBreak)
                return new MobileShift(param, GetText(8439, "Otillåten ändring på rast."));


            if (shift.ShiftTypeId.HasValue)
            {
                List<Employee> employeesWithSkills = new List<Employee>();
                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadEmployeeSkill: true);
                if (employee != null)
                    employeesWithSkills.Add(employee);

                List<int> empIds = TimeScheduleManager.MatchEmployeesByShiftTypeSkills(shift.ShiftTypeId.Value, param.ActorCompanyId, employeesWithSkills);
                preventpermission = empIds.Contains(employeeId);
            }

            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).HandleTimeScheduleShift(HandleShiftAction.Wanted, shiftId, 0, employeeId, 0, param.RoleId, preventpermission);

            if (result.Success)
            {
                mobileShift.SetTaskResult(MobileTask.SetShiftAsWanted, true);
            }
            else
            {
                if (result.ErrorNumber > 0)
                    mobileShift = new MobileShift(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    mobileShift = new MobileShift(param, FormatMessage(Texts.SetShiftAsWantedFailed, result.ErrorNumber));
            }

            return mobileShift;
        }

        private MobileShift PerformUpdateShiftSetUndoWanted(MobileParam param, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileShift mobileShift = new MobileShift(param);
            TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, false, true);

            //Remove this when breaks is implemented in mobile app
            if (shift.IsBreak)
                return new MobileShift(param, GetText(8439, "Otillåten ändring på rast."));

            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).HandleTimeScheduleShift(HandleShiftAction.UndoWanted, shiftId, 0, employeeId, 0, param.RoleId, false);

            if (result.Success)
            {
                mobileShift.SetTaskResult(MobileTask.UpdateShiftSetUndoWanted, true);
            }
            else
            {
                if (result.ErrorNumber > 0)
                    mobileShift = new MobileShift(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    mobileShift = new MobileShift(param, FormatMessage(Texts.UpdateShiftSetUndoWantedFailed, result.ErrorNumber));
            }

            return mobileShift;
        }

        private MobileShift PerformSetShiftAsUnWanted(MobileParam param, int employeeId, int employeeGroupId, int shiftId)
        {
            bool preventpermission = false;
            MobileShift mobileShift = new MobileShift(param);
            TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, false, true);

            if (!FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftUnwanted, Permission.Modify, param.RoleId, param.ActorCompanyId))
                return new MobileShift(param, GetText(8652, "Otillåten ändring, behörighet saknas"));

            if (shift.IsBreak)
                return new MobileShift(param, GetText(8439, "Otillåten ändring på rast."));

            if (shift.ShiftTypeId.HasValue)
            {
                List<Employee> employeesWithSkills = new List<Employee>();
                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadEmployeeSkill: true);
                if (employee != null)
                    employeesWithSkills.Add(employee);

                List<int> empIds = TimeScheduleManager.MatchEmployeesByShiftTypeSkills(shift.ShiftTypeId.Value, param.ActorCompanyId, employeesWithSkills);
                preventpermission = empIds.Contains(employeeId);
            }

            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).HandleTimeScheduleShift(HandleShiftAction.Unwanted, shiftId, 0, employeeId, 0, param.RoleId, preventpermission);

            if (result.Success)
            {
                mobileShift.SetTaskResult(MobileTask.SetShiftAsUnWanted, true);
            }
            else
            {
                if (result.ErrorNumber > 0)
                    mobileShift = new MobileShift(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    mobileShift = new MobileShift(param, FormatMessage(Texts.SetShiftAsUnWantedFailed, result.ErrorNumber));
            }

            return mobileShift;
        }

        private MobileShift PerformUpdateShiftSetUndoUnWanted(MobileParam param, int employeeId, int employeeGroupId, int shiftId)
        {
            MobileShift mobileShift = new MobileShift(param);
            TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, false, true);

            //Remove this when breaks is implemented in mobile app
            if (shift.IsBreak)
                return new MobileShift(param, GetText(8439, "Otillåten ändring på rast."));

            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).HandleTimeScheduleShift(HandleShiftAction.UndoUnwanted, shiftId, 0, employeeId, 0, param.RoleId, false);

            if (result.Success)
            {
                mobileShift.SetTaskResult(MobileTask.UpdateShiftSetUndoUnWanted, true);
            }
            else
            {
                if (result.ErrorNumber > 0)
                    mobileShift = new MobileShift(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    mobileShift = new MobileShift(param, FormatMessage(Texts.UpdateShiftSetUndoUnWantedFailed, result.ErrorNumber));
            }

            return mobileShift;
        }

        private MobileMessageBox PerformWantShiftValidateSkills(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox mobileMessageBox;

            try
            {
                mobileMessageBox = ValidateSkills(param, shiftId, true, MobileDisplayMode.User, employeeId);
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformWantShiftValidateSkills: shiftId = {0}, employeeId = {1}, Error= {2} ", shiftId, employeeId, e.Message));
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
            return mobileMessageBox;
        }

        #endregion

        #region AbsenceRequest

        private MobileRequests PerformGetAbsenceRequestsEmployee(MobileParam param, int employeeId, DateTime? dateFrom, DateTime? dateTo)
        {
            List<EmployeeRequest> requests = TimeEngineManager(param.ActorCompanyId, param.UserId).GetEmployeeRequests(employeeId, null, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }, dateFrom, dateTo).OrderByDescending(o => o.Start).ThenByDescending(o => o.Stop).ThenByDescending(o => o.Created).ToList();

            MobileRequests mobileRequests = new MobileRequests(param, requests, MobileDisplayMode.User);

            return mobileRequests;
        }

        /// <summary>
        /// OBS! EmployeeId is sent from the client
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="absenceRequestId"></param>
        /// <returns></returns>
        private MobileRequest PerformGetAbsenceRequestEmployee(MobileParam param, int absenceRequestId)
        {
            EmployeeRequest request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(absenceRequestId);

            if (request == null)
                return new MobileRequest(param, Texts.AbsenceRequestNotFoundMessage);

            MobileRequest mobileRequest = new MobileRequest(param, request, true, MobileDisplayMode.User, new List<TimeSchedulePlanningDayDTO>());

            return mobileRequest;
        }

        private MobileRequest PerformSaveAbsenceRequest(MobileParam param, int employeeId, int absenceRequestId, DateTime dateFrom, DateTime dateTo, int deviationCauseId, string note, bool absenceWholeDays, int employeeChildId)
        {
            if (deviationCauseId == 0)
                return new MobileRequest(param, GetText(8544, "Orsak måste anges"));

            var deviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(deviationCauseId, param.ActorCompanyId, false);
            if (deviationCause == null)
                return new MobileRequest(param, GetText(8813, "Orsak kunde inte hittas"));

            if (deviationCause.SpecifyChild && employeeChildId <= 0)
                return new MobileRequest(param, GetText(8814, "Du måste ange barn"));

            EmployeeRequest request = null;
            MobileRequest mobileRequest = new MobileRequest(param);

            DateTime endOfday = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, 23, 59, 0); //dont use CalendarUtility.GetEndOfDay, because it will add seconds

            bool updateCreateAbsenceFirstAndLastDaySettings = (dateFrom.TimeOfDay.TotalMinutes != 0 || (dateTo.TimeOfDay.TotalMinutes != 0 && dateTo != endOfday));
            if (absenceWholeDays)
                updateCreateAbsenceFirstAndLastDaySettings = false;

            if (absenceRequestId > 0)
            {
                request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(absenceRequestId);

                if (request == null)
                    return new MobileRequest(param, Texts.AbsenceRequestNotFoundMessage);

                if (request.Status != (int)TermGroup_EmployeeRequestStatus.RequestPending)
                    return new MobileRequest(param, GetText(8530, "Ej tillåtet att ändra en ansökan som är behandlad eller under behandling"));

                request.Comment = note;
                request.Start = CalendarUtility.GetBeginningOfDay(dateFrom);
                request.Stop = CalendarUtility.GetEndOfDay(dateTo);
                request.TimeDeviationCauseId = deviationCauseId;
                request.EmployeeChildId = (deviationCause.SpecifyChild && employeeChildId > 0) ? employeeChildId : (int?)null;

                #region Part time

                if (updateCreateAbsenceFirstAndLastDaySettings)
                {
                    ExtendedAbsenceSetting extendedSettings = null;
                    if (request.ExtendedAbsenceSetting != null)
                    {
                        extendedSettings = request.ExtendedAbsenceSetting;
                    }
                    else
                    {
                        extendedSettings = new ExtendedAbsenceSetting();
                        request.ExtendedAbsenceSetting = extendedSettings;
                    }

                    extendedSettings.AbsenceFirstAndLastDay = true;

                    if (dateFrom.TimeOfDay.TotalMinutes == 0)
                    {
                        extendedSettings.AbsenceWholeFirstDay = true;
                        extendedSettings.AbsenceFirstDayStart = CalendarUtility.DATETIME_DEFAULT;
                    }
                    else
                    {
                        extendedSettings.AbsenceWholeFirstDay = false;
                        extendedSettings.AbsenceFirstDayStart = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, dateFrom.TimeOfDay);
                    }

                    if (dateTo.TimeOfDay.TotalMinutes == 0)
                    {
                        extendedSettings.AbsenceWholeLastDay = true;
                        extendedSettings.AbsenceLastDayStart = CalendarUtility.DATETIME_DEFAULT;
                    }
                    else
                    {
                        extendedSettings.AbsenceWholeLastDay = false;
                        extendedSettings.AbsenceLastDayStart = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, dateTo.TimeOfDay);
                    }
                }
                else
                {
                    if (request.ExtendedAbsenceSetting != null && request.ExtendedAbsenceSetting.AbsenceFirstAndLastDay)
                    {
                        if (request.ExtendedAbsenceSetting.PercentalAbsence || request.ExtendedAbsenceSetting.AdjustAbsencePerWeekDay)
                        {
                            //Clear only part time values
                            request.ExtendedAbsenceSetting.AbsenceFirstAndLastDay = false;
                            request.ExtendedAbsenceSetting.AbsenceWholeFirstDay = false;
                            request.ExtendedAbsenceSetting.AbsenceWholeLastDay = false;
                            request.ExtendedAbsenceSetting.AbsenceFirstDayStart = CalendarUtility.DATETIME_DEFAULT;
                            request.ExtendedAbsenceSetting.AbsenceLastDayStart = CalendarUtility.DATETIME_DEFAULT;
                        }
                        else
                        {
                            //clean whole extendedsetting entity
                            request.ExtendedAbsenceSetting = null;
                            request.ExtendedAbsenceSettingId = null;

                        }
                    }
                }
                #endregion
            }
            else
            {
                request = new EmployeeRequest()
                {
                    EmployeeRequestId = absenceRequestId,
                    ActorCompanyId = param.ActorCompanyId,
                    Comment = note,
                    EmployeeId = employeeId,
                    EmployeeChildId = (deviationCause.SpecifyChild && employeeChildId > 0) ? employeeChildId : (int?)null,
                    Start = CalendarUtility.GetBeginningOfDay(dateFrom),
                    Stop = CalendarUtility.GetEndOfDay(dateTo),
                    TimeDeviationCauseId = deviationCauseId,
                    Type = (int)TermGroup_EmployeeRequestType.AbsenceRequest,
                };

                #region Part time

                if (updateCreateAbsenceFirstAndLastDaySettings)
                {
                    ExtendedAbsenceSetting extendedSettings = new ExtendedAbsenceSetting();
                    extendedSettings.AbsenceFirstAndLastDay = true;
                    if (dateFrom.TimeOfDay.TotalMinutes == 0)
                    {
                        extendedSettings.AbsenceWholeFirstDay = true;
                        extendedSettings.AbsenceFirstDayStart = CalendarUtility.DATETIME_DEFAULT;
                    }
                    else
                    {
                        extendedSettings.AbsenceWholeFirstDay = false;
                        extendedSettings.AbsenceFirstDayStart = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, dateFrom.TimeOfDay);
                    }

                    if (dateTo.TimeOfDay.TotalMinutes == 0)
                    {
                        extendedSettings.AbsenceWholeLastDay = true;
                        extendedSettings.AbsenceLastDayStart = CalendarUtility.DATETIME_DEFAULT;
                    }
                    else
                    {
                        extendedSettings.AbsenceWholeLastDay = false;
                        extendedSettings.AbsenceLastDayStart = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, dateTo.TimeOfDay);
                    }
                    request.ExtendedAbsenceSetting = extendedSettings;
                }
                #endregion
            }

            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveEmployeeRequest(request, employeeId, TermGroup_EmployeeRequestType.AbsenceRequest, false, false);

            if (result.Success)
            {
                mobileRequest.SetTaskResult(MobileTask.SaveEmployeeRequest, true);
            }
            else
            {
                if (result.ErrorNumber > 0)
                    mobileRequest = new MobileRequest(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    mobileRequest = new MobileRequest(param, FormatMessage(Texts.SaveAbsenceRequestFailed, result.ErrorNumber));
            }

            return mobileRequest;
        }

        private MobileRequest PerformDeleteAbsenceRequest(MobileParam param, int absenceRequestId)
        {
            MobileRequest mobileRequest = new MobileRequest(param);

            var request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(absenceRequestId);

            if (request == null)
                return new MobileRequest(param, Texts.AbsenceRequestNotFoundMessage);

            if (request.Status != (int)TermGroup_EmployeeRequestStatus.RequestPending && request.Status != (int)TermGroup_EmployeeRequestStatus.Restored)
                return new MobileRequest(param, Texts.DeleteAbsenceRequestFailed);


            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).DeleteEmployeeRequest(absenceRequestId);

            if (result.Success)
            {
                mobileRequest.SetTaskResult(MobileTask.DeleteEmployeeRequest, true);
            }
            else
            {
                if (result.ErrorNumber > 0)
                    mobileRequest = new MobileRequest(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    mobileRequest = new MobileRequest(param, FormatMessage(Texts.DeleteAbsenceRequestFailed, result.ErrorNumber));
            }

            return mobileRequest;
        }

        #endregion

        #region InterestRequest
        private MobileRequests PerformGetInterestRequests(MobileParam param, int employeeId, DateTime? dateFrom, DateTime? dateTo)
        {
            List<TermGroup_EmployeeRequestType> requestTypes = new List<TermGroup_EmployeeRequestType>();
            requestTypes.AddRange(new[] { TermGroup_EmployeeRequestType.InterestRequest, TermGroup_EmployeeRequestType.NonInterestRequest });

            if (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_21))
            {
                dateFrom = null;
                dateTo = null;
            }

            List<EmployeeRequest> requests = TimeEngineManager(param.ActorCompanyId, param.UserId).GetEmployeeRequests(employeeId, null, requestTypes, dateFrom, dateTo).OrderByDescending(o => o.Start).ThenByDescending(o => o.Stop).ThenByDescending(o => o.Created).ToList();
            bool isMySelf = this.IsMySelf(param, employeeId, null);
            MobileRequests mobileRequests = new MobileRequests(param, requests, (isMySelf ? MobileDisplayMode.User : MobileDisplayMode.Admin), isMySelf);
            return mobileRequests;
        }

        private MobileRequest PerformGetInterestRequestEmployee(MobileParam param, int interestRequestId)
        {
            EmployeeRequest request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(interestRequestId);

            if (request == null)
                return new MobileRequest(param, Texts.InterestRequestNotFoundMessage);

            MobileRequest mobileRequest = new MobileRequest(param, request, true, MobileDisplayMode.User, new List<TimeSchedulePlanningDayDTO>());

            return mobileRequest;
        }

        private MobileRequest PerformSaveInterestRequest(MobileParam param, int employeeId, int interestRequestId, DateTime dateFrom, DateTime dateTo, bool available, string note, bool isAdmin)
        {
            EmployeeRequest request = null;
            MobileRequest mobileRequest = new MobileRequest(param);

            if (isAdmin)
            {
                if (!FeatureManager.HasRolePermission(Feature.Time_Schedule_Availability_EditOnOtherEmployees, Permission.Modify, param.RoleId, param.ActorCompanyId))
                    return new MobileRequest(param, GetText(8739, "Otillåten ändring, behörighet saknas"));
            }
            else if (available)
            {
                if (!FeatureManager.HasRolePermission(Feature.Time_Schedule_AvailabilityUser_Available, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    return new MobileRequest(param, GetText(8739, "Otillåten ändring, behörighet saknas"));
            }
            else
            {
                if (!FeatureManager.HasRolePermission(Feature.Time_Schedule_AvailabilityUser_NotAvailable, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    return new MobileRequest(param, GetText(8739, "Otillåten ändring, behörighet saknas"));
            }

            if (interestRequestId > 0)
            {
                request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(interestRequestId);

                if (request == null)
                    return new MobileRequest(param, Texts.InterestRequestNotFoundMessage);

                request.Comment = note;
                request.Start = dateFrom;
                request.Stop = dateTo;
                request.Type = available ? (int)TermGroup_EmployeeRequestType.InterestRequest : (int)TermGroup_EmployeeRequestType.NonInterestRequest;
            }
            else
            {
                request = new EmployeeRequest()
                {
                    EmployeeRequestId = interestRequestId,
                    ActorCompanyId = param.ActorCompanyId,
                    Comment = note,
                    EmployeeId = employeeId,
                    Start = dateFrom,
                    Stop = dateTo,
                    Type = available ? (int)TermGroup_EmployeeRequestType.InterestRequest : (int)TermGroup_EmployeeRequestType.NonInterestRequest,
                };
            }
            var editOrNew = new List<EmployeeRequestDTO>();

            var requestDTO = new EmployeeRequestDTO()
            {

                EmployeeRequestId = interestRequestId,
                Start = request.Start,
                Stop = request.Stop,
                Comment = request.Comment,
                Type = (TermGroup_EmployeeRequestType)request.Type

            };

            editOrNew.Add(requestDTO);
            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveEmployeeRequest(employeeId, new List<EmployeeRequestDTO>(), editOrNew);

            if (result.Success)
            {
                mobileRequest.SetTaskResult(MobileTask.SaveEmployeeRequest, true);
            }
            else
            {
                if (result.ErrorNumber > 0)
                    mobileRequest = new MobileRequest(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    mobileRequest = new MobileRequest(param, FormatMessage(Texts.SaveInterestRequestFailed, result.ErrorNumber));
            }

            return mobileRequest;
        }

        private MobileRequest PerformDeleteInterestRequest(MobileParam param, int employeeId, int employeeGroupId, int interestRequestId)
        {
            MobileRequest mobileRequest = new MobileRequest(param);

            var request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(interestRequestId);

            if (request == null)
                return new MobileRequest(param, Texts.InterestRequestNotFoundMessage);

            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).DeleteEmployeeRequest(interestRequestId);

            if (result.Success)
            {
                mobileRequest.SetTaskResult(MobileTask.DeleteEmployeeRequest, true);
            }
            else
            {
                if (result.ErrorNumber > 0)
                    mobileRequest = new MobileRequest(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    mobileRequest = new MobileRequest(param, FormatMessage(Texts.DeleteInterestRequestFailed, result.ErrorNumber));
            }

            return mobileRequest;
        }

        #endregion

        #region AbsenceAnnouncement

        private MobileDeviationCauses PerformGetAbsenceAnnouncementCauses(MobileParam param, int employeeId, int employeeGroupId)
        {
            Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId);
            if (employee == null)
                return new MobileDeviationCauses(param, Texts.EmployeeNotFoundMessage);

            List<TimeDeviationCause> causes = TimeDeviationCauseManager.GetTimeDeviationCausesAbsenceAnnouncements(param.ActorCompanyId, employeeGroupId);

            MobileDeviationCauses mobileCauses = new MobileDeviationCauses(param, causes);

            return mobileCauses;
        }

        private MobileAbsenceAnnouncement PerformSaveAbsenceAnnouncement(MobileParam param, DateTime date, int employeeId, int timeDeviationCauseId)
        {
            if (date.Date < DateTime.Now.Date)
                return new MobileAbsenceAnnouncement(param, GetText(8462, "Ej tillåtet att anmäla frånvaro bakåt i tiden."));

            if (date.Date > DateTime.Now.Date.AddDays(1))
                return new MobileAbsenceAnnouncement(param, GetText(8463, "Ej tillåtet att anmäla frånvaro så långt fram i tiden."));

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(entitiesReadOnly, employeeId, date.Date);
            if (shift == null)
                return new MobileAbsenceAnnouncement(param, GetText(8464, "Hittar inga pass för angivet datum"));

            if (shift.ShiftUserStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved)
                return new MobileAbsenceAnnouncement(param, GetText(8469, "Ej tillåtet att anmäla frånvaro på en dag som det redan har rapporterats frånvaro på."));

            ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).HandleTimeScheduleShift(HandleShiftAction.AbsenceAnnouncement, shift.TimeScheduleTemplateBlockId, timeDeviationCauseId, employeeId, 0, param.RoleId, false);

            if (result.Success)
            {
                MobileAbsenceAnnouncement announcement = new MobileAbsenceAnnouncement(param);
                announcement.SetTaskResult(MobileTask.SaveAbsenceAnnouncement, true);

                if (((ActionResultSave)result.SuccessNumber) == ActionResultSave.HandleTimeScheduleShift_AbsenceAnnouncementOKXEmailSentWithErrors)
                    announcement.successMsg = GetText(8466, "Din registrering är genomförd, dina pass under dagen har lagts som lediga pass.") + "\n" + GetText(8467, "Det gick dock inte att skicka ett meddelande till din närmaste chef! Glöm inte att prata med din närmaste chef.");
                else
                    announcement.successMsg = GetText(8465, "Din registrering är genomförd, dina pass under dagen har lagts som lediga pass. Glöm inte att prata med din närmaste chef.");

                return announcement;
            }
            else
            {
                string errorMsg = GetText(8468, "Kunde inte genomföra frånvaroanmälan") + ": " + "\n";

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    errorMsg += result.ErrorMessage;

                MobileAbsenceAnnouncement announcement = new MobileAbsenceAnnouncement(param, FormatMessage(errorMsg, result.ErrorNumber));
                return announcement;
            }
        }

        #endregion

        #region ShiftRequest

        private MobileShiftRequestUsers PerformGetShiftRequestUsers(MobileParam param, int shiftId, int employeeId)
        {
            MobileShiftRequestUsers users = new MobileShiftRequestUsers(param);
            List<MessageRecipientDTO> allItems = new List<MessageRecipientDTO>();
            TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, employeeId);

            if (shift == null)
                return new MobileShiftRequestUsers(param, GetText(8503, "Passet kunde inte hittas"));

            if (shift.ActualStartTime.HasValue && shift.ActualStopTime.HasValue)
            {
                List<UserRequestTypeDTO> result = UserManager.GetUsersByCompanyAndAvailability(param.ActorCompanyId, param.RoleId, param.UserId, shift.ActualStartTime.Value, shift.ActualStopTime.Value, false, true);

                foreach (var usr in result)
                {
                    allItems.Add(new MessageRecipientDTO() { EmployeeRequestType = (TermGroup_EmployeeRequestType)((int)usr.EmployeeRequestTypes), UserId = usr.UserId, IsSelected = false, Name = usr.Name, UserName = usr.LoginName, Type = XEMailRecipientType.User, IsVisible = true });
                }
            }

            users = new MobileShiftRequestUsers(param, allItems.Where(x => x.EmployeeRequestType == TermGroup_EmployeeRequestType.InterestRequest || x.EmployeeRequestType == TermGroup_EmployeeRequestType.Undefined).OrderByDescending(x => x.EmployeeRequestType).ToList());
            return users;
        }

        private MobileXeMail PerformSendShiftRequest(MobileParam param, int shiftId, int employeeId, string comment, string userIds)
        {
            //LogInfo("PerformSendShiftRequest- shiftId: " + shiftId);
            //LogInfo("PerformSendShiftRequest- employeeId: " + employeeId); 
            //LogInfo("PerformSendShiftRequest- comment: " + comment);
            //LogInfo("PerformSendShiftRequest- userids: " + userIds);

            MobileXeMail xemail = new MobileXeMail(param);
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileXeMail(param, Texts.UserNotFound);

            TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, employeeId);
            if (shift == null)
                return new MobileXeMail(param, GetText(8503, "Passet kunde inte hittas"));

            List<MessageRecipientDTO> receivers = new List<MessageRecipientDTO>();

            #region Parse reciepients

            char[] separator = new char[1];
            separator[0] = ',';

            string[] separatedUserIds = userIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var id in separatedUserIds)
            {
                if (Int32.TryParse(id.Trim(), out int userId))
                    receivers.Add(new MessageRecipientDTO() { Type = XEMailRecipientType.Employee, UserId = userId });
            }

            #endregion

            #region Create subject and body

            List<TimeScheduleTemplateBlock> shifts = TimeScheduleManager.GetLinkedTimeScheduleTemplateBlocks(null, param.ActorCompanyId, shiftId, true);
            shifts = shifts.OrderBy(s => s.ActualStartTime).ToList();

            string subject = shift.IsOrder() ? GetText(394, 1000, "Uppdragsförfrågan för") : GetText(377, 1000, "Passförfrågan för") + " " + shift.ActualStartTime.ToShortDateString();
            string text = "";
            foreach (TimeScheduleTemplateBlock shft in shifts)
            {
                string msg = String.Format("{0}-{1} {2}", shft.ActualStartTime.ToShortTimeString(), shft.ActualStopTime.ToShortTimeString(), shft.ShiftType != null ? shft.ShiftType.Name : String.Empty);
                if (shft.Account != null)
                    msg += String.Format(" ({0})", shft.Account.Name);
                if (shifts.Count == 1)
                    subject += ", " + msg;
                else
                    text += msg + "\n";
            }

            // Add comment from user
            if (comment.Length > 0)
            {
                if (text.Length > 0)
                    text += "\n\n";
                text += comment;
            }

            #endregion

            #region Create messageDto
            MessageEditDTO messageDto = new MessageEditDTO()
            {
                ParentId = 0,
                AnswerType = XEMailAnswerType.None,
                LicenseId = user.LicenseId,
                ActorCompanyId = param.ActorCompanyId,
                RoleId = param.RoleId,
                SenderUserId = param.UserId,
                SenderName = user.Name,
                SenderEmail = String.Empty,
                Subject = subject,
                Text = text,
                ShortText = text,
                Entity = SoeEntityType.TimeScheduleTemplateBlock,
                MessagePriority = TermGroup_MessagePriority.Normal,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.Text,
                MessageType = TermGroup_MessageType.ShiftRequest,
                RecordId = shiftId,
                Recievers = receivers,
                MarkAsOutgoing = false,
            };

            #endregion

            ActionResult result = CommunicationManager.SendXEMail(messageDto, param.ActorCompanyId, param.RoleId, param.UserId);

            if (result.Success)
                xemail.SetTaskResult(MobileTask.SendMail, true);
            else
            {
                if (result.ErrorNumber > 0)
                    xemail = new MobileXeMail(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    xemail = new MobileXeMail(param, Texts.SendMailFailed);
            }

            return xemail;
        }

        private MobileMessageBox PerformSendShiftRequestValidateWorkRules(MobileParam param, int shiftId, int employeeId, string userIds)
        {
            TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, employeeId);

            if (shift != null && shift.ActualStartTime.HasValue)
            {
                EvaluateWorkRulesActionResult workRuleResult = TimeScheduleManager.ShiftRequestCheckIfTooEarlyToSend(shift.ActualStartTime.Value, param.ActorCompanyId);
                if (workRuleResult.Result != null && !workRuleResult.Result.Success)
                    return new MobileMessageBox(param, false, false, false, false, string.Format(GetText(12523, "Skicka passförfrågan")), workRuleResult.Result.ErrorMessage);

                if (workRuleResult.EvaluatedRuleResults.Any())
                {
                    EvaluateWorkRuleResultDTO ruleResultRow = workRuleResult.EvaluatedRuleResults.FirstOrDefault();
                    if (ruleResultRow != null)
                    {
                        ruleResultRow.Action = TermGroup_ShiftHistoryType.ShiftRequest;
                        string workRuleString = CreateWorkByPassString(param, ruleResultRow, employeeId, 12523);
                        return new MobileMessageBox(param, workRuleResult.AllRulesSucceded, workRuleResult.CanUserOverrideRuleViolation, true, true, string.Format(GetText(12523, "Skicka passförfrågan")), ruleResultRow.ErrorMessage, ruleResult: workRuleString);
                    }
                }
            }

            return new MobileMessageBox(param, true, false, false, false, "", "");
        }

        private MobileShiftRequestStatus PerformGetShiftRequestStatus(MobileParam param, int shiftId)
        {
            try
            {
                var shiftRequest = TimeScheduleManager.GetShiftRequestStatus(shiftId, param.ActorCompanyId);
                if (shiftRequest == null)
                    return new MobileShiftRequestStatus(param, Texts.ShiftRequestNotFound);

                return new MobileShiftRequestStatus(param, shiftRequest);
            }
            catch (Exception e)
            {
                LogError("PerformGetShiftRequestStatus: " + " (" + shiftId + ") " + e.Message);
                return new MobileShiftRequestStatus(param, Texts.InternalErrorMessage + e.Message);
            }
        }

        private MobileResult PerformRemoveShiftRequestRecipient(MobileParam param, int shiftId, int recipientUserId)
        {
            try
            {
                ActionResult actionResult = TimeScheduleManager.RemoveRecipientFromShiftRequest(shiftId, param.ActorCompanyId, recipientUserId);

                var mobileResult = new MobileResult(param);
                if (actionResult == null)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, false);
                }
                else if (actionResult.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, true);
                }
                else
                {
                    mobileResult = new MobileResult(param, actionResult.ErrorMessage);
                }

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformRemoveShiftRequestRecipient: " + e.Message);
                return new MobileResult(param, Texts.InternalErrorMessage + e.Message);
            }
        }

        private MobileResult PerformUndoShiftRequest(MobileParam param, int shiftId)
        {
            try
            {
                ActionResult actionResult = TimeScheduleManager.UndoShiftRequest(shiftId, param.ActorCompanyId);

                var mobileResult = new MobileResult(param);
                if (actionResult == null)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, false);
                }
                else if (actionResult.Success)
                {
                    mobileResult.SetTaskResult(MobileTask.Update, true);
                }
                else
                {
                    mobileResult = new MobileResult(param, actionResult.ErrorMessage);
                }

                return mobileResult;
            }
            catch (Exception e)
            {
                LogError("PerformUndoShiftRequest: " + e.Message);
                return new MobileResult(param, Texts.InternalErrorMessage + e.Message);
            }
        }

        #endregion

        #region Delete shift

        private MobileExtendedShift PerformDeleteShift(MobileParam param, int shiftId, int employeeId, bool includeLinkedShifts)
        {
            MobileExtendedShift mobileExtendedShift = null;

            try
            {
                TimeScheduleTemplateBlock templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, employeeId);

                if (templateBlock == null)
                    return new MobileExtendedShift(param, GetText(8503, "Passet kunde inte hittas"));

                TimeSchedulePlanningDayDTO shiftDto = TimeScheduleManager.CreateTimeSchedulePlanningDayDTO(templateBlock, param.ActorCompanyId, 0);
                Guid? guid = shiftDto.Link;

                List<int> ids = new List<int>();

                if (includeLinkedShifts && guid.HasValue)
                {
                    List<TimeScheduleTemplateBlock> shifts = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(employeeId, templateBlock.Date.Value, excludeBreaks: true);

                    string linkStr = guid.Value.ToString();
                    shifts = shifts.Where(b => b.Link.Equals(linkStr)).ToList();

                    foreach (var item in shifts)
                    {
                        if (item.TimeDeviationCauseId.HasValue)
                            continue;

                        ids.Add(item.TimeScheduleTemplateBlockId);
                    }
                }
                else
                {
                    if (!templateBlock.TimeDeviationCauseId.HasValue)
                        ids.Add(templateBlock.TimeScheduleTemplateBlockId);
                }

                // ToDo: maybe we need to send in parameter includeOnDutyShifts in a proper way. Now we send in null.
                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).DeleteTimeScheduleShifts(ids, false, null, null);

                if (result.Success)
                {
                    mobileExtendedShift = new MobileExtendedShift(param);
                    mobileExtendedShift.SetTaskResult(MobileTask.DeleteShift, true);
                }
                else
                {
                    string errorMessage = GetText(8507, "Pass kunde inte tas bort") + ": " + "\n";

                    if (string.IsNullOrEmpty(result.ErrorMessage))
                        mobileExtendedShift = new MobileExtendedShift(param, FormatMessage(errorMessage, result.ErrorNumber));
                    else
                        mobileExtendedShift = new MobileExtendedShift(param, FormatMessage(errorMessage + result.ErrorMessage, result.ErrorNumber));
                }
            }
            catch (Exception e)
            {
                LogError("PerformDeleteShift: " + e.Message);
                mobileExtendedShift = new MobileExtendedShift(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileExtendedShift;
        }

        #endregion

        #region AbsenceRequest Planning
        private MobileRequests PerformGetAbsenceRequestsAdmin(MobileParam param, bool includeDefinitive)
        {
            try
            {
                List<EmployeeRequest> requests = TimeEngineManager(param.ActorCompanyId, param.UserId).GetEmployeeRequests(0, null, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }).OrderBy(o => o.Start).ThenBy(o => o.Stop).ThenBy(o => o.Created).ToList();
                if (!includeDefinitive)
                    requests = requests.Where(e => e.Status != (int)TermGroup_EmployeeRequestStatus.Definate).ToList();

                MobileRequests mobileRequests = new MobileRequests(param, requests, MobileDisplayMode.Admin);
                return mobileRequests;
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileRequests(param, GetText(8778, "Ett fel inträffade"));
            }
        }

        private MobileRequest PerformGetAbsenceRequestAdmin(MobileParam param, int absenceRequestId)
        {
            try
            {
                EmployeeRequest request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(absenceRequestId);
                if (request == null)
                    return new MobileRequest(param, Texts.AbsenceRequestNotFoundMessage);

                List<TimeSchedulePlanningDayDTO> affectedShifts;

                bool onlyNoReplacementIsElectable = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeOnlyNoReplacementIsSelectable, 0, param.ActorCompanyId, 0);
                bool setApprovedYesAsDefault = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeSetApprovedYesAsDefault, 0, param.ActorCompanyId, 0);

                if (request.Status == (int)TermGroup_EmployeeRequestStatus.RequestPending)
                    affectedShifts = TimeScheduleManager.GetAbsenceAffectedShifts(param.ActorCompanyId, param.UserId, request.EmployeeId, null, request.Start, request.Stop, request.TimeDeviationCauseId ?? 0, request.ExtendedAbsenceSetting, false).ToList();
                else
                    affectedShifts = TimeScheduleManager.GetAbsenceRequestAffectedShifts(param.ActorCompanyId, param.UserId, null, request.ToDTO(false, false), TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested, request.ExtendedAbsenceSetting).ToList();

                affectedShifts = affectedShifts.Where(x => !x.IsLended).ToList();
                foreach (var affectedShift in affectedShifts)
                {
                    affectedShift.EmployeeId = 0;
                    affectedShift.EmployeeName = "";

                    if (affectedShift.StartTime == affectedShift.StopTime || onlyNoReplacementIsElectable)//zero day
                    {
                        affectedShift.EmployeeId = Constants.NO_REPLACEMENT_EMPLOYEEID;
                        affectedShift.EmployeeName = GetText(8262, "Ingen ersättare");
                    }

                    if (setApprovedYesAsDefault)
                        affectedShift.ApprovalTypeId = (int)TermGroup_YesNo.Yes;
                }

                MobileRequest mobileRequest = new MobileRequest(param, request, true, MobileDisplayMode.Admin, affectedShifts);

                return mobileRequest;
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileRequest(param, GetText(8778, "Ett fel inträffade"));
            }
        }

        private MobileDicts PerformGetApprovalTypes(MobileParam param)
        {
            try
            {
                var terms = base.GetTermGroupDict(TermGroup.YesNo, GetLangId(), addEmptyRow: true);
                return new MobileDicts(param, terms);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileDicts(param, GetText(8778, "Ett fel inträffade"));
            }
        }

        private MobileMessageBox PerformValidateAbsenceRequestPlanning(MobileParam param, int absenceRequestId, string shifts)
        {
            MobileMessageBox mobileMessageBox = null;
            try
            {
                EmployeeRequest request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(absenceRequestId);
                if (request == null)
                    return new MobileMessageBox(param, Texts.AbsenceRequestNotFoundMessage);

                MobileSaveAbsenceRequestShifts saveShifts = new MobileSaveAbsenceRequestShifts(shifts);
                List<TimeSchedulePlanningDayDTO> shiftDtos;
                string errorMsg = "";

                if (!saveShifts.ParseSucceded)
                {
                    string message = GetText(8482, "Gick inte att läsa indata") + " : " + shifts;
                    LogError(message);
                    return new MobileMessageBox(param, message);
                }

                shiftDtos = AbsenceRequestPlanningCreateTimescheduleplanningDtos(param, saveShifts, request.EmployeeId, ref errorMsg);

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    return new MobileMessageBox(param, errorMsg);
                }
                else
                {
                    #region Validate

                    List<int> shiftIdsToEvaluate = saveShifts.GetShiftsToValidate().Select(x => x.Id).ToList();
                    EvaluateWorkRulesActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules(shiftDtos.Where(x => shiftIdsToEvaluate.Contains(x.TimeScheduleTemplateBlockId)).ToList(), request.EmployeeId, null);
                    if (result.Result.Success)
                    {
                        if (result.AllRulesSucceded)
                        {
                            #region Success

                            mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");

                            #endregion
                        }
                        else
                        {
                            #region Warning

                            String message = !String.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : String.Empty;
                            if (result.CanUserOverrideRuleViolation)
                                message += "\n" + GetText(8494, "Vill du fortsätta?");

                            string title = GetText(8495, "Pass bryter mot angivna arbetstidsregler");
                            mobileMessageBox = new MobileMessageBox(param, false, result.CanUserOverrideRuleViolation, true, result.CanUserOverrideRuleViolation, title, message);

                            #endregion
                        }
                    }
                    else
                    {
                        #region Failure

                        mobileMessageBox = new MobileMessageBox(param, result.Result.ErrorMessage);

                        #endregion
                    }

                    #endregion
                }
            }
            catch (Exception e)
            {
                LogError("PerformAbsencePlanningValidateWorkRules ( " + shifts + " ): " + e.Message);
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileMessageBox;
        }

        private MobileAbsenceRequestShift PerformSaveAbsenceRequestPlanning(MobileParam param, int absenceRequestId, string shifts, string comment)
        {
            EmployeeRequest request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(absenceRequestId);
            if (request == null || !request.TimeDeviationCauseId.HasValue)
                return new MobileAbsenceRequestShift(param, Texts.AbsenceRequestNotFoundMessage);

            MobileAbsenceRequestShift mobileAbsenceRequestShift = null;

            try
            {
                MobileSaveAbsenceRequestShifts saveShifts = new MobileSaveAbsenceRequestShifts(shifts);
                List<TimeSchedulePlanningDayDTO> shiftDtos;
                string errorMsg = "";

                if (!saveShifts.ParseSucceded)
                {
                    string message = GetText(8482, "Gick inte att läsa indata") + " : " + shifts;
                    LogError(message);
                    return new MobileAbsenceRequestShift(param, message);
                }

                if (saveShifts.IsShiftsMissingApproval())
                    return new MobileAbsenceRequestShift(param, GetText(8796, "Ett eller flera pass har ersättare men har inte godkänts"));

                if (saveShifts.IsShiftsMissingReplacements())
                    return new MobileAbsenceRequestShift(param, GetText(8797, "Ett eller flera godkända pass saknar ersättare"));

                if (saveShifts.NoShiftsAreApporoved())
                    return new MobileAbsenceRequestShift(param, GetText(8798, "Inga pass har godkänts"));

                shiftDtos = AbsenceRequestPlanningCreateTimescheduleplanningDtos(param, saveShifts, request.EmployeeId, ref errorMsg);

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    mobileAbsenceRequestShift = new MobileAbsenceRequestShift(param, errorMsg);
                }
                else
                {
                    // Old apps do not send comment
                    ActionResult result = null;
                    if (comment != null && !comment.Equals(request.Comment))
                    {
                        request.Comment = comment;
                        result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveEmployeeRequest(request, request.EmployeeId, TermGroup_EmployeeRequestType.AbsenceRequest, true, false);
                    }

                    if (result == null || result.Success)
                    {
                        List<int> shiftIdsToPlan = saveShifts.GetShiftsToPlan().Select(x => x.Id).ToList();
                        //Save shifts
                        result = TimeEngineManager(param.ActorCompanyId, param.UserId).PerformAbsenceRequestPlanningAction(absenceRequestId, shiftDtos.Where(x => shiftIdsToPlan.Contains(x.TimeScheduleTemplateBlockId)).ToList(), false, null);
                    }

                    if (result.Success)
                    {
                        mobileAbsenceRequestShift = new MobileAbsenceRequestShift(param);
                        mobileAbsenceRequestShift.SetTaskResult(MobileTask.SaveAbsenceRequestPlanning, true);
                    }
                    else
                    {
                        string msg = GetText(8509, "Frånvaro kunde inte sparas");
                        if (!string.IsNullOrEmpty(result.ErrorMessage))
                            msg += ": " + "\n" + result.ErrorMessage;

                        mobileAbsenceRequestShift = new MobileAbsenceRequestShift(param, FormatMessage(msg, result.ErrorNumber));
                    }
                }
            }
            catch (Exception e)
            {
                LogError("PerformSaveAbsenceRequestPlanning ( " + shifts + " ): " + e.Message);
                mobileAbsenceRequestShift = new MobileAbsenceRequestShift(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileAbsenceRequestShift;
        }

        #endregion

        #region Absence planning

        private MobileReplaceWithEmployees PerformGetReplaceWithEmployees(MobileParam param, bool addSearchEmployee)
        {
            //code copied from AbsenceRequestPlanning.xaml.cs
            MobileReplaceWithEmployees replaceWithEmployees;
            try
            {
                Dictionary<int, string> employees = new Dictionary<int, string>();
                bool onlyNoReplacementIsElectable = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeOnlyNoReplacementIsSelectable, 0, param.ActorCompanyId, 0);
                if (onlyNoReplacementIsElectable)
                {
                    employees.Add(0, " ");
                    employees.Add(Constants.NO_REPLACEMENT_EMPLOYEEID, GetText(8262, "Ingen ersättare"));
                }
                else
                {
                    employees.AddRange(this.SchedulePlanningGetEmployees(param, 0, MobileDisplayMode.Admin, showAll: false, includeSecondaryCategoriesOrAccounts: false, getHidden: true, addEmptyRow: false, addNoReplacementEmployee: true, addSearchEmployee: addSearchEmployee));
                }

                replaceWithEmployees = new MobileReplaceWithEmployees(param, employees);
            }
            catch (Exception e)
            {
                LogError("PerformGetReplaceWithEmployees: " + e.Message);
                replaceWithEmployees = new MobileReplaceWithEmployees(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return replaceWithEmployees;
        }

        /// <summary>
        /// Called when absence dialog is opened
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId">the employee that the user will register absence for</param>
        /// <param name="shiftId">the shift that user pressed on (for future use)</param>
        /// <param name="date"></param>
        /// <param name="wholeDay">if the user choose wholeday or part of day</param>
        /// <returns></returns>
        private MobileExtendedShifts PerformGetAbsenceAffectedShiftsOpenDialog(MobileParam param, int employeeId, int shiftId, DateTime date, bool wholeDay)
        {
            MobileExtendedShifts extendedShifts = new MobileExtendedShifts(param);
            try
            {
                var affectedShifts = TimeScheduleManager.GetAbsenceAffectedShifts(param.ActorCompanyId, param.UserId, employeeId, null, CalendarUtility.GetBeginningOfDay(date), CalendarUtility.GetEndOfDay(date), 0, null, false).ToList();
                affectedShifts = affectedShifts.Where(x => !x.IsLended).ToList();
                foreach (var affectedShift in affectedShifts)
                {
                    affectedShift.EmployeeId = 0;
                    affectedShift.EmployeeName = "";

                    if (affectedShift.StartTime == affectedShift.StopTime)//zero day
                    {
                        affectedShift.EmployeeId = Constants.NO_REPLACEMENT_EMPLOYEEID;
                        affectedShift.EmployeeName = GetText(8262, "Ingen ersättare");
                    }
                }

                extendedShifts.AddMobileExtendedShifts(affectedShifts, true);
            }
            catch (Exception e)
            {

                LogError("PerformGetAbsenceAffectedShiftsOpenDialog: " + e.Message);
                extendedShifts = new MobileExtendedShifts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return extendedShifts;

        }

        /// <summary>
        /// called when user presses button "get affected shifts"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId">the employee that the user will register absence for</param>
        /// <param name="deviationCauseId">choosen deviationcause</param>
        /// <param name="from">interval from</param>
        /// <param name="to">interval to</param>
        /// <param name="absenceFrom">absence from</param>
        /// <param name="absenceTo">absence to</param>
        /// <param name="wholeDay">if the user choose wholeday or part of day</param>
        /// <returns></returns>
        private MobileExtendedShifts PerformGetAbsenceAffectedShifts(MobileParam param, int employeeId, int deviationCauseId, DateTime from, DateTime to, DateTime absenceFrom, DateTime absenceTo, bool wholeDay, bool isTimeModule)
        {
            MobileExtendedShifts extendedShifts = new MobileExtendedShifts(param);

            try
            {
                if (deviationCauseId == 0)
                    return extendedShifts = new MobileExtendedShifts(param, "Orsak måste anges");

                if (absenceTo < absenceFrom)
                    return extendedShifts = new MobileExtendedShifts(param, "Angiven frånvaro slutar innan det börjar");

                ExtendedAbsenceSetting extendedAbsenceSettings = null;

                if (!wholeDay)
                {
                    extendedAbsenceSettings = new ExtendedAbsenceSetting()
                    {
                        AbsenceFirstAndLastDay = true,
                        AbsenceFirstDayStart = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, absenceFrom),
                        AbsenceLastDayStart = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, absenceTo),
                    };
                }

                var affectedShifts = TimeScheduleManager.GetAbsenceAffectedShifts(param.ActorCompanyId, param.UserId, employeeId, null, CalendarUtility.GetBeginningOfDay(from), CalendarUtility.GetEndOfDay(to), deviationCauseId, extendedAbsenceSettings, isTimeModule).ToList();
                affectedShifts = affectedShifts.Where(x => !x.IsLended).ToList();
                foreach (var affectedShift in affectedShifts)
                {
                    affectedShift.EmployeeId = 0;
                    affectedShift.EmployeeName = "";

                    if (isTimeModule || affectedShift.StartTime == affectedShift.StopTime)//zero day
                    {
                        affectedShift.EmployeeId = Constants.NO_REPLACEMENT_EMPLOYEEID;
                        affectedShift.EmployeeName = GetText(8262, "Ingen ersättare");
                    }
                }

                extendedShifts.AddMobileExtendedShifts(affectedShifts, true);

            }
            catch (Exception e)
            {

                LogError("PerformGetAbsenceAffectedShifts: " + e.Message);
                extendedShifts = new MobileExtendedShifts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return extendedShifts;

        }

        private MobileMessageBox PerformAbsencePlanningValidateWorkRules(MobileParam param, int employeeId, string shifts, bool isTimeModule)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                MobileSaveShifts saveShifts = new MobileSaveShifts(shifts, true, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_16), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_17), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31));
                List<TimeSchedulePlanningDayDTO> shiftDtos;
                string errorMsg = "";

                if (!saveShifts.ParseSucceded)
                {
                    string message = GetText(8482, "Gick inte att läsa indata") + " : " + shifts;
                    LogError(message);
                    return new MobileMessageBox(param, message);
                }

                shiftDtos = AbsencePlanningCreateTimescheduleplanningDtos(param, saveShifts, employeeId, isTimeModule, ref errorMsg);

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    //LogError(errorMsg + "Input: " + shifts);
                    mobileMessageBox = new MobileMessageBox(param, errorMsg);
                }
                else
                {
                    //validate workrules

                    EvaluateWorkRulesActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules(shiftDtos, employeeId, null);

                    if (result.Result.Success)
                    {
                        if (result.AllRulesSucceded)
                        {
                            #region Success

                            mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");

                            #endregion
                        }
                        else
                        {
                            #region Warning

                            String message = !String.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : String.Empty;
                            if (result.CanUserOverrideRuleViolation)
                                message += "\n" + GetText(8494, "Vill du fortsätta?");

                            string title = GetText(8495, "Pass bryter mot angivna arbetstidsregler");
                            mobileMessageBox = new MobileMessageBox(param, false, result.CanUserOverrideRuleViolation, true, result.CanUserOverrideRuleViolation, title, message);

                            #endregion
                        }
                    }
                    else
                    {
                        #region Failure

                        mobileMessageBox = new MobileMessageBox(param, result.Result.ErrorMessage);

                        #endregion
                    }

                }
            }
            catch (Exception e)
            {
                LogError("PerformAbsencePlanningValidateWorkRules ( " + shifts + " ): " + e.Message);
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }


            return mobileMessageBox;
        }

        private MobileExtendedShift PerformSaveAbsencePlanning(MobileParam param, int employeeId, int timeDeviationCauseId, int employeeChildId, string shifts, bool isTimeModule)
        {
            MobileExtendedShift mobileExtendedShift = null;

            //LogInfo("PerformSaveAbsencePlanning: shifts" + shifts);

            try
            {
                if (timeDeviationCauseId == 0)
                    return new MobileExtendedShift(param, GetText(8544, "Orsak måste anges"));

                var deviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(timeDeviationCauseId, param.ActorCompanyId, false);
                if (deviationCause == null)
                    return new MobileExtendedShift(param, GetText(8813, "Orsak kunde inte hittas"));

                if (deviationCause.SpecifyChild && employeeChildId <= 0)
                    return new MobileExtendedShift(param, GetText(8814, "Du måste ange barn"));

                MobileSaveShifts saveShifts = new MobileSaveShifts(shifts, false, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_16), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_17), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31));
                List<TimeSchedulePlanningDayDTO> shiftDtos;
                string errorMsg = "";

                if (!saveShifts.ParseSucceded)
                {
                    string message = GetText(8482, "Gick inte att läsa indata") + " : " + shifts;
                    LogError(message);
                    return new MobileExtendedShift(param, message);
                }

                if (!isTimeModule && saveShifts.IsShiftsMissingReplacements())
                    return new MobileExtendedShift(param, GetText(10263, "Går ej att spara, det finns fortfarande pass som måste ses över"));

                shiftDtos = AbsencePlanningCreateTimescheduleplanningDtos(param, saveShifts, employeeId, isTimeModule, ref errorMsg);

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    //LogError(errorMsg + "Input: " + shifts);
                    mobileExtendedShift = new MobileExtendedShift(param, errorMsg);
                }
                else
                {
                    //Save shifts
                    ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).GenerateAndSaveAbsenceFromStaffing(new EmployeeRequestDTO(employeeId, timeDeviationCauseId, employeeChildId), shiftDtos, true, false, null);

                    if (result.Success)
                    {
                        mobileExtendedShift = new MobileExtendedShift(param);
                        mobileExtendedShift.SetTaskResult(MobileTask.SaveAbsencePlanning, true);
                    }
                    else
                    {
                        string msg = GetText(8509, "Frånvaro kunde inte sparas");
                        if (!string.IsNullOrEmpty(result.ErrorMessage))
                            msg += ": " + "\n" + result.ErrorMessage;

                        mobileExtendedShift = new MobileExtendedShift(param, FormatMessage(msg, result.ErrorNumber));
                    }
                }
            }
            catch (Exception e)
            {
                LogError("PerformSaveAbsencePlanning ( " + shifts + " ): " + e.Message);
                mobileExtendedShift = new MobileExtendedShift(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }


            return mobileExtendedShift;
        }

        private MobileMessageBox PerformAbsencePlanningValidateSkills(MobileParam param, int employeeId, int timeDeviationCauseId, string shifts)
        {
            //Not implemented in XE yet

            return new MobileMessageBox(param, true, false, false, false, "", "");
        }

        #endregion

        #region Permissions/settings

        private MobileSettings PerformGetEditShiftsViewSettings(MobileParam param)
        {
            List<Tuple<string, string>> settings = new List<Tuple<string, string>>();

            bool useScheduleTypes = TimeScheduleManager.GetTimeScheduleTypes(param.ActorCompanyId, getAll: false).ToDTOs(false).Any();
            bool shiftTypeIsMandatory = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeShiftTypeMandatory, 0, param.ActorCompanyId, 0);
            bool useShiftsStartsAfterMidnight = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeCreateShiftsThatStartsAfterMidnigtInMobile, 0, param.ActorCompanyId, 0);
            bool useExtraShift = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningSetShiftAsExtra, 0, param.ActorCompanyId, 0);
            bool useSubstituteShift = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningSetShiftAsSubstitute, 0, param.ActorCompanyId, 0);
            bool useMultipleScheduleTypes = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseMultipleScheduleTypes, 0, param.ActorCompanyId, 0);

            int defaultDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, param.ActorCompanyId, 0);
            int nbrOfBreaks = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeMaxNoOfBrakes, 0, param.ActorCompanyId, 0);

            AccountDim accountDim = AccountManager.GetAccountDim(defaultDimId, param.ActorCompanyId);

            settings.Add(Tuple.Create("UseScheduleTypes", StringUtility.GetString(useScheduleTypes)));
            settings.Add(Tuple.Create("NbrOfBreaks", nbrOfBreaks.ToString()));
            settings.Add(Tuple.Create("ShiftTypeIsMandatory", StringUtility.GetString(shiftTypeIsMandatory)));
            settings.Add(Tuple.Create("useShiftsStartsAfterMidnight", StringUtility.GetString(useShiftsStartsAfterMidnight)));
            settings.Add(Tuple.Create("accountDimName", accountDim != null ? accountDim.Name : ""));
            settings.Add(Tuple.Create("useExtraShift", StringUtility.GetString(useExtraShift)));
            settings.Add(Tuple.Create("useSubstituteShift", StringUtility.GetString(useSubstituteShift)));
            settings.Add(Tuple.Create("useMultipleScheduleTypes", StringUtility.GetString(useMultipleScheduleTypes)));

            MobileSettings mobileSettings = new MobileSettings(param, settings);
            return mobileSettings;
        }

        /// <summary>
        /// Obs! EmployeeId is sent from the client
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="mobileDisplayMode"></param>
        /// <returns></returns>
        private MobilePermissions PerformGetStaffingPermissions(MobileParam param, MobileDisplayMode mobileDisplayMode)
        {
            List<Tuple<string, int>> features = new List<Tuple<string, int>>();

            if (mobileDisplayMode == MobileDisplayMode.User)
            {
                #region User/Employee

                bool showAbsenceAnnouncement = false;
                bool showAbsenceRequest = false;
                bool showInterestRequest = false;

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftAbsenceAnnouncement, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    showAbsenceAnnouncement = true;

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_AbsenceRequestsUser, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    showAbsenceRequest = true;

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_AvailabilityUser, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    showInterestRequest = true;

                if (showAbsenceAnnouncement)
                    features.Add(Tuple.Create("showAbsenceAnnouncement", 1));
                else
                    features.Add(Tuple.Create("showAbsenceAnnouncement", 0));

                if (showAbsenceRequest)
                    features.Add(Tuple.Create("showAbsenceRequest", 1));
                else
                    features.Add(Tuple.Create("showAbsenceRequest", 0));

                if (showInterestRequest)
                    features.Add(Tuple.Create("showInterestRequest", 1));
                else
                    features.Add(Tuple.Create("showInterestRequest", 0));

                //there is no permission for modifing ordershifts, so we use permission Billing_Order_PlanningUser instead.
                //in this cause only companies that uses orderplanning will have modifyOrderShift = 1 and not those that only uses scheduleplanning
                if (FeatureManager.HasRolePermission(Feature.Billing_Order_PlanningUser, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("modifyOrderShift", 1));
                else
                    features.Add(Tuple.Create("modifyOrderShift", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftWanted, Permission.Modify, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("showWantedOption", 1));
                else
                    features.Add(Tuple.Create("showWantedOption", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftUnwanted, Permission.Modify, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("showUnWantedOption", 1));
                else
                    features.Add(Tuple.Create("showUnWantedOption", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_AvailabilityUser_Available, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("showAvailable", 1));
                else
                    features.Add(Tuple.Create("showAvailable", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_AvailabilityUser_NotAvailable, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("showNotAvailable", 1));
                else
                    features.Add(Tuple.Create("showNotAvailable", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_SeeTimeScheduleTemplateBlockTasks, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("showTasks", 1));
                else
                    features.Add(Tuple.Create("showTasks", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftSwapEmployee, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("showSwapShifts", 1));
                else
                    features.Add(Tuple.Create("showSwapShifts", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Time_AttestUser_RestoreToSchedule, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("restoreToSchedule", 1));
                else
                    features.Add(Tuple.Create("restoreToSchedule", 0));

                #endregion

            }
            else if (mobileDisplayMode == MobileDisplayMode.Admin)
            {
                #region Admin

                bool sendShiftRequest = false;
                bool applyAbsenceOnShift = false;
                bool modifyShift = false;

                //Staffing admin       
                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanning, Permission.Modify, param.RoleId, param.ActorCompanyId) || FeatureManager.HasRolePermission(Feature.Billing_Order_Planning, Permission.Modify, param.RoleId, param.ActorCompanyId))
                {
                    sendShiftRequest = true;
                    applyAbsenceOnShift = true;
                    modifyShift = true;
                }

                if (sendShiftRequest)
                    features.Add(Tuple.Create("sendShiftRequest", 1)); // for avaliable shifts and others shifts
                else
                    features.Add(Tuple.Create("sendShiftRequest", 0));

                if (applyAbsenceOnShift)
                    features.Add(Tuple.Create("applyAbsenceOnShift", 1)); //for others shifts
                else
                    features.Add(Tuple.Create("applyAbsenceOnShift", 0));

                if (modifyShift)
                    features.Add(Tuple.Create("modifyShift", 1)); // for avaliable shifts and others shifts
                else
                    features.Add(Tuple.Create("modifyShift", 0));

                if (modifyShift)//For now
                    features.Add(Tuple.Create("showQueue", 1)); //show which employees is on the queue
                else
                    features.Add(Tuple.Create("showQueue", 0));

                if (modifyShift)//For now
                    features.Add(Tuple.Create("assignShiftFromQueue", 1));
                else
                    features.Add(Tuple.Create("assignShiftFromQueue", 0));

                if (modifyShift)//For now
                    features.Add(Tuple.Create("assignShift", 1));
                else
                    features.Add(Tuple.Create("assignShift", 0));

                if (TimeScheduleManager.TimeScheduleTaskExists(param.ActorCompanyId))
                    features.Add(Tuple.Create("showTasks", 1));
                else
                    features.Add(Tuple.Create("showTasks", 0));

                if (SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningSortQueueByLas, 0, param.ActorCompanyId, 0))
                    features.Add(Tuple.Create("showSortByLas", 1));
                else
                    features.Add(Tuple.Create("showSortByLas", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_Availability_EditOnOtherEmployees, Permission.Modify, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("editAvailability", 1));
                else
                    features.Add(Tuple.Create("editAvailability", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Time_Attest_RestoreToSchedule, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("restoreToSchedule", 1));
                else
                    features.Add(Tuple.Create("restoreToSchedule", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_OnDutyShifts, Permission.Modify, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("modifyOnDutyShifts", 1));
                else
                    features.Add(Tuple.Create("modifyOnDutyShifts", 0));

                if (FeatureManager.HasRolePermission(Feature.Time_Employee_EvacuationList_ShowAll, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                    features.Add(Tuple.Create("showAllEvacuationList", 1));
                else
                    features.Add(Tuple.Create("showAllEvacuationList", 0));

                #endregion
            }

            //User and admin
            if (SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.AbsenceRequestPlanningIncludeNoteInMessages, 0, param.ActorCompanyId, 0))
                features.Add(Tuple.Create("showIncludeNoteInMessages", 1));
            else
                features.Add(Tuple.Create("showIncludeNoteInMessages", 0));


            MobilePermissions mobileFeatures = new MobilePermissions(param, features);
            return mobileFeatures;
        }

        private MobileSettings PerformGetStaffingSettings(MobileParam param, MobileDisplayMode mobileDisplayMode)
        {
            List<Tuple<string, string>> settings = new List<Tuple<string, string>>();

            if (mobileDisplayMode == MobileDisplayMode.Admin)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                settings.Add(Tuple.Create("HEId", base.GetHiddenEmployeeIdFromCache(entitiesReadOnly, CacheConfig.Company(param.ActorCompanyId)).ToString()));
            }

            var mobileSettings = new MobileSettings(param, settings);

            return mobileSettings;
        }

        #endregion

        #region TimeStampAttendance

        private MobileTimeStampAttendancies PerformGetTimeStampAttendance(MobileParam param)
        {
            bool includeMissingEmployees = FeatureManager.HasRolePermission(Feature.Time_TimeStampAttendanceGauge_ShowNotStampedIn, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            var result = DashboardManager.GetTimeStampAttendance(param.ActorCompanyId, param.UserId, param.RoleId, TermGroup_TimeStampAttendanceGaugeShowMode.AllLast24Hours, onlyIn: true, includeMissingEmployees: includeMissingEmployees, isMobile: true).ToList();

            MobileTimeStampAttendancies attendance = new MobileTimeStampAttendancies(param, result);
            return attendance;
        }


        #endregion

        #region EvacuationList

        private EvacuationLists PerformGetEvacuationList(MobileParam param)
        {
            EvacuationLists evacuationLists = new EvacuationLists(param);
            List<int> shiftTypeIdsInput = null;
            try
            {
                if (FeatureManager.HasRolePermission(Feature.Time_Employee_EvacuationList, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                {

                    int defaultEmployeDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, param.ActorCompanyId, 0);
                    AccountDim accountDim = AccountManager.GetAccountDim(defaultEmployeDimId, param.ActorCompanyId);

                    bool showAll = FeatureManager.HasRolePermission(Feature.Time_Employee_EvacuationList_ShowAll, Permission.Readonly, param.RoleId, param.ActorCompanyId);
                    IEnumerable<TimeStampEntryDTO> timeStampAttendances = null;
                    List<int> employeeIds = SchedulePlanningGetEmployees(param, 0, MobileDisplayMode.Admin, showAll: showAll, getHidden: false, from: DateTime.Today, to: DateTime.Today, includeSecondaryCategoriesOrAccounts: true).Select(x => x.Key).ToList();

                    if (!showAll)
                        shiftTypeIdsInput = SchedulePlanningGetShiftTypes(param, 0, MobileDisplayMode.Admin, false, null).Select(x => x.Key).ToList();
                    if (!employeeIds.IsNullOrEmpty())
                        timeStampAttendances = TimeStampManager.GetTimeStampEvacuationList(param.ActorCompanyId, employeeIds).ToDTOs();
                    List<TimeSchedulePlanningAggregatedDayDTO> aggregatedDays = TimeScheduleManager.GetTimeSchedulePlanningShiftsAggregated_ByProcedure(param.ActorCompanyId, param.UserId, 0, param.RoleId, DateTime.Now, DateTime.Now, employeeIds, shiftTypeIdsInput, TimeSchedulePlanningDisplayMode.Admin, false, true, includePreliminary: false, timeScheduleScenarioHeadId: null, includeShiftRequest: true, includeOnDuty: false).ToList();
                    List<int> distinctEmployeeIds = aggregatedDays.Where(x => x != null).Select(x => x.EmployeeId).ToList();
                    if (!timeStampAttendances.IsNullOrEmpty())
                        distinctEmployeeIds.AddRange(timeStampAttendances.Where(s => !distinctEmployeeIds.Contains(s.EmployeeId)).Select(x => x.EmployeeId).Distinct().ToList());

                    distinctEmployeeIds = distinctEmployeeIds.Distinct().ToList();
                    List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(param.ActorCompanyId, distinctEmployeeIds);
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    List<ShiftType> shiftTypes = TimeScheduleManager.GetShiftTypes(entitiesReadOnly, param.ActorCompanyId);

                    var markings = RefreshEvacuationListMarkings(param, employeeIds);

                    foreach (Employee employee in employees)
                    {
                        var marking = markings.EvacuationListRow.Where(w => w.EmployeeId == employee.EmployeeId && w.State == (int)SoeEntityState.Active).ToList();
                        TimeStampEntryDTO timeStampAttendance = null;
                        TimeSchedulePlanningDayDTO dayDTO = null;
                        List<TimeStampEntryDTO> timeStampAttendanceList = timeStampAttendances.Where(w => w.EmployeeId == employee.EmployeeId).ToList();

                        if (timeStampAttendanceList.Count != 0)
                            timeStampAttendance = timeStampAttendanceList.OrderBy(o => o.Time).Last();

                        TimeSchedulePlanningAggregatedDayDTO employeeDay = aggregatedDays.FirstOrDefault(x => x.EmployeeId == employee.EmployeeId && (x.ScheduleStartTime.Date == DateTime.Today.Date || x.ScheduleStopTime.Date == DateTime.Today.Date));
                        if (employeeDay != null && employeeDay.DayDTOs.Any())
                        {
                            dayDTO = employeeDay.DayDTOs.FirstOrDefault(w => CalendarUtility.IsDatesOverlapping(w.StartTime, w.StopTime, DateTime.Now, DateTime.Now));
                            if (dayDTO == null)
                            {

                                TimeSchedulePlanningDayDTO dayDTOMin = employeeDay.DayDTOs.OrderBy(w => w.StartTime).FirstOrDefault();
                                TimeSchedulePlanningDayDTO dayDTOMax = employeeDay.DayDTOs.OrderByDescending(w => w.StartTime).FirstOrDefault();

                                if (dayDTOMin != null && dayDTOMin.StartTime > DateTime.Now)
                                    dayDTO = dayDTOMin;
                                else if (dayDTOMax != null && dayDTOMax.StopTime < DateTime.Now)
                                    dayDTO = dayDTOMax;

                            }
                        }
                        if (timeStampAttendance != null && dayDTO != null && timeStampAttendance.Type == TimeStampEntryType.Out && !timeStampAttendance.IsBreak && timeStampAttendance.Time < dayDTO.StartTime && timeStampAttendance.Time < DateTime.Now.AddHours(-2))
                            timeStampAttendance = null;

                        if (employeeDay == null && timeStampAttendance != null && (timeStampAttendance.Type == TimeStampEntryType.In || timeStampAttendance.IsBreak || (timeStampAttendance.Type == TimeStampEntryType.Out && timeStampAttendance.Time > DateTime.Now.AddHours(-2))))
                            evacuationLists.AddEvacuationLists(employee, timeStampAttendance, employeeDay, shiftTypes, dayDTO, accountDim, marking);
                        else if (employeeDay != null && dayDTO != null)
                            evacuationLists.AddEvacuationLists(employee, timeStampAttendance, employeeDay, shiftTypes, dayDTO, accountDim, marking);

                    }
                }
                else
                {
                    evacuationLists = new EvacuationLists(param, GetText(5973, "Behörighet saknas"));
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new EvacuationLists(param, GetText(8778, "Ett fel inträffade"));
            }
            return evacuationLists;
        }

        private UpdateEvacuationList PerformUpdateEvacuationListMarkings(MobileParam param, string employeeLíst, int headId)
        {
            UpdateEvacuationList evacuationLists;

            try
            {
                if (FeatureManager.HasRolePermission(Feature.Time_Employee_EvacuationList, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                {
                    if (employeeLíst == "")
                        return new UpdateEvacuationList(param, GetText(8778, "Ett fel inträffade"));

                    var inputDTO = CreateEvacuationListDTO(param, headId, employeeLíst);

                    if (!inputDTO.EvacuationListRow.Any() && headId == 0)
                        return new UpdateEvacuationList(param, GetText(8778, "Ett fel inträffade"));

                    ActionResult result = TimeScheduleManager.UpdateEvacuationList(inputDTO, headId, param.UserId, param.ActorCompanyId);

                    if (result.Success)
                    {
                        List<int> employeeIds = inputDTO.EvacuationListRow.Select(s => s.EmployeeId).ToList();
                        if (employeeIds.Any()) {
                            var markings = RefreshEvacuationListMarkings(param, employeeIds);
                            var updatedList = new EvacuationListMarkings(param, markings.EvacuationListRow, employeeIds, result.IntegerValue);
                            evacuationLists = new UpdateEvacuationList(param, updatedList, employeeIds, result.IntegerValue);
                        }
                        else
                        {
                            return new UpdateEvacuationList(param, GetText(8778, "Ett fel inträffade"));
                        }
                    }
                    else
                    {
                        evacuationLists = new UpdateEvacuationList(param, result.ErrorMessage);
                    }
                }
                else
                {
                    evacuationLists = new UpdateEvacuationList(param, GetText(5973, "Behörighet saknas"));
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new UpdateEvacuationList(param, GetText(8778, "Ett fel inträffade"));
            }
            return evacuationLists;
        }
  
        private EvacuationListDTO RefreshEvacuationListMarkings(MobileParam param, List<int> employeeIds)
        {
            if (!employeeIds.Any())
                return new EvacuationListDTO();

            return TimeScheduleManager.GetEvacuationListFromEmployeeIds(param.UserId, param.ActorCompanyId, employeeIds, DateTime.Now.AddHours(-12));
        }
        private EvacuationListHistorys PerformGetEvacuationListHistory(MobileParam param, int employeeId)
        {
            EvacuationListHistorys evacuationListHistorys = new EvacuationListHistorys(param);
            DateTime date = DateTime.Now.AddHours(-12);

            try
            {
                if (FeatureManager.HasRolePermission(Feature.Time_Employee_EvacuationList, Permission.Readonly, param.RoleId, param.ActorCompanyId))
                {
                    var dto = TimeScheduleManager.GetEvacuationListFromEmployeeIds(param.UserId, param.ActorCompanyId, employeeId.ObjToList(), date);
                    evacuationListHistorys = new EvacuationListHistorys(param, dto, employeeId);
                }
                       
                else
                {
                    evacuationListHistorys = new EvacuationListHistorys(param, GetText(5973, "Behörighet saknas"));
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new EvacuationListHistorys(param, GetText(8778, "Ett fel inträffade"));
            }
            return evacuationListHistorys;
        }
        

        #endregion

        #region TimeWorkAccountOptions

         private TimeWorkAccountOptions PerformGetTimeWorkAccountOptions(MobileParam param, int employeeId, int timeWorkAccountYearEmployeeId)
        {
            TimeWorkAccountOptions timeWorkAccountOptions = new TimeWorkAccountOptions(param);
            Dictionary<string, string> textDict = new Dictionary<string, string>();
            try
            {
                TimeWorkAccountYearEmployee yearEmployee = TimeWorkAccountManager.GetTimeWorkAccountYearEmployee(timeWorkAccountYearEmployeeId, employeeId, includeTimeWorkAccount: true, includeTimeWorkAccountYear: true);
                if (yearEmployee != null)
                {
                    decimal sum = yearEmployee.SpecifiedWorkingTimePromoted ?? yearEmployee.CalculatedWorkingTimePromoted;
                    string currency = GetText(92008, "kr");

                    textDict.Add("text1", String.Format(GetText(91998, "Underlag till ditt arbetstidskonto är {0} för tiden {1}"), sum.ToString() + " " + currency, yearEmployee.EarningStart.ToShortDateString() + " - " + yearEmployee.EarningStop.ToShortDateString()));
                    textDict.Add("text2", String.Format(GetText(92000, "Enligt avtal ska du senast {0} meddela er arbetsgivare hur du vill disponera pengarna"), yearEmployee.TimeWorkAccountYear.EmployeeLastDecidedDate.ToShortDateString()));
                    textDict.Add("choice1", GetText(92005, "Uttag som pensionspremie"));
                    textDict.Add("choice2", GetText(92006, "Uttag som betald ledighet"));
                    textDict.Add("choice3", GetText(92007, "Uttag som kontant ersättning"));
                    textDict.Add("choice2_extra", GetText(92003, "Tas ut före" +" "+ yearEmployee.TimeWorkAccountYear.PaidAbsenceStopDate.ToShortDateString()));
                    textDict.Add("choice3_extra", GetText(92004, "Utbetalas senast" + " " + yearEmployee.TimeWorkAccountYear.DirectPaymentLastDate.ToShortDateString()));
                    textDict.Add("currency", currency);

                    timeWorkAccountOptions.AddTimeWorkAccountOptions(yearEmployee, textDict);
                }
                else
                {
                    return new TimeWorkAccountOptions(param, GetText(91956, "Arbetstidskonto hittades inte"));
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new TimeWorkAccountOptions(param, GetText(8778, "Ett fel inträffade"));
            }
            return timeWorkAccountOptions;
        }

        private MobileResult PerformSetTimeWorkAccountOption(MobileParam param, int employeeId, int timeWorkAccountYearEmployeeId, int selectedWithdrawalMethod)
        {
            var saveResult = TimeWorkAccountManager.SaveTimeWorkAccountYearEmployeeChoice(timeWorkAccountYearEmployeeId, employeeId, selectedWithdrawalMethod);
            var mobileResult = new MobileResult(param);

            if (saveResult.Success)
            {
                string text = String.Format(GetText(92001, "Ditt val {0} har sparats."), base.GetTermGroupContent(TermGroup.TimeWorkAccountWithdrawalMethod).FirstOrDefault(x => x.Id == selectedWithdrawalMethod)?.Name);
                PerformSendMail(param, 0, 0, GetText(91955, "Arbetstidskonto"), text, param.UserId.ToString(), "", "", "", "", null, "", false);

                mobileResult.SetTaskResult(MobileTask.Update, true);
            }
                
            else
                return new MobileResult(param, Texts.SaveFailed + ": " + saveResult.ErrorMessage);

            return mobileResult;
        }

        #endregion

        #region Assign Available shift from queue

        private MobileMessageBox PerformAssignAvailableShiftFromQueueValidateSkills(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                mobileMessageBox = ValidateSkills(param, shiftId, false, MobileDisplayMode.Admin, employeeId);
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformAssignAvailableShiftFromQueueValidateSkills: shiftId = {0}, employeeId = {1}, Error= {2} ", shiftId, employeeId, e.Message));
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
            return mobileMessageBox;
        }

        private MobileMessageBox PerformAssignAvailableShiftFromQueueValidateWorkRules(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                var templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId);
                if (templateBlock == null)
                    return new MobileMessageBox(param, GetText(8503, "Passet kunde inte hittas"));

                if (!templateBlock.ActualStartTime.HasValue || !templateBlock.ActualStopTime.HasValue || templateBlock.IsBreak)
                    return new MobileMessageBox(param, GetText(8633, "Passet är ogiltigt för att flyttas"));

                DateTime start = templateBlock.ActualStartTime.Value;
                DateTime end = templateBlock.ActualStopTime.Value;
                EvaluateWorkRulesActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateDragShiftAgainstWorkRules(DragShiftAction.Move, shiftId, 0, start, end, employeeId, false, false, null, null, null, null, false, null, false);

                if (result.Result.Success)
                {
                    if (result.AllRulesSucceded)
                    {
                        #region Success

                        mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");

                        #endregion
                    }
                    else
                    {
                        #region Warning

                        String message = !String.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : String.Empty;
                        if (result.CanUserOverrideRuleViolation)
                            message += "\n" + GetText(8494, "Vill du fortsätta?");

                        string title = GetText(8495, "Pass bryter mot angivna arbetstidsregler");
                        mobileMessageBox = new MobileMessageBox(param, false, result.CanUserOverrideRuleViolation, true, result.CanUserOverrideRuleViolation, title, message);

                        #endregion
                    }
                }
                else
                {
                    #region Failure

                    mobileMessageBox = new MobileMessageBox(param, result.Result.ErrorMessage);

                    #endregion
                }
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformAssignAvailableShiftFromQueueValidateWorkRules: shiftId = {0}, employeeId = {1}, Error= {2} ", shiftId, employeeId, e.Message));
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
            return mobileMessageBox;
        }

        private MobileShift PerformAssignAvailableShiftFromQueue(MobileParam param, int shiftId, int employeeId)
        {
            MobileShift mobileShift = null;

            try
            {
                var templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId);
                if (templateBlock == null)
                    return new MobileShift(param, GetText(8503, "Passet kunde inte hittas"));

                if (!templateBlock.ActualStartTime.HasValue || !templateBlock.ActualStopTime.HasValue || templateBlock.IsBreak)
                    return new MobileShift(param, GetText(8633, "Passet är ogiltigt för att flyttas"));

                DateTime start = templateBlock.ActualStartTime.Value;
                DateTime end = templateBlock.ActualStopTime.Value;

                //Call timeengine
                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).DragTimeScheduleShift(DragShiftAction.Move, shiftId, 0, start, end, employeeId, false, false, null, false, 0, null, false, null, false, false, null, null, null, null, false, false, null);

                if (result.Success)
                {
                    mobileShift = new MobileShift(param);
                    mobileShift.SetTaskResult(MobileTask.AssignAvailableShift, true);
                }
                else
                {
                    string errorMessage = GetText(8644, "Kunde inte tilldela passet") + ": " + "\n";
                    errorMessage += GetErrorMessage((ActionResultSave)result.ErrorNumber, result.ErrorMessage);

                    mobileShift = new MobileShift(param, FormatMessage(errorMessage, result.ErrorNumber));
                }
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformAssignAvailableShiftFromQueue: shiftId = {0}, employeeId = {1}, Error= {2} ", shiftId, employeeId, e.Message));
                mobileShift = new MobileShift(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileShift;
        }

        private MobileShiftQueueList PerformGetShiftQueue(MobileParam param, int shiftId)
        {
            bool sortByLas = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningSortQueueByLas, 0, param.ActorCompanyId, 0);
            bool showEmploymentDays = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_17);
            bool showWantExtraShifts = !FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Contact_NoExtraShift, Permission.Readonly, param.RoleId, param.ActorCompanyId);
            List<TimeScheduleShiftQueueDTO> queue = TimeScheduleManager.GetShiftQueue(shiftId, TermGroup_TimeScheduleTemplateBlockQueueType.Unspecified, param.ActorCompanyId).ToList();
            return new MobileShiftQueueList(param, queue, sortByLas, showEmploymentDays, showWantExtraShifts);
        }

        #endregion

        #region Assign Available shift

        private MobileMessageBox PerformAssignShiftValidateSkills(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                mobileMessageBox = ValidateSkills(param, shiftId, true, MobileDisplayMode.Admin, employeeId);
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformAssignShiftValidateSkills: shiftId = {0}, employeeId = {1}, Error= {2} ", shiftId, employeeId, e.Message));
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
            return mobileMessageBox;
        }

        private MobileMessageBox PerformAssignShiftValidateWorkRules(MobileParam param, int shiftId, int employeeId)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                var templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId);
                if (templateBlock == null)
                    return new MobileMessageBox(param, GetText(8503, "Passet kunde inte hittas"));

                if (!templateBlock.ActualStartTime.HasValue || !templateBlock.ActualStopTime.HasValue || templateBlock.IsBreak)
                    return new MobileMessageBox(param, GetText(8633, "Passet är ogiltigt för att flyttas"));

                DateTime start = templateBlock.ActualStartTime.Value;
                DateTime end = templateBlock.ActualStopTime.Value;
                EvaluateWorkRulesActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateDragShiftAgainstWorkRules(DragShiftAction.Move, shiftId, 0, start, end, employeeId, false, false, null, null, null, null, false, null, true, false);

                if (result.Result.Success)
                {
                    if (result.AllRulesSucceded)
                    {
                        #region Success

                        mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");

                        #endregion
                    }
                    else
                    {
                        #region Warning

                        String message = !String.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : String.Empty;
                        if (result.CanUserOverrideRuleViolation)
                            message += "\n" + GetText(8494, "Vill du fortsätta?");

                        string title = GetText(8495, "Pass bryter mot angivna arbetstidsregler");
                        mobileMessageBox = new MobileMessageBox(param, false, result.CanUserOverrideRuleViolation, true, result.CanUserOverrideRuleViolation, title, message);

                        #endregion
                    }
                }
                else
                {
                    #region Failure

                    mobileMessageBox = new MobileMessageBox(param, result.Result.ErrorMessage);

                    #endregion
                }
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformAssignShiftValidateWorkRules: shiftId = {0}, employeeId = {1}, Error= {2} ", shiftId, employeeId, e.Message));
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
            return mobileMessageBox;
        }

        private MobileShift PerformAssignShift(MobileParam param, int shiftId, int employeeId)
        {
            MobileShift mobileShift = null;

            try
            {
                var templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId);
                if (templateBlock == null)
                    return new MobileShift(param, GetText(8503, "Passet kunde inte hittas"));

                if (!templateBlock.ActualStartTime.HasValue || !templateBlock.ActualStopTime.HasValue || templateBlock.IsBreak)
                    return new MobileShift(param, GetText(8633, "Passet är ogiltigt för att flyttas"));

                DateTime start = templateBlock.ActualStartTime.Value;
                DateTime end = templateBlock.ActualStopTime.Value;

                //Call timeengine
                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).DragTimeScheduleShift(DragShiftAction.Move, shiftId, 0, start, end, employeeId, true, false, null, false, 0, null, false, null, false, false, null, null, null, null, false, false, null);

                if (result.Success)
                {
                    mobileShift = new MobileShift(param);
                    mobileShift.SetTaskResult(MobileTask.AssignAvailableShift, true);
                }
                else
                {
                    string errorMessage = GetText(8644, "Kunde inte tilldela passet") + ": " + "\n";
                    errorMessage += GetErrorMessage((ActionResultSave)result.ErrorNumber, result.ErrorMessage);

                    mobileShift = new MobileShift(param, FormatMessage(errorMessage, result.ErrorNumber));
                }
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformAssignShift: shiftId = {0}, employeeId = {1}, Error= {2} ", shiftId, employeeId, e.Message));
                mobileShift = new MobileShift(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileShift;
        }

        #endregion

        #region Orderplanning

        private MobilePlanningData PerformGetPlanningData(MobileParam param, int orderId)
        {
            try
            {
                var order = InvoiceManager.GetCustomerInvoice(orderId);
                if (order == null)
                    return new MobilePlanningData(param, Texts.OrderNotFoundMessage);

                string shiftTypeName = string.Empty;
                if (order.ShiftTypeId.HasValue)
                {
                    var shiftType = TimeScheduleManager.GetShiftType(order.ShiftTypeId.Value);
                    if (shiftType != null)
                        shiftTypeName = shiftType.Name;
                }

                return new MobilePlanningData(param, order, shiftTypeName);
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformGetPlanningData: Error= {0} ", e.Message));
                return new MobilePlanningData(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobilePlanningData PerformSavePlanningData(MobileParam param, int orderId, int? shiftTypeId, DateTime? plannedStartDate, DateTime? plannedStopDate, int estimatedTime, int remainingTime, bool keepAsPlanned, int? priority)
        {
            MobilePlanningData mobilePlanningData = new MobilePlanningData(param);

            try
            {
                var order = InvoiceManager.GetCustomerInvoice(orderId);
                if (order == null)
                    return new MobilePlanningData(param, Texts.OrderNotFoundMessage);

                ActionResult result = InvoiceManager.SaveOrderPlanningData(orderId, shiftTypeId, plannedStartDate, plannedStopDate, estimatedTime, remainingTime, priority);
                if (result.Success)
                    mobilePlanningData.SetTaskResult(MobileTask.SavePlanningData, true);
                else
                    mobilePlanningData = new MobilePlanningData(param, FormatMessage(Texts.SaveFailed, result.ErrorNumber));
            }
            catch (Exception e)
            {
                LogError(string.Format("PeformSavePlanningData: Error= {0} ", e.Message));
                mobilePlanningData = new MobilePlanningData(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobilePlanningData;
        }

        private MobileShiftTypes PerformGetOrderShiftTypes(MobileParam param)
        {
            try
            {
                List<TermGroup_TimeScheduleTemplateBlockType> blockTypes = new List<TermGroup_TimeScheduleTemplateBlockType>();
                blockTypes.Add(TermGroup_TimeScheduleTemplateBlockType.Order);

                var shiftTypes = TimeScheduleManager.GetShiftTypesForUser(false, param.ActorCompanyId, param.RoleId, param.UserId, employeeId: 0, isAdmin: true, includeSecondaryCategories: true, blockTypes: blockTypes).ToDTOs().ToList();
                return new MobileShiftTypes(param, shiftTypes);
            }
            catch (Exception e)
            {
                LogError(string.Format("GetOrderShiftTypes: Error= {0} ", e.Message));
                return new MobileShiftTypes(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileDicts PerformGetOrderShiftOrders(MobileParam param, DateTime? anyDateInWeek)
        {
            DateTime? dateTo = anyDateInWeek;
            if (!dateTo.HasValue)
                dateTo = DateTime.Now.Date;

            Dictionary<int, string> result = new Dictionary<int, string>();
            var orders = TimeScheduleManager.GetUserUnscheduledOrders(param.ActorCompanyId, CalendarUtility.GetLastDateOfWeek(dateTo));

            foreach (var order in orders)
            {
                result.Add(order.InvoiceId, order.InvoiceNr + " (" + order.ActorName + ") ");
            }

            return new MobileDicts(param, result);
        }

        
        private MobileReloadOrderPlanningSchedule PerformReloadOrderPlanningSchedule(MobileParam param, int employeeId, DateTime date)
        {

            MobileReloadOrderPlanningSchedule mobileOrderSchedule = null;
            try
            {
                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadContactPerson: true);
                if (employee == null)
                    return new MobileReloadOrderPlanningSchedule(param, Texts.EmployeeNotFoundMessage);

                var employeeIds = new List<int>() { employeeId };

                var shifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, 0, date, date, employeeIds, TimeSchedulePlanningMode.OrderPlanning, TimeSchedulePlanningDisplayMode.User, false, true, false, includePreliminary: false);
                shifts = shifts.Where(x => x.ActualDate.Date == date.Date && x.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule).ToList();

                var breaksShift = shifts.FirstOrDefault();

                mobileOrderSchedule = new MobileReloadOrderPlanningSchedule(param, shifts, breaksShift?.GetBreaks());

            }
            catch (Exception e)
            {
                LogError(string.Format("PerformReloadOrderPlanningSchedule: Error= {0} ", e.Message));
                mobileOrderSchedule = new MobileReloadOrderPlanningSchedule(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileOrderSchedule;
        }

        private MobileOrderShift PerformGetOrderShift(MobileParam param, int shiftId, int orderId, int employeeId, DateTime date)
        {

            MobileOrderShift mobileOrderShift = null;
            try
            {
                var order = InvoiceManager.GetCustomerInvoice(orderId, loadActor: true, loadProject: true);
                if (order == null)
                    return new MobileOrderShift(param, Texts.OrderNotFoundMessage);

                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadContactPerson: true);
                if (employee == null)
                    return new MobileOrderShift(param, Texts.EmployeeNotFoundMessage);

                var employeeIds = new List<int>() { employeeId };

                var shifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, 0, date, date, employeeIds, TimeSchedulePlanningMode.OrderPlanning, TimeSchedulePlanningDisplayMode.User, false, true, false, includePreliminary: false);
                if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_42))
                {
                    shifts = shifts.Where(x => x.ActualDate.Date == date.Date).ToList();
                }
          

                var selectedShift = shifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == shiftId);
                if (shiftId > 0 && selectedShift == null) //shiftid is not always provided
                    return new MobileOrderShift(param, GetText(8503, "Passet kunde inte hittas"));

                ShiftType shiftType = null;
                if (selectedShift != null)
                    shiftType = TimeScheduleManager.GetShiftType(selectedShift.ShiftTypeId);
                else
                {
                    if (order.ShiftTypeId.HasValue)
                    {
                        shiftType = TimeScheduleManager.GetShiftType(order.ShiftTypeId.Value);
                    }
                }

                bool shiftTypeIsMandatory = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeShiftTypeMandatory, 0, param.ActorCompanyId, 0);
                int dayStartTime = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, 0, param.ActorCompanyId, 0);
                int dayEndTime = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewEndTime, 0, param.ActorCompanyId, 0);

                DateTime startTime;
                DateTime stopTime;
                var breaksShift = shifts.FirstOrDefault(); //only one shift is needed to calculate breaktime

                //See SetTimesBasedOnSchedule in EditOrderShiftDialogViewModel
                if (selectedShift != null)
                {
                    startTime = selectedShift.StartTime;
                    stopTime = selectedShift.StopTime;
                }
                else
                {
                    var firstShift = shifts.OrderBy(x => x.StartTime).FirstOrDefault();
                    var lastShift = shifts.OrderByDescending(x => x.StopTime).FirstOrDefault();
                    startTime = firstShift != null ? firstShift.StartTime : date.Date.AddMinutes(dayStartTime);
                    stopTime = lastShift != null ? lastShift.StopTime : date.Date.AddMinutes(dayEndTime);

                    int orderShiftLength = order != null ? order.RemainingTime : 0;
                    DateTime orderShiftEnd = startTime.AddMinutes(orderShiftLength);


                    int breakLength;
                    if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_42))
                        breakLength = breaksShift != null ? breaksShift.GetBreakTimeWithinShift(startTime, orderShiftEnd) : 0;
                    else
                        breakLength = breaksShift != null ? breaksShift.GetBreakLength(startTime, orderShiftEnd) : 0;


                    if (breakLength > 0)
                    {
                        orderShiftLength += breakLength;
                    }

                    if (stopTime > startTime.AddMinutes(orderShiftLength))
                        stopTime = startTime.AddMinutes(orderShiftLength);
                }

                mobileOrderShift = new MobileOrderShift(param, selectedShift, order, employee, shiftTypeIsMandatory, shiftType, startTime, stopTime);

            }
            catch (Exception e)
            {
                LogError(string.Format("PerformGetOrderShift: shiftId = {0}, orderId = {1}, Error= {2} ", shiftId, orderId, e.Message));
                mobileOrderShift = new MobileOrderShift(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileOrderShift;
        }

        private MobileMessageBox PerformSaveOrderShiftValidateSkills(MobileParam param, int employeeId, DateTime stopTime, int shiftTypeId)
        {
            MobileMessageBox mobileMessageBox = null;
            bool skillCantBeOverridden = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeSkillCantBeOverridden, param.UserId, param.ActorCompanyId, 0);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(param.ActorCompanyId));

            bool skillsMatch = true;
            if (hiddenEmployeeId != employeeId && shiftTypeId != 0)
            {
                bool isValid = TimeScheduleManager.EmployeeHasShiftTypeSkills(employeeId, shiftTypeId, stopTime);
                skillsMatch = isValid;
            }

            if (!skillsMatch)
            {
                string title = GetText(8480, "Observera");
                string message = GetText(8648, "Den anställde uppfyller inte ett eller flera av uppdragstypens krav på kompetenser.");
                if (skillCantBeOverridden)
                {
                    mobileMessageBox = new MobileMessageBox(param, false, false, true, false, title, message);
                }
                else
                {
                    message += GetText(8493, "Vill du spara ändå?");
                    mobileMessageBox = new MobileMessageBox(param, false, true, true, true, title, message);
                }
            }
            else
            {
                mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");
            }

            return mobileMessageBox;
        }

        private MobileMessageBox PerformSaveOrderShiftValidateWorkRules(MobileParam param, int shiftId, int orderId, int employeeId, DateTime startTime, DateTime stopTime, int shiftTypeId)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                List<TimeSchedulePlanningDayDTO> dtos;
                string errorMsg = "";

                dtos = SaveOrderShiftsCreateTimescheduleplanningDtos(param, shiftId, orderId, employeeId, startTime, stopTime, shiftTypeId, ref errorMsg);

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    mobileMessageBox = new MobileMessageBox(param, errorMsg);
                }
                else
                {
                    //validate workrules
                    EvaluateWorkRulesActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluatePlannedShiftsAgainstWorkRules(dtos, false, null);

                    if (result.Result.Success)
                    {
                        if (result.AllRulesSucceded)
                        {
                            #region Success

                            mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");

                            #endregion
                        }
                        else
                        {
                            #region Warning

                            String message = !String.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : String.Empty;
                            if (result.CanUserOverrideRuleViolation)
                                message += "\n" + GetText(8494, "Vill du fortsätta?");

                            string title = GetText(8495, "Pass bryter mot angivna arbetstidsregler");
                            mobileMessageBox = new MobileMessageBox(param, false, result.CanUserOverrideRuleViolation, true, result.CanUserOverrideRuleViolation, title, message);

                            #endregion
                        }
                    }
                    else
                    {
                        #region Failure

                        mobileMessageBox = new MobileMessageBox(param, result.Result.ErrorMessage);

                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformSaveOrderShiftValidateWorkRules: shiftId = {0}, orderId = {1}, employeeId= {2}, shiftTypeId = {3}, startTime = {4}, stopTime = {5}, Error= {6}", shiftId, orderId, employeeId, shiftTypeId, startTime.ToShortDateShortTimeString(), stopTime.ToShortDateShortTimeString(), e.Message));
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileMessageBox;
        }

        private MobileOrderShift PerformSaveOrderShift(MobileParam param, int shiftId, int orderId, int employeeId, DateTime startTime, DateTime stopTime, int shiftTypeId)
        {
            MobileOrderShift mobileOrderShift = null;
            try
            {
                string errorMsg = "";

                var dtos = SaveOrderShiftsCreateTimescheduleplanningDtos(param, shiftId, orderId, employeeId, startTime, stopTime, shiftTypeId, ref errorMsg);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    mobileOrderShift = new MobileOrderShift(param, errorMsg);
                }
                else
                {
                    //Save shifts
                    ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveOrderShift(dtos, false);

                    if (result.Success)
                    {
                        mobileOrderShift = new MobileOrderShift(param);
                        mobileOrderShift.SetTaskResult(MobileTask.SaveOrderShift, true);
                    }
                    else
                    {
                        string errorMessage = GetText(8497, "Misslyckades med att spara") + ": " + "\n";
                        errorMessage += GetErrorMessage((ActionResultSave)result.ErrorNumber, result.ErrorMessage);

                        mobileOrderShift = new MobileOrderShift(param, FormatMessage(errorMessage, result.ErrorNumber));
                    }
                }
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformSaveOrderShift: shiftId = {0}, orderId = {1}, employeeId= {2}, shiftTypeId = {3}, startTime = {4}, stopTime = {5}, Error= {6}", shiftId, orderId, employeeId, shiftTypeId, startTime.ToShortDateShortTimeString(), stopTime.ToShortDateShortTimeString(), e.Message));
                mobileOrderShift = new MobileOrderShift(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileOrderShift;
        }

        #endregion

        #region AbsenceRequestPolicy
        private MobileMessageBox PerformValidateAbsenceRequestPolicy(MobileParam param, int employeeId, int absenceRequestId, DateTime dateFrom, DateTime dateTo, int deviationCauseId)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                if (deviationCauseId == 0)
                    return new MobileMessageBox(param, GetText(8544, "Orsak måste anges"));

                EmployeeRequestDTO request = null;
                if (absenceRequestId == 0)
                {
                    request = new EmployeeRequestDTO()
                    {
                        EmployeeRequestId = absenceRequestId,
                        ActorCompanyId = param.ActorCompanyId,
                        EmployeeId = employeeId,
                        Start = CalendarUtility.GetBeginningOfDay(dateFrom),
                        Stop = CalendarUtility.GetEndOfDay(dateTo),
                        TimeDeviationCauseId = deviationCauseId,
                        Type = TermGroup_EmployeeRequestType.AbsenceRequest,
                    };
                }
                else
                {
                    request = TimeEngineManager(param.ActorCompanyId, param.UserId).LoadEmployeeRequest(absenceRequestId).ToDTO(false, false);
                }

                ActionResult result = TimeDeviationCauseManager.ValidateTimeDeviationCausePolicy(param.ActorCompanyId, request);
                if (result.Success)
                {
                    if (string.IsNullOrEmpty(result.InfoMessage))
                    {
                        #region Success

                        mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");

                        #endregion
                    }
                    else
                    {
                        #region Warning

                        string title = !String.IsNullOrEmpty(result.InfoMessage) ? result.InfoMessage : String.Empty;
                        String message = !String.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : String.Empty;

                        mobileMessageBox = new MobileMessageBox(param, false, result.CanUserOverride, true, result.CanUserOverride, title, message);

                        #endregion
                    }
                }
                else
                {
                    #region Failure

                    mobileMessageBox = new MobileMessageBox(param, result.ErrorMessage);

                    #endregion
                }

            }
            catch (Exception e)
            {
                LogError("PerformValidateAbsenceRequestPolicy: " + e.Message);
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }

            return mobileMessageBox;
        }

        #endregion

        #region Create new shift
        /// <summary>
        ///  Return available employees when creating new shift
        /// </summary>
        /// <param name="param"></param>
        /// <param name="date"></param>
        /// <param name="shifts"></param>
        /// <param name="filterOnShiftType"></param>
        /// <param name="filterOnAvailability"></param>
        /// <param name="filterOnSkills"></param>
        /// <param name="filterOnWorkRules"></param>
        /// <returns></returns>
        private MobileEmployeeList PerformGetCreateNewShiftsEmployeesNew(MobileParam param, DateTime date, string shifts, bool filterOnShiftType, bool filterOnAvailability, bool filterOnSkills, bool filterOnWorkRules, int? filterOnMessageGroupId = null)
        {
            MobileEmployeeList list = new MobileEmployeeList(param);
            try
            {

                MobileSaveShifts saveShifts = new MobileSaveShifts(shifts, false, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_16), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_17), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31));

                string errorMsg = "";

                if (!saveShifts.ParseSucceded)
                {
                    string message = GetText(8482, "Gick inte att läsa indata") + " : " + shifts;
                    LogError(message);
                    return new MobileEmployeeList(param, message);
                }

                List<TimeSchedulePlanningDayDTO> dtos = ValidateSaveShiftsCreateTimescheduleplanningDtos(param, saveShifts, 0, date, ref errorMsg);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    LogWarning(errorMsg + "Input: " + shifts);
                    return new MobileEmployeeList(param, errorMsg);
                }
                else
                {
                    if (dtos.Any())
                    {
                        int tempId = -1; //beacuse this is fake data
                        List<TimeScheduleTemplateBlockDTO> scheduleDTOs = new List<TimeScheduleTemplateBlockDTO>();
                        foreach (var dto in dtos)
                        {
                            TimeScheduleTemplateBlockDTO scheduleDto = new TimeScheduleTemplateBlockDTO()
                            {
                                TimeScheduleTemplateBlockId = tempId,
                                TimeScheduleEmployeePeriodId = -1, //beacuse this is fake data
                                Date = date.Date,
                                StartTime = CalendarUtility.GetScheduleTime(dto.StartTime, date.Date, dto.StartTime.Date),
                                StopTime = CalendarUtility.GetScheduleTime(dto.StopTime, dto.StartTime.Date, dto.StopTime.Date),
                                ShiftTypeId = dto.ShiftTypeId == 0 ? (int?)null : dto.ShiftTypeId,
                                AccountId = dto.AccountId,
                            };
                            scheduleDTOs.Add(scheduleDto);
                            tempId--;
                        }

                        List<BreakDTO> breakDTOs = dtos[0].GetBreaks();
                        foreach (var breakDTO in breakDTOs)
                        {
                            TimeScheduleTemplateBlockDTO scheduleDto = new TimeScheduleTemplateBlockDTO()
                            {
                                TimeScheduleTemplateBlockId = tempId,
                                TimeScheduleEmployeePeriodId = -1, //beacuse this is fake data
                                Date = date.Date,
                                StartTime = CalendarUtility.GetScheduleTime(breakDTO.StartTime, date.Date, breakDTO.StartTime.Date),
                                StopTime = CalendarUtility.GetScheduleTime(breakDTO.StopTime, breakDTO.StartTime.Date, breakDTO.StopTime.Date),
                                TimeCodeId = breakDTO.TimeCodeId,
                                BreakType = SoeTimeScheduleTemplateBlockBreakType.NormalBreak,
                                IsBreak = true,
                            };
                            scheduleDTOs.Add(scheduleDto);
                            tempId--;
                        }

                        List<int> employeeIds = this.SchedulePlanningGetEmployees(param, 0, MobileDisplayMode.Admin, showAll: false, includeSecondaryCategoriesOrAccounts: true, getHidden: false, from: date, to: date).Select(x => x.Key).ToList();
                        List<AvailableEmployeesDTO> availableEmployees = TimeEngineManager(param.ActorCompanyId, param.UserId).GetAvailableEmployees(new List<int>(), employeeIds, filterOnShiftType, filterOnAvailability, filterOnSkills, filterOnWorkRules, filterOnMessageGroupId, false, scheduleDTOs, true, true);
                        list.AddToMobileEmployeeList(availableEmployees);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileEmployeeList(param, GetText(8778, "Ett fel inträffade"));
            }

            return list;
        }

        #endregion

        #region Edit/Save Shifts
        /// <summary>
        /// Its called to open edit shift view
        /// </summary>
        /// <param name="employeeId"> employeeid on the shifts that the user clicked on </param>
        /// <param name="shiftId">the shift that the user clicked on</param>
        private MobileExtendedShifts PerformGetShifts(MobileParam param, int shiftId, int employeeId, DateTime date)
        {
            //LogInfo("PerformGetShifts shiftId: " + shiftId);
            //LogInfo("PerformGetShifts employeeId: " + employeeId);
            //LogInfo("PerformGetShifts date: " + date);
            try
            {
                Guid? guid = null;

                MobileExtendedShifts mobileShifts = new MobileExtendedShifts(param);
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            int hiddenemployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(param.ActorCompanyId));

                if (hiddenemployeeId == employeeId)
                {
                    if (shiftId == 0)
                    {
                        return mobileShifts;
                    }
                    else
                    {
                        TimeScheduleTemplateBlock templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, employeeId);
                        if (templateBlock != null)
                        {
                            if (!templateBlock.TimeScheduleEmployeePeriodId.HasValue)
                                return new MobileExtendedShifts(param, "Ogiltigt pass: passet saknar period"); //should never happen           

                            TimeSchedulePlanningDayDTO shiftDto = TimeScheduleManager.CreateTimeSchedulePlanningDayDTO(templateBlock, param.ActorCompanyId, 0);
                            guid = shiftDto.Link;
                        }
                    }
                }

                var blockTypes = new List<TermGroup_TimeScheduleTemplateBlockType>
                {
                    TermGroup_TimeScheduleTemplateBlockType.Schedule,
                    TermGroup_TimeScheduleTemplateBlockType.Order,
                    TermGroup_TimeScheduleTemplateBlockType.Booking,
                    TermGroup_TimeScheduleTemplateBlockType.Standby
                };

                if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31))
                {
                    blockTypes.Add(TermGroup_TimeScheduleTemplateBlockType.OnDuty);
                }

                List<TimeSchedulePlanningDayDTO> shifts = TimeScheduleManager.GetTimeScheduleShifts(param.ActorCompanyId, param.UserId, param.RoleId, employeeId, date, date, blockTypes, true, false, guid, timeScheduleScenarioHeadId: null, setShiftIsLended: true);
                mobileShifts.AddMobileExtendedShifts(shifts);
                var timeCodeBreaks = TimeCodeManager.GetTimeCodeBreaks(param.ActorCompanyId, true);

                if (shifts.Any())
                {
                    //only include breaks once
                    var firstShift = shifts.First();
                    if (firstShift.Break1Id != 0)
                    {
                        DateTime startTime = firstShift.Break1StartTime;
                        DateTime stopTime = startTime.AddMinutes(firstShift.Break1Minutes);
                        string timeCodeName = "";
                        bool belongsToPreviousDay = (firstShift.Break1StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days == 1;

                        var timecode = timeCodeBreaks.FirstOrDefault(x => x.TimeCodeId == firstShift.Break1TimeCodeId);
                        if (timecode != null)
                            timeCodeName = timecode.Name;

                        mobileShifts.AddMobileExtendedShiftBreak(param, firstShift.Break1Id, startTime, stopTime, belongsToPreviousDay, firstShift.EmployeeId, firstShift.EmployeeName, firstShift.Break1TimeCodeId, timeCodeName, 0, "", firstShift.TimeDeviationCauseId.HasValue, shifts.IsAssociatedShiftLended(firstShift.Break1Id));
                    }

                    if (firstShift.Break2Id != 0)
                    {
                        DateTime startTime = firstShift.Break2StartTime;
                        DateTime stopTime = startTime.AddMinutes(firstShift.Break2Minutes);
                        string timeCodeName = "";
                        bool belongsToPreviousDay = (firstShift.Break2StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days == 1;

                        var timecode = timeCodeBreaks.FirstOrDefault(x => x.TimeCodeId == firstShift.Break2TimeCodeId);
                        if (timecode != null)
                            timeCodeName = timecode.Name;

                        mobileShifts.AddMobileExtendedShiftBreak(param, firstShift.Break2Id, startTime, stopTime, belongsToPreviousDay, firstShift.EmployeeId, firstShift.EmployeeName, firstShift.Break2TimeCodeId, timeCodeName, 0, "", firstShift.TimeDeviationCauseId.HasValue, shifts.IsAssociatedShiftLended(firstShift.Break2Id));
                    }

                    if (firstShift.Break3Id != 0)
                    {
                        DateTime startTime = firstShift.Break3StartTime;
                        DateTime stopTime = startTime.AddMinutes(firstShift.Break3Minutes);
                        string timeCodeName = "";
                        bool belongsToPreviousDay = (firstShift.Break3StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days == 1;

                        var timecode = timeCodeBreaks.FirstOrDefault(x => x.TimeCodeId == firstShift.Break3TimeCodeId);
                        if (timecode != null)
                            timeCodeName = timecode.Name;

                        mobileShifts.AddMobileExtendedShiftBreak(param, firstShift.Break3Id, startTime, stopTime, belongsToPreviousDay, firstShift.EmployeeId, firstShift.EmployeeName, firstShift.Break3TimeCodeId, timeCodeName, 0, "", firstShift.TimeDeviationCauseId.HasValue, shifts.IsAssociatedShiftLended(firstShift.Break3Id));
                    }

                    if (firstShift.Break4Id != 0)
                    {
                        DateTime startTime = firstShift.Break4StartTime;
                        DateTime stopTime = startTime.AddMinutes(firstShift.Break4Minutes);
                        string timeCodeName = "";
                        bool belongsToPreviousDay = (firstShift.Break4StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days == 1;

                        var timecode = timeCodeBreaks.FirstOrDefault(x => x.TimeCodeId == firstShift.Break4TimeCodeId);
                        if (timecode != null)
                            timeCodeName = timecode.Name;

                        mobileShifts.AddMobileExtendedShiftBreak(param, firstShift.Break4Id, startTime, stopTime, belongsToPreviousDay, firstShift.EmployeeId, firstShift.EmployeeName, firstShift.Break4TimeCodeId, timeCodeName, 0, "", firstShift.TimeDeviationCauseId.HasValue, shifts.IsAssociatedShiftLended(firstShift.Break4Id));
                    }
                }
                return mobileShifts;
            }
            catch (Exception e)
            {
                base.LogError(e, this.log);
                return new MobileExtendedShifts(param, "Internt fel: " + e.Message);
            }
        }
        private MobileMessageBox PerformSaveShiftsValidateSkills(MobileParam param, int employeeId, DateTime actualDate, string shifts)
        {
            MobileMessageBox mobileMessageBox = null;

            MobileSaveShifts saveShifts = new MobileSaveShifts(shifts, false, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_16), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_17), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31));

            if (!saveShifts.ParseSucceded)
            {
                string message = GetText(8482, "Gick inte att läsa indata") + " : " + shifts;
                LogError(message);
                return new MobileMessageBox(param, message);
            }

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(param.ActorCompanyId));

            bool skillsMatch = true;
            if (hiddenEmployeeId != employeeId)
            {
                foreach (var parsedShift in saveShifts.ParsedShifts)
                {
                    if (parsedShift.IsDeleted || parsedShift.IsBreak)
                        continue;

                    if (parsedShift.ShiftTypeId != 0)
                    {
                        List<int> empIds = TimeScheduleManager.MatchEmployeesByShiftTypeSkills(parsedShift.ShiftTypeId, param.ActorCompanyId);
                        skillsMatch = empIds.Contains(parsedShift.EmployeeId);

                        if (!skillsMatch)
                            break;
                    }
                }
            }

            if (!skillsMatch)
            {
                string title = GetText(8480, "Observera");
                string message = GetText(8481, "Den anställde uppfyller inte kompetenskraven för ett eller flera pass.");
                message += GetText(8493, "Vill du spara ändå?");
                mobileMessageBox = new MobileMessageBox(param, false, true, true, true, title, message);
            }
            else
            {
                mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");
            }

            return mobileMessageBox;
        }

        private MobileMessageBox PerformSaveShiftsValidateWorkRules(MobileParam param, int employeeId, DateTime actualDate, string shifts)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                MobileSaveShifts saveShifts = new MobileSaveShifts(shifts, false, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_16), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_17), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31));
                List<TimeSchedulePlanningDayDTO> shiftDtos;
                string errorMsg = "";

                if (!saveShifts.ParseSucceded)
                {
                    string message = GetText(8482, "Gick inte att läsa indata") + " : " + shifts;
                    LogError(message);
                    return new MobileMessageBox(param, message);
                }

                shiftDtos = ValidateSaveShiftsCreateTimescheduleplanningDtos(param, saveShifts, employeeId, actualDate, ref errorMsg);

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    LogWarning(errorMsg + "Input: " + shifts);
                    mobileMessageBox = new MobileMessageBox(param, errorMsg);
                }
                else
                {
                    //validate workrules

                    EvaluateWorkRulesActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluatePlannedShiftsAgainstWorkRules(shiftDtos, false, null);

                    if (result.Result.Success)
                    {
                        if (result.AllRulesSucceded)
                        {
                            #region Success

                            mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");

                            #endregion
                        }
                        else
                        {
                            #region Warning

                            String message = !String.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : String.Empty;
                            if (result.CanUserOverrideRuleViolation)
                                message += "\n" + GetText(8494, "Vill du fortsätta?");

                            string title = GetText(8495, "Pass bryter mot angivna arbetstidsregler");
                            mobileMessageBox = new MobileMessageBox(param, false, result.CanUserOverrideRuleViolation, true, result.CanUserOverrideRuleViolation, title, message);

                            #endregion
                        }
                    }
                    else
                    {
                        #region Failure

                        mobileMessageBox = new MobileMessageBox(param, result.Result.ErrorMessage);

                        #endregion
                    }

                }
            }
            catch (Exception e)
            {
                LogError("PerformSaveShiftsValidateWorkRules ( " + shifts + " ): " + e.Message);
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }


            return mobileMessageBox;
        }

        private MobileExtendedShift PerformSaveShifts(MobileParam param, int employeeId, DateTime actualDate, string shifts)
        {
            //LogInfo("PerformSaveShifts shifts: " + shifts);
            //LogInfo("PerformSaveShifts actualDate: " + actualDate.ToShortDateString());
            MobileExtendedShift mobileExtendedShift = null;

            try
            {
                MobileSaveShifts saveShifts = new MobileSaveShifts(shifts, false, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_16), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_17), Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31));
                List<TimeSchedulePlanningDayDTO> shiftDtos;
                string errorMsg = "";

                if (!saveShifts.ParseSucceded)
                {
                    string message = GetText(8482, "Gick inte att läsa indata") + " : " + shifts;
                    LogError(message);
                    return new MobileExtendedShift(param, message);
                }

                shiftDtos = ValidateSaveShiftsCreateTimescheduleplanningDtos(param, saveShifts, employeeId, actualDate, ref errorMsg);

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    //LogError(errorMsg + "Input: " + shifts);
                    mobileExtendedShift = new MobileExtendedShift(param, errorMsg);
                }
                else
                {
                    //Save shifts
                    ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).SaveTimeScheduleShift("mobile", shiftDtos, true, false, false, 0, null);

                    if (result.Success)
                    {
                        mobileExtendedShift = new MobileExtendedShift(param);
                        mobileExtendedShift.SetTaskResult(MobileTask.SaveShifts, true);
                    }
                    else
                    {
                        string errorMessage = GetText(8497, "Misslyckades med att spara") + ": " + "\n";
                        switch ((ActionResultSave)result.ErrorNumber)
                        {
                            case ActionResultSave.TimeSchedulePlanning_ShiftIsNull:
                                errorMessage += GetText(8498, "Inget pass att spara");
                                break;
                            case ActionResultSave.TimeSchedulePlanning_UserNotFound:
                                errorMessage += GetText(8499, "Användare ej funnen");
                                break;
                            case ActionResultSave.TimeSchedulePlanning_HiddenEmployeeNotFound:
                                errorMessage += GetText(8500, "Planering ej aktiverad");
                                break;
                            case ActionResultSave.TimeSchedulePlanning_PeriodNotFound:
                                errorMessage += GetText(8501, "Anställd har ej aktiverat schema för dagen");
                                break;
                            case ActionResultSave.TimeSchedulePlanning_PreliminaryNotUpdated:
                                errorMessage += GetText(8502, "Kan ej ändra status på passen, transaktioner är attesterade");
                                break;
                            default:
                                errorMessage = result.ErrorMessage;
                                break;
                        }

                        mobileExtendedShift = new MobileExtendedShift(param, FormatMessage(errorMessage, result.ErrorNumber));
                    }
                }
            }
            catch (Exception e)
            {
                LogError("PerformSaveShifts ( " + shifts + " ): " + e.Message);
                mobileExtendedShift = new MobileExtendedShift(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }


            return mobileExtendedShift;
        }

        private MobileShiftTypes PerformGetShiftTypesForEditShiftView(MobileParam param, string selectedAccountIdsStr)
        {
            Dictionary<int, string> validShiftTypes = SchedulePlanningGetShiftTypes(param, 0, MobileDisplayMode.Admin, false, selectedAccountIdsStr);
            List<ShiftTypeDTO> shiftTypes = TimeScheduleManager.GetShiftTypes(param.ActorCompanyId, setTimeScheduleTypeName: true).ToDTOs().ToList();
            shiftTypes = shiftTypes.Where(st => validShiftTypes.ContainsKey(st.ShiftTypeId)).ToList();
            shiftTypes.Insert(0, new ShiftTypeDTO() { ShiftTypeId = 0, Name = "" });

            return new MobileShiftTypes(param, shiftTypes);
        }

        #endregion

        #region Swap shift
        private MobileScheduleSwapApproveView PerformMobileScheduleSwapApproveView(MobileParam param, int timeScheduleSwapRequestId, int employeeId, int userId)
        {
            TimeScheduleSwapApproveViewDTO dto;
            MobileSwapScheduleShifts initiatorShifts = new MobileSwapScheduleShifts(param);
            MobileSwapScheduleShifts swapWithShifts = new MobileSwapScheduleShifts(param);
            MobileSwapScheduleShifts currentinitiatorShifts = new MobileSwapScheduleShifts(param);
            MobileSwapScheduleShifts currentSwapWithShifts = new MobileSwapScheduleShifts(param);
            bool admin;
            bool isInitiator;

            try
            {

                dto = TimeScheduleManager.GetScheduleSwapApproveView(base.ActorCompanyId, timeScheduleSwapRequestId, userId, employeeId);
                if (dto == null)
                    return new MobileScheduleSwapApproveView(param, GetText(9998, "Passbytet är avslutat eller borttaget."));

                initiatorShifts.AddMobileSwapScheudleShifts(dto.InitiatorEmployeeRows);
                swapWithShifts.AddMobileSwapScheudleShifts(dto.SwapWithEmployeeRows);
                currentinitiatorShifts.AddMobileSwapScheudleShifts(dto.CurrentInitiatorEmployeeRows);
                currentSwapWithShifts.AddMobileSwapScheudleShifts(dto.CurrentSwapWithEmployeeRows);
                admin = dto.Admin;
                isInitiator = employeeId == dto.InitiatorEmployeeId;

            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileScheduleSwapApproveView(param, GetText(8778, "Ett fel inträffade"));
            }
            if (admin)
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                TimeScheduleSwapLengthComparisonDTO timeScheduleSwapLengthComparisonDTO = TimeScheduleManager.GetScheduleSwapLengthComparisonInfoFromRequest(entities, param.ActorCompanyId, timeScheduleSwapRequestId, dto.InitiatorEmployeeId);
                if (timeScheduleSwapLengthComparisonDTO != null && timeScheduleSwapLengthComparisonDTO.Type != ScheduleSwapLengthComparisonType.Equal)
                {
                    dto.DifferentLength = true;
                    dto.DifferentLengthMessage = GetText(110700, "Berörda pass har olika längd.");
                }
                   
            }

            return new MobileScheduleSwapApproveView(param, timeScheduleSwapRequestId, dto, initiatorShifts, swapWithShifts, currentinitiatorShifts, currentSwapWithShifts, admin, isInitiator);
        }


        private MobileShifts PerformGetShiftsForSwap(MobileParam param, string initiatorShiftIdsStr, string swapWithShiftIdsStr, int employeeId, int employeeIdToView, DateTime initiatorShiftDate, DateTime swapWithShiftDate, MobileDisplayMode mobileDisplayMode)
        {
            MobileShifts mobileShifts = new MobileShifts(param);

            try
            {
                List<int> initiatorShiftIds = GetIds(initiatorShiftIdsStr);
                List<int> swapWithShiftIds = GetIds(swapWithShiftIdsStr);

                bool selectingMyShifts = employeeId == employeeIdToView;
                List<TimeSchedulePlanningDayDTO> employeeShifts = new List<TimeSchedulePlanningDayDTO>();

                if (mobileDisplayMode == MobileDisplayMode.User)
                {
                    //forceFilterOnAccounts = true, because the employees are only allowed to swap shifts based on there accounts
                    employeeShifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, 0, selectingMyShifts ? initiatorShiftDate : swapWithShiftDate, selectingMyShifts ? initiatorShiftDate : swapWithShiftDate, new List<int> { employeeIdToView }, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, true, true, false, includePreliminary: false, checkToIncludeDeliveryAdress: false, timeScheduleScenarioHeadId: null, setSwapShiftInfo: true, forceFilterOnAccounts: true);
                    if (!selectingMyShifts)
                    {
                        List<TimeSchedulePlanningDayDTO> initiatorShiftsThatSwapWithEmployeeCanSee = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, employeeIdToView, 0, initiatorShiftDate, initiatorShiftDate, new List<int> { employeeId }, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, true, true, false, includePreliminary: false, checkToIncludeDeliveryAdress: false, timeScheduleScenarioHeadId: null, setSwapShiftInfo: true, forceFilterOnAccounts: true);
                        if (!initiatorShiftsThatSwapWithEmployeeCanSee.Select(x => x.TimeScheduleTemplateBlockId).ToList().ContainsAll(initiatorShiftIds))
                        {
                            //Swapwithemployee can not "see" the shift that the initiator wants to swap, clear all shifts so that initiator cant choose any shifts of swapwithemployee
                            employeeShifts.Clear();
                        }
                    }
                }
                else if (mobileDisplayMode == MobileDisplayMode.Admin)
                {
                    //TODO?                    
                }

                employeeShifts = employeeShifts.Where(x => selectingMyShifts ? x.ActualDate == initiatorShiftDate.Date : x.ActualDate == swapWithShiftDate.Date && x.ShiftStatus == TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned && !x.IsAbsenceRequest).OrderBy(x => x.StartTime).ToList();
                mobileShifts.AddMobileShifts(employeeShifts, MobileShiftGUIType.SwapShift, false, "", false, false, false, false, mobileDisplayMode, null, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_25));
                mobileShifts.SetSelectedId(selectingMyShifts ? initiatorShiftIds : swapWithShiftIds);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileShifts(param, GetText(8778, "Ett fel inträffade"));
            }

            return mobileShifts;
        }

        private MobileResult PerformInitiateScheduleSwap(MobileParam param, int initiatorEmployeeId, DateTime initiatorShiftDate, string initiatorShiftIds, int swapWithEmployeeId, DateTime swapShiftDate, string swapWithShiftIds, string comment)
        {
            try
            {
                if (initiatorEmployeeId <= 0 || swapWithEmployeeId <= 0)
                    return new MobileResult(param, Texts.IncorrectInputMessage);

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).InitiateScheduleSwap(initiatorEmployeeId, initiatorShiftDate, GetIds(initiatorShiftIds), swapWithEmployeeId, swapShiftDate, GetIds(swapWithShiftIds), comment);
                return new MobileResult(param, result);

            }
            catch (Exception e)
            {
                LogError("PerformInitiateScheduleSwap: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel"));
            }
        }

        private MobileDicts PerformGetScheduleSwapAvailableEmployees(MobileParam param, int initiatorEmployeeId, DateTime initiatorShiftDate, DateTime swapShiftDate)
        {
            try
            {
                Dictionary<int, string> dict = EmployeeManager.GetScheduleSwapAvailableEmployees(param.ActorCompanyId, param.UserId, param.RoleId, initiatorEmployeeId, initiatorShiftDate, swapShiftDate);
                MobileDicts mobileDicts = new MobileDicts(param, dict);
                return mobileDicts;
            }
            catch (Exception e)
            {
                LogError("PerformGetScheduleSwapAvailableEmployees: " + e.Message);
                return new MobileDicts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileResult PerformApproveScheduleSwap(MobileParam param, int userId, int timeScheduleSwapRequestId, bool approved, string comment)
        {
            try
            {
                if (userId <= 0 || timeScheduleSwapRequestId <= 0)
                    return new MobileResult(param, Texts.IncorrectInputMessage);

                ActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).ApproveScheduleSwap(userId, timeScheduleSwapRequestId, approved, comment);
                return new MobileResult(param, result);

            }
            catch (Exception e)
            {
                LogError("PerformApproveScheduleSwap: " + e.Message);
                return new MobileResult(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        private MobileMessageBox PerformAssignScheduleSwapValidateWorkRules(MobileParam param, int timeScheduleSwapRequestId)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                TimeScheduleSwapRequestDTO timeScheduleSwap = TimeScheduleManager.GetEmployeeSwapRequestRowById(param.ActorCompanyId, timeScheduleSwapRequestId);
                if (timeScheduleSwap == null)
                    return new MobileMessageBox(param, GetText(8503, "Passet kunde inte hittas"));

                EvaluateWorkRulesActionResult result = TimeEngineManager(param.ActorCompanyId, param.UserId).EvaluateScheduleSwapAgainstRules(timeScheduleSwapRequestId);

                if (result.Result.Success)
                {
                    if (result.AllRulesSucceded)
                    {
                        #region Success

                        mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");

                        #endregion
                    }
                    else
                    {
                        #region Warning

                        String message = !String.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : String.Empty;
                        if (result.CanUserOverrideRuleViolation)
                            message += "\n" + GetText(8494, "Vill du fortsätta?");

                        string title = GetText(8495, "Pass bryter mot angivna arbetstidsregler");
                        mobileMessageBox = new MobileMessageBox(param, false, result.CanUserOverrideRuleViolation, true, result.CanUserOverrideRuleViolation, title, message);

                        #endregion
                    }
                }
                else
                {
                    #region Failure

                    mobileMessageBox = new MobileMessageBox(param, result.Result.ErrorMessage);

                    #endregion
                }
            }
            catch (Exception e)
            {
                LogError(string.Format("PerformAssignAvailableShiftFromQueueValidateWorkRules: shiftId = {0}, employeeId = {1}, Error= {2} ", 0, 0, e.Message));
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
            return mobileMessageBox;
        }

        private MobileMessageBox PerformGetScheduleSwapValidateLengths(MobileParam param, string sourceScheduleBlockIds, string targetScheduleBlockIds)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                List<int> sourceScheduleBlockIdsList = GetIds(sourceScheduleBlockIds);
                List<int> targetScheduleBlockIdsList = GetIds(targetScheduleBlockIds);
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                TimeScheduleSwapLengthComparisonDTO timeScheduleSwapLengthComparisonDTO = TimeScheduleManager.GetScheduleSwapLengthComparisonInfo(entities, param.ActorCompanyId,sourceScheduleBlockIdsList, targetScheduleBlockIdsList);
                if (timeScheduleSwapLengthComparisonDTO == null) 
                {
                    mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel"));
                }
                else
                {
                    mobileMessageBox = new MobileMessageBox(param, timeScheduleSwapLengthComparisonDTO.Type == ScheduleSwapLengthComparisonType.Equal, true, true, true, timeScheduleSwapLengthComparisonDTO.Title, timeScheduleSwapLengthComparisonDTO.Message);
                }
                
            }

            catch (Exception e)
            {
                LogError(string.Format("PerformGetScheduleSwapValidateLengths: shiftId = {0}, employeeId = {1}, Error= {2} ", 0, 0, e.Message));
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
            return mobileMessageBox;
        }
        private MobileMessageBox PerformGetScheduleSwapValidateLengthsFromRequest(MobileParam param, int timeScheduleSwapRequestId, int employeeId)
        {
            MobileMessageBox mobileMessageBox = null;

            try
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                TimeScheduleSwapLengthComparisonDTO timeScheduleSwapLengthComparisonDTO = TimeScheduleManager.GetScheduleSwapLengthComparisonInfoFromRequest(entities, param.ActorCompanyId, timeScheduleSwapRequestId, employeeId);
                if (timeScheduleSwapLengthComparisonDTO == null)
                {
                    mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel"));
                }
                else
                {
                    mobileMessageBox = new MobileMessageBox(param, timeScheduleSwapLengthComparisonDTO.Type == ScheduleSwapLengthComparisonType.Equal, true, true, true, timeScheduleSwapLengthComparisonDTO.Title, timeScheduleSwapLengthComparisonDTO.Message);
                }

            }

            catch (Exception e)
            {
                LogError(string.Format("PerformGetScheduleSwapValidateLengths: shiftId = {0}, employeeId = {1}, Error= {2} ", 0, 0, e.Message));
                mobileMessageBox = new MobileMessageBox(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
            return mobileMessageBox;
        }
        #endregion

        #region Views

        #region Employee
        /// <summary>
        /// Used for My shiftflow (passflöde)
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="dateFrom"></param>
        /// <returns></returns>
        private MobileShifts PerformGetShiftFlow(MobileParam param, int employeeId, DateTime dateFrom)
        {
            MobileShifts mobileShifts = new MobileShifts(param);
            try
            {
                List<DateTime> datesWithSchedule = TimeScheduleManager.GetScheduledDays(employeeId, dateFrom, 15);
                if (!datesWithSchedule.Any())
                    return mobileShifts;

                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadContactPerson: true);
                if (employee == null)
                    return new MobileShifts(param, Texts.EmployeeNotFoundMessage);

                List<TimeScheduleTemplateBlock> shifts = TimeScheduleManager.GetShifts(employeeId, datesWithSchedule);
                shifts = shifts.Where(x => x.Date.HasValue && x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && x.ShiftStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned).ToList();

                foreach (var shiftsByDate in shifts.GroupBy(x => x.Date.Value).OrderBy(x => x.Key))
                {
                    DateTime scheduleIn = shiftsByDate.ToList().GetScheduleIn();
                    DateTime scheduleOut = shiftsByDate.ToList().GetScheduleOut();
                    bool isPartTimeAbsence = shiftsByDate.ToList().IsPartTimeAbsence();
                    bool isWholeDayAbsence = shiftsByDate.ToList().IsWholeDayAbsence();
                    mobileShifts.AddAggregatedShift(param, scheduleIn, scheduleOut, shiftsByDate.Key, employeeId, employee.Name, isPartTimeAbsence, isWholeDayAbsence, MobileShiftGUIType.MyShiftFlow, false, false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileShifts(param, GetText(8778, "Ett fel inträffade"));
            }

            return mobileShifts;
        }

        /// <summary>
        /// Returns data for the view "Mitt grundschema"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        private MobileTemplateScheduleViewMonth PerformGetTemplateScheduleViewEmployee(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            MobileTemplateScheduleViewMonth templateScheduleView = new MobileTemplateScheduleViewMonth(param);

            if (SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ShowTemplateScheduleForEmployeesInApp, 0, param.ActorCompanyId, 0))
            {
                try
                {
                    dateFrom = CalendarUtility.GetFirstDateOfWeek(dateFrom);
                    dateTo = CalendarUtility.GetLastDateOfWeek(dateTo);

                    Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadContactPerson: true);
                    if (employee == null)
                        return new MobileTemplateScheduleViewMonth(param, Texts.EmployeeNotFoundMessage);

                    bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);
                    List<TimeSchedulePlanningAggregatedDayDTO> aggregatedDays = TimeScheduleManager.GetTimeSchedulePlanningTemplateAggregated(param.ActorCompanyId, param.RoleId, param.UserId, employeeId, dateFrom, dateTo, includeOnDuty: includeOnDuty);
                    //Add zero days
                    while (dateFrom <= dateTo)
                    {
                        var aggDay = aggregatedDays.FirstOrDefault(x => x.Date == dateFrom);
                        if (aggDay == null)
                        {
                            var zeroShift = new TimeSchedulePlanningDayDTO();
                            zeroShift.StartTime = dateFrom;
                            zeroShift.StopTime = dateFrom;
                            zeroShift.WeekNr = CalendarUtility.GetWeekNr(dateFrom);
                            var shifts = new List<TimeSchedulePlanningDayDTO>();
                            shifts.Add(zeroShift);
                            var zeroDay = new TimeSchedulePlanningAggregatedDayDTO(shifts);

                            aggregatedDays.Add(zeroDay);
                        }

                        dateFrom = dateFrom.AddDays(1);
                    }
                    aggregatedDays = aggregatedDays.OrderBy(x => x.Date).ToList();
                    templateScheduleView = new MobileTemplateScheduleViewMonth(param, MobileViewType.MyTemplateMonth, aggregatedDays, false);
                }
                catch (Exception ex)
                {
                    LogError(ex, this.log);
                    return new MobileTemplateScheduleViewMonth(param, GetText(8778, "Ett fel inträffade"));
                }
            }
            return templateScheduleView;
        }

        /// <summary>
        /// Returns data for the view "Mitt schema - månad"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        private MobileScheduleViewMonth PerformGetScheduleViewEmployee(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            MobileScheduleViewMonth scheduleView = new MobileScheduleViewMonth(param);
            try
            {
                bool isCallerVerionsOlderThen22 = Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_21);
                bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);

                // Force monday to be first day of week
                var culture = GetCulture((int)TermGroup_Languages.Swedish);

                dateFrom = CalendarUtility.GetFirstDateOfWeek(dateFrom, culture);
                dateTo = CalendarUtility.GetLastDateOfWeek(dateTo, false, culture);

                // Special functionality for Wikmans El.
                // They are not interested in schedule in the mobile.
                // Temporary solution until we have separated schedule and order planning.
                List<TermGroup_TimeScheduleTemplateBlockType> blockTypes = null;
                if (param.ActorCompanyId == 321142)
                {
                    blockTypes = new List<TermGroup_TimeScheduleTemplateBlockType>();
                    blockTypes.Add(TermGroup_TimeScheduleTemplateBlockType.Order);
                    blockTypes.Add(TermGroup_TimeScheduleTemplateBlockType.Booking);
                }

                List<TimeSchedulePlanningMonthSmallDTO> days = TimeScheduleManager.GetTimeSchedulePlanningPeriods_ByProcedureForEmployee(param.ActorCompanyId, param.RoleId, param.UserId, dateFrom, dateTo, employeeId, TimeSchedulePlanningDisplayMode.User, blockTypes, timeScheduleScenarioHeadId: null, includeOnDuty: includeOnDuty).ToList();
                EmployeeListDTO employeeAvailability = EmployeeManager.GetEmployeeListAvailability(param.ActorCompanyId, new List<int> { employeeId }).FirstOrDefault();
                List<TimeScheduleSwapRequestDTO> employeeSwapRequests = TimeScheduleManager.GetEmployeeSwapRequestInititatedRows(param.ActorCompanyId, employeeId, dateFrom, dateTo, onlyInitiated: false);

                foreach (var weekGroup in days.GroupBy(x => CalendarUtility.GetWeekNr(x.Date, culture)).ToList())
                {
                    string weekInfo = "- " + CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(weekGroup.Sum(x => x.PlannedMinutes)), false, false);
                    scheduleView.AddWeek(MobileViewType.MyScheduleMonth, weekGroup.Key, weekInfo, weekGroup.ToList(), employeeAvailability, employeeId, employeeSwapRequests, isCallerVerionsOlderThen22);
                }

                return scheduleView;
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileScheduleViewMonth(param, GetText(8778, "Ett fel inträffade"));
            }
        }

        /// <summary>
        /// Returns data for the view "Översikt - vecka (Anställd)"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <param name="employeeIdsStr"></param>
        /// <param name="shiftTypeIdsStr"></param>
        /// <returns></returns>
        private MobileScheduleViewWeek PerformGetScheduleOverviewGroupedByEmployeeForEmployee(MobileParam param, int employeeId, DateTime dateFrom, DateTime dateTo, bool includeSecondaryCategoriesOrAccounts, string employeeIdsStr, string shiftTypeIdsStr)
        {
            MobileScheduleViewWeek scheduleViewWeek = new MobileScheduleViewWeek(param);
            try
            {
                Employee callingEmployee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadEmployment: true);
                if (callingEmployee == null)
                    return new MobileScheduleViewWeek(param, Texts.EmployeeNotFoundMessage);

                List<DateTime> employmentDaysInInterval = callingEmployee.GetEmploymentDates(dateFrom, dateTo);
                bool isCallerVerionsOlderThen22 = Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_21);
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(param.ActorCompanyId));
                List<int> employeeIdsInput = this.GetIds(employeeIdsStr);
                if (!employeeIdsInput.Any())
                    employeeIdsInput = this.SchedulePlanningGetEmployees(param, employeeId, MobileDisplayMode.User, showAll: false, includeSecondaryCategoriesOrAccounts: includeSecondaryCategoriesOrAccounts, from: dateFrom, to: dateTo).Select(x => x.Key).ToList();

                if (!employeeIdsInput.Contains(employeeId))
                    employeeIdsInput.Add(employeeId);

                List<int> shiftTypeIdsInput = this.GetIds(shiftTypeIdsStr);
                if (!shiftTypeIdsInput.Any())
                    shiftTypeIdsInput = this.SchedulePlanningGetShiftTypes(param, employeeId, MobileDisplayMode.User, includeSecondaryCategoriesOrAccounts, "").Select(x => x.Key).ToList();

                bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);

                List<TimeSchedulePlanningAggregatedDayDTO> allDays = TimeScheduleManager.GetTimeSchedulePlanningShiftsAggregated_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, param.RoleId, dateFrom, dateTo, employeeIdsInput, shiftTypeIdsInput, TimeSchedulePlanningDisplayMode.User, includeSecondaryCategoriesOrAccounts, false, includePreliminary: false, timeScheduleScenarioHeadId: null, planningMode: TimeSchedulePlanningMode.OrderPlanning, includeOnDuty: includeOnDuty).ToList();
                List<TimeSchedulePlanningAggregatedDayDTO> myAggregatedDays = allDays.Where(x => x.EmployeeId == employeeId).ToList();
                List<TimeSchedulePlanningAggregatedDayDTO> aggregatedDays = allDays.Where(x => x.EmployeeId != employeeId).ToList();
                aggregatedDays = aggregatedDays.Where(x => employmentDaysInInterval.Contains(x.Date)).ToList();

                List<int> distinctEmployeeIds = allDays.Select(x => x.EmployeeId).Distinct().ToList();
                if (!distinctEmployeeIds.Contains(hiddenEmployeeId) && employeeIdsInput.Contains(hiddenEmployeeId))
                    distinctEmployeeIds.Add(hiddenEmployeeId);

                if (!distinctEmployeeIds.Contains(employeeId))
                    distinctEmployeeIds.Add(employeeId);

                List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(param.ActorCompanyId, distinctEmployeeIds);
                foreach (var employee in employees)
                {
                    if (employee.EmployeeId == employeeId)
                    {
                        List<TimeScheduleSwapRequestDTO> employeeSwapRequests = TimeScheduleManager.GetEmployeeSwapRequestInititatedRows(param.ActorCompanyId, employee.EmployeeId, dateFrom, dateTo, onlyInitiated: false);
                        EmployeeListDTO employeeAvailability = EmployeeManager.GetEmployeeListAvailability(param.ActorCompanyId, new List<int> { employee.EmployeeId }).FirstOrDefault();
                        scheduleViewWeek.AddEmployee(MobileViewType.OverviewWeekEmployee, employee.EmployeeId, employee.EmployeeNr, GetText(8784, "Mitt"), GetText(5004, "Schema"), false, "", null, myAggregatedDays, employeeAvailability, employeeSwapRequests, dateFrom, dateTo, isCallerVerionsOlderThen22, includeOnDuty, true);
                    }
                    else
                    {
                        List<TimeSchedulePlanningAggregatedDayDTO> days = aggregatedDays.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
                        if (days.Any() || employee.EmployeeId == hiddenEmployeeId)
                        {
                            var hiddenEmpIncludeOnDuty = employee.EmployeeId == hiddenEmployeeId && includeOnDuty;
                            scheduleViewWeek.AddEmployee(MobileViewType.OverviewWeekEmployee, employee.EmployeeId, employee.EmployeeNr, employee.FirstName, employee.LastName, employee.EmployeeId == hiddenEmployeeId, "", employee.UserId, days, null, null, dateFrom, dateTo, isCallerVerionsOlderThen22, hiddenEmpIncludeOnDuty, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileScheduleViewWeek(param, GetText(8778, "Ett fel inträffade"));
            }

            return scheduleViewWeek;
        }

        /// <summary>
        /// Returns data for the day view for employee
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="date"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <param name="employeeIdsStr"></param>
        /// <param name="shiftTypeIdsStr"></param>
        /// <returns></returns>
        private MobileScheduleViewDay PerformGetScheduleDayViewEmployee(MobileParam param, int employeeId, DateTime date, bool includeSecondaryCategoriesOrAccounts, string employeeIdsStr, string shiftTypeIdsStr)
        {
            MobileScheduleViewDay mobileScheduleViewDay = new MobileScheduleViewDay(param);
            try
            {
                Employee callingEmployee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadEmployment: true);
                if (callingEmployee == null)
                    return new MobileScheduleViewDay(param, Texts.EmployeeNotFoundMessage);

                List<DateTime> employmentDaysInInterval = callingEmployee.GetEmploymentDates(date, date);

                int dayViewStartTimeMinutes = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, 0, param.ActorCompanyId, 0);
                int dayViewEndTimeMinutes = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewEndTime, 0, param.ActorCompanyId, 0);

                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(param.ActorCompanyId));
                bool includeBreaks = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) != (int)TermGroup_TimeSchedulePlanningBreakVisibility.Hidden);
                bool includeTotalBreakInfo = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) == (int)TermGroup_TimeSchedulePlanningBreakVisibility.TotalMinutes);

                List<int> employeeIdsInput = this.GetIds(employeeIdsStr);
                if (!employeeIdsInput.Any())
                    employeeIdsInput = this.SchedulePlanningGetEmployees(param, employeeId, MobileDisplayMode.User, showAll: false, includeSecondaryCategoriesOrAccounts: includeSecondaryCategoriesOrAccounts, from: date, to: date).Select(x => x.Key).ToList();

                if (!employeeIdsInput.Contains(employeeId))
                    employeeIdsInput.Add(employeeId);

                List<int> shiftTypeIdsInput = this.GetIds(shiftTypeIdsStr);
                if (!shiftTypeIdsInput.Any())
                    shiftTypeIdsInput = this.SchedulePlanningGetShiftTypes(param, employeeId, MobileDisplayMode.User, includeSecondaryCategoriesOrAccounts, "").Select(x => x.Key).ToList();

                bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);

                List<TimeSchedulePlanningAggregatedDayDTO> allDays = TimeScheduleManager.GetTimeSchedulePlanningShiftsAggregated_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, param.RoleId, date.Date, date.Date, employeeIdsInput, shiftTypeIdsInput, TimeSchedulePlanningDisplayMode.User, includeSecondaryCategoriesOrAccounts, includeBreaks, includePreliminary: false, timeScheduleScenarioHeadId: null, planningMode: TimeSchedulePlanningMode.OrderPlanning, includeOnDuty: includeOnDuty).ToList();
                TimeSchedulePlanningAggregatedDayDTO myAggregatedDay = allDays.Where(x => x.EmployeeId == employeeId).FirstOrDefault(day => day.Date == date);
                List<TimeSchedulePlanningAggregatedDayDTO> aggregatedDays = allDays.Where(x => x.EmployeeId != employeeId).ToList();
                aggregatedDays = aggregatedDays.Where(x => employmentDaysInInterval.Contains(x.Date)).ToList();

                List<int> distinctEmployeeIds = allDays.Select(x => x.EmployeeId).Distinct().ToList();
                if (!distinctEmployeeIds.Contains(hiddenEmployeeId) && employeeIdsInput.Contains(hiddenEmployeeId))
                    distinctEmployeeIds.Add(hiddenEmployeeId);

                if (!distinctEmployeeIds.Contains(employeeId))
                    distinctEmployeeIds.Add(employeeId);

                List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(param.ActorCompanyId, distinctEmployeeIds);
                List<TimeSchedulePlanningDayDTO> allShifts = new List<TimeSchedulePlanningDayDTO>();

                #region Remove shifts that started yesterday but keep shifts that starts after midnight (see item 43355)
                //The app has currently no support for shifts that started yesterday but ended today
                if (myAggregatedDay != null)
                    myAggregatedDay.DayDTOs = myAggregatedDay.DayDTOs.Where(x => x.ActualDate >= date.Date).ToList();

                foreach (var aggDay in aggregatedDays)
                {
                    aggDay.DayDTOs = aggDay.DayDTOs.Where(x => x.ActualDate >= date.Date).ToList();
                }

                #endregion

                foreach (var employee in employees)
                {
                    if (employee.EmployeeId == employeeId)
                    {
                        List<TimeScheduleSwapRequestDTO> employeeSwapRequests = TimeScheduleManager.GetEmployeeSwapRequestInititatedRows(param.ActorCompanyId, employeeId, date, date, onlyInitiated: false);
                        EmployeeListDTO employeeAvailability = EmployeeManager.GetEmployeeListAvailability(param.ActorCompanyId, new List<int> { employee.EmployeeId }).FirstOrDefault();
                        if (myAggregatedDay != null || (employeeAvailability?.IsAvailableInRange(date, date) ?? false) || (employeeAvailability?.IsUnavailableInRange(date, date) ?? false))
                        {
                            if (myAggregatedDay != null)
                                allShifts.AddRange(myAggregatedDay.DayDTOs);

                            mobileScheduleViewDay.AddEmployee(MobileViewType.DayViewEmployee, employee.EmployeeId, employee.EmployeeNr, GetText(8784, "Mitt"), GetText(5004, "Schema"), false, "", null, date, myAggregatedDay, employeeAvailability, includeBreaks, includeTotalBreakInfo, employeeSwapRequests, false, includeOnDuty);
                        }
                    }
                    else
                    {
                        TimeSchedulePlanningAggregatedDayDTO employeeDay = aggregatedDays.FirstOrDefault(x => x.EmployeeId == employee.EmployeeId && x.Date == date.Date);
                        if ((employeeDay != null && employeeDay.DayDTOs.Any()) || employee.EmployeeId == hiddenEmployeeId)
                        {
                            if (employeeDay != null)
                                allShifts.AddRange(employeeDay.DayDTOs);

                            bool hiddenEmpIncludeOnDuty = employee.EmployeeId == hiddenEmployeeId && includeOnDuty;
                            mobileScheduleViewDay.AddEmployee(MobileViewType.DayViewEmployee, employee.EmployeeId, employee.EmployeeNr, employee.FirstName, employee.LastName, employee.Hidden, "", employee.UserId, date, employeeDay, null, includeBreaks, includeTotalBreakInfo, null, false, hiddenEmpIncludeOnDuty);
                        }
                    }
                }

                #region Decide start and end for time axis

                var firstShift = allShifts.OrderBy(x => x.StartTime).FirstOrDefault();
                var lastShift = allShifts.OrderBy(x => x.StopTime).LastOrDefault();
                if (firstShift != null && lastShift != null)
                {
                    TimeSpan settingStart = new TimeSpan(0, dayViewStartTimeMinutes, 0);
                    TimeSpan firstShiftStart = firstShift.StartTime.TimeOfDay;
                    if (firstShift.StartTime.TimeOfDay > firstShift.StopTime.TimeOfDay || firstShift.BelongsToNextDay)  // Starts previous day
                        firstShiftStart = firstShiftStart.Add(new TimeSpan(0, 0, 0));

                    TimeSpan settingEnd = new TimeSpan(0, dayViewEndTimeMinutes, 0);
                    TimeSpan lastShiftEnd = lastShift.StopTime.TimeOfDay;
                    if (lastShift.StopTime.TimeOfDay < lastShift.StartTime.TimeOfDay || lastShift.BelongsToPreviousDay) // Ends next day
                        lastShiftEnd = lastShiftEnd.Add(new TimeSpan(24, 0, 0));

                    if (firstShiftStart < settingStart)
                        dayViewStartTimeMinutes = (int)firstShiftStart.TotalMinutes;

                    if (lastShiftEnd > settingEnd)
                        dayViewEndTimeMinutes = (int)lastShiftEnd.TotalMinutes;
                }

                bool isOverlappingMidnight = false;
                if (dayViewEndTimeMinutes > new TimeSpan(24, 0, 0).TotalMinutes)
                {
                    isOverlappingMidnight = true;
                    dayViewEndTimeMinutes -= (int)new TimeSpan(24, 0, 0).TotalMinutes;
                }

                mobileScheduleViewDay.SetupTimeAxis(CalendarUtility.FormatMinutes(dayViewStartTimeMinutes), CalendarUtility.FormatMinutes(dayViewEndTimeMinutes), isOverlappingMidnight);

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileScheduleViewDay(param, GetText(8778, "Ett fel inträffade"));
            }
            return mobileScheduleViewDay;
        }

        #endregion

        #region Admin

        /// <summary>
        /// Returns data for the view "Översikt vecka" for admin
        /// </summary>
        /// <param name="param"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <param name="employeeIdsStr"></param>
        /// <param name="shiftTypeIdsStr">will always be empty if useAccountHierarchy is set to true, use accountIdsStr to determine filter on shifttypes </param>
        /// <param name="accountIdsStr"></param>
        /// <returns></returns>
        private MobileScheduleViewWeek PerformGetScheduleOverviewGroupedByEmployeeAdmin(MobileParam param, DateTime dateFrom, DateTime dateTo, bool includeSecondaryCategoriesOrAccounts, string employeeIdsStr, string shiftTypeIdsStr, string accountIdsStr, bool includeUnscheduledEmployees)
        {
            MobileScheduleViewWeek scheduleViewWeek = new MobileScheduleViewWeek(param);
            try
            {
                bool isCallerVerionsOlderThen22 = Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_21);
                bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);

                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, param.ActorCompanyId);
                int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(param.ActorCompanyId));
                List<int> shiftAccountdIds = useAccountHierarchy ? this.GetShiftAccountIdsFromAccountIds(param, GetIds(accountIdsStr)) : null;

                List<int> employeeIdsInput = this.GetIds(employeeIdsStr);
                if (!employeeIdsInput.Any())
                    employeeIdsInput = this.SchedulePlanningGetEmployees(param, 0, MobileDisplayMode.Admin, showAll: false, includeSecondaryCategoriesOrAccounts: includeSecondaryCategoriesOrAccounts, from: dateFrom, to: dateTo, accountIdsStr: accountIdsStr).Select(x => x.Key).ToList();

                List<int> shiftTypeIdsInput = this.GetIds(shiftTypeIdsStr);
                if (!shiftTypeIdsInput.Any())
                    shiftTypeIdsInput = this.SchedulePlanningGetShiftTypes(param, 0, MobileDisplayMode.Admin, includeSecondaryCategoriesOrAccounts, accountIdsStr).Select(x => x.Key).ToList();

                List<TimeSchedulePlanningAggregatedDayDTO> aggregatedDays = TimeScheduleManager.GetTimeSchedulePlanningShiftsAggregated_ByProcedure(param.ActorCompanyId, param.UserId, 0, param.RoleId, dateFrom, dateTo, employeeIdsInput, shiftTypeIdsInput, TimeSchedulePlanningDisplayMode.Admin, includeSecondaryCategoriesOrAccounts, false, includePreliminary: false, hiddenEmployeeId: hiddenEmployeeId, filteredAccountIds: shiftAccountdIds, timeScheduleScenarioHeadId: null, includeShiftRequest: true, planningMode: TimeSchedulePlanningMode.OrderPlanning, includeOnDuty: includeOnDuty).ToList();
                List<int> distinctEmployeeIds = aggregatedDays.Select(x => x.EmployeeId).Distinct().ToList();
                if (!distinctEmployeeIds.Contains(hiddenEmployeeId) && employeeIdsInput.Contains(hiddenEmployeeId))
                    distinctEmployeeIds.Add(hiddenEmployeeId);

                //If user has filtered on shifttypes, exclude shifts without shifttypes
                if (!shiftTypeIdsStr.IsNullOrEmpty())
                {
                    foreach (var aggDay in aggregatedDays)
                    {
                        aggDay.DayDTOs = aggDay.DayDTOs.Where(x => x.ShiftTypeId != 0).ToList();
                    }
                }

                List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(param.ActorCompanyId, includeUnscheduledEmployees ? employeeIdsInput : distinctEmployeeIds);
                List<EmployeeListDTO> employeesAvailability = EmployeeManager.GetEmployeeListAvailability(param.ActorCompanyId, employeeIdsInput);
                List<TimeScheduleSwapRequestDTO> employeesSwapRequests = TimeScheduleManager.GetEmployeesSwapRequestApprovedRows(param.ActorCompanyId, includeUnscheduledEmployees ? employeeIdsInput : distinctEmployeeIds, dateFrom, dateTo, onlyInitiated: false);

                foreach (var employee in employees)
                {
                    List<TimeSchedulePlanningAggregatedDayDTO> days = aggregatedDays.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
                    var employeeAvailability = employeesAvailability.FirstOrDefault(x => x.EmployeeId == employee.EmployeeId);
                    if (includeUnscheduledEmployees || employee.EmployeeId == hiddenEmployeeId || days.Any() || (employeeAvailability?.HasAvailabilityInRange(dateFrom, dateTo) ?? false))
                        scheduleViewWeek.AddEmployee(MobileViewType.OverviewWeekAdmin, employee.EmployeeId, employee.EmployeeNr, employee.FirstName, employee.LastName, employee.EmployeeId == hiddenEmployeeId, "", employee.UserId, days, employeeAvailability, employeesSwapRequests, dateFrom, dateTo, isCallerVerionsOlderThen22, includeOnDuty, true);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileScheduleViewWeek(param, GetText(8778, "Ett fel inträffade"));
            }

            return scheduleViewWeek;
        }

        /// <summary>
        /// Returns data for the day view for admin
        /// </summary>
        /// <param name="param"></param>
        /// <param name="date"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <param name="employeeIdsStr"></param>
        /// <param name="shiftTypeIdsStr"></param>
        /// <returns></returns>
        private MobileScheduleViewDay PerformGetScheduleDayViewAdmin(MobileParam param, DateTime date, bool includeSecondaryCategoriesOrAccounts, string employeeIdsStr, string shiftTypeIdsStr, string accountIdsStr, bool includeUnscheduledEmployees)
        {
            MobileScheduleViewDay mobileScheduleViewDay = new MobileScheduleViewDay(param);
            try
            {
                bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);

                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, param.ActorCompanyId);
                int dayViewStartTimeMinutes = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, 0, param.ActorCompanyId, 0);
                int dayViewEndTimeMinutes = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningDayViewEndTime, 0, param.ActorCompanyId, 0);

                int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(param.ActorCompanyId));
                bool includeBreaks = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) != (int)TermGroup_TimeSchedulePlanningBreakVisibility.Hidden);
                bool includeTotalBreakInfo = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) == (int)TermGroup_TimeSchedulePlanningBreakVisibility.TotalMinutes);
                List<int> shiftAccountdIds = useAccountHierarchy ? this.GetShiftAccountIdsFromAccountIds(param, GetIds(accountIdsStr)) : null;

                List<int> employeeIdsInput = this.GetIds(employeeIdsStr);
                if (!employeeIdsInput.Any())
                    employeeIdsInput = this.SchedulePlanningGetEmployees(param, 0, MobileDisplayMode.Admin, showAll: false, includeSecondaryCategoriesOrAccounts: includeSecondaryCategoriesOrAccounts, from: date, to: date, accountIdsStr: accountIdsStr).Select(x => x.Key).ToList();

                List<int> shiftTypeIdsInput = this.GetIds(shiftTypeIdsStr);
                if (!shiftTypeIdsInput.Any())
                    shiftTypeIdsInput = this.SchedulePlanningGetShiftTypes(param, 0, MobileDisplayMode.Admin, includeSecondaryCategoriesOrAccounts, accountIdsStr).Select(x => x.Key).ToList();

                List<TimeSchedulePlanningAggregatedDayDTO> aggregatedDays = TimeScheduleManager.GetTimeSchedulePlanningShiftsAggregated_ByProcedure(param.ActorCompanyId, param.UserId, 0, param.RoleId, date.Date, date.Date, employeeIdsInput, shiftTypeIdsInput, TimeSchedulePlanningDisplayMode.Admin, includeSecondaryCategoriesOrAccounts, includeBreaks, includePreliminary: false, filteredAccountIds: shiftAccountdIds, timeScheduleScenarioHeadId: null, includeShiftRequest: true, planningMode: TimeSchedulePlanningMode.OrderPlanning, includeOnDuty: includeOnDuty).ToList();
                List<EmployeeListDTO> employeesAvailability = EmployeeManager.GetEmployeeListAvailability(param.ActorCompanyId, employeeIdsInput);
                List<int> distinctEmployeeIds = aggregatedDays.Select(x => x.EmployeeId).ToList();
                employeesAvailability.ForEach(x => distinctEmployeeIds.Add(x.EmployeeId));
                distinctEmployeeIds = distinctEmployeeIds.Distinct().ToList();

                if (!distinctEmployeeIds.Contains(hiddenEmployeeId) && employeeIdsInput.Contains(hiddenEmployeeId))
                    distinctEmployeeIds.Add(hiddenEmployeeId);

                List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(param.ActorCompanyId, includeUnscheduledEmployees ? employeeIdsInput : distinctEmployeeIds);
                List<TimeScheduleSwapRequestDTO> employeesSwapRequests = TimeScheduleManager.GetEmployeesSwapRequestApprovedRows(param.ActorCompanyId, includeUnscheduledEmployees ? employeeIdsInput : distinctEmployeeIds, date, date);

                //If user has filtered on shifttypes, exclude shifts without shifttypes
                if (!shiftTypeIdsStr.IsNullOrEmpty())
                {
                    foreach (var aggDay in aggregatedDays)
                    {
                        aggDay.DayDTOs = aggDay.DayDTOs.Where(x => x.ShiftTypeId != 0).ToList();
                    }
                }

                List<TimeSchedulePlanningDayDTO> allShifts = new List<TimeSchedulePlanningDayDTO>();
                foreach (var employee in employees)
                {
                    var employeeAvailability = employeesAvailability.FirstOrDefault(x => x.EmployeeId == employee.EmployeeId);
                    TimeSchedulePlanningAggregatedDayDTO employeeDay = aggregatedDays.FirstOrDefault(x => x.EmployeeId == employee.EmployeeId && x.Date == date.Date);
                    if (includeUnscheduledEmployees || (employeeAvailability?.HasAvailabilityInRange(date, date) ?? false) || (employeeDay != null && employeeDay.DayDTOs.Any()) || employee.EmployeeId == hiddenEmployeeId)
                    {
                        if (employeeDay != null)
                            allShifts.AddRange(employeeDay.DayDTOs);

                        mobileScheduleViewDay.AddEmployee(MobileViewType.DayViewAdmin, employee.EmployeeId, employee.EmployeeNr, employee.FirstName, employee.LastName, employee.Hidden, "", employee.UserId, date, employeeDay, employeeAvailability, includeBreaks, includeTotalBreakInfo, employeesSwapRequests, includeUnscheduledEmployees: includeUnscheduledEmployees, includeOnDuty);
                    }
                }

                #region Decide start and end for time axis

                var firstShift = allShifts.OrderBy(x => x.StartTime).FirstOrDefault();
                var lastShift = allShifts.OrderBy(x => x.StopTime).LastOrDefault();
                if (firstShift != null && lastShift != null)
                {
                    TimeSpan settingStart = new TimeSpan(0, dayViewStartTimeMinutes, 0);
                    TimeSpan firstShiftStart = firstShift.StartTime.TimeOfDay;
                    if (firstShift.StartTime.TimeOfDay > firstShift.StopTime.TimeOfDay || firstShift.BelongsToNextDay)  // Starts previous day
                        firstShiftStart = firstShiftStart.Add(new TimeSpan(0, 0, 0));

                    TimeSpan settingEnd = new TimeSpan(0, dayViewEndTimeMinutes, 0);
                    TimeSpan lastShiftEnd = lastShift.StopTime.TimeOfDay;
                    if (lastShift.StopTime.TimeOfDay < lastShift.StartTime.TimeOfDay || lastShift.BelongsToPreviousDay) // Ends next day
                        lastShiftEnd = lastShiftEnd.Add(new TimeSpan(24, 0, 0));

                    if (firstShiftStart < settingStart)
                        dayViewStartTimeMinutes = (int)firstShiftStart.TotalMinutes;

                    if (lastShiftEnd > settingEnd)
                        dayViewEndTimeMinutes = (int)lastShiftEnd.TotalMinutes;
                }

                bool isOverlappingMidnight = false;
                if (dayViewEndTimeMinutes > new TimeSpan(24, 0, 0).TotalMinutes)
                {
                    isOverlappingMidnight = true;
                    dayViewEndTimeMinutes -= (int)new TimeSpan(24, 0, 0).TotalMinutes;
                }

                mobileScheduleViewDay.SetupTimeAxis(CalendarUtility.FormatMinutes(dayViewStartTimeMinutes), CalendarUtility.FormatMinutes(dayViewEndTimeMinutes), isOverlappingMidnight);

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileScheduleViewDay(param, GetText(8778, "Ett fel inträffade"));
            }
            return mobileScheduleViewDay;
        }

        #endregion

        #endregion

        #region Popup (shifts)
        /// <summary>
        /// Returns template shifts for one employee/day
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private MobileShifts PerformGetTemplateScheduleDayEmployee(MobileParam param, int employeeId, DateTime date)
        {
            MobileShifts mobileShifts = new MobileShifts(param);
            if (SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ShowTemplateScheduleForEmployeesInApp, 0, param.ActorCompanyId, 0))
            {
                try
                {
                    bool includeDescInShiftType = (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_15));
                    bool includeBreaks = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) != (int)TermGroup_TimeSchedulePlanningBreakVisibility.Hidden);
                    bool includeTotalBreakInfo = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) != (int)TermGroup_TimeSchedulePlanningBreakVisibility.TotalMinutes);

                    Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadContactPerson: true);
                    if (employee == null)
                        return new MobileShifts(param, Texts.EmployeeNotFoundMessage);

                    bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);
                    List<TimeSchedulePlanningAggregatedDayDTO> aggregatedDays = TimeScheduleManager.GetTimeSchedulePlanningTemplateAggregated(param.ActorCompanyId, param.RoleId, param.UserId, employeeId, date, date, includeOnDuty: includeOnDuty);
                    List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();
                    aggregatedDays.ForEach(x => shifts.AddRange(x.DayDTOs));
                    mobileShifts.AddMobileShifts(shifts.OrderBy(x => x.StartTime).ToList(), MobileShiftGUIType.MyTemplateShifts, false, GetText(8101, "Rast"), includeBreaks, includeTotalBreakInfo, includeDescInShiftType, false, MobileDisplayMode.User, null, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_25));
                }
                catch (Exception ex)
                {
                    LogError(ex, this.log);
                    return new MobileShifts(param, GetText(8778, "Ett fel inträffade"));
                }
            }
            return mobileShifts;
        }

        /// <summary>
        /// It is used when an employee wants to view his own shifts for a certain day
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="date"></param>
        /// <param name="includeAvailableShifts"></param>
        /// <returns></returns>
        private MobileShifts PerformGetScheduleDayEmployee(MobileParam param, int employeeId, DateTime date)
        {
            MobileShifts mobileShifts = new MobileShifts(param);

            try
            {
                Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId, loadContactPerson: true);
                if (employee == null)
                    return new MobileShifts(param, Texts.EmployeeNotFoundMessage);

                bool includeDescInShiftType = (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_15));
                bool includeBreaks = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) != (int)TermGroup_TimeSchedulePlanningBreakVisibility.Hidden);
                bool includeTotalBreakInfo = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) == (int)TermGroup_TimeSchedulePlanningBreakVisibility.TotalMinutes);
                bool showAccountName = EmployeeManager.HasMultipelEmployeeAccounts(param.ActorCompanyId, employeeId, date, date);
                List<TimeScheduleSwapRequestDTO> employeeSwapRequests = TimeScheduleManager.GetEmployeeSwapRequestInititatedRows(param.ActorCompanyId, employeeId, date, date, onlyInitiated: false);

                //Add my shifts
                bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);
                List<TimeSchedulePlanningDayDTO> shifts = TimeScheduleManager.GetTimeScheduleShifts(param.ActorCompanyId, param.UserId, 0, employeeId, date, date, null, includeBreaks, false, loadDeviationCause: true, includePreliminary: false, timeScheduleScenarioHeadId: null);
                shifts = shifts.OrderBy(x => x.StartTime).ToList();
                shifts = shifts.Where(x => x.ShiftStatus == TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned && !x.IsPreliminary && !x.IsAbsenceRequest).ToList();
                if (!includeOnDuty)
                {
                    shifts = shifts.Where(x => x.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
                }

                mobileShifts.AddMobileShifts(shifts, MobileShiftGUIType.MyShiftsNew, false, GetText(8101, "Rast"), includeBreaks, includeTotalBreakInfo, includeDescInShiftType, showAccountName, MobileDisplayMode.User, employeeSwapRequests, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_25));
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileShifts(param, GetText(8778, "Ett fel inträffade"));
            }
            return mobileShifts;
        }

        /// <summary>
        /// It is used to view shifts for hidden employee/date 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="date"></param>
        /// <param name="link"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <param name="mobileDisplayMode"></param>
        /// <returns></returns>
        private MobileShifts PerformGetAvailableShiftsNew(MobileParam param, int employeeId, DateTime date, string link, bool includeSecondaryCategoriesOrAccounts, MobileDisplayMode mobileDisplayMode)
        {
            MobileShifts mobileShifts = new MobileShifts(param);

            // Check company setting if using hidden employee or vacant
            List<int> empIds = new List<int>();
            bool showAccountName = EmployeeManager.HasMultipelEmployeeAccounts(param.ActorCompanyId, employeeId, date, date);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            empIds.Add(base.GetHiddenEmployeeIdFromCache(entitiesReadOnly, CacheConfig.Company(param.ActorCompanyId)));

            bool includeDescInShiftType = (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_15));
            bool includeBreaks = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) != (int)TermGroup_TimeSchedulePlanningBreakVisibility.Hidden);
            bool includeTotalBreakInfo = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) == (int)TermGroup_TimeSchedulePlanningBreakVisibility.TotalMinutes);
            bool showQueueCount = false;

            bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);
            List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();
            if (mobileDisplayMode == MobileDisplayMode.User)
            {
                shifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, 0, date, date, empIds, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, includeSecondaryCategoriesOrAccounts, includeBreaks, false, includePreliminary: false, checkToIncludeDeliveryAdress: false, timeScheduleScenarioHeadId: null, includeOnDuty: includeOnDuty);
            }
            else if (mobileDisplayMode == MobileDisplayMode.Admin)
            {
                showQueueCount = true;
                shifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, 0, date, date, empIds, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.Admin, includeSecondaryCategoriesOrAccounts, includeBreaks, false, includePreliminary: false, checkToIncludeDeliveryAdress: false, timeScheduleScenarioHeadId: null, includeShiftRequest: true, setIsLended: true, includeOnDuty: includeOnDuty);
            }

            Guid? linkGuid = string.IsNullOrEmpty(link) ? (Guid?)null : new Guid(link);
            List<TimeSchedulePlanningDayDTO> openShifts = shifts.Where(x => x.ActualDate == date.Date && x.Link == linkGuid && x.ShiftStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Open && !x.IsPreliminary).ToList();
            openShifts = openShifts.Where(x => x.ShiftStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Open && !x.IsPreliminary).OrderBy(x => x.StartTime).ToList();
            mobileShifts.AddMobileShifts(openShifts, MobileShiftGUIType.AvailableShiftsNew, showQueueCount, GetText(8101, "Rast"), includeBreaks, includeTotalBreakInfo, includeDescInShiftType, showAccountName, mobileDisplayMode, null, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_25));

            return mobileShifts;
        }

        /// <summary>
        /// It is used to view shifts for a specified employee and date
        /// Used by both admin and employee
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="employeeIdToView"></param>
        /// <param name="date"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <param name="mobileDisplayMode"></param>
        /// <returns></returns>
        private MobileShifts PerformGetOthersShiftsNew(MobileParam param, int employeeId, int employeeIdToView, DateTime date, bool includeSecondaryCategoriesOrAccounts, MobileDisplayMode mobileDisplayMode)
        {
            MobileShifts mobileShifts = new MobileShifts(param);

            bool includeDescInShiftType = (Utils.IsCallerExpectedVersionOlderOrEqualToGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_15));
            bool includeBreaks = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) != (int)TermGroup_TimeSchedulePlanningBreakVisibility.Hidden);
            bool includeTotalBreakInfo = (SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, 0, param.ActorCompanyId, 0) == (int)TermGroup_TimeSchedulePlanningBreakVisibility.TotalMinutes);
            bool showAccountName = EmployeeManager.HasMultipelEmployeeAccounts(param.ActorCompanyId, employeeId, date, date);
            bool showQueueCount = false;
            List<TimeScheduleSwapRequestDTO> employeesSwapRequests = null;
            List<TimeSchedulePlanningDayDTO> employeeShifts = new List<TimeSchedulePlanningDayDTO>();

            if (mobileDisplayMode == MobileDisplayMode.User)
            {
                employeeShifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, 0, date, date, new List<int> { employeeIdToView }, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, includeSecondaryCategoriesOrAccounts, includeBreaks, false, includePreliminary: false, checkToIncludeDeliveryAdress: false, timeScheduleScenarioHeadId: null);
            }
            else if (mobileDisplayMode == MobileDisplayMode.Admin)
            {
                bool includeOnDuty = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_30);

                showQueueCount = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_25);
                employeeShifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, 0, 0, date, date, new List<int> { employeeIdToView }, TimeSchedulePlanningMode.OrderPlanning, TimeSchedulePlanningDisplayMode.Admin, includeSecondaryCategoriesOrAccounts, includeBreaks, false, includePreliminary: false, checkToIncludeDeliveryAdress: false, timeScheduleScenarioHeadId: null, includeShiftRequest: true, setIsLended: true, includeOnDuty: includeOnDuty);
                employeesSwapRequests = TimeScheduleManager.GetEmployeesSwapRequestApprovedRows(param.ActorCompanyId, new List<int> { employeeIdToView }, date, date, onlyInitiated: false);
            }


            employeeShifts = employeeShifts.Where(x => x.ActualDate == date.Date && x.ShiftStatus == TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned && !x.IsAbsenceRequest).OrderBy(x => x.StartTime).ToList();
            mobileShifts.AddMobileShifts(employeeShifts, MobileShiftGUIType.OtherShiftsNew, showQueueCount, GetText(8101, "Rast"), includeBreaks, includeTotalBreakInfo, includeDescInShiftType, showAccountName, mobileDisplayMode, employeesSwapRequests, Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_25));
            return mobileShifts;
        }

        #endregion

        #region Common

        /// <summary>
        /// Returns the employees that given employee is allowed to see - used in dialog "Välj filter"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <returns></returns>
        private MobileDicts PerformGetSchedulePlanningEmployeesForEmployee(MobileParam param, int employeeId, bool includeSecondaryCategoriesOrAccounts, string employeeIds)
        {
            try
            {
                Dictionary<int, string> dict = SchedulePlanningGetEmployees(param, employeeId, MobileDisplayMode.User, showAll: false, includeSecondaryCategoriesOrAccounts: includeSecondaryCategoriesOrAccounts);
                MobileDicts mobileDicts = new MobileDicts(param, dict);
                mobileDicts.SetSelectedIds(GetIds(employeeIds));
                return mobileDicts;
            }
            catch (Exception e)
            {
                LogError("PerformGetSchedulePlanningEmployeesForEmployee: " + e.Message);
                return new MobileDicts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        /// <summary>
        /// Returns the shifttypes that given employee is allowed to see - used in dialog "Välj filter"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="employeeId"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <returns></returns>
        private MobileDicts PerformGetSchedulePlanningShiftTypesForEmployee(MobileParam param, int employeeId, bool includeSecondaryCategoriesOrAccounts, string shiftTypeIdsStr)
        {
            Dictionary<int, string> dict = SchedulePlanningGetShiftTypes(param, employeeId, MobileDisplayMode.User, includeSecondaryCategoriesOrAccounts, "");
            MobileDicts mobileDicts = new MobileDicts(param, dict);
            mobileDicts.SetSelectedIds(GetIds(shiftTypeIdsStr));
            return mobileDicts;
        }

        /// <summary>
        /// Returns the employees that admin is allowed to see - used in dialog "Välj filter"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <returns></returns>
        private MobileDicts PerformGetSchedulePlanningEmployeesForAdmin(MobileParam param, bool includeSecondaryCategoriesOrAccounts, string accountIdsStr, string employeeIds)
        {
            try
            {
                Dictionary<int, string> dict = SchedulePlanningGetEmployees(param, 0, MobileDisplayMode.Admin, showAll: false, includeSecondaryCategoriesOrAccounts: includeSecondaryCategoriesOrAccounts, accountIdsStr: accountIdsStr);
                MobileDicts mobileDicts = new MobileDicts(param, dict);
                mobileDicts.SetSelectedIds(GetIds(employeeIds));
                return mobileDicts;
            }
            catch (Exception e)
            {
                LogError("PerformGetSchedulePlanningEmployeesForAdmin: " + e.Message);
                return new MobileDicts(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        /// <summary>
        /// Returns the shifttypes that admin is allowed to see - used in dialog "Välj filter"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="includeSecondaryCategoriesOrAccounts"></param>
        /// <returns></returns>
        private MobileDicts PerformGetSchedulePlanningShiftTypesForAdmin(MobileParam param, bool includeSecondaryCategoriesOrAccounts, string accountIdsStr, string shiftTypeIdsStr)
        {
            Dictionary<int, string> dict = SchedulePlanningGetShiftTypes(param, 0, MobileDisplayMode.Admin, includeSecondaryCategoriesOrAccounts, accountIdsStr);
            MobileDicts mobileDicts = new MobileDicts(param, dict);
            mobileDicts.SetSelectedIds(GetIds(shiftTypeIdsStr));
            return mobileDicts;
        }

        /// <summary>
        /// Get list of message groups
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private MobileMessageGroups PerformGetMessageGroups(MobileParam param)
        {
            try
            {
                var mobileMessageGroups = new MobileMessageGroups(param);

                var messageGroups = CommunicationManager.GetMessageGroups(param.ActorCompanyId, param.UserId);
                mobileMessageGroups.AddMobileMessageGroups(messageGroups);

                return mobileMessageGroups;
            }
            catch (Exception e)
            {
                LogError("PerformGetMessageGroups: " + e.Message);
                return new MobileMessageGroups(param, GetText(8455, "Internt fel") + " : " + e.Message);
            }
        }

        /// <summary>
        /// Returns available employees - used for "ShiftRequest, AssignAvailableShift and Absencerequest"
        /// </summary>
        /// <param name="param"></param>
        /// <param name="shiftId"></param>
        /// <param name="filterOnShiftType"></param>
        /// <param name="filterOnAvailability"></param>
        /// <param name="filterOnSkills"></param>
        /// <param name="filterOnWorkRules"></param>
        /// <param name="shiftRequest"></param>
        /// <param name="assignEmployee"></param>
        /// <returns></returns>
        private MobileEmployeeList PerformGetAvailableEmployeesNew(MobileParam param, int shiftId, bool filterOnShiftType, bool filterOnAvailability, bool filterOnSkills, bool filterOnWorkRules, bool shiftRequest, bool absenceRequest, DateTime absenceStartTime, DateTime absenceStopTime, int? filterOnMessageGroupId = null)
        {
            MobileEmployeeList list = new MobileEmployeeList(param);
            try
            {
                TimeScheduleTemplateBlock templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId);

                if (templateBlock == null)
                    return new MobileEmployeeList(param, GetText(8503, "Passet kunde inte hittas"));

                if (templateBlock.IsBreak || templateBlock.TimeDeviationCauseId.HasValue || !templateBlock.EmployeeId.HasValue)
                    return new MobileEmployeeList(param, GetText(8785, "Ogiltigt pass"));

                List<int> employeeIds = this.SchedulePlanningGetEmployees(param, 0, MobileDisplayMode.Admin, showAll: false, includeSecondaryCategoriesOrAccounts: true, getHidden: false, from: templateBlock.Date, to: templateBlock.Date).Select(x => x.Key).ToList();
                List<AvailableEmployeesDTO> availableEmployees = new List<AvailableEmployeesDTO>();
                if (absenceRequest)
                {
                    TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId);
                    if (shift != null && shift.EmployeeId.HasValue && shift.Date.HasValue)
                    {
                        List<TimeScheduleTemplateBlockDTO> scheduleDTOs = new List<TimeScheduleTemplateBlockDTO>();
                        var dto = shift.ToDTO();
                        dto.StartTime = CalendarUtility.GetScheduleTime(absenceStartTime, shift.Date.Value, absenceStartTime.Date);
                        dto.StopTime = CalendarUtility.GetScheduleTime(absenceStopTime, absenceStartTime.Date, absenceStopTime.Date);
                        scheduleDTOs.Add(dto);

                        availableEmployees = TimeEngineManager(param.ActorCompanyId, param.UserId).GetAvailableEmployees(new List<int>(), employeeIds, filterOnShiftType, filterOnAvailability, filterOnSkills, filterOnWorkRules, filterOnMessageGroupId, false, scheduleDTOs, !shiftRequest, !shiftRequest);
                    }
                }
                else
                {
                    string linkStr = string.IsNullOrEmpty(templateBlock.Link) ? "" : templateBlock.Link;
                    List<int> shiftIds = new List<int>();
                    shiftIds.Add(templateBlock.TimeScheduleTemplateBlockId);

                    if (!string.IsNullOrEmpty(linkStr))
                    {
                        List<TimeScheduleTemplateBlock> shifts = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(templateBlock.EmployeeId.Value, templateBlock.Date.Value, excludeBreaks: true);
                        shifts = shifts.Where(b => b.Link.Equals(linkStr)).ToList();

                        foreach (var item in shifts)
                        {
                            if (item.TimeScheduleTemplateBlockId == templateBlock.TimeScheduleTemplateBlockId || item.TimeDeviationCauseId.HasValue || item.IsBreak || !item.IsSchedule())
                                continue;

                            shiftIds.Add(item.TimeScheduleTemplateBlockId);
                        }
                    }

                    availableEmployees = TimeEngineManager(param.ActorCompanyId, param.UserId).GetAvailableEmployees(shiftIds, employeeIds, filterOnShiftType, filterOnAvailability, filterOnSkills, filterOnWorkRules, filterOnMessageGroupId, true, null, !shiftRequest, !shiftRequest);
                }

                list.AddToMobileEmployeeList(availableEmployees);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileEmployeeList(param, GetText(8778, "Ett fel inträffade"));
            }

            return list;
        }

        private MobileCompanyHolidays PerformGetCompanyHolidays(MobileParam param, DateTime dateFrom, DateTime dateTo)
        {
            List<HolidayDTO> companyHolidays = CalendarManager.GetHolidaysByCompany(param.ActorCompanyId, dateFrom, dateTo);
            companyHolidays = companyHolidays.Where(d => (d.Date >= dateFrom && d.Date <= dateTo)).ToList();

            return new MobileCompanyHolidays(param, companyHolidays);
        }

        #endregion

        #region XeMail - NeedsConfoirmation

        private MobileXeMail PerformSendNeedsConfirmationAnswer(MobileParam param, int mailId)
        {
            MobileXeMail xemail = new MobileXeMail(param);
            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileXeMail(param, Texts.UserNotFound);

            MessageEditDTO message = CommunicationManager.GetXEMail(mailId, user.LicenseId, user.UserId);

            if (message == null)
                return new MobileXeMail(param, Texts.MessageNotFound);

            MessageEditDTO messageDto = new MessageEditDTO()
            {
                LicenseId = user.LicenseId,
                MessagePriority = TermGroup_MessagePriority.Normal,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.Text,
                MessageType = message.MessageType,
                RoleId = param.RoleId,
                MarkAsOutgoing = false,
                SenderName = user.Name,
                SenderEmail = string.Empty,
                SenderUserId = user.UserId,
                Entity = message.Entity,
                ActorCompanyId = param.ActorCompanyId,
                RecordId = 0,
                AnswerType = XEMailAnswerType.Yes,
                Recievers = message.Recievers,
                Subject = message.Subject,
                Text = message.Text,
                ShortText = message.ShortText,
                ParentId = message.MessageId
            };

            ActionResult result = CommunicationManager.SendXEMail(messageDto, param.ActorCompanyId, param.RoleId, param.UserId);

            if (result.Success)
                xemail.SetTaskResult(MobileTask.SendNeedsConfirmationAnswer, true);
            else
            {
                if (result.ErrorNumber > 0)
                    xemail = new MobileXeMail(param, FormatMessage(result.ErrorMessage, result.ErrorNumber));
                else
                    xemail = new MobileXeMail(param, Texts.SendAnswerFailed);
            }

            return xemail;
        }
        #endregion

        #endregion

        #region Payroll

        #region Employee TimePeriods
        private MobileEmployeeTimePeriodYears PerformGetEmployeeTimePeriodYears(MobileParam param, int employeeId, int year)
        {
            MobileEmployeeTimePeriodYears employeeTimePeriodYears = new MobileEmployeeTimePeriodYears(param);
            try
            {
                bool usePayroll = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, param.ActorCompanyId, 0);
                List<DataStorageSmallDTO> dto = GeneralManager.GetEmployeeTimePeriodsForYear(param.ActorCompanyId, employeeId, year).OrderByDescending(o => o.TimePeriodPaymentDate).ToList();

                if (!dto.Any())
                    return new MobileEmployeeTimePeriodYears(param, Texts.PayrollPeriodNotFoundMessage);

                if (year == 0)
                {
                    if (dto.Any(f => f.Year.HasValue && f.Year.Value == DateTime.Today.Year))
                        year = DateTime.Today.Year;
                    else if (dto.Any(w => w.Year.Value != 9999))
                        year = dto.Where(w => w.Year.Value != 9999).Max(f => f.Year.Value);
                    else
                        year = 9999;
                }
                if (year == 0)
                    return new MobileEmployeeTimePeriodYears(param, Texts.PayrollPeriodNotFoundMessage);

                employeeTimePeriodYears.AddMobileEmployeeTimePeriodsYears(dto, year, usePayroll);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileEmployeeTimePeriodYears(param, GetText(8778, "Ett fel inträffade"));
            }

            return employeeTimePeriodYears;
        }
        private MobileEmployeeTimePeriods PerformGetEmployeeTimePeriods(MobileParam param, int employeeId)
        {
            MobileEmployeeTimePeriods employeeTimePeriods = new MobileEmployeeTimePeriods(param);

            try
            {
                List<DataStorageAllDTO> dto = GeneralManager.GetTimePayrollSlipByEmployee(employeeId, param.ActorCompanyId, false, false);
                if (!dto.Any())
                    return new MobileEmployeeTimePeriods(param, Texts.PayrollPeriodNotFoundMessage);

                employeeTimePeriods.AddMobileEmployeeTimePeriods(dto);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileEmployeeTimePeriods(param, GetText(8778, "Ett fel inträffade"));
            }

            return employeeTimePeriods;
        }

        private MobileEmployeeTimePeriod PerformGetEmployeeTimePeriodDetails(MobileParam param, int employeeId, int timePeriodId)
        {
            MobileEmployeeTimePeriod employeeTimePeriods;

            try
            {
                TimePeriod period = TimePeriodManager.GetTimePeriod(timePeriodId, param.ActorCompanyId);
                int payrollSlipDataStorageId = GeneralManager.GetDataStorageId(SoeDataStorageRecordType.PayrollSlipXML, timePeriodId, employeeId, param.ActorCompanyId);
                EmployeeTimePeriod employeeTimePeriod = TimePeriodManager.GetEmployeeTimePeriod(employeeId, timePeriodId, param.ActorCompanyId, true);

                if (period == null || employeeTimePeriod == null || !period.PaymentDate.HasValue)
                    return new MobileEmployeeTimePeriod(param, Texts.PayrollPeriodNotFoundMessage);

                PayrollCalculationPeriodSumDTO periodSum = new PayrollCalculationPeriodSumDTO()
                {
                    Gross = employeeTimePeriod.GetGrossSalarySum(),
                    BenefitInvertExcluded = employeeTimePeriod.GetBenefitSum(),
                    Tax = employeeTimePeriod.GetTaxSum(),
                    Compensation = employeeTimePeriod.GetCompensationSum(),
                    Deduction = employeeTimePeriod.GetDeductionSum(),
                    EmploymentTaxDebit = employeeTimePeriod.GetEmploymentTaxCreditSum(),
                    Net = employeeTimePeriod.GetNetSum(),
                };

                employeeTimePeriods = new MobileEmployeeTimePeriod(param, periodSum, period, payrollSlipDataStorageId);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new MobileEmployeeTimePeriod(param, GetText(8778, "Ett fel inträffade"));
            }

            return employeeTimePeriods;
        }
        #endregion

        #region SalarySpecifikations

        private MobileFiles PerformGetSalarySpecifications(MobileParam param, int employeeId)
        {
            MobileFiles mobileFiles = new MobileFiles(param);

            //If mobile exclude Control Info
            List<DataStorageSmallDTO> timeSalaryImports = GeneralManager.GetTimeSalaryImportsByEmployee(employeeId, param.ActorCompanyId, true, false, new List<int>());
            mobileFiles.AddMobileFiles(timeSalaryImports);

            return mobileFiles;
        }

        private MobileFile PerformGetSalarySpecification(MobileParam param, int employeeId, int dataStorageId)
        {
            MobileFile mobileFile = null;
            SoeReportTemplateType soeReportTemplateType = SoeReportTemplateType.TimeSalarySpecificationReport;

            User user = UserManager.GetUser(param.UserId);
            if (user == null)
                return new MobileFile(param, Texts.UserNotFound);

            int defaultRoleId = UserManager.GetDefaultRoleId(param.ActorCompanyId, user);
            DataStorage dataStorage = GeneralManager.GetTimeSalaryImport(dataStorageId, employeeId, param.ActorCompanyId);

            //Include RoleId to check permissions
            int timeSalarySpecificationReportId;
            if (dataStorage != null && dataStorage.Type == (int)SoeDataStorageRecordType.PayrollSlipXML)
            {
                if (dataStorage.Data != null)
                {
                    var data = dataStorage.Data;

                    if (data != null)
                    {
                        string fileName = dataStorage.TimePeriod.Name + Constants.SOE_SERVER_FILE_PDF_SUFFIX;
                        string path = CreateFileOnServer(param, dataStorageId, fileName, data, isSalarySpecification: true);
                        mobileFile = new MobileFile(param, dataStorageId, fileName, path, "", null, false, null, "");
                        return mobileFile;
                    }
                }

                soeReportTemplateType = SoeReportTemplateType.PayrollSlip;
                timeSalarySpecificationReportId = ReportManager.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.DefaultPayrollSlipReport, SoeReportTemplateType.PayrollSlip, param.ActorCompanyId, user.UserId, null);
            }
            else if (dataStorage != null && dataStorage.Type == (int)SoeDataStorageRecordType.TimeKU10ExportEmployee)
            {
                timeSalarySpecificationReportId = ReportManager.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.TimeDefaultKU10Report, SoeReportTemplateType.KU10Report, param.ActorCompanyId, user.UserId, null);
            }
            else if (dataStorage != null && dataStorage.Type == (int)SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee)
            {
                timeSalarySpecificationReportId = ReportManager.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.TimeDefaultTimeSalaryControlInfoReport, SoeReportTemplateType.TimeSalaryControlInfoReport, param.ActorCompanyId, user.UserId, null);
            }
            else
            {
                timeSalarySpecificationReportId = ReportManager.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.TimeDefaultTimeSalarySpecificationReport, SoeReportTemplateType.TimeSalarySpecificationReport, param.ActorCompanyId, user.UserId, defaultRoleId);
            }

            if (timeSalarySpecificationReportId > 0)
            {
                Employee employee = EmployeeManager.GetEmployeeForUser(param.UserId, param.ActorCompanyId);
                if (employee == null)
                    return new MobileFile(param, GetText(5283, "Ingen anställd kopplad till inloggad användare"));
            }
            else
                return new MobileFile(param, GetText(5601, "Ingen standardrapport för lönesepec är upplagd på företaget"));

            Report report = ReportManager.GetReport(timeSalarySpecificationReportId, param.ActorCompanyId);
            if (report == null)
                return new MobileFile(param, GetText(5601, "Ingen standardrapport för lönesepec är upplagd på företaget"));

            if (dataStorage == null || dataStorage.TimePeriod == null)
                return new MobileFile(param, Texts.FileNotFound);

            EvaluatedSelection es = new EvaluatedSelection()
            {
                ActorCompanyId = param.ActorCompanyId,
                UserId = param.UserId,
                RoleId = param.RoleId,
                ReportTemplateType = soeReportTemplateType,
                IsReportStandard = report.Standard,
                ST_DataStorageId = dataStorageId,
                ReportTemplateId = report.ReportTemplateId,
                ReportId = timeSalarySpecificationReportId,
                ReportNr = report.ReportNr,
                ReportName = report.Name,
                ReportDescription = report.Description,
                ExportType = TermGroup_ReportExportType.Pdf,
            };

            ReportPrintoutDTO reportPrintout;
            if (soeReportTemplateType == SoeReportTemplateType.PayrollSlip)
                reportPrintout = ReportDataManager.PrintPayrollSlipData(es.ReportId, employeeId, dataStorage.TimePeriodId.Value, false);
            else
                reportPrintout = ReportDataManager.PrintReport(es).ToDTO(true, true);

            if (reportPrintout == null)
                return new MobileFile(param, GetText(5970, "Rapport kunde inte skrivas ut"));

            if (reportPrintout.Status != (int)TermGroup_ReportPrintoutStatus.Delivered)
            {
                if (reportPrintout.ResultMessage == (int)SoeReportDataResultMessage.ReportsNotAuthorized)
                    return new MobileFile(param, GetText(1156, "Du saknar behörighet för att visa den här sidan"));
                else
                    return new MobileFile(param, GetText(5970, "Rapport kunde inte skrivas ut"));
            }

            if (reportPrintout.Data != null)
            {
                string fileName = dataStorage.TimePeriod.Name + Constants.SOE_SERVER_FILE_PDF_SUFFIX;
                string path = CreateFileOnServer(param, dataStorageId, fileName, reportPrintout.Data, isSalarySpecification: true);
                mobileFile = new MobileFile(param, dataStorageId, fileName, path, "", null, false, null, "");
            }
            else
            {
                mobileFile = new MobileFile(param, GetText(8367, "Kunde inte skapa pdf"));
            }

            return mobileFile;
        }

        #endregion

        #endregion

        #endregion

        #region Help methods

        private bool IsMySelf(MobileParam param, int employeeId, Employee employee)
        {
            if (employee == null)
                employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId);

            if (employee == null)
                return false;

            Employee userEmployee = EmployeeManager.GetEmployeeForUser(param.UserId, param.ActorCompanyId);
            return employee.IsMySelf(userEmployee);
        }

        private void FilterAccounts(List<AccountDimSmallDTO> dims, List<AccountDTO> selectedAccounts, AccountDimSmallDTO parentDim = null)
        {
            if (parentDim == null)
                parentDim = dims.OrderBy(x => x.Level).FirstOrDefault();

            if (parentDim != null)
            {
                if (parentDim.Accounts == null)
                    return;

                List<AccountDTO> parentDimSelectedAccounts = new List<AccountDTO>();

                #region Decide parent account ids to filter child accounts with

                if (parentDim.CurrentSelectableAccounts == null || !parentDim.CurrentSelectableAccounts.Any())
                {
                    //Only first parent should end up in here
                    parentDim.CurrentSelectableAccounts = new List<AccountDTO>();
                    parentDim.CurrentSelectableAccounts = parentDim.Accounts.OrderBy(x => x.Name).ToList();
                }

                //Prio 1 : use selected accounts to filter child accounts
                if (selectedAccounts.Any(x => x.AccountDimId == parentDim.AccountDimId))
                {
                    var currentSelectedAccounts = selectedAccounts.Where(x => x.AccountDimId == parentDim.AccountDimId).ToList();
                    if (parentDim.CurrentSelectableAccounts != null)
                    {
                        //validate currentSelectedAccounts against CurrentSelectableAccounts
                        foreach (var currentSelectedAccount in currentSelectedAccounts)
                        {
                            if (parentDim.CurrentSelectableAccounts.Any(x => x.AccountId == currentSelectedAccount.AccountId))
                                parentDimSelectedAccounts.Add(currentSelectedAccount);
                        }
                    }
                    else
                    {
                        parentDimSelectedAccounts = currentSelectedAccounts;
                    }
                }

                //Prio 2 : use CurrentSelectableAccounts to filter child accounts
                if (!parentDimSelectedAccounts.Any() && parentDim.CurrentSelectableAccounts != null)
                    parentDimSelectedAccounts = parentDim.CurrentSelectableAccounts.ToList();

                #endregion

                var childDim = dims.FirstOrDefault(x => x.ParentAccountDimId == parentDim.AccountDimId);
                if (childDim != null)
                {
                    if (childDim.Accounts == null)
                        return; // shold not happen

                    childDim.CurrentSelectableAccounts = new List<AccountDTO>();

                    //add child accounts for parentdim selected accounts
                    foreach (var parentDimSelectedAccount in parentDimSelectedAccounts)
                        childDim.CurrentSelectableAccounts.AddRange(childDim.Accounts.Where(x => x.ParentAccountId.HasValue && x.ParentAccountId.Value == parentDimSelectedAccount.AccountId).ToList());

                    // Add accounts without parent
                    childDim.CurrentSelectableAccounts.AddRange(childDim.Accounts.Where(x => !x.ParentAccountId.HasValue).ToList());
                    childDim.CurrentSelectableAccounts = childDim.CurrentSelectableAccounts.OrderBy(x => x.Name).ToList();

                    FilterAccounts(dims, selectedAccounts, childDim);
                }


            }
        }

        private string GetPNId(User user)
        {
            if (user == null)
                return "";

            bool useGuid = SettingManager.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.PushNotificationUseGuid, 0, 0, 0);
            return StringUtility.GetPushNotificationId(user.UserId, useGuid ? user.idLoginGuid : (Guid?)null);
        }

        private string GetSetEmailURL(User user)
        {
            if (string.IsNullOrEmpty(user.Email))
            {
                string guid = user.idLoginGuid.HasValue ? user.idLoginGuid.ToString() : "";
                return string.Format("{0}/contactinfo.aspx?idloginguid={1}", "", guid);
            }
            return "";
        }

        private List<int> GetIds(string idsStr, char charSep = ',')
        {
            if (idsStr.IsNullOrEmpty())
                return new List<int>();

            List<int> ids = new List<int>();

            char[] separator = new char[1];
            separator[0] = charSep;

            string[] parsedIds = idsStr.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var parsedId in parsedIds)
            {
                if (Int32.TryParse(parsedId.Trim(), out int id))
                    ids.Add(id);
            }

            return ids;
        }

        private List<int> GetAccountsExcludeAccountsLinkedToShiftTypes(MobileParam param, List<int> accountIds)
        {
            List<int> accountsLinkedToShiftTypes = GetAccountsLinkedToShiftTypesFromAccountIds(param, accountIds);
            return accountIds.Where(x => !accountsLinkedToShiftTypes.Contains(x)).ToList();
        }

        private List<int> GetAccountsLinkedToShiftTypesFromAccountIds(MobileParam param, List<int> accountIds)
        {
            var accountDim = AccountManager.GetAccountDimsByCompany(param.ActorCompanyId).FirstOrDefault(x => x.LinkedToShiftType);
            if (accountDim == null)
                return new List<int>();

            var accounts = AccountManager.GetAccounts(accountIds, param.ActorCompanyId);
            return accounts.Where(x => x.AccountDimId == accountDim.AccountDimId).Select(x => x.AccountId).ToList();
        }

        private List<int> GetShiftAccountIdsFromAccountIds(MobileParam param, List<int> accountIds)
        {
            int defaultDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, param.ActorCompanyId, 0);
            var accounts = AccountManager.GetAccounts(accountIds, param.ActorCompanyId);
            return accounts.Where(x => x.AccountDimId == defaultDimId).Select(x => x.AccountId).ToList();
        }

        private Dictionary<int, string> SchedulePlanningGetEmployees(MobileParam param, int employeeId, MobileDisplayMode displayMode, bool showAll = false, bool includeSecondaryCategoriesOrAccounts = false, bool getHidden = true, bool addEmptyRow = false, bool addNoReplacementEmployee = false, bool addSearchEmployee = false, DateTime? from = null, DateTime? to = null, string accountIdsStr = "")
        {
            DateTime dateFrom = from ?? DateTime.Today;
            DateTime dateTo = to ?? DateTime.Today;

            Dictionary<int, string> employeesDict = new Dictionary<int, string>();
            if (addEmptyRow)
                employeesDict.Add(0, " ");

            if (addNoReplacementEmployee)
                employeesDict.Add(Constants.NO_REPLACEMENT_EMPLOYEEID, GetText(8262, "Ingen ersättare"));

            if (addSearchEmployee)
                employeesDict.Add(Constants.SEARCH_EMPLOYEE_EMPLOYEEID, GetText(8795, "Sök anställd"));

            // Account hierarchy
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, param.ActorCompanyId);
            if (useAccountHierarchy)
            {
                List<int> accountIds = this.GetIds(accountIdsStr);
                List<int> accountsNotLinkedToShiftTypes = this.GetAccountsExcludeAccountsLinkedToShiftTypes(param, accountIds);

                #region AccountHierarchy

                if (getHidden)
                {
                    Employee hiddenEmployee = EmployeeManager.GetHiddenEmployee(param.ActorCompanyId, true);
                    if (hiddenEmployee != null)
                        employeesDict.Add(hiddenEmployee.EmployeeId, hiddenEmployee.EmployeeNrAndName);
                }

                if (showAll)
                {
                    List<Employee> employees = EmployeeManager.GetEmployeesForCompanyWithEmployment(entitiesReadOnly, param.ActorCompanyId, param.UserId, true, dateFrom, dateTo, forceDefaultDim: true);
                    if (!employees.IsNullOrEmpty())
                        employeesDict.AddRange(employees.Select(e => new KeyValuePair<int, string>(e.EmployeeId, e.EmployeeNrAndName)));
                }
                else
                {
                    List<int> employeeIds = (from e in entitiesReadOnly.Employee
                                             where e.ActorCompanyId == param.ActorCompanyId &&
                                             (!e.Hidden) &&
                                             e.State == (int)SoeEntityState.Active
                                             select e.EmployeeId).ToList();

                    Employee currentEmployee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId);

                    AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
                    input.AddParamValue(AccountHierarchyParamType.OnlyDefaultAccounts, !includeSecondaryCategoriesOrAccounts);
                    employeeIds = EmployeeManager.GetValidEmployeeByAccountHierarchy(entitiesReadOnly, param.ActorCompanyId, param.RoleId, param.UserId, employeeIds, currentEmployee, dateFrom, dateTo, useShowOtherEmployeesPermission: true, addAccountHierarchyInfo: false, onlyDefaultAccounts: !includeSecondaryCategoriesOrAccounts, ignoreAttestRoles: displayMode == MobileDisplayMode.User, accountIds: accountsNotLinkedToShiftTypes, input: input);

                    List<Employee> employees = (from e in entitiesReadOnly.Employee
                                                    .Include("ContactPerson")
                                                    .Include("EmployeeAccount.Children")
                                                where e.ActorCompanyId == param.ActorCompanyId &&
                                                employeeIds.Contains(e.EmployeeId)
                                                select e).ToList();

                    foreach (var employee in employees)
                    {
                        bool isValid = false;

                        // GetValidEmployeeByAccountHierarchy only checks accounts not dates
                        // Check that employee account dates are within specified year
                        // Only check main level and for the whole year, more detailed checks are made on the client
                        if (employee.EmployeeAccount != null && employee.EmployeeAccount.Any())
                        {
                            foreach (EmployeeAccount account in employee.EmployeeAccount)
                            {
                                if (account.DateFrom <= dateTo && (!account.DateTo.HasValue || account.DateTo.Value >= dateFrom))
                                {
                                    isValid = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            isValid = true;
                        }

                        if (isValid)
                            employeesDict.Add(employee.EmployeeId, employee.EmployeeNrAndName);
                    }
                }

                #endregion
            }
            else
            {
                #region Categories

                if (displayMode == MobileDisplayMode.User)
                {
                    if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_SeeOtherEmployeesShifts, Permission.Modify, param.RoleId, param.ActorCompanyId))
                    {
                        Dictionary<int, string> categories = CategoryManager.GetCategoriesForRoleFromTypeDict(param.ActorCompanyId, param.UserId, employeeId, SoeCategoryType.Employee, false, includeSecondaryCategoriesOrAccounts, false);
                        employeesDict.AddRange(EmployeeManager.GetEmployeesDictByCategories(param.ActorCompanyId, categories.Select(x => x.Key).ToList(), false, getHidden, false));
                    }
                    else
                    {
                        if (getHidden)
                        {
                            Employee hiddenEmployee = EmployeeManager.GetHiddenEmployee(param.ActorCompanyId);
                            if (hiddenEmployee != null)
                                employeesDict.Add(hiddenEmployee.EmployeeId, hiddenEmployee.EmployeeNrAndName);
                        }
                    }
                }
                else if (displayMode == MobileDisplayMode.Admin)
                {
                    Dictionary<int, string> categories = CategoryManager.GetCategoriesForRoleFromTypeDict(param.ActorCompanyId, param.UserId, 0, SoeCategoryType.Employee, true, includeSecondaryCategoriesOrAccounts, false);
                    employeesDict.AddRange(EmployeeManager.GetEmployeesDictByCategories(param.ActorCompanyId, categories.Select(x => x.Key).ToList(), false, getHidden, false));
                }

                #endregion
            }

            return employeesDict;
        }

        private Dictionary<int, string> SchedulePlanningGetShiftTypes(MobileParam param, int employeeId, MobileDisplayMode displayMode, bool includeSecondaryCategoriesOrAccounts, string selectedAccountIdsStr)
        {
            List<int> validAccountIds = new List<int>();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, param.ActorCompanyId);

            if (useAccountHierarchy && !selectedAccountIdsStr.IsNullOrEmpty())
            {
                MobileAccountDims dims = this.GetAccountHierarchyDims(param, selectedAccountIdsStr, includeSecondaryCategoriesOrAccounts, true);
                validAccountIds.AddRange(dims.GetValidAccountIds());
            }

            return TimeScheduleManager.GetShiftTypesDictForUser(useAccountHierarchy, param.ActorCompanyId, param.RoleId, param.UserId, employeeId, displayMode == MobileDisplayMode.Admin, null, null, includeSecondaryCategoriesOrAccounts, validAccountIds: validAccountIds);
        }

        private MobileMessageBox ValidateSkills(MobileParam param, int shiftId, bool includeLinkedShifts, MobileDisplayMode displayMode, int targetEmployeeId)
        {
            MobileMessageBox mobileMessageBox = null;

            List<Employee> employeesWithSkills = new List<Employee>();
            List<TimeScheduleTemplateBlock> templateBlocks = new List<TimeScheduleTemplateBlock>();
            bool skillsMatch = true;
            if (includeLinkedShifts)
            {
                templateBlocks = TimeScheduleManager.GetLinkedTimeScheduleTemplateBlocks(null, param.ActorCompanyId, shiftId);
            }
            else
            {
                var templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId);
                templateBlocks.Add(templateBlock);
            }

            if (!templateBlocks.Any())
                return new MobileMessageBox(param, GetText(8503, "Passet kunde inte hittas"));

            Employee employee = EmployeeManager.GetEmployee(targetEmployeeId, param.ActorCompanyId, loadEmployeeSkill: true);
            if (employee != null)
                employeesWithSkills.Add(employee);

            foreach (var templateBlock in templateBlocks)
            {
                if (templateBlock.IsBreak || !templateBlock.EmployeeId.HasValue || !templateBlock.ShiftTypeId.HasValue)
                    continue;

                List<int> empIds = TimeScheduleManager.MatchEmployeesByShiftTypeSkills(templateBlock.ShiftTypeId.Value, param.ActorCompanyId, employeesWithSkills);
                skillsMatch = empIds.Contains(targetEmployeeId);
                if (!skillsMatch)
                    break;

            }

            if (!skillsMatch)
            {
                string title = displayMode == MobileDisplayMode.User ? "" : GetText(8480, "Observera");
                string message = displayMode == MobileDisplayMode.User ? GetText(10936, "Du uppfyller inte kompetenskraven för ett eller flera pass.") : GetText(8481, "Den anställde uppfyller inte kompetenskraven för ett eller flera pass.");

                message += GetText(8493, "Vill du spara ändå?");
                mobileMessageBox = new MobileMessageBox(param, false, true, true, true, title, message);
            }
            else
            {
                mobileMessageBox = new MobileMessageBox(param, true, false, false, false, "", "");
            }

            return mobileMessageBox;
        }

        private string GetErrorMessage(ActionResultSave error, string errorMessageInput)
        {
            string errorMessage = string.Empty;
            switch (error)
            {
                case ActionResultSave.TimeSchedulePlanning_ShiftIsNull:
                    errorMessage += GetText(8498, "Inget pass att spara");
                    break;
                case ActionResultSave.TimeSchedulePlanning_UserNotFound:
                    errorMessage += GetText(8499, "Användare ej funnen");
                    break;
                case ActionResultSave.TimeSchedulePlanning_HiddenEmployeeNotFound:
                    errorMessage += GetText(8500, "Planering ej aktiverad");
                    break;
                case ActionResultSave.TimeSchedulePlanning_PeriodNotFound:
                    errorMessage += GetText(8501, "Anställd har ej aktiverat schema för dagen");
                    break;
                case ActionResultSave.TimeSchedulePlanning_PreliminaryNotUpdated:
                    errorMessage += GetText(8502, "Kan ej ändra status på passen, transaktioner är attesterade");
                    break;
                default:
                    errorMessage = errorMessageInput;
                    break;
            }

            return errorMessage;
        }

        private string GetProjectLockedOrFinishedMessage(Project project)
        {
            string projectStatus = string.Empty;
            string msg = string.Empty;
            if (project.Status == (int)TermGroup_ProjectStatus.Locked)
                projectStatus = GetText(8564, "låst");
            else if (project.Status == (int)TermGroup_ProjectStatus.Finished)
                projectStatus = GetText(8565, "avslutat");
            else
                projectStatus = "";

            msg = string.Format(GetText(8566, "Projektet kopplat till ordern är {0}. Projektet måste först aktiveras, det görs i webbsystemet."), projectStatus);
            return msg;
        }

        private List<CustomerInvoiceRowDTO> GetUnattestedRows(List<CustomerInvoiceRowDTO> rows, AttestState initialAttestState)
        {
            return rows.Where(i => (!i.AttestStateId.HasValue || (initialAttestState != null && i.AttestStateId.Value == initialAttestState.AttestStateId))).ToList();
        }

        private List<TimeSchedulePlanningDayDTO> AbsencePlanningCreateTimescheduleplanningDtos(MobileParam param, MobileSaveShifts saveShifts, int employeeId, bool isTimeModule, ref string errorMessage)
        {
            //employeeId in parsedShifts is destinationemployeeId

            var inputShifts = saveShifts.ParsedShifts.Where(x => x.EmployeeId != 0).ToList(); // employeeid = 0 if no replacement has been made

            List<TimeSchedulePlanningDayDTO> shiftDtos = new List<TimeSchedulePlanningDayDTO>();

            foreach (var item in saveShifts.ParsedShifts)
            {
                if (item.EmployeeId == employeeId)
                {
                    errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8483, "Ett eller flera pass har ett ogiltigt anställdaid.");
                    return new List<TimeSchedulePlanningDayDTO>();
                }
            }


            foreach (var inputShift in inputShifts)
            {
                if (inputShift.ShiftId != 0)
                {
                    //get shift from db and update it with input values
                    TimeScheduleTemplateBlock templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(inputShift.ShiftId, employeeId);
                    if (templateBlock != null)
                    {
                        TimeSchedulePlanningDayDTO shiftDto = TimeScheduleManager.CreateTimeSchedulePlanningDayDTO(templateBlock, param.ActorCompanyId, 0, dontChangeTimeOnZeroDays: true);
                        if (shiftDto == null || shiftDto.TimeDeviationCauseId.HasValue)
                            continue;

                        //update shiftdto with input values              

                        shiftDto.UniqueId = Guid.NewGuid().ToString();
                        shiftDto.AbsenceStartTime = inputShift.AbsenceStartTime.Value;
                        shiftDto.AbsenceStopTime = inputShift.AbsenceStopTime.Value;
                        shiftDto.EmployeeId = inputShift.EmployeeId;
                        shiftDto.ApprovalTypeId = (int)TermGroup_YesNo.Yes;

                        //fix for zero days, only NO_REPLACEMENT is allowed
                        if ((shiftDto.StartTime == shiftDto.StopTime && shiftDto.EmployeeId != Constants.NO_REPLACEMENT_EMPLOYEEID) || isTimeModule)
                        {
                            shiftDto.EmployeeId = Constants.NO_REPLACEMENT_EMPLOYEEID;
                        }

                        shiftDtos.Add(shiftDto);
                    }
                }
            }

            return shiftDtos;
        }

        private List<TimeSchedulePlanningDayDTO> AbsenceRequestPlanningCreateTimescheduleplanningDtos(MobileParam param, MobileSaveAbsenceRequestShifts saveShifts, int employeeId, ref string errorMessage)
        {
            List<TimeSchedulePlanningDayDTO> shiftDtos = new List<TimeSchedulePlanningDayDTO>();

            foreach (var item in saveShifts.ParsedShifts)
            {
                if (item.EmployeeId == employeeId)
                {
                    errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8483, "Ett eller flera pass har ett ogiltigt anställdaid.");
                    return new List<TimeSchedulePlanningDayDTO>();
                }
            }

            //get shifts from db and update them with input values
            List<TimeScheduleTemplateBlock> templateBlocks = TimeScheduleManager.GetTimeScheduleTemplateBlocks(saveShifts.ParsedShifts.Select(x => x.Id).ToList(), employeeId, true);

            foreach (var inputShift in saveShifts.ParsedShifts)
            {
                if (inputShift.Id != 0)
                {
                    TimeScheduleTemplateBlock templateBlock = templateBlocks.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == inputShift.Id);
                    if (templateBlock != null)
                    {
                        if (templateBlock.TimeDeviationCauseId.HasValue)
                            continue;

                        TimeSchedulePlanningDayDTO shiftDto = TimeScheduleManager.CreateTimeSchedulePlanningDayDTO(templateBlock, param.ActorCompanyId, 0, dontChangeTimeOnZeroDays: true);

                        //update shiftdto with input values              

                        shiftDto.UniqueId = Guid.NewGuid().ToString();
                        shiftDto.AbsenceStartTime = inputShift.AbsenceStartTime;
                        shiftDto.AbsenceStopTime = inputShift.AbsenceStopTime;
                        shiftDto.EmployeeId = inputShift.EmployeeId;
                        shiftDto.ApprovalTypeId = (int)inputShift.ApprovalType;

                        //fix for zero days, only NO_REPLACEMENT is allowed
                        if (shiftDto.StartTime == shiftDto.StopTime && shiftDto.EmployeeId != Constants.NO_REPLACEMENT_EMPLOYEEID)
                        {
                            shiftDto.EmployeeId = Constants.NO_REPLACEMENT_EMPLOYEEID;
                        }

                        shiftDtos.Add(shiftDto);
                    }
                }
            }

            return shiftDtos;
        }

        private List<TimeSchedulePlanningDayDTO> ValidateSaveShiftsCreateTimescheduleplanningDtos(MobileParam param, MobileSaveShifts saveShifts, int employeeId, DateTime actualDate, ref string errorMessage)
        {
            List<TimeSchedulePlanningDayDTO> shiftDtos = new List<TimeSchedulePlanningDayDTO>();
            var timeCodeBreaks = TimeCodeManager.GetTimeCodeBreaks(param.ActorCompanyId, true);
            var shiftTypes = TimeScheduleManager.GetShiftTypes(param.ActorCompanyId);

            int nbrOfBreaks = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeMaxNoOfBrakes, 0, param.ActorCompanyId, 0);
            bool shiftTypeIsMandatory = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeShiftTypeMandatory, 0, param.ActorCompanyId, 0);
            bool keepShiftsTogether = !SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultDoNotKeepShiftsTogether, 0, param.ActorCompanyId, 0);

            var inputActiveShiftsAndBreaks = saveShifts.ParsedShifts.Where(x => !x.IsDeleted).ToList();
            var inputShifts = saveShifts.ParsedShifts.Where(x => !x.IsBreak).ToList();
            var inputBreaks = saveShifts.ParsedShifts.Where(x => x.IsBreak).ToList();

            #region fix wholes with breaks inside

            HasHolesWithBreaksInside(inputActiveShiftsAndBreaks, true);
            #endregion

            #region Validate employeeId

            if (inputActiveShiftsAndBreaks.Any(x => x.EmployeeId != employeeId))
            {
                errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8483, "Ett eller flera pass har ett ogiltigt anställdaid.");
                return new List<TimeSchedulePlanningDayDTO>();
            }

            #endregion

            #region Validate Start And Stop date
            //Validate midnight
            if (inputActiveShiftsAndBreaks.Any(x => x.StopTime < x.StartTime))
            {
                errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8484, "Ett eller flera pass slutar innan det startar.");
                return new List<TimeSchedulePlanningDayDTO>();
            }

            //Validate startdate
            if (inputActiveShiftsAndBreaks.Any(x => x.StartTime.Date < actualDate.Date))
            {
                errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8485, "Ett eller flera pass har ett startdatum innan det faktiska datumet.");
                return new List<TimeSchedulePlanningDayDTO>();
            }

            //Validate stopdate
            if (inputActiveShiftsAndBreaks.Any(x => x.StopTime.Date > actualDate.Date.AddDays(2)))
            {
                errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8486, "Ett eller flera pass har ett felaktigt stopdatum.");
                return new List<TimeSchedulePlanningDayDTO>();
            }

            //validate startsAfterMidnight
            if (inputActiveShiftsAndBreaks.Any(x => x.StartsAfterMidnight))
            {
                var startsAfterMidnightShifts = inputActiveShiftsAndBreaks.Where(x => x.StartsAfterMidnight).ToList();
                if (startsAfterMidnightShifts.Any(x => x.StartTime.Date != actualDate.Date.AddDays(1)))
                {
                    errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8487, "Ett eller flera pass som börjar efter midnatt har ett felaktigt start eller slutdatum");
                    return new List<TimeSchedulePlanningDayDTO>();
                }
            }

            #endregion

            #region Validate shift type

            if (shiftTypeIsMandatory)
            {
                foreach (var item in inputActiveShiftsAndBreaks.Where(x => !x.IsBreak))
                {
                    if (item.ShiftTypeId == 0)
                    {
                        errorMessage = GetText(8488, "Ett eller flera pass saknar passtyp.");
                        return new List<TimeSchedulePlanningDayDTO>();
                    }
                }
            }

            #endregion            

            #region Validate breaks

            foreach (var item in inputActiveShiftsAndBreaks.Where(x => x.IsBreak))
            {
                if (item.TimeCodeBreakId == 0)
                {
                    errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8489, "En eller flera raster saknar tidkod.");
                    return new List<TimeSchedulePlanningDayDTO>();
                }

                var timeCode = timeCodeBreaks.FirstOrDefault(x => x.TimeCodeId != 0 && x.TimeCodeId == item.TimeCodeBreakId);
                if (timeCode == null)
                {
                    errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8491, "En eller flera raster har felaktig tidkod, tidkod kunde inte hittas.");
                    return new List<TimeSchedulePlanningDayDTO>();
                }
                else
                {
                    if ((item.StopTime - item.StartTime).TotalMinutes != timeCode.DefaultMinutes)
                    {
                        errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8492, "En eller flera raster har felaktig tidkod, rastens längd stämmer inte överens med inställningen på tidkoden.");
                        return new List<TimeSchedulePlanningDayDTO>();
                    }
                }
            }

            if (inputActiveShiftsAndBreaks.Any() && inputActiveShiftsAndBreaks.Count(x => x.IsBreak) == inputActiveShiftsAndBreaks.Count)
            {
                errorMessage = GetText(8510, "Du kan ej spara en dag som endast innehåller raster.");
                return new List<TimeSchedulePlanningDayDTO>();
            }

            if (inputActiveShiftsAndBreaks.Any(x => !x.IsBreak))
            {
                var firstShift = inputActiveShiftsAndBreaks.Where(x => !x.IsBreak).OrderBy(x => x.StartTime).FirstOrDefault();
                var lastShift = inputActiveShiftsAndBreaks.Where(x => !x.IsBreak).OrderBy(x => x.StopTime).LastOrDefault();


                if (inputActiveShiftsAndBreaks.Any(x => x.IsBreak))
                {
                    var firstBreak = inputActiveShiftsAndBreaks.Where(x => x.IsBreak).OrderBy(x => x.StartTime).FirstOrDefault();
                    var lastBreak = inputActiveShiftsAndBreaks.Where(x => x.IsBreak).OrderBy(x => x.StopTime).LastOrDefault();
                    if (firstBreak != null && firstShift != null)
                    {
                        if (firstBreak.StartTime == firstShift.StartTime)
                        {
                            errorMessage = GetText(8634, "Dagen får inte starta med en rast");
                            return new List<TimeSchedulePlanningDayDTO>();
                        }
                        if (firstBreak.StartTime < firstShift.StartTime)
                        {
                            errorMessage = GetText(8511, "En eller flera raster ligger utanför arbetstiden.");
                            return new List<TimeSchedulePlanningDayDTO>();
                        }
                    }
                    if (lastBreak.StopTime > lastShift.StopTime)
                    {
                        errorMessage = GetText(8511, "En eller flera raster ligger utanför arbetstiden.");
                        return new List<TimeSchedulePlanningDayDTO>();
                    }
                }
            }

            if (inputActiveShiftsAndBreaks.Count(x => x.IsBreak) > nbrOfBreaks)
            {
                errorMessage = String.Format(GetText(8490, "Dagen innehåller {0} raster, men företagsinställningen tillåter endast {1}"), inputActiveShiftsAndBreaks.Count(x => x.IsBreak), nbrOfBreaks);
                return new List<TimeSchedulePlanningDayDTO>();
            }

            #endregion

            #region Update/Create dtos

            foreach (var inputShift in inputShifts)
            {

                if (inputShift.ShiftId != 0)
                {
                    //get shift from db and update it with input values
                    TimeScheduleTemplateBlock templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(inputShift.ShiftId, employeeId);
                    if (templateBlock != null)
                    {
                        TimeSchedulePlanningDayDTO shiftDto = TimeScheduleManager.CreateTimeSchedulePlanningDayDTO(templateBlock, param.ActorCompanyId, 0);

                        if (shiftDto.TimeDeviationCauseId.HasValue)
                            continue;

                        //update shiftdto with input values              

                        shiftDto.UniqueId = Guid.NewGuid().ToString();
                        shiftDto.StartTime = inputShift.StartTime;
                        shiftDto.StopTime = inputShift.StopTime;
                        shiftDto.ShiftTypeId = inputShift.ShiftTypeId;
                        shiftDto.TimeScheduleTypeId = inputShift.ScheduleTypeId;
                        shiftDto.BelongsToPreviousDay = inputShift.StartsAfterMidnight;
                        shiftDto.IsDeleted = inputShift.IsDeleted;
                        shiftDto.AccountId = inputShift.AccountId != 0 ? inputShift.AccountId : (int?)null;
                        shiftDto.ExtraShift = inputShift.isExtraShift;
                        shiftDto.SubstituteShift = inputShift.isSubStituteShift;
                        if (shiftDto.IsDeleted)
                        {
                            //Zero day
                            shiftDto.StartTime = shiftDto.StartTime.Date;
                            shiftDto.StopTime = shiftDto.StartTime.Date;
                        }

                        shiftDtos.Add(shiftDto);
                    }

                }
                else
                {
                    //we are not interested in a deleted but not existing shift
                    if (inputShift.IsDeleted)
                        continue;

                    //create a new shift with input values
                    TimeSchedulePlanningDayDTO newShiftDto = new TimeSchedulePlanningDayDTO();

                    if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_31))
                        newShiftDto.Type = (TermGroup_TimeScheduleTemplateBlockType)inputShift.type;
                    else
                        newShiftDto.Type = TermGroup_TimeScheduleTemplateBlockType.Schedule;

                    newShiftDto.UniqueId = Guid.NewGuid().ToString();
                    newShiftDto.EmployeeId = employeeId;
                    newShiftDto.StartTime = inputShift.StartTime;
                    newShiftDto.StopTime = inputShift.StopTime;
                    newShiftDto.ShiftTypeId = inputShift.ShiftTypeId;
                    newShiftDto.TimeScheduleTypeId = inputShift.ScheduleTypeId;
                    newShiftDto.BelongsToPreviousDay = inputShift.StartsAfterMidnight;
                    newShiftDto.AccountId = inputShift.AccountId != 0 ? inputShift.AccountId : (int?)null;
                    newShiftDto.ExtraShift = inputShift.isExtraShift;
                    newShiftDto.SubstituteShift = inputShift.isSubStituteShift;

                    if (newShiftDto.ShiftTypeId != 0 && newShiftDto.TimeScheduleTypeId == 0)
                    {
                        var shiftType = shiftTypes.FirstOrDefault(x => x.ShiftTypeId == newShiftDto.ShiftTypeId);
                        if (shiftType != null && shiftType.TimeScheduleTypeId.HasValue)
                            newShiftDto.TimeScheduleTypeId = shiftType.TimeScheduleTypeId.Value;
                    }

                    shiftDtos.Add(newShiftDto);
                }

            }

            if (shiftDtos.Count > 0 && shiftDtos.Count(x => x.IsDeleted) == shiftDtos.Count)
            {
                //if all shifts has been deleted, then set one of them to active
                var firstShift = shiftDtos.FirstOrDefault();
                firstShift.IsDeleted = false;
            }

            #endregion

            #region Set links on new shifts

            if (keepShiftsTogether)
            {
                //Om det endast finns en unik guid på dagen, använd den annars får alla nya pass från mobilen samma guid

                List<Guid> existingGuids = new List<Guid>();
                foreach (var shiftDto in shiftDtos.Where(x => x.TimeScheduleTemplateBlockId != 0))
                {
                    if (shiftDto.Link.HasValue && !existingGuids.Any(x => x == shiftDto.Link.Value))
                        existingGuids.Add(shiftDto.Link.Value);
                }

                Guid? guid = null;
                if (existingGuids.Count == 1)
                    guid = existingGuids.FirstOrDefault();
                else
                    guid = Guid.NewGuid();

                foreach (var shiftDto in shiftDtos.Where(x => x.TimeScheduleTemplateBlockId == 0))
                {
                    shiftDto.Link = guid;
                }
            }
            else
            {
                foreach (var shiftDto in shiftDtos.Where(x => x.TimeScheduleTemplateBlockId == 0))
                {
                    shiftDto.Link = Guid.NewGuid();
                }

            }

            #endregion

            #region Update breaks on Shiftdto

            int breakNr = 0;
            foreach (var inputBreak in inputBreaks.OrderBy(x => x.StartTime))
            {
                if (inputBreak.IsDeleted)
                    continue;

                breakNr++;
                Guid? breakGuid = null;
                var shift = shiftDtos.FirstOrDefault(x => x.StartTime <= inputBreak.StartTime && x.StopTime >= inputBreak.StopTime);
                if (shift != null)
                    breakGuid = shift.Link;

                foreach (var shiftDto in shiftDtos)
                {
                    var timeCode = timeCodeBreaks.FirstOrDefault(x => x.TimeCodeId == inputBreak.TimeCodeBreakId);

                    if (breakNr == 1)
                    {
                        shiftDto.Break1Id = inputBreak.ShiftId;
                        shiftDto.Break1Link = breakGuid;
                        shiftDto.Break1Minutes = (timeCode != null) ? timeCode.DefaultMinutes : 0;
                        shiftDto.Break1StartTime = inputBreak.StartTime;
                        shiftDto.Break1TimeCodeId = (timeCode != null) ? timeCode.TimeCodeId : 0;
                    }

                    if (breakNr == 2)
                    {
                        shiftDto.Break2Id = inputBreak.ShiftId;
                        shiftDto.Break2Link = breakGuid;
                        shiftDto.Break2Minutes = (timeCode != null) ? timeCode.DefaultMinutes : 0;
                        shiftDto.Break2StartTime = inputBreak.StartTime;
                        shiftDto.Break2TimeCodeId = (timeCode != null) ? timeCode.TimeCodeId : 0;
                    }

                    if (breakNr == 3)
                    {
                        shiftDto.Break3Id = inputBreak.ShiftId;
                        shiftDto.Break3Link = breakGuid;
                        shiftDto.Break3Minutes = (timeCode != null) ? timeCode.DefaultMinutes : 0;
                        shiftDto.Break3StartTime = inputBreak.StartTime;
                        shiftDto.Break3TimeCodeId = (timeCode != null) ? timeCode.TimeCodeId : 0;
                    }

                    if (breakNr == 4)
                    {
                        shiftDto.Break4Id = inputBreak.ShiftId;
                        shiftDto.Break4Link = breakGuid;
                        shiftDto.Break4Minutes = (timeCode != null) ? timeCode.DefaultMinutes : 0;
                        shiftDto.Break4StartTime = inputBreak.StartTime;
                        shiftDto.Break4TimeCodeId = (timeCode != null) ? timeCode.TimeCodeId : 0;
                    }

                }
            }

            #endregion

            return shiftDtos;
        }

        private List<TimeSchedulePlanningDayDTO> SaveOrderShiftsCreateTimescheduleplanningDtos(MobileParam param, int shiftId, int orderId, int employeeId, DateTime startTime, DateTime stopTime, int shiftTypeId, ref string errorMessage)
        {
            if (shiftTypeId == 0)
            {
                bool shiftTypeIsMandatory = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeShiftTypeMandatory, 0, param.ActorCompanyId, 0);
                if (shiftTypeIsMandatory)
                {
                    errorMessage = GetText(8649, "Du måste ange uppdragstyp");
                    return new List<TimeSchedulePlanningDayDTO>();
                }
            }

            if (stopTime < startTime)
            {
                errorMessage = GetText(8496, "Felaktigt data") + " : " + GetText(8650, "Uppdraget slutar innan det börjar.");
                return new List<TimeSchedulePlanningDayDTO>();
            }

            Employee employee = EmployeeManager.GetEmployee(employeeId, param.ActorCompanyId);
            if (employee == null)
            {
                errorMessage = Texts.EmployeeNotFoundMessage;
                return new List<TimeSchedulePlanningDayDTO>();
            }

            OrderListDTO order = TimeScheduleManager.GetOrderListDTO(param.ActorCompanyId, orderId);
            if (order == null)
            {
                errorMessage = GetText(5605, "Ordern hittades inte");
                return new List<TimeSchedulePlanningDayDTO>();
            }

            var employeeIds = new List<int>() { employeeId };

            List<TimeSchedulePlanningDayDTO> shifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, 0, startTime.Date, stopTime.Date, employeeIds, TimeSchedulePlanningMode.OrderPlanning, TimeSchedulePlanningDisplayMode.User, false, true, false, includePreliminary: false);
            if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_42))
            {
                shifts = shifts.Where(x => x.ActualDate.Date == startTime.Date).ToList();
            }
            
            TimeSchedulePlanningDayDTO newOrUpdatedShift = shifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == shiftId);
            if (shiftId > 0 && newOrUpdatedShift == null)
            {
                //User may have changed dates
                TimeScheduleTemplateBlock shift = TimeScheduleManager.GetTimeScheduleTemplateBlock(shiftId, employeeId);
                if (shift != null && shift.Date.HasValue && (shift.Date.Value != startTime.Date || shift.Date.Value != stopTime.Date))
                {
                    List<TimeSchedulePlanningDayDTO> oldshifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(param.ActorCompanyId, param.UserId, employeeId, 0, shift.Date.Value, shift.Date.Value, employeeIds, TimeSchedulePlanningMode.OrderPlanning, TimeSchedulePlanningDisplayMode.User, false, true, false, includePreliminary: false);
                    newOrUpdatedShift = oldshifts.FirstOrDefault(x => x.TimeScheduleTemplateBlockId == shiftId);
                }

                if (newOrUpdatedShift == null)
                {
                    errorMessage = GetText(8503, "Passet kunde inte hittas");
                    return new List<TimeSchedulePlanningDayDTO>();
                }
            }
            var firstShift = shifts.FirstOrDefault(); //only one shift is needed to calculate breaktime

            // Check if any scheduled breaks exists
            int breakLength;
            if (Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_42))
                breakLength = firstShift != null ? firstShift.GetBreakTimeWithinShift(startTime, startTime.AddMinutes((stopTime - startTime).TotalMinutes)) : 0;
            else
                breakLength = firstShift != null ? firstShift.GetBreakLength(startTime, stopTime) : 0;

            TimeSpan remainingTime = CalendarUtility.MinutesToTimeSpan(order.RemainingTime);
            TimeSpan originalLength = (shiftId > 0 && newOrUpdatedShift != null) ? (newOrUpdatedShift.StopTime - newOrUpdatedShift.StartTime) : new TimeSpan();
            TimeSpan plannedTime = (stopTime - startTime).Subtract(TimeSpan.FromMinutes(breakLength));
            TimeSpan remainingTimeToBe = remainingTime.Add(originalLength).Subtract(plannedTime);
            if (remainingTimeToBe.TotalMinutes < 0)
            {
                errorMessage = GetText(8651, "Du kan inte planera in mer tid än vad som återstår på ordern.\nGå i så fall först in på ordern och öka den återstående tiden.");
                return new List<TimeSchedulePlanningDayDTO>();
            }

            if (newOrUpdatedShift == null)
            {
                //create a new shift with input values
                newOrUpdatedShift = new TimeSchedulePlanningDayDTO();
                newOrUpdatedShift.UniqueId = Guid.NewGuid().ToString();
                newOrUpdatedShift.Type = TermGroup_TimeScheduleTemplateBlockType.Order;
                newOrUpdatedShift.EmployeeId = employeeId;
            }

            newOrUpdatedShift.StartTime = startTime;
            newOrUpdatedShift.StopTime = stopTime;
            newOrUpdatedShift.ShiftTypeId = shiftTypeId;
            newOrUpdatedShift.Order = order;

            List<TimeSchedulePlanningDayDTO> dtos = new List<TimeSchedulePlanningDayDTO>();
            bool splitOrderShiftBasedOnBreaks = breakLength > 0;

            if (splitOrderShiftBasedOnBreaks)
            {
                DateTime start = newOrUpdatedShift.StartTime;
                DateTime end = newOrUpdatedShift.StopTime;

                List<BreakDTO> breaks = firstShift != null ? firstShift.GetBreaks() : new List<BreakDTO>();
                foreach (var brk in breaks)
                {

                    DateTime breakStartTime = CalendarUtility.MergeDateAndTime(newOrUpdatedShift.StartTime.Date, brk.StartTime.TimeOfDay);
                    DateTime breakStopTime = breakStartTime.AddMinutes(brk.BreakMinutes);

                    // Calculate break time inside shift
                    if (CalendarUtility.IsNewOverlappedByCurrent(breakStartTime, breakStopTime, start, end))
                    {
                        // Break is completely overlapped by a presence shift
                        // Split shift
                        TimeSchedulePlanningDayDTO clone = newOrUpdatedShift.CopyAsNew();
                        clone.StartTime = start;
                        clone.StopTime = breakStartTime;
                        dtos.Add(clone);

                        start = breakStopTime;
                    }
                    else if (CalendarUtility.IsNewOverlappingCurrentStart(breakStartTime, breakStopTime, start, end))
                    {
                        // Break end intersects with a presence shift
                        // This is OK
                        TimeSchedulePlanningDayDTO clone = newOrUpdatedShift.CopyAsNew();
                        clone.StartTime = start;
                        clone.StopTime = end;
                        dtos.Add(clone);

                        start = end;
                    }
                    else if (CalendarUtility.IsNewOverlappingCurrentStop(breakStartTime, breakStopTime, start, end))
                    {
                        // Break start intersects with a presence shift
                        // This is OK
                        TimeSchedulePlanningDayDTO clone = newOrUpdatedShift.CopyAsNew();
                        clone.StartTime = start;
                        clone.StopTime = end;
                        dtos.Add(clone);

                        start = end;
                    }
                }

                if (start < end)
                {
                    // Create shift after last break
                    TimeSchedulePlanningDayDTO clone = newOrUpdatedShift.CopyAsNew();
                    clone.StartTime = start;
                    clone.StopTime = end;
                    dtos.Add(clone);
                }

                if (dtos.Count > 0)
                    dtos.First().TimeScheduleTemplateBlockId = newOrUpdatedShift.TimeScheduleTemplateBlockId;
            }
            else
            {
                dtos.Add(newOrUpdatedShift);
            }

            return dtos;
        }

        /// <summary>
        /// copied from EditShiftGridViewModel
        /// </summary>
        /// <param name="shifts"></param>
        /// <param name="adjustShifts"></param>
        /// <returns></returns>
        private bool HasHolesWithBreaksInside(List<MobileSaveShift> shifts, bool adjustShifts)
        {
            bool hasHole = false;
            bool hasBreakInsideHole = false;

            MobileSaveShift prevShift = null;
            foreach (MobileSaveShift shift in shifts.Where(s => !s.IsBreak).OrderBy(s => s.StartTime).ThenBy(s => s.StopTime).ToList())
            {
                // Check for holes
                if (prevShift != null && prevShift.StopTime < shift.StartTime)
                {
                    hasHole = true;
                    // A hole found, check if a break is within the hole
                    if (shifts.Any(s => s.IsBreak && (s.StartTime >= prevShift.StopTime && s.StartTime < shift.StartTime) || (s.StopTime > prevShift.StopTime && s.StopTime <= shift.StartTime)))
                    {
                        hasBreakInsideHole = true;
                        if (adjustShifts)
                        {
                            prevShift.StopTime = shift.StartTime;
                        }
                    }
                }
                prevShift = shift;
            }

            return hasHole && hasBreakInsideHole;
        }

        #region StateAnalysis

        private void GenerateGroupSupplierLedgerHtml(ref StringBuilder htmlData, List<StateAnalysisDTO> groupSupplierLedger)
        {
            #region Header row1

            GenerateGroupHeaderRow(ref htmlData, GetText(4, (int)TermGroup.StateAnalysis, "Leverantörsreskontra"));

            #endregion

            #region Header row2

            string header2 = GetText(17, (int)TermGroup.StateAnalysis, "Antal");
            string header3 = GetText(21, (int)TermGroup.StateAnalysis, "Summa");

            GenerateGroupHeaderRow2(ref htmlData, header2, header3, "", "");

            #endregion

            #region Data Rows

            #region Row 1

            if (groupSupplierLedger.Any(x => x.State == SoeStatesAnalysis.SupplierInvoicesUnpayed))
            {
                var dto = groupSupplierLedger.FirstOrDefault(x => x.State == SoeStatesAnalysis.SupplierInvoicesUnpayed);
                htmlData.Append("<tr>");

                //col 1
                htmlData.Append("<td>" + GetText(15, (int)TermGroup.StateAnalysis, "Obetalda") + "</td>");

                //col 2                
                htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");

                //col 3
                htmlData.Append("<td>" + dto.TotalAmount.ToString("N2") + "</td>");

                //col 4               
                htmlData.Append("<td></td>");

                //col 5
                htmlData.Append("<td></td>");

                htmlData.Append("</tr>");
            }

            #endregion

            #region Row 2

            if (groupSupplierLedger.Any(x => x.State == SoeStatesAnalysis.SupplierInvoicesOverdued))
            {
                var dto = groupSupplierLedger.FirstOrDefault(x => x.State == SoeStatesAnalysis.SupplierInvoicesOverdued);
                htmlData.Append("<tr>");

                //col 1
                htmlData.Append("<td>" + GetText(16, (int)TermGroup.StateAnalysis, "Förfallna") + "</td>");

                //col 2                
                htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");

                //col 3
                htmlData.Append("<td>" + dto.TotalAmount.ToString("N2") + "</td>");

                //col 4               
                htmlData.Append("<td></td>");

                //col 5
                htmlData.Append("<td></td>");

                htmlData.Append("</tr>");
            }

            #endregion

            #endregion
        }

        private void GenerateGroupCustomerLedgerHtml(ref StringBuilder htmlData, List<StateAnalysisDTO> groupCustomerLedger)
        {
            #region Header row1

            GenerateGroupHeaderRow(ref htmlData, GetText(3, (int)TermGroup.StateAnalysis, "Kundreskontra"));

            #endregion

            #region Header row2

            string header2 = GetText(17, (int)TermGroup.StateAnalysis, "Antal");
            string header3 = GetText(21, (int)TermGroup.StateAnalysis, "Summa");

            GenerateGroupHeaderRow2(ref htmlData, header2, header3, "", "");

            #endregion

            #region Data Rows

            #region Row 1

            if (groupCustomerLedger.Any(x => x.State == SoeStatesAnalysis.CustomerPaymentsUnpayed))
            {
                var dto = groupCustomerLedger.FirstOrDefault(x => x.State == SoeStatesAnalysis.CustomerPaymentsUnpayed);
                htmlData.Append("<tr>");

                //col 1
                htmlData.Append("<td>" + GetText(15, (int)TermGroup.StateAnalysis, "Obetalda") + "</td>");

                //col 2                
                htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");

                //col 3
                htmlData.Append("<td>" + dto.TotalAmount.ToString("N2") + "</td>");

                //col 4               
                htmlData.Append("<td></td>");

                //col 5
                htmlData.Append("<td></td>");

                htmlData.Append("</tr>");
            }

            #endregion

            #region Row 2

            if (groupCustomerLedger.Any(x => x.State == SoeStatesAnalysis.CustomerInvoicesOverdued))
            {
                var dto = groupCustomerLedger.FirstOrDefault(x => x.State == SoeStatesAnalysis.CustomerInvoicesOverdued);
                htmlData.Append("<tr>");

                //col 1
                htmlData.Append("<td>" + GetText(16, (int)TermGroup.StateAnalysis, "Förfallna") + "</td>");

                //col 2                
                htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");

                //col 3
                htmlData.Append("<td>" + dto.TotalAmount.ToString("N2") + "</td>");

                //col 4               
                htmlData.Append("<td></td>");

                //col 5
                htmlData.Append("<td></td>");

                htmlData.Append("</tr>");
            }

            #endregion

            #endregion
        }

        private void GenerateGroupHouseHoldHtml(ref StringBuilder htmlData, List<StateAnalysisDTO> groupHouseHold)
        {
            #region Header row1

            GenerateGroupHeaderRow(ref htmlData, GetText(43, (int)TermGroup.StateAnalysis, "ROT-AVDRAG"));

            #endregion

            #region Header row2

            string header2 = GetText(39, (int)TermGroup.StateAnalysis, "Ansök");
            string header3 = GetText(40, (int)TermGroup.StateAnalysis, "Ansökta");
            string header4 = GetText(41, (int)TermGroup.StateAnalysis, "Mottagna");
            string header5 = GetText(42, (int)TermGroup.StateAnalysis, "Avslagna");

            GenerateGroupHeaderRow2(ref htmlData, header2, header3, header4, header5);

            #endregion


            #region Data Rows

            #region Row

            if (groupHouseHold.Any())
            {
                htmlData.Append("<tr>");

                //col 1                
                htmlData.Append("<td>" + GetText(44, (int)TermGroup.StateAnalysis, "AVDRAG") + "</td>");

                //col 2                
                var dto = groupHouseHold.FirstOrDefault(x => x.State == SoeStatesAnalysis.HouseHoldTaxDeductionApply);
                if (dto != null)
                    htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");
                else
                    htmlData.Append("<td></td>");

                //col 3
                dto = groupHouseHold.FirstOrDefault(x => x.State == SoeStatesAnalysis.HouseHoldTaxDeductionApplied);
                if (dto != null)
                    htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");
                else
                    htmlData.Append("<td></td>");

                //col 4
                dto = groupHouseHold.FirstOrDefault(x => x.State == SoeStatesAnalysis.HouseHoldTaxDeductionReceived);
                if (dto != null)
                    htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");
                else
                    htmlData.Append("<td></td>");

                //col 5
                dto = groupHouseHold.FirstOrDefault(x => x.State == SoeStatesAnalysis.HouseHoldTaxDeductionDenied);
                if (dto != null)
                    htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");
                else
                    htmlData.Append("<td></td>");

                htmlData.Append("</tr>");
            }

            #endregion

            #endregion
        }

        private void GenerateGroupBillingHtml(ref StringBuilder htmlData, List<StateAnalysisDTO> groupBillingAndLedger, bool showSalesPrice)
        {
            #region Header row1

            GenerateGroupHeaderRow(ref htmlData, GetText(2, (int)TermGroup.StateAnalysis, "Fakturering"));

            #endregion

            #region Header row2

            string header2 = GetText(17, (int)TermGroup.StateAnalysis, "Antal");
            string header3 = GetText(18, (int)TermGroup.StateAnalysis, "Summa ex. moms");
            string header4 = GetText(19, (int)TermGroup.StateAnalysis, "Snitt");
            string header5 = GetText(20, (int)TermGroup.StateAnalysis, "Antal kunder");

            GenerateGroupHeaderRow2(ref htmlData, header2, header3, header4, header5);

            #endregion

            #region Data Rows

            #region Row Offer

            if (groupBillingAndLedger.Any(x => x.State == SoeStatesAnalysis.Offer))
            {
                var dto = groupBillingAndLedger.FirstOrDefault(x => x.State == SoeStatesAnalysis.Offer);
                htmlData.Append("<tr>");

                //col 1
                htmlData.Append("<td>" + GetText(11, (int)TermGroup.StateAnalysis, "Offerter") + "</td>");

                //col 2                
                htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");

                //col 3
                if (showSalesPrice)
                    htmlData.Append("<td>" + dto.TotalAmount.ToString("N2") + "</td>");
                else
                    htmlData.Append("<td></td>");

                //col 4
                if (showSalesPrice)
                {
                    decimal average = dto.NoOfItems > 0 ? Math.Round((dto.TotalAmount / dto.NoOfItems), 2) : 0;
                    htmlData.Append("<td>" + average.ToString("N2") + "</td>");
                }
                else
                    htmlData.Append("<td></td>");

                //col 5
                htmlData.Append("<td>" + dto.NoOfActorsForItems.ToString() + "</td>");

                htmlData.Append("</tr>");
            }

            #endregion

            #region Row Contract

            if (groupBillingAndLedger.Any(x => x.State == SoeStatesAnalysis.Contract))
            {
                var dto = groupBillingAndLedger.FirstOrDefault(x => x.State == SoeStatesAnalysis.Contract);
                htmlData.Append("<tr>");

                //col 1
                htmlData.Append("<td>" + GetText(12, (int)TermGroup.StateAnalysis, "Avtal") + "</td>");

                //col 2                
                htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");

                //col 3
                if (showSalesPrice)
                    htmlData.Append("<td>" + dto.TotalAmount.ToString("N2") + "</td>");
                else
                    htmlData.Append("<td></td>");

                //col 4
                if (showSalesPrice)
                {
                    decimal average = dto.NoOfItems > 0 ? Math.Round((dto.TotalAmount / dto.NoOfItems), 2) : 0;
                    htmlData.Append("<td>" + average.ToString("N2") + "</td>");
                }
                else
                    htmlData.Append("<td></td>");

                //col 5
                htmlData.Append("<td>" + dto.NoOfActorsForItems.ToString() + "</td>");

                htmlData.Append("</tr>");
            }

            #endregion

            #region Row Order

            if (groupBillingAndLedger.Any(x => x.State == SoeStatesAnalysis.Order))
            {
                var dto = groupBillingAndLedger.FirstOrDefault(x => x.State == SoeStatesAnalysis.Order);
                htmlData.Append("<tr>");

                //col 1
                htmlData.Append("<td>" + GetText(13, (int)TermGroup.StateAnalysis, "Ordrar") + "</td>");

                //col 2                
                htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");

                //col 3
                if (showSalesPrice)
                    htmlData.Append("<td>" + dto.TotalAmount.ToString("N2") + "</td>");
                else
                    htmlData.Append("<td></td>");

                //col 4
                if (showSalesPrice)
                {
                    decimal average = dto.NoOfItems > 0 ? Math.Round((dto.TotalAmount / dto.NoOfItems), 2) : 0;
                    htmlData.Append("<td>" + average.ToString("N2") + "</td>");
                }
                else
                    htmlData.Append("<td></td>");

                //col 5
                htmlData.Append("<td>" + dto.NoOfActorsForItems.ToString() + "</td>");

                htmlData.Append("</tr>");
            }

            #endregion

            #region Row Invoice

            if (groupBillingAndLedger.Any(x => x.State == SoeStatesAnalysis.Invoice))
            {
                var dto = groupBillingAndLedger.FirstOrDefault(x => x.State == SoeStatesAnalysis.Invoice);
                htmlData.Append("<tr>");

                //col 1
                htmlData.Append("<td>" + GetText(14, (int)TermGroup.StateAnalysis, "Fakturor") + "</td>");

                //col 2                
                htmlData.Append("<td>" + dto.NoOfItems.ToString() + "</td>");

                //col 3
                if (showSalesPrice)
                    htmlData.Append("<td>" + dto.TotalAmount.ToString("N2") + "</td>");
                else
                    htmlData.Append("<td></td>");

                //col 4
                if (showSalesPrice)
                {
                    decimal average = dto.NoOfItems > 0 ? Math.Round((dto.TotalAmount / dto.NoOfItems), 2) : 0;
                    htmlData.Append("<td>" + average.ToString("N2") + "</td>");
                }
                else
                    htmlData.Append("<td></td>");

                //col 5
                htmlData.Append("<td>" + dto.NoOfActorsForItems.ToString() + "</td>");

                htmlData.Append("</tr>");
            }

            #endregion

            #region Row Order Remaing Amount

            if (groupBillingAndLedger.Any(x => x.State == SoeStatesAnalysis.OrderRemaingAmount))
            {
                var dto = groupBillingAndLedger.FirstOrDefault(x => x.State == SoeStatesAnalysis.OrderRemaingAmount);
                htmlData.Append("<tr>");

                //col 1
                htmlData.Append("<td>" + GetText(46, (int)TermGroup.StateAnalysis, "Kvar att fakturera") + "</td>");

                //col 2                
                htmlData.Append("<td></td>");

                //col 3
                if (showSalesPrice)
                    htmlData.Append("<td>" + dto.TotalAmount.ToString("N2") + "</td>");
                else
                    htmlData.Append("<td></td>");

                //col 4
                htmlData.Append("<td></td>");

                //col 5
                htmlData.Append("<td></td>");

                htmlData.Append("</tr>");
            }

            #endregion
            #endregion
        }

        private void GenerateGroupGeneralHtml(ref StringBuilder htmlData, List<StateAnalysisDTO> groupGenereal)
        {
            #region Header row

            GenerateGroupHeaderRow(ref htmlData, GetText(1, (int)TermGroup.StateAnalysis, "Huvudregister"));

            #endregion

            #region Data Rows

            #region Row 1

            htmlData.Append("<tr>");

            //col 1
            htmlData.Append("<td>" + GetText(5, (int)TermGroup.StateAnalysis, "Antal roller") + "</td>");

            //col 2
            if (groupGenereal.Any(x => x.State == SoeStatesAnalysis.Role))
                htmlData.Append("<td>" + (groupGenereal.FirstOrDefault(x => x.State == SoeStatesAnalysis.Role)?.NoOfItems ?? 0).ToString() + "</td>");
            else
                htmlData.Append("<td></td>");

            //col 3
            htmlData.Append("<td>" + GetText(8, (int)TermGroup.StateAnalysis, "Antal kunder") + "</td>");

            //col 4
            if (groupGenereal.Any(x => x.State == SoeStatesAnalysis.Customer))
                htmlData.Append("<td>" + (groupGenereal.FirstOrDefault(x => x.State == SoeStatesAnalysis.Customer)?.NoOfItems ?? 0).ToString() + "</td>");
            else
                htmlData.Append("<td></td>");

            //col 5
            htmlData.Append("<td></td>");

            htmlData.Append("</tr>");

            #endregion

            #region Row 2

            htmlData.Append("<tr>");

            //col 1
            htmlData.Append("<td>" + GetText(6, (int)TermGroup.StateAnalysis, "Antal Användare") + "</td>");

            //col 2
            if (groupGenereal.Any(x => x.State == SoeStatesAnalysis.User))
                htmlData.Append("<td>" + (groupGenereal.FirstOrDefault(x => x.State == SoeStatesAnalysis.User)?.NoOfItems ?? 0).ToString() + " </td>");
            else
                htmlData.Append("<td></td>");

            //col 3
            htmlData.Append("<td>" + GetText(9, (int)TermGroup.StateAnalysis, "Antal leverantörer") + "</td>");

            //col 4
            if (groupGenereal.Any(x => x.State == SoeStatesAnalysis.Supplier))
                htmlData.Append("<td>" + (groupGenereal.FirstOrDefault(x => x.State == SoeStatesAnalysis.Supplier)?.NoOfItems ?? 0).ToString() + "</td>");
            else
                htmlData.Append("<td></td>");

            //col 5
            htmlData.Append("<td></td>");

            htmlData.Append("</tr>");

            #endregion

            #region Row 3

            htmlData.Append("<tr>");

            //col 1
            htmlData.Append("<td>" + GetText(7, (int)TermGroup.StateAnalysis, "Antal anställda") + "</td>");

            //col 2
            if (groupGenereal.Any(x => x.State == SoeStatesAnalysis.Employee))
                htmlData.Append("<td>" + (groupGenereal.FirstOrDefault(x => x.State == SoeStatesAnalysis.Employee)?.NoOfItems ?? 0).ToString() + "</td>");
            else
                htmlData.Append("<td></td>");

            //col 3
            htmlData.Append("<td>" + GetText(10, (int)TermGroup.StateAnalysis, "Antal artiklar") + "</td>");

            //col 4
            if (groupGenereal.Any(x => x.State == SoeStatesAnalysis.InvoiceProduct))
                htmlData.Append("<td>" + (groupGenereal.FirstOrDefault(x => x.State == SoeStatesAnalysis.InvoiceProduct)?.NoOfItems ?? 0).ToString() + "</td>");
            else
                htmlData.Append("<td></td>");

            //col 5
            htmlData.Append("<td></td>");
            htmlData.Append("</tr>");

            #endregion

            #endregion
        }

        private void GenerateGroupHeaderRow2(ref StringBuilder htmlData, string header2, string header3, string header4, string header5)
        {
            htmlData.Append("<tr class= \"col_head\">");
            htmlData.Append("<td></td>");
            htmlData.Append("<td>" + header2 + "</td>");
            htmlData.Append("<td>" + header3 + "</td>");
            htmlData.Append("<td>" + header4 + "</td>");
            htmlData.Append("<td>" + header5 + "</td>");
            htmlData.Append("</tr>");
        }

        private void GenerateGroupHeaderRow(ref StringBuilder htmlData, string header)
        {
            htmlData.Append("<tr class = \"header\" >");
            htmlData.Append("<td colspan = \"5\">" + header + "</td>");
            //htmlData.Append("<td></td>");
            //htmlData.Append("<td></td>");
            //htmlData.Append("<td></td>");
            //htmlData.Append("<td></td>");
            htmlData.Append("</tr>");
        }

        private List<SoeStatesAnalysis> GetStatesAnalysisGroup(SoeStatesAnalysisGroup group, List<SoeStatesAnalysis> statesToAnalyse)
        {
            List<SoeStatesAnalysis> statesForGroup = new List<SoeStatesAnalysis>();

            foreach (SoeStatesAnalysis state in statesToAnalyse)
            {
                bool valid = false;
                switch (group)
                {
                    case SoeStatesAnalysisGroup.General:

                        valid = state == SoeStatesAnalysis.Role ||
                                state == SoeStatesAnalysis.User ||
                                state == SoeStatesAnalysis.Employee ||
                                state == SoeStatesAnalysis.Customer ||
                                state == SoeStatesAnalysis.Supplier ||
                                state == SoeStatesAnalysis.InActiveTerminals ||
                                state == SoeStatesAnalysis.InvoiceProduct;
                        break;

                    case SoeStatesAnalysisGroup.BillingAndLedger:
                        valid = state == SoeStatesAnalysis.Offer ||
                                state == SoeStatesAnalysis.Contract ||
                                state == SoeStatesAnalysis.Order ||
                                state == SoeStatesAnalysis.Invoice ||
                                state == SoeStatesAnalysis.CustomerInvoicesOpen ||
                                state == SoeStatesAnalysis.CustomerPaymentsUnpayed ||
                                state == SoeStatesAnalysis.CustomerInvoicesOverdued ||
                                state == SoeStatesAnalysis.SupplierInvoicesOpen ||
                                state == SoeStatesAnalysis.SupplierInvoicesUnpayed ||
                                state == SoeStatesAnalysis.SupplierInvoicesOverdued ||
                                state == SoeStatesAnalysis.OrderRemaingAmount;
                        break;

                    case SoeStatesAnalysisGroup.Edi:
                        valid = state == SoeStatesAnalysis.EdiError ||
                                state == SoeStatesAnalysis.EdiOrderError ||
                                state == SoeStatesAnalysis.EdiInvoicError;
                        break;

                    case SoeStatesAnalysisGroup.Scanning:
                        valid = state == SoeStatesAnalysis.ScanningError ||
                                state == SoeStatesAnalysis.ScanningInvoiceError ||
                                state == SoeStatesAnalysis.ScanningUnprocessedArrivals;
                        break;

                    case SoeStatesAnalysisGroup.Communication:
                        valid = state == SoeStatesAnalysis.NewMessages;
                        break;

                    case SoeStatesAnalysisGroup.HouseholdTaxDeduction:
                        valid = state == SoeStatesAnalysis.HouseHoldTaxDeductionApplied ||
                                state == SoeStatesAnalysis.HouseHoldTaxDeductionApply ||
                                state == SoeStatesAnalysis.HouseHoldTaxDeductionDenied ||
                                state == SoeStatesAnalysis.HouseHoldTaxDeductionReceived;
                        break;
                }

                if (valid)
                    statesForGroup.Add(state);
            }

            return statesForGroup;
        }

        #endregion

        private int GetNbrOfInvalidAnswers(List<ChecklistExtendedRowDTO> rowDtos)
        {
            return (from r in rowDtos
                    where !r.IsHeadline &&
                    r.Mandatory &&
                    !HasAnswer(r)
                    select r).Count();
        }

        private bool HasAnswer(ChecklistExtendedRowDTO rowDTO)
        {
            if (rowDTO == null)
                return false;

            bool hasAnswer = false;

            switch (rowDTO.Type)
            {
                case TermGroup_ChecklistRowType.String:
                    hasAnswer = !String.IsNullOrEmpty(rowDTO.Comment);
                    break;
                case TermGroup_ChecklistRowType.YesNo:
                    hasAnswer = rowDTO.BoolData.HasValue;
                    break;
                case TermGroup_ChecklistRowType.Checkbox:
                    hasAnswer = rowDTO.BoolData.HasValue && rowDTO.BoolData.Value; //Must be checked
                    break;
                case TermGroup_ChecklistRowType.MultipleChoice:
                    hasAnswer = rowDTO.IntData.HasValue && rowDTO.IntData > 0;
                    break;
            }

            return hasAnswer;
        }

        private ActionResult ReplaceUserAndSaveAnswerToAttestFlow(MobileParam param, AttestFlow_ReplaceUserReason reason, AttestWorkFlowRowDTO attestRow, bool answer, string comment, int invoiceId)
        {
            ActionResult result = AttestManager.ReplaceAttestWorkFlowUser(reason, attestRow.AttestWorkFlowRowId, String.Empty, attestRow.UserId.Value, param.ActorCompanyId, invoiceId, false, false);
            if (!result.Success)
                return result;

            int newWorkFlowRowId = result.IntegerValue;
            return AttestManager.SaveAttestWorkFlowRowAnswer(newWorkFlowRowId, comment, answer, param.ActorCompanyId);
        }

        private ActionResult SaveAnswerToAttestFlow(MobileParam param, int rowId, bool answer, string comment)
        {
            return AttestManager.SaveAttestWorkFlowRowAnswer(rowId, comment, answer, param.ActorCompanyId);
        }

        private ActionResult UpdateMobileEmployee(int actorCompanyId, int employeeId, string firstName, string lastName, int addressId, string address, string postalCode, string postalAddress, int closestRelativeId, string closestRelativePhone, string closestRelativeName, string closestRelativeRelation, bool? closestRelativeIsSecret, int closestRelativeId2, string closestRelativePhone2, string closestRelativeName2, string closestRelativeRelation2, bool closestRelativeIsSecret2, int mobileId, string mobile, int emailId, string email)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Employee

                        // Get existing employee
                        Employee employee = EmployeeManager.GetEmployeeIgnoreState(entities, actorCompanyId, employeeId, loadContactPerson: true, loadUser: true);
                        if (employee == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(5027, "Anställd hittades inte"));

                        User empUser = employee.User;
                        if (empUser == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8349, "Användare kunde inte hittas"));

                        ContactPerson contactPerson = employee.ContactPerson;

                        #region User

                        //Set updated values                                         
                        empUser.Name = StringUtility.GetName(firstName, lastName, Constants.APPLICATION_NAMESTANDARD);
                        SetModifiedProperties(empUser);

                        #endregion

                        #region ContactPerson
                        //Set updated values
                        contactPerson.FirstName = firstName;
                        contactPerson.LastName = lastName;

                        SetModifiedProperties(contactPerson);

                        #endregion

                        #region Contact

                        Contact contact = ContactManager.GetContactFromActor(entities, contactPerson.ActorContactPersonId);
                        if (contact == null)
                        {
                            #region Add

                            // Get actor
                            Actor actor = ActorManager.GetActor(entities, contactPerson.ActorContactPersonId, false);
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
                                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                result.ErrorMessage = GetText(8358, "Kontaktuppgifter kunde inte skapas");
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

                        if (emailId <= 0)
                        {
                            result = ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.Email, email, transaction);
                            empUser.Email = email;
                        }
                        else
                        {
                            result = ContactManager.UpdateContactECom(entities, emailId, email, transaction);
                            empUser.Email = email;
                        }

                        if (!result.Success)
                        {
                            result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                            result.ErrorMessage = GetText(8359, "Email kunde inte sparas");
                            return result;
                        }

                        #endregion

                        #region PhoneMobile

                        if (mobileId <= 0)
                            result = ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.PhoneMobile, mobile, transaction);
                        else
                        {
                            if (mobile == "")
                                result = ContactManager.DeleteContactECom(entities, mobileId);
                            else
                                result = ContactManager.UpdateContactECom(entities, mobileId, mobile, transaction);
                        }
                            

                        if (!result.Success)
                        {
                            result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                            result.ErrorMessage = GetText(8360, "Mobilnummer kunde inte sparas");
                            return result;
                        }

                        #endregion

                        #region Closest Relative

                        result = SaveClosestRelative(entities, transaction, contact, closestRelativeId, closestRelativePhone, closestRelativeName, closestRelativeRelation, closestRelativeIsSecret);
                        if (!result.Success)
                            return result;

                        result = SaveClosestRelative(entities, transaction, contact, closestRelativeId2, closestRelativePhone2, closestRelativeName2, closestRelativeRelation2, closestRelativeIsSecret2);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Addresses

                        var addresses = ContactManager.GetContactAddresses(entities, contact.ContactId);

                        #region DistributionAddress

                        if (addressId <= 0)
                        {
                            #region Add

                            #region Add ContactAddress

                            //Delivery
                            var contactAddress = new ContactAddress()
                            {
                                SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Distribution,
                                Name = GetText((int)TermGroup_SysContactAddressType.Distribution, (int)TermGroup.SysContactAddressType),

                                //Set references
                                Contact = contact,
                            };
                            SetCreatedProperties(contactAddress);
                            entities.ContactAddress.AddObject(contactAddress);

                            #endregion

                            #region Add ContactAddressRows

                            #region Address
                            result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.Address, address, contactAddress, transaction);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                result.ErrorMessage = GetText(8361, "Adress kunde inte sparas");
                                return result;
                            }

                            #endregion

                            #region PostalCode
                            result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalCode, postalCode, contactAddress, transaction);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                result.ErrorMessage = GetText(8362, "Postnr kunde inte sparas");
                                return result;
                            }

                            #endregion

                            #region PostalAddress
                            result = ContactManager.AddContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalAddress, postalAddress, contactAddress, transaction);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                result.ErrorMessage = GetText(8363, "Postort kunde inte sparas");
                                return result;
                            }
                            #endregion

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            var contactAddress = addresses.FirstOrDefault(i => i.ContactAddressId == addressId);
                            if (contactAddress != null)
                            {
                                #region Update ContactAddress

                                SetModifiedProperties(contactAddress);

                                #endregion

                                #region Update ContactAddressRows

                                //Address                                
                                result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.Address, contactAddress, address, transaction);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                    result.ErrorMessage = GetText(8361, "Adress kunde inte sparas");
                                    return result;
                                }

                                //PostalCode                               
                                result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalCode, contactAddress, postalCode, transaction);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                    result.ErrorMessage = GetText(8362, "Postnr kunde inte sparas");
                                    return result;
                                }

                                //PostalAddress
                                result = ContactManager.UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalAddress, contactAddress, postalAddress, transaction);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                    result.ErrorMessage = GetText(8363, "Postort kunde inte sparas");
                                    return result;
                                }

                                #endregion
                            }

                            #endregion
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
                        result.IntegerValue = employeeId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private ActionResult SaveClosestRelative(CompEntities entities, TransactionScope transaction, Contact contact, int closestRelativeId, string closestRelativePhone, string closestRelativeName, string closestRelativeRelation, bool? closestRelativeIsSecret)
        {
            string description = null;
            if (!String.IsNullOrEmpty(closestRelativeName) || !String.IsNullOrEmpty(closestRelativeRelation))
            {
                closestRelativeName = StringUtility.NullToEmpty(closestRelativeName);
                closestRelativeRelation = StringUtility.NullToEmpty(closestRelativeRelation);
                description = $"{closestRelativeName};{closestRelativeRelation}";
            }

            ActionResult result = null;
            if (closestRelativeId <= 0)
                result = ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.ClosestRelative, closestRelativePhone, transaction, description: description, isSecret: closestRelativeIsSecret);
            else
            {
                if (closestRelativeName.IsNullOrEmpty() && closestRelativeRelation.IsNullOrEmpty() && closestRelativePhone.IsNullOrEmpty())
                    result = ContactManager.DeleteContactECom(entities, closestRelativeId);
                else
                    result = ContactManager.UpdateContactECom(entities, closestRelativeId, closestRelativePhone, transaction, description: description, isSecret: closestRelativeIsSecret);
            }
            if (!result.Success)
            {
                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                result.ErrorMessage = GetText(8364, "Närmast anhörig kunde inte sparas");
            }

            return result;
        }

        private ActionResult CopyProjectTimeBlocksDatesAddDays(MobileParam param, Employee employee, DateTime fromStartDate, DateTime fromToDate, int daysDifference)
        {
            try
            {
                var pm = new ProjectManager(this.parameterObject);

                var extendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, param.ActorCompanyId, 0);
                var dtosToSave = new List<ProjectTimeBlockSaveDTO>();
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(param.ActorCompanyId));

                var projectTimeBlocks = pm.LoadProjectTimeBlockForTimeSheet(fromStartDate, fromToDate, employee.EmployeeId, null, null, null, false, true);
                var projects = new List<ProjectTinyDTO>();
                var errorSB = new StringBuilder();
                foreach (var timeBlock in projectTimeBlocks)
                {
                    // Get project
                    if(!projects.Any(e => e.ProjectId == timeBlock.ProjectId))
                    {
                        projects.Add(pm.GetProjectSmall(param.ActorCompanyId, timeBlock.ProjectId));
                    }

                    var project = projects.FirstOrDefault(e => e.ProjectId == timeBlock.ProjectId);

                    if(project != null && (project.Status == TermGroup_ProjectStatus.Locked || project.Status == TermGroup_ProjectStatus.Finished) || timeBlock.OrderClosed) 
                    {
                        errorSB.AppendLine(timeBlock.Date.ToShortDateString());
                        errorSB.AppendLine(timeBlock.InvoiceNr + " " + timeBlock.CustomerName);
                        errorSB.AppendLine(timeBlock.ProjectName);

                        if(!timeBlock.TimeDeviationCauseName.IsNullOrEmpty())
                        {
                            errorSB.AppendLine(timeBlock.TimeDeviationCauseName);
                        }

                        if (!timeBlock.TimeCodeName.IsNullOrEmpty())
                        {
                            errorSB.AppendLine(timeBlock.TimeCodeName);
                        }

                        errorSB.Append("\n");
                    }
                    else
                    {
                        var projectTimeBlockDto = new ProjectTimeBlockSaveDTO
                        {
                            ActorCompanyId = param.ActorCompanyId,
                            ProjectTimeBlockId = 0,
                            CustomerInvoiceId = timeBlock.CustomerInvoiceId,
                            ProjectId = timeBlock.ProjectId,
                            EmployeeId = timeBlock.EmployeeId,
                            Date = timeBlock.Date.AddDays(daysDifference),
                            From = timeBlock.StartTime,
                            To = timeBlock.StopTime,
                            InvoiceQuantity = timeBlock.InvoiceQuantity,
                            ExternalNote = timeBlock.ExternalNote,
                            InternalNote = timeBlock.InternalNote,
                            TimeCodeId = timeBlock.TimeCodeId,
                            TimeDeviationCauseId = timeBlock.TimeDeviationCauseId,
                            TimePayrollQuantity = timeBlock.TimePayrollQuantity,
                            EmployeeChildId = timeBlock.EmployeeChildId,
                            ProjectInvoiceWeekId = 0,
                            ProjectInvoiceDayId = 0,
                        };

                        if (extendedTimeRegistration)
                        {
                            projectTimeBlockDto.AutoGenTimeAndBreakForProject = employee.GetAutoGenTimeAndBreakForProject(projectTimeBlockDto.Date.Value, employeeGroups).GetValueOrDefault(false);
                        }

                        dtosToSave.Add(projectTimeBlockDto);
                    }
                }

                var result = pm.SaveProjectTimeBlocks(dtosToSave, false);
                if(result.Success)
                {
                    var errorMessage = errorSB.ToString();
                    if(!errorMessage.IsNullOrEmpty())
                    {
                        return new ActionResult(GetText(10270, "Dessa tider sparades inte") + "\n\n" + errorMessage);
                    }

                    return result;
                } 
                else
                {
                    return new ActionResult(Texts.SaveFailed);
                }
            }
            catch (Exception exp)
            {
                LogError("Mobilemanager: CopyProjectTimeBlocksDatesAddDays failed: " + exp.Message);

                if (exp.InnerException != null)
                    LogError("Mobilemanager: CopyProjectTimeBlocksDatesAddDays failed, innerexception: " + exp.InnerException.Message);

                return new ActionResult(Texts.SaveFailed);
            }
        }

        private static void FormatAddresses(List<ContactAddress> addresses)
        {
            foreach (ContactAddress address in addresses)
            {
                FormatAddress(address);
            }
        }

        private static void FormatAddress(ContactAddress address)
        {
            ContactAddressRow addr = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
            ContactAddressRow postalCode = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow postalAddr = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);
            if (address.ContactAddressId != 0)
                address.Address = String.Format("{0}, {1} {2}", addr != null ? addr.Text : String.Empty, postalCode != null ? postalCode.Text : String.Empty, postalAddr != null ? postalAddr.Text : String.Empty);
        }

        private string CreateFileOnServer(MobileParam param, int id, string nameWithExtension, byte[] data, bool isSalarySpecification = false)
        {
            bool useAzurestorage = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_24);
            string currentDirectory = parameterObject?.ExtendedUserParams?.CurrentDirectory;

            string returnPath;
            nameWithExtension = nameWithExtension.Replace("å", "a");
            nameWithExtension = nameWithExtension.Replace("ä", "a");
            nameWithExtension = nameWithExtension.Replace("ö", "o");

            nameWithExtension = StringUtility.ReplaceNonAscii(nameWithExtension, "_");

            if (useAzurestorage)
            {
                BlobUtil blobUtil = new BlobUtil();
                blobUtil.Init(isSalarySpecification ? BlobUtil.CONTAINER_TEMP : BlobUtil.CONTAINER_LONG_TEMP);
                Guid guid = Guid.NewGuid();
                blobUtil.UploadData(guid, data, nameWithExtension, WebUtil.GetContentType(nameWithExtension));
                returnPath = blobUtil.GetDownloadLink(guid.ToString(), nameWithExtension);
                return returnPath;
            }
            else
            {
                string physicalPath = "";
                returnPath = "//Files/";
                FileStream fileStream = null;

                try
                {
                    physicalPath = currentDirectory + "\\Mobile\\Files\\";
                    if (!Directory.Exists(physicalPath))
                        Directory.CreateDirectory(physicalPath);

                    String fileName = id + "G--" + Guid.NewGuid().ToString() + nameWithExtension;
                    fileName = fileName.Replace(" ", "");
                    physicalPath += fileName;
                    returnPath += fileName;

                    fileStream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                    fileStream.Write(data, 0, data.Length);
                }
                catch (Exception e)
                {
                    returnPath = String.Empty;
                    LogError("Create file (" + physicalPath + ") failed: " + e.Message);
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }
                return returnPath;
            }
        }

        //ImageId,Path,Description,Type, Extension
        private Tuple<int, String, String, int, String, String> GetSendImageProperties(MobileParam param, ImagesDTO image, bool dataStorage)
        {
            System.Drawing.Image img = null;
            bool useAzurestorage = Utils.IsCallerExpectedVersionNewerThenGivenVersion(param.Version, Constants.MOBILE_WS_SUPPORTED_OLDVERSION_24);
            String returnPath;
            String extension;
            try
            {

                ImageFormat format = ImageFormat.Jpeg;
                extension = ".jpg";
                switch (image.FormatType)
                {
                    case ImageFormatType.JPG:
                        format = ImageFormat.Jpeg;
                        extension = ".jpg";
                        break;
                    case ImageFormatType.PNG:
                        format = ImageFormat.Png;
                        extension = ".png";
                        break;
                    default:
                        break;
                }

                string nameWithExtension = image.ImageId + "G--" + Guid.NewGuid().ToString() + extension;

                if (useAzurestorage)
                {
                    BlobUtil blobUtil = new BlobUtil();
                    blobUtil.Init(BlobUtil.CONTAINER_LONG_TEMP);
                    Guid guid = Guid.NewGuid();
                    blobUtil.UploadData(guid, image.Image, nameWithExtension, WebUtil.GetContentType(nameWithExtension));
                    returnPath = blobUtil.GetDownloadLink(guid.ToString(), nameWithExtension);
                }
                else
                {
                    img = System.Drawing.Image.FromStream(new MemoryStream(image.Image));
                    returnPath = "//Images/";

                    string phisycalFilePath = parameterObject?.ExtendedUserParams?.CurrentDirectory + "\\Mobile\\Images\\";
                    if (!Directory.Exists(phisycalFilePath))
                        Directory.CreateDirectory(phisycalFilePath);

                    phisycalFilePath += nameWithExtension;
                    returnPath += nameWithExtension;
                    img.Save(phisycalFilePath, format);
                }
            }
            catch (Exception e)
            {
                returnPath = String.Empty;
                extension = String.Empty;

                LogError("Create image path failed: " + e.Message);
            }
            finally
            {
                if (img != null)
                    img.Dispose();
            }

            return Tuple.Create<int, string, string, int, string, String>(image.ImageId, returnPath, image.Description, dataStorage ? (int)image.DataStorageRecordType : (int)image.Type, extension, image.FileName);

        }

        private List<TimeBlockDTO> GetTimeBlocksFromDates(List<DateTime> dates)
        {
            List<TimeBlockDTO> timeBlocks = new List<TimeBlockDTO>();
            if (dates == null || dates.Count == 0)
                return timeBlocks;

            bool newInterval = true;
            DateTime startTime = new DateTime();

            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i].Date;
                if (newInterval)
                {
                    startTime = date.Date;
                    newInterval = false;
                }

                bool last = i == dates.Count - 1;
                bool gap = last || dates[i + 1].Date != date.Date.AddDays(1);
                if (last || gap)
                {
                    timeBlocks.Add(new TimeBlockDTO()
                    {
                        StartTime = startTime,
                        StopTime = date.Date.AddDays(1).AddSeconds(-1),
                    });

                    newInterval = true;
                }
            }

            return timeBlocks;
        }

        private string FormatMessage(string message, int errorNumber)
        {
            if (errorNumber <= 0)
                return message;
            return String.Format("{0}. {1} {2}", message, GetText(5654, "Felkod:"), errorNumber);
        }

        private SoeMobileType GetMobileType(int type)
        {
            switch (type)
            {
                case (int)SoeMobileType.XE:
                    return SoeMobileType.XE;
                case (int)SoeMobileType.Professional:
                    return SoeMobileType.Professional;
                case (int)SoeMobileType.Sauma:
                    return SoeMobileType.Sauma;
                default:
                    return SoeMobileType.Unknown;
            }
        }

        private bool IsXE(int type)
        {
            return GetMobileType(type) == SoeMobileType.XE || GetMobileType(type) == SoeMobileType.Unknown; //For now, approve Unknown as XE
        }

        private bool IsSauma(int type)
        {
            return GetMobileType(type) == SoeMobileType.Sauma;
        }

        private string CreateWorkByPassString(MobileParam param, EvaluateWorkRuleResultDTO rule, int employeeId, int errorMessageNr, string separator = "[#]")
        {
            string returnValue = "";
            if (rule == null)
                return returnValue;

            returnValue += param.ActorCompanyId +
            separator +
            param.UserId +
            separator +
            (rule.EmployeeId.HasValue ? rule.EmployeeId.Value : employeeId) +
            separator +
            rule.EmployeeName +
            separator +
            (int)rule.EvaluatedWorkRule +
            separator +
            (int)rule.Action +
            separator +
            rule.Date +
            separator +
            rule.ErrorNumber +
            separator +
            errorMessageNr;

            return returnValue;
        }

        private EvaluateWorkRulesActionResult CreateWorkRulesResultFromString(string overrideData, string separator = "[#]")
        {
        
            string[] elementSeparator = new string[1];
            elementSeparator[0] = separator;
            string[] separatedElements = overrideData.Trim().Split(elementSeparator, StringSplitOptions.None);

            int employeeId = NumberUtility.ToInteger(separatedElements[2].Trim());
            string employeeName = separatedElements[3].Trim();
            DateTime? date =  CalendarUtility.GetNullableDateTime(separatedElements[6].Trim());
            int errorNumber = NumberUtility.ToInteger(separatedElements[7].Trim());
            string errorName = GetText(NumberUtility.ToInteger(separatedElements[8].Trim()), "");

            var result = new EvaluateWorkRulesActionResult();
            EvaluateWorkRuleResultDTO evaluateWorkRuleResultDTO = new EvaluateWorkRuleResultDTO(errorNumber, errorName, employeeName, date)
            {
                Action = (TermGroup_ShiftHistoryType)NumberUtility.ToInteger(separatedElements[5].Trim()),
                EvaluatedWorkRule = (SoeScheduleWorkRules)NumberUtility.ToInteger(separatedElements[4].Trim()),
                EmployeeId = employeeId,
            };

            result.EvaluatedRuleResults.Add(evaluateWorkRuleResultDTO);
           
            return result;
        }

        private EvacuationListDTO CreateEvacuationListDTO(MobileParam param, int headId, string employeeList, string rowSeparators = "[##]", string separator = "[#]")
        {
            var user = UserManager.GetUser(param.UserId);
            var name = user?.Name ?? string.Empty;
            var result = new EvacuationListDTO(actorCompanyId: param.ActorCompanyId, userId: param.UserId, name: name );

            string[] rowSeparator = new string[1];
            rowSeparator[0] = rowSeparators;
            string[] elementSeparator = new string[1];
            elementSeparator[0] = separator;

            string[] separatedRows = employeeList.Split(rowSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string separatedAnswer in separatedRows)
            {
                string[] separatedElements = separatedAnswer.Trim().Split(elementSeparator, StringSplitOptions.None);
                if (separatedElements.Length == 2)
                {
                    int employeeId = NumberUtility.ToInteger(separatedElements[0].Trim());
                    int marked = NumberUtility.ToInteger(separatedElements[1].Trim());

                    result.EvacuationListRow.Add(new EvacuationListRowDTO(headId: headId, employeeId: employeeId, marked: marked == 1));
                }
            }

            return result;
        }

        #region Test

        public bool GetSuccessValue(XDocument xdoc)
        {
            String value = xdoc.Element(MobileMessages.XML_ELEMENT_SUCCESS) != null ? xdoc.Element(MobileMessages.XML_ELEMENT_SUCCESS).Value : String.Empty;

            if (value == "1")
                return true;
            else
                return false;
        }

        public int GetOrderId(XDocument xdoc)
        {
            string orderId = xdoc.Element(MobileMessages.XML_ELEMENT_ORDERID) != null ? xdoc.Element(MobileMessages.XML_ELEMENT_ORDERID).Value : String.Empty;

            if (String.IsNullOrEmpty(orderId))
                return 0;
            else
                return Int32.Parse(orderId);
        }

        public int GetUserId(XDocument xdoc)
        {
            return XmlUtil.GetElementIntValue(xdoc.Root, MobileMessages.XML_ELEMENT_USERID);
        }

        public int GetRoleId(XDocument xdoc)
        {
            return XmlUtil.GetElementIntValue(xdoc.Root, MobileMessages.XML_ELEMENT_ROLEID);
        }

        public int GetCompanyId(XDocument xdoc)
        {
            return XmlUtil.GetElementIntValue(xdoc.Root, MobileMessages.XML_ELEMENT_COMPANYID);
        }

        public string GetErrorMessage(XDocument xdoc)
        {
            return xdoc.Element(MobileMessages.XML_ELEMENT_ERRORMESSAGE) != null ? xdoc.Element(MobileMessages.XML_ELEMENT_ERRORMESSAGE).Value : String.Empty;
        }

        public string GetSearchProductErrorMessage(XDocument xdoc)
        {
            return XmlUtil.GetChildElementValue(xdoc.Root, MobileMessages.XML_ELEMENT_ERRORMESSAGE);
        }

        public int GetTimeRowRowCount(XDocument xdoc)
        {
            int rowcount = XmlUtil.GetNrOfRootElements(xdoc, MobileTimeRow.ROOTNAME);
            return rowcount;
        }

        public int GetProductsRowCount(XDocument xdoc)
        {
            int rowcount = XmlUtil.GetNrOfRootElements(xdoc, MobileProduct.ROOTNAME);
            return rowcount;
        }

        public int GetSearchProductsMaxFetch()
        {
            return MobileProducts.MAXFETCH;
        }

        #endregion

        #endregion
    }
}

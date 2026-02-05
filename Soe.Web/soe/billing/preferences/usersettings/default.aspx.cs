using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.preferences.usersettings
{
    public partial class _default : PageBase
    {
        #region Variables

        protected SettingManager sm;

        protected bool contractPermission;
        protected bool orderPlanningPermission;
        protected bool orderPlanningUserPermission;
        protected bool calendarViewPermission;
        protected bool dayViewPermission;
        protected bool scheduleViewPermission;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.None;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            sm = new SettingManager(ParameterObject);

            //Mandatory parameters

            //Mode
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            // Permissions
            contractPermission = HasRolePermission(Feature.Billing_Contract_Contracts, Permission.Modify);

            orderPlanningPermission = HasRolePermission(Feature.Billing_Order_Planning, Permission.Modify);
            orderPlanningUserPermission = HasRolePermission(Feature.Billing_Order_PlanningUser, Permission.Modify);

            calendarViewPermission = HasRolePermission(Feature.Billing_Order_Planning_CalendarView, Permission.Modify) || HasRolePermission(Feature.Billing_Order_PlanningUser_CalendarView, Permission.Modify);
            dayViewPermission = HasRolePermission(Feature.Billing_Order_Planning_DayView, Permission.Modify) || HasRolePermission(Feature.Billing_Order_PlanningUser_DayView, Permission.Modify);
            scheduleViewPermission = HasRolePermission(Feature.Billing_Order_Planning_ScheduleView, Permission.Modify) || HasRolePermission(Feature.Billing_Order_PlanningUser_ScheduleView, Permission.Modify);

            DivOrderPlanning.Visible = orderPlanningPermission || orderPlanningUserPermission;

            //Get users
            LoadCompanyUsers();

            LoadStockPlaces();

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            ProductSearchFilterMode.ConnectDataSource(GetGrpText(TermGroup.ProductSearchFilterMode));

            // OrderPlanning views
            Dictionary<int, string> views = new Dictionary<int, string>();
            if (calendarViewPermission)
                views.Add((int)TermGroup_TimeSchedulePlanningViews.Calendar, GetText(6011, 1004));
            if (dayViewPermission)
                views.Add((int)TermGroup_TimeSchedulePlanningViews.Day, GetText(6012, 1004));
            if (scheduleViewPermission)
                views.Add((int)TermGroup_TimeSchedulePlanningViews.Schedule, GetText(6013, 1004));
            OrderPlanningDefaultView.ConnectDataSource(views);

            // OrderPlanning intervals
            SortedDictionary<int, string> intervals = GetGrpTextSorted(TermGroup.TimeSchedulePlanningVisibleDays);
            intervals.Remove((int)TermGroup_TimeSchedulePlanningVisibleDays.Custom);
            intervals.Remove((int)TermGroup_TimeSchedulePlanningVisibleDays.Year);
            OrderPlanningDefaultInterval.ConnectDataSource(intervals);

            // OrderPlanning shift info
            SortedDictionary<int, string> shiftInfos = GetGrpTextSorted(TermGroup.OrderPlanningShiftInfo);
            OrderPlanningShiftInfoTopRight.ConnectDataSource(shiftInfos.Where(s => s.Key != (int)TermGroup_OrderPlanningShiftInfo.DeliveryAddress));
            OrderPlanningShiftInfoBottomLeft.ConnectDataSource(shiftInfos);
            OrderPlanningShiftInfoBottomRight.ConnectDataSource(shiftInfos);

            DefaultOrderType.ConnectDataSource(GetGrpText(TermGroup.OrderType));

            #endregion

            #region Set data

            // Invoice registration - Product rows
            ProductSearchMinPrefixLength.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingProductSearchMinPrefixLength, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            ProductSearchMinPopulateDelay.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingProductSearchMinPopulateDelay, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            ProductSearchFilterMode.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingProductSearchFilterMode, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            DisableWarningPopups.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.BillingDisableWarningPopupWindows, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            ShowWarningBeforeInvoiceRowDeletion.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.BillingShowWarningBeforeDeletingRow, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            HideIncomeRatioAndPercentage.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.BillingOrderIncomeRatioVisibility, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            // Invoice registration - Our reference
            OurReference.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingInvoiceOurReference, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            DefaultOrderType.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingDefaultOrderType, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            UseCashCustomerAsDefault.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.BillingUseOneTimeCustomerAsDefault, UserId, SoeCompany.ActorCompanyId, 0).ToString();


            DefaultStockPlace.Value = sm.GetIntSetting(SettingMainType.UserAndCompany, (int)UserSettingType.BillingDefaultStockPlace, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            // Project
           
            //Order column settings
            /*
            ShowOrderDateColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowOrderDateColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: true).ToString();
            ShowDeliveryDateColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowDeliveryDateColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: true).ToString();
            ShowProjectNumberColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowProjectNumberColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            ShowOrderAmountIncVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowOrderAmountIncVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            ShowOrderAmountExVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowOrderAmountExVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: true).ToString();
            ShowRemainingAmountIncVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowRemainingAmountIncVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            ShowRemainingAmountExVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowRemainingAmountExVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: true).ToString();
            ShowOrderTotalAmountIncVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowOrderTotalAmountIncVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            ShowOrderTotalAmountExVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowOrderTotalAmountExVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            ShowOrderPaymentServiceColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowOrderPaymentServiceColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();

            //Invoice column settings
            ShowOrderNumbersColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowOrderNumbersColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            ShowBillingMethodColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowInvoiceDeliveryTypeColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            ShowInvoiceAmountIncVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowInvoiceAmountIncVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            ShowInvoiceAmountExVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowInvoiceAmountExVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: true).ToString();
            ShowInvoicePaymentServiceColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowInvoicePaymentServiceColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();

            //Contract column settings
            ShowContractAmountIncVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowContractAmountIncVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            ShowContractAmountExVatColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowContractAmountExVatColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: true).ToString();
            ShowContractPaymentServiceColumn.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.ShowContractPaymentServiceColumn, UserId, SoeCompany.ActorCompanyId, 0, defaultValue: false).ToString();
            */

            // OrderPlanning
            OrderPlanningDefaultView.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingOrderPlanningDefaultView, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            int defaultInterval = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingOrderPlanningDefaultInterval, UserId, SoeCompany.ActorCompanyId, 0);
            if (defaultInterval == 0)
                defaultInterval = 7;
            OrderPlanningDefaultInterval.Value = defaultInterval.ToString();

            OrderPlanningShiftInfoTopRight.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingOrderPlanningShiftInfoTopRight, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            OrderPlanningShiftInfoBottomLeft.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingOrderPlanningShiftInfoBottomLeft, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            OrderPlanningShiftInfoBottomRight.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.BillingOrderPlanningShiftInfoBottomRight, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3013, "Inställningar uppdaterade");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3014, "Inställningar kunde inte uppdateras");
            }

            #endregion
        }

        protected override void Save()
        {
            bool success = true;

            #region Bool

            var boolValues = new Dictionary<int, bool>();
            boolValues.Add((int)UserSettingType.BillingDisableWarningPopupWindows, StringUtility.GetBool(F["DisableWarningPopups"]));
            boolValues.Add((int)UserSettingType.BillingShowWarningBeforeDeletingRow, StringUtility.GetBool(F["ShowWarningBeforeInvoiceRowDeletion"]));
            boolValues.Add((int)UserSettingType.BillingOrderIncomeRatioVisibility, StringUtility.GetBool(F["HideIncomeRatioAndPercentage"]));
            boolValues.Add((int)UserSettingType.BillingUseOneTimeCustomerAsDefault, StringUtility.GetBool(F["UseCashCustomerAsDefault"]));

            /*
            boolValues.Add((int)UserSettingType.ShowOrderDateColumn, StringUtility.GetBool(F["ShowOrderDateColumn"]));
            boolValues.Add((int)UserSettingType.ShowDeliveryDateColumn, StringUtility.GetBool(F["ShowDeliveryDateColumn"]));
            boolValues.Add((int)UserSettingType.ShowProjectNumberColumn, StringUtility.GetBool(F["ShowProjectNumberColumn"]));
            boolValues.Add((int)UserSettingType.ShowOrderAmountIncVatColumn, StringUtility.GetBool(F["ShowOrderAmountIncVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowOrderAmountExVatColumn, StringUtility.GetBool(F["ShowOrderAmountExVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowRemainingAmountIncVatColumn, StringUtility.GetBool(F["ShowRemainingAmountIncVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowRemainingAmountExVatColumn, StringUtility.GetBool(F["ShowRemainingAmountExVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowOrderNumbersColumn, StringUtility.GetBool(F["ShowOrderNumbersColumn"]));
            boolValues.Add((int)UserSettingType.ShowInvoiceDeliveryTypeColumn, StringUtility.GetBool(F["ShowBillingMethodColumn"]));
            boolValues.Add((int)UserSettingType.ShowInvoiceAmountIncVatColumn, StringUtility.GetBool(F["ShowInvoiceAmountIncVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowInvoiceAmountExVatColumn, StringUtility.GetBool(F["ShowInvoiceAmountExVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowContractAmountIncVatColumn, StringUtility.GetBool(F["ShowContractAmountIncVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowContractAmountExVatColumn, StringUtility.GetBool(F["ShowContractAmountExVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowOrderTotalAmountIncVatColumn, StringUtility.GetBool(F["ShowOrderTotalAmountIncVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowOrderTotalAmountExVatColumn, StringUtility.GetBool(F["ShowOrderTotalAmountExVatColumn"]));
            boolValues.Add((int)UserSettingType.ShowOrderPaymentServiceColumn, StringUtility.GetBool(F["ShowOrderPaymentServiceColumn"]));
            boolValues.Add((int)UserSettingType.ShowInvoicePaymentServiceColumn, StringUtility.GetBool(F["ShowInvoicePaymentServiceColumn"]));
            boolValues.Add((int)UserSettingType.ShowContractPaymentServiceColumn, StringUtility.GetBool(F["ShowContractPaymentServiceColumn"]));
            */
            if (!sm.UpdateInsertBoolSettings(SettingMainType.User, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Integer

            var intValues = new Dictionary<int, int>();
            intValues.Add((int)UserSettingType.BillingProductSearchMinPrefixLength, StringUtility.GetInt(F["ProductSearchMinPrefixLength"]));
            intValues.Add((int)UserSettingType.BillingProductSearchMinPopulateDelay, StringUtility.GetInt(F["ProductSearchMinPopulateDelay"]));
            intValues.Add((int)UserSettingType.BillingProductSearchFilterMode, StringUtility.GetInt(F["ProductSearchFilterMode"]));
            intValues.Add((int)UserSettingType.BillingInvoiceOurReference, StringUtility.GetInt(F["OurReference"]));
            intValues.Add((int)UserSettingType.BillingOrderPlanningDefaultView, StringUtility.GetInt(F["OrderPlanningDefaultView"], 0));
            intValues.Add((int)UserSettingType.BillingOrderPlanningDefaultInterval, StringUtility.GetInt(F["OrderPlanningDefaultInterval"], 7));
            intValues.Add((int)UserSettingType.BillingOrderPlanningShiftInfoTopRight, StringUtility.GetInt(F["OrderPlanningShiftInfoTopRight"], 0));
            intValues.Add((int)UserSettingType.BillingOrderPlanningShiftInfoBottomLeft, StringUtility.GetInt(F["OrderPlanningShiftInfoBottomLeft"], 0));
            intValues.Add((int)UserSettingType.BillingOrderPlanningShiftInfoBottomRight, StringUtility.GetInt(F["OrderPlanningShiftInfoBottomRight"], 0));

            intValues.Add((int)UserSettingType.BillingDefaultOrderType, StringUtility.GetInt(F["DefaultOrderType"], 0));


            if (!sm.UpdateInsertIntSettings(SettingMainType.User, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            intValues = new Dictionary<int, int>();
            intValues.Add((int)UserSettingType.BillingDefaultStockPlace, StringUtility.GetInt(F["DefaultStockPlace"], 0));

            if (!sm.UpdateInsertIntSettings(SettingMainType.UserAndCompany, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            RedirectToSelf(success ? "UPDATED" : "NOTUPDATED");
        }

        private void LoadCompanyUsers()
        {
            UserManager usm = new UserManager(null);
            Dictionary<int, string> dict = usm.GetUsersByCompanyDict(SoeCompany.ActorCompanyId, RoleId, UserId, true, false, true, false);
            OurReference.ConnectDataSource(dict);
        }

        private void LoadStockPlaces()
        {
            var stockManager = new StockManager(this.ParameterObject);
            var stockPlaces = stockManager.GetStocksDict(SoeCompany.ActorCompanyId, true);
            DefaultStockPlace.ConnectDataSource(stockPlaces);
        }
    }
}

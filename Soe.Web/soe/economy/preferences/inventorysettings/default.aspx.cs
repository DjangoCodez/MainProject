using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.economy.preferences.inventorysettings
{
    public partial class _default : PageBase
    {
        #region Variables

        protected SettingManager sm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_InventorySettings;
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

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate


            #endregion

            #region Set data

            //Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> inventorySettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Inventory, SoeCompany.ActorCompanyId);

            //Combine periodaccounting rows to same voucher
            SeparateVouchersInWriteOffs.Value = sm.GetSettingFromDict(inventorySettingsDict, (int)CompanySettingType.InventorySeparateVouchersInWriteOffs, (int)SettingDataType.Boolean);

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

            var boolValues = new Dictionary<int, bool>();
            boolValues.Add((int)CompanySettingType.InventorySeparateVouchersInWriteOffs, StringUtility.GetBool(F["SeparateVouchersInWriteOffs"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }
    }
}

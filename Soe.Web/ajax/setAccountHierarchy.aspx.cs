using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class setAccountHierarchy : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string hierarchyId = QS["hierarchyId"];
                if (!String.IsNullOrEmpty(hierarchyId))
                {
                    SettingManager sm = new SettingManager(ParameterObject);

                    // Update default AccountYear
                    ActionResult result = sm.UpdateInsertStringSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, hierarchyId, UserId, SoeCompany.ActorCompanyId, 0);

                    // Update Session
                    CurrentAccountHierarchy = hierarchyId;

                    ResponseObject = new { Success = result.Success };
                }
            }
            catch (Exception)
            {
                ResponseObject = new { Success = false };
            }
        }
    }
}
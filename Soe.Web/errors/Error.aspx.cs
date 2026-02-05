using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.errors
{
	public partial class Error : PageBase
	{
		protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            if (SoeUser == null)
                RedirectToHome();

            SysLogManager slm = new SysLogManager(ParameterObject);

            #endregion

            LogEntry.Visible = false;
			if (HasRolePermission(Feature.Manage_Support_Logs_Edit, Permission.Readonly))
			{
                int userId = SoeUser != null ? UserId : 0;
                int actorCompanyId = SoeCompany != null ? SoeCompany.ActorCompanyId : 0;
                SysLog logEntry = slm.GetLastLogEntry(userId, actorCompanyId);
                if (logEntry != null)
                {
                    LinkLogEntry.Value = GetText(1061, "Visa logghändelse");
                    LinkLogEntry.Href = "/soe/manage/support/logs/edit/?log=" + logEntry.SysLogId;
                    LogEntry.Visible = true;
                }
            }
		}
	}
}

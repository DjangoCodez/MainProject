using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.manage.system.admin.tasks
{
    public partial class _default : PageBase
    {
        #region Variables

        private AdminTaskType adminTaskType = AdminTaskType.Unknown;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            string description = "";

            //Mandatory parameters
            int task;
            if (Int32.TryParse(QS["task"], out task))
            {
                switch (task)
                {
                    case (int)AdminTaskType.RestoreTermCache:
                        adminTaskType = AdminTaskType.RestoreTermCache;
                        description = GetText(5769, "Tömmer termcache och laddar om den från databasen");
                        break;
                    case (int)AdminTaskType.RestoreSysCache:
                        adminTaskType = AdminTaskType.RestoreSysCache;
                        description = GetText(5770, "Tömmer SysDbCache så den den laddas om från databasen vid behov");
                        break;
                    case (int)AdminTaskType.RestoreCompCache:
                        adminTaskType = AdminTaskType.RestoreCompCache;
                        description = GetText(5771, "Tömmer CompDbCache så den den laddas om från databasen vid behov");
                        break;
                    case (int)AdminTaskType.ClearTimeRuleCache:
                        adminTaskType = AdminTaskType.ClearTimeRuleCache;
                        description = GetText(5940, "Tömmer termcache och laddar om den från databasen");
                        break;
                }
            }

            if (adminTaskType == AdminTaskType.Unknown)
                throw new SoeQuerystringException("task", this.ToString());

            Form1.Mode = SoeFormMode.ExecuteAdminTask;
            Form1.SetTabHeaderText(1, GetText(5212, "Ladda om syscache"));
            

            #endregion

            #region Actions

            if (Form1.IsPosted)
                ExecuteAdminTask();

            #endregion

            #region Set data

            TaskId.Value = ((int)adminTaskType).ToString();
            Name.Value = adminTaskType.ToString();
            Description.Value = description;

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SUCCEEDED")
                {
                    Form1.MessageSuccess = GetText(5139, "Adminfunktion utförd");

                    var initParams = Session[Constants.SESSION_SYSTEM_ADMINTASK_WCF] as Dictionary<string, string>;
                    if (initParams != null)
                        ExecuteWcfAdminTask(initParams);
                }
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(5140, "Adminfunktion misslyckades");
                else if (MessageFromSelf == "CAUSEDEXCEPTION")
                    Form1.MessageError = GetText(5141, "Adminfunktion orsakade exception");
                else if (MessageFromSelf == "MANDATORYFIELDS_MISSING")
                    Form1.MessageWarning = GetText(5143, "Obligatorisk information saknas");
            }

            #endregion
        }

        protected void ExecuteAdminTask()
        {
            try
            {
                //Reset WCF
                Session[Constants.SESSION_SYSTEM_ADMINTASK_WCF] = null;

                //Log
                SysLogManager.LogInfo<_default>(new SoeGeneralException(String.Format("AdminTask {0}.{1}", (int)adminTaskType, adminTaskType.ToString()), this.ToString()));

                Dictionary<string, string> initParams = null;

                switch (adminTaskType)
                {
                    case AdminTaskType.RestoreTermCache:
                        #region RestoreTermCache

                        // Restore .NET Cache
                        TermCacheManager.Instance.RestoreSysTermCacheTS(Environment.MachineName, "System task");

                        // Restore WCF Cache
                        initParams = new Dictionary<string, string>();
                        initParams.Add("actorCompanyId", SoeCompany.ActorCompanyId.ToString());
                        initParams.Add("userId", UserId.ToString());
                        initParams.Add("roleId", RoleId.ToString());
                        initParams.Add("adminTaskType", ((int)AdminTaskType.RestoreTermCache).ToString());
                        initParams.Add("busyMessage", GetText(5942, "Laddar om termcache"));
                        Session[Constants.SESSION_SYSTEM_ADMINTASK_WCF] = initParams;

                        #endregion
                        break;
                    case AdminTaskType.RestoreSysCache:
                        #region RestoreSysCache

                        SysDbCache.Instance.FlushAll();

                        #endregion
                        break;
                    case AdminTaskType.RestoreCompCache:
                        #region RestoreCompCache

                        CompDbCache.Instance.FlushAll(SoeCompany.ActorCompanyId);

                        #endregion
                        break;
                    case AdminTaskType.ClearTimeRuleCache:
                        #region ClearTimeRuleCache

                        // Restore .NET Cache
                        TimeRuleManager trm = new TimeRuleManager(null);
                        trm.FlushTimeRulesFromCache(SoeCompany.ActorCompanyId);

                        // Restore WCF Cache
                        initParams = new Dictionary<string, string>();
                        initParams.Add("actorCompanyId", SoeCompany.ActorCompanyId.ToString());
                        initParams.Add("userId", UserId.ToString());
                        initParams.Add("roleId", RoleId.ToString());
                        initParams.Add("adminTaskType", ((int)AdminTaskType.ClearTimeRuleCache).ToString());
                        initParams.Add("busyMessage", GetText(5943, "Tömmer tidsregelcache"));
                        Session[Constants.SESSION_SYSTEM_ADMINTASK_WCF] = initParams;

                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                SysLogManager.LogError<_default>(ex);
                RedirectToSelf("CAUSEDEXCEPTION", true);
            }

            RedirectToSelf("SUCCEEDED", true);
        }

        protected void ExecuteWcfAdminTask(Dictionary<string, string> initParams)
        {
            //Do nothing
        }
    }
}

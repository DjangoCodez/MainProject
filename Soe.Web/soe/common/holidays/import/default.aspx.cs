using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.holidays.import
{
    public partial class _default : PageBase
    {
        #region Variables

        //Module specifics
        public bool EnableManage { get; set; }
        public bool EnableTime { get; set; }
        
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Manage_Preferences_Registry_Holidays_Edit:
                        EnableManage = true;                        
                        break;
                    case Feature.Time_Preferences_ScheduleSettings_Holidays_Edit:
                        EnableTime = true;                        
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           //Do nothing
        }
    }
}

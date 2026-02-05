using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Soe.WebServices.External
{
    public class StartupOld
    {
        public void Configuration()
        {
            ConfigurationSetupUtil.Init();
            SysServiceManager ssm = new SysServiceManager(null);
            if (HttpContext.Current?.Server != null)
                ConfigSettings.SetCurrentDirectory(HttpContext.Current.Server.MapPath("~"));
        }

    }
}
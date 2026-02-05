using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Util;
using System;

namespace SoftOne.Soe.Business.Evo.Connectors
{
    public class EvoConnectorBase
    {
        protected EvoConnectorBase()
        {

        }

        protected static string Url
        {
            get
            {
                string uri;

                if (ConfigurationSetupUtil.IsTestBasedOnMachine())
                    uri = ConfigurationSetupUtil.GetEvoUrlByTestPrefix().RemoveTrailingSlash();
                // uri = new Uri("https://localhost:7257/");
                else
                    uri = ConfigurationSetupUtil.GetEvoUrl().RemoveTrailingSlash();

                return uri;
            }
        }

        public static string Token
        {
            get { return ConnectorBase.GetAccessToken(); }
        }
    }
}

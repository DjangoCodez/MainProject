using System;
using System.Collections.Specialized;
using System.Web;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Web.Util;
using System.Web.UI;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web
{
    public class SoeSiteMapProvider : XmlSiteMapProvider
    {
        private SiteMapHandler smsh = new SiteMapHandler();
        private FeatureManager fm = new FeatureManager(((PageBase)HttpContext.Current.Handler).ParameterObject);

        public override void Initialize(string name, NameValueCollection attributes)
        {
            base.Initialize(name, attributes);
            this.SiteMapResolve += new SiteMapResolveEventHandler(SoeSiteMapProvider_SiteMapResolve);

            bool releaseMode = StringUtility.GetBool(WebConfigUtility.GetSoeConfigurationSetting(Constants.SOE_CONFIGURATION_SETTING_RELEASEMODE));
            if (!releaseMode)
                fm.ImportFeaturesFromSiteMap();
        }

        private SiteMapNode SoeSiteMapProvider_SiteMapResolve(object sender, SiteMapResolveEventArgs e)
        {
            if (SiteMap.CurrentNode == null)
                return null;

            PageBase pageBase = null;

            #region Page title

            try
            {
                var client = ((Page)HttpContext.Current.Handler).ClientID;
                pageBase = ((PageBase)HttpContext.Current.Handler);

                SiteMapNode node = null;
                if (pageBase.Feature != Feature.None)
                {
                    if (SiteMap.CurrentNode != null)
                    {
                        pageBase.PageTitle += " - " + pageBase.TextService.GetText(Convert.ToInt32(SiteMap.CurrentNode.Title));
                        node = SiteMap.CurrentNode.ParentNode;
                    }
                    while (node != null)
                    {
                        if (node != node.RootNode)
                            pageBase.PageTitle += "<" + pageBase.TextService.GetText(Convert.ToInt32(node.Title));
                        node = node.ParentNode;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString(); // Prevent compiler warning
            }

            #endregion

            SiteMapNode clonedNode = SiteMap.CurrentNode.Clone(true);
            SiteMapNode tempNode = clonedNode;
            Uri uri = new Uri(e.Context.Request.Url.ToString());
            bool addedNode = false;
            while (tempNode != null)
            {
                #region Node title

                int termId;
                if (!Int32.TryParse(tempNode.Title, out termId))
                    throw new SoeGeneralException("SiteMap title is invalid", this.ToString());
                tempNode.Title = pageBase.TextService.GetText(termId);

                #endregion

                #region Reliance

                NameValueCollection values = GetReliance(tempNode, e.Context);
                if (values != null)
                {
                    string qs = UrlUtil.NameValueCollectionToString(values);
                    if (!addedNode)
                    {
                        smsh.AddNode(tempNode, values);
                        addedNode = true;
                    }
                    tempNode.Url += qs;
                }
                tempNode = tempNode.ParentNode;

                #endregion
            }

            return clonedNode;
        }

        private NameValueCollection GetReliance(SiteMapNode node, HttpContext context)
        {
            //Check to see if the node supports reliance
            var reliantOn = node["reliantOn"];
            if (reliantOn == null)
                return null;

            NameValueCollection values = new NameValueCollection();
            string[] keys = reliantOn.Split(",".ToCharArray());
            foreach (string s in keys)
            {
                string key = s.Trim();
                if (String.IsNullOrEmpty(key))
                    continue;

                string value = "";
                if (context.Request.QueryString[key] != null)
                {
                    //Get QS from URL
                    value = context.Request.QueryString[key];
                }
                else
                {
                    //Get QS from Session
                    value = smsh.GetUrlQS(node.Title, node.Url, key);
                }

                values.Add(key, value);
            }

            if (values.Count == 0)
                return null;
            return values;
        }
    }
}

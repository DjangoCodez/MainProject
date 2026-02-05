using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Util;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web
{
    public partial class BaseMaster : MasterPageBase
    {
        public HtmlGenericControl BodyTag
        {
            get { return this.baseMasterHtmlBody; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Moved these three to Page_PreRender since InitStyleSheets is dependant on the Feature being set
            // and is has not when entering here.
            //InitIcons();
            //InitStyleSheets();
            //InitScripts();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            InitIcons();
            InitStyleSheets();
            InitScripts();

            if (((ClaimsPrincipal)Context.User).HasClaim(SoeClaimType.LegacyLogIn, "true"))
                return;

            if (!PageBase.ShouldLoadOidcClientScripts)
                return;

            int index = 0;

            PageBase.Scripts.Insert(index, "/UserControls/SessionTimeout.js?cs=2021");
            index++;
            PageBase.Scripts.Insert(index, "/cssjs/oidc/oidc-client.min.js?cs=2021");
            index++;
            PageBase.Scripts.Insert(index, "/cssjs/oidc/oidc-client-config.js?cs=2021");
        }

        private void InitIcons()
        {
            //Init for sub-pages
            if (PageBase.Icons == null)
                PageBase.Icons = new List<string>();

            AddIcon("/img/favicon.ico");
        }

        protected void AddIcon(string icon)
        {
            var htmlLink = new HtmlLink();
            htmlLink.Attributes["rel"] = "shortcut icon";
            htmlLink.Attributes["type"] = "image/x-icon";
            htmlLink.Href = icon;
            Page.Header.Controls.Add(htmlLink);
        }

        private void InitStyleSheets()
        {
            //Init for sub-pages
            if (PageBase.StyleSheets == null)
                PageBase.StyleSheets = new List<string>();

            this.AddFonts();

            var prefix = AngularConfig.Prefix;

            AddStyleSheet("/cssjs/style.aspx?v=" + (Request.IsLocal ? DateTime.Now.ToString("HHmmss") : WebConfigUtility.GetSoeConfigurationSetting(Constants.SOE_CONFIGURATION_SETTING_SCRIPTVERSION)));
#if DEBUG
            AddStyleSheet("/angular/node_modules/bootstrap/dist/css/bootstrap.min.css");
#else
            AddStyleSheet(prefix + "Styles/bootstrap.min.css");
#endif

            #region Font Awesome

            //if (PageBase.FontAwesomeSource == FontAwesomeIconSource.FontAwesome_CDN)
            //{
            // Free version
            //AddFontAwesomeLink("https://use.fontawesome.com/releases/v5.15.1/css/all.css", "sha384-vp86vTRFVJgpjF9jiIGPEEqYqlDwgyBgEF109VFjmqGmIY/Y4HV4d3Gp2irVfcrp");

            // Pro version
            //if (PageBase.UseAngularSpa)
            //{
            // Version 6.5.1
            //AddFontAwesomeLink("/fonts/fontawesome/css/all.min.css", "");
            AddFontAwesomeLink("/fonts/fontawesome/css/fontawesome.min.css", "");
            AddFontAwesomeLink("/fonts/fontawesome/css/brands.min.css", "");
            AddFontAwesomeLink("/fonts/fontawesome/css/duotone.min.css", "");
            AddFontAwesomeLink("/fonts/fontawesome/css/light.min.css", "");
            AddFontAwesomeLink("/fonts/fontawesome/css/regular.min.css", "");
            AddFontAwesomeLink("/fonts/fontawesome/css/solid.min.css", "");
            //}
            //else
            //{
            // Version 5.15.1
            //  AddFontAwesomeLink("https://pro.fontawesome.com/releases/v5.15.1/css/all.css", "sha384-9ZfPnbegQSumzaE7mks2IYgHoayLtuto3AS6ieArECeaR8nCfliJVuLh/GaQ1gyM");
            //}
            //}

            #endregion

            if (PageBase.HasAngularHost && !PageBase.UseAngularSpa)
            {
                #region Specific for Angular
#if DEBUG
                AddStyleSheet("/angular/node_modules/angular-ui-grid/ui-grid.min.css");
                AddStyleSheet("/angular/node_modules/jquery-ui-dist/jquery-ui.min.css");
                AddStyleSheet("/angular/node_modules/ui-select/dist/select.min.css");
                AddStyleSheet("/angular/node_modules/nvd3/build/nv.d3.min.css");
                AddStyleSheet("/angular/node_modules/jquery-minicolors/jquery.minicolors.css");
#else
                AddStyleSheet(prefix + "Styles/ui-grid.min.css");                    
                AddStyleSheet(prefix + "Styles/jquery-ui.min.css");
                AddStyleSheet(prefix + "Styles/select.min.css");
                AddStyleSheet(prefix + "Styles/nv.d3.min.css");
                AddStyleSheet(prefix + "Styles/jquery.minicolors.css");
#endif
                AddStyleSheet(prefix + "Core/Views/Styles/core.css");
                AddStyleSheet(prefix + "Core/Views/Styles/common.css");
                AddStyleSheet(prefix + "Core/Views/Styles/tabs.css");
                AddStyleSheet(prefix + "Core/Views/Styles/rightmenu.css");
                AddStyleSheet(prefix + "Core/Views/Styles/uigrid.css");
                AddStyleSheet(prefix + "Core/Views/Styles/aggrid.css");
                AddStyleSheet(prefix + "Core/Views/Styles/pages/dashboard.css");
                AddStyleSheet(prefix + "Core/Views/Styles/pages/timepayrollcalculation.css");
                AddStyleSheet(prefix + "Core/Views/Styles/pages/timeattest.css");
                AddStyleSheet(prefix + "Core/Views/Styles/pages/timetree.css");

                #endregion
            }

            AddStyleSheet("/cssjs/BootstrapFix.css");

            // Add style sheets for specific pages
            if (Page.Request.Path != null)
            {
                string path = Page.Request.Path.ToLower();
                if (PageBase.HasAngularHost)
                {
                    if (path.StartsWith("/soe/default.aspx"))
                    {
                        // Start page
                        AddStyleSheet(AngularConfig.Prefix + "Core/Views/Styles/pages/start.css");
                    }

                    if (path.StartsWith("/soe/time/employee/statistics/default.aspx"))
                    {
                        // Employee Statistics
                        AddStyleSheet(AngularConfig.Prefix + "Core/Views/Styles/pages/employeestatistics.css");
                    }

                    if (path.StartsWith("/soe/time/employee/templates/default.aspx") || path.StartsWith("/soe/time/employee/employees/default.aspx"))
                    {
                        // Employees / Employee Templates
                        AddStyleSheet(AngularConfig.Prefix + "Core/Views/Styles/pages/templatedesigner.css");
                    }

                    if (path.StartsWith("/soe/time/schedule/staffingneeds/default.aspx") ||
                             path.StartsWith("/soe/time/schedule/planning/default.aspx") ||
                             path.StartsWith("/soe/time/schedule/planning/schedule/default.aspx") ||
                             path.StartsWith("/soe/time/schedule/planning/template/default.aspx") ||
                             path.StartsWith("/soe/time/schedule/planning/employeepost/default.aspx") ||
                             path.StartsWith("/soe/time/schedule/planning/scenario/default.aspx") ||
                             path.StartsWith("/soe/time/schedule/planning/standby/default.aspx") ||
                             path.StartsWith("/soe/time/schedule/planning/tasksanddeliveries/default.aspx") ||
                             path.StartsWith("/soe/time/schedule/planning/staffingneeds/default.aspx") ||
                             path.StartsWith("/soe/time/time/attest/default.aspx") ||
                             path.StartsWith("/soe/time/time/attestuser/default.aspx") ||
                             path.StartsWith("/soe/billing/order/planning/default.aspx"))
                    {
                        // Staffing needs / Schedule planning / Order planning
                        AddStyleSheet(AngularConfig.Prefix + "Core/Views/Styles/pages/planning.css");
                    }

                    if (path.StartsWith("/soe/time/time/timecalendar/default.aspx"))
                    {
                        // TimeCalendar
                        AddStyleSheet(AngularConfig.Prefix + "Core/Views/Styles/pages/timecalendar.css");
                    }

                    if (path.StartsWith("/soe/billing/project/timesheetuser/default.aspx"))
                    {
                        // TimeSheet
                        AddStyleSheet(AngularConfig.Prefix + "Core/Views/Styles/pages/timesheet.css");
                    }

                    if (path.StartsWith("/soe/billing/preferences/invoicesettings/pricerules") ||
                             path.StartsWith("/soe/time/preferences/timesettings/timerule") ||
                             path.StartsWith("/soe/time/time/attest"))
                    {
                        // Price rules / Time rules
                        AddStyleSheet(AngularConfig.Prefix + "Core/Views/Styles/pages/formulabuilder.css");
                    }
                }
                else
                {
                    if (path.StartsWith("/soe/default.aspx"))
                    {
                        // Start page
                        AddStyleSheet("/cssjs/StartPage.css");
                    }
                }
            }
        }

        protected void AddStyleSheet(string styleSheet)
        {
            var htmlLink = new HtmlLink();
            htmlLink.Attributes["rel"] = "stylesheet";
            htmlLink.Attributes["type"] = "text/css";
            htmlLink.Attributes["media"] = "screen";
            htmlLink.Href = styleSheet;
            Page.Header.Controls.Add(htmlLink);
        }

        protected void AddFonts()
        {
            AddPreconnectFont("https://fonts.googleapis.com");
            AddPreconnectFont("https://fonts.gstatic.com");
            AddFont("https://fonts.googleapis.com/css2?family=Roboto+Condensed:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;1,100;1,200;1,300;1,400;1,500;1,600;1,700&family=Roboto:ital,wght@0,100;0,300;0,400;0,500;0,700;1,100;1,300;1,400;1,500;1,700&display=swap");

            //AddFont("https://fonts.googleapis.com/css?family=Roboto:light,regular,medium,thin,italic,mediumitalic,bold&display=swap", "sha384-xisixHThWVHhFxGftL4rDkpeBJmEa4wpEo2Mf095w7Dnm4QjDumaruGfVKQzxOGc");
            //AddFont("https://fonts.googleapis.com/css?family=Roboto+Condensed:light,regular,medium,thin,italic,mediumitalic,bold&display=swap", "sha384-zTkwTbkA6KS2decIzuulEn/7gPk2euWrOPsbiqT0B+8i20r941Tn7pyrpIbtqAAg");
        }

        protected void AddPreconnectFont(string href)
        {
            var htmlLink = new HtmlLink();
            htmlLink.Attributes["rel"] = "preconnect";
            htmlLink.Href = href;
            Page.Header.Controls.Add(htmlLink);
        }

        protected void AddFont(string href)
        {
            var htmlLink = new HtmlLink();
            htmlLink.Attributes["rel"] = "stylesheet";
            htmlLink.Href = href;
            Page.Header.Controls.Add(htmlLink);
        }

        //protected void AddFont(string href, string integrity)
        //{
        //    var htmlLink = new HtmlLink();
        //    htmlLink.Attributes["rel"] = "stylesheet";
        //    htmlLink.Href = href;
        //    htmlLink.Attributes["integrity"] = integrity;
        //    htmlLink.Attributes["crossorigin"] = "anonymous";
        //    Page.Header.Controls.Add(htmlLink);
        //}

        protected void AddFontAwesomeLink(string href, string integrity)
        {
            var htmlLink = new HtmlLink();
            htmlLink.Attributes["rel"] = "stylesheet";
            htmlLink.Attributes["integrity"] = integrity;
            htmlLink.Attributes["crossorigin"] = "anonymous";
            htmlLink.Href = href;
            Page.Header.Controls.Add(htmlLink);
        }

        private void InitScripts()
        {
            //Init for sub-pages
            if (PageBase.Scripts == null)
                PageBase.Scripts = new List<string>();

            // To communicate between webforms and angularjs (Top menu and right menu)
            //AddScript("/cssjs/lodash.min.js");
            //AddScript("/cssjs/postal.min.js");

            AddScript("/cssjs/jquery.min.js");
            AddScript("/cssjs/jquery-ui.min.js");

            AddScript("/cssjs/modernizr-2.7.1.js");
            AddScript("/cssjs/DOMAssistant" + (Request.IsLocal ? "Complete" : "Compressed") + "-2.8.1.js");
            AddScript("/cssjs/tablesort.js");
            AddScript("/cssjs/json.js");
            AddScript("/cssjs/scripts.aspx?v=" + (Request.IsLocal ? DateTime.Now.ToString("HHmmss") : WebConfigUtility.GetSoeConfigurationSetting(Constants.SOE_CONFIGURATION_SETTING_SCRIPTVERSION)));
            AddScript("/cssjs/merge/SelectCompanyFilter.js");
            //Loaded locally in web/scripts
            AddScript("/Scripts/angular/bootstrap.min.js");
            //AddScript("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js", "sha384-0mSbJDEHialfmuBBQP6A4Qrprq5OVfW37PRR3j5ELqxss1yVqOtnepnHVP9aJ7xS", "anonymous");
            //if (PageBase.FontAwesomeSource == FontAwesomeIconSource.FontAwesome_Kit)
            //    AddScript("https://kit.fontawesome.com/0cba869439.js", "sha384-SWx+UkE/o/f2PRsJoIDKwO3XkhmHonwu8jv+p8s8zddkFh8tApnlMZbtQ0owRwwT", "anonymous");
            AddScript("/cssjs/navigationinterceptor.js");
        }

        protected void AddScript(string script, string integrity = "", string crossorigin = "")
        {
            var htmlLink = new HtmlGenericControl("script");
            htmlLink.Attributes["type"] = "text/javascript";
            htmlLink.Attributes["src"] = script;
            if (!String.IsNullOrEmpty(integrity))
                htmlLink.Attributes["integrity"] = integrity;
            if (!String.IsNullOrEmpty(crossorigin))
                htmlLink.Attributes["crossorigin"] = crossorigin;
            Page.Header.Controls.Add(htmlLink);
        }

        protected string GetParametersString()
        {
            var str = "{";

            str += "\"licenseId\":" + (PageBase.SoeLicense != null ? PageBase.SoeLicense.LicenseId : 0);
            str += ",\"licenseNr\":\"" + (PageBase.SoeLicense != null ? PageBase.SoeLicense.LicenseNr : "") + "\"";
            str += ",\"actorCompanyId\":" + (PageBase.SoeCompany != null ? PageBase.SoeCompany.ActorCompanyId : 0);
            str += ",\"roleId\":" + (PageBase.RoleId);
            str += ",\"userId\":" + (PageBase.SoeUser != null ? PageBase.UserId : 0);
            str += ",\"loginName\":\"" + (PageBase.SoeUser != null ? PageBase.SoeUser.LoginName : "") + "\"";
            str += ",\"sysCountryId\":" + (PageBase.SoeCompany != null && PageBase.SoeCompany.SysCountryId.HasValue ? PageBase.SoeCompany.SysCountryId : (int)TermGroup_Languages.Swedish);
            str += ",\"isSupportAdmin\":" + (PageBase.IsSupportAdmin.ToString().ToLower());
            str += ",\"isSupportSuperAdmin\":" + (PageBase.IsSupportSuperAdmin.ToString().ToLower());
            str += ",\"supportUserId\":" + (PageBase.SoeSupportUser != null ? PageBase.SoeSupportUser.UserId : 0);

            str += "}";

            var encryptedStr = (new StringEncryption("TestingNewEncryptionToWorkBothIn48AND7")).Encrypt(str);

            return encryptedStr;
        }
    }
}

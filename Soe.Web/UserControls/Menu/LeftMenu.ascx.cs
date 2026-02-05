using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UserControls.Menu
{
    public partial class LeftMenu : ControlBase
    {
        private readonly bool useDuotoneIcons = false;
        private string iconPrefix;

        protected void Page_Load(object sender, EventArgs e)
        {
            BuildMenu();

            PageBase.Scripts.Add("/UserControls/Menu/LeftMenu.js");
        }

        private void BuildMenu()
        {
            var browserIsIE = Request.Browser.Type.ToUpper().Contains("INTERNETEXPLORER");
            iconPrefix = useDuotoneIcons && !browserIsIE ? "fad " : "fal ";

            RenderModules();
            RenderTabs();
        }

        private void RenderModules()
        {
            var modules = new LefteMenuModules();
            modules.AddModule(SoeModule.Billing, Feature.Billing, PageBase.GetText(1829, "Försäljning"), "fa-chart-line");
            modules.AddModule(SoeModule.Economy, Feature.Economy, PageBase.GetText(6, "Ekonomi"), "fa-calculator");
            modules.AddModule(SoeModule.Time, Feature.Time, PageBase.GetText(5002, "Personal"), "fa-user-friends");
            modules.AddModule(SoeModule.ClientManagement, Feature.ClientManagement, PageBase.GetText(6500, "Klienthantering"), "fa-buildings");
            modules.AddModule(SoeModule.Manage, Feature.Manage, PageBase.GetText(7, "Administrera"), "fa-cog");
            modules.Render(DivLeftMenu);
        }

        private void RenderTabs()
        {
            RenderModuleBilling(DivLeftMenu);
            RenderModuleEconomy(DivLeftMenu);
            RenderModuleTime(DivLeftMenu);
            RenderModuleClientManagement(DivLeftMenu);
            RenderModuleManage(DivLeftMenu);
        }

        private void RenderModuleBilling(HtmlGenericControl parent)
        {
            LeftMenuModuleContent moduleContent = new LeftMenuModuleContent(SoeModule.Billing, Page?.Request);
            LeftMenuTab tab;
            LeftMenuTabContent tabContent;

            #region Favorites

            RenderFavorites(moduleContent);

            #endregion

            #region Dashboard

            tab = moduleContent.AddTab(Feature.Billing_Dashboard, PageBase.GetText(3527, "Paneler"), iconPrefix + "fa-tachometer-alt");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Dashboard, PageBase.GetText(5418, "Visa"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Dashboard, PageBase.GetText(3572, "Översiktspanel"), "/soe/billing/?autoLoadOnStart=true");
                }
            }

            #endregion

            #region Contract

            tab = moduleContent.AddTab(Feature.Billing_Contract, PageBase.GetText(3445, "Avtal"), iconPrefix + "fa-file-signature");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Contract, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Contract_Status, PageBase.GetText(3448, "Avtal"), "/soe/billing/contract/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleContracts);
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Contract, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Contract_Groups, PageBase.GetText(7454, "Grupper"), "/soe/billing/contract/groups/");
                    tabContent.AddLink(tabContent, Feature.Common_Categories_Contract, PageBase.GetText(4118, "Kategorier"), "/soe/billing/contract/categories/?type=" + ((int)SoeCategoryType.Contract).ToString());
                }

                #endregion
            }

            #endregion

            #region Offer

            tab = moduleContent.AddTab(Feature.Billing_Offer, PageBase.GetText(5321, "Offert"), iconPrefix + "fa-file-alt");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Offer_Status, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                    tabContent.AddLink(tabContent, Feature.Billing_Offer_Status, PageBase.GetText(5339, "Offerter"), "/soe/billing/offer/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleOffers);

                #endregion
            }

            #endregion

            #region Order

            tab = moduleContent.AddTab(Feature.Billing_Order, PageBase.GetText(5327, "Order"), iconPrefix + "fa-file-check");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Order_Status, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Order_Status, PageBase.GetText(5327, "Order"), "/soe/billing/order/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleOrders);
                    tabContent.AddLink(tabContent, Feature.Billing_Order_HandleBilling, PageBase.GetText(7480, "Periodfakturering"), "/soe/billing/order/HandleBilling/");
                    tabContent.AddLink(tabContent, Feature.Billing_Order_Planning, PageBase.GetText(3780, "Planering"), "/soe/billing/order/planning/");
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Order_Status, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Common_Categories_Order, PageBase.GetText(4118, "Kategorier"), "/soe/billing/order/categories/?type=" + ((int)SoeCategoryType.Order).ToString());
                }

                #endregion
            }

            #endregion

            #region Invoice

            tab = moduleContent.AddTab(Feature.Billing_Invoice, PageBase.GetText(1830, "Faktura"), iconPrefix + "fa-file-invoice");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Invoice, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Invoice_Status, PageBase.GetText(3098, "Betalningar"), "/soe/billing/invoice/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleCustomerPayments);
                    tabContent.AddLink(tabContent, Feature.Billing_Invoice_Status, PageBase.GetText(1809, "Fakturor"), "/soe/billing/invoice/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleCustomerInvoices);
                    tabContent.AddLink(tabContent, Feature.Billing_Invoice_Household, PageBase.GetText(7501, "Skattereduktion"), "/soe/billing/invoice/household/");
                }

                #endregion  
            }

            #endregion

            #region Project

            tab = moduleContent.AddTab(Feature.Billing_Project, PageBase.GetText(3357, "Projekt"), iconPrefix + "fa-briefcase");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Project, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Project_List, PageBase.GetText(3357, "Projekt"), "/soe/billing/project/list/");
                    tabContent.AddLink(tabContent, Feature.Billing_Project_TimeSheetUser, PageBase.GetText(3895, "Tidrapport"), "/soe/billing/project/timesheetuser/");
                    tabContent.AddLink(tabContent, Feature.Billing_Project_Central, PageBase.GetText(3056, "Översikt"), "/soe/billing/project/central/");
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Project, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Common_Categories_Project, PageBase.GetText(4118, "Kategorier"), "/soe/billing/project/categories/?type=" + ((int)SoeCategoryType.Project).ToString());
                }

                #endregion
            }

            #endregion

            #region Products

            tab = moduleContent.AddTab(Feature.Billing_Product, PageBase.GetText(5447, "Artikel"), iconPrefix + "fa-box-alt");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Product, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Product_Products, PageBase.GetText(1860, "Artiklar"), "/soe/billing/product/products/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings_Pricelists, PageBase.GetText(1999, "Prislistor"), "/soe/billing/product/pricelists/");
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Product, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Common_Categories_Product, PageBase.GetText(4118, "Kategorier"), "/soe/billing/product/categories/?type=" + ((int)SoeCategoryType.Product).ToString());
                    tabContent.AddLink(tabContent, Feature.Billing_Product_Products_ExtraFields, PageBase.GetText(7549, "Extrafält"), "/soe/billing/product/extrafields/?entity=" + ((int)SoeEntityType.InvoiceProduct).ToString());
                    tabContent.AddLink(tabContent, Feature.Economy_Intrastat_Administer, PageBase.GetText(7634, "Statistiska varukoder"), "/soe/billing/product/commoditycodes/");
                }

                #endregion
            }

            #endregion

            #region Purchase

            tab = moduleContent.AddTab(Feature.Billing_Purchase, PageBase.GetText(7537, "Inköp"), iconPrefix + "fa-shopping-cart");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Purchase, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Purchase_Purchase_List, PageBase.GetText(7538, "Beställning"), "/soe/billing/purchase/list/");
                    tabContent.AddLink(tabContent, Feature.Billing_Purchase_Delivery_List, PageBase.GetText(7563, "Inleverans"), "/soe/billing/purchase/delivery/");
                    tabContent.AddLink(tabContent, Feature.Billing_Purchase_Products, PageBase.GetText(7587, "Inköpsartiklar"), "/soe/billing/purchase/products/");
                    tabContent.AddLink(tabContent, Feature.Billing_Purchase_Pricelists, PageBase.GetText(7592, "Inköpsprislistor"), "/soe/billing/purchase/pricelists/");
                    tabContent.AddLink(tabContent, Feature.Billing_Price_Optimization, PageBase.GetText(7363, "PrisKompassen"), "/soe/billing/purchase/pricecompass/");
                }

                #endregion
            }

            #endregion

            #region Stock

            tab = moduleContent.AddTab(Feature.Billing_Stock, PageBase.GetText(4601, "Lager"), iconPrefix + "fa-inventory");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Stock, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Stock_Inventory, PageBase.GetText(4642, "Inventering"), "/soe/billing/stock/inventory/");
                    tabContent.AddLink(tabContent, Feature.Billing_Stock_Saldo, PageBase.GetText(4643, "Saldo"), "/soe/billing/stock/saldo/");
                    tabContent.AddLink(tabContent, Feature.Billing_Stock_Purchase, PageBase.GetText(9355, "Beställningsförslag"), "/soe/billing/stock/purchase/");
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Stock, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    //tabContent.AddLink(tabContent, Feature.Billing_Stock_Shelf, PageBase.GetText(9297, "Hyllplats"), "/soe/billing/stock/shelf/");
                    tabContent.AddLink(tabContent, Feature.Billing_Stock_Place, PageBase.GetText(4602, "Lagerplats"), "/soe/billing/stock/edit/");
                }

                #endregion
            }

            #endregion

            #region Customer

            tab = moduleContent.AddTab(Feature.Billing_Customer, PageBase.GetText(1710, "Kund"), iconPrefix + "fa-user-alt");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Customer, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Customer_Customers, PageBase.GetText(48, "Kunder"), "/soe/billing/customer/customers/?company=" + PageBase.SoeCompany.ActorCompanyId);
                    tabContent.AddLink(tabContent, Feature.Billing_Customer_Customers, PageBase.GetText(3056, "Översikt"), "/soe/billing/customer/customercentral/");
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Customer, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Common_ExtraFields_Customer_Edit, PageBase.GetText(7549, "Extrafält"), "/soe/billing/customer/extrafields/?entity=" + ((int)SoeEntityType.Customer).ToString());
                    tabContent.AddLink(tabContent, Feature.Common_Categories_Customer, PageBase.GetText(4118, "Kategorier"), "/soe/billing/customer/categories/?type=" + ((int)SoeCategoryType.Customer).ToString());
                }

                #endregion
            }

            #endregion

            #region Asset Register

            tab = moduleContent.AddTab(Feature.Billing_Asset, PageBase.GetText(1897, "Anläggning"), iconPrefix + "fa-wrench");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Asset_List, PageBase.GetText(1897, "Anläggning"), false);
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Asset_List, PageBase.GetText(1897, "Anläggning"), "/soe/billing/asset/");
                }
            }

            #endregion

            #region Import

            tab = moduleContent.AddTab(Feature.Billing_Import, PageBase.GetText(1803, "Import"), iconPrefix + "fa-download");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Import, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Import_XEConnect, PageBase.GetText(9084, "Connect"), "/soe/billing/import/xeconnect/");
                    tabContent.AddLink(tabContent, Feature.Billing_Import_EDI, PageBase.GetText(5356, "EDI"), "/soe/billing/import/edi/?type=" + (int)TermGroup_EDISourceType.EDI);
                    tabContent.AddLink(tabContent, Feature.Billing_Import_ExcelImport, PageBase.GetText(4258, "Excel"), "/soe/billing/import/excelimport/");
                    tabContent.AddLink(tabContent, Feature.Billing_Import_Pricelist, PageBase.GetText(1999, "Prislistor"), "/soe/billing/import/pricelist/");
                    tabContent.AddLink(tabContent, Feature.Billing_Import_ImportSupplierAgreement, PageBase.GetText(7882, "Rabattbrev/Nettopris"), "/soe/billing/preferences/invoicesettings/supplieragreement/");
                }

                #endregion
            }

            #endregion

            #region Export

            tab = moduleContent.AddTab(Feature.Billing_Export, PageBase.GetText(1800, "Export"), iconPrefix + "fa-upload");
            if (tab != null)
            {
                #region Email

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Export_Email, PageBase.GetText(4127, "E-post"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Export_Email, PageBase.GetText(4126, "Skicka fakturor med e-post"), "/soe/billing/export/email/");
                }

                #endregion
            }

            #endregion

            #region Distribution

            tab = moduleContent.AddTab(Feature.Billing_Distribution, PageBase.GetText(1299, "Rapporter"), iconPrefix + "fa-print");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Distribution, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Distribution_Reports, PageBase.GetText(7293, "Elektroniska utskick"), "/soe/billing/distribution/edistribution/");
                    tabContent.AddLink(tabContent, Feature.Billing_Distribution_Packages, PageBase.GetText(7452, "Paket"), "/soe/billing/distribution/packages/");
                    tabContent.AddLink(tabContent, Feature.Billing_Distribution_Reports, PageBase.GetText(2043, "Rapporter"), "/soe/billing/distribution/reports/");
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Distribution, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Distribution_Templates, PageBase.GetText(7451, "Egna mallar"), "/soe/billing/distribution/templates/");
                    tabContent.AddLink(tabContent, Feature.Billing_Distribution_Groups, PageBase.GetText(7454, "Grupper"), "/soe/billing/distribution/groups/");
                    tabContent.AddLink(tabContent, Feature.Billing_Distribution_Headers, PageBase.GetText(2221, "Rubriker"), "/soe/billing/distribution/headers/");
                    tabContent.AddLink(tabContent, Feature.Billing_Distribution_SysTemplates, PageBase.GetText(1375, "Rapportmallar system"), "/soe/billing/distribution/systemplates/");
                }

                #endregion
            }

            #endregion

            #region Statistics

            tab = moduleContent.AddTab(Feature.Billing_Statistics, PageBase.GetText(3384, "Statistik"), iconPrefix + "fa-analytics");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Statistics, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Statistics_Product, PageBase.GetText(8952, "Artikelstatistik"), "/soe/billing/statistics/product/");
                    tabContent.AddLink(tabContent, Feature.Billing_Statistics, PageBase.GetText(3711, "Försäljningsstatistik"), "/soe/billing/statistics/customer/");
                    tabContent.AddLink(tabContent, Feature.Billing_Statistics_Purchase, PageBase.GetText(8940, "Inköpsstatistik"), "/soe/billing/statistics/purchase/");
                }

                #endregion
            }

            #endregion

            #region Preferences

            tab = moduleContent.AddTab(Feature.Billing_Preferences, PageBase.GetText(14, "Inställningar"), iconPrefix + "fa-cog");
            if (tab != null)
            {
                #region General

                tabContent = moduleContent.AddTabContent(tab, Feature.None, PageBase.GetText(5011, "Generellt"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.None, PageBase.GetText(5414, "Användarinställningar"), "/soe/billing/preferences/usersettings/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_EmailTemplate, PageBase.GetText(4142, "E-postmallar"), "/soe/billing/preferences/emailtemplate/");
                    tabContent.AddLink(tabContent, Feature.Common_Categories, PageBase.GetText(4118, "Kategorier"), "/soe/billing/preferences/categories/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_ProjectSettings, PageBase.GetText(5416, "Projektinställningar"), "/soe/billing/preferences/projectsettings/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_Textblock, PageBase.GetText(9276, "Textblock"), "/soe/billing/preferences/textblock/");
                }

                #endregion

                #region Products

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Preferences, PageBase.GetText(4083, "Artiklar"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_ProductSettings_Products, PageBase.GetText(3266, "Basartiklar"), "/soe/billing/preferences/productsettings/products/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_ProductSettings_Accounts, PageBase.GetText(7450, "Baskonton"), "/soe/billing/preferences/productsettings/accounts/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_ProductSettings, PageBase.GetText(14, "Inställningar"), "/soe/billing/preferences/productsettings/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_ProductSettings_MaterialCode, PageBase.GetText(9024, "Materialkoder"), "/soe/billing/preferences/productsettings/materialcode/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_ProductSettings_ProductUnit, PageBase.GetText(3228, "Produktenheter"), "/soe/billing/preferences/productsettings/productunit/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_ProductSettings_ProductGroup, PageBase.GetText(4236, "Produktgrupper"), "/soe/billing/preferences/productsettings/productgroup/");
                }

                #endregion

                #region Invoice

                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Preferences_InvoiceSettings, PageBase.GetText(1833, "Försäljning"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_PayCondition, PageBase.GetText(3081, "Betalningsvillkor"), "/soe/billing/preferences/paycondition/");
                    //tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings_Pricelists, PageBase.GetText(1888, "Försäljningsprislistor"), "/soe/billing/preferences/invoicesettings/pricelists/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings, PageBase.GetText(14, "Inställningar"), "/soe/billing/preferences/invoicesettings/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings_MarkupDiscount, PageBase.GetText(7136, "Kundrabatt"), "/soe/billing/preferences/invoicesettings/customerdiscount/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_DeliveryType, PageBase.GetText(3234, "Leveranssätt"), "/soe/billing/preferences/deliverytype/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_DeliveryCondition, PageBase.GetText(3231, "Leveransvillkor"), "/soe/billing/preferences/deliverycondition/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings_Templates, PageBase.GetText(7192, "Mallar"), "/soe/billing/preferences/invoicesettings/templates/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings_PriceRules, PageBase.GetText(4176, "Prisformler"), "/soe/billing/preferences/invoicesettings/pricerules/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings_Markup, PageBase.GetText(7002, "Påslagsbrev"), "/soe/billing/preferences/invoicesettings/markup/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings_PriceBasedMarkup, PageBase.GetText(7605, "Prisbaserat påslag"), "/soe/billing/preferences/invoicesettings/pricebasedmarkup/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings_ShiftType, PageBase.GetText(3972, "Uppdragstyper"), "/soe/billing/preferences/invoicesettings/shifttype/");
                }

                #endregion

                #region EDI
                tabContent = moduleContent.AddTabContent(tab, Feature.Billing_Preferences_EDISettings, PageBase.GetText(9183, "Grossister"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_EDISettings, PageBase.GetText(5431, "EDI-inställningar"), "/soe/billing/preferences/edisettings/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_Wholesellers, PageBase.GetText(9183, "Grossister"), "/soe/billing/preferences/wholesellersettings/");
                    tabContent.AddLink(tabContent, Feature.Billing_Preferences_InvoiceSettings_WholeSellerPriceList, PageBase.GetText(4166, "Grossistprislistor"), "/soe/billing/preferences/invoicesettings/wholesellerpricelist/");
                }

                #endregion
            }

            #endregion

            moduleContent.Render(parent);
        }

        private void RenderModuleEconomy(HtmlGenericControl parent)
        {
            LeftMenuModuleContent moduleContent = new LeftMenuModuleContent(SoeModule.Economy, Page?.Request);
            LeftMenuTab tab;
            LeftMenuTabContent tabContent;

            AccountManager am = new AccountManager(PageBase.ParameterObject);

            #region Favorites

            RenderFavorites(moduleContent);

            #endregion

            #region Dashboard

            tab = moduleContent.AddTab(Feature.Economy_Dashboard, PageBase.GetText(3527, "Paneler"), iconPrefix + "fa-tachometer-alt");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Dashboard, PageBase.GetText(5418, "Visa"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Dashboard, PageBase.GetText(3572, "Översiktspanel"), "/soe/economy/?autoLoadOnStart=true");
                }
            }

            #endregion

            #region Accounting

            tab = moduleContent.AddTab(Feature.Economy_Accounting, PageBase.GetText(1797, "Redovisning"), iconPrefix + "fa-balance-scale");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Accounting, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_Budget, PageBase.GetText(7148, "Budget"), "/soe/economy/accounting/budget/");
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_SalesBudget, PageBase.GetText(4786, "Försäljningsbudget"), "/soe/economy/accounting/budget/salesbudget/");
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_LiquidityPlanning, PageBase.GetText(3860, "Likviditetsplanering"), "/soe/economy/accounting/liquidityplanning/");
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_AccountDistributionEntry, PageBase.GetText(3472, "Periodiseringar"), "/soe/economy/accounting/accountdistribution/?type=Period");
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_Vouchers, PageBase.GetText(1742, "Verifikat"), "/soe/economy/accounting/vouchers/");
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_AccountPeriods, PageBase.GetText(2097, "År och perioder"), "/soe/economy/accounting/yearend/");
                }

                #endregion

                #region CompanyGroup

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Accounting_CompanyGroup, PageBase.GetText(7274, "Koncernredovisning"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_CompanyGroup_Companies, PageBase.GetText(7275, "Företag"), "/soe/economy/accounting/companygroup/administration/");
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_CompanyGroup_Transfers, PageBase.GetText(7277, "Överföringar"), "/soe/economy/accounting/companygroup/transfer/");
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_CompanyGroup_TransferDefinitions, PageBase.GetText(7276, "Överföringstabeller"), "/soe/economy/accounting/companygroup/mapping/");
                }

                #endregion

                #region Analys

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Accounting, PageBase.GetText(7447, "Analys"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_Reconciliation, PageBase.GetText(7184, "Avstämning"), "/soe/economy/accounting/reconciliation/");
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_VoucherHistory, PageBase.GetText(1143, "Behandlingshistorik"), "/soe/economy/accounting/voucherhistory/");
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_VoucherSearch, PageBase.GetText(7024, "Kontoanalys"), "/soe/economy/accounting/vouchersearch/");

                    if (PageBase.SoeCompany.SysCountryId.HasValue && (PageBase.SoeCompany.SysCountryId.Value == (int)TermGroup_Country.SE || PageBase.SoeCompany.SysCountryId.Value == (int)TermGroup_Country.DK))  // Only for swedish and danish companies. 
                    {
                        tabContent.AddLink(tabContent, Feature.Economy_Accounting_VatVerification, PageBase.GetText(9248, "Momskontroll"), "/soe/economy/accounting/vatverification/");
                    }
                }

                #endregion


                #region Konteringniv
                List<AccountDimDTO> accountDims = new List<AccountDimDTO>();
                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Accounting_AccountRoles, PageBase.GetText(1064, "Konteringsnivåer"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_AccountRoles, PageBase.GetText(1064, "Konteringsnivåer"), "/soe/economy/accounting/accountroles/");
                    accountDims = am.GetAccountDimsFromCache(new CompEntities(), CacheConfig.Company(PageBase.SoeCompany.ActorCompanyId));
                    foreach (AccountDimDTO accountDim in accountDims.Where(a => !a.IsStandard))
                    {
                        tabContent.AddLink(tabContent, Feature.Economy_Accounting_AccountRoles, accountDim.Name, "/soe/economy/accounting/accounts/?isaccountstd=false&dim=" + accountDim.AccountDimId);
                    }

                }

                #endregion 

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Accounting, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    accountDims = accountDims ?? am.GetAccountDimsFromCache(new CompEntities(), CacheConfig.Company(PageBase.SoeCompany.ActorCompanyId));
                    AccountDimDTO accountDimStd = accountDims.FirstOrDefault(f => f.AccountDimNr == Constants.ACCOUNTDIM_STANDARD);
                    if (accountDimStd != null)
                    {
                        tabContent.AddLink(tabContent, Feature.Economy_Accounting_AccountRoles, accountDimStd.Name, "/soe/economy/accounting/accounts/?isaccountstd=true&dim=" + accountDimStd.AccountDimId);
                    }

                    tabContent.AddLink(tabContent, Feature.Economy_Accounting_VoucherTemplateList, PageBase.GetText(1743, "Verifikatmallar"), "/soe/economy/accounting/vouchertemplates/");
                    tabContent.AddLink(tabContent, Feature.Common_ExtraFields_Account_Edit, PageBase.GetText(7549, "Extrafält"), "/soe/economy/accounting/extrafields/?entity=" + ((int)SoeEntityType.Account).ToString());
                }

                #endregion
            }

            #endregion

            #region Supplier

            tab = moduleContent.AddTab(Feature.Economy_Supplier, PageBase.GetText(1798, "Leverantör"), iconPrefix + "fa-truck");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Supplier, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    if (PageBase.SoeCompany.ActorCompanyId == 7 || PageBase.SoeCompany.ActorCompanyId == 288)
                    {
                        // Under development in Development/Hantverkardemo
                        tabContent.AddLink(tabContent, Feature.Economy_Supplier_Invoice, PageBase.GetText(99, "Inkommande"), "/soe/economy/supplier/supplierinvoicesarrivalhall/");
                    }
                    tabContent.AddLink(tabContent, Feature.Economy_Supplier_Invoice_AttestFlow_Overview, PageBase.GetText(5217, "Attest"), "/soe/economy/supplier/invoice/attest/overview/");
                    tabContent.AddLink(tabContent, Feature.Economy_Supplier_Invoice_Status, PageBase.GetText(3098, "Betalningar"), "/soe/economy/supplier/invoice/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleSupplierPayments);
                    tabContent.AddLink(tabContent, Feature.Economy_Supplier_Invoice_Status, PageBase.GetText(1809, "Fakturor"), "/soe/economy/supplier/invoice/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleSupplierInvoices);
                    tabContent.AddLink(tabContent, Feature.Economy_Supplier_Invoice_Matching, PageBase.GetText(7449, "Utjämning"), "/soe/economy/supplier/invoice/matching/");
                    tabContent.AddLink(tabContent, Feature.Economy_Supplier_Suppliers, PageBase.GetText(3056, "Översikt"), "/soe/economy/supplier/suppliercentral/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleSupplierInvoicesAttestFlow);
                }

                #endregion

                #region Analys

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Supplier_Invoice, PageBase.GetText(7447, "Analys"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Supplier_Invoice_Matches, PageBase.GetText(7112, "Reskontraanalys"), "/soe/economy/supplier/invoice/matches/");
                    tabContent.AddLink(tabContent, Feature.Economy_Supplier_Invoice_AgeDistribution, PageBase.GetText(3844, "Åldersfördelning"), "/soe/economy/supplier/invoice/agedistribution/");
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Supplier, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Supplier_Suppliers_TrackChanges, PageBase.GetText(1143, "Behandlingshistorik"), "/soe/economy/supplier/trackchanges/?type=" + ((int)SoeCategoryType.Supplier).ToString());
                    tabContent.AddLink(tabContent, Feature.Common_ExtraFields_Supplier_Edit, PageBase.GetText(7549, "Extrafält"), "/soe/economy/supplier/extrafields/?entity=" + ((int)SoeEntityType.Supplier).ToString());
                    tabContent.AddLink(tabContent, Feature.Common_Categories_Supplier, PageBase.GetText(4118, "Kategorier"), "/soe/economy/supplier/categories/?type=" + ((int)SoeCategoryType.Supplier).ToString());
                    tabContent.AddLink(tabContent, Feature.Economy_Supplier_Suppliers, PageBase.GetText(49, "Leverantörer"), "/soe/economy/supplier/suppliers/?company=" + PageBase.SoeCompany.ActorCompanyId);
                }

                #endregion
            }

#endregion

            #region Customer

            tab = moduleContent.AddTab(Feature.Economy_Customer, PageBase.GetText(1799, "Kund"), iconPrefix + "fa-user-alt");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Customer, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Customer_Invoice_Status, PageBase.GetText(3098, "Betalningar"), "/soe/economy/customer/invoice/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleCustomerPayments);
                    tabContent.AddLink(tabContent, Feature.Economy_Customer_Invoice_Status, PageBase.GetText(1809, "Fakturor"), "/soe/economy/customer/invoice/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleCustomerInvoices);
                    tabContent.AddLink(tabContent, Feature.Economy_Customer_Invoice_Matching, PageBase.GetText(7449, "Utjämning"), "/soe/economy/customer/invoice/matching/");
                    tabContent.AddLink(tabContent, Feature.Economy_Customer_Customers, PageBase.GetText(3056, "Översikt"), "/soe/economy/customer/customercentral/");
                }

                #endregion

                #region Analys

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Customer_Invoice, PageBase.GetText(7447, "Analys"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Customer_Invoice_Matches, PageBase.GetText(7112, "Reskontraanalys"), "/soe/economy/customer/invoice/matches/");
                    tabContent.AddLink(tabContent, Feature.Economy_Customer_Invoice_AgeDistribution, PageBase.GetText(3844, "Åldersfördelning"), "/soe/economy/customer/invoice/agedistribution/");
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Customer, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Common_ExtraFields_Customer_Edit, PageBase.GetText(7549, "Extrafält"), "/soe/economy/customer/extrafields/?entity=" + ((int)SoeEntityType.Customer).ToString());
                    tabContent.AddLink(tabContent, Feature.Common_Categories_Customer, PageBase.GetText(4118, "Kategorier"), "/soe/economy/customer/categories/?type=" + ((int)SoeCategoryType.Customer).ToString());
                    tabContent.AddLink(tabContent, Feature.Economy_Customer_Customers, PageBase.GetText(48, "Kunder"), "/soe/economy/customer/customers/?company=" + PageBase.SoeCompany.ActorCompanyId);
                }

                #endregion
            }

            #endregion

            #region Inventory

            tab = moduleContent.AddTab(Feature.Economy_Inventory, PageBase.GetText(3476, "Inventarier"), iconPrefix + "fa-boxes");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Inventory, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Inventory_WriteOffs, PageBase.GetText(3480, "Avskrivningar"), $"/soe/economy/inventory/writeoffs/?company={PageBase.SoeCompany.ActorCompanyId}&type=Writeoffs");
                    tabContent.AddLink(tabContent, Feature.Economy_Inventory_Inventories, PageBase.GetText(3477, "Inventarier"), "/soe/economy/inventory/inventories/?company=" + PageBase.SoeCompany.ActorCompanyId);
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Inventory, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Inventory_WriteOffTemplates, PageBase.GetText(3478, "Avskrivningsmallar"), "/soe/economy/inventory/writeofftemplates/?company=" + PageBase.SoeCompany.ActorCompanyId);
                    tabContent.AddLink(tabContent, Feature.Economy_Inventory_WriteOffMethods, PageBase.GetText(3479, "Avskrivningsmetoder"), "/soe/economy/inventory/writeoffmethods/?company=" + PageBase.SoeCompany.ActorCompanyId);
                    tabContent.AddLink(tabContent, Feature.Common_Categories_Inventory, PageBase.GetText(4118, "Kategorier"), "/soe/economy/inventory/categories/?type=" + ((int)SoeCategoryType.Inventory).ToString());
                }

                #endregion
            }

            #endregion

            #region Import

            tab = moduleContent.AddTab(Feature.Economy_Import, PageBase.GetText(1803, "Import"), iconPrefix + "fa-download");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Import, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Import_XEConnect, PageBase.GetText(9084, "Connect"), "/soe/economy/import/xeconnect/");
                    tabContent.AddLink(tabContent, Feature.Economy_Import_ExcelImport, PageBase.GetText(4258, "Excel"), "/soe/economy/import/excelimport/");
                    tabContent.AddLink(tabContent, Feature.Economy_Import_Sie, PageBase.GetText(1134, "SIE"), $"/soe/economy/import/sie/?type={(int)SieImportType.Account_Voucher_AccountBalance}");
                    tabContent.AddLink(
                            tabContent,
                            Feature.Economy_Import_Payments,
                            PageBase.GetText(5462, "Betalningar") + " [BETA]",
                            "/soe/economy/import/spapayments/"
                        );
				}

                #region Payments

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Import_Payments, PageBase.GetText(5462, "Betalningar"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Import_Payments_Supplier, PageBase.GetText(4062, "Leverantörsbetalningar"), $"/soe/economy/import/payments/supplier/?type={(int)ImportPaymentType.SupplierPayment}");
                    tabContent.AddLink(tabContent, Feature.Economy_Import_Payments_Customer, PageBase.GetText(5733, "Kundinbetalningar"), $"/soe/economy/import/payments/customer/?type={(int)ImportPaymentType.CustomerPayment}");
                }

                #endregion

                #region Invoices

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Import_Invoices, PageBase.GetText(1809, "Fakturor"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Import_Invoices_Finvoice, PageBase.GetText(5434, "Finvoice"), "/soe/economy/import/invoices/finvoice/?feature=" + (int)Feature.Economy_Import_Invoices_Finvoice);
                    tabContent.AddLink(tabContent, Feature.Economy_Import_Invoices_Automaster, PageBase.GetText(4679, "Importera Automaster"), "/soe/economy/import/invoices/automaster/?feature=" + (int)Feature.Economy_Import_Invoices_Automaster);
                }

                #endregion

                #region Register

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Import_XEConnect, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Import_XEConnect, PageBase.GetText(7448, "Connect-importer"), "/soe/economy/import/xeconnect/batches/");
                }

                #endregion
            }

            #endregion

            #region Export

            tab = moduleContent.AddTab(Feature.Economy_Export, PageBase.GetText(1800, "Export"), iconPrefix + "fa-upload");
            if (tab != null)
            {

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Export, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Export_Payments, PageBase.GetText(4022, "Exporterade betalningar"), "/soe/economy/export/payments/"); //?type=" + (int)TermGroup_SysPaymentMethod.NoneKundspecifika exporter
                    tabContent.AddLink(tabContent, Feature.Economy_Intrastat_ReportsAndExport, PageBase.GetText(7633, "Intrastat"), "/soe/economy/distribution/intrastatexport/");
                    tabContent.AddLink(tabContent, Feature.Economy_Export_CustomerSpecific, PageBase.GetText(7553, "Kundspecifika exporter"), "/soe/economy/export/customerspecificexports/");
                    tabContent.AddLink(tabContent, Feature.Economy_Export_SalesEU, PageBase.GetText(7296, "Periodisk sammanställning"), "/soe/economy/distribution/saleseu/");
                    tabContent.AddLink(tabContent, Feature.Economy_Export_SAFT, PageBase.GetText(7660, "SAF-T"), "/soe/economy/export/saft/");
                    tabContent.AddLink(
                        tabContent, 
                        Feature.Economy_Export_Sie, 
                        PageBase.GetText(1134, "SIE"),
                        "/soe/economy/export/sie/?type=" + (int)SieExportType.Type1);
                }

                #region Invoices

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Export_Invoices, PageBase.GetText(1809, "Fakturor"));
                if (tabContent != null)
                {
                    // Removed by request - item 34707
                    tabContent.AddLink(tabContent, Feature.Economy_Export_Invoices_SOP, PageBase.GetText(8247, "Exporterade SOP filer"), "/soe/economy/export/invoices/sop/?storageType=" + (int)SoeDataStorageRecordType.SOPCustomerInvoiceExport);
                    tabContent.AddLink(tabContent, Feature.Economy_Export_Invoices_DIRegnskap, PageBase.GetText(9062, "Exporterade DI Regnskap filer"), "/soe/economy/export/invoices/sop/?storageType=" + (int)SoeDataStorageRecordType.DiRegnskapCustomerInvoiceExport);
                    //tabContent.AddLink(tabContent, Feature.Economy_Export_Invoices_UniMicro, PageBase.GetText(9098, "Exporterade Uni Micro filer"), "/soe/economy/export/invoices/sop/?storageType=" + (int)SoeDataStorageRecordType.UniMicroCustomerInvoiceExport);
                    //tabContent.AddLink(tabContent, Feature.Economy_Export_Invoices_DnBNor, PageBase.GetText(9274, "Exporterade Dnb NOR Finans filer"), "/soe/economy/export/invoices/sop/?storageType=" + (int)SoeDataStorageRecordType.DnBNorCustomerInvoiceExport);
                    tabContent.AddLink(tabContent, Feature.Economy_Export_Invoices_PaymentService, PageBase.GetText(3191, "Autogiro"), "/soe/economy/export/invoices/exportcustomerpaymentservices.aspx");
                }

                #endregion

                #region FINNISH_TAX

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Export_Finnish_Tax, PageBase.GetText(4580, "FINSKA SKATT"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Export_Finnish_Tax_VAT_Report, PageBase.GetText(91892, "Periodskattedeklaration"), "/soe/economy/export/finnish_tax/?type=1");
                }

                #endregion

                /*
                #region AltInnExport - Removed by request - item 34999 and temporary back!

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Export_AltInnExport, PageBase.GetText(9109, "Altinn"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Export_AltInnExport, PageBase.GetText(9108, "Momsdeklaration"), "/soe/economy/export/altinn/?type=1");
                }

                #endregion
                */
            }

            #endregion

            #region Distribution

            tab = moduleContent.AddTab(Feature.Economy_Distribution, PageBase.GetText(1299, "Rapporter"), iconPrefix + "fa-print");
            if (tab != null)
            {
                #region Manage

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Distribution, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Distribution_DrillDownReports, PageBase.GetText(2275, "Drillbara rapporter"), "/soe/economy/distribution/drilldown/");
                    tabContent.AddLink(tabContent, Feature.Economy_Distribution_Packages, PageBase.GetText(7452, "Paket"), "/soe/economy/distribution/packages/");
                    tabContent.AddLink(tabContent, Feature.Economy_Distribution, PageBase.GetText(2043, "Rapporter"), "/soe/economy/distribution/reports/");
                }

                #endregion

                #region Registry

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Distribution, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Distribution_Templates, PageBase.GetText(7451, "Egna mallar"), "/soe/economy/distribution/templates/");
                    tabContent.AddLink(tabContent, Feature.Economy_Distribution_Groups, PageBase.GetText(7454, "Grupper"), "/soe/economy/distribution/groups/");
                    tabContent.AddLink(tabContent, Feature.Economy_Distribution_SysTemplates, PageBase.GetText(1375, "Rapportmallar system"), "/soe/economy/distribution/systemplates/");
                    tabContent.AddLink(tabContent, Feature.Economy_Distribution_Headers, PageBase.GetText(2221, "Rubriker"), "/soe/economy/distribution/headers/");
                }

                #endregion
            }

            #endregion

            #region Preferences

            tab = moduleContent.AddTab(Feature.Economy_Preferences, PageBase.GetText(14, "Inställningar"), iconPrefix + "fa-cog");
            if (tab != null)
            {
                #region General

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Preferences, PageBase.GetText(5011, "Generellt"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_PayCondition, PageBase.GetText(3081, "Betalningsvillkor"), "/soe/economy/preferences/paycondition/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_VoucherSettings_VatCodes, PageBase.GetText(3904, "Momskoder"), "/soe/economy/preferences/vouchersettings/vatcodes/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_VoucherSettings_MatchCodes, PageBase.GetText(7117, "Restkoder"), "/soe/economy/preferences/vouchersettings/matchsettings/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_Currency, PageBase.GetText(3208, "Valutor"), "/soe/economy/preferences/currency/");
                }

                #endregion

                #region Voucher

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Preferences_VoucherSettings, PageBase.GetText(1671, "Redovisning"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_VoucherSettings_AccountDistributionAuto, PageBase.GetText(3473, "Automatkontering"), "/soe/economy/preferences/vouchersettings/accountdistributionauto/?type=Auto");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_VoucherSettings_Accounts, PageBase.GetText(7450, "Baskonton"), "/soe/economy/preferences/vouchersettings/accounts/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes, PageBase.GetText(7171, "Bruttovinstkoder"), "/soe/economy/preferences/vouchersettings/grossprofitcodes/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_VoucherSettings_DistributionCodes, PageBase.GetText(7147, "Fördelningskoder"), "/soe/economy/preferences/vouchersettings/distributioncodes/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_VoucherSettings, PageBase.GetText(14, "Inställningar"), "/soe/economy/preferences/vouchersettings/");
                }

                #endregion

                #region SupplierInvoice

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Preferences_SuppInvoiceSettings, PageBase.GetText(3115, "Leverantörsreskontra"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_SuppInvoiceSettings_AttestGroups, PageBase.GetText(9245, "Attestgrupper"), "/soe/economy/preferences/suppinvoicesettings/attestgroups/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_SuppInvoiceSettings_Accounts, PageBase.GetText(7450, "Baskonton"), "/soe/economy/preferences/suppinvoicesettings/accounts/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_SuppInvoiceSettings_PaymentMethods, PageBase.GetText(1790, "Betalningsmetoder"), "/soe/economy/preferences/suppinvoicesettings/paymentmethods/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_SuppInvoiceSettings, PageBase.GetText(14, "Inställningar"), "/soe/economy/preferences/suppinvoicesettings/");
                }

                #endregion

                #region CustomerInvoice

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Preferences_CustInvoiceSettings, PageBase.GetText(3131, "Kundreskontra"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_CustInvoiceSettings_Accounts, PageBase.GetText(7450, "Baskonton"), "/soe/economy/preferences/custinvoicesettings/accounts/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_CustInvoiceSettings, PageBase.GetText(14, "Inställningar"), "/soe/economy/preferences/custinvoicesettings/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_CustInvoiceSettings_PaymentMethods, PageBase.GetText(1845, "Inbetalningsmetoder"), "/soe/economy/preferences/custinvoicesettings/paymentmethods/");
                }

                #endregion

                #region Inventory

                tabContent = moduleContent.AddTabContent(tab, Feature.Economy_Preferences_InventorySettings, PageBase.GetText(3476, "Inventarier"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_InventorySettings_Accounts, PageBase.GetText(7450, "Baskonton"), "/soe/economy/preferences/inventorysettings/accounts/");
                    tabContent.AddLink(tabContent, Feature.Economy_Preferences_InventorySettings, PageBase.GetText(14, "Inställningar"), "/soe/economy/preferences/inventorysettings/");
                }

                #endregion
            }

            #endregion

            moduleContent.Render(parent);
        }

        private void RenderModuleTime(HtmlGenericControl parent)
        {
            LeftMenuModuleContent moduleContent = new LeftMenuModuleContent(SoeModule.Time, Page?.Request);
            LeftMenuTab tab;
            LeftMenuTabContent tabContent;

            #region Favorites

            RenderFavorites(moduleContent);

            #endregion

            #region Dashboard

            tab = moduleContent.AddTab(Feature.Time_Dashboard, PageBase.GetText(3527, "Paneler"), iconPrefix + "fa-tachometer-alt");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Dashboard, PageBase.GetText(5418, "Visa"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Dashboard, PageBase.GetText(3572, "Översiktspanel"), "/soe/time/?autoLoadOnStart=true");
                }
            }

            #endregion

            #region Employee

            tab = moduleContent.AddTab(Feature.Time_Employee, PageBase.GetText(5005, "Anställd"), iconPrefix + "fa-user-tie");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Employee, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Employee, PageBase.GetText(5018, "Anställda"), "/soe/time/employee/employees/");
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Employee, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Employee_Groups, PageBase.GetText(5036, "Tidavtal"), "/soe/time/employee/groups/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_PayrollGroups, PageBase.GetText(9121, "Löneavtal"), "/soe/time/employee/payrollgroups/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_PayrollLevels, PageBase.GetText(8933, "Lönenivåer"), "/soe/time/employee/payrolllevels/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_VacationGroups, PageBase.GetText(2058, "Semesteravtal"), "/soe/time/employee/vacationgroup/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_AnnualLeaveGroups, PageBase.GetText(94100, "Årsledighetsavtal"), "/soe/time/employee/annualleavegroups/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_EmployeeCollectiveAgreements, PageBase.GetText(12156, "Avtalsgrupper"), "/soe/time/employee/collectiveagreements/");
                    tabContent.AddLink(tabContent, Feature.Common_Categories_Employee, PageBase.GetText(5052, "Anställdakategorier"), "/soe/time/employee/categories/?type=" + (int)SoeCategoryType.Employee);
                    tabContent.AddLink(tabContent, Feature.Time_Employee_Positions, PageBase.GetText(3834, "Befattningar"), "/soe/time/employee/positions/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_CardNumbers, PageBase.GetText(3458, "Brickor"), "/soe/time/employee/cardnumbers/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_EndReasons, PageBase.GetText(8567, "Slutorsaker"), "/soe/time/employee/endreasons/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_Vehicles, PageBase.GetText(3219, "Tjänstebilar"), "/soe/time/employee/vehicles/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_FollowUpTypes, PageBase.GetText(8612, "Uppföljningstyper"), "/soe/time/employee/followuptypes/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_EmploymentTypes, PageBase.GetText(10266, "Anställningsformer"), "/soe/time/employee/employmenttypes/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_EmployeeTemplates, PageBase.GetText(12155, "Anställningsmallar"), "/soe/time/employee/templates/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_EventHistory, PageBase.GetText(11968, "Händelsehistorik"), "/soe/time/employee/eventhistory/");
                    tabContent.AddLink(tabContent, Feature.Common_ExtraFields_Employee_Edit, PageBase.GetText(7549, "Extrafält"), "/soe/time/employee/extrafields/?entity=" + ((int)SoeEntityType.Employee).ToString());
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Employee, PageBase.GetText(3692, "Rutiner"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Employee_MassUpdateEmployeeFields, PageBase.GetText(91924, "Massuppdatera anställda"), "/soe/time/employee/massupdateemployeefields/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_PayrollReview, PageBase.GetText(3080, "Uppdatera löner"), "/soe/time/employee/payrollreview/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_VacationDebt, PageBase.GetText(8916, "Semesterskuld"), "/soe/time/employee/vacationdebt/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_Csr_Export, PageBase.GetText(10127, "Skatteavdrag"), "/soe/time/employee/csr/export_angular/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_Accumulators, PageBase.GetText(10259, "Saldon"), "/soe/time/employee/accumulators/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_AnnualLeaveBalance, PageBase.GetText(94103, "Uppdatera årsledighetssaldo"), "/soe/time/employee/annualleavebalance/");
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Employee, PageBase.GetText(455, "HR uppföljning"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Employee_Skills, PageBase.GetText(7085, "Kompetens GAP"), "/soe/time/employee/employeeskills/");
                    tabContent.AddLink(tabContent, Feature.Time_Employee_Statistics, PageBase.GetText(3384, "Statistik"), "/soe/time/employee/statistics/");
                }
            }

            #endregion

            #region Schedule

            tab = moduleContent.AddTab(Feature.Time_Schedule, PageBase.GetText(5690, "Planering"), iconPrefix + "fa-calendar-alt");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Schedule, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    if (PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_DayView, Permission.Readonly) || PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ScheduleView, Permission.Readonly) || PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_CalendarView, Permission.Readonly))
                        tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning, PageBase.GetText(4440, "Aktivt schema"), "/soe/time/schedule/planning/schedule/");
                    if ((PageBase.IsSupportLoggedIn || PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_Beta, Permission.Readonly)) && (PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_DayView, Permission.Readonly) || PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ScheduleView, Permission.Readonly)))
                        tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning, PageBase.GetText(4440, "Aktivt schema") + " [BETA]", "/soe/time/schedule/planning/spaschedule/");
                    if (PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_TemplateDayView, Permission.Readonly) || PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_TemplateScheduleView, Permission.Readonly))
                        tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning, PageBase.GetText(4441, "Grundschema"), "/soe/time/schedule/planning/template/");
                    if (PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_EmployeePostDayView, Permission.Readonly) || PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_EmployeePostScheduleView, Permission.Readonly))
                        tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning, PageBase.GetText(4442, "Tjänster"), "/soe/time/schedule/planning/employeepost/");
                    if (PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ScenarioDayView, Permission.Readonly) || PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ScenarioScheduleView, Permission.Readonly))
                        tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning, PageBase.GetText(4443, "Scenario"), "/soe/time/schedule/planning/scenario/");
                    if (PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_StandbyDayView, Permission.Readonly) || PageBase.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_StandbyScheduleView, Permission.Readonly))
                        tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning, PageBase.GetText(11984, "Beredskap"), "/soe/time/schedule/planning/standby/");
                    if (PageBase.HasRolePermission(Feature.Time_Schedule_StaffingNeeds_Tasks, Permission.Readonly) || PageBase.HasRolePermission(Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries, Permission.Readonly))
                        tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning, PageBase.GetText(12150, "Arb. uppgifter och leveranser"), "/soe/time/schedule/planning/tasksanddeliveries/");
                    if (PageBase.HasRolePermission(Feature.Time_Schedule_StaffingNeeds, Permission.Readonly))
                        tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning, PageBase.GetText(91940, "Behovsplanering"), "/soe/time/schedule/planning/staffingneeds/");

                    tabContent.AddLink(tabContent, Feature.Time_Schedule_Placement, PageBase.GetText(5123, "Aktivera schema"), "/soe/time/schedule/placement/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_AbsenceRequests, PageBase.GetText(5459, "Godkänn ledighet"), "/soe/time/schedule/absencerequests/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_AbsenceRequestsUser, PageBase.GetText(8216, "Mina ledighetsansökningar"), "/soe/time/schedule/absencerequestsuser/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_Templates, PageBase.GetText(3294, "Schemamallar"), "/soe/time/schedule/templates/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Schedule, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_TemplateGroups, PageBase.GetText(12051, "Schemagrupper"), "/soe/time/schedule/templategroups/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_StaffingNeeds_EmployeePost, PageBase.GetText(4687, "Tjänster"), "/soe/time/schedule/employeepost/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_StaffingNeeds_Tasks, PageBase.GetText(10105, "Arbetsuppgifter"), "/soe/time/schedule/staffingneedstask/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries, PageBase.GetText(10106, "Leveranser"), "/soe/time/schedule/staffingneedsdelivery/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_TimeBreakTemplate, PageBase.GetText(8750, "Rastmallar"), "/soe/time/schedule/timebreaktemplate/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_StaffingNeeds_ScheduleCycleRuleType, PageBase.GetText(8761, "Schemacykelregler"), "/soe/time/schedule/schedulecycleruletype/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_StaffingNeeds_ScheduleCycle, PageBase.GetText(8762, "Schemacykler"), "/soe/time/schedule/schedulecycle/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning_SalesCalender, PageBase.GetText(4690, "Händelser"), "/soe/time/schedule/timescheduleevents/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Schedule, PageBase.GetText(5418, "Visa"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_Availability, PageBase.GetText(9046, "Tillgänglighet"), "/soe/time/schedule/availability/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_SchedulePlanning_LoggedWarnings, PageBase.GetText(4691, "Loggade varningar"), "/soe/time/schedule/loggedwarnings/");
                }
            }

            #endregion

            #region Time

            tab = moduleContent.AddTab(Feature.Time_Time, PageBase.GetText(5689, "Tid"), iconPrefix + "fa-clock");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Time, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Time_Attest, PageBase.GetText(8040, "Attestera tid"), "/soe/time/time/attest/");
                    tabContent.AddLink(tabContent, Feature.Time_Time_TimeCalendar, PageBase.GetText(3302, "Kalendarium"), "/soe/time/time/timecalendar/");
                    tabContent.AddLink(tabContent, Feature.Time_Time_AttestUser, PageBase.GetText(5003, "Min tid"), "/soe/time/time/attestuser/");
                    tabContent.AddLink(tabContent, Feature.Time_Time_TimeSalarySpecification, PageBase.GetText(5596, "Lönespecifikationer"), "/soe/time/time/salary/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Time, PageBase.GetText(3692, "Rutiner"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Time_Attest_AdjustTimeStamps, PageBase.GetText(3794, "Justera stämplingar"), "/soe/time/time/attest/adjusttimestamps/");
                    tabContent.AddLink(tabContent, Feature.Time_Time_EarnedHoliday, PageBase.GetText(8738, "Intjänade röda dagar"), "/soe/time/time/earnedholiday/");
                    tabContent.AddLink(tabContent, Feature.Time_Time_TimeWorkReduction, PageBase.GetText(8956, "Arbetstidsförkortning"), "/soe/time/time/timeworkreduction/");
                }
            }

            #endregion

            #region Payroll

            tab = moduleContent.AddTab(Feature.Time_Payroll, PageBase.GetText(5951, "Lön"), iconPrefix + "fa-money-bill-alt");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Payroll, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Payroll_Calculation, PageBase.GetText(5950, "Löneberäkning"), "/soe/time/payroll/calculation/");
                    tabContent.AddLink(tabContent, Feature.Time_Payroll_Retroactive, PageBase.GetText(11554, "Retroaktiv lön"), "/soe/time/payroll/retroactive/");
                    tabContent.AddLink(tabContent, Feature.Time_Payroll_Payment, PageBase.GetText(8597, "Utbetalning"), "/soe/time/payroll/payment/selection/");
                    tabContent.AddLink(tabContent, Feature.Time_Payroll_MassRegistration, PageBase.GetText(8611, "Massregistrering"), "/soe/time/payroll/massregistration/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Payroll, PageBase.GetText(3692, "Rutiner"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Payroll_TimeWorkAccount, PageBase.GetText(91955, "Arbetstidskonto"), "/soe/time/payroll/worktimeaccount/");
                    tabContent.AddLink(tabContent, Feature.Time_Payroll_UnionFee, PageBase.GetText(8622, "Fackavgifter"), "/soe/time/payroll/unionfee/");
                    tabContent.AddLink(tabContent, Feature.Time_Payroll_Provision_AccountProvisionBase, PageBase.GetText(8625, "Provision - Underlag"), "/soe/time/Payroll/accountprovision/accountprovisionbase/");
                    tabContent.AddLink(tabContent, Feature.Time_Payroll_Provision_AccountProvisionTransaction, PageBase.GetText(8626, "Provision - Beräkning"), "/soe/time/Payroll/accountprovision/accountprovisiontransaction/");
                    tabContent.AddLink(tabContent, Feature.Time_Payroll_VacationYearEnd, PageBase.GetText(3207, "Semesterårsskifte"), "/soe/time/payroll/vacationyearend/");
                }
            }

            #endregion

            #region Import

            tab = moduleContent.AddTab(Feature.Time_Import, PageBase.GetText(5006, "Import"), iconPrefix + "fa-download");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Import, PageBase.GetText(5590, "Lön"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Import_Salary, PageBase.GetText(5746, "Importera lönespecar"), "/soe/time/import/salary/");
                    tabContent.AddLink(tabContent, Feature.Time_Import_Salary, PageBase.GetText(5747, "Importerade lönespecar"), "/soe/time/import/salary/imported/");
                    tabContent.AddLink(tabContent, Feature.Time_Import_PayrollStartValuesImported, PageBase.GetText(12153, "Startvärden för lön"), "/soe/time/import/payrollstartvalues/");
                    tabContent.AddLink(tabContent, Feature.Time_Import_PayrollImport, PageBase.GetText(12067, "Importera från försystem"), "/soe/time/import/payrollimport/");
                    tabContent.AddLink(tabContent, Feature.Time_Import_ExcelImport, PageBase.GetText(4258, "Excel"), "/soe/time/import/excelimport/");
                    tabContent.AddLink(tabContent, Feature.Time_Import_XEConnect, PageBase.GetText(9084, "Connect"), "/soe/time/import/xeconnect/");
                    tabContent.AddLink(tabContent, Feature.Time_Import_API, PageBase.GetText(11965, "API"), "/soe/time/import/api/");
                }
            }

            #endregion

            #region Export

            tab = moduleContent.AddTab(Feature.Time_Export, PageBase.GetText(5007, "Export"), iconPrefix + "fa-upload");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Export, PageBase.GetText(5590, "Lön"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Export_Salary, PageBase.GetText(8207, "Löneexport"), "/soe/time/export/salary/selection/");
                    tabContent.AddLink(tabContent, Feature.Time_Export_StandardDefinitions, PageBase.GetText(3868, "Standarddefinitioner"), "/soe/time/export/standarddef/");
                    tabContent.AddLink(tabContent, Feature.Time_Export_XEConnect, PageBase.GetText(9084, "Connect"), "/soe/time/export/xeconnect/");
                }
            }

            #endregion

            #region Distribution

            tab = moduleContent.AddTab(Feature.Time_Distribution, PageBase.GetText(5008, "Rapporter"), iconPrefix + "fa-print");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Distribution, PageBase.GetText(7086, "Hantera"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Distribution_Packages, PageBase.GetText(2079, "Rapportpaket"), "/soe/time/distribution/packages/");
                    tabContent.AddLink(tabContent, Feature.Time_Distribution_Groups, PageBase.GetText(2020, "Rapportgrupper"), "/soe/time/distribution/groups/");
                    tabContent.AddLink(tabContent, Feature.Time_Distribution_Headers, PageBase.GetText(2213, "Rapportrubriker"), "/soe/time/distribution/headers/");
                    tabContent.AddLink(tabContent, Feature.Time_Distribution_Templates, PageBase.GetText(1672, "Rapportmallar egna"), "/soe/time/distribution/templates/");
                    tabContent.AddLink(tabContent, Feature.Time_Distribution_SysTemplates, PageBase.GetText(1375, "Rapportmallar system"), "/soe/time/distribution/systemplates/");
                }
            }

            #endregion

            #region Preferences

            tab = moduleContent.AddTab(Feature.Time_Preferences, PageBase.GetText(14, "Inställningar"), iconPrefix + "fa-cog");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Preferences, PageBase.GetText(5011, "Generellt"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_CompSettings, PageBase.GetText(5415, "Företagsinställningar"), "/soe/time/preferences/compsettings/");
                    tabContent.AddLink(tabContent, Feature.None, PageBase.GetText(5414, "Användarinställningar"), "/soe/time/preferences/usersettings/");
                    if (PageBase.HasRolePermission(Feature.Common_ExtraFields_Employee_Edit, Permission.Readonly) || PageBase.HasRolePermission(Feature.Common_ExtraFields_PayrollProductSetting, Permission.Readonly))
                        tabContent.AddLink(tabContent, Feature.None, PageBase.GetText(7549, "Extrafält"), "/soe/time/preferences/extrafields/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Preferences, PageBase.GetText(5690, "Planering"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_ShiftType, PageBase.GetText(3791, "Passtyper"), "/soe/time/preferences/schedulesettings/shifttype/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_ShiftTypeLink, PageBase.GetText(11515, "Kombination av passtyper"), "/soe/time/preferences/schedulesettings/shifttypelink/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeScheduleType, PageBase.GetText(3917, "Schematyper"), "/soe/time/preferences/timesettings/timescheduletype/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeCodeBreak, PageBase.GetText(5059, "Rasttyper"), "/soe/time/preferences/timesettings/timecodebreak/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup, PageBase.GetText(5925, "Rastgrupper"), "/soe/time/preferences/timesettings/timecodebreakgroup/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_SkillType, PageBase.GetText(3578, "Kompetenstyper"), "/soe/time/preferences/schedulesettings/skilltype/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_Skill, PageBase.GetText(3579, "Kompetenser"), "/soe/time/preferences/schedulesettings/skill/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_DayTypes, PageBase.GetText(3055, "Dagtyper"), "/soe/time/preferences/schedulesettings/daytypes/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_Holidays, PageBase.GetText(3054, "Avvikelsedagar"), "/soe/time/preferences/schedulesettings/holidays/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_Halfdays, PageBase.GetText(4409, "Halvdagar"), "/soe/time/preferences/schedulesettings/halfdays/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_LeisureCodeType, PageBase.GetText(13003, "Fridagstyper"), "/soe/time/preferences/schedulesettings/leisurecodetype/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_LeisureCode, PageBase.GetText(13004, "Fridagar"), "/soe/time/preferences/schedulesettings/leisurecode/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Preferences, PageBase.GetText(3919, "Behov"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_ScheduleSettings_IncomingDeliveryType, PageBase.GetText(91899, "Typ av leverans"), "/soe/time/preferences/schedulesettings/incomingdeliverytype/");
                    tabContent.AddLink(tabContent, Feature.Time_Schedule_StaffingNeeds_TaskTypes, PageBase.GetText(4686, "Typ av arbetsuppgift"), "/soe/time/preferences/schedulesettings/timescheduletasktype/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_NeedsSettings_LocationGroups, PageBase.GetText(3933, "Platsgrupper"), "/soe/time/preferences/needssettings/locationgroups/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_NeedsSettings_Locations, PageBase.GetText(3931, "Bemanningsplatser"), "/soe/time/preferences/needssettings/locations/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_NeedsSettings_Rules, PageBase.GetText(3935, "Regler"), "/soe/time/preferences/needssettings/rules/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Preferences, PageBase.GetText(5012, "Tid"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_Accounts, PageBase.GetText(5201, "Baskonton tid"), "/soe/time/preferences/timesettings/accounts/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeTerminals, PageBase.GetText(3406, "Terminaler"), "/soe/time/preferences/timesettings/timeterminals/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimePeriodHead, PageBase.GetText(5272, "Perioduppsättning"), "/soe/time/preferences/timesettings/timeperiod/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_PlanningPeriod, PageBase.GetText(11833, "Planeringsperioder"), "/soe/time/preferences/timesettings/planningperiod/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeAccumulator, PageBase.GetText(4318, "Saldon"), "/soe/time/preferences/timesettings/timeaccumulator/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeCodeWork, PageBase.GetText(5057, "Närvarotidkoder"), "/soe/time/preferences/timesettings/timecodework/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeCodeAbsense, PageBase.GetText(5058, "Frånvarotidkoder"), "/soe/time/preferences/timesettings/timecodeabsense/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeCodeRanking, PageBase.GetText(8957, "Tidkodsviktning"), "/soe/time/preferences/timesettings/timecoderanking/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeDeviationCause, PageBase.GetText(4304, "Avvikelseorsaker"), "/soe/time/preferences/timesettings/timedeviationcause/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeCodeAdditionDeduction, PageBase.GetText(10123, "Resa/utlägg"), "/soe/time/preferences/timesettings/timecodeadditiondeduction/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeRule, PageBase.GetText(4322, "Tidsregler"), "/soe/time/preferences/timesettings/timerule/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_TimeSettings_TimeAbsenceRule, PageBase.GetText(3517, "Frånvaroregler"), "/soe/time/preferences/timesettings/timeabsencerule/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Time_Preferences, PageBase.GetText(5590, "Lön"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_SalarySettings_PayrollProduct, PageBase.GetText(6000, "Lönearter"), "/soe/time/preferences/salarysettings/payrollproduct/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_SalarySettings_PriceType, PageBase.GetText(7190, "Lönetyper"), "/soe/time/preferences/salarysettings/pricetype/");
                    tabContent.AddLink(tabContent, Feature.Time_Preferences_SalarySettings_PriceFormula, PageBase.GetText(3985, "Löneformler"), "/soe/time/preferences/salarysettings/priceformula/");
                }
            }

            #endregion

            moduleContent.Render(parent);
        }
        private void RenderModuleClientManagement(HtmlGenericControl parent)
        {
            LeftMenuModuleContent moduleContent = new LeftMenuModuleContent(SoeModule.ClientManagement, Page?.Request);
            LeftMenuTab tab;
            LeftMenuTabContent tabContent;

            #region Favorites

            RenderFavorites(moduleContent);

            #endregion

            #region Clients 

            tab = moduleContent.AddTab(Feature.ClientManagement_Clients, PageBase.GetText(6501, "Klienter"), iconPrefix + "fa-buildings");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.ClientManagement_Clients, PageBase.GetText(5014, "Register"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.ClientManagement_Clients, PageBase.GetText(6501, "Klienter"), "/soe/clientmanagement/clients/");
                }
            }

			#endregion

			#region Suppliers

			tab = moduleContent.AddTab(Feature.ClientManagement_Suppliers, PageBase.GetText(6504, "Leverantörer"), iconPrefix + "fa-truck");
			if (tab != null)
			{
				tabContent = moduleContent.AddTabContent(tab, Feature.ClientManagement_Suppliers, PageBase.GetText(5014, "Register"), false);
				if (tabContent != null)
				{
					tabContent.AddLink(tabContent, Feature.ClientManagement_Supplier_Invoices, PageBase.GetText(6505, "Fakturaöversikt"), "/soe/clientmanagement/suppliers/invoices/overview/");
				}
			}

			#endregion

			moduleContent.Render(parent);
        }

        private void RenderModuleManage(HtmlGenericControl parent)
        {
            LeftMenuModuleContent moduleContent = new LeftMenuModuleContent(SoeModule.Manage, Page?.Request);
            LeftMenuTab tab;
            LeftMenuTabContent tabContent;

            #region Favorites

            RenderFavorites(moduleContent);

            #endregion

            #region Dashboard

            tab = moduleContent.AddTab(Feature.Manage_Dashboard, PageBase.GetText(3527, "Paneler"), iconPrefix + "fa-tachometer-alt");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Dashboard, PageBase.GetText(5418, "Visa"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Dashboard, PageBase.GetText(3572, "Översiktspanel"), "/soe/manage/?autoLoadOnStart=true");
                }
            }

            #endregion

            #region Contracts

            tab = moduleContent.AddTab(Feature.Manage_Contracts, PageBase.GetText(8, "Licens"), iconPrefix + "fa-certificate");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Contracts, PageBase.GetText(5014, "Register"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Contracts, PageBase.GetText(5449, "Licenser"), "/soe/manage/contracts/");
                }
            }

            #endregion

            #region Companies

            tab = moduleContent.AddTab(Feature.Manage_Companies, PageBase.GetText(2064, "Företag"), iconPrefix + "fa-building");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Companies, PageBase.GetText(5014, "Register"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Companies, PageBase.GetText(2064, "Företag"), "/soe/manage/companies/?license=" + PageBase.SoeLicense.LicenseId + "&licenseNr=" + PageBase.SoeLicense.LicenseNr);
                }
            }

            #endregion

            #region Roles

            tab = moduleContent.AddTab(Feature.Manage_Roles, PageBase.GetText(5450, "Roll"), iconPrefix + "fa-user-tag");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Roles, PageBase.GetText(5014, "Register"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Roles, PageBase.GetText(2037, "Roller"), "/soe/manage/roles/?license=" + PageBase.SoeLicense.LicenseId + "&licenseNr=" + PageBase.SoeLicense.LicenseNr + "&company=" + PageBase.SoeCompany.ActorCompanyId);
                }
            }

            #endregion

            #region Users

            tab = moduleContent.AddTab(Feature.Manage_Users, PageBase.GetText(9, "Användare"), iconPrefix + "fa-user");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Users, PageBase.GetText(5014, "Register"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Users, PageBase.GetText(9, "Användare"), "/soe/manage/users/?license=" + PageBase.SoeLicense.LicenseId + "&company=" + PageBase.SoeCompany.ActorCompanyId);
                    tabContent.AddLink(tabContent, Feature.Manage_Users_ServiceUsers, PageBase.GetText(7786, "Serviceanvändare"), "/soe/manage/users/service/?license=" + PageBase.SoeLicense.LicenseId + "&company=" + PageBase.SoeCompany.ActorCompanyId);
                }
            }

            #endregion

            #region ContactPersons

            tab = moduleContent.AddTab(Feature.Manage_ContactPersons, PageBase.GetText(5451, "Kontaktperson"), iconPrefix + "fa-male");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_ContactPersons, PageBase.GetText(5014, "Register"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_ContactPersons, PageBase.GetText(1603, "Kontaktpersoner"), "/soe/manage/contactpersons/");
                    tabContent.AddLink(tabContent, Feature.Common_Categories_ContactPersons, PageBase.GetText(4124, "Kategorier"), "/soe/manage/contactpersons/categories/?type=" + ((int)SoeCategoryType.ContactPerson).ToString());
                }
            }

            #endregion

            #region Attest

            tab = moduleContent.AddTab(Feature.Manage_Attest, PageBase.GetText(5217, "Attest"), iconPrefix + "fa-check-circle");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Attest_Time, PageBase.GetText(5731, "Tid"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Time_AttestStates, PageBase.GetText(5729, "Nivåer"), "/soe/manage/attest/time/state/");
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Time_AttestTransitions, PageBase.GetText(5730, "Övergångar"), "/soe/manage/attest/time/transition/");
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Time_AttestRoles, PageBase.GetText(5728, "Roller"), "/soe/manage/attest/time/role/");
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Time_AttestRules, PageBase.GetText(3469, "Automatattestregler"), "/soe/manage/attest/time/rule/");
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Attest_Customer, PageBase.GetText(5727, "Offert/Order"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Customer_AttestStates, PageBase.GetText(5729, "Nivåer"), "/soe/manage/attest/customer/state/");
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Customer_AttestTransitions, PageBase.GetText(5730, "Övergångar"), "/soe/manage/attest/customer/transition/");
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Customer_AttestRoles, PageBase.GetText(5728, "Roller"), "/soe/manage/attest/customer/role/");
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Attest_Supplier, PageBase.GetText(5732, "Leverantör"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Supplier_AttestStates, PageBase.GetText(5729, "Nivåer"), "/soe/manage/attest/supplier/state/");
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Supplier_AttestTransitions, PageBase.GetText(5730, "Övergångar"), "/soe/manage/attest/supplier/transition/");
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Supplier_AttestRoles, PageBase.GetText(5728, "Roller"), "/soe/manage/attest/supplier/role/");
                    tabContent.AddLink(tabContent, Feature.Manage_Attest_Supplier_WorkFlowTemplate, PageBase.GetText(54568, "Attestmallar"), "/soe/manage/attest/supplier/workflowtemplate/");
                }
            }
            tab = moduleContent.AddTab(Feature.Manage_Signing, PageBase.GetText(10937, "Signering"), iconPrefix + "fa-check-circle");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Signing_Document, PageBase.GetText(10938, "Dokument"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Signing_Document_States, PageBase.GetText(5729, "Nivåer"), "/soe/manage/signing/document/state/");
                    tabContent.AddLink(tabContent, Feature.Manage_Signing_Document_Transitions, PageBase.GetText(5730, "Övergångar"), "/soe/manage/signing/document/transition/");
                    tabContent.AddLink(tabContent, Feature.Manage_Signing_Document_Roles, PageBase.GetText(5728, "Roller"), "/soe/manage/signing/document/role/");
                    tabContent.AddLink(tabContent, Feature.Manage_Signing_Document_Templates, PageBase.GetText(7192, "Mallar"), "/soe/manage/signing/document/template/");
                }
            }

            #endregion

            #region Preferences

            tab = moduleContent.AddTab(Feature.Manage, PageBase.GetText(14, "Inställningar"), iconPrefix + "fa-cog");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage, PageBase.GetText(5011, "Generellt"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_LicenseSettings, PageBase.GetText(11954, "Licensinställningar"), "/soe/manage/preferences/licensesettings/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_CompSettings, PageBase.GetText(5415, "Företagsinställningar"), "/soe/manage/preferences/compsettings/");
                    tabContent.AddLink(tabContent, Feature.None, PageBase.GetText(5414, "Användarinställningar"), "/soe/manage/preferences/usersettings/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_SystemInfoSettings, PageBase.GetText(7089, "Systeminfoinställningar"), "/soe/manage/preferences/systeminfosettings/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_FieldSettings, PageBase.GetText(5710, "Fältinställningar mobil"), "/soe/manage/preferences/fieldsettings/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_LogotypeSettings, PageBase.GetText(5417, "Logotypinställningar"), "/soe/manage/preferences/logotype/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_CheckSettings, PageBase.GetText(3463, "Kontrollera inställningar"), "/soe/manage/preferences/checksettings/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_CompanyInformation, PageBase.GetText(7187, "Intern information"), "/soe/manage/preferences/companyinformation/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Preferences, PageBase.GetText(5014, "Register"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_Registry_SchoolHoliday, PageBase.GetText(4908, "Skollov"), "/soe/manage/preferences/registry/schoolholiday/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_Registry_OpeningHours, PageBase.GetText(4909, "Öppettider"), "/soe/manage/preferences/registry/openinghours/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_Registry_Positions, PageBase.GetText(3959, "Befattningar"), "/soe/manage/preferences/registry/positions/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_Registry_Checklists, PageBase.GetText(5695, "Checklistor"), "/soe/manage/preferences/registry/checklists/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_Registry_EventReceiverGroups, PageBase.GetText(4692, "Mottagargrupper"), "/soe/manage/preferences/registry/eventreceivergroup/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_Registry_ScheduledJobs, PageBase.GetText(12019, "Schemaläggningar"), "/soe/manage/preferences/registry/scheduledjobs/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_Registry_ExternalCodes, PageBase.GetText(12080, "Externa koder"), "/soe/manage/preferences/registry/externalcodes/");
                    tabContent.AddLink(tabContent, Feature.Manage_Preferences_Registry_SignatoryContract, PageBase.GetText(124, "Signeringsavtal"), "/soe/manage/preferences/registry/signatorycontract/");
                }
            }

            #endregion

            #region System

            tab = moduleContent.AddTab(Feature.Manage_System, PageBase.GetText(4162, "System"), iconPrefix + "fa-key");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, PageBase.GetText(5127, "Admin"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(5958, "Övervakning"), "/soe/manage/system/admin/fullscreen/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(5529, "Språköversättning"), "/soe/manage/system/admin/language/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(4493, "Publik information"), "/soe/manage/system/admin/sysinformation/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(5176, "Publika nyheter"), "/soe/manage/system/admin/news/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(5128, "Ladda om termcache"), "/soe/manage/system/admin/tasks/?task=" + (int)AdminTaskType.RestoreTermCache);
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(5212, "Ladda om syscache"), "/soe/manage/system/admin/tasks/?task=" + (int)AdminTaskType.RestoreSysCache);
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(5313, "Ladda om företagscache"), "/soe/manage/system/admin/tasks/?task=" + (int)AdminTaskType.RestoreCompCache);
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(5941, "Töm tidsregelcache"), "/soe/manage/system/admin/tasks/?task=" + (int)AdminTaskType.ClearTimeRuleCache);
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(5152, "Kopiera behörigheter"), "/soe/manage/system/admin/features/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(1997, "Sätt artikelbehörigheter"), "/soe/manage/system/admin/xearticles/?permission=" + (int)Permission.Modify);
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(4763, "Kopiera checklistor"), "/soe/manage/system/admin/checklists/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(12174, "UI komponenter testsida"), "/soe/manage/system/admin/uicomponents/");
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, PageBase.GetText(3390, "Schemaläggaren"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(3391, "Registrerade jobb"), "/soe/manage/system/scheduler/jobs/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(3392, "Schemalagda jobb"), "/soe/manage/system/scheduler/scheduledjobs/");
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, PageBase.GetText(6410, "Tester"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(6411, "Testfall"), "/soe/manage/system/test/testcases/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(12125, "Testgrupper"), "/soe/manage/system/test/testgroups/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(6413, "Testresultat"), "/soe/manage/system/test/testresult/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, PageBase.GetText(12555, "Kommunikatör"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System_Communicator, PageBase.GetText(6422, "Inkommande e-post"), "/soe/manage/system/communicator/incomingemail/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, "EDI");
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(1023, "Meddelande"), "/soe/manage/system/EDI/SysEdiMessageHead/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, PageBase.GetText(4164, "Prislista"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System_Price_List, PageBase.GetText(6160, "Systemprislistor"), "/soe/manage/system/importpricelist/");
                }                
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, PageBase.GetText(3059, "Lön"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(3060, "Systempriser"), "/soe/manage/system/payroll/syspayrollprices/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(9295, "Biltyper"), "/soe/manage/system/payroll/sysvehicletypes/");
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, PageBase.GetText(4756, "Volymfakturering"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(4757, "Underlag"), "/soe/manage/system/volymeinvoicing/");
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, PageBase.GetText(3866, "Import"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(3868, "Standarddefinitioner"), "/soe/manage/system/import/standarddef/");
                }
                /*
                // Preparations for Sys level export
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, PageBase.GetText(3870, "Export"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(3868, "Standarddefinitioner"), "/soe/manage/system/export/standarddef/");
                }
                */
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, "SysCompany");
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(1011, "Företag"), "/soe/manage/system/SysCompany/syscompany/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(5125, "Server"), "/soe/manage/system/SysCompany/syscompServer/");
                    tabContent.AddLink(tabContent, Feature.Manage_System, PageBase.GetText(7755, "Databaser"), "/soe/manage/system/SysCompany/syscompDB/");
                }
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System, "Server Utility");
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System, "Status Angularsidor", "/soe/manage/system/softoneserverutility/pagestatuses/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System_Intrastat, PageBase.GetText(7633, "Intrastat"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System_Intrastat_StatisticalCommodityCodes, PageBase.GetText(7634, "Statistiska varukoder"), "/soe/manage/system/intrastat/commoditycodes/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_System_BankIntegration, PageBase.GetText(7676, "Bankintegration"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_System_BankIntegration, PageBase.GetText(7676, "Bankintegration"), "/soe/manage/system/bankintegration/");
                }
            }

            #endregion

            #region GDPR

            tab = moduleContent.AddTab(Feature.Manage_GDPR, PageBase.GetText(3661, "GDPR"), iconPrefix + "fa-lock-alt");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_GDPR_Logs, PageBase.GetText(1008, "Loggar"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_GDPR_Logs, PageBase.GetText(3662, "Sök individ"), "/soe/manage/gdpr/logs/");
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_GDPR_Registry, PageBase.GetText(5014, "Register"), false);
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_GRPR_Registry_HandlePersonalInfo, PageBase.GetText(7412, "Administrera personuppgifter"), "/soe/manage/gdpr/registry/handleinfo/");
                }
            }

            #endregion

            #region Logs

            // Old Support moved into logs, therefore check both permissions
            tab = moduleContent.AddTab(new List<Feature>() { Feature.Manage_Logs, Feature.Manage_Support }, PageBase.GetText(12044, "Loggar"), iconPrefix + "fa-layer-group");
            if (tab != null)
            {
                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Support_Logs, PageBase.GetText(12114, "Systemloggar"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Support_Logs_System, String.Format("{0}", PageBase.GetText(7066, "Sök")), "/soe/manage/support/logs/?type=" + (int)SoeLogType.System_Search);
                    tabContent.AddLink(tabContent, Feature.Manage_Support_Logs_System, String.Format("{0} {1}", PageBase.GetText(5979, "Händelser"), PageBase.GetText(1538, "idag")), "/soe/manage/support/logs/?type=" + (int)SoeLogType.System_All_Today);
                    tabContent.AddLink(tabContent, Feature.Manage_Support_Logs_System, String.Format("{0} {1}", PageBase.GetText(5978, "Fel"), PageBase.GetText(1538, "idag")), "/soe/manage/support/logs/?type=" + (int)SoeLogType.System_Error_Today);
                    tabContent.AddLink(tabContent, Feature.Manage_Support_Logs_System, String.Format("{0} {1}", PageBase.GetText(1541, "Varningar"), PageBase.GetText(1538, "idag")), "/soe/manage/support/logs/?type=" + (int)SoeLogType.System_Warning_Today);
                    tabContent.AddLink(tabContent, Feature.Manage_Support_Logs_System, String.Format("{0} {1}", PageBase.GetText(1804, "Information"), PageBase.GetText(1538, "idag")), "/soe/manage/support/logs/?type=" + (int)SoeLogType.System_Information_Today);
                }

                tabContent = moduleContent.AddTabContent(tab, Feature.Manage_Logs_ChangeLogs, PageBase.GetText(12045, "Förändringsloggar"));
                if (tabContent != null)
                {
                    tabContent.AddLink(tabContent, Feature.Manage_Logs_ChangeLogs_Search, PageBase.GetText(12046, "Sök"), "/soe/manage/logs/changelogs/search/");
                }
            }

            #endregion

            moduleContent.Render(parent);
        }

        private void RenderFavorites(LeftMenuModuleContent moduleContent)
        {
            if (moduleContent == null)
                return;

            LeftMenuTab tab = moduleContent.AddTab(Feature.None, PageBase.GetText(2002, "Favoriter"), iconPrefix + "fa-star");
            if (tab != null)
            {
                List<FavoriteItem> favoriteItems = SettingCacheManager.Instance.GetUserFavorites(PageBase.UserId);
                LeftMenuTabContent tabContent = moduleContent.AddTabContent(tab, Feature.None, PageBase.GetText(7086, "Hantera"), false, "no-active-items");
                if (tabContent != null)
                {
                    favoriteItems = favoriteItems.Count > 10 ? favoriteItems.GetRange(0, 10) : favoriteItems;
                    favoriteItems.ForEach((favoriteItem) =>
                    {
                        tabContent.AddFavoriteItem(tabContent, Feature.None, favoriteItem);
                    });

                    string url = HttpContext.Current?.Request?.Url?.PathAndQuery;
                    if (!string.IsNullOrEmpty(url))
                        tabContent.AddLink(tabContent, Feature.None, PageBase.GetText(2003, "Lägg till favorit"), $"/modalforms/RegFavorite.aspx?c={PageBase.SoeCompany.ActorCompanyId}&url={Server.UrlEncode(url)}", true, "add-favorite-item");
                }
            }
        }

        private SoeModule GetActiveModule()
        {
            SoeModule currentModule = SoeModule.None;

            switch (Module)
            {
                case Constants.SOE_MODULE_BILLING:
                    currentModule = SoeModule.Billing;
                    break;
                case Constants.SOE_MODULE_ECONOMY:
                    currentModule = SoeModule.Economy;
                    break;
                case Constants.SOE_MODULE_TIME:
                    currentModule = SoeModule.Time;
                    break;
                case Constants.SOE_MODULE_MANAGE:
                    currentModule = SoeModule.Manage;
                    break;
                case Constants.SOE_MODULE_CLIENTMANAGEMENT:
                    currentModule = SoeModule.ClientManagement;
                    break;
            }

            return currentModule;
        }

        #region Help-classes

        public class LefteMenuModules : LeftMenu
        {
            //Init params
            public SoeModule ActiveModule { get; set; }

            //Getters
            public Guid Guid { get; }
            public List<LeftMenuModule> Modules { get; }

            public LefteMenuModules()
            {
                this.ActiveModule = GetActiveModule();

                this.Guid = Guid.NewGuid();
                this.Modules = new List<LeftMenuModule>();
            }

            public LeftMenuModule AddModule(SoeModule currentModule, Feature feature, string title, string imageSrc)
            {
                if (!PageBase.HasRolePermission(feature, Permission.Readonly))
                    return null;

                var module = new LeftMenuModule(currentModule, feature, title, imageSrc);
                this.Modules.Add(module);

                return module;
            }

            public void Render(HtmlGenericControl parent)
            {
                var divModuleSelector = new HtmlGenericControl("div");
                divModuleSelector.Attributes.Add("class", "module-selector");

                var ulModuleSelector = new HtmlGenericControl("ul");
                ulModuleSelector.Attributes.Add("class", "module-selector-list");
                ulModuleSelector.Attributes.Add("id", "moduleSelector");

                foreach (var module in this.Modules)
                {
                    ulModuleSelector.Controls.Add(module.Render(module.CurrentModule == this.ActiveModule));
                }

                divModuleSelector.Controls.Add(ulModuleSelector);

                parent.Controls.Add(divModuleSelector);
            }
        }

        public class LeftMenuModule : LeftMenu
        {
            //Init params
            public SoeModule CurrentModule { get; set; }
            public Feature Feature { get; set; }
            public string Title { get; set; }
            public string ImageSrc { get; set; }

            //Getters
            public string Href { get; }
            public Guid Guid { get; }

            public LeftMenuModule(SoeModule currentModule, Feature feature, string title, string imageSrc)
            {
                this.CurrentModule = currentModule;
                this.Feature = feature;
                this.Title = title;
                this.ImageSrc = imageSrc;

                this.Href = "module_" + Enum.GetName(typeof(SoeModule), this.CurrentModule).ToLower();
                this.Guid = Guid.NewGuid();
            }

            public HtmlGenericControl Render(bool activeTab)
            {
                var li = new HtmlGenericControl("li");
                li.Attributes.Add("id", this.CurrentModule.ToString().ToLower());
                li.Attributes.Add("class", activeTab ? "module-active" : "module-inactive");

                var a = new HtmlGenericControl("a");
                a.Attributes.Add("href", "#/" + this.Href.ToLower());
                a.Attributes.Add("data-toggle", "tab");
                a.Attributes.Add("data-bs-toggle", "tab");
                a.Attributes.Add("data-target", "#" + this.Href.ToLower());
                a.Attributes.Add("data-bs-target", "#" + this.Href.ToLower());
                a.Attributes.Add("data-parent", "#ModuleSelector");
                a.Attributes.Add("data-value", this.Title);
                a.Attributes.Add("data-collapse-group", "moduleDivs");
                a.Attributes.Add("class", activeTab ? "module-active" : "module-inactive");

                var container = new HtmlGenericControl("div");
                container.Attributes.Add("class", "module-icon-text-container");

                var icon = new HtmlGenericControl("span");
                icon.Attributes.Add("class", "module-icon fa-fw fal " + this.ImageSrc);
                container.Controls.Add(icon);

                var textSpan = new HtmlGenericControl("span")
                {
                    InnerText = this.Title,
                };
                textSpan.Attributes.Add("class", "module-text");
                container.Controls.Add(textSpan);
                a.Controls.Add(container);
                li.Controls.Add(a);

                return li;
            }
        }

        public class LeftMenuModuleContent : LeftMenu
        {
            //Init params
            public SoeModule TabModule { get; set; }
            public SoeModule ActiveModule { get; set; }
            public HttpRequest PageRequest { get; set; }

            //Getters
            public Guid Guid { get; }
            public List<LeftMenuTab> Tabs { get; }
            public List<LeftMenuTabContent> TabContents { get; }

            public LeftMenuModuleContent(SoeModule tabModule, HttpRequest pageRequest)
            {
                this.TabModule = tabModule;
                this.ActiveModule = GetActiveModule();
                this.PageRequest = pageRequest;

                this.Guid = Guid.NewGuid();
                this.Tabs = new List<LeftMenuTab>();
                this.TabContents = new List<LeftMenuTabContent>();
            }

            public LeftMenuTab AddTab(Feature feature, string label, string imageClass)
            {
                if (!PageBase.HasRolePermission(feature, Permission.Readonly))
                    return null;

                LeftMenuTab tab = new LeftMenuTab(feature, feature.ToString(), label, imageClass);
                this.Tabs.Add(tab);

                return tab;
            }

            public LeftMenuTab AddTab(List<Feature> features, string label, string imageClass)
            {
                LeftMenuTab tab = null;
                foreach (Feature feature in features)
                {
                    tab = AddTab(feature, label, imageClass);
                    if (tab != null)
                        break;
                }

                return tab;
            }

            public LeftMenuTabContent AddTabContent(LeftMenuTab parentTab, Feature feature, string label, bool showHeader = true, string extraClass = "")
            {
                if (!PageBase.HasRolePermission(feature, Permission.Readonly))
                    return null;

                var tabContent = new LeftMenuTabContent(feature, label, parentTab, this.PageRequest, showHeader, extraClass);
                this.TabContents.Add(tabContent);

                return tabContent;
            }

            public void Render(HtmlGenericControl parent)
            {
                var divModule = new HtmlGenericControl("div");
                divModule.Attributes.Add("id", "module_" + Enum.GetName(typeof(SoeModule), this.TabModule).ToLower());

                if (this.ActiveModule == this.TabModule)
                    divModule.Attributes.Add("class", "module-active collapse in");
                else
                    divModule.Attributes.Add("class", "module-inactive collapse");

                var ulNavTabs = new HtmlGenericControl("ul");
                ulNavTabs.Attributes.Add("class", "nav nav-tabs");

                var divTabContent = new HtmlGenericControl("div");
                divTabContent.Attributes.Add("class", "tab-content");

                foreach (var tab in this.Tabs)
                {
                    var tabContents = this.TabContents.Where(i => i.ParentTab.Guid == tab.Guid && i.Links.Count > 0);
                    var activeTabContent = tabContents.FirstOrDefault(i => i.IsActive);
                    bool isTabActive = false;
                    if (activeTabContent != null)
                        isTabActive = !activeTabContent.ExtraClass.Equals("no-active-items");

                    var divTabPane = new HtmlGenericControl("div");
                    divTabPane.Attributes.Add("id", tab.Href.ToLower());
                    divTabPane.Attributes.Add("class", isTabActive ? "tab-pane active" : "tab-pane");

                    var divPanelGroup = new HtmlGenericControl("div")
                    {
                        ID = tab.Guid + "_accordion",
                    };
                    divPanelGroup.Attributes.Add("role", "tablist");
                    divPanelGroup.Attributes.Add("class", "panel-group");

                    int tabContentId = 1;
                    foreach (var tabContent in tabContents)
                    {
                        bool expanded = activeTabContent != null ? activeTabContent.Guid == tabContent.Guid : tabContentId == 1;
                        divPanelGroup.Controls.Add(tabContent.Render(divPanelGroup.ID, expanded: expanded));
                        tabContentId++;
                    }

                    divTabPane.Controls.Add(divPanelGroup);
                    divTabContent.Controls.Add(divTabPane);
                    ulNavTabs.Controls.Add(tab.Render(isTabActive, divTabPane));

                }
                divModule.Controls.Add(ulNavTabs);
                parent.Controls.Add(divModule);
            }
        }

        public class LeftMenuTab : LeftMenu
        {
            //Init params
            public Feature Feature { get; set; }
            public string Href { get; set; }
            public string Label { get; set; }
            public string ImageClass { get; set; }

            //Getters
            public Guid Guid { get; }

            public LeftMenuTab(Feature feature, string href, string label, string imageClass)
            {
                this.Feature = feature;
                this.Href = href;
                this.Label = label;
                this.ImageClass = imageClass;

                this.Guid = Guid.NewGuid();
            }

            public HtmlGenericControl Render(bool isActive, HtmlGenericControl tabContents)
            {
                var li = new HtmlGenericControl("li");
                var id = Feature.ToString() + "_menu";
                li.Attributes.Add("id", id);
                if (isActive)
                    li.Attributes.Add("class", "active");

                var a = new HtmlGenericControl("a");
                a.Attributes.Add("class", "tablink hover-trigger");
                var span = new HtmlGenericControl("span");
                span.Attributes.Add("class", $"{this.ImageClass} fa-fw margin-small-right new-menu-icon");

                var textSpan = new HtmlGenericControl("span");
                textSpan.Attributes.Add("class", "new-menu-item left");

                textSpan.InnerText = this.Label;

                a.Controls.Add(span);
                a.Controls.Add(textSpan);
                li.Controls.Add(a);
                li.Controls.Add(tabContents);
                return li;
            }
        }

        public class LeftMenuTabContent : LeftMenu
        {
            //Init params
            public Feature Feature { get; set; }
            public string Label { get; set; }
            public LeftMenuTab ParentTab { get; set; }
            public HttpRequest PageRequest { get; set; }
            public bool ShowHeader { get; }
            public string ExtraClass { get; }

            //Getters
            public Guid Guid { get; }
            public List<LeftMenuItem> Links { get; }
            public bool IsActive
            {
                get
                {
                    return this.Links != null && this.Links.Any(i => i.IsActive);
                }
            }

            public LeftMenuTabContent(Feature feature, string label, LeftMenuTab parentTab, HttpRequest pageRequest, bool showHeader = true, string extraClass = "")
            {
                this.Feature = feature;
                this.Label = label;
                this.ParentTab = parentTab;
                this.PageRequest = pageRequest;
                this.ShowHeader = showHeader;
                this.ExtraClass = extraClass;
                this.Guid = Guid.NewGuid();
                this.Links = new List<LeftMenuItem>();
            }

            public LeftMenuLink AddLink(LeftMenuTabContent tabContent, Feature feature, string label, string href, bool modal = false, string linkClass = "")
            {
                if (!PageBase.HasRolePermission(feature, Permission.Readonly))
                    return null;

                var link = new LeftMenuLink(feature, href, label, modal, tabContent, this.PageRequest, linkClass);
                this.Links.Add(link);

                return link;
            }

            public FavoriteMenuItem AddFavoriteItem(LeftMenuTabContent tabContent, Feature feature, FavoriteItem favoriteItem)
            {
                var link = new FavoriteMenuItem(feature, favoriteItem, tabContent, this.PageRequest);
                this.Links.Add(link);
                return link;
            }

            public HtmlGenericControl Render(string accordionId, bool expanded = false)
            {
                var divPanel = new HtmlGenericControl("div");
                divPanel.Attributes.Add("class", "panel panel-default");

                string contentId = this.Guid + "_content";

                #region Heading

                if (ShowHeader)
                {
                    var divPanelHeading = new HtmlGenericControl("div");
                    divPanelHeading.Attributes.Add("role", "tab");
                    divPanelHeading.Attributes.Add("class", expanded ? "panel-heading" : "panel-heading collapsed");
                    divPanelHeading.Attributes.Add("data-toggle", "collapse");
                    divPanelHeading.Attributes.Add("data-bs-toggle", "collapse");
                    divPanelHeading.Attributes.Add("data-parent", "#" + accordionId);
                    divPanelHeading.Attributes.Add("data-bs-parent", "#" + accordionId);
                    divPanelHeading.Attributes.Add("href", "#" + contentId);
                    divPanelHeading.Attributes.Add("role", "button");
                    divPanelHeading.Attributes.Add("aria-expanded", expanded ? "true" : "false");

                    var title = new HtmlGenericControl("h4");
                    title.Attributes.Add("class", "panel-title");

                    var a = new HtmlGenericControl("a");
                    a.Attributes.Add("href", "#");
                    a.InnerText = this.Label;
                    title.Controls.Add(a);

                    var span = new HtmlGenericControl("span");
                    span.Attributes.Add("id", "panel-title");
                    span.Attributes.Add("class", expanded ? "fal fa-chevron-up" : "fal fa-chevron-down");
                    title.Controls.Add(span);

                    divPanelHeading.Controls.Add(title);
                    divPanel.Controls.Add(divPanelHeading);
                }

                #endregion

                #region Links

                var divPanelContent = new HtmlGenericControl("div")
                {
                    ID = contentId,
                };
                divPanelContent.Attributes.Add("role", "tabpanel");
                var classes = "panel-collapse collapse";
                if (expanded)
                    classes += " in";
                if (!ShowHeader)
                    classes += " always-expanded";
                if (!string.IsNullOrWhiteSpace(ExtraClass))
                    classes += " " + ExtraClass;
                divPanelContent.Attributes.Add("class", classes);

                var divContentBody = new HtmlGenericControl("div");
                divContentBody.Attributes.Add("class", "panel-body");

                var ulContentBody = new HtmlGenericControl("ul");
                foreach (var link in this.Links)
                {
                    ulContentBody.Controls.Add(link.Render());
                }

                divContentBody.Controls.Add(ulContentBody);
                divPanelContent.Controls.Add(divContentBody);
                divPanel.Controls.Add(divPanelContent);

                #endregion

                return divPanel;
            }
        }

        public class LeftMenuItem : LeftMenu
        {
            public Feature Feature { get; set; }
            public string Href { get; set; }
            public string Label { get; set; }
            public LeftMenuTabContent TabContent { get; set; }
            public HttpRequest PageRequest { get; set; }
            public bool IsActive { get; set; }

            public LeftMenuItem()
            {

            }

            public virtual HtmlGenericControl Render()
            {
                return new HtmlGenericControl("div");
            }

        }
        public class LeftMenuLink : LeftMenuItem
        {
            private readonly string linkClass;

            //Init params
            public bool Modal { get; set; }

            //Getters
            public Guid Guid { get; }


            public LeftMenuLink(Feature feature, string href, string label, bool modal, LeftMenuTabContent tabContent, HttpRequest pageRequest, string linkClass)
            {
                if (!href.Contains("default.aspx"))
                {
                    if (href.EndsWith("/"))
                        href += "default.aspx";
                    else if (!href.EndsWith(".aspx"))
                    {
                        int lastSlash = href.LastIndexOf("/");
                        href = href.Left(lastSlash + 1) + "default.aspx" + href.Right(href.Length - lastSlash - 1);
                    }
                }

                this.Feature = feature;
                this.Href = href;
                this.Label = label;
                this.Modal = modal;
                this.TabContent = tabContent;
                this.PageRequest = pageRequest;
                this.linkClass = linkClass;
                this.Guid = Guid.NewGuid();
                if (this.PageRequest != null && UrlUtil.UrlContainsPathAndQuery(this.Href, this.PageRequest.Path, RemoveMigrationQueryParams(this.PageRequest.Url.Query)))
                    this.IsActive = true;
            }

            private string RemoveMigrationQueryParams(string query)
            {
                return string.Join("&", query.Split('&').Where(s => !(s.StartsWith("spa=") || s.StartsWith("ng="))));
            }

            public override HtmlGenericControl Render()
            {
                var li = new HtmlGenericControl("li");
                var classes = new List<string>();
                if (this.IsActive)
                    classes.Add("active");
                if (!string.IsNullOrEmpty(linkClass))
                    classes.Add(linkClass);
                if (classes.Count > 0)
                {
                    li.Attributes.Add("class", string.Join(" ", classes));
                }
                this.Href = PageBase.AddUrlParameter(this.Href, "c", PageBase.SoeActorCompanyId.ToString(), addFirst: true);

                var a = new HtmlGenericControl("a");
                a.Attributes.Add("href", this.Href.ToLower());
                a.InnerText = this.Label;
                if (this.Modal)
                    a.Attributes.Add("class", "PopLink text-trunkate");
                else
                    a.Attributes.Add("class", "text-trunkate");

                a.Attributes.Add("onclick", $"return navHelper.intercept(event, {(int)this.Feature}, '{this.Href}', '{this.Label}');");

                li.Controls.Add(a);

                return li;
            }
        }

        public class FavoriteMenuItem : LeftMenuItem
        {

            public int FavoriteId { get; set; }

            public FavoriteMenuItem(Feature feature, FavoriteItem favoriteItem, LeftMenuTabContent tabContent, HttpRequest pageRequest)
            {
                this.Feature = feature;
                this.Href = favoriteItem.FavoriteUrl;
                this.Label = favoriteItem.FavoriteName;
                this.TabContent = tabContent;
                this.PageRequest = pageRequest;
                this.FavoriteId = favoriteItem.FavoriteId;
            }

            public override HtmlGenericControl Render()
            {
                var li = new HtmlGenericControl("li");

                var a = new HtmlGenericControl("a");
                a.Attributes.Add("href", this.Href.ToLower());
                a.InnerText = this.Label;
                a.Attributes.Add("class", "text-trunkate");
                var removeFavorite = new HtmlGenericControl("span");
                removeFavorite.Attributes.Add("class", "fa fa-times pull-right remove-favorite-item");
                removeFavorite.Attributes.Add("onclick", $"RemoveFavorite({FavoriteId})");
                a.Controls.Add(removeFavorite);
                li.Controls.Add(a);

                return li;
            }
        }

        #endregion
    }
}
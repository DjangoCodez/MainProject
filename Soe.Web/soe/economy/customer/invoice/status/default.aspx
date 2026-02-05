<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.customer.invoice.status._default" %>
<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">
     
        <%if (handleCustomerPayments)
        {%>
            <SOE:AngularHost ID="AngularHost2" ModuleName="Common,Economy, Billing" AppName="Soe.Common.Customer.Payments" runat="server" />            
            <script type="text/javascript">
                if (!soeConfig)
                    soeConfig = {};
                    soeConfig.module = <%= (int)TargetSoeModule %>;
                    soeConfig.feature = <%= (int)FeatureEdit %>;
                    soeConfig.accountYearId = <%= accountYearId %>;
                    soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>;
            </script>        
        <%}
        else
        {%>
        <SOE:AngularHost ID="AngularHost" ModuleName="Common, Economy, Billing" AppName="Soe.Common.Customer.Invoices" runat="server" />
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
            soeConfig.module = <%= (int)TargetSoeModule %>;
            soeConfig.feature = <%= (int)FeatureEdit %>;
            soeConfig.accountYearId = <%= accountYearId %>
                soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
            soeConfig.invoiceId = <%= invoiceId %>
                soeConfig.invoiceNr = <%= invoiceNr %>
        </script>
        <%}%>
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>


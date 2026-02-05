<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.order.status._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    
    <SOE:AngularHost ID="AngularHost" ModuleName="Common, Economy, Billing" AppName="Soe.Common.Customer.Invoices" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.module = <%= (int)TargetSoeModule %>;
        soeConfig.feature = <%= (int)FeatureEdit %>;
        soeConfig.employeeId = <%= employeeId %>;
        soeConfig.accountYearId = <%= accountYearId %>;
        soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>;
        soeConfig.userName = '<%= userName %>';
        soeConfig.invoiceId = <%= invoiceId %>;
        soeConfig.invoiceNr = '<%= invoiceNr %>';
        soeConfig.customerId = '<%= customerId %>';
        soeConfig.projectId = '<%= projectId %>';
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

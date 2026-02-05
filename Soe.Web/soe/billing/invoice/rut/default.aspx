<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.invoice.rut._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">    
    <SOE:AngularHost ID="AngularHost" ModuleName="Billing, Common, Economy" AppName="Soe.Billing.Invoices.HouseholdDeduction" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
                soeConfig.isRut = <%= isRut %>
                soeConfig.module = <%= (int)TargetSoeModule %>
                soeConfig.accountYearId = <%= accountYearId %>
                soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
    </script>    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

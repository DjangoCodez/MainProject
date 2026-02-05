<%@ Page Language="C#" Trace="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.supplier.invoice.agedistribution._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">    
    <SOE:AngularHost ID="AngularHost" ModuleName="Common,Economy" AppName="Soe.Economy.Customer.Invoice.AgeDistribution" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
            soeConfig.invoiceType = <%= invoiceType %>
            soeConfig.accountYearId = <%= accountYearId %>
            soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
    </script>   
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.accounting.voucher.accountdistributionperiod._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">      
    <SOE:AngularHost ID="AngularHost" ModuleName="Common, Economy" AppName="Soe.Economy.Accounting.AccountDistribution" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.accountDistributionType = 'Period';
        soeConfig.accountYearId = <%= accountYearId %>;     
        soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>;     
        soeConfig.exportFileNameTranslationKey = 'economy.accounting.accountdistribution.accountdistributions';
        soeConfig.feature = <%= (int)Feature %>            
        soeConfig.lastpartEntityNameSingleKey = "accountdistribution";
        soeConfig.lastpartentityNameMultipleKey = "accountdistributions";
        soeConfig.lastpartentityNameNewKey = "new";            
    </script>      
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

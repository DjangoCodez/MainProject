<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.accounting.voucher.accountdistributionauto._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">   
    <%if (UseAngularSpa) {%>
        <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else {%>
        <SOE:AngularHost ID="AngularHost" ModuleName="Common, Economy" AppName="Soe.Economy.Accounting.AccountDistribution" runat="server" />
    <%}%>

    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.accountDistributionType = 'Auto';            
        soeConfig.exportFileNameTranslationKey = 'economy.accounting.accountdistributionauto.accountdistributionsauto';            
        soeConfig.feature = <%= (int)Feature %>            
        soeConfig.lastpartEntityNameSingleKey = "accountdistributionauto";
        soeConfig.lastpartentityNameMultipleKey = "accountdistributionsauto";
        soeConfig.lastpartentityNameNewKey = "newauto";            
    </script>          
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

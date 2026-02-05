<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.accounting.yearend._default" Title="Untitled Page" %>
<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <%if (UseAngularSpa) {%>
    <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else {%>
    <SOE:AngularHost ID="AngularHost" ModuleName="Economy" AppName="Soe.Economy.Accounting.YearEnd" runat="server" />
    <%}%>
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.accountYearId = <%= accountYearId %>
        soeConfig.createNewAccountYear = <%= createYear.ToString().ToLower() %>
        soeConfig.openAccountYearId = <%= openAccountYearId %>
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

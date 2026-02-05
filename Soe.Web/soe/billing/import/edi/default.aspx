<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.import.edi._default" %>

<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <%if (UseAngularSpa)
        { %>
    <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%}
        else
        {%>
    <SOE:AngularHost ID="AngularHost" ModuleName="Billing" AppName="Soe.Billing.Import.Edi" runat="server" />
    <%} %>
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.feature = <%= (int)FeatureEdit %>;
        soeConfig.accountYearId = <%= accountYearId %>
            soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

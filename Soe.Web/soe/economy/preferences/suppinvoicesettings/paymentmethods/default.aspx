<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.suppinvoicesettings.paymentmethods._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content3" ContentPlaceHolderID="soeMainContent" runat="server">
    <%if (UseAngularSpa)
        {%>
    <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%}
        else
        {%>
    <SOE:AngularHost ID="AngularHost" ModuleName="Economy" AppName="Soe.Economy.Supplier.PaymentMethods" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
    </script>
    <%}%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

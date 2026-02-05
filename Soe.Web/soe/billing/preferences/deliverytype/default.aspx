<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.preferences.deliverytype._default" %>
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
    <SOE:AngularHost ID="AngularHost1" ModuleName="Time" AppName="Soe.Billing.Invoices.DeliveryType" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
    </script>
    <%}%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

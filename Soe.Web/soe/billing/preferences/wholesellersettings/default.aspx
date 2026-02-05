<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.preferences.wholesellerssettings._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>

<asp:Content ID="Content2" ContentPlaceHolderID="soeMainContent" runat="server">
        <SOE:AngularHost ID="AngularHost1" ModuleName="Billing" AppName="Soe.Billing.Invoices.WholesellerSettings" runat="server" />
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
        </script>
</asp:Content>

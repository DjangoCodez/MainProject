<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.vouchersettings.vatcodes._default" %>

<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">

    <%if (UseAngularSpa) {%>
        <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else {%>
        <SOE:AngularHost ID="AngularHost" ModuleName="Common, Economy" AppName="Soe.Economy.Accounting.VatCodes" runat="server" />
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
        </script>
    <%}%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

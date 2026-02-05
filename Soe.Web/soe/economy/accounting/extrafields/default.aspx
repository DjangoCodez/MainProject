<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.accounting.extrafields._default" %>
<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content2" ContentPlaceHolderID="soeMainContent" runat="server">
    <%if (UseAngularSpa) {%>
      <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else {%>
      <SOE:AngularHost ID="AngularHost1" ModuleName="Economy" AppName="Soe.Common.ExtraFields" runat="server" />
    <%}%>
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.feature = <%= (int)Feature %>;
        soeConfig.entity = <%= (int)Entity %>;
    </script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

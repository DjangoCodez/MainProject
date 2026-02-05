<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.xeconnect._default" %>

<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content3" ContentPlaceHolderID="soeMainContent" runat="server">
    <%if (UseAngularSpa)
        {%>
    <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%}
    else
    {%>
    <SOE:AngularHost ID="AngularHost1" ModuleName="Common" AppName="Soe.Common.Connect" runat="server" />
    <%}%>
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
            soeConfig.module = <%= (int)TargetSoeModule %>
            soeConfig.feature = <%= (int)Feature %>
            soeConfig.accountYearId = <%= accountYearId %>
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.support.logs._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">    
    <%if (UseAngularSpa) {%>
        <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else {%>
        <SOE:AngularHost ID="AngularHost" ModuleName="Manage" AppName="Soe.Manage.Support.Logs" runat="server" />
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
            soeConfig.logType = <%= (int)logType %>
            soeConfig.licenceId = <%= SoeLicense.LicenseId %>
            soeConfig.clientIpNr = '<%= clientIpNr %>'
            soeConfig.nrOfLoadsRpdWeb = <%= 0 %>
            soeConfig.nrOfDisposeRpdWeb = <%= 0 %>
            soeConfig.diffRpdWeb = <%= 0 %>
            soeConfig.nrOfLoadsRpdWs = <%= nrOfLoadsRpdWs %>
            soeConfig.nrOfDisposeRpdWs = <%= nrOfDisposeRpdWs %>
            soeConfig.diffRpdWs = <%= diffRpdWs %>
        </script>   
    <%}%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
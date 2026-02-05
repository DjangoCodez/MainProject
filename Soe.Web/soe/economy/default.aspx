<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">	
    <SOE:AngularHost ID="AngularHost" ModuleName="Economy" AppName="Soe.Common.Dashboard" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.module = <%= (int)SoftOne.Soe.Common.Util.SoeModule.Economy %>;
        soeConfig.autoLoadOnStart = <%= autoLoadOnStart.ToString().ToLower() %>;
        soeConfig.clientIpNr = '<%= clientIpNr %>'
        soeConfig.accountYearId = <%= accountYearId %>
        soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
    </script>   
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

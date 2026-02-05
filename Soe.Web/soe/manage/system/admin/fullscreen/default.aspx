<%@ Page Language="C#" MasterPageFile="~/soe/fullscreen.master" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.fullscreen._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
  
    <SOE:AngularHost ID="AngularHost" ModuleName="Common" AppName="Soe.Common.Dashboard" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.module = <%= (int)SoftOne.Soe.Common.Util.SoeModule.None %>;
        soeConfig.autoLoadOnStart = <%= autoLoadOnStart.ToString().ToLower() %>;
        soeConfig.clientIpNr = '<%= clientIpNr %>'
    </script>
     
</asp:Content>

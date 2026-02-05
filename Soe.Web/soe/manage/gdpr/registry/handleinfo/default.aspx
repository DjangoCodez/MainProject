<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.gdpr.registry.handleinfo._default" %>

<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">
        <SOE:AngularHost ID="AngularHost" ModuleName="Manage" AppName="Soe.Manage.Gdpr.Registry.HandleInfo" runat="server" />
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
        </script>
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.syscompany.syscompserver._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
        <SOE:AngularHost ID="AngularHost" ModuleName="Manage" AppName="Soe.Manage.System.SysCompany.SysCompServer" runat="server" />
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
        </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
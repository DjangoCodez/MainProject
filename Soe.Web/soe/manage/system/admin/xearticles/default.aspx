<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.xearticles._default" %>
<%@ Register TagName="FeaturePermissionTree" TagPrefix="SOE" Src="~/UserControls/FeaturePermissionTree.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server" EnableViewState="true">
    <SOE:FeaturePermissionTree ID="FeaturePermissionTree" Runat="Server"></SOE:FeaturePermissionTree>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
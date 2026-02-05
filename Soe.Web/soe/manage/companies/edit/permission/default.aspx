<%@ Page Language="C#" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.companies.edit.permission._default" %>
<%@ Register TagName="FeaturePermissionTree" TagPrefix="SOE" Src="~/UserControls/FeaturePermissionTree.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div>
        <SOE:FeaturePermissionTree ID="FeaturePermissionTree" Runat="Server"></SOE:FeaturePermissionTree>
    </div>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

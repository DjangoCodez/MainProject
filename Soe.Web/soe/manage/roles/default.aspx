<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.roles._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content2" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Manage.Role.Roles" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.selectedLicenseId = <%= licenseId %>;
        soeConfig.selectedLicenseNr = <%= licenseNr %>;
        soeConfig.selectedCompanyId = <%= selectedCompanyId %>;
        soeConfig.isAuthorizedForEdit = '<%= isAuthorizedForEdit %>';
        soeConfig.tabHeader = <%= "'" + tabHeader + "'" %>;
    </script>   
</asp:Content>

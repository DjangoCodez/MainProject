<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.companies.edit._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>


<asp:Content ID="Content3" ContentPlaceHolderID="soeMainContent" runat="server">
<SOE:AngularHost ID="AngularHost" ModuleName="Manage" AppName="Soe.Manage.Company.Company" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.selectedLicenseId = <%= licenseId %>;
        soeConfig.selectedLicenseNr = <%= licenseNr %>;
        soeConfig.selectedLicenseSupport = '<%= licenseSupport %>';
        soeConfig.actorCompanyId = <%= actorCompanyId %>;
        soeConfig.selectedCompanyId = <%= selectedCompanyId %>;
        soeConfig.isAuthorizedForEdit = '<%= isAuthorizedForEdit %>';
        soeConfig.isUserInCompany = '<%= isUserInCompany %>';
    </script>    
</asp:Content>

<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
    
<!-- OLD MARKUP IS FOUND AT THE BOTTOM OF THE .cs FILE -->
</asp:content>



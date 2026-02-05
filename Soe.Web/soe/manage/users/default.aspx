<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.users._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">	
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Manage.User.Users" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.isAdmin =  <%= isAdmin.ToString().ToLower() %>;
        soeConfig.selectedLicenseId = <%= license != null ? license.LicenseId : 0 %>;
        soeConfig.selectedCompanyId = <%= company != null ? company.ActorCompanyId : 0 %>;
        soeConfig.selectedRoleId = <%= role != null ? role.RoleId : 0 %>;
        soeConfig.selectedUserId = <%= user != null ? user.UserId : 0 %>;
        soeConfig.hasValidLicenseToSupportLogin = <%= hasValidLicenseToSupportLogin.ToString().ToLower() %>;
        soeConfig.tabHeader = <%= "'" + tabHeader + "'" %>;
    </script>   
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>

<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.time.attest._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">    
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Time.TimeAttest" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.isAdmin = <%= isAdmin.ToString().ToLower() %>
        soeConfig.employeeId = <%= employee != null ? employee.EmployeeId : 0 %>
        soeConfig.employeeGroupId = <%= employeeGroup != null ? employeeGroup.EmployeeGroupId : 0 %>
        soeConfig.attestMode = <%= (int)mode %>
    </script>    
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>

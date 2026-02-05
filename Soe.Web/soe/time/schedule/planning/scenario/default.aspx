<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.schedule.planning.scenario._default" %>

<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Schedule.Planning" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.planningMode = 'schedule';
        soeConfig.view = 'scenario';
        soeConfig.employeeId = <%= employee != null ? employee.EmployeeId : 0 %>
        soeConfig.employeeGroupId = <%= employee != null ? employee.CurrentEmployeeGroupId : 0 %>
        soeConfig.isAdmin = <%= isAdmin.ToString().ToLower() %>;
        soeConfig.startDate =  <%= startDate.ToString("yyyyMMdd") %>;
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

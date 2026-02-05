<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.schedule.planning.spaschedule._default" %>

<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content3" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.planningMode = 'schedule';
        soeConfig.view = 'schedule';
        soeConfig.employeeId = <%= employee != null ? employee.EmployeeId : 0 %>
            soeConfig.employeeGroupId = <%= employee != null ? employee.CurrentEmployeeGroupId : 0 %>
                soeConfig.isAdmin = <%= isAdmin.ToString().ToLower() %>;
        soeConfig.startDate = <%= startDate.ToString("yyyyMMdd") %>;
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

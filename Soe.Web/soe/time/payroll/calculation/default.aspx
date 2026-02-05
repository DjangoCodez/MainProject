<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.payroll.calculation._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">    
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Payroll.PayrollCalculation" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.isAdmin =  <%= isAdmin.ToString().ToLower() %>;
        soeConfig.employeeId = <%= employee != null ? employee.EmployeeId : 0 %>
    </script>    
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>

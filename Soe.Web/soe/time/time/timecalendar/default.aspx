<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.time.timecalendar._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">    
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Time.TimeCalendar" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.employeeId = <%= employee != null ? employee.EmployeeId : 0 %>
        soeConfig.employeeGroupId = <%= employee != null ? employee.CurrentEmployeeGroupId : 0 %>
    </script>  
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

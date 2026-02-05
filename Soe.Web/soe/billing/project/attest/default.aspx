<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.project.attest._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content3" ContentPlaceHolderID="soeMainContent" runat="server">

    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Time.TimeAttest" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.attestMode = <%= (int)mode %>;
        soeConfig.employeeId = <%= employee != null ? employee.EmployeeId : 0 %>;
    </script>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

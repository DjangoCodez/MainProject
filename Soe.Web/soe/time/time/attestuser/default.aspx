<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.time.attestuser._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <form action="" id="formtitle">
        <div id="DivSubTitle" runat="server">
            <input value="<%=subtitle%>" readonly="readonly"/>        
        </div>        
    </form>
    
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Time.TimeAttest" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.employeeId = <%= employee != null ? employee.EmployeeId : 0 %>
        soeConfig.employeeGroupId = <%= employeeGroupId %>
        soeConfig.attestMode = <%= (int)mode %>
    </script>    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
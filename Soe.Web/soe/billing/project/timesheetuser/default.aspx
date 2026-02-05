<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.project.timesheetuser._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>

<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">
    <form action="" id="formtitle">
        <div id="DivSubTitle" runat="server">
            <input value="<%=subtitle%>" readonly="readonly"/>        
        </div>        
    </form>
    
    <%if (UseAngularSpa) {%>
        <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else {%>
    <SOE:AngularHost ID="AngularHost" ModuleName="Billing" AppName="Soe.Billing.Projects.TimeSheets" runat="server" />
    <%}%>
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.employeeId = <%= employeeId %>;
        soeConfig.feature = <%= (int)Feature %>;
    </script>
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>

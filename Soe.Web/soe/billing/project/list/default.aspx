<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.project.list._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <%if (UseAngularSpa) {%>
         <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else { %>
        <SOE:AngularHost ID="AngularHost" ModuleName="Common, Economy, Billing" AppName="Soe.Billing.Projects" runat="server" />
    <%}%> 
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
                soeConfig.accountYearId = <%= accountYearId %>
                soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
                soeConfig.employeeId = <%= employeeId %>;
        </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
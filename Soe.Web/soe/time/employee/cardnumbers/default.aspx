<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.employee.cardnumbers._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content" ContentPlaceHolderID="soeMainContent" runat="server">
    <%if (UseAngularSpa)
        {%>
    <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%}
        else
        {%>
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Employee.CardNumbers" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
    </script>
    <%}%>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

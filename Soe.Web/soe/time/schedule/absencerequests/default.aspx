<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.schedule.absencerequests._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <%if (UseAngularSpa) {%>
       <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else {%>
        <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Schedule.Absencerequests" runat="server" />
    <%}%> 
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.feature = <%= (int)Feature %>;
        soeConfig.requestType =<%= (int)requestType %>;
        soeConfig.displayMode =<%= (int)displayMode %>;
        soeConfig.employeeRequestId = <%= employeeRequestId %>;
    </script>
</asp:Content>

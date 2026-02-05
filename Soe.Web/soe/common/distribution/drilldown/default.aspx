<%@ Page Language="C#" Trace="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.distribution.drilldown._default" %>
<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.isTemplates = <%= Boolean.FalseString.ToLower() %>
        soeConfig.accountYearId = <%= accountYearId %>
        soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
        soeConfig.accountYearLastDate = '<%= accountYearLastDate %>'
    </script>
    <%if (UseAngularSpa)
      { %>
        <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%}
      else
      {%>
    <SOE:AngularHost ID="AngularHost" ModuleName="Common, Economy" AppName="Soe.Common.Reports.DrilldownReports" runat="server" />
    <%}%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

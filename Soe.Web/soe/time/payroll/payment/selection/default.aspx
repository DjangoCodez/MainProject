<%@ Page ValidateRequest="false" Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.payroll.payment.selection._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">       
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Payroll.Payment" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.companyShortName = '<%= SoeCompany.ShortName %>';
    </script> 
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>

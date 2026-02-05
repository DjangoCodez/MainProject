<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.import.payrollstartvalues._default" %>
<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <a href="Startvärden.xlsx"><%=GetText(10056,"Mall startvärden för lön") %></a><br /><br />
    <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Import.PayrollStartValues" runat="server" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

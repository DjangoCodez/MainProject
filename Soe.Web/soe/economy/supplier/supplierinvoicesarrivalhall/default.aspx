<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" 
	Inherits="SoftOne.Soe.Web.soe.economy.supplier.supplierinvoicesarrivalhall._default" %>

<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">
	<SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>

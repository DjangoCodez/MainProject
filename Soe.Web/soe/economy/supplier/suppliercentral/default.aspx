<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.supplier.suppliercentral._default" Title="Untitled Page" %>

<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <%if (UseAngularSpa)
        {%>
    <soe:angularspahost id="AngularSpaHost" runat="server" />
    <%} else {%>
    <soe:angularhost id="AngularHost" modulename="Common, Economy" appname="Soe.Economy.Supplier.SupplierCentral" runat="server" />
    <%}%>
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
            soeConfig.supplierId = <%= supplierId %>
            soeConfig.accountYearId = <%= accountYearId %>
            soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

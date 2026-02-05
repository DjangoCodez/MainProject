<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.export.payments._default" %>
<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
        <SOE:AngularHost ID="AngularHost" ModuleName="Economy" AppName="Soe.Economy.Export.Payments" runat="server" />
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
            soeConfig.accountYearId = <%= accountYearId %>;
            soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>;
            soeConfig.exportType = <%= exportType %>;
        </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.product.products._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
     <%if (UseAngularSpa) {%>
     <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else {%>
        <SOE:AngularHost ID="AngularHost" ModuleName="Billing" AppName="Soe.Billing.Products.Products" runat="server" />
    <%}%>
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
                soeConfig.productId = <%= productId %>;
                soeConfig.productNr = '<%= productNr %>';
        </script>  
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

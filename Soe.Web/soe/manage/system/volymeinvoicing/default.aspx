<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.volymeinvoicing._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
        <SOE:AngularHost ID="AngularHost" ModuleName="Billing" AppName="Soe.Manage.System.VolymInvoicing" runat="server" />
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
        </script>
</asp:Content>
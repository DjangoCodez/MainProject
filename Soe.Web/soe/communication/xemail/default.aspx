<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.communication.xemail._default" %>
<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">        
<SOE:AngularHost ID="AngularHost" ModuleName="Common, Core" AppName="Soe.Common.Messages" runat="server" />
<script type="text/javascript">
    if (!soeConfig)
        soeConfig = {};
        soeConfig.module = <%= (int)TargetSoeModule %>
        soeConfig.feature = <%= (int)FeatureEdit %>

    </script>   
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>

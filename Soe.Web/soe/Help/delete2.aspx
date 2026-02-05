<%@ Page Language="C#"AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.Help._default" %>
<%--<%@ OutputCache CacheProfile="silverlight" %>--%><%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/SilverlightControlHost.ascx" TagPrefix="SOE" TagName="SilverlightControlHost" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>   
</head>
<body>
    <form id="form1" runat="server">
        <SOE:SilverlightControlHost id="SLHost" runat="server" />    
    </form>
</body>
</html>

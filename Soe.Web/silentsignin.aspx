<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="silentsignin.aspx.cs" Inherits="SoftOne.Soe.Web.silentsignin" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <script src="/cssjs/oidc/oidc-client.min.js?cs=2021"></script>
    <script>
        new Oidc.UserManager().signinSilentCallback();
    </script>
</body>
</html>

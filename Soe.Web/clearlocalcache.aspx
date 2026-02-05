<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="clearlocalcache.aspx.cs" Inherits="SoftOne.Soe.Web.clearlocalcache" %>

<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">

<script type="text/javascript">
    window.onload = function () {
        var keys = [];

        $.each(localStorage, function (key, val) {
            keys.push(key);
        });

        for (i = 0; i < keys.length; i++) {
            var key = keys[i];
            if (!key.includes('#Core/SysTermGroup') && !key.includes('#term.')) {
                localStorage.removeItem(key);
            }
        }

        window.history.back();
    }
</script>
</asp:content>

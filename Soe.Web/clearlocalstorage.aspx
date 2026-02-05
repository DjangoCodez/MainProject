<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="clearlocalstorage.aspx.cs" Inherits="SoftOne.Soe.Web.clearlocalstorage" %>

<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">

<script type="text/javascript">
    window.onload = function() {
        window.localStorage.clear();
        window.history.back();
        window.indexedDB.databases().then((r) => {
            for (var i = 0; i < r.length; i++) window.indexedDB.deleteDatabase(r[i].name);
        }).then(() => {
            console.log("IndexedDB cleared.")
        });
    }
</script>
</asp:content>

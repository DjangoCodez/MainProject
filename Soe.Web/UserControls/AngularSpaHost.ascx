<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AngularSpaHost.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.AngularSpaHost" %>

<script type="text/javascript">
    window.softOneSpa = {
        isChromeless: true,
        feature: <%= (int)PageBase.Feature %>,
        module: <%= (int)PageBase.SoeModule %>
    }
</script>

<soe-root></soe-root>
<script src="/angular/dist/spa/browser/polyfills<%=this.PolyfillsHash %>js" type="module"></script>
<script src="/angular/dist/spa/browser/main<%=this.MainHash %>js" type="module"></script>
<link rel="stylesheet" href="/angular/dist/spa/browser/styles<%=this.StylesHash %>css">

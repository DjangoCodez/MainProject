<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AngularHost.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.AngularHost" %>
<div id="ng-app-bootstrap-element" data-soe-app="<%=this.AppName %>" style="height: auto; position: relative; overflow: hidden;">
    <%if (!IsLegacy()) { %>
        <information-menu position-index="1"></information-menu>
        <help-menu position-index="2" feature="<%=(int)PageBase.Feature %>" soe-module="<%=(int)PageBase.SoeModule %>"></help-menu>
        <academy-menu position-index="3"></academy-menu>
        <% if (ShowMessages) { %>
            <message-menu position-index="4"></message-menu>
            <report-menu position-index="5" feature="<%=(int)PageBase.Feature %>" soe-module="<%=(int)PageBase.SoeModule %>"></report-menu>
            <document-menu position-index="6" feature="<%=(int)PageBase.Feature %>"></document-menu>
        <% } else { %>
            <report-menu position-index="4" feature="<%=(int)PageBase.Feature %>" soe-module="<%=(int)PageBase.SoeModule %>"></report-menu>
            <document-menu position-index="5" feature="<%=(int)PageBase.Feature %>"></document-menu>
        <% } %>
    <% } %>
    <div style="width: auto; overflow: hidden;" data-ui-view></div>
</div>

<%
    int languageid = this.PageBase.GetLanguageId();
    string localeFile = "/Scripts/angular/angular-locale_sv-se.js";
    string momentLocaleFile = "/Scripts/angular/moment/sv.js";
    if (languageid == 2)
    {
        localeFile = "/Scripts/angular/angular-locale_en-us.js";
        momentLocaleFile = null;
    }
    else if (languageid == 3)
    {
        localeFile = "/Scripts/angular/angular-locale_fi-fi.js";
        momentLocaleFile = "/Scripts/angular/moment/fi.js";
    }
    else if (languageid == 4)
    {
        localeFile = "/Scripts/angular/angular-locale_nb-no.js";
        momentLocaleFile = "/Scripts/angular/moment/nb.js";
    }
    else if (languageid == 5)
    {
        localeFile = "/Scripts/angular/angular-locale_da-dk.js";
        momentLocaleFile = "/Scripts/angular/moment/da.js";
    }

    string versionNr = SoftOne.Soe.Web.AngularConfig.TermVersionNr;
    string prefix = SoftOne.Soe.Web.AngularConfig.Prefix;
    var suffix = SoftOne.Soe.Web.AngularConfig.UseMinified ? ".min" : SoftOne.Soe.Web.AngularConfig.UseBundle ? ".bundle" : "";
    var includeCachebusting = SoftOne.Soe.Web.AngularConfig.UseCacheBusting;

    SoftOne.Soe.Business.Core.GeneralManager gm = new SoftOne.Soe.Business.Core.GeneralManager(null);
    DateTime modifiedDate = gm.GetAssemblyDate();

    string v = string.Format("{0}_{1}", versionNr, modifiedDate.ToString("yyyyMMdd_HHmmss"));
%>
<script type="text/javascript">
    if (/MSIE \d|Trident.*rv:/.test(navigator.userAgent))
        document.write('<script src="/cssjs/bluebird.min.js"><\/script>');
</script>
<script src="/cssjs/system.js"></script>
<% if (includeCachebusting)
    { %>
    <script src="<%= prefix %>checkSums.js?cs=<%= v %>"></script>
    <script src="/cssjs/systemjs.cachebusting.js"></script>
<% } %>
<script src="/cssjs/store.min.js"></script>
<script src="/cssjs/soe-ie-pollyfill.js"></script>
<script src="/cssjs/systemjs.config.js?cs=<%= v %>"></script>
<script>
    if (window.checksums)
        System.enableCacheBusting(window.checksums);

    var app = document.getElementById("ng-app-bootstrap-element").getAttribute("data-soe-app");
    var path = app.substr(4);

    window.bootSoftOne(app, path, '<%= prefix %>', '<%= suffix %>', '<%= localeFile %>', '<%= momentLocaleFile == null ? "" : momentLocaleFile %>', '<%= v %>')
</script>


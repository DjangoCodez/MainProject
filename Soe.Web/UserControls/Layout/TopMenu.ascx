<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopMenu.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.Layout.TopMenu" %>
<%@ Register TagName="TopMenuSelector" TagPrefix="SOE" Src="~/UserControls/Layout/TopMenuSelector.ascx" %>
<%@ Register TagName="TopMenuCompanySelector" TagPrefix="SOE" Src="~/UserControls/Layout/TopMenuCompanySelector.ascx" %>

<script type="text/javascript">
    function RemoveFavorite(userFavoriteId) {
        var url = '/ajax/removeFavorite.aspx?userFavoriteId=' + userFavoriteId;
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj && obj.Success == true) {
                // Reload page
                location.reload(true);
            }
        });
    }
    function SetAccountYear(accountYearId) {
        var url = '/ajax/setAccountYear.aspx?accountYearId=' + accountYearId + '&timestamp=' + new Date().getTime(); //make each request unique to prevent cache
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj && obj.Success == true) {
                // Reload page
                location.reload(true);
            }
        });
    }
    function SetAccountHierarchy(hierarchyId) {
        var url = '/ajax/setAccountHierarchy.aspx?hierarchyId=' + hierarchyId + '&timestamp=' + new Date().getTime(); //make each request unique to prevent cache
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj && obj.Success == true) {
                // Reload page
                location.reload(true);
            }
        });
    }
    function openRightMenu(message) {
        var channel = postal.channel();
        channel.publish(message, {});
    }
</script>

<form class="form-inline top-menu <%=PageBase.BrandingCompanyClass%>">
    <%-- TODO: Remove class margin-small-left when old bootstrap is removed --%>
    <div class="form-group pull-left margin-small-left ms-3">
        <%if (PageBase.ShowLogoInTopMenu)
            { %>
        <a class="go-logo" href="/soe/?c=<%=PageBase.SoeActorCompanyId.ToString()%>">
            <img src="<%=PageBase.BrandingCompanyLogo%>" />
        </a>
        <% } %>
        <%=""%>
        <%-- Page status --%>
        <%if (PageBase.ShowPageStatusBetaSelector)
            { %>
        <a>Test/REF:</a>
        <SOE:TopMenuSelector ID="PageStatusBetaSelector" Type="PageStatusBeta" runat="Server"></SOE:TopMenuSelector>
        <% } %>

        <%if (PageBase.ShowPageStatusLiveSelector)
            { %>
        <a>Skarpt:</a>
        <SOE:TopMenuSelector ID="PageStatusLiveSelector" Type="PageStatusLive" runat="Server"></SOE:TopMenuSelector>
        <% } %>

        <%if (PageBase.ShowPageStatusBetaSelector || PageBase.ShowPageStatusLiveSelector)
            { %>
        <div class="separator"></div>
        <% } %>

        <%-- Template Company --%>
        <%if (ShowTemplateCompany)
            { %>
        <a style="font-size: 22px; margin-top: 13px;"><%=TemplateCompanyLabel%></a>
        <div class="separator"></div>
        <% } %>

        <%-- Support login --%>
        <%if (PageBase.SoeSupportCompany != null && PageBase.SoeSupportUserId != null)
            { %>
        <a title="<%=PageBase.Enc(PageBase.GetText(3221, "Supportinloggad"))%>" style="margin-left: 6px; margin-right: 6px;">
            <span class="fad fa-life-ring"></span>
        </a>
        <a href="/soe/manage/companies/edit/remote/?logout=1" title="<%=PageBase.Enc(PageBase.GetText(18, "Logga ut"))%>" style="margin-right: 6px;">
            <span class="fal fa-sign-out"></span>
        </a>
        <div class="separator"></div>
        <% } %>
    </div>

    <div class="form-group pull-right">
        <div class="space"></div>
        <div class="separator"></div>
        <%-- The separator makes the dropdown looks ok, otherwise they overlap the black area.. --%>

        <%-- Page version --%>
        <%if (PageBase.UseAngularSpa && PageBase.HasAngularHost && PageBase.HasAngularSpaHost && PageBase.CanUseAngularJs)
            { %>
        <a href="<%=PageBase.AddUrlParameter("", "ng", Boolean.TrueString, false, new string[] { "spa" })%>" title="<%=PageBase.Enc(PageBase.GetText(12151, "Kör gamla versionen av sidan")) %>">
            <span class="fab fa-js-square"></span>
        </a>
        <% } %>
        <%else if (!PageBase.UseAngularSpa && PageBase.ShowAngularSpaIcon && PageBase.HasAngularHost && !PageBase.AngularJsFirst)
            { %>
        <a href="<%=PageBase.AddUrlParameter("", "ng", Boolean.FalseString, false, new string[] { "spa" })%>" title="<%=PageBase.Enc(PageBase.GetText(12152, "Kör nya versionen av sidan"))%>">
            <span class="fab fa-angular"></span>
        </a>
        <% } %>
        <%else if (!PageBase.UseAngularSpa && PageBase.ShowAngularSpaIcon && PageBase.HasAngularHost && PageBase.AngularJsFirst)
            { %>
        <a href="<%=PageBase.AddUrlParameter("", "spa", Boolean.TrueString, false, new string[] { "ng" })%>" title="<%=PageBase.Enc(PageBase.GetText(12152, "Kör nya versionen av sidan"))%>">
            <span class="fab fa-angular"></span>
        </a>
        <% } %>

        <%-- AccountYear --%>
        <% if (Module == SoftOne.Soe.Common.Util.Constants.SOE_MODULE_ECONOMY || Module == SoftOne.Soe.Common.Util.Constants.SOE_MODULE_BILLING)
            { %>
        <SOE:TopMenuSelector ID="AccountYearSelector" Type="AccountYear" runat="Server"></SOE:TopMenuSelector>
        <div class="space"></div>
        <%}%>

        <%-- AccountHierachy --%>
        <% if (PageBase.UseAccountHierarchy())
            { %>
        <SOE:TopMenuSelector ID="AccountHierarchySelector" Type="AccountHierarchy" runat="Server"></SOE:TopMenuSelector>
        <div class="space"></div>
        <%}%>

        <%-- Company --%>
        <SOE:TopMenuCompanySelector ID="CompanySelector" runat="Server"></SOE:TopMenuCompanySelector>
        <div class="space"></div>

        <%-- Role --%>
        <SOE:TopMenuSelector ID="RoleSelector" Type="Role" runat="Server"></SOE:TopMenuSelector>
        <div class="space"></div>

        <%-- User name --%>
        <SOE:TopMenuSelector ID="UserSelector" Type="User" runat="Server"></SOE:TopMenuSelector>
        <div class="space"></div>

        <%-- Favorites --%>
        <SOE:TopMenuSelector ID="FavoritesSelector" Type="Favorites" runat="Server"></SOE:TopMenuSelector>
        <div class="space"></div>

        <%--
        <a style="cursor: pointer; margin-right: 10px;">
            <span class="fal fa-info" onclick="openRightMenu('toggleInformationMenu')"></span>
        </a>
        <a style="cursor: pointer; margin-right: 10px;">
            <span class="fal fa-question" onclick="openRightMenu('toggleHelpMenu')"></span>
        </a>
        <a style="cursor: pointer; margin-right: 10px;">
            <span class="fal fa-graduation-cap" onclick="openRightMenu('showAcademyMenu')"></span>
        </a>
        <a style="cursor: pointer; margin-right: 10px;">
            <span class="fal fa-envelope" onclick="openRightMenu('toggleMessageMenu')"></span>
        </a>
        <a style="cursor: pointer; margin-right: 10px;">
            <span class="fal fa-file-medical-alt" onclick="openRightMenu('toggleReportMenu')"></span>
        </a>
        <a style="cursor: pointer; margin-right: 10px;">
            <span class="fal fa-file-alt" onclick="openRightMenu('toggleDocumentMenu')"></span>
        </a>
        --%>

        <%-- Logout --%>
        <%-- TODO: Remove class margin-small-right when old bootstrap is removed --%>
        <a href="/logout.aspx" title="<%=PageBase.Enc(PageBase.GetText(18, "Logga ut"))%>" class="margin-small-right me-3">
            <span class="fal fa-sign-out"></span>
        </a>
    </div>
</form>

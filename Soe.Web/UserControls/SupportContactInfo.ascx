<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SupportContactInfo.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.SupportContactInfo" %>
<div class="supportcontactinfo">
    <span><%= PageBase.GetText(5476, "Kontakta support")%></span>
    <ul>
        <li>
            <span><%=PageBase.GetText(5477, "E-post") + ": "%>
                <a href= mailto:<%=PageBase.GetText(4618,"support@softone.se")%>><%=PageBase.GetText(4618,"support@softone.se") %></a>
            </span>
        </li>
        <li>
            <span><%=PageBase.GetText(5478, "Telefon") + ": "%>
                <%=PageBase.GetText(4619,"0771 - 55 69 00") %>
            </span>
        </li>
    </ul>
</div>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="JavascriptDisabled.aspx.cs" Inherits="SoftOne.Soe.Web.errors.JavascriptDisabled" %>
<%@ Register Src="~/UserControls/SupportContactInfo.ascx" TagPrefix="SOE" TagName="SupportContactInfo" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div class="error">
        <h1><%=GetText(5482, "Javacript krävs för att köra Softone")%></h1>        
        <p class="description">
            <%=GetText(5483, "Den webbläsaren du använder saknar stöd för JavaScript, eller också tillåts inte skript. Information om stöd för JavaScript och om hur du tillåter skript finns i webbläsarens hjälp")%>
        </p>    
        <ul id="ArrangementList" runat="server">
            <li>
                <SOE:SupportContactInfo ID="SupportContactInfo" Runat="Server"></SOE:SupportContactInfo>
            </li>
        </ul>
    </div>
</asp:Content>

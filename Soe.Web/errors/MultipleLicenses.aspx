<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MultipleLicenses.aspx.cs" Inherits="SoftOne.Soe.Web.errors.MultipleLicenses" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div class="error">
        <h1><%= String.Format(GetText(11686, "Du är inte längre inloggad på företag '{0}' på licens '{1}'"), this.PrevCompanyName, this.PrevLicenseName)%></h1>        
        <p class="description">
            <%= String.Format(GetText(11687, "Sen du använde den här fliken senast så har du loggat in på företag '{0}' på licens '{1}' i annan flik"), this.CurrentCompanyName, this.CurrentLicenseName)%>
            <br /><br /><a href="<%# this.RedirectUrl%>"><%= String.Format(GetText(11688, "Klicka här för att fortsätta jobba med det inloggade företaget '{0}'"), this.CurrentCompanyName)%></a>
        </p>    
    </div>
</asp:Content>

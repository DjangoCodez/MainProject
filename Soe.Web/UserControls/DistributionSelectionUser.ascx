<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DistributionSelectionUser.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.DistributionSelectionUser" %>
 <fieldset>
    <legend><%=PageBase.GetText(1720, "Användareurval")%></legend>
    <table>
        <SOE:SelectEntry
            runat="server" 
            ID="User" 
            TermID="1719" DefaultTerm="Anv.namn">
        </SOE:SelectEntry>
    </table>
</fieldset>
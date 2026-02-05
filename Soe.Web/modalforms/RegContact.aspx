<%@ Page Language="C#" MasterPageFile="ModalForm.Master" AutoEventWireup="true" CodeBehind="RegContact.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.RegContact" %>
<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <SOE:SelectEntry 
        ID="Type"
        TermID="1310" DefaultTerm="Kontakttyp"
        runat="server">
    </SOE:SelectEntry>
    <SOE:TextEntry 
        ID="Text"
        TermID="1311" DefaultTerm="Text"
        Validation="Required"
        InvalidAlertTermID="1312" InvalidAlertDefaultTerm="Du måste ange text"
        MaxLength="50"
        runat="server">
    </SOE:TextEntry>	
</asp:Content>

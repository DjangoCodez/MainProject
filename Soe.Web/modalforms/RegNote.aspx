<%@ Page Language="C#" MasterPageFile="ModalForm.Master" AutoEventWireup="true" CodeBehind="RegNote.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.RegNote" %>    
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>
<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <SOE:TextEntry
        ID="RegTitle"
        TermID="4078"
        DefaultTerm="Titel"
        MaxLength="100"
        runat="server">
    </SOE:TextEntry>
    <SOE:TextEntry
        ID="RegComment" 
        TermID="4079"
        DefaultTerm="Anteckning"
        MaxLength="200"
        runat="server">
    </SOE:TextEntry>        
</asp:Content>

<%@ Page Language="C#" MasterPageFile="ModalForm.Master" AutoEventWireup="true" CodeBehind="RegAddress.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.RegAddress" %>
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>
<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <SOE:SelectEntry 
        ID="AddressType"
        TermID="3383" DefaultTerm="Adresstyp"
        runat="server">
    </SOE:SelectEntry>
</asp:Content>

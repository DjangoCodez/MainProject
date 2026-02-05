<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="ModalForm.Master" CodeBehind="CreateAccountYear.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.CreateAccountYear" %>
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>
<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <table>		
        <SOE:Instruction 
            ID="Message"
            Visible="false"
            runat="server">
        </SOE:Instruction> 
    </table>
</asp:Content>
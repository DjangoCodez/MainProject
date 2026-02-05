<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="ModalForm.Master" CodeBehind="AccountYearSelector.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.AccountYearSelector" %>
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>
<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <table>
        <SOE:Instruction 
            ID="Message"
            Visible="false"
            runat="server">
        </SOE:Instruction>
    </table>
    <br />
    <table>
        <SOE:SelectEntry
	        ID="AccountYear"
	        DisableSettings="true"
            Width="250"
	        TermID="5467" DefaultTerm="Redovisningsår"
	        runat="server">
        </SOE:SelectEntry>
    </table>
</asp:Content>
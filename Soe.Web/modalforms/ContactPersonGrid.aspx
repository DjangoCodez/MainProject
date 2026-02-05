<%@ Page Language="C#" MasterPageFile="ModalForm.Master" AutoEventWireup="true" CodeBehind="ContactPersonGrid.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.ContactPersonGrid" %>
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>

<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <table>
        <tr>
            <td>			
                <fieldset>
	                <table class="ModalTable" id="TableContactInfo" runat="server"></table>
                </fieldset>    
            </td>
        </tr>
    </table>
</asp:Content>
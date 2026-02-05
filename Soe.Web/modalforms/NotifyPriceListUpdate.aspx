<%@ Page Language="C#" MasterPageFile="ModalForm.Master" AutoEventWireup="true" CodeBehind="NotifyPriceListUpdate.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.NotifyPriceListUpdate" %>    
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>
<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <table>
        <tr>
            <td>			
                <fieldset>
	                <table class="ModalTable" id="TableUpdatePricelist" runat="server"></table>
                </fieldset>    
            </td>
        </tr>
    </table>
</asp:Content>

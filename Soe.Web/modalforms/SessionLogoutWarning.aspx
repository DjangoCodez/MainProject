<%@ Page Language="C#" Async="true" MasterPageFile="ModalForm.Master" AutoEventWireup="true" CodeBehind="SessionLogoutWarning.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.SessionLogoutWarning" %>    
<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <tr>
        <td>
            <div id="sessionLogoutWarning" style="text-align:left">
                <span><%= this.timeoutTime %></span>
            </div>
        </td>
    </tr>
</asp:Content>

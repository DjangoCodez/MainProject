<%@ Page Trace="false" MasterPageFile="~/base.master" Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="SoftOne.Soe.Web.Login" %>

<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="baseMasterBodyContent" ContentPlaceHolderID="baseMasterBody" runat="server">
    <div class="modal fade bs-example-modal-sm" id="myPleaseWait" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static">
        <div class="modal-dialog modal-md">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">
                        <%=GetText(5470, "Loggar in i SoftOne GO...")%>
                    </h4>
                </div>
                <div class="modal-body">
                    <div class="progress">
                        <div class="progress-bar progress-bar-warning progress-bar-striped active" role="progressbar" style="width: 100%"></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script>
        window.onload = function () {
            $('#myPleaseWait').modal('show');
            console.log("Waiting for sign in");
        };
    </script>
</asp:Content>

<%@ Page Language="C#" MasterPageFile="ModalForm.Master" AutoEventWireup="true" CodeBehind="SetAccount.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.SetAccount" %>    
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>

<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <fieldset>
        <div style="width:320px">
            <b><%=SelectAnAccountText %></b> <br />
            <div style="height:10px">
                <SOE:TextEntry
                    ID="AccountEntry"
                    TermID="9015"
                    DefaultTerm="Kontoplan"
                    OnChange="accountSearch.searchField('AccountEntry')"
                    OnKeyUp="accountSearch.keydown('AccountEntry')"
                    Width="40"
                    runat="server">
                </SOE:TextEntry>
            </div>
            &nbsp
            <span id="accountNr"></span>
            <%--<asp:Label ID="AccountNr" runat="server" Text="" style="margin-left:5px" />--%>
            <br />
            <SOE:TextEntry
                ID="AmountDiff"
                TermID="9016"
                DefaultTerm="Belopp"
                MaxLength="100"
                ReadOnly= "true" 
                runat="server">
            </SOE:TextEntry>
        </div>
    </fieldset>

    <div style="max-width: 200px">
        <div class="searchTemplate">
	        <div id="searchContainer" class="searchContainer"></div>    
            <div id="accountSearchItem_$accountNr$">
                <div id="account_$id$" class="item" onmouseover="searchComponent.select();" onclick="searchComponent.choose();">
                    <div class="id" id="extendNumWidth_$id$">$accountNr$</div>
                    <div class="name" id="extendNameWidth_$id$">$accountName$</div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
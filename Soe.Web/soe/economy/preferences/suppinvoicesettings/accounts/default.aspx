<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.suppinvoicesettings.accounts._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">

      <script type="text/javascript" language="javascript">
       var stdDimID = "<%=stdDimID%>";
       var companyID = "<%=SoeCompany.ActorCompanyId%>";
    </script>

    <SOE:Form ID="Form1" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="3223" DefaultTerm="Baskonton leverantörsreskontra" runat="server">
				<div>
                    <fieldset>
                        <legend><%=GetText(3126, "Leverantörsreskontra")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="AccountSupplierDebt" 
                                TermID="3141" 
                                DefaultTerm="Leverantörsskuld" 
                                OnChange="accountSearch.searchField('AccountSupplierDebt')"
                                OnKeyUp="accountSearch.keydown('AccountSupplierDebt')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountSupplierPurchase" 
                                TermID="3140" 
                                DefaultTerm="Inköp"
                                OnChange="accountSearch.searchField('AccountSupplierPurchase')"
                                OnKeyUp="accountSearch.keydown('AccountSupplierPurchase')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountSupplierInterim" 
                                TermID="3142" 
                                DefaultTerm="Interimskonto"
                                OnChange="accountSearch.searchField('AccountSupplierInterim')"
                                OnKeyUp="accountSearch.keydown('AccountSupplierInterim')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountSupplierUnderpay" 
                                TermID="3143" 
                                DefaultTerm="Underbetalning" 
                                OnChange="accountSearch.searchField('AccountSupplierUnderpay')"
                                OnKeyUp="accountSearch.keydown('AccountSupplierUnderpay')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountSupplierOverpay" 
                                TermID="3144" 
                                DefaultTerm="Överbetalning"
                                OnChange="accountSearch.searchField('AccountSupplierOverpay')"
                                OnKeyUp="accountSearch.keydown('AccountSupplierOverpay')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>
    
    <div class="searchTemplate">
	    <div id="searchContainer" class="searchContainer"></div>    
        <div id="accountSearchItem_$accountNr$">
            <div id="account_$id$" class="item" onmouseover="searchComponent.select();" onclick="searchComponent.choose();">
                <div class="id" id="extendNumWidth_$id$">$accountNr$</div>
                <div class="name" id="extendNameWidth_$id$">$accountName$</div>
            </div>
        </div>
    </div>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

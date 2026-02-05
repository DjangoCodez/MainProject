<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.inventorysettings.accounts._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="3489" DefaultTerm="Baskonton inventarier" runat="server">
				<div>
                    <fieldset>
                        <legend><%=GetText(3476, "Inventarier")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="AccountInventoryInventories" 
                                TermID="3490" 
                                DefaultTerm="Inventarier"
                                OnChange="accountSearch.searchField('AccountInventoryInventories')"
                                OnKeyUp="accountSearch.keydown('AccountInventoryInventories')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventoryAccWriteOff" 
                                TermID="3491" 
                                DefaultTerm="Ackumulerad avskrivning"
                                OnChange="accountSearch.searchField('AccountInventoryAccWriteOff')"
                                OnKeyUp="accountSearch.keydown('AccountInventoryAccWriteOff')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventoryWriteOff" 
                                TermID="3492" 
                                DefaultTerm="Avskrivning"
                                OnChange="accountSearch.searchField('AccountInventoryWriteOff')"
                                OnKeyUp="accountSearch.keydown('AccountInventoryWriteOff')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventoryAccOverWriteOff" 
                                TermID="3493" 
                                DefaultTerm="Ackumulerad överavskrivning"
                                OnChange="accountSearch.searchField('AccountInventoryAccOverWriteOff')"
                                OnKeyUp="accountSearch.keydown('AccountInventoryAccOverWriteOff')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventoryOverWriteOff" 
                                TermID="3494" 
                                DefaultTerm="Överavskrivning"
                                OnChange="accountSearch.searchField('AccountInventoryOverWriteOff')"
                                OnKeyUp="accountSearch.keydown('AccountInventoryOverWriteOff')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventoryAccWriteDown" 
                                TermID="3497" 
                                DefaultTerm="Ackumulerad nedskrivning" 
                                OnChange="accountSearch.searchField('AccountInventoryAccWriteDown')"
                                OnKeyUp="accountSearch.keydown('AccountInventoryAccWriteDown')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventoryWriteDown" 
                                TermID="3498" 
                                DefaultTerm="Nedskrivning" 
                                OnChange="accountSearch.searchField('AccountInventoryWriteDown')"
                                OnKeyUp="accountSearch.keydown('AccountInventoryWriteDown')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventoryAccWriteUp" 
                                TermID="3495" 
                                DefaultTerm="Ackumulerad uppskrivning" 
                                OnChange="accountSearch.searchField('AccountInventoryAccWriteUp')"
                                OnKeyUp="accountSearch.keydown('AccountInventoryAccWriteUp')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventoryWriteUp" 
                                TermID="3496" 
                                DefaultTerm="Uppskrivning"
                                OnChange="accountSearch.searchField('AccountInventoryWriteUp')"
                                OnKeyUp="accountSearch.keydown('AccountInventoryWriteUp')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventorySales" 
                                TermID="4860" 
                                DefaultTerm="Likvidkonto vid försäljning"
                                OnChange="accountSearch.searchField('AccountInventorySales')"
                                OnKeyUp="accountSearch.keydown('AccountInventorySales')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventorySalesProfit" 
                                TermID="3499" 
                                DefaultTerm="Vinst avyttring inventarier"
                                OnChange="accountSearch.searchField('AccountInventorySalesProfit')"
                                OnKeyUp="accountSearch.keydown('AccountInventorySalesProfit')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountInventorySalesLoss" 
                                TermID="3500" 
                                DefaultTerm="Förlust avyttring inventarier"
                                OnChange="accountSearch.searchField('AccountInventorySalesLoss')"
                                OnKeyUp="accountSearch.keydown('AccountInventorySalesLoss')"
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

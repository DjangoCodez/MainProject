<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.vouchersettings.matchsettings.edit._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnableDelete="true" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
				<div>
                    <fieldset>
                        <legend><%=GetText(9178, "Restkod")%></legend>
                        <table>
	                        <SOE:TextEntry
	                            ID="Name"
	                            TermID="1793" 
                                DefaultTerm="Namn"
						        Validation="Required"
						        InvalidAlertTermID="1794" 
						        InvalidAlertDefaultTerm="Du måste ange namn"
	                            runat="server">
	                        </SOE:TextEntry>
                            <SOE:TextEntry
	                            ID="Description"
	                            TermID="1695" 
                                DefaultTerm="Beskrivning"
	                            runat="server">
	                        </SOE:TextEntry>
                             <SOE:SelectEntry 
                                ID="MatchCodeType"
                                TermID="1873" 
                                DefaultTerm="Typ"
                                runat="server"
                                DataValueField="SysTermId"
                                DataTextField="Name">
                            </SOE:SelectEntry>
                            <SOE:TextEntry 
                                ID="MatchCodeAccount" 
                                TermID="1258" 
                                DefaultTerm="Konto"
                                OnChange="accountSearch.searchField('MatchCodeAccount')"
                                OnKeyUp="accountSearch.keydown('MatchCodeAccount')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="MatchCodeVatAccount" 
                                TermID="7129" 
                                DefaultTerm="Momskonto"
                                OnChange="accountSearch.searchField('MatchCodeVatAccount')"
                                OnKeyUp="accountSearch.keydown('MatchCodeVatAccount')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>		 
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
        </Tabs>
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

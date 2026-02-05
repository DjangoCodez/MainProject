<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.preferences.productsettings.accounts._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    
     <script type="text/javascript" language="javascript">
       var stdDimID = "<%=stdDimID%>";
       var companyID = "<%=SoeCompany.ActorCompanyId%>";
       var defaultCreditNr = '';
       var defaultDebitNr = '';
       var defaultDebitVatFreeNr = '';
    </script>

<SOE:Form ID="Form1" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="4085" DefaultTerm="Baskonton artiklar" runat="server">
                <div id="DivAccounts" runat="server">
 			        <div>
                        <fieldset>
                            <legend><%=GetText(1884, "Kundfordran")%></legend>
                            <table id="PurchaseAccountTable" runat="server">
                            </table>
                        </fieldset>
			        </div>
			        <div>
                        <fieldset>
                            <legend><%=GetText(4086, "Försäljning")%></legend>
                            <table id="SalesAccountTable" runat="server">
                            </table>
                        </fieldset>
			        </div>
			        <div>
                        <fieldset>
                            <legend><%=GetText(1929, "Momsfri försäljning")%></legend>
                            <table id="SalesVatFreeAccountTable" runat="server">
                            </table>
                        </fieldset>
			        </div>
                    <div>
                        <fieldset>
                            <legend><%=GetText(4626, "Lager")%></legend>
                            <table id="StockAccountTable" runat="server">
                            </table>
                        </fieldset>
			        </div>
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
            <div class="C" />
        </div>
    </div>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

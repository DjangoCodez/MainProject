<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.preferences.timesettings.accounts._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    
    <script type="text/javascript" language="javascript">
       var stdDimID = "<%=stdDimID%>";
       var companyID = "<%=SoeCompany.ActorCompanyId%>";
    </script>

    <SOE:Form ID="Form1" runat="server">
        <tabs>
		    <SOE:Tab Type="Setting" TermID="5202" DefaultTerm="Baskonton tid" runat="server">
			    <div>
                    <fieldset>
                        <legend><%=GetText(5203, "Generella")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="EmployeeGroupCost" 
                                TermID="5204" 
                                DefaultTerm="Kostnad"
                                OnChange="accountSearch.searchField('EmployeeGroupCost')"
                                OnKeyUp="accountSearch.keydown('EmployeeGroupCost')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="EmployeeGroupIncome" 
                                TermID="5205" 
                                DefaultTerm="Intäkt"
                                OnChange="accountSearch.searchField('EmployeeGroupIncome')"
                                OnKeyUp="accountSearch.keydown('EmployeeGroupIncome')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                         </table>
                   </fieldset>
                </div>
                <div id="DivPayroll" runat="server">
                    <fieldset>
                        <legend><%=GetText(3108, "Lön")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="EmploymentTax" 
                                TermID="3109" 
                                DefaultTerm="Arbetsgivaravgift"
                                OnChange="accountSearch.searchField('EmploymentTax')"
                                OnKeyUp="accountSearch.keydown('EmploymentTax')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="PayrollTax" 
                                TermID="3110" 
                                DefaultTerm="Egenavgift"
                                OnChange="accountSearch.searchField('PayrollTax')"
                                OnKeyUp="accountSearch.keydown('PayrollTax')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="OwnSupplementCharge" 
                                TermID="3111" 
                                DefaultTerm="Egna påslag"
                                OnChange="accountSearch.searchField('OwnSupplementCharge')"
                                OnKeyUp="accountSearch.keydown('OwnSupplementCharge')"
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

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.preferences.edisettings._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="5431" DefaultTerm="EDI-inställningar" runat="server">
			    <div>
				    <fieldset> 
					    <legend><%=GetText(5356, "EDI")%></legend>
					    <table>
                            <SOE:CheckBoxEntry
                                ID="EdiTransferToInvoice" 
                                TermID="5358"
                                DefaultTerm="Överför EDI automatiskt till leverantörsfaktura" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="BillingUseEDIPriceForSalesPriceRecalculation" 
                                TermID="7774"
                                DefaultTerm="Använd EDI inköpspris vid omräkning av försäljningspris" 
                                runat="server">
                            </SOE:CheckBoxEntry>                      
                            <SOE:SelectEntry
                                ID="CloseEdiEntryCondition"
                                TermID="8171" DefaultTerm="Stäng EDI-post när posten har överförts till"
                                runat="server">
                            </SOE:SelectEntry>                           
		                </table>
		            </fieldset>
                    <fieldset> 
                        <legend><%=GetText(9004, "Automatisk överföring av EDI till order")%></legend>
                        <table>
                             <SOE:CheckBoxEntry
                                ID="EdiTransferCreditInvToOrder" 
                                TermID="7616"
                                DefaultTerm="Överför kreditfakturor till order" 
                                runat="server">
                            </SOE:CheckBoxEntry>           
                            <tr>
                                <th width="200px"><label style="padding-left:5px"><%=GetText(9005, "Grossist") %></label></th>
                                <th width="200px"><label style="padding-left:5px"><%=GetText(9006, "EDI-typ")%></label></th>
                            </tr>
                            <SOE:FormIntervalEntry
                                ID="EDIToOrderTransferRules"
                                OnlyFrom="true"
                                DisableHeader="true"
                                DisableSettings="true"
                                EnableDelete="true"
                                ContentType="2"
                                LabelType="2"
                                LabelWidth="200"
                                FromWidth="200"
                                HideLabel="true"
                                NoOfIntervals="20"
                                runat="server">
                            </SOE:FormIntervalEntry>
                        </table>
                    </fieldset>
                    <fieldset> 
                        <legend><%=GetText(8317, "Prissättning")%></legend>
                        <table>                         
                           <SOE:SelectEntry
                                ID="PriceSetting"
                                TermID="8283" DefaultTerm="Använd prissättningsregel"
                                Width="300"
                                runat="server">
                            </SOE:SelectEntry>                            
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

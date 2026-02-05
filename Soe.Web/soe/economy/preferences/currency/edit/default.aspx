<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.currency.edit._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">
			    <div>
                    <fieldset>
                        <legend><%=GetText(1891, "Valuta")%></legend>
			            <table>
	                        <SOE:SelectEntry
	                            ID="Code" 
	                            TermID="1891" 
	                            DefaultTerm="Valuta"
	                            OnChange="getCurrency()"
	                            runat="server">
	                        </SOE:SelectEntry>
                        </table>
                    </fieldset>
			    </div>
			    <div>
                    <fieldset>
						<legend><%=GetText(1890, "Växlingskurser")%></legend>
		                <table>
                            <SOE:SelectEntry
                                ID="IntervalType"
                                TermID="5703" DefaultTerm="Uppdatering kurser"
                                OnChange="toggleCurrencyRate()"
                                runat="server">
                            </SOE:SelectEntry>
		                    <SOE:NumericEntry 
			                    ID="RateToBase"
			                    TermID="4075" 
			                    DefaultTerm="Basvaluta/valuta"
			                    MaxLength="50"
			                    AllowDecimals="true"
                                OnChange="SetRateFromBase()"
			                    runat="server">
		                    </SOE:NumericEntry>
                            <SOE:NumericEntry 
			                    ID="RateFromBase"
			                    TermID="4074" 
			                    DefaultTerm="Valuta/basvaluta"
                                ReadOnly="true"
			                    MaxLength="50"
			                    AllowDecimals="true"
			                    runat="server">
		                    </SOE:NumericEntry>
					        <SOE:DateEntry runat="server"
					            ID="RateDate"
					            TermID="5704" DefaultTerm="Datum"
					            MaxLength="100">
				            </SOE:DateEntry>
		                </table>
                    </fieldset>
			    </div>
			</SOE:Tab>
		</tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

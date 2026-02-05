<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.preferences.productsettings._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="5429" DefaultTerm="Inställningar artiklar" runat="server">
			    <div>
				    <fieldset> 
					    <legend><%=GetText(3067, "Registrering")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="DefaultInvoiceProductVatType" 
                                TermID="4300"
                                DefaultTerm="Standard artikeltyp" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultInvoiceProductUnit" 
                                TermID="4301"
                                DefaultTerm="Standard enhet" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultMaterialCode"
                                TermID="9039"
                                DefaultTerm="Standard materialkod"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultStock"
                                TermID="4617"
                                DefaultTerm="Standard lager"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="UseProductUnitConvert"
                                TermID="7410"
                                DefaultTerm="Omvandlingsfaktor lager" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ManuallyUpdatedAvgPrices"
                                TermID="7738"
                                DefaultTerm="Manuell uppdatering av snittpris på lagerförda artiklar" 
                                runat="server">
                            </SOE:CheckBoxEntry>                            
                            <SOE:CheckBoxEntry
                                ID="ProductRowDescriptionsUppercase"
                                TermID="6151"
                                DefaultTerm="Omvandla artikel rad benämningar till stora bokstäver" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:SelectEntry
                                ID="DefaultGrossMarginCalculationType"
                                TermID="7750"
                                DefaultTerm="Beräkningsmetod för bruttomarginal"
                                runat="server">
                            </SOE:SelectEntry>
					    </table>
                    </fieldset>
                </div>
			    <div>
				    <fieldset> 
					    <legend><%=GetText(3436, "Sök extern artikel")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="InitProductSearch" 
                                TermID="3437"
                                DefaultTerm="Starta sökning" 
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

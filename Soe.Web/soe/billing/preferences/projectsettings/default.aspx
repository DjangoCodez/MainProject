<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.preferences.projectsettings._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <script type="text/javascript" language="javascript">
        function autoGenerateProjectClick() {
            var autoCreateProjectOnNewInvoice = document.getElementById('AutoCreateProjectOnNewInvoice');
            var useOrderNumberAsProjectNr = document.getElementById('UseOrderNumberAsProjectNumber');

            if (autoCreateProjectOnNewInvoice.checked == 'checked' || autoCreateProjectOnNewInvoice.checked == true)
            {
                useOrderNumberAsProjectNr.checked = 'checked';
                useOrderNumberAsProjectNr.value = true;
                return;
            }
        }

        function useOrderNumberAsProjectClick() {
            var autoCreateProjectOnNewInvoice = document.getElementById('AutoCreateProjectOnNewInvoice');
            var useOrderNumberAsProjectNr = document.getElementById('UseOrderNumberAsProjectNumber');

            if (useOrderNumberAsProjectNr.checked != 'checked' && useOrderNumberAsProjectNr.checked == false)
            {
                autoCreateProjectOnNewInvoice.checked = '';
                autoCreateProjectOnNewInvoice.value = false;
                return;
            }
        }

        function overheadCostAsFixedAmountClick() {
            var overheadCostAsFixedAmount = document.getElementById('OverheadCostAsFixedAmount');
            var overheadCostAsAmountPerHour = document.getElementById('OverheadCostAsAmountPerHour');

            if (overheadCostAsFixedAmount.checked == 'checked' || overheadCostAsFixedAmount.checked == true) {
                overheadCostAsAmountPerHour.checked = '';
                overheadCostAsAmountPerHour.value = false;
                return;
            }
        }

        function overheadCostAsAmountPerHourClick() {
            var overheadCostAsFixedAmount = document.getElementById('OverheadCostAsFixedAmount');
            var overheadCostAsAmountPerHour = document.getElementById('OverheadCostAsAmountPerHour');

            if (overheadCostAsAmountPerHour.checked == 'checked' || overheadCostAsAmountPerHour.checked == true) {
                overheadCostAsFixedAmount.checked = '';
                overheadCostAsFixedAmount.value = false;
                return;
            }
        }
    </script>
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
		<Tabs>
			<SOE:Tab Type="Setting" TermID="5416" DefaultTerm="Projektinställningar" runat="server">
				<div>
                    <asp:HiddenField ID="UsingPayroll" runat="server" /> 
					<fieldset> 
						<legend><%=GetText(7580, "Skapa projekt")%></legend>
						<table>
						  <SOE:CheckBoxEntry
		                        ID="UseOrderNumberAsProjectNumber"
		                        runat="server"
		                        TermID="7137"
		                        DefaultTerm="Föreslå order/fakturanummer som projektnummer"
                                OnClick="useOrderNumberAsProjectClick()" >
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="AutoCreateProjectOnNewInvoice"
		                        runat="server"
		                        TermID="7168"
		                        DefaultTerm="Skapa automatiskt ett projekt för varje faktura/order"
                                OnClick="autoGenerateProjectClick()" >
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="LimitOrderToProjectUsers"
		                        runat="server"
		                        TermID="9106"
		                        DefaultTerm="Tillåt endast projektdeltagare att redigera kopplade ordrar">
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="UseCustomerNameAsProjectName"
		                        runat="server"
		                        TermID="7200"
		                        DefaultTerm="Föreslå kundnamn som projektnamn">
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="ProjectAutoUpdateAccountSettings"
		                        runat="server"
		                        TermID="7583"
		                        DefaultTerm="Uppdatera projektets konteringsinställningar (nivå 3)">
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="ProjectAutoUpdateInternalAccounts"
		                        runat="server"
		                        TermID="7889"
		                        DefaultTerm="Automatisk uppdatering av internkonton">
		                    </SOE:CheckBoxEntry>
						</table>
					</fieldset>
					<fieldset> 
						<legend><%=GetText(7577, "Projektbudget")%></legend>
						<table>
							<SOE:CheckBoxEntry
		                        ID="OverheadCostAsFixedAmount"
		                        runat="server"
		                        TermID="7221"
		                        DefaultTerm="Overheadkostnad som fast belopp"
                                OnClick="overheadCostAsFixedAmountClick()">
		                    </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
		                        ID="OverheadCostAsAmountPerHour"
		                        runat="server"
		                        TermID="7222"
		                        DefaultTerm="Overheadkostnad som belopp per timme"
                                OnClick="overheadCostAsAmountPerHourClick()">
		                    </SOE:CheckBoxEntry>
						</table>
					</fieldset>
					<fieldset> 
						<legend><%=GetText(7579, "Tidsregistering")%></legend>
						<table>
							<!--
							<SOE:CheckBoxEntry
		                        ID="KeepEmployeesOnWeekChange"
		                        runat="server"
		                        TermID="7169"
		                        DefaultTerm="Ta med anställda med tid vid byte av vecka">
		                    </SOE:CheckBoxEntry>
							-->
							<SOE:CheckBoxEntry
		                        ID="AutosaveOnWeekChangeInOrder"
		                        runat="server"
		                        TermID="7243"
		                        DefaultTerm="Autospara vid byte av vecka i order">
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="InvoiceTimeAsWorkTime"
		                        runat="server"
		                        TermID="2239"
		                        DefaultTerm="Arbetad tid ska vara samma som fakturerbar tid">
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="CreateTransactionsBaseOnTimeRules"
		                        runat="server"
		                        TermID="11656"
		                        DefaultTerm="Tidsregistrering enligt tidregelverk">
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="ExtendedTimeRegistration"
		                        runat="server"
		                        TermID="11655"
		                        DefaultTerm="Använd utökad tidsinmatning">
		                    </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
		                        ID="UseProjectTimeBlocks"
		                        runat="server"
		                        TermID="7446"
		                        DefaultTerm="Använd tabellen ProjectTimeBlock">
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="BlockTimeBlockWithZeroStartTime"
		                        runat="server"
		                        TermID="7649"
		                        DefaultTerm="Tillåt inte 00:00 som rapporterad starttid">
		                    </SOE:CheckBoxEntry>
						</table>
					</fieldset>
					<fieldset> 
						<legend><%=GetText(7477, "Fakturering")%></legend>
						<table>
                            <SOE:CheckBoxEntry
		                        ID="IncludeTimeProjectReport"
		                        runat="server"
		                        TermID="8220"
		                        DefaultTerm="Inkludera tidbok med order/faktura vid utskrift">
		                    </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
		                        ID="IncludeOnlyInvoicedTimeInTimeProjectReport"
		                        runat="server"
		                        TermID="7194"
		                        DefaultTerm="Inkludera enbart fakturerad tid i tidbok vid utskrift">
		                    </SOE:CheckBoxEntry>
		                    <SOE:CheckBoxEntry
		                        ID="MoveTransactionToInvoiceRow"
		                        runat="server"
		                        TermID="4524"
		                        DefaultTerm="Flytta över transaktioner som fakturarader">
		                    </SOE:CheckBoxEntry>
						</table>
					</fieldset>
					<fieldset> 
						<legend><%=GetText(7578, "Belasta projekt")%></legend>
						<table>
							<SOE:CheckBoxEntry
		                        ID="ChargeCostsToProject"
		                        runat="server"
		                        TermID="7186"
		                        DefaultTerm="Låt kostnader på leverantörsfaktura belasta projekt">
		                    </SOE:CheckBoxEntry>
                            <SOE:SelectEntry
                                ID="TimeCodeSelectEntry"
                                runat="server"
                                TermID="7404"
                                DefaultTerm="Standardkod vid belastning av projekt">
                            </SOE:SelectEntry>
						</table>
					</fieldset>
					<fieldset> 
						<legend><%=GetText(7629, "Utskrift/Översikt")%></legend>
						<table>
							<SOE:CheckBoxEntry
		                        ID="GetPurchasePriceFromInvoiceProduct"
		                        runat="server"
		                        TermID="7630"
		                        DefaultTerm="Inköpspris från artikel">
		                    </SOE:CheckBoxEntry>
							<SOE:CheckBoxEntry
		                        ID="UseDateIntervalInIncomeNotInvoiced"
		                        runat="server"
		                        TermID="7701"
		                        DefaultTerm="Använd datumurval för Intäkter ofakturerat">
		                    </SOE:CheckBoxEntry>
						</table>
					</fieldset>
				</div>
			</SOE:Tab>		
		</Tabs>
	</SOE:Form>		
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

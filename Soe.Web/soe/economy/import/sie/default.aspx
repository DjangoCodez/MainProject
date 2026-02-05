<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.import.sie._default" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">    
<%if (UseAngularSpa) {%>
    <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
<%} else {%>
    <SOE:Form ID="Form1" TermID="1174" DefaultTerm="Importera" EncType="multipart/form-data" runat="server">
          <Tabs>
		    <SOE:Tab Type="Import" runat="server">
				<div>
				    <SOE:InstructionList ID="NotImplementedInstruction" runat="server"></SOE:InstructionList>
				</div>
			    <div>
                    <fieldset>
				        <legend><%=GetText(1180, "SIE import")%></legend>
				        <table>
                	        <input type="hidden" name="action" value="upload" />
                            <SOE:FileEntry 
		                        ID="File" 
		                        TermID="1171" DefaultTerm="SIE fil" 
		                        Width="500"
		                        runat="server">
		                    </SOE:FileEntry>
                            <SOE:CheckBoxEntry 
                                ID="ImportAsUtf8" 
                                TermID="6412" DefaultTerm="Använd UTF-8 (standard ANSI)"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                        <div id="DivImportTypeSelection" runat="server">
                            <fieldset>
                                <legend><%=GetText(1280, "Välj vad som ska importeras")%></legend>
                                <table>		                                                                
                                    <SOE:CheckBoxEntry 
                                        ID="CheckBoxSelectAccount"                                         
                                        TermID="1258" DefaultTerm="Konto"                                        
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry 
                                        ID="CheckBoxSelectVoucher" 
                                        TermID="1259" DefaultTerm="Verifikat"                                        
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry 
                                        ID="CheckBoxSelectAccountBalance" 
                                        TermID="1391" DefaultTerm="Ingående balans"                                        
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                </table>
                            </fieldset>                                                        
                        </div>
                        <div id="DivAccountYearSelection" runat="server">
                            <fieldset>
                                <legend><%=GetText(1291, "Inställningar redovisningsår")%></legend>
                                <table>
		                            <SOE:SelectEntry 
							            ID="AccountYear" 
							            TermID="1388" DefaultTerm="Redovisningsår"
							            OnChange="getVoucherSeries()"
							            runat="server">
							        </SOE:SelectEntry>
                                    <SOE:CheckBoxEntry 
                                        ID="AllowNotOpenAccountYear" 
                                        TermID="5673" DefaultTerm="Tillåt låsta/stängda redovisningsår"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivAccountSelection" runat="server">                            
                            <fieldset>
                                <legend><%=GetText(1293, "Inställningar konton")%></legend>
                                <table>
                                    <SOE:CheckBoxEntry 
                                        ID="ImportAccountStd" 
                                        TermID="1640" DefaultTerm="Importera konton"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry 
                                        ID="ImportAccountInternal" 
                                        TermID="1641" DefaultTerm="Importera internkonton"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry 
                                        ID="OverrideNameConflicts" 
                                        TermID="1144" DefaultTerm="Skriv över befintligt kontonamn"
                                        runat="server">
                                    </SOE:CheckBoxEntry>		            
                                    <SOE:CheckBoxEntry 
                                        ID="ApproveEmptyAccountNames" 
                                        TermID="1426" DefaultTerm="Tillåt tomma kontonamn"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivVoucherSelection" runat="server">                            
                            <fieldset>
                                <legend><%=GetText(1333, "Inställningar verifikat")%></legend>
                                <table>
	                                <SOE:SelectEntry 
		                                ID="VoucherSeries" 
		                                TermID="1228" 
		                                DefaultTerm="Default verifikatserie"
		                                OnChange="checkVoucherSeries()"
		                                runat="server">
		                            </SOE:SelectEntry>
                                    <SOE:CheckBoxEntry 
                                        ID="OverrideVoucherSeries" 
                                        TermID="1387" DefaultTerm="Importera allt till defaultserie"
                                        OnChange="checkVoucherSeriesMapping()"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry 
                                        ID="UseAccountDistribution" 
                                        TermID="4869" DefaultTerm="Använd automatkontering"
                                        runat="server">
                                    </SOE:CheckBoxEntry> 
                                </table>
                                <div id="DivVoucherSeriesMapping">
                                    <table>
                                        <SOE:Text
			                                ID="VoucherSeriesTypesMappingText"
                                            TermID="5686" DefaultTerm="Matcha verifikatserier mot serier i filen om de inte överresstämmer"
                                            runat="server">
                                        </SOE:Text>
                                    </table>
                                    <table>
                                        <SOE:FormIntervalEntry
                                            ID="VoucherSeriesTypesMapping"
                                            NoOfIntervals="10"
                                            EnableCheck="false"
                                            HideLabel="true"
                                            ContentType="1"
                                            LabelType="2"
                                            OnlyFrom="true"
                                            EnableDelete="true"
                                            runat="server">
                                        </SOE:FormIntervalEntry>
                                    </table>
                                </div>
                                <table>
                                    <SOE:CheckBoxEntry 
                                        ID="SkipAlreadyExistingVouchers" 
                                        TermID="9264" DefaultTerm="Hoppa över verifikat med samma nummer och serie i samma redovisningsår"
                                        OnChange="checkVoucherSkip()"  
                                        Value="True"                                       
                                        runat="server">
                                    </SOE:CheckBoxEntry> 
                                    <SOE:CheckBoxEntry 
                                        ID="OverrideVoucherSeriesDelete" 
                                        TermID="9158" DefaultTerm="Ta bort samtliga verifikat som är importerade från SIE-fil före import"
                                        OnChange="checkVoucherDelete()"
                                        runat="server">
                                    </SOE:CheckBoxEntry> 
                                 </table>
                                 <div id="DivVoucherSeriesDelete">
                                    <table>
                                        <SOE:Text
			                                ID="VoucherSeriesDeleteText"
                                            TermID="9157" DefaultTerm="Ta bort verifikat i markerade verifikatserier"
                                            runat="server">
                                        </SOE:Text>
                                    </table>
                                    <table>
                                        <SOE:FormIntervalEntry
                                            ID="VoucherSeriesDelete"
                                            NoOfIntervals="10"
                                            EnableCheck="true"
                                            ContentType="0"
                                            LabelType="2"
                                            OnlyFrom="true"
                                            EnableDelete="true"
                                            runat="server">
                                        </SOE:FormIntervalEntry>
                                    </table>                                   
                                </div>
                            </fieldset>
                        </div>
                        <div id="DivAccountBalanceSelection" runat="server">                            
                            <fieldset>
                                <legend><%=GetText(1296, "Inställningar ingående balans")%></legend>
                                <table>
                                    <SOE:CheckBoxEntry 
                                        ID="OverrideAccountBalance" 
                                        TermID="1638" DefaultTerm="Skriv över befintlig ingående balans"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry 
                                        ID="UseUBInsteadOfIB" 
                                        TermID="8041" DefaultTerm="Använd utgående balans istället för ingående balans"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                </table>
                            </fieldset>
                        </div>
				    </fieldset>
		        </div>
		    </SOE:Tab>
	    </Tabs>
    </SOE:Form>
    <div>
        <SOE:Grid ID="SoeGrid1" runat="server" AutoGenerateColumns="false">
            <Columns>
                <SOE:BoundField 
                    DataField="Label" 
                    TermID="1191" DefaultTerm="Etikett" 
                    Filterable="Contains" Sortable="Text">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="LineNr" 
                    TermID="1207" DefaultTerm="RadNr" 
                    Filterable="Contains" Sortable="Text">
                </SOE:BoundField>
                <SOE:TemplateField Filterable="Contains" Sortable="Text">
                    <HeaderTemplate><%=GetText(1521, "Fältvärden") %></HeaderTemplate>
                    <ItemTemplate>
                        <asp:PlaceHolder ID="phValues" runat="server"></asp:PlaceHolder>
                    </ItemTemplate>
                </SOE:TemplateField>  
                <SOE:TemplateField Filterable="Contains" Sortable="Text">
                    <HeaderTemplate><%=GetText(1522, "Konflikt") %></HeaderTemplate>
                    <ItemTemplate>
                        <asp:PlaceHolder ID="phConflict" runat="server"></asp:PlaceHolder>
                    </ItemTemplate>
                </SOE:TemplateField>  
             </Columns>
        </SOE:Grid>
    </div>
 <%}%>  
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

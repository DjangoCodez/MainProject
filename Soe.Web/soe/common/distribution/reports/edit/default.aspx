<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.distribution.reports.edit._default" %>
<%@ Register Src="~/UserControls/Translations.ascx" TagPrefix="SOE" TagName="Translations" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="false" EncType="multipart/form-data" runat="server">
	    <Tabs>
		    <SOE:Tab Type="Edit" runat="server">
		        <div>
		            <div>
		                <fieldset>
		                    <legend><%=GetText(1365, "Rapportmall")%></legend>
		                    <table>
                                <SOE:SelectEntry 
                                    ID="ReportTemplate"
                                    TermID="1364" DefaultTerm="Egna"
                                    OnChange = "reportSelected('ReportTemplate','SysReportTemplate')"
                                    Width="200"
                                    runat="server">
                                </SOE:SelectEntry>	                            
                                <SOE:SelectEntry 
                                    ID="SysReportTemplate"
                                    TermID="1371" DefaultTerm="System"
                                    OnChange = "reportSelected('SysReportTemplate','ReportTemplate')"
                                    Width="400"
                                    runat="server">
                                </SOE:SelectEntry>	 
                            </table>              
                        </fieldset>
                     </div>
		            <div id="DivImportReportContent" visible="false" runat="server">
		                <fieldset>
		                    <legend><%=GetText(1622, "Importera rapportinnehåll")%></legend>
		                    <table>
                                <SOE:SelectEntry 
                                    ID="ImportCompany"
                                    TermID="1623" DefaultTerm="Företag"
                                    Width="200"
                                    runat="server">
                                </SOE:SelectEntry>	 
                                <SOE:SelectEntry 
                                    ID="ImportReport"
                                    TermID="1624" DefaultTerm="Rapport"
                                    Width="200"
                                    runat="server">
                                </SOE:SelectEntry>	 
                                 <SOE:CheckBoxEntry 
                                    ID="NewGroupsAndHeaders" 
                                    TermID="2301" DefaultTerm="Registrera nya rapportgrupper och rubriker"                                
                                    runat="server">
                               </SOE:CheckBoxEntry>
                            </table>              
                        </fieldset>
                    </div>
		        </div>
		        <div>
		            <fieldset>
		                <legend><%=GetText(1370, "Rapport") %></legend>
                        <table>
							<SOE:NumericEntry 
							    ID="ReportNr"
							    TermID="1468" DefaultTerm="RapportNr" 
							    Validation="Required"
							    InvalidAlertTermID="1469" InvalidAlertDefaultTerm="Du måste ange rapportnr"
							    MaxLength="10" 
							    runat="server">
							</SOE:NumericEntry>
	                        <SOE:TextEntry 
		                        ID="Name"
		                        TermID="1326" DefaultTerm="Namn"
		                        Validation="Required"
		                        InvalidAlertTermID="1327" InvalidAlertDefaultTerm="Du måste ange namn"
		                        MaxLength="100"
		                        Width="200"
		                        runat="server">
		                    </SOE:TextEntry>
	                        <SOE:TextEntry 
		                        ID="Description"
		                        TermID="1328" DefaultTerm="Beskrivning"
		                        MaxLength="255"
		                        Width="200"
		                        runat="server">
	                        </SOE:TextEntry>
                            <SOE:SelectEntry 
                                ID="ExportType"
                                TermID="1771" DefaultTerm="Exporttyp"
                                OnChange ="exportTypeSelected('ExportTypeChanged')" 
                                Visible="true"
                                runat ="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry 
                                ID="ReportExportFileType"
                                TermID="8593" DefaultTerm="Filtyp"
                                Visible="true"    
                                runat="server">
                            </SOE:SelectEntry >
                            <SOE:CheckBoxEntry 
                                ID="IncludeAllHistoricalData" 
                                TermID="5946" DefaultTerm="Inkludera all historisk data"
                                Visible="false"                                             
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="IncludeBudget" 
                                TermID="9159" DefaultTerm="Inkludera budget"
                                Visible="false"                                             
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:NumericEntry
							    ID="NumberOfYearsBackPreviousData"
							    TermID="9160" DefaultTerm="Tidigare års data hämtas för X antal år tillbaka i tiden"
                                Visible="false" 
							    MaxLength="1" 
							    runat="server">
							</SOE:NumericEntry>
                            <SOE:CheckBoxEntry 
                                ID="GetDetailedInformation" 
                                TermID="9180" DefaultTerm="Inkludera detaljerad information"
                                Visible="false"                                             
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="ShowInAccountingReports" 
                                TermID="9265" DefaultTerm="Visa i Drillbara rapporter"
                                Visible="false"                                             
                                runat="server">
                            </SOE:CheckBoxEntry>
                          </table>
		            </fieldset>
                    <div id="DivSorting" runat="server">
                        <fieldset>
		                    <legend><%=GetText(4901, "Sortering rapporten") %></legend>
                            <table>
                                <tr>
                                    <td>
                                       <SOE:Text 
                                        FitInTable ="true"
                                        ID="ReportGroupByText"
                                        TermID="4902" 
                                        DefaultTerm="Gruppera efter"
                                        runat="server">
                                       </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:SelectEntry
                                            FitInTable ="true"
                                            ID="GroupByLevel1"
                                            TermID="4902" 
                                            DefaultTerm="Gruppera efter"
                                            Width="140"
                                            runat="server"
                                            align="right"
                                            HideLabel="true">
                                            </SOE:SelectEntry>	
                                    </td>
                                    <td>
                                        <SOE:SelectEntry  
                                            FitInTable ="true"
                                            ID="GroupByLevel2"
                                            TermID="4903" 
                                            DefaultTerm="sedan efter"
                                            Width="140"
                                            HideLabel="true"
                                            runat="server">
                                        </SOE:SelectEntry>	
                                    </td>               
                                    <td>
                                        <SOE:SelectEntry 
                                        FitInTable ="true"
                                        ID="GroupByLevel3"
                                        TermID="4903" 
                                        DefaultTerm="sedan efter"
                                        Width="140"
                                        HideLabel="true"
                                        runat="server">
                                        </SOE:SelectEntry>
                                    </td>                
                                    <td>
                                        <SOE:SelectEntry  
                                        FitInTable ="true"
                                        ID="GroupByLevel4"
                                        TermID="4903" 
                                        DefaultTerm="sedan efter"
                                        Width="140"
                                        HideLabel="true"
                                        runat="server">
                                        </SOE:SelectEntry>	
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text 
                                            FitInTable ="true"
                                            ID="ReportSortByText"
                                            TermID="4904" 
                                            DefaultTerm="Sortera efter"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:SelectEntry 
                                            FitInTable ="true"
                                            ID="SortByLevel1"
                                            TermID="4904" 
                                            DefaultTerm ="Sortera efter"
                                            Width="140"
                                            HideLabel="true"
                                            runat="server">
                                        </SOE:SelectEntry>		
                                    </td>               
                                    <td>
                                        <SOE:SelectEntry 
                                            FitInTable ="true"
                                            ID="SortByLevel2"
                                            TermID="4905" DefaultTerm="sedan efter"
                                            Width="140"
                                            HideLabel="true"
                                            runat="server">
                                        </SOE:SelectEntry>		
                                    </td>
                                    <td>
                                        <SOE:SelectEntry 
                                            FitInTable ="true"
                                            ID="SortByLevel3"
                                            TermID="4905" DefaultTerm="sedan efter"
                                            Width="140"
                                            HideLabel="true"
                                            runat="server">
                                        </SOE:SelectEntry>		
                                    </td>
                                    <td>
                                        <SOE:SelectEntry 
                                            FitInTable ="true"
                                            ID="SortByLevel4"
                                            TermID="4905" DefaultTerm="sedan efter"
                                            Width="140"
                                            HideLabel="true"
                                            runat="server">
                                        </SOE:SelectEntry>		
                                    </td>
                                </tr>
                           </table>                           
                          <table> 
                            <tr>
                                <td>
                                    <SOE:Text 
                                        FitInTable ="true"
                                        ID="labelTextSpecialField"
                                        TermID="4906" 
                                        DefaultTerm="Speciellt fält"
                                        runat="server">
                                    </SOE:Text>
                                </td>
                                <td>
                                    <SOE:TextEntry 
		                                ID="Special"
                                        FitInTable ="true"
		                                TermID="4906" DefaultTerm="Special"
                                        HideLabel="true"
		                                InvalidAlertTermID="1354" InvalidAlertDefaultTerm="Du måste ange namn"
		                                MaxLength="100"
		                                Width="200"
		                                runat="server">
		                            </SOE:TextEntry>
                                </td>
                               <td>
                                   <SOE:CheckBoxEntry 
                                        FitInTable ="true"
                                        ID="IsSortAscending" 
                                        TermID="4907" DefaultTerm="Är sortering stigande?"                                
                                        runat="server">
                                   </SOE:CheckBoxEntry>
                              </td>
                           </tr>
                        </table> 
		            </fieldset>
                  </div>                    
                  <div>
                    <SOE:Translations ID="Translations" Runat="Server"></SOE:Translations>      
                  </div>
		        </div>
		    </SOE:Tab>
        </Tabs>
    </SOE:Form>
<script language="javascript">
    window.onload = function () { Page_Start() };
</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

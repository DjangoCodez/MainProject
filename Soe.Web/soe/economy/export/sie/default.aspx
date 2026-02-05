<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.export.sie._default" %>
<%@ Register TagName="SelectionStd" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionStd.ascx" %>
<%@ Register TagName="SelectionAccount" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionAccount.ascx" %>
<%@ Register TagName="SelectionVoucher" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionVoucher.ascx" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
<%if (UseAngularSpa) {%>
    <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
<%} else {%>
    <SOE:Form ID="Form1" TermID="1455" DefaultTerm="Exportera" EncType="multipart/form-data" runat="server">
	    <Tabs>
		    <SOE:Tab Type="Export" runat="server">
				<div>
				    <SOE:InstructionList ID="NotImplementedInstruction" runat="server"></SOE:InstructionList>
				</div>
                <div id="DivAccount" runat="server">
                    <fieldset>
				        <legend><%=GetText(1425, "SIE export")%></legend>
			            <table>
                            <SOE:CheckBoxEntry 
                                ID="ExportPreviousYear" 
                                TermID="1509" DefaultTerm="Redov. föregående år"                
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="ExportObject" 
                                TermID="1490" DefaultTerm="Exportera objekt"                
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="ExportAccount" 
                                TermID="1489" DefaultTerm="Exportera konto"        
                                OnClick="sieExportAccountChanged()"              
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="ExportAccountType" 
                                TermID="1483" DefaultTerm="Exportera kontotyp"                                            
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="ExportSruCodes" 
                                TermID="1492" DefaultTerm="Exportera SRU-koder"                                            
                                runat="server">
                            </SOE:CheckBoxEntry>
                		    <SOE:TextEntry 
                                ID="Comment"
                                TermID="1436" DefaultTerm="Kommentar"
                                MaxLength="255"
                                runat="server">
                            </SOE:TextEntry>
			            </table>
                    </fieldset>
	                <div>
	                    <SOE:SelectionStd ID="SelectionStd" ShowOnlyOpenAccountYears="false" DisableDateSelection="true" AdjustForOnlyFromYearInterval="true" AdjustForOnlyFromPeriodInterval="true" Runat="Server"></SOE:SelectionStd>
                    </div>
                    <div>
	                    <SOE:SelectionVoucher ID="SelectionVoucher" Runat="Server"></SOE:SelectionVoucher>
                    </div>
                    <div>
	                    <SOE:SelectionAccount ID="SelectionAccount" Runat="Server"></SOE:SelectionAccount>
                    </div>
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
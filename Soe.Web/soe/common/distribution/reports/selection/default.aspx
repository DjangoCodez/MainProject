<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.distribution.reports.selection._default" %>
<%@ Register TagName="SelectionStd" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionStd.ascx" %>
<%@ Register TagName="SelectionVoucher" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionVoucher.ascx" %>
<%@ Register TagName="SelectionAccount" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionAccount.ascx" %>
<%@ Register TagName="SelectionFixedAssets" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionFixedAssets.ascx" %>
<%@ Register TagName="SelectionLedger" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionLedger.ascx" %>
<%@ Register TagName="SelectionBilling" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionBilling.ascx" %>
<%@ Register TagName="SelectionTime" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionTime.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" EnableBack="true" EnablePrevNext="false" EnableDelete="false" EnableCopy="false" EnableRunReport="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" TermID="1300" DefaultTerm="Rapporturval" runat="server">
	            <div id="DivReportSelectionText" runat="server">
                    <fieldset>
                        <legend><%=GetText(1290, "Spara urval")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="ReportSelectionText"
                                TermID="1444" DefaultTerm="Urvalsnamn"
                                MaxLength="50"
                                runat="server">
                            </SOE:TextEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
	                <SOE:SelectionStd ID="SelectionStd" ShowOnlyOpenAccountYears="false" Runat="Server"></SOE:SelectionStd>
                </div>
			    <div id="DivSelectionLedger" runat="server">
                    <SOE:SelectionLedger ID="SelectionLedger" Runat="Server"></SOE:SelectionLedger>
			    </div>
			    <div id="DivSelectionBilling" runat="server">
                    <SOE:SelectionBilling ID="SelectionBilling" Runat="Server"></SOE:SelectionBilling>
			    </div>
			    <div id="DivSelectionTime" runat="server">
			        <SOE:SelectionTime ID="SelectionTime" Runat="Server"></SOE:SelectionTime>
			    </div>
			    <div id="DivSelectionAccounting" runat="server">
                    <SOE:SelectionVoucher ID="SelectionVoucher" Runat="Server"></SOE:SelectionVoucher>
                    <SOE:SelectionAccount ID="SelectionAccount" Runat="Server"></SOE:SelectionAccount>                    
                    <SOE:SelectionFixedAssets ID="SelectionFixedAssets" Runat="Server"></SOE:SelectionFixedAssets>
			    </div>                
			</SOE:Tab>
			<SOE:Tab Type="Setting" runat="server">
			    <div>
	                <fieldset>
                        <legend><%=GetText(1455, "Exportera") %></legend>
                        <table>
                            <input type="hidden" id="DownloadReportFlag" value="0" runat="server" />
                            <SOE:SelectEntry 
                                ID="ExportType"
                                TermID="1294" DefaultTerm="Exporttyp"
                                runat="server">
                            </SOE:SelectEntry>	
                        </table>
                    </fieldset>
                </div>
			    <div id="ReportExport" class="ReportExport" runat="server">
                    <fieldset>
                        <legend><%=GetText(1457, "Rapportinställningar") %></legend>
                        <table>
                        </table>
			        </fieldset>
			    </div>
			</SOE:Tab>
	    </Tabs>
	</SOE:Form>
       <div class="searchTemplate">
        <div id="searchContainer" class="searchContainer">
        </div>
                      
         <div id="CustomerByNumberSearchItem_$customerNr$">
            <div id="customer_$id$" class="item" onmouseover="searchComponent.select();" onclick="searchComponent.choose();">
                <div class="id" id="extendNumWidth_$id$">$customerNr$</div>
                <div class="name" id="extendNameWidth_$id$">$customerName$</div>
            </div>
        </div>


        <div id="SupplierByNumberSearchItem_$supplierNr$">
            <div id="supplier_$id$" class="item" onmouseover="searchComponent.select();" onclick="searchComponent.choose();">
                <div class="id" id="extendNumWidth_$id$">$supplierNr$</div>
                <div class="name" id="extendNameWidth_$id$">$supplierName$</div>
            </div>
        </div>

        <div id="ProjectByNumberSearchItem_$projectNr$">
            <div id="project_$id$" class="item" onmouseover="searchComponent.select();" onclick="searchComponent.choose();">
                <div class="id" id="extendNumWidth_$id$">$projectNr$</div>
                <div class="name" id="extendNameWidth_$id$">$projectName$</div>
            </div>
        </div>

           <div id="EmployeeByNumberSearchItem_$employeeNr$">
            <div id="employee_$id$" class="item" onmouseover="searchComponent.select();" onclick="searchComponent.choose();">
                <div class="id" id="extendNumWidth_$id$">$employeeNr$</div>
                <div class="name" id="extendNameWidth_$id$">$employeeName$</div>
            </div>
        </div>
        
    </div>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>


<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.accounting.voucherhistory._default" %>
<%@ Register TagName="SelectionStd" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionStd.ascx" %>
<%@ Register TagName="SelectionVoucher" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionVoucher.ascx" %>
<%@ Register TagName="SelectionAccount" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionAccount.ascx" %>
<%@ Register TagName="SelectionUser" TagPrefix="SOE" Src="~/UserControls/DistributionSelectionUser.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" TermID="1512" DefaultTerm="Filtrera" EnablePrevNext="false" EnableDelete="false" EnableCopy="false" runat="server">
		<Tabs>
			<SOE:Tab Type="View" TermID="1143" DefaultTerm="Behandlingshistorik" runat="server">
			    <div>
			        <div>
			            <SOE:SelectionStd ID="SelectionStd" ShowOnlyOpenAccountYears="false" EnableUserSelection="true" Runat="Server"></SOE:SelectionStd>
                    </div>
                    <div>
                        <SOE:SelectionVoucher ID="SelectionVoucher" Runat="Server"></SOE:SelectionVoucher>
                    </div>
                    <div>
                        <SOE:SelectionUser ID="SelectionUser" Runat="Server"></SOE:SelectionUser>
                    </div>
                    <div>
                       <fieldset>
                            <legend><%=GetText(1651, "Sortering") %></legend>
                            <table>
                                <SOE:SelectEntry 
                                    ID="SortField"
                                    TermID="1649" DefaultTerm="Sortera på fält"
                                    runat="server">
                                </SOE:SelectEntry>	
                                <SOE:SelectEntry 
                                    ID="SortOrder"
                                    TermID="1650" DefaultTerm="Sorteringsordning"
                                    runat="server">
                                </SOE:SelectEntry>	
                            </table>
                        </fieldset>
                    </div>
                </div>
                <div>
                    <div>
                        <SOE:SelectionAccount ID="SelectionAccount" Runat="Server"></SOE:SelectionAccount>
                    </div>
                </div>
            </SOE:Tab>
        </Tabs>
	</SOE:Form>
    <div>
	    <SOE:Grid ID="SoeGrid1" ItemType="SoftOne.Soe.Data.VoucherRowHistory" runat="server" AutoGenerateColumns="false">
		    <Columns>
                <SOE:BoundField
                    DataField="RegDate" 
                    TermID="1141" DefaultTerm="Reg.datum"
                    Filterable="Contains" Sortable="Text">
                </SOE:BoundField>
		        <SOE:BoundField 
                    DataField="VoucherNr" 
                    TermID="1138" DefaultTerm="VerNr" 
                    Filterable="Numeric" Sortable="Numeric">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="AccountNr" 
                    TermID="1139" DefaultTerm="KontoNr" 
                    Filterable="Contains" Sortable="Text">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="Amount" 
                    TermID="1142" DefaultTerm="Belopp" 
                    Filterable="Numeric" Sortable="Numeric">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="Quantity" 
                    TermID="1713" DefaultTerm="Kvantitet" 
                    Filterable="Numeric" Sortable="Numeric">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="VoucherSeriesTypeNr" 
                    TermID="1506" DefaultTerm="Serie" 
                    Filterable="Numeric" Sortable="Numeric">
                </SOE:BoundField>
<%--            <SOE:BoundField 
                    DataField="VoucherSeriesTypeName" 
                    TermID="1507" DefaultTerm="Serienamn" 
                    Filterable="Numeric" Sortable="Numeric">
                </SOE:BoundField>--%>
                <SOE:BoundField 
                    DataField="YearFrom" 
                    TermID="1135" DefaultTerm="År från" 
                    Filterable="StartsWith" Sortable="Date">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="YearTo" 
                    TermID="1136" DefaultTerm="År till" 
                    Filterable="StartsWith" Sortable="Date">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="PeriodNr" 
                    TermID="1137" DefaultTerm="Period" 
                    Filterable="Numeric" Sortable="Numeric">
                </SOE:BoundField>
                <SOE:TemplateField Filterable="Contains" Sortable="Text">
                    <HeaderTemplate><%=GetText(1508, "Händelse")%></HeaderTemplate>
                    <ItemTemplate>
                        <asp:PlaceHolder ID="phEventText" runat="server"></asp:PlaceHolder>
                    </ItemTemplate>
                </SOE:TemplateField>  
<%--            <SOE:BoundField 
                    DataField="UserId" 
                    TermID="1140" DefaultTerm="Anv.Id" 
                    Filterable="Numeric" Sortable="Numeric">
                </SOE:BoundField>--%>
                <SOE:BoundField 
                    DataField="LoginName" 
                    TermID="1399" DefaultTerm="Anv.namn" 
                    Filterable="Contains" Sortable="Text">
                </SOE:BoundField>
                <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
	                <HeaderTemplate></HeaderTemplate>
                    <ItemTemplate>
                        <SOE:Link ID="Link1" runat="server"
	                        Href='<%# String.Format("../vouchers/?voucher={0}", Item.VoucherHeadId) %>'
	                        Alt='<%#GetText(1445, "Redigera")%>'
	                        ImageSrc='/img/edit.png'
	                        Permission="Readonly"
                            Feature="Economy_Accounting_Vouchers_Edit">
                        </SOE:Link>
                    </ItemTemplate>
                </SOE:TemplateField>   
		    </Columns>
	    </SOE:Grid>
    </div>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

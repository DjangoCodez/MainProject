<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.time.salary._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <form action="" id="formtitle">
        <div id="DivSubTitle" runat="server">
            <input value="<%=subtitle%>" readonly="readonly"/>        
        </div>        
    </form>
	<SOE:Grid ID="SoeGrid1" runat="server" AutoGenerateColumns="false">
		<Columns>          
            <SOE:BoundField 
                DataField="TimePeriodName" 
                TermID="5597" DefaultTerm="Period" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="TimePeriodStartDateString" 
                TermID="5598" DefaultTerm="Startdatum löneperiod" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>    
            <SOE:BoundField 
                DataField="TimePeriodStopDateString" 
                TermID="5599" DefaultTerm="Slutdatum löneperiod" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="XMLValue1" 
                TermID="5911" DefaultTerm="Utbetalningsdatum" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>
            <SOE:TemplateField Filterable="Contains" Sortable="Text">
                <HeaderTemplate><%=GetText(5600, "Lönespecifikation")%></HeaderTemplate>
                <ItemTemplate>
                    <asp:PlaceHolder ID="phSalarySpecification" runat="server"></asp:PlaceHolder>
                </ItemTemplate>
            </SOE:TemplateField>   
		</Columns>
	</SOE:Grid>    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
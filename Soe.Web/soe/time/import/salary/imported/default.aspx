<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.import.salary.imported._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <form action="" id="formtitle">
        <div id="DivSubTitle" runat="server">
            <input value="<%=subtitle%>" readonly="readonly"/>        
        </div>        
    </form>
	<SOE:Grid ID="SoeGrid1" ItemType="SoftOne.Soe.Common.DTO.DataStorageSmallDTO" runat="server" AutoGenerateColumns="false">
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
                DataField="NoOfChildrens" 
                TermID="5899" DefaultTerm="Antal lönespecar" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>
            <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
                <HeaderTemplate></HeaderTemplate>
                <ItemTemplate>
                    <SOE:Link ID="Link" runat="server"
                        Href='<%# String.Format("?datastorage={0}&delete=1", Item.DataStorageId) %>'
                        Alt='<%#GetText(5748, "Ta bort lönespecifikation")%>'
                        ImageSrc='<%#"/img/delete.png" %>'
                        Permission='Modify'
                        Feature='<%#SoftOne.Soe.Common.Util.Feature.Time_Import_Salary%>'>
                    </SOE:Link>
                </ItemTemplate>
            </SOE:TemplateField>
		</Columns>
	</SOE:Grid>    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
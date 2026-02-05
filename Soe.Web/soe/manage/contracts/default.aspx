<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.contracts._default" %>
<asp:Content ID="Content2" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Grid ID="SoeGrid1" ItemType="SoftOne.Soe.Data.License" runat="server" AutoGenerateColumns="false">
        <Columns>
            <SOE:BoundField 
                DataField="LicenseId" 
                TermID="1620" DefaultTerm="LicensId" 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField>  
            <SOE:BoundField 
                DataField="LicenseNr" 
                TermID="2014" DefaultTerm="Licensnr" 
                Filterable="Contains" Sortable="Numeric">
            </SOE:BoundField>  
            <SOE:BoundField 
                DataField="Name" 
                TermID="2136" DefaultTerm="Företagsnamn" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>   
            <SOE:BoundField 
                DataField="LegalName" 
                TermID="9150" DefaultTerm="Juridiskt namn" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>   
            <SOE:BoundField 
                DataField="OrgNr" 
                TermID="2137" DefaultTerm="Org nr" 
                Filterable="Contains" Sortable="Numeric">
            </SOE:BoundField>               
            <SOE:BoundField 
                DataField="SysServerUrl" 
                TermID="5125" DefaultTerm="Server" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="MaxNrOfUsers" 
                TermID="2015" DefaultTerm="Max anv." 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField> 
            <SOE:BoundField 
                DataField="MaxNrOfEmployees" 
                TermID="5196" DefaultTerm="Max anst." 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField> 
            <SOE:BoundField 
                DataField="ConcurrentUsers" 
                TermID="2016" DefaultTerm="Samt anv." 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField>            
            <SOE:BoundField 
                DataField="NrOfCompanies" 
                TermID="2017" DefaultTerm="Antal ftg" 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField> 
 		    <SOE:TemplateField>
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link ID="Link1" runat="server"
		                Href='<%# Item.CompaniesUrl%>'
                        Alt='<%#GetText(1613, "Visa företag")%>'
                        FontAwesomeIcon='fal fa-building'
                        Permission='Readonly'
                        Feature='Manage_Companies'>
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>                  
 		    <SOE:TemplateField>
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link runat="server"
		                Href='<%# Item.UsersUrl%>'
                        Alt='<%#GetText(1577, "Visa användare")%>'
                        FontAwesomeIcon='fal fa-user-friends'
                        Permission='Readonly'
                        Feature='Manage_Users'>
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>
 		    <SOE:TemplateField>
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link runat="server"
		                Href='<%# Item.EditUrl %>'
                        Alt='<%#GetText(1445, "Redigera")%>'
                        FontAwesomeIcon='fal fa-pencil'
                        Permission='Readonly'
                        Feature='Manage_Contracts_Edit'>
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>   
        </Columns>
    </SOE:Grid>
</asp:Content>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.licenses._default" %>
<asp:Content ID="Content2" ContentPlaceHolderID="soeMainContent" runat="server">
    <form action="" id="formtitle">
        <input value="<%=Headline%>" readonly="readonly" />  
    </form>
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
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField>  
            <SOE:BoundField 
                DataField="Name" 
                TermID="2136" DefaultTerm="Företagsnamn" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>             
            <SOE:BoundField 
                DataField="MaxNrOfUsers" 
                TermID="2015" DefaultTerm="Max anv." 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField> 
            <SOE:BoundField 
                DataField="ConcurrentUsers" 
                TermID="2016" DefaultTerm="Samt anv." 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField> 
            <SOE:BoundField 
                DataField="CurrentConcurrentUsers" 
                TermID="5145" DefaultTerm="Inlogg.anv." 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField>             
            <SOE:BoundField 
                DataField="NrOfCompanies" 
                TermID="2017" DefaultTerm="Antal ftg" 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField>             
 		    <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link ID="Link2" runat="server"
		                Href='<%# String.Format("users/?license={0}", Item.LicenseId) %>'
                        Alt='<%#GetText(5146, "Inloggade användare")%>'
                        FontAwesomeIcon='fal fa-user-friends'
                        Permission='Readonly'
                        Feature='Common'>
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>
 		    <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link ID="Link1" runat="server"
		                Href='<%# String.Format("/soe/manage/contracts/edit/?licenseNr={0}", Item.LicenseNr) %>'
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
<asp:Content ID="Content1" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

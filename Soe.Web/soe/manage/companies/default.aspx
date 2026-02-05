<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.companies._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    
    <%--<a href='<%#"companysetupwizard/?license=" + licenseId + "&licenseNr=" + licenseNr %>'>
        <img src="/img/wizard.png" alt="Skapa nytt företag" />
    </a>--%>
    <%--<asp:ImageButton runat="server" ImageUrl="/img/wizard.png" PostBackUrl='<%#"companysetupwizard/?license=" + licenseId + "&licenseNr=" + licenseNr %>' />--%>
	<SOE:Grid ID="SoeGrid1" ItemType="SoftOne.Soe.Data.Company" runat="server" AutoGenerateColumns="false">
		<Columns>            
            <SOE:BoundField 
                DataField="CompanyNr" 
                TermID="1411" DefaultTerm="Ftg nr" 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField>   
            <SOE:BoundField 
                DataField="Name" 
                TermID="2028" DefaultTerm="Namn" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>    
            <SOE:BoundField 
                DataField="ShortName" 
                TermID="1412" DefaultTerm="Kortnamn" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>     
            <SOE:BoundField 
                DataField="OrgNr" 
                TermID="2029" DefaultTerm="Org nr" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>   
            <SOE:BoundField 
                DataField="VatNr" 
                TermID="1494" DefaultTerm="VAT-nummer" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>  

 		    <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link ID="Link1" runat="server"
		                Href='<%# String.Format("../roles/?license={0}&licenseNr={1}&company={2}", licenseId, licenseNr, Item.ActorCompanyId)%>'
                        Alt='<%#GetText(1614, "Visa roller")%>'
                        FontAwesomeIcon='fal fa-user-tag'
                        Permission='Readonly'
                        Feature='Manage_Roles'>
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>
 		    <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link runat="server"
		                Href='<%# String.Format("../users/?license={0}&licenseNr={1}&company={2}", licenseId, licenseNr, Item.ActorCompanyId)%>'
                        Alt='<%#GetText(1577, "Visa användare")%>'
                        FontAwesomeIcon='fal fa-user-friends'
                        Permission='Readonly'
                        Feature='Manage_Users'>
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>
 		    <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link runat="server"
		                Href='<%# String.Format("../../../modalforms/ContactPersonGrid.aspx?actor={0}", + Item.ActorCompanyId)%>'
                        Alt='<%#GetText(1611, "Visa kontaktpersoner")%>'
                        FontAwesomeIcon='fal fa-male'
                        CssClass='PopLink'
                        Permission='Readonly'
                        Feature='Manage_Contactpersons'>
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>
            <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
                <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link ID="Link2" runat="server"
		                Href='<%# String.Format("companysetupwizard/?license={0}&licenseNr={1}&company={2}", licenseId, licenseNr, Item.ActorCompanyId) %>'
                        Alt='Wizard för att sätta upp ett nytt företag.'
                        FontAwesomeIcon='fal fa-wand-magic'
                        Permission="Readonly"
                        Feature="None">
		            </SOE:Link>
			    </ItemTemplate>
            </SOE:TemplateField>
 		    <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link ID="Link3" runat="server"
		                Href='<%# String.Format("/modalforms/RegFavorite.aspx?company={0}&remote=1", Item.ActorCompanyId) %>'
                        Alt='<%#GetText(5774, "Lägg till supportlogin som favorit")%>'
                        FontAwesomeIcon='fal fa-star'
                        Invisible='<%#!Convert.ToBoolean(Item.ShowSupportLogin)%>'
                        CssClass='PopLink'
                        Permission="Readonly"
                        Feature="None">
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>
 		    <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link runat="server"
		                Href='<%# String.Format("edit/remote/?company={0}&login=1", Item.ActorCompanyId) %>'
                        Alt='<%#GetText(1653, "Supportlogin")%>'
                        FontAwesomeIcon='fal fa-life-ring errorColor'
                        Invisible='<%#!Convert.ToBoolean(Item.ShowSupportLogin)%>'
                        Permission="Readonly"
                        Feature="None">
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>
            <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
			    <HeaderTemplate></HeaderTemplate>
			    <ItemTemplate>
				    <SOE:Link ID="Link4" runat="server"
		                Href='<%# String.Format("edit/remote/?company={0}&login=1&super=true", Item.ActorCompanyId) %>'
                        Alt='<%#GetText(7264, "Superlogin")%>'
                        FontAwesomeIcon='far fa-exclamation errorColor'
                        Invisible='<%#!HasValidLicenseToSuperSupportLogin%>'
                        Permission="Readonly"
                        Feature="None">
		            </SOE:Link>
			    </ItemTemplate>
		    </SOE:TemplateField>
		    <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
	            <HeaderTemplate></HeaderTemplate>
	            <ItemTemplate>
		            <SOE:Link runat="server"
			            Href='<%# String.Format("edit/?license={0}&licenseNr={1}&company={2}", licenseId, licenseNr, Item.ActorCompanyId) %>'
	                    Alt='<%#GetText(1445, "Redigera")%>'
                        FontAwesomeIcon='fal fa-pencil'
	                    Permission='Readonly'
                        Feature='Manage_Companies_Edit'>
		            </SOE:Link>
	            </ItemTemplate>
            </SOE:TemplateField> 
		</Columns>
	</SOE:Grid>    
</asp:Content>


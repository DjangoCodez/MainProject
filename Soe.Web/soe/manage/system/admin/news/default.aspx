<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.news._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
 	<SOE:Grid ID="SoeGrid1" ItemType="SoftOne.Soe.Data.SysNews" runat="server" AutoGenerateColumns="false">
		<Columns>            
            <SOE:BoundField 
                DataField="PubDate" 
                TermID="5185" DefaultTerm="Datum" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>   
            <SOE:BoundField 
                DataField="Title" 
                TermID="5186" DefaultTerm="Rubrik" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>    
		    <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
	            <HeaderTemplate></HeaderTemplate>
	            <ItemTemplate>
		            <SOE:Link ID="Link1" runat="server"
			            Href='<%# String.Format("edit/?newsId={0}", Item.SysNewsId) %>'
	                    Alt='<%#GetText(1445, "Redigera")%>'
                        FontAwesomeIcon='fal fa-pencil'
	                    Permission='Readonly'
                        Feature='Common'>
		            </SOE:Link>
	            </ItemTemplate>
            </SOE:TemplateField> 
            <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
                <HeaderTemplate></HeaderTemplate>
                <ItemTemplate>
                    <SOE:Link ID="Link2" runat="server"
                        Href='<%# String.Format("?newsId={0}&delete=1", Item.SysNewsId) %>'
                        Alt='<%#GetText(2185, "Ta bort")%>'
                        FontAwesomeIcon='fal fa-times errorColor'
                        Permission='Modify'
                        Feature='Common'>
                    </SOE:Link>
                </ItemTemplate>
            </SOE:TemplateField>
		</Columns>
	</SOE:Grid>    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

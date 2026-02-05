<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FormSettings.aspx.cs" Inherits="SoftOne.Soe.Web.Settings.FormSettings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div>
        <a href="<%=RedirectUrl %>"><%= GetText(1549, "Gå tillbaka")%></a>
    </div>
    <br />
    <SOE:Grid ID="SoeGrid1" ItemType="SoftOne.Soe.Business.Util.SettingObject" runat="server" AutoGenerateColumns="false">
        <Columns>
            <SOE:BoundField 
                DataField="FieldId" 
                TermID="1111" DefaultTerm="Fält" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="FieldName" 
                TermID="1133" DefaultTerm="Fältnamn" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>
            <SOE:TemplateField Filterable="Contains" Sortable="Text">
                <HeaderTemplate><%=GetText(1112, "Dimension")%></HeaderTemplate>
                <ItemTemplate>
                    <asp:PlaceHolder ID="phDimension" runat="server"></asp:PlaceHolder>
                </ItemTemplate>
            </SOE:TemplateField> 
            <SOE:TemplateField Filterable="Contains" Sortable="Text">
                <HeaderTemplate><%=GetText(1113, "Inställning")%></HeaderTemplate>
                <ItemTemplate>
                    <asp:PlaceHolder ID="phSetting" runat="server"></asp:PlaceHolder>
                </ItemTemplate>
            </SOE:TemplateField>  
            <SOE:TemplateField Filterable="Contains" Sortable="Text">
                <HeaderTemplate><%=GetText(1114, "Data")%></HeaderTemplate>
                <ItemTemplate>
                    <asp:PlaceHolder ID="phValue" runat="server"></asp:PlaceHolder>
                </ItemTemplate>
            </SOE:TemplateField>  
            <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
                <HeaderTemplate></HeaderTemplate>
                <ItemTemplate>
                    <SOE:Link runat="server"		                
                        Href='<%# String.Format("/settings/FieldSettings.aspx?form={0}&field={1}&tab={2}", + Item.FormId, Item.FieldId, Item.ActorCompanyId.HasValue ? 2 : 1) %>'
                        Alt='<%#GetText(1445, "Redigera")%>'
                        FontAwesomeIcon='fal fa-pencil'
	                    Permission='Readonly'
                        Feature='<%# Item.ActorCompanyId.HasValue ? SoftOne.Soe.Common.Util.Feature.Common_Field_Company : SoftOne.Soe.Common.Util.Feature.Common_Field_Role %>'> 
                    </SOE:Link>
                </ItemTemplate>
            </SOE:TemplateField>   
            <SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
                <HeaderTemplate></HeaderTemplate>
                <ItemTemplate>
                    <SOE:Link ID="Link2" runat="server"
                        Href='<%# String.Format("?form={0}&field={1}&company={2}&role={3}&delete=1", Item.FormId, Item.FieldId, Item.ActorCompanyId, Item.RoleId) %>'
                        Alt='<%#GetText(1401, "Ta bort inställning")%>'
                        FontAwesomeIcon='fal fa-times errorColor'
                        Permission='Modify'
                        Feature='<%# Item.ActorCompanyId.HasValue ? SoftOne.Soe.Common.Util.Feature.Common_Field_Company : SoftOne.Soe.Common.Util.Feature.Common_Field_Role %>'>
                    </SOE:Link>
                </ItemTemplate>
            </SOE:TemplateField>
         </Columns>
    </SOE:Grid>
</asp:Content>

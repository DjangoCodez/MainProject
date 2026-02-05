<%@ Page EnableViewState="true" Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.features._default" %>
<%@ Register Src="~/UserControls/SoeFormPrefix.ascx" TagPrefix="SOE" TagName="SoeFormPrefix" %>
<%@ Register Src="~/UserControls/SoeFormPostfix.ascx" TagPrefix="SOE" TagName="SoeFormPostfix" %>
<%@ Register Src="~/UserControls/SoeFormFooter.ascx" TagPrefix="SOE" TagName="SoeFormFooter" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <form id="form1" runat="server">
        <SOE:SoeFormPrefix ID="SoeFormPrefix" runat="server"></SOE:SoeFormPrefix>
        <div>
            <table>
                <tr>
                    <td style="vertical-align: top">
                        <fieldset>
                            <legend><%=GetText(5153, "Kopiera från")%></legend>
                            <table cellspacing="5px">
                                <thead valign="top">
                                    <tr>
                                        <td style="width:200px">
                                            <nobr><span class="formText">a)</span> <asp:Label id="DefinePermissionManuallyLabel" Text="Definera manuellt" CssClass="formText" runat="server" /></nobr>
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="DefinePermissionManually" Text="" AutoPostBack="true" runat="server" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <nobr><span class="formText">b)</span> <label id="SourceLicensesLabel" for="SourceLicense"><%=GetText(5154, "Licens")%></label></nobr>
                                        </td>
                                        <td>
                                            <asp:DropDownList 
                                                ID="SourceLicenses" 
                                                AutoPostBack="true"
                                                runat="server">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <nobr><span class="formText">c)</span> <label id="SourceArticlesLabel" for="SourceArticle"><%=GetText(5173, "SoftOne Artikel")%></label></nobr>
                                        </td>
                                        <td>
                                            <asp:DropDownList 
                                                ID="SourceArticles" 
                                                AutoPostBack="true"
                                                runat="server">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2" valign="middle">
                                            <hr />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <label id="ModulesLabel" for="Modules"><%=GetText(1920, "Modul")%></label>
                                        </td>
                                        <td>
                                            <asp:DropDownList 
                                                ID="Modules" 
                                                AutoPostBack="true"
                                                runat="server">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <label id="PermissionLabel" for="Modules"><%=GetText(5156, "Behörighet")%></label>
                                        </td>
                                        <td>
                                            <asp:RadioButtonList ID="SourceLicensePermission" RepeatLayout="Table" RepeatDirection="Horizontal" AutoPostBack="true" runat="server">
                                                <asp:ListItem Text="ReadOnly" Value="ReadOnly"></asp:ListItem>
                                                <asp:ListItem Text="Modify" Value="Modify" Selected="True"></asp:ListItem>
                                            </asp:RadioButtonList>   
                                        </td>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr id="FeatureTreeInfo" runat="server">
                                        <td colspan="2">
                                            <span class="instruction">
                                                <%=GetText(5172, "Endast möjligt att ändra i trädet när 'Definera manuellt' är valt")%>
                                                .&nbsp;<br />
                                                <%=GetText(5318, "Annars visas trädet bara i Read-Only mode")%>
                                            </span>
                                        </td>
                                    </tr>
                                    <tr id="FeatureTreeSettings" runat="server">
                                        <td colspan="2">
                                            <input type="checkbox" id="AlternativeCheck" checked="checked"/>
                                            <label for="checkbox"><%=GetText(4272, "Kryssa inte ur eller i underliggande behörigheter")%></label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <SOE:FeatureTreeView runat="server"
                                                ID="FeatureTree" 
                                                ShowCheckBoxes="All"
                                                onclick="OnTreeNodeChecked(event)"
                                                BackColor="#F8F8F8">
                                                <LevelStyles>
                                                    <asp:TreeNodeStyle ForeColor="Black" Font-Bold="true" /> 
                                                    <asp:TreeNodeStyle ForeColor="Black" Font-Bold="false" /> 
                                                    <asp:TreeNodeStyle ForeColor="Black" Font-Bold="false" /> 
                                                </LevelStyles>
                                            </SOE:FeatureTreeView>   
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </fieldset>
                    </td>
                    <td style="vertical-align: top">
                        <fieldset>
                            <legend><%=GetText(5155, "Kopiera till")%></legend>
                            <table cellspacing="5px">
                                <thead valign="top">
                                    <tr>
                                        <td style="width:200px">
                                            <nobr><span class="formText">a)</span> <asp:Label id="DestinationLicensesAllLabel" Text="Alla licenser" CssClass="formText" runat="server" /></nobr>
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="DestinationLicensesAll" Text="" AutoPostBack="true" Checked="false" runat="server" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <nobr><span class="formText">b)</span> <label id="DestinationLicensesLabel" for="LicenseDestination"><%=GetText(5154, "Licens")%></label></nobr>
                                        </td>
                                        <td>
                                            <asp:DropDownList 
                                                ID="DestinationLicenses" 
                                                AutoPostBack="true"
                                                runat="server">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <nobr><span class="formText">c)</span> <label id="LabelDependencyFeature" for="DependancyFeature"><%=GetText(5319, "Alla som har behörighet")%></label></nobr>
                                        </td>
                                        <td>
                                            <asp:DropDownList 
                                                ID="DependancyFeature" 
                                                AutoPostBack="true"
                                                runat="server">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>&nbsp;</td>
                                        <td>
                                            <table class="slim">
                                                <tr>
                                                    <td>
                                                        <asp:CheckBox ID="DependancyFeatureOnlyLicenses" Text="Kopiera endast till licenser" AutoPostBack="true" Checked="false" runat="server" /><br />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:CheckBox ID="DependancyFeatureOnlyLicensesAndCompanies" Text="Kopiera endast till licenser och företag" AutoPostBack="true" Checked="false" runat="server" /><br />
                                                    </td>
                                                </tr>
                                            </table>
                                      </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <asp:RadioButtonList ID="DependencyFeaturePermission" RepeatLayout="Table" RepeatDirection="Horizontal" runat="server">
                                                <asp:ListItem Text="ReadOnly" Value="ReadOnly"></asp:ListItem>
                                                <asp:ListItem Text="Modify" Value="Modify" Selected="True"></asp:ListItem>
                                            </asp:RadioButtonList>   
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <fieldset>
                                                <legend><%=GetText(5157, "Inställningar")%></legend>
                                                <table class="slim">
                                                    <tr>
                                                        <td>
                                                            <asp:CheckBox ID="DestinationLicenseAddNew" Text="Lägg till nya" Checked="true" runat="server" /><br />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:CheckBox ID="DestinationLicensePromoteExisting" Text="Uppgradera befintliga" Checked="false" runat="server" /><br />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:CheckBox ID="DestinationLicenseDegradeExisting" Text="Degradera befintliga" Checked="false" runat="server" /><br />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:CheckBox ID="DestinationLicenseDeleteLeftOvers" Text="Ta bort överblivna" Checked="false" runat="server" /><br />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </fieldset>
                                        </td>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td colspan="2" align="right">
                                            <asp:Button ID="btnCopyTop" runat="server"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <SOE:CompanyRoleTreeView runat="server"
                                                ID="CompanyRoleTree" 
                                                ShowCheckBoxes="All"
                                                onclick="OnTreeNodeChecked(event)"
                                                BackColor="#F8F8F8">
                                                <LevelStyles>
                                                    <asp:TreeNodeStyle ForeColor="Black" Font-Bold="true" Font-Italic="false" /> 
                                                    <asp:TreeNodeStyle ForeColor="Black" Font-Bold="false" Font-Italic="false" /> 
                                                    <asp:TreeNodeStyle ForeColor="Black" Font-Bold="false" Font-Italic="true" /> 
                                                </LevelStyles>
                                            </SOE:CompanyRoleTreeView>  
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2" align="left">
                                            <asp:Button ID="btnCopyBottom" runat="server"/>
                                        </td>
                                    </tr>
                               </tbody>
                            </table>
                        </fieldset>
                    </td>
                </tr>
            </table>
        </div>
	    <SOE:SoeFormPostfix ID="SoeFormPostfix" runat="server"></SOE:SoeFormPostfix>
	    <SOE:SoeFormFooter ID="SoeFormFooter" runat="server"></SOE:SoeFormFooter>
    </form> 
    <SOE:Grid ID="SoeGrid1" Visible="false" runat="server" AutoGenerateColumns="false">
        <Columns>
            <SOE:BoundField 
                DataField="Type" 
                TermID="5160" DefaultTerm="Typ" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="Name" 
                TermID="5162" DefaultTerm="Namn" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="LicenseNr" 
                TermID="2014" DefaultTerm="Licensnr" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="LicenseId" 
                TermID="5161" DefaultTerm="LicenseId" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="ActorCompanyId" 
                TermID="5306" DefaultTerm="ActorCompanyId" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="CompanyNr" 
                TermID="1411" DefaultTerm="Ftg nr" 
                Filterable="Numeric" Sortable="Numeric">
            </SOE:BoundField>   
            <SOE:BoundField 
                DataField="RoleId" 
                TermID="5307" DefaultTerm="RoleId" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="FeaturesAdded" 
                TermID="5163" DefaultTerm="Tillagda" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="FeaturesPromoted" 
                TermID="5170" DefaultTerm="Uppgraderade" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="FeaturesDegraded" 
                TermID="5164" DefaultTerm="Degraderade" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField>
            <SOE:BoundField 
                DataField="FeaturesDeleted" 
                TermID="5165" DefaultTerm="Borttagna" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField> 
            <SOE:BoundField 
                DataField="FeaturesIgnored" 
                TermID="5171" DefaultTerm="Ignorerade" 
                Filterable="Contains" 
                Sortable="Text">
            </SOE:BoundField> 
         </Columns>
    </SOE:Grid>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

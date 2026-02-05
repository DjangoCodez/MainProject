<%@ Control EnableViewState="true" Language="C#" AutoEventWireup="true" CodeBehind="FeaturePermissionTree.ascx.cs" Inherits="SoftOne.Soe.Web.soe.FeaturePermissionTree" %>
<%@ Register Src="~/UserControls/SoeFormPrefix.ascx" TagPrefix="SOE" TagName="SoeFormPrefix" %>
<%@ Register Src="~/UserControls/SoeFormPostfix.ascx" TagPrefix="SOE" TagName="SoeFormPostfix" %>
<%@ Register Src="~/UserControls/SoeFormFooter.ascx" TagPrefix="SOE" TagName="SoeFormFooter" %>
<form id="form1" runat="server">
    <SOE:SoeFormPrefix ID="SoeFormPrefix" runat="server"></SOE:SoeFormPrefix>
    <div>
        <fieldset>
            <legend><%=PageBase.GetText(5397, "Behörigheter per modul")%></legend>
            <table>
                <thead>
                    <tr id="ArticlesRow" runat="server">
                        <td>
                            <label id="ArticlesLabel" for="Modules"><%=PageBase.GetText(1995, "SoftOne Artikel")%></label>
                        </td>
                        <td>
                            <asp:DropDownList 
                                ID="Articles" 
                                EnabelViewState="true"
                                AutoPostBack="true"
                                Width="150"
                                runat="server">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label id="ModulesLabel" for="Modules"><%=PageBase.GetText(1920, "Modul")%></label>
                        </td>
                        <td>
                            <asp:DropDownList 
                                ID="Modules" 
                                EnabelViewState="true"
                                AutoPostBack="true"
                                Width="150"
                                runat="server">
                            </asp:DropDownList>
                            <SOE:Instruction 
                                ID="Instruction1"
                                TermID="1921" DefaultTerm="Spara ändringar innan du byter modul"
                                FitInTable="true"
                                runat="server">
                            </SOE:Instruction>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <input type="checkbox" id="AlternativeCheck" checked="checked"/>
                            <label for="checkbox" class="adjustToCell"><%=PageBase.GetText(4272, "Kryssa inte ur eller i underliggande behörigheter")%></label>
                        </td>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td colspan="2">
                            <SOE:FeatureTreeView runat="server"
                                ID="FeatureTree" 
                                EnabelViewState="true"
                                ShowCheckBoxes="All"
                                onclick="OnTreeNodeChecked(event)"
                                BackColor="#F5F5F5">
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
    </div>
    <SOE:SoeFormPostfix ID="SoeFormPostfix" runat="server"></SOE:SoeFormPostfix>
    <SOE:SoeFormFooter ID="SoeFormFooter" runat="server"></SOE:SoeFormFooter>
</form>
<%@ Page EnableViewState="true" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" ValidateRequest="false" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.news.edit._default" %>
<%@ Register Src="~/UserControls/SoeFormPrefix.ascx" TagPrefix="SOE" TagName="SoeFormPrefix" %>
<%@ Register Src="~/UserControls/SoeFormPostfix.ascx" TagPrefix="SOE" TagName="SoeFormPostfix" %>
<%@ Register Src="~/UserControls/SoeFormFooter.ascx" TagPrefix="SOE" TagName="SoeFormFooter" %>
<%@ Register TagPrefix="FTB" Namespace="FreeTextBoxControls" Assembly="FreeTextBox" %>  
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">   
    <form id="Form1" runat="server">
	    <SOE:SoeFormPrefix ID="SoeFormPrefix" runat="server"></SOE:SoeFormPrefix>
	    <div>
            <fieldset>
                <legend><%=GetText(5396, "Nyhet") %></legend>
                <div class="col">                
                    <table cellpadding="2" cellspacing="2">
                        <tr>
                            <td style="vertical-align:middle">
                                <label><%=GetText(5189, "Skrivet av") %></label>
                            </td>
                            <td>
                                <asp:TextBox ID="Author" Width="300" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:middle">
                                <label><%=GetText(5177, "Rubrik") %></label>
                            </td>
                            <td>
                                <asp:TextBox ID="NewsTitle" Width="300" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <label><%=GetText(8038, "Kort beskrivning")%></label>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox ID="Preview" Width="380" Rows="5" TextMode="MultiLine" runat="server" />
                            </td>
                        </tr>
                    </table> 
                </div>
                <div>
                    <table>
                        <tr>
                            <td style="vertical-align:middle">
                                <label><%=GetText(5181, "Synlig för alla")%></label>
                            </td>
                            <td>
                                <asp:CheckBox ID="IsPublic" AutoPostBack="true" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:middle">
                                <label><%=GetText(5190, "SoftOne Artikel") %></label>
                            </td>
                            <td>
                                <asp:DropDownList ID="SysXEArticles" Width="300" runat="server"></asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:middle">
                                <label><%=GetText(7144, "Visningstyp") %></label>
                            </td>
                            <td>
                                <asp:DropDownList ID="SysNewsDisplayType" Width="300" runat="server"></asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:middle">
                                <label><%=GetText(8206, "Språk") %></label>
                            </td>
                            <td>
                                <asp:DropDownList ID="SysLang" Width="300" runat="server"></asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td style="vertical-align:middle">
                                <label><%=GetText(5179, "Bifogad fil")%></label>
                            </td>
                            <td>
                                <asp:FileUpload ID="Attachment" Width="300" runat="server" />
                                <asp:Label ID="AttachmentFileName" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="clear">
                    <table>
                        <tr>
                            <td colspan="2">
                                <label><%=GetText(5945, "Beskrivning")%></label>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <FTB:FreeTextBox id="Description" Width="800" runat="server" />  
                            </td>
                        </tr>
                    </table> 
                </div>
            </fieldset>
	    </div>

	    <SOE:SoeFormPostfix ID="SoeFormPostfix" runat="server"></SOE:SoeFormPostfix>
	    <SOE:SoeFormFooter ID="SoeFormFooter" EnableDelete="true" runat="server"></SOE:SoeFormFooter>
    </form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
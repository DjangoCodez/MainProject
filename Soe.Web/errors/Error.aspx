<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="SoftOne.Soe.Web.errors.Error" %>
<%@ Register Src="~/UserControls/SupportContactInfo.ascx" TagPrefix="SOE" TagName="SupportContactInfo" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div class="error">
        <h1><%= GetText(1148, "Ett fel inträffade") %></h1>        
        <p class="description">
            <%= GetText(1151, "Vi ber om ursäkt för det") %>
            <%= Enc(". ") %><a href="javascript:history.go(-1)"><%= GetText(1549, "Gå tillbaka")%></a>
            <%= Enc(" ") %><%= GetText(5475, "och försök igen. Om felet kvarstår kan du") %>
        </p>    
        <ul id="ArrangementList" runat="server">
            <li>
                <SOE:SupportContactInfo ID="SupportContactInfo" Runat="Server"></SOE:SupportContactInfo>
            </li>
            <li id="LogEntry" runat="server">
                <span><%= GetText(5479, "Se logg för detaljerad information")%></span>
                <div class="details">
		            <SOE:Link ID="LinkLogEntry" runat="server"
	                    Permission='Readonly'
                        Feature='Manage_Support_Logs_Edit'>
		            </SOE:Link>
                </div>
            </li>
        </ul>
    </div>
</asp:Content>


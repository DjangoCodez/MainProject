<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RemoteLoginFailed.aspx.cs" Inherits="SoftOne.Soe.Web.errors.RemoteLoginFailed" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div class="error">
        <h1><%= GetText(5999, "Supportinlogg misslyckades")%></h1>
        <p class="description">
            <%= RemoteLoginFailedMessage%> <br /><br />
        </p>    
        <p>
		    <SOE:Link ID="LinkTo2ndServer" runat="server"
	            Permission='Readonly'
                Feature='None'>
		    </SOE:Link>
        </p>    
    </div>
</asp:Content>


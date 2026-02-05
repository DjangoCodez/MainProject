<%@ Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.users.edit.sessions._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Grid ID="SoeGrid1" runat="server" AutoGenerateColumns="false">
        <Columns>
            <SOE:BoundField 
                DataField="Login" 
                TermID="1692" DefaultTerm="Inlogg" 
                Filterable="Contains" Sortable="Date">
            </SOE:BoundField>  
            <SOE:BoundField 
                DataField="Logout" 
                TermID="1693" DefaultTerm="Utlogg" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>    
            <SOE:BoundField 
                DataField="RemoteLogin" 
                TermID="1694" DefaultTerm="Supportlogin" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField> 
            <SOE:BoundField 
                DataField="MobileLogin" 
                TermID="5677" DefaultTerm="Mobillogin" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>   
            <SOE:BoundField 
                DataField="Description" 
                TermID="1695" DefaultTerm="Beskrivning" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>  
            <SOE:BoundField 
                DataField="CacheCredentials" 
                TermID="5744" DefaultTerm="Id" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>  
        </Columns>
    </SOE:Grid>
</asp:Content>


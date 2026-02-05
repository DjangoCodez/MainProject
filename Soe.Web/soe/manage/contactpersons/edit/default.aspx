<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.contactpersons.edit._default" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register TagName="ActorContactPerson" TagPrefix="SOE" Src="~/UserControls/ActorContactPerson.ascx" %>
<asp:Content ID="Content2" ContentPlaceHolderID="soeMainContent" runat="server">    
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
                <SOE:ActorContactPerson ID="ActorContactPerson" Type="Company" Runat="Server"></SOE:ActorContactPerson>
			</SOE:Tab>			
		</Tabs>
	</SOE:Form> 
    <%if (contactPerson != null)
    {%>
        <form action="" id="formtitle">
            <input value="<%=GetText(1703, "Kopplade aktörer")%>" readonly="readonly" />  
        </form>
    <%}%>
    <SOE:Grid ID="SoeGrid1" runat="server" AutoGenerateColumns="false">
        <Columns>
            <SOE:BoundField 
                DataField="Name" 
                TermID="1701" DefaultTerm="Namn" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>    
            <SOE:BoundField 
                DataField="TypeName" 
                TermID="1702" DefaultTerm="Aktörstyp" 
                Filterable="Contains" Sortable="Text">
            </SOE:BoundField>    
        </Columns>
    </SOE:Grid>  
</asp:Content>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ActorContactPerson.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.ActorContactPerson" %>
<%@ Register TagName="ActorECom" TagPrefix="SOE" Src="~/UserControls/ActorECom.ascx" %>
<%@ Register Src="~/UserControls/ActorContactAddressList.ascx" TagPrefix="SOE" TagName="ActorContactAddressList" %>
<div>
    <fieldset> 
        <legend><%=PageBase.GetText(1618, "Kontaktuppgifter") %></legend>
        <table>		                      
            <SOE:TextEntry 
	            ID="FirstName" 
	            TermID="1598" DefaultTerm="Förnamn" 
	            Validation="Required"
                InvalidAlertTermID="1599" InvalidAlertDefaultTerm="Du måste ange förnamn"
	            MaxLength="100" 
	            runat="server">
	        </SOE:TextEntry>
            <SOE:TextEntry 
	            ID="LastName" 
	            TermID="1600" DefaultTerm="Efternamn" 
	            Validation="Required"
                InvalidAlertTermID="1601" InvalidAlertDefaultTerm="Du måste ange efternamn"
	            MaxLength="100" 
	            runat="server">
	        </SOE:TextEntry>	
		    <SOE:SelectEntry 
		        ID="Position" 
		        TermID="1602" DefaultTerm="Position"
		        runat="server" >
		    </SOE:SelectEntry>
            <SOE:TextEntry
			    ID="SocialSec"
			    TermID="5572" DefaultTerm="Personnummer"
			    MaxLength="100"
			    runat="server">
		    </SOE:TextEntry>
		    <SOE:SelectEntry 
		        ID="Sex" 
		        TermID="3778" DefaultTerm="Kön"
		        runat="server" >
		    </SOE:SelectEntry>
	    </table>
    </fieldset>					
</div>	
<div>
    <SOE:ActorECom ID="ActorECom" Runat="Server"></SOE:ActorECom>
    <SOE:ActorContactAddressList ID="ActorContactAddressList" Type="Company" Runat="Server" />
</div>
	
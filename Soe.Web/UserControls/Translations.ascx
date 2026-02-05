<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Translations.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.Translations" %>
<fieldset>
	<legend><%=PageBase.GetText(4727, "Översättningar")%></legend>
	<table>
        <SOE:SelectEntry
            ID="TranslationCountry"
            TermID="1737" DefaultTerm="Land"
            InvalidAlertTermID="7166" InvalidAlertDefaultTerm="Du måste ange land"
            Visible ="true"
            runat="server">
        </SOE:SelectEntry>
        <SOE:TextEntry   
			ID="TranslationName"						            
			TermID="4726" DefaultTerm="Översättning"						        
			InvalidAlertTermID="91933" InvalidAlertDefaultTerm="Du måste ange översättning"
            Visible ="true"
			MaxLength="100"
			Width="250"
			runat="server">
		</SOE:TextEntry>	
        <tr>
            <td>
                &nbsp;
            </td>
            <td>
                <button type="submit" name="AddTranslation" id="AddTranslation"><%=PageBase.GetText(1438, "Lägg till översättning")%></button>
			</td>
        </tr>                               
    </table>                           
    <table id="ExistingTranslations" runat="server"></table>	                 
</fieldset>     
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.paycondition._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">
			    <div>
				    <fieldset> 
					    <legend><%=GetText(5408, "Betalningsvillkoruppgifter")%></legend>
		                <table>
			        <SOE:TextEntry 
				        ID="Code"
				        TermID="3083" DefaultTerm="Kod"
				        Validation="Required"
				        InvalidAlertTermID="3087" InvalidAlertDefaultTerm="Du måste ange kod"
				        MaxLength="20"
				        runat="server">
			        </SOE:TextEntry>	
			        <SOE:TextEntry 
				        ID="Name"
				        TermID="3084" DefaultTerm="Namn"
				        Validation="Required"
				        InvalidAlertTermID="1041" InvalidAlertDefaultTerm="Du måste ange namn"
				        MaxLength="50"
				        runat="server">
			        </SOE:TextEntry>	
			        <SOE:NumericEntry 
				        ID="Days"
				        TermID="3085" DefaultTerm="Dagar"
				        Validation="Required"
				        InvalidAlertTermID="3088" InvalidAlertDefaultTerm="Du måste ange dagar"
				        runat="server">
			        </SOE:NumericEntry>	
			        <SOE:NumericEntry 
				        ID="DiscountDays"
				        TermID="4577" DefaultTerm="Rabattdagar"
				        runat="server">
			        </SOE:NumericEntry>	
			        <SOE:NumericEntry 
				        ID="DiscountPercent"
				        TermID="4578" DefaultTerm="Rabattprocent"
				        runat="server">
			        </SOE:NumericEntry>	
                </table>
                    </fieldset>
                </div>
			</SOE:Tab>
		</tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.daytypes.edit._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">
                <div>
                    <fieldset>
                        <legend><%=GetText(5395, "Dagtyp")%></legend>
		                <table>
			                <SOE:TextEntry 
				                ID="Name"
				                TermID="3070" DefaultTerm="Namn"
				                Validation="Required"
				                InvalidAlertTermID="1041" InvalidAlertDefaultTerm="Du måste ange namn"
				                MaxLength="255"
				                runat="server">
			                </SOE:TextEntry>	
			                <SOE:TextEntry 
				                ID="Description"
				                TermID="4003" DefaultTerm="Beskrivning"
				                MaxLength="255"
				                runat="server">
			                </SOE:TextEntry>	
                            <SOE:SelectEntry
                                ID="FromDayType"
                                DefaultTerm="Från och med veckodag"
                                TermId="4531"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="ToDayType"
                                DefaultTerm="Till och med veckodag"
                                TermId="4532"
                                runat="server">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
			</SOE:Tab>
		</tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

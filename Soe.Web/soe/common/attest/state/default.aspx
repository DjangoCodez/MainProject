<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.attest.state._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true"
        runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">
			    <div>
			        <div>
					    <fieldset> 
						    <legend><%=GetText(3312, "Nivå")%></legend>
				            <table>
                                <SOE:SelectEntry
                                    ID="Entity"
                                    TermID="3325"
                                    DefaultTerm="Typ"
                                    Width="156"
                                    runat="server">
                                </SOE:SelectEntry>
					            <SOE:TextEntry
					                runat="server"  
						            ID="Name"
						            Validation="Required"
						            TermID="3313" DefaultTerm="Namn"
						            InvalidAlertTermID="44" InvalidAlertDefaultTerm="Du måste ange namn"
                                    Width="150"
						            MaxLength="100">
					            </SOE:TextEntry>	
					            <SOE:TextEntry 
					                runat="server"
						            ID="Description"
						            TermID="3324" DefaultTerm="Beskrivning"
						            Width="400"
						            MaxLength="512">
					            </SOE:TextEntry>
                                <SOE:NumericEntry
                                    ID="Sort" 
                                    TermID="3326"
                                    DefaultTerm="Sortering" 
                                    Width="30"
                                    MaxLength="3"
                                    AllowDecimals="false"
                                    AllowNegative="false"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:CheckBoxEntry
                                    ID="Initial" 
                                    TermID="3327"
                                    DefaultTerm="Startnivå"
                                    OnClick="enableDisable()"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <tr>
                                    <td colspan="2">
                                        <SOE:Instruction ID="InitialInstruction" FitInTable="true"
                                            TermID="3328" DefaultTerm="Endast en nivå kan markeras som startnivå."
                                            runat="server">
                                        </SOE:Instruction>
                                    </td>
                                </tr>
                                <SOE:CheckBoxEntry
                                    ID="Closed" 
                                    TermID="9003"
                                    DefaultTerm="Stängnivå"
                                    OnClick="enableDisable()"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="Hidden" 
                                    TermID="3908"
                                    DefaultTerm="Dold"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="Locked" 
                                    TermID="7176"
                                    DefaultTerm="Låst"
                                    runat="server">
                                </SOE:CheckBoxEntry>
					            <SOE:TextEntry 
					                runat="server"
						            ID="Color"
						            CssClass="color"
						            TermID="5279" DefaultTerm="Färg"
						            InvalidAlertTermID="44" InvalidAlertDefaultTerm="Du måste ange färg"
						            Width="100"
						            MaxLength="10">
					            </SOE:TextEntry>
					            <SOE:TextEntry
					                runat="server"  
						            ID="ImageSource"
						            TermID="3978" DefaultTerm="Ikonnamn"
                                    Width="200"
						            MaxLength="100">
					            </SOE:TextEntry>	
				            </table>
				        </fieldset>
			        </div>
                </div>
			</SOE:Tab>
		</tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

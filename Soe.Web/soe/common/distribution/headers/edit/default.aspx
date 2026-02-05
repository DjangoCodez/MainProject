<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.distribution.headers.edit._default" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
				<div>
					<fieldset> 
						<legend><%=GetText(2215, "Rubrik")%></legend>
						<table>														
							<SOE:TextEntry 
						        ID="Name"
						        Validation="Required"
						        TermID="2216" DefaultTerm="Rubrik"
						        InvalidAlertTermID="2217" InvalidAlertDefaultTerm="Du måste ange en rubrik"
						        MaxLength="100"
						        Width="200"
						        runat="server">
					        </SOE:TextEntry>
						    <SOE:TextEntry   
					            ID="Description"						            
					            TermID="1545" DefaultTerm="Beskrivning"						        
					            MaxLength="255"
					            Width="200"
					            runat="server">
					        </SOE:TextEntry>
					        <SOE:SelectEntry 
					            runat="server" 
					            ID="TemplateType" 
					            Width="200"
					            TermID="2251" DefaultTerm="Rapporttyp">
					        </SOE:SelectEntry>	
							<SOE:CheckBoxEntry 
                                ID="ShowRow" 
                                TermID="2230" DefaultTerm="Visa rader"                                
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="ShowZeroRow" 
                                TermID="2240" DefaultTerm="Visa nollrader"                                
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="ShowLabel" 
                                TermID="2231" DefaultTerm="Visa rubrik"                                
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="ShowSum" 
                                TermID="2232" DefaultTerm="Visa summa"                                
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="DoNotSummerizeOnGroup" 
                                TermID="9181" DefaultTerm="Summera inte på rapportgrupp"                                
                                runat="server">
                            </SOE:CheckBoxEntry>		
                            <SOE:CheckBoxEntry 
                                ID="InvertRow" 
                                TermID="2241" DefaultTerm="Vänd tecken"                                                                
                                runat="server">
                            </SOE:CheckBoxEntry>											        				        							
						</table>
					</fieldset>
			    </div>
			    <div>
					<fieldset>
                        <legend><%=GetText(2191, "Intervaller")%></legend>
                        <table>
			                <SOE:FormIntervalEntry
                                    ID="Interval"                                
                                    NoOfIntervals="100"
                                    DisableHeader="false"
                                    DisableSettings="true"
                                    EnableDelete="true"
                                    HideLabel="true"
                                    ContentType="4"
                                    LabelType="1"
                                    runat="server">
                            </SOE:FormIntervalEntry>
                        </table>
                    </fieldset>
                </div>
			</SOE:Tab>		
		</Tabs>
	</SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

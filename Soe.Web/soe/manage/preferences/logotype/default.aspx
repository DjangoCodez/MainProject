<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.preferences.logotype.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" TermID="30" DefaultTerm="Spara" EncType="multipart/form-data" runat="server">
		<Tabs>
			<SOE:Tab Type="Setting" TermID="5417" DefaultTerm="Logotypinställningar" runat="server">
				<div>
					<fieldset> 
						<legend><%=GetText(4092, "Logotyp")%></legend>
                        <SOE:Instruction 
                            ID="FormInstruction1"
                            TermID="1885" DefaultTerm="Format som stöds är: bmp, jpg, png, tiff och wmf"
                            runat="server">
                        </SOE:Instruction>
						<table>
						    <input type="hidden" name="action" value="upload" />
                            <SOE:FileEntry 
	                            ID="File" 
	                            TermID="4092" DefaultTerm="Logotyp" 
	                            Width="200"
	                            runat="server">
	                        </SOE:FileEntry>
						</table>
					</fieldset>
				</div>
				<div>
				    <fieldset> 
					    <legend><%=GetText(4096, "Befintliga logotyper")%></legend>
					    <table id="tableThumbnails" class="thumbnails" runat="server">
                        </table>
				    </fieldset>
				</div>
			</SOE:Tab>		
		</Tabs>
	</SOE:Form>		
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>


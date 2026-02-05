<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.excelimport._default" Title="" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server"> 
<%if (UseAngularSpa) {%>
    <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
<%} else {%>
<SOE:Form ID="Form1" TermID="1174" DefaultTerm="Importera" EncType="multipart/form-data" runat="server">
	    <Tabs>
		    <SOE:Tab Type="Setting" runat="server">
			    <div>
                    <fieldset>
                        <legend><%=GetText(5403, "Importera fil") %></legend>
                        <% if (loadFinnishFiles) 
                        {%>    
                            <a href="../../../common/excelimport/AsiakasTuonti.xlsx"><%=GetText(4263,"Filmall för kundimport") %></a><br />
			                <a href="../../../common/excelimport/ToimittajaTuonti.xlsx"><%=GetText(4264,"Filmall för leverantörimport") %></a><br />
			                <a href="../../../common/excelimport/TuoteTuonti.xlsx"><%=GetText(4265,"Filmall för artikelimport") %></a><br />
                            <a href="../../../common/excelimport/Työntekijätuonti.xlsx"><%=GetText(5575,"Filmall för anställdaimport") %></a><br />
			                <a href="../../../common/excelimport/YhteyshenkilöTuonti.xlsx"><%=GetText(4266,"Filmall för kontaktpersonsimport") %></a><br />
			                <a href="../../../common/excelimport/TuoteryhmäTuonti.xlsx"><%=GetText(4596,"Filmall för artikelkategori import") %></a><br />
			                <a href="../../../common/excelimport/AsiakasryhmäTuonti.xlsx"><%=GetText(4597,"Filmall för kundkategori import") %></a><br />
                            <a href="../../../common/excelimport/TiliTuonti.xlsx"><%=GetText(4693,"Filmall för kontoimport") %></a><br />
			                <a href="../../../common/excelimport/Hinnastotuonti.xlsx"><%=GetText(7731,"Filmall för import av prislistor") %></a><br /><br />
                        <%}
                        else
                        {%>
	                        <a href="../../../common/excelimport/KundImport.xlsx"><%=GetText(4263,"Filmall för kundimport") %></a><br />
			                <a href="../../../common/excelimport/LeverantörImport.xlsx"><%=GetText(4264,"Filmall för leverantörimport") %></a><br />
			                <a href="../../../common/excelimport/ArtikelImport.xlsx"><%=GetText(4265,"Filmall för artikelimport") %></a><br />
                            <a href="../../../common/excelimport/AnstalldImport_NY.xlsx"><%=GetText(5575,"Filmall för anställdaimport") %></a><br />
			                <a href="../../../common/excelimport/KontaktPersonsImport.xlsx"><%=GetText(4266,"Filmall för kontaktpersonsimport") %></a><br />
			                <a href="../../../common/excelimport/ArtikelKategoriImport.xlsx"><%=GetText(4596,"Filmall för artikelkategori import") %></a><br />
			                <a href="../../../common/excelimport/KundKategoriImport.xlsx"><%=GetText(4597,"Filmall för kundkategori import") %></a><br />
                            <a href="../../../common/excelimport/Kontoimport.xlsx"><%=GetText(4693,"Filmall för kontoimport") %></a><br />
                            <a href="../../../common/excelimport/SkattereduktionskontaktsImport.xlsx"><%=GetText(7614,"Filmall för import av skattereduktionskontakter") %></a><br />
                            <a href="../../../common/excelimport/Prislisteimport.xlsx"><%=GetText(7731,"Filmall för import av prislistor") %></a><br /><br />
                        <%}%>  
			            
			            <table>
                            <input type="hidden" name="action" value="upload" />
                            <SOE:FileEntry 
	                            ID="File" 
	                            TermID="4071" DefaultTerm="Fil"
	                            Width="500"
	                            runat="server">
	                        </SOE:FileEntry>
                        </table>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="DoNotModifyWithEmpty" 
                                TermID="5884"
                                DefaultTerm="Uppdatera ej befintliga poster med tomma värden från fil"
                                Value="True"
                                runat="server">
                            </SOE:CheckBoxEntry>
	                    </table>
                    </fieldset>
		        </div>
		    </SOE:Tab> 
	    </Tabs>
    </SOE:Form>
    <div>
        <SOE:Grid ID="SoeGrid1" runat="server" AutoGenerateColumns="false">
            <Columns>
                <SOE:BoundField 
                    DataField="RowNr" 
                    TermID="5617" DefaultTerm="RadNr" 
                    Filterable="Numeric" Sortable="Numeric">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="Field" 
                    TermID="5618" DefaultTerm="Kolumn" 
                    Filterable="Contains" Sortable="Text">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="Message" 
                    TermID="5619" DefaultTerm="Information" 
                    Filterable="Contains" Sortable="Text">
                </SOE:BoundField>
                <SOE:BoundField 
                    DataField="Identifier" 
                    TermID="5620" DefaultTerm="Id" 
                    Filterable="Contains" Sortable="Text">
                </SOE:BoundField>
             </Columns>
        </SOE:Grid>
    </div>
 <%}%>  
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>



    

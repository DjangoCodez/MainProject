<%@ Page EnableViewState="true" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.checklists._default" %>
<%@ Register Src="~/UserControls/SoeFormPrefix.ascx" TagPrefix="SOE" TagName="SoeFormPrefix" %>
<%@ Register Src="~/UserControls/SoeFormPostfix.ascx" TagPrefix="SOE" TagName="SoeFormPostfix" %>
<%@ Register Src="~/UserControls/SoeFormFooter.ascx" TagPrefix="SOE" TagName="SoeFormFooter" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <form id="form1" runat="server">
        <SOE:SoeFormPrefix ID="SoeFormPrefix" runat="server"></SOE:SoeFormPrefix>
        <div>

            <table>
                <tr>
                    <td style="width: 578px">
                     
                        <fieldset style="height: 153px; width: 473px;">
                            <legend>
                                   <asp:Label ID="LabelCopyFrom" runat="server" Text="Kopiera Från"></asp:Label>
                            </legend>
                            <table style="width: 440px">
                                <thead>
                                    <tr>
                                        <td style="width: 172px">
                                           <asp:Label id="LabelSourceLicenses" Text="Select License" CssClass="formText" runat="server" />
                                        </td>
                                        <td style="width: 155px">
                                             <asp:DropDownList 
                                                    ID="SourceLicenses" 
                                                    AutoPostBack="true"
                                                    Width="94%"
                                                    runat="server" OnSelectedIndexChanged="SourceLicenses_SelectedIndexChanged">
                                                </asp:DropDownList>
                                            </td>
                                
                                     
                                    </tr>
                                      <tr>
                          
                                        <td style="width: 172px">
                                            <asp:Label id="LabelSourceCompanies" Text="Select Company" CssClass="formText" runat="server" />
                                        </td>
                                        <td>
                                                  <asp:DropDownList 
                                                    ID="SourceCompanies" 
                                                    AutoPostBack="true"
                                                    Width="150"
                                                    Visible ="true"
                                                    runat="server" OnSelectedIndexChanged="SourceCompanies_SelectedIndexChanged">
                                                  </asp:DropDownList>
                                         </td>
                                 
                                      </tr>
                                      <tr>
                                         <td style="width: 172px">
                                             <asp:Label id="LabelSourceCheckList" Text="Select Checklist" CssClass="formText" runat="server" />
                                          </td>
                                          <td>
                                                  <asp:DropDownList 
                                                    ID="SourceCheckLists" 
                                                    AutoPostBack="true"
                                                    Width="150"
                                                    Visible ="true"
                                                    runat="server">
                                                  </asp:DropDownList>
                                         </td>
                                    
                                         
                                     </tr>
                                 
                                </thead>
                            </table>
                        </fieldset>
                    
                        </td>
                </tr>
                <tr>
                    <td style="width: 578px">
                        <br />
                        <fieldset style="height: 153px">
                            <legend>
                                   <asp:Label ID="LabelCopyTo" runat="server" Text="Kopiera Till"></asp:Label>
                            </legend>
                            <table style="width: 466px">
                                <thead>
                                    <tr>
                                        <td>
                                            <asp:Label id="LabelTargetLicense" Text="Select License" CssClass="formText" runat="server" />
                                        </td>
                                        <td style="width: 155px">
                                             <asp:DropDownList 
                                                    ID="TargetLicenses" 
                                                    AutoPostBack="true"
                                                    Width="150"
                                                    runat="server" OnSelectedIndexChanged="TargetLicenses_SelectedIndexChanged">
                                                </asp:DropDownList>
                                            </td>
                                 
                                    </tr>
                                      <tr>
                          
                                        <td>
                                            <asp:Label id="LabelTargetCompany" Text="Select Company" CssClass="formText" runat="server" />
                                        </td>
                                        <td>
                                                  <asp:DropDownList 
                                                    ID="TargetCompanies" 
                                                    AutoPostBack="true"
                                                    Width="150"
                                                    Visible ="true"
                                                    runat="server" style="height: 22px" >
                                                  </asp:DropDownList>
                                         </td>
                            
                                         
                                     </tr>
                                 
                                </thead>
                            </table>
                            <br />
                            <asp:Button ID="ButtonCopy" runat="server" OnClick="Button1_Click" Visible="false" Text="Kopiera Checklist" Width="305px" />
                            
                            &nbsp;&nbsp;&nbsp;
                            
                            <asp:Label ID="LabelResult" runat="server" Text=""></asp:Label>
                        </fieldset>
                    </td>
                </tr>
            </table>
        </div>
	    
    
        <SOE:SoeFormPostfix ID="SoeFormPostfix" runat="server"></SOE:SoeFormPostfix>
	

    </form> 

    </asp:Content>


<%@ Page Language="C#" MasterPageFile="ModalForm.Master" AutoEventWireup="true" CodeBehind="SelectCompany.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.SelectCompany" %>
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>

<asp:Content ID="Content2" ContentPlaceHolderID="scripts" runat="server">   
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <div class="divSelectCompany">
      <input class="form-control" id="filterCompanies" type="text" onkeyup="doFilterCompanies()" placeholder="Filtrera..">
      <br> 
      <table class="table table-bordered table-striped">
        <thead>
          <tr>
            <th id="company_CompanyNr"><%=GetText(1411, "Ftg nr") %></th>
            <th id="company_Name"><%=GetText(2028, "Namn") %></th>
            <th id="company_ShortName"><%=GetText(1412, "Kortnamn") %></th>
            <th id="company_OrgNr"><%=GetText(1159, "Organisationsnr") %></th>
            <th id="company_Select"></th>
          </tr>
        </thead>
        <tbody id="tblCompanies" class="tblCompaniesContent" runat="server"></tbody>
      </table>
    </div>     
</asp:Content>

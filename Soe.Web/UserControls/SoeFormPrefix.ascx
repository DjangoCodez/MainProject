<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SoeFormPrefix.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.SoeFormPrefix" %>
<div class="SoeForm">
    <div class="SoeTabView">
        <div class="tabList">
            <ul>
                <li>
                    <a href="#" class="active">  
                        <% if(!String.IsNullOrEmpty(TabImageSrc)) 
                        {%>                          
                            <img src="<%=TabImageSrc %>" width="16" height="16" />
                        <%} %>
                        <%=PageBase.Enc(Title)%>
                    </a>
                </li>
            </ul>
        </div>
        <div class="tabContent active">
            <div class="formHeader">
                <% if(!String.IsNullOrEmpty(StatusImageSrc)) 
                {%>                          
                    <li class="messageIcon">
                        <img src="<%=StatusImageSrc %>" width="16" height="16" />
                    </li>
                <%} %>
                <span class="toolBar"></span>
            </div>

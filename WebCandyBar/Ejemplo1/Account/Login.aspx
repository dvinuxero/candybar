<%@ Page Title="Entrar a Candy Bar" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.vb" Inherits="WebCandyBar.Login" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        If (Session("error") IsNot Nothing) Then
            Response.Write(Session("error"))
        End If
    %>
    <form id="form1" runat="server" method="post" action="Login.aspx">
    <div>
    
        Nickname:<br />
        <input id="nickname" name="nickname" type="text" /><br />
        Password:<br />
        <input id="password" name="password" type="password" /><br />
        <input id="login" name="login" type="submit" value="Entrar" /></div>
    </form>
</asp:Content>

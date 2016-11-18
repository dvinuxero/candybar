<%@ Page Title="Entrar" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.vb" Inherits="WebCandyBar.Login" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        If (Session("error") IsNot Nothing) Then
            Response.Write("<div class='error'>" + Session("error") + "</div>")
        End If
        
        If (Session("corrupcion") IsNot Nothing) Then
    %>
    <form id="form2" runat="server" method="post" action="Login.aspx">
        <input type="hidden" name="action" value="resolver" />
        <input type="hidden" name="error" value="corrupcion" />
        <div class="info">
            Al loguearse se resolvera automaticamente el problema de integridad en la base de datos
        </div>
        <input id="logAndResolve" name="logAndResolve" type="submit" value="Loguear y resolver" />
    </form>
    <%
    Else
    %>
    <form id="form1" runat="server">
        <div>
            Nickname:<br />
            <input id="nickname" name="nickname" type="text" /><br />
            Password:<br />
            <input id="password" name="password" type="password" /><br />
            <input id="login" name="login" type="submit" value="Entrar" />
        </div>
    </form>
    <%
    End If
    %>
</asp:Content>

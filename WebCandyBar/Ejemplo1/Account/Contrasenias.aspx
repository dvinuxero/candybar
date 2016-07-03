<%@ Page Title="Administracion de Contrasenias" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Contrasenias.aspx.vb" Inherits="WebCandyBar.Contrasenias" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        If ("GET".Equals(Request.HttpMethod)) Then
    %>
    <form action="Contrasenias.aspx" method="post">
        <table>
            <tr>
                <td>Contraseña Actual:</td>
                <td>
                    <input type="password" name="actual" /></td>
            </tr>
            <tr>
                <td>Contraseña nueva:</td>
                <td>
                    <input type="password" name="nueva" /></td>
            </tr>
            <tr>
                <td>Repetir contraseña nueva:</td>
                <td>
                    <input type="password" name="nuevaRepetida" /></td>
            </tr>
            <tr>
                <td>
                    <input type="submit" name="guardarContrasena" value="Guardar" />
                    <a href='/Default.aspx'>Volver</a></td>
            </tr>
        </table>
    </form>
    <br />
    <div class="info">
        Si no recuerda la contraseña actual solicite ayuda al administrador via <a href="mailto:deliciasnil@gmail.com">Email</a>
    </div>
    <%
    Else
        Try
            Dim actual As String = Request.Form("actual")
            Dim nueva As String = Request.Form("nueva")
            Dim nuevaRepetida As String = Request.Form("nuevaRepetida")
            
            NegocioYSeguridad.UsuarioBO.getInstance().cambiarContrasenia(actual, nueva, nuevaRepetida)
            Response.Write("<div class='exito'>Exito! <a href='/Default.aspx'>Volver</a></div>")
        Catch ex As Exceptions.CandyException
            Response.Write("<div class='error'>Error! " + ex.Message + " <a href='/Default.aspx'>Volver</a></div>")
        End Try
    End If
    %>
</asp:Content>

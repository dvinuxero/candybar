<%@ Page Title="Administracion de usuarios" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Usuarios.aspx.vb" Inherits="WebCandyBar.Usuarios" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        
        If (("post".Equals(Request.Form("action")) Or "put".Equals(Request.Form("action"))) Or ("post".Equals(Request.QueryString("action")) Or "put".Equals(Request.QueryString("action")))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim usuario As New EntidadesDTO.UsuarioDTO()
                If ("put".Equals(Request.QueryString("action"))) Then
                    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
                    usuario = NegocioYSeguridad.UsuarioBO.getInstance().obtenerUsuarioPorId(id)
                End If
    %>
    <form action="Usuarios.aspx" method="post">
        <input type="hidden" name="action" value='<%= IIf("put".Equals(Request.Form("action")) or "put".Equals(Request.QueryString("action")), "put", "post") %>' />
        <%
            If (usuario IsNot Nothing And usuario.id <> 0) Then
                Response.Write("<input type='hidden' name='id' value='" + usuario.id.ToString() + "' />")
            End If
        %>
        <table>
            <tr>
                <td>Nickname:</td>
                <td>
                    <input type="text" name="nickname" value='<%= IIf(usuario IsNot Nothing, usuario.nickname, "")%>' /></td>
            </tr>
            <tr>
                <td>Nombre:</td>
                <td>
                    <input type="text" name="nombre" value='<%= IIf(usuario IsNot Nothing, usuario.nombre, "")%>' /></td>
            </tr>
            <tr>
                <td>Apellido:</td>
                <td>
                    <input type="text" name="apellido" value='<%= IIf(usuario IsNot Nothing, usuario.apellido, "")%>' /></td>
            </tr>
            <tr>
                <td>Idioma:</td>
                <td>
                    <select name="lang">
                        <option value="es">es</option>
                        <option value="es">pt</option>
                        <option value="es">en</option>
                        <option value="es">fr</option>
                    </select>
                </td>
            </tr>
            <tr>
                <td>
                    <input type="submit" name="guardarUsuario" value="Guardar" />
                    <a href='/Account/Usuarios.aspx'>Volver</a></td>
            </tr>
        </table>
    </form>
    <%
    Else
        Dim usuario As New EntidadesDTO.UsuarioDTO()
                
        usuario.nickname = Request.Form("nickname")
        usuario.nombre = Request.Form("nombre")
        usuario.apellido = Request.Form("apellido")
        usuario.lang = Request.Form("lang")
                
        Try
            If (Request.Form("id") IsNot Nothing) Then
                usuario.id = Integer.Parse(Request.Form("id"))
                NegocioYSeguridad.UsuarioBO.getInstance().modificarUsuario(usuario)
            Else
                NegocioYSeguridad.UsuarioBO.getInstance().agregarUsuario(usuario)
            End If
            Response.Write("<div class='exito'>Exito! <a href='/Account/Usuarios.aspx'>Volver</a></div>")
        Catch ex As Exceptions.CandyException
            Response.Write("<div class='error'>Error! " + ex.Message + " <a href='/Account/Usuarios.aspx'>Volver</a></div>")
        End Try
    End If
ElseIf ("delete".Equals(Request.QueryString("action"))) Then
    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
    Dim usuarioABorrar As EntidadesDTO.UsuarioDTO = NegocioYSeguridad.UsuarioBO.getInstance().obtenerUsuarioPorId(id)
    Try
        NegocioYSeguridad.UsuarioBO.getInstance().eliminarUsuario(usuarioABorrar)
        Response.Write("<div class='exito'>Exito! <a href='/Account/Usuarios.aspx'>Volver</a></div>")
    Catch ex As Exceptions.CandyException
        Response.Write("<div class='error'>Error! " + ex.Message + " <a href='/Account/Usuarios.aspx'>Volver</a></div>")
    End Try
ElseIf ("contrasena".Equals(Request.QueryString("action"))) Then
    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
    Try
        NegocioYSeguridad.UsuarioBO.getInstance().reestablecerContraseña(id)
        Response.Write("<div class='exito'>Exito! <a href='/Account/Usuarios.aspx'>Volver</a></div>")
    Catch ex As Exceptions.CandyException
        Response.Write("<div class='error'>Error! " + ex.Message + " <a href='/Account/Usuarios.aspx'>Volver</a></div>")
    End Try
Else
    Dim usuarios As Dictionary(Of String, EntidadesDTO.UsuarioDTO) = NegocioYSeguridad.UsuarioBO.getInstance().obtenerUsuarios()
    Response.Write("<table>")
    Response.Write("<tr><td><b>NICKNAME</b></td><td><b>NOMBRE</b></td><td><b>APELLIDO</b></td><td><b>IDIOMA</b></td><td><b>BAJA</b></td><td><b>ACCIONES(<a href='Usuarios.aspx?action=post'>Nuevo</a>)</b></td></tr>")
    For Each usuario As EntidadesDTO.UsuarioDTO In usuarios.Values
        Response.Write("<tr>")
        Response.Write("<td>" + usuario.nickname + "</td>")
        Response.Write("<td>" + usuario.nombre + "</td>")
        Response.Write("<td>" + usuario.apellido + "</td>")
        Response.Write("<td>" + usuario.lang + "</td>")
        Response.Write("<td>" + usuario.baja + "</td>")
        Response.Write("<td>" + "<a href='/Account/Usuarios.aspx?action=delete&id=" + usuario.id.ToString() + "'>Borrar</a> " + "<a href='/Account/Usuarios.aspx?action=put&id=" + usuario.id.ToString() + "'>Modificar</a> " + "<a href='/Account/Usuarios.aspx?action=lock&id=" + usuario.id.ToString() + "'>Bloquear</a> " + "<a href='/Account/Patentes.aspx?action=user&id=" + usuario.id.ToString() + "'>Patentes</a> " + "<a href='/Account/Familias.aspx?action=user&id=" + usuario.id.ToString() + "'>Familias</a> " + "<a href='/Account/Usuarios.aspx?action=contrasena&id=" + usuario.id.ToString() + "'>Reestablecer contraseña</a> " + "</td>")
        Response.Write("</tr>")
    Next
    Response.Write("</table>")
End If
    %>
</asp:Content>

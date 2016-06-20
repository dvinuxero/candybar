<%@ Page Title="Administracion de familias" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Familias.aspx.vb" Inherits="WebCandyBar.Familias" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">

    <%
        If ("user".Equals(Request.QueryString("action")) Or "user".Equals(Request.Form("action"))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim usuarioId As Integer = Integer.Parse(Request.QueryString("id"))
                Dim familiasDelUsuario = NegocioYSeguridad.PermisoBO.getInstance().obtenerFamiliasPorUsuario(usuarioId)
                Dim familias = NegocioYSeguridad.PermisoBO.getInstance().obtenerFamilias()
            
                Response.Write("<form action='Familias.aspx' method='post'>")
                Response.Write("<input type='hidden' name='action' value='user'>")
                Response.Write("<input type='hidden' name='id' value='" + usuarioId.ToString() + "'>")
                Response.Write("<table>")
                Response.Write("<tr><td><b>ASIGNAR</b></td><td><b>NOMBRE</b></td><td><b>DESCRIPCION</b></td></tr>")
                For Each familia As String In familias.Keys
                    Dim checkedAsignado As String = ""
                    If (familiasDelUsuario.Contains(familia)) Then
                        checkedAsignado = "checked"
                    Else
                        checkedAsignado = ""
                    End If
                
                    Response.Write("<tr>")
                    Response.Write("<td><input type='checkbox' name='asignar' value='" + familia + "' " + checkedAsignado + "/></td>")
                    Response.Write("<td>" + familia + "</td>")
                    Response.Write("<td>" + familias(familia) + "</td>")
                    Response.Write("</tr>")
                Next
                Response.Write("<tr><td><input type='submit' name='guardarFamiliasDelUsuario' value='Guardar' /> <a href='/Account/Usuarios.aspx'>Volver</a></td></tr>")
                Response.Write("</table>")
                Response.Write("</form>")
            Else
                Dim usuarioId As Integer = Integer.Parse(Request.Form("id"))
                Dim familiasAsignadasAlUsuario = Request.Form("asignar")
                Dim familiasAsignadas As New List(Of String)
            
                If (familiasAsignadasAlUsuario IsNot Nothing) Then
                    If (familiasAsignadasAlUsuario.Contains(",")) Then
                        For Each faU As String In familiasAsignadasAlUsuario.Split(",")
                            familiasAsignadas.Add(faU)
                        Next
                    Else
                        familiasAsignadas.Add(familiasAsignadasAlUsuario)
                    End If
                End If
            
                Try
                    NegocioYSeguridad.PermisoBO.getInstance().asociarFamiliasAlUsuario(usuarioId, familiasAsignadas)
                    Response.Write("Exito! <a href='/Account/Usuarios.aspx'>Volver</a>")
                Catch ex As Exceptions.CandyException
                    Response.Write("Error! " + ex.Message + " <a href='/Account/Usuarios.aspx'>Volver</a>")
                End Try
            End If
        
        ElseIf (("post".Equals(Request.Form("action")) Or "put".Equals(Request.Form("action"))) Or ("post".Equals(Request.QueryString("action")) Or "put".Equals(Request.QueryString("action")))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim familiaDesc As String = ""
                Dim familiaId As String = ""
                Dim id As String = ""
                If ("put".Equals(Request.QueryString("action"))) Then
                    id = Request.QueryString("id")
                    familiaDesc = NegocioYSeguridad.PermisoBO.getInstance().obtenerFamilias(id)
                End If
    %>
    <form action="Familias.aspx" method="post">
        <input type="hidden" name="action" value='<%= IIf("put".Equals(Request.Form("action")) or "put".Equals(Request.QueryString("action")), "put", "post") %>' />
        <%
            If (Not "".Equals(id)) Then
                Response.Write("<input type='hidden' name='id' value='" + id + "' />")
            End If
        %>
        <table>
            <tr>
                <td>Nombre:</td>
                <td>
                    <input type="text" name="nombre" value='<%= IIf(Not "".Equals(id), id, "")%>' /></td>
            </tr>
            <tr>
                <td>Descripcion:</td>
                <td>
                    <input type="text" name="descripcion" value='<%= IIf(Not "".Equals(familiaDesc), familiaDesc, "")%>' /></td>
            </tr>
            <tr>
                <td>
                    <input type="submit" name="guardarFamilia" value="Guardar" />
                    <a href='/Account/Familias.aspx'>Volver</a></td>
            </tr>
        </table>
    </form>
    <%
    Else
        Dim familiaId As String = Request.Form("nombre")
        Dim familiaDesc As String = Request.Form("descripcion")
                
        Try
            If (Request.Form("id") IsNot Nothing) Then
                Dim id As String = Request.Form("id")
                NegocioYSeguridad.PermisoBO.getInstance().modificarFamilia(id, familiaId, familiaDesc)
            Else
                NegocioYSeguridad.PermisoBO.getInstance().agregarFamilia(familiaId, familiaDesc)
            End If
            Response.Write("Exito! <a href='/Account/Familias.aspx'>Volver</a>")
        Catch ex As Exceptions.CandyException
            Response.Write("Error! " + ex.Message + " <a href='/Account/Familias.aspx'>Volver</a>")
        End Try
    End If
ElseIf ("delete".Equals(Request.QueryString("action"))) Then
    Dim id As String = Request.QueryString("id")
    Try
        NegocioYSeguridad.PermisoBO.getInstance().eliminarFamilia(id)
        Response.Write("Exito! <a href='/Account/Familias.aspx'>Volver</a>")
    Catch ex As Exceptions.CandyException
        Response.Write("Error! " + ex.Message + " <a href='/Account/Familias.aspx'>Volver</a>")
    End Try
Else
    Dim familias As Dictionary(Of String, String) = NegocioYSeguridad.PermisoBO.getInstance().obtenerFamilias()
    Response.Write("<table>")
    Response.Write("<tr><td><b>NOMBRE</b></td><td><b>DESCRIPCION</b></td><td><b>ACCIONES(<a href='Familias.aspx?action=post'>Nueva</a>)</b></td></tr>")
    For Each familiaId As String In familias.Keys
        Response.Write("<tr>")
        Response.Write("<td>" + familiaId + "</td>")
        Response.Write("<td>" + familias(familiaId) + "</td>")
        Response.Write("<td>" + "<a href='/Account/Familias.aspx?action=delete&id=" + familiaId.ToString() + "'>Borrar</a> " + "<a href='/Account/Familias.aspx?action=put&id=" + familiaId.ToString() + "'>Modificar</a> " + "<a href='/Account/Patentes.aspx?action=familia&id=" + familiaId.ToString() + "'>Patentes</a> " + "</td>")
        Response.Write("</tr>")
    Next
    Response.Write("</table>")
End If
    %>
</asp:Content>

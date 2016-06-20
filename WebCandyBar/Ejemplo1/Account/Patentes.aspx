<%@ Page Title="Administracion de patentes" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Patentes.aspx.vb" Inherits="WebCandyBar.Patentes" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">

    <%
        If ("user".Equals(Request.QueryString("action")) Or "user".Equals(Request.Form("action"))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim usuarioId As Integer = Integer.Parse(Request.QueryString("id"))
                Dim patentesDelUsuario = NegocioYSeguridad.PermisoBO.getInstance().obtenerPatentesPorUsuario(usuarioId)
                Dim patentes = NegocioYSeguridad.PermisoBO.getInstance().obtenerPatentes()
            
                Response.Write("<form action='Patentes.aspx' method='post'>")
                Response.Write("<input type='hidden' name='action' value='user'>")
                Response.Write("<input type='hidden' name='id' value='" + usuarioId.ToString() + "'>")
                Response.Write("<table>")
                Response.Write("<tr><td><b>ASIGNAR</b></td><td><b>NEGAR</b></td><td><b>PATENTE</b></td></tr>")
                For Each patente As String In patentes.Keys
                    Dim checkedAsignado As String = ""
                    Dim checkedNegado As String = ""
                    If (patentesDelUsuario.Contains(patente)) Then
                        checkedNegado = ""
                        checkedAsignado = "checked"
                    ElseIf (patentesDelUsuario.Contains(patente + NegocioYSeguridad.PermisoBO.PATENTE_NEGADA_FLAG)) Then
                        checkedNegado = "checked"
                        checkedAsignado = ""
                    End If
                
                    Response.Write("<tr>")
                    Response.Write("<td><input type='checkbox' name='asignar' value='" + patente + "' " + checkedAsignado + "/></td>")
                    Response.Write("<td><input type='checkbox' name='negar' value='" + patente + "' " + checkedNegado + "/></td>")
                    Response.Write("<td>" + patente + "</td>")
                    Response.Write("</tr>")
                Next
                Response.Write("<tr><td><input type='submit' name='guardarPatentesDelUsuario' value='Guardar' /> <a href='/Account/Usuarios.aspx'>Volver</a></td></tr>")
                Response.Write("</table>")
                Response.Write("</form>")
            Else
                Dim usuarioId As Integer = Integer.Parse(Request.Form("id"))
                Dim patentesAsignadasAlUsuario = Request.Form("asignar")
                Dim patentesNegadasAlUsuario = Request.Form("negar")
                Dim patentesAsignadas As New List(Of String)
            
                If (patentesAsignadasAlUsuario IsNot Nothing) Then
                    If (patentesAsignadasAlUsuario.Contains(",")) Then
                        For Each paU As String In patentesAsignadasAlUsuario.Split(",")
                            patentesAsignadas.Add(paU)
                        Next
                    Else
                        patentesAsignadas.Add(patentesAsignadasAlUsuario)
                    End If
                End If
                If (patentesNegadasAlUsuario IsNot Nothing) Then
                    If (patentesNegadasAlUsuario.Contains(",")) Then
                        For Each pnU As String In patentesNegadasAlUsuario.Split(",")
                            If (patentesAsignadas.Contains(pnU)) Then
                                patentesAsignadas.Remove(pnU)
                            End If
                            patentesAsignadas.Add(pnU + NegocioYSeguridad.PermisoBO.PATENTE_NEGADA_FLAG)
                        Next
                    Else
                        patentesAsignadas.Add(patentesNegadasAlUsuario + NegocioYSeguridad.PermisoBO.PATENTE_NEGADA_FLAG)
                    End If
                End If
            
                NegocioYSeguridad.PermisoBO.getInstance().asociarPatentesAlUsuario(usuarioId, patentesAsignadas)
                Response.Write("Exito! <a href='/Account/Usuarios.aspx'>Volver</a>")
            End If
        
        ElseIf ("familia".Equals(Request.QueryString("action")) Or "familia".Equals(Request.Form("action"))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim familiaId As String = Request.QueryString("id")
                Dim patentesDeLaFamilia = NegocioYSeguridad.PermisoBO.getInstance().obtenerPatentesPorFamilia(familiaId)
                Dim patentes = NegocioYSeguridad.PermisoBO.getInstance().obtenerPatentes()
            
                Response.Write("<form action='Patentes.aspx' method='post'>")
                Response.Write("<input type='hidden' name='action' value='familia'>")
                Response.Write("<input type='hidden' name='id' value='" + familiaId.ToString() + "'>")
                Response.Write("<table>")
                Response.Write("<tr><td><b>ASIGNAR</b></td><td><b>NEGAR</b></td><td><b>PATENTE</b></td></tr>")
                For Each patente As String In patentes.Keys
                    Dim checkedAsignado As String = ""
                    Dim checkedNegado As String = ""
                    If (patentesDeLaFamilia.Contains(patente)) Then
                        checkedNegado = ""
                        checkedAsignado = "checked"
                    ElseIf (patentesDeLaFamilia.Contains(patente + NegocioYSeguridad.PermisoBO.PATENTE_NEGADA_FLAG)) Then
                        checkedNegado = "checked"
                        checkedAsignado = ""
                    End If
                
                    Response.Write("<tr>")
                    Response.Write("<td><input type='checkbox' name='asignar' value='" + patente + "' " + checkedAsignado + "/></td>")
                    Response.Write("<td><input type='checkbox' name='negar' value='" + patente + "' " + checkedNegado + "/></td>")
                    Response.Write("<td>" + patente + "</td>")
                    Response.Write("</tr>")
                Next
                Response.Write("<tr><td><input type='submit' name='guardarPatentesDeLaFamilia' value='Guardar' /> <a href='/Account/Familias.aspx'>Volver</a></td></tr>")
                Response.Write("</table>")
                Response.Write("</form>")
            Else
                Dim familiaId As String = Request.Form("id")
                Dim patentesAsignadasALaFamilia = Request.Form("asignar")
                Dim patentesNegadasALaFamilia = Request.Form("negar")
                Dim patentesAsignadas As New List(Of String)
            
                If (patentesAsignadasALaFamilia IsNot Nothing) Then
                    If (patentesAsignadasALaFamilia.Contains(",")) Then
                        For Each paU As String In patentesAsignadasALaFamilia.Split(",")
                            patentesAsignadas.Add(paU)
                        Next
                    Else
                        patentesAsignadas.Add(patentesAsignadasALaFamilia)
                    End If
                End If
                If (patentesNegadasALaFamilia IsNot Nothing) Then
                    If (patentesNegadasALaFamilia.Contains(",")) Then
                        For Each pnU As String In patentesNegadasALaFamilia.Split(",")
                            If (patentesAsignadas.Contains(pnU)) Then
                                patentesAsignadas.Remove(pnU)
                            End If
                            patentesAsignadas.Add(pnU + NegocioYSeguridad.PermisoBO.PATENTE_NEGADA_FLAG)
                        Next
                    Else
                        patentesAsignadas.Add(patentesNegadasALaFamilia + NegocioYSeguridad.PermisoBO.PATENTE_NEGADA_FLAG)
                    End If
                End If
            
                NegocioYSeguridad.PermisoBO.getInstance().asociarPatentesDeLaFamilia(familiaId, patentesAsignadas)
                Response.Write("Exito! <a href='/Account/Familias.aspx'>Volver</a>")
            End If
        Else
            Dim patentes = NegocioYSeguridad.PermisoBO.getInstance().obtenerPatentes()
        
            Response.Write("<table>")
            Response.Write("<tr><td><b>ID</b></td><td><b>DESCRIPCION</b></td></tr>")
            For Each patenteKey As String In patentes.Keys
                Response.Write("<tr>")
                Response.Write("<td>" + patenteKey + "</td>")
                Response.Write("<td>" + patentes(patenteKey) + "</td>")
                Response.Write("</tr>")
            Next
            Response.Write("</table>")
        End If
    %>
</asp:Content>

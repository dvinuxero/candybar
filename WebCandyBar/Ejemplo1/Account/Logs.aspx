<%@ Page Title="Administracion de usuarios" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Logs.aspx.vb" Inherits="WebCandyBar.Logs" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        Dim fromI As Integer = 0
        Dim toI As Integer = 0
        
        Try
            fromI = Integer.Parse(Request.QueryString("from"))
        Catch ex As Exception
            fromI = NegocioYSeguridad.BitacoraBO.getInstance().obtenerMinId()
        End Try
        
        toI = fromI + 50
        
        Dim logs As Dictionary(Of String, EntidadesDTO.BitacoraDTO) = NegocioYSeguridad.BitacoraBO.getInstance().obtenerLogs(fromI, toI)
        Response.Write("<< <a href='/Account/Logs.aspx?from=" + (IIf((fromI - 50) > 0, fromI - 50, 0)).ToString() + "'>Anterior</a> <a href='/Account/Logs.aspx?from=" + toI.ToString() + "'>Siguiente</a> >><br>")
        Response.Write("<table>")
        Response.Write("<tr><td><b>ID</b></td><td><b>USUARIO</b></td><td><b>FECHA</b></td><td><b>DESCRIPCION</b></td><td><b>CRITICIDAD</b></td></tr>")
        
        For Each log As EntidadesDTO.BitacoraDTO In logs.Values
            Dim identificacionUsuario As String = log.usuarioId.ToString() + " (INDEFINIDO)"
            If (NegocioYSeguridad.UsuarioBO.getInstance().obtenerUsuarioPorId(Integer.Parse(log.usuarioId)) IsNot Nothing) Then
                identificacionUsuario = NegocioYSeguridad.UsuarioBO.getInstance().obtenerUsuarioPorId(Integer.Parse(log.usuarioId)).nickname
            End If
            
            Response.Write("<tr>")
            Response.Write("<td>" + log.id.ToString() + "</td>")
            Response.Write("<td>" + identificacionUsuario + "</td>")
            Response.Write("<td>" + log.fecha + "</td>")
            Response.Write("<td>" + log.descripcion + "</td>")
            Response.Write("<td>" + log.criticidad + "</td>")
            Response.Write("</tr>")
        Next
        Response.Write("</table>")
        Response.Write("<br><< <a href='/Account/Logs.aspx?from=" + (IIf((fromI - 50) > 0, fromI - 50, 0)).ToString() + "'>Anterior</a> <a href='/Account/Logs.aspx?from=" + toI.ToString() + "'>Siguiente</a> >>")
    %>
</asp:Content>

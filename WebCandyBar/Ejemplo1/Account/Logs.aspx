<%@ Page Title="Administracion de usuarios" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Logs.aspx.vb" Inherits="WebCandyBar.Logs" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        Dim logs As Dictionary(Of String, EntidadesDTO.BitacoraDTO) = NegocioYSeguridad.BitacoraBO.getInstance().obtenerLogs(1550, 1600)
        Response.Write("<table>")
        Response.Write("<tr><td><b>ID</b></td><td><b>USUARIO</b></td><td><b>FECHA</b></td><td><b>DESCRIPCION</b></td><td><b>CRITICIDAD</b></td></tr>")
        
        For Each log As EntidadesDTO.BitacoraDTO In logs.Values
            Response.Write("<tr>")
            Response.Write("<td>" + log.id + "</td>")
            Response.Write("<td>" + log.usuarioId + "</td>")
            Response.Write("<td>" + log.fecha + "</td>")
            Response.Write("<td>" + log.descripcion + "</td>")
            Response.Write("<td>" + log.criticidad + "</td>")
            Response.Write("</tr>")
        Next
        Response.Write("</table>")
    %>
</asp:Content>

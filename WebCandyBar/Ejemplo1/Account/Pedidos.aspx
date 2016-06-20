<%@ Page Title="Administracion de pedidos" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Pedidos.aspx.vb" Inherits="WebCandyBar.Pedidos" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        If (("post".Equals(Request.Form("action")) Or "put".Equals(Request.Form("action"))) Or ("post".Equals(Request.QueryString("action")) Or "put".Equals(Request.QueryString("action")))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim pedido As New EntidadesDTO.PedidoDTO()
                If ("put".Equals(Request.QueryString("action"))) Then
                    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
                    pedido = NegocioYSeguridad.PedidoBO.getInstance().obtenerPedidoPorId(id)
                End If
    %>
    <form action="Pedidos.aspx" method="post">
        <input type="hidden" name="action" value='<%= IIf("put".Equals(Request.Form("action")) Or "put".Equals(Request.QueryString("action")), "put", "post")%>' />
        <%
            If (pedido IsNot Nothing) Then
                If (pedido.id <> 0) Then
                    Response.Write("<input type='hidden' name='id' value='" + pedido.id.ToString() + "' />")
                End If
            End If
        %>
        <table>
            <tr>
                <td>Agasajado:</td>
                <td>
                    <input type="text" name="agasajado" value='<%= IIf(pedido IsNot Nothing, pedido.agasajado, "")%>' /></td>
                <tr>
                    <td>Combo:</td>
                    <td>
                        <%
                            Dim combos = NegocioYSeguridad.ComboBO.getInstance().obtenerCombos()
                            Response.Write("<select name='comboId'>")
                            For Each combo As EntidadesDTO.ComboDTO In combos.Values
                                Response.Write("<option value='" + combo.id.ToString() + "'>" + combo.nombre + "</option>")
                            Next
                            Response.Write("</select>")
                        %>
                    </td>
                </tr>
            <tr>
                <td>Fecha de Inicio:</td>
                <td>
                    <input type="date" name="fechaInicio" value='<%= IIf(pedido IsNot Nothing, pedido.fechaInicio, "")%>'></td>
            </tr>
            <tr>
                <td>Fecha de Entrega:</td>
                <td>
                    <input type="date" name="fechaEntrega" value='<%= IIf(pedido IsNot Nothing, pedido.fechaEntrega, "")%>'></td>
            </tr>
            <tr>
                <td>Comentario:</td>
                <td>
                    <textarea name="comentario" rows="5"><%= IIf(pedido IsNot Nothing, pedido.comentario, "")%></textarea></td>
            </tr>
            <tr>
                <td>
                    <input type="submit" name="guardarPedido" value="Guardar" />
                    <a href='/Account/Pedidos.aspx'>Volver</a></td>
            </tr>
        </table>
    </form>
    <%
    Else
        Dim pedido As New EntidadesDTO.PedidoDTO()
                
        pedido.agasajado = Request.Form("agasajado")
        pedido.comboId = Request.Form("comboId")
        pedido.comentario = Request.Form("comentario")
        pedido.fechaInicio = Request.Form("fechaInicio")
        pedido.fechaEntrega = Request.Form("fechaEntrega")
                
        If (Request.Form("id") IsNot Nothing) Then
            pedido.id = Integer.Parse(Request.Form("id"))
            NegocioYSeguridad.PedidoBO.getInstance().actualizarPedido(pedido)
        Else
            NegocioYSeguridad.PedidoBO.getInstance().agregarPedido(pedido)
        End If
        Response.Write("Exito! <a href='/Account/Pedidos.aspx'>Volver</a>")
    End If
ElseIf ("delete".Equals(Request.QueryString("action"))) Then
    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
    NegocioYSeguridad.PedidoBO.getInstance().cancelarPedido(id)
    Response.Write("Exito! <a href='/Account/Pedidos.aspx'>Volver</a>")
Else
    Dim pedidos As Dictionary(Of String, EntidadesDTO.PedidoDTO) = NegocioYSeguridad.PedidoBO.getInstance().obtenerPedidos()
    Response.Write("<table>")
    Response.Write("<tr><td><b>ID</b></td><td><b>COMBO</b></td><td><b>ESTADO</b></td><td><b>FECHA DE ENTREGA</b></td><td><b>AGASAJADO</b></td><td><b>COMENTARIO</b></td><td><b>ACCIONES(<a href='Pedidos.aspx?action=post'>Nuevo</a>)</b></td></tr>")
    For Each pedido As EntidadesDTO.PedidoDTO In pedidos.Values
        Dim comboAsignado As String = NegocioYSeguridad.ComboBO.getInstance().obtenerComboPorId(pedido.comboId).nombre
        Dim comentarioStr As String = ""
        If (pedido.comentario.Length > 0) Then
            If (pedido.comentario.Length > 40) Then
                comentarioStr = pedido.comentario.Substring(0, 40) & "..."
            Else
                comentarioStr = pedido.comentario
            End If
        End If
        Response.Write("<tr>")
        Response.Write("<td>" + pedido.id.ToString() + "</td>")
        Response.Write("<td>" + comboAsignado + "</td>")
        Response.Write("<td>" + EntidadesDTO.PedidoDTO.getPedidoEstado(pedido.estado) + "</td>")
        Response.Write("<td>" + pedido.fechaEntrega + "</td>")
        Response.Write("<td>" + pedido.agasajado + "</td>")
        Response.Write("<td>" + comentarioStr + "</td>")
        Response.Write("<td>" + "<a href='/Account/Pedidos.aspx?action=delete&id=" + pedido.id.ToString() + "'>Borrar</a> " + "<a href='/Account/Pedidos.aspx?action=put&id=" + pedido.id.ToString() + "'>Modificar</a> " + "</td>")
        Response.Write("</tr>")
    Next
    Response.Write("</table>")
End If
    %>
</asp:Content>

<%@ Page Title="Administracion de pedidos" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Pedidos.aspx.vb" Inherits="WebCandyBar.Pedidos" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        If (("post".Equals(Request.Form("action")) Or "put".Equals(Request.Form("action"))) Or ("post".Equals(Request.QueryString("action")) Or "put".Equals(Request.QueryString("action")))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim pedido As New EntidadesDTO.PedidoDTO()
                Dim fechaInicio As String = ""
                Dim fechaEntrega As String = ""
                If ("put".Equals(Request.QueryString("action"))) Then
                    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
                    pedido = NegocioYSeguridad.PedidoBO.getInstance().obtenerPedidoPorId(id)
                    
                    If (pedido.fechaInicio IsNot Nothing) Then
                        If (Not "".Equals(pedido.fechaInicio)) Then
                            fechaInicio = pedido.fechaInicio.Split("/")(2) + "-" + pedido.fechaInicio.Split("/")(1) + "-" + pedido.fechaInicio.Split("/")(0)
                        End If
                    End If
                    
                    If (pedido.fechaEntrega IsNot Nothing) Then
                        If (Not "".Equals(pedido.fechaEntrega)) Then
                            fechaEntrega = pedido.fechaEntrega.Split("/")(2) + "-" + pedido.fechaEntrega.Split("/")(1) + "-" + pedido.fechaEntrega.Split("/")(0)
                        End If
                    End If
                    
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
                <td>Estado:</td>
                <td>
                    <table>
                        <%If ("put".Equals(Request.Form("action")) or "put".Equals(Request.QueryString("action"))) Then%>
                            <tr><td><input type="radio" name="estado" value="PENDIENTE" id="radioPendiente" <%=IIf(pedido.estado = EntidadesDTO.PedidoDTO.PedidoEstado.PENDIENTE, "checked", "")%> /></td><td class="estado-pendiente"><label for="radioPendiente">PENDIENTE</label></td></tr>
                            <tr><td><input type="radio" name="estado" value="PRODUCIR" id="radioProducir" <%=IIf(pedido.estado = EntidadesDTO.PedidoDTO.PedidoEstado.PRODUCIR, "checked", "")%> /></td><td class="estado-producir"><label for="radioProducir">PRODUCIR</label></td></tr>
                            <tr><td><input type="radio" name="estado" value="FINALIZADO" id="radioFinalizado" <%=IIf(pedido.estado = EntidadesDTO.PedidoDTO.PedidoEstado.FINALIZADO, "checked", "")%> /></td><td class="estado-finalizado"><label for="radioFinalizado">FINALIZADO</label></td></tr>
                            <tr><td><input type="radio" name="estado" value="ENTREGADO" id="radioEntregado" <%=IIf(pedido.estado = EntidadesDTO.PedidoDTO.PedidoEstado.ENTREGADO, "checked", "")%> /></td><td class="estado-entregado"><label for="radioEntregado">ENTREGADO</label></td></tr>
                        <%Else%>
                            <tr><td><input type="radio" name="estado" value="PENDIENTE" id="radio1" checked/></td><td class="estado-pendiente"><label for="radioPendiente">PENDIENTE</label></td></tr>
                        <%End If%>
                    </table>
                </td>
            </tr>
            <tr>
                <td>Fecha de Inicio:</td>
                <td>
                    <input type="date" name="fechaInicio" value='<%= IIf(pedido IsNot Nothing, fechaInicio, "")%>'></td>
            </tr>
            <tr>
                <td>Fecha de Entrega:</td>
                <td>
                    <input type="date" name="fechaEntrega" value='<%= IIf(pedido IsNot Nothing, fechaEntrega, "")%>'></td>
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
        pedido.estado = EntidadesDTO.PedidoDTO.setPedidoEstado(Request.Form("estado"))
        
        Dim f1 As String = Request.Form("fechaInicio")
        If (f1 IsNot Nothing) Then
            If (Not "".Equals(f1)) Then
                Dim f1A As String() = f1.Split("-")
                pedido.fechaInicio = f1A(2) + "/" + f1A(1) + "/" + f1A(0)
            End If
        End If
        
        Dim f2 As String = Request.Form("fechaEntrega")
        If (f2 IsNot Nothing) Then
            If (Not "".Equals(f2)) Then
                Dim f2A As String() = f2.Split("-")
                pedido.fechaEntrega = f2A(2) + "/" + f2A(1) + "/" + f2A(0)
            End If
        End If
                
        Try
            If (Request.Form("id") IsNot Nothing) Then
                pedido.id = Integer.Parse(Request.Form("id"))
                
                Dim estadoAnterior As EntidadesDTO.PedidoDTO.PedidoEstado = NegocioYSeguridad.PedidoBO.getInstance().obtenerPedidoPorId(pedido.id).estado
                
                If (Not EntidadesDTO.PedidoDTO.getPedidoEstado(estadoAnterior).Equals(EntidadesDTO.PedidoDTO.getPedidoEstado(pedido.estado))) Then
                    Select Case pedido.estado
                        Case EntidadesDTO.PedidoDTO.PedidoEstado.PENDIENTE
                            If (estadoAnterior = EntidadesDTO.PedidoDTO.PedidoEstado.PRODUCIR) Then
                                NegocioYSeguridad.PedidoBO.getInstance().pasarPedidoAPendiente(pedido.id)
                            Else
                                Throw New Exceptions.CandyException("Error al intentar cambiar de estado el pedido")
                            End If
                        
                        Case EntidadesDTO.PedidoDTO.PedidoEstado.PRODUCIR
                            If (estadoAnterior = EntidadesDTO.PedidoDTO.PedidoEstado.PENDIENTE) Then
                                NegocioYSeguridad.PedidoBO.getInstance().producirPedido(pedido.id)
                            Else
                                Throw New Exceptions.CandyException("Error al intentar cambiar de estado el pedido")
                            End If
                            
                        Case EntidadesDTO.PedidoDTO.PedidoEstado.FINALIZADO
                            If (estadoAnterior = EntidadesDTO.PedidoDTO.PedidoEstado.PRODUCIR) Then
                                NegocioYSeguridad.PedidoBO.getInstance().finalizarPedido(pedido.id)
                            Else
                                Throw New Exceptions.CandyException("Error al intentar cambiar de estado el pedido")
                            End If
                            
                        Case EntidadesDTO.PedidoDTO.PedidoEstado.ENTREGADO
                            If (estadoAnterior = EntidadesDTO.PedidoDTO.PedidoEstado.FINALIZADO) Then
                                NegocioYSeguridad.PedidoBO.getInstance().entregarPedido(pedido.id)
                            Else
                                Throw New Exceptions.CandyException("Error al intentar cambiar de estado el pedido")
                            End If
                    End Select
                End If
                
                NegocioYSeguridad.PedidoBO.getInstance().actualizarPedido(pedido)
                
            Else
                NegocioYSeguridad.PedidoBO.getInstance().agregarPedido(pedido)
            End If
            Response.Write("<div class='exito'>Exito! <a href='/Account/Pedidos.aspx'>Volver</a></div>")
        Catch ex As Exceptions.CandyException
            Response.Write("<div class='error'>Error! " + ex.Message + " <a href='/Account/Pedidos.aspx'>Volver</a></div>")
        End Try
    End If
ElseIf ("delete".Equals(Request.QueryString("action"))) Then
    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
    Try
        NegocioYSeguridad.PedidoBO.getInstance().cancelarPedido(id)
        Response.Write("<div class='exito'>Exito! <a href='/Account/Pedidos.aspx'>Volver</a></div>")
    Catch ex As Exceptions.CandyException
        Response.Write("<div class='error'>Error! " + ex.Message + " <a href='/Account/Pedidos.aspx'>Volver</a></div>")
    End Try
Else
    Dim pedidos As Dictionary(Of String, EntidadesDTO.PedidoDTO) = NegocioYSeguridad.PedidoBO.getInstance().obtenerPedidos()
    Dim filtroEstado As String = IIf(Request.QueryString("estado") is Nothing, "", Request.QueryString("estado"))
    Response.Write("<a href='/Account/Pedidos.aspx?estado=PENDIENTE'>Pendientes</a>")
    Response.Write("<a href='/Account/Pedidos.aspx?estado=PRODUCIR'>Produciendo</a>")
    Response.Write("<a href='/Account/Pedidos.aspx?estado=FINALIZADO'>Finalizados</a>")
    Response.Write("<a href='/Account/Pedidos.aspx?estado=ENTREGADO'>Entregados</a>")
    Response.Write("<a href='/Account/Pedidos.aspx?'>Todos</a>")
    Response.Write("<table>")
    Response.Write("<tr><td><b>ID</b></td><td><b>COMBO</b></td><td><b>ESTADO</b></td><td><b>FECHA DE ENTREGA</b></td><td><b>AGASAJADO</b></td><td><b>COMENTARIO</b></td><td><b>ACCIONES(<a href='Pedidos.aspx?action=post'>Nuevo</a>)</b></td></tr>")
    For Each pedido As EntidadesDTO.PedidoDTO In pedidos.Values
        If ("".Equals(filtroEstado) Or filtroEstado.Equals(EntidadesDTO.PedidoDTO.getPedidoEstado(pedido.estado))) Then
            Dim comboAsignado As String = NegocioYSeguridad.ComboBO.getInstance().obtenerComboPorId(pedido.comboId).nombre
            Dim comentarioStr As String = ""
            If (pedido.comentario.Length > 0) Then
                If (pedido.comentario.Length > 40) Then
                    comentarioStr = pedido.comentario.Substring(0, 40) & "..."
                Else
                    comentarioStr = pedido.comentario
                End If
            End If
        
            Dim estado As String = EntidadesDTO.PedidoDTO.getPedidoEstado(pedido.estado)
        
            Response.Write("<tr>")
            Response.Write("<td>" + pedido.id.ToString() + "</td>")
            Response.Write("<td>" + comboAsignado + "</td>")
            Response.Write("<td><b><div class='estado-" + estado.ToLower() + "'>" + estado + "</div></b></td>")
            Response.Write("<td>" + pedido.fechaEntrega + "</td>")
            Response.Write("<td>" + pedido.agasajado + "</td>")
            Response.Write("<td>" + comentarioStr + "</td>")
            Response.Write("<td>" + "<a href='/Account/Pedidos.aspx?action=delete&id=" + pedido.id.ToString() + "'>Borrar</a> " + "<a href='/Account/Pedidos.aspx?action=put&id=" + pedido.id.ToString() + "'>Modificar</a> " + "</td>")
            Response.Write("</tr>")
        End If
    Next
    Response.Write("</table>")
End If
    %>
</asp:Content>

<%@ Page Title="" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.vb" Inherits="WebCandyBar._Default" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        If (Session("corrupcion") IsNot Nothing) Then
            Response.Write("<div class='info'>" + NegocioYSeguridad.SeguridadBO.obtenerMensaje() + "</div><br>")
            Session.Remove("corrupcion")
        End If
    %>

    <%
        
        If ("post".Equals(Request.Form("action")) Or "post".Equals(Request.QueryString("action"))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim comboId As String = Request.QueryString("comboId")
                Dim comboNombre As String = IIf(Not "".Equals(comboId), NegocioYSeguridad.ComboBO.getInstance().obtenerComboPorId(Integer.Parse(comboId)).nombre, "")
    %>
    <form action="Default.aspx" method="post">
        <input type="hidden" name="action" value='post' />
        <input type="hidden" name="comboId" value='<%= comboId %>' />
        <table>
            <tr>
                <td>Combo:</td>
                <td><div class="info"><%= comboNombre%></div></td>
            </tr>
            <tr>
                <td>Tu nombre:</td>
                <td>
                    <input type="text" name="nombre" value='' /></td>
            </tr>
            <tr>
                <td>Tu email:</td>
                <td>
                    <input type="text" name="email" value='' /></td>
            </tr>
            <tr>
                <td>Tu telefono:</td>
                <td>
                    <input type="text" name="telefono" value='' /></td>
            </tr>
            <tr>
                <td>
                    <input type="submit" name="realizarPedido" value="Realizar pedido" />
                    <a href='/Default.aspx'>Volver</a></td>
            </tr>
        </table>
    </form>
    <%
    Else
        Dim comboId As String = Request.Form("comboId")
        Dim nombre As String = Request.Form("nombre")
        Dim email As String = Request.Form("email")
        Dim telefono As String = Request.Form("telefono")
                
        Try
            If (Not "".Equals(comboId) And Not "".Equals(nombre) And ((Not "".Equals(email)) Or (Not "".Equals(telefono)))) Then
                Dim pedido As New EntidadesDTO.PedidoDTO()
                pedido.comentario = nombre + "( " + email + " " + telefono + " )"
                pedido.comboId = Integer.Parse(comboId)
                pedido.agasajado = nombre
                pedido.estado = EntidadesDTO.PedidoDTO.PedidoEstado.PENDIENTE
                Dim fechaAhora As Date = Date.Now()
                pedido.fechaInicio = fechaAhora.Day.ToString + "/" + fechaAhora.Month.ToString + "/" + fechaAhora.Year.ToString
                pedido.fechaEntrega = fechaAhora.Day.ToString + "/" + fechaAhora.Month.ToString + "/" + fechaAhora.Year.ToString
                NegocioYSeguridad.PedidoBO.getInstance().agregarPedido(pedido)
                
                Response.Write("<div class='exito'>¡Felicitaciones!. Hemos realizado el pedido. Estaremos en contacto para acordar detalles! <a href='/Default.aspx'>Volver</a></div>")
            Else
                Throw New Exceptions.CandyException("Hay datos que deben ser completados")
            End If
            
        Catch ex As Exceptions.CandyException
            If Not "".Equals(comboId) Then
                Response.Write("<div class='error'>Error! " + ex.Message + " <a href='/Default.aspx?action=post&comboId=" + comboId + "'>Volver</a></div>")
            Else
                Response.Write("<div class='error'>Error! " + ex.Message + " <a href='/Default.aspx'>Volver</a></div>")
            End If
        End Try
    End If
Else
    
    Dim combos As Dictionary(Of String, EntidadesDTO.ComboDTO) = NegocioYSeguridad.ComboBO.getInstance().obtenerCombos()
    
    Response.Write("<div class='comboTemplate'>")
    For Each combo As EntidadesDTO.ComboDTO In combos.Values
        Response.Write("<div class='comboExp'>")
        Response.Write("<div class='comboExpNombre'>" + combo.nombre + "</div>")
        Response.Write("<div class='comboExpPrecio'>$" + combo.precio + "</div>")
        'Response.Write("<div class='comboExpInsumos'>$" + "INSUMOS" + "</div>")
        Response.Write("<div class='comboExpPedir'><a href='/Default.aspx?action=post&comboId=" + combo.id.ToString() + "'>Realizar pedido</a></div>")
        Response.Write("</div>")
    Next
    Response.Write("</div>")
End If
        
    %>
</asp:Content>

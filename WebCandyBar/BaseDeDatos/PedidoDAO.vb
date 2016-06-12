Public Class PedidoDAO

    Private Shared ultimoIdUtilizado As Integer

    Private Shared _instance As PedidoDAO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As PedidoDAO
        If (_instance Is Nothing) Then
            _instance = New PedidoDAO()
        End If
        Return _instance
    End Function

    Public Function actualizarPedido(pedidoDTO As EntidadesDTO.PedidoDTO) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update pedido set " _
                                                    & "combo_id=" & pedidoDTO.comboId & ", " _
                                                    & "estado='" & EntidadesDTO.PedidoDTO.getPedidoEstado(pedidoDTO.estado) & "', " _
                                                    & "agasajado='" & pedidoDTO.agasajado & "', " _
                                                    & "fecha_inicio=" & "Convert(datetime, '" & pedidoDTO.fechaInicio & "', 104)" & ", " _
                                                    & "fecha_entrega=" & "Convert(datetime, '" & pedidoDTO.fechaEntrega & "', 104)" & ", " _
                                                    & "comentario='" & pedidoDTO.comentario & "' where id=" & pedidoDTO.id)

        Return ejecutado
    End Function

    Public Function agregarPedido(pedidoDTO As EntidadesDTO.PedidoDTO) As Boolean
        Dim ejecutado = BaseDeDatos.ejecutarConsulta("insert into pedido(id, combo_id, estado, agasajado, fecha_inicio, fecha_entrega, comentario) values(" _
                                                     & pedidoDTO.id & "," _
                                                     & pedidoDTO.comboId & ",'" _
                                                     & EntidadesDTO.PedidoDTO.getPedidoEstado(pedidoDTO.estado) & "','" _
                                                     & pedidoDTO.agasajado & "'," _
                                                     & "Convert(datetime, '" & pedidoDTO.fechaInicio & "', 104)" & "," _
                                                     & "Convert(datetime, '" & pedidoDTO.fechaEntrega & "', 104)" & ",'" _
                                                     & pedidoDTO.comentario & "')")

        Return ejecutado
    End Function

    Public Function cancelarPedido(pedidoId As Integer) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("delete from pedido where id = " & pedidoId)
        Return ejecutado
    End Function

    Public Function entregarPedido(pedidoId As Integer) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update pedido set estado='ENTREGADO' where id = " & pedidoId)
        Return ejecutado
    End Function

    Public Function finalizarPedido(pedidoId As Integer) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update pedido set estado='FINALIZADO' where id = " & pedidoId)
        Return ejecutado
    End Function

    Public Function producirPedido(pedidoId As Integer) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update pedido set estado='PRODUCIR' where id = " & pedidoId)
        Return ejecutado
    End Function

    Public Function pasarPedidoAPendiente(pedidoId As Integer) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update pedido set estado='PENDIENTE' where id = " & pedidoId)
        Return ejecutado
    End Function

    Public Function obtenerPedidos() As Dictionary(Of String, EntidadesDTO.PedidoDTO)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select id, combo_id, estado, agasajado, fecha_inicio, fecha_entrega, comentario from pedido")

        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim pedidosDelSistema As New Dictionary(Of String, EntidadesDTO.PedidoDTO)
            For Each row In dataSet.Tables(0).Rows
                Dim pedidoDTO As New EntidadesDTO.PedidoDTO()
                pedidoDTO.id = row("id")
                pedidoDTO.comboId = row("combo_id")
                pedidoDTO.estado = EntidadesDTO.PedidoDTO.setPedidoEstado(row("estado"))
                pedidoDTO.agasajado = row("agasajado")
                pedidoDTO.fechaInicio = row("fecha_inicio")
                pedidoDTO.fechaEntrega = row("fecha_entrega")
                pedidoDTO.comentario = row("comentario")
                'llenar los pedidos
                pedidosDelSistema.Add(CStr(pedidoDTO.id), pedidoDTO)
            Next
            Return pedidosDelSistema
        End If
    End Function

    Public Function obtenerSiguienteID() As Integer
        If (ultimoIdUtilizado = 0) Then
            ultimoIdUtilizado = BaseDeDatos.ejecutarScalar("select isnull(max(id), 0) from pedido")
        End If
        ultimoIdUtilizado += 1
        Return ultimoIdUtilizado
    End Function

End Class

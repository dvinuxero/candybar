Public Class PedidoDTO

    Public Enum PedidoEstado
        PENDIENTE
        PRODUCIR
        FINALIZADO
        ENTREGADO
    End Enum

    Public id As Integer
    Public agasajado As String
    Public comboId As Integer
    Public estado As PedidoEstado
    Public fechaEntrega As String
    Public fechaInicio As String
    Public comentario As String

    Public Shared Function getPedidoEstado(estado As PedidoEstado) As String
        Select Case estado
            Case PedidoEstado.ENTREGADO
                Return "ENTREGADO"
            Case PedidoEstado.FINALIZADO
                Return "FINALIZADO"
            Case PedidoEstado.PENDIENTE
                Return "PENDIENTE"
            Case PedidoEstado.PRODUCIR
                Return "PRODUCIR"
        End Select

        Return "PENDIENTE"
    End Function

    Public Shared Function setPedidoEstado(estado As String) As PedidoEstado
        Select Case estado
            Case "ENTREGADO"
                Return PedidoEstado.ENTREGADO
            Case "FINALIZADO"
                Return PedidoEstado.FINALIZADO
            Case "PENDIENTE"
                Return PedidoEstado.PENDIENTE
            Case "PRODUCIR"
                Return PedidoEstado.PRODUCIR
        End Select

        Return PedidoEstado.PENDIENTE
    End Function

End Class

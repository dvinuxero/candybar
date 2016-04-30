'Clase encargada de manejar los eventos que van a guardarse en la tabla bitacora
'segun su criticidad y descripcion.

Public Class BitacoraBO

    Public Enum TipoCriticidad
        ALTA
        MEDIA
        BAJA
    End Enum

    Private Shared _instance As BitacoraBO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As BitacoraBO
        If (_instance Is Nothing) Then
            _instance = New BitacoraBO()
        End If
        Return _instance
    End Function

    Public Function guardarEvento(usuarioId As Integer, criticidad As TipoCriticidad, descripcion As String) As Boolean
        'Dim criticidadEncriptada As String = SeguridadBO.getInstance().encriptar(getCriticidad(criticidad), True)
        'Dim descripcionEncriptada As String = SeguridadBO.getInstance().encriptar(descripcion, True)
        'Dim ejecutado As Boolean = AccesoADatos.BitacoraDAO.getInstance().guardarEvento(usuarioId, criticidadEncriptada, descripcionEncriptada)
        'SeguridadBO.getInstance().calcularDVH("bitacora")
        'SeguridadBO.getInstance().calcularDVV("bitacora")
        'Return ejecutado
        Return True
    End Function

    Public Function getCriticidad(criticidad As TipoCriticidad) As String
        Select Case criticidad
            Case TipoCriticidad.ALTA
                Return "ALTA"
            Case TipoCriticidad.MEDIA
                Return "MEDIA"
            Case TipoCriticidad.BAJA
                Return "BAJA"
            Case Else
                Return Nothing
        End Select
    End Function

End Class

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
        Dim criticidadEncriptada As String = SeguridadBO.getInstance().encriptar(getCriticidad(criticidad), True)
        Dim descripcionEncriptada As String = SeguridadBO.getInstance().encriptar(descripcion, True)
        Dim ejecutado As Boolean = AccesoADatos.BitacoraDAO.getInstance().guardarEvento(usuarioId, criticidadEncriptada, descripcionEncriptada)
        SeguridadBO.getInstance().calcularDVH("bitacora")
        SeguridadBO.getInstance().calcularDVV("bitacora")
        Return ejecutado
        Return True
    End Function

    Public Function obtenerLogs(idFrom As Integer, idTo As Integer) As Dictionary(Of String, EntidadesDTO.BitacoraDTO)
        If ((idTo - idFrom) > 50) Then
            Throw New Exceptions.CandyException("Error se piden mas de 50 registros al mismo tiempo")
        End If

        If (idTo < idFrom) Then
            Throw New Exceptions.CandyException("Error no se establecieron limites correctos")
        End If

        If (idFrom < AccesoADatos.BitacoraDAO.getInstance().obtenerMinId()) Then
            idFrom = AccesoADatos.BitacoraDAO.getInstance().obtenerMinId()
        End If

        If (idFrom > AccesoADatos.BitacoraDAO.getInstance().obtenerMaxId()) Then
            idFrom = AccesoADatos.BitacoraDAO.getInstance().obtenerMaxId() - 50
        End If

        If (idTo > AccesoADatos.BitacoraDAO.getInstance().obtenerMaxId()) Then
            If (idFrom + 50 < AccesoADatos.BitacoraDAO.getInstance().obtenerMaxId()) Then
                idTo = idFrom + 50
            Else
                idTo = AccesoADatos.BitacoraDAO.getInstance().obtenerMaxId()
            End If
        End If

        Dim logs As Dictionary(Of String, EntidadesDTO.BitacoraDTO) = AccesoADatos.BitacoraDAO.getInstance().obtenerLogs(idFrom, idTo)

        For Each log As EntidadesDTO.BitacoraDTO In logs.Values
            log.descripcion = NegocioYSeguridad.SeguridadBO.getInstance().desencriptar(log.descripcion)
            log.criticidad = NegocioYSeguridad.SeguridadBO.getInstance().desencriptar(log.criticidad)
        Next

        Return logs
    End Function

    Public Function obtenerMinId() As Integer
        AccesoADatos.BitacoraDAO.getInstance().obtenerMinId()
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

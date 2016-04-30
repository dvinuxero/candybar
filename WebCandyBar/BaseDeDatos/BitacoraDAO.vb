'Clase encargada de guardar los diferentes eventos en la tabla de bitacora

Public Class BitacoraDAO

    Private Shared ultimoIdBitacoraUtilizado As Integer

    Private Shared _instance As BitacoraDAO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As BitacoraDAO
        If (_instance Is Nothing) Then
            _instance = New BitacoraDAO()
        End If
        Return _instance
    End Function

    Public Function guardarEvento(usuarioId As Integer, criticidad As String, descripcion As String) As Boolean
        Dim id As Integer = obtenerSiguienteIDBitacora()
        Dim nowDate As Date = Now()

        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("insert into bitacora(id, usuario_id, fecha, descripcion, nivel_criticidad) values(" & id & "," & usuarioId & ", SYSDATETIME(), '" & descripcion & "', '" & criticidad & "')")
        Return ejecutado
    End Function

    Public Function obtenerSiguienteIDBitacora() As Integer
        If (ultimoIdBitacoraUtilizado = 0) Then
            ultimoIdBitacoraUtilizado = BaseDeDatos.ejecutarScalar("select isnull(max(id), 0) from bitacora")
        End If
        ultimoIdBitacoraUtilizado += 1
        Return ultimoIdBitacoraUtilizado
    End Function

End Class

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

    Public Function obtenerLogs(idFrom As Integer, idTo As Integer) As Dictionary(Of String, EntidadesDTO.BitacoraDTO)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("SELECT id, usuario_id, fecha, descripcion, nivel_criticidad FROM bitacora where id > " + idFrom.ToString() + " and id < " + idTo.ToString() + " order by fecha asc")

        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim logs As New Dictionary(Of String, EntidadesDTO.BitacoraDTO)
            For Each row In dataSet.Tables(0).Rows
                Dim log As New EntidadesDTO.BitacoraDTO()
                log.id = row("id")
                log.usuarioId = row("usuario_id")
                log.fecha = row("fecha")
                log.descripcion = row("descripcion")
                log.criticidad = row("nivel_criticidad")
                logs.Add(CStr(log.id), log)
            Next
            Return logs
        End If
    End Function

    Public Function obtenerMinId() As Integer
        Return BaseDeDatos.ejecutarScalar("select isnull(min(id), 0) from bitacora")
    End Function

    Public Function obtenerMaxId() As Integer
        Return BaseDeDatos.ejecutarScalar("select isnull(max(id), 0) from bitacora")
    End Function

    Public Function obtenerSiguienteIDBitacora() As Integer
        If (ultimoIdBitacoraUtilizado = 0) Then
            ultimoIdBitacoraUtilizado = BaseDeDatos.ejecutarScalar("select isnull(max(id), 0) from bitacora")
        End If
        ultimoIdBitacoraUtilizado += 1
        Return ultimoIdBitacoraUtilizado
    End Function

End Class

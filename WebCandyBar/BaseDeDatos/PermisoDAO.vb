'Clase encargada de manejar los permisos y familias de permisos de los usuarios

Public Class PermisoDAO

    Private Shared _instance As PermisoDAO

    Public Shared PATENTE_NEGADA_FLAG As String = "::SI::"

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As PermisoDAO
        If (_instance Is Nothing) Then
            _instance = New PermisoDAO()
        End If
        Return _instance
    End Function

    Public Function asociarFamiliasAlUsuario(usuarioId As Integer, familias As List(Of String)) As Boolean
        Try
            eliminarFamiliasDelUsuario(usuarioId)
            For Each familia In familias
                BaseDeDatos.ejecutarConsulta("insert into usuario_familia(familia_id, usuario_id) values('" & familia & "'," & usuarioId & ")")
            Next
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function asociarPatentesAlUsuario(usuarioId As Integer, patentes As List(Of String)) As Boolean
        Try
            eliminarPatentesDelUsuario(usuarioId)
            For Each patente In patentes
                If (patente.Contains(PATENTE_NEGADA_FLAG)) Then
                    Dim patenteSinNegado As String = patente.Replace(PATENTE_NEGADA_FLAG, "")
                    BaseDeDatos.ejecutarConsulta("insert into usuario_patente(patente_id, usuario_id, negado) values('" & patenteSinNegado & "'," & usuarioId & ",'" & PATENTE_NEGADA_FLAG & "')")
                Else
                    BaseDeDatos.ejecutarConsulta("insert into usuario_patente(patente_id, usuario_id, negado) values('" & patente & "'," & usuarioId & ", 'NO')")
                End If
            Next
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function asociarPatentesDeLaFamilia(familiaId As String, patentes As List(Of String)) As Boolean
        Try
            eliminarPatentesDeLaFamilia(familiaId)
            For Each patente In patentes
                BaseDeDatos.ejecutarConsulta("insert into familia_patente(patente_id, familia_id) values('" & patente & "','" & familiaId & "')")
            Next
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function eliminarFamiliasDelUsuario(usuarioId As Integer) As Boolean
        Try
            BaseDeDatos.ejecutarConsulta("delete from usuario_familia where usuario_id=" & usuarioId)
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function actualizarFamiliasDelUsuario(familiaIdAnterior As String, familiaId As String) As Boolean
        Try
            BaseDeDatos.ejecutarConsulta("update usuario_familia set familia_id='" & familiaId & "' where familia_id='" & familiaIdAnterior & "'")
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    'utilizamos este metodo para eliminar las familias del usuario cuando se borra una familia del sistema
    Public Function eliminarFamiliasDelUsuario(familiaId As String) As Boolean
        Try
            BaseDeDatos.ejecutarConsulta("delete from usuario_familia where familia_id='" & familiaId & "'")
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function eliminarPatentesDelUsuario(usuarioId As Integer) As Boolean
        Try
            BaseDeDatos.ejecutarConsulta("delete from usuario_patente where usuario_id=" & usuarioId)
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function eliminarPatentesDeLaFamilia(familiaId As String) As Boolean
        Try
            BaseDeDatos.ejecutarConsulta("delete from familia_patente where familia_id='" & familiaId & "'")
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    'cambiar firma del metodo en el analisis
    Public Function obtenerFamilias() As Dictionary(Of String, String)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select id, descripcion from familia")
        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim familias As New Dictionary(Of String, String)
            For Each row In dataSet.Tables(0).Rows
                familias.Add(row("id"), row("descripcion"))
            Next
            Return familias
        End If
    End Function

    Public Function obtenerFamiliasPorUsuario(usuarioId As Integer) As List(Of String)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select familia_id from usuario_familia where usuario_id = " & usuarioId)
        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim familiasPorUsuario As New List(Of String)
            For Each row In dataSet.Tables(0).Rows
                familiasPorUsuario.Add(row("familia_id"))
            Next
            Return familiasPorUsuario
        End If
    End Function

    Public Function obtenerPatentes() As Dictionary(Of String, String)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select id, descripcion from patente")
        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim patentes As New Dictionary(Of String, String)
            For Each row In dataSet.Tables(0).Rows
                patentes.Add(row("id"), row("descripcion"))
            Next
            Return patentes
        End If
    End Function

    Public Function obtenerPatentesPorUsuario(usuarioId As Integer) As List(Of String)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select patente_id, negado from usuario_patente where usuario_id = " & usuarioId)
        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim patentesPorUsuario As New List(Of String)
            For Each row In dataSet.Tables(0).Rows
                Dim dataRow As String = row("patente_id")
                If (PATENTE_NEGADA_FLAG.Equals(row("negado"))) Then
                    dataRow = dataRow & PATENTE_NEGADA_FLAG
                End If
                patentesPorUsuario.Add(dataRow)
            Next

            Return patentesPorUsuario
        End If
    End Function

    Public Function obtenerPatentesPorFamilia(familiaId As String) As List(Of String)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select patente_id, familia_id from familia_patente where familia_id = '" & familiaId & "'")
        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim patentesPorFamilia As New List(Of String)
            For Each row In dataSet.Tables(0).Rows
                patentesPorFamilia.Add(row("patente_id"))
            Next

            Return patentesPorFamilia
        End If
    End Function

    Public Function agregarFamilia(familiaId As String, familiaDescripcion As String) As Boolean
        Dim ejecutado = BaseDeDatos.ejecutarConsulta("insert into familia(id, descripcion) values('" & familiaId & "','" & familiaDescripcion & "')")
        Return ejecutado
    End Function

    Public Function modificarFamilia(familiaId As String, familiaDescripcion As String) As Boolean
        Dim ejecutado = BaseDeDatos.ejecutarConsulta("update familia set id='" & familiaId & "', descripcion='" & familiaDescripcion & "' where id='" & familiaId & "')")
        Return ejecutado
    End Function

    Public Function eliminarFamilia(familiaId As String) As Boolean
        Dim ejecutado = BaseDeDatos.ejecutarConsulta("delete from familia where id='" & familiaId & "'")
        Return ejecutado
    End Function

End Class

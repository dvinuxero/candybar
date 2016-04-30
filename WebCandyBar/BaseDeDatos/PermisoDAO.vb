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

End Class

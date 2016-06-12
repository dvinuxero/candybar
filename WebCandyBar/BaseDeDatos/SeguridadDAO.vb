'Clase encargada de realizar diferentes consultas para mantener la integridad y seguridad de las tablass de la base de datos

Public Class SeguridadDAO

    Private Shared _instance As SeguridadDAO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As SeguridadDAO
        If (_instance Is Nothing) Then
            _instance = New SeguridadDAO()
        End If
        Return _instance
    End Function

    'cambia la firma en analisis
    Public Function buscarDVHYRegistrosPorTablaPendientes(campos As List(Of String), tabla As String) As DataSet
        Dim selectCampos As String = ""
        Dim primero As Boolean = True
        For Each campo In campos
            If (Not primero) Then
                selectCampos += ", "
            End If
            selectCampos += campo
            primero = False
        Next

        selectCampos += ", dvh"

        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select " & selectCampos & " from " & tabla & " where dvh is null")

        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Return dataSet
        End If
    End Function

    'actualizo un set de valores de dvh directamente, eran los q anteriormente estaban en null
    Public Function actualizarDVHPorTabla(dataSet As DataSet, campos As List(Of String), tabla As String) As Boolean
        Dim selectCampos As String = ""
        Dim primero As Boolean = True
        For Each campo In campos
            If (Not primero) Then
                selectCampos += ", "
            End If
            selectCampos += campo
            primero = False
        Next
        selectCampos += ", dvh"
        Dim consulta As String = "select " & selectCampos & " from " & tabla & " where dvh is null"
        Dim bulkDVHEjecutado As Boolean = BaseDeDatos.actualizarDataSetBulk(consulta, tabla, dataSet)
        Return bulkDVHEjecutado
    End Function

    'actualizo un set de valores de dvh directamente, eran los q anteriormente estaban en null
    Public Function actualizarDVVPorTabla(tabla As String) As Boolean
        Dim dvhTotal As Long = BaseDeDatos.ejecutarScalar("select sum(dvh) from " & tabla & " where dvh is not null")
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("delete from digit_vv where tabla_nombre = '" & tabla & "'")
        ejecutado = BaseDeDatos.ejecutarConsulta("insert into digit_vv(tabla_nombre, dvv) values('" & tabla & "'," & dvhTotal & ")")
        Return ejecutado
    End Function

    'valor null en dvh me indica que es un registro auditado, que no coincidieron los valores calculados y necesita corregirse
    Public Function marcarErrorEnDVHPorRegistro(tabla As String, dvh As Long) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update " & tabla & " set dvh=null where dvh=" & dvh)
        Return ejecutado
    End Function

    'valor null en dvv cuando no coincidieron los valores calculados y necesita corregirse
    Public Function marcarErrorEnDVVPorTabla(tabla As String) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update digit_vv set dvv=null where tabla_nombre=" & tabla)
        Return ejecutado
    End Function

    'cambia la firma en analisis
    Public Function buscarDVHYRegistrosPorTabla(campos As List(Of String), tabla As String) As Dictionary(Of String, Long)
        Dim selectCampos As String = ""
        Dim primero As Boolean = True
        For Each campo In campos
            If (Not primero) Then
                selectCampos += ", "
            End If
            selectCampos += campo
            primero = False
        Next

        selectCampos += ", dvh"

        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select " & selectCampos & " from " & tabla)

        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim registrosYDVH As New Dictionary(Of String, Long)
            For Each row In dataSet.Tables(0).Rows
                Dim cadenaACalcularDVH As String = ""
                For Each campo In campos
                    cadenaACalcularDVH += CStr(row(campo))
                Next
                If (IsDBNull(row("dvh"))) Then
                    Return Nothing
                Else
                    Try
                        registrosYDVH.Add(cadenaACalcularDVH, CLng(row("dvh")))
                    Catch exception As Exception
                    End Try
                End If
            Next
            Return registrosYDVH
        End If
    End Function

    Public Function buscarValoresDVV() As Dictionary(Of String, Long)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select tabla_nombre, dvv from digit_vv")

        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim mapeoTablaDVV As New Dictionary(Of String, Long)
            For Each row In dataSet.Tables(0).Rows
                mapeoTablaDVV.Add(row("tabla_nombre"), CLng(row("dvv")))
            Next
            Return mapeoTablaDVV
        End If
    End Function

End Class

Imports System.Data.SqlClient

'Pasos para configurar la conexion a la base de datos
'Instalar candy bar desktop y seguir los pasos de instalacion
'copiar string de conexion y pegarlo en las settings de esta solucion

'Problemas:
'Ensamblado de Optimizations: https://www.nuget.org/packages/Microsoft.AspNet.Web.Optimization.WebForms/
'Correr el PM(package manager de vstudio desde tools e instalarlo )
'PM> Install-Package
'id> Microsoft.AspNet.Web.Optimization.WebForms

'Agregar las nuevas patentes al sistema para tener todos los permisos
'INSERT INTO [candybar].[dbo].[patente]([id],[descripcion]) VALUES('P010_LOGS_ALL','') GO
'INSERT INTO [candybar].[dbo].[usuario_patente]([usuario_id],[patente_id],[negado],[dvh]) VALUES(1,'P010_LOGS_ALL','NO',1) GO
'PATENTES: En Site.Master

Public Class BaseDeDatos

    'conexion a la base de datos
    Private Shared objectS As Object = {0}
    Private Shared conexion As SqlConnection

    'sql commands cacheados para poder ejecutar los updates de dvh contra la base de datos
    Private Shared updateCommandsPorTabla As Dictionary(Of String, SqlCommand)

    Public Shared Function obtenerStringConexion() As String
        Return My.Settings.StringDeConexion
    End Function

    'Ver de cambiar el analisis porque hay cambios en la modalidad de conexion a la base de datos
    Shared Sub New()
        Try
            Dim stringConexion As String = obtenerStringConexion()
            If (stringConexion Is Nothing Or "".Equals(stringConexion)) Then
                Throw New Exceptions.CandyException("Error no se puede conectar a la base de datos, falta string de conexion", True)
            Else
                conexion = New SqlConnection(stringConexion)
            End If

            updateCommandsPorTabla = New Dictionary(Of String, SqlCommand)
            'tabla familia_patente
            Dim updCommandFamiliaPatente = New SqlCommand("update familia_patente set dvh = @dvh WHERE patente_id = @patente_id and familia_id = @familia_id", conexion)
            updCommandFamiliaPatente.Parameters.Add("@dvh", SqlDbType.BigInt, 8, "dvh")
            updCommandFamiliaPatente.Parameters.Add("@familia_id", SqlDbType.VarChar, 30, "familia_id")
            updCommandFamiliaPatente.Parameters.Add("@patente_id", SqlDbType.VarChar, 30, "patente_id")
            updateCommandsPorTabla.Add("familia_patente", updCommandFamiliaPatente)
            'tabla usuario_patente
            Dim updCommandUsuarioPatente = New SqlCommand("update usuario_patente set dvh = @dvh WHERE usuario_id = @usuario_id and patente_id = @patente_id", conexion)
            updCommandUsuarioPatente.Parameters.Add("@dvh", SqlDbType.BigInt, 8, "dvh")
            updCommandUsuarioPatente.Parameters.Add("@usuario_id", SqlDbType.Int, 4, "usuario_id")
            updCommandUsuarioPatente.Parameters.Add("@patente_id", SqlDbType.VarChar, 30, "patente_id")
            updCommandUsuarioPatente.Parameters.Add("@negado", SqlDbType.VarChar, 10, "negado")
            updateCommandsPorTabla.Add("usuario_patente", updCommandUsuarioPatente)
            'tabla bitacora
            Dim updCommandBitacora = New SqlCommand("update bitacora set dvh = @dvh WHERE id = @id and usuario_id = @usuario_id and fecha = @fecha and descripcion = @descripcion and nivel_criticidad = @nivel_criticidad", conexion)
            updCommandBitacora.Parameters.Add("@dvh", SqlDbType.BigInt, 8, "dvh")
            updCommandBitacora.Parameters.Add("@id", SqlDbType.Int, 4, "id")
            updCommandBitacora.Parameters.Add("@usuario_id", SqlDbType.Int, 4, "usuario_id")
            updCommandBitacora.Parameters.Add("@fecha", SqlDbType.DateTime, 8, "fecha")
            updCommandBitacora.Parameters.Add("@descripcion", SqlDbType.VarChar, 120, "descripcion")
            updCommandBitacora.Parameters.Add("@nivel_criticidad", SqlDbType.VarChar, 10, "nivel_criticidad")
            updateCommandsPorTabla.Add("bitacora", updCommandBitacora)
            'tabla insumo
            Dim updCommandInsumo = New SqlCommand("update insumo set dvh = @dvh WHERE nombre = @nombre and precio_unidad = @precio_unidad and stock = @stock", conexion)
            updCommandInsumo.Parameters.Add("@dvh", SqlDbType.BigInt, 8, "dvh")
            updCommandInsumo.Parameters.Add("@nombre", SqlDbType.VarChar, 160, "nombre")
            updCommandInsumo.Parameters.Add("@precio_unidad", SqlDbType.VarChar, 40, "precio_unidad")
            updCommandInsumo.Parameters.Add("@stock", SqlDbType.VarChar, 40, "stock")
            updateCommandsPorTabla.Add("insumo", updCommandInsumo)
            'tabla combo
            Dim updCommandCombo = New SqlCommand("update combo set dvh = @dvh WHERE nombre = @nombre and precio = @precio", conexion)
            updCommandCombo.Parameters.Add("@dvh", SqlDbType.BigInt, 8, "dvh")
            updCommandCombo.Parameters.Add("@nombre", SqlDbType.VarChar, 400, "nombre")
            updCommandCombo.Parameters.Add("@precio", SqlDbType.VarChar, 40, "precio")
            updateCommandsPorTabla.Add("combo", updCommandCombo)

        Catch exception As Exception
        End Try
    End Sub

    Private Shared Sub desconectarBD(conexion As SqlClient.SqlConnection)
        SyncLock objectS
            If (conexion IsNot Nothing) Then
                conexion.Close()
            End If
        End SyncLock
    End Sub

    Public Overloads Shared Function ejecutarConsulta(consulta As String) As Boolean
        SyncLock objectS
            Try
                Dim sqlCommand As SqlCommand = New SqlCommand(consulta, conexion)
                conexion.Open()
                Dim acceptRows As Integer = sqlCommand.ExecuteNonQuery()
                If (acceptRows > 0) Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                If (ex.Message.Contains("UNIQUE KEY 'nickname'")) Then
                    Throw New Exceptions.CandyException("Ya existe ese nickname, por favor cambielo", True)
                Else
                    Return False
                End If
            Finally
                desconectarBD(conexion)
            End Try
        End SyncLock
    End Function

    Friend Overloads Shared Function ejecutarConsulta(consulta As String, conexionBackUp As SqlConnection) As Boolean
        SyncLock objectS
            Try
                Dim sqlCommand As SqlCommand = New SqlCommand(consulta, conexionBackUp)
                conexionBackUp.Open()
                Dim acceptRows As Integer = sqlCommand.ExecuteNonQuery()
                If (acceptRows > 0) Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                Return False
            Finally
                desconectarBD(conexionBackUp)
            End Try
        End SyncLock
    End Function

    Public Shared Function listarConsulta(consulta As String) As DataSet
        SyncLock objectS
            Try
                Dim adapter As New SqlDataAdapter(consulta, conexion)
                Dim dataSet As New DataSet()

                conexion.Open()
                adapter.Fill(dataSet)

                Return dataSet
            Catch ex As Exception
                Return Nothing
            Finally
                desconectarBD(conexion)
            End Try
        End SyncLock
    End Function

    Public Shared Function ejecutarScalar(ByVal selectSql As String) As Long
        SyncLock objectS
            Dim scalarResult As Long = 0
            Try
                Dim command As New SqlCommand(selectSql, conexion)
                conexion.Open()
                Try
                    scalarResult = CInt(command.ExecuteScalar())
                Catch ex As InvalidCastException
                    scalarResult = 0
                End Try
            Catch ex As Exception
                scalarResult = 0
            Finally
                desconectarBD(conexion)
            End Try
            Return scalarResult
        End SyncLock
    End Function

    Friend Shared Function actualizarDataSetBulk(consulta As String, tabla As String, dataBulk As DataSet) As Boolean
        SyncLock objectS
            Try
                Dim adapter As New SqlClient.SqlDataAdapter(consulta, conexion)
                adapter.UpdateCommand = updateCommandsPorTabla.Item(tabla)
                adapter.Update(dataBulk)
                dataBulk.AcceptChanges()
                Return True
            Catch ex As Exception
                Return Nothing
            Finally
                desconectarBD(conexion)
            End Try
        End SyncLock
    End Function

    Public Shared Function realizarBackUp(listaDeBackups As List(Of String)) As Boolean
        SyncLock objectS
            Try
                Dim unidadesDeDisco As String = ""
                Dim primero As Boolean = True
                For Each backup In listaDeBackups
                    If (Not primero) Then
                        unidadesDeDisco += ", "
                    End If
                    unidadesDeDisco += "DISK = N'" & backup & "'"
                    primero = False
                Next

                Dim sqlBackUp As String = "BACKUP DATABASE [candybar] TO " _
                                            & unidadesDeDisco _
                                            & " WITH NOFORMAT, " _
                                            & "NOINIT,  " _
                                            & "NAME = N'backup', " _
                                            & "SKIP, " _
                                            & "NOREWIND, " _
                                            & "NOUNLOAD,  " _
                                            & "STATS = 10"
                'ejecuta el backup
                ejecutarConsulta(sqlBackUp, conexion)

                Return True
            Catch ex As Exception
                Return False
            Finally
            End Try
        End SyncLock
    End Function

    Public Shared Function realizarRestore(listaDeRestores As List(Of String)) As Boolean
        SyncLock objectS
            Try
                Dim unidadesDeDisco As String = ""
                Dim primero As Boolean = True
                For Each restore In listaDeRestores
                    If (Not primero) Then
                        unidadesDeDisco += ", "
                    End If
                    unidadesDeDisco += "DISK = N'" & restore & "'"
                    primero = False
                Next

                Dim sqlBackUp As String = "ALTER DATABASE [candybar] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; USE master; RESTORE DATABASE [candybar] FROM " _
                                            & unidadesDeDisco _
                                            & " WITH REPLACE, " _
                                            & "NOUNLOAD,  " _
                                            & "STATS = 10"
                'ejecuta el restore
                ejecutarConsulta(sqlBackUp, conexion)

                Return True
            Catch ex As Exception
                Return False
            Finally
            End Try
        End SyncLock
    End Function

End Class

'Clase encargada de realizar las diferentes operaciones de ABM de usuarios sobre la base de datos

Public Class UsuarioDAO

    Private Shared ultimoIdUtilizado As Integer

    Public Shared _instance As UsuarioDAO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As UsuarioDAO
        If (_instance Is Nothing) Then
            _instance = New UsuarioDAO()
        End If
        Return _instance
    End Function

    Public Function obtenerUsuario(nickname As String) As EntidadesDTO.UsuarioDTO
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select id, nombre, apellido, nickname, password, intentos_incorrectos, baja, lang from usuario where nickname = '" & nickname & "'")

        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim usuario As New EntidadesDTO.UsuarioDTO()
            If (dataSet.Tables(0).Rows.Count > 0) Then
                usuario.id = dataSet.Tables(0).Rows(0)("id")
                usuario.nombre = dataSet.Tables(0).Rows(0)("nombre")
                usuario.apellido = dataSet.Tables(0).Rows(0)("apellido")
                usuario.nickname = dataSet.Tables(0).Rows(0)("nickname")
                usuario.password = dataSet.Tables(0).Rows(0)("password")
                usuario.intentosIncorrectos = dataSet.Tables(0).Rows(0)("intentos_incorrectos")
                usuario.baja = dataSet.Tables(0).Rows(0)("baja")
                usuario.lang = dataSet.Tables(0).Rows(0)("lang")

                Return usuario
            Else
                Return Nothing
            End If
        End If
    End Function

    Public Function obtenerUsuarios() As Dictionary(Of String, EntidadesDTO.UsuarioDTO)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select id, nombre, apellido, nickname, password, intentos_incorrectos, baja, lang from usuario")

        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim usuariosRegistrados As New Dictionary(Of String, EntidadesDTO.UsuarioDTO)
            For Each row In dataSet.Tables(0).Rows
                Dim usuario As New EntidadesDTO.UsuarioDTO()
                usuario.id = row("id")
                usuario.nombre = row("nombre")
                usuario.apellido = row("apellido")
                usuario.nickname = row("nickname")
                usuario.password = row("password")
                usuario.intentosIncorrectos = row("intentos_incorrectos")
                usuario.baja = row("baja")
                usuario.lang = row("lang")
                usuariosRegistrados.Add(CStr(usuario.id), usuario)
            Next
            Return usuariosRegistrados
        End If
    End Function

    Public Function agregarUsuario(usuarioDTO As EntidadesDTO.UsuarioDTO) As Boolean
        Dim ejecutado = BaseDeDatos.ejecutarConsulta("insert into usuario(id, nombre, apellido, nickname, password, intentos_incorrectos, baja, lang) values(" _
                                                     & usuarioDTO.id & ",'" _
                                                     & usuarioDTO.nombre & "','" _
                                                     & usuarioDTO.apellido & "','" _
                                                     & usuarioDTO.nickname & "','" _
                                                     & usuarioDTO.password & "'," _
                                                     & usuarioDTO.intentosIncorrectos & ",'" _
                                                     & usuarioDTO.baja & "','" _
                                                     & usuarioDTO.lang & "')")
        Return ejecutado
    End Function

    Public Function modificarUsuario(usuarioDTO As EntidadesDTO.UsuarioDTO) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update usuario set " _
                                                     & "nombre='" & usuarioDTO.nombre & "', " _
                                                     & "apellido='" & usuarioDTO.apellido & "', " _
                                                     & "nickname='" & usuarioDTO.nickname & "', " _
                                                     & "lang='" & usuarioDTO.lang & "' where id=" & usuarioDTO.id)
        Return ejecutado
    End Function

    Public Function eliminarUsuario(usuarioDTO As EntidadesDTO.UsuarioDTO) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("delete from usuario where id = " & usuarioDTO.id)
        'tener en cuenta de eliminar en cadena todas sus dependencias como las patentes, familias, etc
        Return ejecutado
    End Function

    Public Function bloquearUsuario(usuarioId As Integer) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update usuario set baja='SI' where id=" & usuarioId)
        Return ejecutado
    End Function

    'cambia firma del metodo en el analisis
    Public Function desbloquearUsuario(usuarioId As Integer) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update usuario set baja='NO', intentos_incorrectos=0 where id=" & usuarioId)
        Return ejecutado
    End Function

    'ver de cambiar la firma en el analisis, para evitar llamar al usuario y saber los intentos incorrectos, le paso el que tengo calculado en memoria
    Public Sub marcarIntentoIncorrecto(usuarioId As Integer, intentoIncorrectoActual As Integer)
        BaseDeDatos.ejecutarConsulta("update usuario set intentos_incorrectos=" & intentoIncorrectoActual & " where id=" & usuarioId)
    End Sub

    'cambia firma en el analisis, se agrega el usuarioId
    Public Function cambiarContrasenia(usuarioId As Integer, contraseniaNueva As String) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update usuario set password='" & contraseniaNueva & "' where id=" & usuarioId)
        Return ejecutado
    End Function

    Public Function obtenerSiguienteID() As Integer
        If (ultimoIdUtilizado = 0) Then
            ultimoIdUtilizado = BaseDeDatos.ejecutarScalar("select isnull(max(id), 0) from usuario")
        End If
        ultimoIdUtilizado += 1
        Return ultimoIdUtilizado
    End Function

End Class

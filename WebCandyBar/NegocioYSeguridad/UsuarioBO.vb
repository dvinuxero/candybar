'Clase encargada de manejar la logica del usuario como loguearse al sistema, ABM de usuarios y validaciones especiales

Public Class UsuarioBO

    Private Shared _instance As UsuarioBO
    Public Shared USUARIO_ID_NUEVO As Integer = 0
    Private Shared CARACTERES_VALIDOS As String = "abcdefghijklmnñopqrstuvwxyz0123456789_-"

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As UsuarioBO
        If (_instance Is Nothing) Then
            _instance = New UsuarioBO()
        End If
        Return _instance
    End Function

    'metodo para agregar usuarios nuevos
    Public Function agregarUsuario(usuarioNuevo As EntidadesDTO.UsuarioDTO) As Boolean
        'If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(usuarioLogueado.id, "P27_USUARIOS_ALTA")) Then
        '    Throw New Exceptions.CandyException("Usuario no tiene permiso para agregar usuarios", True)
        'End If
        Try
            '1 valida
            validarParaAgregar(usuarioNuevo, True)
            usuarioNuevo.id = AccesoADatos.UsuarioDAO.getInstance().obtenerSiguienteID()
            usuarioNuevo.nombre = SeguridadBO.getInstance().encriptar(usuarioNuevo.nombre, True)
            usuarioNuevo.password = SeguridadBO.getInstance().autogenerarContrasenia()
            Dim contraseniaSinEncriptar As String = usuarioNuevo.password
            usuarioNuevo.password = SeguridadBO.getInstance().encriptar(contraseniaSinEncriptar, False)
            usuarioNuevo.intentosIncorrectos = 0
            usuarioNuevo.baja = EntidadesDTO.UsuarioDTO.BAJA_FLAG
            '2 insert en la base
            AccesoADatos.UsuarioDAO.getInstance().agregarUsuario(usuarioNuevo)
            usuarioNuevo.nombre = SeguridadBO.getInstance().desencriptar(usuarioNuevo.nombre)
            '6 se guarda la password en un directorio simulado
            SeguridadBO.getInstance().informarPasswordAlUsuario(usuarioNuevo.id, contraseniaSinEncriptar)
            BitacoraBO.getInstance().guardarEvento(usuarioNuevo.id, BitacoraBO.TipoCriticidad.MEDIA, "Usuario nuevo id " & usuarioNuevo.id)
            Return True
        Catch exception As Exceptions.CandyException
            'si hubo errores informativos se muestran
            If (exception.informarEstado) Then
                Throw exception
            Else
                Return False
            End If
        End Try
    End Function

    'metodo cambia de firma le pasamos el usuarioDTO y la respuesta es un throw exception para poder tener un mensaje
    Public Function validarParaAgregar(usuarioDTO As EntidadesDTO.UsuarioDTO, esNuevo As Boolean) As Boolean

        'validacion de nombre
        If ("".Equals(usuarioDTO.nombre)) Then
            BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion del usuario")
            Throw New Exceptions.CandyException("Error el nombre no puede ser vacio", True)
        End If
        If (usuarioDTO.nombre.Length > 30 Or usuarioDTO.nombre.Length < 2) Then
            BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion del usuario")
            Throw New Exceptions.CandyException("Error el nombre debe ser entre 2 y 30 caracteres", True)
        End If
        If (Not usaCaracteresValidos(usuarioDTO.nombre)) Then
            BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error caracteres no validos en el nombre")
            Throw New Exceptions.CandyException("Error caracteres no validos en el nombre", True)
        End If

        'validacion de apellido
        If ("".Equals(usuarioDTO.apellido)) Then
            BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion del usuario")
            Throw New Exceptions.CandyException("Error el apellido no puede ser vacio", True)
        End If
        If (usuarioDTO.apellido.Length > 30 Or usuarioDTO.apellido.Length < 2) Then
            BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion del usuario")
            Throw New Exceptions.CandyException("Error el apellido debe ser entre 2 y 30 caracteres", True)
        End If
        If (Not usaCaracteresValidos(usuarioDTO.apellido)) Then
            BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error caracteres no validos en el apellido")
            Throw New Exceptions.CandyException("Error caracteres no validos en el apellido", True)
        End If

        'validacion de nickname
        If ("".Equals(usuarioDTO.nickname)) Then
            BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion del usuario")
            Throw New Exceptions.CandyException("Error el nickname no puede ser vacio", True)
        End If
        If (usuarioDTO.nickname.Length > 30 Or usuarioDTO.nickname.Length < 2) Then
            BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion del usuario")
            Throw New Exceptions.CandyException("Error el nickname debe ser entre 2 y 30 caracteres", True)
        End If
        If (Not usaCaracteresValidos(usuarioDTO.nickname)) Then
            BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error caracteres no validos en el nickname")
            Throw New Exceptions.CandyException("Error caracteres no validos en el nickname", True)
        End If
        'valido que el nickname no exista ya que es unico en la base de datos
        If (esNuevo) Then
            Dim existeUsuario As EntidadesDTO.UsuarioDTO = AccesoADatos.UsuarioDAO.getInstance().obtenerUsuario(usuarioDTO.nickname)
            If (existeUsuario IsNot Nothing) Then
                If (existeUsuario.nickname.Equals(usuarioDTO.nickname)) Then
                    BitacoraBO.getInstance().guardarEvento(usuarioDTO.id, BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion del usuario")
                    Throw New Exceptions.CandyException("Error el nickname ya existe en la base de datos", True)
                End If
            End If
        End If

        Return True
    End Function

    Private Function usaCaracteresValidos(cadena As String) As Boolean
        Dim valido As Boolean = True
        For Each c In cadena
            If (Not CARACTERES_VALIDOS.Contains(c.ToString.ToLower())) Then
                valido = False
                Exit For
            End If
        Next
        Return valido
    End Function

    'Metodo nuevo que se agregara al analisis
    Public Overloads Function esUsuarioAdministrador(usuarioId As Integer) As Boolean
        Return PermisoBO.getInstance().tieneTodasLasPatentesEscenciales(usuarioId)
    End Function

    'Metodo especializado en realizar el logueo y chequeos importantes antes de iniciar la entrada al sistema
    'cambio un poco la logica respecto al analisis
    Public Sub loguearUsuario(nickname As String, password As String)
        Try
            Dim passwordEncriptada As String = SeguridadBO.getInstance().encriptar(password, False)

            Dim usuarioLogueado As EntidadesDTO.UsuarioDTO = AccesoADatos.UsuarioDAO.getInstance().obtenerUsuario(nickname)

            If (usuarioLogueado Is Nothing) Then
                Throw New Exceptions.CandyException("El usuario no existe")
            End If

            Dim contraseniaInvalida As Boolean = False

            If (Not passwordEncriptada.Equals(usuarioLogueado.password)) Then
                contraseniaInvalida = True
                'guardo evento en bitacora password incorrecta
                BitacoraBO.getInstance().guardarEvento(usuarioLogueado.id, BitacoraBO.TipoCriticidad.MEDIA, "Error al loguear usuario password incorrecta")
                If (usuarioLogueado.intentosIncorrectos < 3) Then
                    usuarioLogueado.intentosIncorrectos += 1
                    AccesoADatos.UsuarioDAO.getInstance().marcarIntentoIncorrecto(usuarioLogueado.id, usuarioLogueado.intentosIncorrectos)
                End If
            End If

            If (usuarioLogueado.intentosIncorrectos = 3 And "NO".Equals(usuarioLogueado.baja) And Not esUsuarioAdministrador(usuarioLogueado.id)) Then
                'marcar evento en bitacora se bloquea usuario
                BitacoraBO.getInstance().guardarEvento(usuarioLogueado.id, BitacoraBO.TipoCriticidad.ALTA, "Usuario bloqueado")
                usuarioLogueado.baja = "SI"
                If (Not AccesoADatos.UsuarioDAO.getInstance().bloquearUsuario(usuarioLogueado.id)) Then
                    Throw New Exceptions.CandyException("Error al intentar bloquear el usuario")
                End If
            End If

            If ("SI".Equals(usuarioLogueado.baja)) Then
                Throw New Exceptions.CandyException("Usuario bloqueado")
            ElseIf (contraseniaInvalida) Then
                Throw New Exceptions.CandyException("Contraseña invalida")
            End If

            BitacoraBO.getInstance().guardarEvento(usuarioLogueado.id, BitacoraBO.TipoCriticidad.BAJA, "Usuario logueado")
        Catch ex As Exception
            If (TypeOf (ex) Is Exceptions.CandyException) Then
                Throw ex
            Else
                Throw New Exceptions.CandyException("Error inesperado, vuelva a intentar")
            End If
        End Try
    End Sub

End Class

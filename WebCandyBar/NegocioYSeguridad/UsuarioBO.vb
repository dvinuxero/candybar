'Clase encargada de manejar la logica del usuario como loguearse al sistema, ABM de usuarios y validaciones especiales

Public Class UsuarioBO

    Private Shared _instance As UsuarioBO
    'cache de usuarios registrados consultados en la base al iniciar el modulo de usuarios
    Private Shared usuariosRegistrados As Dictionary(Of String, EntidadesDTO.UsuarioDTO)
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

    'obtiene el dto del usuario del cache segun el id especificado
    Public Function obtenerUsuarioPorId(usuarioId As Integer) As EntidadesDTO.UsuarioDTO
        Try
            Return obtenerUsuarios().Item(CStr(usuarioId))
        Catch exception As KeyNotFoundException
            Return Nothing
        End Try
    End Function

    'obtiene los usuarios registrados
    'si el cache no existe los busca en la base de datos, sino devuelve el mapa precargado
    Public Function obtenerUsuarios() As Dictionary(Of String, EntidadesDTO.UsuarioDTO)
        Return obtenerUsuarios(False)
    End Function

    Private Function obtenerUsuarios(forzar As Boolean) As Dictionary(Of String, EntidadesDTO.UsuarioDTO)
        If (usuariosRegistrados Is Nothing Or forzar) Then
            usuariosRegistrados = AccesoADatos.UsuarioDAO.getInstance().obtenerUsuarios()
            For Each usuario In usuariosRegistrados
                usuario.Value.nombre = SeguridadBO.getInstance().desencriptar(usuario.Value.nombre)
            Next
        End If
        Return usuariosRegistrados
    End Function

    Public Function obtenerUsuarioIdLogueado() As Integer
        Dim obj As Object = System.Web.HttpContext.Current.Session("user")
        If (obj IsNot Nothing) Then
            Return CType(obj, EntidadesDTO.UsuarioDTO).id
        End If

        Throw New Exceptions.CandyException("Error usuario no logueado")
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
            '3 actualizo cache
            usuariosRegistrados.Remove(CStr(usuarioNuevo.id))
            usuariosRegistrados.Add(CStr(usuarioNuevo.id), usuarioNuevo)
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

    'modificar un usuario ya existente en la base de datos
    Public Function modificarUsuario(usuarioDTO As EntidadesDTO.UsuarioDTO) As Boolean
        Try
            'If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(usuarioLogueado.id, "P28_USUARIOS_MODIFICAR")) Then
            '    Throw New Exceptions.CandyException("Usuario no tiene permiso para modificar usuarios", True)
            'End If
            '1 validar
            validarParaAgregar(usuarioDTO, False)
            usuarioDTO.nombre = SeguridadBO.getInstance().encriptar(usuarioDTO.nombre, True)
            '2 ejecutar update
            AccesoADatos.UsuarioDAO.getInstance().modificarUsuario(usuarioDTO)
            usuarioDTO.nombre = SeguridadBO.getInstance().desencriptar(usuarioDTO.nombre)
            '3 actualiza la cache, ya que el id del usuario no cambia es facil
            Dim usuarioSinModificaciones As EntidadesDTO.UsuarioDTO = obtenerUsuarioPorId(usuarioDTO.id)
            usuarioSinModificaciones.nickname = usuarioDTO.nickname
            usuarioSinModificaciones.nombre = usuarioDTO.nombre
            usuarioSinModificaciones.apellido = usuarioDTO.apellido
            usuarioSinModificaciones.lang = usuarioDTO.lang
            '4 actualizamos las familias del usuario
            'Dim familiasDelUsuario As List(Of String) = PermisoBO.getInstance().obtenerFamiliasPorUsuario(usuarioDTO.id)
            'PermisoBO.getInstance().asociarFamiliasAlUsuario(usuarioDTO.id, familiasDelUsuario)
            '5 actualizamos las patentes del usuario
            'Dim patentesDelUsuario As List(Of String) = PermisoBO.getInstance().obtenerPatentesPorUsuario(usuarioDTO.id)
            'PermisoBO.getInstance().asociarPatentesAlUsuario(usuarioDTO.id, patentesDelUsuario)
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Usuario modificado id " & usuarioDTO.id)
            Return True
        Catch exception As Exceptions.CandyException
            'se informa un error si es necesario
            If (exception.informarEstado) Then
                Throw exception
            Else
                Return False
            End If
        End Try
    End Function

    'eliminar el usuario especificado
    Public Function eliminarUsuario(usuarioDTO As EntidadesDTO.UsuarioDTO) As Boolean
        Try
            If (esUsuarioAdministrador(usuarioDTO.id)) Then
                Return False
            End If
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(obtenerUsuarioIdLogueado(), "P29_USUARIOS_BAJA")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para eliminar usuarios", True)
            End If
            If (usuarioDTO.id = UsuarioBO.getInstance().obtenerUsuarioIdLogueado()) Then
                Throw New Exceptions.CandyException("No se puede eliminar el usuario con el que esta logueado al sistema", True)
            End If
            '1 eliminar usuario ya se valido previamente q no sea user admin y q tenga permisos
            AccesoADatos.UsuarioDAO.getInstance().eliminarUsuario(usuarioDTO)
            '2 se actualiza la cache
            usuariosRegistrados.Remove(CStr(usuarioDTO.id))
            '3 se eliminan las asociaciones de familia del usuario
            PermisoBO.getInstance().eliminarFamiliasDelUsuario(usuarioDTO.id)
            '3 se eliminan las asociaciones de patentes del usuario
            PermisoBO.getInstance().eliminarPatentesDelUsuario(usuarioDTO.id)
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.ALTA, "Usuario eliminado id " & usuarioDTO.id)
            Return True
        Catch exception As Exceptions.CandyException
            'se informa un error si es necesario
            If (exception.informarEstado) Then
                Throw exception
            Else
                Return False
            End If
        End Try
    End Function

    Public Function cambiarContrasenia(actual As String, contraseniaNueva As String, contraseniaNuevaConfirmada As String) As Boolean
        actual = SeguridadBO.getInstance().encriptar(actual, False)
        If (Not actual.Equals(obtenerUsuarioPorId(obtenerUsuarioIdLogueado()).password)) Then
            Throw New Exceptions.CandyException("Error la constraseña actual no coincide")
        End If

        Dim contraseniaSinEncriptar As String = contraseniaNueva
        If (contraseniaNueva.Equals(contraseniaNuevaConfirmada)) Then
            If (contraseniaNueva.Length <= 20) Then
                contraseniaNueva = SeguridadBO.getInstance().encriptar(contraseniaNueva, False)
                Dim ejecutado As Boolean = AccesoADatos.UsuarioDAO.getInstance().cambiarContrasenia(obtenerUsuarioIdLogueado(), contraseniaNueva)
                If (Not ejecutado) Then
                    BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al cambiar contrasena")
                    Throw New Exceptions.CandyException("Error al modificar la contraseña, vuelva a intentarlo")
                Else
                    obtenerUsuarios().Item(CStr(obtenerUsuarioIdLogueado())).password = contraseniaNueva
                    SeguridadBO.getInstance().informarPasswordAlUsuario(obtenerUsuarioIdLogueado(), contraseniaSinEncriptar)
                    BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Contrasena cambiada para el usuario " & obtenerUsuarioIdLogueado())
                    Return ejecutado
                End If
            Else
                BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al cambiar contrasena")
                Throw New Exceptions.CandyException("El maximo de caracteres de la contraseña es 20")
            End If
        Else
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al cambiar contrasena")
            Throw New Exceptions.CandyException("No coinciden las contraseñas por favor reintentar")
        End If
    End Function

    'nuevo metodo en el analisis
    Public Function reestablecerContraseña(usuarioId As Integer)
        If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(obtenerUsuarioIdLogueado(), "P06_CONTRASENIA_CAMBIAR")) Then
            Throw New Exceptions.CandyException("Usuario no tiene permiso para reestablecer contraseña", True)
        End If

        Dim contraseniaSinEncriptar As String = SeguridadBO.getInstance().autogenerarContrasenia()
        Dim contraseniaNueva As String = SeguridadBO.getInstance().encriptar(contraseniaSinEncriptar, False)
        Dim ejecutado As Boolean = AccesoADatos.UsuarioDAO.getInstance().cambiarContrasenia(usuarioId, contraseniaNueva)
        If (Not ejecutado) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al reestablecer contrasena")
            Throw New Exceptions.CandyException("Error al reestablecer la contraseña, vuelva a intentarlo", True)
        Else
            obtenerUsuarios().Item(CStr(usuarioId)).password = contraseniaNueva
            SeguridadBO.getInstance().informarPasswordAlUsuario(usuarioId, contraseniaSinEncriptar)
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Contrasena reestablecida para el usuario " & usuarioId)
            Return ejecutado
        End If
    End Function

    'metodo se agrega al analisis
    Public Function desbloquearUsuario(usuarioId As Integer) As Boolean
        If (Not NegocioYSeguridad.PermisoBO.getInstance().usuarioTienePermisoParaAccion(obtenerUsuarioIdLogueado(), "P08_USUARIOS_DESBLOQUEAR")) Then
            Throw New Exceptions.CandyException("Usuario no tiene permisos para desbloquear/bloquear")
        End If
        Dim ejecutado As Boolean = AccesoADatos.UsuarioDAO.getInstance().desbloquearUsuario(usuarioId)
        If (ejecutado) Then
            usuariosRegistrados.Item(CStr(usuarioId)).baja = "NO"
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.ALTA, "Usuario " & usuarioId & " desbloqueado")
        End If
        Return ejecutado
    End Function

    'metodo se agrega al analisis
    Public Function bloquearUsuario(usuarioId As Integer) As Boolean
        If (Not NegocioYSeguridad.PermisoBO.getInstance().usuarioTienePermisoParaAccion(obtenerUsuarioIdLogueado(), "P08_USUARIOS_DESBLOQUEAR")) Then
            Throw New Exceptions.CandyException("Usuario no tiene permisos para desbloquear/bloquear")
        End If
        If (esUsuarioAdministrador(usuarioId)) Then
            Throw New Exceptions.CandyException("No se puede bloquear un usuario administrador")
        End If
        Dim ejecutado As Boolean = AccesoADatos.UsuarioDAO.getInstance().bloquearUsuario(usuarioId)
        If (ejecutado) Then
            usuariosRegistrados.Item(CStr(usuarioId)).baja = "SI"
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.ALTA, "Usuario " & usuarioId & " bloqueado")
        End If
        Return ejecutado
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

    Public Sub actualizarCache()
        obtenerUsuarios(True)
    End Sub

    'Metodo especializado en realizar el logueo y chequeos importantes antes de iniciar la entrada al sistema
    'cambio un poco la logica respecto al analisis
    Public Function loguearUsuario(nickname As String, password As String) As EntidadesDTO.UsuarioDTO
        Try
            Dim integridadRespetada As Boolean = True
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

            PermisoBO.getInstance().iniciarPermisos(usuarioLogueado.id)

            System.Web.HttpContext.Current.Session.Add("user", usuarioLogueado)

            integridadRespetada = SeguridadBO.getInstance().chequearIntegridad()

            If (usuarioLogueado.intentosIncorrectos = 3 And "NO".Equals(usuarioLogueado.baja) And Not esUsuarioAdministrador(usuarioLogueado.id)) Then
                'marcar evento en bitacora se bloquea usuario
                BitacoraBO.getInstance().guardarEvento(usuarioLogueado.id, BitacoraBO.TipoCriticidad.ALTA, "Usuario bloqueado")
                usuarioLogueado.baja = "SI"
                If (Not AccesoADatos.UsuarioDAO.getInstance().bloquearUsuario(usuarioLogueado.id)) Then
                    System.Web.HttpContext.Current.Session.Remove("user")
                    Throw New Exceptions.CandyException("Error al intentar bloquear el usuario")
                End If
            End If

            If ("SI".Equals(usuarioLogueado.baja)) Then
                System.Web.HttpContext.Current.Session.Remove("user")
                Throw New Exceptions.CandyException("Usuario bloqueado")
            ElseIf (contraseniaInvalida) Then
                System.Web.HttpContext.Current.Session.Remove("user")
                Throw New Exceptions.CandyException("Contraseña invalida")
            End If

            If (Not integridadRespetada) Then
                Throw New Exceptions.CandyException("Error de [integridad] sobre la base de datos, para mas detalle por favor consulte el reporte de bitacora o al administrador del sistema", True)
            End If

            BitacoraBO.getInstance().guardarEvento(usuarioLogueado.id, BitacoraBO.TipoCriticidad.BAJA, "Usuario logueado")

            Return usuarioLogueado
        Catch ex As Exception
            If (TypeOf (ex) Is Exceptions.CandyException) Then
                Throw ex
            Else
                Throw New Exceptions.CandyException("Error inesperado, vuelva a intentar")
            End If
        End Try
    End Function

End Class

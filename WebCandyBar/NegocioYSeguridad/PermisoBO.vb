'Clase encargada de manejar la logica de permisos del sistema

'Patentes escenciales:
'    -Asociar patentes -> es necesario para asignar patentes nuevas a un usuario
'    -Modificar usuarios -> es necesario para modificar patentes de un usuario, etc
'    -Usuario administrador -> es necesaria para no bloquear al usuario x intentos incorrectos

Public Class PermisoBO

    'se cambia la firma del new, pasa de string a integer porque cacheamos por id del usuario
    Public Shared familiasPorUsuario As Dictionary(Of String, List(Of String))

    'se cambia la firma del new, pasa de string a integer porque cacheamos por id del usuario
    'revisar logica, si es estatico ver si es necesario ir a buscar los permisos al momento de loguear si ya tengo data para ese usuario. De lo contrario blanquear el object en el cache cuando se cambian los permisos, etc
    Public Shared patentesPorUsuario As Dictionary(Of String, List(Of String))

    'se agrega cache de familiasPatentes en el analisis
    Public Shared familiasPatentesDelSistema As Dictionary(Of String, List(Of String))

    Public Shared familiasDelSistema As Dictionary(Of String, String)

    'se agrega cache de patentes en el analisis
    Public Shared patentesDelSistema As Dictionary(Of String, String)

    Public Shared FAMILIA_ID_NUEVA As String = "FAMILIA_NUEVA"
    Public Shared PATENTE_NEGADA_FLAG As String = "::SI::"
    Public Shared ESCENCIA_NEGADA_POR_HERENCIA As String = "::ESCENCIAL_NEGADA_POR_HERENCIA::"
    Public Shared MODULO_LIBRE As String = "LIBRE"
    Public Shared PATENTES_ESCENCIALES As String() = {"P21_USUARIOS_ASOCIARPATENTE", "P28_USUARIOS_MODIFICAR"}

    Public Enum TienePermiso
        HABILITADO
        NEGADO
        INDEFINIDO
    End Enum

    Private Shared _instance As PermisoBO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As PermisoBO
        If (_instance Is Nothing) Then
            _instance = New PermisoBO()
        End If
        Return _instance
    End Function

    Public Function asociarFamiliasAlUsuario(usuarioId As Integer, familias As List(Of String)) As Boolean
        'If (Not usuarioTienePermisoParaAccion(UsuarioBO.getInstance().usuarioLogueado.id, "P26_USUARIOS_ASOCIARFAMILIA")) Then
        '    Throw New Exceptions.CandyException("Usuario no tiene permiso para asociar familias", True)
        'End If
        Dim ejecutado As Boolean = AccesoADatos.PermisoDAO.getInstance().asociarFamiliasAlUsuario(usuarioId, familias)
        cachearFamiliasPorUsuario(usuarioId, familias)
        Return ejecutado
    End Function

    Public Function asociarPatentesAlUsuario(usuarioId As Integer, patentes As List(Of String)) As Boolean
        'If (Not usuarioTienePermisoParaAccion(UsuarioBO.getInstance().usuarioLogueado.id, "P21_USUARIOS_ASOCIARPATENTE")) Then
        '    Throw New Exceptions.CandyException("Usuario no tiene permiso para asociar patentes", True)
        'End If
        Dim ejecutado As Boolean = AccesoADatos.PermisoDAO.getInstance().asociarPatentesAlUsuario(usuarioId, patentes)
        SeguridadBO.getInstance().calcularDVH("usuario_patente")
        SeguridadBO.getInstance().calcularDVV("usuario_patente")
        cachearPatentesPorUsuario(usuarioId, patentes)
        Return ejecutado
    End Function

    Public Function asociarPatentesDeLaFamilia(familiaId As String, patentes As List(Of String)) As Boolean
        Dim ejecutado As Boolean = AccesoADatos.PermisoDAO.getInstance().asociarPatentesDeLaFamilia(familiaId, patentes)
        SeguridadBO.getInstance().calcularDVH("familia_patente")
        SeguridadBO.getInstance().calcularDVV("familia_patente")
        cachearPatentesPorFamilia(familiaId, patentes)
        Return ejecutado
    End Function

    'dejamos que se asignen o borren patentes escenciales como querramos. podemos convertir o desconvertir administradores.
    'pero tener en cuenta que al momento de asignar patentes, ver que ese usuario si era admin, y en su lista de patentes no
    'existen todas las escenciales quiere decir que va a perder su nomina de administrador, por lo tanto chequeamos sobre
    'todos los usuarios registrados si existe uno o mas de un admin con patentes escenciales asignadas sin problemas.
    Private Function sobreviveAlMenosUnAdministrador() As Boolean
        Dim tieneTodasLasEscenciales As Boolean = False
        Dim seMantieneUnAdmin As Boolean = False

        For Each usuario In UsuarioBO.getInstance().obtenerUsuarios()
            Dim patentes As List(Of String) = obtenerPatentesPorUsuario(usuario.Value.id)
            'tiene todas las patentes escenciales y no esta dado de baja
            tieneTodasLasEscenciales = tieneTodasLasPatentesEscenciales(patentes) And EntidadesDTO.UsuarioDTO.BAJA_FLAG.Equals(usuario.Value.baja)
            If (tieneTodasLasEscenciales) Then
                seMantieneUnAdmin = True
                Exit For
            ElseIf (Not patentes.Contains(ESCENCIA_NEGADA_POR_HERENCIA)) Then
                'busco por familia
                For Each familia In obtenerFamiliasPorUsuario(usuario.Value.id)
                    Dim patentesDeLaFamilia As List(Of String) = obtenerPatentesPorFamilia(familia)
                    'tiene todas las patentes escenciales y no esta dado de baja
                    'agrego al final todas las patentes que se sobrecargan porque puede ser administrador con mixto(osea patentes sobrecargadas + patentes que tenga a nivel familia)
                    patentesDeLaFamilia.AddRange(patentes)
                    tieneTodasLasEscenciales = tieneTodasLasPatentesEscenciales(patentesDeLaFamilia) And EntidadesDTO.UsuarioDTO.BAJA_FLAG.Equals(usuario.Value.baja)
                    If (tieneTodasLasEscenciales) Then
                        seMantieneUnAdmin = True
                        Exit For
                    End If
                Next
                If (seMantieneUnAdmin) Then
                    Exit For
                End If
            Else
                patentes.Remove(ESCENCIA_NEGADA_POR_HERENCIA)
            End If
        Next
        If (Not seMantieneUnAdmin) Then
            Throw New Exceptions.CandyException("Error es probable que el usuario administrador pierda sus patentes escenciales si se aplican los cambios. Por favor revise nuevamente.", True)
        End If

        Return seMantieneUnAdmin
    End Function

    Public Sub cachearPatentesPorUsuario(usuarioId As Integer, patentesPorUsuarioList As List(Of String))
        If (patentesPorUsuario Is Nothing) Then
            patentesPorUsuario = New Dictionary(Of String, List(Of String))
        End If

        Try
            Dim ultimasPatentesModificadas As List(Of String) = patentesPorUsuario.Item(CStr(usuarioId))
            Try
                patentesPorUsuario.Remove(CStr(usuarioId))
                patentesPorUsuario.Add(CStr(usuarioId), patentesPorUsuarioList)
                sobreviveAlMenosUnAdministrador()
            Catch exception As Exceptions.CandyException
                patentesPorUsuario.Remove(CStr(usuarioId))
                patentesPorUsuario.Add(CStr(usuarioId), ultimasPatentesModificadas)
                Throw exception
            End Try
        Catch exception As KeyNotFoundException
        End Try
    End Sub

    Public Sub cachearPatentesPorFamiliaNueva(patentes As List(Of String))
        If (familiasPatentesDelSistema Is Nothing) Then
            familiasPatentesDelSistema = New Dictionary(Of String, List(Of String))
        End If
        familiasPatentesDelSistema.Remove(FAMILIA_ID_NUEVA)
        familiasPatentesDelSistema.Add(FAMILIA_ID_NUEVA, patentes)
    End Sub

    Public Sub descachearPatentesPorFamiliaNueva()
        If (familiasPatentesDelSistema Is Nothing) Then
            Return
        End If
        familiasPatentesDelSistema.Remove(FAMILIA_ID_NUEVA)
    End Sub

    Public Sub cachearPatentesPorFamilia(familiaId As String, patentes As List(Of String))
        If (familiasPatentesDelSistema Is Nothing) Then
            familiasPatentesDelSistema = New Dictionary(Of String, List(Of String))
        End If
        Try
            Dim patentesDeLaUltimaFamiliaModificada As List(Of String) = Nothing
            Try
                patentesDeLaUltimaFamiliaModificada = familiasPatentesDelSistema.Item(familiaId)
            Catch exceptionKN As KeyNotFoundException
                patentesDeLaUltimaFamiliaModificada = New List(Of String)
            End Try
            Try
                familiasPatentesDelSistema.Remove(familiaId)
                familiasPatentesDelSistema.Add(familiaId, patentes)
                sobreviveAlMenosUnAdministrador()
            Catch exception As Exceptions.CandyException
                familiasPatentesDelSistema.Remove(familiaId)
                familiasPatentesDelSistema.Add(familiaId, patentesDeLaUltimaFamiliaModificada)
                Throw exception
            End Try
        Catch exception As KeyNotFoundException
        End Try
    End Sub

    Public Sub cachearFamiliasPorUsuario(usuarioId As Integer, familiasPorUsuarioList As List(Of String))
        If (familiasPorUsuario Is Nothing) Then
            familiasPorUsuario = New Dictionary(Of String, List(Of String))
        End If

        Try
            Dim ultimasFamiliasModificadasDelUsuario As List(Of String) = familiasPorUsuario.Item(CStr(usuarioId))
            Try
                familiasPorUsuario.Remove(CStr(usuarioId))
                familiasPorUsuario.Add(CStr(usuarioId), familiasPorUsuarioList)
                sobreviveAlMenosUnAdministrador()
            Catch exception As Exceptions.CandyException
                familiasPorUsuario.Remove(CStr(usuarioId))
                familiasPorUsuario.Add(CStr(usuarioId), ultimasFamiliasModificadasDelUsuario)
                Throw exception
            End Try
        Catch exception As KeyNotFoundException
        End Try
    End Sub

    Public Sub cachearFamiliasPorUsuarioNuevo(familiasPorUsuarioList As List(Of String))
        If (familiasPorUsuario Is Nothing) Then
            familiasPorUsuario = New Dictionary(Of String, List(Of String))
        End If
        familiasPorUsuario.Remove(CStr(UsuarioBO.USUARIO_ID_NUEVO))
        familiasPorUsuario.Add(CStr(UsuarioBO.USUARIO_ID_NUEVO), familiasPorUsuarioList)
    End Sub

    'cambia firma, sacamos parametro moduloId y cambia el nombre del metodo
    'patente: IDNUMERICO_SECCION_ACCION, ej: P02_USUARIOS_ELIMINAR
    Public Function obtenerModulosHabilitadoDelUsuario(usuarioId As Integer) As List(Of String)
        Dim modulosHabilitados As New List(Of String)
        obtenerFamilias(True)

        'secciones del usuario x las patentes de las familias asociadas
        For Each familia In familiasPorUsuario.Item(CStr(usuarioId))
            For Each patente In familiasPatentesDelSistema.Item(familia)
                agregarSeccionALosModulosHabilitados(modulosHabilitados, patente)
            Next
        Next

        'secciones del usuario x las patentes habilitadas
        For Each patente In patentesPorUsuario.Item(CStr(usuarioId))
            agregarSeccionALosModulosHabilitados(modulosHabilitados, patente)
        Next

        Return modulosHabilitados
    End Function

    Private Sub agregarSeccionALosModulosHabilitados(ByRef modulosHabilitados As List(Of String), patente As String)
        Dim desde As Integer = patente.IndexOf("_") + 1
        Dim hasta As Integer = patente.LastIndexOf("_")
        Dim seccion As String = patente.Substring(desde, hasta - desde)
        If (Not modulosHabilitados.Contains(seccion)) Then
            If (Not patente.Contains(PATENTE_NEGADA_FLAG)) Then
                modulosHabilitados.Add(seccion)
            End If
        ElseIf (patente.Contains(PATENTE_NEGADA_FLAG)) Then
            modulosHabilitados.Remove(seccion)
        End If
    End Sub

    'cambia la logica
    Public Sub iniciarPermisos(usuarioId As Integer)
        obtenerFamiliasPorUsuario(usuarioId)
        obtenerPatentesPorUsuario(usuarioId)
    End Sub

    Public Function obtenerFamilias() As Dictionary(Of String, String)
        Return obtenerFamilias(False)
    End Function

    Private Function obtenerFamilias(forzar As Boolean) As Dictionary(Of String, String)
        If (familiasDelSistema Is Nothing Or forzar) Then
            Dim familiasAux As Dictionary(Of String, String) = AccesoADatos.PermisoDAO.getInstance().obtenerFamilias()
            familiasDelSistema = New Dictionary(Of String, String)
            familiasPatentesDelSistema = New Dictionary(Of String, List(Of String))
            If (Not (familiasAux Is Nothing)) Then
                For Each familia In familiasAux
                    Dim familiaDesencriptada = SeguridadBO.getInstance().desencriptar(familia.Key)
                    familiasDelSistema.Add(familiaDesencriptada, familia.Value)
                    Dim patentesDeLaFamilia As List(Of String) = AccesoADatos.PermisoDAO.getInstance().obtenerPatentesPorFamilia(familiaDesencriptada)
                    familiasPatentesDelSistema.Add(familiaDesencriptada, patentesDeLaFamilia)
                Next
            End If
        End If
        Return familiasDelSistema
    End Function

    Public Function obtenerFamiliasPorUsuario(usuarioId As Integer) As List(Of String)
        Return obtenerFamiliasPorUsuario(usuarioId, False)
    End Function

    Private Function obtenerFamiliasPorUsuario(usuarioId As Integer, forzar As Boolean) As List(Of String)
        Try
            If (familiasPorUsuario Is Nothing Or forzar) Then
                familiasPorUsuario = New Dictionary(Of String, List(Of String))
            End If
            Return familiasPorUsuario.Item(CStr(usuarioId))
        Catch exception As KeyNotFoundException
            Dim familiasPorUsuarioList As List(Of String) = AccesoADatos.PermisoDAO.getInstance().obtenerFamiliasPorUsuario(usuarioId)
            familiasPorUsuario.Remove(CStr(usuarioId))
            familiasPorUsuario.Add(CStr(usuarioId), familiasPorUsuarioList)
            Try
                Return familiasPorUsuario.Item(CStr(usuarioId))
            Catch exception1 As KeyNotFoundException
                Return Nothing
            End Try
        End Try
    End Function

    Public Function obtenerPatentesPorFamilia(familiaId As String) As List(Of String)
        Return obtenerPatentesPorFamilia(familiaId, False)
    End Function

    Private Function obtenerPatentesPorFamilia(familiaId As String, forzar As Boolean) As List(Of String)
        Try
            If (familiasPatentesDelSistema Is Nothing Or forzar) Then
                familiasPatentesDelSistema = New Dictionary(Of String, List(Of String))
            End If
            Return familiasPatentesDelSistema.Item(CStr(familiaId))
        Catch exception As KeyNotFoundException
            Dim patentesDeLaFamiliaList As List(Of String) = AccesoADatos.PermisoDAO.getInstance().obtenerPatentesPorFamilia(familiaId)
            familiasPatentesDelSistema.Remove(CStr(familiaId))
            familiasPatentesDelSistema.Add(CStr(familiaId), patentesDeLaFamiliaList)
            Try
                Return familiasPatentesDelSistema.Item(CStr(familiaId))
            Catch exception1 As KeyNotFoundException
                Return Nothing
            End Try
        End Try
    End Function

    Public Function obtenerPatentes() As Dictionary(Of String, String)
        Return obtenerPatentes(False)
    End Function

    Private Function obtenerPatentes(forzar As Boolean) As Dictionary(Of String, String)
        If (patentesDelSistema Is Nothing Or forzar) Then
            patentesDelSistema = AccesoADatos.PermisoDAO.getInstance().obtenerPatentes()
        End If
        Return patentesDelSistema
    End Function

    Public Function obtenerPatentesPorUsuario(usuarioId As Integer) As List(Of String)
        Return obtenerPatentesPorUsuario(usuarioId, False)
    End Function

    Private Function obtenerPatentesPorUsuario(usuarioId As Integer, forzar As Boolean) As List(Of String)
        If (patentesPorUsuario Is Nothing Or forzar) Then
            patentesPorUsuario = New Dictionary(Of String, List(Of String))
        End If
        Try
            Return patentesPorUsuario.Item(CStr(usuarioId))
        Catch exception As KeyNotFoundException
            Dim patentesPorUsuarioList As List(Of String) = AccesoADatos.PermisoDAO.getInstance().obtenerPatentesPorUsuario(usuarioId)
            patentesPorUsuario.Remove(CStr(usuarioId))
            'casos negados, ejemplo patente_id:SI
            patentesPorUsuario.Add(CStr(usuarioId), patentesPorUsuarioList)
            Try
                Return patentesPorUsuario.Item(CStr(usuarioId))
            Catch ex As KeyNotFoundException
                Return Nothing
            End Try
        End Try
    End Function

    'cambia firma sacamos moduloId
    Public Function verificarPermisosPorFamilia(usuarioId As Integer, accionId As String) As Boolean
        Dim encontre As Boolean = False
        obtenerFamilias()

        For Each familia In familiasPorUsuario.Item(usuarioId)
            Try
                For Each patente In familiasPatentesDelSistema.Item(familia)
                    If (patente.Contains(accionId)) Then
                        encontre = True
                    End If
                    If (encontre) Then
                        Exit For
                    End If
                Next
                If (encontre) Then
                    Exit For
                End If
            Catch exception As KeyNotFoundException
            End Try
        Next
        Return encontre
    End Function

    Public Function verificarPermisosPorUsuario(usuarioId As Integer, accionId As String) As TienePermiso
        Dim patentes As List(Of String) = patentesPorUsuario(CStr(usuarioId))
        Dim tienePermiso As TienePermiso = PermisoBO.TienePermiso.INDEFINIDO
        For Each patente In patentes
            If (patente.Contains(accionId)) Then
                If (patente.Contains(PATENTE_NEGADA_FLAG)) Then
                    'esta negada, no tiene permiso
                    tienePermiso = PermisoBO.TienePermiso.NEGADO
                Else
                    'tiene permiso
                    tienePermiso = PermisoBO.TienePermiso.HABILITADO
                End If
                'salgo, ya encontre lo que queria
                Exit For
            End If
        Next
        Return tienePermiso
    End Function

    'cambia firma sacamos moduloId
    'cambia nombre del metodo antes verificarPermisoAccionPorUsuario
    Public Function usuarioTienePermisoParaAccion(usuarioId As Integer, accionId As String) As Boolean
        Dim permisoVerificadoPorUsuario As TienePermiso = verificarPermisosPorUsuario(usuarioId, accionId)
        If (permisoVerificadoPorUsuario = TienePermiso.NEGADO) Then
            'patente negada entonces no seguimos, el usuario no tiene permiso para esa determinada accion
            Return False
        ElseIf (permisoVerificadoPorUsuario = TienePermiso.HABILITADO) Then
            'patente del usuario, entonces tiene permiso para esa determinada accion, no busco en sus familias
            Return True
        Else
            'permiso TienePermiso.INDEFINIDO entonces busco x las familias del usuario
            Return verificarPermisosPorFamilia(usuarioId, accionId)
        End If
    End Function

    Public Function agregarFamilia(familiaId As String, familiaDescripcion As String) As Boolean
        Try
            'If (Not usuarioTienePermisoParaAccion(UsuarioBO.getInstance().usuarioLogueado.id, "P23_FAMILIAS_ALTA")) Then
            '    Throw New Exceptions.CandyException("Usuario no tiene permiso para agregar familia", True)
            'End If
            validarFamilia(familiaId, familiaDescripcion, True)
            AccesoADatos.PermisoDAO.getInstance.agregarFamilia(SeguridadBO.getInstance().encriptar(familiaId, True), familiaDescripcion)
            familiasDelSistema.Remove(familiaId)
            familiasDelSistema.Add(familiaId, familiaDescripcion)
            Dim patentesDeLaFamilia As List(Of String) = obtenerPatentesPorFamilia(FAMILIA_ID_NUEVA)
            asociarPatentesDeLaFamilia(familiaId, patentesDeLaFamilia)
            cachearPatentesPorFamilia(familiaId, patentesDeLaFamilia)
            descachearPatentesPorFamiliaNueva()
            Return True
        Catch exception As Exceptions.CandyException
            If (exception.informarEstado) Then
                Throw exception
            Else
                Return False
            End If
        End Try
    End Function

    Public Function modificarFamilia(familiaIdAnterior As String, familiaId As String, familiaDescripcion As String) As Boolean
        Try
            'If (Not usuarioTienePermisoParaAccion(UsuarioBO.getInstance().usuarioLogueado.id, "P25_FAMILIAS_MODIFICAR")) Then
            '    Throw New Exceptions.CandyException("Usuario no tiene permiso para modificar familia", True)
            'End If
            If (familiaIdAnterior.Equals(familiaId)) Then
                validarFamilia(familiaId, familiaDescripcion, False)
                AccesoADatos.PermisoDAO.getInstance.modificarFamilia(SeguridadBO.getInstance().encriptar(familiaId, True), familiaDescripcion)
                familiasDelSistema.Remove(familiaId)
                familiasDelSistema.Add(familiaId, familiaDescripcion)
                Dim patentesDeLaFamilia As List(Of String) = obtenerPatentesPorFamilia(familiaId)
                asociarPatentesDeLaFamilia(familiaId, patentesDeLaFamilia)
                descachearPatentesPorFamiliaNueva()
            Else
                validarFamilia(familiaId, familiaDescripcion, True)
                'se elimina la familia anterior, xq ha cambiado su nombre
                AccesoADatos.PermisoDAO.getInstance.eliminarFamilia(SeguridadBO.getInstance().encriptar(familiaIdAnterior, True))
                familiasDelSistema.Remove(familiaIdAnterior)
                'nuevos cambios sobre la familia
                AccesoADatos.PermisoDAO.getInstance.agregarFamilia(SeguridadBO.getInstance().encriptar(familiaId, True), familiaDescripcion)
                familiasDelSistema.Add(familiaId, familiaDescripcion)
                'cambios sobre las patentes de la familia
                AccesoADatos.PermisoDAO.getInstance.eliminarPatentesDeLaFamilia(familiaIdAnterior)
                familiasPatentesDelSistema.Remove(familiaIdAnterior)
                Dim patentesDeLaFamilia As List(Of String) = obtenerPatentesPorFamilia(familiaId)
                asociarPatentesDeLaFamilia(familiaId, patentesDeLaFamilia)
                descachearPatentesPorFamiliaNueva()
                'cambios sobre las familias del usuarios
                AccesoADatos.PermisoDAO.getInstance.actualizarFamiliasDelUsuario(familiaIdAnterior, familiaId)
                For Each familiasDelUsuario In familiasPorUsuario
                    If (familiasDelUsuario.Value.Remove(familiaIdAnterior)) Then
                        familiasDelUsuario.Value.Add(familiaId)
                    End If
                Next
            End If
            Return True
        Catch exception As Exceptions.CandyException
            If (exception.informarEstado) Then
                Throw exception
            Else
                Return False
            End If
        End Try
    End Function

    Public Function eliminarFamilia(familiaId As String)
        If (Not usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P24_FAMILIAS_BAJA")) Then
            Throw New Exceptions.CandyException("Usuario no tiene permiso para eliminar usuarios", True)
        End If

        Dim usuariosAfectados As New List(Of String)
        Try
            For Each familias In familiasPorUsuario
                If (familias.Value.Remove(familiaId)) Then
                    usuariosAfectados.Add(familias.Key)
                End If
            Next
            sobreviveAlMenosUnAdministrador()
        Catch exception As Exceptions.CandyException
            For Each familias In familiasPorUsuario
                If (usuariosAfectados.Contains(familias.Key)) Then
                    familias.Value.Add(familiaId)
                End If
            Next
            Throw exception
        End Try

        AccesoADatos.PermisoDAO.getInstance.eliminarFamilia(SeguridadBO.getInstance().encriptar(familiaId, True))
        AccesoADatos.PermisoDAO.getInstance().eliminarPatentesDeLaFamilia(familiaId)
        AccesoADatos.PermisoDAO.getInstance().eliminarFamiliasDelUsuario(familiaId)
        SeguridadBO.getInstance().calcularDVV("familia_patente")
        familiasDelSistema.Remove(familiaId)
        familiasPatentesDelSistema.Remove(familiaId)
        Return True
    End Function

    Public Function eliminarFamiliasDelUsuario(usuarioId As Integer) As Boolean
        Try
            AccesoADatos.PermisoDAO.getInstance().eliminarFamiliasDelUsuario(usuarioId)
            familiasPorUsuario.Remove(CStr(usuarioId))
            Return True
        Catch exception As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function eliminarPatentesDelUsuario(usuarioId As Integer) As Boolean
        Try
            AccesoADatos.PermisoDAO.getInstance().eliminarPatentesDelUsuario(usuarioId)
            SeguridadBO.getInstance().calcularDVV("usuario_patente")
            patentesPorUsuario.Remove(CStr(usuarioId))
            Return True
        Catch exception As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function validarFamilia(familiaId As String, familiaDescripcion As String, esNuevo As Boolean) As Boolean

        If ("".Equals(familiaId)) Then
            Throw New Exceptions.CandyException("Error el id de familia no puede ser vacio", True)
        End If
        If (familiaId.Length > 30 Or familiaId.Length < 2) Then
            Throw New Exceptions.CandyException("Error el id de familia debe ser entre 2 y 30 caracteres", True)
        End If
        'la familia si estamos agregando una nueva no debe existir en la base
        If (esNuevo) Then
            For Each familia In familiasDelSistema
                If (familiaId.Equals(familia.Key)) Then
                    Throw New Exceptions.CandyException("Error el id de familia ya existe en la base de datos", True)
                End If
            Next
        End If

        If ("".Equals(familiaDescripcion)) Then
            Throw New Exceptions.CandyException("Error la descripcion de la familia no puede ser vacia", True)
        End If
        If (familiaDescripcion.Length > 40 Or familiaDescripcion.Length < 2) Then
            Throw New Exceptions.CandyException("Error la descripcion de la familia debeser entre 2 y 40 caracteres", True)
        End If

        Return True
    End Function

    Public Function esPatenteEscencial(patente As String) As Boolean
        Return PATENTES_ESCENCIALES.Contains(patente)
    End Function

    Public Overloads Function tieneTodasLasPatentesEscenciales(patentes As List(Of String)) As Boolean
        Dim tieneTodasLasEscenciales = True
        For Each patenteEscencial In PATENTES_ESCENCIALES
            'P01_ADMIN::SI:: <> P07_ADMINI esto me indicaria que si viene el flag de negado el contains me devuelve false y no cuenta
            If (Not patentes.Contains(patenteEscencial)) Then
                'si es una negada, entonces aca aborto el proceso ya que el usuario por herencia me esta pisando las patentes negandolas.
                If (patentes.Contains(patenteEscencial & PATENTE_NEGADA_FLAG)) Then
                    patentes.Add(ESCENCIA_NEGADA_POR_HERENCIA)
                End If
                tieneTodasLasEscenciales = False
            End If
        Next
        Return tieneTodasLasEscenciales
    End Function

    Public Overloads Function tieneTodasLasPatentesEscenciales(usuarioId As Integer) As Boolean
        Dim tieneTodasLasEscenciales As Boolean = False
        Dim patentesDelUsuario As List(Of String) = obtenerPatentesPorUsuario(usuarioId)

        If (tieneTodasLasPatentesEscenciales(patentesDelUsuario)) Then
            tieneTodasLasEscenciales = True
        End If
        patentesDelUsuario.Remove(ESCENCIA_NEGADA_POR_HERENCIA)

        If (Not tieneTodasLasEscenciales) Then
            'busco por familia
            For Each familiaDelUsuario In obtenerFamiliasPorUsuario(usuarioId)
                Dim patentesDeLaFamiliaDelUsuario As List(Of String) = obtenerPatentesPorFamilia(familiaDelUsuario)
                'tiene todas las patentes escenciales y no esta dado de baja
                'agrego al final todas las patentes que se sobrecargan porque puede ser administrador con mixto(osea patentes sobrecargadas + patentes que tenga a nivel familia)
                patentesDeLaFamiliaDelUsuario.AddRange(patentesDelUsuario)
                tieneTodasLasEscenciales = tieneTodasLasPatentesEscenciales(patentesDeLaFamiliaDelUsuario)
                patentesDelUsuario.Remove(ESCENCIA_NEGADA_POR_HERENCIA)
                If (tieneTodasLasEscenciales) Then
                    Exit For
                End If
            Next
        End If

        Return tieneTodasLasEscenciales
    End Function

    Public Sub actualizarCache()
        obtenerFamiliasPorUsuario(NegocioYSeguridad.UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), True)
        obtenerPatentesPorUsuario(NegocioYSeguridad.UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), True)
        obtenerFamilias(True)
        obtenerPatentes(True)
        For Each familia In familiasDelSistema
            obtenerPatentesPorFamilia(familia.Key, True)
        Next
    End Sub

End Class

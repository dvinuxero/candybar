Public Class InsumoBO

    'cache de insumos del sistema consultados en la base al iniciar el modulo de insumos
    Private Shared insumosDelSistema As Dictionary(Of String, EntidadesDTO.InsumoDTO)

    Private Shared _instance As InsumoBO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As InsumoBO
        If (_instance Is Nothing) Then
            _instance = New InsumoBO()
        End If
        Return _instance
    End Function

    Public Function actualizarInsumo(insumoDTO As EntidadesDTO.InsumoDTO) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P10_INSUMOS_MODIFICAR")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para modificar insumos", True)
            End If
            '1 valida
            validarParaAgregar(insumoDTO)
            insumoDTO.nombre = SeguridadBO.getInstance().encriptar(insumoDTO.nombre, True)
            insumoDTO.precioUnidad = SeguridadBO.getInstance().encriptar(insumoDTO.precioUnidad, True)
            insumoDTO.stock = SeguridadBO.getInstance().encriptar(insumoDTO.stock, True)
            '2 insert en la base
            AccesoADatos.InsumoDAO.getInstance().actualizarInsumo(insumoDTO)
            insumoDTO.nombre = SeguridadBO.getInstance().desencriptar(insumoDTO.nombre)
            insumoDTO.precioUnidad = SeguridadBO.getInstance().desencriptar(insumoDTO.precioUnidad)
            insumoDTO.stock = SeguridadBO.getInstance().desencriptar(insumoDTO.stock)
            '3 actualizo cache
            insumosDelSistema.Remove(CStr(insumoDTO.id))
            insumosDelSistema.Add(CStr(insumoDTO.id), insumoDTO)
            '4 se recalculan los digitos verificadores
            SeguridadBO.getInstance().calcularDVH("insumo")
            SeguridadBO.getInstance().calcularDVV("insumo")
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Insumo " & insumoDTO.id & " actualizado")
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

    'cambiar firma saco metodo no sirve en este caso
    Public Function agregarInsumo(insumoNuevo As EntidadesDTO.InsumoDTO) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P11_INSUMOS_ALTA")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para eliminar insumos", True)
            End If
            '1 valida
            validarParaAgregar(insumoNuevo)
            insumoNuevo.id = AccesoADatos.InsumoDAO.getInstance().obtenerSiguienteID()
            insumoNuevo.nombre = SeguridadBO.getInstance().encriptar(insumoNuevo.nombre, True)
            insumoNuevo.precioUnidad = SeguridadBO.getInstance().encriptar(insumoNuevo.precioUnidad, True)
            insumoNuevo.stock = SeguridadBO.getInstance().encriptar(insumoNuevo.stock, True)
            '2 insert en la base
            AccesoADatos.InsumoDAO.getInstance().agregarInsumo(insumoNuevo)
            insumoNuevo.nombre = SeguridadBO.getInstance().desencriptar(insumoNuevo.nombre)
            insumoNuevo.precioUnidad = SeguridadBO.getInstance().desencriptar(insumoNuevo.precioUnidad)
            insumoNuevo.stock = SeguridadBO.getInstance().desencriptar(insumoNuevo.stock)
            '3 actualizo cache
            insumosDelSistema.Remove(CStr(insumoNuevo.id))
            insumosDelSistema.Add(CStr(insumoNuevo.id), insumoNuevo)
            '4 se recalculan los digitos verificadores
            SeguridadBO.getInstance().calcularDVH("insumo")
            SeguridadBO.getInstance().calcularDVV("insumo")
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Insumo " & insumoNuevo.id & " agregadp")
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

    Public Function eliminarInsumo(insumoDTO As EntidadesDTO.InsumoDTO) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P09_INSUMOS_BAJA")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para eliminar insumos", True)
            End If
            '1 eliminar combo ya se valido previamente q no sea user admin y q tenga permisos
            AccesoADatos.InsumoDAO.getInstance().eliminarInsumo(insumoDTO)
            '2 se actualiza la cache
            insumosDelSistema.Remove(CStr(insumoDTO.id))
            '3 se eliminan las asociaciones de insumos sobre combos, etc
            AccesoADatos.ComboDAO.getInstance().eliminarInsumosAsociadosAUnCombo(insumoDTO.id)

            If (ComboBO.insumosPorCombo IsNot Nothing) Then
                For Each combo In ComboBO.insumosPorCombo
                    Dim index As Integer = 0
                    Dim borrarIndex As Integer = -1
                    For index = 0 To combo.Value.Count - 1
                        Dim insumoId As Integer = combo.Value(index).Split(";")(0)
                        If (insumoId = insumoDTO.id) Then
                            borrarIndex = index
                        End If
                    Next
                    If (borrarIndex > -1) Then
                        combo.Value.RemoveAt(borrarIndex)
                    End If
                Next
            End If
            '4 se recalculan los digitos verificadores
            SeguridadBO.getInstance().calcularDVH("insumo")
            SeguridadBO.getInstance().calcularDVV("insumo")
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Insumo " & insumoDTO.id & " eliminado")
            Return True
        Catch exception As Exceptions.CandyException
            Return False
        End Try
    End Function

    'metodo cambia de firma le pasamos el insumoDTO y la respuesta es un throw exception para poder tener un mensaje
    Public Function validarParaAgregar(insumoDTO As EntidadesDTO.InsumoDTO) As Boolean

        'validacion de nombre
        If ("".Equals(insumoDTO.nombre)) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
            Throw New Exceptions.CandyException("Error el nombre no puede ser vacio", True)
        End If
        If (insumoDTO.nombre.Length > 40 Or insumoDTO.nombre.Length < 2) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
            Throw New Exceptions.CandyException("Error el nombre debe ser entre 2 y 40 caracteres", True)
        End If

        'validacion de tipo
        If ("".Equals(insumoDTO.tipo)) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
            Throw New Exceptions.CandyException("Error el tipo no puede ser vacio", True)
        End If
        If (insumoDTO.tipo.Length > 40 Or insumoDTO.tipo.Length < 2) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
            Throw New Exceptions.CandyException("Error el tipo debe ser entre 2 y 40 caracteres", True)
        End If

        'validacion de precio unitario
        If ("".Equals(insumoDTO.precioUnidad)) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
            Throw New Exceptions.CandyException("Error el precio unitario no puede ser vacio", True)
        End If
        Try
            Dim numero As Decimal = CDec(insumoDTO.precioUnidad)
            If (numero < 0) Then
                BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
                Throw New Exceptions.CandyException("Error el precio unitario no puede ser negativo", True)
            End If
            If (numero > 1000000) Then
                BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
                Throw New Exceptions.CandyException("Error el precio unitario no puede superar 1000000", True)
            End If
        Catch ex As Exception
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
            Throw New Exceptions.CandyException("Error el precio unitario tiene un formato incorrecto", True)
        End Try

        'validacion de stock
        If ("".Equals(insumoDTO.stock)) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
            Throw New Exceptions.CandyException("Error el stock no puede ser vacio", True)
        End If
        Try
            Dim numero As Integer = CInt(insumoDTO.stock)
            If (numero < 0) Then
                BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
                Throw New Exceptions.CandyException("Error el stock no puede ser negativo", True)
            End If
            If (numero > 1000) Then
                BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
                Throw New Exceptions.CandyException("Error el stock no puede superar las 1000 unidades", True)
            End If
        Catch ex As Exception
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el insumo")
            Throw New Exceptions.CandyException("Error el stock tiene un formato incorrecto", True)
        End Try

        Dim existeUnoIgual As Boolean = False
        For Each insumo In obtenerInsumos()
            If (insumo.Value.id <> insumoDTO.id) Then
                If (insumo.Value.nombre.ToLower().Equals(insumoDTO.nombre.ToLower())) Then
                    existeUnoIgual = True
                    Exit For
                End If
            End If
        Next
        If (existeUnoIgual) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error el insumo ya existe")
            Throw New Exceptions.CandyException("Error ya existe un insumo con ese nombre", True)
        End If

        Return True
    End Function

    'cambia firma se devuelve un diccionario en vez de una lista
    Public Function obtenerInsumos() As Dictionary(Of String, EntidadesDTO.InsumoDTO)
        Return obtenerInsumos(False)
    End Function

    Private Function obtenerInsumos(forzar As Boolean) As Dictionary(Of String, EntidadesDTO.InsumoDTO)
        If (insumosDelSistema Is Nothing Or forzar) Then
            insumosDelSistema = AccesoADatos.InsumoDAO.getInstance().obtenerInsumos()
            For Each insumo In insumosDelSistema
                insumo.Value.nombre = SeguridadBO.getInstance().desencriptar(insumo.Value.nombre)
                insumo.Value.precioUnidad = SeguridadBO.getInstance().desencriptar(insumo.Value.precioUnidad)
                insumo.Value.stock = SeguridadBO.getInstance().desencriptar(insumo.Value.stock)
            Next
        End If
        Return insumosDelSistema
    End Function

    'obtiene el dto del insumo del cache segun el id especificado
    Public Function obtenerInsumoPorId(insumoId As Integer) As EntidadesDTO.InsumoDTO
        Try
            Return obtenerInsumos().Item(CStr(insumoId))
        Catch exception As KeyNotFoundException
            Return Nothing
        End Try
    End Function

    Public Function actualizarStock(insumoId As Integer, stockDelta As Integer) As Boolean
        Try
            '1 actualizo stock del insumo
            Dim insumoDTO As EntidadesDTO.InsumoDTO = obtenerInsumoPorId(insumoId)
            Dim stockActual As Integer = CInt(insumoDTO.stock)
            stockActual += stockDelta
            insumoDTO.stock = CStr(stockActual)
            insumoDTO.stock = SeguridadBO.getInstance().encriptar(insumoDTO.stock, True)
            AccesoADatos.InsumoDAO.getInstance().actualizarStock(insumoId, insumoDTO.stock)
            '2 se actualiza la cache
            insumoDTO.stock = SeguridadBO.getInstance().desencriptar(insumoDTO.stock)
            insumosDelSistema.Remove(CStr(insumoDTO.id))
            insumosDelSistema.Add(CStr(insumoDTO.id), insumoDTO)
            '4 se recalculan los digitos verificadores
            SeguridadBO.getInstance().calcularDVH("insumo")
            SeguridadBO.getInstance().calcularDVV("insumo")
            Return True
        Catch exception As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Sub actualizarCache()
        obtenerInsumos(True)
    End Sub

End Class

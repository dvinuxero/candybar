Public Class ComboBO

    'cache de combos del sistema consultados en la base al iniciar el modulo de combos
    Private Shared combosDelSistema As Dictionary(Of String, EntidadesDTO.ComboDTO)

    'id de combo nuevo para poder asignarle insumos en memoria antes de guardarlo en base de datos
    Public Shared COMBO_ID_NUEVO As Integer = 0

    'cache de insumos por combo, guardo como key al comboId y el value a la lista de insumosIds
    Public Shared insumosPorCombo As Dictionary(Of String, List(Of String))

    Private Shared _instance As ComboBO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As ComboBO
        If (_instance Is Nothing) Then
            _instance = New ComboBO()
        End If
        Return _instance
    End Function

    Public Function actualizarCombo(comboDTO As EntidadesDTO.ComboDTO) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P19_COMBOS_MODIFICAR")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para modificar combos", True)
            End If
            '1 valida
            validarParaAgregar(comboDTO, False)
            comboDTO.nombre = SeguridadBO.getInstance().encriptar(comboDTO.nombre, True)
            comboDTO.precio = SeguridadBO.getInstance().encriptar(comboDTO.precio, True)
            '2 insert en la base
            AccesoADatos.ComboDAO.getInstance().actualizarCombo(comboDTO)
            comboDTO.nombre = SeguridadBO.getInstance().desencriptar(comboDTO.nombre)
            comboDTO.precio = SeguridadBO.getInstance().desencriptar(comboDTO.precio)
            '3 actualizo cache
            combosDelSistema.Remove(CStr(comboDTO.id))
            combosDelSistema.Add(CStr(comboDTO.id), comboDTO)

            If (comboDTO.insumos IsNot Nothing) Then
                If (comboDTO.insumos.Count > 0) Then
                    Dim insumosDelCombo As New List(Of String)
                    For Each insumo In comboDTO.insumos.Item(0)
                        Dim cantidad As String = comboDTO.insumos.Item(1).Item(comboDTO.insumos.Item(0).IndexOf(insumo))
                        insumosDelCombo.Add(insumo & ";" & cantidad)
                    Next
                    cachearInsumosPorCombo(comboDTO.id, insumosDelCombo)
                End If
            End If
            '4 se recalculan los digitos verificadores
            SeguridadBO.getInstance().calcularDVH("combo")
            SeguridadBO.getInstance().calcularDVV("combo")
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Combo " & comboDTO.id & " actualizado")
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

    Public Function agregarCombo(comboDTO As EntidadesDTO.ComboDTO) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P20_COMBOS_ALTA")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para agregar combos", True)
            End If
            '1 valida
            validarParaAgregar(comboDTO, True)
            comboDTO.id = AccesoADatos.ComboDAO.getInstance().obtenerSiguienteID()
            comboDTO.nombre = SeguridadBO.getInstance().encriptar(comboDTO.nombre, True)
            comboDTO.precio = SeguridadBO.getInstance().encriptar(comboDTO.precio, True)
            '2 insert en la base
            AccesoADatos.ComboDAO.getInstance().agregarCombo(comboDTO)
            comboDTO.nombre = SeguridadBO.getInstance().desencriptar(comboDTO.nombre)
            comboDTO.precio = SeguridadBO.getInstance().desencriptar(comboDTO.precio)
            '3 actualizo cache
            combosDelSistema.Remove(CStr(comboDTO.id))
            combosDelSistema.Add(CStr(comboDTO.id), comboDTO)

            If (comboDTO.insumos IsNot Nothing) Then
                If (comboDTO.insumos.Count > 0) Then
                    Dim insumosDelCombo As New List(Of String)
                    For Each insumo In comboDTO.insumos.Item(0)
                        Dim cantidad As String = comboDTO.insumos.Item(1).Item(comboDTO.insumos.Item(0).IndexOf(insumo))
                        insumosDelCombo.Add(insumo & ";" & cantidad)
                    Next
                    cachearInsumosPorCombo(comboDTO.id, insumosDelCombo)
                End If
            End If
            If (insumosPorCombo IsNot Nothing) Then
                insumosPorCombo.Remove(COMBO_ID_NUEVO)
            End If
            '4 se recalculan los digitos verificadores
            SeguridadBO.getInstance().calcularDVH("combo")
            SeguridadBO.getInstance().calcularDVV("combo")
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Combo " & comboDTO.id & " agregado")
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

    Public Function eliminarCombo(comboDTO As EntidadesDTO.ComboDTO) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P09_INSUMOS_BAJA")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para eliminar insumos", True)
            End If
            'verifica que no haya un pedido asociado a un combo, sino no se lo puede borrar, avisa para que se desasocie al usuario
            If (AccesoADatos.ComboDAO.getInstance().pedidosAsociadosAUnCombo(comboDTO.id) > 0) Then
                Throw New Exceptions.CandyException("No se puede borrar el combo ya que el mismo existe en un pedido", True)
            End If
            '1 eliminar combo ya se valido previamente q no sea user admin y q tenga permisos
            AccesoADatos.ComboDAO.getInstance().eliminarCombo(comboDTO)
            '2 se actualiza la cache
            If (combosDelSistema IsNot Nothing) Then
                combosDelSistema.Remove(CStr(comboDTO.id))
            End If
            If (insumosPorCombo IsNot Nothing) Then
                insumosPorCombo.Remove(CStr(comboDTO.id))
            End If
            '3 se eliminan las asociaciones de insumos sobre combos, etc
            '4 se recalculan los digitos verificadores
            SeguridadBO.getInstance().calcularDVH("combo")
            SeguridadBO.getInstance().calcularDVV("combo")
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Combo " & comboDTO.id & " eliminado")
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

    Public Function validarParaAgregar(comboDTO As EntidadesDTO.ComboDTO, nuevo As Boolean) As Boolean
        'validacion de nombre
        If ("".Equals(comboDTO.nombre)) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el combo")
            Throw New Exceptions.CandyException("Error el nombre no puede ser vacio", True)
        End If
        If (comboDTO.nombre.Length > 100 Or comboDTO.nombre.Length < 2) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el combo")
            Throw New Exceptions.CandyException("Error el nombre debe ser entre 2 y 100 caracteres", True)
        End If

        'validacion de precio
        If ("".Equals(comboDTO.precio)) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el combo")
            Throw New Exceptions.CandyException("Error el precio no puede ser vacio", True)
        End If
        Try
            Dim numero As Decimal = CDec(comboDTO.precio)
            If (numero < 0) Then
                BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el combo")
                Throw New Exceptions.CandyException("Error el precio no puede ser negativo", True)
            End If
            If (numero > 1000000) Then
                BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el combo")
                Throw New Exceptions.CandyException("Error el precio no puede superar 1000000", True)
            End If
        Catch ex As Exception
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion en el combo")
            Throw New Exceptions.CandyException("Error el precio tiene un formato incorrecto", True)
        End Try

        'verifico si estoy
        If (nuevo) Then
            If (obtenerComboPorNombre(comboDTO.nombre) IsNot Nothing) Then
                BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error el nombre del combo ya existe")
                Throw New Exceptions.CandyException("Error el nombre del combo ya existe", True)
            End If
        Else
            Dim comboAux As EntidadesDTO.ComboDTO = obtenerComboPorNombre(comboDTO.nombre)
            If (comboAux IsNot Nothing) Then
                If (comboAux.id <> comboDTO.id) Then
                    BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error el nombre del combo ya existe")
                    Throw New Exceptions.CandyException("Error el nombre del combo ya existe", True)
                End If
            End If
        End If

        Return True
    End Function

    'cambia firma se devuelve un diccionario en vez de una lista
    Public Function obtenerCombos() As Dictionary(Of String, EntidadesDTO.ComboDTO)
        Return obtenerCombos(False)
    End Function

    Private Function obtenerCombos(forzar As Boolean) As Dictionary(Of String, EntidadesDTO.ComboDTO)
        If (combosDelSistema Is Nothing Or forzar) Then
            combosDelSistema = AccesoADatos.ComboDAO.getInstance().obtenerCombos()
            For Each combo In combosDelSistema
                combo.Value.nombre = SeguridadBO.getInstance().desencriptar(combo.Value.nombre)
                combo.Value.precio = SeguridadBO.getInstance().desencriptar(combo.Value.precio)
            Next
        End If
        Return combosDelSistema
    End Function

    'obtiene el dto del combo del cache segun el id especificado
    Public Function obtenerComboPorId(comboId As Integer) As EntidadesDTO.ComboDTO
        Try
            Return obtenerCombos().Item(CStr(comboId))
        Catch exception As KeyNotFoundException
            Return Nothing
        End Try
    End Function

    'se agrega nuevo metodo al analisis
    Public Function obtenerComboPorNombre(nombre As String) As EntidadesDTO.ComboDTO
        Try
            For Each combo In obtenerCombos()
                If (nombre.Equals(combo.Value.nombre)) Then
                    Return combo.Value
                End If
            Next
            Return Nothing
        Catch exception As KeyNotFoundException
            Return Nothing
        Catch exception As Exception
            Return Nothing
        End Try
    End Function

    Public Function obtenerInsumosPorComboId(comboId As Integer) As List(Of List(Of String))
        Return obtenerInsumosPorComboId(comboId, False)
    End Function

    Private Function obtenerInsumosPorComboId(comboId As Integer, forzar As Boolean) As List(Of List(Of String))
        Try
            If (insumosPorCombo Is Nothing Or forzar) Then
                insumosPorCombo = New Dictionary(Of String, List(Of String))
            End If
            Return dividirInsumoYCantidadAsignada(insumosPorCombo.Item(CStr(comboId)))
        Catch exception As KeyNotFoundException
            Dim insumosPorComboList As List(Of String) = AccesoADatos.ComboDAO.getInstance().obtenerInsumosPorCombo(comboId)
            insumosPorCombo.Remove(CStr(comboId))
            insumosPorCombo.Add(CStr(comboId), insumosPorComboList)
            Try
                Return dividirInsumoYCantidadAsignada(insumosPorCombo.Item(CStr(comboId)))
            Catch exception1 As KeyNotFoundException
                Return Nothing
            End Try
        End Try
    End Function

    Private Function dividirInsumoYCantidadAsignada(insumosPorCombo As List(Of String)) As List(Of List(Of String))
        Dim insumosYCantidadesDivididas As New List(Of List(Of String))
        'posicion 0 para insumosIds
        insumosYCantidadesDivididas.Add(New List(Of String))
        'posicion 1 para cantidades
        insumosYCantidadesDivididas.Add(New List(Of String))

        Try
            If (insumosPorCombo IsNot Nothing) Then
                For Each regInsumo In insumosPorCombo
                    Dim insumoId As String = regInsumo.Split(";")(0)
                    Dim cantidadAsignada As String = regInsumo.Split(";")(1)
                    insumosYCantidadesDivididas.Item(0).Add(insumoId)
                    insumosYCantidadesDivididas.Item(1).Add(cantidadAsignada)
                Next
            End If
        Catch exception As Exception
        End Try

        Return insumosYCantidadesDivididas
    End Function

    Public Sub cachearInsumosPorCombo(comboId As Integer, insumosPorComboList As List(Of String))
        If (insumosPorCombo Is Nothing) Then
            insumosPorCombo = New Dictionary(Of String, List(Of String))
        End If

        Try
            insumosPorCombo.Remove(CStr(comboId))
            insumosPorCombo.Add(CStr(comboId), insumosPorComboList)
        Catch exception As KeyNotFoundException
        End Try
    End Sub

    Public Sub cachearInsumosPorComboNuevo(insumosPorComboList As List(Of String))
        If (insumosPorCombo Is Nothing) Then
            insumosPorCombo = New Dictionary(Of String, List(Of String))
        End If

        insumosPorCombo.Remove(CStr(ComboBO.COMBO_ID_NUEVO))
        insumosPorCombo.Add(CStr(ComboBO.COMBO_ID_NUEVO), insumosPorComboList)
    End Sub

    Public Function verificarStock(comboId As Integer) As Boolean
        Dim insumos As List(Of List(Of String)) = obtenerInsumosPorComboId(comboId)
        Dim hayStock As Boolean = True
        Dim mensaje As New System.Text.StringBuilder()

        For i = 0 To insumos(0).Count - 1

            Dim insumoId As Integer = CInt(insumos(0)(i))
            Dim stockRequerido As Integer = CInt(insumos(1)(i))
            Dim insumo As EntidadesDTO.InsumoDTO = InsumoBO.getInstance().obtenerInsumoPorId(insumoId)
            Dim stockDisponible As Integer = insumo.stock
            Dim nombreInsumo As String = insumo.nombre

            If (stockDisponible < stockRequerido) Then
                mensaje.Append(nombreInsumo & vbCrLf)
                hayStock = False
            End If
        Next

        If (Not hayStock) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Stock no disponible para el combo " & comboId)
            Throw New Exceptions.CandyException("Error, stock no disponible para los insumos: " & mensaje.ToString, True)
        End If

        BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Stock verificado para el combo " & comboId)
        Return True
    End Function

    Public Function descontarStock(comboId As Integer) As Boolean
        Try
            verificarStock(comboId)

            Dim insumos As List(Of List(Of String)) = obtenerInsumosPorComboId(comboId)
            For i = 0 To insumos(0).Count - 1
                Dim insumoId As Integer = CInt(insumos(0)(i))
                Dim stockRequerido As Integer = CInt(insumos(1)(i))
                'decuento el stock
                InsumoBO.getInstance().actualizarStock(insumoId, stockRequerido * -1)
            Next

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

    Public Function devolverStock(comboId As Integer) As Boolean
        Try
            Dim insumos As List(Of List(Of String)) = obtenerInsumosPorComboId(comboId)
            For i = 0 To insumos(0).Count - 1
                Dim insumoId As Integer = CInt(insumos(0)(i))
                Dim stockRequerido As Integer = CInt(insumos(1)(i))
                'decuento el stock
                InsumoBO.getInstance().actualizarStock(insumoId, stockRequerido * 1)
            Next

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

    Public Sub actualizarCache()
        obtenerCombos(True)
        For Each combo In combosDelSistema
            obtenerInsumosPorComboId(combo.Key, True)
        Next
    End Sub

End Class

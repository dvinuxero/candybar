Public Class PedidoBO

    'cache de pedidos del sistema consultados en la base al iniciar el modulo de combos
    Private Shared pedidosDelSistema As Dictionary(Of String, EntidadesDTO.PedidoDTO)

    'id de pedido nuevo antes de guardarlo en base de datos
    Public Shared PEDIDO_ID_NUEVO As Integer = 0

    Private Shared _instance As PedidoBO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As PedidoBO
        If (_instance Is Nothing) Then
            _instance = New PedidoBO()
        End If
        Return _instance
    End Function

    Public Function actualizarPedido(pedidoDTO As EntidadesDTO.PedidoDTO) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P16_PEDIDOS_MODIFICAR")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para modificar pedidos", True)
            End If
            '1 valida
            validarParaActualizar(pedidoDTO)
            '2 insert en la base
            AccesoADatos.PedidoDAO.getInstance().actualizarPedido(pedidoDTO)
            '3 actualizo cache
            pedidosDelSistema.Remove(CStr(pedidoDTO.id))
            pedidosDelSistema.Add(CStr(pedidoDTO.id), pedidoDTO)
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Pedido " & pedidoDTO.id & " actualizado")
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

    Public Function agregarPedido(pedidoDTO As EntidadesDTO.PedidoDTO) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P17_PEDIDOS_ALTA")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para agregar pedidos", True)
            End If
            '1 valida
            validarParaAgregar(pedidoDTO)
            pedidoDTO.id = AccesoADatos.PedidoDAO.getInstance().obtenerSiguienteID()
            '2 insert en la base
            AccesoADatos.PedidoDAO.getInstance().agregarPedido(pedidoDTO)
            '3 actualizo cache
            pedidosDelSistema.Remove(CStr(pedidoDTO.id))
            pedidosDelSistema.Add(CStr(pedidoDTO.id), pedidoDTO)
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Pedido " & pedidoDTO.id & " agregado")
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

    Public Function cancelarPedido(pedidoId As Integer) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P12_PEDIDOS_CANCELAR")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para cancelar pedidos", True)
            End If
            '1 cancelar pedido ya se valido previamente q no sea user admin y q tenga permisos
            AccesoADatos.PedidoDAO.getInstance().cancelarPedido(pedidoId)
            '2 se actualiza la cache
            If (pedidosDelSistema IsNot Nothing) Then
                pedidosDelSistema.Remove(CStr(pedidoId))
            End If
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.ALTA, "Pedido " & pedidoId & " cancelado")
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

    'se agrega valor de retorno
    Public Function entregarPedido(pedidoId As Integer) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P14_PEDIDOS_ENTREGAR")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para entregar pedidos", True)
            End If
            '1 entregar pedido ya se valido previamente q no sea user admin y q tenga permisos
            AccesoADatos.PedidoDAO.getInstance().entregarPedido(pedidoId)
            '2 se actualiza la cache
            If (pedidosDelSistema IsNot Nothing) Then
                Try
                    Dim pedidoDTO As EntidadesDTO.PedidoDTO = pedidosDelSistema.Item(CStr(pedidoId))
                    pedidoDTO.estado = EntidadesDTO.PedidoDTO.PedidoEstado.ENTREGADO
                    pedidosDelSistema.Remove(CStr(pedidoDTO.id))
                    pedidosDelSistema.Add(CStr(pedidoDTO.id), pedidoDTO)
                Catch exception As KeyNotFoundException
                End Try
            End If
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Pedido " & pedidoId & " entregado")
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

    Public Function finalizarPedido(pedidoId As Integer) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P15_PEDIDOS_FINALIZAR")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para finalizar pedidos", True)
            End If
            '1 finalizar pedido ya se valido previamente q no sea user admin y q tenga permisos
            AccesoADatos.PedidoDAO.getInstance().finalizarPedido(pedidoId)
            '2 se actualiza la cache
            If (pedidosDelSistema IsNot Nothing) Then
                Try
                    Dim pedidoDTO As EntidadesDTO.PedidoDTO = pedidosDelSistema.Item(CStr(pedidoId))
                    pedidoDTO.estado = EntidadesDTO.PedidoDTO.PedidoEstado.FINALIZADO
                    pedidosDelSistema.Remove(CStr(pedidoDTO.id))
                    pedidosDelSistema.Add(CStr(pedidoDTO.id), pedidoDTO)
                Catch exception As KeyNotFoundException
                End Try
            End If
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Pedido " & pedidoId & " finalizado")
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

    Public Function producirPedido(pedidoId As Integer) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P13_PEDIDOS_PRODUCIR")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para producir pedidos", True)
            End If

            ComboBO.getInstance().descontarStock(obtenerPedidoPorId(pedidoId).comboId)

            '1 producir pedido ya se valido previamente q no sea user admin y q tenga permisos
            AccesoADatos.PedidoDAO.getInstance().producirPedido(pedidoId)
            '2 se actualiza la cache
            If (pedidosDelSistema IsNot Nothing) Then
                Try
                    Dim pedidoDTO As EntidadesDTO.PedidoDTO = pedidosDelSistema.Item(CStr(pedidoId))
                    pedidoDTO.estado = EntidadesDTO.PedidoDTO.PedidoEstado.PRODUCIR
                    pedidosDelSistema.Remove(CStr(pedidoDTO.id))
                    pedidosDelSistema.Add(CStr(pedidoDTO.id), pedidoDTO)
                Catch exception As KeyNotFoundException
                End Try
            End If
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Pedido " & pedidoId & " a producir")
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

    Public Function pasarPedidoAPendiente(pedidoId As Integer) As Boolean
        Try
            If (Not PermisoBO.getInstance().usuarioTienePermisoParaAccion(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P13_PEDIDOS_PRODUCIR")) Then
                Throw New Exceptions.CandyException("Usuario no tiene permiso para producir pedidos", True)
            End If

            ComboBO.getInstance().devolverStock(obtenerPedidoPorId(pedidoId).comboId)

            '1 producir pedido ya se valido previamente q no sea user admin y q tenga permisos
            AccesoADatos.PedidoDAO.getInstance().pasarPedidoAPendiente(pedidoId)
            '2 se actualiza la cache
            If (pedidosDelSistema IsNot Nothing) Then
                Try
                    Dim pedidoDTO As EntidadesDTO.PedidoDTO = pedidosDelSistema.Item(CStr(pedidoId))
                    pedidoDTO.estado = EntidadesDTO.PedidoDTO.PedidoEstado.PENDIENTE
                    pedidosDelSistema.Remove(CStr(pedidoDTO.id))
                    pedidosDelSistema.Add(CStr(pedidoDTO.id), pedidoDTO)
                Catch exception As KeyNotFoundException
                End Try
            End If
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.BAJA, "Pedido " & pedidoId & " a pendiente")
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

    'cambia nombre del metodo es PorId
    Public Function obtenerPedidoPorId(pedidoId As Integer) As EntidadesDTO.PedidoDTO
        Try
            Return obtenerPedidos().Item(CStr(pedidoId))
        Catch exception As KeyNotFoundException
            Return Nothing
        End Try
    End Function

    'cambia firma se devuelve un diccionario en vez de una lista
    Public Function obtenerPedidos() As Dictionary(Of String, EntidadesDTO.PedidoDTO)
        Return obtenerPedidos(False)
    End Function

    Private Function obtenerPedidos(forzar As Boolean) As Dictionary(Of String, EntidadesDTO.PedidoDTO)
        If (pedidosDelSistema Is Nothing Or forzar) Then
            pedidosDelSistema = AccesoADatos.PedidoDAO.getInstance().obtenerPedidos()
            If (pedidosDelSistema Is Nothing) Then
                pedidosDelSistema = New Dictionary(Of String, EntidadesDTO.PedidoDTO)
            End If
        End If
        Return pedidosDelSistema
    End Function

    'se agrega parametro a la firma
    Public Function validarParaActualizar(pedidoDTO As EntidadesDTO.PedidoDTO) As Boolean
        'validacion de la existencia de un combo
        If (pedidoDTO.comboId = 0 Or ComboBO.getInstance().obtenerComboPorId(pedidoDTO.comboId) Is Nothing) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al modificar pedido")
            Throw New Exceptions.CandyException("Error el pedido debe tener un combo asignado", True)
        End If

        If (pedidoDTO.agasajado.Length > 30 Or pedidoDTO.agasajado.Length < 2) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al modificar pedido")
            Throw New Exceptions.CandyException("Error el agasajado debe ser entre 2 y 30 caracteres", True)
        End If

        'validar fechas

        'validar comentario
        If (pedidoDTO.comentario.Length > 400) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al modificar pedido")
            Throw New Exceptions.CandyException("Error el comentario debe ser menor a 30 caracteres", True)
        End If

        Return True
    End Function

    'se agrega parametro a la firma
    Public Function validarParaAgregar(pedidoDTO As EntidadesDTO.PedidoDTO) As Boolean
        'validacion de la existencia de un combo
        If (pedidoDTO.comboId = 0 And ComboBO.getInstance().obtenerComboPorId(pedidoDTO.comboId) Is Nothing) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al agregar pedido")
            Throw New Exceptions.CandyException("Error el pedido debe tener un combo asignado", True)
        End If

        If (pedidoDTO.agasajado.Length > 30 Or pedidoDTO.agasajado.Length < 2) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al agregar pedido")
            Throw New Exceptions.CandyException("Error el agasajado debe ser entre 2 y 30 caracteres", True)
        End If

        'validar estado
        If (pedidoDTO.estado <> EntidadesDTO.PedidoDTO.PedidoEstado.PENDIENTE) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al agregar pedido")
            Throw New Exceptions.CandyException("Error el pedido debe crearse con un estado pendiente", True)
        End If

        'validar fechas
        If ("".Equals(pedidoDTO.fechaInicio)) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al agregar pedido")
            Throw New Exceptions.CandyException("Error por favor elija una fecha de inicio del pedido", True)
        End If

        If ("".Equals(pedidoDTO.fechaEntrega)) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al agregar pedido")
            Throw New Exceptions.CandyException("Error por favor elija una fecha de entrega del pedido", True)
        End If

        'validar comentario
        If (pedidoDTO.comentario.Length > 400) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Error de validacion al agregar pedido")
            Throw New Exceptions.CandyException("Error el comentario debe ser menor a 400 caracteres", True)
        End If

        'verificar stock!!

        Return True
    End Function

    Public Function obtenerSiguienteEstado(pedidoId As Integer) As EntidadesDTO.PedidoDTO.PedidoEstado
        Dim pedido As EntidadesDTO.PedidoDTO = obtenerPedidoPorId(pedidoId)

        Select Case pedido.estado
            Case EntidadesDTO.PedidoDTO.PedidoEstado.PENDIENTE
                Return EntidadesDTO.PedidoDTO.PedidoEstado.PRODUCIR
            Case EntidadesDTO.PedidoDTO.PedidoEstado.PRODUCIR
                Return EntidadesDTO.PedidoDTO.PedidoEstado.FINALIZADO
            Case EntidadesDTO.PedidoDTO.PedidoEstado.FINALIZADO
                Return EntidadesDTO.PedidoDTO.PedidoEstado.ENTREGADO
            Case Else
                Return EntidadesDTO.PedidoDTO.PedidoEstado.PENDIENTE
        End Select
    End Function

    Public Function obtenerAnteriorEstado(pedidoId As Integer) As EntidadesDTO.PedidoDTO.PedidoEstado
        Dim pedido As EntidadesDTO.PedidoDTO = obtenerPedidoPorId(pedidoId)

        Select Case pedido.estado
            Case EntidadesDTO.PedidoDTO.PedidoEstado.PRODUCIR
                Return EntidadesDTO.PedidoDTO.PedidoEstado.PENDIENTE
            Case EntidadesDTO.PedidoDTO.PedidoEstado.FINALIZADO
                Return EntidadesDTO.PedidoDTO.PedidoEstado.PRODUCIR
            Case EntidadesDTO.PedidoDTO.PedidoEstado.ENTREGADO
                Return EntidadesDTO.PedidoDTO.PedidoEstado.FINALIZADO
            Case Else
                Return EntidadesDTO.PedidoDTO.PedidoEstado.ENTREGADO
        End Select
    End Function

    Public Sub actualizarCache()
        obtenerPedidos(True)
    End Sub

End Class

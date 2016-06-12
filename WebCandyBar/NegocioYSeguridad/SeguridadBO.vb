'Clase encargada de realizar diferentes operaciones de seguridad que mantiene el sistema.
'Por ej calculo de dvh, dvv, encriptacion y 

Public Class SeguridadBO

    Private Shared camposDVHPorTabla As Dictionary(Of String, List(Of String))

    Private Shared CADENA_GENERADORA_CONTRASENIAS As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"

    Private Shared mensaje As String = ""

    Private Shared _instance As SeguridadBO

    Private Sub New()
        'configuracion de integridad
        'que campos respetar para calcular dvh y dvv
        camposDVHPorTabla = New Dictionary(Of String, List(Of String))

        Dim camposUsuarioPatente As New List(Of String)
        camposUsuarioPatente.Add("usuario_id")
        camposUsuarioPatente.Add("patente_id")
        camposUsuarioPatente.Add("negado")
        camposDVHPorTabla.Add("usuario_patente", camposUsuarioPatente)

        Dim camposFamiliaPatente As New List(Of String)
        camposFamiliaPatente.Add("patente_id")
        camposFamiliaPatente.Add("familia_id")
        camposDVHPorTabla.Add("familia_patente", camposFamiliaPatente)

        Dim camposFamiliaBitacora As New List(Of String)
        camposFamiliaBitacora.Add("id")
        camposFamiliaBitacora.Add("usuario_id")
        camposFamiliaBitacora.Add("fecha")
        camposFamiliaBitacora.Add("descripcion")
        camposFamiliaBitacora.Add("nivel_criticidad")
        camposDVHPorTabla.Add("bitacora", camposFamiliaBitacora)

        Dim camposInsumo As New List(Of String)
        camposInsumo.Add("nombre")
        camposInsumo.Add("precio_unidad")
        camposInsumo.Add("stock")
        camposDVHPorTabla.Add("insumo", camposInsumo)

        Dim camposCombo As New List(Of String)
        camposCombo.Add("nombre")
        camposCombo.Add("precio")
        camposDVHPorTabla.Add("combo", camposCombo)
    End Sub

    Public Shared Function getInstance() As SeguridadBO
        If (_instance Is Nothing) Then
            _instance = New SeguridadBO()
        End If
        Return _instance
    End Function

    'cambio firma de este metodo, deja de recibir un parametro usuarioDTO
    Public Function autogenerarContrasenia() As String
        Dim r As New Random
        Dim sb As New System.Text.StringBuilder()

        For i As Integer = 1 To 20
            Dim idx As Integer = r.Next(0, 35)
            sb.Append(CADENA_GENERADORA_CONTRASENIAS.Substring(idx, 1))
        Next

        Return sb.ToString().ToLower()
    End Function

    'calcula el valor dvh en los registros null
    Public Function calcularDVH(tabla As String) As Boolean
        Dim registrosYDVHPendientes As DataSet = AccesoADatos.SeguridadDAO.getInstance().buscarDVHYRegistrosPorTablaPendientes(camposDVHPorTabla.Item(tabla), tabla)
        If (Not (registrosYDVHPendientes Is Nothing)) Then
            For Each regPendiente As DataRow In registrosYDVHPendientes.Tables(0).Rows
                Dim cadenaACalcularDVH As String = ""
                For Each campo In camposDVHPorTabla.Item(tabla)
                    If (IsDBNull(regPendiente(campo))) Then
                        cadenaACalcularDVH += ""
                    Else
                        cadenaACalcularDVH += CStr(regPendiente(campo))
                    End If
                Next
                Dim dvhCalculado As Long = obtenerValorDVH(cadenaACalcularDVH)
                regPendiente("dvh") = dvhCalculado
            Next
        End If
        AccesoADatos.SeguridadDAO.getInstance().actualizarDVHPorTabla(registrosYDVHPendientes, camposDVHPorTabla.Item(tabla), tabla)
        Return True
    End Function

    'actualiza el valor del dvv para la tabla especificada
    Public Function calcularDVV(tabla As String) As Boolean
        Dim ejecutado As Boolean = AccesoADatos.SeguridadDAO.getInstance().actualizarDVVPorTabla(tabla)
        Return ejecutado
    End Function

    'agrega metodo antes era el calcularDVH pero ahora ese mismo se utiliza para otra cosa
    Public Function obtenerValorDVH(cadena As String) As Long
        Dim dvhTotal As Long = 0
        Dim index As Integer = 1
        For Each caracter In cadena.ToCharArray()
            Dim valorAscii As Integer = Asc(caracter)
            dvhTotal += (valorAscii * index)
            index += 1
        Next
        Return dvhTotal
    End Function

    'este metodo calcula si hay problemas de dvh sobre una tabla y me devuelve el dvv sumarizado si no hubo problemas sino una exception
    Public Function chequearDVHPorTabla(tabla As String) As Long
        NegocioYSeguridad.SeguridadBO.guardarMensaje("calculando integridad [tabla: " & tabla & "]...")
        Dim registrosYDVH As Dictionary(Of String, Long) = AccesoADatos.SeguridadDAO.getInstance().buscarDVHYRegistrosPorTabla(camposDVHPorTabla.Item(tabla), tabla)
        Dim dvv As Long = 0
        Dim integridadRespetada As Boolean = True
        If (registrosYDVH Is Nothing) Then
            'registro en la bitacora el error de dvh
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.ALTA, "Error por dvh en tabla: " & tabla)
            integridadRespetada = False
            NegocioYSeguridad.SeguridadBO.guardarMensaje("calculando integridad [tabla: " & tabla & " dvh null]...")
        Else
            For Each dvh In registrosYDVH
                Dim dvhAux As Long = obtenerValorDVH(dvh.Key)
                If (Not compararDVH_DVHAux(dvh.Value, dvhAux)) Then
                    'registro en la bitacora el error de dvh y marco el campo en null para luego ser corregido
                    BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.ALTA, "Error por dvh en tabla: " & tabla & " registro: " & dvh.Key & " <> " & dvh.Value)
                    AccesoADatos.SeguridadDAO.getInstance().marcarErrorEnDVHPorRegistro(tabla, dvh.Value)
                    integridadRespetada = False
                End If
                dvv += dvh.Value
                NegocioYSeguridad.SeguridadBO.guardarMensaje("calculando integridad [tabla: " & tabla & " dvh:" & dvh.Value & "]...")
            Next
        End If
        If (Not integridadRespetada) Then
            dvv = -1
        End If
        Return dvv
    End Function

    Public Function chequearIntegridad() As Boolean
        Dim integridadRespetada As Boolean = True
        Dim mapeoTablaDVV As Dictionary(Of String, Long) = AccesoADatos.SeguridadDAO.getInstance().buscarValoresDVV()

        For Each tabla In camposDVHPorTabla.Keys
            Try
                Dim dvv As Long = mapeoTablaDVV.Item(tabla)
            Catch ex As KeyNotFoundException
                'tabla no existe en la configuracion de calculo de dvh, se inserta obligatoriamente para ser recalculada
                AccesoADatos.SeguridadDAO.getInstance().actualizarDVVPorTabla(tabla)
                mapeoTablaDVV = AccesoADatos.SeguridadDAO.getInstance().buscarValoresDVV()
                integridadRespetada = False
            End Try
        Next

        For Each tabla In mapeoTablaDVV
            Dim dvvAux As Long = chequearDVHPorTabla(tabla.Key)
            If (dvvAux = -1) Then
                integridadRespetada = False
            End If
            NegocioYSeguridad.SeguridadBO.guardarMensaje("calculando integridad [tabla: " & tabla.Key & " dvv: " & tabla.Value & "]...")
            If (Not compararDVVPorTabla(tabla.Value, dvvAux)) Then
                'registro en la bitacora el error de dvv y marco el campo en null para luego ser corregido
                BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.ALTA, "Error por dvv en tabla: " & tabla.Key & " valor: " & tabla.Value)
                AccesoADatos.SeguridadDAO.getInstance().marcarErrorEnDVVPorTabla(tabla.Key)
                integridadRespetada = False
            End If
        Next
        NegocioYSeguridad.SeguridadBO.guardarMensaje("")
        Return integridadRespetada
    End Function

    Public Function corregirIntegridad() As Boolean
        Dim mapeoTablaDVV As Dictionary(Of String, Long) = AccesoADatos.SeguridadDAO.getInstance().buscarValoresDVV()
        For Each tabla In mapeoTablaDVV
            NegocioYSeguridad.SeguridadBO.guardarMensaje("corrigiendo integridad [tabla: " & tabla.Key & " dvh]...")
            calcularDVH(tabla.Key)
            NegocioYSeguridad.SeguridadBO.guardarMensaje("corrigiendo integridad [tabla: " & tabla.Key & " dvv]...")
            calcularDVV(tabla.Key)
        Next
        BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.ALTA, "Integridad corregida en la base de datos")
        NegocioYSeguridad.SeguridadBO.guardarMensaje("")
        Return True
    End Function

    Public Function compararDVH_DVHAux(dvh As Long, dvhAux As Long) As Boolean
        Return (dvh = dvhAux)
    End Function

    Public Function compararDVVPorTabla(dvv As Long, dvvAux As Long) As Boolean
        Return (dvv = dvvAux)
    End Function

    Public Function desencriptar(key As String) As String
        Try
            Dim campoEnBytesDesencriptado() As Byte = Convert.FromBase64String(key)
            Return System.Text.Encoding.UTF8.GetString(campoEnBytesDesencriptado)
        Catch exception As FormatException
            Return ""
        End Try
    End Function

    Public Function encriptar(campo As String, reversible As Boolean) As String
        Try
            'tener en cuenta si el Unicode no trae problemas, realizar pruebas
            Dim campoEnBytes() As Byte = System.Text.Encoding.UTF8.GetBytes(campo)

            If (reversible) Then
                Return Convert.ToBase64String(campoEnBytes)
            Else
                Return getMD5HashData(campo)
            End If
        Catch exception As Exception
            Return ""
        End Try
    End Function

    Private Function getMD5HashData(data As String) As String
        Dim md5 As System.Security.Cryptography.MD5 = System.Security.Cryptography.MD5.Create()
        Dim hashData() As Byte = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(data))
        Dim returnValue As New System.Text.StringBuilder()

        For i As Integer = 0 To hashData.Length - 1
            returnValue.Append(hashData(i).ToString())
        Next

        Return returnValue.ToString()
    End Function

    'metodo nuevo se agrega al analisis
    'se considera q ya ha sido cambiada la password
    'se envia de manera simulada un mail al usuario, entonces queda guardado en un txt dentro de una carpeta
    Friend Function informarPasswordAlUsuario(usuarioId As Integer, contraseniaNueva As String) As Boolean
        Dim directorio As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        Dim nickname As String = UsuarioBO.getInstance().obtenerUsuarioPorId(usuarioId).nickname

        Dim writer As New System.IO.StreamWriter(directorio + "/password_" & nickname.ToLower() & ".txt", False)
        writer.WriteLine(nickname & " te enviamos la nueva contraseña: " & contraseniaNueva)
        writer.Close()
        BitacoraBO.getInstance().guardarEvento(usuarioId, BitacoraBO.TipoCriticidad.MEDIA, "Cambio de contraseña")
        Return True
    End Function

    Public Shared Sub guardarMensaje(msj As String)
        SyncLock SeguridadBO.mensaje
            SeguridadBO.mensaje = msj
        End SyncLock
    End Sub

    Public Shared Function obtenerMensaje() As String
        Dim m As String = ""
        SyncLock SeguridadBO.mensaje
            m = SeguridadBO.mensaje
        End SyncLock
        Return m
    End Function

    Public Function obtenerPathSource() As String
        Dim fullPath As String = IO.Path.GetFullPath(My.Resources.ResourceManager.BaseName)
        fullPath = fullPath.Substring(0, fullPath.LastIndexOf("\") + 1)
        Return fullPath
    End Function

End Class

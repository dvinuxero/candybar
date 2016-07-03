'Clase encargada de realizar el llamado a la base de datos para correr el script de RESTORE y BACKUP
'cambio el nombre con respecto al analisis

Public Class BackUpRestoreBO

    Private Shared _instance As BackUpRestoreBO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As BackUpRestoreBO
        If (_instance Is Nothing) Then
            _instance = New BackUpRestoreBO()
        End If
        Return _instance
    End Function

    Public Function realizarBackUp(listaDeBackups As List(Of String)) As Boolean
        Dim ejecutado As Boolean = AccesoADatos.BaseDeDatos.realizarBackUp(listaDeBackups)
        If (ejecutado) Then
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Backup realizado")
        End If
        Return ejecutado
    End Function

    'cambia la firma con el analisis, es una lista de restores ahora
    Public Function realizarRestore(listaDeRestores As List(Of String)) As Boolean
        Dim ejecutado As Boolean = AccesoADatos.BaseDeDatos.realizarRestore(listaDeRestores)
        If (ejecutado) Then
            ComboBO.getInstance().actualizarCache()
            InsumoBO.getInstance().actualizarCache()
            PedidoBO.getInstance().actualizarCache()
            PermisoBO.getInstance().actualizarCache()
            UsuarioBO.getInstance().actualizarCache()
            BitacoraBO.getInstance().guardarEvento(UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), BitacoraBO.TipoCriticidad.MEDIA, "Restore realizado")
        End If
        Return ejecutado
    End Function

End Class

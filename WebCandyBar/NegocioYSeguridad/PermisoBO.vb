'Clase encargada de manejar la logica de permisos del sistema

'Patentes escenciales:
'    -Asociar patentes -> es necesario para asignar patentes nuevas a un usuario
'    -Modificar usuarios -> es necesario para modificar patentes de un usuario, etc
'    -Usuario administrador -> es necesaria para no bloquear al usuario x intentos incorrectos

Public Class PermisoBO

    Public Shared PATENTE_NEGADA_FLAG As String = "::SI::"
    Public Shared ESCENCIA_NEGADA_POR_HERENCIA As String = "::ESCENCIAL_NEGADA_POR_HERENCIA::"
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

    Public Function obtenerFamiliasPorUsuario(usuarioId As Integer) As List(Of String)
        AccesoADatos.PermisoDAO.getInstance().obtenerFamiliasPorUsuario(usuarioId)
    End Function

    Public Function obtenerPatentesPorFamilia(familiaId As String) As List(Of String)
        AccesoADatos.PermisoDAO.getInstance().obtenerPatentesPorFamilia(familiaId)
    End Function

    Public Function obtenerPatentes() As Dictionary(Of String, String)
        AccesoADatos.PermisoDAO.getInstance().obtenerPatentes()
    End Function

    Public Function obtenerPatentesPorUsuario(usuarioId As Integer) As List(Of String)
        Return AccesoADatos.PermisoDAO.getInstance().obtenerPatentesPorUsuario(usuarioId)
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
            Dim familiasDelUsuario As List(Of String) = obtenerFamiliasPorUsuario(usuarioId)
            If (familiasDelUsuario IsNot Nothing) Then
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
        End If

        Return tieneTodasLasEscenciales
    End Function

End Class

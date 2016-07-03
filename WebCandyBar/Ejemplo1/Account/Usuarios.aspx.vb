Public Class Usuarios
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If (Not NegocioYSeguridad.PermisoBO.getInstance().usuarioTienePermisoParaAccion(NegocioYSeguridad.UsuarioBO.getInstance().obtenerUsuarioIdLogueado(), "P004_USUARIOS_ALL")) Then
                Server.Transfer("/Error.aspx")
            End If
        Catch ex As Exception
            Server.Transfer("/Error.aspx")
        End Try
    End Sub

End Class
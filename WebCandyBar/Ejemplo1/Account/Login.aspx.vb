Public Class Login
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        'RegisterHyperLink.NavigateUrl = "Register.aspx"
        'OpenAuthLogin.ReturnUrl = Request.QueryString("ReturnUrl")

        'Dim returnUrl = HttpUtility.UrlEncode(Request.QueryString("ReturnUrl"))
        'If Not String.IsNullOrEmpty(returnUrl) Then
        '    RegisterHyperLink.NavigateUrl &= "?ReturnUrl=" & returnUrl
        'End If

        Dim isLogin As String = Request.Form("login")

        If isLogin IsNot Nothing Then

            If (Session("user") IsNot Nothing) Then
                Server.Transfer("/Default.aspx")
                Return
            End If

            'valido
            Dim nickname As String = Request.Form("nickname")
            Dim password As String = Request.Form("password")

            Try
                Dim usuarioLogueado As EntidadesDTO.UsuarioDTO = NegocioYSeguridad.UsuarioBO.getInstance().loguearUsuario(nickname, password)
                Session.Add("user", usuarioLogueado)
                Session.Remove("error")
                Server.Transfer("/Default.aspx")
            Catch ex As Exceptions.CandyException
                Session.Add("error", ex.Message)
                If (ex.Message.Contains("[integridad]")) Then
                    If (NegocioYSeguridad.UsuarioBO.getInstance().esUsuarioAdministrador(NegocioYSeguridad.UsuarioBO.getInstance().obtenerUsuarioIdLogueado())) Then
                        Session.Add("corrupcion", True)
                    Else
                        Session.Remove("user")
                    End If
                End If
            End Try
        ElseIf ("exit".Equals(Request.QueryString("action"))) Then
            Session.Remove("user")
            Server.Transfer("/Default.aspx")
        ElseIf ("resolver".Equals(Request.Form("action")) And "corrupcion".Equals(Request.Form("error"))) Then
            Try
                If (NegocioYSeguridad.SeguridadBO.getInstance().corregirIntegridad()) Then
                    Session.Remove("error")
                    Server.Transfer("/Default.aspx")
                End If
            Catch ex As Exceptions.CandyException
                Session.Add("error", ex.Message)
                Session.Remove("user")
            End Try
        End If
    End Sub
End Class
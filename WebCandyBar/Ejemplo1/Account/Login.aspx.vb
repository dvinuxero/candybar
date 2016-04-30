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
            'valido
            Dim nickname As String = Request.Form("nickname")
            Dim password As String = Request.Form("password")

            Try
                NegocioYSeguridad.UsuarioBO.getInstance().loguearUsuario(nickname, password)

                Server.Transfer("Combos.aspx")

            Catch ex As Exceptions.CandyException
                Session.Add("error", ex.Message)
            End Try
        End If
    End Sub
End Class
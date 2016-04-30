'Clase encargada de realizar diferentes operaciones de seguridad que mantiene el sistema.
'Por ej calculo de dvh, dvv, encriptacion y 

Public Class SeguridadBO

    Private Shared CADENA_GENERADORA_CONTRASENIAS As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"

    Private Shared _instance As SeguridadBO

    Private Sub New()
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
        'REALIZAR UN ENVIO DE MAIL
        'Dim directorio As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        'Dim nickname As String = UsuarioBO.getInstance().obtenerUsuarioPorId(usuarioId).nickname

        'Dim writer As New System.IO.StreamWriter(directorio + "/password_" & nickname.ToLower() & ".txt", False)
        'writer.WriteLine(nickname & " te enviamos la nueva contraseña: " & contraseniaNueva)
        'writer.Close()
        BitacoraBO.getInstance().guardarEvento(usuarioId, BitacoraBO.TipoCriticidad.MEDIA, "Cambio de contraseña")
        Return True
    End Function

End Class

'Clase de transporte de los datos de un usuario

Public Class UsuarioDTO

    Public id As Integer

    Public nombre As String
    Public apellido As String
    Public nickname As String
    Public password As String
    Public lang As String

    Public intentosIncorrectos As Integer
    Public baja As String

    Public Shared BAJA_FLAG As String = "NO"

End Class

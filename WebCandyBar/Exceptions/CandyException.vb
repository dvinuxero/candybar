'Clase que maneja y atrapa los errores mas importantes de la aplicacion
'podemos indicarle si es un error con mensaje informativo hacia el usuario para
'poder visualizarlo desde la interfaz

Public Class CandyException : Inherits Exception

    Private mInformarMensaje As Boolean
    Public Property informarEstado() As Boolean
        Get
            Return mInformarMensaje
        End Get
        Set(ByVal value As Boolean)
            mInformarMensaje = value
        End Set
    End Property

    Public Sub New()
    End Sub

    Public Sub New(mensaje As String)
        MyBase.New(mensaje)
        Me.informarEstado = False
    End Sub

    Public Sub New(mensaje As String, informarEstado As Boolean)
        MyBase.New(mensaje)
        Me.informarEstado = informarEstado
    End Sub

End Class

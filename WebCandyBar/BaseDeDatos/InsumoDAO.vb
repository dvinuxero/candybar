Public Class InsumoDAO

    Private Shared ultimoIdUtilizado As Integer

    Private Shared _instance As InsumoDAO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As InsumoDAO
        If (_instance Is Nothing) Then
            _instance = New InsumoDAO()
        End If
        Return _instance
    End Function

    Public Function actualizarInsumo(insumoDTO As EntidadesDTO.InsumoDTO) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update insumo set " _
                                                     & "nombre='" & insumoDTO.nombre & "', " _
                                                     & "tipo='" & insumoDTO.tipo & "', " _
                                                     & "precio_unidad='" & insumoDTO.precioUnidad & "', " _
                                                     & "stock='" & insumoDTO.stock & "', dvh=null where id=" & insumoDTO.id)
        Return ejecutado
    End Function

    Public Function agregarInsumo(insumoDTO As EntidadesDTO.InsumoDTO) As Boolean
        Dim ejecutado = BaseDeDatos.ejecutarConsulta("insert into insumo(id, nombre, tipo, precio_unidad, stock) values(" _
                                                     & insumoDTO.id & ",'" _
                                                     & insumoDTO.nombre & "','" _
                                                     & insumoDTO.tipo & "','" _
                                                     & insumoDTO.precioUnidad & "','" _
                                                     & insumoDTO.stock & "')")
        Return ejecutado
    End Function

    Public Function eliminarInsumo(insumoDTO As EntidadesDTO.InsumoDTO) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("delete from insumo where id = " & insumoDTO.id)
        Return ejecutado
    End Function

    Public Function obtenerInsumos() As Dictionary(Of String, EntidadesDTO.InsumoDTO)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select id, tipo, nombre, precio_unidad, stock from insumo")

        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim insumosDelSistema As New Dictionary(Of String, EntidadesDTO.InsumoDTO)
            For Each row In dataSet.Tables(0).Rows
                Dim insumo As New EntidadesDTO.InsumoDTO()
                insumo.id = row("id")
                insumo.tipo = row("tipo")
                insumo.nombre = row("nombre")
                insumo.precioUnidad = row("precio_unidad")
                insumo.stock = row("stock")
                insumosDelSistema.Add(CStr(insumo.id), insumo)
            Next
            Return insumosDelSistema
        End If
    End Function

    Public Function actualizarStock(insumoId As Integer, stockActual As String) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update insumo set dvh=null, stock='" & stockActual & "' where id=" & insumoId)
        Return ejecutado
    End Function

    Public Function obtenerInsumosYStockPorCombo(comboId As Integer) As Dictionary(Of String, EntidadesDTO.InsumoDTO)
        Return Nothing
    End Function

    Public Function asociarInsumoAlCombo(insumoId As Integer, comboId As Integer, metodo As String, cantidad As Integer) As Boolean
        Return False
    End Function

    Public Function obtenerSiguienteID() As Integer
        If (ultimoIdUtilizado = 0) Then
            ultimoIdUtilizado = BaseDeDatos.ejecutarScalar("select isnull(max(id), 0) from insumo")
        End If
        ultimoIdUtilizado += 1
        Return ultimoIdUtilizado
    End Function

End Class

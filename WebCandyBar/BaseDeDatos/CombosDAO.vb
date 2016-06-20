Public Class ComboDAO

    Private Shared ultimoIdUtilizado As Integer

    Private Shared _instance As ComboDAO

    Private Sub New()
    End Sub

    Public Shared Function getInstance() As ComboDAO
        If (_instance Is Nothing) Then
            _instance = New ComboDAO()
        End If
        Return _instance
    End Function

    Public Function actualizarCombo(comboDTO As EntidadesDTO.ComboDTO) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("update combo set " _
                                                     & "nombre='" & comboDTO.nombre & "', " _
                                                     & "precio='" & comboDTO.precio & "', dvh=null where id=" & comboDTO.id)
        If (comboDTO.insumos IsNot Nothing) Then
            If (comboDTO.insumos.Count > 0) Then
                asociarInsumosAlCombo(comboDTO.id, comboDTO.insumos)
            End If
        End If
        Return ejecutado
    End Function

    Public Function agregarCombo(comboDTO As EntidadesDTO.ComboDTO) As Boolean
        Dim ejecutado = BaseDeDatos.ejecutarConsulta("insert into combo(id, nombre, precio) values(" _
                                                     & comboDTO.id & ",'" _
                                                     & comboDTO.nombre & "','" _
                                                     & comboDTO.precio & "')")
        If (comboDTO.insumos IsNot Nothing) Then
            If (comboDTO.insumos.Count > 0) Then
                asociarInsumosAlCombo(comboDTO.id, comboDTO.insumos)
            End If
        End If
        Return ejecutado
    End Function

    Public Function eliminarCombo(comboDTO As EntidadesDTO.ComboDTO) As Boolean
        Dim ejecutado As Boolean = BaseDeDatos.ejecutarConsulta("delete from combo where id = " & comboDTO.id)
        eliminarInsumosDelCombo(comboDTO.id)
        Return ejecutado
    End Function

    Public Function pedidosAsociadosAUnCombo(comboId As Integer) As Long
        Dim cantidadDeAsociaciones As Long = BaseDeDatos.ejecutarScalar("select count(1) from pedido where combo_id=" & comboId)
        Return cantidadDeAsociaciones
    End Function

    Public Function obtenerCombos() As Dictionary(Of String, EntidadesDTO.ComboDTO)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select id, nombre, precio from combo")

        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim combosDelSistema As New Dictionary(Of String, EntidadesDTO.ComboDTO)
            For Each row In dataSet.Tables(0).Rows
                Dim combo As New EntidadesDTO.ComboDTO()
                combo.id = row("id")
                combo.nombre = row("nombre")
                combo.precio = row("precio")
                'llenar los pedidos
                combosDelSistema.Add(CStr(combo.id), combo)
            Next
            Return combosDelSistema
        End If
    End Function

    Public Function obtenerInsumosPorCombo(comboId As Integer) As List(Of String)
        Dim dataSet As DataSet = BaseDeDatos.listarConsulta("select insumo_id, cantidad from combo_insumo where combo_id = " & comboId)
        If (dataSet Is Nothing) Then
            Return Nothing
        Else
            Dim insumosPorCombo As New List(Of String)
            For Each row In dataSet.Tables(0).Rows
                insumosPorCombo.Add(row("insumo_id") & ";" & row("cantidad"))
            Next
            Return insumosPorCombo
        End If
    End Function

    'cambia firma y nombre del metodo, antes era en singular
    Private Function asociarInsumosAlCombo(comboId As Integer, insumosDelCombo As List(Of List(Of String))) As Boolean
        Try
            eliminarInsumosDelCombo(comboId)
            For Each insumo In insumosDelCombo.Item(0)
                Dim cantidad As Integer = CInt(insumosDelCombo.Item(1).Item(insumosDelCombo.Item(0).IndexOf(insumo)))
                BaseDeDatos.ejecutarConsulta("insert into combo_insumo(combo_id, insumo_id, cantidad) values(" & comboId & "," & insumo & "," & cantidad & ")")
            Next
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function eliminarInsumosDelCombo(comboId As Integer) As Boolean
        Try
            BaseDeDatos.ejecutarConsulta("delete from combo_insumo where combo_id=" & comboId)
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function eliminarInsumosAsociadosAUnCombo(insumoId As Integer) As Boolean
        Try
            BaseDeDatos.ejecutarConsulta("delete from combo_insumo where insumo_id=" & insumoId)
            Return True
        Catch ex As Exceptions.CandyException
            Return False
        End Try
    End Function

    Public Function obtenerSiguienteID() As Integer
        If (ultimoIdUtilizado = 0) Then
            ultimoIdUtilizado = BaseDeDatos.ejecutarScalar("select isnull(max(id), 0) from combo")
        End If
        ultimoIdUtilizado += 1
        Return ultimoIdUtilizado
    End Function

End Class

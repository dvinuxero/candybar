<%@ Page Title="Administracion de insumos" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Insumos.aspx.vb" Inherits="WebCandyBar.Insumos" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        If ("combo".Equals(Request.QueryString("action")) Or "combo".Equals(Request.Form("action"))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim comboId As Integer = Integer.Parse(Request.QueryString("id"))
                Dim insumosDelCombo As List(Of List(Of String)) = NegocioYSeguridad.ComboBO.getInstance().obtenerInsumosPorComboId(comboId)
                Dim insumos = NegocioYSeguridad.InsumoBO.getInstance().obtenerInsumos()
            
                Response.Write("<form action='Insumos.aspx' method='post'>")
                Response.Write("<input type='hidden' name='action' value='combo'>")
                Response.Write("<input type='hidden' name='id' value='" + comboId.ToString() + "'>")
                Response.Write("<table>")
                Response.Write("<tr><td><b>ASIGNAR</b></td><td><b>NOMBRE</b></td><td><b>TIPO</b></td><td><b>PRECIO POR UNIDAD</b></td><td><b>STOCK DEL COMBO</b></td></tr>")
                For Each insumo As EntidadesDTO.InsumoDTO In insumos.Values
                    Dim checkedAsignado As String = ""
                    Dim stockDelCombo As String = "0"
                    If (insumosDelCombo(0).Contains(insumo.id)) Then
                        checkedAsignado = "checked"
                        stockDelCombo = insumosDelCombo(1)(insumosDelCombo(0).IndexOf(insumo.id)).ToString()
                    Else
                        checkedAsignado = ""
                        stockDelCombo = "0"
                    End If
                
                    Response.Write("<tr>")
                    Response.Write("<td><input type='checkbox' name='asignar' value='" + insumo.id.ToString() + "' " + checkedAsignado + "/></td>")
                    Response.Write("<td>" + insumo.nombre + "</td>")
                    Response.Write("<td>" + insumo.tipo + "</td>")
                    Response.Write("<td>" + insumo.precioUnidad + "</td>")
                    Response.Write("<td><input type='text' name='stock_" + insumo.id.ToString() + "' value='" + stockDelCombo + "' /></td>")
                    Response.Write("</tr>")
                Next
                Response.Write("<tr><td><input type='submit' name='guardarInsumosDelCombo' value='Guardar' /> <a href='/Account/Combos.aspx'>Volver</a></td></tr>")
                Response.Write("</table>")
                Response.Write("</form>")
            Else
                Dim comboId As Integer = Integer.Parse(Request.Form("id"))
                Dim insumosAsignadosAlCombo = Request.Form("asignar")
                Dim insumosAsignados As New List(Of List(Of String))
                Dim combo As EntidadesDTO.ComboDTO = NegocioYSeguridad.ComboBO.getInstance().obtenerComboPorId(comboId)
            
                insumosAsignados.Add(New List(Of String))
                insumosAsignados.Add(New List(Of String))
            
                If (insumosAsignadosAlCombo IsNot Nothing) Then
                    If (insumosAsignadosAlCombo.Contains(",")) Then
                        For Each insumoAsignado As String In insumosAsignadosAlCombo.Split(",")
                            Dim insumo As EntidadesDTO.InsumoDTO = NegocioYSeguridad.InsumoBO.getInstance().obtenerInsumoPorId(insumoAsignado)
                            insumosAsignados(0).Add(insumo.id.ToString())
                            insumosAsignados(1).Add(Request.Form("stock_" + insumo.id.ToString()))
                        Next
                    Else
                        Dim insumo As EntidadesDTO.InsumoDTO = NegocioYSeguridad.InsumoBO.getInstance().obtenerInsumoPorId(insumosAsignadosAlCombo)
                        insumosAsignados(0).Add(insumo.id.ToString())
                        insumosAsignados(1).Add(Request.Form("stock_" + insumo.id.ToString()))
                    End If
                End If
            
                combo.insumos = insumosAsignados
                
                Try
                    NegocioYSeguridad.ComboBO.getInstance().actualizarCombo(combo)
                    Response.Write("Exito! <a href='/Account/Combos.aspx'>Volver</a>")
                Catch ex As Exceptions.CandyException
                    Response.Write("Error! " + ex.Message + " <a href='/Account/Combos.aspx'>Volver</a>")
                End Try
            End If
        ElseIf (("post".Equals(Request.Form("action")) Or "put".Equals(Request.Form("action"))) Or ("post".Equals(Request.QueryString("action")) Or "put".Equals(Request.QueryString("action")))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim insumo As New EntidadesDTO.InsumoDTO()
                If ("put".Equals(Request.QueryString("action"))) Then
                    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
                    insumo = NegocioYSeguridad.InsumoBO.getInstance().obtenerInsumoPorId(id)
                End If
    %>
    <form action="Insumos.aspx" method="post">
        <input type="hidden" name="action" value='<%= IIf("put".Equals(Request.Form("action")) Or "put".Equals(Request.QueryString("action")), "put", "post")%>' />
        <%
            If (insumo IsNot Nothing) Then
                If (insumo.id <> 0) Then
                    Response.Write("<input type='hidden' name='id' value='" + insumo.id.ToString() + "' />")
                End If
            End If
        %>
        <table>
            <tr>
                <td>Insumo:</td>
                <td>
                    <input type="text" name="insumo" value='<%= IIf(insumo IsNot Nothing, insumo.nombre, "")%>' /></td>
            </tr>
            <tr>
                <td>Tipo:</td>
                <td>
                    <input type="text" name="tipo" value='<%= IIf(insumo IsNot Nothing, insumo.tipo, "")%>' /></td>
            </tr>
            <tr>
                <td>Precio unitario:</td>
                <td>
                    <input type="text" name="precioUnidad" value='<%= IIf(insumo IsNot Nothing, insumo.precioUnidad, "")%>' /></td>
            </tr>
            <tr>
                <td>Disponibles:</td>
                <td>
                    <input type="text" name="stock" value='<%= IIf(insumo IsNot Nothing, insumo.stock, "")%>' /></td>
            </tr>
            <tr>
                <td>
                    <input type="submit" name="guardarInsumo" value="Guardar" />
                    <a href='/Account/Insumos.aspx'>Volver</a></td>
            </tr>
        </table>
    </form>
    <%
    Else
        Dim insumo As New EntidadesDTO.InsumoDTO()
                
        insumo.nombre = Request.Form("insumo")
        insumo.tipo = Request.Form("tipo")
        insumo.precioUnidad = Request.Form("precioUnidad")
        insumo.stock = Request.Form("stock")
                
        Try
            If (Request.Form("id") IsNot Nothing) Then
                insumo.id = Integer.Parse(Request.Form("id"))
                NegocioYSeguridad.InsumoBO.getInstance().actualizarInsumo(insumo)
            Else
                NegocioYSeguridad.InsumoBO.getInstance().agregarInsumo(insumo)
            End If
            Response.Write("Exito! <a href='/Account/Insumos.aspx'>Volver</a>")
        Catch ex As Exceptions.CandyException
            Response.Write("Error! " + ex.Message + " <a href='/Account/Insumos.aspx'>Volver</a>")
        End Try
    End If
ElseIf ("delete".Equals(Request.QueryString("action"))) Then
    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
    Dim insumo As EntidadesDTO.InsumoDTO = NegocioYSeguridad.InsumoBO.getInstance().obtenerInsumoPorId(id)
    Try
        NegocioYSeguridad.InsumoBO.getInstance().eliminarInsumo(insumo)
        Response.Write("Exito! <a href='/Account/Insumos.aspx'>Volver</a>")
    Catch ex As Exceptions.CandyException
        Response.Write("Error! " + ex.Message + " <a href='/Account/Insumos.aspx'>Volver</a>")
    End Try
Else
        Dim insumos As Dictionary(Of String, EntidadesDTO.InsumoDTO) = NegocioYSeguridad.InsumoBO.getInstance().obtenerInsumos()
        Response.Write("<table>")
        Response.Write("<tr><td><b>INSUMO</b></td><td><b>TIPO</b></td><td><b>PRECIO UNITARIO</b></td><td><b>DISPONIBLES</b></td><td><b>ACCIONES(<a href='Insumos.aspx?action=post'>Nuevo</a>)</b></td></tr>")
        For Each insumo As EntidadesDTO.InsumoDTO In insumos.Values
            Response.Write("<tr>")
            Response.Write("<td>" + insumo.nombre.ToString() + "</td>")
            Response.Write("<td>" + insumo.tipo + "</td>")
            Response.Write("<td>" + insumo.precioUnidad + "</td>")
            Response.Write("<td>" + insumo.stock + "</td>")
            Response.Write("<td>" + "<a href='/Account/Insumos.aspx?action=delete&id=" + insumo.id.ToString() + "'>Borrar</a> " + "<a href='/Account/Insumos.aspx?action=put&id=" + insumo.id.ToString() + "'>Modificar</a> " + "</td>")
            Response.Write("</tr>")
        Next
        Response.Write("</table>")
End If
    %>
</asp:Content>

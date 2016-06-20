<%@ Page Title="Administracion de combos" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Combos.aspx.vb" Inherits="WebCandyBar.Combos" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        If (("post".Equals(Request.Form("action")) Or "put".Equals(Request.Form("action"))) Or ("post".Equals(Request.QueryString("action")) Or "put".Equals(Request.QueryString("action")))) Then
            If ("GET".Equals(Request.HttpMethod)) Then
                Dim combo As New EntidadesDTO.ComboDTO()
                If ("put".Equals(Request.QueryString("action"))) Then
                    Dim id As Integer = Integer.Parse(Request.QueryString("id"))
                    combo = NegocioYSeguridad.ComboBO.getInstance().obtenerComboPorId(id)
                End If
                Response.Write("<form action='Combos.aspx' method='post'>")
                If ("put".Equals(Request.Form("action")) Or "put".Equals(Request.QueryString("action"))) Then
                    Response.Write("<input type='hidden' name='action' value='put'/>")
                Else
                    Response.Write("<input type='hidden' name='action' value='post'/>")
                End If
                If (combo IsNot Nothing And combo.id <> 0) Then
                    Response.Write("<input type='hidden' name='id' value='" + combo.id.ToString() + "' />")
                End If
                Response.Write("<table>")
                Response.Write("<tr><td>Nombre:</td><td><input type='text' name='nombre' value='" + IIf(combo IsNot Nothing, combo.nombre, "") + "' /></td>")
                Response.Write("<tr><td>Precio Total:</td><td><input type='text' name='precio' value='" + IIf(combo IsNot Nothing, combo.precio, "") + "' /></td>")
                Response.Write("<tr><td><input type='submit' name='guardarCombo' value='Guardar' /> <a href='/Account/Combos.aspx'>Volver</a></td></tr>")
                Response.Write("</table>")
                Response.Write("</form>")
            Else
                Dim combo As New EntidadesDTO.ComboDTO()
                
                combo.nombre = Request.Form("nombre")
                combo.precio = Request.Form("precio")
                
                Try
                    If (Request.Form("id") IsNot Nothing) Then
                        combo.id = Integer.Parse(Request.Form("id"))
                        combo.insumos = NegocioYSeguridad.ComboBO.getInstance().obtenerInsumosPorComboId(combo.id)
                        NegocioYSeguridad.ComboBO.getInstance().actualizarCombo(combo)
                    Else
                        NegocioYSeguridad.ComboBO.getInstance().agregarCombo(combo)
                    End If
                    Response.Write("Exito! <a href='/Account/Combos.aspx'>Volver</a>")
                Catch ex As Exceptions.CandyException
                    Response.Write("Error! " + ex.Message + " <a href='/Account/Combos.aspx'>Volver</a>")
                End Try
            End If
        ElseIf ("delete".Equals(Request.QueryString("action"))) Then
            Dim id As Integer = Integer.Parse(Request.QueryString("id"))
            Dim comboABorrar As EntidadesDTO.ComboDTO = NegocioYSeguridad.ComboBO.getInstance().obtenerComboPorId(id)
            Try
                NegocioYSeguridad.ComboBO.getInstance().eliminarCombo(comboABorrar)
                Response.Write("Exito! <a href='/Account/Combos.aspx'>Volver</a>")
            Catch ex As Exceptions.CandyException
                Response.Write("Error! " + ex.Message + " <a href='/Account/Combos.aspx'>Volver</a>")
            End Try
        Else
            Dim combos As Dictionary(Of String, EntidadesDTO.ComboDTO) = NegocioYSeguridad.ComboBO.getInstance().obtenerCombos()
            Response.Write("<table>")
            Response.Write("<tr><td><b>NOMBRE</b></td><td><b>PRECIO</b></td><td><b>ACCIONES(<a href='Combos.aspx?action=post'>Nuevo</a>)</b></td></tr>")
            For Each combo As EntidadesDTO.ComboDTO In combos.Values
                Response.Write("<tr>")
                Response.Write("<td>" + combo.nombre + "</td>")
                Response.Write("<td>" + combo.precio + "</td>")
                Response.Write("<td>" + "<a href='/Account/Combos.aspx?action=delete&id=" + combo.id.ToString() + "'>Borrar</a> " + "<a href='/Account/Combos.aspx?action=put&id=" + combo.id.ToString() + "'>Modificar</a> " + "<a href='/Account/Insumos.aspx?action=combo&id=" + combo.id.ToString() + "'>Insumos</a> " + "</td>")
                Response.Write("</tr>")
            Next
            Response.Write("</table>")
        End If
    %>
</asp:Content>

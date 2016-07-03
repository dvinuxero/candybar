<%@ Page Title="Administracion para restaurar" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Restaurar.aspx.vb" Inherits="WebCandyBar.Restaurar" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        Dim directorio As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        If ("post".Equals(Request.Form("action"))) Then
            Try
                Dim restores As New List(Of String)
                Dim backupId As String = Request.Form("backup")
                restores.Add(directorio + "\" + backupId)
                NegocioYSeguridad.BackUpRestoreBO.getInstance().realizarRestore(restores)
                Response.Write("<div class='exito'>Exito! <a href='/Account/Restaurar.aspx'>Volver</a></div>")
            Catch ex As Exceptions.CandyException
                Response.Write("<div class='error'>Error! " + ex.Message + " <a href='/Account/Restaurar.aspx'>Volver</a></div>")
            End Try
        Else
            Dim listaRestores As String() = System.IO.Directory.GetFiles(directorio, "*.bkp")
            Dim sinRestores As Boolean = True
            
            Response.Write("<div class='info'>Al elegir una opcion de restauracion, la base de datos recuperara los datos hasta la fecha especifica del backup</div>")
            
            If (listaRestores IsNot Nothing) Then
                If (listaRestores.Count > 0) Then
                    sinRestores = False
                    Response.Write("<form action='Restaurar.aspx' method='post'>")
                    Response.Write("<input type='hidden' name='action' value='post' />")
                    Response.Write("<table>")
                    Dim index As Integer = 0
                    For Each restoreString As String In listaRestores
                        index += 1
                        Dim restoreId = restoreString.Substring(restoreString.LastIndexOf("\") + 1, restoreString.Length - (restoreString.LastIndexOf("\") + 1))
                        Response.Write("<tr>")
                        Response.Write("<td><input type='radio' id='backup_" + index.ToString() + "' name='backup' value='" + restoreId + "' ></td>")
                        Response.Write("<td><label for='backup_" + index.ToString() + "'>" + restoreId + "</label></td>")
                        Response.Write("</tr>")
                    Next
                    Response.Write("</table>")
                    Response.Write("<input type='submit' name='realizarRestore' value='Realizar Restauracion' />" + " <a href='/Default.aspx'>Volver</a>")
                    Response.Write("</form>")
                End If
            End If
            If (sinRestores) Then
                Response.Write("<div class='info'>No existen backups para restaurar <a href='/Default.aspx'>Volver</a></div>")
            End If
        End If
    %>
</asp:Content>

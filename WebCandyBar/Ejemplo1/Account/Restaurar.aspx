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
                Response.Write("Exito! <a href='/Account/Restaurar.aspx'>Volver</a>")
            Catch ex As Exceptions.CandyException
                Response.Write("Error! " + ex.Message + " <a href='/Account/Restaurar.aspx'>Volver</a>")
            End Try
        Else
            Dim listaRestores As String() = System.IO.Directory.GetFiles(directorio, "*.bkp")
            Dim sinRestores As Boolean = True
            
            If (listaRestores IsNot Nothing) Then
                If (listaRestores.Count > 0) Then
                    sinRestores = False
                    Response.Write("<form action='Restaurar.aspx' method='post'>")
                    Response.Write("<input type='hidden' name='action' value='post' />")
                    For Each restoreString As String In listaRestores
                        Dim restoreId = restoreString.Substring(restoreString.LastIndexOf("\") + 1, restoreString.Length - (restoreString.LastIndexOf("\") + 1))
                        Response.Write("<input type='radio' name='backup' value='" + restoreId + "' >" + restoreId + "<br>")
                    Next
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

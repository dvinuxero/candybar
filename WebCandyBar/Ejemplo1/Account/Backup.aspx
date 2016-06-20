<%@ Page Title="Administracion de backups" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Backup.aspx.vb" Inherits="WebCandyBar.Backup" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <%
        If ("post".Equals(Request.Form("action"))) Then
            Try
                Dim backups As New List(Of String)
                Dim directorio As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                Dim fecha As String = Date.Now.ToString
                fecha = fecha.Replace("/", "-").Replace(" ", "-").Replace(":","-").Replace(".", "")
                backups.Add(directorio + "\candy_" + fecha + "_backup.bkp")
                NegocioYSeguridad.BackUpRestoreBO.getInstance().realizarBackUp(backups)
                Response.Write("Exito! <a href='/Account/Backup.aspx'>Volver</a>")
            Catch ex As Exceptions.CandyException
                Response.Write("Error! " + ex.Message + " <a href='/Account/Backup.aspx'>Volver</a>")
            End Try
        Else
            
    %>
    <form action="Backup.aspx" method="post">
        <input type="hidden" name="action" value='post' />
        <input type="submit" name="realizarBackup" value="Realizar Backup" />
    </form>
    <%
    End If
    %>
</asp:Content>

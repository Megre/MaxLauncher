'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Windows.Forms

Namespace Utility

    NotInheritable Class AppNotifyIcon
        Implements IDisposable

        Private appName As String
        Private components As System.ComponentModel.IContainer
        Private contextMenu1 As System.Windows.Forms.ContextMenu

        Friend WithEvents notifyIcon1 As System.Windows.Forms.NotifyIcon
        Friend WithEvents menuItemExit As System.Windows.Forms.MenuItem
        Friend WithEvents menuItemShowApp As System.Windows.Forms.MenuItem

        Private Shared nIcon As AppNotifyIcon

        Friend Shared Function NotifyIcon() As AppNotifyIcon
            If (nIcon Is Nothing) Then nIcon = New AppNotifyIcon

            Return nIcon
        End Function

        Public Sub New()
            Me.appName = My.Application.Info.Title

            Me.components = New System.ComponentModel.Container
            Me.contextMenu1 = New System.Windows.Forms.ContextMenu
            Me.menuItemExit = New System.Windows.Forms.MenuItem(Localization.GetString("String_SystemTrayMenuExit"))
            Me.menuItemShowApp = New System.Windows.Forms.MenuItem(
            String.Format(Localization.GetString("String_SystemTrayMenuShow"), Me.appName))

            'Initialize contextMenu1 
            Me.contextMenu1.MenuItems.AddRange(
                    New System.Windows.Forms.MenuItem() {Me.menuItemShowApp,
                                                         New System.Windows.Forms.MenuItem("-"),
                                                         Me.menuItemExit})

            'Create NotifyIcon
            Me.notifyIcon1 = New System.Windows.Forms.NotifyIcon(Me.components)

            'Notify icon from application.
            notifyIcon1.Icon = Utility.Imaging.ApplicationIcon

            'Set context menu
            notifyIcon1.ContextMenu = contextMenu1

            'Set text to current application name.
            notifyIcon1.Text = Me.appName

            'Show notify icon.
            notifyIcon1.Visible = True

        End Sub

        Public Sub Dispose() Implements System.IDisposable.Dispose
            If (Me.contextMenu1 IsNot Nothing) Then Me.contextMenu1.Dispose()
            If (Me.components IsNot Nothing) Then Me.components.Dispose()
            If (Me.notifyIcon1 IsNot Nothing) Then Me.notifyIcon1.Dispose()
            If (Me.menuItemShowApp IsNot Nothing) Then Me.menuItemShowApp.Dispose()
            If (Me.menuItemExit IsNot Nothing) Then Me.menuItemExit.Dispose()

            nIcon = Nothing
            appName = Nothing
        End Sub

        Private Sub MenuItemExit_Click(Sender As Object, e As EventArgs) Handles menuItemExit.Click
            'Close the MainWindow, which closes the application.
            If (My.Application.MainWindow IsNot Nothing) Then DirectCast(My.Application.MainWindow, MainWindow).ExitApp
        End Sub

        Private Sub MenuItemShowApp_Click(Sender As Object, e As EventArgs) Handles menuItemShowApp.Click
            'Show MainWindow.
            ShowApp()
        End Sub

        Private Sub NotifyIcon1_Click(Sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles notifyIcon1.MouseClick
            'Show MainWindow on left click only.
            If ((e.Button And MouseButtons.Left) = MouseButtons.Left) Then ShowApp()
        End Sub

        Private Sub ShowApp()
            'Show MainWindow.
            My.Application.MainWindow.WindowState = Windows.WindowState.Normal
        End Sub

    End Class

End Namespace

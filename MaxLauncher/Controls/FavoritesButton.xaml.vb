'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.ComponentModel
Imports MaxLauncher.Utility

Class FavoritesButton
    Inherits MButton

    Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If Not DesignerProperties.GetIsInDesignMode(Me) Then
            If (ConfigManager.PortableConfig.FavoritesConfigFileRO) Then
                Me.ContextMenu = Nothing
            End If
        End If

    End Sub

    Overrides Function GetButtonData() As MLButtonData
        Dim btnRow = GetButtonRow()

        If btnRow Is Nothing Then
            Return Nothing
        Else
            Return New MLButtonData(Me.Scancode,
                        btnRow.Text, btnRow.Filename, btnRow.Arguments,
                        btnRow.WorkingDirectory, btnRow.Description,
                        btnRow.IconFile, btnRow.IconIndex, btnRow.WindowStyle,
                        btnRow.WindowStyleEx, btnRow.RunAsAdmin, btnRow.WindowTitle,
                        btnRow.WindowProcessName, btnRow.WindowRect, btnRow.GroupID)
        End If
    End Function

    Overrides Sub SetButtonData(btnData As MLButtonData)
        Dim btnRow = GetButtonRow()
        If btnRow IsNot Nothing Then btnRow.Delete()

        If btnData IsNot Nothing AndAlso
            Not String.IsNullOrEmpty(btnData.Filename) Then
            FavoritesBarData.GetInstance.Button.AddButtonRow(
                Me.Scancode,
                btnData.Text,
                btnData.IconFile,
                btnData.IconIndex,
                btnData.Filename,
                btnData.Arguments,
                btnData.WorkingDirectory,
                btnData.Description,
                btnData.WindowStyle,
                btnData.WindowStyleEx,
                btnData.RunAsAdmin,
                btnData.WindowTitle,
                btnData.WindowProcessName,
                btnData.GetRectangleString,
                btnData.GroupID)
        End If

        SaveConfig()
        UpdateUI(True)
    End Sub

    Function GetButtonRow() As FavoritesBarData.ButtonRow
        Return FavoritesBarData.GetInstance.Button.FindById(Me.Scancode)
    End Function

#Region "Button Events"

    Private Sub MButton_Loaded(sender As Object, e As RoutedEventArgs)
        If Not DesignerProperties.GetIsInDesignMode(Me) Then UpdateUI()
    End Sub

#End Region

#Region "ContextMenu Events"

    Private Sub MButton_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs)
        Dim btnRow = GetButtonRow()
        ContextMenu.IsOpen = True

        If (btnRow Is Nothing) Then
            openfilelocationContextMenuItem.IsEnabled = False
            cutContextMenuItem.IsEnabled = False
            copyContextMenuItem.IsEnabled = False
            deleteContextMenuItem.IsEnabled = False
        Else
            openfilelocationContextMenuItem.IsEnabled = True
            cutContextMenuItem.IsEnabled = True
            copyContextMenuItem.IsEnabled = True
            deleteContextMenuItem.IsEnabled = True

            'Enable Open file location menu item.
            If Not (String.IsNullOrEmpty(btnRow.Filename)) Then
                Dim folder As String = Nothing
                Try
                    Dim filename As String = FileUtils.Path.ExpandEnvironmentVariables(btnRow.Filename)
                    folder = System.IO.Path.GetDirectoryName(filename)
                Catch ex As Exception
                    'Ignore
                End Try

                If (My.Computer.FileSystem.DirectoryExists(folder)) Then openfilelocationContextMenuItem.IsEnabled = True
            End If
        End If

        'Enable/Disable Paste menu item.
        If Clipboard.ContainsData(MLButtonData.DataFormat) OrElse Clipboard.ContainsFileDropList() Then
            pasteContextMenuItem.IsEnabled = True
        Else
            pasteContextMenuItem.IsEnabled = False
        End If

        e.Handled = True
    End Sub

#End Region

    Sub SaveConfig()
        ConfigManager.SaveFavorites()
    End Sub

End Class

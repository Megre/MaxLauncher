'Copyright 2014-2015 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports IWshRuntimeLibrary
Imports System.Runtime.InteropServices
Imports System.ComponentModel
Imports MaxLauncher.Utility
Imports System.Globalization

Public MustInherit Class MButton
    Inherits System.Windows.Controls.Button

    Public Property Scancode As Integer

#Region "Dependency Properties"

    ''' <summary>
    ''' Keyboard key label
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Key As String
        Get
            Return GetValue(KeyProperty)
        End Get

        Set(ByVal value As String)
            SetValue(KeyProperty, value)
        End Set
    End Property

    Public Shared ReadOnly KeyProperty As DependencyProperty =
                           DependencyProperty.Register("Key",
                           GetType(String), GetType(MButton),
                           New PropertyMetadata(Nothing))

    Public Property Text As String
        Get
            Return GetValue(TextProperty)
        End Get

        Set(ByVal value As String)
            SetValue(TextProperty, value)
            Me.Content = value
        End Set
    End Property

    Public Shared ReadOnly TextProperty As DependencyProperty =
                           DependencyProperty.Register("Text",
                           GetType(String), GetType(MButton),
                           New PropertyMetadata(Nothing))

    Public Property Description As String
        Get
            Return GetValue(DescriptionProperty)
        End Get

        Set(ByVal value As String)
            SetValue(DescriptionProperty, value)
            Me.Content = value
        End Set
    End Property

    Public Shared ReadOnly DescriptionProperty As DependencyProperty =
                           DependencyProperty.Register("Description",
                           GetType(String), GetType(MButton),
                           New PropertyMetadata(Nothing))

    Public Property Image As ImageSource
        Get
            Return GetValue(ImageProperty)
        End Get

        Set(ByVal value As ImageSource)
            SetValue(ImageProperty, value)
        End Set
    End Property

    Public Shared ReadOnly ImageProperty As DependencyProperty =
                           DependencyProperty.Register("Image",
                           GetType(ImageSource), GetType(MButton),
                           New PropertyMetadata(Nothing))

#End Region

    MustOverride Function GetButtonData() As MLButtonData

    MustOverride Sub SetButtonData(value As MLButtonData)

    Protected Sub UpdateUI(Optional ByVal force As Boolean = False)
        If Not DesignerProperties.GetIsInDesignMode(Me) Then
            Dim btnData = GetButtonData()

            Try
                If btnData Is Nothing Then
                    Me.Text = ""
                    Me.Description = ""
                    Me.Image = Nothing
                Else
                    Me.Text = btnData.Text
                    Me.Description = btnData.Description

                    If (String.IsNullOrEmpty(btnData.IconFile)) Then
                        Me.Image = IconCacheDB.GetInstance.GetImage(btnData.Filename, forceUpdate:=force)
                    Else
                        Me.Image = IconCacheDB.GetInstance.GetImage(btnData.IconFile, btnData.IconIndex, force)
                    End If
                End If
            Catch ex As Exception
                'Ignore.
            End Try
        End If
    End Sub

    Protected Overridable Sub SwapWith(sourceButton As MButton)
        Dim buttonDataTemp = GetButtonData()

        Me.SetButtonData(sourceButton.GetButtonData)
        sourceButton.SetButtonData(buttonDataTemp)
    End Sub

    Protected Overridable Function ReplaceData() As Boolean
        Dim retValue = True

        If (GetButtonData() IsNot Nothing) Then
            If (MessageBoxML.Show(String.Format(Localization.GetString("String_ReplaceButton"), Me.Key, Me.Text),
                                  Localization.GetString("String_Replace"), MsgBoxStyle.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No) <> MsgBoxResult.Yes) Then retValue = False
        End If

        Return retValue
    End Function

    Shared Function CreateData(filename As String) As MLButtonData

        Dim buttonData As New MLButtonData

        If FileUtils.IsShortcut(filename) Then
            Dim wsh As New IWshShell_Class
            Dim sc As IWshShortcut = CType(wsh.CreateShortcut(filename), IWshShortcut)

            'Parse icon for links
            Dim iconArray As String() = Split(sc.IconLocation, ",")

            buttonData.IconFile = iconArray(0)
            buttonData.IconIndex = 0

            Try
                buttonData.IconIndex = Integer.Parse(iconArray(1))
            Catch ex As Exception
            End Try

            'Process window style. IWshShortcut enums are different from ProcessWindowStyle enums
            'Check Shell32.dll/ShellExecute/ShowCommands enum
            buttonData.WindowStyle = ProcessWindowStyle.Normal

            Select Case sc.WindowStyle
                Case IWshRuntimeLibrary.WshWindowStyle.WshHide
                    buttonData.WindowStyle = ProcessWindowStyle.Hidden
                Case IWshRuntimeLibrary.WshWindowStyle.WshNormalFocus
                    buttonData.WindowStyle = ProcessWindowStyle.Normal
                Case IWshRuntimeLibrary.WshWindowStyle.WshMinimizedFocus
                    buttonData.WindowStyle = ProcessWindowStyle.Minimized
                Case 7 'SW_SHOWMINNOACTIVE
                    buttonData.WindowStyle = ProcessWindowStyle.Minimized
                Case IWshRuntimeLibrary.WshWindowStyle.WshMaximizedFocus
                    buttonData.WindowStyle = ProcessWindowStyle.Maximized
                Case Else
                    buttonData.WindowStyle = ProcessWindowStyle.Normal
            End Select

            If (IO.Directory.Exists(Environment.ExpandEnvironmentVariables(sc.TargetPath))) Then
                buttonData.Text = System.IO.Path.GetFileNameWithoutExtension(filename)
                'buttonData.Filename = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%"), "explorer.exe")
                buttonData.Filename = System.IO.Path.Combine("%windir%", "explorer.exe")
                buttonData.Arguments = sc.TargetPath
                buttonData.WorkingDirectory = sc.WorkingDirectory
            Else
                If String.IsNullOrEmpty(sc.TargetPath) Then
                    'Handle special shortcuts. Ex. Metro Apps, Control Panel items and Advertised links.

                    'Check if file exists.
                    Dim name As String = System.IO.Path.GetFileName(filename)
                    Dim destination As String = System.IO.Path.Combine(ConfigManager.PortableConfig.ShortcutsDirectory, name)
                    'Prompt to repace file if it exists.
                    If IO.File.Exists(destination) Then
                        If (MessageBoxML.Show(String.Format(Localization.GetString("String_FileExistsReplace"), destination),
                                              Localization.GetString("String_Replace"), MsgBoxStyle.YesNo,
                            MessageBoxImage.Question, MessageBoxResult.No) = MsgBoxResult.No) Then Return Nothing
                    End If

                    'Copy file to Shortcuts directory and set button.
                    Try
                        IO.File.Copy(filename, destination, True)
                        buttonData.Text = System.IO.Path.GetFileNameWithoutExtension(destination)
                        buttonData.Filename = destination
                    Catch ex As Exception
                        MessageBoxML.Show(ex.Message & " " & Localization.GetString("String_FileReplaceError"),
                                        Localization.GetString("String_Replace"),
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
                        Return Nothing
                    End Try

                Else
                    'Handle regular shortcuts.
                    buttonData.Text = System.IO.Path.GetFileNameWithoutExtension(filename)
                    buttonData.Filename = sc.TargetPath
                    'buttonData.Description = sc.Description 'sc.Description = Comment field of shortcut.
                    buttonData.Arguments = sc.Arguments
                    buttonData.WorkingDirectory = sc.WorkingDirectory
                End If
            End If
        Else
            'Process direct file or directory drop.
            If (IO.Directory.Exists(filename)) Then
                'Process directory drop.
                Dim actualFilename As String = My.Computer.FileSystem.GetName(filename)

                'Handle root directory
                If (String.IsNullOrEmpty(actualFilename)) Then actualFilename = filename

                buttonData.Text = actualFilename
                'buttonData.Filename = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%"), "explorer.exe")
                buttonData.Filename = System.IO.Path.Combine("%windir%", "explorer.exe")
                buttonData.Arguments = filename
                buttonData.WorkingDirectory = String.Empty
            Else
                'Process file drop.
                buttonData.Text = IO.Path.GetFileNameWithoutExtension(filename)
                buttonData.Filename = filename
                buttonData.Arguments = String.Empty
                buttonData.WorkingDirectory = System.IO.Path.GetDirectoryName(filename)
            End If
        End If

        'Convert paths to relative if in portable mode.
        buttonData.Filename = FileUtils.Path.ConvertToRelativePath(buttonData.Filename)
        buttonData.Arguments = FileUtils.Path.ConvertToRelativePath(buttonData.Arguments)
        buttonData.WorkingDirectory = FileUtils.Path.ConvertToRelativePath(buttonData.WorkingDirectory)

        Return buttonData
    End Function


#Region "Events"

    Private Sub MButton_Click(sender As Object, e As RoutedEventArgs)
        If Not ConfigManager.AppConfig.MouseDoubleClick Then
            LaunchFile()
        End If
    End Sub

    Private Sub MButton_PreviewMouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If ConfigManager.AppConfig.MouseDoubleClick Then
            LaunchFile()
            e.Handled = True
        End If
    End Sub

    Private Sub MButton_Drop(sender As Object, e As DragEventArgs)
        'Debug.WriteLine("MButton_Drop | " & e.OriginalSource.ToString & " | " & System.DateTime.Now.ToString())
        Try
            If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
                'Ask to replace.
                If ReplaceData() Then
                    Dim filenames As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())
                    Dim filename As String = filenames(0)

                    Dim btnData = MButton.CreateData(filename)
                    If btnData IsNot Nothing Then Me.SetButtonData(btnData)
                End If
            ElseIf e.Data.GetDataPresent("DragSource") Then
                Dim button As MButton = Nothing

                'Check if DragSource is from another instance.
                Try
                    button = e.Data.GetData("DragSource")
                Catch ex As Exception
                    'Ignore.
                End Try

                'DragSource is from another instance.
                If button Is Nothing Then

                    'Ask to replace.
                    My.Application.MainWindow.Activate() 'Activate app so that the message box will be shown.
                    If ReplaceData() Then Me.SetButtonData(New MLButtonData(e.Data.GetData("MLButtonData")))
                Else
                    If Not (Me.Equals(button)) AndAlso
                            ((e.KeyStates And DragDropKeyStates.ControlKey) = DragDropKeyStates.ControlKey) Then

                        'Ask to replace.
                        If ReplaceData() Then Me.SetButtonData(New MLButtonData(e.Data.GetData("MLButtonData")))
                    Else
                        Me.SwapWith(button)
                        'Debug.WriteLine("MButton_Drop | SwapWith() | " & System.DateTime.Now.ToString())
                    End If
                End If
            End If
            e.Handled = True
        Catch exc As COMException
            'Ignore drop of Modern Apps.
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_ButtonDragAndDrop"), MessageBoxButton.OK,
                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    Private Sub MButton_DragOver(sender As Object, e As DragEventArgs)
        If Not Application.Current.MainWindow.IsActive Then Application.Current.MainWindow.Activate()

        Try
            e.Effects = DragDropEffects.None
            If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
                'Check if object has a targetpath/target filename. This prevents special links from being dropped since
                'they do not contain target filenames(eg. Control Panel shortcuts)
                Dim filenames As String() = TryCast(e.Data.GetData(DataFormats.FileDrop), String())

                If filenames Is Nothing Then
                    e.Handled = True
                    Return
                End If

                e.Effects = DragDropEffects.Copy
            ElseIf e.Data.GetDataPresent("DragSource") Then
                'If DragSource is from another instance, an exception will be thrown and button = Nothing.
                Dim button As MButton = Nothing
                Try
                    button = e.Data.GetData("DragSource")
                Catch ex As Exception
                    'Ignore.
                End Try

                If button Is Nothing Then
                    If e.Data.GetDataPresent("MLButtonData") Then e.Effects = DragDropEffects.Copy
                Else
                    'DragSource is from within this AppDomain.
                    If (Me.Equals(button)) Then
                        e.Effects = DragDropEffects.None
                    Else
                        If ((e.KeyStates And DragDropKeyStates.ControlKey) = DragDropKeyStates.ControlKey) Then
                            e.Effects = DragDropEffects.Copy
                        Else
                            e.Effects = DragDropEffects.Move
                        End If
                    End If
                End If
            End If
            e.Handled = True
        Catch exc As COMException
            'Ignore drop of Modern Apps.
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_ButtonDragAndDrop"), MessageBoxButton.OK,
                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    Private Sub MButton_PreviewMouseMove(sender As Object, e As MouseEventArgs)
        If ConfigManager.PortableConfig.TabControlDataRO OrElse
            ConfigManager.AppConfig.LockGUI OrElse
            Not Me.IsPressed Then Return

        If (e.LeftButton = MouseButtonState.Pressed) Then
            Try
                Dim dataObj = New DataObject("DragSource", Me)
                Dim btnData = GetButtonData()
                If btnData Is Nothing Then
                    dataObj.SetData("MLButtonData", New MLButtonData)
                Else
                    dataObj.SetData("MLButtonData", GetButtonData())
                End If

                DragDrop.DoDragDrop(Me, dataObj, DragDropEffects.Copy + DragDropEffects.Move + DragDropEffects.None)
            Catch ex As Exception
                MessageBoxML.Show(ex.Message, Localization.GetString("String_ButtonDragAndDrop"), MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            End Try
        End If
    End Sub
#End Region

    Sub LaunchFile(Optional ByVal GroupLaunch As Boolean = False)
        Me.Focus()

        Launcher.LaunchFile(GetButtonData)
    End Sub

#Region "ContextMenu Events"

    Protected Sub OpenfilelocationContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim btnData = GetButtonData()

        'Return if filename is empty.
        If String.IsNullOrEmpty(btnData.Filename) Then Return

        Dim folder As String = FileUtils.Path.ExpandEnvironmentVariables(btnData.Filename)

        Try
            folder = System.IO.Path.GetDirectoryName(folder)
            Process.Start(folder)
        Catch ex As Exception
            MessageBoxML.Show(String.Format(Localization.GetString("String_UnableToOpenFolder"), folder),
                              Localization.GetString("String_OpenFileLocation"),
                              MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK,
                              String.Format(Localization.GetString("String_UnableToOpenFolder") & ". {1}",
                                            folder, vbCrLf & ex.ToString))
        End Try
    End Sub

    Protected Sub EditContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim editDlg As New EditButtonDialog(GetButtonData)

        Try
            With editDlg
                .Owner = My.Application.MainWindow
                .ShowDialog()

                If (.DialogResult) Then SetButtonData(.ButtonData)
            End With
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_Edit"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    Protected Sub CutContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim btnData = GetButtonData()

        If (btnData IsNot Nothing) Then
            Try
                Clipboard.SetData(MLButtonData.DataFormat, btnData)
                SetButtonData(Nothing)
                SetVisibility()
            Catch ex As Exception
                MessageBoxML.Show(ex.Message, Localization.GetString("String_Cut"), MessageBoxButton.OK,
                                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            End Try
        End If
    End Sub

    Protected Sub CopyContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim btnData = GetButtonData()

        If (btnData IsNot Nothing) Then
            Try
                Clipboard.SetData(MLButtonData.DataFormat, btnData)
            Catch ex As Exception
                MessageBoxML.Show(ex.Message, Localization.GetString("String_Copy"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            End Try
        End If
    End Sub

    Protected Sub PasteContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim btnData = GetButtonData()

            'Ask to replace.
            If (btnData IsNot Nothing) Then
                If Not ReplaceData() Then Return
            End If

            If (Clipboard.ContainsData(MLButtonData.DataFormat)) Then
                Dim bd As MLButtonData = CType(Clipboard.GetData(MLButtonData.DataFormat), MLButtonData)
                SetButtonData(bd)
            ElseIf (Clipboard.ContainsFileDropList()) Then
                Dim filenames As String() = DirectCast(Clipboard.GetData(DataFormats.FileDrop), String())
                Dim filename As String = filenames(0)

                SetButtonData(MButton.CreateData(filename))
            End If
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_Paste"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    Protected Sub DeleteContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Try
            'Prompt confirmation
            If (MessageBoxML.Show(String.Format(Localization.GetString("String_DeleteButton"), Me.Key, Me.Text),
                                  Localization.GetString("String_Delete"), MsgBoxStyle.YesNo,
              MessageBoxImage.Question, MessageBoxResult.No) <> MsgBoxResult.Yes) Then Return

            SetButtonData(Nothing)
            SetVisibility()
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_Delete"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

#End Region

    Overridable Sub SetVisibility()
    End Sub

End Class

Public Class ExpandEnvironmentVariablesConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value IsNot Nothing Then Return FileUtils.Path.ExpandEnvironmentVariables(value.ToString)

        Return ""
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function

End Class

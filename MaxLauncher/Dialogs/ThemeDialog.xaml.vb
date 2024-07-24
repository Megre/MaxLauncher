'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports MaxLauncher.Utility

Class ThemeDialog

    Friend Const DefaultThemeName = "Default"
    Friend Const MainStyleXAML = "MainStyle.xaml"
    Friend Const CustomXAML = "Custom.xaml"
    Friend Const DefaultXAML = "Default.xaml"
    Friend Const MainStyleRefXAML = "MainStyle.ref"
    Friend Const BasicTheme = "Basic"
    Friend Const DetailedTheme = "Detailed"
    Friend Const PREVENT_CUSTOMIZE = ".nc"

    Public Shared IsCustomizeWindowOpen As Boolean = False
    Private IsListBoxItemSelected As Boolean = False

    Public Property SelectedTheme As Theme

    Sub New(theme As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.SelectedTheme = New Theme(theme)
    End Sub

#Region "Events"
    Private Sub ThemeDialog_Initialized(sender As Object, e As EventArgs)
        Me.Icon = Imaging.GetAppIconImage
    End Sub

    Private Sub ThemeDialog_Loaded(sender As Object, e As RoutedEventArgs)
        Me.MinWidth = Me.ActualWidth
        Me.MinHeight = Me.ActualHeight

        AddHandler themeListBox.ItemContainerGenerator.StatusChanged,
            AddressOf ItemContainerGenerator_StatusChanged

        ThemeListBoxRefreshItems()
        IsListBoxItemSelected = True
        themeListBox.SelectedItem = Me.SelectedTheme.ThemeName

    End Sub

    Private Sub ThemeListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If (Not themeListBox.HasItems) OrElse (Not IsListBoxItemSelected) Then Return

        Try
            Dim selectedThemeName = CStr(themeListBox.SelectedItem)

            ApplyTheme(selectedThemeName)
            Me.SelectedTheme = New Theme(selectedThemeName)
            UpdateButtons()

        Catch ex As Exception
            MessageBoxML.Show(ex.Message, "Theme", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)

            'Use default on failure.
            themeListBox.SelectedItem = ThemeDialog.DefaultThemeName
            Dim item As ListBoxItem = themeListBox.ItemContainerGenerator.ContainerFromIndex(themeListBox.SelectedIndex)
            If (item IsNot Nothing) Then item.Focus()
            ApplyTheme(CStr(themeListBox.SelectedItem))
            Me.SelectedTheme = New Theme(ThemeDialog.DefaultThemeName)
            UpdateButtons()
        End Try
    End Sub

    Private Sub ItemContainerGenerator_StatusChanged(sender As Object, e As EventArgs)
        If (themeListBox.ItemContainerGenerator.Status = System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated) Then
            Dim index = themeListBox.SelectedIndex

            Dim item As ListBoxItem = Nothing
            If (index >= 0) Then
                item = themeListBox.ItemContainerGenerator.ContainerFromIndex(index)
            End If

            If (item IsNot Nothing) Then
                item.Focus()
                IsListBoxItemSelected = True
            End If
        End If
    End Sub

    Private Sub AddButton_Click(sender As Object, e As RoutedEventArgs)
        'Get theme_name.zip
        Try
            Dim openFile = FileUtils.GetFilename(Me,
                                                 IO.FileMode.Open,
                                                 Localization.GetString("String_AddTheme"),
                                                 "",
                                                 "Theme Files|*.zip")
            If Not (String.IsNullOrEmpty(openFile)) Then
                UnZipTheme(ConfigManager.PortableConfig.ThemesDirectory, openFile)

                ThemeListBoxRefreshItems()

                themeListBox.SelectedItem = System.IO.Path.GetFileNameWithoutExtension(openFile)
            End If
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_AddTheme"), MessageBoxButton.OK,
                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try

    End Sub

    Private Sub DeleteButton_Click(sender As Object, e As RoutedEventArgs)
        Dim selectedTheme As String = themeListBox.SelectedItem

        If (String.IsNullOrEmpty(selectedTheme)) Then Return

        Dim sourceDirectory As String = System.IO.Path.Combine(ConfigManager.PortableConfig.ThemesDirectory, selectedTheme)

        'Confirm deletion.
        If (MessageBoxML.Show(Me,
                              String.Format(Localization.GetString("String_DeleteSelectedTheme"), selectedTheme),
                              Localization.GetString("String_DeleteTheme"), MsgBoxStyle.YesNo,
                              MessageBoxImage.Question, MessageBoxResult.No) = MsgBoxResult.Yes) Then

            Try
                My.Computer.FileSystem.DeleteDirectory(sourceDirectory, FileIO.DeleteDirectoryOption.DeleteAllContents)

                ThemeListBoxRefreshItems()

                'Change theme to Default.
                themeListBox.SelectedItem = ThemeDialog.DefaultThemeName

                themeListBox.Focus()
            Catch ex As Exception
                MessageBoxML.Show(ex.Message, Localization.GetString("String_DeleteTheme"), MessageBoxButton.OK,
                 MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            End Try
        End If

    End Sub

    Private Sub CopyButton_Click(sender As Object, e As RoutedEventArgs)
        Dim selectedTheme As String = themeListBox.SelectedItem

        If (String.IsNullOrEmpty(selectedTheme)) Then Return

        Dim sourceDirectory As String = System.IO.Path.Combine(ConfigManager.PortableConfig.ThemesDirectory, selectedTheme)
        Dim oldValue As String = Nothing

        While True
            Dim inputDialog As New InputDialog(Localization.GetString("String_CopyTheme"),
                                               Localization.GetString("String_ThemeName"),
                                               selectedTheme & " - Copy") With {
                .Owner = Me
            }
            If (oldValue IsNot Nothing) Then inputDialog.Value = oldValue
            If (inputDialog.ShowDialog()) Then
                oldValue = inputDialog.Value
                If (String.IsNullOrEmpty(inputDialog.Value)) Then
                    MessageBoxML.Show(Localization.GetString("String_PleaseEnterAName"),
                                      Localization.GetString("String_CopyTheme"), MessageBoxButton.OK,
                        MessageBoxImage.Information, MessageBoxResult.OK)
                    Continue While
                End If

                Dim destDirectory As String = Nothing

                Try
                    destDirectory = System.IO.Path.Combine(ConfigManager.PortableConfig.ThemesDirectory, inputDialog.Value)
                Catch ex As Exception
                    MessageBoxML.Show(ex.Message, Localization.GetString("String_CopyTheme"), MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
                End Try

                If (destDirectory IsNot Nothing) Then
                    'Check if directory exists.
                    If (My.Computer.FileSystem.DirectoryExists(destDirectory)) Then
                        MessageBoxML.Show(String.Format(Localization.GetString("String_DirExistsChooseAnother"), inputDialog.Value),
                                            Localization.GetString("String_CopyTheme"), MessageBoxButton.OK,
                                            MessageBoxImage.Error, MessageBoxResult.OK)
                        Continue While
                    Else
                        'Copy directory if everything is okay.
                        If selectedTheme.Equals(BasicTheme) OrElse selectedTheme.Equals(DetailedTheme) Then
                            My.Computer.FileSystem.CreateDirectory(destDirectory)
                            My.Computer.FileSystem.CopyFile(IO.Path.Combine(sourceDirectory, MainStyleRefXAML),
                                                        IO.Path.Combine(destDirectory, MainStyleXAML))
                            If IO.File.Exists(IO.Path.Combine(sourceDirectory, CustomXAML)) Then
                                My.Computer.FileSystem.CopyFile(IO.Path.Combine(sourceDirectory, CustomXAML),
                                                        IO.Path.Combine(destDirectory, CustomXAML))
                            End If
                        Else
                            My.Computer.FileSystem.CopyDirectory(sourceDirectory, destDirectory)
                            My.Computer.FileSystem.DeleteFile(System.IO.Path.Combine(destDirectory, PREVENT_CUSTOMIZE))
                        End If

                        ThemeListBoxRefreshItems()
                        themeListBox.SelectedItem = inputDialog.Value
                        Exit While
                    End If
                Else
                    Continue While
                End If
            Else
                Exit While
            End If
        End While
    End Sub

    Private Sub ResetButton_Click(sender As Object, e As RoutedEventArgs)
        If Me.SelectedTheme Is Nothing Then Return

        If (IO.File.Exists(Me.SelectedTheme.CustomFile)) Then
            'Ask to confirm.
            If (MessageBoxML.Show(Localization.GetString("String_UseDefaultsForThisTheme"),
                                  Localization.GetString("String_ResetTheme"), MsgBoxStyle.YesNo,
                                  MessageBoxImage.Question, MessageBoxResult.No) <> MsgBoxResult.Yes) Then
                Return
            Else
                Try
                    My.Computer.FileSystem.DeleteFile(Me.SelectedTheme.CustomFile)
                    UpdateButtons()
                Catch ex As Exception
                    MessageBoxML.Show(Localization.GetString("String_UnableToResetTheme"),
                                  Localization.GetString("String_ResetTheme"), MessageBoxButton.OK,
                                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
                End Try
                Try
                    ApplyTheme(CStr(themeListBox.SelectedItem))
                Catch ex As Exception
                    MessageBoxML.Show(ex.Message, Localization.GetString("String_ResetTheme"), MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
                End Try
            End If
        End If
    End Sub

    Private Sub CustomizeButton_Click(sender As Object, e As RoutedEventArgs)
        ThemeDialog.IsCustomizeWindowOpen = True

        'Create custom xaml file if it doesn't exist.
        If Not (IO.File.Exists(SelectedTheme.CustomFile)) Then
            Try
                Dim temp = SelectedTheme.DefaultFile

                My.Computer.FileSystem.CopyFile(SelectedTheme.DefaultFile, SelectedTheme.CustomFile, True)
                UpdateButtons()
            Catch ex As Exception
                MessageBoxML.Show(Localization.GetString("String_ErrorCreatingCustomFile"),
                                  Localization.GetString("String_CustomTheme"), MessageBoxButton.OK,
                                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
                Return
            End Try
        End If

        'Show property dialog.
        Dim pg As New PropertyEditorDialog(Me.SelectedTheme) With {
            .Owner = Application.Current.MainWindow
        }
        pg.Show()

        Me.Close()
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

#End Region

    Private Sub UnZipTheme(ByVal outputDirectory As String, ByVal inputZip As String)
        Dim shObj As Object = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"))

        IO.Directory.CreateDirectory(outputDirectory)

        Dim output As Object = shObj.NameSpace((outputDirectory))

        Dim input As Object = shObj.NameSpace((inputZip))

        output.CopyHere((input.Items), 4)
    End Sub

    Private Sub ThemeListBoxRefreshItems()
        themeListBox.Items.Clear()
        themeListBox.Items.Add(ThemeDialog.DefaultThemeName)

        If Not (System.IO.Directory.Exists(ConfigManager.PortableConfig.ThemesDirectory)) Then
            Return
        Else
            Dim sortedDirs = System.IO.Directory.GetDirectories(ConfigManager.PortableConfig.ThemesDirectory)
            Array.Sort(sortedDirs)
            For Each d As String In sortedDirs
                If (System.IO.File.Exists(System.IO.Path.Combine(d, ThemeDialog.MainStyleXAML))) Then
                    themeListBox.Items.Add(System.IO.Path.GetFileName(d))
                End If
            Next
        End If
    End Sub

    Private Sub UpdateButtons()
        deleteButton.IsEnabled = Not (CStr(themeListBox.SelectedItem).Equals(ThemeDialog.DefaultThemeName) OrElse
                CStr(themeListBox.SelectedItem).Equals(ThemeDialog.BasicTheme) OrElse CStr(themeListBox.SelectedItem).Equals(ThemeDialog.DetailedTheme))

        copyButton.IsEnabled = Not CStr(themeListBox.SelectedItem).Equals(ThemeDialog.DefaultThemeName)

        'Enable/Disable Customize button.
        customizeButton.IsEnabled = SelectedTheme.IsCustomizable

        If SelectedTheme.IsCustomizable Then
            'Enable/Disable Reset button.
            resetButton.IsEnabled = System.IO.File.Exists(SelectedTheme.CustomFile)
        Else
            resetButton.IsEnabled = False
        End If
    End Sub

    Friend Shared Sub ApplyTheme(ByVal themeName As String)
        Try
            Select Case themeName
                Case ThemeDialog.DefaultThemeName
                    ClearCustomThemes()
                Case Else
                    Dim themePath As String = System.IO.Path.Combine(ConfigManager.PortableConfig.ThemesDirectory, themeName)
                    Dim mainThemeFile = System.IO.Path.Combine(themePath, ThemeDialog.MainStyleXAML)
                    Dim mainRD As ResourceDictionary = New ResourceDictionary With {
                        .Source = New Uri(mainThemeFile, UriKind.Absolute)
                    }

                    'Load custom.xaml if it exists.
                    Dim customFile = System.IO.Path.Combine(themePath, ThemeDialog.CustomXAML)

                    If (System.IO.File.Exists(customFile)) Then
                        Dim customRD As ResourceDictionary = New ResourceDictionary With {
                            .Source = New Uri(customFile, UriKind.Absolute)
                        }
                        mainRD.MergedDictionaries.Add(customRD)
                    End If

                    'Set current resource dictionary.
                    ClearCustomThemes()
                    Application.Current.Resources.MergedDictionaries.Add(mainRD)
            End Select

        Catch ex As Exception
            Throw New Exception(String.Format(Localization.GetString("String_ErrorApplyingTheme"), themeName), ex)
        End Try
    End Sub

    Friend Shared Sub ClearCustomThemes()
        Dim themePathURI As Uri = New Uri(ConfigManager.PortableConfig.ThemesDirectory)

        Dim rdList As New List(Of ResourceDictionary)

        For Each rd As ResourceDictionary In Application.Current.Resources.MergedDictionaries
            If rd.Source = Nothing OrElse rd.Source.ToString.StartsWith(themePathURI.ToString, True, Nothing) Then
                rdList.Add(rd)
            End If
        Next

        For Each rd As ResourceDictionary In rdList
            Application.Current.Resources.MergedDictionaries.Remove(rd)
        Next
    End Sub

    Public Class Theme
        Public ReadOnly ThemeName As String

        Public ReadOnly MainFile As String

        Public ReadOnly DefaultFile As String

        Public ReadOnly CustomFile As String
        Public Property ThemeDirectory As String

        Sub New(theme As String)
            Me.ThemeName = theme

            If theme.Equals(ThemeDialog.DefaultThemeName) Then
                MainFile = String.Empty
                DefaultFile = String.Empty
                CustomFile = String.Empty
            Else
                ThemeDirectory = System.IO.Path.Combine(ConfigManager.PortableConfig.ThemesDirectory, theme)
                MainFile = System.IO.Path.Combine(ThemeDirectory, ThemeDialog.MainStyleXAML)
                CustomFile = System.IO.Path.Combine(ThemeDirectory, ThemeDialog.CustomXAML)

                Dim mainFileRD = New ResourceDictionary With {
                    .Source = New Uri(MainFile)
                }

                DefaultFile = GetDefaultFile(mainFileRD)
            End If
        End Sub

        Function IsCustomizable() As Boolean
            If Me.ThemeName.Equals(ThemeDialog.DefaultThemeName) Then Return False

            Dim preventCustFile = System.IO.Path.Combine(ThemeDirectory, PREVENT_CUSTOMIZE)
            If System.IO.File.Exists(preventCustFile) Then Return False

            If String.IsNullOrEmpty(DefaultFile) Then
                Return False
            Else
                Return True
            End If
        End Function

        Private Function GetDefaultFile(mainStyleRD As ResourceDictionary) As String

            For Each rd In mainStyleRD.MergedDictionaries
                If rd.Source.ToString.Equals(ThemeDialog.DefaultXAML) Then

                    If mainStyleRD.Source.IsAbsoluteUri Then
                        Dim themeDirectory = System.IO.Path.GetDirectoryName(mainStyleRD.Source.LocalPath)
                        Return System.IO.Path.Combine(themeDirectory, ThemeDialog.DefaultXAML)
                    Else
                        Dim relativeDefaultFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(mainStyleRD.Source.ToString), ThemeDialog.DefaultXAML)

                        Dim defaultFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(MainFile), relativeDefaultFilePath)
                        Dim fpath = System.IO.Path.GetFullPath(defaultFile)

                        Return System.IO.Path.GetFullPath(defaultFile)
                    End If
                Else
                    Return GetDefaultFile(rd)
                End If
            Next

            Return String.Empty
        End Function

    End Class

End Class

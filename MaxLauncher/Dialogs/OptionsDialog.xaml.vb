'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports MaxLauncher.Utility

Class OptionsDialog

    Public Sub New()
        Me.DataContext = ConfigManager.AppConfig

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Private Sub OptionsDialog_Initialized(sender As Object, e As EventArgs)
        Me.Icon = Imaging.GetAppIconImage
    End Sub

    Private Sub LanguageComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        'Apply language.
        Try
            MaxLauncher.Localization.ApplyLanguage(languageComboBox.SelectedItem)
            ConfigManager.AppConfig.Language = languageComboBox.SelectedItem
        Catch ex As Exception
            ConfigManager.AppConfig.Language = Localization.DefaultLanguage
            languageComboBox.SelectedItem = Localization.DefaultLanguage
            MaxLauncher.Localization.ClearCustomLanguage()
            MessageBoxML.Show(ex.Message, Localization.GetString("String_Language"), MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    Private Sub LanguageComboBox_Loaded(sender As Object, e As RoutedEventArgs)
        'Load languages into combo box.
        languageComboBox.Items.Add(Localization.DefaultLanguage)

        If Not (System.IO.Directory.Exists(ConfigManager.PortableConfig.LanguageDirectory)) Then
            Return
        Else
            For Each f As String In System.IO.Directory.GetFiles(ConfigManager.PortableConfig.LanguageDirectory)
                If System.IO.Path.GetExtension(f).ToLower.Equals(".xaml") Then
                    Dim langName As String = System.IO.Path.GetFileNameWithoutExtension(f)
                    languageComboBox.Items.Add(langName)
                    If (String.Compare(langName, ConfigManager.AppConfig.Language, True) = 0) Then
                        languageComboBox.SelectedItem = langName
                    End If
                End If
            Next
        End If

        If languageComboBox.SelectedIndex = -1 Then
            languageComboBox.SelectedItem = Localization.DefaultLanguage
        End If
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
End Class

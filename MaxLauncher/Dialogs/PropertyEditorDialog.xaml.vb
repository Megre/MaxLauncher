'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.IO
Imports System.Windows.Markup
Imports MaxLauncher.Utility

Class PropertyEditorDialog
    Private ReadOnly customRD As New ResourceDictionary
    Private ReadOnly defaultRD As New ResourceDictionary

    Friend HasChanges As Boolean = False

    Private SelectedTheme As ThemeDialog.Theme

    Public Sub New(ByVal theme As ThemeDialog.Theme)
        InitializeComponent()

        SelectedTheme = theme

        customRD.Source = New Uri(SelectedTheme.CustomFile, UriKind.Absolute)
        defaultRD.Source = New Uri(SelectedTheme.DefaultFile, UriKind.Absolute)
    End Sub

    Private Sub PropertyEditorDialog_Loaded(sender As Object, e As RoutedEventArgs)
        Dim xColl As New XItemCollection

        For Each k In defaultRD.Keys
            Dim key As String = k.ToString
            Dim s As String() = key.Split("."c)
            Dim category As String = "Misc"

            If (s.Length > 1) Then category = s(0)

            If (customRD.Contains(k)) Then
                xColl.Add(New XItem(key, customRD.Item(k), category))
            Else
                xColl.Add(New XItem(key, defaultRD.Item(k), category))
            End If
        Next

        pGrid.SelectedObject = xColl
        pGrid.NameColumnWidth = ConfigManager.AppConfig.PGridNameColumnWidth
    End Sub

    Private Sub PropertyEditorDialog_Initialized(sender As Object, e As EventArgs)
        Me.Icon = Imaging.GetAppIconImage
        'Set customize window size.
        Me.Width = ConfigManager.AppConfig.PGridWinWidth
        Me.Height = ConfigManager.AppConfig.PGridWinHeight
    End Sub

    Private Sub PGrid_PropertyValueChanged(sender As Object, e As Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs)
        If e.NewValue Is Nothing Then Return
        Dim keyName As String = DirectCast(e.OriginalSource, Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem).DisplayName

        'Compare changed value's format.
        If (defaultRD.Item(keyName).GetType.Equals(e.NewValue.GetType)) Then
            'Update in customRD(custom.xaml).
            If (customRD.Contains(keyName)) Then
                customRD.Item(keyName) = e.NewValue
            Else
                customRD.Add(keyName, e.NewValue)
            End If

            'Need to refresh entire merged dictionary because theme reset will not work if a value other
            'than color is changed.
            'mainRD.MergedDictionaries.Add(customRD)
            'ThemeDialog.ClearCustomThemes()
            'Application.Current.Resources.MergedDictionaries.Add(mainRD)

            'The following snippet will not update customizable themes when reset. Requires a program restart.
            Dim mainRD As New ResourceDictionary

            'Reload theme only if the property is of type Color. Theme is not updated when changing Color but other properties work.
            'If (TypeOf e.NewValue Is Color) Then
            mainRD.Source = New Uri(SelectedTheme.MainFile, UriKind.Absolute)
            mainRD.MergedDictionaries.Add(customRD)
            ThemeDialog.ClearCustomThemes()
            Application.Current.Resources.MergedDictionaries.Add(mainRD)

            Me.HasChanges = True
        Else
            MessageBoxML.Show(Me, String.Format(Localization.GetString("String_InvalidValueFor"), keyName),
                          Localization.GetString("String_CustomTheme"), MessageBoxButton.OK,
                          MessageBoxImage.Error, MessageBoxResult.OK)
        End If
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Private Sub PropertyEditorDialog_Closed(sender As Object, e As EventArgs)
        If Me.HasChanges Then
            Dim writer As StreamWriter = Nothing

            Try
                writer = New StreamWriter(SelectedTheme.CustomFile)

                Dim settings As XmlWriterSettings = New XmlWriterSettings With {
                    .Indent = True,
                    .OmitXmlDeclaration = True
                }
                Dim dsm As XamlDesignerSerializationManager =
                    New XamlDesignerSerializationManager(XmlWriter.Create(writer, settings)) With {
                    .XamlWriterMode = XamlWriterMode.Value
                    }

                XamlWriter.Save(Me.customRD, dsm)
            Catch ex As Exception
                MessageBoxML.Show(Localization.GetString("String_ErrorSavingChanges"),
                                  Localization.GetString("String_CustomTheme"), MessageBoxButton.OK,
                                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            Finally
                If (writer IsNot Nothing) Then writer.Close()
            End Try
        End If

        'Reset customization flag.
        ThemeDialog.IsCustomizeWindowOpen = False

        'Save name column width of property grid.
        ConfigManager.AppConfig.PGridNameColumnWidth = pGrid.NameColumnWidth

        'Save customize window size.
        ConfigManager.AppConfig.PGridWinWidth = Me.ActualWidth
        ConfigManager.AppConfig.PGridWinHeight = Me.ActualHeight

        Application.Current.MainWindow.Activate()
    End Sub
End Class

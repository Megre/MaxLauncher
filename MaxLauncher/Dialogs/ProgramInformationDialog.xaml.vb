'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports MaxLauncher.Utility

Class ProgramInformationDialog

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        DialogResult = True
    End Sub

    Private Sub ProgramInformationDialog_Loaded(sender As Object, e As RoutedEventArgs)

        versionTextBox.Text = My.Application.Info.Version.ToString
        programDirectoryTextBox.Text = My.Application.Info.DirectoryPath
        appCfgTextBox.Text = ConfigManager.PortableConfig.ApplicationConfigFile
        favoritesCfgTextBox.Text = ConfigManager.PortableConfig.FavoritesConfigFile
        windowCfgTextBox.Text = ConfigManager.PortableConfig.WindowConfigFile
        iconCacheTextBox.Text = ConfigManager.PortableConfig.IconCacheFile
        themesDirectoryTextBox.Text = ConfigManager.PortableConfig.ThemesDirectory
        languageDirectoryTextBox.Text = ConfigManager.PortableConfig.LanguageDirectory
        shortcutsDirectoryTextBox.Text = ConfigManager.PortableConfig.ShortcutsDirectory

        Me.MinWidth = Me.ActualWidth
        Me.MinHeight = Me.ActualHeight
    End Sub

    Private Sub ProgramInformationDialog_Initialized(sender As Object, e As EventArgs)
        Me.Icon = Imaging.GetAppIconImage
    End Sub
End Class

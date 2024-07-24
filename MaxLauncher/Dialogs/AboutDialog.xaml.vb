'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports MaxLauncher.Utility

Class AboutDialog

    Private Sub AboutDialog_Loaded(sender As Object, e As RoutedEventArgs)
        titleLabel.Content = My.Application.Info.Title
        copyrightLabel.Content = My.Application.Info.Copyright
        companyLabel.Content = My.Application.Info.CompanyName & " (" & My.Settings.EMAIL & ")"
        versionLabel.Content = String.Format(Localization.GetString("String_Version") & " {0}",
                                             My.Application.Info.Version.ToString)
        appImage.Source = Imaging.GetAppIconImage
        licenseText.Text = My.Resources.LICENSE
    End Sub

    Private Sub AboutDialog_Initialized(sender As Object, e As EventArgs)
        Me.Icon = Imaging.GetAppIconImage
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
End Class

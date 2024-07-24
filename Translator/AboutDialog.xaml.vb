'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Reflection
Imports System.IO

Class AboutDialog

    Private Sub AboutDialog_Loaded(sender As Object, e As RoutedEventArgs)
        titleLabel.Content = My.Application.Info.Title
        copyrightLabel.Content = My.Application.Info.Copyright
        companyLabel.Content = My.Application.Info.CompanyName & " (" & My.Settings.EMAIL & ")"
        versionLabel.Content = $"Version: {My.Application.Info.Version}"

        ' Set the image source.
        appImage.Width = 32
        appImage.Source = Imaging.GetAppIconImage

        licenseText.Text = My.Resources.LICENSE

    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
End Class

'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Class MessageBoxML

    Public Property Result As MessageBoxResult = MessageBoxResult.None

    Public Overloads Shared Function Show(message As String,
                                            caption As String,
                                            button As System.Windows.MessageBoxButton,
                                            icon As System.Windows.MessageBoxImage,
                                            Optional defaultButton As System.Windows.MessageBoxResult = MessageBoxResult.None,
                                            Optional details As String = "") As System.Windows.MessageBoxResult
        Return Show(My.Application.MainWindow,
                    message,
                    caption,
                    button,
                    icon,
                    defaultButton,
                    details)
    End Function

    Public Overloads Shared Function Show(owner As System.Windows.Window,
                                                message As String,
                                                caption As String,
                                                button As System.Windows.MessageBoxButton,
                                                icon As System.Windows.MessageBoxImage,
                                                Optional defaultButton As System.Windows.MessageBoxResult = MessageBoxResult.None,
                                                Optional details As String = "") As System.Windows.MessageBoxResult
        Dim messageBox = New MessageBoxML

        Try
            If owner.IsLoaded Then messageBox.Owner = owner
        Catch ex As Exception
        End Try

        messageBox.Title = caption
        messageBox.messageTextBlock.Text = message

        Select Case icon
            Case MessageBoxImage.Asterisk
                messageBox.messageIconImage.Source = Imaging.GetImageFromIcon(System.Drawing.SystemIcons.Asterisk)
            Case MessageBoxImage.Error
                messageBox.messageIconImage.Source = Imaging.GetImageFromIcon(System.Drawing.SystemIcons.Error)
                Beep()
            Case MessageBoxImage.Exclamation
                messageBox.messageIconImage.Source = Imaging.GetImageFromIcon(System.Drawing.SystemIcons.Exclamation)
            Case MessageBoxImage.Hand
                messageBox.messageIconImage.Source = Imaging.GetImageFromIcon(System.Drawing.SystemIcons.Hand)
            Case MessageBoxImage.Information
                messageBox.messageIconImage.Source = Imaging.GetImageFromIcon(System.Drawing.SystemIcons.Information)
            Case MessageBoxImage.Stop
                messageBox.messageIconImage.Source = Imaging.GetImageFromIcon(System.Drawing.SystemIcons.Hand)
            Case MessageBoxImage.Question
                messageBox.messageIconImage.Source = Imaging.GetImageFromIcon(System.Drawing.SystemIcons.Question)
            Case MessageBoxImage.Warning
                messageBox.messageIconImage.Source = Imaging.GetImageFromIcon(System.Drawing.SystemIcons.Warning)
            Case Else
                messageBox.messageIconImage.Source = Nothing
        End Select

        Select Case button
            Case MessageBoxButton.OK
                messageBox.okButton.Visibility = Windows.Visibility.Visible
            Case MessageBoxButton.OKCancel
                messageBox.okButton.Visibility = Windows.Visibility.Visible
                messageBox.cancelButton.Visibility = Windows.Visibility.Visible
            Case MessageBoxButton.YesNo
                messageBox.yesButton.Visibility = Windows.Visibility.Visible
                messageBox.noButton.Visibility = Windows.Visibility.Visible
            Case MessageBoxButton.YesNoCancel
                messageBox.yesButton.Visibility = Windows.Visibility.Visible
                messageBox.noButton.Visibility = Windows.Visibility.Visible
                messageBox.cancelButton.Visibility = Windows.Visibility.Visible
        End Select

        Select Case defaultButton
            Case MessageBoxResult.Cancel
                If (messageBox.cancelButton.Visibility = Windows.Visibility.Visible) Then _
                    messageBox.cancelButton.IsDefault = True
            Case MessageBoxResult.No
                If (messageBox.noButton.Visibility = Windows.Visibility.Visible) Then _
                    messageBox.noButton.IsDefault = True
            Case MessageBoxResult.OK
                If (messageBox.okButton.Visibility = Windows.Visibility.Visible) Then _
                    messageBox.okButton.IsDefault = True
            Case MessageBoxResult.Yes
                If (messageBox.yesButton.Visibility = Windows.Visibility.Visible) Then _
                    messageBox.yesButton.IsDefault = True
        End Select

        If String.IsNullOrEmpty(details) Then
            messageBox.detailsExpander.Visibility = Windows.Visibility.Collapsed
            messageBox.ResizeMode = Windows.ResizeMode.NoResize
        Else
            messageBox.detailsTextBlock.Text = details
        End If

        messageBox.ShowDialog()

        Return messageBox.Result
    End Function

    Private Sub YesButton_Click(sender As Object, e As RoutedEventArgs)
        Result = MessageBoxResult.Yes
        Me.Close()
    End Sub

    Private Sub NoButton_Click(sender As Object, e As RoutedEventArgs)
        Result = MessageBoxResult.No
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs)
        Result = MessageBoxResult.Cancel
        Me.Close()
    End Sub

    Private Sub OkButton_Click(sender As Object, e As RoutedEventArgs)
        Result = MessageBoxResult.OK
        Me.Close()
    End Sub

End Class

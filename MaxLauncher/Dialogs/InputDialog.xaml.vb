'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Class InputDialog
    Friend Property Value As String
    Private ReadOnly LabelText As String
    Private ReadOnly TitleText As String
    Private ReadOnly IsMultiLine As Boolean

    Public Sub New(ByVal title As String, ByVal labelText As String, ByVal textBoxText As String, Optional ByVal IsMultiLine As Boolean = False)
        Me.Value = textBoxText
        Me.TitleText = title
        Me.LabelText = labelText
        Me.IsMultiLine = IsMultiLine

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If IsMultiLine = True Then
            With inputTextBox
                .AcceptsReturn = True
                .VerticalScrollBarVisibility = ScrollBarVisibility.Visible
                .VerticalContentAlignment = VerticalAlignment.Center
                .MinLines = 2
            End With
        End If
    End Sub

    Private Sub OKButton_Click(sender As Object, e As RoutedEventArgs)
        Value = Me.inputTextBox.Text

        DialogResult = True
    End Sub

    Private Sub InputDialog_Initialized(sender As Object, e As EventArgs)
        Me.Icon = Utility.Imaging.GetAppIconImage
    End Sub

    Private Sub InputDialog_Loaded(sender As Object, e As RoutedEventArgs)
        Me.Title = TitleText
        Me.inputTextBox.Text = Value
        Me.inputLabel.Content = LabelText
        inputTextBox.Focus()
    End Sub
End Class

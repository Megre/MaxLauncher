'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Drawing
Imports System.Runtime.InteropServices
Imports MaxLauncher.Utility

Class EditButtonDialog

#Region "Properties"

    Public Property IconFile As String = ""
    Public Property IconIndex As Integer
    Public Property ButtonData As MLButtonData

#End Region

#Region "Constructors"

    Public Sub New(buttonData As MLButtonData)
        InitializeComponent()

        If (buttonData Is Nothing) Then
            Me.ButtonData = New MLButtonData
        Else
            Me.ButtonData = buttonData
        End If
    End Sub

#End Region

#Region "Events"
    Private Sub EditButtonDialog_Initialized(sender As Object, e As EventArgs)
        Me.Icon = Imaging.GetAppIconImage
    End Sub

    Private Sub BrowseFolderButton_Click(sender As Object, e As RoutedEventArgs)
        Dim dlg As New System.Windows.Forms.FolderBrowserDialog() With {
            .Description = Localization.GetString("String_SelectWorkingDirectory")
        }

        If (dlg.ShowDialog = System.Windows.Forms.DialogResult.OK) Then startInTextBox.Text = dlg.SelectedPath
    End Sub

    Private Sub ViewListButton_Click(sender As Object, e As RoutedEventArgs)
        Dim groupLaunchDlg As New GroupLaunchDialog(groupIDComboBox.Text) With {
            .Owner = Me
        }
        groupLaunchDlg.ShowDialog()
    End Sub

    Private Sub OKButton_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrEmpty(targetTextBox.Text) Then
            MessageBoxML.Show(Localization.GetString("String_EmptyTarget"),
                  Localization.GetString("String_EditButton"), MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK)
        Else

            ButtonData.Text = nameTextBox.Text
            ButtonData.Filename = targetTextBox.Text
            ButtonData.Arguments = argumentsTextBox.Text
            ButtonData.WorkingDirectory = startInTextBox.Text
            ButtonData.Description = descriptionTextBox.Text
            ButtonData.IconFile = IconFile
            ButtonData.IconIndex = IconIndex
            ButtonData.WindowStyle = runComboBox.SelectedIndex
            ButtonData.WindowStyleEx = sizeComboBox.SelectedIndex
            ButtonData.RunAsAdmin = runAsAdminCheckBox.IsChecked
            ButtonData.WindowTitle = windowTitleComboBox.Text
            ButtonData.WindowProcessName = windowProcessNameComboBox.Text
            ButtonData.GroupID = Strings.Trim(groupIDComboBox.Text)

            If (ButtonData.WindowStyleEx = Launcher.WindowStyleEx.WindowStyleEx_Custom) Then
                Dim LeftValue, TopValue, WidthValue, HeightValue As Integer

                If Not (Integer.TryParse(leftTextBox.Text, LeftValue)) OrElse (LeftValue < 0) Then
                    'Display error
                    MessageBoxML.Show(Localization.GetString("String_LeftMustBeZeroOrGreater"),
                                    Localization.GetString("String_CustomWindowSize"), MessageBoxButton.OK,
                                    MessageBoxImage.Error, MessageBoxResult.OK)
                    Me.leftTextBox.Focus()
                    Return
                ElseIf Not (Integer.TryParse(topTextBox.Text, TopValue)) OrElse (TopValue < 0) Then
                    'Display error
                    MessageBoxML.Show(Localization.GetString("String_TopMustBeZeroOrGreater"),
                                    Localization.GetString("String_CustomWindowSize"), MessageBoxButton.OK,
                                    MessageBoxImage.Error, MessageBoxResult.OK)
                    Me.topTextBox.Focus()
                    Return
                ElseIf Not (Integer.TryParse(widthTextBox.Text, WidthValue)) OrElse (WidthValue < 0) Then
                    'Display error
                    MessageBoxML.Show(Localization.GetString("String_WidthMustBeZeroOrGreater"),
                                    Localization.GetString("String_CustomWindowSize"), MessageBoxButton.OK,
                                    MessageBoxImage.Error, MessageBoxResult.OK)
                    Me.widthTextBox.Focus()
                    Return
                ElseIf Not (Integer.TryParse(heightTextBox.Text, HeightValue)) OrElse (HeightValue < 0) Then
                    'Display error
                    MessageBoxML.Show(Localization.GetString("String_HeightMustBeZeroOrGreater"),
                                    Localization.GetString("String_CustomWindowSize"), MessageBoxButton.OK,
                                    MessageBoxImage.Error, MessageBoxResult.OK)
                    Me.heightTextBox.Focus()
                    Return
                End If

                ButtonData.WindowRect = New System.Drawing.Rectangle(LeftValue, TopValue, WidthValue, HeightValue)
            End If

            DialogResult = True
            Me.Close()
        End If
    End Sub

    Private Sub TargetComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not Me.IsLoaded Then Return

        Dim comboBox = DirectCast(sender, ComboBox)
        Dim currentCBI As ComboBoxItem = DirectCast(comboBox.SelectedItem, ComboBoxItem)

        Select Case (currentCBI.Tag)
            Case 1 'File
                Dim dlg As New Microsoft.Win32.OpenFileDialog()

                With dlg
                    .DereferenceLinks = False
                    .Title = Localization.GetString("String_SelectFile")
                    '.DereferenceLinks = False

                    'Show open file dialog box 
                    Dim result? As Boolean = .ShowDialog()

                    'Process open file dialog box results 
                    If result = True Then
                        Dim btnData = MButton.CreateData(.FileName)
                        If btnData IsNot Nothing Then ButtonData = btnData

                        'Update controls
                        Try
                            UpdateControls()
                        Catch ex As Exception
                            'Ignore
                        End Try
                    End If
                End With

            Case 2 'Folder
                Dim dlg As New System.Windows.Forms.FolderBrowserDialog()

                With dlg
                    .Description = Localization.GetString("String_SelectFolder")

                    'Process folder browser dialog box results 
                    If (.ShowDialog = System.Windows.Forms.DialogResult.OK) Then
                        ButtonData = MButton.CreateData(.SelectedPath)

                        'Update controls
                        Try
                            UpdateControls()
                        Catch ex As Exception
                            'Ignore
                        End Try
                    End If
                End With
        End Select

        'Reset selection
        comboBox.SelectedIndex = 0
    End Sub

    Private Sub RunComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not Me.IsLoaded Then Return

        UpdateWindowPreviewRectangle()
        UpdateLocationPresetControl()
    End Sub

    Private Sub IconComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not Me.IsLoaded Then Return

        Dim comboBox = DirectCast(sender, ComboBox)
        If comboBox.SelectedIndex = 0 Then Return

        Dim currentCBI As ComboBoxItem = DirectCast(comboBox.SelectedItem, ComboBoxItem)

        Select Case (currentCBI.Tag)
            Case 1 'Default Icon
                IconFile = ""
                IconIndex = 0
            Case 2 'Browse Icon
                Try
                    ShowIconDialog()
                Catch ex As Exception
                    MessageBoxML.Show(ex.Message, Localization.GetString("String_Paste"), MessageBoxButton.OK,
                                      MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
                End Try
            Case 3 'Browse Image
                Dim imageFile = FileUtils.GetFilename(Me,
                                                      IO.FileMode.Open,
                                                      Localization.GetString("String_SelectImageFile"),
                                                      "",
                                                      "All Image Files|*.bmp;*.dib;*.gif;*.jpg;*.jpeg;*.jpe;*.jfif;*.png;*.tif;*.tiff|" &
                                                      "Bitmap Files|*.bmp;*.dib|" &
                                                      "GIF|*.gif|" &
                                                      "JPEG|*.jpg;*.jpeg;*.jpe;*.jfif|" &
                                                      "PNG|*.png|" &
                                                      "TIFF|*.tif;*.tiff")

                If Not (String.IsNullOrEmpty(imageFile)) Then
                    IconFile = FileUtils.Path.ConvertToRelativePath(imageFile)
                    IconIndex = 0
                End If
        End Select

        Try
            UpdateIconImage()
        Catch ex As Exception
            'Ignore.
        End Try
        comboBox.SelectedIndex = 0
    End Sub

    Private Sub WindowTitleComboBox_Initialized(sender As Object, e As EventArgs)
        windowTitleComboBox.ItemsSource = Utility.ActiveWindows.GetWindowTitles()
    End Sub

    Private Sub WindowProcessNameComboBox_Initialized(sender As Object, e As EventArgs)
        windowProcessNameComboBox.ItemsSource = Utility.ActiveWindows.GetProcessNames()
    End Sub

    Private Sub GroupIDComboBox_Initialized(sender As Object, e As EventArgs)
        groupIDComboBox.ItemsSource = GetButtonGroupIDList()
    End Sub

    Private Function GetButtonGroupIDList() As List(Of String)
        Dim groupList As New List(Of String)

        Dim favButtonTable As DataTable = FavoritesBarData.GetInstance.Tables("Button")
        Dim groupIDQuery = From s In favButtonTable.AsEnumerable
                           Where s.RowState <> DataRowState.Deleted AndAlso s.Field(Of String)("GroupID") IsNot Nothing AndAlso s.Field(Of String)("GroupID") <> ""
                           Select New With {.GroupID = s.Field(Of String)("GroupID")}

        For Each buttons In groupIDQuery
            groupList.Add(buttons.GroupID)
        Next

        Dim tabButtonTable As DataTable = TabControlData.GetInstance.Tables("Button")
        Dim groupIDQuery2 = From s In tabButtonTable.AsEnumerable
                            Where s.RowState <> DataRowState.Deleted AndAlso s.Field(Of String)("GroupID") IsNot Nothing AndAlso s.Field(Of String)("GroupID") <> ""
                            Select New With {.GroupID = s.Field(Of String)("GroupID")}

        For Each buttons In groupIDQuery2
            groupList.Add(buttons.GroupID)
        Next

        Return groupList.Distinct.ToList
    End Function

    Private Sub EditButtonDialog_Loaded(sender As Object, e As RoutedEventArgs)
        Try
            UpdateControls()
        Catch ex As Exception
            'Ignore
        End Try
    End Sub

#End Region

#Region "Helper Methods"

    Private Sub ShowIconDialog()
        Try
            'Show icon dialog box
            Dim index As Integer = 0
            Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder(System.IO.Path.Combine(Environment.GetFolderPath(
                                                        Environment.SpecialFolder.System), "shell32.dll"), 500)

            'Try to load icon file in icon dialog.
            Dim curIconFile As String = ""

            If Not (String.IsNullOrEmpty(Me.IconFile)) Then
                curIconFile = Me.IconFile
            Else
                curIconFile = Me.targetTextBox.Text
            End If

            Dim fileExtension As String = System.IO.Path.GetExtension(curIconFile).ToLower

            If (fileExtension.Equals(".exe")) OrElse (fileExtension.Equals(".dll")) OrElse
                    (fileExtension.Equals(".ico")) OrElse (fileExtension.Equals(".icl")) Then

                'Convert relative path to full path.
                Dim iconFileFullPath As String = curIconFile
                iconFileFullPath = FileUtils.Path.ConvertToFullPath(iconFileFullPath)

                sb = New System.Text.StringBuilder(iconFileFullPath, 500)
                index = Me.IconIndex
            End If

            Dim hwnd As IntPtr = New System.Windows.Interop.WindowInteropHelper(Me).Handle
            Dim returnValue As Integer
            returnValue = NativeMethods.PickIconDlg(CInt(hwnd), sb, CUInt(sb.Capacity), index)

            If CBool((returnValue)) Then
                Dim newIconFile As String = FileUtils.Path.ConvertToRelativePath(sb.ToString)

                If String.Equals(newIconFile, Me.targetTextBox.Text, 0) AndAlso index = 0 Then
                    Me.IconFile = String.Empty
                    Me.IconIndex = 0
                Else
                    Me.IconFile = newIconFile
                    Me.IconIndex = index
                End If
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub UpdateIconImage()

        Try
            If (String.IsNullOrEmpty(IconFile)) Then
                If (targetTextBox IsNot Nothing) AndAlso Not (String.IsNullOrEmpty(targetTextBox.Text)) Then
                    Dim target = FileUtils.Path.ExpandEnvironmentVariables(targetTextBox.Text)

                    Dim icon As System.Drawing.Icon

                    If target.StartsWith("\\") Then
                        icon = Imaging.GetAssociatedIconFromUNC(target)
                    Else
                        icon = System.Drawing.Icon.ExtractAssociatedIcon(target)
                    End If

                    If icon IsNot Nothing Then
                        iconImage.Source = Imaging.GetImageFromIcon(icon)
                        icon.Dispose()
                    End If
                End If
            Else
                'Dim iconFileFullPath = System.IO.Path.GetFullPath(IconFile)
                Dim fileExtension As String = System.IO.Path.GetExtension(IconFile).ToLower

                If (fileExtension.Equals(".exe")) OrElse (fileExtension.Equals(".dll")) OrElse
                            (fileExtension.Equals(".ico")) OrElse (fileExtension.Equals(".icl")) Then
                    iconImage.Source = Imaging.GetImageFromIconFile(Me.IconFile, Me.IconIndex)
                Else
                    'The line below does not work anymore.
                    'iconImage.Source = Imaging.GetImageFromImageFile(Me.IconFile, IconCacheDB.DEFAULT_ICON_SIZE)

                    Dim icon As System.Drawing.Icon = Nothing

                    icon = Imaging.GetIconFromImageFile(Me.IconFile, IconCacheDB.DEFAULT_ICON_SIZE)
                    If (icon IsNot Nothing) Then
                        iconImage.Source = Imaging.IconToBitmapImage(icon)
                        icon.Dispose()
                    End If
                End If
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub UpdateControls()
        With ButtonData
            nameTextBox.Text = .Text
            targetTextBox.Text = .Filename
            argumentsTextBox.Text = .Arguments
            startInTextBox.Text = .WorkingDirectory
            descriptionTextBox.Text = .Description
            IconFile = .IconFile
            IconIndex = .IconIndex
            runComboBox.SelectedIndex = .WindowStyle
            runAsAdminCheckBox.IsChecked = .RunAsAdmin
            windowTitleComboBox.Text = .WindowTitle
            windowProcessNameComboBox.Text = .WindowProcessName
            groupIDComboBox.Text = .GroupID

            sizeComboBox.SelectedIndex = .WindowStyleEx
            leftTextBox.Text = .WindowRect.Left
            topTextBox.Text = .WindowRect.Top
            widthTextBox.Text = .WindowRect.Width
            heightTextBox.Text = .WindowRect.Height

            UpdateWindowPreviewRectangle()
            UpdateLocationPresetControl()
            UpdateViewListButton()
        End With

        Try
            UpdateIconImage()
        Catch ex As Exception
            Throw
        End Try
    End Sub

#End Region

    Private Sub SizeComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not Me.IsLoaded Then Return

        UpdateWindowPreviewRectangle()

        If (sizeComboBox.SelectedIndex = Launcher.WindowStyleEx.WindowStyleEx_Custom) Then
            'sizeGroupBox.IsEnabled = True
            sizeGroupBox.Visibility = Visibility.Visible
        Else
            'sizeGroupBox.IsEnabled = False
            sizeGroupBox.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub SizeComboBox_Loaded(sender As Object, e As RoutedEventArgs)
        If (sizeComboBox.SelectedIndex = -1) Then sizeComboBox.SelectedIndex = 0
        UpdateWindowPreviewRectangle()
    End Sub

    Private Sub UpdateWindowPreviewRectangle()
        Dim width As Double = windowPreviewBorder.ActualWidth - windowPreviewBorder.BorderThickness.Left
        Dim height As Double = windowPreviewBorder.ActualHeight - windowPreviewBorder.BorderThickness.Top

        Select Case runComboBox.SelectedIndex
            Case ProcessWindowStyle.Maximized
                windowPreviewRectangle.Margin = New Thickness(0, 0, 0, 0)
            Case ProcessWindowStyle.Minimized
                windowPreviewRectangle.Margin = New Thickness(0, height * 7 / 8, width * 3 / 4, 0)
            Case ProcessWindowStyle.Hidden
                windowPreviewRectangle.Margin = New Thickness(0, 0, width, height)
            Case ProcessWindowStyle.Normal
                If sizeComboBox Is Nothing Then Exit Select
                windowPreviewRectangle.Margin = New Thickness(width / 6, height / 4, width / 6, height / 4)

                If String.IsNullOrEmpty(windowTitleComboBox.Text) Then Exit Select
                Select Case sizeComboBox.SelectedIndex
                    Case Launcher.WindowStyleEx.WindowStyleEx_Normal
                    Case Launcher.WindowStyleEx.WindowStyleEx_Left
                        windowPreviewRectangle.Margin = New Thickness(0, 0, width / 2, 0)
                    Case Launcher.WindowStyleEx.WindowStyleEx_Right
                        windowPreviewRectangle.Margin = New Thickness(width / 2, 0, 0, 0)
                    Case Launcher.WindowStyleEx.WindowStyleEx_Top
                        windowPreviewRectangle.Margin = New Thickness(0, 0, 0, height / 2)
                    Case Launcher.WindowStyleEx.WindowStyleEx_Bottom
                        windowPreviewRectangle.Margin = New Thickness(0, height / 2, 0, 0)
                    Case Launcher.WindowStyleEx.WindowStyleEx_TopLeft
                        windowPreviewRectangle.Margin = New Thickness(0, 0, width / 2, height / 2)
                    Case Launcher.WindowStyleEx.WindowStyleEx_TopRight
                        windowPreviewRectangle.Margin = New Thickness(width / 2, 0, 0, height / 2)
                    Case Launcher.WindowStyleEx.WindowStyleEx_BottomLeft
                        windowPreviewRectangle.Margin = New Thickness(0, height / 2, width / 2, 0)
                    Case Launcher.WindowStyleEx.WindowStyleEx_BottomRight
                        windowPreviewRectangle.Margin = New Thickness(width / 2, height / 2, 0, 0)
                    Case Launcher.WindowStyleEx.WindowStyleEx_Custom
                        windowPreviewRectangle.Margin = New Thickness(width / 2, height / 2, width / 12, height / 6)
                End Select
        End Select
    End Sub

    Private Sub WindowReferenceComboBox_Initialized(sender As Object, e As EventArgs)
        windowReferenceComboBox.ItemsSource = Utility.ActiveWindows.GetWindowTitles()
    End Sub

    Private Sub WindowReferenceComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not Me.IsLoaded Then Return

        Dim selectedWindowTitle As String = windowReferenceComboBox.SelectedValue

        Dim referenceRect = New Rectangle()

        referenceRect = Launcher.GetWindowRect(FindWindowByTitle(selectedWindowTitle))

        If IsNothing(referenceRect) Then Return

        'Make sure maximized windows do not return negative numbers.
        Dim left, top, width, height As Integer

        If (Integer.TryParse(referenceRect.Left, left)) Then
            If (left < 0) Then left = 0
            leftTextBox.Text = left
        End If
        If (Integer.TryParse(referenceRect.Top, top)) Then
            If (top < 0) Then top = 0
            topTextBox.Text = top
        End If
        If (Integer.TryParse(referenceRect.Width, width)) Then
            If (width < 0) Then width = 0
            widthTextBox.Text = width
        End If
        If (Integer.TryParse(referenceRect.Height, height)) Then
            If (height < 0) Then height = 0
            heightTextBox.Text = height
        End If
    End Sub

    Private Function FindWindowByTitle(title As String) As IntPtr
        Dim hwnd As IntPtr = IntPtr.Zero

        'Match partial title.
        For Each p As Process In Process.GetProcesses
            Try
                If (String.Compare(p.MainWindowTitle, title, True) = 0) Then
                    hwnd = p.MainWindowHandle
                End If
            Catch ex As Exception
                'Ignore
            End Try
        Next

        Return hwnd
    End Function

    Private Sub UpdateViewListButton()
        If (String.IsNullOrEmpty(Trim(groupIDComboBox.Text))) Then
            viewListButton.IsEnabled = False
        Else
            viewListButton.IsEnabled = True
        End If
    End Sub

    Private Sub GroupIDComboBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        UpdateViewListButton()
    End Sub

    Private Sub ComboBox_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        Dim comboBox1 As ComboBox = TryCast(sender, ComboBox)
        If comboBox1 Is Nothing Then Return

        If e.Key = Key.Down Then
            If comboBox1.IsDropDownOpen = False Then
                comboBox1.IsDropDownOpen = True
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub UpdateLocationPresetControl()
        If runComboBox.SelectedIndex = ProcessWindowStyle.Normal AndAlso
            Not String.IsNullOrEmpty(windowTitleComboBox.Text) Then
            sizeLabel.IsEnabled = True
            sizeComboBox.IsEnabled = True
        Else
            sizeLabel.IsEnabled = False
            sizeComboBox.IsEnabled = False
        End If
    End Sub

    Private Sub WindowTitleComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not Me.IsLoaded Then Return

        windowTitleComboBox.Text = windowTitleComboBox.SelectedValue

        UpdateLocationPresetControl()
    End Sub

    Private Sub WindowTitleComboBox_KeyUp(sender As Object, e As KeyEventArgs)
        UpdateLocationPresetControl()
    End Sub

End Class

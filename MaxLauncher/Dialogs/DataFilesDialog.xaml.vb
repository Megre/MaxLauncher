'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports MaxLauncher.AppConfig

Public Class DataFilesDialog

    Private Sub DataFilesDialog_Loaded(sender As Object, e As RoutedEventArgs)
        Me.MinWidth = Me.ActualWidth
        Me.MinHeight = Me.ActualHeight
    End Sub

    Private Sub MoveUpButton_Click(sender As Object, e As RoutedEventArgs)
        'moveUpButton.Focus()

        Dim selectedIndex As Integer = dataFilesListView.SelectedIndex

        If (selectedIndex >= 1) Then
            Dim dataFile As AppConfig.DataFile = ConfigManager.AppConfig.DataFiles.Item(selectedIndex)
            ConfigManager.AppConfig.DataFiles.RemoveAt(selectedIndex)
            ConfigManager.AppConfig.DataFiles.Insert(selectedIndex - 1, dataFile)
            dataFilesListView.Items.Refresh()

            UpdateButtonsState()
            'If moveUpButton.IsEnabled Then
            '    moveUpButton.Focus()
            'ElseIf moveDownButton.IsEnabled Then
            '    moveDownButton.Focus()
            'End If
        End If
    End Sub

    Private Sub MoveDownButton_Click(sender As Object, e As RoutedEventArgs)
        'moveDownButton.Focus()

        Dim selectedIndex As Integer = dataFilesListView.SelectedIndex

        If (selectedIndex >= 0) AndAlso (dataFilesListView.Items.Count - 1 > selectedIndex) Then
            Dim dataFile As AppConfig.DataFile = ConfigManager.AppConfig.DataFiles.Item(selectedIndex)

            ConfigManager.AppConfig.DataFiles.RemoveAt(selectedIndex)
            ConfigManager.AppConfig.DataFiles.Insert(selectedIndex + 1, dataFile)
            dataFilesListView.Items.Refresh()

            UpdateButtonsState()
            'If moveDownButton.IsEnabled Then
            '    moveDownButton.Focus()
            'ElseIf moveUpButton.IsEnabled Then
            '    moveUpButton.Focus()
            'End If
        End If
    End Sub

    Private Sub RemoveButton_Click(sender As Object, e As RoutedEventArgs)
        Dim selectedIndex As Integer = dataFilesListView.SelectedIndex

        If (selectedIndex >= 0) Then
            ConfigManager.AppConfig.DataFiles.RemoveAt(selectedIndex)
            dataFilesListView.Items.Refresh()
            If dataFilesListView.HasItems Then
                If selectedIndex < dataFilesListView.Items.Count Then
                    dataFilesListView.SelectedIndex = selectedIndex
                ElseIf selectedIndex = dataFilesListView.Items.Count Then
                    dataFilesListView.SelectedIndex = selectedIndex - 1
                End If
            End If
            UpdateButtonsState()
            'If removeButton.IsEnabled Then removeButton.Focus()
        End If
    End Sub

    Private Sub SortButton_Click(sender As Object, e As RoutedEventArgs)
        If ConfigManager.AppConfig.DataFiles.Count = 0 Then Return

        ConfigManager.AppConfig.DataFiles.Sort()
        dataFilesListView.Items.Refresh()
        dataFilesListView.SelectedIndex = 0

        UpdateButtonsState()
    End Sub

    Private Sub DataFilesDialog_Initialized(sender As Object, e As EventArgs)
        Me.Icon = Utility.Imaging.GetAppIconImage
    End Sub

    Private Sub DataFilesListView_Initialized(sender As Object, e As EventArgs)
        dataFilesListView.ItemsSource = ConfigManager.AppConfig.DataFiles
    End Sub

    Private Sub DataFilesListView_Loaded(sender As Object, e As RoutedEventArgs)
        If Not dataFilesListView.HasItems Then Return

        dataFilesListView.SelectedIndex = 0
    End Sub

    'Set keyboard focus on text box.
    Private Sub FocusListViewItem_TextBox()
        If dataFilesListView.SelectedIndex = -1 Then Return

        Dim icg As ItemContainerGenerator = dataFilesListView.ItemContainerGenerator
        Dim item As ListViewItem = TryCast(icg.ContainerFromIndex(dataFilesListView.SelectedIndex), ListViewItem)

        If item IsNot Nothing Then item.Focus()

        Dim tb As TextBox = TryCast(FindChildControl(item, "DataFilesListViewTextBox"), TextBox)

        If tb IsNot Nothing Then tb.Focus()
    End Sub

    'Selects the list item when the textbox is clicked.
    Protected Sub DataFileItem_GotKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs)
        Dim item As ListViewItem = TryCast(sender, ListViewItem)

        If item IsNot Nothing Then item.IsSelected = True
    End Sub

    'Catch arrow-down and arrow-up on KeyDown Event to cycle in ListView
    Private Sub DataFileItem_PreviewKeyDown(sender As Object, e As KeyEventArgs)

        Dim selectedIndex As Integer = dataFilesListView.SelectedIndex

        Select Case e.Key

            Case Key.Down
                e.Handled = True
                If (selectedIndex < dataFilesListView.Items.Count - 1) Then
                    dataFilesListView.SelectedIndex = selectedIndex + 1
                ElseIf (selectedIndex = dataFilesListView.Items.Count - 1) Then
                    dataFilesListView.SelectedIndex = 0
                End If

            Case Key.Up
                e.Handled = True
                If (selectedIndex > 0) Then
                    dataFilesListView.SelectedIndex = selectedIndex - 1
                ElseIf (selectedIndex = 0) Then
                    dataFilesListView.SelectedIndex = dataFilesListView.Items.Count - 1
                End If
        End Select
    End Sub

    Private Function FindChildControl(startNode As DependencyObject, controlName As String) As Control
        If startNode Is Nothing Then Return Nothing

        Dim count As Integer = VisualTreeHelper.GetChildrenCount(startNode)
        Dim c As Control = Nothing

        For i As Integer = 0 To count - 1
            Dim current As DependencyObject = VisualTreeHelper.GetChild(startNode, i)

            c = TryCast(current, Control)

            If (c IsNot Nothing) Then
                If c.Name.Equals(controlName) Then
                    Return c
                End If
            End If

            c = FindChildControl(current, controlName)
            If c IsNot Nothing Then Return c
        Next

        Return Nothing
    End Function

    Private Sub DataFilesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        FocusListViewItem_TextBox()
        UpdateButtonsState()
    End Sub

    Private Sub UpdateButtonsState()
        If Not dataFilesListView.HasItems OrElse
            (dataFilesListView.SelectedIndex = -1) Then
            removeButton.IsEnabled = False
            moveUpButton.IsEnabled = False
            moveDownButton.IsEnabled = False
            sortButton.IsEnabled = False
            Return
        End If

        If dataFilesListView.SelectedIndex > -1 Then
            removeButton.IsEnabled = True
        End If

        If dataFilesListView.SelectedIndex > 0 Then
            moveUpButton.IsEnabled = True
        Else
            moveUpButton.IsEnabled = False
        End If

        If dataFilesListView.SelectedIndex < dataFilesListView.Items.Count - 1 Then
            moveDownButton.IsEnabled = True
        Else
            moveDownButton.IsEnabled = False
        End If

        If dataFilesListView.Items.Count > 1 Then
            sortButton.IsEnabled = True
        Else
            sortButton.IsEnabled = False
        End If
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
End Class

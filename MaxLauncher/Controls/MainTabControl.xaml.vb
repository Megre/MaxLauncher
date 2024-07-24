'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.ComponentModel

Class MainTabControl
    Inherits TabControl

    Private localThemeValue As String = ""
    Public Property LocalTheme() As String
        Get
            Return localThemeValue
        End Get
        Set(ByVal value As String)
            'Create Theme row if it does not exist.
            Dim themeDataTable As TabControlData.ThemeDataTable = TabControlData.GetInstance.Theme

            If themeDataTable.Count = 0 Then
                TabControlData.GetInstance.Theme.AddThemeRow(value)
            Else
                If Not themeDataTable.First.Name.Equals(value) Then themeDataTable.First.Name = value
            End If

            localThemeValue = value

            If (Not String.IsNullOrEmpty(value)) AndAlso (Not ConfigManager.AppConfig.AlwaysUseApplicationTheme) Then
                Try
                    ThemeDialog.ApplyTheme(value)
                Catch ex As Exception
                    localThemeValue = ThemeDialog.DefaultThemeName
                    If Not themeDataTable.First.Name.Equals(localThemeValue) Then themeDataTable.First.Name = localThemeValue
                    ThemeDialog.ApplyTheme(localThemeValue)
                    MessageBoxML.Show(ex.Message, Localization.GetString("String_Theme"), MessageBoxButton.OK,
                                    MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
                End Try
            End If

            Me.SaveConfig()
        End Set
    End Property

    Private Const MAX_TABS As Integer = 10
    Private Const NO_OF_NEW_TABS As Integer = 10
    Private Property Mode As IO.FileMode

    Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Sub New(mode As IO.FileMode)
        Me.Mode = mode

        InitializeComponent()
    End Sub

    Public Shared Function ConvertToTabId(tabIndex As Integer) As Integer
        If (tabIndex = 9) Then Return 0

        Return tabIndex + 1
    End Function

    Friend Function ConvertToTabIndex(tabID As Integer) As Integer
        If (tabID = 0) Then Return 9

        Return tabID - 1
    End Function

    Private Function GetTabData() As MLTabData
        Dim tabItem = CType(Me.SelectedItem, TabItemControl)
        Dim listOfTabButtons As New List(Of TabButton)
        GetListOfTabButtons(listOfTabButtons, tabItem.Content)

        'Intercept exception thrown when header = nothing.
        If (tabItem.Header Is Nothing) Then tabItem.Header = ""

        Return New MLTabData(tabItem.Header, listOfTabButtons)
    End Function

    Private Sub SetTabData(sourceTabData As MLTabData)
        Dim selectedTab = CType(Me.SelectedItem, TabItemControl)
        Dim listOfTabButtons As New List(Of TabButton)
        GetListOfTabButtons(listOfTabButtons, selectedTab.Content)

        'Set header in GUI.
        If sourceTabData Is Nothing Then
            selectedTab.Header = ""
        Else
            selectedTab.Header = sourceTabData.Header
        End If

        'Set header in Data Table.
        Dim tabRow = TabControlData.GetInstance.Tab.FindById(ConvertToTabId(Me.SelectedIndex))
        tabRow.Text = selectedTab.Header

        If sourceTabData Is Nothing Then
            For Each button In listOfTabButtons
                button.SetButtonData(Nothing)
            Next
        Else
            For Each button In listOfTabButtons
                If (sourceTabData.MLButtonDataList.ContainsKey(button.Scancode)) Then
                    button.SetButtonData(sourceTabData.MLButtonDataList.Item(button.Scancode))
                Else
                    button.SetButtonData(Nothing)
                End If
            Next
        End If

        SetButtonsVisibility("")

        Me.SaveConfig()
    End Sub

    Private Sub SortButtons(ByVal sortType As MLButtonData.SortType)
        'Prompt confirmation
        If (MessageBoxML.Show(Localization.GetString("String_SortButtons"),
                              Localization.GetString("String_Sort"), MsgBoxStyle.YesNo,
                              MessageBoxImage.Question, MessageBoxResult.No) <> MsgBoxResult.Yes) Then Return

        Try
            Dim tabData As MLTabData = GetTabData()

            tabData.Sort(sortType)
            SetTabData(tabData)
        Catch ex As Exception
            MessageBoxML.Show(ex.Message,
                            Localization.GetString("MessageBox_Title_Sort"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

#Region "Rename Tab Header"

    ''' <summary>
    ''' Renames a Tab using a dialog box
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Rename_Click(sender As Object, e As RoutedEventArgs)
        RenameTabHeader()
    End Sub

    ''' <summary>
    ''' Renames the tab header.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub RenameTabHeader()
        Dim renameDlg As New InputDialog(Localization.GetString("InputDialog_Title_RenameTab"),
                                         Localization.GetString("InputDialog_Label_Tabname"),
                                         CType(Me.SelectedItem, TabItem).Header, True) With {
            .Owner = Application.Current.MainWindow
                                         }
        If (renameDlg.ShowDialog()) Then
            CType(Me.SelectedItem, TabItem).Header = renameDlg.Value
            Dim tabRow = TabControlData.GetInstance.Tab.FindById(ConvertToTabId(Me.SelectedIndex))

            tabRow.Text = renameDlg.Value
        End If

        renameDlg.Close()
        Me.SaveConfig()
    End Sub

#End Region

    ''' <summary>
    ''' Moves the mouse cursor below the tab header that it hovers.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub MoveCursor()
        'There is no selected item when a tab is being deleted.
        If (Me.SelectedItem Is Nothing) Then Return

        For Each item As TabItem In Me.Items
            If Not item.IsVisible Then Continue For 'Related to Search. If tab is not visible, an exception is thrown.

            If Me.SelectedItem.Equals(item) Then
                Continue For
            Else
                Dim itemHeaderRectangle As New Rect(item.PointToScreen(New Point(0D, 0D)),
                                                    item.PointToScreen(New Point(item.ActualWidth, item.ActualHeight)))
                If (itemHeaderRectangle.Contains(Windows.Forms.Cursor.Position.X, Windows.Forms.Cursor.Position.Y)) Then
                    Windows.Forms.Cursor.Position = New System.Drawing.Point(Windows.Forms.Cursor.Position.X, itemHeaderRectangle.Bottom + 2)
                    Exit For
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Add a new tab item.
    ''' </summary>
    ''' <remarks>Only a maximum of 10 tabs are allowed.</remarks>
    Private Sub AddTabItem(tabId As Integer, header As String)
        If (Me.Items.Count < MAX_TABS) Then
            'Create tab row if it doesn't exist.
            If (TabControlData.GetInstance.Tab.FindById(tabId) Is Nothing) Then
                TabControlData.GetInstance.Tab.AddTabRow(tabId, header)
            End If

            Dim newTabItem As New TabItemControl With {
                .Header = header,
                .Key = tabId
            }

            Dim newTabButtonGroup = New TabButtonGroup With {
                .IsTabStop = False
            }

            newTabItem.Content = newTabButtonGroup
            Me.AddChild(newTabItem)

            Me.SaveConfig()
        End If
    End Sub

    ''' <summary>
    ''' Gets a list of child tab buttons.
    ''' </summary>
    ''' <typeparam name="T">TabButton</typeparam>
    ''' <param name="results">List of TabButton</param>
    ''' <param name="startNode">Parent DependencyObject</param>
    ''' <remarks></remarks>
    Friend Sub GetListOfTabButtons(Of T As TabButton)(results As List(Of T), startNode As DependencyObject)
        Dim children As IEnumerable = LogicalTreeHelper.GetChildren(startNode)
        For Each child In children
            If (TypeOf child Is T) Then
                results.Add(child)
            Else
                If (TypeOf child Is DependencyObject) Then GetListOfTabButtons(Of T)(results, child)
            End If
        Next
    End Sub

    Friend Sub SetButtonsVisibility(filter As String)
        If Not Me.HasItems Then Return

        Dim blist As New List(Of TabButton)
        GetListOfTabButtons(blist, Me)

        If String.IsNullOrEmpty(filter) OrElse
            String.Compare(filter, Localization.GetString("SearchBox"), False) = 0 Then
            For Each aTab As TabItem In Me.Items
                aTab.Visibility = Visibility.Visible
                aTab.Focusable = True  'Allow tabs to be selectable.
            Next
            For Each btn In blist
                btn.SetVisibility()
            Next
        Else
            'Hide all tabs.
            For Each aTab As TabItem In Me.Items
                aTab.Visibility = Visibility.Hidden
                aTab.Focusable = False 'Disallow tabs to be selectable.
            Next

            'Compare each button to search filter. 
            For Each btn In blist
                If btn.Text.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 Then
                    Dim aTab = CType(Me.Items.Item(btn.GetParentTabIndex), TabItemControl)
                    If Not aTab.IsVisible Then
                        aTab.Visibility = Visibility.Visible
                        aTab.Focusable = True 'Allow tabs to be selectable.
                        Me.SelectedItem = aTab
                    End If

                    btn.Visibility = Visibility.Visible
                Else
                    btn.Visibility = Visibility.Hidden
                End If
            Next
        End If
    End Sub

    Private Sub SwapTabItems(source As TabItemControl, destination As TabItemControl)
        Dim sourceTabId As Short = ConvertToTabId(Me.Items.IndexOf(source))
        Dim destinationTabId As Short = ConvertToTabId(Me.Items.IndexOf(destination))

        Dim sourceTabRow = TabControlData.GetInstance.Tab.FindById(sourceTabId)
        Dim destinationTabRow = TabControlData.GetInstance.Tab.FindById(destinationTabId)

        sourceTabRow.Id = 99
        destinationTabRow.Id = sourceTabId
        sourceTabRow.Id = destinationTabId

        Dim tempHeader As Object = source.Header
        source.Header = destination.Header
        destination.Header = tempHeader

        'VBox workaround.
        Dim tempSourceContent As Object = source.Content
        Dim tempDestinationContent As Object = destination.Content
        source.Content = Nothing
        destination.Content = Nothing
        source.Content = tempDestinationContent
        destination.Content = tempSourceContent

        Me.SaveConfig()
    End Sub

    Friend Sub SaveConfig()
        ConfigManager.SaveTabControlData()
    End Sub

#Region "MainTabControl Events"
    Private Sub MainTabControl_Initialized(sender As Object, e As EventArgs)
        If DesignerProperties.GetIsInDesignMode(Me) Then Return

        Select Case (Mode)
            Case IO.FileMode.Create
                For i As Integer = 0 To NO_OF_NEW_TABS - 1
                    AddTabItem(ConvertToTabId(i), "")
                Next
            Case IO.FileMode.Open
                For i As Integer = 0 To MAX_TABS - 1
                    Dim tabRow = TabControlData.GetInstance.Tab.FindById(ConvertToTabId(i))

                    If (tabRow Is Nothing) Then Exit For

                    AddTabItem(ConvertToTabId(i), tabRow.Text)
                Next
        End Select

        If (ConfigManager.PortableConfig.TabControlDataRO) Then
            Me.ContextMenu = Nothing
        End If

        'Create Theme row if it does not exist.
        Dim themeDataTable As TabControlData.ThemeDataTable = TabControlData.GetInstance.Theme

        If themeDataTable.Count = 0 Then
            LocalTheme = ConfigManager.AppConfig.CurrentTheme
        Else
            LocalTheme = themeDataTable.First.Name
        End If
    End Sub

    Private Sub MainTabControl_Loaded(sender As Object, e As RoutedEventArgs)
        SetButtonsVisibility("")
    End Sub

    Private Sub MainTabControl_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If (ConfigManager.PortableConfig.TabControlDataRO) Then Return

        Try
            If (TypeOf CType(e.OriginalSource, FrameworkElement).TemplatedParent Is TabItemControl) Then
                RenameTabHeader()
            End If
        Catch ex As Exception
            'Ignore
        End Try
    End Sub

    ''' <summary>
    ''' Auto selects the tab where the mouse hovers.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub MainTabControl_MouseMove(sender As Object, e As MouseEventArgs)
        If (ConfigManager.PortableConfig.TabControlDataRO) Then Return
        If ConfigManager.AppConfig.LockGUI Then Return
        'Debug.Print("e.source = " & e.Source.ToString)

        If Not (TypeOf e.Source Is TabItemControl) Then Return

        Try
            If (e.LeftButton = MouseButtonState.Pressed) AndAlso
                (sourceTab IsNot Nothing) Then

                Me.Cursor = Cursors.SizeWE

                Dim tabItemLeft As Double = sourceTab.TranslatePoint(New Point(0D, 0D), Me).X
                Dim tabItemRight As Double = sourceTab.TranslatePoint(New Point(sourceTab.ActualWidth, 0D), Me).X
                Dim currentMouseX As Double = e.GetPosition(Me).X

                If (currentMouseX < tabItemLeft) AndAlso
                    (Me.Items.IndexOf(sourceTab) > 0) Then
                    'Swap with the left TabItem 
                    Dim destinationTab As TabItemControl = CType(Me.Items.Item(Me.Items.IndexOf(sourceTab) - 1), TabItemControl)

                    SwapTabItems(sourceTab, destinationTab)
                    destinationTab.IsSelected = True
                    sourceTab = destinationTab
                ElseIf (currentMouseX > tabItemRight) AndAlso
                    (Me.Items.IndexOf(sourceTab) < Me.Items.Count - 1) Then
                    'Swap with the right TabItem
                    Dim destinationTab = CType(Me.Items.Item(Me.Items.IndexOf(sourceTab) + 1), TabItemControl)

                    SwapTabItems(sourceTab, destinationTab)
                    destinationTab.IsSelected = True
                    sourceTab = destinationTab
                End If
                'Me.Cursor = Cursors.Arrow
                'e.Handled = True
            Else
                If (Me.Cursor IsNot Nothing) AndAlso (Not Me.Cursor.Equals(Cursors.Arrow)) Then
                    Me.Cursor = Cursors.Arrow
                End If
            End If
            'e.Handled = True
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_TabDragandDrop"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    Private sourceTab As TabItem = Nothing
    Private Sub MainTabControl_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If (ConfigManager.PortableConfig.TabControlDataRO) Then Return

        If Not (TypeOf e.Source Is TabItemControl) Then Return

        Dim currentTab = CType(Me.SelectedItem, TabItemControl)
        Dim tabItemBottom As Double = currentTab.TranslatePoint(New Point(0D, currentTab.ActualHeight), Me).Y
        Dim currentMouseY As Double = e.GetPosition(Me).Y

        If (currentTab.IsMouseOver) AndAlso (currentMouseY < tabItemBottom) Then
            sourceTab = currentTab
        End If
    End Sub

    Private Sub MainTabControl_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        If (ConfigManager.PortableConfig.TabControlDataRO) Then Return

        If Not (TypeOf e.Source Is TabItemControl) Then Return

        If (sourceTab IsNot Nothing) Then
            sourceTab = Nothing
            Me.Cursor = Cursors.Arrow
        End If
    End Sub

    ''' <summary>
    ''' Selects tabs when mouse wheel is turned.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub MainTabControl_MouseWheel(sender As Object, e As MouseWheelEventArgs)
        If (e.Delta < 0) Then
            If (Me.SelectedIndex < Me.Items.Count - 1) Then
                'Dim tiControl = CType(Me.SelectedItem, TabItem)
                Me.SelectedItem.MoveFocus(New TraversalRequest(FocusNavigationDirection.Right))
            End If
        Else
            If (Me.SelectedIndex > 0) Then
                'Dim tiControl = CType(Me.SelectedItem, TabItem)
                Me.SelectedItem.MoveFocus(New TraversalRequest(FocusNavigationDirection.Left))
            End If
        End If
    End Sub

    Private Sub MainTabControl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not DesignerProperties.GetIsInDesignMode(Me) Then
            Dim selectedTab = TryCast(SelectedItem, TabItemControl)
            If selectedTab IsNot Nothing Then
                If Not selectedTab.IsMouseOver Then MoveCursor()
            End If
        End If
    End Sub

#End Region

#Region "ContextMenu Events"

    Private Sub PasteContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Try
            'Prompt confirmation
            If (MessageBoxML.Show(String.Format(Localization.GetString("String_ReplaceTab"),
                                  ConvertToTabId(Me.SelectedIndex),
                                  CType(Me.SelectedItem, TabItemControl).Header),
                                  Localization.GetString("String_Replace"), MsgBoxStyle.YesNo,
                                  MessageBoxImage.Question, MessageBoxResult.No) <> MsgBoxResult.Yes) Then Return

            If (Clipboard.ContainsData(MLTabData.DataFormat)) Then
                SetTabData(CType(Clipboard.GetData(MLTabData.DataFormat), MLTabData))
                SetButtonsVisibility("")
            End If
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_Paste"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Enable/disable context menu items when opening.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub MainTabControl_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs)
        'Only open context menu if mouse is over a TabItemControl (The tab area and not the content area).
        If Not (TypeOf CType(e.OriginalSource, FrameworkElement).TemplatedParent Is TabItemControl) Then e.Handled = True

        'Enable/Disable Add
        If (Me.Items.Count < MAX_TABS) Then
            addContextMenuItem.IsEnabled = True
        Else
            addContextMenuItem.IsEnabled = False
        End If

        'Enable/Disable Delete
        If (Me.Items.Count > 1) Then
            deleteContextMenuItem.IsEnabled = True
        Else
            deleteContextMenuItem.IsEnabled = False
        End If

        'Enable/Disable Paste
        If (Me.Items.Count > 1) Then
            Dim clipContent = TryCast(Clipboard.GetData(MLTabData.DataFormat), MLTabData)
            If clipContent Is Nothing Then
                pasteContextMenuItem.IsEnabled = False
            Else
                pasteContextMenuItem.IsEnabled = True
            End If
        Else
            pasteContextMenuItem.IsEnabled = False
        End If
    End Sub

    ''' <summary>
    ''' Adds a tab.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub AddContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        AddTabItem(ConvertToTabId(Me.Items.Count), "")
        SetButtonsVisibility("")
    End Sub

    ''' <summary>
    ''' Deletes a tab.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DeleteContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        If (Me.Items.Count > 1) Then
            If (MsgBoxResult.Yes = MessageBoxML.Show(String.Format(Localization.GetString("String_DeleteTabWarning"),
                                                            CType(Me.SelectedItem, TabItem).Header),
                                                           Localization.GetString("String_DeleteTab"),
                                                           MessageBoxButton.YesNo, MessageBoxImage.Warning,
                                                           MessageBoxResult.No)) Then
                'Delete data first before deleting tab.
                '// The line below removes the row but does not change its state and therefore does not change HasChanges() state.
                '// TabControlData.GetInstance.Tab.RemoveTabRow(TabControlData.GetInstance.Tab.FindById(ConvertToTabId(Me.SelectedIndex))) 
                Dim row = TabControlData.GetInstance.Tab.FindById(ConvertToTabId(Me.SelectedIndex))
                row.Delete()

                Me.Items.Remove(Me.SelectedItem)

                'Update dataset and tab numbers.
                For i = 0 To Me.Items.Count - 1
                    Dim tabKey = CType(Me.Items.GetItemAt(i), TabItemControl).Key
                    If Not (ConvertToTabId(i) = tabKey) Then
                        Dim tabRow = TabControlData.GetInstance.Tab.FindById(tabKey)
                        tabRow.Id = ConvertToTabId(i)
                        CType(Me.Items.GetItemAt(i), TabItemControl).Key = ConvertToTabId(i)
                    End If
                Next
                Me.SaveConfig()
            End If
        End If
    End Sub

    Private Sub ClearContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        If (MsgBoxResult.Yes = MessageBoxML.Show(String.Format(Localization.GetString("String_ClearTabWarning"),
                                                            CType(Me.SelectedItem, TabItem).Header),
                                                           Localization.GetString("String_ClearTab"),
                                                           MessageBoxButton.YesNo, MessageBoxImage.Warning,
                                                           MessageBoxResult.No)) Then
            SetTabData(Nothing)
        End If
    End Sub

    Private Sub CopyContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Try
            Clipboard.SetData(MLTabData.DataFormat, GetTabData)
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_Copy"), MessageBoxButton.OK,
                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    Private Sub SortByNameContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        SortButtons(MLButtonData.SortType.Name)
    End Sub

    Private Sub SortByTargetContextMenuItem_Click(sender As Object, e As RoutedEventArgs)
        SortButtons(MLButtonData.SortType.Target)
    End Sub

#End Region

End Class

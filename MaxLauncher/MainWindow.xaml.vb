'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.IO
Imports System.Windows.Interop
Imports MaxLauncher.Utility

Public Enum HOTKEYID
    ID1 = 100
End Enum

Class MainWindow
    Inherits Window
    Implements IWin32Window

    Private mainTabControl As MainTabControl

    'Used by PressKeyTwiceToLaunch.
    Private LastActiveTab As Integer = -1
    Private LastScanCode As Integer = -1

    'When AutoSelectTab is on, prevent the selection of an inactive tab if the mouse is over it when the main window's state changes to Normal.
    Private mainWindowFirstShown As Boolean

    Private ReadOnly DEFAULT_MLD_FILE As String = "Main.mld"

    Public IgnoreMinimizeOnClose As Boolean = False

    Private InitialLoad As Boolean = True

#Region "Properties"
    ''' <summary>
    ''' Enables FileDialogs to set a WPF window as owner.
    ''' </summary>
    ''' <value>IWin32Window.Handle</value>
    ''' <returns>IWin32Window.Handle of the WPF window.</returns>
    ''' <remarks>Fixes bug in XP: when window is minimized using auto-hide or WIN+D, taskbar will not restore window.</remarks>
    Public ReadOnly Property Handle As IntPtr Implements IWin32Window.Handle
        Get
            Dim interopHelper = New WindowInteropHelper(Me)
            Return interopHelper.Handle
        End Get
    End Property

#End Region

#Region "MainWindow"

#Region "Events"

    Private Sub MainGridSplitter_LostKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs)
        ConfigManager.SaveAppCfg()
    End Sub

    Private Sub MainGridSplitter_DragCompleted(sender As Object, e As Primitives.DragCompletedEventArgs)
        ConfigManager.SaveAppCfg()
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If ConfigManager.AppConfig.MinimizeOnClose AndAlso Not IgnoreMinimizeOnClose Then
            e.Cancel = True
            Me.WindowState = Windows.WindowState.Minimized
        Else
            If ConfigManager.WindowConfig.WinLeft <> RestoreBounds.Left OrElse
                ConfigManager.WindowConfig.WinTop <> RestoreBounds.Top OrElse
                ConfigManager.WindowConfig.WinWidth <> RestoreBounds.Width OrElse
                ConfigManager.WindowConfig.WinHeight <> RestoreBounds.Height Then

                ConfigManager.WindowConfig.WinLeft = RestoreBounds.Left
                ConfigManager.WindowConfig.WinTop = RestoreBounds.Top
                ConfigManager.WindowConfig.WinWidth = RestoreBounds.Width
                ConfigManager.WindowConfig.WinHeight = RestoreBounds.Height

                ConfigManager.SaveWindowCfg()
                ConfigManager.SaveIconCacheDB()
            End If
        End If
    End Sub

    Private Sub MainWindow_Closed(sender As Object, e As EventArgs)
        If (AppNotifyIcon.NotifyIcon IsNot Nothing) Then AppNotifyIcon.NotifyIcon.Dispose()
    End Sub

    Private Sub MainWindow_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        If (Me.WindowState = Windows.WindowState.Normal) Then CenterOnScreen()
    End Sub

    Private Sub MainWindow_LocationChanged(sender As Object, e As EventArgs)
        If (Me.WindowState = Windows.WindowState.Normal) Then CenterOnScreen()
    End Sub

    Private Sub MainWindow_Initialized(sender As Object, e As EventArgs)
        Try
            Me.Top = ConfigManager.WindowConfig.WinTop
            Me.Left = ConfigManager.WindowConfig.WinLeft
            Me.Width = ConfigManager.WindowConfig.WinWidth
            Me.Height = ConfigManager.WindowConfig.WinHeight
        Catch ex As Exception
            Me.Top = 0
            Me.Left = 0
            Me.Width = 800
            Me.Height = 600
        End Try

        If (ConfigManager.PortableConfig.ApplicationConfigFileRO) Then
            mainMenuDockPanel.Visibility = Windows.Visibility.Collapsed
            mainGridSplitter.Visibility = Windows.Visibility.Collapsed
        End If

        'Initialize application icon.
        Try
            Dim customIcon As String = Path.Combine(My.Application.Info.DirectoryPath, "maxlauncher.ico")
            Dim iconSource As String = Nothing

            'Use custom icon if found. Otherwise, use icon in exe file.
            If (System.IO.File.Exists(customIcon)) Then
                iconSource = customIcon
            Else
                iconSource = Path.Combine(My.Application.Info.DirectoryPath, My.Application.Info.AssemblyName & ".exe")
            End If

            Utility.Imaging.ApplicationIcon = Utility.Imaging.GetIconFromIconFile(iconSource, 0)

            Me.Icon = Utility.Imaging.GetAppIconImage
        Catch ex As Exception
            'Ignore and use default.
        End Try

        For Each startupArg As String In My.Application.StartupArgs
            If String.Compare(startupArg, "-DefaultTheme", True) = 0 Then
                ConfigManager.AppConfig.CurrentTheme = ThemeDialog.DefaultThemeName
            End If
        Next
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs)
        Me.DataContext = ConfigManager.AppConfig

        'Apply language.
        Try
            MaxLauncher.Localization.ApplyLanguage(ConfigManager.AppConfig.Language)
        Catch ex As Exception
            ConfigManager.AppConfig.Language = Localization.DefaultLanguage
            MaxLauncher.Localization.ClearCustomLanguage()
            MessageBoxML.Show(ex.Message, Localization.GetString("String_Language"), MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try

        'Initialize IconCache
        Try
            If (File.Exists(ConfigManager.PortableConfig.IconCacheFile)) Then
                IconCacheDB.GetInstance.Open(ConfigManager.PortableConfig.IconCacheFile)
            Else
                If (Not ConfigManager.PortableConfig.IconCacheFileRO) Then _
                    IconCacheDB.GetInstance.Save(ConfigManager.PortableConfig.IconCacheFile)
            End If
        Catch ex As Exception
            MessageBoxML.Show(ex.Message & " " & Localization.GetString("MessageBox_Message_OKtocontinue"),
                              Localization.GetString("MessageBox_Title_InitializingIconCacheDB"),
                              MessageBoxButton.OK,
                              MessageBoxImage.Exclamation, MessageBoxResult.OK, ex.ToString)
        End Try

        'Initialize FavoritesBar. Terminates program if an exception is encountered.
        Try
            InitializeFavoritesBar()
        Catch ex As Exception
            MessageBoxML.Show(ex.Message & " " & Localization.GetString("MessageBox_Message_OKtoexit"),
                            Localization.GetString("MessageBox_Title_InitializingFavoritesBar"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            ExitApp()
            Return
        End Try

        'Initialize Separator
        mainGridSplitter.IsEnabled = Not ConfigManager.AppConfig.LockGUI

        'Save app.cfg if none exist.
        'Logoff or Shutdown causes an exception if app.cfg does not exist.
        If Not (System.IO.File.Exists(ConfigManager.PortableConfig.ApplicationConfigFile)) Then ConfigManager.SaveAppCfg()

        'TEMP - display version number.
#If DEBUG Then
        Dim vInfo = New TextBlock With {
            .Text = My.Application.Info.Version.ToString
        }
        mainMenu.Items.Insert(mainMenu.Items.Count - 1, vInfo)
#End If

        'Show/Hide menu
        If (ConfigManager.AppConfig.HideMenu) Then mainMenu.Visibility = Windows.Visibility.Collapsed

        'Apply theme.
        Try
            ThemeDialog.ApplyTheme(ConfigManager.AppConfig.CurrentTheme)
        Catch ex As Exception
            ConfigManager.AppConfig.CurrentTheme = ThemeDialog.DefaultThemeName
            MessageBoxML.Show(ex.Message, Localization.GetString("String_Theme"), MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try

        'Open initial file.
        Dim fileToOpen As String = Nothing

        If (ConfigManager.AppConfig.OpenLastFileUsed) Then fileToOpen = ConfigManager.AppConfig.LastFileUsed

        'Use args if there is one, else open last file used.
        If (My.Application.StartupArgs.Length > 0) Then
            For Each arg As String In My.Application.StartupArgs
                If Not arg.StartsWith("-") Then fileToOpen = arg
            Next
        End If

        If Not (String.IsNullOrEmpty(fileToOpen)) AndAlso File.Exists(fileToOpen) Then
            Try
                FileOpen(fileToOpen)
            Catch ex As Exception
                Me.WindowState = WindowState.Normal
                MessageBoxML.Show(ex.Message, Localization.GetString("String_OpenFile"), MessageBoxButton.OK,
                      MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            End Try
        Else
            'Set file location.
            If ConfigManager.IsPortable Then
                fileToOpen = Path.Combine(My.Application.Info.DirectoryPath, DEFAULT_MLD_FILE)
            Else
                fileToOpen = System.IO.Path.Combine(Environment.GetFolderPath(
                                                        Environment.SpecialFolder.MyDocuments),
                                                        My.Application.Info.AssemblyName)
                fileToOpen = System.IO.Path.Combine(fileToOpen, DEFAULT_MLD_FILE)
            End If

            'Open default MLD file if it exists. Otherwise, create then open.
            If File.Exists(fileToOpen) Then
                'Try to open.
                Try
                    FileOpen(fileToOpen)
                Catch ex As Exception
                    Me.WindowState = WindowState.Normal
                    MessageBoxML.Show(ex.Message, Localization.GetString("String_OpenFile"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
                End Try
            Else
                Try
                    FileNew(fileToOpen)
                Catch ex As Exception
                    MessageBoxML.Show(ex.Message, Localization.GetString("String_NewFile"), MessageBoxButton.OK,
                             MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
                End Try
            End If
        End If

        'Set initial window state.
        If (ConfigManager.AppConfig.StartMinimized) Then
            Me.WindowState = Windows.WindowState.Minimized
            If Not ConfigManager.AppConfig.ShowInTaskbar Then Me.ShowInTaskbar = False
            Me.WindowStyle = WindowStyle.ToolWindow
        Else
            Me.WindowState = Windows.WindowState.Normal
        End If

        'Show notify icon.
        AppNotifyIcon.NotifyIcon()

        'Register hotkey.
        If (ConfigManager.AppConfig.Hotkey1 <> 0) Then
            Dim hotkey1 As New Hotkey(Me, HOTKEYID.ID1, ConfigManager.AppConfig.Hotkey1)

            If Not (hotkey1.Register()) Then
                MessageBoxML.Show(String.Format(Localization.GetString("String_HotkeyRegistrationFailed"),
                                    hotkey1.ToString()),
                                    Localization.GetString("String_Hotkey"), MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End If

        'Add handler to process hotkey messages.
        AddHandler ComponentDispatcher.ThreadPreprocessMessage, AddressOf ComponentDispatcher_ThreadPreprocessMessage

        'Set active tab.
        ActivateTab()

        If (ConfigManager.AppConfig.CheckUpdate) Then
            Dim update As New Update
            If (update.Check = CHECKUPDATE.AVAILABLE) Then

                If (MessageBoxML.Show($"{Localization.GetString("String_NewVersionAvailable")}{vbCrLf}[ {update.NewVersion} ]{vbCrLf}{vbCrLf}{Localization.GetString("String_VisitWebsite")}",
                            Localization.GetString("String_Update"),
                            MsgBoxStyle.YesNo,
                            MessageBoxImage.Information, MessageBoxResult.No) = MsgBoxResult.Yes) Then
                    Try
                        Process.Start(My.Settings.AppWebsite)
                    Catch ex As Exception
                    End Try
                End If
            End If
        End If

        SearchTextBox_Reset()

    End Sub

    Private Sub MainWindow_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If (Input.Keyboard.Modifiers = ModifierKeys.Alt) Then
            If (ConfigManager.AppConfig.HideMenu) Then
                If Not mainMenu.IsVisible Then mainMenu.Visibility = Visibility.Visible
            End If
        End If

        If (Input.Keyboard.Modifiers = ModifierKeys.Control) AndAlso e.Key = Input.Key.F Then
            If mainMenu.IsVisible Then
                searchTextBox.Focus()
                Return
            End If
        End If

        'Ignore custom keypress actions if the menu is active.
        If (mainMenu.IsKeyboardFocusWithin) Then Return

        If (searchTextBox.IsKeyboardFocusWithin) Then Return

        If Not (Input.Keyboard.Modifiers = ModifierKeys.None) Then Return

        '======================
        'TabControl Key Pressed
        '======================
        If (mainTabControl IsNot Nothing) Then
            'Support for numbers in 10-key
            If ((e.Key >= Input.Key.NumPad0) And (e.Key <= Input.Key.NumPad9)) Then
                Dim tabNo As Integer = e.Key - Input.Key.NumPad0

                tabNo = mainTabControl.ConvertToTabIndex(tabNo)

                If (tabNo < Me.mainTabControl.Items.Count) Then Me.mainTabControl.SelectedIndex = tabNo
                e.Handled = True
                Return
            ElseIf (e.Key = Input.Key.Left) Then
                'Left arrow key moves the tab selection.
                If ConfigManager.AppConfig.ArrowKeysSelectsTabs Then
                    Dim tiControl = CType(mainTabControl.SelectedItem, TabItem)
                    tiControl.MoveFocus(New TraversalRequest(FocusNavigationDirection.Left))

                    e.Handled = True
                    Return
                End If
            ElseIf (e.Key = Input.Key.Right) Then
                'Right arrow key moves the tab selection.
                If ConfigManager.AppConfig.ArrowKeysSelectsTabs Then
                    Dim tiControl = CType(mainTabControl.SelectedItem, TabItem)
                    tiControl.MoveFocus(New TraversalRequest(FocusNavigationDirection.Right))

                    e.Handled = True
                    Return
                End If
            End If
        End If

        Dim vKey As Integer = KeyInterop.VirtualKeyFromKey(e.Key)
        Dim scancode As Integer = NativeMethods.MapVirtualKey(vKey, Keyboard.MAPVK.VK_TO_VSC)

        'F10 key support
        If (e.SystemKey = Input.Key.F10) Then
            If Not (Input.Keyboard.Modifiers = ModifierKeys.None) Then Return
            scancode = 68
        End If

        'Activate corresponding control.
        If (scancode = 1) Then
            'ESCAPE key pressed.
            If Me.OwnedWindows.Count = 0 Then Me.WindowState = Windows.WindowState.Minimized
        ElseIf ((scancode >= 59) AndAlso (scancode <= 68)) Then
            'F1-F10
            If (FavoritesBar.GetInstance Is Nothing) Then Return

            Dim o As Object = LogicalTreeHelper.FindLogicalNode(FavoritesBar.GetInstance, "FB" & scancode)

            If (TypeOf o Is FavoritesButton) Then
                Dim btn As FavoritesButton = TryCast(o, FavoritesButton)
                If (btn IsNot Nothing) Then

                    'Check if PressKeyTwiceToLaunch option is true before launching.
                    If ConfigManager.AppConfig.PressKeyTwiceToLaunch Then
                        If LastScanCode = scancode Then
                            'Reset flag.
                            LastScanCode = -1
                        Else
                            'Set flag for first press.
                            LastScanCode = scancode
                            btn.Focus()
                            e.Handled = True
                            Return
                        End If
                    End If

                    btn.LaunchFile()
                    e.Handled = True
                End If
            End If

        ElseIf ((scancode >= 2) AndAlso (scancode <= 11)) Then
            If (mainTabControl Is Nothing) Then Return

            'Keys 1-0 pressed
            Dim tabNo As Integer = scancode - 2

            If (tabNo < mainTabControl.Items.Count) Then
                Dim ti = CType(mainTabControl.Items.Item(tabNo), TabItem)
                If ti.IsVisible Then ti.IsSelected = True
                e.Handled = True
            End If

        ElseIf (((scancode >= 16) AndAlso (scancode <= 25)) OrElse
       ((scancode >= 30) AndAlso (scancode <= 39)) OrElse
       ((scancode >= 44) AndAlso (scancode <= 53))) Then

            If (mainTabControl Is Nothing) AndAlso (mainTabControl.Items.Count < 1) Then Return

            'Letter keys and symbols
            Dim o As Object = LogicalTreeHelper.FindLogicalNode(mainTabControl.SelectedContent, "TB" & scancode)

            If (TypeOf o Is TabButton) Then
                Dim btn As TabButton = TryCast(o, TabButton)
                If (btn IsNot Nothing) Then

                    'Check if PressKeyTwiceToLaunch option is true before launching.
                    If ConfigManager.AppConfig.PressKeyTwiceToLaunch Then
                        If LastScanCode = scancode And LastActiveTab = mainTabControl.SelectedIndex Then
                            'Reset flags.
                            LastActiveTab = -1
                            LastScanCode = -1
                        Else
                            'Set flags for first press.
                            LastActiveTab = mainTabControl.SelectedIndex
                            LastScanCode = scancode
                            btn.Focus()
                            e.Handled = True
                            Return
                        End If
                    End If

                    If btn.GetButtonRow IsNot Nothing AndAlso btn.IsVisible Then
                        btn.LaunchFile()
                        e.Handled = True
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub MainWindow_StateChanged(sender As Object, e As EventArgs)
        If Me.WindowState = WindowState.Minimized Then
            If ConfigManager.AppConfig.ShowInTaskbar = False Then
                Me.ShowInTaskbar = False
                Me.WindowStyle = WindowStyle.ToolWindow
            End If

        Else
            If Not Me.ShowInTaskbar Then Me.ShowInTaskbar = True
            Me.WindowStyle = WindowStyle.SingleBorderWindow

            If ConfigManager.AppConfig.ClearSearchBox Then
                SearchTextBox_Reset()
                If searchTextBox.IsKeyboardFocusWithin AndAlso mainTabControl IsNot Nothing Then mainTabControl.Focus()
            End If

            ActivateTab()
            CenterOnScreen()

            'When AutoSelectTab is on, prevent the selection of an inactive tab if the mouse is over it when the main window's state changes to Normal.
            If ConfigManager.AppConfig.AutoSelectTab Then
                mainWindowFirstShown = True
                If (mainTabControl IsNot Nothing) Then
                    mainTabControl.MoveCursor()
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Hotkey event handler
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <param name="handled"></param>
    ''' <remarks></remarks>
    Sub ComponentDispatcher_ThreadPreprocessMessage(ByRef msg As MSG, ByRef handled As Boolean)
        If (msg.message = Hotkey.HK_MSG_ID) Then
            Dim id As IntPtr = msg.wParam

            Select Case id
                Case HOTKEYID.ID1
                    If (Me.WindowState = Windows.WindowState.Minimized) Then
                        Me.WindowState = WindowState.Normal
                    Else
                        If Not ConfigManager.AppConfig.AutoHide AndAlso Not Me.IsActive Then
                            Me.Activate()
                        Else
                            If Me.OwnedWindows.Count = 0 Then
                                Me.WindowState = WindowState.Minimized
                            End If
                        End If
                    End If
            End Select
        End If
    End Sub

    'When AutoSelectTab is on, prevent the selection of an inactive tab if the mouse is over it when the main window's state changes to Normal.
    Private Sub MainWindow_PreviewMouseMove(sender As Object, e As MouseEventArgs)
        If mainWindowFirstShown Then
            If ConfigManager.AppConfig.AutoSelectTab Then
                e.Handled = True
                mainWindowFirstShown = False
            End If
        End If
    End Sub
#End Region

#End Region

#Region "Menu"

#Region "File"

    Private Sub FileMenuItem_SubmenuOpened(sender As Object, e As RoutedEventArgs)
        If (mainTabControl Is Nothing) Then
            fileSaveAsMenuItem.IsEnabled = False
        Else
            fileSaveAsMenuItem.IsEnabled = True
        End If
    End Sub

    Private Sub FileNew_Click(sender As Object, e As RoutedEventArgs)
        FileNew()
    End Sub

    Public Sub FileNew()
        Try
            Dim newFile = FileUtils.GetFilename(Me,
                                                IO.FileMode.Create,
                                                Localization.GetString("String_NewFile"),
                                                My.Settings.APPLICATION_FILE_EXTENSION,
                                                My.Settings.APPLICATION_FILE_FILTER)
            If (String.IsNullOrEmpty(newFile)) Then Return

            FileNew(newFile)
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_NewFile"), MessageBoxButton.OK,
                             MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Creates the TabControl using a new file.
    ''' </summary>
    ''' <remarks></remarks>   
    Public Sub FileNew(filename As String)
        If (System.IO.File.Exists(filename)) Then My.Computer.FileSystem.DeleteFile(filename)

        If (TabControlData.GetInstance.Exists) Then TabControlData.GetInstance.Save()
        TabControlData.GetInstance.Create(filename)

        CreateNewTabControl(FileMode.Create)
        TabControlData.GetInstance.Save()
        UpdateWindowTitle(filename)

        If ConfigManager.AppConfig.OpenLastFileUsed Then ConfigManager.SaveAppCfg()
    End Sub

    Private Sub FileOpen_Click(sender As Object, e As RoutedEventArgs)
        FileOpen()
    End Sub

    Public Sub FileOpen()
        Try
            Dim openFile = FileUtils.GetFilename(Me,
                                                 IO.FileMode.Open,
                                                 Localization.GetString("String_OpenFile"),
                                                 My.Settings.APPLICATION_FILE_EXTENSION,
                                                 My.Settings.APPLICATION_FILE_FILTER)

            If Not (String.IsNullOrEmpty(openFile)) Then FileOpen(openFile)
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_OpenFile"), MessageBoxButton.OK,
                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Opens a file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub FileOpen(filename As String)
        Me.Cursor = Cursors.Wait
        Try
            If (TabControlData.GetInstance.Exists) Then TabControlData.GetInstance.Save()
            TabControlData.GetInstance.Open(filename)

            CreateNewTabControl(FileMode.Open)
            UpdateWindowTitle(filename)

            If InitialLoad Then
                InitialLoad = False
            Else
                If ConfigManager.AppConfig.OpenLastFileUsed Then ConfigManager.SaveAppCfg()
            End If
        Finally
            Me.Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub FileSaveAs_Click(sender As Object, e As RoutedEventArgs)
        Dim newFile = FileUtils.GetFilename(Me,
                                            IO.FileMode.Create,
                                            Localization.GetString("String_SaveFileAs"),
                                            My.Settings.APPLICATION_FILE_EXTENSION,
                                            My.Settings.APPLICATION_FILE_FILTER)
        If (String.IsNullOrEmpty(newFile)) Then Return

        If (TabControlData.GetInstance.Exists) Then
            Try
                TabControlData.GetInstance.Save(newFile)
                UpdateWindowTitle(newFile)

                If ConfigManager.AppConfig.OpenLastFileUsed Then ConfigManager.SaveAppCfg()
            Catch ex As Exception
                MessageBoxML.Show(ex.Message, Localization.GetString("String_SaveFileAs"), MessageBoxButton.OK,
                 MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            End Try
        End If
    End Sub

    Private Sub FileImport_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim sourceFile = FileUtils.GetFilename(Me, IO.FileMode.Open, Localization.GetString("String_ImportFile"),
                                                   My.Settings.MAL_FILE_EXTENSION, My.Settings.MAL_FILE_FILTER)
            If (String.IsNullOrEmpty(sourceFile)) Then Return

            Dim destinationFile = FileUtils.GetFilename(Me, IO.FileMode.Create, Localization.GetString("String_SaveAs"), My.Settings.APPLICATION_FILE_EXTENSION,
                                              My.Settings.APPLICATION_FILE_FILTER, Path.GetFileNameWithoutExtension(sourceFile))
            If (String.IsNullOrEmpty(destinationFile)) Then Return

            FileImport(sourceFile, destinationFile)
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_ImportFile"), MessageBoxButton.OK,
                MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    Private Sub FileImport(sourceFile As String, destinationFile As String)
        Dim fileConverter = New FileConverter

        fileConverter.TabControlData(sourceFile, destinationFile)

        Try
            FileOpen(destinationFile)
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_OpenFile"), MessageBoxButton.OK,
                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    Private Sub FileExit_Click(sender As Object, e As RoutedEventArgs)
        ExitApp()
    End Sub

    Public Sub ExitApp()
        IgnoreMinimizeOnClose = True
        Me.Close()
    End Sub

#End Region

#Region "View"

    Dim savedAutoHideValue As Boolean?
    Dim savedAlwaysOnTop As Boolean?
    Dim savedCenterOnScreen As Boolean?
    Dim savedHideEmptyTabButtons As Boolean?
    Dim savedLockGUI As Boolean?

    Private Sub ViewMenuItem_SubmenuOpened(sender As Object, e As RoutedEventArgs)
        If (mainTabControl Is Nothing) Then
            hideEmptyTabButtonsMenuItem.IsEnabled = False
            hideTabButtonsRowMenuItem.IsEnabled = False
        Else
            If Not dragAndDropModeMenuItem.IsChecked Then
                hideEmptyTabButtonsMenuItem.IsEnabled = True
                hideTabButtonsRowMenuItem.IsEnabled = True
            End If
        End If
    End Sub

    Private Sub AutoHideMenuItem_Click(sender As Object, e As RoutedEventArgs)
        ConfigManager.SaveAppCfg()
    End Sub

    Private Sub AlwaysOnTopMenuItem_Click(sender As Object, e As RoutedEventArgs)
        ConfigManager.SaveAppCfg()
    End Sub

    Private Sub CenterOnScreenMenuItem_Click(sender As Object, e As RoutedEventArgs)
        ConfigManager.SaveAppCfg()
        CenterOnScreen()
    End Sub

    Private Sub DragAndDropModeMenuItem_Checked(sender As Object, e As RoutedEventArgs)
        Dim appcfg = ConfigManager.AppConfig

        'Save view values.
        savedAutoHideValue = appcfg.AutoHide
        savedAlwaysOnTop = appcfg.AlwaysOnTop
        savedCenterOnScreen = appcfg.CenterOnScreen
        savedLockGUI = appcfg.LockGUI
        savedHideEmptyTabButtons = appcfg.HideEmptyTabButtons

        'Set options to false.
        appcfg.AutoHide = False
        appcfg.AlwaysOnTop = False
        appcfg.CenterOnScreen = False
        appcfg.LockGUI = False
        appcfg.HideEmptyTabButtons = False

        'Disable view items.
        autoHideMenuItem.IsEnabled = False
        alwaysOnTopMenuItem.IsEnabled = False
        centerOnScreenMenuItem.IsEnabled = False
        lockGUIMenuItem.IsEnabled = False
        hideEmptyTabButtonsMenuItem.IsEnabled = False
        If (mainTabControl IsNot Nothing) Then mainTabControl.SetButtonsVisibility(String.Empty)

        hideTabButtonsRowMenuItem.IsEnabled = False

        'Disable File, Data and Settings menu items.
        fileMenuItem.IsEnabled = False
        dataMenuItem.IsEnabled = False
        settingsMenuItem.IsEnabled = False
    End Sub

    Private Sub DragAndDropModeMenuItem_Unchecked(sender As Object, e As RoutedEventArgs)
        Dim appcfg = ConfigManager.AppConfig

        'Retore options to original values.
        appcfg.AutoHide = savedAutoHideValue
        appcfg.AlwaysOnTop = savedAlwaysOnTop
        appcfg.CenterOnScreen = savedCenterOnScreen
        appcfg.HideEmptyTabButtons = savedHideEmptyTabButtons
        appcfg.LockGUI = savedLockGUI

        'Enable view items.
        autoHideMenuItem.IsEnabled = True
        alwaysOnTopMenuItem.IsEnabled = True
        centerOnScreenMenuItem.IsEnabled = True
        CenterOnScreen()

        lockGUIMenuItem.IsEnabled = True
        hideEmptyTabButtonsMenuItem.IsEnabled = True
        If (mainTabControl IsNot Nothing) Then mainTabControl.SetButtonsVisibility(searchTextBox.Text)

        hideTabButtonsRowMenuItem.IsEnabled = True

        'Enable File, Data and Settings menu items.
        fileMenuItem.IsEnabled = True
        dataMenuItem.IsEnabled = True
        settingsMenuItem.IsEnabled = True

    End Sub

    Private Sub LockGUIMenuItem_Click(sender As Object, e As RoutedEventArgs)
        ConfigManager.SaveAppCfg()

        If lockGUIMenuItem.IsChecked Then
            mainGridSplitter.IsEnabled = False
            Me.ResizeMode = ResizeMode.CanMinimize
        Else
            mainGridSplitter.IsEnabled = True
            Me.ResizeMode = ResizeMode.CanResize
            MainWindow_SourceInitialized(Me, EventArgs.Empty)
        End If
    End Sub

    Private Sub HideEmptyTabButtonsMenuItem_Click(sender As Object, e As RoutedEventArgs)
        ConfigManager.SaveAppCfg()
        If (mainTabControl IsNot Nothing) Then mainTabControl.SetButtonsVisibility(searchTextBox.Text)
    End Sub

    Private Sub HideTabButtonsRowMenuItem_Click(sender As Object, e As RoutedEventArgs)
        ConfigManager.SaveAppCfg()
    End Sub

#End Region

#Region "DataMenuItem"

    ''' <summary>
    ''' Adds the current data file to the list under the Data menu item.
    ''' </summary>
    Private Sub DataAddMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim currentFile As String = FileUtils.Path.ConvertToRelativePath(TabControlData.GetInstance.Filename)
        Dim header As String = "_" & System.IO.Path.GetFileNameWithoutExtension(currentFile)

        Dim df As New AppConfig.DataFile(header, currentFile)
        ConfigManager.AppConfig.DataFiles.Add(df)
        DataFileAdd(header)
        ConfigManager.SaveAppCfg()
    End Sub

    ''' <summary>
    ''' Shows the Data File Organizer dialog box.
    ''' </summary>
    Private Sub DataOrganizeMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim dlg As New DataFilesDialog With {
            .Owner = Me
        }
        dlg.ShowDialog()
        ConfigManager.SaveAppCfg()
    End Sub

    Private Sub DataImportMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Me.Cursor = Cursors.Wait
        Try
            Dim srcFile = FileUtils.GetFilename(Me,
                                                 IO.FileMode.Open,
                                                 Localization.GetString("String_OpenFile"),
                                                 My.Settings.APPLICATION_FILE_EXTENSION,
                                                 My.Settings.APPLICATION_FILE_FILTER)
            Dim destFile = TabControlData.GetInstance.Filename

            If Not (String.IsNullOrEmpty(srcFile)) And Not (String.IsNullOrEmpty(destFile)) Then
                Dim fileImporter As New FileImporter
                fileImporter.ImportTabControlData(srcFile, destFile)
                FileOpen(destFile)

                Dim explorerPath = Environment.GetEnvironmentVariable("windir") & "\explorer.exe "
                Interaction.Shell(explorerPath & fileImporter.GetLog.FileName, AppWinStyle.NormalFocus)
            End If
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_OpenFile"), MessageBoxButton.OK,
                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        Finally
            Me.Cursor = Cursors.Arrow
        End Try
    End Sub


    ''' <summary>
    ''' Adds the current data file to the data file list under the Data menu item.
    ''' </summary>
    ''' <param name="menuItemHeader">String representation of the data file</param>
    Private Sub DataFileAdd(ByVal menuItemHeader As String)
        Dim item As New MenuItem With {
            .Header = menuItemHeader
        }
        AddHandler item.Click, AddressOf DataFileItem_Click
        dataMenuItem.Items.Add(item)
    End Sub

    ''' <summary>
    ''' Data MenuItem Click event handler.
    ''' Opens the data file that corresponds to the menu item clicked under the Data menu item.
    ''' </summary>
    Private Sub DataFileItem_Click(sender As Object, e As RoutedEventArgs)
        Dim menuItemHeader As String = DirectCast(sender, MenuItem).Header

        Dim dataFile As AppConfig.DataFile = ConfigManager.AppConfig.DataFiles.Find(Function(x) String.Compare(x.menuname, menuItemHeader) = 0)

        Try
            FileOpen(dataFile.filename)
        Catch ex As Exception
            MessageBoxML.Show(ex.Message, Localization.GetString("String_OpenFile"), MessageBoxButton.OK,
                  MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Data MenuItem SubMenuOpened event handler.
    ''' </summary>
    Private Sub DataMenuItem_SubmenuOpened(sender As Object, e As RoutedEventArgs)
        If (mainTabControl Is Nothing) Then
            dataAddMenuItem.IsEnabled = False
        Else
            dataAddMenuItem.IsEnabled = True
        End If

        If ConfigManager.AppConfig.DataFiles.Count = 0 Then
            dataOrganizeMenuItem.IsEnabled = False
        Else
            dataOrganizeMenuItem.IsEnabled = True
        End If

        DataMenuItemsRefresh()
    End Sub

    ''' <summary>
    ''' Refreshes the data file entries under the Data menu item.
    ''' </summary>
    Private Sub DataMenuItemsRefresh()
        While (dataMenuItem.Items.Count > 4)
            RemoveHandler DirectCast(dataMenuItem.Items.Item(4), MenuItem).Click, AddressOf DataFileItem_Click
            dataMenuItem.Items.RemoveAt(4)
        End While

        If (ConfigManager.AppConfig.DataFiles Is Nothing) Then Return

        Dim df As AppConfig.DataFile = Nothing
        Try
            For Each df In ConfigManager.AppConfig.DataFiles
                DataFileAdd(df.menuname)
            Next
        Catch ex As Exception
            MessageBoxML.Show(Localization.GetString("String_ErrorLoadingDataFile") & df.menuname,
                              Localization.GetString("String_DataFiles"), MessageBoxButton.OK,
                MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
        End Try

    End Sub

#End Region

#Region "ToolsMenuItem"

    Private Sub ToolsMenuItem_SubmenuOpened(sender As Object, e As RoutedEventArgs)
        If (mainTabControl Is Nothing) Then
            toolsViewGroupLaunchListMenuItem.IsEnabled = False
        Else
            toolsViewGroupLaunchListMenuItem.IsEnabled = True
        End If

        If System.Environment.OSVersion.Version >= New Version("6.2.0.0") Then
            toolsOpenAppsFolderMenuItem.Visibility = Visibility.Visible
        Else
            toolsOpenAppsFolderMenuItem.Visibility = Visibility.Collapsed
        End If

    End Sub

    Private Sub ToolsOpenUserStartMenu_Click(sender As Object, e As RoutedEventArgs)
        Using p As New Process
            p.StartInfo.LoadUserProfile = False
            p.StartInfo.UseShellExecute = True
            p.StartInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
            p.Start()
        End Using
    End Sub

    Private Sub ToolsOpenAllUsersStartMenu_Click(sender As Object, e As RoutedEventArgs)
        Dim folderPath As String = ""

        Dim startMenuPath As String
        Dim osVersion As System.OperatingSystem = System.Environment.OSVersion
        If osVersion.Version.Major = 5 Then
            'For XP
            startMenuPath = Environment.ExpandEnvironmentVariables("%ALLUSERSPROFILE%\Start Menu")
            If System.IO.Directory.Exists(startMenuPath) Then folderPath = startMenuPath
        Else
            startMenuPath = Environment.ExpandEnvironmentVariables("%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu")
            If System.IO.Directory.Exists(startMenuPath) Then folderPath = startMenuPath
        End If

        If Not String.IsNullOrEmpty(folderPath) Then
            Using p As New Process
                p.StartInfo.LoadUserProfile = False
                p.StartInfo.UseShellExecute = True
                p.StartInfo.FileName = folderPath
                p.Start()
            End Using
        End If
    End Sub

    Private Sub ToolsOpenControlPanel_Click(sender As Object, e As RoutedEventArgs)
        Dim process As New Process

        Try
            process.StartInfo.LoadUserProfile = False
            process.StartInfo.UseShellExecute = True

            process.StartInfo.FileName = "control.exe"
            process.Start()
        Catch ex As Exception
            MessageBoxML.Show(String.Format(Localization.GetString("String_ErrorOpening") & " {1}", process.StartInfo.FileName,
                              ex.Message), Localization.GetString("String_Run"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK,
                              String.Format(Localization.GetString("String_ErrorOpening") & " {1}", process.StartInfo.FileName, vbCrLf & ex.ToString))
        Finally
            If (process IsNot Nothing) Then process.Dispose()
        End Try
    End Sub

    Private Sub ToolsOpenAppsFolder_Click(sender As Object, e As RoutedEventArgs)
        Dim process As New Process

        Try
            process.StartInfo.LoadUserProfile = False
            process.StartInfo.UseShellExecute = True

            process.StartInfo.FileName = "shell:appsfolder"
            process.Start()
        Catch ex As Exception
            MessageBoxML.Show(String.Format(Localization.GetString("String_ErrorOpening") & " {1}", process.StartInfo.FileName,
                              ex.Message), Localization.GetString("String_Run"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK,
                              String.Format(Localization.GetString("String_ErrorOpening") & " {1}", process.StartInfo.FileName, vbCrLf & ex.ToString))
        Finally
            If (process IsNot Nothing) Then process.Dispose()
        End Try
    End Sub

    Private Sub ToolsViewGroupLaunchListMenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim groupLaunchDlg As New GroupLaunchDialog() With {
            .Owner = Me
           }

        groupLaunchDlg.Show()
    End Sub

#End Region

#Region "Settings"

    Private Sub Settings_Hotkey_Click(sender As Object, e As RoutedEventArgs)
        Dim hotkey As New Hotkey(Me, HOTKEYID.ID1, ConfigManager.AppConfig.Hotkey1)
        Dim hotkeyDlg As New HotkeyDialog(hotkey) With {
            .Owner = Me
        }
        hotkeyDlg.ShowDialog()

        ConfigManager.AppConfig.Hotkey1 = hotkey.Value
        If (hotkey.Value <> 0) Then hotkey.Register()
        ConfigManager.SaveAppCfg()
    End Sub

    Private Sub Settings_Theme_Click(sender As Object, e As RoutedEventArgs)
        If ThemeDialog.IsCustomizeWindowOpen Then Return

        If ConfigManager.AppConfig.AlwaysUseApplicationTheme = True Then
            Dim themeDlg = New ThemeDialog(ConfigManager.AppConfig.CurrentTheme) With {
                .Owner = Me
            }

            themeDlg.ShowDialog()
            ConfigManager.AppConfig.CurrentTheme = themeDlg.SelectedTheme.ThemeName
            ConfigManager.SaveAppCfg()
        Else
            Dim themeDlg = New ThemeDialog(mainTabControl.LocalTheme) With {
                .Owner = Me
            }

            themeDlg.ShowDialog()
            mainTabControl.LocalTheme = themeDlg.SelectedTheme.ThemeName
            mainTabControl.SaveConfig()
        End If

    End Sub

    Private Sub Settings_Options_Click(sender As Object, e As RoutedEventArgs)
        Dim optionsDlg As New OptionsDialog() With {
            .Owner = Me
        }
        optionsDlg.ShowDialog()

        If ConfigManager.AppConfig.AlwaysUseApplicationTheme = True Then
            Try
                ThemeDialog.ApplyTheme(ConfigManager.AppConfig.CurrentTheme)
            Catch ex As Exception
                ConfigManager.AppConfig.CurrentTheme = ThemeDialog.DefaultThemeName
                ThemeDialog.ApplyTheme(ConfigManager.AppConfig.CurrentTheme)
                MessageBoxML.Show(ex.Message, Localization.GetString("String_Theme"), MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            End Try
        Else
            Try
                ThemeDialog.ApplyTheme(mainTabControl.LocalTheme)
            Catch ex As Exception
                ConfigManager.AppConfig.CurrentTheme = ThemeDialog.DefaultThemeName
                ThemeDialog.ApplyTheme(ConfigManager.AppConfig.CurrentTheme)
                MessageBoxML.Show(ex.Message, Localization.GetString("String_Theme"), MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            End Try
        End If

        'Reload notification icon.
        If (AppNotifyIcon.NotifyIcon IsNot Nothing) Then AppNotifyIcon.NotifyIcon.Dispose()
        AppNotifyIcon.NotifyIcon()

        ConfigManager.SaveAppCfg()

        If (ConfigManager.AppConfig.HideMenu) Then
            mainMenu.Visibility = Windows.Visibility.Collapsed
        Else
            mainMenu.Visibility = Windows.Visibility.Visible
        End If

        SearchTextBox_Reset()

    End Sub

#End Region

#Region "Help"

    Private Sub Help_VisitWebSite_Click(sender As Object, e As RoutedEventArgs)
        Try
            Process.Start(My.Settings.AppWebsite)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Help_CheckForUpdatesNow_Click(sender As Object, e As RoutedEventArgs)
        Dim update As New Update
        Dim result As CHECKUPDATE = CHECKUPDATE.FAILED

        Select Case (update.Check)
            Case CHECKUPDATE.AVAILABLE
                If (MessageBoxML.Show($"{Localization.GetString("String_NewVersionAvailable")}{vbCrLf}[ {update.NewVersion} ]{vbCrLf}{vbCrLf}{Localization.GetString("String_VisitWebsite")}",
                    Localization.GetString("String_Update"),
                    MsgBoxStyle.YesNo,
                    MessageBoxImage.Information, MessageBoxResult.No) = MsgBoxResult.Yes) Then
                    Try
                        Process.Start(My.Settings.AppWebsite)
                    Catch ex As Exception
                    End Try
                End If
            Case CHECKUPDATE.FAILED
            Case CHECKUPDATE.UPTODATE
                MessageBoxML.Show(My.Application.Info.Title & " " &
                    Localization.GetString("String_IsUpToDate"),
                    Localization.GetString("String_Update"),
                    MsgBoxStyle.OkOnly,
                    MessageBoxImage.Information, MessageBoxResult.OK)
        End Select

    End Sub

    Private Sub Help_ProgramInformation_Click(sender As Object, e As RoutedEventArgs)
        Dim programInformationDlg As New ProgramInformationDialog() With {
            .Owner = Me
        }
        programInformationDlg.ShowDialog()
    End Sub

    Private Sub Help_About_Click(sender As Object, e As RoutedEventArgs)
        Dim aboutDlg As New AboutDialog() With {
            .Owner = Me
        }
        aboutDlg.ShowDialog()
    End Sub

#End Region

    Private Sub MainMenu_IsKeyboardFocusWithinChanged(sender As Object, e As DependencyPropertyChangedEventArgs)
        If (ConfigManager.AppConfig.HideMenu) Then
            'If lost focus.
            If Not mainMenu.IsKeyboardFocusWithin Then mainMenu.Visibility = Windows.Visibility.Collapsed
        End If
    End Sub

#End Region

#Region "Helper Methods"

    Private Sub UpdateWindowTitle(title As String)
        Me.Title = My.Application.Info.AssemblyName & " - " & System.IO.Path.GetFullPath(title)
    End Sub

    Private Sub CreateNewTabControl(mode As FileMode)
        mainWindowGrid.Children.Remove(mainTabControl)

        mainTabControl = New MainTabControl(mode)
        Grid.SetRow(mainTabControl, 3)
        mainWindowGrid.Children.Add(mainTabControl)

        'Reset flags for PressKeyTwiceToLaunch.
        If ConfigManager.AppConfig.PressKeyTwiceToLaunch Then
            LastActiveTab = -1
            LastScanCode = -1
        End If
    End Sub

    ''' <summary>
    ''' Center window on screen.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub CenterOnScreen()
        If ConfigManager.AppConfig.CenterOnScreen Then
            Dim pt As System.Drawing.Point = System.Windows.Forms.Control.MousePosition
            Dim currentScreen As Forms.Screen = System.Windows.Forms.Screen.FromPoint(pt)

            Dim newLeft, newTop As Double
            Dim source = PresentationSource.FromVisual(Me)

            If source IsNot Nothing Then
                Dim scaledAreaWidth, scaledAreaHeight As Double

                If source.CompositionTarget.TransformToDevice.M11 > 1 Then
                    scaledAreaWidth = Me.ActualWidth * source.CompositionTarget.TransformToDevice.M11
                    scaledAreaHeight = Me.ActualHeight * source.CompositionTarget.TransformToDevice.M22

                    newLeft = (((currentScreen.WorkingArea.Width - scaledAreaWidth) / 2) / source.CompositionTarget.TransformToDevice.M11)
                    newTop = (((currentScreen.WorkingArea.Height - scaledAreaHeight) / 2) / source.CompositionTarget.TransformToDevice.M22)

                    If Not currentScreen.Primary Then
                        newLeft += (currentScreen.WorkingArea.Left / source.CompositionTarget.TransformToDevice.M11)
                        newTop += (currentScreen.WorkingArea.Top / source.CompositionTarget.TransformToDevice.M22)
                    End If
                Else
                    newLeft = ((currentScreen.WorkingArea.Width - Me.ActualWidth) / 2) + currentScreen.WorkingArea.Left
                    newTop = ((currentScreen.WorkingArea.Height - Me.ActualHeight) / 2) + currentScreen.WorkingArea.Top
                End If
            End If

            If (Me.Left <> newLeft) Then Me.Left = newLeft
            If (Me.Top <> newTop) Then Me.Top = newTop

        End If
    End Sub

    ''' <summary>
    ''' Initializes the favorites bar.
    ''' </summary>
    ''' <remarks>Loads information for each favorites button.</remarks>
    Private Sub InitializeFavoritesBar()

        FavoritesBarData.GetInstance(ConfigManager.PortableConfig.FavoritesConfigFile)
        Dim fBar = FavoritesBar.GetInstance()
        fBar.IsTabStop = False

        Grid.SetRow(fBar, 1)
        mainWindowGrid.Children.Add(fBar)
    End Sub

    ''' <summary>
    ''' Activates a tab when activated using the hotkey.
    ''' </summary>
    ''' <remarks></remarks>
    Sub ActivateTab()
        If (mainTabControl IsNot Nothing) Then
            Dim selectIndex As Integer = mainTabControl.ConvertToTabIndex(ConfigManager.AppConfig.ActivateTab)

            Try
                If (selectIndex >= 0) AndAlso (selectIndex < mainTabControl.Items.Count) Then
                    'If tab is not visible or does not exist, do nothing. This is releated to the Search feature.
                    Dim ti As TabItemControl = TryCast(mainTabControl.Items.Item(selectIndex), TabItemControl)
                    If ti IsNot Nothing AndAlso Not ti.IsVisible Then Return

                    mainTabControl.SelectedIndex = selectIndex
                End If
            Catch ex As Exception
                'Do nothing
            End Try
        End If
    End Sub

    'Disable Maximize button
    Private Const GWL_STYLE As Integer = -16
    Private Const WS_MAXIMIZEBOX As Integer = &H10000

    Private Sub MainWindow_SourceInitialized(sender As Object, e As EventArgs)
        Dim hwnd = New WindowInteropHelper(DirectCast(sender, Window)).Handle
        Dim value = NativeMethods.GetWindowLong(hwnd, GWL_STYLE)
        NativeMethods.SetWindowLong(hwnd, GWL_STYLE, CInt(value And Not WS_MAXIMIZEBOX))
    End Sub

    '<PermissionSetAttribute(SecurityAction.LinkDemand, Name:="FullTrust")>
    Public Sub LaunchButtonGroup(ByVal groupID As String)
        Dim buttonList As List(Of MButton) = GetButtonsInGroup(groupID)

        For Each b As MButton In buttonList
            Launcher.LaunchFile(b.GetButtonData, True)
        Next
    End Sub

    Public Function GetButtonsInGroup(ByVal groupID As String) As List(Of MButton)
        Dim buttonList As New List(Of MButton)

        If (FavoritesBar.GetInstance IsNot Nothing) Then

            Dim groupIDQuery As EnumerableRowCollection = Nothing

            If String.IsNullOrEmpty(groupID) Then
                groupIDQuery = From s In FavoritesBarData.GetInstance.Tables("Button").AsEnumerable
                               Where s.RowState <> DataRowState.Deleted AndAlso (s.Field(Of String)("GroupID") Is Nothing Or s.Field(Of String)("GroupID") <> "")
                               Select New With {.ID = s.Field(Of System.Int16)("ID")}
            Else
                groupIDQuery = From s In FavoritesBarData.GetInstance.Tables("Button").AsEnumerable
                               Where s.RowState <> DataRowState.Deleted AndAlso s.Field(Of String)("GroupID") = groupID
                               Select New With {.ID = s.Field(Of System.Int16)("ID")}
            End If

            If groupIDQuery IsNot Nothing Then
                For Each button In groupIDQuery
                    Dim btn As FavoritesButton = TryCast(LogicalTreeHelper.FindLogicalNode(FavoritesBar.GetInstance, "FB" & button.ID), FavoritesButton)
                    If (btn IsNot Nothing) Then buttonList.Add(btn)
                Next
            End If
        End If

        'Return if no tab or tab control exists.
        If (mainTabControl IsNot Nothing) AndAlso (mainTabControl.Items.Count > 0) Then

            Dim groupIDQuery2 As EnumerableRowCollection = Nothing

            If String.IsNullOrEmpty(groupID) Then
                groupIDQuery2 = From s In TabControlData.GetInstance.Tables("Button").AsEnumerable
                                Where s.RowState <> DataRowState.Deleted AndAlso (s.Field(Of String)("GroupID") Is Nothing Or s.Field(Of String)("GroupID") <> "")
                                Select New With {.Tab_ID = s.Field(Of System.Int16)("Tab_ID"),
                                                 .ID = s.Field(Of System.Int16)("ID")}
            Else
                groupIDQuery2 = From s In TabControlData.GetInstance.Tables("Button").AsEnumerable
                                Where s.RowState <> DataRowState.Deleted AndAlso s.Field(Of String)("GroupID") = groupID
                                Select New With {.Tab_ID = s.Field(Of System.Int16)("Tab_ID"),
                                                 .ID = s.Field(Of System.Int16)("ID")}
            End If

            If groupIDQuery2 IsNot Nothing Then
                For Each button In groupIDQuery2
                    Dim btn As TabButton = TryCast(LogicalTreeHelper.FindLogicalNode(
                        DirectCast(mainTabControl.Items.Item(mainTabControl.ConvertToTabIndex(button.Tab_ID)), TabItemControl).Content, "TB" & button.ID), TabButton)
                    If (btn IsNot Nothing) Then buttonList.Add(btn)
                Next
            End If
        End If

        Return buttonList
    End Function

#End Region

#Region "SearchTextBox"

    Private Sub SearchTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        'Return if no tab or tab control exists.
        If (mainTabControl IsNot Nothing) Then mainTabControl.SetButtonsVisibility(searchTextBox.Text)
    End Sub

    Private Sub SearchTextBox_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Input.Key.Enter OrElse e.Key = Input.Key.Escape OrElse e.Key = Input.Key.Tab Then
            If (mainTabControl IsNot Nothing) Then mainTabControl.Focus()
            e.Handled = True
        End If
    End Sub

    Private Sub SearchTextBox_GotKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs)
        If String.Compare(searchTextBox.Text, Localization.GetString("SearchBox"), False) = 0 Then
            searchTextBox.Clear()
            searchTextBox.Background = New SolidColorBrush(Colors.White)
        End If
    End Sub

    Private Sub SearchTextBox_LostKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs)
        If String.IsNullOrEmpty(searchTextBox.Text) Then SearchTextBox_Reset()
    End Sub

    Public Sub SearchTextBox_Reset()
        searchTextBox.Text = Localization.GetString("SearchBox")
        searchTextBox.Foreground = New SolidColorBrush(Colors.Gray)
        searchTextBox.Background = New SolidColorBrush(Colors.Transparent)
    End Sub

#End Region

End Class

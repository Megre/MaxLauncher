Imports System.ComponentModel
Imports System.Runtime.InteropServices

Namespace Utility

    Public Class Launcher

        Enum WindowStyleEx As Short
            WindowStyleEx_Normal = 0
            WindowStyleEx_Left = 1
            WindowStyleEx_Right = 2
            WindowStyleEx_Top = 3
            WindowStyleEx_Bottom = 4
            WindowStyleEx_TopLeft = 5
            WindowStyleEx_BottomLeft = 6
            WindowStyleEx_TopRight = 7
            WindowStyleEx_BottomRight = 8
            WindowStyleEx_Custom = 9
        End Enum

#Region "Launcher"

        Shared Sub LaunchFile(btnData As MLButtonData, Optional ByVal GroupLaunch As Boolean = False)

            If (btnData Is Nothing) Then Return

            'Return if filename is empty.
            If (btnData.Filename Is Nothing) Then Return

            If Not (GroupLaunch) Then
                If Not (String.IsNullOrEmpty(btnData.GroupID)) Then
                    DirectCast(My.Application.MainWindow, MainWindow).LaunchButtonGroup(btnData.GroupID)
                    Return
                End If
            End If

            'Open file if filename is a maxlauncher data file.
            Try
                Dim fullPath = System.IO.Path.GetFullPath(btnData.Filename)
                If String.Compare(System.IO.Path.GetExtension(btnData.Filename), My.Settings.APPLICATION_FILE_EXTENSION, True) = 0 Then
                    CType(My.Application.MainWindow, MainWindow).FileOpen(btnData.Filename)
                    Return
                End If
            Catch ex As Exception
            End Try

            Dim process As New Process
            Dim currentScreen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Control.MousePosition)

            Try
                'Convert relative title to full path.
                If btnData.IsDirectory AndAlso btnData.WindowTitle.StartsWith(IO.Path.DirectorySeparatorChar) Then
                    btnData.WindowTitle = btnData.WindowTitle.TrimStart(IO.Path.DirectorySeparatorChar)
                    btnData.WindowTitle = System.IO.Path.Combine(System.IO.Path.GetPathRoot(My.Application.Info.DirectoryPath), btnData.WindowTitle)
                End If

                'Activate and size window if window title is not empty. Otherwise, run the program.
                'Search/resize window based on title.
                If Not (String.IsNullOrEmpty(btnData.WindowTitle)) Then
                    If Launcher.ResizeWindow(FindWindowByTitle(btnData), btnData, currentScreen) Then Return
                End If

                'Configure Process
                process.StartInfo.LoadUserProfile = False
                process.StartInfo.UseShellExecute = True

                process.StartInfo.FileName = FileUtils.Path.ExpandEnvironmentVariables(btnData.Filename)
                process.StartInfo.FileName = FileUtils.Path.ReplaceTokens(process.StartInfo.FileName) 'Replace tokens.

                process.StartInfo.WorkingDirectory = FileUtils.Path.ExpandEnvironmentVariables(btnData.WorkingDirectory)

                'If btnData.Arguments is a path, convert relative path to full path.
                Dim argumentsFullPath As String = btnData.Arguments
                If btnData.IsDirectory Then argumentsFullPath = FileUtils.Path.ConvertToFullPath(argumentsFullPath)

                process.StartInfo.Arguments = FileUtils.Path.ExpandEnvironmentVariables(argumentsFullPath)
                process.StartInfo.Arguments = FileUtils.Path.ReplaceTokens(process.StartInfo.Arguments) 'Replace tokens.

                If btnData.IsDirectory AndAlso ConfigManager.AppConfig.FolderPathAsTarget Then
                    If Not String.IsNullOrEmpty(process.StartInfo.Arguments) Then
                        process.StartInfo.FileName = process.StartInfo.Arguments
                        process.StartInfo.Arguments = ""
                    End If
                End If

                process.StartInfo.WindowStyle = CType(btnData.WindowStyle, ProcessWindowStyle)

                'Run as administrator support.
                If (btnData.RunAsAdmin) Then process.StartInfo.Verb = "runas"

                process.Start()

                Dim t As System.Threading.Thread = New System.Threading.Thread(
                            DirectCast(Function() StartResizeThread(btnData, currentScreen), System.Threading.ThreadStart))
                t.Start()

                'Force minimize MainWindow. Launching apps without a window does not deactivate MainWindow.
                If (ConfigManager.AppConfig.AutoHide) Then
                    My.Application.MainWindow.WindowState = WindowState.Minimized
                End If
            Catch ex As Win32Exception
                'Exception is thrown when UAC is cancelled by the user.
                If (ex.NativeErrorCode <> 1223) Then
                    MessageBoxML.Show(String.Format(Localization.GetString("String_ErrorOpening") & " {1}", process.StartInfo.FileName,
                                      ex.Message), Localization.GetString("String_Run"), MessageBoxButton.OK,
                                      MessageBoxImage.Error, MessageBoxResult.OK,
                                      String.Format(Localization.GetString("String_ErrorOpening") & " {1}", process.StartInfo.FileName, vbCrLf & ex.ToString))
                End If
            Catch ex As Exception
                MessageBoxML.Show(String.Format(Localization.GetString("String_ErrorOpening") & " {1}", process.StartInfo.FileName,
                                  ex.Message), Localization.GetString("String_Run"), MessageBoxButton.OK,
                                  MessageBoxImage.Error, MessageBoxResult.OK,
                                  String.Format(Localization.GetString("String_ErrorOpening") & " {1}", process.StartInfo.FileName, vbCrLf & ex.ToString))
            Finally
                If (process IsNot Nothing) Then process.Dispose()
            End Try
        End Sub


#End Region

#Region "Window resizing/moving"

        'Returns false if window is not found.
        Shared Function ResizeWindow(hwnd As System.IntPtr, ByVal mlButtonData As MLButtonData, ByVal currentScreen As System.Windows.Forms.Screen) As Boolean
            If hwnd = IntPtr.Zero Then Return False
            If String.IsNullOrEmpty(mlButtonData.WindowTitle) Then Return False

            Dim IsResized As Boolean

            If (mlButtonData.WindowStyle = ProcessWindowStyle.Normal) Then
                If (mlButtonData.WindowStyleEx = WindowStyleEx.WindowStyleEx_Normal) Then
                    'Restore window only without moving.
                    'If minimized, restore but maintain window state (ie. maximized or normal).
                    Dim winStyle = NativeMethods.GetWindowLong(hwnd, -16) '-16 = retrieve window styles.
                    If (winStyle And &H20000000L) = &H20000000L Then _
                            NativeMethods.ShowWindow(hwnd, WindowShowStyle.ShowNormalNoActivate) '&H20000000L = WS_MINIMIZE
                    'ShowWindow(hwnd, WindowShowStyle.ShowNormalNoActivate)
                    NativeMethods.SetForegroundWindowNative(hwnd)
                    IsResized = True
                Else
                    'MOVE A WINDOW TO A SCREEN AND CHANGE ITS ACTUAL SIZE AND LOCATION BASED ON the mlButtonData SETTINGS.
                    NativeMethods.ShowWindow(hwnd, WindowShowStyle.Restore)

                    'Move window to current screen which contains the current mouse cursor position.
                    Dim rect As System.Drawing.Rectangle = GetWindowStyleExRect(hwnd, mlButtonData, currentScreen)

                    'When moving windows from one screen to another screen with different resolution/scaling, 
                    'the Window Is moved but the window Is Not being resized properly.
                    'Call MoveWindow twice! 
                    If System.Windows.Forms.Screen.AllScreens.Count > 1 Then
                        IsResized = NativeMethods.MoveWindow(hwnd, rect.Left, rect.Top, rect.Width, rect.Height, True)
                    End If

                    IsResized = NativeMethods.MoveWindow(hwnd, rect.Left, rect.Top, rect.Width, rect.Height, True)
                    If IsResized Then NativeMethods.SetForegroundWindowNative(hwnd)
                End If

            ElseIf (mlButtonData.WindowStyle = ProcessWindowStyle.Maximized) Then
                'Restore window only if # of screens > 1.
                If System.Windows.Forms.Screen.AllScreens.Count > 1 Then
                    'If an open window is on the same screen, do not restore or move.
                    Dim screenOfWindow = System.Windows.Forms.Screen.FromHandle(hwnd)

                    If Not (screenOfWindow.DeviceName.Equals(currentScreen.DeviceName)) Then
                        'Restore window state as normal first because MS Windows opens a program from the last screen it was closed.
                        'This prevents a window from opening from another screen before being moved. Thus moving the window will be 
                        'less noticeable.
                        NativeMethods.ShowWindow(hwnd, WindowShowStyle.ShowNormal)

                        'Move window to current screen which contains the current mouse cursor position.
                        IsResized = NativeMethods.MoveWindow(hwnd, currentScreen.WorkingArea.Left, currentScreen.WorkingArea.Top,
                                            currentScreen.WorkingArea.Width, currentScreen.WorkingArea.Height, True)
                    Else
                        IsResized = True
                    End If
                Else
                    IsResized = True
                End If

                'Maximize window.
                NativeMethods.ShowWindow(hwnd, WindowShowStyle.ShowMaximized)
                NativeMethods.SetForegroundWindowNative(hwnd)
            Else
                IsResized = True
            End If

            Return IsResized
        End Function

        ''' <summary>
        ''' Gets the location and size of a window based on the selected WindowStyleEx
        ''' </summary>
        ''' <param name="mlButtonData"></param>
        ''' <returns>Rectangle to be used as window's location and size.</returns>
        Shared Function GetWindowStyleExRect(ByVal hwnd As IntPtr, ByVal mlButtonData As MLButtonData, ByVal currentScreen As System.Windows.Forms.Screen) As System.Drawing.Rectangle
            Dim wAreaWidth As Integer = currentScreen.WorkingArea.Width
            Dim wAreaHeight As Integer = currentScreen.WorkingArea.Height

            Dim rect As New System.Drawing.Rectangle With {
            .X = 0,
            .Y = 0,
            .Width = wAreaWidth,
            .Height = wAreaHeight
        }

            Select Case mlButtonData.WindowStyleEx
                Case WindowStyleEx.WindowStyleEx_Left
                    rect.Width = (wAreaWidth / 2)
                Case WindowStyleEx.WindowStyleEx_Right
                    rect.X = (wAreaWidth / 2)
                    rect.Width = (wAreaWidth / 2)
                Case WindowStyleEx.WindowStyleEx_Top
                    rect.Height = (wAreaHeight / 2)
                Case WindowStyleEx.WindowStyleEx_Bottom
                    rect.Y = (wAreaHeight / 2)
                Case WindowStyleEx.WindowStyleEx_TopLeft
                    rect.Width = wAreaWidth / 2
                    rect.Height = wAreaHeight / 2
                Case WindowStyleEx.WindowStyleEx_TopRight
                    rect.X = wAreaWidth / 2
                    rect.Width = wAreaWidth / 2
                    rect.Height = wAreaHeight / 2
                Case WindowStyleEx.WindowStyleEx_BottomLeft
                    rect.Y = wAreaHeight / 2
                    rect.Height = wAreaHeight / 2
                    rect.Width = wAreaWidth / 2
                Case WindowStyleEx.WindowStyleEx_BottomRight
                    rect.X = wAreaWidth / 2
                    rect.Width = wAreaWidth / 2
                    rect.Y = wAreaHeight / 2
                    rect.Height = wAreaHeight / 2
                Case WindowStyleEx.WindowStyleEx_Custom
                    'Current window's rectangle for fixed width and/or height.
                    Dim windowRect As System.Drawing.Rectangle = GetWindowRect(hwnd)
                    If Not windowRect.IsEmpty Then
                        rect = mlButtonData.WindowRect
                        If (rect.Width = 0) Then rect.Width = windowRect.Width
                        If (rect.Height = 0) Then rect.Height = windowRect.Height
                    End If
            End Select

            'Convert left and top of rect relative to current screen.
            rect.X = currentScreen.WorkingArea.Left + rect.Left
            rect.Y = currentScreen.WorkingArea.Top + rect.Top

            Return rect
        End Function

        Shared Function FindWindowByTitle(ByVal mlButtonData As MLButtonData) As IntPtr
            Dim hwnd As IntPtr = IntPtr.Zero

            'Match title.
            If (mlButtonData.IsDirectory) Then
                hwnd = ActiveWindows.GetWindowHandle(mlButtonData.WindowTitle,
                                                 "explorer", ActiveWindows.MatchingMode.FullMatch)
            Else
                hwnd = ActiveWindows.GetWindowHandle(mlButtonData.WindowTitle,
                                                 mlButtonData.WindowProcessName, ActiveWindows.MatchingMode.Full_PartialMatch)
            End If

            'GC.Collect()

            Return hwnd
        End Function

        'This doesn't work. A dropdown box is resized in explorer.
        '1.Open Explorer.
        '2.Click on drop down box.
        '3. Open MaxLauncher and click on a folder w/ resizing option.
        '4. The dropdown box is resized instead of the window.
        '<DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=CharSet.Auto)> _
        'Protected Shared Function FindWindowByCaption( _
        ' ByVal zero As IntPtr, _
        ' ByVal lpWindowName As String) As IntPtr
        'End Function

        Enum WindowShowStyle As UInteger
            ''' <summary>Hides the window and activates another window.</summary>
            ''' <remarks>See SW_HIDE</remarks>
            Hide = 0
            '''<summary>Activates and displays a window. If the window is minimized 
            ''' or maximized, the system restores it to its original size and 
            ''' position. An application should specify this flag when displaying 
            ''' the window for the first time.</summary>
            ''' <remarks>See SW_SHOWNORMAL</remarks>
            ShowNormal = 1
            ''' <summary>Activates the window and displays it as a minimized window.</summary>
            ''' <remarks>See SW_SHOWMINIMIZED</remarks>
            ShowMinimized = 2
            ''' <summary>Activates the window and displays it as a maximized window.</summary>
            ''' <remarks>See SW_SHOWMAXIMIZED</remarks>
            ShowMaximized = 3
            ''' <summary>Maximizes the specified window.</summary>
            ''' <remarks>See SW_MAXIMIZE</remarks>
            Maximize = 3
            ''' <summary>Displays a window in its most recent size and position. 
            ''' This value is similar to "ShowNormal", except the window is not 
            ''' actived.</summary>
            ''' <remarks>See SW_SHOWNOACTIVATE</remarks>
            ShowNormalNoActivate = 4
            ''' <summary>Activates the window and displays it in its current size 
            ''' and position.</summary>
            ''' <remarks>See SW_SHOW</remarks>
            Show = 5
            ''' <summary>Minimizes the specified window and activates the next 
            ''' top-level window in the Z order.</summary>
            ''' <remarks>See SW_MINIMIZE</remarks>
            Minimize = 6
            '''   <summary>Displays the window as a minimized window. This value is 
            '''   similar to "ShowMinimized", except the window is not activated.</summary>
            ''' <remarks>See SW_SHOWMINNOACTIVE</remarks>
            ShowMinNoActivate = 7
            ''' <summary>Displays the window in its current size and position. This 
            ''' value is similar to "Show", except the window is not activated.</summary>
            ''' <remarks>See SW_SHOWNA</remarks>
            ShowNoActivate = 8
            ''' <summary>Activates and displays the window. If the window is 
            ''' minimized or maximized, the system restores it to its original size 
            ''' and position. An application should specify this flag when restoring 
            ''' a minimized window.</summary>
            ''' <remarks>See SW_RESTORE</remarks>
            Restore = 9
            ''' <summary>Sets the show state based on the SW_ value specified in the 
            ''' STARTUPINFO structure passed to the CreateProcess function by the 
            ''' program that started the application.</summary>
            ''' <remarks>See SW_SHOWDEFAULT</remarks>
            ShowDefault = 10
            ''' <summary>Windows 2000/XP: Minimizes a window, even if the thread 
            ''' that owns the window is hung. This flag should only be used when 
            ''' minimizing windows from a different thread.</summary>
            ''' <remarks>See SW_FORCEMINIMIZE</remarks>
            ForceMinimized = 11
        End Enum

        Shared Function StartResizeThread(ByVal mlButtonData As MLButtonData, ByVal currentScreen As System.Windows.Forms.Screen) As Object
            Try
                Dim loopCount As Integer = 20
                Dim count = 0

                If Not (String.IsNullOrEmpty(mlButtonData.WindowTitle)) Then
                    'Resize by window title specified in the button configuration.
                    Do While count < loopCount
                        If count < 10 Then
                            System.Threading.Thread.Sleep(1000)
                        Else
                            System.Threading.Thread.Sleep(3000)
                        End If
                        If ResizeWindow(FindWindowByTitle(mlButtonData), mlButtonData, currentScreen) Then Exit Do

                        count += 1
                    Loop
                End If
            Catch ex As Exception
                'Ignore errors.
            End Try

            Return Nothing
        End Function

        <StructLayout(LayoutKind.Sequential)>
        Structure RECT
            Public Left As Int32
            Public Top As Int32
            Public Right As Int32
            Public Bottom As Int32
        End Structure

        Shared Function GetWindowRect(ByVal hWnd As IntPtr) As System.Drawing.Rectangle
            Dim resultRect = New System.Drawing.Rectangle()

            Dim rct As RECT

            'Handle error
            If Not NativeMethods.GetWindowRect(hWnd, rct) Then Return Nothing

            resultRect.X = rct.Left
            resultRect.Y = rct.Top
            resultRect.Width = rct.Right - rct.Left
            resultRect.Height = rct.Bottom - rct.Top

            Return resultRect
        End Function
#End Region

    End Class

End Namespace

'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Runtime.InteropServices
Imports System.Text
Imports MaxLauncher.Utility

Namespace Utility

    Friend NotInheritable Class NativeMethods

        <DllImport("user32.dll")>
        Friend Shared Function RegisterHotKey(ByVal wndHandle As IntPtr, ByVal wndHashCode As Integer, ByVal modifier As Integer, ByVal vkey As Integer) As Boolean
        End Function

        <DllImport("user32.dll")>
        Friend Shared Function UnregisterHotKey(ByVal wndHandle As IntPtr, ByVal wndHashCode As Integer) As Boolean
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
        Friend Shared Function MapVirtualKey(ByVal uCode As Integer, ByVal nMapType As Integer) As Integer
        End Function

        <DllImport("user32.dll")>
        Friend Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
        End Function

        <DllImport("user32.dll")>
        Friend Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
        End Function

#Region "Keyboard"

        <DllImport("user32.dll", ExactSpelling:=True)>
        Friend Shared Function GetKeyboardLayout(ByVal threadId As UInteger) As IntPtr
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True)>
        Friend Shared Function ToUnicodeEx(
            ByVal wVirtKey As UInteger,
            ByVal wScanCode As UInteger,
            ByVal lpKeyState() As Byte,
            ByVal pwszBuff As System.Text.StringBuilder,
            ByVal cchBuff As Integer,
            ByVal wFlags As UInteger,
            ByVal dwhkl As IntPtr) As Integer
        End Function

#End Region

#Region "EditButtonDialog"

        <DllImport("shell32.dll", CharSet:=CharSet.Unicode)>
        Friend Shared Function PickIconDlg(ByVal hwndOwner As IntPtr,
                                        ByVal lpstrFile As System.Text.StringBuilder,
                                        ByVal nMaxFile As Integer,
                                        ByRef lpdwIconIndex As Integer) As Integer
        End Function

#End Region

#Region "ActiveWindows"

        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Friend Shared Function GetWindowText(ByVal hwnd As IntPtr, ByVal lpString As StringBuilder, ByVal cch As Integer) As Integer
        End Function

        <DllImport("USER32.DLL")>
        Friend Shared Function GetShellWindow() As IntPtr
        End Function

        <DllImport("USER32.DLL")>
        Friend Shared Function GetWindowTextLength(ByVal hWnd As IntPtr) As Integer
        End Function

        <DllImport("user32.dll", SetLastError:=True)>
        Friend Shared Function GetWindowThreadProcessId(ByVal hWnd As IntPtr, <Out()> ByRef lpdwProcessId As UInt32) As UInt32
        End Function

        <DllImport("USER32.DLL")>
        Friend Shared Function IsWindowVisible(ByVal hWnd As IntPtr) As Boolean
        End Function

        <DllImport("user32.dll", ExactSpelling:=True, CharSet:=CharSet.Unicode)>
        Friend Shared Function GetParent(ByVal hWnd As IntPtr) As IntPtr
        End Function

        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Friend Shared Function GetWindow(ByVal hWnd As IntPtr, ByVal uCmd As UInt32) As IntPtr
        End Function

        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Friend Shared Function FindWindow(
             ByVal lpClassName As String,
             ByVal lpWindowName As String) As IntPtr
        End Function

#End Region

#Region "Launcher"

        <DllImport("user32.dll")>
        Friend Shared Function MoveWindow(ByVal hWnd As IntPtr,
                                      ByVal x As Integer,
                                      ByVal y As Integer,
                                      ByVal nWidth As Integer,
                                      ByVal nHeight As Integer,
                                      ByVal bRepaint As Boolean) As Boolean
        End Function

        <DllImport("user32.dll")>
        Friend Shared Function GetWindowRect(ByVal hWnd As IntPtr, ByRef lpRect As Launcher.RECT) As Boolean
        End Function

        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Friend Shared Function ShowWindow(ByVal hwnd As IntPtr, ByVal nCmdShow As Launcher.WindowShowStyle) As Boolean
        End Function

        <DllImport("User32.dll", EntryPoint:="SetForegroundWindow")>
        Friend Shared Function SetForegroundWindowNative(hWnd As IntPtr) As IntPtr
        End Function

#End Region

#Region "Imaging"

        <DllImport("shell32.dll", CharSet:=CharSet.Unicode)>
        Friend Shared Function ExtractIcon(ByVal hInst As IntPtr, ByVal lpszExeFileName As String, ByVal nIconIndex As Integer) As IntPtr
        End Function

        <DllImport("user32.dll")>
        Friend Shared Function DestroyIcon(ByVal handle As IntPtr) As Boolean
        End Function

        <DllImport("shell32.dll", CharSet:=CharSet.Auto)>
        Shared Function ExtractAssociatedIcon(ByVal hInst As IntPtr, ByVal iconPath As StringBuilder, ByRef index As Short) As IntPtr
        End Function

#End Region

    End Class

End Namespace

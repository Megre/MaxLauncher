'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Text

Namespace Utility

    Public Class ActiveWindows
        Enum MatchingMode As Integer
            FullMatch = 0
            PartialMatch = 1
            Full_PartialMatch = 2
        End Enum

        Public Const GW_HWNDNEXT = 2

        Public Shared ReadOnly hShellWindow As IntPtr = NativeMethods.GetShellWindow()
        Public Shared ReadOnly currentProcessID As Integer = Process.GetCurrentProcess.Id

        Public Shared Function GetProcessNames() As List(Of String)
            Dim hWnd As IntPtr
            Dim list As New List(Of String)

            'Grab the first window handle that Windows finds.
            hWnd = NativeMethods.FindWindow(Nothing, Nothing)

            'Find exact title match.
            'Loop until you find a match or there are no more window handles.
            Do Until hWnd = 0
                Try
                    ' Check if no parent for this window
                    If NativeMethods.GetParent(hWnd) = 0 Then
                        If IsValidWindow(hWnd) Then
                            Dim winProc = GetWindowProcess(hWnd)
                            If (winProc IsNot Nothing) Then list.Add(winProc.ProcessName)
                        End If
                    End If
                Catch ex As Exception
                    'Ignore
                End Try

                ' Get the next window handle
                hWnd = NativeMethods.GetWindow(hWnd, GW_HWNDNEXT)
            Loop

            Return list.Distinct().ToList
        End Function

        Public Shared Function GetWindowTitles() As List(Of String)
            Dim hWnd As IntPtr
            Dim list As New List(Of String)

            'Grab the first window handle that Windows finds.
            hWnd = NativeMethods.FindWindow(Nothing, Nothing)

            'Find exact title match.
            'Loop until a match is found or there are no more window handles.
            Do Until hWnd = 0
                Try
                    ' Check if no parent for this window
                    If NativeMethods.GetParent(hWnd) = 0 Then
                        If IsValidWindow(hWnd) Then
                            Dim winTitle = GetWindowTitle(hWnd)
                            If Not String.IsNullOrEmpty(winTitle) Then list.Add(winTitle)
                        End If
                    End If
                Catch ex As Exception
                    'Ignore
                End Try

                ' Get the next window handle
                hWnd = NativeMethods.GetWindow(hWnd, GW_HWNDNEXT)
            Loop

            Return list.Distinct().ToList
        End Function

        Public Shared Function MatchWindowHandle(ByVal hwnd As IntPtr, ByVal title As String,
                                                 ByVal procName As String, ByVal titleMatching As MatchingMode) As Boolean
            If (hwnd = hShellWindow) Then Return False

            'Ignore hidden windows.
            If Not NativeMethods.IsWindowVisible(hwnd) Then Return False

            Dim winProc = GetWindowProcess(hwnd)
            If (winProc Is Nothing) Then Return False

            ''If procName is null or empty, exclude explorer when matching.
            'If (String.IsNullOrEmpty(procName)) Then
            '    If (String.Equals(winProc.ProcessName.ToLower(), "explorer")) Then Return False
            'End If

            Dim winTitle = GetWindowTitle(hwnd)
            If String.IsNullOrEmpty(winTitle) Then Return False

            'Test for exact title match.
            If titleMatching = ActiveWindows.MatchingMode.FullMatch OrElse
                titleMatching = MatchingMode.Full_PartialMatch Then
                If (String.IsNullOrEmpty(procName)) Then
                    If (String.Equals(winTitle, title)) Then
                        Return True
                    End If
                Else
                    If (String.Equals(winProc.ProcessName.ToLower(), procName.ToLower)) AndAlso
                        (String.Equals(winTitle, title)) Then
                        Return True
                    End If
                End If
            End If

            'Test for partial title match.
            If titleMatching = ActiveWindows.MatchingMode.PartialMatch OrElse
                titleMatching = MatchingMode.Full_PartialMatch Then
                If (String.IsNullOrEmpty(procName)) Then
                    If (winTitle.Contains(title)) Then
                        Return True
                    End If
                Else
                    If (String.Equals(winProc.ProcessName.ToLower(), procName.ToLower)) AndAlso
                        (winTitle.Contains(title)) Then
                        Return True
                    End If
                End If
            End If

            Return False
        End Function

        Public Shared Function GetWindowHandle(ByVal title As String, ByVal procName As String,
                                               ByVal titleMatching As MatchingMode) As IntPtr
            Dim hWnd As IntPtr

            'Grab the first window handle that Windows finds.
            hWnd = NativeMethods.FindWindow(Nothing, Nothing)

            'Loop until you find a match or there are no more window handles.
            Do Until hWnd = 0
                ' Check if no parent for this window
                If NativeMethods.GetParent(hWnd) = 0 Then
                    If MatchWindowHandle(hWnd, title, procName, titleMatching) Then Return hWnd
                End If

                ' Get the next window handle
                hWnd = NativeMethods.GetWindow(hWnd, GW_HWNDNEXT)
            Loop

            Return IntPtr.Zero
        End Function

#Region "Utility methods"

        Public Shared Function GetWindowTitle(ByVal hwnd As IntPtr) As String
            'Ignore windows with zero-length titles.
            Dim length As Integer = NativeMethods.GetWindowTextLength(hwnd)
            If (length = 0) Then Return Nothing

            Dim stringBuilder As New System.Text.StringBuilder(length + 1)
            NativeMethods.GetWindowText(hwnd, stringBuilder, length + 1)

            'Ignore if the window title is equal to "start".
            If (String.Equals(stringBuilder.ToString.ToLower, "start")) Then Return Nothing

            Return stringBuilder.ToString
        End Function

        Public Shared Function GetWindowProcess(ByVal hwnd As IntPtr) As System.Diagnostics.Process
            'Ignore current process ID.
            Dim windowPid As UInt32
            NativeMethods.GetWindowThreadProcessId(hwnd, windowPid)
            If (windowPid = currentProcessID) Then Return Nothing

            Return Process.GetProcessById(windowPid)
        End Function

        Public Shared Function IsValidWindow(ByVal hwnd As IntPtr) As Boolean
            If (hwnd = hShellWindow) Then Return False

            'Ignore hidden windows.
            If Not NativeMethods.IsWindowVisible(hwnd) Then Return False

            If (GetWindowProcess(hwnd) Is Nothing) Then Return False

            Dim winTitle = GetWindowTitle(hwnd)
            If String.IsNullOrEmpty(winTitle) Then Return False

            Return True
        End Function

#End Region 'Utility methods

    End Class

End Namespace
'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Windows.Threading

Class Application

    Friend Shared Property SaveData As Boolean = True

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.
    Private Sub Application_DispatcherUnhandledException(sender As Object, e As DispatcherUnhandledExceptionEventArgs)
        MessageBoxML.Show(e.Exception.Message, "Unhandled Exception",
                          MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                          e.Exception.ToString)

        e.Handled = True
    End Sub

    Private Sub Application_Deactivated(sender As Object, e As EventArgs)
        If (MainWindow IsNot Nothing) AndAlso
            (ConfigManager.AppConfig.AutoHide) AndAlso (MainWindow.OwnedWindows.Count = 0) Then
            MainWindow.WindowState = WindowState.Minimized
        End If
    End Sub

    Public StartupArgs As String()

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs)
        StartupArgs = e.Args

        Try
            ConfigManager.LoadAll()
        Catch ex As Exception
            MessageBoxML.Show(ex.Message & " " & Localization.GetString("MessageBox_Message_OKtoexit"),
                            Localization.GetString("MessageBox_Title_LoadingConfiguration"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error, MessageBoxResult.OK, ex.ToString)
            Application.SaveData = False
            Application.Current.Shutdown()
        End Try

        'Show runnning instance.
        Dim existingProcess As Process = Nothing
        Try
            If ConfigManager.AppConfig.SingleInstance Then
                Try
                    existingProcess = GetExistingProcess()

                    If existingProcess IsNot Nothing Then AppActivate(existingProcess.Id)
                Catch ex As Exception
                    'Ignore
                End Try
            End If
        Catch ex As Exception
            'Ignore
        Finally
            If ConfigManager.AppConfig.SingleInstance AndAlso
                existingProcess IsNot Nothing Then
                Application.SaveData = False
                Application.Current.Shutdown()
            End If
        End Try

    End Sub

    Private Function IsExistProcess() As Boolean
        Dim retValue As Boolean = False
        Dim currentProc As Process = Process.GetCurrentProcess()
        Dim runningProcs = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName)
        Dim count As Integer = runningProcs.Count()

        If count > 1 Then
            Try
                For Each p In runningProcs
                    If p.Id <> currentProc.Id AndAlso
                        String.Equals(p.MainModule.FileName, currentProc.MainModule.FileName) Then
                        retValue = True
                        Exit For
                    End If
                Next
            Catch ex As Exception
                'Ignore
            End Try
        End If

        Return retValue
    End Function

    Private Function GetExistingProcess() As Process
        Dim retValue As Process = Nothing
        Dim currentProc As Process = Process.GetCurrentProcess()
        Dim runningProcs = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName)
        Dim count As Integer = runningProcs.Count()

        If count > 1 Then
            Try
                For Each p In runningProcs
                    If p.Id <> currentProc.Id AndAlso
                        String.Equals(p.MainModule.FileName, currentProc.MainModule.FileName) Then
                        retValue = p
                        Exit For
                    End If
                Next
            Catch ex As Exception
                'Ignore
            End Try
        End If

        Return retValue
    End Function

    Private Sub Application_Activated(sender As Object, e As EventArgs)
        Try
            If ConfigManager.AppConfig.SingleInstance AndAlso
                        IsExistProcess() AndAlso
                        MainWindow IsNot Nothing AndAlso
                        MainWindow.IsLoaded Then
                MainWindow.WindowState = WindowState.Normal
            End If
        Catch ex As Exception
            'Ignore
        End Try
    End Sub

    Private Sub Application_SessionEnding(sender As Object, e As SessionEndingCancelEventArgs)
        e.Cancel = True

        'Close the MainWindow, which closes the application.
        If (My.Application.MainWindow IsNot Nothing) Then DirectCast(My.Application.MainWindow, MainWindow).ExitApp()
    End Sub
End Class

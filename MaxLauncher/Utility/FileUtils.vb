'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Text
Imports System.Windows.Interop

Namespace Utility

    Class FileUtils

        ''' <summary>
        ''' Gets the filename for the new file.
        ''' </summary>
        ''' <returns>Filename</returns>
        ''' <remarks></remarks>
        Public Shared Function GetFilename(owner As Window,
                                mode As System.IO.FileMode,
                                Optional title As String = "",
                                Optional defaultExtension As String = "",
                                Optional filter As String = "",
                                Optional defaultFilename As String = "") As String
            Dim retValue As String = ""

            Dim hiddenChildWindow As New Window With {
            .Owner = owner
        }

            Select Case mode
                Case IO.FileMode.Create
                    'Configure save file dialog box 
                    Dim dlg As New Microsoft.Win32.SaveFileDialog()

                    With dlg
                        .Title = title
                        .DefaultExt = defaultExtension 'Default file extension
                        .Filter = filter ' Filter files by extension
                        .FileName = defaultFilename

                        'Show save file dialog box
                        Dim result? As Boolean = .ShowDialog(owner)
                        'Process save file dialog box results
                        If (result) Then retValue = .FileName
                    End With
                Case IO.FileMode.Open
                    'Configure open file dialog box 
                    Dim dlg As New Microsoft.Win32.OpenFileDialog()

                    With dlg
                        .Title = title
                        .DefaultExt = defaultExtension 'Default file extension 
                        .Filter = filter ' Filter files by extension 
                        .FileName = defaultFilename

                        'Show open file dialog box 
                        Dim result? As Boolean = .ShowDialog(owner)

                        'Process open file dialog box results 
                        If (result) Then retValue = .FileName
                    End With
            End Select

            hiddenChildWindow.Close()
            Return retValue
        End Function

        Public Class Path
            Public Shared Function ConvertToFullPath(path As String) As String
                If Not IsUNC(path) Then
                    If path.StartsWith(IO.Path.DirectorySeparatorChar) Then
                        path = path.TrimStart(IO.Path.DirectorySeparatorChar)
                        path = System.IO.Path.Combine(System.IO.Path.GetPathRoot(My.Application.Info.DirectoryPath), path)
                    End If
                End If

                Return path
            End Function

            Public Shared Function ConvertToRelativePath(path As String) As String
                If Not ConfigManager.IsPortable Then Return path

                Dim appPath As New StringBuilder(My.Application.Info.DirectoryPath)
                Dim oldPath As New StringBuilder(path)
                Dim newPath As String = path

                Dim bError As Boolean = False
                Try
                    If Not (path(1).Equals(":"c)) Then bError = True
                Catch ex As Exception
                    bError = True
                End Try
                If (bError) Then Return path

                Dim appPathArray = Split(My.Application.Info.DirectoryPath, System.IO.Path.DirectorySeparatorChar)
                Dim oldPathArray = Split(path, System.IO.Path.DirectorySeparatorChar)

                If (appPathArray(0).Equals(oldPathArray(0))) Then
                    oldPathArray(0) = ""
                    newPath = String.Join(System.IO.Path.DirectorySeparatorChar, oldPathArray)
                End If

                Return newPath
            End Function

            ''' <summary>
            ''' Recursively expands enironment variables.
            ''' </summary>
            ''' <param name="str">String to expand.</param>
            ''' <returns>Modified string with all environment variables expanded recursively.</returns>
            ''' <exception cref="System.ArgumentNullException">str is null.</exception>
            Public Shared Function ExpandEnvironmentVariables(ByVal str As String) As String
                Dim newString As String

                Try
                    Do
                        newString = System.Environment.ExpandEnvironmentVariables(str)
                        If Not (str.Equals(newString)) Then
                            str = newString
                        Else
                            Exit Do
                        End If
                    Loop
                Catch ex As Exception
                    Throw
                End Try

                'Return newString.Trim(CChar(""""c))
                Return newString
            End Function

            ''' <summary>
            ''' Replaces tokens in a string with corresponding values from the clipboard.
            ''' </summary>
            ''' <param name="str">String to be modified.</param>
            ''' <returns>A string which has its tokens replaced with values from the clipboard.</returns>
            Public Shared Function ReplaceTokens(ByVal str As String) As String
                If (str.Contains("<text>")) Then
                    Dim clipText As String = ""

                    If (System.Windows.Clipboard.ContainsText) Then clipText = System.Windows.Clipboard.GetText()
                    str = str.Replace("<text>", clipText)
                End If

                If (str.Contains("<file>")) Then
                    Dim clipText As String = ""

                    Try
                        If (Clipboard.ContainsData(DataFormats.FileDrop)) Then clipText = """" & Clipboard.GetFileDropList.Item(0) & """"
                    Catch ex As Exception
                    End Try
                    str = str.Replace("<file>", clipText)
                End If

                Return str
            End Function
        End Class

        Public Shared Function IsUNC(ByVal path As String) As Boolean
            If (path.StartsWith("\\")) Then Return True

            Return False
        End Function
        Friend Shared Function IsShortcut(filename As String) As Boolean
            If (String.Compare(System.IO.Path.GetExtension(filename), ".lnk", True) = 0) Then Return True

            Return False
        End Function

    End Class

End Namespace
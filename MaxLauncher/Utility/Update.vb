'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Net
Imports System.IO

Namespace Utility

    Enum CHECKUPDATE As UShort
        AVAILABLE = 1
        FAILED = 2
        UPTODATE = 3
    End Enum

    Class Update

        Public Property NewVersion As New System.Version("0.0.0.0")

        Public Function Check() As CHECKUPDATE
            Dim result As CHECKUPDATE = CHECKUPDATE.FAILED

            'TODO Replace (My.Application.Info.Version.Build = 0) with an option/setting flag that sets whether
            'to check for Beta Versions.
            'If (My.Application.Info.Version.Build = 0) Then
            result = Check(My.Settings.GAVersionFile)
            'Else
            'result = Check(My.Settings.BetaVersionFile)
            'End If

            Return result
        End Function

        'Returns True if a new version is available
        Private Function Check(ByVal versionURL As String) As CHECKUPDATE
            Dim request As WebRequest
            Dim response As WebResponse = Nothing
            Dim reader As StreamReader = Nothing
            Dim result As CHECKUPDATE = CHECKUPDATE.FAILED

            Try
                'Dim client As WebClient = New WebClient()
                'Dim reply As String = client.DownloadString("http://madapplauncher.sourceforge.net/version/test.txt")

                'Console.WriteLine(reply)


                'Update fix if version files(eg. ga_version.txt) is on an https server. However, .Net 3.5 does not support TLS 1.2.
                '(3072 = TLS 1.2). .Net 3.5.1 supports TLS 3.5.1 but it requires an extra installation.
                'ServicePointManager.SecurityProtocol = 3072

                'request = WebRequest.Create("http://madproton.users.sourceforge.net/maxlauncher/version/test.txt")
                request = WebRequest.Create(versionURL)
                request.Timeout = 15000

                response = request.GetResponse()
                reader = New StreamReader(response.GetResponseStream())

                NewVersion = New System.Version(reader.ReadLine())
                Dim currentVersion As System.Version = My.Application.Info.Version

                'FOR TESTING
                'NewVersion = New System.Version("1.29.0.0")

                If ((currentVersion.CompareTo(NewVersion)) < 0) Then
                    result = CHECKUPDATE.AVAILABLE
                Else
                    result = CHECKUPDATE.UPTODATE
                End If

            Catch ex As Exception
                MessageBoxML.Show(Localization.GetString("String_UpdateCheckFailed"), Localization.GetString("String_Update"), MessageBoxButton.OK,
                             MessageBoxImage.Exclamation, MessageBoxResult.OK, ex.ToString)
                result = CHECKUPDATE.FAILED
            Finally
                If (response IsNot Nothing) Then response.Close()
                If (reader IsNot Nothing) Then reader.Close()
            End Try

            Return result
        End Function

    End Class

End Namespace

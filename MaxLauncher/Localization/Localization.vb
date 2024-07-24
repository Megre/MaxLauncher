'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.IO

Class Localization
    Friend Const DefaultLanguage = "English"

    Friend Shared Sub ApplyLanguage(ByVal language As String)
        Try
            If String.Compare(language, DefaultLanguage, True) = 0 Then
                ClearCustomLanguage()
            Else
                Dim languageFile As String = System.IO.Path.Combine(ConfigManager.PortableConfig.LanguageDirectory, language & ".xaml")

                If System.IO.File.Exists(languageFile) Then
                    Dim langRD As New ResourceDictionary()
                    langRD.Source = New Uri(languageFile, UriKind.Absolute)
                    Application.Current.Resources.MergedDictionaries.Add(langRD)
                End If
            End If

            'Set theme in use.
            ConfigManager.AppConfig.Language = language
        Catch ex As Exception
            Throw New Exception(String.Format(Localization.GetString("String_ErrorSettingLanguage"), language), ex)
        End Try
    End Sub

    Friend Shared Sub ClearCustomLanguage()
        Dim langPathURI As Uri = New Uri(ConfigManager.PortableConfig.LanguageDirectory)

        Dim rdList As New List(Of ResourceDictionary)

        For Each rd As ResourceDictionary In Application.Current.Resources.MergedDictionaries
            If rd.Source.ToString.StartsWith(langPathURI.ToString, True, Nothing) Then
                rdList.Add(rd)
            End If
        Next

        For Each rd As ResourceDictionary In rdList
            Application.Current.Resources.MergedDictionaries.Remove(rd)
        Next
    End Sub

    Friend Shared Function GetString(ByVal resourceName As String) As String
        Dim retValue = Application.Current.TryFindResource(resourceName)

        If retValue Is Nothing OrElse Not (TypeOf retValue Is String) Then retValue = String.Empty

        Return retValue
    End Function

    'Friend Shared Function GetTag(ByVal resourceName As String) As Object
    '    Dim resourceObject = Application.Current.TryFindResource(resourceName)
    '    Dim retValue As Object = Nothing

    '    If resourceObject IsNot Nothing Then retValue = resourceObject.

    '    Return retValue.tag
    'End Function
End Class

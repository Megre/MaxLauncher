Imports System.Collections.Generic
Imports System.Xml.Linq
Public Class ButtonIndex
    Private _map As New Dictionary(Of String, XElement)
    Private _log As FileImportLog

    Public Sub New(root As XElement, log As FileImportLog)
        _log = log

        For Each tabElement In root.Elements("Tab")
            For Each buttonElement In tabElement.Elements("Button")
                If Not IndexButtonElement(buttonElement) Then
                    _log.LogButton("duplicate button found", buttonElement)
                End If
            Next
        Next
    End Sub

    Public Function GetByFilenName(filename As String)
        Dim buttonEle As XElement = Nothing
        If _map.TryGetValue(filename, buttonEle) Then
            Return buttonEle
        End If

        Return Nothing
    End Function

    Public Function GetButton(buttonElement As XElement) As XElement
        Return GetByFilenName(FileName(buttonElement))
    End Function

    Public Function IndexButtonElement(buttonElement As XElement)
        Try
            _map.Add(FileName(buttonElement), buttonElement)
            Return True
        Catch
        End Try

        Return False
    End Function

    Private Function FileName(buttonElement As XElement) As String
        Return buttonElement.Element("Filename").Value
    End Function

End Class

'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.IO
Imports System.Data

Partial Class TabControlData

    Private Shared _instance As TabControlData

    <NonSerialized()> _
    Private _filename As String = ""
    Friend ReadOnly Property Filename As String
        Get
            Return _filename
        End Get
    End Property

    Friend Shared Function GetInstance() As TabControlData
        If (_instance Is Nothing) Then _instance = New TabControlData

        Return _instance
    End Function

    Friend Function Exists() As Boolean
        If (String.IsNullOrEmpty(Me.Filename)) Then Return False

        Return True
    End Function

    Friend Sub Create(filename As String)
        Dim oldDS = _instance
        Dim newDS = New TabControlData

        If (oldDS IsNot Nothing) Then oldDS.Dispose()
        _instance = newDS

        newDS._filename = filename
        newDS.Save()
    End Sub

    Friend Sub Open(filename As String)
        Try
            Dim oldDS = _instance
            Dim newDS = New TabControlData

            Using fs As FileStream = New FileStream(filename, FileMode.Open, FileAccess.Read)
                Dim br As BufferedStream = New BufferedStream(fs, 32768)

                Dim dataTable As DataTable

                For Each dataTable In Me.Tables
                    dataTable.BeginLoadData()
                Next

                newDS.ReadXml(br)

                For Each dataTable In Me.Tables
                    dataTable.EndLoadData()
                Next

                newDS.AcceptChanges()
                _instance = newDS

                newDS._filename = filename
                oldDS.Dispose()
            End Using
        Catch ex As Exception
            Throw New Exception(String.Format(Localization.GetString("String_ErrorOpeningFile"), filename), ex)
        End Try
    End Sub

    Friend Sub Save()
        Save(Me.Filename)
    End Sub

    Friend Sub Save(filename As String)
        Try
            If (String.Compare(filename, Me.Filename, True) = 0) Then
                If Me.HasChanges() Then
                    Me.AcceptChanges()
                    Me.WriteXml(filename)
                End If
            Else
                Me.AcceptChanges()
                Me.WriteXml(filename)
                Me._filename = filename
            End If
        Catch ex As Exception
            Throw New Exception(String.Format(Localization.GetString("String_ErrorSavingFile"), filename), ex)
        End Try
    End Sub

End Class

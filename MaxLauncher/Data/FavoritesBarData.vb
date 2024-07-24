'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.IO
Imports System.Data

Partial Class FavoritesBarData

    ''' <summary>
    ''' The singleton instance of this object.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared _instance As FavoritesBarData

    ''' <summary>
    ''' The filename associated with this object.
    ''' </summary>
    ''' <remarks></remarks>
    <NonSerialized()>
    Private _filename As String = ""
    Friend ReadOnly Property Filename As String
        Get
            Return _filename
        End Get
    End Property

    ''' <summary>
    ''' Returns the instance of this singleton object.
    ''' </summary>
    ''' <returns>FavoritesBarData</returns>
    ''' <param name="file">The file to which this data object will be serialized.</param>
    ''' <remarks></remarks>
    Friend Shared Function GetInstance(file As String) As FavoritesBarData
        If _instance Is Nothing Then
            If System.IO.File.Exists(file) Then
                'Load
                Try
                    _instance = New FavoritesBarData
                    _instance.ReadXml(file)
                    _instance.AcceptChanges()
                    _instance._filename = file
                Catch ex As Exception
                    Throw New Exception(String.Format(Localization.GetString("String_ErrorOpeningFavoritesBarConfig"), file), ex)
                End Try
            Else
                _instance = New FavoritesBarData With {
                    ._filename = file
                }
                _instance.Save()
            End If
        End If

        Return _instance
    End Function

    ''' <summary>
    ''' Returns the instance of this singleton object.
    ''' </summary>
    ''' <returns>The singleton object.</returns>
    ''' <remarks></remarks>
    Friend Shared Function GetInstance() As FavoritesBarData
        Return _instance
    End Function

    ''' <summary>
    ''' Checks if a filename is associated with this data object.
    ''' </summary>
    ''' <returns>Returns True if a filename is associated with this object. Otherwise, returns False.</returns>
    ''' <remarks></remarks>
    Friend Function Exists() As Boolean
        If (String.IsNullOrEmpty(Me.Filename)) Then Return False

        Return True
    End Function

    ''' <summary>
    ''' Saves data to a file.
    ''' </summary>
    ''' <remarks></remarks>
    ''' <exception cref="Exception">An error occured while trying to save the file.</exception>
    Friend Sub Save()
        Try
            If (String.Compare(Filename, Me.Filename, True) = 0) Then
                If Me.HasChanges() Then
                    Me.AcceptChanges()
                    Me.WriteXml(Filename)
                End If
            Else
                Me.AcceptChanges()
                Me.WriteXml(Filename)
            End If
        Catch ex As Exception
            Throw New Exception(String.Format(Localization.GetString("String_ErrorSavingFavoritesBarConfig"), Filename), ex)
        End Try
    End Sub

End Class



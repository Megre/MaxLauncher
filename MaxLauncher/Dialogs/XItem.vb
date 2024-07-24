'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Class XItem

    Public Property Name As String
    Public Property Value As Object
    Public Property Category As String

    Public Sub New()
    End Sub

    Public Sub New(ByVal name As String, ByVal value As Object)
        Me.Name = name
        Me.Value = value
    End Sub

    Public Sub New(ByVal name As String, ByVal value As Object, ByVal category As String)
        Me.New(name, value)
        Me.Category = category
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
